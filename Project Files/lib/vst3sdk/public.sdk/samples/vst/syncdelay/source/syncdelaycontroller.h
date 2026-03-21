//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/syncdelay/source/syncdelaycontroller.h
// Created by  : Steinberg, 01/2020
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

#include "sync.h"
#include "public.sdk/source/vst/vsteditcontroller.h"

namespace Steinberg {
namespace Vst {

//-----------------------------------------------------------------------------
class SyncDelayController : public EditController
{
public:
//------------------------------------------------------------------------
	// create function required for plug-in factory,
	// it will be called to create new instances of this controller
//------------------------------------------------------------------------
	static FUnknown* createInstance (void*)
	{
		return static_cast<IEditController*> (new SyncDelayController ());
	}

	//---from IPluginBase--------
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

	//---from EditController-----
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;

	//---Interface---------
	OBJ_METHODS (SyncDelayController, EditController)
	REFCOUNT_METHODS (EditController)
};

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
