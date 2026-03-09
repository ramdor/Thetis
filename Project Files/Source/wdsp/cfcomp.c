/*  cfcomp.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2017, 2021 Warren Pratt, NR0V

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
mw0lge@grange-lane.co.uk - Richard Samphire (c) 2026

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

#include "comm.h"

// -- used in the cfc parametric code
#define TAIL_MIX 0.08		// the blend between the main filter lobe and the tail
#define TAIL_SCALE 2.5		// how wide the tail is relative to the main lobe
#define BW_REF_HZ 1000.0	// reference bandwidth for Q factor calculations (Hz). Q of 1 spreads +-1000Hz
#define MIN_SIGMA 1.0e-12	// minimum allowed bandwidth (sdev in Hz)
#define FWHM_TO_SIGMA (1.0 / sqrt(2.0 * log(2.0)))  // ~0.425  // Full-Width Half-Maximum (FWHM) to standard deviation for a Gaussian
// --

void calc_cfcwindow (CFCOMP a)
{
	int i;
	double arg0, arg1, cgsum, igsum, coherent_gain, inherent_power_gain, wmult;
	switch (a->wintype)
	{
	case 0:
		arg0 = 2.0 * PI / (double)a->fsize;
		cgsum = 0.0;
		igsum = 0.0;
		for (i = 0; i < a->fsize; i++)
		{
			a->window[i] = sqrt (0.54 - 0.46 * cos((double)i * arg0));
			cgsum += a->window[i];
			igsum += a->window[i] * a->window[i];
		}
		coherent_gain = cgsum / (double)a->fsize;
		inherent_power_gain = igsum / (double)a->fsize;
		wmult = 1.0 / sqrt (inherent_power_gain);
		for (i = 0; i < a->fsize; i++)
			a->window[i] *= wmult;
		a->winfudge = sqrt (1.0 / coherent_gain);
		break;
	case 1:
		arg0 = 2.0 * PI / (double)a->fsize;
		cgsum = 0.0;
		igsum = 0.0;
		for (i = 0; i < a->fsize; i++)
		{
			arg1 = cos(arg0 * (double)i);
			a->window[i]  = sqrt   (+0.21747
				          + arg1 * (-0.45325
				          + arg1 * (+0.28256
				          + arg1 * (-0.04672))));
			cgsum += a->window[i];
			igsum += a->window[i] * a->window[i];
		}
		coherent_gain = cgsum / (double)a->fsize;
		inherent_power_gain = igsum / (double)a->fsize;
		wmult = 1.0 / sqrt (inherent_power_gain);
		for (i = 0; i < a->fsize; i++)
			a->window[i] *= wmult;
		a->winfudge = sqrt (1.0 / coherent_gain);
		break;
	}
}

int fCOMPcompare (const void * a, const void * b)
{
	if (*(double*)a < *(double*)b)
		return -1;
	else if (*(double*)a == *(double*)b)
		return 0;
	else
		return 1;
}

void calc_comp(CFCOMP a)
{
	int i, j;
	double f, frac, fincr, fmax;
	double* sary;
	int use_qg;
	int use_qe;
	int recsz;

	a->precomplin = pow(10.0, 0.05 * a->precomp);
	a->prepeqlin = pow(10.0, 0.05 * a->prepeq);
	fmax = 0.5 * a->rate;
	use_qg = (a->Qg != NULL);
	use_qe = (a->Qe != NULL);
	recsz = 3 + use_qg + use_qe;

	for (i = 0; i < a->nfreqs; i++)
	{
		a->F[i] = max(a->F[i], 0.0);
		a->F[i] = min(a->F[i], fmax);
		a->G[i] = max(a->G[i], 0.0);
		if (use_qg) a->Qg[i] = max(a->Qg[i], 0.01);
		if (use_qe) a->Qe[i] = max(a->Qe[i], 0.01);
	}

	sary = (double*)malloc0(recsz * a->nfreqs * sizeof(double));
	if (sary == NULL)
		return;

	for (i = 0; i < a->nfreqs; i++)
	{
		sary[recsz * i + 0] = a->F[i];
		sary[recsz * i + 1] = a->G[i];
		sary[recsz * i + 2] = a->E[i];
		if (use_qg) sary[recsz * i + 3] = a->Qg[i];
		if (use_qe) sary[recsz * i + 3 + use_qg] = a->Qe[i];
	}
	qsort(sary, a->nfreqs, recsz * sizeof(double), fCOMPcompare);
	for (i = 0; i < a->nfreqs; i++)
	{
		a->F[i] = sary[recsz * i + 0];
		a->G[i] = sary[recsz * i + 1];
		a->E[i] = sary[recsz * i + 2];
		if (use_qg) a->Qg[i] = sary[recsz * i + 3];
		if (use_qe) a->Qe[i] = sary[recsz * i + 3 + use_qg];
	}
	_aligned_free(sary);

	a->fp[0] = 0.0;
	a->fp[a->nfreqs + 1] = fmax;
	a->gp[0] = a->G[0];
	a->gp[a->nfreqs + 1] = a->G[a->nfreqs - 1];
	a->ep[0] = a->E[0];
	a->ep[a->nfreqs + 1] = a->E[a->nfreqs - 1];
	for (i = 0, j = 1; i < a->nfreqs; i++, j++)
	{
		a->fp[j] = a->F[i];
		a->gp[j] = a->G[i];
		a->ep[j] = a->E[i];
	}

	fincr = a->rate / (double)a->fsize;

	if (!use_qg && !use_qe)
	{
		j = 0;
		for (i = 0; i < a->msize; i++)
		{
			f = fincr * (double)i;
			while (f >= a->fp[j + 1] && j < a->nfreqs) j++;
			frac = (f - a->fp[j]) / (a->fp[j + 1] - a->fp[j]);
			a->comp[i] = pow(10.0, 0.05 * (frac * a->gp[j + 1] + (1.0 - frac) * a->gp[j]));
			a->peq[i] = pow(10.0, 0.05 * (frac * a->ep[j + 1] + (1.0 - frac) * a->ep[j]));
			a->cfc_gain[i] = a->precomplin * a->comp[i];
		}
	}
	else
	{
		const double tail_norm = 1.0 / (1.0 + TAIL_MIX);
		const double tail_coeff = TAIL_MIX * tail_norm;
		const double tail_scale_inv = 1.0 / TAIL_SCALE;
		const double min_fwhm_bins = 2.0;
		const double q_sharpen = 1.0;
		double* fc_hz = (double*)malloc0(a->nfreqs * sizeof(double));
		double* sigma_inv_g = use_qg ? (double*)malloc0(a->nfreqs * sizeof(double)) : NULL;
		double* sigma_inv_e = use_qe ? (double*)malloc0(a->nfreqs * sizeof(double)) : NULL;
		double* gain_db = (double*)malloc0(a->nfreqs * sizeof(double));
		double* peq_db = (double*)malloc0(a->nfreqs * sizeof(double));
		int jg = 0;
		int je = 0;

		if (!fc_hz || !gain_db || !peq_db || (use_qg && !sigma_inv_g) || (use_qe && !sigma_inv_e))
		{
			_aligned_free(peq_db);
			_aligned_free(gain_db);
			_aligned_free(sigma_inv_e);
			_aligned_free(sigma_inv_g);
			_aligned_free(fc_hz);
			return;
		}

		for (i = 0; i < a->nfreqs; i++)
		{
			double min_fwhm_hz = min_fwhm_bins * fincr;
			fc_hz[i] = a->F[i];
			gain_db[i] = a->G[i];
			peq_db[i] = a->E[i];

			if (use_qg)
			{
				double qi = (a->Qg[i] > 0.0) ? a->Qg[i] : 1.0;
				double fwhm_hz = (q_sharpen * BW_REF_HZ) / qi;
				double sig;

				if (fwhm_hz < min_fwhm_hz) fwhm_hz = min_fwhm_hz;
				sig = (0.5 * fwhm_hz) * FWHM_TO_SIGMA;
				if (sig < MIN_SIGMA) sig = MIN_SIGMA;
				sigma_inv_g[i] = 1.0 / sig;
			}
			if (use_qe)
			{
				double qi = (a->Qe[i] > 0.0) ? a->Qe[i] : 1.0;
				double fwhm_hz = (q_sharpen * BW_REF_HZ) / qi;
				double sig;

				if (fwhm_hz < min_fwhm_hz) fwhm_hz = min_fwhm_hz;
				sig = (0.5 * fwhm_hz) * FWHM_TO_SIGMA;
				if (sig < MIN_SIGMA) sig = MIN_SIGMA;
				sigma_inv_e[i] = 1.0 / sig;
			}
		}

		for (i = 0; i < a->msize; i++)
		{
			double f_hz = fincr * (double)i;
			double gdb = 0.0;
			double edb = 0.0;

			if (use_qg)
			{
				for (j = 0; j < a->nfreqs; j++)
				{
					double df = f_hz - fc_hz[j];
					double x0 = df * sigma_inv_g[j];
					double w0 = exp(-0.5 * x0 * x0);
					double x1 = x0 * tail_scale_inv;
					double w1 = exp(-0.5 * x1 * x1);
					double w = (w0 + tail_coeff * w1) * tail_norm;
					gdb += gain_db[j] * w;
				}
			}
			else
			{
				while (f_hz >= a->fp[jg + 1] && jg < a->nfreqs) jg++;
				frac = (f_hz - a->fp[jg]) / (a->fp[jg + 1] - a->fp[jg]);
				gdb = frac * a->gp[jg + 1] + (1.0 - frac) * a->gp[jg];
			}

			if (use_qe)
			{
				for (j = 0; j < a->nfreqs; j++)
				{
					double df = f_hz - fc_hz[j];
					double x0 = df * sigma_inv_e[j];
					double w0 = exp(-0.5 * x0 * x0);
					double x1 = x0 * tail_scale_inv;
					double w1 = exp(-0.5 * x1 * x1);
					double w = (w0 + tail_coeff * w1) * tail_norm;
					edb += peq_db[j] * w;
				}
			}
			else
			{
				while (f_hz >= a->fp[je + 1] && je < a->nfreqs) je++;
				frac = (f_hz - a->fp[je]) / (a->fp[je + 1] - a->fp[je]);
				edb = frac * a->ep[je + 1] + (1.0 - frac) * a->ep[je];
			}

			a->comp[i] = pow(10.0, 0.05 * gdb);
			a->peq[i] = pow(10.0, 0.05 * edb);
			a->cfc_gain[i] = a->precomplin * a->comp[i];
		}

		_aligned_free(peq_db);
		_aligned_free(gain_db);
		_aligned_free(sigma_inv_e);
		_aligned_free(sigma_inv_g);
		_aligned_free(fc_hz);
	}
}

////original
//void calc_comp (CFCOMP a)
//{
//	int i, j;
//	double f, frac, fincr, fmax;
//	double* sary;
//	a->precomplin = pow (10.0, 0.05 * a->precomp);
//	a->prepeqlin  = pow (10.0, 0.05 * a->prepeq);
//	fmax = 0.5 * a->rate;
//	for (i = 0; i < a->nfreqs; i++)
//	{
//		a->F[i] = max (a->F[i], 0.0);
//		a->F[i] = min (a->F[i], fmax);
//		a->G[i] = max (a->G[i], 0.0);
//	}
//	sary = (double *)malloc0 (3 * a->nfreqs * sizeof (double));
//	for (i = 0; i < a->nfreqs; i++)
//	{
//		sary[3 * i + 0] = a->F[i];
//		sary[3 * i + 1] = a->G[i];
//		sary[3 * i + 2] = a->E[i];
//	}
//	qsort (sary, a->nfreqs, 3 * sizeof (double), fCOMPcompare);
//	for (i = 0; i < a->nfreqs; i++)
//	{
//		a->F[i] = sary[3 * i + 0];
//		a->G[i] = sary[3 * i + 1];
//		a->E[i] = sary[3 * i + 2];
//	}
//	_aligned_free (sary);
//	a->fp[0] = 0.0;
//	a->fp[a->nfreqs + 1] = fmax;
//	a->gp[0] = a->G[0];
//	a->gp[a->nfreqs + 1] = a->G[a->nfreqs - 1];
//	a->ep[0] = a->E[0];								// cutoff?
//	a->ep[a->nfreqs + 1] = a->E[a->nfreqs - 1];		// cutoff?
//	for (i = 0, j = 1; i < a->nfreqs; i++, j++)
//	{
//		a->fp[j] = a->F[i];
//		a->gp[j] = a->G[i];
//		a->ep[j] = a->E[i];
//	}
//	fincr = a->rate / (double)a->fsize;
//	j = 0;
//	// print_impulse ("gp.txt", a->nfreqs+2, a->gp, 0, 0);
//	for (i = 0; i < a->msize; i++)
//	{
//		f = fincr * (double)i;
//		while (f >= a->fp[j + 1] && j < a->nfreqs) j++;
//		frac = (f - a->fp[j]) / (a->fp[j + 1] - a->fp[j]);
//		a->comp[i] = pow (10.0, 0.05 * (frac * a->gp[j + 1] + (1.0 - frac) * a->gp[j]));
//		a->peq[i]  = pow (10.0, 0.05 * (frac * a->ep[j + 1] + (1.0 - frac) * a->ep[j]));
//		a->cfc_gain[i] = a->precomplin * a->comp[i];
//	}
//	// print_impulse ("comp.txt", a->msize, a->comp, 0, 0);
//}

void calc_cfcomp(CFCOMP a)
{
	int i;
	a->incr = a->fsize / a->ovrlp;
	if (a->fsize > a->bsize)
		a->iasize = a->fsize;
	else
		a->iasize = a->bsize + a->fsize - a->incr;
	a->iainidx = 0;
	a->iaoutidx = 0;
	if (a->fsize > a->bsize)
	{
		if (a->bsize > a->incr)  a->oasize = a->bsize;
		else					 a->oasize = a->incr;
		a->oainidx = (a->fsize - a->bsize - a->incr) % a->oasize;
	}
	else
	{
		a->oasize = a->bsize;
		a->oainidx = a->fsize - a->incr;
	}
	a->init_oainidx = a->oainidx;
	a->oaoutidx = 0;
	a->msize = a->fsize / 2 + 1;
	a->window    = (double *)malloc0 (a->fsize  * sizeof(double));
	a->inaccum   = (double *)malloc0 (a->iasize * sizeof(double));
	a->forfftin  = (double *)malloc0 (a->fsize  * sizeof(double));
	a->forfftout = (double *)malloc0 (a->msize  * sizeof(complex));
	a->cmask     = (double *)malloc0 (a->msize  * sizeof(double));
	a->mask      = (double *)malloc0 (a->msize  * sizeof(double));
	a->cfc_gain  = (double *)malloc0 (a->msize  * sizeof(double));
	a->revfftin  = (double *)malloc0 (a->msize  * sizeof(complex));
	a->revfftout = (double *)malloc0 (a->fsize  * sizeof(double));
	a->save      = (double **)malloc0(a->ovrlp  * sizeof(double *));
	for (i = 0; i < a->ovrlp; i++)
		a->save[i] = (double *)malloc0(a->fsize * sizeof(double));
	a->outaccum = (double *)malloc0(a->oasize * sizeof(double));
	a->nsamps = 0;
	a->saveidx = 0;
	a->Rfor = fftw_plan_dft_r2c_1d(a->fsize, a->forfftin, (fftw_complex *)a->forfftout, FFTW_ESTIMATE);
	a->Rrev = fftw_plan_dft_c2r_1d(a->fsize, (fftw_complex *)a->revfftin, a->revfftout, FFTW_ESTIMATE);
	calc_cfcwindow(a);

	a->pregain  = (2.0 * a->winfudge) / (double)a->fsize;
	a->postgain = 0.5 / ((double)a->ovrlp * a->winfudge);

	a->fp = (double *) malloc0 ((a->nfreqs + 2) * sizeof (double));
	a->gp = (double *) malloc0 ((a->nfreqs + 2) * sizeof (double));
	a->ep = (double *) malloc0 ((a->nfreqs + 2) * sizeof (double));
	a->comp = (double *) malloc0 (a->msize * sizeof (double));
	a->peq  = (double *) malloc0 (a->msize * sizeof (double));
	calc_comp (a);

	a->gain = 0.0;
	a->mmult = exp (-1.0 / (a->rate * a->ovrlp * a->mtau));
	a->dmult = exp (-(double)a->fsize / (a->rate * a->ovrlp * a->dtau));

	a->delta         = (double*)malloc0 (a->msize * sizeof(double));
	a->delta_copy    = (double*)malloc0 (a->msize * sizeof(double));
	a->cfc_gain_copy = (double*)malloc0 (a->msize * sizeof(double));
}

void decalc_cfcomp(CFCOMP a)
{
	int i;
	_aligned_free (a->cfc_gain_copy);
	_aligned_free (a->delta_copy);
	_aligned_free (a->delta);
	_aligned_free (a->peq);
	_aligned_free (a->comp);
	_aligned_free (a->ep);
	_aligned_free (a->gp);
	_aligned_free (a->fp);

	fftw_destroy_plan(a->Rrev);
	fftw_destroy_plan(a->Rfor);
	_aligned_free(a->outaccum);
	for (i = 0; i < a->ovrlp; i++)
		_aligned_free(a->save[i]);
	_aligned_free(a->save);
	_aligned_free(a->revfftout);
	_aligned_free(a->revfftin);
	_aligned_free(a->cfc_gain);
	_aligned_free(a->mask);
	_aligned_free(a->cmask);
	_aligned_free(a->forfftout);
	_aligned_free(a->forfftin);
	_aligned_free(a->inaccum);
	_aligned_free(a->window);
}

CFCOMP create_cfcomp (int run, int position, int peq_run, int size, double* in, double* out, int fsize, int ovrlp, 
	int rate, int wintype, int comp_method, int nfreqs, double precomp, double prepeq, double* F, double* G, double* E, double mtau, double dtau)
{
	CFCOMP a = (CFCOMP) malloc0 (sizeof (cfcomp));
	
	a->run = run;
	a->position = position;
	a->peq_run = peq_run;
	a->bsize = size;
	a->in = in;
	a->out = out;
	a->fsize = fsize;
	a->ovrlp = ovrlp;
	a->rate = rate;
	a->wintype = wintype;
	a->comp_method = comp_method;
	a->nfreqs = nfreqs;
	a->precomp = precomp;
	a->prepeq = prepeq;
	a->mtau = mtau;					// compression metering time constant
	a->dtau = dtau;					// compression display time constant
	a->F = (double *)malloc0 (a->nfreqs * sizeof (double));
	a->G = (double *)malloc0 (a->nfreqs * sizeof (double));
	a->E = (double *)malloc0 (a->nfreqs * sizeof (double));
	a->Qg = NULL;
	a->Qe = NULL;
	memcpy (a->F, F, a->nfreqs * sizeof (double));
	memcpy (a->G, G, a->nfreqs * sizeof (double));
	memcpy (a->E, E, a->nfreqs * sizeof (double));
	calc_cfcomp (a);
	return a;
}

void flush_cfcomp (CFCOMP a)
{
	int i;
	memset (a->inaccum, 0, a->iasize * sizeof (double));
	for (i = 0; i < a->ovrlp; i++)
		memset (a->save[i], 0, a->fsize * sizeof (double));
	memset (a->outaccum, 0, a->oasize * sizeof (double));
	a->nsamps   = 0;
	a->iainidx  = 0;
	a->iaoutidx = 0;
	a->oainidx  = a->init_oainidx;
	a->oaoutidx = 0;
	a->saveidx  = 0;
	a->gain = 0.0;
	memset(a->delta, 0, a->msize * sizeof(double));
}

void destroy_cfcomp (CFCOMP a)
{
	decalc_cfcomp (a);
	_aligned_free (a->E);
	_aligned_free (a->G);
	_aligned_free (a->F);
	_aligned_free (a->Qe);
	_aligned_free (a->Qg);
	_aligned_free (a);
}


void calc_mask (CFCOMP a)
{
	int i;
	double comp, mask, delta;
	switch (a->comp_method)
	{
	case 0:
		{
			double mag, test;
			for (i = 0; i < a->msize; i++)
			{
				mag = sqrt (a->forfftout[2 * i + 0] * a->forfftout[2 * i + 0] 
					      + a->forfftout[2 * i + 1] * a->forfftout[2 * i + 1]);
				comp = a->cfc_gain[i];
				test = comp * mag;
				if (test > 1.0)
					mask = 1.0 / mag;
				else
					mask = comp;
				a->cmask[i] = mask;
				if (test > a->gain) a->gain = test;
				else a->gain = a->mmult * a->gain;

				delta = a->cfc_gain[i] - a->cmask[i];
				if (delta > a->delta[i]) a->delta[i] = delta;
				else a->delta[i] *= a->dmult;
			}
			break;
		}
	}
	if (a->peq_run)
	{
		for (i = 0; i < a->msize; i++)
		{
			a->mask[i] = a->cmask[i] * a->prepeqlin * a->peq[i];
		}
	}
	else
		memcpy (a->mask, a->cmask, a->msize * sizeof (double));
	// print_impulse ("mask.txt", a->msize, a->mask, 0, 0);
	a->mask_ready = 1;
}

void xcfcomp (CFCOMP a, int pos)
{
	if (a->run && pos == a->position)
	{
		int i, j, k, sbuff, sbegin;
		for (i = 0; i < 2 * a->bsize; i += 2)
		{
			a->inaccum[a->iainidx] = a->in[i];
			a->iainidx = (a->iainidx + 1) % a->iasize;
		}
		a->nsamps += a->bsize;
		while (a->nsamps >= a->fsize)
		{
			for (i = 0, j = a->iaoutidx; i < a->fsize; i++, j = (j + 1) % a->iasize)
				a->forfftin[i] = a->pregain * a->window[i] * a->inaccum[j];
			a->iaoutidx = (a->iaoutidx + a->incr) % a->iasize;
			a->nsamps -= a->incr;
			fftw_execute (a->Rfor);
			calc_mask(a);
			for (i = 0; i < a->msize; i++)
			{
				a->revfftin[2 * i + 0] = a->mask[i] * a->forfftout[2 * i + 0];
				a->revfftin[2 * i + 1] = a->mask[i] * a->forfftout[2 * i + 1];
			}
			fftw_execute (a->Rrev);
			for (i = 0; i < a->fsize; i++)
				a->save[a->saveidx][i] = a->postgain * a->window[i] * a->revfftout[i];
			for (i = a->ovrlp; i > 0; i--)
			{
				sbuff = (a->saveidx + i) % a->ovrlp;
				sbegin = a->incr * (a->ovrlp - i);
				for (j = sbegin, k = a->oainidx; j < a->incr + sbegin; j++, k = (k + 1) % a->oasize)
				{
					if ( i == a->ovrlp)
						a->outaccum[k]  = a->save[sbuff][j];
					else
						a->outaccum[k] += a->save[sbuff][j];
				}
			}
			a->saveidx = (a->saveidx + 1) % a->ovrlp;
			a->oainidx = (a->oainidx + a->incr) % a->oasize;
		}
		for (i = 0; i < a->bsize; i++)
		{
			a->out[2 * i + 0] = a->outaccum[a->oaoutidx];
			a->out[2 * i + 1] = 0.0;
			a->oaoutidx = (a->oaoutidx + 1) % a->oasize;
		}
	}
	else if (a->out != a->in)
		memcpy (a->out, a->in, a->bsize * sizeof (complex));
}

void setBuffers_cfcomp (CFCOMP a, double* in, double* out)
{
	a->in = in;
	a->out = out;
}

void setSamplerate_cfcomp (CFCOMP a, int rate)
{
	decalc_cfcomp (a);
	a->rate = rate;
	calc_cfcomp (a);
}

void setSize_cfcomp (CFCOMP a, int size)
{
	decalc_cfcomp (a);
	a->bsize = size;
	calc_cfcomp (a);
}

/********************************************************************************************************
*																										*
*											TXA Properties												*
*																										*
********************************************************************************************************/

PORT
void SetTXACFCOMPRun (int channel, int run)
{
	CFCOMP a = txa[channel].cfcomp.p;
	if (a->run != run)
	{
		EnterCriticalSection (&ch[channel].csDSP);
		a->run = run;
		LeaveCriticalSection (&ch[channel].csDSP);
	}
}

PORT 
void SetTXACFCOMPPosition (int channel, int pos)
{
	CFCOMP a = txa[channel].cfcomp.p;
	if (a->position != pos)
	{
		EnterCriticalSection (&ch[channel].csDSP);
		a->position = pos;
		LeaveCriticalSection (&ch[channel].csDSP);
	}
}

PORT
void SetTXACFCOMPprofile (int channel, int nfreqs, double* F, double* G, double *E, double *Qg, double *Qe)
{
	CFCOMP a = txa[channel].cfcomp.p;
	EnterCriticalSection (&ch[channel].csDSP);
	a->nfreqs = nfreqs;
	_aligned_free (a->E);
	_aligned_free (a->F);
	_aligned_free (a->G);
	_aligned_free (a->Qg);
	_aligned_free (a->Qe);
	a->F = (double *)malloc0 (a->nfreqs * sizeof (double));
	a->G = (double *)malloc0 (a->nfreqs * sizeof (double));
	a->E = (double *)malloc0 (a->nfreqs * sizeof (double));
	if (Qg != NULL) {
		a->Qg = (double*)malloc0(a->nfreqs * sizeof(double));
	}
	else {
		a->Qg = NULL;
	}
	if (Qe != NULL) {
		a->Qe = (double*)malloc0(a->nfreqs * sizeof(double));
	}
	else {
		a->Qe = NULL;
	}
	memcpy (a->F, F, a->nfreqs * sizeof (double));
	memcpy (a->G, G, a->nfreqs * sizeof (double));
	memcpy (a->E, E, a->nfreqs * sizeof (double));
	if (Qg != NULL) {
		memcpy(a->Qg, Qg, a->nfreqs * sizeof(double));
	}
	if (Qe != NULL) {
		memcpy(a->Qe, Qe, a->nfreqs * sizeof(double));
	}
	_aligned_free (a->ep);
	_aligned_free (a->gp);
	_aligned_free (a->fp);
	a->fp = (double *) malloc0 ((a->nfreqs + 2) * sizeof (double));
	a->gp = (double *) malloc0 ((a->nfreqs + 2) * sizeof (double));
	a->ep = (double *) malloc0 ((a->nfreqs + 2) * sizeof (double));
	calc_comp (a);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetTXACFCOMPPrecomp (int channel, double precomp)
{
	CFCOMP a = txa[channel].cfcomp.p;
	if (a->precomp != precomp)
	{
		EnterCriticalSection (&ch[channel].csDSP);
		a->precomp = precomp;
		a->precomplin = pow (10.0, 0.05 * a->precomp);
		for (int i = 0; i < a->msize; i++)
		{
			a->cfc_gain[i] = a->precomplin * a->comp[i];
		}
		LeaveCriticalSection (&ch[channel].csDSP);
	}
}

PORT
void SetTXACFCOMPPeqRun (int channel, int run)
{
	CFCOMP a = txa[channel].cfcomp.p;
	if (a->peq_run != run)
	{
		EnterCriticalSection (&ch[channel].csDSP);
		a->peq_run = run;
		LeaveCriticalSection (&ch[channel].csDSP);
	}
}

PORT
void SetTXACFCOMPPrePeq (int channel, double prepeq)
{
	CFCOMP a = txa[channel].cfcomp.p;
	EnterCriticalSection (&ch[channel].csDSP);
	a->prepeq = prepeq;
	a->prepeqlin = pow (10.0, 0.05 * a->prepeq);
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void GetTXACFCOMPDisplayCompression (int channel, double* comp_values, int* ready)
{
	int i;
	CFCOMP a = txa[channel].cfcomp.p;
	EnterCriticalSection(&ch[channel].csDSP);
	if (*ready = a->mask_ready)
	{
		memcpy(a->delta_copy, a->delta, a->msize * sizeof(double));
		memcpy(a->cfc_gain_copy, a->cfc_gain, a->msize * sizeof(double));
		a->mask_ready = 0;
	}
	LeaveCriticalSection(&ch[channel].csDSP);
	if (*ready)
	{
		for (i = 0; i < a->msize; i++)
			comp_values[i] = 20.0 * mlog10 (a->cfc_gain_copy[i] / (a->cfc_gain_copy[i] - a->delta_copy[i]));
	}
}