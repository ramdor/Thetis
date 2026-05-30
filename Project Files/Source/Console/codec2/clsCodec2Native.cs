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

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_decode_ber")]
        internal static extern void codec2_decode_ber(IntPtr codec2_state, short[] speech_out, byte[] bytes, float ber_est);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_samples_per_frame")]
        internal static extern int codec2_samples_per_frame(IntPtr codec2_state);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_bits_per_frame")]
        internal static extern int codec2_bits_per_frame(IntPtr codec2_state);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_bytes_per_frame")]
        internal static extern int codec2_bytes_per_frame(IntPtr codec2_state);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_set_lpc_post_filter")]
        internal static extern void codec2_set_lpc_post_filter(IntPtr codec2_state, int enable, int bass_boost, float beta, float gamma);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_get_spare_bit_index")]
        internal static extern int codec2_get_spare_bit_index(IntPtr codec2_state);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_set_natural_or_gray")]
        internal static extern void codec2_set_natural_or_gray(IntPtr codec2_state, int gray);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_get_energy")]
        internal static extern float codec2_get_energy(IntPtr codec2_state, byte[] bits);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_700c_post_filter")]
        internal static extern void codec2_700c_post_filter(IntPtr codec2_state, [MarshalAs(UnmanagedType.I1)] bool enabled);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "codec2_700c_eq")]
        internal static extern void codec2_700c_eq(IntPtr codec2_state, [MarshalAs(UnmanagedType.I1)] bool enabled);
    }
}
