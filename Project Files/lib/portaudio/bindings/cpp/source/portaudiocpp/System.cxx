// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com
#include "portaudiocpp/System.hxx"
#include <cstddef>
#include <cassert>

#include "portaudiocpp/HostApi.hxx"
#include "portaudiocpp/Device.hxx"
#include "portaudiocpp/Stream.hxx"
#include "portaudiocpp/Exception.hxx"
#include "portaudiocpp/SystemHostApiIterator.hxx"
#include "portaudiocpp/SystemDeviceIterator.hxx"

namespace portaudio {
// -----------------------------------------------------------------------------------

// Static members:
System* System::instance_ = NULL;
int System::initCount_ = 0;
HostApi** System::hostApis_ = NULL;
Device** System::devices_ = NULL;
Device* System::nullDevice_ = NULL;

// -----------------------------------------------------------------------------------

int System::version() {
    return Pa_GetVersion();
}

const char* System::versionText() {
    return Pa_GetVersionText();
}

void System::initialize() {
    ++initCount_;

    if (initCount_ == 1) {
        // Create singleton:
        assert(instance_ == NULL);
        instance_ = new System();

        // Initialize the PortAudio system:
        {
            PaError err = Pa_Initialize();

            if (err != paNoError) throw PaException(err);
        }

        // Create and populate device array:
        {
            auto numDevices = instance().deviceCount();

            devices_ = new Device*[numDevices];

            for (size_t i = 0; i < numDevices; ++i)
                devices_[i] = new Device((PaDeviceIndex)i);
        }

        // Create and populate host api array:
        {
            const auto numHostApis = instance().hostApiCount();

            hostApis_ = new HostApi*[numHostApis];

            for (size_t i = 0; i < numHostApis; ++i)
                hostApis_[i] = new HostApi((PaHostApiIndex)i);
        }

        // Create null device:
        nullDevice_ = new Device(paNoDevice);
    }
}

void System::terminate() {
    PaError err = paNoError;

    if (initCount_ == 1) {
        // Destroy null device:
        delete nullDevice_;

        // Destroy host api array:
        {
            if (hostApis_ != NULL) {
                const auto numHostApis = instance().hostApiCount();

                for (size_t i = 0; i < numHostApis; ++i) delete hostApis_[i];

                delete[] hostApis_;
                hostApis_ = NULL;
            }
        }

        // Destroy device array:
        {
            if (devices_ != NULL) {
                const auto numDevices = instance().deviceCount();

                for (size_t i = 0; i < numDevices; ++i) delete devices_[i];

                delete[] devices_;
                devices_ = NULL;
            }
        }

        // Terminate the PortAudio system:
        assert(instance_ != NULL);
        err = Pa_Terminate();

        // Destroy singleton:
        delete instance_;
        instance_ = NULL;
    }

    if (initCount_ > 0) --initCount_;

    if (err != paNoError) throw PaException(err);
}

System& System::instance() {
    assert(exists());

    return *instance_;
}

bool System::exists() {
    return (instance_ != NULL);
}

// -----------------------------------------------------------------------------------

System::HostApiIterator System::hostApisBegin() {
    System::HostApiIterator tmp = {};
    tmp.ptr_ = &hostApis_[0]; // begin (first element)
    return tmp;
}

System::HostApiIterator System::hostApisEnd() {
    const auto count = hostApiCount();

    System::HostApiIterator tmp = {};
    tmp.ptr_ = &hostApis_[count]; // end (one past last element)
    return tmp;
}

HostApi& System::defaultHostApi() {
    PaHostApiIndex defaultHostApi = Pa_GetDefaultHostApi();

    if (defaultHostApi < 0) throw PaException(defaultHostApi);

    return *hostApis_[(size_t)defaultHostApi];
}

HostApi& System::hostApiByTypeId(PaHostApiTypeId type) {
    PaHostApiIndex index = Pa_HostApiTypeIdToHostApiIndex(type);

    if (index < 0) throw PaException(index);

    return *hostApis_[(size_t)index];
}

HostApi& System::hostApiByIndex(PaHostApiIndex index) {
    if (index < 0 || (size_t)index >= hostApiCount())
        throw PaException(paInternalError);

    return *hostApis_[(size_t)index];
}

size_t System::hostApiCount() const {
    PaHostApiIndex count = Pa_GetHostApiCount();

    if (count < 0) throw PaException(count);

    return (size_t)count;
}

// -----------------------------------------------------------------------------------

System::DeviceIterator System::devicesBegin() {
    DeviceIterator tmp;
    tmp.ptr_ = &devices_[0];

    return tmp;
}

System::DeviceIterator System::devicesEnd() {
    const auto count = deviceCount();

    DeviceIterator tmp;
    tmp.ptr_ = &devices_[count];

    return tmp;
}

//////
/// Returns the System's default input Device, or the null Device if none
/// was available.
//////
Device& System::defaultInputDevice() {
    PaDeviceIndex index = Pa_GetDefaultInputDevice();
    return deviceByIndex(index);
}

//////
/// Returns the System's default output Device, or the null Device if none
/// was available.
//////
Device& System::defaultOutputDevice() {
    PaDeviceIndex index = Pa_GetDefaultOutputDevice();
    return deviceByIndex(index);
}

//////
/// Returns the Device for the given index.
/// Will throw a paInternalError equivalent PaException if the given index
/// is out of range.
//////
Device& System::deviceByIndex(PaDeviceIndex index) {
    if (index < -1 || (size_t)index >= deviceCount()) {
        throw PaException(paInternalError);
    }

    if (index == -1) return System::instance().nullDevice();

    return *devices_[(size_t)index];
}

size_t System::deviceCount() const {
    PaDeviceIndex count = Pa_GetDeviceCount();

    if (count < 0) throw PaException(count);

    return (size_t)count;
}

Device& System::nullDevice() {
    return *nullDevice_;
}

// -----------------------------------------------------------------------------------

void System::sleep(long msec) {
    Pa_Sleep(msec);
}

int System::sizeOfSample(PaSampleFormat format) {
    PaError err = Pa_GetSampleSize(format);
    if (err < 0) {
        throw PaException(err);
        return 0;
    }

    return err;
}

// -----------------------------------------------------------------------------------

System::System() {
    // (left blank intentionally)
}

System::~System() {
    // (left blank intentionally)
}

// -----------------------------------------------------------------------------------

} // namespace portaudio
