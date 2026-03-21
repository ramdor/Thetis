//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again_sampleaccurate/source/tutorial.h
// Created by  : Steinberg, 04/2021
// Description : Tutorial
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "pluginterfaces/base/funknown.h"
#include "pluginterfaces/vst/vsttypes.h"

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace Tutorial {

//------------------------------------------------------------------------
static const FUID ProcessorID (0xCC48BF25, 0x529043DA, 0x80223510, 0xFFE8BD02);
static const FUID ControllerID (0x3A89B2B2, 0x4F474E02, 0x9C96EE27, 0x0AD2A15B);

//------------------------------------------------------------------------
FUnknown* createProcessorInstance (void*);
FUnknown* createControllerInstance (void*);

//------------------------------------------------------------------------
} // AgainSampleAccurate
} // Vst
} // Steinberg
