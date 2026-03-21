#ifndef _vst2_runtime_h
#define _vst2_runtime_h

#include <Windows.h>
#include <stddef.h>

struct Vst2PluginRuntime;
struct VstEditorSession;
struct VstProcessingState;
typedef void (*Vst2RuntimeStateDirtyCallback)(void* context);

int Vst2Runtime_Create(
	Vst2PluginRuntime*& runtime,
	const wchar_t* plugin_path,
	int sample_rate,
	int max_block_size,
	int num_channels,
	wchar_t* plugin_name,
	size_t plugin_name_count);

void Vst2Runtime_Retain(Vst2PluginRuntime* runtime);
void Vst2Runtime_Destroy(Vst2PluginRuntime*& runtime);
int Vst2Runtime_Reconfigure(Vst2PluginRuntime* runtime, int sample_rate, int max_block_size, int num_channels);
int Vst2Runtime_Process(Vst2PluginRuntime* runtime, double* interleaved_buffer, int frames, int chain_channels);
int Vst2Runtime_GetStateSize(Vst2PluginRuntime* runtime);
int Vst2Runtime_GetState(Vst2PluginRuntime* runtime, void* buffer, int buffer_size, int* bytes_written);
int Vst2Runtime_SetState(Vst2PluginRuntime* runtime, const void* buffer, int buffer_size);
void Vst2Runtime_SetStateDirtyCallback(Vst2PluginRuntime* runtime, Vst2RuntimeStateDirtyCallback callback, void* context);
bool Vst2Runtime_IsEditorSession(VstEditorSession* session);
int Vst2Runtime_OpenEditor(Vst2PluginRuntime* runtime, VstProcessingState* owner_state, int plugin_index, HWND parent_window, int& width, int& height, int& can_resize, VstEditorSession*& session);
int Vst2Runtime_CloseOpenEditor(Vst2PluginRuntime* runtime);
int Vst2Runtime_CloseEditor(VstEditorSession*& session);
int Vst2Runtime_ResizeEditor(VstEditorSession* session, int width, int height);

struct _VstPluginProbeInfo;
int Vst2Runtime_ProbePluginMetadataOnly(const wchar_t* plugin_path, struct _VstPluginProbeInfo* info);

#endif
