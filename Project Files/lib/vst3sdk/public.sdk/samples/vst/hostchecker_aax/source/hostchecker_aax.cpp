//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/hostchecker_aax.cpp
// Created by  : Steinberg, 04/2019
// Description : HostChecker AAX Example for VST SDK 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "public.sdk/source/vst/aaxwrapper/aaxwrapper_description.h"

#include "cids.h"
#include "hostcheckercontroller.h"
#include "pluginterfaces/base/futils.h"

//------------------------------------------------------------------------
#if 0 // for additional outputs
AAX_Aux_Desc effAux_stereo[] = 
{
	// name, channel count
	{ "AGain AUX2",  2 }, 
	{ nullptr }
};
#endif

//------------------------------------------------------------------------
#if 1 // MIDI inputs for instruments or Fx with MIDI input
AAX_MIDI_Desc effMIDI[] = 
{
	// port name, channel mask
	{ "HostChecker MIDI", 0xffff }, 
	{ nullptr }
};
#endif

//------------------------------------------------------------------------
// Input/Output meters
/*
AAX_Meter_Desc effMeters[] = 
{
	// not used { "Input", CCONST ('A', 'G', 'I', 'n'),  0 / *AAX_eMeterOrientation_Default* /, 0 / *AAX_eMeterType_Input* / },
	{ "Output", kVuPPMId, 0 / *AAX_eMeterOrientation_Default* /, 1 / *AAX_eMeterType_Output* / },
	{ nullptr }
};
*/

//------------------------------------------------------------------------
AAX_Plugin_Desc effPlugins[] = {
	// effect-ID, name,	
	// Native ID, AudioSuite ID,
	// InChannels, OutChannels, InSideChain channels,
	// MIDI, Aux,
	// Meters
	// Latency
	// note: IDs must be unique across plugins

	// Mono variant
	{"com.steinberg.hostchecker.mono",
	 "HostChecker", 
	 CCONST ('H', 'C', 'h', '1'),
	 CCONST ('H', 'C', 'H', '1'),
	 1, /*mInputChannels*/
	 1, /*mOutputChannels*/
	 1, /*mSideChainInputChannels*/
	 effMIDI, /*effMIDI*/
	 nullptr, /*effAux*/
	 nullptr, /*effMeters*/
	 0 /*Latency*/ 
	},

	// Stereo variant
	{"com.steinberg.hostchecker.stereo",
	 "HostChecker", 
	 CCONST ('H', 'C', 'h', '2'),
	 CCONST ('H', 'C', 'H', '2'), 
	 2, /*mInputChannels*/
	 2, /*mOutputChannels*/
	 2, /*mSideChainInputChannels*/
	 effMIDI, /*effMIDI*/
	 nullptr, /*effAux*/
	 nullptr, /*effMeters*/
	 0 /*Latency*/
	},

	{nullptr}
};

//------------------------------------------------------------------------
AAX_Effect_Desc effDesc = {
	"Steinberg",	// manufacturer
	"HostChecker",		// product
	CCONST ('S', 'M', 'T', 'G'), // manufacturer ID
	CCONST ('H', 'C', 'H', 'E'), // product ID
	PlugVST3Category, // VST category (define cids.h)
	{0},			// VST3 class ID (set later)
	1,				// version
	nullptr,		// no pagetable file "hostchecker.xml",
	effPlugins,
};

//------------------------------------------------------------------------
// this drag's in all the craft from the AAX library
int* forceLinkAAXWrapper = &AAXWrapper_linkAnchor;

//------------------------------------------------------------------------
AAX_Effect_Desc* AAXWrapper_GetDescription ()
{
	// cannot initialize in global descriptor, and it might be link order dependent
	memcpy (effDesc.mVST3PluginID, (const char*)Steinberg::Vst::HostCheckerProcessorUID,
	        sizeof (effDesc.mVST3PluginID));
	return &effDesc;
}
