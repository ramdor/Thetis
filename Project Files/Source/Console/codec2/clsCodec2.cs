using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            SamplesPerFrame = Codec2Native.codec2_samples_per_frame(_codec2_state);
            BitsPerFrame = Codec2Native.codec2_bits_per_frame(_codec2_state);
            BytesPerFrame = Codec2Native.codec2_bytes_per_frame(_codec2_state);
        }

        ~Codec2()
        {
            dispose(false);
        }

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
            throwIfDisposed();

            if (bytes == null)
                throw new ArgumentNullException("bytes");

            if (speech_out == null)
                throw new ArgumentNullException("speech_out");

            if (bytes.Length < BytesPerFrame)
                throw new ArgumentException("bytes must be at least " + BytesPerFrame + " bytes.", "bytes");

            if (speech_out.Length != SamplesPerFrame)
                throw new ArgumentException("speech_out must contain exactly " + SamplesPerFrame + " samples.", "speech_out");

            Codec2Native.codec2_decode(_codec2_state, speech_out, bytes);
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
