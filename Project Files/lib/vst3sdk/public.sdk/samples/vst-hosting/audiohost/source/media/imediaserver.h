//-----------------------------------------------------------------------------
// Flags       : clang-format auto
// Project     : VST SDK
//
// Category    : AudioHost
// Filename    : public.sdk/samples/vst-hosting/audiohost/source/media/imediaserver.h
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

#include <memory>
#include <string>
#include <vector>
#include <pluginterfaces/vst/vsttypes.h>

//----------------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {
using IOName = std::string;
using IONames = std::vector<IOName>;
using AudioClientName = std::string;

struct IAudioClient
{
	struct Buffers
	{
		float** inputs;
		int32_t numInputs;
		float** outputs;
		int32_t numOutputs;
		int32_t numSamples;
	};

	struct IOSetup
	{
		IONames inputs;
		IONames outputs;
	};

	virtual bool process (Buffers& buffers, int64_t continousFrames) = 0;
	virtual bool setSamplerate (SampleRate value) = 0;
	virtual bool setBlockSize (int32 value) = 0;
	virtual IOSetup getIOSetup () const = 0;

	virtual ~IAudioClient () {}
};

//----------------------------------------------------------------------------------
struct IMidiClient
{
	using MidiData = uint8_t;

	struct Event
	{
		MidiData type;
		MidiData channel;
		MidiData data0;
		MidiData data1;
		int64_t timestamp;
	};

	struct IOSetup
	{
		IONames inputs;
		IONames outputs;
	};

	virtual bool onEvent (const Event& event, int32_t port) = 0;
	virtual IOSetup getMidiIOSetup () const = 0;

	virtual ~IMidiClient () {}
};

//----------------------------------------------------------------------------------
struct IMediaServer
{
	virtual bool registerAudioClient (IAudioClient* client) = 0;
	virtual bool registerMidiClient (IMidiClient* client) = 0;

	virtual ~IMediaServer () {}
};

//----------------------------------------------------------------------------------
using IMediaServerPtr = std::shared_ptr<IMediaServer>;

IMediaServerPtr createMediaServer (const AudioClientName& name);
//----------------------------------------------------------------------------------
} // Vst
} // Steinberg
