/*  utilities.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2013, 2019 Warren Pratt, NR0V

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

#define _CRT_SECURE_NO_WARNINGS
#include "comm.h"

/********************************************************************************************************
*																										*
*											Required Utilities											*
*																										*
********************************************************************************************************/

PORT
void *malloc0 (int size)
{
	int alignment = 16;
	void* p = _aligned_malloc (size, alignment);
	if (p != 0) memset (p, 0, size);
	return p;
}

// Exported calls

PORT void
*NewCriticalSection()
{	// used by VAC
	LPCRITICAL_SECTION cs_ptr;
	cs_ptr = (LPCRITICAL_SECTION)calloc (1,sizeof (CRITICAL_SECTION));
	return (void *)cs_ptr;
}

PORT void
DestroyCriticalSection (LPCRITICAL_SECTION cs_ptr)
{	// used by VAC
	free ((char *)cs_ptr);
}


/********************************************************************************************************
*																										*
*										Test & Debug Utilities											*
*																										*
********************************************************************************************************/

void print_impulse (const char* filename, int N, double* impulse, int rtype, int pr_mode)
{
	int i;
	FILE* file;
	if (pr_mode == 0)
		file = fopen (filename, "w");
	else
		file = fopen (filename, "a");
	if (rtype == 0)
		for (i = 0; i < N; i++)
			fprintf (file, "%.17e\n", impulse[i]);
	else
		for (i = 0; i < N; i++)
			fprintf (file, "%.17e\t%.17e\n", impulse[2 * i + 0], impulse[2 * i + 1]);
	fprintf (file, "\n\n\n\n");
	fflush (file);
	fclose (file);
}

PORT
void analyze_bandpass_filter (int N, double f_low, double f_high, double samplerate, int wintype, int rtype, double scale)
{
	double* linphase_imp = (double *) malloc0 (N * sizeof (complex));
	double* minphase_imp = (double *) malloc0 (N * sizeof (complex));
	linphase_imp = fir_bandpass (N, f_low, f_high, samplerate, wintype, rtype, scale);
	mp_imp (N, linphase_imp, minphase_imp, 16, 0);
	print_impulse ("linear_phase_impulse.txt",  N, linphase_imp, 1, 0);
	print_impulse ("minimum_phase_impulse.txt", N, minphase_imp, 1, 0);
	_aligned_free (minphase_imp);
	_aligned_free (linphase_imp);
}

void print_peak_val (const char* filename, int N, double* buff, double thresh)
{
	int i;
	static unsigned int seqnum;
	double peak = 0.0;
	FILE* file;
	for (i = 0; i < N; i++)
		if (buff[i] > peak) peak = buff[i];
	if (peak >= thresh)
	{
		file = fopen(filename, "a");
		fprintf(file, "%d\t\t%.17e\n", seqnum, peak);
		fflush(file);
		fclose(file);
	}
	seqnum++;
}

void print_peak_env (const char* filename, int N, double* buff, double thresh)
{
	int i;
	static unsigned int seqnum;
	double peak = 0.0;
	double new_peak;
	FILE* file;
	for (i = 0; i < N; i++)
	{
		new_peak = sqrt (buff[2 * i + 0] * buff[2 * i + 0] + buff[2 * i + 1] * buff[2 * i + 1]);
		if (new_peak > peak) peak = new_peak;
	}
	if (peak >= thresh)
	{
		file = fopen (filename, "a");
		fprintf (file, "%d\t\t%.17e\n", seqnum, peak);
		fflush (file);
		fclose (file);
	}
	seqnum++;
}

void print_peak_env_f2 (const char* filename, int N, float* Ibuff, float* Qbuff)
{
	int i;
	double peak = 0.0;
	double new_peak;
	FILE* file = fopen (filename, "a");
	for (i = 0; i < N; i++)
	{
		new_peak = sqrt (Ibuff[i] * Ibuff[i] + Qbuff[i] * Qbuff[i]);
		if (new_peak > peak) peak = new_peak;
	}
	fprintf (file, "%.17e\n", peak);
	fflush (file);
	fclose (file);
}

void print_iqc_values (const char* filename, int state, double env_in, double I, double Q, double ym, double yc, double ys, double thresh)
{
	static unsigned int seqnum;
	double env_out;
	FILE* file;
	env_out = sqrt (I * I + Q * Q);
	if (env_out > thresh)
	{
		file = fopen(filename, "a");
		if (seqnum == 0)
			fprintf(file, "seqnum\tstate\tenv_in\t\tenv_out\t\tym\t\tyc\t\tys\n");
		fprintf(file, "%d\t%d\t%f\t%f\t%f\t%f\t%f\n", seqnum, state, env_in, env_out, ym, yc, ys);
		fflush(file);
		fclose(file);
		seqnum++;
	}
}

PORT
void print_buffer_parameters (const char* filename, int channel)
{
	IOB a = ch[channel].iob.pc;
	FILE* file = fopen (filename, "a");
	fprintf (file, "channel            = %d\n", channel);
	fprintf (file, "in_size            = %d\n", a->in_size);
	fprintf (file, "r1_outsize         = %d\n", a->r1_outsize);
	fprintf (file, "r1_size            = %d\n", a->r1_size);
	fprintf (file, "r2_size            = %d\n", a->r2_size);
	fprintf (file, "out_size           = %d\n", a->out_size);
	fprintf (file, "r2_insize          = %d\n", a->r2_insize);
	fprintf (file, "r1_active_buffsize = %d\n", a->r1_active_buffsize);
	fprintf (file, "f2_active_buffsize = %d\n", a->r2_active_buffsize);
	fprintf (file, "r1_inidx           = %d\n", a->r1_inidx);
	fprintf (file, "r1_outidx          = %d\n", a->r1_outidx);
	fprintf (file, "r1_unqueuedsamps   = %d\n", a->r1_unqueuedsamps);
	fprintf (file, "r2_inidx           = %d\n", a->r2_inidx);
	fprintf (file, "r2_outidx          = %d\n", a->r2_outidx);
	fprintf (file, "r2_havesamps       = %d\n", a->r2_havesamps);
	fprintf (file, "in_rate            = %d\n", ch[channel].in_rate);
	fprintf (file, "dsp_rate           = %d\n", ch[channel].dsp_rate);
	fprintf (file, "out_rate           = %d\n", ch[channel].out_rate);
	fprintf (file, "\n");
	fflush (file);
	fclose (file);
}

void print_meter (const char* filename, double* meter, int enum_av, int enum_pk, int enum_gain)
{
	FILE* file = fopen (filename, "a");
	if (enum_gain >= 0)
		fprintf (file, "%.4e\t%.4e\t%.4e\n", meter[enum_av], meter[enum_pk], meter[enum_gain]);
	else
		fprintf (file, "%.4e\t%.4e\n", meter[enum_av], meter[enum_pk]);
	fflush (file);
	fclose (file);
}

void print_message (const char* filename, const char* message, int p0, int p1, int p2)
{
	FILE* file = fopen (filename, "a");
	const char* msg = message;
	fprintf (file, "%s     %d     %d     %d\n", msg, p0, p1, p2);
	fflush (file);
	fclose (file);
}

void print_window_gain (const char* filename, int wintype, double inv_coherent_gain, double inherent_power_gain)
{
	FILE* file = fopen (filename, "a");
	double enb = inherent_power_gain * inv_coherent_gain * inv_coherent_gain;
	switch (wintype)
	{
	case 0:
		fprintf (file, "Rectangular             %.4f\t%.4f\t%.4f\n", inv_coherent_gain, inherent_power_gain, enb);
		break;
	case 1:
		fprintf (file, "Blackman-Harris 4-term  %.4f\t%.4f\t%.4f\n", inv_coherent_gain, inherent_power_gain, enb);
		break;
	case 2:
		fprintf (file, "Hann                    %.4f\t%.4f\t%.4f\n", inv_coherent_gain, inherent_power_gain, enb);
		break;
	case 3:
		fprintf (file, "Flat Top                %.4f\t%.4f\t%.4f\n", inv_coherent_gain, inherent_power_gain, enb);
		break;
	case 4:
		fprintf (file, "Hamming                 %.4f\t%.4f\t%.4f\n", inv_coherent_gain, inherent_power_gain, enb);
		break;
	case 5:
		fprintf (file, "Kaiser                  %.4f\t%.4f\t%.4f\n", inv_coherent_gain, inherent_power_gain, enb);
		break;
	case 6:
		fprintf (file, "Blackman-Harris 7-term  %.4f\t%.4f\t%.4f\n", inv_coherent_gain, inherent_power_gain, enb);
		break;
	}
	fflush (file);
	fclose (file);
}

void print_deviation (const char* filename, double dpmax, double rate)
{
	FILE* file = fopen (filename, "a");
	double peak = dpmax * rate / TWOPI;
	fprintf (file, "Peak Dev = %.4f\n", peak);
	fflush (file);
	fclose (file);
}

void __cdecl CalccPrintSamples (void *pargs)
{
	int i;
	double env_tx, env_rx;
	int channel = (int)(uintptr_t)pargs;
	CALCC a = txa[channel].calcc.p;
	FILE* file = fopen("samples.txt", "w");
	fprintf (file, "\n");
	for (i = 0; i < a->nsamps; i++)
	{
		env_tx = sqrt(a->txs[2 * i + 0] * a->txs[2 * i + 0] + a->txs[2 * i + 1] * a->txs[2 * i + 1]);
		env_rx = sqrt(a->rxs[2 * i + 0] * a->rxs[2 * i + 0] + a->rxs[2 * i + 1] * a->rxs[2 * i + 1]);
		fprintf(file, "%.12f  %.12f  %.12f      %.12f  %.12f  %.12f\n", 
			a->txs[2 * i + 0], a->txs[2 * i + 1], env_tx,
			a->rxs[2 * i + 0], a->rxs[2 * i + 1], env_rx);
	}
	fflush(file);
	fclose(file);
	_endthread();
}

void doCalccPrintSamples(int channel)
{	// no sample buffering - use in single cal mode
	_beginthread(CalccPrintSamples, 0, (void *)(uintptr_t)channel);
}

void print_anb_parms (const char* filename, ANB a)
{
	FILE* file = fopen (filename, "a");
	fprintf (file, "Run         = %d\n", a->run);
	fprintf (file, "Buffer Size = %d\n", a->buffsize);
	fprintf (file, "Sample Rate = %d\n", (int)a->samplerate);
	fprintf (file, "Threshold   = %.6f\n", a->threshold);
	fprintf (file, "BackTau     = %.6f\n", a->backtau);
	fprintf (file, "BackMult    = %.6f\n", a->backmult);
	fprintf (file, "Tau         = %.6f\n", a->tau);
	fflush (file);
	fclose (file);
}

// Audacity:  Import Raw Data, Signed 32-bit PCM, Little-endian, Mono/Stereo per mode selection, 48K rate

int audiocount = 0;
int* data;
int ready = 0;
int done = 0;

void WriteAudioFile(void* arg)
{
	byte* dat = (byte *)arg;
	FILE* file = fopen("AudioFile", "wb");

	// reverse bits of each byte (possibly needed on some platforms)
	// int i;
	// byte b;
	// for (i = 0; i < 4 * audiocount; i++)
	// {
	//	b = dat[i];
	// 	b = ((b >> 1) & 0x55) | ((b << 1) & 0xaa);
	// 	b = ((b >> 2) & 0x33) | ((b << 2) & 0xcc);
	// 	b = ((b >> 4) & 0x0f) | ((b << 4) & 0xf0);
	// 	dat[i] = b;
	// }
	fwrite((int *)dat, sizeof(int), audiocount, file);
	fflush(file);
	fclose(file);
	free(data);
	_endthread();
}

void WriteAudioWDSP (double seconds, int rate, int size, double* indata, int mode, double gain)
{
	// seconds - number of seconds of audio to record
	// rate - sample rate
	// size - number of complex samples
	// indata - pointer to data
	static int n;
	int i;
	const double conv = 2147483647.0 * gain;
	if (!ready)
	{
		if (mode != 3)
			n = (int)(seconds * rate);
		else
			n = 2 * (int)(seconds * rate);
		data = (int *)calloc(n, sizeof(int));
		ready = 1;
	}
	for (i = 0; i < size; i++)
	{
		if (audiocount < n)
		{
			switch (mode)
			{
			case 0:	// I only (mono)
				data[audiocount++] = (int)(conv * indata[2 * i + 0]);
				break;
			case 1: // Q only (mono)
				data[audiocount++] = (int)(conv * indata[2 * i + 1]);
				break;
			case 2: // envelope (mono)
				data[audiocount++] = (int)(conv * sqrt(indata[2 * i + 0] * indata[2 * i + 0] + indata[2 * i + 1] * indata[2 * i + 1]));
				break;
			case 3:	// complex samples (stereo)
				data[audiocount++] = (int)(conv * indata[2 * i + 0]);
				data[audiocount++] = (int)(conv * indata[2 * i + 1]);
				break;
			case 4: // double samples (mono)
				data[audiocount++] = (int)(conv * indata[i]);
				break;
			}
		}
	}
	if ((audiocount == n) && !done)
	{
		done = 1;
		_beginthread(WriteAudioFile, 0, (void *)data);
	}
}

void WriteScaledAudioFile (void* arg)
{
	typedef struct
	{
		int n;
		double* ddata;
	} *dstr;
	dstr dstruct = (dstr)arg;

	FILE* file = fopen("AudioFile", "wb");
	int i;
	double max = 0.0;
	double abs_val;
	const double conv = 2147483647.0;
	int *idata = (int *) malloc0 (dstruct->n * sizeof (int));

	for (i = 0; i < dstruct->n; i++)
	{
		abs_val = fabs (dstruct->ddata[i]);
		if (abs_val > max)
			max = abs_val;
	}
	for (i = 0; i < dstruct->n; i++)
		idata[i] = dstruct->ddata[i] >= 0.0 ? (int)floor(conv * dstruct->ddata[i] / max + 0.5) : (int)ceil(conv * dstruct->ddata[i] / max - 0.5);

	fwrite(idata, sizeof(int), dstruct->n, file);
	fflush(file);
	fclose(file);
	
	_aligned_free (dstruct->ddata);
	_aligned_free (dstruct);
	_aligned_free (idata);
	_endthread();
}

void WriteScaledAudio (
	double seconds,			// number of seconds of audio to record
	int rate,				// sample rate
	int size,				// incoming buffer size
	double* indata )		// pointer to incoming data buffer
{
	static int ready;
	typedef struct
	{
		int n;
		double* ddata;
	} dstr, *DSTR;
	static DSTR dstruct;

	static int count, complete;
	int i;
	
	if (!ready)
	{
		dstruct = (DSTR) malloc0 (sizeof (dstr));
		dstruct->n = 2 * (int)(seconds * rate);
		dstruct->ddata = (double *) malloc0 (dstruct->n * sizeof (double));
		ready = 1;
	}
	for (i = 0; i < size; i++)
	{
		if (count < dstruct->n)
		{
			dstruct->ddata[count++] = indata[2 * i + 0];
			dstruct->ddata[count++] = indata[2 * i + 1];
		}
	}
	if ((count >= dstruct->n) && !complete)
	{
		complete = 1;
		_beginthread (WriteScaledAudioFile, 0, (void *)dstruct);
	}
}