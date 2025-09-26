/*  SportManager2.cs

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Timers;
using System.Globalization;
using System.Web.UI;

namespace Thetis
{
    internal static class SpotManager2
    {
        const int MAX_RX = 2;

        private static List<smSpot> _spots = new List<smSpot>();
        private static Object _objLock = new Object();
        private static int _lifeTime = 60;
        private static int _maxNumber = 100;
        private static Timer _tickTimer;
        public class smSpot
        {
            public string callsign;
            public DSPMode mode;
            public long frequencyHZ;
            public Color colour;
            public DateTime timeAdded;
            public string additionalText;
            public string spotter;
            public int heading;
            public string continent;
            public string country;
            public DateTime utc_spot_time;
            public bool IsSWL;
            public long SwlSecondsToLive;

            public bool previously_highlighted;
            public bool flashing;
            public DateTime flash_start_time;

            public bool[] Visible;
            public SizeF Size;
            public Rectangle[] BoundingBoxInPixels;
            public bool[] Highlight;

            public smSpot()
            {
                DateTime now = DateTime.UtcNow;
                callsign = "";
                mode = DSPMode.FIRST;
                frequencyHZ = 0;
                colour = Color.White;
                timeAdded = now;
                additionalText = "";
                spotter = "";
                heading = -1;
                continent = "";
                country = "";
                utc_spot_time = now;
                IsSWL = false;
                SwlSecondsToLive = 0;
                previously_highlighted = false;
                flashing = false;
                flash_start_time = now;
            }
            public void BrowseQRZ()
            {
                Common.OpenUri("https://www.qrz.com/db/" + callsign.ToUpper().Trim());
            }
            public void BrowseHamQTH()
            {
                Common.OpenUri("https://www.hamqth.com/" + callsign.ToUpper().Trim());
            }
        }

        static SpotManager2()
        {
            _tickTimer = new Timer(1000);
            _tickTimer.Elapsed += OnTick;
            _tickTimer.AutoReset = true;
            _tickTimer.Enabled = true;
        }

        public static int LifeTime
        {
            get { return _lifeTime; }
            set { _lifeTime = value; }
        }
        public static int MaxNumber
        {
            get { return _maxNumber; }
            set { _maxNumber = value; }
        }
        private static void OnTick(Object source, ElapsedEventArgs e)
        {
            lock (_objLock)
            {
                //List<smSpot> oldSpots = _spots.Where(o => (DateTime.Now - o.timeAdded).TotalMinutes > _lifeTime && !o.IsSWL).ToList();
                //foreach(smSpot spot in oldSpots)
                //{
                //    _spots.Remove(spot);
                //}

                //remove any old spots, that are not swl
                _spots.RemoveAll(o => !o.IsSWL && (DateTime.UtcNow - o.timeAdded).TotalMinutes > _lifeTime);

                //remove timeout swl, leave ones with 0
                _spots.RemoveAll(o => o.IsSWL && o.SwlSecondsToLive != 0 && DateTime.UtcNow > (o.timeAdded + TimeSpan.FromSeconds(o.SwlSecondsToLive)));
            }
        }

        public static smSpot HighlightSpot(int x, int y)
        {
            lock (_objLock)
            {
                // clear all highlighted
                for (int rx = 0; rx < 2; rx++)
                {
                    List<smSpot> highlightedSpots = _spots.Where(o => o.Highlight[rx] == true).ToList();

                    foreach (smSpot higlighSpot in highlightedSpots)
                    {
                        higlighSpot.Highlight[rx] = false;
                    }
                }

                // highlight the one we want
                for (int rx = 0; rx < 2; rx++)
                {
                    smSpot spot = _spots.Find(o => o.Visible[rx] && o.BoundingBoxInPixels[rx].Contains(x, y));
                    if (spot != null)
                    {
                        spot.Highlight[rx] = true;
                        return spot;
                    }
                }
            }
            return null;
        }

        public static DSPMode SpotModeNumberToDSPMode(int number, double freq = -1)
        {
            DSPMode mode = DSPMode.FIRST;

            bool isFreqencyNormallyUSB = false;
            if(freq > -1) isFreqencyNormallyUSB = freq >= 10000000 || (freq >= 5300000 && freq < 5410000);

            switch (number)
            {
                case 0: //ssb
                    if (isFreqencyNormallyUSB) mode = DSPMode.USB;
                    else mode = DSPMode.LSB;
                    break;
                case 1: //cw
                    if (isFreqencyNormallyUSB) mode = DSPMode.CWU;
                    else mode = DSPMode.CWL;
                    break;
                case 2://rtty
                case 3://psk
                case 4://olivia
                case 5://jt65
                case 6://contesa
                case 7://fsk
                case 8://mt63
                case 9://domino
                case 10://pactor
                    if (isFreqencyNormallyUSB) mode = DSPMode.DIGU;
                    else mode = DSPMode.DIGL;
                    break;
                case 11://fm
                    mode = DSPMode.FM;
                    break;
                case 12://drm
                    mode = DSPMode.DRM;
                    break;
                case 13://sstv
                    if (isFreqencyNormallyUSB) mode = DSPMode.DIGU;
                    else mode = DSPMode.DIGL;
                    break;
                case 14://am
                    if (isFreqencyNormallyUSB) mode = DSPMode.AM_USB;
                    else mode = DSPMode.AM_LSB;
                    break;
            }

            return mode;
        }
        public static long getSwlTimeToLive(string raw_mode)
        {
            if (string.IsNullOrEmpty(raw_mode)) return -1;

            int skip = 5;
            int start = raw_mode.IndexOf("-swl[", StringComparison.OrdinalIgnoreCase);

            if (start == -1)
            {
                skip = 4;
                start = raw_mode.IndexOf("swl[", StringComparison.OrdinalIgnoreCase);
            }

            if (start != -1)
            {
                start += skip;

                int end = raw_mode.IndexOf("]", start, StringComparison.OrdinalIgnoreCase);
                if (end != -1)
                {
                    bool ok = long.TryParse(raw_mode.Substring(start, end - start), out long time_to_live);
                    if (ok)
                    {
                        if (time_to_live >= 0)
                        {
                            return time_to_live;
                        }
                    }
                }
            }
            return -1;
        }
        public static string FilterForRawMode(string raw_mode)
        {
            // remove any extra info from raw_mode
            if (string.IsNullOrEmpty(raw_mode)) return "";

            int pos = raw_mode.IndexOf("-swl[", StringComparison.OrdinalIgnoreCase);
            if(pos == -1)
            {
                pos = raw_mode.IndexOf("swl[", StringComparison.OrdinalIgnoreCase);
            }

            if (pos != -1)
            {
                return raw_mode.Substring(0, pos);
            }

            return raw_mode;
        }
        public static void AddSpot(string callsign, DSPMode mode, long frequencyHz, Color colour, string additionalText, string spotter = "", string raw_mode = "", int beam_heading = -1, string spot_continent = "", string spot_country = "", string date_time = "")
        {
            DateTime spotted_time = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(date_time))
            {
                const string format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"; // "2025-06-25T16:45:30Z"

                bool success = DateTime.TryParseExact(date_time, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime result);
                if (success) spotted_time = result;
            }

            //[2.10.3.7]MW0LGE added raw_mode which is the raw mode string that came in with the spot
            //can be used as a filter, and in the case below handles swl[N] or -swl[N] where N is seconds to live
            long time_to_live = getSwlTimeToLive(raw_mode);
            smSpot spot = new smSpot()
            {
                callsign = callsign.ToUpper().Trim(),
                mode = mode,
                frequencyHZ = frequencyHz,
                colour = colour,
                additionalText = additionalText.Trim(),
                spotter = spotter.Trim(),
                heading = beam_heading,
                continent = spot_continent,
                country = spot_country,
                timeAdded = DateTime.UtcNow,
                utc_spot_time = spotted_time,

                IsSWL = time_to_live != -1,
                SwlSecondsToLive = time_to_live,

                previously_highlighted = false,
                flashing = (DateTime.UtcNow - spotted_time).TotalSeconds <= 120
            };

            if (_replaceOwnCallAppearance && spot.callsign == _replaceCall)
                spot.colour = _replaceBackgroundColour;

            if (spot.callsign.Length > 20)
                spot.callsign = spot.callsign.Substring(0, 20);
            if (spot.spotter.Length > 20)
                spot.spotter = spot.spotter.Substring(0, 20);
            if (spot.additionalText.Length > 30)
                spot.additionalText = spot.additionalText.Substring(0, 30);
            if (spot.continent.Length > 30)
                spot.continent = spot.continent.Substring(0, 30);
            if (spot.country.Length > 30)
                spot.country = spot.country.Substring(0, 30);
            if (spot.heading < 0 || spot.heading > 360) spot.heading = -1; //should idealy be 359, but we will accept 360 as the same as 0

            spot.Highlight = new bool[MAX_RX];
            spot.BoundingBoxInPixels = new Rectangle[MAX_RX];
            spot.Visible = new bool[MAX_RX];

            for (int rx = 0; rx < MAX_RX; rx++)
            {
                spot.Highlight[rx] = false;
                spot.BoundingBoxInPixels[rx] = new Rectangle(-1, -1, 0, 0);
                spot.Visible[rx] = false;
            }

            lock (_objLock)
            {
                smSpot exists = _spots.Find(o => string.Equals(o.callsign?.Trim(), spot.callsign?.Trim(), StringComparison.OrdinalIgnoreCase) && Math.Abs(o.frequencyHZ - frequencyHz) <= 5000);
                if (exists != null)
                {
                    spot.flash_start_time = exists.flash_start_time;
                    spot.flashing = exists.flashing;

                    //if the data is the same, use the original spot time
                    if (spot.mode == exists.mode &&
                        Math.Abs(spot.frequencyHZ - exists.frequencyHZ) <= 5000 &&
                        spot.colour == exists.colour &&
                        spot.heading == exists.heading &&
                        string.Equals(spot.additionalText?.Trim(), exists.additionalText?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(spot.spotter?.Trim(), exists.spotter?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(spot.continent?.Trim(), exists.continent?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(spot.country?.Trim(), exists.country?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        spot.utc_spot_time = exists.utc_spot_time;
                    }

                    _spots.Remove(exists);
                }

                // Limit to max
                int count_swl = _spots.Count(o => o.IsSWL);
                int count_non_swl = _spots.Count(o => !o.IsSWL);

                if (count_non_swl >= _maxNumber)
                {
                    // Order only the non-IsSWL spots by age
                    List<smSpot> ageOrderedNonSWLSpots = _spots
                        .Where(o => !o.IsSWL)
                        .OrderBy(o => o.timeAdded)
                        .ToList();

                    // Determine how many spots need to be removed
                    int spotsToRemove = count_non_swl - _maxNumber;

                    for (int i = 0; i < spotsToRemove; i++)
                    {
                        smSpot removeSpot = ageOrderedNonSWLSpots[i];
                        _spots.Remove(removeSpot); // Remove from the original _spots list
                    }
                }

                _spots.Add(spot);
            }
        }

        public static List<smSpot> GetFrequencySortedSpots()
        {
            List<smSpot> lst;
            lock (_objLock)
            {
                lst = _spots.OrderBy(o => o.frequencyHZ).ToList();
            }
            return lst;
        }

        public static void ClearAllSpots(bool non_swl, bool swl)
        {
            lock (_objLock)
            {
                if (non_swl) _spots.RemoveAll(o => !o.IsSWL);
                if (swl) _spots.RemoveAll(o => o.IsSWL);
            }
        }

        public static void DeleteSpot(string callsign)
        {
            lock (_objLock)
            {
                string call = callsign.ToUpper().Trim();

                List<smSpot> spots = _spots.Where(o => o.callsign == call).ToList();
                foreach(smSpot spot in spots)
                    _spots.Remove(spot);
            }
        }

        private static bool _replaceOwnCallAppearance = false;
        private static string _replaceCall = "";
        private static Color _replaceBackgroundColour = Color.DarkGray;
        public static void OwnCallApearance(bool bEnabled, string sCall, Color replacementColorBackground)
        {
            _replaceOwnCallAppearance = bEnabled;
            _replaceCall = sCall.ToUpper().Trim();
            _replaceBackgroundColour = replacementColorBackground;
        }
    }
}
