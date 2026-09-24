//=================================================================
// CIVController.cs
//=================================================================
// Controller for native Icom CI-V serial communication in Thetis.
// Supports IC-7100 VFO A/B, PTT, Modulation Modes, IF Filter Width,
// Split Mode, and Full Duplex TX Frequency steering.
//=================================================================

using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Thetis
{
    public class CIVController : IDisposable
    {
        #region Fields & State

        private readonly Console _console;
        private SerialPort _serialPort;
        private readonly object _portLock = new object();
        private readonly object _stateLock = new object();

        // Radio Addresses
        private byte _radioAddr = CIVProtocol.DEFAULT_RADIO_ADDR;
        private byte _hostAddr = CIVProtocol.DEFAULT_HOST_ADDR;

        // Port Parameters
        private string _portName = "COM10";
        private int _baudRate = 19200;

        // Features Configuration
        private bool _transceiveEnabled = true;
        private bool _syncSplitAndFullDuplex = true;
        private bool _syncFilterWidth = true;
        private bool _syncPTT = false;

        // Flood Control / Rate Limiting
        private Timer _floodTimer;
        private const int FLOOD_INTERVAL_MS = 50; // 20 Hz update rate for VFO tuning
        private bool _freqChangePending = false;
        private bool _vfoBChangePending = false;
        private bool _modeChangePending = false;
        private bool _splitChangePending = false;

        // Pending values to transmit
        private double _pendingVfoAFreq = 0.0;
        private double _pendingVfoBFreq = 0.0;
        private double _pendingTxFreq = 0.0;
        private DSPMode _pendingMode = DSPMode.USB;
        private int _pendingFilterWidth = 2700;
        private bool _pendingSplit = false;

        // Last confirmed sent states (Echo / Ping-Pong prevention)
        private double _lastSentVfoAFreq = -1.0;
        private double _lastSentVfoBFreq = -1.0;
        private CIVMode _lastSentCivMode = (CIVMode)0xFF;
        private CIVFilter _lastSentCivFilter = (CIVFilter)0xFF;
        private CIVDataMode _lastSentDataMode = (CIVDataMode)0xFF;
        private byte _lastSentFilterWidthCode = 0xFF;
        private bool _lastSentPtt = false;
        private bool _lastSentSplit = false;
        private bool _actualRadioSplit = false;
        private int _pttPollCounter = 0;
        private byte _currentRadioSelectedVfo = CIVProtocol.VFO_A;
        private long _lastTxReleaseTime = 0;
        private volatile bool _isSwappingVfo = false;
        private volatile bool _radioInitiatedSwapInProgress = false;
        private volatile bool _rx2SplitSwapHandled = false;   // set by CMD_SPLIT when it handles the A/B tap in RX2+SPLIT mode
        private long _lastVfoSwapTime = 0;
        private long _lastVfoBTuneTime = 0;
        private readonly object _vfoSwapLock = new object();

        // VFO B read-from-radio infrastructure (used when IC-7100 initiates split)
        private volatile bool _readingRadioVfoBFreq = false;
        private double _capturedVfoBFreq = 0.0;
        private readonly object _vfoBReadLock = new object();

        // Loop suppression when Thetis is being updated from the radio
        private bool _suppressOutgoingUpdates = false;

        // Serial reception buffer
        private readonly List<byte> _rxBuffer = new List<byte>(512);

        // Echo history for filtering out reflected TX frames on 1-wire buses
        private readonly Queue<byte[]> _recentSentFrames = new Queue<byte[]>();
        private readonly object _echoLock = new object();

        private bool _isDisposed = false;

        #endregion

        #region Debug Logging

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string format, params object[] args)
        {
            try
            {
                string text = (args != null && args.Length > 0) ? string.Format(format, args) : format;
                System.Diagnostics.Debug.WriteLine(string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}] [CIV] {1}", DateTime.Now, text));
            }
            catch
            {
                // Never crash caller
            }
        }

        public static string HexDump(byte[] bytes)
        {
            if (bytes == null) return "<null>";
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        #endregion

        #region Properties

        public string PortName
        {
            get { return _portName; }
            set { _portName = value; }
        }

        public int BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        public byte RadioAddress
        {
            get { return _radioAddr; }
            set { _radioAddr = value; }
        }

        public byte HostAddress
        {
            get { return _hostAddr; }
            set { _hostAddr = value; }
        }

        public bool TransceiveEnabled
        {
            get { return _transceiveEnabled; }
            set { _transceiveEnabled = value; }
        }

        public bool SyncSplitAndFullDuplex
        {
            get { return _syncSplitAndFullDuplex; }
            set { _syncSplitAndFullDuplex = value; }
        }

        public bool SyncFilterWidth
        {
            get { return _syncFilterWidth; }
            set { _syncFilterWidth = value; }
        }

        public bool SyncPTT
        {
            get { return _syncPTT; }
            set { _syncPTT = value; }
        }

        public bool IsOpen
        {
            get
            {
                lock (_portLock)
                {
                    return _serialPort != null && _serialPort.IsOpen;
                }
            }
        }

        #endregion

        #region Constructor

        public CIVController(Console console)
        {
            if (console == null) throw new ArgumentNullException("console");
            _console = console;
            _floodTimer = new Timer(OnFloodTimerTick, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region Connection Management

        public bool Start(string portName, int baudRate = 19200, byte radioAddr = CIVProtocol.DEFAULT_RADIO_ADDR, byte hostAddr = CIVProtocol.DEFAULT_HOST_ADDR, bool transceive = true, bool syncSplit = true, bool syncPTT = false)
        {
            _portName = portName;
            _baudRate = baudRate;
            _radioAddr = radioAddr;
            _hostAddr = hostAddr;
            _transceiveEnabled = transceive;
            _syncSplitAndFullDuplex = syncSplit;
            _syncPTT = syncPTT;

            lock (_portLock)
            {
                try
                {
                    Stop();

                    _serialPort = new SerialPort(_portName, _baudRate, Parity.None, 8, StopBits.One)
                    {
                        Handshake = Handshake.None,
                        DtrEnable = true,
                        RtsEnable = true,
                        ReadTimeout = 500,
                        WriteTimeout = 500
                    };

                    _serialPort.DataReceived += SerialPort_DataReceived;
                    _serialPort.Open();

                    _rxBuffer.Clear();

                    // Start flood control timer
                    _floodTimer.Change(FLOOD_INTERVAL_MS, FLOOD_INTERVAL_MS);

                    // Subscribe to Thetis Console events
                    SubscribeToThetisEvents();

                    // Perform initial sync of current state to IC-7100
                    SyncCurrentThetisState();

                    Log("================================================================================");
                    Log("=== CI-V Controller Started on {0} at {1} baud (RadioAddr=0x{2:X2}, HostAddr=0x{3:X2}, Transceive={4}) ===",
                        _portName, _baudRate, _radioAddr, _hostAddr, _transceiveEnabled);
                    Log("=== Initial State: RX2={0}, Split={1}, VFOBTX={2}, VFOATX={3}, VFOAFreq={4:F6}, VFOBFreq={5:F6} ===",
                        _console != null && _console.RX2Enabled,
                        _console != null && _console.VFOSplit,
                        _console != null && _console.VFOBTX,
                        _console != null && _console.VFOATX,
                        _console != null ? _console.VFOAFreq : 0,
                        _console != null ? _console.VFOBFreq : 0);
                    Log("================================================================================");

                    return true;
                }
                catch (Exception ex)
                {
                    Log("[START:ERROR] Error opening {0}: {1}", _portName, ex.Message);
                    Debug.WriteLine(string.Format("[CIVController] Error opening {0}: {1}", _portName, ex.Message));
                    Stop();
                    return false;
                }
            }
        }

        public void Stop()
        {
            Log("=== CI-V Controller Stopped ===");
            lock (_portLock)
            {
                UnsubscribeFromThetisEvents();

                if (_floodTimer != null)
                {
                    _floodTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }

                if (_serialPort != null)
                {
                    try
                    {
                        if (_serialPort.IsOpen)
                        {
                            // Ensure PTT is released before closing port
                            byte[] rxPttFrame = CIVProtocol.SetPttFrame(_radioAddr, _hostAddr, false);
                            _serialPort.Write(rxPttFrame, 0, rxPttFrame.Length);
                            _serialPort.Close();
                        }
                        _serialPort.DataReceived -= SerialPort_DataReceived;
                        _serialPort.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(string.Format("[CIVController] Error closing serial port: {0}", ex.Message));
                    }
                    _serialPort = null;
                }

                _rxBuffer.Clear();
                lock (_echoLock)
                {
                    _recentSentFrames.Clear();
                }
            }
        }

        public bool Start(string portName, int baudRate, byte radioAddr, bool transceive, bool syncSplit, byte hostAddr = CIVProtocol.DEFAULT_HOST_ADDR)
        {
            return Start(portName, baudRate, radioAddr, hostAddr, transceive, syncSplit, _syncPTT);
        }

        public bool Start(string portName, int baudRate, byte radioAddr, bool transceive, bool syncSplit, bool syncPTT, byte hostAddr = CIVProtocol.DEFAULT_HOST_ADDR)
        {
            return Start(portName, baudRate, radioAddr, hostAddr, transceive, syncSplit, syncPTT);
        }

        public bool Open(string portName, int baudRate = 19200, byte radioAddr = CIVProtocol.DEFAULT_RADIO_ADDR, byte hostAddr = CIVProtocol.DEFAULT_HOST_ADDR, bool transceive = true, bool syncSplit = true, bool syncPTT = false)
        {
            return Start(portName, baudRate, radioAddr, hostAddr, transceive, syncSplit, syncPTT);
        }

        public bool Open(string portName, int baudRate, byte radioAddr, bool transceive, bool syncSplit, byte hostAddr = CIVProtocol.DEFAULT_HOST_ADDR)
        {
            return Start(portName, baudRate, radioAddr, hostAddr, transceive, syncSplit, _syncPTT);
        }

        public bool Open(string portName, int baudRate, byte radioAddr, bool transceive, bool syncSplit, bool syncPTT, byte hostAddr = CIVProtocol.DEFAULT_HOST_ADDR)
        {
            return Start(portName, baudRate, radioAddr, hostAddr, transceive, syncSplit, syncPTT);
        }

        public void Close()
        {
            Stop();
        }

        #endregion

        #region Thetis Event Subscriptions

        private bool _eventsSubscribed = false;

        private void SubscribeToThetisEvents()
        {
            if (_eventsSubscribed || _console == null) return;

            _console.VFOAFrequencyChangeHandlers += OnVFOAFrequencyChanged;
            _console.VFOBFrequencyChangeHandlers += OnVFOBFrequencyChanged;
            _console.VFOASubFrequencyChangeHandlers += OnVFOASubFrequencyChanged;
            _console.TXFrequncyChangedHandlers += OnTXFrequencyChanged;
            _console.MoxChangeHandlers += OnMoxChanged;
            _console.SplitChangedHandlers += OnSplitChanged;
            _console.VFOTXChangedHandlers += OnVFOTXChanged;
            _console.RX2EnabledChangedHandlers += OnRX2EnabledChanged;
            _console.ModeChangeHandlers += OnModeChanged;
            _console.FilterEdgesChangedHandlers += OnFilterEdgesChanged;
            _console.SetBandChangeHanders += OnSetBandChanged;

            _eventsSubscribed = true;
        }

        private void UnsubscribeFromThetisEvents()
        {
            if (!_eventsSubscribed || _console == null) return;

            _console.VFOAFrequencyChangeHandlers -= OnVFOAFrequencyChanged;
            _console.VFOBFrequencyChangeHandlers -= OnVFOBFrequencyChanged;
            _console.VFOASubFrequencyChangeHandlers -= OnVFOASubFrequencyChanged;
            _console.TXFrequncyChangedHandlers -= OnTXFrequencyChanged;
            _console.MoxChangeHandlers -= OnMoxChanged;
            _console.SplitChangedHandlers -= OnSplitChanged;
            _console.VFOTXChangedHandlers -= OnVFOTXChanged;
            _console.RX2EnabledChangedHandlers -= OnRX2EnabledChanged;
            _console.ModeChangeHandlers -= OnModeChanged;
            _console.FilterEdgesChangedHandlers -= OnFilterEdgesChanged;
            _console.SetBandChangeHanders -= OnSetBandChanged;

            _eventsSubscribed = false;
        }

        #endregion

        #region State Sync & Event Handlers

        private bool IsSplitRequired()
        {
            if (!_syncSplitAndFullDuplex || _console == null) return false;

            return _console.VFOSplit || 
                   _console.FullDuplex || 
                   _console.VFOBTX;
        }

        public void SyncCurrentThetisState()
        {
            if (!IsOpen || _console == null) return;

            lock (_vfoSwapLock)
            {
                _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));

                lock (_stateLock)
                {
                    _pendingVfoAFreq = _console.VFOAFreq;
                    bool rx2Split = _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX;
                    bool vfoBTxSplit = _console.VFOBTX && !_console.VFOSplit && !_console.FullDuplex;
                    if (rx2Split)
                    {
                        double txFreq = _console.VFOASubFreq > 0 ? _console.VFOASubFreq : _console.TXFreq;
                        _pendingTxFreq = txFreq;
                        _pendingVfoBFreq = txFreq;
                    }
                    else if (IsSplitRequired() && vfoBTxSplit)
                    {
                        _pendingTxFreq = _console.VFOBFreq;
                        _pendingVfoBFreq = _console.VFOBFreq;
                    }
                    else if (IsSplitRequired())
                    {
                        _pendingTxFreq = _console.TXFreq;
                        _pendingVfoBFreq = _console.TXFreq;
                    }
                    else
                    {
                        _pendingTxFreq = _console.TXFreq;
                        _pendingVfoBFreq = _console.VFOBFreq;
                    }

                    _pendingMode = _console.RX1DSPMode;
                    _pendingFilterWidth = Math.Abs(_console.RX1FilterHigh - _console.RX1FilterLow);
                    bool splitRequired = IsSplitRequired();
                    _pendingSplit = splitRequired;

                    _freqChangePending = true;
                    _vfoBChangePending = true;
                    _modeChangePending = true;
                    _splitChangePending = true;
                }
            }
        }

        private void OnVFOAFrequencyChanged(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
        {
            if (_suppressOutgoingUpdates || !IsOpen) return;

            lock (_stateLock)
            {
                _pendingVfoAFreq = newFreq;
                _freqChangePending = true;

                if (!IsSplitRequired())
                {
                    _pendingTxFreq = newFreq;
                }

                if (oldMode != newMode || oldFilter != newFilter)
                {
                    _pendingMode = newMode;
                    _pendingFilterWidth = Math.Abs(_console.RX1FilterHigh - _console.RX1FilterLow);
                    _modeChangePending = true;
                }
            }
        }

        private void OnVFOBFrequencyChanged(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
        {
            if (_suppressOutgoingUpdates || !IsOpen) return;

            lock (_stateLock)
            {
                // In RX2+SPLIT special mode, Thetis VFO B is RX2 and does not correspond to IC-7100 VFO B.
                // IC-7100 VFO B holds the VFO A sub-frequency (TX).
                // Under no circumstances should Thetis VFO B updates overwrite IC-7100 VFO B or _pendingTxFreq!
                if (_console != null && _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX)
                {
                    return;
                }

                _pendingVfoBFreq = newFreq;
                _vfoBChangePending = true;
                _lastVfoBTuneTime = Stopwatch.GetTimestamp();

                // Track VFO B as the candidate TX freq when VFOBTX is active or when RX2 is enabled without VFOSplit
                // (e.g. WSJT-X setting VFO B before PTT in simplex/split without Thetis VFOSplit).
                if (_console != null && (_console.VFOBTX || (_console.RX2Enabled && !_console.VFOSplit)))
                {
                    _pendingTxFreq = newFreq;
                }
            }
        }

        private void OnVFOASubFrequencyChanged(Band oldBand, Band newBand, DSPMode newMode, Filter newFilter, double oldFreq, double newFreq, double newCentreF, bool newCTUN, int newZoomSlider, double offset, int rx)
        {
            if (_suppressOutgoingUpdates || !IsOpen || _console == null) return;

            lock (_stateLock)
            {
                double txFreq = (_console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX) ? newFreq : _console.TXFreq;
                _pendingTxFreq = txFreq;
                if (IsSplitRequired())
                {
                    _pendingVfoBFreq = txFreq;
                    _vfoBChangePending = true;
                    _lastVfoBTuneTime = Stopwatch.GetTimestamp();
                }
            }
        }

        private void OnTXFrequencyChanged(double old_frequency, double new_frequency, Band old_band, Band new_band, bool rx2_enabled, bool tx_vfob, double centre_freq)
        {
            if (_suppressOutgoingUpdates || !IsOpen) return;

            lock (_stateLock)
            {
                _pendingTxFreq = new_frequency;
                if (IsSplitRequired())
                {
                    _pendingVfoBFreq = new_frequency;
                    _vfoBChangePending = true;
                    _lastVfoBTuneTime = Stopwatch.GetTimestamp();
                }
            }
        }

        private void OnMoxChanged(int rx, bool oldMox, bool newMox)
        {
            if (_suppressOutgoingUpdates || !IsOpen) return;

            if (!newMox && oldMox)
            {
                _lastTxReleaseTime = Stopwatch.GetTimestamp();
            }

            // PTT changes bypass the flood control timer and are transmitted immediately
            SendImmediatePtt(newMox);
        }

        public void NotifySplitOrFullDuplexChanged()
        {
            Log("[NotifySplitChanged] called. suppress={0}, IsOpen={1}, RX2={2}, Split={3}, VFOBTX={4}, FullDuplex={5}",
                _suppressOutgoingUpdates, IsOpen,
                _console != null && _console.RX2Enabled,
                _console != null && _console.VFOSplit,
                _console != null && _console.VFOBTX,
                _console != null && _console.FullDuplex);

            if (_suppressOutgoingUpdates || !IsOpen || _console == null) return;

            lock (_stateLock)
            {
                _pendingVfoAFreq = _console.VFOAFreq;
                bool splitRequired = IsSplitRequired();
                _pendingSplit = splitRequired;
                _splitChangePending = true;
                _vfoBChangePending = true;
                _lastVfoBTuneTime = 0; // Force immediate update on split mode change

                // Determine the correct TX / VFO B frequency for IC-7100:
                //   • RX2+SPLIT special mode (VFOSplit && !VFOBTX): the IC-7100 VFO B must hold
                //     the VFO A sub-frequency (TXFreq), NOT Thetis VFO B which is a second receiver.
                //   • VFOBTX-only split (RX2/WSJT-X, no VFOSplit): TXFreq == VFOAFreq in simplex,
                //     so use VFOBFreq as the TX frequency.
                //   • Normal split / all other cases: use TXFreq.
                bool rx2SplitMode = _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX;
                if (rx2SplitMode)
                {
                    double txFreq = _console.VFOASubFreq > 0 ? _console.VFOASubFreq : _console.TXFreq;
                    if (txFreq <= 0 || txFreq == _console.VFOAFreq)
                    {
                        if (_pendingTxFreq > 0 && _pendingTxFreq != _console.VFOAFreq)
                            txFreq = _pendingTxFreq;
                    }
                    _pendingTxFreq = txFreq;
                    _pendingVfoBFreq = txFreq;
                }
                else if (splitRequired && _console.VFOBTX && !_console.VFOSplit && !_console.FullDuplex)
                {
                    _pendingTxFreq = _console.VFOBFreq;
                    _pendingVfoBFreq = _console.VFOBFreq;
                }
                else if (splitRequired)
                {
                    _pendingTxFreq = _console.TXFreq;
                    _pendingVfoBFreq = _console.TXFreq;
                }
                else
                {
                    _pendingTxFreq = _console.TXFreq;
                    _pendingVfoBFreq = _console.VFOBFreq;
                }
            }

            try
            {
                _floodTimer?.Change(0, FLOOD_INTERVAL_MS);
            }
            catch { }
        }

        public void NotifyVFOAtoB()
        {
            if (_radioInitiatedSwapInProgress || !IsOpen || _console == null) return;

            lock (_vfoSwapLock)
            {
                _isSwappingVfo = true;
                _lastVfoSwapTime = Stopwatch.GetTimestamp();

                try
                {
                    // RX2+SPLIT special mode: IC-7100 VFO B holds the TX sub-frequency of VFO A;
                    // Thetis VFO B is irrelevant to the radio. Only push VFO A to IC-7100 VFO A
                    // and skip the equalize command that would overwrite the sub-frequency.
                    if (_console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX)
                    {
                        double rxFreq = _console.VFOAFreq;
                        if (_currentRadioSelectedVfo != CIVProtocol.VFO_A)
                        {
                            SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                            _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                            Thread.Sleep(40);
                        }
                        if (rxFreq > 0)
                        {
                            SendFrame(CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, rxFreq));
                            lock (_stateLock)
                            {
                                _lastSentVfoAFreq = rxFreq;
                                _pendingVfoAFreq = rxFreq;
                                _freqChangePending = false;
                                // Tell flood timer IC-7100 VFO B is already correct (sub-freq intact);
                                // do NOT let it push _pendingVfoBFreq (now set to new Thetis VFO B)
                                // which would overwrite the radio's sub-frequency.
                                _pendingVfoBFreq = _lastSentVfoBFreq;
                                _vfoBChangePending = false;
                            }
                        }
                        return;
                    }

                    // Ensure radio is on VFO A so active VFO A is copied to inactive VFO B
                    if (_currentRadioSelectedVfo != CIVProtocol.VFO_A)
                    {
                        SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                        _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                        Thread.Sleep(40);
                    }

                    // CI-V 0x07 0xA0: Equalize VFO A -> VFO B
                    SendFrame(CIVProtocol.EqualVfoFrame(_radioAddr, _hostAddr));

                    lock (_stateLock)
                    {
                        _lastSentVfoBFreq = _console.VFOAFreq;
                        _pendingVfoBFreq = _console.VFOAFreq;
                        if (_console.VFOSplit || _console.VFOBTX)
                        {
                            _pendingTxFreq = _console.VFOAFreq;
                        }
                        _vfoBChangePending = false;
                    }
                }
                finally
                {
                    _lastVfoSwapTime = Stopwatch.GetTimestamp();
                    _isSwappingVfo = false;
                }
            }
        }

        public void NotifyVFOBtoA()
        {
            if (_radioInitiatedSwapInProgress || !IsOpen || _console == null) return;

            lock (_vfoSwapLock)
            {
                _isSwappingVfo = true;
                _lastVfoSwapTime = Stopwatch.GetTimestamp();

                try
                {
                    // Copies Thetis VFO B freq to IC-7100 VFO A (active VFO).
                    // In RX2+SPLIT special mode (!VFOBTX) this still works correctly because
                    // we only ever touch VFO A on the radio; IC-7100 VFO B (the sub-frequency)
                    // is not disturbed here.

                    // Ensure radio is on VFO A
                    if (_currentRadioSelectedVfo != CIVProtocol.VFO_A)
                    {
                        SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                        _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                        Thread.Sleep(40);
                    }

                    double freq = _console.VFOBFreq;
                    if (freq > 0)
                    {
                        SendFrame(CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, freq));
                        lock (_stateLock)
                        {
                            _lastSentVfoAFreq = freq;
                            _pendingVfoAFreq = freq;
                            _freqChangePending = false;
                        }
                    }
                }
                finally
                {
                    _lastVfoSwapTime = Stopwatch.GetTimestamp();
                    _isSwappingVfo = false;
                }
            }
        }

        public void NotifyVFOSwap()
        {
            Log("[NotifyVFOSwap] called. swapInProgress={0}, IsOpen={1}, RX2={2}, Split={3}, VFOBTX={4}",
                _radioInitiatedSwapInProgress, IsOpen,
                _console != null && _console.RX2Enabled,
                _console != null && _console.VFOSplit,
                _console != null && _console.VFOBTX);

            if (_radioInitiatedSwapInProgress || !IsOpen || _console == null) return;

            lock (_vfoSwapLock)
            {
                _isSwappingVfo = true;
                _lastVfoSwapTime = Stopwatch.GetTimestamp();

                try
                {
                    // RX2+SPLIT special mode: IC-7100 VFO B holds the TX sub-frequency of VFO A;
                    // swapping the two VFOs on the radio would destroy that sub-frequency value.
                    // Only push the current Thetis VFO A freq to IC-7100 VFO A and return.
                    if (_console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX)
                    {
                        double rxFreq = _console.VFOAFreq;
                        if (_currentRadioSelectedVfo != CIVProtocol.VFO_A)
                        {
                            SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                            _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                            Thread.Sleep(40);
                        }
                        if (rxFreq > 0)
                        {
                            SendFrame(CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, rxFreq));
                            lock (_stateLock)
                            {
                                _lastSentVfoAFreq = rxFreq;
                                _pendingVfoAFreq = rxFreq;
                                _freqChangePending = false;
                                // Tell flood timer IC-7100 VFO B is already correct (sub-freq intact);
                                // do NOT let it push _pendingVfoBFreq (now set to new Thetis VFO B)
                                // which would overwrite the radio's sub-frequency.
                                _pendingVfoBFreq = _lastSentVfoBFreq;
                                _vfoBChangePending = false;
                            }
                        }
                        return;
                    }

                    // 1. Send native Icom CI-V command to exchange VFO A and VFO B (0x07 0xB0)
                    SendFrame(CIVProtocol.SwapVfoFrame(_radioAddr, _hostAddr));
                    Thread.Sleep(50);

                    // 2. In Split mode, ensure Split remains ON after VFO exchange
                    bool isSplit = (_console != null && (_console.VFOBTX || _console.VFOSplit || _console.FullDuplex));
                    if (isSplit && (_actualRadioSplit || _lastSentSplit))
                    {
                        SendFrame(CIVProtocol.SetSplitFrame(_radioAddr, _hostAddr, true));
                        Thread.Sleep(30);
                    }

                    lock (_stateLock)
                    {
                        _lastSentVfoAFreq = _console.VFOAFreq;
                        _lastSentVfoBFreq = _console.VFOBFreq;

                        _pendingVfoAFreq = _console.VFOAFreq;
                        _pendingVfoBFreq = _console.VFOBFreq;
                        _pendingTxFreq = _console.TXFreq;

                        _pendingSplit = isSplit;
                        _lastSentSplit = isSplit;

                        _freqChangePending = false;
                        _vfoBChangePending = false;
                        _splitChangePending = false;
                    }
                }
                finally
                {
                    _lastVfoSwapTime = Stopwatch.GetTimestamp();
                    _isSwappingVfo = false;
                }
            }
        }

        private void OnSplitChanged(int rx, bool oldSplit, bool newSplit)
        {
            NotifySplitOrFullDuplexChanged();
        }

        private void OnVFOTXChanged(bool vfoB, bool oldState, bool newState)
        {
            NotifySplitOrFullDuplexChanged();
        }

        private void OnRX2EnabledChanged(bool enabled)
        {
            NotifySplitOrFullDuplexChanged();
        }

        private void OnModeChanged(int rx, DSPMode oldMode, DSPMode newMode, Band oldBand, Band newBand)
        {
            if (_suppressOutgoingUpdates || !IsOpen || _console == null) return;

            lock (_stateLock)
            {
                _pendingMode = newMode;
                _pendingFilterWidth = Math.Abs(_console.RX1FilterHigh - _console.RX1FilterLow);
                _modeChangePending = true;
            }
        }

        private void OnFilterEdgesChanged(int rx, Filter filter, Band band, int low, int high, string sName, int max_width, int max_shift)
        {
            if (_suppressOutgoingUpdates || !IsOpen || _console == null) return;

            lock (_stateLock)
            {
                _pendingFilterWidth = Math.Abs(high - low);
                _modeChangePending = true;
            }
        }

        private void OnSetBandChanged(int rx, Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider)
        {
            if (!IsOpen || _console == null || rx != 1) return;

            lock (_stateLock)
            {
                _pendingVfoAFreq = newFreq;
                _freqChangePending = true;

                if (!IsSplitRequired())
                {
                    _pendingTxFreq = newFreq;
                    _pendingVfoBFreq = _console.VFOBFreq;
                    _vfoBChangePending = true;
                    _lastVfoBTuneTime = Stopwatch.GetTimestamp();
                }

                _pendingMode = newMode;
                _pendingFilterWidth = Math.Abs(_console.RX1FilterHigh - _console.RX1FilterLow);
                _modeChangePending = true;
            }

            try
            {
                _floodTimer?.Change(0, FLOOD_INTERVAL_MS);
            }
            catch { }
        }

        #endregion

        #region Outgoing Flood Control & Dispatch

        private void OnFloodTimerTick(object state)
        {
            if (!IsOpen || _suppressOutgoingUpdates || _isSwappingVfo || _radioInitiatedSwapInProgress) return;

            double targetVfoAFreq = 0.0;
            double targetVfoBFreq = 0.0;
            bool doVfoA = false;
            bool doVfoB = false;
            bool doMode = false;
            bool doSplit = false;
            bool targetSplit = false;
            DSPMode targetMode = DSPMode.USB;
            int targetFilterWidth = 2700;

            lock (_stateLock)
            {
                bool splitRequired = IsSplitRequired();

                if (splitRequired != _lastSentSplit || _splitChangePending)
                {
                    doSplit = true;
                    targetSplit = splitRequired;
                    _splitChangePending = false;

                    if (_pendingVfoAFreq <= 0 && _console != null)
                    {
                        _pendingVfoAFreq = _console.VFOAFreq;
                    }

                    bool rx2Split = _console != null && _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX;
                    bool vfoBTxSplit = _console != null && _console.VFOBTX && !_console.VFOSplit && !_console.FullDuplex;
                    if (rx2Split)
                    {
                        double txFreq = _console.VFOASubFreq > 0 ? _console.VFOASubFreq : _console.TXFreq;
                        if (txFreq > 0) _pendingTxFreq = txFreq;
                    }
                    else if (vfoBTxSplit)
                    {
                        _pendingTxFreq = _console.VFOBFreq;
                    }
                    else if (_console != null)
                    {
                        _pendingTxFreq = _console.TXFreq;
                    }

                    targetVfoBFreq = _pendingTxFreq;
                    targetVfoAFreq = _pendingVfoAFreq;
                }

                if (_freqChangePending)
                {
                    doVfoA = true;
                    targetVfoAFreq = _pendingVfoAFreq;
                    _freqChangePending = false;
                }

                if (_vfoBChangePending)
                {
                    // Dampen VFO B swapping while actively tuning:
                    // On the IC-7100, setting VFO B requires a VFO A -> VFO B -> VFO A swap sequence.
                    // To avoid audio clicks and display flicker on every 1 Hz dial tick, wait 400ms after tuning pauses
                    // before dispatching the swap (unless transmitting).
                    double msSinceTune = _lastVfoBTuneTime > 0 
                        ? (double)(Stopwatch.GetTimestamp() - _lastVfoBTuneTime) / Stopwatch.Frequency * 1000.0 
                        : 9999.0;

                    // Bypass the debounce when VFOBTX is active (RX2/WSJT-X TX scenario) or when in RX2+SPLIT mode:
                    bool rx2Split = _console != null && _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX;
                    bool urgentVfoBUpdate = splitRequired && _console != null && (_console.VFOBTX || rx2Split);

                    if (msSinceTune >= 400.0 || urgentVfoBUpdate)
                    {
                        if (splitRequired)
                        {
                            bool vfoBTxSplit = _console != null && _console.VFOBTX && !_console.VFOSplit && !_console.FullDuplex;
                            if (rx2Split)
                            {
                                double txFreq = _console.VFOASubFreq > 0 ? _console.VFOASubFreq : _console.TXFreq;
                                if (txFreq > 0) _pendingTxFreq = txFreq;
                            }
                            else if (vfoBTxSplit)
                            {
                                _pendingTxFreq = _console.VFOBFreq;
                            }
                            else if (_console != null)
                            {
                                _pendingTxFreq = _console.TXFreq;
                            }
                            targetVfoBFreq = _pendingTxFreq;
                        }
                        else
                        {
                            targetVfoBFreq = _pendingVfoBFreq;
                        }

                        if (targetVfoBFreq > 0 && Math.Abs(targetVfoBFreq - _lastSentVfoBFreq) > 0.0000015)
                        {
                            doVfoB = true;
                        }
                        _vfoBChangePending = false;
                    }
                }

                if (_modeChangePending)
                {
                    doMode = true;
                    targetMode = _pendingMode;
                    targetFilterWidth = _pendingFilterWidth;
                    _modeChangePending = false;
                }
            }

            // Dispatch pending commands sequentially
            if (doSplit)
            {
                if (targetSplit)
                {
                    ActivateSplit(targetVfoAFreq, targetVfoBFreq);
                }
                else
                {
                    DeactivateSplit(targetVfoAFreq);
                }
            }

            if (doVfoA && !doSplit)
            {
                SendVfoAFrequency(targetVfoAFreq, force: true);
            }

            if (doVfoB && !doSplit)
            {
                SendVfoBFrequency(targetVfoBFreq);
            }

            if (doMode)
            {
                SendModeAndFilter(targetMode, targetFilterWidth);
            }

            // Periodically poll transceiver condition / PTT state (every 100ms = 2 ticks)
            // so pressing the physical microphone PTT on the IC-7100 triggers Thetis PTT/MOX
            if (_syncPTT && !doVfoA && !doVfoB && !doMode && !doSplit)
            {
                _pttPollCounter++;
                if (_pttPollCounter >= 2)
                {
                    _pttPollCounter = 0;
                    if (!_freqChangePending && !_vfoBChangePending && !_isSwappingVfo)
                    {
                        PollPttCondition();
                    }
                }
            }
        }

        private void PollPttCondition()
        {
            if (!IsOpen || _suppressOutgoingUpdates || _isSwappingVfo) return;
            byte[] frame = CIVProtocol.ReadPttFrame(_radioAddr, _hostAddr);
            SendFrame(frame);
        }

        private void SendVfoSwap()
        {
            NotifyVFOSwap();
        }

        private void SendVfoAFrequency(double freqMHz, bool force = false)
        {
            if (freqMHz <= 0) return;
            if (!force && Math.Abs(freqMHz - _lastSentVfoAFreq) < 0.0000005) return;

            lock (_vfoSwapLock)
            {
                if (!force && Math.Abs(freqMHz - _lastSentVfoAFreq) < 0.0000005) return;

                if (_currentRadioSelectedVfo != CIVProtocol.VFO_A)
                {
                    SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                    _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                    Thread.Sleep(30);
                }

                byte[] frame = CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, freqMHz);
                SendFrame(frame);
                _lastSentVfoAFreq = freqMHz;
            }
        }

        private void SendVfoBFrequency(double freqMHz, bool force = false)
        {
            if (freqMHz <= 0) return;
            if (!force && Math.Abs(freqMHz - _lastSentVfoBFreq) < 0.0000005) return;

            lock (_vfoSwapLock)
            {
                if (!force && Math.Abs(freqMHz - _lastSentVfoBFreq) < 0.0000005) return;
                _lastSentVfoBFreq = freqMHz;

                // Set unselected VFO on the IC-7100:
                // 1. Select the other VFO (VFO B)
                // 2. Set Frequency
                // 3. Reselect the original selected VFO (VFO A)
                bool selectOther = (_currentRadioSelectedVfo == CIVProtocol.VFO_A);
                byte[] selOther = CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, selectOther);
                byte[] setFreq = CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, freqMHz);
                byte[] selOriginal = CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, !selectOther);

                _isSwappingVfo = true;
                _lastVfoSwapTime = Stopwatch.GetTimestamp();

                try
                {
                    SendFrame(selOther);
                    Thread.Sleep(50);
                    SendFrame(setFreq);
                    Thread.Sleep(70); // 70ms allows IC-7100 PLL synthesizer to fully lock
                    SendFrame(selOriginal);
                    Thread.Sleep(50);
                    // Fail-safe confirmation: Reselect original VFO a second time
                    SendFrame(selOriginal);
                    Thread.Sleep(30);

                    // On the IC-7100, selecting VFO B (0x07 0x01) causes the radio to exit Split mode.
                    // If Split mode was active, we MUST re-assert Split ON so the radio remains in Split!
                    if (_actualRadioSplit || _lastSentSplit)
                    {
                        SendFrame(CIVProtocol.SetSplitFrame(_radioAddr, _hostAddr, true));
                        _lastSentSplit = true;
                        _actualRadioSplit = true;
                        Thread.Sleep(40);
                    }
                }
                finally
                {
                    _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                    _lastVfoSwapTime = Stopwatch.GetTimestamp();
                    _isSwappingVfo = false;
                }
            }
        }

        private void ActivateSplit(double vfoAFreq, double vfoBFreq)
        {
            lock (_vfoSwapLock)
            {
                if (vfoAFreq <= 0 && _console != null) vfoAFreq = _console.VFOAFreq;
                bool rx2Split = _console != null && _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX;
                if (rx2Split)
                {
                    double txFreq = _console.VFOASubFreq > 0 ? _console.VFOASubFreq : _console.TXFreq;
                    if (txFreq > 0) vfoBFreq = txFreq;
                }
                else if (vfoBFreq <= 0 && _console != null)
                {
                    bool vfoBTxSplit = _console.VFOBTX && !_console.VFOSplit && !_console.FullDuplex;
                    vfoBFreq = vfoBTxSplit ? _console.VFOBFreq : _console.TXFreq;
                }

                _isSwappingVfo = true;
                _lastVfoSwapTime = Stopwatch.GetTimestamp();

                try
                {
                    // 1. Set VFO B frequency on radio WHILE SPLIT IS STILL OFF (always unconditionally sync on split activation).
                    // On the IC-7100, selecting VFO B (0x07 0x01) while Split is ON will cancel Split!
                    // Setting VFO B first guarantees Split will not be immediately cancelled.
                    if (vfoBFreq > 0)
                    {
                        SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, true));
                        Thread.Sleep(50);
                        SendFrame(CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, vfoBFreq));
                        _lastSentVfoBFreq = vfoBFreq;
                        Thread.Sleep(80); // Synthesizer lock settling time
                    }

                    // 2. Select VFO A and ensure VFO A frequency is set
                    SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                    _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                    Thread.Sleep(50);
                    if (vfoAFreq > 0 && Math.Abs(vfoAFreq - _lastSentVfoAFreq) > 0.0000015)
                    {
                        SendFrame(CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, vfoAFreq));
                        _lastSentVfoAFreq = vfoAFreq;
                        Thread.Sleep(40);
                    }

                    // 3. Confirm VFO A is selected
                    SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                    Thread.Sleep(40);

                    // 4. FINALLY, turn Split ON as the last step!
                    // On the IC-7100, entering Split keeps VFO A as RX and VFO B as TX.
                    // Reselecting VFO B after this would cancel Split, so this MUST be the final command!
                    SendFrame(CIVProtocol.SetSplitFrame(_radioAddr, _hostAddr, true));
                    _lastSentSplit = true;
                    _actualRadioSplit = true;
                    Thread.Sleep(50);
                }
                finally
                {
                    _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                    _lastVfoSwapTime = Stopwatch.GetTimestamp();
                    _isSwappingVfo = false;
                }
            }
        }

        private void DeactivateSplit(double vfoAFreq)
        {
            lock (_vfoSwapLock)
            {
                if (vfoAFreq <= 0 && _console != null) vfoAFreq = _console.VFOAFreq;

                _isSwappingVfo = true;
                _lastVfoSwapTime = Stopwatch.GetTimestamp();

                try
                {
                    // 1. Turn Split OFF immediately
                    SendFrame(CIVProtocol.SetSplitFrame(_radioAddr, _hostAddr, false));
                    _lastSentSplit = false;
                    _actualRadioSplit = false;
                    Thread.Sleep(40);

                    // 2. Unconditionally ensure VFO A is selected on the radio
                    SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                    _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                    Thread.Sleep(40);

                    // 3. Unconditionally set VFO A frequency on radio so old VFO B frequency is never shown
                    if (vfoAFreq > 0)
                    {
                        SendFrame(CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, vfoAFreq));
                        _lastSentVfoAFreq = vfoAFreq;
                    }

                    // 4. (RX2+SPLIT only) Push Thetis VFO B to IC-7100 VFO B so the radio's two
                    //    VFOs are coherent now that RX2+SPLIT special mode is being left.
                    //    We select VFO B, set the frequency, then return to VFO A.
                    if (_console?.RX2Enabled == true)
                    {
                        double vfoBFreq = _console.VFOBFreq;
                        if (vfoBFreq > 0)
                        {
                            SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, true));
                            Thread.Sleep(40);
                            SendFrame(CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, vfoBFreq));
                            Thread.Sleep(40);
                            SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                            _lastSentVfoBFreq = vfoBFreq;
                        }
                    }
                }
                finally
                {
                    _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                    _lastVfoSwapTime = Stopwatch.GetTimestamp();
                    _isSwappingVfo = false;
                }
            }
        }

        /// <summary>
        /// Reads IC-7100's current VFO B frequency and synchronises Thetis VFO B to it,
        /// then re-activates Split.  Called on a background thread when the IC-7100 presses SPLIT.
        ///
        /// Strategy: Selecting VFO B on the IC-7100 (0x07 0x01) causes the radio to broadcast
        /// its VFO B frequency as a 0x03 frame.  On the IC-7100 this also cancels split, so we
        /// temporarily back out, read the freq, update Thetis, then re-enable split.
        /// </summary>
        private void SyncVfoBFromRadio(double vfoAFreq)
        {
            if (!IsOpen || _console == null) return;

            double capturedFreq = 0;
            lock (_vfoSwapLock)
            {
                _isSwappingVfo = true;
                _lastVfoSwapTime = Stopwatch.GetTimestamp();
                try
                {
                    // Step 1 — Arm the one-shot capture, then select VFO B.
                    // The IC-7100 will emit 0x0F 0x00 (split cancelled) and then 0x03 <VFO-B-freq>.
                    // _readingRadioVfoBFreq makes CMD_SPLIT ignore the phantom 0x0F 0x00,
                    // and makes CMD_READ_FREQ capture the freq instead of treating it as VFO A.
                    lock (_vfoBReadLock)
                    {
                        _capturedVfoBFreq = 0;
                        _readingRadioVfoBFreq = true;

                        SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, true)); // 0x07 0x01

                        // Wait up to 400 ms for the 0x03 response
                        if (_readingRadioVfoBFreq)
                            Monitor.Wait(_vfoBReadLock, 400);

                        capturedFreq = _capturedVfoBFreq;
                        _readingRadioVfoBFreq = false;
                    }

                    Thread.Sleep(40);

                    // Step 2 — Restore VFO A selection
                    SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false)); // 0x07 0x00
                    _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                    Thread.Sleep(40);
                }
                finally
                {
                    _readingRadioVfoBFreq = false; // safety clear
                    _lastVfoSwapTime = Stopwatch.GetTimestamp();
                    _isSwappingVfo = false;
                }
            }

            // If the read timed out or returned an invalid value, fall back to Thetis VFO B
            if (capturedFreq <= 0)
            {
                double fallback = _console != null ? _console.VFOBFreq : 0;
                ActivateSplit(vfoAFreq, fallback);
                return;
            }

            // Step 3 — Update Thetis VFO B on the UI thread
            _suppressOutgoingUpdates = true;
            try
            {
                _console.BeginInvoke(new Action(() =>
                {
                    try { _console.VFOBFreq = capturedFreq; }
                    finally { _suppressOutgoingUpdates = false; }
                }));
            }
            catch { _suppressOutgoingUpdates = false; }

            // Brief pause to let the UI update land, then re-enable split with the captured VFO B freq
            Thread.Sleep(60);
            lock (_stateLock) { _lastSentVfoBFreq = capturedFreq; }

            // Step 4 — Re-activate split (sets VFO B on radio then sends 0x0F 0x01)
            ActivateSplit(vfoAFreq, capturedFreq);
        }

        private void SendImmediatePtt(bool tx)
        {
            lock (_vfoSwapLock)
            {
                if (tx == _lastSentPtt) return;

                if (tx)
                {
                    // Pre-TX Check: If split operation is required, ensure IC-7100 Split is enabled
                    // and VFO B is set to the current transmit frequency before keying PTT.
                    bool splitRequired;
                    double txFreq;
                    double vfoAFreq;
                    lock (_stateLock)
                    {
                        // Fix D: If _pendingTxFreq is stale/zero and the split is driven by VFOBTX
                        // (not VFOSplit/FullDuplex), use VFOBFreq as fallback — TXFreq equals VFOAFreq
                        // in simplex and would point the radio at the wrong frequency.
                        bool vfoBTxSplit = _console != null && _console.VFOBTX && !_console.VFOSplit && !_console.FullDuplex;
                        double fallbackFreq = vfoBTxSplit ? (_console?.VFOBFreq ?? 0) : (_console != null ? _console.TXFreq : 0);
                        txFreq = _pendingTxFreq > 0 ? _pendingTxFreq : fallbackFreq;
                        vfoAFreq = _pendingVfoAFreq > 0 ? _pendingVfoAFreq : (_console != null ? _console.VFOAFreq : 0);
                        splitRequired = IsSplitRequired();
                    }

                    if (splitRequired)
                    {
                        if (!_actualRadioSplit || !_lastSentSplit)
                        {
                            ActivateSplit(vfoAFreq, txFreq);
                        }
                        else if (txFreq > 0 && Math.Abs(txFreq - _lastSentVfoBFreq) > 0.0000015)
                        {
                            SendVfoBFrequency(txFreq);
                        }
                    }
                    else if (_actualRadioSplit)
                    {
                        DeactivateSplit(vfoAFreq);
                    }
                }

                byte[] frame = CIVProtocol.SetPttFrame(_radioAddr, _hostAddr, tx);
                SendFrame(frame);
                _lastSentPtt = tx;
            }
        }

        private void SendSplit(bool splitOn, bool force = false)
        {
            if (!force && splitOn == _lastSentSplit) return;

            if (splitOn)
            {
                double vfoAFreq = _console != null ? _console.VFOAFreq : 0;
                double vfoBFreq = _console != null ? _console.TXFreq : 0;
                ActivateSplit(vfoAFreq, vfoBFreq);
            }
            else
            {
                double vfoAFreq = _console != null ? _console.VFOAFreq : 0;
                DeactivateSplit(vfoAFreq);
            }
        }

        private void SendModeAndFilter(DSPMode mode, int filterWidthHz)
        {
            CIVMode civMode;
            CIVFilter civFilter;
            CIVDataMode dataMode;
            CIVProtocol.MapThetisMode(mode, filterWidthHz, out civMode, out civFilter, out dataMode);

            if (civMode != _lastSentCivMode || civFilter != _lastSentCivFilter)
            {
                byte[] modeFrame = CIVProtocol.SetModeFrame(_radioAddr, _hostAddr, civMode, civFilter);
                SendFrame(modeFrame);
                _lastSentCivMode = civMode;
                _lastSentCivFilter = civFilter;
            }

            if (dataMode != _lastSentDataMode)
            {
                byte[] dataModeFrame = CIVProtocol.SetDataModeFrame(_radioAddr, _hostAddr, dataMode, civFilter);
                SendFrame(dataModeFrame);
                _lastSentDataMode = dataMode;
            }

            if (_syncFilterWidth && (civMode == CIVMode.USB || civMode == CIVMode.LSB || civMode == CIVMode.CW || civMode == CIVMode.CW_R))
            {
                byte widthCode = CIVProtocol.CalculateSsbIfFilterWidthCode(filterWidthHz);
                if (widthCode != _lastSentFilterWidthCode)
                {
                    byte[] filterWidthFrame = CIVProtocol.SetFilterWidthFrame(_radioAddr, _hostAddr, widthCode);
                    SendFrame(filterWidthFrame);
                    _lastSentFilterWidthCode = widthCode;
                }
            }
        }

        private long _lastFrameSentTime = 0;

        private void SendFrame(byte[] frame)
        {
            if (frame == null || frame.Length == 0) return;

            lock (_portLock)
            {
                if (_serialPort == null || !_serialPort.IsOpen) return;

                try
                {
                    // Enforce minimum inter-frame spacing of 25ms on the physical half-duplex CI-V bus
                    // to prevent UART buffer overflow and bus collisions with the radio's ACK frames.
                    if (_lastFrameSentTime > 0)
                    {
                        double elapsedMs = (double)(Stopwatch.GetTimestamp() - _lastFrameSentTime) / Stopwatch.Frequency * 1000.0;
                        if (elapsedMs < 25.0)
                        {
                            Thread.Sleep((int)(25.0 - elapsedMs));
                        }
                    }

                    // Track recently sent frame for echo suppression
                    lock (_echoLock)
                    {
                        if (_recentSentFrames.Count >= 20)
                        {
                            _recentSentFrames.Dequeue();
                        }
                        _recentSentFrames.Enqueue(frame);
                    }

                    _serialPort.Write(frame, 0, frame.Length);
                    _lastFrameSentTime = Stopwatch.GetTimestamp();
                    Log("[TX] {0}", HexDump(frame));
                }
                catch (Exception ex)
                {
                    Log("[TX:ERROR] Error sending frame {0}: {1}", HexDump(frame), ex.Message);
                    Debug.WriteLine(string.Format("[CIVController] Error sending frame: {0}", ex.Message));
                }
            }
        }

        #endregion

        #region Incoming Serial Processing & Echo Suppression

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_portLock)
            {
                if (_serialPort == null || !_serialPort.IsOpen) return;

                try
                {
                    int bytesAvailable = _serialPort.BytesToRead;
                    if (bytesAvailable <= 0) return;

                    byte[] buffer = new byte[bytesAvailable];
                    int bytesRead = _serialPort.Read(buffer, 0, bytesAvailable);

                    for (int i = 0; i < bytesRead; i++)
                    {
                        byte b = buffer[i];
                        _rxBuffer.Add(b);

                        if (b == CIVProtocol.EOM)
                        {
                            ProcessRxBuffer();
                        }
                    }

                    // Guard against unbounded buffer growth from corrupted streams
                    if (_rxBuffer.Count > 1024)
                    {
                        _rxBuffer.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("[CIVController] Error reading serial data: {0}", ex.Message));
                }
            }
        }

        private void ProcessRxBuffer()
        {
            while (true)
            {
                int startIndex = -1;

                // Locate preamble FE FE
                for (int i = 0; i < _rxBuffer.Count - 1; i++)
                {
                    if (_rxBuffer[i] == CIVProtocol.PREAMBLE && _rxBuffer[i + 1] == CIVProtocol.PREAMBLE)
                    {
                        startIndex = i;
                        break;
                    }
                }

                if (startIndex == -1)
                {
                    _rxBuffer.Clear();
                    return;
                }

                // Remove any garbage preceding the preamble
                if (startIndex > 0)
                {
                    _rxBuffer.RemoveRange(0, startIndex);
                }

                // Find EOM FD
                int endIndex = _rxBuffer.IndexOf(CIVProtocol.EOM);
                if (endIndex == -1) return; // Incomplete packet, wait for more data

                int frameLen = endIndex + 1;
                byte[] frame = new byte[frameLen];
                _rxBuffer.CopyTo(0, frame, 0, frameLen);
                _rxBuffer.RemoveRange(0, frameLen);

                Log("[RX] {0}", HexDump(frame));
                HandleCIVFrame(frame);
            }
        }

        private void HandleCIVFrame(byte[] frame)
        {
            // Minimum valid frame: FE FE [to] [from] [cmd] FD (6 bytes)
            if (frame == null || frame.Length < 6)
            {
                Log("[HANDLE:DROP] Frame too short: len={0}", frame != null ? frame.Length : 0);
                return;
            }

            // Check for echo of our own sent command
            if (IsLocalEcho(frame))
            {
                // Already logged in IsLocalEcho
                return;
            }

            byte toAddr = frame[2];
            byte fromAddr = frame[3];
            byte cmd = frame[4];

            // Frame must be directed to Host (0xE0) or Broadcast (0x00)
            if (toAddr != _hostAddr && toAddr != CIVProtocol.BROADCAST_ADDR)
            {
                Log("[HANDLE:DROP] Unexpected toAddr=0x{0:X2} (expected 0x{1:X2} or 0x00)", toAddr, _hostAddr);
                return;
            }

            // Frame must originate from Radio or Host (local loopback)
            if (fromAddr != _radioAddr && fromAddr != _hostAddr)
            {
                Log("[HANDLE:DROP] Unexpected fromAddr=0x{0:X2} (expected 0x{1:X2} or 0x{2:X2})", fromAddr, _radioAddr, _hostAddr);
                return;
            }

            // Acknowledgment or NAK
            if (cmd == CIVProtocol.ACK)
            {
                Log("[HANDLE:ACK] ACK received from IC-7100");
                Debug.WriteLine("[CIVController] ACK received from IC-7100");
                return;
            }
            if (cmd == CIVProtocol.NAK)
            {
                Log("[HANDLE:NAK] NAK received from IC-7100");
                Debug.WriteLine("[CIVController] NAK received from IC-7100 (unsupported command or PLL out-of-lock)");
                return;
            }

            // If transceive is disabled, ignore unsolicited status broadcasts from the radio
            if (!_transceiveEnabled)
            {
                Log("[HANDLE:DROP] Transceive disabled, ignoring cmd=0x{0:X2}", cmd);
                return;
            }

            switch (cmd)
            {
                // Frequency report (0x00 or 0x03)
                case 0x00:
                case CIVProtocol.CMD_READ_FREQ:
                    if (frame.Length >= 10 && frame.Length <= 12)
                    {
                        double freqMHz = CIVProtocol.DecodeFrequency(frame, 5);
                        // If a VFO B read is in progress, capture this freq instead of treating it as VFO A
                        if (_readingRadioVfoBFreq)
                        {
                            lock (_vfoBReadLock)
                            {
                                if (_readingRadioVfoBFreq)
                                {
                                    _capturedVfoBFreq = freqMHz;
                                    _readingRadioVfoBFreq = false;
                                    Monitor.PulseAll(_vfoBReadLock);
                                    break;
                                }
                            }
                        }
                        HandleIncomingFrequency(freqMHz);
                    }
                    break;

                // Mode report (0x01 or 0x04)
                case 0x01:
                case CIVProtocol.CMD_READ_MODE:
                    if (frame.Length >= 7)
                    {
                        CIVMode mode = (CIVMode)frame[5];
                        CIVFilter filter = frame.Length >= 8 ? (CIVFilter)frame[6] : CIVFilter.FIL2;
                        HandleIncomingMode(mode, filter);
                    }
                    break;

                // VFO selection report (0x07)
                case CIVProtocol.CMD_VFO_SEL:
                    if (frame.Length >= 6)
                    {
                        byte vfoId = frame[5];

                        Log("[HANDLE:VFO_SEL] vfoId=0x{0:X2}, MOX={1}, isSwappingVfo={2}, radioSwapInProgress={3}, rx2SplitHandled={4}, RX2={5}, Split={6}, VFOBTX={7}, VFOATX={8}",
                            vfoId,
                            _console != null && _console.MOX,
                            _isSwappingVfo,
                            _radioInitiatedSwapInProgress,
                            _rx2SplitSwapHandled,
                            _console != null && _console.RX2Enabled,
                            _console != null && _console.VFOSplit,
                            _console != null && _console.VFOBTX,
                            _console != null && _console.VFOATX);

                        // Do not process VFO swaps while transmitting
                        if (_console != null && _console.MOX)
                        {
                            Log("[HANDLE:VFO_SEL] Blocked by MOX");
                            return;
                        }

                        if (_isSwappingVfo || _radioInitiatedSwapInProgress)
                        {
                            Log("[HANDLE:VFO_SEL] Blocked by isSwappingVfo={0} / radioInitiatedSwapInProgress={1}",
                                _isSwappingVfo, _radioInitiatedSwapInProgress);
                            return;
                        }

                        if (vfoId == CIVProtocol.VFO_SWAP)
                        {
                            Log("[HANDLE:VFO_SWAP] Entered. rx2SplitSwapHandled={0}", _rx2SplitSwapHandled);
                            // If CMD_SPLIT already handled an RX2+SPLIT A/B tap (the radio sent 0x0F 0x00 first),
                            // clear the flag and skip — we've already done VFOSwap+VFOBTX in that handler.
                            if (_rx2SplitSwapHandled)
                            {
                                Log("[HANDLE:VFO_SWAP] Skipped because _rx2SplitSwapHandled was true");
                                _rx2SplitSwapHandled = false;
                                return;
                            }

                            // Operator physically triggered VFO swap on the radio
                            lock (_vfoSwapLock)
                            {
                                _lastVfoSwapTime = Stopwatch.GetTimestamp();
                                _radioInitiatedSwapInProgress = true;

                                lock (_stateLock)
                                {
                                    double temp = _lastSentVfoAFreq;
                                    _lastSentVfoAFreq = _lastSentVfoBFreq;
                                    _lastSentVfoBFreq = temp;

                                    _pendingVfoAFreq = _lastSentVfoAFreq;
                                    _pendingVfoBFreq = _lastSentVfoBFreq;
                                    _pendingTxFreq = IsSplitRequired() ? _lastSentVfoBFreq : _lastSentVfoAFreq;

                                    _freqChangePending = false;
                                    _vfoBChangePending = false;
                                    _splitChangePending = false;
                                }

                                _suppressOutgoingUpdates = true;
                                try
                                {
                                    _console.BeginInvoke(new Action(() =>
                                    {
                                        // Capture RX2+SPLIT state BEFORE any modification (UI thread).
                                        bool inRX2SplitMode = _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX;
                                        Log("[UI:VFO_SWAP] BeginInvoke: inRX2SplitMode={0}, RX2={1}, Split={2}, VFOBTX={3}, VFOATX={4}",
                                            inRX2SplitMode, _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                                        try
                                        {
                                            _console.VFOSwap();
                                            Log("[UI:VFO_SWAP] After VFOSwap: RX2={0}, Split={1}, VFOBTX={2}, VFOATX={3}",
                                                _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                                            if (inRX2SplitMode)
                                            {
                                                Log("[UI:VFO_SWAP] Setting VFOBTX = true...");
                                                _suppressOutgoingUpdates = false;
                                                _console.VFOBTX = true;
                                                Log("[UI:VFO_SWAP] After VFOBTX=true: RX2={0}, Split={1}, VFOBTX={2}, VFOATX={3}",
                                                    _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                                            }
                                        }
                                        finally
                                        {
                                            _suppressOutgoingUpdates = false;
                                            _radioInitiatedSwapInProgress = false;
                                        }
                                    }));
                                }
                                catch (Exception ex)
                                {
                                    Log("[UI:VFO_SWAP:ERROR] {0}", ex.Message);
                                    _suppressOutgoingUpdates = false;
                                    _radioInitiatedSwapInProgress = false;
                                }
                            }
                            return;
                        }

                        if (vfoId == CIVProtocol.VFO_EQUAL)
                        {
                            Log("[HANDLE:VFO_EQUAL] Entered.");
                            HandleRadioVfoEqual();
                            return;
                        }

                        if (vfoId == CIVProtocol.VFO_A || vfoId == CIVProtocol.VFO_B)
                        {
                            Log("[HANDLE:VFO_A/B] vfoId=0x{0:X2}, currentSelected=0x{1:X2}, RX2={2}, Split={3}, VFOBTX={4}",
                                vfoId, _currentRadioSelectedVfo,
                                _console != null && _console.RX2Enabled,
                                _console != null && _console.VFOSplit,
                                _console != null && _console.VFOBTX);

                            if (vfoId == _currentRadioSelectedVfo)
                            {
                                Log("[HANDLE:VFO_A/B] Redundant vfoId=0x{0:X2}, ignoring", vfoId);
                                return; // Redundant — radio already on this VFO
                            }

                            lock (_vfoSwapLock)
                            {
                                _currentRadioSelectedVfo = vfoId;
                                _lastVfoSwapTime = Stopwatch.GetTimestamp();
                                _radioInitiatedSwapInProgress = true;

                                lock (_stateLock)
                                {
                                    double temp = _lastSentVfoAFreq;
                                    _lastSentVfoAFreq = _lastSentVfoBFreq;
                                    _lastSentVfoBFreq = temp;

                                    _pendingVfoAFreq = _lastSentVfoAFreq;
                                    _pendingVfoBFreq = _lastSentVfoBFreq;
                                    _pendingTxFreq = IsSplitRequired() ? _lastSentVfoBFreq : _lastSentVfoAFreq;

                                    _freqChangePending = false;
                                    _vfoBChangePending = false;
                                    _splitChangePending = false;
                                }

                                _suppressOutgoingUpdates = true;
                                try
                                {
                                    _console.BeginInvoke(new Action(() =>
                                    {
                                        bool inRX2SplitMode = _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX;
                                        Log("[UI:VFO_A/B] inRX2SplitMode={0}, RX2={1}, Split={2}, VFOBTX={3}, VFOATX={4}",
                                            inRX2SplitMode, _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                                        try
                                        {
                                            Log("[UI:VFO_A/B] Calling VFOSwap()...");
                                            _console.VFOSwap();
                                            Log("[UI:VFO_A/B] After VFOSwap: RX2={0}, Split={1}, VFOBTX={2}, VFOATX={3}",
                                                _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                                            if (inRX2SplitMode)
                                            {
                                                Log("[UI:VFO_A/B] Setting VFOBTX = true...");
                                                _suppressOutgoingUpdates = false;
                                                _console.VFOBTX = true;
                                                Log("[UI:VFO_A/B] After VFOBTX=true: RX2={0}, Split={1}, VFOBTX={2}, VFOATX={3}",
                                                    _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                                            }
                                        }
                                        finally
                                        {
                                            _suppressOutgoingUpdates = false;
                                            _radioInitiatedSwapInProgress = false;
                                        }
                                    }));
                                }
                                catch (Exception ex)
                                {
                                    Log("[UI:VFO_A/B:ERROR] {0}", ex.Message);
                                    _suppressOutgoingUpdates = false;
                                    _radioInitiatedSwapInProgress = false;
                                }
                            }
                            return;
                        }

                        Log("[HANDLE:VFO_SEL] Unrecognized vfoId=0x{0:X2}", vfoId);
                    }
                    break;

                // Data Mode report (0x1A 0x06)
                case CIVProtocol.CMD_MISC_1A:
                    if (frame.Length >= 8 && frame[5] == CIVProtocol.SUBCMD_1A_DATA_MODE)
                    {
                        CIVDataMode dataMode = (CIVDataMode)frame[6];
                        CIVFilter filter = frame.Length >= 9 ? (CIVFilter)frame[7] : CIVFilter.FIL2;
                        HandleIncomingDataMode(dataMode, filter);
                    }
                    break;

                // PTT Condition (0x1C 0x00)
                case CIVProtocol.CMD_CONDITION_1C:
                    if (_syncPTT && frame.Length >= 8 && frame[5] == CIVProtocol.SUBCMD_1C_PTT)
                    {
                        bool tx = frame[6] == 0x01;
                        HandleIncomingPtt(tx);
                    }
                    break;

                // Split report (0x0F)
                case CIVProtocol.CMD_SPLIT:
                    if (frame.Length >= 7 && fromAddr == _radioAddr)
                    {
                        byte splitByte = frame[5];
                        bool isSplit = (splitByte == CIVProtocol.SPLIT_ON);

                        Log("[HANDLE:SPLIT] splitByte=0x{0:X2} (isSplit={1}), MOX={2}, isSwapping={3}, swapInProgress={4}, readingVfoB={5}, actualRadioSplit={6}, lastSentSplit={7}, RX2={8}, Split={9}, VFOBTX={10}, VFOATX={11}",
                            splitByte, isSplit,
                            _console != null && _console.MOX,
                            _isSwappingVfo, _radioInitiatedSwapInProgress, _readingRadioVfoBFreq,
                            _actualRadioSplit, _lastSentSplit,
                            _console != null && _console.RX2Enabled,
                            _console != null && _console.VFOSplit,
                            _console != null && _console.VFOBTX,
                            _console != null && _console.VFOATX);

                        if (_console != null && _console.MOX)
                        {
                            Log("[HANDLE:SPLIT] Ignored: MOX active");
                            break;
                        }

                        // Guard against spurious split reports during active VFO swaps or VFO B read:
                        if (_isSwappingVfo || _radioInitiatedSwapInProgress || _readingRadioVfoBFreq)
                        {
                            Log("[HANDLE:SPLIT] Blocked by isSwapping={0} / swapInProgress={1} / readingVfoB={2}",
                                _isSwappingVfo, _radioInitiatedSwapInProgress, _readingRadioVfoBFreq);
                            break;
                        }

                        if (splitByte == CIVProtocol.SPLIT_ON || splitByte == CIVProtocol.SPLIT_OFF)
                        {
                            // Ignore redundant echoes of what we sent or already confirmed
                            if (isSplit == _lastSentSplit && isSplit == _actualRadioSplit)
                            {
                                Log("[HANDLE:SPLIT] Ignored redundant split echo: isSplit={0}", isSplit);
                                break;
                            }

                            _actualRadioSplit = isSplit;
                            _lastSentSplit = isSplit;

                            lock (_stateLock)
                            {
                                _pendingSplit = isSplit;
                                _splitChangePending = false;
                            }

                            if (_console != null)
                            {
                                double vfoAFreq = _console.VFOAFreq;
                                bool isRX2SplitSnap = !isSplit &&
                                    _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX;

                                Log("[HANDLE:SPLIT] Evaluated isRX2SplitSnap={0} (!isSplit={1}, RX2={2}, Split={3}, !VFOBTX={4})",
                                    isRX2SplitSnap, !isSplit, _console.RX2Enabled, _console.VFOSplit, !_console.VFOBTX);

                                if (isRX2SplitSnap)
                                {
                                    // IC-7100 pressed A/B while Thetis is in RX2+SPLIT special mode.
                                    // The radio sent 0x0F 0x00 (Split OFF). We handle the full swap here
                                    // and set a flag so that if 0x07 0xB0 (VFO_SWAP) follows, it is ignored.
                                    Log("[HANDLE:SPLIT] Taking RX2+SPLIT A/B tap branch. Setting _rx2SplitSwapHandled = true");
                                    _rx2SplitSwapHandled = true;
                                    _suppressOutgoingUpdates = true;
                                    try
                                    {
                                        _console.BeginInvoke(new Action(() =>
                                        {
                                            try
                                            {
                                                Log("[UI:SPLIT_RX2] BeginInvoke: calling VFOSwap()...");
                                                _console.VFOSwap();
                                                Log("[UI:SPLIT_RX2] After VFOSwap: calling VFOBTX=true... RX2={0}, Split={1}, VFOBTX={2}",
                                                    _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX);
                                                _console.VFOBTX = true;
                                                Log("[UI:SPLIT_RX2] Done: RX2={0}, Split={1}, VFOBTX={2}, VFOATX={3}",
                                                    _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                                            }
                                            catch (Exception ex)
                                            {
                                                Log("[UI:SPLIT_RX2:ERROR] {0}", ex.Message);
                                            }
                                            finally
                                            {
                                                _suppressOutgoingUpdates = false;
                                            }
                                        }));
                                    }
                                    catch (Exception ex)
                                    {
                                        Log("[HANDLE:SPLIT:ERROR] BeginInvoke failed: {0}", ex.Message);
                                        _suppressOutgoingUpdates = false;
                                        _rx2SplitSwapHandled = false;
                                    }
                                }
                                else
                                {
                                    Log("[HANDLE:SPLIT] Taking standard branch (isSplit={0})", isSplit);
                                    _suppressOutgoingUpdates = true;
                                    try
                                    {
                                        _console.BeginInvoke(new Action(() =>
                                        {
                                            try
                                            {
                                                if (isSplit)
                                                {
                                                    _console.VFOSplit = true;
                                                    _console.VFOBTX = true;
                                                }
                                                else
                                                {
                                                    _console.VFOSplit = false;
                                                    _console.VFOATX = true;
                                                }
                                                Log("[UI:SPLIT_STD] Done setting split: RX2={0}, Split={1}, VFOBTX={2}, VFOATX={3}",
                                                    _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                                            }
                                            catch (Exception ex)
                                            {
                                                Log("[UI:SPLIT_STD:ERROR] {0}", ex.Message);
                                            }
                                            finally
                                            {
                                                _suppressOutgoingUpdates = false;
                                            }
                                        }));
                                    }
                                    catch (Exception ex)
                                    {
                                        Log("[HANDLE:SPLIT:ERROR] BeginInvoke failed: {0}", ex.Message);
                                        _suppressOutgoingUpdates = false;
                                    }
                                    if (isSplit)
                                    {
                                        // IC-7100 initiated Split: synchronize IC-7100's VFO B → Thetis VFO B.
                                        // SyncVfoBFromRadio temporarily selects VFO B (which cancels split on IC-7100),
                                        // captures the VFO B freq from the 0x03 response, then re-activates split.
                                        double captureVfoAFreq = vfoAFreq;
                                        ThreadPool.QueueUserWorkItem(_ =>
                                        {
                                            Thread.Sleep(80); // let IC-7100 split-ON settle
                                            SyncVfoBFromRadio(captureVfoAFreq);
                                        });
                                    }
                                    if (!isSplit && vfoAFreq > 0)
                                    {
                                        // Radio just switched to Simplex: ensure radio is on VFO A and displays VFO A frequency.
                                        ThreadPool.QueueUserWorkItem(_ =>
                                        {
                                            Thread.Sleep(80);
                                            lock (_vfoSwapLock)
                                            {
                                                _isSwappingVfo = true;
                                                _lastVfoSwapTime = Stopwatch.GetTimestamp();
                                                try
                                                {
                                                    SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                                                    _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                                                    Thread.Sleep(40);
                                                    SendFrame(CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, vfoAFreq));
                                                    _lastSentVfoAFreq = vfoAFreq;
                                                }
                                                finally
                                                {
                                                    _lastVfoSwapTime = Stopwatch.GetTimestamp();
                                                    _isSwappingVfo = false;
                                                }
                                            }
                                        });
                                    }
                                }
                            }
                        }
                    }
                    break;

                default:
                    Log("[HANDLE:OTHER] cmd=0x{0:X2}, len={1}, frame={2}", cmd, frame.Length, HexDump(frame));
                    break;
            }
        }

        private void HandleRadioVfoEqual()
        {
            if (_console == null) return;

            _lastVfoSwapTime = Stopwatch.GetTimestamp();

            lock (_stateLock)
            {
                _lastSentVfoBFreq = _lastSentVfoAFreq;
                _pendingVfoBFreq = _lastSentVfoAFreq;
                _pendingTxFreq = IsSplitRequired() ? _lastSentVfoAFreq : _pendingTxFreq;
                _vfoBChangePending = false;
            }

            _suppressOutgoingUpdates = true;
            try
            {
                _console.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (_currentRadioSelectedVfo == CIVProtocol.VFO_B)
                        {
                            _console.CopyVFOBtoA();
                        }
                        else
                        {
                            _console.CopyVFOAtoB();
                        }
                    }
                    finally
                    {
                        _suppressOutgoingUpdates = false;
                    }
                }));
            }
            catch
            {
                _suppressOutgoingUpdates = false;
            }
        }

        private bool IsLocalEcho(byte[] frame)
        {
            lock (_echoLock)
            {
                if (_recentSentFrames.Count == 0) return false;

                // Scan for a match, then consume it (one-shot: each sent frame
                // suppresses exactly one echo, so a later identical frame from
                // the radio operator is not accidentally dropped).
                var frames = _recentSentFrames.ToArray();
                for (int j = 0; j < frames.Length; j++)
                {
                    var sentFrame = frames[j];
                    if (sentFrame.Length != frame.Length) continue;

                    bool match = true;
                    for (int i = 0; i < frame.Length; i++)
                    {
                        if (sentFrame[i] != frame[i]) { match = false; break; }
                    }

                    if (match)
                    {
                        // Remove only the first matching entry and rebuild the queue
                        _recentSentFrames.Clear();
                        for (int k = 0; k < frames.Length; k++)
                        {
                            if (k != j) _recentSentFrames.Enqueue(frames[k]);
                        }
                        Log("[ECHO] Dropped local echo: {0}", HexDump(frame));
                        return true;
                    }
                }
            }
            return false;
        }

        private void HandleIncomingFrequency(double freqMHz)
        {
            if (freqMHz <= 0 || _console == null) return;

            // Frequency range validation: if not using transverters, prevent out-of-range frequencies
            // (e.g. VHF/UHF frequencies from IC-7100 VFO B clamping to MaxFreq 61.440 MHz)
            if (_console.RX1XVTRIndex < 0)
            {
                if (freqMHz > _console.MaxFreq || freqMHz < _console.MinFreq)
                {
                    return;
                }
            }

            // 0. Guard against incoming frequency reports generated during programmatic VFO swap
            if (_isSwappingVfo || _radioInitiatedSwapInProgress) return;
            if (_lastVfoSwapTime > 0)
            {
                double msSinceSwap = (double)(Stopwatch.GetTimestamp() - _lastVfoSwapTime) / Stopwatch.Frequency * 1000.0;
                if (msSinceSwap < 300.0)
                {
                    return;
                }
            }

            // 1. Guard against updates while transmitting:
            // When transmitting, the IC-7100 broadcasts the TX frequency (e.g. VFO B in split).
            // Under no circumstances should transmit broadcasts be treated as receive VFO changes.
            if (_console.MOX) return;

            // 2. Post-TX Settling Window (350 ms):
            // After PTT is released, the transceiver or CI-V bus may still be draining queued frames
            // representing the transmit frequency. Drop any frequency matching the transmit frequency or VFO B.
            if (_lastTxReleaseTime > 0)
            {
                double msSinceRelease = (double)(Stopwatch.GetTimestamp() - _lastTxReleaseTime) / Stopwatch.Frequency * 1000.0;
                if (msSinceRelease < 350.0)
                {
                    if (Math.Abs(freqMHz - _lastSentVfoBFreq) < 0.0000015 ||
                        Math.Abs(freqMHz - _pendingTxFreq) < 0.0000015 ||
                        Math.Abs(freqMHz - _console.TXFreq) < 0.0000015 ||
                        Math.Abs(freqMHz - _console.VFOBFreq) < 0.0000015 ||
                        (_console.RX2Enabled && Math.Abs(freqMHz - _console.VFOASubFreq) < 0.0000015))
                    {
                        return;
                    }
                }
            }

            // 3. Detect Radio-Initiated VFO Swap ([A/B] button on IC-7100):
            // Since the IC-7100 does not broadcast a CI-V VFO report (0x07) in transceive mode,
            // pressing [A/B] on the IC-7100 causes it to switch to the other VFO and broadcast
            // that VFO's frequency.
            // If the incoming frequency matches Thetis VFOB (and differs from VFOA),
            // this signals that the operator pressed [A/B] on the IC-7100.
            double targetVfoBFreq = (_console.VFOBTX || _console.VFOSplit) && _console.TXFreq > 0 
                ? _console.TXFreq 
                : _console.VFOBFreq;

            bool matchesVfoB = (targetVfoBFreq > 0 && Math.Abs(freqMHz - targetVfoBFreq) < 0.0000015) ||
                               (_lastSentVfoBFreq > 0 && Math.Abs(freqMHz - _lastSentVfoBFreq) < 0.0000015) ||
                               (_console.VFOBFreq > 0 && Math.Abs(freqMHz - _console.VFOBFreq) < 0.0000015);

            bool differsFromVfoA = Math.Abs(freqMHz - _console.VFOAFreq) > 0.0000015 &&
                                   Math.Abs(freqMHz - _lastSentVfoAFreq) > 0.0000015;

            if (matchesVfoB && differsFromVfoA)
            {
                Log("[FREQ_IN:SWAP_DETECTED] freq={0:F6} MHz matches VfoB ({1:F6}) and differs from VfoA ({2:F6})",
                    freqMHz, targetVfoBFreq, _console.VFOAFreq);

                lock (_vfoSwapLock)
                {
                    _currentRadioSelectedVfo = CIVProtocol.VFO_B;
                    _lastVfoSwapTime = Stopwatch.GetTimestamp();
                    _radioInitiatedSwapInProgress = true;
                    _suppressOutgoingUpdates = true;

                    try
                    {
                        _console.BeginInvoke(new Action(() =>
                        {
                            bool inRX2SplitMode = _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX;
                            Log("[UI:FREQ_SWAP] BeginInvoke: inRX2SplitMode={0}, RX2={1}, Split={2}, VFOBTX={3}, VFOATX={4}",
                                inRX2SplitMode, _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                            try
                            {
                                Log("[UI:FREQ_SWAP] Calling VFOSwap()...");
                                _console.VFOSwap();
                                Log("[UI:FREQ_SWAP] After VFOSwap: RX2={0}, Split={1}, VFOBTX={2}, VFOATX={3}",
                                    _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                                if (inRX2SplitMode)
                                {
                                    Log("[UI:FREQ_SWAP] Setting VFOBTX = true...");
                                    _console.VFOBTX = true;
                                    Log("[UI:FREQ_SWAP] After VFOBTX=true: RX2={0}, Split={1}, VFOBTX={2}, VFOATX={3}",
                                        _console.RX2Enabled, _console.VFOSplit, _console.VFOBTX, _console.VFOATX);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log("[UI:FREQ_SWAP:ERROR] {0}", ex.Message);
                            }

                            // Cleanly and reliably synchronize the IC-7100 in the background
                            // after the radio's physical A/B transceive broadcast frames have settled on the CI-V wire.
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                try
                                {
                                    Thread.Sleep(200); // Wait 200ms for half-duplex CI-V bus to clear

                                    lock (_vfoSwapLock)
                                    {
                                        _isSwappingVfo = true;
                                        try
                                        {
                                            double targetA;
                                            double targetB;
                                            bool splitReq;
                                            lock (_stateLock)
                                            {
                                                targetA = _console.VFOAFreq;
                                                bool rx2Split = _console.RX2Enabled && _console.VFOSplit && !_console.VFOBTX;
                                                bool vfoBTx = _console.VFOBTX && !_console.VFOSplit && !_console.FullDuplex;
                                                if (rx2Split)
                                                    targetB = _console.VFOASubFreq > 0 ? _console.VFOASubFreq : _console.TXFreq;
                                                else if (vfoBTx)
                                                    targetB = _console.VFOBFreq;
                                                else if (IsSplitRequired())
                                                    targetB = _console.TXFreq;
                                                else
                                                    targetB = _console.VFOBFreq;

                                                splitReq = IsSplitRequired();
                                            }

                                            Log("[UI:FREQ_SWAP:SYNC] Starting radio sync: targetA={0:F6}, targetB={1:F6}, splitReq={2}",
                                                targetA, targetB, splitReq);

                                            // 1. Select VFO B on radio and set frequency
                                            SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, true));
                                            Thread.Sleep(60);
                                            SendFrame(CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, targetB));
                                            Thread.Sleep(80);

                                            // 2. Select VFO A on radio and set frequency
                                            SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                                            Thread.Sleep(60);
                                            SendFrame(CIVProtocol.SetFrequencyFrame(_radioAddr, _hostAddr, targetA));
                                            Thread.Sleep(80);

                                            // 3. Confirm VFO A is selected
                                            SendFrame(CIVProtocol.SelectVfoFrame(_radioAddr, _hostAddr, false));
                                            Thread.Sleep(50);

                                            // 4. Assert Split state
                                            SendFrame(CIVProtocol.SetSplitFrame(_radioAddr, _hostAddr, splitReq));
                                            Thread.Sleep(50);

                                            lock (_stateLock)
                                            {
                                                _currentRadioSelectedVfo = CIVProtocol.VFO_A;
                                                _lastSentVfoAFreq = targetA;
                                                _lastSentVfoBFreq = targetB;
                                                _pendingVfoAFreq = targetA;
                                                _pendingVfoBFreq = targetB;
                                                _pendingTxFreq = targetB;
                                                _lastSentSplit = splitReq;
                                                _actualRadioSplit = splitReq;

                                                _freqChangePending = false;
                                                _vfoBChangePending = false;
                                                _splitChangePending = false;
                                            }

                                            Log("[UI:FREQ_SWAP:SYNC] Radio sync complete: VFOA={0:F6}, VFOB={1:F6}, Split={2}",
                                                targetA, targetB, splitReq);
                                        }
                                        finally
                                        {
                                            _lastVfoSwapTime = Stopwatch.GetTimestamp();
                                            _isSwappingVfo = false;
                                            _radioInitiatedSwapInProgress = false;
                                            _suppressOutgoingUpdates = false;
                                        }
                                    }
                                }
                                catch (Exception syncEx)
                                {
                                    Log("[UI:FREQ_SWAP:SYNC:ERROR] {0}", syncEx.Message);
                                    _radioInitiatedSwapInProgress = false;
                                    _suppressOutgoingUpdates = false;
                                }
                            });
                        }));
                    }
                    catch (Exception ex)
                    {
                        Log("[FREQ_IN:ERROR] BeginInvoke failed: {0}", ex.Message);
                        _suppressOutgoingUpdates = false;
                        _radioInitiatedSwapInProgress = false;
                    }
                }
                return;
            }

            // 4. VFO A Update:
            // The selected VFO on the IC-7100 (whether VFO A or VFO B) represents the active receiver.
            // Check if delta is significant (> 1.5 Hz) and not an echo of our last sent VFO A frequency
            if (Math.Abs(freqMHz - _console.VFOAFreq) < 0.0000015 || Math.Abs(freqMHz - _lastSentVfoAFreq) < 0.0000015)
            {
                return;
            }

            _lastSentVfoAFreq = freqMHz;
            lock (_stateLock)
            {
                _pendingVfoAFreq = freqMHz;
                if (!IsSplitRequired())
                {
                    _pendingTxFreq = freqMHz;
                }
            }

            _suppressOutgoingUpdates = true;
            try
            {
                _console.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        _console.VFOAFreq = freqMHz;
                    }
                    finally
                    {
                        _suppressOutgoingUpdates = false;
                    }
                }));
            }
            catch
            {
                _suppressOutgoingUpdates = false;
            }
        }

        private void HandleIncomingMode(CIVMode mode, CIVFilter filter)
        {
            if (_console == null) return;
            if (_isSwappingVfo || _radioInitiatedSwapInProgress) return;
            if (_lastVfoSwapTime > 0)
            {
                double msSinceSwap = (double)(Stopwatch.GetTimestamp() - _lastVfoSwapTime) / Stopwatch.Frequency * 1000.0;
                if (msSinceSwap < 300.0)
                {
                    return;
                }
            }

            DSPMode currentMode = _console.RX1DSPMode;
            DSPMode targetMode = currentMode;
            bool modeNeedsChange = false;

            switch (mode)
            {
                case CIVMode.LSB:
                    // Preserve DIGL if already in digital LSB mode
                    if (currentMode != DSPMode.LSB && currentMode != DSPMode.DIGL)
                    {
                        targetMode = DSPMode.LSB;
                        modeNeedsChange = true;
                    }
                    break;

                case CIVMode.USB:
                    // Preserve DIGU, DSB, SPEC, or DRM if already in digital or variant USB mode
                    if (currentMode != DSPMode.USB && currentMode != DSPMode.DIGU &&
                        currentMode != DSPMode.DSB && currentMode != DSPMode.SPEC && currentMode != DSPMode.DRM)
                    {
                        targetMode = DSPMode.USB;
                        modeNeedsChange = true;
                    }
                    break;

                case CIVMode.AM:
                    if (currentMode != DSPMode.AM && currentMode != DSPMode.SAM)
                    {
                        targetMode = DSPMode.AM;
                        modeNeedsChange = true;
                    }
                    break;

                case CIVMode.CW:
                    if (currentMode != DSPMode.CWL)
                    {
                        targetMode = DSPMode.CWL;
                        modeNeedsChange = true;
                    }
                    break;

                case CIVMode.CW_R:
                    if (currentMode != DSPMode.CWU)
                    {
                        targetMode = DSPMode.CWU;
                        modeNeedsChange = true;
                    }
                    break;

                case CIVMode.FM:
                    if (currentMode != DSPMode.FM)
                    {
                        targetMode = DSPMode.FM;
                        modeNeedsChange = true;
                    }
                    break;
            }

            Filter targetFilter = CIVProtocol.MapCIVFilterToThetisFilter(targetMode, filter);
            bool filterNeedsChange = (targetFilter != _console.RX1Filter);

            if (!modeNeedsChange && !filterNeedsChange) return;

            _suppressOutgoingUpdates = true;
            try
            {
                _console.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (modeNeedsChange)
                        {
                            _console.RX1DSPMode = targetMode;
                        }
                        if (filterNeedsChange)
                        {
                            _console.RX1Filter = targetFilter;
                        }
                    }
                    finally
                    {
                        _suppressOutgoingUpdates = false;
                    }
                }));
            }
            catch
            {
                _suppressOutgoingUpdates = false;
            }
        }

        private void HandleIncomingDataMode(CIVDataMode dataMode, CIVFilter filter)
        {
            if (_console == null) return;

            DSPMode currentMode = _console.RX1DSPMode;
            DSPMode targetMode = currentMode;
            bool modeNeedsChange = false;

            if (dataMode != CIVDataMode.OFF)
            {
                // Turn digital mode ON: USB -> DIGU, LSB -> DIGL
                if (currentMode == DSPMode.USB)
                {
                    targetMode = DSPMode.DIGU;
                    modeNeedsChange = true;
                }
                else if (currentMode == DSPMode.LSB)
                {
                    targetMode = DSPMode.DIGL;
                    modeNeedsChange = true;
                }
            }
            else
            {
                // Turn digital mode OFF: DIGU -> USB, DIGL -> LSB
                if (currentMode == DSPMode.DIGU)
                {
                    targetMode = DSPMode.USB;
                    modeNeedsChange = true;
                }
                else if (currentMode == DSPMode.DIGL)
                {
                    targetMode = DSPMode.LSB;
                    modeNeedsChange = true;
                }
            }

            Filter targetFilter = CIVProtocol.MapCIVFilterToThetisFilter(targetMode, filter);
            bool filterNeedsChange = (targetFilter != _console.RX1Filter);

            if (!modeNeedsChange && !filterNeedsChange) return;

            _suppressOutgoingUpdates = true;
            try
            {
                _console.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (modeNeedsChange)
                        {
                            _console.RX1DSPMode = targetMode;
                        }
                        if (filterNeedsChange)
                        {
                            _console.RX1Filter = targetFilter;
                        }
                    }
                    finally
                    {
                        _suppressOutgoingUpdates = false;
                    }
                }));
            }
            catch
            {
                _suppressOutgoingUpdates = false;
            }
        }

        private void HandleIncomingPtt(bool tx)
        {
            if (_console == null || tx == _console.MOX) return;

            _suppressOutgoingUpdates = true;
            try
            {
                _console.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        _console.MOX = tx;
                    }
                    finally
                    {
                        _suppressOutgoingUpdates = false;
                    }
                }));
            }
            catch
            {
                _suppressOutgoingUpdates = false;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Stop();

            if (_floodTimer != null)
            {
                _floodTimer.Dispose();
                _floodTimer = null;
            }
        }

        #endregion
    }
}
