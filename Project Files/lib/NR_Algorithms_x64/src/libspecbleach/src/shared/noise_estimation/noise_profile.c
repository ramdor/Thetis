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

#include "noise_profile.h"
#include "../configurations.h"
#include "../utils/spectral_utils.h"
#include <stdlib.h>
#include <string.h>

struct NoiseProfile {
  uint32_t noise_profile_size;
  uint32_t noise_profile_blocks_averaged;
  float *noise_profile;
  bool noise_spectrum_available;
};

NoiseProfile *noise_profile_initialize(const uint32_t size) {
  NoiseProfile *self = (NoiseProfile *)calloc(1U, sizeof(NoiseProfile));
  self->noise_profile_size = size;
  self->noise_profile_blocks_averaged = 0U;
  self->noise_spectrum_available = false;

  self->noise_profile = (float *)calloc(size, sizeof(float));

  return self;
}

void noise_profile_free(NoiseProfile *self) {
  free(self->noise_profile);

  free(self);
}

bool is_noise_estimation_available(NoiseProfile *self) {
  return self->noise_spectrum_available;
}

float *get_noise_profile(NoiseProfile *self) { return self->noise_profile; }

uint32_t get_noise_profile_size(NoiseProfile *self) {
  return self->noise_profile_size;
}

uint32_t get_noise_profile_blocks_averaged(NoiseProfile *self) {
  return self->noise_profile_blocks_averaged;
}
void set_noise_profile_available(NoiseProfile *self) {
  self->noise_spectrum_available = true;
}

bool set_noise_profile(NoiseProfile *self, const float *noise_profile,
                       const uint32_t noise_profile_size,
                       const uint32_t noise_profile_blocks_averaged) {
  if (!self || !noise_profile ||
      noise_profile_size != self->noise_profile_size) {
    return false;
  }
  memcpy(self->noise_profile, noise_profile,
         noise_profile_size * sizeof(float));

  self->noise_profile_size = noise_profile_size;
  self->noise_profile_blocks_averaged = noise_profile_blocks_averaged;
  self->noise_spectrum_available = true;

  return true;
}

bool increment_blocks_averaged(NoiseProfile *self) {
  if (!self) {
    return false;
  }

  self->noise_profile_blocks_averaged++;

  if (self->noise_profile_blocks_averaged >
          MIN_NUMBER_OF_WINDOWS_NOISE_AVERAGED &&
      !self->noise_spectrum_available) {
    self->noise_spectrum_available = true;
  }

  return true;
}

bool reset_noise_profile(NoiseProfile *self) {
  if (!self) {
    return false;
  }

  initialize_spectrum_with_value(self->noise_profile, self->noise_profile_size,
                                 0.F);
  self->noise_profile_blocks_averaged = 0U;
  self->noise_spectrum_available = false;

  return true;
}