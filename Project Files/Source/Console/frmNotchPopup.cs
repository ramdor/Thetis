/*  clsNotchPopup.cs

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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Thetis
{
    public partial class frmNotchPopup : Form
    {
        private int _notch_index;

        public delegate void NotchDeleteHandler(int notch_index);
        private event NotchDeleteHandler deleteEvents;

        public delegate void NotchBWChangeHandler(int notch_index, double width);
        private event NotchBWChangeHandler bwChangeEvents;

        public delegate void NotchActiveChangedHandler(int notch_index, bool active);
        private event NotchActiveChangedHandler activeEvents;

        private DateTime _deactivate_time;
        public frmNotchPopup()
        {
            _notch_index = -1;

            InitializeComponent();

            _deactivate_time = DateTime.UtcNow;
        }
        public DateTime DeactivateTime
        {
            get { return _deactivate_time; }
        }
        public void Show(MNotch notch, int minWidth, int maxWidth, bool top, int notch_index = -1)
        {
            _deactivate_time = DateTime.UtcNow;

            // init with the passed notch
            if (notch == null) return;  // Todo initialise empty?

            _notch_index = notch_index;

            if (top) //MW0LGE_21k9
            {
                Win32.SetWindowPos(this.Handle.ToInt32(),
                    -1, this.Left, this.Top, this.Width, this.Height, 0);
            }
            else
            {
                Win32.SetWindowPos(this.Handle.ToInt32(),
                    -2, this.Left, this.Top, this.Width, this.Height, 0);
            }

            trkWidth.Minimum = minWidth;
            if ((int)notch.FWidth > maxWidth)
            {   // this copes with filters that have been dragged out really wide
                trkWidth.Maximum = (int)notch.FWidth;
            }
            else
            {
                // use passed in maxWidth
                trkWidth.Maximum = maxWidth;
            }

            trkWidth.TickFrequency = 10;
            trkWidth.TickStyle = TickStyle.None;


            trkWidth.Value = (int)notch.FWidth;

            chkActive.Checked = notch.Active;

            setText(trkWidth.Value);

            this.Show();
        }
        private void FrmNotchPopup_Deactivate(object sender, EventArgs e)
        {
            _notch_index = -1;

            _deactivate_time = DateTime.UtcNow;
            this.Hide();  // something other than -1 will be done from filter item multi meter
        }
        
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            deleteEvents(_notch_index);

            _notch_index = -1;

            _deactivate_time = DateTime.UtcNow;
            this.Hide();
        }

        public event NotchDeleteHandler NotchDeleteEvent {
            add {
                deleteEvents += value;
            }
            remove {
                deleteEvents -= value;
            }
        }
        public event NotchBWChangeHandler NotchBWChangedEvent {
            add {
                bwChangeEvents += value;
            }
            remove {
                bwChangeEvents -= value;
            }
        }
        public event NotchActiveChangedHandler NotchActiveChangedEvent {
            add {
                activeEvents += value;
            }
            remove {
                activeEvents -= value;
            }
        }

        private void setBW(int width)
        {
            trkWidth.Value = width;
            setText(width);
            bwChangeEvents(_notch_index, width);
        }

        private void Btn25_Click(object sender, EventArgs e)
        {
            setBW(25);
        }
       
        private void Btn50_Click(object sender, EventArgs e)
        {
            setBW(50);
        }

        private void Btn100_Click(object sender, EventArgs e)
        {
            setBW(100);
        }

        private void Btn200_Click(object sender, EventArgs e)
        {
            setBW(200);
        }

        private void TrkWidth_Scroll(object sender, EventArgs e)
        {
            setText(trkWidth.Value);

            bwChangeEvents(_notch_index, trkWidth.Value);
        }

        private void setText(int v)
        {
            lblWidth.Text = v.ToString() + " Hz";
        }

        private void ChkActive_CheckedChanged(object sender, EventArgs e)
        {
            activeEvents(_notch_index, chkActive.Checked);
        }
    }
}
