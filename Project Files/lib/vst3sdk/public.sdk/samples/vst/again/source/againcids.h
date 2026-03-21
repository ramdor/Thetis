//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again/source/againcids.h
// Created by  : Steinberg, 12/2007
// Description : define the class IDs for AGain
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

// Here are defined the UIDs for the 2 processors (2 plug-ins) and 1 controller (shared by the 2 plug-ins) 
static const FUID AGainProcessorUID (0x84E8DE5F, 0x92554F53, 0x96FAE413, 0x3C935A18);
static const FUID AGainWithSideChainProcessorUID (0x41347FD6, 0xFED64094, 0xAFBB12B7, 0xDBA1D441);

static const FUID AGainControllerUID (0xD39D5B65, 0xD7AF42FA, 0x843F4AC8, 0x41EB04F0);

#define AGainVST3Category "Fx"

//------------------------------------------------------------------------
} // namespace Vst
} // namespace Steinberg
