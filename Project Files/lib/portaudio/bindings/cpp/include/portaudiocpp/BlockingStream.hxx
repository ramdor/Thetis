// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com
#ifndef INCLUDED_PORTAUDIO_BLOCKINGSTREAM_HXX
#define INCLUDED_PORTAUDIO_BLOCKINGSTREAM_HXX

// ---------------------------------------------------------------------------------------

#include "portaudiocpp/Stream.hxx"

// ---------------------------------------------------------------------------------------

namespace portaudio {

//////
/// @brief Stream class for blocking read/write-style input and output.
//////
class BlockingStream : public Stream {
    public:
    BlockingStream();
    BlockingStream(const StreamParameters& parameters);
    ~BlockingStream();

    void open(const StreamParameters& parameters);

    void read(void* buffer, unsigned long numFrames);
    void write(const void* buffer, unsigned long numFrames);

    signed long availableReadSize() const;
    signed long availableWriteSize() const;

    private:
    BlockingStream(const BlockingStream&); // non-copyable
    BlockingStream& operator=(const BlockingStream&); // non-copyable
};

} // namespace portaudio

// ---------------------------------------------------------------------------------------

#endif // INCLUDED_PORTAUDIO_BLOCKINGSTREAM_HXX
