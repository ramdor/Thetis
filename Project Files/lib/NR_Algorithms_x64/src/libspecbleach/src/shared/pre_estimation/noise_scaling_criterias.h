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

#ifndef NOISE_SCALING_CRITERIAS_H
#define NOISE_SCALING_CRITERIAS_H

#include "../utils/spectral_features.h"
#include "critical_bands.h"
#include <stdbool.h>
#include <stdint.h>

typedef enum NoiseScalingType {
  A_POSTERIORI_SNR = 0,
  A_POSTERIORI_SNR_CRITICAL_BANDS = 1,
  MASKING_THRESHOLDS = 2,
} NoiseScalingType;

typedef struct NoiseScalingParameters {
  float undersubtraction;
  float oversubtraction;
  int scaling_type;
} NoiseScalingParameters;

typedef struct NoiseScalingCriterias NoiseScalingCriterias;

NoiseScalingCriterias *noise_scaling_criterias_initialize(
    uint32_t fft_size, CriticalBandType critical_band_type,
    uint32_t sample_rate, SpectrumType spectrum_type);
void noise_scaling_criterias_free(NoiseScalingCriterias *self);
bool apply_noise_scaling_criteria(NoiseScalingCriterias *self,
                                  const float *spectrum,
                                  const float *noise_spectrum, float *alpha,
                                  float *beta,
                                  NoiseScalingParameters parameters);

#endif