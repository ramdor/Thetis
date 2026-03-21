//-----------------------------------------------------------------------------
// Flags       : clang-format auto
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/mac/window.h
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

#include "public.sdk/samples/vst-hosting/editorhost/source/platform/iwindow.h"

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace EditorHost {

//------------------------------------------------------------------------
class Window : public IWindow, public std::enable_shared_from_this<Window>
{
public:
	static WindowPtr make (const std::string& name, Size size, bool resizeable,
	                       const WindowControllerPtr& controller);

	Window ();
	~Window () noexcept override;

	bool init (const std::string& name, Size size, bool resizeable,
	           const WindowControllerPtr& controller);

	void show () override;
	void close () override;
	void resize (Size newSize) override;
	Size getContentSize () override;

	NativePlatformWindow getNativePlatformWindow () const override;

	tresult queryInterface (const TUID iid, void** obj) override { return kNoInterface; }

	WindowControllerPtr getController () const;
	void windowClosed ();

private:
	struct Impl;
	std::unique_ptr<Impl> impl;
};

//------------------------------------------------------------------------
} // EditorHost
} // Vst
} // Steinberg
