//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/legacymidiccout/source/plugcontroller.cpp
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

#include "plugcontroller.h"
#include "plugparamids.h"

#include "public.sdk/source/vst/vsteventshelper.h"
#include "base/source/fstreamer.h"
#include "pluginterfaces/base/futils.h"
#include "pluginterfaces/base/ibstream.h"
#include "pluginterfaces/vst/ivstcomponent.h"

namespace Steinberg {
namespace Vst {
namespace LegacyMIDICCOut {
//------------------------------------------------------------------------
// PlugController Implementation
//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::initialize (FUnknown* context)
{
	tresult result = EditControllerEx1::initialize (context);
	if (result != kResultOk)
	{
		return result;
	}

	//---Create Parameters------------

	//---Bypass parameter---
	int32 stepCount = 1;
	ParamValue defaultVal = 0;
	int32 flags = ParameterInfo::kCanAutomate | ParameterInfo::kIsBypass;
	int32 tag = kBypassId;
	parameters.addParameter (STR ("Bypass"), nullptr, stepCount, defaultVal, flags, tag);

	//---Controller parameter - only for demo ---
	parameters.addParameter (new RangeParameter (
	    STR16 ("MIDI Channel"), kChannelId, nullptr, 1, kMaxMIDIChannelSupported, 1,
	    kMaxMIDIChannelSupported - 1, ParameterInfo::kNoFlags));

	parameters.addParameter (new RangeParameter (STR16 ("Controller Num"), kControllerNumId,
	                                             nullptr, 0., 127., 0., 127,
	                                             ParameterInfo::kCanAutomate));
	parameters.addParameter (new RangeParameter (STR16 ("Controller"), kControllerId, nullptr, 0.,
	                                             127., 0., 127, ParameterInfo::kCanAutomate));

	parameters.addParameter (new RangeParameter (STR16 ("PitchBend"), kPitchBendId, nullptr,
	                                             -0x2000, 0x1FFF, 0, 0x3FFF,
	                                             ParameterInfo::kCanAutomate));

	parameters.addParameter (new RangeParameter (STR16 ("ProgramChange"), kProgramChangeId, nullptr,
	                                             1., 128., 1., 127, ParameterInfo::kCanAutomate));

	parameters.addParameter (new RangeParameter (STR16 ("PolyPressure Key"), kPolyPressureNoteId,
	                                             nullptr, 0., 127., 0., 127,
	                                             ParameterInfo::kCanAutomate));
	parameters.addParameter (new RangeParameter (STR16 ("PolyPressure"), kPolyPressureId, nullptr,
	                                             0., 127., 0., 127, ParameterInfo::kCanAutomate));

	parameters.addParameter (new RangeParameter (STR16 ("Aftertouch"), kAftertouchId, nullptr, 0.,
	                                             127., 0., 127, ParameterInfo::kCanAutomate));

	parameters.addParameter (new RangeParameter (STR16 ("CtrlQuarterFrame"), kCtrlQuarterFrameId,
	                                             nullptr, 0., 127., 0., 127,
	                                             ParameterInfo::kCanAutomate));
	parameters.addParameter (new RangeParameter (STR16 ("SystemSongSelect"), kSystemSongSelectId,
	                                             nullptr, 0., 127., 0., 127,
	                                             ParameterInfo::kCanAutomate));
	parameters.addParameter (new RangeParameter (STR16 ("SystemSongPointer"), kSystemSongPointerId,
	                                             nullptr, 0x0000, 0x3FFF, 0, 0x3FFF,
	                                             ParameterInfo::kCanAutomate));

	parameters.addParameter (new RangeParameter (STR16 ("SystemCableSelect"), kSystemCableSelectId,
	                                             nullptr, 0., 127., 0., 127,
	                                             ParameterInfo::kCanAutomate));
	parameters.addParameter (STR ("SystemTuneRequest"), nullptr, 1, 0, ParameterInfo::kCanAutomate,
	                         kSystemTuneRequestId);
	parameters.addParameter (STR ("SystemMidiClockStart"), nullptr, 1, 0,
	                         ParameterInfo::kCanAutomate, kSystemMidiClockStartId);
	parameters.addParameter (STR ("SystemMidiClockContinue"), nullptr, 1, 0,
	                         ParameterInfo::kCanAutomate, kSystemMidiClockContinueId);
	parameters.addParameter (STR ("SystemMidiClockStop"), nullptr, 1, 0,
	                         ParameterInfo::kCanAutomate, kSystemMidiClockStopId);
	parameters.addParameter (STR ("SystemActiveSensing"), nullptr, 1, 0,
	                         ParameterInfo::kCanAutomate, kSystemActiveSensingId);

	return result;
}

//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::setComponentState (IBStream* state)
{
	// we receive the current state of the component (processor part)
	// we read only the gain and bypass value...
	if (!state)
		return kResultFalse;

	IBStreamer streamer (state, kLittleEndian);

	// read the bypass
	int32 bypassState = 0;
	if (streamer.readInt32 (bypassState) == false)
		return kResultFalse;
	setParamNormalized (kBypassId, bypassState ? 1 : 0);

	uint8 ui8Val;
	if (streamer.readInt8u (ui8Val) == true)
		setParamNormalized (kChannelId, ToNormalized<ParamValue> (ui8Val, kMaxMIDIChannelSupported - 1));

	if (streamer.readInt8u (ui8Val) == true)
		setParamNormalized (kControllerNumId, Helpers::getMIDINormValue (ui8Val));

	if (streamer.readInt8u (ui8Val) == true)
		setParamNormalized (kPolyPressureNoteId, Helpers::getMIDINormValue (ui8Val));

	int8 i8Val;
	int16 i16Val;

	if (streamer.readInt8 (i8Val) == true)
		setParamNormalized (kControllerId, Helpers::getMIDINormValue (i8Val));
	if (streamer.readInt8 (i8Val) == true)
		setParamNormalized (kProgramChangeId, Helpers::getMIDINormValue (i8Val));
	if (streamer.readInt8 (i8Val) == true)
		setParamNormalized (kAftertouchId, Helpers::getMIDINormValue (i8Val));
	if (streamer.readInt8 (i8Val) == true)
		setParamNormalized (kPolyPressureId, Helpers::getMIDINormValue (i8Val));
	if (streamer.readInt16 (i16Val) == true)
		setParamNormalized (kPitchBendId, Helpers::getMIDI14BitNormValue (i16Val));

	if (streamer.readInt8 (i8Val) == true)
		setParamNormalized (kCtrlQuarterFrameId, Helpers::getMIDINormValue (i8Val));
	if (streamer.readInt8 (i8Val) == true)
		setParamNormalized (kSystemSongSelectId, Helpers::getMIDINormValue (i8Val));

	if (streamer.readInt16 (i16Val) == true)
		setParamNormalized (kSystemSongPointerId, Helpers::getMIDI14BitNormValue (i16Val));

	if (streamer.readInt8 (i8Val) == true)
		setParamNormalized (kSystemCableSelectId, Helpers::getMIDINormValue (i8Val));

	return kResultOk;
}

//------------------------------------------------------------------------
} // namespace LegacyMIDICCOut
} // namespace Vst
} // namespace Steinberg
