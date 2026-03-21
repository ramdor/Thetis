//------------------------------------------------------------------------
// Copyright(c) 2023 Steinberg Media Technologies.
//------------------------------------------------------------------------

#pragma once

#include "pluginterfaces/base/funknown.h"
#include "pluginterfaces/vst/vsttypes.h"

namespace Steinberg::Tutorial {
//------------------------------------------------------------------------
static const Steinberg::FUID kDataExchangeProcessorUID (0xE35B96A9, 0x9BB353A9, 0x910130C1, 0x2C55EC26);
static const Steinberg::FUID kDataExchangeControllerUID (0xADFD02D2, 0x20525504, 0x90D96683, 0xFD2DDF86);

#define DataExchangeVST3Category "Fx"

//------------------------------------------------------------------------
} // namespace Steinberg::Tutorial
