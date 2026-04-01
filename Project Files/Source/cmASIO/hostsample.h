#ifndef _hostsample_h
#define _hostsample_h


#ifdef __cplusplus
extern "C" {
#endif

	__declspec(dllexport) long getASIODriverString(void* szData);
	__declspec(dllexport) long getASIOBlockNum(void* dwData);
	__declspec(dllexport) int prepareASIO(int blocksize, int samplerate, char* asioDriverName, void (*CallbackASIO)(void* inputL, void* inputR, void* outputL, void* outputR), long input_base_channel, long output_base_channel);
	__declspec(dllexport) void unloadASIO();
	__declspec(dllexport) long asioStart();
	__declspec(dllexport) long asioStop();
	__declspec(dllexport) long getASIOBaseInputChannel(void* dwData);
	__declspec(dllexport) long getASIOBaseOutputChannel(void* dwData);
	__declspec(dllexport) long getASIOInputMode(void* dwData);

#ifdef __cplusplus
}
#endif


#endif