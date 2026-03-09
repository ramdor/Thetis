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

        public frmCFCConfig()
        {
            _CFCCompValues = new double[1025];
            _cfc_data = null;

            _ignore_udpates = false;

            _selected_index_eq = -1;
            _selected_index_comp = -1;

            _busy = false;

            setTimer();

            InitializeComponent();
        }

        private void frmCFCConfig_Load(object sender, EventArgs e)
        {

        }

        private void radCFC_bands_CheckedChanged(object sender, EventArgs e)
        {
            int bands = 10;
            if (radCFC_5.Checked) bands = 5;
            else if (radCFC_10.Checked) bands = 10;
            else if (radCFC_18.Checked) bands = 18;

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
        }

        private void nudCFC_precomp_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;
        }

        private void nudCFC_c_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;
        }

        private void nudCFC_posteqgain_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;
        }

        private void nudCFC_gain_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;
        }

        private void nudCFC_q_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;
        }

        private void nudCFC_cq_ValueChanged(object sender, EventArgs e)
        {
            if (_ignore_udpates) return;
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
            nudCFC_f.Enabled = (_selected_index_comp != -1);
            nudCFC_precomp.Enabled = (_selected_index_comp != -1);
            nudCFC_c.Enabled = (_selected_index_comp != -1);

            nudCFC_posteqgain.Enabled = (_selected_index_eq != -1);
            nudCFC_gain.Enabled = (_selected_index_eq != -1);
            nudCFC_q.Enabled = (_selected_index_eq != -1);

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

            //pre comp
            WDSP.SetTXACFCOMPPrecomp(WDSP.id(1, 0), pre_comp);

            //pre eq gain
            WDSP.SetTXACFCOMPPrePeq(WDSP.id(1, 0), post_eq_gain);

            int nfreqs = cf.Length;

            //profile
            unsafe
            {
                fixed (double* Fptr = &cf[0], Gptr = &cg[0], Eptr = &eg[0], GQptr  = &cq[0], EQptr = &eq[0])
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
                if(_cfc_data == null || _cfc_data.Length != len)
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
            ucCFC_comp.GlobalGainDb = 0;
            ucCFC_comp.ResetPoints();
        }

        private void btnResetEQ_Click(object sender, EventArgs e)
        {
            ucCFC_eq.GlobalGainDb = 0;
            ucCFC_eq.ResetPoints();
        }
    }
}
