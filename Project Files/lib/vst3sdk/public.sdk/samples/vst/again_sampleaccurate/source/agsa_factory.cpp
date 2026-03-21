//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again_sampleaccurate/source/agsa_factory.cpp
// Created by  : Steinberg, 04/2021
// Description : AGain with Sample Accurate Parameter Changes
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "agsa.h"
#include "tutorial.h"
#include "version.h"
#include "public.sdk/source/main/pluginfactory.h"
#include "public.sdk/source/vst/utility/testing.h"
#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include "pluginterfaces/vst/ivsteditcontroller.h"

#define stringPluginName "AGain Sample Accurate"

using namespace Steinberg;
using namespace Steinberg::Vst;
using namespace Steinberg::Vst::AgainSampleAccurate;

//------------------------------------------------------------------------
//  VST Plug-in Factory
//------------------------------------------------------------------------

BEGIN_FACTORY_DEF (stringCompanyName, stringCompanyWeb, stringCompanyEmail)

// AGain sample accurate
DEF_CLASS2 (INLINE_UID_FROM_FUID (ProcessorID), PClassInfo::kManyInstances, kVstAudioEffectClass,
            stringPluginName, Vst::kDistributable, "Fx", FULL_VERSION_STR, kVstVersionString,
            createProcessorInstance)

DEF_CLASS2 (INLINE_UID_FROM_FUID (ControllerID), PClassInfo::kManyInstances,
            kVstComponentControllerClass, stringPluginName "Controller", 0, "", FULL_VERSION_STR,
            kVstVersionString, createControllerInstance)

// Test
DEF_CLASS2 (INLINE_UID_FROM_FUID (getTestFactoryUID ()), PClassInfo::kManyInstances, kTestClass,
            stringPluginName "Test Factory", 0, "", "", "", createTestFactoryInstance)

// Tutorial
DEF_CLASS2 (INLINE_UID_FROM_FUID (Tutorial::ProcessorID), PClassInfo::kManyInstances,
            kVstAudioEffectClass, "Advanced Tutorial", Vst::kDistributable, "Fx", FULL_VERSION_STR,
            kVstVersionString, Tutorial::createProcessorInstance)

DEF_CLASS2 (INLINE_UID_FROM_FUID (Tutorial::ControllerID), PClassInfo::kManyInstances,
            kVstComponentControllerClass, "Advanced Tutorial Controller", 0, "", FULL_VERSION_STR,
            kVstVersionString, Tutorial::createControllerInstance)

END_FACTORY
