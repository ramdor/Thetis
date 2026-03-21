//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again/source/againparamids.h
// Created by  : Steinberg, 12/2007
// Description : define the parameter IDs used by AGain
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
	kGainId = 0,	///< for the gain value (is automatable)
	kVuPPMId,		///< for the Vu value return to host (ReadOnly parameter for our UI)
	kBypassId		///< Bypass value (we will handle the bypass process) (is automatable)
};
