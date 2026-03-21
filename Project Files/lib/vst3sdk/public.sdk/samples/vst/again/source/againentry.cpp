//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again/source/againentry.cpp
// Created by  : Steinberg, 04/2005
// Description : AGain Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "again.h"	// for AGain
#include "againsidechain.h"	// for AGain SideChain
#include "againcontroller.h" // for AGainController
#include "againcids.h"	// for class ids and category
#include "version.h"	// for versioning

#include "public.sdk/source/main/pluginfactory.h"

#define stringPluginName "AGain VST3"
#define stringPluginSideChainName "AGain SideChain VST3"

#if TARGET_OS_IPHONE
#include "public.sdk/source/vst/vstguieditor.h"
extern void* moduleHandle;
#endif

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
	DEF_CLASS2 (INLINE_UID_FROM_FUID(AGainProcessorUID),
				PClassInfo::kManyInstances,	// cardinality
				kVstAudioEffectClass,	// the component category (do not change this)
				stringPluginName,		// here the plug-in name (to be changed)
				Vst::kDistributable,	// means that component and controller could be distributed on different computers
				AGainVST3Category,		// Subcategory for this plug-in (to be changed)
				FULL_VERSION_STR,		// Plug-in version (to be changed)
				kVstVersionString,		// the VST 3 SDK version (do not change this, always use this define)
				Steinberg::Vst::AGain::createInstance)	// function pointer called when this component should be instantiated

	// its kVstComponentControllerClass component
	DEF_CLASS2 (INLINE_UID_FROM_FUID (AGainControllerUID),
				PClassInfo::kManyInstances, // cardinality
				kVstComponentControllerClass,// the Controller category (do not change this)
				stringPluginName "Controller",	// controller name (can be the same as the component name)
				0,						// not used here
				"",						// not used here
				FULL_VERSION_STR,		// Plug-in version (to be changed)
				kVstVersionString,		// the VST 3 SDK version (do not change this, always use this define)
				Steinberg::Vst::AGainController::createInstance)// function pointer called when this component should be instantiated

	//---Second plug-in (AGain with sidechain (only component, use the same controller) included in this factory-------
	DEF_CLASS2 (INLINE_UID_FROM_FUID(AGainWithSideChainProcessorUID),
				PClassInfo::kManyInstances,	// cardinality
				kVstAudioEffectClass,		// the component category (do not change this)
				stringPluginSideChainName,	// here the plug-in name (to be changed)
				Vst::kDistributable,	// means that component and controller could be distributed on different computers
				AGainVST3Category,		// Subcategory for this plug-in (to be changed)
				FULL_VERSION_STR,		// Plug-in version (to be changed)
				kVstVersionString,		// the VST 3 SDK version (do not change this, always use this define)
				Steinberg::Vst::AGainWithSideChain::createInstance)	// function pointer called when this component should be instantiated
	
	//----for others plug-ins contained in this factory, put like for the first plug-in different DEF_CLASS2---

END_FACTORY
