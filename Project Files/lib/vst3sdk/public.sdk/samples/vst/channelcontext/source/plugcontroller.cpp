//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/channelcontext/source/PlugController.cpp
// Created by  : Steinberg, 02/2014
// Description : Plug Controller Example for VST SDK 3.x
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

#include "pluginterfaces/base/ibstream.h"
#include "pluginterfaces/base/ustring.h"
#include "base/source/fstreamer.h"

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
// PlugController Implementation
//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::initialize (FUnknown* context)
{
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

	//---Read only parameters
	String128 undefinedStr;
	Steinberg::UString (undefinedStr, 128).fromAscii ("undefined");

	flags = ParameterInfo::kIsReadOnly;
	auto* strParam = NEW StringListParameter (STR16 ("Ch Uid"), kChannelUIDId, nullptr, flags);
	strParam->appendString (undefinedStr);
	parameters.addParameter (strParam);

	strParam = NEW StringListParameter (STR16 ("Ch Uid Len"), kChannelUIDLengthId, nullptr, flags);
	strParam->appendString (undefinedStr);
	parameters.addParameter (strParam);

	strParam = NEW StringListParameter (STR16 ("Ch Name"), kChannelNameId, nullptr, flags);
	strParam->appendString (undefinedStr);
	parameters.addParameter (strParam);

	strParam = NEW StringListParameter (STR16 ("Ch Name Len"), kChannelNameLengthId, nullptr, flags);
	strParam->appendString (undefinedStr);
	parameters.addParameter (strParam);

	strParam = NEW StringListParameter (STR16 ("Ch Index"), kChannelIndexId, nullptr, flags);
	strParam->appendString (undefinedStr);
	parameters.addParameter (strParam);

	strParam =
		NEW StringListParameter (STR16 ("Ch Index Namespace Order"), kChannelIndexNamespaceOrderId, nullptr, flags);
	strParam->appendString (undefinedStr);
	parameters.addParameter (strParam); 
	
	strParam =
		NEW StringListParameter (STR16 ("Ch Index Namespace"), kChannelIndexNamespaceId, nullptr, flags);
	strParam->appendString (undefinedStr);
	parameters.addParameter (strParam);

	strParam = NEW StringListParameter (STR16 ("Ch Index Namespace Len"),
		kChannelIndexNamespaceLengthId, nullptr, flags);
	strParam->appendString (undefinedStr);
	parameters.addParameter (strParam);

	strParam = NEW StringListParameter (STR16 ("Ch Color"), kChannelColorId, nullptr, flags);
	strParam->appendString (undefinedStr);
	parameters.addParameter (strParam);

	strParam = NEW StringListParameter (STR16 ("Ch Plug Loc."), kChannelPluginLocationId, nullptr, flags);
	strParam->appendString (undefinedStr);
	parameters.addParameter (strParam);

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
	int32 bypassState = 0;
	if (streamer.readInt32 (bypassState) == false)
		return kResultFalse;
	setParamNormalized (kBypassId, bypassState ? 1 : 0);

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API PlugController::setChannelContextInfos (IAttributeList* list)
{
	if (!list)
		return kResultFalse;

	String128 undefinedStr;
	Steinberg::UString (undefinedStr, 128).fromAscii ("undefined");

	// get the channel name length (optional) where we, as plugin, are instantiated
	auto* param =
	    static_cast<StringListParameter*> (parameters.getParameter (kChannelNameLengthId));
	if (param)
	{
		int64 length;
		if (list->getInt (ChannelContext::kChannelNameLengthKey, length) == kResultTrue)
		{
			String128 string128;
			Steinberg::UString (string128, 128).printInt (length);
			param->replaceString (0, string128);
		}
		else
			param->replaceString (0, undefinedStr);
	}

	// get the channel name where we, as plugin, are instantiated
	param = static_cast<StringListParameter*> (parameters.getParameter (kChannelNameId));
	if (param)
	{
		String128 name;
		if (list->getString (ChannelContext::kChannelNameKey, name, sizeof (name)) == kResultTrue)
			param->replaceString (0, name);
		else
			param->replaceString (0, undefinedStr);
	}

	// get the channel UID Length
	param = static_cast<StringListParameter*> (parameters.getParameter (kChannelUIDLengthId));
	if (param)
	{
		int64 length;
		if (list->getInt (ChannelContext::kChannelUIDLengthKey, length) == kResultTrue)
		{
			String128 string128;
			Steinberg::UString (string128, 128).printInt (length);
			param->replaceString (0, string128);
		}
		else
			param->replaceString (0, undefinedStr);
	}

	// get the channel UID
	param = static_cast<StringListParameter*> (parameters.getParameter (kChannelUIDId));
	if (param)
	{
		String128 name;
		if (list->getString (ChannelContext::kChannelUIDKey, name, sizeof (name)) == kResultTrue)
			param->replaceString (0, name);
		else
			param->replaceString (0, undefinedStr);
	}

	// get Channel Index
	param = static_cast<StringListParameter*> (parameters.getParameter (kChannelIndexId));
	if (param)
	{
		int64 index;
		if (list->getInt (ChannelContext::kChannelIndexKey, index) == kResultTrue)
		{
			String128 string128;
			Steinberg::UString (string128, 128).printInt (index);
			param->replaceString (0, string128);
		}
		else
			param->replaceString (0, undefinedStr);
	}

	// get Channel Index Namespace Order
	param = static_cast<StringListParameter*> (parameters.getParameter (kChannelIndexNamespaceOrderId));
	if (param)
	{
		int64 index;
		if (list->getInt (ChannelContext::kChannelIndexNamespaceOrderKey, index) == kResultTrue)
		{
			String128 string128;
			Steinberg::UString (string128, 128).printInt (index);
			param->replaceString (0, string128);
		}
		else
			param->replaceString (0, undefinedStr);
	}

	// get the channel Index Namespace Length
	param = static_cast<StringListParameter*> (
		parameters.getParameter (kChannelIndexNamespaceLengthId));
	if (param)
	{
		int64 length;
		if (list->getInt (ChannelContext::kChannelIndexNamespaceLengthKey, length) == kResultTrue)
		{
			String128 string128;
			Steinberg::UString (string128, 128).printInt (length);
			param->replaceString (0, string128);
		}
		else
			param->replaceString (0, undefinedStr);
	}

	// get the channel Index Namespace
	param = static_cast<StringListParameter*> (parameters.getParameter (kChannelIndexNamespaceId));
	if (param)
	{
		String128 name;
		if (list->getString (ChannelContext::kChannelIndexNamespaceKey, name, sizeof (name)) ==
			kResultTrue)
			param->replaceString (0, name);
		else
			param->replaceString (0, undefinedStr);
	}

	// get plug-in Channel Location
	param = static_cast<StringListParameter*> (parameters.getParameter (kChannelPluginLocationId));
	if (param)
	{
		int64 location;
		if (list->getInt (ChannelContext::kChannelPluginLocationKey, location) == kResultTrue)
		{
			String128 string128;
			switch (location)
			{
				case ChannelContext::kPreVolumeFader:
					Steinberg::UString (string128, 128).fromAscii ("PreVolFader");
					break;
				case ChannelContext::kPostVolumeFader:
					Steinberg::UString (string128, 128).fromAscii ("PostVolFader");
					break;
				case ChannelContext::kUsedAsPanner:
					Steinberg::UString (string128, 128).fromAscii ("UsedAsPanner");
					break;
				default: Steinberg::UString (string128, 128).fromAscii ("unknown!"); break;
			}
			param->replaceString (0, string128);
		}
		else
			param->replaceString (0, undefinedStr);
	}

	// get Channel Color
	param = static_cast<StringListParameter*> (parameters.getParameter (kChannelColorId));
	if (param)
	{
		int64 color;
		if (list->getInt (ChannelContext::kChannelColorKey, color) == kResultTrue)
		{
			uint32 channelColor = (uint32)color;
			char str[10];
			snprintf (str, 10, "%x%x%x%x", ChannelContext::GetAlpha (channelColor),
			          ChannelContext::GetRed (channelColor),
			          ChannelContext::GetGreen (channelColor),
			          ChannelContext::GetBlue (channelColor));
			String128 string128;
			Steinberg::UString (string128, 128).fromAscii (str);
			param->replaceString (0, string128);
		}
		else
			param->replaceString (0, undefinedStr);
	}

	// we have to inform the host that our strings have changed (values not)
	if (componentHandler)
		componentHandler->restartComponent (kParamValuesChanged);

	return kResultTrue;
}
}
} // namespaces
