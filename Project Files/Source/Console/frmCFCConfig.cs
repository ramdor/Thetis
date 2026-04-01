/*  frmCFCConfig.cs

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
    public partial class frmCFCConfig : Form
    {
        private int _selected_index_eq;
        private int _selected_index_comp;

        private System.Threading.Timer _timer;
        private bool _busy;

        private double[] _CFCCompValues;
        private double[] _cfc_data;

        private bool _ignore_udpates;
        private bool _ignore_unselected;

        private bool _active;

        public frmCFCConfig()
        {
            _CFCCompValues = new double[1025];
            _cfc_data = null;

            _ignore_udpates = false;
            _ignore_unselected = false;

            _selected_index_eq = -1;
            _selected_index_comp = -1;

            _busy = false;

            _active = false;

            InitializeComponent();

            Common.RestoreForm(this, "CFCConfig", false);
            Common.ForceFormOnScreen(this);

            ucCFC_comp.GetDefaults(out double[] f, out double[] g, out double[] q, out double pa, out double minHz, out double maxHz, out _, out _, 10);

            ucCFC_comp.SetPointsData(f, g, q);
            ucCFC_comp.FrequencyMinHz = minHz;
            ucCFC_comp.FrequencyMaxHz = maxHz;
            ucCFC_comp.GlobalGainDb = pa;

            ucCFC_eq.SetPointsData(f, g, q);
            ucCFC_eq.FrequencyMinHz = minHz;
            ucCFC_eq.FrequencyMaxHz = maxHz;
            ucCFC_eq.GlobalGainDb = pa;

            udCFC_low.Value = (decimal)minHz;
            udCFC_high.Value = (decimal)maxHz;

            updateSelected(null);
            setTimer();
        }

        private void radCFC_bands_CheckedChanged(object sender, EventArgs e)
        {
            int bands = 10;
            if (radCFC_5.Checked) bands = 5;
            else if (radCFC_18.Checked) bands = 18;

            nudCFC_selected_band.Maximum = bands;

            ucCFC_comp.BandCount = bands;
            ucCFC_eq.BandCount = bands;
        }

        private void udCFC_low_ValueChanged(object sender, EventArgs e)
        {
            if (udCFC_low.Value > udCFC_high.Value - 1000)
            {
                udCFC_low.Value = udCFC_high.Value - 1000;
                return;
            }
            ucCFC_comp.FrequencyMinHz = (int)udCFC_low.Value;
            ucCFC_eq.FrequencyMinHz = ucCFC_comp.FrequencyMinHz;
        }

        private void udCFC_high_ValueChanged(object sender, EventArgs e)
        {
            if (udCFC_high.Value < udCFC_low.Value + 1000)
            {
                udCFC_high.Value = udCFC_low.Value + 1000;
                return;
            }
            ucCFC_comp.FrequencyMaxHz = (int)udCFC_high.Value;
            ucCFC_eq.FrequencyMaxHz = ucCFC_comp.FrequencyMaxHz;
        }

        private void nudCFC_f_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;

            int index = ucCFC_comp.SelectedIndex;
            ucCFC_comp.GetPointData(index, out double f, out double g, out double q);
            f = (double)nudCFC_f.Value;
            ucCFC_comp.SetPointData(index, f, g, q);
        }

        private void nudCFC_precomp_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;

            ucCFC_comp.GlobalGainDb = (double)nudCFC_precomp.Value;
        }

        private void nudCFC_c_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;

            int index = ucCFC_comp.SelectedIndex;
            ucCFC_comp.GetPointData(index, out double f, out double g, out double q);
            g = (double)nudCFC_c.Value;
            ucCFC_comp.SetPointData(index, f, g, q);
        }

        private void nudCFC_posteqgain_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;

            ucCFC_eq.GlobalGainDb = (double)nudCFC_posteqgain.Value;
        }

        private void nudCFC_gain_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;

            int index = ucCFC_eq.SelectedIndex;
            ucCFC_eq.GetPointData(index, out double f, out double g, out double q);
            g = (double)nudCFC_gain.Value;
            ucCFC_eq.SetPointData(index, f, g, q);
        }

        private void nudCFC_q_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;

            int index = ucCFC_eq.SelectedIndex;
            ucCFC_eq.GetPointData(index, out double f, out double g, out double q);
            q = (double)nudCFC_q.Value;
            ucCFC_eq.SetPointData(index, f, g, q);
        }

        private void nudCFC_cq_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;

            int index = ucCFC_comp.SelectedIndex;
            ucCFC_comp.GetPointData(index, out double f, out double g, out double q);
            q = (double)nudCFC_cq.Value;
            ucCFC_comp.SetPointData(index, f, g, q);
        }

        private void ucCFC_comp_GlobalGainChanged(object sender, ucParametricEq.EqDraggingEventArgs e)
        {
            if (!e.IsDragging || chkCFC_PanaEQ_live.Checked)
            {
                setCFCProfile();
            }
            else
            {
                setCFCProfile(-1, true);
            }
        }

        private void ucCFC_comp_PointDataChanged(object sender, ucParametricEq.EqPointDataChangedEventArgs e)
        {
            ucCFC_eq.SetPointHz(e.BandId, e.FrequencyHz, e.IsDragging);
            int i = ucCFC_eq.GetIndexFromBandId(e.BandId);
            ucCFC_eq.SelectedIndex = i;

            if (!e.IsDragging || chkCFC_PanaEQ_live.Checked)
            {
                setCFCProfile(e.Index);
            }
            else
            {
                setCFCProfile(e.Index, true);
            }
        }

        private void ucCFC_comp_PointsChanged(object sender, ucParametricEq.EqDraggingEventArgs e)
        {
            if (e.IsDragging) return;
            setCFCProfile();
        }

        private void ucCFC_comp_PointSelected(object sender, ucParametricEq.EqPointSelectionChangedEventArgs e)
        {
            _selected_index_comp = e.Index;
            int i = ucCFC_eq.GetIndexFromBandId(e.BandId);
            ucCFC_eq.SelectedIndex = i;
            updateSelected(e);
        }

        private void ucCFC_comp_PointUnselected(object sender, ucParametricEq.EqPointSelectionChangedEventArgs e)
        {
            if (_ignore_unselected) return;

            _selected_index_comp = -1;
            ucCFC_eq.SelectedIndex = -1;
            updateSelected(null);
        }

        private void ucCFC_eq_GlobalGainChanged(object sender, ucParametricEq.EqDraggingEventArgs e)
        {
            if (!e.IsDragging || chkCFC_PanaEQ_live.Checked)
            {
                setCFCProfile();
            }
            else
            {
                setCFCProfile(-1, true);
            }
        }

        private void ucCFC_eq_PointDataChanged(object sender, ucParametricEq.EqPointDataChangedEventArgs e)
        {
            ucCFC_comp.SetPointHz(e.BandId, e.FrequencyHz, e.IsDragging);
            int i = ucCFC_comp.GetIndexFromBandId(e.BandId);
            ucCFC_comp.SelectedIndex = i;

            if (!e.IsDragging || chkCFC_PanaEQ_live.Checked)
            {
                setCFCProfile(e.Index);
            }
            else
            {
                setCFCProfile(e.Index, true);
            }
        }

        private void ucCFC_eq_PointsChanged(object sender, ucParametricEq.EqDraggingEventArgs e)
        {
            if (e.IsDragging) return;
            setCFCProfile();
        }

        private void ucCFC_eq_PointSelected(object sender, ucParametricEq.EqPointSelectionChangedEventArgs e)
        {
            _selected_index_eq = e.Index;
            int i = ucCFC_comp.GetIndexFromBandId(e.BandId);
            ucCFC_comp.SelectedIndex = i;
            updateSelected(e);
        }

        private void ucCFC_eq_PointUnselected(object sender, ucParametricEq.EqPointSelectionChangedEventArgs e)
        {
            if (_ignore_unselected) return;

            _selected_index_eq = -1;
            ucCFC_comp.SelectedIndex = -1;
            updateSelected(null);
        }
        private int SelectedIndex
        {
            get
            {
                if (_selected_index_comp != -1) return _selected_index_comp;
                if (_selected_index_eq != -1) return _selected_index_eq;
                return -1;
            }
        }
        private void updateSelected(ucParametricEq.EqPointSelectionChangedEventArgs e)
        {
            //enable/disable controls
            bool enable = (_selected_index_comp != -1);
            nudCFC_f.Enabled = enable;
            nudCFC_precomp.Enabled = enable;
            nudCFC_c.Enabled = enable;
            nudCFC_cq.Enabled = enable;

            nudCFC_posteqgain.Enabled = enable;
            nudCFC_gain.Enabled = enable;
            nudCFC_q.Enabled = enable;

            if (SelectedIndex == -1 || e == null) return;

            nudCFC_f.Value = (decimal)e.FrequencyHz;
        }
        private void setCFCProfile(int index = -1, bool just_text = false)
        {
            double[] cf;
            double[] cg;
            double[] cq;

            double[] ef;
            double[] eg;
            double[] eq;

            ucCFC_comp.GetPointsData(out cf, out cg, out cq);
            ucCFC_eq.GetPointsData(out ef, out eg, out eq);

            double pre_comp = (double)ucCFC_comp.GlobalGainDb;
            double post_eq_gain = (double)ucCFC_eq.GlobalGainDb;

            _ignore_udpates = true;
            nudCFC_precomp.Value = (decimal)pre_comp;
            nudCFC_posteqgain.Value = (decimal)post_eq_gain;
            if (index >= 0 && index < cf.Length)
            {
                nudCFC_selected_band.Value = (decimal)(index + 1);
                nudCFC_f.Value = (decimal)cf[index];
                nudCFC_c.Value = (decimal)cg[index];
                nudCFC_cq.Value = (decimal)cq[index];

                nudCFC_gain.Value = (decimal)eg[index];
                nudCFC_q.Value = (decimal)eq[index];
            }
            _ignore_udpates = false;

            if (just_text) return;
            if (!_active) return;

            //pre comp
            WDSP.SetTXACFCOMPPrecomp(WDSP.id(1, 0), pre_comp);

            //pre eq gain
            WDSP.SetTXACFCOMPPrePeq(WDSP.id(1, 0), post_eq_gain);

            int nfreqs = cf.Length;

            //profile
            unsafe
            {
                fixed (double* Fptr = &cf[0], Gptr = &cg[0], Eptr = &eg[0], GQptr = &cq[0], EQptr = &eq[0])
                {
                    WDSP.SetTXACFCOMPprofile(WDSP.id(1, 0), nfreqs, Fptr, Gptr, Eptr, GQptr, EQptr);
                }
            }
        }

        private void timerTick(object state)
        {
            if (_busy || this.Disposing || this.IsDisposed) return;

            _busy = true;

            int ready = 0;
            unsafe
            {
                fixed (double* ptrCompValues = &_CFCCompValues[0])
                    WDSP.GetTXACFCOMPDisplayCompression(WDSP.id(1, 0), ptrCompValues, &ready);
            }

            if (ready == 1)
            {
                double start = ucCFC_comp.FrequencyMinHz;
                double stop = ucCFC_comp.FrequencyMaxHz;
                float binsPerHz = _CFCCompValues.Length / 48000f;
                int endIndex = (int)(stop * binsPerHz);
                int startIndex = (int)(start * binsPerHz);
                int len = endIndex - startIndex + 1;
                if (_cfc_data == null || _cfc_data.Length != len)
                {
                    _cfc_data = new double[len];
                }

                int i = 0;
                for (int n = startIndex; n <= endIndex; n++)
                {
                    _cfc_data[i] = _CFCCompValues[n];
                    i++;
                }

                ucCFC_comp.DrawBarChart(_cfc_data);
            }

            _busy = false;
        }

        private void frmCFCConfig_VisibleChanged(object sender, EventArgs e)
        {
            setTimer();
        }
        private void setTimer()
        {
            _timer?.Dispose();

            if (this.Visible)
            {
                _timer = new System.Threading.Timer(
                    timerTick,
                    null,
                    100,    // init delay
                    50);   // interval
            }
        }

        private void btnResetComp_Click(object sender, EventArgs e)
        {
            ucCFC_comp.SelectedIndex = -1;
            ucCFC_comp.GlobalGainDb = 0;
            ucCFC_comp.ResetPoints();
        }

        private void btnResetEQ_Click(object sender, EventArgs e)
        {
            ucCFC_eq.SelectedIndex = -1;
            ucCFC_eq.GlobalGainDb = 0;
            ucCFC_eq.ResetPoints();
        }

        private void nudCFC_selected_band_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;

            _ignore_unselected = true;
            ucCFC_comp.SelectedIndex = (int)(nudCFC_selected_band.Value - 1);
            ucCFC_eq.SelectedIndex = ucCFC_comp.SelectedIndex;
            _ignore_unselected = false;

            setCFCProfile(ucCFC_eq.SelectedIndex, true);
        }

        private void frmCFCConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, "CFCConfig");
        }

        private void chkCFC_UseQFactors_CheckedChanged(object sender, EventArgs e)
        {
            ucCFC_comp.ParametricEQ = chkCFC_UseQFactors.Checked;
            ucCFC_eq.ParametricEQ = ucCFC_comp.ParametricEQ;

            setCFCProfile(-1, false);
        }

        public string ConfigData
        {
            get
            {
                try
                {
                    string j1 = ucCFC_comp.SaveToJson();
                    string j2 = ucCFC_eq.SaveToJson();
                    string total = j1 + "<SEP>" + j2;
                    string comp = Common.Compress_gzip(total);
                    return comp;
                }
                catch
                {
                    return "";
                }
            }
            set
            {
                try
                {
                    string exp = Common.Decompress_gzip(value);
                    if (!string.IsNullOrEmpty(exp))
                    {
                        string[] parts = exp.Split(new string[] { "<SEP>" }, 2, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            string j1 = parts[0];
                            string j2 = parts[1];

                            bool ok = ucCFC_comp.LoadFromJson(j1);
                            ok |= ucCFC_eq.LoadFromJson(j2);

                            if (ok)
                            {
                                _ignore_udpates = true;
                                switch (ucCFC_comp.BandCount)
                                {
                                    case 5:
                                        radCFC_5.Checked = true;
                                        nudCFC_selected_band.Maximum = 5;
                                        break;
                                    case 10:
                                        radCFC_10.Checked = true;
                                        nudCFC_selected_band.Maximum = 10;
                                        break;
                                    case 18:
                                        radCFC_18.Checked = true;
                                        nudCFC_selected_band.Maximum = 18;
                                        break;
                                }

                                chkCFC_UseQFactors.Checked = ucCFC_comp.ParametricEQ;

                                udCFC_low.Value = (decimal)ucCFC_comp.FrequencyMinHz;
                                udCFC_high.Value = (decimal)ucCFC_comp.FrequencyMaxHz;
                                _ignore_udpates = false;

                                setCFCProfile(-1, false);

                                _selected_index_comp = -1;
                                _selected_index_eq = -1;
                                updateSelected(null);

                                return;
                            }
                        }
                    }
                }
                catch { }

                radCFC_10.Checked = true;
                ucCFC_comp.BandCount = 10;
                nudCFC_selected_band.Maximum = 10;
                ucCFC_comp.GlobalGainDb = 0;
                ucCFC_comp.ResetPoints();
                ucCFC_eq.BandCount = 10;
                ucCFC_eq.GlobalGainDb = 0;
                ucCFC_eq.ResetPoints();

                _selected_index_comp = -1;
                _selected_index_eq = -1;
                updateSelected(null);
            }
        }

        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                if (_active)
                {
                    setCFCProfile(-1, false);
                }
            }
        }
        public void HighlightTXProfileSaveItems(bool bHighlight)
        {
            Common.HightlightControl(this, bHighlight);
        }

        private void lblOGGuide_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Common.OpenUri("https://www.w1aex.com/anan/CFC_Audio_Tools/CFC_Audio_Tools.html");
        }

        private void chkLogScale_CheckedChanged(object sender, EventArgs e)
        {
            ucCFC_comp.LogScale = chkLogScale.Checked;
            ucCFC_eq.LogScale = ucCFC_comp.LogScale;
        }
    }
}
