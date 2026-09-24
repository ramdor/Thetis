//=================================================================
// CIVProtocol.cs
//=================================================================
// Icom CI-V Protocol definitions, BCD framing, and IC-7100 mappings.
// Supported by Thetis for native Icom IC-7100 transceiver control.
//=================================================================

using System;

namespace Thetis
{
    #region CI-V Enums

    public enum CIVMode : byte
    {
        LSB = 0x00,
        USB = 0x01,
        AM = 0x02,
        CW = 0x03,
        RTTY = 0x04,
        FM = 0x05,
        WFM = 0x06,
        CW_R = 0x07,
        RTTY_R = 0x08,
        DV = 0x17
    }

    public enum CIVFilter : byte
    {
        None = 0x00,
        FIL1 = 0x01, // Wide
        FIL2 = 0x02, // Mid
        FIL3 = 0x03  // Narrow
    }

    public enum CIVDataMode : byte
    {
        OFF = 0x00,
        DATA1 = 0x01,
        DATA2 = 0x02,
        DATA3 = 0x03
    }

    #endregion

    public static class CIVProtocol
    {
        #region Constants

        public const byte PREAMBLE = 0xFE;
        public const byte EOM = 0xFD;
        public const byte ACK = 0xFB;
        public const byte NAK = 0xFA;

        public const byte DEFAULT_RADIO_ADDR = 0x88; // Default Icom IC-7100 CI-V Address
        public const byte DEFAULT_HOST_ADDR = 0xE0;  // Default PC / Controller Address
        public const byte BROADCAST_ADDR = 0x00;

        // CI-V Commands
        public const byte CMD_SEND_FREQ = 0x05;
        public const byte CMD_READ_FREQ = 0x03;
        public const byte CMD_SEND_MODE = 0x06;
        public const byte CMD_READ_MODE = 0x04;
        public const byte CMD_VFO_SEL = 0x07;
        public const byte CMD_SPLIT = 0x0F;
        public const byte CMD_MISC_1A = 0x1A;
        public const byte CMD_CONDITION_1C = 0x1C;
        public const byte CMD_VFO_FREQ_25 = 0x25;
        public const byte CMD_VFO_MODE_26 = 0x26;

        // Subcommands for 0x1A
        public const byte SUBCMD_1A_IF_FILTER_WIDTH = 0x03;
        public const byte SUBCMD_1A_DATA_MODE = 0x06;

        // Subcommands for 0x1C
        public const byte SUBCMD_1C_PTT = 0x00;

        // Subcommands for 0x07 (VFO)
        public const byte VFO_A = 0x00;
        public const byte VFO_B = 0x01;
        public const byte VFO_SWAP = 0xB0;
        public const byte VFO_EQUAL = 0xA0;

        // Subcommands for 0x0F (Split)
        public const byte SPLIT_OFF = 0x00;
        public const byte SPLIT_ON = 0x01;

        #endregion

        #region BCD Frequency Encoding & Decoding

        /// <summary>
        /// Encodes a frequency in MHz into 5 Icom Little-Endian BCD bytes (1 Hz resolution).
        /// Example: 14.074000 MHz -> [0x00, 0x40, 0x07, 0x14, 0x00]
        /// </summary>
        public static byte[] EncodeFrequency(double freqMHz)
        {
            byte[] bcd = new byte[5];
            if (freqMHz <= 0) return bcd;

            long hz = (long)Math.Round(freqMHz * 1000000.0);
            for (int i = 0; i < 5; i++)
            {
                int dLow = (int)(hz % 10);
                hz /= 10;
                int dHigh = (int)(hz % 10);
                hz /= 10;
                bcd[i] = (byte)((dHigh << 4) | (dLow & 0x0F));
            }
            return bcd;
        }

        /// <summary>
        /// Decodes Icom Little-Endian BCD bytes (4, 5, or 6 bytes) into a frequency in MHz.
        /// Strictly validates that all nibbles are valid BCD digits (0-9) and stops before EOM (0xFD).
        /// </summary>
        public static double DecodeFrequency(byte[] frame, int offset = 0)
        {
            if (frame == null || frame.Length <= offset) return 0.0;

            // Determine how many BCD bytes exist before EOM (0xFD)
            int eomIndex = -1;
            for (int j = offset; j < frame.Length; j++)
            {
                if (frame[j] == EOM)
                {
                    eomIndex = j;
                    break;
                }
            }

            int bcdLen = (eomIndex >= 0) ? (eomIndex - offset) : (frame.Length - offset);
            // Valid frequency frames contain 4 (CI-V 731 mode), 5 (standard HF/VHF/UHF), or 6 bytes (GHz)
            if (bcdLen < 4 || bcdLen > 6) return 0.0;

            long hz = 0;
            long multiplier = 1;

            for (int i = 0; i < bcdLen; i++)
            {
                byte b = frame[offset + i];
                int dLow = b & 0x0F;
                int dHigh = (b >> 4) & 0x0F;

                // Strict BCD validation: both nibbles must be decimal digits 0-9
                if (dLow > 9 || dHigh > 9)
                {
                    return 0.0;
                }

                hz += (dLow + dHigh * 10) * multiplier;
                multiplier *= 100;
            }

            return (double)hz / 1000000.0;
        }

        #endregion

        #region Frame Builders

        /// <summary>
        /// Creates a standard CI-V frame: FE FE [to] [from] [cmd] [data...] FD
        /// </summary>
        public static byte[] CreateFrame(byte toAddr, byte fromAddr, byte cmd, byte[] data = null)
        {
            int dataLen = data != null ? data.Length : 0;
            byte[] frame = new byte[6 + dataLen];
            frame[0] = PREAMBLE;
            frame[1] = PREAMBLE;
            frame[2] = toAddr;
            frame[3] = fromAddr;
            frame[4] = cmd;

            if (dataLen > 0)
            {
                Buffer.BlockCopy(data, 0, frame, 5, dataLen);
            }

            frame[frame.Length - 1] = EOM;
            return frame;
        }

        /// <summary>
        /// Creates a CI-V frame with subcommand: FE FE [to] [from] [cmd] [subcmd] [data...] FD
        /// </summary>
        public static byte[] CreateSubcmdFrame(byte toAddr, byte fromAddr, byte cmd, byte subcmd, byte[] data = null)
        {
            int dataLen = data != null ? data.Length : 0;
            byte[] frame = new byte[7 + dataLen];
            frame[0] = PREAMBLE;
            frame[1] = PREAMBLE;
            frame[2] = toAddr;
            frame[3] = fromAddr;
            frame[4] = cmd;
            frame[5] = subcmd;

            if (dataLen > 0)
            {
                Buffer.BlockCopy(data, 0, frame, 6, dataLen);
            }

            frame[frame.Length - 1] = EOM;
            return frame;
        }

        /// <summary>
        /// Frame to set operating frequency (0x05).
        /// </summary>
        public static byte[] SetFrequencyFrame(byte toAddr, byte fromAddr, double freqMHz)
        {
            byte[] bcd = EncodeFrequency(freqMHz);
            return CreateFrame(toAddr, fromAddr, CMD_SEND_FREQ, bcd);
        }

        /// <summary>
        /// Frame to set unselected VFO frequency (0x25 0x01) without swapping VFOs.
        /// </summary>
        public static byte[] SetUnselectedVfoFrequencyFrame(byte toAddr, byte fromAddr, double freqMHz)
        {
            byte[] bcd = EncodeFrequency(freqMHz);
            return CreateSubcmdFrame(toAddr, fromAddr, CMD_VFO_FREQ_25, 0x01, bcd);
        }

        /// <summary>
        /// Frame to set operating mode and filter preset (0x06).
        /// </summary>
        public static byte[] SetModeFrame(byte toAddr, byte fromAddr, CIVMode mode, CIVFilter filter)
        {
            return CreateFrame(toAddr, fromAddr, CMD_SEND_MODE, new byte[] { (byte)mode, (byte)filter });
        }

        /// <summary>
        /// Frame to set data mode (0x1A 0x06).
        /// </summary>
        public static byte[] SetDataModeFrame(byte toAddr, byte fromAddr, CIVDataMode dataMode, CIVFilter filter)
        {
            return CreateSubcmdFrame(toAddr, fromAddr, CMD_MISC_1A, SUBCMD_1A_DATA_MODE, new byte[] { (byte)dataMode, (byte)filter });
        }

        /// <summary>
        /// Frame to set IF passband filter width (0x1A 0x03).
        /// </summary>
        public static byte[] SetFilterWidthFrame(byte toAddr, byte fromAddr, byte widthBcd)
        {
            return CreateSubcmdFrame(toAddr, fromAddr, CMD_MISC_1A, SUBCMD_1A_IF_FILTER_WIDTH, new byte[] { widthBcd });
        }

        /// <summary>
        /// Frame to set PTT state (0x1C 0x00).
        /// </summary>
        public static byte[] SetPttFrame(byte toAddr, byte fromAddr, bool tx)
        {
            return CreateSubcmdFrame(toAddr, fromAddr, CMD_CONDITION_1C, SUBCMD_1C_PTT, new byte[] { (byte)(tx ? 0x01 : 0x00) });
        }

        /// <summary>
        /// Frame to query transceiver condition / PTT status (0x1C 0x00).
        /// </summary>
        public static byte[] ReadPttFrame(byte toAddr, byte fromAddr)
        {
            return CreateSubcmdFrame(toAddr, fromAddr, CMD_CONDITION_1C, SUBCMD_1C_PTT, null);
        }

        /// <summary>
        /// Frame to set Split mode (0x0F).
        /// </summary>
        public static byte[] SetSplitFrame(byte toAddr, byte fromAddr, bool splitOn)
        {
            return CreateFrame(toAddr, fromAddr, CMD_SPLIT, new byte[] { (byte)(splitOn ? SPLIT_ON : SPLIT_OFF) });
        }

        /// <summary>
        /// Frame to query Split status (0x0F).
        /// </summary>
        public static byte[] ReadSplitFrame(byte toAddr, byte fromAddr)
        {
            return CreateFrame(toAddr, fromAddr, CMD_SPLIT, null);
        }

        /// <summary>
        /// Frame to select active VFO (0x07 0x00 for VFO A, 0x07 0x01 for VFO B).
        /// </summary>
        public static byte[] SelectVfoFrame(byte toAddr, byte fromAddr, bool vfoB)
        {
            return CreateFrame(toAddr, fromAddr, CMD_VFO_SEL, new byte[] { (byte)(vfoB ? VFO_B : VFO_A) });
        }

        /// <summary>
        /// Frame to swap VFO A and VFO B registers (0x07 0xB0).
        /// </summary>
        public static byte[] SwapVfoFrame(byte toAddr, byte fromAddr)
        {
            return CreateFrame(toAddr, fromAddr, CMD_VFO_SEL, new byte[] { VFO_SWAP });
        }

        /// <summary>
        /// Frame to equalize VFO A and VFO B (0x07 0xA0).
        /// </summary>
        public static byte[] EqualVfoFrame(byte toAddr, byte fromAddr)
        {
            return CreateFrame(toAddr, fromAddr, CMD_VFO_SEL, new byte[] { VFO_EQUAL });
        }

        #endregion

        #region Mode and Filter Mappings

        /// <summary>
        /// Maps Thetis DSPMode to IC-7100 CI-V mode, filter preset, and data mode flag.
        /// </summary>
        public static void MapThetisMode(DSPMode thetisMode, int filterWidthHz, out CIVMode civMode, out CIVFilter civFilter, out CIVDataMode dataMode)
        {
            dataMode = CIVDataMode.OFF;

            // Determine filter preset (FIL1: Wide, FIL2: Mid, FIL3: Narrow)
            if (filterWidthHz <= 1800)
                civFilter = CIVFilter.FIL3;
            else if (filterWidthHz <= 2700)
                civFilter = CIVFilter.FIL2;
            else
                civFilter = CIVFilter.FIL1;

            switch (thetisMode)
            {
                case DSPMode.LSB:
                    civMode = CIVMode.LSB;
                    break;
                case DSPMode.USB:
                case DSPMode.DSB:
                case DSPMode.SPEC:
                case DSPMode.DRM:
                    civMode = CIVMode.USB;
                    break;
                case DSPMode.CWL:
                    civMode = CIVMode.CW;
                    break;
                case DSPMode.CWU:
                    civMode = CIVMode.CW_R;
                    break;
                case DSPMode.AM:
                case DSPMode.SAM:
                    civMode = CIVMode.AM;
                    break;
                case DSPMode.FM:
                    civMode = CIVMode.FM;
                    break;
                case DSPMode.DIGL:
                    civMode = CIVMode.LSB;
                    dataMode = CIVDataMode.DATA1; // Activates LSB-D1
                    break;
                case DSPMode.DIGU:
                    civMode = CIVMode.USB;
                    dataMode = CIVDataMode.DATA1; // Activates USB-D1
                    break;
                default:
                    civMode = CIVMode.USB;
                    break;
            }
        }

        /// <summary>
        /// Calculates the IC-7100 IF filter width code (BCD) for SSB passband.
        /// 50 Hz to 500 Hz (50 Hz steps, codes 00 to 09).
        /// 600 Hz to 3600 Hz (100 Hz steps, codes 10 to 40).
        /// </summary>
        public static byte CalculateSsbIfFilterWidthCode(int filterWidthHz)
        {
            if (filterWidthHz < 50) filterWidthHz = 50;
            if (filterWidthHz > 3600) filterWidthHz = 3600;

            int code;
            if (filterWidthHz <= 500)
            {
                code = (filterWidthHz - 50) / 50;
            }
            else
            {
                code = 10 + (filterWidthHz - 600) / 100;
            }

            if (code > 40) code = 40;
            if (code < 0) code = 0;

            // Convert to BCD byte
            int tens = code / 10;
            int units = code % 10;
            return (byte)((tens << 4) | (units & 0x0F));
        }

        /// <summary>
        /// Maps an Icom filter preset (FIL1, FIL2, FIL3) to a Thetis Filter preset enum for the given mode.
        /// </summary>
        public static Filter MapCIVFilterToThetisFilter(DSPMode mode, CIVFilter civFilter)
        {
            switch (mode)
            {
                case DSPMode.USB:
                case DSPMode.LSB:
                    switch (civFilter)
                    {
                        case CIVFilter.FIL1: return Filter.F6; // 2.7k
                        case CIVFilter.FIL2: return Filter.F7; // 2.4k
                        case CIVFilter.FIL3: return Filter.F9; // 1.8k
                        default: return Filter.F7;
                    }
                case DSPMode.DIGU:
                case DSPMode.DIGL:
                    switch (civFilter)
                    {
                        case CIVFilter.FIL1: return Filter.F1; // 3.0k
                        case CIVFilter.FIL2: return Filter.F2; // 2.5k
                        case CIVFilter.FIL3: return Filter.F5; // 1.0k
                        default: return Filter.F2;
                    }
                case DSPMode.CWL:
                case DSPMode.CWU:
                    switch (civFilter)
                    {
                        case CIVFilter.FIL1: return Filter.F1; // 1.0k
                        case CIVFilter.FIL2: return Filter.F4; // 500 Hz
                        case CIVFilter.FIL3: return Filter.F6; // 250 Hz
                        default: return Filter.F4;
                    }
                case DSPMode.AM:
                case DSPMode.SAM:
                    switch (civFilter)
                    {
                        case CIVFilter.FIL1: return Filter.F1; // 6.0k
                        case CIVFilter.FIL2: return Filter.F2; // 4.0k
                        case CIVFilter.FIL3: return Filter.F3; // 3.0k
                        default: return Filter.F2;
                    }
                case DSPMode.FM:
                    switch (civFilter)
                    {
                        case CIVFilter.FIL1: return Filter.F1;
                        case CIVFilter.FIL2: return Filter.F2;
                        case CIVFilter.FIL3: return Filter.F3;
                        default: return Filter.F2;
                    }
                default:
                    switch (civFilter)
                    {
                        case CIVFilter.FIL1: return Filter.F6;
                        case CIVFilter.FIL2: return Filter.F7;
                        case CIVFilter.FIL3: return Filter.F9;
                        default: return Filter.F7;
                    }
            }
        }

        #endregion
    }
}
