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
            _model = HPSDRModel.HERMES;
            _old_model = _model;

            _hardware = HPSDRHW.Atlas;
            _old_hardware = _hardware;
        }

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
                        NetworkIO.SetMKIIBPF(1);
                        cmaster.SetADCSupply(0, 50);
                        NetworkIO.LRAudioSwap(0);
                        HardwareSpecific.Hardware = HPSDRHW.OrionMKII;
                        break;
                }
            }
        }
        public static HPSDRModel OldModel
        {
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

        //volts/amps
        public static bool HasVolts
        {
            get
            {
                return _model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||
                       _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2 ||
                       _model == HPSDRModel.ANAN_G2_1K || _model == HPSDRModel.REDPITAYA;
            }
        }
        public static bool HasAmps
        {
            get
            {
                return _model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||
                       _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2 ||
                       _model == HPSDRModel.ANAN_G2_1K || _model == HPSDRModel.REDPITAYA;
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
                case HPSDRModel.ANAN_G2_1K:                       // will need adjustment probably
                    voff = 66.23f;                                // current reading sensitivity
                    sens = 0.0f;                                  // current sensor voltage offset
                    break;
                default:
                    voff = 360.0f;
                    sens = 120.0f;
                    break;
            }
            return (voff, sens);
        }

        // pursignal
        public static double PSDefaultPeak
        {
            get
            {
                if (NetworkIO.CurrentRadioProtocol == RadioProtocol.USB)
                { //protocol 1
                    switch (_hardware)
                    {
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
                case "HERMESLITE":
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
    }
}
