/*  clsCMASIOConfig.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2025 Richard Samphire MW0LGE

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at

mw0lge@grange-lane.co.uk
*/
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//

using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Diagnostics;

namespace Thetis
{
    internal static class CMASIOConfig
    {
        //SOFTWARE\\OpenHPSDR\\Thetis-x64
        //ASIOdrivername
        //ASIOblocknum
        private const string _registry_path = @"SOFTWARE\OpenHPSDR\Thetis-x64";

        private static RegistryKey openRegistryKey()
        {
            RegistryKey key = null;
            RegistryView view = Common.Is64Bit ? RegistryView.Registry64 : RegistryView.Registry32;

            try
            {
                key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view).OpenSubKey(_registry_path, true);
            }
            catch { }

            if (key == null)
            {
                try
                {
                    key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view).CreateSubKey(_registry_path);
                }
                catch { }
            }

            return key;
        }
        public static bool DoesRegistryValueExist(string valueName, RegistryKey key = null)
        {
            if (key == null) key = openRegistryKey();
            if (key != null)
            {
                object value = key.GetValue(valueName);
                return value != null;
            }
            return false;
        }

        public static string GetASIOdrivername(RegistryKey key = null)
        {
            if (key == null) key = openRegistryKey();
            if (key != null)
            {
                object value = key.GetValue("ASIOdrivername");
                if (value is string)
                {
                    return (string)value;
                }
            }
            return null;
        }

        public static void SetASIOdrivername(string driverName, RegistryKey key = null)
        {
            if (key == null) key = openRegistryKey();
            if (key != null)
            {
                key.SetValue("ASIOdrivername", driverName, RegistryValueKind.String);
            }
        }

        public static int GetASIOblocknum(RegistryKey key = null)
        {
            if (key == null) key = openRegistryKey();
            if (key != null)
            {
                object value = key.GetValue("ASIOblocknum");
                if (value is int)
                {
                    return (int)value & 0xFFFF;
                }
            }
            return 5;
        }
        public static bool GetASIOlockmode(RegistryKey key = null)
        {
            if (key == null) key = openRegistryKey();
            if (key != null)
            {
                object value = key.GetValue("ASIOblocknum");
                if (value is int)
                {
                    return ((int)value & 0xFFFF0000) != 0;
                }
            }
            return false;
        }
        public static void SetASIOblocknum(int blockNum, bool lock_mode, RegistryKey key = null)
        {
            if (key == null) key = openRegistryKey();
            if (key != null)
            {
                if (lock_mode) blockNum |= 0x000F0000; // some of the top bits set
                key.SetValue("ASIOblocknum", blockNum, RegistryValueKind.DWord);
            }
        }
        private static void deleteRegistryValue(string valueName, RegistryKey key = null)
        {
            if (key == null) key = openRegistryKey();
            if (key != null)
            {
                try
                {
                    key.DeleteValue(valueName);
                }
                catch { }
            }
        }

        public static List<string> GetASIODevices()
        {
            List<string> asioDevices = new List<string>();

            int deviceCount = PA19.PA_GetDeviceCount();
            for (int i = 0; i < deviceCount; i++)
            {
                PA19.PaDeviceInfo devInfo = PA19.PA_GetDeviceInfo(i);
                PA19.PaHostApiInfo hostInfo = PA19.PA_GetHostApiInfo(devInfo.hostApi);

                if (hostInfo.name.Contains("asio", StringComparison.OrdinalIgnoreCase))
                {
                    bool hasInput = devInfo.maxInputChannels > 0;
                    bool hasOutput = devInfo.maxOutputChannels > 0;

                    int result;
                    unsafe
                    {
                        PA19.PaStreamParameters outputParams;
                        PA19.PaStreamParameters inputParams;

                        outputParams.device = i;
                        outputParams.channelCount = devInfo.maxOutputChannels;
                        outputParams.sampleFormat = PA19.paInt32;
                        outputParams.suggestedLatency = devInfo.defaultLowOutputLatency;
                        outputParams.hostApiSpecificStreamInfo = null;

                        inputParams.device = i;
                        inputParams.channelCount = devInfo.maxInputChannels;
                        inputParams.sampleFormat = PA19.paInt32;
                        inputParams.suggestedLatency = devInfo.defaultLowInputLatency;
                        inputParams.hostApiSpecificStreamInfo = null;

                        result = PA19.PA_IsFormatSupported(&inputParams, &outputParams, 48000.0);
                    }
                    
                    if (result == 0 && hasInput && hasOutput && !string.IsNullOrEmpty(devInfo.name))
                    {
                        asioDevices.Add(devInfo.name.Left(32));
                    }
                }
            }
            return asioDevices;
        }
    }
}
