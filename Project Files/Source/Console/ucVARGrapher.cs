/*  ucVARGrapher.cs

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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Thetis
{
    public partial class ucVARGrapher : UserControl
    {
        private const double m_MIN = 0.000005;

        private List<double> m_dData = null;
        private int m_nMaxPoints;
        private double m_dPlusMinusSwing = 0.04;
        private double m_dAutoSwing = 0.04;
        private bool m_bAutoSwing = false;
        private double m_dRingBufferPerc = 0;
        private string m_sCaption = "";

        public ucVARGrapher()
        {
            m_dData = new List<double>();

            InitializeComponent();
            Common.DoubleBufferAll(this, true);

            MaxPoints = 100;
        }

        public string Caption
        {
            get { return m_sCaption; }
            set { 
                m_sCaption = value;
                this.Invalidate();
            }
        }
        public double RingBufferPerc
        {
            get { return m_dRingBufferPerc; }
            set
            {
                m_dRingBufferPerc = value;
                this.Invalidate();
            }
        }
        public bool AutoSwing
        {
            get { return m_bAutoSwing; }
            set { m_bAutoSwing = value; this.Invalidate(); }
        }
        public double PlusMinusSwing
        {
            get
            { return m_dPlusMinusSwing; }
            set 
            {
                m_dPlusMinusSwing = value;

                this.Invalidate();
            }
        }
        public void AddDataPoint(double dataPoint)
        {
            if (m_dData == null) return;

            m_dData.Add(Math.Round(dataPoint, 6));

            if (m_dData.Count > m_nMaxPoints) m_dData.RemoveAt(0);

            double min = Math.Abs(m_dData.Min());
            double max = Math.Abs(m_dData.Max());
            m_dAutoSwing = Math.Max(min, max);

            this.Invalidate();
        }

        public int MaxPoints
        {
            get
            {
                return m_nMaxPoints;
            }
            set
            {
                if (m_dData == null) return;

                m_nMaxPoints = value;
                if (m_dData.Count > m_nMaxPoints)
                {
                    m_dData.RemoveRange(0, m_dData.Count - m_nMaxPoints - 1);

                    this.Invalidate();
                }
            }
        }

        private void VARGraph_Paint(object sender, PaintEventArgs e)
        {
            if (m_dData == null) return;

            if (m_dData.Count < 2) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int centreY = (this.Height / 2);

            //split down the 0 point
            e.Graphics.DrawLine(Pens.Gray, 0, centreY, this.Width, centreY);

            double swing = m_bAutoSwing ? m_dAutoSwing : m_dPlusMinusSwing;
            if (swing < m_MIN) swing = m_MIN;
            string sSwing = swing.ToString("F6");

            //text (uses the font defined on the user control)            
            using (StringFormat format = new StringFormat())
            {

                format.Alignment = StringAlignment.Far;
                SizeF sz = e.Graphics.MeasureString("±" + sSwing, this.Font);

                e.Graphics.DrawString("±" + sSwing, this.Font, Brushes.White, new PointF(this.Width, 0), format);
                e.Graphics.DrawString(m_sCaption, this.Font, Brushes.White, new PointF(this.Width, this.Height - sz.Height - 1), format);
            }

            //the data as a line
            double perPixel = this.Height / (swing * 2);

            Point[] points = new Point[m_dData.Count];
            for (int n = 0; n < m_dData.Count; n++)
            {
                int y = (int)(m_dData[n] * perPixel) + centreY;
                points[n] = new Point(n, (this.Height - 1) - y);
            }
            e.Graphics.DrawLines(Pens.Red, points);

            // yellow ring buffer perc line
            int xBufferPos = (this.Height - 1) - (int)((this.Height-1) * (m_dRingBufferPerc / 100));
            e.Graphics.DrawLine(Pens.Yellow, 0, xBufferPos, 10, xBufferPos);
        }

        private void VARGraph_Resize(object sender, EventArgs e)
        {
            MaxPoints = this.Width;
        }
    }
}
