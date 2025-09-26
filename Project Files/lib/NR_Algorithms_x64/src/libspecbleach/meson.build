project('libspecbleach', 'c', version: '0.1.7', default_options: ['c_std=c99'])

# Host OS / Arch
current_os   = host_machine.system()
current_arch = build_machine.cpu_family()

# Shared C args for libraries
lib_c_args = []
if current_arch == 'x86' or (current_arch == 'x86_64' and current_os != 'darwin')
  lib_c_args += [
    '-msse', '-msse2', '-mfpmath=sse',
    '-ffast-math', '-fomit-frame-pointer',
    '-fno-finite-math-only'
  ]
endif

# Compiler handle
cc = meson.get_compiler('c')

# Math in CRT on Windows
m_dep = cc.find_library('m', required: false)

# FFTW3f: on Windows pull in the vcpkg-built libfftw3f-3.lib,
# elsewhere use pkg-config dependency
if host_machine.system() == 'windows'
  fftw_dep = cc.find_library(
    'libfftw3f-3',
    dirs: ['D:/TestBuild/vcpkg/installed/x64-windows/lib'],
    required: true
  )
  inc_dirs = include_directories(
    'include',
    'D:/TestBuild/vcpkg/installed/x64-windows/include'
  )
else
  fftw_dep = dependency('fftw3f', required: true)
  inc_dirs  = include_directories('include')
endif

# Combined link deps
dep = [ m_dep, fftw_dep ]

# Public headers
subdir('include')

# Library sources (will pick up inc_dirs & dep in src/meson.build)
subdir('src')

# Optional examples
if get_option('enable_examples')
  subdir('example')
endif
