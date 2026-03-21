//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/remap_paramid/source/remapparamidcontroller.cpp
// Created by  : Steinberg, 02/2024
// Description : Remap ParamID Example for VST 3 (by replacing AGain)
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "remapparamidcontroller.h"
#include "remapparamidcids.h"
#include "base/source/fstreamer.h"

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
// TestRemapParamIDController Implementation
//------------------------------------------------------------------------
tresult PLUGIN_API TestRemapParamIDController::initialize (FUnknown* context)
{
	// Here the Plug-in will be instantiated

	//---do not forget to call parent ------
	tresult result = EditController::initialize (context);
	if (result != kResultOk)
	{
		return result;
	}

	//---this parameter is the one which is compatible with the kGainId parameter of AGain
	parameters.addParameter (STR16 ("compatible Gain"), nullptr, 0, 0.5,
	                         ParameterInfo::kCanAutomate, kMyGainParamTag);

	//---Bypass parameter---
	parameters.addParameter (STR16 ("Bypass"), nullptr, 1, 0,
	                         ParameterInfo::kCanAutomate | ParameterInfo::kIsBypass, kBypassId);

	return result;
}

//------------------------------------------------------------------------
tresult PLUGIN_API TestRemapParamIDController::setComponentState (IBStream* state)
{
	// Here you get the state of the component (Processor part)
	if (!state)
		return kResultFalse;

	//---Compatible with AGain state -----------
	IBStreamer streamer (state, kLittleEndian);
	float savedGain = 0.f;
	if (streamer.readFloat (savedGain) == false)
		return kResultFalse;
	setParamNormalized (kMyGainParamTag, savedGain);

	// jump the GainReduction
	streamer.seek (sizeof (float), kSeekCurrent);

	int32 bypassState = 0;
	if (streamer.readInt32 (bypassState) == false)
		return kResultFalse;
	setParamNormalized (kBypassId, bypassState ? 1 : 0);

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API TestRemapParamIDController::getCompatibleParamID (const TUID pluginToReplaceUID,
                                                                     Vst::ParamID oldParamID,
                                                                     Vst::ParamID& newParamID)
{
	//--- We want to replace the AGain plug-in-------
	//--- check if the host is asking for remapping parameter of the AGain plug-in
	static const FUID AGainProcessorUID (0x84E8DE5F, 0x92554F53, 0x96FAE413, 0x3C935A18);
	FUID uidToCheck (FUID::fromTUID (pluginToReplaceUID));
	if (AGainProcessorUID != uidToCheck)
		return kResultFalse;

	//--- host wants to remap from AGain------------
	newParamID = -1;
	switch (oldParamID)
	{
		//--- map the kGainId (0) to our param kMyGainParamTag
		case 0:
		{
			newParamID = kMyGainParamTag;
			break;
		}
	}
	//--- return kResultTrue if the mapping happens------------
	return (newParamID == -1) ? kResultFalse : kResultTrue;
}

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
