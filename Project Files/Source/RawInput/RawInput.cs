////////////////////////////////////////////////////////////////////////////////////////
/// Based on code from :
/// https://www.codeproject.com/Articles/17123/Using-Raw-Input-from-C-to-handle-multiple-keyboard
/// 9 Mar 2015 - Emma Burrows, Steve Messer
/// Please note that downloads for this article are governed by the 
/// The GNU Lesser General Public License (LGPLv3).
/// MW0LGE 08/08/19 - Mouse support added, devices changed event added
////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace RawInput_dll
{
    public class RawInput : NativeWindow  
    {
        //static RawKeyboard _keyboardDriver;
        //static RawMouse _mouseDriver;
        private RawKeyboard _keyboardDriver;
        private RawMouse _mouseDriver;

        private string _id;

        readonly IntPtr _devNotifyHandle;
        static readonly Guid DeviceInterfaceHid = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");
        private PreMessageFilter _filter;

        public delegate void DevicesEventHandler(object sender);
        public event DevicesEventHandler DevicesChanged;

        public event RawKeyboard.DeviceEventHandler KeyPressed
        {
            add { _keyboardDriver.KeyPressed += value; }
            remove { _keyboardDriver.KeyPressed -= value;}
        }
        public event RawMouse.DeviceEventHandler MouseMoved {
            add { _mouseDriver.MouseMoved += value; }
            remove { _mouseDriver.MouseMoved -= value; }
        }
        public int NumberOfMice {
            get { return _mouseDriver.NumberOfMice; }
        }
        public int NumberOfKeyboards
        {
            get { return _keyboardDriver.NumberOfKeyboards; } 
        }
        public Dictionary<IntPtr, MouseEvent> MouseDevices()
        {
            return _mouseDriver._deviceList;
        }

        public bool IgnoreNextWheelEvent {
            get {
                    if(_filter == null)
                    {
                        return false;
                    }
                    else
                    {
                        return _filter.IgnoreNextWheelEvent;
                    }
                }
            set {
                if (_filter != null)
                {
                    _filter.IgnoreNextWheelEvent = value;
                }
            }
        }
        public void AddMessageFilter()
        {
            if (null != _filter) return;
            
            _filter = new PreMessageFilter();
            Application.AddMessageFilter(_filter);
        }

        public void RemoveMessageFilter()
        {
            if (null == _filter) return;

            Application.RemoveMessageFilter(_filter);

            _filter = null;
        }

        public RawInput(IntPtr parentHandle, bool captureOnlyInForegroundMouse, bool captureOnlyInForegroundKeyboard, string id = "")
        {
            _id = id;

            AssignHandle(parentHandle);

            _keyboardDriver = new RawKeyboard(parentHandle, captureOnlyInForegroundKeyboard);
            _keyboardDriver.EnumerateDevices(id);

            _mouseDriver = new RawMouse(parentHandle, captureOnlyInForegroundMouse);
            _mouseDriver.EnumerateDevices(id);

            _devNotifyHandle = RegisterForDeviceNotifications(parentHandle);
        }

        static IntPtr RegisterForDeviceNotifications(IntPtr parent)
        {
            var usbNotifyHandle = IntPtr.Zero;
            var bdi = new BroadcastDeviceInterface();
            bdi.DbccSize = Marshal.SizeOf(bdi);
            bdi.BroadcastDeviceType = BroadcastDeviceType.DBT_DEVTYP_DEVICEINTERFACE;
            bdi.DbccClassguid = DeviceInterfaceHid;

            var mem = IntPtr.Zero;
            try
            {
                mem = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BroadcastDeviceInterface)));
                Marshal.StructureToPtr(bdi, mem, false);
                usbNotifyHandle = Win32.RegisterDeviceNotification(parent, mem, DeviceNotification.DEVICE_NOTIFY_WINDOW_HANDLE);
            }
            catch (Exception e)
            {
                Debug.Print("Registration for device notifications Failed. Error: {0}", Marshal.GetLastWin32Error());
                Debug.Print(e.StackTrace);
            }
            finally
            {
                Marshal.FreeHGlobal(mem);
            }

            if (usbNotifyHandle == IntPtr.Zero)
            {
                Debug.Print("Registration for device notifications Failed. Error: {0}", Marshal.GetLastWin32Error());
            }
            
            return usbNotifyHandle;
        }

        protected override void WndProc(ref Message message)
        {
            switch (message.Msg)
            {
                case Win32.WM_INPUT:
                    {
                        _keyboardDriver.ProcessRawInput(message.LParam);
                        _mouseDriver.ProcessRawInput(message.LParam);
                    }
                    break;

                case Win32.WM_USB_DEVICECHANGE:
                    {
                        const int DBT_DEVICEARRIVAL = 0x8000;
                        const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

                        if (((int)message.WParam & DBT_DEVICEARRIVAL) == DBT_DEVICEARRIVAL)
                        {
                            Debug.WriteLine("USB Device Added");
                            _keyboardDriver.EnumerateDevices(_id);
                            _mouseDriver.EnumerateDevices(_id);

                            DevicesChanged?.Invoke(this);
                        }

                        if (((int)message.WParam & DBT_DEVICEREMOVECOMPLETE) == DBT_DEVICEREMOVECOMPLETE)
                        {
                            Debug.WriteLine("USB Device Removed");
                            _keyboardDriver.EnumerateDevices(_id);
                            _mouseDriver.EnumerateDevices(_id);

                            DevicesChanged?.Invoke(this);
                        }
                    }
                    break;
            }

            base.WndProc(ref message);
        }

        ~RawInput()
        {
            Win32.UnregisterDeviceNotification(_devNotifyHandle);
            RemoveMessageFilter();
        }
    }
}
