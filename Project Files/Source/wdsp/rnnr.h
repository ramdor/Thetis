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
*/

#ifndef _rnnr_h
#define _rnnr_h

#include "rnnoise.h"

#define FRAME_SIZE

typedef struct _queuenode {
    float value;
    struct _queuenode* next;
} queuenode;

typedef struct _rnnr
{
	int run;
    	int position;
        int frame_size;
        DenoiseState *st;
        double *in;
        double *out;

        //MW0LGE
        int buffer_size;
        float* output_buffer;
        float gain;

        float* to_process_buffer;
        float* processed_output_buffer;

        queuenode* input_queue_head;
        queuenode* input_queue_tail;
        int input_queue_count;

        queuenode* output_queue_head;
        queuenode* output_queue_tail;
        int output_queue_count;

}rnnr, *RNNR;

extern RNNR create_rnnr (int run, int position, double *in, double *out);
extern void setSize_rnnr(RNNR a, int size);
extern void setBuffers_rnnr (RNNR a, double* in, double* out);
extern void destroy_rnnr (RNNR a);
extern void xrnnr (RNNR a, int pos);

#endif //_rnnr_h
