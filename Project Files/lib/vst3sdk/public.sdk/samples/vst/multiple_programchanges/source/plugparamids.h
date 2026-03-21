//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/multiple_programchanges/source/plugparamids.h
// Created by  : Steinberg, 02/2016
// Description : Plug-in Example for VST SDK 3.x using Multiple ProgramChange parameters
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "pluginterfaces/base/ftypes.h"

namespace Steinberg {

enum : int32
{
	kNumSlots = 16,
	kNumProgs = 128,
	kMinProgramCount = 6, // minimum number of programs in a programList
	kMaxProgramCount = 128, // maximum number of programs in a programList

	/** parameter ID */
	kBypassId = 0, ///< Bypass value (we will handle the bypass process) (is automatable)

	kProgramStartId,
	kProgramEndId = kProgramStartId + kNumSlots,

	kGainId = 1000,
	kProgramCountId = 2000
};

//------------------------------------------------------------------------
} // namespace Steinberg