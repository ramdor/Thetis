cmake_minimum_required(VERSION 3.14.0)
set(CMAKE_OSX_DEPLOYMENT_TARGET 10.13 CACHE STRING "")

if(NOT vst3sdk_SOURCE_DIR)
    message(FATAL_ERROR "Path to VST3 SDK is empty! Please specify the vst3sdk_SOURCE_DIR cmake cache entry")
endif()

project(advanced-techniques-tutorial
    # This is your plug-in version number. Change it here only.
    # Version number symbols usable in C++ can be found in
    # source/version.h and ${PROJECT_BINARY_DIR}/projectversion.h.
    VERSION 1.0.0.0 
    DESCRIPTION "advanced-techniques-tutorial VST 3 Plug-in"
)

set(SMTG_ENABLE_VST3_HOSTING_EXAMPLES 0)
set(SMTG_ENABLE_VST3_PLUGIN_EXAMPLES 0)
set(SMTG_ENABLE_VSTGUI_SUPPORT 0)

set(SMTG_VSTGUI_ROOT "${vst3sdk_SOURCE_DIR}")

add_subdirectory(${vst3sdk_SOURCE_DIR} ${PROJECT_BINARY_DIR}/vst3sdk)
smtg_enable_vst3_sdk()

smtg_add_vst3plugin(advanced-techniques-tutorial
    README.md
    source/cids.h
    source/controller.cpp
    source/entry.cpp
    source/pids.h
    source/processor.cpp
    source/version.h
)

target_compile_features(advanced-techniques-tutorial
    PUBLIC
        cxx_std_17
)

target_link_libraries(advanced-techniques-tutorial
    PRIVATE
        sdk
)

smtg_target_configure_version_file(advanced-techniques-tutorial)

if(SMTG_MAC)
    smtg_target_set_bundle(advanced-techniques-tutorial
        BUNDLE_IDENTIFIER com.steinberg.vst3.tutorial.dataexchange
        COMPANY_NAME "Steinberg Media Technologies"
    )
    smtg_target_set_debug_executable(advanced-techniques-tutorial
        "/Applications/VST3PluginTestHost.app"
        "--pluginfolder;$(BUILT_PRODUCTS_DIR)"
    )
elseif(SMTG_WIN)
    target_sources(advanced-techniques-tutorial PRIVATE 
        resource/win32resource.rc
    )
    if(MSVC)
        set_property(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} PROPERTY VS_STARTUP_PROJECT advanced-techniques-tutorial)

        smtg_target_set_debug_executable(advanced-techniques-tutorial
            "$(ProgramW6432)/Steinberg/VST3PluginTestHost/VST3PluginTestHost.exe"
            "--pluginfolder \"$(OutDir)/\""
        )
    endif()
endif(SMTG_MAC)
