/*  calcc.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2013, 2016, 2023 Warren Pratt, NR0V

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

#ifndef _calcc_h
#define _calcc_h
#include "delay.h"
#include "lmath.h"
typedef struct _calcc
{
	int channel;
	int runcal;
	int size;
	volatile long mox;
	volatile long solidmox;
	int rate;
	int ints;
	int spi;
	int nsamps;
	int npsamps;
	int pin;
	int map;
	int convex;
	int stbl;
	int scOK;
	double hw_scale;
	double rx_scale;
	double alpha;

	int tsamps;
	double* env_TX;
	double* env_RX;
	double* x;
	double* ym;
	double* yc;
	double* ys;
	double* cat;

	double* t;
	double* tmap;
	double* cm;
	double* cc;
	double* cs;
	double* cm_old;
	double* rxs;
	double* txs;
	double ptol;
	int* info;
	int* binfo;
	double txdel;
	BLDR ccbld;
	volatile long savecorr_bypass;
	HANDLE Sem_SaveCorr;
	volatile long restcorr_bypass;
	HANDLE Sem_RestCorr;
	volatile long calccorr_bypass;
	HANDLE Sem_CalcCorr;
	volatile long turnoff_bypass;
	HANDLE Sem_TurnOff;
	struct _ctrl
	{
		double moxdelay;
		double loopdelay;
		int state;
		int reset;
		int automode;
		int mancal;
		int turnon;
		int moxsamps;
		int moxcount;
		int count;
		int* cpi;
		int* sindex;
		int* sbase;
		int full_ints;
		int calcinprogress;
		volatile LONG calcdone;
		int waitsamps;
		int waitcount;
		double env_maxtx;
		volatile long running;
		int bs_count;
		volatile long current_state;
		CRITICAL_SECTION cs_SafeToEnd;
	} ctrl;
	struct _disp
	{
		double* x;
		double* ym;
		double* yc;
		double* ys;
		double* cm;
		double* cc;
		double* cs;
		CRITICAL_SECTION cs_disp;
	} disp;
	DELAY rxdelay;
	DELAY txdelay;
	struct _util
	{
		char savefile[256];
		char restfile[256];
		int ints;
		int channel;
		double* pm;
		double* pc;
		double* ps;
	} util;
	double* temptx;				//////////////////////////////////////////////////// temporary tx complex buffer - remove with new callback3port()
	double* temprx;				//////////////////////////////////////////////////// temporary rx complex buffer - remove with new callback3port()
} calcc, *CALCC;

extern CALCC create_calcc (int channel, int runcal, int size, int rate, int ints, int spi, double hw_scale, 
	double moxdelay, double loopdelay, double ptol, int mox, int solidmox, int pin, int map, int stbl,
	int npsamps, double alpha);

extern void destroy_calcc (CALCC a);

extern void flush_calcc (CALCC a);

extern __declspec(dllexport) void pscc (int channel, int size, double* tx, double* rx);

extern void __cdecl PSSaveCorrection(void* pargs);

extern void __cdecl PSRestoreCorrection(void* pargs);

extern void __cdecl doPSCalcCorrection(void* arg);

extern void __cdecl doPSTurnoff(void* arg);

#endif

// 'info' assignments:
//		 0 - builder for rx_scale
//		 1 - builder for cm
//		 2 - builder for cc
//		 3 - builder for cs
//		 4 - feedback level warning
//		 5 - count of attempted calibrations
//		 6 - results from scheck()
//       7 - results from rxscheck()
//
//		13 - dogcount
//		14 - indicates iqc_Run = 1
//		15 - control state