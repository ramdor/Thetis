// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com

#include "portaudiocpp/HostApi.hxx"
#include "portaudiocpp/System.hxx"
#include "portaudiocpp/Device.hxx"
#include "portaudiocpp/SystemDeviceIterator.hxx"
#include "portaudiocpp/Exception.hxx"

namespace portaudio {

// -----------------------------------------------------------------------------------

HostApi::HostApi(PaHostApiIndex index) : devices_(NULL) {
    try {
        info_ = Pa_GetHostApiInfo(index);

        // Create and populate devices array:
        size_t numDevices = deviceCount();

        devices_ = new Device*[numDevices];

        for (size_t i = 0; i < numDevices; ++i) {
            PaDeviceIndex deviceIndex
                = Pa_HostApiDeviceIndexToDeviceIndex(index, (int)i);

            if (deviceIndex < 0) {
                throw PaException(deviceIndex);
            }

            devices_[i] = &System::instance().deviceByIndex(deviceIndex);
        }
    } catch (const std::exception& e) {
        // Delete any (partially) constructed objects (deconstructor isn't
        // called):
        delete[] devices_; // devices_ is either NULL or valid

        // Re-throw exception:
        throw e;
    }
}

HostApi::~HostApi() {
    // Destroy devices array:
    delete[] devices_;
}

// -----------------------------------------------------------------------------------

PaHostApiTypeId HostApi::typeId() const noexcept {
    return info_->type;
}

PaHostApiIndex HostApi::index() const {
    PaHostApiIndex index = Pa_HostApiTypeIdToHostApiIndex(typeId());

    if (index < 0) throw PaException(index);

    return index;
}

const char* HostApi::name() const noexcept {
    return info_->name;
}

size_t HostApi::deviceCount() const noexcept {
    return (size_t)info_->deviceCount;
}

// -----------------------------------------------------------------------------------

HostApi::DeviceIterator HostApi::devicesBegin() noexcept {
    {
        DeviceIterator tmp = {};
        tmp.ptr_ = &devices_[0]; // begin (first element)
        return tmp;
    }
}

HostApi::DeviceIterator HostApi::devicesEnd() noexcept {
    DeviceIterator tmp = {};
    tmp.ptr_ = &devices_[deviceCount()]; // end (one past last element)
    return tmp;
}

// -----------------------------------------------------------------------------------

Device& HostApi::defaultInputDevice() const noexcept {
    return System::instance().deviceByIndex(info_->defaultInputDevice);
}

Device& HostApi::defaultOutputDevice() const noexcept {
    return System::instance().deviceByIndex(info_->defaultOutputDevice);
}

// -----------------------------------------------------------------------------------

bool HostApi::operator==(const HostApi& rhs) const noexcept {
    return (typeId() == rhs.typeId());
}

bool HostApi::operator!=(const HostApi& rhs) const noexcept {
    return !(*this == rhs);
}

// -----------------------------------------------------------------------------------

} // namespace portaudio
