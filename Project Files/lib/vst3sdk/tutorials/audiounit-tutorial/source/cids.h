//------------------------------------------------------------------------
// Copyright(c) 2024 Steinberg Media Technologies.
//------------------------------------------------------------------------

#pragma once

#include "pluginterfaces/base/funknown.h"
#include "pluginterfaces/vst/vsttypes.h"

namespace Steinberg::Vst {
//------------------------------------------------------------------------
static const Steinberg::FUID kVST3AUPlugInProcessorUID (0x301DF339, 0xAFA3533F, 0xB5053B1B, 0x41367137);
static const Steinberg::FUID kVST3AUPlugInControllerUID (0x473E512F, 0x68875B49, 0xB2EEFB2D, 0x886C33C6);

#define VST3AUPlugInVST3Category "Fx"

//------------------------------------------------------------------------
} // namespace Steinberg::Vst
