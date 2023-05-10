// klj_mem.h
#pragma once

#include <stdlib.h>
#include <string.h>
#include <Windows.h>
#include <Memoryapi.h>
#include <assert.h>
#include <crtdbg.h>

#define HOOK_MALLOC_KLJ_WDSP

#ifndef Assert
#define Assert assert
#endif

#define KB_KLJ 1024ULL
#define MB_KLJ 1024ULL * 1024ULL
static size_t DEFAULT_BUILDER_STORAGE_SIZE_KLJ = MB_KLJ * 10;

// my dumb allocator that never frees anything
typedef struct Storage Storage;
typedef struct Storage* PSTORAGE;

// Make a lump of storage from where we will hand out pointers
// sz can be 0, in which case you get DEFAULT_STORAGE_SIZE
struct Storage* StorageCreate(size_t sz);

// takes the double pointer (the address of your STORAGE)
// and you must never use arena ever again after this call.
void StorageDestroy(Storage** pparena);

// Use the PushSize, PushStruct, PushArray macros instead of this directly
void* StoragePushSize_(Storage* Arena, size_t Size);

#define PushSize(STORAGE, Size) StoragePushSize_(STORAGE, Size)
#define PushStruct(STORAGE, type) (type*)StoragePushSize_(STORAGE, sizeof(type))
#define PushArray(STORAGE, Count, type)                                        \
    (type*)StoragePushSize_(STORAGE, (Count) * sizeof(type))

void StorageReset(PSTORAGE, int clear);

#ifdef HOOK_MALLOC_KLJ_WDSP
int WDSPAllocHook(int allocType, void* userData, size_t size, int blockType,
    long requestNumber, const unsigned char* filename, int lineNumber);
#endif
