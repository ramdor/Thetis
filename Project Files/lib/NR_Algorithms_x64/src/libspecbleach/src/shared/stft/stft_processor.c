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

#include "stft_processor.h"
#include "../configurations.h"
#include "../utils/spectral_features.h"
#include "stft_buffer.h"
#include "stft_windows.h"
#include <math.h>
#include <stdlib.h>
#include <string.h>

struct StftProcessor {
  uint32_t input_latency;
  uint32_t hop;
  uint32_t overlap_factor;
  uint32_t fft_size;
  uint32_t frame_size;
  float *output_accumulator;
  float *tmp_buffer;

  FftTransform *fft_transform;
  StftBuffer *stft_buffer;
  StftWindows *stft_windows;
};

StftProcessor *stft_processor_initialize(const uint32_t sample_rate,
                                         const float stft_frame_size,
                                         const uint32_t overlap_factor,
                                         ZeroPaddingType padding_type,
                                         const uint32_t zeropadding_amount,
                                         WindowTypes input_window,
                                         WindowTypes output_window) {
  StftProcessor *self = (StftProcessor *)calloc(1U, sizeof(StftProcessor));

  self->frame_size =
      (uint32_t)((stft_frame_size / 1000.F) * (float)sample_rate);
  self->fft_transform = fft_transform_initialize(self->frame_size, padding_type,
                                                 zeropadding_amount);
  self->fft_size = get_fft_size(self->fft_transform);
  self->overlap_factor = overlap_factor;
  self->hop = self->frame_size / self->overlap_factor;
  self->input_latency = self->frame_size - self->hop;

  self->output_accumulator =
      (float *)calloc(self->frame_size * 2L, sizeof(float));
  self->tmp_buffer = (float *)calloc(self->frame_size, sizeof(float));

  self->stft_buffer =
      stft_buffer_initialize(self->frame_size, self->input_latency, self->hop);

  self->stft_windows = stft_window_initialize(
      self->fft_size, self->overlap_factor, input_window, output_window);

  return self;
}

void stft_processor_free(StftProcessor *self) {
  stft_buffer_free(self->stft_buffer);
  stft_window_free(self->stft_windows);
  fft_transform_free(self->fft_transform);

  free(self->output_accumulator);
  free(self->tmp_buffer);

  free(self);
}

bool stft_processor_run(StftProcessor *self, const uint32_t number_of_samples,
                        const float *input, float *output,
                        spectral_processing spectral_processing,
                        SpectralProcessorHandle spectral_processor) {
  if (!self || !input || !output || number_of_samples <= 0U) {
    return false;
  }

  for (uint32_t k = 0U; k < number_of_samples; k++) {
    // Start filling buffer sample per sample
    output[k] = stft_buffer_fill(self->stft_buffer, input[k]);

    if (is_buffer_full(self->stft_buffer)) {
      fft_load_input_samples(self->fft_transform,
                             get_full_buffer_block(self->stft_buffer));

      // STFT Analysis
      stft_window_apply(self->stft_windows,
                        get_fft_input_buffer(self->fft_transform),
                        INPUT_WINDOW);

      compute_forward_fft(self->fft_transform);

      // Apply processing
      spectral_processing(spectral_processor,
                          get_fft_output_buffer(self->fft_transform));

      // STFT Synthesis
      compute_backward_fft(self->fft_transform);

      stft_window_apply(self->stft_windows,
                        get_fft_input_buffer(self->fft_transform),
                        OUTPUT_WINDOW);

      fft_get_output_samples(self->fft_transform, self->tmp_buffer);

      // STFT Overlap Add
      for (uint32_t k = 0U; k < self->frame_size; k++) {
        self->output_accumulator[k] += self->tmp_buffer[k];
      }

      stft_buffer_advance_block(self->stft_buffer, self->output_accumulator);

      memmove(self->output_accumulator, &self->output_accumulator[self->hop],
              self->frame_size * sizeof(float));
    }
  }

  return true;
}

uint32_t get_stft_latency(StftProcessor *self) { return self->input_latency; }

uint32_t get_stft_fft_size(StftProcessor *self) { return self->fft_size; }

uint32_t get_stft_real_spectrum_size(StftProcessor *self) {
  return get_fft_real_spectrum_size(self->fft_transform);
}