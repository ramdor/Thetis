#pragma once
// klj_fastmaths.h

// math definitions
#define PI 3.1415926535897932
#define TWOPI 6.2831853071795864

#define EXTRA_PRECISION

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