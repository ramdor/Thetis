//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_text/source/PlugController.cpp
// Created by  : Steinberg, 04/2005
// Description : Plug Controller Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "plugcontroller.h"
#include "plugparamids.h"
#include "vstgui/lib/controls/ctextlabel.h"
#include "vstgui/lib/cstring.h"
#include "public.sdk/source/vst/utility/stringconvert.h"
#include "base/source/fstreamer.h"
#include "pluginterfaces/base/ibstream.h"
#include "pluginterfaces/base/ustring.h"

using namespace VSTGUI;

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
// PlugController Implementation
//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::initialize (FUnknown* context)
{
	mTextLabel = nullptr;

	tresult result = EditControllerEx1::initialize (context);
	if (result != kResultOk)
	{
		return result;
	}

	//---Create Parameters------------

	//---Bypass parameter---
	int32 stepCount = 1;
	ParamValue defaultVal = 0;
	int32 flags = ParameterInfo::kCanAutomate | ParameterInfo::kIsBypass;
	int32 tag = kBypassId;
	parameters.addParameter (STR16 ("Bypass"), nullptr, stepCount, defaultVal, flags, tag);

	return result;
}

//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::setComponentState (IBStream* state)
{
	// we receive the current state of the component (processor part)
	// we read only the gain and bypass value...

	if (!state)
		return kResultFalse;

	IBStreamer streamer (state, kLittleEndian);

	// read the bypass
	int32 savedBypass = 0;
	if (streamer.readInt32 (savedBypass) == false)
		return kResultFalse;

	setParamNormalized (kBypassId, savedBypass > 0 ? 1 : 0);

	return kResultOk;
}

//------------------------------------------------------------------------
IPlugView* PLUGIN_API PlugController::createView (const char* _name)
{
	std::string_view name (_name);
	if (name == ViewType::kEditor)
	{
		return new VST3Editor (this, "Editor", "plug.uidesc");
	}
	return nullptr;
}

//------------------------------------------------------------------------
CView* PlugController::createCustomView (UTF8StringPtr name, const UIAttributes& /*attributes*/,
                                         const IUIDescription* /*description*/,
                                         VST3Editor* /*editor*/)
{
	if (name && strcmp (name, "NoteExpressionText") == 0)
	{
		CRect size;
		mTextLabel = new CTextLabel (size);
		return mTextLabel;
	}
	return nullptr;
}

//------------------------------------------------------------------------
void PlugController::willClose (VST3Editor* /*editor*/)
{
	mTextLabel = nullptr;
}

//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::notify (IMessage* message)
{
	if (!message)
		return kInvalidArgument;

	if (strcmp (message->getMessageID (), "TextMessage") == 0 && mTextLabel)
	{
		TChar string[256] = {0};
		if (message->getAttributes ()->getString ("Text", string,
		                                          sizeof (string) / sizeof (char16)) == kResultOk)
		{
			mTextLabel->setText (Vst::StringConvert::convert (string));
			return kResultOk;
		}
	}

	return kResultFalse;
}

//------------------------------------------------------------------------
tresult PlugController::receiveText (const char8* text)
{
	if (mTextLabel)
		mTextLabel->setText (text);
	return kResultOk;
}

//------------------------------------------------------------------------
int32 PLUGIN_API PlugController::getNoteExpressionCount (int32 busIndex, int16 channel)
{
	if (busIndex == 0 && channel == 0)
		return 2;
	return 0;
}

//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::getNoteExpressionInfo (int32 busIndex, int16 channel,
                                                          int32 noteExpressionIndex,
                                                          NoteExpressionTypeInfo& info /*out*/)
{
	if (busIndex == 0 && channel == 0)
	{
		if (noteExpressionIndex == 0)
		{
			info.typeId = kTextTypeID;
			UString128 ("Lyrics").copyTo (info.title, 128);
			UString128 ("Lyrics").copyTo (info.shortTitle, 128);
			UString128 ("").copyTo (info.units, 128);
			info.unitId = -1;
			info.associatedParameterId = kNoParamId;
			info.flags = 0;
		}
		else if (noteExpressionIndex == 1)
		{
			info.typeId = kTextTypeID;
			UString128 ("Phoneme").copyTo (info.title, 128);
			UString128 ("Phoneme").copyTo (info.shortTitle, 128);
			UString128 ("").copyTo (info.units, 128);
			info.unitId = -1;
			info.associatedParameterId = kNoParamId;
			info.flags = 0;
		}
		return kResultTrue;
	}

	return kResultFalse;
}

//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::getNoteExpressionStringByValue (
    int32 /*busIndex*/, int16 /*channel*/, NoteExpressionTypeID /*id*/,
    NoteExpressionValue /*valueNormalized*/, String128 /*string*/)
{
	return kResultFalse;
}

//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::getNoteExpressionValueByString (
    int32 /*busIndex*/, int16 /*channel*/, NoteExpressionTypeID /*id*/,
    const TChar* /*string*/ /*in*/, NoteExpressionValue& /*valueNormalized*/ /*out*/)
{
	return kResultFalse;
}
}
} // namespaces
