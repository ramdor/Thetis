#include "vst2_runtime.h"
#include "vst_runtime.h"
#include "vst_scoped_lock.h"

#include "..\..\lib\vst2sdk\include\vst.h"

#include <Ole2.h>
#include <algorithm>
#include <cmath>
#include <cstdarg>
#include <cstdio>
#include <cstdint>
#include <cstring>
#include <cwchar>
#include <memory>
#include <new>
#include <process.h>
#include <string>
#include <vector>

/*
VST2 runtime wrapper.

Unlike VST3, many VST2 plugins are sensitive to thread ownership. This runtime
keeps plugin creation, editor work, and control commands on a persistent host
thread, while audio uses a try-lock against the same API surface.
*/

namespace
{
	volatile LONG g_vst2_editor_trace_enabled = -1;
	__declspec(thread) DWORD g_vst2_last_plugin_exception_code = 0;
	const int kVst2DispatchExceptionResult = (-2147483647 - 1);

	bool vst2_editor_trace_enabled()
	{
		LONG enabled = InterlockedCompareExchange(&g_vst2_editor_trace_enabled, -1, -1);
		wchar_t value[8] = {};

		if (enabled != -1)
			return enabled != 0;

		enabled = 0;
		if (GetEnvironmentVariableW(L"THETIS_VST_OOP_TRACE", value, _countof(value)) > 0)
			enabled = 1;

		InterlockedExchange(&g_vst2_editor_trace_enabled, enabled);
		return enabled != 0;
	}

	void trace_vst2_editor(const wchar_t* format, ...)
	{
		wchar_t message[768] = {};
		wchar_t log_path[MAX_PATH] = {};
		FILE* log_file = 0;
		va_list args;

		if (!vst2_editor_trace_enabled())
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

	void clear_last_vst2_plugin_exception()
	{
		g_vst2_last_plugin_exception_code = 0;
	}

	DWORD consume_last_vst2_plugin_exception()
	{
		DWORD code = g_vst2_last_plugin_exception_code;
		g_vst2_last_plugin_exception_code = 0;
		return code;
	}
}

struct Vst2PluginRuntime
{
	Vst2PluginRuntime()
	{
		InitializeCriticalSection(&api_lock);
		InitializeCriticalSection(&command_lock);
	}

	~Vst2PluginRuntime()
	{
		DeleteCriticalSection(&command_lock);
		DeleteCriticalSection(&api_lock);
	}

	enum HostCommandType
	{
		HostCommandNone,
		HostCommandInitialize,
		HostCommandReconfigure,
		HostCommandOpenEditor,
		HostCommandCloseEditor,
		HostCommandCloseOpenEditor,
		HostCommandResizeEditor,
		HostCommandShutdown
	};

	volatile LONG ref_count = 1;
	CRITICAL_SECTION api_lock = {};
	CRITICAL_SECTION command_lock = {};
	std::wstring plugin_path;
	std::wstring plugin_directory;
	std::wstring plugin_name;
	std::string plugin_directory_ansi;
	HMODULE module_handle = 0;
	vst_effect_t* effect = nullptr;
	int sample_rate = 0;
	int max_block_size = 0;
	int chain_channels = 0;
	int input_channels = 0;
	int output_channels = 0;
	int plugin_category = 0;
	bool initialized = false;
	bool active = false;
	bool supports_double = false;
	bool supports_float = false;
	bool uses_chunks = false;
	void* editor_session = nullptr;
	HANDLE host_thread = 0;
	HANDLE host_ready_event = 0;
	HANDLE command_event = 0;
	HANDLE command_complete_event = 0;
	DWORD host_thread_id = 0;
	HostCommandType command_type = HostCommandNone;
	int command_result = 0;
	const wchar_t* command_plugin_path = 0;
	wchar_t* command_plugin_name = 0;
	size_t command_plugin_name_count = 0;
	HWND command_parent_window = 0;
	int command_sample_rate = 0;
	int command_max_block_size = 0;
	int command_num_channels = 0;
	VstEditorSession* command_session = 0;
	int command_width = 0;
	int command_height = 0;
	int command_can_resize = 0;
	volatile LONG host_faulted = 0;
	Vst2RuntimeStateDirtyCallback state_dirty_callback = nullptr;
	void* state_dirty_context = nullptr;
	std::vector<std::vector<double>> input_buffers_64;
	std::vector<std::vector<double>> output_buffers_64;
	std::vector<std::vector<float>> input_buffers_32;
	std::vector<std::vector<float>> output_buffers_32;
	std::vector<const double*> input_ptrs_64;
	std::vector<double*> output_ptrs_64;
	std::vector<const float*> input_ptrs_32;
	std::vector<float*> output_ptrs_32;
};

struct Vst2EditorSession
{
	uint32_t magic = 0x32534544; // "2SED"
	Vst2PluginRuntime* runtime = nullptr;
	HWND window = 0;
	HWND container_window = 0;
	HWND parent_window = 0;
	int width = 0;
	int height = 0;
	bool can_resize = false;
	bool editor_open = false;
};

namespace
{
	__declspec(thread) Vst2PluginRuntime* g_vst2_runtime_create_context = nullptr;
	const uint32_t kVst2StateBlobMagic = 0x32545356; // VST2
	const uint32_t kVst2StateBlobVersion = 1;
	const uint32_t kVst2StateBlobFlagChunks = 0x1;
	const int kVst2MaxSupportedChannels = 2;
	const DWORD kVst2HostCommandTimeoutMs = 5000;
	const DWORD kVst2HostEditorCommandTimeoutMs = 12000;
	const DWORD kVst2HostShutdownTimeoutMs = 2000;
	INIT_ONCE g_vst2_editor_window_class_once = INIT_ONCE_STATIC_INIT;
	INIT_ONCE g_vst2_editor_container_window_class_once = INIT_ONCE_STATIC_INIT;
	const wchar_t* g_vst2_editor_window_class_name = L"ThetisVst2EditorHostWindow";
	const wchar_t* g_vst2_editor_container_window_class_name = L"ThetisVst2EditorContainerWindow";

	typedef vst_effect_t* (VST_FUNCTION_INTERFACE* vst2_entrypoint_t)(vst_host_callback_t callback);

	struct Vst2StateBlobHeader
	{
		uint32_t magic;
		uint32_t version;
		uint32_t flags;
		int32_t program_index;
		uint32_t param_count;
		uint32_t payload_size;
	};

	void shutdown_runtime_host(Vst2PluginRuntime& runtime);

	// ScopedRuntimeApiLock and ScopedTryRuntimeApiLock are defined in
	// vst_scoped_lock.h and shared with vst_runtime.cpp.

	std::wstring extract_directory_path(const wchar_t* plugin_path)
	{
		const wchar_t* slash;

		if (!plugin_path || !plugin_path[0])
			return std::wstring();

		slash = wcsrchr(plugin_path, L'\\');
		if (!slash)
			slash = wcsrchr(plugin_path, L'/');
		if (!slash)
			return std::wstring();

		return std::wstring(plugin_path, static_cast<size_t>(slash - plugin_path));
	}

	std::wstring extract_file_stem(const wchar_t* plugin_path)
	{
		const wchar_t* slash;
		const wchar_t* dot;
		const wchar_t* start;

		if (!plugin_path || !plugin_path[0])
			return std::wstring();

		slash = wcsrchr(plugin_path, L'\\');
		if (!slash)
			slash = wcsrchr(plugin_path, L'/');
		start = slash ? slash + 1 : plugin_path;
		dot = wcsrchr(start, L'.');
		if (!dot || dot <= start)
			return std::wstring(start);

		return std::wstring(start, static_cast<size_t>(dot - start));
	}

	std::string wide_to_ansi(const std::wstring& text)
	{
		int required;
		std::string result;

		if (text.empty())
			return result;

		required = WideCharToMultiByte(CP_ACP, 0, text.c_str(), -1, nullptr, 0, nullptr, nullptr);
		if (required <= 1)
			return result;

		result.resize(static_cast<size_t>(required - 1));
		WideCharToMultiByte(CP_ACP, 0, text.c_str(), -1, &result[0], required - 1, nullptr, nullptr);
		return result;
	}

	void ansi_to_wide(const char* text, wchar_t* destination, size_t destination_count)
	{
		if (!destination || destination_count == 0)
			return;

		destination[0] = L'\0';
		if (!text || !text[0])
			return;

		MultiByteToWideChar(CP_ACP, 0, text, -1, destination, static_cast<int>(destination_count));
		destination[destination_count - 1] = L'\0';
	}

	void notify_runtime_state_dirty(Vst2PluginRuntime* runtime)
	{
		if (runtime && runtime->state_dirty_callback)
			runtime->state_dirty_callback(runtime->state_dirty_context);
	}

	bool string_equals_ansi(const char* left, const char* right)
	{
		return left && right && strcmp(left, right) == 0;
	}

	bool set_editor_window_size(Vst2EditorSession* session, int width, int height)
	{
		RECT rect = {};
		DWORD style;
		DWORD ex_style;

		if (!session || !session->window || width <= 0 || height <= 0)
			return false;

		style = static_cast<DWORD>(GetWindowLongPtrW(session->window, GWL_STYLE));
		ex_style = static_cast<DWORD>(GetWindowLongPtrW(session->window, GWL_EXSTYLE));
		rect.left = 0;
		rect.top = 0;
		rect.right = width;
		rect.bottom = height;
		AdjustWindowRectEx(&rect, style, FALSE, ex_style);

		session->width = width;
		session->height = height;
		if (SetWindowPos(
			session->window,
			nullptr,
			0,
			0,
			rect.right - rect.left,
			rect.bottom - rect.top,
			SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE) == FALSE)
			return false;

		if (session->container_window)
		{
			RECT client_rect = {};
			if (GetClientRect(session->window, &client_rect))
			{
				MoveWindow(
					session->container_window,
					0,
					0,
					client_rect.right - client_rect.left,
					client_rect.bottom - client_rect.top,
					TRUE);
			}
		}

		return true;
	}

	Vst2PluginRuntime* resolve_runtime_context(vst_effect_t* effect)
	{
		if (effect && effect->host_internal)
			return static_cast<Vst2PluginRuntime*>(effect->host_internal);
		return g_vst2_runtime_create_context;
	}

	intptr_t VST_FUNCTION_INTERFACE vst2_host_callback(vst_effect_t* effect, int32_t opcode, int32_t p_int1, int64_t p_int2, const char* p_str, float p_float)
	{
		Vst2PluginRuntime* runtime = resolve_runtime_context(effect);

		(void)p_int1;
		(void)p_int2;
		(void)p_float;

		switch (opcode)
		{
		case VST_HOST_OPCODE_VST_VERSION:
			return VST_VERSION_2_4_0_0;
		case VST_HOST_OPCODE_GET_SAMPLE_RATE:
			return runtime ? runtime->sample_rate : 0;
		case VST_HOST_OPCODE_GET_BLOCK_SIZE:
			return runtime ? runtime->max_block_size : 0;
		case VST_HOST_OPCODE_INPUT_LATENCY:
		case VST_HOST_OPCODE_OUTPUT_LATENCY:
			return 0;
		case VST_HOST_OPCODE_VENDOR_VERSION:
			return 1;
		case VST_HOST_OPCODE_CURRENT_EFFECT_ID:
			return 0;
		case VST_HOST_OPCODE_VENDOR_NAME:
			if (p_str)
				strcpy_s(const_cast<char*>(p_str), VST_BUFFER_SIZE_VENDOR_NAME, "Thetis");
			return VST_STATUS_TRUE;
		case VST_HOST_OPCODE_PRODUCT_NAME:
			if (p_str)
				strcpy_s(const_cast<char*>(p_str), VST_BUFFER_SIZE_PRODUCT_NAME, "Thetis");
			return VST_STATUS_TRUE;
		case VST_HOST_OPCODE_GET_EFFECT_DIRECTORY:
			return runtime && !runtime->plugin_directory_ansi.empty()
				? reinterpret_cast<intptr_t>(runtime->plugin_directory_ansi.c_str())
				: 0;
		case VST_HOST_OPCODE_SUPPORTS:
			if (string_equals_ansi(p_str, "startStopProcess"))
				return VST_STATUS_TRUE;
			if (string_equals_ansi(p_str, "sizeWindow"))
				return VST_STATUS_TRUE;
			if (string_equals_ansi(p_str, "receiveVstEvents"))
				return VST_STATUS_FALSE;
			if (string_equals_ansi(p_str, "receiveVstMidiEvent"))
				return VST_STATUS_FALSE;
			return VST_STATUS_FALSE;
		case VST_HOST_OPCODE_PARAM_UPDATE:
		case VST_HOST_OPCODE_PARAM_START_EDIT:
		case VST_HOST_OPCODE_PARAM_STOP_EDIT:
		case VST_HOST_OPCODE_EDITOR_UPDATE:
			notify_runtime_state_dirty(runtime);
			return VST_STATUS_TRUE;
		case VST_HOST_OPCODE_EDITOR_RESIZE:
		{
			Vst2EditorSession* session = runtime
				? static_cast<Vst2EditorSession*>(InterlockedCompareExchangePointer(reinterpret_cast<PVOID*>(&runtime->editor_session), nullptr, nullptr))
				: nullptr;
			if (set_editor_window_size(session, p_int1, static_cast<int>(p_int2)))
			{
				if (session)
					session->can_resize = true;
				return VST_STATUS_TRUE;
			}
			return VST_STATUS_FALSE;
		}
		default:
			return 0;
		}
	}

	int dispatcher(vst_effect_t* effect, int32_t opcode, int32_t p_int1, intptr_t p_int2, void* p_ptr, float p_float)
	{
		if (!effect || !effect->control)
			return 0;

		clear_last_vst2_plugin_exception();
		__try
		{
			return static_cast<int>(effect->control(effect, opcode, p_int1, p_int2, p_ptr, p_float));
		}
		__except (EXCEPTION_EXECUTE_HANDLER)
		{
			g_vst2_last_plugin_exception_code = 1;
			trace_vst2_editor(
				L"[VST2 runtime] effect=%p opcode=%d exception=0x%08x\r\n",
				effect,
				static_cast<int>(opcode),
				static_cast<unsigned int>(g_vst2_last_plugin_exception_code));
			return kVst2DispatchExceptionResult;
		}
	}

	bool create_effect_instance(vst2_entrypoint_t entry, vst_effect_t*& effect, const wchar_t* plugin_label)
	{
		effect = nullptr;
		clear_last_vst2_plugin_exception();
		__try
		{
			effect = entry ? entry(vst2_host_callback) : nullptr;
		}
		__except (EXCEPTION_EXECUTE_HANDLER)
		{
			g_vst2_last_plugin_exception_code = 1;
			trace_vst2_editor(
				L"[VST2 runtime] plugin=\"%s\" create exception=0x%08x\r\n",
				plugin_label && plugin_label[0] ? plugin_label : L"(unknown)",
				static_cast<unsigned int>(g_vst2_last_plugin_exception_code));
			effect = nullptr;
		}
		return effect != nullptr;
	}

	bool get_parameter_safe(Vst2PluginRuntime* runtime, uint32_t index, float& value)
	{
		if (!runtime || !runtime->effect || !runtime->effect->get_parameter)
			return false;

		clear_last_vst2_plugin_exception();
		__try
		{
			value = runtime->effect->get_parameter(runtime->effect, index);
			return true;
		}
		__except (EXCEPTION_EXECUTE_HANDLER)
		{
			g_vst2_last_plugin_exception_code = 1;
			trace_vst2_editor(
				L"[VST2 runtime] plugin=\"%s\" get_parameter index=%u exception=0x%08x\r\n",
				runtime->plugin_name.empty() ? L"(unnamed)" : runtime->plugin_name.c_str(),
				static_cast<unsigned int>(index),
				static_cast<unsigned int>(g_vst2_last_plugin_exception_code));
			return false;
		}
	}

	bool set_parameter_safe(Vst2PluginRuntime* runtime, uint32_t index, float value)
	{
		if (!runtime || !runtime->effect || !runtime->effect->set_parameter)
			return false;

		clear_last_vst2_plugin_exception();
		__try
		{
			runtime->effect->set_parameter(runtime->effect, index, value);
			return true;
		}
		__except (EXCEPTION_EXECUTE_HANDLER)
		{
			g_vst2_last_plugin_exception_code = 1;
			trace_vst2_editor(
				L"[VST2 runtime] plugin=\"%s\" set_parameter index=%u exception=0x%08x\r\n",
				runtime->plugin_name.empty() ? L"(unnamed)" : runtime->plugin_name.c_str(),
				static_cast<unsigned int>(index),
				static_cast<unsigned int>(g_vst2_last_plugin_exception_code));
			return false;
		}
	}

	bool process_audio_safe(Vst2PluginRuntime* runtime, int frames)
	{
		if (!runtime || !runtime->effect)
			return false;

		clear_last_vst2_plugin_exception();
		__try
		{
			if (runtime->supports_double)
				runtime->effect->process_double(runtime->effect, runtime->input_ptrs_64.data(), runtime->output_ptrs_64.data(), frames);
			else if (runtime->supports_float)
				runtime->effect->process_float(runtime->effect, runtime->input_ptrs_32.data(), runtime->output_ptrs_32.data(), frames);
			else
				runtime->effect->process(runtime->effect, runtime->input_ptrs_32.data(), runtime->output_ptrs_32.data(), frames);
			return true;
		}
		__except (EXCEPTION_EXECUTE_HANDLER)
		{
			g_vst2_last_plugin_exception_code = 1;
			trace_vst2_editor(
				L"[VST2 runtime] plugin=\"%s\" process exception=0x%08x\r\n",
				runtime->plugin_name.empty() ? L"(unnamed)" : runtime->plugin_name.c_str(),
				static_cast<unsigned int>(g_vst2_last_plugin_exception_code));

			// Zero output buffers so the caller does not read corrupted samples.
			if (runtime->supports_double)
			{
				for (size_t ch = 0; ch < runtime->output_buffers_64.size(); ++ch)
					memset(runtime->output_buffers_64[ch].data(), 0, static_cast<size_t>(frames) * sizeof(double));
			}
			else
			{
				for (size_t ch = 0; ch < runtime->output_buffers_32.size(); ++ch)
					memset(runtime->output_buffers_32[ch].data(), 0, static_cast<size_t>(frames) * sizeof(float));
			}
			return false;
		}
	}

	bool query_plugin_name(vst_effect_t* effect, wchar_t* plugin_name, size_t plugin_name_count, const wchar_t* fallback_path)
	{
		char buffer[256] = {};

		if (plugin_name && plugin_name_count > 0)
			plugin_name[0] = L'\0';

		if (!effect)
			return false;

		dispatcher(effect, VST_EFFECT_OPCODE_PRODUCT_NAME, 0, 0, buffer, 0.0f);
		if (consume_last_vst2_plugin_exception() != 0)
			buffer[0] = '\0';
		if (!buffer[0])
		{
			dispatcher(effect, VST_EFFECT_OPCODE_EFFECT_NAME, 0, 0, buffer, 0.0f);
			if (consume_last_vst2_plugin_exception() != 0)
				buffer[0] = '\0';
		}

		if (buffer[0] && plugin_name && plugin_name_count > 0)
		{
			ansi_to_wide(buffer, plugin_name, plugin_name_count);
			return plugin_name[0] != L'\0';
		}

		if (plugin_name && plugin_name_count > 0)
		{
			std::wstring fallback_name = extract_file_stem(fallback_path);
			if (!fallback_name.empty())
			{
				wcsncpy_s(plugin_name, plugin_name_count, fallback_name.c_str(), _TRUNCATE);
				return true;
			}
		}

		return false;
	}

	int get_chunk_state(Vst2PluginRuntime* runtime, void*& chunk)
	{
		chunk = nullptr;
		if (!runtime)
			return 0;

		ScopedRuntimeApiLock api_lock(runtime);
		return dispatcher(runtime->effect, VST_EFFECT_OPCODE_GET_CHUNK_DATA, 0, 0, &chunk, 0.0f);
	}

	bool is_supported_effect(vst_effect_t* effect, int& category)
	{
		if (!effect)
			return false;
		if (effect->magic_number != VST_MAGICNUMBER)
			return false;
		if (!effect->control || (!effect->process_double && !effect->process_float && !effect->process))
			return false;
		if (effect->num_inputs <= 0 || effect->num_outputs <= 0)
			return false;
		if (effect->num_inputs > kVst2MaxSupportedChannels || effect->num_outputs > kVst2MaxSupportedChannels)
			return false;

		category = dispatcher(effect, VST_EFFECT_OPCODE_CATEGORY, 0, 0, nullptr, 0.0f);
		if (consume_last_vst2_plugin_exception() != 0)
			return false;
		if ((effect->flags & VST_EFFECT_FLAG_INSTRUMENT) != 0)
			return false;
		if (category == VST_EFFECT_CATEGORY_INSTRUMENT ||
			category == VST_EFFECT_CATEGORY_WAVEGENERATOR ||
			category == VST_EFFECT_CATEGORY_CONTAINER ||
			category == VST_EFFECT_CATEGORY_OFFLINE)
			return false;

		return true;
	}

	bool allocate_processing_buffers(Vst2PluginRuntime& runtime)
	{
		int i;

		if (runtime.max_block_size <= 0 || runtime.input_channels <= 0 || runtime.output_channels <= 0)
			return false;

		runtime.input_buffers_64.assign(static_cast<size_t>(runtime.input_channels), std::vector<double>(static_cast<size_t>(runtime.max_block_size), 0.0));
		runtime.output_buffers_64.assign(static_cast<size_t>(runtime.output_channels), std::vector<double>(static_cast<size_t>(runtime.max_block_size), 0.0));
		runtime.input_buffers_32.assign(static_cast<size_t>(runtime.input_channels), std::vector<float>(static_cast<size_t>(runtime.max_block_size), 0.0f));
		runtime.output_buffers_32.assign(static_cast<size_t>(runtime.output_channels), std::vector<float>(static_cast<size_t>(runtime.max_block_size), 0.0f));
		runtime.input_ptrs_64.resize(static_cast<size_t>(runtime.input_channels));
		runtime.output_ptrs_64.resize(static_cast<size_t>(runtime.output_channels));
		runtime.input_ptrs_32.resize(static_cast<size_t>(runtime.input_channels));
		runtime.output_ptrs_32.resize(static_cast<size_t>(runtime.output_channels));

		for (i = 0; i < runtime.input_channels; ++i)
		{
			runtime.input_ptrs_64[i] = runtime.input_buffers_64[i].data();
			runtime.input_ptrs_32[i] = runtime.input_buffers_32[i].data();
		}

		for (i = 0; i < runtime.output_channels; ++i)
		{
			runtime.output_ptrs_64[i] = runtime.output_buffers_64[i].data();
			runtime.output_ptrs_32[i] = runtime.output_buffers_32[i].data();
		}

		return true;
	}

	void deactivate_runtime(Vst2PluginRuntime& runtime)
	{
		if (!runtime.effect || !runtime.initialized || !runtime.active)
			return;

		ScopedRuntimeApiLock api_lock(&runtime);
		dispatcher(runtime.effect, VST_EFFECT_OPCODE_PROCESS_END, 0, 0, nullptr, 0.0f);
		dispatcher(runtime.effect, VST_EFFECT_OPCODE_SUSPEND_RESUME, 0, VST_STATUS_FALSE, nullptr, 0.0f);
		runtime.active = false;
	}

	int activate_runtime(Vst2PluginRuntime& runtime)
	{
		if (!runtime.effect || !runtime.initialized)
			return -1;

		ScopedRuntimeApiLock api_lock(&runtime);
		dispatcher(runtime.effect, VST_EFFECT_OPCODE_SET_SAMPLE_RATE, 0, 0, nullptr, static_cast<float>(runtime.sample_rate));
		dispatcher(runtime.effect, VST_EFFECT_OPCODE_SET_BLOCK_SIZE, 0, runtime.max_block_size, nullptr, 0.0f);
		dispatcher(runtime.effect, VST_EFFECT_OPCODE_SUSPEND_RESUME, 0, VST_STATUS_TRUE, nullptr, 0.0f);
		dispatcher(runtime.effect, VST_EFFECT_OPCODE_PROCESS_BEGIN, 0, 0, nullptr, 0.0f);
		runtime.active = true;
		return 0;
	}

	void release_runtime(Vst2PluginRuntime& runtime)
	{
		if (runtime.effect)
		{
			{
				ScopedRuntimeApiLock api_lock(&runtime);
				if (runtime.active)
				{
					dispatcher(runtime.effect, VST_EFFECT_OPCODE_PROCESS_END, 0, 0, nullptr, 0.0f);
					dispatcher(runtime.effect, VST_EFFECT_OPCODE_SUSPEND_RESUME, 0, VST_STATUS_FALSE, nullptr, 0.0f);
					runtime.active = false;
				}
				dispatcher(runtime.effect, VST_EFFECT_OPCODE_DESTROY, 0, 0, nullptr, 0.0f);
			}
			runtime.effect = nullptr;
		}

		if (runtime.module_handle)
		{
			FreeLibrary(runtime.module_handle);
			runtime.module_handle = 0;
		}

		runtime.input_buffers_64.clear();
		runtime.output_buffers_64.clear();
		runtime.input_buffers_32.clear();
		runtime.output_buffers_32.clear();
		runtime.input_ptrs_64.clear();
		runtime.output_ptrs_64.clear();
		runtime.input_ptrs_32.clear();
		runtime.output_ptrs_32.clear();
		runtime.initialized = false;
		runtime.active = false;
	}

	int fail_create(std::unique_ptr<Vst2PluginRuntime>& runtime, int code)
	{
		if (runtime)
		{
			shutdown_runtime_host(*runtime);
			release_runtime(*runtime);
		}
		return code;
	}

	Vst2EditorSession* as_vst2_editor_session(VstEditorSession* session)
	{
		Vst2EditorSession* local_session = reinterpret_cast<Vst2EditorSession*>(session);
		if (!local_session || local_session->magic != 0x32534544)
			return nullptr;
		return local_session;
	}

	void clear_runtime_editor_session(Vst2PluginRuntime* runtime, Vst2EditorSession* session)
	{
		if (!runtime || !session)
			return;

		InterlockedCompareExchangePointer(reinterpret_cast<PVOID*>(&runtime->editor_session), nullptr, session);
	}

	void destroy_vst2_editor_session(Vst2EditorSession*& session)
	{
		Vst2EditorSession* local_session = session;

		session = nullptr;
		if (!local_session)
			return;

		clear_runtime_editor_session(local_session->runtime, local_session);

		if (local_session->runtime)
			Vst2Runtime_Destroy(local_session->runtime);
		delete local_session;
	}

	bool open_vst2_editor_session(Vst2EditorSession* session)
	{
		vst_rect_t* rect = nullptr;
		int open_result = -1;
		int fallback_open_result = -1;
		bool opened = false;

		if (!session || !session->runtime || !session->runtime->effect)
			return false;

		trace_vst2_editor(
			L"[VST2 editor] plugin=\"%s\" opening editor session\r\n",
			session->runtime->plugin_name.empty() ? L"(unnamed)" : session->runtime->plugin_name.c_str());

		auto query_editor_rect = [&]() -> bool
		{
			rect = nullptr;
			dispatcher(session->runtime->effect, VST_EFFECT_OPCODE_EDITOR_GET_RECT, 0, 0, &rect, 0.0f);
			if (consume_last_vst2_plugin_exception() != 0)
				rect = nullptr;
			return rect &&
				static_cast<int>(rect->right - rect->left) > 0 &&
				static_cast<int>(rect->bottom - rect->top) > 0;
		};

		auto finalize_editor_size = [&]()
		{
			if (!rect)
				return;

			int editor_width = (std::max)(1, static_cast<int>(rect->right - rect->left));
			int editor_height = (std::max)(1, static_cast<int>(rect->bottom - rect->top));
			if (editor_width != session->width || editor_height != session->height)
			{
				session->width = editor_width;
				session->height = editor_height;
				set_editor_window_size(session, editor_width, editor_height);
			}
		};

		auto try_open_editor = [&](HWND parent_window) -> bool
		{
			/*
			VST2 editor calls stay under the runtime API lock because the plugin may
			assume serialized dispatcher access between audio, state, and UI calls.
			*/
			ScopedRuntimeApiLock api_lock(session->runtime);
			trace_vst2_editor(
				L"[VST2 editor] plugin=\"%s\" calling effEditOpen parent=%p\r\n",
				session->runtime->plugin_name.empty() ? L"(unnamed)" : session->runtime->plugin_name.c_str(),
				parent_window);
			open_result = dispatcher(session->runtime->effect, VST_EFFECT_OPCODE_EDITOR_OPEN, 0, 0, parent_window, 0.0f);
			DWORD exception_code = consume_last_vst2_plugin_exception();
			if (exception_code != 0)
			{
				trace_vst2_editor(
					L"[VST2 editor] plugin=\"%s\" effEditOpen exception=0x%08x\r\n",
					session->runtime->plugin_name.empty() ? L"(unnamed)" : session->runtime->plugin_name.c_str(),
					static_cast<unsigned int>(exception_code));
				open_result = -11;
			}
			trace_vst2_editor(
				L"[VST2 editor] plugin=\"%s\" effEditOpen returned=%d\r\n",
				session->runtime->plugin_name.empty() ? L"(unnamed)" : session->runtime->plugin_name.c_str(),
				open_result);
			query_editor_rect();
			return open_result >= 0;
		};

		rect = nullptr;
		opened = try_open_editor(session->container_window);
		trace_vst2_editor(
			L"[VST2 editor] plugin=\"%s\" container open result=%d rect=%p child=%p\r\n",
			session->runtime->plugin_name.empty() ? L"(unnamed)" : session->runtime->plugin_name.c_str(),
			open_result,
			rect,
			session->container_window ? GetWindow(session->container_window, GW_CHILD) : nullptr);
		if (opened)
		{
			InvalidateRect(session->container_window, nullptr, TRUE);
			UpdateWindow(session->container_window);
			InvalidateRect(session->window, nullptr, TRUE);
			UpdateWindow(session->window);
		}

		if (!opened)
		{
			fallback_open_result = open_result;
			open_result = -1;
			rect = nullptr;
			dispatcher(session->runtime->effect, VST_EFFECT_OPCODE_EDITOR_CLOSE, 0, 0, nullptr, 0.0f);
			consume_last_vst2_plugin_exception();
			opened = try_open_editor(session->window);
			trace_vst2_editor(
				L"[VST2 editor] plugin=\"%s\" top-level fallback result=%d rect=%p child=%p\r\n",
				session->runtime->plugin_name.empty() ? L"(unnamed)" : session->runtime->plugin_name.c_str(),
				open_result,
				rect,
				session->window ? GetWindow(session->window, GW_CHILD) : nullptr);
			if (opened)
			{
				InvalidateRect(session->window, nullptr, TRUE);
				UpdateWindow(session->window);
			}
		}

		if (!opened)
		{
			int final_result = fallback_open_result < 0 ? fallback_open_result : (open_result < 0 ? open_result : -5);
			trace_vst2_editor(
				L"[VST2 editor] plugin=\"%s\" open failed final=%d first=%d second=%d\r\n",
				session->runtime->plugin_name.empty() ? L"(unnamed)" : session->runtime->plugin_name.c_str(),
				final_result,
				fallback_open_result,
				open_result);
			if (session->window)
				DestroyWindow(session->window);
			return false;
		}

		finalize_editor_size();
		session->editor_open = true;
		SetTimer(session->window, 1, 30, nullptr);
		trace_vst2_editor(
			L"[VST2 editor] plugin=\"%s\" open succeeded size=%dx%d resize=%d\r\n",
			session->runtime->plugin_name.empty() ? L"(unnamed)" : session->runtime->plugin_name.c_str(),
			session->width,
			session->height,
			session->can_resize ? 1 : 0);
		return true;
	}

	LRESULT CALLBACK vst2_editor_window_proc(HWND hwnd, UINT message, WPARAM w_param, LPARAM l_param)
	{
		Vst2EditorSession* session = reinterpret_cast<Vst2EditorSession*>(GetWindowLongPtrW(hwnd, GWLP_USERDATA));

		switch (message)
		{
		case WM_NCCREATE:
		{
			CREATESTRUCTW* create_struct = reinterpret_cast<CREATESTRUCTW*>(l_param);
			Vst2EditorSession* creating_session = create_struct ? static_cast<Vst2EditorSession*>(create_struct->lpCreateParams) : nullptr;
			if (creating_session)
			{
				SetWindowLongPtrW(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(creating_session));
				creating_session->window = hwnd;
			}
			return TRUE;
		}
		case WM_TIMER:
			if (session && session->runtime && session->runtime->effect && session->editor_open)
			{
				ScopedRuntimeApiLock api_lock(session->runtime);
				dispatcher(session->runtime->effect, VST_EFFECT_OPCODE_EDITOR_KEEP_ALIVE, 0, 0, nullptr, 0.0f);
				if (consume_last_vst2_plugin_exception() != 0)
				{
					session->editor_open = false;
					PostMessageW(hwnd, WM_CLOSE, 0, 0);
				}
			}
			return 0;
		case WM_SIZE:
			if (session && session->container_window)
			{
				RECT client_rect = {};
				if (GetClientRect(hwnd, &client_rect))
				{
					MoveWindow(
						session->container_window,
						0,
						0,
						client_rect.right - client_rect.left,
						client_rect.bottom - client_rect.top,
						TRUE);
				}
			}
			return 0;
		case WM_CLOSE:
			DestroyWindow(hwnd);
			return 0;
		case WM_DESTROY:
			if (session && session->runtime && session->runtime->effect && session->editor_open)
			{
				ScopedRuntimeApiLock api_lock(session->runtime);
				dispatcher(session->runtime->effect, VST_EFFECT_OPCODE_EDITOR_CLOSE, 0, 0, nullptr, 0.0f);
				consume_last_vst2_plugin_exception();
				session->editor_open = false;
			}
			KillTimer(hwnd, 1);
			clear_runtime_editor_session(session ? session->runtime : nullptr, session);
			return 0;
		}

		return DefWindowProcW(hwnd, message, w_param, l_param);
	}

	LRESULT CALLBACK vst2_editor_container_window_proc(HWND hwnd, UINT message, WPARAM w_param, LPARAM l_param)
	{
		switch (message)
		{
		case WM_ERASEBKGND:
			return 1;
		}

		return DefWindowProcW(hwnd, message, w_param, l_param);
	}

	BOOL CALLBACK ensure_vst2_editor_window_class(PINIT_ONCE, PVOID, PVOID*)
	{
		WNDCLASSEXW window_class = {};

		window_class.cbSize = sizeof(window_class);
		window_class.lpfnWndProc = vst2_editor_window_proc;
		window_class.hInstance = GetModuleHandleW(nullptr);
		window_class.hCursor = LoadCursorW(nullptr, IDC_ARROW);
		window_class.hIcon = LoadIconW(nullptr, IDI_APPLICATION);
		window_class.hbrBackground = reinterpret_cast<HBRUSH>(COLOR_BTNFACE + 1);
		window_class.lpszClassName = g_vst2_editor_window_class_name;
		return RegisterClassExW(&window_class) != 0 || GetLastError() == ERROR_CLASS_ALREADY_EXISTS;
	}

	BOOL CALLBACK ensure_vst2_editor_container_window_class(PINIT_ONCE, PVOID, PVOID*)
	{
		WNDCLASSEXW window_class = {};

		window_class.cbSize = sizeof(window_class);
		window_class.style = CS_OWNDC | CS_HREDRAW | CS_VREDRAW;
		window_class.lpfnWndProc = vst2_editor_container_window_proc;
		window_class.hInstance = GetModuleHandleW(nullptr);
		window_class.hCursor = LoadCursorW(nullptr, IDC_ARROW);
		window_class.hbrBackground = reinterpret_cast<HBRUSH>(COLOR_BTNFACE + 1);
		window_class.lpszClassName = g_vst2_editor_container_window_class_name;
		return RegisterClassExW(&window_class) != 0 || GetLastError() == ERROR_CLASS_ALREADY_EXISTS;
	}

	int initialize_runtime_instance_on_host_thread(
		Vst2PluginRuntime& runtime,
		const wchar_t* plugin_path,
		wchar_t* plugin_name,
		size_t plugin_name_count,
		int sample_rate,
		int max_block_size,
		int num_channels)
	{
		vst2_entrypoint_t entry = nullptr;
		int category = 0;

		if (!runtime.module_handle || !plugin_path || !plugin_path[0])
			return -5;

		entry = reinterpret_cast<vst2_entrypoint_t>(GetProcAddress(runtime.module_handle, "VSTPluginMain"));
		if (!entry)
			entry = reinterpret_cast<vst2_entrypoint_t>(GetProcAddress(runtime.module_handle, "main"));
		if (!entry)
			entry = reinterpret_cast<vst2_entrypoint_t>(GetProcAddress(runtime.module_handle, "MAIN"));
		if (!entry)
			return -5;

		runtime.sample_rate = sample_rate;
		runtime.max_block_size = max_block_size;
		runtime.chain_channels = num_channels;
		g_vst2_runtime_create_context = &runtime;
		if (!create_effect_instance(entry, runtime.effect, plugin_path))
		{
			g_vst2_runtime_create_context = nullptr;
			return -6;
		}
		g_vst2_runtime_create_context = nullptr;

		runtime.effect->host_internal = &runtime;
		if (dispatcher(runtime.effect, VST_EFFECT_OPCODE_CREATE, 0, 0, nullptr, 0.0f) == kVst2DispatchExceptionResult)
			return -7;
		if (!is_supported_effect(runtime.effect, category))
			return -8;

		runtime.plugin_category = category;
		runtime.input_channels = (std::max)(1, (std::min)(static_cast<int>(runtime.effect->num_inputs), kVst2MaxSupportedChannels));
		runtime.output_channels = (std::max)(1, (std::min)(static_cast<int>(runtime.effect->num_outputs), kVst2MaxSupportedChannels));
		runtime.supports_double =
			(runtime.effect->flags & VST_EFFECT_FLAG_SUPPORTS_DOUBLE) != 0 &&
			runtime.effect->process_double != nullptr;
		runtime.supports_float =
			(runtime.effect->flags & VST_EFFECT_FLAG_SUPPORTS_FLOAT) != 0 &&
			runtime.effect->process_float != nullptr;
		runtime.uses_chunks = (runtime.effect->flags & VST_EFFECT_FLAG_CHUNKS) != 0;
		runtime.initialized = true;

		query_plugin_name(runtime.effect, plugin_name, plugin_name_count, plugin_path);
		if (plugin_name && plugin_name[0])
			runtime.plugin_name = plugin_name;
		else
			runtime.plugin_name = extract_file_stem(plugin_path);
		return 0;
	}

	int close_editor_window_on_host_thread(Vst2EditorSession*& session)
	{
		Vst2EditorSession* local_session = session;

		session = nullptr;
		if (!local_session)
			return 0;

		if (local_session->editor_open && local_session->runtime && local_session->runtime->effect)
		{
			ScopedRuntimeApiLock api_lock(local_session->runtime);
			dispatcher(local_session->runtime->effect, VST_EFFECT_OPCODE_EDITOR_CLOSE, 0, 0, nullptr, 0.0f);
			consume_last_vst2_plugin_exception();
			local_session->editor_open = false;
		}

		if (local_session->window)
		{
			HWND window = local_session->window;
			SetWindowLongPtrW(window, GWLP_USERDATA, 0);
			local_session->window = 0;
			DestroyWindow(window);
		}

		clear_runtime_editor_session(local_session->runtime, local_session);
		destroy_vst2_editor_session(local_session);
		return 0;
	}

	int open_editor_window_on_host_thread(
		Vst2PluginRuntime& runtime,
		HWND parent_window,
		int& width,
		int& height,
		int& can_resize,
		VstEditorSession*& session_out)
	{
		std::unique_ptr<Vst2EditorSession> new_session;
		Vst2EditorSession* existing_session = static_cast<Vst2EditorSession*>(InterlockedCompareExchangePointer(reinterpret_cast<PVOID*>(&runtime.editor_session), nullptr, nullptr));
		RECT window_rect = {};
		DWORD style = WS_OVERLAPPEDWINDOW | WS_CLIPSIBLINGS | WS_CLIPCHILDREN;

		width = 0;
		height = 0;
		can_resize = 0;
		session_out = nullptr;

		if (existing_session)
		{
			if (existing_session->window)
			{
				ShowWindow(existing_session->window, SW_SHOWNORMAL);
				SetWindowPos(existing_session->window, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
				SetForegroundWindow(existing_session->window);
				UpdateWindow(existing_session->window);
			}
			width = existing_session->width;
			height = existing_session->height;
			can_resize = existing_session->can_resize ? 1 : 0;
			session_out = reinterpret_cast<VstEditorSession*>(existing_session);
			return 0;
		}

		new_session.reset(new (std::nothrow) Vst2EditorSession());
		if (!new_session)
			return -2;

		new_session->runtime = &runtime;
		new_session->parent_window = parent_window;
		new_session->width = 800;
		new_session->height = 600;
		new_session->can_resize = true;
		Vst2Runtime_Retain(&runtime);

		window_rect.left = 0;
		window_rect.top = 0;
		window_rect.right = new_session->width;
		window_rect.bottom = new_session->height;
		AdjustWindowRectEx(&window_rect, style, FALSE, 0);

		new_session->window = CreateWindowExW(
			WS_EX_TOPMOST,
			g_vst2_editor_window_class_name,
			runtime.plugin_name.empty() ? L"VST2 Plugin Editor" : runtime.plugin_name.c_str(),
			style,
			CW_USEDEFAULT,
			CW_USEDEFAULT,
			window_rect.right - window_rect.left,
			window_rect.bottom - window_rect.top,
			parent_window,
			nullptr,
			GetModuleHandleW(nullptr),
			new_session.get());
		if (!new_session->window)
		{
			Vst2EditorSession* cleanup_session = new_session.release();
			destroy_vst2_editor_session(cleanup_session);
			return -3;
		}

		new_session->container_window = CreateWindowExW(
			0,
			g_vst2_editor_container_window_class_name,
			nullptr,
			WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN,
			0,
			0,
			new_session->width,
			new_session->height,
			new_session->window,
			nullptr,
			GetModuleHandleW(nullptr),
			nullptr);
		if (!new_session->container_window)
		{
			Vst2EditorSession* cleanup_session = new_session.release();
			destroy_vst2_editor_session(cleanup_session);
			return -4;
		}

		SetWindowPos(new_session->window, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
		ShowWindow(new_session->window, SW_SHOWNORMAL);
		SetForegroundWindow(new_session->window);
		UpdateWindow(new_session->window);
		InterlockedExchangePointer(reinterpret_cast<PVOID*>(&runtime.editor_session), new_session.get());
		if (!open_vst2_editor_session(new_session.get()))
		{
			Vst2EditorSession* cleanup_session = new_session.release();
			destroy_vst2_editor_session(cleanup_session);
			return -5;
		}

		width = new_session->width;
		height = new_session->height;
		can_resize = new_session->can_resize ? 1 : 0;
		session_out = reinterpret_cast<VstEditorSession*>(new_session.release());
		return 0;
	}

	int process_host_command(Vst2PluginRuntime& runtime)
	{
		/*
		All non-audio VST2 operations are marshalled onto the persistent host thread.
		This preserves plugin thread affinity for creation, editor lifetime, and
		control/state work.
		*/
		switch (runtime.command_type)
		{
		case Vst2PluginRuntime::HostCommandInitialize:
			return initialize_runtime_instance_on_host_thread(
				runtime,
				runtime.command_plugin_path,
				runtime.command_plugin_name,
				runtime.command_plugin_name_count,
				runtime.command_sample_rate,
				runtime.command_max_block_size,
				runtime.command_num_channels);

		case Vst2PluginRuntime::HostCommandReconfigure:
			deactivate_runtime(runtime);
			runtime.sample_rate = runtime.command_sample_rate;
			runtime.max_block_size = runtime.command_max_block_size;
			runtime.chain_channels = runtime.command_num_channels;
			return activate_runtime(runtime);

		case Vst2PluginRuntime::HostCommandOpenEditor:
			return open_editor_window_on_host_thread(
				runtime,
				runtime.command_parent_window,
				runtime.command_width,
				runtime.command_height,
				runtime.command_can_resize,
				runtime.command_session);

		case Vst2PluginRuntime::HostCommandCloseEditor:
		{
			Vst2EditorSession* session = as_vst2_editor_session(runtime.command_session);
			return close_editor_window_on_host_thread(session);
		}

		case Vst2PluginRuntime::HostCommandCloseOpenEditor:
		{
			Vst2EditorSession* session = static_cast<Vst2EditorSession*>(InterlockedCompareExchangePointer(reinterpret_cast<PVOID*>(&runtime.editor_session), nullptr, nullptr));
			return close_editor_window_on_host_thread(session);
		}

		case Vst2PluginRuntime::HostCommandResizeEditor:
		{
			Vst2EditorSession* session = as_vst2_editor_session(runtime.command_session);
			if (!session || !session->window || !session->can_resize)
				return -1;
			return set_editor_window_size(session, runtime.command_width, runtime.command_height) ? 0 : -1;
		}

		case Vst2PluginRuntime::HostCommandShutdown:
		{
			Vst2EditorSession* session = static_cast<Vst2EditorSession*>(InterlockedCompareExchangePointer(reinterpret_cast<PVOID*>(&runtime.editor_session), nullptr, nullptr));
			close_editor_window_on_host_thread(session);
			release_runtime(runtime);
			return 0;
		}
		default:
			return 0;
		}
	}

	unsigned __stdcall vst2_runtime_host_thread_proc(void* context)
	{
		Vst2PluginRuntime* runtime = static_cast<Vst2PluginRuntime*>(context);
		MSG message = {};
		bool ole_initialized = false;
		bool running = true;
		HRESULT ole_result = E_FAIL;

		if (!runtime)
			return 1;

		/*
		This thread is the owning thread for thread-sensitive VST2 operations. It
		runs a normal message pump so editor windows and host commands share the
		same affinity when a plugin depends on that behavior.
		*/
		__try
		{
			PeekMessageW(&message, 0, WM_USER, WM_USER, PM_NOREMOVE);
			ole_result = OleInitialize(nullptr);
			ole_initialized = SUCCEEDED(ole_result);
			InitOnceExecuteOnce(&g_vst2_editor_window_class_once, ensure_vst2_editor_window_class, 0, 0);
			InitOnceExecuteOnce(&g_vst2_editor_container_window_class_once, ensure_vst2_editor_container_window_class, 0, 0);
			SetEvent(runtime->host_ready_event);

			while (running)
			{
				DWORD wait_result = MsgWaitForMultipleObjects(1, &runtime->command_event, FALSE, INFINITE, QS_ALLINPUT);
				if (wait_result == WAIT_OBJECT_0)
				{
					runtime->command_result = process_host_command(*runtime);
					running = runtime->command_type != Vst2PluginRuntime::HostCommandShutdown;
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
				else
				{
					break;
				}
			}
		}
		__except (EXCEPTION_EXECUTE_HANDLER)
		{
			InterlockedExchange(&runtime->host_faulted, 1);
			runtime->command_result = -90;
			SetEvent(runtime->command_complete_event);
		}

		if (ole_initialized)
			OleUninitialize();
		return 0;
	}

	int send_host_command(Vst2PluginRuntime& runtime, Vst2PluginRuntime::HostCommandType command_type, DWORD timeout_ms)
	{
		HANDLE wait_handles[2];
		DWORD wait_result;

		if (!runtime.host_thread || !runtime.command_event || !runtime.command_complete_event)
			return -91;
		if (InterlockedCompareExchange(&runtime.host_faulted, 0, 0) != 0)
			return -92;

		EnterCriticalSection(&runtime.command_lock);
		runtime.command_type = command_type;
		runtime.command_result = 0;
		SetEvent(runtime.command_event);

		wait_handles[0] = runtime.command_complete_event;
		wait_handles[1] = runtime.host_thread;
		wait_result = WaitForMultipleObjects(2, wait_handles, FALSE, timeout_ms);
		if (wait_result == WAIT_OBJECT_0)
		{
			int result = runtime.command_result;
			LeaveCriticalSection(&runtime.command_lock);
			return result;
		}

		if (wait_result == WAIT_OBJECT_0 + 1)
			InterlockedExchange(&runtime.host_faulted, 1);
		LeaveCriticalSection(&runtime.command_lock);
		return wait_result == WAIT_TIMEOUT ? -93 : -94;
	}

	void shutdown_runtime_host(Vst2PluginRuntime& runtime)
	{
		if (runtime.host_thread)
		{
			if (GetCurrentThreadId() != runtime.host_thread_id)
				send_host_command(runtime, Vst2PluginRuntime::HostCommandShutdown, kVst2HostShutdownTimeoutMs);
			WaitForSingleObject(runtime.host_thread, kVst2HostShutdownTimeoutMs);
			CloseHandle(runtime.host_thread);
			runtime.host_thread = 0;
		}

		if (runtime.host_ready_event)
		{
			CloseHandle(runtime.host_ready_event);
			runtime.host_ready_event = 0;
		}
		if (runtime.command_event)
		{
			CloseHandle(runtime.command_event);
			runtime.command_event = 0;
		}
		if (runtime.command_complete_event)
		{
			CloseHandle(runtime.command_complete_event);
			runtime.command_complete_event = 0;
		}
	}

}

int Vst2Runtime_Create(
	Vst2PluginRuntime*& runtime,
	const wchar_t* plugin_path,
	int sample_rate,
	int max_block_size,
	int num_channels,
	wchar_t* plugin_name,
	size_t plugin_name_count)
{
	std::unique_ptr<Vst2PluginRuntime> new_runtime;
	uintptr_t thread_handle = 0;
	unsigned thread_id = 0;
	int result = 0;

	runtime = nullptr;
	if (!plugin_path || !plugin_path[0])
		return -1;
	if (GetFileAttributesW(plugin_path) == INVALID_FILE_ATTRIBUTES)
		return -2;

	new_runtime.reset(new (std::nothrow) Vst2PluginRuntime());
	if (!new_runtime)
		return -3;

	new_runtime->plugin_path = plugin_path;
	new_runtime->plugin_directory = extract_directory_path(plugin_path);
	new_runtime->plugin_directory_ansi = wide_to_ansi(new_runtime->plugin_directory);
	new_runtime->module_handle = LoadLibraryExW(plugin_path, nullptr, LOAD_WITH_ALTERED_SEARCH_PATH);
	if (!new_runtime->module_handle)
		return fail_create(new_runtime, -4);

	new_runtime->host_ready_event = CreateEventW(nullptr, TRUE, FALSE, nullptr);
	new_runtime->command_event = CreateEventW(nullptr, FALSE, FALSE, nullptr);
	new_runtime->command_complete_event = CreateEventW(nullptr, FALSE, FALSE, nullptr);
	if (!new_runtime->host_ready_event || !new_runtime->command_event || !new_runtime->command_complete_event)
		return fail_create(new_runtime, -5);

	thread_handle = _beginthreadex(nullptr, 0, vst2_runtime_host_thread_proc, new_runtime.get(), 0, &thread_id);
	if (!thread_handle)
		return fail_create(new_runtime, -6);

	new_runtime->host_thread = reinterpret_cast<HANDLE>(thread_handle);
	new_runtime->host_thread_id = thread_id;
	if (WaitForSingleObject(new_runtime->host_ready_event, kVst2HostCommandTimeoutMs) != WAIT_OBJECT_0)
		return fail_create(new_runtime, -7);

	new_runtime->command_plugin_path = plugin_path;
	new_runtime->command_plugin_name = plugin_name;
	new_runtime->command_plugin_name_count = plugin_name_count;
	new_runtime->command_sample_rate = sample_rate;
	new_runtime->command_max_block_size = max_block_size;
	new_runtime->command_num_channels = num_channels;
	result = send_host_command(*new_runtime, Vst2PluginRuntime::HostCommandInitialize, kVst2HostCommandTimeoutMs);
	if (result != 0)
		return fail_create(new_runtime, result);

	if (!allocate_processing_buffers(*new_runtime))
		return fail_create(new_runtime, -8);

	new_runtime->command_sample_rate = sample_rate;
	new_runtime->command_max_block_size = max_block_size;
	new_runtime->command_num_channels = num_channels;
	result = send_host_command(*new_runtime, Vst2PluginRuntime::HostCommandReconfigure, kVst2HostCommandTimeoutMs);
	if (result != 0)
		return fail_create(new_runtime, result);

	runtime = new_runtime.release();
	return 0;
}

void Vst2Runtime_Retain(Vst2PluginRuntime* runtime)
{
	if (runtime)
		InterlockedIncrement(&runtime->ref_count);
}

void Vst2Runtime_Destroy(Vst2PluginRuntime*& runtime)
{
	Vst2PluginRuntime* local_runtime = runtime;

	runtime = nullptr;
	if (!local_runtime)
		return;
	if (InterlockedDecrement(&local_runtime->ref_count) != 0)
		return;

	shutdown_runtime_host(*local_runtime);
	release_runtime(*local_runtime);
	delete local_runtime;
}

int Vst2Runtime_Reconfigure(Vst2PluginRuntime* runtime, int sample_rate, int max_block_size, int num_channels)
{
	if (!runtime || !runtime->initialized)
		return -1;
	if (sample_rate <= 0 || max_block_size <= 0 || num_channels <= 0)
		return -2;

	runtime->sample_rate = sample_rate;
	runtime->max_block_size = max_block_size;
	runtime->chain_channels = num_channels;
	if (!allocate_processing_buffers(*runtime))
		return -3;
	runtime->command_sample_rate = sample_rate;
	runtime->command_max_block_size = max_block_size;
	runtime->command_num_channels = num_channels;
	return send_host_command(*runtime, Vst2PluginRuntime::HostCommandReconfigure, kVst2HostCommandTimeoutMs);
}

int Vst2Runtime_Process(Vst2PluginRuntime* runtime, double* interleaved_buffer, int frames, int chain_channels)
{
	int frame_index;

	if (!runtime || !runtime->effect || !runtime->active || !interleaved_buffer)
		return -1;
	if (frames <= 0 || frames > runtime->max_block_size)
		return -2;
	if (chain_channels <= 0)
		return -3;

	ScopedTryRuntimeApiLock api_lock(runtime);
	if (!api_lock.is_locked())
		return 0;

	for (frame_index = 0; frame_index < frames; ++frame_index)
	{
		double left = interleaved_buffer[frame_index * chain_channels];
		double right = chain_channels > 1 ? interleaved_buffer[(frame_index * chain_channels) + 1] : left;
		double mono = 0.5 * (left + right);

		if (runtime->supports_double)
		{
			runtime->input_buffers_64[0][frame_index] = runtime->input_channels == 1 ? mono : left;
			if (runtime->input_channels > 1)
				runtime->input_buffers_64[1][frame_index] = right;
		}
		else
		{
			runtime->input_buffers_32[0][frame_index] = static_cast<float>(runtime->input_channels == 1 ? mono : left);
			if (runtime->input_channels > 1)
				runtime->input_buffers_32[1][frame_index] = static_cast<float>(right);
		}
	}

	if (!process_audio_safe(runtime, frames))
		return -4;

	for (frame_index = 0; frame_index < frames; ++frame_index)
	{
		double left;
		double right;

		if (runtime->supports_double)
		{
			left = runtime->output_ptrs_64[0][frame_index];
			right = runtime->output_channels > 1 ? runtime->output_ptrs_64[1][frame_index] : left;
		}
		else
		{
			left = runtime->output_ptrs_32[0][frame_index];
			right = runtime->output_channels > 1 ? runtime->output_ptrs_32[1][frame_index] : left;
		}

		interleaved_buffer[frame_index * chain_channels] = left;
		if (chain_channels > 1)
			interleaved_buffer[(frame_index * chain_channels) + 1] = right;
	}

	return 0;
}

int Vst2Runtime_GetStateSize(Vst2PluginRuntime* runtime)
{
	void* chunk = nullptr;
	int chunk_size;
	size_t param_bytes;

	if (!runtime || !runtime->effect || !runtime->initialized)
		return -1;

	if (runtime->uses_chunks)
	{
		chunk_size = get_chunk_state(runtime, chunk);
		if (chunk_size <= 0 || !chunk)
			return 0;
		return static_cast<int>(sizeof(Vst2StateBlobHeader) + chunk_size);
	}

	if (runtime->effect->num_params <= 0 || !runtime->effect->get_parameter)
		return 0;

	param_bytes = static_cast<size_t>(runtime->effect->num_params) * sizeof(float);
	return static_cast<int>(sizeof(Vst2StateBlobHeader) + param_bytes);
}

int Vst2Runtime_GetState(Vst2PluginRuntime* runtime, void* buffer, int buffer_size, int* bytes_written)
{
	Vst2StateBlobHeader header = {};
	int payload_size = 0;
	unsigned char* output = static_cast<unsigned char*>(buffer);
	void* chunk = nullptr;
	int param_index;

	if (bytes_written)
		*bytes_written = 0;
	if (!runtime || !runtime->effect || !runtime->initialized)
		return -1;
	if (!buffer || buffer_size < static_cast<int>(sizeof(Vst2StateBlobHeader)))
		return -2;

	header.magic = kVst2StateBlobMagic;
	header.version = kVst2StateBlobVersion;
	{
		ScopedRuntimeApiLock api_lock(runtime);
		header.program_index = dispatcher(runtime->effect, VST_EFFECT_OPCODE_GET_PROGRAM, 0, 0, nullptr, 0.0f);
	}

	if (runtime->uses_chunks)
	{
		payload_size = get_chunk_state(runtime, chunk);
		if (payload_size <= 0 || !chunk)
			return -3;
		header.flags = kVst2StateBlobFlagChunks;
		header.payload_size = static_cast<uint32_t>(payload_size);
	}
	else
	{
		if (runtime->effect->num_params <= 0 || !runtime->effect->get_parameter)
			return -3;
		header.param_count = static_cast<uint32_t>(runtime->effect->num_params);
		header.payload_size = header.param_count * static_cast<uint32_t>(sizeof(float));
		payload_size = static_cast<int>(header.payload_size);
	}

	if (buffer_size < static_cast<int>(sizeof(Vst2StateBlobHeader) + header.payload_size))
		return -4;

	memcpy_s(output, static_cast<size_t>(buffer_size), &header, sizeof(header));
	if ((header.flags & kVst2StateBlobFlagChunks) != 0)
	{
		memcpy_s(output + sizeof(Vst2StateBlobHeader), static_cast<size_t>(buffer_size - sizeof(Vst2StateBlobHeader)), chunk, static_cast<size_t>(payload_size));
	}
	else
	{
		float* values = reinterpret_cast<float*>(output + sizeof(Vst2StateBlobHeader));
		ScopedRuntimeApiLock api_lock(runtime);
		for (param_index = 0; param_index < static_cast<int>(header.param_count); ++param_index)
		{
			if (!get_parameter_safe(runtime, static_cast<uint32_t>(param_index), values[param_index]))
			{
				// Zero remaining parameters so the buffer is not left
				// with uninitialized data if the caller ignores the error.
				for (; param_index < static_cast<int>(header.param_count); ++param_index)
					values[param_index] = 0.0f;
				return -4;
			}
		}
	}

	if (bytes_written)
		*bytes_written = static_cast<int>(sizeof(Vst2StateBlobHeader) + header.payload_size);
	return 0;
}

int Vst2Runtime_SetState(Vst2PluginRuntime* runtime, const void* buffer, int buffer_size)
{
	const Vst2StateBlobHeader* header = static_cast<const Vst2StateBlobHeader*>(buffer);
	const unsigned char* payload = static_cast<const unsigned char*>(buffer) + sizeof(Vst2StateBlobHeader);
	uint32_t expected_payload_size;
	int param_index;
	int dispatch_result = 0;

	if (!runtime || !runtime->effect || !runtime->initialized)
		return -1;
	if (!buffer || buffer_size < static_cast<int>(sizeof(Vst2StateBlobHeader)))
		return -2;
	if (header->magic != kVst2StateBlobMagic || header->version != kVst2StateBlobVersion)
		return -3;
	if (buffer_size < static_cast<int>(sizeof(Vst2StateBlobHeader) + header->payload_size))
		return -4;

	if ((header->flags & kVst2StateBlobFlagChunks) != 0)
	{
		if (header->payload_size == 0)
			return 0;
		ScopedRuntimeApiLock api_lock(runtime);
		dispatch_result = dispatcher(runtime->effect, VST_EFFECT_OPCODE_SET_CHUNK_DATA, 0, header->payload_size, const_cast<unsigned char*>(payload), 0.0f);
		if (dispatch_result < 0)
			return -7;
		return 0;
	}

	expected_payload_size = header->param_count * static_cast<uint32_t>(sizeof(float));
	if (expected_payload_size != header->payload_size)
		return -5;
	if (!runtime->effect->set_parameter)
		return -6;

	{
		const float* values = reinterpret_cast<const float*>(payload);
		ScopedRuntimeApiLock api_lock(runtime);

		if (header->program_index >= 0)
			dispatcher(runtime->effect, VST_EFFECT_OPCODE_SET_PROGRAM, 0, header->program_index, nullptr, 0.0f);

		for (param_index = 0; param_index < static_cast<int>(header->param_count); ++param_index)
		{
			if (!set_parameter_safe(runtime, static_cast<uint32_t>(param_index), values[param_index]))
				return -8;
		}

		if (runtime->effect->get_parameter)
		{
			for (param_index = 0; param_index < static_cast<int>(header->param_count); ++param_index)
			{
				float applied_value = 0.0f;
				if (!get_parameter_safe(runtime, static_cast<uint32_t>(param_index), applied_value))
					return -8;
				if (fabs(static_cast<double>(applied_value) - static_cast<double>(values[param_index])) > 0.001)
					return -8;
			}
		}
	}

	return 0;
}

void Vst2Runtime_SetStateDirtyCallback(Vst2PluginRuntime* runtime, Vst2RuntimeStateDirtyCallback callback, void* context)
{
	if (!runtime)
		return;

	runtime->state_dirty_callback = callback;
	runtime->state_dirty_context = context;
}

bool Vst2Runtime_IsEditorSession(VstEditorSession* session)
{
	return as_vst2_editor_session(session) != nullptr;
}

int Vst2Runtime_OpenEditor(Vst2PluginRuntime* runtime, VstProcessingState* owner_state, int plugin_index, HWND parent_window, int& width, int& height, int& can_resize, VstEditorSession*& session)
{
	Vst2EditorSession* existing_session;
	int host_result;

	width = 0;
	height = 0;
	can_resize = 0;
	session = nullptr;

	if (!runtime || !runtime->effect || !runtime->initialized)
		return -1;

	(void)owner_state;
	trace_vst2_editor(
		L"[VST2 editor] open begin plugin=\"%s\" index=%d flags=0x%08x parent=%p\r\n",
		runtime->plugin_name.empty() ? L"(unnamed)" : runtime->plugin_name.c_str(),
		plugin_index,
		runtime->effect ? runtime->effect->flags : 0,
		parent_window);

	existing_session = static_cast<Vst2EditorSession*>(InterlockedCompareExchangePointer(reinterpret_cast<PVOID*>(&runtime->editor_session), nullptr, nullptr));
	if (existing_session)
	{
		if (send_host_command(*runtime, Vst2PluginRuntime::HostCommandCloseOpenEditor, kVst2HostEditorCommandTimeoutMs) != 0)
			return -6;
	}
	runtime->command_parent_window = parent_window;
	runtime->command_width = 0;
	runtime->command_height = 0;
	runtime->command_can_resize = 0;
	runtime->command_session = nullptr;
	host_result = send_host_command(*runtime, Vst2PluginRuntime::HostCommandOpenEditor, kVst2HostEditorCommandTimeoutMs);
	trace_vst2_editor(
		L"[VST2 editor] plugin=\"%s\" host-thread open result=%ld session=%p\r\n",
		runtime->plugin_name.empty() ? L"(unnamed)" : runtime->plugin_name.c_str(),
		static_cast<long>(host_result),
		runtime->command_session);
	if (host_result != 0)
		return host_result;

	width = runtime->command_width;
	height = runtime->command_height;
	can_resize = runtime->command_can_resize;
	session = runtime->command_session;
	return 0;
}

int Vst2Runtime_CloseOpenEditor(Vst2PluginRuntime* runtime)
{
	Vst2EditorSession* session;

	if (!runtime)
		return 0;

	session = static_cast<Vst2EditorSession*>(InterlockedCompareExchangePointer(reinterpret_cast<PVOID*>(&runtime->editor_session), nullptr, nullptr));
	if (!session)
		return 0;

	return send_host_command(*runtime, Vst2PluginRuntime::HostCommandCloseOpenEditor, kVst2HostEditorCommandTimeoutMs);
}

int Vst2Runtime_CloseEditor(VstEditorSession*& session)
{
	Vst2EditorSession* local_session = as_vst2_editor_session(session);

	session = nullptr;
	if (!local_session)
		return 0;
	return send_host_command(*local_session->runtime, Vst2PluginRuntime::HostCommandCloseEditor, kVst2HostEditorCommandTimeoutMs);
}

int Vst2Runtime_ResizeEditor(VstEditorSession* session, int width, int height)
{
	Vst2EditorSession* local_session = as_vst2_editor_session(session);

	if (!local_session || !local_session->window || !local_session->can_resize)
		return -1;

	local_session->runtime->command_session = session;
	local_session->runtime->command_width = width;
	local_session->runtime->command_height = height;
	return send_host_command(*local_session->runtime, Vst2PluginRuntime::HostCommandResizeEditor, kVst2HostCommandTimeoutMs);
}

int Vst2Runtime_ProbePluginMetadataOnly(const wchar_t* plugin_path, _VstPluginProbeInfo* info)
{
	HMODULE module_handle = 0;
	vst2_entrypoint_t entry = nullptr;
	vst_effect_t* effect = nullptr;
	int category = 0;
	char name_buffer[256] = {};
	char vendor_buffer[256] = {};

	if (!info)
		return -1;
	memset(info, 0, sizeof(*info));
	if (!plugin_path || !plugin_path[0])
		return -2;

	wcsncpy_s(info->path, VST_MAX_PLUGIN_PATH_CHARS, plugin_path, _TRUNCATE);
	{
		std::wstring stem = extract_file_stem(plugin_path);
		if (!stem.empty())
			wcsncpy_s(info->name, VST_MAX_PLUGIN_NAME_CHARS, stem.c_str(), _TRUNCATE);
	}

	if (GetFileAttributesW(plugin_path) == INVALID_FILE_ATTRIBUTES)
		return -3;

	module_handle = LoadLibraryExW(plugin_path, nullptr, LOAD_WITH_ALTERED_SEARCH_PATH);
	if (!module_handle)
		return -4;

	entry = reinterpret_cast<vst2_entrypoint_t>(GetProcAddress(module_handle, "VSTPluginMain"));
	if (!entry)
		entry = reinterpret_cast<vst2_entrypoint_t>(GetProcAddress(module_handle, "main"));
	if (!entry)
		entry = reinterpret_cast<vst2_entrypoint_t>(GetProcAddress(module_handle, "MAIN"));
	if (!entry)
	{
		FreeLibrary(module_handle);
		return -5;
	}

	g_vst2_runtime_create_context = nullptr;
	create_effect_instance(entry, effect, plugin_path);
	if (!effect)
	{
		FreeLibrary(module_handle);
		return -6;
	}

	if (dispatcher(effect, VST_EFFECT_OPCODE_CREATE, 0, 0, nullptr, 0.0f) == kVst2DispatchExceptionResult)
	{
		FreeLibrary(module_handle);
		return -7;
	}

	dispatcher(effect, VST_EFFECT_OPCODE_PRODUCT_NAME, 0, 0, name_buffer, 0.0f);
	if (!name_buffer[0])
		dispatcher(effect, VST_EFFECT_OPCODE_EFFECT_NAME, 0, 0, name_buffer, 0.0f);
	if (name_buffer[0])
		ansi_to_wide(name_buffer, info->name, VST_MAX_PLUGIN_NAME_CHARS);

	dispatcher(effect, VST_EFFECT_OPCODE_VENDOR_NAME, 0, 0, vendor_buffer, 0.0f);
	if (vendor_buffer[0])
		ansi_to_wide(vendor_buffer, info->vendor, VST_MAX_PLUGIN_VENDOR_CHARS);

	info->has_audio_input = effect->num_inputs > 0 ? 1 : 0;
	info->has_audio_output = effect->num_outputs > 0 ? 1 : 0;

	if (is_supported_effect(effect, category))
	{
		info->is_valid = 1;
		info->is_audio_effect = 1;
	}
	else
	{
		info->is_valid = 1;
		info->is_audio_effect = 0;
	}

	dispatcher(effect, VST_EFFECT_OPCODE_DESTROY, 0, 0, nullptr, 0.0f);
	FreeLibrary(module_handle);
	return 0;
}
