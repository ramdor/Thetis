cmake_minimum_required(VERSION 3.14.0)
set(CMAKE_OSX_DEPLOYMENT_TARGET 10.13 CACHE STRING "")

if(NOT vst3sdk_SOURCE_DIR)
    message(FATAL_ERROR "Path to VST3 SDK is empty! Please specify the vst3sdk_SOURCE_DIR cmake cache entry")
endif()

project(dataexchange_tutorial
    # This is your plug-in version number. Change it here only.
    # Version number symbols usable in C++ can be found in
    # source/version.h and ${PROJECT_BINARY_DIR}/projectversion.h.
    VERSION 1.0.0.0 
    DESCRIPTION "dataexchange_tutorial VST 3 Plug-in"
)

set(SMTG_ENABLE_VST3_HOSTING_EXAMPLES 0)
set(SMTG_ENABLE_VST3_PLUGIN_EXAMPLES 0)
set(SMTG_ENABLE_VSTGUI_SUPPORT 0)

set(SMTG_VSTGUI_ROOT "${vst3sdk_SOURCE_DIR}")

add_subdirectory(${vst3sdk_SOURCE_DIR} ${PROJECT_BINARY_DIR}/vst3sdk)
smtg_enable_vst3_sdk()

smtg_add_vst3plugin(dataexchange_tutorial
    README.md
    source/cids.h
    source/controller.cpp
    source/controller.h
    source/dataexchange.h
    source/entry.cpp
    source/processor.cpp
    source/processor.h
    source/version.h
)

target_compile_features(dataexchange_tutorial
    PUBLIC
        cxx_std_17
)

target_link_libraries(dataexchange_tutorial
    PRIVATE
        sdk
)

smtg_target_configure_version_file(dataexchange_tutorial)

if(SMTG_MAC)
    smtg_target_set_bundle(dataexchange_tutorial
        BUNDLE_IDENTIFIER com.steinberg.vst3.tutorial.dataexchange
        COMPANY_NAME "Steinberg Media Technologies"
    )
    smtg_target_set_debug_executable(dataexchange_tutorial
        "/Applications/VST3PluginTestHost.app"
        "--pluginfolder;$(BUILT_PRODUCTS_DIR)"
    )
elseif(SMTG_WIN)
    target_sources(dataexchange_tutorial PRIVATE 
        resource/win32resource.rc
    )
    if(MSVC)
        set_property(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} PROPERTY VS_STARTUP_PROJECT dataexchange_tutorial)

        smtg_target_set_debug_executable(dataexchange_tutorial
            "$(ProgramW6432)/Steinberg/VST3PluginTestHost/VST3PluginTestHost.exe"
            "--pluginfolder \"$(OutDir)/\""
        )
    endif()
endif(SMTG_MAC)
