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

#include "adaptive_denoiser.h"
#include "../../shared/configurations.h"
#include "../../shared/gain_estimation/gain_estimators.h"
#include "../../shared/noise_estimation/adaptive_noise_estimator.h"
#include "../../shared/post_estimation/postfilter.h"
#include "../../shared/pre_estimation/critical_bands.h"
#include "../../shared/pre_estimation/noise_scaling_criterias.h"
#include "../../shared/pre_estimation/spectral_smoother.h"
#include "../../shared/utils/denoise_mixer.h"
#include "../../shared/utils/spectral_features.h"
#include "../../shared/utils/spectral_utils.h"
#include <float.h>
#include <math.h>
#include <stdlib.h>
#include <string.h>

typedef struct SpectralAdaptiveDenoiser {
  uint32_t fft_size;
  uint32_t real_spectrum_size;
  uint32_t sample_rate;
  uint32_t hop;
  float default_oversubtraction;
  float default_undersubtraction;

  AdaptiveDenoiserParameters parameters;

  float *alpha;
  float *beta;
  float *gain_spectrum;
  float *residual_spectrum;
  float *denoised_spectrum;
  float *noise_profile;

  SpectrumType spectrum_type;
  CriticalBandType band_type;
  GainEstimationType gain_estimation_type;
  TimeSmoothingType time_smoothing_type;

  DenoiseMixer *mixer;
  NoiseScalingCriterias *noise_scaling_criteria;
  SpectralSmoother *spectrum_smoothing;
  PostFilter *postfiltering;
  AdaptiveNoiseEstimator *adaptive_estimator;
  SpectralFeatures *spectral_features;
} SpectralAdaptiveDenoiser;

SpectralProcessorHandle
spectral_adaptive_denoiser_initialize(const uint32_t sample_rate,
                                      const uint32_t fft_size,
                                      const uint32_t overlap_factor) {

  SpectralAdaptiveDenoiser *self =
      (SpectralAdaptiveDenoiser *)calloc(1U, sizeof(SpectralAdaptiveDenoiser));

  self->fft_size = fft_size;
  self->real_spectrum_size = self->fft_size / 2U + 1U;
  self->sample_rate = sample_rate;
  self->hop = self->fft_size / overlap_factor;
  self->default_oversubtraction = DEFAULT_OVERSUBTRACTION;
  self->default_undersubtraction = DEFAULT_UNDERSUBTRACTION;
  self->spectrum_type = SPECTRAL_TYPE_SPEECH;
  self->band_type = CRITICAL_BANDS_TYPE_SPEECH;
  self->gain_estimation_type = GAIN_ESTIMATION_TYPE_SPEECH;
  self->time_smoothing_type = TIME_SMOOTHING_TYPE_SPEECH;

  self->gain_spectrum = (float *)calloc(self->fft_size, sizeof(float));
  initialize_spectrum_with_value(self->gain_spectrum, self->fft_size, 1.F);
  self->alpha = (float *)calloc(self->real_spectrum_size, sizeof(float));
  initialize_spectrum_with_value(self->alpha, self->real_spectrum_size, 1.F);
  self->beta = (float *)calloc(self->real_spectrum_size, sizeof(float));
  self->noise_profile =
      (float *)calloc(self->real_spectrum_size, sizeof(float));

  self->adaptive_estimator = louizou_estimator_initialize(
      self->real_spectrum_size, sample_rate, fft_size);

  self->residual_spectrum = (float *)calloc((self->fft_size), sizeof(float));
  self->denoised_spectrum = (float *)calloc((self->fft_size), sizeof(float));

  self->postfiltering = postfilter_initialize(self->fft_size);

  self->spectrum_smoothing =
      spectral_smoothing_initialize(self->fft_size, self->time_smoothing_type);

  self->noise_scaling_criteria = noise_scaling_criterias_initialize(
      self->fft_size, self->band_type, self->sample_rate, self->spectrum_type);

  self->spectral_features =
      spectral_features_initialize(self->real_spectrum_size);

  self->mixer =
      denoise_mixer_initialize(self->fft_size, self->sample_rate, self->hop);

  return self;
}

void spectral_adaptive_denoiser_free(SpectralProcessorHandle instance) {
  SpectralAdaptiveDenoiser *self = (SpectralAdaptiveDenoiser *)instance;

  louizou_estimator_free(self->adaptive_estimator);
  spectral_features_free(self->spectral_features);
  noise_scaling_criterias_free(self->noise_scaling_criteria);
  spectral_smoothing_free(self->spectrum_smoothing);
  postfilter_free(self->postfiltering);
  denoise_mixer_free(self->mixer);

  free(self->residual_spectrum);
  free(self->denoised_spectrum);
  free(self->noise_profile);
  free(self->gain_spectrum);
  free(self->alpha);
  free(self->beta);

  free(self);
}

bool load_adaptive_reduction_parameters(SpectralProcessorHandle instance,
                                        AdaptiveDenoiserParameters parameters) {
  if (!instance) {
    return false;
  }

  SpectralAdaptiveDenoiser *self = (SpectralAdaptiveDenoiser *)instance;
  self->parameters = parameters;

  return true;
}

bool spectral_adaptive_denoiser_run(SpectralProcessorHandle instance,
                                    float *fft_spectrum) {
  if (!fft_spectrum || !instance) {
    return false;
  }

  SpectralAdaptiveDenoiser *self = (SpectralAdaptiveDenoiser *)instance;

  float *reference_spectrum =
      get_spectral_feature(self->spectral_features, fft_spectrum,
                           self->fft_size, self->spectrum_type);

  // Estimate noise
  louizou_estimator_run(self->adaptive_estimator, reference_spectrum,
                        self->noise_profile);

  // Scale estimated noise profile for oversubtraction
  NoiseScalingParameters oversubtraction_parameters = (NoiseScalingParameters){
      .oversubtraction =
          self->default_oversubtraction + self->parameters.noise_rescale,
      .undersubtraction = self->default_undersubtraction,
      .scaling_type = self->parameters.noise_scaling_type,
  };
  apply_noise_scaling_criteria(self->noise_scaling_criteria, reference_spectrum,
                               self->noise_profile, self->alpha, self->beta,
                               oversubtraction_parameters);

  TimeSmoothingParameters spectral_smoothing_parameters =
      (TimeSmoothingParameters){
          .smoothing = self->parameters.smoothing_factor,
      };
  spectral_smoothing_run(self->spectrum_smoothing,
                         spectral_smoothing_parameters, reference_spectrum);

  // Get reduction gain weights
  estimate_gains(self->real_spectrum_size, self->fft_size, reference_spectrum,
                 self->noise_profile, self->gain_spectrum, self->alpha,
                 self->beta, self->gain_estimation_type);

  // Apply post filtering to reduce residual noise on low SNR frames
  PostFiltersParameters post_filter_parameters = (PostFiltersParameters){
      .snr_threshold = self->parameters.post_filter_threshold,
  };
  postfilter_apply(self->postfiltering, fft_spectrum, self->gain_spectrum,
                   post_filter_parameters);

  // Mix results
  DenoiseMixerParameters mixer_parameters = (DenoiseMixerParameters){
      .noise_level = self->parameters.reduction_amount,
      .residual_listen = self->parameters.residual_listen,
      .whitening_amount = self->parameters.whitening_factor,
  };

  denoise_mixer_run(self->mixer, fft_spectrum, self->gain_spectrum,
                    mixer_parameters);

  return true;
}