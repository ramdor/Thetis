//------------------------------------------------------------------------
// Copyright(c) 2023 Steinberg Media Technologies.
//------------------------------------------------------------------------

#pragma once

#include "pluginterfaces/base/funknown.h"
#include "pluginterfaces/vst/vsttypes.h"

namespace Steinberg::Tutorial {
//------------------------------------------------------------------------
static const FUID ProcessorUID (0xC18D3C1E, 0x719E4E29, 0x924D3ECA, 0xA5E4DA18);
static const FUID ControllerUID (0xC244B7E6, 0x24084E20, 0xA24A8C43, 0xF84C8BE8);

#define DataExchangeVST3Category "Fx"

//------------------------------------------------------------------------
} // namespace Steinberg::Tutorial

