sndfile_dep = meson.get_compiler('c').find_library('sndfile',
                has_headers: ['sndfile.h'],
                required: true)

executable('adenoiser_demo',
  sources: 'adenoiser_demo.c',
  dependencies: [sndfile_dep, libspecbleach_dep],
  install: true)

executable('denoiser_demo',
  sources: 'denoiser_demo.c',
  dependencies: [sndfile_dep, libspecbleach_dep],
  install: true)
