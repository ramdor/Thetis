//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/prefetchablesupport/source/plug.h
// Created by  : Steinberg, 04/2015
// Description : Plug-in Example for VST SDK 3.x using IPrefetchableSupport
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
#include "pluginterfaces/vst/ivstprefetchablesupport.h"

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
// Plug: directly derived from the helper class AudioEffect
//------------------------------------------------------------------------
class Plug : public AudioEffect, public Vst::IPrefetchableSupport
{
public:
	Plug ();

//------------------------------------------------------------------------
// create function required for plug-in factory,
// it will be called to create new instances of this plug-in
//------------------------------------------------------------------------
	static FUnknown* createInstance (void* /*context*/)
	{
		return (IAudioProcessor*)new Plug;
	}

//------------------------------------------------------------------------
// AudioEffect overrides:
//------------------------------------------------------------------------
	/** Called at first after constructor */
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
		
	/** Here we go...the process call */
	tresult PLUGIN_API process (ProcessData& data) SMTG_OVERRIDE;

	/** For persistence */
	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;

	//---------------------------------------------------------------------
	// IPrefetchableSupport
	//---------------------------------------------------------------------
	/** */
	tresult PLUGIN_API getPrefetchableSupport (PrefetchableSupport& prefetchable /*out*/) SMTG_OVERRIDE;

	//---Interface---------
	OBJ_METHODS (Plug, AudioEffect)
	DEFINE_INTERFACES
		DEF_INTERFACE (Vst::IPrefetchableSupport)
	END_DEFINE_INTERFACES (AudioEffect)
	REFCOUNT_METHODS (AudioEffect)

//------------------------------------------------------------------------
protected:
	int32 mPrefetchableMode;
	bool bBypass;
};

} // namespace Vst
} // namespace Steinberg
