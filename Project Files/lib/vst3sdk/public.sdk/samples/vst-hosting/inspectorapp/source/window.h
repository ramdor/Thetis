//-----------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : VST3Inspector
// Filename    : public.sdk/samples/vst-hosting/inspectorapp/window.h
// Created by  : Steinberg, 01/2021
// Description : VST3 Inspector Application
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "vstgui/standalone/include/iwindow.h"

//------------------------------------------------------------------------
namespace VST3Inspector {

//------------------------------------------------------------------------
VSTGUI::Standalone::WindowPtr makeWindow ();

//------------------------------------------------------------------------
} // VST3Inspector
