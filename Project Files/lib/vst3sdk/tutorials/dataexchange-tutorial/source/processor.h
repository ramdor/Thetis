//------------------------------------------------------------------------
// Copyright(c) 2023 Steinberg Media Technologies.
//------------------------------------------------------------------------

#pragma once

#include "dataexchange.h"
#include "public.sdk/source/vst/vstaudioeffect.h"

namespace Steinberg::Tutorial {

//------------------------------------------------------------------------
static constexpr Vst::DataExchangeBlock InvalidDataExchangeBlock = {
    nullptr, 0, Vst::InvalidDataExchangeBlockID};

//------------------------------------------------------------------------
//  DataExchangeProcessor
//------------------------------------------------------------------------
class DataExchangeProcessor : public Vst::AudioEffect
{
public:
	DataExchangeProcessor ();
	~DataExchangeProcessor () override;

	static FUnknown* createInstance (void* /*context*/)
	{
		return (Vst::IAudioProcessor*)new DataExchangeProcessor;
	}

	tresult PLUGIN_API initialize (FUnknown* context) override;
	tresult PLUGIN_API connect (Vst::IConnectionPoint* other) override;
	tresult PLUGIN_API disconnect (Vst::IConnectionPoint* other) override;
	tresult PLUGIN_API setActive (TBool state) override;
	tresult PLUGIN_API canProcessSampleSize (int32 symbolicSampleSize) override;
	tresult PLUGIN_API process (Vst::ProcessData& data) override;
//------------------------------------------------------------------------
protected:
	void acquireNewExchangeBlock ();

	std::unique_ptr<Vst::DataExchangeHandler> dataExchange;
	Vst::DataExchangeBlock currentExchangeBlock {InvalidDataExchangeBlock};
	uint16_t numChannels {0};
};

//------------------------------------------------------------------------
} // namespace Steinberg::Tutorial
