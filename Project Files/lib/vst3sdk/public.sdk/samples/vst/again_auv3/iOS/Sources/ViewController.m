//-----------------------------------------------------------------------------
// Project     : VST SDK
//
// Category    : Helpers
// Filename    : public.sdk/samples/vst/again_auv3/iOS/Sources/ViewController.m
// Created by  : Steinberg, 07/2017.
// Description : VST 3 - AUv3Wrapper
//
//-----------------------------------------------------------------------------
// This file is part of a Steinberg SDK. It is subject to the license terms
// in the LICENSE file found in the top-level directory of this distribution
// and at www.steinberg.net/sdklicenses.
// No part of the SDK, including this file, may be copied, modified, propagated,
// or distributed except according to the terms contained in the LICENSE file.
//-----------------------------------------------------------------------------

#import "ViewController.h"
#import <CoreAudioKit/AUViewController.h>
#import "public.sdk/source/vst/auv3wrapper/Shared/AUv3AudioEngine.h"
#import "public.sdk/source/vst/auv3wrapper/Shared/AUv3Wrapper.h"

@class AUv3WrapperViewController;

@interface ViewController () {
	// Button for playback
	IBOutlet UIButton *playButton;

	// Container for our custom view.
	__weak IBOutlet UIView *auContainerView;
	
	// Audio playback engine.
	AUv3AudioEngine *audioEngine;
	
	// Container for the custom view.
	AUv3WrapperViewController *auV3ViewController;
}

-(IBAction)togglePlay:(id)sender;
-(IBAction)loadFile:(id)sender;

@end

@implementation ViewController

//------------------------------------------------------------------------
- (void)viewDidLoad {
	[super viewDidLoad];
	
	// Do any additional setup after loading the view.
	[self embedPlugInView];

	AudioComponentDescription desc;

	desc.componentType = kAUcomponentType;
	desc.componentSubType = kAUcomponentSubType;
	desc.componentManufacturer = kAUcomponentManufacturer;
	desc.componentFlags = kAUcomponentFlags;
	desc.componentFlagsMask = kAUcomponentFlagsMask;

	[AUAudioUnit registerSubclass: AUv3Wrapper.class asComponentDescription:desc name:@"Local AUv3" version: UINT32_MAX];
	
	audioEngine = [[AUv3AudioEngine alloc] initWithComponentType:desc.componentType];
	
	[audioEngine loadAudioUnitWithComponentDescription:desc completion:^{
		auV3ViewController.audioUnit = (AUv3Wrapper*)audioEngine.currentAudioUnit;

		NSString* fileName = @kAudioFileName;
		NSString* fileFormat = @kAudioFileFormat;
		NSURL* fileURL = [[NSBundle mainBundle] URLForResource:fileName withExtension:fileFormat];
		NSError* error = [audioEngine loadAudioFile:fileURL];
		if (error)
		{
			NSLog (@"Error setting up audio or midi file: %@", [error description]);
		}
	}];
}

//------------------------------------------------------------------------
- (void)embedPlugInView {
	NSURL *builtInPlugInURL = [[NSBundle mainBundle] builtInPlugInsURL];
	NSURL *pluginURL = [builtInPlugInURL URLByAppendingPathComponent: @"AUv3WrapperiOSExtension.appex"];
	NSBundle *appExtensionBundle = [NSBundle bundleWithURL: pluginURL];
	
	auV3ViewController = [[AUv3WrapperViewController alloc] initWithNibName: @"AUv3WrapperViewController" bundle: appExtensionBundle];
	
	// Present the view controller's view.
	UIView *view = auV3ViewController.view;
	view.frame = auContainerView.bounds;
	
	[auContainerView addSubview: view];
	
	view.translatesAutoresizingMaskIntoConstraints = NO;
	
	NSArray *constraints = [NSLayoutConstraint constraintsWithVisualFormat: @"H:|-[view]-|" options:0 metrics:nil views:NSDictionaryOfVariableBindings(view)];
	[auContainerView addConstraints: constraints];
	
	constraints = [NSLayoutConstraint constraintsWithVisualFormat: @"V:|-[view]-|" options:0 metrics:nil views:NSDictionaryOfVariableBindings(view)];
	[auContainerView addConstraints: constraints];
}

//------------------------------------------------------------------------
- (void)didReceiveMemoryWarning {
	[super didReceiveMemoryWarning];
	// Dispose of any resources that can be recreated.
}

//------------------------------------------------------------------------
-(IBAction)loadFile:(id)sender {
	MPMediaPickerController *soundPicker=[[MPMediaPickerController alloc]
										  initWithMediaTypes:MPMediaTypeAnyAudio];
	soundPicker.delegate=self;
	soundPicker.allowsPickingMultipleItems=NO; // You can set it to yes for multiple selection
	[self presentViewController:soundPicker animated:YES completion:nil];
}

//------------------------------------------------------------------------
-(void)mediaPicker:(MPMediaPickerController *)mediaPicker didPickMediaItems:
(MPMediaItemCollection *)mediaItemCollection
{
	MPMediaItem *item = [[mediaItemCollection items] objectAtIndex:0]; // For multiple you can iterate iTems array
	NSURL *url = [item valueForProperty:MPMediaItemPropertyAssetURL];
	[mediaPicker dismissViewControllerAnimated:YES completion:nil];
	NSError* error = [audioEngine loadAudioFile:url];
	if (error != nil)
	{
		NSLog(@"something went wrong");
	}
}

- (void) mediaPickerDidCancel: (MPMediaPickerController *) mediaPicker
{
	[mediaPicker dismissViewControllerAnimated:YES completion:nil];
}

//------------------------------------------------------------------------
-(IBAction)togglePlay:(id)sender {	
	BOOL isPlaying = [audioEngine startStop];
	
	[playButton setTitle: isPlaying ? @"Stop" : @"Play" forState: UIControlStateNormal];
}

@end
