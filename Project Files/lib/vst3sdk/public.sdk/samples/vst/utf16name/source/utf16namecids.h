//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/utf16name/source/utf16namecids.h
// Created by  : Steinberg, 11/2023
// Description : UTF16Name Example for VST 3
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
static const Steinberg::FUID kUTF16NameProcessorUID (0x04F0CA5C, 0x1A1650F6, 0x9F2EE4F3, 0xC216D8B5);
static const Steinberg::FUID kUTF16NameControllerUID (0xB6F0EFA6, 0x5EAB57DF, 0x9965FCF8, 0x026EA04D);

#define UTF16NameVST3Category "Fx"

//------------------------------------------------------------------------
} // namespace Steinberg
