// clang-format off
//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Examples
// Filename    : public.sdk/samples/vst/interappaudio/InterAppAudioExample/VSTInterAppAudioAppDelegate.mm
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

#import "VSTInterAppAudioAppDelegate.h"

#import "VSTInterAppAudioHostUIControllerViewController.h"
#import "public.sdk/source/vst/interappaudio/AudioIO.h"

using namespace Steinberg::Vst::InterAppAudio;

//------------------------------------------------------------------------
@interface VSTInterAppAudioAppDelegate ()
//------------------------------------------------------------------------
{
	UIButton* showHostView;
}
@end

//------------------------------------------------------------------------
@implementation VSTInterAppAudioAppDelegate
//------------------------------------------------------------------------

//------------------------------------------------------------------------
- (BOOL)application:(UIApplication*)application
    didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
	if ([super application:application didFinishLaunchingWithOptions:launchOptions])
	{
		[self performSelector:@selector (createShowHostViewButton) withObject:nil afterDelay:0.2];
		[[NSNotificationCenter defaultCenter] addObserver:self
		                                         selector:@selector (audioIOConnectionChanged)
		                                             name:AudioIO::kConnectionStateChange
		                                           object:nil];
		return YES;
	}
	return NO;
}

//------------------------------------------------------------------------
- (NSUInteger)application:(UIApplication*)application
    supportedInterfaceOrientationsForWindow:(UIWindow*)window
{
	return UIInterfaceOrientationMaskLandscapeLeft | UIInterfaceOrientationMaskLandscapeRight;
}

//------------------------------------------------------------------------
- (void)audioIOConnectionChanged
{
	showHostView.hidden = AudioIO::instance ()->getInterAppAudioConnected () == false;
	if (showHostView.hidden)
	{
		for (id childController in [self.window.rootViewController childViewControllers])
		{
			if ([childController
			        isKindOfClass:[VSTInterAppAudioHostUIControllerViewController class]])
			{
				[childController hideView:self];
			}
		}
	}
}

//------------------------------------------------------------------------
- (void)createShowHostViewButton
{
	showHostView = [UIButton buttonWithType:UIButtonTypeInfoDark];
	[showHostView addTarget:self
	                 action:@selector (showHostViewAction:)
	       forControlEvents:UIControlEventTouchDown];

	const CGFloat margin = 15;
	CGRect frame = showHostView.frame;
	frame.origin.y =
	    [self.window.rootViewController.view bounds].size.height - (frame.size.height + margin);
	frame.origin.x = margin;
	showHostView.frame = frame;

	[self.window.rootViewController.view addSubview:showHostView];

	if (AudioIO::instance ()->getInterAppAudioConnected () == false)
	{
		showHostView.hidden = YES;
	}
}

//------------------------------------------------------------------------
- (void)showHostViewAction:(id)sender
{
	VSTInterAppAudioHostUIControllerViewController* controller =
	    [[VSTInterAppAudioHostUIControllerViewController alloc] init];
	[self.window.rootViewController addChildViewController:controller];
	CGRect frame = controller.view.frame;
	frame.origin.y = [self.window.rootViewController.view bounds].size.height;
	controller.view.frame = frame;

	[self.window.rootViewController.view addSubview:controller.view];

	frame.origin.y = [self.window.rootViewController.view bounds].size.height - frame.size.height;

	[UIView animateWithDuration:0.3 animations:^{ controller.view.frame = frame; }];
}

@end
