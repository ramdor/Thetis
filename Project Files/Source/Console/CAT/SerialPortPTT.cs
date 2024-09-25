//=================================================================
// SerialPortPTT.cs
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
// This class is used to implement a PTT using RTS or DTS 
//=================================================================

#define DBG_PRINT

using System; 
using System.IO.Ports;

namespace Thetis
{
	/// <summary>
	/// Summary description for SerialPortPTT.
	/// </summary>
	public class SerialPortPTT
	{
        public static event SerialRXEventHandler serial_rx_event;

		// instance vars 
		// 
		private int portNum = 0; 
		private bool rtsIsPTT = false; 
		public bool RTSIsPTT 
		{
			get {return rtsIsPTT;}
			set {rtsIsPTT = value;}
		}
		private bool dtrIsPTT = false; 

		public bool DTRIsPTT 
		{
			get {return dtrIsPTT;}
			set {dtrIsPTT = value;}
		}
		private SDRSerialPort commPort = null; 
		private bool Initialized = false; 

		//
		// 
		//

		public SerialPortPTT(int portidx, bool rts_is_ptt, bool dtr_is_ptt)
		{
			portNum = portidx; 
			rtsIsPTT = rts_is_ptt; 
			dtrIsPTT = dtr_is_ptt; 
		}

		public void Init() 
		{ 
			lock ( this )  // do this only once -- keep the lock until we're ready to go less we hose up the poll ptt thread 
			{
				if ( Initialized ) return;  							
				if ( portNum == 0 ) return; // bail out 
				try
				{
					commPort = new SDRSerialPort(portNum);//, null); 
					commPort.Create(true); // true says to create bit bang only port  -- fixme needs error checking! 
					Initialized = true;
				}
				catch { }
			}
			return; 
		}

		public bool isPTT() 
		{
			try
			{
				if (!Initialized) return false;
				if (rtsIsPTT && commPort.isCTS()) return true;
				if (dtrIsPTT && commPort.isDSR()) return true;
				return false;
			}
            catch { return false; }
        }

		public bool isCTS() 
		{
			try
			{
				return commPort.isCTS();
			}
			catch { return false; }
		}

		public bool isDSR()
		{
			try
			{
				return commPort.isDSR();
			}
			catch { return false; }
		}

		public void setDTR(bool v) 
		{
			try
			{
				commPort.setDTR(v);
			}
			catch { }
		}
		
		public void Destroy() 
		{ 
			lock ( this )  // we only get in here once 
			{ 
				if ( !Initialized ) return; 
				Initialized = false; 
			}
			try
			{
				if (commPort != null)
				{
					commPort.Destroy();
					commPort = null;
				}
			} catch { }
		}

        void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
        {
            serial_rx_event(this, new SerialRXEvent(commPort.BasePort.ReadExisting()));
        }

	}
}