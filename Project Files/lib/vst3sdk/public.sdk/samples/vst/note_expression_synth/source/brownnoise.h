//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_synth/source/brownnoise.h
// Created by  : Steinberg, 03/2010
// Description : 
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

#include "pluginterfaces/base/ftypes.h"
#include <stdlib.h>
#include <cmath>

namespace Steinberg {
namespace Vst {
namespace NoteExpressionSynth {

//-----------------------------------------------------------------------------
template<class SamplePrecision>
class BrownNoise
{
public:
	BrownNoise (int32 bufferSize, SamplePrecision sampleRate);
	~BrownNoise ();

	SamplePrecision at (int32 pos) const { return buffer[pos]; }
	int32 getSize () const { return bufferSize; }
protected:
	SamplePrecision* buffer;
	int32 bufferSize;
};

//-----------------------------------------------------------------------------
template<class SamplePrecision>
BrownNoise<SamplePrecision>::BrownNoise (int32 bufferSize, SamplePrecision sampleRate)
: buffer (0)
, bufferSize (bufferSize)
{
	buffer = new SamplePrecision [bufferSize];

	const SamplePrecision f = (SamplePrecision)0.0045;
	SamplePrecision accu = (SamplePrecision)0.;
	SamplePrecision y;
	for (int32 frame = 0; frame < bufferSize; frame++)
	{
		y = ((SamplePrecision)rand () / (SamplePrecision)RAND_MAX - (SamplePrecision)0.5) * (SamplePrecision)2.;

		accu = (f * y) + (((SamplePrecision)1.0 - f) * accu);
		y = (SamplePrecision)1.55 * accu * (SamplePrecision)100. / (SamplePrecision)::sqrt (::sqrt (sampleRate));
		buffer[frame] = y;
	}
}

//-----------------------------------------------------------------------------
template<class SamplePrecision>
BrownNoise<SamplePrecision>::~BrownNoise ()
{
	delete [] buffer;
}

}}} // namespaces
