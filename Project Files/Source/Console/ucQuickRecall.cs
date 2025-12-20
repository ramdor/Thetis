/*  ucQuickRecall.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2025 Richard Samphire MW0LGE

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    [DefaultEvent("ButtonClicked")]
    public partial class ucQuickRecall : UserControl
    {
        private const int m_nWAITDELAY = 4000; // number of ms the frequency has to be constant before it gets added to the head of the list

        private Console m_objConsole;
        private Timer m_objTimerVFOA;
        private Timer m_objTimerVFOAMode;

        private Timer m_objBackgroundColourPinger;

        private double m_dLastVFOAFreq;
        private DSPMode m_lastDSPMode = DSPMode.FIRST;

        private int m_nVFOASelectedIndex = -1;
        private struct QuickInfo
        {
            public double dFrequency;
            public string sFormattedFrequency;
            public DSPMode mode;
        }

        private List<QuickInfo> m_lstVFOAFrequencies;
        private frmQuickRecallPopupList m_frmPopupList;

        private ToolStripDropDown m_popup;
        private ToolStripControlHost m_host;
        private bool m_bMox = false;

        public event EventHandler ButtonClicked;

        public ucQuickRecall()
        {
            InitializeComponent();
            Disposed += OnDispose;

            m_frmPopupList = new frmQuickRecallPopupList();
            m_frmPopupList.TopLevel = false;
            m_frmPopupList.EntrySelectedHandlers += OnEntrySelected;

            // build the popup
            m_host = new ToolStripControlHost(m_frmPopupList);
            m_host.Margin = Padding.Empty;
            m_host.Padding = Padding.Empty;

            m_popup = new ToolStripDropDown();
            m_popup.Margin = Padding.Empty;
            m_popup.Padding = Padding.Empty;
            m_popup.Items.Add(m_host);

            m_popup.Closed += OnPopupClosed;
            //

            m_lstVFOAFrequencies = new List<QuickInfo>();

            m_objTimerVFOA = new Timer();
            m_objTimerVFOA.Tick += new EventHandler(OnVFOATick);
            m_objTimerVFOAMode = new Timer();
            m_objTimerVFOAMode.Tick += new EventHandler(OnVFOAModeTick);


            // timer used to clear of the colour pinger
            m_objBackgroundColourPinger = new Timer();
            m_objBackgroundColourPinger.Tick += new EventHandler(OnBackgroundColourPingerTick);

            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            setButtonBackColour();

            resizeAndReposition();

            lblFlashColour.Visible = false;
        }
        public Button NextButton {
            get { return this.btnNext; }
        }
        public Button ListButton {
            get { return this.btnList; }
        }
        public Button PreviousButton {
            get { return this.btnPrevious; }
        }
        public Console console {
            get { return m_objConsole; }
            set {
                m_objConsole = value;
                if (m_objConsole == null) return;

                m_objConsole.VFOAFrequencyChangeHandlers += OnVFOAChange;
                m_objConsole.ModeChangeHandlers += OnModeChanged;
                m_objConsole.MoxChangeHandlers += OnMoxChanged;
            }
        }
        private void resizeAndReposition()
        {
            // resize to buttons
            this.Size = new Size(this.btnNext.Right + 1, this.btnNext.Bottom + 1);

            // repos the flash
            int nMidOfbtnListX = btnList.Left + (btnList.Width / 2);
            int nMidOfbtnListY = btnList.Top + (btnList.Height / 2);
            Point p = new Point(nMidOfbtnListX - (lblFlashColour.Width / 2), nMidOfbtnListY - (lblFlashColour.Height / 2));
            lblFlashColour.Location = p;
        }
        private void OnPopupClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            this.btnList.Enabled = true;
        }

        private void OnMoxChanged(int rx, bool oldMox, bool newMox)
        {
            m_bMox = newMox; // used to limit store to RX frequencies only

            btnPrevious.Enabled = !m_bMox;
            btnList.Enabled = !m_bMox;
            btnNext.Enabled = !m_bMox;

            //we could cancel them, but it should be ok
            //if (m_bMox)
            //{
            //    // stop any timers
            //    m_objTimerVFOA.Stop();
            //    m_objTimerVFOAMode.Stop();
            //}
        }
        private void OnModeChanged(int rx, DSPMode oldMode, DSPMode newMode, Band oldBand, Band newBand)
        {
            if (m_bMox) return; // dont do anything if we are in TX

            if (rx == 1)
            {
                m_lastDSPMode = newMode;

                //reset timer, when timer ticks update the mode
                m_objTimerVFOAMode.Stop();
                m_objTimerVFOAMode.Interval = m_nWAITDELAY;
                m_objTimerVFOAMode.Start();
            }
            else
            {

            }
        }
        private void OnVFOAChange(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
        {
            if (m_bMox) return; // dont do anything if we are in TX

            m_dLastVFOAFreq = newFreq;

            //reset timer, when timer ticks store new F
            m_objTimerVFOA.Stop();
            m_objTimerVFOA.Interval = m_nWAITDELAY;
            m_objTimerVFOA.Start();
        }

        private void buttonClicked(EventArgs e)
        {
            // used by external to regain focus or other such things
            ButtonClicked?.Invoke(this, e);
        }

        private string formatFrequencyToString(double f)
        {
            // take double, and turn 0.0 into 0.000.000, or 9999.123456123 into 9999.123.456
            string sTmp = f.ToString("0.000000");

            string sLastThree = sTmp.Substring(sTmp.Length - 3, 3);
            string sPreLastThree = sTmp.Substring(0, sTmp.Length - 3);

            return sPreLastThree + "." + sLastThree;
        }

        private int findExistingInVFOA(double f)
        {
            for(int n=0;n< m_lstVFOAFrequencies.Count; n++)
            {
                if (m_lstVFOAFrequencies[n].dFrequency == f) return n;
            }
            return -1;
        }

        private void addVFOAEntry(QuickInfo qi)
        {
            //insert at front
            m_lstVFOAFrequencies.Insert(0, qi);
            if (m_lstVFOAFrequencies.Count > 16) m_lstVFOAFrequencies.RemoveAt(m_lstVFOAFrequencies.Count - 1);
            m_nVFOASelectedIndex = 0;
        }
        
        private void OnBackgroundColourPingerTick(Object sender, EventArgs e)
        {
            m_objBackgroundColourPinger.Stop();

            lblFlashColour.Visible = false;
        }
        private void OnVFOAModeTick(Object sender, EventArgs e)
        {
            m_objTimerVFOAMode.Stop();
            if (m_bMox) return; // dont do anything if we are in TX

            // update the entry if in list
            int index = findExistingInVFOA(m_dLastVFOAFreq);
            if (index != -1)
            {
                if (m_lstVFOAFrequencies[index].mode != m_lastDSPMode)
                {
                    QuickInfo qi = m_lstVFOAFrequencies[index];
                    qi.mode = m_lastDSPMode;
                    m_lstVFOAFrequencies[index] = qi;

                    lblFlashColour.BackColor = Color.Orange;
                    lblFlashColour.Visible = true;

                    m_objBackgroundColourPinger.Interval = 250;
                    m_objBackgroundColourPinger.Start();
                }
            }
        }
        private void OnVFOATick(Object sender, EventArgs e)
        {
            // we have sat for long enough on same frquency, add or select
            m_objTimerVFOA.Stop();
            if (m_bMox) return; // dont do anything if we are in TX

            int existsIndex = findExistingInVFOA(m_dLastVFOAFreq);
            if (existsIndex == -1)
            {
                // not in the list, add it
                QuickInfo qi = new QuickInfo();
                qi.dFrequency = m_dLastVFOAFreq;
                qi.sFormattedFrequency = formatFrequencyToString(m_dLastVFOAFreq);
                qi.mode = m_lastDSPMode;

                addVFOAEntry(qi);

                lblFlashColour.BackColor = Color.LimeGreen;
                lblFlashColour.Visible = true;

                m_objBackgroundColourPinger.Interval = 250;
                m_objBackgroundColourPinger.Start();
            }
            else
            {
                m_nVFOASelectedIndex = existsIndex;
            }

            //update menu if shown !
            if (m_popup.Visible) buildAndShowPopup();
        }

        private void OnDispose(object sender, EventArgs e)
        {
            m_popup.Closed -= OnPopupClosed;
            m_frmPopupList.EntrySelectedHandlers -= OnEntrySelected;

            if (m_objConsole != null)
            {
                m_objConsole.VFOAFrequencyChangeHandlers -= OnVFOAChange;
                m_objConsole.ModeChangeHandlers -= OnModeChanged;
            }
        }
        private void OnEntrySelected(int index)
        {
            m_popup.Hide();
            if (index < 0 || index > m_lstVFOAFrequencies.Count - 1) return;
            m_nVFOASelectedIndex = index;
            selectVFOAEntry(m_nVFOASelectedIndex);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (m_nVFOASelectedIndex == -1 || m_lstVFOAFrequencies.Count <= 1) return;
            m_nVFOASelectedIndex--;
            if (m_nVFOASelectedIndex < 0) m_nVFOASelectedIndex = m_lstVFOAFrequencies.Count - 1;

            selectVFOAEntry(m_nVFOASelectedIndex);

            buttonClicked(EventArgs.Empty);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (m_nVFOASelectedIndex == -1 || m_lstVFOAFrequencies.Count <= 1) return;
            m_nVFOASelectedIndex++;
            if (m_nVFOASelectedIndex > m_lstVFOAFrequencies.Count - 1) m_nVFOASelectedIndex = 0;

            selectVFOAEntry(m_nVFOASelectedIndex);

            buttonClicked(EventArgs.Empty);
        }

        private void selectVFOAEntry(int index)
        {
            if (index < 0 || index > m_lstVFOAFrequencies.Count - 1) return;

            // frequency first, so we get notified of a frequency change first
            console.VFOAFreq = m_lstVFOAFrequencies[index].dFrequency;
            if (m_lstVFOAFrequencies[index].mode != DSPMode.FIRST) console.RX1DSPMode = m_lstVFOAFrequencies[index].mode;
        }
        private void btnList_Click(object sender, EventArgs e)
        {
            // a bit of a hack, but I was unable to stop the popup window from re-appearing
            // The popup closed event happens even before this button event if the popup is shown
            // so if you query if it is visible it will always return false. Turning off
            // btnList click events did not work either. It is related to the autohide property of the
            // popup.
            this.btnList.Enabled = false;

            buildAndShowPopup();

            buttonClicked(EventArgs.Empty);
        }
        private void buildAndShowPopup()
        {
            m_frmPopupList.ClearItems();

            for (int n = 0; n < m_lstVFOAFrequencies.Count; n++)
            {
                int index = m_frmPopupList.AddItem(Math.Round(m_lstVFOAFrequencies[n].dFrequency,6));
                if (m_nVFOASelectedIndex == index) m_frmPopupList.FreqList.SelectedIndex = index;
            }

            m_host.Width = m_frmPopupList.Width;
            m_host.Height = 20 + (m_lstVFOAFrequencies.Count * (m_frmPopupList.FontEntryHeight + 1));
            m_frmPopupList.Height = m_host.Height;

            m_popup.Show(this, new Point((btnList.Left + btnList.Width / 2) - (m_frmPopupList.Width / 2), btnList.Top + btnList.Height));
        }

        private void ucQuickRecall_BackColorChanged(object sender, EventArgs e)
        {
            setButtonBackColour();
        }

        private void setButtonBackColour()
        {
            this.btnPrevious.BackColor = this.BackColor;
            this.btnList.BackColor = this.BackColor;
            this.btnNext.BackColor = this.BackColor;
        }

        private void ucQuickRecall_Resize(object sender, EventArgs e)
        {
            resizeAndReposition();
        }

        private void lblFlashColour_Click(object sender, EventArgs e)
        {
            // if we get a click, pass it on through to the button below
            if(btnList.Enabled) btnList_Click(sender, e);
        }
    }
}
