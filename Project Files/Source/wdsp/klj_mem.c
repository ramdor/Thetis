// klj_mem.c
#include "klj_mem.h"
#include <malloc.h>
#include <stdio.h>

// my dumb allocator that never frees anything
typedef struct Storage {
    size_t capacity;
    char* base;
    char* current;
    size_t used;
    size_t nallocs;
    size_t offset;
} STORAGE, *PSTORAGE;

static size_t NextPowerOf2(size_t val) {
    val--;
    val = (val >> 1) | val;
    val = (val >> 2) | val;
    val = (val >> 4) | val;
    val = (val >> 8) | val;
    val = (val >> 16) | val;
    return ++val;
}


#ifdef HOOK_MALLOC_KLJ_WDSP
int WDSPAllocHook(int allocType, void* userData, size_t size, int blockType,
    long requestNumber, const unsigned char* filename, int lineNumber) {

    static int busy = 0;
    if (busy) return TRUE;

    busy = 1;

    switch (allocType) {
    case _HOOK_ALLOC: break;
    case _HOOK_REALLOC: break;
    case _HOOK_FREE: break;
    default: assert(0);
    };
    busy = 0;
    return TRUE; // let malloc do it's thing.
}
#endif

PSTORAGE StorageCreate(size_t sz) {

    if (sz == 0) sz = DEFAULT_STORAGE_SIZE_KLJ;
    sz = NextPowerOf2(sz);
    assert(sz);
#ifdef HOOK_MALLOC_KLJ_WDSP
    static _CRT_ALLOC_HOOK RealMalloc;
    if (!RealMalloc) RealMalloc = _CrtSetAllocHook(WDSPAllocHook);
#endif
    char* mem = (char*)_aligned_malloc(sz + sizeof(STORAGE), 16);

    assert(mem);
    if (!mem) return 0;
    memset(mem, 0, sz + sizeof(STORAGE));
    PSTORAGE p = (PSTORAGE)mem;
    p->base = mem;

    p->used += sizeof(STORAGE);
    p->capacity = sz + sizeof(STORAGE);
    p->nallocs = 0;
    p->offset = sizeof(STORAGE);
    p->current = p->base + p->offset;
    return p;
}

// takes the double pointer (the address of your STORAGE)
// and you must never use arena ever again after this call.
void StorageDestroy(PSTORAGE* pparena) {
    assert(pparena && *pparena);
    PSTORAGE p = *pparena;
    _aligned_free(p->base);
    *pparena = 0;
}

void* StoragePushSize_(PSTORAGE Arena, size_t Size) {

    if (Arena->nallocs == 0) {
        if ((Arena->used + Size) > Arena->capacity) {
            assert("KLJ Storage not even big enough to fit first 'allocation'. "
                   "You need a bigger buffer size in the call to MakeStorage."
                == 0);
            return 0;
        }
    } else {
        if ((Arena->used + Size) > Arena->capacity) {
            printf(
                "KLJ Storage out of space after %ld\n.", (long)Arena->nallocs);
            fflush(stdout);
            assert("KLJ Storage out of storage space." == 0);
            return 0;
        }
    }

    void* Result = Arena->current + Arena->used;
    Arena->current += Size;
    Arena->used += Size;
    ++Arena->nallocs;
    return Result;
}

void StorageReset(PSTORAGE arena, int clear) {
    assert(arena);
    size_t remain = (arena->base + arena->capacity) - arena->current;
    double dmb = (double)MB_KLJ;
    double remainMB = (double)remain / dmb;
    printf("remain MB in buffer at reset: %f\n", remainMB);
    fflush(stdout);
    arena->current = arena->base + arena->offset;
    arena->used = arena->offset;
    if (clear) {
        memset(arena->current, 0, arena->capacity - arena->offset);
    }
    // could reset nallocs here, but let's not for now.
}
