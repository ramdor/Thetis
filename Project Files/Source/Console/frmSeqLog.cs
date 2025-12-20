/*  frmSeqLog.cs

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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Thetis
{
    public partial class frmSeqLog : Form
    {
        private int m_nTabMainTop;

        public delegate void ClearButtonHandler();
        private event ClearButtonHandler clearButtonEvents;

        public frmSeqLog()
        {
            InitializeComponent();

            m_nTabMainTop = tabMain.Top;

            btnClear.Text = "Close";
            btnCopyImageToClipboard.Enabled = false;
            btnCopyToClipboard.Enabled = false;

            tabMain.SelectedIndex = 0;
        }

        public void InitAndShow()
        {
            tabMain.SelectedIndex = 0;

            this.Show();
        }

        public void SetWireSharkPath(string sPath)
        {
            setupControlsDumpCap(sPath);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            btnClear.Text = "Close";
            btnCopyImageToClipboard.Enabled = false;
            btnCopyToClipboard.Enabled = false;
            txtLog.Text = "";
            log.Clear();
            clearButtonEvents();
            this.Close();
        }

        private StringBuilder log = new StringBuilder();
        public void LogString(string s)
        {
            log.Insert(0, s + System.Environment.NewLine);
            if (log.Length > 16000) log.Remove(16000, log.Length - 16000);

            txtLog.Text = log.ToString();

            btnClear.Text = "Clear Log + Close";
            btnCopyImageToClipboard.Enabled = true;
            btnCopyToClipboard.Enabled = true;
        }

        private void frmSeqLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        public event ClearButtonHandler ClearButtonEvent {
            add {
                clearButtonEvents += value;
            }
            remove {
                clearButtonEvents -= value;
            }
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtLog.Text);
        }

        private void btnCopyImageToClipboard_Click(object sender, EventArgs e)
        {
            using (Bitmap bmp = new Bitmap(txtLog.Width, txtLog.Height))
            {
                txtLog.DrawToBitmap(bmp, new Rectangle(new Point(0, 0), txtLog.Size));
                Clipboard.SetImage(bmp);
            }
        }

        private void btnSetWireSharkFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    setupControlsDumpCap(fbd.SelectedPath);
                }
            }
        }

        private void setupControlsDumpCap(string sPath)
        {
            txtWireSharkFolder.Text = sPath;
            DumpCap.WireSharkPath = sPath;

            bool bExists = DumpCap.DumpCapExists();

            groupDumpCap.Enabled = bExists;
            if (bExists) groupDumpCap.Text = "DumpCap [FOUND]";
                else groupDumpCap.Text = "DumpCap [NOT FOUND]";

            // default always off
            chkDumpCapEnabled.Checked = false;
            DumpCap.Enabled = false;

            udInterface.Value = DumpCap.Interface;
            chkKillOnNegativeOnly.Checked = DumpCap.KillOnNegativeSeqOnly;
            chkClearRingBufferFolderOnRestart.Checked = DumpCap.ClearFolderOnRestart;
        }

        private void udInterface_ValueChanged(object sender, EventArgs e)
        {
            DumpCap.Interface = (int)udInterface.Value;
        }

        private void chkKillOnNegativeOnly_CheckedChanged(object sender, EventArgs e)
        {
            DumpCap.KillOnNegativeSeqOnly = chkKillOnNegativeOnly.Checked;
        }

        private void chkDumpCapEnabled_CheckedChanged(object sender, EventArgs e)
        {
            DumpCap.Enabled = chkDumpCapEnabled.Checked;
        }

        private void chkClearRingBufferFolderOnRestart_CheckedChanged(object sender, EventArgs e)
        {
            DumpCap.ClearFolderOnRestart = chkClearRingBufferFolderOnRestart.Checked;
        }

        private void btnShowDumpCapFolder_Click(object sender, EventArgs e)
        {
            DumpCap.ShowAppPathFolder();
        }

        private void chkStatusBarWarningNegativeOnly_CheckedChanged(object sender, EventArgs e)
        {

        }

        public bool StatusBarWarningOnNegativeOnly {
            get { return chkStatusBarWarningNegativeOnly.Checked; }
            set { chkStatusBarWarningNegativeOnly.Checked = value; }
        }
    }
}
