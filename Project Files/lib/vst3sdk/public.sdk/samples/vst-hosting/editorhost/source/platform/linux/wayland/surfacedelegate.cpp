//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/linux/wayland/surfacedelegate.cpp
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

#include "surfacedelegate.h"

namespace Steinberg::Vst::EditorHost::Wayland {

//------------------------------------------------------------------------
// SurfaceDelegate
//------------------------------------------------------------------------
SurfaceDelegate::SurfaceDelegate ()
: WaylandServerDelegate::WaylandResource (&::wl_surface_interface,
                                          static_cast<wl_surface_interface*> (this))
{
	wl_surface_interface::destroy = destroy;
	wl_surface_interface::attach = attach;
	wl_surface_interface::damage = damage;
	wl_surface_interface::frame = frame;
	wl_surface_interface::set_opaque_region = set_opaque_region;
	wl_surface_interface::set_input_region = set_input_region;
	wl_surface_interface::commit = commit;
	wl_surface_interface::set_buffer_transform = set_buffer_transform;
	wl_surface_interface::set_buffer_scale = set_buffer_scale;
	wl_surface_interface::damage_buffer = damage_buffer;
	wl_surface_interface::offset = offset;
}

//------------------------------------------------------------------------
void SurfaceDelegate::destroy (struct wl_client* client, struct wl_resource* resource)
{
	WaylandResource::onDestroy (resource);
}

//------------------------------------------------------------------------
void SurfaceDelegate::SurfaceDelegate::attach (struct wl_client* client,
                                               struct wl_resource* resource,
                                               struct wl_resource* buffer, int32_t x, int32_t y)
{
}

//------------------------------------------------------------------------
void SurfaceDelegate::damage (struct wl_client* client, struct wl_resource* resource, int32_t x,
                              int32_t y, int32_t width, int32_t height)
{
}

//------------------------------------------------------------------------
void SurfaceDelegate::frame (struct wl_client* client, struct wl_resource* resource,
                             uint32_t callback)
{
}

//------------------------------------------------------------------------
void SurfaceDelegate::set_opaque_region (struct wl_client* client, struct wl_resource* resource,
                                         struct wl_resource* region)
{
}
//------------------------------------------------------------------------
void SurfaceDelegate::set_input_region (struct wl_client* client, struct wl_resource* resource,
                                        struct wl_resource* region)
{
}
//------------------------------------------------------------------------
void SurfaceDelegate::commit (struct wl_client* client, struct wl_resource* resource)
{
}

//------------------------------------------------------------------------
void SurfaceDelegate::set_buffer_transform (struct wl_client* client, struct wl_resource* resource,
                                            int32_t transform)
{
}

//------------------------------------------------------------------------
void SurfaceDelegate::set_buffer_scale (struct wl_client* client, struct wl_resource* resource,
                                        int32_t scale)
{
}

//------------------------------------------------------------------------
void SurfaceDelegate::damage_buffer (struct wl_client* client, struct wl_resource* resource,
                                     int32_t x, int32_t y, int32_t width, int32_t height)
{
}

//------------------------------------------------------------------------
void SurfaceDelegate::offset (struct wl_client* client, struct wl_resource* resource, int32_t x,
                              int32_t y)
{
}

//------------------------------------------------------------------------
// XdgSurfaceDelegate
//------------------------------------------------------------------------
XdgSurfaceDelegate::XdgSurfaceDelegate ()
: WaylandServerDelegate::WaylandResource (&::xdg_surface_interface,
                                          static_cast<xdg_surface_interface*> (this))
{
	xdg_surface_interface::destroy = destroy;
	xdg_surface_interface::get_toplevel = get_toplevel;
	xdg_surface_interface::get_popup = get_popup;
	xdg_surface_interface::set_window_geometry = set_window_geometry;
	xdg_surface_interface::ack_configure = ack_configure;
}

//------------------------------------------------------------------------
void XdgSurfaceDelegate::destroy (struct wl_client* client, struct wl_resource* resource)
{
	WaylandResource::onDestroy (resource);
}

//------------------------------------------------------------------------
void XdgSurfaceDelegate::get_toplevel (struct wl_client* client, struct wl_resource* resource,
                                       uint32_t id)
{
}

//------------------------------------------------------------------------
void XdgSurfaceDelegate::get_popup (struct wl_client* client, struct wl_resource* resource,
                                    uint32_t id, struct wl_resource* parent,
                                    struct wl_resource* positioner)
{
}

//------------------------------------------------------------------------
void XdgSurfaceDelegate::set_window_geometry (struct wl_client* client,
                                              struct wl_resource* resource, int32_t x, int32_t y,
                                              int32_t width, int32_t height)
{
}

//------------------------------------------------------------------------
void XdgSurfaceDelegate::ack_configure (struct wl_client* client, struct wl_resource* resource,
                                        uint32_t serial)
{
}

//------------------------------------------------------------------------
// XdgToplevelDelegate
//------------------------------------------------------------------------
XdgToplevelDelegate::XdgToplevelDelegate ()
: WaylandServerDelegate::WaylandResource (&::xdg_toplevel_interface,
                                          static_cast<xdg_toplevel_interface*> (this))
{
	xdg_toplevel_interface::destroy = destroy;
	xdg_toplevel_interface::set_parent = set_parent;
	xdg_toplevel_interface::set_title = set_title;
	xdg_toplevel_interface::set_app_id = set_app_id;
	xdg_toplevel_interface::show_window_menu = show_window_menu;
	xdg_toplevel_interface::move = move;
	xdg_toplevel_interface::resize = resize;
	xdg_toplevel_interface::set_max_size = set_max_size;
	xdg_toplevel_interface::set_min_size = set_min_size;
	xdg_toplevel_interface::set_maximized = set_maximized;
	xdg_toplevel_interface::unset_maximized = unset_maximized;
	xdg_toplevel_interface::set_fullscreen = set_fullscreen;
	xdg_toplevel_interface::unset_fullscreen = unset_fullscreen;
	xdg_toplevel_interface::set_minimized = set_minimized;
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::destroy (struct wl_client* client, struct wl_resource* resource)
{
	WaylandResource::onDestroy (resource);
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::set_parent (struct wl_client* client, struct wl_resource* resource,
                                      struct wl_resource* parent)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::set_title (struct wl_client* client, struct wl_resource* resource,
                                     const char* title)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::set_app_id (struct wl_client* client, struct wl_resource* resource,
                                      const char* app_id)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::show_window_menu (struct wl_client* client, struct wl_resource* resource,
                                            struct wl_resource* seat, uint32_t serial, int32_t x,
                                            int32_t y)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::move (struct wl_client* client, struct wl_resource* resource,
                                struct wl_resource* seat, uint32_t serial)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::resize (struct wl_client* client, struct wl_resource* resource,
                                  struct wl_resource* seat, uint32_t serial, uint32_t edges)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::set_max_size (struct wl_client* client, struct wl_resource* resource,
                                        int32_t width, int32_t height)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::set_min_size (struct wl_client* client, struct wl_resource* resource,
                                        int32_t width, int32_t height)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::set_maximized (struct wl_client* client, struct wl_resource* resource)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::unset_maximized (struct wl_client* client, struct wl_resource* resource)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::set_fullscreen (struct wl_client* client, struct wl_resource* resource,
                                          struct wl_resource* output)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::unset_fullscreen (struct wl_client* client, struct wl_resource* resource)
{
}

//------------------------------------------------------------------------
void XdgToplevelDelegate::set_minimized (struct wl_client* client, struct wl_resource* resource)
{
}

//-----------------------------------------------------------------------------
} // namespace Steinberg::Vst::EditorHost::Wayland
