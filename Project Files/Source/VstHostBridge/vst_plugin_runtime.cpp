#include "vst_plugin_runtime.h"

#include "vst2_runtime.h"
#include "vst_runtime.h"

#include <cwchar>
#include <new>

struct ThetisPluginRuntime
{
	volatile LONG ref_count = 1;
	VstPluginFormat format = VST_PLUGIN_FORMAT_UNKNOWN;
	VstPluginRuntime* vst3_runtime = nullptr;
	Vst2PluginRuntime* vst2_runtime = nullptr;
};

namespace
{
	VstPluginFormat detect_plugin_format(const wchar_t* plugin_path)
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
}

int ThetisPluginRuntime_Create(
	ThetisPluginRuntime*& runtime,
	const wchar_t* plugin_path,
	int sample_rate,
	int max_block_size,
	int num_channels,
	wchar_t* plugin_name,
	size_t plugin_name_count)
{
	ThetisPluginRuntime* wrapper = nullptr;
	VstPluginRuntime* vst3_runtime = nullptr;
	Vst2PluginRuntime* vst2_runtime = nullptr;
	VstPluginFormat format;
	int result;

	runtime = nullptr;
	format = detect_plugin_format(plugin_path);
	if (format == VST_PLUGIN_FORMAT_VST2)
	{
		result = Vst2Runtime_Create(
			vst2_runtime,
			plugin_path,
			sample_rate,
			max_block_size,
			num_channels,
			plugin_name,
			plugin_name_count);
	}
	else if (format == VST_PLUGIN_FORMAT_VST3)
	{
		result = VstRuntime_Create(
			vst3_runtime,
			plugin_path,
			sample_rate,
			max_block_size,
			num_channels,
			plugin_name,
			plugin_name_count);
	}
	else
		return -51;
	if (result != 0)
		return result;

	wrapper = new (std::nothrow) ThetisPluginRuntime();
	if (!wrapper)
	{
		if (format == VST_PLUGIN_FORMAT_VST3)
			VstRuntime_Destroy(vst3_runtime);
		else if (format == VST_PLUGIN_FORMAT_VST2)
			Vst2Runtime_Destroy(vst2_runtime);
		return -52;
	}

	wrapper->format = format;
	wrapper->vst3_runtime = vst3_runtime;
	wrapper->vst2_runtime = vst2_runtime;
	runtime = wrapper;
	return 0;
}

void ThetisPluginRuntime_Retain(ThetisPluginRuntime* runtime)
{
	if (runtime)
		InterlockedIncrement(&runtime->ref_count);
}

void ThetisPluginRuntime_Destroy(ThetisPluginRuntime*& runtime)
{
	ThetisPluginRuntime* local_runtime = runtime;

	runtime = nullptr;
	if (!local_runtime)
		return;
	if (InterlockedDecrement(&local_runtime->ref_count) != 0)
		return;

	if (local_runtime->format == VST_PLUGIN_FORMAT_VST3)
		VstRuntime_Destroy(local_runtime->vst3_runtime);
	else if (local_runtime->format == VST_PLUGIN_FORMAT_VST2)
		Vst2Runtime_Destroy(local_runtime->vst2_runtime);
	delete local_runtime;
}

int ThetisPluginRuntime_Reconfigure(ThetisPluginRuntime* runtime, int sample_rate, int max_block_size, int num_channels)
{
	if (!runtime)
		return -1;

	switch (runtime->format)
	{
	case VST_PLUGIN_FORMAT_VST3:
		return VstRuntime_Reconfigure(runtime->vst3_runtime, sample_rate, max_block_size, num_channels);
	case VST_PLUGIN_FORMAT_VST2:
		return Vst2Runtime_Reconfigure(runtime->vst2_runtime, sample_rate, max_block_size, num_channels);
	default:
		return -1;
	}
}

int ThetisPluginRuntime_Process(ThetisPluginRuntime* runtime, double* interleaved_buffer, int frames, int chain_channels)
{
	if (!runtime)
		return -1;

	switch (runtime->format)
	{
	case VST_PLUGIN_FORMAT_VST3:
		return VstRuntime_Process(runtime->vst3_runtime, interleaved_buffer, frames, chain_channels);
	case VST_PLUGIN_FORMAT_VST2:
		return Vst2Runtime_Process(runtime->vst2_runtime, interleaved_buffer, frames, chain_channels);
	default:
		return -1;
	}
}

int ThetisPluginRuntime_GetStateSize(ThetisPluginRuntime* runtime)
{
	if (!runtime)
		return -1;

	switch (runtime->format)
	{
	case VST_PLUGIN_FORMAT_VST3:
		return VstRuntime_GetStateSize(runtime->vst3_runtime);
	case VST_PLUGIN_FORMAT_VST2:
		return Vst2Runtime_GetStateSize(runtime->vst2_runtime);
	default:
		return -1;
	}
}

int ThetisPluginRuntime_GetState(ThetisPluginRuntime* runtime, void* buffer, int buffer_size, int* bytes_written)
{
	if (!runtime)
		return -1;

	switch (runtime->format)
	{
	case VST_PLUGIN_FORMAT_VST3:
		return VstRuntime_GetState(runtime->vst3_runtime, buffer, buffer_size, bytes_written);
	case VST_PLUGIN_FORMAT_VST2:
		return Vst2Runtime_GetState(runtime->vst2_runtime, buffer, buffer_size, bytes_written);
	default:
		return -1;
	}
}

int ThetisPluginRuntime_SetState(ThetisPluginRuntime* runtime, const void* buffer, int buffer_size)
{
	if (!runtime)
		return -1;

	switch (runtime->format)
	{
	case VST_PLUGIN_FORMAT_VST3:
		return VstRuntime_SetState(runtime->vst3_runtime, buffer, buffer_size);
	case VST_PLUGIN_FORMAT_VST2:
		return Vst2Runtime_SetState(runtime->vst2_runtime, buffer, buffer_size);
	default:
		return -1;
	}
}

void ThetisPluginRuntime_SetStateDirtyCallback(ThetisPluginRuntime* runtime, ThetisPluginRuntimeStateDirtyCallback callback, void* context)
{
	if (!runtime)
		return;

	switch (runtime->format)
	{
	case VST_PLUGIN_FORMAT_VST3:
		VstRuntime_SetStateDirtyCallback(runtime->vst3_runtime, callback, context);
		break;
	case VST_PLUGIN_FORMAT_VST2:
		Vst2Runtime_SetStateDirtyCallback(runtime->vst2_runtime, callback, context);
		break;
	default:
		break;
	}
}

int ThetisPluginRuntime_OpenEditor(ThetisPluginRuntime* runtime, VstProcessingState* owner_state, int plugin_index, HWND parent_window, int& width, int& height, int& can_resize, VstEditorSession*& session)
{
	if (!runtime)
		return -1;

	switch (runtime->format)
	{
	case VST_PLUGIN_FORMAT_VST3:
		return VstRuntime_OpenEditor(runtime->vst3_runtime, owner_state, plugin_index, parent_window, width, height, can_resize, session);
	case VST_PLUGIN_FORMAT_VST2:
		return Vst2Runtime_OpenEditor(runtime->vst2_runtime, owner_state, plugin_index, parent_window, width, height, can_resize, session);
	default:
		return -1;
	}
}

int ThetisPluginRuntime_CloseOpenEditor(ThetisPluginRuntime* runtime)
{
	if (!runtime)
		return -1;

	switch (runtime->format)
	{
	case VST_PLUGIN_FORMAT_VST3:
		return VstRuntime_CloseOpenEditor(runtime->vst3_runtime);
	case VST_PLUGIN_FORMAT_VST2:
		return Vst2Runtime_CloseOpenEditor(runtime->vst2_runtime);
	default:
		return -1;
	}
}

int ThetisPluginRuntime_CloseEditor(VstEditorSession*& session)
{
	if (Vst2Runtime_IsEditorSession(session))
		return Vst2Runtime_CloseEditor(session);
	return VstRuntime_CloseEditor(session);
}

int ThetisPluginRuntime_ResizeEditor(VstEditorSession* session, int width, int height)
{
	if (Vst2Runtime_IsEditorSession(session))
		return Vst2Runtime_ResizeEditor(session, width, height);
	return VstRuntime_ResizeEditor(session, width, height);
}

VstPluginFormat ThetisPluginRuntime_GetFormat(ThetisPluginRuntime* runtime)
{
	return runtime ? runtime->format : VST_PLUGIN_FORMAT_UNKNOWN;
}
