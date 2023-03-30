// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com

#ifndef INCLUDED_PORTAUDIO_SAMPLEDATAFORMAT_HXX
#define INCLUDED_PORTAUDIO_SAMPLEDATAFORMAT_HXX

// ---------------------------------------------------------------------------------------

#include "portaudio.h"
#include <cassert>
#include <string>
#include <array>
// ---------------------------------------------------------------------------------------

namespace portaudio {

//////
/// @brief PortAudio sample data formats.
///
/// Small helper enum to wrap the PortAudio defines.
//////
enum class SampleDataFormat : unsigned int {
    INVALID_FORMAT = 0,
    FLOAT64 = paFloat64,
    FLOAT32 = paFloat32,
    INT32 = paInt32,
    INT24 = paInt24,
    INT16 = paInt16,
    INT8 = paInt8,
    UINT8 = paUInt8
};

namespace {
    enum class SampleDataFormatIndex {
        INVALID_FORMAT = 0,
        FLOAT64 = 1,
        FLOAT32,
        INT32,
        INT24,
        INT16,
        INT8,
        UINT8
    };

    SampleDataFormatIndex FormatIndexFromFormat(SampleDataFormat d) {

        if (d == SampleDataFormat::FLOAT64)
            return SampleDataFormatIndex::FLOAT64;
        if (d == SampleDataFormat::FLOAT32)
            return SampleDataFormatIndex::FLOAT32;
        if (d == SampleDataFormat::INT32) return SampleDataFormatIndex::INT32;
        if (d == SampleDataFormat::INT24) return SampleDataFormatIndex::INT24;
        if (d == SampleDataFormat::INT16) return SampleDataFormatIndex::INT16;
        if (d == SampleDataFormat::INT8) return SampleDataFormatIndex::INT8;
        if (d == SampleDataFormat::UINT8) return SampleDataFormatIndex::UINT8;
        assert(0);
        return SampleDataFormatIndex::INVALID_FORMAT;
    }
} // namespace

static constexpr inline uint8_t NSAMPLEFORMATS = 8;
static constexpr inline std::array<std::string_view, NSAMPLEFORMATS> SdfStrings
    = {"Invalid Sample Format", "Float64", "Float32", "Int32", "Int24", "Int16",
        "Int8", "UInt8"};

static inline std::string_view SampleDataFormatToString(SampleDataFormat d) {
    SampleDataFormatIndex sdfi = FormatIndexFromFormat(d);
    unsigned int i = static_cast<unsigned int>(sdfi);
    assert(i < NSAMPLEFORMATS);
    if (i >= NSAMPLEFORMATS) {
        return "Bad Sample Format";
    }
    return SdfStrings[i];
}

static inline SampleDataFormat SampleDataFormatFromString(std::string_view s) {

    if (_strcmpi(s.data(), "Float64") == 0) return SampleDataFormat::FLOAT64;
    if (_strcmpi(s.data(), "Float32") == 0) return SampleDataFormat::FLOAT32;
    if (_strcmpi(s.data(), "Int32") == 0) return SampleDataFormat::INT32;
    if (_strcmpi(s.data(), "Int24") == 0) return SampleDataFormat::INT24;
    if (_strcmpi(s.data(), "Int16") == 0) return SampleDataFormat::INT16;
    if (_strcmpi(s.data(), "Int8") == 0) return SampleDataFormat::INT8;
    if (_strcmpi(s.data(), "UInt8") == 0) return SampleDataFormat::UINT8;
    return SampleDataFormat::INVALID_FORMAT;
}

} // namespace portaudio

// ---------------------------------------------------------------------------------------

#endif // INCLUDED_PORTAUDIO_SAMPLEDATAFORMAT_HXX
