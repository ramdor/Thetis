//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again/source/again.h
// Created by  : Steinberg, 04/2005
// Description : AGain Example for VST SDK 3.0
//               Simple gain plug-in with gain, bypass values and 1 midi input
//               and the same plug-in with sidechain 
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
// AGain: directly derived from the helper class AudioEffect
//------------------------------------------------------------------------
class AGain : public AudioEffect
{
public:
	AGain ();
	~AGain () override;

	//--- ---------------------------------------------------------------------
	// create function required for plug-in factory,
	// it will be called to create new instances of this plug-in
	//--- ---------------------------------------------------------------------
	static FUnknown* createInstance (void* /*context*/) { return (IAudioProcessor*)new AGain; }

	//--- ---------------------------------------------------------------------
	// AudioEffect overrides:
	//--- ---------------------------------------------------------------------
	/** Called at first after constructor */
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

	/** Called at the end before destructor */
	tresult PLUGIN_API terminate () SMTG_OVERRIDE;

	/** Switch the plug-in on/off */
	tresult PLUGIN_API setActive (TBool state) SMTG_OVERRIDE;

	/** Here we go...the process call */
	tresult PLUGIN_API process (ProcessData& data) SMTG_OVERRIDE;

	/** Test of a communication channel between controller and component */
	tresult receiveText (const char* text) SMTG_OVERRIDE;

	/** For persistence */
	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;

	/** Will be called before any process call */
	tresult PLUGIN_API setupProcessing (ProcessSetup& newSetup) SMTG_OVERRIDE;

	/** Bus arrangement managing: in this example the 'again' will be mono for mono input/output and
	 * stereo for other arrangements. */
	tresult PLUGIN_API setBusArrangements (SpeakerArrangement* inputs, int32 numIns,
	                                       SpeakerArrangement* outputs,
	                                       int32 numOuts) SMTG_OVERRIDE;

	/** Asks if a given sample size is supported see \ref SymbolicSampleSizes. */
	tresult PLUGIN_API canProcessSampleSize (int32 symbolicSampleSize) SMTG_OVERRIDE;

	/** We want to receive message. */
	tresult PLUGIN_API notify (IMessage* message) SMTG_OVERRIDE;

//------------------------------------------------------------------------
protected:
	//==============================================================================
	template <typename SampleType>
	SampleType processAudio (SampleType** input, SampleType** output, int32 numChannels,
	                         int32 sampleFrames, float gain);

	template <typename SampleType>
	SampleType processVuPPM (SampleType** input, int32 numChannels, int32 sampleFrames);

	// our model values
	float fGain {1.f};
	float fGainReduction {0.f};
	float fVuPPMOld {0.f};

	int32 currentProcessMode {-1}; // -1 means not initialized

	bool bHalfGain {false};
	bool bBypass {false};
};

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
