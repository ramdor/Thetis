//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/eventlogger.cpp
// Created by  : Steinberg, 12/2012
// Description : Event List check
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "eventlogger.h"
#include "logevents.h"

//------------------------------------------------------------------------
//    EventLogger
//------------------------------------------------------------------------
EventLogger::EventLogger ()
{
	mLogEvents.resize (kNumLogEvents);
	for (Steinberg::uint32 i = 0; i < mLogEvents.size (); ++i)
	{
		mLogEvents[i].id = i;
		mLogEvents[i].fromProcessor = logEventContext[i];
	}
}

//------------------------------------------------------------------------
void EventLogger::clearLogEvents ()
{
	mLogEvents.clear ();
}

//------------------------------------------------------------------------
void EventLogger::resetLogEvents ()
{
	for (auto& mLogEvent : mLogEvents)
	{
		mLogEvent.count = 0;
	}
}

//------------------------------------------------------------------------
const EventLogger::Codes& EventLogger::getLogEvents () const { return mLogEvents; }

//------------------------------------------------------------------------
void EventLogger::addLogEvent (Steinberg::int32 logId)
{
	LogEvent& logEvent = mLogEvents.at (logId);
	logEvent.count++;
}
