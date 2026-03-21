#ifndef _vst_plugin_runtime_h
#define _vst_plugin_runtime_h

#include <Windows.h>
#include <stddef.h>

#include "vst_host_bridge.h"

struct ThetisPluginRuntime;
struct VstEditorSession;
struct VstProcessingState;
typedef void (*ThetisPluginRuntimeStateDirtyCallback)(void* context);

int ThetisPluginRuntime_Create(
	ThetisPluginRuntime*& runtime,
	const wchar_t* plugin_path,
	int sample_rate,
	int max_block_size,
	int num_channels,
	wchar_t* plugin_name,
	size_t plugin_name_count);

void ThetisPluginRuntime_Retain(ThetisPluginRuntime* runtime);
void ThetisPluginRuntime_Destroy(ThetisPluginRuntime*& runtime);
int ThetisPluginRuntime_Reconfigure(ThetisPluginRuntime* runtime, int sample_rate, int max_block_size, int num_channels);
int ThetisPluginRuntime_Process(ThetisPluginRuntime* runtime, double* interleaved_buffer, int frames, int chain_channels);
int ThetisPluginRuntime_GetStateSize(ThetisPluginRuntime* runtime);
int ThetisPluginRuntime_GetState(ThetisPluginRuntime* runtime, void* buffer, int buffer_size, int* bytes_written);
int ThetisPluginRuntime_SetState(ThetisPluginRuntime* runtime, const void* buffer, int buffer_size);
void ThetisPluginRuntime_SetStateDirtyCallback(ThetisPluginRuntime* runtime, ThetisPluginRuntimeStateDirtyCallback callback, void* context);
int ThetisPluginRuntime_OpenEditor(ThetisPluginRuntime* runtime, VstProcessingState* owner_state, int plugin_index, HWND parent_window, int& width, int& height, int& can_resize, VstEditorSession*& session);
int ThetisPluginRuntime_CloseOpenEditor(ThetisPluginRuntime* runtime);
int ThetisPluginRuntime_CloseEditor(VstEditorSession*& session);
int ThetisPluginRuntime_ResizeEditor(VstEditorSession* session, int width, int height);
VstPluginFormat ThetisPluginRuntime_GetFormat(ThetisPluginRuntime* runtime);

#endif
