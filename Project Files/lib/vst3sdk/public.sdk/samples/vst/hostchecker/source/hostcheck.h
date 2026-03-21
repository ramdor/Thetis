//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostcheck/source/hostcheck.h
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

#pragma once

#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include "pluginterfaces/vst/ivstevents.h"
#include <vector>
#include <set>
#include "logevents.h"
#include "eventlogger.h"
#include "processsetupcheck.h"
#include "processcontextcheck.h"
#include "eventlistcheck.h"
#include "parameterchangescheck.h"

namespace Steinberg {
namespace Vst {
class IEventList;
class IComponent;
}
}
//------------------------------------------------------------------------
//	ProcessDataValidator
//------------------------------------------------------------------------
class HostCheck
{
public:
//------------------------------------------------------------------------
	static HostCheck& Instance ()
	{
		static HostCheck instance;
		return instance;
	}

	using ParamIDs = std::set<Steinberg::Vst::ParamID>;

	HostCheck ();
	void addParameter (Steinberg::Vst::ParamID paramId);
	void setProcessSetup (Steinberg::Vst::ProcessSetup& setup);
	void setComponent (Steinberg::Vst::IComponent* component);
	bool validate (Steinberg::Vst::ProcessData& data, Steinberg::int32 minInputBufferCount,
	               Steinberg::int32 minOutputBufferCount);

	const EventLogger::Codes& getEventLogs () const { return mEventLogger.getLogEvents (); }

	EventLogger& getEventLogger ()
	{
		return mEventLogger;
	} /// Caution logger is used by audio thread...!!!
//------------------------------------------------------------------------
private:
	void addLogEvent (Steinberg::int32 logId);
	void checkAudioBuffers (Steinberg::Vst::AudioBusBuffers* buffers, Steinberg::int32 numBuffers,
	                        Steinberg::Vst::BusDirection dir, Steinberg::int32 symbolicSampleSize,
	                        Steinberg::int32 minBufferCount);

	Steinberg::Vst::IComponent* mComponent {nullptr};
	ParamIDs mParameterIds;

	ProcessSetupCheck mProcessSetupCheck;
	ProcessContextCheck mProcessContextCheck;
	EventListCheck mEventListCheck;
	ParameterChangesCheck mParamChangesCheck;
	EventLogger mEventLogger;
};

//------------------------------------------------------------------------
