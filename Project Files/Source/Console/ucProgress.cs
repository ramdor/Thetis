/*  ucProgress.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2026 Richard Samphire MW0LGE

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

namespace Thetis
{
    public partial class ucProgress : UserControl
    {
        private int _value = 0;
        private int _min = 0;
        private int _max = 100;

        private int _vertical_line_value = -1;
        private int _vertical_line_width = 1;
        private Color _bar_color = Color.Green;
        private Color _veritical_line_color = Color.Red;

        [Browsable(true)]
        [DefaultValue(0)]
        public int Value { get { return _value; } set { _value = Math.Max(_min, Math.Min(_max, value)); Refresh(); } }

        [Browsable(true)]
        [DefaultValue(100)]
        public int Maximum { get { return _max; } set { _max = value; _value = Math.Max(_min, Math.Min(_max, _value)); Refresh(); } }

        [Browsable(true)]
        [DefaultValue(0)]
        public int Minimum { get { return _min; } set { _min = value; _value = Math.Max(_min, Math.Min(_max, _value)); Refresh(); } }

        [Browsable(true)]
        [DefaultValue(-1)]
        public int VerticalLineValue { get { return _vertical_line_value; } set { _vertical_line_value = value; Refresh(); } }

        [Browsable(true)]
        [DefaultValue(1)]
        public int VerticalLineWidth { get { return _vertical_line_width; } set { _vertical_line_width = Math.Max(1, value); Refresh(); } }

        [Browsable(true)]
        [DefaultValue(typeof(Color), "Green")]
        public Color BarColor { get { return _bar_color; } set { _bar_color = value; Refresh(); } }

        [Browsable(true)]
        [DefaultValue(typeof(Color), "Red")]
        public Color VeriticalLineColor { get { return _veritical_line_color; } set { _veritical_line_color = value; Refresh(); } }

        public ucProgress()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int denom = _max - _min;

            using (Pen blackPen = new Pen(Color.Black, 1))
            {
                if (denom <= 0 || Width <= 0 || Height <= 0)
                {
                    e.Graphics.DrawRectangle(blackPen, 0, 0, Width - 1, Height - 1);
                    return;
                }

                double frac = (_value - _min) / (double)denom;
                int barWidth = (int)Math.Round(Width * frac);
                if (barWidth < 0) barWidth = 0;
                if (barWidth > Width) barWidth = Width;

                using (Brush barBrush = new SolidBrush(_bar_color))
                {
                    e.Graphics.FillRectangle(barBrush, 0, 0, barWidth, Height);
                }

                if (_vertical_line_value >= _min && _vertical_line_value <= _max)
                {
                    double vfrac = (_vertical_line_value - _min) / (double)denom;
                    int x = (int)Math.Round((Width - 1) * vfrac);
                    if (x < 0) x = 0;
                    if (x > Width - 1) x = Width - 1;

                    using (Pen vpen = new Pen(_veritical_line_color, _vertical_line_width))
                    {
                        e.Graphics.DrawLine(vpen, x, 0, x, Height - 1);
                    }
                }

                e.Graphics.DrawRectangle(blackPen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
}
