/*  pipe.h

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

#ifndef _pipe_h
#define _pipe_h
#include "cmsetup.h"
#include "comm.h"

#define cmMAXspc0		(4)

typedef struct _pipe
{
	double** rbuff;																// receiver audio buffers
	void (__stdcall *create_Scope)(int id);
	void (__stdcall *create_WaveRecord)(int id);
	void (__stdcall *create_WavePlay)(int id);
	struct // receiver items
	{
		volatile long top_pan3_run;
		void (__stdcall *xscope)(int state, double* data);						// scope display
		void (__stdcall *xplaywave)(int state, double* data);					// WAV player
		void (__stdcall *xrecordwave)(int state, int pos, double* data);		// WAV recorder
		int scope_run;
		int playwave_run;
		int recordwave_run;
	} rcvr[cmMAXrcvr];
	struct // transmitter items
	{
		volatile long txvac;
	} xmtr[cmMAXxmtr];
	struct _spc0 // spc0 items
	{
		ANB panb;
		NOB pnob;
	} spc0[cmMAXspc0];
} pipe, *PIPE;

extern PIPE ppip;

void create_pipe();

void destroy_pipe();

void xpipe (int stream, int pos, double** buff);

#endif