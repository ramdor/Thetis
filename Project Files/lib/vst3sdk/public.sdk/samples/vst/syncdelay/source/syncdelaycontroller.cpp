//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/syncdelay/source/syncdelaycontroller.cpp
// Created by  : Steinberg, 01/2020
// Description :
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "syncdelaycontroller.h"
#include "sync.h"
#include "syncdelayids.h"
#include "base/source/fstreamer.h"
#include "pluginterfaces/base/futils.h"
#include "pluginterfaces/base/ibstream.h"
#include "pluginterfaces/base/ustring.h"

namespace Steinberg {
namespace Vst {

//-----------------------------------------------------------------------------
tresult PLUGIN_API SyncDelayController::initialize (FUnknown* context)
{
	auto result = EditController::initialize (context);
	if (result == kResultTrue)
	{
		auto delayParam = new StringListParameter (STR16 ("Delay"), kDelayId, nullptr);
		for (const auto& entry : Synced)
			delayParam->appendString (entry.title);

		parameters.addParameter (delayParam); // parameters takes ownership of delayParam

		parameters.addParameter (STR16 ("Bypass"), nullptr, 1, 0,
		                         ParameterInfo::kCanAutomate | ParameterInfo::kIsBypass, kBypassId);
	}
	return kResultTrue;
}

//------------------------------------------------------------------------
tresult PLUGIN_API SyncDelayController::setComponentState (IBStream* state)
{
	if (!state)
		return kResultFalse;

	IBStreamer streamer (state, kLittleEndian);

	uint32 savedDelay = 0;
	if (streamer.readInt32u (savedDelay) == false)
		return kResultFalse;
	int32 savedBypassState = 0;
	if (streamer.readInt32 (savedBypassState) == false)
		return kResultFalse;

	setParamNormalized (kDelayId, ToNormalized<ParamValue> (savedDelay, static_cast<int32> (Synced.size () - 1)));
	setParamNormalized (kBypassId, savedBypassState ? 1 : 0);

	return kResultOk;
}

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
