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
using System.Text;
using System.Threading.Tasks;

namespace Thetis
{
    internal static class Hardware
    {
        private static HPSDRModel _model;
        static Hardware()
        {
            _model = HPSDRModel.HERMES;
        }
        public static HPSDRModel Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
            }
        }
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
                case HPSDRModel.REDPITAYA:
                    return "RED-PITAYA";
                default:
                    return "HERMES";
            }
        }
    }
}
