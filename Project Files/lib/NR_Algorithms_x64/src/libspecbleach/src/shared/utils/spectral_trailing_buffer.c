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

#include "spectral_trailing_buffer.h"
#include <math.h>
#include <stdlib.h>
#include <string.h>

struct SpectralTrailingBuffer {
  uint32_t real_spectrum_size;
  uint32_t buffer_size;

  float *buffer;
};

SpectralTrailingBuffer *
spectral_trailing_buffer_initialize(const uint32_t real_spectrum_size,
                                    const uint32_t buffer_size) {
  SpectralTrailingBuffer *self =
      (SpectralTrailingBuffer *)calloc(1U, sizeof(SpectralTrailingBuffer));

  self->real_spectrum_size = real_spectrum_size;
  self->buffer_size = buffer_size;

  self->buffer = (float *)calloc(
      ((size_t)self->real_spectrum_size * (size_t)self->buffer_size),
      sizeof(float));

  return self;
}

void spectral_trailing_buffer_free(SpectralTrailingBuffer *self) {
  free(self->buffer);

  free(self);
}

bool spectral_trailing_buffer_push_back(SpectralTrailingBuffer *self,
                                        const float *input_spectrum) {
  if (!input_spectrum) {
    return false;
  }

  memmove(self->buffer, &self->buffer[self->real_spectrum_size],
          sizeof(float) * self->real_spectrum_size * (self->buffer_size - 1U));
  memcpy(&self->buffer[(size_t)self->real_spectrum_size *
                       (size_t)(self->buffer_size - 1U)],
         input_spectrum, sizeof(float) * self->real_spectrum_size);

  return true;
}

float *get_trailing_spectral_buffer(SpectralTrailingBuffer *self) {
  return self->buffer;
}

uint32_t get_spectrum_buffer_size(SpectralTrailingBuffer *self) {
  return self->buffer_size;
}

uint32_t get_spectrum_size(SpectralTrailingBuffer *self) {
  return self->real_spectrum_size;
}