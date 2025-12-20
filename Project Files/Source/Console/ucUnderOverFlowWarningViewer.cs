/*  ucUnderOverFlowWarningViewer.cs

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
    public partial class ucUnderOverFlowWarningViewer : UserControl
    {
        public event EventHandler ClearIssuesClick;

        private bool[] _hasHadIssues;
        private Color _OutOverflowsColour = Color.Transparent;
        private Color _OutUnderflowsColour = Color.Transparent;
        private Color _InOverflowsColour = Color.Transparent;
        private Color _InUnderflowsColour = Color.Transparent;
        private bool _noFade = false;

        public ucUnderOverFlowWarningViewer()
        {
            InitializeComponent();

            _hasHadIssues = new bool[4];
        }

        private void UnderOverFlowWarningViewer_Load(object sender, EventArgs e)
        {
            //this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            tmrFade.Enabled = false;

            clearIssues();
        }

        private void setColours(bool forceUpdate = false)
        {
            bool update = false;
            for(int i = 0; i < _hasHadIssues.Length; i++)
            {
                switch(i)
                {
                    case 0:
                        if (_hasHadIssues[i])
                        {
                            _OutOverflowsColour = Color.Red;
                            update = true;
                        }
                        break;
                    case 1:
                        if (_hasHadIssues[i])
                        {
                            _OutUnderflowsColour = Color.Red;
                            update = true;
                        }
                        break;
                    case 2:
                        if (_hasHadIssues[i])
                        {
                            _InOverflowsColour = Color.Lime;
                            update = true;
                        }
                        break;
                    case 3:
                        if (_hasHadIssues[i])
                        {
                            _InUnderflowsColour = Color.Lime;
                            update = true;
                        }
                        break;
                }
                _hasHadIssues[i] = false;
            }

            if (update || forceUpdate)
            {
                tmrFade.Enabled = !_noFade;
                this.Invalidate();
            }
        }
        private void clearIssues()
        {
            tmrFade.Enabled = false;

            for (int i = 0; i < _hasHadIssues.Length; i++)
                _hasHadIssues[i] = false;

            _OutOverflowsColour = Color.Transparent;
            _OutUnderflowsColour = Color.Transparent;
            _InOverflowsColour = Color.Transparent;
            _InUnderflowsColour = Color.Transparent;

            setColours(true);
        }
        public bool OutOverflow
        {
            set 
            { 
                _hasHadIssues[0] = true;
                setColours();
            }
        }
        public bool OutUnderflow
        {
            set 
            { 
                _hasHadIssues[1] = true;
                setColours();
            }
        }
        public bool InOverflow
        {
            set 
            { 
                _hasHadIssues[2] = true;
                setColours();
            }
        }
        public bool InUnderflow
        {
            set 
            { 
                _hasHadIssues[3] = true;
                setColours();
            }
        }
        private Color fadeBackground(Color c)
        {
            if (c == Color.Transparent) return Color.Transparent;

            float a;
            Color outColour;

            a = (float)c.A;

            a *= 0.9f;

            if (a < 16)
            {
                outColour = Color.Transparent;
            }
            else
            {
                outColour = Color.FromArgb((int)a, c.R, c.G, c.B);
            }

            return outColour;
        }
        private void tmrFade_Tick(object sender, EventArgs e)
        {
            _OutOverflowsColour = fadeBackground(_OutOverflowsColour);
            _OutUnderflowsColour = fadeBackground(_OutUnderflowsColour);
            _InOverflowsColour = fadeBackground(_InOverflowsColour);
            _InUnderflowsColour = fadeBackground(_InUnderflowsColour);

            if (_OutOverflowsColour == Color.Transparent &&
                _OutUnderflowsColour == Color.Transparent &&
                _InOverflowsColour == Color.Transparent &&
                _InUnderflowsColour == Color.Transparent
                )
            {
                tmrFade.Enabled = false;

                for (int i = 0; i < _hasHadIssues.Length; i++)
                    _hasHadIssues[i] = false;
            }

            this.Invalidate();
        }

        private void UnderOverFlowWarningViewer_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            SolidBrush brushOutOverflowsColour = new SolidBrush(_OutOverflowsColour);
            SolidBrush brushOutUnderflowsColour = new SolidBrush(_OutUnderflowsColour);
            SolidBrush brushInOverflowsColour = new SolidBrush(_InOverflowsColour);
            SolidBrush brushInUnderflowsColour = new SolidBrush(_InUnderflowsColour);

            int maxA = Math.Max(brushOutOverflowsColour.Color.A, brushOutUnderflowsColour.Color.A);
            maxA = Math.Max(maxA, brushInOverflowsColour.Color.A);
            maxA = Math.Max(maxA, brushInUnderflowsColour.Color.A);

            using (Pen pen = new Pen(Color.FromArgb(maxA, 32, 32, 32), 1)) // fade that matches max A
            {
                //v lines
                g.DrawLine(pen, new Point(0, 0), new Point(0, 14));
                g.DrawLine(pen, new Point(7, 0), new Point(7, 14));
                g.DrawLine(pen, new Point(14, 0), new Point(14, 14));
                //h lines
                g.DrawLine(pen, new Point(0, 0), new Point(14, 0));
                g.DrawLine(pen, new Point(0, 7), new Point(14, 7));
                g.DrawLine(pen, new Point(0, 14), new Point(14, 14));               
            }

            // rectangles
            Rectangle r = new Rectangle(1, 1, 6, 6);
            g.FillRectangle(brushOutOverflowsColour, r);
            r.Location = new Point(1, 8);
            g.FillRectangle(brushOutUnderflowsColour, r);
            r.Location = new Point(8, 1);
            g.FillRectangle(brushInOverflowsColour, r);
            r.Location = new Point(8, 8);
            g.FillRectangle(brushInUnderflowsColour, r);

            brushOutOverflowsColour.Dispose();
            brushOutUnderflowsColour.Dispose();
            brushInOverflowsColour.Dispose();
            brushInUnderflowsColour.Dispose();
        }

        public bool NoFade
        {
            get { return _noFade; }
            set { _noFade = value; }
        }

        private void UnderOverFlowWarningViewer_Click(object sender, EventArgs e)
        {
            if (_noFade)
            {
                clearIssues();

                ClearIssuesClick?.Invoke(sender, e);
            }
        }
    }
}
