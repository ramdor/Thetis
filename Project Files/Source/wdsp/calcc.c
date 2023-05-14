/*  calcc.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2013, 2014, 2016, 2019 Warren Pratt, NR0V

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

#define _CRT_SECURE_NO_WARNINGS
#include "comm.h"
#include "klj_mem.h"

void size_calcc(CALCC a) { // for change in ints or spi
    int i;
    a->nsamps = a->ints * a->spi;
    a->t = (double*)malloc0((a->ints + 1) * sizeof(double));
    a->tmap = (double*)malloc0((a->ints + 1) * sizeof(double));
    for (i = 0; i < a->ints + 1; i++) a->t[i] = (double)i / (double)a->ints;

    a->cm = (double*)malloc0(a->ints * 4 * sizeof(double));
    a->cc = (double*)malloc0(a->ints * 4 * sizeof(double));
    a->cs = (double*)malloc0(a->ints * 4 * sizeof(double));
    a->cm_old = (double*)malloc0(a->ints * 4 * sizeof(double));

    a->rxs = (double*)malloc0(a->nsamps * sizeof(complex));
    a->txs = (double*)malloc0(a->nsamps * sizeof(complex));

    a->ctrl.cpi = (int*)malloc0(a->ints * sizeof(int));
    a->ctrl.sindex = (int*)malloc0(a->ints * sizeof(int));
    a->ctrl.sbase = (int*)malloc0(a->ints * sizeof(int));

    for (i = 0; i < a->ints; i++) {
        a->ctrl.cpi[i] = 0;
        a->ctrl.sindex[i] = 0;
        a->ctrl.sbase[i] = i * a->spi;
    }

    a->disp.x = (double*)malloc0(a->nsamps * sizeof(double));
    a->disp.ym = (double*)malloc0(a->nsamps * sizeof(double));
    a->disp.yc = (double*)malloc0(a->nsamps * sizeof(double));
    a->disp.ys = (double*)malloc0(a->nsamps * sizeof(double));
    a->disp.cm = (double*)malloc0(a->ints * 4 * sizeof(double));
    a->disp.cc = (double*)malloc0(a->ints * 4 * sizeof(double));
    a->disp.cs = (double*)malloc0(a->ints * 4 * sizeof(double));
}

void desize_calcc(CALCC a) {
    _aligned_free(a->disp.cs);
    _aligned_free(a->disp.cc);
    _aligned_free(a->disp.cm);
    _aligned_free(a->disp.ys);
    _aligned_free(a->disp.yc);
    _aligned_free(a->disp.ym);
    _aligned_free(a->disp.x);
    _aligned_free(a->ctrl.sbase);
    _aligned_free(a->ctrl.sindex);
    _aligned_free(a->ctrl.cpi);
    _aligned_free(a->rxs);
    _aligned_free(a->txs);
    _aligned_free(a->cm_old);
    _aligned_free(a->cm);
    _aligned_free(a->cc);
    _aligned_free(a->cs);
    _aligned_free(a->tmap);
    _aligned_free(a->t);
}

CALCC create_calcc(int channel, int runcal, int size, int rate, int ints,
    int spi, double hw_scale, double moxdelay, double loopdelay, double ptol,
    int mox, int solidmox, int pin, int map, int stbl, int npsamps,
    double alpha) {
    CALCC a = (CALCC)malloc0(sizeof(calcc));
    a->channel = channel;
    a->runcal = runcal;
    a->size = size;
    a->rate = rate;
    a->ints = ints;
    a->spi = spi;
    a->hw_scale = hw_scale;
    a->ctrl.moxdelay = moxdelay;
    a->ctrl.loopdelay = loopdelay;
    a->ptol = ptol;
    a->mox = mox;
    a->solidmox = solidmox;
    a->pin = pin;
    a->map = map;
    a->stbl = stbl;
    a->npsamps = npsamps;
    a->alpha = alpha;

    a->info = (int*)malloc0(16 * sizeof(int));
    a->binfo = (int*)malloc0(16 * sizeof(int));

    a->ctrl.state = 0;
    a->ctrl.reset = 0;
    a->ctrl.automode = 0;
    a->ctrl.mancal = 0;
    a->ctrl.turnon = 0;
    a->ctrl.moxsamps = (int)(a->rate * a->ctrl.moxdelay);
    a->ctrl.moxcount = 0;
    a->ctrl.count = 0;
    a->ctrl.full_ints = 0;
    a->ctrl.calcinprogress = 0;
    a->ctrl.calcdone = 0;
    a->ctrl.waitsamps = (int)(a->rate * a->ctrl.loopdelay);
    a->ctrl.waitcount = 0;
    a->ctrl.running = 0;
    a->ctrl.current_state = 0;
    InitializeCriticalSectionAndSpinCount(
        &txa[a->channel].calcc.cs_update, 2500);
    a->rxdelay = create_delay(1, // run
        0, // size				[stuff later]
        0, // input buffer		[stuff later]
        0, // output buffer	[stuff later]
        a->rate, // sample rate
        20.0e-09, // delta (delay stepsize)
        0.0); // delay
    a->txdelay = create_delay(1, // run
        0, // size				[stuff later]
        0, // input buffer		[stuff later]
        0, // output buffer	[stuff later]
        a->rate, // sample rate
        20.0e-09, // delta (delay stepsize)
        0.0); // delay

    InitializeCriticalSectionAndSpinCount(&a->disp.cs_disp, 2500);
    a->util.ints = a->ints;
    a->util.channel = a->channel;

    size_calcc(a);

    a->temprx = (double*)malloc0(2048 * sizeof(complex)); // remove later
    a->temptx = (double*)malloc0(2048 * sizeof(complex)); // remove later
    return a;
}

void destroy_calcc(CALCC a) {
    _aligned_free(a->temptx); // remove later
    _aligned_free(a->temprx); // remove later
    desize_calcc(a);
    DeleteCriticalSection(&a->disp.cs_disp);
    destroy_delay(a->txdelay);
    destroy_delay(a->rxdelay);
    DeleteCriticalSection(&txa[a->channel].calcc.cs_update);
    _aligned_free(a->binfo);
    _aligned_free(a->info);
    _aligned_free(a);
}

void flush_calcc(CALCC a) {
    flush_delay(a->rxdelay);
    flush_delay(a->txdelay);
}

int fcompare(const void* a, const void* b) {
    if (*(double*)a < *(double*)b)
        return -1;
    else if (*(double*)a == *(double*)b)
        return 0;
    else
        return 1;
}

static double* wrk = 0;
static size_t last_wrk_alloc_size = 0;
static volatile int decomp_busy = 0;

void decomp(int n, double* a, int* piv, int* info) {

    assert(!decomp_busy); /// OK for static allocation?
    decomp_busy = 1;
    int i, j, k;
    int t_piv;
    double m_row, mt_row, m_col, mt_col;
    if (last_wrk_alloc_size < n) { // KLJ
        if (last_wrk_alloc_size) {
            assert(last_wrk_alloc_size < n);
        }
        last_wrk_alloc_size = n * 10;
        if (wrk) {
            _aligned_free(wrk);
        }

        wrk = (double*)malloc0((int)(last_wrk_alloc_size * sizeof(double)));
    }
    *info = 0;
    for (i = 0; i < n; i++) {
        piv[i] = i;
        m_row = 0.0;
        for (j = 0; j < n; j++) {
            mt_row = a[n * i + j];
            if (mt_row < 0.0) mt_row = -mt_row;
            if (mt_row > m_row) m_row = mt_row;
        }
        if (m_row == 0.0) {
            *info = i;
            goto cleanup;
        }
        wrk[i] = m_row;
    }
    for (k = 0; k < n - 1; k++) {
        j = k;
        m_col = a[n * piv[k] + k] / wrk[piv[k]];
        if (m_col < 0) m_col = -m_col;
        for (i = k + 1; i < n; i++) {
            mt_col = a[n * piv[i] + k] / wrk[piv[k]];
            if (mt_col < 0.0) mt_col = -mt_col;
            if (mt_col > m_col) {
                m_col = mt_col;
                j = i;
            }
        }
        if (m_col == 0) {
            *info = -k;
            goto cleanup;
        }
        t_piv = piv[k];
        piv[k] = piv[j];
        piv[j] = t_piv;
        for (i = k + 1; i < n; i++) {
            a[n * piv[i] + k] /= a[n * piv[k] + k];
            for (j = k + 1; j < n; j++)
                a[n * piv[i] + j] -= a[n * piv[i] + k] * a[n * piv[k] + j];
        }
    }
    if (a[n * n - 1] == 0.0) *info = -n;
cleanup:
    decomp_busy = 0;
    // _aligned_free(wrk);
}

void dsolve(int n, double* a, int* piv, double* b, double* x) {
    int j, k;
    double sum;

    for (k = 0; k < n; k++) {
        sum = 0.0;
        for (j = 0; j < k; j++) sum += a[n * piv[k] + j] * x[j];
        x[k] = b[piv[k]] - sum;
    }

    for (k = n - 1; k >= 0; k--) {
        sum = 0.0;
        for (j = k + 1; j < n; j++) sum += a[n * piv[k] + j] * x[j];
        x[k] = (x[k] - sum) / a[n * piv[k] + k];
    }
}

void cull(int* n, int ints, double* x, double* t, double ptol) {
    int k = 0;
    int i = *n;
    int ntopint;
    int npx;

    while (x[i - 1] > t[ints - 1]) i--;
    ntopint = *n - i;
    npx = (int)(ntopint * (1.0 - ptol));
    i = *n;
    while ((k < npx) && (x[--i] > t[ints])) k++;
    *n -= k;
}

Storage* PSBuilderArena = 0;

void builder(int points, double* x, double* y, int ints, double* t, int* info,
    double* c, double ptol) {

    static int busy = 0;
    assert(!busy);
    busy = 1;

    if (!PSBuilderArena) {
        PSBuilderArena = StorageCreate(DEFAULT_BUILDER_STORAGE_SIZE_KLJ);
        assert(PSBuilderArena);
    }
    Storage* MemArena = PSBuilderArena;

#ifdef ALLOW_MALLOC_AFTER_TAKEOFF_KLJ
    double* catxy = (double*)malloc0(2 * points * sizeof(double));
    double* sx = (double*)malloc0(points * sizeof(double));
    double* sy = (double*)malloc0(points * sizeof(double));
    double* h = (double*)malloc0(ints * sizeof(double));
    int* p = (int*)malloc0(ints * sizeof(int));
    int* np = (int*)malloc0(ints * sizeof(int));
    double u, v, alpha, beta, gamma, delta;
    double* taa = (double*)malloc0(ints * sizeof(double));
    double* tab = (double*)malloc0(ints * sizeof(double));
    double* tag = (double*)malloc0(ints * sizeof(double));
    double* tad = (double*)malloc0(ints * sizeof(double));
    double* tbb = (double*)malloc0(ints * sizeof(double));
    double* tbg = (double*)malloc0(ints * sizeof(double));
    double* tbd = (double*)malloc0(ints * sizeof(double));
    double* tgg = (double*)malloc0(ints * sizeof(double));
    double* tgd = (double*)malloc0(ints * sizeof(double));
    double* tdd = (double*)malloc0(ints * sizeof(double));
    int nsize = 3 * ints + 1;
    int intp1 = ints + 1;
    int intm1 = ints - 1;
    double* A = (double*)malloc0(intp1 * intp1 * sizeof(double));
    double* B = (double*)malloc0(intp1 * intp1 * sizeof(double));
    double* C = (double*)malloc0(intm1 * intp1 * sizeof(double));
    double* D = (double*)malloc0(intp1 * sizeof(double));
    double* E = (double*)malloc0(intp1 * intp1 * sizeof(double));
    double* F = (double*)malloc0(intm1 * intp1 * sizeof(double));
    double* G = (double*)malloc0(intp1 * sizeof(double));
    double* MAT = (double*)malloc0(nsize * nsize * sizeof(double));
    double* RHS = (double*)malloc0(nsize * sizeof(double));
    double* SLN = (double*)malloc0(nsize * sizeof(double));
    double* z = (double*)malloc0(intp1 * sizeof(double));
    double* zp = (double*)malloc0(intp1 * sizeof(double));
    int i, j, k, m;
    int dinfo;
    int* ipiv = (int*)malloc0(nsize * sizeof(int));
#else

    double* catxy
        = (double*)PushArray(MemArena, 2 * points * sizeof(double), double);
    double* sx = (double*)PushArray(MemArena, points * sizeof(double), double);
    double* sy = (double*)PushArray(MemArena, points * sizeof(double), double);
    double* h = (double*)PushArray(MemArena, ints * sizeof(double), double);
    int* p = (int*)PushArray(MemArena, ints * sizeof(int), int);
    int* np = (int*)PushArray(MemArena, ints * sizeof(int), int);
    double u, v, alpha, beta, gamma, delta;
    double* taa = (double*)PushArray(MemArena, ints * sizeof(double), double);
    double* tab = (double*)PushArray(MemArena, ints * sizeof(double), double);
    double* tag = (double*)PushArray(MemArena, ints * sizeof(double), double);
    double* tad = (double*)PushArray(MemArena, ints * sizeof(double), double);
    double* tbb = (double*)PushArray(MemArena, ints * sizeof(double), double);
    double* tbg = (double*)PushArray(MemArena, ints * sizeof(double), double);
    double* tbd = (double*)PushArray(MemArena, ints * sizeof(double), double);
    double* tgg = (double*)PushArray(MemArena, ints * sizeof(double), double);
    double* tgd = (double*)PushArray(MemArena, ints * sizeof(double), double);
    double* tdd = (double*)PushArray(MemArena, ints * sizeof(double), double);
    int nsize = 3 * ints + 1;
    int intp1 = ints + 1;
    int intm1 = ints - 1;
    double* A
        = (double*)PushArray(MemArena, intp1 * intp1 * sizeof(double), double);
    double* B
        = (double*)PushArray(MemArena, intp1 * intp1 * sizeof(double), double);
    double* C
        = (double*)PushArray(MemArena, intm1 * intp1 * sizeof(double), double);
    double* D = (double*)PushArray(MemArena, intp1 * sizeof(double), double);
    double* E
        = (double*)PushArray(MemArena, intp1 * intp1 * sizeof(double), double);
    double* F
        = (double*)PushArray(MemArena, intm1 * intp1 * sizeof(double), double);
    double* G = (double*)PushArray(MemArena, intp1 * sizeof(double), double);
    double* MAT
        = (double*)PushArray(MemArena, nsize * nsize * sizeof(double), double);
    double* RHS = (double*)PushArray(MemArena, nsize * sizeof(double), double);
    double* SLN = (double*)PushArray(MemArena, nsize * sizeof(double), double);
    double* z = (double*)PushArray(MemArena, intp1 * sizeof(double), double);
    double* zp = (double*)PushArray(MemArena, intp1 * sizeof(double), double);
    int i, j, k, m;
    int dinfo;
    int* ipiv = (int*)PushArray(MemArena, nsize * sizeof(int), int);

#endif

    for (i = 0; i < points; i++) {
        catxy[2 * i + 0] = x[i];
        catxy[2 * i + 1] = y[i];
    }
    qsort(catxy, points, 2 * sizeof(double), fcompare);
    for (i = 0; i < points; i++) {
        sx[i] = catxy[2 * i + 0];
        sy[i] = catxy[2 * i + 1];
    }
    cull(&points, ints, sx, t, ptol);
    if (points <= 0 || sx[points - 1] > t[ints]) {
        *info = -1000;
        goto cleanup;
    } else
        *info = 0;

    for (j = 0; j < ints; j++) h[j] = t[j + 1] - t[j];
    p[0] = 0;
    j = 0;
    for (i = 0; i < points; i++) {
        if (sx[i] <= t[j + 1])
            np[j]++;
        else {
            p[++j] = i;
            while (sx[i] > t[j + 1]) p[++j] = i;
            np[j] = 1;
        }
    }
    for (i = 0; i < ints; i++)
        for (j = p[i]; j < p[i] + np[i]; j++) {
            if (j >= points) {
                break;
            }
            u = (sx[j] - t[i]) / h[i];
            v = u - 1.0;
            alpha = (2.0 * u + 1.0) * v * v;
            beta = u * u * (1.0 - 2.0 * v);
            gamma = h[i] * u * v * v;
            delta = h[i] * u * u * v;
            taa[i] += alpha * alpha;
            tab[i] += alpha * beta;
            tag[i] += alpha * gamma;
            tad[i] += alpha * delta;
            tbb[i] += beta * beta;
            tbg[i] += beta * gamma;
            tbd[i] += beta * delta;
            tgg[i] += gamma * gamma;
            tgd[i] += gamma * delta;
            tdd[i] += delta * delta;
            D[i + 0] += 2.0 * sy[j] * alpha;
            D[i + 1] += 2.0 * sy[j] * beta;
            G[i + 0] += 2.0 * sy[j] * gamma;
            G[i + 1] += 2.0 * sy[j] * delta;
        }
    for (i = 0; i < ints; i++) {
        A[(i + 0) * intp1 + (i + 0)] += 2.0 * taa[i];
        A[(i + 1) * intp1 + (i + 1)] = 2.0 * tbb[i];
        A[(i + 0) * intp1 + (i + 1)] = 2.0 * tab[i];
        A[(i + 1) * intp1 + (i + 0)] = 2.0 * tab[i];
        B[(i + 0) * intp1 + (i + 0)] += 2.0 * tag[i];
        B[(i + 1) * intp1 + (i + 1)] = 2.0 * tbd[i];
        B[(i + 0) * intp1 + (i + 1)] = 2.0 * tbg[i];
        B[(i + 1) * intp1 + (i + 0)] = 2.0 * tad[i];
        E[(i + 0) * intp1 + (i + 0)] += 2.0 * tgg[i];
        E[(i + 1) * intp1 + (i + 1)] = 2.0 * tdd[i];
        E[(i + 0) * intp1 + (i + 1)] = 2.0 * tgd[i];
        E[(i + 1) * intp1 + (i + 0)] = 2.0 * tgd[i];
    }
    for (i = 0; i < intm1; i++) {
        C[i * intp1 + (i + 0)] = +3.0 * h[i + 1] / h[i];
        C[i * intp1 + (i + 2)] = -3.0 * h[i] / h[i + 1];
        C[i * intp1 + (i + 1)]
            = -C[i * intp1 + (i + 0)] - C[i * intp1 + (i + 2)];
        F[i * intp1 + (i + 0)] = h[i + 1];
        F[i * intp1 + (i + 1)] = 2.0 * (h[i] + h[i + 1]);
        F[i * intp1 + (i + 2)] = h[i];
    }
    for (i = 0, k = 0; i < intp1; i++, k++) {
        for (j = 0, m = 0; j < intp1; j++, m++)
            MAT[k * nsize + m] = A[i * intp1 + j];
        for (j = 0, m = intp1; j < intp1; j++, m++)
            MAT[k * nsize + m] = B[j * intp1 + i];
        for (j = 0, m = 2 * intp1; j < intm1; j++, m++)
            MAT[k * nsize + m] = C[j * intp1 + i];
        RHS[k] = D[i];
    }
    for (i = 0, k = intp1; i < intp1; i++, k++) {
        for (j = 0, m = 0; j < intp1; j++, m++)
            MAT[k * nsize + m] = B[i * intp1 + j];
        for (j = 0, m = intp1; j < intp1; j++, m++)
            MAT[k * nsize + m] = E[i * intp1 + j];
        for (j = 0, m = 2 * intp1; j < intm1; j++, m++)
            MAT[k * nsize + m] = F[j * intp1 + i];
        RHS[k] = G[i];
    }
    for (i = 0, k = 2 * intp1; i < intm1; i++, k++) {
        for (j = 0, m = 0; j < intp1; j++, m++)
            MAT[k * nsize + m] = C[i * intp1 + j];
        for (j = 0, m = intp1; j < intp1; j++, m++)
            MAT[k * nsize + m] = F[i * intp1 + j];
        for (j = 0, m = 2 * intp1; j < intm1; j++, m++)
            MAT[k * nsize + m] = 0.0;
        RHS[k] = 0.0;
    }
    decomp(nsize, MAT, ipiv, &dinfo);
    dsolve(nsize, MAT, ipiv, RHS, SLN);
    if (dinfo != 0) {
        *info = dinfo;
        goto cleanup;
    }

    for (i = 0; i <= ints; i++) {
        z[i] = SLN[i];
        zp[i] = SLN[i + ints + 1];
    }
    for (i = 0; i < ints; i++) {
        c[4 * i + 0] = z[i];
        c[4 * i + 1] = zp[i];
        c[4 * i + 2] = -3.0 / (h[i] * h[i]) * (z[i] - z[i + 1])
            - 1.0 / h[i] * (2.0 * zp[i] + zp[i + 1]);
        c[4 * i + 3] = 2.0 / (h[i] * h[i] * h[i]) * (z[i] - z[i + 1])
            + 1.0 / (h[i] * h[i]) * (zp[i] + zp[i + 1]);
    }
cleanup:
    StorageReset(MemArena, 1);
    busy = 0;
}

void scheck(CALCC a) {
    int i, j, k;
    double v, dx, out, x, xold;
    int intm1 = a->ints - 1;
    const double diff_thresh = 0.05;
    a->binfo[6] = 0x0000;

    for (i = 0; i < 4 * a->ints; i++) {
        if (isnan(a->cm[i])) a->binfo[6] |= 0x0001;
        if (isnan(a->cc[i])) a->binfo[6] |= 0x0001;
        if (isnan(a->cs[i])) a->binfo[6] |= 0x0001;
    }

    for (i = 0; i < a->ints; i++)
        if ((a->cm[4 * i + 0] == 0.0) && (a->cm[4 * i + 1] == 0.0)
            && (a->cm[4 * i + 2] == 0.0) && (a->cm[4 * i + 3] == 0.0))
            a->binfo[6] |= 0x0002;

    for (i = 0; i < a->ints; i++) {
        for (j = 0; j < 4; j++) {
            k = 4 * i + j;
            v = (double)k / (4.0 * (double)a->ints);
            dx = (a->t[i + 1] - a->t[i]) * (double)j / 4.0;
            out = v
                * (a->cm[4 * i + 0]
                    + dx
                        * (a->cm[4 * i + 1]
                            + dx * (a->cm[4 * i + 2] + dx * a->cm[4 * i + 3])));
            if (out > 1.0) a->binfo[6] |= 0x0004;
            if (out < 0.0) a->binfo[6] |= 0x0010;
        }
    }

    dx = a->t[a->ints] - a->t[intm1];
    x = a->cm[4 * intm1 + 0]
        + dx
            * (a->cm[4 * intm1 + 1]
                + dx * (a->cm[4 * intm1 + 2] + dx * a->cm[4 * intm1 + 3]));

    if (x > 1.07) // VALUE
        a->binfo[6] |= 0x0008;
    if (x < 0.0) a->binfo[6] |= 0x0020;

    for (i = 4; i < a->ints; i++)
        if (fabs(a->cm[4 * i + 0] - a->cm_old[4 * i + 0]) > diff_thresh)
            a->binfo[6] |= 0x0040;
    xold = a->cm_old[4 * intm1 + 0]
        + dx
            * (a->cm_old[4 * intm1 + 1]
                + dx
                    * (a->cm_old[4 * intm1 + 2]
                        + dx * a->cm_old[4 * intm1 + 3]));
    if (fabs(x - xold) > diff_thresh) a->binfo[6] |= 0x0040;
    memcpy(a->cm_old, a->cm, a->ints * 4 * sizeof(double));
}

void rxscheck(int rints, double* tvec, double* coef, int* info) {
    int i, j, k;
    int rintsm1 = rints - 1;
    double v, dx, out;
    *info = 0x0000;
    for (i = 0; i < 4 * rints; i++)
        if (isnan(coef[i])) *info |= 0x0001;
    for (i = 0; i < rints; i++)
        if ((coef[4 * i + 0] == 0.0) && (coef[4 * i + 1] == 0.0)
            && (coef[4 * i + 2] == 0.0) && (coef[4 * i + 3] == 0.0))
            *info |= 0x0002;
    for (i = 0; i < rints; i++) {
        for (j = 0; j < 4; j++) {
            k = 4 * i + j;
            v = (double)k / (4.0 * (double)rints);
            dx = (tvec[i + 1] - tvec[i]) * (double)j / 4.0;
            out = v
                * (coef[4 * i + 0]
                    + dx
                        * (coef[4 * i + 1]
                            + dx * (coef[4 * i + 2] + dx * coef[4 * i + 3])));
            if (out > 1.0) // potentially use hw_scale here
                *info |= 0x0004;
            if (out < 0.0) *info |= 0x0010;
        }
    }
    dx = tvec[rints] - tvec[rints - 1];
    out = coef[4 * rintsm1 + 0]
        + dx
            * (coef[4 * rintsm1 + 1]
                + dx * (coef[4 * rintsm1 + 2] + dx * coef[4 * rintsm1 + 3]));
    if (out > 1.07) *info |= 0x0008;
    if (out < 0.00) *info |= 0x0020;
}

typedef struct calc_struct_klj {

    double* env_TX;
    double* env_RX;
    double* x;
    double* ym;
    double* yc;
    double* ys;
    double* cat;

    int nsamps;
    int tsamps;

    int nsamps_overalloced;
    int tsamps_overalloced;
} CALC_STRUCT, *PCALC_STRUCT;

static CALC_STRUCT static_calc_struct_klj;

void calc_destroy(PCALC_STRUCT p) {
    assert(p);
    _aligned_free(p->x);
    _aligned_free(p->ym);
    _aligned_free(p->yc);
    _aligned_free(p->ys);

    _aligned_free(p->env_TX);
    _aligned_free(p->env_RX);
    _aligned_free(p->cat);
}

volatile int calc_create_busy = 0;

PCALC_STRUCT calc_create(int tsamps, int nsamps) {

    int create = 0;
    int nsamps_more = nsamps * 4;
    int tsamps_more = tsamps * 4;

    assert(!calc_create_busy); // check it's actually OK to create the calc
                               // statically.
    calc_create_busy = 1;

    if (static_calc_struct_klj.tsamps_overalloced < nsamps
        || static_calc_struct_klj.nsamps_overalloced < tsamps) {
        create = 1;
    }

    if (create) {
        memset(&static_calc_struct_klj, 0, sizeof(CALC_STRUCT));
    }

    PCALC_STRUCT c = &static_calc_struct_klj;

    if (create) {
        c->env_TX = (double*)malloc0(nsamps_more * sizeof(double));
        c->env_RX = (double*)malloc0(nsamps_more * sizeof(double));
        c->x = (double*)malloc0(tsamps_more * sizeof(double));
        c->ym = (double*)malloc0(tsamps_more * sizeof(double));
        c->yc = (double*)malloc0(tsamps_more * sizeof(double));
        c->ys = (double*)malloc0(tsamps_more * sizeof(double));
        c->nsamps = nsamps;
        c->tsamps = tsamps;
        c->nsamps_overalloced = nsamps_more;
        c->tsamps_overalloced = tsamps_more;
        c->cat = (double*)malloc0(4 * nsamps * sizeof(double) * 10);
    }

    calc_create_busy = 0;
    return &static_calc_struct_klj;
}
volatile int calc_busy = 0;
void calc(CALCC a) {

    assert(!calc_busy);
    calc_busy = 1;
    int i;
    int tsamps = a->nsamps + a->npsamps;
    PCALC_STRUCT pcs = calc_create(tsamps, a->nsamps);

    double* env_TX = pcs->env_TX;
    double* env_RX = pcs->env_RX;
    double* x = pcs->x;
    double* ym = pcs->ym;
    double* yc = pcs->yc;
    double* ys = pcs->ys;
    double norm;

    for (i = 0; i < a->nsamps; i++) {
        env_TX[i] = sqrt(a->txs[2 * i + 0] * a->txs[2 * i + 0]
            + a->txs[2 * i + 1] * a->txs[2 * i + 1]);
        env_RX[i] = sqrt(a->rxs[2 * i + 0] * a->rxs[2 * i + 0]
            + a->rxs[2 * i + 1] * a->rxs[2 * i + 1]);
    }
    {
        int rints, ix;
        double dx;
        double tvec[3];
        double txrxcoefs[4 * 2];
        double rx_scale;
        if (a->ints < 16)
            rints = 1;
        else
            rints = 2;
        ix = rints - 1;
        for (i = 0; i <= rints; i++)
            tvec[i] = (double)i / (double)rints / a->hw_scale;
        dx = tvec[rints] - tvec[rints - 1];
        builder(a->nsamps, env_TX, env_RX, rints, tvec, &(a->binfo[0]),
            txrxcoefs, a->ptol);
        rxscheck(rints, tvec, txrxcoefs, &a->binfo[7]);
        if ((a->binfo[0] == 0) && (a->binfo[7] == 0))
            rx_scale = 1.0
                / (txrxcoefs[4 * ix + 0]
                    + dx
                        * (txrxcoefs[4 * ix + 1]
                            + dx
                                * (txrxcoefs[4 * ix + 2]
                                    + dx * txrxcoefs[4 * ix + 3])));
        else {
            a->scOK = 0;
            goto cleanup;
        }
        if (a->stbl && _InterlockedAnd(&a->ctrl.running, 1))
            a->rx_scale = a->alpha * a->rx_scale + (1.0 - a->alpha) * rx_scale;
        else
            a->rx_scale = rx_scale;
    }

    a->binfo[4] = (int)(256.0 * (a->hw_scale / a->rx_scale));
    a->binfo[5]++;

    if (a->pin) // regress
    {
        const double slope = 0.001;
        double max_rx;
        for (i = 0; i < a->nsamps; i++) {
            max_rx
                = (1.0 - slope + slope * a->hw_scale * env_TX[i]) / a->rx_scale;
            if (env_RX[i] > max_rx) env_RX[i] = max_rx;
        }
    }

    for (i = 0; i < a->nsamps; i++) {
        norm = env_TX[i] * env_RX[i];
        x[i] = a->rx_scale * env_RX[i];
        ym[i] = (a->hw_scale * env_TX[i]) / (a->rx_scale * env_RX[i]);
        yc[i] = (+a->txs[2 * i + 0] * a->rxs[2 * i + 0]
                    + a->txs[2 * i + 1] * a->rxs[2 * i + 1])
            / norm;
        ys[i] = (-a->txs[2 * i + 0] * a->rxs[2 * i + 1]
                    + a->txs[2 * i + 1] * a->rxs[2 * i + 0])
            / norm;
        if (a->stbl && _InterlockedAnd(&a->ctrl.running, 1) && a->scOK) {
            int k;
            double dx, ymo, yco, yso;
            if ((k = (int)(x[i] * a->ints)) > a->ints - 1) k = a->ints - 1;
            dx = x[i] - a->t[k];
            ymo = a->cm[4 * k + 0]
                + dx
                    * (a->cm[4 * k + 1]
                        + dx * (a->cm[4 * k + 2] + dx * a->cm[4 * k + 3]));
            yco = a->cc[4 * k + 0]
                + dx
                    * (a->cc[4 * k + 1]
                        + dx * (a->cc[4 * k + 2] + dx * a->cc[4 * k + 3]));
            yso = a->cs[4 * k + 0]
                + dx
                    * (a->cs[4 * k + 1]
                        + dx * (a->cs[4 * k + 2] + dx * a->cs[4 * k + 3]));
            ym[i] = a->alpha * ymo + (1.0 - a->alpha) * ym[i];
            yc[i] = a->alpha * yco + (1.0 - a->alpha) * yc[i];
            ys[i] = a->alpha * yso + (1.0 - a->alpha) * ys[i];
        }
    }

    if (a->pin) // pin
    {
        const double mval = 1.0e+00 - 1.0e-10;
        double cval, sval;
        double* cat = pcs->cat; // (double*)malloc0(4 * a->nsamps *
                                // sizeof(double)); //KLJ
        for (i = 0; i < a->nsamps; i++) {
            cat[4 * i + 0] = x[i];
            cat[4 * i + 1] = ym[i];
            cat[4 * i + 2] = yc[i];
            cat[4 * i + 3] = ys[i];
        }
        qsort(cat, a->nsamps, 4 * sizeof(double), fcompare);
        for (i = 0; i < a->nsamps; i++) {
            x[i] = cat[4 * i + 0];
            ym[i] = cat[4 * i + 1];
            yc[i] = cat[4 * i + 2];
            ys[i] = cat[4 * i + 3];
        }
        // _aligned_free(cat); KLJ
        cval = 0.0;
        sval = 0.0;
        for (i = a->nsamps - 1; i > a->nsamps - 17; i--) {
            cval += yc[i];
            sval += ys[i];
        }
        cval /= 16.0;
        sval /= 16.0;
        for (i = a->nsamps; i < tsamps; i++) {
            x[i] = mval;
            ym[i] = mval;
            yc[i] = cval;
            ys[i] = sval;
        }
        builder(tsamps, x, ym, a->ints, a->t, &(a->binfo[1]), a->cm, a->ptol);
        builder(tsamps, x, yc, a->ints, a->t, &(a->binfo[2]), a->cc, a->ptol);
        builder(tsamps, x, ys, a->ints, a->t, &(a->binfo[3]), a->cs, a->ptol);
    } else {
        builder(
            a->nsamps, x, ym, a->ints, a->t, &(a->binfo[1]), a->cm, a->ptol);
        builder(
            a->nsamps, x, yc, a->ints, a->t, &(a->binfo[2]), a->cc, a->ptol);
        builder(
            a->nsamps, x, ys, a->ints, a->t, &(a->binfo[3]), a->cs, a->ptol);
    }

    if (a->pin) // tune
    {
        int k = a->ints - 1;
        double dx = a->t[a->ints] - a->t[a->ints - 1];
        double sf = 1.0
            / (a->cm[4 * k + 0]
                + dx
                    * (a->cm[4 * k + 1]
                        + dx * (a->cm[4 * k + 2] + dx * a->cm[4 * k + 3])));
        for (i = 0; i < 4 * a->ints; i++) a->cm[i] *= sf;
    }

    scheck(a);
    a->scOK = ((a->binfo[0] == 0) && (a->binfo[1] == 0) && (a->binfo[2] == 0)
        && (a->binfo[3] == 0) && (a->binfo[6] == 0));

    if (a->scOK) // map calc
    {
        for (i = 0; i < a->ints; i++) a->tmap[i] = a->cm[4 * i] * a->t[i];
        a->tmap[a->ints] = 1.0;
        a->convex = ((a->tmap[a->ints] - a->tmap[a->ints - 1])
            > (a->t[a->ints] - a->t[a->ints - 1]));
    }

    EnterCriticalSection(&a->disp.cs_disp);
    memcpy(a->disp.x, x, a->nsamps * sizeof(double));
    memcpy(a->disp.ym, ym, a->nsamps * sizeof(double));
    memcpy(a->disp.yc, yc, a->nsamps * sizeof(double));
    memcpy(a->disp.ys, ys, a->nsamps * sizeof(double));
    if (a->scOK) {
        memcpy(a->disp.cm, a->cm, a->ints * 4 * sizeof(double));
        memcpy(a->disp.cc, a->cc, a->ints * 4 * sizeof(double));
        memcpy(a->disp.cs, a->cs, a->ints * 4 * sizeof(double));
    } else {
        memset(a->disp.cm, 0, a->ints * 4 * sizeof(double));
        memset(a->disp.cc, 0, a->ints * 4 * sizeof(double));
        memset(a->disp.cs, 0, a->ints * 4 * sizeof(double));
    }
    LeaveCriticalSection(&a->disp.cs_disp);
cleanup:
    calc_busy = 0;
    /*/
    _aligned_free(x);
    _aligned_free(ym);
    _aligned_free(yc);
    _aligned_free(ys);

    _aligned_free(env_TX);
    _aligned_free(env_RX);
    /*/
}

volatile int wdsp_running = 1;
CALCC calcCCForTurnOff = 0;
CALCC calcCCForCorrection = 0;

HANDLE doCalcCorrectionEvent = 0;
HANDLE doTurnOffEvent = 0;

HANDLE doTurnOffThread = 0;
HANDLE doCalcCorrectionThread = 0;

DWORD WINAPI doCalcCorrection(void* arg) {

    while (wdsp_running) {
        DWORD dwWait = WaitForSingleObject(doCalcCorrectionEvent, INFINITE);
        CALCC a = calcCCForCorrection;
        if (!a) {
            return 0;
        }
        calc(a); // is this supposed to be outside the critical section? KLJ
        if (a->scOK) {
            if (!InterlockedBitTestAndSet(&a->ctrl.running, 0))
                SetTXAiqcStart(a->channel, a->cm, a->cc, a->cs);
            else
                SetTXAiqcSwap(a->channel, a->cm, a->cc, a->cs);
        }
        InterlockedBitTestAndSet(&a->ctrl.calcdone, 0);
    }
    return 0;
    /*/
    CALCC a = (CALCC)arg;
    calc(a);
    if (a->scOK) {
        if (!InterlockedBitTestAndSet(&a->ctrl.running, 0))
            SetTXAiqcStart(a->channel, a->cm, a->cc, a->cs);
        else
            SetTXAiqcSwap(a->channel, a->cm, a->cc, a->cs);
    }
    InterlockedBitTestAndSet(&a->ctrl.calcdone, 0);
    /*/
}

DWORD WINAPI doTurnoff(void* arg) {

    while (wdsp_running) {
        DWORD dwWait = WaitForSingleObject(doTurnOffEvent, INFINITE);
        CALCC a = calcCCForTurnOff;
        if (!a) return 0;
        assert(a);
        SetTXAiqcEnd(a->channel);
    }

    return 0;
}

enum _calcc_state {
    LRESET,
    LWAIT,
    LMOXDELAY,
    LSETUP,
    LCOLLECT,
    MOXCHECK,
    LCALC,
    LDELAY,
    LSTAYON,
    LTURNON
};

void __cdecl SaveCorrection(void* pargs) {
    int i, k;
    CALCC a = (CALCC)pargs;
    double* pm = (double*)malloc0(4 * a->util.ints * sizeof(double));
    double* pc = (double*)malloc0(4 * a->util.ints * sizeof(double));
    double* ps = (double*)malloc0(4 * a->util.ints * sizeof(double));
    FILE* file = fopen(a->util.savefile, "w");
    GetTXAiqcValues(a->util.channel, pm, pc, ps);
    for (i = 0; i < a->util.ints; i++) {
        for (k = 0; k < 4; k++) fprintf(file, "%.17e\t", pm[4 * i + k]);
        fprintf(file, "\n");
        for (k = 0; k < 4; k++) fprintf(file, "%.17e\t", pc[4 * i + k]);
        fprintf(file, "\n");
        for (k = 0; k < 4; k++) fprintf(file, "%.17e\t", ps[4 * i + k]);
        fprintf(file, "\n\n");
    }
    fflush(file);
    fclose(file);
    _aligned_free(ps);
    _aligned_free(pc);
    _aligned_free(pm);
    _endthread();
}

void __cdecl RestoreCorrection(void* pargs) {
    int i, k;
    CALCC a = (CALCC)pargs;
    double* pm = (double*)malloc0(4 * a->util.ints * sizeof(double));
    double* pc = (double*)malloc0(4 * a->util.ints * sizeof(double));
    double* ps = (double*)malloc0(4 * a->util.ints * sizeof(double));
    FILE* file = fopen(a->util.restfile, "r");
    for (i = 0; i < a->util.ints; i++) {
        for (k = 0; k < 4; k++) fscanf(file, "%le", &(pm[4 * i + k]));
        for (k = 0; k < 4; k++) fscanf(file, "%le", &(pc[4 * i + k]));
        for (k = 0; k < 4; k++) fscanf(file, "%le", &(ps[4 * i + k]));
    }
    fclose(file);
    if (!InterlockedBitTestAndSet(&a->ctrl.running, 0))
        SetTXAiqcStart(a->channel, pm, pc, ps);
    else
        SetTXAiqcSwap(a->channel, pm, pc, ps);
    _aligned_free(ps);
    _aligned_free(pc);
    _aligned_free(pm);
    _endthread();
}

/********************************************************************************************************
 *																										*
 *										  Public Functions
 **
 *																										*
 ********************************************************************************************************/

PORT void pscc(int channel, int size, double* tx, double* rx) {
    int i, n, m;
    double env;
    CALCC a;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    if (a->runcal) {
        a->size = size;
        if (InterlockedAnd(&a->mox, 1)
            && (a->txdelay->tdelay != 0.0 || a->rxdelay->tdelay != 0.0)) {
            SetDelayBuffs(a->rxdelay, a->size, rx, rx);
            xdelay(a->rxdelay);
            SetDelayBuffs(a->txdelay, a->size, tx, tx);
            xdelay(a->txdelay);
        }
        a->info[15] = a->ctrl.state;

        switch (a->ctrl.state) {
            case LRESET:
                InterlockedExchange(&a->ctrl.current_state, LRESET);
                a->ctrl.reset = 0;
                if (!a->ctrl.turnon)
                    if (InterlockedBitTestAndReset(&a->ctrl.running, 0)) {

                        if (doTurnOffEvent)
                            // thread is up, and waiting on the event
                            assert(doTurnOffEvent
                                && doTurnOffEvent != INVALID_HANDLE_VALUE);
                        if (doTurnOffEvent) {
                            calcCCForTurnOff = a;
                            SetEvent(doTurnOffEvent);
                        }

                        // _beginthread(doTurnoff, 0, (void*)a);
                    }
                a->info[14] = 0;
                a->ctrl.env_maxtx = 0.0;
                a->ctrl.bs_count = 0;
                if (a->ctrl.turnon)
                    a->ctrl.state = LTURNON;
                else if (a->ctrl.automode || a->ctrl.mancal)
                    a->ctrl.state = LWAIT;
                break;
            case LWAIT:
                InterlockedExchange(&a->ctrl.current_state, LWAIT);
                a->ctrl.mancal = 0;
                a->ctrl.moxcount = 0;
                if (a->ctrl.reset)
                    a->ctrl.state = LRESET;
                else if (a->ctrl.turnon)
                    a->ctrl.state = LTURNON;
                else if (InterlockedAnd(&a->mox, 1)) {
                    a->ctrl.state = LMOXDELAY;
                    InterlockedBitTestAndSet(&a->solidmox, 0);
                }
                break;
            case LMOXDELAY:
                InterlockedExchange(&a->ctrl.current_state, LMOXDELAY);
                a->ctrl.moxcount += a->size;
                if (a->ctrl.reset)
                    a->ctrl.state = LRESET;
                else if (a->ctrl.turnon)
                    a->ctrl.state = LTURNON;
                else if (!InterlockedAnd(&a->mox, 1)
                    || !InterlockedAnd(&a->solidmox, 1))
                    a->ctrl.state = LWAIT;
                else if ((a->ctrl.moxcount - a->size) >= a->ctrl.moxsamps)
                    a->ctrl.state = LSETUP;
                break;
            case LSETUP:
                InterlockedExchange(&a->ctrl.current_state, LSETUP);
                a->ctrl.count = 0;
                for (i = 0; i < a->ints; i++) {
                    a->ctrl.cpi[i] = 0;
                    a->ctrl.sindex[i] = 0;
                }
                a->ctrl.full_ints = 0;
                a->ctrl.waitcount = 0;

                if (a->ctrl.reset)
                    a->ctrl.state = LRESET;
                else if (a->ctrl.turnon)
                    a->ctrl.state = LTURNON;
                else if (InterlockedAnd(&a->mox, 1)
                    && InterlockedAnd(&a->solidmox, 1)) {
                    a->ctrl.state = LCOLLECT;
                    SetTXAiqcDogCount(channel, a->info[13] = 0);
                } else
                    a->ctrl.state = LWAIT;
                break;
            case LCOLLECT:
                InterlockedExchange(&a->ctrl.current_state, LCOLLECT);
                for (i = 0; i < a->size; i++) {
                    env = sqrt(tx[2 * i + 0] * tx[2 * i + 0]
                        + tx[2 * i + 1] * tx[2 * i + 1]);
                    if (env > a->ctrl.env_maxtx) a->ctrl.env_maxtx = env;
                    if ((env *= a->hw_scale) <= 1.0) {
                        if (env == 1.0)
                            n = a->ints - 1;
                        else if (a->map && _InterlockedAnd(&a->ctrl.running, 1)
                            && a->convex) {
                            int nmin = 0;
                            int nmax = a->ints;

                            while (nmax - nmin > 1) {
                                n = (nmin + nmax) / 2;
                                if (env >= a->tmap[n])
                                    nmin = n;
                                else
                                    nmax = n;
                            }
                            if (env < a->tmap[n]) n--;
                        } else
                            n = (int)(env * (double)a->ints);
                        m = a->ctrl.sbase[n] + a->ctrl.sindex[n];
                        a->txs[2 * m + 0] = tx[2 * i + 0];
                        a->txs[2 * m + 1] = tx[2 * i + 1];
                        a->rxs[2 * m + 0] = rx[2 * i + 0];
                        a->rxs[2 * m + 1] = rx[2 * i + 1];
                        if (++a->ctrl.sindex[n] == a->spi)
                            a->ctrl.sindex[n] = 0;
                        if (a->ctrl.cpi[n] != a->spi)
                            if (++a->ctrl.cpi[n] == a->spi) a->ctrl.full_ints++;
                        ++a->ctrl.count;
                    }
                }
                GetTXAiqcDogCount(channel, &a->info[13]);
                if (a->ctrl.reset)
                    a->ctrl.state = LRESET;
                else if (a->ctrl.turnon)
                    a->ctrl.state = LTURNON;
                else if (!InterlockedAnd(&a->mox, 1)
                    || !InterlockedAnd(&a->solidmox, 1))
                    a->ctrl.state = LWAIT;
                else if (a->ctrl.full_ints == a->ints)
                    a->ctrl.state = MOXCHECK;
                else if (a->info[13] >= 6)
                    a->ctrl.state = LRESET;
                else if (a->ctrl.count >= 4 * a->rate) {
                    a->ctrl.count = 0;
                    for (i = 0; i < a->ints; i++) {
                        a->ctrl.cpi[i] = 0;
                        a->ctrl.sindex[i] = 0;
                    }
                    a->ctrl.full_ints = 0;
                }
                break;
            case MOXCHECK:
                InterlockedExchange(&a->ctrl.current_state, MOXCHECK);
                if (a->ctrl.reset)
                    a->ctrl.state = LRESET;
                else if (a->ctrl.turnon)
                    a->ctrl.state = LTURNON;
                else if (!InterlockedAnd(&a->mox, 1)
                    || !InterlockedAnd(&a->solidmox, 1))
                    a->ctrl.state = LWAIT;
                else
                    a->ctrl.state = LCALC;
                break;
            case LCALC:
                InterlockedExchange(&a->ctrl.current_state, LCALC);
                if (!a->ctrl.calcinprogress) {
                    a->ctrl.calcinprogress = 1;
                    if (doCalcCorrectionEvent) {
                        // thread running, just signal it to do work:
                        calcCCForCorrection = a;
                        SetEvent(doCalcCorrectionEvent);
                    }

                    // _beginthread(doCalcCorrection, 0, (void*)a);
                }

                if (InterlockedBitTestAndReset(&a->ctrl.calcdone, 0)) {
                    memcpy(a->info, a->binfo, 8 * sizeof(int));
                    a->info[14] = _InterlockedAnd(&a->ctrl.running, 1);
                    a->ctrl.calcinprogress = 0;
                    if (a->ctrl.reset)
                        a->ctrl.state = LRESET;
                    else if (a->ctrl.turnon)
                        a->ctrl.state = LTURNON;
                    else if (a->scOK) {
                        a->ctrl.bs_count = 0;
                        a->ctrl.state = LDELAY;
                    } else if (++(a->ctrl.bs_count) >= 2)
                        a->ctrl.state = LRESET;
                    else if (InterlockedAnd(&a->mox, 1)
                        && InterlockedAnd(&a->solidmox, 1))
                        a->ctrl.state = LSETUP;
                    else
                        a->ctrl.state = LWAIT;
                }
                break;
            case LDELAY:
                InterlockedExchange(&a->ctrl.current_state, LDELAY);
                a->ctrl.waitcount += a->size;
                if (a->ctrl.reset)
                    a->ctrl.state = LRESET;
                else if (a->ctrl.turnon)
                    a->ctrl.state = LTURNON;
                else if ((a->ctrl.waitcount - a->size) >= a->ctrl.waitsamps) {
                    if (a->ctrl.automode) {
                        if (InterlockedAnd(&a->mox, 1)
                            && InterlockedAnd(&a->solidmox, 1))
                            a->ctrl.state = LSETUP;
                        else
                            a->ctrl.state = LWAIT;
                    } else
                        a->ctrl.state = LSTAYON;
                }
                break;
            case LSTAYON:
                InterlockedExchange(&a->ctrl.current_state, LSTAYON);
                if (a->ctrl.reset || a->ctrl.automode || a->ctrl.mancal)
                    a->ctrl.state = LRESET;
                break;
            case LTURNON:
                InterlockedExchange(&a->ctrl.current_state, LTURNON);
                a->ctrl.turnon = 0;
                a->ctrl.automode = 0;
                a->info[14] = 1;
                a->ctrl.state = LSTAYON;
                break;
        }
    }
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void psccF(int channel, int size, float* Itxbuff, float* Qtxbuff,
    float* Irxbuff, float* Qrxbuff, int mox, int solidmox) {
    int i;
    CALCC a;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    // a->mox = mox;
    // a->solidmox = solidmox;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
    for (i = 0; i < size; i++) {
        a->temptx[2 * i + 0] = (double)Itxbuff[i];
        a->temptx[2 * i + 1] = (double)Qtxbuff[i];
        a->temprx[2 * i + 0] = (double)Irxbuff[i];
        a->temprx[2 * i + 1] = (double)Qrxbuff[i];
    }
    pscc(channel, size, a->temptx, a->temprx);
}

PORT void PSSaveCorr(int channel, char* filename) {
    CALCC a;
    int i = 0;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    while (a->util.savefile[i++] = *filename++)
        ;
    _beginthread(SaveCorrection, 0, (void*)a);
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void PSRestoreCorr(int channel, char* filename) {
    CALCC a;
    int i = 0;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    while (a->util.restfile[i++] = *filename++)
        ;
    a->ctrl.turnon = 1;
    _beginthread(RestoreCorrection, 0, (void*)a);
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

// thread creation for PS
void MakeThreadsKLJ() {

    if (!doTurnOffEvent) {

        doTurnOffEvent = CreateEvent(0, 0, 0, 0);
        assert(doTurnOffEvent);
        doTurnOffThread = CreateThread(0, 0, doTurnoff, 0, 0, 0);
        assert(doTurnOffThread && doTurnOffThread != INVALID_HANDLE_VALUE);
    }

    if (!doCalcCorrectionEvent) {

        doCalcCorrectionEvent = CreateEvent(0, 0, 0, 0);
        assert(doCalcCorrectionEvent);
        if (doCalcCorrectionEvent) {
            doCalcCorrectionThread
                = CreateThread(0, 0, doCalcCorrection, 0, 0, 0);
            assert(doCalcCorrectionEvent != INVALID_HANDLE_VALUE);
        }
    }
}

/********************************************************************************************************
 *																										*
 *											  Properties
 **
 *																										*
 ********************************************************************************************************/

PORT void SetPSRunCal(int channel, int run) {

    if (run) {
        MakeThreadsKLJ();
    }
    CALCC a;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    a->runcal = run;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSMox(int channel, int mox) {
    CALCC a = txa[channel].calcc.p;
    ;
    if (mox)
        InterlockedBitTestAndSet(&a->mox, 0);
    else {
        InterlockedBitTestAndReset(&a->mox, 0);
        InterlockedBitTestAndReset(&a->solidmox, 0);
    }
}

PORT void GetPSInfo(int channel, int* info) {
    CALCC a;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    memcpy(info, a->info, 16 * sizeof(int));
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSReset(int channel, int reset) {
    CALCC a;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    a->ctrl.reset = reset;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSMancal(int channel, int mancal) {
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    txa[channel].calcc.p->ctrl.mancal = mancal;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSAutomode(int channel, int automode) {
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    txa[channel].calcc.p->ctrl.automode = automode;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSTurnon(int channel, int turnon) {
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    txa[channel].calcc.p->ctrl.turnon = turnon;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSControl(
    int channel, int reset, int mancal, int automode, int turnon) {
    CALCC a;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    a->ctrl.reset = reset;
    a->ctrl.mancal = mancal;
    a->ctrl.automode = automode;
    a->ctrl.turnon = turnon;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSLoopDelay(int channel, double delay) {
    CALCC a;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    a->ctrl.loopdelay = delay;
    a->ctrl.waitsamps = (int)(a->rate * a->ctrl.loopdelay);
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSMoxDelay(int channel, double delay) {
    CALCC a;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    a->ctrl.moxdelay = delay;
    a->ctrl.moxsamps = (int)(a->rate * a->ctrl.moxdelay);
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT double SetPSTXDelay(int channel, double delay) {
    CALCC a;
    double adelay;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    a->txdel = delay;
    if (a->txdel >= 0.0) {
        adelay = SetDelayValue(a->txdelay, a->txdel);
        SetDelayValue(a->rxdelay, 0.0);
    } else {
        adelay = -SetDelayValue(a->rxdelay, -a->txdel);
        SetDelayValue(a->txdelay, 0.0);
    }
    // adelay = SetDelayValue (a->txdelay, a->txdel);
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
    return adelay;
}

PORT void SetPSHWPeak(int channel, double peak) {
    CALCC a;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a = txa[channel].calcc.p;
    a->hw_scale = 1.0 / peak;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void GetPSHWPeak(int channel, double* peak) {
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    *peak = 1.0 / txa[channel].calcc.p->hw_scale;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void GetPSMaxTX(int channel, double* maxtx) {
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    *maxtx = txa[channel].calcc.p->ctrl.env_maxtx;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSPtol(int channel, double ptol) {
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    txa[channel].calcc.p->ptol = ptol;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void GetPSDisp(int channel, double* x, double* ym, double* yc, double* ys,
    double* cm, double* cc, double* cs) {
    CALCC a = txa[channel].calcc.p;
    EnterCriticalSection(&a->disp.cs_disp);
    memcpy(x, a->disp.x, a->nsamps * sizeof(double));
    memcpy(ym, a->disp.ym, a->nsamps * sizeof(double));
    memcpy(yc, a->disp.yc, a->nsamps * sizeof(double));
    memcpy(ys, a->disp.ys, a->nsamps * sizeof(double));
    memcpy(cm, a->disp.cm, a->ints * 4 * sizeof(double));
    memcpy(cc, a->disp.cc, a->ints * 4 * sizeof(double));
    memcpy(cs, a->disp.cs, a->ints * 4 * sizeof(double));
    LeaveCriticalSection(&a->disp.cs_disp);
}

PORT void SetPSFeedbackRate(int channel, int rate) {
    CALCC a = txa[channel].calcc.p;
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    a->rate = rate;
    a->ctrl.moxsamps = (int)(a->rate * a->ctrl.moxdelay);
    a->ctrl.waitsamps = (int)(a->rate * a->ctrl.loopdelay);
    destroy_delay(a->txdelay);
    destroy_delay(a->rxdelay);
    a->rxdelay = create_delay(1, // run
        0, // size				[stuff later]
        0, // input buffer		[stuff later]
        0, // output buffer	[stuff later]
        a->rate, // sample rate
        20.0e-09, // delta (delay stepsize)
        0.0); // delay
    a->txdelay = create_delay(1, // run
        0, // size				[stuff later]
        0, // input buffer		[stuff later]
        0, // output buffer	[stuff later]
        a->rate, // sample rate
        20.0e-09, // delta (delay stepsize)
        a->txdel); // delay
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSPinMode(int channel, int pin) {
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    txa[channel].calcc.p->pin = pin;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSMapMode(int channel, int map) {
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    txa[channel].calcc.p->map = map;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

PORT void SetPSStabilize(int channel, int stbl) {
    EnterCriticalSection(&txa[channel].calcc.cs_update);
    txa[channel].calcc.p->stbl = stbl;
    LeaveCriticalSection(&txa[channel].calcc.cs_update);
}

void ForceShutDown(CALCC a, IQC b, int timeout) {
    a->runcal = 0; // close pscc() gate
    InterlockedBitTestAndReset(&b->run, 0); // close xiqc() gate
    Sleep(timeout); // wait for anything possibly running to clear
    a->ctrl.state = LRESET; // set next_state
    InterlockedBitTestAndReset(&a->ctrl.running, 0); // set running = 0
    InterlockedBitTestAndReset(
        &b->busy, 0); // set busy = 0 so turnoff thread can finish & terminate
    InterlockedBitTestAndReset(&a->ctrl.calcdone, 0);
    a->info[14] = 0;
    a->ctrl.env_maxtx = 0.0;
    a->ctrl.bs_count = 0;
}

PORT void SetPSIntsAndSpi(int channel, int ints, int spi) {
    CALCC a = txa[channel].calcc.p;
    IQC b = txa[channel].iqc.p1;
    if (b->ints != ints || b->dog.spi != spi || a->ints != ints
        || a->spi != spi) {
        // SHUT-DOWN
        const int timeout = 50;
        int runcal = a->runcal;
        int mancal = a->ctrl.mancal;
        int automode = a->ctrl.automode;
        int turnon = a->ctrl.turnon;
        int count = 0;
        SetPSControl(a->channel, 1, 0, 0, 0);
        while (count++ < timeout
            && (LRESET != _InterlockedAnd(&a->ctrl.current_state, 0xFFFFFFFF)
                || _InterlockedAnd(&a->ctrl.running, 1)
                || _InterlockedAnd(&b->run, 1)))
            Sleep(1); // wait for normal shutdown (when samples are flowing)
        if (LRESET != _InterlockedAnd(&a->ctrl.current_state, 0xFFFFFFFF)
            || _InterlockedAnd(&a->ctrl.running, 1)
            || _InterlockedAnd(&b->run, 1))
            ForceShutDown(a, b,
                timeout); // apparently no sammples flowing; force shutdown.
        // MAKE CHANGES
        desize_iqc(b);
        desize_calcc(a);
        b->ints = ints;
        b->dog.spi = spi;
        a->ints = ints;
        a->spi = spi;
        size_calcc(a);
        size_iqc(b);
        // START-UP
        SetPSControl(a->channel, 1, mancal, automode, turnon);
        a->runcal = runcal;
    }
}