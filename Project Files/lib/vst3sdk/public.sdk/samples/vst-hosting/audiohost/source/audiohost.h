//-----------------------------------------------------------------------------
// Flags       : clang-format auto
// Project     : VST SDK
//
// Category    : AudioHost
// Filename    : public.sdk/samples/vst-hosting/audiohost/source/audiohost.h
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

#pragma once

#include "public.sdk/samples/vst-hosting/editorhost/source/platform/iapplication.h"
#include "public.sdk/samples/vst-hosting/audiohost/source/media/audioclient.h"
#include "public.sdk/source/vst/hosting/module.h"
#include "public.sdk/source/vst/hosting/plugprovider.h"
#include "public.sdk/source/vst/utility/optional.h"

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
namespace AudioHost {

//------------------------------------------------------------------------
class App : public EditorHost::IApplication
{
public:
	~App () noexcept override;
	void init (const std::vector<std::string>& cmdArgs) override;
	void terminate () override;

private:
	enum OpenFlags
	{
	};
	void startAudioClient (const std::string& path, VST3::Optional<VST3::UID> effectID,
	                       uint32 flags);

	VST3::Hosting::Module::Ptr module {nullptr};
	IPtr<PlugProvider> plugProvider {nullptr};
	AudioClientPtr vst3Processor;
};

//------------------------------------------------------------------------
} // AudioHost
} // Vst
} // Steinberg
