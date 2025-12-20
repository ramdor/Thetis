//=================================================================
// SDRSerialPort.cs
//=================================================================
// Copyright (C) 2005  Bill Tracey
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
//================================================================= 
// Serial port support for PowerSDR support of CAT and serial port control  
//=================================================================

#define DBG_PRINT

using System;
using System.Threading;
using System.IO.Ports;

namespace Thetis
{
    public class SDRSerialPort
    {
        public static event SerialRXEventHandler serial_rx_event;

        private SerialPort commPort;
        public SerialPort BasePort
        {
            get { return commPort; }
        }

        private bool isOpen = false;
        private bool bitBangOnly = false;

        //Added 2/14/2008 BT
        public bool IsOpen
        {
            get { return commPort.IsOpen; }
        }

        public void Open()
        {
            commPort.Open();
        }

        public void Close()
        {
            commPort.Close();
        }

        public static Parity StringToParity(string s)
        {
            if (s == "none") return Parity.None;
            if (s == "odd") return Parity.Odd;
            if (s == "even") return Parity.Even;
            if (s == "space") return Parity.Space;
            if (s == "mark") return Parity.Mark;
            return Parity.None;  // error -- default to none
        }

        public static StopBits StringToStopBits(string s)
        {
            if (s == "0") return StopBits.None;
            if (s == "1") return StopBits.One;
            if (s == "1.5") return StopBits.OnePointFive;
            if (s == "2") return StopBits.Two;
            return StopBits.One; // error -- default 
        }

        public SDRSerialPort(int portidx)
        {
            commPort = new SerialPort();
            commPort.Encoding = System.Text.Encoding.ASCII;
            commPort.RtsEnable = true; // hack for soft rock ptt 
            commPort.DtrEnable = true; // set dtr off 
            //commPort.ErrorReceived += new SerialErrorReceivedEventHandler(this.SerialErrorReceived);
            commPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialReceivedData);
            commPort.PinChanged += new SerialPinChangedEventHandler(this.SerialPinChanged);

            commPort.PortName = "COM" + portidx.ToString();

            commPort.Handshake = Handshake.None; //[2.10.3.9]MW0LGE
            commPort.Parity = Parity.None;
            commPort.StopBits = StopBits.One;
            commPort.DataBits = 8;
            commPort.BaudRate = 9600;
            commPort.ReadTimeout = 5000;
            commPort.WriteTimeout = 500;
            commPort.ReceivedBytesThreshold = 1;
        }
        // set the comm parms ... can only be done if port is not open -- silently fails if port is open (fixme -- add some error checking) 
        // 
        public void setCommParms(int baudrate, Parity p, int databits, StopBits stop)
        {
            if (commPort.IsOpen) return; // bail out if it's already open 

            commPort.BaudRate = baudrate;
            commPort.Parity = p;
            commPort.StopBits = stop;
            commPort.DataBits = databits;
        }

        public uint put(string s)
        {
            if (bitBangOnly) return 0;  // fixme -- throw exception?		
            commPort.Write(s);
            return (uint)s.Length; // wjt fixme -- hack -- we don't know if we actually wrote things 			
        }

        public int Create()
        {
            return Create(false);
        }

        // create port 
        public int Create(bool bit_bang_only)
        {
            bitBangOnly = bit_bang_only;
            if (isOpen) { return -1; }
            commPort.Open();
            isOpen = commPort.IsOpen;
            if (isOpen)
                return 0; // all is well
            else
                return -1;  //error
        }

        public void Destroy()
        {
            try
            {
                commPort.Close();
            }
            catch (Exception)
            {

            }
            isOpen = false;
        }

        public bool isCTS()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CtsHolding;
        }

        public bool isDSR()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.DsrHolding;

        }
        public bool isRI()
        {
            if (!isOpen) return false; // fixme error check 
            return false;
        }

        public bool isRLSD()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CDHolding;
        }

        public void setDTR(bool v)
        {
            if (!isOpen) return;
            commPort.DtrEnable = v;
        }

        void SerialErrorReceived(object source, SerialErrorReceivedEventArgs e)
        {

        }

        private bool use_for_cat_ptt = false;
        public bool UseForCATPTT
        {
            get { return use_for_cat_ptt; }
            set { use_for_cat_ptt = value; }
        }
        
        private bool use_for_keyptt = false;
        public bool UseForKeyPTT
        {
            get { return use_for_keyptt; }
            set { use_for_keyptt = value; }
        }

        private bool use_for_paddles = false;
        public bool UseForPaddles
        {
            get { return use_for_paddles; }
            set { use_for_paddles = value; }
        }

        private bool ptt_on_dtr = false;
        public bool PTTOnDTR
        {
            get { return ptt_on_dtr; }
            set { ptt_on_dtr = value; }
        }

        private bool ptt_on_rts = false;
        public bool PTTOnRTS
        {
            get { return ptt_on_rts; }
            set { ptt_on_rts = value; }
        }

        private bool key_on_dtr = false;
        public bool KeyOnDTR
        {
            get { return key_on_dtr; }
            set { key_on_dtr = value; }
        }

        private bool key_on_rts = false;
        public bool KeyOnRTS
        {
            get { return key_on_rts; }
            set { key_on_rts = value; }
        }

        void SerialPinChanged(object source, SerialPinChangedEventArgs e)
        {
            //refactored for MAX performance [2.10.3.9]MW0LGE
            // added else's to match switch's from old code

            if (!use_for_keyptt && !use_for_paddles && !use_for_cat_ptt) return;
            bool dsrHolding = commPort.DsrHolding;
            bool ctsHolding = commPort.CtsHolding;
            bool isDsrChanged = e.EventType == SerialPinChange.DsrChanged;
            bool isCtsChanged = e.EventType == SerialPinChange.CtsChanged;
            bool isPinChangeRelevant = isDsrChanged || isCtsChanged;

            if (use_for_keyptt && isPinChangeRelevant)
            {
                if (isDsrChanged)
                {
                    if (ptt_on_dtr) CWInput.KeyerPTT = dsrHolding;
                    if (key_on_dtr) NetworkIO.SetCWX(dsrHolding ? 1 : 0);
                }
                else if (isCtsChanged)
                {
                    if (ptt_on_rts) CWInput.KeyerPTT = ctsHolding;
                    if (key_on_rts) NetworkIO.SetCWX(ctsHolding ? 1 : 0);
                }
            }
            else if (use_for_paddles && isPinChangeRelevant)
            {
                if (isDsrChanged) NetworkIO.SetCWDot(dsrHolding ? 1 : 0);
                else if (isCtsChanged) NetworkIO.SetCWDash(ctsHolding ? 1 : 0);
            }

            if (use_for_cat_ptt)
            {
                if (isDsrChanged && ptt_on_dtr) CWInput.CATPTT = dsrHolding;
                else if (isCtsChanged && ptt_on_rts) CWInput.CATPTT = ctsHolding;
            }

            //if (!use_for_keyptt && !use_for_paddles && !use_for_cat_ptt) return;

            //if (use_for_keyptt)
            //{
            //    switch (e.EventType)
            //    {
            //        case SerialPinChange.DsrChanged:

            //            if (ptt_on_dtr)
            //            {
            //                CWInput.KeyerPTT = commPort.DsrHolding;
            //            }

            //            if (key_on_dtr)
            //            {
            //                NetworkIO.SetCWX(Convert.ToInt32(commPort.DsrHolding));
            //            }
            //            break;
            //        case SerialPinChange.CtsChanged:

            //            if (ptt_on_rts)
            //            {
            //                CWInput.KeyerPTT = commPort.CtsHolding;
            //            }

            //            if (key_on_rts)
            //            {
            //                NetworkIO.SetCWX(Convert.ToInt32(commPort.CtsHolding));
            //            }
            //            break;
            //    }
            //}
            //else if (use_for_paddles)
            //{
            //    switch (e.EventType)
            //    {
            //        case SerialPinChange.DsrChanged:
            //            NetworkIO.SetCWDot(Convert.ToInt32(commPort.DsrHolding));
            //            break;
            //        case SerialPinChange.CtsChanged:
            //            NetworkIO.SetCWDash(Convert.ToInt32(commPort.CtsHolding));
            //            break;
            //    }
            //}

            //if (use_for_cat_ptt)
            //{
            //    switch (e.EventType)
            //    {
            //        case SerialPinChange.DsrChanged:

            //            if (ptt_on_dtr)
            //            {
            //                CWInput.CATPTT = commPort.DsrHolding;
            //            }
            //            break;
            //        case SerialPinChange.CtsChanged:

            //            if (ptt_on_rts)
            //            {
            //                CWInput.CATPTT = commPort.CtsHolding;
            //            }                      
            //            break;
            //    }

            //}
        }

        void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
        {
            serial_rx_event(this, new SerialRXEvent(commPort.ReadExisting()));
        }

    }

    public class SDRSerialPort2
    {
        public static event SerialRXEventHandler serial_rx_event;

        private SerialPort commPort;
        public SerialPort BasePort
        {
            get { return commPort; }
        }

        private bool isOpen = false;
        private bool bitBangOnly = false;

        //Added 2/14/2008 BT
        public bool IsOpen
        {
            get { return commPort.IsOpen; }
        }

        public void Open()
        {
            commPort.Open();
        }

        public void Close()
        {
            commPort.Close();
        }

        public static Parity StringToParity(string s)
        {
            if (s == "none") return Parity.None;
            if (s == "odd") return Parity.Odd;
            if (s == "even") return Parity.Even;
            if (s == "space") return Parity.Space;
            if (s == "mark") return Parity.Mark;
            return Parity.None;  // error -- default to none
        }

        public static StopBits StringToStopBits(string s)
        {
            if (s == "0") return StopBits.None;
            if (s == "1") return StopBits.One;
            if (s == "1.5") return StopBits.OnePointFive;
            if (s == "2") return StopBits.Two;
            return StopBits.One; // error -- default 
        }

        public SDRSerialPort2(int portidx) //, SerialDataReceivedEventHandler datareceived)
        {
            commPort = new SerialPort();
            commPort.Encoding = System.Text.Encoding.ASCII;
            commPort.RtsEnable = true; // hack for soft rock ptt 
            commPort.DtrEnable = true; // set dtr off 
            //commPort.ErrorReceived += new SerialErrorReceivedEventHandler(this.SerialErrorReceived);
            commPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialReceivedData);
            // commPort.DataReceived += new SerialDataReceivedEventHandler(datareceived);
            commPort.PinChanged += new SerialPinChangedEventHandler(this.SerialPinChanged);

            commPort.PortName = "COM" + portidx.ToString();

            commPort.Handshake = Handshake.None;
            commPort.Parity = Parity.None;
            commPort.StopBits = StopBits.One;
            commPort.DataBits = 8;
            commPort.BaudRate = 9600;
            commPort.ReadTimeout = 5000;
            commPort.WriteTimeout = 500;
            commPort.ReceivedBytesThreshold = 1;
        }
        // set the comm parms ... can only be done if port is not open -- silently fails if port is open (fixme -- add some error checking) 
        // 
        public void setCommParms(int baudrate, Parity p, int databits, StopBits stop)
        {
            if (commPort.IsOpen) return; // bail out if it's already open 

            commPort.BaudRate = baudrate;
            commPort.Parity = p;
            commPort.StopBits = stop;
            commPort.DataBits = databits;
        }

        public uint put(string s)
        {
            if (bitBangOnly) return 0;  // fixme -- throw exception?			
            commPort.Write(s);
            return (uint)s.Length; // wjt fixme -- hack -- we don't know if we actually wrote things 			
        }

        public int Create()
        {
            return Create(false);
        }

        // create port 
        public int Create(bool bit_bang_only)
        {
            bitBangOnly = bit_bang_only;
            if (isOpen) { return -1; }
            commPort.Open();
            isOpen = commPort.IsOpen;
            if (isOpen)
                return 0; // all is well
            else
                return -1;  //error
        }

        public void Destroy()
        {
            try
            {
                commPort.Close();
            }
            catch (Exception)
            {

            }
            isOpen = false;
        }

        public bool isCTS()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CtsHolding;
        }

        public bool isDSR()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.DsrHolding;

        }
        public bool isRI()
        {
            if (!isOpen) return false; // fixme error check 
            return false;
        }

        public bool isRLSD()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CDHolding;
        }

        public void setDTR(bool v)
        {
            if (!isOpen) return;
            commPort.DtrEnable = v;
        }

        void SerialErrorReceived(object source, SerialErrorReceivedEventArgs e)
        {

        }

        private bool use_for_cat_ptt = false;
        public bool UseForCATPTT
        {
            get { return use_for_cat_ptt; }
            set { use_for_cat_ptt = value; }
        }

        private bool use_for_keyptt = false;
        public bool UseForKeyPTT
        {
            get { return use_for_keyptt; }
            set { use_for_keyptt = value; }
        }

        private bool use_for_paddles = false;
        public bool UseForPaddles
        {
            get { return use_for_paddles; }
            set { use_for_paddles = value; }
        }

        private bool ptt_on_dtr = false;
        public bool PTTOnDTR
        {
            get { return ptt_on_dtr; }
            set { ptt_on_dtr = value; }
        }

        private bool ptt_on_rts = false;
        public bool PTTOnRTS
        {
            get { return ptt_on_rts; }
            set { ptt_on_rts = value; }
        }

        private bool key_on_dtr = false;
        public bool KeyOnDTR
        {
            get { return key_on_dtr; }
            set { key_on_dtr = value; }
        }

        private bool key_on_rts = false;
        public bool KeyOnRTS
        {
            get { return key_on_rts; }
            set { key_on_rts = value; }
        }

        void SerialPinChanged(object source, SerialPinChangedEventArgs e)
        {
            if (!use_for_keyptt && !use_for_paddles && !use_for_cat_ptt) return;

            if (use_for_keyptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.KeyerPTT = commPort.DsrHolding;
                        }

                        if (key_on_dtr)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.DsrHolding));
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.KeyerPTT = commPort.CtsHolding;
                        }

                        if (key_on_rts)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.CtsHolding));
                        }
                        break;
                }
            }
            else if (use_for_paddles)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:
                        NetworkIO.SetCWDot(Convert.ToInt32(commPort.DsrHolding));
                        break;
                    case SerialPinChange.CtsChanged:
                        NetworkIO.SetCWDash(Convert.ToInt32(commPort.CtsHolding));
                        break;
                }
            }

            if (use_for_cat_ptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.CATPTT = commPort.DsrHolding;
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.CATPTT = commPort.CtsHolding;
                        }
                        break;
                }

            }
        }

        void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
        {
            serial_rx_event(this, new SerialRXEvent(commPort.ReadExisting()));
        }

    }

    public class SDRSerialPort3
    {
        public static event SerialRXEventHandler serial_rx_event;

        private SerialPort commPort;
        public SerialPort BasePort
        {
            get { return commPort; }
        }

        private bool isOpen = false;
        private bool bitBangOnly = false;

        //Added 2/14/2008 BT
        public bool IsOpen
        {
            get { return commPort.IsOpen; }
        }

        public void Open()
        {
            commPort.Open();
        }

        public void Close()
        {
            commPort.Close();
        }

        public static Parity StringToParity(string s)
        {
            if (s == "none") return Parity.None;
            if (s == "odd") return Parity.Odd;
            if (s == "even") return Parity.Even;
            if (s == "space") return Parity.Space;
            if (s == "mark") return Parity.Mark;
            return Parity.None;  // error -- default to none
        }

        public static StopBits StringToStopBits(string s)
        {
            if (s == "0") return StopBits.None;
            if (s == "1") return StopBits.One;
            if (s == "1.5") return StopBits.OnePointFive;
            if (s == "2") return StopBits.Two;
            return StopBits.One; // error -- default 
        }

        public SDRSerialPort3(int portidx) //, SerialDataReceivedEventHandler datareceived)
        {
            commPort = new SerialPort();
            commPort.Encoding = System.Text.Encoding.ASCII;
            commPort.RtsEnable = true; // hack for soft rock ptt 
            commPort.DtrEnable = true; // set dtr off 
            //commPort.ErrorReceived += new SerialErrorReceivedEventHandler(this.SerialErrorReceived);
            commPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialReceivedData);
            // commPort.DataReceived += new SerialDataReceivedEventHandler(datareceived);
            commPort.PinChanged += new SerialPinChangedEventHandler(this.SerialPinChanged);

            commPort.PortName = "COM" + portidx.ToString();

            commPort.Handshake = Handshake.None;
            commPort.Parity = Parity.None;
            commPort.StopBits = StopBits.One;
            commPort.DataBits = 8;
            commPort.BaudRate = 9600;
            commPort.ReadTimeout = 5000;
            commPort.WriteTimeout = 500;
            commPort.ReceivedBytesThreshold = 1;
        }
        // set the comm parms ... can only be done if port is not open -- silently fails if port is open (fixme -- add some error checking) 
        // 
        public void setCommParms(int baudrate, Parity p, int databits, StopBits stop)
        {
            if (commPort.IsOpen) return; // bail out if it's already open 

            commPort.BaudRate = baudrate;
            commPort.Parity = p;
            commPort.StopBits = stop;
            commPort.DataBits = databits;
        }

        public uint put(string s)
        {
            if (bitBangOnly) return 0;  // fixme -- throw exception?			
            commPort.Write(s);
            return (uint)s.Length; // wjt fixme -- hack -- we don't know if we actually wrote things 			
        }

        public int Create()
        {
            return Create(false);
        }

        // create port 
        public int Create(bool bit_bang_only)
        {
            bitBangOnly = bit_bang_only;
            if (isOpen) { return -1; }
            commPort.Open();
            isOpen = commPort.IsOpen;
            if (isOpen)
                return 0; // all is well
            else
                return -1;  //error
        }

        public void Destroy()
        {
            try
            {
                commPort.Close();
            }
            catch (Exception)
            {

            }
            isOpen = false;
        }

        public bool isCTS()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CtsHolding;
        }

        public bool isDSR()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.DsrHolding;

        }
        public bool isRI()
        {
            if (!isOpen) return false; // fixme error check 
            return false;
        }

        public bool isRLSD()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CDHolding;
        }

        public void setDTR(bool v)
        {
            if (!isOpen) return;
            commPort.DtrEnable = v;
        }

        void SerialErrorReceived(object source, SerialErrorReceivedEventArgs e)
        {

        }

        private bool use_for_cat_ptt = false;
        public bool UseForCATPTT
        {
            get { return use_for_cat_ptt; }
            set { use_for_cat_ptt = value; }
        }

        private bool use_for_keyptt = false;
        public bool UseForKeyPTT
        {
            get { return use_for_keyptt; }
            set { use_for_keyptt = value; }
        }

        private bool use_for_paddles = false;
        public bool UseForPaddles
        {
            get { return use_for_paddles; }
            set { use_for_paddles = value; }
        }

        private bool ptt_on_dtr = false;
        public bool PTTOnDTR
        {
            get { return ptt_on_dtr; }
            set { ptt_on_dtr = value; }
        }

        private bool ptt_on_rts = false;
        public bool PTTOnRTS
        {
            get { return ptt_on_rts; }
            set { ptt_on_rts = value; }
        }

        private bool key_on_dtr = false;
        public bool KeyOnDTR
        {
            get { return key_on_dtr; }
            set { key_on_dtr = value; }
        }

        private bool key_on_rts = false;
        public bool KeyOnRTS
        {
            get { return key_on_rts; }
            set { key_on_rts = value; }
        }

        void SerialPinChanged(object source, SerialPinChangedEventArgs e)
        {
            if (!use_for_keyptt && !use_for_paddles && !use_for_cat_ptt) return;

            if (use_for_keyptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.KeyerPTT = commPort.DsrHolding;
                        }

                        if (key_on_dtr)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.DsrHolding));
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.KeyerPTT = commPort.CtsHolding;
                        }

                        if (key_on_rts)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.CtsHolding));
                        }
                        break;
                }
            }
            else if (use_for_paddles)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:
                        NetworkIO.SetCWDot(Convert.ToInt32(commPort.DsrHolding));
                        break;
                    case SerialPinChange.CtsChanged:
                        NetworkIO.SetCWDash(Convert.ToInt32(commPort.CtsHolding));
                        break;
                }
            }

            if (use_for_cat_ptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.CATPTT = commPort.DsrHolding;
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.CATPTT = commPort.CtsHolding;
                        }
                        break;
                }

            }
        }

        void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
        {
            serial_rx_event(this, new SerialRXEvent(commPort.ReadExisting()));
        }

    }

    public class SDRSerialPort4
    {
        public static event SerialRXEventHandler serial_rx_event;

        private SerialPort commPort;
        public SerialPort BasePort
        {
            get { return commPort; }
        }

        private bool isOpen = false;
        private bool bitBangOnly = false;

        //Added 2/14/2008 BT
        public bool IsOpen
        {
            get { return commPort.IsOpen; }
        }

        public void Open()
        {
            commPort.Open();
        }

        public void Close()
        {
            commPort.Close();
        }

        public static Parity StringToParity(string s)
        {
            if (s == "none") return Parity.None;
            if (s == "odd") return Parity.Odd;
            if (s == "even") return Parity.Even;
            if (s == "space") return Parity.Space;
            if (s == "mark") return Parity.Mark;
            return Parity.None;  // error -- default to none
        }

        public static StopBits StringToStopBits(string s)
        {
            if (s == "0") return StopBits.None;
            if (s == "1") return StopBits.One;
            if (s == "1.5") return StopBits.OnePointFive;
            if (s == "2") return StopBits.Two;
            return StopBits.One; // error -- default 
        }

        public SDRSerialPort4(int portidx) //, SerialDataReceivedEventHandler datareceived)
        {
            commPort = new SerialPort();
            commPort.Encoding = System.Text.Encoding.ASCII;
            commPort.RtsEnable = true; // hack for soft rock ptt 
            commPort.DtrEnable = true; // set dtr off 
            //commPort.ErrorReceived += new SerialErrorReceivedEventHandler(this.SerialErrorReceived);
            commPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialReceivedData);
            // commPort.DataReceived += new SerialDataReceivedEventHandler(datareceived);
            commPort.PinChanged += new SerialPinChangedEventHandler(this.SerialPinChanged);

            commPort.PortName = "COM" + portidx.ToString();

            commPort.Handshake = Handshake.None;
            commPort.Parity = Parity.None;
            commPort.StopBits = StopBits.One;
            commPort.DataBits = 8;
            commPort.BaudRate = 9600;
            commPort.ReadTimeout = 5000;
            commPort.WriteTimeout = 500;
            commPort.ReceivedBytesThreshold = 1;
        }
        // set the comm parms ... can only be done if port is not open -- silently fails if port is open (fixme -- add some error checking) 
        // 
        public void setCommParms(int baudrate, Parity p, int databits, StopBits stop)
        {
            if (commPort.IsOpen) return; // bail out if it's already open 

            commPort.BaudRate = baudrate;
            commPort.Parity = p;
            commPort.StopBits = stop;
            commPort.DataBits = databits;
        }

        public uint put(string s)
        {
            if (bitBangOnly) return 0;  // fixme -- throw exception?			
            commPort.Write(s);
            return (uint)s.Length; // wjt fixme -- hack -- we don't know if we actually wrote things 			
        }

        public int Create()
        {
            return Create(false);
        }

        // create port 
        public int Create(bool bit_bang_only)
        {
            bitBangOnly = bit_bang_only;
            if (isOpen) { return -1; }
            commPort.Open();
            isOpen = commPort.IsOpen;
            if (isOpen)
                return 0; // all is well
            else
                return -1;  //error
        }

        public void Destroy()
        {
            try
            {
                commPort.Close();
            }
            catch (Exception)
            {

            }
            isOpen = false;
        }

        public bool isCTS()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CtsHolding;
        }

        public bool isDSR()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.DsrHolding;

        }
        public bool isRI()
        {
            if (!isOpen) return false; // fixme error check 
            return false;
        }

        public bool isRLSD()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CDHolding;
        }

        public void setDTR(bool v)
        {
            if (!isOpen) return;
            commPort.DtrEnable = v;
        }

        void SerialErrorReceived(object source, SerialErrorReceivedEventArgs e)
        {

        }

        private bool use_for_cat_ptt = false;
        public bool UseForCATPTT
        {
            get { return use_for_cat_ptt; }
            set { use_for_cat_ptt = value; }
        }

        private bool use_for_keyptt = false;
        public bool UseForKeyPTT
        {
            get { return use_for_keyptt; }
            set { use_for_keyptt = value; }
        }

        private bool use_for_paddles = false;
        public bool UseForPaddles
        {
            get { return use_for_paddles; }
            set { use_for_paddles = value; }
        }

        private bool ptt_on_dtr = false;
        public bool PTTOnDTR
        {
            get { return ptt_on_dtr; }
            set { ptt_on_dtr = value; }
        }

        private bool ptt_on_rts = false;
        public bool PTTOnRTS
        {
            get { return ptt_on_rts; }
            set { ptt_on_rts = value; }
        }

        private bool key_on_dtr = false;
        public bool KeyOnDTR
        {
            get { return key_on_dtr; }
            set { key_on_dtr = value; }
        }

        private bool key_on_rts = false;
        public bool KeyOnRTS
        {
            get { return key_on_rts; }
            set { key_on_rts = value; }
        }

        void SerialPinChanged(object source, SerialPinChangedEventArgs e)
        {
            if (!use_for_keyptt && !use_for_paddles && !use_for_cat_ptt) return;

            if (use_for_keyptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.KeyerPTT = commPort.DsrHolding;
                        }

                        if (key_on_dtr)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.DsrHolding));
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.KeyerPTT = commPort.CtsHolding;
                        }

                        if (key_on_rts)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.CtsHolding));
                        }
                        break;
                }
            }
            else if (use_for_paddles)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:
                        NetworkIO.SetCWDot(Convert.ToInt32(commPort.DsrHolding));
                        break;
                    case SerialPinChange.CtsChanged:
                        NetworkIO.SetCWDash(Convert.ToInt32(commPort.CtsHolding));
                        break;
                }
            }

            if (use_for_cat_ptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.CATPTT = commPort.DsrHolding;
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.CATPTT = commPort.CtsHolding;
                        }
                        break;
                }

            }
        }

        void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
        {
            serial_rx_event(this, new SerialRXEvent(commPort.ReadExisting()));
        }

    }

    // SDRSerialPort5 is used for Andromeda front panel communication
        public class SDRSerialPort5
    {
        public static event SerialRXEventHandler serial_rx_event;

        private SerialPort commPort;
        public SerialPort BasePort
        {
            get { return commPort; }
        }

        private bool isOpen = false;
        private bool bitBangOnly = false;

        //Added 2/14/2008 BT
        public bool IsOpen
        {
            get { return commPort.IsOpen; }
        }

        public void Open()
        {
            commPort.Open();
        }

        public void Close()
        {
            commPort.Close();
        }

        public static Parity StringToParity(string s)
        {
            if (s == "none") return Parity.None;
            if (s == "odd") return Parity.Odd;
            if (s == "even") return Parity.Even;
            if (s == "space") return Parity.Space;
            if (s == "mark") return Parity.Mark;
            return Parity.None;  // error -- default to none
        }

        public static StopBits StringToStopBits(string s)
        {
            if (s == "0") return StopBits.None;
            if (s == "1") return StopBits.One;
            if (s == "1.5") return StopBits.OnePointFive;
            if (s == "2") return StopBits.Two;
            return StopBits.One; // error -- default 
        }

        public SDRSerialPort5(int portidx) //, SerialDataReceivedEventHandler datareceived)
        {
            commPort = new SerialPort();
            commPort.Encoding = System.Text.Encoding.ASCII;
            commPort.RtsEnable = true; // hack for soft rock ptt 
            commPort.DtrEnable = true; // set dtr off 
            //commPort.ErrorReceived += new SerialErrorReceivedEventHandler(this.SerialErrorReceived);
            commPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialReceivedData);
            // commPort.DataReceived += new SerialDataReceivedEventHandler(datareceived);
            commPort.PinChanged += new SerialPinChangedEventHandler(this.SerialPinChanged);

            commPort.PortName = "COM" + portidx.ToString();

            commPort.Handshake = Handshake.None;
            commPort.Parity = Parity.None;
            commPort.StopBits = StopBits.One;
            commPort.DataBits = 8;
            commPort.BaudRate = 9600;
            commPort.ReadTimeout = 5000;
            commPort.WriteTimeout = 500;
            commPort.ReceivedBytesThreshold = 1;
        }
        // set the comm parms ... can only be done if port is not open -- silently fails if port is open (fixme -- add some error checking) 
        // 
        public void setCommParms(int baudrate, Parity p, int databits, StopBits stop)
        {
            if (commPort.IsOpen) return; // bail out if it's already open 

            commPort.BaudRate = baudrate;
            commPort.Parity = p;
            commPort.StopBits = stop;
            commPort.DataBits = databits;
        }

        public uint put(string s)
        {
            if (bitBangOnly) return 0;  // fixme -- throw exception?			
            commPort.Write(s);
            return (uint)s.Length; // wjt fixme -- hack -- we don't know if we actually wrote things 			
        }

        public int Create()
        {
            return Create(false);
        }

        // create port 
        public int Create(bool bit_bang_only)
        {
            bitBangOnly = bit_bang_only;
            if (isOpen) { return -1; }
            commPort.Open();
            isOpen = commPort.IsOpen;
            if (isOpen)
                return 0; // all is well
            else
                return -1;  //error
        }

        public void Destroy()
        {
            try
            {
                commPort.Close();
            }
            catch (Exception)
            {

            }
            isOpen = false;
        }

        public bool isCTS()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CtsHolding;
        }

        public bool isDSR()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.DsrHolding;

        }
        public bool isRI()
        {
            if (!isOpen) return false; // fixme error check 
            return false;
        }

        public bool isRLSD()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CDHolding;
        }

        public void setDTR(bool v)
        {
            if (!isOpen) return;
            commPort.DtrEnable = v;
        }

        void SerialErrorReceived(object source, SerialErrorReceivedEventArgs e)
        {

        }

        private bool use_for_cat_ptt = false;
        public bool UseForCATPTT
        {
            get { return use_for_cat_ptt; }
            set { use_for_cat_ptt = value; }
        }

        private bool use_for_keyptt = false;
        public bool UseForKeyPTT
        {
            get { return use_for_keyptt; }
            set { use_for_keyptt = value; }
        }

        private bool use_for_paddles = false;
        public bool UseForPaddles
        {
            get { return use_for_paddles; }
            set { use_for_paddles = value; }
        }

        private bool ptt_on_dtr = false;
        public bool PTTOnDTR
        {
            get { return ptt_on_dtr; }
            set { ptt_on_dtr = value; }
        }

        private bool ptt_on_rts = false;
        public bool PTTOnRTS
        {
            get { return ptt_on_rts; }
            set { ptt_on_rts = value; }
        }

        private bool key_on_dtr = false;
        public bool KeyOnDTR
        {
            get { return key_on_dtr; }
            set { key_on_dtr = value; }
        }

        private bool key_on_rts = false;
        public bool KeyOnRTS
        {
            get { return key_on_rts; }
            set { key_on_rts = value; }
        }

        void SerialPinChanged(object source, SerialPinChangedEventArgs e)
        {
            if (!use_for_keyptt && !use_for_paddles && !use_for_cat_ptt) return;

            if (use_for_keyptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.KeyerPTT = commPort.DsrHolding;
                        }

                        if (key_on_dtr)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.DsrHolding));
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.KeyerPTT = commPort.CtsHolding;
                        }

                        if (key_on_rts)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.CtsHolding));
                        }
                        break;
                }
            }
            else if (use_for_paddles)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:
                        NetworkIO.SetCWDot(Convert.ToInt32(commPort.DsrHolding));
                        break;
                    case SerialPinChange.CtsChanged:
                        NetworkIO.SetCWDash(Convert.ToInt32(commPort.CtsHolding));
                        break;
                }
            }

            if (use_for_cat_ptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.CATPTT = commPort.DsrHolding;
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.CATPTT = commPort.CtsHolding;
                        }
                        break;
                }

            }
        }

        void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
        {
            serial_rx_event(this, new SerialRXEvent(commPort.ReadExisting()));
        }

    }

    // SDRSerialPort6 is used for Aries ATU communication
    public class SDRSerialPort6
    {
        public static event SerialRXEventHandler serial_rx_event;

        private SerialPort commPort;
        public SerialPort BasePort
        {
            get { return commPort; }
        }

        private bool isOpen = false;
        private bool bitBangOnly = false;

        //Added 2/14/2008 BT
        public bool IsOpen
        {
            get { return commPort.IsOpen; }
        }

        public void Open()
        {
            commPort.Open();
        }

        public void Close()
        {
            commPort.Close();
        }

        public static Parity StringToParity(string s)
        {
            if (s == "none") return Parity.None;
            if (s == "odd") return Parity.Odd;
            if (s == "even") return Parity.Even;
            if (s == "space") return Parity.Space;
            if (s == "mark") return Parity.Mark;
            return Parity.None;  // error -- default to none
        }

        public static StopBits StringToStopBits(string s)
        {
            if (s == "0") return StopBits.None;
            if (s == "1") return StopBits.One;
            if (s == "1.5") return StopBits.OnePointFive;
            if (s == "2") return StopBits.Two;
            return StopBits.One; // error -- default 
        }

        public SDRSerialPort6(int portidx) //, SerialDataReceivedEventHandler datareceived)
        {
            commPort = new SerialPort();
            commPort.Encoding = System.Text.Encoding.ASCII;
            commPort.RtsEnable = true; // hack for soft rock ptt 
            commPort.DtrEnable = true; // set dtr off 
            //commPort.ErrorReceived += new SerialErrorReceivedEventHandler(this.SerialErrorReceived);
            commPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialReceivedData);
            // commPort.DataReceived += new SerialDataReceivedEventHandler(datareceived);
            commPort.PinChanged += new SerialPinChangedEventHandler(this.SerialPinChanged);

            commPort.PortName = "COM" + portidx.ToString();

            commPort.Handshake = Handshake.None;
            commPort.Parity = Parity.None;
            commPort.StopBits = StopBits.One;
            commPort.DataBits = 8;
            commPort.BaudRate = 9600;
            commPort.ReadTimeout = 5000;
            commPort.WriteTimeout = 500;
            commPort.ReceivedBytesThreshold = 1;
        }
        // set the comm parms ... can only be done if port is not open -- silently fails if port is open (fixme -- add some error checking) 
        // 
        public void setCommParms(int baudrate, Parity p, int databits, StopBits stop)
        {
            if (commPort.IsOpen) return; // bail out if it's already open 

            commPort.BaudRate = baudrate;
            commPort.Parity = p;
            commPort.StopBits = stop;
            commPort.DataBits = databits;
        }

        public uint put(string s)
        {
            if (bitBangOnly) return 0;  // fixme -- throw exception?			
            commPort.Write(s);
            return (uint)s.Length; // wjt fixme -- hack -- we don't know if we actually wrote things 			
        }

        public int Create()
        {
            return Create(false);
        }

        // create port 
        public int Create(bool bit_bang_only)
        {
            bitBangOnly = bit_bang_only;
            if (isOpen) { return -1; }
            commPort.Open();
            isOpen = commPort.IsOpen;
            if (isOpen)
                return 0; // all is well
            else
                return -1;  //error
        }

        public void Destroy()
        {
            try
            {
                commPort.Close();
            }
            catch (Exception)
            {

            }
            isOpen = false;
        }

        public bool isCTS()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CtsHolding;
        }

        public bool isDSR()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.DsrHolding;

        }
        public bool isRI()
        {
            if (!isOpen) return false; // fixme error check 
            return false;
        }

        public bool isRLSD()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CDHolding;
        }

        public void setDTR(bool v)
        {
            if (!isOpen) return;
            commPort.DtrEnable = v;
        }

        void SerialErrorReceived(object source, SerialErrorReceivedEventArgs e)
        {

        }

        private bool use_for_cat_ptt = false;
        public bool UseForCATPTT
        {
            get { return use_for_cat_ptt; }
            set { use_for_cat_ptt = value; }
        }

        private bool use_for_keyptt = false;
        public bool UseForKeyPTT
        {
            get { return use_for_keyptt; }
            set { use_for_keyptt = value; }
        }

        private bool use_for_paddles = false;
        public bool UseForPaddles
        {
            get { return use_for_paddles; }
            set { use_for_paddles = value; }
        }

        private bool ptt_on_dtr = false;
        public bool PTTOnDTR
        {
            get { return ptt_on_dtr; }
            set { ptt_on_dtr = value; }
        }

        private bool ptt_on_rts = false;
        public bool PTTOnRTS
        {
            get { return ptt_on_rts; }
            set { ptt_on_rts = value; }
        }

        private bool key_on_dtr = false;
        public bool KeyOnDTR
        {
            get { return key_on_dtr; }
            set { key_on_dtr = value; }
        }

        private bool key_on_rts = false;
        public bool KeyOnRTS
        {
            get { return key_on_rts; }
            set { key_on_rts = value; }
        }

        void SerialPinChanged(object source, SerialPinChangedEventArgs e)
        {
            if (!use_for_keyptt && !use_for_paddles && !use_for_cat_ptt) return;

            if (use_for_keyptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.KeyerPTT = commPort.DsrHolding;
                        }

                        if (key_on_dtr)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.DsrHolding));
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.KeyerPTT = commPort.CtsHolding;
                        }

                        if (key_on_rts)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.CtsHolding));
                        }
                        break;
                }
            }
            else if (use_for_paddles)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:
                        NetworkIO.SetCWDot(Convert.ToInt32(commPort.DsrHolding));
                        break;
                    case SerialPinChange.CtsChanged:
                        NetworkIO.SetCWDash(Convert.ToInt32(commPort.CtsHolding));
                        break;
                }
            }

            if (use_for_cat_ptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.CATPTT = commPort.DsrHolding;
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.CATPTT = commPort.CtsHolding;
                        }
                        break;
                }

            }
        }

        void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
        {
            serial_rx_event(this, new SerialRXEvent(commPort.ReadExisting()));
        }

    }

    // SDRSerialPort7 is used for Gsanymede 500W PA communication
    public class SDRSerialPort7
    {
        public static event SerialRXEventHandler serial_rx_event;

        private SerialPort commPort;
        public SerialPort BasePort
        {
            get { return commPort; }
        }

        private bool isOpen = false;
        private bool bitBangOnly = false;

        //Added 2/14/2008 BT
        public bool IsOpen
        {
            get { return commPort.IsOpen; }
        }

        public void Open()
        {
            commPort.Open();
        }

        public void Close()
        {
            commPort.Close();
        }

        public static Parity StringToParity(string s)
        {
            if (s == "none") return Parity.None;
            if (s == "odd") return Parity.Odd;
            if (s == "even") return Parity.Even;
            if (s == "space") return Parity.Space;
            if (s == "mark") return Parity.Mark;
            return Parity.None;  // error -- default to none
        }

        public static StopBits StringToStopBits(string s)
        {
            if (s == "0") return StopBits.None;
            if (s == "1") return StopBits.One;
            if (s == "1.5") return StopBits.OnePointFive;
            if (s == "2") return StopBits.Two;
            return StopBits.One; // error -- default 
        }

        public SDRSerialPort7(int portidx) //, SerialDataReceivedEventHandler datareceived)
        {
            commPort = new SerialPort();
            commPort.Encoding = System.Text.Encoding.ASCII;
            commPort.RtsEnable = true; // hack for soft rock ptt 
            commPort.DtrEnable = true; // set dtr off 
            //commPort.ErrorReceived += new SerialErrorReceivedEventHandler(this.SerialErrorReceived);
            commPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialReceivedData);
            // commPort.DataReceived += new SerialDataReceivedEventHandler(datareceived);
            commPort.PinChanged += new SerialPinChangedEventHandler(this.SerialPinChanged);

            commPort.PortName = "COM" + portidx.ToString();

            commPort.Handshake = Handshake.None;
            commPort.Parity = Parity.None;
            commPort.StopBits = StopBits.One;
            commPort.DataBits = 8;
            commPort.BaudRate = 9600;
            commPort.ReadTimeout = 5000;
            commPort.WriteTimeout = 500;
            commPort.ReceivedBytesThreshold = 1;
        }
        // set the comm parms ... can only be done if port is not open -- silently fails if port is open (fixme -- add some error checking) 
        // 
        public void setCommParms(int baudrate, Parity p, int databits, StopBits stop)
        {
            if (commPort.IsOpen) return; // bail out if it's already open 

            commPort.BaudRate = baudrate;
            commPort.Parity = p;
            commPort.StopBits = stop;
            commPort.DataBits = databits;
        }

        public uint put(string s)
        {
            if (bitBangOnly) return 0;  // fixme -- throw exception?			
            commPort.Write(s);
            return (uint)s.Length; // wjt fixme -- hack -- we don't know if we actually wrote things 			
        }

        public int Create()
        {
            return Create(false);
        }

        // create port 
        public int Create(bool bit_bang_only)
        {
            bitBangOnly = bit_bang_only;
            if (isOpen) { return -1; }
            commPort.Open();
            isOpen = commPort.IsOpen;
            if (isOpen)
                return 0; // all is well
            else
                return -1;  //error
        }

        public void Destroy()
        {
            try
            {
                commPort.Close();
            }
            catch (Exception)
            {

            }
            isOpen = false;
        }

        public bool isCTS()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CtsHolding;
        }

        public bool isDSR()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.DsrHolding;

        }
        public bool isRI()
        {
            if (!isOpen) return false; // fixme error check 
            return false;
        }

        public bool isRLSD()
        {
            if (!isOpen) return false; // fixme error check 
            return commPort.CDHolding;
        }

        public void setDTR(bool v)
        {
            if (!isOpen) return;
            commPort.DtrEnable = v;
        }

        void SerialErrorReceived(object source, SerialErrorReceivedEventArgs e)
        {

        }

        private bool use_for_cat_ptt = false;
        public bool UseForCATPTT
        {
            get { return use_for_cat_ptt; }
            set { use_for_cat_ptt = value; }
        }

        private bool use_for_keyptt = false;
        public bool UseForKeyPTT
        {
            get { return use_for_keyptt; }
            set { use_for_keyptt = value; }
        }

        private bool use_for_paddles = false;
        public bool UseForPaddles
        {
            get { return use_for_paddles; }
            set { use_for_paddles = value; }
        }

        private bool ptt_on_dtr = false;
        public bool PTTOnDTR
        {
            get { return ptt_on_dtr; }
            set { ptt_on_dtr = value; }
        }

        private bool ptt_on_rts = false;
        public bool PTTOnRTS
        {
            get { return ptt_on_rts; }
            set { ptt_on_rts = value; }
        }

        private bool key_on_dtr = false;
        public bool KeyOnDTR
        {
            get { return key_on_dtr; }
            set { key_on_dtr = value; }
        }

        private bool key_on_rts = false;
        public bool KeyOnRTS
        {
            get { return key_on_rts; }
            set { key_on_rts = value; }
        }

        void SerialPinChanged(object source, SerialPinChangedEventArgs e)
        {
            if (!use_for_keyptt && !use_for_paddles && !use_for_cat_ptt) return;

            if (use_for_keyptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.KeyerPTT = commPort.DsrHolding;
                        }

                        if (key_on_dtr)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.DsrHolding));
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.KeyerPTT = commPort.CtsHolding;
                        }

                        if (key_on_rts)
                        {
                            NetworkIO.SetCWX(Convert.ToInt32(commPort.CtsHolding));
                        }
                        break;
                }
            }
            else if (use_for_paddles)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:
                        NetworkIO.SetCWDot(Convert.ToInt32(commPort.DsrHolding));
                        break;
                    case SerialPinChange.CtsChanged:
                        NetworkIO.SetCWDash(Convert.ToInt32(commPort.CtsHolding));
                        break;
                }
            }

            if (use_for_cat_ptt)
            {
                switch (e.EventType)
                {
                    case SerialPinChange.DsrChanged:

                        if (ptt_on_dtr)
                        {
                            CWInput.CATPTT = commPort.DsrHolding;
                        }
                        break;
                    case SerialPinChange.CtsChanged:

                        if (ptt_on_rts)
                        {
                            CWInput.CATPTT = commPort.CtsHolding;
                        }
                        break;
                }

            }
        }

        void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
        {
            serial_rx_event(this, new SerialRXEvent(commPort.ReadExisting()));
        }

    }


}
