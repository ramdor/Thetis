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

#pragma once

#include "vstgui/lib/vstguifwd.h"
#include "vstgui/uidescription/icontroller.h"
#include "public.sdk/source/vst/vsteditcontroller.h"
#include "public.sdk/source/vst/vstparameters.h"
#include "base/source/fobject.h"
#include <functional>
#include <vector>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {

using SizeFactors = std::vector<float>;
static const SizeFactors kSizeFactors = {0.75f, 1.f, 1.5f};

class EditorSizeController : public FObject, public VSTGUI::IController
{
public:
//------------------------------------------------------------------------
	using SizeFunc = std::function<void (float)>;
	EditorSizeController (EditController* editController, const SizeFunc& sizeFunc, double currentSizeFactor);
	~EditorSizeController () override;

	static const int32_t kSizeParamTag = 2000;

	void PLUGIN_API update (FUnknown* changedUnknown, int32 message) override;
	VSTGUI::CView* verifyView (VSTGUI::CView* view, const VSTGUI::UIAttributes& attributes,
	                           const VSTGUI::IUIDescription* description) override;
	void valueChanged (VSTGUI::CControl* pControl) override;
	void controlBeginEdit (VSTGUI::CControl* pControl) override;
	void controlEndEdit (VSTGUI::CControl* pControl) override;

	void setSizeFactor (double factor);

	OBJ_METHODS (EditorSizeController, FObject)
//------------------------------------------------------------------------
private:
	VSTGUI::CControl* sizeControl = nullptr;
	RangeParameter* sizeParameter = nullptr;
	SizeFunc sizeFunc;
};

//------------------------------------------------------------------------
} // Vst
} // Steinberg
