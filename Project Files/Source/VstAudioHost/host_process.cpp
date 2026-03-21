#include <Windows.h>

#include "host_process.h"

#include "../VstCommon/vst_ipc.h"
#include "../VstHostBridge/vst_chain.h"
#include "../VstHostBridge/vst3_sdk.h"

#include <cstdarg>
#include <cstdio>
#include <cstring>
#include <memory>
#include <process.h>
#include <string>

/*
VstAudioHost owns exactly one RX or TX chain per process.

The control thread handles IPC and editor/state commands. The audio worker owns
the shared-memory drain loop and must remain the highest-priority path in the
process.
*/

namespace
{
	VstChainState g_chains[VST_CHAIN_COUNT] = {};
	volatile LONG g_chain_snapshot_generation[VST_CHAIN_COUNT] = {};
	volatile LONG g_out_of_process_trace_enabled = -1;
	const DWORD kControlPipeIoTimeoutMs = 5000;
	const int kAvrtPriorityCritical = 2;

	struct HostAudioState
	{
		HANDLE mapping_handle = 0;
		HANDLE request_event = 0;
		HANDLE state_dirty_event = 0;
		HANDLE stop_event = 0;
		HANDLE thread_handle = 0;
		VstHostAudioSharedBlock* shared = 0;
		VstChainKind chain_kind = VST_CHAIN_RX;
		LARGE_INTEGER qpc_frequency = {};
		ULONGLONG blocks_processed = 0;
		ULONGLONG invalid_blocks = 0;
		ULONGLONG overrun_skipped_blocks = 0;
		ULONGLONG total_process_ticks = 0;
		ULONGLONG max_process_ticks = 0;
		DWORD last_log_tick = 0;
	};

	struct OpenEditorRequest
	{
		/*
		Ownership transfers to the worker thread immediately after _beginthreadex.
		The control thread only waits on completion and may delete the request
		object only after the worker has definitely exited.
		*/
		VstChainKind kind = VST_CHAIN_RX;
		int plugin_index = -1;
		HANDLE completed_event = 0;
		volatile LONG result = -1;
	};

	unsigned __stdcall open_editor_thread_proc(void* context);

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

	LONG current_chain_snapshot_generation(VstChainKind kind)
	{
		return static_cast<uint32_t>(kind) < VST_CHAIN_COUNT
			? InterlockedCompareExchange(&g_chain_snapshot_generation[kind], 0, 0)
			: 0;
	}

	void bump_chain_snapshot_generation(VstChainKind kind)
	{
		if (static_cast<uint32_t>(kind) < VST_CHAIN_COUNT)
			InterlockedIncrement(&g_chain_snapshot_generation[kind]);
	}

	void on_chain_state_dirty(void* context)
	{
		HostAudioState* audio_state = static_cast<HostAudioState*>(context);

		if (!audio_state || !audio_state->shared)
			return;

		bump_chain_snapshot_generation(audio_state->chain_kind);
		InterlockedIncrement(&audio_state->shared->state_dirty_generation);
		if (audio_state->state_dirty_event)
			SetEvent(audio_state->state_dirty_event);
	}

	void configure_host_process_priorities()
	{
		SetPriorityClass(GetCurrentProcess(), HIGH_PRIORITY_CLASS);
		SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_BELOW_NORMAL);
	}

	void configure_audio_thread_priority()
	{
		SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_HIGHEST);
		SetThreadPriorityBoost(GetCurrentThread(), TRUE);
	}

	void configure_editor_thread_priority()
	{
		SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_LOWEST);
	}

	struct MmcssRegistration
	{
		HMODULE avrt_module = 0;
		HANDLE task_handle = 0;
	};

	MmcssRegistration register_mmcss_pro_audio()
	{
		MmcssRegistration registration = {};
		typedef HANDLE(WINAPI* AvSetMmThreadCharacteristicsWFn)(LPCWSTR, LPDWORD);
		typedef BOOL(WINAPI* AvSetMmThreadPriorityFn)(HANDLE, int);
		AvSetMmThreadCharacteristicsWFn set_characteristics;
		AvSetMmThreadPriorityFn set_priority;
		DWORD task_index = 0;

		registration.avrt_module = LoadLibraryW(L"avrt.dll");
		if (!registration.avrt_module)
			return registration;

		set_characteristics = reinterpret_cast<AvSetMmThreadCharacteristicsWFn>(GetProcAddress(registration.avrt_module, "AvSetMmThreadCharacteristicsW"));
		set_priority = reinterpret_cast<AvSetMmThreadPriorityFn>(GetProcAddress(registration.avrt_module, "AvSetMmThreadPriority"));
		if (!set_characteristics)
			return registration;

		registration.task_handle = set_characteristics(L"Pro Audio", &task_index);
		if (registration.task_handle && set_priority)
			set_priority(registration.task_handle, kAvrtPriorityCritical);
		return registration;
	}

	void unregister_mmcss(MmcssRegistration& registration)
	{
		typedef BOOL(WINAPI* AvRevertMmThreadCharacteristicsFn)(HANDLE);
		AvRevertMmThreadCharacteristicsFn revert_characteristics;

		if (registration.avrt_module)
		{
			revert_characteristics = reinterpret_cast<AvRevertMmThreadCharacteristicsFn>(GetProcAddress(registration.avrt_module, "AvRevertMmThreadCharacteristics"));
			if (registration.task_handle && revert_characteristics)
				revert_characteristics(registration.task_handle);
			FreeLibrary(registration.avrt_module);
		}

		registration.avrt_module = 0;
		registration.task_handle = 0;
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

	bool try_get_argument_value(const wchar_t* command_line, const wchar_t* key, std::wstring& value)
	{
		const wchar_t* match;
		size_t key_length;

		value.clear();
		if (!command_line || !key || !key[0])
			return false;

		match = wcsstr(command_line, key);
		if (!match)
			return false;

		key_length = wcslen(key);
		match += key_length;
		while (*match == L' ' || *match == L'\t')
			++match;

		if (*match == L'"')
		{
			const wchar_t* end_quote;
			++match;
			end_quote = wcschr(match, L'"');
			if (!end_quote)
				return false;

			value.assign(match, end_quote - match);
			return !value.empty();
		}

		while (*match && *match != L' ' && *match != L'\t')
		{
			value.push_back(*match);
			++match;
		}

		return !value.empty();
	}

	bool wait_for_overlapped_pipe_io(HANDLE pipe_handle, OVERLAPPED& overlapped, HANDLE interrupt_handle, DWORD timeout_ms, DWORD& bytes_transferred)
	{
		HANDLE wait_handles[2] = { interrupt_handle, overlapped.hEvent };
		DWORD wait_result;

		wait_result = WaitForMultipleObjects(interrupt_handle ? 2 : 1, interrupt_handle ? wait_handles : &wait_handles[1], FALSE, timeout_ms);
		if (interrupt_handle && wait_result == WAIT_OBJECT_0)
		{
			CancelIoEx(pipe_handle, &overlapped);
			WaitForSingleObject(overlapped.hEvent, INFINITE);
			SetLastError(ERROR_OPERATION_ABORTED);
			return false;
		}
		if ((interrupt_handle && wait_result != WAIT_OBJECT_0 + 1) || (!interrupt_handle && wait_result != WAIT_OBJECT_0))
		{
			CancelIoEx(pipe_handle, &overlapped);
			WaitForSingleObject(overlapped.hEvent, INFINITE);
			SetLastError(wait_result == WAIT_TIMEOUT ? ERROR_SEM_TIMEOUT : ERROR_OPERATION_ABORTED);
			return false;
		}

		return GetOverlappedResult(pipe_handle, &overlapped, &bytes_transferred, TRUE) != FALSE;
	}

	bool transfer_packet(HANDLE pipe_handle, void* buffer, DWORD buffer_size, bool is_read, HANDLE interrupt_handle, DWORD timeout_ms)
	{
		OVERLAPPED overlapped = {};
		DWORD bytes_transferred = 0;
		BOOL io_result;
		DWORD last_error;

		overlapped.hEvent = CreateEventW(nullptr, TRUE, FALSE, nullptr);
		if (!overlapped.hEvent)
			return false;

		io_result = is_read
			? ReadFile(pipe_handle, buffer, buffer_size, nullptr, &overlapped)
			: WriteFile(pipe_handle, buffer, buffer_size, nullptr, &overlapped);
		if (!io_result)
		{
			last_error = GetLastError();
			if (last_error != ERROR_IO_PENDING)
			{
				CloseHandle(overlapped.hEvent);
				return false;
			}

			io_result = wait_for_overlapped_pipe_io(pipe_handle, overlapped, interrupt_handle, timeout_ms, bytes_transferred) != FALSE;
		}
		else
		{
			io_result = GetOverlappedResult(pipe_handle, &overlapped, &bytes_transferred, TRUE);
		}

		CloseHandle(overlapped.hEvent);
		return io_result != FALSE && bytes_transferred == buffer_size;
	}

	bool write_packet(HANDLE pipe_handle, const VstHostIpcPacket& packet, HANDLE interrupt_handle = 0, DWORD timeout_ms = kControlPipeIoTimeoutMs)
	{
		return transfer_packet(pipe_handle, const_cast<VstHostIpcPacket*>(&packet), sizeof(packet), false, interrupt_handle, timeout_ms);
	}

	bool read_packet(HANDLE pipe_handle, VstHostIpcPacket& packet, HANDLE interrupt_handle = 0, DWORD timeout_ms = kControlPipeIoTimeoutMs)
	{
		memset(&packet, 0, sizeof(packet));
		if (!transfer_packet(pipe_handle, &packet, sizeof(packet), true, interrupt_handle, timeout_ms))
			return false;

		return packet.header.magic == VST_HOST_IPC_MAGIC &&
			packet.header.version == VST_HOST_IPC_VERSION &&
			packet.header.size == sizeof(packet);
	}

	uint32_t parse_chain_kind(const std::wstring& chain_name)
	{
		if (_wcsicmp(chain_name.c_str(), L"rx") == 0)
			return VST_CHAIN_RX;
		if (_wcsicmp(chain_name.c_str(), L"tx") == 0)
			return VST_CHAIN_TX;
		return 0xffffffffu;
	}

	bool is_valid_chain_kind(uint32_t chain_kind)
	{
		return chain_kind < VST_CHAIN_COUNT;
	}

	void initialize_response(const VstHostIpcPacket& request, VstHostIpcPacket& response)
	{
		memset(&response, 0, sizeof(response));
		response.header.magic = VST_HOST_IPC_MAGIC;
		response.header.version = VST_HOST_IPC_VERSION;
		response.header.type = VST_HOST_IPC_RESPONSE;
		response.header.size = sizeof(response);
		response.header.chain_kind = request.header.chain_kind;
	}

	void handle_chain_command(const VstHostIpcPacket& request, VstHostIpcPacket& response)
	{
		VstChainKind kind = static_cast<VstChainKind>(request.header.chain_kind);
		bool bump_snapshot_generation_now = false;
		LONG expected_generation;

		initialize_response(request, response);
		if (!is_valid_chain_kind(request.header.chain_kind))
		{
			response.result = -200;
			return;
		}

		switch (request.header.type)
		{
		case VST_HOST_IPC_CREATE_CHAIN:
			response.result = VstChain_Create(g_chains[kind], request.arg0, request.arg1, request.arg2);
			bump_snapshot_generation_now = response.result == 0;
			break;
		case VST_HOST_IPC_DESTROY_CHAIN:
			VstChain_Destroy(g_chains[kind]);
			response.result = 0;
			bump_snapshot_generation_now = true;
			break;
		case VST_HOST_IPC_SET_CHAIN_BYPASS:
			VstChain_SetBypass(g_chains[kind], request.arg0);
			response.result = 0;
			bump_snapshot_generation_now = true;
			break;
		case VST_HOST_IPC_GET_CHAIN_BYPASS:
			response.result = VstChain_GetBypass(g_chains[kind]);
			break;
		case VST_HOST_IPC_SET_CHAIN_GAIN:
			VstChain_SetGain(g_chains[kind], request.double_value);
			response.result = 0;
			bump_snapshot_generation_now = true;
			break;
		case VST_HOST_IPC_GET_CHAIN_GAIN:
			response.double_value = VstChain_GetGain(g_chains[kind]);
			response.result = 0;
			break;
		case VST_HOST_IPC_GET_CHAIN_READY:
			response.result = VstChain_GetReady(g_chains[kind]);
			break;
		case VST_HOST_IPC_CLEAR_CHAIN:
			response.result = VstChain_ClearPlugins(g_chains[kind]);
			bump_snapshot_generation_now = response.result == 0;
			break;
		case VST_HOST_IPC_ADD_PLUGIN:
			response.result = VstChain_AddPlugin(g_chains[kind], request.plugin_path);
			bump_snapshot_generation_now = response.result >= 0;
			break;
		case VST_HOST_IPC_REMOVE_PLUGIN:
			response.result = VstChain_RemovePlugin(g_chains[kind], request.arg0);
			bump_snapshot_generation_now = response.result == 0;
			break;
		case VST_HOST_IPC_MOVE_PLUGIN:
			response.result = VstChain_MovePlugin(g_chains[kind], request.arg0, request.arg1);
			bump_snapshot_generation_now = response.result == 0;
			break;
		case VST_HOST_IPC_GET_CHAIN_SNAPSHOT_INFO:
			response.result = 0;
			VstChain_GetSnapshotInfo(
				g_chains[kind],
				response.chain_snapshot_info.created,
				response.chain_snapshot_info.sample_rate,
				response.chain_snapshot_info.max_block_size,
				response.chain_snapshot_info.num_channels,
				response.chain_snapshot_info.ready,
				response.chain_snapshot_info.bypass,
				response.chain_snapshot_info.gain,
				response.chain_snapshot_info.plugin_count);
			response.chain_snapshot_info.generation = current_chain_snapshot_generation(kind);
			break;
		case VST_HOST_IPC_GET_PLUGIN_COUNT:
			response.result = VstChain_GetPluginCount(g_chains[kind]);
			break;
		case VST_HOST_IPC_GET_PLUGIN_INFO:
			expected_generation = request.arg1;
			if (expected_generation >= 0 && expected_generation != current_chain_snapshot_generation(kind))
			{
				response.result = VST_HOST_IPC_RESULT_SNAPSHOT_CHANGED;
				break;
			}
			response.result = VstChain_GetPluginInfo(g_chains[kind], request.arg0, &response.plugin_info);
			break;
		case VST_HOST_IPC_SET_PLUGIN_BYPASS:
			response.result = VstChain_SetPluginBypass(g_chains[kind], request.arg0, request.arg1);
			bump_snapshot_generation_now = response.result == 0;
			break;
		case VST_HOST_IPC_SET_PLUGIN_ENABLED:
			response.result = VstChain_SetPluginEnabled(g_chains[kind], request.arg0, request.arg1);
			bump_snapshot_generation_now = response.result == 0;
			break;
		case VST_HOST_IPC_GET_PLUGIN_STATE_SIZE:
			expected_generation = request.arg1;
			if (expected_generation >= 0 && expected_generation != current_chain_snapshot_generation(kind))
			{
				response.result = VST_HOST_IPC_RESULT_SNAPSHOT_CHANGED;
				break;
			}
			response.result = VstChain_GetPluginStateSize(g_chains[kind], request.arg0);
			break;
		case VST_HOST_IPC_GET_PLUGIN_STATE:
			expected_generation = request.arg2;
			if (expected_generation >= 0 && expected_generation != current_chain_snapshot_generation(kind))
			{
				response.result = VST_HOST_IPC_RESULT_SNAPSHOT_CHANGED;
				break;
			}
			if (request.arg1 < 0 || request.arg1 > static_cast<int>(VST_HOST_IPC_MAX_STATE_BYTES))
			{
				response.result = -202;
				break;
			}
			response.result = VstChain_GetPluginState(g_chains[kind], request.arg0, response.state_data, request.arg1, &response.arg1);
			break;
		case VST_HOST_IPC_SET_PLUGIN_STATE:
			if (request.arg1 <= 0 || request.arg1 > static_cast<int>(VST_HOST_IPC_MAX_STATE_BYTES))
			{
				response.result = -203;
				break;
			}
			response.result = VstChain_SetPluginState(g_chains[kind], request.arg0, request.state_data, request.arg1);
			bump_snapshot_generation_now = response.result == 0;
			break;
		case VST_HOST_IPC_OPEN_PLUGIN_EDITOR:
		{
			std::unique_ptr<OpenEditorRequest> open_request(new OpenEditorRequest());
			OpenEditorRequest* open_request_ptr = nullptr;
			uintptr_t thread_handle = 0;
			unsigned thread_id = 0;
			DWORD wait_result;
			DWORD thread_wait_result = WAIT_TIMEOUT;

			if (!open_request)
			{
				response.result = -204;
				break;
			}

			open_request->kind = kind;
			open_request->plugin_index = request.arg0;
			open_request->completed_event = CreateEventW(nullptr, TRUE, FALSE, nullptr);
			if (!open_request->completed_event)
			{
				response.result = -206;
				break;
			}

			thread_handle = _beginthreadex(nullptr, 0, open_editor_thread_proc, open_request.get(), 0, &thread_id);
			if (!thread_handle)
			{
				CloseHandle(open_request->completed_event);
				response.result = -205;
				break;
			}

			open_request_ptr = open_request.release();
			trace_oop(
				L"[VST OOP host %s] open editor request index=%d thread=%lu\r\n",
				host_kind_name(kind),
				request.arg0,
				thread_id);
			wait_result = WaitForSingleObject(open_request_ptr->completed_event, 8000);
			if (wait_result == WAIT_OBJECT_0)
			{
				thread_wait_result = WaitForSingleObject(reinterpret_cast<HANDLE>(thread_handle), 2000);
				response.result = static_cast<int>(InterlockedCompareExchange(&open_request_ptr->result, 0, 0));
				trace_oop(
					L"[VST OOP host %s] open editor completed index=%d result=%d thread_wait=%lu\r\n",
					host_kind_name(kind),
					request.arg0,
					response.result,
					thread_wait_result);
			}
			else
			{
				response.result = -207;
				trace_oop(
					L"[VST OOP host %s] open editor timed out index=%d wait=%lu\r\n",
					host_kind_name(kind),
					request.arg0,
					wait_result);
			}

			CloseHandle(reinterpret_cast<HANDLE>(thread_handle));
			if (open_request_ptr)
			{
				if (open_request_ptr->completed_event)
					CloseHandle(open_request_ptr->completed_event);
				open_request_ptr->completed_event = 0;
				delete open_request_ptr;
			}
			break;
		}
		default:
			response.result = -201;
			break;
		}

		if (bump_snapshot_generation_now)
			bump_chain_snapshot_generation(kind);
	}

	unsigned __stdcall open_editor_thread_proc(void* context)
	{
		OpenEditorRequest* request = static_cast<OpenEditorRequest*>(context);
		VstEditorSession* session = 0;
		int width = 0;
		int height = 0;
		int can_resize = 0;
		int result;

		configure_editor_thread_priority();

		if (!request || !is_valid_chain_kind(static_cast<uint32_t>(request->kind)))
			return 1;

		result = VstChain_OpenPluginEditor(g_chains[request->kind], request->plugin_index, 0, width, height, can_resize, session);
		InterlockedExchange(&request->result, result);
		trace_oop(
			L"[VST OOP host %s] editor worker index=%d result=%d width=%d height=%d resize=%d\r\n",
			host_kind_name(request->kind),
			request->plugin_index,
			result,
			width,
			height,
			can_resize);
		if (request->completed_event)
			SetEvent(request->completed_event);
		return 0;
	}

	unsigned __stdcall audio_thread_proc(void* context)
	{
		HostAudioState* audio_state = static_cast<HostAudioState*>(context);
		LONG last_processed_sequence = 0;
		MmcssRegistration mmcss_registration = register_mmcss_pro_audio();

		configure_audio_thread_priority();

		if (!audio_state || !audio_state->shared)
			return 1;

		/*
		The audio worker drains every pending request sequence in order. It copies
		input to output first so a chain bypass or failed plugin leaves a valid dry
		block behind rather than stalling the transport.
		*/
		for (;;)
		{
			HANDLE wait_handles[2] = { audio_state->stop_event, audio_state->request_event };
			DWORD wait_result = WaitForMultipleObjects(2, wait_handles, FALSE, INFINITE);
			if (wait_result == WAIT_OBJECT_0)
				break;
			if (wait_result != WAIT_OBJECT_0 + 1)
				break;

			for (;;)
			{
				LONG request_sequence = InterlockedCompareExchange(&audio_state->shared->request_sequence, 0, 0);
				LONG next_sequence = last_processed_sequence + 1;
				LONG slot_sequence;
				int frames;
				int sample_count;
				int process_result;
				int slot;

				if (next_sequence <= 0 || next_sequence > request_sequence)
					break;

				slot = next_sequence % VST_HOST_IPC_AUDIO_SLOT_COUNT;
				slot_sequence = InterlockedCompareExchange(&audio_state->shared->input_sequence[slot], 0, 0);
				if (slot_sequence != next_sequence)
				{
					LONG oldest_retained_sequence = request_sequence - static_cast<LONG>(VST_HOST_IPC_AUDIO_SLOT_COUNT) + 1;
					if (oldest_retained_sequence < 1)
						oldest_retained_sequence = 1;

					if (next_sequence < oldest_retained_sequence)
					{
						audio_state->overrun_skipped_blocks += static_cast<ULONGLONG>(oldest_retained_sequence - next_sequence);
						last_processed_sequence = oldest_retained_sequence - 1;
						continue;
					}

					break;
				}

				frames = InterlockedCompareExchange(&audio_state->shared->input_frames[slot], 0, 0);
				if (frames <= 0 || frames > static_cast<int>(VST_HOST_IPC_AUDIO_MAX_FRAMES))
				{
					last_processed_sequence = next_sequence;
					++audio_state->invalid_blocks;
					InterlockedExchange(&audio_state->shared->last_process_result, -10);
					InterlockedExchange(&audio_state->shared->output_frames[slot], 0);
					InterlockedExchange(&audio_state->shared->processed_sequence[slot], next_sequence);
					continue;
				}

				sample_count = frames * static_cast<int>(VST_HOST_IPC_AUDIO_MAX_CHANNELS);
				memcpy(audio_state->shared->output_buffer[slot], audio_state->shared->input_buffer[slot], sample_count * sizeof(double));
				LARGE_INTEGER process_begin = {};
				LARGE_INTEGER process_end = {};
				QueryPerformanceCounter(&process_begin);
				process_result = VstChain_ProcessInterleavedDouble(g_chains[audio_state->chain_kind], audio_state->shared->output_buffer[slot], frames);
				QueryPerformanceCounter(&process_end);
				{
					ULONGLONG process_ticks = static_cast<ULONGLONG>(process_end.QuadPart - process_begin.QuadPart);
					audio_state->total_process_ticks += process_ticks;
					if (process_ticks > audio_state->max_process_ticks)
						audio_state->max_process_ticks = process_ticks;
				}
				++audio_state->blocks_processed;
				InterlockedExchange(&audio_state->shared->last_process_result, process_result);
				InterlockedExchange(&audio_state->shared->output_frames[slot], frames);
				InterlockedExchange(&audio_state->shared->processed_sequence[slot], next_sequence);
				last_processed_sequence = next_sequence;

				if (out_of_process_trace_enabled())
				{
					DWORD now = GetTickCount();
					if (audio_state->last_log_tick == 0)
						audio_state->last_log_tick = now;
					if (now - audio_state->last_log_tick >= 2000 && audio_state->qpc_frequency.QuadPart > 0)
					{
						double avg_us = 0.0;
						double max_us = 0.0;
						if (audio_state->blocks_processed > 0)
						{
							avg_us = (static_cast<double>(audio_state->total_process_ticks) * 1000000.0) /
								(static_cast<double>(audio_state->qpc_frequency.QuadPart) * static_cast<double>(audio_state->blocks_processed));
							max_us = (static_cast<double>(audio_state->max_process_ticks) * 1000000.0) /
								static_cast<double>(audio_state->qpc_frequency.QuadPart);
						}

						trace_oop(
							L"[VST OOP host %s] processed=%llu invalid=%llu skipped=%llu avg_us=%.1f max_us=%.1f last_seq=%ld last_result=%ld\r\n",
							audio_state->chain_kind == VST_CHAIN_RX ? L"rx" : L"tx",
							audio_state->blocks_processed,
							audio_state->invalid_blocks,
							audio_state->overrun_skipped_blocks,
							avg_us,
							max_us,
							last_processed_sequence,
							InterlockedCompareExchange(&audio_state->shared->last_process_result, 0, 0));
						audio_state->last_log_tick = now;
					}
				}
			}
		}

		unregister_mmcss(mmcss_registration);
		return 0;
	}
}

int RunVstAudioHost(HINSTANCE instance, PWSTR command_line)
{
	std::wstring ready_event_name;
	std::wstring shutdown_event_name;
	std::wstring control_pipe_name;
	std::wstring audio_mapping_name;
	std::wstring audio_request_event_name;
	std::wstring state_dirty_event_name;
	std::wstring chain_name;
	HANDLE ready_event = 0;
	HANDLE shutdown_event = 0;
	HANDLE control_pipe = INVALID_HANDLE_VALUE;
	HostAudioState audio_state = {};
	uint32_t chain_kind = 0xffffffffu;
	int exit_code = 0;

	(void)instance;
	configure_host_process_priorities();

	for (int i = 0; i < VST_CHAIN_COUNT; ++i)
		VstChain_Initialize(g_chains[i]);

	if (!try_get_argument_value(command_line, L"--ready-event", ready_event_name) ||
		!try_get_argument_value(command_line, L"--shutdown-event", shutdown_event_name) ||
		!try_get_argument_value(command_line, L"--control-pipe", control_pipe_name) ||
		!try_get_argument_value(command_line, L"--audio-mapping", audio_mapping_name) ||
		!try_get_argument_value(command_line, L"--audio-request-event", audio_request_event_name) ||
		!try_get_argument_value(command_line, L"--state-dirty-event", state_dirty_event_name) ||
		!try_get_argument_value(command_line, L"--chain", chain_name))
	{
		exit_code = 2;
		goto cleanup;
	}

	chain_kind = parse_chain_kind(chain_name);
	if (!is_valid_chain_kind(chain_kind))
	{
		exit_code = 7;
		goto cleanup;
	}
	audio_state.chain_kind = static_cast<VstChainKind>(chain_kind);
	QueryPerformanceFrequency(&audio_state.qpc_frequency);

	ready_event = OpenEventW(EVENT_MODIFY_STATE, FALSE, ready_event_name.c_str());
	if (!ready_event)
	{
		exit_code = 3;
		goto cleanup;
	}

	shutdown_event = OpenEventW(SYNCHRONIZE, FALSE, shutdown_event_name.c_str());
	if (!shutdown_event)
	{
		exit_code = 4;
		goto cleanup;
	}

	control_pipe = CreateNamedPipeW(
		control_pipe_name.c_str(),
		PIPE_ACCESS_DUPLEX | FILE_FLAG_OVERLAPPED,
		PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT,
		1,
		sizeof(VstHostIpcPacket),
		sizeof(VstHostIpcPacket),
		0,
		0);
	if (control_pipe == INVALID_HANDLE_VALUE)
	{
		exit_code = 8;
		goto cleanup;
	}

	audio_state.mapping_handle = OpenFileMappingW(FILE_MAP_ALL_ACCESS, FALSE, audio_mapping_name.c_str());
	if (!audio_state.mapping_handle)
	{
		exit_code = 14;
		goto cleanup;
	}

	audio_state.shared = static_cast<VstHostAudioSharedBlock*>(MapViewOfFile(
		audio_state.mapping_handle,
		FILE_MAP_ALL_ACCESS,
		0,
		0,
		sizeof(VstHostAudioSharedBlock)));
	if (!audio_state.shared)
	{
		exit_code = 15;
		goto cleanup;
	}

	audio_state.request_event = OpenEventW(SYNCHRONIZE, FALSE, audio_request_event_name.c_str());
	if (!audio_state.request_event)
	{
		exit_code = 16;
		goto cleanup;
	}

	audio_state.state_dirty_event = OpenEventW(EVENT_MODIFY_STATE, FALSE, state_dirty_event_name.c_str());
	if (!audio_state.state_dirty_event)
	{
		exit_code = 19;
		goto cleanup;
	}

	audio_state.stop_event = CreateEventW(0, TRUE, FALSE, 0);
	if (!audio_state.stop_event)
	{
		exit_code = 17;
		goto cleanup;
	}

	audio_state.thread_handle = reinterpret_cast<HANDLE>(_beginthreadex(0, 0, audio_thread_proc, &audio_state, 0, 0));
	if (!audio_state.thread_handle)
	{
		exit_code = 18;
		goto cleanup;
	}

	VstChain_SetStateDirtyCallback(g_chains[chain_kind], on_chain_state_dirty, &audio_state);

	if (!SetEvent(ready_event))
	{
		exit_code = 5;
		goto cleanup;
	}

	{
		OVERLAPPED connect_overlapped = {};
		BOOL connected;

		connect_overlapped.hEvent = CreateEventW(nullptr, TRUE, FALSE, nullptr);
		if (!connect_overlapped.hEvent)
		{
			exit_code = 9;
			goto cleanup;
		}

		connected = ConnectNamedPipe(control_pipe, &connect_overlapped)
			? TRUE
			: (GetLastError() == ERROR_PIPE_CONNECTED);
		if (!connected)
		{
			DWORD last_error = GetLastError();
			DWORD bytes_transferred = 0;
			if (last_error != ERROR_IO_PENDING ||
				!wait_for_overlapped_pipe_io(control_pipe, connect_overlapped, shutdown_event, kControlPipeIoTimeoutMs, bytes_transferred))
			{
				CloseHandle(connect_overlapped.hEvent);
				exit_code = 9;
				goto cleanup;
			}
		}

		CloseHandle(connect_overlapped.hEvent);
	}

	{
		VstHostIpcPacket hello = {};

		hello.header.magic = VST_HOST_IPC_MAGIC;
		hello.header.version = VST_HOST_IPC_VERSION;
		hello.header.type = VST_HOST_IPC_HELLO;
		hello.header.size = sizeof(hello);
		hello.header.chain_kind = chain_kind;

		if (!write_packet(control_pipe, hello, shutdown_event))
		{
			exit_code = 10;
			goto cleanup;
		}
	}

	for (;;)
	{
		VstHostIpcPacket request = {};
		VstHostIpcPacket response = {};
		bool fatal_runtime_failure = false;

		if (!read_packet(control_pipe, request, shutdown_event, INFINITE))
		{
			DWORD last_error = GetLastError();
			if (last_error == ERROR_OPERATION_ABORTED)
				break;
			exit_code = 11;
			break;
		}

		switch (request.header.type)
		{
		case VST_HOST_IPC_PING:
			initialize_response(request, response);
			response.header.type = VST_HOST_IPC_PONG;
			response.result = 0;
			break;
		case VST_HOST_IPC_SHUTDOWN:
			initialize_response(request, response);
			response.result = 0;
			if (!write_packet(control_pipe, response, shutdown_event))
				exit_code = 12;
			goto cleanup;
		default:
			handle_chain_command(request, response);
			fatal_runtime_failure = response.result == VST_HOST_IPC_RESULT_CONTROL_TIMEOUT ||
				response.result == VST_HOST_IPC_RESULT_RUNTIME_THREAD_EXITED;
			break;
		}

		if (!write_packet(control_pipe, response, shutdown_event))
		{
			exit_code = 12;
			break;
		}
		if (fatal_runtime_failure)
		{
			exit_code = 20;
			break;
		}
	}

cleanup:
	if (chain_kind < VST_CHAIN_COUNT)
		VstChain_SetStateDirtyCallback(g_chains[chain_kind], nullptr, nullptr);
	if (audio_state.stop_event)
		SetEvent(audio_state.stop_event);
	if (audio_state.thread_handle)
	{
		WaitForSingleObject(audio_state.thread_handle, 2000);
		CloseHandle(audio_state.thread_handle);
	}
	if (audio_state.stop_event)
		CloseHandle(audio_state.stop_event);
	if (audio_state.request_event)
		CloseHandle(audio_state.request_event);
	if (audio_state.state_dirty_event)
		CloseHandle(audio_state.state_dirty_event);
	if (audio_state.shared)
		UnmapViewOfFile(audio_state.shared);
	if (audio_state.mapping_handle)
		CloseHandle(audio_state.mapping_handle);
	if (control_pipe != INVALID_HANDLE_VALUE)
	{
		FlushFileBuffers(control_pipe);
		DisconnectNamedPipe(control_pipe);
		CloseHandle(control_pipe);
	}
	if (shutdown_event)
		CloseHandle(shutdown_event);
	if (ready_event)
		CloseHandle(ready_event);

	for (int i = 0; i < VST_CHAIN_COUNT; ++i)
		VstChain_Terminate(g_chains[i]);

	return exit_code;
}
