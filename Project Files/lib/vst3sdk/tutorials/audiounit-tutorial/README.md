#  AudioUnit Version 2 Tutorial

In this tutorial you will learn how to add AudioUnit Version 2 support to your **VST 3** plug-in.

First of all, you need a **VST 3** plug-in project. For this tutorial we have generated one via the [VST 3 Project Generator](https://steinbergmedia.github.io/vst3_dev_portal/pages/What+is+the+VST+3+SDK/Project+Generator.html) from the SDK.

# Adding the AudioUnit Version 2 Target

## Obtaining the required AudioUnit SDK

The *AudioUnit Version 2* target needs the official *AudioUnit SDK* from Apple.
As of this writing you can find it on GitHub: [https://github.com/apple/AudioUnitSDK](https://github.com/apple/AudioUnitSDK)

How you obtain and store the SDK is up to you, for the reproducibility of this tutorial, we will download it via *CMake* when generating the project.
So we add the following text to the *CMakeLists.txt* directly before we include the **VST 3 SDK**.

```
include(FetchContent)
FetchContent_Declare(
	AudioUnitSDK
	GIT_REPOSITORY https://github.com/apple/AudioUnitSDK.git
	GIT_TAG        HEAD
)
FetchContent_MakeAvailable(AudioUnitSDK)
FetchContent_GetProperties(
	AudioUnitSDK
	SOURCE_DIR SMTG_AUDIOUNIT_SDK_PATH
)
```

It is important to set the `SMTG_AUDIOUNIT_SDK_PATH` variable to tell the **VST 3 SDK** where to find the AudioUnit SDK.

## Creating the property list

For *AudioUnit Version 2* you need a manufacturer OSType registered with Apple. 
How to do this is out of the scope for this tutorial, please search the web on how this is done.

Besides the manufacturer OSType you also need a subtype OSType which you can choose by yourself.
Both the manufacturer and subtype account for the uniqueness of your *AudioUnit Version 2*.

Now you can generate the required property list file:

```
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>CFBundleDevelopmentRegion</key>
	<string>English</string>
	<key>CFBundleExecutable</key>
	<string>$(EXECUTABLE_NAME)</string>
	<key>CFBundleIdentifier</key>
	<string>$(PRODUCT_BUNDLE_IDENTIFIER)</string>
	<key>CFBundleName</key>
	<string>$(PRODUCT_NAME)</string>
	<key>CFBundleInfoDictionaryVersion</key>
	<string>6.0</string>
	<key>CFBundlePackageType</key>
	<string>BNDL</string>
	<key>CFBundleSignature</key>
	<string>????</string>
	<key>CFBundleVersion</key>
	<string>1.0</string>
	<key>CSResourcesFileMapped</key>
	<string>yes</string>
	<key>AudioComponents</key>
	<array>
		<dict>
			<key>factoryFunction</key>
			<string>AUWrapperFactory</string>
			<key>description</key>
			<string>AudioUnit Tutorial</string>
			<key>manufacturer</key>
			<string>Stgb</string>
			<key>name</key>
			<string>Steinberg: AudioUnit Tutorial</string>
			<key>subtype</key>
			<string>0002</string>
			<key>type</key>
			<string>aufx</string>
			<key>version</key>
			<integer>1</integer>
		</dict>
	</array>
</dict>
</plist>
```

Make sure to change the strings for `description`, `manufacturer`, `name`, `subtype` and `type`.
The type must be one of:
- aufx (Audio Effect)
- aumu (Instrument)
- aumf (Audio Effect with MIDI Input/Output)

If you build an audio effect you also need to add the supported channel layouts to the list:

```
<key>AudioUnit SupportedNumChannels</key>
<array>
	<dict>
		<key>Outputs</key>
		<string>2</string>
		<key>Inputs</key>
		<string>2</string>
	</dict>
	<dict>
		<key>Outputs</key>
		<string>2</string>
		<key>Inputs</key>
		<string>1</string>
	</dict>
	<dict>
		<key>Outputs</key>
		<string>1</string>
		<key>Inputs</key>
		<string>1</string>
	</dict>
</array>
```

Save it to a file called `au-info.plist` inside the resource directory.

## Adding the AudioUnit Version 2 Target

Now you can add the AudioUnit target to the end of your *CMakeLists.txt* file:

```
if (SMTG_MAC AND XCODE AND SMTG_COREAUDIO_SDK_PATH)
	list(APPEND CMAKE_MODULE_PATH "${vst3sdk_SOURCE_DIR}/cmake/modules")
	include(SMTG_AddVST3AuV2)
	smtg_target_add_auv2(VST3_AU_PlugIn_AU
		BUNDLE_NAME audiounit_tutorial
		BUNDLE_IDENTIFIER com.steinberg.vst3sdk.audiounit_tutorial.audiounit
		INFO_PLIST_TEMPLATE ${CMAKE_CURRENT_SOURCE_DIR}/resource/au-info.plist
		VST3_PLUGIN_TARGET VST3_AU_PlugIn)
    smtg_target_set_debug_executable(VST3_AU_PlugIn_AU "/Applications/Reaper.app")
endif(SMTG_MAC AND XCODE AND SMTG_COREAUDIO_SDK_PATH)
```

Now after generating and building the project the "audiounit_tutorial" plug-in should be available in
any AudioUnit host.
