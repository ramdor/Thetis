// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com

#ifndef INCLUDED_PORTAUDIO_ASIODEVICEADAPTER_HXX
#define INCLUDED_PORTAUDIO_ASIODEVICEADAPTER_HXX

namespace portaudio {

// Forward declaration(s):
class Device;

// Declaration(s):
//////
/// @brief Adapts the given Device to an ASIO specific extension.
///
/// Deleting the AsioDeviceAdapter does not affect the underlying
/// Device.
//////
class AsioDeviceAdapter {
    public:
    AsioDeviceAdapter(Device& device);

    Device& device();

    long minBufferSize() const;
    long maxBufferSize() const;
    long preferredBufferSize() const;
    long granularity() const;

    void showControlPanel(void* systemSpecific);

    const char* inputChannelName(int channelIndex) const;
    const char* outputChannelName(int channelIndex) const;

    private:
    Device* device_ = 0;

    long minBufferSize_ = 0;
    long maxBufferSize_ = 0;
    long preferredBufferSize_ = 0;
    long granularity_ = 0;
};
} // namespace portaudio

#endif // INCLUDED_PORTAUDIO_ASIODEVICEADAPTER_HXX
