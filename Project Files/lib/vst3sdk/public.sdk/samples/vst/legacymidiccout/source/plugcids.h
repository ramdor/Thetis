//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/legacymidiccout/source/plugcids.h
// Created by  : Steinberg, 11/2018
// Description : Plug-in Example for VST SDK 3.x using Legacy MIDI CC
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

namespace Steinberg {
namespace Vst {
namespace LegacyMIDICCOut {
static const FUID PlugProcessorUID  (0x5DDBB0D4, 0xF9284622, 0x90FE5A5E, 0x1414905A);
static const FUID PlugControllerUID (0x9F34E8E9, 0x22FC42E6, 0x993C6007, 0x642512C9);
}
}
} // namespaces
