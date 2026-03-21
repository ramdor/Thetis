//-----------------------------------------------------------------------------
// Flags       : clang-format auto
// Project     : VST SDK
//
// Category    : AudioHost
// Filename    : public.sdk/samples/vst-hosting/audiohost/source/media/miditovst.h
// Created by  : Steinberg 09.2016
// Description : Audio Host Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "public.sdk/source/vst/utility/optional.h"
#include "pluginterfaces/vst/ivstevents.h"
#include "pluginterfaces/vst/ivstmidicontrollers.h"
#include <functional>
#include <limits>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {

const uint8_t kNoteOff = 0x80; ///< note, off velocity
const uint8_t kNoteOn = 0x90; ///< note, on velocity
const uint8_t kPolyPressure = 0xA0; ///< note, pressure
const uint8_t kController = 0xB0; ///< controller, value
const uint8_t kProgramChangeStatus = 0xC0; ///< program change
const uint8_t kAfterTouchStatus = 0xD0; ///< channel pressure
const uint8_t kPitchBendStatus = 0xE0; ///< lsb, msb
static const uint32 kDataMask = 0x7F;

const float kMidiScaler = 1.f / 127.f;

using MidiData = uint8_t;

float toNormalized (const MidiData& data)
{
	return (float)data * kMidiScaler;
}

using OptionalEvent = VST3::Optional<Event>;

using ParameterChange = std::pair<ParamID, ParamValue>;
using OptionParamChange = VST3::Optional<ParameterChange>;

OptionalEvent midiToEvent (MidiData status, MidiData channel, MidiData midiData0,
                           MidiData midiData1)
{
	Event new_event = {};
	if (status == kNoteOn || status == kNoteOff)
	{
		if (status == kNoteOff) // note off
		{
			new_event.noteOff.noteId = -1;
			new_event.type = Event::kNoteOffEvent;
			new_event.noteOff.channel = channel;
			new_event.noteOff.pitch = midiData0;
			new_event.noteOff.velocity = toNormalized (midiData1);
			return new_event;
		}
		if (status == kNoteOn) // note on
		{
			new_event.noteOn.noteId = -1;
			new_event.type = Event::kNoteOnEvent;
			new_event.noteOn.channel = channel;
			new_event.noteOn.pitch = midiData0;
			new_event.noteOn.velocity = toNormalized (midiData1);
			return new_event;
		}
	}
	//--- -----------------------------
	else if (status == kPolyPressure)
	{
		new_event.type = Vst::Event::kPolyPressureEvent;
		new_event.polyPressure.channel = channel;
		new_event.polyPressure.pitch = midiData0;
		new_event.polyPressure.pressure = toNormalized (midiData1);
		return new_event;
	}

	return {};
}

//------------------------------------------------------------------------
using ToParameterIdFunc = std::function<ParamID (int32, MidiData)>;
OptionParamChange midiToParameter (MidiData status, MidiData channel, MidiData midiData1,
                                   MidiData midiData2, const ToParameterIdFunc& toParamID)
{
	if (!toParamID)
		return {};

	ParameterChange paramChange;
	if (status == kController) // controller
	{
		paramChange.first = toParamID (channel, midiData1);
		if (paramChange.first != kNoParamId)
		{
			paramChange.second = (double)midiData2 * kMidiScaler;
			return paramChange;
		}
	}
	else if (status == kPitchBendStatus)
	{
		paramChange.first = toParamID (channel, Vst::kPitchBend);
		if (paramChange.first != kNoParamId)
		{
			const double kPitchWheelScaler = 1. / (double)0x3FFF;

			const int32 ctrl = (midiData1 & kDataMask) | (midiData2 & kDataMask) << 7;
			paramChange.second = kPitchWheelScaler * (double)ctrl;
			return paramChange;
		};
	}
	else if (status == kAfterTouchStatus)
	{
		paramChange.first = toParamID (channel, Vst::kAfterTouch);
		if (paramChange.first != kNoParamId)
		{
			paramChange.second = (ParamValue) (midiData1 & kDataMask) * kMidiScaler;
			return paramChange;
		};
	}
	else if (status == kProgramChangeStatus)
	{
		// TODO
	}

	return {};
}
//------------------------------------------------------------------------
} // Vst
} // Steinberg
