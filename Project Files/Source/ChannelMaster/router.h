/*  router.h

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

#ifndef _router_h
#define _router_h

#define rtNVAR			(8)							// number of variable values = 2^num_variables
#define rtMAXCALLS		(2)							// maximum number of calls for each data buffer
#define rtMAXSOURCES	(12)						// maximum index value for sources of data
#define rtMAXSTREAMS	(2)							// maximum number of streams to be interleaved
#define rtMAXSIZE		(720)						// maximum number of complex samples in a buffer of data

typedef struct _router
{
	int id;
	volatile long controlword;						// control word used to select routing choice, contains variable values
	int sources;									// number of sources from which input can be received
	int ncalls;										// number of calls to make when data is received
	int function[rtMAXSOURCES][rtMAXCALLS][rtNVAR];	// identifier for function to be called
	int callid[rtMAXSOURCES][rtMAXCALLS][rtNVAR];	// 'id' to be used in the call
	int nstreams[rtMAXSOURCES];						// number of data streams interleaved for each port
	double ddata[2 * rtMAXSIZE];					// data buffer for de-interleaved output
	CRITICAL_SECTION cs_update;
} router, *ROUTER;

void* create_router(
	int id
	);

void destroy_router(void* ptr, int id);

PORT
void xrouter(void* ptr, int id, int source, int nsamples, double* data);

#endif
