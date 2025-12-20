/*  ucLGPicker.cs

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
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Text;

namespace Thetis
{
    [DefaultEvent("Changed")]
    public partial class ucLGPicker : UserControl
    {
        private const int LOW = 150; // -150 dbm
        private const int HIGH = 10; // +10 dbm
        private const int SPAN = LOW + HIGH;

        private const int m_nPadding = 16;                // the gaps around left/right side
        private const int m_nGrippers = 8;                // must be at least 2

        private int m_nSelectedGripper = -1;                // currently hightlighted/selected gripper index
        private bool m_bGripperSelectedForDrag = false;    // are we mouse dragging a gripper
        private bool m_bChangedDueToDrag = false;           // have we made a drag change (used by changed event)
        private bool m_bIncludeAlphaInPreview = false;      // alphas not used in preview, but they are included in color data
        private int m_nMouseOverIndex = -1;
        private bool _show_percent = false;
        public struct ColourGradientData
        {
            public Color color;
            public float percent;
        }
        private struct GradColours
        {
            public float percent;
            public Color color;
            public bool enabled;
            public bool highlighted;
        }

        private Dictionary<int, GradColours> m_dictColours;
        private IOrderedEnumerable<KeyValuePair<int, GradColours>> m_kvpSortedColours;

        public delegate void GripperSelectedEventHandler(object sender, ColourEventArgs e);
        public delegate void GripperDBMChangedEventHandler(object sender, GripperEventArgs e);
        public delegate void GripperMouseEnterEventHandler(object sender, GripperEventArgs e);
        public delegate void GripperMouseLeaveEventHandler(object sender, GripperEventArgs e);

        public event EventHandler Changed;

        public event GripperSelectedEventHandler GripperSelected;
        public event GripperDBMChangedEventHandler GripperDBMChanged;
        public event GripperMouseEnterEventHandler GripperMouseEnter;
        public event GripperMouseLeaveEventHandler GripperMouseLeave;

        public ucLGPicker()
        {
            InitializeComponent();

            m_dictColours = new Dictionary<int, GradColours>();

            for (int i = 0; i < m_nGrippers; i++)
            {
                addColour(i, i / (float)m_nGrippers, Color.FromArgb(255, i * (256 / m_nGrippers - 1), i * (256 / m_nGrippers - 1), i * (256 / m_nGrippers - 1)), m_dictColours);
            }
            addColour(m_nGrippers, 1, Color.FromArgb(255, 255, 255, 255), m_dictColours);

            for (int i = 1; i < m_nGrippers; i++)
            {
                enableGripper(i, false);
            }

            rebuildSortedColours();
        }

        public int Low
        {
            get { return -LOW; }
        }
        public int High
        {
            get { return HIGH; }
        }
        public bool ShowAsPercent
        {
            get { return _show_percent; }
            set 
            { 
                _show_percent = value;
                this.Invalidate();
            }
        }
        private void rebuildSortedColours()
        {
            // need to have a sorted list used for drawing the linear grad line in paint event
            // we can select into this list only the enabled entries
            lock (m_objListLocker)
            {
                List<KeyValuePair<int, GradColours>> list = m_dictColours.ToList();
                
                m_kvpSortedColours = from pair in list
                                     where pair.Value.enabled == true
                                     orderby pair.Value.percent ascending
                                     select pair;
            }
        }

        private void enableGripper(int index, bool bEnable)
        {
            lock (m_objListLocker)
            {
                GradColours gc = m_dictColours[index];
                gc.enabled = bEnable;
                gc.highlighted = false;
                m_dictColours[index] = gc;
            }
        }

        private void addColour(int index, float percpos, Color c, Dictionary<int,GradColours> lstColours, bool bEnable = true)
        {
            lock (m_objListLocker)
            {
                GradColours gc = new GradColours();
                gc.color = c;
                gc.percent = percpos;
                gc.enabled = bEnable;
                gc.highlighted = false;

                lstColours.Add(index, gc);
            }
        }

        private int actualWidth {
            get { return this.Width - m_nPadding * 2; }
            set { }
        }

        private void drawTextCentre(Graphics g, string s, int x)
        {
            Size textSize = TextRenderer.MeasureText(s, drawFont);

            Brush br = Enabled ? Brushes.Black : Brushes.Gray;

            g.DrawString(s, drawFont, br, new Point(x - (textSize.Width / 2), 24));
        }
        private Font drawFont = new Font("Microsft Sans Serif", 8);
        private void drawScales(Graphics g)
        {
            int zeroPoint = LOW;
            int span = SPAN;

            if (_show_percent)
            {
                drawTextCentre(g, "LOW", m_nPadding);
                drawTextCentre(g, "HIGH", m_nPadding + actualWidth);

                drawTextCentre(g, "MID", m_nPadding + (actualWidth / 2));
            }
            else
            {
                drawTextCentre(g, (-LOW).ToString(), m_nPadding);
                drawTextCentre(g, (HIGH).ToString(), m_nPadding + actualWidth);

                drawTextCentre(g, "0", m_nPadding + (int)((actualWidth / (float)span) * (float)(zeroPoint)));

                drawTextCentre(g, "-93", m_nPadding + (int)((actualWidth / (float)span) * (float)(zeroPoint - 93)));
                drawTextCentre(g, "-73", m_nPadding + (int)((actualWidth / (float)span) * (float)(zeroPoint - 73)));
            }
        }

        private void LGPicker_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            Point pEnd = new Point(m_nPadding + actualWidth, 12);
            Brush brFill; // fill the gripper if highlighted

            KeyValuePair<int, GradColours> kvpFirst = m_kvpSortedColours.First();
            GradColours previous = kvpFirst.Value;

            bool bIgnoreFirst = true;

            foreach (KeyValuePair<int, GradColours> kvp in m_kvpSortedColours)
            {
                if (!bIgnoreFirst)
                {
                    GradColours gc = kvp.Value;

                    Point pPrevious = new Point(m_nPadding + (int)(actualWidth * previous.percent), 12);
                    Point pThis = new Point(m_nPadding + (int)(actualWidth * gc.percent), 12);

                    if (pPrevious.X < pThis.X) // only use if there is at least a gap. LinearGradBrush fails with memory error if 0 line length
                    {
                        Color prevColour = Enabled ? previous.color : Color.Gray;
                        Color thisColour = Enabled ? gc.color : Color.Gray;

                        if (!m_bIncludeAlphaInPreview)
                        { // ignore alpha
                            prevColour = Color.FromArgb(255, prevColour);
                            thisColour = Color.FromArgb(255, thisColour);
                        }

                        using (LinearGradientBrush linGrBrush = new LinearGradientBrush(
                           pPrevious,
                           pThis,
                           prevColour,
                           thisColour))
                        {
                            using (Pen pen = new Pen(linGrBrush, 24))
                            {
                                g.DrawLine(pen, pPrevious, pThis);

                                brFill = previous.highlighted ? Brushes.Red : Brushes.White;
                                brFill = Enabled ? brFill : Brushes.Gray;

                                g.FillEllipse(brFill, pPrevious.X - 6, pPrevious.Y - 6, 12, 12);
                                g.DrawEllipse(Pens.Black, pPrevious.X - 6, pPrevious.Y - 6, 12, 12);
                            }
                        }
                    }

                    previous = gc;
                }
                bIgnoreFirst = false;
            }

            brFill = previous.highlighted ? Brushes.Red : Brushes.White;
            brFill = Enabled ? brFill : Brushes.Gray;

            g.FillEllipse(brFill, pEnd.X - 6, pEnd.Y - 6, 12, 12);
            g.DrawEllipse(Pens.Black, pEnd.X - 6, pEnd.Y - 6, 12, 12);

            drawScales(g);
        }

        private int findColourIndexGrip(int X, int Y)
        {
            int nRet = -1;

            for (int i = 0; i < m_dictColours.Count; i++)
            {
                int posX = m_nPadding + (int)(actualWidth * m_dictColours[i].percent);

                if (m_dictColours[i].enabled && X >= posX - 6 && X <= posX + 6 && Y >= 0 && Y <= 24)
                {
                    nRet = i;
                    break;
                }
            }

            return nRet;
        }
        private void LGPicker_MouseMove(object sender, MouseEventArgs e)
        {
            int X = e.X;
            int Y = e.Y;

            //if (Y < (this.Height/2) - 6 || Y > (this.Height/2) + 6) return;

            if (m_bGripperSelectedForDrag && m_nSelectedGripper > 0 && m_nSelectedGripper < m_dictColours.Count - 1)
            {
                float percPos = percFromPixels(X);

                int leftClose = indexOfClosestToLeft(m_nSelectedGripper);
                int rightClose = indexOfClosestToRight(m_nSelectedGripper);

                if (actualWidth * percPos < (actualWidth * m_dictColours[leftClose].percent) + 6)
                {
                    percPos = ((actualWidth * m_dictColours[leftClose].percent) + 6) / (float)actualWidth;
                }
                if (actualWidth * percPos > (actualWidth * m_dictColours[rightClose].percent) - 6)
                {
                    percPos = ((actualWidth * m_dictColours[rightClose].percent) - 6) / (float)actualWidth;
                }

                lock (m_objListLocker)
                {
                    GradColours gc = m_dictColours[m_nSelectedGripper];
                    gc.percent = percPos;
                    m_dictColours[m_nSelectedGripper] = gc;
                }
                m_bChangedDueToDrag = true;
                //Debug.Print(percPos.ToString());

                rebuildSortedColours();

                Invalidate();

                OnGripperDBMChanged(-LOW + (int)(SPAN * percPos), percPos);
            }
            else
            {
                // not dragging
                int index = findColourIndexGrip(X, Y);
                if (index != -1)
                {
                    if (index != m_nMouseOverIndex)
                    {
                        if (m_nMouseOverIndex != -1)
                        {
                            OnGripperMouseLeave(0, 0);
                        }

                        int dBm = -LOW + (int)(SPAN * m_dictColours[index].percent);

                        OnGripperMouseEnter(dBm, m_dictColours[index].percent); // TODO
                        m_nMouseOverIndex = index;
                    }
                }
                else
                {
                    if (m_nMouseOverIndex != -1)
                    {
                        OnGripperMouseLeave(0, 0);
                        m_nMouseOverIndex = -1;
                    }
                }
            }
        }

        private void highlightGripper(int index, bool bHightlight)
        {
            if (index < 0 || index > m_dictColours.Count - 1) return;

            lock (m_objListLocker)
            {
                GradColours gc = m_dictColours[index];
                gc.highlighted = bHightlight;
                m_dictColours[index] = gc;
            }


            rebuildSortedColours();
        }
        private float percFromPixels(int X)
        {
            float percPos = (X - m_nPadding) / (float)actualWidth;

            if (percPos < 0) percPos = 0;
            if (percPos > 1) percPos = 1;

            return percPos;
        }
        private void LGPicker_MouseDown(object sender, MouseEventArgs e)
        {
            int X = e.X;
            int Y = e.Y;

            int index = findColourIndexGrip(X, Y);
            if (index != m_nSelectedGripper)
            {
                highlightGripper(m_nSelectedGripper, false);
            }

            if (index == -1 && Y >= 0 && Y <= 24)
            {
                // add one
                // find left right index from this perc pos

                float perc = percFromPixels(X);

                int indexLeft = indexOfClosestToLeft(perc);
                int indexRight = indexOfClosestToRight(perc);

                if (indexLeft != -1 && indexRight != -1)
                {
                    float percWidth = m_dictColours[indexRight].percent - m_dictColours[indexLeft].percent;
                    if (percWidth * actualWidth > 32) // enough space?
                    {
                        float midWay = m_dictColours[indexLeft].percent + (percWidth / 2);

                        int newIndex = addGripper(midWay, GetColourAtPercent(midWay), false);

                        if (newIndex != -1)
                        {
                            highlightGripper(newIndex, true);
                            m_nSelectedGripper = newIndex;

                            OnGripperSelected(m_dictColours[m_nSelectedGripper].color);

                            Invalidate();
                        }
                    }
                }

                return;
            }

            if (index == -1) return;

            if (e.Button != MouseButtons.None)
            {
                m_bGripperSelectedForDrag = true;

                highlightGripper(index, true);
                m_nSelectedGripper = index;

                OnGripperSelected(m_dictColours[m_nSelectedGripper].color);
            }

            Invalidate();
        }

        private void LGPicker_MouseUp(object sender, MouseEventArgs e)
        {
            int X = e.X;
            int Y = e.Y;

            if (m_bChangedDueToDrag)
            {
                m_bChangedDueToDrag = false;
                OnChanged(EventArgs.Empty);
            }

            m_bGripperSelectedForDrag = false;

            if (m_nSelectedGripper == -1) OnGripperSelected(Color.Empty);

            //highlightGripper(m_nHighlighted, false);
            //m_nHighlighted = -1;

            //Invalidate();
            //if (Y < (this.Height / 2) - 6 || Y > (this.Height / 2) + 6) return;
        }

        private int indexOfClosestToLeft(float perc)
        {
            int closestIndex = -1;
            float closestDiff = float.MaxValue;
            for (int i = 0; i < m_dictColours.Count; i++)
            {
                if (m_dictColours[i].enabled && m_dictColours[i].percent < perc && (perc - m_dictColours[i].percent < closestDiff))
                {
                    closestDiff = perc - m_dictColours[i].percent;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
        private int indexOfClosestToLeft(int index)
        {
            int closestIndex = -1;
            float closestDiff = float.MaxValue;
            for (int i = 0; i < m_dictColours.Count; i++)
            {
                if (m_dictColours[i].enabled && m_dictColours[i].percent < m_dictColours[index].percent && (m_dictColours[index].percent - m_dictColours[i].percent < closestDiff))
                {
                    closestDiff = m_dictColours[index].percent - m_dictColours[i].percent;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
        private int indexAtPerc(float perc)
        {
            int onTopIndex = -1;
            for (int i = 0; i < m_dictColours.Count; i++)
            {
                if (m_dictColours[i].enabled && m_dictColours[i].percent == perc)
                {
                    onTopIndex = i;
                    break;
                }
            }

            return onTopIndex;
        }
        private int indexOfClosestToRight(float perc)
        {
            int closestIndex = -1;
            float closestDiff = float.MaxValue;
            for (int i = 0; i < m_dictColours.Count; i++)
            {
                if (m_dictColours[i].enabled && m_dictColours[i].percent > perc && (m_dictColours[i].percent - perc < closestDiff))
                {
                    closestDiff = m_dictColours[i].percent - perc;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
        private int indexOfClosestToRight(int index)
        {
            int closestIndex = -1;
            float closestDiff = float.MaxValue;
            for (int i = 0; i < m_dictColours.Count; i++)
            {
                if (m_dictColours[i].enabled && m_dictColours[i].percent > m_dictColours[index].percent && (m_dictColours[i].percent - m_dictColours[index].percent < closestDiff))
                {
                    closestDiff = m_dictColours[i].percent - m_dictColours[index].percent;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private void LGPicker_MouseLeave(object sender, EventArgs e)
        {
            if (m_nMouseOverIndex != -1)
            {
                OnGripperMouseLeave(0, 0);
                m_nMouseOverIndex = -1;
            }
        }
        public Color ColourForSelectedGripper {
            get {
                if (m_nSelectedGripper == -1) return Color.Empty;

                return m_dictColours[m_nSelectedGripper].color;
            }
            set {
                if (m_nSelectedGripper == -1) return;

                setColour(m_nSelectedGripper, value, true);

                OnChanged(EventArgs.Empty);
            }
        }
        private void setColour(int index, Color c, bool bRefresh = false)
        {
            if (index < 0 || index > m_dictColours.Count - 1) return;

            lock (m_objListLocker)
            {
                GradColours gc = m_dictColours[index];
                gc.color = c;
                m_dictColours[index] = gc;
            }

            rebuildSortedColours();

            if (bRefresh) Invalidate();
        }

        public void RemoveSelectedGripper(bool bRefresh = false)
        {
            if (m_nSelectedGripper == -1) return;
            // prevent either end
            if (m_nSelectedGripper == 0 || m_nSelectedGripper == m_dictColours.Count - 1) return;

            enableGripper(m_nSelectedGripper, false);

            m_nSelectedGripper = -1;

            rebuildSortedColours();

            if (bRefresh) Invalidate();

            OnChanged(EventArgs.Empty);
        }

        private int addGripper(float perc, Color colour, bool bRefresh = false)
        {
            // add a gripper at given percentage
            int index = findFreeDisabledGripper();
            if (index == -1) return -1; // none left !!

            lock (m_objListLocker)
            {
                GradColours gc = m_dictColours[index];
                gc.color = colour;
                gc.percent = perc;
                gc.enabled = true;
                gc.highlighted = false;

                m_dictColours[index] = gc;
            }

            rebuildSortedColours();

            if (bRefresh) Invalidate();

            OnChanged(EventArgs.Empty);

            return index;
        }

        private int findFreeDisabledGripper()
        {
            int nRet = -1;
            for (int i = 1; i < m_dictColours.Count - 1; i++)
            {
                if (!m_dictColours[i].enabled)
                {
                    nRet = i;
                    break;
                }
            }

            return nRet;
        }
        public void HighlightFirstGripper()
        {
            for (int i = 0; i < m_dictColours.Count; i++)
            {
                if (m_dictColours[i].enabled)
                {
                    highlightGripper(i, true);
                    m_nSelectedGripper = i;
                    Invalidate();
                    OnGripperSelected(m_dictColours[i].color);
                    break;
                }
            }
        }
        public void Clear()
        {
            for (int i = 1; i < m_dictColours.Count - 1; i++)
            {
                enableGripper(i, false);
            }

            rebuildSortedColours();

            Invalidate();

            OnChanged(EventArgs.Empty);
        }
        public override string Text {
            get {
                string sRet = m_dictColours.Count.ToString() + "|";
                for (int i = 0; i < m_dictColours.Count; i++)
                {
                    if (m_dictColours[i].enabled)
                    {
                        sRet += "1|";
                    }
                    else
                    {
                        sRet += "0|";
                    }
                    sRet += m_dictColours[i].percent.ToString("0.000") + "|";
                    sRet += m_dictColours[i].color.ToArgb().ToString() + "|";
                }
                return sRet;
            }
            set {
                string[] parts = value.Split('|');

                bool bOk = false;
                //lock (m_objListLocker)
                //{
                    //m_dictColours.Clear();

                Dictionary<int, GradColours> lstTmp = new Dictionary<int, GradColours>();

                int tot;
                bOk = int.TryParse(parts[0], out tot);
                if (!bOk) return;
                bOk = parts.Length == (tot * 3) + 2;
                if (bOk)
                {
                    bOk = false;
                    for (int i = 0; i < tot; i++)
                    {
                        bool enabled = false;
                        int col = 0;
                        float percent = 0;

                        int entry = 1 + (i * 3);
                        if (parts[entry] == "1") enabled = true;
                        bOk = float.TryParse(parts[entry + 1], out percent);
                        if (bOk) bOk = int.TryParse(parts[entry + 2], out col);

                        if (bOk)
                        {
                            addColour(i, percent, Color.FromArgb(col), lstTmp, enabled);
                            //enableGripper(i, enabled);
                        }
                        if (!bOk) break;
                    }
                }
                //}
                if (bOk)
                {
                    lock (m_objListLocker)
                    {
                        m_dictColours.Clear();
                        foreach (KeyValuePair<int, GradColours> kvp in lstTmp) 
                        {
                            m_dictColours.Add(kvp.Key, kvp.Value);
                        }
                    }
                    rebuildSortedColours();
                    HighlightFirstGripper();

                    Invalidate();

                    OnChanged(EventArgs.Empty);
                }
            }
        }
        private void OnChanged(EventArgs e)
        {
            Changed?.Invoke(this, e);
        }
        private void OnGripperSelected(Color c)
        {
            GripperSelected?.Invoke(this, new ColourEventArgs(c));
        }
        private void OnGripperMouseEnter(int dbm, float percent)
        {
            GripperMouseEnter?.Invoke(this, new GripperEventArgs(dbm, percent));
        }
        private void OnGripperMouseLeave(int dbm, float percent)
        {
            GripperMouseLeave?.Invoke(this, new GripperEventArgs(dbm, percent));
        }
        private void OnGripperDBMChanged(int dbm, float percent)
        {
            GripperDBMChanged?.Invoke(this, new GripperEventArgs(dbm, percent));
        }
        private void LGPicker_EnabledChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
        public Color GetColourForDBM(float dbm)
        {
            float f = GetPercForDBM(dbm);

            return GetColourAtPercent(f);
        }
        public Color GetColourAtPercent(float perc)
        {
            Color c = Color.Empty;

            int iL = indexAtPerc(perc);//;indexOfClosestToLeft(perc);
            if (iL == -1) iL = indexOfClosestToLeft(perc);
            int iR = indexOfClosestToRight(perc);

            if (iR == -1) iR = iL; // if none to the right, the L and R the same

            if (iL != -1 && iR != -1)
            {
                float percWidth = m_dictColours[iR].percent - m_dictColours[iL].percent;
                float scale = percWidth == 0 ? 0 : 1 / percWidth;

                float offset = perc - m_dictColours[iL].percent;
                perc = (offset * scale);

                c = ColorInterpolator.InterpolateBetween(m_dictColours[iL].color, m_dictColours[iR].color, perc);
            }

            return c;
        }
        public float GetPercForDBM(float dbm)
        {
            float max = SPAN; // LOW through to HIGH
            dbm += LOW;

            float perc = dbm / max;
            if (perc < 0) perc = 0;
            if (perc > 1) perc = 1;

            return perc;
        }
        private void addColourGradientData(List<ColourGradientData> lst, Color color, float perc)
        {
            ColourGradientData c = new ColourGradientData();
            c.color = color;
            c.percent = perc;
            lst.Add(c);
        }
        private readonly Object m_objListLocker = new Object(); // used so that another thread (ie display is unable have this data corrupt by
                                                                // making changes
        public List<ColourGradientData> GetColourGradientDataForDBMRange(float low, float high)
        {
            lock (m_objListLocker)
            {
                List<ColourGradientData> lst = new List<ColourGradientData>();

                float l = GetPercForDBM(low);
                float h = GetPercForDBM(high);

                // check if we have a node for these spefic percs
                int nLow = indexAtPerc(l);
                int nHigh = indexAtPerc(h);

                if (nLow != -1)
                {
                    addColourGradientData(lst, m_dictColours[nLow].color, m_dictColours[nLow].percent);

                    l = m_dictColours[nLow].percent;
                }
                else
                {
                    // need to add a start node
                    addColourGradientData(lst, GetColourAtPercent(l), l);
                }

                bool bRunning = true;
                while (bRunning)
                {
                    int nNext = indexOfClosestToRight(l);
                    if (nNext != -1)
                    {
                        if (m_dictColours[nNext].percent < h)
                        {
                            addColourGradientData(lst, m_dictColours[nNext].color, m_dictColours[nNext].percent);
                        }
                        else
                        {
                            // check we already have an end node
                            if (nHigh != -1)
                            {
                                addColourGradientData(lst, m_dictColours[nHigh].color, m_dictColours[nHigh].percent);
                            }
                            else
                            {
                                addColourGradientData(lst, GetColourAtPercent(h), h);
                            }
                            bRunning = false;
                        }
                        l = m_dictColours[nNext].percent;
                    }
                    else bRunning = false;
                }

                // normalise percs
                float lowPerc = lst.First<ColourGradientData>().percent;
                float highPerc = lst.Last<ColourGradientData>().percent;
                float scale = 1 / (highPerc - lowPerc);
                for (int n = 0; n < lst.Count; n++)
                {
                    ColourGradientData cgd = lst[n];
                    cgd.percent -= lowPerc;
                    cgd.percent *= scale;
                    lst[n] = cgd;
                }
                return lst;
            }
        }

        public bool IncludeAlphaInPreview {
            get {
                return m_bIncludeAlphaInPreview;
            }
            set {
                m_bIncludeAlphaInPreview = value;
                rebuildSortedColours();
                Invalidate();
            }
        }
        public void ApplyGlobalAlpha(int A)
        {
            lock (m_objListLocker)
            {
                for (int n = 0; n < m_dictColours.Count; n++)
                {
                    GradColours gc = m_dictColours[n];
                    if (gc.enabled)
                    {
                        gc.color = Color.FromArgb(A, gc.color);
                        m_dictColours[n] = gc;
                    }
                }
            }
            rebuildSortedColours();

            Invalidate();

            OnChanged(EventArgs.Empty);
        }

        public string EncodedText
        {
            get
            {
                try
                {
                    return Convert.ToBase64String(Encoding.UTF8.GetBytes(this.Text));
                }
                catch { return ""; }
            }
            set
            {
                try
                {
                    this.Text = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                }
                catch { }
            }
        }
        //private void LGPicker_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (m_nSelectedGripper == 0 && (Keyboard.IsKeyDown(Keys.LControlKey) || Keyboard.IsKeyDown(Keys.RControlKey)))
        //    {
        //        if (e.KeyCode == Keys.C)
        //        {
        //            try
        //            {
        //                string sTmp = Convert.ToBase64String(Encoding.UTF8.GetBytes(this.Text));
        //                Clipboard.SetText(sTmp);
        //            }
        //            catch { }                    
        //        }
        //        else if(e.KeyCode == Keys.V)
        //        {
        //            try
        //            {
        //                string sTmp = Encoding.UTF8.GetString(Convert.FromBase64String(Clipboard.GetText()));
        //                this.Text = sTmp;
        //            }
        //            catch { }
        //        }
        //    }
        //}
    }

    // down here beacuse of class order restrictions with user controls
    public class ColourEventArgs
    {
        public ColourEventArgs(Color c) { Colour = c; }
        public Color Colour { get; } // read only
    }
    public class GripperEventArgs
    {
        public GripperEventArgs(int dBm, float percent) { DBM = dBm; Percent = percent; }
        public int DBM { get; } // read only
        public float Percent { get; }
    }

    // based on : https://stackoverflow.com/questions/1236683/color-interpolation-between-3-colors-in-net
    public class ColorInterpolator
    {
        delegate byte ComponentSelector(Color color);
        static ComponentSelector _alphaSelector = color => color.A;
        static ComponentSelector _redSelector = color => color.R;
        static ComponentSelector _greenSelector = color => color.G;
        static ComponentSelector _blueSelector = color => color.B;

        public static Color InterpolateBetween(
            Color endPoint1,
            Color endPoint2,
            double lambda)
        {
            if (lambda < 0 || lambda > 1)
            {
                //throw new ArgumentOutOfRangeException("lambda");
                return Color.Empty;
            }
            Color color = Color.FromArgb(
                InterpolateComponent(endPoint1, endPoint2, lambda, _alphaSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _redSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _greenSelector),
                InterpolateComponent(endPoint1, endPoint2, lambda, _blueSelector)
            );

            return color;
        }

        static byte InterpolateComponent(
            Color endPoint1,
            Color endPoint2,
            double lambda,
            ComponentSelector selector)
        {
            return (byte)(selector(endPoint1)
                + (selector(endPoint2) - selector(endPoint1)) * lambda);
        }
    }
}
