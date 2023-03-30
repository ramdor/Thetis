/*  ring.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2015-2016 Warren Pratt, NR0V
Copyright (C) 2015-2016 Doug Wigley, W5WC

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

/************************************************************************************************
 *																								*
 *							  VERSION FOR COMPLEX DOUBLE-PRECISION
 **
 *																								*
 ************************************************************************************************/

ringbuffer_t* ringbuffer_create(int sz) {
    int power_of_two;
    ringbuffer_t* rb;

    if ((rb = (ringbuffer_t*)malloc(sizeof(ringbuffer_t))) == NULL) {
        return NULL;
    }

    for (power_of_two = 1; (unsigned int)(1 << power_of_two) < (unsigned int)sz;
         power_of_two++)
        ;

    rb->size = 1 << power_of_two;
    rb->size_mask = rb->size;
    rb->size_mask -= 1;
    rb->write_ptr = 0;
    rb->read_ptr = 0;
    rb->write_flag
        = 0; // W4WMT set this flag after writes & reset it after reads
    if ((rb->buf = (double*)malloc(rb->size * sizeof(double))) == NULL) {
        free(rb);
        return NULL;
    }

    return rb;
}

void ringbuffer_free(ringbuffer_t* rb) {
    free(rb->buf);
    free(rb);
}

void ringbuffer_reset_size(ringbuffer_t* rb, int sz) {
    rb->size = sz;
    rb->size_mask = rb->size;
    rb->size_mask -= 1;
    rb->read_ptr = 0;
    rb->write_ptr = 0;
    rb->write_flag = 0;
}

void ringbuffer_reset(ringbuffer_t* rb) {
    rb->read_ptr = 0;
    rb->write_ptr = 0;
    rb->write_flag = 0;
    memset(rb->buf, 0, rb->size);
}

void ringbuffer_clear(ringbuffer_t* rb, int sz) {
    double* zero = (double*)malloc(sz * sizeof(double));

    memset(zero, 0, sz * sizeof(double));
    ringbuffer_write(rb, zero, sz);
    free(zero);
}

void ringbuffer_restart(ringbuffer_t* rb, int sz) {
    ringbuffer_reset(rb);
    ringbuffer_clear(rb, sz);
}

int ringbuffer_read_space(const ringbuffer_t* rb) {
    int w, r;
    int wf;

    w = rb->write_ptr;
    r = rb->read_ptr;
    wf = rb->write_flag;

    if (w > r) {
        return w - r;
    } else if (w < r) {
        return (w - r + rb->size);
    } else {
        return (wf ? rb->size : 0);
    }
}

int ringbuffer_write_space(const ringbuffer_t* rb) {
    int w, r;
    int wf;

    w = rb->write_ptr;
    r = rb->read_ptr;
    wf = rb->write_flag;

    if (w > r) {
        return (r - w + rb->size);
    } else if (w < r) {
        return (r - w);
    } else {
        return (wf ? 0 : rb->size);
    }
}

int ringbuffer_write(ringbuffer_t* rb, const double* src, int cnt) {
    int free_cnt;
    int cnt2;
    int to_write;
    int n1, n2;

    if ((free_cnt = ringbuffer_write_space(rb)) == 0) {
        return 0;
    }

    to_write = cnt > free_cnt ? free_cnt : cnt;

    cnt2 = rb->write_ptr + to_write;

    if (cnt2 > rb->size) {
        n1 = rb->size - rb->write_ptr;
        n2 = cnt2 & rb->size_mask;
    } else {
        n1 = to_write;
        n2 = 0;
    }

    memcpy(&(rb->buf[rb->write_ptr]), src, n1 * sizeof(double));
    rb->write_ptr = (rb->write_ptr + n1) & rb->size_mask;

    if (n2) {
        memcpy(&(rb->buf[rb->write_ptr]), src + n1, n2 * sizeof(double));
        rb->write_ptr = (rb->write_ptr + n2) & rb->size_mask;
    }

    rb->write_flag = 1;

    return to_write;
}

int ringbuffer_read(ringbuffer_t* rb, double* dest, int cnt) {
    int free_cnt;
    int cnt2;
    int to_read;
    int n1, n2;

    if ((free_cnt = ringbuffer_read_space(rb)) == 0) {
        return 0;
    }

    to_read = cnt > free_cnt ? free_cnt : cnt;
    cnt2 = rb->read_ptr + to_read;

    if (cnt2 > rb->size) {
        n1 = rb->size - rb->read_ptr;
        n2 = cnt2 & rb->size_mask;
    } else {
        n1 = to_read;
        n2 = 0;
    }

    memcpy(dest, &(rb->buf[rb->read_ptr]), n1 * sizeof(double));
    rb->read_ptr = (rb->read_ptr + n1) & rb->size_mask;

    if (n2) {
        memcpy(dest + n1, &(rb->buf[rb->read_ptr]), n2 * sizeof(double));
        rb->read_ptr = (rb->read_ptr + n2) & rb->size_mask;
    }

    rb->write_flag = 0;

    return to_read;
}
