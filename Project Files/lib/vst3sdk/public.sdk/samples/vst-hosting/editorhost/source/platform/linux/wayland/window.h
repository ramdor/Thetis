//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/linux/wayland/window.h
// Created by  : Steinberg 10.2025
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
#include "runloop.h"
#include "pluginterfaces/base/funknownimpl.h"
#include "pluginterfaces/gui/iwaylandframe.h"
#include <functional>
#include <unordered_map>

namespace WaylandServerDelegate {
class IWaylandClientContext;
class WaylandResource;
}

struct wl_compositor;
struct xdg_wm_base;
struct wl_shm;
struct zxdg_decoration_manager_v1;
struct wl_proxy;

namespace Steinberg::Vst::EditorHost::Wayland {

//------------------------------------------------------------------------
// IWaylandProxyCreator, see also: IWaylandServer
//------------------------------------------------------------------------
struct IWaylandProxyCreator
{
	virtual wl_proxy* createProxy (wl_display* display, wl_proxy* object,
	                               WaylandServerDelegate::WaylandResource* implementation) = 0;
	virtual void destroyProxy (wl_proxy* proxy) = 0;

	virtual ~IWaylandProxyCreator () noexcept = default;
};

//------------------------------------------------------------------------
// WaylandWindow
//------------------------------------------------------------------------
class WaylandWindow : public IWindow,
                      private U::ImplementsNonDestroyable<U::Directly<IWaylandFrame>>
{
public:
//------------------------------------------------------------------------
	using Ptr = std::shared_ptr<WaylandWindow>;
	using WindowClosedFunc = std::function<void (WaylandWindow*)>;
	using IWaylandClientContext = WaylandServerDelegate::IWaylandClientContext;
	static Ptr make (const std::string& name, Size size, bool resizeable,
	                 const WindowControllerPtr& controller,
	                 const WindowClosedFunc& windowClosedFunc, IWaylandProxyCreator* proxyCreator,
	                 IPtr<RunLoop> runLoop, IPtr<IWaylandHost> host,
	                 const IWaylandClientContext& waylandContext);

	WaylandWindow (const IWaylandClientContext& waylandContext);
	~WaylandWindow () noexcept override;

//------------------------------------------------------------------------
private:
	void doClose ();

	// IWindow
	void show () override;
	void close () override;
	void resize (Size newSize) override;
	Size getContentSize () override;
	NativePlatformWindow getNativePlatformWindow () const override;
	tresult queryInterface (const TUID iid, void** obj) override;

	// IWaylandFrame
	wl_surface* PLUGIN_API getWaylandSurface (wl_display* display) override;
	xdg_surface* PLUGIN_API getParentSurface (ViewRect& parentSize, wl_display* display) override;
	xdg_toplevel* PLUGIN_API getParentToplevel (wl_display* display) override;

	struct Impl;
	std::unique_ptr<Impl> impl;
};

//------------------------------------------------------------------------
} // Steinberg::Vst::EditorHost::Wayland
