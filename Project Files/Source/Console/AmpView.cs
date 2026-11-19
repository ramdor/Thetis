/*  AmpView.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2000-2025 Original authors
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
        public AmpView()
        {
            InitializeComponent();
            Common.DoubleBufferAll(this, true);
        }

        //GCHandle hx, hym, hyc, hys, hcm, hcc, hcs;
        const int max_samps = 4096;
        double[] x  = new double[max_samps];
        double[] ym = new double[max_samps];
        double[] yc = new double[max_samps];
        double[] ys = new double[max_samps];
        const int max_correction_points = 512;
        double[] xm_cor = new double[max_correction_points];
        double[] ym_cor = new double[max_correction_points];
        double[] xa_cor = new double[max_correction_points];
        double[] ya_cor = new double[max_correction_points];
        private int _disp_nsamps;
        private int _disp_cpts;
        private double _disp_phs_ref_deg;
        private int _oldDispCpts = -1;
        int skip = 1;
        bool showgain = false;
        private static Object intslock = new Object();

        private void AmpView_Load(object sender, EventArgs e)
        {
            Common.FadeIn(this);

            /*PSForm.ampv.*/this.ClientSize = new System.Drawing.Size(560, 445); //
            Common.RestoreForm(this, "AmpView", true); //[2.10.3.5]MW0LGE  #292
            //hx  = GCHandle.Alloc(x,  GCHandleType.Pinned);
            //hym = GCHandle.Alloc(ym, GCHandleType.Pinned);
            //hyc = GCHandle.Alloc(yc, GCHandleType.Pinned);
            //hys = GCHandle.Alloc(ys, GCHandleType.Pinned);
            //hcm = GCHandle.Alloc(cm, GCHandleType.Pinned);
            //hcc = GCHandle.Alloc(cc, GCHandleType.Pinned);
            //hcs = GCHandle.Alloc(cs, GCHandleType.Pinned);
            EventArgs ex = EventArgs.Empty;
            chkAVShowGain_CheckedChanged(this, ex);
            chkAVLowRes_CheckedChanged(this, ex);
            chkAVPhaseZoom_CheckedChanged(this, ex);
            chkStayOnTop_CheckedChanged(this, ex);
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
        private void init_data(int nsamps, int cpts)
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
            for (int i = 0; i < cpts; i++)
            {
                chart1.Series["MagCorr"].Points.AddXY(0.0, 0.0);
                chart1.Series["PhsCorr"].Points.AddXY(0.0, 0.0);
            }

            for (int i = 0; i < nsamps; i++)
            {
                chart1.Series["MagAmp"].Points.AddXY(0.0, 0.0);
                chart1.Series["PhsAmp"].Points.AddXY(0.0, 0.0); // note: low res will skip every 4
            }
        }

        private void disp_data_Update(int nsamps, int cpts)
        {
            nsamps = Math.Min(nsamps, Math.Min(x.Length, Math.Min(ym.Length, Math.Min(yc.Length, ys.Length))));
            cpts = Math.Min(cpts, Math.Min(xm_cor.Length, Math.Min(ym_cor.Length, Math.Min(xa_cor.Length, ya_cor.Length))));
            if (nsamps <= 0 || cpts <= 0) return;

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

            for (int i = 0; i < cpts; i++)
            {
                double input = xm_cor[i];
                double gain = ym_cor[i];
                chart1.Series["MagCorr"].Points[i].SetValueXY(input, showgain ? gain : gain * input);
                double phase = ya_cor[i];
                if (phase > -180.0 && phase < 180.0)
                    chart1.Series["PhsCorr"].Points[i].SetValueXY(xa_cor[i], phase);
                else if (i > 0)
                {
                    DataPoint previous = chart1.Series["PhsCorr"].Points[i - 1];
                    chart1.Series["PhsCorr"].Points[i].SetValueXY(previous.XValue, previous.YValues[0]);
                }
                else
                    chart1.Series["PhsCorr"].Points[i].SetValueXY(0.0, 0.0);
            }

            int lastGoodPoint = -1;
            for (int i = 0; i < nsamps; i++)
            {
                if (i % skip == 0)
                {
                    if (!showgain)
                        chart1.Series["MagAmp"].Points[i].SetValueXY(ym[i] * x[i], x[i]);
                    else
                        chart1.Series["MagAmp"].Points[i].SetValueXY(ym[i] * x[i], ym[i] != 0.0 ? 1.0 / ym[i] : 0.0);

                    double phase = 180.0 / Math.PI * Math.Atan2(ys[i], yc[i]) - _disp_phs_ref_deg;
                    chart1.Series["PhsAmp"].Points[i].SetValueXY(x[i], phase);
                    lastGoodPoint = i;
                }
                else if (lastGoodPoint >= 0)
                {
                    DataPoint previous = chart1.Series["MagAmp"].Points[lastGoodPoint];
                    chart1.Series["MagAmp"].Points[i].SetValueXY(previous.XValue, previous.YValues[0]);
                    previous = chart1.Series["PhsAmp"].Points[lastGoodPoint];
                    chart1.Series["PhsAmp"].Points[i].SetValueXY(previous.XValue, previous.YValues[0]);
                }
                else
                {
                    chart1.Series["MagAmp"].Points[i].SetValueXY(0.0, 0.0);
                    chart1.Series["PhsAmp"].Points[i].SetValueXY(0.0, 0.0);
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

        private void chkStayOnTop_CheckedChanged(object sender, EventArgs e)
        {
            //this.TopMost = chkStayOnTop.Checked;
            FixOnTop();
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

        private int _oldDispNsamps = -1;
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
                fixed (double* pxm_cor = xm_cor)
                fixed (double* pym_cor = ym_cor)
                fixed (double* pxa_cor = xa_cor)
                fixed (double* pya_cor = ya_cor)
                {
                    puresignal.GetPSDisp(
                        WDSP.id(1, 0),
                        new IntPtr(px),
                        new IntPtr(pym),
                        new IntPtr(pyc),
                        new IntPtr(pys),
                        new IntPtr(pxm_cor),
                        new IntPtr(pym_cor),
                        new IntPtr(pxa_cor),
                        new IntPtr(pya_cor),
                        out _disp_nsamps,
                        out _disp_cpts,
                        out _disp_phs_ref_deg
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

                if (_oldDispNsamps != _disp_nsamps || _oldDispCpts != _disp_cpts)
                {
                    _oldDispNsamps = _disp_nsamps;
                    _oldDispCpts = _disp_cpts;
                    _init = true;
                }
                if (_init)
                {
                    init_data(_disp_nsamps, _disp_cpts);
                    _init = false;
                }
                disp_data_Update(_disp_nsamps, _disp_cpts);

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

        //
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        public void FixOnTop()
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)FixOnTop);
                return;
            }

            if (!IsHandleCreated) return;

            bool want_top = chkStayOnTop.Checked;

            TopMost = want_top;

            IntPtr insert_after = want_top ? HWND_TOPMOST : HWND_NOTOPMOST;
            SetWindowPos(Handle, insert_after, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            FixOnTop();
        }
        //
    }
}
