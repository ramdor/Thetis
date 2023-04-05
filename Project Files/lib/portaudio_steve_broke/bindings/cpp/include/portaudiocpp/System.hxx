// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com

#ifndef INCLUDED_PORTAUDIO_SYSTEM_HXX
#define INCLUDED_PORTAUDIO_SYSTEM_HXX

// ---------------------------------------------------------------------------------------

#include "portaudio.h"

// ---------------------------------------------------------------------------------------

// Forward declaration(s):
namespace portaudio {
class Device;
class Stream;
class HostApi;
} // namespace portaudio

// ---------------------------------------------------------------------------------------

// Declaration(s):
namespace portaudio {

//////
/// @brief System singleton which represents the PortAudio system.
///
/// The System is used to initialize/terminate PortAudio and provide
/// a single access point to the PortAudio System (instance()).
/// It can be used to iterate through all HostApi 's in the System as
/// well as all devices in the System. It also provides some utility
/// functionality of PortAudio.
///
/// Terminating the System will also abort and close the open streams.
/// The Stream objects will need to be deallocated by the client though
/// (it's usually a good idea to have them cleaned up automatically).
//////
class System {
    public:
    class HostApiIterator; // forward declaration
    class DeviceIterator; // forward declaration

    // -------------------------------------------------------------------------------

    static int version();
    static const char* versionText();

    static void initialize();
    static void terminate();

    static System& instance();
    static bool exists();

    // -------------------------------------------------------------------------------

    // host apis:
    HostApiIterator hostApisBegin();
    HostApiIterator hostApisEnd();

    HostApi& defaultHostApi();

    HostApi& hostApiByTypeId(PaHostApiTypeId type);
    HostApi& hostApiByIndex(PaHostApiIndex index);

    size_t hostApiCount() const;

    // -------------------------------------------------------------------------------

    // devices:
    DeviceIterator devicesBegin();
    DeviceIterator devicesEnd();

    Device& defaultInputDevice();
    Device& defaultOutputDevice();

    Device& deviceByIndex(PaDeviceIndex index);

    size_t deviceCount() const;

    static Device& nullDevice();

    // -------------------------------------------------------------------------------

    // misc:
    void sleep(long msec);
    int sizeOfSample(PaSampleFormat format);

    private:
    System();
    ~System();

    static System* instance_;
    static int initCount_;

    static HostApi** hostApis_;
    static Device** devices_;

    static Device* nullDevice_;
};

} // namespace portaudio

#endif // INCLUDED_PORTAUDIO_SYSTEM_HXX
