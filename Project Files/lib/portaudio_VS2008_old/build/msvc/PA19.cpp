/*   Port Audio Library Dll Creation Program  -- Copyright 2004 FlexRadio Systems  /*
 *   Written by Eric Wachsmann
 */

#include <windows.h>
#include "portaudio.h"
//#include "PA19.h"

#ifdef PA19_EXPORTS
#define PA19_API __declspec(dllexport)
#else
#define PA19_API __declspec(dllimport)
#endif

#define CCONV __stdcall

int WINAPI DllMain(HANDLE hModule, 
                      DWORD  ul_reason_for_call, 
                      LPVOID lpReserved)
{
    switch (ul_reason_for_call)
	{
		case DLL_PROCESS_ATTACH:
		case DLL_THREAD_ATTACH:
		case DLL_THREAD_DETACH:
		case DLL_PROCESS_DETACH:
			break;
    }
    return TRUE;
}


#ifdef __cplusplus
extern "C" {
#endif

void *streamCB;
void *streamFinishedCB;
void *streamCB2;
void *streamFinishedCB2;

int __cdecl myStreamCallback(const void *input, void *output, unsigned long frameCount, const PaStreamCallbackTimeInfo* timeInfo, PaStreamCallbackFlags statusFlags, void *userData)
{
	typedef int (__stdcall *FNPTR)(const void *input, void *output, unsigned long frameCount, const PaStreamCallbackTimeInfo* timeInfo, PaStreamCallbackFlags statusFlags, void *userData);
	FNPTR fnptr = (FNPTR)streamCB;
	return fnptr(input, output, frameCount, timeInfo, statusFlags, userData);
}

int __cdecl myStreamCallback2(const void *input, void *output, unsigned long frameCount, const PaStreamCallbackTimeInfo* timeInfo, PaStreamCallbackFlags statusFlags, void *userData)
{
	typedef int (__stdcall *FNPTR)(const void *input, void *output, unsigned long frameCount, const PaStreamCallbackTimeInfo* timeInfo, PaStreamCallbackFlags statusFlags, void *userData);
	FNPTR fnptr = (FNPTR)streamCB2;
	return fnptr(input, output, frameCount, timeInfo, statusFlags, userData);
}

void __cdecl myStreamFinishedCallback(void *userData)
{
	typedef int (__stdcall *FNPTR)(void *userData);
	FNPTR fnptr = (FNPTR)streamFinishedCB;
	fnptr(userData);
}

void __cdecl myStreamFinishedCallback2(void *userData)
{
	typedef int (__stdcall *FNPTR)(void *userData);
	FNPTR fnptr = (FNPTR)streamFinishedCB2;
	fnptr(userData);
}

PA19_API int CCONV PA_GetVersion()
{
	return Pa_GetVersion();
}

PA19_API const char* CCONV PA_GetVersionText()
{
	return Pa_GetVersionText();
}

PA19_API const char* CCONV PA_GetErrorText(PaError errorCode) 
{  
	return Pa_GetErrorText(errorCode);
}

PA19_API PaError CCONV PA_Initialize()
{
	return Pa_Initialize();
}

PA19_API PaError CCONV PA_Terminate()
{
	return Pa_Terminate();
}

PA19_API PaHostApiIndex CCONV PA_GetHostApiCount()
{
	return Pa_GetHostApiCount();
}

PA19_API PaHostApiIndex CCONV PA_GetDefaultHostApi()
{
	return Pa_GetDefaultHostApi();
}

PA19_API const PaHostApiInfo* CCONV PA_GetHostApiInfo(PaHostApiIndex hostApi)
{
	return Pa_GetHostApiInfo(hostApi);
}

PA19_API PaHostApiIndex CCONV PA_HostApiTypeIdToHostApiIndex(PaHostApiTypeId type)
{
	return Pa_HostApiTypeIdToHostApiIndex(type);
}

PA19_API PaDeviceIndex CCONV PA_HostApiDeviceIndexToDeviceIndex(PaHostApiIndex hostApi, int hostApiDeviceIndex)
{
	return Pa_HostApiDeviceIndexToDeviceIndex(hostApi, hostApiDeviceIndex);
}

PA19_API const PaHostErrorInfo* CCONV PA_GetLastHostErrorInfo()
{
	return Pa_GetLastHostErrorInfo();
}

PA19_API PaDeviceIndex CCONV PA_GetDeviceCount()
{
	return Pa_GetDeviceCount();
}

PA19_API PaDeviceIndex CCONV PA_GetDefaultInputDevice()
{
	return Pa_GetDefaultInputDevice();
}

PA19_API PaDeviceIndex CCONV PA_GetDefaultOutputDevice()
{
	return Pa_GetDefaultOutputDevice();
}

PA19_API const PaDeviceInfo* CCONV PA_GetDeviceInfo(PaDeviceIndex device)
{
	return Pa_GetDeviceInfo(device);
}

PA19_API PaError CCONV PA_IsFormatSupported(const PaStreamParameters *inputParameters,
									  const PaStreamParameters *outputParameters, double sampleRate) 
{
	return Pa_IsFormatSupported(inputParameters, outputParameters, sampleRate);
}

PA19_API PaError CCONV PA_OpenStream(PaStream **stream, const PaStreamParameters *inputParameters,
							   const PaStreamParameters *outputParameters, double sampleRate,
							   unsigned long framesPerBuffer, PaStreamFlags streamFlags,
							   PaStreamCallback *streamCallback, void *userData)
{
	if(userData)
	{
		streamCB2 = streamCallback;
		return Pa_OpenStream(stream, inputParameters, outputParameters, sampleRate, framesPerBuffer,
			 streamFlags, myStreamCallback2, userData);
	}
	else
	{
		streamCB = streamCallback;
		return Pa_OpenStream(stream, inputParameters, outputParameters, sampleRate, framesPerBuffer,
			 streamFlags, myStreamCallback, userData);
	}
}

PA19_API PaError CCONV PA_OpenDefaultStream(PaStream **stream, int numInputChannels, int numOutputChannels,
									  PaSampleFormat sampleFormat, double sampleRate,
									  unsigned long framesPerBuffer, PaStreamCallback *streamCallback,
									  void *userData)
{
	if(userData)
	{
		streamCB2 = streamCallback;
		return Pa_OpenDefaultStream(stream, numInputChannels, numOutputChannels, sampleFormat, sampleRate,
				framesPerBuffer, myStreamCallback2, userData);
	}
	else
	{
		streamCB = streamCallback;
		return Pa_OpenDefaultStream(stream, numInputChannels, numOutputChannels, sampleFormat, sampleRate,
				framesPerBuffer, myStreamCallback, userData);
	}
}

PA19_API PaError CCONV PA_CloseStream(PaStream *stream) 
{
	return Pa_CloseStream(stream);
}

PA19_API PaError CCONV PA_SetStreamFinishedCallback(PaStream *stream, PaStreamFinishedCallback *streamFinishedCallback)
{
	streamFinishedCB = streamFinishedCallback;
	return Pa_SetStreamFinishedCallback(stream, myStreamFinishedCallback);
}

PA19_API PaError CCONV PA_StartStream(PaStream *stream)
{
	return Pa_StartStream(stream);
}

PA19_API PaError CCONV PA_StopStream(PaStream *stream)
{
	return Pa_StopStream(stream);
}

PA19_API PaError CCONV PA_AbortStream(PaStream *stream)
{
	return Pa_AbortStream(stream);
}

PA19_API PaError CCONV PA_IsStreamStopped(PaStream *stream) 
{
	return Pa_IsStreamStopped(stream);
}

PA19_API PaError CCONV PA_IsStreamActive(PaStream *stream) 
{
	return Pa_IsStreamActive(stream);
}

PA19_API const PaStreamInfo* CCONV PA_GetStreamInfo(PaStream *stream) 
{
	return Pa_GetStreamInfo(stream);
}

PA19_API PaTime CCONV PA_GetStreamTime(PaStream *stream)
{
	return Pa_GetStreamTime(stream);
}

PA19_API double CCONV PA_GetStreamCpuLoad(PaStream *stream) 
{
	return Pa_GetStreamCpuLoad(stream);
}

PA19_API PaError CCONV PA_ReadStream(PaStream *stream, void *buffer, unsigned long frames) 
{
	return Pa_ReadStream(stream, buffer, frames);
}

PA19_API PaError CCONV PA_WriteStream(PaStream *stream, const void *buffer, unsigned long frames) 
{
	return Pa_WriteStream(stream, buffer, frames);
}

PA19_API signed long CCONV PA_GetStreamReadAvailable(PaStream *stream) 
{
	return Pa_GetStreamReadAvailable(stream);
}

PA19_API signed long CCONV PA_GetStreamWriteAvailable(PaStream *stream)
{
	return Pa_GetStreamWriteAvailable(stream);
}

PA19_API PaError CCONV PA_GetSampleSize(PaSampleFormat format) 
{
	return Pa_GetSampleSize(format);
}

PA19_API void CCONV PA_Sleep(long msec)
{
	Pa_Sleep(msec);
}

#ifdef __cplusplus
}
#endif
