//=================================================================
// UsbBCDCable.cs
//=================================================================
// Thetis is a C# implementation of a Software Defined Radio.
// Copyright (C) 2010-2023  Doug Wigley
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// You may contact us via email at: sales@flex-radio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    8900 Marybank Dr.
//    Austin, TX 78750
//    USA
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTD2XX_NET;

namespace Thetis
{
    #region Exceptions
    class UsbRelayDeviceNotFoundException : Exception
    {
        public override string Message
        {
	        get { return "USB relay not found."; }
        }
    }

    class UsbRelayConfigurationException : Exception
    {
        public override string Message
        {
            get { return "USB relay configuration error."; }
        }
    }

    class UsbRelayInvalidRelayException : Exception
    {
        public override string Message
        {
	        get { return "USB relay invalid relay number."; }
        }
    }

    class UsbRelayReadException : Exception
    {
        public override string Message
        {
            get { return "USB relay read error."; }
        }
    }

    class UsbRelayWriteException : Exception
    {
        public override string Message
        {
            get { return "USB relay write error."; }
        }
    }

    class UsbRelayStatusException : Exception
    {
        public override string Message
        {
            get { return "USB status not OK error."; }
        }
    }


    #endregion // Exceptions

    /// <summary>
    /// USB Relay 8 Device Class
    /// This class handles the read/writes to a particular USB
    /// relay 8 device.  Because of the nature of the writes/reads to the
    /// device with 8 relays.  It may work with device with less than
    /// 8 relays, but it has not been tested.
    /// </summary>
    class UsbBCDCable
    {
        #region Constants
        const int BaudRate = 921600;
        const byte BitMask = 0xff;          // 8-bit mask
        const byte BitMode = FTDI.FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG;
        const int RelayCount = 8;           // Maximum number of relays supported by device
        #endregion // Constants

        #region Private Variables
        FTDI _device;
        FTDI.FT_STATUS _status;
        byte _values;

        // FTD2 driver requires byte arrays, only a byte is needed for device
        byte[] _writeBuffer = new byte[2];
        byte[] _readBuffer = new byte[2];

        #endregion // Private Variables

        #region Public Properties
        public byte Values
        {
            get { return _values; }
        }
        #endregion // Public Properties

        #region Constuctors
        /// <summary>
        /// Create a USB Relay Device and open it by the serial number
        /// </summary>
        /// <param name="serialNumber">Device serial number</param>
        public UsbBCDCable( String serialNumber )
        {          
            // Open the relay device by serial number
            //  The serial number is always uniques, so the safest
            _device = new FTDI();
            _status = _device.OpenBySerialNumber(serialNumber);
            if (_status != FTDI.FT_STATUS.FT_OK)
            {
                throw new UsbRelayDeviceNotFoundException();
            }

            // Set the baud rate
            _status = _device.SetBaudRate(BaudRate);
            if (_status != FTDI.FT_STATUS.FT_OK)
            {
                throw new UsbRelayConfigurationException();
            }
            
            // Set the bit mode
            _status = _device.SetBitMode(BitMask, BitMode);
            if (_status != FTDI.FT_STATUS.FT_OK)
            {
                throw new UsbRelayConfigurationException();
            }

            // Clear all the relays
            // Note: From the Data Sheet, when in Sync Mode you can
            //  only read the interface pins when writing.  So start
            //  start out with all the values cleared.
            _values = 0x00;
            SetRelays(_values);
        }

        #endregion  // Constructors

        #region Public Methods
        /// <summary>
        /// Get the relay values
        /// </summary>
        /// <returns></returns>
        public byte GetRelayValues()
        {
            return _values;
        }

        public bool GetRelayValue(int relay)
        {
            byte values = GetRelayValues();

            byte mask = (byte) (~(byte)(1 << (relay - 1)));
            if ( (mask & values) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set an individual relay value.
        /// </summary>
        /// <param name="relay">relay index, zero based</param>
        /// <param name="value">relay value</param>
        public void SetRelay(int relay, bool value)
        {
            // Relay sanity check
            if (relay < 0 || relay > RelayCount - 1)
            {
                throw new UsbRelayInvalidRelayException();
            }
            
            // Get the current values
            byte newValue = GetRelayValues();

            // Please note, that this only works for devices with up to 8 relays 
            // note: C# doesn't have a power operator, have to use a Math a method call. 
            byte bitValue = (byte) Math.Pow(2,relay);
            if (true == value)
            {
                // Setting a relay value, or it in.
                newValue |= bitValue;
            }
            else
            {
                // Clearing a relay value, AND it out
                newValue &= (byte) ~bitValue;
            }
            _values = newValue;

            SetRelays(_values);
        }

        /// <summary>
        /// Set all the relay values, each relay is a bit
        /// </summary>
        /// <param name="values">combined relay values</param>
        public void SetRelays(byte values)
        {
            // Set the write buffer with the values
            _values = values;
            _writeBuffer[0] = values;

            // Write the value
            uint bytesWritten = 0;
            _status = _device.Write(_writeBuffer, 1, ref bytesWritten);
            if (_status != FTDI.FT_STATUS.FT_OK || bytesWritten != 1)
            {
                throw new UsbRelayWriteException();
            }
           // Console.WriteLine( String.Format("SetRelays(0x{0:x2})", _values));
        }

        public void CloseDevice()
        {
            if (_device.IsOpen) _status = _device.Close();
            _device = null;
        }

        #endregion  // Public Methods
    }


    /// <summary>
    /// USB Relays Class.  
    /// This class holds a collection of relay devices and handles the setting
    /// of relays by using DeviceName:RelayNumber or assigned names for individual
    /// DeviceName:RelayNumber pairs.
    /// </summary>
    class UsbBCDDevices
    {
        #region Constants
        #endregion      // Constants
       
        #region Private Variables
        FTDI _device;
        FTDI.FT_STATUS _status;
        FTDI.FT_DEVICE_INFO_NODE[] _deviceList;

        Dictionary<String, UsbBCDCable> _relays;

        List<string> _serialNumbers;
        List<uint> _locationIds;
        List<uint> _ids;
        #endregion

        #region Properties

        /// <summary>
        /// Determine if any devices exist
        /// </summary>
        public bool HasDevices
        {
            get
            {
                try
                {
                    return GetDeviceCount() != 0;
                }
                catch (UsbRelayStatusException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Device Count
        /// </summary>
        public int DeviceCount
        {
            get { return GetDeviceCount(); }
        }
        
        /// <summary>
        /// Device List
        /// </summary>
        public FTDI.FT_DEVICE_INFO_NODE[] Devices
        {
            get 
            {
                PopulateDeviceList();
                return _deviceList;
            }
        }

        /// <summary>
        /// Device serial numbers as a list of strings
        /// </summary>
        public List<String> DeviceSerialNumbers
        {
            get
            {
                PopulateDeviceList();
                return _serialNumbers;
            }
        }

        /// <summary>
        /// Device location IDs as list of uints
        /// </summary>
        public List<uint> DeviceLocationIDs
        {
            get
            {
                PopulateDeviceList();
                return _locationIds;
            }
        }

        /// <summary>
        /// Device IDs as a list of uints
        /// </summary>
        public List<uint> DeviceIDs
        {
            get
            {
                PopulateDeviceList();
                return _ids;
            }
        }

        #endregion //Properties


        #region Constructor
        public UsbBCDDevices()
        {
            _device = new FTDI();
            _relays = new Dictionary<string, UsbBCDCable>();
        }
        #endregion  // Constructor


        #region Public Methods
        
        public void OpenDevice(string serialNumber)
        {
            UsbBCDCable relay = new UsbBCDCable(serialNumber);
            _relays.Add(serialNumber, relay);
        }

        public void SetRelay(string serialNumber, int relay, bool value)
        {
            _relays[serialNumber].SetRelay(relay, value);
        }

        public void SetRelays(string serialNumber, byte values)
        {
            _relays[serialNumber].SetRelays(values);
        }

        public void SetBCDbyBand(string serialNumber, Band b)
        {
            byte n = 0;

            switch (b)
            {
                case Band.GEN:
                case Band.B60M:
                case Band.B2M:
                case Band.WWV:
                    n = 0;
                    break;

                case Band.B160M:
                    n = 1;
                    break;
                case Band.B80M:
                    n = 2;
                    break;
                case Band.B40M:
                    n = 3;
                    break;
                case Band.B30M:
                    n = 4;
                    break;
                case Band.B20M:
                    n = 5;
                    break;
                case Band.B17M:
                    n = 6;
                    break;
                case Band.B15M:
                    n = 7;
                    break;
                case Band.B12M:
                    n = 8;
                    break;
                case Band.B10M:
                    n = 9;
                    break;
                case Band.B6M:
                    n = 10;
                    break;

                case Band.VHF0:
                case Band.VHF1:
                case Band.VHF2:
                case Band.VHF3:
                case Band.VHF4:
                case Band.VHF5:
                case Band.VHF6:
                case Band.VHF7:
                case Band.VHF8:
                case Band.VHF9:
                case Band.VHF10:
                case Band.VHF11:
                case Band.VHF12:
                case Band.VHF13:
                    n = 0;
                    break;

                case Band.BLMF:
                case Band.B120M:
                case Band.B90M:
                case Band.B61M:
                case Band.B49M:
                case Band.B41M:
                case Band.B31M:
                case Band.B25M:
                case Band.B22M:
                case Band.B19M:
                case Band.B16M:
                case Band.B14M:
                case Band.B13M:
                case Band.B11M:
                    n = 0;
                    break;
            }

            SetRelays(serialNumber, n);

        }

        public bool GetRelay(string serialNumber, int relay)
        {
            return _relays[serialNumber].GetRelayValue(relay);
        }

        public byte GetRelays(string serialNumber)
        {
            return _relays[serialNumber].GetRelayValues();
        }

        public void CloseDevice(string serialNumber)
        {
            if (_device.IsOpen) _status = _device.Close();
            if (serialNumber != null && _relays.Count() != 0) _relays[serialNumber].CloseDevice();
        }

        public void ResetDevice()
        {
            if (_device.IsOpen) _status = _device.ResetDevice();
        }

        #endregion  // Public Methods


        #region Private Methods
        /// <summary>
        /// Return the device count
        /// </summary>
        /// <returns>Number of devices</returns>
        private int GetDeviceCount()
        {
            uint count = 0;
            _status = _device.GetNumberOfDevices(ref count);
            if (_status != FTDI.FT_STATUS.FT_OK)
            {
                throw new UsbRelayStatusException();
            }

            return (int)count;
        }

        private void PopulateDeviceList()
        {
            // Have to create known size array to get the device list
            int count = GetDeviceCount();
            _deviceList = new FTDI.FT_DEVICE_INFO_NODE[count];
            _status = _device.GetDeviceList(_deviceList);
            if (_status != FTDI.FT_STATUS.FT_OK)
            {
                throw new UsbRelayStatusException();
            }

            // Create new lists to populate
            _serialNumbers = new List<string>();
            _locationIds = new List<uint>();
            _ids = new List<uint>();

            // Populate each list
            foreach( FTDI.FT_DEVICE_INFO_NODE device in _deviceList)
            {
                _serialNumbers.Add(device.SerialNumber);
               // Console.WriteLine("Serial Number: " + device.SerialNumber);

                _locationIds.Add(device.LocId);
               // Console.WriteLine("Location ID: " + device.LocId.ToString());

                _ids.Add(device.ID);
               // Console.WriteLine("ID: " + device.ID.ToString());
            }
        }

        #endregion      // Private Methods

    }
}
