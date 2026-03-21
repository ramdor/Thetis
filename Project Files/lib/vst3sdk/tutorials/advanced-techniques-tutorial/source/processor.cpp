//------------------------------------------------------------------------
// Copyright(c) 2023 Steinberg Media Technologies.
//------------------------------------------------------------------------

#include "cids.h"
#include "pids.h"
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
namespace Steinberg::Tutorial {

using namespace Steinberg::Vst;

//------------------------------------------------------------------------
struct StateModel
{
	double gain;
};

//------------------------------------------------------------------------
struct MyEffect : public AudioEffect
{
	using RTTransfer = RTTransferT<StateModel>;

	MyEffect ();
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

	SampleAccurate::Parameter gainParameter {ParameterID::Gain, 1.};
	RTTransfer stateTransfer;
};

//------------------------------------------------------------------------
MyEffect::MyEffect ()
{
	setControllerClass (ControllerUID);
}

//------------------------------------------------------------------------
tresult PLUGIN_API MyEffect::initialize (FUnknown* context)
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
tresult PLUGIN_API MyEffect::terminate ()
{
	stateTransfer.clear_ui ();
	return AudioEffect::terminate ();
}

//------------------------------------------------------------------------
tresult PLUGIN_API MyEffect::setState (IBStream* state)
{
	if (!state)
		return kInvalidArgument;

	IBStreamer streamer (state, kLittleEndian);

	uint32 numParams;
	if (streamer.readInt32u (numParams) == false)
		return kResultFalse;

	auto model = std::make_unique<StateModel> ();

	ParamValue value;
	if (!streamer.readDouble (value))
		return kResultFalse;

	model->gain = value;

	stateTransfer.transferObject_ui (std::move (model));
	return kResultTrue;
}

//------------------------------------------------------------------------
tresult PLUGIN_API MyEffect::getState (IBStream* state)
{
	if (!state)
		return kInvalidArgument;

	IBStreamer streamer (state, kLittleEndian);
	streamer.writeDouble (gainParameter.getValue ());
	return kResultTrue;
}

//------------------------------------------------------------------------
tresult PLUGIN_API MyEffect::setBusArrangements (SpeakerArrangement* inputs, int32 numIns,
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
tresult PLUGIN_API MyEffect::canProcessSampleSize (int32 symbolicSampleSize)
{
	return (symbolicSampleSize == SymbolicSampleSizes::kSample32 ||
	        symbolicSampleSize == SymbolicSampleSizes::kSample64) ?
	           kResultTrue :
	           kResultFalse;
}

//------------------------------------------------------------------------
template <SymbolicSampleSizes SampleSize>
void MyEffect::process (ProcessData& data)
{
	ProcessDataSlicer slicer (8);

	auto doProcessing = [this] (ProcessData& data) {
		// get the gain value for this block
		ParamValue gain = gainParameter.advance (data.numSamples);

		// process audio
		AudioBusBuffers* inputs = data.inputs;
		AudioBusBuffers* outputs = data.outputs;
		for (auto channelIndex = 0; channelIndex < inputs[0].numChannels; ++channelIndex)
		{
			auto inputBuffers = getChannelBuffers<SampleSize> (inputs[0])[channelIndex];
			auto outputBuffers = getChannelBuffers<SampleSize> (outputs[0])[channelIndex];
			for (auto sampleIndex = 0; sampleIndex < data.numSamples; ++sampleIndex)
			{
				auto sample = inputBuffers[sampleIndex];
				outputBuffers[sampleIndex] = sample * gain;
			}
		}
	};

	slicer.process<SampleSize> (data, doProcessing);
}

//------------------------------------------------------------------------
void MyEffect::handleParameterChanges (IParameterChanges* changes)
{
	if (!changes)
		return;
	int32 changeCount = changes->getParameterCount ();
	for (auto i = 0; i < changeCount; ++i)
	{
		if (auto queue = changes->getParameterData (i))
		{
			auto paramID = queue->getParameterId ();
			if (paramID == ParameterID::Gain)
			{
				gainParameter.beginChanges (queue);
			}
		}
	}
}

//------------------------------------------------------------------------
tresult PLUGIN_API MyEffect::process (ProcessData& data)
{
	stateTransfer.accessTransferObject_rt (
	    [this] (const auto& stateModel) { gainParameter.setValue (stateModel.gain); });

	handleParameterChanges (data.inputParameterChanges);

	if (processSetup.symbolicSampleSize == SymbolicSampleSizes::kSample32)
		process<SymbolicSampleSizes::kSample32> (data);
	else
		process<SymbolicSampleSizes::kSample64> (data);

	gainParameter.endChanges ();
	return kResultTrue;
}

//------------------------------------------------------------------------
FUnknown* createProcessorInstance (void*)
{
	return static_cast<IAudioProcessor*> (new MyEffect);
}

} // Steinberg::Tutorial
