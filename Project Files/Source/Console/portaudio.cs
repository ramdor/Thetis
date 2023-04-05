//=================================================================
// portaudio.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2012  FlexRadio Systems
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// You may contact us via email at: gpl@flexradio.com.
// Paper mail may be sent to:
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
//=================================================================

using System;
using System.Collections;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using PaError = System.Int32;
using PaDeviceIndex = System.Int32;
using PaHostApiIndex = System.Int32;
using PaTime = System.Double;
using PaSampleFormat = System.UInt32;
using PaStreamFlags = System.UInt32;
using PaStreamCallbackFlags = System.UInt32;
using System.Reflection;
using System.Xml.Linq;

namespace Thetis
{
    public class PortAudioForThetis
    {
        #region Constants

        public const PaDeviceIndex paNoDevice = (PaDeviceIndex)(-1);
        public const PaDeviceIndex paUseHostApiSpecificDeviceSpecification
            = (PaDeviceIndex)(-2);
        public const PaSampleFormat paFloat32 = (PaSampleFormat)0x01;
        public const PaSampleFormat paInt32 = (PaSampleFormat)0x02;
        public const PaSampleFormat paInt24 = (PaSampleFormat)0x04;
        public const PaSampleFormat paInt16 = (PaSampleFormat)0x08;
        public const PaSampleFormat paInt8 = (PaSampleFormat)0x10;
        public const PaSampleFormat paUInt8 = (PaSampleFormat)0x20;
        public const PaSampleFormat paCustomFormat = (PaSampleFormat)0x10000;
        public const PaSampleFormat paNonInterleaved = (PaSampleFormat)0x80000000;
        public const uint paFormatIsSupported = 0;
        public const uint paFramesPerBufferUnspecified = 0;
        public const PaStreamFlags paNoFlag = (PaStreamFlags)0;
        public const PaStreamFlags paClipOff = (PaStreamFlags)0x01;
        public const PaStreamFlags paDitherOff = (PaStreamFlags)0x02;
        public const PaStreamFlags paNeverDropInput = (PaStreamFlags)0x04;
        public const PaStreamFlags paPrimeOutputBuffersUsingStreamCallback
            = (PaStreamFlags)0x08;
        public const PaStreamFlags paPlatformSpecificFlags
            = (PaStreamFlags)0xFFFF0000;
        public const PaStreamCallbackFlags paInputUnderflow
            = (PaStreamCallbackFlags)0x01;
        public const PaStreamCallbackFlags paInputOverflow
            = (PaStreamCallbackFlags)0x02;
        public const PaStreamCallbackFlags paOutputUnderflow
            = (PaStreamCallbackFlags)0x04;
        public const PaStreamCallbackFlags paOutputOverflow
            = (PaStreamCallbackFlags)0x08;
        public const PaStreamCallbackFlags paPrimingOutput
            = (PaStreamCallbackFlags)0x10;

        #endregion

        #region Enums

        public enum PaErrorCode
        {
            paNoError = 0,
            paNotInitialized = -10000,
            paUnanticipatedHostError,
            paInvalidChannelCount,
            paInvalidSampleRate,
            paInvalidDevice,
            paInvalidFlag,
            paSampleFormatNotSupported,
            paBadIODeviceCombination,
            paInsufficientMemory,
            paBufferTooBig,
            paBufferTooSmall,
            paNullCallback,
            paBadStreamPtr,
            paTimedOut,
            paInternalError,
            paDeviceUnavailable,
            paIncompatibleHostApiSpecificStreamInfo,
            paStreamIsStopped,
            paStreamIsNotStopped,
            paInputOverflowed,
            paOutputUnderflowed,
            paHostApiNotFound,
            paInvalidHostApi,
            paCanNotReadFromACallbackStream,
            paCanNotWriteToACallbackStream,
            paCanNotReadFromAnOutputOnlyStream,
            paCanNotWriteToAnInputOnlyStream,
            paIncompatibleStreamHostApi
        }

        public enum PaHostApiTypeId
        {
            paInDevelopment = 0,
            paDirectSound = 1,
            paMME = 2,
            paASIO = 3,
            paSoundManager = 4,
            paCoreAudio = 5,
            paOSS = 7,
            paALSA = 8,
            paAL = 9,
            paBeOS = 10
        }

        public enum PaStreamCallbackResult
        {
            paContinue = 0,
            paComplete = 1,
            paAbort = 2
        }

        #endregion

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        public struct PaHostApiInfo
        {
            public string name => _name == IntPtr.Zero
                ? null
                : Marshal.PtrToStringAnsi(_name);

            public int structVersion;
            public int type;
            // [MarshalAs(UnmanagedType.LPStr)]
            // public string name;
            private readonly IntPtr _name;
            public int deviceCount;
            public PaDeviceIndex defaultInputDevice;
            public PaDeviceIndex defaultOutputDevice;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PaHostErrorInfo
        {
            public PaHostApiTypeId hostApiType;
            public int errorCode;
            [MarshalAs(UnmanagedType.LPStr)]
            public string errorText;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PaDeviceInfo
        {
            public string name => _name == IntPtr.Zero
                ? null
                : Marshal.PtrToStringAnsi(_name);

            public int structVersion;
            private IntPtr _name;
            // [MarshalAs(UnmanagedType.LPStr)]
            // public string name;
            public PaHostApiIndex hostApi;
            public int maxInputChannels;
            public int maxOutputChannels;
            public PaTime defaultLowInputLatency;
            public PaTime defaultLowOutputLatency;
            public PaTime defaultHighInputLatency;
            public PaTime defaultHighOutputLatency;
            public double defaultSampleRate;
        }

        public struct PaDeviceInfoEx
        {
            internal PaDeviceInfoEx(
                int _index, PaDeviceInfo inf, int apiIndex, int devIndex)
            {
                deviceInfo = inf;
                _name = inf.name;
                index = _index;
                APIIndex = apiIndex;
                DeviceIndex = devIndex;
            }

            internal PaDeviceInfoEx(String argName, int argIndex)
            {
                _name = argName;
                index = argIndex;
                DeviceIndex = argIndex;
                APIIndex = argIndex;
                deviceInfo = new PaDeviceInfo();
            }

            private string _name;
            public PaDeviceInfo deviceInfo;
            int index;
            // as given by PA_HostApiDeviceIndexToDeviceIndex()
            int DeviceIndex;
            int APIIndex;
            public string Name
            {
                get
                {
                    if (String.IsNullOrEmpty(_name))
                    {
                        _name = deviceInfo.name;
                    }
                    return _name;
                }
            }

            public int Index
            {
                get { return index; }
            }

            // this is what is shown in the VAC combos
            public override string ToString() { return Name; }
        }

        [StructLayout(LayoutKind.Sequential)]
        unsafe public struct PaStreamParameters
        {
            public PaDeviceIndex device;
            public int channelCount;
            public PaSampleFormat sampleFormat;
            public PaTime suggestedLatency;
            public void* hostApiSpecificStreamInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PaStreamCallbackTimeInfo
        {
            public PaTime inputBufferAdcTime;
            public PaTime currentTime;
            public PaTime outputBufferDacTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PaStreamInfo
        {
            public int structVersion;
            public PaTime inputLatency;
            public PaTime outputLatency;
            public double sampleRate;
        }

        #endregion

        #region Function Definitions

#pragma warning disable CS0414 // The field 'PortAudioForThetis.PaNoError' is
        // assigned but its value is never used
        static readonly int PaNoError = 0;
#pragma warning restore CS0414 // The field 'PortAudioForThetis.PaNoError' is
        // assigned but its value is never used
        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_GetVersion",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int PA_GetVersion();

        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_GetVersionText",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern String PA_GetVersionText();

        // note that using the stock source and calling this function
        // on errorCode = 0 will result in an Exception (no object
        // reference.  To fix this, I added a single statement in
        // pa_front.c.  The new line 444 is below.
        // case paNoError:                  result = "1"; result = "Success"; break;
        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_GetErrorText",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IntPtr_PA_GetErrorText(PaError error);

        public static string PA_GetErrorText(PaError error)
        {
            IntPtr strptr = IntPtr_PA_GetErrorText(error);
            return Marshal.PtrToStringAnsi(strptr);
        }

        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_Initialize",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern PaError PA_Initialize();

        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_Terminate",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern PaError PA_Terminate();

        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_GetHostApiCount",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern PaHostApiIndex PA_GetHostApiCount();

        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_GetDefaultHostApi",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern PaHostApiIndex PA_GetDefaultHostApi();

        [DllImport("PortAudioForCoolSDR.dll",
            EntryPoint = "Pa_GetDefaultInputDevice",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern PaHostApiIndex PA_GetDefaultInputDevice();

        [DllImport("PortAudioForCoolSDR.dll",
            EntryPoint = "Pa_GetDefaultOutputDevice",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern PaHostApiIndex PA_GetDefaultOutputDevice();

        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_IsFormatSupported2",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern PaError Pa_IsFormatSupported(int hostAPI,
            double samplerate, int deviceIndexIn, int deviceIndexOut, int channels,
            PaSampleFormat fmt, int exclusive);

        // Added layer to convert from the struct pointer to a C#
        // struct automatically.
        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_GetHostApiInfo",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PA_GetHostApiInfoPtr(int hostId);
        public static PaHostApiInfo PA_GetHostApiInfo(int hostId)
        {
            IntPtr ptr = PA_GetHostApiInfoPtr(hostId);
            PaHostApiInfo info
                = (PaHostApiInfo)Marshal.PtrToStructure(ptr, typeof(PaHostApiInfo));
            return info;
        }

        [DllImport("PortAudioForCoolSDR.dll",
            EntryPoint = "Pa_HostApiDeviceIndexToDeviceIndex",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern PaDeviceIndex PA_HostApiDeviceIndexToDeviceIndex(
            int hostAPI, int hostApiDeviceIndex);

        [DllImport("PortAudioForCoolSDR.dll",
            EntryPoint = "Pa_GetLastHostErrorInfo",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PA_GetLastHostErrorInfoPtr();
        public static PaHostErrorInfo PA_GetLastHostErrorInfo()
        {
            IntPtr ptr = PA_GetLastHostErrorInfoPtr();
            PaHostErrorInfo info = (PaHostErrorInfo)Marshal.PtrToStructure(
                ptr, typeof(PaHostErrorInfo));
            return info;
        }

        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_GetDeviceCount",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern PaDeviceIndex PA_GetDeviceCount();

        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_GetDeviceInfo",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PA_GetDeviceInfoPtr(int device);
        public static PaDeviceInfo PA_GetDeviceInfo(int device)
        {
            IntPtr ptr = PA_GetDeviceInfoPtr(device);
            PaDeviceInfo info
                = (PaDeviceInfo)Marshal.PtrToStructure(ptr, typeof(PaDeviceInfo));
            return info;
        }

        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_GetStreamInfo",
            CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern IntPtr PA_GetStreamInfoPtr(void* stream);
        unsafe public static PaStreamInfo PA_GetStreamInfo(void* stream)
        {
            IntPtr ptr = PA_GetStreamInfoPtr(stream);
            PaStreamInfo info
                = (PaStreamInfo)Marshal.PtrToStructure(ptr, typeof(PaStreamInfo));
            return info;
        }

        [DllImport("PortAudioForCoolSDR.dll", EntryPoint = "Pa_Sleep",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern void PA_Sleep(int msec);

        #endregion // Function Definitions
    }
}
