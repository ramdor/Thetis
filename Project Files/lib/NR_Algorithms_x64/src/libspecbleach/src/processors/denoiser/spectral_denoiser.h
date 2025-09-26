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

#ifndef SPECTRAL_DENOISER_H
#define SPECTRAL_DENOISER_H

#include "../../interfaces/spectral_processor.h"
#include "../../shared/noise_estimation/noise_profile.h"
#include <stdbool.h>
#include <stdint.h>

typedef struct DenoiserParameters {
  float reduction_amount;
  int noise_scaling_type;
  float noise_rescale;
  bool residual_listen;
  bool transient_protection;
  int learn_noise;
  float smoothing_factor;
  float whitening_factor;
  float post_filter_threshold;
} DenoiserParameters;

SpectralProcessorHandle
spectral_denoiser_initialize(uint32_t sample_rate, uint32_t fft_size,
                             uint32_t overlap_factor,
                             NoiseProfile *noise_profile);
void spectral_denoiser_free(SpectralProcessorHandle instance);
bool load_reduction_parameters(SpectralProcessorHandle instance,
                               DenoiserParameters parameters);
bool spectral_denoiser_run(SpectralProcessorHandle instance,
                           float *fft_spectrum);

#endif