//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/adelay/source/adelaycontroller.h
// Created by  : Steinberg, 06/2009
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

#include "public.sdk/source/vst/vsteditcontroller.h"

#if SMTG_OS_MACOS
#include <TargetConditionals.h>
#endif

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
class IDelayTestController : public FUnknown
{
public:
	virtual bool PLUGIN_API doTest () = 0;
//------------------------------------------------------------------------
	static const FUID iid;
};

DECLARE_CLASS_IID (IDelayTestController, 0x9FC98F39, 0x27234512, 0x84FBC4AD, 0x618A14FD)

//-----------------------------------------------------------------------------
class ADelayController : public EditController, public IDelayTestController
{
public:
//------------------------------------------------------------------------
	// create function required for plug-in factory,
	// it will be called to create new instances of this controller
//------------------------------------------------------------------------
	static FUnknown* createInstance (void*) { return (IEditController*)new ADelayController (); }

	//---from IPluginBase--------
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

//---from EditController-----
#if TARGET_OS_IPHONE
	IPlugView* PLUGIN_API createView (FIDString name) SMTG_OVERRIDE;
#endif
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;

	bool PLUGIN_API doTest () SMTG_OVERRIDE;

	//---Interface---------
	OBJ_METHODS (ADelayController, EditController)
	DEFINE_INTERFACES
		DEF_INTERFACE (IDelayTestController)
	END_DEFINE_INTERFACES (EditController)
	REFCOUNT_METHODS (EditController)
};

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
