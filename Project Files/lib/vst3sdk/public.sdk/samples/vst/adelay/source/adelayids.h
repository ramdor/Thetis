//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/adelay/source/adelayids.h
// Created by  : Steinberg, 06/2009
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

namespace Steinberg {
namespace Vst {

// parameter tags
enum {
	kDelayId = 100,
	kBypassId = 101
};

// unique class ids
static DECLARE_UID (ADelayProcessorUID, 0x0CDBB669, 0x85D548A9, 0xBFD83719, 0x09D24BB3);
static DECLARE_UID (ADelayControllerUID, 0x038E7FA9, 0x629A4EAA, 0x8541B889, 0x18E8952C);

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
