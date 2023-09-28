//=================================================================
// titlebar.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2012  FlexRadio Systems 
// Copyright (C) 2010-2020  Doug Wigley
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
// You may contact us via email at: gpl@flexradio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
//=================================================================

using System;
using System.Diagnostics;
using System.Reflection;

namespace Thetis
{
    class TitleBar
    {
        public const string BUILD_NAME = "wip_lge_7";
        public const string BUILD_DATE = "(8/23/23)<FW>"; //MW0LGE_21g <FW> gets replaced in BasicTitle (console.cs) with firmware version

        public static string GetString()
        {
            string sRevision = "." + Common.GetRevision(); //MW0LGE_22b
            if (sRevision == ".0") sRevision = "";

            string version = Common.GetVerNum() + sRevision;
            string s = "Thetis";

            string sBits = Common.Is64Bit ? " x64" : " x86";

            s += " v" + version + sBits;
            if (BUILD_DATE != "") s += " " + BUILD_DATE;
            if (BUILD_NAME != "") s += " " + BUILD_NAME;

            return s;
        }
    }
}