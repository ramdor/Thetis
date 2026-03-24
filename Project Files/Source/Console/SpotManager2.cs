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
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Timers;
using System.Globalization;
using System.Web.UI;
using System.Collections.Concurrent;

namespace Thetis
{
    internal static class SpotManager2
    {
        const int MAX_RX = 2;

        private static readonly List<smSpot> _spots = new List<smSpot>();
        private static readonly Object _objLock = new Object();
        private static smSpot[] _sortedSpotsCache = Array.Empty<smSpot>();
        private static bool _sortedSpotsDirty = true;
        private static readonly smSpot[] _highlightedSpots = new smSpot[MAX_RX];
        private static int _lifeTime = 60;
        private static int _maxNumber = 100;
        private static Timer _tickTimer;

        private static ConcurrentDictionary<string, Image> _flag_images;

        public class JsonSpotData
        {
            public string Spotter { get; set; } = "";
            public string Comment { get; set; } = "";
            public int Heading { get; set; } = -1; // -1 = no heading
            public string Continent { get; set; } = "";
            public string Country { get; set; } = "";
            public string UtcTime { get; set; } = "";
            public string TextColor { get; set; } = ""; // text colour A92GE
            public string Flag { get; set; } = "";
            public string FlagSpotter { get; set; } = "";

            public bool IsSWL { get; set; } = false;
            public long SWLSecondsToLive { get; set; } = 0; // 0 = no expiry
        }

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
            public long SWLSecondsToLive;

            public bool previously_highlighted;
            public bool flashing;
            public DateTime flash_start_time;

            public bool[] Visible;
            public SizeF Size;
            public Rectangle[] BoundingBoxInPixels;
            public bool[] Highlight;

            public Color text_colour;
            public bool use_text_colour;
            public string cached_display_text;
            public int colour_luminance;

            public bool spot_flag_in_use;
            public bool spotter_flag_in_use;

            public Image flag;
            public Image flag_spotter;

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
                flag = null;
                flag_spotter = null;
                utc_spot_time = now;
                IsSWL = false;
                SWLSecondsToLive = 0;
                previously_highlighted = false;
                flashing = false;
                flash_start_time = now;
                text_colour = Color.Empty;
                use_text_colour = false;
                cached_display_text = null;
                colour_luminance = Common.GetLuminance(colour);
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
            // init flags
            _flag_images = new ConcurrentDictionary<string, Image>();
            FlagAtlas.Init(Properties.Resources.flagatlas_image, Properties.Resources.flagatlas_json);

            _tickTimer = new Timer(1000);
            _tickTimer.Elapsed += onTick;
            _tickTimer.AutoReset = true;
            _tickTimer.Enabled = true;
        }

        public static void FreeUpFlags()
        {
            if (_flag_images != null)
            {
                while (_flag_images.Count > 0)
                {
                    _flag_images.TryRemove(_flag_images.First().Key, out Image i);
                    if (i != null) i.Dispose();
                }
            }
        }

        private static int compareByFrequency(smSpot left, smSpot right)
        {
            return left.frequencyHZ.CompareTo(right.frequencyHZ);
        }

        private static void markSortedSpotsDirty()
        {
            _sortedSpotsDirty = true;
        }

        private static void RebuildSortedSpotsCache()
        {
            if (!_sortedSpotsDirty) return;

            if (_spots.Count == 0)
            {
                _sortedSpotsCache = Array.Empty<smSpot>();
                _sortedSpotsDirty = false;
                return;
            }

            smSpot[] snapshot = _spots.ToArray();
            Array.Sort(snapshot, compareByFrequency);
            _sortedSpotsCache = snapshot;
            _sortedSpotsDirty = false;
        }

        private static void clearHighlightedReference(smSpot spot)
        {
            if (spot == null) return;

            for (int rx = 0; rx < MAX_RX; rx++)
            {
                if (!ReferenceEquals(_highlightedSpots[rx], spot)) continue;

                spot.Highlight[rx] = false;
                _highlightedSpots[rx] = null;
            }
        }

        private static void pruneHighlightedReferences()
        {
            for (int rx = 0; rx < MAX_RX; rx++)
            {
                smSpot highlightedSpot = _highlightedSpots[rx];
                if (highlightedSpot == null || _spots.Contains(highlightedSpot)) continue;

                highlightedSpot.Highlight[rx] = false;
                _highlightedSpots[rx] = null;
            }
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
        private static void onTick(Object source, ElapsedEventArgs e)
        {
            lock (_objLock)
            {
                DateTime utcNow = DateTime.UtcNow;

                int removed = _spots.RemoveAll(o => !o.IsSWL && (utcNow - o.timeAdded).TotalMinutes > _lifeTime);
                removed += _spots.RemoveAll(o => o.IsSWL && o.SWLSecondsToLive != 0 && utcNow > (o.timeAdded + TimeSpan.FromSeconds(o.SWLSecondsToLive)));

                if (removed > 0)
                {
                    markSortedSpotsDirty();
                    pruneHighlightedReferences();
                }
            }
        }

        public static smSpot HighlightSpot(int x, int y)
        {
            lock (_objLock)
            {
                smSpot hitSpot = null;
                int hitRx = -1;

                for (int rx = 0; rx < MAX_RX && hitSpot == null; rx++)
                {
                    for (int i = 0; i < _spots.Count; i++)
                {
                        smSpot spot = _spots[i];
                        if (!spot.Visible[rx] || !spot.BoundingBoxInPixels[rx].Contains(x, y)) continue;

                        hitSpot = spot;
                        hitRx = rx;
                        break;
                    }
                }

                for (int rx = 0; rx < MAX_RX; rx++)
                {
                    smSpot highlightedSpot = _highlightedSpots[rx];
                    if (highlightedSpot == null) continue;
                    if (ReferenceEquals(highlightedSpot, hitSpot) && rx == hitRx) continue;

                    highlightedSpot.Highlight[rx] = false;
                    _highlightedSpots[rx] = null;
                }

                if (hitSpot != null)
                    {
                    hitSpot.Highlight[hitRx] = true;
                    _highlightedSpots[hitRx] = hitSpot;
                }

                return hitSpot;
            }
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
        public static string FilterForRawMode(string raw_mode)
        {
            // remove any extra info from raw_mode
            // this is left in here so not to break other spot sources that may still include -swl
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
        private static Color getSpotTextColour(string text_colour)
        {
            //convert spot colour if present to system.drawing.color
            //#RRGGBB or #AARRGGBB or argb int

            Color spotTextColour;

            if (string.IsNullOrEmpty(text_colour))
            {
                spotTextColour = Color.Empty;
            }
            else
            {
                string s = text_colour.Trim();

                // try a TCI colour format argb int
                if (int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int int_value))
                {
                    spotTextColour = Color.FromArgb(int_value);
                }
                else
                { // otherwise try #RRGGBB #AARRGGBB
                    if (s.Length > 0 && s[0] == '#') s = s.Substring(1);

                    if (s.Length == 6)
                    {
                        if (int.TryParse(s, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out int hex_value))
                        {
                            int r = (hex_value >> 16) & 0xFF;
                            int g = (hex_value >> 8) & 0xFF;
                            int b = hex_value & 0xFF;
                            spotTextColour = Color.FromArgb(255, r, g, b);
                        }
                        else
                        {
                            spotTextColour = Color.Empty;
                        }
                    }
                    else if (s.Length == 8)
                    {
                        if (int.TryParse(s, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out int hex_value))
                        {
                            int a = (hex_value >> 24) & 0xFF;
                            int r = (hex_value >> 16) & 0xFF;
                            int g = (hex_value >> 8) & 0xFF;
                            int b = hex_value & 0xFF;
                            spotTextColour = Color.FromArgb(a, r, g, b);
                        }
                        else
                        {
                            spotTextColour = Color.Empty;
                        }
                    }
                    else
                    {
                        spotTextColour = Color.Empty;
                    }
                }
            }
            return spotTextColour;
        }
        private static Image getFlagImage(string flag)
        {
            if (string.IsNullOrWhiteSpace(flag)) return null;

            flag = flag.Trim();

            if (flag.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                flag = flag.Substring(0, flag.Length - 4);

            if (string.IsNullOrWhiteSpace(flag)) return null;

            flag = flag.ToLowerInvariant();

            Image image;
            if (_flag_images.TryGetValue(flag, out image))
                return image;

            try
            {
                image = FlagAtlas.GetFlag(flag);
                _flag_images[flag] = image;
                return image;
            }
            catch
            {
                return null;
            }
        }

        private static Image getFlagImageFromCallsign(string callsign, out string country)
        {
            country = null;
            if (string.IsNullOrWhiteSpace(callsign)) return null;

            CountryData.PrefixData pd = CountryData.GetCallsignData(callsign);
            if (pd == null) return null;

            country = pd.Country;

            string country_ansi = pd.AssetCode;
            if (string.IsNullOrWhiteSpace(country_ansi)) return null;

            return getFlagImage(country_ansi);
        }

        public static void AddSpot(string callsign, DSPMode mode, long frequencyHz, Color colour, string additionalText, JsonSpotData jsonSpotData = null)
        {
            callsign = callsign.ToUpper().Trim();
            additionalText = additionalText.Trim();

            string date_time = "";
            string spotter = "";
            int beam_heading = -1;
            string spot_country = "";
            string spot_continent = "";
            Color text_colour = Color.Empty;
            bool use_text_colour = false;
            bool is_swl = false;
            long swl_seconds_to_live = 0;
            Image flag_image = null;
            Image flag_spotter_image = null;

            if (jsonSpotData != null)
            {
                date_time = jsonSpotData.UtcTime.Trim();
                spotter = jsonSpotData.Spotter.Trim();
                beam_heading = jsonSpotData.Heading;
                spot_country = jsonSpotData.Country.Trim();
                text_colour = getSpotTextColour(jsonSpotData.TextColor);
                use_text_colour = text_colour != Color.Empty;
                additionalText = jsonSpotData.Comment.Trim();

                flag_image = getFlagImage(jsonSpotData.Flag);
                flag_spotter_image = getFlagImage(jsonSpotData.FlagSpotter);

                is_swl = jsonSpotData.IsSWL;
                swl_seconds_to_live = jsonSpotData.SWLSecondsToLive;
                if (swl_seconds_to_live < 0) swl_seconds_to_live = 0;
            }

            string temp_country = null;
            if (!string.IsNullOrEmpty(callsign) && flag_image == null)
            {
                flag_image = getFlagImageFromCallsign(callsign, out temp_country);
            }
            if (string.IsNullOrEmpty(spot_country) && !string.IsNullOrEmpty(temp_country)) spot_country = temp_country;

            string spotter_tmp = string.IsNullOrEmpty(spotter) ? callsign : spotter;

            if (!string.IsNullOrEmpty(spotter_tmp) && flag_spotter_image == null)
                flag_spotter_image = getFlagImageFromCallsign(spotter_tmp, out _);

            DateTime spotted_time = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(date_time))
            {
                const string format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"; // "2025-06-25T16:45:30Z"

                bool success = DateTime.TryParseExact(date_time, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime result);
                if (success) spotted_time = result;
            }

            smSpot spot = new smSpot()
            {
                callsign = callsign,
                mode = mode,
                frequencyHZ = frequencyHz,
                colour = colour,
                additionalText = additionalText,
                spotter = spotter,
                heading = beam_heading,
                continent = spot_continent,
                country = spot_country,
                timeAdded = DateTime.UtcNow,
                utc_spot_time = spotted_time,

                flag = flag_image,
                flag_spotter = flag_spotter_image,

                IsSWL = is_swl,
                SWLSecondsToLive = swl_seconds_to_live,

                previously_highlighted = false,
                flashing = (DateTime.UtcNow - spotted_time).TotalSeconds <= 120,

                text_colour = text_colour,
                use_text_colour = use_text_colour
            };

            if (_replaceOwnCallAppearance && spot.callsign == _replaceCall)
                spot.colour = _replaceBackgroundColour;
            spot.colour_luminance = Common.GetLuminance(spot.colour);

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
            if (spot.heading < 0 || spot.heading > 360) spot.heading = -1;

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
                smSpot exists = null;
                for (int i = 0; i < _spots.Count; i++)
                {
                    smSpot candidate = _spots[i];
                    if (!string.Equals(candidate.callsign?.Trim(), spot.callsign?.Trim(), StringComparison.OrdinalIgnoreCase)) continue;
                    if (Math.Abs(candidate.frequencyHZ - frequencyHz) > 5000) continue;

                    exists = candidate;
                    break;
                }

                if (exists != null)
                {
                    spot.flash_start_time = exists.flash_start_time;
                    spot.flashing = exists.flashing;

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

                    clearHighlightedReference(exists);
                    _spots.Remove(exists);
                }

                int count_non_swl = 0;
                for (int i = 0; i < _spots.Count; i++)
                {
                    if (!_spots[i].IsSWL) count_non_swl++;
                }

                if (!spot.IsSWL && count_non_swl >= _maxNumber)
                {
                    int spotsToRemove = count_non_swl - _maxNumber + 1;

                    while (spotsToRemove > 0)
                    {
                        int oldestIndex = -1;
                        DateTime oldestTime = DateTime.MaxValue;

                        for (int i = 0; i < _spots.Count; i++)
                        {
                            smSpot candidate = _spots[i];
                            if (candidate.IsSWL || candidate.timeAdded >= oldestTime) continue;

                            oldestTime = candidate.timeAdded;
                            oldestIndex = i;
                        }

                        if (oldestIndex < 0) break;

                        smSpot removeSpot = _spots[oldestIndex];
                        clearHighlightedReference(removeSpot);
                        _spots.RemoveAt(oldestIndex);
                        spotsToRemove--;
                    }
                }

                _spots.Add(spot);
                markSortedSpotsDirty();
            }
        }
        

        public static bool HasSpots
        {
            get
            {
                lock (_objLock)
                {
                    return _spots.Count > 0;
                }
            }
        }

        public static smSpot[] GetFrequencySortedSpots()
        {
            lock (_objLock)
            {
                RebuildSortedSpotsCache();
                return _sortedSpotsCache;
            }
        }

        public static void ClearAllSpots(bool non_swl, bool swl)
        {
            lock (_objLock)
            {
                bool removed = false;

                for (int i = _spots.Count - 1; i >= 0; i--)
                {
                    smSpot spot = _spots[i];
                    bool removeSpot = (non_swl && !spot.IsSWL) || (swl && spot.IsSWL);
                    if (!removeSpot) continue;

                    clearHighlightedReference(spot);
                    _spots.RemoveAt(i);
                    removed = true;
                }

                if (removed) markSortedSpotsDirty();
            }
        }

        public static void DeleteSpot(string callsign)
        {
            lock (_objLock)
            {
                string call = callsign.ToUpper().Trim();
                bool removed = false;

                for (int i = _spots.Count - 1; i >= 0; i--)
                {
                    smSpot spot = _spots[i];
                    if (spot.callsign != call) continue;

                    clearHighlightedReference(spot);
                    _spots.RemoveAt(i);
                    removed = true;
                }

                if (removed) markSortedSpotsDirty();
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
