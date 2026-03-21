//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/channelcontext/source/plugcontroller.h
// Created by  : Steinberg, 02/2014
// Description : channelcontext Controller Example for VST SDK 3.x
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "public.sdk/source/vst/vsteditcontroller.h"
#include "pluginterfaces/vst/ivstchannelcontextinfo.h"

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
// PlugController
//------------------------------------------------------------------------
class PlugController : public EditControllerEx1, public ChannelContext::IInfoListener
{
public:
//------------------------------------------------------------------------
	// create function required for plug-in factory,
	// it will be called to create new instances of this controller
//------------------------------------------------------------------------
	static FUnknown* createInstance (void* /*context*/) { return (IEditController*)new PlugController; }

	//---from IPluginBase--------
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

	//---from EditController-----
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;

	//---from ChannelContext::IInfoListener-----
	tresult PLUGIN_API setChannelContextInfos (IAttributeList* list) SMTG_OVERRIDE;

	//---Interface---------
	OBJ_METHODS (PlugController, EditControllerEx1)
	DEFINE_INTERFACES
		DEF_INTERFACE (ChannelContext::IInfoListener)
	END_DEFINE_INTERFACES (EditController)
	DELEGATE_REFCOUNT (EditControllerEx1)

//------------------------------------------------------------------------

private:
};
}
} // namespaces
