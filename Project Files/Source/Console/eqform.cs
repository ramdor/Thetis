//=================================================================
// eqform.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// You may contact us via email at: sales@flex-radio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    8900 Marybank Dr.
//    Austin, TX 78750
//    USA
//=================================================================
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

namespace Thetis
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Summary description for EQForm.
    /// </summary>
    public class EQForm : System.Windows.Forms.Form
    {
        #region Variable Declaration

        private Console console;
        private System.Windows.Forms.GroupBoxTS grpRXEQ;
        private System.Windows.Forms.GroupBoxTS grpTXEQ;
        private System.Windows.Forms.TrackBarTS tbRXEQ1;
        private System.Windows.Forms.TrackBarTS tbRXEQ2;
        private System.Windows.Forms.TrackBarTS tbRXEQ3;
        private System.Windows.Forms.TrackBarTS tbTXEQ2;
        private System.Windows.Forms.TrackBarTS tbTXEQ0;
        private System.Windows.Forms.TrackBarTS tbTXEQ1;
        private System.Windows.Forms.LabelTS lblRXEQ0dB;
        private System.Windows.Forms.LabelTS lblTXEQ0dB;
        private System.Windows.Forms.LabelTS lblRXEQ1;
        private System.Windows.Forms.LabelTS lblRXEQ2;
        private System.Windows.Forms.LabelTS lblRXEQ3;
        private System.Windows.Forms.LabelTS lblRXEQPreamp;
        private System.Windows.Forms.LabelTS lblTXEQPreamp;
        private System.Windows.Forms.CheckBoxTS chkTXEQEnabled;
        private System.Windows.Forms.TrackBarTS tbRXEQPreamp;
        private System.Windows.Forms.TrackBarTS tbTXEQPre;
        private System.Windows.Forms.CheckBoxTS chkRXEQEnabled;
        private System.Windows.Forms.PictureBox picRXEQ;
        private System.Windows.Forms.ButtonTS btnRXEQReset;
        private System.Windows.Forms.LabelTS lblRXEQ15db;
        private System.Windows.Forms.LabelTS lblTXEQ15db;
        private System.Windows.Forms.LabelTS lblRXEQminus12db;
        private System.Windows.Forms.LabelTS lblTXEQminus12db;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TrackBarTS tbRXEQ4;
        private System.Windows.Forms.TrackBarTS tbRXEQ5;
        private System.Windows.Forms.TrackBarTS tbRXEQ6;
        private System.Windows.Forms.TrackBarTS tbRXEQ7;
        private System.Windows.Forms.TrackBarTS tbRXEQ8;
        private System.Windows.Forms.TrackBarTS tbRXEQ9;
        private System.Windows.Forms.TrackBarTS tbRXEQ10;
        private System.Windows.Forms.LabelTS lblRXEQ15db2;
        private System.Windows.Forms.LabelTS lblRXEQ0dB2;
        private System.Windows.Forms.LabelTS lblRXEQminus12db2;
        private System.Windows.Forms.LabelTS lblTXEQ15db2;
        private System.Windows.Forms.LabelTS lblTXEQ0dB2;
        private System.Windows.Forms.LabelTS lblTXEQminus12db2;
        private System.Windows.Forms.RadioButtonTS rad3Band;
        private System.Windows.Forms.RadioButtonTS rad10Band;
        private System.Windows.Forms.TrackBarTS tbTXEQ3;
        private System.Windows.Forms.TrackBarTS tbTXEQ4;
        private System.Windows.Forms.TrackBarTS tbTXEQ5;
        private System.Windows.Forms.TrackBarTS tbTXEQ6;
        private System.Windows.Forms.TrackBarTS tbTXEQ7;
        private System.Windows.Forms.TrackBarTS tbTXEQ8;
        private System.Windows.Forms.TrackBarTS tbTXEQ9;
        private System.Windows.Forms.LabelTS lblRXEQ4;
        private System.Windows.Forms.LabelTS lblRXEQ5;
        private System.Windows.Forms.LabelTS lblRXEQ6;
        private System.Windows.Forms.LabelTS lblRXEQ7;
        private System.Windows.Forms.LabelTS lblRXEQ8;
        private System.Windows.Forms.LabelTS lblRXEQ9;
        private System.Windows.Forms.LabelTS lblRXEQ10;
        private LabelTS lblCFCFreq;
        private NumericUpDownTS udTXEQ9;
        private NumericUpDownTS udTXEQ8;
        private NumericUpDownTS udTXEQ7;
        private NumericUpDownTS udTXEQ6;
        private NumericUpDownTS udTXEQ5;
        private NumericUpDownTS udTXEQ4;
        private NumericUpDownTS udTXEQ3;
        private NumericUpDownTS udTXEQ2;
        private NumericUpDownTS udTXEQ1;
        private NumericUpDownTS udTXEQ0;
        private PanelTS pnlLegacyEQ;
        private CheckBoxTS chkLegacyEQ;
        private PanelTS pnlParaEQ;
        private RadioButtonTS radParaEQ_RX;
        private RadioButtonTS radParaEQ_TX;
        private ucParametricEq ucParametricEq1;
        private System.ComponentModel.IContainer components;

        private Point _legacyEQ_base_panel_location;
        private Size _legacyEQ_base_form_size;
        private CheckBoxTS chkUseQFactors;
        private ButtonTS btnParaEQReset;
        private RadioButtonTS radParaEQ_18;
        private RadioButtonTS radParaEQ_10;
        private PanelTS panelTS1;
        private CheckBoxTS chkParaEQ_enabled;
        private bool _initalising;
        private NumericUpDownTS udParaEQ_high;
        private NumericUpDownTS udParaEQ_low;
        private LabelTS labelTS2;
        private LabelTS labelTS1;
        private LabelTS labelTS3;
        private NumericUpDownTS nudParaEQ_selected_band;
        private LabelTS labelTS6;
        private NumericUpDownTS nudParaEQ_q;
        private LabelTS labelTS5;
        private LabelTS labelTS4;
        private NumericUpDownTS nudParaEQ_gain;
        private LabelTS labelTS8;
        private LabelTS labelTS7;
        private NumericUpDownTS nudParaEQ_f;
        private CheckBoxTS chkPanaEQ_live;
        private LabelTS labelTS10;
        private LabelTS labelTS9;
        private NumericUpDownTS nudParaEQ_preamp;
        private PictureBox pbParaEQ_live_warning;
        private PanelTS pnlParaEQ2;
        private RadioButtonTS radParaEQ_5;

        private System.Threading.Timer _dspUpdateTimer;
        private bool _wdspIsBusy = false;
        private object _dspLock = new object();

        // Temp buffers for RX and TX data
        private bool _pendingRX_Update = false;
        private double[] _tempRX_F;
        private double[] _tempRX_G;
        private double[] _tempRX_Q;
        private int _tempRX_BandCount;

        private bool _pendingTX_Update = false;
        private double[] _tempTX_F;
        private double[] _tempTX_G;
        private double[] _tempTX_Q;
        private int _tempTX_BandCount;
        #endregion

        #region Constructor and Destructor

        public EQForm(Console c)
        {
            _initalising = true;
            InitializeComponent();
            console = c;

            _state = new ParaEQState(this);

            ucParametricEq1.GetDefaults(out _state.RX_F, out _state.RX_G, out _state.RX_Q, out _state.RX_Preamp, out _state.RX_minHz, out _state.RX_maxHz, out _state.RX_ParametricEQ, out _state.RX_BandCount, 10);
            ucParametricEq1.GetDefaults(out _state.TX_F, out _state.TX_G, out _state.TX_Q, out _state.TX_Preamp, out _state.TX_minHz, out _state.TX_maxHz, out _state.TX_ParametricEQ, out _state.TX_BandCount, 10);            

            _legacyEQ_base_panel_location = pnlLegacyEQ.Location;
            _legacyEQ_base_form_size = this.MinimumSize;
            this.Size = _legacyEQ_base_form_size;

            Common.RestoreForm(this, "EQForm", false);
            Common.ForceFormOnScreen(this); //MW0LGE [2.9.0.7]                                                       

            _initalising = false;

            _state.UsingLegacyEQ = chkLegacyEQ.Checked;
            setupLowHigh();

            chkLegacyEQ_CheckedChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EQForm));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.chkPanaEQ_live = new System.Windows.Forms.CheckBoxTS();
            this.lblRXEQ9 = new System.Windows.Forms.LabelTS();
            this.lblRXEQ5 = new System.Windows.Forms.LabelTS();
            this.lblRXEQ1 = new System.Windows.Forms.LabelTS();
            this.pbParaEQ_live_warning = new System.Windows.Forms.PictureBox();
            this.pnlParaEQ = new System.Windows.Forms.PanelTS();
            this.labelTS10 = new System.Windows.Forms.LabelTS();
            this.labelTS9 = new System.Windows.Forms.LabelTS();
            this.nudParaEQ_preamp = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS8 = new System.Windows.Forms.LabelTS();
            this.labelTS5 = new System.Windows.Forms.LabelTS();
            this.labelTS7 = new System.Windows.Forms.LabelTS();
            this.btnParaEQReset = new System.Windows.Forms.ButtonTS();
            this.radParaEQ_TX = new System.Windows.Forms.RadioButtonTS();
            this.chkParaEQ_enabled = new System.Windows.Forms.CheckBoxTS();
            this.radParaEQ_RX = new System.Windows.Forms.RadioButtonTS();
            this.nudParaEQ_f = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS6 = new System.Windows.Forms.LabelTS();
            this.nudParaEQ_q = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS4 = new System.Windows.Forms.LabelTS();
            this.nudParaEQ_gain = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS3 = new System.Windows.Forms.LabelTS();
            this.nudParaEQ_selected_band = new System.Windows.Forms.NumericUpDownTS();
            this.ucParametricEq1 = new Thetis.ucParametricEq();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.udParaEQ_high = new System.Windows.Forms.NumericUpDownTS();
            this.udParaEQ_low = new System.Windows.Forms.NumericUpDownTS();
            this.panelTS1 = new System.Windows.Forms.PanelTS();
            this.radParaEQ_5 = new System.Windows.Forms.RadioButtonTS();
            this.radParaEQ_10 = new System.Windows.Forms.RadioButtonTS();
            this.radParaEQ_18 = new System.Windows.Forms.RadioButtonTS();
            this.chkUseQFactors = new System.Windows.Forms.CheckBoxTS();
            this.chkLegacyEQ = new System.Windows.Forms.CheckBoxTS();
            this.pnlLegacyEQ = new System.Windows.Forms.PanelTS();
            this.rad3Band = new System.Windows.Forms.RadioButtonTS();
            this.rad10Band = new System.Windows.Forms.RadioButtonTS();
            this.grpTXEQ = new System.Windows.Forms.GroupBoxTS();
            this.lblCFCFreq = new System.Windows.Forms.LabelTS();
            this.udTXEQ9 = new System.Windows.Forms.NumericUpDownTS();
            this.udTXEQ8 = new System.Windows.Forms.NumericUpDownTS();
            this.udTXEQ7 = new System.Windows.Forms.NumericUpDownTS();
            this.udTXEQ6 = new System.Windows.Forms.NumericUpDownTS();
            this.udTXEQ5 = new System.Windows.Forms.NumericUpDownTS();
            this.udTXEQ4 = new System.Windows.Forms.NumericUpDownTS();
            this.udTXEQ3 = new System.Windows.Forms.NumericUpDownTS();
            this.udTXEQ2 = new System.Windows.Forms.NumericUpDownTS();
            this.udTXEQ1 = new System.Windows.Forms.NumericUpDownTS();
            this.udTXEQ0 = new System.Windows.Forms.NumericUpDownTS();
            this.lblTXEQ15db2 = new System.Windows.Forms.LabelTS();
            this.lblTXEQ0dB2 = new System.Windows.Forms.LabelTS();
            this.lblTXEQminus12db2 = new System.Windows.Forms.LabelTS();
            this.tbTXEQ9 = new System.Windows.Forms.TrackBarTS();
            this.tbTXEQ6 = new System.Windows.Forms.TrackBarTS();
            this.tbTXEQ7 = new System.Windows.Forms.TrackBarTS();
            this.tbTXEQ8 = new System.Windows.Forms.TrackBarTS();
            this.tbTXEQ3 = new System.Windows.Forms.TrackBarTS();
            this.tbTXEQ4 = new System.Windows.Forms.TrackBarTS();
            this.tbTXEQ5 = new System.Windows.Forms.TrackBarTS();
            this.chkTXEQEnabled = new System.Windows.Forms.CheckBoxTS();
            this.tbTXEQ0 = new System.Windows.Forms.TrackBarTS();
            this.tbTXEQ1 = new System.Windows.Forms.TrackBarTS();
            this.tbTXEQ2 = new System.Windows.Forms.TrackBarTS();
            this.lblTXEQPreamp = new System.Windows.Forms.LabelTS();
            this.tbTXEQPre = new System.Windows.Forms.TrackBarTS();
            this.lblTXEQ15db = new System.Windows.Forms.LabelTS();
            this.lblTXEQ0dB = new System.Windows.Forms.LabelTS();
            this.lblTXEQminus12db = new System.Windows.Forms.LabelTS();
            this.grpRXEQ = new System.Windows.Forms.GroupBoxTS();
            this.lblRXEQ15db2 = new System.Windows.Forms.LabelTS();
            this.lblRXEQ0dB2 = new System.Windows.Forms.LabelTS();
            this.lblRXEQminus12db2 = new System.Windows.Forms.LabelTS();
            this.lblRXEQ10 = new System.Windows.Forms.LabelTS();
            this.tbRXEQ10 = new System.Windows.Forms.TrackBarTS();
            this.lblRXEQ7 = new System.Windows.Forms.LabelTS();
            this.lblRXEQ8 = new System.Windows.Forms.LabelTS();
            this.tbRXEQ7 = new System.Windows.Forms.TrackBarTS();
            this.tbRXEQ8 = new System.Windows.Forms.TrackBarTS();
            this.tbRXEQ9 = new System.Windows.Forms.TrackBarTS();
            this.tbRXEQ4 = new System.Windows.Forms.TrackBarTS();
            this.tbRXEQ5 = new System.Windows.Forms.TrackBarTS();
            this.tbRXEQ6 = new System.Windows.Forms.TrackBarTS();
            this.lblRXEQ4 = new System.Windows.Forms.LabelTS();
            this.lblRXEQ6 = new System.Windows.Forms.LabelTS();
            this.picRXEQ = new System.Windows.Forms.PictureBox();
            this.btnRXEQReset = new System.Windows.Forms.ButtonTS();
            this.chkRXEQEnabled = new System.Windows.Forms.CheckBoxTS();
            this.tbRXEQ1 = new System.Windows.Forms.TrackBarTS();
            this.tbRXEQ2 = new System.Windows.Forms.TrackBarTS();
            this.tbRXEQ3 = new System.Windows.Forms.TrackBarTS();
            this.lblRXEQ2 = new System.Windows.Forms.LabelTS();
            this.lblRXEQ3 = new System.Windows.Forms.LabelTS();
            this.lblRXEQPreamp = new System.Windows.Forms.LabelTS();
            this.tbRXEQPreamp = new System.Windows.Forms.TrackBarTS();
            this.lblRXEQ15db = new System.Windows.Forms.LabelTS();
            this.lblRXEQ0dB = new System.Windows.Forms.LabelTS();
            this.lblRXEQminus12db = new System.Windows.Forms.LabelTS();
            this.pnlParaEQ2 = new System.Windows.Forms.PanelTS();
            ((System.ComponentModel.ISupportInitialize)(this.pbParaEQ_live_warning)).BeginInit();
            this.pnlParaEQ.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudParaEQ_preamp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudParaEQ_f)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudParaEQ_q)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudParaEQ_gain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudParaEQ_selected_band)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udParaEQ_high)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udParaEQ_low)).BeginInit();
            this.panelTS1.SuspendLayout();
            this.pnlLegacyEQ.SuspendLayout();
            this.grpTXEQ.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQPre)).BeginInit();
            this.grpRXEQ.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRXEQ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQPreamp)).BeginInit();
            this.pnlParaEQ2.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkPanaEQ_live
            // 
            this.chkPanaEQ_live.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPanaEQ_live.Image = null;
            this.chkPanaEQ_live.Location = new System.Drawing.Point(39, 47);
            this.chkPanaEQ_live.Name = "chkPanaEQ_live";
            this.chkPanaEQ_live.Size = new System.Drawing.Size(84, 17);
            this.chkPanaEQ_live.TabIndex = 165;
            this.chkPanaEQ_live.Text = "Live Update";
            this.toolTip1.SetToolTip(this.chkPanaEQ_live, "Warning: Having live updates may cause slowdown whilst the EQ change is being pro" +
        "cessed.");
            this.chkPanaEQ_live.UseVisualStyleBackColor = true;
            this.chkPanaEQ_live.CheckedChanged += new System.EventHandler(this.chkPanaEQ_live_CheckedChanged);
            // 
            // lblRXEQ9
            // 
            this.lblRXEQ9.Image = null;
            this.lblRXEQ9.Location = new System.Drawing.Point(400, 56);
            this.lblRXEQ9.Name = "lblRXEQ9";
            this.lblRXEQ9.Size = new System.Drawing.Size(40, 16);
            this.lblRXEQ9.TabIndex = 123;
            this.lblRXEQ9.Text = "High";
            this.lblRXEQ9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblRXEQ9, "1500-6000Hz");
            // 
            // lblRXEQ5
            // 
            this.lblRXEQ5.Image = null;
            this.lblRXEQ5.Location = new System.Drawing.Point(240, 56);
            this.lblRXEQ5.Name = "lblRXEQ5";
            this.lblRXEQ5.Size = new System.Drawing.Size(40, 16);
            this.lblRXEQ5.TabIndex = 116;
            this.lblRXEQ5.Text = "Mid";
            this.lblRXEQ5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblRXEQ5, "400-1500Hz");
            // 
            // lblRXEQ1
            // 
            this.lblRXEQ1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRXEQ1.Image = null;
            this.lblRXEQ1.Location = new System.Drawing.Point(80, 56);
            this.lblRXEQ1.Name = "lblRXEQ1";
            this.lblRXEQ1.Size = new System.Drawing.Size(40, 16);
            this.lblRXEQ1.TabIndex = 43;
            this.lblRXEQ1.Text = "Low";
            this.lblRXEQ1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblRXEQ1, "0-400Hz");
            // 
            // pbParaEQ_live_warning
            // 
            this.pbParaEQ_live_warning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbParaEQ_live_warning.Image = ((System.Drawing.Image)(resources.GetObject("pbParaEQ_live_warning.Image")));
            this.pbParaEQ_live_warning.Location = new System.Drawing.Point(13, 44);
            this.pbParaEQ_live_warning.Name = "pbParaEQ_live_warning";
            this.pbParaEQ_live_warning.Size = new System.Drawing.Size(20, 20);
            this.pbParaEQ_live_warning.TabIndex = 171;
            this.pbParaEQ_live_warning.TabStop = false;
            this.toolTip1.SetToolTip(this.pbParaEQ_live_warning, "Live updates may take a while due to large DSP buffer sizes.\r\nUI interactions may" +
        " be degraded. Switching live off will\r\ncause the changes to apply after you fini" +
        "sh dragging/adjusting.");
            this.pbParaEQ_live_warning.Visible = false;
            // 
            // pnlParaEQ
            // 
            this.pnlParaEQ.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.pnlParaEQ.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.pnlParaEQ.Controls.Add(this.labelTS10);
            this.pnlParaEQ.Controls.Add(this.labelTS9);
            this.pnlParaEQ.Controls.Add(this.nudParaEQ_preamp);
            this.pnlParaEQ.Controls.Add(this.labelTS8);
            this.pnlParaEQ.Controls.Add(this.labelTS5);
            this.pnlParaEQ.Controls.Add(this.labelTS7);
            this.pnlParaEQ.Controls.Add(this.btnParaEQReset);
            this.pnlParaEQ.Controls.Add(this.radParaEQ_TX);
            this.pnlParaEQ.Controls.Add(this.chkParaEQ_enabled);
            this.pnlParaEQ.Controls.Add(this.radParaEQ_RX);
            this.pnlParaEQ.Controls.Add(this.nudParaEQ_f);
            this.pnlParaEQ.Controls.Add(this.labelTS6);
            this.pnlParaEQ.Controls.Add(this.nudParaEQ_q);
            this.pnlParaEQ.Controls.Add(this.labelTS4);
            this.pnlParaEQ.Controls.Add(this.nudParaEQ_gain);
            this.pnlParaEQ.Controls.Add(this.labelTS3);
            this.pnlParaEQ.Controls.Add(this.nudParaEQ_selected_band);
            this.pnlParaEQ.Controls.Add(this.ucParametricEq1);
            this.pnlParaEQ.Location = new System.Drawing.Point(569, 35);
            this.pnlParaEQ.Name = "pnlParaEQ";
            this.pnlParaEQ.Size = new System.Drawing.Size(536, 489);
            this.pnlParaEQ.TabIndex = 7;
            // 
            // labelTS10
            // 
            this.labelTS10.AutoSize = true;
            this.labelTS10.Image = null;
            this.labelTS10.Location = new System.Drawing.Point(414, 53);
            this.labelTS10.Name = "labelTS10";
            this.labelTS10.Size = new System.Drawing.Size(20, 13);
            this.labelTS10.TabIndex = 168;
            this.labelTS10.Text = "dB";
            // 
            // labelTS9
            // 
            this.labelTS9.AutoSize = true;
            this.labelTS9.Image = null;
            this.labelTS9.Location = new System.Drawing.Point(310, 53);
            this.labelTS9.Name = "labelTS9";
            this.labelTS9.Size = new System.Drawing.Size(43, 13);
            this.labelTS9.TabIndex = 167;
            this.labelTS9.Text = "Preamp";
            // 
            // nudParaEQ_preamp
            // 
            this.nudParaEQ_preamp.DecimalPlaces = 1;
            this.nudParaEQ_preamp.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudParaEQ_preamp.Location = new System.Drawing.Point(360, 51);
            this.nudParaEQ_preamp.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.nudParaEQ_preamp.Minimum = new decimal(new int[] {
            24,
            0,
            0,
            -2147483648});
            this.nudParaEQ_preamp.Name = "nudParaEQ_preamp";
            this.nudParaEQ_preamp.Size = new System.Drawing.Size(49, 20);
            this.nudParaEQ_preamp.TabIndex = 166;
            this.nudParaEQ_preamp.TinyStep = false;
            this.nudParaEQ_preamp.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudParaEQ_preamp.ValueChanged += new System.EventHandler(this.nudParaEQ_preamp_ValueChanged);
            // 
            // labelTS8
            // 
            this.labelTS8.AutoSize = true;
            this.labelTS8.Image = null;
            this.labelTS8.Location = new System.Drawing.Point(104, 34);
            this.labelTS8.Name = "labelTS8";
            this.labelTS8.Size = new System.Drawing.Size(20, 13);
            this.labelTS8.TabIndex = 164;
            this.labelTS8.Text = "Hz";
            // 
            // labelTS5
            // 
            this.labelTS5.AutoSize = true;
            this.labelTS5.Image = null;
            this.labelTS5.Location = new System.Drawing.Point(194, 34);
            this.labelTS5.Name = "labelTS5";
            this.labelTS5.Size = new System.Drawing.Size(20, 13);
            this.labelTS5.TabIndex = 159;
            this.labelTS5.Text = "dB";
            // 
            // labelTS7
            // 
            this.labelTS7.AutoSize = true;
            this.labelTS7.Image = null;
            this.labelTS7.Location = new System.Drawing.Point(72, 53);
            this.labelTS7.Name = "labelTS7";
            this.labelTS7.Size = new System.Drawing.Size(10, 13);
            this.labelTS7.TabIndex = 163;
            this.labelTS7.Text = "f";
            // 
            // btnParaEQReset
            // 
            this.btnParaEQReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnParaEQReset.Image = null;
            this.btnParaEQReset.Location = new System.Drawing.Point(459, 49);
            this.btnParaEQReset.Name = "btnParaEQReset";
            this.btnParaEQReset.Selectable = true;
            this.btnParaEQReset.Size = new System.Drawing.Size(75, 23);
            this.btnParaEQReset.TabIndex = 10;
            this.btnParaEQReset.Text = "Reset";
            this.btnParaEQReset.UseVisualStyleBackColor = true;
            this.btnParaEQReset.Click += new System.EventHandler(this.btnParaEQReset_Click);
            // 
            // radParaEQ_TX
            // 
            this.radParaEQ_TX.AutoSize = true;
            this.radParaEQ_TX.Checked = true;
            this.radParaEQ_TX.Image = null;
            this.radParaEQ_TX.Location = new System.Drawing.Point(68, 0);
            this.radParaEQ_TX.Name = "radParaEQ_TX";
            this.radParaEQ_TX.Size = new System.Drawing.Size(57, 17);
            this.radParaEQ_TX.TabIndex = 8;
            this.radParaEQ_TX.TabStop = true;
            this.radParaEQ_TX.Text = "TX EQ";
            this.radParaEQ_TX.CheckedChanged += new System.EventHandler(this.radParaEQ_RXTX_CheckedChanged);
            // 
            // chkParaEQ_enabled
            // 
            this.chkParaEQ_enabled.AutoSize = true;
            this.chkParaEQ_enabled.Image = null;
            this.chkParaEQ_enabled.Location = new System.Drawing.Point(28, 23);
            this.chkParaEQ_enabled.Name = "chkParaEQ_enabled";
            this.chkParaEQ_enabled.Size = new System.Drawing.Size(65, 17);
            this.chkParaEQ_enabled.TabIndex = 107;
            this.chkParaEQ_enabled.Text = "Enabled";
            this.chkParaEQ_enabled.CheckedChanged += new System.EventHandler(this.chkParaEQ_enabled_CheckedChanged);
            // 
            // radParaEQ_RX
            // 
            this.radParaEQ_RX.AutoSize = true;
            this.radParaEQ_RX.Image = null;
            this.radParaEQ_RX.Location = new System.Drawing.Point(4, -1);
            this.radParaEQ_RX.Name = "radParaEQ_RX";
            this.radParaEQ_RX.Size = new System.Drawing.Size(58, 17);
            this.radParaEQ_RX.TabIndex = 7;
            this.radParaEQ_RX.Text = "RX EQ";
            this.radParaEQ_RX.CheckedChanged += new System.EventHandler(this.radParaEQ_RXTX_CheckedChanged);
            // 
            // nudParaEQ_f
            // 
            this.nudParaEQ_f.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudParaEQ_f.Location = new System.Drawing.Point(88, 51);
            this.nudParaEQ_f.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.nudParaEQ_f.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudParaEQ_f.Name = "nudParaEQ_f";
            this.nudParaEQ_f.Size = new System.Drawing.Size(50, 20);
            this.nudParaEQ_f.TabIndex = 162;
            this.nudParaEQ_f.TinyStep = false;
            this.nudParaEQ_f.Value = new decimal(new int[] {
            16000,
            0,
            0,
            0});
            this.nudParaEQ_f.ValueChanged += new System.EventHandler(this.nudParaEQ_f_ValueChanged);
            // 
            // labelTS6
            // 
            this.labelTS6.AutoSize = true;
            this.labelTS6.Image = null;
            this.labelTS6.Location = new System.Drawing.Point(234, 54);
            this.labelTS6.Name = "labelTS6";
            this.labelTS6.Size = new System.Drawing.Size(15, 13);
            this.labelTS6.TabIndex = 161;
            this.labelTS6.Text = "Q";
            // 
            // nudParaEQ_q
            // 
            this.nudParaEQ_q.DecimalPlaces = 2;
            this.nudParaEQ_q.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudParaEQ_q.Location = new System.Drawing.Point(255, 51);
            this.nudParaEQ_q.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudParaEQ_q.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.nudParaEQ_q.Name = "nudParaEQ_q";
            this.nudParaEQ_q.Size = new System.Drawing.Size(49, 20);
            this.nudParaEQ_q.TabIndex = 160;
            this.nudParaEQ_q.TinyStep = false;
            this.nudParaEQ_q.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nudParaEQ_q.ValueChanged += new System.EventHandler(this.nudParaEQ_q_ValueChanged);
            // 
            // labelTS4
            // 
            this.labelTS4.AutoSize = true;
            this.labelTS4.Image = null;
            this.labelTS4.Location = new System.Drawing.Point(144, 54);
            this.labelTS4.Name = "labelTS4";
            this.labelTS4.Size = new System.Drawing.Size(29, 13);
            this.labelTS4.TabIndex = 158;
            this.labelTS4.Text = "Gain";
            // 
            // nudParaEQ_gain
            // 
            this.nudParaEQ_gain.DecimalPlaces = 1;
            this.nudParaEQ_gain.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudParaEQ_gain.Location = new System.Drawing.Point(179, 51);
            this.nudParaEQ_gain.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.nudParaEQ_gain.Minimum = new decimal(new int[] {
            24,
            0,
            0,
            -2147483648});
            this.nudParaEQ_gain.Name = "nudParaEQ_gain";
            this.nudParaEQ_gain.Size = new System.Drawing.Size(49, 20);
            this.nudParaEQ_gain.TabIndex = 157;
            this.nudParaEQ_gain.TinyStep = false;
            this.nudParaEQ_gain.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudParaEQ_gain.ValueChanged += new System.EventHandler(this.nudParaEQ_gain_ValueChanged);
            // 
            // labelTS3
            // 
            this.labelTS3.AutoSize = true;
            this.labelTS3.Image = null;
            this.labelTS3.Location = new System.Drawing.Point(8, 54);
            this.labelTS3.Name = "labelTS3";
            this.labelTS3.Size = new System.Drawing.Size(14, 13);
            this.labelTS3.TabIndex = 156;
            this.labelTS3.Text = "#";
            // 
            // nudParaEQ_selected_band
            // 
            this.nudParaEQ_selected_band.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudParaEQ_selected_band.Location = new System.Drawing.Point(28, 51);
            this.nudParaEQ_selected_band.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudParaEQ_selected_band.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudParaEQ_selected_band.Name = "nudParaEQ_selected_band";
            this.nudParaEQ_selected_band.ReadOnly = true;
            this.nudParaEQ_selected_band.Size = new System.Drawing.Size(38, 20);
            this.nudParaEQ_selected_band.TabIndex = 155;
            this.nudParaEQ_selected_band.TinyStep = false;
            this.nudParaEQ_selected_band.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudParaEQ_selected_band.ValueChanged += new System.EventHandler(this.nudParaEQ_selected_band_ValueChanged);
            // 
            // ucParametricEq1
            // 
            this.ucParametricEq1.AllowPointReorder = true;
            this.ucParametricEq1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ucParametricEq1.AxisTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.ucParametricEq1.AxisTickColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.ucParametricEq1.AxisTickLength = 6;
            this.ucParametricEq1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.ucParametricEq1.BandShadeAlpha = 70;
            this.ucParametricEq1.BandShadeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.ucParametricEq1.BandShadeWeightCutoff = 0.002D;
            this.ucParametricEq1.DbMax = 24D;
            this.ucParametricEq1.DbMin = -24D;
            this.ucParametricEq1.ForeColor = System.Drawing.Color.Gainsboro;
            this.ucParametricEq1.FrequencyMaxHz = 4000D;
            this.ucParametricEq1.FrequencyMinHz = 0D;
            this.ucParametricEq1.GlobalGainDb = 0D;
            this.ucParametricEq1.Location = new System.Drawing.Point(3, 75);
            this.ucParametricEq1.MinPointSpacingHz = 5D;
            this.ucParametricEq1.Name = "ucParametricEq1";
            this.ucParametricEq1.ParametricEQ = true;
            this.ucParametricEq1.QMax = 20D;
            this.ucParametricEq1.QMin = 0.2D;
            this.ucParametricEq1.SelectedIndex = -1;
            this.ucParametricEq1.ShowAxisScales = true;
            this.ucParametricEq1.ShowBandShading = true;
            this.ucParametricEq1.ShowReadout = false;
            this.ucParametricEq1.Size = new System.Drawing.Size(530, 411);
            this.ucParametricEq1.TabIndex = 6;
            this.ucParametricEq1.UsePerBandColours = true;
            this.ucParametricEq1.PointsChanged += new System.EventHandler<Thetis.ucParametricEq.EqDraggingEventArgs>(this.ucParametricEq1_PointsChanged);
            this.ucParametricEq1.GlobalGainChanged += new System.EventHandler<Thetis.ucParametricEq.EqDraggingEventArgs>(this.ucParametricEq1_GlobalGainChanged);
            this.ucParametricEq1.PointDataChanged += new System.EventHandler<Thetis.ucParametricEq.EqPointDataChangedEventArgs>(this.ucParametricEq1_PointDataChanged);
            this.ucParametricEq1.PointSelected += new System.EventHandler<Thetis.ucParametricEq.EqPointSelectionChangedEventArgs>(this.ucParametricEq1_PointSelected);
            this.ucParametricEq1.PointUnselected += new System.EventHandler<Thetis.ucParametricEq.EqPointSelectionChangedEventArgs>(this.ucParametricEq1_PointUnselected);
            // 
            // labelTS2
            // 
            this.labelTS2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTS2.AutoSize = true;
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(140, 49);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(29, 13);
            this.labelTS2.TabIndex = 154;
            this.labelTS2.Text = "High";
            // 
            // labelTS1
            // 
            this.labelTS1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTS1.AutoSize = true;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(142, 27);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(27, 13);
            this.labelTS1.TabIndex = 153;
            this.labelTS1.Text = "Low";
            // 
            // udParaEQ_high
            // 
            this.udParaEQ_high.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.udParaEQ_high.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udParaEQ_high.Location = new System.Drawing.Point(175, 47);
            this.udParaEQ_high.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udParaEQ_high.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udParaEQ_high.Name = "udParaEQ_high";
            this.udParaEQ_high.Size = new System.Drawing.Size(50, 20);
            this.udParaEQ_high.TabIndex = 152;
            this.udParaEQ_high.TinyStep = false;
            this.udParaEQ_high.Value = new decimal(new int[] {
            16000,
            0,
            0,
            0});
            this.udParaEQ_high.ValueChanged += new System.EventHandler(this.nudParaEQ_high_ValueChanged);
            // 
            // udParaEQ_low
            // 
            this.udParaEQ_low.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.udParaEQ_low.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udParaEQ_low.Location = new System.Drawing.Point(175, 25);
            this.udParaEQ_low.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udParaEQ_low.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udParaEQ_low.Name = "udParaEQ_low";
            this.udParaEQ_low.Size = new System.Drawing.Size(50, 20);
            this.udParaEQ_low.TabIndex = 151;
            this.udParaEQ_low.TinyStep = false;
            this.udParaEQ_low.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udParaEQ_low.ValueChanged += new System.EventHandler(this.nudParaEQ_low_ValueChanged);
            // 
            // panelTS1
            // 
            this.panelTS1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTS1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.panelTS1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.panelTS1.Controls.Add(this.radParaEQ_5);
            this.panelTS1.Controls.Add(this.radParaEQ_10);
            this.panelTS1.Controls.Add(this.radParaEQ_18);
            this.panelTS1.Location = new System.Drawing.Point(231, 2);
            this.panelTS1.Name = "panelTS1";
            this.panelTS1.Size = new System.Drawing.Size(82, 68);
            this.panelTS1.TabIndex = 13;
            // 
            // radParaEQ_5
            // 
            this.radParaEQ_5.AutoSize = true;
            this.radParaEQ_5.Image = null;
            this.radParaEQ_5.Location = new System.Drawing.Point(15, 3);
            this.radParaEQ_5.Name = "radParaEQ_5";
            this.radParaEQ_5.Size = new System.Drawing.Size(58, 17);
            this.radParaEQ_5.TabIndex = 13;
            this.radParaEQ_5.Text = "5-band";
            this.radParaEQ_5.CheckedChanged += new System.EventHandler(this.radParaEQ_CheckedChanged);
            // 
            // radParaEQ_10
            // 
            this.radParaEQ_10.AutoSize = true;
            this.radParaEQ_10.Checked = true;
            this.radParaEQ_10.Image = null;
            this.radParaEQ_10.Location = new System.Drawing.Point(15, 26);
            this.radParaEQ_10.Name = "radParaEQ_10";
            this.radParaEQ_10.Size = new System.Drawing.Size(64, 17);
            this.radParaEQ_10.TabIndex = 11;
            this.radParaEQ_10.TabStop = true;
            this.radParaEQ_10.Text = "10-band";
            this.radParaEQ_10.CheckedChanged += new System.EventHandler(this.radParaEQ_CheckedChanged);
            // 
            // radParaEQ_18
            // 
            this.radParaEQ_18.AutoSize = true;
            this.radParaEQ_18.Image = null;
            this.radParaEQ_18.Location = new System.Drawing.Point(15, 49);
            this.radParaEQ_18.Name = "radParaEQ_18";
            this.radParaEQ_18.Size = new System.Drawing.Size(64, 17);
            this.radParaEQ_18.TabIndex = 12;
            this.radParaEQ_18.Text = "18-band";
            this.radParaEQ_18.CheckedChanged += new System.EventHandler(this.radParaEQ_CheckedChanged);
            // 
            // chkUseQFactors
            // 
            this.chkUseQFactors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUseQFactors.Checked = true;
            this.chkUseQFactors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseQFactors.Image = null;
            this.chkUseQFactors.Location = new System.Drawing.Point(39, 28);
            this.chkUseQFactors.Name = "chkUseQFactors";
            this.chkUseQFactors.Size = new System.Drawing.Size(94, 17);
            this.chkUseQFactors.TabIndex = 9;
            this.chkUseQFactors.Text = "Use Q Factors";
            this.chkUseQFactors.UseVisualStyleBackColor = true;
            this.chkUseQFactors.CheckedChanged += new System.EventHandler(this.chkUseQFactors_CheckedChanged);
            // 
            // chkLegacyEQ
            // 
            this.chkLegacyEQ.AutoSize = true;
            this.chkLegacyEQ.Checked = true;
            this.chkLegacyEQ.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLegacyEQ.Image = null;
            this.chkLegacyEQ.Location = new System.Drawing.Point(12, 12);
            this.chkLegacyEQ.Name = "chkLegacyEQ";
            this.chkLegacyEQ.Size = new System.Drawing.Size(79, 17);
            this.chkLegacyEQ.TabIndex = 5;
            this.chkLegacyEQ.Text = "Legacy EQ";
            this.chkLegacyEQ.UseVisualStyleBackColor = true;
            this.chkLegacyEQ.CheckedChanged += new System.EventHandler(this.chkLegacyEQ_CheckedChanged);
            // 
            // pnlLegacyEQ
            // 
            this.pnlLegacyEQ.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.pnlLegacyEQ.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.pnlLegacyEQ.Controls.Add(this.rad3Band);
            this.pnlLegacyEQ.Controls.Add(this.rad10Band);
            this.pnlLegacyEQ.Controls.Add(this.grpTXEQ);
            this.pnlLegacyEQ.Controls.Add(this.grpRXEQ);
            this.pnlLegacyEQ.Location = new System.Drawing.Point(12, 35);
            this.pnlLegacyEQ.Name = "pnlLegacyEQ";
            this.pnlLegacyEQ.Size = new System.Drawing.Size(536, 489);
            this.pnlLegacyEQ.TabIndex = 4;
            // 
            // rad3Band
            // 
            this.rad3Band.Image = null;
            this.rad3Band.Location = new System.Drawing.Point(12, 3);
            this.rad3Band.Name = "rad3Band";
            this.rad3Band.Size = new System.Drawing.Size(120, 24);
            this.rad3Band.TabIndex = 2;
            this.rad3Band.Text = "3-Band Equalizer";
            this.rad3Band.CheckedChanged += new System.EventHandler(this.rad3Band_CheckedChanged);
            // 
            // rad10Band
            // 
            this.rad10Band.Checked = true;
            this.rad10Band.Image = null;
            this.rad10Band.Location = new System.Drawing.Point(140, 3);
            this.rad10Band.Name = "rad10Band";
            this.rad10Band.Size = new System.Drawing.Size(120, 24);
            this.rad10Band.TabIndex = 3;
            this.rad10Band.TabStop = true;
            this.rad10Band.Text = "10-Band Equalizer";
            this.rad10Band.CheckedChanged += new System.EventHandler(this.rad10Band_CheckedChanged);
            // 
            // grpTXEQ
            // 
            this.grpTXEQ.Controls.Add(this.lblCFCFreq);
            this.grpTXEQ.Controls.Add(this.udTXEQ9);
            this.grpTXEQ.Controls.Add(this.udTXEQ8);
            this.grpTXEQ.Controls.Add(this.udTXEQ7);
            this.grpTXEQ.Controls.Add(this.udTXEQ6);
            this.grpTXEQ.Controls.Add(this.udTXEQ5);
            this.grpTXEQ.Controls.Add(this.udTXEQ4);
            this.grpTXEQ.Controls.Add(this.udTXEQ3);
            this.grpTXEQ.Controls.Add(this.udTXEQ2);
            this.grpTXEQ.Controls.Add(this.udTXEQ1);
            this.grpTXEQ.Controls.Add(this.udTXEQ0);
            this.grpTXEQ.Controls.Add(this.lblTXEQ15db2);
            this.grpTXEQ.Controls.Add(this.lblTXEQ0dB2);
            this.grpTXEQ.Controls.Add(this.lblTXEQminus12db2);
            this.grpTXEQ.Controls.Add(this.tbTXEQ9);
            this.grpTXEQ.Controls.Add(this.tbTXEQ6);
            this.grpTXEQ.Controls.Add(this.tbTXEQ7);
            this.grpTXEQ.Controls.Add(this.tbTXEQ8);
            this.grpTXEQ.Controls.Add(this.tbTXEQ3);
            this.grpTXEQ.Controls.Add(this.tbTXEQ4);
            this.grpTXEQ.Controls.Add(this.tbTXEQ5);
            this.grpTXEQ.Controls.Add(this.chkTXEQEnabled);
            this.grpTXEQ.Controls.Add(this.tbTXEQ0);
            this.grpTXEQ.Controls.Add(this.tbTXEQ1);
            this.grpTXEQ.Controls.Add(this.tbTXEQ2);
            this.grpTXEQ.Controls.Add(this.lblTXEQPreamp);
            this.grpTXEQ.Controls.Add(this.tbTXEQPre);
            this.grpTXEQ.Controls.Add(this.lblTXEQ15db);
            this.grpTXEQ.Controls.Add(this.lblTXEQ0dB);
            this.grpTXEQ.Controls.Add(this.lblTXEQminus12db);
            this.grpTXEQ.Location = new System.Drawing.Point(4, 267);
            this.grpTXEQ.Name = "grpTXEQ";
            this.grpTXEQ.Size = new System.Drawing.Size(528, 218);
            this.grpTXEQ.TabIndex = 1;
            this.grpTXEQ.TabStop = false;
            this.grpTXEQ.Text = "Transmit Equalizer";
            // 
            // lblCFCFreq
            // 
            this.lblCFCFreq.AutoSize = true;
            this.lblCFCFreq.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCFCFreq.Image = null;
            this.lblCFCFreq.Location = new System.Drawing.Point(38, 50);
            this.lblCFCFreq.Name = "lblCFCFreq";
            this.lblCFCFreq.Size = new System.Drawing.Size(35, 13);
            this.lblCFCFreq.TabIndex = 159;
            this.lblCFCFreq.Text = "FREQ";
            // 
            // udTXEQ9
            // 
            this.udTXEQ9.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTXEQ9.Location = new System.Drawing.Point(438, 37);
            this.udTXEQ9.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udTXEQ9.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udTXEQ9.Name = "udTXEQ9";
            this.udTXEQ9.Size = new System.Drawing.Size(50, 20);
            this.udTXEQ9.TabIndex = 158;
            this.udTXEQ9.TinyStep = false;
            this.udTXEQ9.Value = new decimal(new int[] {
            16000,
            0,
            0,
            0});
            this.udTXEQ9.ValueChanged += new System.EventHandler(this.setTXEQProfile);
            // 
            // udTXEQ8
            // 
            this.udTXEQ8.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTXEQ8.Location = new System.Drawing.Point(398, 60);
            this.udTXEQ8.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udTXEQ8.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udTXEQ8.Name = "udTXEQ8";
            this.udTXEQ8.Size = new System.Drawing.Size(50, 20);
            this.udTXEQ8.TabIndex = 157;
            this.udTXEQ8.TinyStep = false;
            this.udTXEQ8.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            this.udTXEQ8.ValueChanged += new System.EventHandler(this.setTXEQProfile);
            // 
            // udTXEQ7
            // 
            this.udTXEQ7.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTXEQ7.Location = new System.Drawing.Point(358, 37);
            this.udTXEQ7.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udTXEQ7.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udTXEQ7.Name = "udTXEQ7";
            this.udTXEQ7.Size = new System.Drawing.Size(50, 20);
            this.udTXEQ7.TabIndex = 156;
            this.udTXEQ7.TinyStep = false;
            this.udTXEQ7.Value = new decimal(new int[] {
            4000,
            0,
            0,
            0});
            this.udTXEQ7.ValueChanged += new System.EventHandler(this.setTXEQProfile);
            // 
            // udTXEQ6
            // 
            this.udTXEQ6.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTXEQ6.Location = new System.Drawing.Point(318, 60);
            this.udTXEQ6.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udTXEQ6.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udTXEQ6.Name = "udTXEQ6";
            this.udTXEQ6.Size = new System.Drawing.Size(50, 20);
            this.udTXEQ6.TabIndex = 155;
            this.udTXEQ6.TinyStep = false;
            this.udTXEQ6.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.udTXEQ6.ValueChanged += new System.EventHandler(this.setTXEQProfile);
            // 
            // udTXEQ5
            // 
            this.udTXEQ5.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTXEQ5.Location = new System.Drawing.Point(278, 37);
            this.udTXEQ5.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udTXEQ5.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udTXEQ5.Name = "udTXEQ5";
            this.udTXEQ5.Size = new System.Drawing.Size(50, 20);
            this.udTXEQ5.TabIndex = 154;
            this.udTXEQ5.TinyStep = false;
            this.udTXEQ5.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.udTXEQ5.ValueChanged += new System.EventHandler(this.setTXEQProfile);
            // 
            // udTXEQ4
            // 
            this.udTXEQ4.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTXEQ4.Location = new System.Drawing.Point(238, 60);
            this.udTXEQ4.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udTXEQ4.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udTXEQ4.Name = "udTXEQ4";
            this.udTXEQ4.Size = new System.Drawing.Size(50, 20);
            this.udTXEQ4.TabIndex = 153;
            this.udTXEQ4.TinyStep = false;
            this.udTXEQ4.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.udTXEQ4.ValueChanged += new System.EventHandler(this.setTXEQProfile);
            // 
            // udTXEQ3
            // 
            this.udTXEQ3.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTXEQ3.Location = new System.Drawing.Point(198, 37);
            this.udTXEQ3.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udTXEQ3.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udTXEQ3.Name = "udTXEQ3";
            this.udTXEQ3.Size = new System.Drawing.Size(50, 20);
            this.udTXEQ3.TabIndex = 152;
            this.udTXEQ3.TinyStep = false;
            this.udTXEQ3.Value = new decimal(new int[] {
            250,
            0,
            0,
            0});
            this.udTXEQ3.ValueChanged += new System.EventHandler(this.setTXEQProfile);
            // 
            // udTXEQ2
            // 
            this.udTXEQ2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTXEQ2.Location = new System.Drawing.Point(158, 60);
            this.udTXEQ2.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udTXEQ2.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udTXEQ2.Name = "udTXEQ2";
            this.udTXEQ2.Size = new System.Drawing.Size(50, 20);
            this.udTXEQ2.TabIndex = 151;
            this.udTXEQ2.TinyStep = false;
            this.udTXEQ2.Value = new decimal(new int[] {
            125,
            0,
            0,
            0});
            this.udTXEQ2.ValueChanged += new System.EventHandler(this.setTXEQProfile);
            // 
            // udTXEQ1
            // 
            this.udTXEQ1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTXEQ1.Location = new System.Drawing.Point(118, 37);
            this.udTXEQ1.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udTXEQ1.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udTXEQ1.Name = "udTXEQ1";
            this.udTXEQ1.Size = new System.Drawing.Size(50, 20);
            this.udTXEQ1.TabIndex = 150;
            this.udTXEQ1.TinyStep = false;
            this.udTXEQ1.Value = new decimal(new int[] {
            63,
            0,
            0,
            0});
            this.udTXEQ1.ValueChanged += new System.EventHandler(this.setTXEQProfile);
            // 
            // udTXEQ0
            // 
            this.udTXEQ0.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTXEQ0.Location = new System.Drawing.Point(78, 60);
            this.udTXEQ0.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udTXEQ0.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udTXEQ0.Name = "udTXEQ0";
            this.udTXEQ0.Size = new System.Drawing.Size(50, 20);
            this.udTXEQ0.TabIndex = 149;
            this.udTXEQ0.TinyStep = false;
            this.udTXEQ0.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.udTXEQ0.ValueChanged += new System.EventHandler(this.setTXEQProfile);
            // 
            // lblTXEQ15db2
            // 
            this.lblTXEQ15db2.Image = null;
            this.lblTXEQ15db2.Location = new System.Drawing.Point(483, 87);
            this.lblTXEQ15db2.Name = "lblTXEQ15db2";
            this.lblTXEQ15db2.Size = new System.Drawing.Size(32, 16);
            this.lblTXEQ15db2.TabIndex = 129;
            this.lblTXEQ15db2.Text = "15dB";
            this.lblTXEQ15db2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTXEQ0dB2
            // 
            this.lblTXEQ0dB2.Image = null;
            this.lblTXEQ0dB2.Location = new System.Drawing.Point(483, 143);
            this.lblTXEQ0dB2.Name = "lblTXEQ0dB2";
            this.lblTXEQ0dB2.Size = new System.Drawing.Size(32, 16);
            this.lblTXEQ0dB2.TabIndex = 128;
            this.lblTXEQ0dB2.Text = "  0dB";
            this.lblTXEQ0dB2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTXEQminus12db2
            // 
            this.lblTXEQminus12db2.Image = null;
            this.lblTXEQminus12db2.Location = new System.Drawing.Point(480, 187);
            this.lblTXEQminus12db2.Name = "lblTXEQminus12db2";
            this.lblTXEQminus12db2.Size = new System.Drawing.Size(38, 16);
            this.lblTXEQminus12db2.TabIndex = 130;
            this.lblTXEQminus12db2.Text = "-12dB";
            this.lblTXEQminus12db2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbTXEQ9
            // 
            this.tbTXEQ9.AutoSize = false;
            this.tbTXEQ9.LargeChange = 3;
            this.tbTXEQ9.Location = new System.Drawing.Point(448, 81);
            this.tbTXEQ9.Maximum = 15;
            this.tbTXEQ9.Minimum = -12;
            this.tbTXEQ9.Name = "tbTXEQ9";
            this.tbTXEQ9.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQ9.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQ9.TabIndex = 126;
            this.tbTXEQ9.TickFrequency = 3;
            this.tbTXEQ9.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // tbTXEQ6
            // 
            this.tbTXEQ6.AutoSize = false;
            this.tbTXEQ6.LargeChange = 3;
            this.tbTXEQ6.Location = new System.Drawing.Point(328, 81);
            this.tbTXEQ6.Maximum = 15;
            this.tbTXEQ6.Minimum = -12;
            this.tbTXEQ6.Name = "tbTXEQ6";
            this.tbTXEQ6.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQ6.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQ6.TabIndex = 120;
            this.tbTXEQ6.TickFrequency = 3;
            this.tbTXEQ6.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // tbTXEQ7
            // 
            this.tbTXEQ7.AutoSize = false;
            this.tbTXEQ7.LargeChange = 3;
            this.tbTXEQ7.Location = new System.Drawing.Point(368, 81);
            this.tbTXEQ7.Maximum = 15;
            this.tbTXEQ7.Minimum = -12;
            this.tbTXEQ7.Name = "tbTXEQ7";
            this.tbTXEQ7.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQ7.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQ7.TabIndex = 121;
            this.tbTXEQ7.TickFrequency = 3;
            this.tbTXEQ7.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // tbTXEQ8
            // 
            this.tbTXEQ8.AutoSize = false;
            this.tbTXEQ8.LargeChange = 3;
            this.tbTXEQ8.Location = new System.Drawing.Point(408, 81);
            this.tbTXEQ8.Maximum = 15;
            this.tbTXEQ8.Minimum = -12;
            this.tbTXEQ8.Name = "tbTXEQ8";
            this.tbTXEQ8.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQ8.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQ8.TabIndex = 122;
            this.tbTXEQ8.TickFrequency = 3;
            this.tbTXEQ8.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // tbTXEQ3
            // 
            this.tbTXEQ3.AutoSize = false;
            this.tbTXEQ3.LargeChange = 3;
            this.tbTXEQ3.Location = new System.Drawing.Point(208, 81);
            this.tbTXEQ3.Maximum = 15;
            this.tbTXEQ3.Minimum = -12;
            this.tbTXEQ3.Name = "tbTXEQ3";
            this.tbTXEQ3.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQ3.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQ3.TabIndex = 114;
            this.tbTXEQ3.TickFrequency = 3;
            this.tbTXEQ3.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // tbTXEQ4
            // 
            this.tbTXEQ4.AutoSize = false;
            this.tbTXEQ4.LargeChange = 3;
            this.tbTXEQ4.Location = new System.Drawing.Point(248, 81);
            this.tbTXEQ4.Maximum = 15;
            this.tbTXEQ4.Minimum = -12;
            this.tbTXEQ4.Name = "tbTXEQ4";
            this.tbTXEQ4.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQ4.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQ4.TabIndex = 115;
            this.tbTXEQ4.TickFrequency = 3;
            this.tbTXEQ4.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // tbTXEQ5
            // 
            this.tbTXEQ5.AutoSize = false;
            this.tbTXEQ5.LargeChange = 3;
            this.tbTXEQ5.Location = new System.Drawing.Point(288, 81);
            this.tbTXEQ5.Maximum = 15;
            this.tbTXEQ5.Minimum = -12;
            this.tbTXEQ5.Name = "tbTXEQ5";
            this.tbTXEQ5.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQ5.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQ5.TabIndex = 116;
            this.tbTXEQ5.TickFrequency = 3;
            this.tbTXEQ5.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // chkTXEQEnabled
            // 
            this.chkTXEQEnabled.Image = null;
            this.chkTXEQEnabled.Location = new System.Drawing.Point(16, 18);
            this.chkTXEQEnabled.Name = "chkTXEQEnabled";
            this.chkTXEQEnabled.Size = new System.Drawing.Size(72, 16);
            this.chkTXEQEnabled.TabIndex = 106;
            this.chkTXEQEnabled.Text = "Enabled";
            this.chkTXEQEnabled.CheckedChanged += new System.EventHandler(this.chkTXEQEnabled_CheckedChanged);
            // 
            // tbTXEQ0
            // 
            this.tbTXEQ0.AutoSize = false;
            this.tbTXEQ0.LargeChange = 3;
            this.tbTXEQ0.Location = new System.Drawing.Point(88, 81);
            this.tbTXEQ0.Maximum = 15;
            this.tbTXEQ0.Minimum = -12;
            this.tbTXEQ0.Name = "tbTXEQ0";
            this.tbTXEQ0.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQ0.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQ0.TabIndex = 4;
            this.tbTXEQ0.TickFrequency = 3;
            this.tbTXEQ0.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // tbTXEQ1
            // 
            this.tbTXEQ1.AutoSize = false;
            this.tbTXEQ1.LargeChange = 3;
            this.tbTXEQ1.Location = new System.Drawing.Point(128, 81);
            this.tbTXEQ1.Maximum = 15;
            this.tbTXEQ1.Minimum = -12;
            this.tbTXEQ1.Name = "tbTXEQ1";
            this.tbTXEQ1.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQ1.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQ1.TabIndex = 5;
            this.tbTXEQ1.TickFrequency = 3;
            this.tbTXEQ1.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // tbTXEQ2
            // 
            this.tbTXEQ2.AutoSize = false;
            this.tbTXEQ2.LargeChange = 3;
            this.tbTXEQ2.Location = new System.Drawing.Point(168, 81);
            this.tbTXEQ2.Maximum = 15;
            this.tbTXEQ2.Minimum = -12;
            this.tbTXEQ2.Name = "tbTXEQ2";
            this.tbTXEQ2.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQ2.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQ2.TabIndex = 6;
            this.tbTXEQ2.TickFrequency = 3;
            this.tbTXEQ2.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // lblTXEQPreamp
            // 
            this.lblTXEQPreamp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTXEQPreamp.Image = null;
            this.lblTXEQPreamp.Location = new System.Drawing.Point(8, 69);
            this.lblTXEQPreamp.Name = "lblTXEQPreamp";
            this.lblTXEQPreamp.Size = new System.Drawing.Size(48, 16);
            this.lblTXEQPreamp.TabIndex = 105;
            this.lblTXEQPreamp.Text = "Preamp";
            this.lblTXEQPreamp.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tbTXEQPre
            // 
            this.tbTXEQPre.AutoSize = false;
            this.tbTXEQPre.LargeChange = 3;
            this.tbTXEQPre.Location = new System.Drawing.Point(16, 81);
            this.tbTXEQPre.Maximum = 15;
            this.tbTXEQPre.Minimum = -12;
            this.tbTXEQPre.Name = "tbTXEQPre";
            this.tbTXEQPre.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTXEQPre.Size = new System.Drawing.Size(32, 128);
            this.tbTXEQPre.TabIndex = 36;
            this.tbTXEQPre.TickFrequency = 3;
            this.tbTXEQPre.Scroll += new System.EventHandler(this.setTXEQProfile);
            // 
            // lblTXEQ15db
            // 
            this.lblTXEQ15db.Image = null;
            this.lblTXEQ15db.Location = new System.Drawing.Point(56, 89);
            this.lblTXEQ15db.Name = "lblTXEQ15db";
            this.lblTXEQ15db.Size = new System.Drawing.Size(32, 16);
            this.lblTXEQ15db.TabIndex = 43;
            this.lblTXEQ15db.Text = "15dB";
            this.lblTXEQ15db.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTXEQ0dB
            // 
            this.lblTXEQ0dB.Image = null;
            this.lblTXEQ0dB.Location = new System.Drawing.Point(56, 143);
            this.lblTXEQ0dB.Name = "lblTXEQ0dB";
            this.lblTXEQ0dB.Size = new System.Drawing.Size(32, 16);
            this.lblTXEQ0dB.TabIndex = 0;
            this.lblTXEQ0dB.Text = "  0dB";
            this.lblTXEQ0dB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTXEQminus12db
            // 
            this.lblTXEQminus12db.Image = null;
            this.lblTXEQminus12db.Location = new System.Drawing.Point(52, 185);
            this.lblTXEQminus12db.Name = "lblTXEQminus12db";
            this.lblTXEQminus12db.Size = new System.Drawing.Size(38, 16);
            this.lblTXEQminus12db.TabIndex = 45;
            this.lblTXEQminus12db.Text = "-12dB";
            this.lblTXEQminus12db.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpRXEQ
            // 
            this.grpRXEQ.Controls.Add(this.lblRXEQ15db2);
            this.grpRXEQ.Controls.Add(this.lblRXEQ0dB2);
            this.grpRXEQ.Controls.Add(this.lblRXEQminus12db2);
            this.grpRXEQ.Controls.Add(this.lblRXEQ10);
            this.grpRXEQ.Controls.Add(this.tbRXEQ10);
            this.grpRXEQ.Controls.Add(this.lblRXEQ7);
            this.grpRXEQ.Controls.Add(this.lblRXEQ8);
            this.grpRXEQ.Controls.Add(this.lblRXEQ9);
            this.grpRXEQ.Controls.Add(this.tbRXEQ7);
            this.grpRXEQ.Controls.Add(this.tbRXEQ8);
            this.grpRXEQ.Controls.Add(this.tbRXEQ9);
            this.grpRXEQ.Controls.Add(this.tbRXEQ4);
            this.grpRXEQ.Controls.Add(this.tbRXEQ5);
            this.grpRXEQ.Controls.Add(this.tbRXEQ6);
            this.grpRXEQ.Controls.Add(this.lblRXEQ4);
            this.grpRXEQ.Controls.Add(this.lblRXEQ5);
            this.grpRXEQ.Controls.Add(this.lblRXEQ6);
            this.grpRXEQ.Controls.Add(this.picRXEQ);
            this.grpRXEQ.Controls.Add(this.btnRXEQReset);
            this.grpRXEQ.Controls.Add(this.chkRXEQEnabled);
            this.grpRXEQ.Controls.Add(this.tbRXEQ1);
            this.grpRXEQ.Controls.Add(this.tbRXEQ2);
            this.grpRXEQ.Controls.Add(this.tbRXEQ3);
            this.grpRXEQ.Controls.Add(this.lblRXEQ1);
            this.grpRXEQ.Controls.Add(this.lblRXEQ2);
            this.grpRXEQ.Controls.Add(this.lblRXEQ3);
            this.grpRXEQ.Controls.Add(this.lblRXEQPreamp);
            this.grpRXEQ.Controls.Add(this.tbRXEQPreamp);
            this.grpRXEQ.Controls.Add(this.lblRXEQ15db);
            this.grpRXEQ.Controls.Add(this.lblRXEQ0dB);
            this.grpRXEQ.Controls.Add(this.lblRXEQminus12db);
            this.grpRXEQ.Location = new System.Drawing.Point(4, 35);
            this.grpRXEQ.Name = "grpRXEQ";
            this.grpRXEQ.Size = new System.Drawing.Size(528, 224);
            this.grpRXEQ.TabIndex = 1;
            this.grpRXEQ.TabStop = false;
            this.grpRXEQ.Text = "Receive Equalizer";
            // 
            // lblRXEQ15db2
            // 
            this.lblRXEQ15db2.Image = null;
            this.lblRXEQ15db2.Location = new System.Drawing.Point(483, 78);
            this.lblRXEQ15db2.Name = "lblRXEQ15db2";
            this.lblRXEQ15db2.Size = new System.Drawing.Size(32, 16);
            this.lblRXEQ15db2.TabIndex = 126;
            this.lblRXEQ15db2.Text = "15dB";
            this.lblRXEQ15db2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRXEQ0dB2
            // 
            this.lblRXEQ0dB2.Image = null;
            this.lblRXEQ0dB2.Location = new System.Drawing.Point(483, 134);
            this.lblRXEQ0dB2.Name = "lblRXEQ0dB2";
            this.lblRXEQ0dB2.Size = new System.Drawing.Size(32, 16);
            this.lblRXEQ0dB2.TabIndex = 127;
            this.lblRXEQ0dB2.Text = "  0dB";
            this.lblRXEQ0dB2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRXEQminus12db2
            // 
            this.lblRXEQminus12db2.Image = null;
            this.lblRXEQminus12db2.Location = new System.Drawing.Point(480, 178);
            this.lblRXEQminus12db2.Name = "lblRXEQminus12db2";
            this.lblRXEQminus12db2.Size = new System.Drawing.Size(38, 16);
            this.lblRXEQminus12db2.TabIndex = 128;
            this.lblRXEQminus12db2.Text = "-12dB";
            this.lblRXEQminus12db2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRXEQ10
            // 
            this.lblRXEQ10.Image = null;
            this.lblRXEQ10.Location = new System.Drawing.Point(440, 56);
            this.lblRXEQ10.Name = "lblRXEQ10";
            this.lblRXEQ10.Size = new System.Drawing.Size(40, 16);
            this.lblRXEQ10.TabIndex = 125;
            this.lblRXEQ10.Text = "16K";
            this.lblRXEQ10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRXEQ10.Visible = false;
            // 
            // tbRXEQ10
            // 
            this.tbRXEQ10.AutoSize = false;
            this.tbRXEQ10.LargeChange = 3;
            this.tbRXEQ10.Location = new System.Drawing.Point(448, 72);
            this.tbRXEQ10.Maximum = 15;
            this.tbRXEQ10.Minimum = -12;
            this.tbRXEQ10.Name = "tbRXEQ10";
            this.tbRXEQ10.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQ10.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQ10.TabIndex = 124;
            this.tbRXEQ10.TickFrequency = 3;
            this.tbRXEQ10.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // lblRXEQ7
            // 
            this.lblRXEQ7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRXEQ7.Image = null;
            this.lblRXEQ7.Location = new System.Drawing.Point(320, 56);
            this.lblRXEQ7.Name = "lblRXEQ7";
            this.lblRXEQ7.Size = new System.Drawing.Size(40, 16);
            this.lblRXEQ7.TabIndex = 121;
            this.lblRXEQ7.Text = "2K";
            this.lblRXEQ7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRXEQ7.Visible = false;
            // 
            // lblRXEQ8
            // 
            this.lblRXEQ8.Image = null;
            this.lblRXEQ8.Location = new System.Drawing.Point(360, 56);
            this.lblRXEQ8.Name = "lblRXEQ8";
            this.lblRXEQ8.Size = new System.Drawing.Size(40, 16);
            this.lblRXEQ8.TabIndex = 122;
            this.lblRXEQ8.Text = "4K";
            this.lblRXEQ8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRXEQ8.Visible = false;
            // 
            // tbRXEQ7
            // 
            this.tbRXEQ7.AutoSize = false;
            this.tbRXEQ7.LargeChange = 3;
            this.tbRXEQ7.Location = new System.Drawing.Point(328, 72);
            this.tbRXEQ7.Maximum = 15;
            this.tbRXEQ7.Minimum = -12;
            this.tbRXEQ7.Name = "tbRXEQ7";
            this.tbRXEQ7.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQ7.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQ7.TabIndex = 118;
            this.tbRXEQ7.TickFrequency = 3;
            this.tbRXEQ7.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // tbRXEQ8
            // 
            this.tbRXEQ8.AutoSize = false;
            this.tbRXEQ8.LargeChange = 3;
            this.tbRXEQ8.Location = new System.Drawing.Point(368, 72);
            this.tbRXEQ8.Maximum = 15;
            this.tbRXEQ8.Minimum = -12;
            this.tbRXEQ8.Name = "tbRXEQ8";
            this.tbRXEQ8.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQ8.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQ8.TabIndex = 119;
            this.tbRXEQ8.TickFrequency = 3;
            this.tbRXEQ8.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // tbRXEQ9
            // 
            this.tbRXEQ9.AutoSize = false;
            this.tbRXEQ9.LargeChange = 3;
            this.tbRXEQ9.Location = new System.Drawing.Point(408, 72);
            this.tbRXEQ9.Maximum = 15;
            this.tbRXEQ9.Minimum = -12;
            this.tbRXEQ9.Name = "tbRXEQ9";
            this.tbRXEQ9.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQ9.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQ9.TabIndex = 120;
            this.tbRXEQ9.TickFrequency = 3;
            this.tbRXEQ9.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // tbRXEQ4
            // 
            this.tbRXEQ4.AutoSize = false;
            this.tbRXEQ4.LargeChange = 3;
            this.tbRXEQ4.Location = new System.Drawing.Point(208, 72);
            this.tbRXEQ4.Maximum = 15;
            this.tbRXEQ4.Minimum = -12;
            this.tbRXEQ4.Name = "tbRXEQ4";
            this.tbRXEQ4.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQ4.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQ4.TabIndex = 112;
            this.tbRXEQ4.TickFrequency = 3;
            this.tbRXEQ4.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // tbRXEQ5
            // 
            this.tbRXEQ5.AutoSize = false;
            this.tbRXEQ5.LargeChange = 3;
            this.tbRXEQ5.Location = new System.Drawing.Point(248, 72);
            this.tbRXEQ5.Maximum = 15;
            this.tbRXEQ5.Minimum = -12;
            this.tbRXEQ5.Name = "tbRXEQ5";
            this.tbRXEQ5.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQ5.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQ5.TabIndex = 113;
            this.tbRXEQ5.TickFrequency = 3;
            this.tbRXEQ5.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // tbRXEQ6
            // 
            this.tbRXEQ6.AutoSize = false;
            this.tbRXEQ6.LargeChange = 3;
            this.tbRXEQ6.Location = new System.Drawing.Point(288, 72);
            this.tbRXEQ6.Maximum = 15;
            this.tbRXEQ6.Minimum = -12;
            this.tbRXEQ6.Name = "tbRXEQ6";
            this.tbRXEQ6.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQ6.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQ6.TabIndex = 114;
            this.tbRXEQ6.TickFrequency = 3;
            this.tbRXEQ6.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // lblRXEQ4
            // 
            this.lblRXEQ4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRXEQ4.Image = null;
            this.lblRXEQ4.Location = new System.Drawing.Point(200, 56);
            this.lblRXEQ4.Name = "lblRXEQ4";
            this.lblRXEQ4.Size = new System.Drawing.Size(40, 16);
            this.lblRXEQ4.TabIndex = 115;
            this.lblRXEQ4.Text = "250";
            this.lblRXEQ4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRXEQ4.Visible = false;
            // 
            // lblRXEQ6
            // 
            this.lblRXEQ6.Image = null;
            this.lblRXEQ6.Location = new System.Drawing.Point(280, 56);
            this.lblRXEQ6.Name = "lblRXEQ6";
            this.lblRXEQ6.Size = new System.Drawing.Size(40, 16);
            this.lblRXEQ6.TabIndex = 117;
            this.lblRXEQ6.Text = "1K";
            this.lblRXEQ6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRXEQ6.Visible = false;
            // 
            // picRXEQ
            // 
            this.picRXEQ.BackColor = System.Drawing.Color.Black;
            this.picRXEQ.Location = new System.Drawing.Point(88, 24);
            this.picRXEQ.Name = "picRXEQ";
            this.picRXEQ.Size = new System.Drawing.Size(384, 24);
            this.picRXEQ.TabIndex = 111;
            this.picRXEQ.TabStop = false;
            this.picRXEQ.Paint += new System.Windows.Forms.PaintEventHandler(this.picRXEQ_Paint);
            // 
            // btnRXEQReset
            // 
            this.btnRXEQReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRXEQReset.Image = null;
            this.btnRXEQReset.Location = new System.Drawing.Point(478, 28);
            this.btnRXEQReset.Name = "btnRXEQReset";
            this.btnRXEQReset.Selectable = true;
            this.btnRXEQReset.Size = new System.Drawing.Size(42, 20);
            this.btnRXEQReset.TabIndex = 110;
            this.btnRXEQReset.Text = "Reset";
            this.btnRXEQReset.Click += new System.EventHandler(this.btnRXEQReset_Click);
            // 
            // chkRXEQEnabled
            // 
            this.chkRXEQEnabled.Image = null;
            this.chkRXEQEnabled.Location = new System.Drawing.Point(16, 24);
            this.chkRXEQEnabled.Name = "chkRXEQEnabled";
            this.chkRXEQEnabled.Size = new System.Drawing.Size(72, 16);
            this.chkRXEQEnabled.TabIndex = 109;
            this.chkRXEQEnabled.Text = "Enabled";
            this.chkRXEQEnabled.CheckedChanged += new System.EventHandler(this.chkRXEQEnabled_CheckedChanged);
            // 
            // tbRXEQ1
            // 
            this.tbRXEQ1.AutoSize = false;
            this.tbRXEQ1.LargeChange = 3;
            this.tbRXEQ1.Location = new System.Drawing.Point(88, 72);
            this.tbRXEQ1.Maximum = 15;
            this.tbRXEQ1.Minimum = -12;
            this.tbRXEQ1.Name = "tbRXEQ1";
            this.tbRXEQ1.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQ1.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQ1.TabIndex = 4;
            this.tbRXEQ1.TickFrequency = 3;
            this.tbRXEQ1.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // tbRXEQ2
            // 
            this.tbRXEQ2.AutoSize = false;
            this.tbRXEQ2.LargeChange = 3;
            this.tbRXEQ2.Location = new System.Drawing.Point(128, 72);
            this.tbRXEQ2.Maximum = 15;
            this.tbRXEQ2.Minimum = -12;
            this.tbRXEQ2.Name = "tbRXEQ2";
            this.tbRXEQ2.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQ2.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQ2.TabIndex = 5;
            this.tbRXEQ2.TickFrequency = 3;
            this.tbRXEQ2.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // tbRXEQ3
            // 
            this.tbRXEQ3.AutoSize = false;
            this.tbRXEQ3.LargeChange = 3;
            this.tbRXEQ3.Location = new System.Drawing.Point(168, 72);
            this.tbRXEQ3.Maximum = 15;
            this.tbRXEQ3.Minimum = -12;
            this.tbRXEQ3.Name = "tbRXEQ3";
            this.tbRXEQ3.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQ3.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQ3.TabIndex = 6;
            this.tbRXEQ3.TickFrequency = 3;
            this.tbRXEQ3.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // lblRXEQ2
            // 
            this.lblRXEQ2.Image = null;
            this.lblRXEQ2.Location = new System.Drawing.Point(120, 56);
            this.lblRXEQ2.Name = "lblRXEQ2";
            this.lblRXEQ2.Size = new System.Drawing.Size(40, 16);
            this.lblRXEQ2.TabIndex = 44;
            this.lblRXEQ2.Text = "63";
            this.lblRXEQ2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRXEQ2.Visible = false;
            // 
            // lblRXEQ3
            // 
            this.lblRXEQ3.Image = null;
            this.lblRXEQ3.Location = new System.Drawing.Point(160, 56);
            this.lblRXEQ3.Name = "lblRXEQ3";
            this.lblRXEQ3.Size = new System.Drawing.Size(40, 16);
            this.lblRXEQ3.TabIndex = 45;
            this.lblRXEQ3.Text = "125";
            this.lblRXEQ3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRXEQ3.Visible = false;
            // 
            // lblRXEQPreamp
            // 
            this.lblRXEQPreamp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRXEQPreamp.Image = null;
            this.lblRXEQPreamp.Location = new System.Drawing.Point(8, 56);
            this.lblRXEQPreamp.Name = "lblRXEQPreamp";
            this.lblRXEQPreamp.Size = new System.Drawing.Size(48, 16);
            this.lblRXEQPreamp.TabIndex = 74;
            this.lblRXEQPreamp.Text = "Preamp";
            this.lblRXEQPreamp.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tbRXEQPreamp
            // 
            this.tbRXEQPreamp.AutoSize = false;
            this.tbRXEQPreamp.LargeChange = 3;
            this.tbRXEQPreamp.Location = new System.Drawing.Point(16, 72);
            this.tbRXEQPreamp.Maximum = 15;
            this.tbRXEQPreamp.Minimum = -12;
            this.tbRXEQPreamp.Name = "tbRXEQPreamp";
            this.tbRXEQPreamp.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbRXEQPreamp.Size = new System.Drawing.Size(32, 128);
            this.tbRXEQPreamp.TabIndex = 35;
            this.tbRXEQPreamp.TickFrequency = 3;
            this.tbRXEQPreamp.Scroll += new System.EventHandler(this.tbRXEQ_Scroll);
            // 
            // lblRXEQ15db
            // 
            this.lblRXEQ15db.Image = null;
            this.lblRXEQ15db.Location = new System.Drawing.Point(56, 78);
            this.lblRXEQ15db.Name = "lblRXEQ15db";
            this.lblRXEQ15db.Size = new System.Drawing.Size(32, 16);
            this.lblRXEQ15db.TabIndex = 40;
            this.lblRXEQ15db.Text = "15dB";
            this.lblRXEQ15db.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRXEQ0dB
            // 
            this.lblRXEQ0dB.Image = null;
            this.lblRXEQ0dB.Location = new System.Drawing.Point(56, 134);
            this.lblRXEQ0dB.Name = "lblRXEQ0dB";
            this.lblRXEQ0dB.Size = new System.Drawing.Size(32, 16);
            this.lblRXEQ0dB.TabIndex = 41;
            this.lblRXEQ0dB.Text = "  0dB";
            this.lblRXEQ0dB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRXEQminus12db
            // 
            this.lblRXEQminus12db.Image = null;
            this.lblRXEQminus12db.Location = new System.Drawing.Point(52, 178);
            this.lblRXEQminus12db.Name = "lblRXEQminus12db";
            this.lblRXEQminus12db.Size = new System.Drawing.Size(38, 16);
            this.lblRXEQminus12db.TabIndex = 42;
            this.lblRXEQminus12db.Text = "-12dB";
            this.lblRXEQminus12db.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlParaEQ2
            // 
            this.pnlParaEQ2.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.pnlParaEQ2.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.pnlParaEQ2.Controls.Add(this.pbParaEQ_live_warning);
            this.pnlParaEQ2.Controls.Add(this.panelTS1);
            this.pnlParaEQ2.Controls.Add(this.chkUseQFactors);
            this.pnlParaEQ2.Controls.Add(this.udParaEQ_low);
            this.pnlParaEQ2.Controls.Add(this.chkPanaEQ_live);
            this.pnlParaEQ2.Controls.Add(this.udParaEQ_high);
            this.pnlParaEQ2.Controls.Add(this.labelTS1);
            this.pnlParaEQ2.Controls.Add(this.labelTS2);
            this.pnlParaEQ2.Location = new System.Drawing.Point(789, 7);
            this.pnlParaEQ2.Name = "pnlParaEQ2";
            this.pnlParaEQ2.Size = new System.Drawing.Size(316, 73);
            this.pnlParaEQ2.TabIndex = 8;
            // 
            // EQForm
            // 
            this.ClientSize = new System.Drawing.Size(1115, 533);
            this.Controls.Add(this.pnlParaEQ2);
            this.Controls.Add(this.pnlParaEQ);
            this.Controls.Add(this.chkLegacyEQ);
            this.Controls.Add(this.pnlLegacyEQ);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(572, 572);
            this.Name = "EQForm";
            this.Text = "Equalizer Settings";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.EQForm_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.pbParaEQ_live_warning)).EndInit();
            this.pnlParaEQ.ResumeLayout(false);
            this.pnlParaEQ.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudParaEQ_preamp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudParaEQ_f)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudParaEQ_q)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudParaEQ_gain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudParaEQ_selected_band)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udParaEQ_high)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udParaEQ_low)).EndInit();
            this.panelTS1.ResumeLayout(false);
            this.panelTS1.PerformLayout();
            this.pnlLegacyEQ.ResumeLayout(false);
            this.grpTXEQ.ResumeLayout(false);
            this.grpTXEQ.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTXEQ0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQ2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbTXEQPre)).EndInit();
            this.grpRXEQ.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRXEQ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQ3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbRXEQPreamp)).EndInit();
            this.pnlParaEQ2.ResumeLayout(false);
            this.pnlParaEQ2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region Properties
        public void HighlightTXProfileSaveItems(bool bHighlight)
        {
            Common.HightlightControl(chkTXEQEnabled, bHighlight);

            Common.HightlightControl(rad3Band, bHighlight);
            Common.HightlightControl(rad10Band, bHighlight);

            Common.HightlightControl(tbTXEQPre, bHighlight);
            Common.HightlightControl(tbTXEQ0, bHighlight);
            Common.HightlightControl(tbTXEQ1, bHighlight);
            Common.HightlightControl(tbTXEQ2, bHighlight);
            Common.HightlightControl(tbTXEQ3, bHighlight);
            Common.HightlightControl(tbTXEQ4, bHighlight);
            Common.HightlightControl(tbTXEQ5, bHighlight);
            Common.HightlightControl(tbTXEQ6, bHighlight);
            Common.HightlightControl(tbTXEQ7, bHighlight);
            Common.HightlightControl(tbTXEQ8, bHighlight);
            Common.HightlightControl(tbTXEQ9, bHighlight);

            Common.HightlightControl(udTXEQ0, bHighlight);
            Common.HightlightControl(udTXEQ1, bHighlight);
            Common.HightlightControl(udTXEQ2, bHighlight);
            Common.HightlightControl(udTXEQ3, bHighlight);
            Common.HightlightControl(udTXEQ4, bHighlight);
            Common.HightlightControl(udTXEQ5, bHighlight);
            Common.HightlightControl(udTXEQ6, bHighlight);
            Common.HightlightControl(udTXEQ7, bHighlight);
            Common.HightlightControl(udTXEQ8, bHighlight);
            Common.HightlightControl(udTXEQ9, bHighlight);

            //
            Common.HightlightControl(chkRXEQEnabled, bHighlight);
            Common.HightlightControl(tbRXEQPreamp, bHighlight);
            Common.HightlightControl(tbRXEQ1, bHighlight);
            Common.HightlightControl(tbRXEQ2, bHighlight);
            Common.HightlightControl(tbRXEQ3, bHighlight);
            Common.HightlightControl(tbRXEQ4, bHighlight);
            Common.HightlightControl(tbRXEQ5, bHighlight);
            Common.HightlightControl(tbRXEQ6, bHighlight);
            Common.HightlightControl(tbRXEQ7, bHighlight);
            Common.HightlightControl(tbRXEQ8, bHighlight);
            Common.HightlightControl(tbRXEQ9, bHighlight);
            Common.HightlightControl(tbRXEQ10, bHighlight);
            //
        }

        public int NumBands
        {
            get
            {
                if (rad3Band.Checked) return 3;
                else return 10;
            }
            set
            {
                switch (value)
                {
                    case 3: rad3Band.Checked = true; break;
                    case 10: rad10Band.Checked = true; break;
                }
            }
        }

        public int[] RXEQ
        {
            get
            {
                if (rad3Band.Checked)
                {
                    int[] eq = new int[4];
                    eq[0] = tbRXEQPreamp.Value;
                    eq[1] = tbRXEQ1.Value;
                    eq[2] = tbRXEQ5.Value;
                    eq[3] = tbRXEQ9.Value;
                    return eq;
                }
                else //if(rad10Band.Checked)
                {
                    int[] eq = new int[11];
                    eq[0] = tbRXEQPreamp.Value;
                    eq[1] = tbRXEQ1.Value;
                    eq[2] = tbRXEQ2.Value;
                    eq[3] = tbRXEQ3.Value;
                    eq[4] = tbRXEQ4.Value;
                    eq[5] = tbRXEQ5.Value;
                    eq[6] = tbRXEQ6.Value;
                    eq[7] = tbRXEQ7.Value;
                    eq[8] = tbRXEQ8.Value;
                    eq[9] = tbRXEQ9.Value;
                    eq[10] = tbRXEQ10.Value;
                    return eq;
                }
            }

            set
            {
                if (rad3Band.Checked)
                {
                    if (value.Length < 4)
                    {
                        MessageBox.Show("Error setting RX EQ");
                        return;
                    }
                    tbRXEQPreamp.Value = Math.Max(tbRXEQPreamp.Minimum, Math.Min(tbRXEQPreamp.Maximum, value[0]));
                    tbRXEQ1.Value = Math.Max(tbRXEQ1.Minimum, Math.Min(tbRXEQ1.Maximum, value[1]));
                    tbRXEQ5.Value = Math.Max(tbRXEQ5.Minimum, Math.Min(tbRXEQ5.Maximum, value[2]));
                    tbRXEQ9.Value = Math.Max(tbRXEQ9.Minimum, Math.Min(tbRXEQ9.Maximum, value[3]));
                }
                else if (rad10Band.Checked)
                {
                    if (value.Length < 11)
                    {
                        MessageBox.Show("Error setting RX EQ");
                        return;
                    }
                    tbRXEQPreamp.Value = Math.Max(tbRXEQPreamp.Minimum, Math.Min(tbRXEQPreamp.Maximum, value[0]));
                    tbRXEQ1.Value = Math.Max(tbRXEQ1.Minimum, Math.Min(tbRXEQ1.Maximum, value[1]));
                    tbRXEQ2.Value = Math.Max(tbRXEQ2.Minimum, Math.Min(tbRXEQ2.Maximum, value[2]));
                    tbRXEQ3.Value = Math.Max(tbRXEQ3.Minimum, Math.Min(tbRXEQ3.Maximum, value[3]));
                    tbRXEQ4.Value = Math.Max(tbRXEQ4.Minimum, Math.Min(tbRXEQ4.Maximum, value[4]));
                    tbRXEQ5.Value = Math.Max(tbRXEQ5.Minimum, Math.Min(tbRXEQ5.Maximum, value[5]));
                    tbRXEQ6.Value = Math.Max(tbRXEQ6.Minimum, Math.Min(tbRXEQ6.Maximum, value[6]));
                    tbRXEQ7.Value = Math.Max(tbRXEQ7.Minimum, Math.Min(tbRXEQ7.Maximum, value[7]));
                    tbRXEQ8.Value = Math.Max(tbRXEQ8.Minimum, Math.Min(tbRXEQ8.Maximum, value[8]));
                    tbRXEQ9.Value = Math.Max(tbRXEQ9.Minimum, Math.Min(tbRXEQ9.Maximum, value[9]));
                    tbRXEQ10.Value = Math.Max(tbRXEQ10.Minimum, Math.Min(tbRXEQ10.Maximum, value[10]));
                }

                picRXEQ.Invalidate();
                tbRXEQ_Scroll(this, EventArgs.Empty);

                // nasty, if only the value changed event had been used instead. At least 1 5 9 is used in 3 band so dont need any special case for this
                // notice how now we are RXEQ1 and not RXEQ0 as per the TX array :/
                setDBtip(tbRXEQPreamp);
                setDBtip(tbRXEQ1);
                setDBtip(tbRXEQ2);
                setDBtip(tbRXEQ3);
                setDBtip(tbRXEQ4);
                setDBtip(tbRXEQ5);
                setDBtip(tbRXEQ6);
                setDBtip(tbRXEQ7);
                setDBtip(tbRXEQ8);
                setDBtip(tbRXEQ9);
                setDBtip(tbRXEQ10);
            }
        }

        public int[] TXEQ
        {
            get
            {
                //if(rad3Band.Checked)
                //{
                //    int[] eq = new int[4];
                //    //eq[0] = tbTXEQPreamp.Value;
                //    //eq[1] = tbTXEQ1.Value;
                //    //eq[2] = tbTXEQ5.Value;
                //    //eq[3] = tbTXEQ9.Value;
                //    return eq;
                //}
                //else //if(rad10Band.Checked)
                //{
                int[] eq = new int[21];
                eq[0] = tbTXEQPre.Value;
                eq[1] = tbTXEQ0.Value;
                eq[2] = tbTXEQ1.Value;
                eq[3] = tbTXEQ2.Value;
                eq[4] = tbTXEQ3.Value;
                eq[5] = tbTXEQ4.Value;
                eq[6] = tbTXEQ5.Value;
                eq[7] = tbTXEQ6.Value;
                eq[8] = tbTXEQ7.Value;
                eq[9] = tbTXEQ8.Value;
                eq[10] = tbTXEQ9.Value;

                eq[11] = (int)udTXEQ0.Value;
                eq[12] = (int)udTXEQ1.Value;
                eq[13] = (int)udTXEQ2.Value;
                eq[14] = (int)udTXEQ3.Value;
                eq[15] = (int)udTXEQ4.Value;
                eq[16] = (int)udTXEQ5.Value;
                eq[17] = (int)udTXEQ6.Value;
                eq[18] = (int)udTXEQ7.Value;
                eq[19] = (int)udTXEQ8.Value;
                eq[20] = (int)udTXEQ9.Value;
                return eq;
                //}
            }
            set
            {
                //if(rad3Band.Checked)
                //{
                //    if(value.Length < 4)
                //    {
                //        MessageBox.Show("Error setting TX EQ");
                //        return;
                //    }
                //    //tbTXEQPreamp.Value = Math.Max(tbTXEQPreamp.Minimum, Math.Min(tbTXEQPreamp.Maximum, value[0]));
                //    //tbTXEQ1.Value = Math.Max(tbTXEQ1.Minimum, Math.Min(tbTXEQ1.Maximum, value[1]));
                //    //tbTXEQ5.Value = Math.Max(tbTXEQ5.Minimum, Math.Min(tbTXEQ5.Maximum, value[2]));
                //    //tbTXEQ9.Value = Math.Max(tbTXEQ9.Minimum, Math.Min(tbTXEQ9.Maximum, value[3]));
                //}
                //else if(rad10Band.Checked)
                //{
                //    if(value.Length < 11)
                //    {
                //        MessageBox.Show("Error setting TX EQ");
                //        return;
                //    }
                tbTXEQPre.Value = Math.Max(tbTXEQPre.Minimum, Math.Min(tbTXEQPre.Maximum, value[0]));
                tbTXEQ0.Value = Math.Max(tbTXEQ0.Minimum, Math.Min(tbTXEQ0.Maximum, value[1]));
                tbTXEQ1.Value = Math.Max(tbTXEQ1.Minimum, Math.Min(tbTXEQ1.Maximum, value[2]));
                tbTXEQ2.Value = Math.Max(tbTXEQ2.Minimum, Math.Min(tbTXEQ2.Maximum, value[3]));
                tbTXEQ3.Value = Math.Max(tbTXEQ3.Minimum, Math.Min(tbTXEQ3.Maximum, value[4]));
                tbTXEQ4.Value = Math.Max(tbTXEQ4.Minimum, Math.Min(tbTXEQ4.Maximum, value[5]));
                tbTXEQ5.Value = Math.Max(tbTXEQ5.Minimum, Math.Min(tbTXEQ5.Maximum, value[6]));
                tbTXEQ6.Value = Math.Max(tbTXEQ6.Minimum, Math.Min(tbTXEQ6.Maximum, value[7]));
                tbTXEQ7.Value = Math.Max(tbTXEQ7.Minimum, Math.Min(tbTXEQ7.Maximum, value[8]));
                tbTXEQ8.Value = Math.Max(tbTXEQ8.Minimum, Math.Min(tbTXEQ8.Maximum, value[9]));
                tbTXEQ9.Value = Math.Max(tbTXEQ9.Minimum, Math.Min(tbTXEQ9.Maximum, value[10]));

                udTXEQ0.Value = Math.Max(udTXEQ0.Minimum, Math.Min(udTXEQ0.Maximum, value[11]));
                udTXEQ1.Value = Math.Max(udTXEQ1.Minimum, Math.Min(udTXEQ1.Maximum, value[12]));
                udTXEQ2.Value = Math.Max(udTXEQ2.Minimum, Math.Min(udTXEQ2.Maximum, value[13]));
                udTXEQ3.Value = Math.Max(udTXEQ3.Minimum, Math.Min(udTXEQ3.Maximum, value[14]));
                udTXEQ4.Value = Math.Max(udTXEQ4.Minimum, Math.Min(udTXEQ4.Maximum, value[15]));
                udTXEQ5.Value = Math.Max(udTXEQ5.Minimum, Math.Min(udTXEQ5.Maximum, value[16]));
                udTXEQ6.Value = Math.Max(udTXEQ6.Minimum, Math.Min(udTXEQ6.Maximum, value[17]));
                udTXEQ7.Value = Math.Max(udTXEQ7.Minimum, Math.Min(udTXEQ7.Maximum, value[18]));
                udTXEQ8.Value = Math.Max(udTXEQ8.Minimum, Math.Min(udTXEQ8.Maximum, value[19]));
                udTXEQ9.Value = Math.Max(udTXEQ9.Minimum, Math.Min(udTXEQ9.Maximum, value[20]));
                //}
                //picTXEQ.Invalidate();
                //tbTXEQ_Scroll(this, EventArgs.Empty);

                //MW0LGE_21g
                // the sliders accessed above with the new data do not cause scroll events
                // so nothing will get updated. They may cause value change events but these are not handled
                // let us force the txeqprofile through
                setTXEQProfile(this, EventArgs.Empty);
                //

                // nasty, if only the value changed event had been used instead
                setDBtip(tbTXEQPre);
                setDBtip(tbTXEQ0);
                setDBtip(tbTXEQ1);
                setDBtip(tbTXEQ2);
                setDBtip(tbTXEQ3);
                setDBtip(tbTXEQ4);
                setDBtip(tbTXEQ5);
                setDBtip(tbTXEQ6);
                setDBtip(tbTXEQ7);
                setDBtip(tbTXEQ8);
                setDBtip(tbTXEQ9);

            }
        }

        public bool RXEQEnabled
        {
            get
            {
                if (_state.UsingLegacyEQ)
                {
                    if (chkRXEQEnabled != null) return chkRXEQEnabled.Checked;
                    else return false;
                }
                else
                {
                    return _state.RXenabled;
                }                
            }
            set
            {
                if (_state.UsingLegacyEQ)
                {
                    if (chkRXEQEnabled != null) chkRXEQEnabled.Checked = value;
                }
                else
                {
                    _state.RXenabled = value;
                }
            }
        }

        public bool TXEQEnabled
        {
            get
            {
                if (_state.UsingLegacyEQ)
                {
                    if (chkTXEQEnabled != null) return chkTXEQEnabled.Checked;
                    else return false;
                }
                else
                {
                    return _state.TXenabled;
                }
            }
            set
            {
                if (_state.UsingLegacyEQ)
                {
                    if (chkTXEQEnabled != null) chkTXEQEnabled.Checked = value;
                }
                else
                {
                    _state.TXenabled = value;
                }
            }
        }

        #endregion

        #region Event Handlers

        private void EQForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            setupTimer(false);

            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, "EQForm");
        }

        private void tbRXEQ_Scroll(object sender, System.EventArgs e)
        {
            if (!_state.UsingLegacyEQ) return;

            int[] rxeq = RXEQ;
            if (rad3Band.Checked)
            {
                console.radio.GetDSPRX(0, 0).RXEQ3 = rxeq;
                console.radio.GetDSPRX(0, 1).RXEQ3 = rxeq;
                console.radio.GetDSPRX(1, 0).RXEQ3 = rxeq;
            }
            else
            {
                console.radio.GetDSPRX(0, 0).RXEQ10 = rxeq;
                console.radio.GetDSPRX(0, 1).RXEQ10 = rxeq;
                console.radio.GetDSPRX(1, 0).RXEQ10 = rxeq;
            }
            picRXEQ.Invalidate();

            setDBtip(sender);
        }

        private void setDBtip(object sender)
        {
            if (sender.GetType() != typeof(TrackBarTS)) return;
            TrackBarTS tb = (TrackBarTS)sender;

            int db;

            switch (tb.Name)
            {
                //RX
                case "tbRXEQPreamp":
                    db = tbRXEQPreamp.Value;
                    break;
                case "tbRXEQ1":
                    db = tbRXEQ1.Value;
                    break;
                case "tbRXEQ2":
                    db = tbRXEQ2.Value;
                    break;
                case "tbRXEQ3":
                    db = tbRXEQ3.Value;
                    break;
                case "tbRXEQ4":
                    db = tbRXEQ4.Value;
                    break;
                case "tbRXEQ5":
                    db = tbRXEQ5.Value;
                    break;
                case "tbRXEQ6":
                    db = tbRXEQ6.Value;
                    break;
                case "tbRXEQ7":
                    db = tbRXEQ7.Value;
                    break;
                case "tbRXEQ8":
                    db = tbRXEQ8.Value;
                    break;
                case "tbRXEQ9":
                    db = tbRXEQ9.Value;
                    break;
                case "tbRXEQ10":
                    db = tbRXEQ10.Value;
                    break;
                //TX
                case "tbTXEQPre":
                    db = tbTXEQPre.Value;
                    break;
                case "tbTXEQ0":
                    db = tbTXEQ0.Value;
                    break;
                case "tbTXEQ1":
                    db = tbTXEQ1.Value;
                    break;
                case "tbTXEQ2":
                    db = tbTXEQ2.Value;
                    break;
                case "tbTXEQ3":
                    db = tbTXEQ3.Value;
                    break;
                case "tbTXEQ4":
                    db = tbTXEQ4.Value;
                    break;
                case "tbTXEQ5":
                    db = tbTXEQ5.Value;
                    break;
                case "tbTXEQ6":
                    db = tbTXEQ6.Value;
                    break;
                case "tbTXEQ7":
                    db = tbTXEQ7.Value;
                    break;
                case "tbTXEQ8":
                    db = tbTXEQ8.Value;
                    break;
                case "tbTXEQ9":
                    db = tbTXEQ9.Value;
                    break;

                default:
                    return;
            }
            toolTip1.SetToolTip(tb, db.ToString() + " dB");
        }
        //private void tbTXEQ_Scroll(object sender, System.EventArgs e)
        //{
        //    int[] txeq = TXEQ;
        //    if(rad3Band.Checked) 
        //    {
        //        console.radio.GetDSPTX(0).TXEQ3 = txeq;
        //    }
        //    else
        //    {
        //        console.radio.GetDSPTX(0).TXEQ10 = txeq;
        //    }
        //    picTXEQ.Invalidate();
        //}

        private void picRXEQ_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            int[] rxeq = RXEQ;
            if (!chkRXEQEnabled.Checked)
            {
                for (int i = 0; i < rxeq.Length; i++)
                    rxeq[i] = 0;
            }

            Point[] points = new Point[rxeq.Length - 1];
            for (int i = 1; i < rxeq.Length; i++)
            {
                points[i - 1].X = (int)((i - 1) * picRXEQ.Width / (float)(rxeq.Length - 2));
                points[i - 1].Y = picRXEQ.Height / 2 - (int)(rxeq[i] * (picRXEQ.Height - 6) / 2 / 15.0f +
                    tbRXEQPreamp.Value * 3 / 15.0f);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, picRXEQ.Width, picRXEQ.Height);
            e.Graphics.DrawLines(new Pen(Color.LightGreen), points);
        }

        //private void picTXEQ_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        //{
        //    int[] txeq = TXEQ;
        //    if(!chkTXEQEnabled.Checked)
        //    {
        //        for(int i=0; i<txeq.Length; i++)
        //            txeq[i] = 0;
        //    }

        //    Point[] points = new Point[txeq.Length-1];
        //    for(int i=1; i<txeq.Length; i++)
        //    {
        //        points[i-1].X = (int)((i-1)*picTXEQ.Width/(float)(txeq.Length-2));
        //        points[i-1].Y = picTXEQ.Height/2 - (int)(txeq[i]*(picTXEQ.Height-6)/2/15.0f +
        //            tbTXEQPre.Value * 3 / 15.0f);
        //    }

        //    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        //    e.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, picTXEQ.Width, picTXEQ.Height);
        //    e.Graphics.DrawLines(new Pen(Color.LightGreen), points);
        //}

        private void chkRXEQEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_state.UsingLegacyEQ) return;

            enableRxEq(chkRXEQEnabled.Checked);
        }

        private void enableRxEq(bool enable)
        {
            console.radio.GetDSPRX(0, 0).RXEQOn = enable;
            console.radio.GetDSPRX(1, 0).RXEQOn = enable; //MW0LGE_21b set anyway, even if using single rx as the
                                                                          //eq data is applied to this rx anyway
                                                                          //in tbRXEQ_Scroll
            console.RXEQ = enable;

            if (_state.UsingLegacyEQ)
            {
                picRXEQ.Invalidate();
            }
        }
        private void chkTXEQEnabled_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!_state.UsingLegacyEQ) return;

            enableTxEq(chkTXEQEnabled.Checked);
        }
        private void enableTxEq(bool enable)
        {
            console.radio.GetDSPTX(0).TXEQOn = enable;
            console.TXEQ = enable;
        }

        private void btnRXEQReset_Click(object sender, System.EventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                "Are you sure you want to reset the Receive Equalizer\n" +
                "to flat (zero)?",
                "Are you sure?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr == DialogResult.No)
                return;

            foreach (Control c in grpRXEQ.Controls)
            {
                if (c.GetType() == typeof(TrackBarTS))
                    ((TrackBarTS)c).Value = 0;
            }

            tbRXEQ_Scroll(this, EventArgs.Empty);
        }

        //private void btnTXEQReset_Click(object sender, System.EventArgs e)
        //{
        //    DialogResult dr = MessageBox.Show(
        //        "Are you sure you want to reset the Transmit Equalizer\n"+
        //        "to flat (zero)?",
        //        "Are you sure?",
        //        MessageBoxButtons.YesNo,
        //        MessageBoxIcon.Question);

        //    if(dr == DialogResult.No)
        //        return;

        //    foreach (Control c in grpTXEQ.Controls)
        //    {
        //        if (c.GetType() == typeof(TrackBarTS))
        //            ((TrackBarTS)c).Value = 0;
        //    }

        //    //tbTXEQ_Scroll(this, EventArgs.Empty);
        //}

        private void rad3Band_CheckedChanged(object sender, System.EventArgs e)
        {
            if (rad3Band.Checked)
            {
                lblRXEQ2.Visible = false;
                lblRXEQ3.Visible = false;
                lblRXEQ4.Visible = false;
                lblRXEQ6.Visible = false;
                lblRXEQ7.Visible = false;
                lblRXEQ8.Visible = false;
                lblRXEQ10.Visible = false;

                tbRXEQ2.Visible = false;
                tbRXEQ3.Visible = false;
                tbRXEQ4.Visible = false;
                tbRXEQ6.Visible = false;
                tbRXEQ7.Visible = false;
                tbRXEQ8.Visible = false;
                tbRXEQ10.Visible = false;

                lblRXEQ1.Text = "Low";
                lblRXEQ5.Text = "Mid";
                lblRXEQ9.Text = "High";

                toolTip1.SetToolTip(lblRXEQ1, "0-400Hz");
                toolTip1.SetToolTip(tbRXEQ1, "0-400Hz");
                toolTip1.SetToolTip(lblRXEQ5, "400-1500Hz");
                toolTip1.SetToolTip(tbRXEQ5, "400-1500Hz");
                toolTip1.SetToolTip(lblRXEQ9, "1500-6000Hz");
                toolTip1.SetToolTip(tbRXEQ9, "1500-6000Hz");

                //lblTXEQ2.Visible = false;
                //lblTXEQ3.Visible = false;
                //lblTXEQ4.Visible = false;
                //lblTXEQ6.Visible = false;
                //lblTXEQ7.Visible = false;
                //lblTXEQ8.Visible = false;
                //lblTXEQ10.Visible = false;

                //tbTXEQ2.Visible = false;
                //tbTXEQ3.Visible = false;
                //tbTXEQ4.Visible = false;
                //tbTXEQ6.Visible = false;
                //tbTXEQ7.Visible = false;
                //tbTXEQ8.Visible = false;
                //tbTXEQ10.Visible = false;

                //lblTXEQ1.Text = "Low";
                //lblTXEQ5.Text = "Mid";
                //lblTXEQ9.Text = "High";

                //toolTip1.SetToolTip(lblTXEQ1, "0-400Hz");
                //toolTip1.SetToolTip(tbTXEQ1, "0-400Hz");
                //toolTip1.SetToolTip(lblTXEQ5, "400-1500Hz");
                //toolTip1.SetToolTip(tbTXEQ5, "400-1500Hz");
                //toolTip1.SetToolTip(lblTXEQ9, "1500-6000Hz");
                //toolTip1.SetToolTip(tbTXEQ9, "1500-6000Hz");

                RXEQ = console.radio.GetDSPRX(0, 0).RXEQ3;
                //TXEQ = console.radio.GetDSPTX(0).TXEQ3;

                tbRXEQ_Scroll(this, EventArgs.Empty);
                //tbTXEQ_Scroll(this, EventArgs.Empty);

                picRXEQ.Invalidate();
                //picTXEQ.Invalidate();

                console.radio.GetDSPRX(0, 0).RXEQNumBands = 3;
                //console.radio.GetDSPTX(0).TXEQNumBands = 3;
            }
        }

        private void rad10Band_CheckedChanged(object sender, System.EventArgs e)
        {
            if (rad10Band.Checked)
            {
                lblRXEQ2.Visible = true;
                lblRXEQ3.Visible = true;
                lblRXEQ4.Visible = true;
                lblRXEQ6.Visible = true;
                lblRXEQ7.Visible = true;
                lblRXEQ8.Visible = true;
                lblRXEQ10.Visible = true;

                tbRXEQ2.Visible = true;
                tbRXEQ3.Visible = true;
                tbRXEQ4.Visible = true;
                tbRXEQ6.Visible = true;
                tbRXEQ7.Visible = true;
                tbRXEQ8.Visible = true;
                tbRXEQ10.Visible = true;

                lblRXEQ1.Text = "32";
                lblRXEQ5.Text = "500";
                lblRXEQ9.Text = "8K";

                toolTip1.SetToolTip(lblRXEQ1, "");
                toolTip1.SetToolTip(tbRXEQ1, "");
                toolTip1.SetToolTip(lblRXEQ5, "");
                toolTip1.SetToolTip(tbRXEQ5, "");
                toolTip1.SetToolTip(lblRXEQ9, "");
                toolTip1.SetToolTip(tbRXEQ9, "");

                //lblTXEQ2.Visible = true;
                //lblTXEQ3.Visible = true;
                //lblTXEQ4.Visible = true;
                //lblTXEQ6.Visible = true;
                //lblTXEQ7.Visible = true;
                //lblTXEQ8.Visible = true;
                //lblTXEQ10.Visible = true;

                //tbTXEQ2.Visible = true;
                //tbTXEQ3.Visible = true;
                //tbTXEQ4.Visible = true;
                //tbTXEQ6.Visible = true;
                //tbTXEQ7.Visible = true;
                //tbTXEQ8.Visible = true;
                //tbTXEQ10.Visible = true;

                //lblTXEQ1.Text = "32";
                //lblTXEQ5.Text = "500";
                //lblTXEQ9.Text = "8K";

                //toolTip1.SetToolTip(lblTXEQ1, "");
                //toolTip1.SetToolTip(tbTXEQ1, "");
                //toolTip1.SetToolTip(lblTXEQ5, "");
                //toolTip1.SetToolTip(tbTXEQ5, "");
                //toolTip1.SetToolTip(lblTXEQ9, "");
                //toolTip1.SetToolTip(tbTXEQ9, "");

                RXEQ = console.radio.GetDSPRX(0, 0).RXEQ10;
                //TXEQ = console.radio.GetDSPTX(0).TXEQ10;

                tbRXEQ_Scroll(this, EventArgs.Empty);
                //tbTXEQ_Scroll(this, EventArgs.Empty);

                picRXEQ.Invalidate();
                //picTXEQ.Invalidate();	

                console.radio.GetDSPRX(0, 0).RXEQNumBands = 10;
                //console.radio.GetDSPTX(0).TXEQNumBands = 10;
            }
        }
        public void SetTXProfile()
        {
            if (_state.UsingLegacyEQ)
            {
                setTXEQProfile(this, EventArgs.Empty);
            }
            else
            {
                setupWDSPdataFromParaEQ(true);
                setupWDSPdataFromParaEQ(false);
            }
        }
        private void setTXEQProfile(object sender, EventArgs e)
        {
            if (!_state.UsingLegacyEQ) return;

            const int nfreqs = 10;
            double[] F = new double[nfreqs + 1];
            double[] G = new double[nfreqs + 1];
            F[0] = 0.0;
            F[1] = (double)udTXEQ0.Value;
            F[2] = (double)udTXEQ1.Value;
            F[3] = (double)udTXEQ2.Value;
            F[4] = (double)udTXEQ3.Value;
            F[5] = (double)udTXEQ4.Value;
            F[6] = (double)udTXEQ5.Value;
            F[7] = (double)udTXEQ6.Value;
            F[8] = (double)udTXEQ7.Value;
            F[9] = (double)udTXEQ8.Value;
            F[10] = (double)udTXEQ9.Value;
            G[0] = (double)tbTXEQPre.Value;
            G[1] = (double)tbTXEQ0.Value;
            G[2] = (double)tbTXEQ1.Value;
            G[3] = (double)tbTXEQ2.Value;
            G[4] = (double)tbTXEQ3.Value;
            G[5] = (double)tbTXEQ4.Value;
            G[6] = (double)tbTXEQ5.Value;
            G[7] = (double)tbTXEQ6.Value;
            G[8] = (double)tbTXEQ7.Value;
            G[9] = (double)tbTXEQ8.Value;
            G[10] = (double)tbTXEQ9.Value;
            unsafe
            {
                fixed (double* Fptr = &F[0], Gptr = &G[0])
                {
                    WDSP.SetTXAEQProfile(WDSP.id(1, 0), nfreqs, Fptr, Gptr, null);
                }
            }

            setDBtip(sender);
        }

        public ToolTip ToolTip
        {
            get
            {
                return toolTip1;
            }
        }
        #endregion

        //-------------------------------------
        private class ParaEQState
        {
            private EQForm _eqform;
            public ParaEQState(EQForm eqform)
            {
                _eqform = eqform;
            }

            public bool UsingLegacyEQ;
            public bool RXselected;
            public int BandSelected = -1;
            public bool LiveUpdate = false;

            private bool _rx_enabled = false;
            public bool RXenabled
            {
                get { return _rx_enabled; }
                set
                {
                    _rx_enabled = value;
                    _eqform.UpdateEQEnabled(true);
                }
            }
            public int RX_BandCount = 10;
            public bool RX_ParametricEQ = true;
            public double[] RX_F;
            public double[] RX_G;
            public double[] RX_Q;
            public double RX_Preamp = 0;
            public double RX_minHz = 0;
            public double RX_maxHz = 4000;

            private bool _tx_enabled = false;
            public bool TXenabled
            {
                get { return _tx_enabled; }
                set
                {
                    _tx_enabled = value;
                    _eqform.UpdateEQEnabled(false);
                }
            }
            public int TX_BandCount = 10;
            public bool TX_ParametricEQ = true;
            public double[] TX_F;
            public double[] TX_G;
            public double[] TX_Q;
            public double TX_Preamp = 0;
            public double TX_minHz = 0;
            public double TX_maxHz = 4000;
        }
        private ParaEQState _state;

        private void chkLegacyEQ_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            _state.UsingLegacyEQ = chkLegacyEQ.Checked;
            setupTimer(!_state.UsingLegacyEQ);

            console.radio.GetDSPRX(0, 0).LegacyEQ = _state.UsingLegacyEQ;
            console.radio.GetDSPRX(0, 1).LegacyEQ = _state.UsingLegacyEQ;
            console.radio.GetDSPRX(1, 0).LegacyEQ = _state.UsingLegacyEQ;

            this.Size = _legacyEQ_base_form_size;

            bool enabled = chkLegacyEQ.Checked;
            if (enabled)
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.SizeGripStyle = SizeGripStyle.Hide;
                pnlLegacyEQ.Location = _legacyEQ_base_panel_location;
                pnlLegacyEQ.Visible = true;
                pnlParaEQ.Visible = false;
                pnlParaEQ2.Visible = false;

                chkRXEQEnabled.Checked = console.RXEQ;
                chkTXEQEnabled.Checked = console.TXEQ;

                tbRXEQ_Scroll(this, EventArgs.Empty);
                rad10Band_CheckedChanged(this, EventArgs.Empty);
                setTXEQProfile(this, EventArgs.Empty);
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.SizeGripStyle = SizeGripStyle.Show;

                pnlLegacyEQ.Visible = false;
                pnlParaEQ.Location = _legacyEQ_base_panel_location;
                pnlParaEQ.Visible = true;
                Point p2Loc = new Point(pnlParaEQ.Location.X + 220, pnlParaEQ2.Location.Y);
                pnlParaEQ2.Location = p2Loc;
                pnlParaEQ2.Visible = true;

                radParaEQ_RXTX_CheckedChanged(sender, e);
                _state.RXenabled = console.RXEQ;
                _state.TXenabled = console.TXEQ;
                
                radParaEQ_CheckedChanged(sender, e);
                chkUseQFactors_CheckedChanged(sender, e);

                pnlParaEQ.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
                pnlParaEQ2.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                pnlParaEQ2.BringToFront();
            }
        }

        private void chkUseQFactors_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            if (_state.RXselected)
            {
                _state.RX_ParametricEQ = chkUseQFactors.Checked;
            }
            else
            {
                _state.TX_ParametricEQ = chkUseQFactors.Checked;
            }

            ucParametricEq1.ParametricEQ = chkUseQFactors.Checked;
            ucParametricEq1_PointsChanged(sender, new ucParametricEq.EqDraggingEventArgs(false));

            updateBandData();
        }

        private void setupWDSPdataFromParaEQ(bool is_rx)
        {
            if (_state.UsingLegacyEQ) return;

            if (is_rx && (_state.RX_F == null || _state.RX_G == null || _state.RX_Q == null)) return;
            if (!is_rx && (_state.TX_F == null || _state.TX_G == null || _state.TX_Q == null)) return;

            lock (_dspLock)
            {
                if (is_rx)
                {
                    _tempRX_BandCount = _state.RX_BandCount;
                    _tempRX_F = (double[])_state.RX_F.Clone();
                    _tempRX_G = (double[])_state.RX_G.Clone();
                    _tempRX_Q = (double[])_state.RX_Q.Clone();
                    _pendingRX_Update = true;
                }
                else
                {
                    _tempTX_BandCount = _state.TX_BandCount;
                    _tempTX_F = (double[])_state.TX_F.Clone();
                    _tempTX_G = (double[])_state.TX_G.Clone();
                    _tempTX_Q = (double[])_state.TX_Q.Clone();
                    _pendingTX_Update = true;
                }
            }
        }
        private void dspUpdateTimerTick(object state)
        {
            lock (_dspLock)
            {
                if (_wdspIsBusy) return;

                if (_pendingRX_Update)
                {
                    _pendingRX_Update = false;
                    sendRXDspUpdate();
                    return; // one at a time
                }

                if (_pendingTX_Update)
                {
                    _pendingTX_Update = false;
                    sendTXDspUpdate();
                }
            }
        }

        private void sendRXDspUpdate()
        {
            if (_state.UsingLegacyEQ) return;

            int nfreqs = _tempRX_BandCount;

            double[] F = new double[nfreqs + 1];
            double[] G = new double[nfreqs + 1];
            double[] Q = new double[nfreqs + 1];

            F[0] = 0.0;
            G[0] = _state.RX_Preamp;
            Q[0] = 0.0;

            for (int n = 0; n < nfreqs; n++)
            {
                F[n + 1] = _tempRX_F[n];
                G[n + 1] = _tempRX_G[n];
                Q[n + 1] = _tempRX_Q[n];
            }

            try
            {
                _wdspIsBusy = true;

                unsafe
                {
                    fixed (double* Fptr = &F[0], Gptr = &G[0], Qptr = &Q[0])
                    {
                        WDSP.SetRXAEQProfile(WDSP.id(0, 0), nfreqs, Fptr, Gptr, _state.RX_ParametricEQ ? Qptr : null);
                        WDSP.SetRXAEQProfile(WDSP.id(0, 1), nfreqs, Fptr, Gptr, _state.RX_ParametricEQ ? Qptr : null);
                    }
                }

                _wdspIsBusy = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WDSP RX update error: {ex.Message}");
                _wdspIsBusy = false;
            }
        }

        private void sendTXDspUpdate()
        {
            if (_state.UsingLegacyEQ) return;

            int nfreqs = _tempTX_BandCount;

            double[] F = new double[nfreqs + 1];
            double[] G = new double[nfreqs + 1];
            double[] Q = new double[nfreqs + 1];

            F[0] = 0.0;
            G[0] = _state.TX_Preamp;
            Q[0] = 0.0;

            for (int n = 0; n < nfreqs; n++)
            {
                F[n + 1] = _tempTX_F[n];
                G[n + 1] = _tempTX_G[n];
                Q[n + 1] = _tempTX_Q[n];
            }

            try
            {
                _wdspIsBusy = true;

                unsafe
                {
                    fixed (double* Fptr = &F[0], Gptr = &G[0], Qptr = &Q[0])
                    {
                        WDSP.SetTXAEQProfile(WDSP.id(1, 0), nfreqs, Fptr, Gptr, _state.TX_ParametricEQ ? Qptr : null);
                    }
                }

                _wdspIsBusy = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WDSP TX update error: {ex.Message}");
                _wdspIsBusy = false;
            }
        }

        private void btnParaEQReset_Click(object sender, EventArgs e)
        {
            ucParametricEq1.SelectedIndex = -1;
            ucParametricEq1.GlobalGainDb = 0;
            ucParametricEq1.ResetPoints();
        }

        private void chkParaEQ_enabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            if (_state.UsingLegacyEQ) return;

            if(_state.RXselected)
            {
                _state.RXenabled = chkParaEQ_enabled.Checked;
                console.RXEQ = _state.RXenabled;
                enableRxEq(_state.RXenabled);
            }
            else
            {
                _state.TXenabled = chkParaEQ_enabled.Checked;
                console.TXEQ = _state.TXenabled;
                enableTxEq(_state.TXenabled);
            }
        }

        private void radParaEQ_RX_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            if (!radParaEQ_RX.Checked) return;

            _state.RXselected = chkParaEQ_enabled.Checked;

            chkParaEQ_enabled.Checked = console.RXEQ;
        }

        private void radParaEQ_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            ucParametricEq1.SelectedIndex = -1;

            int bc = -1;
            if (radParaEQ_5.Checked)
            {
                bc = 5;
            }
            else if (radParaEQ_10.Checked)
            {
                bc = 10;
            }
            else if (radParaEQ_18.Checked)
            {
                bc = 18;
            }

            if (bc == -1) return;

            if (_state.RXselected)
            {
                ucParametricEq1.GetDefaults(out _state.RX_F, out _state.RX_G, out _state.RX_Q, out _state.RX_Preamp, out _state.RX_minHz, out _state.RX_maxHz, out _state.RX_ParametricEQ, out _state.RX_BandCount, bc);
                _state.RX_BandCount = bc;
            }
            else
            {
                ucParametricEq1.GetDefaults(out _state.TX_F, out _state.TX_G, out _state.TX_Q, out _state.TX_Preamp, out _state.TX_minHz, out _state.TX_maxHz, out _state.TX_ParametricEQ, out _state.TX_BandCount, bc);
                _state.TX_BandCount = bc;
            }
            nudParaEQ_selected_band.Maximum = bc;
            ucParametricEq1.BandCount = bc;

            setupWDSPdataFromParaEQ(_state.RXselected);            
        }

        private void ucParametricEq1_GlobalGainChanged(object sender, ucParametricEq.EqDraggingEventArgs e)
        {
            if (_initalising) return;

            if (_state.RXselected)
            {
                _state.RX_Preamp = ucParametricEq1.GlobalGainDb;
            }
            else
            {
                _state.TX_Preamp = ucParametricEq1.GlobalGainDb;
            }

            _ignore_update = true;
            nudParaEQ_preamp.Value = (decimal)ucParametricEq1.GlobalGainDb;
            _ignore_update = false;

            if (e.IsDragging && !_state.LiveUpdate) return;

            setupWDSPdataFromParaEQ(_state.RXselected);
        }

        private void ucParametricEq1_PointDataChanged(object sender, ucParametricEq.EqPointDataChangedEventArgs e)
        {
            if (_initalising) return;

            _state.BandSelected = e.Index;
            updateBandData();
        }

        private void ucParametricEq1_PointsChanged(object sender, ucParametricEq.EqDraggingEventArgs e)
        {
            if (_initalising) return;

            if (_state.RXselected)
            {
                ucParametricEq1.GetPointsData(out _state.RX_F, out _state.RX_G, out _state.RX_Q);
            }
            else
            {
                ucParametricEq1.GetPointsData(out _state.TX_F, out _state.TX_G, out _state.TX_Q);
            }

            if (e.IsDragging && !_state.LiveUpdate) return;            

            setupWDSPdataFromParaEQ(_state.RXselected);
        }

        public string ParaEQRXData
        {
            get
            {
                string json = ucParametricEq1.SaveToJsonFromPoints(_state.RX_F, _state.RX_G, _state.RX_Q, _state.RX_Preamp, _state.RX_minHz, _state.RX_maxHz, _state.RX_ParametricEQ);
                string comp = Common.Compress_gzip(json);
                return string.IsNullOrEmpty(comp) ? "" : comp; // returning "" to make the db life easier
            }
            set
            {
                double[] f;
                double[] g;
                double[] q;
                double preamp;
                double min_hz;
                double max_hz;
                bool parametric_eq;
                int band_count;

                string json = Common.Decompress_gzip(value);

                bool ok = ucParametricEq1.PointsFromJson(
                    json,
                    out f,
                    out g,
                    out q,
                    out preamp,
                    out min_hz,
                    out max_hz,
                    out parametric_eq,
                    out band_count);

                if (ok)
                {
                    _state.RX_F = f;
                    _state.RX_G = g;
                    _state.RX_Q = q;
                    _state.RX_Preamp = preamp;
                    _state.RX_minHz = min_hz;
                    _state.RX_maxHz = max_hz;
                    _state.RX_ParametricEQ = parametric_eq;
                    _state.RX_BandCount = band_count;                    
                }
                else
                {
                    ucParametricEq1.GetDefaults(out _state.RX_F, out _state.RX_G, out _state.RX_Q, out _state.RX_Preamp, out _state.RX_minHz, out _state.RX_maxHz, out _state.RX_ParametricEQ, out _state.RX_BandCount);
                }

                if (_state.RXselected) setParaEQData();

                setupWDSPdataFromParaEQ(true);
            }
        }
        public string ParaEQTXData
        {
            get
            {
                string json = ucParametricEq1.SaveToJsonFromPoints(_state.TX_F, _state.TX_G, _state.TX_Q, _state.TX_Preamp, _state.TX_minHz, _state.TX_maxHz, _state.TX_ParametricEQ);
                string comp = Common.Compress_gzip(json);
                return string.IsNullOrEmpty(comp) ? "" : comp; // returning "" to make the db life easier
            }
            set
            {
                double[] f;
                double[] g;
                double[] q;
                double preamp;
                double min_hz;
                double max_hz;
                bool parametric_eq;
                int band_count;

                string json = Common.Decompress_gzip(value);

                bool ok = ucParametricEq1.PointsFromJson(
                    json,
                    out f,
                    out g,
                    out q,
                    out preamp,
                    out min_hz,
                    out max_hz,
                    out parametric_eq,
                    out band_count);

                                
                if (ok)
                {
                    _state.TX_F = f;
                    _state.TX_G = g;
                    _state.TX_Q = q;
                    _state.TX_Preamp = preamp;
                    _state.TX_minHz = min_hz;
                    _state.TX_maxHz = max_hz;
                    _state.TX_ParametricEQ = parametric_eq;
                    _state.TX_BandCount = band_count;                    
                }
                else
                {
                    ucParametricEq1.GetDefaults(out _state.TX_F, out _state.TX_G, out _state.TX_Q, out _state.TX_Preamp, out _state.TX_minHz, out _state.TX_maxHz, out _state.TX_ParametricEQ, out _state.TX_BandCount);
                }

                if (!_state.RXselected) setParaEQData();

                setupWDSPdataFromParaEQ(false);
            }
        }
        private void setParaEQData()
        {
            chkUseQFactors.CheckedChanged -= chkUseQFactors_CheckedChanged;
            ucParametricEq1.PointsChanged -= ucParametricEq1_PointsChanged;
            udParaEQ_low.ValueChanged -= nudParaEQ_low_ValueChanged;
            udParaEQ_high.ValueChanged -= nudParaEQ_high_ValueChanged;
            radParaEQ_5.CheckedChanged -= radParaEQ_CheckedChanged;
            radParaEQ_10.CheckedChanged -= radParaEQ_CheckedChanged;
            radParaEQ_18.CheckedChanged -= radParaEQ_CheckedChanged;
            nudParaEQ_selected_band.ValueChanged -= nudParaEQ_selected_band_ValueChanged;

            if (_state.RXselected)
            {
                ucParametricEq1.ParametricEQ = _state.RX_ParametricEQ;
                ucParametricEq1.BandCount = _state.RX_BandCount;
                ucParametricEq1.GlobalGainDb = _state.RX_Preamp;
                ucParametricEq1.FrequencyMinHz = _state.RX_minHz;
                ucParametricEq1.FrequencyMaxHz = _state.RX_maxHz;
                ucParametricEq1.SetPointsData(_state.RX_F, _state.RX_G, _state.RX_Q);
            }
            else
            {
                ucParametricEq1.ParametricEQ = _state.TX_ParametricEQ;
                ucParametricEq1.BandCount = _state.TX_BandCount;
                ucParametricEq1.GlobalGainDb = _state.TX_Preamp;
                ucParametricEq1.FrequencyMinHz = _state.TX_minHz;
                ucParametricEq1.FrequencyMaxHz = _state.TX_maxHz;
                ucParametricEq1.SetPointsData(_state.TX_F, _state.TX_G, _state.TX_Q);
            }

            if (ucParametricEq1.BandCount == 10)
            {
                radParaEQ_10.Checked = true;
            }
            else if (ucParametricEq1.BandCount == 18)
            {
                radParaEQ_18.Checked = true;
            }
            else
            {
                radParaEQ_5.Checked = true;
            }
            udParaEQ_low.Value = (decimal)ucParametricEq1.FrequencyMinHz;
            udParaEQ_high.Value = (decimal)ucParametricEq1.FrequencyMaxHz;            
            chkUseQFactors.Checked = ucParametricEq1.ParametricEQ;

            nudParaEQ_selected_band.Maximum = ucParametricEq1.BandCount;

            nudParaEQ_selected_band.ValueChanged += nudParaEQ_selected_band_ValueChanged;
            radParaEQ_5.CheckedChanged += radParaEQ_CheckedChanged;
            radParaEQ_10.CheckedChanged += radParaEQ_CheckedChanged;
            radParaEQ_18.CheckedChanged += radParaEQ_CheckedChanged;
            udParaEQ_low.ValueChanged += nudParaEQ_low_ValueChanged;
            udParaEQ_high.ValueChanged += nudParaEQ_high_ValueChanged;
            ucParametricEq1.PointsChanged += ucParametricEq1_PointsChanged;
            chkUseQFactors.CheckedChanged += chkUseQFactors_CheckedChanged;
        }

        private void radParaEQ_RXTX_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            if (radParaEQ_RX.Checked)
            {
                _state.RXselected = true;

                chkParaEQ_enabled.CheckedChanged -= chkParaEQ_enabled_CheckedChanged;
                chkParaEQ_enabled.Checked = console.RXEQ;// _state.RXenabled;
                _state.RXenabled = console.RXEQ;
                chkParaEQ_enabled.CheckedChanged += chkParaEQ_enabled_CheckedChanged;
            }
            else if (radParaEQ_TX.Checked)
            {
                _state.RXselected = false;

                chkParaEQ_enabled.CheckedChanged -= chkParaEQ_enabled_CheckedChanged;
                _state.TXenabled = console.TXEQ;
                chkParaEQ_enabled.Checked = console.TXEQ;// _state.TXenabled;
                chkParaEQ_enabled.CheckedChanged += chkParaEQ_enabled_CheckedChanged;
            }

            ucParametricEq1.SelectedIndex = -1;

            setParaEQData();
            DSPOptionsChanged();
        }

        private void nudParaEQ_selected_band_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            _state.BandSelected = ((int)nudParaEQ_selected_band.Value) - 1;
            ucParametricEq1.SelectedIndex = _state.BandSelected;
            updateBandData();
        }
        private bool _ignore_update = false;
        private void updateBandData()
        {
            if (_state.BandSelected < 0 || _state.BandSelected >= ucParametricEq1.BandCount)
            {
                nudParaEQ_f.Enabled = false;
                nudParaEQ_gain.Enabled = false;
                nudParaEQ_q.Enabled = false;
                return;
            }

            nudParaEQ_f.Enabled = true;
            nudParaEQ_gain.Enabled = true;
            nudParaEQ_q.Enabled = ucParametricEq1.ParametricEQ ? true : false;

            if (nudParaEQ_selected_band.Value != _state.BandSelected + 1)
            {
                nudParaEQ_selected_band.Value = _state.BandSelected + 1;
                return;
            }

            ucParametricEq1.GetPointData(_state.BandSelected, out double f, out double g, out double q);

            _ignore_update = true;
            nudParaEQ_f.Value = (decimal)f;
            nudParaEQ_gain.Value = (decimal)g;
            nudParaEQ_q.Value = ucParametricEq1.ParametricEQ ? (decimal)q : 1;
            _ignore_update = false;
        }

        private void ucParametricEq1_PointSelected(object sender, ucParametricEq.EqPointSelectionChangedEventArgs e)
        {
            if (_initalising) return;

            _state.BandSelected = e.Index;
            updateBandData();
        }

        private void ucParametricEq1_PointUnselected(object sender, ucParametricEq.EqPointSelectionChangedEventArgs e)
        {
            if (_initalising) return;

            _state.BandSelected = -1;
            updateBandData();
        }

        private void nudParaEQ_f_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            if (_ignore_update || _state.BandSelected == -1) return;

            ucParametricEq1.GetPointData(_state.BandSelected, out double f, out double g, out double q);

            f = (double)nudParaEQ_f.Value;

            ucParametricEq1.SetPointData(_state.BandSelected, f, g, q);
        }

        private void nudParaEQ_gain_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            if (_ignore_update || _state.BandSelected == -1) return;

            ucParametricEq1.GetPointData(_state.BandSelected, out double f, out double g, out double q);

            g = (double)nudParaEQ_gain.Value;

            ucParametricEq1.SetPointData(_state.BandSelected, f, g, q);
        }

        private void nudParaEQ_q_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            if (_ignore_update || _state.BandSelected == -1) return;

            ucParametricEq1.GetPointData(_state.BandSelected, out double f, out double g, out double q);

            q = (double)nudParaEQ_q.Value;

            ucParametricEq1.SetPointData(_state.BandSelected, f, g, q);
        }

        private void chkPanaEQ_live_CheckedChanged(object sender, EventArgs e)
        {
            _state.LiveUpdate = chkPanaEQ_live.Checked;
            DSPOptionsChanged();
        }

        private void nudParaEQ_preamp_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (_ignore_update) return;

            double pre = (double)nudParaEQ_preamp.Value;
            ucParametricEq1.GlobalGainDb = pre;

            if (_state.RXselected)
            {
                _state.RX_Preamp = pre;
            }
            else
            {
                _state.TX_Preamp = pre;
            }

            setupWDSPdataFromParaEQ(_state.RXselected);
        }
        public void DSPOptionsChanged()
        {
            bool warningRX = console.radio.GetDSPRX(0, 0).FilterSize > 2048 ||
                console.radio.GetDSPRX(0, 1).FilterSize > 2048;

            bool warningTX = console.radio.GetDSPTX(0).FilterSize > 2048;

            bool warn = (_state.RXselected && warningRX) || (!_state.RXselected && warningTX);

            pbParaEQ_live_warning.Visible = _state.LiveUpdate && warn;
        }

        private void nudParaEQ_low_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            if (udParaEQ_low.Value > udParaEQ_high.Value - 1000)
            {
                udParaEQ_low.Value = udParaEQ_high.Value - 1000;
                return;
            }
            ucParametricEq1.FrequencyMinHz = (int)udParaEQ_low.Value;
            if (_state.RXselected)
            {
                _state.RX_minHz = ucParametricEq1.FrequencyMinHz;
            }
            else
            {
                _state.TX_minHz = ucParametricEq1.FrequencyMinHz;
            }
        }

        private void nudParaEQ_high_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;

            if (udParaEQ_high.Value < udParaEQ_low.Value + 1000)
            {
                udParaEQ_high.Value = udParaEQ_low.Value + 1000;
                return;
            }
            ucParametricEq1.FrequencyMaxHz = (int)udParaEQ_high.Value;
            if (_state.RXselected)
            {
                _state.RX_maxHz = ucParametricEq1.FrequencyMaxHz;
            }
            else
            {
                _state.TX_maxHz = ucParametricEq1.FrequencyMaxHz;
            }
        }
        private void setupLowHigh()
        {
            nudParaEQ_low_ValueChanged(this, EventArgs.Empty);
            nudParaEQ_high_ValueChanged(this, EventArgs.Empty);
        }
        private void UpdateEQEnabled(bool is_rx)
        {
            chkParaEQ_enabled.CheckedChanged -= chkParaEQ_enabled_CheckedChanged;
            if (is_rx)
            {
                if (_state.RXselected)
                {
                    chkParaEQ_enabled.Checked = _state.RXenabled;
                }
                enableRxEq(_state.RXenabled);
            }
            else
            {
                if (!_state.RXselected)
                {
                    chkParaEQ_enabled.Checked = _state.TXenabled;
                }
                enableTxEq(_state.TXenabled);
            }
            chkParaEQ_enabled.CheckedChanged += chkParaEQ_enabled_CheckedChanged;
        }
        private void setupTimer(bool run)
        {
            _dspUpdateTimer?.Dispose();

            if (run)
            {
                _dspUpdateTimer = new System.Threading.Timer(
                    dspUpdateTimerTick,
                    null,
                    100,    // init delay
                    100);   // interval
            }
        }
    }
}