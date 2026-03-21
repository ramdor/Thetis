//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/syncdelay/source/sync.h
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

#include "pluginterfaces/vst/vsttypes.h"
#include <array>
#include <cassert>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
struct SyncEntry
{
	double value;
	const TChar* title;
};

//------------------------------------------------------------------------
static constexpr std::array<SyncEntry, 18> Synced = {{
    {4.000, STR16 ("1/1")},
    {2.000, STR16 ("1/2")},
    {1.000, STR16 ("1/4")},
    {0.500, STR16 ("1/8")},
    {0.250, STR16 ("1/16")},
    {0.125, STR16 ("1/32")},
    {8.000 / 3., STR16 ("1/1 T")},
    {4.000 / 3., STR16 ("1/2 T")},
    {2.000 / 3., STR16 ("1/4 T")},
    {1.000 / 3., STR16 ("1/8 T")},
    {0.500 / 3., STR16 ("1/16 T")},
    {0.250 / 3., STR16 ("1/32 T")},
    {6.000, STR16 ("1/1 D")},
    {3.000, STR16 ("1/2 D")},
    {1.500, STR16 ("1/4 D")},
    {0.750, STR16 ("1/8 D")},
    {0.375, STR16 ("1/16 D")},
    {0.1875, STR16 ("1/32 D")},
}};

//------------------------------------------------------------------------
} // Vst
} // Steinberg
