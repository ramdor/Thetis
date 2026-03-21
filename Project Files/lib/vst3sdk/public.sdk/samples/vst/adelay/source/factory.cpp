//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/adelay/source/factory.cpp
// Created by  : Steinberg, 06/2009
// Description :
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "adelaycontroller.h"
#include "adelayids.h"
#include "adelayprocessor.h"
#include "version.h" // for versioning
#include "public.sdk/source/main/pluginfactory_constexpr.h"
#include "public.sdk/source/vst/utility/testing.h"

#define stringPluginName "ADelay"

BEGIN_FACTORY_DEF (stringCompanyName, stringCompanyWeb, stringCompanyEmail, 3)

DEF_CLASS (Steinberg::Vst::ADelayProcessorUID, Steinberg::PClassInfo::kManyInstances,
           kVstAudioEffectClass, 
           stringPluginName, 
           Steinberg::Vst::kDistributable, 
           "Fx|Delay",
           FULL_VERSION_STR, // Plug-in version (to be changed)
           kVstVersionString, 
           Steinberg::Vst::ADelayProcessor::createInstance, 
           nullptr)

DEF_CLASS (Steinberg::Vst::ADelayControllerUID, Steinberg::PClassInfo::kManyInstances,
           kVstComponentControllerClass,
           stringPluginName "Controller", // controller name (can be the same as the component name)
           0, // not used here
           "", // not used here
           FULL_VERSION_STR, // Plug-in version (to be changed)
           kVstVersionString, 
           Steinberg::Vst::ADelayController::createInstance, 
           nullptr)

// add Test Factory
DEF_CLASS (Steinberg::Vst::TestFactoryUID, 
		   Steinberg::PClassInfo::kManyInstances, 
		   kTestClass,
           stringPluginName "Test Factory", 
           0, 
           "", 
           "", 
           "",
           Steinberg::Vst::createTestFactoryInstance, 
           nullptr)

END_FACTORY
