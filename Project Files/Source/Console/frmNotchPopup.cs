/*  clsNotchPopup.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2024 Richard Samphire MW0LGE

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
using System;
using System.Windows.Forms;

namespace Thetis
{
    public partial class frmNotchPopup : Form
    {
        public delegate void NotchDeleteHandler();
        private event NotchDeleteHandler deleteEvents;

        public delegate void NotchBWChangeHandler(double width);
        private event NotchBWChangeHandler bwChangeEvents;

        public delegate void NotchActiveChangedHandler(bool active);
        private event NotchActiveChangedHandler activeEvents;

        public frmNotchPopup()
        {
            InitializeComponent();           
        }

        public void Show(MNotch notch, int minWidth, int maxWidth, bool top)
        {
            // init with the passed notch
            if (notch == null) return;  // Todo initialise empty?

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
            this.Hide();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            deleteEvents();

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
            bwChangeEvents(width);
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

            bwChangeEvents(trkWidth.Value);
        }

        private void setText(int v)
        {
            lblWidth.Text = v.ToString() + " Hz";
        }

        private void ChkActive_CheckedChanged(object sender, EventArgs e)
        {
            activeEvents(chkActive.Checked);
        }
    }
}
