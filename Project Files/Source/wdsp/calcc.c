/*  calcc.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2013, 2014, 2016, 2019, 2023 Warren Pratt, NR0V

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

void size_calcc (CALCC a)
{	// for change in ints or spi
	int i;
	a->nsamps = a->ints * a->spi;

	a->tsamps = a->nsamps + a->npsamps;
	a->env_TX = (double*)malloc0(a->nsamps * sizeof(double));
	a->env_RX = (double*)malloc0(a->nsamps * sizeof(double));
	a->x = (double*)malloc0(a->tsamps * sizeof(double));
	a->ym = (double*)malloc0(a->tsamps * sizeof(double));
	a->yc = (double*)malloc0(a->tsamps * sizeof(double));
	a->ys = (double*)malloc0(a->tsamps * sizeof(double));
	a->cat = (double*)malloc0(4 * a->nsamps * sizeof(double));

	a->t    = (double *) malloc0 ((a->ints + 1) * sizeof(double));
	a->tmap = (double *) malloc0 ((a->ints + 1) * sizeof(double));
	for (i = 0; i < a->ints + 1; i++)
		a->t[i] = (double)i / (double)a->ints;

	a->cm = (double *) malloc0 (a->ints * 4 * sizeof(double));
	a->cc = (double *) malloc0 (a->ints * 4 * sizeof(double));
	a->cs = (double *) malloc0 (a->ints * 4 * sizeof(double));
	a->cm_old = (double *) malloc0 (a->ints * 4 * sizeof (double));

	a->rxs = (double *) malloc0 (a->nsamps * sizeof (complex));
	a->txs = (double *) malloc0 (a->nsamps * sizeof (complex));

	a->ccbld = create_builder(a->nsamps + a->npsamps, a->ints);

	a->ctrl.cpi = (int *) malloc0 (a->ints * sizeof (int));
	a->ctrl.sindex = (int *) malloc0 (a->ints * sizeof (int));
	a->ctrl.sbase = (int *) malloc0 (a->ints * sizeof (int));

	for (i = 0; i < a->ints; i++)
	{
		a->ctrl.cpi[i] = 0;
		a->ctrl.sindex[i] = 0;
		a->ctrl.sbase[i] = i * a->spi;
	}

	a->disp.x  = (double *) malloc0 (a->nsamps * sizeof (double));
	a->disp.ym = (double *) malloc0 (a->nsamps * sizeof (double));
	a->disp.yc = (double *) malloc0 (a->nsamps * sizeof (double));
	a->disp.ys = (double *) malloc0 (a->nsamps * sizeof (double));
	a->disp.cm = (double *) malloc0 (a->ints * 4 * sizeof(double));
	a->disp.cc = (double *) malloc0 (a->ints * 4 * sizeof(double));
	a->disp.cs = (double *) malloc0 (a->ints * 4 * sizeof(double));

	a->util.pm = (double *) malloc0 (4 * a->util.ints * sizeof(double));
	a->util.pc = (double *) malloc0 (4 * a->util.ints * sizeof(double));
	a->util.ps = (double *) malloc0 (4 * a->util.ints * sizeof(double));
}

void desize_calcc (CALCC a)
{
	_aligned_free(a->util.pm);
	_aligned_free(a->util.pc);
	_aligned_free(a->util.ps);

	_aligned_free (a->disp.cs);
	_aligned_free (a->disp.cc);
	_aligned_free (a->disp.cm);
	_aligned_free (a->disp.ys);
	_aligned_free (a->disp.yc);
	_aligned_free (a->disp.ym);
	_aligned_free (a->disp.x);

	_aligned_free (a->ctrl.sbase);
	_aligned_free (a->ctrl.sindex);
	_aligned_free (a->ctrl.cpi);
	destroy_builder(a->ccbld);
	_aligned_free (a->rxs);
	_aligned_free (a->txs);
	_aligned_free (a->cm_old);
	_aligned_free (a->cm);
	_aligned_free (a->cc);
	_aligned_free (a->cs);
	_aligned_free (a->tmap);
	_aligned_free (a->t);

	_aligned_free(a->cat);
	_aligned_free(a->x);
	_aligned_free(a->ym);
	_aligned_free(a->yc);
	_aligned_free(a->ys);
	_aligned_free(a->env_TX);
	_aligned_free(a->env_RX);
}

CALCC create_calcc (int channel, int runcal, int size, int rate, int ints, int spi, double hw_scale, 
	double moxdelay, double loopdelay, double ptol, int mox, int solidmox, int pin, int map, int stbl,
	int npsamps, double alpha)
{
	CALCC a = (CALCC) malloc0 (sizeof (calcc));
	a->channel = channel;
	a->runcal = runcal;
	a->size = size;
	a->rate = rate;
	a->ints = ints;
	a->spi = spi;
	a->hw_scale = hw_scale;
	a->ctrl.moxdelay = moxdelay;
	a->ctrl.loopdelay = loopdelay;
	a->ptol = ptol;
	a->mox = mox;
	a->solidmox = solidmox;
	a->pin = pin;
	a->map = map;
	a->stbl = stbl;
	a->npsamps = npsamps;
	a->alpha = alpha;

	a->info  = (int *) malloc0 (16 * sizeof (int));
	a->binfo = (int *) malloc0 (16 * sizeof (int));

	a->ctrl.state = 0;
	a->ctrl.reset = 0;
	a->ctrl.automode = 0;
	a->ctrl.mancal = 0;
	a->ctrl.turnon = 0;
	a->ctrl.moxsamps = (int)(a->rate * a->ctrl.moxdelay);
	a->ctrl.moxcount = 0;
	a->ctrl.count = 0;
	a->ctrl.full_ints = 0;
	a->ctrl.calcinprogress = 0;
	a->ctrl.calcdone = 0;
	a->ctrl.waitsamps = (int)(a->rate * a->ctrl.loopdelay);
	a->ctrl.waitcount = 0;
	a->ctrl.running = 0;
	a->ctrl.current_state = 0;
	InitializeCriticalSectionAndSpinCount (&txa[a->channel].calcc.cs_update, 2500);
	InitializeCriticalSectionAndSpinCount (&a->ctrl.cs_SafeToEnd, 2500);
	a->rxdelay = create_delay (
		1,											// run
		0,											// size				[stuff later]
		0,											// input buffer		[stuff later]
		0,											// output buffer	[stuff later]
		a->rate,									// sample rate
		20.0e-09,									// delta (delay stepsize)
		0.0);										// delay
	a->txdelay = create_delay (
		1,											// run
		0,											// size				[stuff later]
		0,											// input buffer		[stuff later]
		0,											// output buffer	[stuff later]
		a->rate,									// sample rate
		20.0e-09,									// delta (delay stepsize)
		0.0);										// delay
	
	InitializeCriticalSectionAndSpinCount (&a->disp.cs_disp, 2500);
	a->util.ints = a->ints;
	a->util.channel = a->channel;

	size_calcc (a);

	a->temprx = (double*)malloc0(2048 * sizeof(complex));														// remove later
	a->temptx = (double*)malloc0(2048 * sizeof(complex));														// remove later

	// correction save and restore threads
	InterlockedBitTestAndReset(&a->savecorr_bypass, 0);
	a->Sem_SaveCorr = CreateSemaphore(0, 0, 1, 0);
	_beginthread(PSSaveCorrection, 0, (void*)a);
	InterlockedBitTestAndReset(&a->restcorr_bypass, 0);
	a->Sem_RestCorr = CreateSemaphore(0, 0, 1, 0);
	_beginthread(PSRestoreCorrection, 0, (void*)a);
	InterlockedBitTestAndReset(&a->calccorr_bypass, 0);
	a->Sem_CalcCorr = CreateSemaphore(0, 0, 1, 0);
	_beginthread(doPSCalcCorrection, 0, (void*)a);
	InterlockedBitTestAndReset(&a->turnoff_bypass, 0);
	a->Sem_TurnOff = CreateSemaphore(0, 0, 1, 0);
	_beginthread(doPSTurnoff, 0, (void*)a);

	return a;
}

void destroy_calcc (CALCC a)
{
	// correction save and restore threads
	InterlockedBitTestAndReset(&txa[a->channel].iqc.p1->busy, 0);
	Sleep(10);
	InterlockedBitTestAndSet(&a->savecorr_bypass, 0);
	ReleaseSemaphore(a->Sem_SaveCorr, 1, 0);
	while (InterlockedAnd(&a->savecorr_bypass, 0xffffffff)) Sleep(1);
	CloseHandle(a->Sem_SaveCorr);
	InterlockedBitTestAndSet(&a->restcorr_bypass, 0);
	ReleaseSemaphore(a->Sem_RestCorr, 1, 0);
	while (InterlockedAnd(&a->restcorr_bypass, 0xffffffff)) Sleep(1);
	CloseHandle(a->Sem_RestCorr);
	InterlockedBitTestAndSet(&a->calccorr_bypass, 0);
	ReleaseSemaphore(a->Sem_CalcCorr, 1, 0);
	while (InterlockedAnd(&a->calccorr_bypass, 0xffffffff)) Sleep(1);
	CloseHandle(a->Sem_CalcCorr);
	InterlockedBitTestAndSet(&a->turnoff_bypass, 0);
	ReleaseSemaphore(a->Sem_TurnOff, 1, 0);
	while (InterlockedAnd(&a->turnoff_bypass, 0xffffffff)) Sleep(1);
	CloseHandle(a->Sem_TurnOff);

	_aligned_free (a->temptx);																						// remove later
	_aligned_free (a->temprx);																						// remove later
	desize_calcc (a);
	DeleteCriticalSection (&a->disp.cs_disp);
	destroy_delay (a->txdelay);
	destroy_delay (a->rxdelay);
	DeleteCriticalSection (&a->ctrl.cs_SafeToEnd);
	DeleteCriticalSection (&txa[a->channel].calcc.cs_update);
	_aligned_free (a->binfo);
	_aligned_free (a->info);

	_aligned_free (a);
}

void flush_calcc (CALCC a)
{
	flush_delay (a->rxdelay);
	flush_delay (a->txdelay);
}

void scheck(CALCC a)
{
	int i, j, k;
	double v, dx, out, x, xold;
	int intm1 = a->ints - 1;
	const double diff_thresh = 0.05;
	a->binfo[6] = 0x0000;

	for (i = 0; i < 4 * a->ints; i++)
	{
		if (isnan (a->cm[i])) a->binfo[6] |= 0x0001;
		if (isnan (a->cc[i])) a->binfo[6] |= 0x0001;
		if (isnan (a->cs[i])) a->binfo[6] |= 0x0001;
	}

	for (i = 0; i < a->ints; i++)
		if ((a->cm[4 * i + 0] == 0.0) && (a->cm[4 * i + 1] == 0.0) &&
			(a->cm[4 * i + 2] == 0.0) && (a->cm[4 * i + 3] == 0.0)) a->binfo[6] |= 0x0002;

	for (i = 0; i < a->ints; i++)
	{
		for (j = 0; j < 4; j++)
		{
			k = 4 * i + j;
			v = (double)k / (4.0 * (double)a->ints);
			dx = (a->t[i + 1] - a->t[i]) * (double)j / 4.0;
			out = v * (a->cm[4 * i + 0] + dx * (a->cm[4 * i + 1] + dx * (a->cm[4 * i + 2] + dx * a->cm[4 * i + 3])));
			if (out > 1.0)
				a->binfo[6] |= 0x0004;
			if (out < 0.0) a->binfo[6] |= 0x0010;
		} 
	}

	dx = a->t[a->ints] - a->t[intm1];
	x = a->cm[4 * intm1 + 0] + dx * (a->cm[4 * intm1 + 1] + dx * (a->cm[4 * intm1 + 2] + dx * a->cm[4 * intm1 + 3]));

	if (x > 1.07)	// VALUE
		a->binfo[6] |= 0x0008;
	if (x < 0.0) a->binfo[6] |= 0x0020;

	for (i = 4; i < a->ints; i++)
		if (fabs (a->cm[4 * i + 0] - a->cm_old[4 * i + 0]) > diff_thresh) a->binfo[6] |= 0x0040;
	xold = a->cm_old[4 * intm1 + 0] + dx * (a->cm_old[4 * intm1 + 1] + dx * (a->cm_old[4 * intm1 + 2] + dx * a->cm_old[4 * intm1 + 3]));
	if (fabs (x - xold) > diff_thresh) a->binfo[6] |= 0x0040;
	memcpy (a->cm_old, a->cm, a->ints * 4 * sizeof(double));
}

void rxscheck (int rints, double* tvec, double* coef, int* info)
{
	int i, j, k;
	int rintsm1 = rints - 1;
	double v, dx, out;
	*info = 0x0000;
	for (i = 0; i < 4 * rints; i++)
		if (isnan (coef[i])) *info |= 0x0001;
	for (i = 0; i < rints; i++)
		if ((coef[4 * i + 0] == 0.0) && (coef[4 * i + 1] == 0.0) && (coef[4 * i + 2] == 0.0) && (coef[4 * i + 3] == 0.0))
			*info |= 0x0002;
	for (i = 0; i < rints; i++)
	{
		for (j = 0; j < 4; j++)
		{
			k = 4 * i + j;
			v = (double)k / (4.0 * (double)rints);
			dx = (tvec[i + 1] - tvec[i]) * (double)j / 4.0;
			out = v * (coef[4 * i + 0] + dx * (coef[4 * i + 1] + dx * (coef[4 * i + 2] + dx * coef[4 * i + 3])));
			if (out > 1.0)	// potentially use hw_scale here
				*info |= 0x0004;
			if (out < 0.0) *info |= 0x0010;
		}
	}
	dx = tvec[rints] - tvec[rints - 1];
	out = coef[4 * rintsm1 + 0] + dx * (coef[4 * rintsm1 + 1] + dx * (coef[4 * rintsm1 + 2] + dx * coef[4 * rintsm1 + 3]));
	if (out > 1.07) *info |= 0x0008;
	if (out < 0.00) *info |= 0x0020;
}

void calc (CALCC a)
{
	int i;
	double norm;
	for (i = 0; i < a->nsamps; i++)
	{
		a->env_TX[i] = sqrt (a->txs[2 * i + 0] * a->txs[2 * i + 0] + a->txs[2 * i + 1] * a->txs[2 * i + 1]);
		a->env_RX[i] = sqrt (a->rxs[2 * i + 0] * a->rxs[2 * i + 0] + a->rxs[2 * i + 1] * a->rxs[2 * i + 1]);
	}
	{
		int rints, ix;
		double dx;
		double tvec[3];
		double txrxcoefs[4 * 2];
		double rx_scale;
		if (a->ints < 16) rints = 1;
		else              rints = 2;
		ix = rints - 1;
		for (i = 0; i <= rints; i++)
			tvec[i] = (double)i / (double)rints / a->hw_scale;
		dx = tvec[rints] - tvec[rints - 1];
		xbuilder(a->ccbld, a->nsamps, a->env_TX, a->env_RX, rints, tvec, &(a->binfo[0]), txrxcoefs, a->ptol);
		rxscheck (rints, tvec, txrxcoefs, &a->binfo[7]);
		if ((a->binfo[0] == 0) && (a->binfo[7] == 0))
			rx_scale = 1.0 / (txrxcoefs[4 * ix + 0] + dx * (txrxcoefs[4 * ix + 1] + dx * (txrxcoefs[4 * ix + 2] + dx * txrxcoefs[4 * ix + 3])));
		else
		{
			a->scOK = 0;
			goto cleanup;
		}
		if (a->stbl && _InterlockedAnd (&a->ctrl.running, 1))
			a->rx_scale = a->alpha * a->rx_scale + (1.0 - a->alpha) * rx_scale;
		else
			a->rx_scale = rx_scale;
	}

	a->binfo[4] = (int)(256.0 * (a->hw_scale / a->rx_scale));
	a->binfo[5]++;

	if (a->pin)	// regress
	{
		const double slope = 0.001;
		double max_rx;
		for (i = 0; i < a->nsamps; i++)
		{
			max_rx = (1.0 - slope + slope * a->hw_scale * a->env_TX[i]) / a->rx_scale;
			if (a->env_RX[i] > max_rx)
				a->env_RX[i] = max_rx;
		}
	}

	for (i = 0; i < a->nsamps; i++)
	{
		norm = a->env_TX[i] * a->env_RX[i];
		a->x[i]  = a->rx_scale * a->env_RX[i];
		a->ym[i] = (a->hw_scale * a->env_TX[i]) / (a->rx_scale * a->env_RX[i]);
		a->yc[i] = (+ a->txs[2 * i + 0] * a->rxs[2 * i + 0] + a->txs[2 * i + 1] * a->rxs[2 * i + 1]) / norm;
		a->ys[i] = (- a->txs[2 * i + 0] * a->rxs[2 * i + 1] + a->txs[2 * i + 1] * a->rxs[2 * i + 0]) / norm;
		if (a->stbl && _InterlockedAnd (&a->ctrl.running, 1) && a->scOK)
		{
			int k;
			double dx, ymo, yco, yso;
			if ((k = (int)(a->x[i] * a->ints)) > a->ints - 1) k = a->ints - 1;
			dx = a->x[i] - a->t[k];
			ymo = a->cm[4 * k + 0] + dx * (a->cm[4 * k + 1] + dx * (a->cm[4 * k + 2] + dx * a->cm[4 * k + 3]));
			yco = a->cc[4 * k + 0] + dx * (a->cc[4 * k + 1] + dx * (a->cc[4 * k + 2] + dx * a->cc[4 * k + 3]));
			yso = a->cs[4 * k + 0] + dx * (a->cs[4 * k + 1] + dx * (a->cs[4 * k + 2] + dx * a->cs[4 * k + 3]));
			a->ym[i] = a->alpha * ymo + (1.0 - a->alpha) * a->ym[i];
			a->yc[i] = a->alpha * yco + (1.0 - a->alpha) * a->yc[i];
			a->ys[i] = a->alpha * yso + (1.0 - a->alpha) * a->ys[i];
		}
	}

	if (a->pin)	// pin
	{
		const double mval = 1.0e+00 - 1.0e-10;
		double cval, sval;
		for (i = 0; i < a->nsamps; i++)
		{
			a->cat[4 * i + 0] = a->x[i];
			a->cat[4 * i + 1] = a->ym[i];
			a->cat[4 * i + 2] = a->yc[i];
			a->cat[4 * i + 3] = a->ys[i];
		}
		qsort(a->cat, a->nsamps, 4 * sizeof(double), fcompare);
		for (i = 0; i < a->nsamps; i++)
		{
			a->x[i]  = a->cat[4 * i + 0];
			a->ym[i] = a->cat[4 * i + 1];
			a->yc[i] = a->cat[4 * i + 2];
			a->ys[i] = a->cat[4 * i + 3];
		}
		cval = 0.0;
		sval = 0.0;
		for (i = a->nsamps - 1; i > a->nsamps - 17; i--)
		{
			cval += a->yc[i];
			sval += a->ys[i];
		}
		cval /= 16.0;
		sval /= 16.0;
		for (i = a->nsamps; i < a->tsamps; i++)
		{
			a->x[i]  = mval;
			a->ym[i] = mval;
			a->yc[i] = cval;
			a->ys[i] = sval;
		}
		xbuilder(a->ccbld, a->tsamps, a->x, a->ym, a->ints, a->t, &(a->binfo[1]), a->cm, a->ptol);
		xbuilder(a->ccbld, a->tsamps, a->x, a->yc, a->ints, a->t, &(a->binfo[2]), a->cc, a->ptol);
		xbuilder(a->ccbld, a->tsamps, a->x, a->ys, a->ints, a->t, &(a->binfo[3]), a->cs, a->ptol);
	}
	else
	{
		xbuilder(a->ccbld, a->nsamps, a->x, a->ym, a->ints, a->t, &(a->binfo[1]), a->cm, a->ptol);
		xbuilder(a->ccbld, a->nsamps, a->x, a->yc, a->ints, a->t, &(a->binfo[2]), a->cc, a->ptol);
		xbuilder(a->ccbld, a->nsamps, a->x, a->ys, a->ints, a->t, &(a->binfo[3]), a->cs, a->ptol);
	}

	if (a->pin)	// tune
	{
		int k = a->ints - 1;
		double dx = a->t[a->ints] - a->t[a->ints - 1];
		double sf = 1.0 / (a->cm[4 * k + 0] + dx * (a->cm[4 * k + 1] + dx * (a->cm[4 * k + 2] + dx * a->cm[4 * k + 3])));
		for (i = 0; i < 4 * a->ints; i++)
			a->cm[i] *= sf;
	}

	scheck (a);
	a->scOK = ((a->binfo[0] == 0) && (a->binfo[1] == 0) && (a->binfo[2] == 0) && (a->binfo[3] == 0) && (a->binfo[6] == 0));

	if (a->scOK)	// map calc
	{
		for (i = 0; i < a->ints; i++)
			a->tmap[i] = a->cm[4 * i] * a->t[i];
		a->tmap[a->ints] = 1.0;
		a->convex = ((a->tmap[a->ints] - a->tmap[a->ints - 1]) > (a->t[a->ints] - a->t[a->ints - 1]));
	}

	EnterCriticalSection (&a->disp.cs_disp);
	memcpy(a->disp.x, a->x,  a->nsamps * sizeof (double));
	memcpy(a->disp.ym, a->ym, a->nsamps * sizeof (double));
	memcpy(a->disp.yc, a->yc, a->nsamps * sizeof (double));
	memcpy(a->disp.ys, a->ys, a->nsamps * sizeof (double));
	if (a->scOK)
	{
		memcpy(a->disp.cm, a->cm, a->ints * 4 * sizeof (double));
		memcpy(a->disp.cc, a->cc, a->ints * 4 * sizeof (double));
		memcpy(a->disp.cs, a->cs, a->ints * 4 * sizeof (double));
	}
	else
	{
		memset(a->disp.cm, 0, a->ints * 4 * sizeof (double));
		memset(a->disp.cc, 0, a->ints * 4 * sizeof (double));
		memset(a->disp.cs, 0, a->ints * 4 * sizeof (double));
	}
	LeaveCriticalSection (&a->disp.cs_disp);
cleanup:
	return;
}

void __cdecl doPSCalcCorrection (void *arg)
{
	CALCC a = (CALCC)arg;
	while (!InterlockedAnd(&a->calccorr_bypass, 0xffffffff))
	{
		WaitForSingleObject(a->Sem_CalcCorr, INFINITE);
		if (!InterlockedAnd(&a->calccorr_bypass, 0xffffffff))
		{
			calc(a);
			if (a->scOK)
			{
				EnterCriticalSection (&a->ctrl.cs_SafeToEnd);
				if (!InterlockedBitTestAndSet(&a->ctrl.running, 0))
					SetTXAiqcStart(a->channel, a->cm, a->cc, a->cs);
				else
					SetTXAiqcSwap(a->channel, a->cm, a->cc, a->cs);
				LeaveCriticalSection(&a->ctrl.cs_SafeToEnd);
			}
			InterlockedBitTestAndSet(&a->ctrl.calcdone, 0);
		}
	}
	InterlockedBitTestAndReset(&a->calccorr_bypass, 0);
}

void __cdecl doPSTurnoff (void *arg)
{
	CALCC a = (CALCC)arg;
	while (!InterlockedAnd(&a->turnoff_bypass, 0xffffffff))
	{
		WaitForSingleObject(a->Sem_TurnOff, INFINITE);
		if (!InterlockedAnd(&a->turnoff_bypass, 0xffffffff))
		{
			EnterCriticalSection(&a->ctrl.cs_SafeToEnd);
			SetTXAiqcEnd(a->channel);
			LeaveCriticalSection(&a->ctrl.cs_SafeToEnd);
		}
	}
	InterlockedBitTestAndReset(&a->turnoff_bypass, 0);
}

enum _calcc_state
{
	LRESET,
	LWAIT,
	LMOXDELAY,
	LSETUP,
	LCOLLECT,
	MOXCHECK,
	LCALC,
	LDELAY,
	LSTAYON,
	LTURNON
};

void __cdecl PSSaveCorrection (void *pargs)
{
	int i, k;
	CALCC a = (CALCC)pargs;
	while (!InterlockedAnd(&a->savecorr_bypass, 0xffffffff))
	{
		WaitForSingleObject(a->Sem_SaveCorr, INFINITE);
		if (!InterlockedAnd(&a->savecorr_bypass, 0xffffffff))
		{
			FILE* file = fopen(a->util.savefile, "w");
			if (file)
			{
			GetTXAiqcValues(a->util.channel, a->util.pm, a->util.pc, a->util.ps);
			for (i = 0; i < a->util.ints; i++)
			{
				for (k = 0; k < 4; k++)
					fprintf(file, "%.17e\t", a->util.pm[4 * i + k]);
				fprintf(file, "\n");
				for (k = 0; k < 4; k++)
					fprintf(file, "%.17e\t", a->util.pc[4 * i + k]);
				fprintf(file, "\n");
				for (k = 0; k < 4; k++)
					fprintf(file, "%.17e\t", a->util.ps[4 * i + k]);
				fprintf(file, "\n\n");
			}
			fflush(file);
			fclose(file);
		}
	}
	}
	InterlockedBitTestAndReset(&a->savecorr_bypass, 0);
}

void __cdecl PSRestoreCorrection(void *pargs)
{
	int i, k;
	CALCC a = (CALCC)pargs;
	while (!InterlockedAnd(&a->restcorr_bypass, 0xffffffff))
	{
		WaitForSingleObject(a->Sem_RestCorr, INFINITE);
		if (!InterlockedAnd(&a->restcorr_bypass, 0xffffffff))
		{
			FILE* file = fopen(a->util.restfile, "r");
			if (file)
			{
				int error = 0;
			for (i = 0; i < a->util.ints; i++)
			{
					// no additional reads after first error occurs
				for (k = 0; k < 4; k++)
						if (error == 0 && fscanf(file, "%le", &(a->util.pm[4 * i + k])) != 1) error = 1;
				for (k = 0; k < 4; k++)
						if (error == 0 && fscanf(file, "%le", &(a->util.pc[4 * i + k])) != 1) error = 1;
				for (k = 0; k < 4; k++)
						if (error == 0 && fscanf(file, "%le", &(a->util.ps[4 * i + k])) != 1) error = 1;
			}
			fclose(file);
				if (!error)
				{
			if (!InterlockedBitTestAndSet(&a->ctrl.running, 0))
				SetTXAiqcStart(a->channel, a->util.pm, a->util.pc, a->util.ps);
			else
				SetTXAiqcSwap(a->channel, a->util.pm, a->util.pc, a->util.ps);
		}
	}
		}
	}
	InterlockedBitTestAndReset(&a->restcorr_bypass, 0);
}


/********************************************************************************************************
*																										*
*										  Public Functions												*
*																										*
********************************************************************************************************/

PORT
void pscc (int channel, int size, double* tx, double* rx)
{
	int i, n, m;
	double env;
	CALCC a;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	if (a->runcal)
	{
		a->size = size;
		if (InterlockedAnd (&a->mox, 1) && (a->txdelay->tdelay != 0.0 || a->rxdelay->tdelay != 0.0))
		{
			SetDelayBuffs (a->rxdelay, a->size, rx, rx);
			xdelay (a->rxdelay);
			SetDelayBuffs (a->txdelay, a->size, tx, tx);
			xdelay (a->txdelay);
		}
		a->info[15] = a->ctrl.state;

		switch (a->ctrl.state)
		{
			case LRESET:
				InterlockedExchange (&a->ctrl.current_state, LRESET);
				a->ctrl.reset = 0;
				if (!a->ctrl.turnon)
					if (InterlockedBitTestAndReset(&a->ctrl.running, 0))
						ReleaseSemaphore(a->Sem_TurnOff, 1, 0);
				a->info[14] = 0;
				a->ctrl.env_maxtx = 0.0;
				a->ctrl.bs_count = 0;
				if (a->ctrl.turnon)
					a->ctrl.state = LTURNON;
				else if (a->ctrl.automode || a->ctrl.mancal)
					a->ctrl.state = LWAIT;
				break;
			case LWAIT:
				InterlockedExchange (&a->ctrl.current_state, LWAIT);
				a->ctrl.mancal = 0;
				a->ctrl.moxcount = 0;
				if (a->ctrl.reset)
					a->ctrl.state = LRESET;
				else if (a->ctrl.turnon)
					a->ctrl.state = LTURNON;
				else if (InterlockedAnd (&a->mox, 1))
				{
					a->ctrl.state = LMOXDELAY;
					InterlockedBitTestAndSet (&a->solidmox, 0);
				}
				break;
			case LMOXDELAY:
				InterlockedExchange (&a->ctrl.current_state, LMOXDELAY);
				a->ctrl.moxcount += a->size;
				if (a->ctrl.reset)
					a->ctrl.state = LRESET;
				else if (a->ctrl.turnon)
					a->ctrl.state = LTURNON;
				else if (!InterlockedAnd (&a->mox, 1) || !InterlockedAnd (&a->solidmox, 1))
					a->ctrl.state = LWAIT;
				else if ((a->ctrl.moxcount - a->size) >= a->ctrl.moxsamps)
					a->ctrl.state = LSETUP;
				break;
			case LSETUP:
				InterlockedExchange (&a->ctrl.current_state, LSETUP);
				a->ctrl.count = 0;
				for (i = 0; i < a->ints; i++)
				{
					a->ctrl.cpi[i] = 0;
					a->ctrl.sindex[i] = 0;
				}
				a->ctrl.full_ints = 0;
				a->ctrl.waitcount = 0;
				
				if (a->ctrl.reset)
					a->ctrl.state = LRESET;
				else if (a->ctrl.turnon)
					a->ctrl.state = LTURNON;
				else if (InterlockedAnd (&a->mox, 1) && InterlockedAnd (&a->solidmox, 1))
				{
					a->ctrl.state = LCOLLECT;
					SetTXAiqcDogCount (channel, a->info[13] = 0);
				}
				else
					a->ctrl.state = LWAIT;
				break;
			case LCOLLECT:
				InterlockedExchange (&a->ctrl.current_state, LCOLLECT);
				for (i = 0; i < a->size; i++)
				{
					env = sqrt(tx[2 * i + 0] * tx[2 * i + 0] + tx[2 * i + 1] * tx[2 * i + 1]);
					if (env > a->ctrl.env_maxtx)
						a->ctrl.env_maxtx = env;
					if ((env *= a->hw_scale) <= 1.0)
					{
						if (env == 1.0)
							n = a->ints - 1;
						else if (a->map && _InterlockedAnd (&a->ctrl.running, 1) && a->convex)
						{
							int nmin = 0;
							int nmax = a->ints;
							n = (nmin + nmax) / 2;
							while (nmax - nmin > 1)
							{
								n = (nmin + nmax) / 2;
								if (env >= a->tmap[n])
									nmin = n;
								else
									nmax = n;
							}
							if (env < a->tmap[n]) n--;
						}
						else
							n = (int)(env * (double)a->ints);
						m = a->ctrl.sbase[n] + a->ctrl.sindex[n];
						a->txs[2 * m + 0] = tx[2 * i + 0];
						a->txs[2 * m + 1] = tx[2 * i + 1];
						a->rxs[2 * m + 0] = rx[2 * i + 0];
						a->rxs[2 * m + 1] = rx[2 * i + 1];
						if (++a->ctrl.sindex[n] == a->spi) a->ctrl.sindex[n] = 0;
						if (a->ctrl.cpi[n] != a->spi)
							if (++a->ctrl.cpi[n] == a->spi) a->ctrl.full_ints++;
						++a->ctrl.count;
					}
				}
				GetTXAiqcDogCount (channel, &a->info[13]);
				if (a->ctrl.reset)
					a->ctrl.state = LRESET;
				else if (a->ctrl.turnon)
					a->ctrl.state = LTURNON;
				else if (!InterlockedAnd (&a->mox, 1) || !InterlockedAnd (&a->solidmox, 1))
					a->ctrl.state = LWAIT;
				else if (a->ctrl.full_ints == a->ints)
					a->ctrl.state = MOXCHECK;
				else if (a->info[13] >= 6)
					a->ctrl.state = LRESET;
				else if (a->ctrl.count >= 4 * a->rate)
				{
					a->ctrl.count = 0;
					for (i = 0; i < a->ints; i++)
					{
						a->ctrl.cpi[i] = 0;
						a->ctrl.sindex[i] = 0;
					}
					a->ctrl.full_ints = 0;
				}
				break;
			case MOXCHECK:
				InterlockedExchange (&a->ctrl.current_state, MOXCHECK);
				if (a->ctrl.reset)
					a->ctrl.state = LRESET;
				else if (a->ctrl.turnon)
					a->ctrl.state = LTURNON;
				else if (!InterlockedAnd (&a->mox, 1) || !InterlockedAnd (&a->solidmox, 1))
					a->ctrl.state = LWAIT;
				else
					a->ctrl.state = LCALC;
				break;
			case LCALC:
				InterlockedExchange (&a->ctrl.current_state, LCALC);
				if (!a->ctrl.calcinprogress)	
				{
					a->ctrl.calcinprogress = 1;
					ReleaseSemaphore(a->Sem_CalcCorr, 1, 0);
				}

				if (InterlockedBitTestAndReset(&a->ctrl.calcdone, 0))
				{
					memcpy (a->info, a->binfo, 8 * sizeof (int));
					a->info[14] = _InterlockedAnd (&a->ctrl.running, 1);
					a->ctrl.calcinprogress = 0;
					if (a->ctrl.reset)
						a->ctrl.state = LRESET;
					else if (a->ctrl.turnon)
						a->ctrl.state = LTURNON;
					else if (a->scOK)
					{
						a->ctrl.bs_count = 0;
						a->ctrl.state = LDELAY;
					}
					else if (++(a->ctrl.bs_count) >= 2)
						a->ctrl.state = LRESET;
					else if (InterlockedAnd (&a->mox, 1) && InterlockedAnd (&a->solidmox, 1)) 
						a->ctrl.state = LSETUP;
					else a->ctrl.state = LWAIT;
				}
				break;
			case LDELAY:
				InterlockedExchange (&a->ctrl.current_state, LDELAY);
				a->ctrl.waitcount += a->size;
				if (a->ctrl.reset)
					a->ctrl.state = LRESET;
				else if (a->ctrl.turnon)
					a->ctrl.state = LTURNON;
				else if ((a->ctrl.waitcount - a->size) >= a->ctrl.waitsamps)
				{
					if (a->ctrl.automode)
					{
						if (InterlockedAnd (&a->mox, 1) && InterlockedAnd (&a->solidmox, 1))
							a->ctrl.state = LSETUP;
						else
							a->ctrl.state = LWAIT;
					}
					else
						a->ctrl.state = LSTAYON;
				}
				break;
			case LSTAYON:
				InterlockedExchange (&a->ctrl.current_state, LSTAYON);
				if (a->ctrl.reset || a->ctrl.automode || a->ctrl.mancal)
					a->ctrl.state = LRESET;
				break;
			case LTURNON:
				InterlockedExchange (&a->ctrl.current_state, LTURNON);
				a->ctrl.turnon = 0;
				a->ctrl.automode = 0;
				a->info[14] = 1;
				a->ctrl.state = LSTAYON;
				break;
		}
	}
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT 
void psccF (int channel, int size, float *Itxbuff, float *Qtxbuff, float *Irxbuff, float *Qrxbuff, int mox, int solidmox)
{
	int i;
	CALCC a;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	// a->mox = mox;
	// a->solidmox = solidmox;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
	for (i = 0; i < size; i++)
	{
		a->temptx[2 * i + 0] = (double)Itxbuff[i];
		a->temptx[2 * i + 1] = (double)Qtxbuff[i];
		a->temprx[2 * i + 0] = (double)Irxbuff[i];
		a->temprx[2 * i + 1] = (double)Qrxbuff[i];
	}
	pscc (channel, size, a->temptx, a->temprx);
}

PORT
void PSSaveCorr (int channel, char* filename)
{
	CALCC a;
	int i = 0;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	while (a->util.savefile[i++] = *filename++);
	ReleaseSemaphore(a->Sem_SaveCorr, 1, 0);
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void PSRestoreCorr (int channel, char* filename)
{
	CALCC a;
	int i = 0;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	while (a->util.restfile[i++] = *filename++);
	a->ctrl.turnon = 1;
	ReleaseSemaphore(a->Sem_RestCorr, 1, 0);
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

/********************************************************************************************************
*																										*
*											  Properties												*
*																										*
********************************************************************************************************/

PORT
void SetPSRunCal (int channel, int run)
{
	CALCC a;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	a->runcal = run;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSMox (int channel, int mox)
{
	CALCC a = txa[channel].calcc.p;;
	if (mox)
		InterlockedBitTestAndSet (&a->mox, 0);
	else
	{
		InterlockedBitTestAndReset (&a->mox, 0);
		InterlockedBitTestAndReset (&a->solidmox, 0);
	}
}

PORT 
void GetPSInfo (int channel, int *info)
{
	CALCC a;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	memcpy (info, a->info, 16 * sizeof(int));
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSReset (int channel, int reset)
{
	CALCC a;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	a->ctrl.reset = reset;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSMancal (int channel, int mancal)
{
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	txa[channel].calcc.p->ctrl.mancal = mancal;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSAutomode (int channel, int automode)
{
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	txa[channel].calcc.p->ctrl.automode = automode;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSTurnon (int channel, int turnon)
{
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	txa[channel].calcc.p->ctrl.turnon = turnon;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSControl (int channel, int reset, int mancal, int automode, int turnon)
{
	CALCC a;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	a->ctrl.reset = reset;
	a->ctrl.mancal = mancal;
	a->ctrl.automode = automode;
	a->ctrl.turnon = turnon;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSLoopDelay (int channel, double delay)
{
	CALCC a;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	a->ctrl.loopdelay = delay;
	a->ctrl.waitsamps = (int)(a->rate * a->ctrl.loopdelay);
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSMoxDelay (int channel, double delay)
{
	CALCC a;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	a->ctrl.moxdelay = delay;
	a->ctrl.moxsamps = (int)(a->rate * a->ctrl.moxdelay);
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
double SetPSTXDelay (int channel, double delay)
{
	CALCC a;
	double adelay;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	a->txdel = delay;
	if (a->txdel >= 0.0)
	{
		adelay = SetDelayValue (a->txdelay, a->txdel);
		SetDelayValue (a->rxdelay, 0.0);
	}
	else
	{
		adelay = -SetDelayValue (a->rxdelay, -a->txdel);
		SetDelayValue (a->txdelay, 0.0);
	}
	//adelay = SetDelayValue (a->txdelay, a->txdel);
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
	return adelay;
}

PORT
void SetPSHWPeak (int channel, double peak)
{
	CALCC a;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a = txa[channel].calcc.p;
	a->hw_scale = 1.0 / peak;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void GetPSHWPeak (int channel, double* peak)
{
EnterCriticalSection (&txa[channel].calcc.cs_update);
*peak = 1.0 / txa[channel].calcc.p->hw_scale;
LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void GetPSMaxTX (int channel, double* maxtx)
{
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	*maxtx = txa[channel].calcc.p->ctrl.env_maxtx;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSPtol (int channel, double ptol)
{
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	txa[channel].calcc.p->ptol = ptol;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void GetPSDisp (int channel, double* x, double* ym, double* yc, double* ys, double* cm, double* cc, double* cs)
{
	CALCC a = txa[channel].calcc.p;
	EnterCriticalSection (&a->disp.cs_disp);
	memcpy (x,  a->disp.x,  a->nsamps * sizeof (double));
	memcpy (ym, a->disp.ym, a->nsamps * sizeof (double));
	memcpy (yc, a->disp.yc, a->nsamps * sizeof (double));
	memcpy (ys, a->disp.ys, a->nsamps * sizeof (double));
	memcpy (cm, a->disp.cm, a->ints * 4 * sizeof (double));
	memcpy (cc, a->disp.cc, a->ints * 4 * sizeof (double));
	memcpy (cs, a->disp.cs, a->ints * 4 * sizeof (double));
	LeaveCriticalSection (&a->disp.cs_disp);
}

PORT
void SetPSFeedbackRate (int channel, int rate)
{
	CALCC a = txa[channel].calcc.p;
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	a->rate = rate;
	a->ctrl.moxsamps = (int)(a->rate * a->ctrl.moxdelay);
	a->ctrl.waitsamps = (int)(a->rate * a->ctrl.loopdelay);
	destroy_delay (a->txdelay);
	destroy_delay (a->rxdelay);
	a->rxdelay = create_delay (
		1,											// run
		0,											// size				[stuff later]
		0,											// input buffer		[stuff later]
		0,											// output buffer	[stuff later]
		a->rate,									// sample rate
		20.0e-09,									// delta (delay stepsize)
		0.0);										// delay
	a->txdelay = create_delay (
		1,											// run
		0,											// size				[stuff later]
		0,											// input buffer		[stuff later]
		0,											// output buffer	[stuff later]
		a->rate,									// sample rate
		20.0e-09,									// delta (delay stepsize)
		a->txdel);									// delay
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSPinMode (int channel, int pin)
{
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	txa[channel].calcc.p->pin = pin;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSMapMode (int channel, int map)
{
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	txa[channel].calcc.p->map = map;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

PORT
void SetPSStabilize (int channel, int stbl)
{
	EnterCriticalSection (&txa[channel].calcc.cs_update);
	txa[channel].calcc.p->stbl = stbl;
	LeaveCriticalSection (&txa[channel].calcc.cs_update);
}

void ForceShutDown (CALCC a, IQC b, int timeout)
{
	a->runcal = 0;										// close pscc() gate
	InterlockedBitTestAndReset (&b->run, 0);			// close xiqc() gate
	Sleep (timeout);									// wait for anything possibly running to clear
	a->ctrl.state = LRESET;								// set next_state
	InterlockedBitTestAndReset (&a->ctrl.running, 0);	// set running = 0
	InterlockedBitTestAndReset (&b->busy, 0);			// set busy = 0 so turnoff thread can finish & terminate
	InterlockedBitTestAndReset (&a->ctrl.calcdone, 0);
	a->info[14] = 0;
	a->ctrl.env_maxtx = 0.0;
	a->ctrl.bs_count = 0;
}

PORT
void SetPSIntsAndSpi (int channel, int ints, int spi)
{
	CALCC a = txa[channel].calcc.p;
	IQC   b = txa[channel].iqc.p1;
	if (b->ints != ints || b->dog.spi != spi || a->ints != ints || a->spi != spi)
	{
		// SHUT-DOWN
		const int timeout = 50;
		int runcal   = a->runcal;
		int mancal   = a->ctrl.mancal;
		int automode = a->ctrl.automode;
		int turnon   = a->ctrl.turnon;
		int count = 0;
		SetPSControl (a->channel, 1, 0, 0, 0);
		while (count++ < timeout && (LRESET != _InterlockedAnd (&a->ctrl.current_state, 0xFFFFFFFF) 
			|| _InterlockedAnd (&a->ctrl.running, 1) || _InterlockedAnd (&b->run, 1))) 
			Sleep (1);						// wait for normal shutdown (when samples are flowing)
		if (LRESET != _InterlockedAnd (&a->ctrl.current_state, 0xFFFFFFFF) 
			|| _InterlockedAnd (&a->ctrl.running, 1) || _InterlockedAnd (&b->run, 1))
			ForceShutDown(a, b, timeout);	// apparently no sammples flowing; force shutdown.
		// MAKE CHANGES
		desize_iqc (b);
		desize_calcc (a);
		b->ints = ints;
		b->dog.spi = spi;
		a->ints = ints;
		a->spi = spi;
		size_calcc (a);
		size_iqc (b);
		// START-UP
		SetPSControl (a->channel, 1, mancal, automode, turnon);
		a->runcal = runcal;
	}
}