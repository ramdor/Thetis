/*  clsDisplayCompression.cs

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
using System.Runtime.CompilerServices;

namespace Thetis
{
    internal sealed class clsDisplayCompression
    {
        internal const int DisplayBlockDeltaMarker = 8;
        internal const int DisplayUseLastFrameMarker = 9;
        internal const int DisplayCompactHeaderLength = 24;
        internal const byte DisplayCompactMagic0 = 84; // T
        internal const byte DisplayCompactMagic1 = 68; // D
        internal const byte DisplayCompactVersion = 1; // 1  ... TCI Display v1
        internal const int DisplayBlockDeltaDefaultBlockSize = 64;
        internal const int DisplayBlockDeltaMinSavingPercent = 5;

        private const int DisplayBlockDeltaRawMode = 0;
        private const int DisplayBlockDeltaUnchangedMode = 1;

        private static readonly int[] AdaptiveBlockSizes = new int[] { 64, 32, 16, 128 };

        internal enum PayloadKind
        {
            Raw = 0,
            Delta = 1,
            BlockDelta = 2,
            UseLast = 3
        }

        internal sealed class Settings
        {
            public bool DeltaMode = false;
            public bool BlockDeltaMode = false;
            public bool AdaptiveBlocks = false;
            public int AccuracyMode = 0;
            public int KeyFrameIntervalFrames = 0;
            public int QuantizationBits = 8;
        }

        internal sealed class State
        {
            public byte[] LastSentFrame;
            public byte[] EncodedFrame;
            public int FramesSinceKeyFrame;

            public void Reset()
            {
                LastSentFrame = null;
                EncodedFrame = null;
                FramesSinceKeyFrame = 0;
            }
        }

        internal sealed class Result
        {
            public PayloadKind Kind;
            public byte[] SamplePayload;
            public int LogicalLength;
            public int DeltaBits;
            public int BlockSize;
            public int BlockCount;
            public int QuantizationBits;
        }

        internal Result Encode(float[] pixels, int pixelCount, float minDbm, float maxDbm, Settings settings, State state)
        {
            if (pixels == null || pixelCount <= 0 || state == null)
                return null;

            if (settings == null)
                settings = new Settings();

            int quantizationBits = ClampQuantizationBits(settings.QuantizationBits);
            ensureEncodedFrame(state, pixelCount);

            for (int i = 0; i < pixelCount; i++)
                state.EncodedFrame[i] = quantizeDisplayValue(pixels[i], minDbm, maxDbm, quantizationBits);

            byte[] previousFrame = state.LastSentFrame;
            bool hasPreviousFrame = previousFrame != null && previousFrame.Length == pixelCount;
            bool forceKeyFrame = !settings.DeltaMode || !hasPreviousFrame || shouldSendKeyFrame(settings, state);
            int accuracyThreshold = getAccuracyThreshold(settings.AccuracyMode);

            if (hasPreviousFrame && accuracyThreshold > 0)
                applyAccuracyThreshold(state.EncodedFrame, previousFrame, pixelCount, accuracyThreshold);

            if (settings.DeltaMode && hasPreviousFrame && !forceKeyFrame && framesAreEqual(state.EncodedFrame, previousFrame, pixelCount))
            {
                state.FramesSinceKeyFrame++;
                return new Result()
                {
                    Kind = PayloadKind.UseLast,
                    SamplePayload = Array.Empty<byte>(),
                    LogicalLength = pixelCount,
                    DeltaBits = DisplayUseLastFrameMarker,
                    BlockSize = 0,
                    BlockCount = 0,
                    QuantizationBits = quantizationBits
                };
            }

            if (forceKeyFrame)
                return buildRawResult(state, pixelCount, settings, quantizationBits);

            int bestLength = getRawPayloadLength(pixelCount, quantizationBits);
            PayloadKind bestKind = PayloadKind.Raw;
            byte[] bestPayload = null;
            int bestDeltaBits = 0;
            int bestBlockSize = 0;
            int bestBlockCount = 0;

            int maxAbsoluteDelta = getMaxAbsoluteDelta(state.EncodedFrame, previousFrame, 0, pixelCount);
            int bitsPerDelta = maxAbsoluteDelta >= 64 ? 0 : getDisplayDeltaBits(maxAbsoluteDelta);
            byte[] deltaPayload = null;
            if (bitsPerDelta > 0)
            {
                deltaPayload = buildDisplayDeltaPayload(state.EncodedFrame, previousFrame, pixelCount, bitsPerDelta);
                if (deltaPayload != null && deltaPayload.Length < bestLength)
                {
                    bestLength = deltaPayload.Length;
                    bestKind = PayloadKind.Delta;
                    bestPayload = deltaPayload;
                    bestDeltaBits = bitsPerDelta;
                    bestBlockSize = 0;
                    bestBlockCount = 0;
                }
            }

            if (settings.BlockDeltaMode)
            {
                int blockSize;
                int blockCount;
                byte[] blockDeltaPayload = buildBestDisplayBlockDeltaPayload(state.EncodedFrame, previousFrame, pixelCount, settings.AdaptiveBlocks, out blockSize, out blockCount);
                if (blockDeltaPayload != null && isDisplayBlockDeltaWorthSending(blockDeltaPayload.Length, pixelCount) && blockDeltaPayload.Length < bestLength)
                {
                    bestLength = blockDeltaPayload.Length;
                    bestKind = PayloadKind.BlockDelta;
                    bestPayload = blockDeltaPayload;
                    bestDeltaBits = DisplayBlockDeltaMarker;
                    bestBlockSize = blockSize;
                    bestBlockCount = blockCount;
                }
            }

            if (bestKind == PayloadKind.Raw)
                return buildRawResult(state, pixelCount, settings, quantizationBits);

            copyToLastSentFrame(state, pixelCount);
            state.FramesSinceKeyFrame++;

            return new Result()
            {
                Kind = bestKind,
                SamplePayload = bestPayload,
                LogicalLength = pixelCount,
                DeltaBits = bestDeltaBits,
                BlockSize = bestBlockSize,
                BlockCount = bestBlockCount,
                QuantizationBits = quantizationBits
            };
        }

        internal static int ClampAccuracyMode(int mode)
        {
            if (mode < 0) return 0;
            if (mode > 2) return 2;
            return mode;
        }

        internal static int ClampKeyFrameIntervalFrames(int intervalFrames)
        {
            if (intervalFrames < 0) return 0;
            if (intervalFrames > 1000000) return 1000000;
            return intervalFrames;
        }

        internal static int ClampQuantizationBits(int quantizationBits)
        {
            if (quantizationBits < 6) return 6;
            if (quantizationBits > 8) return 8;
            return quantizationBits;
        }

        private static Result buildRawResult(State state, int pixelCount, Settings settings, int quantizationBits)
        {
            byte[] payload;
            if (quantizationBits < 8)
                payload = packRawFrame(state.EncodedFrame, pixelCount, quantizationBits);
            else
                payload = state.EncodedFrame;

            copyToLastSentFrame(state, pixelCount);
            state.FramesSinceKeyFrame = 0;

            return new Result()
            {
                Kind = PayloadKind.Raw,
                SamplePayload = payload,
                LogicalLength = pixelCount,
                DeltaBits = 0,
                BlockSize = 0,
                BlockCount = 0,
                QuantizationBits = quantizationBits
            };
        }

        private static void ensureEncodedFrame(State state, int pixelCount)
        {
            if (state.EncodedFrame == null || state.EncodedFrame.Length != pixelCount)
                state.EncodedFrame = new byte[pixelCount];
        }

        private static bool shouldSendKeyFrame(Settings settings, State state)
        {
            int intervalFrames = ClampKeyFrameIntervalFrames(settings.KeyFrameIntervalFrames);
            return intervalFrames > 0 && state.FramesSinceKeyFrame >= intervalFrames;
        }

        private static int getAccuracyThreshold(int accuracyMode)
        {
            switch (ClampAccuracyMode(accuracyMode))
            {
                case 1:
                    return 1;
                case 2:
                    return 2;
                default:
                    return 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte quantizeDisplayValue(float value, float minDbm, float maxDbm, int quantizationBits)
        {
            int maxValue = (1 << quantizationBits) - 1;
            if (float.IsNaN(value) || float.IsInfinity(value) || value < minDbm) return 0;
            if (value > maxDbm) return (byte)maxValue;

            float normalized = (value - minDbm) / (maxDbm - minDbm);
            int quantized = 1 + (int)Math.Round(normalized * (maxValue - 2));
            if (quantized < 1) quantized = 1;
            if (quantized > maxValue - 1) quantized = maxValue - 1;
            return (byte)quantized;
        }

        private static void applyAccuracyThreshold(byte[] currentFrame, byte[] previousFrame, int pixelCount, int threshold)
        {
            for (int i = 0; i < pixelCount; i++)
            {
                int absoluteDelta = currentFrame[i] - previousFrame[i];
                if (absoluteDelta < 0)
                    absoluteDelta = -absoluteDelta;

                if (absoluteDelta <= threshold)
                    currentFrame[i] = previousFrame[i];
            }
        }

        private static bool framesAreEqual(byte[] currentFrame, byte[] previousFrame, int pixelCount)
        {
            for (int i = 0; i < pixelCount; i++)
            {
                if (currentFrame[i] != previousFrame[i])
                    return false;
            }

            return true;
        }

        private static void copyToLastSentFrame(State state, int pixelCount)
        {
            if (state.LastSentFrame == null || state.LastSentFrame.Length != pixelCount)
                state.LastSentFrame = new byte[pixelCount];

            Buffer.BlockCopy(state.EncodedFrame, 0, state.LastSentFrame, 0, pixelCount);
        }

        private static int getRawPayloadLength(int pixelCount, int quantizationBits)
        {
            if (quantizationBits < 8)
                return ((pixelCount * quantizationBits) + 7) / 8;

            return pixelCount;
        }

        private static byte[] packRawFrame(byte[] frame, int pixelCount, int quantizationBits)
        {
            int totalBits = pixelCount * quantizationBits;
            int totalBytes = (totalBits + 7) / 8;
            byte[] payload = new byte[totalBytes];
            int bitOffset = 0;

            for (int i = 0; i < pixelCount; i++)
            {
                int packedValue = frame[i];
                for (int bitIndex = quantizationBits - 1; bitIndex >= 0; bitIndex--)
                {
                    int bit = (packedValue >> bitIndex) & 1;
                    int byteIndex = bitOffset >> 3;
                    int bitInByte = 7 - (bitOffset & 7);
                    if (bit != 0)
                        payload[byteIndex] = (byte)(payload[byteIndex] | (byte)(1 << bitInByte));
                    bitOffset++;
                }
            }

            return payload;
        }

        private static int getDisplayDeltaBits(int maxAbsoluteDelta)
        {
            if (maxAbsoluteDelta <= 1) return 2;
            if (maxAbsoluteDelta <= 3) return 3;
            if (maxAbsoluteDelta <= 7) return 4;
            if (maxAbsoluteDelta <= 15) return 5;
            if (maxAbsoluteDelta <= 31) return 6;
            if (maxAbsoluteDelta <= 63) return 7;
            return 0;
        }

        private static int getMaxAbsoluteDelta(byte[] currentFrame, byte[] previousFrame, int startIndex, int pixelCount)
        {
            int maxAbsoluteDelta = 0;

            for (int i = 0; i < pixelCount; i++)
            {
                int sourceIndex = startIndex + i;
                int absoluteDelta = currentFrame[sourceIndex] - previousFrame[sourceIndex];
                if (absoluteDelta < 0)
                    absoluteDelta = -absoluteDelta;

                if (absoluteDelta > maxAbsoluteDelta)
                    maxAbsoluteDelta = absoluteDelta;

                if (maxAbsoluteDelta >= 64)
                    return maxAbsoluteDelta;
            }

            return maxAbsoluteDelta;
        }

        private static byte[] buildDisplayDeltaPayload(byte[] currentFrame, byte[] previousFrame, int pixelCount, int bitsPerDelta)
        {
            if (currentFrame == null || previousFrame == null || pixelCount <= 0 || bitsPerDelta < 2 || bitsPerDelta > 7)
                return null;

            int totalBits = pixelCount * bitsPerDelta;
            int totalBytes = (totalBits + 7) / 8;
            byte[] payload = new byte[totalBytes];
            packDisplayDeltaBlock(currentFrame, previousFrame, 0, pixelCount, bitsPerDelta, payload, 0);
            return payload;
        }

        private static void packDisplayDeltaBlock(byte[] currentFrame, byte[] previousFrame, int startIndex, int pixelCount, int bitsPerDelta, byte[] payload, int payloadOffset)
        {
            int bias = (1 << (bitsPerDelta - 1)) - 1;
            int bitOffset = 0;

            for (int i = 0; i < pixelCount; i++)
            {
                int sourceIndex = startIndex + i;
                int delta = currentFrame[sourceIndex] - previousFrame[sourceIndex];
                int packedValue = delta + bias;

                for (int bitIndex = bitsPerDelta - 1; bitIndex >= 0; bitIndex--)
                {
                    int bit = (packedValue >> bitIndex) & 1;
                    int byteIndex = payloadOffset + (bitOffset >> 3);
                    int bitInByte = 7 - (bitOffset & 7);
                    if (bit != 0)
                        payload[byteIndex] = (byte)(payload[byteIndex] | (byte)(1 << bitInByte));
                    bitOffset++;
                }
            }
        }

        private static int getDisplayBlockDeltaMode(byte[] currentFrame, byte[] previousFrame, int startIndex, int pixelCount)
        {
            int maxAbsoluteDelta = getMaxAbsoluteDelta(currentFrame, previousFrame, startIndex, pixelCount);

            if (maxAbsoluteDelta >= 64)
                return DisplayBlockDeltaRawMode;

            if (maxAbsoluteDelta == 0)
                return DisplayBlockDeltaUnchangedMode;

            return getDisplayDeltaBits(maxAbsoluteDelta);
        }

        private static byte[] buildBestDisplayBlockDeltaPayload(byte[] currentFrame, byte[] previousFrame, int pixelCount, bool adaptiveBlocks, out int blockSize, out int blockCount)
        {
            blockSize = DisplayBlockDeltaDefaultBlockSize;
            blockCount = 0;

            if (!adaptiveBlocks)
                return buildDisplayBlockDeltaPayload(currentFrame, previousFrame, pixelCount, DisplayBlockDeltaDefaultBlockSize, out blockCount);

            byte[] bestPayload = null;
            int bestBlockSize = DisplayBlockDeltaDefaultBlockSize;
            int bestBlockCount = 0;
            int bestLength = int.MaxValue;

            for (int i = 0; i < AdaptiveBlockSizes.Length; i++)
            {
                int candidateBlockCount;
                byte[] candidatePayload = buildDisplayBlockDeltaPayload(currentFrame, previousFrame, pixelCount, AdaptiveBlockSizes[i], out candidateBlockCount);
                if (candidatePayload != null && candidatePayload.Length < bestLength)
                {
                    bestPayload = candidatePayload;
                    bestBlockSize = AdaptiveBlockSizes[i];
                    bestBlockCount = candidateBlockCount;
                    bestLength = candidatePayload.Length;
                }
            }

            blockSize = bestBlockSize;
            blockCount = bestBlockCount;
            return bestPayload;
        }

        private static byte[] buildDisplayBlockDeltaPayload(byte[] currentFrame, byte[] previousFrame, int pixelCount, int blockSize, out int blockCount)
        {
            blockCount = 0;
            if (currentFrame == null || previousFrame == null || pixelCount <= 0 || blockSize <= 0)
                return null;

            blockCount = (pixelCount + blockSize - 1) / blockSize;
            int[] blockModes = new int[blockCount];
            int payloadLength = blockCount;

            for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                int startIndex = blockIndex * blockSize;
                int blockPixelCount = Math.Min(blockSize, pixelCount - startIndex);
                int blockMode = getDisplayBlockDeltaMode(currentFrame, previousFrame, startIndex, blockPixelCount);
                blockModes[blockIndex] = blockMode;

                if (blockMode == DisplayBlockDeltaRawMode)
                    payloadLength += blockPixelCount;
                else if (blockMode >= 2 && blockMode <= 7)
                    payloadLength += ((blockPixelCount * blockMode) + 7) / 8;
            }

            byte[] payload = new byte[payloadLength];
            int payloadOffset = 0;

            for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
                payload[payloadOffset++] = (byte)blockModes[blockIndex];

            for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                int startIndex = blockIndex * blockSize;
                int blockPixelCount = Math.Min(blockSize, pixelCount - startIndex);
                int blockMode = blockModes[blockIndex];

                if (blockMode == DisplayBlockDeltaRawMode)
                {
                    Buffer.BlockCopy(currentFrame, startIndex, payload, payloadOffset, blockPixelCount);
                    payloadOffset += blockPixelCount;
                }
                else if (blockMode >= 2 && blockMode <= 7)
                {
                    int blockPayloadLength = ((blockPixelCount * blockMode) + 7) / 8;
                    packDisplayDeltaBlock(currentFrame, previousFrame, startIndex, blockPixelCount, blockMode, payload, payloadOffset);
                    payloadOffset += blockPayloadLength;
                }
            }

            return payload;
        }

        private static bool isDisplayBlockDeltaWorthSending(int blockPayloadLength, int pixelCount)
        {
            if (blockPayloadLength <= 0 || pixelCount <= 0)
                return false;

            int minimumSaving = Math.Max(1, (pixelCount * DisplayBlockDeltaMinSavingPercent) / 100);
            return blockPayloadLength <= pixelCount - minimumSaving;
        }
    }
}
