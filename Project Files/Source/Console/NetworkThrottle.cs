/*  NetworkThrottle.cs

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
using Microsoft.Win32;
using System.Windows.Forms;

namespace Thetis
{
    static class NetworkThrottle
    {
        public static bool GetNetworkThrottle(out int throttle, bool showErrors = true)
        {
            bool bRet = false;
            throttle = 0;

            RegistryKey hklm = null;
            try
            {
                hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            }
            catch
            {
                if (showErrors)
                {
                    MessageBox.Show("Unable to open LocalMachine registry base key.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                }
            }
            if (hklm != null)
            {
                RegistryKey key = null;
                try
                {
                    key = hklm.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile", false);
                }
                catch
                {
                    if (showErrors)
                    {
                        MessageBox.Show("Unable to open SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile registry key.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                    }
                }

                if (key != null)
                {
                    try
                    {
                        object o = key.GetValue("NetworkThrottlingIndex");
                        if (o != null)
                        {
                            if (o is int)
                            {
                                throttle = (int)o;
                                bRet = true;
                            }
                            else
                            {
                                if (showErrors)
                                {
                                    MessageBox.Show("Unsuitable value in NetworkThrottlingIndex key.",
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                                }
                            }
                        }
                    }
                    catch
                    {
                        if (showErrors)
                        {
                            MessageBox.Show("Unable to GetValue on NetworkThrottlingIndex registry entry.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                        }
                    }
                    key.Close();
                }
                hklm.Close();
            }
            return bRet;
        }
        public static bool SetNetworkThrottle(int throttle)
        {
            bool bRet = false;

            if (Common.IsAdministrator())
            {
                RegistryKey hklm = null;
                try
                {
                    hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                }
                catch
                {
                    MessageBox.Show("Unable to open LocalMachine registry base key.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                }

                if (hklm != null)
                {
                    RegistryKey key = null;
                    try
                    {
                        key = hklm.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile", true);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to open SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile registry key.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                    }

                    if (key != null)
                    {
                        try
                        {
                            key.SetValue("NetworkThrottlingIndex", throttle, RegistryValueKind.DWord);             //unchecked((int)0xffffffffu)

                            bRet = true;
                        }
                        catch
                        {
                            MessageBox.Show("Unable to SetValue on NetworkThrottlingIndex registry entry.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                        }
                        key.Close();
                    }
                    hklm.Close();
                }
            }
            else
            {
                //msgbox need to be admin !
                MessageBox.Show("You need to be an Administrator. Please run Thetis 'As Administrator'.",
                    "No Administrator Rights",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }

            return bRet;
        }
    }
}
