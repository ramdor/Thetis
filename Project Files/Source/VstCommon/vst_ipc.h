#ifndef _vst_ipc_h
#define _vst_ipc_h

#include "../VstHostBridge/vst_host_bridge.h"

#include <stdint.h>

enum VstHostIpcMessageType
{
	VST_HOST_IPC_HELLO = 1,
	VST_HOST_IPC_PING = 2,
	VST_HOST_IPC_PONG = 3,
	VST_HOST_IPC_SHUTDOWN = 4,
	VST_HOST_IPC_CREATE_CHAIN = 10,
	VST_HOST_IPC_DESTROY_CHAIN = 11,
	VST_HOST_IPC_SET_CHAIN_BYPASS = 12,
	VST_HOST_IPC_GET_CHAIN_BYPASS = 13,
	VST_HOST_IPC_SET_CHAIN_GAIN = 14,
	VST_HOST_IPC_GET_CHAIN_GAIN = 15,
	VST_HOST_IPC_GET_CHAIN_READY = 16,
	VST_HOST_IPC_CLEAR_CHAIN = 17,
	VST_HOST_IPC_ADD_PLUGIN = 18,
	VST_HOST_IPC_REMOVE_PLUGIN = 19,
	VST_HOST_IPC_MOVE_PLUGIN = 20,
	VST_HOST_IPC_GET_PLUGIN_COUNT = 21,
	VST_HOST_IPC_GET_PLUGIN_INFO = 22,
	VST_HOST_IPC_SET_PLUGIN_BYPASS = 23,
	VST_HOST_IPC_SET_PLUGIN_ENABLED = 24,
	VST_HOST_IPC_GET_PLUGIN_STATE_SIZE = 25,
	VST_HOST_IPC_GET_PLUGIN_STATE = 26,
	VST_HOST_IPC_SET_PLUGIN_STATE = 27,
	VST_HOST_IPC_OPEN_PLUGIN_EDITOR = 28,
	VST_HOST_IPC_GET_CHAIN_SNAPSHOT_INFO = 29,
	VST_HOST_IPC_RESPONSE = 100
};

enum
{
	VST_HOST_IPC_MAGIC = 0x56544950u, /* "VTIP" */
	VST_HOST_IPC_VERSION = 1u,
	VST_HOST_IPC_MAX_STATE_BYTES = 65536u,
	VST_HOST_IPC_AUDIO_MAX_CHANNELS = 2u,
	VST_HOST_IPC_AUDIO_MAX_FRAMES = 4096u,
	VST_HOST_IPC_AUDIO_MAX_SAMPLES = VST_HOST_IPC_AUDIO_MAX_CHANNELS * VST_HOST_IPC_AUDIO_MAX_FRAMES,
	VST_HOST_IPC_AUDIO_SLOT_COUNT = 32u,
	VST_HOST_IPC_AUDIO_PIPELINE_LATENCY = 4u
};

enum
{
	VST_HOST_IPC_RESULT_SNAPSHOT_CHANGED = -250,
	VST_HOST_IPC_RESULT_CONTROL_TIMEOUT = -260,
	VST_HOST_IPC_RESULT_RUNTIME_THREAD_EXITED = -261
};

typedef struct _VstHostIpcMessage
{
	uint32_t magic;
	uint32_t version;
	uint32_t type;
	uint32_t size;
	uint32_t chain_kind;
	uint32_t reserved;
} VstHostIpcMessage;

typedef struct _VstChainSnapshotInfo
{
	int32_t created;
	int32_t sample_rate;
	int32_t max_block_size;
	int32_t num_channels;
	int32_t ready;
	int32_t bypass;
	int32_t plugin_count;
	int32_t generation;
	double gain;
} VstChainSnapshotInfo;

typedef struct _VstHostIpcPacket
{
	VstHostIpcMessage header;
	int32_t result;
	int32_t arg0;
	int32_t arg1;
	int32_t arg2;
	int32_t arg3;
	double double_value;
	VstChainSnapshotInfo chain_snapshot_info;
	VstPluginInfo plugin_info;
	wchar_t plugin_path[VST_MAX_PLUGIN_PATH_CHARS];
	unsigned char state_data[VST_HOST_IPC_MAX_STATE_BYTES];
} VstHostIpcPacket;

typedef struct _VstHostAudioSharedBlock
{
	volatile LONG request_sequence;
	volatile LONG sample_rate;
	volatile LONG max_block_size;
	volatile LONG num_channels;
	volatile LONG last_process_result;
	volatile LONG state_dirty_generation;
	volatile LONG input_sequence[VST_HOST_IPC_AUDIO_SLOT_COUNT];
	volatile LONG processed_sequence[VST_HOST_IPC_AUDIO_SLOT_COUNT];
	volatile LONG input_frames[VST_HOST_IPC_AUDIO_SLOT_COUNT];
	volatile LONG output_frames[VST_HOST_IPC_AUDIO_SLOT_COUNT];
	double input_buffer[VST_HOST_IPC_AUDIO_SLOT_COUNT][VST_HOST_IPC_AUDIO_MAX_SAMPLES];
	double output_buffer[VST_HOST_IPC_AUDIO_SLOT_COUNT][VST_HOST_IPC_AUDIO_MAX_SAMPLES];
} VstHostAudioSharedBlock;

#endif
