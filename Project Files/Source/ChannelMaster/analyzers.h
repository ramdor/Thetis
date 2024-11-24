/*  analyzers.h

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

#ifndef _analyzers_h
#define _analyzers_h

typedef struct _analyzers
{
	int m_analyzers;				// maximum number of analyzers (in addition to basic ones), use 32
	int base_disp;					// base value for new 'disp' numbers, use 32; range will be 32 - 63
	int* disp;						// array of 'disp' identifiers for the additional analyzers
	int* stype;						// array of 'stype'; 0 for receiver, 1 for transmitter
	int* id;						// 0 to N-1 for receivers instantiated; N for first transmitter
	int* max_fft_size;				// maximum FFT size to be allocated; must be a power-of-two
	int* stream;					// calculated ChannelMaster 'stream' number
	int* run;						// array of 'run' variables to turn analyzers OFF/ON

	// support up to 16 TX DSP CHANNELS
	int* n_tx_disps;				// number of current TX additional displays FOR EACH TX CHANNEL
	int** tx_run;					// run vectors for TX channels (up to 16 transmitters)
	int** tx_disp;					// 'disp' parameter vectors for TX channels (up to 16 transmitters)

	CRITICAL_SECTION cs_update;		// lock for alloc and free of analyzers
} analyzers, *ANALYZERS;


extern void* create_analyzer_alloc (int m_analyzers, int base_disp);

extern int destroy_analyzer_alloc ();

extern __declspec (dllexport) int alloc_analyzer(int stype, int id, int max_fft_size);

extern __declspec (dllexport) int free_analyzer(int disp);

#endif
