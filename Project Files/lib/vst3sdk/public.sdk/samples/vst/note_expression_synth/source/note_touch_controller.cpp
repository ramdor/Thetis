//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_synth/source/note_touch_controller.cpp
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

#include "note_touch_controller.h"
#include "pluginterfaces/vst/ivstevents.h"
#include "pluginterfaces/vst/ivstnoteexpression.h"

using namespace Steinberg;
using VstEvent = Steinberg::Vst::Event;
using VstEventTypes = Steinberg::Vst::Event::EventTypes;

namespace VSTGUI {

//-----------------------------------------------------------------------------
NoteTouchController::NoteTouchController (int32_t pitch, Steinberg::Vst::IInterAppAudioHost* host)
: host (host), pad (nullptr), pitch (pitch), noteID (-1), xNEType (-1), yNEType (-1)
{
}

//-----------------------------------------------------------------------------
void NoteTouchController::startNote (float velocity)
{
	VstEvent e = {};
	e.type = VstEventTypes::kNoteOnEvent;
	e.noteOn.pitch = pitch;
	e.noteOn.velocity = velocity;
	if (host->scheduleEventFromUI (e) == kResultTrue)
	{
		noteID = e.noteOn.noteId;
		pad->setBackColor (kGreyCColor);
	}
}

//-----------------------------------------------------------------------------
void NoteTouchController::stopNote (float velocity)
{
	if (noteID != -1)
	{
		VstEvent e = {};
		e.type = VstEventTypes::kNoteOffEvent;
		e.noteOff.noteId = noteID;
		e.noteOff.pitch = pitch;
		e.noteOff.velocity = velocity;
		host->scheduleEventFromUI (e);
		noteID = -1;
		pad->setBackColor (originalPadBackgroundColor);
	}
}

//-----------------------------------------------------------------------------
void NoteTouchController::sendNoteExpression (int32_t type, float value)
{
	if (type != -1 && noteID != -1)
	{
		VstEvent e = {};
		e.type = VstEventTypes::kNoteExpressionValueEvent;
		e.noteExpressionValue.noteId = noteID;
		e.noteExpressionValue.typeId = type;
		if (type == Vst::kTuningTypeID)
		{
			value = ((value - 0.5f) * 0.1f + 0.5f);
		}
		else if (type == Vst::kVolumeTypeID)
		{
			value = 0.3f * value;
		}
		e.noteExpressionValue.value = value;
		host->scheduleEventFromUI (e);
	}
}

//-----------------------------------------------------------------------------
void NoteTouchController::controlBeginEdit (CControl* pControl)
{
	if (pControl == pad)
	{
	}
}

//-----------------------------------------------------------------------------
void NoteTouchController::controlEndEdit (CControl* pControl)
{
	if (pControl == pad)
	{
		float x, y;
		CXYPad::calculateXY (pad->getValue (), x, y);
		stopNote (y);
	}
}

//-----------------------------------------------------------------------------
void NoteTouchController::valueChanged (CControl* pControl)
{
	if (pControl == pad)
	{
		float x, y;
		CXYPad::calculateXY (pad->getValue (), x, y);
		if (noteID == -1)
		{
			float velocity = logf (y + 0.4f) + 0.8f;
#if DEBUG_LOG
			FDebugPrint ("%f\n", velocity);
#endif
			startNote (velocity);
		}
		sendNoteExpression (xNEType, x);
		sendNoteExpression (yNEType, y);
	}
}

//-----------------------------------------------------------------------------
CView* NoteTouchController::verifyView (CView* view, const UIAttributes& attributes,
                                        const IUIDescription* description)
{
	if (pad == nullptr)
	{
		pad = dynamic_cast<CXYPad*> (view);
		pad->setListener (this);
		pad->setStopTrackingOnMouseExit (true);
		originalPadBackgroundColor = pad->getBackColor ();
	}
	return view;
}
}
