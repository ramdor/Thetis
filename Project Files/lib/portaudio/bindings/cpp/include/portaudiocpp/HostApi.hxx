// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com

#ifndef INCLUDED_PORTAUDIO_HOSTAPI_HXX
#define INCLUDED_PORTAUDIO_HOSTAPI_HXX

// ---------------------------------------------------------------------------------------

#include "portaudio.h"

#include "portaudiocpp/System.hxx"

// ---------------------------------------------------------------------------------------

// Forward declaration(s):
namespace portaudio {
class Device;
}

// ---------------------------------------------------------------------------------------

// Declaration(s):
namespace portaudio {

//////
/// @brief HostApi represents a host API (usually type of driver) in the System.
///
/// A single System can support multiple HostApi's each one typically having
/// a set of Devices using that HostApi (usually driver type). All Devices in
/// the HostApi can be enumerated and the default input/output Device for this
/// HostApi can be retrieved.
//////
class HostApi {
    public:
    typedef System::DeviceIterator DeviceIterator;

    // query info: id, name, numDevices
    PaHostApiTypeId typeId() const noexcept;
    PaHostApiIndex index() const;
    const char* name() const noexcept;
    size_t deviceCount() const noexcept;

    // iterate devices
    DeviceIterator devicesBegin() noexcept;
    DeviceIterator devicesEnd() noexcept;

    // default devices
    Device& defaultInputDevice() const noexcept;
    Device& defaultOutputDevice() const noexcept;

    // comparison operators
    bool operator==(const HostApi& rhs) const noexcept;
    bool operator!=(const HostApi& rhs) const noexcept;

    private:
    const PaHostApiInfo* info_;
    Device** devices_;

    private:
    friend class System;

    explicit HostApi(PaHostApiIndex index);
    ~HostApi();

    HostApi(const HostApi&); // non-copyable
    HostApi& operator=(const HostApi&); // non-copyable
};

} // namespace portaudio

// ---------------------------------------------------------------------------------------

#endif // INCLUDED_PORTAUDIO_HOSTAPI_HXX
