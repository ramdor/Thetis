//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/syncdelay/source/syncdelayprocessor.h
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

#pragma once

#include "public.sdk/source/vst/vstaudioeffect.h"
#include "public.sdk/source/vst/vstbypassprocessor.h"

namespace Steinberg {
namespace Vst {

//-----------------------------------------------------------------------------
class SyncDelayProcessor : public AudioEffect
{
public:
	SyncDelayProcessor ();

	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API setBusArrangements (SpeakerArrangement* inputs, int32 numIns,
	                                       SpeakerArrangement* outputs,
	                                       int32 numOuts) SMTG_OVERRIDE;

	tresult PLUGIN_API setActive (TBool state) SMTG_OVERRIDE;
	tresult PLUGIN_API setProcessing (TBool state) SMTG_OVERRIDE;
	tresult PLUGIN_API process (ProcessData& data) SMTG_OVERRIDE;

//------------------------------------------------------------------------
	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;

	static FUnknown* createInstance (void*)
	{
		return static_cast<IAudioProcessor*> (new SyncDelayProcessor ());
	}

protected:
	void doParameterChanges (IParameterChanges& changes);
	void calculateDelay ();
	bool resetDelay ();

	BypassProcessor<Vst::Sample32> mBypassProcessor;

	uint32 mDelayIndex {0};
	uint32 mDelayInSamples {1};
	uint32 mBufferSizeInSamples {0};
	double mTempo {120};
	float** mBuffer {nullptr};
	uint32 mBufferPos {0};
	int32 mNumChannels {0};
};

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
