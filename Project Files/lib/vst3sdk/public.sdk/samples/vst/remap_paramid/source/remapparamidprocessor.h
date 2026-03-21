//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/remap_paramid/source/remapparamidprocessor.h
// Created by  : Steinberg, 02/2024
// Description : Remap ParamID Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "public.sdk/source/vst/vstaudioeffect.h"

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
//  TestRemapParamIDProcessor
//------------------------------------------------------------------------
class TestRemapParamIDProcessor : public AudioEffect
{
public:
	TestRemapParamIDProcessor ();
	~TestRemapParamIDProcessor () SMTG_OVERRIDE;

	// Create function
	static FUnknown* createInstance (void* /*context*/)
	{
		return (Vst::IAudioProcessor*)new TestRemapParamIDProcessor;
	}

	//--- ---------------------------------------------------------------------
	// AudioEffect overrides:
	//--- ---------------------------------------------------------------------
	/** Called at first after constructor */
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

	/** Bus arrangement managing: in this example we support only Stereo/Stereo */
	tresult PLUGIN_API setBusArrangements (SpeakerArrangement* inputs, int32 numIns,
	                                       SpeakerArrangement* outputs,
	                                       int32 numOuts) SMTG_OVERRIDE;

	/** Asks if a given sample size is supported see SymbolicSampleSizes. */
	tresult PLUGIN_API canProcessSampleSize (int32 symbolicSampleSize) SMTG_OVERRIDE;

	/** Here we go...the process call */
	tresult PLUGIN_API process (Vst::ProcessData& data) SMTG_OVERRIDE;

	/** For persistence */
	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;

//------------------------------------------------------------------------
protected:
	//--- ---------------------------------------------------------------------
	template <typename SampleType>
	SampleType processAudio (SampleType** in, SampleType** out, int32 numChannels,
	                         int32 sampleFrames, float gain)
	{
		SampleType vuPPM = 0;

		// in real Plug-in it would be better to do dezippering to avoid jump (click) in gain value
		for (int32 i = 0; i < numChannels; i++)
		{
			int32 samples = sampleFrames;
			auto* ptrIn = (SampleType*)in[i];
			auto* ptrOut = (SampleType*)out[i];
			SampleType tmp;
			while (--samples >= 0)
			{
				// apply gain
				tmp = (*ptrIn++) * gain;
				(*ptrOut++) = tmp;

				// check only positive values
				if (tmp > vuPPM)
				{
					vuPPM = tmp;
				}
			}
		}
		return vuPPM;
	}

	float fGain {1.f};
	bool bBypass {false};
};

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
