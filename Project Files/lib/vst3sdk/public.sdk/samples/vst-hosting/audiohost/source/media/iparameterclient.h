//-----------------------------------------------------------------------------
// Flags       : clang-format auto
// Project     : VST SDK
//
// Category    : AudioHost
// Filename    : public.sdk/samples/vst-hosting/audiohost/source/media/iparameterclient.h
// Created by  : Steinberg 09.2016
// Description : Audio Host Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include <memory>
#include <pluginterfaces/vst/vsttypes.h>

//----------------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {

//----------------------------------------------------------------------------------
struct IParameterClient
{
	virtual void setParameter (ParamID id, ParamValue value, int32 sampleOffset) = 0;

	virtual ~IParameterClient () {}
};
//----------------------------------------------------------------------------------
using IParameterClientPtr = std::weak_ptr<IParameterClient>;

//----------------------------------------------------------------------------------
} // Vst
} // Steinberg
