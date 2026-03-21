//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/remap_paramid/source/remapparamidcontroller.h
// Created by  : Steinberg, 02/2024
// Description : Remap ParamID Example for VST 3
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
#include "pluginterfaces/vst/ivstremapparamid.h"

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
//  TestRemapParamIDController
//------------------------------------------------------------------------
class TestRemapParamIDController : public EditController, public IRemapParamID
{
public:
//------------------------------------------------------------------------
	TestRemapParamIDController () = default;
	~TestRemapParamIDController () SMTG_OVERRIDE = default;

	// Create function
	static FUnknown* createInstance (void* /*context*/)
	{
		return (Vst::IEditController*)new TestRemapParamIDController;
	}

	//--- from IPluginBase -----------------------------------------------
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

	//--- from EditController --------------------------------------------
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;

	//--- from IRemapParamID ---------------------------------------------
	tresult PLUGIN_API getCompatibleParamID (const TUID pluginToReplaceUID /*in*/,
	                                         Vst::ParamID oldParamID /*in*/,
	                                         Vst::ParamID& newParamID /*out*/) override;

	//---Interface---------
	DEFINE_INTERFACES
		DEF_INTERFACE (IRemapParamID)
	END_DEFINE_INTERFACES (EditController)
	DELEGATE_REFCOUNT (EditController)

//------------------------------------------------------------------------
protected:
};

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
