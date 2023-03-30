// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com
#include "portaudiocpp/SystemHostApiIterator.hxx"

namespace portaudio {
// -----------------------------------------------------------------------------------

HostApi& System::HostApiIterator::operator*() const {
    return **ptr_;
}

HostApi* System::HostApiIterator::operator->() const {
    return &**this;
}

// -----------------------------------------------------------------------------------

System::HostApiIterator& System::HostApiIterator::operator++() {
    ++ptr_;
    return *this;
}

System::HostApiIterator System::HostApiIterator::operator++(int) {
    System::HostApiIterator prev = *this;
    ++*this;
    return prev;
}

System::HostApiIterator& System::HostApiIterator::operator--() {
    --ptr_;
    return *this;
}

System::HostApiIterator System::HostApiIterator::operator--(int) {
    System::HostApiIterator prev = *this;
    --*this;
    return prev;
}

// -----------------------------------------------------------------------------------

bool System::HostApiIterator::operator==(
    const System::HostApiIterator& rhs) const { //-V835
    return (ptr_ == rhs.ptr_);
}

bool System::HostApiIterator::operator!=(
    const System::HostApiIterator& rhs) const { //-V835
    return !(*this == rhs);
}

// -----------------------------------------------------------------------------------
} // namespace portaudio
