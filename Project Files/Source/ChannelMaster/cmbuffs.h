/*  cmbuffs.h

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

#ifndef _cmbuffs_h
#define _cmbuffs_h
#include "comm.h"

#define CMB_MULT		(3)						
typedef struct _cmb
{
	int   id;
	int   max_in_size;							// max input number of complex samples
	int   max_outsize;							// max output number of complex samples
	int   r1_outsize;							// number of complex samples taken out of the ring for processing 

	int   r1_size;								// size of a single maximum sized transfer
	int   r1_active_buffsize;					// size of ring (in complex samples)
	
	double* r1_baseptr;							// pointer to ring
	int   r1_inidx;								// in 'double', actual index into the buffer is 2 times this
	int   r1_outidx;							// in 'double', actual index into the buffer is 2 times this
	int   r1_unqueuedsamps;						// number of input samples not yet queued/released for execution
	volatile long run;							// when 1, thread loops; when 0, thread terminates
	volatile long accept;						// flag indicating whether accepting input data
	HANDLE Sem_BuffReady;						// count = number of output-sized buffers queued for processing
	CRITICAL_SECTION csOUT;						// used to block output while parameters are updated or buffers flushed
	CRITICAL_SECTION csIN;						// used to block input while parameters are updated or buffers flushed
} cmb, *CMB;

extern void create_cmbuffs (int id, int accept, int max_insize, int max_outsize, int outsize);

extern void destroy_cmbuffs (int id);

extern void flush_cmbuffs (int id);

extern __declspec (dllexport) void Inbound (int id, int nsamples, double* in);

extern void cmdata (int id, double* out);

extern void cm_main (void *pargs);

extern void SetCMRingOutsize (int id, int size);


#endif

