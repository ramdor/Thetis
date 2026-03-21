#include "vst_host_bridge.h"

#include "vst3_sdk.h"
#include "vst_runtime.h"
#include "vst2_runtime.h"
#include "../VstCommon/vst_ipc.h"

#include <cstdarg>
#include <cstdio>
#include <cstring>
#include <string>
#include <vector>
#include <process.h>

/*
Bridge responsibilities:
- own the persistent UI/persistence/restart model for RX/TX chains
- translate managed/native control calls into host IPC
- move realtime audio through the shared-memory transport without blocking DSP

The live host is authoritative for current execution state. The bridge cache is
authoritative for replay/persistence/UI reads. Refactors in this file need to
preserve that split.
*/

namespace
{
	volatile LONG g_initialized = 0;
	volatile LONG g_out_of_process_trace_enabled = -1;
	VstStateChangedCallback g_state_changed_callback = nullptr;
	const DWORD kControlPipeIoTimeoutMs = 5000;
	const DWORD kEditorOpenPipeIoTimeoutMs = 15000;
	const DWORD kStateDirtyCoalesceDelayMs = 150;
	const LONG kRxDefaultPipelineLatency = 8;
	const LONG kTxDefaultPipelineLatency = 2;
	const ULONGLONG kPipelineLatencyReduceStableBlocks = 20000;
	const ULONGLONG kPipelineLatencyFloorReduceStableBlocks = 60000;

	bool is_valid_kind(VstChainKind kind);
	VstPluginFormat detect_plugin_format_from_path(const wchar_t* plugin_path);

	struct HostProcessState
	{
		SRWLOCK cache_lock = SRWLOCK_INIT;
		SRWLOCK mutation_lock = SRWLOCK_INIT;
		HANDLE process_handle = 0;
		HANDLE thread_handle = 0;
		HANDLE control_pipe_handle = INVALID_HANDLE_VALUE;
		SRWLOCK control_lock = SRWLOCK_INIT;
		SRWLOCK audio_transport_lock = SRWLOCK_INIT;
		HANDLE audio_mapping_handle = 0;
		VstHostAudioSharedBlock* audio_shared = 0;
		HANDLE audio_request_event = 0;
		HANDLE state_dirty_event = 0;
		HANDLE state_dirty_stop_event = 0;
		HANDLE state_dirty_thread = 0;
		HANDLE ready_event = 0;
		HANDLE shutdown_event = 0;
		wchar_t ready_event_name[128] = {};
		wchar_t shutdown_event_name[128] = {};
		wchar_t control_pipe_name[128] = {};
		wchar_t audio_mapping_name[128] = {};
		wchar_t audio_request_event_name[128] = {};
		wchar_t state_dirty_event_name[128] = {};
		LONG audio_request_sequence = 0;
		LONG audio_consumed_sequence = 0;
		LONG audio_pending_sequence = 0;
		LONG audio_pending_frames = 0;
		LONG audio_pipeline_latency = VST_HOST_IPC_AUDIO_PIPELINE_LATENCY;
		LONG audio_min_pipeline_latency = VST_HOST_IPC_AUDIO_PIPELINE_LATENCY;
		LONG audio_last_output_frames = 0;
		LONG last_state_dirty_generation = 0;
		volatile LONG cache_generation = 0;
		LONG host_status = VST_HOST_STATE_DISABLED;
		LONG restart_attempted = 0;
		LONG restart_in_progress = 0;
		int cached_created = 0;
		int cached_sample_rate = 0;
		int cached_max_block_size = 0;
		int cached_num_channels = 0;
		int cached_ready = 0;
		int cached_bypass = 0;
		double cached_gain = 1.0;
		struct CachedPluginState
		{
			wchar_t path[VST_MAX_PLUGIN_PATH_CHARS] = {};
			wchar_t name[VST_MAX_PLUGIN_NAME_CHARS] = {};
			int enabled = 1;
			int bypass = 0;
			int load_state = VST_PLUGIN_LOAD_DESCRIPTOR_ONLY;
			int format = VST_PLUGIN_FORMAT_UNKNOWN;
			std::vector<unsigned char> state_data;
		};
		std::vector<CachedPluginState> cached_plugins;
		ULONGLONG audio_blocks_submitted = 0;
		ULONGLONG audio_blocks_consumed = 0;
		ULONGLONG audio_fallback_blocks = 0;
		ULONGLONG audio_concealed_blocks = 0;
		ULONGLONG audio_not_ready_blocks = 0;
		ULONGLONG audio_frame_mismatch_blocks = 0;
		ULONGLONG audio_signal_failures = 0;
		ULONGLONG audio_consecutive_ready_blocks = 0;
		DWORD audio_last_log_tick = 0;
		double audio_last_output[VST_HOST_IPC_AUDIO_MAX_SAMPLES] = {};
	};

	HostProcessState g_host_processes[VST_CHAIN_COUNT] = {};

	struct ScopedExclusiveLock
	{
		PSRWLOCK lock;

		explicit ScopedExclusiveLock(PSRWLOCK value)
			: lock(value)
		{
			AcquireSRWLockExclusive(lock);
		}

		~ScopedExclusiveLock()
		{
			ReleaseSRWLockExclusive(lock);
		}
	};

	LONG default_pipeline_latency_for_kind(VstChainKind kind)
	{
		return kind == VST_CHAIN_RX ? kRxDefaultPipelineLatency : kTxDefaultPipelineLatency;
	}

	LONG minimum_pipeline_latency_for_kind(VstChainKind kind)
	{
		return kind == VST_CHAIN_RX ? static_cast<LONG>(VST_HOST_IPC_AUDIO_PIPELINE_LATENCY) : kTxDefaultPipelineLatency;
	}

	void flush_bypassed_audio_transport(HostProcessState& state)
	{
		AcquireSRWLockExclusive(&state.audio_transport_lock);
		state.audio_consumed_sequence = state.audio_request_sequence;
		state.audio_pending_sequence = 0;
		state.audio_pending_frames = 0;
		state.audio_last_output_frames = 0;
		state.audio_consecutive_ready_blocks = 0;
		ReleaseSRWLockExclusive(&state.audio_transport_lock);
	}

	VstPluginFormat detect_plugin_format_from_path(const wchar_t* plugin_path)
	{
		const wchar_t* extension;

		if (!plugin_path || !plugin_path[0])
			return VST_PLUGIN_FORMAT_UNKNOWN;

		extension = wcsrchr(plugin_path, L'.');
		if (!extension)
			return VST_PLUGIN_FORMAT_UNKNOWN;
		if (_wcsicmp(extension, L".vst3") == 0)
			return VST_PLUGIN_FORMAT_VST3;
		if (_wcsicmp(extension, L".dll") == 0)
			return VST_PLUGIN_FORMAT_VST2;
		return VST_PLUGIN_FORMAT_UNKNOWN;
	}

	void set_host_status(HostProcessState& state, LONG host_status)
	{
		InterlockedExchange(&state.host_status, host_status);
	}

	void advance_cache_generation(HostProcessState& state)
	{
		InterlockedIncrement(&state.cache_generation);
	}

	bool host_set_plugin_state(VstChainKind kind, int index, const void* buffer, int buffer_size);
	void request_host_restart(VstChainKind kind);
	bool launch_host_process(VstChainKind kind);
	void shutdown_host_process(VstChainKind kind);
	unsigned __stdcall restart_host_thread_proc(void* context);
	unsigned __stdcall state_dirty_watch_thread_proc(void* context);

	const wchar_t* host_kind_name(VstChainKind kind)
	{
		switch (kind)
		{
		case VST_CHAIN_RX:
			return L"rx";
		case VST_CHAIN_TX:
			return L"tx";
		default:
			return L"unknown";
		}
	}

	bool out_of_process_trace_enabled()
	{
		LONG enabled = InterlockedCompareExchange(&g_out_of_process_trace_enabled, -1, -1);
		wchar_t value[8] = {};

		if (enabled != -1)
			return enabled != 0;

		enabled = 0;
		if (GetEnvironmentVariableW(L"THETIS_VST_OOP_TRACE", value, _countof(value)) > 0)
			enabled = 1;

		InterlockedExchange(&g_out_of_process_trace_enabled, enabled);
		return enabled != 0;
	}

	void trace_oop(const wchar_t* format, ...)
	{
		wchar_t message[512] = {};
		wchar_t log_path[MAX_PATH] = {};
		FILE* log_file = 0;
		va_list args;

		if (!out_of_process_trace_enabled())
			return;

		va_start(args, format);
		_vsnwprintf_s(message, _countof(message), _TRUNCATE, format, args);
		va_end(args);
		OutputDebugStringW(message);

		if (GetTempPathW(_countof(log_path), log_path) > 0)
		{
			wcscat_s(log_path, L"ThetisVstOop.log");
			if (_wfopen_s(&log_file, log_path, L"a+, ccs=UTF-8") == 0 && log_file)
			{
				fputws(message, log_file);
				fclose(log_file);
			}
		}
	}

	bool get_host_executable_path(std::wstring& executable_path)
	{
		wchar_t module_path[MAX_PATH * 2] = {};
		wchar_t* last_separator;

		executable_path.clear();
		if (!GetModuleFileNameW(0, module_path, _countof(module_path)))
			return false;

		last_separator = wcsrchr(module_path, L'\\');
		if (!last_separator)
			return false;

		*(last_separator + 1) = L'\0';
		executable_path.assign(module_path);
		executable_path += L"VstAudioHost.exe";
		return GetFileAttributesW(executable_path.c_str()) != INVALID_FILE_ATTRIBUTES;
	}

	void clear_cached_chain_snapshot(HostProcessState& state)
	{
		AcquireSRWLockExclusive(&state.cache_lock);
		state.cached_created = 0;
		state.cached_sample_rate = 0;
		state.cached_max_block_size = 0;
		state.cached_num_channels = 0;
		state.cached_ready = 0;
		state.cached_bypass = 0;
		state.cached_gain = 1.0;
		state.cached_plugins.clear();
		advance_cache_generation(state);
		ReleaseSRWLockExclusive(&state.cache_lock);
	}

	void initialize_cached_chain_snapshot(HostProcessState& state, int sample_rate, int max_block_size, int num_channels)
	{
		AcquireSRWLockExclusive(&state.cache_lock);
		state.cached_created = 1;
		state.cached_sample_rate = sample_rate;
		state.cached_max_block_size = max_block_size;
		state.cached_num_channels = num_channels;
		state.cached_ready = 0;
		advance_cache_generation(state);
		ReleaseSRWLockExclusive(&state.cache_lock);
	}

	void initialize_cached_plugin_state(HostProcessState::CachedPluginState& plugin, const wchar_t* plugin_path)
	{
		plugin.path[0] = L'\0';
		plugin.name[0] = L'\0';
		plugin.enabled = 1;
		plugin.bypass = 0;
		plugin.load_state = VST_PLUGIN_LOAD_DESCRIPTOR_ONLY;
		plugin.format = VST_PLUGIN_FORMAT_UNKNOWN;
		plugin.state_data.clear();
		if (plugin_path)
		{
			wcsncpy_s(plugin.path, VST_MAX_PLUGIN_PATH_CHARS, plugin_path, _TRUNCATE);
			plugin.format = detect_plugin_format_from_path(plugin.path);
			wchar_t name_buffer[VST_MAX_PLUGIN_NAME_CHARS] = {};
			const wchar_t* last_separator = wcsrchr(plugin_path, L'\\');
			const wchar_t* file_name = last_separator ? last_separator + 1 : plugin_path;
			wcsncpy_s(name_buffer, VST_MAX_PLUGIN_NAME_CHARS, file_name, _TRUNCATE);
			wchar_t* extension = wcsrchr(name_buffer, L'.');
			if (extension && (_wcsicmp(extension, L".vst3") == 0 || _wcsicmp(extension, L".dll") == 0))
				*extension = L'\0';
			wcsncpy_s(plugin.name, VST_MAX_PLUGIN_NAME_CHARS, name_buffer, _TRUNCATE);
		}
	}

	int cached_get_plugin_info(const HostProcessState& state, int index, VstPluginInfo* info)
	{
		const HostProcessState::CachedPluginState* plugin;
		int result = 0;

		if (!info)
			return -1;
		AcquireSRWLockShared(const_cast<PSRWLOCK>(&state.cache_lock));
		if (index < 0 || index >= static_cast<int>(state.cached_plugins.size()))
		{
			ReleaseSRWLockShared(const_cast<PSRWLOCK>(&state.cache_lock));
			return -1;
		}

		plugin = &state.cached_plugins[index];
		memset(info, 0, sizeof(*info));
		info->index = index;
		info->enabled = plugin->enabled;
		info->bypass = plugin->bypass;
		info->load_state = plugin->load_state;
		info->format = plugin->format;
		wcsncpy_s(info->path, VST_MAX_PLUGIN_PATH_CHARS, plugin->path, _TRUNCATE);
		wcsncpy_s(info->name, VST_MAX_PLUGIN_NAME_CHARS, plugin->name, _TRUNCATE);
		ReleaseSRWLockShared(const_cast<PSRWLOCK>(&state.cache_lock));
		return result;
	}

	int cached_get_plugin_state(const HostProcessState& state, int index, void* buffer, int buffer_size, int* bytes_written)
	{
		const HostProcessState::CachedPluginState* plugin;
		int state_size;

		if (bytes_written)
			*bytes_written = 0;
		AcquireSRWLockShared(const_cast<PSRWLOCK>(&state.cache_lock));
		if (index < 0 || index >= static_cast<int>(state.cached_plugins.size()))
		{
			ReleaseSRWLockShared(const_cast<PSRWLOCK>(&state.cache_lock));
			return -1;
		}

		plugin = &state.cached_plugins[index];
		state_size = static_cast<int>(plugin->state_data.size());
		if (state_size <= 0)
		{
			ReleaseSRWLockShared(const_cast<PSRWLOCK>(&state.cache_lock));
			return 0;
		}
		if (!buffer || buffer_size < state_size)
		{
			ReleaseSRWLockShared(const_cast<PSRWLOCK>(&state.cache_lock));
			return -1;
		}

		memcpy(buffer, plugin->state_data.data(), state_size);
		if (bytes_written)
			*bytes_written = state_size;
		ReleaseSRWLockShared(const_cast<PSRWLOCK>(&state.cache_lock));
		return 0;
	}

	void close_host_process_handles(VstChainKind kind, HostProcessState& state)
	{
		if (state.state_dirty_stop_event)
			SetEvent(state.state_dirty_stop_event);
		if (state.state_dirty_thread)
		{
			WaitForSingleObject(state.state_dirty_thread, 2000);
			CloseHandle(state.state_dirty_thread);
			state.state_dirty_thread = 0;
		}
		if (state.state_dirty_stop_event)
		{
			CloseHandle(state.state_dirty_stop_event);
			state.state_dirty_stop_event = 0;
		}

		AcquireSRWLockExclusive(&state.control_lock);
		if (state.control_pipe_handle != INVALID_HANDLE_VALUE)
		{
			CloseHandle(state.control_pipe_handle);
			state.control_pipe_handle = INVALID_HANDLE_VALUE;
		}
		ReleaseSRWLockExclusive(&state.control_lock);

		AcquireSRWLockExclusive(&state.audio_transport_lock);
		if (state.audio_shared)
		{
			UnmapViewOfFile(state.audio_shared);
			state.audio_shared = 0;
		}
		if (state.audio_mapping_handle)
		{
			CloseHandle(state.audio_mapping_handle);
			state.audio_mapping_handle = 0;
		}
		if (state.audio_request_event)
		{
			CloseHandle(state.audio_request_event);
			state.audio_request_event = 0;
		}
		if (state.state_dirty_event)
		{
			CloseHandle(state.state_dirty_event);
			state.state_dirty_event = 0;
		}
		if (state.thread_handle)
		{
			CloseHandle(state.thread_handle);
			state.thread_handle = 0;
		}
		if (state.process_handle)
		{
			CloseHandle(state.process_handle);
			state.process_handle = 0;
		}
		if (state.ready_event)
		{
			CloseHandle(state.ready_event);
			state.ready_event = 0;
		}
		if (state.shutdown_event)
		{
			CloseHandle(state.shutdown_event);
			state.shutdown_event = 0;
		}

		state.ready_event_name[0] = L'\0';
		state.shutdown_event_name[0] = L'\0';
		state.control_pipe_name[0] = L'\0';
		state.audio_mapping_name[0] = L'\0';
		state.audio_request_event_name[0] = L'\0';
		state.state_dirty_event_name[0] = L'\0';
		state.audio_request_sequence = 0;
		state.audio_consumed_sequence = 0;
		state.audio_pending_sequence = 0;
		state.audio_pending_frames = 0;
		state.audio_pipeline_latency = default_pipeline_latency_for_kind(kind);
		state.audio_min_pipeline_latency = default_pipeline_latency_for_kind(kind);
		state.audio_last_output_frames = 0;
		state.last_state_dirty_generation = 0;
		state.audio_blocks_submitted = 0;
		state.audio_blocks_consumed = 0;
		state.audio_fallback_blocks = 0;
		state.audio_concealed_blocks = 0;
		state.audio_not_ready_blocks = 0;
		state.audio_frame_mismatch_blocks = 0;
		state.audio_signal_failures = 0;
		state.audio_consecutive_ready_blocks = 0;
		state.audio_last_log_tick = 0;
		memset(state.audio_last_output, 0, sizeof(state.audio_last_output));
		ReleaseSRWLockExclusive(&state.audio_transport_lock);
	}

	bool write_control_packet(HANDLE pipe_handle, const VstHostIpcPacket& packet, DWORD timeout_ms = kControlPipeIoTimeoutMs)
	{
		OVERLAPPED overlapped = {};
		DWORD bytes_written = 0;
		BOOL io_result;
		DWORD last_error;

		if (pipe_handle == INVALID_HANDLE_VALUE)
			return false;

		overlapped.hEvent = CreateEventW(nullptr, TRUE, FALSE, nullptr);
		if (!overlapped.hEvent)
			return false;

		io_result = WriteFile(pipe_handle, &packet, sizeof(packet), nullptr, &overlapped);
		if (!io_result)
		{
			last_error = GetLastError();
			if (last_error != ERROR_IO_PENDING)
			{
				CloseHandle(overlapped.hEvent);
				return false;
			}

			if (WaitForSingleObject(overlapped.hEvent, timeout_ms) != WAIT_OBJECT_0)
			{
				CancelIoEx(pipe_handle, &overlapped);
				WaitForSingleObject(overlapped.hEvent, INFINITE);
				CloseHandle(overlapped.hEvent);
				SetLastError(ERROR_SEM_TIMEOUT);
				return false;
			}
		}

		io_result = GetOverlappedResult(pipe_handle, &overlapped, &bytes_written, TRUE);
		CloseHandle(overlapped.hEvent);
		return io_result != FALSE && bytes_written == sizeof(packet);
	}

	bool read_control_packet(HANDLE pipe_handle, VstHostIpcPacket& packet, DWORD timeout_ms = kControlPipeIoTimeoutMs)
	{
		OVERLAPPED overlapped = {};
		DWORD bytes_read = 0;
		BOOL io_result;
		DWORD last_error;

		if (pipe_handle == INVALID_HANDLE_VALUE)
			return false;

		memset(&packet, 0, sizeof(packet));
		overlapped.hEvent = CreateEventW(nullptr, TRUE, FALSE, nullptr);
		if (!overlapped.hEvent)
			return false;

		io_result = ReadFile(pipe_handle, &packet, sizeof(packet), nullptr, &overlapped);
		if (!io_result)
		{
			last_error = GetLastError();
			if (last_error != ERROR_IO_PENDING)
			{
				CloseHandle(overlapped.hEvent);
				return false;
			}

			if (WaitForSingleObject(overlapped.hEvent, timeout_ms) != WAIT_OBJECT_0)
			{
				CancelIoEx(pipe_handle, &overlapped);
				WaitForSingleObject(overlapped.hEvent, INFINITE);
				CloseHandle(overlapped.hEvent);
				SetLastError(ERROR_SEM_TIMEOUT);
				return false;
			}
		}

		io_result = GetOverlappedResult(pipe_handle, &overlapped, &bytes_read, TRUE);
		CloseHandle(overlapped.hEvent);
		if (!io_result || bytes_read != sizeof(packet))
			return false;

		return packet.header.magic == VST_HOST_IPC_MAGIC &&
			packet.header.version == VST_HOST_IPC_VERSION &&
			packet.header.size == sizeof(packet);
	}

	bool write_control_message(HANDLE pipe_handle, uint32_t type, uint32_t chain_kind)
	{
		VstHostIpcPacket packet = {};

		packet.header.magic = VST_HOST_IPC_MAGIC;
		packet.header.version = VST_HOST_IPC_VERSION;
		packet.header.type = type;
		packet.header.size = sizeof(packet);
		packet.header.chain_kind = chain_kind;
		return write_control_packet(pipe_handle, packet);
	}

	bool connect_to_host_pipe(HostProcessState& state)
	{
		DWORD read_mode = PIPE_READMODE_MESSAGE;

		if (!WaitNamedPipeW(state.control_pipe_name, 2000))
			return false;

		state.control_pipe_handle = CreateFileW(
			state.control_pipe_name,
			GENERIC_READ | GENERIC_WRITE,
			0,
			0,
			OPEN_EXISTING,
			FILE_FLAG_OVERLAPPED,
			0);
		if (state.control_pipe_handle == INVALID_HANDLE_VALUE)
			return false;

		if (!SetNamedPipeHandleState(state.control_pipe_handle, &read_mode, 0, 0))
		{
			CloseHandle(state.control_pipe_handle);
			state.control_pipe_handle = INVALID_HANDLE_VALUE;
			return false;
		}

		return true;
	}

	LONG query_host_status(VstChainKind kind)
	{
		HostProcessState& state = g_host_processes[kind];
		HANDLE local_process_handle;
		HANDLE local_pipe_handle;
		DWORD wait_result;

		if (InterlockedCompareExchange(&state.restart_in_progress, 0, 0) != 0)
		{
			set_host_status(state, VST_HOST_STATE_RESTARTING);
			return VST_HOST_STATE_RESTARTING;
		}

		// Snapshot handles under lock so the restart thread cannot close
		// them between our read and the WaitForSingleObject call.
		AcquireSRWLockShared(&state.audio_transport_lock);
		local_process_handle = state.process_handle;
		local_pipe_handle = state.control_pipe_handle;
		ReleaseSRWLockShared(&state.audio_transport_lock);

		if (!local_process_handle)
		{
			if (InterlockedCompareExchange(&state.host_status, 0, 0) == VST_HOST_STATE_STARTING)
				return VST_HOST_STATE_STARTING;

			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			return VST_HOST_STATE_UNAVAILABLE;
		}

		wait_result = WaitForSingleObject(local_process_handle, 0);
		if (wait_result == WAIT_OBJECT_0)
		{
			set_host_status(state, VST_HOST_STATE_CRASHED);
			return VST_HOST_STATE_CRASHED;
		}

		if (local_pipe_handle == INVALID_HANDLE_VALUE)
		{
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			return VST_HOST_STATE_UNAVAILABLE;
		}

		set_host_status(state, VST_HOST_STATE_RUNNING);
		return VST_HOST_STATE_RUNNING;
	}

	void ensure_host_recovery(VstChainKind kind)
	{
		LONG status;

		if (!is_valid_kind(kind))
			return;

		status = query_host_status(kind);
		if ((status == VST_HOST_STATE_CRASHED || status == VST_HOST_STATE_UNAVAILABLE) &&
			InterlockedCompareExchange(&g_host_processes[kind].restart_attempted, 0, 0) == 0)
		{
			request_host_restart(kind);
		}
	}

	bool validate_host_handshake(HostProcessState& state, VstChainKind kind)
	{
		VstHostIpcPacket packet = {};

		if (!read_control_packet(state.control_pipe_handle, packet))
			return false;
		if (packet.header.type != VST_HOST_IPC_HELLO || packet.header.chain_kind != static_cast<uint32_t>(kind))
			return false;
		if (!write_control_message(state.control_pipe_handle, VST_HOST_IPC_PING, static_cast<uint32_t>(kind)))
			return false;
		if (!read_control_packet(state.control_pipe_handle, packet))
			return false;
		return packet.header.type == VST_HOST_IPC_PONG && packet.header.chain_kind == static_cast<uint32_t>(kind);
	}

	bool initialize_audio_transport(VstChainKind kind, HostProcessState& state)
	{
		DWORD current_process_id = GetCurrentProcessId();
		HANDLE mapping_handle = 0;
		VstHostAudioSharedBlock* audio_shared = 0;
		HANDLE request_event = 0;
		HANDLE state_dirty_event = 0;

		swprintf_s(state.audio_mapping_name, L"Local\\ThetisVstHostAudio_%lu_%s", current_process_id, host_kind_name(kind));
		swprintf_s(state.audio_request_event_name, L"Local\\ThetisVstHostAudioRequest_%lu_%s", current_process_id, host_kind_name(kind));
		swprintf_s(state.state_dirty_event_name, L"Local\\ThetisVstHostStateDirty_%lu_%s", current_process_id, host_kind_name(kind));

		mapping_handle = CreateFileMappingW(
			INVALID_HANDLE_VALUE,
			0,
			PAGE_READWRITE,
			0,
			sizeof(VstHostAudioSharedBlock),
			state.audio_mapping_name);
		if (!mapping_handle)
			return false;

		audio_shared = static_cast<VstHostAudioSharedBlock*>(MapViewOfFile(
			mapping_handle,
			FILE_MAP_ALL_ACCESS,
			0,
			0,
			sizeof(VstHostAudioSharedBlock)));
		if (!audio_shared)
		{
			CloseHandle(mapping_handle);
			return false;
		}

		memset(audio_shared, 0, sizeof(VstHostAudioSharedBlock));
		request_event = CreateEventW(0, FALSE, FALSE, state.audio_request_event_name);
		if (!request_event)
		{
			UnmapViewOfFile(audio_shared);
			CloseHandle(mapping_handle);
			return false;
		}

		state_dirty_event = CreateEventW(0, FALSE, FALSE, state.state_dirty_event_name);
		if (!state_dirty_event)
		{
			CloseHandle(request_event);
			UnmapViewOfFile(audio_shared);
			CloseHandle(mapping_handle);
			return false;
		}

		AcquireSRWLockExclusive(&state.audio_transport_lock);
		state.audio_mapping_handle = mapping_handle;
		state.audio_shared = audio_shared;
		state.audio_request_event = request_event;
		state.state_dirty_event = state_dirty_event;
		state.audio_pipeline_latency = default_pipeline_latency_for_kind(kind);
		state.audio_min_pipeline_latency = default_pipeline_latency_for_kind(kind);
		state.audio_last_output_frames = 0;
		state.audio_consecutive_ready_blocks = 0;
		state.audio_concealed_blocks = 0;
		memset(state.audio_last_output, 0, sizeof(state.audio_last_output));
		state.last_state_dirty_generation = 0;
		ReleaseSRWLockExclusive(&state.audio_transport_lock);
		return true;
	}

	bool send_host_request(VstChainKind kind, VstHostIpcPacket& request, VstHostIpcPacket& response, DWORD timeout_ms = kControlPipeIoTimeoutMs)
	{
		HostProcessState& state = g_host_processes[kind];
		bool request_failed = false;

		if (state.control_pipe_handle == INVALID_HANDLE_VALUE)
		{
			ensure_host_recovery(kind);
			return false;
		}

		request.header.magic = VST_HOST_IPC_MAGIC;
		request.header.version = VST_HOST_IPC_VERSION;
		request.header.size = sizeof(request);
		request.header.chain_kind = static_cast<uint32_t>(kind);

		AcquireSRWLockExclusive(&state.control_lock);
		if (!write_control_packet(state.control_pipe_handle, request, timeout_ms) ||
			!read_control_packet(state.control_pipe_handle, response, timeout_ms))
		{
			request_failed = true;
		}
		else if (response.result == VST_HOST_IPC_RESULT_CONTROL_TIMEOUT ||
			response.result == VST_HOST_IPC_RESULT_RUNTIME_THREAD_EXITED)
		{
			request_failed = true;
		}
		if (request_failed && state.control_pipe_handle != INVALID_HANDLE_VALUE)
		{
			CloseHandle(state.control_pipe_handle);
			state.control_pipe_handle = INVALID_HANDLE_VALUE;
		}
		ReleaseSRWLockExclusive(&state.control_lock);
		if (request_failed)
		{
			ensure_host_recovery(kind);
			return false;
		}

		return response.header.type == VST_HOST_IPC_RESPONSE ||
			response.header.type == VST_HOST_IPC_PONG;
	}

	bool host_command_available(VstChainKind kind)
	{
		return is_valid_kind(kind) &&
			query_host_status(kind) == VST_HOST_STATE_RUNNING &&
			g_host_processes[kind].control_pipe_handle != INVALID_HANDLE_VALUE;
	}

	bool host_control_connected(VstChainKind kind)
	{
		return is_valid_kind(kind) &&
			g_host_processes[kind].process_handle != 0 &&
			g_host_processes[kind].control_pipe_handle != INVALID_HANDLE_VALUE;
	}

	bool host_audio_available(VstChainKind kind)
	{
		return is_valid_kind(kind) &&
			query_host_status(kind) == VST_HOST_STATE_RUNNING &&
			g_host_processes[kind].audio_shared &&
			g_host_processes[kind].audio_request_event;
	}

	int host_command_simple(VstChainKind kind, uint32_t type, int arg0, int arg1, int arg2, double double_value)
	{
		VstHostIpcPacket request = {};
		VstHostIpcPacket response = {};

		request.header.type = type;
		request.arg0 = arg0;
		request.arg1 = arg1;
		request.arg2 = arg2;
		request.double_value = double_value;
		if (!send_host_request(kind, request, response))
			return -300;
		return response.result;
	}

	bool host_get_chain_snapshot_info(
		VstChainKind kind,
		int& created,
		int& sample_rate,
		int& max_block_size,
		int& num_channels,
		int& ready,
		int& bypass,
		double& gain,
		int& plugin_count,
		LONG& generation)
	{
		VstHostIpcPacket request = {};
		VstHostIpcPacket response = {};

		request.header.type = VST_HOST_IPC_GET_CHAIN_SNAPSHOT_INFO;
		if (!send_host_request(kind, request, response) || response.result != 0)
			return false;

		created = response.chain_snapshot_info.created;
		sample_rate = response.chain_snapshot_info.sample_rate;
		max_block_size = response.chain_snapshot_info.max_block_size;
		num_channels = response.chain_snapshot_info.num_channels;
		ready = response.chain_snapshot_info.ready;
		bypass = response.chain_snapshot_info.bypass;
		plugin_count = response.chain_snapshot_info.plugin_count;
		generation = static_cast<LONG>(response.chain_snapshot_info.generation);
		gain = response.chain_snapshot_info.gain;
		return true;
	}

	int host_get_plugin_count(VstChainKind kind)
	{
		VstHostIpcPacket request = {};
		VstHostIpcPacket response = {};

		request.header.type = VST_HOST_IPC_GET_PLUGIN_COUNT;
		if (!send_host_request(kind, request, response))
			return -300;

		return response.result;
	}

	int host_get_plugin_info(VstChainKind kind, int index, VstPluginInfo& info, int expected_generation = -1)
	{
		VstHostIpcPacket request = {};
		VstHostIpcPacket response = {};

		request.header.type = VST_HOST_IPC_GET_PLUGIN_INFO;
		request.arg0 = index;
		request.arg1 = expected_generation;
		if (!send_host_request(kind, request, response))
			return -300;
		if (response.result != 0)
			return response.result;

		info = response.plugin_info;
		return 0;
	}

	int host_get_plugin_state_size(VstChainKind kind, int index, int expected_generation = -1)
	{
		VstHostIpcPacket request = {};
		VstHostIpcPacket response = {};

		request.header.type = VST_HOST_IPC_GET_PLUGIN_STATE_SIZE;
		request.arg0 = index;
		request.arg1 = expected_generation;
		if (!send_host_request(kind, request, response))
			return -300;

		return response.result;
	}

	int host_get_plugin_state(VstChainKind kind, int index, void* buffer, int buffer_size, int* bytes_written, int expected_generation = -1)
	{
		VstHostIpcPacket request = {};
		VstHostIpcPacket response = {};

		if (bytes_written)
			*bytes_written = 0;
		if (!buffer || buffer_size <= 0 || buffer_size > static_cast<int>(VST_HOST_IPC_MAX_STATE_BYTES))
			return -301;

		request.header.type = VST_HOST_IPC_GET_PLUGIN_STATE;
		request.arg0 = index;
		request.arg1 = buffer_size;
		request.arg2 = expected_generation;
		if (!send_host_request(kind, request, response))
			return -300;
		if (response.result != 0)
			return response.result;

		if (response.arg1 > 0 && response.arg1 <= buffer_size)
		{
			memcpy(buffer, response.state_data, response.arg1);
			if (bytes_written)
				*bytes_written = response.arg1;
		}

		return 0;
	}

	bool update_cached_chain_snapshot(VstChainKind kind, bool prefer_host_state)
	{
		HostProcessState& host_state = g_host_processes[kind];
		bool use_host_state = host_command_available(kind);
		int cached_created = 0;
		int cached_sample_rate = 0;
		int cached_max_block_size = 0;
		int cached_num_channels = 0;
		int cached_ready = 0;
		int cached_bypass = 0;
		double cached_gain = 1.0;
		std::vector<HostProcessState::CachedPluginState> cached_plugins;
		int plugin_count = 0;
		LONG host_generation = 0;
		/*
		Snapshot refresh runs under the same mutation lock used by structural cache
		updates. That keeps replay, refresh, and local cache edits serialized so a
		new cache image always represents one coherent host generation.
		*/
		ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

		if (use_host_state)
		{
			for (int attempt = 0; attempt < 4; ++attempt)
			{
				bool retry_snapshot = false;
				int host_ready = 0;
				int host_bypass = 0;
				int host_created = 0;
				int host_sample_rate = 0;
				int host_max_block_size = 0;
				int host_num_channels = 0;
				double host_gain = 1.0;

				cached_plugins.clear();
				if (!host_get_chain_snapshot_info(
					kind,
					host_created,
					host_sample_rate,
					host_max_block_size,
					host_num_channels,
					host_ready,
					host_bypass,
					host_gain,
					plugin_count,
					host_generation))
				{
					return false;
				}
				if (plugin_count < 0)
					return false;

				cached_created = host_created;
				cached_sample_rate = host_sample_rate;
				cached_max_block_size = host_max_block_size;
				cached_num_channels = host_num_channels;
				cached_ready = host_ready;
				cached_bypass = host_bypass;
				cached_gain = host_gain;
				cached_plugins.reserve(plugin_count);
				for (int i = 0; i < plugin_count; ++i)
				{
					VstPluginInfo info = {};
					HostProcessState::CachedPluginState cached_plugin;
					int state_size = 0;
					int bytes_written = 0;
					int info_result = host_get_plugin_info(kind, i, info, static_cast<int>(host_generation));

					if (info_result == VST_HOST_IPC_RESULT_SNAPSHOT_CHANGED)
					{
						retry_snapshot = true;
						break;
					}
					if (info_result != 0)
						return false;

					initialize_cached_plugin_state(cached_plugin, info.path);
					wcsncpy_s(cached_plugin.name, VST_MAX_PLUGIN_NAME_CHARS, info.name, _TRUNCATE);
					cached_plugin.enabled = info.enabled;
					cached_plugin.bypass = info.bypass;
					cached_plugin.load_state = info.load_state;
					cached_plugin.format = info.format;

					state_size = host_get_plugin_state_size(kind, i, static_cast<int>(host_generation));
					if (state_size == VST_HOST_IPC_RESULT_SNAPSHOT_CHANGED)
					{
						retry_snapshot = true;
						break;
					}
					if (state_size < 0)
						return false;
					if (state_size > 0 && state_size <= static_cast<int>(VST_HOST_IPC_MAX_STATE_BYTES))
					{
						cached_plugin.state_data.resize(state_size);
						state_size = host_get_plugin_state(kind, i, cached_plugin.state_data.data(), state_size, &bytes_written, static_cast<int>(host_generation));
						if (state_size == VST_HOST_IPC_RESULT_SNAPSHOT_CHANGED)
						{
							retry_snapshot = true;
							break;
						}
						if (state_size != 0 || bytes_written <= 0)
							bytes_written = 0;
						if (bytes_written > 0 && bytes_written < static_cast<int>(cached_plugin.state_data.size()))
							cached_plugin.state_data.resize(bytes_written);
						else if (bytes_written <= 0)
							cached_plugin.state_data.clear();
					}

					cached_plugins.push_back(cached_plugin);
				}

				if (retry_snapshot)
					continue;

				AcquireSRWLockExclusive(&host_state.cache_lock);
				host_state.cached_created = cached_created;
				host_state.cached_sample_rate = cached_sample_rate;
				host_state.cached_max_block_size = cached_max_block_size;
				host_state.cached_num_channels = cached_num_channels;
				host_state.cached_ready = cached_ready;
				host_state.cached_bypass = cached_bypass;
				host_state.cached_gain = cached_gain;
				host_state.cached_plugins.swap(cached_plugins);
				advance_cache_generation(host_state);
				ReleaseSRWLockExclusive(&host_state.cache_lock);
				return true;
			}

			return false;
		}

		UNREFERENCED_PARAMETER(prefer_host_state);
		return false;
	}

	bool apply_cached_chain_snapshot(VstChainKind kind)
	{
		HostProcessState& host_state = g_host_processes[kind];
		int cached_created;
		int cached_sample_rate;
		int cached_max_block_size;
		int cached_num_channels;
		int cached_ready;
		int cached_bypass;
		double cached_gain;
		std::vector<HostProcessState::CachedPluginState> cached_plugins;
		/*
		Replay is driven from the bridge cache, not from ad hoc host reads. The
		cache is the restart/persistence source of truth, so replay must happen
		under the mutation lock to avoid racing concurrent cache edits or refreshes.
		*/
		ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

		if (!host_control_connected(kind))
			return false;

		AcquireSRWLockShared(&host_state.cache_lock);
		cached_created = host_state.cached_created;
		cached_sample_rate = host_state.cached_sample_rate;
		cached_max_block_size = host_state.cached_max_block_size;
		cached_num_channels = host_state.cached_num_channels;
		cached_ready = host_state.cached_ready;
		cached_bypass = host_state.cached_bypass;
		cached_gain = host_state.cached_gain;
		cached_plugins = host_state.cached_plugins;
		ReleaseSRWLockShared(&host_state.cache_lock);

		if (!cached_created)
		{
			return host_command_simple(kind, VST_HOST_IPC_DESTROY_CHAIN, 0, 0, 0, 0.0) == 0;
		}

		if (host_command_simple(
			kind,
			VST_HOST_IPC_CREATE_CHAIN,
			cached_sample_rate,
			cached_max_block_size,
			cached_num_channels,
			0.0) != 0)
		{
			return false;
		}

		if (host_command_simple(kind, VST_HOST_IPC_SET_CHAIN_BYPASS, cached_bypass, 0, 0, 0.0) != 0)
			return false;
		if (host_command_simple(kind, VST_HOST_IPC_SET_CHAIN_GAIN, 0, 0, 0, cached_gain) != 0)
			return false;
		if (host_command_simple(kind, VST_HOST_IPC_CLEAR_CHAIN, 0, 0, 0, 0.0) != 0)
			return false;

		for (int i = 0; i < static_cast<int>(cached_plugins.size()); ++i)
		{
			VstHostIpcPacket request = {};
			VstHostIpcPacket response = {};
			HostProcessState::CachedPluginState& cached_plugin = cached_plugins[i];
			int host_index;

			request.header.type = VST_HOST_IPC_ADD_PLUGIN;
			wcsncpy_s(request.plugin_path, VST_MAX_PLUGIN_PATH_CHARS, cached_plugin.path, _TRUNCATE);
			if (!send_host_request(kind, request, response) || response.result < 0)
				return false;
			host_index = response.result;

			{
				VstPluginInfo host_info = {};
				if (host_get_plugin_info(kind, host_index, host_info) == 0)
				{
					wcsncpy_s(cached_plugin.name, VST_MAX_PLUGIN_NAME_CHARS, host_info.name, _TRUNCATE);
					cached_plugin.load_state = host_info.load_state;
					if (host_info.format != VST_PLUGIN_FORMAT_UNKNOWN)
						cached_plugin.format = host_info.format;
					if (host_info.path[0] != L'\0')
						wcsncpy_s(cached_plugin.path, VST_MAX_PLUGIN_PATH_CHARS, host_info.path, _TRUNCATE);
				}
			}
			if (!cached_plugin.state_data.empty())
			{
				if (!host_set_plugin_state(kind, host_index, cached_plugin.state_data.data(), static_cast<int>(cached_plugin.state_data.size())))
					return false;
			}
			if (host_command_simple(kind, VST_HOST_IPC_SET_PLUGIN_ENABLED, host_index, cached_plugin.enabled, 0, 0.0) != 0)
				return false;
			if (host_command_simple(kind, VST_HOST_IPC_SET_PLUGIN_BYPASS, host_index, cached_plugin.bypass, 0, 0.0) != 0)
				return false;
		}

		{
			int host_created = cached_created;
			int host_sample_rate = cached_sample_rate;
			int host_max_block_size = cached_max_block_size;
			int host_num_channels = cached_num_channels;
			int host_ready = 0;
			int host_bypass = 0;
			double host_gain = cached_gain;
			int host_plugin_count = 0;
			LONG host_generation = 0;
			if (host_get_chain_snapshot_info(
				kind,
				host_created,
				host_sample_rate,
				host_max_block_size,
				host_num_channels,
				host_ready,
				host_bypass,
				host_gain,
				host_plugin_count,
				host_generation))
			{
				cached_created = host_created;
				cached_sample_rate = host_sample_rate;
				cached_max_block_size = host_max_block_size;
				cached_num_channels = host_num_channels;
				cached_ready = host_ready;
				cached_bypass = host_bypass;
				cached_gain = host_gain;
			}
		}

		AcquireSRWLockExclusive(&host_state.cache_lock);
		host_state.cached_created = cached_created;
		host_state.cached_sample_rate = cached_sample_rate;
		host_state.cached_max_block_size = cached_max_block_size;
		host_state.cached_num_channels = cached_num_channels;
		host_state.cached_bypass = cached_bypass;
		host_state.cached_gain = cached_gain;
		host_state.cached_plugins.swap(cached_plugins);
		host_state.cached_ready = cached_ready;
		advance_cache_generation(host_state);
		ReleaseSRWLockExclusive(&host_state.cache_lock);
		return true;
	}

	void request_host_restart(VstChainKind kind)
	{
		uintptr_t thread_handle = 0;

		if (!is_valid_kind(kind))
			return;
		if (InterlockedCompareExchange(&g_host_processes[kind].restart_attempted, 1, 0) != 0)
			return;
		if (InterlockedCompareExchange(&g_host_processes[kind].restart_in_progress, 1, 0) != 0)
			return;

		set_host_status(g_host_processes[kind], VST_HOST_STATE_RESTARTING);
		trace_oop(L"[VST OOP bridge %s] scheduling automatic host restart\r\n", host_kind_name(kind));
		thread_handle = _beginthreadex(nullptr, 0, restart_host_thread_proc, reinterpret_cast<void*>(static_cast<uintptr_t>(kind)), 0, nullptr);
		if (thread_handle)
		{
			CloseHandle(reinterpret_cast<HANDLE>(thread_handle));
			return;
		}

		InterlockedExchange(&g_host_processes[kind].restart_in_progress, 0);
		set_host_status(g_host_processes[kind], VST_HOST_STATE_CRASHED);
	}

	bool host_set_plugin_state(VstChainKind kind, int index, const void* buffer, int buffer_size)
	{
		VstHostIpcPacket request = {};
		VstHostIpcPacket response = {};

		if (!buffer || buffer_size <= 0 || buffer_size > static_cast<int>(VST_HOST_IPC_MAX_STATE_BYTES))
			return false;

		request.header.type = VST_HOST_IPC_SET_PLUGIN_STATE;
		request.arg0 = index;
		request.arg1 = buffer_size;
		memcpy(request.state_data, buffer, buffer_size);
		if (!send_host_request(kind, request, response))
			return false;

		return response.result == 0;
	}

	bool host_open_plugin_editor(VstChainKind kind, int index)
	{
		VstHostIpcPacket request = {};
		VstHostIpcPacket response = {};

		request.header.type = VST_HOST_IPC_OPEN_PLUGIN_EDITOR;
		request.arg0 = index;
		trace_oop(L"[VST OOP bridge %s] request open editor index=%d\r\n", host_kind_name(kind), index);
		if (!send_host_request(kind, request, response, kEditorOpenPipeIoTimeoutMs))
		{
			trace_oop(L"[VST OOP bridge %s] open editor index=%d transport failure\r\n", host_kind_name(kind), index);
			return false;
		}

		trace_oop(
			L"[VST OOP bridge %s] open editor index=%d response=%d\r\n",
			host_kind_name(kind),
			index,
			response.result);

		return response.result == 0;
	}

	bool request_host_chain_sync(VstChainKind kind)
	{
		if (!host_command_available(kind))
			ensure_host_recovery(kind);
		if (!host_command_available(kind))
			return false;
		return apply_cached_chain_snapshot(kind);
	}

unsigned __stdcall restart_host_thread_proc(void* context)
{
	VstChainKind kind = static_cast<VstChainKind>(reinterpret_cast<uintptr_t>(context));
		HostProcessState& state = g_host_processes[kind];

		if (!is_valid_kind(kind))
			return 1;

		trace_oop(L"[VST OOP bridge %s] restart thread begin\r\n", host_kind_name(kind));
		shutdown_host_process(kind);
		set_host_status(state, VST_HOST_STATE_RESTARTING);

		if (launch_host_process(kind) && apply_cached_chain_snapshot(kind))
		{
			trace_oop(L"[VST OOP bridge %s] restart thread succeeded\r\n", host_kind_name(kind));
			set_host_status(state, VST_HOST_STATE_RUNNING);
			InterlockedExchange(&state.restart_attempted, 0);
		}
		else
		{
			trace_oop(L"[VST OOP bridge %s] restart thread failed\r\n", host_kind_name(kind));
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
		}

	InterlockedExchange(&state.restart_in_progress, 0);
	return 0;
}

	unsigned __stdcall state_dirty_watch_thread_proc(void* context)
	{
		VstChainKind kind = static_cast<VstChainKind>(reinterpret_cast<uintptr_t>(context));
		HostProcessState& state = g_host_processes[kind];

		if (!is_valid_kind(kind))
			return 1;

		for (;;)
		{
			HANDLE wait_handles[3] = { state.state_dirty_stop_event, state.process_handle, state.state_dirty_event };
			DWORD wait_result = WaitForMultipleObjects(3, wait_handles, FALSE, INFINITE);
			LONG dirty_generation;
			VstStateChangedCallback callback;

			if (wait_result == WAIT_OBJECT_0 || wait_result == WAIT_FAILED)
				break;
			if (wait_result == WAIT_OBJECT_0 + 1)
				break;
			if (wait_result != WAIT_OBJECT_0 + 2)
				continue;

			AcquireSRWLockShared(&state.audio_transport_lock);
			dirty_generation = state.audio_shared ? InterlockedCompareExchange(&state.audio_shared->state_dirty_generation, 0, 0) : 0;
			ReleaseSRWLockShared(&state.audio_transport_lock);
			if (dirty_generation <= InterlockedCompareExchange(&state.last_state_dirty_generation, 0, 0))
				continue;

			for (;;)
			{
				DWORD coalesce_wait = WaitForMultipleObjects(3, wait_handles, FALSE, kStateDirtyCoalesceDelayMs);

				if (coalesce_wait == WAIT_OBJECT_0 || coalesce_wait == WAIT_FAILED)
					return 0;
				if (coalesce_wait == WAIT_OBJECT_0 + 1)
					return 0;
				if (coalesce_wait != WAIT_OBJECT_0 + 2)
					break;

				AcquireSRWLockShared(&state.audio_transport_lock);
				dirty_generation = state.audio_shared ? InterlockedCompareExchange(&state.audio_shared->state_dirty_generation, 0, 0) : dirty_generation;
				ReleaseSRWLockShared(&state.audio_transport_lock);
			}

			if (host_command_available(kind) && !update_cached_chain_snapshot(kind, true))
				continue;
			InterlockedExchange(&state.last_state_dirty_generation, dirty_generation);

			callback = g_state_changed_callback;
			if (callback)
				callback(kind);
		}

		return 0;
	}

	int process_audio_via_host(VstChainKind kind, double* buffer, int frames)
	{
		HostProcessState& state = g_host_processes[kind];
		VstHostAudioSharedBlock* audio_shared;
		HANDLE audio_request_event;
		double dry_copy[VST_HOST_IPC_AUDIO_MAX_SAMPLES];
		int sample_count;
		LONG consume_sequence;
		int consume_slot;
		LONG current_sequence;
		int current_slot;
		LONG pipeline_latency;
		LONG min_pipeline_latency;
		LONG max_pipeline_latency = static_cast<LONG>(VST_HOST_IPC_AUDIO_SLOT_COUNT - 1u);
		LONG minimum_pipeline_latency;
		bool consumed_processed_output = false;
		bool used_concealment = false;

		/*
		This path must never block the DSP thread. The bridge consumes a processed
		block from an older sequence, submits the current dry block to the host, and
		uses adaptive latency to absorb jitter. RX prefers concealment with the last
		good wet block; TX falls back to dry audio when a block is late.
		*/
		if (!buffer || frames <= 0 || frames > static_cast<int>(VST_HOST_IPC_AUDIO_MAX_FRAMES))
			return -1;
		if (!is_valid_kind(kind) || query_host_status(kind) != VST_HOST_STATE_RUNNING)
		{
			ensure_host_recovery(kind);
			return -2;
		}

		AcquireSRWLockShared(&state.audio_transport_lock);
		audio_shared = state.audio_shared;
		audio_request_event = state.audio_request_event;
		if (!audio_shared || !audio_request_event)
		{
			ReleaseSRWLockShared(&state.audio_transport_lock);
			return -2;
		}

		++state.audio_blocks_submitted;
		sample_count = frames * static_cast<int>(VST_HOST_IPC_AUDIO_MAX_CHANNELS);
		memcpy(dry_copy, buffer, sample_count * sizeof(double));

		pipeline_latency = state.audio_pipeline_latency;
		min_pipeline_latency = state.audio_min_pipeline_latency;
		minimum_pipeline_latency = minimum_pipeline_latency_for_kind(kind);
		if (pipeline_latency < minimum_pipeline_latency)
			pipeline_latency = minimum_pipeline_latency;
		if (min_pipeline_latency < minimum_pipeline_latency)
			min_pipeline_latency = minimum_pipeline_latency;
		if (pipeline_latency > max_pipeline_latency)
			pipeline_latency = max_pipeline_latency;
		if (min_pipeline_latency > max_pipeline_latency)
			min_pipeline_latency = max_pipeline_latency;
		if (pipeline_latency < min_pipeline_latency)
			pipeline_latency = min_pipeline_latency;

		consume_sequence = state.audio_request_sequence - (pipeline_latency - 1);
		if (consume_sequence > state.audio_consumed_sequence)
		{
			consume_slot = consume_sequence % VST_HOST_IPC_AUDIO_SLOT_COUNT;
			if (InterlockedCompareExchange(&audio_shared->processed_sequence[consume_slot], 0, 0) == consume_sequence &&
				InterlockedCompareExchange(&audio_shared->output_frames[consume_slot], 0, 0) == frames)
			{
				memcpy(buffer, audio_shared->output_buffer[consume_slot], sample_count * sizeof(double));
				memcpy(state.audio_last_output, buffer, sample_count * sizeof(double));
				state.audio_last_output_frames = frames;
				state.audio_consumed_sequence = consume_sequence;
				++state.audio_blocks_consumed;
				++state.audio_consecutive_ready_blocks;
				consumed_processed_output = true;
			}
			else
			{
				++state.audio_not_ready_blocks;
				state.audio_consecutive_ready_blocks = 0;
				if (kind == VST_CHAIN_RX && out_of_process_trace_enabled())
				{
					trace_oop(
						L"[VST OOP bridge rx miss] requested=%ld consumed=%ld target=%ld slot=%d outstanding=%ld latency=%ld min_latency=%ld frames=%d\r\n",
						state.audio_request_sequence,
						state.audio_consumed_sequence,
						consume_sequence,
						consume_slot,
						state.audio_request_sequence - state.audio_consumed_sequence,
						state.audio_pipeline_latency,
						state.audio_min_pipeline_latency,
						frames);
				}
				if (state.audio_pipeline_latency < max_pipeline_latency)
					++state.audio_pipeline_latency;
				if (state.audio_pipeline_latency > state.audio_min_pipeline_latency)
					state.audio_min_pipeline_latency = state.audio_pipeline_latency;
			}
		}
		else
		{
			++state.audio_consecutive_ready_blocks;
		}

		if (!consumed_processed_output &&
			kind == VST_CHAIN_RX &&
			state.audio_last_output_frames == frames)
		{
			memcpy(buffer, state.audio_last_output, sample_count * sizeof(double));
			++state.audio_concealed_blocks;
			used_concealment = true;
			if (out_of_process_trace_enabled())
			{
				trace_oop(
					L"[VST OOP bridge rx conceal] requested=%ld consumed=%ld outstanding=%ld latency=%ld min_latency=%ld frames=%d\r\n",
					state.audio_request_sequence,
					state.audio_consumed_sequence,
					state.audio_request_sequence - state.audio_consumed_sequence,
					state.audio_pipeline_latency,
					state.audio_min_pipeline_latency,
					frames);
			}
		}

		if (!consumed_processed_output)
		{
			if (!used_concealment)
				++state.audio_fallback_blocks;
		}

		if (consumed_processed_output &&
			state.audio_pipeline_latency > state.audio_min_pipeline_latency &&
			state.audio_consecutive_ready_blocks >= kPipelineLatencyReduceStableBlocks)
		{
			--state.audio_pipeline_latency;
			state.audio_consecutive_ready_blocks = 0;
		}
		else if (consumed_processed_output &&
			state.audio_pipeline_latency == state.audio_min_pipeline_latency &&
			state.audio_min_pipeline_latency > default_pipeline_latency_for_kind(kind) &&
			state.audio_consecutive_ready_blocks >= kPipelineLatencyFloorReduceStableBlocks)
		{
			--state.audio_min_pipeline_latency;
			--state.audio_pipeline_latency;
			state.audio_consecutive_ready_blocks = 0;
		}

		current_sequence = InterlockedIncrement(&state.audio_request_sequence);
		current_slot = current_sequence % VST_HOST_IPC_AUDIO_SLOT_COUNT;
		memcpy(audio_shared->input_buffer[current_slot], dry_copy, sample_count * sizeof(double));
		InterlockedExchange(&audio_shared->input_frames[current_slot], frames);
		InterlockedExchange(&audio_shared->input_sequence[current_slot], current_sequence);
		state.audio_pending_sequence = current_sequence;
		state.audio_pending_frames = frames;
		InterlockedExchange(&audio_shared->request_sequence, current_sequence);
		if (!SetEvent(audio_request_event))
		{
			++state.audio_signal_failures;
			ensure_host_recovery(kind);
		}

		if (out_of_process_trace_enabled())
		{
			DWORD now = GetTickCount();
			if (state.audio_last_log_tick == 0)
				state.audio_last_log_tick = now;
			if (now - state.audio_last_log_tick >= 2000)
			{
				LONG outstanding = state.audio_request_sequence - state.audio_consumed_sequence;
				trace_oop(
					L"[VST OOP bridge %s] submitted=%llu consumed=%llu fallback=%llu concealed=%llu not_ready=%llu frame_mismatch=%llu signal_fail=%llu outstanding=%ld latency=%ld\r\n",
					host_kind_name(kind),
					state.audio_blocks_submitted,
					state.audio_blocks_consumed,
					state.audio_fallback_blocks,
					state.audio_concealed_blocks,
					state.audio_not_ready_blocks,
					state.audio_frame_mismatch_blocks,
					state.audio_signal_failures,
					outstanding,
					state.audio_pipeline_latency);
				state.audio_last_log_tick = now;
			}
		}
		ReleaseSRWLockShared(&state.audio_transport_lock);
		return 0;
	}

	bool launch_host_process(VstChainKind kind)
	{
		HostProcessState& state = g_host_processes[kind];
		std::wstring executable_path;
		std::wstring command_line;
		STARTUPINFOW startup_info = {};
		PROCESS_INFORMATION process_information = {};
		DWORD wait_result;
		DWORD current_process_id = GetCurrentProcessId();

		if (state.process_handle)
			return true;
		if (!get_host_executable_path(executable_path))
		{
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			return false;
		}
		set_host_status(state, VST_HOST_STATE_STARTING);

		swprintf_s(state.ready_event_name, L"Local\\ThetisVstHostReady_%lu_%s", current_process_id, host_kind_name(kind));
		swprintf_s(state.shutdown_event_name, L"Local\\ThetisVstHostShutdown_%lu_%s", current_process_id, host_kind_name(kind));
		swprintf_s(state.control_pipe_name, L"\\\\.\\pipe\\ThetisVstHostControl_%lu_%s", current_process_id, host_kind_name(kind));
		if (!initialize_audio_transport(kind, state))
		{
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			close_host_process_handles(kind, state);
			return false;
		}

		state.ready_event = CreateEventW(0, TRUE, FALSE, state.ready_event_name);
		if (!state.ready_event)
		{
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			close_host_process_handles(kind, state);
			return false;
		}

		state.shutdown_event = CreateEventW(0, TRUE, FALSE, state.shutdown_event_name);
		if (!state.shutdown_event)
		{
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			close_host_process_handles(kind, state);
			return false;
		}

		command_line = L"\"";
		command_line += executable_path;
		command_line += L"\" --chain ";
		command_line += host_kind_name(kind);
		command_line += L" --ready-event \"";
		command_line += state.ready_event_name;
		command_line += L"\" --shutdown-event \"";
		command_line += state.shutdown_event_name;
		command_line += L"\" --control-pipe \"";
		command_line += state.control_pipe_name;
		command_line += L"\" --audio-mapping \"";
		command_line += state.audio_mapping_name;
		command_line += L"\" --audio-request-event \"";
		command_line += state.audio_request_event_name;
		command_line += L"\" --state-dirty-event \"";
		command_line += state.state_dirty_event_name;
		command_line += L"\"";

		startup_info.cb = sizeof(startup_info);
		if (!CreateProcessW(
			executable_path.c_str(),
			command_line.data(),
			0,
			0,
			FALSE,
			CREATE_NO_WINDOW,
			0,
			0,
			&startup_info,
			&process_information))
		{
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			close_host_process_handles(kind, state);
			return false;
		}

		state.process_handle = process_information.hProcess;
		state.thread_handle = process_information.hThread;

		wait_result = WaitForSingleObject(state.ready_event, 2000);
		if (wait_result != WAIT_OBJECT_0)
		{
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			if (state.shutdown_event)
				SetEvent(state.shutdown_event);
			if (state.process_handle)
			{
				WaitForSingleObject(state.process_handle, 500);
				TerminateProcess(state.process_handle, 1);
			}
			close_host_process_handles(kind, state);
			return false;
		}
		if (!connect_to_host_pipe(state) || !validate_host_handshake(state, kind))
		{
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			if (state.shutdown_event)
				SetEvent(state.shutdown_event);
			if (state.process_handle)
			{
				WaitForSingleObject(state.process_handle, 500);
				TerminateProcess(state.process_handle, 1);
			}
			close_host_process_handles(kind, state);
			return false;
		}

		state.state_dirty_stop_event = CreateEventW(0, TRUE, FALSE, 0);
		if (!state.state_dirty_stop_event)
		{
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			if (state.shutdown_event)
				SetEvent(state.shutdown_event);
			if (state.process_handle)
			{
				WaitForSingleObject(state.process_handle, 500);
				TerminateProcess(state.process_handle, 1);
			}
			close_host_process_handles(kind, state);
			return false;
		}

		{
			uintptr_t watch_thread = _beginthreadex(nullptr, 0, state_dirty_watch_thread_proc, reinterpret_cast<void*>(static_cast<uintptr_t>(kind)), 0, nullptr);
			if (!watch_thread)
			{
				set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
				if (state.shutdown_event)
					SetEvent(state.shutdown_event);
				if (state.process_handle)
				{
					WaitForSingleObject(state.process_handle, 500);
					TerminateProcess(state.process_handle, 1);
				}
				close_host_process_handles(kind, state);
				return false;
			}

			state.state_dirty_thread = reinterpret_cast<HANDLE>(watch_thread);
		}

		set_host_status(state, VST_HOST_STATE_RUNNING);
		return true;
	}

	void shutdown_host_process(VstChainKind kind)
	{
		HostProcessState& state = g_host_processes[kind];

		if (!state.process_handle)
		{
			set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
			close_host_process_handles(kind, state);
			return;
		}

		AcquireSRWLockExclusive(&state.control_lock);
		if (state.control_pipe_handle != INVALID_HANDLE_VALUE)
			write_control_message(state.control_pipe_handle, VST_HOST_IPC_SHUTDOWN, static_cast<uint32_t>(kind));
		ReleaseSRWLockExclusive(&state.control_lock);
		if (state.shutdown_event)
			SetEvent(state.shutdown_event);

		if (WaitForSingleObject(state.process_handle, 2000) == WAIT_TIMEOUT)
			TerminateProcess(state.process_handle, 0);

		set_host_status(state, VST_HOST_STATE_UNAVAILABLE);
		close_host_process_handles(kind, state);
	}

	void ensure_initialized()
	{
		if (!InterlockedCompareExchange(&g_initialized, 1, 1))
			VST_Initialize();
	}

	bool is_valid_kind(VstChainKind kind)
	{
		return kind >= VST_CHAIN_RX && kind < VST_CHAIN_COUNT;
	}
}

int VST_Initialize(void)
{
	if (InterlockedCompareExchange(&g_initialized, 1, 0))
		return 0;

	for (int i = 0; i < VST_CHAIN_COUNT; ++i)
		launch_host_process(static_cast<VstChainKind>(i));

	return 0;
}

void VST_Shutdown(void)
{
	if (!InterlockedCompareExchange(&g_initialized, 0, 1))
		return;

	for (int i = 0; i < VST_CHAIN_COUNT; ++i)
	{
		shutdown_host_process(static_cast<VstChainKind>(i));
		clear_cached_chain_snapshot(g_host_processes[i]);
		InterlockedExchange(&g_host_processes[i].restart_attempted, 0);
		InterlockedExchange(&g_host_processes[i].restart_in_progress, 0);
		set_host_status(g_host_processes[i], VST_HOST_STATE_DISABLED);
	}
}

int VST_GetSdkAvailable(void)
{
	return Vst3Sdk_IsAvailable();
}

int VST_ProbePluginMetadataOnly(const wchar_t* plugin_path, VstPluginProbeInfo* info)
{
	ensure_initialized();

	if (detect_plugin_format_from_path(plugin_path) == VST_PLUGIN_FORMAT_VST2)
	{
		__try
		{
			return Vst2Runtime_ProbePluginMetadataOnly(plugin_path, info);
		}
		__except (EXCEPTION_EXECUTE_HANDLER)
		{
			if (info)
			{
				if (info->is_valid)
					return 0;
				memset(info, 0, sizeof(*info));
				if (plugin_path)
					wcsncpy_s(info->path, VST_MAX_PLUGIN_PATH_CHARS, plugin_path, _TRUNCATE);
			}
			return -7;
		}
	}

	__try
	{
		return VstRuntime_ProbePluginMetadataOnly(plugin_path, info);
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		if (info)
		{
			/* If the metadata was already filled in successfully but
			   the crash happened during module unloading
			   (DLL_PROCESS_DETACH in the plugin's DllMain), preserve
			   the metadata and return success. */
			if (info->is_valid)
				return 0;

			/* If the name was populated, the probe completed class
			   enumeration but found no suitable audio effect — return
			   the "not an audio effect" code instead of "crashed". */
			if (info->name[0] != L'\0')
				return -6;

			memset(info, 0, sizeof(*info));
			if (plugin_path)
				wcsncpy_s(info->path, VST_MAX_PLUGIN_PATH_CHARS, plugin_path, _TRUNCATE);
		}
		return -7;
	}
}

int VST_CreateChain(VstChainKind kind, int sample_rate, int max_block_size, int num_channels)
{
	int host_result;
	int cached_before = 0;
	int cached_after = 0;
	HostProcessState& host_state = g_host_processes[kind];

	ensure_initialized();

	if (!is_valid_kind(kind))
		return -2;

	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

	AcquireSRWLockShared(&host_state.cache_lock);
	cached_before = static_cast<int>(host_state.cached_plugins.size());
	ReleaseSRWLockShared(&host_state.cache_lock);

	if (host_command_available(kind))
	{
		host_result = host_command_simple(kind, VST_HOST_IPC_CREATE_CHAIN, sample_rate, max_block_size, num_channels, 0.0);
		if (host_result != 0)
			return host_result;
	}
	else
	{
		ensure_host_recovery(kind);
	}

	initialize_cached_chain_snapshot(host_state, sample_rate, max_block_size, num_channels);
	AcquireSRWLockShared(&host_state.cache_lock);
	cached_after = static_cast<int>(host_state.cached_plugins.size());
	ReleaseSRWLockShared(&host_state.cache_lock);
	trace_oop(
		L"[VST bridge %s] create chain sr=%d block=%d ch=%d cached_before=%d cached_after=%d\r\n",
		host_kind_name(kind),
		sample_rate,
		max_block_size,
		num_channels,
		cached_before,
		cached_after);
	return 0;
}

	void VST_DestroyChain(VstChainKind kind)
	{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return;

	HostProcessState& host_state = g_host_processes[kind];
	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

	if (host_command_available(kind))
		host_command_simple(kind, VST_HOST_IPC_DESTROY_CHAIN, 0, 0, 0, 0.0);
	else
		ensure_host_recovery(kind);
	clear_cached_chain_snapshot(host_state);
	InterlockedExchange(&host_state.restart_attempted, 0);
	InterlockedExchange(&host_state.restart_in_progress, 0);
}

void VST_SetChainBypass(VstChainKind kind, int bypass)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return;

	HostProcessState& host_state = g_host_processes[kind];
	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

	if (host_command_available(kind))
		host_command_simple(kind, VST_HOST_IPC_SET_CHAIN_BYPASS, bypass, 0, 0, 0.0);
	else
		ensure_host_recovery(kind);
	AcquireSRWLockExclusive(&host_state.cache_lock);
	host_state.cached_bypass = bypass ? 1 : 0;
	advance_cache_generation(host_state);
	ReleaseSRWLockExclusive(&host_state.cache_lock);
}

int VST_GetChainBypass(VstChainKind kind)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return 0;

	int result;
	AcquireSRWLockShared(&g_host_processes[kind].cache_lock);
	result = g_host_processes[kind].cached_bypass;
	ReleaseSRWLockShared(&g_host_processes[kind].cache_lock);
	return result;
}

void VST_SetChainGain(VstChainKind kind, double gain)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return;

	HostProcessState& host_state = g_host_processes[kind];
	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

	if (host_command_available(kind))
		host_command_simple(kind, VST_HOST_IPC_SET_CHAIN_GAIN, 0, 0, 0, gain);
	else
		ensure_host_recovery(kind);
	AcquireSRWLockExclusive(&host_state.cache_lock);
	host_state.cached_gain = gain;
	advance_cache_generation(host_state);
	ReleaseSRWLockExclusive(&host_state.cache_lock);
}

double VST_GetChainGain(VstChainKind kind)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return 1.0;

	double gain;
	AcquireSRWLockShared(&g_host_processes[kind].cache_lock);
	gain = g_host_processes[kind].cached_gain;
	ReleaseSRWLockShared(&g_host_processes[kind].cache_lock);
	return gain;
}

int VST_GetChainReady(VstChainKind kind)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return 0;
	if (query_host_status(kind) != VST_HOST_STATE_RUNNING)
		return 0;

	AcquireSRWLockShared(&g_host_processes[kind].cache_lock);
	int ready = g_host_processes[kind].cached_ready;
	ReleaseSRWLockShared(&g_host_processes[kind].cache_lock);
	return ready;
}

int VST_ClearChain(VstChainKind kind)
{
	int host_result = 0;

	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	HostProcessState& host_state = g_host_processes[kind];
	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

	if (host_command_available(kind))
	{
		host_result = host_command_simple(kind, VST_HOST_IPC_CLEAR_CHAIN, 0, 0, 0, 0.0);
		if (host_result != 0)
			return host_result;
	}
	else
	{
		ensure_host_recovery(kind);
	}

	AcquireSRWLockExclusive(&host_state.cache_lock);
	host_state.cached_plugins.clear();
	advance_cache_generation(host_state);
	ReleaseSRWLockExclusive(&host_state.cache_lock);
	trace_oop(
		L"[VST bridge %s] clear chain cached_after=%d\r\n",
		host_kind_name(kind),
		VST_GetPluginCount(kind));
	return 0;
}

int VST_AddPlugin(VstChainKind kind, const wchar_t* plugin_path)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	HostProcessState& host_state = g_host_processes[kind];
	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

	if (host_command_available(kind))
	{
		HostProcessState::CachedPluginState cached_plugin;
		int plugin_index;
		VstPluginInfo host_info = {};

		VstHostIpcPacket request = {};
		VstHostIpcPacket response = {};

		request.header.type = VST_HOST_IPC_ADD_PLUGIN;
		wcsncpy_s(request.plugin_path, VST_MAX_PLUGIN_PATH_CHARS, plugin_path, _TRUNCATE);
		if (!send_host_request(kind, request, response) || response.result < 0)
		{
			trace_oop(
				L"[VST bridge %s] add plugin failed path=\"%s\" result=%d\r\n",
				host_kind_name(kind),
				plugin_path ? plugin_path : L"",
				response.result < 0 ? response.result : -300);
			return response.result < 0 ? response.result : -300;
		}
		plugin_index = response.result;

		initialize_cached_plugin_state(cached_plugin, plugin_path);
		cached_plugin.load_state = host_get_plugin_info(kind, plugin_index, host_info) == 0 ? host_info.load_state : VST_PLUGIN_LOAD_DESCRIPTOR_ONLY;
		if (host_info.name[0] != L'\0')
			wcsncpy_s(cached_plugin.name, VST_MAX_PLUGIN_NAME_CHARS, host_info.name, _TRUNCATE);
		if (host_info.path[0] != L'\0')
			wcsncpy_s(cached_plugin.path, VST_MAX_PLUGIN_PATH_CHARS, host_info.path, _TRUNCATE);
		if (host_info.format != VST_PLUGIN_FORMAT_UNKNOWN)
			cached_plugin.format = host_info.format;
		if (host_info.enabled || host_info.bypass || host_info.load_state)
		{
			cached_plugin.enabled = host_info.enabled;
			cached_plugin.bypass = host_info.bypass;
		}
		AcquireSRWLockExclusive(&host_state.cache_lock);
		if (plugin_index >= 0 && plugin_index <= static_cast<int>(host_state.cached_plugins.size()))
			host_state.cached_plugins.insert(host_state.cached_plugins.begin() + plugin_index, cached_plugin);
		advance_cache_generation(host_state);
		ReleaseSRWLockExclusive(&host_state.cache_lock);
		trace_oop(
			L"[VST bridge %s] add plugin path=\"%s\" index=%d cached_count=%d host_load=%d format=%d\r\n",
			host_kind_name(kind),
			plugin_path ? plugin_path : L"",
			plugin_index,
			VST_GetPluginCount(kind),
			cached_plugin.load_state,
			cached_plugin.format);
		return plugin_index;
	}
	else
	{
		HostProcessState::CachedPluginState cached_plugin;
		int plugin_index;

		initialize_cached_plugin_state(cached_plugin, plugin_path);
		ensure_host_recovery(kind);
		AcquireSRWLockExclusive(&host_state.cache_lock);
		plugin_index = static_cast<int>(host_state.cached_plugins.size());
		host_state.cached_plugins.push_back(cached_plugin);
		advance_cache_generation(host_state);
		ReleaseSRWLockExclusive(&host_state.cache_lock);
		trace_oop(
			L"[VST bridge %s] add plugin path=\"%s\" index=%d cached_count=%d host_load=%d format=%d\r\n",
			host_kind_name(kind),
			plugin_path ? plugin_path : L"",
			plugin_index,
			VST_GetPluginCount(kind),
			cached_plugin.load_state,
			cached_plugin.format);
		return plugin_index;
	}
}

int VST_RemovePlugin(VstChainKind kind, int index)
{
	int host_result = 0;

	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	HostProcessState& host_state = g_host_processes[kind];
	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);
	AcquireSRWLockShared(&host_state.cache_lock);
	bool index_valid = index >= 0 && index < static_cast<int>(host_state.cached_plugins.size());
	ReleaseSRWLockShared(&host_state.cache_lock);
	if (!index_valid)
		return -1;
	if (host_command_available(kind))
	{
		host_result = host_command_simple(kind, VST_HOST_IPC_REMOVE_PLUGIN, index, 0, 0, 0.0);
		if (host_result != 0)
			return host_result;
	}
	else
	{
		ensure_host_recovery(kind);
	}

	AcquireSRWLockExclusive(&host_state.cache_lock);
	host_state.cached_plugins.erase(host_state.cached_plugins.begin() + index);
	advance_cache_generation(host_state);
	ReleaseSRWLockExclusive(&host_state.cache_lock);
	trace_oop(
		L"[VST bridge %s] remove plugin index=%d cached_count=%d\r\n",
		host_kind_name(kind),
		index,
		VST_GetPluginCount(kind));
	return 0;
}

int VST_MovePlugin(VstChainKind kind, int from_index, int to_index)
{
	int host_result = 0;

	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	HostProcessState& host_state = g_host_processes[kind];
	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);
	AcquireSRWLockShared(&host_state.cache_lock);
	bool indexes_valid = from_index >= 0 && to_index >= 0 &&
		from_index < static_cast<int>(host_state.cached_plugins.size()) &&
		to_index < static_cast<int>(host_state.cached_plugins.size());
	ReleaseSRWLockShared(&host_state.cache_lock);
	if (!indexes_valid)
		return -1;
	if (host_command_available(kind))
	{
		host_result = host_command_simple(kind, VST_HOST_IPC_MOVE_PLUGIN, from_index, to_index, 0, 0.0);
		if (host_result != 0)
			return host_result;
	}
	else
	{
		ensure_host_recovery(kind);
	}

	AcquireSRWLockExclusive(&host_state.cache_lock);
	HostProcessState::CachedPluginState moved_plugin = host_state.cached_plugins[from_index];
	host_state.cached_plugins.erase(host_state.cached_plugins.begin() + from_index);
	host_state.cached_plugins.insert(host_state.cached_plugins.begin() + to_index, moved_plugin);
	advance_cache_generation(host_state);
	ReleaseSRWLockExclusive(&host_state.cache_lock);
	trace_oop(
		L"[VST bridge %s] move plugin from=%d to=%d cached_count=%d\r\n",
		host_kind_name(kind),
		from_index,
		to_index,
		VST_GetPluginCount(kind));
	return 0;
}

int VST_GetPluginCount(VstChainKind kind)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	int cached_count;
	AcquireSRWLockShared(&g_host_processes[kind].cache_lock);
	cached_count = static_cast<int>(g_host_processes[kind].cached_plugins.size());
	ReleaseSRWLockShared(&g_host_processes[kind].cache_lock);
	trace_oop(L"[VST bridge %s] get plugin count=%d\r\n", host_kind_name(kind), cached_count);
	return cached_count;
}

int VST_GetPluginInfo(VstChainKind kind, int index, VstPluginInfo* info)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;
	if (!info)
		return -1;

	int result = cached_get_plugin_info(g_host_processes[kind], index, info);
	if (result == 0)
	{
		trace_oop(
			L"[VST bridge %s] get plugin info index=%d result=%d name=\"%s\" path=\"%s\" load=%d format=%d enabled=%d bypass=%d\r\n",
			host_kind_name(kind),
			index,
			result,
			info->name,
			info->path,
			info->load_state,
			info->format,
			info->enabled,
			info->bypass);
	}
	else
	{
		trace_oop(L"[VST bridge %s] get plugin info index=%d result=%d\r\n", host_kind_name(kind), index, result);
	}
	return result;
}

int VST_SetPluginBypass(VstChainKind kind, int index, int bypass)
{
	int host_result;

	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	HostProcessState& host_state = g_host_processes[kind];
	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

	AcquireSRWLockShared(&host_state.cache_lock);
	bool index_valid = index >= 0 && index < static_cast<int>(host_state.cached_plugins.size());
	ReleaseSRWLockShared(&host_state.cache_lock);
	if (!index_valid)
		return -1;
	if (host_command_available(kind))
	{
		host_result = host_command_simple(kind, VST_HOST_IPC_SET_PLUGIN_BYPASS, index, bypass, 0, 0.0);
		if (host_result != 0)
			return host_result;
	}
	else
	{
		ensure_host_recovery(kind);
	}
	AcquireSRWLockExclusive(&host_state.cache_lock);
	host_state.cached_plugins[index].bypass = bypass;
	advance_cache_generation(host_state);
	ReleaseSRWLockExclusive(&host_state.cache_lock);
	return 0;
}

int VST_SetPluginEnabled(VstChainKind kind, int index, int enabled)
{
	int host_result;

	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	HostProcessState& host_state = g_host_processes[kind];
	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

	AcquireSRWLockShared(&host_state.cache_lock);
	bool index_valid = index >= 0 && index < static_cast<int>(host_state.cached_plugins.size());
	ReleaseSRWLockShared(&host_state.cache_lock);
	if (!index_valid)
		return -1;
	if (host_command_available(kind))
	{
		host_result = host_command_simple(kind, VST_HOST_IPC_SET_PLUGIN_ENABLED, index, enabled, 0, 0.0);
		if (host_result != 0)
			return host_result;
	}
	else
	{
		ensure_host_recovery(kind);
	}
	AcquireSRWLockExclusive(&host_state.cache_lock);
	host_state.cached_plugins[index].enabled = enabled;
	advance_cache_generation(host_state);
	ReleaseSRWLockExclusive(&host_state.cache_lock);
	return 0;
}

int VST_GetPluginStateSize(VstChainKind kind, int index)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	int result = -1;
	AcquireSRWLockShared(&g_host_processes[kind].cache_lock);
	if (index >= 0 && index < static_cast<int>(g_host_processes[kind].cached_plugins.size()))
		result = static_cast<int>(g_host_processes[kind].cached_plugins[index].state_data.size());
	ReleaseSRWLockShared(&g_host_processes[kind].cache_lock);
	return result;
}

int VST_GetPluginState(VstChainKind kind, int index, void* buffer, int buffer_size, int* bytes_written)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	return cached_get_plugin_state(g_host_processes[kind], index, buffer, buffer_size, bytes_written);
}

int VST_SetPluginState(VstChainKind kind, int index, const void* buffer, int buffer_size)
{
	int host_result;

	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	HostProcessState& host_state = g_host_processes[kind];
	ScopedExclusiveLock mutation_guard(&host_state.mutation_lock);

	AcquireSRWLockShared(&host_state.cache_lock);
	bool index_valid = index >= 0 && index < static_cast<int>(host_state.cached_plugins.size());
	ReleaseSRWLockShared(&host_state.cache_lock);
	if (!index_valid)
		return -1;
	if (!buffer || buffer_size <= 0)
		return -1;
	if (host_command_available(kind))
	{
		host_result = host_set_plugin_state(kind, index, buffer, buffer_size) ? 0 : -300;
		if (host_result != 0)
			return host_result;
	}
	else
	{
		ensure_host_recovery(kind);
	}
	AcquireSRWLockExclusive(&host_state.cache_lock);
	host_state.cached_plugins[index].load_state = VST_PLUGIN_LOAD_ACTIVE;
	host_state.cached_plugins[index].state_data.assign(static_cast<const unsigned char*>(buffer), static_cast<const unsigned char*>(buffer) + buffer_size);
	advance_cache_generation(host_state);
	ReleaseSRWLockExclusive(&host_state.cache_lock);
	return 0;
}

int VST_GetHostState(VstChainKind kind)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return VST_HOST_STATE_UNAVAILABLE;

	return static_cast<int>(query_host_status(kind));
}

int VST_GetChainSnapshotGeneration(VstChainKind kind)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return 0;

	return static_cast<int>(InterlockedCompareExchange(&g_host_processes[kind].cache_generation, 0, 0));
}

int VST_GetPipelineLatency(VstChainKind kind, int* current_blocks, int* floor_blocks)
{
	if (!is_valid_kind(kind))
		return -1;

	HostProcessState& state = g_host_processes[kind];
	if (current_blocks)
		*current_blocks = static_cast<int>(InterlockedCompareExchange(&state.audio_pipeline_latency, 0, 0));
	if (floor_blocks)
		*floor_blocks = static_cast<int>(InterlockedCompareExchange(&state.audio_min_pipeline_latency, 0, 0));
	return 0;
}

void VST_SetPipelineLatencyFloor(VstChainKind kind, int floor_blocks)
{
	if (!is_valid_kind(kind))
		return;

	HostProcessState& state = g_host_processes[kind];
	LONG minimum = minimum_pipeline_latency_for_kind(kind);
	LONG clamped = floor_blocks < minimum ? minimum : static_cast<LONG>(floor_blocks);

	InterlockedExchange(&state.audio_min_pipeline_latency, clamped);

	// If current latency is below the new floor, raise it.
	LONG current = InterlockedCompareExchange(&state.audio_pipeline_latency, 0, 0);
	if (current < clamped)
		InterlockedExchange(&state.audio_pipeline_latency, clamped);
}

int VST_GetSampleRate(VstChainKind kind)
{
	if (!is_valid_kind(kind))
		return 0;

	HostProcessState& state = g_host_processes[kind];
	AcquireSRWLockShared(&state.cache_lock);
	int sample_rate = state.cached_sample_rate;
	ReleaseSRWLockShared(&state.cache_lock);
	return sample_rate;
}

int VST_GetBlockSize(VstChainKind kind)
{
	if (!is_valid_kind(kind))
		return 0;

	HostProcessState& state = g_host_processes[kind];
	AcquireSRWLockShared(&state.cache_lock);
	int block_size = state.cached_max_block_size;
	ReleaseSRWLockShared(&state.cache_lock);
	return block_size;
}

int VST_RequestHostChainSync(VstChainKind kind)
{
	int result;

	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;

	result = request_host_chain_sync(kind) ? 0 : -300;
	trace_oop(
		L"[VST bridge %s] request host chain sync result=%d cached_count=%d\r\n",
		host_kind_name(kind),
		result,
		VST_GetPluginCount(kind));
	return result;
}

void VST_SetStateChangedCallback(VstStateChangedCallback callback)
{
	g_state_changed_callback = callback;
}

int VST_OpenPluginEditorWindow(VstChainKind kind, int index)
{
	ensure_initialized();

	if (!is_valid_kind(kind))
		return 0;

	if (host_command_available(kind) && host_open_plugin_editor(kind, index))
		return 1;
	return 0;
}

int VST_ProcessInterleavedDouble(VstChainKind kind, double* buffer, int frames)
{
	int host_result;
	int chain_bypass = 0;

	ensure_initialized();

	if (!is_valid_kind(kind))
		return -1;
	if (!buffer || frames <= 0)
		return -1;

	AcquireSRWLockShared(&g_host_processes[kind].cache_lock);
	chain_bypass = g_host_processes[kind].cached_bypass;
	ReleaseSRWLockShared(&g_host_processes[kind].cache_lock);
	if (chain_bypass)
	{
		flush_bypassed_audio_transport(g_host_processes[kind]);
		return 0;
	}
	host_result = process_audio_via_host(kind, buffer, frames);
	if (host_result == 0)
		return 0;
	return 1;
}
