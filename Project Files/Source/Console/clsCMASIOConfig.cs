using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if(key == null) key = openRegistryKey();
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
                    if(!string.IsNullOrEmpty(devInfo.name))
                        asioDevices.Add(devInfo.name.Left(32));
                }
            }
            return asioDevices;
        }
    }
}
