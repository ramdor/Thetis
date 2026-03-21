//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/eventlistcheck.h
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
#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include "pluginterfaces/vst/ivstnoteexpression.h"
#include <set>

class EventLogger;

namespace Steinberg {
namespace Vst {
class IEventList;
class IComponent;
struct ProcessSetup;
struct Event;
}
}

//------------------------------------------------------------------------
//	EventListCheck
//------------------------------------------------------------------------
class EventListCheck
{
public:
//------------------------------------------------------------------------
	EventListCheck ();

	static const Steinberg::int32 kMaxEvents = 2048;
	using Notes = std::set<Steinberg::int32>;

	void check (Steinberg::Vst::IEventList* events);
	void setComponent (Steinberg::Vst::IComponent* component) { mComponent = component; }
	void setProcessSetup (Steinberg::Vst::ProcessSetup setup);
	void setEventLogger (EventLogger* eventLogger);

//------------------------------------------------------------------------
protected:
	bool checkEventCount (Steinberg::Vst::IEventList* events);
	void checkEventProperties (const Steinberg::Vst::Event& event);
	bool checkEventBusIndex (Steinberg::int32 busIndex);
	bool checkEventSampleOffset (Steinberg::int32 sampleOffset);
	bool checkEventChannelIndex (Steinberg::int32 busIndex, Steinberg::int32 channelIndex);
	bool checkValidPitch (Steinberg::int16 pitch);
	bool isNormalized (double normVal) const;
	void checkNoteExpressionValueEvent (Steinberg::Vst::NoteExpressionTypeID type,
	                                    Steinberg::int32 id,
	                                    Steinberg::Vst::NoteExpressionValue exprVal) const;

	EventLogger* mEventLogger;
	Steinberg::Vst::IComponent* mComponent;
	Steinberg::Vst::ProcessSetup mSetup;
	Notes mNotePitches;
	Notes mNoteIDs;
};
