//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again_sampleaccurate/source/agsa_controller.cpp
// Created by  : Steinberg, 04/2021
// Description : AGain with Sample Accurate Parameter Changes
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "agsa.h"
#include "public.sdk/source/vst/vsteditcontroller.h"
#include "base/source/fstreamer.h"

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace AgainSampleAccurate {

//------------------------------------------------------------------------
class Controller : public EditController
{
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API terminate () SMTG_OVERRIDE;
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;
};

//------------------------------------------------------------------------
tresult PLUGIN_API Controller::initialize (FUnknown* context)
{
	tresult result = EditController::initialize (context);
	if (result != kResultOk)
	{
		return result;
	}
	parameters.addParameter (STR ("Bypass"), nullptr, 1, 0.,
	                         ParameterInfo::kCanAutomate | ParameterInfo::kIsBypass,
	                         ParameterID::Bypass);
	parameters.addParameter (STR ("Gain"), STR ("%"), 0, 1., ParameterInfo::kCanAutomate,
	                         ParameterID::Gain);
	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API Controller::terminate ()
{
	return EditController::terminate ();
}

//------------------------------------------------------------------------
tresult PLUGIN_API Controller::setComponentState (IBStream* state)
{
	if (!state)
		return kInvalidArgument;

	IBStreamer streamer (state, kLittleEndian);

	uint32 numParams;
	if (streamer.readInt32u (numParams) == false)
		return kResultFalse;

	ParamID pid;
	ParamValue value;
	for (uint32 i = 0u; i < numParams; ++i)
	{
		if (!streamer.readInt32u (pid))
			break;
		if (!streamer.readDouble (value))
			break;
		if (auto param = parameters.getParameter (pid))
			param->setNormalized (value);
	}
	return kResultTrue;
}

//------------------------------------------------------------------------
FUnknown* createControllerInstance (void*)
{
	return static_cast<IEditController*> (new Controller);
}

//------------------------------------------------------------------------
} // AgainSampleAccurate
} // Vst
} // Steinberg
