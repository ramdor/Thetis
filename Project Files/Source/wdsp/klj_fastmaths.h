#pragma once
// klj_fastmaths.h

// math definitions
#define PI 3.1415926535897932
#define TWOPI 6.2831853071795864

#define EXTRA_PRECISION
// #define USE_REAL_COSINE

#ifndef USE_REAL_COSINE

// halves radio creation time.
__forceinline double COS(double x) {
    static const double tp = 1. / (2. * PI);
    x *= tp;
    x -= (.25) + floor(x + (.25));
    x *= (16.) * (fabs(x) - (.5));
#ifdef EXTRA_PRECISION
    x += (.225) * x * (fabs(x) - (1.));
#endif
    return x;
}

#else

#ifndef COS
#define COS cos
#endif

#endif