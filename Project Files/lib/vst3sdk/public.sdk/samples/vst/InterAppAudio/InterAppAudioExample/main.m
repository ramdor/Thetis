//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/interappaudio/InterAppAudioExample/main.m
// Created by  : Steinberg, 08/2013
// Description :
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#import <UIKit/UIKit.h>

#import "VSTInterAppAudioAppDelegate.h"

int main(int argc, char * argv[])
{
	@autoreleasepool {
	    return UIApplicationMain (argc, argv, nil, NSStringFromClass ([VSTInterAppAudioAppDelegate class]));
	}
}
