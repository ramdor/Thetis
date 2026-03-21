//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/adelay/source/adelaycontroller.cpp
// Created by  : Steinberg, 06/2009
// Description : 
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "adelaycontroller.h"
#include "adelayids.h"
#include "pluginterfaces/base/ibstream.h"

#if TARGET_OS_IPHONE
#include "interappaudio/iosEditor.h"
#endif
#include "base/source/fstreamer.h"

namespace Steinberg {
namespace Vst {

DEF_CLASS_IID (IDelayTestController)

//-----------------------------------------------------------------------------
tresult PLUGIN_API ADelayController::initialize (FUnknown* context)
{
	tresult result = EditController::initialize (context);
	if (result == kResultTrue)
	{
		parameters.addParameter (STR16 ("Bypass"), nullptr, 1, 0, ParameterInfo::kCanAutomate|ParameterInfo::kIsBypass, kBypassId);

		parameters.addParameter (STR16 ("Delay"), STR16 ("sec"), 0, 1, ParameterInfo::kCanAutomate, kDelayId);
	}
	return kResultTrue;
}

#if TARGET_OS_IPHONE
//-----------------------------------------------------------------------------
IPlugView* PLUGIN_API ADelayController::createView (FIDString name)
{
	if (FIDStringsEqual (name, ViewType::kEditor))
	{
		return new ADelayEditorForIOS (this);
	}
	return 0;
}
#endif

//------------------------------------------------------------------------
tresult PLUGIN_API ADelayController::setComponentState (IBStream* state)
{
	// we receive the current state of the component (processor part)
	// we read only the gain and bypass value...
	if (!state)
		return kResultFalse;
	
	IBStreamer streamer (state, kLittleEndian);
	float savedDelay = 0.f;
	if (streamer.readFloat (savedDelay) == false)
		return kResultFalse;
	setParamNormalized (kDelayId, static_cast<ParamValue> (savedDelay));

	int32 bypassState = 0;
	if (streamer.readInt32 (bypassState) == false)
	{
		// could be an old version, continue 
	}
	setParamNormalized (kBypassId, bypassState ? 1 : 0);

	return kResultOk;
}

//------------------------------------------------------------------------
bool PLUGIN_API ADelayController::doTest ()
{
	// this is called when running thru the validator
	// we can now run our own test cases
	return true;
}

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
