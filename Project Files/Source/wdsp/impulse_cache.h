/*  impulse_cache.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2013, 2019, 2024 Warren Pratt, NR0V
Copyright (C) 2025 Richard Samphire, MW0LGE

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
mw0lge@grange-lane.co.uk

*/

#ifndef _impulse_cache_h
#define _impulse_cache_h

#if defined(_WIN64)
	// 64-bit build
	extern uint64_t fnv1a_hash64(const void* data, size_t len);		// 64 bit fnv1a hashing algo
	#define GOLDEN_RATIO_64 0x9E3779B97F4A7C15ULL					// 64-bit golden ratio

	#define HASH_T       uint64_t
	#define fnv1a_hash   fnv1a_hash64
	#define GOLDEN_RATIO GOLDEN_RATIO_64
#else
	// 32-bit build
	extern uint32_t fnv1a_hash32(const void* data, size_t len);		// 32 bit fnv1a hashing algo
	#define GOLDEN_RATIO_32 0x9e3779b9U								// 32-bit golden ratio

	#define HASH_T       uint32_t
	#define fnv1a_hash   fnv1a_hash32
	#define GOLDEN_RATIO GOLDEN_RATIO_32
#endif

#define MAX_CACHE_ENTRIES		1024	// max number of cache entires per bucket
#define CACHE_BUCKETS			4		// 4 cache buckets, one for fir_bandpass, mp, eq and fc. Unique indexes in the #defines below

#define FIR_CACHE	0
#define MP_CACHE	1
#define EQ_CACHE	2
#define FC_CACHE	3

typedef struct _cache_entry {
	HASH_T  hash;
	int		N;							// N complex entries in impulse. Leave as signed in as that is used everywhere
	double* impulse;
	struct _cache_entry* next;
} _cache_entry_t;

double* get_impulse_cache_entry(size_t bucket, HASH_T hash);
void add_impulse_to_cache(size_t bucket, HASH_T hash, int N, double* impulse);

__declspec (dllexport) void free_impulse_cache(void);
__declspec (dllexport) int save_impulse_cache(const char* path);
__declspec (dllexport) int read_impulse_cache(const char* path);

#endif