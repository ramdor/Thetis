/*  version.cpp

MW0LGE

*/

#include "version.h"

extern "C" {
	__declspec(dllexport)
		int GetCMasioVersion()
	{
		// MW0LGE version number now stored in Thetis->Versions.cs file, to keep shared
		// version numbers between c/c++/c#

		return _CMASTER_ASIO_VERSION;
	}
}
