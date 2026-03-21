//-----------------------------------------------------------------------------
// Project     : VST SDK
// Flags       : clang-format SMTGSequencer
// Category    : Examples
// Filename    : public.sdk/samples/vst/dataexchange/shared.h
// Created by  : Steinberg, 06/2023
// Description : VST Data Exchange API Example
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "pluginterfaces/base/funknown.h"
#include "pluginterfaces/vst/ivstdataexchange.h"
#include "pluginterfaces/vst/vsttypes.h"

#include <cassert>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
static DECLARE_UID (DataExchangeProcessorUID, 0x2AF3DF1C, 0x93D243B3, 0xBA13E61C, 0xDCFDAC5D);
static DECLARE_UID (DataExchangeControllerUID, 0xB49E781B, 0xED8F486F, 0x85D4306E, 0x2C6207A0);

//------------------------------------------------------------------------
FUnknown* createDataExchangeProcessor (void*);
FUnknown* createDataExchangeController (void*);

//------------------------------------------------------------------------
static SMTG_CONSTEXPR ParamID ParamIDEnableDataExchange = 1;

//------------------------------------------------------------------------
static SMTG_CONSTEXPR auto MessageIDForceMessageHandling = "ForceMessageHandling";
static SMTG_CONSTEXPR auto MessageKeyValue = "Value";

//------------------------------------------------------------------------
static SMTG_CONSTEXPR DataExchangeUserContextID SampleBufferQueueID = 2;

//------------------------------------------------------------------------
struct SampleBufferExchangeData
{
	int64 systemTime;
	SampleRate sampleRate;
	uint32 numChannels;
	uint32 numSamples;
	float sampleData[1]; // variable size
};

//------------------------------------------------------------------------
inline SampleBufferExchangeData& getSampleBufferExchangeData (const DataExchangeBlock& block)
{
	return *reinterpret_cast<SampleBufferExchangeData*> (block.data);
}

//------------------------------------------------------------------------
inline uint32 calculateExampleDataSizeForSamples (uint32 numSamples, uint32 numChannels)
{
	return sizeof (SampleBufferExchangeData) + ((sizeof (float) * numSamples) * numChannels);
}

//------------------------------------------------------------------------
inline uint32 sampleDataOffsetForChannel (uint32 channel, uint32 numSamplesInBuffer)
{
	return numSamplesInBuffer * channel;
}

//------------------------------------------------------------------------
} // Vst
} // Steinberg
