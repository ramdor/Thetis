//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/linux/wayland/window.cpp
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

#include "window.h"

#include "runloop.h"
#include "surfacedelegate.h"
#include "wayland-server-delegate/iwaylandclientcontext.h"
#include "xdg-decoration-unstable-v1-client-protocol.h"

#include <fcntl.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <unistd.h>

DEF_CLASS_IID (Steinberg::IWaylandFrame)
DEF_CLASS_IID (Steinberg::IWaylandHost);

namespace Steinberg::Vst::EditorHost::Wayland {
namespace {

//------------------------------------------------------------------------
/* Shared memory support code */
void randname (char* buf)
{
	struct timespec ts;
	clock_gettime (CLOCK_REALTIME, &ts);
	long r = ts.tv_nsec;
	for (int i = 0; i < 6; ++i)
	{
		buf[i] = 'A' + (r & 15) + (r & 16) * 2;
		r >>= 5;
	}
}

//------------------------------------------------------------------------
int create_shm_file (void)
{
	int retries = 100;
	do
	{
		char name[] = "/wl_shm-XXXXXX";
		randname (name + sizeof (name) - 7);
		--retries;
		int fd = shm_open (name, O_RDWR | O_CREAT | O_EXCL, 0600);
		if (fd >= 0)
		{
			shm_unlink (name);
			return fd;
		}
	} while (retries > 0 && errno == EEXIST);
	return -1;
}

//------------------------------------------------------------------------
int allocate_shm_file (size_t size)
{
	int fd = create_shm_file ();
	if (fd < 0)
		return -1;
	int ret;
	do
	{
		ret = ftruncate (fd, size);
	} while (ret < 0 && errno == EINTR);
	if (ret < 0)
	{
		close (fd);
		return -1;
	}
	return fd;
}

//------------------------------------------------------------------------
struct wl_buffer* draw_frame (wl_shm* shm, Size inSize)
{
	const int width = inSize.width, height = inSize.height;
	int stride = width * 4;
	int size = stride * height;

	int fd = allocate_shm_file (size);
	if (fd == -1)
	{
		return NULL;
	}

	auto data = mmap (NULL, size, PROT_READ | PROT_WRITE, MAP_SHARED, fd, 0);
	if (data == MAP_FAILED)
	{
		close (fd);
		return NULL;
	}

	struct wl_shm_pool* pool = wl_shm_create_pool (shm, fd, size);
	struct wl_buffer* buffer =
	    wl_shm_pool_create_buffer (pool, 0, width, height, stride, WL_SHM_FORMAT_XRGB8888);
	wl_shm_pool_destroy (pool);
	close (fd);

	auto int32data = reinterpret_cast<int32_t*> (data);
	/* Draw checkerboxed background */
	for (int y = 0; y < height; ++y)
	{
		for (int x = 0; x < width; ++x)
		{
			if ((x + y / 8 * 8) % 16 < 8)
				int32data[y * width + x] = 0xFF666666;
			else
				int32data[y * width + x] = 0xFFEEEEEE;
		}
	}

	munmap (data, size);

	static const struct wl_buffer_listener wlBufferListener = {
	    .release = [] (void* data, struct wl_buffer* wl_buffer) { wl_buffer_destroy (wl_buffer); },
	};

	wl_buffer_add_listener (buffer, &wlBufferListener, NULL);
	return buffer;
}

//------------------------------------------------------------------------
} // anonymous

//------------------------------------------------------------------------
// WaylandWindow::Impl
//------------------------------------------------------------------------
struct WaylandWindow::Impl
{
	using IWaylandClientContext = WaylandServerDelegate::IWaylandClientContext;
	Impl (const IWaylandClientContext& waylandClientContext)
	: waylandClientContext (waylandClientContext)
	{
	}

	WindowControllerPtr controller;
	WindowClosedFunc windowClosedFunc;
	IWaylandProxyCreator* wlProxyCreator = nullptr;

	const IWaylandClientContext& waylandClientContext;
	wl_surface* wlSurface {nullptr};
	xdg_surface* xdgSurface {nullptr};
	xdg_toplevel* xdgToplevel {nullptr};
	zxdg_toplevel_decoration_v1* topLevelDeco {nullptr};
	Size size {};
	std::string title;
	int32_t currentPreferredBufferScale {1};

	wl_proxy* wlSurfaceProxy {nullptr};
	wl_proxy* xdgSurfaceProxy {nullptr};
	wl_proxy* xdgToplevelProxy {nullptr};

	IPtr<RunLoop> runLoop;
	IPtr<IWaylandHost> host;

	using FuncDoClose = std::function<void ()>;
	FuncDoClose doCloseFunc;

	using FuncOnScaleFactorChanged = std::function<void (float scaleFactor)>;
	FuncOnScaleFactorChanged onScaleFactorChangedFunc;

	void initSurfaceGeometry (int32_t width, int32_t height);
	void addSurfaceListener ();
	void addXdgSurfaceListener ();
	void addToplevelListener ();
	void addTopLevelDecorationListener ();

	static void updateScaleFactor (WaylandWindow::Impl* self, int32_t factor);

	static void handleXdgSurfaceConfigure (void* data, struct xdg_surface* xdg_surface,
	                                       uint32_t serial);
	static void handleXdgToplevelConfigure (void* data, struct xdg_toplevel* xdg_toplevel,
	                                        int32_t width, int32_t height, struct wl_array* states);
	static void handleXdgToplevelClose (void* data, struct xdg_toplevel* toplevel);
	static void handleXdgToplevelConfigureBounds (void* data, struct xdg_toplevel* xdg_toplevel,
	                                              int32_t width, int32_t height);
	static void handleXdgToplevelWmCapabilities (void* data, struct xdg_toplevel* xdg_toplevel,
	                                             struct wl_array* capabilities);
	static void handleTopLevelDecorationConfigure (
	    void* data, struct zxdg_toplevel_decoration_v1* zxdg_toplevel_decoration_v1, uint32_t mode);
	static void handleEnter (void* data, wl_surface* wl_surface, wl_output* output);
	static void handleLeave (void* data, wl_surface* wl_surface, wl_output* output);
	static void handlePreferredBufferScale (void* data, wl_surface* wl_surface, int32_t factor);
	static void handlePreferredBufferTransform (void* data, wl_surface* wl_surface,
	                                            uint32_t transform);
};

//------------------------------------------------------------------------
WaylandWindow::WaylandWindow (const IWaylandClientContext& waylandClientContext)
{
	impl = std::make_unique<Impl> (waylandClientContext);
}

//------------------------------------------------------------------------
WaylandWindow::~WaylandWindow () noexcept
{
}

//------------------------------------------------------------------------
auto WaylandWindow::make (const std::string& name, Size size, bool resizeable,
                          const WindowControllerPtr& controller,
                          const WindowClosedFunc& windowClosedFunc,
                          IWaylandProxyCreator* wlProxyCreator, IPtr<RunLoop> runLoop,
                          IPtr<IWaylandHost> host,
                          const IWaylandClientContext& waylandClientContext) -> Ptr
{
	auto window = std::make_shared<WaylandWindow> (waylandClientContext);
	if (!window)
		return {};

	window->impl->runLoop = runLoop;
	window->impl->host = host;
	window->impl->controller = controller;
	window->impl->windowClosedFunc = windowClosedFunc;
	window->impl->wlProxyCreator = wlProxyCreator;
	window->impl->title = name;
	window->impl->size = size;
	window->impl->doCloseFunc = [window] () { window->doClose (); };
	window->impl->onScaleFactorChangedFunc = [window, controller] (float scaleFactor) {
		controller->onContentScaleFactorChanged (*window.get (), scaleFactor);
	};

	window->impl->wlSurface = wl_compositor_create_surface (waylandClientContext.getCompositor ());
	if (!window->impl->wlSurface)
		return {};

	window->impl->xdgSurface = xdg_wm_base_get_xdg_surface (
	    waylandClientContext.getWindowManager (), window->impl->wlSurface);
	if (!window->impl->xdgSurface)
		return {};

	window->impl->xdgToplevel = xdg_surface_get_toplevel (window->impl->xdgSurface);
	if (!window->impl->xdgToplevel)
		return {};

	window->impl->addSurfaceListener ();
	window->impl->addXdgSurfaceListener ();
	window->impl->addToplevelListener ();
	window->impl->initSurfaceGeometry (window->impl->size.width, window->impl->size.height);

	/* TODO
	if (std::string_view (interface) == zxdg_decoration_manager_v1_interface.name)
	{
	    static constexpr uint32_t kVersion = 8;
	    void* object = wl_registry_bind (
	        registry, name, &zxdg_decoration_manager_v1_interface, kVersion);
	    auto deco_manager = reinterpret_cast<zxdg_decoration_manager_v1*> (object);
	    globals.deco_manager = deco_manager;
	    return;
	}

	if (waylandGlobals.deco_manager != nullptr)
	{
	    window->impl->topLevelDeco = zxdg_decoration_manager_v1_get_toplevel_decoration (
	        waylandGlobals.deco_manager, window->impl->xdgToplevel);
	    window->impl->addTopLevelDecorationListener ();
	    zxdg_toplevel_decoration_v1_set_mode (window->impl->topLevelDeco,
	                                          ZXDG_TOPLEVEL_DECORATION_V1_MODE_SERVER_SIDE);
	}
	*/

	return window;
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::handleXdgSurfaceConfigure (void* data, struct xdg_surface* xdg_surface,
                                                     uint32_t serial)
{
	auto self = reinterpret_cast<WaylandWindow::Impl*> (data);
	if (!self)
		return;

	xdg_surface_ack_configure (xdg_surface, serial);

	auto buffer = draw_frame (self->waylandClientContext.getSharedMemory (), self->size);
	wl_surface_set_buffer_scale (self->wlSurface, self->currentPreferredBufferScale);
	wl_surface_attach (self->wlSurface, buffer, 0, 0);
	wl_surface_commit (self->wlSurface);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::handleXdgToplevelConfigure (void* data, xdg_toplevel* xdg_toplevel,
                                                      int32_t width, int32_t height,
                                                      wl_array* states)
{
	auto self = reinterpret_cast<WaylandWindow::Impl*> (data);
	if (!self)
		return;

	if (width > 0 && height > 0)
	{
		self->size.width = width;
		self->size.height = height;
	}
	xdg_toplevel_set_title (self->xdgToplevel, self->title.data ());
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::handleXdgToplevelClose (void* data, struct xdg_toplevel* toplevel)
{
	auto self = reinterpret_cast<WaylandWindow::Impl*> (data);
	if (self && self->doCloseFunc)
		self->doCloseFunc ();
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::handleXdgToplevelConfigureBounds (void* data,
                                                            struct xdg_toplevel* xdg_toplevel,
                                                            int32_t width, int32_t height)
{
	auto self = reinterpret_cast<WaylandWindow::Impl*> (data);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::handleXdgToplevelWmCapabilities (void* data,
                                                           struct xdg_toplevel* xdg_toplevel,
                                                           struct wl_array* capabilities)
{
	auto self = reinterpret_cast<WaylandWindow::Impl*> (data);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::handleTopLevelDecorationConfigure (
    void* data, struct zxdg_toplevel_decoration_v1* zxdg_toplevel_decoration_v1, uint32_t mode)
{
	auto self = reinterpret_cast<WaylandWindow::Impl*> (data);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::updateScaleFactor (WaylandWindow::Impl* self, int32_t factor)
{
	self->currentPreferredBufferScale = factor;
	wl_surface_set_buffer_scale (self->wlSurface, factor);
	wl_surface_commit (self->wlSurface);

	if (self->onScaleFactorChangedFunc)
		self->onScaleFactorChangedFunc (factor);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::handleEnter (void* data, wl_surface* wl_surface, wl_output* output)
{
	auto self = reinterpret_cast<WaylandWindow::Impl*> (data);
	if (!self)
		return;

	if (wl_surface != self->wlSurface)
		return;

#if WL_SURFACE_PREFERRED_BUFFER_SCALE_SINCE_VERSION
// 'preferred_buffer_scale' is handling the scale factor
#else
	const auto count = self->waylandClientContext.countOutputs ();
	for (auto i = 0; i < count; ++i)
	{
		const auto& wlOutput = self->waylandClientContext.getOutput (i);
		if (wlOutput.handle != output)
			continue;

		updateScaleFactor (self, wlOutput.scaleFactor);
	}
#endif
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::handleLeave (void* data, wl_surface* wl_surface, wl_output* output)
{
	auto self = reinterpret_cast<WaylandWindow::Impl*> (data);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::handlePreferredBufferScale (void* data, wl_surface* wl_surface,
                                                      int32_t factor)
{
	auto self = reinterpret_cast<WaylandWindow::Impl*> (data);
	if (!self)
		return;

	if (wl_surface != self->wlSurface)
		return;

	updateScaleFactor (self, factor);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::handlePreferredBufferTransform (void* data, wl_surface* wl_surface,
                                                          uint32_t transform)
{
	auto self = reinterpret_cast<WaylandWindow::Impl*> (data);
}

//------------------------------------------------------------------------
void WaylandWindow::doClose ()
{
	impl->controller->onClose (*this);
	if (impl->windowClosedFunc)
		impl->windowClosedFunc (this);

	if (impl->wlProxyCreator)
	{
		if (impl->wlSurfaceProxy)
			impl->wlProxyCreator->destroyProxy (impl->wlSurfaceProxy);
		if (impl->xdgSurfaceProxy)
			impl->wlProxyCreator->destroyProxy (impl->xdgSurfaceProxy);
		if (impl->xdgToplevelProxy)
			impl->wlProxyCreator->destroyProxy (impl->xdgToplevelProxy);
	}

	impl->xdgToplevelProxy = {};
	impl->xdgSurfaceProxy = {};
	impl->wlSurfaceProxy = {};

	if (impl->xdgToplevel)
		xdg_toplevel_destroy (impl->xdgToplevel);
	if (impl->xdgSurface)
		xdg_surface_destroy (impl->xdgSurface);
	if (impl->wlSurface)
		wl_surface_destroy (impl->wlSurface);

	impl->xdgToplevel = {};
	impl->xdgSurface = {};
	impl->wlSurface = {};
}

//------------------------------------------------------------------------
void WaylandWindow::show ()
{
	impl->controller->onShow (*this);
}

//------------------------------------------------------------------------
void WaylandWindow::close ()
{
	impl->controller->onClose (*this);
}

//------------------------------------------------------------------------
void WaylandWindow::resize (Size newSize)
{
	impl->size = newSize;
	if (impl->xdgSurface)
		xdg_surface_set_window_geometry (impl->xdgSurface, 0, 0, impl->size.width,
		                                 impl->size.height);

	impl->controller->onResize (*this, impl->size);
}

//------------------------------------------------------------------------
Size WaylandWindow::getContentSize ()
{
	return impl->size;
}

//------------------------------------------------------------------------
NativePlatformWindow WaylandWindow::getNativePlatformWindow () const
{
	return {kPlatformTypeWaylandSurfaceID, nullptr};
}

//------------------------------------------------------------------------
wl_surface* PLUGIN_API WaylandWindow::getWaylandSurface (wl_display* display)
{
	if (!impl->wlProxyCreator)
		return nullptr;

	if (impl->wlSurfaceProxy)
		return reinterpret_cast<wl_surface*> (impl->wlSurfaceProxy);

	impl->wlSurfaceProxy = impl->wlProxyCreator->createProxy (
	    display, reinterpret_cast<wl_proxy*> (impl->wlSurface), new SurfaceDelegate);

	return reinterpret_cast<wl_surface*> (impl->wlSurfaceProxy);
}

//------------------------------------------------------------------------
xdg_surface* PLUGIN_API WaylandWindow::getParentSurface (ViewRect& parentSize, wl_display* display)
{
	if (!impl->wlProxyCreator)
		return nullptr;

	if (impl->xdgSurfaceProxy)
		return reinterpret_cast<xdg_surface*> (impl->xdgSurfaceProxy);

	impl->xdgSurfaceProxy = impl->wlProxyCreator->createProxy (
	    display, reinterpret_cast<wl_proxy*> (impl->xdgSurface), new XdgSurfaceDelegate);

	parentSize = ViewRect (0, 0, impl->size.width, impl->size.height);
	return reinterpret_cast<xdg_surface*> (impl->xdgSurfaceProxy);
}

//------------------------------------------------------------------------
xdg_toplevel* PLUGIN_API WaylandWindow::getParentToplevel (wl_display* display)
{
	if (!impl->wlProxyCreator)
		return nullptr;

	if (impl->xdgToplevelProxy)
		return reinterpret_cast<xdg_toplevel*> (impl->xdgToplevelProxy);

	impl->xdgToplevelProxy = impl->wlProxyCreator->createProxy (
	    display, reinterpret_cast<wl_proxy*> (impl->xdgToplevelProxy), new XdgToplevelDelegate);

	return reinterpret_cast<xdg_toplevel*> (impl->xdgToplevelProxy);
}

//------------------------------------------------------------------------
tresult WaylandWindow::queryInterface (const TUID iid, void** obj)
{
	if (FUnknownPrivate::iidEqual (iid, IWaylandFrame::iid))
	{
		*obj = static_cast<IWaylandFrame*> (this);
		return kResultTrue;
	}
	if (FUnknownPrivate::iidEqual (iid, IWaylandHost::iid))
	{
		*obj = static_cast<IWaylandHost*> (impl->host);
		impl->host->addRef ();
		return kResultTrue;
	}
	if (FUnknownPrivate::iidEqual (iid, Linux::IRunLoop::iid))
	{
		*obj = static_cast<Linux::IRunLoop*> (impl->runLoop);
		impl->runLoop->addRef ();
		return kResultTrue;
	}
	return kNoInterface;
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::initSurfaceGeometry (int32_t width, int32_t height)
{
	xdg_surface_set_window_geometry (xdgSurface, 0, 0, size.width, size.height);
	wl_surface_commit (wlSurface);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::addSurfaceListener ()
{
	static const wl_surface_listener listener = {
	    .enter = handleEnter,
	    .leave = handleLeave,
	    .preferred_buffer_scale = handlePreferredBufferScale,
	    .preferred_buffer_transform = handlePreferredBufferTransform,
	};

	wl_surface_add_listener (wlSurface, &listener, this);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::addXdgSurfaceListener ()
{
	static const xdg_surface_listener listener = {
	    .configure = handleXdgSurfaceConfigure,
	};
	xdg_surface_add_listener (xdgSurface, &listener, this);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::addToplevelListener ()
{
	static const struct xdg_toplevel_listener listener = {
	    .configure = handleXdgToplevelConfigure,
	    .close = handleXdgToplevelClose,
	    .configure_bounds = handleXdgToplevelConfigureBounds,
	    .wm_capabilities = handleXdgToplevelWmCapabilities,
	};
	xdg_toplevel_add_listener (xdgToplevel, &listener, this);
}

//------------------------------------------------------------------------
void WaylandWindow::Impl::addTopLevelDecorationListener ()
{
	static struct zxdg_toplevel_decoration_v1_listener zxdgTopLevelDecorationListener = {
	    .configure = handleTopLevelDecorationConfigure,
	};
	zxdg_toplevel_decoration_v1_add_listener (topLevelDeco, &zxdgTopLevelDecorationListener, this);
}

//------------------------------------------------------------------------
} // Steinberg::Vst::EditorHost::Wayland
