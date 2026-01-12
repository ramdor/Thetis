/*  cmasio.h

This file is part of a program that implements a Software-Defined Radio.

Copyright (C) 2023 Bryan Rambo W4WMT

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

bryanr@bometals.com

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

#ifndef _cmasio_h
#define _cmasio_h

void create_cmasio();
void destroy_cmasio();
long cm_asioStart(int protocol);
long cm_asioStop();
void asioIN(double* in_tx);
void asioOUT(int id, int nsamples, double* buff);

void CallbackASIO(void* inputL, void* inputR, void* outputL, void* outputR);

extern __declspec(dllimport) long getASIODriverString(void* szData);
extern __declspec(dllimport) long getASIOBlockNum(void* dwData);
extern __declspec(dllimport) int prepareASIO(int blocksize, int samplerate, char* asioDriverName, void (*CallbackASIO)(void* inputL, void* inputR, void* outputL, void* outputR), long input_base_channel, long output_base_channel);
extern __declspec(dllimport) void unloadASIO();
extern __declspec(dllimport) long asioStart();
extern __declspec(dllimport) long asioStop();
extern __declspec(dllimport) long getASIOBaseInputChannel(void* dwData);
extern __declspec(dllimport) long getASIOBaseOutputChannel(void* dwData);
extern __declspec(dllimport) long getASIOInputMode(void* dwData);

typedef enum
{
	left = 0,
	right,
	both
} input_mode_t;

typedef struct _cmasio
{
	void* rmatchIN;
	void* rmatchOUT;
	double* input;
	double* output;
	int blocksize;
	int run;
	void* bufferFull;
	void* bufferEmpty;
	long overFlowsIn;
	long overFlowsOut;
	long underFlowsIn;
	long underFlowsOut;
	int lockMode;
	int protocol; // W4WMT cmASIO via Protocol 1

	int base_input_channel; //[2.10.3.13]MW0LGE added explicit base channel indices
	int base_output_channel;

	input_mode_t input_mode; //[2.10.3.13]MW0LGE added input mode, so would use ch1, ch2, or both for input
} cmasio, *CMASIO;

extern CMASIO pcma;

#endif