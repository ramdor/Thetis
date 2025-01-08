/*  ucProgress.cs

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

namespace Thetis
{
    public partial class ucProgress : UserControl
    {
        private int _value = 0;
        private int _min = 0;
        private int _max = 100;

        [Browsable(true)]
        [DefaultValue(0)]
        public int Value { get { return _value; } set { _value = Math.Max(_min, Math.Min(_max, value)); Refresh(); } }
        [Browsable(true)]
        [DefaultValue(100)]
        public int Maximum { get { return _max; } set { _max = value; Refresh(); } }
        [Browsable(true)]
        [DefaultValue(0)]
        public int Minimum { get { return _min; } set { _min = value; Refresh(); } }

        public ucProgress()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int barWidth = (int)(Width * (_value - _min) / (double)(_max - _min));

            using (Pen blackPen = new Pen(Color.Black, 1))
            using (Brush greenBrush = new SolidBrush(Color.Green))
            {
                e.Graphics.FillRectangle(greenBrush, 0, 0, barWidth, Height);
                e.Graphics.DrawRectangle(blackPen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
}
