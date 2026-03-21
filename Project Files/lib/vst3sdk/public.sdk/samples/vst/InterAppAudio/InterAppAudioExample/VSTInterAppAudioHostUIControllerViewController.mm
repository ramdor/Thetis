// clang-format off
//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/interappaudio/InterAppAudioExample/VSTInterAppAudioHostUIControllerViewController.mm
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

#import "VSTInterAppAudioHostUIControllerViewController.h"

#import "public.sdk/source/vst/interappaudio/AudioIO.h"

using namespace Steinberg::Vst::InterAppAudio;

//------------------------------------------------------------------------
static UIImage* scaleImageToSize (UIImage* image, CGSize newSize)
{
	UIGraphicsBeginImageContextWithOptions (newSize, NO, 0.0);
	[image drawInRect:CGRectMake (0, 0, newSize.width, newSize.height)];
	UIImage* newImage = UIGraphicsGetImageFromCurrentImageContext ();
	UIGraphicsEndImageContext ();

	return newImage;
}

//------------------------------------------------------------------------
@interface VSTInterAppAudioHostUIControllerViewController ()
//------------------------------------------------------------------------
@property (assign) IBOutlet UIButton* hostButton;

@end

//------------------------------------------------------------------------
@implementation VSTInterAppAudioHostUIControllerViewController
//------------------------------------------------------------------------

//------------------------------------------------------------------------
- (id)init
{
	self = [super initWithNibName:@"VSTInterAppAudioHostUIControllerView" bundle:nil];
	if (self)
	{
	}
	return self;
}

//------------------------------------------------------------------------
- (void)viewDidLoad
{
	[super viewDidLoad];

	UIImage* image = AudioIO::instance ()->getHostIcon ();
	if (image)
	{
		image = scaleImageToSize (image, self.hostButton.bounds.size);
		[self.hostButton setTitle:@"" forState:UIControlStateNormal];
		[self.hostButton setImage:image forState:UIControlStateNormal];
	}

	self.view.layer.shadowColor = [[UIColor blackColor] CGColor];
	self.view.layer.shadowOpacity = 0.5;
	self.view.layer.shadowRadius = 5;
	self.view.layer.shadowOffset = CGSizeMake (5, -5);
}

//------------------------------------------------------------------------
- (IBAction)hideView:(id)sender
{
	[UIView animateWithDuration:0.3
	    animations:^{
		  CGRect frame = self.view.frame;
		  frame.origin.y += frame.size.height;
		  self.view.frame = frame;
	    }
	    completion:^(BOOL finished) {
		  [self.view removeFromSuperview];
		  [self removeFromParentViewController];
	    }];
}

//------------------------------------------------------------------------
- (IBAction)switchToHost:(id)sender
{
	AudioIO::instance ()->switchToHost ();
}

//------------------------------------------------------------------------
- (IBAction)play:(id)sender
{
	AudioIO::instance ()->sendRemoteControlEvent (kAudioUnitRemoteControlEvent_TogglePlayPause);
}

//------------------------------------------------------------------------
- (IBAction)rewind:(id)sender
{
	AudioIO::instance ()->sendRemoteControlEvent (kAudioUnitRemoteControlEvent_Rewind);
}

//------------------------------------------------------------------------
- (IBAction)record:(id)sender
{
	AudioIO::instance ()->sendRemoteControlEvent (kAudioUnitRemoteControlEvent_ToggleRecord);
}

@end
