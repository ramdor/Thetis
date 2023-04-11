#include "comm.h"
int IsPowerOfTwo(unsigned long x) {
    return (x != 0) && ((x & (x - 1)) == 0);
}
