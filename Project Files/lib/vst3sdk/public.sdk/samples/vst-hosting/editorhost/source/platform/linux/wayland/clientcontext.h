//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/linux/wayland/clientcontext.h
// Created by  : Steinberg 10.2025
// Description : Implementation of WaylandServerDelegate::IWaylandClientContext
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "wayland-server-delegate/iwaylandclientcontext.h"
#include <memory>

namespace Steinberg::Vst::EditorHost::Wayland {

//------------------------------------------------------------------------
// WaylandClientContext
//------------------------------------------------------------------------
class WaylandClientContext final : public WaylandServerDelegate::IWaylandClientContext
{
public:
//------------------------------------------------------------------------
	// WaylandServerDelegate
	using WaylandOutput = WaylandServerDelegate::WaylandOutput;
	using IContextListener = WaylandServerDelegate::IContextListener;

	WaylandClientContext ();
	~WaylandClientContext ();

	bool initWayland (wl_display* display);

	// IWaylandClientContext
	bool addListener (IContextListener* listener) override;
	bool removeListener (IContextListener* listener) override;
	wl_compositor* getCompositor () const override;
	wl_subcompositor* getSubCompositor () const override;
	wl_shm* getSharedMemory () const override;
	wl_seat* getSeat () const override;
	xdg_wm_base* getWindowManager () const override;
	uint32_t getSeatCapabilities () const override;
	const char* getSeatName () const override;
	int countOutputs () const override;
	zwp_linux_dmabuf_v1* getDmaBuffer () const override;
	const WaylandOutput& getOutput (int index) const override;

//------------------------------------------------------------------------
private:
	struct Impl;
	std::unique_ptr<Impl> impl;
};

//------------------------------------------------------------------------
} // namespace Steinberg::Vst::EditorHost::Wayland
