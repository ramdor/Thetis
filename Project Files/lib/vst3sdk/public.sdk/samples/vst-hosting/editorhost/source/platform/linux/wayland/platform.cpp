//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/linux/wayland/platform.cpp
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

#include "public.sdk/samples/vst-hosting/editorhost/source/platform/linux/irunloopimpl.h"

#include "../../../editorhost.h"
#include "../../iplatform.h"
#include "clientcontext.h"
#include "wayland-server-delegate/iwaylandclientcontext.h"
#include "wayland-server-delegate/iwaylandserver.h"
#include "wayland-server-delegate/waylandresource.h"
#include "window.h"
#include "xdg-decoration-unstable-v1-client-protocol.h"
#include "xdg-shell-client-protocol.h"
#include "pluginterfaces/base/funknownimpl.h"
#include "pluginterfaces/gui/iwaylandframe.h"
#include <array>
#include <cassert>
#include <fcntl.h>
#include <iostream>
#include <mutex>
#include <optional>
#include <poll.h>
#include <string_view>
#include <sys/socket.h>
#include <thread>
#include <unistd.h>
#include <wayland-server.h>

namespace Steinberg::Vst::EditorHost {
namespace Wayland {

//------------------------------------------------------------------------
struct ReleaseWaylandDisplay
{
	void operator () (wl_display* d) const { wl_display_disconnect (d); }
};
using WaylandDisplayPtr = std::unique_ptr<wl_display, ReleaseWaylandDisplay>;

//------------------------------------------------------------------------
static int createCancelSockets (int (&fds)[2])
{
	if (::socketpair (AF_UNIX, SOCK_STREAM, 0, fds) == -1)
		return -1;

	::fcntl (fds[0], F_SETFD, FD_CLOEXEC);
	return 0;
}

//------------------------------------------------------------------------
static void closeCancelSockets (int (&fds)[2])
{
	int msg = 0;
	ssize_t bytesWritten = write (fds[0], &msg, sizeof (msg));

	::close (fds[0]);
	::close (fds[1]);
}

//------------------------------------------------------------------------
// Threading
//------------------------------------------------------------------------
namespace Threading {
using Mutex = std::mutex;
using ScopedLock = const std::lock_guard<std::mutex>;
using Thread = std::thread;
} // namespace Threading

//------------------------------------------------------------------------
// Platform
//------------------------------------------------------------------------
class Platform
: public IPlatform,
  public U::ImplementsNonDestroyable<U::Directly<IWaylandHost, Linux::IEventHandler>>,
  public IWaylandProxyCreator
{
public:
//------------------------------------------------------------------------
	static Platform& instance ()
	{
		static Platform gInstance;
		return gInstance;
	}
	void run (const std::vector<std::string>& cmdArgs);

//------------------------------------------------------------------------
private:
	using WaylandWindows = std::vector<std::shared_ptr<WaylandWindow>>;

	void setApplication (ApplicationPtr&& app) override;
	WindowPtr createWindow (const std::string& title, Size size, bool resizeable,
	                        const WindowControllerPtr& controller) override;

	void quit () override;
	void kill (int resultCode, const std::string& reason) override;

	FUnknown* getPluginFactoryContext () override;

	void initWayland ();
	wl_display* getDisplay () const { return displayPtr.get (); }

	// IWaylandHost
	wl_display* PLUGIN_API openWaylandConnection () override;
	tresult PLUGIN_API closeWaylandConnection (wl_display* display) override;

	// IWaylandProxyCreator
	wl_proxy* createProxy (wl_display* display, wl_proxy* object,
	                       WaylandServerDelegate::WaylandResource* implementation) override;
	void destroyProxy (wl_proxy* proxy) override;

	// Linux::IEventHandler
	void PLUGIN_API onFDIsSet (Linux::FileDescriptor fd) override;

	WaylandClientContext client;
	ApplicationPtr application;
	WaylandDisplayPtr displayPtr;
	wl_event_queue* eventQueue {nullptr};
	WaylandWindows windows;
	IPtr<RunLoop> runLoop;

	bool isQuitting = false;
	bool shouldTerminate () { return isQuitting; }

	Threading::Mutex lock;
	int threadEntry ();
	int serverFd {};
	int cancelFd[2];
};

//------------------------------------------------------------------------
void Platform::initWayland ()
{
	displayPtr = WaylandDisplayPtr (wl_display_connect (nullptr));
	if (!getDisplay ())
		kill (-1, "wl_display_connect failed");

	bool initWaylandDone = client.initWayland (getDisplay ());
	if (!initWaylandDone)
		kill (-1, "init wayland failed");

	eventQueue = wl_display_create_queue (getDisplay ());
	serverFd = WaylandServerDelegate::IWaylandServer::instance ().startup (&client, eventQueue);
	if (serverFd == -1)
		kill (-1, "IWaylandServer startup failed");

	wl_display_roundtrip (getDisplay ());
	wl_display_flush (getDisplay ());
}

//------------------------------------------------------------------------
FUnknown* Platform::getPluginFactoryContext ()
{
	return &Steinberg::Linux::RunLoopImpl::instance ();
}

//------------------------------------------------------------------------
void Platform::run (const std::vector<std::string>& cmdArgs)
{
	initWayland ();

	runLoop = owned (new RunLoop (getDisplay ()));

	// start server dispatch before initializing the application, otherwise plug-ins might block the
	// main thread
	createCancelSockets (cancelFd);
	Threading::Thread serverThread (&Platform::threadEntry, this);

	application->init (cmdArgs);
	runLoop->run ();

	closeCancelSockets (cancelFd);
	serverThread.join ();
}

//------------------------------------------------------------------------
void PLUGIN_API Platform::onFDIsSet (Linux::FileDescriptor fd)
{
	{
		Threading::ScopedLock guard (lock);
		WaylandServerDelegate::IWaylandServer::instance ().dispatch ();
	}
}

//------------------------------------------------------------------------
#define ARRAY_COUNT(arr) (std::end (arr) - std::begin (arr))
int Platform::threadEntry ()
{
	wl_display* display = getDisplay ();
	wl_event_queue* queue = eventQueue;
	int displayFd = wl_display_get_fd (display);

	bool receivedClientEvents = false;
	while (true)
	{
		// flush server events (server -> clients)
		{
			Threading::ScopedLock guard (lock);
			WaylandServerDelegate::IWaylandServer::instance ().flush ();
		}

		// flush display (server -> session compositor)
		if (receivedClientEvents)
			wl_display_roundtrip_queue (display, queue);
		else
			wl_display_flush (display);

		receivedClientEvents = false;
		if (wl_display_prepare_read_queue (display, queue) == 0)
		{
			pollfd fds[] = {
			    {serverFd, POLLIN, 0}, {displayFd, POLLIN, 0}, {cancelFd[0], POLLIN, 0}};

			::poll (fds, ARRAY_COUNT (fds), -1);

			// dispatch incoming server events (clients -> server)
			if ((fds[0].revents & POLLIN) > 0)
			{
				Threading::ScopedLock guard (lock);
				WaylandServerDelegate::IWaylandServer::instance ().dispatch ();

				receivedClientEvents = true;
			}

			// dispatch server-side Wayland objects (session compositor -> server)
			if ((fds[1].revents & POLLIN) > 0)
			{
				if (wl_display_read_events (display) < 0)
				{
					// CCL_WARN ("%s: %s\n", "Failed to read server Wayland events", ::strerror
					// (errno))
					break;
				}
			}
			else
			{
				wl_display_cancel_read (display);
			}
		}
		else
		{
			// CCL_PRINTLN ("WaylandServerRunLoop: Dispatching pending server-side Wayland objects
			// (session compositor -> server)")
			if (wl_display_dispatch_queue_pending (display, queue) < 0)
			{
				// CCL_WARN ("%s: %s\n", "Failed to dispatch pending Wayland server events to
				// display", ::strerror (errno))
				break;
			}
		}

		if (shouldTerminate ())
			break;
	}

	return 0;
}

//------------------------------------------------------------------------
void Platform::setApplication (ApplicationPtr&& obj)
{
	application = std::move (obj);
}

//------------------------------------------------------------------------
WindowPtr Platform::createWindow (const std::string& title, Size size, bool resizeable,
                                  const WindowControllerPtr& controller)
{
	auto window = WaylandWindow::make (
	    title, size, resizeable, controller,
	    [this] (WaylandWindow* window) {
		    auto it = std::find_if (windows.begin (), windows.end (),
		                            [&] (const auto& el) { return el.get () == window; });
		    if (it != windows.end ())
			    windows.erase (it);
	    },
	    this, runLoop, this, client);
	windows.push_back (window);
	return window;
}

//------------------------------------------------------------------------
void Platform::quit ()
{
	assert (getDisplay ());
	isQuitting = true;
	runLoop->stop ();
	if (application)
		application->terminate ();
	WaylandServerDelegate::IWaylandServer::instance ().shutdown ();
	wl_event_queue_destroy (eventQueue);
}

//------------------------------------------------------------------------
void Platform::kill (int resultCode, const std::string& reason)
{
	std::cout << reason << "\n";
	exit (resultCode);
}

//------------------------------------------------------------------------
wl_display* PLUGIN_API Platform::openWaylandConnection ()
{
	Threading::ScopedLock guard (lock);
	return WaylandServerDelegate::IWaylandServer::instance ().openClientConnection ();
}

//------------------------------------------------------------------------
tresult PLUGIN_API Platform::closeWaylandConnection (wl_display* display)
{
	Threading::ScopedLock guard (lock);
	return WaylandServerDelegate::IWaylandServer::instance ().closeClientConnection (display) ?
	           kResultTrue :
	           kResultFalse;
}

//------------------------------------------------------------------------
wl_proxy* Platform::createProxy (wl_display* display, wl_proxy* object,
                                 WaylandServerDelegate::WaylandResource* implementation)
{
	Threading::ScopedLock guard (lock);
	return WaylandServerDelegate::IWaylandServer::instance ().createProxy (display, object,
	                                                                       implementation);
}

//------------------------------------------------------------------------
void Platform::destroyProxy (wl_proxy* proxy)
{
	Threading::ScopedLock guard (lock);
	return WaylandServerDelegate::IWaylandServer::instance ().destroyProxy (proxy);
}

//------------------------------------------------------------------------
} // Wayland

//------------------------------------------------------------------------
#if SMTG_EDITOR_HOST_USE_WAYLAND
IPlatform& IPlatform::instance ()
{
	return Wayland::Platform::instance ();
}
#endif
//------------------------------------------------------------------------
} // Steinberg::Vst::EditorHost

//------------------------------------------------------------------------
//------------------------------------------------------------------------
#if SMTG_EDITOR_HOST_USE_WAYLAND
int main (int argc, char* argv[])
{
#ifdef EDITORHOST_GTK
	gtk_init (&argc, &argv);
#endif

	std::vector<std::string> cmdArgs;
	for (int i = 1; i < argc; ++i)
		cmdArgs.push_back (argv[i]);

	Steinberg::Vst::EditorHost::Wayland::Platform::instance ().run (cmdArgs);

	return 0;
}
#endif
