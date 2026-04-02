/*
*
* Copyright (C) 2025 Reid Campbell, MI0BOT, mi0bot@trom.uk 
* This program is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

// This module contains code to support the I/O Board used in the Hermes Lite 2 
// 
// 

using System;
using System.Threading;
using System.Threading.Tasks;
using Thetis;

namespace Thetis
{
	/// <summary>
	/// Summary description for I/O Board
	/// </summary>
	using System.Diagnostics;
    using Microsoft.Win32;
    using static Thetis.IOBoard;

    public class IOBoard
	{
		public enum Registers
		{
            HardwareVersion		= -1,
            REG_TX_FREQ_BYTE4	= 0,
			REG_TX_FREQ_BYTE3	= 1,
			REG_TX_FREQ_BYTE2	= 2,
			REG_TX_FREQ_BYTE1	= 3,	
			REG_TX_FREQ_BYTE0	= 4,
            REG_CONTROL			= 5,
            REG_INPUT_PINS		= 6,
            REG_ANTENNA_TUNER	= 7,
            REG_FAULT			= 8,
            REG_FIRMWARE_MAJOR	= 9,
			REG_FIRMWARE_MINOR	= 10,
            REG_RF_INPUTS		= 11,
            REG_FAN_SPEED		= 12,
            REG_FCODE_RX1		= 13,
			REG_FCODE_RX2		= 14,
			REG_FCODE_RX3		= 15,
			REG_FCODE_RX4		= 16,
			REG_FCODE_RX5		= 17,
            REG_FCODE_RX6		= 18,
            REG_FCODE_RX7		= 19,
            REG_FCODE_RX8		= 20,
            REG_FCODE_RX9		= 21,
            REG_FCODE_RX10		= 22,
            REG_FCODE_RX11		= 23,
            REG_FCODE_RX12		= 24,
            REG_ADC0_MSB		= 25,
			REG_ADC0_LSB		= 26,
			REG_ADC1_MSB		= 27,
			REG_ADC1_LSB		= 28,
			REG_ADC2_MSB		= 29,
			REG_ADC2_LSB		= 30,
            REG_ANTENNA			= 31,
            REG_OP_MODE			= 32,

            REG_STATUS			= 167,
            REG_IN_PINS			= 168,
			REG_OUT_PINS		= 169,
            GPIO_DIRECT_BASE	= 170
        }

        public enum HardwareVersion
		{
			Version_1 = 0xf1,
		}

		private static IOBoard theSingleton = null; 
        private static Console console;
		public byte hardwareVersion { set;  get; } = 0;

		private byte[] registers = new byte[256];
		private int lastReadRequest = 0;
		private long currentFreq = 0;


        public static IOBoard getIOBoard(Console c) 
		{ 
			lock ( typeof(IOBoard) ) 
			{
				if ( theSingleton == null ) 
				{ 
					theSingleton = new IOBoard();
                    console = c;
				} 
			}
			return theSingleton; 
		} 
		public static IOBoard getIOBoard() 
		{ 
			lock ( typeof(IOBoard) ) 
			{
				if ( theSingleton == null ) 
				{ 
					theSingleton = new IOBoard(); 
				} 
			}
			return theSingleton; 
		}

		private IOBoard()
		{
            for (int i = 0; i < 256; i++)
            {
                registers[i] = 0;
            }
        }


        public byte readRequest(Registers readRequest)
		{
			byte returnCode = 0;

            if (readRequest == Registers.HardwareVersion)
			{
                returnCode = (byte)NetworkIO.I2CReadInitiate(1, 0x41, 0);	// Hardware version is stored at a different address 
			}																// in register 0
			else
			{
                returnCode = (byte)NetworkIO.I2CReadInitiate(1, 0x1d, (int)readRequest);
			}

			if (returnCode == 0)
				lastReadRequest = (int) readRequest;

			return returnCode;
        }

		public byte readResponse()
		{
			byte[] read_data = new byte[4];

            byte returnCode = (byte) NetworkIO.I2CResponse(read_data);

			if( returnCode == 0)
			{
				if (lastReadRequest == (int)Registers.HardwareVersion)
				{
                    hardwareVersion = read_data[3];
				}
				else
				{
					registers[lastReadRequest+0] = read_data[3];
					registers[lastReadRequest+1] = read_data[2];
					registers[lastReadRequest+2] = read_data[1];
					registers[lastReadRequest+3] = read_data[0];
				}
			}

			return returnCode;
        }
		public byte readRegister(Registers readRequest)
		{
			return registers[(int)readRequest];
		}

        public void writeRequest(Registers writeRequest, byte writeData)
        {
			registers[(int) writeRequest] = writeData;

            NetworkIO.I2CWrite(1, 0x1d, (int) writeRequest, writeData);
        }

		public void setFrequency(long frequency)
		{
			if(currentFreq != frequency)
			{
                // Write frequency on bus 2 at address 0x1d into the five registers
                registers[(int)IOBoard.Registers.REG_TX_FREQ_BYTE4] = (byte)(frequency >> 32);
                registers[(int)IOBoard.Registers.REG_TX_FREQ_BYTE3] = (byte)(frequency >> 24);
                registers[(int)IOBoard.Registers.REG_TX_FREQ_BYTE2] = (byte)(frequency >> 16);
                registers[(int)IOBoard.Registers.REG_TX_FREQ_BYTE1] = (byte)(frequency >> 08);
                registers[(int)IOBoard.Registers.REG_TX_FREQ_BYTE0] = (byte)(frequency >> 00);

				NetworkIO.I2CWrite(1, 0x1d, (int) IOBoard.Registers.REG_TX_FREQ_BYTE4, registers[(int)IOBoard.Registers.REG_TX_FREQ_BYTE4]);
				NetworkIO.I2CWrite(1, 0x1d, (int) IOBoard.Registers.REG_TX_FREQ_BYTE3, registers[(int)IOBoard.Registers.REG_TX_FREQ_BYTE3]);
				NetworkIO.I2CWrite(1, 0x1d, (int) IOBoard.Registers.REG_TX_FREQ_BYTE2, registers[(int)IOBoard.Registers.REG_TX_FREQ_BYTE2]);
				NetworkIO.I2CWrite(1, 0x1d, (int) IOBoard.Registers.REG_TX_FREQ_BYTE1, registers[(int)IOBoard.Registers.REG_TX_FREQ_BYTE1]);
				NetworkIO.I2CWrite(1, 0x1d, (int) IOBoard.Registers.REG_TX_FREQ_BYTE0, registers[(int)IOBoard.Registers.REG_TX_FREQ_BYTE0]);

				currentFreq = frequency;
            }
        }
    }
}
