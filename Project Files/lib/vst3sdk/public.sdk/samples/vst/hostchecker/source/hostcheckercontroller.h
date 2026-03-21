//-----------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/hostchecker/source/hostcheckercontroller.h
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

#pragma once

#include "eventlogdatabrowsersource.h"
#include "hostcheck.h"
#include "logevents.h"

#include "vstgui/lib/cvstguitimer.h"
#include "vstgui/plugin-bindings/vst3editor.h"

#include "public.sdk/source/common/threadchecker.h"
#include "public.sdk/source/vst/utility/dataexchange.h"
#include "public.sdk/source/vst/vstaudioeffect.h"
#include "public.sdk/source/vst/vsteditcontroller.h"

#include "base/source/fstring.h"

#include "pluginterfaces/vst/ivstautomationstate.h"
#include "pluginterfaces/vst/ivstchannelcontextinfo.h"
#include "pluginterfaces/vst/ivstmidilearn.h"
#include "pluginterfaces/vst/ivstnoteexpression.h"
#include "pluginterfaces/vst/ivstparameterfunctionname.h"
#include "pluginterfaces/vst/ivstphysicalui.h"
#include "pluginterfaces/vst/ivstprefetchablesupport.h"
#include "pluginterfaces/vst/ivstremapparamid.h"
#include "pluginterfaces/vst/ivstrepresentation.h"

namespace Steinberg {

namespace HostChecker {
const double kMaxLatencyInSeconds = 10.; // this is quite big for a latency (max used by Cubase)
const uint32 kParamWarnCount = 8;
const uint32 kParamWarnBitCount = 24;
const uint32 kParamWarnStepCount = 1 << kParamWarnBitCount;

const uint32 kParamUnitStruct1Count = 4;
const uint32 kParamUnitStruct2Count = 4;
const uint32 kParamUnitStruct3Count = 2;
const uint32 kParamUnitStructCount =
    2 * (kParamUnitStruct1Count * kParamUnitStruct2Count * kParamUnitStruct3Count + 1);
}

namespace Vst {

enum
{
	// for Parameters
	kProcessingLoadTag = 1000,
	kGeneratePeaksTag,
	kLatencyTag,
	kBypassTag,
	kCanResizeTag,
	kScoreTag,
	kParamWhichCouldBeHiddenTag,
	kTriggerHiddenTag,
	kTriggerProgressTag,
	kProgressValueTag,
	kCopy2ClipboardTag,
	kRestartNoteExpressionChangedTag,
	kRestartKeyswitchChangedTag,
	kRestartParamValuesChangedTag,
	kRestartParamTitlesChangedTag,

	kProcessContextProjectTimeSamplesTag,
	kProcessContextProjectTimeMusicTag,
	kProcessContextTempoTag,
	kProcessContextStateTag,
	kProcessContextSystemTimeTag,
	kProcessContextContinousTimeSamplesTag,
	kProcessContextTimeSigNumeratorTag,
	kProcessContextTimeSigDenominatorTag,
	kProcessContextBarPositionMusicTag,

	kParamLowLatencyTag,
	kParamRandomizeTag,
	kParamProcessModeTag,

	kProcessWarnTag,
	kLastTag = kProcessWarnTag + HostChecker::kParamWarnCount,

	kParamUnitStructStart,
	kParamUnitStructEnd = kParamUnitStructStart + HostChecker::kParamUnitStructCount,

	// for Units
	kUnitId = 1234,
	kUnit2Id = 1235,
	kUnitParamIdStart = 2345,
};

class EditorSizeController;

//-----------------------------------------------------------------------------
class HostCheckerController : public EditControllerEx1,
                              public VSTGUI::VST3EditorDelegate,
                              public ChannelContext::IInfoListener,
                              public IXmlRepresentationController,
                              public IAutomationState,
                              public IEditControllerHostEditing,
                              public IMidiMapping,
                              public IMidiLearn,
                              public INoteExpressionController,
                              public INoteExpressionPhysicalUIMapping,
                              public IKeyswitchController,
                              public IParameterFunctionName,
                              public IDataExchangeReceiver,
                              public IRemapParamID
{
public:
	using UTF8StringPtr = VSTGUI::UTF8StringPtr;
	using IController = VSTGUI::IController;
	using IUIDescription = VSTGUI::IUIDescription;
	using VST3Editor = VSTGUI::VST3Editor;

	HostCheckerController ();

	tresult PLUGIN_API initialize (FUnknown* context) SMTG_OVERRIDE;
	tresult PLUGIN_API terminate () SMTG_OVERRIDE;

	tresult PLUGIN_API setComponentState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getUnitByBus (MediaType type, BusDirection dir, int32 busIndex,
	                                 int32 channel, UnitID& unitId /*out*/) SMTG_OVERRIDE;
	tresult PLUGIN_API setComponentHandler (IComponentHandler* handler) SMTG_OVERRIDE;
	int32 PLUGIN_API getUnitCount () SMTG_OVERRIDE;
	tresult PLUGIN_API setParamNormalized (ParamID tag, ParamValue value) SMTG_OVERRIDE;

	tresult beginEdit (ParamID tag) SMTG_OVERRIDE;
	tresult endEdit (ParamID tag) SMTG_OVERRIDE;

	IPlugView* PLUGIN_API createView (FIDString name) SMTG_OVERRIDE;
	tresult PLUGIN_API notify (IMessage* message) SMTG_OVERRIDE;
	tresult PLUGIN_API connect (IConnectionPoint* other) SMTG_OVERRIDE;

	//---from VST3EditorDelegate --------------------
	VSTGUI::CView* createCustomView (VSTGUI::UTF8StringPtr name,
	                                 const VSTGUI::UIAttributes& attributes,
	                                 const VSTGUI::IUIDescription* description,
	                                 VSTGUI::VST3Editor* editor) SMTG_OVERRIDE;
	void willClose (VSTGUI::VST3Editor* editor) SMTG_OVERRIDE;

	//---from IEditController2-------
	tresult PLUGIN_API setKnobMode (KnobMode mode) SMTG_OVERRIDE;
	tresult PLUGIN_API openHelp (TBool /*onlyCheck*/) SMTG_OVERRIDE;
	tresult PLUGIN_API openAboutBox (TBool /*onlyCheck*/) SMTG_OVERRIDE;

	tresult PLUGIN_API setState (IBStream* state) SMTG_OVERRIDE;
	tresult PLUGIN_API getState (IBStream* state) SMTG_OVERRIDE;

	//---ChannelContext::IInfoListener-------
	tresult PLUGIN_API setChannelContextInfos (IAttributeList* list) SMTG_OVERRIDE;

	//---IXmlRepresentationController--------
	tresult PLUGIN_API getXmlRepresentationStream (RepresentationInfo& info /*in*/,
	                                               IBStream* stream /*out*/) SMTG_OVERRIDE;

	//---IMidiMapping---------------------------
	tresult PLUGIN_API getMidiControllerAssignment (int32 busIndex, int16 channel,
	                                                CtrlNumber midiControllerNumber,
	                                                ParamID& id /*out*/) SMTG_OVERRIDE;

	//---IMidiLearn-----------------------------
	tresult PLUGIN_API onLiveMIDIControllerInput (int32 busIndex, int16 channel,
	                                              CtrlNumber midiCC) SMTG_OVERRIDE;

	//---INoteExpressionController----------------------
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

	//---INoteExpressionPhysicalUIMapping-----------------
	tresult PLUGIN_API getPhysicalUIMapping (int32 busIndex, int16 channel,
	                                         PhysicalUIMapList& list) SMTG_OVERRIDE;

	//--- IKeyswitchController ---------------------------
	int32 PLUGIN_API getKeyswitchCount (int32 busIndex, int16 channel) SMTG_OVERRIDE;
	tresult PLUGIN_API getKeyswitchInfo (int32 busIndex, int16 channel, int32 keySwitchIndex,
	                                     KeyswitchInfo& info /*out*/) SMTG_OVERRIDE;

	//---IAutomationState---------------------------------
	tresult PLUGIN_API setAutomationState (int32 state) SMTG_OVERRIDE;

	//---IEditControllerHostEditing-----------------------
	tresult PLUGIN_API beginEditFromHost (ParamID paramID) SMTG_OVERRIDE;
	tresult PLUGIN_API endEditFromHost (ParamID paramID) SMTG_OVERRIDE;

	//---IParameterFunctionName---------------------------
	tresult PLUGIN_API getParameterIDFromFunctionName (UnitID unitID, FIDString functionName,
	                                                   ParamID& paramID) SMTG_OVERRIDE;

	//---IDataExchangeReceiver----------------------------
	void PLUGIN_API queueOpened (DataExchangeUserContextID userContextID, uint32 blockSize,
	                             TBool& dispatchOnBackgroundThread) override;
	void PLUGIN_API queueClosed (DataExchangeUserContextID userContextID) override;
	void PLUGIN_API onDataExchangeBlocksReceived (DataExchangeUserContextID userContextID,
	                                              uint32 numBlocks, DataExchangeBlock* block,
	                                              TBool onBackgroundThread) override;

	//---IRemapParamID -----------------------------------
	tresult PLUGIN_API getCompatibleParamID (const TUID pluginToReplaceUID /*in*/,
	                                         Vst::ParamID oldParamID /*in*/,
	                                         Vst::ParamID& newParamID /*out*/) override;

	//--- --------------------------------------------------------------------------
	void editorAttached (EditorView* editor) SMTG_OVERRIDE;
	void editorRemoved (EditorView* editor) SMTG_OVERRIDE;
	void editorDestroyed (EditorView* editor) SMTG_OVERRIDE;

	IController* createSubController (UTF8StringPtr name, const IUIDescription* description,
	                                  VST3Editor* editor) override;

	tresult PLUGIN_API queryInterface (const Steinberg::TUID iid, void** obj) override;

	REFCOUNT_METHODS (EditControllerEx1)

	static FUnknown* createInstance (void*)
	{
		return (IEditController*)new HostCheckerController ();
	}

	void addFeatureLog (int64 iD, int32 count = 1, bool addToLastCount = true);
	bool getSavedSize (ViewRect& size) const
	{
		if (sizeFactor <= 0)
			return false;
		ViewRect rect (0, 0, width, height);
		size = rect;
		return true;
	}

protected:
	void extractCurrentInfo (EditorView* editor);
	float updateScoring (int64 iD);
	void onProgressTimer (VSTGUI::CVSTGUITimer*);

	std::map<VSTGUI::VST3Editor*, VSTGUI::SharedPointer<VSTGUI::CDataBrowser>> mDataBrowserMap;
	VSTGUI::SharedPointer<VSTGUI::EventLogDataBrowserSource> mDataSource;

	bool mLatencyInEdit {false};
	ParamValue mWantedLatency {0.0};

	using EditorVector = std::vector<Steinberg::Vst::EditorView*>;
	EditorVector editors;

	using EditorMap = std::map<Steinberg::Vst::EditorView*, EditorSizeController*>;
	EditorMap editorsSubCtlerMap;

	uint32 width {0};
	uint32 height {0};
	double sizeFactor {0};

	using EditFromHostMap = std::map<Steinberg::Vst::ParamID, int32>;
	EditFromHostMap mEditFromHost;

	std::unique_ptr<ThreadChecker> threadChecker {ThreadChecker::create ()};

	int32 mNumKeyswitch {1};

	DataExchangeReceiverHandler dataExchange {this};

	struct ScoreEntry
	{
		ScoreEntry (float factor = 1.f) : factor (factor) {}
		float factor {1.f};
		bool use {false};
	};

	using ScoreMap = std::map<int64, ScoreEntry>;
	ScoreMap mScoreMap;

	VSTGUI::CVSTGUITimer* mProgressTimer {nullptr};
	IProgress::ID mProgressID;
	bool mInProgress {false};
};

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
