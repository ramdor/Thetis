//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/linux/wayland/runloop.cpp
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

#include "runloop.h"
#include <algorithm>
#include <cstdio>
#include <iostream>
#include <memory>
#include <poll.h>
#include <sys/timerfd.h>
#include <unistd.h>
#include <vector>
#include <wayland-client.h>
#include <wayland-server.h>

namespace Steinberg::Vst::EditorHost::Wayland {

//------------------------------------------------------------------------
static uint64_t toSeconds (uint64_t milliseconds)
{
	return milliseconds / 1000;
}

//------------------------------------------------------------------------
static uint64_t toNanoSecs (uint64_t milliseconds)
{
	return milliseconds * 1000000;
}

//------------------------------------------------------------------------
static auto interval_timerfd_create (Linux::TimerInterval milliseconds) -> int
{
	auto timer_fd = timerfd_create (CLOCK_REALTIME, 0);

	if (timer_fd == -1)
	{
		std::cerr << "Failed to create a new timer." << std::endl;
		return timer_fd;
	}

	const auto secs = toSeconds (milliseconds);
	const auto nanoSecs = toNanoSecs (milliseconds % 1000);

	struct itimerspec timerValue;
	timerValue.it_value.tv_sec = secs;
	timerValue.it_value.tv_nsec = nanoSecs;
	timerValue.it_interval.tv_sec = secs;
	timerValue.it_interval.tv_nsec = nanoSecs;

	if (timerfd_settime (timer_fd, 0, &timerValue, NULL) == -1)
	{
		std::cerr << "Failed to set a timer specification." << std::endl;
		return -1;
	}

	return timer_fd;
}

//------------------------------------------------------------------------
// IntervalTimer
//------------------------------------------------------------------------
struct IntervalTimer : std::enable_shared_from_this<IntervalTimer>
{
	using ITimerHandler = Linux::ITimerHandler;

	IntervalTimer (ITimerHandler* handler) : handler (handler) {}

	auto onFDIsSet () -> void
	{
		if (handler)
			handler->onTimer ();
	}

	ITimerHandler* getHandler () const { return handler; }

private:
	IPtr<ITimerHandler> handler;
};

//------------------------------------------------------------------------
// RunLoop::Impl
//------------------------------------------------------------------------
struct RunLoop::Impl
{
	using FileDescriptors = std::vector<int>;
	using Timers = std::vector<std::pair<std::shared_ptr<IntervalTimer>, FileDescriptor>>;
	using EventHandlers = std::vector<std::pair<IPtr<IEventHandler>, FileDescriptor>>;
	using PollRequests = std::vector<pollfd>;

	wl_display* display {nullptr};
	Timers timers;

	EventHandlers eventHandlers;
	PollRequests pfds;

	bool running {false};

	static auto collectPollRequests (const FileDescriptors& fds, const EventHandlers& handlers,
	                                 const Timers& timers, PollRequests& pfds) -> void;

	static auto dispatchPolledEvents (const PollRequests& pfds, EventHandlers& handlers,
	                                  Timers& timers) -> void;

	void run ();
	void stop () { running = false; }
};

//------------------------------------------------------------------------
// RunLoop::Impl
//------------------------------------------------------------------------
auto RunLoop::Impl::collectPollRequests (const FileDescriptors& fds, const EventHandlers& handlers,
                                         const Timers& timers, PollRequests& pfds) -> void
{
	pfds.clear ();

	for (const auto& fd : fds)
	{
		pfds.push_back ({/* .fd = */ fd,
		                 /* .events = */ POLLIN,
		                 /* .revents = */ 0});
	}

	for (const auto& handler : handlers)
	{
		pfds.push_back ({/* .fd = */ handler.second,
		                 /* .events = */ POLLIN,
		                 /* .revents = */ 0});
	};

	for (const auto& timer : timers)
	{
		pfds.push_back ({/* .fd = */ timer.second,
		                 /* .events = */ POLLIN,
		                 /* .revents = */ 0});
	};
}

//------------------------------------------------------------------------
auto RunLoop::Impl::dispatchPolledEvents (const PollRequests& pfds, EventHandlers& handlers,
                                          Timers& timers) -> void
{
	for (const auto& pfd : pfds)
	{
		auto found = [&] (const auto& el) { return el.second == pfd.fd; };

		if (pfd.revents & POLLIN)
		{
			auto it = std::find_if (handlers.begin (), handlers.end (), found);
			if (it != handlers.end ())
			{
				it->first->onFDIsSet (it->second);
				continue;
			}

			auto iter = std::find_if (timers.begin (), timers.end (), found);
			if (iter != timers.end ())
			{
				uint64_t exp;
				const int timer_fd = pfd.fd;
				// 'read' must be called to clear the file descriptor's events
				ssize_t s = read (timer_fd, &exp, sizeof (uint64_t));
				iter->first->onFDIsSet ();
			}
		}
	}
}

//------------------------------------------------------------------------
void RunLoop::Impl::run ()
{
	running = true;

	const auto displayFd = wl_display_get_fd (display);
	const FileDescriptors fds = {displayFd};

	while (running)
	{
		collectPollRequests (fds, eventHandlers, timers, pfds);

		while (wl_display_prepare_read (display) != 0)
			wl_display_dispatch_pending (display);

		wl_display_flush (display);

		// -1: 'poll' blocks until one of the provided polling requests has events that occurred
		const auto res = poll (pfds.data (), pfds.size (), -1);
		// Afterwards read all events for the display, if there are any
		if (res == -1 || pfds[0].revents == 0)
			wl_display_cancel_read (display);
		else if (pfds[0].revents & POLLIN)
			wl_display_read_events (display);

		dispatchPolledEvents (pfds, eventHandlers, timers);
	}
}

//------------------------------------------------------------------------
// RunLoop
//------------------------------------------------------------------------
RunLoop::RunLoop (wl_display* display)
{
	impl = std::make_unique<Impl> ();
	impl->display = display;
}

//------------------------------------------------------------------------
RunLoop::~RunLoop () noexcept = default;

//------------------------------------------------------------------------
void RunLoop::run ()
{
	impl->run ();
}

//------------------------------------------------------------------------
void RunLoop::stop ()
{
	impl->stop ();
}

//------------------------------------------------------------------------
tresult PLUGIN_API RunLoop::registerEventHandler (IEventHandler* handler, FileDescriptor fd)
{
	if (!handler)
		return kInvalidArgument;

	impl->eventHandlers.push_back ({handler, fd});
	return kResultTrue;
}

//------------------------------------------------------------------------
tresult PLUGIN_API RunLoop::unregisterEventHandler (IEventHandler* handler)
{
	auto it = std::find_if (impl->eventHandlers.begin (), impl->eventHandlers.end (),
	                        [&] (const auto& el) { return el.first == handler; });
	if (it != impl->eventHandlers.end ())
	{
		impl->eventHandlers.erase (it);
		return kResultTrue;
	}
	return kInvalidArgument;
}

//------------------------------------------------------------------------
tresult PLUGIN_API RunLoop::registerTimer (ITimerHandler* handler, TimerInterval milliseconds)
{
	auto src = std::make_shared<IntervalTimer> (handler);
	auto timer_fd = interval_timerfd_create (milliseconds);
	if (timer_fd == -1)
		return kInternalError;

	impl->timers.push_back (std::make_pair (src, timer_fd));
	return kResultTrue;
}

//------------------------------------------------------------------------
tresult PLUGIN_API RunLoop::unregisterTimer (ITimerHandler* handler)
{
	auto it = std::find_if (impl->timers.begin (), impl->timers.end (),
	                        [&] (const auto& el) { return el.first->getHandler () == handler; });
	if (it != impl->timers.end ())
	{
		const auto timer_fd = it->second;
		::close (timer_fd);
		impl->timers.erase (it);
		return kResultTrue;
	}
	return kResultFalse;
}

//------------------------------------------------------------------------
} // Steinberg::Vst::EditorHost::Wayland
