/*  frmQuickRecallPopupList.cs

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
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public partial class frmQuickRecallPopupList : Form
    {
        public delegate void EntrySelected(int index);    
        public EntrySelected EntrySelectedHandlers;

        public frmQuickRecallPopupList()
        {
            InitializeComponent();
        }

        public ListBox FreqList {
            get {
                return lstboxFrequencies;
            }
        }

        public int AddItem(double dFreq)
        {
            return lstboxFrequencies.AddItem(dFreq);
        }
        public void ClearItems()
        {
            lstboxFrequencies.ClearItems();
        }
        public int FontEntryHeight {
            get { return lstboxFrequencies.FontEntryHeight; }
        }
        private void lstboxFrequencies_MouseClick(object sender, MouseEventArgs e)
        {
            int index = lstboxFrequencies.IndexFromPoint(e.Location);

            if (lstboxFrequencies.SelectedIndex >= 0 && index >= 0)
                EntrySelectedHandlers?.Invoke(lstboxFrequencies.SelectedIndex);
        }
    }

    #region QuickRecallListBox
    //based on
    //http://yacsharpblog.blogspot.com/2008/07/listbox-flicker.html
    internal class QuickRecallListBox : System.Windows.Forms.ListBox
    {
        private int m_nHighlighted = -1;
        private int m_nFrequencyMHzWidth = -1;
        private int m_nFontHeight = -1;

        public QuickRecallListBox()
        {
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            this.DrawMode = DrawMode.OwnerDrawFixed;
        }
        public void ClearItems()
        {
            m_nFrequencyMHzWidth = -1;
            
            this.Items.Clear();
            this.Invalidate();
        }
        public int AddItem(double dFreq)
        {
            dFreq = Math.Round(dFreq, 6);
            int nCalc = TextRenderer.MeasureText(((int)dFreq).ToString(), this.Font).Width;
            if (nCalc > m_nFrequencyMHzWidth) m_nFrequencyMHzWidth = nCalc;

            return this.Items.Add(dFreq);
        }
        public int FontEntryHeight {
            get { return m_nFontHeight; }
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (this.Items.Count > 0 && e.Index >= 0)
            {
                e.DrawBackground();

                bool bOk = double.TryParse(this.Items[e.Index].ToString(), out double dNum); // just because we can put any junk in the list

                if (bOk)
                {
                    SolidBrush sb = new SolidBrush(this.ForeColor);

                    int nFreq = (int)dNum;//((double)this.Items[e.Index]);
                    double remain = dNum - nFreq;//(double)this.Items[e.Index] - nFreq;
                    string remainString = remain.ToString("f6");
                    string sTmp3After = remainString.Substring(2, 3);
                    string sTmp3After3 = remainString.Substring(5, 3);

                    Size textSizeBeforeDecimalPoint = TextRenderer.MeasureText(nFreq.ToString(), this.Font);

                    int nTextPos = m_nFrequencyMHzWidth - textSizeBeforeDecimalPoint.Width;
                    Rectangle r = new Rectangle(nTextPos, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

                    e.Graphics.DrawString(nFreq.ToString(), e.Font, sb, r, StringFormat.GenericDefault);

                    r.X = m_nFrequencyMHzWidth - 8;

                    e.Graphics.DrawString("." + sTmp3After + "." + sTmp3After3, e.Font, sb, r, StringFormat.GenericDefault);

                    sb.Dispose();
                }
            }

            base.OnDrawItem(e);
        }
        protected override void OnFontChanged(EventArgs e)
        {
            Size textSize = TextRenderer.MeasureText("0", this.Font);
            // need this to space each entry correctly
            this.ItemHeight = textSize.Height;

            m_nFontHeight = textSize.Height;

            base.OnFontChanged(e);
        }
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            this.Invalidate();

            base.OnSelectedIndexChanged(e);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            m_nHighlighted = -1;
            this.Invalidate();

            base.OnMouseLeave(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point p = new Point(e.X, e.Y);
            int index = this.IndexFromPoint(p);

            if (index != ListBox.NoMatches)
            {

                if (index != m_nHighlighted)
                {
                    m_nHighlighted = index;

                    this.Invalidate();
                }
            }
            base.OnMouseMove(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Region iRegion = new Region(e.ClipRectangle);
            SolidBrush sb = new SolidBrush(this.BackColor);
            e.Graphics.FillRegion(sb, iRegion);

            if (this.Items.Count > 0)
            {
                for (int i = 0; i < this.Items.Count; ++i)
                {
                    Rectangle irect = this.GetItemRectangle(i);

                    if (e.ClipRectangle.IntersectsWith(irect))
                    {
                        if ((this.SelectionMode == SelectionMode.One && this.SelectedIndex == i)
                        || (this.SelectionMode == SelectionMode.MultiSimple && this.SelectedIndices.Contains(i))
                        || (this.SelectionMode == SelectionMode.MultiExtended && this.SelectedIndices.Contains(i)))
                        {
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                irect, i,
                                DrawItemState.Selected, this.ForeColor,
                                this.BackColor));
                        }
                        else if(m_nHighlighted == i){
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                irect, i,
                                DrawItemState.HotLight, this.ForeColor,
                                Color.Silver));
                        }
                        else
                        {
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                irect, i,
                                DrawItemState.Default, this.ForeColor,
                                this.BackColor));
                        }
                        iRegion.Complement(irect);
                    }
                }
            }

        }
    }
    #endregion
}
