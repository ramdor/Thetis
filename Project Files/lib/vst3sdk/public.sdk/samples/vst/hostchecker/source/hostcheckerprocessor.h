//-----------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/hostcheckerprocessor.h
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

#include "hostcheck.h"
#include "logevents.h"

#include "public.sdk/source/common/threadchecker.h"
#include "public.sdk/source/vst/utility/dataexchange.h"
#include "public.sdk/source/vst/vstaudioeffect.h"
#include "public.sdk/source/vst/vstbypassprocessor.h"
#include "base/thread/include/flock.h"
#include "pluginterfaces/vst/ivstprefetchablesupport.h"

#include <list>

namespace Steinberg {
namespace Vst {

static constexpr DataExchangeBlock InvalidDataExchangeBlock = {nullptr, 0,
                                                               InvalidDataExchangeBlockID};

//-----------------------------------------------------------------------------
class HostCheckerProcessor : public AudioEffect,
                             public IAudioPresentationLatency,
                             public IPrefetchableSupport
{
public:
	HostCheckerProcessor ();

	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API terminate () SMTG_OVERRIDE;

	tresult PLUGIN_API process (ProcessData& data) SMTG_OVERRIDE;
	tresult PLUGIN_API setupProcessing (ProcessSetup& setup) SMTG_OVERRIDE;
	tresult PLUGIN_API setActive (TBool state) SMTG_OVERRIDE;
	tresult PLUGIN_API notify (IMessage* message) SMTG_OVERRIDE;
	uint32 PLUGIN_API getLatencySamples () SMTG_OVERRIDE;
	uint32 PLUGIN_API getTailSamples () SMTG_OVERRIDE;
	tresult PLUGIN_API canProcessSampleSize (int32 symbolicSampleSize) SMTG_OVERRIDE;
	tresult PLUGIN_API setProcessing (TBool state) SMTG_OVERRIDE;

	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;

	tresult PLUGIN_API getRoutingInfo (RoutingInfo& inInfo, RoutingInfo& outInfo) SMTG_OVERRIDE;
	tresult PLUGIN_API activateBus (MediaType type, BusDirection dir, int32 index,
	                                TBool state) SMTG_OVERRIDE;
	tresult PLUGIN_API setBusArrangements (SpeakerArrangement* inputs, int32 numIns,
	                                       SpeakerArrangement* outputs,
	                                       int32 numOuts) SMTG_OVERRIDE;
	tresult PLUGIN_API getBusArrangement (BusDirection dir, int32 busIndex,
	                                      SpeakerArrangement& arr) SMTG_OVERRIDE;

	static FUnknown* createInstance (void*)
	{
		return (IAudioProcessor*)new HostCheckerProcessor ();
	}

	tresult PLUGIN_API connect (IConnectionPoint* other) SMTG_OVERRIDE;
	tresult PLUGIN_API disconnect (IConnectionPoint* other) SMTG_OVERRIDE;

	//---IAudioPresentationLatency-------------------------
	tresult PLUGIN_API setAudioPresentationLatencySamples (BusDirection dir, int32 busIndex,
	                                                       uint32 latencyInSamples) SMTG_OVERRIDE;

	//---IPrefetchableSupport------------------------------
	tresult PLUGIN_API getPrefetchableSupport (PrefetchableSupport& prefetchable /*out*/)
	    SMTG_OVERRIDE;

	//---IProcessContextRequirements-----------------------
	uint32 PLUGIN_API getProcessContextRequirements () SMTG_OVERRIDE;

	DEFINE_INTERFACES
		DEF_INTERFACE (IAudioPresentationLatency)
		DEF_INTERFACE (IPrefetchableSupport)
	END_DEFINE_INTERFACES (AudioEffect)
	REFCOUNT_METHODS (AudioEffect)

	enum class State : uint32
	{
		kUninitialized = 0,
		kInitialized,
		kSetupDone,
		kActivated,
		kProcessing
	};

protected:
	void addLogEvent (Steinberg::int32 logId);

	void informLatencyChanged ();
	void sendLatencyChanged ();

	void addLogEventMessage (const LogEvent& logEvent);
	void sendLogEventMessage (const LogEvent& logEvent);
	void sendNowAllLogEvents ();

	ProcessContext* getCurrentExchangeData ();

	HostCheck mHostCheck;

	BypassProcessor<Vst::Sample32> mBypassProcessorFloat;
	BypassProcessor<Vst::Sample64> mBypassProcessorDouble;

	DataExchangeBlock mCurrentExchangeBlock {InvalidDataExchangeBlock};

	float mLastBlockMarkerValue {-0.5f};

	int32 mNumNoteOns {0};
	uint32 mLatency {0}; // in samples
	uint32 mWantedLatency {0}; // in samples
	float mGeneratePeaks {0.f};
	float mProcessingLoad {0.f};
	State mCurrentState {State::kUninitialized};

	uint32 mMinimumOfInputBufferCount {0};
	uint32 mMinimumOfOutputBufferCount {0};

	TSamples mLastContinuousProjectTimeSamples {kMinInt64};
	TSamples mLastProjectTimeSamples {kMinInt64};
	int32 mLastNumSamples {0};
	uint32 mLastState {0};

	std::unique_ptr<ThreadChecker> threadChecker {ThreadChecker::create ()};

	Steinberg::Base::Thread::FLock msgQueueLock;
	std::list<LogEvent*> msgQueue;

	DataExchangeHandler* dataExchangeHandler {nullptr};
	int64 mLastExchangeBlockSendSystemTime {0};
	int32 mLastProcessMode {-1};

	bool mBypass {false};
	bool mSetActiveCalled {false};
	bool mCheckGetLatencyCall {true};
	bool mGetLatencyCalled {false};
	bool mGetLatencyCalledAfterSetActive {false};
};

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
