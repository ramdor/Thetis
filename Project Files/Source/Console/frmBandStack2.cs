/*  frmBandStack2.cs

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
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Thetis
{
    public partial class frmBandStack2 : Form
    {
        public delegate void EntrySelected(BandStackFilter bsf, BandStackEntry bse, bool updateLastVisited = true, bool obeyHide = true);
        public EntrySelected EntrySelectedHandlers;

        public delegate void EntryAdd(BandStackFilter bsf);
        public EntryAdd EntryAddHandlers;

        public delegate void EntryUpdate(BandStackFilter bsf, BandStackEntry bse);
        public EntryUpdate EntryUpdateHandlers;

        public delegate void EntryDelete(BandStackFilter bsf, BandStackEntry bse);
        public EntryDelete EntryDeleteHandlers;

        public delegate void IgnoreDupes(bool ignore);
        public IgnoreDupes IgnoreDupeHandlers;

        public delegate void HideOnSelect(bool hideOnSelect);
        public HideOnSelect HideOnSelectHandlers;

        public delegate void ShowInSpectrum(bool show);
        public HideOnSelect ShowInSpectrumHandlers;

        private BandStackFilter m_bsf;

        private bool _is_popup;
        private bool _is_popup_on_top;
        private Point _location;
        private bool m_bIgnoreIndexChanged; // ignore select index change when updating the list with initbandstack filter or the update selected
        private Console _console;
        private bool _mox;

        public frmBandStack2()
        {
            InitializeComponent();
            _console = null;
        }

        public void InitForm(Console console)
        {
            _console = console;
            _mox = _console.MOX;
            _console.MoxChangeHandlers += OnMox;
            _is_popup = false;
            _is_popup_on_top = false;
            m_bIgnoreIndexChanged = false;
            bandStackListBox.Items.Clear();

            Common.RestoreForm(this, "BandStack2Form", true);

            this.Width = 256;
            btnOptions.Text = "Options >>";

            btnLockSelected.Enabled = false;
            btnDeleteSelected.Enabled = false;
            btnSetSpecific.Enabled = false;
            btnUpdateEntry.Enabled = false;

            Common.ForceFormOnScreen(this);

            _location = this.Location;
        }
        private void OnMox(int rx, bool oldMox,  bool newMox)
        {
            if (rx == 1) _mox = newMox;
        }
        public void RemoveDelegates()
        {
            if (_console != null)
            {
                _console.MoxChangeHandlers -= OnMox;
            }
        }
        public void InitBandStackFilter(BandStackFilter bsf, bool select = true)
        {
            m_bIgnoreIndexChanged = true;

            bandStackListBox.BeginUpdate();
            // NOTE: the use of entries here is accesing a COPY
            // of the filters internal entries. You can update m_bsf.Entries
            // but it wont change the contents of the filter
            // you need to use UpdateEntry

            bandStackListBox.ClearItems();
            bandStackListBox.SelectedIndex = -1;

            m_bsf = bsf;

            if (m_bsf == null)
            {
                bandStackListBox.EndUpdate();
                return;
            }

            string sTmp = bsf.FilterName;
            if (!bsf.UserDefined && (sTmp.StartsWith("B") || sTmp.StartsWith("b"))) sTmp = sTmp.Substring(1); // pull off the b if the name is a band enum
            lblFilterName.Text = sTmp;

            if (!bsf.UserDefined)
            {
                lblFilterName.ForeColor = BandStackManager.BandToColour(BandStackManager.StringToBand(bsf.FilterName)); // filter name is same as band enum for standard filters
            }
            else
            {
                lblFilterName.ForeColor = Color.White;
            }

            foreach (BandStackEntry bse in m_bsf.Entries)
            {
                int n = bandStackListBox.AddItem(bse);

                if (select && n == m_bsf.IndexOfCurrent)
                {                    
                    bandStackListBox.SelectedIndex = n;
                }
            }

            bandStackListBox.EndUpdate();

            setupSelectedButtons();
            setupRadioButtons();

            m_bIgnoreIndexChanged = false;
        }
        public void UpdateSelected()
        {
            if (m_bsf == null) return;

            if (m_bsf.Current() == null) return;

            m_bIgnoreIndexChanged = true;

            bandStackListBox.BeginUpdate();
            string sGUID = m_bsf.Current().GUID;

            for (int n = 0; n < bandStackListBox.Items.Count; n++)
            {
                BandStackEntry bs = bandStackListBox.Items[n] as BandStackEntry;

                if (sGUID == bs.GUID)
                {
                    bandStackListBox.SelectedIndex = n;
                    break;
                }
            }
            bandStackListBox.EndUpdate();
            setupSelectedButtons();

            m_bIgnoreIndexChanged = false;
        }
        private void setupSelectedButtons()
        {
            if (bandStackListBox.SelectedIndex < 0)
            {
                // nothing selected                
                btnDeleteSelected.Enabled = false;
                btnUpdateEntry.Enabled = false;
                btnLockSelected.Enabled = false;
                if (radioSpecific.Checked)
                {
                    btnSetSpecific.Enabled = false;
                }
            }
            else
            {
                BandStackEntry bse = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;
                btnDeleteSelected.Enabled = !bse.Locked;
                btnUpdateEntry.Enabled = !bse.Locked;
                btnLockSelected.Text = bse.Locked ? "Unlock Selected" : "Lock Selected";                

                btnLockSelected.Enabled = true;
                if (radioSpecific.Checked)
                {
                    btnSetSpecific.Enabled = true;
                }
            }
        }
        private void setupRadioButtons(bool bCheck = true)
        {
            if (m_bsf == null) return;

            switch (m_bsf.ReturnMode)
            {
                case BandStackFilter.FilterReturnMode.Current:
                    if(bCheck) radioLastUsedEntry.Checked = true;
                    btnSetSpecific.Enabled = false;
                    bandStackListBox.SpecificReturnIndex = -1;
                    break;
                case BandStackFilter.FilterReturnMode.LastVisited:
                    if (bCheck) radioLastUsed.Checked = true;
                    btnSetSpecific.Enabled = false;
                    bandStackListBox.SpecificReturnIndex = -1;
                    break;
                case BandStackFilter.FilterReturnMode.Specific:
                    if (bCheck) radioSpecific.Checked = true;
                    btnSetSpecific.Enabled = bandStackListBox.Items.Count > 0;
                    
                    bandStackListBox.SpecificReturnIndex = -1;

                    for (int n =0;n<bandStackListBox.Items.Count;n++)
                    {
                        if((bandStackListBox.Items[n] as BandStackEntry).GUID == m_bsf.ReturnGUID)
                        {
                            bandStackListBox.SpecificReturnIndex = n;
                            break;
                        }
                    }
                    break;
            }
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            if (this.Width > 256)
            {
                this.Width = 256;
                btnOptions.Text = "Options >>";
            }
            else
            {
                this.Width = 512;
                btnOptions.Text = "Options <<";
            }
        }

        private void radioLastUsedEntry_CheckedChanged(object sender, EventArgs e)
        {
            if(m_bsf == null || m_bsf.ReturnMode == BandStackFilter.FilterReturnMode.Current) return;

            m_bsf.ReturnMode = BandStackFilter.FilterReturnMode.Current;
            setupRadioButtons(false);
            bandStackListBox.Invalidate();
        }

        private void radioSpecific_CheckedChanged(object sender, EventArgs e)
        {
            if (m_bsf == null || m_bsf.ReturnMode == BandStackFilter.FilterReturnMode.Specific) return;

            m_bsf.ReturnMode = BandStackFilter.FilterReturnMode.Specific;
            setupRadioButtons(false);
            bandStackListBox.Invalidate();
        }

        private void radioLastUsed_CheckedChanged(object sender, EventArgs e)
        {
            if (m_bsf == null || m_bsf.ReturnMode == BandStackFilter.FilterReturnMode.LastVisited) return;

            m_bsf.ReturnMode = BandStackFilter.FilterReturnMode.LastVisited;
            setupRadioButtons(false);
            bandStackListBox.Invalidate();
        }

        private void btnSetSpecific_Click(object sender, EventArgs e)
        {
            if (m_bsf == null) return;

            if (bandStackListBox.SelectedIndex >= 0)
            {
                BandStackEntry bse = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;
                m_bsf.ReturnGUID = bse.GUID;

                bandStackListBox.SpecificReturnIndex = bandStackListBox.SelectedIndex;
                bandStackListBox.Invalidate();
            }
        }

        private void btnLockSelected_Click(object sender, EventArgs e)
        {
            if (m_bsf == null) return;

            if (bandStackListBox.SelectedIndex >= 0)
            {
                BandStackEntry bse = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;
                bse.Locked = !bse.Locked;
                btnDeleteSelected.Enabled = !bse.Locked;
                btnUpdateEntry.Enabled = !bse.Locked;
                btnLockSelected.Text = bse.Locked ? "Unlock Selected" : "Lock Selected";                

                //update filter
                m_bsf.UpdateEntry(bse);
                bandStackListBox.Invalidate();
            }
        }

        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            if (m_bsf == null) return;

            if (bandStackListBox.SelectedIndex < 0 || bandStackListBox.Items.Count == 0) return;

            BandStackEntry bse = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;

            EntryDeleteHandlers?.Invoke(m_bsf, bse);
        }

        private void bandStackListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_bIgnoreIndexChanged || _mox) return;

            if (bandStackListBox.SelectedIndex >= 0)
            {
                BandStackEntry bse = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;

                btnLockSelected.Text = bse.Locked ? "Unlock Selected" : "Lock Selected";

                if (m_bsf.ReturnMode == BandStackFilter.FilterReturnMode.Specific)
                {
                    btnSetSpecific.Enabled = bandStackListBox.Items.Count > 0;
                }

                btnLockSelected.Enabled = bandStackListBox.SelectedIndex >= 0;
                btnDeleteSelected.Enabled = bandStackListBox.SelectedIndex >= 0;
                btnUpdateEntry.Enabled = bandStackListBox.SelectedIndex >= 0;

                EntrySelectedHandlers?.Invoke(m_bsf, bse);
            }
        }

        private void btnAddStackEntry_Click(object sender, EventArgs e)
        {
            EntryAddHandlers?.Invoke(m_bsf);
        }

        private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkAlwaysOnTop.Checked;
        }

        public void HideClose()
        {
            this.Hide();
            Store();

            if (_is_popup && _is_popup_on_top && !chkAlwaysOnTop.Checked)
                this.TopMost = false;

            _is_popup = false;
            _is_popup_on_top = false;
        }

        private void frmBandStack2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            HideClose();
        }
#pragma warning disable CS0109
        public new void Show(bool is_popup = false, Point? popup_location = null, bool on_top = false)
#pragma warning restore CS0109
        { // shadow of show
            _is_popup = is_popup;
            _is_popup_on_top = on_top;
            InitBandStackFilter(m_bsf, true);
            this.BringToFront();

            if (_is_popup)
            {
                if (on_top) this.TopMost = true;
                this.Location = popup_location ?? Point.Empty;
                Common.ForceFormOnScreen(this, false, true);
            }
            else
            {
                this.Location = _location;
                Common.ForceFormOnScreen(this, false, false);
            }            

            base.Show();
        }
        public void Store()
        {
            if(!_is_popup)
                Common.SaveForm(this, "BandStack2Form");
        }

        private void btnUpdateEntry_Click(object sender, EventArgs e)
        {
            if (m_bsf == null) return;

            if (bandStackListBox.SelectedIndex < 0 || bandStackListBox.Items.Count == 0) return;

            BandStackEntry bse = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;

            EntryUpdateHandlers?.Invoke(m_bsf, bse);
        }

        private void chkIgnoreDuplicates_CheckedChanged(object sender, EventArgs e)
        {
            IgnoreDupeHandlers?.Invoke(chkIgnoreDuplicates.Checked);
        }

        private void chkHideOnSelect_CheckedChanged(object sender, EventArgs e)
        {
            HideOnSelectHandlers?.Invoke(chkHideOnSelect.Checked);
        }

        private void chkShowInSpectrum_CheckedChanged(object sender, EventArgs e)
        {
            ShowInSpectrumHandlers?.Invoke(chkShowInSpectrum.Checked);
        }
        public ToolTip ToolTip
        {
            get
            {
                return toolTip1;
            }
        }

        private void frmBandStack2_LocationChanged(object sender, EventArgs e)
        {
            if (!_is_popup) _location = this.Location;
        }
    }
    #region BandStackListBox
    //based on
    //http://yacsharpblog.blogspot.com/2008/07/listbox-flicker.html
    internal class BandStackListBox : System.Windows.Forms.ListBox
    {
        private int m_nHighlighted = -1;        

        private int m_nNumberWidth = -1;
        private int m_nFrequencyMHzWidth = -1;

        private Font m_SmallModeFont;

        public BandStackListBox()
        {
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);

            this.DrawMode = DrawMode.OwnerDrawFixed;            
        }
        private Image m_LockLockedImage;
        private Image m_LockUnlockedImage;
        private Image m_MemoryImage;
        private Image m_SpecificReturnImage;

        public int m_specficReturnIndex = -1; 

        public Image LockImageLocked {
            get {
                return m_LockLockedImage;
            }
            set {
                m_LockLockedImage = value;
            }
        }
        public Image LockImageUnLocked {
            get {
                return m_LockUnlockedImage;
            }
            set {
                m_LockUnlockedImage = value;
            }
        }
        public Image Memory {
            get {
                return m_MemoryImage;
            }
            set {
                m_MemoryImage = value;
            }
        }
        public Image SpecificReturnImage {
            get {
                return m_SpecificReturnImage;
            }
            set {
                m_SpecificReturnImage = value;
            }
        }
        public int SpecificReturnIndex {
            get {
                return m_specficReturnIndex;
            }
            set {
                m_specficReturnIndex = value;
            }
        }
        public int AddItem(BandStackEntry bse)
        {
            m_nNumberWidth = TextRenderer.MeasureText(this.Items.Count.ToString() + ") ", m_SmallModeFont).Width;
            int nCalc = TextRenderer.MeasureText(((int)((bse as BandStackEntry).Frequency)).ToString(), this.Font).Width;
            if (nCalc > m_nFrequencyMHzWidth) m_nFrequencyMHzWidth = nCalc;

            int nRet = this.Items.Add(bse);

            return nRet;
        }
        public void ClearItems()
        {
            m_nFrequencyMHzWidth = -1;
            m_nNumberWidth = -1;

            this.Items.Clear();
            this.Invalidate();
        }
        protected override void OnFontChanged(EventArgs e)
        {
            Size textSize = TextRenderer.MeasureText("0", this.Font);
            // need this to space each entry correctly
            this.ItemHeight = textSize.Height;

            m_SmallModeFont = new Font(this.Font.FontFamily, this.Font.Size * 0.6f, FontStyle.Regular);

            base.OnFontChanged(e);
        }
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            this.Invalidate();

            base.OnSelectedIndexChanged(e);
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (this.Items.Count > 0 && e.Index >= 0)
            {
                BandStackEntry bse = this.Items[e.Index] as BandStackEntry;
                
                if (bse != null)
                {
                    if (bse.Locked)
                    {
                        e.Graphics.DrawImage(m_LockLockedImage, new Point(e.Bounds.Width - 20, e.Bounds.Y + (e.Bounds.Height / 2) - (m_LockLockedImage.Height / 2)));
                    }
                    else
                    {
                        e.Graphics.DrawImage(m_LockUnlockedImage, new Point(e.Bounds.Width - 20, e.Bounds.Y + (e.Bounds.Height / 2) - (m_LockLockedImage.Height / 2)));
                    }

                    if (e.Index == m_specficReturnIndex)
                    {
                        e.Graphics.DrawImage(m_SpecificReturnImage, new Point(e.Bounds.Width - 30, e.Bounds.Y + (e.Bounds.Height / 2) - (m_SpecificReturnImage.Height / 2)));
                    }

                    //using (SolidBrush sb = new SolidBrush(this.ForeColor))
                    //{
                        SolidBrush sb = new SolidBrush(this.ForeColor);

                        if (m_SmallModeFont != null)
                        {
                            string sNumber = (e.Index + 1).ToString() + ") ";
                            Size numberSize = TextRenderer.MeasureText(sNumber, m_SmallModeFont);
                            e.Graphics.DrawString(sNumber, m_SmallModeFont, sb, 0, e.Bounds.Y + (e.Bounds.Height / 2) - (numberSize.Height / 2), StringFormat.GenericDefault);
                        }

                        int nFreq = (int)bse.Frequency;
                        double remain = bse.Frequency - nFreq;
                        string remainString = remain.ToString("f6");
                        string sTmp3After = remainString.Substring(2, 3);
                        string sTmp3After3 = remainString.Substring(5, 3);

                        Size textSizeBeforeDecimalPoint = TextRenderer.MeasureText(nFreq.ToString(), this.Font);

                        int nTextPos = m_nNumberWidth + m_nFrequencyMHzWidth - textSizeBeforeDecimalPoint.Width;
                        Rectangle r = new Rectangle(nTextPos, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

                        e.Graphics.DrawString(nFreq.ToString(), e.Font, sb, r, StringFormat.GenericDefault);

                        r.X = m_nNumberWidth + m_nFrequencyMHzWidth - 8;

                        e.Graphics.DrawString("." + sTmp3After + "." + sTmp3After3, e.Font, sb, r, StringFormat.GenericDefault);

                        if (m_SmallModeFont != null)
                        {
                            string s = bse.Mode.ToString();
                            Size modeSize = TextRenderer.MeasureText(s, m_SmallModeFont);
                            e.Graphics.DrawString(bse.Mode.ToString(), m_SmallModeFont, sb, e.Bounds.Width - modeSize.Width - 24, e.Bounds.Y + ((e.Bounds.Height/2) - (modeSize.Height/2)), StringFormat.GenericDefault);

                            //e.Graphics.DrawString(bse.GUID, m_SmallModeFont, sb, e.Bounds.Width - 120, e.Bounds.Y + ((e.Bounds.Height / 2) - (modeSize.Height / 2)), StringFormat.GenericDefault);
                        }

                        sb.Dispose();
                    //}
                }
                else
                {
                    e.DrawBackground();
                }
            }
            base.OnDrawItem(e);
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
                        else if (m_nHighlighted == i)
                        {
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
