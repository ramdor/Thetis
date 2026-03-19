/*  TCIServer.cs

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
//tx_frequency_thetis: 7070468,b40m,false,false;
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
        private sealed class clsReadingState
        {
            public double Value = -127.0;
            public bool Updated = false;
        }

        private sealed class clsTxReadingState
        {
            public double MicLevelDbm = -127.0;
            public double PowerWatts = 0.0;
            public double PeakPowerWatts = 0.0;
            public double Swr = 1.0;
            public bool Updated = false;
        }

        private readonly object _lock = new object();
        private readonly clsReadingState[,] _rxChannelReadings = new clsReadingState[2, 2];
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
                    _rxChannelReadings[rx, channel] = new clsReadingState();
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
                        clsReadingState state = _rxChannelReadings[rx, channel];
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

                clsReadingState state = _rxChannelReadings[receiver, channel];
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
                    case Reading.SIGNAL_STRENGTH:
                        {
                            int rx = receiver - 1;
                            if (rx < 0 || rx > 1) return false;
                            clsReadingState state = _rxChannelReadings[rx, 0];
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

        public void SetRxChannelReading(int receiver, int channel, double value)
        {
            lock (_lock)
            {
                if (receiver < 0 || receiver > 1 || channel < 0 || channel > 1) return;

                clsReadingState state = _rxChannelReadings[receiver, channel];
                state.Value = value;
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

        public bool TryGetRxChannelReadingForSend(int receiver, int channel, out double value)
        {
            lock (_lock)
            {
                value = -127.0;
                if (!_rxSensorsEnabled) return false;
                if (receiver < 0 || receiver > 1 || channel < 0 || channel > 1) return false;

                clsReadingState state = _rxChannelReadings[receiver, channel];
                if (!state.Updated) return false;

                value = state.Value;
                return true;
            }
        }

        public void ConsumeRxChannelReading(int receiver, int channel)
        {
            lock (_lock)
            {
                if (receiver < 0 || receiver > 1 || channel < 0 || channel > 1) return;

                clsReadingState state = _rxChannelReadings[receiver, channel];
                state.Updated = false;
            }
        }

        public bool TryGetTxReadingsForSend(out double micLevelDbm, out double powerWatts, out double peakPowerWatts, out double swr)
        {
            lock (_lock)
            {
                micLevelDbm = -127.0;
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
			m_stream = client.GetStream();
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
			sendTXEnable(1, enabled && !console.ThreadSafeTCIAccessor.MOX);
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

            if (readings.TryGetValue(Reading.SIGNAL_STRENGTH, out float signal))
            {
                m_sensorManager.SetRxChannelReading(receiver, 0, signal);
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
            rightOut = right ?? left;
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
			bool bSplit = console.ThreadSafeTCIAccessor.VFOSplit;
			sendSplit(rx-1, bSplit);
		}
        public void MuteChanged(int rx, bool newState)
        {
            if (m_disconnected) return;
			sendMute(console.ThreadSafeTCIAccessor.MUT || (console.ThreadSafeTCIAccessor.RX2Enabled && console.ThreadSafeTCIAccessor.MUT2));
			sendMuteRX(rx - 1, newState);
        }
        public void NrChanged(int rx, int newNR)
        {
            if (m_disconnected) return;
            bool enabled = newNR > 0;
            sendNrEnable(rx - 1, enabled, false, newNR);
            sendNrEnable(rx - 1, enabled, true, newNR);
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
					if (console.ThreadSafeTCIAccessor.RX2Enabled) sendTXEnable(1, false);
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
					if(console.ThreadSafeTCIAccessor.RX2Enabled) sendTXEnable(1, true);
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
			sendTXEnable(rx-1, rx == 1 ? true : console.ThreadSafeTCIAccessor.RX2Enabled); // MW0LGE_22b fixed, rx1 should be tx only, not rx2
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

		private static byte[] GetFrameFromString(string Message, EOpcodeType Opcode = EOpcodeType.Text)
		{
			byte[] response;
			byte[] bytesRaw = Encoding.Default.GetBytes(Message);
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
                case "modulation":
                case "drive":
                case "tune_drive":
                case "tune":
                case "rx_enable":
                case "tx_enable":
                    if (args.Length >= 1)
                        return command + ":" + args[0];
                    break;
                case "tx_frequency":
                case "tx_frequency_thetis":
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
                            m_server.LogForm.Log(false, outboundFrame.LogText);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("problem writing queued frame");
                    m_stopClient = true;
                }
            }
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
		private void sendVFO(int rx, int chan, long vfo = -1)
        {
			bool bVFOaUseRX2;
			if (m_server != null && console != null)
				bVFOaUseRX2 = console.ThreadSafeTCIAccessor.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
			else
				bVFOaUseRX2 = false;

			if (vfo == -1)
			{
				if (rx == 0)
				{
					if (chan == 0)
						vfo = (long)(console.ThreadSafeTCIAccessor.VFOAFreq * 1e6);
					else if (chan == 1)
						vfo = (long)(console.ThreadSafeTCIAccessor.VFOBFreq * 1e6);
				}
				else if (rx == 1)
				{
					if(chan == 0)
                    {
						if(bVFOaUseRX2)
							vfo = (long)(console.ThreadSafeTCIAccessor.VFOAFreq * 1e6);
						else
							vfo = (long)(console.ThreadSafeTCIAccessor.VFOBFreq * 1e6);
					}
                    else if (chan == 1)
                    {
						vfo = (long)(console.ThreadSafeTCIAccessor.VFOBFreq * 1e6);
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
						offset = (int)console.ThreadSafeTCIAccessor.radio.GetDSPRX(0, 0).RXOsc;
					}
					else if (chan == 1)
					{
						offset = (int)console.ThreadSafeTCIAccessor.radio.GetDSPRX(0, 1).RXOsc;
					}
					else offset = 0;
				}
				else if (rx == 1)
					offset = (int)console.ThreadSafeTCIAccessor.radio.GetDSPRX(1, 0).RXOsc;
			}

			offset += -console.ThreadSafeTCIAccessor.GetDSPcwPitchShiftToZero(rx + 1); //MW0LGE [2.9.0.7] note we invert with -

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
					mode = console.ThreadSafeTCIAccessor.RX1DSPMode;
				else if(rx == 1)
					mode = console.ThreadSafeTCIAccessor.RX2DSPMode;
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
		private void sendMONVolume(double volume)
		{
            if (volume < -60f || volume > 0f) return;

            string s = "mon_volume:" + volume.ToString("F1", CultureInfo.InvariantCulture).ToLower() + ";";
            sendTextFrame(s);
        }
		private void sendTXFrequencyChanged(long new_frequency, Band new_band, bool rx2_enabled, bool tx_vfob)
		{
            string s = $"tx_frequency:{new_frequency};";
            sendTextFrame(s.ToLower());

            // bespoke TCI command for anan to make life easier determining active TX frequency
            // format is : tx_frequency_thetis:3700000,b80m,false,false;
            // arg1 freq (long)
            // arg2 band b80m, b40m etc
            // arg3 rx2 enabled  true/false
            // arg4 tx on vfoB  true/false
            s = $"tx_frequency_thetis:{new_frequency},{new_band.ToString()},{rx2_enabled.ToString()},{tx_vfob.ToString()};";
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
		private void sendRxChannelSensors(int rx, int channel, double levelDbm)
		{
			sendTextFrame("rx_channel_sensors:" + rx.ToString() + "," + channel.ToString() + "," + levelDbm.ToString("F1", CultureInfo.InvariantCulture) + ";");
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
					ddsFreq = (long)(console.ThreadSafeTCIAccessor.CentreFrequency * 1e6);
				else if (rx == 1)
					ddsFreq = (long)(console.ThreadSafeTCIAccessor.CentreRX2Frequency * 1e6);
			}

			ddsFreq += console.ThreadSafeTCIAccessor.GetDSPcwPitchShiftToZero(rx+1); //MW0LGE [2.9.0.7]

            string s = "dds:" + rx.ToString() + "," + ddsFreq.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendFilterBand(int rx, int low, int high)
        {
			string s = "rx_filter_band:" + rx.ToString() + "," + low.ToString() + "," + high.ToString() + ";";
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

        private void sendInitialRadioState()
        {
			bool bSend = m_server != null ? m_server.SendInitialFrequencyStateOnConnect : true;
			bool bRX2Enabled = console.ThreadSafeTCIAccessor.RX2Enabled;

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
                sendTXFrequencyChanged((long)(console.ThreadSafeTCIAccessor.TXFreq * 1e6), console.ThreadSafeTCIAccessor.TXBand, console.ThreadSafeTCIAccessor.RX2Enabled, console.ThreadSafeTCIAccessor.VFOBTX);
            }

            sendMode(0);
			sendMode(1);

            sendFilterBand(0, console.ThreadSafeTCIAccessor.RX1FilterLow, console.ThreadSafeTCIAccessor.RX1FilterHigh);
            sendFilterBand(1, console.ThreadSafeTCIAccessor.RX2FilterLow, console.ThreadSafeTCIAccessor.RX2FilterHigh);

            sendRXEnable(0, !console.ThreadSafeTCIAccessor.MOX);
			sendRXEnable(1, bRX2Enabled && !console.ThreadSafeTCIAccessor.MOX);

            int rx1nr = console.ThreadSafeTCIAccessor.GetSelectedNR(1);
            int rx2nr = console.ThreadSafeTCIAccessor.GetSelectedNR(2);
            sendNrEnable(0, rx1nr > 0, false, rx1nr);
            sendNrEnable(1, rx2nr > 0, false, rx2nr);
            sendNrEnable(0, rx1nr > 0, true, rx1nr);
            sendNrEnable(1, rx2nr > 0, true, rx2nr);

            sendAnfEnable(0, console.ThreadSafeTCIAccessor.GetANF(1));
            sendAnfEnable(1, console.ThreadSafeTCIAccessor.GetANF(2));

            double rx1vol = audioGainToDb(console.ThreadSafeTCIAccessor.RX0Gain / 100f);
            double rx1Subvol = audioGainToDb(console.ThreadSafeTCIAccessor.RX1Gain / 100f);
            double rx2vol = audioGainToDb(console.ThreadSafeTCIAccessor.RX2Gain / 100f);

            sendRxVolume(0, 0, rx1vol);
            sendRxVolume(0, 1, rx1Subvol);
            sendRxVolume(1, 0, rx2vol);
            sendRxVolume(1, 1, rx2vol);

            sendCTUN(0, console.ThreadSafeTCIAccessor.GetCTUN(1));
            sendCTUN(1, console.ThreadSafeTCIAccessor.GetCTUN(2));

            sendTXProfiles();
            sendTXProfile(console.ThreadSafeTCIAccessor.TXProfile);
            //lock
            //TODO rx channel enable
            //rit/xit

            sendSplit(0, console.ThreadSafeTCIAccessor.VFOSplit);
			sendSplit(1, bRX2Enabled && console.ThreadSafeTCIAccessor.VFOSplit);

			sendTXEnable(0, !console.ThreadSafeTCIAccessor.MOX);
			sendTXEnable(1, bRX2Enabled && !console.ThreadSafeTCIAccessor.MOX);

            sendRxChannelEnable(0, 0, true);
            sendRxChannelEnable(0, 1, console.ThreadSafeTCIAccessor.GetSubRX(1));
            sendRxChannelEnable(1, 0, bRX2Enabled);
			sendRxChannelEnable(1, 1, false); // no sub rx on rx2

            sendMOX(0, console.ThreadSafeTCIAccessor.MOX && !(console.ThreadSafeTCIAccessor.VFOBTX && bRX2Enabled));
            sendMOX(1, console.ThreadSafeTCIAccessor.MOX && (console.ThreadSafeTCIAccessor.VFOBTX && bRX2Enabled));

            sendTune(0, console.ThreadSafeTCIAccessor.TUN && !(console.ThreadSafeTCIAccessor.VFOBTX && bRX2Enabled));
            sendTune(1, console.ThreadSafeTCIAccessor.TUN && (console.ThreadSafeTCIAccessor.VFOBTX && bRX2Enabled));

            sendIQStartStop(0, false);
            sendIQStartStop(1, false);

            sendIQSampleRate(getPublishedIQSampleRate());
			sendAudioSampleRate(m_audioSampleRate);
			sendAudioStreamSampleType(m_audioSampleType);
			sendAudioStreamChannels(m_audioStreamChannels);
			sendAudioStreamSamples(m_audioStreamSamples);
			sendTxStreamAudioBuffering(m_txStreamAudioBufferingMs);

			sendMute(console.ThreadSafeTCIAccessor.MUT || (console.ThreadSafeTCIAccessor.MUT2 && bRX2Enabled));
			sendMuteRX(0, console.ThreadSafeTCIAccessor.MUT);
            sendMuteRX(1, console.ThreadSafeTCIAccessor.MUT2);

			sendMONEnable(console.ThreadSafeTCIAccessor.MON);
            sendMONVolume(linearToDbVolume(console.ThreadSafeTCIAccessor.TXAF));

            sendStartStop(console.ThreadSafeTCIAccessor.PowerOn);// MW0LGE_22b moved here to replicate sun

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

			sendVFOLimits(0, (int)(console.ThreadSafeTCIAccessor.MaxFreq * 1e6));

			int halfSample = console.ThreadSafeTCIAccessor.SampleRateRX1 / 2;
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
                rx2Enabled = console.ThreadSafeTCIAccessor.RX2Enabled;
                rx1SubEnabled = console.ThreadSafeTCIAccessor.GetSubRX(1);
            }
            catch
            { }

			if (m_sensorManager.TryGetRxChannelReadingForSend(0, 0, out double rx1Main))
			{
			    sendRxSensors(0, rx1Main);
			    sendRxChannelSensors(0, 0, rx1Main);
			    m_sensorManager.ConsumeRxChannelReading(0, 0);
		    }

			if (rx1SubEnabled && m_sensorManager.TryGetRxChannelReadingForSend(0, 1, out double rx0Sub))
		    {
				sendRxChannelSensors(0, 1, rx0Sub);
				m_sensorManager.ConsumeRxChannelReading(0, 1);
			}

			if (rx2Enabled && m_sensorManager.TryGetRxChannelReadingForSend(1, 0, out double rx2Main))
			{
				sendRxSensors(1, rx2Main);
				sendRxChannelSensors(1, 0, rx2Main);
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
			for (int i = 0; i < bytes.Length; i++)
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

			Debug.Print("TCPIP TCI Client Connected !");
			ClientConnectedHandlers?.Invoke();

			//SendClientData("# Thetis TCP/IP TCI #" + Environment.NewLine);

			while (!m_stopClient)
			{
				try
				{
					if (m_stream != null && m_stream.DataAvailable)
					{
						byte[] bytes = new byte[m_client.Available];
						int nRead = m_stream.Read(bytes, 0, bytes.Length);

						if (nRead > 0)
						{
							int nStart = 0;

							if (!m_bWebSocket)
							{
                                string msg = Encoding.UTF8.GetString(bytes);
                                if (Regex.IsMatch(msg, "^GET", RegexOptions.IgnoreCase))
								{

									if (upgradeToWebSocket(msg))
									{
										m_bWebSocket = true;
										Debug.Print("Upgraded to websocket");

										nStart = findEndOfHeader(bytes);

										// move rest of bytes if any to the buffer
										_m_buffer.Clear();
                                        _m_buffer.AddRange(bytes.Skip(nStart).Take(nRead - nStart));
                                        nRead = 0; // so that we dont re-add these below

										sendInitialisationData();
									}
									else
									{
										Debug.Print("Not Upgraded to websocket");
										m_stopClient = true;
									}
								}
							}

							if (m_bWebSocket)
							{
                                // add new bytes to buffer
                                _m_buffer.AddRange(bytes.Take(nRead));

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
                        else
                        {
							m_stopClient = true;
						}
					}
                    else
                    {
                        if (!m_client.Connected)
							m_stopClient = true;
						else
							Thread.Sleep(50);
                    }
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
			ClientDisconnectedHandlers?.Invoke();
		}
		private void sendPingFrame(string sMsg)
		{
			try
			{
				if (!m_stopClient && !m_disconnected && m_bWebSocket && m_client != null && m_stream != null)
				{
					if (m_client.Connected)
					{
                        enqueueOutboundFrame(GetFrameFromString(sMsg, EOpcodeType.Ping), null, TCIOutboundPriority.Urgent);
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
                        enqueueOutboundFrame(GetFrameFromString(sMsg, EOpcodeType.Pong), null, TCIOutboundPriority.Urgent);
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
                        enqueueOutboundFrame(
                            GetFrameFromString(sMsg, EOpcodeType.Text),
                            sMsg,
                            TCIOutboundPriority.Control,
                            getCoalescedTextFrameKey(sMsg));
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
                        enqueueOutboundFrame(GetFrameFromString("", EOpcodeType.ClosedConnection), null, TCIOutboundPriority.Urgent);
					}
				}
			}
            catch
            {

            }
		}
		public void StopSocketListener()
		{
			TCPIPtciServer server = m_server;
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
			console.ThreadSafeTCIAccessor.Focus();
        }
		private void handleStart()
        {
			if(!console.ThreadSafeTCIAccessor.PowerOn)
				console.ThreadSafeTCIAccessor.PowerOn = true;
        }
		private void handleStop()
		{
            if (console.ThreadSafeTCIAccessor.PowerOn)
				console.ThreadSafeTCIAccessor.PowerOn = false;
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
					if (!console.ThreadSafeTCIAccessor.IsSetupFormNull)
					{
                        if (console.ThreadSafeTCIAccessor.SetupForm.SplitFromCATorTCIcancelsQSPLIT)
                        {
                            if (console.ThreadSafeTCIAccessor.SetupForm.QuickSplitEnabled)
                                console.ThreadSafeTCIAccessor.SetupForm.QuickSplitEnabled = false;
                        }
                    }
					if (rx == 0 || rx == 1)
						if(console.ThreadSafeTCIAccessor.VFOSplit != bSplit)
							console.ThreadSafeTCIAccessor.VFOSplit = bSplit;
				}
			}
			else if(args.Length == 1)
			{
				// get
				if (bOK)
				{
					bool bSplitGet = console.ThreadSafeTCIAccessor.VFOSplit;
					sendSplit(rx, bSplitGet);
				}
			}
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
                bool useTciAudio = args.Length > 2 && args[2].ToLower() == "tci";
                bool alreadyMox = console.ThreadSafeTCIAccessor.MOX;
                bool alreadyActiveTciPtt;
                lock (m_objStreamLock)
                {
                    alreadyActiveTciPtt = m_tciPttActive;
                }

                bool wantsActiveTciPtt = useTciAudio && bOK && bMox && (!alreadyMox || alreadyActiveTciPtt);
                bool ownsActiveTciPtt = false;

                if (wantsActiveTciPtt)
                    ownsActiveTciPtt = m_server?.TryAcquireActiveTxAudioListener(this) ?? true;
                else
                    m_server?.ReleaseActiveTxAudioListener(this);

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
						if (console.ThreadSafeTCIAccessor.RX2Enabled && console.ThreadSafeTCIAccessor.VFOBTX)
							console.ThreadSafeTCIAccessor.VFOATX = true;

						if (console.ThreadSafeTCIAccessor.MOX != bMox)
							console.ThreadSafeTCIAccessor.TCIPTT = bMox;
					}
                    else if (rx == 1 && console.ThreadSafeTCIAccessor.RX2Enabled)
                    {
						if (!console.ThreadSafeTCIAccessor.VFOBTX)
							console.ThreadSafeTCIAccessor.VFOBTX = true;

						if (console.ThreadSafeTCIAccessor.MOX != bMox)
							console.ThreadSafeTCIAccessor.TCIPTT = bMox;
					}
				}

				m_server?.RefreshTxAudioSourceState();
				m_server?.RefreshStreamRunState();
			}
			else if (bOK && args.Length == 1)
            {
				sendMOX(rx, console.ThreadSafeTCIAccessor.MOX, m_txUsesTCIAudio);
            }
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
						vfo = console.ThreadSafeTCIAccessor.CentreFrequency + dIF;
						vfo = Math.Round(vfo, 6);
						if (chan == 0)
						{
							if (console.ThreadSafeTCIAccessor.VFOAFreq != vfo)
								console.ThreadSafeTCIAccessor.VFOAFreq = vfo;
						}
						else if (chan == 1)
						{
							if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
								console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
						}
					}
					else if (rx == 1)
					{
						if (console.ThreadSafeTCIAccessor.RX2Enabled)
						{
							vfo = console.ThreadSafeTCIAccessor.CentreRX2Frequency + dIF;
							vfo = Math.Round(vfo, 6);
							if (chan == 0)
							{
								if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
									console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
							}
							else if (chan == 1)
							{
								if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
									console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
							}
						}
					}
				}
			}
			else if (args.Length == 2)
			{
				bool bVFOaUseRX2;
				if (m_server != null && console != null)
					bVFOaUseRX2 = console.ThreadSafeTCIAccessor.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
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
							dIF = console.ThreadSafeTCIAccessor.VFOAFreq - console.ThreadSafeTCIAccessor.CentreFrequency;
						}
						else if (chan == 1)
						{
							dIF = console.ThreadSafeTCIAccessor.VFOBFreq - console.ThreadSafeTCIAccessor.CentreFrequency;
						}
					}
					else if (rx == 1)
					{
						if (chan == 0)
						{
							if(bVFOaUseRX2)
								dIF = console.ThreadSafeTCIAccessor.VFOAFreq - console.ThreadSafeTCIAccessor.CentreFrequency;
							else
								dIF = console.ThreadSafeTCIAccessor.VFOBFreq - console.ThreadSafeTCIAccessor.CentreRX2Frequency;
						}
						else
						{
							dIF = console.ThreadSafeTCIAccessor.VFOBFreq - console.ThreadSafeTCIAccessor.CentreRX2Frequency;
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
						double c = dds - console.ThreadSafeTCIAccessor.CentreFrequency;
						c = Math.Round(c, 6);
						console.ThreadSafeTCIAccessor.CentreFrequency = dds;
						console.ThreadSafeTCIAccessor.VFOAFreq += c;
					}
					else if (rx == 1)
					{
						double c = dds - console.ThreadSafeTCIAccessor.CentreRX2Frequency;
						c = Math.Round(c, 6);
						console.ThreadSafeTCIAccessor.CentreRX2Frequency = dds;
						console.ThreadSafeTCIAccessor.VFOBFreq += c;
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
						dds = console.ThreadSafeTCIAccessor.CentreFrequency;
					}
					else if (rx == 1)
					{
						dds = console.ThreadSafeTCIAccessor.CentreRX2Frequency;
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
			if (m_server != null && console != null)
				bVFOaUseRX2 = console.ThreadSafeTCIAccessor.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
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
							if (console.ThreadSafeTCIAccessor.VFOAFreq != vfo)
								console.ThreadSafeTCIAccessor.VFOAFreq = vfo;
						}
						else if (chan == 1)
						{
							if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
								console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
						}
					}
					else if (rx == 1)
					{
						if (console.ThreadSafeTCIAccessor.RX2Enabled)
						{
							if (chan == 0)
							{
								if (bVFOaUseRX2)
								{
									if (console.ThreadSafeTCIAccessor.VFOAFreq != vfo)
										console.ThreadSafeTCIAccessor.VFOAFreq = vfo;
								}
                                else
                                {
									if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
										console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
								}
							}
							else if (chan == 1)
							{
								if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
									console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
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
							vfo = console.ThreadSafeTCIAccessor.VFOAFreq;
						}
						else if (chan == 1)
						{
							vfo = console.ThreadSafeTCIAccessor.VFOBFreq;
						}
					}
					else if (rx == 1)
					{
						if (chan == 0)
						{
							if (bVFOaUseRX2)
								vfo = console.ThreadSafeTCIAccessor.VFOAFreq;
							else
								vfo = console.ThreadSafeTCIAccessor.VFOBFreq;
						}
						else
						{
							vfo = console.ThreadSafeTCIAccessor.VFOBFreq;
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
                            if (m_server != null && console != null)
							{
								if(m_server.CWbecomesCWUabove10mhz)
								{
									bool bVFOA10orAbove = console.ThreadSafeTCIAccessor.VFOAFreq >= 10.0;
                                    bool bVFOB10orAbove = console.ThreadSafeTCIAccessor.VFOBFreq >= 10.0;
									
                                    if (rx == 0)
									{
										if(console.ThreadSafeTCIAccessor.VFOATX)
                                            bChange = bVFOA10orAbove;
										else
                                            bChange = bVFOB10orAbove;
                                    }
                                    else if (rx == 1)
									{
										if (console.ThreadSafeTCIAccessor.VFOBTX)
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
							if(console.ThreadSafeTCIAccessor.RX1DSPMode != mode)
								console.ThreadSafeTCIAccessor.RX1DSPMode = mode;
						}
						else if (rx == 1)
						{
							if(console.ThreadSafeTCIAccessor.RX2DSPMode != mode)
								console.ThreadSafeTCIAccessor.RX2DSPMode = mode;
						}
					}
				}
			}
            else if (bOK && args.Length == 1)
            {
                //query
                if (rx == 0)
                {
					sendMode(rx, console.ThreadSafeTCIAccessor.RX1DSPMode);
                }
				else if(rx == 1)
                {
					sendMode(rx, console.ThreadSafeTCIAccessor.RX2DSPMode);
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
                    console.ThreadSafeTCIAccessor.Invoke(new MethodInvoker(() =>
                    {
						if (console.ThreadSafeTCIAccessor.SetupForm.VACEnable != enable)
						{
							console.ThreadSafeTCIAccessor.SetupForm.VACEnable = enable;
						}
                    }));
                    break;
				case 1:
                    console.ThreadSafeTCIAccessor.Invoke(new MethodInvoker(() =>
                    {
						if (console.ThreadSafeTCIAccessor.SetupForm.VAC2Enable != enable)
						{
							console.ThreadSafeTCIAccessor.SetupForm.VAC2Enable = enable;
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

                if (bOK && !console.ThreadSafeTCIAccessor.IsSetupFormNull)
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

                if (bOK && !console.ThreadSafeTCIAccessor.IsSetupFormNull)
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
					console.ThreadSafeTCIAccessor.PWR = pwr;
                }
			}
			else if (bOK && args.Length == 1)
            {
				//read
				int pwr;
				if (console.SendLimitedPowerLevels)
					pwr = console.ThreadSafeTCIAccessor.PWRConstrained;
				else
					pwr = console.ThreadSafeTCIAccessor.PWR;

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
					console.ThreadSafeTCIAccessor.TunePWR = pwr;
				}
			}
			else if (bOK && args.Length == 1)
			{
				//read
				sendTunePower(rx, console.ThreadSafeTCIAccessor.TunePWRConstrained);
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
					console.ThreadSafeTCIAccessor.MUT = mute;
					console.ThreadSafeTCIAccessor.MUT2 = mute;
                }
            }
            else if (!hasArgs)
            {
                //read
                sendMute(console.ThreadSafeTCIAccessor.MUT || console.ThreadSafeTCIAccessor.MUT2);
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
                        console.ThreadSafeTCIAccessor.MUT = mute;
                    else if(rx == 1)
                        console.ThreadSafeTCIAccessor.MUT2 = mute;
                }
            }
            else if (bOK && args.Length == 1)
            {
                //read
                sendMuteRX(rx, rx == 0 ? console.ThreadSafeTCIAccessor.MUT : console.ThreadSafeTCIAccessor.MUT2);
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
					console.ThreadSafeTCIAccessor.MON = enable;
                }
            }
            else if (!hasArgs)
            {
				//read
				sendMONEnable(console.ThreadSafeTCIAccessor.MON);
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
		private void handleMONVolume(string[] args, bool hasArgs = true)
		{
            if (hasArgs && args.Length == 1)
            {
                //set
                bool bOK = double.TryParse(args[0], out double dBLevel);
                if (bOK)
                {
                    double dbMin = -60f;
                    double dbMax = 0;
                    double linearMax = 100f;
                    double linearMin = 0;
                    double linearValue = ((dBLevel - dbMin) / (dbMax - dbMin)) * (linearMax - linearMin) + linearMin;
                    linearValue = Math.Max(linearMin, Math.Min(linearMax, linearValue));

                    console.ThreadSafeTCIAccessor.TXAF = (int)linearValue;
                }
            }
            else if (!hasArgs)
            {
				//read
                sendMONVolume(linearToDbVolume(console.ThreadSafeTCIAccessor.TXAF));
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
					if(console.ThreadSafeTCIAccessor.TUN != tune)
						console.ThreadSafeTCIAccessor.TUN = tune;
                }
			}
            else if(args.Length == 1)
            {
				//query
				sendTune(rx, console.ThreadSafeTCIAccessor.TUN);
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
						low = console.ThreadSafeTCIAccessor.RX1FilterLow;
						high = console.ThreadSafeTCIAccessor.RX1FilterHigh;
                        break;
                    case 1:
						low = console.ThreadSafeTCIAccessor.RX2FilterLow;
						high = console.ThreadSafeTCIAccessor.RX2FilterHigh;
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
                            console.ThreadSafeTCIAccessor.UpdateRX1Filters(low, high);
                            break;
						case 1:
                            console.ThreadSafeTCIAccessor.UpdateRX2Filters(low, high);
                            break;
					}
				}
            }
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
						if (console.ThreadSafeTCIAccessor.RX2Enabled != enable)
							console.ThreadSafeTCIAccessor.RX2Enabled = enable;
					}
				}
			}
			else if (bOK && args.Length == 1)
			{
				//query
				if (rx == 0)
				{
					sendRXEnable(rx, !console.ThreadSafeTCIAccessor.MOX);
				}
                else if(rx == 1)
                {
					sendRXEnable(rx, console.ThreadSafeTCIAccessor.RX2Enabled && !console.ThreadSafeTCIAccessor.MOX);
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
        private void sendNrEnable(int rx, bool enabled, bool is_extended, int nr)
        {
            string s;
            if(is_extended)
                s = "rx_nr_enable_ex:" + rx.ToString() + "," + enabled.ToString().ToLower() + "," + nr.ToString() + ";";
            else
                s = "rx_nr_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";

            sendTextFrame(s);
        }
        private void sendAnfEnable(int rx, bool enabled)
        {
            string s = "rx_anf_enable:" + rx.ToString() + "," + enabled.ToString().ToLower() + ";";

            sendTextFrame(s);
        }
        private void handleNrEnable(string[] args, bool is_extended)
        {
            if (args == null || args.Length < 1 || is_extended && args.Length < 3) return;
            if (!int.TryParse(args[0], out int rx)) return;
            if (rx < 0 || rx > 1) return;

            int nr = 1;
            bool enable = false;
            if (args.Length == 1)
            {
                //get
                nr = console.ThreadSafeTCIAccessor.GetSelectedNR(rx + 1);
                enable = nr > 0;
                sendNrEnable(rx, enable, false, nr);
                sendNrEnable(rx, enable, true, nr);
            }
            else
            {
                //set
                if (!bool.TryParse(args[1], out enable)) return;
                if (is_extended && !int.TryParse(args[2], out nr)) return;
                if (nr < 1 || nr > 4) return;

                if (enable)
                {
                    console.ThreadSafeTCIAccessor.SelectNR(rx + 1, false, is_extended ? nr : 1);
                }
                else
                {
                    console.ThreadSafeTCIAccessor.SelectNR(rx + 1, false, 0);
                }                
            }
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
                enable = console.ThreadSafeTCIAccessor.GetANF(rx + 1);
                sendAnfEnable(rx, enable);
            }
            else
            {
                //set
                if (!bool.TryParse(args[1], out enable)) return;

                console.ThreadSafeTCIAccessor.SetANF(rx + 1, enable);                
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
                            vol = console.ThreadSafeTCIAccessor.RX0Gain; // such horrible naming
                        }
                        else if (chan == 1)
                        {
                            vol = console.ThreadSafeTCIAccessor.RX1Gain;
                        }
                        else
                            return;
                        break;
                    case 2:
                        vol = console.ThreadSafeTCIAccessor.RX2Gain; // no sub
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
                            console.ThreadSafeTCIAccessor.RX0Gain = (int)ag;
                        }
                        else if (chan == 1)
                        {
                            console.ThreadSafeTCIAccessor.RX1Gain = (int)ag;
                        }
                        else
                            return;
                        break;
                    case 2:
                        console.ThreadSafeTCIAccessor.RX2Gain = (int)ag;
                        break;
                }
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
                enable = console.ThreadSafeTCIAccessor.GetCTUN(rx + 1);
                sendCTUN(rx, enable);
            }
            else
            {
                //set
                if (!bool.TryParse(args[1], out enable)) return;
                console.ThreadSafeTCIAccessor.SetCTUN(rx + 1, enable);
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
            if (console == null || console.IsSetupFormNull) return;

            string[] profiles = console.ThreadSafeTCIAccessor.SetupForm.GetTXProfileStrings();
            string formatted_profiles = string.Join(",", profiles);
            string s = "tx_profiles_ex:" + formatted_profiles + ";";

            sendTextFrame(s);
        }
        private void handleTXProfile(string[] args)
        {
            if (args == null || args.Length > 1) return;

            if (args.Length == 0)
            {
                //get current tcprofille
                string prof = console.ThreadSafeTCIAccessor.TXProfile;
                sendTXProfile(prof);
            }
            else
            {
                //set
                console.ThreadSafeTCIAccessor.SafeTXProfileSet(args[1]);
            }
        }
        private void handleTXProfiles()
        {
            sendTXProfiles();
        }
        private void handleShutdown()
        {
            if (console.ThreadSafeTCIAccessor.InvokeRequired)
            {
                console.ThreadSafeTCIAccessor.BeginInvoke(new MethodInvoker(() =>
                {
                    console.ThreadSafeTCIAccessor.Close();
                }));
            }
            else
            {
                console.ThreadSafeTCIAccessor.Close();
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
                        handleNrEnable(args, false);
                        break;
                    case "rx_nr_enable_ex":
                        handleNrEnable(args, true);
                        break;
                    case "rx_anf_enable":
                        handleAnfEnable(args);
                        break;
                    case "rx_volume":
                        handleRxVolume(args);
                        break;
                    case "rx_ctun_ex":
                        handleCTUN(args); // bespoke thetis cmd for ctun
                        break;
                    case "tx_profile_ex":
                        handleTXProfile(args); // bespoke thetis cmd to select tx profile
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
                    case "set_in_focus":
                        handleSetInFocus();
                        break;
                    case "mute":
                        handleMute(null, false);
                        break;
                    case "mon_enable":
                        handleMONEnable(null, false);
                        break;
                    case "mon_volume":
                        handleMONVolume(null, false);
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
            Console localConsole = console;
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
                        if (localConsole.ThreadSafeTCIAccessor.RX2Enabled && localConsole.SetupForm.GetHWSampleRate(2) != sampleRate)
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
						enabled = channel == 0 ? true : console.ThreadSafeTCIAccessor.GetSubRX(1);
                        break;
					case 1:
						//just return rx2 state as no subrx
						enabled = channel == 0 ? console.ThreadSafeTCIAccessor.RX2Enabled : false;
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
					console.ThreadSafeTCIAccessor.SetSubRX(1, enabled);
				}
				else if(receiver == 1) // main or sub will set state
				{
					console.ThreadSafeTCIAccessor.RX2Enabled = enabled;
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
		private bool m_bEmulateExpertSDR3Protocol = false;
        private bool m_bIQSwap = true;
        private bool m_bAlwaysStreamIQ = false;
        private TCITxStereoInputMode m_txStereoInputMode = TCITxStereoInputMode.Both;

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
            set { m_bAlwaysStreamIQ = value; RefreshStreamRunState(); }
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
                }

				_console = c;

				m_socketListenersList = new List<TCPIPtciSocketListener>();
                cmaster.SetTCIRun(0);

                if (console != null && !m_bDelegatesAdded)
				{
					console.ThreadSafeTCIAccessor.VFOAFrequencyChangeHandlers += OnVFOAFrequencyChangeHandler;
					console.ThreadSafeTCIAccessor.VFOBFrequencyChangeHandlers += OnVFOBFrequencyChangeHandler;
					console.ThreadSafeTCIAccessor.MoxChangeHandlers += OnMoxChangeHandler;
                    console.ThreadSafeTCIAccessor.MoxPreChangeHandlers += OnMoxPreChangeHandler;
					console.ThreadSafeTCIAccessor.ModeChangeHandlers += OnModeChangeHandler;
					console.ThreadSafeTCIAccessor.BandChangeHandlers += OnBandChangeHandler;
					console.ThreadSafeTCIAccessor.CentreFrequencyHandlers += OnCentreFrequencyChanged;
					console.ThreadSafeTCIAccessor.FilterChangedHandlers += OnFilterChanged;
					console.ThreadSafeTCIAccessor.FilterEdgesChangedHandlers += OnFilterEdgesChanged;
					console.ThreadSafeTCIAccessor.PowerChangeHanders += OnPowerChangeHander;
					console.ThreadSafeTCIAccessor.SplitChangedHandlers += OnSplitChanged;
					console.ThreadSafeTCIAccessor.TuneChangedHandlers += OnTuneChanged;
					console.ThreadSafeTCIAccessor.DrivePowerChangedHandlers += OnDrivePowerChanged;
					console.ThreadSafeTCIAccessor.HWSampleRateChangedHandlers += OnHWSampleRateChanged;
					console.ThreadSafeTCIAccessor.ThetisFocusChangedHandlers += OnThetisFocusChanged;
					console.ThreadSafeTCIAccessor.RX2EnabledChangedHandlers += OnRX2EnabledChanged;
					console.ThreadSafeTCIAccessor.SpotClickedHandlers += OnSpotClicked;
					console.ThreadSafeTCIAccessor.MuteChangedHandlers += OnMuteChanged;
					console.ThreadSafeTCIAccessor.MONChangedHandlers += OnMONChanged;
                    console.ThreadSafeTCIAccessor.MONVolumeChangedHandlers += OnMONVolumeChanged;
					console.ThreadSafeTCIAccessor.TXFrequncyChangedHandlers += OnTXFrequencyChanged;
                    console.ThreadSafeTCIAccessor.MeterReadingsChangedHandlers += OnMeterReadingsChanged;
                    console.ThreadSafeTCIAccessor.NRChangedHandlers += OnNrChanged;
                    console.ThreadSafeTCIAccessor.ANFChangedHandlers += OnAnfChanged;
                    console.ThreadSafeTCIAccessor.RXGainChangedHandlers += OnRxAfGainChanged;
                    console.ThreadSafeTCIAccessor.CTUNChangedHandlers += OnCTUNChanged;
                    console.ThreadSafeTCIAccessor.TXProfileChangedHandlers += OnTXProfileChanged;
                    console.ThreadSafeTCIAccessor.TXProfilesChangedHandlers += OnTXProfilesChanged;

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
					console.ThreadSafeTCIAccessor.VFOAFrequencyChangeHandlers -= OnVFOAFrequencyChangeHandler;
					console.ThreadSafeTCIAccessor.VFOBFrequencyChangeHandlers -= OnVFOBFrequencyChangeHandler;
					console.ThreadSafeTCIAccessor.MoxChangeHandlers -= OnMoxChangeHandler;
                    console.ThreadSafeTCIAccessor.MoxPreChangeHandlers -= OnMoxPreChangeHandler;
					console.ThreadSafeTCIAccessor.ModeChangeHandlers -= OnModeChangeHandler;
					console.ThreadSafeTCIAccessor.BandChangeHandlers -= OnBandChangeHandler;
					console.ThreadSafeTCIAccessor.CentreFrequencyHandlers -= OnCentreFrequencyChanged;
					console.ThreadSafeTCIAccessor.FilterChangedHandlers -= OnFilterChanged;
					console.ThreadSafeTCIAccessor.FilterEdgesChangedHandlers -= OnFilterEdgesChanged;
					console.ThreadSafeTCIAccessor.PowerChangeHanders -= OnPowerChangeHander;
					console.ThreadSafeTCIAccessor.SplitChangedHandlers -= OnSplitChanged;
					console.ThreadSafeTCIAccessor.TuneChangedHandlers -= OnTuneChanged;
					console.ThreadSafeTCIAccessor.DrivePowerChangedHandlers -= OnDrivePowerChanged;
					console.ThreadSafeTCIAccessor.HWSampleRateChangedHandlers -= OnHWSampleRateChanged;
					console.ThreadSafeTCIAccessor.ThetisFocusChangedHandlers -= OnThetisFocusChanged;
					console.ThreadSafeTCIAccessor.RX2EnabledChangedHandlers -= OnRX2EnabledChanged;
					console.ThreadSafeTCIAccessor.SpotClickedHandlers -= OnSpotClicked;
                    console.ThreadSafeTCIAccessor.MuteChangedHandlers -= OnMuteChanged;
                    console.ThreadSafeTCIAccessor.MONChangedHandlers -= OnMONChanged;
                    console.ThreadSafeTCIAccessor.MONVolumeChangedHandlers -= OnMONVolumeChanged;
                    console.ThreadSafeTCIAccessor.TXFrequncyChangedHandlers -= OnTXFrequencyChanged;
                    console.ThreadSafeTCIAccessor.MeterReadingsChangedHandlers -= OnMeterReadingsChanged;
                    console.ThreadSafeTCIAccessor.NRChangedHandlers -= OnNrChanged;
                    console.ThreadSafeTCIAccessor.ANFChangedHandlers -= OnAnfChanged;
                    console.ThreadSafeTCIAccessor.RXGainChangedHandlers -= OnRxAfGainChanged;
                    console.ThreadSafeTCIAccessor.CTUNChangedHandlers -= OnCTUNChanged;
                    console.ThreadSafeTCIAccessor.TXProfileChangedHandlers -= OnTXProfileChanged;
                    console.ThreadSafeTCIAccessor.TXProfilesChangedHandlers -= OnTXProfilesChanged;

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
                cmaster.SetTCIRun(0);

				m_server = null;
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
                bVFOaUseRX2 = console.ThreadSafeTCIAccessor.RX2Enabled && UseRX1VFOaForRX2VFOa;
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
                duplicate_tochan = m_bCopyRX2VFObToVFOa && console.ThreadSafeTCIAccessor.RX2Enabled ? 0 : -1,
                replace_if_duplicated = m_bCopyRX2VFObToVFOa && _replace_if_copy_RX2VFObToVFOa && console.ThreadSafeTCIAccessor.RX2Enabled,
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
			bool bCTun = rx == 1 ? console.ThreadSafeTCIAccessor.ClickTuneDisplay : console.ThreadSafeTCIAccessor.ClickTuneRX2Display;

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
                    socketListener.NrChanged(rx, new_nr);
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

            cmaster.SetTCIRun(run ? 1 : 0);
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
            cmaster.SetTXTCIAudio(0, UsesActiveTCITxAudio() ? 1 : 0);
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
    }
}
