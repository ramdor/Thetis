//-----------------------------------------------------------------------------
// Flags       : clang-format auto
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/iwindow.h
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

#include "pluginterfaces/gui/iplugview.h"
#include <memory>
#include <string>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace EditorHost {

using Coord = int32_t;

//------------------------------------------------------------------------
struct Point
{
	Coord x;
	Coord y;
};

//------------------------------------------------------------------------
struct Size
{
	Coord width;
	Coord height;
};

//------------------------------------------------------------------------
inline bool operator!= (const Size& lhs, const Size& rhs)
{
	return lhs.width != rhs.width || lhs.height != rhs.height;
}

//------------------------------------------------------------------------
inline bool operator== (const Size& lhs, const Size& rhs)
{
	return lhs.width == rhs.width && lhs.height == rhs.height;
}

//------------------------------------------------------------------------
struct Rect
{
	Point origin;
	Size size;
};

//------------------------------------------------------------------------
inline Rect ViewRectToRect (ViewRect r)
{
	Rect result {};
	result.origin.x = r.left;
	result.origin.y = r.top;
	result.size.width = r.right - r.left;
	result.size.height = r.bottom - r.top;
	return result;
}

//------------------------------------------------------------------------
struct NativePlatformWindow
{
	FIDString type;
	void* ptr;
};

class IWindow;

//------------------------------------------------------------------------
class IWindowController
{
public:
	virtual ~IWindowController () noexcept = default;

	virtual void onShow (IWindow& window) = 0;
	virtual void onClose (IWindow& window) = 0;
	virtual void onResize (IWindow& window, Size newSize) = 0;
	virtual void onContentScaleFactorChanged (IWindow& window, float newScaleFactor) = 0;
	virtual Size constrainSize (IWindow& window, Size requestedSize) = 0;
};

using WindowControllerPtr = std::shared_ptr<IWindowController>;

//------------------------------------------------------------------------
class IWindow
{
public:
	virtual ~IWindow () noexcept = default;

	virtual void show () = 0;
	virtual void close () = 0;
	virtual void resize (Size newSize) = 0;
	virtual Size getContentSize () = 0;

	virtual NativePlatformWindow getNativePlatformWindow () const = 0;

	virtual tresult queryInterface (const TUID iid, void** obj) = 0;
};

using WindowPtr = std::shared_ptr<IWindow>;

//------------------------------------------------------------------------
} // EditorHost
} // Vst
} // Steinberg
