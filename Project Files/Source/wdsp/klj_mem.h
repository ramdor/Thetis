// klj_mem.h
#pragma once

#include <stdlib.h>
#include <string.h>
#include <Windows.h>
#include <Memoryapi.h>

static const size_t KB = 1024;
static size_t const MB = 1024 * 1024;
static size_t const GB = 1024 * 1024 * 1024;

typedef struct Storage {
    size_t capacity;
    char* mem;
} STORAGE, *PSTORAGE;

static inline void* PermanentStorage(STORAGE* p) {
    p->mem
        = (char*)VirtualAlloc(0, GB, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
}
