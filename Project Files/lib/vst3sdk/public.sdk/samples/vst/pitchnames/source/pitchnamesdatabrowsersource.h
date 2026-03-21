//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/pitchnames/source/pitchnamesdatabrowsersource.h
// Created by  : Steinberg, 12/2010
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
#include "vstgui/vstgui.h"
#include <string>

namespace VSTGUI {

//-----------------------------------------------------------------------------
class PitchNamesDataBrowserSource : public CBaseObject, public DataBrowserDelegateAdapter
{
public:
	PitchNamesDataBrowserSource (Steinberg::Vst::EditControllerEx1* editController,
	                             Steinberg::Vst::UnitID unitID);
	~PitchNamesDataBrowserSource ();

	int32_t dbGetNumRows (CDataBrowser* browser) SMTG_OVERRIDE;
	int32_t dbGetNumColumns (CDataBrowser* browser) SMTG_OVERRIDE;
	bool dbGetColumnDescription (int32_t index, CCoord& minWidth, CCoord& maxWidth,
	                             CDataBrowser* browser) SMTG_OVERRIDE;
	CCoord dbGetCurrentColumnWidth (int32_t index, CDataBrowser* browser) SMTG_OVERRIDE;
	void dbSetCurrentColumnWidth (int32_t index, const CCoord& width,
	                              CDataBrowser* browser) SMTG_OVERRIDE;
	CCoord dbGetRowHeight (CDataBrowser* browser) SMTG_OVERRIDE;
	bool dbGetLineWidthAndColor (CCoord& width, CColor& color, CDataBrowser* browser) SMTG_OVERRIDE;

	void dbDrawHeader (CDrawContext* context, const CRect& size, int32_t column, int32_t flags,
	                   CDataBrowser* browser) SMTG_OVERRIDE;
	void dbDrawCell (CDrawContext* context, const CRect& size, int32_t row, int32_t column,
	                 int32_t flags, CDataBrowser* browser) SMTG_OVERRIDE;

	CMouseEventResult dbOnMouseDown (const CPoint& where, const CButtonState& buttons, int32_t row,
	                                 int32_t column, CDataBrowser* browser) SMTG_OVERRIDE;
	CMouseEventResult dbOnMouseMoved (const CPoint& where, const CButtonState& buttons, int32_t row,
	                                  int32_t column, CDataBrowser* browser) SMTG_OVERRIDE;
	CMouseEventResult dbOnMouseUp (const CPoint& where, const CButtonState& buttons, int32_t row,
	                               int32_t column, CDataBrowser* browser) SMTG_OVERRIDE;

	void dbSelectionChanged (CDataBrowser* browser) SMTG_OVERRIDE;

	void dbCellTextChanged (int32_t row, int32_t column, UTF8StringPtr newText,
	                        CDataBrowser* browser) SMTG_OVERRIDE;
	void dbCellSetupTextEdit (int32_t row, int32_t column, CTextEdit* textEditControl,
	                          CDataBrowser* browser) SMTG_OVERRIDE;

	int32_t dbOnKeyDown (const VstKeyCode& key, CDataBrowser* browser) SMTG_OVERRIDE;

protected:
	void getPitchName (int16_t pitch, std::string& name);

	Steinberg::Vst::ProgramListWithPitchNames* pitchnames;
};

} // namespace
