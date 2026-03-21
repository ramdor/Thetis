//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/legacymidiccout/source/plug.h
// Created by  : Steinberg, 11/2018
// Description : Plug-in Example for VST SDK 3.x using Legacy MIDI CC
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
namespace LegacyMIDICCOut {

//------------------------------------------------------------------------
// Plug: directly derived from the helper class AudioEffect
//------------------------------------------------------------------------
class Plug : public AudioEffect
{
public:
	Plug ();

	//--- ---------------------------------------------------------------------
	// create function required for plug-in factory,
	// it will be called to create new instances of this plug-in
	//--- ---------------------------------------------------------------------
	static FUnknown* createInstance (void* /*context*/) { return (IAudioProcessor*)new Plug; }

	//--- ---------------------------------------------------------------------
	// AudioEffect overrides:
	//--- ---------------------------------------------------------------------
	/** Called at first after constructor */
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

	/** Here we go...the process call */
	tresult PLUGIN_API process (ProcessData& data) SMTG_OVERRIDE;

	/** For persistence */
	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;
//------------------------------------------------------------------------
protected:
	int8 mLastController {0};
	int8 mLastProgramChange {0};
	int8 mLastAftertouch {0};
	int8 mLastPolyPressure {0};
	int16 mLastPitchBend {0};

	int8 mLastCtrlQuarterFrame {0};
	int8 mLastSystemSongSelect {0};
	int16 mLastSystemSongPointer {0};
	int8 mLastSystemCableSelect {0};
	int8 mLastSystemTuneRequest {0};
	int8 mLastSystemMidiClockStart {0};
	int8 mLastSystemMidiClockContinue {0};
	int8 mLastSystemMidiClockStop {0};
	int8 mLastSystemActiveSensing {0};

	uint8 mChannel {0};
	uint8 mControllerNum {11};
	uint8 mPolyPressureKey {36};

	bool bBypass {false};
};
//------------------------------------------------------------------------
} // namespace LegacyMIDICCOut
} // namespace Vst
} // namespace Steinberg
