//------------------------------------------------------------------------
// Copyright(c) 2023 Steinberg Media Technologies.
//------------------------------------------------------------------------

#pragma once

#include "dataexchange.h"
#include "public.sdk/source/vst/vsteditcontroller.h"

namespace Steinberg::Tutorial {

//------------------------------------------------------------------------
//  DataExchangeController
//------------------------------------------------------------------------
class DataExchangeController : public Vst::EditControllerEx1, public Vst::IDataExchangeReceiver
{
public:
//------------------------------------------------------------------------
	// Create function
	static FUnknown* createInstance (void* /*context*/)
	{
		return (Vst::IEditController*)new DataExchangeController;
	}

	// EditController
	tresult PLUGIN_API notify (Vst::IMessage* message) override;

	// IDataExchangeReceiver
	void PLUGIN_API queueOpened (Vst::DataExchangeUserContextID userContextID, uint32 blockSize,
	                             TBool& dispatchOnBackgroundThread) override;
	void PLUGIN_API queueClosed (Vst::DataExchangeUserContextID userContextID) override;
	void PLUGIN_API onDataExchangeBlocksReceived (Vst::DataExchangeUserContextID userContextID,
	                                              uint32 numBlocks, Vst::DataExchangeBlock* blocks,
	                                              TBool onBackgroundThread) override;
	//---Interface---------
	DEFINE_INTERFACES
		// Here you can add more supported VST3 interfaces
		DEF_INTERFACE (Vst::IDataExchangeReceiver)
	END_DEFINE_INTERFACES (EditController)
	DELEGATE_REFCOUNT (EditController)

//------------------------------------------------------------------------
private:
	Vst::DataExchangeReceiverHandler dataExchange {this};
};

//------------------------------------------------------------------------
} // namespace Steinberg::Tutorial
