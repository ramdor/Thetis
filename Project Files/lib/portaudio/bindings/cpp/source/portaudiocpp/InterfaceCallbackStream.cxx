// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com

#include "portaudiocpp/InterfaceCallbackStream.hxx"
#include "portaudiocpp/StreamParameters.hxx"
#include "portaudiocpp/Exception.hxx"
#include "portaudiocpp/CallbackInterface.hxx"

namespace portaudio {

// ---------------------------------------------------------------------------------==

InterfaceCallbackStream::InterfaceCallbackStream() {}

InterfaceCallbackStream::InterfaceCallbackStream(
    const StreamParameters& parameters, CallbackInterface& instance) {
    open(parameters, instance);
}

InterfaceCallbackStream::~InterfaceCallbackStream() {
    try {
        close();
    } catch (...) {
        // ignore all errors
    }
}

// ---------------------------------------------------------------------------------==

void InterfaceCallbackStream::open(
    const StreamParameters& parameters, CallbackInterface& instance) {
    PaError err = Pa_OpenStream(&stream_,
        parameters.inputParameters().paStreamParameters(),
        parameters.outputParameters().paStreamParameters(),
        parameters.sampleRate(), parameters.framesPerBuffer(),
        parameters.flags(), &impl::callbackInterfaceToPaCallbackAdapter,
        static_cast<void*>(&instance));

    if (err != paNoError) {
        throw PaException(err);
    }
}
} // namespace portaudio
