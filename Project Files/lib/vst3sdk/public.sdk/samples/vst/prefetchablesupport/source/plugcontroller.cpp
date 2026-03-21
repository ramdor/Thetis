//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/prefetchablesupport/source/plugcontroller.cpp
// Created by  : Steinberg, 04/2015
// Description : Plug Controller Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------


#include "plugcontroller.h"
#include "plugparamids.h"

#include "pluginterfaces/base/ibstream.h"
#include "pluginterfaces/vst/ivstprefetchablesupport.h"
#include "base/source/fstreamer.h"

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
// PlugController Implementation
//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::initialize (FUnknown* context)
{
	tresult result = EditControllerEx1::initialize (context);
	if (result != kResultOk)
	{
		return result;
	}

	//---Create Parameters------------

	//---Bypass parameter---
	int32 stepCount = 1;
	ParamValue defaultVal = 0;
	int32 flags = ParameterInfo::kCanAutomate|ParameterInfo::kIsBypass;
	int32 tag = kBypassId;
	parameters.addParameter (STR16 ("Bypass"), nullptr, stepCount, defaultVal, flags, tag);

	//---PrefetchMode parameter
	tag = kPrefetchableMode;
	auto* prefetchList = new StringListParameter (STR16 ("Prefetch Mode"), tag);
	parameters.addParameter (prefetchList);

	prefetchList->appendString (STR16 ("Is Never"));
	prefetchList->appendString (STR16 ("Is Yet"));
	prefetchList->appendString (STR16 ("Is Not Yet"));
	prefetchList->setNormalized (kIsYetPrefetchable / (kNumPrefetchableSupport - 1));

	return result;
}

//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::setComponentState (IBStream* state)
{
	// we receive the current state of the component (processor part)
	// we read only the gain and bypass value...

	if (!state)
		return kResultFalse;

	IBStreamer streamer (state, kLittleEndian);

	// read the bypass
	int32 bypassState = 0;
	if (streamer.readInt32 (bypassState) == false)
		return kResultFalse;
	setParamNormalized (kBypassId, bypassState ? 1 : 0);

	int32 prefetchableMode;
	if (streamer.readInt32 (prefetchableMode) == false)
		return kResultFalse;
	setParamNormalized (kPrefetchableMode, prefetchableMode / (kNumPrefetchableSupport - 1));

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::setParamNormalized (ParamID tag, ParamValue value)
{
	tresult res = kResultFalse;

	if (tag == kPrefetchableMode)
	{
		if (value != getParamNormalized (tag))
		{
			res = EditControllerEx1::setParamNormalized (tag, value);
			getComponentHandler ()->restartComponent (kPrefetchableSupportChanged);
		}
	}
	else
		res = EditControllerEx1::setParamNormalized (tag, value);

	return res;
}

}} // namespaces
