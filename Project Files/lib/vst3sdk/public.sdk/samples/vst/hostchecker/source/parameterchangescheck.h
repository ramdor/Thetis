//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/parameterchangescheck.h
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

#pragma once

#include "pluginterfaces/base/ftypes.h"
#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include "pluginterfaces/vst/ivstnoteexpression.h"
#include <set>
#include <vector>

class EventLogger;

namespace Steinberg {
namespace Vst {
class IParamValueQueue;
}
}

//------------------------------------------------------------------------
//	EventListCheck
//------------------------------------------------------------------------
class ParameterChangesCheck
{
public:
//------------------------------------------------------------------------
	ParameterChangesCheck ();

	using ParamIDs = std::set<Steinberg::Vst::ParamID>;

	void checkParameterChanges (Steinberg::Vst::IParameterChanges* paramChanges);
	void setEventLogger (EventLogger* eventLogger);
	void setParamIDs (ParamIDs* parameterID);
	void updateParameterIDs ();
//------------------------------------------------------------------------
protected:
	void checkAllChanges (Steinberg::Vst::IParameterChanges* paramChanges);
	void checkParameterCount (Steinberg::int32 paramCount);
	void checkParameterId (Steinberg::Vst::ParamID paramId);
	void checkNormalized (double normVal);
	void checkSampleOffset (Steinberg::int32 sampleOffset, Steinberg::int32 lastSampleOffset);
	bool checkParameterQueue (Steinberg::Vst::IParamValueQueue* paramQueue);
	void checkPoints (Steinberg::Vst::IParamValueQueue* paramQueue);

	bool isNormalized (double normVal) const;
	bool isValidSampleOffset (Steinberg::int32 sampleOffset, Steinberg::int32 lastSampleOffset) const;
	bool isValidParamID (Steinberg::Vst::ParamID paramId) const;
	bool isValidParamCount (Steinberg::int32 paramCount) const;

	EventLogger* mEventLogger;
	ParamIDs* mParameterIds;
	std::vector<Steinberg::Vst::ParamID> mTempUsedId;
};
