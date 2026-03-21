//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/programchange/source/plugcontroller.cpp
// Created by  : Steinberg, 02/2016
// Description : Plug-in Example for VST SDK 3.x using ProgramChange parameter
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

#include "public.sdk/source/vst/utility/stringconvert.h"
#include "base/source/fstreamer.h"

#include "pluginterfaces/base/ibstream.h"
#include "pluginterfaces/base/futils.h"
#include "pluginterfaces/vst/ivstcomponent.h"

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
	int32 flags = ParameterInfo::kCanAutomate | ParameterInfo::kIsBypass;
	int32 tag = kBypassId;
	parameters.addParameter (STR ("Bypass"), nullptr, stepCount, defaultVal, flags, tag);

	// create top root unit with kProgramId as id for the programList
	addUnit (new Unit (STR ("Root"), kRootUnitId, kNoParentUnitId, kProgramId));

	// create the program list: here kNumProgs entries
	auto* prgList = new ProgramList (STR ("Bank"), kProgramId, kRootUnitId);
	addProgramList (prgList);
	for (int32 i = 0; i < kNumProgs; i++)
	{
		std::u16string title = STR ("Prog ");
		title += Vst::toString (i + 1);
		prgList->addProgram (title.data ());
	}

	//---Program Change parameter---
	Parameter* prgParam = prgList->getParameter ();

	// by default this program change parameter if automatable we can overwrite this:
	prgParam->getInfo ().flags &= ~ParameterInfo::kCanAutomate;

	parameters.addParameter (prgParam);

	//---Gain parameter---
	parameters.addParameter (STR16 ("Gain"), nullptr, 0, 1.f, ParameterInfo::kCanAutomate, kGainId);

	return result;
}

//-----------------------------------------------------------------------------
tresult PLUGIN_API PlugController::setParamNormalized (ParamID tag, ParamValue value)
{
	tresult res = EditControllerEx1::setParamNormalized (tag, value);
	if (res == kResultOk && tag == kProgramId) // program change
	{
		// here we use the 1-program as gain...just an example
		EditControllerEx1::setParamNormalized (kGainId, value);

		if (componentHandler)
			componentHandler->restartComponent (kParamValuesChanged);
	}
	return res;
}

//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::getUnitByBus (MediaType type, BusDirection dir, int32 busIndex,
	int32 channel, UnitID& unitId) 
{
	if (type == kEvent && dir == kInput && busIndex == 0 && channel == 0)
	{
		unitId = kRootUnitId;
		return kResultTrue;
	}

	return kResultFalse;
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

	// read the program
	int32 programState = 0;
	if (streamer.readInt32 (programState) == false)
		return kResultFalse;

	EditControllerEx1::setParamNormalized (kProgramId,
	                                       ToNormalized<ParamValue> (programState, kNumProgs - 1));

	// read the Gain param
	float savedGain = 0.f;
	if (streamer.readFloat (savedGain) == false)
		return kResultFalse;
	setParamNormalized (kGainId, savedGain);

	return kResultOk;
}
}} // namespaces
