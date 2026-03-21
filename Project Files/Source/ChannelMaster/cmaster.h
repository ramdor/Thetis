/*  cmaster.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2014-2019 Warren Pratt, NR0V

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at  

warren@wpratt.com

*/

#ifndef _cmaster_h
#define _cmaster_h
#include "cmbuffs.h"
#include "cmsetup.h"
#include "ilv.h"
#include "pipe.h"
#include "sync.h"
#include "txgain.h"
#include "vox.h"
#include "aamix.h"
#include "analyzers.h"

typedef struct _cmaster
{
	// radio structure
	int cmSTREAM;													// total number of input streams to set up
	int cmRCVR;														// number of receivers to set up
	int cmXMTR;														// number of transmitters to set up
	int cmSubRCVR;													// number of sub-receivers per receiver to set up
	int cmNspc;														// number of TYPES of special units
	int cmSPC[cmMAXspc];											// number of special units of each type
	int cmMAXInbound[cmMAXstream];									// maximum number of samples in a call to Inbound()
	int cmMAXInRate;												// maximum sample rate of an input stream
	int cmMAXAudioRate;												// maximum channel audio output rate (incl. rcvr and tx monitor audio)
	int cmMAXTxOutRate;												// maximum transmitter channel output sample rate

	// xcmaster
	double *in[cmMAXstream];										// xcmaster() input buffer
	int xcm_inrate[cmMAXstream];									// sample rate of xcmaster() input stream
	int xcm_insize[cmMAXstream];									// samples per xcmaster() input buffer
	int audio_outrate;												// sample rate of audio output stream
	int audio_outsize;												// samples per audio output buffer
	CMB pcbuff[cmMAXstream];										// pointers to input ring buffer structs
	CMB pdbuff[cmMAXstream];
	CMB pebuff[cmMAXstream];
	CMB pfbuff[cmMAXstream];
	CRITICAL_SECTION update[cmMAXstream];
	int aamix_inrates[cmMAXrcvr * cmMAXSubRcvr + cmMAXxmtr];
	void (*OutboundRx)(int id, int nsamples, double* buff);			// pointer to Outbound function called by aamix with rx audio from the global mixer
	void (*OutboundTx)(int id, int nsamples, double* buff);			// pointer to Outbound function called by ilv with xmtr samples from the interleaver
	void (*OutboundTCIRxIQ)(int id, int nsamples, double* buff);		// pointer to callback with receiver IQ samples
	void (*InboundTCITxAudio)(int nsamples, double* buff);			// pointer to callback to fill TX audio input
	volatile long tci_run;											// run TCI RX IQ/audio callbacks
	void (*VstRxProcess)(double* buff, int frames);					// pointer to callback to process RX audio through VST chain
	void (*VstTxProcess)(double* buff, int frames);					// pointer to callback to process TX audio through VST chain
	void (*VstInitialize)(void);									// pointer to callback to initialize VST hosting
	void (*VstShutdown)(void);										// pointer to callback to shut down VST hosting
	void (*VstCreateRxChain)(int sample_rate, int block_size);		// pointer to callback to create/reconfigure RX VST chain
	void (*VstCreateTxChain)(int sample_rate, int block_size);		// pointer to callback to create/reconfigure TX VST chain
	void (*VstDestroyRxChain)(void);								// pointer to callback to destroy RX VST chain
	void (*VstDestroyTxChain)(void);								// pointer to callback to destroy TX VST chain
	int	audioCodecId;
	ANALYZERS panalalloc;											// pointer to additional analyzer data structure
	
	// receivers
	struct _rcvr
	{
		int ch_outrate;												// rate at rcvr channel output = rcvr input to aamix
		int ch_outsize;												// size at rcvr channel output = rcvr input to aamix
		double* audio[cmMAXSubRcvr];								// audio buff, per subrx
		volatile long run_pan;										// run panadapter
		ANB panb;													// noiseblanker, per receiver
		NOB pnob;													// noiseblanker II, per receiver
	} rcvr[cmMAXrcvr];

	// transmitters
	struct _xmtr
	{
		int ch_outrate;												// xmtr output rate = tx channel output rate
		int ch_outsize;												// xmtr output size = tx channel output size
		double* out[3];												// output buff, per transmitter
		VOX pvox;													// vox, per transmitter
		void (__stdcall *pushvox)(int channel, int active);			// vox, per transmitter
		TXGAIN pgain;												// gain block, for Penelope power control & amp protect
		EER peer;													// eer block
		ILV pilv;													// interleave for EER
		AAMIX pavoxmix;												// anti-vox mixer
		volatile long use_tci_audio;								// use TCI TX audio instead of other TX sources
	} xmtr[cmMAXxmtr];

} cmaster, *CMASTER;

extern CMASTER pcm;

extern __declspec (dllexport) void xcmaster (int id);

extern void create_cmaster();

extern void destroy_cmaster();

extern __declspec (dllexport) void SendpOutboundRx (void (*Outbound)(int id, int nsamples, double* buff));
extern __declspec (dllexport) void SendpOutboundTx (void (*Outbound)(int id, int nsamples, double* buff));
extern __declspec (dllexport) void SendpOutboundTCIRxIQ (void (*Outbound)(int id, int nsamples, double* buff));
extern __declspec (dllexport) void SendpInboundTCITxAudio (void (*Inbound)(int nsamples, double* buff));
extern __declspec (dllexport) void SendpVstRxProcess (void (*Process)(double* buff, int frames));
extern __declspec (dllexport) void SendpVstTxProcess (void (*Process)(double* buff, int frames));
extern __declspec (dllexport) void SendpVstInitialize (void (*Init)(void));
extern __declspec (dllexport) void SendpVstShutdown (void (*Shutdown)(void));
extern __declspec (dllexport) void SendpVstCreateRxChain (void (*Create)(int sample_rate, int block_size));
extern __declspec (dllexport) void SendpVstCreateTxChain (void (*Create)(int sample_rate, int block_size));
extern __declspec (dllexport) void SendpVstDestroyRxChain (void (*Destroy)(void));
extern __declspec (dllexport) void SendpVstDestroyTxChain (void (*Destroy)(void));
extern __declspec (dllexport) void SetTCIRun (int active);
extern __declspec (dllexport) void SetTXTCIAudio (int txid, int active);

enum AudioCODEC
{
	HERMES = 0,														// audio codec chip in radio hardware unit
	ASIO   = 1,														// asio sound device on host
	WASAPI = 2														// wasapi sound device on host (to be implemented)
};

#endif
