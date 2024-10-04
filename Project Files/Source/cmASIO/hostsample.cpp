// hostsample.cpp : a simple ASIO host example.
// - instantiates the driver
// - get the information from the driver
// - built up some audio channels
// - plays silence for 20 seconds
// - destruct the driver
// Note: This sample cannot work with the "ASIO DirectX Driver" as it does
//       not have a valid Application Window handle, which is used as sysRef
//       on the Windows platform.


// ********** Note to openhpsdr.org maintainers **********
// This is a patched version of hostsample.cpp from the ASIO SDK.
// I have kept as much of it intact as possible, on the premise that the folks at Steinberg know more about ASIO than I do.
// Where possible, I have kept the original code in place, commented out, to make it easier to see where changes were made to accommodate The ChannelMaster.
// I feel in time much of the code here can be moved over to cmasio.c, leaving this file to serve as a thin wrapper around the c++ code in the ASIO SDK.
// Bryan W4WMT


#include <stdio.h>
#include <string.h>
#include "hostsample.h"
#include "asiosys.h"
#include "asio.h"
#include "asiodrivers.h"

// name of the ASIO device to be used
#if WINDOWS
//#define ASIO_DRIVER_NAME    "ASIO Multimedia Driver"
//#define ASIO_DRIVER_NAME    "ASIO Lynx"
//#define ASIO_DRIVER_NAME    "Studio 192 ASIO"
//#define ASIO_DRIVER_NAME    "UMC ASIO Driver"
//#define ASIO_DRIVER_NAME    "Focusrite USB ASIO"
//#define ASIO_DRIVER_NAME    "AudioBox ASIO Driver"
//#define ASIO_DRIVER_NAME    "MOTU M Series"
//#define ASIO_DRIVER_NAME    "Voicemeeter Virtual ASIO"
#elif MAC
//	#define ASIO_DRIVER_NAME   	"Apple Sound Manager"
	#define ASIO_DRIVER_NAME   	"ASIO Sample"
#endif


#define TEST_RUN_TIME  20.0		// run for 20 seconds


enum {
	// number of input and outputs supported by the host application
	// you can change these to higher or lower values
	//kMaxInputChannels = 32,
	//kMaxOutputChannels = 32
	//ChannelMaster only needs 2 input and 2 output channels
	kMaxInputChannels = 2,
	kMaxOutputChannels = 2
};


// internal data storage
typedef struct DriverInfo
{
	// ASIOInit()
	ASIODriverInfo driverInfo;

	// ASIOGetChannels()
	long           inputChannels;
	long           outputChannels;

	// ASIOGetBufferSize()
	long           minSize;
	long           maxSize;
	long           preferredSize;
	long           granularity;

	// ASIOGetSampleRate()
	ASIOSampleRate sampleRate;

	// ASIOOutputReady()
	bool           postOutput;

	// ASIOGetLatencies ()
	long           inputLatency;
	long           outputLatency;

	// ASIOCreateBuffers ()
	long inputBuffers;	// becomes number of actual created input buffers
	long outputBuffers;	// becomes number of actual created output buffers
	ASIOBufferInfo bufferInfos[kMaxInputChannels + kMaxOutputChannels]; // buffer info's

	// ASIOGetChannelInfo()
	ASIOChannelInfo channelInfos[kMaxInputChannels + kMaxOutputChannels]; // channel info's
	// The above two arrays share the same indexing, as the data in them are linked together

	// Information from ASIOGetSamplePosition()
	// data is converted to double floats for easier use, however 64 bit integer can be used, too
	double         nanoSeconds;
	double         samples;
	double         tcSamples;	// time code samples

	// bufferSwitchTimeInfo()
	ASIOTime       tInfo;			// time info state
	unsigned long  sysRefTime;      // system reference time, when bufferSwitch() was called

	// Signal the end of processing in this example
	bool           stopped;

	//cmASIO()
	char requestedDriverName[32];
	int requestedBufferSize;
	int requestedSampleRate;
	void (*CallbackASIO)(void* inputL, void* inputR, void* outputL, void* outputR);
} DriverInfo;


DriverInfo asioDriverInfo = {0};	// ***WARNING***  Several functions below use a locally scoped pointer variable having the same symbol as this asioDriverInfo field variable
ASIOCallbacks asioCallbacks;

//----------------------------------------------------------------------------------
// some external references
extern AsioDrivers* asioDrivers;
bool loadAsioDriver(char *name);

// internal prototypes (required for the Metrowerks CodeWarrior compiler)
//int main(int argc, char* argv[]);
//long init_asio_static_data (DriverInfo *asioDriverInfo);
//ASIOError create_asio_buffers (DriverInfo *asioDriverInfo);
//unsigned long get_sys_reference_time();


// callback prototypes
void bufferSwitch(long index, ASIOBool processNow);
ASIOTime *bufferSwitchTimeInfo(ASIOTime *timeInfo, long index, ASIOBool processNow);
void sampleRateChanged(ASIOSampleRate sRate);
long asioMessages(long selector, long value, void* message, double* opt);



//----------------------------------------------------------------------------------
long init_asio_static_data (DriverInfo *asioDriverInfo)
{	// collect the informational data of the driver
	// get the number of available channels
	char buf[128];
	if(ASIOGetChannels(&asioDriverInfo->inputChannels, &asioDriverInfo->outputChannels) == ASE_OK)
	{
		sprintf_s (buf, 128, "ASIOGetChannels (inputs: %d, outputs: %d);\n", asioDriverInfo->inputChannels, asioDriverInfo->outputChannels);
		OutputDebugStringA(buf);

		// get the usable buffer sizes
		if(ASIOGetBufferSize(&asioDriverInfo->minSize, &asioDriverInfo->maxSize, &asioDriverInfo->preferredSize, &asioDriverInfo->granularity) == ASE_OK)
		{
			sprintf_s (buf, 128, "ASIOGetBufferSize (min: %d, max: %d, preferred: %d, granularity: %d);\n",
					 asioDriverInfo->minSize, asioDriverInfo->maxSize,
					 asioDriverInfo->preferredSize, asioDriverInfo->granularity);
			OutputDebugStringA(buf);

			// get the currently selected sample rate
			if(ASIOGetSampleRate(&asioDriverInfo->sampleRate) == ASE_OK)
			{
				sprintf_s (buf, 128, "ASIOGetSampleRate (sampleRate: %f);\n", asioDriverInfo->sampleRate);
				OutputDebugStringA(buf);
				//if (asioDriverInfo->sampleRate <= 0.0 || asioDriverInfo->sampleRate > 96000.0)
				if (asioDriverInfo->sampleRate != asioDriverInfo->requestedSampleRate)
				{
					// Driver does not store it's internal sample rate, so set it to a know one.
					// Usually you should check beforehand, that the selected sample rate is valid
					// with ASIOCanSampleRate().
					//if(ASIOSetSampleRate(44100.0) == ASE_OK)
					sprintf_s(buf, 128, "Trying to change driver sample rate...");
					OutputDebugStringA(buf);
					if (ASIOSetSampleRate(asioDriverInfo->requestedSampleRate) == ASE_OK)
					{
						if (ASIOGetSampleRate(&asioDriverInfo->sampleRate) == ASE_OK)
						{
							sprintf_s(buf, 128, "ASIOGetSampleRate (sampleRate: %f);\n", asioDriverInfo->sampleRate);
							OutputDebugStringA(buf);
						}
						else
							return -6;
					}
					else
						return -5;
				}
				//ASIOSetSampleRate(48000.0);
				//ASIOSetSampleRate(asioDriverInfo->requestedSampleRate);

				// check wether the driver requires the ASIOOutputReady() optimization
				// (can be used by the driver to reduce output latency by one block)
				//if(ASIOOutputReady() == ASE_OK)
				//	asioDriverInfo->postOutput = true;
				//else
				//	asioDriverInfo->postOutput = false;
				//sprintf_s (buf, 128, "ASIOOutputReady(); - %s\n", asioDriverInfo->postOutput ? "Supported" : "Not supported");
				//OutputDebugStringA(buf);

				return 0;
			}
			return -3;
		}
		return -2;
	}
	return -1;
}


//----------------------------------------------------------------------------------
// conversion from 64 bit ASIOSample/ASIOTimeStamp to double float
#if NATIVE_INT64
	#define ASIO64toDouble(a)  (a)
#else
	const double twoRaisedTo32 = 4294967296.;
	#define ASIO64toDouble(a)  ((a).lo + (a).hi * twoRaisedTo32)
#endif

ASIOTime *bufferSwitchTimeInfo(ASIOTime *timeInfo, long index, ASIOBool processNow)
{	// the actual processing callback.
	// Beware that this is normally in a seperate thread, hence be sure that you take care
	// about thread synchronization. This is omitted here for simplicity.

	// store the timeInfo for later use
	asioDriverInfo.tInfo = *timeInfo;

	// perform the processing
	asioDriverInfo.CallbackASIO(asioDriverInfo.bufferInfos[0].buffers[index], asioDriverInfo.bufferInfos[1].buffers[index], asioDriverInfo.bufferInfos[2].buffers[index], asioDriverInfo.bufferInfos[3].buffers[index]);

	// finally if the driver supports the ASIOOutputReady() optimization, do it here, all data are in place
	//if (asioDriverInfo.postOutput)
	//	ASIOOutputReady();

	return 0L;
}

//----------------------------------------------------------------------------------
void bufferSwitch(long index, ASIOBool processNow)
{	// the actual processing callback.
	// Beware that this is normally in a seperate thread, hence be sure that you take care
	// about thread synchronization. This is omitted here for simplicity.

	// as this is a "back door" into the bufferSwitchTimeInfo a timeInfo needs to be created
	// though it will only set the timeInfo.samplePosition and timeInfo.systemTime fields and the according flags
	ASIOTime  timeInfo;
	memset (&timeInfo, 0, sizeof (timeInfo));

	// get the time stamp of the buffer, not necessary if no
	// synchronization to other media is required
	if(ASIOGetSamplePosition(&timeInfo.timeInfo.samplePosition, &timeInfo.timeInfo.systemTime) == ASE_OK)
		timeInfo.timeInfo.flags = kSystemTimeValid | kSamplePositionValid;

	bufferSwitchTimeInfo (&timeInfo, index, processNow);
}


//----------------------------------------------------------------------------------
void sampleRateChanged(ASIOSampleRate sRate)
{
	// do whatever you need to do if the sample rate changed
	// usually this only happens during external sync.
	// Audio processing is not stopped by the driver, actual sample rate
	// might not have even changed, maybe only the sample rate status of an
	// AES/EBU or S/PDIF digital input at the audio device.
	// You might have to update time/sample related conversion routines, etc.
}

//----------------------------------------------------------------------------------
long asioMessages(long selector, long value, void* message, double* opt)
{
	// currently the parameters "value", "message" and "opt" are not used.
	long ret = 0;
	switch(selector)
	{
		case kAsioSelectorSupported:
			if(value == kAsioResetRequest
			|| value == kAsioEngineVersion
			|| value == kAsioResyncRequest
			|| value == kAsioLatenciesChanged
			// the following three were added for ASIO 2.0, you don't necessarily have to support them
			|| value == kAsioSupportsTimeInfo
			|| value == kAsioSupportsTimeCode
			|| value == kAsioSupportsInputMonitor)
				ret = 1L;
			break;
		case kAsioResetRequest:
			// defer the task and perform the reset of the driver during the next "safe" situation
			// You cannot reset the driver right now, as this code is called from the driver.
			// Reset the driver is done by completely destruct is. I.e. ASIOStop(), ASIODisposeBuffers(), Destruction
			// Afterwards you initialize the driver again.
			asioDriverInfo.stopped;  // In this sample the processing will just stop
			ret = 1L;
			break;
		case kAsioResyncRequest:
			// This informs the application, that the driver encountered some non fatal data loss.
			// It is used for synchronization purposes of different media.
			// Added mainly to work around the Win16Mutex problems in Windows 95/98 with the
			// Windows Multimedia system, which could loose data because the Mutex was hold too long
			// by another thread.
			// However a driver can issue it in other situations, too.
			ret = 1L;
			break;
		case kAsioLatenciesChanged:
			// This will inform the host application that the drivers were latencies changed.
			// Beware, it this does not mean that the buffer sizes have changed!
			// You might need to update internal delay data.
			ret = 1L;
			break;
		case kAsioEngineVersion:
			// return the supported ASIO version of the host application
			// If a host applications does not implement this selector, ASIO 1.0 is assumed
			// by the driver
			ret = 2L;
			break;
		case kAsioSupportsTimeInfo:
			// informs the driver wether the asioCallbacks.bufferSwitchTimeInfo() callback
			// is supported.
			// For compatibility with ASIO 1.0 drivers the host application should always support
			// the "old" bufferSwitch method, too.
			ret = 1;
			break;
		case kAsioSupportsTimeCode:
			// informs the driver wether application is interested in time code info.
			// If an application does not need to know about time code, the driver has less work
			// to do.
			ret = 0;
			break;
	}
	return ret;
}


//----------------------------------------------------------------------------------
ASIOError create_asio_buffers (DriverInfo *asioDriverInfo)
{	// create buffers for all inputs and outputs of the card with the 
	// preferredSize from ASIOGetBufferSize() as buffer size
	long i;
	ASIOError result;
	char buf[128];

	// fill the bufferInfos from the start without a gap
	ASIOBufferInfo *info = asioDriverInfo->bufferInfos;

	// prepare inputs (Though this is not necessaily required, no opened inputs will work, too
	//if (asioDriverInfo->inputChannels > kMaxInputChannels)
	//	asioDriverInfo->inputBuffers = kMaxInputChannels;
	//else
	//	asioDriverInfo->inputBuffers = asioDriverInfo->inputChannels;
	if (asioDriverInfo->inputChannels >= kMaxInputChannels)
		asioDriverInfo->inputBuffers = kMaxInputChannels;
	else
		return ASE_InvalidMode;
	for(i = 0; i < asioDriverInfo->inputBuffers; i++, info++)
	{
		info->isInput = ASIOTrue;
		info->channelNum = i;
		info->buffers[0] = info->buffers[1] = 0;
	}

	// prepare outputs
	//if (asioDriverInfo->outputChannels > kMaxOutputChannels)
	//	asioDriverInfo->outputBuffers = kMaxOutputChannels;
	//else
	//	asioDriverInfo->outputBuffers = asioDriverInfo->outputChannels;
	if (asioDriverInfo->outputChannels >= kMaxOutputChannels)
		asioDriverInfo->outputBuffers = kMaxOutputChannels;
	else
		return ASE_InvalidMode;
	for(i = 0; i < asioDriverInfo->outputBuffers; i++, info++)
	{
		info->isInput = ASIOFalse;
		info->channelNum = i;
		info->buffers[0] = info->buffers[1] = 0;
	}

	// create and activate buffers
	result = ASIOCreateBuffers(asioDriverInfo->bufferInfos,
		asioDriverInfo->inputBuffers + asioDriverInfo->outputBuffers,
		asioDriverInfo->requestedBufferSize, &asioCallbacks); // Set Buffer Size Here!!!
	if (result == ASE_OK)
	{
		// now get all the buffer details, sample word length, name, word clock group and activation
		for (i = 0; i < asioDriverInfo->inputBuffers + asioDriverInfo->outputBuffers; i++)
		{
			asioDriverInfo->channelInfos[i].channel = asioDriverInfo->bufferInfos[i].channelNum;
			asioDriverInfo->channelInfos[i].isInput = asioDriverInfo->bufferInfos[i].isInput;
			result = ASIOGetChannelInfo(&asioDriverInfo->channelInfos[i]);
			if (result != ASE_OK)
				break;
		}

		if (result == ASE_OK)
		{
			// get the input and output latencies
			// Latencies often are only valid after ASIOCreateBuffers()
			// (input latency is the age of the first sample in the currently returned audio block)
			// (output latency is the time the first sample in the currently returned audio block requires to get to the output)
			result = ASIOGetLatencies(&asioDriverInfo->inputLatency, &asioDriverInfo->outputLatency);
			if (result == ASE_OK)
			{
				sprintf_s(buf, 128, "ASIOGetLatencies (input: %d, output: %d);\n", asioDriverInfo->inputLatency, asioDriverInfo->outputLatency);
				OutputDebugStringA(buf);
			}
		}
	}
	return result;
}

	int prepareASIO(int blocksize, int samplerate, char* asioDriverName, void (*CallbackASIO)(void* inputL, void* inputR, void* outputL, void* outputR))
{
	char buf[128];
	asioDriverInfo.requestedBufferSize = blocksize;
	asioDriverInfo.requestedSampleRate = samplerate;
	memcpy(asioDriverInfo.requestedDriverName, asioDriverName, 32);
	asioDriverInfo.CallbackASIO = CallbackASIO;

	// load the driver, this will setup all the necessary internal data structures
	if (loadAsioDriver (asioDriverInfo.requestedDriverName))
	{
		sprintf_s(buf, 128, "loadAsioDriver() OK\n");
		OutputDebugStringA(buf);
		// initialize the driver
		asioDriverInfo.driverInfo.asioVersion = 2;  //W4WMT
		if (ASIOInit (&asioDriverInfo.driverInfo) == ASE_OK)
		{
			sprintf_s(buf, 128, "ASIOInit() OK\n");
			OutputDebugStringA(buf);
			sprintf_s (buf, 128,
					"asioVersion:   %d\n"
					"driverVersion: %d\n"
					"Name:          %s\n"
					"ErrorMessage:  %s\n",
					asioDriverInfo.driverInfo.asioVersion, asioDriverInfo.driverInfo.driverVersion,
					asioDriverInfo.driverInfo.name, asioDriverInfo.driverInfo.errorMessage);
			OutputDebugStringA(buf);
			//ASIOSetSampleRate(samplerate);  Where is the best place to set the sample rate???
			if (init_asio_static_data (&asioDriverInfo) == 0)
			{
				sprintf_s(buf, 128, "init_asio_static_data() OK\n");
				OutputDebugStringA(buf);
				//ASIOControlPanel(); //you might want to check wether the ASIOControlPanel() can open

				// set up the asioCallback structure and create the ASIO data buffer
				asioCallbacks.bufferSwitch = &bufferSwitch;
				asioCallbacks.sampleRateDidChange = &sampleRateChanged;
				asioCallbacks.asioMessage = &asioMessages;
				asioCallbacks.bufferSwitchTimeInfo = &bufferSwitchTimeInfo;
				ASIOError result = create_asio_buffers(&asioDriverInfo);
				//if (create_asio_buffers (&asioDriverInfo) == ASE_OK)
				if (result == ASE_OK)
				{
					sprintf_s(buf, 128, "create_asio_buffers() OK\n\n");
					OutputDebugStringA(buf);

					for (int i = 0; i < asioDriverInfo.inputBuffers + asioDriverInfo.outputBuffers; i++)
					{
						if (asioDriverInfo.channelInfos[i].type != ASIOSTInt32LSB) result = ASE_InvalidMode;
						sprintf_s(buf, 128, "Channel %d:  type = %d   name = %s   isInput = %d   channelNum = %d", i, asioDriverInfo.channelInfos[i].type, asioDriverInfo.channelInfos[i].name, asioDriverInfo.bufferInfos[i].isInput, asioDriverInfo.bufferInfos[i].channelNum);
						OutputDebugStringA(buf);
					}

					ASIOGetBufferSize(&asioDriverInfo.minSize, &asioDriverInfo.maxSize, &asioDriverInfo.preferredSize, &asioDriverInfo.granularity);
					ASIOGetSampleRate(&asioDriverInfo.sampleRate);
					sprintf_s(buf, 128, "\nSample Rate = %f\nBuffer Size = %d", asioDriverInfo.sampleRate, asioDriverInfo.preferredSize);
					OutputDebugStringA(buf);

					if (result == ASE_InvalidMode)
					{
						sprintf_s(buf, 128, "Incompatible sample format type: %d", asioDriverInfo.channelInfos[0].type);
						OutputDebugStringA(buf);
						return result;
					}

					//ASIOError result;
					//result = ASIOStart();
					//if (result == ASE_OK)
					//{
					//	// Now all is up and running
					//	sprintf (buf, "\nASIO Driver started successfully.\n\n");
					//	OutputDebugStringA(buf);

					//	return 0;
					//}
					//sprintf(buf, "ASIOStart() FAILED: %d\n", result);
					//OutputDebugStringA(buf);
					//ASIODisposeBuffers();
					return 0;
				}
				else //error
				{
					sprintf_s(buf, 128, "create_asio_buffers() FAILED : ASIOError = %d\n\n", result);
					OutputDebugStringA(buf);
				}
			}
			ASIOExit();
		}
		else
		{
			sprintf_s(buf, 128, "ASIOInit() FAILED\n");
			OutputDebugStringA(buf);
		}
		asioDrivers->removeCurrentDriver();
	}
	else
	{
		sprintf_s(buf, 128, "loadAsioDriver() FAILED\n");
		OutputDebugStringA(buf);
	}
	//return 0;
	return 1;
}


void unloadASIO()
{
	int a =	ASIODisposeBuffers();
	int b =	ASIOExit();
	asioDrivers->removeCurrentDriver();
	char buf[128];
	sprintf_s(buf, 128, "unloadASIO results: %d : %d", a, b);
	OutputDebugStringA(buf);
}


long getASIODriverString(void* szData)
{
	//const HKEY hKeyPath = HKEY_LOCAL_MACHINE;
	const LPCSTR subKey = "SOFTWARE\\OpenHPSDR\\Thetis-x64";
	const LPCSTR valueName = "ASIOdrivername";

	DWORD szDataSize = 32;  //length of ASIO driver names are limited to 32 bytes, including the zero terminator

	// check Current User first
	HKEY hKeyPath = HKEY_CURRENT_USER;
	LSTATUS status = RegGetValueA(hKeyPath, subKey, valueName, RRF_RT_REG_SZ | RRF_SUBKEY_WOW6464KEY, NULL, szData, &szDataSize);
	if (*(char*)szData == 0) 
	{
		// not found under current user, try local machine
		hKeyPath = HKEY_LOCAL_MACHINE;
		status = RegGetValueA(hKeyPath, subKey, valueName, RRF_RT_REG_SZ | RRF_SUBKEY_WOW6464KEY, NULL, szData, &szDataSize);
	}
	char buf[128];
	sprintf_s(buf, 128, "RegGetValue(sz) status = %d", status);
	OutputDebugStringA(buf);
	if (*(char*)szData == 0) status = ERROR_FILE_NOT_FOUND;  //so if the string in the reg key value is empty, we don't try to open an ASIO driver

	return status;
}


long getASIOBlockNum(void* dwData)
{
	//const HKEY hKeyPath = HKEY_LOCAL_MACHINE;
	const LPCSTR subKey = "SOFTWARE\\OpenHPSDR\\Thetis-x64";
	const LPCSTR valueName = "ASIOblocknum";

	DWORD dwDataSize = sizeof(REG_DWORD);

	// check Current User first
	HKEY hKeyPath = HKEY_CURRENT_USER;
	LSTATUS status = RegGetValueA(hKeyPath, subKey, valueName, RRF_RT_REG_DWORD | RRF_SUBKEY_WOW6464KEY, NULL, dwData, &dwDataSize);
	if (status == ERROR_FILE_NOT_FOUND)
	{
		// not found under current user, try local machine
		hKeyPath = HKEY_LOCAL_MACHINE;
		status = RegGetValueA(hKeyPath, subKey, valueName, RRF_RT_REG_DWORD | RRF_SUBKEY_WOW6464KEY, NULL, dwData, &dwDataSize);
	}
	char buf[128];
	sprintf_s(buf, 128, "RegGetValue(dword) status = %d", status);
	OutputDebugStringA(buf);

	return status;
}

long asioStart()
{
	char buf[128];
	long result = ASIOStart();
	if (result == 0)
	{
		sprintf_s(buf, 128, "ASIOStart OK");
	}
	else
	{
		sprintf_s(buf, 128, "ASIOStart Failed = %d", result);
	}
	OutputDebugStringA(buf);
	return result;
}


long asioStop()
{
	char buf[128];
	long result = ASIOStop();
	if (result == 0)
	{
		sprintf_s(buf, 128, "ASIOStop OK");
	}
	else
	{
		sprintf_s(buf, 128, "ASIOStop Failed = %d", result);
	}
	OutputDebugStringA(buf);
	return result;
}


unsigned long get_sys_reference_time()
{	// get the system reference time
#if WINDOWS
	//return timeGetTime(); getting rid of this to avoid error about winmm.lib not being linked, nobody calls this func apparently
	return 0;
#elif MAC
static const double twoRaisedTo32 = 4294967296.;
	UnsignedWide ys;
	Microseconds(&ys);
	double r = ((double)ys.hi * twoRaisedTo32 + (double)ys.lo);
	return (unsigned long)(r / 1000.);
#endif
}
