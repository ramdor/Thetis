using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;
using System.Diagnostics;

namespace Thetis
{
    public unsafe partial class AmpView : Form
    {
        private PSForm psform;
        public AmpView(PSForm ps)
        {
            InitializeComponent();
            Common.DoubleBufferAll(this, true);

            psform = ps;
        }

        //GCHandle hx, hym, hyc, hys, hcm, hcc, hcs;
        const int max_ints = 16;
        const int max_samps = 4096;
        const int np = 512;
        double[] x  = new double[max_samps];
        double[] ym = new double[max_samps];
        double[] yc = new double[max_samps];
        double[] ys = new double[max_samps];
        double[] cm = new double[4 * max_ints];
        double[] cc = new double[4 * max_ints];
        double[] cs = new double[4 * max_ints];
        double[] t  = new double[max_ints + 1];
        int skip = 1;
        bool showgain = false;
        private static Object intslock = new Object();

        private void AmpView_Load(object sender, EventArgs e)
        {
            Common.FadeIn(this);

            PSForm.ampv.ClientSize = new System.Drawing.Size(560, 445); //
            Common.RestoreForm(this, "AmpView", true); //[2.10.3.5]MW0LGE  #292
            //hx  = GCHandle.Alloc(x,  GCHandleType.Pinned);
            //hym = GCHandle.Alloc(ym, GCHandleType.Pinned);
            //hyc = GCHandle.Alloc(yc, GCHandleType.Pinned);
            //hys = GCHandle.Alloc(ys, GCHandleType.Pinned);
            //hcm = GCHandle.Alloc(cm, GCHandleType.Pinned);
            //hcc = GCHandle.Alloc(cc, GCHandleType.Pinned);
            //hcs = GCHandle.Alloc(cs, GCHandleType.Pinned);
            double delta = 1.0 / (double)psform.Ints;
            t[0] = 0.0;
            for (int i = 1; i <= psform.Ints; i++)
                t[i] = t[i - 1] + delta;
            EventArgs ex = EventArgs.Empty;
            chkAVShowGain_CheckedChanged(this, ex);
            chkAVLowRes_CheckedChanged(this, ex);
            chkAVPhaseZoom_CheckedChanged(this, ex);
        }
        private void disp_setup()
        {
            chart1.ChartAreas[0].AxisX.Minimum = 0.0;
            chart1.ChartAreas[0].AxisX.Maximum = 1.0;
            chart1.ChartAreas[0].AxisY.Minimum = 0.0;
            chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.LightSalmon;
            chart1.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.LightSalmon;
            chart1.ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.LightSalmon;
            chart1.ChartAreas[0].AxisX.Title = "Input Magnitude";
            chart1.ChartAreas[0].AxisX.TitleForeColor = Color.LightSalmon;
            chart1.ChartAreas[0].AxisY.TitleForeColor = Color.LightSalmon;
            chart1.ChartAreas[0].AxisY2.Title = "Phase";
            chart1.ChartAreas[0].AxisY2.TitleForeColor = Color.LightSalmon;
        }

        // MW0LGE [2.9.0.8] re-factored to use fixed set of chart points, which get adjusted, these poins are re-init under certain conditions
        private void init_data(int ints, int spi)
        {
            chart1.Series["Ref"].Points.Clear();
            chart1.Series["MagCorr"].Points.Clear();
            chart1.Series["PhsCorr"].Points.Clear();
            chart1.Series["MagAmp"].Points.Clear();
            chart1.Series["PhsAmp"].Points.Clear();
            if (!showgain)
            {
                chart1.Series["Ref"].Points.AddXY(0.0, 0.0);
                chart1.Series["Ref"].Points.AddXY(1.0, 1.0);
                chart1.Series["Ref"].Points.AddXY(1.0, 0.5);
                chart1.Series["Ref"].Points.AddXY(0.0, 0.5);
            }
            else
            {
                chart1.Series["Ref"].Points.AddXY(0.0, 1.0);
                chart1.Series["Ref"].Points.AddXY(1.0, 1.0);
            }
            chart1.Series["MagCorr"].Points.AddXY(0.0, 0.0);
            for (int i = 1; i <= np; i++)
            {
                chart1.Series["MagCorr"].Points.AddXY(0.0, 0.0);                
                chart1.Series["PhsCorr"].Points.AddXY(0.0, 0.0); // note: not always np number of entries, disp_data_Update handles this
            }

            for (int i = 0; i < ints * spi; i++) // += skip)
            {
                chart1.Series["MagAmp"].Points.AddXY(0.0, 0.0);
                chart1.Series["PhsAmp"].Points.AddXY(0.0, 0.0); // note: low res will skip every 4
            }
        }

        private void disp_data_Update(int ints, int spi)
        {
            double delta = 1.0 / (double)np;
            double qx = delta;
            double dx;
            double qym, qyc, qys, phs;
            double phs_base;
            int k;
            double dt = 1.0 / (double)ints;
            t[0] = 0.0;
            for (int i = 1; i <= ints; i++)
                t[i] = t[i - 1] + dt;

            if (!showgain)
            {
                chart1.Series["Ref"].Points[0].SetValueXY(0.0, 0.0);
                chart1.Series["Ref"].Points[1].SetValueXY(1.0, 1.0);
                chart1.Series["Ref"].Points[2].SetValueXY(1.0, 0.5);
                chart1.Series["Ref"].Points[3].SetValueXY(0.0, 0.5);
            }
            else
            {
                chart1.Series["Ref"].Points[0].SetValueXY(0.0, 1.0);
                chart1.Series["Ref"].Points[1].SetValueXY(1.0, 1.0);
            }
            chart1.Series["MagCorr"].Points[0].SetValueXY(0.0, 0.0);
            k = ints - 1;
            dx = t[ints] - t[ints - 1];
            qyc = cc[4 * k + 0] + dx * (cc[4 * k + 1] + dx * (cc[4 * k + 2] + dx * cc[4 * k + 3]));
            qys = cs[4 * k + 0] + dx * (cs[4 * k + 1] + dx * (cs[4 * k + 2] + dx * cs[4 * k + 3]));
            phs_base = 180.0 / Math.PI * Math.Atan2(qys, qyc);
            int nLastGoodPoint = -1;
            for (int i = 1; i <= np; i++)
            {
                if ((k = (int)(qx * ints)) > ints - 1) k = ints - 1;
                dx = qx - t[k];
                qym = cm[4 * k + 0] + dx * (cm[4 * k + 1] + dx * (cm[4 * k + 2] + dx * cm[4 * k + 3]));
                qyc = cc[4 * k + 0] + dx * (cc[4 * k + 1] + dx * (cc[4 * k + 2] + dx * cc[4 * k + 3]));
                qys = cs[4 * k + 0] + dx * (cs[4 * k + 1] + dx * (cs[4 * k + 2] + dx * cs[4 * k + 3]));
                if (!showgain)
                    chart1.Series["MagCorr"].Points[i].SetValueXY(qx, qym * qx);
                else
                    chart1.Series["MagCorr"].Points[i].SetValueXY(qx, qym);
                phs = 180.0 / Math.PI * Math.Atan2(qys, qyc) - phs_base;
                if (phs > -180.0 && phs < +180.0)
                {
                    chart1.Series["PhsCorr"].Points[i-1].SetValueXY(qx, phs);
                    nLastGoodPoint = i-1;
                }
                else
                {
                    // no data, so need to use last known x/y so that un-used point is 'invisible'
                    if (nLastGoodPoint != -1)
                    {
                        DataPoint dp = chart1.Series["PhsCorr"].Points[nLastGoodPoint];
                        chart1.Series["PhsCorr"].Points[i - 1].SetValueXY((double)dp.XValue, (double)dp.YValues[0]);
                    }
                    else
                    {
                        chart1.Series["PhsCorr"].Points[i - 1].SetValueXY(0.0, 0.0);
                    }
                    
                }
                qx += delta;
            }
            k = ints * spi - 1;
            phs_base = 180.0 / Math.PI * Math.Atan2(yc[k], ys[k]);

            nLastGoodPoint = -1;
            for (int i = 0; i < ints * spi; i++)
            {
                if (i % skip == 0)
                {
                    if (!showgain)
                        chart1.Series["MagAmp"].Points[i].SetValueXY(ym[i] * x[i], x[i]);
                    else
                        chart1.Series["MagAmp"].Points[i].SetValueXY(ym[i] * x[i], 1.0 / ym[i]);
                    phs = 180.0 / Math.PI * Math.Atan2(yc[i], ys[i]) - phs_base;
                    chart1.Series["PhsAmp"].Points[i].SetValueXY(x[i], phs);

                    nLastGoodPoint = i;
                }
                else
                {
                    // no data, so need to use last known x/y so that un-used point is 'invisible'
                    if (nLastGoodPoint != -1)
                    {
                        DataPoint dp;
                        dp = chart1.Series["MagAmp"].Points[nLastGoodPoint];
                        chart1.Series["MagAmp"].Points[i].SetValueXY((double)dp.XValue, (double)dp.YValues[0]);

                        dp = chart1.Series["PhsAmp"].Points[nLastGoodPoint];
                        chart1.Series["PhsAmp"].Points[i].SetValueXY((double)dp.XValue, (double)dp.YValues[0]);
                    }
                    else
                    {
                        chart1.Series["MagAmp"].Points[i].SetValueXY(0.0, 0.0);
                        chart1.Series["PhsAmp"].Points[i].SetValueXY(0.0, 0.0);
                    }
                }
            }
        }

        // MW0LGE [2.9.0.8] kept for code record
        //private void disp_data()
        //{
        //    double delta = 1.0 / (double)np;
        //    double qx = delta;
        //    double dx;
        //    double qym, qyc, qys, phs;
        //    double phs_base;
        //    int k;
        //    int ints = psform.Ints;
        //    int spi = psform.Spi;
        //    double dt = 1.0 / (double)ints;
        //    t[0] = 0.0;
        //    for (int i = 1; i <= ints; i++)
        //        t[i] = t[i - 1] + dt;
        //    chart1.Series["Ref"].Points.Clear();
        //    chart1.Series["MagCorr"].Points.Clear();
        //    chart1.Series["PhsCorr"].Points.Clear();
        //    chart1.Series["MagAmp"].Points.Clear();
        //    chart1.Series["PhsAmp"].Points.Clear();
        //    if (!showgain)
        //    {
        //        chart1.Series["Ref"].Points.AddXY(0.0, 0.0);
        //        chart1.Series["Ref"].Points.AddXY(1.0, 1.0);
        //        chart1.Series["Ref"].Points.AddXY(1.0, 0.5);
        //        chart1.Series["Ref"].Points.AddXY(0.0, 0.5);
        //    }
        //    else
        //    {
        //        chart1.Series["Ref"].Points.AddXY(0.0, 1.0);
        //        chart1.Series["Ref"].Points.AddXY(1.0, 1.0);
        //    }
        //    chart1.Series["MagCorr"].Points.AddXY(0.0, 0.0);
        //    k = ints - 1;
        //    dx = t[ints] - t[ints - 1];
        //    qyc = cc[4 * k + 0] + dx * (cc[4 * k + 1] + dx * (cc[4 * k + 2] + dx * cc[4 * k + 3]));
        //    qys = cs[4 * k + 0] + dx * (cs[4 * k + 1] + dx * (cs[4 * k + 2] + dx * cs[4 * k + 3]));
        //    phs_base = 180.0 / Math.PI * Math.Atan2(qys, qyc);
        //    for (int i = 1; i <= np; i++)
        //    {
        //        if ((k = (int)(qx * ints)) > ints - 1) k = ints - 1;
        //        dx = qx - t[k];
        //        qym = cm[4 * k + 0] + dx * (cm[4 * k + 1] + dx * (cm[4 * k + 2] + dx * cm[4 * k + 3]));
        //        qyc = cc[4 * k + 0] + dx * (cc[4 * k + 1] + dx * (cc[4 * k + 2] + dx * cc[4 * k + 3]));
        //        qys = cs[4 * k + 0] + dx * (cs[4 * k + 1] + dx * (cs[4 * k + 2] + dx * cs[4 * k + 3]));
        //        if (!showgain)
        //            chart1.Series["MagCorr"].Points.AddXY(qx, qym * qx);
        //        else
        //            chart1.Series["MagCorr"].Points.AddXY(qx, qym);
        //        phs = 180.0 / Math.PI * Math.Atan2(qys, qyc) - phs_base;
        //        if (phs > -180.0 && phs < +180.0)
        //            chart1.Series["PhsCorr"].Points.AddXY(qx, phs);
        //        qx += delta;
        //    }
        //    k = ints * spi - 1;
        //    phs_base = 180.0 / Math.PI * Math.Atan2(yc[k], ys[k]);
        //    for (int i = 0; i < ints * spi; i += skip)
        //    {
        //        if (!showgain)
        //            chart1.Series["MagAmp"].Points.AddXY(ym[i] * x[i], x[i]);
        //        else
        //            chart1.Series["MagAmp"].Points.AddXY(ym[i] * x[i], 1.0 / ym[i]);
        //        phs = 180.0 / Math.PI * Math.Atan2(yc[i], ys[i]) - phs_base;
        //        chart1.Series["PhsAmp"].Points.AddXY(x[i], phs);
        //    }
        //}

        private bool _init = true;
        private bool _is_closing = false;
        private bool _in_timer = false;

        private void chkStayOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkStayOnTop.Checked;
        }

        public void CloseDown()
        {
            _is_closing = true;
            timer1.Stop();
            this.Close();
            Application.ExitThread();
        }

        //private void AmpView_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    if (hx.IsAllocated) hx.Free();
        //    if (hym.IsAllocated) hym.Free();
        //    if (hyc.IsAllocated) hyc.Free();
        //    if (hys.IsAllocated) hys.Free();
        //    if (hcm.IsAllocated) hcm.Free();
        //    if (hcc.IsAllocated) hcc.Free();
        //    if (hcs.IsAllocated) hcs.Free();
        //}

        private int _oldIntsSpi = -1;
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();

            if (_is_closing) return;

            disp_setup();

            //puresignal.GetPSDisp(WDSP.id(1, 0),
            //    hx.AddrOfPinnedObject(),
            //    hym.AddrOfPinnedObject(),
            //    hyc.AddrOfPinnedObject(),
            //    hys.AddrOfPinnedObject(),
            //    hcm.AddrOfPinnedObject(),
            //    hcc.AddrOfPinnedObject(),
            //    hcs.AddrOfPinnedObject());
            unsafe
            {
                fixed (double* px = x)
                fixed (double* pym = ym)
                fixed (double* pyc = yc)
                fixed (double* pys = ys)
                fixed (double* pcm = cm)
                fixed (double* pcc = cc)
                fixed (double* pcs = cs)
                {
                    puresignal.GetPSDisp(
                        WDSP.id(1, 0),
                        new IntPtr(px),
                        new IntPtr(pym),
                        new IntPtr(pyc),
                        new IntPtr(pys),
                        new IntPtr(pcm),
                        new IntPtr(pcc),
                        new IntPtr(pcs)
                    );
                }
            }
            //

            lock (intslock)
            {
                //disp_data(); // MW0LGE [2.9.0.8] changed to an add once, update points method.
                               // Prevents the chart from having 1000's of points added and removed
                               // 10 times a second.
                chart1.Series.SuspendUpdates();
                chart1.Series["Ref"].Points.SuspendUpdates();
                chart1.Series["MagCorr"].Points.SuspendUpdates();
                chart1.Series["PhsCorr"].Points.SuspendUpdates();
                chart1.Series["MagAmp"].Points.SuspendUpdates();
                chart1.Series["PhsAmp"].Points.SuspendUpdates();

                int ints = psform.Ints;
                int spi = psform.Spi;
                int instSpiTot = ints * spi;
                if (_oldIntsSpi != instSpiTot)
                {
                    _oldIntsSpi = instSpiTot;
                    _init = true;
                }
                if (_init)
                {
                    init_data(ints, spi);
                    _init = false;
                }
                disp_data_Update(ints, spi);

                chart1.Series["PhsAmp"].Points.ResumeUpdates();
                chart1.Series["MagAmp"].Points.ResumeUpdates();
                chart1.Series["PhsCorr"].Points.ResumeUpdates();
                chart1.Series["MagCorr"].Points.ResumeUpdates();
                chart1.Series["Ref"].Points.ResumeUpdates();
                chart1.Series.ResumeUpdates();

                chart1.Invalidate();
            }

            if(!_is_closing) timer1.Start();
        }

        private void chkAVShowGain_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAVShowGain.Checked)
            {
                chart1.ChartAreas[0].AxisY.Title = "Gain";
                chart1.ChartAreas[0].AxisY.Maximum = 2.0;
                chart1.Series["MagCorr"].LegendText = "Gain Corr";
                chart1.Series["_magamp"].LegendText = "Gain Amp";
                showgain = true;
            }
            else
            {
                chart1.ChartAreas[0].AxisY.Title = "Magnitude";
                chart1.ChartAreas[0].AxisY.Maximum = 1.0;
                chart1.Series["MagCorr"].LegendText = "Mag Corr";
                chart1.Series["_magamp"].LegendText = "Mag Amp";
                showgain = false;
            }

            _init = true;
        }

        private void chkAVLowRes_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAVLowRes.Checked)
                skip = 4;
            else
                skip = 1;
        }

        private void AmpView_FormClosing(object sender, FormClosingEventArgs e)
        {
            Common.SaveForm(this, "AmpView");
        }

        private void chkAVPhaseZoom_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAVPhaseZoom.Checked)
            {
                chart1.ChartAreas[0].AxisY2.Minimum = -45.0;
                chart1.ChartAreas[0].AxisY2.Maximum = +45.0;
            }
            else
            {
                chart1.ChartAreas[0].AxisY2.Minimum = -180.0;
                chart1.ChartAreas[0].AxisY2.Maximum = +180.0;
            }
        }

        private void AmpView_FormClosed(object sender, FormClosedEventArgs e)
        {
            PSForm.ampv = null;
        }
    }
}
