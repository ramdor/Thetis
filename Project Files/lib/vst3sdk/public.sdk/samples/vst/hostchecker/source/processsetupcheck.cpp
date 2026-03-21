//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/processsetupcheck.cpp
// Created by  : Steinberg, 12/2012
// Description : VST::ProcessSetup check
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "processsetupcheck.h"
#include "logevents.h"

//------------------------------------------------------------------------
//    ProcessSetupCheck
//------------------------------------------------------------------------
ProcessSetupCheck::ProcessSetupCheck () : mEventLogger (nullptr) {}

//------------------------------------------------------------------------
void ProcessSetupCheck::setProcessSetup (Steinberg::Vst::ProcessSetup setup) { mSetup = setup; }

//------------------------------------------------------------------------
void ProcessSetupCheck::check (const Steinberg::Vst::ProcessData& data)
{
	if (data.symbolicSampleSize != mSetup.symbolicSampleSize)
	{
		mEventLogger->addLogEvent (kLogIdInvalidSymbolicSampleSize);
	}

	if (data.processMode != mSetup.processMode)
	{
		// exception toggle between kRealtime kPrefetch
		if (!((mSetup.processMode == Steinberg::Vst::kRealtime &&
		       data.processMode == Steinberg::Vst::kPrefetch) ||
		      (mSetup.processMode == Steinberg::Vst::kPrefetch &&
		       data.processMode == Steinberg::Vst::kRealtime)))
			mEventLogger->addLogEvent (kLogIdInvalidProcessMode);
	}

	if (data.numSamples < 0 || data.numSamples > mSetup.maxSamplesPerBlock)
	{
		mEventLogger->addLogEvent (kLogIdInvalidBlockSize);
	}
}

//------------------------------------------------------------------------
void ProcessSetupCheck::setEventLogger (EventLogger* eventLogger) { mEventLogger = eventLogger; }
