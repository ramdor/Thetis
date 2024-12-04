/*  clsHardwareSpecific.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2024 Richard Samphire MW0LGE

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
    internal static class HardWare
    {
        private static HPSDRModel _model;
        static HardWare()
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
                       _model == HPSDRModel.ANAN_G2_1K;
            }
        }
        public static bool HasAmps
        {
            get
            {
                return _model == HPSDRModel.ANAN7000D || _model == HPSDRModel.ANAN8000D ||
                       _model == HPSDRModel.ANVELINAPRO3 || _model == HPSDRModel.ANAN_G2 ||
                       _model == HPSDRModel.ANAN_G2_1K;
            }
        }
    }
}
