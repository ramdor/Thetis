// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com
#include <iostream>
#include "portaudiocpp/PortAudioCpp.hxx"

#ifdef WIN32
#include "portaudiocpp/AsioDeviceAdapter.hxx"
#endif

#include <cassert>

extern int sine_main(void);
// ---------------------------------------------------------------------------------------

void printSupportedStandardSampleRates(
    const portaudio::DirectionSpecificStreamParameters& inputParameters,
    const portaudio::DirectionSpecificStreamParameters& outputParameters) {
    static double STANDARD_SAMPLE_RATES[] = {8000.0, 9600.0, 11025.0, 12000.0,
        16000.0, 22050.0, 24000.0, 32000.0, 44100.0, 48000.0, 88200.0, 96000.0,
        192000, -1}; // negative terminated list

    int printCount = 0;
#define FOUR 4
    for (int i = 0; STANDARD_SAMPLE_RATES[i] > 0; ++i) {
        portaudio::StreamParameters tmp
            = portaudio::StreamParameters(inputParameters, outputParameters,
                STANDARD_SAMPLE_RATES[i], 0, paNoFlag);

        if (tmp.isSupported()) {
            if (printCount == 0) {
                std::cout << "    " << STANDARD_SAMPLE_RATES[i]; // 8.2
                printCount = 1;
            } else if (printCount == FOUR) {
                std::cout << "," << std::endl;
                std::cout << "    " << STANDARD_SAMPLE_RATES[i]; // 8.2
                printCount = 1;
            } else {
                std::cout << ", " << STANDARD_SAMPLE_RATES[i]; // 8.2
                ++printCount;
            }
        }
    }

    if (printCount == 0) //-V547
        std::cout << "None" << std::endl;
    else
        std::cout << std::endl;
}

// ---------------------------------------------------------------------------------------

void poll_sample_rates(portaudio::System::DeviceIterator i);

extern int sine_main();
int main(int, char*[]) {
    try {
        portaudio::AutoSystem autoSys;

        portaudio::System& sys = portaudio::System::instance();

        std::cout << "PortAudio version number = " << sys.version()
                  << std::endl;
        std::cout << "PortAudio version text = '" << sys.versionText() << "'"
                  << std::endl;

        const auto numDevices = sys.deviceCount();
        std::cout << "Number of devices = " << numDevices << std::endl;

        for (portaudio::System::DeviceIterator i = sys.devicesBegin();
             i != sys.devicesEnd(); ++i) {

            if ((*i).index() == 34) {
                std::cout << "OK" << std::endl;
            }
            std::cout << "-------- device # --------" << (*i).index()
                      << std::endl;

            // Mark global and API specific default devices:
            bool defaultDisplayed = false;

            if ((*i).isSystemDefaultInputDevice()) {
                std::cout << "[ Default Input";
                defaultDisplayed = true;
            } else if ((*i).isHostApiDefaultInputDevice()) {
                std::cout << "[ Default " << (*i).hostApi().name() << " Input";
                defaultDisplayed = true;
            }

            if ((*i).isSystemDefaultOutputDevice()) {
                std::cout << (defaultDisplayed ? "," : "[");
                std::cout << " Default Output";
                defaultDisplayed = true;
            } else if ((*i).isHostApiDefaultOutputDevice()) {
                std::cout << (defaultDisplayed ? "," : "[");
                std::cout << " Default " << (*i).hostApi().name() << " Output";
                defaultDisplayed = true;
            }

            if (defaultDisplayed) std::cout << " ]" << std::endl;

            // Print device info:
            std::cout << "Name                        = " << (*i).name()
                      << std::endl;
            std::cout << "Host API                    = "
                      << (*i).hostApi().name() << std::endl;
            std::cout << "Max inputs = " << (*i).maxInputChannels()
                      << ", Max outputs = " << (*i).maxOutputChannels()
                      << std::endl;

            std::cout << "Default low input latency   = "
                      << (*i).defaultLowInputLatency() << std::endl; // 8.3
            std::cout << "Default low output latency  = "
                      << (*i).defaultLowOutputLatency() << std::endl; // 8.3
            std::cout << "Default high input latency  = "
                      << (*i).defaultHighInputLatency() << std::endl; // 8.3
            std::cout << "Default high output latency = "
                      << (*i).defaultHighOutputLatency() << std::endl; // 8.3

#ifdef WIN32
            // ASIO specific latency information:
            if ((*i).hostApi().typeId() == paASIO) {
                portaudio::AsioDeviceAdapter asioDevice((*i));

                std::cout << "ASIO minimum buffer size    = "
                          << asioDevice.minBufferSize() << std::endl;
                std::cout << "ASIO maximum buffer size    = "
                          << asioDevice.maxBufferSize() << std::endl;
                std::cout << "ASIO preferred buffer size  = "
                          << asioDevice.preferredBufferSize() << std::endl;

                if (asioDevice.granularity() == -1)
                    std::cout << "ASIO buffer granularity     = power of 2"
                              << std::endl;
                else
                    std::cout << "ASIO buffer granularity     = "
                              << asioDevice.granularity() << std::endl;
            }
#endif // WIN32

            std::cout << "Default sample rate         = "
                      << (*i).defaultSampleRate() << std::endl; // 8.2

            poll_sample_rates(i);
        }

        std::cout << "----------------------------------------------"
                  << std::endl;
    } catch (const portaudio::PaException& e) {
        std::cout << "A PortAudio error occurred: " << e.paErrorText()
                  << std::endl;
    } catch (const portaudio::PaCppException& e) {
        std::cout << "A PortAudioCpp error occurred: " << e.what() << std::endl;
    } catch (const std::exception& e) {
        std::cout << "A generic exception occurred: " << e.what() << std::endl;
    } catch (...) {
        std::cout << "An unknown exception occurred." << std::endl;
    }

    sine_main();

    return 0;
}

void print_supported(
    portaudio::System::DeviceIterator i, const portaudio::SampleDataFormat df) {

    assert(df != portaudio::SampleDataFormat::INVALID_FORMAT);
    portaudio::DirectionSpecificStreamParameters inputParameters(
        (*i), (*i).maxInputChannels(), df, true, 0.0, NULL);
    portaudio::DirectionSpecificStreamParameters outputParameters(
        (*i), (*i).maxOutputChannels(), df, true, 0.0, NULL);

    const auto s = portaudio::SampleDataFormatToString(df);

    if (inputParameters.numChannels() > 0) {
        std::cout << "Supported standard sample rates" << std::endl;
        std::cout << " for half-duplex " << s << " "
                  << inputParameters.numChannels()
                  << " channel input = " << std::endl;
        printSupportedStandardSampleRates(inputParameters,
            portaudio::DirectionSpecificStreamParameters::null());
    } else {
        // std::cout << "NOTE: Input for bitDepth " << s
        //         << " is NOT supported by this device" << std::endl;
    }

    if (outputParameters.numChannels() > 0) {
        std::cout << "Supported standard sample rates" << std::endl;
        std::cout << " for half-duplex " << s << " "
                  << outputParameters.numChannels()
                  << " channel output = " << std::endl;
        printSupportedStandardSampleRates(
            portaudio::DirectionSpecificStreamParameters::null(),
            outputParameters);
    } else {
        // std::cout << "NOTE: Output for bitDepth " << s
        //          << " is NOT supported by this device" << std::endl;
    }

    if (inputParameters.numChannels() > 0
        && outputParameters.numChannels() > 0) {
        std::cout << "Supported standard sample rates" << std::endl;
        std::cout << " for full-duplex " << s << " "
                  << inputParameters.numChannels() << " channel input, "
                  << outputParameters.numChannels()
                  << " channel output = " << std::endl;
        printSupportedStandardSampleRates(inputParameters, outputParameters);
    } else {
        // std::cout << "NOTE: Full duplex for bitDepth " << s
        //           << " is NOT supported by this device" << std::endl;
    }
}

void poll_sample_rates(portaudio::System::DeviceIterator i) {

    for (auto& st : portaudio::SdfStrings) {

        const auto bitDepth = portaudio::SampleDataFormatFromString(st);

        if (bitDepth != portaudio::SampleDataFormat::INVALID_FORMAT) {
            print_supported(i, bitDepth);
        }
    }
}
