//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/linux/wayland/runloop.h
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

#include "pluginterfaces/base/funknownimpl.h"
#include "pluginterfaces/gui/iplugview.h"
#include <memory>

struct wl_display;
struct wl_event_loop;

namespace Steinberg::Vst::EditorHost::Wayland {

//------------------------------------------------------------------------
// RunLoop
//------------------------------------------------------------------------
struct RunLoop : U::Implements<U::Directly<Linux::IRunLoop>>
{
	using IEventHandler = Linux::IEventHandler;
	using FileDescriptor = Linux::FileDescriptor;
	using TimerInterval = Linux::TimerInterval;
	using ITimerHandler = Linux::ITimerHandler;

	// IRunLoop
	tresult PLUGIN_API registerEventHandler (IEventHandler* handler, FileDescriptor fd) override;
	tresult PLUGIN_API unregisterEventHandler (IEventHandler* handler) override;
	tresult PLUGIN_API registerTimer (ITimerHandler* handler, TimerInterval milliseconds) override;
	tresult PLUGIN_API unregisterTimer (ITimerHandler* handler) override;

	RunLoop (wl_display* display);
	~RunLoop () noexcept override;

	void run ();
	void stop ();

private:
	struct Impl;
	std::unique_ptr<Impl> impl;
};

//------------------------------------------------------------------------
} // Steinberg::Vst::EditorHost::Wayland
