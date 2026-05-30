using System;
using System.Runtime.InteropServices;

namespace Thetis.codec2
{
    internal static class Codec2Native
    {
        private const string DllName = "libcodec2.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_create")]
        internal static extern IntPtr codec2_create(int mode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_destroy")]
        internal static extern void codec2_destroy(IntPtr codec2_state);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_encode")]
        internal static extern void codec2_encode(IntPtr codec2_state, byte[] bytes, short[] speech_in);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_decode")]
        internal static extern void codec2_decode(IntPtr codec2_state, short[] speech_out, byte[] bytes);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_samples_per_frame")]
        internal static extern int codec2_samples_per_frame(IntPtr codec2_state);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_bits_per_frame")]
        internal static extern int codec2_bits_per_frame(IntPtr codec2_state);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_bytes_per_frame")]
        internal static extern int codec2_bytes_per_frame(IntPtr codec2_state);
    }
}
