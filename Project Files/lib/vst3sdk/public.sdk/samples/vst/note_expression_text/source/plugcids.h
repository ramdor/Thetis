//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/note_expression_text/source/plugcids.h
// Created by  : Steinberg, 12/2007
// Description : define the class IDs
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

namespace Steinberg {
namespace Vst {

#if PLUGIN_A
	// Plug A
	static const FUID PlugProcessorUID (0x8F2B1ED9, 0x1D2C40E0, 0xB60F4AF2, 0x0F27711F);
	static const FUID PlugControllerUID (0x001B3315, 0x818044BE, 0x8FBF4AA2, 0xC9993C47);
#else
	// Plug B
	static const FUID PlugProcessorUID (0x1E4DC62B, 0x974147D6, 0x9CAB1B86, 0x50D4B11C);
	static const FUID PlugControllerUID (0xA8D238C1, 0x97F54908, 0xB32EB808, 0x3EA43CEC);

	// Plug B will support Plug A (could replace it when Plug B not present)
	static const FUID PlugAProcessorUID (0x8F2B1ED9, 0x1D2C40E0, 0xB60F4AF2, 0x0F27711F);

	static const FUID PluginCompatibilityUID (0x7A8EA518, 0xECA24036, 0x9AD492D1, 0x70AD04FA);
#endif

}} // namespaces
