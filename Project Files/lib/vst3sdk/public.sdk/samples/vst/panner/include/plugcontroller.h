///------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/panner/include/plugcontroller.h
// Created by  : Steinberg, 02/2020
// Description : Panner Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "vstgui/plugin-bindings/vst3editor.h"
#include "public.sdk/source/vst/vsteditcontroller.h"
#include "pluginterfaces/vst/ivstparameterfunctionname.h"

namespace Steinberg {
namespace Panner {

//-----------------------------------------------------------------------------
class PlugController : public Vst::EditController,
                       public VSTGUI::VST3EditorDelegate,
                       public Vst::IParameterFunctionName
{
public:
//------------------------------------------------------------------------
	// create function required for plug-in factory,
	// it will be called to create new instances of this controller
//------------------------------------------------------------------------
	static FUnknown* createInstance (void*)
	{
		return (Vst::IEditController*)new PlugController ();
	}

	//---from IPluginBase--------
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

	//---from EditController-----
	IPlugView* PLUGIN_API createView (const char* name) SMTG_OVERRIDE;
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;

	//---from IParameterFunctionName----
	tresult PLUGIN_API getParameterIDFromFunctionName (Vst::UnitID unitID, FIDString functionName,
	                                                   Vst::ParamID& paramID) override;

	OBJ_METHODS (PlugController, Vst::EditController)
	DEFINE_INTERFACES
		DEF_INTERFACE (Vst::IParameterFunctionName)
	END_DEFINE_INTERFACES (Vst::EditController)
	DELEGATE_REFCOUNT (Vst::EditController)
};

//------------------------------------------------------------------------
} // namespace Panner
} // namespace Steinberg
