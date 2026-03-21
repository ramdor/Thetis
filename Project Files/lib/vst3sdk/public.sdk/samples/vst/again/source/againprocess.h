//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/again/source/againprocess.h
// Created by  : Steinberg, 11/2016
// Description : AGain Example for VST SDK 3.0
//               Simple gain plug-in with gain, bypass values and 1 midi input
//               and the same plug-in with sidechain 
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

namespace Steinberg {
namespace Vst {

//------------------------------------------------------------------------
template <typename SampleType>
SampleType AGain::processAudio (SampleType** in, SampleType** out, int32 numChannels,
                                int32 sampleFrames, float gain)
{
	SampleType vuPPM = 0;

	// in real Plug-in it would be better to do dezippering to avoid jump (click) in gain value
	for (int32 i = 0; i < numChannels; i++)
	{
		int32 samples = sampleFrames;
		SampleType* ptrIn = (SampleType*)in[i];
		SampleType* ptrOut = (SampleType*)out[i];
		SampleType tmp;
		while (--samples >= 0)
		{
			// apply gain
			tmp = (*ptrIn++) * gain;
			(*ptrOut++) = tmp;

			// check only positive values
			if (tmp > vuPPM)
			{
				vuPPM = tmp;
			}
		}
	}
	return vuPPM;
}


//------------------------------------------------------------------------
template <typename SampleType>
SampleType AGain::processVuPPM (SampleType** in, int32 numChannels,	int32 sampleFrames)
{
	SampleType vuPPM = 0;

	for (int32 i = 0; i < numChannels; i++)
	{
		int32 samples = sampleFrames;
		SampleType* ptrIn = (SampleType*)in[i];
		SampleType tmp;
		while (--samples >= 0)
		{
			tmp = (*ptrIn++);
			
			// check only positive values
			if (tmp > vuPPM)
			{
				vuPPM = tmp;
			}
		}
	}
	return vuPPM;
}

} // Vst
} // Steinberg
