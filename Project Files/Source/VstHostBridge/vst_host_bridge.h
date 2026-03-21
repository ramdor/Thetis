#ifndef _vst_host_bridge_h
#define _vst_host_bridge_h

#include <Windows.h>
#include <wchar.h>

#ifdef VSTHOSTBRIDGE_EXPORTS
#define VSTBRIDGE_API __declspec(dllexport)
#else
#define VSTBRIDGE_API __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef enum _VstChainKind
{
	VST_CHAIN_RX = 0,
	VST_CHAIN_TX = 1,
	VST_CHAIN_COUNT = 2
} VstChainKind;

enum
{
	VST_MAX_PLUGIN_PATH_CHARS = 1024,
	VST_MAX_PLUGIN_NAME_CHARS = 256,
	VST_MAX_PLUGIN_VENDOR_CHARS = 256,
	VST_MAX_PLUGIN_VERSION_CHARS = 64,
	VST_MAX_PLUGIN_SUBCATEGORY_CHARS = 256
};

typedef enum _VstPluginLoadState
{
	VST_PLUGIN_LOAD_FAILED = -1,
	VST_PLUGIN_LOAD_NONE = 0,
	VST_PLUGIN_LOAD_DESCRIPTOR_ONLY = 1,
	VST_PLUGIN_LOAD_ACTIVE = 2
} VstPluginLoadState;

typedef enum _VstPluginFormat
{
	VST_PLUGIN_FORMAT_UNKNOWN = 0,
	VST_PLUGIN_FORMAT_VST3 = 1,
	VST_PLUGIN_FORMAT_VST2 = 2
} VstPluginFormat;

typedef enum _VstHostState
{
	VST_HOST_STATE_DISABLED = 0,
	VST_HOST_STATE_STARTING = 1,
	VST_HOST_STATE_RUNNING = 2,
	VST_HOST_STATE_UNAVAILABLE = 3,
	VST_HOST_STATE_CRASHED = 4,
	VST_HOST_STATE_RESTARTING = 5
} VstHostState;

typedef struct _VstPluginInfo
{
	int index;
	int enabled;
	int bypass;
	int load_state;
	int format;
	wchar_t path[VST_MAX_PLUGIN_PATH_CHARS];
	wchar_t name[VST_MAX_PLUGIN_NAME_CHARS];
} VstPluginInfo;

typedef struct _VstPluginProbeInfo
{
	int is_valid;
	int is_audio_effect;
	int has_audio_input;
	int has_audio_output;
	wchar_t path[VST_MAX_PLUGIN_PATH_CHARS];
	wchar_t name[VST_MAX_PLUGIN_NAME_CHARS];
	wchar_t vendor[VST_MAX_PLUGIN_VENDOR_CHARS];
	wchar_t version[VST_MAX_PLUGIN_VERSION_CHARS];
	wchar_t subcategories[VST_MAX_PLUGIN_SUBCATEGORY_CHARS];
} VstPluginProbeInfo;

typedef void (__cdecl *VstStateChangedCallback)(VstChainKind kind);

VSTBRIDGE_API int  VST_Initialize(void);
VSTBRIDGE_API void VST_Shutdown(void);
VSTBRIDGE_API int  VST_GetSdkAvailable(void);
VSTBRIDGE_API int  VST_ProbePluginMetadataOnly(const wchar_t* plugin_path, VstPluginProbeInfo* info);

VSTBRIDGE_API int  VST_CreateChain(VstChainKind kind, int sample_rate, int max_block_size, int num_channels);
VSTBRIDGE_API void VST_DestroyChain(VstChainKind kind);

VSTBRIDGE_API void VST_SetChainBypass(VstChainKind kind, int bypass);
VSTBRIDGE_API int  VST_GetChainBypass(VstChainKind kind);
VSTBRIDGE_API void VST_SetChainGain(VstChainKind kind, double gain);
VSTBRIDGE_API double VST_GetChainGain(VstChainKind kind);
VSTBRIDGE_API int  VST_GetChainReady(VstChainKind kind);
VSTBRIDGE_API int  VST_ClearChain(VstChainKind kind);
VSTBRIDGE_API int  VST_AddPlugin(VstChainKind kind, const wchar_t* plugin_path);
VSTBRIDGE_API int  VST_RemovePlugin(VstChainKind kind, int index);
VSTBRIDGE_API int  VST_MovePlugin(VstChainKind kind, int from_index, int to_index);
VSTBRIDGE_API int  VST_GetPluginCount(VstChainKind kind);
VSTBRIDGE_API int  VST_GetPluginInfo(VstChainKind kind, int index, VstPluginInfo* info);
VSTBRIDGE_API int  VST_SetPluginBypass(VstChainKind kind, int index, int bypass);
VSTBRIDGE_API int  VST_SetPluginEnabled(VstChainKind kind, int index, int enabled);
VSTBRIDGE_API int  VST_GetPluginStateSize(VstChainKind kind, int index);
VSTBRIDGE_API int  VST_GetPluginState(VstChainKind kind, int index, void* buffer, int buffer_size, int* bytes_written);
VSTBRIDGE_API int  VST_SetPluginState(VstChainKind kind, int index, const void* buffer, int buffer_size);
VSTBRIDGE_API int  VST_GetHostState(VstChainKind kind);
VSTBRIDGE_API int  VST_GetPipelineLatency(VstChainKind kind, int* current_blocks, int* floor_blocks);
VSTBRIDGE_API void VST_SetPipelineLatencyFloor(VstChainKind kind, int floor_blocks);
VSTBRIDGE_API int  VST_GetSampleRate(VstChainKind kind);
VSTBRIDGE_API int  VST_GetBlockSize(VstChainKind kind);
VSTBRIDGE_API int  VST_GetChainSnapshotGeneration(VstChainKind kind);
VSTBRIDGE_API int  VST_RequestHostChainSync(VstChainKind kind);
VSTBRIDGE_API void VST_SetStateChangedCallback(VstStateChangedCallback callback);
VSTBRIDGE_API int  VST_OpenPluginEditorWindow(VstChainKind kind, int index);

VSTBRIDGE_API int  VST_ProcessInterleavedDouble(VstChainKind kind, double* buffer, int frames);

#ifdef __cplusplus
}
#endif

#endif
