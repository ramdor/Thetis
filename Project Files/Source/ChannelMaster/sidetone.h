/*  sidetone.h

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

#ifndef _sidetone_h
#define _sidetone_h

typedef struct _sidetone
{
	int id;					// 'id' of the sidetone instance = id of the transmitter
	int run_st;				// run the sidetone (0 or 1)
	int run_tx;				// output IQ to transmit CW (0 or 1)
	int rate;				// sample rate
	int size;				// buffer size
	double outI;			// I value without volume applied
	double outQ;			// Q value without volume applied
	double* in;				// pointer to input buffer
	double* out_st;			// pointer to sidetone output buffer
	double* out_tx;			// pointer to transmit output buffer
	int IQ_polarity;		// transmit upper or lower sideband CW (0 or 1)
	double pitch;			// pitch frequency for both sidetone and transmit (Hz)
	double volume_st;		// volume multiplier to apply for sidetone (0.0 - 1.0)
	double volume_tx;		// volume multiplier to apply for CW tx IQ (0.0 - 1.0)
	int wpm;				// words-per-minute for dot/dash creation
	int edgetype;			// cw edge; 0 -> raised_cosine
	double edgelength;		// length of a rising or falling edge (seconds)

	double tone_phs;
	double tone_cos_phs;
	double tone_sin_phs;
	double tone_delta;
	double tone_cosdelta;
	double tone_sindelta;

	int nrise;
	double* rise_samps;
	double rise_delta;
	double rise_theta;

	int nfall;
	double* fall_samps;
	double fall_delta;
	double fall_theta;

	int n_dot_high;
	int n_dash_high;

	int state;
	double tone_out0;
	double tone_out1;
	int ntimer;
	int key;
	int dot;
	int dash;
	int use_key;

	CRITICAL_SECTION update;

} sidetone, *SIDETONE;

extern SIDETONE create_sidetone(
	int id,
	int run_st,
	int run_tx,
	int rate,
	int size,
	double* in,
	double* out_st,
	double* out_tx,
	int IQ_polarity,
	double pitch,						// Hertz
	double volume_st,					// 0.0 - 1.0
	double volume_tx,					// 0.0 - 1.0
	int wpm,							// words-per-minute
	int edgetype,						// '0' = raised-cosine
	double edgelength					// time in seconds
);

extern void destroy_sidetone(int id);

extern void xsidetone(int id);

extern __declspec (dllexport) void SetSidetoneSelectKey(int id, int select);

extern __declspec (dllexport) void keySidetone(int id, int key_select, int state);	// 0 = KEY-UP; 1 = KEY-DOWN

extern __declspec (dllexport) void makedotSidetone(int id);

extern __declspec (dllexport) void makedashSidetone(int id);

extern void setSidetoneSize(int id, int size);

extern void setSidetoneRate(int id, int rate);

extern __declspec (dllexport) void SetCWtxIQpolarity(int id, int polarity);

extern __declspec (dllexport) void SetSidetoneVolume(int id, double volume);

extern __declspec (dllexport) void SetCWtxVolume(int id, double volume);

extern __declspec (dllexport) void SetSidetoneWPM(int id, int wpm);

extern __declspec (dllexport) void SetSidetoneRun(int id, int run);

extern __declspec (dllexport) void SetCWtxRun(int id, int run);

extern __declspec (dllexport) void SetSidetonePitch(int id, double pitch);

extern __declspec (dllexport) void SetSidetoneEdgetype(int id, int type);

extern __declspec (dllexport) void SetSidetoneEdgelength(int id, double length);

#endif

// SYMBOL GENERATION
//
// There are three internal bits in the sidetone generator : key, dot, and dash.  There are corresponding 
// functions that will be called from the ChannelMaster code for each of these bits.  (It’s the 
// ChannelMaster code that receives and unpacks the incoming packets.)
//
// The ‘Key’ function is used to set the state of the ‘key’ bit.  If a call is made with a ‘1’ state indicated, 
// the internal ‘key’ bit is set to ‘1’ and the generator will generate a leading-edge and then stay high 
// until a call is received setting the internal ‘key’ bit to ‘0’.  When the ‘key’ bit is set to '0', a 
// trailing-edge is generated and the generator will go to a wait state with no output.  Continuous calls, 
// say every 1ms can be received.  If the bit is already ‘1’, getting another call to set it to ‘1’ has no effect.
// Likewise, if the internal bit is already ‘0’, subsequent ‘0’ calls have no effect.
// 
// Multiple sources can be used to drive the 'Key' function.  Only one of these can be active at a time.  The 
// choice is made by calling the function 'SetSidetoneSelectKey(int id, int select)'.  By default, 'select' will
// be set to source '0'. The source identifies itself in the call to 'keySidetone(..., key_select, ...)'.
//
// The internal ’dash’ and ‘dot’ bits work a bit differently.  The function calls for these bits can only set 
// them to ‘1’ … they are reset to ‘0’ internally in the sidetone generator.  So, if we want a dot, we call 
// the ‘dot-function’ and it sets the internal ‘dot’ bit.  The generator will then produce a fully-formed dot
// (leading edge, high time based upon speed setting, trailing edge) and then reset the internal ‘dot’ bit 
// to zero.  At that point, another call to the ‘dot-function’ will initiate another dot.  Once a dot is 
// initiated, subsequent calls for a dot have no effect until the current dot is complete, i.e., the bit is 
// already ‘1’ and setting it to ‘1’ again has no effect.
//
// This implies that the hardware should send a single packet with its dot bit set to ‘1’ as soon as the keyer 
// is beginning a dot.  Subsequent packets would be set to ‘0’.  It’s also possible to send more than one ‘1’ 
// packet just in case the first one does not arrive.  As long as those additional packets arrive during the 
// generation of the dot in the sidetone generator, they have no effect.  So, practically, one could send say 
// three or five ‘1’ packets, at 1ms intervals, followed by ‘0’ packets until it’s time for the next dot.
// 
// ‘Dash’ works just like ‘dot’, it just generates a symbol of appropriate length for a dash, given the speed setting.
//
//
// FUNCTION INPUTS AND OUTPUTS
//
// This function was originally written to only provide CW Sidetone to be mixed into audio.  It has since been
// extended to also provide a 'matching' transmit IQ output.  'Matching' implies that the symbol shape, timing,
// and pitch will be the same between the sidetone and IQ output.  Things that can vary between the two are:
// * They can be turned OFF/ON independently.
// * They have different output buffers (they have a common input buffer).
// * They have different volume multipliers.
// * The TX output has a 'polarity' indicator to select upper or lower sideband.
//
// Considering the above differences, the following calls have been added for the TX IQ output to change its
// unique parameters:
//
// void SetCWtxRun(int id, int run);				// 0 or 1 to turn OFF/ON the CW TX output
// void SetCWtxVolume(int id, double volume);		// Value of 0.0 to 1.0 to specify output level
// void SetCWtxIQpolarity(int id, int polarity);	// 0 or 1 to select sideband for CW TX
//

