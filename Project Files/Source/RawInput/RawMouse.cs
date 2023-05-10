using System;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RawInput_dll
{
    public sealed class RawMouse
    {
        public readonly Dictionary<IntPtr, MouseEvent> _deviceList = new Dictionary<IntPtr, MouseEvent>();
        public delegate void DeviceEventHandler(object sender, RawInputEventArg e);
        public event DeviceEventHandler MouseMoved;
        readonly object _padLock = new object();
        public int NumberOfMice { get; private set; }
        static InputData _rawBuffer;
        //static bool m_bPreviouslyRegistered = false;
        public RawMouse(IntPtr hwnd, bool captureOnlyInForeground)
        {
            var rid = new RawInputDevice[1];

            //if (m_bPreviouslyRegistered)
            //{
            //    rid[0].UsagePage = HidUsagePage.GENERIC;
            //    rid[0].Usage = HidUsage.Mouse;
            //    rid[0].Flags |= RawInputDeviceFlags.REMOVE;
            //    rid[0].Target = IntPtr.Zero;

            //    if (!Win32.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
            //    {
            //        throw new ApplicationException("Failed to unregister raw input device(s).");
            //    }
            //    m_bPreviouslyRegistered = false;
            //}

            rid[0].UsagePage = HidUsagePage.GENERIC;
            rid[0].Usage = HidUsage.Mouse;
            rid[0].Flags = (captureOnlyInForeground ? RawInputDeviceFlags.NONE : RawInputDeviceFlags.INPUTSINK) | RawInputDeviceFlags.DEVNOTIFY;
            rid[0].Target = hwnd;

            if (!Win32.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
            {
                throw new ApplicationException("Failed to register raw mouse input device(s).");
            }
            else { /*m_bPreviouslyRegistered = true;*/ }
        }

        public void EnumerateDevices()
        {
            lock (_padLock)
            {
                _deviceList.Clear();

                var mouseNumber = 0;

                var globalDevice = new MouseEvent
                {
                    DeviceName = "Global Mouse",
                    DeviceHandle = IntPtr.Zero,
                    DeviceType = Win32.GetDeviceType(DeviceType.RimTypekeyboard),
                    Name = "Fake Mouse",
                    Source = mouseNumber++.ToString(CultureInfo.InvariantCulture)
                };

                _deviceList.Add(globalDevice.DeviceHandle, globalDevice);

                var numberOfDevices = 0;
                uint deviceCount = 0;
                var dwSize = (Marshal.SizeOf(typeof(Rawinputdevicelist)));

                if (Win32.GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, (uint)dwSize) == 0)
                {
                    var pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));
                    Win32.GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, (uint)dwSize);

                    for (var i = 0; i < deviceCount; i++)
                    {
                        uint pcbSize = 0;

                        // On Window 8 64bit when compiling against .Net > 3.5 using .ToInt32 you will generate an arithmetic overflow. Leave as it is for 32bit/64bit applications
                        var rid = (Rawinputdevicelist)Marshal.PtrToStructure(new IntPtr((pRawInputDeviceList.ToInt64() + (dwSize * i))), typeof(Rawinputdevicelist));

                        Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, IntPtr.Zero, ref pcbSize);

                        if (pcbSize <= 0) continue;

                        var pData = Marshal.AllocHGlobal((int)pcbSize);
                        Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, pData, ref pcbSize);
                        var deviceName = Marshal.PtrToStringAnsi(pData);

                        if (rid.dwType == DeviceType.RimTypemouse
                            && !String.IsNullOrEmpty(deviceName)
                            && deviceName.Contains("#"))// || rid.dwType == DeviceType.RimTypeHid)
                        {
                            var deviceDesc = Win32.GetDeviceDescription(deviceName);

                            var dInfo = new MouseEvent
                            {
                                DeviceName = Marshal.PtrToStringAnsi(pData),
                                DeviceHandle = rid.hDevice,
                                DeviceType = Win32.GetDeviceType(rid.dwType),
                                Name = deviceDesc,
                                Source = mouseNumber++.ToString(CultureInfo.InvariantCulture)
                            };

                            if (!_deviceList.ContainsKey(rid.hDevice))
                            {
                                numberOfDevices++;
                                _deviceList.Add(rid.hDevice, dInfo);
                            }
                        }

                        Marshal.FreeHGlobal(pData);
                    }

                    Marshal.FreeHGlobal(pRawInputDeviceList);

                    NumberOfMice = numberOfDevices;
                    Debug.WriteLine("EnumerateDevices() found {0} Mice(s)", NumberOfMice);
                    return;
                }
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public void ProcessRawInput(IntPtr hdevice)
        {
            if (_deviceList.Count == 0) return;

            var dwSize = 0;
            Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, IntPtr.Zero, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader)));

            if (dwSize != Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, out _rawBuffer, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader))))
            {
                Debug.WriteLine("Error getting the rawinput buffer");
                return;
            }

            if (_rawBuffer.header.dwType != DeviceType.RimTypemouse) return;

            int lastX = _rawBuffer.data.mouse.lLastX;
            int lastY = _rawBuffer.data.mouse.lLastY;
            uint buttons = _rawBuffer.data.mouse.ulButtons;
            ushort buttonFlags = _rawBuffer.data.mouse.usButtonFlags;
            short buttonData = (short)_rawBuffer.data.mouse.usButtonData; // short so -ve mouse wheel delta

            MouseEvent mouseEvent;

            if (_deviceList.ContainsKey(_rawBuffer.header.hDevice))
            {
                lock (_padLock)
                {
                    mouseEvent = _deviceList[_rawBuffer.header.hDevice];
                }
            }
            else
            {
                // Debug.WriteLine("Handle: {0} was not in the mouse device list.", _rawBuffer.header.hDevice);
                return;
            }

            mouseEvent.lastX = lastX;
            mouseEvent.lastY = lastY;
            mouseEvent.buttons = buttons;
            mouseEvent.buttonFlags = buttonFlags;
            mouseEvent.buttonData = buttonData;

            if ((MouseMoved != null) && ((buttonFlags & 0x0400) == 0x0400))  //only if this is mouse wheel info
            {
                MouseMoved(this, new RawInputEventArg(mouseEvent));
            }
        }
    }
}
