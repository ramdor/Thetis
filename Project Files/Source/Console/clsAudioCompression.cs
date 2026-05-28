/*  clsAudioCompression.cs

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

using System;
using Concentus;
using Concentus.Enums;

namespace Thetis
{
    internal enum TCIAudioCodec
    {
        PCM = 0,
        OPUS = 1
    }

    internal sealed class clsAudioCompression : IDisposable
    {
        internal enum OpusApplicationMode
        {
            Voip = 0,
            Audio = 1,
            LowDelay = 2
        }

        internal enum OpusSignalMode
        {
            Auto = 0,
            Voice = 1,
            Music = 2
        }

        internal enum OpusBandwidthMode
        {
            Auto = 0,
            Narrowband = 1,
            Mediumband = 2,
            Wideband = 3,
            SuperWideband = 4,
            Fullband = 5
        }

        internal sealed class OpusSettings
        {
            public OpusApplicationMode Application = OpusApplicationMode.Audio;
            public OpusSignalMode Signal = OpusSignalMode.Auto;
            public OpusBandwidthMode Bandwidth = OpusBandwidthMode.Auto;
            public int Bitrate = 64000;
            public bool UseVbr = true;
            public bool UseConstrainedVbr = false;
            public int Complexity = 10;
            public bool UseFec = false;
            public bool UseDtx = false;
            public int PacketLossPercent = 0;
            public double FrameMs = 20.0;
        }

        private IOpusEncoder m_encoder;
        private IOpusDecoder m_decoder;
        private int m_encoderSampleRate;
        private int m_encoderChannels;
        private int m_decoderSampleRate;
        private int m_decoderChannels;
        private OpusApplicationMode m_encoderApplication;
        private int m_encoderBitrate;
        private bool m_encoderUseVbr;
        private bool m_encoderUseConstrainedVbr;
        private int m_encoderComplexity;
        private bool m_encoderUseFec;
        private bool m_encoderUseDtx;
        private int m_encoderPacketLossPercent;
        private OpusSignalMode m_encoderSignal;
        private OpusBandwidthMode m_encoderBandwidth;
        private double m_encoderFrameMs;
        private byte[] m_encodeBuffer;
        private float[] m_decodeBuffer;

        public clsAudioCompression()
        {
            m_encodeBuffer = new byte[4096];
            m_decodeBuffer = Array.Empty<float>();
        }

        public void Dispose()
        {
            resetEncoder();
            resetDecoder();
        }

        public void Reset()
        {
            resetEncoder();
            resetDecoder();
        }

        public byte[] EncodeOpus(float[] interleaved, int samplesPerChannel, int sampleRate, int channels, OpusSettings settings, out int encodedBytes)
        {
            encodedBytes = 0;
            if (interleaved == null || interleaved.Length == 0 || samplesPerChannel <= 0 || !IsSupportedSampleRate(sampleRate) || channels <= 0 || settings == null)
                return Array.Empty<byte>();

            ensureEncoder(sampleRate, channels, settings);

            int maxPacketBytes = getRecommendedPacketBytes(settings.Bitrate, settings.FrameMs);
            if (m_encodeBuffer == null || m_encodeBuffer.Length < maxPacketBytes)
                m_encodeBuffer = new byte[maxPacketBytes];

            encodedBytes = m_encoder.Encode(interleaved.AsSpan(0, samplesPerChannel * channels), samplesPerChannel, m_encodeBuffer.AsSpan(), m_encodeBuffer.Length);
            if (encodedBytes <= 0)
                return Array.Empty<byte>();

            byte[] encoded = new byte[encodedBytes];
            Buffer.BlockCopy(m_encodeBuffer, 0, encoded, 0, encodedBytes);
            return encoded;
        }

        public float[] DecodeOpus(byte[] payload, int payloadOffset, int payloadLength, int sampleRate, int channels, OpusSettings settings, out int decodedValueCount)
        {
            decodedValueCount = 0;
            if (payload == null || payloadLength <= 0 || payloadOffset < 0 || payloadLength < 0 || payloadOffset > payload.Length || payloadOffset + payloadLength > payload.Length || !IsSupportedSampleRate(sampleRate) || channels <= 0 || settings == null)
                return Array.Empty<float>();

            ensureDecoder(sampleRate, channels, settings);

            int maxFrameSamples = GetFrameSamples(sampleRate, settings.FrameMs);
            int requiredValues = maxFrameSamples * channels;
            if (m_decodeBuffer == null || m_decodeBuffer.Length < requiredValues)
                m_decodeBuffer = new float[requiredValues];

            int decodedSamplesPerChannel = m_decoder.Decode(new ReadOnlySpan<byte>(payload, payloadOffset, payloadLength), m_decodeBuffer.AsSpan(0, requiredValues), maxFrameSamples, false);
            if (decodedSamplesPerChannel <= 0)
                return Array.Empty<float>();

            decodedValueCount = decodedSamplesPerChannel * channels;
            float[] decoded = new float[decodedValueCount];
            Buffer.BlockCopy(m_decodeBuffer, 0, decoded, 0, decodedValueCount * sizeof(float));
            return decoded;
        }

        public static bool TryParseApplication(string value, out OpusApplicationMode mode)
        {
            mode = OpusApplicationMode.Audio;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            switch (value.Trim().ToLowerInvariant())
            {
                case "voip":
                    mode = OpusApplicationMode.Voip;
                    return true;
                case "audio":
                    mode = OpusApplicationMode.Audio;
                    return true;
                case "lowdelay":
                case "low_delay":
                case "restricted_lowdelay":
                case "restricted-lowdelay":
                    mode = OpusApplicationMode.LowDelay;
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryParseSignal(string value, out OpusSignalMode mode)
        {
            mode = OpusSignalMode.Auto;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            switch (value.Trim().ToLowerInvariant())
            {
                case "auto":
                    mode = OpusSignalMode.Auto;
                    return true;
                case "voice":
                    mode = OpusSignalMode.Voice;
                    return true;
                case "music":
                    mode = OpusSignalMode.Music;
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryParseBandwidth(string value, out OpusBandwidthMode mode)
        {
            mode = OpusBandwidthMode.Auto;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            switch (value.Trim().ToLowerInvariant())
            {
                case "auto":
                    mode = OpusBandwidthMode.Auto;
                    return true;
                case "narrowband":
                    mode = OpusBandwidthMode.Narrowband;
                    return true;
                case "mediumband":
                    mode = OpusBandwidthMode.Mediumband;
                    return true;
                case "wideband":
                    mode = OpusBandwidthMode.Wideband;
                    return true;
                case "superwideband":
                case "super_wideband":
                case "super-wideband":
                    mode = OpusBandwidthMode.SuperWideband;
                    return true;
                case "fullband":
                    mode = OpusBandwidthMode.Fullband;
                    return true;
                default:
                    return false;
            }
        }

        public static double NormalizeFrameMs(double frameMs)
        {
            if (Math.Abs(frameMs - 2.5) < 0.001) return 2.5;
            if (Math.Abs(frameMs - 5.0) < 0.001) return 5.0;
            if (Math.Abs(frameMs - 10.0) < 0.001) return 10.0;
            if (Math.Abs(frameMs - 20.0) < 0.001) return 20.0;
            if (Math.Abs(frameMs - 40.0) < 0.001) return 40.0;
            if (Math.Abs(frameMs - 60.0) < 0.001) return 60.0;
            return 20.0;
        }

        public static bool IsSupportedSampleRate(int sampleRate)
        {
            return sampleRate == 8000 || sampleRate == 12000 || sampleRate == 16000 || sampleRate == 24000 || sampleRate == 48000;
        }

        public static int GetFrameSamples(int sampleRate, double frameMs)
        {
            double normalizedFrameMs = NormalizeFrameMs(frameMs);
            int samples = (int)Math.Round(sampleRate * normalizedFrameMs / 1000.0);
            return Math.Max(1, samples);
        }

        public static string GetApplicationString(OpusApplicationMode mode)
        {
            switch (mode)
            {
                case OpusApplicationMode.Voip:
                    return "voip";
                case OpusApplicationMode.LowDelay:
                    return "lowdelay";
                case OpusApplicationMode.Audio:
                default:
                    return "audio";
            }
        }

        public static string GetSignalString(OpusSignalMode mode)
        {
            switch (mode)
            {
                case OpusSignalMode.Voice:
                    return "voice";
                case OpusSignalMode.Music:
                    return "music";
                case OpusSignalMode.Auto:
                default:
                    return "auto";
            }
        }

        public static string GetBandwidthString(OpusBandwidthMode mode)
        {
            switch (mode)
            {
                case OpusBandwidthMode.Narrowband:
                    return "narrowband";
                case OpusBandwidthMode.Mediumband:
                    return "mediumband";
                case OpusBandwidthMode.Wideband:
                    return "wideband";
                case OpusBandwidthMode.SuperWideband:
                    return "superwideband";
                case OpusBandwidthMode.Fullband:
                    return "fullband";
                case OpusBandwidthMode.Auto:
                default:
                    return "auto";
            }
        }

        private void ensureEncoder(int sampleRate, int channels, OpusSettings settings)
        {
            if (m_encoder == null ||
                m_encoderSampleRate != sampleRate ||
                m_encoderChannels != channels ||
                m_encoderApplication != settings.Application)
            {
                resetEncoder();
                m_encoder = OpusCodecFactory.CreateEncoder(sampleRate, channels, mapApplication(settings.Application), null);
                m_encoderSampleRate = sampleRate;
                m_encoderChannels = channels;
                m_encoderApplication = settings.Application;
                m_encoderBitrate = int.MinValue;
                m_encoderUseVbr = !settings.UseVbr;
                m_encoderUseConstrainedVbr = !settings.UseConstrainedVbr;
                m_encoderComplexity = int.MinValue;
                m_encoderUseFec = !settings.UseFec;
                m_encoderUseDtx = !settings.UseDtx;
                m_encoderPacketLossPercent = int.MinValue;
                m_encoderSignal = (OpusSignalMode)(-1);
                m_encoderBandwidth = (OpusBandwidthMode)(-1);
                m_encoderFrameMs = double.MinValue;
            }

            if (m_encoderBitrate != settings.Bitrate)
            {
                m_encoder.Bitrate = settings.Bitrate;
                m_encoderBitrate = settings.Bitrate;
            }

            if (m_encoderUseVbr != settings.UseVbr)
            {
                m_encoder.UseVBR = settings.UseVbr;
                m_encoderUseVbr = settings.UseVbr;
            }

            if (m_encoderUseConstrainedVbr != settings.UseConstrainedVbr)
            {
                m_encoder.UseConstrainedVBR = settings.UseConstrainedVbr;
                m_encoderUseConstrainedVbr = settings.UseConstrainedVbr;
            }

            if (m_encoderComplexity != settings.Complexity)
            {
                m_encoder.Complexity = settings.Complexity;
                m_encoderComplexity = settings.Complexity;
            }

            if (m_encoderUseFec != settings.UseFec)
            {
                m_encoder.UseInbandFEC = settings.UseFec;
                m_encoderUseFec = settings.UseFec;
            }

            if (m_encoderUseDtx != settings.UseDtx)
            {
                m_encoder.UseDTX = settings.UseDtx;
                m_encoderUseDtx = settings.UseDtx;
            }

            if (m_encoderPacketLossPercent != settings.PacketLossPercent)
            {
                m_encoder.PacketLossPercent = settings.PacketLossPercent;
                m_encoderPacketLossPercent = settings.PacketLossPercent;
            }

            if (m_encoderSignal != settings.Signal)
            {
                m_encoder.SignalType = mapSignal(settings.Signal);
                m_encoderSignal = settings.Signal;
            }

            if (m_encoderBandwidth != settings.Bandwidth)
            {
                //m_encoder.Bandwidth = mapBandwidth(settings.Bandwidth);
                //m_encoder.MaxBandwidth = mapBandwidth(settings.Bandwidth);
                //need to set a max limit when using auto
                if (settings.Bandwidth == OpusBandwidthMode.Auto)
                {
                    m_encoder.Bandwidth = OpusBandwidth.OPUS_BANDWIDTH_AUTO;
                    m_encoder.MaxBandwidth = OpusBandwidth.OPUS_BANDWIDTH_FULLBAND;
                }
                else
                {
                    OpusBandwidth bandwidth = mapBandwidth(settings.Bandwidth);
                    m_encoder.Bandwidth = bandwidth;
                    m_encoder.MaxBandwidth = bandwidth;
                }

                m_encoderBandwidth = settings.Bandwidth;
            }

            double normalizedFrameMs = NormalizeFrameMs(settings.FrameMs);
            if (Math.Abs(m_encoderFrameMs - normalizedFrameMs) > 0.0001)
            {
                m_encoder.ExpertFrameDuration = mapFrameSize(normalizedFrameMs);
                m_encoderFrameMs = normalizedFrameMs;
            }
        }

        private void ensureDecoder(int sampleRate, int channels, OpusSettings settings)
        {
            if (m_decoder == null || m_decoderSampleRate != sampleRate || m_decoderChannels != channels)
            {
                resetDecoder();
                m_decoder = OpusCodecFactory.CreateDecoder(sampleRate, channels, null);
                m_decoderSampleRate = sampleRate;
                m_decoderChannels = channels;
            }
        }

        private static OpusApplication mapApplication(OpusApplicationMode mode)
        {
            switch (mode)
            {
                case OpusApplicationMode.Voip:
                    return OpusApplication.OPUS_APPLICATION_VOIP;
                case OpusApplicationMode.LowDelay:
                    return OpusApplication.OPUS_APPLICATION_RESTRICTED_LOWDELAY;
                case OpusApplicationMode.Audio:
                default:
                    return OpusApplication.OPUS_APPLICATION_AUDIO;
            }
        }

        private static OpusSignal mapSignal(OpusSignalMode mode)
        {
            switch (mode)
            {
                case OpusSignalMode.Voice:
                    return OpusSignal.OPUS_SIGNAL_VOICE;
                case OpusSignalMode.Music:
                    return OpusSignal.OPUS_SIGNAL_MUSIC;
                case OpusSignalMode.Auto:
                default:
                    return OpusSignal.OPUS_SIGNAL_AUTO;
            }
        }

        private static OpusBandwidth mapBandwidth(OpusBandwidthMode mode)
        {
            switch (mode)
            {
                case OpusBandwidthMode.Narrowband:
                    return OpusBandwidth.OPUS_BANDWIDTH_NARROWBAND;
                case OpusBandwidthMode.Mediumband:
                    return OpusBandwidth.OPUS_BANDWIDTH_MEDIUMBAND;
                case OpusBandwidthMode.Wideband:
                    return OpusBandwidth.OPUS_BANDWIDTH_WIDEBAND;
                case OpusBandwidthMode.SuperWideband:
                    return OpusBandwidth.OPUS_BANDWIDTH_SUPERWIDEBAND;
                case OpusBandwidthMode.Fullband:
                    return OpusBandwidth.OPUS_BANDWIDTH_FULLBAND;
                case OpusBandwidthMode.Auto:
                default:
                    return OpusBandwidth.OPUS_BANDWIDTH_AUTO;
            }
        }

        private static OpusFramesize mapFrameSize(double frameMs)
        {
            double normalizedFrameMs = NormalizeFrameMs(frameMs);
            if (Math.Abs(normalizedFrameMs - 2.5) < 0.001) return OpusFramesize.OPUS_FRAMESIZE_2_5_MS;
            if (Math.Abs(normalizedFrameMs - 5.0) < 0.001) return OpusFramesize.OPUS_FRAMESIZE_5_MS;
            if (Math.Abs(normalizedFrameMs - 10.0) < 0.001) return OpusFramesize.OPUS_FRAMESIZE_10_MS;
            if (Math.Abs(normalizedFrameMs - 20.0) < 0.001) return OpusFramesize.OPUS_FRAMESIZE_20_MS;
            if (Math.Abs(normalizedFrameMs - 40.0) < 0.001) return OpusFramesize.OPUS_FRAMESIZE_40_MS;
            if (Math.Abs(normalizedFrameMs - 60.0) < 0.001) return OpusFramesize.OPUS_FRAMESIZE_60_MS;
            return OpusFramesize.OPUS_FRAMESIZE_20_MS;
        }

        private static int getRecommendedPacketBytes(int bitrate, double frameMs)
        {
            int safeBitrate = Math.Max(6000, bitrate);
            double safeFrameMs = NormalizeFrameMs(frameMs);
            int estimatedBytes = (int)Math.Ceiling((safeBitrate * safeFrameMs) / 8000.0);
            if (estimatedBytes < 128) estimatedBytes = 128;
            if (estimatedBytes > 4096) estimatedBytes = 4096;
            return estimatedBytes;
        }

        private void resetEncoder()
        {
            if (m_encoder != null)
            {
                if (m_encoder is IDisposable disposableEncoder)
                    disposableEncoder.Dispose();
                m_encoder = null;
            }

            m_encoderSampleRate = 0;
            m_encoderChannels = 0;
            m_encoderApplication = OpusApplicationMode.Audio;
            m_encoderBitrate = 0;
            m_encoderUseVbr = false;
            m_encoderUseConstrainedVbr = false;
            m_encoderComplexity = 0;
            m_encoderUseFec = false;
            m_encoderUseDtx = false;
            m_encoderPacketLossPercent = 0;
            m_encoderSignal = OpusSignalMode.Auto;
            m_encoderBandwidth = OpusBandwidthMode.Auto;
            m_encoderFrameMs = 0.0;
        }

        private void resetDecoder()
        {
            if (m_decoder != null)
            {
                if (m_decoder is IDisposable disposableDecoder)
                    disposableDecoder.Dispose();
                m_decoder = null;
            }

            m_decoderSampleRate = 0;
            m_decoderChannels = 0;
        }
    }
}
