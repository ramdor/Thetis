// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com
#include "portaudiocpp/SystemDeviceIterator.hxx"

namespace portaudio {
// -----------------------------------------------------------------------------------

Device& System::DeviceIterator::operator*() const {
    return **ptr_;
}

Device* System::DeviceIterator::operator->() const {
    return &**this;
}

// -----------------------------------------------------------------------------------

System::DeviceIterator& System::DeviceIterator::operator++() {
    ++ptr_;
    return *this;
}

System::DeviceIterator System::DeviceIterator::operator++(int) {
    System::DeviceIterator prev = *this;
    ++*this;
    return prev;
}

System::DeviceIterator& System::DeviceIterator::operator--() {
    --ptr_;
    return *this;
}

System::DeviceIterator System::DeviceIterator::operator--(int) {
    System::DeviceIterator prev = *this;
    --*this;
    return prev;
}

// -----------------------------------------------------------------------------------

bool System::DeviceIterator::operator==(
    const System::DeviceIterator& rhs) const {
    return (ptr_ == rhs.ptr_);
}

bool System::DeviceIterator::operator!=(
    const System::DeviceIterator& rhs) const {
    return !(*this == rhs);
}

// -----------------------------------------------------------------------------------
} // namespace portaudio
