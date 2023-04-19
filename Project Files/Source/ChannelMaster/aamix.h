/*  aamix.h

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

#ifndef _aamix_h
#define _aamix_h

#include "resample.h"

typedef struct _aamix {
    int id; // id of this aamixer
    int outbound_id; // id to use in the Outbound() call
    volatile long run; // thread runs when set to 1
    volatile long accept[32]; // ring accepts data when set to 1
    int ringinsize; // input size to rings, complex samples
    int outsize; // size to output, complex samples
    int ninputs; // number of inputs, assumed to be consecutive beginning at 0
    int rsize; // total size of a ring, complex samples
    double* ring[32]; // ring buffers
    double* out; // pointer to output buffer
    volatile long active; // one bit per active (data flowing) input
    int nactive; // number of active inputs
    volatile long what; // one bit per item to mix
    double vol[32]; // volume scaling per input
    double tvol[32]; // final volume scaling
    double volume; // master volume scaling
    int inidx[32]; // input  indexes of rings, complex samples
    int outidx[32]; // output indexes of rings, complex samples
    int unqueuedsamps[32]; // for each ring, number of complex samples not yet
                           // released for mixing
    HANDLE Ready[32]; // semaphore handles, one per possible input
    HANDLE Aready[32]; // semaphore handles for active inputs
    CRITICAL_SECTION cs_in[32]; // cs to protect input process
    CRITICAL_SECTION cs_out; // cs to protect output process
    RESAMPLE rsmp[32]; // array of resampler pointers
    int inrate[32]; // sample rates of the inputs
    int outrate; // sample rate of the output
    double* resampbuff[32]; // buffers for resampler outputs
    void (*Outbound)(int id, int nsamples,
        double* buff); // function to call with output data
    struct {
        double tdelayup; // delay before upslew
        double tslewup; // upslew time
        double tdelaydown; // delay before downslew
        double tslewdown; // downslew time
        int ustate; // state of upslew function
        int dstate; // state of downslew function
        int ucount; // counter for upslew function
        int dcount; // counter for downslew function
        int ndelup; // number of samples for delayup time
        int ntup; // number of samples for upslew
        double* cup; // upslew coefficients
        int ndeldown; // number of samples for delaydown time
        int ntdown; // number of samples for downslew
        double* cdown; // coefficients for downslew
        volatile long uflag; // set when upslew is to proceed or is in progress
        volatile long
            dflag; // set when downslew is to proceed or is in progress
        HANDLE uwait;
        HANDLE dwait;
        int dtimeout;
        int utimeout;
    } slew;
} aamix, *AAMIX;

extern void* create_aamix(int id, int outbound_id, int ringinsize, int outsize,
    int ninputs, long active, long what, double volume, int ring_size,
    int* inrates, int outrate,
    void (*Outbound)(int id, int nsamples, double* buff), double tdelayup,
    double tslewup, double tdelaydown, double tslewdown);

extern void destroy_aamix(void* ptr, int id);

extern void xaamix(AAMIX a);

extern void xMixAudio(void* ptr, int id, int stream, double* data);

extern void SetAAudioMixOutputPointer(
    void* ptr, int id, void (*Outbound)(int id, int nsamples, double* buff));

extern __declspec(dllexport) void SetAAudioMixVol(
    void* ptr, int id, int stream, double vol);
extern __declspec(dllexport) void SetAAudioMixVolume(
    void* ptr, int id, double volume);
extern __declspec(dllexport) void SetAAudioMixState(
    void* ptr, int id, int stream, int state);
extern __declspec(dllexport) void SetAAudioMixStates(
    void* ptr, int id, int streams, int states);
extern __declspec(dllexport) void SetAAudioMixWhat(
    void* ptr, int id, int stream, int state);
extern void SetAAudioRingInsize(void* ptr, int id, int size);
extern void SetAAudioRingOutsize(void* ptr, int id, int size);
extern void SetAAudioOutRate(void* ptr, int id, int rate);
extern void SetAAudioStreamRate(void* ptr, int id, int mixinid, int rate);

#endif

// Accessing the Mixer Functions.
// A data structure is stored for each instance of the mixer.  A pointer to the
// data structure is required to use the corresponding instance of the mixer.
// That pointer can either be stored in a central bank of pointers and accessed
// using an 'id', OR, the owner of the mixer can store the pointer and use it in
// the various calls to the mixer.  In using the create_aamix(...) function, if
// a negative value is supplied for the 'id', a pointer will NOT be stored in
// the central bank; the function will also ALWAYS return the pointer which may
// be used or ignored.  In calls to other mixer functions, there are parameters
// for both 'id' and 'ptr'. If ptr == 0, the id will be used.  If ptr != 0, the
// pointer, supplied as 'ptr', will be used.
//
// The audio mixer is created with a specific number of inputs, 'ninputs'.
// 'ninputs' should be the MAXIMUM number of data streams that the creator will
// need to mix.
//
// Of these total 'ninputs', some can be active and some inactive at any point
// in time.  SetAAudioMixState() is used to set whether an input is active or
// not.  Data MUST CONTINUOUSLY flow into all the active inputs. To be able to
// mix the input samples and produce output, the mixer MUST have samples from
// each input stream. Therefore, if there is no data flowing in from even one
// active input, the mixer will be unable to produce any output.
//
// SetAAudioMixState() is intended to be used, for example, when a new receiver
// is created in the radio or when a receiver is removed from the radio.  It is
// NOT intended for use just to turn OFF/ON whether a stream is currently being
// mixed into the output stream.  Why?  Because its transition is much slower
// than using SetAAudioMixWhat().  (Keep reading for discussion of that.)
//
// Note also that if a stream is marked active and data is not yet flowing, the
// mixer will block until data flows!
//
// Having continuous data flow to all inputs does NOT imply that all inputs will
// always be mixed into the output. The content of the output, at any given
// time, is determined by which bits are set in the word 'what'.  Note that
// 'what' can be changed at any time, while data is flowing.  Turning a bit
// OFF/ON in 'what' could be used, for example, to MUTE/UNMUTE a particular
// software receiver.  Call SetAAudioMixWhat() to use this functionality.
//
// Individual volume settings are provided for each input.  There is also a
// master volume setting that applies to all inputs.  The individual and master
// volume settings can be changed anytime, while data is flowing.
//
// There is a ring buffer corresponding to each input stream.  When the ring for
// each input has at least the number of samples required for one 'outsize'
// output buffer, mixing proceeds and the output buffer is filled. A resampler
// is provided for each input and is activated if the input sample rate for that
// input does not match the specified output sample rate for the mixer.  The
// resampling occurs BEFORE data is entered into the ring.  This implies that
// the input size 'ringinsize' is the same for each ring.  Obviously the amount
// of data supplied to xMixAudio(), i.e., to the resampler, would vary depending
// upon input sample rate.
//
// The mixer supports changing 'outsize' and 'ringinsize' while data is flowing.
// However, it is anticipated that these parameters would likely be set when the
// radio is created and not be changed after that.
//
//
// Mixer slewing.
// When the mixer is to be 'closed' to add or remove an input using
// SetAAudioMixState(), provision is made to downslew before stopping the output
// and to upslew when the mixer is again 'opened'.
//
// When data is continuously flowing for all active inputs, operation is as
// follows.  SetAAudioMixState() calls close_mixer() where the a->slew.dflag is
// set.  Because this flag is set, for subsequent processing of data buffers in
// xaamix(), downslew() is called.  When the downslew completes, the downslew()
// function resets the a->slew.dflag.  In parallel with the downslewing activity
// in xaamix() and downslew(), the thread calling SetAAudioMixState() and
// subsequently close_mixer() is blocked waiting for semaphore a->slew.dwait.
// This prevents further closure of the mixer until the downslew is complete.
// Once the semaphore is released by the downslew() function, the mixer will be
// closed.  At that point, the changes in "MixState" are made and open_mixer()
// is then called.  a->slew.uflag is immediately set so that, when output
// buffers begin to flow in xaamix(), an upslew() will occur.  After setting the
// flag, the rest of the operations to open the mixer are completed.  However,
// the open_mixer() function blocks the calling thread until the upslew is
// complete using the a->slew.uwait semaphore.  With the calling-thread blocking
// on downslew and upslew, we are insured that multiple successive calls to
// SetAAudioMixState() will be properly handled.
//
// At initial startup, when there is no data flowing, calls to
// SetAAudioMixState() can still be made without creating a permanent block on
// the calling thread.  Each of the semaphore waits (one for downslew and the
// other for upslew) has a timeout that is set to be slightly greater than the
// downslew or upslew time. Therefore, the timeouts do not occur if data is
// flowing and the slews happen; however, if there is no data flowing, the
// timeouts prevent a permanent block of the calling thread.
//
// NOTE:  The above describes two distinct valid modes of operation:  (1) no
// data is flowing during the entire close_mixer() and open_mixer() process, and
// (2) data is continually flowing during the entire close_mixer() and
// open_mixer() process.  Correct operation will NOT be preserved if data flow
// starts or stops during these operations.
//
// Calling SetAAudioMixState() on the same thread used to turn ON/OFF dataflow
// (for example by setting the 'run' bit or setting 'enable' bits) can be used
// to keep this properly sorted out.  If data is NOT flowing, just do NOT start
// it until the call to SetAAudioMixState() returns.  If data IS flowing, simply
// do NOT turn it off until the call to SetAAudioMixState() returns.
