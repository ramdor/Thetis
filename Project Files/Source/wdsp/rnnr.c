/*  rnnr.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

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
and and uses RNNoise and libspecbleach
https://gitlab.xiph.org/xiph/rnnoise
https://github.com/lucianodato/libspecbleach
*/
 
#define _CRT_SECURE_NO_WARNINGS
#include "comm.h"

#include "rnnoise.h"

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
    a->output_buffer = (float*)malloc0(a->buffer_size * sizeof(float));
}

void setBuffers_rnnr (RNNR a, double* in, double* out)
{
	a->in = in;
	a->out = out;
}

RNNR create_rnnr (int run, int position, double *in, double *out)
{
    RNNR a = (RNNR) malloc0 (sizeof (rnnr));

    a->run = run;
    a->position = position;
    a->st = rnnoise_create(NULL);
    a->frame_size = rnnoise_get_frame_size();
    a->in = in;
    a->out = out;
    a->buffer_size = 64;
    a->gain = 5000000.0;// 500000.0; // large gain factor, seems to change with model

    a->input_queue_head = NULL;
    a->input_queue_tail = NULL;
    a->input_queue_count = 0;

    a->output_queue_head = NULL;
    a->output_queue_tail = NULL;
    a->output_queue_count = 0;

    a->output_buffer = (float*)malloc0(a->buffer_size * sizeof(float));
    a->to_process_buffer = (float*)malloc0(a->frame_size * sizeof(float));
    a->processed_output_buffer = (float*)malloc0(a->frame_size * sizeof(float));

    return a;
}


void input_enqueue(RNNR a, float value) {
    queuenode* new_node = (queuenode*)malloc(sizeof(queuenode));
    new_node->value = value;
    new_node->next = NULL;

    if (a->input_queue_tail) {
        a->input_queue_tail->next = new_node;
    }
    else {
        a->input_queue_head = new_node;
    }

    a->input_queue_tail = new_node;
    a->input_queue_count++;
}
void input_dequeue_bulk(RNNR a, float* buffer, int count) {
    for (int i = 0; i < count; i++) {
        queuenode* temp = a->input_queue_head;
        buffer[i] = temp->value;
        a->input_queue_head = a->input_queue_head->next;
        free(temp);
        a->input_queue_count--;

        if (!a->input_queue_head) {
            a->input_queue_tail = NULL;
        }
    }
}

void output_enqueue(RNNR a, float value) {
    queuenode* new_node = (queuenode*)malloc(sizeof(queuenode));
    new_node->value = value;
    new_node->next = NULL;

    if (a->output_queue_tail) {
        a->output_queue_tail->next = new_node;
    }
    else {
        a->output_queue_head = new_node;
    }

    a->output_queue_tail = new_node;
    a->output_queue_count++;
}
void output_dequeue_bulk(RNNR a, float *buffer, int count) {
    for (int i = 0; i < count; i++) {
        queuenode* temp = a->output_queue_head;
        buffer[i] = temp->value;
        a->output_queue_head = a->output_queue_head->next;
        free(temp);
        a->output_queue_count--;

        if (!a->output_queue_head) {
            a->output_queue_tail = NULL;
        }
    }
}

void xrnnr (RNNR a, int pos)
{
    if (a->run && pos == a->position) 
    {
        // MW0LGE
        // add buffer size samples # to the input queue
        // if we have >= rnnoise frame size in input queue remove them from the head of input queue and process them
        // add the output from the process to the end of the output queue
        // take buffer size sample from the front of the output queue if available and send them on their way outa here !

        double* in = a->in;
        double* out = a->out;
        float   gain = a->gain;
        int     bs = a->buffer_size;
        int     fs = a->frame_size;
        float*  to_proc = a->to_process_buffer;
        float*  proc_out = a->processed_output_buffer;

        for (int i = 0; i < bs; i++) 
        {
            input_enqueue(a, (float)in[2 * i] * gain);

            if (a->input_queue_count >= fs)
            {
                input_dequeue_bulk(a, to_proc, fs);

                rnnoise_process_frame(a->st, proc_out, to_proc);

                for (int ii = 0; ii < fs; ii++)
                {
                    output_enqueue(a, proc_out[ii] / gain);
                }
            }
        }

        if (a->output_queue_count >= bs)
        {
            output_dequeue_bulk(a, a->output_buffer, bs);
            for (int i = 0; i < bs; i++)
            {
                out[2 * i] = (double)a->output_buffer[i];
                out[2 * i + 1] = 0;
            }
        }
        else
        {
            memcpy(out, in, a->buffer_size * sizeof(complex));
        }
    }
    else if (a->out != a->in)
    {
        memcpy(a->out, a->in, a->buffer_size * sizeof(complex));
    }
}

void destroy_rnnr (RNNR a)
{
    rnnoise_destroy(a->st);
    _aligned_free(a->to_process_buffer);
    _aligned_free(a->processed_output_buffer);
    _aligned_free(a->output_buffer);
    _aligned_free (a);
}

PORT
void SetRXARNNRgain(int channel, float gain)
{
    if (gain <= 0) return;

    EnterCriticalSection(&ch[channel].csDSP);
    rxa[channel].rnnr.p->gain = gain;
    LeaveCriticalSection(&ch[channel].csDSP);
}
