//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/utf16name/source/utf16nameentry.cpp
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

#include "utf16nameprocessor.h"
#include "utf16namecontroller.h"
#include "utf16namecids.h"
#include "version.h"

#include "public.sdk/source/main/pluginfactory.h"

// name with UTF characters
#define stringPluginNameU	u"UTF16Name öüäéèê-やあ-مرحبًا"
#define stringCompanyNameU	u"Steinberg Media Technologies - öüäéèê-やあ-مرحبًا"

using namespace Steinberg::Vst;

#define CONCAT(prefix, text) prefix##text
#define U(text) CONCAT(u, text)

//------------------------------------------------------------------------
//  VST Plug-in Entry
//------------------------------------------------------------------------
// Windows: do not forget to include a .def file in your project to export
// GetPluginFactory function!
//------------------------------------------------------------------------

BEGIN_FACTORY_DEF (stringCompanyName, stringCompanyWeb, stringCompanyEmail)

	//---First Plug-in included in this factory-------
	// its kVstAudioEffectClass component
	DEF_CLASS_W2 (INLINE_UID_FROM_FUID(kUTF16NameProcessorUID),
				PClassInfo::kManyInstances,	// cardinality
				kVstAudioEffectClass,	// the component category (do not change this)
				stringPluginNameU,		// here the Plug-in name (to be changed)
				Vst::kDistributable,	// means that component and controller could be distributed on different computers
				UTF16NameVST3Category,	// Subcategory for this Plug-in (to be changed)
				stringCompanyNameU,
				U(FULL_VERSION_STR),	// Plug-in version (to be changed)
				U(kVstVersionString),	// the VST 3 SDK version (do not change this, use always this define)
				UTF16NameProcessor::createInstance)	// function pointer called when this component should be instantiated

	// its kVstComponentControllerClass component
	DEF_CLASS_W2 (INLINE_UID_FROM_FUID (kUTF16NameControllerUID),
				PClassInfo::kManyInstances, // cardinality
				kVstComponentControllerClass,// the Controller category (do not changed this)
				stringPluginNameU,		// controller name (could be the same than component name)
				0,						// not used here
				"",						// not used here
				U(""),					// not used here
				U(FULL_VERSION_STR),	// Plug-in version (to be changed)
				U(kVstVersionString),	// the VST 3 SDK version (do not changed this, use always this define)
				UTF16NameController::createInstance)// function pointer called when this component should be instantiated
END_FACTORY

#define TEXT "toto" 
#define UTEXT u##TEXT