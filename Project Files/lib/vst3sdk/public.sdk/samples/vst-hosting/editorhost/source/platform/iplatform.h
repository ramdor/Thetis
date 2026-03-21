//-----------------------------------------------------------------------------
// Flags       : clang-format auto
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/iplatform.h
// Created by  : Steinberg 09.2016
// Description : Example of opening a plug-in editor
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
#include "public.sdk/samples/vst-hosting/editorhost/source/platform/iwindow.h"
#include <functional>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace EditorHost {

//------------------------------------------------------------------------
class IPlatform
{
public:
	virtual ~IPlatform () noexcept = default;

	virtual void setApplication (ApplicationPtr&& app) = 0;

	virtual WindowPtr createWindow (const std::string& title, Size size, bool resizeable,
	                                const WindowControllerPtr& controller) = 0;
	virtual void quit () = 0;
	virtual void kill (int resultCode, const std::string& reason) = 0;

	virtual FUnknown* getPluginFactoryContext () = 0;

	static IPlatform& instance ();
};

//------------------------------------------------------------------------
} // EditorHost
} // Vst
} // Steinberg

extern void ApplicationInit (int argc, const char* argv[]);
