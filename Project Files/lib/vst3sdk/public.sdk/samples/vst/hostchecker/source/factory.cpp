//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/factory.cpp
// Created by  : Steinberg, 04/2012
// Description : 
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "public.sdk/source/main/pluginfactory.h"
#include "hostcheckerprocessor.h"
#include "hostcheckercontroller.h"
#include "cids.h"
#include "version.h"	// for versioning

BEGIN_FACTORY_DEF (stringCompanyName, stringCompanyWeb, stringCompanyEmail)

	//---First plug-in included in this factory-------
	// its kVstAudioEffectClass component
    DEF_CLASS2 (INLINE_UID_FROM_FUID (Steinberg::Vst::HostCheckerProcessorUID),
            PClassInfo::kManyInstances, kVstAudioEffectClass, stringPluginName, Vst::kDistributable,
            "Fx|Instrument", /*"Fx",*/ /*"Spatial|Fx|Instrument|Up-Downmix",*/
            FULL_VERSION_STR, // Plug-in version (to be changed)
            kVstVersionString, Steinberg::Vst::HostCheckerProcessor::createInstance)

    DEF_CLASS2 (INLINE_UID_FROM_FUID (Steinberg::Vst::HostCheckerControllerUID),
            PClassInfo::kManyInstances, kVstComponentControllerClass,
            stringPluginName, // controller name (can be the same as the component name)
            0, // not used here
            "", // not used here
            FULL_VERSION_STR, // Plug-in version (to be changed)
            kVstVersionString, Steinberg::Vst::HostCheckerController::createInstance)

END_FACTORY
