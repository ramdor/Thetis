/*  rnnr.h

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2025 Richard Samphire MW0LGE

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

This code is based on code and ideas from  : https://github.com/vu3rdd/wdsp
and and uses RNNoise and libspecbleach
https://gitlab.xiph.org/xiph/rnnoise
https://github.com/lucianodato/libspecbleach

It uses a non modified version of rmnoise and implements a ringbuffer to handle input/output frame size differences.
*/

#ifndef _rnnr_h
#define _rnnr_h

#include "rnnoise.h"

#define FRAME_SIZE

typedef struct _rnnr_ring_buffer {
    float* buf;
    int capacity;
    int head;
    int tail;
    int count;
} rnnr_ring_buffer;

typedef struct _rnnr
{
	int run;
    int run_old; // used when loading a new model
    int position;
    int frame_size;
    DenoiseState *st;
    double *in;
    double *out;
    float gain;
    float gain_db;
    float agc_att_a;
    float agc_rel_a;

    int buffer_size;
    int rate;
    float* output_buffer;

    float* to_process_buffer;
    float* processed_output_buffer;

    rnnr_ring_buffer input_ring;
    rnnr_ring_buffer output_ring;

    CRITICAL_SECTION cs;

} rnnr, *RNNR;

extern RNNR create_rnnr (int run, int position, int size, double *in, double *out, int rate);
extern void setSize_rnnr(RNNR a, int size);
extern void setBuffers_rnnr (RNNR a, double* in, double* out);
extern void destroy_rnnr (RNNR a);
extern void xrnnr (RNNR a, int pos);
extern void setSamplerate_rnnr(RNNR a, int rate);

#endif //_rnnr_h
