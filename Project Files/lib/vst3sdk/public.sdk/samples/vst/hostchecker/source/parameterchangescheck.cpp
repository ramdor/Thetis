//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/parameterchangescheck.cpp
// Created by  : Steinberg, 12/2012
// Description : ParameterChanges check
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "parameterchangescheck.h"
#include "eventlogger.h"
#include "logevents.h"
#include "pluginterfaces/vst/ivstparameterchanges.h"

//------------------------------------------------------------------------
//    ParameterChangesCheck
//------------------------------------------------------------------------
ParameterChangesCheck::ParameterChangesCheck () : mEventLogger (nullptr), mParameterIds (nullptr) {}

//------------------------------------------------------------------------
void ParameterChangesCheck::checkParameterChanges (Steinberg::Vst::IParameterChanges* paramChanges)
{
	if (!paramChanges)
	{
		mEventLogger->addLogEvent (kLogIdParameterChangesPointerIsNull);
		return;
	}

	checkParameterCount (paramChanges->getParameterCount ());
	checkAllChanges (paramChanges);
}

//------------------------------------------------------------------------
void ParameterChangesCheck::checkAllChanges (Steinberg::Vst::IParameterChanges* paramChanges)
{
	for (Steinberg::int32 paramIdx = 0; paramIdx < paramChanges->getParameterCount (); ++paramIdx)
	{
		Steinberg::Vst::IParamValueQueue* paramQueue = paramChanges->getParameterData (paramIdx);
		if (checkParameterQueue (paramQueue))
		{
			bool found = false;
			auto id = paramQueue->getParameterId ();
			for (auto item : mTempUsedId)
			{
				if (item == id)
				{
					mEventLogger->addLogEvent (kLogIdParameterIDMoreThanOneTimeinList);
					found = true;
					break;
				}
			}
			if (!found)
				mTempUsedId.emplace_back (id);

			checkParameterId (id);
			checkPoints (paramQueue);
		}
	}
	mTempUsedId.clear ();
}

//------------------------------------------------------------------------
void ParameterChangesCheck::checkPoints (Steinberg::Vst::IParamValueQueue* paramQueue)
{
	Steinberg::int32 lastLastSampleOffset = -1;
	Steinberg::int32 lastSampleOffset = -1;
	for (Steinberg::int32 pointIdx = 0; pointIdx < paramQueue->getPointCount (); ++pointIdx)
	{
		Steinberg::int32 sampleOffset = 0;
		Steinberg::Vst::ParamValue paramValue = 0;
		if (paramQueue->getPoint (pointIdx, sampleOffset, paramValue) == Steinberg::kResultOk)
		{
			checkNormalized (paramValue);
			checkSampleOffset (sampleOffset, lastSampleOffset);
			lastLastSampleOffset = lastSampleOffset;
			lastSampleOffset = sampleOffset;
			// here we have more than 3 points at the same sample position
			if (lastLastSampleOffset == sampleOffset)
				mEventLogger->addLogEvent (kLogIdParametersHaveSameSampleOffset);
		}
	}
}

//------------------------------------------------------------------------
void ParameterChangesCheck::setEventLogger (EventLogger* eventLogger)
{
	mEventLogger = eventLogger;
}

//------------------------------------------------------------------------
void ParameterChangesCheck::setParamIDs (ParamIDs* parameterID)
{
	mParameterIds = parameterID;
	updateParameterIDs ();
}

//------------------------------------------------------------------------
void ParameterChangesCheck::updateParameterIDs ()
{
	if (mParameterIds)
		mTempUsedId.resize (mParameterIds->size ());
}

//------------------------------------------------------------------------
void ParameterChangesCheck::checkParameterCount (Steinberg::int32 paramCount)
{
	if (!isValidParamCount (paramCount))
	{
		mEventLogger->addLogEvent (kLogIdInvalidParameterCount);
	}
}

//------------------------------------------------------------------------
bool ParameterChangesCheck::isValidParamCount (Steinberg::int32 paramCount) const
{
	return paramCount <= (Steinberg::int32)mParameterIds->size ();
}

//------------------------------------------------------------------------
void ParameterChangesCheck::checkParameterId (Steinberg::Vst::ParamID paramId)
{
	if (!isValidParamID (paramId))
	{
		mEventLogger->addLogEvent (kLogIdInvalidParameterID);
	}
}

//------------------------------------------------------------------------
bool ParameterChangesCheck::isValidParamID (Steinberg::Vst::ParamID paramId) const
{
	return mParameterIds->find (paramId) != mParameterIds->end ();
}

//------------------------------------------------------------------------
void ParameterChangesCheck::checkNormalized (double normVal)
{
	if (!isNormalized (normVal))
	{
		mEventLogger->addLogEvent (kLogIdInvalidParamValue);
	}
}

//------------------------------------------------------------------------
void ParameterChangesCheck::checkSampleOffset (Steinberg::int32 sampleOffset,
                                               Steinberg::int32 lastSampleOffset)
{
	if (!isValidSampleOffset (sampleOffset, lastSampleOffset))
	{
		mEventLogger->addLogEvent (kLogIdParametersAreNotSortedBySampleOffset);
	}
}

//------------------------------------------------------------------------
bool ParameterChangesCheck::isNormalized (double normVal) const
{
	return normVal >= 0. && normVal <= 1.;
}

//------------------------------------------------------------------------
bool ParameterChangesCheck::isValidSampleOffset (Steinberg::int32 sampleOffset, Steinberg::int32 lastSampleOffset) const
{
	return sampleOffset >= lastSampleOffset;
}

//------------------------------------------------------------------------
bool ParameterChangesCheck::checkParameterQueue (Steinberg::Vst::IParamValueQueue* paramQueue)
{
	if (!paramQueue)
	{
		mEventLogger->addLogEvent (kLogIdParameterQueueIsNullForValidIndex);
	}

	return paramQueue != nullptr;
}
