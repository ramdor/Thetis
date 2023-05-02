// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/*  lmath.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2015, 2016 Warren Pratt, NR0V

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

#include "comm.h"

void dR (int n, double* r, double* y)
{
	int i, j, k;
    double alpha, beta, gamma;
	double* z = (double *) malloc0 ((n - 1) * sizeof (double));
    y[0] = -r[1];
    alpha = -r[1];
    beta = 1.0;
    for (k = 0; k < n - 1; k++)
    {
        beta *= 1.0 - alpha * alpha;
        gamma = 0.0;
        for (i = k + 1, j = 0; i > 0; i--, j++)
            gamma += r[i] * y[j];
        alpha = - (r[k + 2] + gamma) / beta;
        for (i = 0, j = k; i <= k; i++, j--)
            z[i] = y[i] + alpha * y[j];
		memcpy (y, z, (k + 1) * sizeof (double));
        y[k + 1] = alpha;
    }
	_aligned_free (z);
}

void trI (
    int n,
    double* r,
    double* B
    )
{
    int i, j, ni, nj;
    double gamma, t, scale, b;
	double* y = (double *) malloc0 ((n - 1) * sizeof (double));
	double* v = (double *) malloc0 ((n - 1) * sizeof (double));
    scale = 1.0 / r[0];
    for (i = 0; i < n; i++)
        r[i] *= scale;
    dR(n - 1, r, y);

    t = 0.0;
    for (i = 0; i < n - 1; i++)
        t += r[i + 1] * y[i];
    gamma = 1.0 / (1.0 + t);
    for (i = 0, j = n - 2; i < n - 1; i++, j--)
        v[i] = gamma * y[j];
    B[0] = gamma;
    for (i = 1, j = n - 2; i < n; i++, j--)
        B[i] = v[j];
    for (i = 1; i <= (n - 1) / 2; i++)
        for (j = i; j < n - i; j++)
            B[i * n + j] = B[(i - 1) * n + (j - 1)] + (v[n - j - 1] * v[n - i - 1] - v[i - 1] * v[j - 1]) / gamma;
    for (i = 0; i <= (n - 1)/2; i++)
        for (j = i; j < n - i; j++)
        {
            b = B[i * n + j] *= scale;
            B[j * n + i] = b;
            ni = n - i - 1;
            nj = n - j - 1;
            B[ni * n + nj] = b;
            B[nj * n + ni] = b;
        }
    _aligned_free (v);
    _aligned_free (y);
}

void asolve(int xsize, int asize, double* x, double* a)
{
    int i, j, k;
    double beta, alpha, t;
	double* r = (double *) malloc0 ((asize + 1) * sizeof (double));
	double* z = (double *) malloc0 ((asize + 1) * sizeof (double));
    for (i = 0; i <= asize; i++)
    {
		for (j = 0; j < xsize; j++)
			r[i] += x[j] * x[j - i];
    }
    z[0] = 1.0;
    beta = r[0];
    for (k = 0; k < asize; k++)
    {
        alpha = 0.0;
        for (j = 0; j <= k; j++)
            alpha -= z[j] * r[k + 1 - j];
        alpha /= beta;
        for (i = 0; i <= (k + 1) / 2; i++)
        {
            t = z[k + 1 - i] + alpha * z[i];
            z[i] = z[i] + alpha * z[k + 1 - i];
            z[k + 1 - i] = t;
        }
        beta *= 1.0 - alpha * alpha;
    }
    for (i = 0; i < asize; i++)
	{
        a[i] = - z[i + 1];
		if (a[i] != a[i]) a[i] = 0.0;
	}

	_aligned_free (z);
	_aligned_free (r);
}

void median (int n, double* a, double* med)
{
    int S0, S1, i, j, m, k;
    double x, t;
    S0 = 0;
    S1 = n - 1;
    k = n / 2;
    while (S1 > S0 + 1)
    {
        m = (S0 + S1) / 2;
        t = a[m];
        a[m] = a[S0 + 1];
        a[S0 + 1] = t;
        if (a[S0] > a[S1])
        {
            t = a[S0];
            a[S0] = a[S1];
            a[S1] = t;
        }
        if (a[S0 + 1] > a[S1])
        {
            t = a[S0 + 1];
            a[S0 + 1] = a[S1];
            a[S1] = t;
        }
        if (a[S0] > a[S0 + 1])
        {
            t = a[S0];
            a[S0] = a[S0 + 1];
            a[S0 + 1] = t;
        }
        i = S0 + 1;
        j = S1;
        x = a[S0 + 1];
		do i++; while (a[i] < x);
        do j--; while (a[j] > x);
        while (j >= i)
        {
            t = a[i];
            a[i] = a[j];
            a[j] = t;
			do i++; while (a[i] < x);
            do j--; while (a[j] > x);
        }
        a[S0 + 1] = a[j];
        a[j] = x;
        if (j >= k) S1 = j - 1;
        if (j <= k) S0 = i;
    }
    if (S1 == S0 + 1 && a[S1] < a[S0])
    {
        t = a[S0];
        a[S0] = a[S1];
        a[S1] = t;
    }
	*med = a[k];
}