#!/bin/sh

#//------------------------------------------------------------------------
#// Project     : VST SDK
#//
#// Category    : Examples
#// Filename    : public.sdk/samples/vst/mac/make_vst3_symbolic_link.sh
#// Created by  : Steinberg, 06/2009
#// Description : places a symbolic link for the VST 3 Plug-In into the users VST3 folder.
#//				  To be used as "Run Script" build phase in Xcode
#//
#//-----------------------------------------------------------------------------
#// This file is part of a Steinberg SDK. It is subject to the license terms
#// in the LICENSE file found in the top-level directory of this distribution
#// and at www.steinberg.net/sdklicenses.
#// No part of the SDK, including this file, may be copied, modified, propagated,
#// or distributed except according to the terms contained in the LICENSE file.
#//-----------------------------------------------------------------------------

if [[ "$CONFIGURATION" == "Debug" ]]
then
	mkdir -p ~/Library/Audio/Plug-Ins/VST3/
	cd ~/Library/Audio/Plug-Ins/VST3/
	ln -f -s "$BUILT_PRODUCTS_DIR/$WRAPPER_NAME" .
fi
