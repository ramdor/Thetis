/*  sidetone.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2024 Warren Pratt, NR0V

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

warren@pratt.one

*/

#include "cmcomm.h"

SIDETONE psidetone[cmMAXxmtr];	// Use 'id' 0 through 'cmMAXxmtr - 1'

void calc_tone1(SIDETONE a)
{
	a->tone_phs = 0.0;
	a->tone_cos_phs = cos(a->tone_phs);
	a->tone_sin_phs = sin(a->tone_phs);
	a->tone_delta = TWOPI * a->pitch / (double)a->rate;
	a->tone_cosdelta = cos(a->tone_delta);
	a->tone_sindelta = sin(a->tone_delta);
}

void calc_rising_edge(SIDETONE a)
{
	int i;
	a->nrise = (int)(a->edgelength * a->rate);
	a->rise_samps = (double*)malloc0((a->nrise + 1) * sizeof(double));
	switch (a->edgetype)
	{
	case 0:		// raised-cosine
		a->rise_delta = PI / (double)a->nrise;
		a->rise_theta = 0.0;
		for (i = 0; i <= a->nrise; i++)
		{
			a->rise_samps[i] = 0.5 * (1.0 - cos(a->rise_theta));
			a->rise_theta += a->rise_delta;
		}
		break;
	}
}

void decalc_rising_edge(SIDETONE a)
{
	_aligned_free(a->rise_samps);
}

void calc_falling_edge(SIDETONE a)
{
	int i;
	a->nfall = (int)(a->edgelength * a->rate);
	a->fall_samps = (double*)malloc0((a->nfall + 1) * sizeof(double));
	switch (a->edgetype)
	{
	case 0:		// raised-cosine
		a->fall_delta = PI / (double)a->nfall;
		a->fall_theta = PI;
		for (i = 0; i <= a->nfall; i++)
		{
			a->fall_samps[i] = 0.5 * (1.0 - cos(a->fall_theta));
			a->fall_theta -= a->fall_delta;
		}
		break;
	}
}

void decalc_falling_edge(SIDETONE a)
{
	_aligned_free(a->fall_samps);
}

void calc_wpm_times(SIDETONE a)
{
	double dot_time, dash_time;			// times in seconds
	dot_time = 1.2 / (double)a->wpm;
	dash_time = 3.0 * dot_time;
	a->n_dot_high  = (int)round((dot_time  - a->edgelength) * (double)a->rate);
	a->n_dash_high = (int)round((dash_time - a->edgelength) * (double)a->rate);
}

void calc_sidetone(SIDETONE a)
{
	calc_tone1(a);
	calc_rising_edge(a);
	calc_falling_edge(a);
	calc_wpm_times(a);
	a->state = 0;
	a->ntimer = 0;
}

void decalc_sidetone(SIDETONE a)
{
	decalc_rising_edge(a);
	decalc_falling_edge(a);
}

SIDETONE create_sidetone(
	int id,
	int run, 
	int rate, 
	int size, 
	double* in,
	double* out,
	double pitch,						// Hertz
	double volume,						// 0.0 - 1.0
	int wpm,							// words-per-minute
	int edgetype,						// '0' = raised-cosine
	double edgelength					// time in seconds
	)
{
	SIDETONE a = (SIDETONE)malloc0(sizeof(sidetone));
	a->id = id;
	a->run = run;
	a->rate = rate;
	a->size = size;
	a->in = in;
	a->out = out;
	a->pitch = pitch;
	a->volume = volume;
	a->wpm = wpm;
	a->edgetype = edgetype;
	a->edgelength = edgelength;
	InitializeCriticalSectionAndSpinCount(&a->update, 2500);
	a->key = 0;
	a->dot = 0;
	a->dash = 0;
	a->use_key = 0;
	psidetone[a->id] = a;
	return a;
}

void destroy_sidetone(int id)
{
	SIDETONE a = psidetone[id];
	decalc_sidetone(a);
	DeleteCriticalSection(&a->update);
}

enum States
{
	LOW,
	RISE,
	HIGH,
	FALL
};

void osc_init(SIDETONE a)
{
	a->tone_cos_phs = cos(a->tone_phs);
	a->tone_sin_phs = sin(a->tone_phs);
}

void osc(SIDETONE a)
{
	double t1, t2;
	a->tone_out0 = +(a->tone_cos_phs);
	a->tone_out1 = -(a->tone_sin_phs);
	t1 = a->tone_cos_phs;
	t2 = a->tone_sin_phs;
	a->tone_cos_phs = t1 * a->tone_cosdelta - t2 * a->tone_sindelta;
	a->tone_sin_phs = t1 * a->tone_sindelta + t2 * a->tone_cosdelta;
	a->tone_phs += a->tone_delta;
	if (a->tone_phs >= TWOPI) a->tone_phs -= TWOPI;
	if (a->tone_phs <  0.0  ) a->tone_phs += TWOPI;
	return;
}

void xsidetone(int id)
{
	SIDETONE a = psidetone[id];
	int i;
	
	if (a->run)
	{
		osc_init(a);
		for (i = 0; i < a->size; i++)
		{
			EnterCriticalSection(&a->update);
			osc(a);
			switch (a->state)
			{
			case LOW:
				a->out[2 * i + 0] = 0.0;
				a->out[2 * i + 1] = 0.0;
				if (a->key || a->dot || a->dash)
					a->state = RISE;
				a->ntimer = 0;
				break;
			case RISE:
				a->out[2 * i + 0] = a->rise_samps[a->ntimer] * a->tone_out0 * a->volume;
				a->out[2 * i + 1] = a->rise_samps[a->ntimer] * a->tone_out1 * a->volume;
				++(a->ntimer);
				if (a->ntimer > a->nrise)
				{
					if (a->key || a->dot || a->dash)
					{
						a->state = HIGH;
						if (a->dot)  a->ntimer = a->n_dot_high;
						if (a->dash) a->ntimer = a->n_dash_high;
					}
					else
					{
						a->state = FALL;
						a->ntimer = 0;
					}
				}
				break;
			case HIGH:
				a->out[2 * i + 0] = a->tone_out0 * a->volume;
				a->out[2 * i + 1] = a->tone_out1 * a->volume;
				// a->out[2 * i + 0] = a->in[2 * i + 0];			// for testing
				// a->out[2 * i + 1] = a->in[2 * i + 1];
				if (a->ntimer > 0)
					--(a->ntimer);
				if (a->ntimer <= 0 && !a->key)
				{
					a->state = FALL;
					a->ntimer = 0;
				}
				break;
			case FALL:
				a->out[2 * i + 0] = a->fall_samps[a->ntimer] * a->tone_out0 * a->volume;
				a->out[2 * i + 1] = a->fall_samps[a->ntimer] * a->tone_out1 * a->volume;
				++(a->ntimer);
				if (a->ntimer > a->nfall)
				{
					a->dot = 0;
					a->dash = 0;
					if (a->key)
					{
						a->state = RISE;
						a->ntimer = 0;
					}
					else
					{
						a->state = LOW;
						a->ntimer = 0;
					}
				}
				break;
			}
			LeaveCriticalSection(&a->update);
		}
	}
	else
	{
		if (a->out != a->in) memcpy(a->out, a->in, a->size * sizeof(complex));
	}
}

void setSidetoneRate(int id, int rate)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	decalc_sidetone(a);
	a->rate = rate;
	calc_sidetone(a);
	LeaveCriticalSection(&a->update);
}

void setSidetoneSize(int id, int size)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	a->size = size;
	LeaveCriticalSection(&a->update);
}

PORT
void SetSidetoneSelectKey(int id, int select)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	a->use_key = select;
	LeaveCriticalSection(&a->update);
}

PORT
void keySidetone(int id, int key_select, int state)	// 0 = KEY-UP; 1 = KEY-DOWN
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	if (key_select == a->use_key)
		a->key = state;
	LeaveCriticalSection(&a->update);
}

PORT
void makedotSidetone(int id)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	a->dot = 1;
	LeaveCriticalSection(&a->update);
}

PORT
void makedashSidetone(int id)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	a->dash = 1;
	LeaveCriticalSection(&a->update);
}

PORT
void SetSidetoneVolume(int id, double volume)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	a->volume = volume;
	LeaveCriticalSection(&a->update);
}

PORT
void SetSidetoneWPM(int id, int wpm)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	a->wpm = wpm;
	calc_wpm_times(a);
	LeaveCriticalSection(&a->update);
}

PORT
void SetSidetoneRun(int id, int run)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	a->run = run;
	LeaveCriticalSection(&a->update);
}

PORT
void SetSidetonePitch(int id, double pitch)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	a->pitch = pitch;
	calc_tone1(a);
	LeaveCriticalSection(&a->update);
}

PORT
void SetSidetoneEdgetype(int id, int type)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	decalc_rising_edge(a);
	decalc_falling_edge(a);
	a->edgetype = type;
	calc_rising_edge(a);
	calc_falling_edge(a);
	LeaveCriticalSection(&a->update);
}

PORT
void SetSidetoneEdgelength(int id, double length)
{
	SIDETONE a = psidetone[id];
	EnterCriticalSection(&a->update);
	decalc_rising_edge(a);
	decalc_falling_edge(a);
	a->edgelength = length;
	calc_rising_edge(a);
	calc_falling_edge(a);
	LeaveCriticalSection(&a->update);
}
