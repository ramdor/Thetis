//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/hostchecker.h
// Created by  : Steinberg, 04/2012
// Description :
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "editorsizecontroller.h"
#include "vstgui/lib/controls/ccontrol.h"

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
EditorSizeController::EditorSizeController (EditController* /*editController*/,
                                            const SizeFunc& sizeFunc, double currentSizeFactor)
: sizeFunc (sizeFunc)
{
	const auto kMaxValue = static_cast<ParamValue> (kSizeFactors.size () - 1);
	sizeParameter = new RangeParameter (STR ("EditorSize"), kSizeParamTag, nullptr, 0, kMaxValue, 1,
	                                    static_cast<int32> (kMaxValue));

	setSizeFactor (currentSizeFactor);

	sizeParameter->addDependent (this);
}

//------------------------------------------------------------------------
EditorSizeController::~EditorSizeController ()
{
	if (sizeParameter)
		sizeParameter->removeDependent (this);
}

//------------------------------------------------------------------------
void PLUGIN_API EditorSizeController::update (FUnknown* changedUnknown, int32 /*message*/)
{
	auto* param = FCast<Parameter> (changedUnknown);
	if (param && param->getInfo ().id == kSizeParamTag)
	{
		auto index = static_cast<size_t> (param->toPlain (param->getNormalized ()));
		if (sizeFunc)
			sizeFunc (kSizeFactors.at (index));
	}
}

//------------------------------------------------------------------------
VSTGUI::CView* EditorSizeController::verifyView (VSTGUI::CView* view,
                                                 const VSTGUI::UIAttributes& /*attributes*/,
                                                 const VSTGUI::IUIDescription* /*description*/)
{
	auto* control = dynamic_cast<VSTGUI::CControl*> (view);
	if (control)
	{
		sizeControl = control;
		sizeControl->setValueNormalized (static_cast<float> (sizeParameter->getNormalized ()));
		sizeControl->setListener (this);
		sizeParameter->deferUpdate ();
	}
	return view;
}

//------------------------------------------------------------------------
void EditorSizeController::valueChanged (VSTGUI::CControl* pControl)
{
	if (!pControl)
		return;

	auto normValue = static_cast<ParamValue> (pControl->getValue ());
	sizeParameter->setNormalized (normValue);
}

//------------------------------------------------------------------------
void EditorSizeController::controlBeginEdit (VSTGUI::CControl* pControl)
{
	if (!pControl)
		return;
}

//------------------------------------------------------------------------
void EditorSizeController::controlEndEdit (VSTGUI::CControl* pControl)
{
	if (!pControl)
		return;
}

//------------------------------------------------------------------------
void EditorSizeController::setSizeFactor (double factor)
{
	if (!sizeParameter)
		return;
	auto iter = std::find (kSizeFactors.begin (), kSizeFactors.end (), factor);
	if (iter != kSizeFactors.end ())
	{
		sizeParameter->setNormalized (
		    sizeParameter->toNormalized (static_cast<ParamValue> (iter - kSizeFactors.begin ())));
		if (sizeControl)
			sizeControl->setValueNormalized (static_cast<float> (sizeParameter->getNormalized ()));
	}
}

//------------------------------------------------------------------------

} // Vst
} // Steinberg
