//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/programchange/source/plugcontroller.h
// Created by  : Steinberg, 02/2016
// Description : Plug-in Example for VST SDK 3.x using ProgramChange parameter
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
namespace Vst {

//------------------------------------------------------------------------
// PlugController
//------------------------------------------------------------------------
class PlugController: public EditControllerEx1
{
public:
//------------------------------------------------------------------------
// create function required for plug-in factory,
// it will be called to create new instances of this controller
//------------------------------------------------------------------------
	static FUnknown* createInstance (void* /*context*/)
	{
		return (IEditController*)new PlugController;
	}

	//---from IPluginBase--------
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

	//---from EditControllerEx1-----
	tresult PLUGIN_API setParamNormalized (ParamID tag, ParamValue value) SMTG_OVERRIDE;
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getUnitByBus (MediaType /*type*/, BusDirection /*dir*/, int32 /*busIndex*/,
	                                 int32 /*channel*/, UnitID& /*unitId*/ /*out*/) SMTG_OVERRIDE;

	DELEGATE_REFCOUNT (EditControllerEx1)

//------------------------------------------------------------------------

private:

};

}} // namespaces
