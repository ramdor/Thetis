using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaSampleFormat = System.UInt32;

namespace Thetis.Extras
{
    internal class Extras { }

    internal enum PortAudioInfoIndexes { input, output }

    internal struct PaFormat
    {
        internal PaSampleFormat fmt;
        internal int nch;
        internal int samplerate;

        public PaFormat(PaSampleFormat _fmt, int ch, int sr)
        {
            this.fmt = _fmt;
            this.nch = ch;
            this.samplerate = sr;
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

        internal PortAudioInfo(int hostAPI, int[] deviceIndexes)
        {
            this.apiIndex = hostAPI;
            Debug.Assert(deviceIndexes.Count() == 2);

            apiInfo = PortAudioForThetis.PA_GetHostApiInfo(hostAPI);

            for (int i = 0; i < 2; ++i)
            {
                this.deviceIndexes[i] = deviceIndexes[i];
                deviceInfos[i]
                    = PortAudioForThetis.PA_GetDeviceInfo(deviceIndexes[i]);
                deviceNames[i] = deviceInfos[i].name;
            }
        }

        public void GetSupportedFormats(uint fmt = PortAudioForThetis.paFloat32,
            int nch = 2, int exclusive = 0)
        {
            Debug.Assert(!string.IsNullOrEmpty(deviceNames[0])
                && !string.IsNullOrEmpty(deviceNames[1]));
            Debug.Assert(apiIndex >= 0 && !string.IsNullOrEmpty(apiName));
            Debug.Assert(apiInfo.deviceCount > 0);

            foreach (var sr in sampleRates)
            {
                PortAudioForThetis.PaErrorCode res
                    = (PortAudioForThetis.PaErrorCode)
                          PortAudioForThetis.Pa_IsFormatSupported(apiIndex,
                              (double)sr, deviceIndexes[0], deviceIndexes[1], nch,
                              fmt, exclusive);
                if (res != PortAudioForThetis.PaErrorCode.paNoError)
                {
                    Debug.Print("device supports!");
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
    }

#if (DEBUG)
    internal class Tests
    {
        public static void TestSaneDefaults()
        {

            var extras = new PortAudioExtras();
            var defs = extras.SaneDefaults();
            Debug.Assert(defs.apiInfo.deviceCount > 0);
            Debug.Assert(!(String.IsNullOrEmpty(defs.deviceNames[0])));
            Debug.Assert(!(String.IsNullOrEmpty(defs.deviceNames[1])));
            Debug.Assert(defs.supportedFormats.Count() > 0);
        }
    }
#endif
}
