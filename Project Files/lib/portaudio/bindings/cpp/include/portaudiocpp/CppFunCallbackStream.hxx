// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com
#ifndef INCLUDED_PORTAUDIO_CPPFUNCALLBACKSTREAM_HXX
#define INCLUDED_PORTAUDIO_CPPFUNCALLBACKSTREAM_HXX

// ---------------------------------------------------------------------------------------

#include "portaudio.h"

#include "portaudiocpp/CallbackStream.hxx"

// ---------------------------------------------------------------------------------------

// Forward declaration(s):
namespace portaudio {
class StreamParameters;
}

// ---------------------------------------------------------------------------------------

// Declaration(s):
namespace portaudio {

namespace impl {
    extern "C" {
    int cppCallbackToPaCallbackAdapter(const void* inputBuffer,
        void* outputBuffer, unsigned long numFrames,
        const PaStreamCallbackTimeInfo* timeInfo,
        PaStreamCallbackFlags statusFlags, void* userData);
    } // extern "C"
} // namespace impl

// -----------------------------------------------------------------------------------

//////
/// @brief Callback stream using a C++ function (either a free function or a
/// static function) callback.
//////
class FunCallbackStream : public CallbackStream {
    public:
    typedef int (*CallbackFunPtr)(const void* inputBuffer, void* outputBuffer,
        unsigned long numFrames, const PaStreamCallbackTimeInfo* timeInfo,
        PaStreamCallbackFlags statusFlags, void* userData);

    // -------------------------------------------------------------------------------

    //////
    /// @brief Simple structure containing a function pointer to the C++
    /// callback function and a (void) pointer to the user supplied data.
    //////
    struct CppToCCallbackData {
        CppToCCallbackData();
        CppToCCallbackData(CallbackFunPtr funPtr, void* userData);
        void init(CallbackFunPtr funPtr, void* userData);

        CallbackFunPtr funPtr = nullptr;
        void* userData = nullptr;
    };

    // -------------------------------------------------------------------------------

    FunCallbackStream();
    FunCallbackStream(const StreamParameters& parameters, CallbackFunPtr funPtr,
        void* userData);
    ~FunCallbackStream();

    void open(const StreamParameters& parameters, CallbackFunPtr funPtr,
        void* userData);

    private:
    FunCallbackStream(const FunCallbackStream&); // non-copyable
    FunCallbackStream& operator=(const FunCallbackStream&); // non-copyable

    CppToCCallbackData adapterData_;

    void open(const StreamParameters& parameters);
};

} // namespace portaudio

// ---------------------------------------------------------------------------------------

#endif // INCLUDED_PORTAUDIO_CPPFUNCALLBACKSTREAM_HXX
