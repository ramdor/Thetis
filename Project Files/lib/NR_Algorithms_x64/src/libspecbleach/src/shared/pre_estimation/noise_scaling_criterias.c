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

#include "noise_scaling_criterias.h"
#include "../configurations.h"
#include "../utils/spectral_utils.h"
#include "critical_bands.h"
#include "masking_estimator.h"
#include <float.h>
#include <math.h>
#include <stdlib.h>

static void a_posteriori_snr_critical_bands(NoiseScalingCriterias *self,
                                            const float *spectrum,
                                            const float *noise_spectrum,
                                            float *alpha,
                                            NoiseScalingParameters parameters);
static void a_posteriori_snr(NoiseScalingCriterias *self, const float *spectrum,
                             const float *noise_spectrum, float *alpha,
                             NoiseScalingParameters parameters);
static void masking_thresholds(NoiseScalingCriterias *self,
                               const float *spectrum,
                               const float *noise_spectrum, float *alpha,
                               float *beta, NoiseScalingParameters parameters);

struct NoiseScalingCriterias {
  NoiseScalingType noise_scaling_type;
  uint32_t fft_size;
  uint32_t real_spectrum_size;
  uint32_t sample_rate;
  SpectrumType spectrum_type;
  uint32_t number_critical_bands;
  float lower_snr;
  float higher_snr;
  float alpha_minimun;
  float beta_minimun;
  CriticalBandIndexes band_indexes;
  CriticalBandType critical_band_type;

  float *masking_thresholds;
  float *clean_signal_estimation;
  float *critical_bands_noise_profile;
  float *critical_bands_reference_spectrum;

  MaskingEstimator *masking_estimation;
  CriticalBands *critical_bands;
};

NoiseScalingCriterias *noise_scaling_criterias_initialize(
    const uint32_t fft_size, const CriticalBandType critical_band_type,
    const uint32_t sample_rate, SpectrumType spectrum_type) {

  NoiseScalingCriterias *self =
      (NoiseScalingCriterias *)calloc(1U, sizeof(NoiseScalingCriterias));

  self->fft_size = fft_size;
  self->real_spectrum_size = self->fft_size / 2U + 1U;
  self->critical_band_type = critical_band_type;
  self->sample_rate = sample_rate;
  self->spectrum_type = spectrum_type;
  self->lower_snr = LOWER_SNR;
  self->higher_snr = HIGHER_SNR;
  self->alpha_minimun = ALPHA_MIN;
  self->beta_minimun = BETA_MIN;

  self->critical_bands = critical_bands_initialize(
      self->sample_rate, self->fft_size, self->critical_band_type);
  self->masking_estimation = masking_estimation_initialize(
      self->fft_size, self->sample_rate, self->spectrum_type);
  self->number_critical_bands =
      get_number_of_critical_bands(self->critical_bands);

  self->critical_bands_noise_profile =
      (float *)calloc(self->number_critical_bands, sizeof(float));
  self->critical_bands_reference_spectrum =
      (float *)calloc(self->number_critical_bands, sizeof(float));

  self->masking_thresholds =
      (float *)calloc(self->real_spectrum_size, sizeof(float));
  self->clean_signal_estimation =
      (float *)calloc(self->real_spectrum_size, sizeof(float));

  return self;
}

void noise_scaling_criterias_free(NoiseScalingCriterias *self) {
  critical_bands_free(self->critical_bands);
  masking_estimation_free(self->masking_estimation);

  free(self->clean_signal_estimation);
  free(self->masking_thresholds);
  free(self->critical_bands_noise_profile);
  free(self->critical_bands_reference_spectrum);

  free(self);
}

bool apply_noise_scaling_criteria(NoiseScalingCriterias *self,
                                  const float *spectrum,
                                  const float *noise_spectrum, float *alpha,
                                  float *beta,
                                  NoiseScalingParameters parameters) {
  if (!spectrum || !noise_spectrum) {
    return false;
  }

  switch ((NoiseScalingType)parameters.scaling_type) {
  case A_POSTERIORI_SNR:
    a_posteriori_snr(self, spectrum, noise_spectrum, alpha, parameters);
    break;
  case A_POSTERIORI_SNR_CRITICAL_BANDS:
    a_posteriori_snr_critical_bands(self, spectrum, noise_spectrum, alpha,
                                    parameters);
    break;
  case MASKING_THRESHOLDS:
    masking_thresholds(self, spectrum, noise_spectrum, alpha, beta, parameters);
    break;

  default:
    break;
  }

  return true;
}

static void a_posteriori_snr_critical_bands(NoiseScalingCriterias *self,
                                            const float *spectrum,
                                            const float *noise_spectrum,
                                            float *alpha,
                                            NoiseScalingParameters parameters) {

  compute_critical_bands_spectrum(self->critical_bands, noise_spectrum,
                                  self->critical_bands_noise_profile);
  compute_critical_bands_spectrum(self->critical_bands, spectrum,
                                  self->critical_bands_reference_spectrum);

  float a_posteriori_snr = 20.F;
  float oversustraction_factor = 1.F;

  for (uint32_t j = 0U; j < self->number_critical_bands; j++) {

    self->band_indexes = get_band_indexes(self->critical_bands, j);

    a_posteriori_snr =
        10.F * log10f(self->critical_bands_reference_spectrum[j] /
                      self->critical_bands_noise_profile[j]);

    if (a_posteriori_snr >= self->lower_snr &&
        a_posteriori_snr <= self->higher_snr) {
      oversustraction_factor =
          -0.05F * (a_posteriori_snr) + parameters.oversubtraction;
    } else if (a_posteriori_snr < 0.F) {
      oversustraction_factor = parameters.oversubtraction;
    } else if (a_posteriori_snr > 20.F) {
      oversustraction_factor = 1.F;
    }

    for (uint32_t k = self->band_indexes.start_position;
         k < self->band_indexes.end_position; k++) {
      alpha[k] = oversustraction_factor;
    }
  }
}

static void a_posteriori_snr(NoiseScalingCriterias *self, const float *spectrum,
                             const float *noise_spectrum, float *alpha,
                             NoiseScalingParameters parameters) {
  float a_posteriori_snr = 20.F;
  float oversustraction_factor = 1.F;
  float noisy_spectrum_sum = 0.F;
  float noise_spectrum_sum = 0.F;

  for (uint32_t k = 1U; k < self->real_spectrum_size; k++) {
    noisy_spectrum_sum += spectrum[k];
    noise_spectrum_sum += noise_spectrum[k];
  }

  a_posteriori_snr = 10.F * log10f(noisy_spectrum_sum / noise_spectrum_sum);

  if (a_posteriori_snr >= self->lower_snr &&
      a_posteriori_snr <= self->higher_snr) {
    oversustraction_factor =
        -0.05F * (a_posteriori_snr) + parameters.oversubtraction;
  } else if (a_posteriori_snr < 0.F) {
    oversustraction_factor = parameters.oversubtraction;
  } else if (a_posteriori_snr > 20.F) {
    oversustraction_factor = 1.F;
  }

  for (uint32_t k = 1U; k < self->real_spectrum_size; k++) {
    alpha[k] = oversustraction_factor;
  }
}

static void masking_thresholds(NoiseScalingCriterias *self,
                               const float *spectrum,
                               const float *noise_spectrum, float *alpha,
                               float *beta, NoiseScalingParameters parameters) {

  for (uint32_t k = 1U; k < self->real_spectrum_size; k++) {
    self->clean_signal_estimation[k] =
        fmaxf(spectrum[k] - noise_spectrum[k], 0.F);
  }

  compute_masking_thresholds(self->masking_estimation,
                             self->clean_signal_estimation,
                             self->masking_thresholds);

  float max_masked_value =
      max_spectral_value(self->masking_thresholds, self->real_spectrum_size);
  float min_masked_value =
      min_spectral_value(self->masking_thresholds, self->real_spectrum_size);

  for (uint32_t k = 1U; k < self->real_spectrum_size; k++) {
    if (self->masking_thresholds[k] == max_masked_value) {
      alpha[k] = self->alpha_minimun;
      beta[k] = self->beta_minimun;
    } else if (self->masking_thresholds[k] == min_masked_value) {
      alpha[k] = parameters.oversubtraction;
      beta[k] = parameters.undersubtraction;
    } else {
      const float normalized_value =
          (self->masking_thresholds[k] - min_masked_value) /
          (max_masked_value - min_masked_value);

      alpha[k] = (1.F - normalized_value) * parameters.oversubtraction +
                 normalized_value * self->alpha_minimun;
      beta[k] = (1.F - normalized_value) * parameters.undersubtraction +
                normalized_value * self->beta_minimun;
    }
  }
}