//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/panner/include/plugids.h
// Created by  : Steinberg, 02/2020
// Description : Panner Example for VST 3
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
namespace Panner {

// HERE are defined the parameter Ids which are exported to the host
enum PannerParams : Vst::ParamID
{
	kBypassId = 100,

	kParamPanId = 102,
};


// HERE you have to define new unique class ids: for processor and for controller
// you can use GUID creator tools like https://www.guidgenerator.com/
static const FUID MyProcessorUID (0xA2EAF7DB, 0x320640F4, 0x8EDE380D, 0xDF89562C);
static const FUID MyControllerUID (0x239F80C2, 0x4F1442C4, 0x8B58AE6E, 0x7C8644EB);

//------------------------------------------------------------------------
} // namespace Panner
} // namespace Steinberg
