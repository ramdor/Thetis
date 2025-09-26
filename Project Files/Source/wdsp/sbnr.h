/*  snnr.h

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

#ifndef _sbnr_h
#define _sbnr_h

#include <specbleach_adenoiser.h>

typedef struct _sbnr
{
	int run;
    	int position;
        double *in;
        double *out;
        float reduction_amount;
        float smoothing_factor;
        float whitening_factor;
        int noise_scaling_type;
        float noise_rescale;
        float post_filter_threshold;
        SpectralBleachHandle st;
        int buffer_size;

        float* input;
        float* output;
} sbnr, *SBNR;

// define the public api of this module
extern SBNR create_sbnr(int run, int position, double *in, double *out);
extern void destroy_sbnr(SBNR a);
extern void setSize_sbnr(SBNR a, int size);
extern void setBuffers_sbnr(SBNR a, double *in, double *out);
extern void xsbnr(SBNR a, int pos);

#endif // _sbnr_h
