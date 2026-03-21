//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/utf16name/source/utf16nameprocessor.cpp
// Created by  : Steinberg, 11/2023
// Description : UTF16Name Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "utf16nameprocessor.h"
#include "utf16namecids.h"

#include "base/source/fstreamer.h"
#include "pluginterfaces/vst/ivstparameterchanges.h"

namespace Steinberg {
//------------------------------------------------------------------------
// UTF16NameProcessor
//------------------------------------------------------------------------
UTF16NameProcessor::UTF16NameProcessor ()
{
	//--- set the wanted controller for our processor
	setControllerClass (kUTF16NameControllerUID);
}

//------------------------------------------------------------------------
UTF16NameProcessor::~UTF16NameProcessor ()
{
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameProcessor::initialize (FUnknown* context)
{
	// Here the Plug-in will be instantiated

	//---always initialize the parent-------
	tresult result = AudioEffect::initialize (context);
	// if everything Ok, continue
	if (result != kResultOk)
	{
		return result;
	}

	//--- create Audio IO ------
	addAudioInput (STR16 ("Stereo In-öüäéèê-やあ-مرحبًا"), Steinberg::Vst::SpeakerArr::kStereo);
	addAudioOutput (STR16 ("Stereo Out-öüäéèê-やあ-مرحبًا"), Steinberg::Vst::SpeakerArr::kStereo);

	/* If you don't need an event bus, you can remove the next line */
	addEventInput (STR16 ("Event In-öüäéèê-やあ-مرحبًا"), 1);

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameProcessor::terminate ()
{
	// Here the Plug-in will be de-instantiated, last possibility to remove some memory!

	//---do not forget to call parent ------
	return AudioEffect::terminate ();
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameProcessor::setActive (TBool state)
{
	//--- called when the Plug-in is enable/disable (On/Off) -----
	return AudioEffect::setActive (state);
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameProcessor::process (Vst::ProcessData& data)
{
	//--- First : Read inputs parameter changes-----------

	/*if (data.inputParameterChanges)
	{
	    int32 numParamsChanged = data.inputParameterChanges->getParameterCount ();
	    for (int32 index = 0; index < numParamsChanged; index++)
	    {
	        if (auto* paramQueue = data.inputParameterChanges->getParameterData (index))
	        {
	            Vst::ParamValue value;
	            int32 sampleOffset;
	            int32 numPoints = paramQueue->getPointCount ();
	            switch (paramQueue->getParameterId ())
	            {
	            }
	        }
	    }
	}*/

	//--- Here you have to implement your processing

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameProcessor::setupProcessing (Vst::ProcessSetup& newSetup)
{
	//--- called before any processing ----
	return AudioEffect::setupProcessing (newSetup);
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameProcessor::canProcessSampleSize (int32 symbolicSampleSize)
{
	// by default kSample32 is supported
	if (symbolicSampleSize == Vst::kSample32)
		return kResultTrue;

	// disable the following comment if your processing support kSample64
	/* if (symbolicSampleSize == Vst::kSample64)
	    return kResultTrue; */

	return kResultFalse;
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameProcessor::setState (IBStream* state)
{
	// called when we load a preset, the model has to be reloaded
	IBStreamer streamer (state, kLittleEndian);

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameProcessor::getState (IBStream* state)
{
	// here we need to save the model
	IBStreamer streamer (state, kLittleEndian);

	return kResultOk;
}

//------------------------------------------------------------------------
} // namespace Steinberg
