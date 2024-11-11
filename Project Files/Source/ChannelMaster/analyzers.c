/*  analyzers.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2024 Warren Pratt, NR0V

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

warren@pratt.one

*/

#include "cmcomm.h"

ANALYZERS a;

// create the analyzer allocator; called from cmaster.c
void* create_analyzer_alloc (int m_analyzers, int base_disp)
{
	a = (ANALYZERS) malloc0 (sizeof(analyzers));
	a->m_analyzers = m_analyzers;
	a->base_disp = base_disp;
	
	a->stype        = (int*) malloc0 (m_analyzers * sizeof(int));
	a->id           = (int*) malloc0 (m_analyzers * sizeof(int));
	a->max_fft_size = (int*) malloc0 (m_analyzers * sizeof(int));
	a->stream       = (int*) malloc0 (m_analyzers * sizeof(int));
	a->disp         = (int*) malloc0 (m_analyzers * sizeof(int));
	a->run          = (int*) malloc0 (m_analyzers * sizeof(int));

	// support up to 16 TX DSP channels
	a->n_tx_disps = (int*) malloc0 (16 * sizeof(int));	// number of allocated_disps for each of 16 transmitters
	a->tx_run     = (int**) malloc0 (16 * sizeof(int*));
	a->tx_disp    = (int**) malloc0 (16 * sizeof(int*));
	for (int i = 0; i < 16; i++)
	{
		a->tx_run[i]  = (int*) malloc0 (a->m_analyzers * sizeof(int));	// array of 'run' values for each TX 'i'
		a->tx_disp[i] = (int*) malloc0 (a->m_analyzers * sizeof(int));	// array of 'disp' parameters for each TX 'i'
	}
	// initialize 'disp' id's to '-1' when not in use  (each analyzer has a 'disp' id)
	// initialize 'run' values to ON  (each analyzer has a 'run' variable)
	for (int i = 0; i < a->m_analyzers; i++)
	{
		a->disp[i] = -1;
		a->run[i]  = +1;
	}
	InitializeCriticalSectionAndSpinCount (&a->cs_update, 2500);
	return a;
}

// destroy the analyzer allocator; called from cmaster.c
int destroy_analyzer_alloc ()
{
	DeleteCriticalSection(&a->cs_update);
	for (int i = 0; i < 16; i++)
	{
		_aligned_free (a->tx_run[i]);
		_aligned_free (a->tx_disp[i]);
	}
	_aligned_free (a->tx_disp);
	_aligned_free (a->tx_run);
	_aligned_free (a->n_tx_disps);
	_aligned_free (a->run);
	_aligned_free (a->disp);
	_aligned_free (a->stream);
	_aligned_free (a->max_fft_size);
	_aligned_free (a->id);
	_aligned_free (a->stype);
	_aligned_free (a);
	return 0;
}

void tx_analyzers ()
{
	int tx_id, ch_id, stream_id;
	// formulate and send data to WDSP for transmitter channels that have allocated analyzers
	memset (a->n_tx_disps, 0, 16 * sizeof(int));					// reset n_tx_disps for each TX; we're going to re-count
	// go through each analyzer and store variables in the correct 'tx_id'
	for (int j = 0; j < a->m_analyzers; j++)						// for each analyzer (any 'stype')
	{
		if (a->disp[j] >= a->base_disp && a->stype[j] == 1)			// if it's an active transmitter analyzer ...
		{
			tx_id = txid (a->stream[j]);							// get the Transmitter ID
			a->tx_run[tx_id][a->n_tx_disps[tx_id]] = a->run[j];		// save 'run' value for this analyzer
			a->tx_disp[tx_id][a->n_tx_disps[tx_id]] = a->disp[j];	// save 'disp' value for this analyzer
			a->n_tx_disps[tx_id]++;									// increment number of disps for the channel
		}
	}
	// for each transmitter where we have analyzers, call the correct channel with the data
	for (int k = 0; k < 16; k++)
	{
		// We currently only have one TX channel.  If we make calls to transmitter channels that don't exist, there's
		//    definitely a problem somewhere.
		if (a->n_tx_disps[k] > 0)
		{
			tx_id = k;
			stream_id = tx_id + pcm->cmRCVR;
			ch_id = chid (stream_id, 0);
			TXASetSipAllocDisps (ch_id, a->n_tx_disps[tx_id], a->tx_run[tx_id], a->tx_disp[tx_id]);
		}
	}
}

// Call from console to allocate a new analyzer.
// The 'disp' number (needed for SetAnalyzer(...), GetPixels(...), etc.), is returned.
// If no analyzer is available, the return value is negative.
PORT
int alloc_analyzer (int stype, int id, int max_fft_size)
// stype: 0 for receiver, 1 for transmitter
// id: 0 to N-1 for instantiated receivers; 0 for first transmitter
// max_fft_size: must be a power-of-two; smaller values save memory.
{
	int rc, i, mdisp;
	EnterCriticalSection (&a->cs_update);
	i = 0;
	// find the first available 'disp' number
	while (a->disp[i] != -1 && i < a->m_analyzers) i++;
	// if there's a 'disp' available, assign it below
	if (i < a->m_analyzers)
		mdisp = i;
	else
	{
		// if no 'disp' available, we're done and '-1' is returned.
		LeaveCriticalSection (&a->cs_update);
		return -1;
	}
	a->stype[mdisp] = stype;
	a->id[mdisp] = id;
	a->max_fft_size[mdisp] = max_fft_size;
	// calculate the stream id for the ChannelMaster
	a->stream[mdisp] = inid (stype, id);
	a->disp[mdisp] = mdisp + a->base_disp;
	// create the analyzer.
	XCreateAnalyzer (a->disp[mdisp], &rc, a->max_fft_size[mdisp], 1, 1, "");
	// formulate and send data to WDSP for transmitter channels that have allocated analyzers
	tx_analyzers();
	LeaveCriticalSection (&a->cs_update);
	if (rc < 0) return rc;
	else        return a->disp[mdisp];
}

// Call from console to free an analyzer that is no longer in use.
PORT
int free_analyzer (int disp)
{
	EnterCriticalSection (&a->cs_update);
	// when a 'disp' is not in use, we set its value to '-1'
	a->disp[disp - a->base_disp] = -1;
	tx_analyzers();
	DestroyAnalyzer (disp);
	LeaveCriticalSection (&a->cs_update);
	return 0;
}

// Call from console to turn OFF/ON one of the additional analyzers.
PORT
void run_analyzer(int disp, int run)
{
	EnterCriticalSection (&a->cs_update);
	a->run[disp - a->base_disp] = run;
	tx_analyzers();
	LeaveCriticalSection (&a->cs_update);
}

