using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

//directX
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
//

namespace Thetis
{
    // this enum is similar to MeterRXMode & MeterTXMode
    public enum Reading
    {
        NONE = -1,
        //RX
        SIGNAL_STRENGTH = 0,
        AVG_SIGNAL_STRENGTH,
        ADC_REAL,
        ADC_IMAG,
        AGC_GAIN,
        //TX
        MIC,
        PWR,
        ALC,
        EQ,
        LEVELER,
        COMP,
        CPDR,
        ALC_G,
        ALC_GROUP,
        LVL_G,
        MIC_PK,
        ALC_PK,
        EQ_PK,
        LEVELER_PK,
        COMP_PK,
        CPDR_PK,
        CFC_PK,
        CFC_G,
        //additional to MeterRXMode & MeterTXMode
        REVERSE_PWR,
        SWR,
        ADC2_REAL,
        ADC2_IMAG,
        //pa
        DRIVE_FWD_ADC,
        FWD_ADC,
        REV_ADC,
        DRIVE_PWR,
        PA_FWD_PWR,
        PA_REV_PWR,
        CAL_FWD_PWR,
        REV_VOLT,
        FWD_VOLT,
        // volts/amps
        VOLTS,
        AMPS,
        LAST
    }

    internal static class MeterManager
    {
        #region MeterManager
        private enum MeterType
        {
            NONE = 0,
            RX = 1,
            MOX = 2,
            TUNE = 4,
            TWOTONE = 8
        }
        // member variables
        private static Console _console;
        private static bool _delegatesAdded;
        private static Dictionary<int, clsReadings> _readings;
        private static Dictionary<string, clsMeter> _meters;
        private static Thread _meterThread;
        private static bool _meterThreadRunning;
        private static object _readingsLock = new object();
        //private static bool _mox;
        private static bool _power;
        private static bool _rx1VHForAbove;
        private static bool _rx2VHForAbove;
        //
        private static DXRenderer _rx1Renderer;
        private static DXRenderer _rx2Renderer;
        //
        private static HPSDRModel _currentHPSDRmodel;
        private static bool _alexPresent;
        private static bool _paPresent;
        private static bool _apolloPresent;
        private static int _transverterIndex;

        static MeterManager()
        {
            // static constructor
            _rx1Renderer = null;
            _rx2Renderer = null;
            _rx1VHForAbove = false;
            _rx2VHForAbove = false;
            _delegatesAdded = false;
            _console = null;
            _readings = new Dictionary<int, clsReadings>();
            _meters = new Dictionary<string, clsMeter>();

            _currentHPSDRmodel = HPSDRModel.HERMES;
            _alexPresent = false;
            _paPresent = false;
            _apolloPresent = false;
            _transverterIndex = -1; // no transverter
            //_mox = false;

            _readings.Add(1, new clsReadings());
            _readings.Add(2, new clsReadings());

            // test
            clsMeter cm = new clsMeter(1, "test1meter", (int)MeterType.RX, 1f, 0.542f);
            _meters.Add(cm.Name, cm);

            cm = new clsMeter(2, "test2meter", (int)MeterType.RX, 1f, 0.542f);
            _meters.Add(cm.Name, cm);

            cm = new clsMeter(1, "test3meter", (int)MeterType.MOX | (int)MeterType.TUNE | (int)MeterType.TWOTONE, 1f, 1.361f);
            cm.AddDisplayGroup("PWR/SWR"); // 1
            cm.AddDisplayGroup("Comp");
            cm.AddDisplayGroup("ALC");
            cm.AddDisplayGroup("Volts/Amps");
            cm.DisplayGroup = 1;
            _meters.Add(cm.Name, cm);

            cm = new clsMeter(2, "test4meter", (int)MeterType.MOX | (int)MeterType.TUNE | (int)MeterType.TWOTONE, 1f, 1.361f);
            cm.AddDisplayGroup("PWR/SWR"); // 1
            cm.AddDisplayGroup("Comp");
            cm.AddDisplayGroup("ALC");
            cm.AddDisplayGroup("Volts/Amps");
            cm.DisplayGroup = 1;
            _meters.Add(cm.Name, cm);


            _meterThreadRunning = false;
        }

        private static void UpdateMeters()
        {
            _meterThreadRunning = true;
            while (_meterThreadRunning)
            {
                if (_power)
                {
                    // udpate any meter
                    foreach (KeyValuePair<string, clsMeter> kvp in _meters)
                    {
                        List<Reading> readingsUsed = new List<Reading>();

                        clsMeter m = kvp.Value;

                        m.Update(ref readingsUsed);

                        lock (_readingsLock)
                        {
                            // use/invalidate any readings that have been used, so that they can be updated
                            foreach (Reading rt in readingsUsed)
                            {
                                _readings[m.RX].UseReading(rt);
                            }
                        }
                    }
                    Thread.Sleep(overallUpdateInterval());
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
        public static void Init(Console c, PictureBox rx1, PictureBox rx2, string sImagePath = "")
        {
            _console = c;
            //_mox = _console.MOX;
            _power = _console.PowerOn;
            _rx1VHForAbove = _console.VFOAFreq >= 30;
            _rx2VHForAbove = _console.RX2Enabled && _console.VFOBFreq > 30;
            _currentHPSDRmodel = _console.CurrentHPSDRModel;
            _apolloPresent = _console.ApolloPresent;
            _alexPresent = _console.AlexPresent;
            _paPresent = _console.PAPresent;
            _transverterIndex = _console.TXXVTRIndex;

            addDelegates();

            if (rx1 != null)
            {
                _rx1Renderer = new DXRenderer(1, rx1, _console, sImagePath);
                rx1.Parent.Show();
            }

            if (rx2 != null)
            {
                _rx2Renderer = new DXRenderer(2, rx2, _console, sImagePath);
                if (c.RX2Enabled)
                    rx2.Parent.Show();
                else
                    rx2.Parent.Hide();
            }

            _meterThread = new Thread(new ThreadStart(UpdateMeters))
            {
                Name = "Multimeter Thread",
                Priority = ThreadPriority.Lowest,
                IsBackground = true
            };
            _meterThread.Start();
        }
        public static void Shutdown()
        {
            if (_rx1Renderer != null) _rx1Renderer.ShutdownDX();
            if (_rx2Renderer != null) _rx2Renderer.ShutdownDX();

            removeDelegates();
            
            _meterThreadRunning = false;
            if (_meterThread != null && _meterThread.IsAlive) _meterThread.Join();
        }
        private static void addDelegates()
        {
            if (_delegatesAdded) return;

            _console.MeterReadingsChangedHandlers += OnMeterReadings;
            _console.MoxChangeHandlers += OnMox;
            _console.PowerChangeHanders += OnPower;
            _console.VFOAFrequencyChangeHandlers += OnVFOA;
            _console.VFOBFrequencyChangeHandlers += OnVFOB;

            _console.AlexPresentChangedHandlers += OnAlexPresentChanged;
            _console.PAPresentChangedHandlers += OnPAPresentChanged;
            _console.ApolloPresentChangedHandlers += OnApolloPresentChanged;
            _console.CurrentModelChangedHandlers += OnCurrentModelChanged;
            _console.TransverterIndexChangedHandlers += OnTransverterIndexChanged;

            _delegatesAdded = true;
        }
        private static void removeDelegates()
        {
            if (!_delegatesAdded) return;

            _console.MeterReadingsChangedHandlers -= OnMeterReadings;
            _console.MoxChangeHandlers -= OnMox;
            _console.PowerChangeHanders -= OnPower;
            _console.VFOAFrequencyChangeHandlers -= OnVFOA;
            _console.VFOBFrequencyChangeHandlers -= OnVFOB;

            _console.AlexPresentChangedHandlers -= OnAlexPresentChanged;
            _console.PAPresentChangedHandlers -= OnPAPresentChanged;
            _console.ApolloPresentChangedHandlers -= OnApolloPresentChanged;
            _console.CurrentModelChangedHandlers -= OnCurrentModelChanged;
            _console.TransverterIndexChangedHandlers -= OnTransverterIndexChanged;

            _delegatesAdded = false;
        }
        private static void OnTransverterIndexChanged(int oldIndex, int newIndex)
        {
            _transverterIndex = newIndex;
        }
        public static void OnAlexPresentChanged(bool oldSetting, bool newSetting)
        {
            _alexPresent = newSetting;
        }
        public static void OnPAPresentChanged(bool oldSetting, bool newSetting)
        {
            _paPresent = newSetting;
        }
        public static void OnApolloPresentChanged(bool oldSetting, bool newSetting)
        {
            _apolloPresent = newSetting;
        }
        public static void OnCurrentModelChanged(HPSDRModel oldModel, HPSDRModel newModel)
        {
            _currentHPSDRmodel = newModel;
        }
        private static void OnVFOA(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
        {
            _rx1VHForAbove = _console.VFOAFreq >= 30;
        }
        private static void OnVFOB(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
        {
            _rx2VHForAbove = _console.RX2Enabled && _console.VFOBFreq >= 30;
        }
        private static void OnPower(bool oldPower, bool newPower)
        {
            _power = newPower;
        }
        private static void OnMox(int rx, bool oldMox, bool newMox)
        {
            foreach (KeyValuePair<string, clsMeter> mkvp in _meters)
            {
                clsMeter m = mkvp.Value;
                m.MOX = rx == m.RX && newMox;
            }
        }
        public static int CurrentPowerRating
        {
            get
            {
                int nWatts;
                if (_alexPresent && 
                    ((_currentHPSDRmodel == HPSDRModel.ORIONMKII || _currentHPSDRmodel == HPSDRModel.ANAN8000D) 
                    && _transverterIndex < 0))
                {
                    nWatts = 200;
                }
                else if ((_alexPresent || _paPresent) &&
                                (_currentHPSDRmodel != HPSDRModel.ANAN10 &&
                                 _currentHPSDRmodel != HPSDRModel.ANAN10E &&
                                !_apolloPresent))
                {
                    nWatts = 100;
                }
                else if (_currentHPSDRmodel == HPSDRModel.ANAN10 ||
                                _currentHPSDRmodel == HPSDRModel.ANAN10E)
                {
                    nWatts = 30;
                }
                else if (_apolloPresent)
                {
                    nWatts = 15;
                }
                else
                {
                    nWatts = 1;
                }

                return nWatts;
            }
        }
        private static float dbmOffsetForAbove30(int rx)
        {
            if ((rx == 1 && _rx1VHForAbove) || (rx == 2 && _rx2VHForAbove))
                return 20f;
            else
                return 0f;
        }
        private static float getReading(int rx, Reading rt, bool bUseReading = false)
        {
            lock (_readingsLock)
            {
                return _readings[rx].GetReading(rt, bUseReading);
            }
        }
        private static void setReading(int rx, Reading rt, ref Dictionary<Reading, float> readings)
        {
            // locked outside
            if (_readings[rx].RequiresUpdate(rt)) _readings[rx].SetReading(rt, readings[rt]);
        }
        private static void setReading(int rx, Reading rt, float reading)
        {
            lock (_readingsLock)
            {
                if (_readings[rx].RequiresUpdate(rt)) _readings[rx].SetReading(rt, reading);
            }
        }
        private static void OnMeterReadings(int rx, bool mox, ref Dictionary<Reading, float> readings)
        {
            lock (_readingsLock)
            {
                //
                if (!mox)
                {
                    setReading(rx, Reading.SIGNAL_STRENGTH, ref readings);
                    setReading(rx, Reading.AVG_SIGNAL_STRENGTH, ref readings);
                    setReading(rx, Reading.ADC_REAL, ref readings);
                    setReading(rx, Reading.ADC_IMAG, ref readings);
                }
                else
                {
                    setReading(rx, Reading.MIC, ref readings);
                    setReading(rx, Reading.MIC_PK, ref readings);
                    setReading(rx, Reading.EQ_PK, ref readings);
                    setReading(rx, Reading.EQ, ref readings);
                    setReading(rx, Reading.LEVELER_PK, ref readings);
                    setReading(rx, Reading.LEVELER, ref readings);
                    setReading(rx, Reading.LVL_G, ref readings);
                    setReading(rx, Reading.CFC_PK, ref readings);
                    setReading(rx, Reading.CFC_G, ref readings);
                    setReading(rx, Reading.CPDR_PK, ref readings);
                    setReading(rx, Reading.CPDR, ref readings);

                    setReading(rx, Reading.ALC_PK, ref readings);
                    setReading(rx, Reading.ALC, ref readings);
                    setReading(rx, Reading.ALC_G, ref readings);

                    setReading(rx, Reading.COMP, ref readings);
                    setReading(rx, Reading.COMP_PK, ref readings);
                    setReading(rx, Reading.ALC_GROUP, ref readings);

                    setReading(rx, Reading.PWR, ref readings);
                    setReading(rx, Reading.REVERSE_PWR, ref readings);
                    setReading(rx, Reading.SWR, ref readings);

                    setReading(rx, Reading.ADC2_REAL, ref readings);
                    setReading(rx, Reading.ADC2_IMAG, ref readings);

                    if (rx == 1)
                    { // only rx1 data
                        setReading(rx, Reading.DRIVE_FWD_ADC, ref readings);
                        setReading(rx, Reading.FWD_ADC, ref readings);
                        setReading(rx, Reading.REV_ADC, ref readings);
                        setReading(rx, Reading.DRIVE_PWR, ref readings);
                        setReading(rx, Reading.PA_FWD_PWR, ref readings);
                        setReading(rx, Reading.PA_REV_PWR, ref readings);
                        setReading(rx, Reading.CAL_FWD_PWR, ref readings);
                    }
                }
                setReading(rx, Reading.VOLTS, ref readings);
                setReading(rx, Reading.AMPS, ref readings);
            }
        }
        public static bool RequiresUpdate(int rx, Reading rt)
        {
            lock (_readingsLock)
            {
                return _readings[rx].RequiresUpdate(rt);
            }
        }

        // meters
        public static int QuickestUpdateInterval(int rx, bool mox)
        {
            int updateRate = 50;

            if (!mox)
            {
                foreach (KeyValuePair<string, clsMeter> kvp in _meters.Where(kvp => kvp.Value.RX == rx && (kvp.Value.TypeBitField & (int)MeterType.RX) == (int)MeterType.RX))
                {
                    int ui = kvp.Value.QuickestUpdateInterval;
                    if (ui < updateRate) updateRate = ui;
                }
            }
            else
            {
                foreach (KeyValuePair<string, clsMeter> kvp in _meters.Where(kvp => kvp.Value.RX == rx && (kvp.Value.TypeBitField & (int)MeterType.RX) != (int)MeterType.RX))
                {
                    int ui = kvp.Value.QuickestUpdateInterval;
                    if (ui < updateRate) updateRate = ui;
                }
            }
            return updateRate;
        }
        public static int QuickestUpdateInterval(int rx)
        {
            int updateRate = 50;

            foreach (KeyValuePair<string, clsMeter> kvp in _meters.Where(kvp => kvp.Value.RX == rx && kvp.Value.TypeBitField != (int)MeterType.NONE))
            {
                int ui = kvp.Value.QuickestUpdateInterval;
                if (ui < updateRate) updateRate = ui;
            }

            return updateRate;
        }
        private static int overallUpdateInterval()
        {
            int updateInterval = 5000;

            for (int rx = 1; rx <= 2; rx++)
            {
                int tmp = QuickestUpdateInterval(rx);
                if (tmp < updateInterval) updateInterval = tmp;
            }

            return updateInterval;
        }
        #endregion
        #region clsMeterItem
        private class clsMeterItem
        {
            public enum MeterItemType
            {
                BASE = 0, // done
                H_BAR, // done
                V_BAR,
                H_SCALE,
                V_SCALE,
                NEEDLE, // done
                TEXT, // done
                IMAGE, // done
                SOLID_COLOUR, // done
                CLICKBOX // done
            }

            private string _sGUID;

            private MeterItemType _ItemType;

            private PointF _topLeft; // 0-1,0-1
            private PointF _bottomRight; // 0-1,0-1
            private SizeF _size;  // 0-1,0-1
            private int _zOrder; // lower first
            private int _msUpdateInterval; //ms
            private float _attackRatio; // 0f - 1f
            private float _decayRatio; // 0f - 1f
            private float _value;
            private System.Drawing.Color _historyColour;
            private Reading _readingType;
            private Stopwatch _updateStopwatch;
            private Dictionary<float, PointF> _scaleCalibration;
            private int _displayGroup;

            public clsMeterItem()
            {
                // constructor
                _sGUID = Guid.NewGuid().ToString();
                _ItemType = MeterItemType.BASE;
                _readingType = Reading.NONE;
                _topLeft.X = 0;
                _topLeft.Y = 0;
                _zOrder = 0;
                _bottomRight.X = 1;
                _bottomRight.Y = 1;
                _msUpdateInterval = 5000; //ms
                _attackRatio = 0.8f;
                _decayRatio = 0.2f;
                _value = -200;
                _historyColour = System.Drawing.Color.FromArgb(128, 255, 255, 255);
                _scaleCalibration = new Dictionary<float, PointF>();
                _size.Width = 1f;
                _size.Height = 1f;
                _displayGroup = 0;
                _updateStopwatch = new Stopwatch();
            }

            public string ID { get { return _sGUID; } }
            public PointF TopLeft {
                get { return _topLeft; }
                set { _topLeft = value; }
            }
            public PointF BottomRight {
                get { return _bottomRight; }
                set { /*_bottomRight = value;*/ }
            }
            public SizeF Size
            {
                get { return _size; }
                set 
                { 
                    _size = value;
                    _bottomRight.X = _topLeft.X + _size.Width;
                    _bottomRight.Y = _topLeft.Y + _size.Height;
                }
            }
            public int DisplayGroup
            {
                get { return _displayGroup; }
                set { _displayGroup = value; }
            }
            public int UpdateInterval
            {
                get { return _msUpdateInterval; }
                set
                {
                    _msUpdateInterval = value;
                    if (_msUpdateInterval < 1) _msUpdateInterval = 1;
                }
            }
            public float AttackRatio
            {
                get { return _attackRatio; }
                set {
                    _attackRatio = value;
                    if (_attackRatio < 0) _attackRatio = 0;
                    if (_attackRatio > 1f) _attackRatio = 1f;
                }
            }
            public float DecayRatio
            {
                get { return _decayRatio; }
                set
                {
                    _decayRatio = value;
                    if (_decayRatio < 0) _decayRatio = 0;
                    if (_decayRatio > 1f) _decayRatio = 1f;
                }
            }
            public MeterItemType ItemType
            {
                get { return _ItemType; }
                set { _ItemType = value; }
            }
            public Reading ReadingSource
            {
                get { return _readingType; }
                set { _readingType = value; }
            }

            public bool RequiresUpdate
            {
                get
                {
                    if (!_updateStopwatch.IsRunning) _updateStopwatch.Restart();
                    bool bNeedsUpdate = _updateStopwatch.ElapsedMilliseconds >= _msUpdateInterval;
                    if(bNeedsUpdate) _updateStopwatch.Stop();

                    return bNeedsUpdate;
                }
            }
            public virtual void Update(int rx, ref List<Reading> readingsUsed)
            {
                // can be overriden by derived

                //// get latest reading
                //float reading = MeterManager.GetReading(rx, ReadingSource);

                //Value = reading;

                //// this reading has been used
                //if (!readingsUsed.Contains(ReadingSource))
                //    readingsUsed.Add(ReadingSource);
            }
            public float Value
            {
                get { return _value; }
                set { _value = value; }
            }
            public int ZOrder
            {
                get { return _zOrder; }
                set { _zOrder = value; }
            }
            public virtual void History(out float minHistory, out float maxHistory)
            {
                minHistory = 0f;
                maxHistory = 0f;
            }
            public virtual float MinHistory
            {
                get
                {
                    return 0f;
                }
            }
            public virtual float MaxHistory
            {
                get
                {
                    return 0f;
                }
            }
            public virtual bool ShowHistory
            {
                get { return false; }
                set { }
            }
            public virtual System.Drawing.Color HistoryColour
            {
                get { return _historyColour; }
                set { _historyColour = value; }
            }
            public virtual Dictionary<float, PointF> ScaleCalibration
            {
                get { return _scaleCalibration; }
                set { }
            }
        }
        //
        private class clsClickBox : clsMeterItem
        {
            private MouseButtons _button;
            private int _gotoGroup;
            public clsClickBox()
            {
                ItemType = MeterItemType.CLICKBOX;

                _button = MouseButtons.Left | MouseButtons.Right | MouseButtons.Middle;
                _gotoGroup = 0;
            }
            public MouseButtons Button
            {
                get { return _button; }
                set { _button = value; }
            }
            public int GotoGroup 
            {
                get { return _gotoGroup; }
                set { _gotoGroup = value; }
            }
        }
        private class clsSolidColour : clsMeterItem
        {
            private System.Drawing.Color _colour;
            public clsSolidColour()
            {
                ItemType = MeterItemType.SOLID_COLOUR;

                _colour = System.Drawing.Color.Black;
            }
            public System.Drawing.Color Colour
            {
                get { return _colour; }
                set
                { 
                    _colour = value;
                }
            }
        }
        private class clsImage : clsMeterItem
        {
            private string _name;
            private PointF _clipTopLeft;
            //private PointF _clipBottomRight;
            private SizeF _clipSize;
            private bool _clipped;
            public clsImage()
            {
                ItemType = MeterItemType.IMAGE;

                _name = "";
                _clipTopLeft = new PointF(0f, 0f);
                //_clipBottomRight = new PointF(1f, 1f);
                _clipSize = new SizeF(1f, 1f);
                _clipped = false;
            }
            public string ImageName
            {
                get { return _name; }
                set
                {
                    _name = value;
                }
            }
            public PointF ClipTopLeft
            {
                get { return _clipTopLeft; }
                set { _clipTopLeft = value; }
            }
            //public PointF ClipBottomRight
            //{
            //    get { return _clipBottomRight; }
            //    set { _clipBottomRight = value; }
            //}
            public SizeF ClipSize
            {
                get { return _clipSize; }
                set { _clipSize = value; }
            }
            public bool Clipped
            {
                get { return _clipped; }
                set { _clipped = value; }
            }
        }
        private class clsBarItem : clsMeterItem
        {
            public enum BarStyle
            {
                Line = 0,
                SolidFilled,
                GradientFilled,
                Segments
            }
            private List<float> _history;
            private int _msHistoryDuration; //ms
            private bool _showHistory;
            private object _historyLock = new object();
            private BarStyle _style;
            private System.Drawing.Color _colour;
            private float _strokeWidth;

            private Dictionary<float, PointF> _scaleCalibration;

            public clsBarItem()
            {
                _history = new List<float>();
                _msHistoryDuration = 2000;
                _showHistory = false;

                _style = BarStyle.Line;
                _colour = System.Drawing.Color.Red;
                _strokeWidth = 3f;

                _scaleCalibration = new Dictionary<float, PointF>();

                ItemType = MeterItemType.H_BAR;
                ReadingSource = Reading.NONE;
                UpdateInterval = 100;
                AttackRatio = 0.8f;
                DecayRatio = 0.2f;
            }
            public override void Update(int rx, ref List<Reading> readingsUsed)
            {
                // get latest reading
                float reading = MeterManager.getReading(rx, ReadingSource);

                lock (_historyLock)
                {
                    if (reading > Value)
                        Value = (reading * AttackRatio) + (Value * (1f - AttackRatio));
                    else
                        Value = (reading * DecayRatio) + (Value * (1f - DecayRatio));

                    // signal history
                    _history.Add(Value); // adds to end of the list
                    int numberToRemove = _history.Count - (_msHistoryDuration / UpdateInterval);
                    // the list is sized based on delay
                    if (numberToRemove > 0) _history.RemoveRange(0, numberToRemove); // remove the oldest, the head of the list
                }

                // this reading has been used
                if (!readingsUsed.Contains(ReadingSource))
                    readingsUsed.Add(ReadingSource);
            }

            public BarStyle Style
            {
                get { return _style; }
                set { _style = value; }
            }
            public override bool ShowHistory
            {
                get { return _showHistory; }
                set { _showHistory = value; }
            }
            public int HistoryDuration
            {
                get { return _msHistoryDuration; }
                set
                {
                    _msHistoryDuration = value;
                    if (_msHistoryDuration < UpdateInterval) _msHistoryDuration = UpdateInterval;
                }
            }
            public override float MinHistory
            {
                get
                {
                    lock (_historyLock)
                    {
                        if (_history.Count == 0) return Value;
                        return _history.Min();
                    }
                }
            }
            public override float MaxHistory
            {
                get
                {
                    lock (_historyLock)
                    {
                        if (_history.Count == 0) return Value;
                        return _history.Max();
                    }
                }
            }
            public override void History(out float minHistory, out float maxHistory)
            {
                if (_history.Count == 0)
                {
                    minHistory = Value;
                    maxHistory = Value;
                }
                else
                {
                    minHistory = _history.Min();
                    maxHistory = _history.Max();
                }
            }
            public System.Drawing.Color Colour
            {
                get { return _colour; }
                set { _colour = value; }
            }
            public float StrokeWidth
            {
                get { return _strokeWidth; }
                set { _strokeWidth = value; }
            }
            public override Dictionary<float, PointF> ScaleCalibration
            {
                get { return _scaleCalibration; }
                set { }
            }
        }
        private class clsNeedleItem : clsMeterItem
        {
            public enum NeedlePlacement
            {
                Bottom = 0,
                Top,
                Left,
                Right
            }
            public enum NeedleStyle
            {
                Line = 0,
                Arrow,
                Hollow
            }
            public enum NeedleDirection
            {
                Clockwise = 0,
                CounterClockwise
            }
            private List<float> _history;
            private int _msHistoryDuration; //ms
            private bool _showHistory;
            private object _historyLock = new object();
            private NeedleStyle _style;
            private PointF _needleOffset;
            private NeedlePlacement _placement;
            private System.Drawing.Color _colour;
            private float _strokeWidth;
            private NeedleDirection _needleDirection;
            private float _lengthFactor;
            private bool _setup;
            private Dictionary<float, PointF> _scaleCalibration;
            private PointF _radiusRatio;

            public clsNeedleItem()
            {
                _history = new List<float>();
                _msHistoryDuration = 500;
                _showHistory = false;
                _placement = NeedlePlacement.Bottom;
                _needleOffset.X = 0f;
                _needleOffset.Y = 0.5f;
                _lengthFactor = 1f;
                _setup = false;
                _radiusRatio.X = 1f;
                _radiusRatio.Y = 1f;
                _style = NeedleStyle.Line;
                _colour = System.Drawing.Color.White;
                _strokeWidth = 3f;
                _needleDirection = NeedleDirection.Clockwise;
                _scaleCalibration = new Dictionary<float, PointF>();

                ItemType = MeterItemType.NEEDLE;
                ReadingSource = Reading.NONE;
                UpdateInterval = 100;
                AttackRatio = 0.8f;
                DecayRatio = 0.2f;
            }
            public override void Update(int rx, ref List<Reading> readingsUsed)
            {
                // get latest reading
                float reading = MeterManager.getReading(rx, ReadingSource);

                lock (_historyLock)
                {
                    if (reading > Value)
                       Value = (reading * AttackRatio) + (Value * (1f - AttackRatio));
                    else
                        Value = (reading * DecayRatio) + (Value * (1f - DecayRatio));

                    // signal history
                    _history.Add(Value); // adds to end of the list
                    int numberToRemove = _history.Count - (_msHistoryDuration / UpdateInterval);
                    // the list is sized based on delay
                    if (numberToRemove > 0) _history.RemoveRange(0, numberToRemove); // remove the oldest, the head of the list
                }

                // this reading has been used
                if (!readingsUsed.Contains(ReadingSource))
                    readingsUsed.Add(ReadingSource);
            }
            public float StrokeWidth
            {
                get { return _strokeWidth; }
                set { _strokeWidth = value; }
            }
            public System.Drawing.Color Colour
            {
                get { return _colour; }
                set
                { _colour = value; }
            }
            public bool Setup
            {
                get { return _setup; }
                set { _setup = value; }
            }
            public NeedlePlacement Placement
            {
                get { return _placement; }
                set { _placement = value; }
            }
            public NeedleDirection Direction
            {
                get { return _needleDirection; }
                set { _needleDirection = value; }
            }
            public PointF NeedleOffset
            {
                get { return _needleOffset; }
                set { _needleOffset = value; }
            }
            public NeedleStyle Style
            {
                get { return _style; }
                set { _style = value; }
            }
            public override bool ShowHistory
            {
                get { return _showHistory; }
                set { _showHistory = value; }
            }
            public int HistoryDuration
            {
                get { return _msHistoryDuration; }
                set
                {
                    _msHistoryDuration = value;
                    if (_msHistoryDuration < UpdateInterval) _msHistoryDuration = UpdateInterval;
                }
            }
            public override float MinHistory
            {
                get
                {
                    lock (_historyLock)
                    {
                        if(_history.Count == 0) return Value;
                        return _history.Min();
                    }
                }
            }
            public override float MaxHistory
            {
                get
                {
                    lock (_historyLock)
                    {
                        if (_history.Count == 0) return Value;
                        return _history.Max();
                    }
                }
            }
            public override void History(out float minHistory, out float maxHistory)
            {
                lock (_historyLock)
                {
                    if (_history.Count == 0)
                    {
                        minHistory = Value;
                        maxHistory = Value;
                    }
                    else
                    {
                        minHistory = _history.Min();
                        maxHistory = _history.Max();
                    }
                }
            }
            public float LengthFactor
            {
                get { return _lengthFactor; }
                set { _lengthFactor = value; }
            }
            public override Dictionary<float, PointF> ScaleCalibration
            {
                get { return _scaleCalibration; }
                set { }
            }
            public PointF RadiusRatio
            {
                get { return _radiusRatio; }
                set { _radiusRatio = value; }
            }
        }
        private class clsText : clsMeterItem
        {
            private string _sText;
            private string _fontFamily;
            private FontStyle _fontStyle;
            private System.Drawing.Color _color;
            private bool _fixedSize;
            private float _fontSize;
            public clsText()
            {
                _sText = "";
                _fontFamily = "Trebuchet MS";
                _fontStyle = FontStyle.Regular;
                _color = System.Drawing.Color.White;
                _fixedSize = false;
                _fontSize = 9f;

                ItemType = MeterItemType.TEXT;
                ReadingSource = Reading.NONE;
                UpdateInterval = 100;
            }

            public string Text
            {
                get { return _sText; }
                set { _sText = value; }
            }
            public string FontFamily
            {
                get { return _fontFamily; }
                set { _fontFamily = value; }
            }
            public FontStyle Style
            {
                get { return _fontStyle; }
                set { _fontStyle = value; }
            }
            public System.Drawing.Color Colour
            {
                get { return _color; }
                set { _color = value; }
            }
            public bool FixedSize
            {
                get { return _fixedSize; }
                set { _fixedSize = value; }
            }
            public float FontSize
            {
                get { return _fontSize; }
                set { _fontSize = value; }
            }
        }
        //
        #endregion
        #region clsMeter
        private class clsMeter
        {
            // the single meter, that may contain many bars/labels/analog/images/etc
            private string _sGuid;
            private string _Name;
            private int _TypeBitfield; // bitfield of MeterType
            private int _rx;

            private float _XRatio; // 0-1
            private float _YRatio; // 0-1

            private Dictionary<string, clsMeterItem> _meterItems;
            private int _displayGroup;
            private List<string> _displayGroups;
            private bool _mox;

            public clsMeter(int rx, string sName, int typeBitfield, float XRatio = 1f, float YRatio = 0.441f)
            {
                // constructor
                _sGuid = Guid.NewGuid().ToString();
                _rx = rx;
                _Name = sName;
                _TypeBitfield = typeBitfield;
                _mox = false;

                _XRatio = XRatio;
                _YRatio = YRatio;

                _meterItems = new Dictionary<string, clsMeterItem>();
                _displayGroups = new List<string>();

                _displayGroup = 0;

                //test
                if ((TypeBitField & (int)MeterType.RX) == (int)MeterType.RX)
                {
                    //clsBarItem cb;

                    //cb = new clsBarItem();
                    //cb.TopLeft = new PointF(0.1f, 0.1f);
                    //cb.BottomRight = new PointF(0.9f, 0.15f);
                    //cb.ReadingSource = Reading.SIGNAL_STRENGTH;
                    //cb.AttackRatio = 0.1f;
                    //cb.DecayRatio = 0.05f;
                    //cb.UpdateInterval = 16;
                    //cb.ShowHistory = false;
                    //cb.Colour = System.Drawing.Color.DarkGray;
                    //cb.StrokeWidth = 1f;
                    //cb.Style = clsBarItem.BarStyle.Line;
                    //cb.ScaleCalibration.Add(-127, new PointF(0, 0)); // position for S0 or below
                    //cb.ScaleCalibration.Add(-73, new PointF(0.5f, 0)); // position for S9
                    //cb.ScaleCalibration.Add(-13, new PointF(1, 0)); // position for S9+60dB or above
                    //_meterItems.Add(cb.ID, cb);

                    //cb = new clsBarItem();
                    //cb.TopLeft = new PointF(0.1f, 0.1f);
                    //cb.BottomRight = new PointF(0.9f, 0.15f);
                    //cb.ReadingSource = Reading.AVG_SIGNAL_STRENGTH;
                    //cb.AttackRatio = 0.1f;
                    //cb.DecayRatio = 0.05f;
                    //cb.UpdateInterval = 16;
                    //cb.HistoryDuration = 4000;
                    //cb.ShowHistory = true;
                    //cb.Colour = System.Drawing.Color.White;
                    //cb.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Red);
                    //cb.Style = clsBarItem.BarStyle.Line;
                    //cb.ScaleCalibration.Add(-127, new PointF(0, 0)); // position for S0 or below
                    //cb.ScaleCalibration.Add(-73, new PointF(0.5f, 0)); // position for S9
                    //cb.ScaleCalibration.Add(-13, new PointF(1, 0)); // position for S9+60dB or above
                    //cb.ZOrder = 1;
                    //_meterItems.Add(cb.ID, cb);

                    //cb = new clsBarItem();
                    //cb.TopLeft = new PointF(0.1f, 0.2f);
                    //cb.BottomRight = new PointF(0.9f, 0.25f);
                    //cb.ReadingSource = Reading.AVG_SIGNAL_STRENGTH;
                    //cb.AttackRatio = 0.1f;
                    //cb.DecayRatio = 0.05f;
                    //cb.UpdateInterval = 16;
                    //cb.HistoryDuration = 2000;
                    //cb.ShowHistory = true;
                    //cb.Style = clsBarItem.BarStyle.Line;
                    //_meterItems.Add(cb.ID, cb);

                    //cb = new clsBarItem();
                    //cb.TopLeft = new PointF(0.1f, 0.3f);
                    //cb.BottomRight = new PointF(0.9f, 0.35f);
                    //cb.ReadingSource = Reading.ADC_REAL;
                    //cb.ShowHistory = true;
                    //_meterItems.Add(cb.ID, cb);

                    //cb = new clsBarItem();
                    //cb.TopLeft = new PointF(0.1f, 0.4f);
                    //cb.BottomRight = new PointF(0.9f, 0.45f);
                    //cb.ReadingSource = Reading.ADC_IMAG;
                    //cb.ShowHistory = true;
                    //_meterItems.Add(cb.ID, cb);

                    //clsSolidColour sc = new clsSolidColour();
                    //sc.TopLeft = new PointF(0.1f, 0.1f);
                    //sc.BottomRight = new PointF(0.9f, 0.15f);
                    //sc.Colour = System.Drawing.Color.Black;
                    //sc.ZOrder = -1;
                    //_meterItems.Add(sc.ID, sc);

                    //clsNeedleItem ni = new clsNeedleItem(); ;
                    //ni.TopLeft = new PointF(0f, 0f);
                    //ni.BottomRight = new PointF(1f, 0.579f);
                    //ni.ReadingSource = Reading.AVG_SIGNAL_STRENGTH;
                    //ni.AttackRatio = 0.1f;
                    //ni.DecayRatio = 0.05f;
                    //ni.UpdateInterval = 16;
                    //ni.HistoryDuration = 4000;
                    //ni.ShowHistory = true;
                    //ni.Style = clsNeedleItem.NeedleStyle.Line;
                    //ni.Colour = System.Drawing.Color.YellowGreen;
                    //ni.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Green);
                    //ni.ZOrder = 1;
                    //ni.NeedleOffset = new PointF(-0.004f, 0.616f);
                    //ni.LengthFactor = 1.14f;
                    ////ni.Setup = true;
                    //ni.ScaleCalibration.Add(-121f, new PointF(0.091f, 0.42f));
                    //ni.ScaleCalibration.Add(-115f, new PointF(0.144f, 0.359f));
                    //ni.ScaleCalibration.Add(-109f, new PointF(0.191f, 0.281f));
                    //ni.ScaleCalibration.Add(-103f, new PointF(0.25f, 0.243f));
                    //ni.ScaleCalibration.Add(-97f, new PointF(0.306f, 0.184f));
                    //ni.ScaleCalibration.Add(-91f, new PointF(0.37f, 0.17f));
                    //ni.ScaleCalibration.Add(-85f, new PointF(0.432f, 0.134f));
                    //ni.ScaleCalibration.Add(-79f, new PointF(0.495f, 0.147f));
                    //ni.ScaleCalibration.Add(-73f, new PointF(0.56f, 0.134f));
                    //ni.ScaleCalibration.Add(-63f, new PointF(0.623f, 0.153f));
                    //ni.ScaleCalibration.Add(-53f, new PointF(0.686f, 0.184f));
                    //ni.ScaleCalibration.Add(-43f, new PointF(0.745f, 0.225f));
                    //ni.ScaleCalibration.Add(-33f, new PointF(0.802f, 0.279f));
                    //ni.ScaleCalibration.Add(-23f, new PointF(0.865f, 0.344f));
                    //ni.ScaleCalibration.Add(-13f, new PointF(0.902f, 0.419f));
                    //_meterItems.Add(ni.ID, ni);

                    //ni = new clsNeedleItem();
                    //ni.TopLeft = new PointF(0f, 0f);
                    //ni.BottomRight = new PointF(1f, 0.579f);
                    //ni.ReadingSource = Reading.SIGNAL_STRENGTH;
                    //ni.AttackRatio = 0.1f;
                    //ni.DecayRatio = 0.05f;
                    //ni.UpdateInterval = 16;
                    //ni.ShowHistory = false;
                    //ni.Style = clsNeedleItem.NeedleStyle.Line;
                    //ni.Colour = System.Drawing.Color.LimeGreen;
                    //ni.StrokeWidth = 2f;
                    //ni.NeedleOffset = new PointF(-0.004f, 0.616f);
                    //ni.LengthFactor = 1.14f;
                    //ni.ScaleCalibration.Add(-121f, new PointF(0.091f, 0.42f));
                    //ni.ScaleCalibration.Add(-115f, new PointF(0.144f, 0.359f));
                    //ni.ScaleCalibration.Add(-109f, new PointF(0.191f, 0.281f));
                    //ni.ScaleCalibration.Add(-103f, new PointF(0.25f, 0.243f));
                    //ni.ScaleCalibration.Add(-97f, new PointF(0.306f, 0.184f));
                    //ni.ScaleCalibration.Add(-91f, new PointF(0.37f, 0.17f));
                    //ni.ScaleCalibration.Add(-85f, new PointF(0.432f, 0.134f));
                    //ni.ScaleCalibration.Add(-79f, new PointF(0.495f, 0.147f));
                    //ni.ScaleCalibration.Add(-73f, new PointF(0.56f, 0.134f));
                    //ni.ScaleCalibration.Add(-63f, new PointF(0.623f, 0.153f));
                    //ni.ScaleCalibration.Add(-53f, new PointF(0.686f, 0.184f));
                    //ni.ScaleCalibration.Add(-43f, new PointF(0.745f, 0.225f));
                    //ni.ScaleCalibration.Add(-33f, new PointF(0.802f, 0.279f));
                    //ni.ScaleCalibration.Add(-23f, new PointF(0.865f, 0.344f));
                    //ni.ScaleCalibration.Add(-13f, new PointF(0.902f, 0.419f));
                    //_meterItems.Add(ni.ID, ni);

                    ////sc = new clsSolidColour();
                    ////sc.TopLeft = new PointF(0.1f, 0.5f);
                    ////sc.BottomRight = new PointF(0.9f, 0.9f);
                    ////sc.Colour = System.Drawing.Color.FromArgb(255,48,48,48);
                    ////sc.ZOrder = -1;
                    ////_meterItems.Add(sc.ID, sc);

                    //clsImage img = new clsImage();
                    //img.TopLeft = new PointF(0f, 0f);
                    //img.BottomRight = new PointF(1f, 0.579f);
                    //img.ZOrder = -2;
                    //img.ImageName = "s-meter-green";
                    //_meterItems.Add(img.ID, img);

                    //img = new clsImage();
                    //img.TopLeft = new PointF(0.4f, 0.579f - 0.05f);
                    //img.BottomRight = new PointF(0.6f, 0.579f + 0.15f);
                    //img.ZOrder = 10;
                    //img.ImageName = "cover";
                    //img.Clipped = true;
                    //img.ClipTopLeft = new PointF(0f, 0f);
                    //img.ClipBottomRight = new PointF(1f, 0.579f);
                    //_meterItems.Add(img.ID, img);
                    ////
                    ///

                    //kenwood
                    clsNeedleItem ni = new clsNeedleItem(); ;
                    ni.TopLeft = new PointF(0f, 0.0f);
                    //ni.BottomRight = new PointF(1f, 0.441f);
                    ni.Size = new SizeF(1f, 0.441f);
                    ni.ReadingSource = Reading.AVG_SIGNAL_STRENGTH;
                    ni.AttackRatio = 0.1f;
                    ni.DecayRatio = 0.05f;
                    ni.UpdateInterval = 33;
                    ni.HistoryDuration = 4000;
                    ni.ShowHistory = true;
                    ni.Style = clsNeedleItem.NeedleStyle.Line;
                    ni.Colour = System.Drawing.Color.FromArgb(255, 233, 51, 50);
                    ni.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                    ni.ZOrder = 2;
                    ni.NeedleOffset = new PointF(0.004f, 0.736f);
                    ni.RadiusRatio = new PointF(1f, 0.58f);
                    ni.LengthFactor = 1.65f;
                    //ni.Setup = true;
                    ni.ScaleCalibration.Add(-127f, new PointF(0.076f, 0.31f));
                    ni.ScaleCalibration.Add(-121f, new PointF(0.131f, 0.272f));
                    ni.ScaleCalibration.Add(-115f, new PointF(0.189f, 0.254f));
                    ni.ScaleCalibration.Add(-109f, new PointF(0.233f, 0.211f));
                    ni.ScaleCalibration.Add(-103f, new PointF(0.284f, 0.207f));
                    ni.ScaleCalibration.Add(-97f, new PointF(0.326f, 0.177f));
                    ni.ScaleCalibration.Add(-91f, new PointF(0.374f, 0.177f));
                    ni.ScaleCalibration.Add(-85f, new PointF(0.414f, 0.151f));
                    ni.ScaleCalibration.Add(-79f, new PointF(0.459f, 0.168f));
                    ni.ScaleCalibration.Add(-73f, new PointF(0.501f, 0.142f));
                    ni.ScaleCalibration.Add(-63f, new PointF(0.564f, 0.172f));
                    ni.ScaleCalibration.Add(-53f, new PointF(0.63f, 0.164f));
                    ni.ScaleCalibration.Add(-43f, new PointF(0.695f, 0.203f));
                    ni.ScaleCalibration.Add(-33f, new PointF(0.769f, 0.211f));
                    ni.ScaleCalibration.Add(-23f, new PointF(0.838f, 0.272f));
                    ni.ScaleCalibration.Add(-13f, new PointF(0.926f, 0.31f));
                    ni.Value = -127f;
                    MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    _meterItems.Add(ni.ID, ni);

                    //ni = new clsNeedleItem(); ;
                    //ni.TopLeft = new PointF(0f, 0f);
                    //ni.BottomRight = new PointF(1f, 0.441f);
                    //ni.ReadingSource = Reading.SIGNAL_STRENGTH;
                    //ni.AttackRatio = 0.1f;
                    //ni.DecayRatio = 0.05f;
                    //ni.UpdateInterval = 16;
                    //ni.HistoryDuration = 500;
                    //ni.ShowHistory = false;
                    //ni.Style = clsNeedleItem.NeedleStyle.Line;
                    //ni.Colour = System.Drawing.Color.FromArgb(255, 233, 51, 50);
                    //ni.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                    //ni.ZOrder = 1;
                    //ni.NeedleOffset = new PointF(0.004f, 0.736f);
                    //ni.RadiusRatio = new PointF(1f, 0.58f);
                    //ni.LengthFactor = 1.65f;
                    //ni.StrokeWidth = 1f;
                    ////ni.Setup = true;
                    //ni.ScaleCalibration.Add(-127f, new PointF(0.076f, 0.31f));
                    //ni.ScaleCalibration.Add(-121f, new PointF(0.131f, 0.272f));
                    //ni.ScaleCalibration.Add(-115f, new PointF(0.189f, 0.254f));
                    //ni.ScaleCalibration.Add(-109f, new PointF(0.233f, 0.211f));
                    //ni.ScaleCalibration.Add(-103f, new PointF(0.284f, 0.207f));
                    //ni.ScaleCalibration.Add(-97f, new PointF(0.326f, 0.177f));
                    //ni.ScaleCalibration.Add(-91f, new PointF(0.374f, 0.177f));
                    //ni.ScaleCalibration.Add(-85f, new PointF(0.414f, 0.151f));
                    //ni.ScaleCalibration.Add(-79f, new PointF(0.459f, 0.168f));
                    //ni.ScaleCalibration.Add(-73f, new PointF(0.501f, 0.142f));
                    //ni.ScaleCalibration.Add(-63f, new PointF(0.564f, 0.172f));
                    //ni.ScaleCalibration.Add(-53f, new PointF(0.63f, 0.164f));
                    //ni.ScaleCalibration.Add(-43f, new PointF(0.695f, 0.203f));
                    //ni.ScaleCalibration.Add(-33f, new PointF(0.769f, 0.211f));
                    //ni.ScaleCalibration.Add(-23f, new PointF(0.838f, 0.272f));
                    //ni.ScaleCalibration.Add(-13f, new PointF(0.926f, 0.31f));
                    //ni.Value = -127f;
                    //MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    //_meterItems.Add(ni.ID, ni);

                    //volts
                    ni = new clsNeedleItem(); ;
                    ni.TopLeft = new PointF(0f, 0f);
                    //ni.BottomRight = new PointF(1f, 0.441f);
                    ni.Size = new SizeF(1f, 0.441f);
                    ni.ReadingSource = Reading.VOLTS;
                    ni.AttackRatio = 0.2f;
                    ni.DecayRatio = 0.2f;
                    ni.UpdateInterval = 100;
                    ni.HistoryDuration = 500;
                    ni.ShowHistory = false;
                    ni.Style = clsNeedleItem.NeedleStyle.Line;
                    ni.Colour = System.Drawing.Color.Black;
                    ni.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                    ni.ZOrder = 1;
                    ni.NeedleOffset = new PointF(0.004f, 0.736f);
                    ni.RadiusRatio = new PointF(1f, 0.58f);
                    ni.LengthFactor = .75f;
                    //ni.Setup = true;
                    ni.ScaleCalibration.Add(10f, new PointF(0.559f, 0.756f));
                    ni.ScaleCalibration.Add(12.5f, new PointF(0.605f, 0.772f));
                    ni.ScaleCalibration.Add(15f, new PointF(0.665f, 0.784f));
                    ni.Value = 10f;
                    MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    _meterItems.Add(ni.ID, ni);

                    clsImage img = new clsImage();
                    img.TopLeft = new PointF(0f, 0f);
                    //img.BottomRight = new PointF(1f, 0.441f);
                    img.Size = new SizeF(1f, 0.441f);
                    img.ZOrder = 0;
                    img.ImageName = "kenwood-s-meter";
                    _meterItems.Add(img.ID, img);

                    img = new clsImage();
                    img.TopLeft = new PointF(0f, 0.441f);
                    //img.BottomRight = new PointF(1f, 0.441f + 0.218f);
                    img.Size = new SizeF(1f, 0.101f);
                    img.ZOrder = 2;
                    img.ImageName = "kenwood-s-meter-bg";
                    _meterItems.Add(img.ID, img);
                }
                else if ((typeBitfield & (int)MeterType.MOX) == (int)MeterType.MOX)
                {
                    clsNeedleItem ni;
                    clsImage img;
                    //// pwr
                    //ni = new clsNeedleItem();
                    //ni.TopLeft = new PointF(0.1f, 0.54f);
                    //ni.BottomRight = new PointF(0.9f, 0.99f);
                    //ni.ReadingSource = Reading.PWR;
                    //ni.AttackRatio = 0.2f;
                    //ni.DecayRatio = 0.05f;
                    //ni.UpdateInterval = 16;
                    //ni.HistoryDuration = 1000;
                    //ni.ShowHistory = true;
                    //ni.Style = clsNeedleItem.NeedleStyle.Line;
                    //ni.Colour = System.Drawing.Color.Red;
                    //ni.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Red);
                    //ni.ZOrder = 1;
                    //ni.NeedleOffset = new PointF(0f, 0.64f);
                    //ni.LengthFactor = 1.08f;
                    //ni.ScaleCalibration.Add(0, new PointF(0.119f, 0.468f)); // position for 0W
                    //ni.ScaleCalibration.Add(5, new PointF(0.214f, 0.341f));
                    //ni.ScaleCalibration.Add(10, new PointF(0.277f, 0.292f));
                    //ni.ScaleCalibration.Add(20, new PointF(0.356f, 0.240f));
                    //ni.ScaleCalibration.Add(30, new PointF(0.428f, 0.208f));
                    //ni.ScaleCalibration.Add(50, new PointF(0.557f, 0.124f));
                    //ni.ScaleCalibration.Add(60, new PointF(0.615f, 0.225f));
                    //ni.ScaleCalibration.Add(100, new PointF(0.786f, 0.346f)); // position for MAX W
                    ////ni.Setup = true;
                    //_meterItems.Add(ni.ID, ni);

                    ////swr
                    //ni = new clsNeedleItem();
                    //ni.TopLeft = new PointF(0.1f, 0.54f);
                    //ni.BottomRight = new PointF(0.9f, 0.99f);
                    //ni.ReadingSource = Reading.SWR;
                    //ni.AttackRatio = 0.1f;
                    //ni.DecayRatio = 0.05f;
                    //ni.UpdateInterval = 16;
                    //ni.HistoryDuration = 500;
                    //ni.ShowHistory = true;
                    //ni.Style = clsNeedleItem.NeedleStyle.Line;
                    //ni.Colour = System.Drawing.Color.Yellow;
                    //ni.HistoryColour = System.Drawing.Color.FromArgb(128, System.Drawing.Color.Yellow);
                    //ni.ZOrder = 2;
                    //ni.NeedleOffset = new PointF(0f, 0.64f);
                    //ni.LengthFactor = 0.88f;
                    //ni.ScaleCalibration.Add(1f, new PointF(0.179f, 0.577f)); // position for 1:1
                    //ni.ScaleCalibration.Add(1.5f, new PointF(0.238f, 0.483f));
                    //ni.ScaleCalibration.Add(2f, new PointF(0.298f, 0.427f));
                    //ni.ScaleCalibration.Add(2.5f, new PointF(0.370f, 0.379f));
                    //ni.ScaleCalibration.Add(3f, new PointF(0.419f, 0.358f));
                    //ni.ScaleCalibration.Add(3.5f, new PointF(0.467f, 0.352f));
                    //ni.ScaleCalibration.Add(4f, new PointF(0.503f, 0.350f));
                    //ni.ScaleCalibration.Add(5f, new PointF(0.554f, 0.358f));
                    //ni.ScaleCalibration.Add(10f, new PointF(0.651f, 0.397f)); // position for 10:1
                    //ni.ScaleCalibration.Add(float.MaxValue, new PointF(0.744f, 0.479f)); // position for infinity
                    //_meterItems.Add(ni.ID, ni);

                    //img = new clsImage();
                    //img.TopLeft = new PointF(0.1f, 0.54f);
                    //img.BottomRight = new PointF(0.9f, 0.99f);
                    //img.ZOrder = -2;
                    //img.ImageName = "s-meter-green";
                    //_meterItems.Add(img.ID, img);

                    //img = new clsImage();
                    //img.TopLeft = new PointF(0.4f, 0.95f);
                    //img.BottomRight = new PointF(0.6f, 1.15f);
                    //img.ZOrder = 10;
                    //img.ImageName = "cover";
                    //img.Clipped = true;
                    //img.ClipTopLeft = new PointF(0.1f, 0.54f);
                    //img.ClipBottomRight = new PointF(0.9f, 0.99f); ;
                    //_meterItems.Add(img.ID, img);


                    // cross
                    ni = new clsNeedleItem();
                    ni.TopLeft = new PointF(0f, 0f);
                    //ni.BottomRight = new PointF(1f, 0.782f);
                    ni.Size = new SizeF(1f, 0.782f);
                    ni.ReadingSource = Reading.PWR;
                    ni.AttackRatio = 0.2f;//0.325f;
                    ni.DecayRatio = 0.1f;//0.5f;
                    ni.UpdateInterval = 33;
                    ni.HistoryDuration = 1000;
                    ni.ShowHistory = true;
                    ni.StrokeWidth = 2.5f;
                    ni.Style = clsNeedleItem.NeedleStyle.Line;
                    ni.Colour = System.Drawing.Color.Black;
                    ni.HistoryColour = System.Drawing.Color.FromArgb(64, System.Drawing.Color.Red);
                    ni.ZOrder = 2;
                    ni.NeedleOffset = new PointF(0.322f, 0.611f);
                    ni.LengthFactor = 1.62f;
                    ni.ScaleCalibration.Add(0f, new PointF(0.052f, 0.732f));
                    ni.ScaleCalibration.Add(5f, new PointF(0.146f, 0.528f));
                    ni.ScaleCalibration.Add(10f, new PointF(0.188f, 0.434f));
                    ni.ScaleCalibration.Add(15f, new PointF(0.235f, 0.387f));
                    ni.ScaleCalibration.Add(20f, new PointF(0.258f, 0.338f));
                    ni.ScaleCalibration.Add(25f, new PointF(0.303f, 0.313f));
                    ni.ScaleCalibration.Add(30f, new PointF(0.321f, 0.272f));
                    ni.ScaleCalibration.Add(35f, new PointF(0.361f, 0.257f));
                    ni.ScaleCalibration.Add(40f, new PointF(0.381f, 0.223f));
                    ni.ScaleCalibration.Add(50f, new PointF(0.438f, 0.181f));
                    ni.ScaleCalibration.Add(60f, new PointF(0.483f, 0.155f));
                    ni.ScaleCalibration.Add(70f, new PointF(0.532f, 0.13f));
                    ni.ScaleCalibration.Add(80f, new PointF(0.577f, 0.111f));
                    ni.ScaleCalibration.Add(90f, new PointF(0.619f, 0.098f));
                    ni.ScaleCalibration.Add(100f, new PointF(0.662f, 0.083f));
                    ni.Value = 0f;
                    MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    _meterItems.Add(ni.ID, ni);

                    ni = new clsNeedleItem();
                    ni.TopLeft = new PointF(0f, 0f);
                    //ni.BottomRight = new PointF(1f, 0.782f);
                    ni.Size = new SizeF(1f, 0.782f);
                    ni.ReadingSource = Reading.REVERSE_PWR;
                    ni.AttackRatio = 0.2f;//0.325f;
                    ni.DecayRatio = 0.1f;//0.5f;
                    ni.UpdateInterval = 33;
                    ni.HistoryDuration = 1000;
                    ni.ShowHistory = true;
                    ni.StrokeWidth = 2.5f;
                    ni.Direction = clsNeedleItem.NeedleDirection.CounterClockwise;
                    ni.Style = clsNeedleItem.NeedleStyle.Line;
                    ni.Colour = System.Drawing.Color.Black;
                    ni.HistoryColour = System.Drawing.Color.FromArgb(64, System.Drawing.Color.CornflowerBlue);
                    ni.ZOrder = 1;
                    ni.NeedleOffset = new PointF(-0.322f, 0.611f);
                    ni.LengthFactor = 1.62f;
                    ni.ScaleCalibration.Add(0f, new PointF(0.948f, 0.74f));
                    ni.ScaleCalibration.Add(0.25f, new PointF(0.913f, 0.7f));
                    ni.ScaleCalibration.Add(0.5f, new PointF(0.899f, 0.638f));
                    ni.ScaleCalibration.Add(0.75f, new PointF(0.875f, 0.594f));
                    ni.ScaleCalibration.Add(1f, new PointF(0.854f, 0.538f));
                    ni.ScaleCalibration.Add(2f, new PointF(0.814f, 0.443f));
                    ni.ScaleCalibration.Add(3f, new PointF(0.769f, 0.4f));
                    ni.ScaleCalibration.Add(4f, new PointF(0.744f, 0.351f));
                    ni.ScaleCalibration.Add(5f, new PointF(0.702f, 0.321f));
                    ni.ScaleCalibration.Add(6f, new PointF(0.682f, 0.285f));
                    ni.ScaleCalibration.Add(7f, new PointF(0.646f, 0.268f));
                    ni.ScaleCalibration.Add(8f, new PointF(0.626f, 0.234f));
                    ni.ScaleCalibration.Add(9f, new PointF(0.596f, 0.228f));
                    ni.ScaleCalibration.Add(10f, new PointF(0.569f, 0.196f));
                    ni.ScaleCalibration.Add(12f, new PointF(0.524f, 0.166f));
                    ni.ScaleCalibration.Add(14f, new PointF(0.476f, 0.14f));
                    ni.ScaleCalibration.Add(16f, new PointF(0.431f, 0.121f));
                    ni.ScaleCalibration.Add(18f, new PointF(0.393f, 0.109f));
                    ni.ScaleCalibration.Add(20f, new PointF(0.349f, 0.098f));
                    ni.Value = 0f;
                    MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    _meterItems.Add(ni.ID, ni);

                    img = new clsImage();
                    img.TopLeft = new PointF(0f, 0f);
                    img.BottomRight = new PointF(1f, 0.782f);
                    img.Size = new SizeF(1f, 0.782f);
                    img.ZOrder = 0;
                    img.ImageName = "cross-needle-100";
                    _meterItems.Add(img.ID, img);

                    img = new clsImage();
                    img.TopLeft = new PointF(0f, 0.782f - 0.180f);
                    //img.BottomRight = new PointF(1f, 1f);
                    img.Size = new SizeF(1f, 0.217f);
                    img.ZOrder = 3;
                    img.ImageName = "cross-needle-bg";
                    _meterItems.Add(img.ID, img);
                    // end cross

                    PointF tl = new PointF(0f, (0.782f - 0.180f) + 0.217f);
                    //kenwood
                    clsClickBox cb = new clsClickBox();
                    cb.TopLeft = tl;
                    cb.Size = new SizeF(1f, 0.441f);
                    cb.Button = MouseButtons.Left;
                    _meterItems.Add(cb.ID, cb);

                    clsText tx = new clsText();
                    tx.TopLeft = new PointF(tl.X + 0.74f, tl.Y + 0.48f);
                    tx.Size = new SizeF(0.2f, 0.2f);
                    tx.Text = "%group%";
                    tx.ZOrder = 10;
                    tx.Style = FontStyle.Bold;
                    tx.Colour = System.Drawing.Color.DarkGray;
                    tx.FixedSize = true;
                    tx.FontSize = 8f;
                    _meterItems.Add(tx.ID, tx);

                    ni = new clsNeedleItem(); ;
                    ni.TopLeft = tl;// new PointF(0f, 0.85f);
                    //ni.BottomRight = new PointF(1f, 0.441f);
                    ni.Size = new SizeF(1f, 0.441f);
                    ni.ReadingSource = Reading.PWR;
                    ni.AttackRatio = 0.2f;//0.325f;
                    ni.DecayRatio = 0.1f;//0.5f;
                    ni.UpdateInterval = 33;
                    ni.HistoryDuration = 1000;
                    ni.ShowHistory = true;
                    ni.Style = clsNeedleItem.NeedleStyle.Line;
                    ni.Colour = System.Drawing.Color.FromArgb(255, 233, 51, 50);
                    ni.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                    ni.ZOrder = 2;
                    ni.NeedleOffset = new PointF(0.004f, 0.736f);
                    ni.RadiusRatio = new PointF(1f, 0.58f);
                    ni.LengthFactor = 1.55f;
                    ni.DisplayGroup = 1;
                    //ni.Setup = true;
                    ni.ScaleCalibration.Add(0f, new PointF(0.099f, 0.352f));
                    ni.ScaleCalibration.Add(5f, new PointF(0.164f, 0.312f));
                    ni.ScaleCalibration.Add(10f, new PointF(0.224f, 0.28f));
                    ni.ScaleCalibration.Add(25f, new PointF(0.335f, 0.236f));
                    ni.ScaleCalibration.Add(30f, new PointF(0.367f, 0.228f));
                    ni.ScaleCalibration.Add(40f, new PointF(0.436f, 0.22f));
                    ni.ScaleCalibration.Add(50f, new PointF(0.499f, 0.212f));
                    ni.ScaleCalibration.Add(60f, new PointF(0.559f, 0.216f));
                    ni.ScaleCalibration.Add(100f, new PointF(0.751f, 0.272f));
                    ni.ScaleCalibration.Add(150f, new PointF(0.899f, 0.352f));
                    ni.Value = 0f;
                    MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    _meterItems.Add(ni.ID, ni);

                    ni = new clsNeedleItem(); ;
                    ni.TopLeft = tl;//new PointF(0f, 0.85f);
                    //ni.BottomRight = new PointF(1f, 0.441f);
                    ni.Size = new SizeF(1f, 0.441f);
                    ni.ReadingSource = Reading.SWR;
                    ni.AttackRatio = 0.2f;//0.325f;
                    ni.DecayRatio = 0.1f;//0.5f;
                    ni.UpdateInterval = 33;
                    ni.HistoryDuration = 1000;
                    ni.ShowHistory = true;
                    ni.Style = clsNeedleItem.NeedleStyle.Line;
                    ni.Colour = System.Drawing.Color.Black;
                    ni.HistoryColour = System.Drawing.Color.FromArgb(64, System.Drawing.Color.CornflowerBlue);
                    ni.ZOrder = 3;
                    ni.NeedleOffset = new PointF(0.004f, 0.736f);
                    ni.RadiusRatio = new PointF(1f, 0.58f);
                    ni.LengthFactor = 1.36f;
                    ni.DisplayGroup = 1;
                    //ni.Setup = true;
                    ni.ScaleCalibration.Add(1f, new PointF(0.152f, 0.468f));
                    ni.ScaleCalibration.Add(1.5f, new PointF(0.28f, 0.404f));
                    ni.ScaleCalibration.Add(2f, new PointF(0.393f, 0.372f));
                    ni.ScaleCalibration.Add(2.5f, new PointF(0.448f, 0.36f));
                    ni.ScaleCalibration.Add(3f, new PointF(0.499f, 0.36f));
                    ni.ScaleCalibration.Add(10f, new PointF(0.847f, 0.476f));
                    ni.Value = 1f;
                    MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    _meterItems.Add(ni.ID, ni);

                    ni = new clsNeedleItem(); ;
                    ni.TopLeft = tl;//new PointF(0f, 0.85f);
                    //ni.BottomRight = new PointF(1f, 0.441f);
                    ni.Size = new SizeF(1f, 0.441f);
                    ni.ReadingSource = Reading.ALC_G; // alc_comp
                    ni.AttackRatio = 0.2f;//0.325f;
                    ni.DecayRatio = 0.1f;//0.5f;
                    ni.UpdateInterval = 33;
                    ni.HistoryDuration = 1000;
                    ni.ShowHistory = false;
                    ni.Style = clsNeedleItem.NeedleStyle.Line;
                    ni.Colour = System.Drawing.Color.Black;
                    ni.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                    ni.ZOrder = 3;
                    ni.NeedleOffset = new PointF(0.004f, 0.736f);
                    ni.RadiusRatio = new PointF(1f, 0.58f);
                    ni.LengthFactor = 0.96f;
                    ni.DisplayGroup = 2;
                    //ni.Setup = true;
                    ni.ScaleCalibration.Add(0f, new PointF(0.249f, 0.68f));
                    ni.ScaleCalibration.Add(5f, new PointF(0.342f, 0.64f));
                    ni.ScaleCalibration.Add(10f, new PointF(0.425f, 0.624f));
                    ni.ScaleCalibration.Add(15f, new PointF(0.499f, 0.62f));
                    ni.ScaleCalibration.Add(20f, new PointF(0.571f, 0.628f));
                    ni.ScaleCalibration.Add(25f, new PointF(0.656f, 0.64f));
                    ni.ScaleCalibration.Add(30f, new PointF(0.751f, 0.688f));
                    ni.Value = 0f;
                    MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    _meterItems.Add(ni.ID, ni);

                    ni = new clsNeedleItem();
                    ni.TopLeft = tl;//new PointF(0f, 0.85f);
                    //ni.BottomRight = new PointF(1f, 0.441f);
                    ni.Size = new SizeF(1f, 0.441f);
                    ni.ReadingSource = Reading.ALC_GROUP;
                    ni.AttackRatio = 0.2f;//0.325f;
                    ni.DecayRatio = 0.1f;//0.5f;
                    ni.UpdateInterval = 33;
                    ni.HistoryDuration = 1000;
                    ni.ShowHistory = false;
                    ni.Style = clsNeedleItem.NeedleStyle.Line;
                    ni.Colour = System.Drawing.Color.Red;
                    ni.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                    ni.ZOrder = 3;
                    ni.NeedleOffset = new PointF(0.004f, 0.736f);
                    ni.RadiusRatio = new PointF(1f, 0.58f);
                    ni.LengthFactor = 0.75f;
                    ni.DisplayGroup = 3;
                    //ni.Setup = true;
                    ni.ScaleCalibration.Add(-30f, new PointF(0.295f, 0.804f));
                    ni.ScaleCalibration.Add(0f, new PointF(0.332f, 0.784f));
                    ni.ScaleCalibration.Add(25f, new PointF(0.499f, 0.756f));
                    ni.Value = -30f;
                    MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    _meterItems.Add(ni.ID, ni);

                    //volts
                    ni = new clsNeedleItem(); ;
                    ni.TopLeft = tl;//new PointF(0f, 0.85f);
                    //ni.BottomRight = new PointF(1f, 0.441f);
                    ni.Size = new SizeF(1f, 0.441f);
                    ni.ReadingSource = Reading.VOLTS;
                    ni.AttackRatio = 0.2f;
                    ni.DecayRatio = 0.2f;
                    ni.UpdateInterval = 100;
                    ni.HistoryDuration = 500;
                    ni.ShowHistory = false;
                    ni.Style = clsNeedleItem.NeedleStyle.Line;
                    ni.Colour = System.Drawing.Color.Black;
                    ni.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                    ni.ZOrder = 1;
                    ni.NeedleOffset = new PointF(0.004f, 0.736f);
                    ni.RadiusRatio = new PointF(1f, 0.58f);
                    ni.LengthFactor = .75f;
                    //ni.Setup = true;
                    ni.ScaleCalibration.Add(10f, new PointF(0.559f, 0.756f));
                    ni.ScaleCalibration.Add(12.5f, new PointF(0.605f, 0.772f));
                    ni.ScaleCalibration.Add(15f, new PointF(0.665f, 0.784f));
                    ni.Value = 10f;
                    MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    _meterItems.Add(ni.ID, ni);

                    //amps
                    ni = new clsNeedleItem(); ;
                    ni.TopLeft = tl;//new PointF(0f, 0.85f);
                    //ni.BottomRight = new PointF(1f, 0.441f);
                    ni.Size = new SizeF(1f, 0.441f);
                    ni.ReadingSource = Reading.AMPS;
                    ni.AttackRatio = 0.2f;
                    ni.DecayRatio = 0.2f;
                    ni.UpdateInterval = 100;
                    ni.HistoryDuration = 500;
                    ni.ShowHistory = false;
                    ni.Style = clsNeedleItem.NeedleStyle.Line;
                    ni.Colour = System.Drawing.Color.Black;
                    ni.HistoryColour = System.Drawing.Color.FromArgb(64, 233, 51, 50);
                    ni.ZOrder = 1;
                    ni.NeedleOffset = new PointF(0.004f, 0.736f);
                    ni.RadiusRatio = new PointF(1f, 0.58f);
                    ni.LengthFactor = 1.15f;
                    ni.DisplayGroup = 4;
                    //ni.Setup = true;
                    ni.ScaleCalibration.Add(0f, new PointF(0.199f, 0.576f));
                    ni.ScaleCalibration.Add(2f, new PointF(0.27f, 0.54f));
                    ni.ScaleCalibration.Add(4f, new PointF(0.333f, 0.516f));
                    ni.ScaleCalibration.Add(6f, new PointF(0.393f, 0.504f));
                    ni.ScaleCalibration.Add(8f, new PointF(0.448f, 0.492f));
                    ni.ScaleCalibration.Add(10f, new PointF(0.499f, 0.492f));
                    ni.ScaleCalibration.Add(12f, new PointF(0.554f, 0.488f));
                    ni.ScaleCalibration.Add(14f, new PointF(0.608f, 0.5f));
                    ni.ScaleCalibration.Add(16f, new PointF(0.667f, 0.516f));
                    ni.ScaleCalibration.Add(18f, new PointF(0.728f, 0.54f));
                    ni.ScaleCalibration.Add(20f, new PointF(0.799f, 0.576f));
                    ni.Value = 10f;
                    MeterManager.setReading(rx, ni.ReadingSource, ni.Value);
                    _meterItems.Add(ni.ID, ni);

                    img = new clsImage();
                    img.TopLeft = tl;//new PointF(0f, 0.85f);
                    //img.BottomRight = new PointF(1f, 0.441f);
                    img.Size = new SizeF(1f, 0.441f);
                    img.ZOrder = 0;
                    img.ImageName = "kenwood-s-meter";
                    _meterItems.Add(img.ID, img);

                    img = new clsImage();
                    img.TopLeft = new PointF(tl.X, tl.Y + 0.441f);//new PointF(0f, 0.441f + 0.85f);
                    //img.BottomRight = new PointF(1f, 0.441f + 0.218f);
                    img.Size = new SizeF(1f, 0.101f);
                    img.ZOrder = 2;
                    img.ImageName = "kenwood-s-meter-bg";
                    _meterItems.Add(img.ID, img);
                }
            }
            public void MouseUp(System.Windows.Forms.MouseEventArgs e, clsMeter m, clsClickBox cb)
            {
                if (cb.GotoGroup > 0)
                {
                    m.DisplayGroup = cb.GotoGroup;
                }
                else
                {
                    if (e.Button == MouseButtons.Left)
                        IncrementDisplayGroup();
                    else if (e.Button == MouseButtons.Right)
                        DecrementDisplayGroup();
                }
            }
            public bool MOX
            {
                get { return _mox; }
                set { _mox = value; }
            }
            public float XRatio { get { return _XRatio; } }
            public float YRatio { get { return _YRatio; } }
            public string Name { get { return _Name; } }
            public int TypeBitField { get { return _TypeBitfield; } }
            public string ID { get { return _sGuid; } }
            public int RX { get { return _rx; } }

            public int QuickestUpdateInterval
            {
                get
                {
                    int updateInterval = int.MaxValue;
                    foreach (var kvp in _meterItems)
                    {
                        clsMeterItem mi = kvp.Value;
                        if (mi.UpdateInterval < updateInterval) updateInterval = mi.UpdateInterval;
                    }
                    return updateInterval;
                }
            }
            public void Update(ref List<Reading> readingsUsed)
            {
                // this is called for each meter from the meter manager thread
                foreach (KeyValuePair<string, clsMeterItem> kvp in _meterItems)
                {
                    // now we need to update each item in the meter
                    if(kvp.Value.RequiresUpdate) kvp.Value.Update(_rx, ref readingsUsed);
                }
            }
            public Dictionary<string, clsMeterItem> MeterItems
            {
                get { return _meterItems; }
            }
            public void IncrementDisplayGroup()
            {
                _displayGroup++;
                if (_displayGroup > _displayGroups.Count) _displayGroup = 1;
            }
            public void DecrementDisplayGroup()
            {
                _displayGroup--;
                if (_displayGroup < 1) _displayGroup = _displayGroups.Count;
            }
            public int DisplayGroups
            {
                get { return _displayGroups.Count; }
            }
            public int DisplayGroup
            {
                get { return _displayGroup; }
                set
                {
                    _displayGroup = value;
                    if (_displayGroup < 1) _displayGroup = 1;
                    if (_displayGroup > _displayGroups.Count) _displayGroup = _displayGroups.Count;
                }
            }
            public void AddDisplayGroup(string sName)
            {
                if (_displayGroups.Contains(sName)) return;

                _displayGroups.Add(sName);
                _displayGroup = 0; // 0 is all
            }
            public string DisplayGroupText
            {
                get
                {
                    if (_displayGroup == 0) return "ALL";

                    return _displayGroups[_displayGroup-1];
                }
            }           
        }
        #endregion
        #region clsReading
        private class clsReading
        {
            public Reading Type;
            public float Reading;
            public bool Updated;
        }
        private class clsReadings
        {
            private Dictionary<Reading, clsReading> _latestReading;
            public clsReadings()
            {
                _latestReading = new Dictionary<Reading, clsReading>();

                for (int n = 0; n < (int)Reading.LAST; n++)
                {
                    clsReading cr = new clsReading() { Type = (Reading)n, Reading = -200f, Updated = false };
                    _latestReading.Add((Reading)n, cr);
                }
            }
            public float GetReading(Reading rt, bool useReading = false)
            {
                if (useReading) UseReading(rt);
                return _latestReading[rt].Reading;
            }
            public void SetReading(Reading rt, float value)
            {
                _latestReading[rt].Reading = value;
                _latestReading[rt].Updated = true;
            }
            public bool RequiresUpdate(Reading rt)
            {
                return !_latestReading[rt].Updated;
            }
            public void UseReading(Reading rt)
            {
                _latestReading[rt].Updated = false;
            }
        }
        #endregion       
        #region DirectX
        //based on display.cs
        private class DXRenderer
        {
            private const SharpDX.Direct2D1.AlphaMode _ALPHA_MODE = SharpDX.Direct2D1.AlphaMode.Premultiplied;
            private bool _bDXSetup;
            private bool _displayRunning;
            private bool _dxDisplayThreadRunning;
            private Thread _dxRenderThread;
            private PresentFlags _NoVSYNCpresentFlag;
            private SharpDX.DXGI.Factory1 _factory1;
            private SharpDX.Direct3D11.Device _device;
            private int _nBufferCount;
            private Surface _surface;
            private SwapChain _swapChain;
            private SwapChain1 _swapChain1;
            private RenderTarget _renderTarget;
            private SharpDX.Direct2D1.Factory _factory;
            private bool _bAntiAlias;
            private Vector2 _pixelShift;
            private int _nVBlanks;
            private PictureBox _displayTarget;
            private object _DXlock = new object();
            //
            private Dictionary<System.Drawing.Color, SharpDX.Direct2D1.Brush> _DXBrushes;
            private Color4 _backColour;
            private Dictionary<string, SharpDX.Direct2D1.Bitmap> _images;
            //
            private Dictionary<string, SharpDX.DirectWrite.TextFormat> _textFormats; // fonts
            private static SharpDX.DirectWrite.Factory _fontFactory;
            private Dictionary<string, SizeF> _stringMeasure;
            //

            private int _rx;
            private Console _console;

            public DXRenderer(int rx, PictureBox target, Console c, string sImagePath)
            {
                _rx = rx;
                _console = c;

                //dx
                _displayRunning = false;
                _DXBrushes = null;
                _textFormats = null;
                _stringMeasure = null;
                _bAntiAlias = true;
                _bDXSetup = false;
                _dxDisplayThreadRunning = false;
                _NoVSYNCpresentFlag = PresentFlags.None;
                _pixelShift = new Vector2(0.5f, 0.5f);
                _nVBlanks = 0;
                _displayTarget = target;
                _backColour = convertColour(System.Drawing.Color.Black);
                //_nBufferCount = 1;
                _images = new Dictionary<string, SharpDX.Direct2D1.Bitmap>();

                _displayTarget.Resize += target_Resize;
                _displayTarget.MouseUp += OnMouseUp;

                init();

                loadImages(sImagePath);

                _dxDisplayThreadRunning = false;
                _dxRenderThread = new Thread(new ThreadStart(dxRender))
                {
                    Name = "Multimeter DX Render Thread",
                    Priority = ThreadPriority.Lowest,
                    IsBackground = true
                };
                _dxRenderThread.Start();

                _displayRunning = true;
            }

            //DIRECTX
            private void loadImages(string sImagePath)
            {
                if (!sImagePath.EndsWith("\\")) sImagePath += "\\";

                if (System.IO.Directory.Exists(sImagePath))
                {
                    string[] sFiles = System.IO.Directory.GetFiles(sImagePath);
                    foreach (string sFile in sFiles)
                    {
                        string sID = System.IO.Path.GetFileNameWithoutExtension(sFile);
                        loadImage(sFile, sID);
                    }
                }
            }
            private void loadImage(string sPath, string sID)
            {
                if (System.IO.File.Exists(sPath) && !_images.ContainsKey(sID))
                {
                    try
                    {
                        System.Drawing.Image image = System.Drawing.Image.FromFile(sPath);

                        if (image != null)
                        {
                            using (System.Drawing.Bitmap graphicsImage = new System.Drawing.Bitmap(image))
                            {
                                SharpDX.Direct2D1.Bitmap img = bitmapFromSysBitmap(_renderTarget, graphicsImage);
                                _images.Add(sID, img);
                            }
                        }
                    }
                    catch { }
                }
            }
            private int getMaxSamples()
            {
                //D3D11_MAX_MULTISAMPLE_SAMPLE_COUNT 32u32
                //NOTE: can not be used with FlipDiscard atm unfortunately
                for (int n = 32; n>0; n /= 2)
                {
                    if(_device.CheckMultisampleQualityLevels(Format.B8G8R8A8_UNorm, n) > 0)
                    {
                        return n;
                    }
                }
                return 1;
            }
            private void init(DriverType driverType = DriverType.Hardware)
            {
                // code based on display.cs
                if (_bDXSetup) return;

                try
                {
                    DeviceCreationFlags debug = DeviceCreationFlags.None;

                    // to get this to work, need to target the os
                    // https://www.prugg.at/2019/09/09/properly-detect-windows-version-in-c-net-even-windows-10/
                    // you need to enable operating system support in the app1.manifest file, otherwise majVers will not report 10+
                    // note: windows 10, 11, server 2016, server 2019, server 2022 all use the windows 10 os id in the manifest file at this current time
                    int majVers = Environment.OSVersion.Version.Major;
                    int minVers = Environment.OSVersion.Version.Minor;

                    SharpDX.Direct3D.FeatureLevel[] featureLevels;

                    if (majVers >= 10) // win10 + 11
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_12_1,
                            SharpDX.Direct3D.FeatureLevel.Level_12_0,
                            SharpDX.Direct3D.FeatureLevel.Level_11_1, // windows 8 and up
                            SharpDX.Direct3D.FeatureLevel.Level_11_0, // windows 7 and up (level 11 was only partial on 7, not 11_1)
                            SharpDX.Direct3D.FeatureLevel.Level_10_1,
                            SharpDX.Direct3D.FeatureLevel.Level_10_0,
                            SharpDX.Direct3D.FeatureLevel.Level_9_3,
                            SharpDX.Direct3D.FeatureLevel.Level_9_2,
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.DoNotWait;
                    }
                    else if (majVers == 6 && minVers >= 2) // windows 8, windows 8.1
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_11_1, // windows 8 and up
                            SharpDX.Direct3D.FeatureLevel.Level_11_0, // windows 7 and up (level 11 was only partial on 7, not 11_1)
                            SharpDX.Direct3D.FeatureLevel.Level_10_1,
                            SharpDX.Direct3D.FeatureLevel.Level_10_0,
                            SharpDX.Direct3D.FeatureLevel.Level_9_3,
                            SharpDX.Direct3D.FeatureLevel.Level_9_2,
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.DoNotWait;
                    }
                    else if (majVers == 6 && minVers < 2) // windows 7, 2008 R2, 2008, vista
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_11_0, // windows 7 and up (level 11 was only partial on 7, not 11_1)
                            SharpDX.Direct3D.FeatureLevel.Level_10_1,
                            SharpDX.Direct3D.FeatureLevel.Level_10_0,
                            SharpDX.Direct3D.FeatureLevel.Level_9_3,
                            SharpDX.Direct3D.FeatureLevel.Level_9_2,
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.None;
                    }
                    else
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.None;
                    }

                    _factory1 = new SharpDX.DXGI.Factory1();

                    _device = new SharpDX.Direct3D11.Device(driverType, debug | DeviceCreationFlags.PreventAlteringLayerSettingsFromRegistry | DeviceCreationFlags.BgraSupport | DeviceCreationFlags.SingleThreaded, featureLevels);

                    SharpDX.DXGI.Device1 device1 = _device.QueryInterfaceOrNull<SharpDX.DXGI.Device1>();
                    if (device1 != null)
                    {
                        device1.MaximumFrameLatency = 1;
                        Utilities.Dispose(ref device1);
                        device1 = null;
                    }

                    //this code should ideally be used to prevent use of flip if vsync is 0
                    //but is not used at this time
                    //SharpDX.DXGI.Factory5 f5 = factory.QueryInterfaceOrNull<SharpDX.DXGI.Factory5>();
                    //bool bAllowTearing = false;
                    //if(f5 != null)
                    //{
                    //    int size = Marshal.SizeOf(typeof(bool));
                    //    IntPtr pBool = Marshal.AllocHGlobal(size);

                    //    f5.CheckFeatureSupport(SharpDX.DXGI.Feature.PresentAllowTearing, pBool, size);

                    //    bAllowTearing = Marshal.ReadInt32(pBool) == 1;

                    //    Marshal.FreeHGlobal(pBool);
                    //}
                    //

                    // check if the device has a factory4 interface
                    // if not, then we need to use old bitplit swapeffect
                    SwapEffect swapEffect;

                    SharpDX.DXGI.Factory4 factory4 = _factory1.QueryInterfaceOrNull<SharpDX.DXGI.Factory4>();
                    bool bFlipPresent = false;
                    if (factory4 != null)
                    {
                        /*if (!_bUseLegacyBuffers)*/ bFlipPresent = true;
                        Utilities.Dispose(ref factory4);
                        factory4 = null;
                    }

                    //https://walbourn.github.io/care-and-feeding-of-modern-swapchains/
                    swapEffect = bFlipPresent ? SwapEffect.FlipDiscard : SwapEffect.Discard; //NOTE: FlipSequential should work, but is mostly used for storeapps
                    _nBufferCount = bFlipPresent ? 2 : 1;

                    //int multiSample = 2; // eg 2 = MSAA_2, 2 times multisampling
                    //int maxQuality = _device.CheckMultisampleQualityLevels(Format.B8G8R8A8_UNorm, multiSample) - 1;
                    //maxQuality = Math.Max(0, maxQuality);

                    ModeDescription md = new ModeDescription(_displayTarget.Width, _displayTarget.Height,
                                                               new Rational(60, 1)/*console.DisplayFPS, 1)*/, Format.B8G8R8A8_UNorm);
                    md.ScanlineOrdering = DisplayModeScanlineOrder.Progressive;
                    md.Scaling = DisplayModeScaling.Centered;
                    
                    SwapChainDescription desc = new SwapChainDescription()
                    {
                        BufferCount = _nBufferCount,
                        ModeDescription = md,
                        IsWindowed = true,
                        OutputHandle = _displayTarget.Handle,
                        //SampleDescription = new SampleDescription(multiSample, maxQuality),
                        SampleDescription = new SampleDescription(1, 0), // no multi sampling (1 sample), no antialiasing
                        SwapEffect = swapEffect,
                        Usage = Usage.RenderTargetOutput,// | Usage.BackBuffer,  // dont need usage.backbuffer as it is implied
                        Flags = SwapChainFlags.None,
                    };

                    _factory1.MakeWindowAssociation(_displayTarget.Handle, WindowAssociationFlags.IgnoreAll);
                    _swapChain = new SwapChain(_factory1, _device, desc);
                    _swapChain1 = _swapChain.QueryInterface<SwapChain1>();

                    _factory = new SharpDX.Direct2D1.Factory(FactoryType.SingleThreaded, DebugLevel.None);
                    
                    _surface = _swapChain1.GetBackBuffer<Surface>(0);

                    RenderTargetProperties rtp = new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(_swapChain.Description.ModeDescription.Format, _ALPHA_MODE));                    
                    _renderTarget = new RenderTarget(_factory, _surface, rtp);

                    if (debug == DeviceCreationFlags.Debug)
                    {
                        _device.DebugName = "MeterDeviceDB_" + _rx.ToString();
                        _swapChain.DebugName = "MeterSwapChainDB_" + _rx.ToString();
                        _swapChain1.DebugName = "MeterSwapChain1DB_" + _rx.ToString();
                        _surface.DebugName = "MeterSurfaceDB_" + _rx.ToString();
                    }
                    else
                    {
                        _device.DebugName = ""; // used in shutdown
                    }

                    _bDXSetup = true;

                    setupAliasing();

                    buildDXResources();
                    buildDXFonts();
                }
                catch (Exception e)
                {
                    // issue setting up dx
                    ShutdownDX();
                    MessageBox.Show("Problem initialising Meter DirectX !" + System.Environment.NewLine + System.Environment.NewLine + "[" + e.ToString() + "]", "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            public void ShutdownDX(bool bFromRenderThread = false)
            {
                if (_displayTarget != null)
                {
                    _displayTarget.Resize -= target_Resize;
                    _displayTarget.MouseUp -= OnMouseUp;
                }

                if (!_bDXSetup) return;

                _dxDisplayThreadRunning = false;
                if (_dxRenderThread != null && _dxRenderThread.IsAlive && !bFromRenderThread) _dxRenderThread.Join();

                try
                {
                    if (_device != null && _device.ImmediateContext != null)
                    {
                        _device.ImmediateContext.ClearState();
                        _device.ImmediateContext.Flush();
                    }

                    foreach (KeyValuePair<string, SharpDX.Direct2D1.Bitmap> kvp in _images)
                    {
                        SharpDX.Direct2D1.Bitmap b = kvp.Value;
                        Utilities.Dispose(ref b);
                    }
                    _images.Clear();

                    releaseDXFonts();
                    releaseDXResources();

                    Utilities.Dispose(ref _renderTarget);
                    Utilities.Dispose(ref _swapChain1);
                    Utilities.Dispose(ref _swapChain);
                    Utilities.Dispose(ref _surface);
                    Utilities.Dispose(ref _factory);
                    Utilities.Dispose(ref _factory1);

                    _renderTarget = null;
                    _swapChain1 = null;
                    _swapChain = null;
                    _surface = null;
                    _factory = null;
                    _factory1 = null;

                    if (_device != null && _device.ImmediateContext != null)
                    {
                        SharpDX.Direct3D11.DeviceContext dc = _device.ImmediateContext;
                        Utilities.Dispose(ref dc);
                        dc = null;
                    }

                    SharpDX.Direct3D11.DeviceDebug ddb = null;
                    if (_device != null && _device.DebugName != "")
                    {
                        ddb = new SharpDX.Direct3D11.DeviceDebug(_device);
                        ddb.ReportLiveDeviceObjects(ReportingLevel.Detail);
                    }

                    if (ddb != null)
                    {
                        Utilities.Dispose(ref ddb);
                        ddb = null;
                    }
                    Utilities.Dispose(ref _device);
                    _device = null;

                    _bDXSetup = false;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Problem Shutting Down Meter DirectX !" + System.Environment.NewLine + System.Environment.NewLine + "[" + e.ToString() + "]", "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            private void dxRender()
            {
                if (!_bDXSetup) return;

                try
                {
                    _dxDisplayThreadRunning = true;
                    while (_dxDisplayThreadRunning)
                    {
                        if (_displayRunning && _displayTarget.Visible)
                        {
                            lock (_DXlock)
                            {
                                _renderTarget.BeginDraw();

                                // middle pixel align shift
                                Matrix3x2 t = _renderTarget.Transform;
                                t.TranslationVector = _pixelShift;
                                _renderTarget.Transform = t;

                                // background for entire form/area?
                                _renderTarget.Clear(_backColour);

                                SharpDX.RectangleF rect = new SharpDX.RectangleF(0, 0, targetWidth - 1f, targetHeight - 1f);
                                _renderTarget.DrawRectangle(rect, getDXBrushForColour(System.Drawing.Color.FromArgb(255, 64, 64, 64)));

                                drawMeters();                                

                                // undo the translate
                                _renderTarget.Transform = Matrix3x2.Identity;

                                _renderTarget.EndDraw();

                                // render
                                // note: the only way to have Present non block when using vsync number of blanks 0 , is to use DoNotWait
                                // however the gpu will error if it is busy doing something and the data can not be queued
                                // It will error and just ignore everything, we try present and ignore the 0x887A000A error
                                PresentFlags pf = _nVBlanks == 0 ? _NoVSYNCpresentFlag : PresentFlags.None;
                                Result r = _swapChain1.TryPresent(_nVBlanks, pf);

                                if (r != Result.Ok && r != 0x887A000A)
                                {
                                    string sMsg = "";
                                    if (r == 0x887A0001) sMsg = "Present Device Invalid Call" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";    //DXGI_ERROR_INVALID_CALL
                                    if (r == 0x887A0007) sMsg = "Present Device Reset" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";           //DXGI_ERROR_DEVICE_RESET
                                    if (r == 0x887A0005) sMsg = "Present Device Removed" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";         //DXGI_ERROR_DEVICE_REMOVED
                                    if (r == 0x88760870) sMsg = "Present Device DD3DDI Removed" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";  //D3DDDIERR_DEVICEREMOVED
                                                                                                                                                                                //if (r == 0x087A0001) sMsg = "Present Device Occluded" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";      //DXGI_STATUS_OCCLUDED
                                                                                                                                                                                //(ignored in the preceding if statement) if (r == 0x887A000A) sMsg = "Present Device Still Drawping" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]"; //DXGI_ERROR_WAS_STILL_DRAWING

                                    if (sMsg != "") throw (new Exception(sMsg));
                                }
                            }
                        }
                        if(_displayTarget.Visible)
                            Thread.Sleep(overallUpdateInterval());
                        else
                            Thread.Sleep(500); // if not visible, sleep for half second
                    }
                }
                catch (Exception e)
                {
                    ShutdownDX(true);
                    MessageBox.Show("Problem in DirectX Meter Renderer !" + System.Environment.NewLine + System.Environment.NewLine + "[ " + e.ToString() + " ]", "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            private void setupAliasing()
            {
                if (!_bDXSetup) return;

                if (_bAntiAlias)
                    _renderTarget.AntialiasMode = AntialiasMode.PerPrimitive; // this will antialias even if multisampling is off
                else
                    _renderTarget.AntialiasMode = AntialiasMode.Aliased; // this will result in non antialiased lines only if multisampling = 1

                _renderTarget.TextAntialiasMode = TextAntialiasMode.Default;
            }
            public int VerticalBlanks
            {
                get { return _nVBlanks; }
                set
                {
                    int v = value;
                    if (v < 0) v = 0;
                    if (v > 4) v = 4;
                    _nVBlanks = v;
                }
            }
            public PictureBox Target
            {
                get { return _displayTarget; }
                set
                {
                    _displayTarget = value;
                }
            }
            private int targetWidth
            {
                get { return _displayTarget.Width; }
            }
            private int targetHeight
            {
                get { return _displayTarget.Height; }
            }
            private void buildDXResources()
            {
                if (!_bDXSetup) return;

                releaseDXResources();
            }
            private void releaseDXResources()
            {
                if (!_bDXSetup) return;

                clearAllDynamicBrushes();
                clearAllDynamicTextFormats();
            }
            private void buildDXFonts()
            {
                if (!_bDXSetup) return;

                _fontFactory = new SharpDX.DirectWrite.Factory();
            }
            private void releaseDXFonts()
            {
                if (!_bDXSetup) return;

                if (_fontFactory != null) Utilities.Dispose(ref _fontFactory);

                _fontFactory = null;
            }
            private void clearAllDynamicBrushes()
            {
                if (!_bDXSetup || _DXBrushes == null) return;

                foreach (KeyValuePair<System.Drawing.Color, SharpDX.Direct2D1.Brush> kvp in _DXBrushes)
                {
                    SharpDX.Direct2D1.Brush b = kvp.Value;
                    Utilities.Dispose(ref b);
                    b = null;
                }

                _DXBrushes.Clear();
                _DXBrushes = null;
            }
            private void clearAllDynamicTextFormats()
            {
                if (!_bDXSetup || _textFormats == null) return;

                foreach (KeyValuePair<string, SharpDX.DirectWrite.TextFormat> kvp in _textFormats)
                {
                    SharpDX.DirectWrite.TextFormat tf = kvp.Value;
                    Utilities.Dispose(ref tf);
                    tf = null;
                }

                _textFormats.Clear();
                _textFormats = null;

                if (_fontFactory != null) Utilities.Dispose(ref _fontFactory);
                _fontFactory = null;
            }
            private SharpDX.Direct2D1.Brush getDXBrushForColour(System.Drawing.Color c, int replaceAlpha = -1)
            {
                if (!_bDXSetup) return null;

                if (_DXBrushes == null) _DXBrushes = new Dictionary<System.Drawing.Color, SharpDX.Direct2D1.Brush>();

                System.Drawing.Color newC;
                if (replaceAlpha >= 0 && replaceAlpha <= 255)
                    newC = System.Drawing.Color.FromArgb(replaceAlpha, c.R, c.G, c.B); // override the alpha
                else
                    newC = c;

                if (_DXBrushes.ContainsKey(newC)) return _DXBrushes[newC];

                SolidBrush sb = new SolidBrush(newC);
                SharpDX.Direct2D1.Brush b = convertBrush(sb);
                sb.Dispose();

                _DXBrushes.Add(newC, b);

                return b;
            }
            private SharpDX.Direct2D1.SolidColorBrush convertBrush(SolidBrush b)
            {
                return new SharpDX.Direct2D1.SolidColorBrush(_renderTarget, convertColour(b.Color));
            }
            private SharpDX.Color4 convertColour(System.Drawing.Color c)
            {
                return new SharpDX.Color4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            }
            private SharpDX.DirectWrite.TextFormat getDXTextFormatForFont(string sFontFamily, float emSize, FontStyle style)
            {
                if (!_bDXSetup) return null;

                if (_textFormats == null) _textFormats = new Dictionary<string, SharpDX.DirectWrite.TextFormat>();

                string sKey = sFontFamily + emSize.ToString("0.0") + style.ToString();

                if (_textFormats.ContainsKey(sKey)) return _textFormats[sKey];

                try
                {
                    SharpDX.DirectWrite.FontWeight fontWeight = SharpDX.DirectWrite.FontWeight.Regular;
                    SharpDX.DirectWrite.FontStyle fontStyle = SharpDX.DirectWrite.FontStyle.Normal;
                    if (((int)style & (int)FontStyle.Bold) == (int)FontStyle.Bold) fontWeight = SharpDX.DirectWrite.FontWeight.Bold;
                    if (((int)style & (int)FontStyle.Italic) == (int)FontStyle.Italic) fontStyle = SharpDX.DirectWrite.FontStyle.Italic;

                    SharpDX.DirectWrite.TextFormat tf = new SharpDX.DirectWrite.TextFormat(_fontFactory, sFontFamily, fontWeight, fontStyle, (emSize / 72) * _renderTarget.DotsPerInch.Width);
                    tf.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;

                    _textFormats.Add(sKey, tf);

                    return tf;                    
                }
                catch { }

                return null;
            }
            private void resizeDX()
            {
                lock (_DXlock)
                {
                    if (!_bDXSetup) return;

                    try
                    {
                        Utilities.Dispose(ref _renderTarget);
                        Utilities.Dispose(ref _surface);

                        _renderTarget = null;
                        _surface = null;

                        _device.ImmediateContext.ClearState();
                        _device.ImmediateContext.Flush();

                        _swapChain1.ResizeBuffers(_nBufferCount, targetWidth, targetHeight, _swapChain.Description.ModeDescription.Format, SwapChainFlags.None);

                        _surface = _swapChain1.GetBackBuffer<Surface>(0);

                        RenderTargetProperties rtp = new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(_swapChain.Description.ModeDescription.Format, _ALPHA_MODE));
                        _renderTarget = new RenderTarget(_factory, _surface, rtp);

                        setupAliasing();
                    }
                    catch (Exception e)
                    {
                        ShutdownDX();
                        MessageBox.Show("DirectX resizeDX() Meter failure\n" + e.Message, "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            //
            private void target_Resize(object sender, System.EventArgs e)
            {
                resizeDX();
            }
            private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
            {
                foreach (KeyValuePair<string, clsMeter> mkvp in _meters)
                {
                    clsMeter m = mkvp.Value;

                    float tw;
                    //if (targetWidth >= targetHeight)
                    //    tw = targetHeight - 0.5f;
                    //else
                    //    tw = targetWidth - 0.5f;

                    tw = targetWidth - 0.5f;
                    float rw = m.XRatio;
                    float rh = m.YRatio;

                    SharpDX.RectangleF rect = new SharpDX.RectangleF(0, 0, tw * rw, tw * rh);

                    foreach (KeyValuePair<string, clsMeterItem> mikvp in mkvp.Value.MeterItems.OrderBy(mikvp => mikvp.Value.ZOrder))
                    {
                        clsMeterItem mi = mikvp.Value;
                        if(mi.ItemType == clsMeterItem.MeterItemType.CLICKBOX)
                        {
                            clsClickBox cb = (clsClickBox)mi;

                            float x = (mi.TopLeft.X / m.XRatio) * rect.Width;
                            float y = (mi.TopLeft.Y / m.YRatio) * rect.Height;
                            float w = rect.Width * (mi.Size.Width / m.XRatio);
                            float h = rect.Height * (mi.Size.Height / m.YRatio);

                            SharpDX.RectangleF clickRect = new SharpDX.RectangleF(x, y, w, h);

                            if (clickRect.Contains(new SharpDX.Point(e.X, e.Y)))
                            {
                                m.MouseUp(e, m, cb);
                            }
                        }
                    }
                }
            }
            //
            private void drawMeters()
            {
                foreach (KeyValuePair<string, clsMeter> mkvp in _meters)
                {
                    clsMeter m = mkvp.Value;

                    float tw;
                    //if (targetWidth >= targetHeight)
                    //    tw = targetHeight - 0.5f;
                    //else
                    //    tw = targetWidth - 0.5f;

                    tw = targetWidth - 1f;
                    float rw = m.XRatio;
                    float rh = m.YRatio;

                    SharpDX.RectangleF rect = new SharpDX.RectangleF(0, 0, tw * rw, tw * rh);

                    if (m.RX == _rx)
                    {
                        // this rx
                        if (
                            ((m.TypeBitField & (int)MeterType.RX) == (int)MeterType.RX && !m.MOX) ||
                            ((m.TypeBitField & (int)MeterType.MOX) == (int)MeterType.MOX && m.MOX)
                            )
                        {
                            // display when rx only
                            _renderTarget.DrawRectangle(rect, getDXBrushForColour(System.Drawing.Color.LimeGreen), 2f);

                            //test
                            foreach(KeyValuePair<string, clsMeterItem> mikvp in mkvp.Value.MeterItems.OrderBy(mikvp => mikvp.Value.ZOrder))
                            {
                                clsMeterItem mi = mikvp.Value;
                                if ((m.DisplayGroup == 0 || mi.DisplayGroup == 0) || (mi.DisplayGroup == m.DisplayGroup))
                                {
                                    switch (mi.ItemType)
                                    {
                                        case clsMeterItem.MeterItemType.H_BAR:
                                            renderHBar(rect, mi, m);
                                            break;
                                        case clsMeterItem.MeterItemType.SOLID_COLOUR:
                                            renderSolidColour(rect, mi, m);
                                            break;
                                        case clsMeterItem.MeterItemType.NEEDLE:
                                            renderNeedle(rect, mi, m);
                                            break;
                                        case clsMeterItem.MeterItemType.IMAGE:
                                            renderImage(rect, mi, m);
                                            break;
                                        case clsMeterItem.MeterItemType.TEXT:
                                            renderText(rect, mi, m);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private float limit(float value, float min, float max)
            {
                if(value < min) value = min;
                if(value > max) value = max;
                return value;
            }
            private SizeF measureString(string sText, string sFontFamily, FontStyle style, float emSize)
            {
                if (!_bDXSetup) return SizeF.Empty;

                if (_stringMeasure == null) _stringMeasure = new Dictionary<string, SizeF>();

                emSize = (float)Math.Round(emSize, 1);

                string sKey = sText + emSize.ToString("0.0");

                if (_stringMeasure.ContainsKey(sKey)) return _stringMeasure[sKey];

                SharpDX.DirectWrite.FontWeight fontWeight = SharpDX.DirectWrite.FontWeight.Regular;
                SharpDX.DirectWrite.FontStyle fontStyle = SharpDX.DirectWrite.FontStyle.Normal;
                if (((int)style & (int)FontStyle.Bold) == (int)FontStyle.Bold) fontWeight = SharpDX.DirectWrite.FontWeight.Bold;
                if (((int)style & (int)FontStyle.Italic) == (int)FontStyle.Italic) fontStyle = SharpDX.DirectWrite.FontStyle.Italic;

                // calculate how big the string would be @ emSize pt
                SharpDX.DirectWrite.TextFormat tf = new SharpDX.DirectWrite.TextFormat(_fontFactory, sFontFamily, fontWeight, fontStyle, emSize);
                tf.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;

                SharpDX.DirectWrite.TextLayout layout = new SharpDX.DirectWrite.TextLayout(_fontFactory, sText, tf, float.MaxValue, float.MaxValue);
                float width = layout.Metrics.Width;
                float height = layout.Metrics.Height;
                Utilities.Dispose(ref layout);
                layout = null;
                Utilities.Dispose(ref tf);
                tf = null;

                SizeF size = new SizeF(width, height);
                _stringMeasure.Add(sKey, size);

                return size;
            }
            private void renderText(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsText txt = (clsText)mi;

                float x = (mi.TopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.TopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                //if (txt.ShowContainer) { 
                //SharpDX.RectangleF mirect = new SharpDX.RectangleF(x, y, w, h);
                //_renderTarget.DrawRectangle(mirect, getDXBrushForColour(System.Drawing.Color.Red));
                //}

                string sText;
                switch (txt.Text.ToLower())
                {
                    case "%group%":
                        sText = m.DisplayGroupText;
                        break;
                    case "%swr%":
                        float swr = MeterManager.getReading(_rx, Reading.SWR);
                        sText = swr.ToString("0.00");
                        break;
                    default:
                        sText = txt.Text;
                        break;
                }

                float fontSize;
                SizeF size;
                if (txt.FixedSize)
                {
                    fontSize = txt.FontSize;
                    size = measureString("0", txt.FontFamily, txt.Style, fontSize);
                }
                else
                {
                    fontSize = 72f;
                    size = measureString(sText, txt.FontFamily, txt.Style, fontSize);
                }

                float r = w / size.Width;
                float newSize = (float)Math.Round((fontSize * r) * (fontSize / _renderTarget.DotsPerInch.Width), 1);

                //if (txt.ShowContainer) { 
                //size.Width *= r;
                //size.Height *= r;
                //SharpDX.RectangleF tmp = new SharpDX.RectangleF(x, y, size.Width, size.Height);
                //_renderTarget.DrawRectangle(tmp, getDXBrushForColour(System.Drawing.Color.Green));
                //}

                SharpDX.RectangleF txtrect = new SharpDX.RectangleF(x, y, w, h);
                _renderTarget.DrawText(sText, getDXTextFormatForFont(txt.FontFamily, newSize, txt.Style), txtrect, getDXBrushForColour(txt.Colour));
            }
            private void renderHBar(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsBarItem cbi = (clsBarItem)mi;

                float x = (mi.TopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.TopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                SharpDX.RectangleF mirect = new SharpDX.RectangleF(x, y, w, h);
                _renderTarget.DrawRectangle(mirect, getDXBrushForColour(System.Drawing.Color.Red));

                PointF min, max;
                float percX, percY;

                if (cbi.ShowHistory)
                {
                    getPerc(cbi, cbi.MinHistory, out percX, out percY, out min, out max);
                    float minHistory_x = percX * w;
                    getPerc(cbi, cbi.MaxHistory, out percX, out percY, out min, out max);
                    float maxHistory_x = percX * w;

                    SharpDX.RectangleF history = new SharpDX.RectangleF(minHistory_x + x, y, maxHistory_x - minHistory_x, h);
                    _renderTarget.FillRectangle(history, getDXBrushForColour(cbi.HistoryColour));
                }

                getPerc(cbi, cbi.Value, out float pixel_x, out float pixel_y, out min, out max);
                pixel_x *= w;

                switch (cbi.Style)
                {
                    case clsBarItem.BarStyle.SolidFilled:
                        SharpDX.RectangleF barrect = new SharpDX.RectangleF(x, y, pixel_x, h);
                        _renderTarget.FillRectangle(barrect, getDXBrushForColour(cbi.Colour));
                        break;
                    case clsBarItem.BarStyle.Line:
                        _renderTarget.DrawLine(new SharpDX.Vector2(pixel_x + x, y), new SharpDX.Vector2(pixel_x + x, y + h), getDXBrushForColour(cbi.Colour), cbi.StrokeWidth);
                        break;
                }
            }
            private void renderSolidColour(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsSolidColour sc = (clsSolidColour)mi;

                float x = (mi.TopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.TopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                SharpDX.RectangleF rectSC = new SharpDX.RectangleF(x, y, w, h);
                _renderTarget.FillRectangle(rectSC, getDXBrushForColour(sc.Colour));
            }
            private void renderImage(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsImage ipg = (clsImage)mi;

                float x = (mi.TopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.TopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                string sImage = ipg.ImageName;

                string sKey = sImage + "-" + MeterManager.CurrentPowerRating.ToString();
                if (_images.ContainsKey(sKey)) sImage = sKey;

                sKey = sImage + "-small";
                if ((w < 100 || h < 100) && _images.ContainsKey(sKey)) sImage = sKey; // use small version of the image if available

                if (_images.ContainsKey(sImage))
                {
                    SharpDX.RectangleF imgRect = new SharpDX.RectangleF(x, y, w, h);

                    if (!ipg.Clipped)
                    {
                        _renderTarget.PushAxisAlignedClip(imgRect, AntialiasMode.Aliased); // prevent anything drawing from outside the rectangle, no nee to cut the image
                    }
                    else
                    {
                        float cx = (ipg.ClipTopLeft.X / m.XRatio) * rect.Width;
                        float cy = (ipg.ClipTopLeft.Y / m.YRatio) * rect.Height;
                        float cw = rect.Width * (ipg.ClipSize.Width / m.XRatio);
                        float ch = rect.Height * (ipg.ClipSize.Height / m.YRatio);

                        SharpDX.RectangleF clipRect = new SharpDX.RectangleF(cx, cy, cw, ch);
                        _renderTarget.PushAxisAlignedClip(clipRect, AntialiasMode.Aliased); // prevent anything drawing from outside the rectangle, no nee to cut the image
                    }

                    SharpDX.Direct2D1.Bitmap b = _images[sImage];

                    // maintain aspect ratio, the clip removes anything outside the rect
                    float im_w = b.Size.Width;
                    float im_h = b.Size.Height;

                    if (w > h) 
                        imgRect.Height = imgRect.Width * (im_h / im_w);
                    else
                        imgRect.Width = imgRect.Height * (im_w / im_h);

                    _renderTarget.DrawBitmap(b, imgRect, 1f, BitmapInterpolationMode.Linear);//, sourceRect);

                    _renderTarget.PopAxisAlignedClip();
                }
            }
            //private Vector _p;
            //private Vector _p2;
            //private Vector _q;
            //private Vector _q2;

            //private bool _bGotPWR = false;
            //private bool _bGotRefPWR = false;

            //private Dictionary<string, SharpDX.Vector2> _fanSWR = new Dictionary<string, Vector2>();

            private void renderNeedle(SharpDX.RectangleF rect, clsMeterItem mi, clsMeter m)
            {
                clsNeedleItem ni = (clsNeedleItem)mi;

                float x = (mi.TopLeft.X / m.XRatio) * rect.Width;
                float y = (mi.TopLeft.Y / m.YRatio) * rect.Height;
                float w = rect.Width * (mi.Size.Width / m.XRatio);
                float h = rect.Height * (mi.Size.Height / m.YRatio);

                SharpDX.RectangleF nirect = new SharpDX.RectangleF(x, y, w, h);
                //_renderTarget.DrawRectangle(nirect, getDXBrushForColour(System.Drawing.Color.Red));

                _renderTarget.PushAxisAlignedClip(nirect, AntialiasMode.Aliased); // prevent anything drawing from outside the rectangle

                //float startX = x + (w * ni.NeedleOffset.X);
                //float startY = y + (h * ni.NeedleOffset.Y);

                // needle offset from centre
                float cX = x + (w / 2);
                float cY = y + (h / 2);
                float startX = cX + (w * ni.NeedleOffset.X);
                float startY = cY + (h * ni.NeedleOffset.Y);

                float rotation = 180f;

                float radiusX = (w / 2) * (ni.LengthFactor * ni.RadiusRatio.X);
                float radiusY = (w / 2) * (ni.LengthFactor * ni.RadiusRatio.Y);

                //todo
                switch (ni.Placement)
                {
                    case clsNeedleItem.NeedlePlacement.Bottom:
                        rotation = 180f;
                        break;
                    case clsNeedleItem.NeedlePlacement.Left:
                        //rotation = 90f;
                        break;
                    case clsNeedleItem.NeedlePlacement.Top:
                        //rotation = 0f;
                        break;
                    case clsNeedleItem.NeedlePlacement.Right:
                        //rotation = 270f;
                        break;
                }

                float eX, eY, dX, dY;

                if (ni.ShowHistory || ni.Setup)
                {
                    float valueMin;
                    float valueMax;

                    if (ni.Setup) 
                    {
                        KeyValuePair<float, PointF> kvp;
                        kvp = ni.ScaleCalibration.OrderBy(p => p.Key).First();
                        valueMin = kvp.Key;
                        kvp = ni.ScaleCalibration.OrderByDescending(p => p.Key).First();
                        valueMax = kvp.Key;
                    }
                    else
                    {
                        valueMin = ni.MinHistory;
                        valueMax = ni.MaxHistory;
                    }

                    getPerc(ni, valueMin, out float minPercX, out float minPercY, out PointF minHistoryMin, out PointF minHistoryMax);
                    getPerc(ni, valueMax, out float maxPercX, out float maxPercY, out PointF maxHistoryMin, out PointF maxHistoryMax);

                    //if (ni.Direction == clsNeedleItem.NeedleDirection.CounterClockwise)
                    //{
                    //    minPercX = 1f - minPercX;
                    //    maxPercX = 1f - maxPercX;
                    //    minPercY = 1f - minPercY;
                    //    maxPercY = 1f - maxPercY;
                    //}

                    // map the meter scales to pixels
                    float eXmin = x + (minHistoryMin.X * w) + (minPercX * ((minHistoryMax.X - minHistoryMin.X) * w));
                    float eYmin = y + (minHistoryMin.Y * h) + (minPercY * ((minHistoryMax.Y - minHistoryMin.Y) * h));
                    float eXmax = x + (maxHistoryMin.X * w) + (maxPercX * ((maxHistoryMax.X - maxHistoryMin.X) * w));
                    float eYmax = y + (maxHistoryMin.Y * h) + (maxPercY * ((maxHistoryMax.Y - maxHistoryMin.Y) * h));

                    // calc angle required
                    float dXmin = startX - eXmin;
                    float dYmin = startY - eYmin;
                    float dXmax = startX - eXmax;
                    float dYmax = startY - eYmax;
                    // expand for ellipse
                    dXmin /= ni.RadiusRatio.X;
                    dYmin /= ni.RadiusRatio.Y;
                    dXmax /= ni.RadiusRatio.X;
                    dYmax /= ni.RadiusRatio.Y;
                    //
                    float angMin = (float)Math.Atan2(dYmin, dXmin);
                    float angMax = (float)Math.Atan2(dYmax, dXmax);

                    float endMinX = startX + (float)(Math.Cos(angMin + degToRad(rotation)) * radiusX);
                    float endMinY = startY + (float)(Math.Sin(angMin + degToRad(rotation)) * radiusY);
                    float endMaxX = startX + (float)(Math.Cos(angMax + degToRad(rotation)) * radiusX);
                    float endMaxY = startY + (float)(Math.Sin(angMax + degToRad(rotation)) * radiusY);

                    PathGeometry sharpGeometry = new PathGeometry(_renderTarget.Factory);

                    GeometrySink geo = sharpGeometry.Open();
                    geo.BeginFigure(new SharpDX.Vector2(startX, startY), FigureBegin.Filled);

                    geo.AddLine(new SharpDX.Vector2(endMinX, endMinY));

                    ArcSegment arcSegment = new ArcSegment();
                    arcSegment.Point = new SharpDX.Vector2(endMaxX, endMaxY);
                    arcSegment.SweepDirection = ni.Direction == clsNeedleItem.NeedleDirection.Clockwise ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;
                    arcSegment.ArcSize = Math.Abs(radToDeg(angMax) - radToDeg(angMin)) <= 180f ? ArcSize.Small : ArcSize.Large;
                    arcSegment.Size = new Size2F(radiusX, radiusY);
                    geo.AddArc(arcSegment);

                    geo.EndFigure(FigureEnd.Closed); // adds the closing line
                    geo.Close();

                    _renderTarget.FillGeometry(sharpGeometry, getDXBrushForColour(ni.HistoryColour));

                    Utilities.Dispose(ref geo);
                    geo = null;
                    Utilities.Dispose(ref sharpGeometry);
                    sharpGeometry = null;
                }

                getPerc(ni, ni.Value, out float percX, out float percY, out PointF min, out PointF max);
                //if (ni.Direction == clsNeedleItem.NeedleDirection.CounterClockwise)
                //{
                //    percX = 1f - percX;
                //    percY = 1f - percY;
                //}

                // map the meter scales to pixels
                eX = x + (min.X * w) + (percX * ((max.X - min.X) * w));
                eY = y + (min.Y * h) + (percY * ((max.Y - min.Y) * h));

                // calc angle required
                dX = startX - eX;
                dY = startY - eY;
                // expand
                dX /= ni.RadiusRatio.X;
                dY /= ni.RadiusRatio.Y;
                float ang = (float)Math.Atan2(dY, dX);

                float endX = startX + (float)(Math.Cos(ang + degToRad(rotation)) * radiusX);
                float endY = startY + (float)(Math.Sin(ang + degToRad(rotation)) * radiusY);

                _renderTarget.DrawLine(new SharpDX.Vector2(startX, startY), new SharpDX.Vector2(endX, endY), getDXBrushForColour(ni.Colour), ni.StrokeWidth);

                if (ni.Setup)
                {
                    foreach (KeyValuePair<float, PointF> kvp in ni.ScaleCalibration)
                    {
                        PointF p = kvp.Value;
                        _renderTarget.FillEllipse(new Ellipse(new SharpDX.Vector2(x + (p.X * w), y + (p.Y * h)), 2f, 2f), getDXBrushForColour(System.Drawing.Color.Red));
                    }
                }
                //if (ni.ReadingSource == Reading.PWR)
                //{
                //    _p = new Vector(startX, startY);
                //    _p2 = new Vector(endX, endY);
                //    _bGotPWR = true;
                //}
                //if(ni.ReadingSource == Reading.REVERSE_PWR)
                //{
                //    _q = new Vector(startX, startY);
                //    _q2 = new Vector(endX, endY);
                //    _bGotRefPWR = true;
                    
                //}
                //if(_bGotPWR && _bGotRefPWR)
                //{
                //    bool b = lineSegementsIntersect(_p, _p2, _q, _q2, out Vector intersect);

                //    if (b)
                //    {
                //        float xx = (float)(_p.X + (_q.X - _p.X)/2f);
                //        float yy = (float)(_p.Y + (_q.Y - _p.Y)/2f);

                //        string sKey = MeterManager.getReading(_rx, Reading.REVERSE_PWR).ToString("0.000");
                //        if (!_fanSWR.ContainsKey(sKey))
                //            _fanSWR.Add(sKey, new SharpDX.Vector2((float)intersect.X, (float)intersect.Y));

                //        foreach(KeyValuePair<string, SharpDX.Vector2> pair in _fanSWR)
                //        {
                //            _renderTarget.DrawLine(new SharpDX.Vector2(xx, yy), new SharpDX.Vector2(pair.Value.X, pair.Value.Y), getDXBrushForColour(System.Drawing.Color.Red), ni.StrokeWidth);
                //        }
                //    }

                //    _bGotPWR = false;
                //    _bGotRefPWR = false;
                //}

                _renderTarget.PopAxisAlignedClip();
            }
            private double degToRad(float deg)
            {
                return (deg * Math.PI) / 180f;
            }
            private double radToDeg(float rad)
            {
                return rad * (180f / Math.PI);
            }
            private void getPerc(clsMeterItem mi, float value, out float percX, out float percY, out PointF min, out PointF max)
            {
                percX = 0;
                percY = 0;
                min = new PointF(0f, 0f);
                max = new PointF(0f, 0f);

                value = (float)Math.Round(value, 3);

                // adjust for >= 30mhz
                if (mi.ReadingSource == Reading.SIGNAL_STRENGTH || mi.ReadingSource == Reading.AVG_SIGNAL_STRENGTH)
                    value += MeterManager.dbmOffsetForAbove30(_rx);

                if (mi.ScaleCalibration.Count > 0)
                {
                    // todo, only implemented for clockwise needles atm
                    PointF p_low = PointF.Empty;
                    PointF p_high = PointF.Empty;
                    float value_low = -200f;
                    float value_high = -200f;
                    bool bEqual = false;

                    if (mi.ScaleCalibration.ContainsKey(value))
                    {
                        p_low = mi.ScaleCalibration[value];
                        p_high = mi.ScaleCalibration[value];
                        value_low = value;
                        value_high = value;
                        bEqual = true;
                    }
                    else if (value < mi.ScaleCalibration.OrderBy(p => p.Key).First().Key) // below lowest
                    {
                        p_low = p_high = mi.ScaleCalibration.OrderBy(p => p.Key).First().Value;
                        value_low = value_high = mi.ScaleCalibration.OrderBy(p => p.Key).First().Key;
                        bEqual = true;
                    }
                    else if (value > mi.ScaleCalibration.OrderBy(p => p.Key).Last().Key) // above highest
                    {
                        p_low = p_high = mi.ScaleCalibration.OrderBy(p => p.Key).Last().Value;
                        value_low = value_high = mi.ScaleCalibration.OrderBy(p => p.Key).Last().Key;
                        bEqual = true;
                    }
                    else
                    {
                        //low side
                        foreach (KeyValuePair<float, PointF> kvp in mi.ScaleCalibration.OrderByDescending(p => p.Key))
                        {
                            if (kvp.Key <= value)
                            {
                                p_low = kvp.Value;
                                value_low = kvp.Key;
                                break;
                            }
                        }

                        // high
                        foreach (KeyValuePair<float, PointF> kvp in mi.ScaleCalibration.OrderBy(p => p.Key))
                        {
                            if (kvp.Key >= value)
                            {
                                p_high = kvp.Value;
                                value_high = kvp.Key;
                                break;
                            }
                        }
                    }
                    //get mins/maxs
                    float minX = (float)Math.Round(mi.ScaleCalibration.OrderBy(p => p.Value.X).First().Value.X, 3);
                    float maxX = (float)Math.Round(mi.ScaleCalibration.OrderByDescending(p => p.Value.X).First().Value.X, 3);

                    float minY = (float)Math.Round(mi.ScaleCalibration.OrderBy(p => p.Value.Y).First().Value.Y, 3);
                    float maxY = (float)Math.Round(mi.ScaleCalibration.OrderByDescending(p => p.Value.Y).First().Value.Y, 3);

                    float rangeX = maxX - minX;
                    float rangeY = maxY - minY;

                    if (bEqual)
                    {
                        percX = (p_low.X - minX) / rangeX;
                        percY = (p_low.Y - minY) / rangeY;
                    }
                    else
                    {
                        float value_range = value_high - value_low;
                        float ratio = (value - value_low) / value_range;
                        float newRangeX = p_high.X - p_low.X;
                        float newRangeY = p_high.Y - p_low.Y;

                        percX = ((p_low.X - minX) + (newRangeX * ratio)) / rangeX;
                        percY = ((p_low.Y - minY) + (newRangeY * ratio)) / rangeY;
                    }

                    percX = (float)Math.Round(percX, 3);
                    percY = (float)Math.Round(percY, 3);
                    min = new PointF(minX, minY);
                    max = new PointF(maxX, maxY);
                }
            }
            private SharpDX.Direct2D1.Bitmap bitmapFromSysBitmap(RenderTarget rt, System.Drawing.Bitmap bitmap)
            {
                System.Drawing.Rectangle sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                BitmapProperties bitmapProperties = new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, _ALPHA_MODE)); //was R8G8B8A8_UNorm  //MW0LGE_21k9
                Size2 size = new Size2(bitmap.Width, bitmap.Height);

                // Transform pixels from BGRA to RGBA
                int stride = bitmap.Width * sizeof(int);
                DataStream tempStream = new DataStream(bitmap.Height * stride, true, true);

                // Lock System.Drawing.Bitmap
                System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                // Convert all pixels 
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int offset = bitmapData.Stride * y;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        //int rgba = R | (G << 8) | (B << 16) | (A << 24); //MW0LGE_21k9
                        //tempStream.Write(rgba);
                        int bgra = B | (G << 8) | (R << 16) | (A << 24);
                        tempStream.Write(bgra);
                    }

                }
                bitmap.UnlockBits(bitmapData);

                tempStream.Position = 0;

                SharpDX.Direct2D1.Bitmap dxBitmap = new SharpDX.Direct2D1.Bitmap(rt, size, tempStream, stride, bitmapProperties);

                Utilities.Dispose(ref tempStream);
                tempStream = null;

                return dxBitmap;
            }
            //private class Vector
            //{
            //    public double X;
            //    public double Y;

            //    // Constructors.
            //    public Vector(double x, double y) { X = x; Y = y; }
            //    public Vector() : this(double.NaN, double.NaN) { }

            //    public static Vector operator -(Vector v, Vector w)
            //    {
            //        return new Vector(v.X - w.X, v.Y - w.Y);
            //    }

            //    public static Vector operator +(Vector v, Vector w)
            //    {
            //        return new Vector(v.X + w.X, v.Y + w.Y);
            //    }

            //    public static double operator *(Vector v, Vector w)
            //    {
            //        return v.X * w.X + v.Y * w.Y;
            //    }

            //    public static Vector operator *(Vector v, double mult)
            //    {
            //        return new Vector(v.X * mult, v.Y * mult);
            //    }

            //    public static Vector operator *(double mult, Vector v)
            //    {
            //        return new Vector(v.X * mult, v.Y * mult);
            //    }

            //    public double Cross(Vector v)
            //    {
            //        return X * v.Y - Y * v.X;
            //    }

            //    public override bool Equals(object obj)
            //    {
            //        var v = (Vector)obj;
            //        return (X - v.X).IsZero() && (Y - v.Y).IsZero();
            //    }
            //}
            ////https://www.codeproject.com/Tips/862988/Find-the-Intersection-Point-of-Two-Line-Segments
            //private bool lineSegementsIntersect(Vector p, Vector p2, Vector q, Vector q2, out Vector intersection, bool considerCollinearOverlapAsIntersect = false)
            //{
            //    intersection = new Vector();

            //    Vector r = p2 - p;
            //    Vector s = q2 - q;
            //    double rxs = r.Cross(s);
            //    double qpxr = (q - p).Cross(r);

            //    // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            //    if (rxs.IsZero() && qpxr.IsZero())
            //    {
            //        // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
            //        // then the two lines are overlapping,
            //        if (considerCollinearOverlapAsIntersect)
            //            if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s))
            //                return true;

            //        // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
            //        // then the two lines are collinear but disjoint.
            //        // No need to implement this expression, as it follows from the expression above.
            //        return false;
            //    }

            //    // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            //    if (rxs.IsZero() && !qpxr.IsZero())
            //        return false;

            //    // t = (q - p) x s / (r x s)
            //    double t = (q - p).Cross(s) / rxs;

            //    // u = (q - p) x r / (r x s)

            //    double u = (q - p).Cross(r) / rxs;

            //    // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            //    // the two line segments meet at the point p + t r = q + u s.
            //    if (!rxs.IsZero() && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            //    {
            //        // We can calculate the intersection point using either t or u.
            //        intersection = p + t * r;

            //        // An intersection was found.
            //        return true;
            //    }

            //    // 5. Otherwise, the two line segments are not parallel but do not intersect.
            //    return false;
            //}
        }        
        #endregion
    }
}
