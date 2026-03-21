//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/eventlogger.h
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

#pragma once

#include "pluginterfaces/base/ftypes.h"
#include <vector>

//------------------------------------------------------------------------
//    EventLogger
//------------------------------------------------------------------------
class EventLogger
{
public:
//------------------------------------------------------------------------
	EventLogger ();

	using Codes = std::vector<struct LogEvent>;

	void clearLogEvents ();
	void resetLogEvents ();
	const Codes& getLogEvents () const;
	void addLogEvent (Steinberg::int32 logId);
	bool empty () const { return mLogEvents.empty (); }

//------------------------------------------------------------------------
protected:
	Codes mLogEvents;
};
