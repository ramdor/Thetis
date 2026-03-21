//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/utf16name/source/utf16nameprocessor.h
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

#pragma once

#include "public.sdk/source/vst/vstaudioeffect.h"

namespace Steinberg {

//------------------------------------------------------------------------
//  UTF16NameProcessor
//------------------------------------------------------------------------
class UTF16NameProcessor : public Vst::AudioEffect
{
public:
	UTF16NameProcessor ();
	~UTF16NameProcessor () SMTG_OVERRIDE;

	// Create function
	static FUnknown* createInstance (void* /*context*/)
	{
		return (Vst::IAudioProcessor*)new UTF16NameProcessor;
	}

	//--- ---------------------------------------------------------------------
	// AudioEffect overrides:
	//--- ---------------------------------------------------------------------
	/** Called at first after constructor */
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

	/** Called at the end before destructor */
	tresult PLUGIN_API terminate () SMTG_OVERRIDE;

	/** Switch the Plug-in on/off */
	tresult PLUGIN_API setActive (TBool state) SMTG_OVERRIDE;

	/** Will be called before any process call */
	tresult PLUGIN_API setupProcessing (Vst::ProcessSetup& newSetup) SMTG_OVERRIDE;

	/** Asks if a given sample size is supported see SymbolicSampleSizes. */
	tresult PLUGIN_API canProcessSampleSize (int32 symbolicSampleSize) SMTG_OVERRIDE;

	/** Here we go...the process call */
	tresult PLUGIN_API process (Vst::ProcessData& data) SMTG_OVERRIDE;

	/** For persistence */
	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;

//------------------------------------------------------------------------
protected:
};

//------------------------------------------------------------------------
} // namespace Steinberg
