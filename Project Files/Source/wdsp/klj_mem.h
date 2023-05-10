// klj_mem.h
#pragma once
/*/
#include <stdlib.h>
#include <string.h>
#include <Windows.h>
#include <Memoryapi.h>
static size_t const ONE_GIG = 1'000'000'000'000;

static inline void* PermanentStorage() {
    VirtualAlloc(0, ONE_GIG, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
}
/*/
