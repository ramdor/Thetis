
// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com
#include "portaudiocpp/CallbackStream.hxx"

namespace portaudio {
CallbackStream::CallbackStream() {}

CallbackStream::~CallbackStream() {}

// -----------------------------------------------------------------------------------

double CallbackStream::cpuLoad() const {
    return Pa_GetStreamCpuLoad(stream_);
}

} // namespace portaudio
