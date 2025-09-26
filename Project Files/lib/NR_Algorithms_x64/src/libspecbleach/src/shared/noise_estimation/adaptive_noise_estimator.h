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

#ifndef ADAPTIVE_NOISE_ESTIMATOR_H
#define ADAPTIVE_NOISE_ESTIMATOR_H

#include <stdbool.h>
#include <stdint.h>

typedef struct AdaptiveNoiseEstimator AdaptiveNoiseEstimator;

AdaptiveNoiseEstimator *
louizou_estimator_initialize(uint32_t noise_spectrum_size, uint32_t sample_rate,
                             uint32_t fft_size);
void louizou_estimator_free(AdaptiveNoiseEstimator *self);
bool louizou_estimator_run(AdaptiveNoiseEstimator *self, const float *spectrum,
                           float *noise_spectrum);

#endif