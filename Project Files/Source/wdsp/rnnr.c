/*  rnnr.c

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2000-2025 Original authors
Copyright (C) 2020-2025 Richard Samphire MW0LGE

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

mw0lge@grange-lane.co.uk

This code is based on code and ideas from  : https://github.com/vu3rdd/wdsp
and and uses RNNoise
https://gitlab.xiph.org/xiph/rnnoise

It uses a non modified version of rmnoise and implements a ringbuffer to handle input/output frame size differences.
*/
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//

#define _CRT_SECURE_NO_WARNINGS
#include "comm.h"
#include "rnnoise.h"

static inline float db_to_lin(float db) { return powf(10.0f, db / 20.0f); }
static inline float lin_to_db(float lin) { return 20.0f * log10f(fmaxf(lin, 1e-12f)); }

#define AGC_TARGET_DB   (60.0f)
#define AGC_MIN_DB      (-12.0f)
#define AGC_MAX_DB      (+220.0f)
#define AGC_ATTACK_MS   (10.0f)
#define AGC_RELEASE_MS  (200.0f)
#define AGC_RMS_FLOOR   (1e-6f)
#define SAFETY_CEIL     (30000.0f)

static float agc_alpha_ms(float ms, float frame_hz) {
    const float tc = ms / 1000.0f;
    const float a = expf(-(1.0f / frame_hz) / tc);
    return a < 0.0f ? 0.0f : (a > 1.0f ? 1.0f : a);
}

void rnnr_agc_init(RNNR a)
{
    const float frame_hz = (a->frame_size > 0) ? ((float)a->rate / (float)a->frame_size) : 100.0f;
    a->agc_att_a = agc_alpha_ms(AGC_ATTACK_MS, frame_hz);
    a->agc_rel_a = agc_alpha_ms(AGC_RELEASE_MS, frame_hz);
    a->gain_db = AGC_TARGET_DB;
    a->gain = db_to_lin(a->gain_db);
}

static float frame_rms(const float* x, int n) {
    double s = 0.0;
    for (int i = 0; i < n; i++) { double v = (double)x[i]; s += v * v; }
    float r = (float)sqrt(s / (double)n);
    return (r < AGC_RMS_FLOOR) ? AGC_RMS_FLOOR : r;
}

//used to track RNNR instances
static RNNR* _rnnr_instances = NULL;
static int _rnnr_count = 0;
static int _rnnr_capacity = 0;

// the model to use when creating new RNNR instances
static RNNModel* _rnnr_model = NULL;

//ringbuffer
static void ring_buffer_init(rnnr_ring_buffer* rb, int capacity) 
{
    rb->buf = malloc0(capacity * sizeof(float));
    rb->capacity = capacity;
    rb->head = 0;
    rb->tail = 0;
    rb->count = 0;
}

static void ring_buffer_free(rnnr_ring_buffer* rb) 
{
    _aligned_free(rb->buf);
    rb->buf = NULL;
    rb->capacity = 0;
    rb->head = rb->tail = rb->count = 0;
}

static void ring_buffer_put(rnnr_ring_buffer* rb, float v) 
{
    if (rb->count < rb->capacity) 
    {
        rb->buf[rb->tail] = v;
        rb->tail = (rb->tail + 1) % rb->capacity;
        rb->count++;
    }
}

static int ring_buffer_get_bulk(rnnr_ring_buffer* rb, float* dest, int n)
{
    int to_get = n < rb->count ? n : rb->count;
    for (int i = 0; i < to_get; i++)
    {
        dest[i] = rb->buf[rb->head];
        rb->head = (rb->head + 1) % rb->capacity;
    }
    rb->count -= to_get;
    return to_get;
}

static void ring_buffer_resize(rnnr_ring_buffer* rb, int new_capacity)
{
    if (new_capacity == rb->capacity) return;
    float* new_buf = malloc0(new_capacity * sizeof(float));
    int cnt = rb->count;
    for (int i = 0; i < cnt; i++)
    {
        new_buf[i] = rb->buf[(rb->head + i) % rb->capacity];
    }
    _aligned_free(rb->buf);
    rb->buf = new_buf;
    rb->capacity = new_capacity;
    rb->head = 0;
    rb->tail = cnt % new_capacity;
}
//

PORT
void SetRXARNNRRun (int channel, int run)
{
	RNNR a = rxa[channel].rnnr.p;
	if (a->run != run)
	{
		RXAbp1Check (channel, rxa[channel].amd.p->run, rxa[channel].snba.p->run, 
                             rxa[channel].emnr.p->run, rxa[channel].anf.p->run, rxa[channel].anr.p->run,
                             run, rxa[channel].sbnr.p->run); // NR3 + NR4 support

		EnterCriticalSection (&ch[channel].csDSP);
		a->run = run;
		RXAbp1Set (channel);
		LeaveCriticalSection (&ch[channel].csDSP);
	}
}

void setSize_rnnr(RNNR a, int size) 
{
    _aligned_free(a->output_buffer);
    a->buffer_size = size;
    a->output_buffer = malloc0(a->buffer_size * sizeof(float));

    int new_cap = a->frame_size + a->buffer_size;
    ring_buffer_resize(&a->input_ring, new_cap);
    ring_buffer_resize(&a->output_ring, new_cap);
}

void setBuffers_rnnr(RNNR a, double* in, double* out)
{
	a->in = in;
	a->out = out;
}

void setSamplerate_rnnr(RNNR a, int rate)
{
    a->rate = rate;
    rnnr_agc_init(a);
}

RNNR create_rnnr(int run, int position, int size, double* in, double* out, int rate)
{
    RNNR a = malloc0(sizeof(rnnr));
    InitializeCriticalSection(&a->cs);
    a->run = run;
    a->position = position;
    a->rate = rate; // not used currently, but here for future use
    a->st = rnnoise_create(_rnnr_model);    
    a->frame_size = rnnoise_get_frame_size();
    a->in = in;
    a->out = out;
    a->buffer_size = size;
    rnnr_agc_init(a);

    ring_buffer_init(&a->input_ring, a->frame_size + a->buffer_size);
    ring_buffer_init(&a->output_ring, a->frame_size + a->buffer_size);
    a->to_process_buffer = malloc0(a->frame_size * sizeof(float));
    a->processed_output_buffer = malloc0(a->frame_size * sizeof(float));
    a->output_buffer = malloc0(a->buffer_size * sizeof(float));

    // used to maintain a record of RNNR's here and is used so we can update them all if/when model is changed
    if (_rnnr_count == _rnnr_capacity)
    {
        int new_cap = _rnnr_capacity ? _rnnr_capacity * 2 : 4; // limit number of reallocs by doubling space each time, overkill but that is my middle name ;)
        RNNR* tmp = malloc0(new_cap * sizeof(RNNR));
        memcpy(tmp, _rnnr_instances, _rnnr_count * sizeof(RNNR));
        _aligned_free(_rnnr_instances);
        _rnnr_instances = tmp;
        _rnnr_capacity = new_cap;
    }
    _rnnr_instances[_rnnr_count++] = a;
    //

    return a;
}

void xrnnr(RNNR a, int pos)
{
    if (a->st && a->run && pos == a->position)
    {
        int  bs = a->buffer_size;
        int  fs = a->frame_size;
        float* to_process = a->to_process_buffer;
        float* process_out = a->processed_output_buffer;

        EnterCriticalSection(&a->cs);
        for (int i = 0; i < bs; i++)
        {
            ring_buffer_put(&a->input_ring, (float)a->in[2 * i + 0]);

            if (a->input_ring.count >= fs)
            {
                ring_buffer_get_bulk(&a->input_ring, to_process, fs);

                float rms = frame_rms(to_process, fs);
                float cur_db = lin_to_db(rms);
                float desired_db = AGC_TARGET_DB - cur_db;

                float alpha = (desired_db > a->gain_db) ? a->agc_att_a : a->agc_rel_a;
                a->gain_db = alpha * a->gain_db + (1.0f - alpha) * desired_db;
                if (a->gain_db < AGC_MIN_DB) a->gain_db = AGC_MIN_DB;
                if (a->gain_db > AGC_MAX_DB) a->gain_db = AGC_MAX_DB;

                a->gain = db_to_lin(a->gain_db);

                for (int j = 0; j < fs; j++)
                {
                    float v = to_process[j] * a->gain;
                    if (v > SAFETY_CEIL) v = SAFETY_CEIL;
                    if (v < -SAFETY_CEIL) v = -SAFETY_CEIL;
                    to_process[j] = v;
                }

                rnnoise_process_frame(a->st, process_out, to_process);

                const float inv = (a->gain > 0.0f) ? (1.0f / a->gain) : 0.0f;
                for (int j = 0; j < fs; j++)
                {
                    ring_buffer_put(&a->output_ring, process_out[j] * inv);
                }
            }
        }
        LeaveCriticalSection(&a->cs);

        if (a->output_ring.count >= bs)
        {
            ring_buffer_get_bulk(&a->output_ring, a->output_buffer, bs);
            for (int i = 0; i < bs; i++)
            {
                a->out[2 * i + 0] = (double)a->output_buffer[i];
                a->out[2 * i + 1] = 0;
            }
        }
        else
        {
            memcpy(a->out, a->in, a->buffer_size * sizeof(complex));
        }
    }
    else if (a->out != a->in)
    {
        memcpy(a->out, a->in, a->buffer_size * sizeof(complex));
    }
}

void destroy_rnnr(RNNR a) 
{
    // we dont need to maintain order, so just replace with last, and decrement total
    for (int i = 0; i < _rnnr_count; i++)
    {
        if (_rnnr_instances[i] == a)
        {
            _rnnr_instances[i] = _rnnr_instances[--_rnnr_count];
            break;
        }
    }

    EnterCriticalSection(&a->cs);
    rnnoise_destroy(a->st);
    LeaveCriticalSection(&a->cs);
    DeleteCriticalSection(&a->cs);

    _aligned_free(a->to_process_buffer);
    _aligned_free(a->processed_output_buffer);
    _aligned_free(a->output_buffer);
    ring_buffer_free(&a->input_ring);
    ring_buffer_free(&a->output_ring);
    _aligned_free(a);

    // tidy if none now in use
    if (_rnnr_count == 0)
    {
        _aligned_free(_rnnr_instances);
        _rnnr_instances = NULL;
        _rnnr_capacity = 0;
    }
}

PORT
void RNNRloadModel(const char* file_path)
{
    // destroy any in use
    for (int i = 0; i < _rnnr_count; i++)
    {
        RNNR a = _rnnr_instances[i];
        EnterCriticalSection(&a->cs);
        a->run_old = a->run;
        a->run = 0;
        rnnoise_destroy(a->st);
        LeaveCriticalSection(&a->cs);
    }

    // free up any previous loaded model
    if (_rnnr_model)
    {
        rnnoise_model_free(_rnnr_model);
    }

    _rnnr_model = NULL; // default to baked in model

    // try to load
    if (file_path && file_path[0])
    {
        _rnnr_model = rnnoise_model_from_filename(file_path);
    }

    // recreate any we had created previously and restart if needed
    for (int i = 0; i < _rnnr_count; i++)
    {
        RNNR a = _rnnr_instances[i];
        EnterCriticalSection(&a->cs);
        a->st = rnnoise_create(_rnnr_model);
        a->run = a->run_old;
        LeaveCriticalSection(&a->cs);
    }
}

PORT
void SetRXARNNRPosition(int channel, int position)
{
    EnterCriticalSection(&ch[channel].csDSP);
    rxa[channel].rnnr.p->position = position;
    rxa[channel].bp1.p->position = position;
    LeaveCriticalSection(&ch[channel].csDSP);
}

