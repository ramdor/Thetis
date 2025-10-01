/*  frmReleaseNotes.cs

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
using System.IO;
using Markdig;

namespace Thetis
{
    public partial class frmReleaseNotes : Form
    {
        private string _releaseNotesPath;

        public frmReleaseNotes()
        {
            InitializeComponent();
            webBrowser1.Navigating += WebBrowser1_Navigating;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void InitPath(string directoryPath)
        {
            _releaseNotesPath = directoryPath;//Path.Combine(directoryPath, "ReleaseNotes.txt");
        }

        public void ShowReleaseNotes()
        {
            try
            {
                if (this.Visible) return;
                Opacity = 0f;
                string file = Path.Combine(_releaseNotesPath, "ReleaseNotes.txt");
                if (!File.Exists(file)) return;
                string releaseNotesText = File.ReadAllText(file);
                string formattedText = Markdown.ToHtml(releaseNotesText);
                string finalHtml = $"<html><head><style>body{{font-family: Arial, sans-serif; background-color: black; color: white;}}</style></head><body>{formattedText}</body></html>";
                webBrowser1.DocumentText = finalHtml;
                Show();
                Common.FadeIn(this);
            }
            catch(Exception ex) 
            {
                MessageBox.Show("Issue showing Release Notes",
                     ex.Message,
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }
        }
        private void WebBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.ToString().ToLower() == "about:blank") return;
            e.Cancel = true;
            string url = e.Url.ToString();
            Common.OpenUri(url);
        }

        private void frmReleaseNotes_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }
    }
}
