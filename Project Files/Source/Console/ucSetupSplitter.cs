/*  ucSetupSplitter.cs

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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public partial class ucSetupSplitter : UserControl
    {
        private string _sectionText = "GENERAL";
        private Color _lineColor = Color.Silver;
        private int _lineThickness = 1;
        private int _textGap = 8;
        private int _sideMargin = 5;

        public ucSetupSplitter()
        {
            InitializeComponent();

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            Height = 24;
        }

        // ============================================================
        // Section Text
        // ============================================================

        [Category("Appearance")]
        [Description("Text displayed in the middle of the splitter.")]
        [DefaultValue("GENERAL")]
        public string SectionText
        {
            get => _sectionText;
            set
            {
                _sectionText = value ?? string.Empty;
                Invalidate();
            }
        }

        // ============================================================
        // Line Colour
        // ============================================================

        [Category("Appearance")]
        [Description("Colour of the horizontal splitter lines.")]
        public Color LineColor
        {
            get => _lineColor;
            set
            {
                _lineColor = value;
                Invalidate();
            }
        }

        // ============================================================
        // Line Thickness
        // ============================================================

        [Category("Appearance")]
        [Description("Thickness of the horizontal splitter lines.")]
        [DefaultValue(1)]
        public int LineThickness
        {
            get => _lineThickness;
            set
            {
                _lineThickness = Math.Max(1, value);
                Invalidate();
            }
        }

        // ============================================================
        // Gap between text and line
        // ============================================================

        [Category("Appearance")]
        [Description("Space between the text and the horizontal lines.")]
        [DefaultValue(8)]
        public int TextGap
        {
            get => _textGap;
            set
            {
                _textGap = Math.Max(0, value);
                Invalidate();
            }
        }

        // ============================================================
        // Left / Right margin
        // ============================================================

        [Category("Appearance")]
        [Description("Margin at the left and right ends of the splitter.")]
        [DefaultValue(5)]
        public int SideMargin
        {
            get => _sideMargin;
            set
            {
                _sideMargin = Math.Max(0, value);
                Invalidate();
            }
        }

        // ============================================================
        // Bold
        // ============================================================

        [Category("Appearance")]
        [Description("Makes the section text bold.")]
        public bool BoldText
        {
            get => Font.Bold;

            set
            {
                if (Font.Bold == value)
                    return;

                FontStyle style = Font.Style;

                if (value)
                    style |= FontStyle.Bold;
                else
                    style &= ~FontStyle.Bold;

                Font = new Font(
                    Font.FontFamily,
                    Font.Size,
                    style,
                    Font.Unit);

                Invalidate();
            }
        }

        // ============================================================
        // Painting
        // ============================================================

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (string.IsNullOrEmpty(_sectionText))
                return;

            Graphics g = e.Graphics;

            // Measure the text without the normal TextRenderer padding.
            Size textSize = TextRenderer.MeasureText(
                _sectionText,
                Font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine);

            int textX = (ClientSize.Width - textSize.Width) / 2;
            int textY = (ClientSize.Height - textSize.Height) / 2;

            int lineY = ClientSize.Height / 2;

            // --------------------------------------------------------
            // Draw text
            // --------------------------------------------------------

            TextRenderer.DrawText(
                g,
                _sectionText,
                Font,
                new Point(textX, textY),
                ForeColor,
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine);

            // --------------------------------------------------------
            // Calculate line positions
            // --------------------------------------------------------

            int leftStart = _sideMargin;
            int leftEnd = textX - _textGap;

            int rightStart = textX + textSize.Width + _textGap;
            int rightEnd = ClientSize.Width - _sideMargin;

            using (Pen pen = new Pen(_lineColor, _lineThickness))
            {
                if (leftEnd > leftStart)
                {
                    g.DrawLine(
                        pen,
                        leftStart,
                        lineY,
                        leftEnd,
                        lineY);
                }

                if (rightEnd > rightStart)
                {
                    g.DrawLine(
                        pen,
                        rightStart,
                        lineY,
                        rightEnd,
                        lineY);
                }
            }
        }

        // ============================================================
        // Redraw when standard UserControl properties change
        // ============================================================

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Invalidate();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            Invalidate();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            Invalidate();
        }
    }
}