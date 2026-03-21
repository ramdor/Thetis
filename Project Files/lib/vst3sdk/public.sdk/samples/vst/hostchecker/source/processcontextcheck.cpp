//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/processcontextcheck.cpp
// Created by  : Steinberg, 12/2012
// Description : Process Context check
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "pluginterfaces/vst/ivstprocesscontext.h"
#include "processcontextcheck.h"
#include "eventlogger.h"
#include "logevents.h"

using namespace Steinberg::Vst;
//------------------------------------------------------------------------
//	ProcessContextCheck
//------------------------------------------------------------------------
ProcessContextCheck::ProcessContextCheck () : mEventLogger (nullptr), mSampleRate (0) {}

//------------------------------------------------------------------------
void ProcessContextCheck::setEventLogger (EventLogger* eventLogger) { mEventLogger = eventLogger; }

//------------------------------------------------------------------------
void ProcessContextCheck::check (ProcessContext* context)
{
	if (!context)
	{
		mEventLogger->addLogEvent (kLogIdProcessContextPointerNull);
		return;
	}

	if (context->sampleRate != mSampleRate)
	{
		mEventLogger->addLogEvent (kLogIdInvalidProcessContextSampleRate);
	}
	if (context->state & ProcessContext::StatesAndFlags::kSystemTimeValid)
	{
		if (mLastSystemTime >= context->systemTime)
		{
			mEventLogger->addLogEvent (kLogIdInvalidProcessContextSystemTime);
		}
		mLastSystemTime = context->systemTime;
	}
}

//------------------------------------------------------------------------
void ProcessContextCheck::setSampleRate (double sampleRate) { mSampleRate = sampleRate; }
