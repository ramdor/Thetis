//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_synth/source/note_expression_synth_processor.h
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

#pragma once

#include "public.sdk/source/vst/vstaudioeffect.h"
#include "public.sdk/source/vst/utility/ringbuffer.h"
#include "note_expression_synth_voice.h"

namespace Steinberg {
namespace Vst {
class VoiceProcessor;

namespace NoteExpressionSynth {

//-----------------------------------------------------------------------------
/** Example Note Expression Audio Processor

\sa Steinberg::Vst::VoiceProcessor
\sa Steinberg::Vst::VoiceBase
*/
class Processor : public AudioEffect
{
public:
	Processor ();
	
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API setBusArrangements (SpeakerArrangement* inputs, int32 numIns, SpeakerArrangement* outputs, int32 numOuts) SMTG_OVERRIDE;

	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;

	tresult PLUGIN_API canProcessSampleSize (int32 symbolicSampleSize) SMTG_OVERRIDE;
	tresult PLUGIN_API setActive (TBool state) SMTG_OVERRIDE;
	tresult PLUGIN_API process (ProcessData& data) SMTG_OVERRIDE;

	tresult PLUGIN_API notify (IMessage* message) SMTG_OVERRIDE;

	static FUnknown* createInstance (void*) { return (IAudioProcessor*)new Processor (); }

	static FUID cid;
protected:
	VoiceProcessor* voiceProcessor;
	GlobalParameterState paramState;
	OneReaderOneWriter::RingBuffer<Event> controllerEvents {16};
};

}}} // namespaces
