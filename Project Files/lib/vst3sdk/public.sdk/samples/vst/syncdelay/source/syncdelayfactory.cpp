//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/syncdelay/source/syncdelayfactory.cpp
// Created by  : Steinberg, 01/2020
// Description :
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "syncdelaycontroller.h"
#include "syncdelayids.h"
#include "syncdelayprocessor.h"
#include "syncdelayversion.h"
#include "public.sdk/source/main/pluginfactory.h"

#define stringPluginName "SyncDelay"

BEGIN_FACTORY_DEF (stringCompanyName, stringCompanyWeb, stringCompanyEmail)

	//---First plug-in included in this factory-------
	// its kVstAudioEffectClass component
	DEF_CLASS2 (INLINE_UID_FROM_FUID (Steinberg::Vst::SyncDelayProcessorUID),
				PClassInfo::kManyInstances, kVstAudioEffectClass, stringPluginName, Vst::kDistributable,
				"Fx|Delay", FULL_VERSION_STR, kVstVersionString,
				Steinberg::Vst::SyncDelayProcessor::createInstance)

	DEF_CLASS2 (INLINE_UID_FROM_FUID (Steinberg::Vst::SyncDelayControllerUID),
				PClassInfo::kManyInstances, kVstComponentControllerClass, stringPluginName "Controller",
				0, "", FULL_VERSION_STR, kVstVersionString,
				Steinberg::Vst::SyncDelayController::createInstance)
END_FACTORY
