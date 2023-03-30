/*  obbuffs.h

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

#include <Windows.h>
#include <process.h>
#include <intrin.h>
#include <math.h>
#include <time.h>
#include <avrt.h>
#include "cmUtilities.h"

typedef double complex[2];
#ifndef PORT
#define PORT __declspec(dllexport)
#endif
#define numRings (16)
#define obMAXSIZE (360)

#ifndef _obbuffs_h
#define _obbuffs_h

#define OBB_MULT (2)
typedef struct _obb {
    int id;
    int max_in_size; // max input number of complex samples
    int r1_outsize; // number of complex samples taken out of the ring for
                    // processing

    int r1_size; // size of a single maximum sized transfer
    int r1_active_buffsize; // size of ring (in complex samples)

    double* r1_baseptr; // pointer to ring
    int r1_inidx; // in 'double', actual index into the buffer is 2 times this
    int r1_outidx; // in 'double', actual index into the buffer is 2 times this
    int r1_unqueuedsamps; // number of input samples not yet queued/released for
                          // execution
    volatile long run; // when 1, thread loops; when 0, thread terminates
    volatile long accept; // flag indicating whether accepting input data
    HANDLE Sem_BuffReady; // count = number of output-sized buffers queued for
                          // processing
    CRITICAL_SECTION csOUT; // used to block output while parameters are updated
                            // or buffers flushed
    CRITICAL_SECTION csIN; // used to block input while parameters are updated
                           // or buffers flushed
    double* out;
    HANDLE obThread;
    long obThreadId;
} obb, *OBB;

extern void create_obbuffs(int id, int accept, int max_insize, int outsize);

extern void destroy_obbuffs(int id);

extern void flush_obbuffs(int id);

extern __declspec(dllexport) void OutBound(int id, int nsamples, double* in);

extern void obdata(int id, double* out);

extern void ob_main(void* pargs);

extern void SetOBRingOutsize(int id, int size);

extern int sendOutbound(int id, double* out, OBB a);

extern void signalWakeup();

#endif