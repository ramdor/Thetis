//=================================================================
// MW0LGE 2022
//=================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Timers;

namespace Thetis
{
    internal static class SpotManager2
    {
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

            public bool[] Visible;
            public SizeF Size;
            public Rectangle[] BoundingBoxInPixels;
            public bool[] Highlight;

            public void BrowseQRZ()
            {
                Common.OpenUri("https://www.qrz.com/db/" + callsign.ToUpper().Trim());
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
                List<smSpot> oldSpots = _spots.Where(o => (DateTime.Now - o.timeAdded).TotalMinutes > _lifeTime).ToList();
                foreach(smSpot spot in oldSpots)
                {
                    _spots.Remove(spot);
                }
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

        public static void AddSpot(string callsign, DSPMode mode, long frequencyHz, Color colour, string additionalText, string spotter = "")
        {
            smSpot spot = new smSpot()
            {
                callsign = callsign.ToUpper().Trim(),
                mode = mode,
                frequencyHZ = frequencyHz,
                colour = colour,
                additionalText = additionalText.Trim(),
                spotter = spotter.Trim(),
                timeAdded = DateTime.Now
            };

            if (_replaceOwnCallAppearance && spot.callsign == _replaceCall)
                spot.colour = _replaceBackgroundColour;

            if (spot.callsign.Length > 20)
                spot.callsign = spot.callsign.Substring(0, 20);
            if (spot.spotter.Length > 20)
                spot.spotter = spot.spotter.Substring(0, 20);
            if (spot.additionalText.Length > 30)
                spot.additionalText = spot.additionalText.Substring(0, 30);

            const int maxRX = 2;

            spot.Highlight = new bool[maxRX];
            spot.BoundingBoxInPixels = new Rectangle[maxRX];
            spot.Visible = new bool[maxRX];

            for (int rx = 0; rx < maxRX; rx++)
            {
                spot.Highlight[rx] = false;
                spot.BoundingBoxInPixels[rx] = new Rectangle(-1, -1, 0, 0);
                spot.Visible[rx] = false;
            }

            lock (_objLock)
            {
                smSpot exists = _spots.Find(o => (o.callsign == spot.callsign) && (Math.Abs(o.frequencyHZ - frequencyHz) <= 5000));
                if (exists != null)
                    _spots.Remove(exists);

                // limit to max
                if (_spots.Count >= _maxNumber)
                {
                    List<smSpot> ageOrderedSpots = _spots.OrderBy(o => o.timeAdded).ToList();
                    for(int i = 0; i < ageOrderedSpots.Count - _maxNumber; i++)
                    {
                        smSpot removeSpot = ageOrderedSpots[i];
                        _spots.Remove(removeSpot);
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

        public static void ClearAllSpots()
        {
            lock (_objLock)
            {
                _spots.Clear();
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
