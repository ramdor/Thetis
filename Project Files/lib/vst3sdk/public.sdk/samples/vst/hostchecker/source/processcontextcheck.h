//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/processcontextcheck.h
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

#pragma once

#include "pluginterfaces/vst/ivstaudioprocessor.h"

class EventLogger;

//------------------------------------------------------------------------
//    ProcessContextCheck
//------------------------------------------------------------------------
class ProcessContextCheck
{
public:
//------------------------------------------------------------------------
	ProcessContextCheck ();
	void setEventLogger (EventLogger* eventLogger);
	void setSampleRate (double sampleRate);
	void check (Steinberg::Vst::ProcessContext* context);

//------------------------------------------------------------------------
protected:
	EventLogger* mEventLogger;
	double mSampleRate;
	Steinberg::int64 mLastSystemTime {0};
};
