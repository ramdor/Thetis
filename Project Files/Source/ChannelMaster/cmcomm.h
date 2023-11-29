/*  cmcomm.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2014 Warren Pratt, NR0V

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

#include <Windows.h>
#include <process.h>
#include <intrin.h>
#include <math.h>
#include <time.h>
#include <avrt.h>

#include "aamix.h"
#include "amix.h"
#include "cmaster.h"
#include "cmbuffs.h"
#include "cmsetup.h"
#include "ilv.h"
#include "ivac.h"
#include "pipe.h"
#include "ring.h"
#include "router.h"
#include "sync.h"
#include "txgain.h"
#include "cmUtilities.h"
#include "vox.h"
#include "znob.h"
#include "znobII.h"