#include "vst_runtime.h"
#include "vst_chain.h"
#include "vst_plugin_runtime.h"
#include "vst_scoped_lock.h"
#include "../VstCommon/vst_ipc.h"

#include "public.sdk/source/vst/hosting/hostclasses.h"
#include "public.sdk/source/vst/hosting/module.h"
#include "public.sdk/source/vst/hosting/parameterchanges.h"
#include "public.sdk/source/vst/hosting/processdata.h"
#include "public.sdk/source/common/memorystream.h"
#include "pluginterfaces/base/funknown.h"
#include "pluginterfaces/base/funknownimpl.h"
#include "pluginterfaces/gui/iplugview.h"
#include "pluginterfaces/base/ipluginbase.h"
#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include "pluginterfaces/vst/ivstcomponent.h"
#include "pluginterfaces/vst/ivstprocesscontext.h"
#include "pluginterfaces/vst/ivsteditcontroller.h"
#include "pluginterfaces/vst/ivstmessage.h"
#include "pluginterfaces/vst/vstspeaker.h"
#include <Windows.h>

#include <algorithm>
#include <cstdint>
#include <cstdarg>
#include <cstring>
#include <memory>
#include <process.h>
#include <string>
#include <utility>
#include <vector>

/*
VST3 runtime wrapper.

Single-instance model: the live audio runtime and editor share one
component/controller pair.  Thread safety between the audio thread
and the editor is handled by a try-lock in VstRuntime_Process.
*/

const int kMaxOutputParamCacheEntries = 128;
const DWORD kOutputParamFlushIntervalMs = 33;  // ~30 Hz

struct OutputParamCacheEntry
{
	volatile LONG active;  // 1 if this slot has a pending value
	Steinberg::Vst::ParamID id;
	double value;
};

struct VstPluginRuntime
{
	volatile LONG ref_count = 1;
	enum HostCommandType
	{
		HostCommandNone = 0,
		HostCommandInitialize,
		HostCommandReconfigure,
		HostCommandOpenEditor,
		HostCommandCloseEditor,
		HostCommandCloseOpenEditor,
		HostCommandResizeEditor,
		HostCommandGetStateSize,
		HostCommandGetState,
		HostCommandSetState,
		HostCommandShutdown
	};

	std::wstring plugin_path;
	std::shared_ptr<VST3::Hosting::Module> module;
	VST3::Hosting::ClassInfo class_info;
	std::shared_ptr<Steinberg::Vst::HostApplication> host_application;
	Steinberg::IPtr<Steinberg::Vst::IComponent> component;
	Steinberg::IPtr<Steinberg::Vst::IEditController> controller;
	Steinberg::IPtr<Steinberg::Vst::IAudioProcessor> processor;
	Steinberg::Vst::HostProcessData process_data;
	Steinberg::Vst::ProcessContext process_context {};
	Steinberg::Vst::ParameterChanges input_parameter_changes;
	Steinberg::Vst::ParameterChanges output_parameter_changes;
	Steinberg::Vst::ParameterChangeTransfer input_parameter_transfer;
	std::vector<std::vector<double>> input_buffers_64;
	std::vector<std::vector<double>> output_buffers_64;
	std::vector<std::vector<float>> input_buffers_32;
	std::vector<std::vector<float>> output_buffers_32;
	std::vector<Steinberg::Vst::Sample64*> input_ptrs_64;
	std::vector<Steinberg::Vst::Sample64*> output_ptrs_64;
	std::vector<Steinberg::Vst::Sample32*> input_ptrs_32;
	std::vector<Steinberg::Vst::Sample32*> output_ptrs_32;
	int sample_rate = 0;
	int max_block_size = 0;
	int chain_channels = 0;
	int input_channels = 0;
	int output_channels = 0;
	int symbolic_sample_size = Steinberg::Vst::kSample64;
	bool use_in_place_buffers = false;
	bool owns_component = true;
	bool controller_is_component = false;
	bool initialized = false;
	bool active = false;
	HANDLE host_thread = 0;
	HANDLE host_ready_event = 0;
	HANDLE command_event = 0;
	HANDLE command_complete_event = 0;
	CRITICAL_SECTION command_lock = {};
	CRITICAL_SECTION api_lock = {};
	DWORD host_thread_id = 0;
	HostCommandType command_type = HostCommandNone;
	int command_result = 0;
	const wchar_t* command_plugin_path = 0;
	wchar_t* command_plugin_name = 0;
	size_t command_plugin_name_count = 0;
	int command_sample_rate = 0;
	int command_max_block_size = 0;
	int command_num_channels = 0;
	VstProcessingState* command_owner_state = 0;
	int command_plugin_index = 0;
	VstEditorSession* command_session = 0;
	int command_width = 0;
	int command_height = 0;
	int command_can_resize = 0;
	void* command_state_buffer = 0;
	int command_state_buffer_size = 0;
	int* command_bytes_written = 0;
	void* component_handler = nullptr;
	void* component_connection_proxy = nullptr;
	void* controller_connection_proxy = nullptr;
	void* editor_session = nullptr;
	VstRuntimeStateDirtyCallback state_dirty_callback = nullptr;
	void* state_dirty_context = nullptr;
	volatile LONG host_faulted = 0;
	OutputParamCacheEntry output_param_cache[kMaxOutputParamCacheEntries] = {};
	volatile LONG output_param_cache_count = 0;
};

struct VstEditorSession
{
	VstPluginRuntime* runtime = nullptr;
	VstPluginRuntime* live_runtime = nullptr;
	VstProcessingState* owner_state = nullptr;
	int plugin_index = -1;
	HWND window = 0;
	bool can_resize = false;
	int requested_width = 0;
	int requested_height = 0;
	Steinberg::IPtr<Steinberg::IPlugView> view;
	void* frame = nullptr;
};

namespace
{
	SRWLOCK g_editor_log_lock = SRWLOCK_INIT;
	INIT_ONCE g_editor_window_class_once = INIT_ONCE_STATIC_INIT;
	const wchar_t* g_editor_window_class_name = L"ThetisVstEditorHostWindow";
	LONG g_editor_trace_enabled = -1;
	const DWORD kRuntimeHostCommandTimeoutMs = 5000;
	const DWORD kRuntimeHostShutdownTimeoutMs = 2000;

	// ScopedRuntimeApiLock and ScopedTryRuntimeApiLock are defined in
	// vst_scoped_lock.h and shared with vst2_runtime.cpp.

	void trace_editor(const char* format, ...);

	bool editor_trace_enabled()
	{
		LONG enabled = InterlockedCompareExchange(&g_editor_trace_enabled, -1, -1);
		wchar_t value[8] = {};

		if (enabled != -1)
			return enabled != 0;

		enabled = IsDebuggerPresent() ? 1 : 0;
		if (GetEnvironmentVariableW(L"THETIS_VST_EDITOR_TRACE", value, _countof(value)) > 0)
			enabled = 1;
		InterlockedExchange(&g_editor_trace_enabled, enabled);
		return enabled != 0;
	}

	using Steinberg::FUnknown;
	using Steinberg::IPluginBase;
	using Steinberg::IPtr;
	using Steinberg::MemoryStream;
	using Steinberg::int32;
	using Steinberg::int64;
	using Steinberg::tresult;
	using Steinberg::uint32;
	using Steinberg::IPlugFrame;
	using Steinberg::IPlugView;
	using Steinberg::ViewRect;
	using Steinberg::kResultOk;
	using Steinberg::kResultTrue;
	using Steinberg::Vst::BusInfo;
	using Steinberg::Vst::HostApplication;
	using Steinberg::Vst::IAudioProcessor;
	using Steinberg::Vst::IComponent;
	using Steinberg::Vst::IComponentHandler;
	using Steinberg::Vst::IComponentHandler2;
	using Steinberg::Vst::IConnectionPoint;
	using Steinberg::Vst::IEditController;
	using Steinberg::Vst::kAudio;
	using Steinberg::Vst::kInput;
	using Steinberg::Vst::kOutput;
	using Steinberg::Vst::kRealtime;
	using Steinberg::Vst::kSample32;
	using Steinberg::Vst::kSample64;
	using Steinberg::Vst::kSimple;
	using Steinberg::Vst::ParamID;
	using Steinberg::Vst::ParamValue;
	using Steinberg::Vst::ParameterChanges;
	using Steinberg::Vst::ParameterChangeTransfer;
	using Steinberg::Vst::ProcessSetup;
	using Steinberg::Vst::SpeakerArrangement;

	bool is_ok(Steinberg::tresult result);
	void notify_runtime_state_dirty(VstPluginRuntime& runtime);
	void release_runtime(VstPluginRuntime& runtime);
	int configure_runtime(VstPluginRuntime& runtime, int sample_rate, int max_block_size, int num_channels);
	bool load_runtime_instance(
		VstPluginRuntime& runtime,
		const wchar_t* plugin_path,
		wchar_t* plugin_name,
		size_t plugin_name_count,
		ParameterChangeTransfer* transfer_target = nullptr);
	int resize_editor_session(VstEditorSession& session, int width, int height);
	void detach_editor_session_view(VstEditorSession* session);
	int capture_runtime_state_size(VstPluginRuntime& runtime);
	int capture_runtime_state(VstPluginRuntime& runtime, void* buffer, int buffer_size, int* bytes_written);
	int apply_runtime_state(VstPluginRuntime& runtime, const void* buffer, int buffer_size);
	bool capture_runtime_state_blob_locked(VstPluginRuntime& runtime, std::vector<unsigned char>& state_blob);
	bool create_editor_runtime_instance(const VstPluginRuntime& source, VstPluginRuntime*& runtime);
	void destroy_editor_runtime_instance(VstPluginRuntime*& runtime);
	void sync_editor_runtime_state_to_live_runtime(void* context);
	void destroy_editor_session(VstEditorSession*& session, bool release_owner_state);
	LRESULT CALLBACK editor_window_proc(HWND hwnd, UINT message, WPARAM w_param, LPARAM l_param);
	BOOL CALLBACK ensure_editor_window_class(PINIT_ONCE, PVOID, PVOID*);
	int process_host_command(VstPluginRuntime& runtime);
	unsigned __stdcall runtime_host_thread_proc(void* context);
	int send_host_command(VstPluginRuntime& runtime);
	unsigned __stdcall deferred_owner_state_release_proc(void* context);
	void release_owner_state_safe(VstPluginRuntime* runtime, VstProcessingState* owner_state);

	class RuntimeConnectionProxy : public IConnectionPoint
	{
	public:
		explicit RuntimeConnectionProxy(IConnectionPoint* source)
		: source_connection(source)
		{
		}

		tresult PLUGIN_API connect(IConnectionPoint* other) SMTG_OVERRIDE
		{
			if (!other)
				return Steinberg::kInvalidArgument;
			if (destination_connection)
				return Steinberg::kResultFalse;

			destination_connection = other;
			tresult result = source_connection ? source_connection->connect(this) : Steinberg::kResultFalse;
			if (result != kResultTrue)
				destination_connection = nullptr;
			return result;
		}

		tresult PLUGIN_API disconnect(IConnectionPoint* other) SMTG_OVERRIDE
		{
			if (!other)
				return Steinberg::kInvalidArgument;
			if (other != destination_connection)
				return Steinberg::kInvalidArgument;

			if (source_connection)
				source_connection->disconnect(this);
			destination_connection = nullptr;
			return kResultTrue;
		}

		tresult PLUGIN_API notify(Steinberg::Vst::IMessage* message) SMTG_OVERRIDE
		{
			return destination_connection ? destination_connection->notify(message) : Steinberg::kResultFalse;
		}

		bool disconnect()
		{
			return disconnect(destination_connection) == kResultTrue;
		}

		tresult PLUGIN_API queryInterface(const Steinberg::TUID iid, void** obj) SMTG_OVERRIDE
		{
			if (!obj)
				return Steinberg::kInvalidArgument;

			QUERY_INTERFACE(iid, obj, FUnknown::iid, IConnectionPoint)
			QUERY_INTERFACE(iid, obj, IConnectionPoint::iid, IConnectionPoint)
			*obj = nullptr;
			return Steinberg::kNoInterface;
		}

		uint32 PLUGIN_API addRef() SMTG_OVERRIDE { return 1; }
		uint32 PLUGIN_API release() SMTG_OVERRIDE { return 1; }

	private:
		IPtr<IConnectionPoint> source_connection;
		IPtr<IConnectionPoint> destination_connection;
	};

	class RuntimeComponentHandler : public IComponentHandler, public IComponentHandler2
	{
	public:
		RuntimeComponentHandler(VstPluginRuntime& runtime_ref, ParameterChangeTransfer& transfer_ref)
		: runtime(runtime_ref), transfer(transfer_ref)
		{
		}

		tresult PLUGIN_API beginEdit(ParamID /*id*/) SMTG_OVERRIDE { return kResultTrue; }
		tresult PLUGIN_API endEdit(ParamID /*id*/) SMTG_OVERRIDE { return kResultTrue; }

		tresult PLUGIN_API performEdit(ParamID id, ParamValue valueNormalized) SMTG_OVERRIDE
		{
			transfer.addChange(id, valueNormalized, 0);
			notify_runtime_state_dirty(runtime);
			return kResultTrue;
		}

		tresult PLUGIN_API restartComponent(int32 flags) SMTG_OVERRIDE
		{
			if (flags & Steinberg::Vst::kParamValuesChanged)
				notify_runtime_state_dirty(runtime);
			return kResultTrue;
		}

		tresult PLUGIN_API setDirty(Steinberg::TBool state) SMTG_OVERRIDE
		{
			if (state)
				notify_runtime_state_dirty(runtime);
			return kResultTrue;
		}

		tresult PLUGIN_API requestOpenEditor(Steinberg::FIDString /*name*/) SMTG_OVERRIDE
		{
			return Steinberg::kResultFalse;
		}

		tresult PLUGIN_API startGroupEdit() SMTG_OVERRIDE { return kResultTrue; }
		tresult PLUGIN_API finishGroupEdit() SMTG_OVERRIDE { return kResultTrue; }

		tresult PLUGIN_API queryInterface(const Steinberg::TUID iid, void** obj) SMTG_OVERRIDE
		{
			if (!obj)
				return Steinberg::kInvalidArgument;

			QUERY_INTERFACE(iid, obj, FUnknown::iid, IComponentHandler)
			QUERY_INTERFACE(iid, obj, IComponentHandler::iid, IComponentHandler)
			QUERY_INTERFACE(iid, obj, IComponentHandler2::iid, IComponentHandler2)
			*obj = nullptr;
			return Steinberg::kNoInterface;
		}

		uint32 PLUGIN_API addRef() SMTG_OVERRIDE { return 1; }
		uint32 PLUGIN_API release() SMTG_OVERRIDE { return 1; }

	private:
		VstPluginRuntime& runtime;
		ParameterChangeTransfer& transfer;
	};

	class RuntimePlugFrame : public IPlugFrame
	{
	public:
		explicit RuntimePlugFrame(VstEditorSession& session_ref)
		: session(session_ref)
		{
		}

		tresult PLUGIN_API resizeView(IPlugView* view, ViewRect* newSize) SMTG_OVERRIDE
		{
			int width;
			int height;

			if (!view || !newSize)
				return Steinberg::kInvalidArgument;

			width = newSize->right - newSize->left;
			height = newSize->bottom - newSize->top;
			if (width <= 0 || height <= 0)
				return Steinberg::kInvalidArgument;

			trace_editor("resizeView requested %dx%d", width, height);
			session.requested_width = width;
			session.requested_height = height;

			return view->onSize(newSize);
		}

		tresult PLUGIN_API queryInterface(const Steinberg::TUID iid, void** obj) SMTG_OVERRIDE
		{
			if (!obj)
				return Steinberg::kInvalidArgument;

			QUERY_INTERFACE(iid, obj, FUnknown::iid, IPlugFrame)
			QUERY_INTERFACE(iid, obj, IPlugFrame::iid, IPlugFrame)
			*obj = nullptr;
			return Steinberg::kNoInterface;
		}

		uint32 PLUGIN_API addRef() SMTG_OVERRIDE { return 1; }
		uint32 PLUGIN_API release() SMTG_OVERRIDE { return 1; }

	private:
		VstEditorSession& session;
	};

	bool is_ok(Steinberg::tresult result)
	{
		return result == kResultOk || result == kResultTrue;
	}

	void notify_runtime_state_dirty(VstPluginRuntime& runtime)
	{
		VstRuntimeStateDirtyCallback callback = runtime.state_dirty_callback;
		if (callback)
			callback(runtime.state_dirty_context);
	}

	void trace_editor(const char* format, ...)
	{
		char message[1024];
		char line[1200];
		wchar_t temp_path[MAX_PATH];
		wchar_t log_path[MAX_PATH];
		DWORD written = 0;
		HANDLE file_handle;
		SYSTEMTIME local_time;
		int prefix_length;
		va_list args;

		if (!editor_trace_enabled())
			return;

		GetLocalTime(&local_time);
		prefix_length = _snprintf_s(
			line,
			_countof(line),
			_TRUNCATE,
			"[VST editor %02u:%02u:%02u.%03u tid=%lu] ",
			static_cast<unsigned>(local_time.wHour),
			static_cast<unsigned>(local_time.wMinute),
			static_cast<unsigned>(local_time.wSecond),
			static_cast<unsigned>(local_time.wMilliseconds),
			static_cast<unsigned long>(GetCurrentThreadId()));
		if (prefix_length < 0)
			prefix_length = 0;

		va_start(args, format);
		_vsnprintf_s(message, _countof(message), _TRUNCATE, format, args);
		va_end(args);
		_snprintf_s(
			line + prefix_length,
			_countof(line) - static_cast<size_t>(prefix_length),
			_TRUNCATE,
			"%s\r\n",
			message);

		OutputDebugStringA(line);

		AcquireSRWLockExclusive(&g_editor_log_lock);
		if (GetTempPathW(_countof(temp_path), temp_path) > 0)
		{
			wcscpy_s(log_path, temp_path);
			wcscat_s(log_path, L"ThetisVstEditor.log");
			file_handle = CreateFileW(log_path, FILE_APPEND_DATA, FILE_SHARE_READ, nullptr, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
			if (file_handle != INVALID_HANDLE_VALUE)
			{
				WriteFile(file_handle, line, static_cast<DWORD>(strlen(line)), &written, nullptr);
				CloseHandle(file_handle);
			}
		}
		ReleaseSRWLockExclusive(&g_editor_log_lock);
	}

	enum
	{
		kStateBlobMagic = 0x31535654u,
		kStateBlobHeaderSize = 12
	};

	uint32_t read_u32(const unsigned char* data)
	{
		return static_cast<uint32_t>(data[0]) |
			(static_cast<uint32_t>(data[1]) << 8) |
			(static_cast<uint32_t>(data[2]) << 16) |
			(static_cast<uint32_t>(data[3]) << 24);
	}

	void write_u32(unsigned char* data, uint32_t value)
	{
		data[0] = static_cast<unsigned char>(value & 0xffu);
		data[1] = static_cast<unsigned char>((value >> 8) & 0xffu);
		data[2] = static_cast<unsigned char>((value >> 16) & 0xffu);
		data[3] = static_cast<unsigned char>((value >> 24) & 0xffu);
	}

	BOOL CALLBACK ensure_editor_window_class(PINIT_ONCE, PVOID, PVOID*)
	{
		WNDCLASSEXW window_class = {};

		window_class.cbSize = sizeof(window_class);
		window_class.lpfnWndProc = editor_window_proc;
		window_class.hInstance = GetModuleHandleW(nullptr);
		window_class.hCursor = LoadCursorW(nullptr, IDC_ARROW);
		window_class.hbrBackground = reinterpret_cast<HBRUSH>(COLOR_WINDOW + 1);
		window_class.lpszClassName = g_editor_window_class_name;
		window_class.style = CS_DBLCLKS;
		return RegisterClassExW(&window_class) != 0;
	}

	int send_host_command(VstPluginRuntime& runtime)
	{
		HANDLE wait_handles[2];
		DWORD wait_result;
		DWORD timeout_ms;

		if (!runtime.host_thread || !runtime.command_event || !runtime.command_complete_event)
			return -100;
		if (InterlockedCompareExchange(&runtime.host_faulted, 0, 0) != 0)
			return VST_HOST_IPC_RESULT_RUNTIME_THREAD_EXITED;
		if (runtime.host_thread_id != 0 && GetCurrentThreadId() == runtime.host_thread_id)
		{
			trace_editor("processing host command inline type=%d", static_cast<int>(runtime.command_type));
			return process_host_command(runtime);
		}

		wait_handles[0] = runtime.command_complete_event;
		wait_handles[1] = runtime.host_thread;
		timeout_ms = runtime.command_type == VstPluginRuntime::HostCommandShutdown
			? kRuntimeHostShutdownTimeoutMs
			: kRuntimeHostCommandTimeoutMs;
		trace_editor("send host command begin type=%d runtime=%p", static_cast<int>(runtime.command_type), &runtime);
		SetEvent(runtime.command_event);
		wait_result = WaitForMultipleObjects(2, wait_handles, FALSE, timeout_ms);
		if (wait_result == WAIT_OBJECT_0)
		{
			trace_editor("send host command complete type=%d result=%d", static_cast<int>(runtime.command_type), runtime.command_result);
			return runtime.command_result;
		}
		if (wait_result == WAIT_OBJECT_0 + 1)
		{
			InterlockedExchange(&runtime.host_faulted, 1);
			trace_editor("host thread exited during command type=%d", static_cast<int>(runtime.command_type));
			return VST_HOST_IPC_RESULT_RUNTIME_THREAD_EXITED;
		}
		if (wait_result == WAIT_TIMEOUT)
		{
			InterlockedExchange(&runtime.host_faulted, 1);
			trace_editor("host command timed out type=%d timeout_ms=%lu", static_cast<int>(runtime.command_type), static_cast<unsigned long>(timeout_ms));
			return VST_HOST_IPC_RESULT_CONTROL_TIMEOUT;
		}

		InterlockedExchange(&runtime.host_faulted, 1);
		trace_editor("host command wait failed result=%lu type=%d", static_cast<unsigned long>(wait_result), static_cast<int>(runtime.command_type));
		return VST_HOST_IPC_RESULT_RUNTIME_THREAD_EXITED;
	}

	unsigned __stdcall deferred_owner_state_release_proc(void* context)
	{
		VstProcessingState* owner_state = static_cast<VstProcessingState*>(context);
		if (owner_state)
			VstProcessingState_Release(owner_state);
		return 0;
	}

	void release_owner_state_safe(VstPluginRuntime* runtime, VstProcessingState* owner_state)
	{
		uintptr_t release_thread = 0;

		if (!owner_state)
			return;

		if (runtime && runtime->host_thread_id != 0 && GetCurrentThreadId() == runtime->host_thread_id)
		{
			trace_editor("deferring owner state release off host thread runtime=%p state=%p", runtime, owner_state);
			release_thread = _beginthreadex(nullptr, 0, deferred_owner_state_release_proc, owner_state, 0, nullptr);
			if (release_thread != 0)
			{
				CloseHandle(reinterpret_cast<HANDLE>(release_thread));
				return;
			}

			trace_editor("failed to create deferred owner state release thread; falling back inline");
		}

		VstProcessingState_Release(owner_state);
	}

	int capture_runtime_state_size(VstPluginRuntime& runtime)
	{
		MemoryStream component_state_stream;
		MemoryStream controller_state_stream;
		int component_size = 0;
		int controller_size = 0;

		if (runtime.component && is_ok(runtime.component->getState(&component_state_stream)))
			component_size = static_cast<int>(component_state_stream.getSize());
		if (runtime.controller && !runtime.controller_is_component &&
			is_ok(runtime.controller->getState(&controller_state_stream)))
			controller_size = static_cast<int>(controller_state_stream.getSize());
		if (component_size <= 0 && controller_size <= 0)
			return 0;

		return kStateBlobHeaderSize + component_size + controller_size;
	}

	int capture_runtime_state(VstPluginRuntime& runtime, void* buffer, int buffer_size, int* bytes_written)
	{
		MemoryStream component_state_stream;
		MemoryStream controller_state_stream;
		int component_size = 0;
		int controller_size = 0;
		int total_size;
		unsigned char* output = static_cast<unsigned char*>(buffer);

		if (bytes_written)
			*bytes_written = 0;
		if (!buffer || buffer_size < kStateBlobHeaderSize)
			return -1;

		if (runtime.component && is_ok(runtime.component->getState(&component_state_stream)))
			component_size = static_cast<int>(component_state_stream.getSize());
		if (runtime.controller && !runtime.controller_is_component &&
			is_ok(runtime.controller->getState(&controller_state_stream)))
			controller_size = static_cast<int>(controller_state_stream.getSize());
		if (component_size <= 0 && controller_size <= 0)
			return -2;

		total_size = kStateBlobHeaderSize + component_size + controller_size;
		if (buffer_size < total_size)
			return -3;

		write_u32(output + 0, kStateBlobMagic);
		write_u32(output + 4, static_cast<uint32_t>(component_size));
		write_u32(output + 8, static_cast<uint32_t>(controller_size));
		if (component_size > 0)
			memcpy_s(output + kStateBlobHeaderSize, static_cast<size_t>(buffer_size - kStateBlobHeaderSize),
				component_state_stream.getData(), static_cast<size_t>(component_size));
		if (controller_size > 0)
			memcpy_s(output + kStateBlobHeaderSize + component_size,
				static_cast<size_t>(buffer_size - (kStateBlobHeaderSize + component_size)),
				controller_state_stream.getData(),
				static_cast<size_t>(controller_size));
		if (bytes_written)
			*bytes_written = total_size;
		return 0;
	}

	int apply_runtime_state(VstPluginRuntime& runtime, const void* buffer, int buffer_size)
	{
		const unsigned char* input = static_cast<const unsigned char*>(buffer);
		uint32_t magic;
		uint32_t component_size;
		uint32_t controller_size;
		const char* component_data;
		const char* controller_data;

		if (!buffer || buffer_size < kStateBlobHeaderSize)
			return -1;

		magic = read_u32(input + 0);
		component_size = read_u32(input + 4);
		controller_size = read_u32(input + 8);
		if (magic != kStateBlobMagic)
			return -2;
		if (buffer_size < static_cast<int>(kStateBlobHeaderSize + component_size + controller_size))
			return -3;

		component_data = reinterpret_cast<const char*>(input + kStateBlobHeaderSize);
		controller_data = reinterpret_cast<const char*>(input + kStateBlobHeaderSize + component_size);

		if (component_size > 0)
		{
			MemoryStream component_stream(const_cast<char*>(component_data), component_size);
			if (runtime.component && !is_ok(runtime.component->setState(&component_stream)))
				return -4;
			if (runtime.controller && !runtime.controller_is_component)
			{
				MemoryStream controller_component_stream(const_cast<char*>(component_data), component_size);
				// setComponentState may fail for controllers that don't
				// implement it; treat as non-fatal but log.
				if (!is_ok(runtime.controller->setComponentState(&controller_component_stream)))
					trace_editor("setComponentState returned failure (non-fatal)");
			}
		}

		if (controller_size > 0 && runtime.controller && !runtime.controller_is_component)
		{
			MemoryStream controller_stream(const_cast<char*>(controller_data), controller_size);
			if (!is_ok(runtime.controller->setState(&controller_stream)))
				return -5;
		}

		return 0;
	}

	bool capture_runtime_state_blob_locked(VstPluginRuntime& runtime, std::vector<unsigned char>& state_blob)
	{
		int state_size;
		int bytes_written = 0;

		state_blob.clear();
		state_size = capture_runtime_state_size(runtime);
		if (state_size < 0)
			return false;
		if (state_size == 0)
			return true;

		state_blob.resize(static_cast<size_t>(state_size));
		if (capture_runtime_state(runtime, state_blob.data(), state_size, &bytes_written) != 0 || bytes_written != state_size)
		{
			state_blob.clear();
			return false;
		}

		return true;
	}

	std::string wide_to_utf8(const wchar_t* text)
	{
		int required;
		std::string converted;

		if (!text || !text[0])
			return converted;

		required = WideCharToMultiByte(CP_UTF8, 0, text, -1, 0, 0, 0, 0);
		if (required <= 1)
			return converted;

		converted.resize(static_cast<size_t>(required - 1));
		WideCharToMultiByte(CP_UTF8, 0, text, -1, &converted[0], required - 1, 0, 0);
		return converted;
	}

	void utf8_to_wide(const std::string& text, wchar_t* destination, size_t destination_count)
	{
		if (!destination || destination_count == 0)
			return;

		destination[0] = L'\0';
		if (!text.empty())
		{
			MultiByteToWideChar(
				CP_UTF8,
				0,
				text.c_str(),
				-1,
				destination,
				static_cast<int>(destination_count));
			destination[destination_count - 1] = L'\0';
		}
	}

	void clear_probe_info(VstPluginProbeInfo* info)
	{
		if (!info)
			return;

		memset(info, 0, sizeof(VstPluginProbeInfo));
	}

	SpeakerArrangement arrangement_for_channels(int channels)
	{
		return channels <= 1 ? Steinberg::Vst::SpeakerArr::kMono : Steinberg::Vst::SpeakerArr::kStereo;
	}

	bool deactivate_runtime(VstPluginRuntime& runtime)
	{
		if (!runtime.active || !runtime.processor || !runtime.component)
			return true;

		runtime.processor->setProcessing(false);
		runtime.component->setActive(false);
		runtime.active = false;
		return true;
	}

	void detach_runtime_controller(VstPluginRuntime& runtime)
	{
		RuntimeConnectionProxy* component_connection_proxy;
		RuntimeConnectionProxy* controller_connection_proxy;
		RuntimeComponentHandler* component_handler;

		if (runtime.controller)
			runtime.controller->setComponentHandler(nullptr);

		component_connection_proxy = static_cast<RuntimeConnectionProxy*>(runtime.component_connection_proxy);
		controller_connection_proxy = static_cast<RuntimeConnectionProxy*>(runtime.controller_connection_proxy);
		if (component_connection_proxy)
			component_connection_proxy->disconnect();
		if (controller_connection_proxy)
			controller_connection_proxy->disconnect();

		component_handler = static_cast<RuntimeComponentHandler*>(runtime.component_handler);
		delete component_handler;
		delete component_connection_proxy;
		delete controller_connection_proxy;

		runtime.component_handler = nullptr;
		runtime.component_connection_proxy = nullptr;
		runtime.controller_connection_proxy = nullptr;
		runtime.input_parameter_changes.clearQueue();
		runtime.output_parameter_changes.clearQueue();
		runtime.input_parameter_transfer.removeChanges();

		if (runtime.controller && !runtime.controller_is_component)
		{
			auto base = Steinberg::U::cast<IPluginBase>(runtime.controller);
			if (base)
				base->terminate();
		}

		runtime.controller = nullptr;
		runtime.controller_is_component = false;
	}

	void release_runtime(VstPluginRuntime& runtime)
	{
		if (runtime.editor_session)
		{
			VstEditorSession* dangling_session = static_cast<VstEditorSession*>(runtime.editor_session);
			dangling_session->runtime = nullptr;
			runtime.editor_session = 0;
		}

		deactivate_runtime(runtime);
		runtime.process_data.unprepare();
		runtime.process_data.inputParameterChanges = nullptr;
		runtime.process_data.outputParameterChanges = nullptr;
		detach_runtime_controller(runtime);

		if (runtime.component && runtime.owns_component)
		{
			auto base = Steinberg::U::cast<IPluginBase>(runtime.component);
			if (base)
				base->terminate();
		}

		runtime.processor = nullptr;
		runtime.component = nullptr;
		runtime.host_application.reset();
		runtime.module.reset();
		runtime.input_buffers_64.clear();
		runtime.output_buffers_64.clear();
		runtime.input_buffers_32.clear();
		runtime.output_buffers_32.clear();
		runtime.input_ptrs_64.clear();
		runtime.output_ptrs_64.clear();
		runtime.input_ptrs_32.clear();
		runtime.output_ptrs_32.clear();
		runtime.sample_rate = 0;
		runtime.max_block_size = 0;
		runtime.chain_channels = 0;
		runtime.input_channels = 0;
		runtime.output_channels = 0;
		runtime.use_in_place_buffers = false;
		runtime.owns_component = true;
		runtime.controller_is_component = false;
		runtime.initialized = false;
		runtime.active = false;
		runtime.plugin_path.clear();
	}

	bool query_main_bus_channels(IComponent& component, int direction, int& channels)
	{
		BusInfo bus_info = {};

		channels = 0;
		if (component.getBusCount(kAudio, direction) <= 0)
			return false;

		if (!is_ok(component.getBusInfo(kAudio, direction, 0, bus_info)))
			return false;

		channels = bus_info.channelCount;
		return channels > 0;
	}

	bool activate_main_audio_buses(IComponent& component)
	{
		int input_bus_count;
		int output_bus_count;
		int i;

		input_bus_count = component.getBusCount(kAudio, kInput);
		output_bus_count = component.getBusCount(kAudio, kOutput);

		for (i = 0; i < input_bus_count; ++i)
		{
			if (!is_ok(component.activateBus(kAudio, kInput, i, i == 0)))
				return false;
		}

		for (i = 0; i < output_bus_count; ++i)
		{
			if (!is_ok(component.activateBus(kAudio, kOutput, i, i == 0)))
				return false;
		}

		return true;
	}

	bool configure_buffers(VstPluginRuntime& runtime)
	{
		int i;

		runtime.process_data.unprepare();
		runtime.process_data.prepare(*runtime.component, 0, runtime.symbolic_sample_size);
		runtime.process_data.processMode = kRealtime;
		runtime.process_data.symbolicSampleSize = runtime.symbolic_sample_size;
		runtime.process_data.processContext = &runtime.process_context;
		runtime.process_data.inputEvents = nullptr;
		runtime.process_data.outputEvents = nullptr;
		runtime.process_data.inputParameterChanges = &runtime.input_parameter_changes;
		runtime.process_data.outputParameterChanges = &runtime.output_parameter_changes;

		if (runtime.symbolic_sample_size == kSample64)
		{
			runtime.input_buffers_64.assign(runtime.input_channels, std::vector<double>(runtime.max_block_size, 0.0));
			runtime.output_buffers_64.assign(runtime.output_channels, std::vector<double>(runtime.max_block_size, 0.0));
			runtime.input_ptrs_64.resize(runtime.input_channels);
			runtime.output_ptrs_64.resize(runtime.output_channels);
			for (i = 0; i < runtime.input_channels; ++i)
				runtime.input_ptrs_64[i] = runtime.input_buffers_64[i].data();
			for (i = 0; i < runtime.output_channels; ++i)
				runtime.output_ptrs_64[i] = runtime.output_buffers_64[i].data();
			runtime.input_buffers_32.clear();
			runtime.output_buffers_32.clear();
			runtime.input_ptrs_32.clear();
			runtime.output_ptrs_32.clear();
		}
		else
		{
			runtime.input_buffers_32.assign(runtime.input_channels, std::vector<float>(runtime.max_block_size, 0.0f));
			runtime.output_buffers_32.assign(runtime.output_channels, std::vector<float>(runtime.max_block_size, 0.0f));
			runtime.input_ptrs_32.resize(runtime.input_channels);
			runtime.output_ptrs_32.resize(runtime.output_channels);
			for (i = 0; i < runtime.input_channels; ++i)
				runtime.input_ptrs_32[i] = runtime.input_buffers_32[i].data();
			for (i = 0; i < runtime.output_channels; ++i)
				runtime.output_ptrs_32[i] = runtime.output_buffers_32[i].data();
			runtime.input_buffers_64.clear();
			runtime.output_buffers_64.clear();
			runtime.input_ptrs_64.clear();
			runtime.output_ptrs_64.clear();
		}

		return true;
	}

	void prepare_output_buffers(VstPluginRuntime& runtime, int frames)
	{
		int i;

		if (runtime.symbolic_sample_size == kSample64)
		{
			for (i = 0; i < runtime.output_channels; ++i)
			{
				if (runtime.use_in_place_buffers && i < runtime.input_channels)
				{
					runtime.output_ptrs_64[i] = runtime.input_ptrs_64[i];
					continue;
				}

				runtime.output_ptrs_64[i] = runtime.output_buffers_64[i].data();
				if (i < runtime.input_channels)
					std::copy(runtime.input_buffers_64[i].begin(), runtime.input_buffers_64[i].begin() + frames, runtime.output_buffers_64[i].begin());
				else
					std::fill(runtime.output_buffers_64[i].begin(), runtime.output_buffers_64[i].begin() + frames, 0.0);
			}
		}
		else
		{
			for (i = 0; i < runtime.output_channels; ++i)
			{
				if (runtime.use_in_place_buffers && i < runtime.input_channels)
				{
					runtime.output_ptrs_32[i] = runtime.input_ptrs_32[i];
					continue;
				}

				runtime.output_ptrs_32[i] = runtime.output_buffers_32[i].data();
				if (i < runtime.input_channels)
					std::copy(runtime.input_buffers_32[i].begin(), runtime.input_buffers_32[i].begin() + frames, runtime.output_buffers_32[i].begin());
				else
					std::fill(runtime.output_buffers_32[i].begin(), runtime.output_buffers_32[i].begin() + frames, 0.0f);
			}
		}
	}

	int configure_runtime(VstPluginRuntime& runtime, int sample_rate, int max_block_size, int num_channels)
	{
		ProcessSetup setup = {};
		SpeakerArrangement input_arrangement;
		SpeakerArrangement output_arrangement;

		if (!runtime.component || !runtime.processor)
			return -1;
		if (sample_rate <= 0 || max_block_size <= 0 || num_channels <= 0)
			return -2;

		deactivate_runtime(runtime);

		input_arrangement = arrangement_for_channels(num_channels);
		output_arrangement = arrangement_for_channels(num_channels);
		runtime.component->setIoMode(kSimple);
		if (runtime.component->getBusCount(kAudio, kInput) > 0 && runtime.component->getBusCount(kAudio, kOutput) > 0)
			runtime.processor->setBusArrangements(&input_arrangement, 1, &output_arrangement, 1);

		if (!query_main_bus_channels(*runtime.component, kInput, runtime.input_channels))
			return -3;
		if (!query_main_bus_channels(*runtime.component, kOutput, runtime.output_channels))
			return -4;
		if (runtime.input_channels < 1 || runtime.input_channels > 2 || runtime.output_channels < 1 || runtime.output_channels > 2)
			return -5;
		if (!activate_main_audio_buses(*runtime.component))
			return -6;

		runtime.symbolic_sample_size = is_ok(runtime.processor->canProcessSampleSize(kSample64)) ? kSample64 : kSample32;
		if (!is_ok(runtime.processor->canProcessSampleSize(runtime.symbolic_sample_size)))
			return -7;

		runtime.sample_rate = sample_rate;
		runtime.max_block_size = max_block_size;
		runtime.chain_channels = num_channels;
		runtime.use_in_place_buffers = runtime.input_channels == runtime.output_channels;
		runtime.input_parameter_changes.clearQueue();
		runtime.output_parameter_changes.clearQueue();
		runtime.process_data.inputParameterChanges = &runtime.input_parameter_changes;
		runtime.process_data.outputParameterChanges = &runtime.output_parameter_changes;
		runtime.process_context = {};
		runtime.process_context.sampleRate = static_cast<double>(sample_rate);
		runtime.process_context.tempo = 120.0;
		configure_buffers(runtime);

		setup.processMode = kRealtime;
		setup.symbolicSampleSize = runtime.symbolic_sample_size;
		setup.maxSamplesPerBlock = max_block_size;
		setup.sampleRate = static_cast<double>(sample_rate);

		if (!is_ok(runtime.processor->setupProcessing(setup)))
			return -8;
		if (!is_ok(runtime.component->setActive(true)))
			return -9;

		runtime.processor->setProcessing(true);
		runtime.active = true;
		return 0;
	}

	bool connect_runtime_components(VstPluginRuntime& runtime)
	{
		IPtr<IConnectionPoint> component_connection_point;
		IPtr<IConnectionPoint> controller_connection_point;
		RuntimeConnectionProxy* component_proxy;
		RuntimeConnectionProxy* controller_proxy;

		if (!runtime.component || !runtime.controller)
			return false;

		component_connection_point = Steinberg::FUnknownPtr<IConnectionPoint>(runtime.component);
		controller_connection_point = Steinberg::FUnknownPtr<IConnectionPoint>(runtime.controller);
		if (!component_connection_point || !controller_connection_point)
			return true;

		component_proxy = new RuntimeConnectionProxy(component_connection_point);
		controller_proxy = new RuntimeConnectionProxy(controller_connection_point);
		if (!component_proxy || !controller_proxy)
		{
			delete component_proxy;
			delete controller_proxy;
			return false;
		}

		runtime.component_connection_proxy = component_proxy;
		runtime.controller_connection_proxy = controller_proxy;
		if (component_proxy->connect(controller_connection_point) != kResultTrue)
			return false;
		if (controller_proxy->connect(component_connection_point) != kResultTrue)
			return false;

		return true;
	}

	bool initialize_runtime_controller(VstPluginRuntime& runtime, ParameterChangeTransfer& transfer_target)
	{
		RuntimeComponentHandler* component_handler;
		MemoryStream component_state_stream;
		int64 seek_result = 0;

		if (!runtime.controller)
			return true;

		trace_editor("load initialize_runtime_controller begin");
		component_handler = new RuntimeComponentHandler(runtime, transfer_target);
		if (!component_handler)
			return false;

		runtime.component_handler = component_handler;
		trace_editor("load initialize_runtime_controller setComponentHandler");
		if (!is_ok(runtime.controller->setComponentHandler(component_handler)))
			return false;

		trace_editor("load initialize_runtime_controller component getState");
		if (runtime.component && is_ok(runtime.component->getState(&component_state_stream)))
		{
			component_state_stream.seek(0, Steinberg::IBStream::kIBSeekSet, &seek_result);
			trace_editor("load initialize_runtime_controller controller setComponentState");
			runtime.controller->setComponentState(&component_state_stream);
		}

		runtime.input_parameter_changes.setMaxParameters(runtime.controller->getParameterCount());
		runtime.output_parameter_changes.setMaxParameters(runtime.controller->getParameterCount());
		runtime.input_parameter_transfer.setMaxParameters(runtime.controller->getParameterCount());
		trace_editor("load initialize_runtime_controller complete params=%d", runtime.controller->getParameterCount());
		return true;
	}

	bool sync_runtime_state(const VstPluginRuntime& source, VstPluginRuntime& target)
	{
		MemoryStream component_state_stream;
		MemoryStream controller_state_stream;
		int64 seek_result = 0;

		if (source.component && target.component && is_ok(source.component->getState(&component_state_stream)))
		{
			component_state_stream.seek(0, Steinberg::IBStream::kIBSeekSet, &seek_result);
			if (!is_ok(target.component->setState(&component_state_stream)))
				return false;

			if (target.controller)
			{
				component_state_stream.seek(0, Steinberg::IBStream::kIBSeekSet, &seek_result);
				if (!is_ok(target.controller->setComponentState(&component_state_stream)))
					return false;
			}
		}

		if (source.controller && target.controller && is_ok(source.controller->getState(&controller_state_stream)))
		{
			controller_state_stream.seek(0, Steinberg::IBStream::kIBSeekSet, &seek_result);
			if (!is_ok(target.controller->setState(&controller_state_stream)))
				return false;
		}

		return true;
	}

	bool load_editor_controller_instance(
		VstPluginRuntime& runtime,
		const VstPluginRuntime& source,
		ParameterChangeTransfer& transfer_target)
	{
		VST3::Hosting::PluginFactory factory(nullptr);
		Steinberg::TUID controller_cid = {};
		IPtr<IEditController> controller_candidate;
		IPtr<IPluginBase> controller_base;

		runtime.plugin_path = source.plugin_path;
		runtime.class_info = source.class_info;
		runtime.module = source.module;
		runtime.host_application = source.host_application;
		if (!runtime.module)
		{
			trace_editor("editor-controller source module missing");
			return false;
		}
		if (!runtime.host_application)
		{
			trace_editor("editor-controller source host application missing");
			return false;
		}
		trace_editor("editor-controller reuse source module and host application");

		factory = runtime.module->getFactory();

		if (!source.component)
		{
			trace_editor("editor-controller source component missing");
			return false;
		}

		if (source.component && is_ok(source.component->getControllerClassId(controller_cid)))
		{
			trace_editor("editor-controller create separate controller");
			controller_candidate = factory.createInstance<IEditController>(VST3::UID(controller_cid));
		}
		else if (source.controller_is_component)
		{
			trace_editor("editor-controller create controller from component class");
			controller_candidate = factory.createInstance<IEditController>(source.class_info.ID());
		}
		else
		{
			trace_editor("editor-controller no controller class id");
			return false;
		}

		if (!controller_candidate)
		{
			trace_editor("editor-controller createInstance failed");
			return false;
		}

		controller_base = Steinberg::U::cast<IPluginBase>(controller_candidate);
		if (!controller_base)
		{
			trace_editor("editor-controller cast IPluginBase failed");
			return false;
		}

		trace_editor("editor-controller initialize begin");
		if (!is_ok(controller_base->initialize(static_cast<FUnknown*>(runtime.host_application.get()))))
		{
			trace_editor("editor-controller initialize failed");
			return false;
		}
		trace_editor("editor-controller initialize ok");

		runtime.component = source.component;
		runtime.owns_component = false;
		runtime.controller = controller_candidate;
		runtime.controller_is_component = false;
		trace_editor("editor-controller connect borrowed component");
		if (!connect_runtime_components(runtime))
		{
			trace_editor("editor-controller connect borrowed component failed");
			detach_runtime_controller(runtime);
			runtime.component = nullptr;
			runtime.owns_component = true;
			return false;
		}
		trace_editor("editor-controller connect borrowed component ok");
		if (!initialize_runtime_controller(runtime, transfer_target))
		{
			trace_editor("editor-controller initialize_runtime_controller failed");
			detach_runtime_controller(runtime);
			runtime.component = nullptr;
			runtime.owns_component = true;
			return false;
		}

		trace_editor("editor-controller sync state begin");
		if (source.controller)
		{
			MemoryStream controller_state_stream;
			int64 seek_result = 0;

			if (is_ok(source.controller->getState(&controller_state_stream)))
			{
				controller_state_stream.seek(0, Steinberg::IBStream::kIBSeekSet, &seek_result);
				if (!is_ok(runtime.controller->setState(&controller_state_stream)))
				{
					trace_editor("editor-controller setState failed");
					detach_runtime_controller(runtime);
					runtime.component = nullptr;
					runtime.owns_component = true;
					return false;
				}
				trace_editor("editor-controller setState ok");
			}
			else
			{
				trace_editor("editor-controller source controller getState unavailable");
			}
		}

		runtime.input_parameter_changes.setMaxParameters(runtime.controller->getParameterCount());
		runtime.output_parameter_changes.setMaxParameters(runtime.controller->getParameterCount());
		runtime.input_parameter_transfer.setMaxParameters(runtime.controller->getParameterCount());
		trace_editor("editor-controller parameter count after state=%d", runtime.controller->getParameterCount());

		runtime.initialized = true;
		trace_editor("editor-controller ready");
		return true;
	}

	/* Resolve the actual DLL path for a VST3 plugin.  For bundle directories
	   the DLL is at Contents/x86_64-win/<name>.vst3.  For bare DLL files the
	   path is returned unchanged.  Returns true if a valid DLL path was found. */
	bool resolve_plugin_dll_path(const wchar_t* plugin_path, wchar_t* dll_path, size_t dll_path_count)
	{
		DWORD attrs = GetFileAttributesW(plugin_path);
		if (attrs == INVALID_FILE_ATTRIBUTES)
			return false;

		if (!(attrs & FILE_ATTRIBUTE_DIRECTORY))
		{
			/* Bare DLL file — use as-is. */
			wcsncpy_s(dll_path, dll_path_count, plugin_path, _TRUNCATE);
			return true;
		}

		/* Bundle directory — look for Contents/x86_64-win/<name>.vst3 */
		const wchar_t* basename = wcsrchr(plugin_path, L'\\');
		if (!basename)
			basename = wcsrchr(plugin_path, L'/');
		basename = basename ? basename + 1 : plugin_path;

		wchar_t inner_path[MAX_PATH * 2];
		_snwprintf_s(inner_path, _countof(inner_path), _TRUNCATE,
			L"%s\\Contents\\x86_64-win\\%s", plugin_path, basename);

		if (GetFileAttributesW(inner_path) != INVALID_FILE_ATTRIBUTES)
		{
			wcsncpy_s(dll_path, dll_path_count, inner_path, _TRUNCATE);
			return true;
		}

		return false;
	}

	bool load_runtime_instance(
		VstPluginRuntime& runtime,
		const wchar_t* plugin_path,
		wchar_t* plugin_name,
		size_t plugin_name_count,
		ParameterChangeTransfer* transfer_target)
	{
		std::string module_path;
		std::string error_description;
		VST3::Hosting::PluginFactory factory(nullptr);
		VST3::Hosting::PluginFactory::ClassInfos class_infos;
		size_t i;

		module_path = wide_to_utf8(plugin_path);
		if (module_path.empty())
			return false;
		runtime.plugin_path.assign(plugin_path);

		trace_editor("load module create begin");

		/* Pre-load the DLL with LOAD_WITH_ALTERED_SEARCH_PATH so that
		   dependent vendor DLLs adjacent to the plugin are found.  The
		   SDK's Module::create uses plain LoadLibraryW which resolves
		   dependencies from the calling process directory instead. */
		HMODULE preloaded = 0;
		{
			wchar_t dll_path[MAX_PATH * 2];
			if (resolve_plugin_dll_path(plugin_path, dll_path, _countof(dll_path)))
				preloaded = LoadLibraryExW(dll_path, NULL, LOAD_WITH_ALTERED_SEARCH_PATH);
		}

		runtime.module = VST3::Hosting::Module::create(module_path, error_description);

		if (preloaded)
			FreeLibrary(preloaded);
		if (!runtime.module)
		{
			trace_editor("load module create failed error=%s", error_description.c_str());
			return false;
		}
		trace_editor("load module create ok");

		trace_editor("load getFactory");
		factory = runtime.module->getFactory();
		trace_editor("load classInfos");
		class_infos = factory.classInfos();
		for (i = 0; i < class_infos.size(); ++i)
		{
			if (class_infos[i].category() == kVstAudioEffectClass)
			{
				runtime.class_info = class_infos[i];
				trace_editor("load selected class name=%s", runtime.class_info.name().c_str());
				break;
			}
		}

		if (i == class_infos.size())
		{
			trace_editor("load no audio effect class found");
			return false;
		}

		runtime.host_application.reset(new HostApplication());
		trace_editor("load create component");
		runtime.component = factory.createInstance<IComponent>(runtime.class_info.ID());
		if (!runtime.component)
		{
			trace_editor("load create component failed");
			return false;
		}

		auto base = Steinberg::U::cast<IPluginBase>(runtime.component);
		if (base)
		{
			trace_editor("load component initialize begin");
			if (!is_ok(base->initialize(static_cast<FUnknown*>(runtime.host_application.get()))))
				return false;
			trace_editor("load component initialize ok");
		}
		else
		{
			trace_editor("load component cast to IPluginBase failed");
			return false;
		}

		trace_editor("load query processor");
		runtime.processor = Steinberg::FUnknownPtr<IAudioProcessor>(runtime.component);
		if (!runtime.processor)
		{
			trace_editor("load query processor failed");
			return false;
		}
		trace_editor("load query processor ok");

		trace_editor("load query controller from component");
		runtime.controller = Steinberg::FUnknownPtr<IEditController>(runtime.component);
		if (runtime.controller)
		{
			runtime.controller_is_component = true;
			trace_editor("load controller is single component");
		}
		else
		{
			Steinberg::TUID controller_cid = {};
			IPtr<IEditController> controller_candidate;
			IPtr<IPluginBase> controller_base;

			trace_editor("load getControllerClassId");
			if (is_ok(runtime.component->getControllerClassId(controller_cid)))
			{
				trace_editor("load create controller");
				controller_candidate = factory.createInstance<IEditController>(VST3::UID(controller_cid));
				if (controller_candidate)
				{
					controller_base = Steinberg::U::cast<IPluginBase>(controller_candidate);
					if (controller_base &&
						is_ok(controller_base->initialize(static_cast<FUnknown*>(runtime.host_application.get()))))
					{
						runtime.controller = controller_candidate;
						trace_editor("load controller initialize ok");
					}
					else
					{
						trace_editor("load controller initialize failed");
					}
				}
				else
				{
					trace_editor("load create controller failed");
				}
			}
			else
			{
				trace_editor("load getControllerClassId failed");
			}
		}

		if (runtime.controller)
		{
			bool controller_ready = true;
			ParameterChangeTransfer& effective_transfer = transfer_target ? *transfer_target : runtime.input_parameter_transfer;

			if (!runtime.controller_is_component)
			{
				trace_editor("load connect_runtime_components");
				controller_ready = connect_runtime_components(runtime);
				trace_editor("load connect_runtime_components result=%d", controller_ready ? 1 : 0);
			}
			if (controller_ready)
			{
				trace_editor("load initialize_runtime_controller");
				controller_ready = initialize_runtime_controller(runtime, effective_transfer);
				trace_editor("load initialize_runtime_controller result=%d", controller_ready ? 1 : 0);
			}
			if (!controller_ready)
				detach_runtime_controller(runtime);
		}
		else
		{
			trace_editor("load runtime has no controller");
		}

		utf8_to_wide(runtime.class_info.name(), plugin_name, plugin_name_count);
		runtime.initialized = true;
		trace_editor("load runtime instance complete");
		return true;
	}

	void assign_process_buffers(VstPluginRuntime& runtime)
	{
		int i;

		if (runtime.symbolic_sample_size == kSample64)
		{
			for (i = 0; i < runtime.input_channels; ++i)
				runtime.process_data.setChannelBuffer64(kInput, 0, i, runtime.input_ptrs_64[i]);
			for (i = 0; i < runtime.output_channels; ++i)
				runtime.process_data.setChannelBuffer64(kOutput, 0, i, runtime.output_ptrs_64[i]);
		}
		else
		{
			for (i = 0; i < runtime.input_channels; ++i)
				runtime.process_data.setChannelBuffer(kInput, 0, i, runtime.input_ptrs_32[i]);
			for (i = 0; i < runtime.output_channels; ++i)
				runtime.process_data.setChannelBuffer(kOutput, 0, i, runtime.output_ptrs_32[i]);
		}

		// HostProcessData does not guarantee silenceFlags are reset when the host owns the
		// sample storage, so explicitly mark the active buses as carrying audio each block.
		for (i = 0; i < runtime.process_data.numInputs; ++i)
			runtime.process_data.inputs[i].silenceFlags = 0;
		for (i = 0; i < runtime.process_data.numOutputs; ++i)
			runtime.process_data.outputs[i].silenceFlags = 0;
	}

	void fill_input_buffers(VstPluginRuntime& runtime, const double* interleaved_buffer, int frames, int chain_channels)
	{
		int i;

		for (i = 0; i < frames; ++i)
		{
			double left = interleaved_buffer[i * chain_channels];
			double right = chain_channels > 1 ? interleaved_buffer[(i * chain_channels) + 1] : left;
			double mono = 0.5 * (left + right);

			if (runtime.symbolic_sample_size == kSample64)
			{
				runtime.input_buffers_64[0][i] = runtime.input_channels == 1 ? mono : left;
				if (runtime.input_channels > 1)
					runtime.input_buffers_64[1][i] = right;
			}
			else
			{
				runtime.input_buffers_32[0][i] = static_cast<float>(runtime.input_channels == 1 ? mono : left);
				if (runtime.input_channels > 1)
					runtime.input_buffers_32[1][i] = static_cast<float>(right);
			}
		}
	}

	void write_output_buffers(VstPluginRuntime& runtime, double* interleaved_buffer, int frames, int chain_channels)
	{
		int i;

		for (i = 0; i < frames; ++i)
		{
			double left;
			double right;

			if (runtime.symbolic_sample_size == kSample64)
			{
				left = runtime.output_ptrs_64[0][i];
				right = runtime.output_channels > 1 ? runtime.output_ptrs_64[1][i] : left;
			}
			else
			{
				left = runtime.output_ptrs_32[0][i];
				right = runtime.output_channels > 1 ? runtime.output_ptrs_32[1][i] : left;
			}

			interleaved_buffer[i * chain_channels] = left;
			if (chain_channels > 1)
				interleaved_buffer[(i * chain_channels) + 1] = right;
		}
	}

	void destroy_editor_session(VstEditorSession*& session, bool release_owner_state)
	{
		VstPluginRuntime* runtime;
		VstPluginRuntime* live_runtime;
		VstProcessingState* owner_state;

		if (!session)
			return;

		trace_editor("destroy editor session=%p release_owner=%d", session, release_owner_state ? 1 : 0);
		runtime = session->runtime;
		live_runtime = session->live_runtime;
		owner_state = session->owner_state;
		detach_editor_session_view(session);

		session->runtime = nullptr;
		session->live_runtime = nullptr;
		session->owner_state = nullptr;
		delete session;
		session = 0;

		if (live_runtime && live_runtime->editor_session)
			live_runtime->editor_session = nullptr;
		if (release_owner_state)
			release_owner_state_safe(live_runtime ? live_runtime : runtime, owner_state);
	}

	void detach_editor_session_view(VstEditorSession* session)
	{
		VstPluginRuntime* runtime;
		RuntimePlugFrame* frame;
		ScopedRuntimeApiLock api_lock(session ? session->runtime : nullptr);

		if (!session)
			return;

		runtime = session->runtime;
		frame = static_cast<RuntimePlugFrame*>(session->frame);
		if (session->view)
		{
			trace_editor("detach editor session setFrame null session=%p", session);
			session->view->setFrame(nullptr);
			trace_editor("detach editor session removed begin session=%p", session);
			session->view->removed();
			trace_editor("detach editor session removed complete session=%p", session);
			session->view = nullptr;
		}
		delete frame;
		session->frame = 0;
		if (runtime && runtime->editor_session == session)
			runtime->editor_session = 0;
		if (session->live_runtime && session->live_runtime->editor_session == session)
			session->live_runtime->editor_session = 0;
	}

	int resize_editor_session(VstEditorSession& session, int width, int height)
	{
		ViewRect rect;
		ScopedRuntimeApiLock api_lock(session.runtime);

		if (!session.view)
			return -1;
		if (width <= 0 || height <= 0)
			return -2;

		rect.left = 0;
		rect.top = 0;
		rect.right = width;
		rect.bottom = height;

		return is_ok(session.view->onSize(&rect)) ? 0 : -3;
	}

	int open_editor_window_on_host_thread(
		VstPluginRuntime& runtime,
		VstProcessingState* owner_state,
		int plugin_index,
		int& width,
		int& height,
		int& can_resize,
		VstEditorSession*& session)
	{
		std::unique_ptr<VstEditorSession> new_session;
		RuntimePlugFrame* frame;
		ViewRect rect;
		RECT window_rect = {};
		DWORD style;
		wchar_t title[256] = {};

		session = 0;
		width = 0;
		height = 0;
		can_resize = 0;

		if (!runtime.controller)
			return -1;

		VstEditorSession* existing_session = static_cast<VstEditorSession*>(
			InterlockedCompareExchangePointer(reinterpret_cast<PVOID*>(&runtime.editor_session), nullptr, nullptr));
		if (existing_session)
		{
			bool has_live_window = existing_session->window && IsWindow(existing_session->window);

			if (!has_live_window || !existing_session->view)
			{
				trace_editor("open found stale editor session=%p window=%p; recreating",
					existing_session,
					existing_session ? existing_session->window : 0);
				destroy_editor_session(existing_session, true);
			}
			else
			{
				trace_editor("open reusing existing editor session=%p window=%p", existing_session, existing_session->window);
				VstProcessingState_Release(owner_state);
				ShowWindow(existing_session->window, SW_SHOWNORMAL);
				SetWindowPos(existing_session->window, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
				SetForegroundWindow(existing_session->window);
				session = existing_session;
				can_resize = existing_session->can_resize ? 1 : 0;
				width = existing_session->requested_width;
				height = existing_session->requested_height;
				return 0;
			}
		}

		/*
		Single-instance model: the live audio runtime and editor share
		one component/controller pair.  Thread safety between the audio
		thread and the editor is handled by the existing try-lock in
		VstRuntime_Process -- if a control-path operation holds the API
		lock, the audio thread skips the block and falls back to dry
		audio, which is the designed behaviour.
		*/

		new_session.reset(new VstEditorSession());
		if (!new_session)
			return -2;

		new_session->runtime = &runtime;
		new_session->live_runtime = &runtime;
		new_session->owner_state = owner_state;
		new_session->plugin_index = plugin_index;
		new_session->requested_width = 640;
		new_session->requested_height = 480;

		trace_editor("host open createView begin");
		new_session->view = runtime.controller->createView(Steinberg::Vst::ViewType::kEditor);
		if (!new_session->view)
			return -3;
		trace_editor("host open createView ok");
		if (!is_ok(new_session->view->isPlatformTypeSupported(Steinberg::kPlatformTypeHWND)))
			return -4;
		trace_editor("host open platform type supported");

		if (is_ok(new_session->view->getSize(&rect)))
		{
			new_session->requested_width = rect.right - rect.left;
			new_session->requested_height = rect.bottom - rect.top;
		}

		frame = new RuntimePlugFrame(*new_session);
		if (!frame)
			return -5;
		new_session->frame = frame;
		trace_editor("host open setFrame begin");
		if (!is_ok(new_session->view->setFrame(frame)))
		{
			delete frame;
			new_session->frame = 0;
			return -6;
		}
		trace_editor("host open setFrame ok");

		new_session->can_resize = is_ok(new_session->view->canResize());
		can_resize = new_session->can_resize ? 1 : 0;
		style = new_session->can_resize ? WS_OVERLAPPEDWINDOW : (WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX);
		window_rect.left = 0;
		window_rect.top = 0;
		window_rect.right = new_session->requested_width;
		window_rect.bottom = new_session->requested_height;
		AdjustWindowRectEx(&window_rect, style, FALSE, 0);
		utf8_to_wide(runtime.class_info.name(), title, _countof(title));
		if (!title[0])
			wcsncpy_s(title, _countof(title), L"VST Plugin Editor", _TRUNCATE);

		new_session->window = CreateWindowExW(
			WS_EX_TOPMOST,
			g_editor_window_class_name,
			title,
			style | WS_CLIPCHILDREN | WS_CLIPSIBLINGS,
			CW_USEDEFAULT,
			CW_USEDEFAULT,
			window_rect.right - window_rect.left,
			window_rect.bottom - window_rect.top,
			0,
			0,
			GetModuleHandleW(nullptr),
			new_session.get());
		if (!new_session->window)
		{
			new_session->view->setFrame(nullptr);
			delete frame;
			new_session->frame = 0;
			return -7;
		}

		trace_editor("host open attached begin");
		if (!is_ok(new_session->view->attached(new_session->window, Steinberg::kPlatformTypeHWND)))
		{
			trace_editor("host open attached failed");
			new_session->view->setFrame(nullptr);
			delete frame;
			new_session->frame = 0;
			new_session->view = nullptr;
			SetWindowLongPtrW(new_session->window, GWLP_USERDATA, 0);
			DestroyWindow(new_session->window);
			new_session->window = 0;
			return -8;
		}
		trace_editor("host open attached ok");

		runtime.editor_session = new_session.get();
		if (is_ok(new_session->view->getSize(&rect)))
		{
			new_session->requested_width = rect.right - rect.left;
			new_session->requested_height = rect.bottom - rect.top;
		}

		SetWindowPos(new_session->window, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
		ShowWindow(new_session->window, SW_SHOWNORMAL);
		UpdateWindow(new_session->window);

		width = new_session->requested_width;
		height = new_session->requested_height;
		session = new_session.release();
		return 0;
	}

	LRESULT CALLBACK editor_window_proc(HWND hwnd, UINT message, WPARAM w_param, LPARAM l_param)
	{
		VstEditorSession* session = reinterpret_cast<VstEditorSession*>(GetWindowLongPtrW(hwnd, GWLP_USERDATA));

		switch (message)
		{
		case WM_NCCREATE:
			{
				CREATESTRUCTW* create_struct = reinterpret_cast<CREATESTRUCTW*>(l_param);
				VstEditorSession* creating_session = static_cast<VstEditorSession*>(create_struct->lpCreateParams);
				if (!creating_session)
					return FALSE;
				SetWindowLongPtrW(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(creating_session));
				creating_session->window = hwnd;
				return TRUE;
			}

		case WM_SIZE:
			if (session && session->view && w_param != SIZE_MINIMIZED)
				resize_editor_session(*session, LOWORD(l_param), HIWORD(l_param));
			return 0;

		case WM_CLOSE:
			trace_editor("editor window WM_CLOSE session=%p hwnd=%p", session, hwnd);
			if (session)
			{
				SetWindowLongPtrW(hwnd, GWLP_USERDATA, 0);
				session->window = 0;
				detach_editor_session_view(session);
				DestroyWindow(hwnd);
				destroy_editor_session(session, true);
				return 0;
			}
			DestroyWindow(hwnd);
			return 0;

		case WM_NCDESTROY:
			if (session)
			{
				trace_editor("editor window WM_NCDESTROY session=%p hwnd=%p", session, hwnd);
				SetWindowLongPtrW(hwnd, GWLP_USERDATA, 0);
				session->window = 0;
				destroy_editor_session(session, true);
			}
			return 0;
		}

		return DefWindowProcW(hwnd, message, w_param, l_param);
	}

	int process_host_command(VstPluginRuntime& runtime)
	{
		VstEditorSession* local_session;

		/*
		Host-thread commands own control/editor/state transitions. Audio uses a
		try-lock on the same API lock so control work can serialize access without
		ever blocking the DSP thread indefinitely.
		*/
		switch (runtime.command_type)
		{
		case VstPluginRuntime::HostCommandInitialize:
			{
				ScopedRuntimeApiLock api_lock(&runtime);
				if (!load_runtime_instance(runtime, runtime.command_plugin_path, runtime.command_plugin_name, runtime.command_plugin_name_count))
					return -2;
				return configure_runtime(runtime, runtime.command_sample_rate, runtime.command_max_block_size, runtime.command_num_channels);
			}

		case VstPluginRuntime::HostCommandReconfigure:
			{
				ScopedRuntimeApiLock api_lock(&runtime);
				return configure_runtime(runtime, runtime.command_sample_rate, runtime.command_max_block_size, runtime.command_num_channels);
			}

		case VstPluginRuntime::HostCommandOpenEditor:
			trace_editor("process host command open editor runtime=%p", &runtime);
			return open_editor_window_on_host_thread(
				runtime,
				runtime.command_owner_state,
				runtime.command_plugin_index,
				runtime.command_width,
				runtime.command_height,
				runtime.command_can_resize,
				runtime.command_session);

		case VstPluginRuntime::HostCommandCloseEditor:
			trace_editor("process host command close editor runtime=%p session=%p", &runtime, runtime.command_session);
			local_session = runtime.command_session;
			if (local_session && local_session->window)
			{
				HWND window = local_session->window;
				SetWindowLongPtrW(window, GWLP_USERDATA, 0);
				local_session->window = 0;
				detach_editor_session_view(local_session);
				DestroyWindow(window);
				destroy_editor_session(local_session, true);
			}
			else if (local_session)
				destroy_editor_session(local_session, true);
			runtime.command_session = 0;
			return 0;

		case VstPluginRuntime::HostCommandCloseOpenEditor:
			trace_editor("process host command close open editor runtime=%p session=%p", &runtime, runtime.editor_session);
			if (runtime.editor_session)
			{
				VstEditorSession* open_session = static_cast<VstEditorSession*>(runtime.editor_session);
				if (open_session->window)
				{
					HWND window = open_session->window;
					SetWindowLongPtrW(window, GWLP_USERDATA, 0);
					open_session->window = 0;
					detach_editor_session_view(open_session);
					DestroyWindow(window);
					destroy_editor_session(open_session, true);
				}
				else
					destroy_editor_session(open_session, true);
			}
			return 0;

		case VstPluginRuntime::HostCommandResizeEditor:
			if (!runtime.command_session || !runtime.command_session->window)
				return -1;
			SetWindowPos(runtime.command_session->window, 0, 0, 0, runtime.command_width, runtime.command_height, SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE);
			return 0;

		case VstPluginRuntime::HostCommandGetStateSize:
			{
				ScopedRuntimeApiLock api_lock(&runtime);
				return capture_runtime_state_size(runtime);
			}

		case VstPluginRuntime::HostCommandGetState:
			{
				ScopedRuntimeApiLock api_lock(&runtime);
				return capture_runtime_state(runtime, runtime.command_state_buffer, runtime.command_state_buffer_size, runtime.command_bytes_written);
			}

		case VstPluginRuntime::HostCommandSetState:
			{
				ScopedRuntimeApiLock api_lock(&runtime);
				return apply_runtime_state(runtime, runtime.command_state_buffer, runtime.command_state_buffer_size);
			}

		case VstPluginRuntime::HostCommandShutdown:
			trace_editor("process host command shutdown runtime=%p session=%p", &runtime, runtime.editor_session);
			{
				ScopedRuntimeApiLock api_lock(&runtime);
				if (runtime.editor_session)
				{
					VstEditorSession* open_session = static_cast<VstEditorSession*>(runtime.editor_session);
					if (open_session->window)
					{
						HWND window = open_session->window;
						SetWindowLongPtrW(window, GWLP_USERDATA, 0);
						open_session->window = 0;
						detach_editor_session_view(open_session);
						DestroyWindow(window);
						destroy_editor_session(open_session, true);
					}
					else
						destroy_editor_session(open_session, true);
				}
				release_runtime(runtime);
				return 0;
			}
		}

		return 0;
	}

	void flush_output_param_cache(VstPluginRuntime& runtime)
	{
		LONG count = InterlockedCompareExchange(&runtime.output_param_cache_count, 0, 0);
		if (count <= 0 || !runtime.controller)
			return;

		for (LONG i = 0; i < count; ++i)
		{
			if (InterlockedCompareExchange(&runtime.output_param_cache[i].active, 0, 1) == 1)
				runtime.controller->setParamNormalized(runtime.output_param_cache[i].id, runtime.output_param_cache[i].value);
		}
	}

	unsigned __stdcall runtime_host_thread_proc(void* context)
	{
		VstPluginRuntime* runtime = static_cast<VstPluginRuntime*>(context);
		MSG message = {};
		bool running = true;

		PeekMessageW(&message, 0, WM_USER, WM_USER, PM_NOREMOVE);
		InitOnceExecuteOnce(&g_editor_window_class_once, ensure_editor_window_class, 0, 0);
		SetEvent(runtime->host_ready_event);

		while (running)
		{
			DWORD wait_result = MsgWaitForMultipleObjects(1, &runtime->command_event, FALSE, kOutputParamFlushIntervalMs, QS_ALLINPUT);

			if (wait_result == WAIT_OBJECT_0)
			{
				runtime->command_result = process_host_command(*runtime);
				running = runtime->command_type != VstPluginRuntime::HostCommandShutdown;
				SetEvent(runtime->command_complete_event);
			}
			else if (wait_result == WAIT_OBJECT_0 + 1)
			{
				while (PeekMessageW(&message, 0, 0, 0, PM_REMOVE))
				{
					if (message.message == WM_QUIT)
					{
						running = false;
						break;
					}
					TranslateMessage(&message);
					DispatchMessageW(&message);
				}
			}

			// Flush buffered output parameters to the controller on the
			// host thread (~30 Hz).  This is the correct thread for
			// setParamNormalized per VST3 spec.
			flush_output_param_cache(*runtime);
		}

		return 0;
	}
}

int VstRuntime_Create(
	VstPluginRuntime*& runtime,
	const wchar_t* plugin_path,
	int sample_rate,
	int max_block_size,
	int num_channels,
	wchar_t* plugin_name,
	size_t plugin_name_count)
{
	std::unique_ptr<VstPluginRuntime> new_runtime(new VstPluginRuntime());
	uintptr_t thread_handle = 0;
	unsigned thread_id = 0;
	int result;

	if (!plugin_path || !plugin_path[0])
		return -1;

	if (!InitializeCriticalSectionAndSpinCount(&new_runtime->command_lock, 2500))
		return -2;
	if (!InitializeCriticalSectionAndSpinCount(&new_runtime->api_lock, 2500))
	{
		DeleteCriticalSection(&new_runtime->command_lock);
		return -2;
	}
	new_runtime->host_ready_event = CreateEventW(nullptr, TRUE, FALSE, nullptr);
	new_runtime->command_event = CreateEventW(nullptr, FALSE, FALSE, nullptr);
	new_runtime->command_complete_event = CreateEventW(nullptr, FALSE, FALSE, nullptr);
	if (!new_runtime->host_ready_event || !new_runtime->command_event || !new_runtime->command_complete_event)
	{
		if (new_runtime->host_ready_event)
			CloseHandle(new_runtime->host_ready_event);
		if (new_runtime->command_event)
			CloseHandle(new_runtime->command_event);
		if (new_runtime->command_complete_event)
			CloseHandle(new_runtime->command_complete_event);
		DeleteCriticalSection(&new_runtime->api_lock);
		DeleteCriticalSection(&new_runtime->command_lock);
		return -2;
	}

	thread_handle = _beginthreadex(nullptr, 0, runtime_host_thread_proc, new_runtime.get(), 0, &thread_id);
	if (!thread_handle)
	{
		CloseHandle(new_runtime->host_ready_event);
		CloseHandle(new_runtime->command_event);
		CloseHandle(new_runtime->command_complete_event);
		DeleteCriticalSection(&new_runtime->api_lock);
		DeleteCriticalSection(&new_runtime->command_lock);
		return -2;
	}
	new_runtime->host_thread = reinterpret_cast<HANDLE>(thread_handle);
	new_runtime->host_thread_id = thread_id;
	{
		HANDLE startup_wait_handles[2] = { new_runtime->host_ready_event, new_runtime->host_thread };
		DWORD startup_wait = WaitForMultipleObjects(2, startup_wait_handles, FALSE, kRuntimeHostCommandTimeoutMs);

		if (startup_wait != WAIT_OBJECT_0)
		{
			InterlockedExchange(&new_runtime->host_faulted, 1);
			if (startup_wait == WAIT_TIMEOUT)
				TerminateThread(new_runtime->host_thread, 1);
			WaitForSingleObject(new_runtime->host_thread, 500);
			CloseHandle(new_runtime->host_thread);
			CloseHandle(new_runtime->host_ready_event);
			CloseHandle(new_runtime->command_event);
			CloseHandle(new_runtime->command_complete_event);
			DeleteCriticalSection(&new_runtime->api_lock);
			DeleteCriticalSection(&new_runtime->command_lock);
			return startup_wait == WAIT_TIMEOUT ? VST_HOST_IPC_RESULT_CONTROL_TIMEOUT : VST_HOST_IPC_RESULT_RUNTIME_THREAD_EXITED;
		}
	}

	EnterCriticalSection(&new_runtime->command_lock);
	new_runtime->command_type = VstPluginRuntime::HostCommandInitialize;
	new_runtime->command_plugin_path = plugin_path;
	new_runtime->command_plugin_name = plugin_name;
	new_runtime->command_plugin_name_count = plugin_name_count;
	new_runtime->command_sample_rate = sample_rate;
	new_runtime->command_max_block_size = max_block_size;
	new_runtime->command_num_channels = num_channels;
	result = send_host_command(*new_runtime);
	LeaveCriticalSection(&new_runtime->command_lock);
	if (result != 0)
	{
		EnterCriticalSection(&new_runtime->command_lock);
		new_runtime->command_type = VstPluginRuntime::HostCommandShutdown;
		send_host_command(*new_runtime);
		LeaveCriticalSection(&new_runtime->command_lock);
		if (WaitForSingleObject(new_runtime->host_thread, kRuntimeHostShutdownTimeoutMs) == WAIT_TIMEOUT)
			TerminateThread(new_runtime->host_thread, 1);
		WaitForSingleObject(new_runtime->host_thread, 500);
		CloseHandle(new_runtime->host_thread);
		CloseHandle(new_runtime->host_ready_event);
		CloseHandle(new_runtime->command_event);
		CloseHandle(new_runtime->command_complete_event);
		DeleteCriticalSection(&new_runtime->api_lock);
		DeleteCriticalSection(&new_runtime->command_lock);
		return result;
	}

	runtime = new_runtime.release();
	return 0;
}

void VstRuntime_Retain(VstPluginRuntime* runtime)
{
	if (runtime)
		InterlockedIncrement(&runtime->ref_count);
}

void VstProcessingState_Retain(VstProcessingState* state)
{
	if (state)
		InterlockedIncrement(&state->ref_count);
}

void VstProcessingState_Release(VstProcessingState*& state)
{
	VstProcessingState* local_state = state;
	int i;

	state = 0;
	if (!local_state)
		return;

	if (InterlockedDecrement(&local_state->ref_count) != 0)
		return;

	for (i = 0; i < local_state->plugin_count; ++i)
		ThetisPluginRuntime_Destroy(local_state->plugins[i].runtime);

	delete local_state;
}

void VstRuntime_Destroy(VstPluginRuntime*& runtime)
{
	VstPluginRuntime* local_runtime = runtime;
	runtime = 0;

	if (!local_runtime)
		return;
	if (InterlockedDecrement(&local_runtime->ref_count) != 0)
		return;

	EnterCriticalSection(&local_runtime->command_lock);
	if (InterlockedCompareExchange(&local_runtime->host_faulted, 0, 0) == 0)
	{
		local_runtime->command_type = VstPluginRuntime::HostCommandShutdown;
		send_host_command(*local_runtime);
	}
	LeaveCriticalSection(&local_runtime->command_lock);

	if (WaitForSingleObject(local_runtime->host_thread, kRuntimeHostShutdownTimeoutMs) == WAIT_TIMEOUT)
		TerminateThread(local_runtime->host_thread, 1);
	WaitForSingleObject(local_runtime->host_thread, 500);
	CloseHandle(local_runtime->host_thread);
	CloseHandle(local_runtime->host_ready_event);
	CloseHandle(local_runtime->command_event);
	CloseHandle(local_runtime->command_complete_event);
	DeleteCriticalSection(&local_runtime->api_lock);
	DeleteCriticalSection(&local_runtime->command_lock);
	delete local_runtime;
}

int VstRuntime_Reconfigure(VstPluginRuntime* runtime, int sample_rate, int max_block_size, int num_channels)
{
	if (!runtime || !runtime->initialized)
		return -1;

	EnterCriticalSection(&runtime->command_lock);
	runtime->command_type = VstPluginRuntime::HostCommandReconfigure;
	runtime->command_sample_rate = sample_rate;
	runtime->command_max_block_size = max_block_size;
	runtime->command_num_channels = num_channels;
	sample_rate = send_host_command(*runtime);
	LeaveCriticalSection(&runtime->command_lock);
	return sample_rate;
}

int VstRuntime_Process(VstPluginRuntime* runtime, double* interleaved_buffer, int frames, int chain_channels)
{
	Steinberg::tresult result;
	ScopedTryRuntimeApiLock api_lock(runtime);

	if (!runtime || !runtime->processor || !runtime->active || !interleaved_buffer)
		return -1;
	if (frames <= 0 || frames > runtime->max_block_size)
		return -2;
	if (chain_channels <= 0)
		return -3;
	if (!api_lock.is_locked())
		return 0;

	fill_input_buffers(*runtime, interleaved_buffer, frames, chain_channels);
	prepare_output_buffers(*runtime, frames);
	assign_process_buffers(*runtime);
	runtime->input_parameter_changes.clearQueue();
	runtime->output_parameter_changes.clearQueue();
	runtime->input_parameter_transfer.transferChangesTo(runtime->input_parameter_changes);

	runtime->process_data.numSamples = frames;
	runtime->process_data.processContext = &runtime->process_context;
	result = runtime->processor->process(runtime->process_data);
	if (!is_ok(result))
		return -4;

	// Buffer output parameter changes (metering, visualization) for the
	// host thread to forward to the controller.  setParamNormalized must
	// be called from the UI thread per VST3 spec, so the audio thread
	// writes to a lock-free cache that the host thread drains at ~30 Hz.
	//
	// Lock-free protocol:
	//   - output_param_cache_count only grows (audio thread increments,
	//     never decremented).  Each slot's id is written once when claimed.
	//   - Audio thread: writes value, then sets active=1 via interlocked
	//     (full barrier ensures value is visible before active).
	//   - Host thread: clears active=0 via interlocked after reading value
	//     (full barrier ensures value is read before active is cleared).
	//   - Host thread never modifies count or id fields.
	if (runtime->output_parameter_changes.getParameterCount() > 0)
	{
		LONG cache_count = InterlockedCompareExchange(&runtime->output_param_cache_count, 0, 0);
		for (Steinberg::int32 i = 0; i < runtime->output_parameter_changes.getParameterCount(); ++i)
		{
			Steinberg::Vst::IParamValueQueue* queue = runtime->output_parameter_changes.getParameterData(i);
			if (!queue || queue->getPointCount() <= 0)
				continue;

			Steinberg::Vst::ParamValue value;
			Steinberg::int32 sampleOffset;
			queue->getPoint(queue->getPointCount() - 1, sampleOffset, value);
			Steinberg::Vst::ParamID id = queue->getParameterId();

			// Update existing slot or claim a new one.
			bool found = false;
			for (LONG j = 0; j < cache_count; ++j)
			{
				if (runtime->output_param_cache[j].id == id)
				{
					runtime->output_param_cache[j].value = value;
					InterlockedExchange(&runtime->output_param_cache[j].active, 1);
					found = true;
					break;
				}
			}
			if (!found && cache_count < kMaxOutputParamCacheEntries)
			{
				LONG slot = cache_count;
				runtime->output_param_cache[slot].id = id;
				runtime->output_param_cache[slot].value = value;
				InterlockedExchange(&runtime->output_param_cache[slot].active, 1);
				InterlockedIncrement(&runtime->output_param_cache_count);
				++cache_count;
			}
		}
	}

	write_output_buffers(*runtime, interleaved_buffer, frames, chain_channels);
	return 0;
}

int VstRuntime_GetStateSize(VstPluginRuntime* runtime)
{
	int result;

	if (!runtime || !runtime->initialized)
		return -1;

	EnterCriticalSection(&runtime->command_lock);
	runtime->command_type = VstPluginRuntime::HostCommandGetStateSize;
	result = send_host_command(*runtime);
	LeaveCriticalSection(&runtime->command_lock);
	return result;
}

int VstRuntime_GetState(VstPluginRuntime* runtime, void* buffer, int buffer_size, int* bytes_written)
{
	int result;

	if (bytes_written)
		*bytes_written = 0;
	if (!runtime || !runtime->initialized)
		return -1;

	EnterCriticalSection(&runtime->command_lock);
	runtime->command_type = VstPluginRuntime::HostCommandGetState;
	runtime->command_state_buffer = buffer;
	runtime->command_state_buffer_size = buffer_size;
	runtime->command_bytes_written = bytes_written;
	result = send_host_command(*runtime);
	runtime->command_state_buffer = 0;
	runtime->command_state_buffer_size = 0;
	runtime->command_bytes_written = 0;
	LeaveCriticalSection(&runtime->command_lock);
	return result;
}

int VstRuntime_SetState(VstPluginRuntime* runtime, const void* buffer, int buffer_size)
{
	int result;

	if (!runtime || !runtime->initialized)
		return -1;
	if (!buffer || buffer_size <= 0)
		return -2;

	EnterCriticalSection(&runtime->command_lock);
	runtime->command_type = VstPluginRuntime::HostCommandSetState;
	runtime->command_state_buffer = const_cast<void*>(buffer);
	runtime->command_state_buffer_size = buffer_size;
	runtime->command_bytes_written = 0;
	result = send_host_command(*runtime);
	runtime->command_state_buffer = 0;
	runtime->command_state_buffer_size = 0;
	LeaveCriticalSection(&runtime->command_lock);
	return result;
}

void VstRuntime_SetStateDirtyCallback(VstPluginRuntime* runtime, VstRuntimeStateDirtyCallback callback, void* context)
{
	if (!runtime)
		return;

	runtime->state_dirty_callback = callback;
	runtime->state_dirty_context = context;
}

int VstRuntime_ProbePluginMetadataOnly(const wchar_t* plugin_path, VstPluginProbeInfo* info)
{
	std::string module_path;
	std::string error_description;
	VST3::Hosting::PluginFactory factory(nullptr);
	VST3::Hosting::PluginFactory::ClassInfos class_infos;
	std::shared_ptr<VST3::Hosting::Module> module;
	HMODULE preloaded = 0;
	size_t i;

	clear_probe_info(info);
	if (!plugin_path || !plugin_path[0] || !info)
		return -1;
	if (GetFileAttributesW(plugin_path) == INVALID_FILE_ATTRIBUTES)
		return -2;

	module_path = wide_to_utf8(plugin_path);
	if (module_path.empty())
		return -3;

	// Pre-load the DLL with LOAD_WITH_ALTERED_SEARCH_PATH so that
	// dependent vendor DLLs adjacent to the plugin are resolved from
	// the plugin's own directory rather than the scanner's directory.
	// Module::create will then re-use the already loaded module.
	wcsncpy_s(info->path, VST_MAX_PLUGIN_PATH_CHARS, plugin_path, _TRUNCATE);

	wchar_t dll_path[MAX_PATH * 2];
	if (!resolve_plugin_dll_path(plugin_path, dll_path, _countof(dll_path)))
		return -4;

	preloaded = LoadLibraryExW(dll_path, NULL, LOAD_WITH_ALTERED_SEARCH_PATH);
	if (!preloaded)
		return -4;

	module = VST3::Hosting::Module::create(module_path, error_description);
	FreeLibrary(preloaded);
	if (!module)
		return -4;

	factory = module->getFactory();
	class_infos = factory.classInfos();

	for (i = 0; i < class_infos.size(); ++i)
	{
		const VST3::Hosting::ClassInfo& class_info = class_infos[i];
		std::string subcategories;
		bool has_fx_subcategory;
		bool instrument_only;

		if (class_info.category() != kVstAudioEffectClass)
			continue;

		subcategories = class_info.subCategoriesString();
		has_fx_subcategory = subcategories.find("Fx") != std::string::npos;
		instrument_only = subcategories.find("Instrument") != std::string::npos && !has_fx_subcategory;
		if (instrument_only)
			continue;

		info->is_audio_effect = 1;
		info->is_valid = 1;
		info->has_audio_input = 1;
		info->has_audio_output = 1;
		utf8_to_wide(class_info.name(), info->name, VST_MAX_PLUGIN_NAME_CHARS);
		utf8_to_wide(class_info.vendor(), info->vendor, VST_MAX_PLUGIN_VENDOR_CHARS);
		utf8_to_wide(class_info.version(), info->version, VST_MAX_PLUGIN_VERSION_CHARS);
		utf8_to_wide(subcategories, info->subcategories, VST_MAX_PLUGIN_SUBCATEGORY_CHARS);
		return 0;
	}

	/* Fill in name/vendor from the first class even if it's not a
	   suitable audio effect, so the scanner can display it. */
	if (class_infos.size() > 0 && info->name[0] == L'\0')
	{
		utf8_to_wide(class_infos[0].name(), info->name, VST_MAX_PLUGIN_NAME_CHARS);
		utf8_to_wide(class_infos[0].vendor(), info->vendor, VST_MAX_PLUGIN_VENDOR_CHARS);
		utf8_to_wide(class_infos[0].version(), info->version, VST_MAX_PLUGIN_VERSION_CHARS);
		utf8_to_wide(class_infos[0].subCategoriesString(), info->subcategories, VST_MAX_PLUGIN_SUBCATEGORY_CHARS);
	}

	return info->is_audio_effect ? -5 : -6;
}

int VstRuntime_OpenEditor(VstPluginRuntime* runtime, VstProcessingState* owner_state, int plugin_index, HWND parent_window, int& width, int& height, int& can_resize, VstEditorSession*& session)
{
	int result;

	session = 0;
	width = 0;
	height = 0;
	can_resize = 0;

	trace_editor("open begin plugin_index=%d runtime=%p parent=%p", plugin_index, runtime, parent_window);
	if (!runtime || !runtime->controller)
		return -1;
	(void)parent_window;
	EnterCriticalSection(&runtime->command_lock);
	runtime->command_type = VstPluginRuntime::HostCommandOpenEditor;
	runtime->command_owner_state = owner_state;
	runtime->command_plugin_index = plugin_index;
	runtime->command_width = 0;
	runtime->command_height = 0;
	runtime->command_can_resize = 0;
	runtime->command_session = 0;
	result = send_host_command(*runtime);
	session = runtime->command_session;
	width = runtime->command_width;
	height = runtime->command_height;
	can_resize = runtime->command_can_resize;
	LeaveCriticalSection(&runtime->command_lock);
	trace_editor("open complete session=%p result=%d", session, result);
	return result;
}

VstProcessingState* VstRuntime_GetEditorOwnerState(VstEditorSession* session)
{
	return session ? session->owner_state : 0;
}

int VstRuntime_CloseOpenEditor(VstPluginRuntime* runtime)
{
	int result;

	if (!runtime)
		return 0;

	EnterCriticalSection(&runtime->command_lock);
	runtime->command_type = VstPluginRuntime::HostCommandCloseOpenEditor;
	runtime->command_session = 0;
	result = send_host_command(*runtime);
	LeaveCriticalSection(&runtime->command_lock);
	return result;
}

int VstRuntime_CloseEditor(VstEditorSession*& session)
{
	VstPluginRuntime* runtime;

	if (!session)
		return 0;

	runtime = session->runtime;
	if (!runtime)
	{
		destroy_editor_session(session, true);
		return 0;
	}
	EnterCriticalSection(&runtime->command_lock);
	runtime->command_type = VstPluginRuntime::HostCommandCloseEditor;
	runtime->command_session = session;
	runtime->command_result = send_host_command(*runtime);
	LeaveCriticalSection(&runtime->command_lock);
	session = 0;
	return runtime->command_result;
}

int VstRuntime_ResizeEditor(VstEditorSession* session, int width, int height)
{
	if (!session)
		return -1;

	if (!session->runtime)
		return -1;
	EnterCriticalSection(&session->runtime->command_lock);
	session->runtime->command_type = VstPluginRuntime::HostCommandResizeEditor;
	session->runtime->command_session = session;
	session->runtime->command_width = width;
	session->runtime->command_height = height;
	width = send_host_command(*session->runtime);
	LeaveCriticalSection(&session->runtime->command_lock);
	return width;
}
