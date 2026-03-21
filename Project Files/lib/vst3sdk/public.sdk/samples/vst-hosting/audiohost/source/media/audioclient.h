//-----------------------------------------------------------------------------
// Flags       : clang-format auto
// Project     : VST SDK
//
// Category    : AudioHost
// Filename    : public.sdk/samples/vst-hosting/audiohost/source/audioclient.h
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

#include "public.sdk/samples/vst-hosting/audiohost/source/media/imediaserver.h"
#include "public.sdk/samples/vst-hosting/audiohost/source/media/iparameterclient.h"
#include "public.sdk/source/vst/hosting/eventlist.h"
#include "public.sdk/source/vst/hosting/parameterchanges.h"
#include "public.sdk/source/vst/hosting/processdata.h"
#include "pluginterfaces/vst/ivstaudioprocessor.h"
#include <array>

//------------------------------------------------------------------------
namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
class IMidiMapping;
class IComponent;

enum
{
	kMaxMidiMappingBusses = 4,
	kMaxMidiChannels = 16
};
using Controllers = std::vector<int32>;
using Channels = std::array<Controllers, kMaxMidiChannels>;
using Busses = std::array<Channels, kMaxMidiMappingBusses>;
using MidiCCMapping = Busses;

//------------------------------------------------------------------------
using AudioClientPtr = std::shared_ptr<class AudioClient>;
//------------------------------------------------------------------------
class AudioClient : public IAudioClient, public IMidiClient, public IParameterClient
{
public:
//--------------------------------------------------------------------
	using Name = std::string;

	AudioClient ();
	~AudioClient () override;

	static AudioClientPtr create (const Name& name, IComponent* component,
	                              IMidiMapping* midiMapping);

	// IAudioClient
	bool process (Buffers& buffers, int64_t continousFrames) override;
	bool setSamplerate (SampleRate value) override;
	bool setBlockSize (int32 value) override;
	IAudioClient::IOSetup getIOSetup () const override;

	// IMidiClient
	bool onEvent (const Event& event, int32_t port) override;
	IMidiClient::IOSetup getMidiIOSetup () const override;

	// IParameterClient
	void setParameter (ParamID id, ParamValue value, int32 sampleOffset) override;

	bool initialize (const Name& name, IComponent* component, IMidiMapping* midiMapping);

//--------------------------------------------------------------------
private:
	void createLocalMediaServer (const Name& name);
	void terminate ();
	void updateBusBuffers (Buffers& buffers, HostProcessData& processData);
	void initProcessData ();
	void initProcessContext ();
	bool updateProcessSetup ();
	void preprocess (Buffers& buffers, int64_t continousFrames);
	void postprocess (Buffers& buffers);
	bool isPortInRange (int32 port, int32 channel) const;
	bool processVstEvent (const IMidiClient::Event& event, int32 port);
	bool processParamChange (const IMidiClient::Event& event, int32 port);

	SampleRate sampleRate = 0;
	int32 blockSize = 0;
	HostProcessData processData;
	ProcessContext processContext;
	EventList eventList;
	ParameterChanges inputParameterChanges;
	IComponent* component = nullptr;
	ParameterChangeTransfer paramTransferrer;

	MidiCCMapping midiCCMapping;
	IMediaServerPtr mediaServer;
	bool isProcessing = false;

	Name name;
};

//------------------------------------------------------------------------
} // Vst
} // Steinberg
