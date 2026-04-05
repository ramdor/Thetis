/*  Tci.c

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2026 Richard Samphire MW0LGE

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

mw0lge@grange-lane.co.uk
*/
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//

#include "cmcomm.h"

typedef struct _tci_rcvr
{
	void* mixer;
	int audio_rate;
	int txmon_rate;
	int audio_size;
	int mox;
	int mon;
	double mon_scale;
	long active;
	long what;
} tci_rcvr;

typedef struct _tci
{
	void (*OutboundTCIRxAudio)(int id, int nsamples, double* buff);
	tci_rcvr rcvr[cmMAXrcvr];
} tci, *TCI;

static tci gtci = {0};
static TCI ptci = &gtci;

static void get_tci_audio_mix_state(int rx, long* active, long* what)
{
	if (active == 0 || what == 0)
		return;

	*active = 0;
	*what = 0;

	if (rx < 0 || rx >= pcm->cmRCVR)
		return;

	if (!ptci->rcvr[rx].mox)
	{
		*active = 1;
		*what = 1;
	}
	else
	{
		*active = 2;
		*what = ptci->rcvr[rx].mon ? 2 : 0;
	}
}

static void apply_tci_audio_mix_state(int rx)
{
	long active;
	long what;
	tci_rcvr* a;

	if (rx < 0 || rx >= pcm->cmRCVR)
		return;

	a = &ptci->rcvr[rx];
	if (!a->mixer)
		return;

	get_tci_audio_mix_state(rx, &active, &what);

	if (a->active != active)
	{
		SetAAudioMixState(a->mixer, 0, 0, (active & 1) != 0);
		SetAAudioMixState(a->mixer, 0, 1, (active & 2) != 0);
		a->active = active;
	}

	if (a->what != what)
	{
		SetAAudioMixWhat(a->mixer, 0, 0, (what & 1) != 0);
		SetAAudioMixWhat(a->mixer, 0, 1, (what & 2) != 0);
		a->what = what;
	}
}

static void tci_audio_out(int id, int nsamples, double* buff)
{
	if (_InterlockedAnd(&pcm->tci_rx_out_run, 1) && ptci->OutboundTCIRxAudio)
		(*ptci->OutboundTCIRxAudio)(id, nsamples, buff);
}

static void create_tci_audio_mixer(int rx)
{
	int inrate[2];
	long active;
	long what;
	tci_rcvr* a;

	if (rx < 0 || rx >= pcm->cmRCVR)
		return;

	a = &ptci->rcvr[rx];
	inrate[0] = a->audio_rate;
	inrate[1] = a->txmon_rate;
	get_tci_audio_mix_state(rx, &active, &what);

	a->mixer = create_aamix(
		-1,
		rx,
		a->audio_size,
		a->audio_size,
		2,
		active,
		what,
		1.0,
		4096,
		inrate,
		a->audio_rate,
		tci_audio_out,
		0.0,
		0.0,
		0.0,
		0.0);

	a->active = active;
	a->what = what;
	SetAAudioMixVol(a->mixer, 0, 1, a->mon_scale);
}

static void destroy_tci_audio_mixer(int rx)
{
	tci_rcvr* a;

	if (rx < 0 || rx >= pcm->cmRCVR)
		return;

	a = &ptci->rcvr[rx];
	if (a->mixer)
	{
		destroy_aamix(a->mixer, 0);
		a->mixer = 0;
	}
	a->active = 0;
	a->what = 0;
}

void create_tci()
{
	int i;

	for (i = 0; i < pcm->cmRCVR; i++)
	{
		ptci->rcvr[i].audio_rate = pcm->rcvr[i].ch_outrate;
		ptci->rcvr[i].txmon_rate = pcm->xmtr[0].ch_outrate;
		ptci->rcvr[i].audio_size = pcm->rcvr[i].ch_outsize;
		create_tci_audio_mixer(i);
	}
}

void destroy_tci()
{
	int i;

	for (i = 0; i < pcm->cmRCVR; i++)
		destroy_tci_audio_mixer(i);
}

void xtciOUT(int id, int stream, double* data)
{
	tci_rcvr* a;

	if (id < 0 || id >= pcm->cmRCVR)
		return;

	if (!(_InterlockedAnd(&pcm->tci_rx_out_run, 1) && ptci->OutboundTCIRxAudio))
		return;

	a = &ptci->rcvr[id];
	if (!a->mixer)
		return;

	if (stream == 1)
		xMixAudio(a->mixer, -1, 0, data);
	else if (stream == 2)
		xMixAudio(a->mixer, -1, 1, data);
}

PORT
void SendpOutboundTCIRxAudio(void (*Outbound)(int id, int nsamples, double* buff))
{
	ptci->OutboundTCIRxAudio = Outbound;
}

void SetTCIRxAudioRate(int id, int rate)
{
	tci_rcvr* a;

	if (id < 0 || id >= pcm->cmRCVR)
		return;

	a = &ptci->rcvr[id];
	if (!a->mixer || a->audio_rate == rate)
	{
		a->audio_rate = rate;
		return;
	}

	a->audio_rate = rate;
	SetAAudioMixState(a->mixer, 0, 0, 0);
	SetAAudioMixState(a->mixer, 0, 1, 0);
	a->active = 0;
	SetAAudioOutRate(a->mixer, 0, a->audio_rate);
	SetAAudioStreamRate(a->mixer, 0, 0, a->audio_rate);
	SetAAudioStreamRate(a->mixer, 0, 1, a->txmon_rate);
	apply_tci_audio_mix_state(id);
}

void SetTCIRxAudioSize(int id, int size)
{
	tci_rcvr* a;

	if (id < 0 || id >= pcm->cmRCVR)
		return;

	a = &ptci->rcvr[id];
	a->audio_size = size;
	if (!a->mixer)
		return;

	SetAAudioRingInsize(a->mixer, 0, a->audio_size);
	SetAAudioRingOutsize(a->mixer, 0, a->audio_size);
	apply_tci_audio_mix_state(id);
}

void SetTCITxMonitorRate(int id, int rate)
{
	tci_rcvr* a;

	if (id < 0 || id >= pcm->cmRCVR)
		return;

	a = &ptci->rcvr[id];
	if (!a->mixer || a->txmon_rate == rate)
	{
		a->txmon_rate = rate;
		return;
	}

	a->txmon_rate = rate;
	SetAAudioMixState(a->mixer, 0, 1, 0);
	a->active &= ~2;
	SetAAudioStreamRate(a->mixer, 0, 1, a->txmon_rate);
	apply_tci_audio_mix_state(id);
}

PORT
void SetTCIRxAudioMox(int id, int mox)
{
	if (id < 0 || id >= cmMAXrcvr)
		return;

	ptci->rcvr[id].mox = mox;
	if (id < pcm->cmRCVR)
		apply_tci_audio_mix_state(id);
}

PORT
void SetTCIRxAudioMon(int id, int mon)
{
	if (id < 0 || id >= cmMAXrcvr)
		return;

	ptci->rcvr[id].mon = mon;
	if (id < pcm->cmRCVR)
		apply_tci_audio_mix_state(id);
}

PORT
void SetTCIRxAudioMonVol(int id, double vol)
{
	if (id < 0 || id >= cmMAXrcvr)
		return;

	ptci->rcvr[id].mon_scale = vol;
	if (!ptci->rcvr[id].mixer)
		return;

	SetAAudioMixVol(ptci->rcvr[id].mixer, 0, 1, ptci->rcvr[id].mon_scale);
}
