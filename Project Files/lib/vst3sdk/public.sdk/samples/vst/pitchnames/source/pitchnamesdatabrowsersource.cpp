//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/pitchnames/source/pitchnamesdatabrowsersource.cpp
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

#include "pitchnamesdatabrowsersource.h"
#include "public.sdk/source/vst/utility/stringconvert.h"
#include "pluginterfaces/base/ustring.h"

using namespace Steinberg;
using namespace Steinberg::Vst;

namespace VSTGUI {

//-----------------------------------------------------------------------------
PitchNamesDataBrowserSource::PitchNamesDataBrowserSource (EditControllerEx1* editController,
                                                          Steinberg::Vst::UnitID unitID)
: pitchnames (nullptr)
{
	UnitInfo info {0};
	int32_t unitCount = editController->getUnitCount ();
	for (int32_t i = 0; i < unitCount; i++)
	{
		editController->getUnitInfo (i, info);
		if (info.id == unitID)
			break;
	}
	if (info.id == unitID && info.programListId != kNoProgramListId)
	{
		if (editController->hasProgramPitchNames (info.programListId, 0) == kResultTrue)
		{
			pitchnames = FCast<ProgramListWithPitchNames> (
			    editController->getProgramList (info.programListId));
		}
	}
}

//-----------------------------------------------------------------------------
PitchNamesDataBrowserSource::~PitchNamesDataBrowserSource () {}

//-----------------------------------------------------------------------------
int32_t PitchNamesDataBrowserSource::dbGetNumRows (CDataBrowser* /*browser*/)
{
	if (pitchnames)
		return 128;
	return 0;
}

//-----------------------------------------------------------------------------
int32_t PitchNamesDataBrowserSource::dbGetNumColumns (CDataBrowser* /*browser*/) { return 2; }

//-----------------------------------------------------------------------------
bool PitchNamesDataBrowserSource::dbGetColumnDescription (int32_t /*index*/, CCoord& /*minWidth*/,
                                                          CCoord& /*maxWidth*/, CDataBrowser* /*browser*/)
{
	return false;
}

//-----------------------------------------------------------------------------
CCoord PitchNamesDataBrowserSource::dbGetCurrentColumnWidth (int32_t index, CDataBrowser* browser)
{
	const CCoord pitchWidth = 40;
	if (index == 0)
		return pitchWidth;
	return browser->getWidth () - pitchWidth;
}

//-----------------------------------------------------------------------------
void PitchNamesDataBrowserSource::dbSetCurrentColumnWidth (int32_t /*index*/, const CCoord& /*width*/,
                                                           CDataBrowser* /*browser*/)
{
}

//-----------------------------------------------------------------------------
CCoord PitchNamesDataBrowserSource::dbGetRowHeight (CDataBrowser* /*browser*/) { return 18; }

//-----------------------------------------------------------------------------
bool PitchNamesDataBrowserSource::dbGetLineWidthAndColor (CCoord& width, CColor& color,
                                                          CDataBrowser* /*browser*/)
{
	width = 1.;
	color = kGreyCColor;
	return true;
}

//-----------------------------------------------------------------------------
void PitchNamesDataBrowserSource::dbDrawHeader (CDrawContext* context, const CRect& size,
                                                int32_t column, int32_t /*flags*/,
                                                CDataBrowser* /*browser*/)
{
	context->setDrawMode (kAliasing);
	context->setFillColor (kGreyCColor);
	context->drawRect (size, kDrawFilled);

	std::string name;
	switch (column)
	{
		case 0: name = "Note"; break;
		case 1: name = "Pitch Name"; break;
	}
	context->setFont (kNormalFont);
	context->setFontColor (kBlackCColor);
	context->drawString (name.data (), size);
}

//-----------------------------------------------------------------------------
void PitchNamesDataBrowserSource::getPitchName (int16_t pitch, std::string& name)
{
	String128 pitchName;
	if (pitchnames->getPitchName (0, pitch, pitchName) == kResultTrue)
	{
		name = Vst::StringConvert::convert (pitchName);
	}
}

//-----------------------------------------------------------------------------
void PitchNamesDataBrowserSource::dbDrawCell (CDrawContext* context, const CRect& size, int32_t row,
                                              int32_t column, int32_t flags, CDataBrowser* browser)
{
	if (pitchnames == nullptr)
		return;

	if (flags & kRowSelected)
	{
		CFrame* frame = browser->getFrame ();
		CColor focusColor (255, 0, 0, 100);
		if (frame && frame->focusDrawingEnabled ())
			focusColor = frame->getFocusColor ();
		focusColor.alpha /= 2;
		context->setFillColor (focusColor);
		context->drawRect (size, kDrawFilled);
	}

	std::string cellValue;
	switch (column)
	{
		case 0:
		{
			static const char* noteNames[] = {"C",  "C#", "D",  "D#", "E",  "F",
			                                  "F#", "G",  "G#", "A",  "A#", "B"};
			int32_t octave = row / 12;
			cellValue = noteNames[row - octave * 12];
			cellValue += std::to_string (octave - 2);
			break;
		}
		case 1:
		{
			getPitchName ((int16_t)row, cellValue);
			break;
		}
	}

	CRect cellSize (size);
	cellSize.inset (5, 0);
	context->setFont (kNormalFontSmall);
	context->setFontColor (kBlackCColor);
	context->drawString (cellValue.data (), cellSize, kLeftText);
}

//-----------------------------------------------------------------------------
CMouseEventResult PitchNamesDataBrowserSource::dbOnMouseDown (const CPoint& /*where*/,
                                                              const CButtonState& buttons,
                                                              int32_t row, int32_t column,
                                                              CDataBrowser* browser)
{
	if (buttons.isLeftButton () && buttons.isDoubleClick () && column == 1)
	{
		std::string pitchName;
		getPitchName ((int16_t)row, pitchName);
		browser->beginTextEdit (CDataBrowser::Cell (row, column), pitchName.data ());
	}
	return kMouseDownEventHandledButDontNeedMovedOrUpEvents;
}

//-----------------------------------------------------------------------------
CMouseEventResult PitchNamesDataBrowserSource::dbOnMouseMoved (const CPoint& /*where*/,
                                                               const CButtonState& /*buttons*/,
                                                               int32_t /*row*/, int32_t /*column*/,
                                                               CDataBrowser* /*browser*/)
{
	return kMouseEventNotHandled;
}

//-----------------------------------------------------------------------------
CMouseEventResult PitchNamesDataBrowserSource::dbOnMouseUp (const CPoint& /*where*/,
                                                            const CButtonState& /*buttons*/,
                                                            int32_t /*row*/, int32_t /*column*/,
                                                            CDataBrowser* /*browser*/)
{
	return kMouseEventNotHandled;
}

//-----------------------------------------------------------------------------
void PitchNamesDataBrowserSource::dbSelectionChanged (CDataBrowser* /*browser*/) {}

//-----------------------------------------------------------------------------
void PitchNamesDataBrowserSource::dbCellTextChanged (int32_t row, int32_t /*column*/,
                                                     UTF8StringPtr newText, CDataBrowser* /*browser*/)
{
	if (pitchnames)
	{
		UString128 str (newText);
		if (str.getLength () == 0)
			pitchnames->removePitchName (0, (int16)row);
		else
			pitchnames->setPitchName (0, (int16)row, str);
	}
}

//-----------------------------------------------------------------------------
void PitchNamesDataBrowserSource::dbCellSetupTextEdit (int32_t /*row*/, int32_t /*column*/,
                                                       CTextEdit* textEditControl,
                                                       CDataBrowser* /*browser*/)
{
	textEditControl->setBackColor (kWhiteCColor);
	textEditControl->setFont (kNormalFontSmall);
	textEditControl->setFontColor (kRedCColor);
	textEditControl->setTextInset (CPoint (5, 0));
	textEditControl->setHoriAlign (kLeftText);
}

//-----------------------------------------------------------------------------
int32_t PitchNamesDataBrowserSource::dbOnKeyDown (const VstKeyCode& key, CDataBrowser* browser)
{
	if (key.virt == VKEY_RETURN && browser->getSelectedRow () != CDataBrowser::kNoSelection)
	{
		std::string pitchName;
		getPitchName ((int16_t)browser->getSelectedRow (), pitchName);
		browser->beginTextEdit (CDataBrowser::Cell (browser->getSelectedRow (), 1), pitchName.data ());
		return 1;
	}
	return -1;
}

} // namespace
