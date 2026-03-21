//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/pitchnames/source/pitchnames.h
// Created by  : Steinberg, 12/2010
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
#include "public.sdk/source/vst/vstaudioeffect.h"
#include "base/source/fstring.h"
#include "vstgui/plugin-bindings/vst3editor.h"

namespace Steinberg {
namespace Vst {

//-----------------------------------------------------------------------------
class PitchNamesController : public EditControllerEx1, public VSTGUI::VST3EditorDelegate
{
public:
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;

	tresult PLUGIN_API getUnitByBus (MediaType type, BusDirection dir, int32 busIndex,
	                                 int32 channel, UnitID& unitId /*out*/) SMTG_OVERRIDE;

	IPlugView* PLUGIN_API createView (FIDString name) SMTG_OVERRIDE;

	VSTGUI::CView* createCustomView (VSTGUI::UTF8StringPtr name,
	                                 const VSTGUI::UIAttributes& attributes,
	                                 const VSTGUI::IUIDescription* description,
	                                 VSTGUI::VST3Editor* editor) SMTG_OVERRIDE;

	static FUnknown* createInstance (void*)
	{
		return (IEditController*)new PitchNamesController ();
	}

	static FUID cid;
};

//-----------------------------------------------------------------------------
class PitchNamesProcessor : public AudioEffect
{
public:
	PitchNamesProcessor ();

	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API process (ProcessData& data) SMTG_OVERRIDE;

	static FUnknown* createInstance (void*) { return (IAudioProcessor*)new PitchNamesProcessor (); }
	static FUID cid;
};
}
} // namespaces
