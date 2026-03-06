/*  eq.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2013, 2016, 2017, 2025 Warren Pratt, NR0V

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

#include "comm.h"

int fEQcompare (const void * a, const void * b)
{
	if (*(double*)a < *(double*)b)
		return -1;
	else if (*(double*)a == *(double*)b)
		return 0;
	else
		return 1;
}


double* eq_impulse(int N, int nfreqs, double* F, double* G, double* Q, double samplerate, double scale, int ctfmode, int wintype)
{
	// check for previous in the cache
	struct Params 
	{
		int     N;
		int     nfreqs;
		int     ctfmode;
		int     wintype;
		double  samplerate;
		double  scale;
	};

	struct Params params;
	memset(&params, 0, sizeof(params));
	params.N = N;
	params.nfreqs = nfreqs;
	params.ctfmode = ctfmode;
	params.wintype = wintype;
	params.samplerate = samplerate;
	params.scale = scale;

	HASH_T h = fnv1a_hash(&params, sizeof(params));

	size_t arr_len = (nfreqs + 1) * sizeof(double);
	HASH_T hf = fnv1a_hash((uint8_t*)F, arr_len);
	h ^= hf + GOLDEN_RATIO + (h << 6) + (h >> 2);
	HASH_T hg = fnv1a_hash((uint8_t*)G, arr_len);
	h ^= hg + GOLDEN_RATIO + (h << 6) + (h >> 2);
	if (Q != NULL)
	{
		HASH_T hq = fnv1a_hash((uint8_t*)Q, arr_len);
		h ^= hq + GOLDEN_RATIO + (h << 6) + (h >> 2);
	}

	double* imp = get_impulse_cache_entry(EQ_CACHE, h, N);
	if (imp) return imp;

	double* impulse;
	double* sary;
	double f;
	double gpreamp, frac;
	int i, j;
	int mid = N / 2;
	double* fp = (double*)malloc0((nfreqs + 2) * sizeof(double));
	double* gp = (double*)malloc0((nfreqs + 2) * sizeof(double));
	double* A = (double*)malloc0((mid + 1) * sizeof(double));

	if (Q == NULL)
	{
		sary = (double*)malloc0(2 * nfreqs * sizeof(double));
		fp[0] = 0.0;
		fp[nfreqs + 1] = 1.0;
		gpreamp = G[0];
		for (i = 1; i <= nfreqs; i++)
		{
			fp[i] = 2.0 * F[i] / samplerate;
			if (fp[i] < 0.0) fp[i] = 0.0;
			if (fp[i] > 1.0) fp[i] = 1.0;
			gp[i] = G[i];
		}
		for (i = 1, j = 0; i <= nfreqs; i++, j += 2)
		{
			sary[j + 0] = fp[i];
			sary[j + 1] = gp[i];
		}
		qsort(sary, nfreqs, 2 * sizeof(double), fEQcompare);
		for (i = 1, j = 0; i <= nfreqs; i++, j += 2)
		{
			fp[i] = sary[j + 0];
			gp[i] = sary[j + 1];
		}
		gp[0] = gp[1];
		gp[nfreqs + 1] = gp[nfreqs];
		j = 0;
		if (N & 1)
		{
			for (i = 0; i <= mid; i++)
			{
				f = (double)i / (double)mid;
				while (f > fp[j + 1]) j++;
				frac = (f - fp[j]) / (fp[j + 1] - fp[j]);
				A[i] = pow(10.0, 0.05 * (frac * gp[j + 1] + (1.0 - frac) * gp[j] + gpreamp)) * scale;
			}
		}
		else
		{
			for (i = 0; i < mid; i++)
			{
				f = ((double)i + 0.5) / (double)mid;
				while (f > fp[j + 1]) j++;
				frac = (f - fp[j]) / (fp[j + 1] - fp[j]);
				A[i] = pow(10.0, 0.05 * (frac * gp[j + 1] + (1.0 - frac) * gp[j] + gpreamp)) * scale;
			}
		}

		_aligned_free(sary);
	}
	else
	{
		double low_fc_hz = samplerate * 0.5;
		double high_fc_hz = 0.0;
		double low_sigma_hz = 0.0;
		double high_sigma_hz = 0.0;
		double nyquist_hz = samplerate * 0.5;

		fp[0] = 0.0;
		fp[nfreqs + 1] = 1.0;

		gpreamp = G[0];

		double bin_offset = (N & 1) ? 0.0 : 0.5;
		double bin_hz = (mid > 0) ? (nyquist_hz / (double)mid) : 0.0;

		double tail_mix = 0.08;
		double tail_scale = 2.5;
		double tail_norm = 1.0 / (1.0 + tail_mix);
		double min_fwhm_bins = 2.0;
		double q_sharpen = 1.0;
		double bw_ref_hz = 1000.0;
		double edge_weight = 0.05;

		double* fc_hz = (double*)malloc0((nfreqs + 1) * sizeof(double));
		double* sigma_hz = (double*)malloc0((nfreqs + 1) * sizeof(double));
		double* gain_db = (double*)malloc0((nfreqs + 1) * sizeof(double));

		for (i = 1; i <= nfreqs; i++)
		{
			double fc_norm = 2.0 * F[i] / samplerate;
			if (fc_norm < 0.0) fc_norm = 0.0;
			if (fc_norm > 1.0) fc_norm = 1.0;

			double fci_hz = fc_norm * nyquist_hz;

			double qi = Q[i];
			if (!(qi > 0.0)) qi = 1.0;

			double fwhm_hz = (q_sharpen * bw_ref_hz) / qi;

			double min_fwhm_hz = min_fwhm_bins * bin_hz;
			if (fwhm_hz < min_fwhm_hz) fwhm_hz = min_fwhm_hz;

			double sig = (0.5 * fwhm_hz) / sqrt(2.0 * log(2.0));
			if (sig < 1.0e-12) sig = 1.0e-12;

			fc_hz[i] = fci_hz;
			sigma_hz[i] = sig;
			gain_db[i] = G[i];

			if ((i == 1) || (fci_hz < low_fc_hz) || ((fci_hz == low_fc_hz) && (sig > low_sigma_hz)))
			{
				low_fc_hz = fci_hz;
				low_sigma_hz = sig;
			}

			if ((i == 1) || (fci_hz > high_fc_hz) || ((fci_hz == high_fc_hz) && (sig > high_sigma_hz)))
			{
				high_fc_hz = fci_hz;
				high_sigma_hz = sig;
			}
		}

		if (N & 1)
		{
			for (i = 0; i <= mid; i++)
			{
				double f_hz = ((double)i + bin_offset) * bin_hz;
				double gdb = gpreamp;

				for (j = 1; j <= nfreqs; j++)
				{
					double df = f_hz - fc_hz[j];

					double x0 = df / sigma_hz[j];
					double w0 = exp(-0.5 * x0 * x0);

					double x1 = df / (sigma_hz[j] * tail_scale);
					double w1 = exp(-0.5 * x1 * x1);

					double w = (w0 + tail_mix * w1) * tail_norm;

					gdb += gain_db[j] * w;
				}

				A[i] = pow(10.0, 0.05 * gdb) * scale;
			}
		}
		else
		{
			for (i = 0; i < mid; i++)
			{
				double f_hz = ((double)i + bin_offset) * bin_hz;
				double gdb = gpreamp;

				for (j = 1; j <= nfreqs; j++)
				{
					double df = f_hz - fc_hz[j];

					double x0 = df / sigma_hz[j];
					double w0 = exp(-0.5 * x0 * x0);

					double x1 = df / (sigma_hz[j] * tail_scale);
					double w1 = exp(-0.5 * x1 * x1);

					double w = (w0 + tail_mix * w1) * tail_norm;

					gdb += gain_db[j] * w;
				}

				A[i] = pow(10.0, 0.05 * gdb) * scale;
			}
		}

		if (nfreqs > 0)
		{
			double tail_coeff = tail_mix * tail_norm;
			double edge_x;
			double low_edge_hz;
			double high_edge_hz;
			double min_low_edge_hz;
			double max_high_edge_hz;
			int low_reaches_dc = 0;
			int high_reaches_nyquist = 0;

			if ((tail_coeff > 0.0) && (edge_weight > 0.0) && (edge_weight < tail_coeff))
				edge_x = sqrt(-2.0 * log(edge_weight / tail_coeff));
			else
				edge_x = 2.0;

			low_edge_hz = low_fc_hz - (low_sigma_hz * tail_scale * edge_x);
			high_edge_hz = high_fc_hz + (high_sigma_hz * tail_scale * edge_x);

			if (low_edge_hz <= 0.0) low_reaches_dc = 1;
			if (high_edge_hz >= nyquist_hz) high_reaches_nyquist = 1;

			min_low_edge_hz = ((N & 1) ? 2.0 : 2.5) * bin_hz;
			max_high_edge_hz = (N & 1) ? nyquist_hz : (nyquist_hz - 0.5 * bin_hz);

			if (!low_reaches_dc && low_edge_hz < min_low_edge_hz) low_edge_hz = min_low_edge_hz;
			if (!high_reaches_nyquist && high_edge_hz > max_high_edge_hz) high_edge_hz = max_high_edge_hz;

			if (low_reaches_dc) low_edge_hz = 0.0;
			if (high_reaches_nyquist) high_edge_hz = max_high_edge_hz;

			if (high_edge_hz <= low_edge_hz)
			{
				if (low_reaches_dc)
				{
					high_edge_hz = bin_hz;
					if (high_edge_hz > max_high_edge_hz) high_edge_hz = max_high_edge_hz;
				}
				else
				{
					high_edge_hz = low_edge_hz + bin_hz;
					if (high_edge_hz > max_high_edge_hz) high_edge_hz = max_high_edge_hz;
					if (high_edge_hz <= low_edge_hz)
					{
						low_edge_hz = high_edge_hz - bin_hz;
						if (low_edge_hz < min_low_edge_hz) low_edge_hz = min_low_edge_hz;
					}
				}
			}

			fp[1] = low_edge_hz / nyquist_hz;
			fp[nfreqs] = high_edge_hz / nyquist_hz;
		}

		_aligned_free(gain_db);
		_aligned_free(sigma_hz);
		_aligned_free(fc_hz);
	}

	if (ctfmode == 0)
	{
		int k, low, high;
		double lowmag, highmag, flow4, fhigh4;
		if (N & 1)
		{
			low = (int)(fp[1] * mid);
			high = (int)(fp[nfreqs] * mid + 0.5);
			lowmag = A[low];
			highmag = A[high];
			flow4 = pow((double)low / (double)mid, 4.0);
			fhigh4 = pow((double)high / (double)mid, 4.0);
			k = low;
			while (--k >= 0)
			{
				f = (double)k / (double)mid;
				lowmag *= (f * f * f * f) / flow4;
				if (lowmag < 1.0e-100) lowmag = 1.0e-100;
				A[k] = lowmag;
			}
			k = high;
			while (++k <= mid)
			{
				f = (double)k / (double)mid;
				highmag *= fhigh4 / (f * f * f * f);
				if (highmag < 1.0e-100) highmag = 1.0e-100;
				A[k] = highmag;
			}
		}
		else
		{
			low = (int)(fp[1] * mid - 0.5);
			high = (int)(fp[nfreqs] * mid - 0.5);
			lowmag = A[low];
			highmag = A[high];
			flow4 = pow((double)low / (double)mid, 4.0);
			fhigh4 = pow((double)high / (double)mid, 4.0);
			k = low;
			while (--k >= 0)
			{
				f = (double)k / (double)mid;
				lowmag *= (f * f * f * f) / flow4;
				if (lowmag < 1.0e-100) lowmag = 1.0e-100;
				A[k] = lowmag;
			}
			k = high;
			while (++k < mid)
			{
				f = (double)k / (double)mid;
				highmag *= fhigh4 / (f * f * f * f);
				if (highmag < 1.0e-100) highmag = 1.0e-100;
				A[k] = highmag;
			}
		}
	}

	if (N & 1)
		impulse = fir_fsamp_odd(N, A, 1, 1.0, wintype);
	else
		impulse = fir_fsamp(N, A, 1, 1.0, wintype);
	// print_impulse("eq.txt", N, impulse, 1, 0);	
	_aligned_free(A);
	_aligned_free(gp);
	_aligned_free(fp);

	// store in cache
	add_impulse_to_cache(EQ_CACHE, h, N, impulse);

	return impulse;
}
 
////original
//double* eq_impulse(int N, int nfreqs, double* F, double* G, double samplerate, double scale, int ctfmode, int wintype)
//{
//	// check for previous in the cache
//	struct Params 
//	{
//		int     N;
//		int     nfreqs;
//		int     ctfmode;
//		int     wintype;
//		double  samplerate;
//		double  scale;
//	};
//
//	struct Params params;
//	memset(&params, 0, sizeof(params));
//	params.N = N;
//	params.nfreqs = nfreqs;
//	params.ctfmode = ctfmode;
//	params.wintype = wintype;
//	params.samplerate = samplerate;
//	params.scale = scale;
//
//	HASH_T h = fnv1a_hash(&params, sizeof(params));
//
//	size_t arr_len = (nfreqs + 1) * sizeof(double);
//	HASH_T hf = fnv1a_hash((uint8_t*)F, arr_len);
//	h ^= hf + GOLDEN_RATIO + (h << 6) + (h >> 2);
//	HASH_T hg = fnv1a_hash((uint8_t*)G, arr_len);
//	h ^= hg + GOLDEN_RATIO + (h << 6) + (h >> 2);
//
//	double* imp = get_impulse_cache_entry(EQ_CACHE, h, N);
//	if (imp) return imp;
//	//
//
//	double* fp = (double *) malloc0 ((nfreqs + 2)   * sizeof (double));
//	double* gp = (double *) malloc0 ((nfreqs + 2)   * sizeof (double));
//	double* A  = (double *) malloc0 ((N / 2 + 1) * sizeof (double));
//	double* sary = (double *) malloc0 (2 * nfreqs * sizeof (double));
//	double gpreamp, f, frac;
//	double* impulse;
//	int i, j, mid;
//	fp[0] = 0.0;
//	fp[nfreqs + 1] = 1.0;
//	gpreamp = G[0];
//	for (i = 1; i <= nfreqs; i++)
//	{
//		fp[i] = 2.0 * F[i] / samplerate;
//		if (fp[i] < 0.0) fp[i] = 0.0;
//		if (fp[i] > 1.0) fp[i] = 1.0;
//		gp[i] = G[i];
//	}
//	for (i = 1, j = 0; i <= nfreqs; i++, j+=2)
//	{
//		sary[j + 0] = fp[i];
//		sary[j + 1] = gp[i];
//	}
//	qsort (sary, nfreqs, 2 * sizeof (double), fEQcompare);
//	for (i = 1, j = 0; i <= nfreqs; i++, j+=2)
//	{
//		fp[i] = sary[j + 0];
//		gp[i] = sary[j + 1];
//	}
//	gp[0] = gp[1];
//	gp[nfreqs + 1] = gp[nfreqs];
//	mid = N / 2;
//	j = 0;
//	if (N & 1)
//	{
//		for (i = 0; i <= mid; i++)
//		{
//			f = (double)i / (double)mid;
//			while (f > fp[j + 1]) j++;
//			frac = (f - fp[j]) / (fp[j + 1] - fp[j]);
//			A[i] = pow (10.0, 0.05 * (frac * gp[j + 1] + (1.0 - frac) * gp[j] + gpreamp)) * scale;
//		}
//	}
//	else
//	{
//		for (i = 0; i < mid; i++)
//		{
//			f = ((double)i + 0.5) / (double)mid;
//			while (f > fp[j + 1]) j++;
//			frac = (f - fp[j]) / (fp[j + 1] - fp[j]);
//			A[i] = pow (10.0, 0.05 * (frac * gp[j + 1] + (1.0 - frac) * gp[j] + gpreamp)) * scale;
//		}
//	}
//	if (ctfmode == 0)
//	{
//		int k, low, high;
//		double lowmag, highmag, flow4, fhigh4;
//		if (N & 1)
//		{
//			low = (int)(fp[1] * mid);
//			high = (int)(fp[nfreqs] * mid + 0.5);
//			lowmag = A[low];
//			highmag = A[high];
//			flow4 = pow((double)low / (double)mid, 4.0);
//			fhigh4 = pow((double)high / (double)mid, 4.0);
//			k = low;
//			while (--k >= 0)
//			{
//				f = (double)k / (double)mid;
//				lowmag *= (f * f * f * f) / flow4;
//				if (lowmag < 1.0e-100) lowmag = 1.0e-100;
//				A[k] = lowmag;
//			}
//			k = high;
//			while (++k <= mid)
//			{
//				f = (double)k / (double)mid;
//				highmag *= fhigh4 / (f * f * f * f);
//				if (highmag < 1.0e-100) highmag = 1.0e-100;
//				A[k] = highmag;
//			}
//		}
//		else
//		{
//			low = (int)(fp[1] * mid - 0.5);
//			high = (int)(fp[nfreqs] * mid - 0.5);
//			lowmag = A[low];
//			highmag = A[high];
//			flow4 = pow((double)low / (double)mid, 4.0);
//			fhigh4 = pow((double)high / (double)mid, 4.0);
//			k = low;
//			while (--k >= 0)
//			{
//				f = (double)k / (double)mid;
//				lowmag *= (f * f * f * f) / flow4;
//				if (lowmag < 1.0e-100) lowmag = 1.0e-100;
//				A[k] = lowmag;
//			}
//			k = high;
//			while (++k < mid)
//			{
//				f = (double)k / (double)mid;
//				highmag *= fhigh4 / (f * f * f * f);
//				if (highmag < 1.0e-100) highmag = 1.0e-100;
//				A[k] = highmag;
//			}
//		}
//	}
//	if (N & 1)
//		impulse = fir_fsamp_odd(N, A, 1, 1.0, wintype);
//	else
//		impulse = fir_fsamp(N, A, 1, 1.0, wintype);
//	// print_impulse("eq.txt", N, impulse, 1, 0);
//	_aligned_free (sary);
//	_aligned_free (A);
//	_aligned_free (gp);
//	_aligned_free (fp);
//
//	// store in cache
//	add_impulse_to_cache(EQ_CACHE, h, N, impulse);
//
//	return impulse;
//}

/********************************************************************************************************
*																										*
*									Partitioned Overlap-Save Equalizer									*
*																										*
********************************************************************************************************/

EQP create_eqp (int run, int size, int nc, int mp, double *in, double *out, int nfreqs, double* F, double* G, int ctfmode, int wintype, int samplerate)
{
	// NOTE:  'nc' must be >= 'size'
	EQP a = (EQP) malloc0 (sizeof (eqp));
	double* impulse;
	a->run = run;
	a->size = size;
	a->nc = nc;
	a->mp = mp;
	a->in = in;
	a->out = out;
	a->nfreqs = nfreqs;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->Q = NULL;
	memcpy (a->F, F, (nfreqs + 1) * sizeof (double));
	memcpy (a->G, G, (nfreqs + 1) * sizeof (double));
	a->ctfmode = ctfmode;
	a->wintype = wintype;
	a->samplerate = (double)samplerate;
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	a->p = create_fircore (a->size, a->in, a->out, a->nc, a->mp, impulse);
	_aligned_free (impulse);
	return a;
}

void destroy_eqp (EQP a)
{
	destroy_fircore (a->p);
	_aligned_free (a);
}

void flush_eqp (EQP a)
{
	flush_fircore (a->p);
}

void xeqp (EQP a)
{
	if (a->run)
		xfircore (a->p);
	else
		memcpy (a->out, a->in, a->size * sizeof (complex));
}

void setBuffers_eqp (EQP a, double* in, double* out)
{
	a->in = in;
	a->out = out;
	setBuffers_fircore (a->p, a->in, a->out);
}

void setSamplerate_eqp (EQP a, int rate)
{
	double* impulse;
	a->samplerate = rate;
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

void setSize_eqp (EQP a, int size)
{
	double* impulse;
	a->size = size;
	setSize_fircore (a->p, a->size);
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

/********************************************************************************************************
*																										*
*							Partitioned Overlap-Save Equalizer:  RXA Properties							*
*																										*
********************************************************************************************************/

PORT
void SetRXAEQRun (int channel, int run)
{
	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].eqp.p->run = run;
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetRXAEQNC (int channel, int nc)
{
	EQP a;
	double* impulse;
	EnterCriticalSection (&ch[channel].csDSP);
	a = rxa[channel].eqp.p;
	if (a->nc != nc)
	{
		a->nc = nc;
		impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
		setNc_fircore (a->p, a->nc, impulse);
		_aligned_free (impulse);
	}
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetRXAEQMP (int channel, int mp)
{
	EQP a;
	a = rxa[channel].eqp.p;
	if (a->mp != mp)
	{
		a->mp = mp;
		setMp_fircore (a->p, a->mp);
	}
}

PORT
void SetRXAEQProfile (int channel, int nfreqs, double* F, double* G, double* Q)
{
	EQP a;
	double* impulse;
	a = rxa[channel].eqp.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	_aligned_free (a->Q);
	a->nfreqs = nfreqs;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	memcpy (a->F, F, (nfreqs + 1) * sizeof (double));
	memcpy (a->G, G, (nfreqs + 1) * sizeof (double));

	if (Q != NULL)
	{
		a->Q = (double*)malloc0((a->nfreqs + 1) * sizeof(double));
		memcpy(a->Q, Q, (nfreqs + 1) * sizeof(double));
	}
	else
		a->Q = NULL;

	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q,
		a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

PORT
void SetRXAEQCtfmode (int channel, int mode)
{
	EQP a;
	double* impulse;
	a = rxa[channel].eqp.p;
	a->ctfmode = mode;
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

PORT
void SetRXAEQWintype (int channel, int wintype)
{
	EQP a;
	double* impulse;
	a = rxa[channel].eqp.p;
	a->wintype = wintype;
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

PORT
void SetRXAGrphEQ (int channel, int *rxeq)
{	// three band equalizer (legacy compatibility)
	EQP a;
	double* impulse;
	a = rxa[channel].eqp.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	_aligned_free (a->Q);
	a->nfreqs = 4;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->Q = NULL;
	a->F[1] =  150.0;
	a->F[2] =  400.0;
	a->F[3] = 1500.0;
	a->F[4] = 6000.0;
	a->G[0] = (double)rxeq[0];
	a->G[1] = (double)rxeq[1];
	a->G[2] = (double)rxeq[1];
	a->G[3] = (double)rxeq[2];
	a->G[4] = (double)rxeq[3];
	a->ctfmode = 0;
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

PORT
void SetRXAGrphEQ10 (int channel, int *rxeq)
{	// ten band equalizer (legacy compatibility)
	EQP a;
	double* impulse;
	int i;
	a = rxa[channel].eqp.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	_aligned_free (a->Q);
	a->nfreqs = 10;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->Q = NULL;
	a->F[1]  =    32.0;
	a->F[2]  =    63.0;
	a->F[3]  =   125.0;
	a->F[4]  =   250.0;
	a->F[5]  =   500.0;
	a->F[6]  =  1000.0;
	a->F[7]  =  2000.0;
	a->F[8]  =  4000.0;
	a->F[9]  =  8000.0;
	a->F[10] = 16000.0;
	for (i = 0; i <= a->nfreqs; i++)
		a->G[i] = (double)rxeq[i];
	a->ctfmode = 0;
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	// print_impulse ("rxeq.txt", a->nc, impulse, 1, 0);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

/********************************************************************************************************
*																										*
*							Partitioned Overlap-Save Equalizer:  TXA Properties							*
*																										*
********************************************************************************************************/

PORT
void SetTXAEQRun (int channel, int run)
{
	EnterCriticalSection (&ch[channel].csDSP);
	txa[channel].eqp.p->run = run;
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetTXAEQNC (int channel, int nc)
{
	EQP a;
	double* impulse;
	EnterCriticalSection (&ch[channel].csDSP);
	a = txa[channel].eqp.p;
	if (a->nc != nc)
	{
		a->nc = nc;
		impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
		setNc_fircore (a->p, a->nc, impulse);
		_aligned_free (impulse);
	}
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetTXAEQMP (int channel, int mp)
{
	EQP a;
	a = txa[channel].eqp.p;
	if (a->mp != mp)
	{
		a->mp = mp;
		setMp_fircore (a->p, a->mp);
	}
}

PORT
void SetTXAEQProfile (int channel, int nfreqs, double* F, double* G, double* Q)
{
	EQP a;
	double* impulse;
	a = txa[channel].eqp.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	_aligned_free (a->Q);
	a->nfreqs = nfreqs;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	memcpy (a->F, F, (nfreqs + 1) * sizeof (double));
	memcpy (a->G, G, (nfreqs + 1) * sizeof (double));
	
	if(Q != NULL)
	{
		a->Q = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
		memcpy (a->Q, Q, (nfreqs + 1) * sizeof (double));
	}
	else
		a->Q = NULL;

	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

PORT
void SetTXAEQCtfmode (int channel, int mode)
{
	EQP a;
	double* impulse;
	a = txa[channel].eqp.p;
	a->ctfmode = mode;
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

PORT
void SetTXAEQWintype (int channel, int wintype)
{
	EQP a;
	double* impulse;
	a = txa[channel].eqp.p;
	a->wintype = wintype;
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

PORT
void SetTXAGrphEQ (int channel, int *txeq)
{	// three band equalizer (legacy compatibility)
	EQP a;
	double* impulse;
	a = txa[channel].eqp.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	_aligned_free (a->Q);
	a->nfreqs = 4;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->F[1] =  150.0;
	a->F[2] =  400.0;
	a->F[3] = 1500.0;
	a->F[4] = 6000.0;
	a->G[0] = (double)txeq[0];
	a->G[1] = (double)txeq[1];
	a->G[2] = (double)txeq[1];
	a->G[3] = (double)txeq[2];
	a->G[4] = (double)txeq[3];
	a->Q = NULL;
	a->ctfmode = 0;
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

PORT
void SetTXAGrphEQ10 (int channel, int *txeq)
{	// ten band equalizer (legacy compatibility)
	EQP a;
	double* impulse;
	int i;
	a = txa[channel].eqp.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	_aligned_free (a->Q);
	a->nfreqs = 10;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->F[1]  =    32.0;
	a->F[2]  =    63.0;
	a->F[3]  =   125.0;
	a->F[4]  =   250.0;
	a->F[5]  =   500.0;
	a->F[6]  =  1000.0;
	a->F[7]  =  2000.0;
	a->F[8]  =  4000.0;
	a->F[9]  =  8000.0;
	a->F[10] = 16000.0;
	for (i = 0; i <= a->nfreqs; i++)
		a->G[i] = (double)txeq[i];
	a->Q = NULL;
	a->ctfmode = 0;
	impulse = eq_impulse (a->nc, a->nfreqs, a->F, a->G, a->Q, a->samplerate, 1.0 / (2.0 * a->size), a->ctfmode, a->wintype);
	setImpulse_fircore (a->p, impulse, 1);
	_aligned_free (impulse);
}

/********************************************************************************************************
*																										*
*											Overlap-Save Equalizer										*
*																										*
********************************************************************************************************/


double* eq_mults (int size, int nfreqs, double* F, double* G, double* Q, double samplerate, double scale, int ctfmode, int wintype)
{
	double* impulse = eq_impulse (size + 1, nfreqs, F, G, Q, samplerate, scale, ctfmode, wintype);
	double* mults = fftcv_mults(2 * size, impulse);
	_aligned_free (impulse);
	return mults;
}

void calc_eq (EQ a)
{
	a->scale = 1.0 / (double)(2 * a->size);
	a->infilt = (double *)malloc0(2 * a->size * sizeof(complex));
	a->product = (double *)malloc0(2 * a->size * sizeof(complex));
	a->CFor = fftw_plan_dft_1d(2 * a->size, (fftw_complex *)a->infilt, (fftw_complex *)a->product, FFTW_FORWARD, FFTW_PATIENT);
	a->CRev = fftw_plan_dft_1d(2 * a->size, (fftw_complex *)a->product, (fftw_complex *)a->out, FFTW_BACKWARD, FFTW_PATIENT);
	a->mults = eq_mults(a->size, a->nfreqs, a->F, a->G, a->Q, a->samplerate, a->scale, a->ctfmode, a->wintype);
}

void decalc_eq (EQ a)
{
	fftw_destroy_plan(a->CRev);
	fftw_destroy_plan(a->CFor);
	_aligned_free(a->mults);
	_aligned_free(a->product);
	_aligned_free(a->infilt);
}

EQ create_eq (int run, int size, double *in, double *out, int nfreqs, double* F, double* G, int ctfmode, int wintype, int samplerate)
{
	EQ a = (EQ) malloc0 (sizeof (eq));
	a->run = run;
	a->size = size;
	a->in = in;
	a->out = out;
	a->nfreqs = nfreqs;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	memcpy (a->F, F, (nfreqs + 1) * sizeof (double));
	memcpy (a->G, G, (nfreqs + 1) * sizeof (double));
	a->ctfmode = ctfmode;
	a->wintype = wintype;
	a->samplerate = (double)samplerate;
	calc_eq (a);
	return a;
}

void destroy_eq (EQ a)
{
	decalc_eq (a);
	_aligned_free (a->G);
	_aligned_free (a->F);
	_aligned_free (a->Q);
	_aligned_free (a);
}

void flush_eq (EQ a)
{
	memset (a->infilt, 0, 2 * a->size * sizeof (complex));
}

void xeq (EQ a)
{
	int i;
	double I, Q;
	if (a->run)
	{
		memcpy (&(a->infilt[2 * a->size]), a->in, a->size * sizeof (complex));
		fftw_execute (a->CFor);
		for (i = 0; i < 2 * a->size; i++)
		{
			I = a->product[2 * i + 0];
			Q = a->product[2 * i + 1];
			a->product[2 * i + 0] = I * a->mults[2 * i + 0] - Q * a->mults[2 * i + 1];
			a->product[2 * i + 1] = I * a->mults[2 * i + 1] + Q * a->mults[2 * i + 0];
		}
		fftw_execute (a->CRev);
		memcpy (a->infilt, &(a->infilt[2 * a->size]), a->size * sizeof(complex));
	}
	else if (a->in != a->out)
		memcpy (a->out, a->in, a->size * sizeof (complex));
}

void setBuffers_eq (EQ a, double* in, double* out)
{
	decalc_eq (a);
	a->in = in;
	a->out = out;
	calc_eq (a);
}

void setSamplerate_eq (EQ a, int rate)
{
	decalc_eq (a);
	a->samplerate = rate;
	calc_eq (a);
}

void setSize_eq (EQ a, int size)
{
	decalc_eq (a);
	a->size = size;
	calc_eq (a);
}

/********************************************************************************************************
*																										*
*								Overlap-Save Equalizer:  RXA Properties									*
*																										*
********************************************************************************************************/
/*  // UNCOMMENT properties when a pointer is in place in rxa[channel]
PORT
void SetRXAEQRun (int channel, int run)
{
	EnterCriticalSection (&ch[channel].csDSP);
	rxa[channel].eq.p->run = run;
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetRXAEQProfile (int channel, int nfreqs, double* F, double* G)
{
	EQ a;
	EnterCriticalSection (&ch[channel].csDSP);
	a = rxa[channel].eq.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	a->nfreqs = nfreqs;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	memcpy (a->F, F, (nfreqs + 1) * sizeof (double));
	memcpy (a->G, G, (nfreqs + 1) * sizeof (double));
	_aligned_free (a->mults);
	a->mults = eq_mults (a->size, a->nfreqs, a->F, a->G, a->samplerate, a->scale, a->ctfmode, a->wintype);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetRXAEQCtfmode (int channel, int mode)
{
	EQ a;
	EnterCriticalSection (&ch[channel].csDSP);
	a = rxa[channel].eq.p;
	a->ctfmode = mode;
	_aligned_free (a->mults);
	a->mults = eq_mults (a->size, a->nfreqs, a->F, a->G, a->samplerate, a->scale, a->ctfmode, a->wintype);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetRXAEQWintype (int channel, int wintype)
{
	EQ a;
	EnterCriticalSection (&ch[channel].csDSP);
	a = rxa[channel].eq.p;
	a->wintype = wintype;
	_aligned_free (a->mults);
	a->mults = eq_mults (a->size, a->nfreqs, a->F, a->G, a->samplerate, a->scale, a->ctfmode, a->wintype);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetRXAGrphEQ (int channel, int *rxeq)
{	// three band equalizer (legacy compatibility)
	EQ a;
	EnterCriticalSection (&ch[channel].csDSP);
	a = rxa[channel].eq.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	a->nfreqs = 4;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->F[1] =  150.0;
	a->F[2] =  400.0;
	a->F[3] = 1500.0;
	a->F[4] = 6000.0;
	a->G[0] = (double)rxeq[0];
	a->G[1] = (double)rxeq[1];
	a->G[2] = (double)rxeq[1];
	a->G[3] = (double)rxeq[2];
	a->G[4] = (double)rxeq[3];
	a->ctfmode = 0;
	_aligned_free (a->mults);
	a->mults = eq_mults (a->size, a->nfreqs, a->F, a->G, a->samplerate, a->scale, a->ctfmode, a->wintype);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetRXAGrphEQ10 (int channel, int *rxeq)
{	// ten band equalizer (legacy compatibility)
	EQ a;
	int i;
	EnterCriticalSection (&ch[channel].csDSP);
	a = rxa[channel].eq.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	a->nfreqs = 10;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->F[1]  =    32.0;
	a->F[2]  =    63.0;
	a->F[3]  =   125.0;
	a->F[4]  =   250.0;
	a->F[5]  =   500.0;
	a->F[6]  =  1000.0;
	a->F[7]  =  2000.0;
	a->F[8]  =  4000.0;
	a->F[9]  =  8000.0;
	a->F[10] = 16000.0;
	for (i = 0; i <= a->nfreqs; i++)
		a->G[i] = (double)rxeq[i];
	a->ctfmode = 0;
	_aligned_free (a->mults);
	a->mults = eq_mults (a->size, a->nfreqs, a->F, a->G, a->samplerate, a->scale, a->ctfmode, a->wintype);
	LeaveCriticalSection (&ch[channel].csDSP);
}
*/
/********************************************************************************************************
*																										*
*								Overlap-Save Equalizer:  TXA Properties									*
*																										*
********************************************************************************************************/
/*  // UNCOMMENT properties when a pointer is in place in rxa[channel]
PORT
void SetTXAEQRun (int channel, int run)
{
	EnterCriticalSection (&ch[channel].csDSP);
	txa[channel].eq.p->run = run;
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetTXAEQProfile (int channel, int nfreqs, double* F, double* G)
{
	EQ a;
	EnterCriticalSection (&ch[channel].csDSP);
	a = txa[channel].eq.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	a->nfreqs = nfreqs;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	memcpy (a->F, F, (nfreqs + 1) * sizeof (double));
	memcpy (a->G, G, (nfreqs + 1) * sizeof (double));
	_aligned_free (a->mults);
	a->mults = eq_mults (a->size, a->nfreqs, a->F, a->G, a->samplerate, a->scale, a->ctfmode, a->wintype);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetTXAEQCtfmode (int channel, int mode)
{
	EQ a;
	EnterCriticalSection (&ch[channel].csDSP);
	a = txa[channel].eq.p;
	a->ctfmode = mode;
	_aligned_free (a->mults);
	a->mults = eq_mults (a->size, a->nfreqs, a->F, a->G, a->samplerate, a->scale, a->ctfmode, a->wintype);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetTXAEQMethod (int channel, int wintype)
{
	EQ a;
	EnterCriticalSection (&ch[channel].csDSP);
	a = txa[channel].eq.p;
	a->wintype = wintype;
	_aligned_free (a->mults);
	a->mults = eq_mults (a->size, a->nfreqs, a->F, a->G, a->samplerate, a->scale, a->ctfmode, a->wintype);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetTXAGrphEQ (int channel, int *txeq)
{	// three band equalizer (legacy compatibility)
	EQ a;
	EnterCriticalSection (&ch[channel].csDSP);
	a = txa[channel].eq.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	a->nfreqs = 4;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->F[1] =  150.0;
	a->F[2] =  400.0;
	a->F[3] = 1500.0;
	a->F[4] = 6000.0;
	a->G[0] = (double)txeq[0];
	a->G[1] = (double)txeq[1];
	a->G[2] = (double)txeq[1];
	a->G[3] = (double)txeq[2];
	a->G[4] = (double)txeq[3];
	a->ctfmode = 0;
	_aligned_free (a->mults);
	a->mults = eq_mults (a->size, a->nfreqs, a->F, a->G, a->samplerate, a->scale, a->ctfmode, a->wintype);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetTXAGrphEQ10 (int channel, int *txeq)
{	// ten band equalizer (legacy compatibility)
	EQ a;
	int i;
	EnterCriticalSection (&ch[channel].csDSP);
	a = txa[channel].eq.p;
	_aligned_free (a->G);
	_aligned_free (a->F);
	a->nfreqs = 10;
	a->F = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->G = (double *) malloc0 ((a->nfreqs + 1) * sizeof (double));
	a->F[1]  =    32.0;
	a->F[2]  =    63.0;
	a->F[3]  =   125.0;
	a->F[4]  =   250.0;
	a->F[5]  =   500.0;
	a->F[6]  =  1000.0;
	a->F[7]  =  2000.0;
	a->F[8]  =  4000.0;
	a->F[9]  =  8000.0;
	a->F[10] = 16000.0;
	for (i = 0; i <= a->nfreqs; i++)
		a->G[i] = (double)txeq[i];
	a->ctfmode = 0;
	_aligned_free (a->mults);
	a->mults = eq_mults (a->size, a->nfreqs, a->F, a->G, a->samplerate, a->scale, a->ctfmode, a->wintype);
	LeaveCriticalSection (&ch[channel].csDSP);
}
*/