//------------------------------------------------------------------------
// Copyright(c) 2023 Steinberg Media Technologies.
//------------------------------------------------------------------------

#include "cids.h"
#include "controller.h"

namespace Steinberg::Tutorial {

//------------------------------------------------------------------------
// DataExchangeController Implementation
//------------------------------------------------------------------------
tresult PLUGIN_API DataExchangeController::notify (Vst::IMessage* message)
{
	if (dataExchange.onMessage (message))
		return kResultTrue;
	return EditControllerEx1::notify (message);
}

//------------------------------------------------------------------------
void PLUGIN_API DataExchangeController::queueOpened (Vst::DataExchangeUserContextID userContextID,
                                                     uint32 blockSize,
                                                     TBool& dispatchOnBackgroundThread)
{
	FDebugPrint ("Data Exchange Queue opened.\n");
}

//------------------------------------------------------------------------
void PLUGIN_API DataExchangeController::queueClosed (Vst::DataExchangeUserContextID userContextID)
{
	FDebugPrint ("Data Exchange Queue closed.\n");
}

//------------------------------------------------------------------------
void PLUGIN_API DataExchangeController::onDataExchangeBlocksReceived (
    Vst::DataExchangeUserContextID userContextID, uint32 numBlocks, Vst::DataExchangeBlock* blocks,
    TBool onBackgroundThread)
{
	for (auto index = 0u; index < numBlocks; ++index)
	{
		auto dataBlock = toDataBlock (blocks[index]);
		FDebugPrint (
		    "Received Data Block: SampleRate: %d, SampleSize: %d, NumChannels: %d, NumSamples: %d\n",
		    dataBlock->sampleRate, static_cast<uint32_t> (dataBlock->sampleSize),
		    static_cast<uint32_t> (dataBlock->numChannels),
		    static_cast<uint32_t> (dataBlock->numSamples));
	}
}

//------------------------------------------------------------------------
} // namespace Steinberg::Tutorial
