//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : AudioHost
// Filename    : public.sdk/samples/vst-hosting/audiohost/source/audiohost.cpp
// Created by  : Steinberg 09.2016
// Description : Audio Host Example for VST 3
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#include "public.sdk/samples/vst-hosting/audiohost/source/audiohost.h"
#include "public.sdk/samples/vst-hosting/audiohost/source/platform/appinit.h"
#include "public.sdk/source/vst/hosting/hostclasses.h"
#include "public.sdk/source/vst/utility/stringconvert.h"
#include "base/source/fcommandline.h"
#include "pluginterfaces/base/funknown.h"
#include "pluginterfaces/base/fstrdefs.h"
#include "pluginterfaces/gui/iplugview.h"
#include "pluginterfaces/gui/iplugviewcontentscalesupport.h"
#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include "pluginterfaces/vst/ivsteditcontroller.h"
#include "pluginterfaces/vst/vsttypes.h"
#include <cstdio>
#include <iostream>

#if WIN32
#include "windows.h"
#include <wtypes.h>
#endif

//------------------------------------------------------------------------
namespace Steinberg {
FUnknown* gStandardPluginContext = new Vst::HostApplication ();

namespace Vst {
namespace AudioHost {
static AudioHost::AppInit gInit (std::make_unique<App> ());

//------------------------------------------------------------------------
App::~App () noexcept
{
}

//------------------------------------------------------------------------
void App::startAudioClient (const std::string& path, VST3::Optional<VST3::UID> effectID,
                            uint32 flags)
{
	std::string error;
	module = VST3::Hosting::Module::create (path, error);
	if (!module)
	{
		std::string reason = "Could not create Module for file:";
		reason += path;
		reason += "\nError: ";
		reason += error;
		// EditorHost::IPlatform::instance ().kill (-1, reason);
        return;
	}
	auto factory = module->getFactory ();
	for (auto& classInfo : factory.classInfos ())
	{
		if (classInfo.category () == kVstAudioEffectClass)
		{
			if (effectID)
			{
				if (*effectID != classInfo.ID ())
					continue;
			}
			plugProvider = owned (new PlugProvider (factory, classInfo, true));
			break;
		}
	}
	if (!plugProvider)
	{
		std::string error;
		if (effectID)
			error =
			    "No VST3 Audio Module Class with UID " + effectID->toString () + " found in file ";
		else
			error = "No VST3 Audio Module Class found in file ";
		error += path;
		// EditorHost::IPlatform::instance ().kill (-1, error);
        return;
	}

	OPtr<IComponent> component = plugProvider->getComponent ();
	OPtr<IEditController> controller = plugProvider->getController ();
	auto midiMapping = U::cast<IMidiMapping> (controller);

	//! TODO: Query the plugProvider for a proper name which gets displayed in JACK.
	vst3Processor = AudioClient::create ("VST 3 SDK", component, midiMapping);
}

//------------------------------------------------------------------------
void App::init (const std::vector<std::string>& cmdArgs)
{
	if (cmdArgs.empty ())
	{
		/*auto helpText = R"(
		usage: AudioHost pluginPath
		)";
		*/
		return;
	}

	VST3::Optional<VST3::UID> uid;
	uint32 flags {};

	startAudioClient (cmdArgs.back (), std::move (uid), flags);
}

//------------------------------------------------------------------------
void App::terminate ()
{
}

//------------------------------------------------------------------------
} // EditorHost
} // Vst
} // Steinberg

//------------------------------------------------------------------------
#if WIN32
int wmain (int argc, wchar_t* argv[])
{
	std::vector<std::string> cmdArgs;
	for (int i = 1; i < argc; ++i)
		cmdArgs.push_back (Steinberg::Vst::StringConvert::convert (Steinberg::wscast (argv[i])));

	Steinberg::Vst::AudioHost::gInit.app->init (cmdArgs);

	std::cout << "Press <enter> to continue . . .";
	std::getchar ();

	return 0;
}
#else
int main (int argc, char* argv[])

{
	std::vector<std::string> cmdArgs;
	for (int i = 1; i < argc; ++i)
		cmdArgs.push_back (argv[i]);

	Steinberg::Vst::AudioHost::gInit.app->init (cmdArgs);

	std::cout << "Press <enter> to continue . . .";
	std::getchar ();

	return 0;
}
#endif
