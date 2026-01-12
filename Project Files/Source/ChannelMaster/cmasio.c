/*  cmasio.c

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

#include "cmcomm.h"
#include "obbuffs.h"

cmasio cma = { 0 };
CMASIO pcma = &cma;
int cmaError = 0;

void create_cmasio()
{
	pcma->protocol = 1; // default Protocol 2
	pcma->blocksize = pcm->audio_outsize;
	int samplerate = pcm->audio_outrate;
	char* asioDriverName = (char*)calloc(32, sizeof(char));
	if (getASIODriverString(asioDriverName) != 0) { free(asioDriverName); return; }
	char buf[128];
	sprintf_s(buf, 128, "Initializing cmASIO with: \nblock size = %d\nsample rate = %d\ndriver name = %s\n\n", pcma->blocksize, samplerate, asioDriverName);
	OutputDebugStringA(buf);

	//[2.10.3.13]MW0LGE get explicit base channel indices for the stereo pair, default to 0 if none in registry
	long input_base_channel = 0;
	if (getASIOBaseInputChannel(&input_base_channel) != 0) input_base_channel = 0;
	long output_base_channel = 0;
	if (getASIOBaseOutputChannel(&output_base_channel) != 0) output_base_channel = 0;

	//[2.10.3.13]MW0LGE the input mode, left = ch1, right = ch2, both = stereo
	long input_mode = 0;
	if (getASIOInputMode(&input_mode) != 0) input_mode = (long)both; //both is default
	pcma->input_mode = (input_mode_t)input_mode;

	//int result = prepareASIO(pcma->blocksize, samplerate, asioDriverName, &CallbackASIO)
	int result = prepareASIO(pcma->blocksize, samplerate, asioDriverName, &CallbackASIO, input_base_channel, output_base_channel);
	sprintf_s(buf, 128, "prepareASIO return = %d", result);
	OutputDebugStringA(buf);
	if (result == 0)
	{
		sprintf_s(buf, 128, "ASIO driver \"%s\" is loaded, initialized, and prepared.", asioDriverName);
		OutputDebugStringA(buf);
		pcm->audioCodecId = ASIO;
		pcma->run = 0;

		pcma->output = (double*)malloc0(pcma->blocksize * sizeof(complex));
		pcma->input = (double*)malloc0(pcma->blocksize * sizeof(complex));

		long blockNum = 0;
		if (getASIOBlockNum(&blockNum) != 0) blockNum = 5;
		pcma->lockMode = (0xffff0000 & blockNum) ? 1 : 0;
		blockNum = 0x0000ffff & blockNum;
		sprintf_s(buf, 128, "blockNum = %d\nlockMode = %d", blockNum, pcma->lockMode);
		OutputDebugStringA(buf);

		pcma->rmatchIN = create_rmatchV(pcma->blocksize, pcma->blocksize, samplerate, samplerate, blockNum * pcma->blocksize, 1);
		pcma->rmatchOUT = create_rmatchV(pcma->blocksize, pcma->blocksize, samplerate, samplerate, blockNum * pcma->blocksize, 1);
		forceRMatchVar(pcma->rmatchIN, 1, 1);
		forceRMatchVar(pcma->rmatchOUT, 1, 1);

		pcma->bufferFull = CreateSemaphore(NULL, 0, 1, NULL);
		pcma->bufferEmpty = CreateSemaphore(NULL, 1, 1, NULL);

		pcma->overFlowsIn = pcma->overFlowsOut = pcma->underFlowsIn = pcma->underFlowsOut = 0;
	}
	cmaError = result;
	free(asioDriverName);
}

void destroy_cmasio()
{
	if (pcm->audioCodecId != ASIO) return;
	CloseHandle(pcma->bufferFull);
	CloseHandle(pcma->bufferEmpty);
	destroy_rmatchV(pcma->rmatchIN);
	destroy_rmatchV(pcma->rmatchOUT);
	_aligned_free(pcma->input);
	_aligned_free(pcma->output);
	unloadASIO();

	char buf[128];
	sprintf_s(buf, 128, "cmASIO has been destroyed");
	OutputDebugStringA(buf);
}

void asioIN(double* in_tx)
{	// used for ASIO MIC data to TX
	if (pcm->audioCodecId != ASIO) return;
	if (pcma->lockMode)
	{
		if (WaitForSingleObject(pcma->bufferFull, 2) == WAIT_TIMEOUT) { ++pcma->underFlowsIn; return; }
		memcpy(in_tx, pcma->input, pcma->blocksize * sizeof(complex));
		combinebuff(pcma->blocksize, in_tx, in_tx);
		ReleaseSemaphore(pcma->bufferEmpty, 1, NULL);
	}
	else
	{
		xrmatchOUT(pcma->rmatchIN, in_tx);
		combinebuff(pcma->blocksize, in_tx, in_tx);
	}
}

void asioOUT(int id, int nsamples, double* buff)
{	// called by the global mixer with a buffer of output data for ASIO
	if (!pcma->run) return;
	xrmatchIN(pcma->rmatchOUT, buff);		// audio data from mixer
	if (pcma->protocol == 0) // W4WMT cmASIO via Protocol 1
	{
		memset(buff, 0, nsamples * sizeof(complex));
		OutBound(0, nsamples, buff);
	} // W4WMT cmASIO via Protocol 1
}

//[2.10.3.13]MW0LGE added input mode, so can use ch1(L), ch2(R), or both for input
void CallbackASIO(void* inputL, void* inputR, void* outputL, void* outputR)
{
	long* temp_inputL = (long*)inputL;
	long* temp_inputR = (long*)inputR;

	if (pcma->input_mode == left)
	{
		temp_inputR = temp_inputL;
	}
	else if (pcma->input_mode == right)
	{
		temp_inputL = temp_inputR;
	}

	if (pcma->lockMode)
	{
		if (WaitForSingleObject(pcma->bufferEmpty, 0) == WAIT_OBJECT_0)
		{
			for (int i = 0; i < pcma->blocksize; i++)
			{
				pcma->input[2 * i] = ((double)temp_inputL[i]) / 2147483648.0;
				pcma->input[2 * i + 1] = ((double)temp_inputR[i]) / 2147483648.0;
			}
			ReleaseSemaphore(pcma->bufferFull, 1, NULL);

			xrmatchOUT(pcma->rmatchOUT, pcma->output);
			for (int i = 0; i < pcma->blocksize; i++)
			{
				((long*)outputL)[i] = (long)(pcma->output[2 * i] * 2147483648.0);
				((long*)outputR)[i] = (long)(pcma->output[2 * i + 1] * 2147483648.0);
			}
		}
		else
		{
			xrmatchOUT(pcma->rmatchOUT, pcma->output);
			for (int i = 0; i < pcma->blocksize; i++)
			{
				((long*)outputL)[i] = (long)(pcma->output[2 * i] * 2147483648.0);
				((long*)outputR)[i] = (long)(pcma->output[2 * i + 1] * 2147483648.0);
			}

			if (WaitForSingleObject(pcma->bufferEmpty, 2) == WAIT_TIMEOUT) { ++pcma->overFlowsIn; return; }
			for (int i = 0; i < pcma->blocksize; i++)
			{
				pcma->input[2 * i] = ((double)temp_inputL[i]) / 2147483648.0;
				pcma->input[2 * i + 1] = ((double)temp_inputR[i]) / 2147483648.0;
			}
			ReleaseSemaphore(pcma->bufferFull, 1, NULL);
		}
	}
	else
	{
		for (int i = 0; i < pcma->blocksize; i++)
		{
			pcma->input[2 * i] = ((double)temp_inputL[i]) / 2147483648.0;
			pcma->input[2 * i + 1] = ((double)temp_inputR[i]) / 2147483648.0;
		}
		xrmatchIN(pcma->rmatchIN, pcma->input);

		xrmatchOUT(pcma->rmatchOUT, pcma->output);
		for (int i = 0; i < pcma->blocksize; i++)
		{
			((long*)outputL)[i] = (long)(pcma->output[2 * i] * 2147483648.0);
			((long*)outputR)[i] = (long)(pcma->output[2 * i + 1] * 2147483648.0);
		}
	}
}

//void CallbackASIO(void* inputL, void* inputR, void* outputL, void* outputR)
//{
//	if (pcma->lockMode)
//	{
//		if (WaitForSingleObject(pcma->bufferEmpty, 0) == WAIT_OBJECT_0)
//		{
//			for (int i = 0; i < pcma->blocksize; i++)
//			{
//				pcma->input[2 * i] = ((double)((long*)inputL)[i]) / 2147483648.0;
//				pcma->input[2 * i + 1] = ((double)((long*)inputR)[i]) / 2147483648.0;
//			}
//			ReleaseSemaphore(pcma->bufferFull, 1, NULL);
//
//			xrmatchOUT(pcma->rmatchOUT, pcma->output);
//			for (int i = 0; i < pcma->blocksize; i++)
//			{
//				((long*)outputL)[i] = (long)(pcma->output[2 * i] * 2147483648.0);
//				((long*)outputR)[i] = (long)(pcma->output[2 * i + 1] * 2147483648.0);
//			}
//		}
//		else
//		{
//			xrmatchOUT(pcma->rmatchOUT, pcma->output);
//			for (int i = 0; i < pcma->blocksize; i++)
//			{
//				((long*)outputL)[i] = (long)(pcma->output[2 * i] * 2147483648.0);
//				((long*)outputR)[i] = (long)(pcma->output[2 * i + 1] * 2147483648.0);
//			}
//
//			if (WaitForSingleObject(pcma->bufferEmpty, 2) == WAIT_TIMEOUT) { ++pcma->overFlowsIn; return; }
//			for (int i = 0; i < pcma->blocksize; i++)
//			{
//				pcma->input[2 * i] = ((double)((long*)inputL)[i]) / 2147483648.0;
//				pcma->input[2 * i + 1] = ((double)((long*)inputR)[i]) / 2147483648.0;
//			}
//			ReleaseSemaphore(pcma->bufferFull, 1, NULL);
//		}
//	}
//	else
//	{
//		for (int i = 0; i < pcma->blocksize; i++)
//		{
//			pcma->input[2 * i] = ((double)((long*)inputL)[i]) / 2147483648.0;
//			pcma->input[2 * i + 1] = ((double)((long*)inputR)[i]) / 2147483648.0;
//		}
//		xrmatchIN(pcma->rmatchIN, pcma->input);
//
//		xrmatchOUT(pcma->rmatchOUT, pcma->output);
//		for (int i = 0; i < pcma->blocksize; i++)
//		{
//			((long*)outputL)[i] = (long)(pcma->output[2 * i] * 2147483648.0);
//			((long*)outputR)[i] = (long)(pcma->output[2 * i + 1] * 2147483648.0);
//		}
//	}
//}

//SetAAudioMixOutputPointer(0, 0, asio_out);

long cm_asioStart(int protocol)
{
	if (pcm->audioCodecId != ASIO) return -1;
	pcma->protocol = protocol; // W4WMT cmASIO via Protocol 1
	//if (protocol == 0)
	//{
	//	SendpOutboundRx(OutBound);
	//	pcm->audioCodecId = HERMES;
	//	char buf[128];
	//	sprintf_s(buf, 128, "Protocol1 Aborting!");
	//	OutputDebugStringA(buf);
	//	return -1;
	//}
	char buf[128];
	long result = asioStart();
	sprintf_s(buf, 128, "asioStart = %d", result);
	OutputDebugStringA(buf);
	pcma->run = 1;
	return result;
}

long cm_asioStop()
{
	if (pcm->audioCodecId != ASIO) return -1;
	pcma->run = 0;
	char buf[128];
	long result = asioStop();
	sprintf_s(buf, 128, "asioStop = %d", result);
	OutputDebugStringA(buf);
	return result;
}

PORT
int getCMAstate()
{
	if (cmaError)
	{
		return -1;
	}
	return (pcm->audioCodecId == ASIO) ? 1 : 0;
}

PORT
void getCMAevents(long* overFlowsIn, long* overFlowsOut, long* underFlowsIn, long* underFlowsOut)
{
	if (pcm->audioCodecId != ASIO) return;
	double foo = 0.0;
	double* pfoo = &foo;
	int bar = 0;
	int* pbar = &bar;
	getRMatchDiags(pcma->rmatchIN, underFlowsIn, overFlowsIn, pfoo, pbar, pbar);
	getRMatchDiags(pcma->rmatchOUT, underFlowsOut, overFlowsOut, pfoo, pbar, pbar);
	if (pcma->lockMode)
	{
		*overFlowsIn = pcma->overFlowsIn;
		*underFlowsIn = pcma->underFlowsIn;
	}
}

PORT
void resetCMAevents()
{
	if (pcm->audioCodecId != ASIO) return;
	pcma->overFlowsIn = pcma->overFlowsOut = pcma->underFlowsIn = pcma->underFlowsOut = 0;
	resetRMatchDiags(pcma->rmatchIN);
	resetRMatchDiags(pcma->rmatchOUT);
}
