/*  frmBandwidth.cs

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
using System.Windows.Forms;

namespace Thetis
{
    public partial class frmBandwidth : Form
    {
        public frmBandwidth()
        {
            InitializeComponent();
            timerReadBandwidth.Interval = 500;
            timerReadBandwidth.Enabled = false;
            ucBandwidthView.SmoothingFactor = 0.7;
            ucBandwidthView.EnableSmoothing = true;
        }

        private void timerReadBandwidth_Tick(object sender, EventArgs e)
        {
            double out_bps = NetworkIO.GetOutboundBps();
            double in_bps = NetworkIO.GetInboundBps();
            ucBandwidthView.PushSample(in_bps, out_bps);
        }

        public void RecoverShow()
        {
            Common.RestoreForm(this, this.Name, true);
            timerReadBandwidth.Enabled = true;
            this.Show();
        }

        private void frmBandwidth_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                timerReadBandwidth.Enabled = false;
                this.Hide();
                ucBandwidthView.Reset();
                e.Cancel = true;
            }

            Common.SaveForm(this, this.Name);
        }

        private void radUnits_CheckedChanged(object sender, EventArgs e)
        {
            ucBandwidthView.DisplayUnits = radKB.Checked ? ucBandwidthView.BandwidthUnits.KBps : ucBandwidthView.BandwidthUnits.Mbitps;
        }
    }
}
