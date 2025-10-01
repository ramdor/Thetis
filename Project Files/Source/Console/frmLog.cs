/*  frmLog.cs

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
    public partial class frmLog : Form
    {
        private const int MAX_ENTRIES = 500;

        private bool _log = false;
        private Object _lock = new Object();

        private string[] _logLines;

        public frmLog()
        {
            InitializeComponent();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lock (_lock)
            {
                _logLines = null;
            }
            txtLog.Lines = _logLines;
        }

        private void chkLog_CheckedChanged(object sender, EventArgs e)
        {
            _log = chkLog.Checked;
            if (_log)
            {
                lock (_lock)
                {
                    _logLines = null; // clear on start
                }
                txtLog.Lines = _logLines;
            }
        }

        public void Log(bool bIn, string sMessage)
        {
            if (!_log) return;

            string[] newLines;

            lock (_lock)
            {
                if (_logLines == null) _logLines = new string[0];

                if (_logLines.Length + 1 > MAX_ENTRIES)
                    newLines = new string[MAX_ENTRIES];
                else
                    newLines = new string[_logLines.Length + 1];

                for (int i = 1; i < newLines.Length; i++)
                {
                    newLines[i] = _logLines[i - 1];
                }

                newLines[0] = (bIn ? "< " : "> ") + sMessage;

                _logLines = newLines;
            }

            if (_log)
                txtLog.Lines = _logLines;
        }

        public void ShowWithTitle(string title)
        {
            this.Text = "Log: " + title;
            this.Show();
        }

        private void frmLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }
    }
}
