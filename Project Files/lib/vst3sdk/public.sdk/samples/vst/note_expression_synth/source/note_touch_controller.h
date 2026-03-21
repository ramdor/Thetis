//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_synth/source/note_touch_controller.h
// Created by  : Steinberg, 08/2013
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

#include "vstgui/vstgui.h"
#include "vstgui/vstgui_uidescription.h"
#include "pluginterfaces/vst/ivstinterappaudio.h"

namespace VSTGUI {

class NoteTouchController : public IController
{
public:
	NoteTouchController (int32_t pitch, Steinberg::Vst::IInterAppAudioHost* host);

	void setPitch (int32_t _pitch) { pitch = _pitch; }

	void setHorizontalNoteExpType (int32_t type) { xNEType = type; }
	void setVerticalNoteExpType (int32_t type) { yNEType = type; }

protected:
	void controlBeginEdit (CControl* pControl) override;
	void controlEndEdit (CControl* pControl) override;
	void valueChanged (CControl* pControl) override;
	CView* verifyView (CView* view, const UIAttributes& attributes, const IUIDescription* description) override;

	void startNote (float velocity);
	void stopNote (float velocity);
	void sendNoteExpression (int32_t type, float value);
	
	Steinberg::Vst::IInterAppAudioHost* host;
	CXYPad* pad;
	CColor originalPadBackgroundColor;
	int32_t pitch;
	int32_t noteID;
	int32_t xNEType;
	int32_t yNEType;
};

}
