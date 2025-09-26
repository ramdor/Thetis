subdir('shared')

subdir('processors')

specbleach_sources = [shared_sources, processors_sources]

# Build of the shared object
libspecbleach = library('specbleach',
  sources: specbleach_sources,
  c_args: lib_c_args,
  dependencies: dep,
  include_directories: inc,
  install: true)

# Generate package module configuration
pkg_mod = import('pkgconfig')
pkg_mod.generate(libraries: libspecbleach,
                 version: meson.project_version(),
                 name: 'libspecbleach',
                 description: 'C library for audio noise reduction and other spectral effects')

# Declare libspecbleach dependency
libspecbleach_dep = declare_dependency(include_directories: inc,
  link_with: libspecbleach)