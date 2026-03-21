#ifndef _vst_runtime_h
#define _vst_runtime_h

#include <Windows.h>
#include <stddef.h>

#include "vst_host_bridge.h"

struct VstPluginRuntime;
struct VstEditorSession;
struct VstProcessingState;
typedef void (*VstRuntimeStateDirtyCallback)(void* context);

void VstProcessingState_Retain(VstProcessingState* state);
void VstProcessingState_Release(VstProcessingState*& state);

int VstRuntime_Create(
	VstPluginRuntime*& runtime,
	const wchar_t* plugin_path,
	int sample_rate,
	int max_block_size,
	int num_channels,
	wchar_t* plugin_name,
	size_t plugin_name_count);

void VstRuntime_Retain(VstPluginRuntime* runtime);
void VstRuntime_Destroy(VstPluginRuntime*& runtime);
int VstRuntime_Reconfigure(VstPluginRuntime* runtime, int sample_rate, int max_block_size, int num_channels);
int VstRuntime_Process(VstPluginRuntime* runtime, double* interleaved_buffer, int frames, int chain_channels);
int VstRuntime_GetStateSize(VstPluginRuntime* runtime);
int VstRuntime_GetState(VstPluginRuntime* runtime, void* buffer, int buffer_size, int* bytes_written);
int VstRuntime_SetState(VstPluginRuntime* runtime, const void* buffer, int buffer_size);
void VstRuntime_SetStateDirtyCallback(VstPluginRuntime* runtime, VstRuntimeStateDirtyCallback callback, void* context);
int VstRuntime_ProbePluginMetadataOnly(const wchar_t* plugin_path, VstPluginProbeInfo* info);
int VstRuntime_OpenEditor(VstPluginRuntime* runtime, VstProcessingState* owner_state, int plugin_index, HWND parent_window, int& width, int& height, int& can_resize, VstEditorSession*& session);
VstProcessingState* VstRuntime_GetEditorOwnerState(VstEditorSession* session);
int VstRuntime_CloseOpenEditor(VstPluginRuntime* runtime);
int VstRuntime_CloseEditor(VstEditorSession*& session);
int VstRuntime_ResizeEditor(VstEditorSession* session, int width, int height);

#endif
