//-----------------------------------------------------------------------------
// Flags       : clang-format SMTGSequencer
// Project     : VST SDK
//
// Category    : VST3Inspector
// Filename    : public.sdk/samples/vst-hosting/inspectorapp/app.cpp
// Created by  : Steinberg, 01/2021
// Description : VST3 Inspector Application
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "vstgui/standalone/include/helpers/appdelegate.h"
#include "vstgui/standalone/include/helpers/windowlistener.h"
#include "vstgui/standalone/include/iapplication.h"
#include "window.h"

//------------------------------------------------------------------------
namespace VST3Inspector {

using namespace VSTGUI;
using namespace VSTGUI::Standalone;

//------------------------------------------------------------------------
struct App : Application::DelegateAdapter, WindowListenerAdapter
{
	App () : Application::DelegateAdapter ({"VST3Inspector", "1.0.0", VSTGUI_STANDALONE_APP_URI}) {}

	void finishLaunching () override
	{
		if (auto window = makeWindow ())
		{
			window->registerWindowListener (this);
			window->show ();
		}
	}

	void onClosed (const IWindow& window) override { IApplication::instance ().quit (); }
};

//------------------------------------------------------------------------
static Application::Init gAppDelegate (std::make_unique<App> ());

//------------------------------------------------------------------------
} // VST3Inspector
