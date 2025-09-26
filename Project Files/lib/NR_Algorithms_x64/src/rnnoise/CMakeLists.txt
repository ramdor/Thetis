cmake_minimum_required(VERSION 3.15)
project(rnnoise C)

# Use the DLL CRT (/MD) on Windows so all DLLs share one heap
if (MSVC OR CMAKE_C_COMPILER_ID STREQUAL "Clang")
  set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreadedDLL" CACHE STRING "" FORCE)
  add_compile_definitions(_USE_MATH_DEFINES)
endif()

option(BUILD_SHARED_LIBS "Produce a shared library" ON)

# Collect every .c under src/
file(GLOB_RECURSE RNNOISE_SOURCES
    "${CMAKE_CURRENT_SOURCE_DIR}/src/*.c"
)

if (WIN32)
  # drop POSIX/sample binaries
  list(FILTER RNNOISE_SOURCES EXCLUDE REGEX ".+[\\\\/]dump_rnnoise_tables\\.c$")
  list(FILTER RNNOISE_SOURCES EXCLUDE REGEX ".+[\\\\/]write_weights\\.c$")
  # drop the big-endian tables on little-endian Windows
  list(FILTER RNNOISE_SOURCES EXCLUDE REGEX ".+[\\\\/]rnnoise_data\\.c$")
  # drop POSIX-only unistd.h usage
  list(FILTER RNNOISE_SOURCES EXCLUDE REGEX ".+[\\\\/]dump_features\\.c$")
  # drop x86-asm kernels (we’ll use the plain-C)
  list(FILTER RNNOISE_SOURCES EXCLUDE REGEX ".+[\\\\/]src[\\\\/]x86[\\\\/].+\\.c$")
endif()

add_library(rnnoise SHARED ${RNNOISE_SOURCES})

# Export every public symbol
set_target_properties(rnnoise PROPERTIES
  WINDOWS_EXPORT_ALL_SYMBOLS ON
)

target_include_directories(rnnoise PUBLIC
  "${CMAKE_CURRENT_SOURCE_DIR}/include"
)

if (WIN32)
  target_compile_definitions(rnnoise PRIVATE
    RNNOISE_EXPORTS
    RNNOISE_BUILD
    DLL_EXPORT
  )
endif()

# Match Linux fast-math on Windows
if (CMAKE_C_COMPILER_ID STREQUAL "Clang")
  target_compile_options(rnnoise PRIVATE
    -Ofast -ffast-math -fno-math-errno -fno-trapping-math -fno-signaling-nans
  )
elseif (MSVC)
  target_compile_options(rnnoise PRIVATE
    /O2 /fp:fast /arch:SSE2
  )
endif()
