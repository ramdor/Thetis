// clang-format off
//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/interappaudio/InterAppAudioExample/VSTInterAppAudioHostUIControllerViewController.h
// Created by  : Steinberg, 08/2013
// Description :
// Flags       : clang-format SMTGSequencer
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------
// clang-format on

#import <UIKit/UIKit.h>

//------------------------------------------------------------------------
@interface VSTInterAppAudioHostUIControllerViewController : UIViewController
//------------------------------------------------------------------------

- (IBAction)hideView:(id)sender;

- (IBAction)switchToHost:(id)sender;
- (IBAction)play:(id)sender;
- (IBAction)rewind:(id)sender;
- (IBAction)record:(id)sender;

@end

