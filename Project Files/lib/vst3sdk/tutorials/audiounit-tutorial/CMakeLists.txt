cmake_minimum_required(VERSION 3.14.0)

option(SMTG_ENABLE_VST3_PLUGIN_EXAMPLES "Enable VST 3 Plug-in Examples" OFF)
option(SMTG_ENABLE_VST3_HOSTING_EXAMPLES "Enable VST 3 Hosting Examples" OFF)

set(CMAKE_OSX_DEPLOYMENT_TARGET 10.13 CACHE STRING "")

set(vst3sdk_SOURCE_DIR "../../")
if(NOT vst3sdk_SOURCE_DIR)
    message(FATAL_ERROR "Path to VST3 SDK is empty!")
endif()

project(VST3_AU_PlugIn
    # This is your plug-in version number. Change it here only.
    # Version number symbols usable in C++ can be found in
    # source/version.h and ${PROJECT_BINARY_DIR}/projectversion.h.
    VERSION 1.2.1.8 
    DESCRIPTION "VST3_AU_PlugIn VST 3 Plug-in"
)

# -- AudioUnitSDK --
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
# -------------------

set(SMTG_VSTGUI_ROOT "${vst3sdk_SOURCE_DIR}")

add_subdirectory(${vst3sdk_SOURCE_DIR} ${PROJECT_BINARY_DIR}/vst3sdk)
smtg_enable_vst3_sdk()

smtg_add_vst3plugin(VST3_AU_PlugIn
    source/version.h
    source/cids.h
    source/processor.h
    source/processor.cpp
    source/controller.h
    source/controller.cpp
    source/entry.cpp
)

#- VSTGUI Wanted ----
if(SMTG_ENABLE_VSTGUI_SUPPORT)
    target_sources(VST3_AU_PlugIn
        PRIVATE
            resource/editor.uidesc
    )
    target_link_libraries(VST3_AU_PlugIn
        PRIVATE
            vstgui_support
    )
    smtg_target_add_plugin_resources(VST3_AU_PlugIn
        RESOURCES
            "resource/editor.uidesc"
    )
endif(SMTG_ENABLE_VSTGUI_SUPPORT)
# -------------------

smtg_target_add_plugin_snapshots (VST3_AU_PlugIn
    RESOURCES
        resource/301DF339AFA3533FB5053B1B41367137_snapshot.png
        resource/301DF339AFA3533FB5053B1B41367137_snapshot_2.0x.png
)

target_link_libraries(VST3_AU_PlugIn
    PRIVATE
        sdk
)

smtg_target_configure_version_file(VST3_AU_PlugIn)

if(SMTG_MAC)
    smtg_target_set_bundle(VST3_AU_PlugIn
        BUNDLE_IDENTIFIER com.steinberg.vst3sdk.audiounit-tutorial
        COMPANY_NAME "Steinberg Media Technologies"
    )
    smtg_target_set_debug_executable(VST3_AU_PlugIn
        "/Applications/VST3PluginTestHost.app"
        "--pluginfolder;$(BUILT_PRODUCTS_DIR)"
    )
elseif(SMTG_WIN)
    target_sources(VST3_AU_PlugIn PRIVATE 
        resource/win32resource.rc
    )
    if(MSVC)
        set_property(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} PROPERTY VS_STARTUP_PROJECT VST3_AU_PlugIn)

        smtg_target_set_debug_executable(VST3_AU_PlugIn
            "$(ProgramW6432)/Steinberg/VST3PluginTestHost/VST3PluginTestHost.exe"
            "--pluginfolder \"$(OutDir)/\""
        )
    endif()
endif(SMTG_MAC)

# -- Add the AUv2 target
if (SMTG_MAC AND XCODE AND SMTG_ENABLE_AUV2_BUILDS)
    list(APPEND CMAKE_MODULE_PATH "${vst3sdk_SOURCE_DIR}/cmake/modules")
    include(SMTG_AddVST3AuV2)
    smtg_target_add_auv2(VST3_AU_PlugIn_AU
        BUNDLE_NAME audiounit_tutorial
        BUNDLE_IDENTIFIER com.steinberg.vst3sdk.audiounit_tutorial.audiounit
        INFO_PLIST_TEMPLATE ${CMAKE_CURRENT_SOURCE_DIR}/resource/au-info.plist
        VST3_PLUGIN_TARGET VST3_AU_PlugIn)
    smtg_target_set_debug_executable(VST3_AU_PlugIn_AU "/Applications/Reaper.app")
endif(SMTG_MAC AND XCODE AND SMTG_ENABLE_AUV2_BUILDS)

