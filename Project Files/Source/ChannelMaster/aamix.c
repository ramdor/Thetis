/*  aamix.c

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

#include "cmcomm.h"

#define MAX_EXT_AAMIX	(4)									// maximum number of AAMIXs called from outside wdsp
__declspec (align (16)) AAMIX paamix[MAX_EXT_AAMIX];		// array of pointers for AAMIXs used EXTERNAL to wdsp

void mix_main (void *pargs)
{
	DWORD taskIndex = 0;
	HANDLE hTask = AvSetMmThreadCharacteristics(TEXT("Pro Audio"), &taskIndex);
	if (hTask != 0) AvSetMmThreadPriority(hTask, 2);
	else SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_HIGHEST);

	AAMIX a = (AAMIX) pargs;
	
	while (_InterlockedAnd (&a->run, 1))
	{
		WaitForMultipleObjects (a->nactive, a->Aready, TRUE, INFINITE);
		xaamix (a);
		(*a->Outbound)(a->outbound_id, a->outsize, a->out);
		// WriteAudio (30.0, 48000, a->outsize, a->out, 3);
	}
	_endthread();
}

void start_mixthread (AAMIX a)
{
	HANDLE handle = (HANDLE) _beginthread(mix_main, 0, (void *)a);
	//SetThreadPriority (handle, THREAD_PRIORITY_HIGHEST);
}

enum _slew
{
	BEGIN = 0,
	DELAYUP,
	UPSLEW,
	ON,
	DELAYDOWN,
	DOWNSLEW,
	ZERO,
	OFF
};

void create_aaslew (AAMIX a)
{
	int i;
	double delta, theta;
	a->slew.ustate = BEGIN;
	a->slew.dstate = BEGIN;
	a->slew.ucount = 0;
	a->slew.dcount = 0;
	a->slew.ndelup = (int)(a->slew.tdelayup * a->outrate);
	a->slew.ndeldown = (int)(a->slew.tdelaydown * a->outrate);
	a->slew.ntup = (int)(a->slew.tslewup * a->outrate);
	a->slew.ntdown = (int)(a->slew.tslewdown * a->outrate);
	a->slew.cup   = (double *) malloc0 ((a->slew.ntup + 1) * sizeof (double));
	a->slew.cdown = (double *) malloc0 ((a->slew.ntdown + 1) * sizeof (double));

	delta = PI / (double)a->slew.ntup;
	theta = 0.0;
	for (i = 0; i <= a->slew.ntup; i++)
	{
		a->slew.cup[i] = 0.5 * (1.0 - cos (theta));
		theta += delta;
	}

	delta = PI / (double)a->slew.ntdown;
	theta = 0.0;
	for (i = 0; i <= a->slew.ntdown; i++)
	{
		a->slew.cdown[i] = 0.5 * (1 + cos (theta));
		theta += delta;
	}

	a->slew.uwait = CreateSemaphore (0, 0, 1, 0);
	a->slew.dwait = CreateSemaphore (0, 0, 1, 0);
	InterlockedBitTestAndReset (&a->slew.uflag, 0);
	InterlockedBitTestAndReset (&a->slew.dflag, 0);

	a->slew.utimeout = (int)(1000.0 * (a->slew.tdelayup   + a->slew.tslewup  )) + 2;
	a->slew.dtimeout = (int)(1000.0 * (a->slew.tdelaydown + a->slew.tslewdown)) + 2;
}

void destroy_aaslew (AAMIX a)
{
	CloseHandle (a->slew.dwait);
	CloseHandle (a->slew.uwait);
	_aligned_free (a->slew.cdown);
	_aligned_free (a->slew.cup);
}

void flush_aaslew (AAMIX a)
{
	a->slew.ustate = BEGIN;
	a->slew.dstate = BEGIN;
	a->slew.ucount = 0;
	a->slew.dcount = 0;
	InterlockedBitTestAndReset (&a->slew.uflag, 0);
	InterlockedBitTestAndReset (&a->slew.dflag, 0);
}


void* create_aamix (
	int id, 
	int outbound_id,
	int ringinsize,
	int outsize, 
	int ninputs,
	long active,
	long what, 
	double volume,
	int ring_size,
	int* inrates,
	int outrate,
	void (*Outbound)(int id, int nsamples, double* buff),
	double tdelayup,
	double tslewup,
	double tdelaydown,
	double tslewdown
	)
{
	int i;
	AAMIX a = (AAMIX) malloc0 (sizeof (aamix));
	a->id = id;
	a->outbound_id = outbound_id;
	a->ringinsize = ringinsize;
	a->outsize = outsize;
	a->ninputs = ninputs;
	a->active = active;
	a->what = what;
	a->volume = volume;
	a->rsize = ring_size;
	a->outrate = outrate;
	a->Outbound = Outbound;
	a->slew.tdelayup = tdelayup;
	a->slew.tslewup = tslewup;
	a->slew.tdelaydown = tdelaydown;
	a->slew.tslewdown = tslewdown;
	for (i = 0; i < a->ninputs; i++)
		a->ring[i] = (double*) malloc0 (a->rsize * sizeof (complex));
	a->out = (double*) malloc0 (a->outsize * sizeof (complex));
	a->nactive = 0;
	for (i = 0; i < a->ninputs; i++)
	{
		a->vol[i]			= 1.0;
		a->tvol[i]			= a->volume;
		a->inidx[i]			= 0;
		a->outidx[i]		= 0;
		a->unqueuedsamps[i] = 0;
		a->Ready[i] = CreateSemaphore (0, 0, 1000, 0);
		InitializeCriticalSectionAndSpinCount(&a->cs_in[i], 2500);
		if (_InterlockedAnd(&a->active, 0xffffffff) & (1 << i))
		{
			a->Aready[a->nactive++] = a->Ready[i];
			InterlockedBitTestAndSet (&a->accept[i], 0);
		}
		else
			InterlockedBitTestAndReset (&a->accept[i], 0);
	}
	for (i = 0; i < a->ninputs; i++)	// resamplers
	{
		int run, size;
		a->inrate[i] = inrates[i];
		// inrate & outrate must be related by an integer multiple or sub-multiple
		if (a->inrate[i] != a->outrate) run = 1;
		else							run = 0;
		if (a->inrate[i] > a->outrate)
			size = a->ringinsize * (a->inrate[i] / a->outrate);
		else
			size = a->ringinsize / (a->outrate / a->inrate[i]);
		a->resampbuff[i] = (double *) malloc0 (a->ringinsize * sizeof (complex));
		a->rsmp[i] = create_resample (run, size, 0, a->resampbuff[i], a->inrate[i], a->outrate, 0.0, 0, 1.0);
	}
	InitializeCriticalSectionAndSpinCount(&a->cs_out, 2500);
	// slew
	create_aaslew (a);
	// slew_end
	InterlockedBitTestAndSet (&a->run, 0);
	if (a->nactive) start_mixthread (a);
	if (a->id >= 0) paamix[id] = a;
	return (void *)a;
}

void destroy_aamix (void* ptr, int id)
{
	int i;
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	InterlockedBitTestAndReset (&a->run, 0);
	for (i = 0; i < a->ninputs; i++)
		ReleaseSemaphore (a->Ready[i], 1, 0);
	Sleep (2);
	DeleteCriticalSection (&a->cs_out);
	for (i = 0; i < a->ninputs; i++)
	{
		destroy_resample (a->rsmp[i]);
		_aligned_free (a->resampbuff[i]);
		DeleteCriticalSection (&a->cs_in[i]);
		CloseHandle (a->Ready[i]);
	}
	_aligned_free (a->out);
	for (i = 0; i < a->ninputs; i++)
		_aligned_free (a->ring[i]);
	// slew
	destroy_aaslew (a);
	// slew_end
	_aligned_free (a);
}

// loads data from a buffer into an audio mixer ring
void xMixAudio (void* ptr, int id, int stream, double* data)
{
	int first, second, n;
	double* indata;
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	if (_InterlockedAnd (&a->accept[stream], 1))
	{
		EnterCriticalSection (&a->cs_in[stream]);
		if (a->rsmp[stream]->run)
		{
			a->rsmp[stream]->in = data;
			indata = a->resampbuff[stream];
			xresample (a->rsmp[stream]);
		}
		else
			indata = data;
		if (a->ringinsize > (a->rsize - a->inidx[stream]))
		{
			first = a->rsize - a->inidx[stream];
			second = a->ringinsize - first;
		}
		else
		{
			first = a->ringinsize;
			second = 0;
		}
		memcpy (a->ring[stream] + 2 * a->inidx[stream], indata,             first  * sizeof (complex));
		memcpy (a->ring[stream],						indata + 2 * first, second * sizeof (complex));

		if ((a->unqueuedsamps[stream] += a->ringinsize) >= a->outsize)
		{
			n = a->unqueuedsamps[stream] / a->outsize;
			ReleaseSemaphore (a->Ready[stream], n, 0);
			a->unqueuedsamps[stream] -= n * a->outsize;
		}
		if ((a->inidx[stream] += a->ringinsize) >= a->rsize)
			a->inidx[stream] -= a->rsize;
		LeaveCriticalSection (&a->cs_in[stream]);
	}
}

void upslew (AAMIX a)
{
	int i;
	double I, Q;
	double *pin  = a->out;
	double *pout = a->out;
	for (i = 0; i < a->outsize; i++)
	{
		I = pin[2 * i + 0];
		Q = pin[2 * i + 1];
		switch (a->slew.ustate)
			{
			case BEGIN:
				pout[2 * i + 0] = 0.0;
				pout[2 * i + 1] = 0.0;
				if ((I != 0.0) || (Q != 0.0))
				{
					if (a->slew.ndelup > 0)
					{
						a->slew.ustate = DELAYUP;
						a->slew.ucount = a->slew.ndelup;
					}
					else if (a->slew.ntup > 0)
					{
						a->slew.ustate = UPSLEW;
						a->slew.ucount = a->slew.ntup;
					}
					else
						a->slew.ustate = ON;
				}
				break;
			case DELAYUP:
				pout[2 * i + 0] = 0.0;
				pout[2 * i + 1] = 0.0;
				if (a->slew.ucount-- == 0)
				{
					if (a->slew.ntup > 0)
					{
						a->slew.ustate = UPSLEW;
						a->slew.ucount = a->slew.ntup;
					}
					else
						a->slew.ustate = ON;
				}
				break;
			case UPSLEW:
				pout[2 * i + 0] = I * a->slew.cup[a->slew.ntup - a->slew.ucount];
				pout[2 * i + 1] = Q * a->slew.cup[a->slew.ntup - a->slew.ucount];
				if (a->slew.ucount-- == 0)
					a->slew.ustate = ON;
				break;
			case ON:
				pout[2 * i + 0] = I;
				pout[2 * i + 1] = Q;
				if (i == a->outsize - 1)
				{
					a->slew.ustate = BEGIN;
					InterlockedBitTestAndReset (&a->slew.uflag, 0);
					ReleaseSemaphore (a->slew.uwait, 1, 0);
				}
				break;
			}
	}
}

void downslew (AAMIX a)
{
	int i;
	double I, Q;
	double *pin  = a->out;
	double *pout = a->out;
	for (i = 0; i < a->outsize; i++)
	{
		I = pin[2 * i + 0];
		Q = pin[2 * i + 1];
		switch (a->slew.dstate)
			{
			case BEGIN:
				pout[2 * i + 0] = I;
				pout[2 * i + 1] = Q;
				if (a->slew.ndeldown > 0)
				{
					a->slew.dstate = DELAYDOWN;
					a->slew.dcount = a->slew.ndeldown;
				}
				else if (a->slew.ntdown > 0)
				{
					a->slew.dstate = DOWNSLEW;
					a->slew.dcount = a->slew.ntdown;
				}
				else
				{
					a->slew.dstate = ZERO;
					a->slew.dcount = a->outsize;
				}
				break;
			case DELAYDOWN:
				pout[2 * i + 0] = I;
				pout[2 * i + 1] = Q;
				if (a->slew.dcount-- == 0)
				{
					if (a->slew.ntdown > 0)
					{
						a->slew.dstate = DOWNSLEW;
						a->slew.dcount = a->slew.ntdown;
					}
					else
					{
						a->slew.dstate = ZERO;
						a->slew.dcount = a->outsize;
					}
				}
				break;
			case DOWNSLEW:
				pout[2 * i + 0] = I * a->slew.cdown[a->slew.ntdown - a->slew.dcount];
				pout[2 * i + 1] = Q * a->slew.cdown[a->slew.ntdown - a->slew.dcount];
				if (a->slew.dcount-- == 0)
				{
					a->slew.dstate = ZERO;
					a->slew.dcount = a->outsize;
				}
				break;
			case ZERO:
				pout[2 * i + 0] = 0.0;
				pout[2 * i + 1] = 0.0;
				if (a->slew.dcount-- == 0)
					a->slew.dstate = OFF;
				break;
			case OFF:
				pout[2 * i + 0] = 0.0;
				pout[2 * i + 1] = 0.0;
				if (i == a->outsize - 1)
				{
					a->slew.dstate = BEGIN;
					InterlockedBitTestAndReset (&a->slew.dflag, 0);
					ReleaseSemaphore (a->slew.dwait, 1, 0);
				}
				break;
			}
	}
}

// pulls data from audio rings and mixes with output
void xaamix (AAMIX a)
{
	int i, j;
	int what, mask, idx;
	EnterCriticalSection(&a->cs_out);
	if (!_InterlockedAnd (&a->run, 1)) 
	{
		LeaveCriticalSection (&a->cs_out);
		_endthread();
		return; //MW0LGE_21j
	}
	memset (a->out, 0, a->outsize * sizeof (complex));
	what = _InterlockedAnd(&a->what, 0xffffffff) & _InterlockedAnd(&a->active, 0xffffffff);
	i = 0;
	while (what != 0)
	{
		mask = 1 << i;
		if ((mask & what) != 0)
		{
			idx = a->outidx[i];
			for (j = 0; j < a->outsize; j++)
			{
				a->out[2 * j + 0] += a->tvol[i] * a->ring[i][2 * idx + 0];
				a->out[2 * j + 1] += a->tvol[i] * a->ring[i][2 * idx + 1];
				if (++idx == a->rsize) idx = 0;
			}
			what &= ~mask;
		}
		i++;
	}
	for (i = 0; i < a->ninputs; i++)
		if (_InterlockedAnd (&a->accept[i], 1))
			if ((a->outidx[i] += a->outsize) >= a->rsize) a->outidx[i] -= a->rsize;
	if (_InterlockedAnd (&a->slew.uflag, 1)) upslew   (a);
	if (_InterlockedAnd (&a->slew.dflag, 1)) downslew (a);
	LeaveCriticalSection(&a->cs_out);
}

void flush_mix_ring (AAMIX a, int stream)
{
	memset (a->ring[stream], 0, a->rsize * sizeof (complex));
	a->inidx [stream] = 0;
	a->outidx[stream] = 0;
	a->unqueuedsamps[stream] = 0;
	while (!WaitForSingleObject (a->Ready[stream], 1)) ;
	flush_resample (a->rsmp[stream]);
}

void close_mixer (AAMIX a)
{
	int i;
	InterlockedBitTestAndSet (&a->slew.dflag, 0);		// set a bit telling downslew to proceed
	WaitForSingleObject (a->slew.dwait, a->slew.dtimeout);// block until downslew is complete or timeout if data is not flowing
	InterlockedBitTestAndReset (&a->slew.dflag, 0);
	for (i = 0; i < a->ninputs; i++)
		InterlockedBitTestAndReset(&a->accept[i], 0);	// shut the gates to prevent new infusions
	Sleep(1);											// if a thread has just passed the gate, allow time to get cs_in and get through
	for (i = 0; i < a->ninputs; i++)
		EnterCriticalSection (&a->cs_in[i]);			// wait until the current infusions are all finished
	EnterCriticalSection (&a->cs_out);					// block the mixer thread at the beginning of xaamix()
	Sleep (25);											// wait for thread to arrive at the top of the main() loop
	InterlockedBitTestAndReset(&a->run, 0);				// set a trap for the mixer thread
	for (i = 0; i < a->ninputs; i++)
		ReleaseSemaphore(a->Ready[i], 1, 0);			// be sure the mixer thread can pass WaitForMultipleObjects in main()
	LeaveCriticalSection (&a->cs_out);					// let the thread pass to the trap in xaamix()
	Sleep (2);											// wait for the mixer thread to die
	for (i = 0; i < a->ninputs; i++)
		flush_mix_ring (a, i);							// restore rings to pristine condition
}

void open_mixer (AAMIX a)
{
	int i;
	InterlockedBitTestAndSet (&a->slew.uflag, 0);		// set a bit telling upslew to proceed (when there are samples flowing)
	InterlockedBitTestAndSet(&a->run,0);				// remove the mixer thread trap
	if (a->nactive) start_mixthread (a);				// start the mixer thread if there's anything to mix
	for (i = a->ninputs - 1; i >= 0; i--)
		LeaveCriticalSection (&a->cs_in[i]);			// enable xMixAudio() processing
	for (i = a->ninputs - 1; i >= 0; i--)
		if (_InterlockedAnd (&a->active, 0xffffffff) & (1 << i))
			InterlockedBitTestAndSet(&a->accept[i], 0);	// open the xMixAudio() gates for active streams
	WaitForSingleObject (a->slew.uwait, a->slew.utimeout);// block on semaphore until upslew complete, or until timeout if no data flow
}

/********************************************************************************************************
*																										*
*									         MIXER PROPERTIES											*
*																										*
********************************************************************************************************/

void SetAAudioMixOutputPointer(void* ptr, int id, void (*Outbound)(int id, int nsamples, double* buff))
{
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	a->Outbound = Outbound;
}

PORT
void SetAAudioMixState (void* ptr, int id, int stream, int state)
{
	int i;
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	if (((_InterlockedAnd(&a->active, 0xffffffff) >> stream) & 1) != state)
	{
		close_mixer(a);
		if (state)
			_InterlockedOr(&a->active, 1 << stream);				// set stream active
		else
			_InterlockedAnd(&a->active, ~(1 << stream));			// set stream inactive
		a->nactive = 0;
		for (i = 0; i < a->ninputs; i++)
			if (_InterlockedAnd(&a->active, 0xffffffff) & (1 << i))
			{
				a->Aready[a->nactive++] = a->Ready[i];
				InterlockedBitTestAndSet(&a->accept[i], 0);
			}
			else
				InterlockedBitTestAndReset(&a->accept[i], 0);
		open_mixer(a);
	}
}

// SetAAudioMixStates() is an alternative to SetAAudioMixState() that can be used to set multiple
// mix states with only a single call.  'streams' has one bit per mix state that you want to set
// and 'states' has one bit specifying the state of each stream that you want to set.  For example,
// if you want to set the state of streams 0 and 3 to 1 and 0, respectively:
// streams = 9 [...1001]
// states =  1 [...0001]
PORT
void SetAAudioMixStates (void* ptr, int id, int streams, int states)
{
	int i;
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	if ((_InterlockedAnd (&a->active, 0xffffffff) & streams) != (states & streams))
	{
		close_mixer (a);
		for (i = 0; i < a->ninputs; i++)
			if ((streams >> i) & 1)
				if ((states >> i) & 1)
					_InterlockedOr(&a->active, 1 << i);				// set stream active
				else
					_InterlockedAnd(&a->active, ~(1 << i));			// set stream inactive

		a->nactive = 0;
		for (i = 0; i < a->ninputs; i++)
			if (_InterlockedAnd(&a->active, 0xffffffff) & (1 << i))
			{
				a->Aready[a->nactive++] = a->Ready[i];
				InterlockedBitTestAndSet(&a->accept[i], 0);
			}
			else
				InterlockedBitTestAndReset(&a->accept[i], 0);
		open_mixer(a);
	}
}

PORT
void SetAAudioMixWhat (void* ptr, int id, int stream, int state)
{
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	if (state)
		InterlockedBitTestAndSet   (&a->what, stream);		// turn on mixing
	else
		InterlockedBitTestAndReset (&a->what, stream);		// turn off mixing
}

PORT
void SetAAudioMixVolume (void* ptr, int id, double volume)
{
	int i;
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	EnterCriticalSection(&a->cs_out);
	a->volume = volume;
	for (i = 0; i < 32; i++)
		a->tvol[i] = a->volume * a->vol[i];
	LeaveCriticalSection(&a->cs_out);
}

PORT
void SetAAudioMixVol (void* ptr, int id, int stream, double vol)
{
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	EnterCriticalSection(&a->cs_out);
	a->vol [stream] = vol;
	a->tvol[stream] = a->vol[stream] * a->volume;
	LeaveCriticalSection(&a->cs_out);
}

void SetAAudioRingInsize (void* ptr, int id, int size)
{
	int i, rs_size;
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	close_mixer (a);
	a->ringinsize = size;
	for (i = 0; i < a->ninputs; i++)
	{	// inrate & outrate must be related by an integer multiple or sub-multiple
		if (a->inrate[i] > a->outrate)
			rs_size = a->ringinsize * (a->inrate[i] / a->outrate);
		else
			rs_size = a->ringinsize / (a->outrate / a->inrate[i]);
		a->rsmp[i]->size = rs_size;
		_aligned_free (a->resampbuff[i]);
		a->resampbuff[i] = (double *) malloc0 (a->ringinsize * sizeof (complex));
		a->rsmp[i]->out = a->resampbuff[i];
	}
	open_mixer (a);
}

void SetAAudioRingOutsize (void* ptr, int id, int size)
{
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	close_mixer (a);
	a->outsize = size;
	_aligned_free (a->out);
	a->out = (double*) malloc0 (a->outsize * sizeof (complex));
	open_mixer (a);
}

void SetAAudioOutRate (void* ptr, int id, int rate)
{
	int i;
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	close_mixer (a);
	a->outrate = rate;
	for (i = 0; i < a->ninputs; i++)	// resamplers
	{
		int run, size;
		destroy_resample (a-> rsmp[i]);
		_aligned_free (a->resampbuff[i]);
		// inrate & outrate must be related by an integer multiple or sub-multiple
		if (a->inrate[i] != a->outrate) run = 1;
		else							run = 0;
		if (a->inrate[i] > a->outrate)
			size = a->ringinsize * (a->inrate[i] / a->outrate);
		else
			size = a->ringinsize / (a->outrate / a->inrate[i]);
		a->resampbuff[i] = (double *) malloc0 (a->ringinsize * sizeof (complex));
		a->rsmp[i] = create_resample (run, size, 0, a->resampbuff[i], a->inrate[i], a->outrate, 0.0, 0, 1.0);
		a->rsmp[i]->out = a->resampbuff[i];
	}
	open_mixer (a);
}

void SetAAudioStreamRate (void* ptr, int id, int mixinid, int rate)
{	// NOTE: you must set the stream state to INACTIVE before using this function!
	int run, size;
	AAMIX a;
	if (ptr == 0)	a = paamix[id];
	else			a = (AAMIX)ptr;
	a->inrate[mixinid] = rate;
	destroy_resample (a->rsmp[mixinid]);
	// inrate & outrate must be related by an integer multiple or sub-multiple
	if (a->inrate[mixinid] != a->outrate)	run = 1;
	else									run = 0;
	if (a->inrate[mixinid] > a->outrate)
		size = a->ringinsize * (a->inrate[mixinid] / a->outrate);
	else
		size = a->ringinsize / (a->outrate / a->inrate[mixinid]);
	a->rsmp[mixinid] = create_resample (run, size, 0, a->resampbuff[mixinid], a->inrate[mixinid], a->outrate, 0.0, 0, 1.0);
}
