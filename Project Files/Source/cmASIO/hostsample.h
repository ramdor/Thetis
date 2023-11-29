#ifndef _hostsample_h
#define _hostsample_h


#ifdef __cplusplus
extern "C" {
#endif

	__declspec(dllexport) long getASIODriverString(void* szData);
	__declspec(dllexport) long getASIOBlockNum(void* dwData);
	__declspec(dllexport) int prepareASIO(int blocksize, int samplerate, char* asioDriverName, void (*CallbackASIO)(void* inputL, void* inputR, void* outputL, void* outputR));
	__declspec(dllexport) void unloadASIO();
	__declspec(dllexport) long asioStart();
	__declspec(dllexport) long asioStop();

#ifdef __cplusplus
}
#endif


#endif