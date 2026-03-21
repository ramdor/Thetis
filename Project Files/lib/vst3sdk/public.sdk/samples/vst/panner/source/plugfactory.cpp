//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/panner/source/plugfactory.cpp
// Created by  : Steinberg, 02/2020
// Description : Panner Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "public.sdk/source/main/pluginfactory.h"

#include "../include/plugcontroller.h"	// for createInstance
#include "../include/plugprocessor.h"	// for createInstance
#include "../include/plugids.h"			// for uids
#include "../include/version.h"			// for version and naming

#define stringSubCategory Vst::PlugType::kSpatialFx	// Subcategory for this plug-in (to be changed if needed, see PlugType in ivstaudioprocessor.h)

BEGIN_FACTORY_DEF (stringCompanyName, stringCompanyWeb,	stringCompanyEmail)

	DEF_CLASS2 (INLINE_UID_FROM_FUID(Steinberg::Panner::MyProcessorUID),
				PClassInfo::kManyInstances,	// cardinality  
				kVstAudioEffectClass,	// the component category (do not change this)
				stringPluginName,		// here the plug-in name (to be changed)
				Vst::kDistributable,	// means that component and controller could be distributed on different computers
				stringSubCategory,		// Subcategory for this plug-in (to be changed)
				FULL_VERSION_STR,		// Plug-in version (to be changed)
				kVstVersionString,		// the VST 3 SDK version (do not change this, always use this define)
				Steinberg::Panner::PlugProcessor::createInstance)	// function pointer called when this component should be instantiated

	DEF_CLASS2 (INLINE_UID_FROM_FUID(Steinberg::Panner::MyControllerUID),
				PClassInfo::kManyInstances,  // cardinality   
				kVstComponentControllerClass,// the Controller category (do not change this)
				stringPluginName "Controller",	// controller name (can be the same as the component name)
				0,						// not used here
				"",						// not used here
				FULL_VERSION_STR,		// Plug-in version (to be changed)
				kVstVersionString,		// the VST 3 SDK version (do not change this, always use this define)
				Steinberg::Panner::PlugController::createInstance)// function pointer called when this component should be instantiated

END_FACTORY

