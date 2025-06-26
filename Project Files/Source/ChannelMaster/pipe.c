/*  pipe.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2014 Warren Pratt, NR0V

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

pipe pip  = {0};
PIPE ppip = &pip;

void create_spc0()
{
	int i;
	for (i = 0; i < pcm->cmSPC[0]; i++)
	{
		int in_id = inid (2, i);
		// noise blanker
		ppip->spc0[i].panb = create_anb (
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
		ppip->spc0[i].pnob = create_nob (
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
	}
}

void destroy_spc0()
{
	int i;
	for (i = 0; i < pcm->cmSPC[0]; i++)
	{
		destroy_anb (ppip->spc0[i].panb);
		destroy_nob (ppip->spc0[i].pnob);
	}
}

void create_pipe()
{
	int i;
	create_siphonEXT(							// siphon for phase2 display for rcvr[0]
		0,										// id
		1,										// run
		pcm->xcm_insize[inid (0, 0)],			// buffer size
		512,									// sipsize
		512,									// fftsize
		0);										// specmode
	(*pip.create_Scope)(0);						// scope display for rcvr[0]
	ppip->rbuff = (double **) malloc0 (pcm->cmRCVR * sizeof (double *));
	for (i = 0; i < pcm->cmRCVR; i++)
	{
		ppip->rbuff[i] = (double *) malloc0 (pcm->rcvr[i].ch_outsize * sizeof (complex));
		(*pip.create_WavePlay)(i);
		(*pip.create_WaveRecord)(i);
		create_ivac(
			i,									// id
			0,									// run
			0,									// iq_type
			0,									// stereo
			pcm->xcm_inrate[inid(0, i)],		// rx i-q rate
			pcm->xcm_inrate[inid(1, 0)],		// mic rate
			pcm->rcvr[i].ch_outrate,			// receiver audio rate
			pcm->xmtr[0].ch_outrate,			// tx monitor rate
			48000,								// vac rate
			pcm->xcm_insize[inid(1, 0)],		// mic buffer size
			pcm->xcm_insize[inid(0, i)],		// iq buffer size
			pcm->rcvr[i].ch_outsize,			// receiver audio buffer size
			pcm->xmtr[0].ch_outsize,			// tx monitor buffer size
			1024);								// vac size
		ppip->rcvr[i].scope_run = 0;			// scope run
		ppip->rcvr[i].playwave_run = 0;			// playwave run
		ppip->rcvr[i].recordwave_run = 0;		// recordwave run
	}
	create_spc0();
}

void destroy_pipe()
{
	int i;
	destroy_spc0();
	for (i = 0; i < pcm->cmRCVR; i++)
	{
		_aligned_free (ppip->rbuff[i]);
		destroy_ivac (i);
	}
	_aligned_free (ppip->rbuff);
	destroy_siphonEXT (0);
}

void xplaywave(int rx, int state, double* data)
{
	// prevent 1000's of calls when not being used
	if (ppip->rcvr[rx].playwave_run)
	{
		(*pip.rcvr[rx].xplaywave)(state, data);
	}
}

void xrecordwave(int rx, int state, int pos, double* data)
{
	// prevent 1000's of calls when not being used
	if (ppip->rcvr[rx].recordwave_run)
	{
		(*pip.rcvr[rx].xrecordwave)(state, pos, data);
	}
}

void xscope(int rx, int state, double* data)
{
	// prevent 1000's of calls when not being used
	if (ppip->rcvr[rx].scope_run)
	{
		(*pip.rcvr[rx].xscope)(state, data);
	}
}

void xpipe (int stream, int pos, double** buffs)
{
	double* buff = buffs[stream];
	int i, j;
	int rx, tx, sp0;
	int st = stype (stream);
	if      (st == 0) rx  = rxid (stream);
	else if (st == 1) tx  = txid (stream);
	else if (st == 2) sp0 = sp0id (stream);

	if (stream == 0)	// PowerSDR RX1
	{
		switch (pos)
		{
		case 0:	// IQ data
			xplaywave(rx, 0, buff);																// wav player
			xrecordwave(rx, 0, 0, buff);														// wav recorder
			xsiphonEXT(rx, buff);																// siphon for phase2 display
			Spectrum0(_InterlockedAnd(&pip.rcvr[0].top_pan3_run, 0xffffffff), rx, 1, 0, buff);	// stitched pan
			xvacOUT(rx, 0, buff);																// data to VAC
			break;
		case 1: // Audio data
			memcpy (ppip->rbuff[rx], buffs[0], pcm->rcvr[rx].ch_outsize * sizeof (complex));
			for (i = 1; i < pcm->cmSubRCVR; i++)
				for (j = 0; j < 2 * pcm->rcvr[rx].ch_outsize; j++)
					ppip->rbuff[rx][j] += buffs[i][j];
			xscope(rx, 0, ppip->rbuff[rx]);														// scope
			xvacOUT(rx, 1, ppip->rbuff[rx]);													// data to VAC
			xrecordwave(rx, 0, 1, ppip->rbuff[rx]);												// wav recorder
			break;
		}
	}
	else if (stream < pcm->cmRCVR)	// other PowerSDR receivers
	{
		switch (pos)
		{
		case 0: // IQ data
			xplaywave(rx, 0, buff);																// wav player
			xrecordwave(rx, 0, 0, buff);														// wav recorder
			xvacOUT(rx, 0, buff);																// data to VAC
			break;
		case 1: // Audio data
			memcpy (ppip->rbuff[rx], buffs[0], pcm->rcvr[rx].ch_outsize * sizeof (complex));
			for (i = 1; i < pcm->cmSubRCVR; i++)
				for (j = 0; j < 2 * pcm->rcvr[rx].ch_outsize; j++)
					ppip->rbuff[rx][j] += buffs[i][j];
			xvacOUT(rx, 1, ppip->rbuff[rx]);													// data to VAC
			xrecordwave(rx, 0, 1, ppip->rbuff[rx]);												// wav recorder
			break;
		}
	}
	else if (stream == inid (1, 0))	// PowerSDR single transmitter
	{
		switch (pos)
		{
		case 0: // MIC data
			xplaywave(0, 1, buff);																// wav player 0
			xplaywave(1, 1, buff);																// wav player 1
			if (pip.xmtr[0].txvac == 0)  { xvacIN(0, buff, 0);  xvacIN(1, buff, 1); }
			if (pip.xmtr[0].txvac == 1)  { xvacIN(1, buff, 0);  xvacIN(0, buff, 1); }
			xrecordwave(0, 1, 0, buff);															// wav recorder 0 //[2.10.3.6]MW0LGE moved after vac
			xrecordwave(1, 1, 0, buff);															// wav recorder 1
			break;
		case 1: // IQ data
			xscope(0, 1, buffs[2]);																// scope
			xvacOUT(0, 2, buffs[2]);															// data to VAC 0
			xvacOUT(1, 2, buffs[2]);															// data to VAC 1
			xrecordwave(0, 1, 1, buffs[2]);														// wav recorder 0
			xrecordwave(1, 1, 1, buffs[2]);														// wav recorder 1
			break;
		}
	}
	else if (stream == inid(2, 0))	// PowerSDR Stitched Rcvrs, Left side
	{
		xanb(ppip->spc0[sp0].panb);									// nb
		xnob(ppip->spc0[sp0].pnob);									// nb II
		Spectrum0 (_InterlockedAnd (&pip.rcvr[0].top_pan3_run, 0xffffffff), 0, 0, 0, buff);// stitched pan
	}
	else if (stream == inid(2, 1))	// PowerSDR Stitched Rcvrs, Right side
	{
		xanb (ppip->spc0[sp0].panb);								// nb
		xnob (ppip->spc0[sp0].pnob);								// nb2
		Spectrum0 (_InterlockedAnd (&pip.rcvr[0].top_pan3_run, 0xffffffff), 0, 2, 0, buff);// stitched pan
	}
}

PORT
void SendCBCreateScope (void (__stdcall *create_Scope)(int id))
{
	pip.create_Scope = create_Scope;
}

PORT
void SendCBScope (int id, void (__stdcall *xscope)(int state, double* data))
{
	pip.rcvr[id].xscope = xscope;
}

PORT
void SetScopeRun(int id, int run)
{
	pip.rcvr[id].scope_run = run;
}

PORT
void SendCBCreateWRecord (void (__stdcall *create_WaveRecord)(int id))
{
	pip.create_WaveRecord = create_WaveRecord;
}

PORT
void SendCBWaveRecorder (int id, void (__stdcall *xrecordwave)(int state, int pos, double* data))
{
	pip.rcvr[id].xrecordwave = xrecordwave;
}

PORT
void SetWaveRecorderRun(int id, int run)
{
	pip.rcvr[id].recordwave_run = run;
}

PORT
void SendCBCreateWPlay (void (__stdcall *create_WavePlay)(int id))
{
	pip.create_WavePlay = create_WavePlay;
}

PORT
void SendCBWavePlayer (int id, void (__stdcall *xplaywave)(int state, double* data))
{
	pip.rcvr[id].xplaywave = xplaywave;
}

PORT
void SetWavePlayerRun(int id, int run)
{
	pip.rcvr[id].playwave_run = run;
}

PORT
void SetTopPan3Run (int run)
{
	_InterlockedExchange (&pip.rcvr[0].top_pan3_run, run);
}

PORT
void SetTXVAC (int txid, int txvac)
{
	_InterlockedExchange (&pip.xmtr[txid].txvac, txvac);
}