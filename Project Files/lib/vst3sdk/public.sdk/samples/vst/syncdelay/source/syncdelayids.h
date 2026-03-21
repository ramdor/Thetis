//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/syncdelay/source/syncdelayids.h
// Created by  : Steinberg, 01/2020
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
enum
{
	kDelayId = 100,
	kBypassId = 101
};

// unique class ids
static const FUID SyncDelayProcessorUID (0x73088025, 0xDFC14D03, 0xACFEE5B5, 0x0D2C4356);
static const FUID SyncDelayControllerUID (0x2B00D28A, 0x0A1F4A87, 0xB65DD9A1, 0xB315804B);

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
