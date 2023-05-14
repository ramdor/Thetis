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
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Thetis
{
    class TitleBar
    {

        public static DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }
        public static DateTime AppBuildDate()
        {
            return GetLinkerTime(Assembly.GetExecutingAssembly());
        }
        public const string BUILD_NAME = "G7KLJ Edition";
        public const string BUILD_DATE = "<FW>"; //MW0LGE_21g <FW> gets replaced in BasicTitle (console.cs) with firmware version

        public static string GetString()
        {
            string sRevision = "." + Common.GetRevision(); //MW0LGE_22b
            if (sRevision == ".0") sRevision = "";

            string version = Common.GetVerNum() + sRevision;
            string s = "Thetis";

            string sBits = Common.Is64Bit ? " x64" : " x86";
            var sDate = AppBuildDate().Date.ToString("dd/MM/yyyy", new CultureInfo("en-GB"));
            s += " v" + version + sBits;
            if (BUILD_DATE != "") s += " (" + sDate + ")" + BUILD_DATE;
            if (BUILD_NAME != "") s += " " + BUILD_NAME;

            return s;
        }
    }
}