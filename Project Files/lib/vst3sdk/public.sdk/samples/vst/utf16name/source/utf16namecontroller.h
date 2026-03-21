//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/utf16name/source/utf16namecontroller.h
// Created by  : Steinberg, 11/2023
// Description : UTF16Name Example for VST 3
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

namespace Steinberg {

//------------------------------------------------------------------------
//  UTF16NameController
//------------------------------------------------------------------------
class UTF16NameController : public Vst::EditControllerEx1
{
public:
//------------------------------------------------------------------------
	UTF16NameController () = default;
	~UTF16NameController () SMTG_OVERRIDE = default;

	// Create function
	static FUnknown* createInstance (void* /*context*/)
	{
		return (Vst::IEditController*)new UTF16NameController;
	}

	// IPluginBase
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API terminate () SMTG_OVERRIDE;

	// EditController
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;
	IPlugView* PLUGIN_API createView (FIDString name) SMTG_OVERRIDE;
	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API setParamNormalized (Vst::ParamID tag, Vst::ParamValue value) SMTG_OVERRIDE;
	tresult PLUGIN_API getParamStringByValue (Vst::ParamID tag, Vst::ParamValue valueNormalized,
	                                          Vst::String128 string) SMTG_OVERRIDE;
	tresult PLUGIN_API getParamValueByString (Vst::ParamID tag, Vst::TChar* string,
	                                          Vst::ParamValue& valueNormalized) SMTG_OVERRIDE;

	//---Interface---------
	DEFINE_INTERFACES
	// Here you can add more supported VST3 interfaces
	// DEF_INTERFACE (Vst::IXXX)
	END_DEFINE_INTERFACES (EditControllerEx1)
	DELEGATE_REFCOUNT (EditControllerEx1)

//------------------------------------------------------------------------
protected:
};

//------------------------------------------------------------------------
} // namespace Steinberg
