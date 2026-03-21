//------------------------------------------------------------------------
// Copyright(c) 2023 Steinberg Media Technologies.
//------------------------------------------------------------------------

#pragma once

#include "public.sdk/source/vst/utility/dataexchange.h"
#include <cstdint>

//------------------------------------------------------------------------
namespace Steinberg::Tutorial {

//------------------------------------------------------------------------
struct DataBlock
{
	uint32_t sampleRate;
	uint16_t sampleSize;
	uint16_t numChannels;
	uint32_t numSamples;
	float samples[0];
};

//------------------------------------------------------------------------
inline DataBlock* toDataBlock (const Vst::DataExchangeBlock& block)
{
	if (block.blockID != Vst::InvalidDataExchangeBlockID)
		return reinterpret_cast<DataBlock*> (block.data);
	return nullptr;
}

//------------------------------------------------------------------------
} // Steinberg::Tutorial
