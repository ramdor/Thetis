//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again/source/againsidechain.h
// Created by  : Steinberg, 04/2016
// Description : AGain Example for VST SDK 3.0
//               Simple gain plug-in with gain, bypass values and 1 midi input
//               and a sidechain
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "again.h"

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
// AGainWithSideChain: directly derived from AGain
//------------------------------------------------------------------------
class AGainWithSideChain : public AGain
{
public:
	// just overwrite some functions
	static FUnknown* createInstance (void* /*context*/)
	{
		return (IAudioProcessor*)new AGainWithSideChain;
	}

	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API process (ProcessData& data) SMTG_OVERRIDE;
	tresult PLUGIN_API setBusArrangements (SpeakerArrangement* inputs, int32 numIns,
	                                       SpeakerArrangement* outputs,
	                                       int32 numOuts) SMTG_OVERRIDE;

protected:
	//==============================================================================
	template <typename SampleType>
	SampleType processAudioWithSideChain (SampleType** in, SampleType** out, SampleType** aux,
	                                      int32 numChannels, int32 sampleFrames, float gain);
};

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
