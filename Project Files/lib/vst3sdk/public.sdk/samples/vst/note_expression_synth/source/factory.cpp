//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_synth/source/factory.cpp
// Created by  : Steinberg, 02/2010
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
#include "note_expression_synth_processor.h"
#include "note_expression_synth_controller.h"
#include "note_expression_synth_ui.h"
#include "version.h"	// for versioning

#if TARGET_OS_IPHONE
#include "public.sdk/source/vst/vstguieditor.h"
extern void* moduleHandle;
#endif

#define stringPluginName "Note Expression Synth"


BEGIN_FACTORY_DEF (stringCompanyName, stringCompanyWeb, stringCompanyEmail)

	//---First plug-in included in this factory-------
	// its kVstAudioEffectClass component
	DEF_CLASS2 (INLINE_UID_FROM_FUID(Steinberg::Vst::NoteExpressionSynth::ProcessorWithUIController::cid),
				PClassInfo::kManyInstances,
				kVstAudioEffectClass,
				stringPluginName " With UI",
				Vst::kDistributable,
				Vst::PlugType::kInstrumentSynth,
				FULL_VERSION_STR,
				kVstVersionString,
				Steinberg::Vst::NoteExpressionSynth::ProcessorWithUIController::createInstance)

	DEF_CLASS2 (INLINE_UID_FROM_FUID(Steinberg::Vst::NoteExpressionSynth::ControllerWithUI::cid),
				PClassInfo::kManyInstances,
				kVstComponentControllerClass,
				stringPluginName " With UI",
				0,						// not used here
				"",						// not used here
				FULL_VERSION_STR,
				kVstVersionString,
				Steinberg::Vst::NoteExpressionSynth::ControllerWithUI::createInstance)

	DEF_CLASS2 (INLINE_UID_FROM_FUID(Steinberg::Vst::NoteExpressionSynth::Processor::cid),
				PClassInfo::kManyInstances,
				kVstAudioEffectClass,
				stringPluginName,
				Vst::kDistributable,
				Vst::PlugType::kInstrumentSynth,
				FULL_VERSION_STR,
				kVstVersionString,
				Steinberg::Vst::NoteExpressionSynth::Processor::createInstance)

	DEF_CLASS2 (INLINE_UID_FROM_FUID(Steinberg::Vst::NoteExpressionSynth::Controller::cid),
				PClassInfo::kManyInstances,
				kVstComponentControllerClass,
				stringPluginName,
				0,						// not used here
				"",						// not used here
				FULL_VERSION_STR,
				kVstVersionString,
				Steinberg::Vst::NoteExpressionSynth::Controller::createInstance)

END_FACTORY
