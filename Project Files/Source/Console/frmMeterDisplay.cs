/*  frmMeterDisplay.cs

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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Thetis
{
    public partial class frmMeterDisplay : Form
    {
        private Console _console;
        private int _rx;
        private string _id;
        private bool _container_minimises = true;
        private bool _container_hides_when_rx_not_used = true;
        private bool _is_enabled = true;
        private bool _floating = false;
        private bool _rx2_enabled;

        public frmMeterDisplay(Console c, int rx)
        {
            InitializeComponent();

            this.MinimumSize = new Size(ucMeter.MIN_CONTAINER_WIDTH, ucMeter.MIN_CONTAINER_HEIGHT);

            _id = System.Guid.NewGuid().ToString();
            _console = c;
            _rx = rx;
            _rx2_enabled = _console.RX2Enabled;

            Common.DoubleBufferAll(this, true);

            _console.WindowStateChangedHandlers += OnWindowStateChanged;
            _console.RX2EnabledChangedHandlers += OnRX2Enabled;

            setTitle();
        }
        public int RX
        {
            get { return _rx; }
            set { _rx = value; }
        }
        private void OnRX2Enabled(bool enabled)
        {
            _rx2_enabled = enabled;
        }
        public bool Floating
        {
            get { return _floating; }
            set { _floating = value; }
        }
        public bool FormEnabled
        {
            get { return _is_enabled; }
            set { _is_enabled = value; }
        }
        public bool ContainerMinimises
        {
            get { return _container_minimises; }
            set { _container_minimises = value; }
        }
        public bool ContainerHidesWhenRXNotUsed
        {
            get { return _container_hides_when_rx_not_used; }
            set { _container_hides_when_rx_not_used = value; }
        }
        private void OnWindowStateChanged(FormWindowState state)
        {
            if (this.Disposing || this.IsDisposed) return;

            if (_is_enabled && _floating && _container_minimises)
            {
                if (state == FormWindowState.Minimized)
                {
                    this.Hide();
                }
                else
                {
                    switch (_rx)
                    {
                        case 1:
                            this.Show();
                            break;
                        case 2:
                            if (_rx2_enabled || !_container_hides_when_rx_not_used) this.Show();
                            break;
                    }
                }
            }
        }
        private void setTitle()
        {
            // so each meter title is 'unique'. Useful for streaming software such as OBS
            this.Text = "Thetis Meter [" + Common.FiveDigitHash(_id).ToString("00000") + "]";
        }
        public string ID
        {
            get { return _id; }
            set 
            {
                _id = value;

                Common.RestoreForm(this, "MeterDisplay_" + _id, true);
                Common.ForceFormOnScreen(this);

                setTitle();
            }
        }
        private void frmMeterDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
            Common.SaveForm(this, "MeterDisplay_" + _id);
        }

        public void TakeOwner(ucMeter m)
        {
            _container_minimises = m.ContainerMinimises;
            _is_enabled = m.MeterEnabled;

            m.Parent = this;
            m.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            m.Location = new Point(0, 0);            
            m.Size = new Size(this.Width, this.Height);
            m.BringToFront();
            m.Show();
        }
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //IMPORTANT NOTE *****   //[2.10.3.7]MW0LGE
            //This crazy looking code that moves the window by 1 pixel X and then back again
            //causes windows to reculate the window size based on the DPI of the monitor it is on.
            //Otherwise we would need to enable Per Monitor DPI support in Thetis which breaks a
            //couple of things. Containers should now launch correctly scaled when on a secondary monitor
            //that is at a different UI scaling/resolution compared to the main Thetis window.

            IntPtr handle = this.Handle;

            Rectangle bounds = this.Bounds;
            int originalX = bounds.X;
            int originalY = bounds.Y;

            // Move the window by 1 pixel
            SetWindowPos(handle, IntPtr.Zero, originalX + 1, originalY, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

            // Move it back to the original position
            SetWindowPos(handle, IntPtr.Zero, originalX, originalY, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }
    }
}
