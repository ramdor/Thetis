//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/syncdelay/source/syncdelayprocessor.cpp
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

#include "syncdelayprocessor.h"
#include "sync.h"
#include "syncdelayids.h"
#include "base/source/fstreamer.h"
#include "pluginterfaces/base/futils.h"
#include "pluginterfaces/base/ibstream.h"
#include "pluginterfaces/base/ustring.h"
#include "pluginterfaces/vst/ivstparameterchanges.h"
#include "pluginterfaces/vst/ivstprocesscontext.h"
#include <algorithm>
#include <cmath>
#include <cstdlib>

namespace Steinberg {
namespace Vst {

static constexpr auto maxDelaySeconds = 5;

//-----------------------------------------------------------------------------
SyncDelayProcessor::SyncDelayProcessor ()
{
	setControllerClass (SyncDelayControllerUID);
	processContextRequirements.needTempo ();
}

//-----------------------------------------------------------------------------
tresult PLUGIN_API SyncDelayProcessor::initialize (FUnknown* context)
{
	auto result = AudioEffect::initialize (context);
	if (result == kResultTrue)
	{
		addAudioInput (STR16 ("AudioInput"), SpeakerArr::kStereo);
		addAudioOutput (STR16 ("AudioOutput"), SpeakerArr::kStereo);
		mNumChannels = 2;
	}
	return result;
}

//-----------------------------------------------------------------------------
tresult PLUGIN_API SyncDelayProcessor::setBusArrangements (SpeakerArrangement* inputs, int32 numIns,
                                                           SpeakerArrangement* outputs,
                                                           int32 numOuts)
{
	// we only support one in and output bus and these busses must have the same number of channels
	if (numIns == 1 && numOuts == 1 && inputs[0] == outputs[0])
	{
		tresult res = AudioEffect::setBusArrangements (inputs, numIns, outputs, numOuts);
		if (res == kResultOk)
			mNumChannels = SpeakerArr::getChannelCount (outputs[0]);
		return res;
	}
	return kResultFalse;
}

//-----------------------------------------------------------------------------
tresult PLUGIN_API SyncDelayProcessor::setActive (TBool state)
{
	if (mBuffer)
	{
		for (int32 channel = 0; channel < mNumChannels; channel++)
		{
			std::free (mBuffer[channel]);
		}
		std::free (mBuffer);
		mBuffer = nullptr;
	}

	if (state)
	{
		mBuffer = static_cast<float**> (std::malloc (mNumChannels * sizeof (float*)));
		if (mBuffer)
		{
			// use a maximum of 5 seconds delay time
			mBufferSizeInSamples =
			    static_cast<uint32> (std::ceil (processSetup.sampleRate) * maxDelaySeconds);
			auto size = static_cast<size_t> (mBufferSizeInSamples * sizeof (float));
			for (int32 channel = 0; channel < mNumChannels; channel++)
			{
				mBuffer[channel] = static_cast<float*> (std::malloc (size));
			}
			resetDelay ();
		}
		mDelayIndex = FromNormalized<ParamValue> (0, static_cast<int32> (Synced.size () - 1));
		mBypassProcessor.setup (*this, processSetup, 0);
	}
	else
	{
		mBypassProcessor.reset ();
	}
	return AudioEffect::setActive (state);
}

//------------------------------------------------------------------------
void SyncDelayProcessor::calculateDelay ()
{
	mDelayInSamples =
	    static_cast<uint32> ((60. / mTempo) * Synced[mDelayIndex].value * processSetup.sampleRate);
	mDelayInSamples = Bound (uint32 (1), mBufferSizeInSamples, mDelayInSamples);
}

//------------------------------------------------------------------------
void SyncDelayProcessor::doParameterChanges (IParameterChanges& changes)
{
	auto numParamsChanged = changes.getParameterCount ();
	for (int32 index = 0; index < numParamsChanged; index++)
	{
		if (auto queue = changes.getParameterData (index))
		{
			ParamValue value;
			int32 sampleOffset;
			auto numPoints = queue->getPointCount ();
			switch (queue->getParameterId ())
			{
				case kDelayId:
				{
					if (queue->getPoint (numPoints - 1, sampleOffset, value) == kResultTrue)
					{
						mDelayIndex = FromNormalized<ParamValue> (
						    value, static_cast<int32> (Synced.size () - 1));
					}
					break;
				}
				case kBypassId:
				{
					if (queue->getPoint (numPoints - 1, sampleOffset, value) == kResultTrue)
					{
						mBypassProcessor.setActive (value > 0.5);
					}
					break;
				}
			}
		}
	}
}

//-----------------------------------------------------------------------------
bool SyncDelayProcessor::resetDelay ()
{
	if (!mBuffer)
		return false;

	auto size = static_cast<size_t> (mBufferSizeInSamples * sizeof (float));
	for (int32 channel = 0; channel < mNumChannels; channel++)
	{
		if (mBuffer[channel])
			memset (mBuffer[channel], 0, size);
	}
	mBufferPos = 0;

	return true;
}

//-----------------------------------------------------------------------------
tresult PLUGIN_API SyncDelayProcessor::setProcessing (TBool state)
{
	if (state)
	{
		resetDelay ();
	}
	return kResultOk;
}

//-----------------------------------------------------------------------------
tresult PLUGIN_API SyncDelayProcessor::process (ProcessData& data)
{
	if (data.processContext && data.processContext->state & ProcessContext::kTempoValid)
	{
		if (data.processContext->tempo != mTempo)
		{
			mTempo = data.processContext->tempo;
		}
	}
	if (data.inputParameterChanges)
	{
		doParameterChanges (*data.inputParameterChanges);
	}

	if (mBypassProcessor.isActive ())
	{
		mBypassProcessor.process (data);
	}
	else if (data.numSamples > 0)
	{
		calculateDelay ();

		for (int32 channel = 0; channel < mNumChannels; channel++)
		{
			auto inputChannel = data.inputs[0].channelBuffers32[channel];
			auto outputChannel = data.outputs[0].channelBuffers32[channel];

			auto tempBufferPos = mBufferPos;
			for (int32 sampleIndex = 0; sampleIndex < data.numSamples; sampleIndex++)
			{
				auto sampleValue = inputChannel[sampleIndex];
				outputChannel[sampleIndex] = mBuffer[channel][tempBufferPos];
				mBuffer[channel][tempBufferPos] = sampleValue;
				tempBufferPos++;
				if (tempBufferPos >= mDelayInSamples)
					tempBufferPos = 0;
			}
		}
		mBufferPos += data.numSamples;
		while (mDelayInSamples && mBufferPos >= mDelayInSamples)
			mBufferPos -= mDelayInSamples;
	}
	return kResultTrue;
}

//------------------------------------------------------------------------
tresult PLUGIN_API SyncDelayProcessor::setState (IBStream* state)
{
	if (!state)
		return kResultFalse;

	IBStreamer streamer (state, kLittleEndian);
	uint32 savedDelayIndex = 0;
	if (streamer.readInt32u (savedDelayIndex) == false)
		return kResultFalse;
	int32 savedBypassState = 0;
	if (streamer.readInt32 (savedBypassState) == false)
		return kResultFalse;

	mDelayIndex = savedDelayIndex;
	mBypassProcessor.setActive (savedBypassState > 0);

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API SyncDelayProcessor::getState (IBStream* state)
{
	IBStreamer streamer (state, kLittleEndian);

	streamer.writeInt32u (mDelayIndex);
	streamer.writeInt32 (mBypassProcessor.isActive () ? 1 : 0);

	return kResultOk;
}

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
