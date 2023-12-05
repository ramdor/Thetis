/*  cmaster.c

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

#include "cmcomm.h"

cmaster cm  = {0};
CMASTER pcm = &cm;

// standard receiver
void create_rcvr()
{
	int i, j, rc;
	for (i = 0; i < pcm->cmRCVR; i++)
	{
		int in_id = inid (0, i);
		// audio buffer, one per subreceiver
		for (j = 0; j < pcm->cmSubRCVR; j++)
			pcm->rcvr[i].audio[j] = (double *) malloc0 (getbuffsize (pcm->cmMAXAudioRate) * sizeof (complex));
		// noise blanker
		pcm->rcvr[i].panb = create_anb (
			0,									// run
			pcm->xcm_insize[in_id],				// buffsize
			pcm->in[in_id],						// input buffer
			pcm->in[in_id],						// output buffer
			pcm->xcm_inrate[in_id],				// sample rate
			0.0001,								// tau
			0.0001,								// hangtime
			0.0001,								// advtime
			0.05,								// backtau
			30.0);								// threshold
		// noise blanker II
		pcm->rcvr[i].pnob = create_nob (
			0,									// run
			pcm->xcm_insize[in_id],				// buffsize
			pcm->in[in_id],						// input buffer
			pcm->in[in_id],						// output buffer
			pcm->xcm_inrate[in_id],				// sample rate
			0,									// mode
			0.0001,								// advslewtime
			0.0001,								// advtime
			0.0001,								// hangslewtime
			0.0001,								// hangtime
			0.025,								// max_imp_seq_time
			0.05,								// backtau
			30.0);								// threshold
		// dsp channel, one per subreceiver
		for (j = 0; j < pcm->cmSubRCVR; j++)
		{
			OpenChannel(
			chid (in_id, j),					// channel number
			pcm->xcm_insize[in_id],				// input buffer size
			4096,								// dsp buffer size
			pcm->xcm_inrate[in_id],				// input sample rate
			48000,								// dsp sample rate
			pcm->rcvr[i].ch_outrate,			// output sample rate
			0,									// channel type
			0,									// initial state
			0.010,								// tdelayup
			0.025,								// tslewup
			0.000,								// tdelaydown
			0.010,								// tslewdown
			1);									// block until output available
		}
		// displays
		//if (i == 0)
			//XCreateAnalyzer(i, &rc, 262144, 1, 3, "");	// sized for stitch = 3
		//else
			XCreateAnalyzer(i, &rc, 262144, 1, 1, "");
	}
	
}

void destroy_rcvr()
{
	int i, j;
	for (i = 0; i < pcm->cmRCVR; i++)
	{
		DestroyAnalyzer(i);
		for (j = 0; j < pcm->cmSubRCVR; j++)
			CloseChannel (chid (inid (0, i), j));
		destroy_nob (pcm->rcvr[i].pnob);
		destroy_anb (pcm->rcvr[i].panb);
		for (j = 0; j < pcm->cmSubRCVR; j++)
			_aligned_free (pcm->rcvr[i].audio[j]);
	}
}

// standard transmitter
void create_xmtr()
{
	int i, j, rc;
	int avoxmix_inrates[cmMAXrcvr * cmMAXSubRcvr];	// anti-vox mixer inrates
	for (i = 0; i < pcm->cmRCVR; i++)
			for (j = 0; j < pcm->cmSubRCVR; j++)
				avoxmix_inrates[pcm->cmSubRCVR * i + j] = pcm->rcvr[i].ch_outrate;	
	for (i = 0; i < pcm->cmXMTR; i++)
	{
		int in_id = inid (1, i);
		// output buffers (two because of EER)
		for (j = 0; j < 2; j++)
			pcm->xmtr[i].out[j] = (double *) malloc0 (getbuffsize (pcm->cmMAXTxOutRate) * sizeof (complex));
		// dexp - vox
		create_dexp (
			i,									// transmitter id, txid
			0,									// dexp initially set to OFF
			pcm->xcm_insize[in_id],				// input buffer size
			pcm->in[in_id],						// input buffer
			pcm->in[in_id],						// output buffer 
			pcm->xcm_inrate[in_id],				// sample-rate 
			0.01,								// detector smoothing time-constant 
			0.025,								// attack time 
			0.100,								// release time 
			1.000,								// hold time
			4.000,								// expansion ratio 
			0.750,								// hysteresis ratio
			0.050,								// attack threshold 
			256,								// 256 taps for side-channel filter
			0,									// BH-4 window for side-channel filter
			1000.0,								// low-cut for side-channel filter
			2000.0,								// high-cut for side-channel filter 
			0,									// side-channel filter initially set to OFF
			1,									// VOX initially set to ON
			1,									// audio delay initially set to ON
			0.060,								// audio delay set to 60ms
			pcm->xmtr[i].pushvox,				// pointer to VOX callback
			0,									// anti-vox 'run' flag
			pcm->audio_outsize,					// anti-vox data buffer size (complex samples)
			pcm->audio_outrate,					// anti-vox data sample-rate
			0.01,								// anti-vox gain
			0.01);								// anti-vox smoothing time-constant
		// anti-vox mixer
		pcm->xmtr[i].pavoxmix = (AAMIX) create_aamix (
			-1,									// no id, return a pointer
			i,									// outbound id to use when calling dexp-vox
			pcm->audio_outsize,					// aamix ring_insize (after aamix resamplers)
			pcm->audio_outsize,					// outsize (use audio size)
			pcm->cmRCVR * pcm->cmSubRCVR,		// maximum number of inputs
			0,									// 'active' inputs (data must flow for each)
			(1<<(pcm->cmRCVR*pcm->cmSubRCVR))-1,// inputs to currently mix ('what' variable)
			1.0,								// volume
			4096,								// ring buffer size
			avoxmix_inrates,					// sample rates of input streams
			pcm->audio_outrate,					// audio output sample rate (use audio rate)
			SendAntiVOXData,					// pointer to call sending output to Anti-VOX
			0.000,								// tdelayup
			0.000,								// tslewup
			0.000,								// tdelaydown
			0.000);								// tslewdown
		// dsp channel
		OpenChannel(
			chid (in_id, 0),					// channel number
			pcm->xcm_insize[in_id],				// input buffer size
			4096,								// dsp buffer size
			pcm->xcm_inrate[in_id],				// input sample rate
			96000,								// dsp sample rate
			pcm->xmtr[i].ch_outrate,			// output sample rate
			1,									// channel type
			0,									// initial state
			0.000,								// tdelayup
			0.010,								// tslewup
			0.000,								// tdelaydown
			0.010,								// tslewdown
			1);									// block until output is available
		// display
		XCreateAnalyzer (
			in_id, 
			&rc, 
			262144, 
			1, 
			1, 
			"");
		// Penelope gain
		pcm->xmtr[i].pgain = create_txgain(
			0,									// run fixed gain (Penelope)
			0,									// run protection gain for external amplifier
			pcm->xmtr[i].ch_outsize,			// size
			pcm->xmtr[i].out[0],				// in
			pcm->xmtr[i].out[0],				// out
			1.0,								// Igain (for fixed gain)
			1.0,								// Qgain (for fixed gain)
			0,									// ain4Value
			50);								// ADC Supply = 5.0V

		// eer
		pcm->xmtr[i].peer = create_eer (
			0,									// run
			pcm->xmtr[i].ch_outsize,			// size
			pcm->xmtr[i].out[0],				// in
			pcm->xmtr[i].out[0],				// out
			pcm->xmtr[i].out[1],				// outM
			pcm->xmtr[i].ch_outrate,			// sample rate
			1.0,								// mgain
			1.0,								// pgain
			0,									// rundelays
			0.0,								// mdelay
			0.0,								// pdelay
			1);									// amiq
		// interleave (for eer)
		pcm->xmtr[i].pilv = create_ilv(
			0,									// run
			1,									// id to use in Outbound call
			pcm->xmtr[i].ch_outsize,			// input buffer size
			2,									// maximum number of inputs
			3,									// which streams to interleave, one bit per stream
			pcm->OutboundTx);					// function to call with Outbound data
	}
}

void destroy_xmtr()
{
	int i, j;
	for (i = 0; i < pcm->cmXMTR; i++)
	{
		destroy_ilv (pcm->xmtr[i].pilv);
		destroy_eer (pcm->xmtr[i].peer);
		destroy_txgain (pcm->xmtr[i].pgain);
		DestroyAnalyzer (inid (1, i));
		CloseChannel (chid (inid (1, i), 0));
		destroy_aamix ((void *)(pcm->xmtr[i].pavoxmix), -1);
		destroy_dexp (i);
		for (j = 0; j < 2; j++)
			_aligned_free (pcm->xmtr[i].out[j]);
	}
}

void create_cmaster()
{
	int i, j;
	for (i = 0; i < pcm->cmSTREAM; i++)
	{
		InitializeCriticalSectionAndSpinCount(&pcm->update[i], 2500);	// 'update' critical section
		create_cmbuffs(													// input ring buffer
			i,															// stream number
			1,															// 'accept' data
			pcm->cmMAXInbound[i],										// maximum input size
			getbuffsize (pcm->cmMAXInRate),								// maximum output size
			pcm->xcm_insize[i]);										// ring outsize = xcmaster() insize	
		pcm->in[i] = (double *) malloc0 (getbuffsize (pcm->cmMAXInRate) * sizeof (complex));// input buffer
	}
	create_rcvr();														// standard receiver
	create_xmtr();														// standard transmitter
	{																	// audio mixer
		int active = 0;													// no inputs active initially
		int what = (1 << (pcm->cmRCVR * pcm->cmSubRCVR + pcm->cmXMTR)) - 1;	// mix all
		for (i = 0; i < pcm->cmRCVR; i++)
			for (j = 0; j < pcm->cmSubRCVR; j++)
				pcm->aamix_inrates[pcm->cmSubRCVR * i + j] = pcm->rcvr[i].ch_outrate;
		for (i = 0; i < pcm->cmXMTR; i++)
			pcm->aamix_inrates[pcm->cmRCVR * pcm->cmSubRCVR + i] = pcm->xmtr[i].ch_outrate;
		create_aamix (
			0,															// aamix id
			0,															// id to use in Outbound call
			pcm->audio_outsize,											// aamix ring_insize
			pcm->audio_outsize,											// outsize
			pcm->cmRCVR * pcm->cmSubRCVR + pcm->cmXMTR,					// number of inputs
			active,														// active inputs
			what,														// inputs to currently mix
			1.0,														// volume
			4096,														// ring buffer size
			pcm->aamix_inrates,											// sample rates of input streams
			pcm->audio_outrate,											// audio output sample rate
			pcm->OutboundRx,												// pointer to Outbound() call
			0.000,														// tdelayup
			0.010,														// tslewup
			0.000,														// tdelaydown
			0.010);														// tslewdown
	}
	create_cmasio();
	create_router(0);
}

void destroy_cmaster()
{
	int i;
	destroy_router(0, 0);
	destroy_cmasio();
	destroy_aamix  (0, 0);
	destroy_xmtr();
	destroy_rcvr();
	for (i = 0; i < pcm->cmSTREAM; i++)
	{
		DeleteCriticalSection (&pcm->update[i]);
		destroy_cmbuffs (i);
		_aligned_free (pcm->in[i]);
	}
}

PORT
void xcmaster (int stream)
{
	int error;
	EnterCriticalSection (&pcm->update[stream]);
	switch (stype (stream))
	{
	int rx, tx, j, k;

	case 0:  // standard receiver
		rx = rxid (stream);
		xpipe (stream, 0, pcm->in);
		xanb (pcm->rcvr[rx].panb);																// nb
		xnob (pcm->rcvr[rx].pnob);																// nb2
		Spectrum0  (_InterlockedAnd (&pcm->rcvr[rx].run_pan, 0xffffffff), rx, 0, 0,				// panadapter 
			pcm->in[stream]);
		for (j = 0; j < pcm->cmSubRCVR; j++)
			fexchange0 (chid (stream, j), pcm->in[stream], pcm->rcvr[rx].audio[j], &error);		// dsp
		xpipe (stream, 1, pcm->rcvr[rx].audio);
		for (j = 0; j < pcm->cmSubRCVR; j++)
		{
			xMixAudio (0, 0, chid (stream, j), pcm->rcvr[rx].audio[j]);							// mix audio
			for (k = 0; k < pcm->cmXMTR; k++)
				xMixAudio (pcm->xmtr[k].pavoxmix, -1, chid (stream, j), pcm->rcvr[rx].audio[j]);// send audio to anti-vox mixer(s)
		}
		// if (rx == 0) WriteAudio(30.0, 48000, 64, pcm->rcvr[0].audio[0], 3);
		break;

	case 1:  // standard transmitter
		tx = txid (stream);
		asioIN(pcm->in[stream]);
		xpipe (stream, 0, pcm->in);
		xdexp (tx);																				// vox-dexp
		fexchange0 (chid (stream, 0), pcm->in[stream], pcm->xmtr[tx].out[0], &error);			// dsp
		xpipe (stream, 1, pcm->xmtr[tx].out);
		// Spectrum0 (1, stream, 0, 0, pcm->xmtr[tx].out[0]);									// panadapter
		xMixAudio (0, 0, chid (stream, 0), pcm->xmtr[tx].out[0]);								// mix monitor audio
		// WriteAudio(30.0, 192000, 256, pcm->xmtr[0].out[0], 3);
		xtxgain (pcm->xmtr[tx].pgain);															// Gain for Penelope & amp_protect
		xeer (pcm->xmtr[tx].peer);																// EER transmission
		xilv(pcm->xmtr[tx].pilv, pcm->xmtr[tx].out);											// interleave EER, call Outbound()
		break;

	case 2:  // special 0, stitched upper panadapter
		xpipe (stream, 0, pcm->in);
		break;
	}
	LeaveCriticalSection (&pcm->update[stream]);
}

PORT
void SendpOutboundRx (void (*Outbound)(int id, int nsamples, double* buff))
{
	pcm->OutboundRx = Outbound;
	SetAAudioMixOutputPointer (0, 0, pcm->OutboundRx);
}

PORT
void SendpOutboundTx(void (*Outbound)(int id, int nsamples, double* buff))
{
	pcm->OutboundTx = Outbound;
	SetILVOutputPointer(0, pcm->OutboundTx);
}

PORT
void SetRunPanadapter (int id, int run)
{
	_InterlockedExchange (&pcm->rcvr[id].run_pan, run);
}

PORT
void SetXcmInrate (int in_id, int rate)	// 2014-12-18:  called for streams 0, 1, 3, 4 (RX).  Stream 2 (TX) called in CMCreateCMaster().
{
	int i, rx, tx, sp0;
	EnterCriticalSection (&pcm->update[in_id]);
	if (pcm->xcm_inrate[in_id] != rate)
	{
		pcm->xcm_inrate[in_id] = rate;
		pcm->xcm_insize[in_id] = getbuffsize (rate);
		SetCMRingOutsize(in_id, pcm->xcm_insize[in_id]);
		switch (stype (in_id))
		{
		case 0:  // receiver
			rx = rxid (in_id);
			SetRCVRANBBuffsize (0, rx, pcm->xcm_insize[in_id]);					// set anb input size
			SetRCVRANBSamplerate (0, rx, rate);									// set anb input rate
			SetRCVRNOBBuffsize (0, rx, pcm->xcm_insize[in_id]);					// set nob input size
			SetRCVRNOBSamplerate (0, rx, rate);									// set nob input rate
			// set display (currently in C#)
			for (i = 0; i < pcm->cmSubRCVR; i++)
			{
				SetInputSamplerate (chid (in_id, i), rate);						// dsp channel input rate
				SetInputBuffsize (chid (in_id, i), pcm->xcm_insize[in_id]);		// dsp channel input size
			}
			// PIPE - set wave player (leave in C# since player is there)
			// PIPE - set wave recorder (leave in C# since player is there)
			if (rx == 0) SetSiphonInsize (rx, pcm->xcm_insize[in_id]);			// PIPE - set siphon for phase2 display, RX1 only
			SetIVACiqSizeAndRate (rx, pcm->xcm_insize[in_id], pcm->xcm_inrate[in_id]);	// PIPE - set vacOUT size and rate for IQ data
			break;
		case 1:  // transmitter
			tx = txid (in_id);
			SetInputSamplerate (chid (in_id, 0), rate);							// dsp channel input rate
			SetInputBuffsize (chid (in_id, 0), pcm->xcm_insize[in_id]);			// dsp channel input size
			//SetTXAVoxSize (tx, pcm->xcm_insize[in_id]);							// VOX size
			SetDEXPSize (tx, pcm->xcm_insize[in_id]);							// vox-dexp size
			SetDEXPRate (tx, rate);												// vox-dexp rate
			// PIPE - set wave player, rcvr0 (leave in C# since player is there)
			// PIPE - set wave player, rcvr1 (leave in C# since player is there)
			// PIPE - set wave recorder, rcvr0 (leave in C# since recorder is there)
			// PIPE - set wave recorder, rcvr1 (leave in C# since recorder is there)
			SetIVACmicSize (0, pcm->xcm_insize[in_id]);							// PIPE - set vacIN0 input size
			SetIVACmicRate (0, rate);											// PIPE - set vacIN0 input rate
			SetIVACmicSize (1, pcm->xcm_insize[in_id]);							// PIPE - set vacIN1 input size
			SetIVACmicRate (1, rate);											// PIPE - set vacIN1 input rate
			break;
		case 2:	// special0 for stitched rx display
			sp0 = sp0id (in_id);
			SetRCVRANBBuffsize (2, sp0, pcm->xcm_insize[in_id]);				// PIPE - set anb input size
			SetRCVRANBSamplerate (2, sp0, rate);								// PIPE - set anb input rate
			SetRCVRNOBBuffsize (2, sp0, pcm->xcm_insize[in_id]);				// PIPE - set nob input size
			SetRCVRNOBSamplerate (2, sp0, rate);								// PIPE - set nob input rate
			break;
		}
	}
	LeaveCriticalSection (&pcm->update[in_id]);
}

PORT
void SetCMAudioOutrate (int in_id, int rate)		// 2014-11-24:  NOT called by console because this is fixed at 48K by the
{													//   protocol and that is the default rate being set at creation.
	EnterCriticalSection (&pcm->update[in_id]);
	pcm->audio_outrate = rate;
	pcm->audio_outsize = getbuffsize (rate);
	SetAAudioOutRate     (0, 0, pcm->audio_outrate);
	SetAAudioRingInsize  (0, 0, pcm->audio_outsize);
	SetAAudioRingOutsize (0, 0, pcm->audio_outsize);
	LeaveCriticalSection (&pcm->update[in_id]);
}

PORT
void SetRcvrChannelOutrate (int rcvr_id, int rate, int state)	// 2014-12-18:  NOT called by console as values are set at creation and
{																//   not changed.
	int j;
	int in_id = inid (0, rcvr_id);
	int mix_in_id;
	EnterCriticalSection (&pcm->update[in_id]);
	pcm->rcvr[rcvr_id].ch_outrate = rate;
	pcm->rcvr[rcvr_id].ch_outsize = getbuffsize (rate);
	for (j = 0; j < pcm->cmSubRCVR; j++)
	{
		SetOutputSamplerate(chid(in_id, j), rate);							// set DSP Channel output rate (sets size too)
		mix_in_id = mixinid (in_id, j);										// set audio mixer
		SetAAudioMixState (0, 0, mix_in_id, 0);								//	.Make this stream INACTIVE in AAMixer
		SetAAudioStreamRate (0, 0, mix_in_id, rate);						//	.Set Mixer input rate (sets size too)
		SetAAudioMixState (0, 0, mix_in_id, state);							//	.Conditionally make this stream ACTIVE in AAMixer
	}
	SetIVACaudioRate (rcvr_id, rate);
	SetIVACaudioSize (rcvr_id, pcm->rcvr[rcvr_id].ch_outsize);
	// PIPE - set Scope, PSDR RX1 Only (leave in C# since scope is there)
	// PIPE - set wave recorder, rcvr0 (leave in C# since recorder is there)
	// PIPE - set wave recorder, rcvr1 (leave in C# since recorder is there) 
	LeaveCriticalSection (&pcm->update[in_id]);
}

PORT
void SetXmtrChannelOutrate (int xmtr_id, int rate, int state)	// 2014-11-24:  Called by console when TX rate is set
{
	int in_id = inid (1, xmtr_id);
	int mix_in_id = mixinid (inid (1, xmtr_id), 0);
	int size = getbuffsize (rate);
	EnterCriticalSection (&pcm->update[in_id]);
	pcm->xmtr[xmtr_id].ch_outrate = rate;									// channel out_rate
	pcm->xmtr[xmtr_id].ch_outsize = size;									// channel out_size
	SetOutputSamplerate(chid(in_id, 0), rate);								// set DSP Channel output rate (sets size too)
	// set TX display (currently in C#)
																			// set audio mixer
	SetAAudioMixState (0, 0, mix_in_id, 0);									//	.Make this stream INACTIVE in AAMixer
	SetAAudioStreamRate (0, 0, mix_in_id, rate);							//	.Set Mixer input rate (sets size too)
	SetAAudioMixState (0, 0, mix_in_id, state);								//	.Conditionally make this stream ACTIVE in AAMixer
																			//
	SetTXGainSize(pcm->xmtr[xmtr_id].pgain, size);							// set size for Penelope Gain Block
	pSetEERSamplerate(pcm->xmtr[xmtr_id].peer, rate);						// set rate for EER
	pSetEERSize(pcm->xmtr[xmtr_id].peer, size);								// set size for EER
	pSetILVInsize(pcm->xmtr[xmtr_id].pilv, size);							// set size for Interleave & output
	// PIPE - set Scope (leave in C# since scope is there)
	SetIVACtxmonRate (0, rate);												// set vacOUT0 rate for tx monitor
	SetIVACtxmonSize (0, size);												// set vacOUT0 size for tx monitor
	SetIVACtxmonRate (1, rate);												// set vacOUT1 rate for tx monitor
	SetIVACtxmonSize (1, size);												// set vacOUT1 size for tx monitor
	// PIPE - set Wave Recorder (leave in C# since recorder is there)
	LeaveCriticalSection (&pcm->update[in_id]);
}

PORT
void SetAntiVOXSourceStates (int txid, int streams, int states)
{
	void* a = (void *)pcm->xmtr[txid].pavoxmix;
	SetAAudioMixStates (a, -1, streams, states);
}

PORT
void SetAntiVOXSourceWhat (int txid, int stream, int state)
{
	void* a = (void *)pcm->xmtr[txid].pavoxmix;
	SetAAudioMixWhat (a, -1, stream, state);
}
