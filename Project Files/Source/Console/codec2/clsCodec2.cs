using System;

namespace Thetis.codec2
{
    public sealed class Codec2 : IDisposable
    {
        public const int Mode3200 = 0;
        public const int Mode2400 = 1;
        public const int Mode1600 = 2;
        public const int Mode1400 = 3;
        public const int Mode1300 = 4;
        public const int Mode1200 = 5;
        public const int Mode700C = 8;

        private IntPtr _codec2_state;
        private bool _disposed;

        public Codec2(int mode)
        {
            _codec2_state = Codec2Native.codec2_create(mode);

            if (_codec2_state == IntPtr.Zero)
                throw new InvalidOperationException("codec2_create failed.");

            Mode = mode;
            SamplesPerFrame = Codec2Native.codec2_samples_per_frame(_codec2_state);
            BitsPerFrame = Codec2Native.codec2_bits_per_frame(_codec2_state);
            BytesPerFrame = Codec2Native.codec2_bytes_per_frame(_codec2_state);
        }

        ~Codec2()
        {
            dispose(false);
        }

        public int Mode { get; private set; }

        public int SamplesPerFrame { get; private set; }

        public int BitsPerFrame { get; private set; }

        public int BytesPerFrame { get; private set; }

        public byte[] Encode(short[] speech_in)
        {
            throwIfDisposed();

            if (speech_in == null)
                throw new ArgumentNullException("speech_in");

            if (speech_in.Length != SamplesPerFrame)
                throw new ArgumentException("speech_in must contain exactly " + SamplesPerFrame + " samples.", "speech_in");

            byte[] bytes = new byte[BytesPerFrame];

            Codec2Native.codec2_encode(_codec2_state, bytes, speech_in);

            return bytes;
        }

        public void Encode(short[] speech_in, byte[] bytes)
        {
            throwIfDisposed();

            if (speech_in == null)
                throw new ArgumentNullException("speech_in");

            if (bytes == null)
                throw new ArgumentNullException("bytes");

            if (speech_in.Length != SamplesPerFrame)
                throw new ArgumentException("speech_in must contain exactly " + SamplesPerFrame + " samples.", "speech_in");

            if (bytes.Length < BytesPerFrame)
                throw new ArgumentException("bytes must be at least " + BytesPerFrame + " bytes.", "bytes");

            Codec2Native.codec2_encode(_codec2_state, bytes, speech_in);
        }

        public short[] Decode(byte[] bytes)
        {
            throwIfDisposed();

            if (bytes == null)
                throw new ArgumentNullException("bytes");

            if (bytes.Length < BytesPerFrame)
                throw new ArgumentException("bytes must be at least " + BytesPerFrame + " bytes.", "bytes");

            short[] speech_out = new short[SamplesPerFrame];

            Codec2Native.codec2_decode(_codec2_state, speech_out, bytes);

            return speech_out;
        }

        public void Decode(byte[] bytes, short[] speech_out)
        {
            Decode(bytes, speech_out, 0.0f);
        }

        public void Decode(byte[] bytes, short[] speech_out, float ber_est)
        {
            throwIfDisposed();

            if (bytes == null)
                throw new ArgumentNullException("bytes");

            if (speech_out == null)
                throw new ArgumentNullException("speech_out");

            if (bytes.Length < BytesPerFrame)
                throw new ArgumentException("bytes must be at least " + BytesPerFrame + " bytes.", "bytes");

            if (speech_out.Length != SamplesPerFrame)
                throw new ArgumentException("speech_out must contain exactly " + SamplesPerFrame + " samples.", "speech_out");

            if (ber_est > 0.0f)
                Codec2Native.codec2_decode_ber(_codec2_state, speech_out, bytes, ber_est);
            else
            Codec2Native.codec2_decode(_codec2_state, speech_out, bytes);
        }

        public void SetLpcPostFilter(bool enable, bool bass_boost, float beta, float gamma)
        {
            throwIfDisposed();
            Codec2Native.codec2_set_lpc_post_filter(_codec2_state, enable ? 1 : 0, bass_boost ? 1 : 0, beta, gamma);
        }

        public int GetSpareBitIndex()
        {
            throwIfDisposed();
            return Codec2Native.codec2_get_spare_bit_index(_codec2_state);
        }

        public void SetNaturalOrGray(bool gray)
        {
            throwIfDisposed();
            Codec2Native.codec2_set_natural_or_gray(_codec2_state, gray ? 1 : 0);
        }

        public float GetEnergy(byte[] bits)
        {
            throwIfDisposed();

            if (bits == null)
                throw new ArgumentNullException("bits");

            if (bits.Length < BytesPerFrame)
                throw new ArgumentException("bits must be at least " + BytesPerFrame + " bytes.", "bits");

            return Codec2Native.codec2_get_energy(_codec2_state, bits);
        }

        public void Set700CPostFilter(bool enabled)
        {
            throwIfDisposed();
            Codec2Native.codec2_700c_post_filter(_codec2_state, enabled);
        }

        public void Set700CEq(bool enabled)
        {
            throwIfDisposed();
            Codec2Native.codec2_700c_eq(_codec2_state, enabled);
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void throwIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Codec2");
        }

        private void dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (_codec2_state != IntPtr.Zero)
            {
                Codec2Native.codec2_destroy(_codec2_state);
                _codec2_state = IntPtr.Zero;
            }

            _disposed = true;
        }
    }
}
