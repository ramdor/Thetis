/*  ivac.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2015-2025 Warren Pratt, NR0V
Copyright (C) 2015-2016 Doug Wigley, W5WC

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

#ifndef _ivac_h
#define _ivac_h

#include "ring.h"
#include "portaudio.h"

#define MAX_EXT_VACS			(16)

typedef struct _ivac
{
	int run;
	int iq_type;					// 1 if using raw IQ data; 0 for audio
	int stereo;						// 1 for stereo; 0 otherwise
	int iq_rate;
	int mic_rate;
	int audio_rate;
	int txmon_rate;
	int vac_rate;					// VAC sample rate
	int mic_size;
	int iq_size;
	int audio_size;
	int txmon_size;
	int vac_size;					// VAC buffer size
	void *mixer;					// pointer to async audio mixer
	double* bitbucket;				// dump for un-needed resampler output

	void *rmatchIN;
	void *rmatchOUT;

	int INringsize;
	int OUTringsize;

	PaStreamParameters inParam, outParam;
	PaStream *Stream;

	size_t mono_in_to_stereo_capacity;		// capacity of mono to stereo buffer
	double* mono_in_to_stereo_buffer;		// buffer for mono to stereo conversion

	int host_api_index;
	int input_dev_index;
	int output_dev_index;
	int num_channels;
	double in_latency;
	double out_latency;
	double pa_in_latency;
	double pa_out_latency;
	int vox;
	int mox;
	int mon;
	int vac_bypass;
	int vac_combine_input;
	double vac_preamp;
	double vac_rx_scale;
	double vac_mon_scale;		// MW0LGE_21k9d the volume level of the vac mon
	int INforce;				// force var ratio for rmatchIN
	double INfvar;				// var value when forced for rmatchIN
	int OUTforce;				// force var ratio for rmatchOUT
	double OUTfvar;				// var value when forced for rmatchOUT

	double initial_INvar;			// init the var ratio
	double initial_OUTvar;			// init the var ratio

	int swapIQout;

	int exclusive_in;				// only use with wasapi right now
	int exclusive_out;				// only use with wasapi right now

	CRITICAL_SECTION cs_ivac;
} ivac, *IVAC;

void combinebuff (int n, double* a, double* combined);
void scalebuff (int n, double* in, double k, double* out);
void xvac_out(int id, int nsamples, double* buff);

extern __declspec(dllexport) void *create_resampleV (int samplerate_in, int samplerate_out);
extern __declspec(dllexport) void xresampleV (double *input, double *output, int numsamps, int *outsamps, void *ptr);
extern __declspec(dllexport) void destroy_resampleV (/*ResSt*/ void * resst);
extern __declspec(dllexport) void destroy_ivac (int id);
extern __declspec(dllexport) void xvacIN(int id, double* in_tx, int bypass);
extern __declspec(dllexport) void xvacOUT(int id, int stream, double* data);
extern __declspec(dllexport) void create_ivac (
	int id,
	int run,
	int iq_type,				// 1 if using raw IQ samples, 0 for audio
	int stereo,					// 1 for stereo, 0 otherwise
	int iq_rate,				// sample rate of RX I-Q data
	int mic_rate,				// sample rate of data from VAC to TX MIC input
	int audio_rate,				// sample rate of data from RCVR Audio data to VAC
	int txmon_rate,				// sample rate of data from TX Monitor to VAC
	int vac_rate,				// VAC sample rate
	int mic_size,				// buffer size for data from VAC to TX MIC input
	int iq_size,				// buffer size for RCVR IQ data to VAC
	int audio_size,				// buffer size for RCVR Audio data to VAC
	int txmon_size,				// buffer size for TX Monitor data to VAC
	int vac_size				// VAC buffer size
	);

extern void SetIVACtxmonRate (int id, int rate);
extern void SetIVACtxmonSize (int id, int size);
extern __declspec(dllexport) void SetIVACiqSizeAndRate (int id, int size, int rate);
extern __declspec(dllexport) void SetIVACmicSize (int id, int size);
extern __declspec(dllexport) void SetIVACmicRate (int id, int rate);
extern __declspec(dllexport) void SetIVACaudioRate (int id, int rate);
extern __declspec(dllexport) void SetIVACaudioSize (int id, int size);

#endif
