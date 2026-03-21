//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/legacymidiccout/source/plug.cpp
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

#include "plug.h"
#include "plugcids.h" // for class ids
#include "plugparamids.h"

#include "public.sdk/source/vst/vsteventshelper.h"
#include "pluginterfaces/base/futils.h"
#include "pluginterfaces/base/ibstream.h"
#include "pluginterfaces/vst/ivstevents.h"
#include "pluginterfaces/vst/ivstmidicontrollers.h"
#include "pluginterfaces/vst/ivstparameterchanges.h"

#include "base/source/fstreamer.h"
#include <cstdio>

namespace Steinberg {
namespace Vst {
namespace LegacyMIDICCOut {

//------------------------------------------------------------------------
// Plug Implementation
//------------------------------------------------------------------------
Plug::Plug ()
{
	// register its editor class (the same than used in plugentry.cpp)
	setControllerClass (PlugControllerUID);
}

//------------------------------------------------------------------------
tresult PLUGIN_API Plug::initialize (FUnknown* context)
{
	//---always initialize the parent-------
	tresult result = AudioEffect::initialize (context);
	// if everything Ok, continue
	if (result != kResultOk)
	{
		return result;
	}

	//---create Audio In/Out busses------
	// we want a stereo Input and a Stereo Output
	addAudioInput (STR16 ("Stereo In"), SpeakerArr::kStereo);
	addAudioOutput (STR16 ("Stereo Out"), SpeakerArr::kStereo);

	// one output for event and only 1 channel wanted
	addEventOutput (STR16 ("Event Out"), kMaxMIDIChannelSupported);

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API Plug::process (ProcessData& data)
{
	//---1) Read inputs parameter changes-----------
	if (IParameterChanges* paramChanges = data.inputParameterChanges)
	{
		IEventList* eventList = data.outputEvents;

		int32 numParamsChanged = paramChanges->getParameterCount ();
		// for each parameter which are some changes in this audio block:
		for (int32 i = 0; i < numParamsChanged; i++)
		{
			IParamValueQueue* paramQueue = paramChanges->getParameterData (i);
			if (!paramQueue)
				continue;

			int32 offsetSamples;
			double value;
			int32 numPoints = paramQueue->getPointCount ();
			ParamID id = paramQueue->getParameterId ();
			switch (id)
			{
				//--- ----------------------------------------------
				case kBypassId:
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						bBypass = (value > 0.5f);
					}
					break;
				//--- ----------------------------------------------
				case kChannelId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						mChannel = FromNormalized<ParamValue> (value, kMaxMIDIChannelSupported - 1);
					}
				}
				break;
				//--- ----------------------------------------------
				case kControllerNumId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						mControllerNum = FromNormalized<ParamValue> (value, 127);
					}
				}
				break;
				//--- ----------------------------------------------
				case kControllerId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = Helpers::getMIDICCOutValue (value);
						if (newValue != mLastController)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, mControllerNum, mChannel,
								                                   newValue);
								event.sampleOffset = offsetSamples;
								event.flags = Event::kIsLive;
								eventList->addEvent (event);
							}
							mLastController = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kAftertouchId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = Helpers::getMIDICCOutValue (value);
						if (newValue != mLastAftertouch)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kAfterTouch, mChannel,
								                                   newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastAftertouch = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kProgramChangeId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = Helpers::getMIDICCOutValue (value);
						if (newValue != mLastProgramChange)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kCtrlProgramChange,
								                                   mChannel, newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastProgramChange = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kPolyPressureNoteId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						mPolyPressureKey = Helpers::boundTo<uint8> (
						    0, 127, FromNormalized<ParamValue> (value, 127));
					}
				}
				break;
				//--- ----------------------------------------------
				case kPolyPressureId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = Helpers::getMIDICCOutValue (value);
						if (newValue != mLastPolyPressure)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (
								    event, kCtrlPolyPressure, mChannel, mPolyPressureKey, newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastPolyPressure = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kPitchBendId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = Helpers::getMIDI14BitValue (value);
						if (newValue != mLastPitchBend)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kPitchBend, mChannel);
								Helpers::setPitchBendValue (event.midiCCOut, value);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastPitchBend = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kCtrlQuarterFrameId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = Helpers::getMIDICCOutValue (value);
						if (newValue != mLastCtrlQuarterFrame)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kCtrlQuarterFrame,
								                                   mChannel, newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastCtrlQuarterFrame = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kSystemSongSelectId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = Helpers::getMIDICCOutValue (value);
						if (newValue != mLastSystemSongSelect)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kSystemSongSelect,
								                                   mChannel, newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastSystemSongSelect = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kSystemSongPointerId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = Helpers::getMIDI14BitValue (value);
						if (newValue != mLastSystemSongPointer)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								///< use LegacyMIDICCOutEvent.value for LSB and
								/// LegacyMIDICCOutEvent.value2 for MSB
								Helpers::initLegacyMIDICCOutEvent (event, kSystemSongPointer,
								                                   mChannel);
								Helpers::setPitchBendValue (event.midiCCOut, value);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastSystemSongPointer = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kSystemCableSelectId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = Helpers::getMIDICCOutValue (value);
						if (newValue != mLastSystemCableSelect)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kSystemCableSelect,
								                                   mChannel, newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastSystemCableSelect = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kSystemTuneRequestId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = value >= 0.5 ? 127 : 0;
						if (newValue != mLastSystemTuneRequest)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kSystemTuneRequest,
								                                   mChannel, newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastSystemTuneRequest = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kSystemMidiClockStartId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = value >= 0.5 ? 127 : 0;
						if (newValue != mLastSystemMidiClockStart)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kSystemMidiClockStart,
								                                   mChannel, newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastSystemMidiClockStart = newValue;
						}
					}
				}
				break;

				//--- ----------------------------------------------
				case kSystemMidiClockContinueId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = value >= 0.5 ? 127 : 0;
						if (newValue != mLastSystemMidiClockContinue)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kSystemMidiClockContinue,
								                                   mChannel, newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastSystemMidiClockContinue = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kSystemMidiClockStopId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = value >= 0.5 ? 127 : 0;
						if (newValue != mLastSystemMidiClockStop)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kSystemMidiClockStop,
								                                   mChannel, newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastSystemMidiClockStop = newValue;
						}
					}
				}
				break;
				//--- ----------------------------------------------
				case kSystemActiveSensingId:
				{
					if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) == kResultTrue)
					{
						auto newValue = value >= 0.5 ? 127 : 0;
						if (newValue != mLastSystemActiveSensing)
						{
							if (eventList && !bBypass)
							{
								Event event {};
								Helpers::initLegacyMIDICCOutEvent (event, kSystemActiveSensing,
								                                   mChannel, newValue);
								event.sampleOffset = offsetSamples;
								eventList->addEvent (event);
							}
							mLastSystemActiveSensing = newValue;
						}
					}
				}
				break;
			}
		}
	}

	//--- ----------------------------------
	//---3) Process Audio---------------------
	//--- ----------------------------------
	if (data.numInputs == 0 || data.numOutputs == 0)
	{
		// nothing to do
		return kResultOk;
	}

	// (simplification) we suppose in this example that we have the same input channel count than
	// the output
	int32 numChannels = data.inputs[0].numChannels;

	//---get audio buffers----------------
	float** in = data.inputs[0].channelBuffers32;
	float** out = data.outputs[0].channelBuffers32;

	for (int32 i = 0; i < numChannels; i++)
	{
		// do not need to be copied if the buffers are the same
		if (in[i] != out[i])
		{
			memcpy (out[i], in[i], data.numSamples * sizeof (float));
		}
	}
	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API Plug::setState (IBStream* state)
{
	// called when we load a preset, the model has to be reloaded

	if (!state)
		return kResultFalse;

	IBStreamer streamer (state, kLittleEndian);

	// read the bypass
	int32 savedBypass = 0;
	if (streamer.readInt32 (savedBypass) == false)
		return kResultFalse;

	bBypass = savedBypass > 0;

	streamer.readInt8u (mChannel);
	streamer.readInt8u (mControllerNum);
	streamer.readInt8u (mPolyPressureKey);

	streamer.readInt8 (mLastController);
	streamer.readInt8 (mLastProgramChange);
	streamer.readInt8 (mLastAftertouch);
	streamer.readInt8 (mLastPolyPressure);
	streamer.readInt16 (mLastPitchBend);

	streamer.readInt8 (mLastCtrlQuarterFrame);
	streamer.readInt8 (mLastSystemSongSelect);
	streamer.readInt16 (mLastSystemSongPointer);
	streamer.readInt8 (mLastSystemCableSelect);

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API Plug::getState (IBStream* state)
{
	// here we need to save the model

	if (!state)
		return kResultFalse;

	IBStreamer streamer (state, kLittleEndian);

	streamer.writeInt32 (bBypass ? 1 : 0);

	streamer.writeInt8u (mChannel);
	streamer.writeInt8u (mControllerNum);
	streamer.writeInt8u (mPolyPressureKey);

	streamer.writeInt8 (mLastController);
	streamer.writeInt8 (mLastProgramChange);
	streamer.writeInt8 (mLastAftertouch);
	streamer.writeInt8 (mLastPolyPressure);
	streamer.writeInt16 (mLastPitchBend);

	streamer.writeInt8 (mLastCtrlQuarterFrame);
	streamer.writeInt8 (mLastSystemSongSelect);
	streamer.writeInt16 (mLastSystemSongPointer);
	streamer.writeInt8 (mLastSystemCableSelect);

	return kResultOk;
}

//------------------------------------------------------------------------
} // namespace LegacyMIDICCOut
} // namespace Vst
} // namespace Steinberg
