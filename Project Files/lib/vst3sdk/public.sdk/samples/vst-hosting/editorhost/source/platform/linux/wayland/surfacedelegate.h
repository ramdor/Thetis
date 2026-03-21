//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/linux/wayland/surfacedelegate.h
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

#include "wayland-server-delegate/iwaylandserver.h"
#include "wayland-server-delegate/waylandresource.h"

#include "xdg-shell-client-protocol.h"
#include "xdg-shell-server-protocol.h"

namespace Steinberg::Vst::EditorHost::Wayland {

//------------------------------------------------------------------------
// SurfaceDelegate
//------------------------------------------------------------------------
struct SurfaceDelegate : WaylandServerDelegate::WaylandResource, wl_surface_interface
{
	SurfaceDelegate ();

	static void destroy (struct wl_client* client, struct wl_resource* resource);
	static void attach (struct wl_client* client, struct wl_resource* resource,
	                    struct wl_resource* buffer, int32_t x, int32_t y);
	static void damage (struct wl_client* client, struct wl_resource* resource, int32_t x,
	                    int32_t y, int32_t width, int32_t height);
	static void frame (struct wl_client* client, struct wl_resource* resource, uint32_t callback);
	static void set_opaque_region (struct wl_client* client, struct wl_resource* resource,
	                               struct wl_resource* region);
	static void set_input_region (struct wl_client* client, struct wl_resource* resource,
	                              struct wl_resource* region);
	static void commit (struct wl_client* client, struct wl_resource* resource);
	static void set_buffer_transform (struct wl_client* client, struct wl_resource* resource,
	                                  int32_t transform);
	static void set_buffer_scale (struct wl_client* client, struct wl_resource* resource,
	                              int32_t scale);
	static void damage_buffer (struct wl_client* client, struct wl_resource* resource, int32_t x,
	                           int32_t y, int32_t width, int32_t height);
	static void offset (struct wl_client* client, struct wl_resource* resource, int32_t x,
	                    int32_t y);
};

//------------------------------------------------------------------------
// XdgSurfaceDelegate
//------------------------------------------------------------------------
struct XdgSurfaceDelegate : WaylandServerDelegate::WaylandResource, xdg_surface_interface
{
	XdgSurfaceDelegate ();

	static void destroy (struct wl_client* client, struct wl_resource* resource);
	static void get_toplevel (struct wl_client* client, struct wl_resource* resource, uint32_t id);
	static void get_popup (struct wl_client* client, struct wl_resource* resource, uint32_t id,
	                       struct wl_resource* parent, struct wl_resource* positioner);
	static void set_window_geometry (struct wl_client* client, struct wl_resource* resource,
	                                 int32_t x, int32_t y, int32_t width, int32_t height);
	static void ack_configure (struct wl_client* client, struct wl_resource* resource,
	                           uint32_t serial);
};

//------------------------------------------------------------------------
// XdgToplevelDelegate
//------------------------------------------------------------------------
struct XdgToplevelDelegate : WaylandServerDelegate::WaylandResource, xdg_toplevel_interface
{
	XdgToplevelDelegate ();

	static void destroy (struct wl_client* client, struct wl_resource* resource);
	static void set_parent (struct wl_client* client, struct wl_resource* resource,
	                        struct wl_resource* parent);
	static void set_title (struct wl_client* client, struct wl_resource* resource,
	                       const char* title);
	static void set_app_id (struct wl_client* client, struct wl_resource* resource,
	                        const char* app_id);
	static void show_window_menu (struct wl_client* client, struct wl_resource* resource,
	                              struct wl_resource* seat, uint32_t serial, int32_t x, int32_t y);
	static void move (struct wl_client* client, struct wl_resource* resource,
	                  struct wl_resource* seat, uint32_t serial);
	static void resize (struct wl_client* client, struct wl_resource* resource,
	                    struct wl_resource* seat, uint32_t serial, uint32_t edges);
	static void set_max_size (struct wl_client* client, struct wl_resource* resource, int32_t width,
	                          int32_t height);
	static void set_min_size (struct wl_client* client, struct wl_resource* resource, int32_t width,
	                          int32_t height);
	static void set_maximized (struct wl_client* client, struct wl_resource* resource);
	static void unset_maximized (struct wl_client* client, struct wl_resource* resource);
	static void set_fullscreen (struct wl_client* client, struct wl_resource* resource,
	                            struct wl_resource* output);
	static void unset_fullscreen (struct wl_client* client, struct wl_resource* resource);
	static void set_minimized (struct wl_client* client, struct wl_resource* resource);
};

//-----------------------------------------------------------------------------
} // namespace Steinberg::Vst::EditorHost::Wayland
