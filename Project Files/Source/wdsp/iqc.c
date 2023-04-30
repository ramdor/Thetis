/*  iqc.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2013, 2016 Warren Pratt, NR0V

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

void size_iqc (IQC a)
{
	int i;
	a->t =	(double *) malloc0 ((a->ints + 1) * sizeof(double));
	for (i = 0; i <= a->ints; i++)
		a->t[i] = (double)i / (double)a->ints;
	for (i = 0; i < 2; i++)
	{
		a->cm[i] = (double *) malloc0 (a->ints * 4 * sizeof(double));
		a->cc[i] = (double *) malloc0 (a->ints * 4 * sizeof(double));
		a->cs[i] = (double *) malloc0 (a->ints * 4 * sizeof(double));
	}
	a->dog.cpi = (int *) malloc0 (a->ints * sizeof (int));
	a->dog.count = 0;
	a->dog.full_ints = 0;
}

void desize_iqc (IQC a)
{
	int i;
	_aligned_free (a->dog.cpi);
	for (i = 0; i < 2; i++)
	{
		_aligned_free (a->cm[i]);
		_aligned_free (a->cc[i]);
		_aligned_free (a->cs[i]);
	}
	_aligned_free (a->t);
}

void calc_iqc (IQC a)
{
	int i;
	double delta, theta;
	a->cset = 0;
	a->count = 0;
	a->state = 0;
	a->busy = 0;
	a->ntup = (int)(a->tup * a->rate);
	a->cup = (double *) malloc0 ((a->ntup + 1) * sizeof (double));
	delta = PI / (double)a->ntup;
	theta = 0.0;
	for (i = 0; i <= a->ntup; i++)
	{
		a->cup[i] = 0.5 * (1.0 - cos (theta));
		theta += delta;
	}
	InitializeCriticalSectionAndSpinCount (&a->dog.cs, 2500);
	size_iqc (a);
}

void decalc_iqc (IQC a)
{
	desize_iqc (a);
	DeleteCriticalSection (&a->dog.cs);
	_aligned_free (a->cup);
}

IQC create_iqc (int run, int size, double* in, double* out, double rate, int ints, double tup, int spi)
{
	IQC a = (IQC) malloc0 (sizeof (iqc));
	a->run = run;
	a->size = size;
	a->in = in;
	a->out = out;
	a->rate = rate;
	a->ints = ints;
	a->tup = tup;
	a->dog.spi = spi;
	calc_iqc (a);
	return a;
}

void destroy_iqc (IQC a)
{
	decalc_iqc (a);
	_aligned_free (a);
}

void flush_iqc (IQC a)
{

}

enum _iqcstate
{
	RUN = 0,
	BEGIN,
	SWAP,
	END,
	DONE
};

void xiqc (IQC a)
{
	if (_InterlockedAnd(&a->run, 1))
	{
		int i, k, cset, mset;
		double I, Q, env, dx, ym, yc, ys, PRE0, PRE1;
		for (i = 0; i < a->size; i++)
		{
			I = a->in[2 * i + 0];
			Q = a->in[2 * i + 1];
			env = sqrt (I * I + Q * Q);
			if ((k = (int)(env * a->ints)) > a->ints - 1) k = a->ints - 1;
			dx = env - a->t[k];
			cset = a->cset;
			ym = a->cm[cset][4 * k + 0] + dx * (a->cm[cset][4 * k + 1] + dx * (a->cm[cset][4 * k + 2] + dx * a->cm[cset][4 * k + 3]));
			yc = a->cc[cset][4 * k + 0] + dx * (a->cc[cset][4 * k + 1] + dx * (a->cc[cset][4 * k + 2] + dx * a->cc[cset][4 * k + 3]));
			ys = a->cs[cset][4 * k + 0] + dx * (a->cs[cset][4 * k + 1] + dx * (a->cs[cset][4 * k + 2] + dx * a->cs[cset][4 * k + 3]));
			PRE0 = ym * (I * yc - Q * ys);
			PRE1 = ym * (I * ys + Q * yc);

			switch (a->state)
			{
			case RUN:
				if (a->dog.cpi[k] != a->dog.spi)
					if (++a->dog.cpi[k] == a->dog.spi)
						a->dog.full_ints++;
				if (a->dog.full_ints == a->ints)
				{
					EnterCriticalSection (&a->dog.cs);
					++a->dog.count;
					LeaveCriticalSection (&a->dog.cs);
					a->dog.full_ints = 0;
					memset (a->dog.cpi, 0, a->ints * sizeof (int));
				}
				break;
			case BEGIN:
				PRE0 = (1.0 - a->cup[a->count]) * I + a->cup[a->count] * PRE0;
				PRE1 = (1.0 - a->cup[a->count]) * Q + a->cup[a->count] * PRE1;
				if (a->count++ == a->ntup)
				{
					a->state = RUN;
					a->count = 0;
					InterlockedBitTestAndReset (&a->busy, 0);
				}
				break;
			case SWAP:
				mset = 1 - cset;
				ym = a->cm[mset][4 * k + 0] + dx * (a->cm[mset][4 * k + 1] + dx * (a->cm[mset][4 * k + 2] + dx * a->cm[mset][4 * k + 3]));
				yc = a->cc[mset][4 * k + 0] + dx * (a->cc[mset][4 * k + 1] + dx * (a->cc[mset][4 * k + 2] + dx * a->cc[mset][4 * k + 3]));
				ys = a->cs[mset][4 * k + 0] + dx * (a->cs[mset][4 * k + 1] + dx * (a->cs[mset][4 * k + 2] + dx * a->cs[mset][4 * k + 3]));
				PRE0 = (1.0 - a->cup[a->count]) * ym * (I * yc - Q * ys) + a->cup[a->count] * PRE0;
				PRE1 = (1.0 - a->cup[a->count]) * ym * (I * ys + Q * yc) + a->cup[a->count] * PRE1;
				if (a->count++ == a->ntup)
				{
					a->state = RUN;
					a->count = 0;
					InterlockedBitTestAndReset (&a->busy, 0);
				}
				break;
			case END:
				PRE0 = (1.0 - a->cup[a->count]) * PRE0 + a->cup[a->count] * I;
				PRE1 = (1.0 - a->cup[a->count]) * PRE1 + a->cup[a->count] * Q;
				if (a->count++ == a->ntup)
				{
					a->state = DONE;
					a->count = 0;
					InterlockedBitTestAndReset (&a->busy, 0);
				}
				break;
			case DONE:
				PRE0 = I;
				PRE1 = Q;
				break;
			}
			a->out[2 * i + 0] = PRE0;
			a->out[2 * i + 1] = PRE1;
			// print_iqc_values("iqc.txt", a->state, env, PRE0, PRE1, ym, yc, ys, 1.1);
		}
	}
	else if (a->out != a->in)
		memcpy (a->out, a->in, a->size * sizeof (complex));
}

void setBuffers_iqc (IQC a, double* in, double* out)
{
	a->in = in;
	a->out = out;
}

void setSamplerate_iqc (IQC a, int rate)
{
	decalc_iqc (a);
	a->rate = rate;
	calc_iqc (a);
}

void setSize_iqc (IQC a, int size)
{
	a->size = size;
}

/********************************************************************************************************
*																										*
*											TXA Properties												*
*																										*
********************************************************************************************************/

PORT
void GetTXAiqcValues (int channel, double* cm, double* cc, double* cs)
{
	IQC a;
	EnterCriticalSection (&ch[channel].csDSP);
	a = txa[channel].iqc.p0;
	memcpy (cm, a->cm[a->cset], a->ints * 4 * sizeof (double));
	memcpy (cc, a->cc[a->cset], a->ints * 4 * sizeof (double));
	memcpy (cs, a->cs[a->cset], a->ints * 4 * sizeof (double));
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetTXAiqcValues (int channel, double* cm, double* cc, double* cs)
{
	IQC a;
	EnterCriticalSection (&ch[channel].csDSP);
	a = txa[channel].iqc.p0;
	a->cset = 1 - a->cset;
	memcpy (a->cm[a->cset], cm, a->ints * 4 * sizeof (double));
	memcpy (a->cc[a->cset], cc, a->ints * 4 * sizeof (double));
	memcpy (a->cs[a->cset], cs, a->ints * 4 * sizeof (double));
	a->state = RUN;
	LeaveCriticalSection (&ch[channel].csDSP);
}

PORT
void SetTXAiqcSwap (int channel, double* cm, double* cc, double* cs)
{
	IQC a = txa[channel].iqc.p1;
	EnterCriticalSection (&ch[channel].csDSP);
	a->cset = 1 - a->cset;
	memcpy (a->cm[a->cset], cm, a->ints * 4 * sizeof (double));
	memcpy (a->cc[a->cset], cc, a->ints * 4 * sizeof (double));
	memcpy (a->cs[a->cset], cs, a->ints * 4 * sizeof (double));
	InterlockedBitTestAndSet (&a->busy, 0);
	a->state = SWAP;
	a->count = 0;
	LeaveCriticalSection (&ch[channel].csDSP);
	while (_InterlockedAnd (&a->busy, 1)) Sleep(1);
}

PORT
void SetTXAiqcStart (int channel, double* cm, double* cc, double* cs)
{
	IQC a = txa[channel].iqc.p1;
	EnterCriticalSection (&ch[channel].csDSP);
	a->cset = 0;
	memcpy (a->cm[a->cset], cm, a->ints * 4 * sizeof (double));
	memcpy (a->cc[a->cset], cc, a->ints * 4 * sizeof (double));
	memcpy (a->cs[a->cset], cs, a->ints * 4 * sizeof (double));
	InterlockedBitTestAndSet (&a->busy, 0);
	a->state = BEGIN;
	a->count = 0;
	LeaveCriticalSection (&ch[channel].csDSP);
	InterlockedBitTestAndSet   (&txa[channel].iqc.p1->run, 0);
	while (_InterlockedAnd (&a->busy, 1)) Sleep(1);
}

PORT
void SetTXAiqcEnd (int channel)
{
	IQC a = txa[channel].iqc.p1;
	EnterCriticalSection (&ch[channel].csDSP);
	InterlockedBitTestAndSet (&a->busy, 0);
	a->state = END;
	a->count = 0;
	LeaveCriticalSection (&ch[channel].csDSP);
	while (_InterlockedAnd (&a->busy, 1)) Sleep(1);
	InterlockedBitTestAndReset (&txa[channel].iqc.p1->run, 0);
}

void GetTXAiqcDogCount (int channel, int* count)
{
	IQC a = txa[channel].iqc.p1;
	EnterCriticalSection (&a->dog.cs);
	*count = a->dog.count;
	LeaveCriticalSection (&a->dog.cs);
}

void SetTXAiqcDogCount (int channel, int count)
{
	IQC a = txa[channel].iqc.p1;
	EnterCriticalSection (&a->dog.cs);
	a->dog.count = count;
	LeaveCriticalSection (&a->dog.cs);
}
