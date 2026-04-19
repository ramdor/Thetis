/*  TCIServer.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2026 Richard Samphire MW0LGE

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


// info from
// https://www.codeproject.com/Articles/5733/A-TCP-IP-Server-written-in-C
// https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
// https://stackoverflow.com/questions/10200910/creating-a-hello-world-websocket-example
// https://github.com/ExpertSDR3/TCI

// from my SunSDR2PRO as reference
// protocol: ExpertSDR3,1.8;
// device: SunSDR2PRO;
// receive_only: false;
// trx_count: 2;
// channels_count: 2;
// vfo_limits: 0.0,160000000.0;
// if_limits: -156250,156250;
// modulations_list: AM,LSB,USB,CW,NFM,WSPR,FT8,FT4,JT65,JT9,RTTY,BPSK,DIGL,DIGU,WFM,DRM;
// iq_samplerate: 48000;
// audio_samplerate: 48000;
// volume: -8;
// mute: false;
// mon_volume: -45;
// mon_enable: false;
// digl_offset: 0;
// digu_offset: 0;
// dds: 0,144559730;
// dds: 1,1950000;
// if:0,0,106320;
// if:0,1,-429530;
// if:1,0,0;
// if:1,1,0;
// vfo: 0,0,144666050;
// vfo: 0,1,144130200;
// vfo: 1,0,1950000;
// vfo: 1,1,1950000;
// modulation: 0,USB;
// modulation: 1,AM;
// agc_mode: 0,normal;
// agc_gain: 0,100;
// agc_mode: 1,normal;
// agc_gain: 1,80;
// rx_filter_band: 0,10,3960;
// rx_filter_band: 1,-3000,3000;
// rx_nb_enable: 0,false;
// rx_nb_param: 0,70,20;
// rx_nb_enable: 1,false;
// rx_nb_param: 1,70,20;
// rx_bin_enable: 0,false;
// rx_bin_enable: 1,false;
// rx_nr_enable: 0,false;
// rx_nr_enable: 1,false;
// rx_anc_enable: 0,false;
// rx_anc_enable: 1,false;
// rx_anf_enable: 0,false;
// rx_anf_enable: 1,false;
// rx_apf_enable: 0,false;
// rx_apf_enable: 1,false;
// rx_dse_enable: 0,false;
// rx_dse_enable: 1,false;
// rx_nf_enable: 0,false;
// rx_nf_enable: 1,false;
// rx_enable: 1,false;
// tx_enable: 0,true;
// tx_enable: 1,true;
// lock:0,false;
// lock:1,false;
// rx_channel_enable: 0,1,false;
// rit_enable: 0,false;
// xit_enable: 0,false;
// split_enable: 0,false;
// rit_offset: 0,0;
// xit_offset: 0,0;
// sql_enable: 0,false;
// sql_level: 0,-80;
// rx_mute: 0,false;
// rx_volume: 0,0,0;
// rx_balance: 0,0,0;
// rx_volume: 0,1,0;
// rx_balance: 0,1,0;
// rx_channel_enable: 1,1,false;
// rit_enable: 1,false;
// xit_enable: 1,false;
// split_enable: 1,false;
// rit_offset: 1,0;
// xit_offset: 1,0;
// sql_enable: 1,false;
// sql_level: 1,-80;
// rx_mute: 1,false;
// rx_volume: 1,0,0;
// rx_balance: 1,0,0;
// rx_volume: 1,1,0;
// rx_balance: 1,1,0;
// cw_macros_speed: 30;
// cw_macros_delay: 10;
// cw_keyer_speed: 30;
// drive: 0,0;
// tune_drive: 0,100;
// drive: 1,50;
// tune_drive: 1,50;
// trx: 0,false;
// tune: 0,false;
// iq_stop: 0;
// trx: 1,false;
// tune: 1,false;
// iq_stop: 1;
// app_focus: false;
// tx_frequency: 1900000;
// stop;
// ready;
//

//from initial connect to ExpertSDR3 version 1.1.7
//protocol: ExpertSDR3,2.0;
//device: SunSDR2PRO;
//receive_only: false;
//trx_count: 2;
//channels_count: 2;
//vfo_limits: 0,160000000;
//if_limits: -19531,19531;
//modulations_list: AM,LSB, USB, CW, NFM, WSPR, FT8, FT4, JT65, JT9, RTTY, BPSK, DIGL, DIGU, WFM, DRM;
//iq_samplerate: 48000;
//audio_samplerate: 48000;
//volume: 0;
//mute: false;
//mon_volume: 0;
//mon_enable: false;
//digl_offset: 0;
//digu_offset: 0;
//dds: 0,3520000;
//dds: 1,3520000;
//if:0,0,0;
//if:0,1,0;
//if:1,0,0;
//if:1,1,0;
//vfo: 0,0,3520000;
//vfo: 0,1,3520000;
//vfo: 1,0,3520000;
//vfo: 1,1,3520000;
//modulation: 0,LSB;
//modulation: 1,LSB;
//agc_mode: 0,normal;
//agc_gain: 0,100;
//agc_mode: 1,normal;
//agc_gain: 1,100;
//rx_filter_band: 0,-3500,-40;
//rx_filter_band: 1,-3500,-40;
//rx_nb_enable: 0,false;
//rx_nb_param: 0,70,20;
//rx_nb_enable: 1,false;
//rx_nb_param: 1,70,20;
//rx_bin_enable: 0,false;
//rx_bin_enable: 1,false;
//rx_nr_enable: 0,false;
//rx_nr_enable: 1,false;
//rx_anc_enable: 0,false;
//rx_anc_enable: 1,false;
//rx_anf_enable: 0,false;
//rx_anf_enable: 1,false;
//rx_apf_enable: 0,false;
//rx_apf_enable: 1,false;
//rx_dse_enable: 0,false;
//rx_dse_enable: 1,false;
//rx_nf_enable: 0,false;
//rx_nf_enable: 1,false;
//rx_enable: 1,false;
//tx_enable: 0,true;
//tx_enable: 1,true;
//lock:0,false;
//lock:1,false;
//rx_channel_enable: 0,1,false;
//rit_enable: 0,false;
//xit_enable: 0,false;
//split_enable: 0,false;
//rit_offset: 0,0;
//xit_offset: 0,0;
//sql_enable: 0,false;
//sql_level: 0,-80;
//rx_mute: 0,false;
//rx_volume: 0,0,0;
//rx_balance: 0,0,0;
//rx_volume: 0,1,0;
//rx_balance: 0,1,0;
//rx_channel_enable: 1,1,false;
//rit_enable: 1,false;
//xit_enable: 1,false;
//split_enable: 1,false;
//rit_offset: 1,0;
//xit_offset: 1,0;
//sql_enable: 1,false;
//sql_level: 1,-80;
//rx_mute: 1,false;
//rx_volume: 1,0,0;
//rx_balance: 1,0,0;
//rx_volume: 1,1,0;
//rx_balance: 1,1,0;
//cw_macros_speed: 30;
//cw_macros_delay: 10;
//cw_keyer_speed: 30;
//drive: 0,0;
//tune_drive: 0,0;
//drive: 1,0;
//tune_drive: 1,0;
//trx: 0,false;
//tune: 0,false;
//iq_stop: 0;
//trx: 1,false;
//tune: 1,false;
//iq_stop: 1;
//app_focus: false;
//tx_frequency: 3520000;
//audio_stream_sample_type: float32;
//audio_stream_channels: 2;
//start;
//vfo_lock: 0,0,false;
//vfo_lock: 0,1,false;
//vfo_lock: 1,0,false;
//vfo_lock: 1,1,false;
//ready;

//from Thetis 2.10.3.9 dev3n5
//protocol: ExpertSDR3,2.0;
//device: SunSDR2PRO;
//receive_only: false;
//trx_count: 2;
//channels_count: 2;
//vfo_limits: 0,61440000;
//if_limits: -192000,192000;
//modulations_list: AM,SAM, DSB, LSB, USB, NFM, FM, DIGL, DIGU, CWL, CWU;
//dds: 0,7074000;
//dds: 1,14026729;
//if:0,0,3532;
//if:0,1,0;
//if:1,1,600;
//if:1,1,600;
//vfo: 0,0,7070468;
//vfo: 0,1,14027329;
//vfo: 1,0,14027329;
//vfo: 1,1,14027329;
//tx_frequency: 7070468;
//tx_frequency_thetis: 7070468,b40m,false,false;    /// THIS HAS CHANGED TO tx_frequency_ex as of 2.10.3.14
//modulation: 0,DIGU;
//modulation: 1,CWU;
//rx_filter_band: 0,0,3000;
//rx_filter_band: 1,475,725;
//rx_enable: 0,true;
//rx_enable: 1,false;
//split_enable: 0,false;
//split_enable: 1,false;
//tx_enable: 0,true;
//tx_enable: 1,false;
//trx: 0,false;
//trx: 1,false;
//tune: 0,false;
//tune: 1,false;
//iq_samplerate: 384000;
//audio_samplerate: 48000;
//audio_stream_sample_type: float32;
//audio_stream_channels: 2;
//audio_stream_samples: 2048;
//tx_stream_audio_buffering: 50;
//mute: false;
//rx_mute: 0,false;
//rx_mute: 1,false;
//mon_enable: false;
//mon_volume: -30.0;
//start;
//ready;


using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Globalization;

namespace Thetis
{
    public enum TCICWSpotForce
    {
        CWU = 0,
        CWL,
        DEFAULT = 99
    }

    public enum TCITxStereoInputMode
    {
        Left = 0,
        Right,
        Both
    }

    internal enum TCIStreamType : uint
    {
        IQ_STREAM = 0,
        RX_AUDIO_STREAM = 1,
        TX_AUDIO_STREAM = 2,
        TX_CHRONO = 3,
        LINEOUT_STREAM = 4
    }

    internal enum TCISampleType : uint
    {
        INT16 = 0,
        INT24 = 1,
        INT32 = 2,
        FLOAT32 = 3
    }


    internal sealed class TCIQueuedTxAudio
    {
        public int Receiver;
        public int SampleRate;
        public TCISampleType SampleType;
        public int Channels;
        public int ComplexSamples;
        public double[] Samples;
    }

    internal sealed class TCIPendingFloatBuffer
    {
        private float[] m_buffer;
        private int m_readIndex;
        private int m_count;

        public TCIPendingFloatBuffer(int initialCapacity = 16)
        {
            m_buffer = new float[Math.Max(16, initialCapacity)];
        }

        public int Count
        {
            get { return m_count; }
        }

        public void Enqueue(float[] source, int sourceOffset, int count)
        {
            if (source == null || count <= 0)
                return;

            ensureCapacity(count);
            Array.Copy(source, sourceOffset, m_buffer, m_readIndex + m_count, count);
            m_count += count;
        }

        public void CopyTo(float[] destination, int destinationOffset, int count)
        {
            if (count <= 0)
                return;

            Array.Copy(m_buffer, m_readIndex, destination, destinationOffset, count);
        }

        public float Peek(int index)
        {
            return m_buffer[m_readIndex + index];
        }

        public void Advance(int count)
        {
            if (count <= 0)
                return;

            if (count >= m_count)
            {
                m_readIndex = 0;
                m_count = 0;
                return;
            }

            m_readIndex += count;
            m_count -= count;

            if (m_count == 0)
            {
                m_readIndex = 0;
            }
            else if (m_readIndex >= m_buffer.Length / 2)
            {
                Array.Copy(m_buffer, m_readIndex, m_buffer, 0, m_count);
                m_readIndex = 0;
            }
        }

        private void ensureCapacity(int additionalCount)
        {
            int requiredCount = m_count + additionalCount;
            if (m_readIndex + requiredCount <= m_buffer.Length)
                return;

            if (requiredCount <= m_buffer.Length)
            {
                Array.Copy(m_buffer, m_readIndex, m_buffer, 0, m_count);
                m_readIndex = 0;
                return;
            }

            int newLength = m_buffer.Length;
            while (newLength < requiredCount)
                newLength *= 2;

            float[] newBuffer = new float[newLength];
            if (m_count > 0)
                Array.Copy(m_buffer, m_readIndex, newBuffer, 0, m_count);

            m_buffer = newBuffer;
            m_readIndex = 0;
        }
    }

    public sealed class clsTCISensorManager
    {
        private sealed class clsRxReadingState
        {
            public double Signal = -200.0;
            public double AvgSignal = -200.0;
            public double PeakBinSignal = -200.0;
            public bool Updated = false;
        }

        private sealed class clsTxReadingState
        {
            public double MicLevelDbm = -200.0;
            public double PowerWatts = 0.0;
            public double PeakPowerWatts = 0.0;
            public double Swr = 1.0;
            public bool Updated = false;
        }

        private readonly object _lock = new object();
        private readonly clsRxReadingState[,] _rxChannelReadings = new clsRxReadingState[2, 2];
        private readonly clsTxReadingState _txReadings = new clsTxReadingState();
        private bool _rxSensorsEnabled = false;
        private bool _txSensorsEnabled = false;
        private int _rxIntervalMs = 200;
        private int _txIntervalMs = 200;

        public clsTCISensorManager()
        {
            for (int rx = 0; rx < 2; rx++)
            {
                for (int channel = 0; channel < 2; channel++)
                {
                    _rxChannelReadings[rx, channel] = new clsRxReadingState();
                }
            }
        }

        private static int clampIntervalMs(int intervalMs)
        {
            if (intervalMs < 30) return 30;
            if (intervalMs > 1000) return 1000;
            return intervalMs;
        }

        public int RxIntervalMs
        {
            get { lock (_lock) return _rxIntervalMs; }
        }

        public int TxIntervalMs
        {
            get { lock (_lock) return _txIntervalMs; }
        }

        public bool RxSensorsEnabled
        {
            get { lock (_lock) return _rxSensorsEnabled; }
        }

        public bool TxSensorsEnabled
        {
            get { lock (_lock) return _txSensorsEnabled; }
        }

        public void ConfigureRxSensors(bool enabled, int intervalMs)
        {
            lock (_lock)
            {
                _rxSensorsEnabled = enabled;
                _rxIntervalMs = clampIntervalMs(intervalMs);
                for (int rx = 0; rx < 2; rx++)
                {
                    for (int channel = 0; channel < 2; channel++)
                    {
                        clsRxReadingState state = _rxChannelReadings[rx, channel];
                        state.Updated = false;
                    }
                }
            }
        }

        public void ConfigureTxSensors(bool enabled, int intervalMs)
        {
            lock (_lock)
            {
                _txSensorsEnabled = enabled;
                _txIntervalMs = clampIntervalMs(intervalMs);
                _txReadings.Updated = false;
            }
        }

        public bool RequiresRxChannelUpdate(int receiver, int channel)
        {
            lock (_lock)
            {
                if (!_rxSensorsEnabled) return false;
                if (receiver < 0 || receiver > 1 || channel < 0 || channel > 1) return false;

                clsRxReadingState state = _rxChannelReadings[receiver, channel];
                return !state.Updated;
            }
        }

        public bool RequiresTxUpdate()
        {
            lock (_lock)
            {
                return _txSensorsEnabled && !_txReadings.Updated;
            }
        }

        public bool SensorRequiresUpdate(int receiver, Reading reading)
        {
            lock (_lock)
            {
                switch (reading)
                {
                    case Reading.AVG_SIGNAL_STRENGTH:
                    case Reading.SIGNAL_MAX_BIN:
                    case Reading.SIGNAL_STRENGTH:                    
                        {
                            int rx = receiver - 1;
                            if (rx < 0 || rx > 1) return false;
                            clsRxReadingState state = _rxChannelReadings[rx, 0];
                            return _rxSensorsEnabled && !state.Updated;
                        }
                    case Reading.MIC:
                    case Reading.PWR:
                    case Reading.SWR:
                        return _txSensorsEnabled && !_txReadings.Updated;
                    default:
                        return false;
                }
            }
        }

        public void SetRxChannelReading(int receiver, int channel, double signal, double avg_signal, double peak_bin_signal)
        {
            lock (_lock)
            {
                if (receiver < 0 || receiver > 1 || channel < 0 || channel > 1) return;

                clsRxReadingState state = _rxChannelReadings[receiver, channel];
                state.Signal = signal;
                state.AvgSignal = avg_signal;
                state.PeakBinSignal = peak_bin_signal;
                state.Updated = true;
            }
        }

        public void SetTxReadings(double micLevelDbm, double powerWatts, double peakPowerWatts, double swr)
        {
            lock (_lock)
            {
                _txReadings.MicLevelDbm = micLevelDbm;
                _txReadings.PowerWatts = powerWatts;
                _txReadings.PeakPowerWatts = peakPowerWatts;
                _txReadings.Swr = swr;
                _txReadings.Updated = true;
            }
        }

        public bool TryGetRxChannelReadingForSend(int receiver, int channel, out double signal, out double avg_signal, out double peak_bin_signal)
        {
            lock (_lock)
            {
                signal = -200.0;
                avg_signal = -200.0;
                peak_bin_signal = -200.0;
                if (!_rxSensorsEnabled) return false;
                if (receiver < 0 || receiver > 1 || channel < 0 || channel > 1) return false;

                clsRxReadingState state = _rxChannelReadings[receiver, channel];
                if (!state.Updated) return false;

                signal = state.Signal;
                avg_signal = state.AvgSignal;
                peak_bin_signal= state.PeakBinSignal;
                return true;
            }
        }

        public void ConsumeRxChannelReading(int receiver, int channel)
        {
            lock (_lock)
            {
                if (receiver < 0 || receiver > 1 || channel < 0 || channel > 1) return;

                clsRxReadingState state = _rxChannelReadings[receiver, channel];
                state.Updated = false;
            }
        }

        public bool TryGetTxReadingsForSend(out double micLevelDbm, out double powerWatts, out double peakPowerWatts, out double swr)
        {
            lock (_lock)
            {
                micLevelDbm = -200.0;
                powerWatts = 0.0;
                peakPowerWatts = 0.0;
                swr = 1.0;

                if (!_txSensorsEnabled || !_txReadings.Updated) return false;

                micLevelDbm = _txReadings.MicLevelDbm;
                powerWatts = _txReadings.PowerWatts;
                peakPowerWatts = _txReadings.PeakPowerWatts;
                swr = _txReadings.Swr;
                return true;
            }
        }

        public void ConsumeTxReadings()
        {
            lock (_lock)
            {
                _txReadings.Updated = false;
            }
        }
    }

    public class TCPIPtciSocketListener
	{
        private enum TCIOutboundPriority
        {
            Urgent = 0,
            Binary = 1,
            Control = 2
        }

        private const int SOCKET_READ_TIMEOUT_MS = 250;
        private const int SOCKET_READ_BUFFER_SIZE = 8192;

        private sealed class TCIOutboundFrame
        {
            public byte[] Frame;
            public string LogText;
        }

        private sealed class TCIRxAudioResamplerState
        {
            public int InputRate;
            public int OutputRate;
            public IntPtr LeftResampler = IntPtr.Zero;
            public IntPtr RightResampler = IntPtr.Zero;
        }

		//
		public delegate void ClientConnected();
		public delegate void ClientDisconnected();
		public delegate void ClientError(SocketException se);
		public ClientConnected ClientConnectedHandlers;
		public ClientDisconnected ClientDisconnectedHandlers;
		public ClientError ClientErrorHandlers;
        //

        public struct VFOData
        {
            public double freqMHz;
            public int offsetHz;
            public double centreMHz;
            public int chan;
            public int duplicate_tochan;
            public bool replace_if_duplicated;
            public bool cen;
            public int rx;
            public bool sendIF;

            //bespoke for tx info only
            public bool SendTXInfo;
            public Band TXfreqBand;
            public bool RX2EnabledForTX;
            public bool TXVFOB;
        }

        private Console _console;
		private TcpClient m_client = null;
		private NetworkStream m_stream = null;
		private bool m_stopClient = false;
		private bool m_disconnected = false;
        private int m_disconnectNotified = 0;
		private Thread m_clientListenerThread = null;
		private bool m_markedForDeletion = false;
		private bool m_bWebSocket = false;
		private Thread m_VFODataThread = null;
        private Thread m_sendThread = null;
		private TCPIPtciServer m_server = null;
		private int m_nRateLimit;
        private LinkedList<VFOData> m_vfoDataList = new LinkedList<VFOData>();
        private List<byte> _m_buffer = new List<byte>();
        private Stopwatch m_swVFO = new Stopwatch();
        private System.Threading.Timer m_tmVFOtimer;
        private Stopwatch m_swCentre = new Stopwatch();
        private System.Threading.Timer m_tmCentretimer;
        private Stopwatch m_swTXFrequency = new Stopwatch();
        private System.Threading.Timer m_tmTXFrequency;
        private readonly object m_objStreamLock = new object();
        private readonly object m_objOutboundLock = new object();
        private readonly object m_objTxQueueLock = new object();
        private readonly object m_objRxAudioLock = new object();
        private const int MAX_TX_AUDIO_QUEUE_BLOCKS = 64;
        private const int MAX_TX_AUDIO_QUEUE_COMPLEX_SAMPLES = 96000;
        private readonly AutoResetEvent m_outboundFrameEvent = new AutoResetEvent(false);
        private readonly HashSet<int> m_iqStreamEnabled = new HashSet<int>();
        private readonly HashSet<int> m_audioStreamEnabled = new HashSet<int>();
        private readonly Queue<TCIQueuedTxAudio> m_txAudioQueue = new Queue<TCIQueuedTxAudio>();
        private readonly Queue<TCIOutboundFrame> m_outboundUrgentFrames = new Queue<TCIOutboundFrame>();
        private readonly Queue<TCIOutboundFrame> m_outboundBinaryFrames = new Queue<TCIOutboundFrame>();
        private readonly Queue<TCIOutboundFrame> m_outboundControlFrames = new Queue<TCIOutboundFrame>();
        private readonly Queue<string> m_outboundCoalescedOrder = new Queue<string>();
        private readonly HashSet<string> m_outboundCoalescedKeys = new HashSet<string>();
        private readonly Dictionary<string, TCIOutboundFrame> m_outboundCoalescedFrames = new Dictionary<string, TCIOutboundFrame>();
        private readonly Dictionary<int, TCIPendingFloatBuffer> m_rxAudioLeftPending = new Dictionary<int, TCIPendingFloatBuffer>();
        private readonly Dictionary<int, TCIPendingFloatBuffer> m_rxAudioRightPending = new Dictionary<int, TCIPendingFloatBuffer>();
        private readonly Dictionary<int, TCIRxAudioResamplerState> m_rxAudioResamplers = new Dictionary<int, TCIRxAudioResamplerState>();
        private int[] m_hwSampleRate = new int[] { 48000, 48000 };
        private int m_audioSampleRate = 48000;
		private TCISampleType m_audioSampleType = TCISampleType.FLOAT32;
		private int m_audioStreamChannels = 2;
		private int m_audioStreamSamples = 2048;
        private bool m_audioStreamSamplesExplicitlySet = false;
        private int m_txStreamAudioBufferingMs = 50;
        private bool m_txUsesTCIAudio = false;
        private bool m_tciPttActive = false;
        private int m_txQueuedComplexSamples = 0;
        private bool m_seenModernTxAudioNegotiation = false;
        private readonly clsTCISensorManager m_sensorManager = new clsTCISensorManager();
        private System.Threading.Timer m_tmRxSensors;
        private System.Threading.Timer m_tmTxSensors;
        public TCPIPtciSocketListener(TcpClient client, Console c, TCPIPtciServer server, int rateLimit)
		{
			_console = c;
			m_nRateLimit = rateLimit;
			m_server = server;
			m_client = client;
            m_client.ReceiveTimeout = 0;
			m_stream = client.GetStream();
            m_stream.ReadTimeout = Timeout.Infinite;
			m_audioStreamSamples = getDefaultAudioStreamSamples(m_audioSampleRate);
			for (int i = 0; i < m_hwSampleRate.Length; i++)
			{
				m_hwSampleRate[i] = cmaster.GetInputRate(0, i);
            }
		}
		~TCPIPtciSocketListener()
		{
			StopSocketListener();
		}
        private Console consoleThreadSafe
        {
            get
            {
                if (_console == null) return null;

                if (_console.InvokeRequired)
                {
                    return (Console)_console.Invoke(new Func<Console>(() => _console.ThreadSafeTCIAccessor));
                }
                else
                    return _console.ThreadSafeTCIAccessor;
            }
        }
        //
        public void ClickedOnSpot(string callsign, long frequency, int rx = -1, int chan = -1)
        {
			if(rx == -1 || chan == -1)
            {
				sendClickedOnSpot(callsign, frequency);
			}
            else
            {
				sendClickedOnSpotRX(rx - 1, chan, callsign, frequency);
            }
        }
		public void ThetisFocusChange(bool focus)
		{
			if (m_disconnected) return;
			sendAppFocus(focus);
		}
		public void RX2EnabledChange(bool enabled)
        {
			if (m_disconnected) return;
			sendRXEnable(1, enabled);
			sendTXEnable(1, enabled && !consoleThreadSafe.MOX);
		}
        public void HWSampleRateChange(int rx, int oldSampleRate, int newSampleRate)
        {
            if (m_disconnected) return;

            int publishedRate;
            int halfSample;
            lock (m_objStreamLock)
            {
                int index = rx - 1;
                if (index >= 0 && index < m_hwSampleRate.Length)
                    m_hwSampleRate[index] = newSampleRate;

                publishedRate = getPublishedIQSampleRateLocked();

                int rx1SampleRate = 48000;
                if (m_hwSampleRate != null && m_hwSampleRate.Length > 0 && m_hwSampleRate[0] > 0)
                    rx1SampleRate = m_hwSampleRate[0];

                halfSample = rx1SampleRate / 2;
            }

            sendIQSampleRate(publishedRate);

            //sendIFLimits(-halfSample, halfSample);
            sendIFLimits(-halfSample, halfSample); // sadly this is global in tci, so use rx1
        }

        internal bool RequiresRxSensorUpdate(int receiver, int channel)
        {
            return m_sensorManager.RequiresRxChannelUpdate(receiver, channel);
        }

        internal bool SensorRequiresUpdate(int receiver, Reading reading)
        {
            return m_sensorManager.SensorRequiresUpdate(receiver, reading);
        }

        internal bool RequiresTxSensorUpdate()
        {
            return m_sensorManager.RequiresTxUpdate();
        }

        internal void MeterReadingsChanged(int rx, bool tx, ref Dictionary<Reading, float> readings)
        {
            if (readings == null) return;

            if (tx)
            {
                if (readings.TryGetValue(Reading.MIC, out float micLevel) &&
                    readings.TryGetValue(Reading.PWR, out float power) &&
                    readings.TryGetValue(Reading.SWR, out float swr))
                {
                    m_sensorManager.SetTxReadings(micLevel, power, power, swr);
                }

                return;
            }

            int receiver = rx - 1;
            if (receiver < 0 || receiver > 1) return;

            if (readings.TryGetValue(Reading.SIGNAL_STRENGTH, out float signal) && readings.TryGetValue(Reading.AVG_SIGNAL_STRENGTH, out float avg_signal) && readings.TryGetValue(Reading.SIGNAL_MAX_BIN, out float peak_bin_signal))
            {
                m_sensorManager.SetRxChannelReading(receiver, 0, signal, avg_signal, peak_bin_signal);
            }
        }

        internal int MinimumRequiredRxSensorInterval()
        {
            return m_sensorManager.RxSensorsEnabled ? m_sensorManager.RxIntervalMs : int.MaxValue;
        }

        internal int MinimumRequiredTxSensorInterval()
        {
            return m_sensorManager.TxSensorsEnabled ? m_sensorManager.TxIntervalMs : int.MaxValue;
        }

        private int getPublishedIQSampleRate()
        {
            lock (m_objStreamLock)
            {
                return getPublishedIQSampleRateLocked();
            }
        }

        private int getPublishedIQSampleRateLocked()
        {
            int maxRate = 48000;
            for (int i = 0; i < m_hwSampleRate.Length; i++)
            {
                if (m_hwSampleRate[i] > maxRate)
                    maxRate = m_hwSampleRate[i];
            }

            return Math.Min(maxRate, 384000);
        }

        private unsafe void destroyRxAudioResamplerState(TCIRxAudioResamplerState state)
        {
            if (state == null)
                return;

            if (state.LeftResampler != IntPtr.Zero)
            {
                WDSP.destroy_resampleFV(state.LeftResampler.ToPointer());
                state.LeftResampler = IntPtr.Zero;
            }

            if (state.RightResampler != IntPtr.Zero)
            {
                WDSP.destroy_resampleFV(state.RightResampler.ToPointer());
                state.RightResampler = IntPtr.Zero;
            }

            state.InputRate = 0;
            state.OutputRate = 0;
        }

        private void clearRxAudioStateForReceiver(int receiver)
        {
            lock (m_objRxAudioLock)
            {
                m_rxAudioLeftPending.Remove(receiver);
                m_rxAudioRightPending.Remove(receiver);

                if (m_rxAudioResamplers.TryGetValue(receiver, out TCIRxAudioResamplerState state))
                {
                    destroyRxAudioResamplerState(state);
                    m_rxAudioResamplers.Remove(receiver);
                }
            }
        }

        private void clearRxAudioStreamState()
        {
            lock (m_objRxAudioLock)
            {
                m_rxAudioLeftPending.Clear();
                m_rxAudioRightPending.Clear();

                foreach (TCIRxAudioResamplerState state in m_rxAudioResamplers.Values)
                    destroyRxAudioResamplerState(state);

                m_rxAudioResamplers.Clear();
            }
        }

        private unsafe int resampleRxAudioSamples(int receiver, int inputRate, int targetRate, float[] left, float[] right, int samples, out float[] leftOut, out float[] rightOut, out bool resampled)
        {
            leftOut = left;
            if (right != null)
                rightOut = right;
            else
                rightOut = left;
            resampled = false;

            if (left == null || samples <= 0)
                return 0;

            if (inputRate <= 0 || targetRate <= 0 || inputRate == targetRate)
                return Math.Min(samples, left.Length);

            if (!m_rxAudioResamplers.TryGetValue(receiver, out TCIRxAudioResamplerState state))
            {
                state = new TCIRxAudioResamplerState();
                m_rxAudioResamplers[receiver] = state;
            }

            if (state.LeftResampler == IntPtr.Zero ||
                state.RightResampler == IntPtr.Zero ||
                state.InputRate != inputRate ||
                state.OutputRate != targetRate)
            {
                destroyRxAudioResamplerState(state);
                state.LeftResampler = (IntPtr)WDSP.create_resampleFV(inputRate, targetRate);
                state.RightResampler = (IntPtr)WDSP.create_resampleFV(inputRate, targetRate);
                state.InputRate = inputRate;
                state.OutputRate = targetRate;
            }

            if (state.LeftResampler == IntPtr.Zero || state.RightResampler == IntPtr.Zero)
                return Math.Min(samples, left.Length);

            float[] rightSource = right;
            if (rightSource == null || rightSource.Length < samples)
            {
                rightSource = new float[samples];
                int copied = 0;
                if (right != null && right.Length > 0)
                {
                    copied = Math.Min(samples, right.Length);
                    Array.Copy(right, 0, rightSource, 0, copied);
                }

                if (copied < samples)
                    Array.Copy(left, copied, rightSource, copied, samples - copied);
            }

            int maxOutputSamples = Math.Max(
                samples + 64,
                (int)Math.Ceiling((double)samples * targetRate / inputRate) + 64);

            float[] leftOutput = new float[maxOutputSamples];
            float[] rightOutput = new float[maxOutputSamples];
            int leftOutputSamples = 0;
            int rightOutputSamples = 0;

            fixed (float* pLeftInput = left)
            fixed (float* pRightInput = rightSource)
            fixed (float* pLeftOutput = leftOutput)
            fixed (float* pRightOutput = rightOutput)
            {
                WDSP.xresampleFV(pLeftInput, pLeftOutput, samples, &leftOutputSamples, state.LeftResampler.ToPointer());
                WDSP.xresampleFV(pRightInput, pRightOutput, samples, &rightOutputSamples, state.RightResampler.ToPointer());
            }

            int outputSamples = Math.Min(leftOutputSamples, rightOutputSamples);
            if (outputSamples <= 0)
            {
                leftOut = Array.Empty<float>();
                rightOut = Array.Empty<float>();
                return 0;
            }

            if (outputSamples != leftOutput.Length)
            {
                float[] resizedLeft = new float[outputSamples];
                float[] resizedRight = new float[outputSamples];
                Array.Copy(leftOutput, resizedLeft, outputSamples);
                Array.Copy(rightOutput, resizedRight, outputSamples);
                leftOutput = resizedLeft;
                rightOutput = resizedRight;
            }

            leftOut = leftOutput;
            rightOut = rightOutput;
            resampled = true;
            return outputSamples;
        }

		public void DrivePowerChange(int rx, int newPower, bool tune)
        {
			if (m_disconnected) return;
            if (tune)
				sendTunePower(rx - 1, newPower);
			else
				sendDrivePower(rx - 1, newPower);
		}
		public void TuneChange(int rx, bool oldTune, bool newTune)
        {
			if (m_disconnected) return;
			sendTune(rx-1, newTune);
        }
		public void SplitChange(int rx, bool newSplit)
		{
			if (m_disconnected) return;
			bool bSplit = consoleThreadSafe.VFOSplit;
			sendSplit(rx-1, bSplit);
		}
        public void MuteChanged(int rx, bool newState)
        {
            if (m_disconnected) return;
			sendMute(consoleThreadSafe.MUT || (consoleThreadSafe.RX2Enabled && consoleThreadSafe.MUT2));
			sendMuteRX(rx - 1, newState);
        }
        public void AnfChanged(int rx, bool newState)
        {
            if (m_disconnected) return;
            sendAnfEnable(rx - 1, newState);
        }
        public void RxAfGainChanged(int rx, bool is_subrx, int gain)
        {
            if (m_disconnected) return;
            int chan = is_subrx ? 1 : 0;
            double db = audioGainToDb(gain / 100f);
            sendRxVolume(rx - 1, chan, db);
        }
        public void CTUNChanged(int rx, bool enabled)
        {
            if (m_disconnected) return;
            sendCTUN(rx - 1, enabled);
        }
        public void VFOSyncChanged(bool enabled)
        {
            if (m_disconnected) return;
            sendVFOSyncEx(enabled);
        }
        public void FMDeviationChanged(int rx, int deviationHz)
        {
            if (m_disconnected) return;
            sendFMDeviationEx(rx - 1, deviationHz);
        }
        public void AGCModeChanged(int rx, AGCMode mode)
        {
            if (m_disconnected) return;
            sendAgcMode(rx - 1, mode);
        }
        public void AGCAutoChanged(int rx, bool enabled)
        {
            if (m_disconnected) return;
            sendAgcAutoEx(rx - 1, enabled);
        }
        public void TXProfileChanged(string profile)
        {
            if (m_disconnected) return;
            sendTXProfile(profile);
        }
        public void TXProfilesChanged()
        {
            if (m_disconnected) return;
            sendTXProfiles();
        }
        public void CalibrationChanged(int rx)
        {
            if (m_disconnected) return;

            float meter_offset;
            float display_offset;
            float xvtr_gain_offset;
            float offset_6m;
            float tx_display_offset;

            meter_offset = rx == 0 ? consoleThreadSafe.RX1MeterCalOffset : consoleThreadSafe.RX2MeterCalOffset;
            display_offset = rx == 0 ? consoleThreadSafe.RX1DisplayCalOffset : consoleThreadSafe.RX2DisplayCalOffset;
            xvtr_gain_offset = rx == 0 ? consoleThreadSafe.RX1XVTRGainOffset : consoleThreadSafe.RX2XVTRGainOffset;
            offset_6m = rx == 0 ? consoleThreadSafe.RX6mGainOffset_RX1 : consoleThreadSafe.RX6mGainOffset_RX2;
            tx_display_offset = consoleThreadSafe.TXDisplayCalOffset;

            sendCalibration(rx, meter_offset, display_offset, xvtr_gain_offset, offset_6m, tx_display_offset);
        }
        public void MONChanged(bool newState)
		{
            if (m_disconnected) return;
			sendMONEnable(newState);
        }
		public void MONVolumeChanged(int newVolume)
		{
			if (m_disconnected) return;
            sendMONVolume(linearToDbVolume(newVolume));
		}
        public void VolumeChanged(int newVolume)
        {
            if (m_disconnected) return;
            sendVolume(linearToDbVolume(newVolume));
        }
        public void BalanceChanged(int rx, bool is_subrx, int newBalance)
        {
            if (m_disconnected) return;
            int chan = is_subrx ? 1 : 0;
            double balance = 40.0 - (newBalance * 0.8);
            sendRxBalance(rx - 1, chan, balance);
        }
        public void RxStepAttChanged(int rx, int attenuation)
        {
            if (m_disconnected) return;
            sendRxStepAttEx(rx - 1, attenuation);
        }
        public void RxPreampAttChanged(int rx, PreampMode preamp_mode)
        {
            if (m_disconnected) return;

            int attenuation = preampModeToAttenuation(preamp_mode);
            sendRxPreampAttEx(rx - 1, -attenuation);
        }
        public void RxStepAttEnabledChanged(int rx, bool enabled)
        {
            if (m_disconnected) return;
            sendRxStepAttEnabledEx(rx - 1, enabled);
        }
        public void AGCGainChanged(int rx, int newGain)
        {
            if (m_disconnected) return;
            sendAgcGain(rx - 1, newGain);
        }
        public void RITChanged(bool newState)
        {
            if (m_disconnected) return;
            sendRITEnable(0, newState);
            sendRITEnable(1, newState);
        }
        public void XITChanged(bool newState)
        {
            if (m_disconnected) return;
            sendXITEnable(0, newState);
            sendXITEnable(1, newState);
        }
        public void RITValueChanged(int newValue)
        {
            if (m_disconnected) return;
            sendRITOffset(0, newValue);
            sendRITOffset(1, newValue);
        }
        public void XITValueChanged(int newValue)
        {
            if (m_disconnected) return;
            sendXITOffset(0, newValue);
            sendXITOffset(1, newValue);
        }
        public void CwMacrosSpeedChanged(int newSpeed)
        {
            if (m_disconnected) return;
            sendCwMacrosSpeed(newSpeed);
        }
        public void CwMacrosDelayChanged(int newDelay)
        {
            if (m_disconnected) return;
            sendCwMacrosDelay(newDelay);
        }
        public void CwKeyerSpeedChanged(int newSpeed)
        {
            if (m_disconnected) return;
            sendCwKeyerSpeed(newSpeed);
        }
        public void CwMacrosEmpty(int rx)
        {
            if (m_disconnected) return;
            sendCwMacrosEmpty(rx);
        }
        public void CwCallsignSent(string callsign)
        {
            if (m_disconnected) return;
            sendCallsignSend(callsign);
        }
        public void NBChanged(int rx, int newNB)
        {
            if (m_disconnected) return;
            bool enabled = newNB > 0;
            sendNBEnable(rx - 1, enabled, false, newNB);
            sendNBEnable(rx - 1, enabled, true, newNB);
        }
        public void NRChanged(int rx, int newNR)
        {
            if (m_disconnected) return;
            bool enabled = newNR > 0;
            sendNREnable(rx - 1, enabled, false, newNR);
            sendNREnable(rx - 1, enabled, true, newNR);
        }
        public void BinChanged(int rx, bool newState)
        {
            if (m_disconnected) return;
            sendRxBinEnable(rx - 1, newState);
        }
        public void LockChanged(int rx, bool newState)
        {
            if (m_disconnected) return;
            sendLock(rx - 1, newState);
        }
        public void VFOLocksChanged()
        {
            if (m_disconnected) return;
            sendAllVFOLocks();
        }
        public void SqlChanged(int rx, SquelchState newState)
        {
            if (m_disconnected) return;
            sendSqlEnable(rx - 1, newState != SquelchState.OFF);
        }
        public void SqlLevelChanged(int rx, int newValue)
        {
            if (m_disconnected) return;
            sendSqlLevel(rx - 1, newValue);
        }
        public void ApfChanged(int rx, bool newState)
        {
            if (m_disconnected) return;
            sendRxApfEnable(rx - 1, newState);
        }
        public void NfChanged(bool newState)
        {
            if (m_disconnected) return;
            sendRxNfEnable(0, newState);
            sendRxNfEnable(1, newState);
        }
        public void DiglOffsetChanged(int newValue)
        {
            if (m_disconnected) return;
            sendDiglOffset(newValue);
        }
        public void DiguOffsetChanged(int newValue)
        {
            if (m_disconnected) return;
            sendDiguOffset(newValue);
        }
		public void TXFrequencyChanged(long new_frequency, Band new_band, bool rx2_enabled, bool tx_vfob)
		{
            if (m_disconnected) return;
            sendTXFrequencyChanged(new_frequency, new_band, rx2_enabled, tx_vfob);
        }
        private void limitList()
        {
			lock (m_objVFODataLock)
			{
				while (m_vfoDataList.Count > 10)
				{
					m_vfoDataList.RemoveFirst();
				}
			}
		}

		private object m_objVFODataLock = new Object();
		private async void VFOdata()
        {
			while (!m_stopClient)
            {
				int nCount;
				lock (m_objVFODataLock)
                {
					nCount = m_vfoDataList.Count;
                }

				if(nCount > 0)
                {
					LinkedListNode<VFOData> vfon = null;
					VFOData vfoData;

					lock (m_objVFODataLock)
                    {
						vfon = m_vfoDataList.First;
						vfoData = vfon.Value;
						vfon = null;
						m_vfoDataList.RemoveFirst();
					}

					if (vfoData.SendTXInfo)
					{
						// tx info only
						sendTXFrequencyChanged((long)(vfoData.freqMHz * 1e6), vfoData.TXfreqBand, vfoData.RX2EnabledForTX, vfoData.TXVFOB);
					}
					else
					{
						if (vfoData.cen)
						{
							sendDDS(vfoData.rx, (long)(vfoData.centreMHz * 1e6));
							if (vfoData.sendIF) sendIF(vfoData.rx, vfoData.chan, (int)vfoData.offsetHz);
						}
						else
						{
							if (vfoData.duplicate_tochan != -1)
							{
								if (!vfoData.replace_if_duplicated)
								{
									if (vfoData.sendIF) sendIF(vfoData.rx, vfoData.chan, (int)vfoData.offsetHz);
									sendVFO(vfoData.rx, vfoData.chan, (long)(vfoData.freqMHz * 1e6));
								}
								if (vfoData.sendIF) sendIF(vfoData.rx, vfoData.duplicate_tochan, (int)vfoData.offsetHz);
								sendVFO(vfoData.rx, vfoData.duplicate_tochan, (long)(vfoData.freqMHz * 1e6));
							}
							else
							{
								if (vfoData.sendIF) sendIF(vfoData.rx, vfoData.chan, (int)vfoData.offsetHz);
								sendVFO(vfoData.rx, vfoData.chan, (long)(vfoData.freqMHz * 1e6));
							}
						}
					}
                }

                lock (m_objVFODataLock)
                {
					nCount = m_vfoDataList.Count;
				}

				if (!m_stopClient)
				{ 
					if(nCount == 0) 
						await Task.Delay(1);
				}
			}
		}

		private void vfoFrequencyChange(VFOData vfod)
		{
			if (m_disconnected) return;
			lock (m_objVFODataLock)
			{
				limitList();
				m_vfoDataList.AddLast(vfod);
			}
		}
		private void centreFrequencyChange(VFOData vfod)
		{
			if (m_disconnected) return;
			lock (m_objVFODataLock)
			{
				limitList();
				m_vfoDataList.AddLast(vfod);
			}
		}
        private void txFrequencyChange(VFOData vfod)
        {
            if (m_disconnected) return;
            lock (m_objVFODataLock)
            {
                limitList();
                m_vfoDataList.AddLast(vfod);
            }
        }
        public void MoxChange(int rx, bool oldMox, bool newMox)
        {
			if (m_disconnected) return;

            if (newMox)
            {
				if (rx == 1)
				{
					if (consoleThreadSafe.RX2Enabled) sendTXEnable(1, false);
				}
				else
				{
					sendTXEnable(0, false);
				}
			}
			else
			{
				if (rx == 1)
				{
					if(consoleThreadSafe.RX2Enabled) sendTXEnable(1, true);
				}
				else
				{
					sendTXEnable(0, true);
				}
			}
		
			sendMOX(rx - 1, newMox);
		}
		public void ModeChange(int rx, DSPMode oldMode, DSPMode newMode, Band oldBand, Band newBand)
        {
			if (m_disconnected) return;
			sendMode(rx-1, newMode);
        }
		public void BandChange(int rx, Band oldBand, Band newBand)
        {
			if (m_disconnected) return;

			// check band for tx? TODO
			sendTXEnable(rx-1, rx == 1 ? true : consoleThreadSafe.RX2Enabled); // MW0LGE_22b fixed, rx1 should be tx only, not rx2
		}
		public void FilterChange(int rx, Filter oldFilter, Filter newFilter, Band band, int low, int high)
        {
			if (m_disconnected) return;
			sendFilterBand(rx-1, low, high);
		}
		public void FilterEdgesChange(int rx, Filter filter, Band band, int low, int high)
		{
			if (m_disconnected) return;
			sendFilterBand(rx-1, low, high);
        }
        public void TXFilterBandChanged(int low, int high)
        {
            if (m_disconnected) return;
            sendTXFilterBandEx(low, high);
        }
		public void PowerChange(bool oldPower, bool newPower)
        {
			if (m_disconnected) return;
			sendStartStop(newPower);
        }
		//

		public void StartSocketListener()
		{
			if (m_client != null)
			{
                m_sendThread = new Thread(new ThreadStart(SendThreadProc));
                m_sendThread.Name = "TCI client sender Thread";
                m_sendThread.Priority = ThreadPriority.AboveNormal;
                m_sendThread.Start();

				m_VFODataThread = new Thread(new ThreadStart(VFOdata));
				m_VFODataThread.Priority = ThreadPriority.Normal;
				m_VFODataThread.Start();

				m_clientListenerThread =
					new Thread(new ThreadStart(SocketListenerThreadStart));
				m_clientListenerThread.Name = "TCI client listener Thread";

                m_clientListenerThread.Start();
			}
		}

		private enum EOpcodeType
		{			
			Fragment = 0, /* continuation code */			
			Text = 1, /*  text code */			
			Binary = 2, /*  binary code */			
			ClosedConnection = 8, /* closed connection */			
			Ping = 9, /* ping */			
			Pong = 10 /* pong */
		}

		private static byte[] getFrameFromString(string Message, EOpcodeType Opcode = EOpcodeType.Text)
		{
			byte[] response;
            //byte[] bytesRaw = Encoding.Default.GetBytes(Message);
            byte[] bytesRaw = Encoding.UTF8.GetBytes(Message ?? string.Empty); // utf-8 required for text frame
            byte[] frame = new byte[10];

			long indexStartRawData = -1;
			long length = (long)bytesRaw.Length;

			frame[0] = (byte)(128 + (int)Opcode);
			if (length <= 125)
			{
				frame[1] = (byte)length;
				indexStartRawData = 2;
			}
			else if (length >= 126 && length <= 65535)
			{
				frame[1] = (byte)126;
				frame[2] = (byte)((length >> 8) & 255);
				frame[3] = (byte)(length & 255);
				indexStartRawData = 4;
			}
			else
			{
				frame[1] = (byte)127;
				frame[2] = (byte)((length >> 56) & 255);
				frame[3] = (byte)((length >> 48) & 255);
				frame[4] = (byte)((length >> 40) & 255);
				frame[5] = (byte)((length >> 32) & 255);
				frame[6] = (byte)((length >> 24) & 255);
				frame[7] = (byte)((length >> 16) & 255);
				frame[8] = (byte)((length >> 8) & 255);
				frame[9] = (byte)(length & 255);

				indexStartRawData = 10;
			}

			response = new byte[indexStartRawData + length];

			long i, reponseIdx = 0;

			//Add the frame bytes to the reponse
			for (i = 0; i < indexStartRawData; i++)
			{
				response[reponseIdx] = frame[i];
				reponseIdx++;
			}

			//Add the data bytes to the response
			for (i = 0; i < length; i++)
			{
				response[reponseIdx] = bytesRaw[i];
				reponseIdx++;
			}

			return response;
		}

		private static byte[] GetFrameFromBytes(byte[] payload, EOpcodeType opcode = EOpcodeType.Binary)
		{
			if (payload == null) payload = Array.Empty<byte>();

			byte[] frame = new byte[10];
			long indexStartRawData = -1;
			long length = payload.LongLength;

			frame[0] = (byte)(128 + (int)opcode);
			if (length <= 125)
			{
				frame[1] = (byte)length;
				indexStartRawData = 2;
			}
			else if (length <= 65535)
			{
				frame[1] = (byte)126;
				frame[2] = (byte)((length >> 8) & 255);
				frame[3] = (byte)(length & 255);
				indexStartRawData = 4;
			}
			else
			{
				frame[1] = (byte)127;
				frame[2] = (byte)((length >> 56) & 255);
				frame[3] = (byte)((length >> 48) & 255);
				frame[4] = (byte)((length >> 40) & 255);
				frame[5] = (byte)((length >> 32) & 255);
				frame[6] = (byte)((length >> 24) & 255);
				frame[7] = (byte)((length >> 16) & 255);
				frame[8] = (byte)((length >> 8) & 255);
				frame[9] = (byte)(length & 255);
				indexStartRawData = 10;
			}

			byte[] response = new byte[indexStartRawData + length];
			Buffer.BlockCopy(frame, 0, response, 0, (int)indexStartRawData);
			if (length > 0) Buffer.BlockCopy(payload, 0, response, (int)indexStartRawData, (int)length);
			return response;
		}

        private static string getCoalescedTextFrameKey(string message)
        {
            if (string.IsNullOrEmpty(message))
                return null;

            string trimmed = message.Trim();
            int colonIndex = trimmed.IndexOf(':');
            if (colonIndex <= 0)
                return null;

            string command = trimmed.Substring(0, colonIndex).ToLowerInvariant();
            string[] args = trimmed.Substring(colonIndex + 1).TrimEnd(';').Split(',');

            switch (command)
            {
                case "vfo":
                case "if":
                    if (args.Length >= 2)
                        return command + ":" + args[0] + "," + args[1];
                    break;
                case "dds":
                case "rx_filter_band":
                case "rx_balance":
                case "rx_step_att_ex":
                case "rx_preamp_att_ex":
                case "agc_gain":
                case "drive":
                case "tune_drive":
                case "tune":
                    if (args.Length >= 1)
                        return command + ":" + args[0];
                    break;
                case "tx_filter_band_ex":
                case "tx_frequency":
                case "tx_frequency_ex":
                case "volume":
                    return command;
            }

            return null;
        }

        private bool hasPendingOutboundFramesLocked()
        {
            return m_outboundUrgentFrames.Count > 0 ||
                   m_outboundBinaryFrames.Count > 0 ||
                   m_outboundControlFrames.Count > 0 ||
                   m_outboundCoalescedOrder.Count > 0;
        }

        private bool tryDequeueNextOutboundFrameLocked(out TCIOutboundFrame frame)
        {
            frame = null;

            if (m_outboundUrgentFrames.Count > 0)
            {
                frame = m_outboundUrgentFrames.Dequeue();
                return true;
            }

            if (m_outboundBinaryFrames.Count > 0)
            {
                frame = m_outboundBinaryFrames.Dequeue();
                return true;
            }

            if (m_outboundControlFrames.Count > 0)
            {
                frame = m_outboundControlFrames.Dequeue();
                return true;
            }

            while (m_outboundCoalescedOrder.Count > 0)
            {
                string key = m_outboundCoalescedOrder.Dequeue();
                m_outboundCoalescedKeys.Remove(key);
                if (m_outboundCoalescedFrames.TryGetValue(key, out frame))
                {
                    m_outboundCoalescedFrames.Remove(key);
                    return true;
                }
            }

            return false;
        }

        private void clearOutboundFrames()
        {
            lock (m_objOutboundLock)
            {
                m_outboundUrgentFrames.Clear();
                m_outboundBinaryFrames.Clear();
                m_outboundControlFrames.Clear();
                m_outboundCoalescedOrder.Clear();
                m_outboundCoalescedKeys.Clear();
                m_outboundCoalescedFrames.Clear();
            }
        }

        private void flushOutboundFrames(int timeoutMs)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                lock (m_objOutboundLock)
                {
                    if (!hasPendingOutboundFramesLocked())
                        return;
                }

                Thread.Sleep(5);
            }
        }

        private void enqueueOutboundFrame(byte[] frameBytes, string logText, TCIOutboundPriority priority, string coalescedKey = null)
        {
            if (frameBytes == null || frameBytes.Length == 0)
                return;

            lock (m_objOutboundLock)
            {
                TCIOutboundFrame frame = new TCIOutboundFrame()
                {
                    Frame = frameBytes,
                    LogText = logText
                };

                if (!string.IsNullOrEmpty(coalescedKey) && priority == TCIOutboundPriority.Control)
                {
                    m_outboundCoalescedFrames[coalescedKey] = frame;
                    if (m_outboundCoalescedKeys.Add(coalescedKey))
                        m_outboundCoalescedOrder.Enqueue(coalescedKey);
                }
                else
                {
                    switch (priority)
                    {
                        case TCIOutboundPriority.Urgent:
                            m_outboundUrgentFrames.Enqueue(frame);
                            break;
                        case TCIOutboundPriority.Binary:
                            m_outboundBinaryFrames.Enqueue(frame);
                            break;
                        default:
                            m_outboundControlFrames.Enqueue(frame);
                            break;
                    }
                }
            }

            m_outboundFrameEvent.Set();
        }

        private void SendThreadProc()
        {
            while (true)
            {
                TCIOutboundFrame outboundFrame = null;

                lock (m_objOutboundLock)
                {
                    tryDequeueNextOutboundFrameLocked(out outboundFrame);
                }

                if (outboundFrame == null)
                {
                    if (m_stopClient)
                    {
                        lock (m_objOutboundLock)
                        {
                            if (!hasPendingOutboundFramesLocked())
                                break;
                        }
                    }

                    m_outboundFrameEvent.WaitOne(20);
                    continue;
                }

                try
                {
                    if (m_bWebSocket && m_client != null && m_stream != null && m_client.Connected)
                    {
                        m_stream.Write(outboundFrame.Frame, 0, outboundFrame.Frame.Length);
                        if (!string.IsNullOrEmpty(outboundFrame.LogText) && m_server != null && m_server.LogForm != null)
                        {
                            m_server.LogForm.Log(false, outboundFrame.LogText);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("problem writing queued frame");
                    abortSocketTransport();
                }
            }
        }

        private void abortSocketTransport()
        {
            m_stopClient = true;
            m_outboundFrameEvent.Set();

            lock (m_objStreamLock)
            {
                try
                {
                    if (m_stream != null)
                    {
                        m_stream.Close();
                        m_stream = null;
                    }
                }
                catch
                {
                }

                try
                {
                    m_client?.Close();
                }
                catch
                {
                }
            }
        }

        private static bool isSocketReadTimeout(IOException ex)
        {
            if (ex?.InnerException is SocketException socketEx)
            {
                return socketEx.SocketErrorCode == SocketError.TimedOut ||
                       socketEx.SocketErrorCode == SocketError.WouldBlock;
            }

            return false;
        }

		private bool upgradeToWebSocket(string msg)
        {
			bool bRet;

			try
			{
				// 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
				// 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
				// 3. Compute SHA-1 and Base64 hash of the new value
				// 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
				string swk = Regex.Match(msg, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
				string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
				byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
				string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

				// HTTP/1.1 defines the sequence CR LF as the end-of-line marker
				byte[] response = Encoding.UTF8.GetBytes(
					"HTTP/1.1 101 Switching Protocols\r\n" +
					"Connection: Upgrade\r\n" +
					"Upgrade: websocket\r\n" +
					"Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

				m_stream.Write(response, 0, response.Length);

				bRet = true;
			}
			catch
            {
				bRet = false;
            }

			return bRet;
		}

		//
		private void sendStart()
        {
			sendTextFrame("start;");
		}
		private void sendStop()
		{
			sendTextFrame("stop;");
		}
		private void sendSplit(int rx, bool bSplit)
        {
			string s = "split_enable:" + rx.ToString() + "," + bSplit.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
        private void sendRITEnable(int rx, bool enabled)
        {
            string s = "rit_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendXITEnable(int rx, bool enabled)
        {
            string s = "xit_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendRITOffset(int rx, int offset)
        {
            string s = "rit_offset:" + rx.ToString() + "," + offset.ToString() + ";";
            sendTextFrame(s);
        }
        private void sendXITOffset(int rx, int offset)
        {
            string s = "xit_offset:" + rx.ToString() + "," + offset.ToString() + ";";
            sendTextFrame(s);
        }
        private void sendRxBinEnable(int rx, bool enabled)
        {
            string s = "rx_bin_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendRxApfEnable(int rx, bool enabled)
        {
            string s = "rx_apf_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendRxNfEnable(int rx, bool enabled)
        {
            string s = "rx_nf_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendLock(int rx, bool enabled)
        {
            string s = "lock:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendVFOLock(int rx, int chan, bool enabled)
        {
            string s = "vfo_lock:" + rx.ToString() + "," + chan.ToString() + "," + enabled.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendSqlEnable(int rx, bool enabled)
        {
            string s = "sql_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendSqlLevel(int rx, int level)
        {
            string s = "sql_level:" + rx.ToString() + "," + level.ToString() + ";";
            sendTextFrame(s);
        }
        private void sendCwMacrosSpeed(int speed)
        {
            sendTextFrame("cw_macros_speed:" + speed.ToString() + ";");
        }
        private void sendCwMacrosDelay(int delayMs)
        {
            sendTextFrame("cw_macros_delay:" + delayMs.ToString() + ";");
        }
        private void sendCwKeyerSpeed(int speed)
        {
            sendTextFrame("cw_keyer_speed:" + speed.ToString() + ";");
        }
        private void sendCwMacrosEmpty(int rx)
        {
            sendTextFrame("cw_macros_empty:" + rx.ToString() + ";");
        }
        private void sendCallsignSend(string callsign)
        {
            sendTextFrame("callsign_send:" + callsign + ";");
        }
        private bool tryGetVFOLockState(int rx, int chan, out bool enabled)
        {
            enabled = false;

            if (rx < 0 || rx > 1 || chan < 0 || chan > 1)
                return false;

            bool rx2Enabled = consoleThreadSafe != null && consoleThreadSafe.RX2Enabled;

            if (!rx2Enabled)
            {
                if (rx != 0)
                    return false;

                enabled = chan == 0 ? consoleThreadSafe.VFOALock : consoleThreadSafe.VFOBLock;
                return true;
            }

            if (rx == 0)
            {
                if (chan != 0)
                    return false;

                enabled = consoleThreadSafe.VFOALock;
                return true;
            }

            if (rx == 1)
            {
                enabled = consoleThreadSafe.VFOBLock;
                return true;
            }

            return false;
        }
        private bool trySetVFOLockState(int rx, int chan, bool enabled)
        {
            if (rx < 0 || rx > 1 || chan < 0 || chan > 1)
                return false;

            bool rx2Enabled = consoleThreadSafe != null && consoleThreadSafe.RX2Enabled;

            if (!rx2Enabled)
            {
                if (rx != 0)
                    return false;

                if (chan == 0)
                    consoleThreadSafe.VFOALock = enabled;
                else
                    consoleThreadSafe.VFOBLock = enabled;
                return true;
            }

            if (rx == 0)
            {
                if (chan != 0)
                    return false;

                consoleThreadSafe.VFOALock = enabled;
                return true;
            }

            if (rx == 1)
            {
                consoleThreadSafe.VFOBLock = enabled;
                return true;
            }

            return false;
        }
        private void sendAllVFOLocks()
        {
            if (consoleThreadSafe.RX2Enabled)
            {
            if (tryGetVFOLockState(0, 0, out bool lock00))
                sendVFOLock(0, 0, lock00);
                if (tryGetVFOLockState(1, 0, out bool lock10))
                    sendVFOLock(1, 0, lock10);
                if (tryGetVFOLockState(1, 1, out bool lock11))
                    sendVFOLock(1, 1, lock11);
            }
            else
            {
                if (tryGetVFOLockState(0, 0, out bool lock00))
                    sendVFOLock(0, 0, lock00);
                if (tryGetVFOLockState(0, 1, out bool lock01))
                    sendVFOLock(0, 1, lock01);
            }
        }
        private void sendDiglOffset(int offset)
        {
            string s = "digl_offset:" + offset.ToString() + ";";
            sendTextFrame(s);
        }
        private void sendDiguOffset(int offset)
        {
            string s = "digu_offset:" + offset.ToString() + ";";
            sendTextFrame(s);
        }
		private void sendVFO(int rx, int chan, long vfo = -1)
        {
			bool bVFOaUseRX2;
			if (m_server != null && consoleThreadSafe != null)
				bVFOaUseRX2 = consoleThreadSafe.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
			else
				bVFOaUseRX2 = false;

			if (vfo == -1)
			{
				if (rx == 0)
				{
					if (chan == 0)
						vfo = (long)(consoleThreadSafe.VFOAFreq * 1e6);
					else if (chan == 1)
						vfo = (long)(consoleThreadSafe.VFOBFreq * 1e6);
				}
				else if (rx == 1)
				{
					if(chan == 0)
                    {
						if(bVFOaUseRX2)
							vfo = (long)(consoleThreadSafe.VFOAFreq * 1e6);
						else
							vfo = (long)(consoleThreadSafe.VFOBFreq * 1e6);
					}
                    else if (chan == 1)
                    {
						vfo = (long)(consoleThreadSafe.VFOBFreq * 1e6);
					}
				}					
			}
			string s = "vfo:" + rx.ToString() + "," + chan.ToString() + "," + vfo.ToString() + ";";
			sendTextFrame(s);
        }
		private void sendIF(int rx, int chan, int offset = -999999999)
		{
			if (offset == -999999999)
			{
				if (rx == 0)
				{
					if (chan == 0)
					{
						offset = (int)consoleThreadSafe.radio.GetDSPRX(0, 0).RXOsc;
					}
					else if (chan == 1)
					{
						offset = (int)consoleThreadSafe.radio.GetDSPRX(0, 1).RXOsc;
					}
					else offset = 0;
				}
				else if (rx == 1)
					offset = (int)consoleThreadSafe.radio.GetDSPRX(1, 0).RXOsc;
			}

			offset += -consoleThreadSafe.GetDSPcwPitchShiftToZero(rx + 1); //MW0LGE [2.9.0.7] note we invert with -

            string s = "if:" + rx.ToString() + "," + chan.ToString() + "," + offset.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendMOX(int rx, bool mox, bool signalTCI = false)
        {
			string sTXSignalFromTCI;
			if (signalTCI)
				sTXSignalFromTCI = ",tci";
			else
				sTXSignalFromTCI = "";

			string s = "trx:" + rx.ToString() + "," + mox.ToString().ToLower() + sTXSignalFromTCI + ";";
			sendTextFrame(s);
		}
        private void sendAudioStartStop(int receiver, bool enable)
        {
            sendTextFrame((enable ? "audio_start:" : "audio_stop:") + receiver.ToString() + ";");
        }
		private void sendMode(int rx, DSPMode mode = DSPMode.FIRST)
        {
			if(mode == DSPMode.FIRST)
            {
                if (rx == 0)
					mode = consoleThreadSafe.RX1DSPMode;
				else if(rx == 1)
					mode = consoleThreadSafe.RX2DSPMode;
            }
			if (mode == DSPMode.FIRST || mode == DSPMode.LAST) return;

			string sMode;
			if (m_server != null && m_server.CWLUbecomesCW && (mode == DSPMode.CWL || mode == DSPMode.CWU))
			{
				sMode = "cw";
			}
			else
				sMode = mode.ToString().ToLower();

			string s = "modulation:" + rx.ToString() + "," + sMode.ToUpper() + ";"; // MW0LGE_22b mods are uppcase on the sun, replicate
			sendTextFrame(s);
		}
        private void sendMute(bool mute)
        {
            string s = "mute:" + mute.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendMuteRX(int rx, bool mute)
        {
            string s = "rx_mute:" + rx.ToString() + "," + mute.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
		private void sendMONEnable(bool enable)
		{
            string s = "mon_enable:" + enable.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendVolume(double volume)
        {
            if (volume < -60f || volume > 0f) return;

            string s = "volume:" + volume.ToString("F1", CultureInfo.InvariantCulture).ToLower() + ";";
            sendTextFrame(s);
        }
		private void sendMONVolume(double volume)
		{
            if (volume < -60f || volume > 0f) return;

            string s = "mon_volume:" + volume.ToString("F1", CultureInfo.InvariantCulture).ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendRxBalance(int rx, int chan, double balance)
        {
            string s = "rx_balance:" + rx.ToString() + "," + chan.ToString() + "," + balance.ToString("F2", CultureInfo.InvariantCulture) + ";";
            sendTextFrame(s);
        }
        private void sendRxStepAttEx(int rx, int attenuation)
        {
            string s = "rx_step_att_ex:" + rx.ToString() + "," + Math.Abs(attenuation).ToString(CultureInfo.InvariantCulture) + ";";
            sendTextFrame(s);
        }
        private void sendRxPreampAttEx(int rx, int attenuation)
        {
            string s = "rx_preamp_att_ex:" + rx.ToString() + "," + attenuation.ToString(CultureInfo.InvariantCulture) + ";";
            sendTextFrame(s);
        }
        private void sendRxStepAttEnabledEx(int rx, bool enabled)
        {
            string s = "rx_step_att_enabled_ex:" + rx.ToString() + "," + enabled.ToString().ToLowerInvariant() + ";";
            sendTextFrame(s);
        }
        private void sendVFOSyncEx(bool enabled)
        {
            string s = "vfo_sync_ex:" + enabled.ToString().ToLowerInvariant() + ";";
            sendTextFrame(s);
        }
        private void sendFMDeviationEx(int rx, int deviationHz)
        {
            string s = "fm_deviation_ex:" + rx.ToString() + "," + deviationHz.ToString(CultureInfo.InvariantCulture) + ";";
            sendTextFrame(s);
        }
        private void sendAgcAutoEx(int rx, bool enabled)
        {
            string s = "agc_auto_ex:" + rx.ToString() + "," + enabled.ToString().ToLowerInvariant() + ";";
            sendTextFrame(s);
        }
        private string agcModeToTciMode(AGCMode mode)
        {
            switch (mode)
            {
                case AGCMode.FIXD:
                    return "off";
                case AGCMode.LONG:
                    return "long";
                case AGCMode.SLOW:
                    return "slow";
                case AGCMode.FAST:
                    return "fast";
                case AGCMode.CUSTOM:
                    return "custom";
                case AGCMode.MED:
                    return "normal";
                default:
                    return "normal";
            }
        }
        private AGCMode tciModeToAgcMode(string mode)
        {
            switch (mode.Trim().ToLowerInvariant())
            {
                case "off":
                case "fixd":
                case "fixed":
                    return AGCMode.FIXD;
                case "long":
                    return AGCMode.LONG;
                case "slow":
                    return AGCMode.SLOW;
                case "fast":
                    return AGCMode.FAST;
                case "custom":
                    return AGCMode.CUSTOM;
                case "normal":
                case "med":
                case "medium":
                    return AGCMode.MED;
                default:
                    return AGCMode.MED;
            }
        }
        private void sendAgcMode(int rx, AGCMode mode)
        {
            string s = "agc_mode:" + rx.ToString() + "," + agcModeToTciMode(mode) + ";";
            sendTextFrame(s);
        }
        private void sendAgcGain(int rx, int gain)
        {
            string s = "agc_gain:" + rx.ToString() + "," + gain.ToString() + ";";
            sendTextFrame(s);
        }
		private void sendTXFrequencyChanged(long new_frequency, Band new_band, bool rx2_enabled, bool tx_vfob)
		{
            string s = $"tx_frequency:{new_frequency};";
            sendTextFrame(s.ToLower());

            // bespoke TCI command for anan to make life easier determining active TX frequency
            // format is : tx_frequency_ex:3700000,b80m,false,false;
            // arg1 freq (long)
            // arg2 band b80m, b40m etc
            // arg3 rx2 enabled  true/false
            // arg4 tx on vfoB  true/false
            s = $"tx_frequency_ex:{new_frequency},{new_band.ToString()},{rx2_enabled.ToString()},{tx_vfob.ToString()};";
            sendTextFrame(s.ToLower());
        }
        private void sendTunePower(int rx, int drive)
		{
			if (drive < 0 || drive > 100) return;

			string s = "tune_drive:" + rx.ToString() + "," + drive.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendDrivePower(int rx, int drive)
		{
			if (drive < 0 || drive > 100) return;

			string s = "drive:" + rx.ToString() + "," + drive.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendTune(int rx, bool tune)
		{
			string s = "tune:" + rx.ToString() + "," + tune.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendRXEnable(int rx, bool enable)
		{
			string s = "rx_enable:" + rx.ToString() + "," + enable.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendTXEnable(int rx, bool bEnable)
        {
			string s = "tx_enable:" + rx.ToString() + "," + bEnable.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendVFOLimits(int low, int high)
        {
			string s = "vfo_limits:" + low.ToString() + "," + high.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendAppFocus(bool focus)
        {
			string s = "app_focus:" + focus.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendIFLimits(int low, int high)
		{
			string s = "if_limits:" + low.ToString() + "," + high.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendClickedOnSpot(string callsign, long frequency)
		{
			string s = "clicked_on_spot:" + callsign.Trim() + "," + frequency.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendClickedOnSpotRX(int rx, int chan, string callsign, long frequency)
		{
			string s = "rx_clicked_on_spot:" + rx.ToString() + "," + chan.ToString() + "," + callsign.Trim() + "," + frequency.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendRxSensors(int rx, double levelDbm)
		{
			sendTextFrame("rx_sensors:" + rx.ToString() + "," + levelDbm.ToString("F1", CultureInfo.InvariantCulture) + ";");
		}
		private void sendRxChannelSensors(int rx, int channel, double levelDbm, double avgLevelDbm, double peakBinDbm)
		{
			sendTextFrame("rx_channel_sensors:" + rx.ToString() + "," + channel.ToString() + "," + levelDbm.ToString("F1", CultureInfo.InvariantCulture) + ";");
            sendTextFrame("rx_channel_sensors_ex:" + rx.ToString() + "," + channel.ToString() + "," + levelDbm.ToString("F1", CultureInfo.InvariantCulture) + "," + avgLevelDbm.ToString("F1", CultureInfo.InvariantCulture) + "," + peakBinDbm.ToString("F1", CultureInfo.InvariantCulture) + ";");
        }
		private void sendTxSensors(int rx, double micLevelDbm, double rmsPowerWatts, double peakPowerWatts, double swr)
		{
			string message = string.Format(
				"tx_sensors:{0},{1:F1},{2:F1},{3:F1},{4:F1};",
				rx,
				micLevelDbm,
				rmsPowerWatts,
				peakPowerWatts,
				swr);
			sendTextFrame(message);
		}
		private void sendDDS(int rx, long ddsFreq = -1)
        {
			if (ddsFreq == -1)
			{
				if (rx == 0)
					ddsFreq = (long)(consoleThreadSafe.CentreFrequency * 1e6);
				else if (rx == 1)
					ddsFreq = (long)(consoleThreadSafe.CentreRX2Frequency * 1e6);
			}

			ddsFreq += consoleThreadSafe.GetDSPcwPitchShiftToZero(rx+1); //MW0LGE [2.9.0.7]

            string s = "dds:" + rx.ToString() + "," + ddsFreq.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendFilterBand(int rx, int low, int high)
        {
			string s = "rx_filter_band:" + rx.ToString() + "," + low.ToString() + "," + high.ToString() + ";";
			sendTextFrame(s);
		}
        private void normalizeTXFilterBandForSet(ref int low, ref int high)
        {
            low = Math.Max(0, low);
            high = Math.Max(0, high);

            if (low > high)
            {
                int tmp = low;
                low = high;
                high = tmp;
            }

            if (high < low + 100)
                high = low + 100;
        }
        private void normalizeTXFilterBandForSend(ref int low, ref int high)
        {
            low = Math.Abs(low);
            high = Math.Abs(high);
            normalizeTXFilterBandForSet(ref low, ref high);
        }
        private void sendTXFilterBandEx(int low, int high)
        {
            normalizeTXFilterBandForSend(ref low, ref high);
            string s = "tx_filter_band_ex:" + low.ToString() + "," + high.ToString() + ";";
            sendTextFrame(s);
        }
		private void sendStartStop(bool bPower)
        {
			if (bPower)
				sendStart();
			else
				sendStop();
        }
        //
        private int preampModeToAttenuation(PreampMode mode)
        {
            switch (mode)
            {
                case PreampMode.HPSDR_OFF:
                    return 20;
                case PreampMode.HPSDR_ON:
                    return 0;
                case PreampMode.HPSDR_MINUS10:
                    return 10;
                case PreampMode.HPSDR_MINUS20:
                    return 20;
                case PreampMode.HPSDR_MINUS30:
                    return 30;
                case PreampMode.HPSDR_MINUS40:
                    return 40;
                case PreampMode.HPSDR_MINUS50:
                    return 50;
                case PreampMode.SA_MINUS10:
                    return 10;
                case PreampMode.SA_MINUS20:
                    return -20;
                case PreampMode.SA_MINUS30:
                    return 30;
                default:
                    return 0;
            }
        }

        private void sendInitialRadioState()
        {
			bool bSend = m_server != null ? m_server.SendInitialFrequencyStateOnConnect : true;
			bool bRX2Enabled = consoleThreadSafe.RX2Enabled;

            if (bSend)
			{
				sendDDS(0);
				sendDDS(1);
				sendIF(0, 0);
				sendIF(0, 1);
				sendIF(1, 1);
				sendIF(1, 1);
				sendVFO(0, 0);
				sendVFO(0, 1);
				sendVFO(1, 0);
				sendVFO(1, 1);

                //bespoke
                sendTXFrequencyChanged((long)(consoleThreadSafe.TXFreq * 1e6), consoleThreadSafe.TXBand, consoleThreadSafe.RX2Enabled, consoleThreadSafe.VFOBTX);
            }

            sendMode(0);
			sendMode(1);

            sendFilterBand(0, consoleThreadSafe.RX1FilterLow, consoleThreadSafe.RX1FilterHigh);
            sendFilterBand(1, consoleThreadSafe.RX2FilterLow, consoleThreadSafe.RX2FilterHigh);
            sendTXFilterBandEx(consoleThreadSafe.TXFilterLow, consoleThreadSafe.TXFilterHigh);

            sendRXEnable(0, !consoleThreadSafe.MOX);
			sendRXEnable(1, bRX2Enabled && !consoleThreadSafe.MOX);

            //NR
            for (int rx = 1; rx <= 2; rx++)
            {
                int nr = consoleThreadSafe.GetSelectedNR(rx);
                int zeroBaseRx = rx - 1;

                sendNREnable(zeroBaseRx, nr > 0, false, nr);
                sendNREnable(zeroBaseRx, nr > 0, true, nr);
            }
            //NB
            for (int rx = 1; rx <= 2; rx++)
            {
                int nb = consoleThreadSafe.GetSelectedNB(rx);
                int zeroBaseRx = rx - 1;

                sendNBEnable(zeroBaseRx, nb > 0, false, nb);
                sendNBEnable(zeroBaseRx, nb > 0, true, nb);
            }

            sendRxBinEnable(0, consoleThreadSafe.GetBin(1));
            sendRxBinEnable(1, consoleThreadSafe.GetBin(2));

            sendAnfEnable(0, consoleThreadSafe.GetANF(1));
            sendAnfEnable(1, consoleThreadSafe.GetANF(2));
            if (!consoleThreadSafe.IsSetupFormNull)
            {
                sendRxApfEnable(0, consoleThreadSafe.SetupForm.RX1APFEnable);
                sendRxApfEnable(1, consoleThreadSafe.SetupForm.RX2APFEnable);
            }
            sendRxNfEnable(0, consoleThreadSafe.GetMNF(1));
            sendRxNfEnable(1, consoleThreadSafe.GetMNF(2));

            double rx1vol = audioGainToDb(consoleThreadSafe.RX0Gain / 100f);
            double rx1Subvol = audioGainToDb(consoleThreadSafe.RX1Gain / 100f);
            double rx2vol = audioGainToDb(consoleThreadSafe.RX2Gain / 100f);

            sendRxVolume(0, 0, rx1vol);
            sendRxVolume(0, 1, rx1Subvol);
            sendRxVolume(1, 0, rx2vol);
            sendRxVolume(1, 1, rx2vol);
            sendRxBalance(0, 0, 40.0 - (consoleThreadSafe.GetBal(1, false) * 0.8));
            sendRxBalance(0, 1, 40.0 - (consoleThreadSafe.GetBal(1, true) * 0.8));
            sendRxBalance(1, 0, 40.0 - (consoleThreadSafe.GetBal(2, false) * 0.8));
            sendRxBalance(1, 1, 40.0 - (consoleThreadSafe.GetBal(2, true) * 0.8));
            sendVFOSyncEx(consoleThreadSafe.VFOSync);
            sendRxStepAttEnabledEx(0, consoleThreadSafe.RX1StepAttEnabled);
            sendRxStepAttEnabledEx(1, consoleThreadSafe.RX2StepAttEnabled);
            sendRxStepAttEx(0, consoleThreadSafe.RX1AttenuatorData);
            sendRxStepAttEx(1, consoleThreadSafe.RX2AttenuatorData);
            sendRxPreampAttEx(0, -preampModeToAttenuation(consoleThreadSafe.RX1PreampMode));
            sendRxPreampAttEx(1, -preampModeToAttenuation(consoleThreadSafe.RX2PreampMode));
            sendAgcMode(0, consoleThreadSafe.GetAGCMode(1));
            sendAgcMode(1, consoleThreadSafe.GetAGCMode(2));
            sendAgcGain(0, consoleThreadSafe.GetAgcT(1));
            sendAgcGain(1, consoleThreadSafe.GetAgcT(2));
            sendAgcAutoEx(0, consoleThreadSafe.GetAGCAuto(1));
            sendAgcAutoEx(1, consoleThreadSafe.GetAGCAuto(2));
            sendFMDeviationEx(0, consoleThreadSafe.FMDeviation_Hz);
            sendFMDeviationEx(1, consoleThreadSafe.FMDeviation_Hz);

            sendCTUN(0, consoleThreadSafe.GetCTUN(1));
            sendCTUN(1, consoleThreadSafe.GetCTUN(2));

            sendTXProfiles();
            sendTXProfile(consoleThreadSafe.TXProfile);

            CalibrationChanged(0);
            CalibrationChanged(1);

            sendRITEnable(0, consoleThreadSafe.RITOn);
            sendRITEnable(1, consoleThreadSafe.RITOn);
            sendXITEnable(0, consoleThreadSafe.XITOn);
            sendXITEnable(1, consoleThreadSafe.XITOn);
            sendRITOffset(0, consoleThreadSafe.RITValue);
            sendRITOffset(1, consoleThreadSafe.RITValue);
            sendXITOffset(0, consoleThreadSafe.XITValue);
            sendXITOffset(1, consoleThreadSafe.XITValue);
            sendLock(0, consoleThreadSafe.VFOALock);
            if (bRX2Enabled)
            {
                sendLock(1, consoleThreadSafe.VFOBLock);
            }
            sendAllVFOLocks();
            sendSqlEnable(0, consoleThreadSafe.GetSqlMode(1) != SquelchState.OFF);
            sendSqlEnable(1, consoleThreadSafe.GetSqlMode(2) != SquelchState.OFF);
            sendSqlLevel(0, consoleThreadSafe.GetSql(1));
            sendSqlLevel(1, consoleThreadSafe.GetSql(2));
            sendDiglOffset(consoleThreadSafe.DIGLClickTuneOffset);
            sendDiguOffset(consoleThreadSafe.DIGUClickTuneOffset);

            if (m_server != null)
            {
                sendCwMacrosSpeed(m_server.GetCwMacrosSpeed());
                sendCwMacrosDelay(m_server.GetCwMacrosDelay());
                sendCwKeyerSpeed(m_server.GetCwKeyerSpeed());
            }

            sendSplit(0, consoleThreadSafe.VFOSplit);
			sendSplit(1, bRX2Enabled && consoleThreadSafe.VFOSplit);

			sendTXEnable(0, !consoleThreadSafe.MOX);
			sendTXEnable(1, bRX2Enabled && !consoleThreadSafe.MOX);

            sendRxChannelEnable(0, 0, true);
            sendRxChannelEnable(0, 1, consoleThreadSafe.GetSubRX(1));
            sendRxChannelEnable(1, 0, bRX2Enabled);
			sendRxChannelEnable(1, 1, false); // no sub rx on rx2

            //sendDrivePower()
            //sendTunePower()
            handleDrive(new string[] { "0" });
            handleDrive(new string[] { "1" });
            handleTuneDrive(new string[] { "0" });
            handleTuneDrive(new string[] { "1" });

            sendMOX(0, consoleThreadSafe.MOX && !(consoleThreadSafe.VFOBTX && bRX2Enabled));
            sendMOX(1, consoleThreadSafe.MOX && (consoleThreadSafe.VFOBTX && bRX2Enabled));

            sendTune(0, consoleThreadSafe.TUN && !(consoleThreadSafe.VFOBTX && bRX2Enabled));
            sendTune(1, consoleThreadSafe.TUN && (consoleThreadSafe.VFOBTX && bRX2Enabled));

            sendIQStartStop(0, false);
            sendIQStartStop(1, false);

            sendIQSampleRate(getPublishedIQSampleRate());
			sendAudioSampleRate(m_audioSampleRate);
			sendAudioStreamSampleType(m_audioSampleType);
			sendAudioStreamChannels(m_audioStreamChannels);
			sendAudioStreamSamples(m_audioStreamSamples);
			sendTxStreamAudioBuffering(m_txStreamAudioBufferingMs);

			sendMute(consoleThreadSafe.MUT || (consoleThreadSafe.MUT2 && bRX2Enabled));
			sendMuteRX(0, consoleThreadSafe.MUT);
            sendMuteRX(1, consoleThreadSafe.MUT2);
            sendVolume(linearToDbVolume(consoleThreadSafe.AF));

			sendMONEnable(consoleThreadSafe.MON);
            sendMONVolume(linearToDbVolume(consoleThreadSafe.TXAF));

            sendStartStop(consoleThreadSafe.PowerOn);// MW0LGE_22b moved here to replicate sun

			Debug.Print("SENT INITIAL STATE");
		}

		private void sendInitialisationData()
        {
			string sProtocol; //MW0LGE_22 emulate ee3 protocol
			if (m_server != null && m_server.EmulateExpertSDR3Protocol)
				sProtocol = "ExpertSDR3";
            else
				sProtocol = "Thetis";

			sendTextFrame("protocol:" + sProtocol + ",2.0;");

			string sDevice; //MW0LGE_22 emulate sunsdr
			if (m_server != null && m_server.EmulateSunSDR2Pro)
				sDevice = "SunSDR2PRO";
			else
				sDevice = HardwareSpecific.Model.ToString();

			sendTextFrame("device:" + sDevice + ";");
			sendTextFrame("receive_only:false;");
			sendTextFrame("trx_count:2;");
			sendTextFrame("channels_count:2;");

			sendVFOLimits(0, (int)(consoleThreadSafe.MaxFreq * 1e6));

			int halfSample = consoleThreadSafe.SampleRateRX1 / 2;
			sendIFLimits(-halfSample, halfSample); // only VFOA/rx1

			string sCW;
			if (m_server != null)
				sCW = m_server.CWLUbecomesCW ? "cwl,cwu,cw" : "cwl,cwu";
			else
				sCW = "cwl,cwu";

			sendTextFrame("modulations_list:" + ("am,sam,dsb,lsb,usb,nfm,fm,digl,digu," + sCW).ToUpper() + ";"); // MW0LGE_22b modulations are upper in sun, so replicate

			sendInitialRadioState();

			sendTextFrame("ready;");

			Debug.Print("SENT INIT DATA");
            m_server?.RefreshStreamRunState();
		}

		private void setRxSensorsEnabled(bool enabled, int intervalMs, bool fireImmediately)
		{
			m_sensorManager.ConfigureRxSensors(enabled, intervalMs);

			if (m_tmRxSensors != null)
			{
				m_tmRxSensors.Change(Timeout.Infinite, Timeout.Infinite);
				m_tmRxSensors.Dispose();
				m_tmRxSensors = null;
			}

			if (enabled)
			m_tmRxSensors = new System.Threading.Timer(RxSensorsTimerCallback, null, fireImmediately ? 0 : m_sensorManager.RxIntervalMs, m_sensorManager.RxIntervalMs);
		}

		private void setTxSensorsEnabled(bool enabled, int intervalMs, bool fireImmediately)
		{
			m_sensorManager.ConfigureTxSensors(enabled, intervalMs);

			if (m_tmTxSensors != null)
			{
				m_tmTxSensors.Change(Timeout.Infinite, Timeout.Infinite);
				m_tmTxSensors.Dispose();
				m_tmTxSensors = null;
			}

			if (enabled)
			m_tmTxSensors = new System.Threading.Timer(TxSensorsTimerCallback, null, fireImmediately ? 0 : m_sensorManager.TxIntervalMs, m_sensorManager.TxIntervalMs);
		}

		private void RxSensorsTimerCallback(object state)
		{
			if (m_stopClient || m_disconnected) return;

			bool rx2Enabled = false;
			bool rx1SubEnabled = false;
            try
            {
                rx2Enabled = consoleThreadSafe.RX2Enabled;
                rx1SubEnabled = !rx2Enabled && consoleThreadSafe.GetSubRX(1);
            }
            catch
            { }

			if (m_sensorManager.TryGetRxChannelReadingForSend(0, 0, out double rx1Main_sig, out double rx1Main_avgsig, out double rx1Main_peak_bin_sig))
			{
			    sendRxSensors(0, rx1Main_sig);
			    sendRxChannelSensors(0, 0, rx1Main_sig, rx1Main_avgsig, rx1Main_peak_bin_sig);
			    m_sensorManager.ConsumeRxChannelReading(0, 0);
		    }

			if (rx1SubEnabled && m_sensorManager.TryGetRxChannelReadingForSend(0, 1, out double rx1Sub_sig, out double rx1Sub_avgsig, out double rx1Sub_peak_bin_sig))
		    {
				sendRxChannelSensors(0, 1, rx1Sub_sig, rx1Sub_avgsig, rx1Sub_peak_bin_sig);
				m_sensorManager.ConsumeRxChannelReading(0, 1);
			}

			if (rx2Enabled && m_sensorManager.TryGetRxChannelReadingForSend(1, 0, out double rx2Main_sig, out double rx2Miain_avgsig, out double rx2Main_peak_bin_sig))
			{
				sendRxSensors(1, rx2Main_sig);
				sendRxChannelSensors(1, 0, rx2Main_sig, rx2Miain_avgsig, rx2Main_peak_bin_sig);
				m_sensorManager.ConsumeRxChannelReading(1, 0);
			}
		}

		private void TxSensorsTimerCallback(object state)
		{
			if (m_stopClient || m_disconnected) return;

			if (m_sensorManager.TryGetTxReadingsForSend(out double micLevelDbm, out double powerWatts, out double peakPowerWatts, out double swr))
			{
			    sendTxSensors(0, micLevelDbm, powerWatts, peakPowerWatts, swr);
			    sendTxSensors(1, micLevelDbm, powerWatts, peakPowerWatts, swr);
				m_sensorManager.ConsumeTxReadings();
			}
		}

		private int findEndOfHeader(byte[] bytes)
		{
			int nFind = 0;
			for (int i = 0; i <= bytes.Length-4; i++)
			{
				if (bytes[i] == '\r' &&
					bytes[i + 1] == '\n' &&
					bytes[i + 2] == '\r' &&
					bytes[i + 3] == '\n')
				{
					nFind = i + 4;
					break;
				}
			}
			return nFind;
		}		

		private void SocketListenerThreadStart()
		{
			System.Threading.Timer t = new System.Threading.Timer(new TimerCallback(PingFrameTimer),
				null, 1000 * 20, 1000 * 20); // per websock spec ping frames are every 20 seconds.
											 // Ideally we should receive something
											// back within 20 seconds, but just use it to cause exception
											// on socket if client has dc'ed without telling us with a disconnect frame
            byte[] bytes = new byte[SOCKET_READ_BUFFER_SIZE];

			Debug.Print("TCPIP TCI Client Connected !");
			ClientConnectedHandlers?.Invoke();

			//SendClientData("# Thetis TCP/IP TCI #" + Environment.NewLine);

			while (!m_stopClient)
			{
				try
				{
                    if (m_stream == null || m_client == null)
					{
                        m_stopClient = true;
                        continue;
                    }

                    Socket socket = m_client.Client;
                    if (socket == null)
                    {
                        m_stopClient = true;
                        continue;
                    }

                    if (!socket.Poll(SOCKET_READ_TIMEOUT_MS * 1000, SelectMode.SelectRead))
                        continue;

                    if (socket.Available < 1)
                    {
                        m_stopClient = true;
                        continue;
                    }

					int nRead = m_stream.Read(bytes, 0, bytes.Length);
                    if (nRead < 1)
                    {
                        m_stopClient = true;
                        continue;
                    }

                    _m_buffer.AddRange(bytes.Take(nRead));

					if (!m_bWebSocket)
					{
                        byte[] bufferedBytes = _m_buffer.ToArray();
                        int nStart = findEndOfHeader(bufferedBytes);
                        if (nStart > 0)
                        {
                            string msg = Encoding.UTF8.GetString(bufferedBytes, 0, nStart);
                            if (Regex.IsMatch(msg, "^GET", RegexOptions.IgnoreCase))
							{

								if (upgradeToWebSocket(msg))
								{
									m_bWebSocket = true;
									Debug.Print("Upgraded to websocket");

                                    _m_buffer.RemoveRange(0, nStart);
									sendInitialisationData();
								}
								else
								{
									Debug.Print("Not Upgraded to websocket");
									m_stopClient = true;
								}
							}
						}
                    }

					if (m_bWebSocket)
					{
                        byte[] bytesAsArray = _m_buffer.ToArray();
						int frameLen = GetFrameLength(bytesAsArray);
						while (!m_stopClient && frameLen > -1 && bytesAsArray.Length >= frameLen)
						{
							// enough data to process a frame, dump bytes from the buffer
							_m_buffer.RemoveRange(0, frameLen);

							ParseReceiveBuffer(bytesAsArray);

							bytesAsArray = _m_buffer.ToArray();
							frameLen = GetFrameLength(bytesAsArray);
						}
					}
				}
                catch (IOException ioEx) when (isSocketReadTimeout(ioEx))
                {
                    continue;
				}
				catch (SocketException se)
				{
					m_stopClient = true;
					ClientErrorHandlers?.Invoke(se);
				}
                catch
                {
					m_stopClient = true;
				}
			}

            m_markedForDeletion = true;

            t.Change(Timeout.Infinite, Timeout.Infinite);
			t = null;

			Debug.Print("TCPIP TCI Client Disconnected !");
			m_disconnected = true;
            notifyServerDisconnected();
			ClientDisconnectedHandlers?.Invoke();
		}
        private void notifyServerDisconnected(TCPIPtciServer server = null)
        {
            if (Interlocked.Exchange(ref m_disconnectNotified, 1) != 0) return;

            try
            {
                TCPIPtciServer target = server != null ? server : m_server;
                target?.OnSocketListenerDisconnected(this);
            }
            catch { }
        }
		private void sendPingFrame(string sMsg)
		{
			try
			{
				if (!m_stopClient && !m_disconnected && m_bWebSocket && m_client != null && m_stream != null)
				{
					if (m_client.Connected)
					{
                        enqueueOutboundFrame(getFrameFromString(sMsg, EOpcodeType.Ping), null, TCIOutboundPriority.Urgent);
					}
				}
			}
			catch
			{
				Debug.Print("problem writing ping frame");
				m_stopClient = true;
			}
		}
		private void sendPongFrame(string sMsg)
		{
			try
			{
				if (!m_stopClient && !m_disconnected && m_bWebSocket && m_client != null && m_stream != null)
				{
					if (m_client.Connected)
					{
                        enqueueOutboundFrame(getFrameFromString(sMsg, EOpcodeType.Pong), null, TCIOutboundPriority.Urgent);
					}
				}
			}
			catch
			{
				Debug.Print("problem writing pong frame");
				m_stopClient = true;
			}
		}
		private void sendTextFrame(string sMsg)
		{
			try
			{
				if (!m_stopClient && !m_disconnected && m_bWebSocket && m_client != null && m_stream != null)
				{
                    if (m_client.Connected)
					{
                        enqueueOutboundFrame( getFrameFromString(sMsg, EOpcodeType.Text),
                            sMsg, TCIOutboundPriority.Control, getCoalescedTextFrameKey(sMsg));
					}
				}
			}
			catch(Exception)
			{
				Debug.Print("problem writing text frame");
				m_stopClient = true;
			}
		}
		private void sendBinaryFrame(byte[] payload)
		{
			try
			{
				if (!m_stopClient && !m_disconnected && m_bWebSocket && m_client != null && m_stream != null)
				{
					if (m_client.Connected)
					{
                        enqueueOutboundFrame(GetFrameFromBytes(payload, EOpcodeType.Binary), null, TCIOutboundPriority.Binary);
					}
				}
			}
			catch
			{
				Debug.Print("problem writing binary frame");
				m_stopClient = true;
			}
		}

		private void sendCloseFrame()
		{
            try
            {
                if (!m_stopClient && m_bWebSocket && m_client != null && m_stream != null)
                {
                    if (m_client.Connected)
                    {
                        enqueueOutboundFrame(getFrameFromString("", EOpcodeType.ClosedConnection), null, TCIOutboundPriority.Urgent);
                    }
                }
            }
            catch { }
		}
		public void StopSocketListener()
		{
			TCPIPtciServer server = m_server;
            notifyServerDisconnected(server);
			lock (m_objStreamLock)
			{
				m_txUsesTCIAudio = false;
                m_tciPttActive = false;
			}
			clearQueuedTxAudio();
            clearRxAudioStreamState();
            server?.ReleaseActiveTxAudioListener(this);
			server?.RefreshTxAudioSourceState();
            m_server?.RefreshStreamRunState();
			if (m_client != null)
			{
                if (m_tmVFOtimer != null)
                {
                    m_tmVFOtimer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_tmVFOtimer = null;
                }
                if (m_tmCentretimer != null)
                {
                    m_tmCentretimer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_tmCentretimer = null;
                }
                if (m_tmTXFrequency != null)
                {
                    m_tmTXFrequency.Change(Timeout.Infinite, Timeout.Infinite);
                    m_tmTXFrequency = null;
                }
                if (m_tmRxSensors != null)
                {
                    m_tmRxSensors.Change(Timeout.Infinite, Timeout.Infinite);
                    m_tmRxSensors.Dispose();
                    m_tmRxSensors = null;
                }
                if (m_tmTxSensors != null)
                {
                    m_tmTxSensors.Change(Timeout.Infinite, Timeout.Infinite);
                    m_tmTxSensors.Dispose();
                    m_tmTxSensors = null;
                }

                if (m_stream != null)
                {
					sendStop();
					
					sendCloseFrame();

                    flushOutboundFrames(100);

					m_stream.Close();
					m_stream = null;
				}

				m_stopClient = true;
                m_outboundFrameEvent.Set();
				m_client.Close();

                if (m_sendThread != null)
                {
                    m_sendThread.Join(100);

                    if (m_sendThread.IsAlive)
                    {
                        m_sendThread.Abort();
                    }
                    m_sendThread = null;
                }

				if (m_VFODataThread != null)
				{
					m_VFODataThread.Join(50);

					if (m_VFODataThread.IsAlive)
					{
						m_VFODataThread.Abort();
					}
					m_VFODataThread = null;
				}

				if (m_clientListenerThread != null)
				{
					m_clientListenerThread.Join(50);

					if (m_clientListenerThread.IsAlive)
					{
						m_clientListenerThread.Abort();
						m_disconnected = true;
                        notifyServerDisconnected(server);
						ClientDisconnectedHandlers?.Invoke();
					}

					m_clientListenerThread = null;
				}

				m_server = null;
				m_client = null;
                clearOutboundFrames();
				m_markedForDeletion = true;
			}
		}

		public bool IsMarkedForDeletion()
		{
			return m_markedForDeletion;
		}
		public bool IsDisconnected()
		{
			return m_disconnected;
		}
		private int GetFrameLength(Byte[] bytes)
		{
            //https://noio-ws.readthedocs.io/en/latest/overview_of_websockets.html

            if (bytes.Length < 2) return -1; // at least 2 bytes to get payload length

            bool mask = (bytes[1] & 0b10000000) != 0;	// frame is masked bit
            int payloadLen = bytes[1] & 0b01111111;

			if (payloadLen <= 125)
			{
				return mask ? payloadLen + 6 : payloadLen + 2;
            }
			else if (payloadLen == 126)
			{
                // need more bytes
				if(bytes.Length < 4) return -1;
                int extendedLen = (int)BitConverter.ToInt16(new byte[] { bytes[3], bytes[2] }, 0);
                return mask ? extendedLen + 8 : extendedLen + 4;
            }
            else if (payloadLen == 127)
            {
                // need more bytes
                if (bytes.Length < 10) return -1;
                int extendedLen = (int)BitConverter.ToInt64(new byte[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] }, 0);
                return mask ? extendedLen + 14 : extendedLen + 10;
            }

            return -1;
        }
		private void ParseReceiveBuffer(Byte[] bytes)
		{
			if (bytes.Length == 0) return;

            bool mask = (bytes[1] & 0b10000000) != 0;
			EOpcodeType opcode = (EOpcodeType)(bytes[0] & 0b00001111);
			
			if (opcode == EOpcodeType.ClosedConnection)
			{
				sendCloseFrame();

				m_stopClient = true;
				return;
			}

            int dataLength = 0;
            int keyIndex = 0;
            int payloadLen = bytes[1] & 0b01111111;

            if (payloadLen <= 125)
            {
                keyIndex = 2;
				dataLength = payloadLen;
            }
            else if (payloadLen == 126)
            {
                keyIndex = 4;
                dataLength = (int)BitConverter.ToInt16(new byte[] { bytes[3], bytes[2] }, 0);
            }
            else if (payloadLen == 127)
            {
                keyIndex = 10;
                dataLength = (int)BitConverter.ToInt64(new byte[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] }, 0);
            }

			int dataIndex = keyIndex;

			if (mask)
			{
				byte[] key = new byte[] { bytes[keyIndex], bytes[keyIndex + 1], bytes[keyIndex + 2], bytes[keyIndex + 3] };

				dataIndex += 4;
				for (int i = 0; i < dataLength; i++)
				{
					bytes[dataIndex + i] = (byte)(bytes[dataIndex + i] ^ key[i % 4]);
				}
			}

			if (opcode == EOpcodeType.Text)
			{
				parseTextFrame(Encoding.UTF8.GetString(bytes, dataIndex, dataLength));
			}
			else if(opcode == EOpcodeType.Ping)
            {
				sendPongFrame("Thetis");
            }
			else if(opcode == EOpcodeType.Binary)
            {
                byte[] payload = new byte[dataLength];
                Buffer.BlockCopy(bytes, dataIndex, payload, 0, dataLength);
                handleBinaryFrame(payload);
            }
			//else
			//{
			//	Debug.Print("UNNOWN OPCODE " + opcode.ToString());
			//}
		}
		private void handleSetInFocus()
        {
			consoleThreadSafe.Focus();
        }
		private void handleStart()
        {
			if(!consoleThreadSafe.PowerOn)
				consoleThreadSafe.PowerOn = true;
        }
		private void handleStop()
		{
            if (consoleThreadSafe.PowerOn)
				consoleThreadSafe.PowerOn = false;
		}
		private void handleSpotClear()
		{
			SpotManager2.ClearAllSpots(true, false);
		}
		private void handleSplitEnableMessage(string[] args)
		{
			int rx = 0;
			bool bSplit = false;

			bool bOK = int.TryParse(args[0], out rx);
			if (args.Length == 2)
			{
				// set
				if (bOK)
					bOK = bool.TryParse(args[1], out bSplit);

				if (bOK)
				{
					if (!consoleThreadSafe.IsSetupFormNull)
					{
                        if (consoleThreadSafe.SetupForm.SplitFromCATorTCIcancelsQSPLIT)
                        {
                            if (consoleThreadSafe.SetupForm.QuickSplitEnabled)
                                consoleThreadSafe.SetupForm.QuickSplitEnabled = false;
                        }
                    }
					if (rx == 0 || rx == 1)
						if(consoleThreadSafe.VFOSplit != bSplit)
							consoleThreadSafe.VFOSplit = bSplit;
				}
			}
			else if(args.Length == 1)
			{
				// get
				if (bOK)
				{
					bool bSplitGet = consoleThreadSafe.VFOSplit;
					sendSplit(rx, bSplitGet);
				}
			}
		}
        private void handleRITEnableMessage(string[] args)
        {
            if (args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;

            if (args.Length == 2)
            {
                if (!bool.TryParse(args[1], out bool enabled)) return;
                if (rx == 0 || rx == 1)
                    consoleThreadSafe.RITOn = enabled;
            }
            else
            {
                sendRITEnable(rx, consoleThreadSafe.RITOn);
            }
        }
        private void handleXITEnableMessage(string[] args)
        {
            if (args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;

            if (args.Length == 2)
            {
                if (!bool.TryParse(args[1], out bool enabled)) return;
                if (rx == 0 || rx == 1)
                    consoleThreadSafe.XITOn = enabled;
            }
            else
            {
                sendXITEnable(rx, consoleThreadSafe.XITOn);
            }
        }
        private void handleRITOffsetMessage(string[] args)
        {
            if (args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;

            if (args.Length == 2)
            {
                if (!int.TryParse(args[1], out int offset)) return;
                if (rx == 0 || rx == 1)
                    consoleThreadSafe.RITValue = offset;
            }
            else
            {
                sendRITOffset(rx, consoleThreadSafe.RITValue);
            }
        }
        private void handleXITOffsetMessage(string[] args)
        {
            if (args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;

            if (args.Length == 2)
            {
                if (!int.TryParse(args[1], out int offset)) return;
                if (rx == 0 || rx == 1)
                    consoleThreadSafe.XITValue = offset;
            }
            else
            {
                sendXITOffset(rx, consoleThreadSafe.XITValue);
            }
        }

        private void handleRxBinEnable(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                sendRxBinEnable(rx, consoleThreadSafe.GetBin(rx + 1));
            }
            else
            {
                if (!bool.TryParse(args[1], out bool enabled)) return;
                consoleThreadSafe.SetBin(rx + 1, enabled);
            }
        }
        private void handleRxApfEnable(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;
            if (consoleThreadSafe.IsSetupFormNull) return;

            if (args.Length == 1)
            {
                bool enabled = rx == 0 ? consoleThreadSafe.SetupForm.RX1APFEnable : consoleThreadSafe.SetupForm.RX2APFEnable;
                sendRxApfEnable(rx, enabled);
            }
            else
            {
                if (!bool.TryParse(args[1], out bool enabled)) return;
                    if (rx == 0)
                {
                    consoleThreadSafe.SetupForm.RX1APFEnable = enabled;
                }
                else
                {
                    consoleThreadSafe.SetupForm.RX2APFEnable = enabled;
                }
            }
        }
        private void handleRxNfEnable(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                sendRxNfEnable(rx, consoleThreadSafe.GetMNF(rx + 1));
            }
            else
            {
                if (!bool.TryParse(args[1], out bool enabled)) return;
                consoleThreadSafe.TNFActive = enabled;
            }
        }
        private void handleLock(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                sendLock(rx, rx == 0 ? consoleThreadSafe.VFOALock : consoleThreadSafe.VFOBLock);
            }
            else
            {
                if (!bool.TryParse(args[1], out bool enabled)) return;
                if (rx == 0)
                    consoleThreadSafe.VFOALock = enabled;
                else
                    consoleThreadSafe.VFOBLock = enabled;
            }
        }
        private void handleVFOLock(string[] args)
        {
            if (args == null || args.Length < 2 || args.Length > 3) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (!int.TryParse(args[1], out int chan)) return;

            if (args.Length == 2)
            {
                if (tryGetVFOLockState(rx, chan, out bool enabled))
                    sendVFOLock(rx, chan, enabled);
            }
            else
            {
                if (!bool.TryParse(args[2], out bool enabled)) return;
                trySetVFOLockState(rx, chan, enabled);
            }
        }
        private void handleSqlEnable(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                sendSqlEnable(rx, consoleThreadSafe.GetSqlMode(rx + 1) != SquelchState.OFF);
            }
            else
            {
                if (!bool.TryParse(args[1], out bool enabled)) return;
                consoleThreadSafe.SetSqlMode(rx + 1, enabled ? SquelchState.SQL : SquelchState.OFF);
            }
        }
        private void handleSqlLevel(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                sendSqlLevel(rx, consoleThreadSafe.GetSql(rx + 1));
            }
            else
            {
                if (!int.TryParse(args[1], out int level)) return;
                level = Math.Max(-140, Math.Min(0, level));
                consoleThreadSafe.SetSql(rx + 1, level);
            }
        }
        private void handleDiglOffset(string[] args, bool hasArgs = true)
        {
            if (!hasArgs || args == null || args.Length == 0)
            {
                sendDiglOffset(consoleThreadSafe.DIGLClickTuneOffset);
                return;
            }

            if (!int.TryParse(args[0], out int offset)) return;
            offset = Math.Max(0, Math.Min(4000, offset));
            consoleThreadSafe.DIGLClickTuneOffset = offset;
        }
        private void handleDiguOffset(string[] args, bool hasArgs = true)
        {
            if (!hasArgs || args == null || args.Length == 0)
            {
                sendDiguOffset(consoleThreadSafe.DIGUClickTuneOffset);
                return;
            }

            if (!int.TryParse(args[0], out int offset)) return;
            offset = Math.Max(0, Math.Min(4000, offset));
            consoleThreadSafe.DIGUClickTuneOffset = offset;
        }
        private void handleCwMacrosSpeed(string[] args, bool hasArgs = true)
        {
            if (!hasArgs || args == null || args.Length < 1)
            {
                if (m_server != null)
                    sendCwMacrosSpeed(m_server.GetCwMacrosSpeed());
                return;
            }

            if (!int.TryParse(args[0], out int speed)) return;
            m_server?.SetCwMacrosSpeed(speed);
        }
        private void handleCwMacrosDelay(string[] args, bool hasArgs = true)
        {
            if (!hasArgs || args == null || args.Length < 1)
            {
                if (m_server != null)
                    sendCwMacrosDelay(m_server.GetCwMacrosDelay());
                return;
            }

            if (!int.TryParse(args[0], out int delayMs)) return;
            m_server?.SetCwMacrosDelay(delayMs);
        }
        private void handleCwKeyerSpeed(string[] args, bool hasArgs = true)
        {
            if (!hasArgs || args == null || args.Length < 1)
            {
                if (m_server != null)
                    sendCwKeyerSpeed(m_server.GetCwKeyerSpeed());
                return;
            }

            if (!int.TryParse(args[0], out int speed)) return;
            m_server?.SetCwKeyerSpeed(speed);
        }
        private void handleCwMacrosSpeedUp(string[] args)
        {
            if (args == null || args.Length != 1) return;
            if (!int.TryParse(args[0], out int amount)) return;
            m_server?.IncreaseCwMacrosSpeed(amount);
        }
        private void handleCwMacrosSpeedDown(string[] args)
        {
            if (args == null || args.Length != 1) return;
            if (!int.TryParse(args[0], out int amount)) return;
            m_server?.DecreaseCwMacrosSpeed(amount);
        }
        private void handleCwMacros(string[] args)
        {
            if (args == null || args.Length < 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            string text = string.Join(",", args.Skip(1).ToArray());
            m_server?.SendCwMacro(this, rx, text);
        }
        private void handleCwTerminal(string[] args)
        {
            if (args == null || args.Length != 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;
            if (!bool.TryParse(args[1], out bool enabled)) return;

            m_server?.SetCwTerminalEnabled(this, rx, enabled);
        }
        private void handleCwMsg(string[] args)
        {
            if (args == null || args.Length < 1) return;

            if (args.Length == 1)
            {
                m_server?.UpdateCwMessageCallsign(this, args[0]);
                return;
            }

            if (args.Length < 4) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            string prefix = args[1];
            string callsign = args[2];
            string suffix = string.Join(",", args.Skip(3).ToArray());
            m_server?.SendCwMessage(this, rx, prefix, callsign, suffix);
        }
        private void handleCwMacrosStop()
        {
            m_server?.StopCwMacros(this);
        }
        private void handleKeyer(string[] args)
        {
            if (args == null || args.Length < 2 || args.Length > 3) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;
            if (!bool.TryParse(args[1], out bool pressed)) return;

            int durationMs = 0;
            if (args.Length > 2 && !int.TryParse(args[2], out durationMs)) return;

            m_server?.HandleCwKeyer(this, rx, pressed, Math.Max(0, durationMs));
        }
		private void handleTrxMessage(string[] args)
		{
			int rx = 0;
			bool bMox = false;

			bool bOK = int.TryParse(args[0], out rx);
			if (bOK && args.Length > 1)
				bOK = bool.TryParse(args[1], out bMox);

			if (args.Length > 1)
			{ 
                if (bOK && shouldIgnoreTrxForCurrentCwBreakIn())
                {
                    bool releaseOwner;
                    lock (m_objStreamLock)
                    {
                        releaseOwner = m_tciPttActive;
                        m_txUsesTCIAudio = false;
                        m_tciPttActive = false;
                    }

                    clearQueuedTxAudio();
                    if (releaseOwner)
                        m_server?.ReleaseActiveTxAudioListener(this);

                    m_server?.RefreshTxAudioSourceState();
                    m_server?.RefreshStreamRunState();
                    return;
                }

                bool useTciAudio = args.Length > 2 && args[2].ToLower() == "tci";
                bool alreadyMox = consoleThreadSafe.MOX;
                bool alreadyActiveTciPtt;
                lock (m_objStreamLock)
                {
                    alreadyActiveTciPtt = m_tciPttActive;
                }

                bool wantsActiveTciPtt = useTciAudio && bOK && bMox && (!alreadyMox || alreadyActiveTciPtt);
                bool ownsActiveTciPtt = false;

                if (wantsActiveTciPtt)
                {
                    if (m_server != null)
                        ownsActiveTciPtt = m_server.TryAcquireActiveTxAudioListener(this);
                    else
                        ownsActiveTciPtt = true;
                }
                else
                {
                    if (m_server != null)
                        m_server.ReleaseActiveTxAudioListener(this);
                }

                lock (m_objStreamLock)
                {
                    m_txUsesTCIAudio = useTciAudio;
                    m_tciPttActive = wantsActiveTciPtt && ownsActiveTciPtt;
                }

                if (!m_tciPttActive)
                    clearQueuedTxAudio();

				if (bOK)
				{
					if (bMox && alreadyMox)
                    {
                        m_server?.RefreshTxAudioSourceState();
						m_server?.RefreshStreamRunState();
                        return;
                    }

					if (rx == 0)
					{
						if (consoleThreadSafe.RX2Enabled && consoleThreadSafe.VFOBTX)
							consoleThreadSafe.VFOATX = true;

						if (consoleThreadSafe.MOX != bMox)
							consoleThreadSafe.TCIPTT = bMox;
					}
                    else if (rx == 1 && consoleThreadSafe.RX2Enabled)
                    {
						if (!consoleThreadSafe.VFOBTX)
							consoleThreadSafe.VFOBTX = true;

						if (consoleThreadSafe.MOX != bMox)
							consoleThreadSafe.TCIPTT = bMox;
					}

                    if (!bMox)
                        m_server?.NotifyCwTciPttReleased(this);
				}

				m_server?.RefreshTxAudioSourceState();
				m_server?.RefreshStreamRunState();
			}
			else if (bOK && args.Length == 1)
            {
				sendMOX(rx, consoleThreadSafe.MOX, m_txUsesTCIAudio);
            }
		}

        private bool shouldIgnoreTrxForCurrentCwBreakIn()
        {
            if (consoleThreadSafe == null) return false;

            bool txOnRx2 = consoleThreadSafe.RX2Enabled && consoleThreadSafe.VFOBTX;
            DSPMode mode = txOnRx2 ? consoleThreadSafe.RX2DSPMode : consoleThreadSafe.RX1DSPMode;

            if (mode != DSPMode.CWL && mode != DSPMode.CWU)
                return false;

            BreakIn breakInMode = consoleThreadSafe.CurrentBreakInMode;
            return breakInMode == BreakIn.QSK || breakInMode == BreakIn.Manual;
        }

		private void handleIF(string[] args)
		{
			int rx = 0;
			int chan = 0;
			long lIF = 0;

			bool bOK = int.TryParse(args[0], out rx);
			if (bOK)
				bOK = int.TryParse(args[1], out chan);
			if (args.Length == 3)
			{
				if (bOK)
					bOK = long.TryParse(args[2], out lIF);

				if (bOK)
				{
					double dIF = lIF / 1e6;
					double vfo;

					if (rx == 0)
					{
						vfo = consoleThreadSafe.CentreFrequency + dIF;
						vfo = Math.Round(vfo, 6);
						if (chan == 0)
						{
							if (consoleThreadSafe.VFOAFreq != vfo)
								consoleThreadSafe.VFOAFreq = vfo;
						}
						else if (chan == 1)
						{
							if (consoleThreadSafe.VFOBFreq != vfo)
								consoleThreadSafe.VFOBFreq = vfo;
						}
					}
					else if (rx == 1)
					{
						if (consoleThreadSafe.RX2Enabled)
						{
							vfo = consoleThreadSafe.CentreRX2Frequency + dIF;
							vfo = Math.Round(vfo, 6);
							if (chan == 0)
							{
								if (consoleThreadSafe.VFOBFreq != vfo)
									consoleThreadSafe.VFOBFreq = vfo;
							}
							else if (chan == 1)
							{
								if (consoleThreadSafe.VFOBFreq != vfo)
									consoleThreadSafe.VFOBFreq = vfo;
							}
						}
					}
				}
			}
			else if (args.Length == 2)
			{
				bool bVFOaUseRX2;
				if (m_server != null && consoleThreadSafe != null)
					bVFOaUseRX2 = consoleThreadSafe.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
				else
					bVFOaUseRX2 = false;

				// query
				if (bOK)
				{
					double dIF = 0;
					if (rx == 0)
					{
						if (chan == 0)
						{
							dIF = consoleThreadSafe.VFOAFreq - consoleThreadSafe.CentreFrequency;
						}
						else if (chan == 1)
						{
							dIF = consoleThreadSafe.VFOBFreq - consoleThreadSafe.CentreFrequency;
						}
					}
					else if (rx == 1)
					{
						if (chan == 0)
						{
							if(bVFOaUseRX2)
								dIF = consoleThreadSafe.VFOAFreq - consoleThreadSafe.CentreFrequency;
							else
								dIF = consoleThreadSafe.VFOBFreq - consoleThreadSafe.CentreRX2Frequency;
						}
						else
						{
							dIF = consoleThreadSafe.VFOBFreq - consoleThreadSafe.CentreRX2Frequency;
						}
					}

					dIF *= 1e6; // into HZ
					sendIF(rx, chan, (int)dIF);
				}
			}
		}

		private void handleDDS(string[] args)
		{
			int rx = 0;
			long ddsLong = 0;

			bool bOK = int.TryParse(args[0], out rx);

			if (args.Length == 2)
			{
				if (bOK)
					bOK = long.TryParse(args[1], out ddsLong);
				if (bOK)
				{
					double dds = ddsLong / 1e6;

					if (rx == 0)
					{
						double c = dds - consoleThreadSafe.CentreFrequency;
						c = Math.Round(c, 6);
						consoleThreadSafe.CentreFrequency = dds;
						consoleThreadSafe.VFOAFreq += c;
					}
					else if (rx == 1)
					{
						double c = dds - consoleThreadSafe.CentreRX2Frequency;
						c = Math.Round(c, 6);
						consoleThreadSafe.CentreRX2Frequency = dds;
						consoleThreadSafe.VFOBFreq += c;
					}
				}
			}
			else if (bOK && args.Length == 1)
			{
				// query
				if (bOK)
				{
					double dds = 0;
					if (rx == 0)
					{
						dds = consoleThreadSafe.CentreFrequency;
					}
					else if (rx == 1)
					{
						dds = consoleThreadSafe.CentreRX2Frequency;
					}

					sendDDS(rx, (long)(dds * 1e6));
				}
			}
		}

		private void handleVFOMessage(string[] args)
		{
			int rx = 0;
			int chan = 0;
			long freq = 0;

			bool bVFOaUseRX2;
			if (m_server != null && consoleThreadSafe != null)
				bVFOaUseRX2 = consoleThreadSafe.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
			else
				bVFOaUseRX2 = false;

			bool bOK = int.TryParse(args[0], out rx);
			if (bOK)
				bOK = int.TryParse(args[1], out chan);
			if (args.Length == 3)
			{
				if (bOK)
					bOK = long.TryParse(args[2], out freq);

				if (bOK)
				{
					double vfo = freq / 1e6;
					vfo = Math.Round(vfo, 6);

					if (rx == 0)
					{
						if (chan == 0)
						{
							if (consoleThreadSafe.VFOAFreq != vfo)
								consoleThreadSafe.VFOAFreq = vfo;
						}
						else if (chan == 1)
						{
							if (consoleThreadSafe.VFOBFreq != vfo)
								consoleThreadSafe.VFOBFreq = vfo;
						}
					}
					else if (rx == 1)
					{
						if (consoleThreadSafe.RX2Enabled)
						{
							if (chan == 0)
							{
								if (bVFOaUseRX2)
								{
									if (consoleThreadSafe.VFOAFreq != vfo)
										consoleThreadSafe.VFOAFreq = vfo;
								}
                                else
                                {
									if (consoleThreadSafe.VFOBFreq != vfo)
										consoleThreadSafe.VFOBFreq = vfo;
								}
							}
							else if (chan == 1)
							{
								if (consoleThreadSafe.VFOBFreq != vfo)
									consoleThreadSafe.VFOBFreq = vfo;
							}
						}
					}
				}
			}
			else if(args.Length == 2)
			{
				if (bOK)
				{
					double vfo = 0;
					if (rx == 0)
					{
						if (chan == 0)
						{
							vfo = consoleThreadSafe.VFOAFreq;
						}
						else if (chan == 1)
						{
							vfo = consoleThreadSafe.VFOBFreq;
						}
					}
					else if (rx == 1)
					{
						if (chan == 0)
						{
							if (bVFOaUseRX2)
								vfo = consoleThreadSafe.VFOAFreq;
							else
								vfo = consoleThreadSafe.VFOBFreq;
						}
						else
						{
							vfo = consoleThreadSafe.VFOBFreq;
						}
					}

					TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData()
					{
						cen = false,
						centreMHz = -1,
						rx = bVFOaUseRX2 ? 1 : rx,
						freqMHz = vfo,
						offsetHz = -1,
						chan = chan,
						duplicate_tochan = -1,
						replace_if_duplicated = false,
						sendIF = false
					};

					VFOChange(vfod);
				}
			}
		}

		private void handleModulationMessage(string[] args)
        {
			bool bOK = int.TryParse(args[0], out int rx);

			if (args.Length == 2)
			{
				if (bOK)
				{
					DSPMode mode;

					switch (args[1].ToLower())
					{
						case "lsb":
							mode = DSPMode.LSB;
							break;
						case "usb":
							mode = DSPMode.USB;
							break;
						case "dsb":
							mode = DSPMode.DSB;
							break;
						case "am":
							mode = DSPMode.AM;
							break;
						case "sam":
							mode = DSPMode.SAM;
							break;
						case "nfm":
						case "fm":
							mode = DSPMode.FM;
							break;
						case "cw":
                            //change if needed [2.10.3.6]MW0LGE fixes #365
                            bool bChange = false;
                            if (m_server != null && consoleThreadSafe != null)
							{
								if(m_server.CWbecomesCWUabove10mhz)
								{
									bool bVFOA10orAbove = consoleThreadSafe.VFOAFreq >= 10.0;
                                    bool bVFOB10orAbove = consoleThreadSafe.VFOBFreq >= 10.0;
									
                                    if (rx == 0)
									{
										if(consoleThreadSafe.VFOATX)
                                            bChange = bVFOA10orAbove;
										else
                                            bChange = bVFOB10orAbove;
                                    }
                                    else if (rx == 1)
									{
										if (consoleThreadSafe.VFOBTX)
											bChange = bVFOB10orAbove;
										else
                                            bChange = bVFOA10orAbove;
                                    }
                                }
                            }
                            mode = bChange ? DSPMode.CWU : DSPMode.CWL;
                            break;
                        case "cwl":
							mode = DSPMode.CWL;
							break;
						case "cwu":
							mode = DSPMode.CWU;
							break;
						case "digl":
							mode = DSPMode.DIGL;
							break;
						case "digu":
							mode = DSPMode.DIGU;
							break;
						default:
							mode = DSPMode.FIRST;
							break;

					}
					if (mode != DSPMode.FIRST)
					{
						if (rx == 0)
						{
							if(consoleThreadSafe.RX1DSPMode != mode)
								consoleThreadSafe.RX1DSPMode = mode;
						}
						else if (rx == 1)
						{
							if(consoleThreadSafe.RX2DSPMode != mode)
								consoleThreadSafe.RX2DSPMode = mode;
						}
					}
				}
			}
            else if (bOK && args.Length == 1)
            {
                //query
                if (rx == 0)
                {
					sendMode(rx, consoleThreadSafe.RX1DSPMode);
                }
				else if(rx == 1)
                {
					sendMode(rx, consoleThreadSafe.RX2DSPMode);
				}
            }
        }
		private void handleDeleteSpot(string[] args)
        {
			if(args.Length == 1)
            {
				SpotManager2.DeleteSpot(args[0]);
            }
        }

        // line out to switch vacs
		private void lineOutEnable(int vac_number, bool enable)
		{			
			switch (vac_number)
			{
				case 0:
                    consoleThreadSafe.Invoke(new MethodInvoker(() =>
                    {
						if (consoleThreadSafe.SetupForm.VACEnable != enable)
						{
							consoleThreadSafe.SetupForm.VACEnable = enable;
						}
                    }));
                    break;
				case 1:
                    consoleThreadSafe.Invoke(new MethodInvoker(() =>
                    {
						if (consoleThreadSafe.SetupForm.VAC2Enable != enable)
						{
							consoleThreadSafe.SetupForm.VAC2Enable = enable;
						}
                    }));
                    break;
				default:
					break;
			}
        }

        private void handleLineOutStart(string[] args)
        {
            if (args.Length == 1)
            {
                bool bOK = int.TryParse(args[0], out int vac_number);

                if (bOK && !consoleThreadSafe.IsSetupFormNull)
                {
                    lineOutEnable(vac_number, true);
                }
            }
        }

        private void handleLineOutStop(string[] args)
        {
            if (args.Length == 1)
            {
                bool bOK = int.TryParse(args[0], out int vac_number);

                if (bOK && !consoleThreadSafe.IsSetupFormNull)
                {
                    lineOutEnable(vac_number, false);
                }
            }
        }
        //
        private void handleDrive(string[] args)
		{
			if (args.Length < 1) return;

			bool bOK = int.TryParse(args[0], out int rx);

			if (bOK && args.Length == 2)
            {
				//set
				bOK = int.TryParse(args[1], out int pwr);
                if (bOK)
                {
					consoleThreadSafe.PWR = pwr;
                }
			}
			else if (bOK && args.Length == 1)
            {
				//read
				int pwr;
				if (consoleThreadSafe.SendLimitedPowerLevels)
					pwr = consoleThreadSafe.PWRConstrained;
				else
					pwr = consoleThreadSafe.PWR;

				sendDrivePower(rx, pwr);
            }
		}
		private void handleTuneDrive(string[] args)
        {
			if (args.Length < 1) return;

			bool bOK = int.TryParse(args[0], out int rx);

			if (bOK && args.Length == 2)
			{
				//set
				bOK = int.TryParse(args[1], out int pwr);
				if (bOK)
				{
					switch (consoleThreadSafe.TuneDrivePowerOrigin)
					{
						case DrivePowerSource.DRIVE_SLIDER:
							consoleThreadSafe.PWR = pwr;
							break;
						case DrivePowerSource.TUNE_SLIDER:
					        consoleThreadSafe.TunePWR = pwr;
							break;
					}
				}
			}
			else if (bOK && args.Length == 1)
			{
				//read
				int pwr = 0;
				switch (consoleThreadSafe.TuneDrivePowerOrigin)
				{
					case DrivePowerSource.DRIVE_SLIDER:
						if (consoleThreadSafe.SendLimitedPowerLevels)
							pwr = consoleThreadSafe.PWRConstrained;
						else
							pwr = consoleThreadSafe.PWR;
						break;
					case DrivePowerSource.TUNE_SLIDER:
						if (consoleThreadSafe.SendLimitedPowerLevels)
							pwr = consoleThreadSafe.TunePWRConstrained;
						else
							pwr = consoleThreadSafe.TunePWR;
						break;
					case DrivePowerSource.FIXED:
						pwr = consoleThreadSafe.TunePower;
						break;
				}

				sendTunePower(rx, pwr);
			}
		}
        private void handleMute(string[] args, bool hasArgs = true)
        {
            if (hasArgs && args.Length == 1)
            {
                bool bOK = bool.TryParse(args[0], out bool mute);
                //set
                if (bOK)
                {
					consoleThreadSafe.MUT = mute;
					consoleThreadSafe.MUT2 = mute;
                }
            }
            else if (!hasArgs)
            {
                //read
                sendMute(consoleThreadSafe.MUT || consoleThreadSafe.MUT2);
            }
        }
        private void handleMuteRX(string[] args)
        {
            if (args.Length < 1) return;

            bool bOK = int.TryParse(args[0], out int rx);

            if (bOK && args.Length == 2)
            {
                //set
                bOK = bool.TryParse(args[1], out bool mute);
                if (bOK)
                {
                    if(rx == 0)
                        consoleThreadSafe.MUT = mute;
                    else if(rx == 1)
                        consoleThreadSafe.MUT2 = mute;
                }
            }
            else if (bOK && args.Length == 1)
            {
                //read
                sendMuteRX(rx, rx == 0 ? consoleThreadSafe.MUT : consoleThreadSafe.MUT2);
            }
        }
		private void handleMONEnable(string[] args, bool hasArgs = true)
		{
            if (hasArgs && args.Length == 1)
            {
                //set
                bool bOK = bool.TryParse(args[0], out bool enable);
                if (bOK)
                {
					consoleThreadSafe.MON = enable;
                }
            }
            else if (!hasArgs)
            {
				//read
				sendMONEnable(consoleThreadSafe.MON);
            }
        }
		private double linearToDbVolume(int volume)
		{
            double dbMin = -60f;
            double dbMax = 0;
            double linearMax = 100f;
            double linearMin = 0;

            double dbValue = ((volume - linearMin) / (linearMax - linearMin)) * (dbMax - dbMin) + dbMin;

            return Math.Max(dbMin, Math.Min(dbMax, dbValue));
        }
        private int dbToLinearVolume(double dBLevel)
        {
            double dbMin = -60f;
            double dbMax = 0;
            double linearMax = 100f;
            double linearMin = 0;

            double linearValue = ((dBLevel - dbMin) / (dbMax - dbMin)) * (linearMax - linearMin) + linearMin;
            linearValue = Math.Max(linearMin, Math.Min(linearMax, linearValue));

            return (int)linearValue;
        }
		private void handleMONVolume(string[] args, bool hasArgs = true)
		{
            if (hasArgs && args.Length == 1)
            {
                //set
                bool bOK = double.TryParse(args[0], out double dBLevel);
                if (bOK)
                {
                    consoleThreadSafe.TXAF = dbToLinearVolume(dBLevel);
                }
            }
            else if (!hasArgs)
            {
				//read
                sendMONVolume(linearToDbVolume(consoleThreadSafe.TXAF));
            }
        }
        private void handleVolume(string[] args, bool hasArgs = true)
        {
            if (hasArgs && args.Length == 1)
            {
                if (!double.TryParse(args[0], out double dBLevel)) return;
                consoleThreadSafe.AF = dbToLinearVolume(dBLevel);
            }
            else if (!hasArgs)
            {
                sendVolume(linearToDbVolume(consoleThreadSafe.AF));
            }
        }
        private void handleSpotSimulateClick(string[] args)
        {
			if (m_server == null) return;

            if (args.Length == 2)
            {
				string callsign = args[0];
                bool bOK = long.TryParse(args[1], out long freq);
                if (bOK)
				{
					m_server.SendSpotSimulationClickToAll(callsign, freq);
                }
            }
        }
		private void handleSpot(string[] args, bool is_json, string msg)
        {
			if (args.Length >= 4) // 4 as argument 5 may contain commas
			{
				long freq = 0;
				uint argb = 0;
				DSPMode mode = DSPMode.FIRST;
				bool json_found = false;

                string sAdditional = "";
				if (!is_json)
				{
                    // join 5+ arguments back together
                    for (int i = 4; i < args.Length; i++)
					{
						sAdditional += args[i] + ",";
					}
					if (sAdditional.EndsWith(",")) sAdditional = sAdditional.Substring(0, sAdditional.Length - 1);
				}
				else
				{
					int thetis_tag = msg.ToLower().IndexOf("[json]{");
					if (thetis_tag > -1)
					{
						sAdditional = msg.Substring(thetis_tag + 6); // +8  =  the len of [json]
                        json_found = true;
					}
					else
					{
                        // we should have found a [json] tag, ignore if not
                        return;
					}
                }

				bool bOK = long.TryParse(args[2], out freq);
                if (bOK)
					bOK = uint.TryParse(args[3], out argb);
				if (bOK)
				{
					bOK = Enum.TryParse(args[1].ToUpper(), out mode);
                    if (!bOK)
                    {
						bool isFreqencyNormallyUSB = freq >= 10000000 || (freq >= 5300000 && freq < 5410000);

						string mode_filtered = SpotManager2.FilterForRawMode(args[1].ToLower());

                        switch (mode_filtered)
                        {
							case "lsb":
								mode = DSPMode.LSB;
								break;
							case "usb":
								mode = DSPMode.USB;
								break;
							case "am":
								mode = DSPMode.AM;
								break;
							case "fm":
                            case "nfm":
                                mode = DSPMode.FM;
								break;
							case "dsb":
								mode = DSPMode.DSB;
								break;
							case "drm":
								mode = DSPMode.DRM;
								break;
							case "spec":
								mode = DSPMode.SPEC;
								break;
							case "sam":
								mode = DSPMode.SAM;
								break;
							case "cwl":
								mode = DSPMode.CWL;
								break;
							case "cwu":
								mode = DSPMode.CWU;
                                break;
                            case "digu":
								mode = DSPMode.DIGU;
                                break;
                            case "digl":
								mode = DSPMode.DIGL;
                                break;
                            case "ssb":
								if(isFreqencyNormallyUSB)
									mode = DSPMode.USB;
								else
									mode = DSPMode.LSB;
								break;
							case "cw":
								switch(m_server.CWSpotForce)
								{
									case TCICWSpotForce.CWU:
                                        mode = DSPMode.CWU;
                                        break;
									case TCICWSpotForce.CWL:
                                        mode = DSPMode.CWL;
                                        break;
									case TCICWSpotForce.DEFAULT:
									default:
                                        if (isFreqencyNormallyUSB)
                                            mode = DSPMode.CWU;
                                        else
                                            mode = DSPMode.CWL;
                                        break;
								}
								break;
							case "rtty":
							case "psk":
							case "olivia":
							case "jt65":
							case "jt9":
							case "contesa":
							case "fsk":
							case "mt63":
							case "domi":
							case "packtor":
							case "sstv":
								if (isFreqencyNormallyUSB)
									mode = DSPMode.DIGU;
								else
									mode = DSPMode.DIGL;
								break;
							case "ft8":
								mode = DSPMode.DIGU; // always usb on ft8
								break;
							default:
								// unknown mode, just set to first, so that Display will draw it centred at least
								mode = DSPMode.FIRST;
								break;
                        }
						bOK = true;
                    }
				}
				if (bOK)
				{
					byte[] bytes = Encoding.UTF8.GetBytes(sAdditional);
					byte[] converted = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes);

					string sAdditionalConvertedText = Encoding.Unicode.GetString(converted, 0, converted.Length);

					if (sAdditionalConvertedText.ToLower() == "nil") sAdditionalConvertedText = ""; //[2.10.3.6]MW0LGE rumlog fills arg5 with Nil - spotted buy GW3JVB
																									//downside is that any additional text that is the string 'nil'
																									//will be removed, not that it is too much of an issue

					SpotManager2.JsonSpotData jsonSpotData = null;

                    if (json_found)
					{
						try
						{
                            jsonSpotData = JsonConvert.DeserializeObject<SpotManager2.JsonSpotData>(sAdditionalConvertedText);
                        }
                        catch 
						{
							// json failed in some way, ignore
							return;
						}
					}

					SpotManager2.AddSpot(args[0], mode, freq, Color.FromArgb((int)argb), sAdditionalConvertedText, jsonSpotData);
				}
			}
		}

        private void handleTune(string[] args)
        {
			int rx = 0;
			bool tune = false;

			if (args.Length == 2)
			{
				bool bOK = int.TryParse(args[0], out rx);
				if (bOK)
					bOK = bool.TryParse(args[1], out tune);
                if (bOK)
                {
					if(consoleThreadSafe.TUN != tune)
						consoleThreadSafe.TUN = tune;
                }
			}
            else if(args.Length == 1)
            {
				//query
				sendTune(rx, consoleThreadSafe.TUN);
            }
		}

        private void handleRxFilterBand(string[] args)
        {
            if (m_server == null) return;

			int rx = 0;
			int low = 0;
			int high = 0;

			if (args.Length == 1)
			{
				//read
				bool bOK = int.TryParse(args[0], out rx);

                switch (rx)
                {
                    case 0:
						low = consoleThreadSafe.RX1FilterLow;
						high = consoleThreadSafe.RX1FilterHigh;
                        break;
                    case 1:
						low = consoleThreadSafe.RX2FilterLow;
						high = consoleThreadSafe.RX2FilterHigh;
                        break;
                }

                sendFilterBand(rx, low, high);
			}
            else if (args.Length == 3)
            {
                //set
                bool bOK = int.TryParse(args[0], out rx);
                if (bOK) bOK = int.TryParse(args[1], out low);
                if (bOK) bOK = int.TryParse(args[2], out high);
				if (bOK)
				{
					switch (rx)
					{
						case 0:
                            consoleThreadSafe.UpdateRX1Filters(low, high);
                            break;
						case 1:
                            consoleThreadSafe.UpdateRX2Filters(low, high);
                            break;
					}
				}
            }
        }
        private void handleTXFilterBandEx(string[] args)
        {
            if (m_server == null) return;
            if (args != null && args.Length != 0 && args.Length != 2) return;

            if (args == null || args.Length == 0)
            {
                sendTXFilterBandEx(consoleThreadSafe.TXFilterLow, consoleThreadSafe.TXFilterHigh);
                return;
            }

            if (!int.TryParse(args[0], out int low)) return;
            if (!int.TryParse(args[1], out int high)) return;

            normalizeTXFilterBandForSet(ref low, ref high);

            if (consoleThreadSafe.TXFilterLow != low) consoleThreadSafe.TXFilterLow = low;
            if (consoleThreadSafe.TXFilterHigh != high) consoleThreadSafe.TXFilterHigh = high;
        }
        private void handleRXEnable(string[] args)
        {
			int rx = 0;
			bool enable = false;

			bool bOK = int.TryParse(args[0], out rx);
            if (rx < 0 || rx > 1) return;

            if (args.Length == 2)
			{				
				if (bOK)
					bOK = bool.TryParse(args[1], out enable);
				if (bOK)
				{
					// rx0 is always enabled
					if (rx == 1)
					{
						if (consoleThreadSafe.RX2Enabled != enable)
							consoleThreadSafe.RX2Enabled = enable;
					}
				}
			}
			else if (bOK && args.Length == 1)
			{
				//query
				if (rx == 0)
				{
					sendRXEnable(rx, !consoleThreadSafe.MOX);
				}
                else if(rx == 1)
                {
					sendRXEnable(rx, consoleThreadSafe.RX2Enabled && !consoleThreadSafe.MOX);
                }
			}
		}

		private void handleRxSensorsEnable(string[] args)
		{
			if (args == null || args.Length < 1 || args.Length > 2) return;
			if (!bool.TryParse(args[0], out bool enabled)) return;

			int intervalMs = m_sensorManager.RxIntervalMs;
			if (args.Length == 2 && !int.TryParse(args[1], out intervalMs))
				return;

			setRxSensorsEnabled(enabled, intervalMs, enabled);
		}
        private void handleTxSensorsEnable(string[] args)
		{
			if (args == null || args.Length < 1 || args.Length > 2) return;
			if (!bool.TryParse(args[0], out bool enabled)) return;

			int intervalMs = m_sensorManager.TxIntervalMs;
			if (args.Length == 2 && !int.TryParse(args[1], out intervalMs))
				return;

			setTxSensorsEnabled(enabled, intervalMs, enabled);
		}
        //
        private void sendNREnable(int rx, bool enabled, bool is_extended, int nr)
        {
            string s;
            if(is_extended)
                s = "rx_nr_enable_ex:" + rx.ToString() + "," + enabled.ToString().ToLower() + "," + nr.ToString() + ";";
            else
                s = "rx_nr_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";

            sendTextFrame(s);
        }
        private void sendNBEnable(int rx, bool enabled, bool is_extended, int nb)
        {
            string s;
            if (is_extended)
                s = "rx_nb_enable_ex:" + rx.ToString() + "," + enabled.ToString().ToLower() + "," + nb.ToString() + ";";
            else
                s = "rx_nb_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";

            sendTextFrame(s);
        }
        private void handleNREnable(string[] args, bool is_extended)
        {
            if (args == null || args.Length < 1 || is_extended && args.Length < 3) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            int nr = 1;
            bool enable = false;
            if (args.Length == 1)
            {
                //get
                nr = consoleThreadSafe.GetSelectedNR(rx + 1);
                enable = nr > 0;
                sendNREnable(rx, enable, false, nr);
                sendNREnable(rx, enable, true, nr);
            }
            else
            {
                //set
                if (!bool.TryParse(args[1], out enable)) return;
                if (is_extended && !int.TryParse(args[2], out nr)) return;
                if (nr < 0 || nr > 4) return;

                if (enable)
                {
                    consoleThreadSafe.SelectNR(rx + 1, false, is_extended ? nr : 1);
                }
                else
                {
                    consoleThreadSafe.SelectNR(rx + 1, false, 0);
                }
            }
        }
        private void handleRxNBEnable(string[] args, bool is_extended)
        {
            if (args == null || args.Length < 1 || is_extended && args.Length < 3) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            int nb = 1;
            bool enable = false;
            if (args.Length == 1)
            {
                //get
                nb = consoleThreadSafe.GetSelectedNB(rx + 1);
                enable = nb > 0;
                sendNBEnable(rx, enable, false, nb);
                sendNBEnable(rx, enable, true, nb);
            }
            else
            {
                //set
                if (!bool.TryParse(args[1], out enable)) return;
                if (is_extended && !int.TryParse(args[2], out nb)) return;
                if (nb < 0 || nb > 2) return;

                if (enable)
                {
                    consoleThreadSafe.SetSelectedNB(rx + 1, is_extended ? nb : 1);
                }
                else
                {
                    consoleThreadSafe.SetSelectedNB(rx + 1, 0);
                }
            }
        }
        private void sendAnfEnable(int rx, bool enabled)
        {
            string s = "rx_anf_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";

            sendTextFrame(s);
        }
        private void handleAnfEnable(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            bool enable = false;
            if (args.Length == 1)
            {
                // get
                enable = consoleThreadSafe.GetANF(rx + 1);
                sendAnfEnable(rx, enable);
            }
            else
            {
                //set
                if (!bool.TryParse(args[1], out enable)) return;

                consoleThreadSafe.SetANF(rx + 1, enable);                
            }            
        }
        private double dbToAudioGain(double db)
        {
            if (db <= -60.0)
                return 0.0;

            if (db >= 0.0)
                return 1.0;

            return Math.Pow(10.0, db / 20.0);
        }

        private double audioGainToDb(double gain)
        {
            if (gain <= 0.0)
                return -60.0;

            if (gain >= 1.0)
                return 0.0;

            return 20.0 * Math.Log10(gain);
        }
        private void sendRxVolume(int rx, int chan, double volume)
        {
            string s = "rx_volume:" + rx.ToString() + "," + chan.ToString() + "," + volume.ToString("F2", CultureInfo.InvariantCulture) + ";";

            sendTextFrame(s);
        }
        private void handleRxVolume(string[] args)
        {
            if (args == null || args.Length < 2 || args.Length > 3) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;
            if (!int.TryParse(args[1], out int chan)) return;
            if (chan < 0 || chan > 1) return;

            int vol = 0;

            if (args.Length == 2)
            {
                //get
                switch (rx + 1)
                {
                    case 1:
                        if (chan == 0)
                        {
                            vol = consoleThreadSafe.RX0Gain; // such horrible naming
                        }
                        else if (chan == 1)
                        {
                            vol = consoleThreadSafe.RX1Gain;
                        }
                        else
                            return;
                        break;
                    case 2:
                        vol = consoleThreadSafe.RX2Gain; // no sub
                        break;
                }
                double perc = vol / 100f;
                double db = audioGainToDb(perc);
                sendRxVolume(rx, chan, db);
            }
            else
            {
                //set
                if (!double.TryParse(args[2], out double db)) return;

                double ag = dbToAudioGain(db) * 100f;              

                switch (rx + 1)
                {
                    case 1:
                        if (chan == 0)
                        {
                            consoleThreadSafe.RX0Gain = (int)ag;
                        }
                        else if (chan == 1)
                        {
                            consoleThreadSafe.RX1Gain = (int)ag;
                        }
                        else
                            return;
                        break;
                    case 2:
                        consoleThreadSafe.RX2Gain = (int)ag;
                        break;
                }
            }
        }
        private void handleRxBalance(string[] args)
        {
            if (args == null || args.Length < 2 || args.Length > 3) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;
            if (!int.TryParse(args[1], out int chan)) return;
            if (chan < 0 || chan > 1) return;

            bool subrx = chan == 1;

            if (args.Length == 2)
            {
                int pan = consoleThreadSafe.GetBal(rx + 1, subrx);
                double balance = 40.0 - (pan * 0.8);
                sendRxBalance(rx, chan, balance);
            }
            else
            {
                if (!double.TryParse(args[2], out double balance)) return;
                balance = Math.Max(-40.0, Math.Min(40.0, balance));

                int pan = (int)Math.Round((40.0 - balance) / 0.8, MidpointRounding.AwayFromZero);
                pan = Math.Max(0, Math.Min(100, pan));

                consoleThreadSafe.SetBal(rx + 1, pan, subrx);
            }
        }
        private void handleRxStepAttEnabledEx(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                bool enabled = rx == 0 ? consoleThreadSafe.RX1StepAttEnabled : consoleThreadSafe.RX2StepAttEnabled;
                sendRxStepAttEnabledEx(rx, enabled);
            }
            else
            {
                if (!bool.TryParse(args[1], out bool enabled)) return;
                if (consoleThreadSafe == null || consoleThreadSafe.IsSetupFormNull) return;

                if (rx == 0)
                    consoleThreadSafe.SetupForm.RX1EnableAtt = enabled;
                else
                    consoleThreadSafe.SetupForm.RX2EnableAtt = enabled;
            }
        }
        private void handleRxStepAttEx(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                int attenuation = rx == 0 ? consoleThreadSafe.RX1AttenuatorData : consoleThreadSafe.RX2AttenuatorData;
                sendRxStepAttEx(rx, attenuation);
            }
            else
            {
                if (!int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int attenuation)) return;
                if (attenuation < 0) return; // attenuation is always a +ve value, -ve attenuation would be gain !

                if (rx == 0)
                    consoleThreadSafe.RX1AttenuatorData = attenuation;
                else
                    consoleThreadSafe.RX2AttenuatorData = attenuation;
            }
        }
        private void handleRxPreampAttEx(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                PreampMode mode = rx == 0 ? consoleThreadSafe.RX1PreampMode : consoleThreadSafe.RX2PreampMode;
                sendRxPreampAttEx(rx, -preampModeToAttenuation(mode));
            }
            else
            {
                if (!int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int attenuation)) return;
                if (attenuation > 0) return;
                consoleThreadSafe.SetATT(rx + 1, Math.Abs(attenuation), Thetis.Console.SetAttMode.PREAMP_MODE);
            }
        }
        private void handleVfoSyncEx(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                sendVFOSyncEx(consoleThreadSafe.VFOSync);
                return;
            }

            if (args.Length != 1) return;
            if (!bool.TryParse(args[0], out bool enabled)) return;

            consoleThreadSafe.VFOSync = enabled;
        }
        private void handleVfoSwapEx()
        {
            consoleThreadSafe.VFOSwap();
        }
        private void handleFMDeviationEx(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                sendFMDeviationEx(rx, consoleThreadSafe.FMDeviation_Hz);
            }
            else
            {
                if (!int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int deviationHz)) return;
                if (deviationHz != 2500 && deviationHz != 5000) return;

                consoleThreadSafe.FMDeviation_Hz = deviationHz;
            }
        }
        private void handleAgcAutoEx(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                sendAgcAutoEx(rx, consoleThreadSafe.GetAGCAuto(rx + 1));
            }
            else
            {
                if (!bool.TryParse(args[1], out bool enabled)) return;
                consoleThreadSafe.SetAGCAuto(rx + 1, enabled);
            }
        }
        private void handleAgcMode(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                sendAgcMode(rx, consoleThreadSafe.GetAGCMode(rx + 1));
            }
            else
            {
                consoleThreadSafe.SetAGCMode(rx + 1, tciModeToAgcMode(args[1]));
            }
        }
        private void handleAgcGain(string[] args)
        {
            if (args == null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            if (args.Length == 1)
            {
                sendAgcGain(rx, consoleThreadSafe.GetAgcT(rx + 1));
            }
            else
            {
                if (!int.TryParse(args[1], out int gain)) return;
                gain = Math.Max(-20, Math.Min(120, gain));
                consoleThreadSafe.SetAgcT(rx + 1, gain);
            }
        }
        private void sendCTUN(int rx, bool enabled)
        {
            string s = "rx_ctun_ex:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";

            sendTextFrame(s);
        }
        private void handleCTUN(string[] args)
        {
            if(args== null || args.Length < 1 || args.Length > 2) return;
            if (!int.TryParse(args[0], out int rx)) return;

            bool enable = false;
            if(args.Length == 1)
            {
                //get
                enable = consoleThreadSafe.GetCTUN(rx + 1);
                sendCTUN(rx, enable);
            }
            else
            {
                //set
                if (!bool.TryParse(args[1], out enable)) return;
                consoleThreadSafe.SetCTUN(rx + 1, enable);
            }
        }
        private void sendTXProfile(string prof)
        {
            string s = "tx_profile_ex:" + prof + ";";

            sendTextFrame(s);
        }
        private void sendTXProfiles()
        {
            // each profile surrounded with {} and , separated
            if (consoleThreadSafe == null || consoleThreadSafe.IsSetupFormNull) return;

            string[] profiles = consoleThreadSafe.SetupForm.GetTXProfileStrings();
            string formatted_profiles = string.Join(",", profiles);
            string s = "tx_profiles_ex:" + formatted_profiles + ";";

            sendTextFrame(s);
        }
        private void handleTXProfile(string[] args)
        {
            if (args == null || args.Length > 1) return;

            if (args.Length == 0)
            {
                //get current txprofille
                string prof = consoleThreadSafe.TXProfile;
                sendTXProfile(prof);
            }
            else
            {
                //set
                consoleThreadSafe.SafeTXProfileSet(args[0]);
            }
        }
        private void handleTXProfiles()
        {
            sendTXProfiles();
        }
        private void handleShutdown()
        {
            if (consoleThreadSafe.InvokeRequired)
            {
                consoleThreadSafe.BeginInvoke(new MethodInvoker(() =>
                {
                    consoleThreadSafe.Close();
                }));
            }
            else
            {
                consoleThreadSafe.Close();
            }
        }
        private void sendCalibration(int rx, float meter, float display, float xvtr, float six_meter, float tx_display_offset)
        {
            if (rx < 0 || rx > 1) return;

            string s = $"calibration_ex:{rx},{meter.ToString("F6", CultureInfo.InvariantCulture)}," +
                $"{display.ToString("F6", CultureInfo.InvariantCulture)},{xvtr.ToString("F6", CultureInfo.InvariantCulture)}," +
                $"{six_meter.ToString("F6", CultureInfo.InvariantCulture)},{tx_display_offset.ToString("F6", CultureInfo.InvariantCulture)};";

            sendTextFrame(s);
        }
        private void handleCalibration(string[] args)
        {
            if (args.Length != 1) return;
            if (!int.TryParse(args[0], out int rx)) return;

            CalibrationChanged(rx);
        }

        private void handleRunCatCommand(string msg)
        {
            Debug.Print(msg);
            int col_pos = msg.IndexOf(':');
            if (col_pos == -1 || col_pos == msg.Length - 1) return;

            string cat_com = msg.Substring(col_pos + 1);
            if (string.IsNullOrWhiteSpace(cat_com)) return;

            // single cat command
            try
            {
                // ; will be stripped by parser calling us, lets add it
                cat_com += ";";

                string ret = consoleThreadSafe.ThreadSafeCatParse(cat_com);
                if (!string.IsNullOrEmpty(ret))
                {
                    ret.Replace(";", "");
                    string response = $"run_cat_ex:{cat_com},{ret}";
                    if(response.Right(1) != ";") response += ";";
                    sendTextFrame(response);
                }
            }
            catch
            {
                sendTextFrame($"run_cat_ex:{cat_com},?;");
            }
        }
        //

        private List<string> splitTextCommands(string msg)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(msg))
                return result;

            int start = 0;
            bool current_is_spot = false;
            bool spot_json_started = false;
            bool in_quotes = false;
            bool escape_next = false;
            int brace_depth = 0;

            for (int i = 0; i < msg.Length; i++)
            {
                char ch = msg[i];

                if (!current_is_spot && ch == ':')
                {
                    string cmd_name = msg.Substring(start, i - start).Trim();
                    current_is_spot = cmd_name.Equals("spot", StringComparison.OrdinalIgnoreCase);
                }

                if (current_is_spot &&
                    !spot_json_started &&
                    ch == '[' &&
                    i + 6 < msg.Length &&
                    string.Compare(msg, i, "[json]{", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    spot_json_started = true;
                    brace_depth = 1;
                    in_quotes = false;
                    escape_next = false;
                    i += 6;
                    continue;
                }

                if (spot_json_started)
                {
                    if (escape_next)
                    {
                        escape_next = false;
                        continue;
                    }

                    if (ch == '\\')
                    {
                        escape_next = true;
                        continue;
                    }

                    if (ch == '"')
                    {
                        in_quotes = !in_quotes;
                        continue;
                    }

                    if (!in_quotes)
                    {
                        if (ch == '{')
                        {
                            brace_depth++;
                        }
                        else if (ch == '}')
                        {
                            brace_depth--;

                            if (brace_depth <= 0)
                            {
                                spot_json_started = false;
                                brace_depth = 0;
                            }
                        }
                    }

                    continue;
                }

                if (ch == ';')
                {
                    string part = msg.Substring(start, i - start).Trim();

                    if (part.Length > 0)
                        result.Add(part);

                    start = i + 1;
                    current_is_spot = false;
                    spot_json_started = false;
                    in_quotes = false;
                    escape_next = false;
                    brace_depth = 0;
                }
            }

            if (start < msg.Length)
            {
                string tail = msg.Substring(start).Trim();

                if (tail.Length > 0)
                    result.Add(tail);
            }

            return result;
        }

        private void parseTextFrame(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            List<string> commands = splitTextCommands(msg);

            if (commands.Count > 1)
            {
                for (int i = 0; i < commands.Count; i++)
                    parseTextFrame(commands[i]);

                return;
            }

            if (commands.Count == 1)
                msg = commands[0];

            //Debug.Print("TCI Msg : " + msg);

            if (m_server != null && m_server.LogForm != null) m_server.LogForm.Log(true, msg);

            if (msg.EndsWith(";")) msg = msg.Substring(0, msg.Length - 1).Trim();

            string[] parts = msg.Split(new char[] { ':' }, 2);

            bool json_spot = parts.Length >= 2 && parts[0].ToLower().Trim() == "spot" && msg.ToLower().IndexOf("[json]{") >= 0; // a spot with some json info

            if (parts.Length == 2 || json_spot)
            {
                string cmd = parts[0].ToLower().Trim();
                string[] args = parts[1].Split(',');

                switch (cmd)
                {
                    case "modulation":
                        handleModulationMessage(args);
                        break;
                    case "vfo":
                        handleVFOMessage(args);
                        break;
                    case "trx":
                        handleTrxMessage(args);
                        break;
                    case "split_enable":
                        handleSplitEnableMessage(args);
                        break;
                    case "rit_enable":
                        handleRITEnableMessage(args);
                        break;
                    case "xit_enable":
                        handleXITEnableMessage(args);
                        break;
                    case "rit_offset":
                        handleRITOffsetMessage(args);
                        break;
                    case "xit_offset":
                        handleXITOffsetMessage(args);
                        break;
                    case "rx_bin_enable":
                        handleRxBinEnable(args);
                        break;
                    case "rx_apf_enable":
                        handleRxApfEnable(args);
                        break;
                    case "rx_nf_enable":
                        handleRxNfEnable(args);
                        break;
                    case "lock":
                        handleLock(args);
                        break;
                    case "vfo_lock":
                        handleVFOLock(args);
                        break;
                    case "sql_enable":
                        handleSqlEnable(args);
                        break;
                    case "sql_level":
                        handleSqlLevel(args);
                        break;
                    case "digl_offset":
                        handleDiglOffset(args);
                        break;
                    case "digu_offset":
                        handleDiguOffset(args);
                        break;
                    case "cw_macros_speed":
                        handleCwMacrosSpeed(args);
                        break;
                    case "cw_macros_delay":
                        handleCwMacrosDelay(args);
                        break;
                    case "cw_keyer_speed":
                        handleCwKeyerSpeed(args);
                        break;
                    case "cw_macros_speed_up":
                        handleCwMacrosSpeedUp(args);
                        break;
                    case "cw_macros_speed_down":
                        handleCwMacrosSpeedDown(args);
                        break;
                    case "cw_macros":
                        handleCwMacros(args);
                        break;
                    case "cw_terminal":
                        handleCwTerminal(args);
                        break;
                    case "cw_msg":
                        handleCwMsg(args);
                        break;
                    case "keyer":
                        handleKeyer(args);
                        break;
                    case "tune":
                        handleTune(args);
                        break;
                    case "rx_enable":
                        handleRXEnable(args);
                        break;
                    case "dds":
                        handleDDS(args);
                        break;
                    case "iq_samplerate":
                        handleIQSampleRate(args);
                        break;
                    case "audio_samplerate":
                        handleAudioSampleRate(args);
                        break;
                    case "iq_start":
                        handleIQStart(args, true);
                        break;
                    case "iq_stop":
                        handleIQStart(args, false);
                        break;
                    case "audio_start":
                        handleAudioStart(args, true);
                        break;
                    case "audio_stop":
                        handleAudioStart(args, false);
                        break;
                    case "audio_stream_sample_type":
                        handleAudioStreamSampleType(args);
                        break;
                    case "audio_stream_channels":
                        handleAudioStreamChannels(args);
                        break;
                    case "audio_stream_samples":
                        handleAudioStreamSamples(args);
                        break;
                    case "tx_stream_audio_buffering":
                        handleTxStreamAudioBuffering(args);
                        break;
                    case "if":
                        handleIF(args);
                        break;
                    case "spot":
                        handleSpot(args, json_spot, msg);
                        break;
                    case "spot_delete":
                        handleDeleteSpot(args);
                        break;
                    case "drive":
                        handleDrive(args);
                        break;
                    case "tune_drive":
                        handleTuneDrive(args);
                        break;
                    case "mute":
                        handleMute(args);
                        break;
                    case "volume":
                        handleVolume(args);
                        break;
                    case "rx_mute":
                        handleMuteRX(args);
                        break;
                    case "mon_volume":
                        handleMONVolume(args);
                        break;
                    case "mon_enable":
                        handleMONEnable(args);
                        break;
                    case "line_out_start":
                        handleLineOutStart(args);
                        break;
                    case "line_out_stop":
                        handleLineOutStop(args);
                        break;
                    case "spot_simulate_click":
                        handleSpotSimulateClick(args);
                        break;
                    case "rx_filter_band":
                        handleRxFilterBand(args);
                        break;
                    case "tx_filter_band_ex":
                        handleTXFilterBandEx(args);
                        break;
                    case "rx_channel_enable":
                        handleRxChannelEnable(args);
                        break;
                    case "rx_sensors_enable":
                        handleRxSensorsEnable(args);
                        break;
                    case "tx_sensors_enable":
                        handleTxSensorsEnable(args);
                        break;
                    case "rx_nr_enable":
                        handleNREnable(args, false);
                        break;
                    case "rx_nr_enable_ex":
                        handleNREnable(args, true);
                        break;
                    case "rx_nb_enable":
                        handleRxNBEnable(args, false);
                        break;
                    case "rx_nb_enable_ex":
                        handleRxNBEnable(args, true);
                        break;
                    case "rx_anf_enable":
                        handleAnfEnable(args);
                        break;
                    case "rx_volume":
                        handleRxVolume(args);
                        break;
                    case "rx_balance":
                        handleRxBalance(args);
                        break;
                    case "rx_step_att_enabled_ex":
                        handleRxStepAttEnabledEx(args);
                        break;
                    case "rx_step_att_ex":
                        handleRxStepAttEx(args);
                        break;
                    case "rx_preamp_att_ex":
                        handleRxPreampAttEx(args);
                        break;
                    case "vfo_sync_ex":
                        handleVfoSyncEx(args);
                        break;
                    case "fm_deviation_ex":
                        handleFMDeviationEx(args);
                        break;
                    case "agc_auto_ex":
                        handleAgcAutoEx(args);
                        break;
                    case "agc_mode":
                        handleAgcMode(args);
                        break;
                    case "agc_gain":
                        handleAgcGain(args);
                        break;
                    case "rx_ctun_ex":
                        handleCTUN(args); // bespoke thetis cmd for ctun
                        break;
                    case "tx_profile_ex":
                        handleTXProfile(args); // bespoke thetis cmd to select tx profile
                        break;
                    case "calibration_ex":
                        handleCalibration(args); // bespoke thetis cmd to get calibration data
                        break;
                    case "run_cat_ex":
                        // this is special, we send whole of msg and handle it there
                        handleRunCatCommand(msg);
                        break;

                }
            }
            else if (parts.Length == 1)
            {
                string cmd = parts[0].ToLower().Trim();
                // just command
                switch (cmd)
                {
                    case "start":
                        handleStart();
                        break;
                    case "stop":
                        handleStop();
                        break;
                    case "tx_filter_band_ex":
                        handleTXFilterBandEx(null);
                        break;
                    case "set_in_focus":
                        handleSetInFocus();
                        break;
                    case "mute":
                        handleMute(null, false);
                        break;
                    case "vfo_sync_ex":
                        handleVfoSyncEx(null);
                        break;
                    case "vfo_swap_ex":
                        handleVfoSwapEx();
                        break;
                    case "volume":
                        handleVolume(null, false);
                        break;
                    case "mon_enable":
                        handleMONEnable(null, false);
                        break;
                    case "mon_volume":
                        handleMONVolume(null, false);
                        break;
                    case "digl_offset":
                        handleDiglOffset(null, false);
                        break;
                    case "digu_offset":
                        handleDiguOffset(null, false);
                        break;
                    case "cw_macros_speed":
                        handleCwMacrosSpeed(null, false);
                        break;
                    case "cw_macros_delay":
                        handleCwMacrosDelay(null, false);
                        break;
                    case "cw_keyer_speed":
                        handleCwKeyerSpeed(null, false);
                        break;
                    case "cw_macros_stop":
                        handleCwMacrosStop();
                        break;
                    case "iq_samplerate":
                        sendIQSampleRate(getPublishedIQSampleRate());
                        break;
                    case "audio_samplerate":
                        sendAudioSampleRate(m_audioSampleRate);
                        break;
                    case "tx_stream_audio_buffering":
                        sendTxStreamAudioBuffering(m_txStreamAudioBufferingMs);
                        break;
                    case "spot_clear":
                        handleSpotClear();
                        break;
                    case "tx_profiles_ex":
                        handleTXProfiles(); // bespoke thetis cmd to send out tx profiles list
                        break;
                    case "shutdown_ex":
                        handleShutdown(); // bespoke thetis cmd  to do a full shutdown
                        break;
                    case "tx_profile_ex":
                        string[] tmpArgs = new string[0];
                        handleTXProfile(tmpArgs); // bespoke thetis cmd to select tx profile
                        break;
                }
            }
        }

        private static int getDefaultAudioStreamSamples(int sampleRate)
        {
            switch (sampleRate)
            {
                case 8000:
                    return 256;
                case 12000:
                    return 512;
                case 24000:
                    return 1024;
                case 48000:
                default:
                    return 2048;
            }
        }

        private static int getBytesPerSample(TCISampleType sampleType)
        {
            switch (sampleType)
            {
                case TCISampleType.INT16:
                    return 2;
                case TCISampleType.INT24:
                    return 3;
                case TCISampleType.INT32:
                case TCISampleType.FLOAT32:
                default:
                    return 4;
            }
        }

        private static void writeUInt32(byte[] buffer, int offset, uint value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private byte[] buildStreamPayload(int receiver, int sampleRate, TCISampleType sampleType, int length, TCIStreamType streamType, int channels, byte[] samplePayload)
        {
            int samplePayloadLength = samplePayload != null ? samplePayload.Length : 0;
            byte[] payload = new byte[64 + samplePayloadLength];
            int offset = 0;

            writeUInt32(payload, offset, (uint)receiver); offset += 4;
            writeUInt32(payload, offset, (uint)sampleRate); offset += 4;
            writeUInt32(payload, offset, (uint)sampleType); offset += 4;
            writeUInt32(payload, offset, 0); offset += 4;
            writeUInt32(payload, offset, 0); offset += 4;
            writeUInt32(payload, offset, (uint)length); offset += 4;
            writeUInt32(payload, offset, (uint)streamType); offset += 4;
            writeUInt32(payload, offset, (uint)channels); offset += 4;

            for (int i = 0; i < 8; i++, offset += 4)
                writeUInt32(payload, offset, 0);

            if (samplePayloadLength > 0)
                Buffer.BlockCopy(samplePayload, 0, payload, 64, samplePayloadLength);

            return payload;
        }

        private byte[] encodeSamples(float[] samples, TCISampleType sampleType)
        {
            if (samples == null || samples.Length == 0)
                return Array.Empty<byte>();

            int bytesPerSample = getBytesPerSample(sampleType);
            byte[] data = new byte[samples.Length * bytesPerSample];

            if (sampleType == TCISampleType.FLOAT32)
            {
                Buffer.BlockCopy(samples, 0, data, 0, data.Length);
                return data;
            }

            int offset = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                float clippedSample = Math.Max(-1.0f, Math.Min(1.0f, samples[i]));
                switch (sampleType)
                {
                    case TCISampleType.INT16:
                        short s16 = (short)Math.Round(clippedSample * short.MaxValue);
                        data[offset++] = (byte)(s16 & 0xFF);
                        data[offset++] = (byte)((s16 >> 8) & 0xFF);
                        break;
                    case TCISampleType.INT24:
                        int s24 = (int)Math.Round(clippedSample * 8388607.0f);
                        data[offset++] = (byte)(s24 & 0xFF);
                        data[offset++] = (byte)((s24 >> 8) & 0xFF);
                        data[offset++] = (byte)((s24 >> 16) & 0xFF);
                        break;
                    case TCISampleType.INT32:
                        int s32 = (int)Math.Round(clippedSample * int.MaxValue);
                        data[offset++] = (byte)(s32 & 0xFF);
                        data[offset++] = (byte)((s32 >> 8) & 0xFF);
                        data[offset++] = (byte)((s32 >> 16) & 0xFF);
                        data[offset++] = (byte)((s32 >> 24) & 0xFF);
                        break;
                }
            }
            return data;
        }

        private static float[] decodeSamples(byte[] payload, int dataOffset, int length, TCISampleType sampleType)
        {
            float[] samples = new float[length];
            int offset = dataOffset;
            for (int i = 0; i < length; i++)
            {
                switch (sampleType)
                {
                    case TCISampleType.INT16:
                        samples[i] = BitConverter.ToInt16(payload, offset) / 32768.0f;
                        offset += 2;
                        break;
                    case TCISampleType.INT24:
                        int s24 = payload[offset] | (payload[offset + 1] << 8) | (payload[offset + 2] << 16);
                        if ((s24 & 0x800000) != 0) s24 |= unchecked((int)0xFF000000);
                        samples[i] = s24 / 8388608.0f;
                        offset += 3;
                        break;
                    case TCISampleType.INT32:
                        samples[i] = BitConverter.ToInt32(payload, offset) / 2147483648.0f;
                        offset += 4;
                        break;
                    case TCISampleType.FLOAT32:
                    default:
                        samples[i] = BitConverter.ToSingle(payload, offset);
                        offset += 4;
                        break;
                }
            }
            return samples;
        }

        private static double[] convertStreamSamplesToComplex(float[] samples, int channels)
        {
            if (channels < 1) channels = 1;
            int complexSamples = channels <= 1 ? samples.Length : samples.Length / channels;
            double[] complex = new double[complexSamples * 2];
            if (channels == 1)
            {
                for (int i = 0; i < complexSamples; i++)
                {
                    double value = samples[i];
                    complex[2 * i] = value;
                    complex[2 * i + 1] = value;
                }
            }
            else
            {
                for (int i = 0, j = 0; i < complexSamples; i++, j += channels)
                {
                    complex[2 * i] = samples[j];
                    complex[2 * i + 1] = samples[j + 1];
                }
            }
            return complex;
        }

        private void sendIQSampleRate(int sampleRate)
        {
			if (sampleRate < 48000) sampleRate = 48000;
			if (sampleRate > 384000) sampleRate = 384000; // iq can only go up to that

			sendTextFrame("iq_samplerate:" + sampleRate.ToString() + ";");
        }

        private void sendAudioSampleRate(int sampleRate)
        {
            sendTextFrame("audio_samplerate:" + sampleRate.ToString() + ";");
        }

        private void sendAudioStreamSampleType(TCISampleType sampleType)
        {
            sendTextFrame("audio_stream_sample_type:" + sampleType.ToString().ToLower() + ";");
        }

        private void sendAudioStreamChannels(int channels)
        {
            sendTextFrame("audio_stream_channels:" + channels.ToString() + ";");
        }

        private void sendAudioStreamSamples(int samples)
        {
            sendTextFrame("audio_stream_samples:" + samples.ToString() + ";");
        }

        private void sendTxStreamAudioBuffering(int milliseconds)
        {
            sendTextFrame("tx_stream_audio_buffering:" + milliseconds.ToString() + ";");
        }

        private bool wantsIQStream(int receiver)
        {
            lock (m_objStreamLock)
            {
				if (m_server != null && m_server.AlwaysStreamIQ) return true;
                return m_iqStreamEnabled.Contains(receiver);
            }
        }

        private bool wantsAudioStream(int receiver)
        {
            lock (m_objStreamLock)
            {
                return m_audioStreamEnabled.Contains(receiver);
            }
        }

        internal bool IsReadyForStreaming()
        {
            return m_bWebSocket && !m_disconnected;
        }

        internal bool WantsAnyRxStream()
        {
            lock (m_objStreamLock)
            {
                return m_iqStreamEnabled.Count > 0 || m_audioStreamEnabled.Count > 0;
            }
        }

        internal void PublishIQSamples(int receiver, int sampleRate, float[] iqSamples, int complexSamples = -1)
        {
            if (iqSamples == null) return;
            if (!wantsIQStream(receiver)) return;
            if (complexSamples < 0) complexSamples = iqSamples.Length / 2;

            byte[] encoded = encodeSamples(iqSamples, TCISampleType.FLOAT32);
            sendBinaryFrame(buildStreamPayload(receiver, sampleRate, TCISampleType.FLOAT32, complexSamples * 2, TCIStreamType.IQ_STREAM, 2, encoded));
        }

        internal void PublishRxAudioSamples(int receiver, int sampleRate, float[] left, float[] right, int samples = -1)
        {
            if (!wantsAudioStream(receiver) || left == null) return;
            if (samples < 0) samples = left.Length;
            if (samples <= 0) return;
            if (samples > left.Length) samples = left.Length;

            TCISampleType sampleType;
            int channels;
            int packetSamples;
            int targetSampleRate;
            lock (m_objStreamLock)
            {
                sampleType = m_audioSampleType;
                channels = m_audioStreamChannels;
                packetSamples = m_audioStreamSamples;
                targetSampleRate = m_audioSampleRate;
            }

            lock (m_objRxAudioLock)
            {
                if (sampleRate != targetSampleRate)
                {
                    samples = resampleRxAudioSamples(receiver, sampleRate, targetSampleRate, left, right, samples, out left, out right, out bool resampled);
                    if (resampled)
                    sampleRate = targetSampleRate;
                    if (samples <= 0)
                        return;
                }

                if (!m_rxAudioLeftPending.TryGetValue(receiver, out TCIPendingFloatBuffer leftPending))
                {
                    leftPending = new TCIPendingFloatBuffer(samples * 2);
                    m_rxAudioLeftPending[receiver] = leftPending;
                }

                if (!m_rxAudioRightPending.TryGetValue(receiver, out TCIPendingFloatBuffer rightPending))
                {
                    rightPending = new TCIPendingFloatBuffer(samples * 2);
                    m_rxAudioRightPending[receiver] = rightPending;
                }

                leftPending.Enqueue(left, 0, samples);

                int rightSamples = right != null ? Math.Min(samples, right.Length) : 0;
                if (rightSamples > 0)
                    rightPending.Enqueue(right, 0, rightSamples);
                if (rightSamples < samples)
                    rightPending.Enqueue(left, rightSamples, samples - rightSamples);

                while (true)
                {
                    if (packetSamples <= 0 || leftPending.Count < packetSamples || rightPending.Count < packetSamples)
                        break;

					float[] interleaved = channels <= 1 ? new float[packetSamples] : new float[packetSamples * 2];
					if (channels <= 1)
					{
						leftPending.CopyTo(interleaved, 0, packetSamples);
					}
					else
					{
						for (int i = 0; i < packetSamples; i++)
						{
							interleaved[2 * i] = leftPending.Peek(i);
							interleaved[2 * i + 1] = rightPending.Peek(i);
						}
					}

					leftPending.Advance(packetSamples);
					rightPending.Advance(packetSamples);

					byte[] encoded = encodeSamples(interleaved, sampleType);
					sendBinaryFrame(buildStreamPayload(receiver, sampleRate, sampleType, interleaved.Length, TCIStreamType.RX_AUDIO_STREAM, channels, encoded));
				}
            }
        }

        internal void SendTxChrono(int receiver)
        {
            int sampleRate;
            int samples;
            int channels;
            TCISampleType sampleType;
            bool useModernLengthSemantics;
            lock (m_objStreamLock)
            {
                sampleRate = m_audioSampleRate;
                samples = m_audioStreamSamples;
                channels = m_audioStreamChannels;
                sampleType = m_audioSampleType;
                useModernLengthSemantics = m_seenModernTxAudioNegotiation;
            }

			int requestLength = useModernLengthSemantics ? samples * Math.Max(1, channels) : samples;
            sendBinaryFrame(buildStreamPayload(receiver, sampleRate, sampleType, requestLength, TCIStreamType.TX_CHRONO, channels, Array.Empty<byte>()));
        }

        internal bool UsesTCITxAudio()
        {
            lock (m_objStreamLock)
            {
                return m_txUsesTCIAudio;
            }
        }

        internal bool UsesActiveTCITxAudio()
        {
            lock (m_objStreamLock)
            {
                return m_txUsesTCIAudio && m_tciPttActive;
            }
        }
        internal bool TryGetTxAudioRequestSettings(out int sampleRate, out int samples, out int bufferingMs)
        {
            lock (m_objStreamLock)
            {
                sampleRate = m_audioSampleRate;
                samples = m_audioStreamSamples;
                bufferingMs = m_txStreamAudioBufferingMs;
                return m_tciPttActive;
            }
        }
        internal void SyncTciPttToMox(bool expectedMox)
        {
            bool releaseOwner = false;
            lock (m_objStreamLock)
            {
                if (!expectedMox)
                {
                    releaseOwner = m_tciPttActive;
                    m_tciPttActive = false;
                }
            }

            if (releaseOwner)
            {
                clearQueuedTxAudio();
                m_server?.ReleaseActiveTxAudioListener(this);
            }
        }
        private void clearQueuedTxAudio()
        {
            lock (m_objTxQueueLock)
            {
                m_txAudioQueue.Clear();
                m_txQueuedComplexSamples = 0;
            }
        }
        internal bool TryDequeueTxAudio(out TCIQueuedTxAudio queuedAudio)
        {
            lock (m_objTxQueueLock)
            {
                if (m_txAudioQueue.Count > 0)
                {
                    queuedAudio = m_txAudioQueue.Dequeue();
                    if (queuedAudio != null)
                        m_txQueuedComplexSamples = Math.Max(0, m_txQueuedComplexSamples - Math.Max(0, queuedAudio.ComplexSamples));
                    return true;
                }
            }

            queuedAudio = null;
            return false;
        }
        private void handleBinaryFrame(byte[] payload)
        {
            if (payload == null || payload.Length < 64)
                return;

            int receiver = BitConverter.ToInt32(payload, 0);
            int sampleRate = BitConverter.ToInt32(payload, 4);
            TCISampleType sampleType = (TCISampleType)BitConverter.ToUInt32(payload, 8);
            int length = BitConverter.ToInt32(payload, 20);
            TCIStreamType streamType = (TCIStreamType)BitConverter.ToUInt32(payload, 24);
            int headerChannels = BitConverter.ToInt32(payload, 28);

            if (streamType != TCIStreamType.TX_AUDIO_STREAM || length <= 0)
                return;

            int bytesPerSample = getBytesPerSample(sampleType);
            int dataOffset = 64;
            int actualDataBytes = payload.Length - dataOffset;
            if (actualDataBytes < bytesPerSample)
                return;

            int actualValueCount = actualDataBytes / bytesPerSample;

            int channels;
            int decodedValueCount;

            bool modernHeader = (headerChannels == 1 || headerChannels == 2);

            if (modernHeader)
            {
                channels = headerChannels;
                decodedValueCount = Math.Min(length, actualValueCount);

                if (channels > 1)
                    decodedValueCount -= decodedValueCount % channels;
            }
            else
            {
                // legacy/JTDX
                // no real channels field
                // if payload is twice length, first 'length' floats are valid stereo-interleaved audio
                if (actualValueCount >= length * 2)
                    channels = 2;
                else
                    channels = 1;

                decodedValueCount = Math.Min(length, actualValueCount);

                if (channels > 1)
                    decodedValueCount -= decodedValueCount % channels;
            }

            if (decodedValueCount <= 0)
                return;

            float[] decoded = decodeSamples(payload, dataOffset, decodedValueCount, sampleType);
            for (int i = 0; i < decoded.Length; i++)
            {
                float sample = decoded[i];
                if (float.IsNaN(sample) || float.IsInfinity(sample))
                {
                    decoded[i] = 0.0f;
                }
                else if (sample > 4.0f)
                {
                    decoded[i] = 4.0f;
                }
                else if (sample < -4.0f)
                {
                    decoded[i] = -4.0f;
                }
            }

            int complexSamples = channels <= 1 ? decoded.Length : decoded.Length / channels;

            TCIQueuedTxAudio queuedAudio = new TCIQueuedTxAudio()
            {
                Receiver = receiver,
                SampleRate = sampleRate,
                SampleType = sampleType,
                Channels = channels,
                ComplexSamples = complexSamples,
                Samples = convertStreamSamplesToComplex(decoded, channels)
            };

            lock (m_objTxQueueLock)
            {
                while (m_txAudioQueue.Count >= MAX_TX_AUDIO_QUEUE_BLOCKS ||
                       (m_txQueuedComplexSamples + queuedAudio.ComplexSamples) > MAX_TX_AUDIO_QUEUE_COMPLEX_SAMPLES)
                {
                    if (m_txAudioQueue.Count == 0)
                        break;

                    TCIQueuedTxAudio droppedAudio = m_txAudioQueue.Dequeue();
                    if (droppedAudio != null)
                        m_txQueuedComplexSamples = Math.Max(0, m_txQueuedComplexSamples - Math.Max(0, droppedAudio.ComplexSamples));
                }

                m_txAudioQueue.Enqueue(queuedAudio);
                m_txQueuedComplexSamples += Math.Max(0, queuedAudio.ComplexSamples);
            }
        }

        private void handleIQSampleRate(string[] args)
        {
            if (args.Length != 1) return;
            if (int.TryParse(args[0], out int sampleRate))
            {
				//just echo out that we have changed to keep client happy, we dont change Thetis H/W sample rate for now
				//
                //if (sampleRate == 48000 || sampleRate == 96000 || sampleRate == 192000 || sampleRate == 384000)
                //{
				//	for (int i = 0; i < m_hwSampleRate.Length; i++)
				//	{
				//		m_hwSampleRate[i] = sampleRate;
				//		applyIQSampleRateToReceiver(0, sampleRate);
				//	}
                //}
                sendIQSampleRate(sampleRate);
            }
        }

        private int getCurrentMaxHWSampleRate()
        {
			int max = 48000;
            try
            {
                for (int receiver = 0; receiver < cmaster.CMrcvr; receiver++)
                {
                    int sampleRate = cmaster.GetInputRate(0, receiver);
					if (sampleRate > max) max = sampleRate;
                }
            }
            catch { }

			return max;
        }

        private void handleAudioSampleRate(string[] args)
        {
            if (args.Length != 1) return;
            bool samplesChanged = false;
            int audioSampleRate;
            int audioStreamSamples;
            bool rateChanged = false;

            if (int.TryParse(args[0], out int sampleRate))
            {
                if (sampleRate == 8000 || sampleRate == 12000 || sampleRate == 24000 || sampleRate == 48000)
                {
                    lock (m_objStreamLock)
                    {
                        rateChanged = m_audioSampleRate != sampleRate;
                        m_audioSampleRate = sampleRate;

                        if (!m_audioStreamSamplesExplicitlySet)
                        {
                            int defaultSamples = getDefaultAudioStreamSamples(sampleRate);
                            if (m_audioStreamSamples != defaultSamples)
                            {
                                m_audioStreamSamples = defaultSamples;
                                samplesChanged = true;
                            }
                        }

                        audioSampleRate = m_audioSampleRate;
                        audioStreamSamples = m_audioStreamSamples;
                    }
                }
                else
                {
                    lock (m_objStreamLock)
                    {
                        audioSampleRate = m_audioSampleRate;
                        audioStreamSamples = m_audioStreamSamples;
                    }
                }
            }
            else
            {
                lock (m_objStreamLock)
                {
                    audioSampleRate = m_audioSampleRate;
                    audioStreamSamples = m_audioStreamSamples;
                }
            }

            if (rateChanged)
                clearRxAudioStreamState();

            sendAudioSampleRate(audioSampleRate);
            if (samplesChanged)
                sendAudioStreamSamples(audioStreamSamples);
        }

        private void handleIQStart(string[] args, bool enable)
        {
            if (args.Length != 1) return;
            if (!int.TryParse(args[0], out int receiver)) return;
            lock (m_objStreamLock)
            {
                if (enable) m_iqStreamEnabled.Add(receiver);
                else m_iqStreamEnabled.Remove(receiver);
            }
			// not used atm due to the TCI protocol not having per RX IQ sample rate
			// here for the future
			//if (enable)
			//    applyIQSampleRateToReceiver(receiver, m_iqSampleRate);

			sendIQStartStop(receiver, enable);
            m_server?.RefreshStreamRunState();
        }
        private void sendIQStartStop(int receiver, bool enable)
        {
            sendTextFrame((enable ? "iq_start:" : "iq_stop:") + receiver.ToString() + ";");
        }

        private void applyIQSampleRateToReceiver(int receiver, int sampleRate)
        {
            Console localConsole = consoleThreadSafe;
            if (localConsole == null || sampleRate <= 0) return;

            MethodInvoker applyRate = delegate
            {
                if (!localConsole.IsSetupFormNull)
                {
                    if (receiver == 0)
                    {
                        if (localConsole.SetupForm.GetHWSampleRate(1) != sampleRate)
                            localConsole.SetupForm.SetHWSampleRate(1, sampleRate);
                    }
                    else if (receiver == 1)
                    {
                        if (localConsole.RX2Enabled && localConsole.SetupForm.GetHWSampleRate(2) != sampleRate)
                            localConsole.SetupForm.SetHWSampleRate(2, sampleRate);
                    }
                }
            };

            if (localConsole.InvokeRequired)
                localConsole.Invoke(applyRate);
            else
                applyRate();
        }

		private void handleRxChannelEnable(string[] args)
		{
            if (args.Length < 2 || args.Length > 3) return;
            if (!int.TryParse(args[0], out int receiver)) return;
			if(args.Length == 2)
			{
                if (!int.TryParse(args[1], out int channel)) return;

				//get
				bool enabled = false;
                switch (receiver)
				{
					case 0:
						enabled = channel == 0 ? true : consoleThreadSafe.GetSubRX(1);
                        break;
					case 1:
						//just return rx2 state as no subrx
						enabled = channel == 0 ? consoleThreadSafe.RX2Enabled : false;
                        break;
				}
				sendRxChannelEnable(receiver, channel, enabled);
			}
			else
			{
                //set len 3
                if (!int.TryParse(args[1], out int channel)) return;
                if (!bool.TryParse(args[2], out bool enabled)) return;

				if (receiver == 0 && channel == 1)  // rx1 sub rx, cant disable rx1
				{
					consoleThreadSafe.SetSubRX(1, enabled);
				}
				else if(receiver == 1) // main or sub will set state
				{
					consoleThreadSafe.RX2Enabled = enabled;
				}

                sendRxChannelEnable(receiver, channel, enabled);
            }
        }
		private void sendRxChannelEnable(int rx, int channel, bool enabled)
		{
            sendTextFrame("rx_channel_enable:" + rx.ToString() + "," + channel.ToString() + "," + enabled.ToString().ToLower() + ";");
        }
        private void handleAudioStart(string[] args, bool enable)
        {
            if (args.Length != 1) return;
            if (!int.TryParse(args[0], out int receiver)) return;
            lock (m_objStreamLock)
            {
                if (enable) m_audioStreamEnabled.Add(receiver);
                else m_audioStreamEnabled.Remove(receiver);
            }

            if (!enable)
                clearRxAudioStateForReceiver(receiver);

            sendAudioStartStop(receiver, enable);
            m_server?.RefreshStreamRunState();
        }

        private void handleAudioStreamSampleType(string[] args)
        {
            if (args.Length != 1) return;
            lock (m_objStreamLock)
            {
                switch (args[0].Trim().ToLower())
                {
                    case "int16":
                        m_audioSampleType = TCISampleType.INT16;
                        break;
                    case "int24":
                        m_audioSampleType = TCISampleType.INT24;
                        break;
                    case "int32":
                        m_audioSampleType = TCISampleType.INT32;
                        break;
                    case "float32":
                    default:
                        m_audioSampleType = TCISampleType.FLOAT32;
                        break;
                }

                m_seenModernTxAudioNegotiation = true;
            }
            sendAudioStreamSampleType(m_audioSampleType);
        }

        private void handleAudioStreamChannels(string[] args)
        {
            if (args.Length != 1) return;
            lock (m_objStreamLock)
            {
                if (int.TryParse(args[0], out int channels))
                {
                    if (channels == 1 || channels == 2)
                        m_audioStreamChannels = channels;
                }

                m_seenModernTxAudioNegotiation = true;
            }
            sendAudioStreamChannels(m_audioStreamChannels);
        }

        private void handleAudioStreamSamples(string[] args)
        {
            if (args.Length != 1) return;
            int audioStreamSamples;
            if (int.TryParse(args[0], out int samples))
            {
                if (samples >= 100 && samples <= 2048)
                {
                    lock (m_objStreamLock)
                    {
                        m_audioStreamSamples = samples;
                        m_audioStreamSamplesExplicitlySet = true;
                        m_seenModernTxAudioNegotiation = true;
                        audioStreamSamples = m_audioStreamSamples;
                    }
                }
                else
                {
                    lock (m_objStreamLock)
                    {
                        audioStreamSamples = m_audioStreamSamples;
                    }
                }
            }
            else
            {
                lock (m_objStreamLock)
                {
                    audioStreamSamples = m_audioStreamSamples;
                }
            }
            sendAudioStreamSamples(audioStreamSamples);
        }

        private void handleTxStreamAudioBuffering(string[] args)
        {
            if (args.Length != 1) return;
            lock (m_objStreamLock)
            {
                if (int.TryParse(args[0], out int buffering))
                {
                    if (buffering >= 50 && buffering <= 500)
                       m_txStreamAudioBufferingMs = buffering;
                }

                m_seenModernTxAudioNegotiation = true;
            }
            sendTxStreamAudioBuffering(m_txStreamAudioBufferingMs);
        }

		private void PingFrameTimer(object o)
		{
			sendPingFrame("Thetis");
		}

        private void VFOcallback(Object o)
        {
            TCPIPtciSocketListener.VFOData vfod = (TCPIPtciSocketListener.VFOData)o;
            vfoFrequencyChange(vfod);
        }
        private void Centrecallback(Object o)
        {
            TCPIPtciSocketListener.VFOData vfod = (TCPIPtciSocketListener.VFOData)o;
            centreFrequencyChange(vfod);
        }
        public void VFOChange(VFOData vfod)
        {
            if (m_tmVFOtimer != null)
            {
                m_tmVFOtimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_tmVFOtimer = null;
            }

			bool bOK = !m_swVFO.IsRunning || (m_swVFO.IsRunning && m_swVFO.ElapsedMilliseconds > m_nRateLimit);

            if (bOK)
            {
                vfoFrequencyChange(vfod);
                if (m_nRateLimit > 0) m_swVFO.Restart();
            }
            else
            {
                m_tmVFOtimer = new System.Threading.Timer(VFOcallback, vfod, m_nRateLimit, Timeout.Infinite);
            }
        }
		public void CentreChange(VFOData vfod)
        {
            if (m_tmCentretimer != null)
            {
                m_tmCentretimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_tmCentretimer = null;
            }

			bool bOK = !m_swCentre.IsRunning || (m_swCentre.IsRunning && m_swCentre.ElapsedMilliseconds > m_nRateLimit);

            if (bOK)
            {
                centreFrequencyChange(vfod);
                if (m_nRateLimit > 0) m_swCentre.Restart();
            }
            else
            {
                m_tmCentretimer = new System.Threading.Timer(Centrecallback, vfod, m_nRateLimit, Timeout.Infinite);
            }
        }
        public void TXFrequencyChange(VFOData vfod)
        {
            if (m_tmTXFrequency != null)
            {
                m_tmTXFrequency.Change(Timeout.Infinite, Timeout.Infinite);
                m_tmTXFrequency = null;
            }

            bool bOK = !m_swTXFrequency.IsRunning || (m_swTXFrequency.IsRunning && m_swTXFrequency.ElapsedMilliseconds > m_nRateLimit);

            if (bOK)
            {
                txFrequencyChange(vfod);
                if (m_nRateLimit > 0) m_swTXFrequency.Restart();
            }
            else
            {
                m_tmTXFrequency = new System.Threading.Timer(Centrecallback, vfod, m_nRateLimit, Timeout.Infinite);
            }
        }
    }

	public class TCPIPtciServer
	{
		//
		public delegate void ClientConnected();
		public delegate void ClientDisconnected();
		public delegate void ClientError(SocketException se);
		public delegate void ServerError(SocketException se);
		public ClientConnected ClientConnectedHandlers;
		public ClientDisconnected ClientDisconnectedHandlers;
		public ClientError ClientErrorHandlers;
		public ServerError ServerErrorHandlers;
		//

		private Console _console;

		public static IPAddress DEFAULT_SERVER = IPAddress.Parse("127.0.0.1");
		public static int DEFAULT_PORT = 31001;
		public static IPEndPoint DEFAULT_IP_END_POINT =
			new IPEndPoint(DEFAULT_SERVER, DEFAULT_PORT);

		private TcpListener m_server = null;
		private bool m_stopServer = false;
		private bool m_stopPurging = false;
		private Thread m_serverThread = null;
		private Thread m_purgingThread = null;
		private List<TCPIPtciSocketListener> m_socketListenersList = null;
        private TCPIPtciSocketListener m_activeTxAudioListener = null;
		private object m_objLocker = new object();
        private bool m_bSleepingInPurge = false;
		private bool m_bDelegatesAdded = false;
		private int m_nRateLimit = 0;
		private bool m_bEmulateSunSDR2Pro = false;
		private bool m_bEmulateExpertSDR3Protocol = true;
        private bool m_bIQSwap = true;
        private bool m_bAlwaysStreamIQ = false;
        private TCITxStereoInputMode m_txStereoInputMode = TCITxStereoInputMode.Both;
        private TCICWController m_cwController = null;
        private int m_cwInternalMacroSpeedUpdates = 0;

        private frmLog _log;

		private Console console
		{
			get
			{
				if (_console == null) return null;

				if (_console.InvokeRequired)
				{
                    return (Console)_console.Invoke(new Func<Console>(() => _console));
                }
				else 
					return _console;
			}
		}
		public TCPIPtciServer()
		{
			Init(DEFAULT_IP_END_POINT);
		}
		public TCPIPtciServer(IPAddress serverIP)
		{
			Init(new IPEndPoint(serverIP, DEFAULT_PORT));
		}

		public TCPIPtciServer(int port)
		{
			Init(new IPEndPoint(DEFAULT_SERVER, port));
		}

		public TCPIPtciServer(IPAddress serverIP, int port)
		{
			Init(new IPEndPoint(serverIP, port));
		}

		public TCPIPtciServer(IPEndPoint ipNport)
		{
			Init(ipNport);
		}
		~TCPIPtciServer()
		{
			StopServer();
			if(_log != null)
            {
				_log.Close();
				_log = null;
            }
		}

		private void Init(IPEndPoint ipNport)
		{
			try
			{
				if (_log != null)
                {
					_log = null;
                }
				_log = new frmLog();
				m_server = new TcpListener(ipNport);
			}
			catch
			{
				m_server = null;
            }
        }

		public frmLog LogForm
        {
            get { return _log; }
        }
		private bool m_bCopyRX2VFObToVFOa = false;
		public bool CopyRX2VFObToVFOa
		{
			get { return m_bCopyRX2VFObToVFOa; }
			set { m_bCopyRX2VFObToVFOa = value;	}
        }
        private bool _replace_if_copy_RX2VFObToVFOa = false;
        public bool ReplaceRX2VFObIfCopyBtoA
        {
			get { return _replace_if_copy_RX2VFObToVFOa; }
			set { _replace_if_copy_RX2VFObToVFOa = value; }
        }
        private bool m_bCWLUbecomesCW = false;
		public bool CWLUbecomesCW
        {
			get { return m_bCWLUbecomesCW; }
			set { m_bCWLUbecomesCW = value; }
        }
		private bool m_bCWbecomesCWUabove10mhz = false;
		public bool CWbecomesCWUabove10mhz
		{
			get { return m_bCWbecomesCWUabove10mhz; }
			set { m_bCWbecomesCWUabove10mhz = value; }
		}
        private TCICWSpotForce _spot_force = TCICWSpotForce.DEFAULT;
        public TCICWSpotForce CWSpotForce
        {
            get { return _spot_force; }
            set { _spot_force = value; }
        }
        private bool m_bUseRX1VFOaForRX2VFOa = false;
		public bool UseRX1VFOaForRX2VFOa
		{
			get { return m_bUseRX1VFOaForRX2VFOa; }
			set { m_bUseRX1VFOaForRX2VFOa = value; }
		}
		private bool m_bSendInitialStateOnConnect = true;
		public bool SendInitialFrequencyStateOnConnect
		{
			get { return m_bSendInitialStateOnConnect; }
			set { m_bSendInitialStateOnConnect = value;	}
		}
		public bool EmulateSunSDR2Pro
        {
            get { return m_bEmulateSunSDR2Pro; }
			set { m_bEmulateSunSDR2Pro = value; }
        }
		public bool EmulateExpertSDR3Protocol
		{
			get { return m_bEmulateExpertSDR3Protocol; }
			set { m_bEmulateExpertSDR3Protocol = value; }
		}
        public bool IQSwap
        {
            get { return m_bIQSwap; }
            set { m_bIQSwap = value; }
        }
        public bool AlwaysStreamIQ
        {
            get { return m_bAlwaysStreamIQ; }
            set 
            { 
                m_bAlwaysStreamIQ = value; 
                RefreshStreamRunState(); 
            }
        }
        public TCITxStereoInputMode TXStereoInputMode
        {
            get { return m_txStereoInputMode; }
            set { m_txStereoInputMode = value; }
        }

        public void StartServer(Console c, int rateLimit = 0)
		{
			if (m_server != null)
			{
				m_nRateLimit = rateLimit;

				if (c != null && !c.IsSetupFormNull)
				{
					m_bSendInitialStateOnConnect = c.SetupForm.TCIsendInitialStateOnConnect;;

					m_bCopyRX2VFObToVFOa = c.SetupForm.TCIcopyRX2VFObToVFOa;
					_replace_if_copy_RX2VFObToVFOa = c.SetupForm.TCIreplaceRX2VFObToVFOa;
					m_bUseRX1VFOaForRX2VFOa = c.SetupForm.TCIuseRX1vfoaForRX2vfoa;

					m_bCWLUbecomesCW = c.SetupForm.TCICWLUbecomesCW;
					m_bCWbecomesCWUabove10mhz = c.SetupForm.TCICWbecomesCWUabove10mhz; //[2.10.3.9]MW0LGE fixes issue #559

					m_bEmulateSunSDR2Pro = c.SetupForm.EmulateSunSDR2Pro;
					m_bEmulateExpertSDR3Protocol = c.SetupForm.EmulateExpertSDR3Protocol;

					_spot_force = c.SetupForm.CWSpotForce;

                    m_bIQSwap = c.SetupForm.TCISwapIQ;
                    m_bAlwaysStreamIQ = c.SetupForm.TCIAlwaysStreamIQ;
                    m_txStereoInputMode = c.SetupForm.TCITXInputChannel;
                }
				else
				{
					m_bSendInitialStateOnConnect = true;

					m_bCopyRX2VFObToVFOa = false;
					_replace_if_copy_RX2VFObToVFOa = false;
					m_bUseRX1VFOaForRX2VFOa = false;

					m_bCWLUbecomesCW = false;
					m_bCWbecomesCWUabove10mhz = false;

					m_bEmulateSunSDR2Pro = false;
					m_bEmulateExpertSDR3Protocol = false;

					_spot_force = TCICWSpotForce.DEFAULT;

                    m_bIQSwap = true;
                    m_bAlwaysStreamIQ = false;
                    m_txStereoInputMode = TCITxStereoInputMode.Both;
                }

				_console = c;
                if (m_cwController != null)
                {
                    m_cwController.Dispose();
                    m_cwController = null;
                }
                if (_console != null)
                    m_cwController = new TCICWController(this);

				m_socketListenersList = new List<TCPIPtciSocketListener>();
                cmaster.SetRXTCIRun(0);

                if (console != null && !m_bDelegatesAdded)
				{
					console.VFOAFrequencyChangeHandlers += OnVFOAFrequencyChangeHandler;
					console.VFOBFrequencyChangeHandlers += OnVFOBFrequencyChangeHandler;
					console.MoxChangeHandlers += OnMoxChangeHandler;
                    console.MoxPreChangeHandlers += OnMoxPreChangeHandler;
					console.ModeChangeHandlers += OnModeChangeHandler;
					console.BandChangeHandlers += OnBandChangeHandler;
					console.CentreFrequencyHandlers += OnCentreFrequencyChanged;
					console.FilterChangedHandlers += OnFilterChanged;
					console.FilterEdgesChangedHandlers += OnFilterEdgesChanged;
                    console.TXFiltersChangedHandlers += OnTXFiltersChanged;
					console.PowerChangeHanders += OnPowerChangeHander;
					console.SplitChangedHandlers += OnSplitChanged;
					console.TuneChangedHandlers += OnTuneChanged;
					console.DrivePowerChangedHandlers += OnDrivePowerChanged;
					console.HWSampleRateChangedHandlers += OnHWSampleRateChanged;
					console.ThetisFocusChangedHandlers += OnThetisFocusChanged;
					console.RX2EnabledChangedHandlers += OnRX2EnabledChanged;
					console.SpotClickedHandlers += OnSpotClicked;
					console.MuteChangedHandlers += OnMuteChanged;
					console.MONChangedHandlers += OnMONChanged;
                    console.MONVolumeChangedHandlers += OnMONVolumeChanged;
                    console.VolumeChangedHandlers += OnVolumeChanged;
                    console.BalanceChangedHandlers += OnBalanceChanged;
                    console.StepAttEnabledChangedHandlers += OnStepAttEnabledChanged;
                    console.AttenuatorDataChangedHandlers += OnAttenuatorDataChanged;
                    console.PreampModeChangedHandlers += OnPreampModeChanged;
                    console.FMDeviationChangedHandlers += OnFMDeviationChanged;
                    console.AGCGainChangedHandlers += OnAGCGainChanged;
                    console.RITChangedHandlers += OnRITChanged;
                    console.XITChangedHandlers += OnXITChanged;
                    console.RITValueChangedHandlers += OnRITValueChanged;
                    console.XITValueChangedHandlers += OnXITValueChanged;
					console.TXFrequncyChangedHandlers += OnTXFrequencyChanged;
                    console.MeterReadingsChangedHandlers += OnMeterReadingsChanged;
                    console.NRChangedHandlers += OnNrChanged;
                    console.NBChangedHandlers += OnNbChanged;
                    console.ANFChangedHandlers += OnAnfChanged;
                    console.BINChangedHandlers += OnBinChanged;
                    console.AGCModeChangedHandlers += OnAGCModeChanged;
                    console.AGCAutoModeChangedHandlers += OnAGCAutoModeChanged;
                    console.VFOSyncChangedHandlers += OnVFOSyncChanged;
                    console.VfoALockChangedHandlers += OnVfoALockChanged;
                    console.VfoBLockChangedHandlers += OnVfoBLockChanged;
                    console.SQLChangedHandlers += OnSqlChanged;
                    console.SQLLevelChangedHandlers += OnSqlLevelChanged;
                    console.APFChangedHandlers += OnApfChanged;
                    console.TNFChangedHandlers += OnTnfChanged;
                    console.DIGLOffsetChangedHandlers += OnDiglOffsetChanged;
                    console.DIGUOffsetChangedHandlers += OnDiguOffsetChanged;

                    console.CWXSpeedChangedHandlers += OnCwMacrosSpeedChanged;
                    console.CWXDelayChangedHandlers += OnCwMacrosDelayChanged;
                    console.CWXRemoteCharacterStartedHandlers += OnCwRemoteCharacterStarted;
                    console.CWKeyerSpeedChangedHandlers += OnCwKeyerSpeedChanged;

                    console.RXGainChangedHandlers += OnRxAfGainChanged;
                    console.CTUNChangedHandlers += OnCTUNChanged;
                    console.TXProfileChangedHandlers += OnTXProfileChanged;
                    console.TXProfilesChangedHandlers += OnTXProfilesChanged;

                    console.MeterCalOffsetChangedHandlers += OnCalibrationChanged;
                    console.DisplayOffsetChangedHandlers += OnCalibrationChanged;
                    console.XvtrGainOffsetChangedHandlers += OnCalibrationChanged;
                    console.Rx6mOffsetChangedHandlers += OnCalibrationChanged;

                    m_bDelegatesAdded = true;
				}

				try
				{
					m_server.Start();

					m_serverThread = new Thread(new ThreadStart(ServerThreadStart));
					m_serverThread.Priority = ThreadPriority.BelowNormal;
					m_serverThread.Name = "TCI server Thread";
					m_serverThread.Start();

					m_purgingThread = new Thread(new ThreadStart(PurgingThreadStart));
					m_purgingThread.Priority = ThreadPriority.Lowest;
                    m_purgingThread.Name = "TCI purging Thread";
                    m_purgingThread.Start();
				}
				catch(SocketException se)
                {
					m_sLastError = se.Message;
					StopServer();

					ServerErrorHandlers?.Invoke(se);
				}
                catch
				{
					StopServer();
				}
			}
		}

		private string m_sLastError = "";
		public string LastError
        {
			get 
            {
				string s = m_sLastError;
				m_sLastError = "";
				return s; 
			}
        }

		public void StopServer()
		{
			if (m_server != null)
			{
				if (m_bDelegatesAdded)
				{
					console.VFOAFrequencyChangeHandlers -= OnVFOAFrequencyChangeHandler;
					console.VFOBFrequencyChangeHandlers -= OnVFOBFrequencyChangeHandler;
					console.MoxChangeHandlers -= OnMoxChangeHandler;
                    console.MoxPreChangeHandlers -= OnMoxPreChangeHandler;
					console.ModeChangeHandlers -= OnModeChangeHandler;
					console.BandChangeHandlers -= OnBandChangeHandler;
					console.CentreFrequencyHandlers -= OnCentreFrequencyChanged;
					console.FilterChangedHandlers -= OnFilterChanged;
					console.FilterEdgesChangedHandlers -= OnFilterEdgesChanged;
                    console.TXFiltersChangedHandlers -= OnTXFiltersChanged;
					console.PowerChangeHanders -= OnPowerChangeHander;
					console.SplitChangedHandlers -= OnSplitChanged;
					console.TuneChangedHandlers -= OnTuneChanged;
					console.DrivePowerChangedHandlers -= OnDrivePowerChanged;
					console.HWSampleRateChangedHandlers -= OnHWSampleRateChanged;
					console.ThetisFocusChangedHandlers -= OnThetisFocusChanged;
					console.RX2EnabledChangedHandlers -= OnRX2EnabledChanged;
					console.SpotClickedHandlers -= OnSpotClicked;
                    console.MuteChangedHandlers -= OnMuteChanged;
                    console.MONChangedHandlers -= OnMONChanged;
                    console.MONVolumeChangedHandlers -= OnMONVolumeChanged;
                    console.VolumeChangedHandlers -= OnVolumeChanged;
                    console.BalanceChangedHandlers -= OnBalanceChanged;
                    console.StepAttEnabledChangedHandlers -= OnStepAttEnabledChanged;
                    console.AttenuatorDataChangedHandlers -= OnAttenuatorDataChanged;
                    console.PreampModeChangedHandlers -= OnPreampModeChanged;
                    console.FMDeviationChangedHandlers -= OnFMDeviationChanged;
                    console.AGCGainChangedHandlers -= OnAGCGainChanged;
                    console.RITChangedHandlers -= OnRITChanged;
                    console.XITChangedHandlers -= OnXITChanged;
                    console.RITValueChangedHandlers -= OnRITValueChanged;
                    console.XITValueChangedHandlers -= OnXITValueChanged;
                    console.TXFrequncyChangedHandlers -= OnTXFrequencyChanged;
                    console.MeterReadingsChangedHandlers -= OnMeterReadingsChanged;
                    console.NRChangedHandlers -= OnNrChanged;
                    console.NBChangedHandlers -= OnNbChanged;
                    console.ANFChangedHandlers -= OnAnfChanged;
                    console.BINChangedHandlers -= OnBinChanged;
                    console.AGCModeChangedHandlers -= OnAGCModeChanged;
                    console.AGCAutoModeChangedHandlers -= OnAGCAutoModeChanged;
                    console.VFOSyncChangedHandlers -= OnVFOSyncChanged;
                    console.VfoALockChangedHandlers -= OnVfoALockChanged;
                    console.VfoBLockChangedHandlers -= OnVfoBLockChanged;
                    console.SQLChangedHandlers -= OnSqlChanged;
                    console.SQLLevelChangedHandlers -= OnSqlLevelChanged;
                    console.APFChangedHandlers -= OnApfChanged;
                    console.TNFChangedHandlers -= OnTnfChanged;
                    console.DIGLOffsetChangedHandlers -= OnDiglOffsetChanged;
                    console.DIGUOffsetChangedHandlers -= OnDiguOffsetChanged;

                    console.CWXSpeedChangedHandlers -= OnCwMacrosSpeedChanged;
                    console.CWXDelayChangedHandlers -= OnCwMacrosDelayChanged;
                    console.CWXRemoteCharacterStartedHandlers -= OnCwRemoteCharacterStarted;
                    console.CWKeyerSpeedChangedHandlers -= OnCwKeyerSpeedChanged;

                    console.RXGainChangedHandlers -= OnRxAfGainChanged;
                    console.CTUNChangedHandlers -= OnCTUNChanged;
                    console.TXProfileChangedHandlers -= OnTXProfileChanged;
                    console.TXProfilesChangedHandlers -= OnTXProfilesChanged;

                    console.MeterCalOffsetChangedHandlers -= OnCalibrationChanged;
                    console.DisplayOffsetChangedHandlers -= OnCalibrationChanged;
                    console.XvtrGainOffsetChangedHandlers -= OnCalibrationChanged;
                    console.Rx6mOffsetChangedHandlers -= OnCalibrationChanged;

                    m_bDelegatesAdded = false;
				}

				// Stop the TCP/IP Server, so can clean up without more clients connecting
				m_stopServer = true;
				try
				{
					m_server.Stop();
				}
                catch
                {

                }

				if (m_serverThread != null) {
					m_serverThread.Join(50); // dont need to wait long here, as we are blocking anyway
					if (m_serverThread.IsAlive)
						m_serverThread.Abort();
					m_serverThread = null;
				}

				m_stopPurging = true;
				if (m_purgingThread != null)
				{
					if (!m_bSleepingInPurge)
						m_purgingThread.Join(500);

					if (m_purgingThread.IsAlive)
						m_purgingThread.Abort();
					m_purgingThread = null;
				}


				// Stop All clients.
				StopAllSocketListers();
                cmaster.SetRXTCIRun(0);

                if (m_cwController != null)
                {
                    m_cwController.Dispose();
                    m_cwController = null;
                }

				m_server = null;
			}
		}

        internal int GetCwMacrosSpeed()
        {
            return m_cwController != null ? m_cwController.GetMacroSpeed() : 30;
        }

        internal void SetCwMacrosSpeed(int wpm)
        {
            m_cwController?.SetMacroSpeed(wpm);
        }

        internal int GetCwMacrosDelay()
        {
            return m_cwController != null ? m_cwController.GetMacroDelayMs() : 0;
        }

        internal void SetCwMacrosDelay(int delayMs)
        {
            m_cwController?.SetMacroDelayMs(delayMs);
        }

        internal int GetCwKeyerSpeed()
        {
            return m_cwController != null ? m_cwController.GetKeyerSpeed() : 30;
        }

        internal void SetCwKeyerSpeed(int wpm)
        {
            m_cwController?.SetKeyerSpeed(wpm);
        }

        internal void IncreaseCwMacrosSpeed(int amount)
        {
            m_cwController?.IncreaseMacroSpeed(amount);
        }

        internal void DecreaseCwMacrosSpeed(int amount)
        {
            m_cwController?.DecreaseMacroSpeed(amount);
        }

        internal void SetCwTerminalEnabled(TCPIPtciSocketListener socketListener, int rx, bool enabled)
        {
            m_cwController?.SetTerminalEnabled(socketListener, rx, enabled);
        }

        internal void SendCwMacro(TCPIPtciSocketListener socketListener, int rx, string text)
        {
            m_cwController?.SendMacro(socketListener, rx, text);
        }

        internal void SendCwMessage(TCPIPtciSocketListener socketListener, int rx, string prefix, string callsign, string suffix)
        {
            m_cwController?.SendMessage(socketListener, rx, prefix, callsign, suffix);
        }

        internal void UpdateCwMessageCallsign(TCPIPtciSocketListener socketListener, string callsign)
        {
            m_cwController?.UpdatePendingCallsign(socketListener, callsign);
        }

        internal void StopCwMacros(TCPIPtciSocketListener socketListener)
        {
            m_cwController?.Stop(socketListener);
        }

        internal void HandleCwKeyer(TCPIPtciSocketListener socketListener, int rx, bool pressed, int durationMs)
        {
            m_cwController?.HandleKeyer(socketListener, rx, pressed, durationMs);
        }

        internal void NotifyCwTciPttReleased(TCPIPtciSocketListener socketListener)
        {
            m_cwController?.HandleTciPttReleased(socketListener);
        }

        internal void OnSocketListenerDisconnected(TCPIPtciSocketListener socketListener)
        {
            m_cwController?.DisconnectClient(socketListener);
        }

        internal void OnCwMacrosEmpty(int rx)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.CwMacrosEmpty(rx);
                }
            }
        }

        internal void OnCwCallsignSent(string callsign)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.CwCallsignSent(callsign);
                }
            }
        }

        private void OnCwMacrosSpeedChanged(int oldSpeed, int newSpeed)
        {
            if (Interlocked.CompareExchange(ref m_cwInternalMacroSpeedUpdates, 0, 0) > 0)
                return;

            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.CwMacrosSpeedChanged(newSpeed);
                }
            }
        }

        private void OnCwMacrosDelayChanged(int oldDelay, int newDelay)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.CwMacrosDelayChanged(newDelay);
                }
            }
        }

        private void OnCwRemoteCharacterStarted(int remainingRemoteCharacters, int pendingElements)
        {
            m_cwController?.OnRemoteCharacterStarted(remainingRemoteCharacters, pendingElements);
        }

        private void OnCwKeyerSpeedChanged(int oldSpeed, int newSpeed)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.CwKeyerSpeedChanged(newSpeed);
                }
            }
        }

		public int ClientsConnected
        {
            get 
            {				
				int nRet = 0;
				lock (m_objLocker)
				{
                    if (m_server == null || m_socketListenersList == null) return nRet;

                    foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                    {
						if (!socketListener.IsDisconnected()) nRet++;
                    }
				}
				return nRet;
			}
        }
		public bool IsServerRunning
        {
			get { return m_server != null; }
        }
		private void StopAllSocketListers()
		{
            List<TCPIPtciSocketListener> stopList;

			lock (m_objLocker)
			{
                if (m_socketListenersList == null) return;

                stopList = new List<TCPIPtciSocketListener>(m_socketListenersList);
				m_socketListenersList.Clear();
				m_socketListenersList = null;
			}

            foreach (TCPIPtciSocketListener socketListener in stopList)
			{
				socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
				socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
				socketListener.ClientErrorHandlers -= ClientErrorHandler;
                socketListener.StopSocketListener();
			}
		}
		private void ServerThreadStart()
		{
			//Socket clientSocket = null;
			TcpClient client = null;

			TCPIPtciSocketListener socketListener = null;
			bool bAddedDelegates = false;
			while (!m_stopServer)
			{
				try
				{
					bAddedDelegates = false;
					//clientSocket = m_server.AcceptSocket();
					client = m_server.AcceptTcpClient();
					client.NoDelay = true;

					socketListener = new TCPIPtciSocketListener(client, console, this, m_nRateLimit);

					lock (m_objLocker)
					{
						m_socketListenersList.Add(socketListener);
					}

					socketListener.ClientConnectedHandlers += ClientConnectedHandler;
					socketListener.ClientDisconnectedHandlers += ClientDisconnectedHandler;
					socketListener.ClientErrorHandlers += ClientErrorHandler;
					bAddedDelegates = true;

					socketListener.StartSocketListener();
				}
				catch (SocketException se)
				{
                    if (bAddedDelegates && socketListener != null)
                    {
						socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
						socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
						socketListener.ClientErrorHandlers -= ClientErrorHandler;
					}
					m_stopServer = true;
					m_sLastError = se.Message;
					ServerErrorHandlers?.Invoke(se);
				}
                catch
                {
					if (bAddedDelegates && socketListener != null)
					{
						socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
						socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
						socketListener.ClientErrorHandlers -= ClientErrorHandler;
					}
					m_stopServer = true;
				}
			}
		}
		private void PurgingThreadStart()
		{
			while (!m_stopPurging)
			{
                List<TCPIPtciSocketListener> deleteList = new List<TCPIPtciSocketListener>();

				lock (m_objLocker)
				{
                    if (m_server == null || m_socketListenersList == null) return;

                    foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
					{
						if (socketListener.IsMarkedForDeletion())
						{
							deleteList.Add(socketListener);
						}
					}

					for (int i = 0; i < deleteList.Count; ++i)
					{
						m_socketListenersList.Remove(deleteList[i]);
					}
				}

                foreach (TCPIPtciSocketListener socketListener in deleteList)
                {
                    socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
                    socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
                    socketListener.ClientErrorHandlers -= ClientErrorHandler;
                    socketListener.StopSocketListener();
                }

				deleteList = null;

				m_bSleepingInPurge = true;
				Thread.Sleep(5000);
				m_bSleepingInPurge = false;
			}
		}
		private void ClientConnectedHandler()
		{
            RefreshStreamRunState();
			ClientConnectedHandlers?.Invoke();
		}
		private void ClientDisconnectedHandler()
        {
            RefreshStreamRunState();
			ClientDisconnectedHandlers?.Invoke();
        }
		private void ClientErrorHandler(SocketException se)
        {
			m_sLastError = se.Message;

			ClientErrorHandlers?.Invoke(se);
		}
		public void OnVFOAFrequencyChangeHandler(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
		{
            bool bVFOaUseRX2;
            if (console != null)
                bVFOaUseRX2 = console.RX2Enabled && UseRX1VFOaForRX2VFOa;
            else
                bVFOaUseRX2 = false;

            TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData()
            {
                cen = false,
                centreMHz = -1,
                rx = bVFOaUseRX2 ? 1 : rx - 1,
                freqMHz = newFreq,
                offsetHz = (int)-offset,
                chan = 0,
                duplicate_tochan = -1,
                replace_if_duplicated = false,
                sendIF = true
            };

            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
					socketListener.VFOChange(vfod);
                }
            }
        }
		public void OnVFOBFrequencyChangeHandler(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
		{
            TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData()
            {
                cen = false,
                centreMHz = -1,
                rx = rx - 1,
                freqMHz = newFreq,
                offsetHz = (int)-offset,
                chan = 1,
                duplicate_tochan = m_bCopyRX2VFObToVFOa && console.RX2Enabled ? 0 : -1,
                replace_if_duplicated = m_bCopyRX2VFObToVFOa && _replace_if_copy_RX2VFObToVFOa && console.RX2Enabled,
                sendIF = true
            };

			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.VFOChange(vfod);
				}
			}
        }
		public void OnMoxChangeHandler(int rx, bool oldMox, bool newMox)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.MoxChange(rx, oldMox, newMox);
					socketListener.SyncTciPttToMox(newMox);
				}
			}

            RefreshTxAudioSourceState();
		}

        public void OnMoxPreChangeHandler(int rx, bool currentMox, bool expectedMox)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.SyncTciPttToMox(expectedMox);
                }
            }

            RefreshTxAudioSourceState();
		}
		public void OnModeChangeHandler(int rx, DSPMode oldMode, DSPMode newMode, Band oldBand, Band newBand)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.ModeChange(rx,oldMode,newMode, oldBand, newBand);
				}
			}
		}
		public void OnBandChangeHandler(int rx, Band oldBand, Band newBand)
		{
			lock (m_objLocker)
			{
				if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.BandChange(rx, oldBand, newBand);
				}
			}
		}
		public void OnCentreFrequencyChanged(int rx, double oldFreq, double newFreq, Band band, double offset)
		{
			//only want to send IF with this if CTUN is enabled
			bool bCTun = rx == 1 ? console.ClickTuneDisplay : console.ClickTuneRX2Display;

            TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData()
            {
                freqMHz = -1,
                offsetHz = bCTun ? (int)-offset : -1,
                chan = 0,
                centreMHz = newFreq,
                cen = true,
                rx = rx - 1,
                duplicate_tochan = -1,
                replace_if_duplicated = false,
                sendIF = bCTun
            };
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.CentreChange(vfod);
                }
            }
        }
		public void OnFilterChanged(int rx, Filter oldFilter, Filter newFilter, Band band, int low, int high, string sName)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.FilterChange(rx, oldFilter, newFilter, band, low, high);
				}
			}
		}
		public void OnFilterEdgesChanged(int rx, Filter filter, Band band, int low, int high, string sName, int max_width, int max_shift)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.FilterEdgesChange(rx, filter, band, low, high);
				}
			}
		}
        public void OnTXFiltersChanged(int low, int high)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.TXFilterBandChanged(low, high);
                }
            }
        }
		public void OnPowerChangeHander(bool oldPower, bool newPower)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.PowerChange(oldPower, newPower);
				}
			}
		}
		public void OnThetisFocusChanged(bool focus)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.ThetisFocusChange(focus);
				}
			}
		}
		public void OnRX2EnabledChanged(bool enabled)
        {
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.RX2EnabledChange(enabled);
				}
			}
		}
		private void OnHWSampleRateChanged(int rx, int oldSampleRate, int newSampleRate)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.HWSampleRateChange(rx, oldSampleRate, newSampleRate);
				}
			}
		}
		private void OnDrivePowerChanged(int rx, int newPower, bool tune)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.DrivePowerChange(rx, newPower, tune);
				}
			}
		}
		private void OnTuneChanged(int rx, bool oldTune, bool newTune)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.TuneChange(rx, oldTune, newTune);
				}
			}
		}
		private void OnSplitChanged(int rx, bool oldSplit, bool newSplit)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.SplitChange(rx, newSplit);
				}
			}
		}

		private void OnSpotClicked(string callsign, long frequencyHz, int rx = -1, bool vfoB = false)
		{
			lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.ClickedOnSpot(callsign, frequencyHz); // also send legacy command (EESDR3 does this)	MW0LGE [2.9.0.8]																	
                    socketListener.ClickedOnSpot(callsign, frequencyHz, rx, vfoB ? 1 : 0);
				}
			}
		}
		private void OnMuteChanged(int rx, bool oldState, bool newState)
		{
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.MuteChanged(rx, newState);
                }
            }
        }
        private void OnNrChanged(int rx, int old_nr, int new_nr)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.NRChanged(rx, new_nr);
                }
            }
        }
        private void OnNbChanged(int rx, int old_nb, int new_nb)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.NBChanged(rx, new_nb);
                }
            }
        }
        private void OnAnfChanged(int rx, bool old_state, bool new_state)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.AnfChanged(rx, new_state);
                }
            }
        }
        private void OnBinChanged(int rx, bool old_state, bool new_state)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.BinChanged(rx, new_state);
                }
            }
        }
        private void OnAGCModeChanged(int rx, AGCMode old_mode, AGCMode new_mode)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.AGCModeChanged(rx, new_mode);
                }
            }
        }
        private void OnAGCAutoModeChanged(int rx, bool old_state, bool new_state)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.AGCAutoChanged(rx, new_state);
                }
            }
        }
        private void OnVFOSyncChanged(int rx, bool old_state, bool new_state)
        {
            if (rx != 1) return;

            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.VFOSyncChanged(new_state);
                }
            }
        }
        private void OnVfoALockChanged(int rx, bool old_state, bool new_state)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.LockChanged(1, new_state);
                    socketListener.VFOLocksChanged();
                }
            }
        }
        private void OnVfoBLockChanged(int rx, bool old_state, bool new_state)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    if (rx == 2)
                    socketListener.LockChanged(2, new_state);
                    socketListener.VFOLocksChanged();
                }
            }
        }
        private void OnSqlChanged(int rx, SquelchState old_state, SquelchState new_state)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.SqlChanged(rx, new_state);
                }
            }
        }
        private void OnSqlLevelChanged(int rx, int oldValue, int newValue)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.SqlLevelChanged(rx, newValue);
                }
            }
        }
        private void OnApfChanged(int rx, bool oldState, bool newState)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.ApfChanged(rx, newState);
                }
            }
        }
        private void OnTnfChanged(bool old_tnf, bool new_tnf)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.NfChanged(new_tnf);
                }
            }
        }
        private void OnDiglOffsetChanged(int oldValue, int newValue)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.DiglOffsetChanged(newValue);
                }
            }
        }
        private void OnDiguOffsetChanged(int oldValue, int newValue)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.DiguOffsetChanged(newValue);
                }
            }
        }
        private void OnRxAfGainChanged(int rx, bool is_subrx, int old_gain, int new_gain)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.RxAfGainChanged(rx, is_subrx, new_gain);
                }
            }
        }
        private void OnCTUNChanged(int rx, bool oldCTUN, bool newCTUN, Band band)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.CTUNChanged(rx, newCTUN);
                }
            }
        }
        private void OnTXProfileChanged(string old_name, string new_name)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.TXProfileChanged(new_name);
                }
            }
        }
        private void OnTXProfilesChanged()
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.TXProfilesChanged();
                }
            }
        }
        private void OnCalibrationChanged(int rx, float oldcal, float newcal)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.CalibrationChanged(rx);
                }
            }
        }
        private void OnMONChanged(bool oldState, bool newState)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.MONChanged(newState);
                }
            }
        }
        private void OnMONVolumeChanged(int oldVolume, int newVolume)
        {
            lock (m_objLocker)
            {
				if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.MONVolumeChanged(newVolume);
                }
            }
        }
        private void OnVolumeChanged(int oldVolume, int newVolume)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.VolumeChanged(newVolume);
                }
            }
        }
        private void OnBalanceChanged(int rx, bool is_subrx, int oldValue, int newValue)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.BalanceChanged(rx, is_subrx, newValue);
                }
            }
        }
        private void OnAttenuatorDataChanged(int rx, int oldValue, int newValue)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.RxStepAttChanged(rx, newValue);
                }
            }
        }
        private void OnStepAttEnabledChanged(int rx, bool oldEnabled, bool newEnabled)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.RxStepAttEnabledChanged(rx, newEnabled);
                }
            }
        }
        private void OnPreampModeChanged(int rx, PreampMode oldMode, PreampMode newMode)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.RxPreampAttChanged(rx, newMode);
                }
            }
        }
        private void OnFMDeviationChanged(int rx, int oldValue, int newValue)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.FMDeviationChanged(rx, newValue);
                }
            }
        }
        private void OnAGCGainChanged(int rx, int oldValue, int newValue)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.AGCGainChanged(rx, newValue);
                }
            }
        }
        private void OnRITChanged(bool oldState, bool newState)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.RITChanged(newState);
                }
            }
        }
        private void OnXITChanged(bool oldState, bool newState)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.XITChanged(newState);
                }
            }
        }
        private void OnRITValueChanged(int oldValue, int newValue)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.RITValueChanged(newValue);
                }
            }
        }
        private void OnXITValueChanged(int oldValue, int newValue)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.XITValueChanged(newValue);
                }
            }
        }
		private void OnTXFrequencyChanged(double old_frequency, double new_frequency, Band old_band, Band new_band, bool rx2_enabled, bool tx_vfob, double centre_freq)
		{
            TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData()
            {
                cen = false,
                centreMHz = -1,
                //rx = -1,
                rx = rx2_enabled && tx_vfob ? 1 : 0,
                freqMHz = new_frequency,
                offsetHz = -1,
                chan = -1,
                duplicate_tochan = -1,
                replace_if_duplicated = false,
                sendIF = false,

                SendTXInfo = true,
                TXfreqBand = new_band,
                RX2EnabledForTX = rx2_enabled,
                TXVFOB = tx_vfob
            };

            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.TXFrequencyChange(vfod);
                }
            }
        }
        private void OnMeterReadingsChanged(int rx, bool tx, ref Dictionary<Reading, float> readings)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.MeterReadingsChanged(rx, tx, ref readings);
                }
            }
        }
        public void ShowLog()
        {
			if (_log != null) _log.ShowWithTitle("TCI");
		}

		public void CloseLog()
        {
			if (_log != null) _log.Hide();
		}
		public void SendSpotSimulationClickToAll(string callsign, long freq)
		{            
            lock (m_objLocker)
			{
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
					socketListener.ClickedOnSpot(callsign, freq);
                    socketListener.ClickedOnSpot(callsign, freq, 1, 0); //[2.10.3.9]MW0LGE also send out RX_CLICKED_ON_SPOT defaults to rx1 and vfoA
                }
            }
		}
        internal void RefreshStreamRunState()
        {
            bool run = false;

            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                bool hasReadyClient = false;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    if (socketListener == null || !socketListener.IsReadyForStreaming())
                        continue;

                    hasReadyClient = true;

                    if (socketListener.WantsAnyRxStream())
                    {
                        run = true;
                        break;
                    }
                }

                if (!run && hasReadyClient && m_bAlwaysStreamIQ)
                    run = true;
            }

            cmaster.SetRXTCIRun(run ? 1 : 0);
        }

        public void PublishIQSamples(int receiver, int sampleRate, float[] iqSamples, int complexSamples = -1)
        {            
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.PublishIQSamples(receiver, sampleRate, iqSamples, complexSamples);
                }
            }
        }

        public void PublishRxAudioSamples(int receiver, int sampleRate, float[] left, float[] right, int samples = -1)
        {            
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.PublishRxAudioSamples(receiver, sampleRate, left, right, samples);
                }
            }
        }

        public bool RequiresRxSensorUpdate(int receiver, int channel)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return false;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    if (socketListener != null && socketListener.RequiresRxSensorUpdate(receiver, channel))
                        return true;
                }
            }

            return false;
        }

        public bool SensorRequiresUpdate(int receiver, Reading reading)
        {
            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return false;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    if (socketListener != null && socketListener.SensorRequiresUpdate(receiver, reading))
                        return true;
                }
            }

            return false;
        }

        public int MinimumRequiredRxSensorInterval()
        {
            int interval = int.MaxValue;

            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return interval;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    if (socketListener == null) continue;

                    int listenerInterval = socketListener.MinimumRequiredRxSensorInterval();
                    if (listenerInterval < interval) interval = listenerInterval;
                }
            }

            return interval;
        }

        public int MinimumRequiredTxSensorInterval()
        {
            int interval = int.MaxValue;

            lock (m_objLocker)
            {
                if (m_server == null || m_socketListenersList == null) return interval;

                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    if (socketListener == null) continue;

                    int listenerInterval = socketListener.MinimumRequiredTxSensorInterval();
                    if (listenerInterval < interval) interval = listenerInterval;
                }
            }

            return interval;
        }

        private TCPIPtciSocketListener GetActiveTxAudioListener()
        {
            if (m_server == null || m_socketListenersList == null)
            {
                m_activeTxAudioListener = null;
                return null;
            }

            if (m_activeTxAudioListener != null && !m_socketListenersList.Contains(m_activeTxAudioListener))
                m_activeTxAudioListener = null;

            return m_activeTxAudioListener;
        }

        internal bool TryAcquireActiveTxAudioListener(TCPIPtciSocketListener socketListener)
        {
            lock (m_objLocker)
            {
                TCPIPtciSocketListener activeListener = GetActiveTxAudioListener();
                if (activeListener != null && !activeListener.UsesActiveTCITxAudio())
                {
                    m_activeTxAudioListener = null;
                    activeListener = null;
                }

                if (activeListener == null || activeListener == socketListener)
                {
                    m_activeTxAudioListener = socketListener;
                        return true;
                }
            }

            return false;
        }

        internal void ReleaseActiveTxAudioListener(TCPIPtciSocketListener socketListener)
        {
            lock (m_objLocker)
            {
                if (m_activeTxAudioListener == socketListener)
                    m_activeTxAudioListener = null;
            }
        }

        internal bool UsesActiveTCITxAudio()
        {
            lock (m_objLocker)
            {
                TCPIPtciSocketListener activeListener = GetActiveTxAudioListener();
                return activeListener != null && activeListener.UsesActiveTCITxAudio();
            }
        }
        internal bool TryGetTxAudioRequestSettings(out int sampleRate, out int samples, out int bufferingMs)
        {
            sampleRate = 0;
            samples = 0;
            bufferingMs = 0;

            lock (m_objLocker)
            {
                TCPIPtciSocketListener activeListener = GetActiveTxAudioListener();
                if (activeListener == null)
                    return false;

                return activeListener.TryGetTxAudioRequestSettings(out sampleRate, out samples, out bufferingMs);
            }
        }

        internal void RefreshTxAudioSourceState()
        {
            cmaster.SetTXTCIAudioRun(0, UsesActiveTCITxAudio() ? 1 : 0);
        }

        public void SendTxChrono(int receiver)
        {           
            lock (m_objLocker)
            {
                TCPIPtciSocketListener activeListener = GetActiveTxAudioListener();
                if (activeListener == null) return;

                activeListener.SendTxChrono(receiver);
            }
        }

        internal bool TryDequeueTxAudio(out TCIQueuedTxAudio queuedAudio)
        {
            lock (m_objLocker)
            {
                TCPIPtciSocketListener activeListener = GetActiveTxAudioListener();
                if (activeListener != null && activeListener.TryDequeueTxAudio(out queuedAudio))
                    return true;
            }

            queuedAudio = null;
            return false;
        }

        // CW TCI SUPPORT
        #region TCICW_Support
        private sealed class TCICWController : IDisposable
        {
            private const int DirectKeyerWatchdogMs = 3000;

            private sealed class CWTxSegment
            {
                public string Text;
                public int SpeedWpm;
            }

            private sealed class CWTextParseResult
            {
                public readonly List<CWTxSegment> Segments = new List<CWTxSegment>();
                public int FinalSpeedWpm;
                public bool UsedInlineSpeedChanges;
            }

            private sealed class CWTxOperation
            {
                public readonly List<CWTxSegment> Segments = new List<CWTxSegment>();
                public int Rx;
                public int NextSegmentIndex;
                public int ActiveSegmentIndex = -1;
                public int CallsignSegmentIndex = -1;
                public string CallsignBase = string.Empty;
                public int BaseSpeedWpm;
                public bool RestoreBaseSpeed;
                public bool EmptyNotified;
                public bool CallsignNotified;
                public TCPIPtciSocketListener Owner;
            }

            private readonly TCPIPtciServer _server;
            private readonly object _lockObj = new object();
            private readonly System.Threading.Timer _pollTimer;
            private readonly AutoResetEvent _keyerScheduleEvent;
            private readonly Thread _keyerSchedulerThread;
            private readonly Stopwatch _keyerStopwatch;
            private readonly Queue<CWTxOperation> _pendingOperations = new Queue<CWTxOperation>();
            private CWTxOperation _activeOperation = null;
            private readonly bool[] _terminalEnabledByRx = new bool[2];
            private TCPIPtciSocketListener _currentOwner = null;
            private bool _terminalTciPttAsserted = false;
            private int _terminalTciPttRx = -1;
            private bool _releaseTerminalTciPttWhenIdle = false;
            private bool _keyerPressed = false;
            private bool _keyerReleasePending = false;
            private long _keyerPressedAtTicks = -1;
            private long _keyerReleaseAtTicks = -1;
            private int _keyerRx = -1;
            private bool _keyerAssertedMox = false;
            private bool _disposed = false;

            public TCICWController(TCPIPtciServer server)
            {
                _server = server;
                _pollTimer = new System.Threading.Timer(PollCallback, null, 50, 50);
                _keyerScheduleEvent = new AutoResetEvent(false);
                _keyerStopwatch = Stopwatch.StartNew();
                _keyerSchedulerThread = new Thread(KeyerSchedulerThreadProc)
                {
                    IsBackground = true,
                    Name = "TCI CW Keyer",
                    Priority = ThreadPriority.Highest
                };
                _keyerSchedulerThread.Start();

                // Best-effort reset so a stale direct-key state from an earlier session
                // does not affect the first keyed element after startup.
                setDirectKeyerState(false);
            }

            public void Dispose()
            {
                bool stopKeyer;
                bool shouldReleaseDirectKeyerMox;
                bool releaseTerminalTciPtt;

                lock (_lockObj)
                {
                    _disposed = true;
                    _pendingOperations.Clear();
                    _activeOperation = null;
                    stopKeyer = _keyerPressed || _keyerReleasePending;
                    _keyerPressed = false;
                    _keyerReleasePending = false;
                    _keyerPressedAtTicks = -1;
                    _keyerReleaseAtTicks = -1;
                    _keyerRx = -1;
                    shouldReleaseDirectKeyerMox = captureDirectKeyerMoxReleaseLocked();
                    releaseTerminalTciPtt = _terminalTciPttAsserted;
                    _terminalTciPttAsserted = false;
                    _terminalTciPttRx = -1;
                    _currentOwner = null;
                }

                _keyerScheduleEvent.Set();
                try
                {
                    _keyerSchedulerThread?.Join(250);
                }
                catch
                {
                }

                _pollTimer?.Dispose();
                if (stopKeyer)
                    setDirectKeyerState(false);
                if (shouldReleaseDirectKeyerMox)
                    releaseDirectKeyerMox();
                if (releaseTerminalTciPtt)
                    InvokeOnConsole(c => c.TCIPTT = false);
                _keyerScheduleEvent?.Dispose();
            }

            public int GetMacroSpeed()
            {
                lock (_lockObj)
                {
                    if (_activeOperation != null && _activeOperation.RestoreBaseSpeed)
                        return _activeOperation.BaseSpeedWpm;
                }

                return InvokeOnConsole(c => c.CWXForm.WPM, 30);
            }

            public void SetMacroSpeed(int wpm)
            {
                int clamped = clampMacroSpeed(wpm);

                lock (_lockObj)
                {
                    if (_activeOperation != null && _activeOperation.RestoreBaseSpeed)
                        _activeOperation.BaseSpeedWpm = clamped;
                }

                InvokeOnConsole(c => c.CWXForm.WPM = clamped);
            }

            private void SetMacroSpeedSilently(int wpm)
            {
                Interlocked.Increment(ref _server.m_cwInternalMacroSpeedUpdates);
                try
                {
                    InvokeOnConsole(c => c.CWXForm.WPM = clampMacroSpeed(wpm));
                }
                finally
                {
                    Interlocked.Decrement(ref _server.m_cwInternalMacroSpeedUpdates);
                }
            }

            public int GetMacroDelayMs()
            {
                return InvokeOnConsole(c => c.CWXForm.PTTDelayMs, 0);
            }

            public void SetMacroDelayMs(int delayMs)
            {
                InvokeOnConsole(c => c.CWXForm.PTTDelayMs = Math.Max(0, delayMs));
            }

            public int GetKeyerSpeed()
            {
                return InvokeOnConsole(c => c.CATCWSpeed, 30);
            }

            public void SetKeyerSpeed(int wpm)
            {
                InvokeOnConsole(c => c.CATCWSpeed = Math.Max(1, Math.Min(60, wpm)));
            }

            public void IncreaseMacroSpeed(int amount)
            {
                SetMacroSpeed(GetMacroSpeed() + Math.Max(0, amount));
            }

            public void DecreaseMacroSpeed(int amount)
            {
                SetMacroSpeed(GetMacroSpeed() - Math.Max(0, amount));
            }

            public void SetTerminalEnabled(TCPIPtciSocketListener owner, int rx, bool enabled)
            {
                lock (_lockObj)
                {
                    if (rx < 0 || rx > 1) return;

                    if (enabled)
                    {
                        if (!tryAcquireOwnershipLocked(owner)) return;
                    }
                    else if (!isCurrentOwnerLocked(owner))
                    {
                        return;
                    }

                    _terminalEnabledByRx[rx] = enabled;

                    if (enabled)
                    {
                        if (_activeOperation != null && _activeOperation.Rx == rx)
                            ensureTerminalTciPttLocked();
                        _releaseTerminalTciPttWhenIdle = false;
                    }
                    else if (_activeOperation != null && _activeOperation.Rx == rx)
                    {
                        _releaseTerminalTciPttWhenIdle = true;
                    }
                    else if (_activeOperation == null && _terminalTciPttAsserted && _terminalTciPttRx == rx)
                    {
                        releaseTerminalTciPttIfOwnedLocked();
                    }

                    releaseOwnershipIfIdleLocked();
                }
            }

            public void SendMacro(TCPIPtciSocketListener owner, int rx, string text)
            {
                lock (_lockObj)
                {
                    if (!isCwTargetAvailableLocked(rx)) return;
                    if (!tryAcquireOwnershipLocked(owner)) return;
                    if (_keyerPressed) return;

                    CWTxOperation operation = buildMacroOperation(rx, text);
                    operation.Owner = owner;
                    _pendingOperations.Enqueue(operation);
                    startNextOperationLocked();
                }
            }

            public void SendMessage(TCPIPtciSocketListener owner, int rx, string prefix, string callsign, string suffix)
            {
                lock (_lockObj)
                {
                    if (!isCwTargetAvailableLocked(rx)) return;
                    if (!tryAcquireOwnershipLocked(owner)) return;
                    if (_keyerPressed) return;

                    CWTxOperation operation = buildMessageOperation(rx, prefix, callsign, suffix);
                    operation.Owner = owner;
                    _pendingOperations.Enqueue(operation);
                    startNextOperationLocked();
                }
            }

            public void HandleKeyer(TCPIPtciSocketListener owner, int rx, bool pressed, int durationMs)
            {
                lock (_lockObj)
                {
                    if (pressed)
                    {
                        if (!tryAcquireOwnershipLocked(owner)) return;
                        if (_activeOperation != null || _pendingOperations.Count > 0) return;

                        if (_keyerReleasePending)
                            releaseKeyerLocked();

                        if (_keyerPressed)
                            return;

                        if (!selectCwTargetLocked(rx) || !isCWModeLocked())
                        {
                            releaseOwnershipIfIdleLocked();
                            return;
                        }

                        if (!beginDirectKeyerLocked())
                        {
                            releaseOwnershipIfIdleLocked();
                            return;
                        }

                        _keyerPressed = true;
                        _keyerReleasePending = false;
                        _keyerPressedAtTicks = _keyerStopwatch.ElapsedTicks;
                        _keyerReleaseAtTicks = -1;
                        _keyerRx = rx;
                        return;
                    }

                    if (!isCurrentOwnerLocked(owner)) return;
                    if (!_keyerPressed) return;

                    scheduleKeyerReleaseLocked(durationMs);
                }
            }

            public void UpdatePendingCallsign(TCPIPtciSocketListener owner, string callsign)
            {
                lock (_lockObj)
                {
                    if (!isCurrentOwnerLocked(owner)) return;
                    if (_activeOperation == null) return;
                    if (_activeOperation.CallsignSegmentIndex < 0) return;
                    if (_activeOperation.NextSegmentIndex > _activeOperation.CallsignSegmentIndex) return;

                    int repeatCount;
                    string callsignBase = parseCallsignBase(decodeTciText(callsign), out repeatCount);
                    if (string.IsNullOrWhiteSpace(callsignBase)) return;

                    _activeOperation.CallsignBase = callsignBase;
                    _activeOperation.Segments[_activeOperation.CallsignSegmentIndex].Text = buildRepeatedCallsign(callsignBase, repeatCount);
                }
            }

            public void OnRemoteCharacterStarted(int remainingRemoteCharacters, int pendingElements)
            {
                bool notifyEmpty = false;
                int rx = 0;

                lock (_lockObj)
                {
                    if (_activeOperation == null) return;
                    if (!isTerminalEnabledLocked(_activeOperation.Rx)) return;
                    if (_activeOperation.EmptyNotified) return;
                    if (_pendingOperations.Count > 0) return;
                    if (_activeOperation.NextSegmentIndex < _activeOperation.Segments.Count) return;
                    if (remainingRemoteCharacters > 0) return;
                    if (pendingElements <= 0) return;

                    _activeOperation.EmptyNotified = true;
                    rx = _activeOperation.Rx;
                    notifyEmpty = true;
                }

                if (notifyEmpty)
                    _server.OnCwMacrosEmpty(rx);
            }

            public void Stop(TCPIPtciSocketListener owner)
            {
                int restoreSpeed = -1;
                bool shouldReleaseDirectKeyerMox = false;
                bool shouldReleaseMox = false;
                bool stopKeyer = false;

                lock (_lockObj)
                {
                    if (!isCurrentOwnerLocked(owner)) return;

                    cancelKeyerReleaseScheduleLocked();
                    _pendingOperations.Clear();

                    if (_activeOperation != null && _activeOperation.RestoreBaseSpeed)
                        restoreSpeed = _activeOperation.BaseSpeedWpm;

                    _activeOperation = null;
                    stopKeyer = _keyerPressed || _keyerReleasePending;
                    _keyerPressed = false;
                    _keyerReleasePending = false;
                    _keyerPressedAtTicks = -1;
                    _keyerReleaseAtTicks = -1;
                    _keyerRx = -1;
                    shouldReleaseDirectKeyerMox = captureDirectKeyerMoxReleaseLocked();
                        shouldReleaseMox = true;

                    _releaseTerminalTciPttWhenIdle = false;
                    releaseOwnershipIfIdleLocked();
                }

                if (restoreSpeed > 0)
                    SetMacroSpeed(restoreSpeed);

                InvokeOnConsole(c => c.CWXForm.AbortSending());
                if (stopKeyer)
                    setDirectKeyerState(false);
                if (shouldReleaseDirectKeyerMox)
                    releaseDirectKeyerMox();

                if (shouldReleaseMox)
                {
                    lock (_lockObj)
                    {
                        releaseTerminalTciPttIfOwnedLocked();
                    }
                }
            }

            public void HandleTciPttReleased(TCPIPtciSocketListener owner)
            {
                lock (_lockObj)
                {
                    if (!isCurrentOwnerLocked(owner)) return;
                    if (_activeOperation != null || _pendingOperations.Count > 0) return;
                    if (!_keyerPressed && !_keyerReleasePending) return;

                    Debug.Print("TCI explicit trx:false releasing direct keyer state.");
                    releaseKeyerLocked();
                }
            }

            public void DisconnectClient(TCPIPtciSocketListener owner)
            {
                if (owner == null) return;

                lock (_lockObj)
                {
                    if (!isCurrentOwnerLocked(owner)) return;

                    for (int i = 0; i < _terminalEnabledByRx.Length; i++)
                        _terminalEnabledByRx[i] = false;

                    _releaseTerminalTciPttWhenIdle = false;
                }

                Stop(owner);
            }

            private static int clampMacroSpeed(int speed)
            {
                return Math.Max(1, Math.Min(99, speed));
            }

            private static string decodeTciText(string text)
            {
                if (string.IsNullOrEmpty(text)) return string.Empty;
                return text.Replace('^', ':').Replace('~', ',').Replace('*', ';');
            }

            private static string normalizeMessageField(string text)
            {
                text = decodeTciText(text);
                return text == "_" ? string.Empty : text;
            }

            private static string translateAbbreviationToken(string token)
            {
                string value = token;
                if (value == null)
                    value = string.Empty;

                switch (value.Trim().ToUpperInvariant())
                {
                    case "SK": return "*";
                    case "AR": return "+";
                    case "KN": return "(";
                    case "SN": return "!";
                    case "BT": return "=";
                    case "BK": return "\\";
                    case "AS": return "%";
                    default: return value;
                }
            }

            private static string buildRepeatedCallsign(string callsign, int repeatCount)
            {
                string tmp = callsign != null ? callsign : string.Empty;
                callsign = tmp.Trim();
                if (repeatCount < 2) return callsign;
                return string.Join(" ", Enumerable.Repeat(callsign, repeatCount));
            }

            private static string parseCallsignBase(string callsign, out int repeatCount)
            {
                repeatCount = 1;
                string tmp = callsign != null ? callsign : string.Empty;
                callsign = tmp.Trim();

                int dollarIndex = callsign.LastIndexOf('$');
                if (dollarIndex > 0 && dollarIndex < callsign.Length - 1 &&
                    int.TryParse(callsign.Substring(dollarIndex + 1), out int parsedRepeat))
                {
                    repeatCount = Math.Max(1, parsedRepeat);
                    callsign = callsign.Substring(0, dollarIndex).Trim();
                }

                return callsign;
            }

            private static CWTextParseResult parseMacroText(string text, int startingSpeed)
            {
                CWTextParseResult result = new CWTextParseResult()
                {
                    FinalSpeedWpm = clampMacroSpeed(startingSpeed)
                };

                if (string.IsNullOrEmpty(text))
                {
                    result.Segments.Add(new CWTxSegment() { Text = " ", SpeedWpm = result.FinalSpeedWpm });
                    return result;
                }

                StringBuilder current = new StringBuilder();

                Action flushCurrent = () =>
                {
                    if (current.Length < 1) return;

                    result.Segments.Add(new CWTxSegment()
                    {
                        Text = current.ToString(),
                        SpeedWpm = result.FinalSpeedWpm
                    });
                    current.Clear();
                };

                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];

                    if (ch == '|' && i + 1 < text.Length)
                    {
                        int end = text.IndexOf('|', i + 1);
                        if (end > i + 1)
                        {
                            current.Append(translateAbbreviationToken(text.Substring(i + 1, end - i - 1)));
                            i = end;
                            continue;
                        }
                    }

                    if (ch == '>' || ch == '<')
                    {
                        flushCurrent();
                        result.UsedInlineSpeedChanges = true;
                        result.FinalSpeedWpm = clampMacroSpeed(result.FinalSpeedWpm + (ch == '>' ? 5 : -5));
                        continue;
                    }

                    current.Append(ch);
                }

                flushCurrent();

                if (result.Segments.Count < 1)
                    result.Segments.Add(new CWTxSegment() { Text = " ", SpeedWpm = result.FinalSpeedWpm });

                return result;
            }

            private CWTxOperation buildMacroOperation(int rx, string text)
            {
                int baseSpeed = GetMacroSpeed();
                CWTextParseResult parsed = parseMacroText(decodeTciText(text), baseSpeed);
                CWTxOperation operation = new CWTxOperation()
                {
                    Rx = rx,
                    BaseSpeedWpm = baseSpeed,
                    RestoreBaseSpeed = parsed.UsedInlineSpeedChanges
                };
                operation.Segments.AddRange(parsed.Segments);
                return operation;
            }

            private CWTxOperation buildMessageOperation(int rx, string prefix, string callsign, string suffix)
            {
                int baseSpeed = GetMacroSpeed();
                prefix = normalizeMessageField(prefix);
                suffix = normalizeMessageField(suffix);
                callsign = normalizeMessageField(callsign);

                int repeatCount;
                string callsignBase = parseCallsignBase(callsign, out repeatCount);
                if (string.IsNullOrWhiteSpace(callsignBase))
                    callsignBase = "?";

                CWTextParseResult prefixParsed = parseMacroText(prefix, baseSpeed);
                CWTextParseResult suffixParsed = parseMacroText(suffix, prefixParsed.FinalSpeedWpm);

                CWTxOperation operation = new CWTxOperation()
                {
                    Rx = rx,
                    BaseSpeedWpm = baseSpeed,
                    RestoreBaseSpeed = prefixParsed.UsedInlineSpeedChanges || suffixParsed.UsedInlineSpeedChanges,
                    CallsignBase = callsignBase
                };

                if (!string.IsNullOrEmpty(prefix))
                    operation.Segments.AddRange(prefixParsed.Segments);

                operation.CallsignSegmentIndex = operation.Segments.Count;
                operation.Segments.Add(new CWTxSegment()
                {
                    Text = buildRepeatedCallsign(callsignBase, repeatCount),
                    SpeedWpm = prefixParsed.FinalSpeedWpm
                });

                if (!string.IsNullOrEmpty(suffix))
                    operation.Segments.AddRange(suffixParsed.Segments);

                return operation;
            }

            private void PollCallback(object state)
            {
                lock (_lockObj)
                {
                    if (_disposed) return;

                    if (_activeOperation != null && (!selectCwTargetLocked(_activeOperation.Rx) || !isCWModeLocked()))
                    {
                        abortOperationsForNonCWLocked();
                        return;
                    }

                    if ((_keyerPressed || _keyerReleasePending) &&
                        (_keyerRx < 0 || !selectCwTargetLocked(_keyerRx) || !isCWModeLocked()))
                    {
                        abortOperationsForNonCWLocked();
                        return;
                    }

                    if (tryReleaseDirectKeyerFromPollLocked())
                        return;

                    if (_activeOperation == null)
                    {
                        if (_releaseTerminalTciPttWhenIdle)
                            releaseTerminalTciPttIfOwnedLocked();

                        startNextOperationLocked();
                        return;
                    }

                    int pendingRemote = InvokeOnConsole(c => c.CWXForm.PendingRemoteCharacters, 0);
                    int pendingElements = InvokeOnConsole(c => c.CWXForm.Characters2Send, 0);
                    bool idle = pendingRemote <= 0 && pendingElements <= 0;

                    if (isTerminalEnabledLocked(_activeOperation.Rx) &&
                        !_activeOperation.EmptyNotified &&
                        _pendingOperations.Count < 1 &&
                        _activeOperation.NextSegmentIndex >= _activeOperation.Segments.Count &&
                        pendingRemote <= 0 &&
                        pendingElements > 0)
                    {
                        _activeOperation.EmptyNotified = true;
                        _server.OnCwMacrosEmpty(_activeOperation.Rx);
                    }

                    if (!idle) return;

                    if (_activeOperation.ActiveSegmentIndex == _activeOperation.CallsignSegmentIndex &&
                        !_activeOperation.CallsignNotified)
                    {
                        _activeOperation.CallsignNotified = true;
                        _server.OnCwCallsignSent(_activeOperation.CallsignBase);
                    }

                    if (_activeOperation.NextSegmentIndex < _activeOperation.Segments.Count)
                    {
                        queueNextSegmentLocked();
                    }
                    else
                    {
                        completeActiveOperationLocked();
                    }
                }
            }

            private void startNextOperationLocked()
            {
                if (_activeOperation != null || _pendingOperations.Count < 1 || _keyerPressed) return;

                CWTxOperation nextOperation = _pendingOperations.Peek();
                if (!selectCwTargetLocked(nextOperation.Rx) || !isCWModeLocked())
                {
                    abortOperationsForNonCWLocked();
                    return;
                }

                _activeOperation = _pendingOperations.Dequeue();
                if (_terminalTciPttAsserted &&
                    _terminalTciPttRx != _activeOperation.Rx &&
                    !isTerminalEnabledLocked(_activeOperation.Rx))
                {
                    releaseTerminalTciPttIfOwnedLocked();
                }

                ensureTerminalTciPttLocked();
                queueNextSegmentLocked();
            }

            private void queueNextSegmentLocked()
            {
                if (_activeOperation == null) return;
                if (_activeOperation.NextSegmentIndex >= _activeOperation.Segments.Count) return;
                if (!selectCwTargetLocked(_activeOperation.Rx) || !isCWModeLocked())
                {
                    abortOperationsForNonCWLocked();
                    return;
                }

                CWTxSegment segment = _activeOperation.Segments[_activeOperation.NextSegmentIndex];
                string text = string.IsNullOrEmpty(segment.Text) ? " " : segment.Text;

                ensureTerminalTciPttLocked();

                SetMacroSpeedSilently(segment.SpeedWpm);
                _activeOperation.ActiveSegmentIndex = _activeOperation.NextSegmentIndex;
                _activeOperation.NextSegmentIndex++;
                InvokeOnConsole(c =>
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(text);
                        c.CWXForm.RemoteMessage(bytes);
                });
            }

            private void completeActiveOperationLocked()
            {
                CWTxOperation completed = _activeOperation;
                _activeOperation = null;

                if (completed != null && completed.RestoreBaseSpeed)
                    SetMacroSpeedSilently(completed.BaseSpeedWpm);

                if (completed == null || !isTerminalEnabledLocked(completed.Rx) || _releaseTerminalTciPttWhenIdle)
                    releaseTerminalTciPttIfOwnedLocked();

                startNextOperationLocked();
                releaseOwnershipIfIdleLocked();
            }

            private bool isCWModeLocked()
            {
                DSPMode mode = InvokeOnConsole(c =>
                {
                    bool txOnRx2 = c.RX2Enabled && c.VFOBTX;
                    return txOnRx2 ? c.RX2DSPMode : c.RX1DSPMode;
                }, DSPMode.FIRST);
                return mode == DSPMode.CWL || mode == DSPMode.CWU;
            }

            private void abortOperationsForNonCWLocked()
            {
                int restoreSpeed = -1;

                cancelKeyerReleaseScheduleLocked();
                _pendingOperations.Clear();

                if (_activeOperation != null && _activeOperation.RestoreBaseSpeed)
                    restoreSpeed = _activeOperation.BaseSpeedWpm;

                _activeOperation = null;
                bool stopKeyer = _keyerPressed || _keyerReleasePending;
                _keyerPressed = false;
                _keyerReleasePending = false;
                _keyerPressedAtTicks = -1;
                _keyerReleaseAtTicks = -1;
                _keyerRx = -1;
                bool shouldReleaseDirectKeyerMox = captureDirectKeyerMoxReleaseLocked();

                if (restoreSpeed > 0)
                    SetMacroSpeedSilently(restoreSpeed);

                InvokeOnConsole(c => c.CWXForm.AbortSending());
                if (stopKeyer)
                    setDirectKeyerState(false);
                if (shouldReleaseDirectKeyerMox)
                    releaseDirectKeyerMox();
                releaseTerminalTciPttIfOwnedLocked();
                releaseOwnershipIfIdleLocked();
            }

            private bool isCurrentOwnerLocked(TCPIPtciSocketListener owner)
            {
                return owner == null || (_currentOwner != null && ReferenceEquals(_currentOwner, owner));
            }

            private bool tryAcquireOwnershipLocked(TCPIPtciSocketListener owner)
            {
                if (owner == null) return true;

                if (_currentOwner == null || ReferenceEquals(_currentOwner, owner))
                {
                    _currentOwner = owner;
                    return true;
                }

                return false;
            }

            private void releaseOwnershipIfIdleLocked()
            {
                if (!isAnyTerminalEnabledLocked() && _activeOperation == null && _pendingOperations.Count < 1 &&
                    !_keyerPressed && !_keyerReleasePending)
                    _currentOwner = null;
            }

            private void KeyerSchedulerThreadProc()
            {
                while (true)
                {
                    _keyerScheduleEvent.WaitOne();

                    while (true)
                    {
                        long releaseAtTicks;

                        lock (_lockObj)
                        {
                            if (_disposed) return;
                            if (!_keyerReleasePending || !_keyerPressed)
                                break;

                            releaseAtTicks = _keyerReleaseAtTicks;
                        }

                        if (!waitForScheduledKeyerRelease(releaseAtTicks))
                            continue;

                        lock (_lockObj)
                        {
                            if (_disposed) return;
                            if (!_keyerReleasePending || !_keyerPressed)
                                break;
                            if (_keyerReleaseAtTicks != releaseAtTicks)
                                continue;

                            releaseKeyerLocked();
                        }

                        break;
                    }
                }
            }

            private void scheduleKeyerReleaseLocked(int durationMs)
            {
                long desiredReleaseTicks = _keyerPressedAtTicks + millisecondsToStopwatchTicks(Math.Max(0, durationMs));
                long remainingTicks = desiredReleaseTicks - _keyerStopwatch.ElapsedTicks;

                if (remainingTicks <= 0)
                {
                    releaseKeyerLocked();
                    return;
                }

                _keyerReleasePending = true;
                _keyerReleaseAtTicks = desiredReleaseTicks;
                _keyerScheduleEvent.Set();
            }

            private bool tryReleaseDirectKeyerFromPollLocked()
            {
                if (!_keyerPressed) return false;

                long nowTicks = _keyerStopwatch.ElapsedTicks;

                if (_keyerReleasePending)
                {
                    if (_keyerReleaseAtTicks >= 0 && nowTicks >= _keyerReleaseAtTicks)
                    {
                        releaseKeyerLocked();
                        return true;
                    }

                    return false;
                }

                if (_keyerPressedAtTicks < 0)
                    return false;

                long watchdogAtTicks = _keyerPressedAtTicks + millisecondsToStopwatchTicks(DirectKeyerWatchdogMs);
                if (nowTicks < watchdogAtTicks)
                    return false;

                Debug.Print("TCI keyer watchdog releasing stale direct-key press.");
                releaseKeyerLocked();
                return true;
            }

            private void releaseKeyerLocked()
            {
                cancelKeyerReleaseScheduleLocked();

                if (!_keyerPressed && !_keyerReleasePending) return;

                _keyerPressed = false;
                _keyerReleasePending = false;
                _keyerPressedAtTicks = -1;
                _keyerReleaseAtTicks = -1;
                _keyerRx = -1;
                bool shouldReleaseDirectKeyerMox = captureDirectKeyerMoxReleaseLocked();

                setDirectKeyerState(false);
                if (shouldReleaseDirectKeyerMox)
                    releaseDirectKeyerMox();
                releaseOwnershipIfIdleLocked();
            }

            private void cancelKeyerReleaseScheduleLocked()
            {
                _keyerReleasePending = false;
                _keyerReleaseAtTicks = -1;
                _keyerScheduleEvent.Set();
            }

            private bool isCwTargetAvailableLocked(int rx)
            {
                if (rx != 1) return true;
                return InvokeOnConsole(c => c.RX2Enabled, false);
            }

            private bool selectCwTargetLocked(int rx)
            {
                return InvokeOnConsole(c =>
                {
                    if (rx == 1)
                    {
                        if (!c.RX2Enabled)
                            return false;

                        if (!c.VFOBTX)
                            c.VFOBTX = true;

                        return true;
                    }

                    if (c.RX2Enabled && c.VFOBTX)
                        c.VFOATX = true;

                    return true;
                }, false);
            }

            private void ensureTerminalTciPttLocked()
            {
                if (_activeOperation == null) return;
                if (!isTerminalEnabledLocked(_activeOperation.Rx)) return;
                if (_terminalTciPttAsserted)
                {
                    _terminalTciPttRx = _activeOperation.Rx;
                    return;
                }

                bool alreadyTciPtt = InvokeOnConsole(c => c.TCIPTT, false);
                if (!alreadyTciPtt)
                {
                    InvokeOnConsole(c => c.TCIPTT = true);
                    _terminalTciPttAsserted = true;
                    _terminalTciPttRx = _activeOperation.Rx;
                }
            }

            private void releaseTerminalTciPttIfOwnedLocked()
            {
                if (_terminalTciPttAsserted)
                    InvokeOnConsole(c => c.TCIPTT = false);

                _terminalTciPttAsserted = false;
                _terminalTciPttRx = -1;
                _releaseTerminalTciPttWhenIdle = false;
            }

            private bool isTerminalEnabledLocked(int rx)
            {
                return rx >= 0 && rx < _terminalEnabledByRx.Length && _terminalEnabledByRx[rx];
            }

            private bool isAnyTerminalEnabledLocked()
            {
                for (int i = 0; i < _terminalEnabledByRx.Length; i++)
                {
                    if (_terminalEnabledByRx[i])
                        return true;
                }

                return false;
            }

            private bool beginDirectKeyerLocked()
            {
                if (!InvokeOnConsole(c => !c.DisablePTT, false))
                    return false;

                if (!ensureDirectKeyerMoxLocked())
                    return false;

                if (setDirectKeyerState(true))
                    return true;

                bool shouldReleaseDirectKeyerMox = captureDirectKeyerMoxReleaseLocked();
                if (shouldReleaseDirectKeyerMox)
                    releaseDirectKeyerMox();

                return false;
            }

            private bool ensureDirectKeyerMoxLocked()
            {
                BreakIn breakInMode = InvokeOnConsole(c => c.CurrentBreakInMode, BreakIn.Manual);
                if (breakInMode != BreakIn.Semi)
                {
                    _keyerAssertedMox = false;
                    return true;
                }

                bool alreadyMox = InvokeOnConsole(c => c.MOX, false);
                if (alreadyMox)
                {
                    _keyerAssertedMox = false;
                    return true;
                }

                bool externalTciPtt = InvokeOnConsole(c => c.TCIPTT, false);
                bool moxActive = InvokeOnConsole(c =>
                {
                    if (!c.MOX)
                        c.MOX = true;
                    return c.MOX;
                }, false);

                _keyerAssertedMox = moxActive && !externalTciPtt;
                return moxActive;
            }

            private bool captureDirectKeyerMoxReleaseLocked()
            {
                bool releaseDirectKeyerMox = _keyerAssertedMox && !InvokeOnConsole(c => c.TCIPTT, false);
                _keyerAssertedMox = false;
                return releaseDirectKeyerMox;
            }

            private void releaseDirectKeyerMox()
            {
                InvokeOnConsole(c =>
                {
                    if (c.MOX)
                        c.MOX = false;
                    return 0;
                }, 0);
            }

            private bool waitForScheduledKeyerRelease(long releaseAtTicks)
            {
                while (true)
                {
                    long remainingTicks = releaseAtTicks - _keyerStopwatch.ElapsedTicks;
                    if (remainingTicks <= 0)
                        return true;

                    double remainingMs = stopwatchTicksToMilliseconds(remainingTicks);

                    if (remainingMs > 2.0)
                    {
                        int waitMs = Math.Max(1, (int)Math.Floor(remainingMs) - 1);
                        if (_keyerScheduleEvent.WaitOne(waitMs))
                            return false;
                    }
                    else
                    {
                        Thread.SpinWait(128);
                    }
                }
            }

            private static long millisecondsToStopwatchTicks(int durationMs)
            {
                if (durationMs <= 0) return 0;
                return (long)Math.Round((durationMs / 1000.0) * Stopwatch.Frequency, MidpointRounding.AwayFromZero);
            }

            private static double stopwatchTicksToMilliseconds(long ticks)
            {
                return (ticks * 1000.0) / Stopwatch.Frequency;
            }

            private static bool setDirectKeyerState(bool pressed)
            {
                try
                {
                    NetworkIO.SetCWX(pressed ? 1 : 0);
                    NetworkIO.SendHighPriority(1);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            private T InvokeOnConsole<T>(Func<Console, T> action, T defaultValue)
            {
                Console c = _server._console;
                if (c == null || c.IsDisposed) return defaultValue;

                try
                {
                    if (c.InvokeRequired)
                        return (T)c.Invoke(action, c);

                    return action(c);
                }
                catch
                {
                    return defaultValue;
                }
            }

            private void InvokeOnConsole(Action<Console> action)
            {
                Console c = _server._console;
                if (c == null || c.IsDisposed) return;

                try
                {
                    if (c.InvokeRequired)
                        c.Invoke(action, c);
                    else
                        action(c);
                }
                catch
                {
                }
            }
        }
        #endregion
    }
}
