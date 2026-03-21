//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_text/source/plugentry.cpp
// Created by  : Steinberg, 02/2011
// Description : note_expression_text Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "plug.h"
#include "plugcontroller.h"
#include "plugcids.h"	// for class ids
#include "version.h"	// for versioning

#include "public.sdk/source/main/pluginfactory.h"

#define stringPluginName "Note Expression Text"

using namespace Steinberg;
using namespace Steinberg::Vst;


//------------------------------------------------------------------------
//  VST Plug-in Entry
//------------------------------------------------------------------------
// Windows: do not forget to include a .def file in your project to export
// GetPluginFactory function!
//------------------------------------------------------------------------

BEGIN_FACTORY_DEF (stringCompanyName, stringCompanyWeb, stringCompanyEmail)

	//---First plug-in included in this factory-------
	// its kVstAudioEffectClass component
	DEF_CLASS2 (INLINE_UID_FROM_FUID(PlugProcessorUID),
				PClassInfo::kManyInstances,	// cardinality  
				kVstAudioEffectClass,		// the component category (do not change this)
				stringPluginName,			// here the plug-in name (to be changed)
				Vst::kDistributable,	// means that component and controller could be distributed on different computers
				Vst::PlugType::kFxInstrument,			// Subcategory for this plug-in (to be changed)
				FULL_VERSION_STR,		// Plug-in version (to be changed)
				kVstVersionString,		// the VST 3 SDK version (do not change this, always use this define)
				Steinberg::Vst::Plug::createInstance)	// function pointer called when this component should be instantiated

	// its kVstComponentControllerClass component
	DEF_CLASS2 (INLINE_UID_FROM_FUID (PlugControllerUID),
				PClassInfo::kManyInstances,  // cardinality   
				kVstComponentControllerClass,// the Controller category (do not change this)
				stringPluginName "Controller",	// controller name (can be the same as the component name)
				0,						// not used here
				"",						// not used here
				FULL_VERSION_STR,		// Plug-in version (to be changed)
				kVstVersionString,		// the VST 3 SDK version (do not change this, always use this define)
				Steinberg::Vst::PlugController::createInstance)// function pointer called when this component should be instantiated
END_FACTORY
