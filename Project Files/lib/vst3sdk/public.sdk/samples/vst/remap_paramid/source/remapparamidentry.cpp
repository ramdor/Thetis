//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/remap_paramid/source/remapparamidcontroller.h
// Created by  : Steinberg, 02/2024
// Description : Remap ParamID Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "remapparamidcids.h"
#include "remapparamidcontroller.h"
#include "remapparamidprocessor.h"
#include "version.h"

#include "public.sdk/source/main/pluginfactory.h"

#define stringPluginName "Test Remap ParamID"

using namespace Steinberg;

//------------------------------------------------------------------------
//  VST Plug-in Entry
//------------------------------------------------------------------------

BEGIN_FACTORY_DEF ("Steinberg", "https://www.mycompanyname.com", "mailto:test@test.fr")

//---First Plug-in included in this factory-------
// its kVstAudioEffectClass component
DEF_CLASS2 (
    INLINE_UID_FROM_FUID (kTestRemapParamIDProcessorUID),
    PClassInfo::kManyInstances, // cardinality
    kVstAudioEffectClass, // the component category (do not changed this)
    stringPluginName, // here the Plug-in name (to be changed)
    Vst::kDistributable, // means that component and controller could be distributed on different
                         // computers
    TestRemapParamIDVST3Category, // Subcategory for this Plug-in (to be changed)
    FULL_VERSION_STR, // Plug-in version (to be changed)
    kVstVersionString, // the VST 3 SDK version (do not changed this, use always this define)
    Vst::TestRemapParamIDProcessor::createInstance) // function pointer called when this component
                                                    // should be instantiated

// its kVstComponentControllerClass component
DEF_CLASS2 (
    INLINE_UID_FROM_FUID (kTestRemapParamIDControllerUID),
    PClassInfo::kManyInstances, // cardinality
    kVstComponentControllerClass, // the Controller category (do not changed this)
    stringPluginName "Controller", // controller name (could be the same than component name)
    0, // not used here
    "", // not used here
    FULL_VERSION_STR, // Plug-in version (to be changed)
    kVstVersionString, // the VST 3 SDK version (do not changed this, use always this define)
    Vst::TestRemapParamIDController::createInstance) // function pointer called when this component
                                                     // should be instantiated

//----for others Plug-ins contained in this factory, put like for the first Plug-in different
//DEF_CLASS2---

END_FACTORY
