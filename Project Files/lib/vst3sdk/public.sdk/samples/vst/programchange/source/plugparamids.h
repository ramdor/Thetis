//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/programchange/source/plugparamids.h
// Created by  : Steinberg, 02/2016
// Description : Plug-in Example for VST SDK 3.x using ProgramChange parameter
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

enum
{
	/** parameter ID */
	kBypassId = 0, ///< Bypass value (we will handle the bypass process) (is automatable)
	kProgramId,

	kGainId,

	kNumProgs = 100
};
