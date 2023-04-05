// This is an independent project of an individual developer. Dear PVS-Studio,
// please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java:
// http://www.viva64.com
#include <string.h>
#include "asiodrivers.h"

AsioDrivers* asioDrivers = 0;

bool loadAsioDriver(char* name);

bool loadAsioDriver(char* name) {
    if (!asioDrivers) asioDrivers = new AsioDrivers();
    if (asioDrivers) //-V668
        return asioDrivers->loadDriver(name);
    return false;
}

//------------------------------------------------------------------------------------

#if MAC

bool resolveASIO(unsigned long aconnID);

AsioDrivers::AsioDrivers() : CodeFragments("ASIO Drivers", 'AsDr', 'Asio') {
    connID = -1;
    curIndex = -1;
}

AsioDrivers::~AsioDrivers() {
    removeCurrentDriver();
}

bool AsioDrivers::getCurrentDriverName(char* name) {
    if (curIndex >= 0) return getName(curIndex, name);
    return false;
}

long AsioDrivers::getDriverNames(char** names, long maxDrivers) {
    for (long i = 0; i < getNumFragments() && i < maxDrivers; i++)
        getName(i, names[i]);
    return getNumFragments() < maxDrivers ? getNumFragments() : maxDrivers;
}

bool AsioDrivers::loadDriver(char* name) {
    char dname[64];
    unsigned long newID;

    for (long i = 0; i < getNumFragments(); i++) {
        if (getName(i, dname) && !strcmp(name, dname)) {
            if (newInstance(i, &newID)) {
                if (resolveASIO(newID)) {
                    if (connID != -1) removeInstance(curIndex, connID);
                    curIndex = i;
                    connID = newID;
                    return true;
                }
            }
            break;
        }
    }
    return false;
}

void AsioDrivers::removeCurrentDriver() {
    if (connID != -1) removeInstance(curIndex, connID);
    connID = -1;
    curIndex = -1;
}

//------------------------------------------------------------------------------------

#elif WINDOWS

#include "../hostapi/asio/ASIOSDK/common/iasiodrv.h"

extern IASIO* theAsioDriver;

AsioDrivers::AsioDrivers() : AsioDriverList() {
    curIndex = -1;
    connID = -1;
}

AsioDrivers::~AsioDrivers() = default;
static constexpr size_t NAMEBUFLEN = 32;

bool AsioDrivers::getCurrentDriverName(char* name) {
    if (curIndex >= 0)
        return asioGetDriverName(curIndex, name, NAMEBUFLEN) == 0 ? true
                                                                  : false;
    name[0] = 0;
    return false;
}

long AsioDrivers::getDriverNames(char** names, long maxDrivers) {
    for (size_t i = 0; i < (size_t)asioGetNumDev() && i < (size_t)maxDrivers;
         i++)
        asioGetDriverName((int)i, names[i], NAMEBUFLEN);
    return asioGetNumDev() < maxDrivers ? asioGetNumDev() : maxDrivers;
}

bool AsioDrivers::loadDriver(char* name) {
    char dname[64] = {0};
    char curName[64] = {0};

    for (long i = 0; i < asioGetNumDev(); i++) {
        if (!asioGetDriverName(i, dname, NAMEBUFLEN) && !strcmp(name, dname)) {
            curName[0] = 0; //-V1048
            getCurrentDriverName(curName); // in case we fail...
            removeCurrentDriver();

            if (!asioOpenDriver(i, (void**)&theAsioDriver)) {
                curIndex = i;
                return true;
            } else {
                theAsioDriver = 0;
                if (curName[0] && _strcmpi(dname, curName) == 0)
                    loadDriver(curName); // try restore
            }
            break;
        }
    }
    return false;
}

void AsioDrivers::removeCurrentDriver() {
    if (curIndex != -1) asioCloseDriver(curIndex);
    curIndex = -1;
}

#elif SGI || BEOS

#include "asiolist.h"

AsioDrivers::AsioDrivers() : AsioDriverList() {
    curIndex = -1;
}

AsioDrivers::~AsioDrivers() {}

bool AsioDrivers::getCurrentDriverName(char* name) {
    return false;
}

long AsioDrivers::getDriverNames(char** names, long maxDrivers) {
    return 0;
}

bool AsioDrivers::loadDriver(char* name) {
    return false;
}

void AsioDrivers::removeCurrentDriver() {}

#else
#error implement me
#endif
