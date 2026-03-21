//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_text/source/plugcontroller.h
// Created by  : Steinberg, 04/2005
// Description : Note Expression Editor Example for VST 3
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
#include "pluginterfaces/vst/ivstnoteexpression.h"
#include "vstgui/plugin-bindings/vst3editor.h"

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
// PlugController
//------------------------------------------------------------------------
class PlugController : public EditControllerEx1,
                       public VSTGUI::VST3EditorDelegate,
                       public INoteExpressionController
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

	using CView = VSTGUI::CView;
	using CTextEdit = VSTGUI::CTextEdit;
	using UTF8StringPtr = VSTGUI::UTF8StringPtr;
	using UIAttributes = VSTGUI::UIAttributes;
	using IUIDescription = VSTGUI::IUIDescription;
	using VST3Editor = VSTGUI::VST3Editor;
	using CTextLabel = VSTGUI::CTextLabel;

	//---from IPluginBase--------
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	
	//---from EditController-----
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;

	IPlugView* PLUGIN_API createView (const char* name) SMTG_OVERRIDE;

	//---from VST3EditorDelegate----
	CView* createCustomView (UTF8StringPtr name, const UIAttributes& attributes,
	                         const IUIDescription* description, VST3Editor* editor) SMTG_OVERRIDE;
	void willClose (VST3Editor* editor) SMTG_OVERRIDE;

	tresult PLUGIN_API notify (IMessage* message) SMTG_OVERRIDE;
	tresult receiveText (const char8* text) SMTG_OVERRIDE;

	//---from INoteExpressionController--------
	int32 PLUGIN_API getNoteExpressionCount (int32 busIndex, int16 channel) SMTG_OVERRIDE;
	tresult PLUGIN_API getNoteExpressionInfo (int32 busIndex, int16 channel, int32 noteExpressionIndex, NoteExpressionTypeInfo& info /*out*/) SMTG_OVERRIDE;
	tresult PLUGIN_API getNoteExpressionStringByValue (int32 busIndex, int16 channel, NoteExpressionTypeID id, NoteExpressionValue valueNormalized /*in*/, String128 string /*out*/) SMTG_OVERRIDE;
	tresult PLUGIN_API getNoteExpressionValueByString (int32 busIndex, int16 channel, NoteExpressionTypeID id, const TChar* string /*in*/, NoteExpressionValue& valueNormalized /*out*/) SMTG_OVERRIDE;

	//---Interface---------
	OBJ_METHODS (PlugController, EditControllerEx1)
	DEFINE_INTERFACES
		DEF_INTERFACE (INoteExpressionController)
	END_DEFINE_INTERFACES (EditControllerEx1)
	REFCOUNT_METHODS(EditControllerEx1)

	//------------------------------------------------------------------------

private:
	CTextLabel* mTextLabel;
};

}} // namespaces
