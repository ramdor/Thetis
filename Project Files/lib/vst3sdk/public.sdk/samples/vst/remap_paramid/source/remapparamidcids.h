//------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/remap_paramid/source/remapparamidcontroller.h
// Created by  : Steinberg, 02/2024
// Description : Remap ParamID Example for VST 3
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

namespace Steinberg {
//------------------------------------------------------------------------
static const FUID kTestRemapParamIDProcessorUID (0x1012FB81, 0xB92C57E4, 0x85AD3D5D, 0x5FC2469D);
static const FUID kTestRemapParamIDControllerUID (0x0F1CA171, 0xFC7755DC, 0x906DD887, 0x2363F93A);

#define TestRemapParamIDVST3Category "Fx"

enum RemapParamID : Vst::ParamID
{
	kMyGainParamTag = 123,

	kBypassId = 1000
};

//------------------------------------------------------------------------
} // namespace Steinberg
