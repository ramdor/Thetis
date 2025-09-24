/*  clsHardwareSpecific.cs

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
// WORK IN PROGRESS
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Thetis
{
    internal static class HardwareSpecific
    {
        private static HPSDRModel _model;
        private static HPSDRModel _old_model;
        private static HPSDRHW _hardware;
        private static HPSDRHW _old_hardware;
        static HardwareSpecific()
        {
            _model = HPSDRModel.FIRST;
            _old_model = HPSDRModel.FIRST;

            _hardware = HPSDRHW.Unknown;
            _old_hardware = HPSDRHW.Unknown;
        }

        #region MODEL
        // model
        public static HPSDRModel Model
        {
            get
            {
                return _model;
            }
            set
            {
                _old_model = _model;
                _model = value;

                NetworkIO.fwVersionsChecked = false;

                switch (_model)
                {
                    case HPSDRModel.HERMES:
                        NetworkIO.SetRxADC(1);
                        NetworkIO.SetMKIIBPF(0);
                        cmaster.SetADCSupply(0, 33);
                        NetworkIO.LRAudioSwap(1);
                        HardwareSpecific.Hardware = HPSDRHW.Hermes;
                        break;
                    case HPSDRModel.HERMESLITE:
                        NetworkIO.SetRxADC(1);
                        NetworkIO.SetMKIIBPF(0);
                        cmaster.SetADCSupply(0, 33);
                        NetworkIO.LRAudioSwap(1);
                        HardwareSpecific.Hardware = HPSDRHW.HermesLite;
                        break;
                    case HPSDRModel.ANAN10:
                        NetworkIO.SetRxADC(1);
                        NetworkIO.SetMKIIBPF(0);
                        cmaster.SetADCSupply(0, 33);
                        NetworkIO.LRAudioSwap(1);
                        HardwareSpecific.Hardware = HPSDRHW.Hermes;
                        break;
                    case HPSDRModel.ANAN10E:
                        NetworkIO.SetRxADC(1);
                        NetworkIO.SetMKIIBPF(0);
                        cmaster.SetADCSupply(0, 33);
                        NetworkIO.LRAudioSwap(1);
                        HardwareSpecific.Hardware = HPSDRHW.HermesII;
                        break;
                    case HPSDRModel.ANAN100:
                        NetworkIO.SetRxADC(1);
                        NetworkIO.SetMKIIBPF(0);
                        cmaster.SetADCSupply(0, 33);
                        NetworkIO.LRAudioSwap(1);
                        HardwareSpecific.Hardware = HPSDRHW.Hermes;
                        break;
                    case HPSDRModel.ANAN100B:
                        NetworkIO.SetRxADC(1);
                        NetworkIO.SetMKIIBPF(0);
                        cmaster.SetADCSupply(0, 33);
                        NetworkIO.LRAudioSwap(1);
                        HardwareSpecific.Hardware = HPSDRHW.HermesII;
                        break;
                    case HPSDRModel.ANAN100D:
                        NetworkIO.SetRxADC(2);
                        NetworkIO.SetMKIIBPF(0);
                        cmaster.SetADCSupply(0, 33);
                        NetworkIO.LRAudioSwap(0);
                        HardwareSpecific.Hardware = HPSDRHW.Angelia;
                        break;
                    case HPSDRModel.ANAN200D:
                        NetworkIO.SetRxADC(2);
                        NetworkIO.SetMKIIBPF(0);
                        cmaster.SetADCSupply(0, 50);
                        NetworkIO.LRAudioSwap(0);
                        HardwareSpecific.Hardware = HPSDRHW.Orion;
                        break;
                    case HPSDRModel.ORIONMKII:
                        NetworkIO.SetRxADC(2);
                        NetworkIO.SetMKIIBPF(1);
                        cmaster.SetADCSupply(0, 50);
                        NetworkIO.LRAudioSwap(0);
                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;
                        break;
                    case HPSDRModel.ANAN7000D:
                        NetworkIO.SetRxADC(2);
                        NetworkIO.SetMKIIBPF(1);
                        cmaster.SetADCSupply(0, 50);
                        NetworkIO.LRAudioSwap(0);
                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;
                        break;
                    case HPSDRModel.ANAN8000D:
                        NetworkIO.SetRxADC(2);
                        NetworkIO.SetMKIIBPF(1);
                        cmaster.SetADCSupply(0, 50);
                        NetworkIO.LRAudioSwap(0);
                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;
                        break;
                    case HPSDRModel.ANAN_G2:
                        NetworkIO.SetRxADC(2);
                        NetworkIO.SetMKIIBPF(1);
                        cmaster.SetADCSupply(0, 50);
                        NetworkIO.LRAudioSwap(0);
                        HardwareSpecific.Hardware = HPSDRHW.Saturn;
                        break;
                    case HPSDRModel.ANAN_G2_1K:             // G8NJJ: likely to need further changes for PA
                        NetworkIO.SetRxADC(2);
                        NetworkIO.SetMKIIBPF(1);
                        cmaster.SetADCSupply(0, 50);
                        NetworkIO.LRAudioSwap(0);
                        HardwareSpecific.Hardware = HPSDRHW.Saturn;
                        break;
                    case HPSDRModel.ANVELINAPRO3:
                        NetworkIO.SetRxADC(2);
                        NetworkIO.SetMKIIBPF(1);
                        cmaster.SetADCSupply(0, 50);
                        NetworkIO.LRAudioSwap(0);
                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;
                        break;
                    case HPSDRModel.REDPITAYA: //DH1KLM
                        NetworkIO.SetRxADC(2);
                        NetworkIO.SetMKIIBPF(0); // DH1KLM: changed for compatibility reasons for OpenHPSDR compat. DIY PA/Filter boards
                        cmaster.SetADCSupply(0, 50);
                        NetworkIO.LRAudioSwap(0);
                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;
                        break;
                }
            }
        }
        public static HPSDRModel OldModel
        {
            // obtains the model prior to the one now set
            get
            {
                return _old_model;
            }
        }

        public static int ModelInt
        {
            get
            {
                return (int)_model;
            }
        }
        public static int OldModelInt
        {
            get
            {
                return (int)_old_model;
            }
        }
        #endregion

        #region HARDWARE
        // hardware
        public static HPSDRHW Hardware
        {
            get
            {
                return _hardware;
            }
            set
            {
                _old_hardware = _hardware;
                _hardware = value;
            }
        }
        public static HPSDRHW OldHardware
        {
            get
            {
                return _old_hardware;
            }
        }
        #endregion

        #region VOLTS_AMPS
        //volts/amps
        public static bool HasVolts
        {
            get
            {
                return _model == HPSDRModel.ANAN7000D ||
                       _model == HPSDRModel.ANAN8000D ||
                       _model == HPSDRModel.ANVELINAPRO3 ||
                       _model == HPSDRModel.ANAN_G2 ||
                       _model == HPSDRModel.ANAN_G2_1K ||
                       _model == HPSDRModel.REDPITAYA;
            }
        }
        public static bool HasAmps
        {
            get
            {
                return _model == HPSDRModel.ANAN7000D || 
                       _model == HPSDRModel.ANAN8000D ||
                       _model == HPSDRModel.ANVELINAPRO3 || 
                       _model == HPSDRModel.ANAN_G2 ||
                       _model == HPSDRModel.ANAN_G2_1K || 
                       _model == HPSDRModel.REDPITAYA ||
                       _model == HPSDRModel.HERMESLITE;
            }
        }
        public static (float, float) GetDefaultVoltCalibration()
        {
            float voff;
            float sens;

            switch (_model)
            {
                case HPSDRModel.ANAN7000D:
                case HPSDRModel.ANVELINAPRO3:
                case HPSDRModel.REDPITAYA:
                    voff = 340.0f;
                    sens = 88.0f;
                    break;
                case HPSDRModel.ANAN_G2:
                    voff = 0.001f;                                // current sensor voltage offset
                    sens = 66.23f;                                // current reading sensitivity //0.001 to prevent /0 in the calcs
                    break;
                case HPSDRModel.ANAN_G2_1K:                       // will need adjustment probably
                    voff = 0.001f;                                // current sensor voltage offset
                    sens = 66.23f;                                // current reading sensitivity //0.001 to prevent /0 in the calcs
                    break;
                default:
                    voff = 360.0f;
                    sens = 120.0f;
                    break;
            }
            return (voff, sens);
        }
        #endregion

        #region PURESIGNAL
        // pursignal
        public static double PSDefaultPeak
        {
            get
            {
                if (NetworkIO.CurrentRadioProtocol == RadioProtocol.USB)
                { //protocol 1
                    switch (_hardware)
                    {
                        case HPSDRHW.HermesLite:
                            return 0.233;
                        default:
                            return 0.4072;
                    }
                }
                else
                { //protocol 2
                    switch (_hardware)
                    {
                        case HPSDRHW.Saturn:
                            return 0.6121;
                        default:
                            return 0.2899;
                    }
                }
            }
        }
        #endregion

        #region STRING_CONVERSION
        // enums/string conversion
        public static HPSDRModel StringModelToEnum(string sModel)
        {
            switch (sModel.ToUpper())
            {
                case "HERMES":
                    return HPSDRModel.HERMES;
                case "ANAN-10":
                    return HPSDRModel.ANAN10;
                case "ANAN-10E":
                    return HPSDRModel.ANAN10E;
                case "ANAN-100":
                    return HPSDRModel.ANAN100;
                case "ANAN-100B":
                    return HPSDRModel.ANAN100B;
                case "ANAN-100D":
                    return HPSDRModel.ANAN100D;
                case "ANAN-200D":
                    return HPSDRModel.ANAN200D;
                case "ANAN-7000DLE":
                    return HPSDRModel.ANAN7000D;
                case "ANAN-8000DLE":
                    return HPSDRModel.ANAN8000D;
                case "ANAN-G2":
                    return HPSDRModel.ANAN_G2;
                case "ANAN-G2-1K":
                    return HPSDRModel.ANAN_G2_1K;
                case "ANVELINA-PRO3":
                    return HPSDRModel.ANVELINAPRO3;
                case "HERMES LITE":
                    return HPSDRModel.HERMESLITE;
                case "HERMES-LITE":
                    return HPSDRModel.HERMESLITE;
                case "RED-PITAYA":
                    return HPSDRModel.REDPITAYA;
                default:
                    return HPSDRModel.HERMES;
            }
        }
        public static string EnumModelToString(HPSDRModel model)
        {
            switch (model)
            {
                case HPSDRModel.HERMES:
                    return "HERMES";
                case HPSDRModel.ANAN10:
                    return "ANAN-10";
                case HPSDRModel.ANAN10E:
                    return "ANAN-10E";
                case HPSDRModel.ANAN100:
                    return "ANAN-100";
                case HPSDRModel.ANAN100B:
                    return "ANAN-100B";
                case HPSDRModel.ANAN100D:
                    return "ANAN-100D";
                case HPSDRModel.ANAN200D:
                    return "ANAN-200D";
                case HPSDRModel.ANAN7000D:
                    return "ANAN-7000DLE";
                case HPSDRModel.ANAN8000D:
                    return "ANAN-8000DLE";
                case HPSDRModel.ANAN_G2:
                    return "ANAN-G2";
                case HPSDRModel.ANAN_G2_1K:
                    return "ANAN-G2-1K";
                case HPSDRModel.ANVELINAPRO3:
                    return "ANVELINA-PRO3";
                case HPSDRModel.HERMESLITE:
                    return "HERMES-LITE";
                case HPSDRModel.REDPITAYA:
                    return "RED-PITAYA";
                default:
                    return "HERMES";
            }
        }
        public static string ModelString
        {
            get { return EnumModelToString(_model); }
        }            
        #endregion

        #region RX_CALIBRATION_OFFSET
        public static float RXMeterCalbrationOffsetDefaults(HPSDRModel model)
        {
            switch (model)
            {
                case HPSDRModel.ANAN7000D:
                case HPSDRModel.ANAN8000D:
                case HPSDRModel.ORIONMKII:
                case HPSDRModel.ANVELINAPRO3:
                case HPSDRModel.REDPITAYA: //DH1KLM
                    return 4.841644f;
                case HPSDRModel.ANAN_G2:
                case HPSDRModel.ANAN_G2_1K:
                    return -4.476f;
                default:
                    return 0.98f;
            }
        }
        public static float RXDisplayCalbrationOffsetDefauls(HPSDRModel model)
        {
            switch (model)
            {
                case HPSDRModel.ANAN7000D:
                case HPSDRModel.ANAN8000D:
                case HPSDRModel.ORIONMKII:
                case HPSDRModel.ANVELINAPRO3:
                case HPSDRModel.REDPITAYA: //DH1KLM
                    return 5.259f;
                case HPSDRModel.ANAN_G2:
                case HPSDRModel.ANAN_G2_1K:
                    return -4.4005f;
                default:
                    return -2.1f;
            }
        }
        public static float RXMeterCalbrationOffset
        {
            get
            {
                return RXMeterCalbrationOffsetDefaults(_model);
            }
        }
        public static float RXDisplayCalbrationOffset
        {
            get
            {
                return RXDisplayCalbrationOffsetDefauls(_model);
            }
        }
        #endregion

        #region AUDIO
        public static bool HasAudioAmplifier
        {
            get
            {
                return NetworkIO.CurrentRadioProtocol == RadioProtocol.ETH && //only protocol 2
                (_model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||
                _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2 ||
                _model == HPSDRModel.ANAN_G2_1K || _model == HPSDRModel.REDPITAYA);
            }
        }
        #endregion

        #region PA_GAINS
        public static float[] DefaultPAGainsForBands(HPSDRModel model)
        {
            float[] gains = new float[(int)Band.LAST];

            // max them out, these gains are PA attenuations, so 100 is no output power
            for (int i = 0; i < (int)Band.LAST; i++)
            {
                gains[i] = 100f;
            }

            switch (model) 
            {
                case HPSDRModel.FIRST:
                case HPSDRModel.HERMES:
                case HPSDRModel.HPSDR:
                case HPSDRModel.ORIONMKII:
                    gains[(int)Band.B160M] = 41.0f;
                    gains[(int)Band.B80M] = 41.2f;
                    gains[(int)Band.B60M] = 41.3f;
                    gains[(int)Band.B40M] = 41.3f;
                    gains[(int)Band.B30M] = 41.0f;
                    gains[(int)Band.B20M] = 40.5f;
                    gains[(int)Band.B17M] = 39.9f;
                    gains[(int)Band.B15M] = 38.8f;
                    gains[(int)Band.B12M] = 38.8f;
                    gains[(int)Band.B10M] = 38.8f;
                    gains[(int)Band.B6M] = 38.8f;

                    gains[(int)Band.VHF0] = 56.2f;
                    gains[(int)Band.VHF1] = 56.2f;
                    gains[(int)Band.VHF2] = 56.2f;
                    gains[(int)Band.VHF3] = 56.2f;
                    gains[(int)Band.VHF4] = 56.2f;
                    gains[(int)Band.VHF5] = 56.2f;
                    gains[(int)Band.VHF6] = 56.2f;
                    gains[(int)Band.VHF7] = 56.2f;
                    gains[(int)Band.VHF8] = 56.2f;
                    gains[(int)Band.VHF9] = 56.2f;
                    gains[(int)Band.VHF10] = 56.2f;
                    gains[(int)Band.VHF11] = 56.2f;
                    gains[(int)Band.VHF12] = 56.2f;
                    gains[(int)Band.VHF13] = 56.2f;

                    return gains;

                case HPSDRModel.ANAN10:
                case HPSDRModel.ANAN10E:
                    gains[(int)Band.B160M] = 41.0f;
                    gains[(int)Band.B80M] = 41.2f;
                    gains[(int)Band.B60M] = 41.3f;
                    gains[(int)Band.B40M] = 41.3f;
                    gains[(int)Band.B30M] = 41.0f;
                    gains[(int)Band.B20M] = 40.5f;
                    gains[(int)Band.B17M] = 39.9f;
                    gains[(int)Band.B15M] = 38.8f;
                    gains[(int)Band.B12M] = 38.8f;
                    gains[(int)Band.B10M] = 38.8f;
                    gains[(int)Band.B6M] = 38.8f;

                    gains[(int)Band.VHF0] = 56.2f;
                    gains[(int)Band.VHF1] = 56.2f;
                    gains[(int)Band.VHF2] = 56.2f;
                    gains[(int)Band.VHF3] = 56.2f;
                    gains[(int)Band.VHF4] = 56.2f;
                    gains[(int)Band.VHF5] = 56.2f;
                    gains[(int)Band.VHF6] = 56.2f;
                    gains[(int)Band.VHF7] = 56.2f;
                    gains[(int)Band.VHF8] = 56.2f;
                    gains[(int)Band.VHF9] = 56.2f;
                    gains[(int)Band.VHF10] = 56.2f;
                    gains[(int)Band.VHF11] = 56.2f;
                    gains[(int)Band.VHF12] = 56.2f;
                    gains[(int)Band.VHF13] = 56.2f;

                    return gains;

                case HPSDRModel.ANAN100:
                    gains[(int)Band.B160M] = 50.0f;
                    gains[(int)Band.B80M] = 50.5f;
                    gains[(int)Band.B60M] = 50.5f;
                    gains[(int)Band.B40M] = 50.0f;
                    gains[(int)Band.B30M] = 49.5f;
                    gains[(int)Band.B20M] = 48.5f;
                    gains[(int)Band.B17M] = 48.0f;
                    gains[(int)Band.B15M] = 47.5f;
                    gains[(int)Band.B12M] = 46.5f;
                    gains[(int)Band.B10M] = 42.0f;
                    gains[(int)Band.B6M] = 43.0f;

                    gains[(int)Band.VHF0] = 56.2f;
                    gains[(int)Band.VHF1] = 56.2f;
                    gains[(int)Band.VHF2] = 56.2f;
                    gains[(int)Band.VHF3] = 56.2f;
                    gains[(int)Band.VHF4] = 56.2f;
                    gains[(int)Band.VHF5] = 56.2f;
                    gains[(int)Band.VHF6] = 56.2f;
                    gains[(int)Band.VHF7] = 56.2f;
                    gains[(int)Band.VHF8] = 56.2f;
                    gains[(int)Band.VHF9] = 56.2f;
                    gains[(int)Band.VHF10] = 56.2f;
                    gains[(int)Band.VHF11] = 56.2f;
                    gains[(int)Band.VHF12] = 56.2f;
                    gains[(int)Band.VHF13] = 56.2f;

                    return gains;

                case HPSDRModel.ANAN100B:
                    gains[(int)Band.B160M] = 50.0f;
                    gains[(int)Band.B80M] = 50.5f;
                    gains[(int)Band.B60M] = 50.5f;
                    gains[(int)Band.B40M] = 50.0f;
                    gains[(int)Band.B30M] = 49.5f;
                    gains[(int)Band.B20M] = 48.5f;
                    gains[(int)Band.B17M] = 48.0f;
                    gains[(int)Band.B15M] = 47.5f;
                    gains[(int)Band.B12M] = 46.5f;
                    gains[(int)Band.B10M] = 42.0f;
                    gains[(int)Band.B6M] = 43.0f;

                    gains[(int)Band.VHF0] = 56.2f;
                    gains[(int)Band.VHF1] = 56.2f;
                    gains[(int)Band.VHF2] = 56.2f;
                    gains[(int)Band.VHF3] = 56.2f;
                    gains[(int)Band.VHF4] = 56.2f;
                    gains[(int)Band.VHF5] = 56.2f;
                    gains[(int)Band.VHF6] = 56.2f;
                    gains[(int)Band.VHF7] = 56.2f;
                    gains[(int)Band.VHF8] = 56.2f;
                    gains[(int)Band.VHF9] = 56.2f;
                    gains[(int)Band.VHF10] = 56.2f;
                    gains[(int)Band.VHF11] = 56.2f;
                    gains[(int)Band.VHF12] = 56.2f;
                    gains[(int)Band.VHF13] = 56.2f;

                    return gains;

                case HPSDRModel.ANAN100D:
                    gains[(int)Band.B160M] = 49.5f;
                    gains[(int)Band.B80M] = 50.5f;
                    gains[(int)Band.B60M] = 50.5f;
                    gains[(int)Band.B40M] = 50.0f;
                    gains[(int)Band.B30M] = 49.0f;
                    gains[(int)Band.B20M] = 48.0f;
                    gains[(int)Band.B17M] = 47.0f;
                    gains[(int)Band.B15M] = 46.5f;
                    gains[(int)Band.B12M] = 46.0f;
                    gains[(int)Band.B10M] = 43.5f;
                    gains[(int)Band.B6M] = 43.0f;

                    gains[(int)Band.VHF0] = 56.2f;
                    gains[(int)Band.VHF1] = 56.2f;
                    gains[(int)Band.VHF2] = 56.2f;
                    gains[(int)Band.VHF3] = 56.2f;
                    gains[(int)Band.VHF4] = 56.2f;
                    gains[(int)Band.VHF5] = 56.2f;
                    gains[(int)Band.VHF6] = 56.2f;
                    gains[(int)Band.VHF7] = 56.2f;
                    gains[(int)Band.VHF8] = 56.2f;
                    gains[(int)Band.VHF9] = 56.2f;
                    gains[(int)Band.VHF10] = 56.2f;
                    gains[(int)Band.VHF11] = 56.2f;
                    gains[(int)Band.VHF12] = 56.2f;
                    gains[(int)Band.VHF13] = 56.2f;

                    return gains;

                case HPSDRModel.ANAN200D:
                    gains[(int)Band.B160M] = 49.5f;
                    gains[(int)Band.B80M] = 50.5f;
                    gains[(int)Band.B60M] = 50.5f;
                    gains[(int)Band.B40M] = 50.0f;
                    gains[(int)Band.B30M] = 49.0f;
                    gains[(int)Band.B20M] = 48.0f;
                    gains[(int)Band.B17M] = 47.0f;
                    gains[(int)Band.B15M] = 46.5f;
                    gains[(int)Band.B12M] = 46.0f;
                    gains[(int)Band.B10M] = 43.5f;
                    gains[(int)Band.B6M] = 43.0f;

                    gains[(int)Band.VHF0] = 56.2f;
                    gains[(int)Band.VHF1] = 56.2f;
                    gains[(int)Band.VHF2] = 56.2f;
                    gains[(int)Band.VHF3] = 56.2f;
                    gains[(int)Band.VHF4] = 56.2f;
                    gains[(int)Band.VHF5] = 56.2f;
                    gains[(int)Band.VHF6] = 56.2f;
                    gains[(int)Band.VHF7] = 56.2f;
                    gains[(int)Band.VHF8] = 56.2f;
                    gains[(int)Band.VHF9] = 56.2f;
                    gains[(int)Band.VHF10] = 56.2f;
                    gains[(int)Band.VHF11] = 56.2f;
                    gains[(int)Band.VHF12] = 56.2f;
                    gains[(int)Band.VHF13] = 56.2f;

                    return gains;

                case HPSDRModel.ANAN8000D:
                    gains[(int)Band.B160M] = 50.0f;
                    gains[(int)Band.B80M] = 50.5f;
                    gains[(int)Band.B60M] = 50.5f;
                    gains[(int)Band.B40M] = 50.0f;
                    gains[(int)Band.B30M] = 49.5f;
                    gains[(int)Band.B20M] = 48.5f;
                    gains[(int)Band.B17M] = 48.0f;
                    gains[(int)Band.B15M] = 47.5f;
                    gains[(int)Band.B12M] = 46.5f;
                    gains[(int)Band.B10M] = 42.0f;
                    gains[(int)Band.B6M] = 43.0f;

                    gains[(int)Band.VHF0] = 56.2f;
                    gains[(int)Band.VHF1] = 56.2f;
                    gains[(int)Band.VHF2] = 56.2f;
                    gains[(int)Band.VHF3] = 56.2f;
                    gains[(int)Band.VHF4] = 56.2f;
                    gains[(int)Band.VHF5] = 56.2f;
                    gains[(int)Band.VHF6] = 56.2f;
                    gains[(int)Band.VHF7] = 56.2f;
                    gains[(int)Band.VHF8] = 56.2f;
                    gains[(int)Band.VHF9] = 56.2f;
                    gains[(int)Band.VHF10] = 56.2f;
                    gains[(int)Band.VHF11] = 56.2f;
                    gains[(int)Band.VHF12] = 56.2f;
                    gains[(int)Band.VHF13] = 56.2f;

                    return gains;

                case HPSDRModel.ANAN7000D:
                case HPSDRModel.ANAN_G2:
                case HPSDRModel.ANVELINAPRO3:
                case HPSDRModel.REDPITAYA:
                    gains[(int)Band.B160M] = 47.9f;
                    gains[(int)Band.B80M] = 50.5f;
                    gains[(int)Band.B60M] = 50.8f;
                    gains[(int)Band.B40M] = 50.8f;
                    gains[(int)Band.B30M] = 50.9f;
                    gains[(int)Band.B20M] = 50.9f;
                    gains[(int)Band.B17M] = 50.5f;
                    gains[(int)Band.B15M] = 47.0f;
                    gains[(int)Band.B12M] = 47.9f;
                    gains[(int)Band.B10M] = 46.5f;
                    gains[(int)Band.B6M] = 44.6f;

                    gains[(int)Band.VHF0] = 63.1f;
                    gains[(int)Band.VHF1] = 63.1f;
                    gains[(int)Band.VHF2] = 63.1f;
                    gains[(int)Band.VHF3] = 63.1f;
                    gains[(int)Band.VHF4] = 63.1f;
                    gains[(int)Band.VHF5] = 63.1f;
                    gains[(int)Band.VHF6] = 63.1f;
                    gains[(int)Band.VHF7] = 63.1f;
                    gains[(int)Band.VHF8] = 63.1f;
                    gains[(int)Band.VHF9] = 63.1f;
                    gains[(int)Band.VHF10] = 63.1f;
                    gains[(int)Band.VHF11] = 63.1f;
                    gains[(int)Band.VHF12] = 63.1f;
                    gains[(int)Band.VHF13] = 63.1f;

                    return gains;

                case HPSDRModel.ANAN_G2_1K:
                    gains[(int)Band.B160M] = 47.9f;
                    gains[(int)Band.B80M] = 50.5f;
                    gains[(int)Band.B60M] = 50.8f;
                    gains[(int)Band.B40M] = 50.8f;
                    gains[(int)Band.B30M] = 50.9f;
                    gains[(int)Band.B20M] = 50.9f;
                    gains[(int)Band.B17M] = 50.5f;
                    gains[(int)Band.B15M] = 47.0f;
                    gains[(int)Band.B12M] = 47.9f;
                    gains[(int)Band.B10M] = 46.5f;
                    gains[(int)Band.B6M] = 44.6f;

                    gains[(int)Band.VHF0] = 63.1f;
                    gains[(int)Band.VHF1] = 63.1f;
                    gains[(int)Band.VHF2] = 63.1f;
                    gains[(int)Band.VHF3] = 63.1f;
                    gains[(int)Band.VHF4] = 63.1f;
                    gains[(int)Band.VHF5] = 63.1f;
                    gains[(int)Band.VHF6] = 63.1f;
                    gains[(int)Band.VHF7] = 63.1f;
                    gains[(int)Band.VHF8] = 63.1f;
                    gains[(int)Band.VHF9] = 63.1f;
                    gains[(int)Band.VHF10] = 63.1f;
                    gains[(int)Band.VHF11] = 63.1f;
                    gains[(int)Band.VHF12] = 63.1f;
                    gains[(int)Band.VHF13] = 63.1f;

                    return gains;

                case HPSDRModel.HERMESLITE:
                    gains[(int)Band.B160M] = 100f;
                    gains[(int)Band.B80M] = 100f;
                    gains[(int)Band.B60M] = 100f;
                    gains[(int)Band.B40M] = 100f;
                    gains[(int)Band.B30M] = 100f;
                    gains[(int)Band.B20M] = 100f;
                    gains[(int)Band.B17M] = 100f;
                    gains[(int)Band.B15M] = 100f;
                    gains[(int)Band.B12M] = 100f;
                    gains[(int)Band.B10M] = 100f;
                    gains[(int)Band.B6M] = 38.8f;

                    gains[(int)Band.VHF0] = 38.8f;
                    gains[(int)Band.VHF1] = 38.8f;
                    gains[(int)Band.VHF2] = 38.8f;
                    gains[(int)Band.VHF3] = 38.8f;
                    gains[(int)Band.VHF4] = 38.8f;
                    gains[(int)Band.VHF5] = 38.8f;
                    gains[(int)Band.VHF6] = 38.8f;
                    gains[(int)Band.VHF7] = 38.8f;
                    gains[(int)Band.VHF8] = 38.8f;
                    gains[(int)Band.VHF9] = 38.8f;
                    gains[(int)Band.VHF10] = 38.8f;
                    gains[(int)Band.VHF11] = 38.8f;
                    gains[(int)Band.VHF12] = 38.8f;
                    gains[(int)Band.VHF13] = 38.8f;

                    return gains;

                default:
                    return gains;
            }
        }
        public static float[] DefaultPAGainsForBands()
        {
            return DefaultPAGainsForBands(_model);
        }
        #endregion

        #region UI
        public static bool SupportsPathIllustrator
        {
            get
            {
                return !(_model == HPSDRModel.ORIONMKII || _model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||
                    _model == HPSDRModel.ANAN_G2 || _model == HPSDRModel.ANAN_G2_1K || _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.REDPITAYA);
            }
        }
        #endregion
    }
}
