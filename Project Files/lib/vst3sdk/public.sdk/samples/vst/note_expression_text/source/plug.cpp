//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_text/source/plug.cpp
// Created by  : Steinberg, 04/2005
// Description : Plug Example for VST SDK 3.0
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

#include "public.sdk/source/vst/vstaudioprocessoralgo.h"

#include "pluginterfaces/base/ibstream.h"
#include "pluginterfaces/vst/ivstevents.h"
#include "pluginterfaces/vst/ivstparameterchanges.h"

#include "base/source/fstreamer.h"
#include "base/source/fstring.h"

#include <cstdio>

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
// Plug Implementation
//------------------------------------------------------------------------
Plug::Plug () : bBypass (false)
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
	addEventInput (STR16 ("Event In"), 1);
	addAudioOutput (STR16 ("Stereo Out"), SpeakerArr::kStereo);

	return kResultOk;
}

//------------------------------------------------------------------------
tresult Plug::sendTextMessage (const char16* text)
{
	auto message = owned (allocateMessage ());
	if (!message)
		return kResultFalse;
	message->setMessageID ("TextMessage");
	message->getAttributes ()->setString ("Text", text);
	return sendMessage (message);
}

//------------------------------------------------------------------------
tresult PLUGIN_API Plug::process (ProcessData& data)
{
	//---1) Read inputs parameter changes-----------
	if (IParameterChanges* paramChanges = data.inputParameterChanges)
	{
		int32 numParamsChanged = paramChanges->getParameterCount ();
		// for each parameter which are some changes in this audio block:
		for (int32 i = 0; i < numParamsChanged; i++)
		{
			if (IParamValueQueue* paramQueue = paramChanges->getParameterData (i))
			{
				int32 offsetSamples;
				double value;
				int32 numPoints = paramQueue->getPointCount ();
				switch (paramQueue->getParameterId ())
				{
					case kBypassId:
						if (paramQueue->getPoint (numPoints - 1, offsetSamples, value) ==
						    kResultTrue)
						{
							bBypass = (value > 0.5f);
						}
						break;
				}
			}
		}
	}

	//---2) Read input events-------------
	if (IEventList* eventList = data.inputEvents)
	{
		int32 numEvent = eventList->getEventCount ();
		for (int32 i = 0; i < numEvent; i++)
		{
			Event event {};
			if (eventList->getEvent (i, event) == kResultOk)
			{
				switch (event.type)
				{
					//--- -------------------
					case Event::kNoteOnEvent:
					{
						mLastNoteOnPitch = event.noteOn.pitch;
						mLastNoteOnId = event.noteOn.noteId;
						/*String str;
						str.printf (STR("noteON %d"), event.noteOff.noteId);
						sendTextMessage (str);*/
					}
					break;

					//--- -------------------
					case Event::kNoteOffEvent:
					{
					    /*	String str;
							str.printf (STR("noteOff %d"), event.noteOff.noteId);
							sendTextMessage (str);
						*/}
					    break;

						//--- -------------------
					    case Event::kNoteExpressionTextEvent:
						    // noteOff reset the reduction
						    if (event.noteExpressionText.typeId == kTextTypeID)
						    {
							    // if (mLastNoteOnId == event.noteExpressionText.noteId)
							    {
								    String str (STR ("Text: "));
								    str += event.noteExpressionText.text;
								    String tmp1;
								    tmp1.printInt64 (mLastNoteOnId);
								    String tmp2;
								    tmp2.printInt64 (event.noteExpressionText.noteId);
								    str += STR (" - id:");
								    str += tmp2;
								    str += STR (" - noteOn id:");
								    str += tmp1;
								    sendTextMessage (str);
							    }
						    }
						    else if (event.noteExpressionText.typeId == kPhonemeTypeID)
						    {
							    // if (mLastNoteOnId == event.noteExpressionText.noteId)
							    {
								    String str (STR ("Phoneme: "));
								    str += event.noteExpressionText.text;
								    String tmp1;
								    tmp1.printInt64 (mLastNoteOnId);
								    String tmp2;
								    tmp2.printInt64 (event.noteExpressionText.noteId);
								    str += STR (" - id:");
								    str += tmp2;
								    str += STR (" - noteOn id:");
								    str += tmp1;
							    }
						    }
						    break;
				}
			}
		}
	}

	//--- ----------------------------------
	//---3) Process Audio---------------------
	//--- ----------------------------------

	if (data.numOutputs == 0)
	{
		// nothing to do
		return kResultOk;
	}

	// no output
	float** out = data.outputs[0].channelBuffers32;
	for (int32 i = 0; i < data.outputs[0].numChannels; i++)
	{
		memset (out[i], 0, data.numSamples * sizeof (float));
	}
	data.outputs[0].silenceFlags = getChannelMask (data.outputs[0].numChannels);

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API Plug::setState (IBStream* state)
{
	// called when we load a preset, the model has to be reloaded

	if (!state)
		return kResultFalse;

	IBStreamer streamer (state, kLittleEndian);

	int32 savedBypass = 0;
	if (streamer.readInt32 (savedBypass) == false)
		return kResultFalse;
	bBypass = savedBypass > 0;

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

	return kResultOk;
}
}
} // namespaces
