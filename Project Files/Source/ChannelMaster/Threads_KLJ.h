#ifndef PRIO_THRD_DEFINED
#define PRIO_THRD_DEFINED

/*/
static inline int IsPowerOfTwo(unsigned long x) {
    return (x != 0) && ((x & (x - 1)) == 0);
}
/*/

#include <Windows.h>
#include <avrt.h>
#include <assert.h>
#include <stdio.h>

static inline HANDLE prioritise_thread_max() {

    DWORD taskIndex = 0;
    HANDLE hTask = AvSetMmThreadCharacteristics(TEXT("Pro Audio"), &taskIndex);
    if (hTask != 0) {

        BOOL ok = AvSetMmThreadPriority(hTask, AVRT_PRIORITY_CRITICAL);
        assert(ok);

    } else {
        // assert("Why did setting thread priority fail?" == 0);
        const DWORD dw = GetLastError();
        if (dw == 1552) { // the specified thread is already joining a task
            // assert(0);

        } else {
            SetThreadPriority(
                GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);
            fprintf(stderr,
                "I don't like this, falling back to "
                "THREAD_PRIORITY_TIME_CRITICAL");
            fflush(stderr);
        }
    }
    return hTask;
}

static inline BOOL prioritise_thread_cleanup(HANDLE h) {
    BOOL ret = AvRevertMmThreadCharacteristics(h);
    if (ret == 0) {
        DWORD dw = GetLastError();
        assert(0);
        fprintf(stderr,
            "Failed to clean up thread priority, with error code: %ld\n",
            (int)dw);
    }

    return ret;
}
#endif