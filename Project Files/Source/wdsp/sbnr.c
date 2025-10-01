/*  sbnr.c

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2000-2025 Original authors
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
and and uses libspecbleach
https://github.com/lucianodato/libspecbleach
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

#define _CRT_SECURE_NO_WARNINGS

#include <specbleach_adenoiser.h>

#include "comm.h"

void setSize_sbnr (SBNR a, int size)
{
    _aligned_free(a->input);
    _aligned_free(a->output);
    a->input = malloc0(size * sizeof(float));
    a->output = malloc0(size * sizeof(float));
    a->buffer_size = size;
}

void setBuffers_sbnr (SBNR a, double* in, double* out)
{
	a->in = in;
	a->out = out;
}

SBNR create_sbnr (int run, int position, int size, double *in, double *out, int rate)
{
    SBNR a = (SBNR) malloc0 (sizeof (sbnr));

    a->run = run;
    a->position = position;
    a->rate = rate;
    a->st = specbleach_adaptive_initialize(a->rate, 20); //20ms frame size, documentation recommends 20-100
    a->in = in;
    a->out = out;
    a->reduction_amount = 10.F;
    a->smoothing_factor = 0.F;
    a->whitening_factor = 0.F;
    a->noise_scaling_type = 0;
    a->noise_rescale = 2.F;
    a->post_filter_threshold = -10.F;
    a->buffer_size = size;
    a->input = malloc0(a->buffer_size * sizeof(float));
    a->output = malloc0(a->buffer_size * sizeof(float));

    return a;
}

void setSamplerate_sbnr(SBNR a, int rate)
{
    specbleach_adaptive_free(a->st);
    a->rate = rate;
    a->st = specbleach_adaptive_initialize(a->rate, 20); //20ms frame size, documentation recommends 20-100
}

void xsbnr (SBNR a, int pos)
{
    if (a->run && pos == a->position)
    {
        SpectralBleachParameters parameters =
              (SpectralBleachParameters){.residual_listen = false,
                                 .reduction_amount = a->reduction_amount,
                                 .smoothing_factor = a->smoothing_factor,
                                 .whitening_factor = a->whitening_factor,
                                 .noise_scaling_type = a->noise_scaling_type,
                                 .noise_rescale = a->noise_rescale,
                                 .post_filter_threshold = a->post_filter_threshold};

        specbleach_adaptive_load_parameters(a->st, parameters);

        double*  in = a->in;
        double* out = a->out;
        int      bs = a->buffer_size;
        float* proc_out = a->output;
        float*  to_proc = a->input;

        for (size_t i = 0; i < bs; i++)
        {
            to_proc[i] = (float)in[2 * i + 0];
        }
        specbleach_adaptive_process(a->st, (uint32_t)bs, to_proc, proc_out);

        for (size_t i = 0; i < bs; i++)
        {
            out[2 * i + 0] = (double) proc_out[i];
            out[2 * i + 1] = 0.0;
        }
    }
    else if (a->out != a->in) 
    {
        memcpy (a->out, a->in, a->buffer_size * sizeof (complex));
    }
}

void destroy_sbnr (SBNR a)
{
    specbleach_adaptive_free(a->st);
    _aligned_free (a->input);
    _aligned_free (a->output);
    _aligned_free (a);
}

PORT
void SetRXASBNRRun (int channel, int run)
{
	SBNR a = rxa[channel].sbnr.p;
	if (a->run != run)
	{
		RXAbp1Check (channel, rxa[channel].amd.p->run, rxa[channel].snba.p->run, 
                             rxa[channel].emnr.p->run, rxa[channel].anf.p->run, rxa[channel].anr.p->run,
                             rxa[channel].rnnr.p->run, run);
		EnterCriticalSection (&ch[channel].csDSP);
		a->run = run;
		RXAbp1Set (channel);
		LeaveCriticalSection (&ch[channel].csDSP);
	}
}

/* Sets the amount of dBs that the noise will be attenuated. It goes from 0 dB
  * to 20 dB */
PORT
void SetRXASBNRreductionAmount (int channel, float amount)
{
    if (amount < 0 || amount > 20) return;

	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].sbnr.p->reduction_amount = amount;
	LeaveCriticalSection (&ch[channel].csDSP);
}

/* Percentage of smoothing to apply. Averages the reduction calculation frame
 * per frame so the rate of change is less resulting in less musical noise but
 * if too strong it can blur transient and reduce high frequencies. It goes
 * from 0 to 100 percent */
PORT
void SetRXASBNRsmoothingFactor (int channel, float factor)
{
    if (factor < 0 || factor > 100) return;

	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].sbnr.p->smoothing_factor = factor;
	LeaveCriticalSection (&ch[channel].csDSP);
}

/* Percentage of whitening that is going to be applied to the residue of the
 * reduction. It modifies the noise floor to be more like white noise. This
 * can help hide musical noise when the noise is colored. It goes from 0 to
 * 100 percent */
PORT
void SetRXASBNRwhiteningFactor (int channel, float factor)
{
    if (factor < 0 || factor > 100) return;

	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].sbnr.p->whitening_factor = factor;
	LeaveCriticalSection (&ch[channel].csDSP);
}

/* Strength in which the reduction will be applied. It uses the masking
 * thresholds of the signal to determine where in the spectrum the reduction
 * needs to be stronger. This parameter scales how much in each of the
 * frequencies the reduction is going to be applied. It can be a positive dB
 * value in between 0 dB and 12 dB */
PORT
void SetRXASBNRnoiseRescale (int channel, float factor)
{
    if (factor < 0 || factor > 12) return;

	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].sbnr.p->noise_rescale = factor;
	LeaveCriticalSection (&ch[channel].csDSP);
}

/* Sets the SNR threshold in dB in which the post-filter will start to blur
 * musical noise. It can be a positive or negative dB value in between -10 dB
 * and 10 dB */
PORT
void SetRXASBNRpostFilterThreshold (int channel, float threshold)
{
    if (threshold < -10 || threshold > 10) return;

	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].sbnr.p->post_filter_threshold = threshold;
	LeaveCriticalSection (&ch[channel].csDSP);
}

/* Type of algorithm used to scale noise in order to apply over or under
 * subtraction in different parts of the spectrum while calculating the
 * reduction. 0 is a-posteriori snr scaling using the complete spectrum, 1 is
 * a-posteriori using critical bands and 2 is using masking thresholds
 */
PORT
void SetRXASBNRnoiseScalingType(int channel, int noise_scaling_type)
{
    if (noise_scaling_type < 0 || noise_scaling_type > 2) return;

    EnterCriticalSection(&ch[channel].csDSP);
    rxa[channel].sbnr.p->noise_scaling_type = noise_scaling_type;
    LeaveCriticalSection(&ch[channel].csDSP);
}

PORT
void SetRXASBNRPosition(int channel, int position)
{
    EnterCriticalSection(&ch[channel].csDSP);
    rxa[channel].sbnr.p->position = position;
    rxa[channel].bp1.p->position = position;
    LeaveCriticalSection(&ch[channel].csDSP);
}
