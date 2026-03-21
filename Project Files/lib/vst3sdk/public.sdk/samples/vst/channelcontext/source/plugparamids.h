//------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/channelcontext/source/plugparamids.h
// Created by  : Steinberg, 02/2014
// Description : define the parameter IDs used by channelcontext
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses. 
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#pragma once

enum
{
	/** parameter ID */
	kBypassId = 0, ///< Bypass value (we will handle the bypass process) (is automatable)

	kChannelUIDId, ///< read Only parameters
	kChannelUIDLengthId,
	kChannelNameId,
	kChannelNameLengthId,
	kChannelIndexNamespaceOrderId, 
	kChannelIndexNamespaceId,
	kChannelIndexNamespaceLengthId,
	kChannelColorId,
	kChannelIndexId,
	kChannelPluginLocationId
};
