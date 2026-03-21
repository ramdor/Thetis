#include "vst3_sdk.h"

#include "pluginterfaces/vst/ivsthostapplication.h"
#include "pluginterfaces/vst/vsttypes.h"
#include "public.sdk/source/vst/hosting/hostclasses.h"
#include "public.sdk/source/vst/hosting/module.h"

#define VST3SDK_WIDEN_IMPL(value) L##value
#define VST3SDK_WIDEN(value) VST3SDK_WIDEN_IMPL(value)

namespace
{
	const wchar_t* const kVst3SdkVersion = VST3SDK_WIDEN(kVstVersionString);
}

int Vst3Sdk_IsAvailable(void)
{
	return VST_VERSION > 0 ? 1 : 0;
}

const wchar_t* Vst3Sdk_GetVersionString(void)
{
	return kVst3SdkVersion;
}
