/*  obbuffs.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2014 Warren Pratt, NR0V

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

#include "obbuffs.h"

struct _obpointers
{
	OBB pcbuff[numRings];
	OBB pdbuff[numRings];
	OBB pebuff[numRings];
	OBB pfbuff[numRings];
} obp;

void start_obthread (int id)
{
	HANDLE handle = (HANDLE) _beginthread(ob_main, 0, (void *)id);
	//SetThreadPriority(handle, THREAD_PRIORITY_HIGHEST);
}

void create_obbuffs (int id, int accept, int max_insize, int outsize)
{
	OBB a = (OBB) calloc (1, sizeof(obb));
	obp.pcbuff[id] = obp.pdbuff[id] = obp.pebuff[id] = obp.pfbuff[id] = a;
	a->id = id;
	a->accept = accept;
	a->run = 1;
	a->max_in_size = max_insize;
	a->r1_outsize = outsize;
	if (a->r1_outsize > a->max_in_size)
		a->r1_size = a->r1_outsize;
	else
		a->r1_size = a->max_in_size;
	a->r1_active_buffsize = OBB_MULT * a->r1_size;
	a->r1_baseptr = (double*) calloc (a->r1_active_buffsize, sizeof (complex));
	a->r1_inidx = 0;
	a->r1_outidx = 0;
	a->r1_unqueuedsamps = 0;
	a->Sem_BuffReady = CreateSemaphore(0, 0, 1000, 0);
	InitializeCriticalSectionAndSpinCount ( &a->csIN, 2500 );
	InitializeCriticalSectionAndSpinCount ( &a->csOUT,  2500 );
	a->out = (double *) calloc (obMAXSIZE, sizeof (complex));
	start_obthread (id);
}

void destroy_obbuffs (int id)
{
	OBB a = obp.pcbuff[id];
	if (obp.pcbuff[0] == NULL) return;
	InterlockedBitTestAndReset(&a->accept, 0);
	EnterCriticalSection (&a->csIN);
	EnterCriticalSection (&a->csOUT);
	Sleep (25);
	InterlockedBitTestAndReset(&a->run, 0);
	ReleaseSemaphore(a->Sem_BuffReady, 1, 0);
	LeaveCriticalSection (&a->csOUT);
	Sleep (2);
	DeleteCriticalSection (&a->csOUT);
	DeleteCriticalSection (&a->csIN);
	CloseHandle (a->Sem_BuffReady);
	free (a->out);
	free (a->r1_baseptr);
	free (a);
}

void flush_obbuffs (int id)
{
	OBB a = obp.pfbuff[id];
	memset (a->r1_baseptr, 0, a->r1_active_buffsize * sizeof (complex));
	a->r1_inidx = 0;
	a->r1_outidx = 0;
	a->r1_unqueuedsamps = 0;
	while (!WaitForSingleObject (a->Sem_BuffReady, 1)) ;
}

PORT
void OutBound (int id, int nsamples, double* in)
{
	int n;
	int first, second;
	OBB a = obp.pebuff[id];
	if (_InterlockedAnd (&a->accept, 1))
	{
		EnterCriticalSection (&a->csIN);
		if (nsamples > (a->r1_active_buffsize - a->r1_inidx))
		{
			first = a->r1_active_buffsize - a->r1_inidx;
			second = nsamples - first;
		}
		else
		{
			first = nsamples;
			second = 0;
		}
		memcpy (a->r1_baseptr + 2 * a->r1_inidx, in,             first  * sizeof (complex));
		memcpy (a->r1_baseptr,                   in + 2 * first, second * sizeof (complex));

		if ((a->r1_unqueuedsamps += nsamples) >= a->r1_outsize)
		{
			n = a->r1_unqueuedsamps / a->r1_outsize;
			ReleaseSemaphore (a->Sem_BuffReady, n, 0);
			a->r1_unqueuedsamps -= n * a->r1_outsize;
		}
		if ((a->r1_inidx += nsamples) >= a->r1_active_buffsize)
			a->r1_inidx -= a->r1_active_buffsize;
		LeaveCriticalSection (&a->csIN);
	}
}

void obdata (int id, double* out)
{
	int first, second;
	OBB a = obp.pdbuff[id];
	if (!_InterlockedAnd(&a->run, 1)) _endthread();
	if (a->r1_outsize > (a->r1_active_buffsize - a->r1_outidx))
	{
		first = a->r1_active_buffsize - a->r1_outidx;
		second = a->r1_outsize - first;
	}
	else
	{
		first = a->r1_outsize;
		second = 0;
	}
	memcpy (out,             a->r1_baseptr + 2 * a->r1_outidx, first  * sizeof (complex));
	memcpy (out + 2 * first, a->r1_baseptr,                    second * sizeof (complex));
	if ((a->r1_outidx += a->r1_outsize) >= a->r1_active_buffsize)
		a->r1_outidx -= a->r1_active_buffsize;
}

void ob_main (void *pargs)
{
	const wchar_t* threadName = L"ob_main Thread";
	SetThreadDescription(GetCurrentThread(), threadName);

	DWORD taskIndex = 0;
	HANDLE hTask = AvSetMmThreadCharacteristics(TEXT("Pro Audio"), &taskIndex);
	if (hTask != 0) AvSetMmThreadPriority(hTask, 2);
	else SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_HIGHEST);

	int id = (int)pargs;
	OBB a = obp.pdbuff[id];
	
	while (_InterlockedAnd (&a->run, 1))
	{
		WaitForSingleObject(a->Sem_BuffReady,INFINITE);
		EnterCriticalSection (&a->csOUT);
		LeaveCriticalSection (&a->csOUT);
		obdata (id, a->out);
		sendOutbound(id, a->out);
		// if (id == 0) WriteAudio(15.0, 48000, 126, a->out, 3);
	}
	_endthread();
}

void SetOBRingOutsize (int id, int size)
{
	OBB a = obp.pcbuff[id];
	InterlockedBitTestAndReset(&a->accept, 0);
	EnterCriticalSection (&a->csIN);
	EnterCriticalSection (&a->csOUT);
	Sleep (25);
	InterlockedBitTestAndReset(&a->run, 0);
	ReleaseSemaphore(a->Sem_BuffReady, 1, 0);
	LeaveCriticalSection (&a->csOUT);
	Sleep (2);
	flush_obbuffs(id);
	a->r1_outsize = size;
	InterlockedBitTestAndSet(&a->run,0);
	start_obthread(id);
	LeaveCriticalSection (&a->csIN);
	InterlockedBitTestAndSet(&a->accept, 0);
}
