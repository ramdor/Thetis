//------------------------------------------------------------------------
// Flags       : clang-format auto
// Project     : VST SDK
//
// Category    : AudioHost
// Filename    : public.sdk/samples/vst-hosting/audiohost/source/platform/appinit.h
// Created by  : Steinberg, 04/2005
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

#include "public.sdk/samples/vst-hosting/editorhost/source/platform/iapplication.h"

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace AudioHost {

//------------------------------------------------------------------------
struct AppInit
{
	explicit AppInit (EditorHost::ApplicationPtr&& _app) { app = std::move (_app); }

	EditorHost::ApplicationPtr app;
};

//------------------------------------------------------------------------
} // EditorHost
} // Vst
} // Steinberg
