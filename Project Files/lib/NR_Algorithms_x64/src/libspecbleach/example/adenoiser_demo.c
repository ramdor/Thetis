/*
libspecbleach - A spectral processing library

Copyright 2022 Luciano Dato <lucianodato@gmail.com>

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/

/**
 * This is just a simple console application example using the library with the
 * adaptive denoiser processor. It uses libsndfile to read audio, process it
 * with the algorithm and write it to an output file
 */

#include <sndfile.h>
#include <specbleach_adenoiser.h>
#include <stdlib.h>

// This is not a deliberate value. The library handles any amount passed through
// a circular buffer
#define BLOCK_SIZE 512
#define FRAME_SIZE 20

int main(int argc, char **argv) {
  if (argc != 3) {
    fprintf(stderr, "usage: %s <noisy input> <denoised output>\n", argv[0]);
    return 1;
  }

  const char *input_file_name = argv[1];
  const char *output_file_name = argv[2];

  SF_INFO *sfinfo = (SF_INFO *)calloc(1, sizeof(SF_INFO));
  SNDFILE *input_file = sf_open(input_file_name, SFM_READ, sfinfo);
  SNDFILE *output_file = sf_open(output_file_name, SFM_WRITE, sfinfo);

  // Buffers for input and output to be used by the library
  float *input_library_buffer = (float *)calloc(BLOCK_SIZE, sizeof(float));
  float *output_library_buffer = (float *)calloc(BLOCK_SIZE, sizeof(float));

  // Declaration of the library instance. It needs to know the samplerate of the
  // audio
  SpectralBleachHandle lib_instance =
      specbleach_adaptive_initialize((uint32_t)sfinfo->samplerate, FRAME_SIZE);

  // Configuration of the denoising parameters. These are hardcoded just for the
  // example
  SpectralBleachParameters parameters =
      (SpectralBleachParameters){.residual_listen = false,
                                 .reduction_amount = 10.F,
                                 .smoothing_factor = 0.F,
                                 .whitening_factor = 0.F,
                                 .noise_scaling_type = 0,
                                 .noise_rescale = 2.F,
                                 .post_filter_threshold = -10.F};

  // Load the parameters before doing the denoising. This can be done during an
  // audio loop. It's RT safe
  specbleach_adaptive_load_parameters(lib_instance, parameters);

  while (sf_readf_float(input_file, input_library_buffer, BLOCK_SIZE)) {

    // Call to the audio process. Needs to know the number of samples to
    // receive.
    specbleach_adaptive_process(lib_instance, (uint32_t)BLOCK_SIZE,
                                input_library_buffer, output_library_buffer);

    sf_writef_float(output_file, output_library_buffer, BLOCK_SIZE);
  }

  sf_close(input_file);
  sf_close(output_file);
  free(sfinfo);

  // Once done you can free the library instance and the buffers used
  specbleach_adaptive_free(lib_instance);
  free(input_library_buffer);
  free(output_library_buffer);
  return 0;
}