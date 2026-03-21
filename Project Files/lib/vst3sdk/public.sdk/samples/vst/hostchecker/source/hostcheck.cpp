//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/hostcheck.cpp
// Created by  : Steinberg, 04/2012
// Description :
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "hostcheck.h"
#include "logevents.h"
#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include "pluginterfaces/vst/ivstevents.h"
#include "pluginterfaces/vst/ivstnoteexpression.h"
#include "pluginterfaces/vst/ivstparameterchanges.h"

//------------------------------------------------------------------------
HostCheck::HostCheck ()
{
	mProcessSetupCheck.setEventLogger (&mEventLogger);
	mProcessContextCheck.setEventLogger (&mEventLogger);
	mEventListCheck.setEventLogger (&mEventLogger);
	mParamChangesCheck.setEventLogger (&mEventLogger);
	mParamChangesCheck.setParamIDs (&mParameterIds);
}

//------------------------------------------------------------------------
void HostCheck::addParameter (Steinberg::Vst::ParamID paramId)
{
	mParameterIds.insert (paramId);
	mParamChangesCheck.updateParameterIDs ();
}

//------------------------------------------------------------------------
void HostCheck::addLogEvent (Steinberg::int32 logId)
{
	mEventLogger.addLogEvent (logId);
}

//------------------------------------------------------------------------
bool HostCheck::validate (Steinberg::Vst::ProcessData& data, Steinberg::int32 minInputBufferCount,
                          Steinberg::int32 minOutputBufferCount)
{
	mProcessSetupCheck.check (data);
	mProcessContextCheck.check (data.processContext);
	mEventListCheck.check (data.inputEvents);
	mParamChangesCheck.checkParameterChanges (data.inputParameterChanges);

	checkAudioBuffers (data.inputs, data.numInputs, Steinberg::Vst::kInput, data.symbolicSampleSize,
	                   minInputBufferCount);
	checkAudioBuffers (data.outputs, data.numOutputs, Steinberg::Vst::kOutput,
	                   data.symbolicSampleSize, minOutputBufferCount);

	return mEventLogger.empty ();
}

//------------------------------------------------------------------------
void HostCheck::checkAudioBuffers (Steinberg::Vst::AudioBusBuffers* buffers,
                                   Steinberg::int32 numBuffers, Steinberg::Vst::BusDirection dir,
                                   Steinberg::int32 symbolicSampleSize,
                                   Steinberg::int32 minBufferCount)
{
	if (mComponent)
	{
		if (numBuffers > 0)
		{
			bool isValid = minBufferCount <= numBuffers;
			if (!isValid)
			{
				addLogEvent (kLogIdAudioBufNotMatchComponentBusCount);
			}
			// check only output, an instrument could have side chain input not activated
			if (dir == Steinberg::Vst::BusDirections::kOutput && minBufferCount == 0)
			{
				addLogEvent (kLogIdNoBusActivated);
			}
		}
	}

	if (numBuffers > 0)
	{
		if (!buffers)
		{
			addLogEvent (kLogIdNullPointerToAudioBusBuffer);
			return;
		}

		for (Steinberg::int32 bufferIdx = 0; bufferIdx < numBuffers; ++bufferIdx)
		{
			Steinberg::Vst::BusInfo busInfo = {};
			mComponent->getBusInfo (Steinberg::Vst::kAudio, dir, bufferIdx, busInfo);
			Steinberg::Vst::AudioBusBuffers& tmpBuffers = buffers[bufferIdx];
			if (tmpBuffers.numChannels != busInfo.channelCount)
			{
				addLogEvent (kLogIdInvalidAudioBufNumOfChannels);
			}

			if (symbolicSampleSize == Steinberg::Vst::kSample32)
			{
				for (Steinberg::int32 chIdx = 0; chIdx < tmpBuffers.numChannels; ++chIdx)
				{
					if (!tmpBuffers.channelBuffers32 || !tmpBuffers.channelBuffers32[chIdx])
					{
						if (busInfo.busType == Steinberg::Vst::kAux)
							addLogEvent (kLogIdNullPointerToAuxChannelBuf);
						else
							addLogEvent (kLogIdNullPointerToChannelBuf);
					}
				}
			}
			else
			{
				for (Steinberg::int32 chIdx = 0; chIdx < tmpBuffers.numChannels; ++chIdx)
				{
					if (!tmpBuffers.channelBuffers64 || !tmpBuffers.channelBuffers64[chIdx])
					{
						if (busInfo.busType == Steinberg::Vst::kAux)
							addLogEvent (kLogIdNullPointerToAuxChannelBuf);
						else
							addLogEvent (kLogIdNullPointerToChannelBuf);
					}
				}
			}
		}
	}
}

//------------------------------------------------------------------------
void HostCheck::setComponent (Steinberg::Vst::IComponent* component)
{
	mEventListCheck.setComponent (component);
	mComponent = component;
}

//------------------------------------------------------------------------
void HostCheck::setProcessSetup (Steinberg::Vst::ProcessSetup& setup)
{
	mProcessSetupCheck.setProcessSetup (setup);
	mEventListCheck.setProcessSetup (setup);
	mProcessContextCheck.setSampleRate (setup.sampleRate);
}
