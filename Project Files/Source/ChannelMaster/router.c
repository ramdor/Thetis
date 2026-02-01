/*  router.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2015 Warren Pratt, NR0V

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

#include "cmcomm.h"

#define MAX_EXT_ROUTER	(4)									// maximum number of AAMIXs called from outside wdsp
__declspec (align (16)) ROUTER prouter[MAX_EXT_ROUTER];		// array of pointers for AAMIXs used EXTERNAL to wdsp

void* create_router(
	int id
	)
{
	ROUTER a = (ROUTER)malloc0(sizeof(router));
	a->id = id;
	if (a->id >= 0) prouter[id] = a;
	InitializeCriticalSectionAndSpinCount(&a->cs_update, 2500);
	return (void *)a;
}

void destroy_router(void* ptr, int id)
{
	ROUTER a;
	if (ptr == 0)	a = prouter[id];
	else			a = (ROUTER)ptr;
	DeleteCriticalSection(&a->cs_update);
	_aligned_free(a);
}

void flush_router(ROUTER a)
{
	int i, j, k;
	for (i = 0; i < rtMAXSOURCES; i++)
	{
		for (j = 0; j < rtMAXCALLS; j++)
		{
			for (k = 0; k < rtNVAR; k++)
			{
				a->function[i][j][k] = 0;
				a->callid[i][j][k] = 0;
			}
		}
	}
	memset(a->nstreams, 0, rtMAXSOURCES * sizeof(int));
	memset(a->ddata, 0, rtMAXSIZE * sizeof(complex));
}

PORT
void xrouter(void* ptr, int id, int source, int nsamples, double* data)
{
	int i, j, k, bport, sps, si, ctrl;
	ROUTER a;
	if (ptr == 0)	a = prouter[id];
	else			a = (ROUTER)ptr;
	double* ptrs[rtMAXSTREAMS];
	ctrl = _InterlockedAnd(&(a->controlword), 0xffffffff);
	EnterCriticalSection(&a->cs_update);
	bport = source;															// 0-based port number
	if (bport < a->sources)													// if the port is valid ...
	{
		sps = nsamples / a->nstreams[bport];								// samples per stream
		for (i = 0; i < a->ncalls; i++)
		{
			switch (a->function[bport][i][ctrl])
			{
			case 1:
				Inbound(a->callid[bport][i][ctrl], nsamples, data);
				break;
			case 2:
				for (j = 0; j < a->nstreams[bport]; j++)					// for each stream
				{
					si = j * sps;											// stream index
					ptrs[j] = &(a->ddata[2 * si]);							// save pointer to the stream
					for (k = 0; k < sps; k++)								// for each sample of the stream
					{
						a->ddata[2 * (si + k) + 0] = data[2 * (a->nstreams[bport] * k + j) + 0];
						a->ddata[2 * (si + k) + 1] = data[2 * (a->nstreams[bport] * k + j) + 1];
					}
				}
				InboundBlock(a->callid[bport][i][ctrl], sps, ptrs);
				break;
			}
		}
	}
	LeaveCriticalSection(&a->cs_update);
}

PORT
void LoadRouterAll(
	void* ptr,
	int id,
	int sources,
	int calls,
	int varvals,
	int* nstreams,
	int* function,
	int* callid
	)
{
	int i, j, k;
	ROUTER a;
	if (ptr == 0)	a = prouter[id];
	else			a = (ROUTER)ptr;
	EnterCriticalSection(&a->cs_update);
	flush_router(a);
	a->sources = sources;
	a->ncalls = calls;
	for (i = 0; i < a->sources; i++)
	{
		for (j = 0; j < a->ncalls; j++)
		{
			for (k = 0; k < varvals; k++)
			{
				a->function[i][j][k] = function[varvals * (calls * i + j) + k];
				a->callid[i][j][k]   = callid  [varvals * (calls * i + j) + k];
			}
		}
		a->nstreams[i] = nstreams[i];
	}
	LeaveCriticalSection(&a->cs_update);
}

PORT
void LoadRouterControlBit(void* ptr, int id, int var_number, int bit)
{
	ROUTER a;
	if (ptr == 0)	a = prouter[id];
	else			a = (ROUTER)ptr;
	if (bit)
		InterlockedBitTestAndSet   (&(a->controlword), var_number);
	else
		InterlockedBitTestAndReset (&(a->controlword), var_number);
}
