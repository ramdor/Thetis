//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_synth/source/note_expression_synth_ui.h
// Created by  : Steinberg, 12/2010
// Description : Note Expression Synth UI version using VSTGUI 4
// Flags       : clang-format SMTGSequencer
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "note_expression_synth_controller.h"
#include "note_expression_synth_processor.h"
#include "vstgui/contrib/keyboardview.h"
#include "vstgui/plugin-bindings/vst3editor.h"
#include "pluginterfaces/vst/ivstmidimapping2.h"

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace NoteExpressionSynth {

//-----------------------------------------------------------------------------
/** Example Note Expression Audio Controller + User Interface */
class ControllerWithUI : public Controller,
                         public IMidiLearn,
                         public IMidiLearn2,
                         public VSTGUI::VST3EditorDelegate
{
public:
	using UTF8StringPtr = VSTGUI::UTF8StringPtr;
	using IController = VSTGUI::IController;
	using IUIDescription = VSTGUI::IUIDescription;
	using VST3Editor = VSTGUI::VST3Editor;

	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API terminate () SMTG_OVERRIDE;
	IPlugView* PLUGIN_API createView (FIDString name) SMTG_OVERRIDE;
	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;
	tresult beginEdit (ParamID tag) SMTG_OVERRIDE;
	tresult performEdit (ParamID tag, ParamValue valueNormalized) SMTG_OVERRIDE;
	tresult endEdit (ParamID tag) SMTG_OVERRIDE;

	//--- IMidiLearn ---------------------------------
	tresult PLUGIN_API onLiveMIDIControllerInput (int32 busIndex, int16 channel,
	                                              CtrlNumber midiCC) SMTG_OVERRIDE;

	//--- IMidiLearn2 --------------------------------
	tresult PLUGIN_API onLiveMidi2ControllerInput (BusIndex index, MidiChannel channel,
	                                               Midi2Controller midiCC) SMTG_OVERRIDE;
	tresult PLUGIN_API onLiveMidi1ControllerInput (BusIndex index, MidiChannel channel,
	                                               CtrlNumber midiCC) SMTG_OVERRIDE;

	// VST3EditorDelegate
	IController* createSubController (UTF8StringPtr name, const IUIDescription* description,
	                                  VST3Editor* editor) SMTG_OVERRIDE;
	bool isPrivateParameter (const ParamID paramID) SMTG_OVERRIDE;

	static FUnknown* createInstance (void*) { return (IEditController*)new ControllerWithUI (); }

	static FUID cid;

	DEFINE_INTERFACES
		DEF_INTERFACE (IMidiLearn)
		DEF_INTERFACE (IMidiLearn2)
	END_DEFINE_INTERFACES (Controller)
	REFCOUNT_METHODS (Controller)

private:
	VSTGUI::IKeyboardViewPlayerDelegate* playerDelegate {nullptr};
	VSTGUI::KeyboardViewRangeSelector::Range keyboardRange {};
	static constexpr ParamID InvalidParamID = std::numeric_limits<ParamID>::max ();
	ParamID midiLearnParamID {InvalidParamID};
	bool doMIDILearn {false};

	void removeCurrentMidiLearnParamAssignment ();
};

//-----------------------------------------------------------------------------
/** Example Note Expression Audio Processor + User Interface */
class ProcessorWithUIController : public Processor
{
public:
	ProcessorWithUIController ();

	static FUnknown* createInstance (void*)
	{
		return (IAudioProcessor*)new ProcessorWithUIController ();
	}

	static FUID cid;
};

//------------------------------------------------------------------------
} // NoteExpressionSynth
} // Vst
} // Steinberg
