//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again_sampleaccurate/source/agsa_processor.cpp
// Created by  : Steinberg, 04/2021
// Description : AGain with Sample Accurate Parameter Changes
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "agsa.h"
#include "public.sdk/source/vst/utility/audiobuffers.h"
#include "public.sdk/source/vst/utility/processdataslicer.h"
#include "public.sdk/source/vst/utility/rttransfer.h"
#include "public.sdk/source/vst/utility/sampleaccurate.h"
#include "public.sdk/source/vst/vstaudioeffect.h"
#include "base/source/fstreamer.h"
#include <array>
#include <cassert>
#include <limits>
#include <vector>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace AgainSampleAccurate {

//------------------------------------------------------------------------
struct Processor : public AudioEffect
{
	using ParameterVector = std::vector<std::pair<ParamID, ParamValue>>;
	using RTTransfer = RTTransferT<ParameterVector>;

	Processor ();
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API terminate () SMTG_OVERRIDE;
	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API setBusArrangements (SpeakerArrangement* inputs, int32 numIns,
	                                       SpeakerArrangement* outputs,
	                                       int32 numOuts) SMTG_OVERRIDE;
	tresult PLUGIN_API canProcessSampleSize (int32 symbolicSampleSize) SMTG_OVERRIDE;
	tresult PLUGIN_API process (ProcessData& data) SMTG_OVERRIDE;

	void handleParameterChanges (IParameterChanges* changes);

	template <SymbolicSampleSizes SampleSize>
	void process (ProcessData& data);

	std::array<SampleAccurate::Parameter, 2> parameters;
	RTTransfer stateTransfer;
};

//------------------------------------------------------------------------
Processor::Processor ()
{
	setControllerClass (ControllerID);
	parameters[0].setParamID (ParameterID::Bypass);
	parameters[1].setParamID (ParameterID::Gain);
}

//------------------------------------------------------------------------
tresult PLUGIN_API Processor::initialize (FUnknown* context)
{
	auto result = AudioEffect::initialize (context);
	if (result == kResultTrue)
	{
		addAudioInput (STR ("Input"), SpeakerArr::kStereo);
		addAudioOutput (STR ("Output"), SpeakerArr::kStereo);
	}
	return result;
}

//------------------------------------------------------------------------
tresult PLUGIN_API Processor::terminate ()
{
	stateTransfer.clear_ui ();
	return AudioEffect::terminate ();
}

//------------------------------------------------------------------------
tresult PLUGIN_API Processor::setState (IBStream* state)
{
	if (!state)
		return kInvalidArgument;

	IBStreamer streamer (state, kLittleEndian);

	uint32 numParams;
	if (streamer.readInt32u (numParams) == false)
		return kResultFalse;

	auto paramChanges = std::make_unique<ParameterVector> ();
	ParamID pid;
	ParamValue value;
	for (uint32 i = 0u; i < numParams; ++i)
	{
		if (!streamer.readInt32u (pid))
			break;
		if (!streamer.readDouble (value))
			break;
		for (auto& param : parameters)
		{
			if (param.getParamID () == pid)
			{
				paramChanges->emplace_back (std::make_pair (pid, value));
				break;
			}
		}
	}
	stateTransfer.transferObject_ui (std::move (paramChanges));
	return kResultTrue;
}

//------------------------------------------------------------------------
tresult PLUGIN_API Processor::getState (IBStream* state)
{
	if (!state)
		return kInvalidArgument;

	IBStreamer streamer (state, kLittleEndian);
	streamer.writeInt32u (static_cast<uint32> (parameters.size ()));
	for (auto& param : parameters)
	{
		streamer.writeInt32u (param.getParamID ());
		streamer.writeDouble (param.getValue ());
	}
	return kResultTrue;
}

//------------------------------------------------------------------------
tresult PLUGIN_API Processor::setBusArrangements (SpeakerArrangement* inputs, int32 numIns,
                                                  SpeakerArrangement* outputs, int32 numOuts)
{
	if (numIns != 1 || numOuts != 1)
		return kResultFalse;
	if (SpeakerArr::getChannelCount (inputs[0]) == SpeakerArr::getChannelCount (outputs[0]))
	{
		getAudioInput (0)->setArrangement (inputs[0]);
		getAudioOutput (0)->setArrangement (outputs[0]);
		return kResultTrue;
	}
	return kResultFalse;
}

//------------------------------------------------------------------------
tresult PLUGIN_API Processor::canProcessSampleSize (int32 symbolicSampleSize)
{
	return (symbolicSampleSize == SymbolicSampleSizes::kSample32 ||
	        symbolicSampleSize == SymbolicSampleSizes::kSample64) ?
	           kResultTrue :
	           kResultFalse;
}

//------------------------------------------------------------------------
template <SymbolicSampleSizes SampleSize>
void Processor::process (ProcessData& data)
{
	using SampleT = typename std::conditional<SampleSize == SymbolicSampleSizes::kSample32, float,
	                                          double>::type;

	static constexpr auto SliceSize = 16u;

	ProcessDataSlicer slicer (SliceSize);
	std::array<SampleT, SliceSize> againValueBuffer;

	auto doProcessing = [this, &againValueBuffer] (ProcessData& data) {
		parameters[ParameterID::Bypass].advance (data.numSamples);

		auto inputs = data.inputs;
		auto outputs = data.outputs;
		if (parameters[ParameterID::Bypass].getValue () > 0.5)
		{
			for (auto channelIndex = 0; channelIndex < inputs[0].numChannels; ++channelIndex)
			{
				auto inChannelBuffer = getChannelBuffers<SampleSize> (inputs[0])[channelIndex];
				auto outChannelBuffer = getChannelBuffers<SampleSize> (outputs[0])[channelIndex];
				for (auto sampleIndex = 0; sampleIndex < data.numSamples; ++sampleIndex)
				{
					outChannelBuffer[sampleIndex] = inChannelBuffer[sampleIndex];
				}
			}
			return;
		}
		for (auto i = 0; i < data.numSamples; ++i)
			againValueBuffer[i] = static_cast<SampleT> (parameters[ParameterID::Gain].advance (1));

		for (auto channelIndex = 0; channelIndex < inputs[0].numChannels; ++channelIndex)
		{
			auto inChannelBuffer = getChannelBuffers<SampleSize> (inputs[0])[channelIndex];
			auto outChannelBuffer = getChannelBuffers<SampleSize> (outputs[0])[channelIndex];
			for (auto sampleIndex = 0; sampleIndex < data.numSamples; ++sampleIndex)
			{
				auto sample = inChannelBuffer[sampleIndex] * againValueBuffer[sampleIndex];
				outChannelBuffer[sampleIndex] = sample;
			}
		}
	};

	slicer.process<SampleSize> (data, doProcessing);
}

//------------------------------------------------------------------------
void Processor::handleParameterChanges (IParameterChanges* changes)
{
	if (changes)
	{
		auto changeCount = changes->getParameterCount ();
		for (auto i = 0; i < changeCount; ++i)
		{
			if (auto queue = changes->getParameterData (i))
			{
				auto paramID = queue->getParameterId ();
				if (paramID >= ParameterID::Bypass && paramID <= ParameterID::Gain)
					parameters[paramID].beginChanges (queue);
			}
		}
	}
}

//------------------------------------------------------------------------
tresult PLUGIN_API Processor::process (ProcessData& data)
{
	stateTransfer.accessTransferObject_rt ([this] (const auto& stateChanges) {
		for (const auto& change : stateChanges)
		{
			if (change.first >= ParameterID::Bypass && change.first <= ParameterID::Gain)
				parameters[change.first].setValue (change.second);
		}
	});

	handleParameterChanges (data.inputParameterChanges);

	if (data.numSamples > 0)
	{
		if (processSetup.symbolicSampleSize == SymbolicSampleSizes::kSample32)
			process<SymbolicSampleSizes::kSample32> (data);
		else
			process<SymbolicSampleSizes::kSample64> (data);
	}

	for (auto& param : parameters)
		param.endChanges ();

	return kResultTrue;
}

//------------------------------------------------------------------------
FUnknown* createProcessorInstance (void*)
{
	return static_cast<IAudioProcessor*> (new Processor);
}

//------------------------------------------------------------------------
} // AgainSampleAccurate
} // Vst
} // Steinberg
