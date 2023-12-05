/*  version.h

MW0LGE

*/

#ifndef SHARED_VERSIONS_H_
#define SHARED_VERSIONS_H_
#include "../Console/Versions.cs"
#endif

extern "C" {
	__declspec (dllexport) int GetCMasioVersion();
}