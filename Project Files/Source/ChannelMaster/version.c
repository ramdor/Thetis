/*  version.c

MW0LGE

*/

#include "cmcomm.h"
#include "version.h"

PORT
int GetCMVersion()
{
	// MW0LGE version number now stored in Thetis->Versions.cs file, to keep shared
	// version numbers between c/c++/c#

	return _CMASTER_VERSION;
}

