//-----------------------------------------------------------------------------
// Project     : VST SDK
// Flags       : clang-format SMTGSequencer
// Category    : Examples
// Filename    : public.sdk/samples/vst/dataexchange/factory.cpp
// Created by  : Steinberg, 06/2023
// Description : VST Data Exchange API Example
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "shared.h"
#include "version.h" // for versioning
#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include "pluginterfaces/vst/ivsteditcontroller.h"

//------------------------------------------------------------------------
#include "public.sdk/source/main/pluginfactory_constexpr.h"

#define stringPluginName "DataExchange"

BEGIN_FACTORY_DEF (stringCompanyName, stringCompanyWeb, stringCompanyEmail, 2)

DEF_CLASS (Steinberg::Vst::DataExchangeProcessorUID, //
           Steinberg::PClassInfo::kManyInstances, //
           kVstAudioEffectClass, //
           stringPluginName, //
           Steinberg::Vst::kDistributable, //
           "Fx", //
           FULL_VERSION_STR, //
           kVstVersionString, //
           Steinberg::Vst::createDataExchangeProcessor, //
           nullptr)

DEF_CLASS (Steinberg::Vst::DataExchangeControllerUID, //
           Steinberg::PClassInfo::kManyInstances, //
           kVstComponentControllerClass, //
           stringPluginName "Controller", //
           0, //
           "", //
           FULL_VERSION_STR, //
           kVstVersionString, //
           Steinberg::Vst::createDataExchangeController, //
           nullptr)

END_FACTORY
