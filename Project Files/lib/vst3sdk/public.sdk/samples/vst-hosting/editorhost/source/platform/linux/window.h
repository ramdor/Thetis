//-----------------------------------------------------------------------------
// Flags       : clang-format auto
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/linux/window.h
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
#include <functional>

struct _XDisplay;
typedef struct _XDisplay Display;

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace EditorHost {

//------------------------------------------------------------------------
class X11Window : public IWindow
{
public:
	using Ptr = std::shared_ptr<X11Window>;
	using WindowClosedFunc = std::function<void (X11Window*)>;

	static Ptr make (const std::string& name, Size size, bool resizeable,
	                 const WindowControllerPtr& controller, Display* display,
	                 const WindowClosedFunc& windowClosedFunc);

	X11Window ();
	~X11Window () override;

	bool init (const std::string& name, Size size, bool resizeable,
	           const WindowControllerPtr& controller, Display* display,
	           const WindowClosedFunc& windowClosedFunc);

	void show () override;
	void close () override;
	void resize (Size newSize) override;
	Size getContentSize () override;

	NativePlatformWindow getNativePlatformWindow () const override;
	tresult queryInterface (const TUID iid, void** obj) override;

	void onIdle ();

private:
	Size getSize () const;

	struct Impl;
	std::unique_ptr<Impl> impl;
};

//------------------------------------------------------------------------
} // EditorHost
} // Vst
} // Steinberg
