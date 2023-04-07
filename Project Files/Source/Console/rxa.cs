using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
//using WeifenLuo.WinFormsUI.Docking;

namespace Thetis
{

    unsafe public partial class rxa : Form
    {
        private int fwid;
        private int stid;
        private int chid;
        public bool update = false;
        private Size console_basis_size = new Size(100, 100);
        private Size gr_display_size_basis = new Size(100, 100);
        private Size pic_display_size_basis = new Size(100, 100);
        // rxaControls rxControls;

        public rxa(int i)
        {
            InitializeComponent();
            fwid = i;                                                       // firmware id
            stid = i - 2;                                                   // ChannelMaster stream id
            chid = 2 * stid;
#if (USE_PANDISPLAY)// WDSP channel id
            panDisplay.init = true;
            panDisplay.DisplayID = stid;
#endif
            this.Text = "Using Rx" + fwid.ToString();
            this.Name = "rxa" + fwid.ToString();
            create_rxa();
            Common.RestoreForm(this, this.Name, false);
            ForceRxa();
            console_basis_size = this.Size;
#if (USE_PANDISPLAY)
            gr_display_size_basis = this.panelPanDisplay.Size;
            pic_display_size_basis = this.panDisplay.Size;
#endif

            //  rxControls = new rxaControls(fwid);
            //dockPanel.ShowDocumentIcon = false;
            //dockPanel.Theme = vS2012LightTheme1;
            //  rxControls.Show(dockPanel, DockState.DockRightAutoHide);
            //int width = dockPanel.GetDockWindowSize(DockState.DockRight);
        }

        private void create_rxa()
        {
            // JanusAudio setup
            NetworkIO.SetDDCRate(fwid, 48000);                       // set receivers samplerate to 48K
            NetworkIO.EnableRx(fwid, 1);                                   // enable the receiver

            // ChannelMaster setup
            cmaster.SetXcmInrate(stid, 48000);                              // tell ChannelMaster the sample rate is 48K
            cmaster.SetRunPanadapter(stid, true);                          // turn off the panadapter computation
            cmaster.SetAAudioMixState((void*)0, 0, chid, true);             // add main-rcvr to mixer state
            cmaster.SetAAudioMixWhat((void*)0, 0, chid, true);             // add main-rcvr to mixer what

            // WDSP setup
            WDSP.SetChannelState(chid + 0, 1, 0);                           // main rcvr ON
            WDSP.SetChannelState(chid + 1, 0, 0);                           // sub-rcvr OFF
        }

        private void ForceRxa()
        {
            EventArgs e = EventArgs.Empty;
            udRXAAGCGain_ValueChanged(this, e);
            udRXAVolume_ValueChanged(this, e);
            udRXAMode_ValueChanged(this, e);
#if (USE_PANDISPLAY)
            panDisplay.initAnalyzer();
#endif
            udRXAFreq_ValueChanged(this, e);
        }

        public long RXFreq
        {
            get { return (long)((double)udRXAFreq.Value * 1e6); }
            set
            {
                udRXAFreq.Value = (decimal)(value * 1e-6);
            }
        }

        private void udRXAFreq_ValueChanged(object sender, EventArgs e)
        {
            NetworkIO.SetVFOfreq(fwid, NetworkIO.Freq2PW((int)(1000000.0 * (double)udRXAFreq.Value)), 0); // sending phaseword to firmware         
#if (USE_PANDISPLAY)
            panDisplay.VFOHz = RXFreq;
#endif
        }

        private void udRXAAGCGain_ValueChanged(object sender, EventArgs e)
        {
            WDSP.SetRXAAGCTop(chid, (double)udRXAAGCGain.Value);
        }

        private void udRXAVolume_ValueChanged(object sender, EventArgs e)
        {
            WDSP.SetRXAPanelGain1(chid, 0.01 * (double)udRXAVolume.Value);
        }

        private void udRXAMode_ValueChanged(object sender, EventArgs e)
        {
            int v = (int)udRXAMode.Value;
            DSPMode mode = DSPMode.LSB;
            switch (v)
            {
                case 0:
                    mode = DSPMode.LSB;
                    WDSP.SetRXABandpassFreqs(chid, -3100.0, -200.0);
                    WDSP.RXANBPSetFreqs(chid, -3100.0, -200.0);
                    WDSP.SetRXASNBAOutputBandwidth(chid, -3100.0, -200.0);
                    break;
                case 1:
                    mode = DSPMode.USB;
                    WDSP.SetRXABandpassFreqs(chid, +200.0, +3100.0);
                    WDSP.RXANBPSetFreqs(chid, +200.0, +3100.0);
                    WDSP.SetRXASNBAOutputBandwidth(chid, +200.0, +3100.0);
                    break;
                case 10:
                    mode = DSPMode.SAM;
                    WDSP.SetRXABandpassFreqs(chid, -10000.0, +10000.0);
                    WDSP.RXANBPSetFreqs(chid, -10000.0, +10000.0);
                    WDSP.SetRXASNBAOutputBandwidth(chid, -10000.0, +10000.0);
                    break;
            }
            WDSP.SetRXAMode(chid, (DSPMode)mode);
        }

        private void rxa_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, this.Name);
        }

#if (USE_PANDISPLAY)
        public PanDisplay pDisplay
        {
            get { return this.panDisplay; }
            set { this.panDisplay = value; }
        }

        private void panDisplay_Resize(object sender, EventArgs e)
        {
            //if (this.Width < console_basis_size.Width)
            //{
            //    this.Width = console_basis_size.Width;
            //    return;
            //}
            //if (this.Height < console_basis_size.Height)
            //{
            //    this.Height = console_basis_size.Height;
            //    return;
            //}

            //int h_delta = this.Width - console_basis_size.Width;
            //int v_delta = Math.Max(this.Height - console_basis_size.Height, 0);

            //panDisplay.pauseDisplayThread = true;
            //panelPanDisplay.Size = new Size(gr_display_size_basis.Width + h_delta, gr_display_size_basis.Height + v_delta);

            //panDisplay.Init();
            //panDisplay.UpdateGraphicsBuffer();
            //panDisplay.pauseDisplayThread = false;
        }
#endif

        private void rxa_Resize(object sender, EventArgs e)
        {
            if (this.Width < console_basis_size.Width)
            {
                this.Width = console_basis_size.Width;
                return;
            }
            if (this.Height < console_basis_size.Height)
            {
                this.Height = console_basis_size.Height;
                return;
            }

            int h_delta = this.Width - console_basis_size.Width;
            int v_delta = Math.Max(this.Height - console_basis_size.Height, 0);

#if (USE_PANDISPLAY)
            panDisplay.pauseDisplayThread = true;
            panelPanDisplay.Size = new Size(gr_display_size_basis.Width + h_delta, gr_display_size_basis.Height + v_delta);
            panDisplay.Size = new Size(pic_display_size_basis.Width + h_delta, pic_display_size_basis.Height + v_delta);

            panDisplay.Init();
            panDisplay.UpdateGraphicsBuffer();
            panDisplay.pauseDisplayThread = false;
#endif

        }

        //private void dockPanel_ActiveAutoHideContentChanged(object sender, EventArgs e)
        //{
        //    if (dockPanel.ActiveAutoHideContent != null)
        //    panDisplay.pauseDisplayThread = true;
        //    else panDisplay.pauseDisplayThread = false;
        //}

    }
}
