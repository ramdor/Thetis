#pragma once

/*  ivacextras.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2015-2017 Steve, G7KLJ

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
*/

#include <string.h> // size_t
#include <stdint.h>
#include <assert.h>

// static functions that allow the app to use Float32 inside of PortAudio.
// Blatantly stolen from pa_converters.c, with modifications and paranoic bounds
// checking. KLJ

static inline void Float64_To_Float32(void* destinationBuffer,
    size_t destinationSizeInBytes, unsigned int destinationStride,
    void* sourceBuffer, unsigned long sourceSizeInBytes,
    signed int sourceStride, unsigned int count) {

    uint8_t* src_b = (uint8_t*)sourceBuffer;
    uint8_t* src_e = src_b + sourceSizeInBytes;
    double* src_end = (double*)src_e;
    double* src = (double*)sourceBuffer;
    assert(src < src_end);

    uint8_t* dest_b = (uint8_t*)destinationBuffer;
    uint8_t* dest_e = dest_b + destinationSizeInBytes;
    float* dest_end = (float*)dest_e;
    float* dest = (float*)destinationBuffer;
    assert(dest < dest_end);

    unsigned int mycount = count;

    while (mycount--) {
        *dest = (float)*src;
        src += sourceStride;
        assert(src <= src_end);
        dest += destinationStride;
        assert(dest <= dest_end);
    }

    intptr_t remain_src = src_end - src;
    intptr_t remain_dest = dest_end - dest;
    assert(remain_src >= 0 && remain_dest >= 0);
    assert(remain_dest == 0); // I assume you wanted to fill the output buffer
    // I can't do this for the input buffer as you may well have delib
    // oversized it.
}

static inline void Float32_To_Float64(void* destinationBuffer,
    size_t destinationSizeInBytes, unsigned int destinationStride,
    void* sourceBuffer, unsigned long sourceSizeInBytes,
    signed int sourceStride, unsigned int count) {

    uint8_t* src_b = (uint8_t*)sourceBuffer;
    uint8_t* src_e = src_b + sourceSizeInBytes;
    float* src_end = (float*)src_e;
    float* src = (float*)sourceBuffer;
    assert(src < src_end);

    uint8_t* dest_b = (uint8_t*)destinationBuffer;
    uint8_t* dest_e = dest_b + destinationSizeInBytes;
    double* dest_end = (double*)dest_e;
    double* dest = (double*)destinationBuffer;
    assert(dest < dest_end);

    unsigned int mycount = count;
    while (mycount--) {
        *dest = *src;
        src += sourceStride;
        assert(src <= src_end);
        dest += destinationStride;
        assert(dest <= dest_end);
    }
    intptr_t remain_src = src_end - src;
    intptr_t remain_dest = dest_end - dest;
    assert(remain_src >= 0 && remain_dest >= 0);
    assert(remain_src
        == 0); // I assume you wanted to convert everything you sent in.
    // I can't do this for the output buffer as you may well have delib
    // oversized it.
}