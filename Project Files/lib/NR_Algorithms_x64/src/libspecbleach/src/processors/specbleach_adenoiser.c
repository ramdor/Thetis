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

#include "../../include/specbleach_adenoiser.h"
#include "../shared/configurations.h"
#include "../shared/stft/stft_processor.h"
#include "../shared/utils/general_utils.h"
#include "adaptivedenoiser/adaptive_denoiser.h"
#include <math.h>
#include <stdlib.h>
#include <string.h>

typedef struct SbAdaptiveDenoiser {
  uint32_t sample_rate;
  AdaptiveDenoiserParameters denoise_parameters;

  SpectralProcessorHandle adaptive_spectral_denoiser;
  StftProcessor *stft_processor;
} SbAdaptiveDenoiser;

SpectralBleachHandle specbleach_adaptive_initialize(const uint32_t sample_rate,
                                                    float frame_size) {
  SbAdaptiveDenoiser *self =
      (SbAdaptiveDenoiser *)calloc(1U, sizeof(SbAdaptiveDenoiser));

  self->sample_rate = sample_rate;

  self->stft_processor = stft_processor_initialize(
      sample_rate, frame_size, OVERLAP_FACTOR_SPEECH,
      PADDING_CONFIGURATION_SPEECH, ZEROPADDING_AMOUNT_SPEECH,
      INPUT_WINDOW_TYPE_SPEECH, OUTPUT_WINDOW_TYPE_SPEECH);

  if (!self->stft_processor) {
    specbleach_adaptive_free(self);
    return NULL;
  }

  const uint32_t fft_size = get_stft_fft_size(self->stft_processor);

  self->adaptive_spectral_denoiser = spectral_adaptive_denoiser_initialize(
      self->sample_rate, fft_size, OVERLAP_FACTOR_SPEECH);

  if (!self->adaptive_spectral_denoiser) {
    specbleach_adaptive_free(self);
    return NULL;
  }

  return self;
}

void specbleach_adaptive_free(SpectralBleachHandle instance) {
  SbAdaptiveDenoiser *self = (SbAdaptiveDenoiser *)instance;

  spectral_adaptive_denoiser_free(self->adaptive_spectral_denoiser);
  stft_processor_free(self->stft_processor);

  free(self);
}

uint32_t specbleach_adaptive_get_latency(SpectralBleachHandle instance) {
  SbAdaptiveDenoiser *self = (SbAdaptiveDenoiser *)instance;

  return get_stft_latency(self->stft_processor);
}

bool specbleach_adaptive_process(SpectralBleachHandle instance,
                                 const uint32_t number_of_samples,
                                 const float *input, float *output) {
  if (!instance || number_of_samples == 0 || !input || !output) {
    return false;
  }

  SbAdaptiveDenoiser *self = (SbAdaptiveDenoiser *)instance;

  stft_processor_run(self->stft_processor, number_of_samples, input, output,
                     &spectral_adaptive_denoiser_run,
                     self->adaptive_spectral_denoiser);

  return true;
}

bool specbleach_adaptive_load_parameters(SpectralBleachHandle instance,
                                         SpectralBleachParameters parameters) {
  if (!instance) {
    return false;
  }

  SbAdaptiveDenoiser *self = (SbAdaptiveDenoiser *)instance;

  // clang-format off
  self->denoise_parameters = (AdaptiveDenoiserParameters){
      .residual_listen = parameters.residual_listen,
      .reduction_amount =
          from_db_to_coefficient(parameters.reduction_amount * -1.F),
      .noise_rescale = from_db_to_coefficient(parameters.noise_rescale),
      .noise_scaling_type = parameters.noise_scaling_type,
      .smoothing_factor = remap_percentage_log_like_unity(parameters.smoothing_factor / 100.F),
      .whitening_factor = parameters.whitening_factor / 100.F,
      .post_filter_threshold = from_db_to_coefficient(parameters.post_filter_threshold),
  };
  // clang-format on

  load_adaptive_reduction_parameters(self->adaptive_spectral_denoiser,
                                     self->denoise_parameters);

  return true;
}