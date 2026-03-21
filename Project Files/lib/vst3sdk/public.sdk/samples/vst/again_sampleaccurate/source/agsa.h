//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again_sampleaccurate/source/agsa.h
// Created by  : Steinberg, 04/2021
// Description : AGain with Sample Accurate Parameter Changes
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
namespace AgainSampleAccurate {

//------------------------------------------------------------------------
static const FUID ProcessorID (0xC18D3C1E, 0x719E4E29, 0x924D3ECA, 0xA5E4DA18);
static const FUID ControllerID (0xC244B7E6, 0x24084E20, 0xA24A8C43, 0xF84C8BE8);

//------------------------------------------------------------------------
FUnknown* createProcessorInstance (void*);
FUnknown* createControllerInstance (void*);

//------------------------------------------------------------------------
enum ParameterID : ParamID
{
	Bypass,
	Gain,
};

//------------------------------------------------------------------------
} // AgainSampleAccurate
} // Vst
} // Steinberg
