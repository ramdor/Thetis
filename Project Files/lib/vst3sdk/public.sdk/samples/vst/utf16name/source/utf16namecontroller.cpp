//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/utf16name/source/utf16namecontroller.cpp
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

#include "utf16namecontroller.h"
#include "utf16namecids.h"
#include "pluginterfaces/base/ustring.h"
#include "pluginterfaces/vst/ivstunits.h"

namespace Steinberg {

//------------------------------------------------------------------------
// UTF16NameController Implementation
//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameController::initialize (FUnknown* context)
{
	// Here the Plug-in will be instantiated

	//---do not forget to call parent ------
	tresult result = EditControllerEx1::initialize (context);
	if (result != kResultOk)
	{
		return result;
	}

	// Here you could register some parameters

	//--- Create Units-------------
	Vst::UnitInfo unitInfo {};
	unitInfo.id = 1;
	unitInfo.parentUnitId = Vst::kRootUnitId; // attached to the root unit
	unitInfo.programListId = Vst::kNoProgramListId;

	// English
	UString (unitInfo.name, str16BufferSize (Vst::String128)).assign (STR16 ("Folder 1"));
	auto unit = new Vst::Unit (unitInfo);
	addUnit (unit);

	// Japanese
	unitInfo.id++;
	UString (unitInfo.name, str16BufferSize (Vst::String128)).assign (STR16 ("フォルダー2"));
	unit = new Vst::Unit (unitInfo);
	addUnit (unit);

	// Korean
	unitInfo.id++;
	UString (unitInfo.name, str16BufferSize (Vst::String128)).assign (STR16 ("폴더 3"));
	unit = new Vst::Unit (unitInfo);
	addUnit (unit);

	// Arabic
	unitInfo.id++;
	UString (unitInfo.name, str16BufferSize (Vst::String128)).assign (STR16 ("المجلد 4"));
	unit = new Vst::Unit (unitInfo);
	addUnit (unit);

	// Persian
	unitInfo.id++;
	UString (unitInfo.name, str16BufferSize (Vst::String128)).assign (STR16 ("پوشه 5"));
	unit = new Vst::Unit (unitInfo);
	addUnit (unit);

	//---Create Parameters------------
	int32 stepCount = 0;
	Vst::ParamValue defaultVal = 0;
	int32 flags = Vst::ParameterInfo::kNoFlags;
	int32 tag = 100;
	Vst::UnitID unitId = 1;
	
	// English
	parameters.addParameter (STR16 ("Hello"), nullptr, stepCount, defaultVal, flags, tag, unitId);
	
	// Japanese
	tag++;
	unitId++;
	parameters.addParameter (STR16 ("こんにちは"), nullptr, stepCount, defaultVal, flags, tag, unitId);
	
	// Korean
	tag++;
	unitId++;
	parameters.addParameter (STR16 ("안녕하세요"), nullptr, stepCount, defaultVal, flags, tag, unitId);
	
	flags = Vst::ParameterInfo::kCanAutomate;

	// Arabic
	tag++;
	unitId++;
	parameters.addParameter (STR16 ("مرحبا"), nullptr, stepCount, defaultVal, flags, tag, unitId);
	
	// Persian
	tag++;
	unitId++;
	parameters.addParameter (STR16 ("سلام"), nullptr, stepCount, defaultVal, flags, tag, unitId);

	return result;
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameController::terminate ()
{
	// Here the Plug-in will be de-instantiated, last possibility to remove some memory!

	//---do not forget to call parent ------
	return EditControllerEx1::terminate ();
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameController::setComponentState (IBStream* state)
{
	// Here you get the state of the component (Processor part)
	if (!state)
		return kResultFalse;

	return kResultOk;
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameController::setState (IBStream* state)
{
	// Here you get the state of the controller

	return kResultTrue;
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameController::getState (IBStream* state)
{
	// Here you are asked to deliver the state of the controller (if needed)
	// Note: the real state of your plug-in is saved in the processor

	return kResultTrue;
}

//------------------------------------------------------------------------
IPlugView* PLUGIN_API UTF16NameController::createView (FIDString name)
{
	// Here the Host wants to open your editor (if you have one)
	if (FIDStringsEqual (name, Vst::ViewType::kEditor))
	{
		// create your editor here and return a IPlugView ptr of it
		return nullptr;
	}
	return nullptr;
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameController::setParamNormalized (Vst::ParamID tag, Vst::ParamValue value)
{
	// called by host to update your parameters
	tresult result = EditControllerEx1::setParamNormalized (tag, value);
	return result;
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameController::getParamStringByValue (Vst::ParamID tag,
                                                               Vst::ParamValue valueNormalized,
                                                               Vst::String128 string)
{
	// called by host to get a string for given normalized value of a specific parameter
	// (without having to set the value!)
	return EditControllerEx1::getParamStringByValue (tag, valueNormalized, string);
}

//------------------------------------------------------------------------
tresult PLUGIN_API UTF16NameController::getParamValueByString (Vst::ParamID tag, Vst::TChar* string,
                                                               Vst::ParamValue& valueNormalized)
{
	// called by host to get a normalized value from a string representation of a specific parameter
	// (without having to set the value!)
	return EditControllerEx1::getParamValueByString (tag, string, valueNormalized);
}

//------------------------------------------------------------------------
} // namespace Steinberg
