#define _CRT_SECURE_NO_WARNINGS
#include "cmcomm.h"

PORT
void print_cmbuff_parameters(const char* filename, int id)
{
	CMB a = pcm->pcbuff[id];
	FILE* file = fopen (filename, "a");
	fprintf (file, "id                 = %d\n", id);
	fprintf (file, "accept             = %d\n", a->accept);
	fprintf (file, "max_insize         = %d\n", a->max_in_size);
	fprintf (file, "outsize            = %d\n", a->r1_outsize);
	fprintf (file, "\n");
	fflush (file);
	fclose (file);
}

// Audacity:  Import Raw Data, Signed 32-bit PCM, Little-endian, Mono/Stereo per mode selection, 48K rate

int audiocount = 0;
int* data;
int ready = 0;
int done = 0;

void WriteAudioFile (void* arg)
{
	const wchar_t* threadName = L"WriteAudioFile Thread";
	SetThreadDescription(GetCurrentThread(), threadName);

	byte* dat = (byte *) arg;
	FILE* file = fopen ("AudioFile", "wb");

	// reverse bits of each byte (possibly needed on some platforms)
	// int i;
	// byte b;
	// for (i = 0; i < 4 * audiocount; i++)
	// {
	//	b = dat[i];
	// 	b = ((b >> 1) & 0x55) | ((b << 1) & 0xaa);
	// 	b = ((b >> 2) & 0x33) | ((b << 2) & 0xcc);
	// 	b = ((b >> 4) & 0x0f) | ((b << 4) & 0xf0);
	// 	dat[i] = b;
	// }
	fwrite ((int *)dat, sizeof (int), audiocount, file);
	fflush (file);
	fclose (file);
	free (data);
	_endthread();
}

PORT
void WriteAudio(double seconds, int rate, int size, double* indata, int mode)
{
	// seconds - number of seconds of audio to record
	// rate - sample rate
	// size - number of complex samples
	// indata - pointer to data
	static int n; 
	int i;
	const double conv = 2147483647.0;
	if (!ready)
	{
		if (mode < 3)
			n = (int)(seconds * rate);
		else
			n = 2 * (int)(seconds * rate);
		data = (int *) calloc (n, sizeof (int));
		ready = 1;
	}
	for (i = 0; i < size; i++)
	{
		if (audiocount < n)
		{
			switch (mode)
			{
				case 0:	// I only (mono)
					data[audiocount++] = (int)(conv * indata[2 * i + 0]);
					break;
				case 1: // Q only (mono)
					data[audiocount++] = (int)(conv * indata[2 * i + 1]);
					break;
				case 2: // envelope (mono)
					data[audiocount++] = (int)(conv * sqrt (indata[2 * i + 0] * indata[2 * i + 0] + indata[2 * i + 1] * indata[2 * i + 1]));
					break;
				case 3:	// complex samples (stereo)
					data[audiocount++] = (int)(conv * indata[2 * i + 0]);
					data[audiocount++] = (int)(conv * indata[2 * i + 1]);
					break;
			}
		}
	}
	if ((audiocount == n) && !done)
	{
		done = 1;
		_beginthread (WriteAudioFile, 0, (void *)data);
	}
}


/***********************************************************************************************************************************/

// WriteCharFiles(...) is specifically to dissect radio=>PC Protocol_1 packets (frames portion) 
//    and write out files for Audacity.
// This is to be called ONCE FOR EACH PACKET (2 frames, 1024 bytes) of data.
// The ddc files written should be imported as 'Raw Data' into Audacity as Signed 32-bit Stereo.
// The mic file written should be imported as 'Raw Data' into Audacity as Signed 32-bit, Mono, 48Khz.

int char_ready = 0;
struct EscribeEstructura
{
	int* ddc[8];
	int* mic;
	int nddcs;
	int n;
	int nmic;
};
struct EscribeEstructura EscribeCosas;

void EscribeLoTodo (void* arg)
{
	const wchar_t* threadName = L"EscribeLoTodo Thread";
	SetThreadDescription(GetCurrentThread(), threadName);

	for (int i = 0; i < EscribeCosas.nddcs; i++)
	{
		char num[1];
		char filename[4] = "ddc";
		_itoa (i, num, 10); //MW0LGE_21a changed to _itoa from itoa
		strcat(filename, num);
		FILE* file = fopen(&filename[0], "wb");
		fwrite((int*)EscribeCosas.ddc[i], sizeof(int), EscribeCosas.n, file);
		fflush(file);
		fclose(file);
		free (EscribeCosas.ddc[i]);
	}
	FILE* file = fopen("mic", "wb");
	fwrite((int*)EscribeCosas.mic, sizeof (int), EscribeCosas.nmic, file);
	fflush (file);
	fclose (file);
	free (EscribeCosas.mic);
	_endthread();
}


PORT
void WriteCharFiles (int seconds, int rate, unsigned char* indata, int num_ddcs)
{
	static int n;							// number of sample sets to collect
	static int n_mic;						// number of mic samples to collect
	static int mic_dec;						// decimation factor for mic
	static int spf;							// samples for each ddc in one frame of data = sample_sets per frame
	static int nframes;						// total number of frames needed
	static int* ddc_data[8];				// array or pointers to complex samples from each ddc
	static int* mic_data;					// pointer to mic samples
	static int mic_dec_count;				// decimation counter for mic samples
	static int mic_sample_count;			// count of mic samples stored
	static int frame_count;					// number of frames processed thus far
	static int char_done;
	if (!char_ready)
		{
			n = seconds * rate;				// this is equal to the samples per ddc to collect
			switch (rate)
			{
			case 48000:
				mic_dec = 1;
				break;
			case 96000:
				mic_dec = 2;
				break;
			case 192000:
				mic_dec = 4;
				break;
			case 384000:
				mic_dec = 8;
				break;
			}
			n_mic = n / mic_dec;
			spf = 504 / (6 * num_ddcs + 2);
			nframes = (int) ceil ((double)n / (double)spf);
			for (int i = 0; i < num_ddcs; i++)
				ddc_data[i] = (int *) calloc (nframes * spf, 2 * sizeof (int));		// large enough for complex samples
			mic_data = (int *) calloc (nframes * spf, sizeof (int));
			mic_dec_count = 0;
			mic_sample_count = 0;
			char_done = 0;
			frame_count = 0;
			char_ready = 1;
		}
	for (int frame = 0, bidx = 8; frame < 2; frame++, bidx += 512)					// for each frame in the packet
	{
		if (frame_count < nframes)
		{
			int offset = frame_count * spf;
			for (int j = 0, ssidx = bidx; j < spf; j++, ssidx += 6 * num_ddcs + 2)	// for each sample set in the frame
			{
				int stidx = 2 * (offset + j);
				for (int iddc = 0, idx = ssidx; iddc < num_ddcs; iddc++, idx += 6)	// for each ddc in the sample set
				{
					ddc_data[iddc][stidx + 0] = (int)((indata[idx + 0] << 24) | (indata[idx + 1] << 16) | (indata[idx + 2] << 8));	// I
					ddc_data[iddc][stidx + 1] = (int)((indata[idx + 3] << 24) | (indata[idx + 4] << 16) | (indata[idx + 5] << 8));	// Q
				}
				if (++mic_dec_count == mic_dec)
				{
					int idx = ssidx + num_ddcs * 6;
					mic_data[mic_sample_count++] = (int)((indata[idx + 0] << 24) | (indata[idx + 1] << 16));	// mic sample
					mic_dec_count = 0;
				}
			}
			frame_count++;
		}
	}
	if ((frame_count >= nframes) && !char_done)
	{
		for (int i = 0; i < 8; i++)
			EscribeCosas.ddc[i] = ddc_data[i];
		EscribeCosas.mic = mic_data;
		EscribeCosas.nddcs = num_ddcs;
		EscribeCosas.n = n;
		EscribeCosas.nmic = n_mic;
		_beginthread(EscribeLoTodo, 0, (void*)0);
		char_done = 1;
	}
}

/***********************************************************************************************************************************/
