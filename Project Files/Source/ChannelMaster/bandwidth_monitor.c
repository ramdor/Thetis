/*  bandwidth_monitor.c

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2025-2026 Richard Samphire, MW0LGE

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

#include "bandwidth_monitor.h"

static volatile LONG64 in_total_bytes;
static volatile LONG64 out_total_bytes;

static volatile LONG64 in_last_bytes;
static volatile LONG64 out_last_bytes;

static volatile LONG64 in_last_ms;
static volatile LONG64 out_last_ms;

static volatile double in_last_bps;
static volatile double out_last_bps;

static int64_t now_ms(void)
{
    return (int64_t)GetTickCount64();
}

void bandwidth_monitor_reset(void)
{
    InterlockedExchange64(&in_total_bytes, 0);
    InterlockedExchange64(&out_total_bytes, 0);

    InterlockedExchange64(&in_last_bytes, 0);
    InterlockedExchange64(&out_last_bytes, 0);

    InterlockedExchange64(&in_last_ms, 0);
    InterlockedExchange64(&out_last_ms, 0);

    in_last_bps = 0.0;
    out_last_bps = 0.0;
}

void bandwidth_monitor_in(int bytes)
{
    if (bytes > 0)
        InterlockedAdd64(&in_total_bytes, (LONG64)bytes);
}

void bandwidth_monitor_out(int bytes)
{
    if (bytes > 0)
        InterlockedAdd64(&out_total_bytes, (LONG64)bytes);
}

static double compute_bps(volatile LONG64* total_bytes, volatile LONG64* last_bytes, volatile LONG64* last_ms, volatile double* last_bps)
{
    int64_t t = now_ms();
    int64_t total = InterlockedAdd64(total_bytes, 0);
    int64_t prev_bytes = InterlockedAdd64(last_bytes, 0);
    int64_t prev_ms = InterlockedAdd64(last_ms, 0);

    if (prev_ms == 0)
    {
        InterlockedExchange64(last_bytes, total);
        InterlockedExchange64(last_ms, t);
        *last_bps = 0.0;
        return 0.0;
    }

    int64_t dt = t - prev_ms;
    if (dt <= 0)
        return *last_bps;

    int64_t db = total - prev_bytes;
    double bps = (double)db * 1000.0 / (double)dt;

    InterlockedExchange64(last_bytes, total);
    InterlockedExchange64(last_ms, t);

    *last_bps = bps;
    return bps;
}

PORT double GetInboundBps(void)
{
    return compute_bps(&in_total_bytes, &in_last_bytes, &in_last_ms, &in_last_bps);
}

PORT double GetOutboundBps(void)
{
    return compute_bps(&out_total_bytes, &out_last_bytes, &out_last_ms, &out_last_bps);
}
