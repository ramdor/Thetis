using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thetis
{
    internal class HL2
    {

        internal HL2(Console c, Setup s)
        {
            console = c;
            setup = s;
        }

        // KLJ NOTE: this is just a flag. To actually do anything you need to call ApplyHL2Defaults()
        public bool HermesLite2
        {
            get;
            set;
        }

        public Console console
        {
            get;
            private set;
        }
        public Setup setup
        {
            get;
            private set;
        }

        private void ApplyHL2PASettings()
        {
            setup.Hercules = true; // n2adr filter
            setup.HermesLite2 = true;
            decimal d = 38.8M;
            setup.setAllHFPAGains(d);
        }

        public void ApplyHL2Defaults(bool canDoN2ADR)
        {
            ApplyHL2PASettings();
            setup.ApolloFilter = true;
            setup.ApolloTuner = true;
            setup.ApolloPresent = true;
            setup.HermesEnableAttenuator = true;
            setup.MaxFreq = 38.8M;
            // setup.RadioSampleRate = 48000; // no need for this, HL2 is quite happy @ 192k
            if (canDoN2ADR)
                setup.HL2N2ADRFilters(this);
            setup.HermesLite2 = true;
        }


    }
}
