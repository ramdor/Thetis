//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/processsetupcheck.h
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

#pragma once
 
#include "eventlogger.h"
#include "pluginterfaces/vst/ivstaudioprocessor.h"

//------------------------------------------------------------------------
//	ProcessSetupCheck
//------------------------------------------------------------------------
class ProcessSetupCheck
{
public:
//------------------------------------------------------------------------
    ProcessSetupCheck ();
    
	void setProcessSetup (Steinberg::Vst::ProcessSetup setup);
	void setEventLogger (EventLogger* eventLogger);
	void check (const Steinberg::Vst::ProcessData& data);

//------------------------------------------------------------------------
protected:
    Steinberg::Vst::ProcessSetup mSetup;
	EventLogger* mEventLogger;
};
