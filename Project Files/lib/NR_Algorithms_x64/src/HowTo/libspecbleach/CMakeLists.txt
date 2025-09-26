# MW0LGE (c) 2025
cmake_minimum_required(VERSION 3.15)
project(libspecbleach C)

# 1) Use C99
set(CMAKE_C_STANDARD 99)
set(CMAKE_C_STANDARD_REQUIRED ON)

# 2) Build shared by default
option(BUILD_SHARED_LIBS "Build shared libraries" ON)

# 3) Pull in M_PI on MSVC/Clang-CL
if(MSVC OR CMAKE_C_COMPILER_ID STREQUAL "Clang")
  add_compile_definitions(_USE_MATH_DEFINES)
endif()

# 4) Where your FFTW3f lives
set(FFTW3F_ROOT
    #"C:/Users/Richie/Documents/Coding/Thetis/Project Files/lib/fftw_x64"
	"${CMAKE_CURRENT_LIST_DIR}/../../../fftw_x64"
    CACHE PATH "Path to your fftw_x64 folder (with fftw3.h, libfftw3f-3.lib, etc.)")

# 5) Collect all your .c files
file(GLOB_RECURSE LIBSPECBLEACH_SRCS
     "${CMAKE_CURRENT_LIST_DIR}/src/*.c")

# 6) Declare the shared library
add_library(specbleach SHARED ${LIBSPECBLEACH_SRCS})

# 7) Expose your own public headers
target_include_directories(specbleach
  PUBLIC
    "${CMAKE_CURRENT_LIST_DIR}/include"
)

# 8) Also add the FFTW3 include dir so <fftw3.h> is found
target_include_directories(specbleach
  PRIVATE
    "${FFTW3F_ROOT}"
)

# 9) Locate exactly the import-lib for FFTW3f
find_library(FFTW3F_LIB
  NAMES libfftw3f-3 fftw3f-3
  PATHS "${FFTW3F_ROOT}"
  NO_DEFAULT_PATH
)
if(NOT FFTW3F_LIB)
  message(FATAL_ERROR "Could not find libfftw3f-3.lib in ${FFTW3F_ROOT}")
endif()

# 10) Link against it
target_link_libraries(specbleach
  PRIVATE
    ${FFTW3F_LIB}
)

# 10b) auto–export every symbol, so the .lib actually contains all your functions
set_target_properties(specbleach PROPERTIES
  WINDOWS_EXPORT_ALL_SYMBOLS ON
)

# 11) Export your DLL symbols on Windows
target_compile_definitions(specbleach
  PRIVATE
    LIBSPECBLEACH_EXPORTS
)

# 12) (Optional) put your outputs in bin/ and lib/ under the build tree
set_target_properties(specbleach PROPERTIES
  RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
  ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib"
)
