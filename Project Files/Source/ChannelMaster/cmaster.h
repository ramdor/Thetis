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
	void (*Outbound)(int id, int nsamples, double* buff);			// pointer to Outbound function
																	// this function is called by xmtr and aamix
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
		double* out[2];												// output buff, per transmitter
		VOX pvox;													// vox, per transmitter
		void (__stdcall *pushvox)(int channel, int active);			// vox, per transmitter
		TXGAIN pgain;												// gain block, for Penelope power control & amp protect
		EER peer;													// eer block
		ILV pilv;													// interleave for EER
		AAMIX pavoxmix;												// anti-vox mixer
	} xmtr[cmMAXxmtr];

} cmaster, *CMASTER;

extern CMASTER pcm;

extern __declspec (dllexport) void xcmaster (int id);

extern void create_cmaster();

extern void destroy_cmaster();

extern __declspec (dllexport) void SendpOutbound (void (*Outbound)(int id, int nsamples, double* buff));

#endif