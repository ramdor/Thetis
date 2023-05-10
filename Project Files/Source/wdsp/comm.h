/*  comm.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2013 Warren Pratt, NR0V

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
#include <stdint.h>
#include <time.h>
#include <avrt.h>
#include "fftw3.h"

#include "amd.h"
#include "ammod.h"
#include "amsq.h"
#include "analyzer.h"
#include "anf.h"
#include "anr.h"
#include "bandpass.h"
#include "calcc.h"
#include "cblock.h"
#include "cfcomp.h"
#include "cfir.h"
#include "channel.h"
#include "compress.h"
#include "delay.h"
#include "dexp.h"
#include "div.h"
#include "eer.h"
#include "emnr.h"
#include "emph.h"
#include "eq.h"
#include "fcurve.h"
#include "fir.h"
#include "firmin.h"
#include "fmd.h"
#include "fmmod.h"
#include "fmsq.h"
#include "gain.h"
#include "gen.h"
#include "icfir.h"
#include "iir.h"
#include "iobuffs.h"
#include "iqc.h"
#include "lmath.h"
#include "main.h"
#include "meter.h"
#include "meterlog10.h"
#include "nbp.h"
#include "nob.h"
#include "nobII.h"
#include "osctrl.h"
#include "patchpanel.h"
#include "resample.h"
#include "rmatch.h"
#include "RXA.h"
#include "sender.h"
#include "shift.h"
#include "siphon.h"
#include "slew.h"
#include "snb.h"
#include "ssql.h"
#include "syncbuffs.h"
#include "TXA.h"
#include "utilities.h"
#include "varsamp.h"
#include "wcpAGC.h"
#include <assert.h>
#include "klj_mem.h"

#ifndef PRIO_THRD_DEFINED
#define PRIO_THRD_DEFINED

/*/
static inline int IsPowerOfTwo(unsigned long x) {
    return (x != 0) && ((x & (x - 1)) == 0);
}
/*/

static inline HANDLE prioritise_thread_max() {

    DWORD taskIndex = 0;
    HANDLE hTask = AvSetMmThreadCharacteristics(TEXT("Pro Audio"), &taskIndex);
    if (hTask != 0) {

        BOOL ok = AvSetMmThreadPriority(hTask, AVRT_PRIORITY_CRITICAL);
        assert(ok);

    } else {
        // assert("Why did setting thread priority fail?" == 0);
        const DWORD dw = GetLastError();
        if (dw == 1552) { // the specified thread is already joining a task
            // assert(0);

        } else {
            SetThreadPriority(
                GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);
            fprintf(stderr,
                "I don't like this, falling back to "
                "THREAD_PRIORITY_TIME_CRITICAL");
            fflush(stderr);
        }
    }
    return hTask;
}

static inline BOOL prioritise_thread_cleanup(HANDLE h) {
    BOOL ret = AvRevertMmThreadCharacteristics(h);
    if (ret == 0) {
        DWORD dw = GetLastError();
        assert(0);
        fprintf(stderr,
            "Failed to clean up thread priority, with error code: %ld\n",
            (int)dw);
    }

    return ret;
}
#endif

// manage differences among consoles
#define _Thetis

// channel definitions
#define MAX_CHANNELS 32 // maximum number of supported channels
#define DSP_MULT                                                               \
    2 // number of dsp_buffsizes that are held in an iobuff pseudo-ring
#define INREAL float // data type for channel input buffer
#define OUTREAL float // data type for channel output buffer

// display definitions
#define dMAX_DISPLAYS 64 // maximum number of displays = max instances
#define dMAX_STITCH 4 // maximum number of sub-spans to stitch together
#define dMAX_NUM_FFT 1 // maximum number of ffts for an elimination
#define dMAX_PIXELS 16384 // maximum number of pixels that can be requested
#define dMAX_AVERAGE                                                           \
    60 // maximum number of pixel frames that will be window-averaged
#ifdef _Thetis
#define dINREAL double
#else
#define dINREAL float
#endif
#define dOUTREAL float
#define dSAMP_BUFF_MULT                                                        \
    2 // ratio of input sample buffer size to fft size (for overlap)
#define dNUM_PIXEL_BUFFS 3 // number of pixel output buffers
#define dMAX_M 1 // number of variables to calibrate
#define dMAX_N 100 // maximum number of frequencies at which to calibrate
#define dMAX_CAL_SETS 2 // maximum number of calibration data sets
#define dMAX_PIXOUTS 4 // maximum number of det/avg/outputs per display instance

// wisdom definitions
#define MAX_WISDOM_SIZE_DISPLAY 262144
#define MAX_WISDOM_SIZE_FILTER 262144 // was 32769

// math definitions
#define PI 3.1415926535897932
#define TWOPI 6.2831853071795864

// miscellaneous
typedef double complex[2];
#define PORT __declspec(dllexport)
