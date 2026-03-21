//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : EditorHost
// Filename    : public.sdk/samples/vst-hosting/editorhost/source/platform/linux/platform.cpp
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

#include "public.sdk/samples/vst-hosting/editorhost/source/platform/iplatform.h"
#include "public.sdk/samples/vst-hosting/editorhost/source/platform/linux/irunloopimpl.h"
#include "public.sdk/samples/vst-hosting/editorhost/source/platform/linux/window.h"

#include <chrono>
#include <iostream>
#include <string>
#include <vector>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace EditorHost {

using namespace std::chrono;
using clock = high_resolution_clock;

//------------------------------------------------------------------------
static int pause (int milliSeconds)
{
	struct timeval timeOut;
	if (milliSeconds > 0)
	{
		timeOut.tv_usec = (milliSeconds % (unsigned long)1000) * 1000;
		timeOut.tv_sec = milliSeconds / (unsigned long)1000;

		select (0, nullptr, nullptr, nullptr, &timeOut);
	}
	return (milliSeconds > 0 ? milliSeconds : 0);
}

//------------------------------------------------------------------------
//------------------------------------------------------------------------
class Platform : public IPlatform
{
public:
	static Platform& instance ()
	{
		static Platform gInstance;
		return gInstance;
	}

	void setApplication (ApplicationPtr&& app) override;
	WindowPtr createWindow (const std::string& title, Size size, bool resizeable,
	                        const WindowControllerPtr& controller) override;

	void quit () override;
	void kill (int resultCode, const std::string& reason) override;

	FUnknown* getPluginFactoryContext () override;

	void run (const std::vector<std::string>& cmdArgs);

	static const int kMinEventLoopRate = 16; // 60Hz
private:
	void onWindowClosed (X11Window* window);
	void closeAllWindows ();
	void eventLoop ();

	ApplicationPtr application;
	Display* xDisplay {nullptr};

	std::vector<std::shared_ptr<X11Window>> windows;
};

//------------------------------------------------------------------------
#if !SMTG_EDITOR_HOST_USE_WAYLAND
IPlatform& IPlatform::instance ()
{
	return Platform::instance ();
}
#endif
//------------------------------------------------------------------------
void Platform::setApplication (ApplicationPtr&& app)
{
	application = std::move (app);
}

//------------------------------------------------------------------------
WindowPtr Platform::createWindow (const std::string& title, Size size, bool resizeable,
                                  const WindowControllerPtr& controller)
{
	auto window = X11Window::make (title, size, resizeable, controller, xDisplay,
	                               [this] (X11Window* window) { onWindowClosed (window); });
	if (window)
		windows.push_back (std::static_pointer_cast<X11Window> (window));
	return window;
}

//------------------------------------------------------------------------
void Platform::onWindowClosed (X11Window* window)
{
	for (auto it = windows.begin (); it != windows.end (); ++it)
	{
		if (it->get () == window)
		{
			windows.erase (it);
			break;
		}
	}
}

//------------------------------------------------------------------------
void Platform::closeAllWindows ()
{
	for (auto& w : windows)
	{
		w->close ();
	}
}

//------------------------------------------------------------------------
void Platform::quit ()
{
	static bool recursiveGuard = false;
	if (recursiveGuard)
		return;
	recursiveGuard = true;

	closeAllWindows ();

	if (application)
		application->terminate ();

	RunLoop::instance ().stop ();

	recursiveGuard = false;
}

//------------------------------------------------------------------------
void Platform::kill (int resultCode, const std::string& reason)
{
	std::cout << reason << "\n";
	exit (resultCode);
}

//------------------------------------------------------------------------
FUnknown* Platform::getPluginFactoryContext ()
{
	return &Steinberg::Linux::RunLoopImpl::instance ();
}

//------------------------------------------------------------------------
void Platform::run (const std::vector<std::string>& cmdArgs)
{
	// Connect to X server
	std::string displayName (getenv ("DISPLAY"));
	if (displayName.empty ())
		displayName = ":0.0";

	if ((xDisplay = XOpenDisplay (displayName.data ())) == nullptr)
	{
		return;
	}

	RunLoop::instance ().setDisplay (xDisplay);

	application->init (cmdArgs);

	eventLoop ();

	XCloseDisplay (xDisplay);
}

//------------------------------------------------------------------------
void Platform::eventLoop ()
{
	RunLoop::instance ().start ();
}

//------------------------------------------------------------------------
} // EditorHost
} // Vst
} // Steinberg

//------------------------------------------------------------------------
//------------------------------------------------------------------------
#if !SMTG_EDITOR_HOST_USE_WAYLAND
int main (int argc, char* argv[])
{
	std::vector<std::string> cmdArgs;
	for (int i = 1; i < argc; ++i)
		cmdArgs.push_back (argv[i]);

	Steinberg::Vst::EditorHost::Platform::instance ().run (cmdArgs);

	return 0;
}
#endif
