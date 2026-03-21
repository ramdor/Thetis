#ifndef _vst_chain_h
#define _vst_chain_h

#include "vst_host_bridge.h"

struct ThetisPluginRuntime;
struct VstEditorSession;
typedef void (*VstChainStateDirtyCallback)(void* context);

enum
{
	VST_MAX_CHAIN_PLUGINS = 16
};

struct VstPluginSlot
{
	volatile LONG enabled;
	volatile LONG bypass;
	volatile LONG load_state;
	volatile LONG format;
	wchar_t path[VST_MAX_PLUGIN_PATH_CHARS];
	wchar_t name[VST_MAX_PLUGIN_NAME_CHARS];
};

struct VstProcessingPlugin
{
	volatile LONG enabled;
	volatile LONG bypass;
	volatile LONG load_state;
	ThetisPluginRuntime* runtime;
};

struct VstProcessingState
{
	volatile LONG ref_count;
	int sample_rate;
	int max_block_size;
	int num_channels;
	int plugin_count;
	VstProcessingPlugin plugins[VST_MAX_CHAIN_PLUGINS];
};

struct VstChainState
{
	SRWLOCK state_lock;
	CRITICAL_SECTION mutation_lock;
	HANDLE rebuild_event;
	HANDLE worker_thread;
	volatile LONG stop_worker;
	volatile LONG rebuild_generation;
	volatile LONG created;
	volatile LONG ready;
	volatile LONG bypass;
	volatile LONG plugin_count;
	volatile LONG64 gain_bits;
	int sample_rate;
	int max_block_size;
	int num_channels;
	VstChainStateDirtyCallback state_dirty_callback;
	void* state_dirty_context;
	VstPluginSlot plugins[VST_MAX_CHAIN_PLUGINS];
	VstProcessingState* active_state;
};

void VstProcessingState_Retain(VstProcessingState* state);
void VstProcessingState_Release(VstProcessingState*& state);

void VstChain_Initialize(VstChainState& chain);
void VstChain_Terminate(VstChainState& chain);
void VstChain_Reset(VstChainState& chain);
int VstChain_Create(VstChainState& chain, int sample_rate, int max_block_size, int num_channels);
void VstChain_Destroy(VstChainState& chain);

void VstChain_SetBypass(VstChainState& chain, int bypass);
int VstChain_GetBypass(const VstChainState& chain);
int VstChain_GetReady(const VstChainState& chain);

void VstChain_SetGain(VstChainState& chain, double gain);
double VstChain_GetGain(const VstChainState& chain);

int VstChain_ClearPlugins(VstChainState& chain);
int VstChain_AddPlugin(VstChainState& chain, const wchar_t* plugin_path);
int VstChain_RemovePlugin(VstChainState& chain, int index);
int VstChain_MovePlugin(VstChainState& chain, int from_index, int to_index);
void VstChain_GetSnapshotInfo(
	const VstChainState& chain,
	int& created,
	int& sample_rate,
	int& max_block_size,
	int& num_channels,
	int& ready,
	int& bypass,
	double& gain,
	int& plugin_count);
int VstChain_GetPluginCount(const VstChainState& chain);
int VstChain_GetPluginInfo(const VstChainState& chain, int index, VstPluginInfo* info);
int VstChain_SetPluginBypass(VstChainState& chain, int index, int bypass);
int VstChain_SetPluginEnabled(VstChainState& chain, int index, int enabled);
int VstChain_GetPluginStateSize(VstChainState& chain, int index);
int VstChain_GetPluginState(VstChainState& chain, int index, void* buffer, int buffer_size, int* bytes_written);
int VstChain_SetPluginState(VstChainState& chain, int index, const void* buffer, int buffer_size);
void VstChain_SetStateDirtyCallback(VstChainState& chain, VstChainStateDirtyCallback callback, void* context);
int VstChain_OpenPluginEditor(VstChainState& chain, int index, HWND parent_window, int& width, int& height, int& can_resize, VstEditorSession*& session);
int VstChain_ClosePluginEditor(VstEditorSession*& session);
int VstChain_ResizePluginEditor(VstEditorSession* session, int width, int height);

int VstChain_ProcessInterleavedDouble(VstChainState& chain, double* buffer, int frames);

#endif
