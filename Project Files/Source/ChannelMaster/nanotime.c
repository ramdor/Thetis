/*
 * JanusAudio.dll - Support library for HPSDR.org's Janus/Ozy Audio card
 * Copyright (C) 2006,2007  Bill Tracey (bill@ejwt.com) (KD5TFD)
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301 USA
 *
 */

/* hi resolution timing routins */

#include <windows.h>
#include "nanotimer.h"
#include <stdio.h>

__int64 PerfFreq = 0;

// returns current tick count from the performance
// counter.  Convert this tick to nanosecs with perfTicksToNanos
// 0 is returned on failure
NANOTIMER_API __int64 getPerfTicks(void) {
    __int64 count;
    BOOL rc;
    rc = QueryPerformanceCounter((LARGE_INTEGER*)&count);
    if (rc == 0) {
        return 0;
    }
    /* else */
    return count;
}

// returns freq in hertz of perf counter
// returns 0 on failure
NANOTIMER_API __int64 getPerfFreq(void) {
    BOOL rc;
    __int64 freq;
    rc = QueryPerformanceFrequency((LARGE_INTEGER*)&freq);
    if (rc == 0) { /* ugh - failed */
        return 0;
    }
    /* else */
    return freq;
}

NANOTIMER_API __int64 perfTicksToNanos(__int64 ticks) {
    __int64 result;
    if (ticks == 0) return 0;
    if (PerfFreq == 0) {
        PerfFreq = getPerfFreq();
    }
    if (PerfFreq == 0) {
        return 0;
    }
    result = (ticks * 1000000000) / PerfFreq;
    return result;
}

NANOTIMER_API void updateHLA(HLA_COUNTER* p, __int64 v) {
    p->sum = v + p->sum;
    p->count = (__int64)1 + p->count;
    if (v > p->hi) {
        p->hi = v;
    }
    if (v < p->lo) {
        p->lo = v;
    }
    return;
}

NANOTIMER_API void initHLA(HLA_COUNTER* p) {
    p->sum = 0;
    p->count = 0;
    p->lo = 0x7fffffffffffffff;
    p->hi = 0x8000000000000001;
    return;
}

NANOTIMER_API void printHLA(
    HLA_COUNTER* p, /* FILE *f, */ unsigned char* prefix) {
    __int64 avg = 0;
    // printf("p @: 0x%08x\n", (int)p);
    if (p->count != 0) {
        avg = p->sum / p->count;
    }
    if (prefix == NULL) {
        prefix = "";
    }
    // printf("about to printf\n");  fflush(stdout);
    printf("%s count: %I64d hi: %I64d lo: %I64d avg: %I64d\n", prefix, p->count,
        p->hi, p->lo, avg);
    fflush(stdout);
    // fprintf(f, "count: %I64d hi: %I64d lo: %I64d avg: %I64d\n",  p->count,
    // p->hi, p->lo, avg); fflush(stdout); printf("printf done - returning\n");
    // fflush(stdout);
    return;
}

NANOTIMER_API void printHLANano(HLA_COUNTER* p, unsigned char* prefix) {
    __int64 avg = 0;
    // printf("p @: 0x%08x\n", (int)p);
    if (p->count != 0) {
        avg = p->sum / p->count;
    }
    if (prefix == NULL) {
        prefix = "";
    }
    // printf("about to printf\n");  fflush(stdout);
    printf("%s count: %I64d hi: %I64d lo: %I64d avg: %I64d\n", prefix, p->count,
        perfTicksToNanos(p->hi), perfTicksToNanos(p->lo),
        perfTicksToNanos(avg));
    fflush(stdout);
    return;
}