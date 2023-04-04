using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaSampleFormat = System.UInt32;

namespace Thetis.AudioExtras
{
    internal class AudioExtras { }

    internal enum DeviceIndexes { input, output }

    internal struct PaFormat
    {
        internal PaSampleFormat fmt;
        internal int nch;
        internal int samplerate;
        internal int exclusive;
        internal int apiIndex;
        internal int[] deviceIndexes;

        public PaFormat(PaSampleFormat _fmt, int ch, int sr, int exclusive,
            int apiIndex, int[] deviceIndexes)
        {
            this.fmt = _fmt;
            this.nch = ch;
            this.samplerate = sr;
            this.exclusive = exclusive;
            this.apiIndex = apiIndex;
            this.deviceIndexes = deviceIndexes;
        }
    }

    internal class PortAudioInfo
    {
        private int[] sampleRates = { 22050, 44100, 48000, 96000, 128000, 192000 };
        public string[] deviceNames = new string[2];
        public int[] deviceIndexes = new int[2];
        public PortAudioForThetis.PaDeviceInfo[] deviceInfos
            = new PortAudioForThetis.PaDeviceInfo[2];
        public List<PaFormat> supportedFormats;
        public int apiIndex = -1;
        public string apiName;
        public PortAudioForThetis.PaHostApiInfo apiInfo;

        public int Device(DeviceIndexes index) { return deviceIndexes[(int)index]; }

        public string DeviceName(DeviceIndexes index)
        {
            return deviceNames[(int)index];
        }

        internal PortAudioInfo(int hostAPI, int[] deviceIndexes)
        {
            this.apiIndex = hostAPI;
            Debug.Assert(deviceIndexes.Count() == 2);

            InitHost(hostAPI, deviceIndexes);

            GetSupportedFormats();
        }

        internal PortAudioInfo(int hostAPI, PaFormat fmt)
        {
            Debug.Assert(hostAPI >= 0);
            Debug.Assert(fmt.deviceIndexes.Count() == 2);
            InitHost(hostAPI, deviceIndexes);
            GetSupportedFormats(fmt.fmt, fmt.nch, fmt.exclusive);
        }

        internal PortAudioInfo(int hostAPI, string[] deviceNames)
        {
            Debug.Assert(hostAPI >= 0);
            Debug.Assert(deviceNames.Count() == 2);
            var deviceIndexes = FindDeviceIndexes(hostAPI, deviceNames);
        }

        static internal int DeviceCount()
        {
            return PortAudioForThetis.PA_GetDeviceCount();
        }

        internal int[] FindDeviceIndexes(int hostAPI, string[] deviceNames)
        {
            int[] ret = new int[2];
            var cnt = DeviceCount();
            bool got_input = false;
            bool got_output = false;
            for (int i = 0; i < cnt; ++i)
            {
                var inf = PortAudioForThetis.PA_GetDeviceInfo(i);
                if (inf.hostApi == hostAPI)
                {
                    if (inf.maxInputChannels > 0
                        && inf.name == deviceNames[(int)DeviceIndexes.input])
                    {
                        ret[0] = i;
                        got_input = true;
                    }

                    if (inf.maxOutputChannels > 0
                        && inf.name == deviceNames[(int)DeviceIndexes.output])
                    {
                        ret[1] = i;
                        got_output = true;
                    }
                    if (got_input && got_output)
                    {
                        break;
                    }
                }
            };
            if (!got_input && !got_output)
            {
                return null;
            }
            return ret;
        }

        private void InitHost(int hostAPI, int[] deviceIndexes)
        {
            apiInfo = PortAudioForThetis.PA_GetHostApiInfo(hostAPI);
            apiIndex = hostAPI;
            apiName = apiInfo.name;
            Debug.Assert(deviceIndexes != null);
            Debug.Assert(deviceIndexes.Count() == 2);

            for (int i = 0; i < 2; ++i)
            {
                this.deviceIndexes[i] = deviceIndexes[i];
                this.deviceInfos[i]
                    = PortAudioForThetis.PA_GetDeviceInfo(deviceIndexes[i]);
                this.deviceNames[i] = deviceInfos[i].name;
            }
        }

        public void GetSupportedFormats(uint fmt = PortAudioForThetis.paFloat32,
            int nch = 2, int exclusive = 0)
        {
            Debug.Assert(!string.IsNullOrEmpty(deviceNames[0])
                && !string.IsNullOrEmpty(deviceNames[1]));
            Debug.Assert(apiIndex >= 0 && !string.IsNullOrEmpty(apiName));
            Debug.Assert(apiInfo.deviceCount > 0);
            if (supportedFormats == null)
            {
                supportedFormats = new List<PaFormat>();
            }

            foreach (var sr in sampleRates)
            {
                PortAudioForThetis.PaErrorCode res
                    = (PortAudioForThetis.PaErrorCode)
                          PortAudioForThetis.Pa_IsFormatSupported(apiIndex,
                              (double)sr, deviceIndexes[0], deviceIndexes[1], nch,
                              fmt, exclusive);
                if (res == PortAudioForThetis.PaErrorCode.paNoError)
                {
                    supportedFormats.Add(new PaFormat(
                        fmt, nch, sr, exclusive, apiIndex, deviceIndexes));
                }
                else
                {
                    Debug.Print("device DOES support");
                }
            }
        }
    }

    internal class PortAudioExtras
    {
        internal PortAudioInfo SaneDefaults()
        {
            int[] devices = new int[2];
            devices[0] = PortAudioForThetis.PA_GetDefaultInputDevice();
            devices[1] = PortAudioForThetis.PA_GetDefaultOutputDevice();
            PortAudioInfo ret = new PortAudioInfo(
                PortAudioForThetis.PA_GetDefaultHostApi(), devices);

            return ret;
        }

        internal bool IsFormatSupported(int hostAPI, PaFormat pafmt)
        {
            PortAudioInfo pai = new PortAudioInfo(hostAPI, pafmt);
            foreach (var fmt in pai.supportedFormats)
            {
                if (fmt.fmt == pafmt.fmt && fmt.nch == pafmt.nch
                    && fmt.samplerate == pafmt.samplerate)
                {
                    return true;
                }
            }
            return false;
        }

        internal List<PaFormat> ListFormats(int hostAPI, string[] deviceNames)
        {
            Debug.Assert(deviceNames.Count() == 2);
            Debug.Assert(hostAPI > 0);
            var inf = new PortAudioInfo(hostAPI, deviceNames);
            var ret = new List<PaFormat>();
            return ret;
        }
    }

    internal class Tests
    {
#if (DEBUG)
        public static void TestSaneDefaults()
        {

            var extras = new PortAudioExtras();
            var defs = extras.SaneDefaults();
            Debug.Assert(defs.apiInfo.deviceCount > 0);
            Debug.Assert(!(String.IsNullOrEmpty(defs.deviceNames[0])));
            Debug.Assert(!(String.IsNullOrEmpty(defs.deviceNames[1])));
            Debug.Assert(defs.supportedFormats.Count() > 0);
        }
#else
    public static void TestSaneDefaults() {}
#endif
    }
}
