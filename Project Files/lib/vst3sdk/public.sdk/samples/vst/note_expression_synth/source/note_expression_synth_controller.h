//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_synth/source/note_expression_synth_controller.h
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

#include "public.sdk/source/vst/vsteditcontroller.h"
#include "public.sdk/source/vst/vstnoteexpressiontypes.h"
#include "pluginterfaces/vst/ivstmidicontrollers.h"
#include "pluginterfaces/vst/ivstmidilearn.h"
#include "pluginterfaces/vst/ivstmidimapping2.h"
#include "pluginterfaces/vst/ivstnoteexpression.h"
#include "pluginterfaces/vst/ivstphysicalui.h"

#include <array>
#include <limits>
#include <variant>

#define MAX_VOICES 64
#define MAX_RELEASE_TIME_SEC 5.0
#define NUM_FILTER_TYPE 3
#define NUM_TUNING_RANGE 2

namespace Steinberg {
namespace Vst {
namespace NoteExpressionSynth {

//-----------------------------------------------------------------------------
// Global Parameters
//-----------------------------------------------------------------------------
enum
{
	kParamReleaseTime,
	kParamNoiseVolume,
	kParamSinusVolume,
	kParamTriangleVolume,
	kParamSinusDetune,
	kParamBypassSNA,
	kParamTriangleSlop,
	kParamFilterType,
	kParamFilterFreq,
	kParamFilterQ,
	kParamMasterVolume,
	kParamMasterTuning,
	kParamVelToLevel,
	kParamFilterFreqModDepth,
	kParamTuningRange,
	kParamActiveVoices,
	kParamSquareVolume,

	kNumGlobalParameters
};

//-----------------------------------------------------------------------------
/** Example Note Expression Edit Controller

\sa Steinberg::Vst::INoteExpressionController
\sa Steinberg::Vst::NoteExpressionTypeContainer
\sa Steinberg::Vst::NoteExpressionType
*/
class Controller : public EditController,
                   public INoteExpressionController,
                   public IMidiMapping,
                   public IMidiMapping2,
                   public INoteExpressionPhysicalUIMapping
{
public:
	//--- EditController -----------------------------
	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API terminate () SMTG_OVERRIDE;
	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API setParamNormalized (ParamID tag, ParamValue value) SMTG_OVERRIDE;

	//--- IMidiMapping -------------------------------
	tresult PLUGIN_API getMidiControllerAssignment (int32 busIndex, int16 channel,
	                                                CtrlNumber midiControllerNumber,
	                                                ParamID& id /*out*/) SMTG_OVERRIDE;

	//--- IMidiMapping2 ------------------------------
	uint32 PLUGIN_API getNumMidi2ControllerAssignments (BusDirections direction) SMTG_OVERRIDE;
	tresult PLUGIN_API getMidi2ControllerAssignments (
	    BusDirections direction, const Midi2ControllerParamIDAssignmentList& list) SMTG_OVERRIDE;
	uint32 PLUGIN_API getNumMidi1ControllerAssignments (BusDirections direction) SMTG_OVERRIDE;
	tresult PLUGIN_API getMidi1ControllerAssignments (
	    BusDirections direction, const Midi1ControllerParamIDAssignmentList& list) SMTG_OVERRIDE;

	//--- INoteExpressionController ------------------
	int32 PLUGIN_API getNoteExpressionCount (int32 busIndex, int16 channel) SMTG_OVERRIDE;
	tresult PLUGIN_API getNoteExpressionInfo (int32 busIndex, int16 channel,
	                                          int32 noteExpressionIndex,
	                                          NoteExpressionTypeInfo& info /*out*/) SMTG_OVERRIDE;
	tresult PLUGIN_API getNoteExpressionStringByValue (int32 busIndex, int16 channel,
	                                                   NoteExpressionTypeID id,
	                                                   NoteExpressionValue valueNormalized /*in*/,
	                                                   String128 string /*out*/) SMTG_OVERRIDE;
	tresult PLUGIN_API getNoteExpressionValueByString (
	    int32 busIndex, int16 channel, NoteExpressionTypeID id, const TChar* string /*in*/,
	    NoteExpressionValue& valueNormalized /*out*/) SMTG_OVERRIDE;

	enum NoteExpressionTypeIds
	{
		kNoiseVolumeTypeID = kCustomStart,
		kFilterFreqModTypeID,
		kFilterQModTypeID,
		kSinusVolumeTypeID,
		kTriangleVolumeTypeID,
		kFilterTypeTypeID,
		kTriangleSlopeTypeID,
		kSinusDetuneTypeID,
		kReleaseTimeModTypeID,
		kTextInputTypeID,
		kSquareVolumeTypeID,
	};

	//--- INoteExpressionPhysicalUIMapping ------------
	tresult PLUGIN_API getPhysicalUIMapping (int32 busIndex, int16 channel,
	                                         PhysicalUIMapList& list) SMTG_OVERRIDE;

	//--- ---------------------------------------------
	static FUnknown* createInstance (void*) { return (IEditController*)new Controller (); }

	static FUID cid;

	OBJ_METHODS (Controller, EditController)
	DEFINE_INTERFACES
		DEF_INTERFACE (INoteExpressionController)
		DEF_INTERFACE (IMidiMapping)
		DEF_INTERFACE (IMidiMapping2)
		DEF_INTERFACE (INoteExpressionPhysicalUIMapping)
	END_DEFINE_INTERFACES (EditController)
	REFCOUNT_METHODS (EditController)

protected:
	NoteExpressionTypeContainer noteExpressionTypes;

	enum class CCType : uint8
	{
		CC, // CCIndex = Vst::ControllerNumbers
		RPN, // CCIndex = [0, 16383] / 14 Bit like in MIDI Spec
		NRPN // CCIndex = [0, 16383] / 14 Bit like in MIDI Spec
	};
	using CCIndex = uint16; // see CCType
	using CCKey = std::pair<CCType, CCIndex>;
	using CCMap = std::map<CCKey, ParamID>;
	CCMap midiCCMapping;

	//--- IMidiMapping2 Helpers ----------------------
	uint32 getNumMidiControllerAssignments (BusDirections direction, bool midi1);
	tresult getMidiControllerAssignments (
	    BusDirections direction,
	    std::variant<Midi2ControllerParamIDAssignmentList, Midi1ControllerParamIDAssignmentList>
	        list);
	bool hasMatchingCCType (bool midi1, CCType type) const;
};

//------------------------------------------------------------------------
static constexpr auto MsgIDEvent = "Event";

//------------------------------------------------------------------------
} // NoteExpressionSynth
} // Vst
} // Steinberg
