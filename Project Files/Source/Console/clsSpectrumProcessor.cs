/*  clsSpectrumProcessor.cs

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
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using ConsoleForm = Thetis.Console;

namespace Thetis
{
    public sealed class clsSpectrumProcessor : IDisposable
    {
        private enum SpectrumSourceType
        {
            Receiver = 0,
            Transmitter = 1
        }

        private const int DefaultPixels = 1024;
        private const int DefaultFrameRate = 15;
        private const int DefaultMaxFftSize = 262144;
        private const int DefaultSampleRate = 192000;
        private const int DefaultFftSize = 4096;
        private const int DefaultWindowType = 4;
        private const int DefaultDetectorTypePan = 0;
        private const int DefaultAverageMode = 0;
        private const double DefaultAverageTau = 0.12;
        private const float EmptyPixelValue = -200.0f;
        private const int DataIndexWrap = 1000000000;
        private static readonly ArrayPool<float> PixelArrayPool = ArrayPool<float>.Shared;

        private readonly ConsoleForm _console;
        private readonly object _endpointsLock = new object();
        private readonly Dictionary<string, SpectrumEndpoint> _endpoints;
        private readonly Thread _workerThread;
        private volatile SpectrumEndpoint[] _endpointSnapshot;

        private volatile bool _workerRunning;
        private bool _disposed;

        public clsSpectrumProcessor(ConsoleForm console)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));

            _console = console;
            _endpoints = new Dictionary<string, SpectrumEndpoint>(StringComparer.OrdinalIgnoreCase);
            _endpointSnapshot = Array.Empty<SpectrumEndpoint>();
            _workerRunning = true;

            SubscribeDelegates();

            _workerThread = new Thread(RunWorker)
            {
                Name = "Spectrum Processor Thread",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _workerThread.Start();
        }

        public bool AddReceiver(int receiverId)
        {
            return AddReceiver(receiverId, DefaultPixels, DefaultFrameRate, DefaultFftSize);
        }

        public bool AddReceiver(int receiverId, int pixels, int frameRate, int fftSize)
        {
            return AddSource(SpectrumSourceType.Receiver, receiverId, pixels, frameRate, fftSize);
        }

        public bool AddTransmitter(int transmitterId)
        {
            return AddTransmitter(transmitterId, DefaultPixels, DefaultFrameRate, DefaultFftSize);
        }

        public bool AddTransmitter(int transmitterId, int pixels, int frameRate, int fftSize)
        {
            return AddSource(SpectrumSourceType.Transmitter, transmitterId, pixels, frameRate, fftSize);
        }

        private bool AddSource(SpectrumSourceType sourceType, int sourceId, int pixels, int frameRate, int fftSize)
        {
            ThrowIfDisposed();
            ValidateSourceId(sourceId);

            pixels = ClampInt(pixels, 64, 32768);
            frameRate = ClampInt(frameRate, 1, 120);
            fftSize = NormalizeFftSize(fftSize);

            string key = MakeSourceKey(sourceType, sourceId);
            SpectrumEndpoint endpoint = new SpectrumEndpoint(sourceType, sourceId, pixels, frameRate, fftSize);

            lock (_endpointsLock)
            {
                if (_endpoints.ContainsKey(key))
                {
                    endpoint.Shutdown();
                    return false;
                }

                _endpoints.Add(key, endpoint);
                UpdateEndpointSnapshotLocked();
            }

            return true;
        }

        public bool RemoveReceiver(int receiverId)
        {
            return RemoveSource(SpectrumSourceType.Receiver, receiverId);
        }

        public bool RemoveTransmitter(int transmitterId)
        {
            return RemoveSource(SpectrumSourceType.Transmitter, transmitterId);
        }

        private bool RemoveSource(SpectrumSourceType sourceType, int sourceId)
        {
            SpectrumEndpoint endpoint;

            if (!TryDetachEndpoint(sourceType, sourceId, out endpoint)) return false;

            endpoint.Shutdown();
            return true;
        }

        public void Clear()
        {
            SpectrumEndpoint[] endpoints;

            lock (_endpointsLock)
            {
                endpoints = _endpointSnapshot;
                _endpoints.Clear();
                _endpointSnapshot = Array.Empty<SpectrumEndpoint>();
            }

            for (int i = 0; i < endpoints.Length; i++)
                endpoints[i].Shutdown();
        }

        private bool ContainsSource(SpectrumSourceType sourceType, int sourceId)
        {
            ValidateSourceId(sourceId);

            lock (_endpointsLock)
            {
                return _endpoints.ContainsKey(MakeSourceKey(sourceType, sourceId));
            }
        }

        public bool SetReceiverPixelResolution(int receiverId, int pixels)
        {
            return SetPixelResolution(SpectrumSourceType.Receiver, receiverId, pixels);
        }

        public bool SetTransmitterPixelResolution(int transmitterId, int pixels)
        {
            return SetPixelResolution(SpectrumSourceType.Transmitter, transmitterId, pixels);
        }

        private bool SetPixelResolution(SpectrumSourceType sourceType, int sourceId, int pixels)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint)) return false;

            endpoint.SetPixels(ClampInt(pixels, 64, 32768));
            return true;
        }

        public bool SetReceiverFrameRate(int receiverId, int frameRate)
        {
            return SetFrameRate(SpectrumSourceType.Receiver, receiverId, frameRate);
        }

        public bool SetTransmitterFrameRate(int transmitterId, int frameRate)
        {
            return SetFrameRate(SpectrumSourceType.Transmitter, transmitterId, frameRate);
        }

        private bool SetFrameRate(SpectrumSourceType sourceType, int sourceId, int frameRate)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint)) return false;

            endpoint.SetFrameRate(ClampInt(frameRate, 1, 120));
            return true;
        }

        public bool SetReceiverFFTSize(int receiverId, int fftSize)
        {
            return SetFFTSize(SpectrumSourceType.Receiver, receiverId, fftSize);
        }

        public bool SetTransmitterFFTSize(int transmitterId, int fftSize)
        {
            return SetFFTSize(SpectrumSourceType.Transmitter, transmitterId, fftSize);
        }

        private bool SetFFTSize(SpectrumSourceType sourceType, int sourceId, int fftSize)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint)) return false;

            endpoint.SetFFTSize(NormalizeFftSize(fftSize));
            return true;
        }

        public bool SetReceiverSampleRate(int receiverId, int sampleRate)
        {
            return SetSampleRate(SpectrumSourceType.Receiver, receiverId, sampleRate);
        }

        public bool SetTransmitterSampleRate(int transmitterId, int sampleRate)
        {
            return SetSampleRate(SpectrumSourceType.Transmitter, transmitterId, sampleRate);
        }

        private bool SetSampleRate(SpectrumSourceType sourceType, int sourceId, int sampleRate)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint)) return false;

            endpoint.SetSampleRate(ValidateSampleRate(sampleRate));
            return true;
        }

        public bool SetReceiverZoomSlider(int receiverId, double zoomSlider)
        {
            return SetZoomSlider(SpectrumSourceType.Receiver, receiverId, zoomSlider);
        }

        public bool SetTransmitterZoomSlider(int transmitterId, double zoomSlider)
        {
            return SetZoomSlider(SpectrumSourceType.Transmitter, transmitterId, zoomSlider);
        }

        private bool SetZoomSlider(SpectrumSourceType sourceType, int sourceId, double zoomSlider)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint)) return false;

            endpoint.SetZoomSlider(ClampZoomSlider(zoomSlider));
            return true;
        }

        public bool SetReceiverPanSlider(int receiverId, double panSlider)
        {
            return SetPanSlider(SpectrumSourceType.Receiver, receiverId, panSlider);
        }

        public bool SetTransmitterPanSlider(int transmitterId, double panSlider)
        {
            return SetPanSlider(SpectrumSourceType.Transmitter, transmitterId, panSlider);
        }

        private bool SetPanSlider(SpectrumSourceType sourceType, int sourceId, double panSlider)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint)) return false;

            endpoint.SetPanSlider(ClampPanSlider(panSlider));
            return true;
        }

        public bool CentreReceiverPan(int receiverId)
        {
            return CentrePan(SpectrumSourceType.Receiver, receiverId);
        }

        public bool CentreTransmitterPan(int transmitterId)
        {
            return CentrePan(SpectrumSourceType.Transmitter, transmitterId);
        }

        private bool CentrePan(SpectrumSourceType sourceType, int sourceId)
        {
            return SetPanSlider(sourceType, sourceId, 0.5);
        }

        public bool SetReceiverEnabled(int receiverId, bool enabled)
        {
            return SetEnabled(SpectrumSourceType.Receiver, receiverId, enabled);
        }

        public bool SetTransmitterEnabled(int transmitterId, bool enabled)
        {
            return SetEnabled(SpectrumSourceType.Transmitter, transmitterId, enabled);
        }

        private bool SetEnabled(SpectrumSourceType sourceType, int sourceId, bool enabled)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint)) return false;

            endpoint.SetEnabled(enabled);
            return true;
        }

        public bool ResetReceiverBuffers(int receiverId)
        {
            return ResetBuffers(SpectrumSourceType.Receiver, receiverId);
        }

        public bool ResetTransmitterBuffers(int transmitterId)
        {
            return ResetBuffers(SpectrumSourceType.Transmitter, transmitterId);
        }

        private bool ResetBuffers(SpectrumSourceType sourceType, int sourceId)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint)) return false;

            endpoint.ResetBuffers();
            return true;
        }

        public bool TryGetReceiverPixels(int receiverId, out float[] pixels, out int dataIndex)
        {
            return TryGetLatestPixels(SpectrumSourceType.Receiver, receiverId, out pixels, out dataIndex);
        }

        public bool TryGetTransmitterPixels(int transmitterId, out float[] pixels, out int dataIndex)
        {
            return TryGetLatestPixels(SpectrumSourceType.Transmitter, transmitterId, out pixels, out dataIndex);
        }

        public bool TryCopyReceiverPixels(int receiverId, float[] destination, out int pixelCount, out int dataIndex)
        {
            return TryCopyLatestPixels(SpectrumSourceType.Receiver, receiverId, destination, out pixelCount, out dataIndex);
        }

        public bool TryCopyTransmitterPixels(int transmitterId, float[] destination, out int pixelCount, out int dataIndex)
        {
            return TryCopyLatestPixels(SpectrumSourceType.Transmitter, transmitterId, destination, out pixelCount, out dataIndex);
        }

        private bool TryGetLatestPixels(SpectrumSourceType sourceType, int sourceId, out float[] pixels, out int dataIndex)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint))
            {
                pixels = null;
                dataIndex = 0;
                return false;
            }

            return endpoint.TryGetLatestPixels(out pixels, out dataIndex);
        }

        private bool TryCopyLatestPixels(SpectrumSourceType sourceType, int sourceId, float[] destination, out int pixelCount, out int dataIndex)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint))
            {
                pixelCount = 0;
                dataIndex = 0;
                return false;
        }

            return endpoint.TryCopyLatestPixels(destination, out pixelCount, out dataIndex);
        }

        private bool TryGetViewport(SpectrumSourceType sourceType, int sourceId, out double zoomSlider, out double panSlider)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint))
            {
                zoomSlider = 0.0;
                panSlider = 0.5;
                return false;
            }

            return endpoint.TryGetViewport(out zoomSlider, out panSlider);
        }

        private bool TryGetSampleRate(SpectrumSourceType sourceType, int sourceId, out int sampleRate)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint))
            {
                sampleRate = 0;
                return false;
            }

            return endpoint.TryGetSampleRate(out sampleRate);
        }

        private bool TryGetFFTSize(SpectrumSourceType sourceType, int sourceId, out int fftSize)
        {
            SpectrumEndpoint endpoint;
            if (!TryGetEndpoint(sourceType, sourceId, out endpoint))
            {
                fftSize = 0;
                return false;
            }

            return endpoint.TryGetFFTSize(out fftSize);
        }

        public Form ShowReceiverTestForm(int receiverId, float? min_dBm = null, float? max_dBm = null)
        {
            return ShowTestForm(SpectrumSourceType.Receiver, receiverId, min_dBm, max_dBm);
        }

        public Form ShowTransmitterTestForm(int transmitterId, float? min_dBm = null, float? max_dBm = null)
        {
            return ShowTestForm(SpectrumSourceType.Transmitter, transmitterId, min_dBm, max_dBm);
        }

        private Form ShowTestForm(SpectrumSourceType sourceType, int sourceId, float? min_dBm = null, float? max_dBm = null)
        {
            ThrowIfDisposed();
            ValidateSourceId(sourceId);

            if (!ContainsSource(sourceType, sourceId))
                AddSource(sourceType, sourceId, DefaultPixels, DefaultFrameRate, DefaultFftSize);

            SpectrumTestForm form = new SpectrumTestForm(this, sourceType, sourceId, min_dBm, max_dBm);
            form.Show();
            return form;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _workerRunning = false;

            if (_workerThread != null && _workerThread.IsAlive)
                _workerThread.Join(250);

            UnsubscribeDelegates();
            Clear();
        }

        private void RunWorker()
        {
            while (_workerRunning)
            {
                SpectrumEndpoint[] endpoints = GetEndpointArray();
                long nowTicks = DateTime.UtcNow.Ticks;
                bool didWork = false;

                for (int i = 0; i < endpoints.Length; i++)
                {
                    if (endpoints[i].ProcessFrame(nowTicks))
                        didWork = true;
                }

                Thread.Sleep(didWork ? 1 : 10);
            }
        }

        private void SubscribeDelegates()
        {
            _console.CentreFrequencyHandlers += OnCentreFrequencyChanged;
            _console.HWSampleRateChangedHandlers += OnHWSampleRateChanged;
            _console.PowerChangeHanders += OnPowerChanged;
        }

        private void UnsubscribeDelegates()
        {
            _console.CentreFrequencyHandlers -= OnCentreFrequencyChanged;
            _console.HWSampleRateChangedHandlers -= OnHWSampleRateChanged;
            _console.PowerChangeHanders -= OnPowerChanged;
        }

        private void OnCentreFrequencyChanged(int rx, double oldFreq, double newFreq, Band band, double offset)
        {
            SpectrumEndpoint[] endpoints = GetEndpointArray();

            for (int i = 0; i < endpoints.Length; i++)
            {
                SpectrumEndpoint endpoint = endpoints[i];

                if (endpoint.SourceType == SpectrumSourceType.Transmitter || endpoint.MatchesReceiverEvent(rx))
                    endpoint.SyncFrequencies();
            }
        }

        private void OnHWSampleRateChanged(int rx, int oldRate, int newRate)
        {
            SpectrumEndpoint[] endpoints = GetEndpointArray();
            int sampleRate = ValidateSampleRate(newRate);

            for (int i = 0; i < endpoints.Length; i++)
            {
                if (endpoints[i].MatchesReceiverEvent(rx))
                    endpoints[i].SetSampleRate(sampleRate);
            }
        }

        private void OnPowerChanged(bool oldPower, bool newPower)
        {
            if (!oldPower || newPower) return;

            SpectrumEndpoint[] endpoints = GetEndpointArray();
            for (int i = 0; i < endpoints.Length; i++)
                endpoints[i].ClearData();
        }

        private SpectrumEndpoint[] GetEndpointArray()
        {
            return _endpointSnapshot;
        }

        private bool TryGetEndpoint(SpectrumSourceType sourceType, int sourceId, out SpectrumEndpoint endpoint)
        {
            endpoint = null;
            if (sourceId < 0) return false;

            lock (_endpointsLock)
            {
                return _endpoints.TryGetValue(MakeSourceKey(sourceType, sourceId), out endpoint);
            }
        }

        private bool TryDetachEndpoint(SpectrumSourceType sourceType, int sourceId, out SpectrumEndpoint endpoint)
        {
            endpoint = null;
            if (sourceId < 0) return false;

            lock (_endpointsLock)
            {
                string key = MakeSourceKey(sourceType, sourceId);
                if (!_endpoints.TryGetValue(key, out endpoint))
                    return false;

                _endpoints.Remove(key);
                UpdateEndpointSnapshotLocked();
                return true;
            }
        }

        private void UpdateEndpointSnapshotLocked()
        {
            if (_endpoints.Count == 0)
            {
                _endpointSnapshot = Array.Empty<SpectrumEndpoint>();
                return;
            }

            SpectrumEndpoint[] endpoints = new SpectrumEndpoint[_endpoints.Count];
            _endpoints.Values.CopyTo(endpoints, 0);
            _endpointSnapshot = endpoints;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
        }

        private static string MakeSourceKey(SpectrumSourceType sourceType, int sourceId)
        {
            return (sourceType == SpectrumSourceType.Receiver ? "R:" : "T:") + sourceId.ToString();
        }

        private static void ValidateSourceId(int sourceId)
        {
            if (sourceId < 0) throw new ArgumentOutOfRangeException(nameof(sourceId), "sourceId must be zero or greater.");
        }

        private static int ClampInt(int value, int minimum, int maximum)
        {
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }

        private static int ValidateSampleRate(int sampleRate)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate), "sampleRate must be greater than zero.");

            return sampleRate;
        }

        private static double ClampZoomSlider(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return 0.0;
            if (value < 0.0) return 0.0;
            if (value > 1.0) return 1.0;
            return value;
        }

        private static double ClampPanSlider(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return 0.5;
            if (value < 0.0) return 0.0;
            if (value > 1.0) return 1.0;
            return value;
        }

        private static int NormalizeFftSize(int fftSize)
        {
            int clamped = ClampInt(fftSize, DefaultFftSize, DefaultMaxFftSize);
            int best = DefaultFftSize;
            int bestDistance = Math.Abs(clamped - best);

            for (int value = DefaultFftSize << 1; value <= DefaultMaxFftSize; value <<= 1)
        {
                int distance = Math.Abs(clamped - value);
                if (distance < bestDistance)
                {
                    best = value;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private sealed class SpectrumEndpoint
        {
            private readonly object _syncRoot = new object();
            private readonly SpectrumSourceType _sourceType;
            private readonly int _sourceId;
            private readonly int _displayId;
            private readonly int _maxFftSize;
            private readonly SpecHPSDR _spec;

            private float[] _latestPixels;
            private float[] _workingPixels;
            private int _pixels;
            private int _frameRate;
            private int _configuredSampleRate;
            private int _fftSize;
            private int _blockSize;
            private int _windowType;
            private int _detectorTypePan;
            private int _averageMode;
            private double _averageTau;
            private bool _averageOn;
            private bool _peakOn;
            private bool _normOneHzPan;
            private double _zoomSlider;
            private double _panSlider;
            private bool _enabled;
            private bool _hasData;
            private bool _shutdown;
            private int _dataIndex;
            private long _nextReadUtcTicks;

            public SpectrumEndpoint(SpectrumSourceType sourceType, int sourceId, int pixels, int frameRate, int fftSize)
            {
                _sourceType = sourceType;
                _sourceId = sourceId;
                _pixels = pixels;
                _frameRate = frameRate;
                _configuredSampleRate = DefaultSampleRate;
                _fftSize = NormalizeFftSize(fftSize);
                _blockSize = ResolveDefaultBlockSize(_configuredSampleRate);
                _windowType = DefaultWindowType;
                _detectorTypePan = DefaultDetectorTypePan;
                _averageMode = DefaultAverageMode;
                _averageTau = DefaultAverageTau;
                _averageOn = false;
                _peakOn = false;
                _normOneHzPan = false;
                _zoomSlider = 0.0;
                _panSlider = 0.5;
                _maxFftSize = DefaultMaxFftSize;
                _enabled = true;

                EnsurePixelBuffersLocked(_pixels);
                ClearPixelBuffersLocked();

                _displayId = cmaster.AllocAnalyzer((int)_sourceType, _sourceId, _maxFftSize);
                if (_displayId < 0)
                    throw new InvalidOperationException("Unable to allocate analyzer for source type " + _sourceType.ToString() + " id " + _sourceId.ToString() + ".");

                _spec = new SpecHPSDR(_displayId)
                {
                    Update = false
                };

                Refresh();
                cmaster.RunAnalyzer(_displayId, 1);
            }

            public SpectrumSourceType SourceType
            {
                get { return _sourceType; }
            }

            public bool MatchesReceiverEvent(int rx)
            {
                return _sourceType == SpectrumSourceType.Receiver && (_sourceId + 1) == rx;
            }

            public void SetPixels(int pixels)
            {
                lock (_syncRoot)
                {
                    if (_shutdown || pixels == _pixels) return;

                    _pixels = pixels;
                    ClearPixelBuffersLocked();

                    _spec.Pixels = _pixels;
                    _spec.resetPixelBuffers();
                    _hasData = false;
                    _nextReadUtcTicks = 0;
                }
            }

            public void SetFrameRate(int frameRate)
            {
                lock (_syncRoot)
                {
                    if (_shutdown || frameRate == _frameRate) return;

                    _frameRate = frameRate;
                    _spec.FrameRate = _frameRate;
                    _spec.resetPixelBuffers();
                    _hasData = false;
                    _nextReadUtcTicks = 0;
                }
            }

            public void SetFFTSize(int fftSize)
            {
                lock (_syncRoot)
                {
                    if (_shutdown || fftSize == _fftSize) return;

                    _fftSize = fftSize;
                    RefreshLocked();
                }
            }

            public void SetSampleRate(int sampleRate)
            {
                lock (_syncRoot)
                {
                    if (_shutdown || sampleRate == _configuredSampleRate) return;

                    _configuredSampleRate = sampleRate;
                    _blockSize = ResolveDefaultBlockSize(_configuredSampleRate);
                    RefreshLocked();
                }
            }

            public void SetZoomSlider(double zoomSlider)
            {
                lock (_syncRoot)
                {
                    if (_shutdown || Math.Abs(_zoomSlider - zoomSlider) < 0.000001) return;

                    _zoomSlider = zoomSlider;
                    _spec.ZoomSlider = _zoomSlider;
                    _spec.resetPixelBuffers();
                    ClearPixelBuffersLocked();
                    _hasData = false;
                    _nextReadUtcTicks = 0;
                }
            }

            public void SetPanSlider(double panSlider)
            {
                lock (_syncRoot)
                {
                    if (_shutdown || Math.Abs(_panSlider - panSlider) < 0.000001) return;

                    _panSlider = panSlider;
                    _spec.PanSlider = _panSlider;
                    _spec.resetPixelBuffers();
                    ClearPixelBuffersLocked();
                    _hasData = false;
                    _nextReadUtcTicks = 0;
                }
            }

            public void SetEnabled(bool enabled)
            {
                lock (_syncRoot)
                {
                    if (_shutdown || enabled == _enabled) return;

                    _enabled = enabled;
                    cmaster.RunAnalyzer(_displayId, _enabled ? 1 : 0);
                    _nextReadUtcTicks = 0;
                }
            }

            public void SyncFrequencies()
            {
                lock (_syncRoot)
                {
                    if (_shutdown) return;

                    _spec.resetPixelBuffers();
                    ClearPixelBuffersLocked();
                    _hasData = false;
                    _nextReadUtcTicks = 0;
                }
            }

            public void Refresh()
            {
                lock (_syncRoot)
                {
                    if (_shutdown) return;

                    RefreshLocked();
                }
            }

            public void ResetBuffers()
            {
                lock (_syncRoot)
                {
                    if (_shutdown) return;

                    _spec.resetPixelBuffers();
                    ClearPixelBuffersLocked();
                    _hasData = false;
                    _nextReadUtcTicks = 0;
                }
            }

            public void ClearData()
            {
                lock (_syncRoot)
                {
                    ClearPixelBuffersLocked();
                    _hasData = false;
                    _nextReadUtcTicks = 0;
                }
            }

            public bool ProcessFrame(long nowUtcTicks)
            {
                lock (_syncRoot)
                {
                    if (_shutdown || !_enabled) return false;
                    if (nowUtcTicks < _nextReadUtcTicks) return false;

                    _nextReadUtcTicks = nowUtcTicks + (TimeSpan.TicksPerSecond / Math.Max(1, _frameRate));

                    int flag = 0;
                    unsafe
                    {
                        fixed (float* ptr = &_workingPixels[0])
                            SpecHPSDRDLL.GetPixels(_displayId, 0, ptr, ref flag);
                    }

                    if (flag != 1) return false;

                    float offset = GetPixelOffset();
                    if (offset != 0.0f)
                    {
                        for (int i = 0; i < _pixels; i++)
                            _workingPixels[i] += offset;
                    }

                    float[] temp = _latestPixels;
                    _latestPixels = _workingPixels;
                    _workingPixels = temp;

                    _hasData = true;
                    _dataIndex++;
                    if (_dataIndex >= DataIndexWrap) _dataIndex = 1;

                    return true;
                }
            }

            public bool TryGetLatestPixels(out float[] pixels, out int dataIndex)
            {
                lock (_syncRoot)
                {
                    pixels = new float[_pixels];
                    Buffer.BlockCopy(_latestPixels, 0, pixels, 0, _pixels * sizeof(float));
                    dataIndex = _dataIndex;
                    return _hasData;
                }
            }

            public bool TryCopyLatestPixels(float[] destination, out int pixelCount, out int dataIndex)
            {
                if (destination == null) throw new ArgumentNullException(nameof(destination));

                lock (_syncRoot)
                {
                    pixelCount = _pixels;
                    dataIndex = _dataIndex;

                    if (destination.Length < _pixels)
                        return false;

                    Buffer.BlockCopy(_latestPixels, 0, destination, 0, _pixels * sizeof(float));
                    return _hasData;
                }
            }

            public bool TryGetViewport(out double zoomSlider, out double panSlider)
            {
                lock (_syncRoot)
                {
                    zoomSlider = _zoomSlider;
                    panSlider = _panSlider;
                    return !_shutdown;
                }
            }

            public bool TryGetSampleRate(out int sampleRate)
            {
                lock (_syncRoot)
                {
                    sampleRate = _configuredSampleRate;
                    return !_shutdown;
                }
            }

            public bool TryGetFFTSize(out int fftSize)
            {
                lock (_syncRoot)
                {
                    fftSize = _fftSize;
                    return !_shutdown;
                }
            }

            public void Shutdown()
            {
                lock (_syncRoot)
                {
                    if (_shutdown) return;

                    _shutdown = true;
                    _enabled = false;

                    try { cmaster.RunAnalyzer(_displayId, 0); } catch { }
                    try { cmaster.FreeAnalyzer(_displayId); } catch { }
                    ReturnPixelBuffer(ref _latestPixels);
                    ReturnPixelBuffer(ref _workingPixels);
                }
            }

            private void RefreshLocked()
            {
                _spec.Update = false;
                _spec.PixelOut = 1;
                _spec.IgnoreFrequencyOffset = true;
                _spec.FrameRate = _frameRate;
                _spec.Pixels = _pixels;
                _spec.DetTypePan = _detectorTypePan;
                _spec.AvTau = _averageTau;
                _spec.FFTSize = Math.Min(_fftSize, _maxFftSize);
                _spec.BlockSize = _blockSize;
                _spec.SampleRate = _configuredSampleRate;
                _spec.WindowType = _windowType;
                _spec.AverageMode = _averageMode;
                _spec.AverageOn = _averageOn;
                _spec.PeakOn = _peakOn;
                _spec.NormOneHzPan = _normOneHzPan;
                _spec.ZoomSlider = _zoomSlider;
                _spec.PanSlider = _panSlider;

                _spec.Update = true;
                _spec.initAnalyzer();
                _spec.resetPixelBuffers();

                _hasData = false;
                _nextReadUtcTicks = 0;
                ClearPixelBuffersLocked();
            }

            private static int ResolveDefaultBlockSize(int sampleRate)
            {
                int blockSize = cmaster.GetBuffSize(sampleRate);
                return blockSize > 0 ? blockSize : 2048;
            }

            private float GetPixelOffset()
            {
                switch (_sourceType)
                {
                    case SpectrumSourceType.Transmitter:
                        return Display.TXDisplayCalOffset;
                    case SpectrumSourceType.Receiver:
                        return _sourceId == 1 ? Display.RX2OffsetWithDup : Display.RX1OffsetWithDup;
                    default:
                        return 0.0f;
                }
            }

            private void EnsurePixelBuffersLocked(int pixels)
            {
                if (_latestPixels == null || _latestPixels.Length < pixels)
                {
                    ReturnPixelBuffer(ref _latestPixels);
                    _latestPixels = RentPixelBuffer(pixels);
                }

                if (_workingPixels == null || _workingPixels.Length < pixels)
            {
                    ReturnPixelBuffer(ref _workingPixels);
                    _workingPixels = RentPixelBuffer(pixels);
                }
            }

            private void ClearPixelBuffersLocked()
            {
                EnsurePixelBuffersLocked(_pixels);
                FillPixelBuffer(_latestPixels, _pixels);
                FillPixelBuffer(_workingPixels, _pixels);
            }

            private static float[] RentPixelBuffer(int pixels)
            {
                return PixelArrayPool.Rent(pixels);
            }

            private static void ReturnPixelBuffer(ref float[] buffer)
            {
                if (buffer == null) return;

                PixelArrayPool.Return(buffer, false);
                buffer = null;
            }

            private static void FillPixelBuffer(float[] data, int pixels)
            {
                for (int i = 0; i < pixels; i++)
                    data[i] = EmptyPixelValue;
            }
        }

        private sealed class SpectrumTestForm : Form
        {
            private readonly clsSpectrumProcessor _processor;
            private readonly SpectrumSourceType _sourceType;
            private readonly int _sourceId;
            private readonly float? _minDbm;
            private readonly float? _maxDbm;
            private readonly SpectrumGraphPanel _graphPanel;
            private readonly System.Windows.Forms.Timer _refreshTimer;

            public SpectrumTestForm(clsSpectrumProcessor processor, SpectrumSourceType sourceType, int sourceId, float? minDbm, float? maxDbm)
            {
                _processor = processor;
                _sourceType = sourceType;
                _sourceId = sourceId;
                _minDbm = minDbm;
                _maxDbm = maxDbm;

                Text = BuildTitle(sourceType, sourceId, _minDbm, _maxDbm);
                StartPosition = FormStartPosition.CenterScreen;
                Size = new Size(900, 450);
                MinimumSize = new Size(320, 200);
                BackColor = Color.FromArgb(18, 18, 18);

                _graphPanel = new SpectrumGraphPanel(_processor, _sourceType, _sourceId, _minDbm, _maxDbm)
                {
                    Dock = DockStyle.Fill
                };

                Controls.Add(_graphPanel);

                _refreshTimer = new System.Windows.Forms.Timer
                {
                    Interval = 50
                };
                _refreshTimer.Tick += RefreshTimer_Tick;
                _refreshTimer.Start();

                FormClosed += SpectrumTestForm_FormClosed;
            }

            private static string BuildTitle(SpectrumSourceType sourceType, int sourceId, float? minDbm, float? maxDbm)
            {
                string suffix = string.Empty;
                if (minDbm.HasValue && maxDbm.HasValue && maxDbm.Value > minDbm.Value)
                    suffix = " [" + minDbm.Value.ToString("F0") + " to " + maxDbm.Value.ToString("F0") + " dBm]";

                if (sourceType == SpectrumSourceType.Receiver)
                    return "Spectrum Test - RX" + (sourceId + 1).ToString() + suffix;

                return "Spectrum Test - TX" + sourceId.ToString() + suffix;
            }

            private void RefreshTimer_Tick(object sender, EventArgs e)
            {
                _graphPanel.Invalidate();
            }

            private void SpectrumTestForm_FormClosed(object sender, FormClosedEventArgs e)
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
            }
        }

        private sealed class SpectrumGraphPanel : Panel
        {
            private readonly clsSpectrumProcessor _processor;
            private readonly SpectrumSourceType _sourceType;
            private readonly int _sourceId;
            private readonly float? _fixedMinDbm;
            private readonly float? _fixedMaxDbm;
            private readonly Pen _tracePen;
            private readonly Pen _gridPen;
            private readonly Brush _textBrush;
            private readonly Brush _backgroundBrush;
            private readonly Font _font;
            private float[] _pixelBuffer;
            private PointF[] _pointBuffer;

            public SpectrumGraphPanel(clsSpectrumProcessor processor, SpectrumSourceType sourceType, int sourceId, float? fixedMinDbm, float? fixedMaxDbm)
            {
                _processor = processor;
                _sourceType = sourceType;
                _sourceId = sourceId;
                _fixedMinDbm = fixedMinDbm;
                _fixedMaxDbm = fixedMaxDbm;

                DoubleBuffered = true;
                ResizeRedraw = true;

                _tracePen = new Pen(Color.LimeGreen, 1.5f);
                _gridPen = new Pen(Color.FromArgb(45, 90, 90, 90), 1f);
                _textBrush = Brushes.Gainsboro;
                _backgroundBrush = new SolidBrush(Color.FromArgb(18, 18, 18));
                _font = new Font("Segoe UI", 9f, FontStyle.Regular);
                _pixelBuffer = new float[DefaultPixels];
                _pointBuffer = new PointF[DefaultPixels];
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _tracePen.Dispose();
                    _gridPen.Dispose();
                    _backgroundBrush.Dispose();
                    _font.Dispose();
                }

                base.Dispose(disposing);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Graphics g = e.Graphics;
                g.Clear(Color.FromArgb(18, 18, 18));

                Rectangle plotRect = ClientRectangle;
                plotRect.Inflate(-12, -12);

                if (plotRect.Width < 10 || plotRect.Height < 10)
                    return;

                DrawGrid(g, plotRect);

                float[] pixels = _pixelBuffer;
                int pixelCount;
                int dataIndex;
                bool hasData = TryCopyPixels(out pixels, out pixelCount, out dataIndex);
                if (!hasData || pixels == null || pixelCount < 2)
                {
                    g.DrawString("Waiting for pixel data...", _font, _textBrush, plotRect.Left, plotRect.Top);
                    return;
                }

                float min = float.MaxValue;
                float max = float.MinValue;

                for (int i = 0; i < pixelCount; i++)
                {
                    if (pixels[i] < min) min = pixels[i];
                    if (pixels[i] > max) max = pixels[i];
                }

                if (min == float.MaxValue || max == float.MinValue)
                {
                    g.DrawString("No valid dBm data.", _font, _textBrush, plotRect.Left, plotRect.Top);
                    return;
                }

                float plotMin = min;
                float plotMax = max;
                bool fixedScale = _fixedMinDbm.HasValue && _fixedMaxDbm.HasValue && _fixedMaxDbm.Value > _fixedMinDbm.Value;
                if (fixedScale)
                {
                    plotMin = _fixedMinDbm.Value;
                    plotMax = _fixedMaxDbm.Value;
                }

                if (Math.Abs(plotMax - plotMin) < 0.001f)
                {
                    plotMax += 1.0f;
                    plotMin -= 1.0f;
                }

                EnsurePointBuffer(pixelCount);
                PointF[] points = _pointBuffer;
                float width = plotRect.Width - 1f;
                float height = plotRect.Height - 1f;
                float range = plotMax - plotMin;

                for (int i = 0; i < pixelCount; i++)
                {
                    float x = plotRect.Left + (width * i / (pixelCount - 1f));
                    float normalized = (pixels[i] - plotMin) / range;
                    if (normalized < 0f) normalized = 0f;
                    if (normalized > 1f) normalized = 1f;
                    float y = plotRect.Bottom - (normalized * height);
                    points[i] = new PointF(x, y);
                }

                g.DrawLines(_tracePen, points);

                string sourceText = _sourceType == SpectrumSourceType.Receiver
                    ? "RX" + (_sourceId + 1).ToString()
                    : "TX" + _sourceId.ToString();
                double zoomSlider;
                double panSlider;
                bool hasViewport = _processor.TryGetViewport(_sourceType, _sourceId, out zoomSlider, out panSlider);
                int sampleRate;
                bool hasSampleRate = _processor.TryGetSampleRate(_sourceType, _sourceId, out sampleRate);
                int fftSize;
                bool hasFftSize = _processor.TryGetFFTSize(_sourceType, _sourceId, out fftSize);

                string info = sourceText +
                    "  Pixels=" + pixelCount.ToString() +
                    "  Frame=" + dataIndex.ToString() +
                    "  Min=" + min.ToString("F1") +
                    " dBm  Max=" + max.ToString("F1") + " dBm";

                if (hasViewport)
                {
                    info += "  Zoom=" + zoomSlider.ToString("F3") +
                        "  Pan=" + panSlider.ToString("F3");
                }

                if (hasSampleRate)
                {
                    info += "  SR=" + sampleRate.ToString();
                }

                if (hasFftSize)
                {
                    info += "  FFT=" + fftSize.ToString();
                }

                if (fixedScale)
                {
                    info += "  Scale=" + plotMin.ToString("F1") + " to " + plotMax.ToString("F1") + " dBm";
                }

                SizeF textSize = g.MeasureString(info, _font);
                RectangleF textRect = new RectangleF(plotRect.Left, plotRect.Top, Math.Min(textSize.Width + 8f, plotRect.Width), textSize.Height + 4f);
                g.FillRectangle(_backgroundBrush, textRect);
                g.DrawString(info, _font, _textBrush, plotRect.Left + 2, plotRect.Top + 2);
            }

            private bool TryCopyPixels(out float[] pixels, out int pixelCount, out int dataIndex)
            {
                if (_pixelBuffer == null)
                    _pixelBuffer = new float[DefaultPixels];

                bool hasData = TryCopyPixelsWithBuffer(_pixelBuffer, out pixelCount, out dataIndex);
                if (pixelCount > _pixelBuffer.Length)
                {
                    _pixelBuffer = new float[pixelCount];
                    hasData = TryCopyPixelsWithBuffer(_pixelBuffer, out pixelCount, out dataIndex);
                }

                pixels = _pixelBuffer;
                return hasData;
            }

            private bool TryCopyPixelsWithBuffer(float[] destination, out int pixelCount, out int dataIndex)
            {
                return _sourceType == SpectrumSourceType.Receiver
                    ? _processor.TryCopyReceiverPixels(_sourceId, destination, out pixelCount, out dataIndex)
                    : _processor.TryCopyTransmitterPixels(_sourceId, destination, out pixelCount, out dataIndex);
            }

            private void EnsurePointBuffer(int pixelCount)
            {
                if (_pointBuffer == null || _pointBuffer.Length != pixelCount)
                    _pointBuffer = new PointF[pixelCount];
            }

            private void DrawGrid(Graphics g, Rectangle plotRect)
            {
                const int verticalDivisions = 8;
                const int horizontalDivisions = 6;

                for (int i = 0; i <= verticalDivisions; i++)
                {
                    float x = plotRect.Left + (plotRect.Width * i / (float)verticalDivisions);
                    g.DrawLine(_gridPen, x, plotRect.Top, x, plotRect.Bottom);
                }

                for (int i = 0; i <= horizontalDivisions; i++)
                {
                    float y = plotRect.Top + (plotRect.Height * i / (float)horizontalDivisions);
                    g.DrawLine(_gridPen, plotRect.Left, y, plotRect.Right, y);
                }
            }
        }
    }
}
