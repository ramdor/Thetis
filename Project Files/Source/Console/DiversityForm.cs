//=================================================================
// DiversityForm.cs
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

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Timers;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Thetis
{
    /// <summary>
    /// Summary description for DiversityForm.
    /// </summary>
    public class DiversityForm : System.Windows.Forms.Form
    {
        //private Point p = new Point(200, 200); //MW0LGE_21c
        //private const double m_SCALEFACTOR = 10.0; //MW0LGE_21f this now applied to udR1 and udR2
        //private Point last_p;
        private Point last_Phase1 = new Point(100, 250);
        private Point last_Phase2 = new Point(100, 100);
        private double angle;
        private double angle_A;
        private double r_A;
        private double steering_angle;
        private double cross_fire;              // phase shift if using cross-fire feed; pi or 0
        private double fine_null;
        private double m_dGainMulti = 1; //MW0LGE_21f

        //        private Color topColor = Color.FromArgb(0, 120, 0);
        //        private Color bottomColor = Color.FromArgb(0, 40, 0);        
        private Color topColor = Color.FromArgb(0, 140, 180);
        private Color bottomColor = Color.FromArgb(0, 40, 80);

        //        private Color lineColor = Color.FromArgb(0, 255, 0);
        private Color lineColor = Color.FromArgb(0, 255, 255);


        private double locked_r = 0.0f;
        private double locked_angle = 0.0f;
        private Console console;

        private System.Windows.Forms.PictureBox picRadar;
        private System.Windows.Forms.CheckBox chkAuto;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.NumericUpDownTS udR;
        private System.Windows.Forms.NumericUpDownTS udAngle;
        private System.Windows.Forms.CheckBox chkEnable;
        private GroupBoxTS panelDivControls;
        private LabelTS labelTS6;
        private ButtonTS btnShift180;
        private ButtonTS btnShiftUp45;
        private GroupBoxTS groupBox_refMerc;
        private RadioButtonTS radioButtonMerc1;
        private RadioButtonTS radioButtonMerc2;
        private LabelTS labelTS3;
        private NumericUpDownTS udR2;
        private NumericUpDownTS udR1;
        private NumericUpDownTS udCalib;
        private LabelTS labelTS5;
        private LabelTS labelTS9;
        private NumericUpDownTS udAngle0;
        private ButtonTS btnShiftDwn45;
        private LabelTS labelDirection;

        private double d_lambda = 0;
        private LabelTS labelTS30;
        private LabelTS labelTS33;
        private LabelTS labelTS40;
        private LabelTS label_d;
        private LabelTS labelTS41;
        private NumericUpDownTS udAntSpacing;
        private CheckBoxTS chkCrossFire;
        private NumericUpDownTS udFineNull;
        private LabelTS labelTS4;
        private GroupBoxTS grpRxSource;
        private RadioButtonTS radRxSourceRx1Rx2;
        private RadioButtonTS radRxSource2;
        private RadioButtonTS radRxSource1;
        private GroupBoxTS groupBoxTS1;
        private CheckBoxTS chkLockR;
        private CheckBoxTS chkLockAngle;
        private CheckBoxTS chkEnableDiversity;
        private bool FormAutoShown;
        System.Timers.Timer AutoHideTimer;                         // times auto hiding of form 
        private LabelTS labelTS1;
        private NumericUpDownTS udGainMulti;
        private CheckBoxTS chkAlwaysOnTop;
        private ToolTip toolTip1;
        private CheckBoxTS chkNoAttLink;
        private CheckBoxTS chkVFOSync;
        private GroupBoxTS groupBoxTS2;
        private ButtonTS btnM8;
        private ButtonTS btnM7;
        private ButtonTS btnM6;
        private ButtonTS btnM5;
        private ButtonTS btnM4;
        private ButtonTS btnM3;
        private ButtonTS btnM2;
        private ButtonTS btnM1;
        private Label label1;
        private TextBoxTS txtMemoryDataHidden;
        private ButtonTS btnShiftDown10;
        private ButtonTS btnShift90;
        private ButtonTS btnShiftUp10;
        private IContainer components;
        private Label label2;
        private bool _initalising;

        public DiversityForm(Console c)
        {
            _initalising = true;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            Common.DoubleBufferAll(this, true);

            setNewNaming(true);

            console = c;

            udR1.Maximum = udGainMulti.Maximum; //[2.10.3.6]MW0LGE these need to be high so that restore form can recover values
            udR2.Maximum = udGainMulti.Maximum; //this is reset by udGainMulti_ValueChanged below

            chkVFOSync.Checked = console.VFOSync;

            //// labelTS2.Visible = false;
            //labelTS30.Visible = false;
            //labelTS40.Visible = false;
            //labelTS41.Visible = false;
            //label_d.Visible = false;
            //udAntSpacing.Visible = false;
            //labelDirection.Visible = false;
            ////            trackBarR1.Visible = false;
            ////            trackBarPhase1.Visible = false;
            //chkLockR.Visible = true;

            Common.RestoreForm(this, "DiversityForm", true);

            //[2.10.3.6]MW0LGE implement memories. A bit of a hack to store all this in a text box, but it is easy with the saveform/restoreform
            try
            {
                _memories = DeserializeStringToObject<memorySettings[]>(txtMemoryDataHidden.Text);
            }
            catch
            {
                initMemories();
            }
            _initalising = false;
            //udFineNull.Value = 0;
            //groupBoxTS1.Visible = false;
            bool oldPhaseLock = chkLockAngle.Checked;
            bool oldGainLock = chkLockR.Checked;
            _initalising = true;
            chkLockAngle.Checked = false;
            chkLockR.Checked = false;
            _initalising = false;
            EventArgs e = EventArgs.Empty;
            udGainMulti_ValueChanged(this, e);
            radRxSource1_CheckedChanged(this, e);
            radRxSource2_CheckedChanged(this, e);
            radRxSourceRx1Rx2_CheckedChanged(this, e);
            radioButtonMerc1_CheckedChanged(this, e);
            radioButtonMerc2_CheckedChanged(this, e);

            _initalising = true;
            chkLockAngle.Checked = oldPhaseLock;
            chkLockR.Checked = oldGainLock;
            _initalising = false;
            chkLockR_CheckedChanged(this, e);
            chkLockAngle_CheckedChanged(this, e);

            WDSP.SetEXTDIVNr(0, 2);
            // console.Diversity2 = true;
            UpdateDiversity();
            chkEnableDiversity_CheckedChanged(this, e);
            // create timer for autohide and attach callback
            AutoHideTimer = new System.Timers.Timer();
            AutoHideTimer.Elapsed += new ElapsedEventHandler(Callback);
            AutoHideTimer.Enabled = false;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                console.Diversity2 = false;
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiversityForm));
            this.picRadar = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnM8 = new System.Windows.Forms.ButtonTS();
            this.btnM7 = new System.Windows.Forms.ButtonTS();
            this.btnM6 = new System.Windows.Forms.ButtonTS();
            this.btnM5 = new System.Windows.Forms.ButtonTS();
            this.btnM4 = new System.Windows.Forms.ButtonTS();
            this.btnM3 = new System.Windows.Forms.ButtonTS();
            this.btnM2 = new System.Windows.Forms.ButtonTS();
            this.btnM1 = new System.Windows.Forms.ButtonTS();
            this.chkVFOSync = new System.Windows.Forms.CheckBoxTS();
            this.chkNoAttLink = new System.Windows.Forms.CheckBoxTS();
            this.groupBoxTS2 = new System.Windows.Forms.GroupBoxTS();
            this.txtMemoryDataHidden = new System.Windows.Forms.TextBoxTS();
            this.labelTS9 = new System.Windows.Forms.LabelTS();
            this.udAngle0 = new System.Windows.Forms.NumericUpDownTS();
            this.chkAuto = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.labelDirection = new System.Windows.Forms.LabelTS();
            this.udCalib = new System.Windows.Forms.NumericUpDownTS();
            this.udAngle = new System.Windows.Forms.NumericUpDownTS();
            this.chkEnable = new System.Windows.Forms.CheckBox();
            this.groupBoxTS1 = new System.Windows.Forms.GroupBoxTS();
            this.labelTS41 = new System.Windows.Forms.LabelTS();
            this.labelTS30 = new System.Windows.Forms.LabelTS();
            this.labelTS40 = new System.Windows.Forms.LabelTS();
            this.label_d = new System.Windows.Forms.LabelTS();
            this.udAntSpacing = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS5 = new System.Windows.Forms.LabelTS();
            this.udR = new System.Windows.Forms.NumericUpDownTS();
            this.panelDivControls = new System.Windows.Forms.GroupBoxTS();
            this.btnShiftDown10 = new System.Windows.Forms.ButtonTS();
            this.btnShift90 = new System.Windows.Forms.ButtonTS();
            this.btnShiftUp10 = new System.Windows.Forms.ButtonTS();
            this.label1 = new System.Windows.Forms.Label();
            this.chkAlwaysOnTop = new System.Windows.Forms.CheckBoxTS();
            this.chkEnableDiversity = new System.Windows.Forms.CheckBoxTS();
            this.grpRxSource = new System.Windows.Forms.GroupBoxTS();
            this.radRxSourceRx1Rx2 = new System.Windows.Forms.RadioButtonTS();
            this.radRxSource2 = new System.Windows.Forms.RadioButtonTS();
            this.radRxSource1 = new System.Windows.Forms.RadioButtonTS();
            this.btnShiftDwn45 = new System.Windows.Forms.ButtonTS();
            this.btnShift180 = new System.Windows.Forms.ButtonTS();
            this.labelTS6 = new System.Windows.Forms.LabelTS();
            this.groupBox_refMerc = new System.Windows.Forms.GroupBoxTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.udGainMulti = new System.Windows.Forms.NumericUpDownTS();
            this.chkLockAngle = new System.Windows.Forms.CheckBoxTS();
            this.chkLockR = new System.Windows.Forms.CheckBoxTS();
            this.chkCrossFire = new System.Windows.Forms.CheckBoxTS();
            this.labelTS4 = new System.Windows.Forms.LabelTS();
            this.labelTS33 = new System.Windows.Forms.LabelTS();
            this.udFineNull = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS3 = new System.Windows.Forms.LabelTS();
            this.udR2 = new System.Windows.Forms.NumericUpDownTS();
            this.udR1 = new System.Windows.Forms.NumericUpDownTS();
            this.radioButtonMerc2 = new System.Windows.Forms.RadioButtonTS();
            this.radioButtonMerc1 = new System.Windows.Forms.RadioButtonTS();
            this.btnShiftUp45 = new System.Windows.Forms.ButtonTS();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picRadar)).BeginInit();
            this.groupBoxTS2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udAngle0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCalib)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udAngle)).BeginInit();
            this.groupBoxTS1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udAntSpacing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udR)).BeginInit();
            this.panelDivControls.SuspendLayout();
            this.grpRxSource.SuspendLayout();
            this.groupBox_refMerc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udGainMulti)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udFineNull)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udR2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udR1)).BeginInit();
            this.SuspendLayout();
            // 
            // picRadar
            // 
            this.picRadar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picRadar.BackColor = System.Drawing.SystemColors.Control;
            this.picRadar.Location = new System.Drawing.Point(4, 272);
            this.picRadar.Name = "picRadar";
            this.picRadar.Size = new System.Drawing.Size(338, 338);
            this.picRadar.TabIndex = 0;
            this.picRadar.TabStop = false;
            this.picRadar.Paint += new System.Windows.Forms.PaintEventHandler(this.picRadar_Paint);
            this.picRadar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picRadar_MouseDown);
            this.picRadar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picRadar_MouseMove);
            this.picRadar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picRadar_MouseUp);
            // 
            // btnM8
            // 
            this.btnM8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnM8.Image = null;
            this.btnM8.Location = new System.Drawing.Point(51, 202);
            this.btnM8.Name = "btnM8";
            this.btnM8.Selectable = true;
            this.btnM8.Size = new System.Drawing.Size(36, 23);
            this.btnM8.TabIndex = 112;
            this.btnM8.Text = "M8";
            this.toolTip1.SetToolTip(this.btnM8, "Memory. Shift click to store. Ctrl click to remove.");
            this.btnM8.UseVisualStyleBackColor = true;
            this.btnM8.Click += new System.EventHandler(this.btnMemory_Click);
            // 
            // btnM7
            // 
            this.btnM7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnM7.Image = null;
            this.btnM7.Location = new System.Drawing.Point(6, 202);
            this.btnM7.Name = "btnM7";
            this.btnM7.Selectable = true;
            this.btnM7.Size = new System.Drawing.Size(36, 23);
            this.btnM7.TabIndex = 111;
            this.btnM7.Text = "M7";
            this.toolTip1.SetToolTip(this.btnM7, "Memory. Shift click to store. Ctrl click to remove.");
            this.btnM7.UseVisualStyleBackColor = true;
            this.btnM7.Click += new System.EventHandler(this.btnMemory_Click);
            // 
            // btnM6
            // 
            this.btnM6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnM6.Image = null;
            this.btnM6.Location = new System.Drawing.Point(51, 177);
            this.btnM6.Name = "btnM6";
            this.btnM6.Selectable = true;
            this.btnM6.Size = new System.Drawing.Size(36, 23);
            this.btnM6.TabIndex = 110;
            this.btnM6.Text = "M6";
            this.toolTip1.SetToolTip(this.btnM6, "Memory. Shift click to store. Ctrl click to remove.");
            this.btnM6.UseVisualStyleBackColor = true;
            this.btnM6.Click += new System.EventHandler(this.btnMemory_Click);
            // 
            // btnM5
            // 
            this.btnM5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnM5.Image = null;
            this.btnM5.Location = new System.Drawing.Point(6, 177);
            this.btnM5.Name = "btnM5";
            this.btnM5.Selectable = true;
            this.btnM5.Size = new System.Drawing.Size(36, 23);
            this.btnM5.TabIndex = 109;
            this.btnM5.Text = "M5";
            this.toolTip1.SetToolTip(this.btnM5, "Memory. Shift click to store. Ctrl click to remove.");
            this.btnM5.UseVisualStyleBackColor = true;
            this.btnM5.Click += new System.EventHandler(this.btnMemory_Click);
            // 
            // btnM4
            // 
            this.btnM4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnM4.Image = null;
            this.btnM4.Location = new System.Drawing.Point(51, 151);
            this.btnM4.Name = "btnM4";
            this.btnM4.Selectable = true;
            this.btnM4.Size = new System.Drawing.Size(36, 23);
            this.btnM4.TabIndex = 108;
            this.btnM4.Text = "M4";
            this.toolTip1.SetToolTip(this.btnM4, "Memory. Shift click to store. Ctrl click to remove.");
            this.btnM4.UseVisualStyleBackColor = true;
            this.btnM4.Click += new System.EventHandler(this.btnMemory_Click);
            // 
            // btnM3
            // 
            this.btnM3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnM3.Image = null;
            this.btnM3.Location = new System.Drawing.Point(6, 151);
            this.btnM3.Name = "btnM3";
            this.btnM3.Selectable = true;
            this.btnM3.Size = new System.Drawing.Size(36, 23);
            this.btnM3.TabIndex = 107;
            this.btnM3.Text = "M3";
            this.toolTip1.SetToolTip(this.btnM3, "Memory. Shift click to store. Ctrl click to remove.");
            this.btnM3.UseVisualStyleBackColor = true;
            this.btnM3.Click += new System.EventHandler(this.btnMemory_Click);
            // 
            // btnM2
            // 
            this.btnM2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnM2.Image = null;
            this.btnM2.Location = new System.Drawing.Point(51, 126);
            this.btnM2.Name = "btnM2";
            this.btnM2.Selectable = true;
            this.btnM2.Size = new System.Drawing.Size(36, 23);
            this.btnM2.TabIndex = 106;
            this.btnM2.Text = "M2";
            this.toolTip1.SetToolTip(this.btnM2, "Memory. Shift click to store. Ctrl click to remove.");
            this.btnM2.UseVisualStyleBackColor = true;
            this.btnM2.Click += new System.EventHandler(this.btnMemory_Click);
            // 
            // btnM1
            // 
            this.btnM1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnM1.Image = null;
            this.btnM1.Location = new System.Drawing.Point(6, 126);
            this.btnM1.Name = "btnM1";
            this.btnM1.Selectable = true;
            this.btnM1.Size = new System.Drawing.Size(36, 23);
            this.btnM1.TabIndex = 105;
            this.btnM1.Text = "M1";
            this.toolTip1.SetToolTip(this.btnM1, "Memory. Shift click to store. Ctrl click to remove.");
            this.btnM1.UseVisualStyleBackColor = true;
            this.btnM1.Click += new System.EventHandler(this.btnMemory_Click);
            // 
            // chkVFOSync
            // 
            this.chkVFOSync.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkVFOSync.BackColor = System.Drawing.SystemColors.Control;
            this.chkVFOSync.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkVFOSync.Image = null;
            this.chkVFOSync.Location = new System.Drawing.Point(221, 67);
            this.chkVFOSync.Name = "chkVFOSync";
            this.chkVFOSync.Size = new System.Drawing.Size(84, 23);
            this.chkVFOSync.TabIndex = 104;
            this.chkVFOSync.Text = "VFO Sync";
            this.chkVFOSync.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.chkVFOSync, "Enable VFO sync");
            this.chkVFOSync.UseVisualStyleBackColor = false;
            this.chkVFOSync.CheckedChanged += new System.EventHandler(this.chkVFOSync_CheckedChanged);
            // 
            // chkNoAttLink
            // 
            this.chkNoAttLink.AutoSize = true;
            this.chkNoAttLink.Image = null;
            this.chkNoAttLink.Location = new System.Drawing.Point(230, 96);
            this.chkNoAttLink.Name = "chkNoAttLink";
            this.chkNoAttLink.Size = new System.Drawing.Size(83, 17);
            this.chkNoAttLink.TabIndex = 103;
            this.chkNoAttLink.Text = "No ATT link";
            this.toolTip1.SetToolTip(this.chkNoAttLink, "Normally if RX1+RX2 are in use the attenuators will be linked. Select this if you" +
        " dont want that to happen.");
            this.chkNoAttLink.UseVisualStyleBackColor = true;
            this.chkNoAttLink.CheckedChanged += new System.EventHandler(this.chkNoAttLink_CheckedChanged);
            // 
            // groupBoxTS2
            // 
            this.groupBoxTS2.Controls.Add(this.txtMemoryDataHidden);
            this.groupBoxTS2.Controls.Add(this.labelTS9);
            this.groupBoxTS2.Controls.Add(this.udAngle0);
            this.groupBoxTS2.Controls.Add(this.chkAuto);
            this.groupBoxTS2.Controls.Add(this.textBox1);
            this.groupBoxTS2.Controls.Add(this.labelDirection);
            this.groupBoxTS2.Controls.Add(this.udCalib);
            this.groupBoxTS2.Controls.Add(this.udAngle);
            this.groupBoxTS2.Controls.Add(this.chkEnable);
            this.groupBoxTS2.Controls.Add(this.groupBoxTS1);
            this.groupBoxTS2.Controls.Add(this.labelTS5);
            this.groupBoxTS2.Controls.Add(this.udR);
            this.groupBoxTS2.Location = new System.Drawing.Point(396, 10);
            this.groupBoxTS2.Name = "groupBoxTS2";
            this.groupBoxTS2.Size = new System.Drawing.Size(315, 256);
            this.groupBoxTS2.TabIndex = 101;
            this.groupBoxTS2.TabStop = false;
            this.groupBoxTS2.Text = "hidden";
            this.groupBoxTS2.Visible = false;
            // 
            // txtMemoryDataHidden
            // 
            this.txtMemoryDataHidden.Location = new System.Drawing.Point(10, 228);
            this.txtMemoryDataHidden.Name = "txtMemoryDataHidden";
            this.txtMemoryDataHidden.Size = new System.Drawing.Size(100, 20);
            this.txtMemoryDataHidden.TabIndex = 107;
            // 
            // labelTS9
            // 
            this.labelTS9.AutoSize = true;
            this.labelTS9.Image = null;
            this.labelTS9.Location = new System.Drawing.Point(24, 21);
            this.labelTS9.Name = "labelTS9";
            this.labelTS9.Size = new System.Drawing.Size(76, 13);
            this.labelTS9.TabIndex = 64;
            this.labelTS9.Text = "Direction (deg)";
            // 
            // udAngle0
            // 
            this.udAngle0.BackColor = System.Drawing.Color.White;
            this.udAngle0.DecimalPlaces = 1;
            this.udAngle0.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udAngle0.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.udAngle0.Location = new System.Drawing.Point(81, 43);
            this.udAngle0.Maximum = new decimal(new int[] {
            361,
            0,
            0,
            0});
            this.udAngle0.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.udAngle0.Name = "udAngle0";
            this.udAngle0.Size = new System.Drawing.Size(74, 32);
            this.udAngle0.TabIndex = 64;
            this.udAngle0.TinyStep = false;
            this.udAngle0.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udAngle0.ValueChanged += new System.EventHandler(this.udAngle0_ValueChanged);
            // 
            // chkAuto
            // 
            this.chkAuto.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.chkAuto.Enabled = false;
            this.chkAuto.Location = new System.Drawing.Point(209, 164);
            this.chkAuto.Name = "chkAuto";
            this.chkAuto.Size = new System.Drawing.Size(48, 24);
            this.chkAuto.TabIndex = 1;
            this.chkAuto.Text = "Auto";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(194, 130);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 4;
            // 
            // labelDirection
            // 
            this.labelDirection.AutoSize = true;
            this.labelDirection.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelDirection.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelDirection.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDirection.Image = null;
            this.labelDirection.Location = new System.Drawing.Point(10, 43);
            this.labelDirection.Name = "labelDirection";
            this.labelDirection.Size = new System.Drawing.Size(63, 33);
            this.labelDirection.TabIndex = 65;
            this.labelDirection.Text = "NW";
            this.labelDirection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // udCalib
            // 
            this.udCalib.DecimalPlaces = 3;
            this.udCalib.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udCalib.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.udCalib.Location = new System.Drawing.Point(13, 189);
            this.udCalib.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.udCalib.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            -2147483648});
            this.udCalib.Name = "udCalib";
            this.udCalib.Size = new System.Drawing.Size(60, 23);
            this.udCalib.TabIndex = 60;
            this.udCalib.TinyStep = false;
            this.udCalib.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udCalib.ValueChanged += new System.EventHandler(this.udCalib_ValueChanged);
            // 
            // udAngle
            // 
            this.udAngle.DecimalPlaces = 3;
            this.udAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udAngle.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.udAngle.Location = new System.Drawing.Point(209, 87);
            this.udAngle.Maximum = new decimal(new int[] {
            65,
            0,
            0,
            65536});
            this.udAngle.Minimum = new decimal(new int[] {
            65,
            0,
            0,
            -2147418112});
            this.udAngle.Name = "udAngle";
            this.udAngle.Size = new System.Drawing.Size(60, 23);
            this.udAngle.TabIndex = 6;
            this.udAngle.TinyStep = false;
            this.udAngle.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udAngle.ValueChanged += new System.EventHandler(this.udTheta_ValueChanged);
            // 
            // chkEnable
            // 
            this.chkEnable.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkEnable.Location = new System.Drawing.Point(221, 39);
            this.chkEnable.Name = "chkEnable";
            this.chkEnable.Size = new System.Drawing.Size(48, 24);
            this.chkEnable.TabIndex = 48;
            this.chkEnable.Text = "Enable";
            this.chkEnable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkEnable.CheckedChanged += new System.EventHandler(this.chkEnable_CheckedChanged);
            // 
            // groupBoxTS1
            // 
            this.groupBoxTS1.Controls.Add(this.labelTS41);
            this.groupBoxTS1.Controls.Add(this.labelTS30);
            this.groupBoxTS1.Controls.Add(this.labelTS40);
            this.groupBoxTS1.Controls.Add(this.label_d);
            this.groupBoxTS1.Controls.Add(this.udAntSpacing);
            this.groupBoxTS1.Location = new System.Drawing.Point(10, 90);
            this.groupBoxTS1.Name = "groupBoxTS1";
            this.groupBoxTS1.Size = new System.Drawing.Size(142, 75);
            this.groupBoxTS1.TabIndex = 100;
            this.groupBoxTS1.TabStop = false;
            this.groupBoxTS1.Text = "Antenna Spacing";
            // 
            // labelTS41
            // 
            this.labelTS41.AutoSize = true;
            this.labelTS41.Image = null;
            this.labelTS41.Location = new System.Drawing.Point(9, 30);
            this.labelTS41.Name = "labelTS41";
            this.labelTS41.Size = new System.Drawing.Size(55, 13);
            this.labelTS41.TabIndex = 98;
            this.labelTS41.Text = "D (meters)";
            // 
            // labelTS30
            // 
            this.labelTS30.AutoSize = true;
            this.labelTS30.Font = new System.Drawing.Font("Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.labelTS30.Image = null;
            this.labelTS30.Location = new System.Drawing.Point(29, 56);
            this.labelTS30.Name = "labelTS30";
            this.labelTS30.Size = new System.Drawing.Size(21, 13);
            this.labelTS30.TabIndex = 87;
            this.labelTS30.Text = "l =";
            // 
            // labelTS40
            // 
            this.labelTS40.AutoSize = true;
            this.labelTS40.Image = null;
            this.labelTS40.Location = new System.Drawing.Point(9, 56);
            this.labelTS40.Name = "labelTS40";
            this.labelTS40.Size = new System.Drawing.Size(23, 13);
            this.labelTS40.TabIndex = 95;
            this.labelTS40.Text = " D/";
            // 
            // label_d
            // 
            this.label_d.AutoSize = true;
            this.label_d.BackColor = System.Drawing.Color.Transparent;
            this.label_d.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_d.Image = null;
            this.label_d.Location = new System.Drawing.Point(52, 56);
            this.label_d.Name = "label_d";
            this.label_d.Size = new System.Drawing.Size(15, 15);
            this.label_d.TabIndex = 96;
            this.label_d.Text = "0";
            // 
            // udAntSpacing
            // 
            this.udAntSpacing.BackColor = System.Drawing.Color.White;
            this.udAntSpacing.DecimalPlaces = 2;
            this.udAntSpacing.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udAntSpacing.ForeColor = System.Drawing.Color.Black;
            this.udAntSpacing.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.udAntSpacing.Location = new System.Drawing.Point(66, 26);
            this.udAntSpacing.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.udAntSpacing.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udAntSpacing.Name = "udAntSpacing";
            this.udAntSpacing.Size = new System.Drawing.Size(60, 23);
            this.udAntSpacing.TabIndex = 97;
            this.udAntSpacing.TinyStep = false;
            this.udAntSpacing.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udAntSpacing.ValueChanged += new System.EventHandler(this.udAntSpacing_ValueChanged_1);
            // 
            // labelTS5
            // 
            this.labelTS5.AutoSize = true;
            this.labelTS5.Image = null;
            this.labelTS5.Location = new System.Drawing.Point(10, 173);
            this.labelTS5.Name = "labelTS5";
            this.labelTS5.Size = new System.Drawing.Size(72, 13);
            this.labelTS5.TabIndex = 61;
            this.labelTS5.Text = "calib direction";
            // 
            // udR
            // 
            this.udR.DecimalPlaces = 3;
            this.udR.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.udR.Location = new System.Drawing.Point(122, 189);
            this.udR.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udR.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udR.Name = "udR";
            this.udR.Size = new System.Drawing.Size(56, 20);
            this.udR.TabIndex = 5;
            this.udR.TinyStep = false;
            this.udR.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udR.ValueChanged += new System.EventHandler(this.udR_ValueChanged);
            // 
            // panelDivControls
            // 
            this.panelDivControls.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panelDivControls.Controls.Add(this.label2);
            this.panelDivControls.Controls.Add(this.btnShiftDown10);
            this.panelDivControls.Controls.Add(this.btnShift90);
            this.panelDivControls.Controls.Add(this.btnShiftUp10);
            this.panelDivControls.Controls.Add(this.label1);
            this.panelDivControls.Controls.Add(this.btnM8);
            this.panelDivControls.Controls.Add(this.btnM7);
            this.panelDivControls.Controls.Add(this.btnM6);
            this.panelDivControls.Controls.Add(this.btnM5);
            this.panelDivControls.Controls.Add(this.btnM4);
            this.panelDivControls.Controls.Add(this.btnM3);
            this.panelDivControls.Controls.Add(this.btnM2);
            this.panelDivControls.Controls.Add(this.btnM1);
            this.panelDivControls.Controls.Add(this.chkVFOSync);
            this.panelDivControls.Controls.Add(this.chkNoAttLink);
            this.panelDivControls.Controls.Add(this.chkAlwaysOnTop);
            this.panelDivControls.Controls.Add(this.chkEnableDiversity);
            this.panelDivControls.Controls.Add(this.grpRxSource);
            this.panelDivControls.Controls.Add(this.btnShiftDwn45);
            this.panelDivControls.Controls.Add(this.btnShift180);
            this.panelDivControls.Controls.Add(this.labelTS6);
            this.panelDivControls.Controls.Add(this.groupBox_refMerc);
            this.panelDivControls.Controls.Add(this.btnShiftUp45);
            this.panelDivControls.ImeMode = System.Windows.Forms.ImeMode.AlphaFull;
            this.panelDivControls.Location = new System.Drawing.Point(10, 4);
            this.panelDivControls.Name = "panelDivControls";
            this.panelDivControls.Size = new System.Drawing.Size(352, 262);
            this.panelDivControls.TabIndex = 51;
            this.panelDivControls.TabStop = false;
            this.panelDivControls.Enter += new System.EventHandler(this.panelDivControls_Enter);
            // 
            // btnShiftDown10
            // 
            this.btnShiftDown10.BackColor = System.Drawing.Color.White;
            this.btnShiftDown10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.btnShiftDown10.Image = null;
            this.btnShiftDown10.Location = new System.Drawing.Point(47, 87);
            this.btnShiftDown10.Name = "btnShiftDown10";
            this.btnShiftDown10.Selectable = true;
            this.btnShiftDown10.Size = new System.Drawing.Size(40, 26);
            this.btnShiftDown10.TabIndex = 116;
            this.btnShiftDown10.Text = "-10";
            this.btnShiftDown10.UseVisualStyleBackColor = false;
            this.btnShiftDown10.Click += new System.EventHandler(this.btnShiftDown10_Click);
            // 
            // btnShift90
            // 
            this.btnShift90.BackColor = System.Drawing.Color.White;
            this.btnShift90.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.btnShift90.Image = null;
            this.btnShift90.Location = new System.Drawing.Point(47, 57);
            this.btnShift90.Name = "btnShift90";
            this.btnShift90.Selectable = true;
            this.btnShift90.Size = new System.Drawing.Size(40, 26);
            this.btnShift90.TabIndex = 114;
            this.btnShift90.Text = "90";
            this.btnShift90.UseVisualStyleBackColor = false;
            this.btnShift90.Click += new System.EventHandler(this.btnShift90_Click);
            // 
            // btnShiftUp10
            // 
            this.btnShiftUp10.BackColor = System.Drawing.Color.White;
            this.btnShiftUp10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.btnShiftUp10.Image = null;
            this.btnShiftUp10.Location = new System.Drawing.Point(47, 27);
            this.btnShiftUp10.Name = "btnShiftUp10";
            this.btnShiftUp10.Selectable = true;
            this.btnShiftUp10.Size = new System.Drawing.Size(40, 26);
            this.btnShiftUp10.TabIndex = 115;
            this.btnShiftUp10.Text = "+10";
            this.btnShiftUp10.UseVisualStyleBackColor = false;
            this.btnShiftUp10.Click += new System.EventHandler(this.btnShiftUp10_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Location = new System.Drawing.Point(2, 228);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 113;
            this.label1.Text = "shift click to store";
            // 
            // chkAlwaysOnTop
            // 
            this.chkAlwaysOnTop.AutoSize = true;
            this.chkAlwaysOnTop.Image = null;
            this.chkAlwaysOnTop.Location = new System.Drawing.Point(215, 13);
            this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            this.chkAlwaysOnTop.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkAlwaysOnTop.Size = new System.Drawing.Size(98, 17);
            this.chkAlwaysOnTop.TabIndex = 102;
            this.chkAlwaysOnTop.Text = "Always On Top";
            this.chkAlwaysOnTop.UseVisualStyleBackColor = true;
            this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(this.chkAlwaysOnTop_CheckedChanged);
            // 
            // chkEnableDiversity
            // 
            this.chkEnableDiversity.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkEnableDiversity.BackColor = System.Drawing.SystemColors.Control;
            this.chkEnableDiversity.Checked = true;
            this.chkEnableDiversity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableDiversity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkEnableDiversity.Image = null;
            this.chkEnableDiversity.Location = new System.Drawing.Point(221, 38);
            this.chkEnableDiversity.Name = "chkEnableDiversity";
            this.chkEnableDiversity.Size = new System.Drawing.Size(84, 23);
            this.chkEnableDiversity.TabIndex = 101;
            this.chkEnableDiversity.Text = "Enabled";
            this.chkEnableDiversity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkEnableDiversity.UseVisualStyleBackColor = false;
            this.chkEnableDiversity.CheckedChanged += new System.EventHandler(this.chkEnableDiversity_CheckedChanged);
            // 
            // grpRxSource
            // 
            this.grpRxSource.Controls.Add(this.radRxSourceRx1Rx2);
            this.grpRxSource.Controls.Add(this.radRxSource2);
            this.grpRxSource.Controls.Add(this.radRxSource1);
            this.grpRxSource.Location = new System.Drawing.Point(93, 19);
            this.grpRxSource.Name = "grpRxSource";
            this.grpRxSource.Size = new System.Drawing.Size(113, 90);
            this.grpRxSource.TabIndex = 52;
            this.grpRxSource.TabStop = false;
            this.grpRxSource.Text = "Receiver Source";
            // 
            // radRxSourceRx1Rx2
            // 
            this.radRxSourceRx1Rx2.AutoSize = true;
            this.radRxSourceRx1Rx2.Checked = true;
            this.radRxSourceRx1Rx2.Image = null;
            this.radRxSourceRx1Rx2.Location = new System.Drawing.Point(6, 22);
            this.radRxSourceRx1Rx2.Name = "radRxSourceRx1Rx2";
            this.radRxSourceRx1Rx2.Size = new System.Drawing.Size(75, 17);
            this.radRxSourceRx1Rx2.TabIndex = 2;
            this.radRxSourceRx1Rx2.TabStop = true;
            this.radRxSourceRx1Rx2.Text = "Rx1 + Rx2";
            this.radRxSourceRx1Rx2.UseVisualStyleBackColor = true;
            this.radRxSourceRx1Rx2.CheckedChanged += new System.EventHandler(this.radRxSourceRx1Rx2_CheckedChanged);
            // 
            // radRxSource2
            // 
            this.radRxSource2.AutoSize = true;
            this.radRxSource2.Image = null;
            this.radRxSource2.Location = new System.Drawing.Point(6, 66);
            this.radRxSource2.Name = "radRxSource2";
            this.radRxSource2.Size = new System.Drawing.Size(77, 17);
            this.radRxSource2.TabIndex = 1;
            this.radRxSource2.Text = "Receiver 2";
            this.radRxSource2.UseVisualStyleBackColor = true;
            this.radRxSource2.CheckedChanged += new System.EventHandler(this.radRxSource2_CheckedChanged);
            // 
            // radRxSource1
            // 
            this.radRxSource1.AutoSize = true;
            this.radRxSource1.Image = null;
            this.radRxSource1.Location = new System.Drawing.Point(6, 43);
            this.radRxSource1.Name = "radRxSource1";
            this.radRxSource1.Size = new System.Drawing.Size(77, 17);
            this.radRxSource1.TabIndex = 0;
            this.radRxSource1.Text = "Receiver 1";
            this.radRxSource1.UseVisualStyleBackColor = true;
            this.radRxSource1.CheckedChanged += new System.EventHandler(this.radRxSource1_CheckedChanged);
            // 
            // btnShiftDwn45
            // 
            this.btnShiftDwn45.BackColor = System.Drawing.Color.White;
            this.btnShiftDwn45.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.btnShiftDwn45.Image = null;
            this.btnShiftDwn45.Location = new System.Drawing.Point(6, 87);
            this.btnShiftDwn45.Name = "btnShiftDwn45";
            this.btnShiftDwn45.Selectable = true;
            this.btnShiftDwn45.Size = new System.Drawing.Size(40, 26);
            this.btnShiftDwn45.TabIndex = 62;
            this.btnShiftDwn45.Text = "-45";
            this.btnShiftDwn45.UseVisualStyleBackColor = false;
            this.btnShiftDwn45.Click += new System.EventHandler(this.btnShiftDwn45_Click);
            // 
            // btnShift180
            // 
            this.btnShift180.BackColor = System.Drawing.Color.White;
            this.btnShift180.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.btnShift180.Image = null;
            this.btnShift180.Location = new System.Drawing.Point(6, 57);
            this.btnShift180.Name = "btnShift180";
            this.btnShift180.Selectable = true;
            this.btnShift180.Size = new System.Drawing.Size(40, 26);
            this.btnShift180.TabIndex = 57;
            this.btnShift180.Text = "180";
            this.btnShift180.UseVisualStyleBackColor = false;
            this.btnShift180.Click += new System.EventHandler(this.btnShift180_Click);
            // 
            // labelTS6
            // 
            this.labelTS6.AutoSize = true;
            this.labelTS6.Image = null;
            this.labelTS6.Location = new System.Drawing.Point(33, 14);
            this.labelTS6.Name = "labelTS6";
            this.labelTS6.Size = new System.Drawing.Size(26, 13);
            this.labelTS6.TabIndex = 54;
            this.labelTS6.Text = "shift";
            // 
            // groupBox_refMerc
            // 
            this.groupBox_refMerc.Controls.Add(this.labelTS1);
            this.groupBox_refMerc.Controls.Add(this.udGainMulti);
            this.groupBox_refMerc.Controls.Add(this.chkLockAngle);
            this.groupBox_refMerc.Controls.Add(this.chkLockR);
            this.groupBox_refMerc.Controls.Add(this.chkCrossFire);
            this.groupBox_refMerc.Controls.Add(this.labelTS4);
            this.groupBox_refMerc.Controls.Add(this.labelTS33);
            this.groupBox_refMerc.Controls.Add(this.udFineNull);
            this.groupBox_refMerc.Controls.Add(this.labelTS3);
            this.groupBox_refMerc.Controls.Add(this.udR2);
            this.groupBox_refMerc.Controls.Add(this.udR1);
            this.groupBox_refMerc.Controls.Add(this.radioButtonMerc2);
            this.groupBox_refMerc.Controls.Add(this.radioButtonMerc1);
            this.groupBox_refMerc.Location = new System.Drawing.Point(93, 115);
            this.groupBox_refMerc.Name = "groupBox_refMerc";
            this.groupBox_refMerc.Size = new System.Drawing.Size(252, 140);
            this.groupBox_refMerc.TabIndex = 59;
            this.groupBox_refMerc.TabStop = false;
            this.groupBox_refMerc.Text = "Reference Source";
            this.groupBox_refMerc.Enter += new System.EventHandler(this.groupBox_refMerc_Enter);
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(20, 115);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(56, 13);
            this.labelTS1.TabIndex = 106;
            this.labelTS1.Text = "Gain multi:";
            // 
            // udGainMulti
            // 
            this.udGainMulti.BackColor = System.Drawing.Color.White;
            this.udGainMulti.DecimalPlaces = 2;
            this.udGainMulti.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udGainMulti.ForeColor = System.Drawing.Color.Black;
            this.udGainMulti.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.udGainMulti.Location = new System.Drawing.Point(82, 111);
            this.udGainMulti.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.udGainMulti.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udGainMulti.Name = "udGainMulti";
            this.udGainMulti.Size = new System.Drawing.Size(56, 23);
            this.udGainMulti.TabIndex = 105;
            this.udGainMulti.TinyStep = false;
            this.udGainMulti.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udGainMulti.ValueChanged += new System.EventHandler(this.udGainMulti_ValueChanged);
            // 
            // chkLockAngle
            // 
            this.chkLockAngle.AutoSize = true;
            this.chkLockAngle.Image = null;
            this.chkLockAngle.Location = new System.Drawing.Point(155, 61);
            this.chkLockAngle.Name = "chkLockAngle";
            this.chkLockAngle.Size = new System.Drawing.Size(83, 17);
            this.chkLockAngle.TabIndex = 104;
            this.chkLockAngle.Text = "Lock Phase";
            this.chkLockAngle.UseVisualStyleBackColor = true;
            this.chkLockAngle.CheckedChanged += new System.EventHandler(this.chkLockAngle_CheckedChanged);
            // 
            // chkLockR
            // 
            this.chkLockR.AutoSize = true;
            this.chkLockR.Image = null;
            this.chkLockR.Location = new System.Drawing.Point(70, 84);
            this.chkLockR.Name = "chkLockR";
            this.chkLockR.Size = new System.Drawing.Size(75, 17);
            this.chkLockR.TabIndex = 103;
            this.chkLockR.Text = "Lock Gain";
            this.chkLockR.UseVisualStyleBackColor = true;
            this.chkLockR.CheckedChanged += new System.EventHandler(this.chkLockR_CheckedChanged);
            // 
            // chkCrossFire
            // 
            this.chkCrossFire.AutoSize = true;
            this.chkCrossFire.Image = null;
            this.chkCrossFire.Location = new System.Drawing.Point(155, 84);
            this.chkCrossFire.Name = "chkCrossFire";
            this.chkCrossFire.Size = new System.Drawing.Size(89, 17);
            this.chkCrossFire.TabIndex = 100;
            this.chkCrossFire.Text = "Enable X-Fire";
            this.chkCrossFire.UseVisualStyleBackColor = true;
            this.chkCrossFire.Visible = false;
            this.chkCrossFire.CheckedChanged += new System.EventHandler(this.chkCrossFire_CheckedChanged);
            // 
            // labelTS4
            // 
            this.labelTS4.AutoSize = true;
            this.labelTS4.Image = null;
            this.labelTS4.Location = new System.Drawing.Point(167, 11);
            this.labelTS4.Name = "labelTS4";
            this.labelTS4.Size = new System.Drawing.Size(37, 13);
            this.labelTS4.TabIndex = 102;
            this.labelTS4.Text = "Phase";
            // 
            // labelTS33
            // 
            this.labelTS33.AutoSize = true;
            this.labelTS33.Image = null;
            this.labelTS33.Location = new System.Drawing.Point(240, -13);
            this.labelTS33.Name = "labelTS33";
            this.labelTS33.Size = new System.Drawing.Size(9, 13);
            this.labelTS33.TabIndex = 93;
            this.labelTS33.Text = "i";
            // 
            // udFineNull
            // 
            this.udFineNull.BackColor = System.Drawing.Color.White;
            this.udFineNull.DecimalPlaces = 2;
            this.udFineNull.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udFineNull.ForeColor = System.Drawing.Color.Black;
            this.udFineNull.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.udFineNull.Location = new System.Drawing.Point(155, 28);
            this.udFineNull.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.udFineNull.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
            this.udFineNull.Name = "udFineNull";
            this.udFineNull.Size = new System.Drawing.Size(67, 23);
            this.udFineNull.TabIndex = 101;
            this.udFineNull.TinyStep = false;
            this.udFineNull.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udFineNull.ValueChanged += new System.EventHandler(this.udFineNull_ValueChanged);
            // 
            // labelTS3
            // 
            this.labelTS3.AutoSize = true;
            this.labelTS3.Image = null;
            this.labelTS3.Location = new System.Drawing.Point(90, 11);
            this.labelTS3.Name = "labelTS3";
            this.labelTS3.Size = new System.Drawing.Size(29, 13);
            this.labelTS3.TabIndex = 51;
            this.labelTS3.Text = "Gain";
            // 
            // udR2
            // 
            this.udR2.BackColor = System.Drawing.Color.White;
            this.udR2.DecimalPlaces = 3;
            this.udR2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udR2.ForeColor = System.Drawing.Color.Black;
            this.udR2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.udR2.Location = new System.Drawing.Point(82, 53);
            this.udR2.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.udR2.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udR2.Name = "udR2";
            this.udR2.Size = new System.Drawing.Size(56, 23);
            this.udR2.TabIndex = 11;
            this.udR2.TinyStep = false;
            this.udR2.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.udR2.ValueChanged += new System.EventHandler(this.udR2_ValueChanged);
            // 
            // udR1
            // 
            this.udR1.BackColor = System.Drawing.Color.White;
            this.udR1.DecimalPlaces = 3;
            this.udR1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udR1.ForeColor = System.Drawing.Color.Black;
            this.udR1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.udR1.Location = new System.Drawing.Point(83, 28);
            this.udR1.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.udR1.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udR1.Name = "udR1";
            this.udR1.Size = new System.Drawing.Size(56, 23);
            this.udR1.TabIndex = 10;
            this.udR1.TinyStep = false;
            this.udR1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udR1.ValueChanged += new System.EventHandler(this.udR1_ValueChanged);
            // 
            // radioButtonMerc2
            // 
            this.radioButtonMerc2.AutoSize = true;
            this.radioButtonMerc2.Image = null;
            this.radioButtonMerc2.Location = new System.Drawing.Point(6, 55);
            this.radioButtonMerc2.Name = "radioButtonMerc2";
            this.radioButtonMerc2.Size = new System.Drawing.Size(77, 17);
            this.radioButtonMerc2.TabIndex = 1;
            this.radioButtonMerc2.Text = "Receiver 2";
            this.radioButtonMerc2.UseVisualStyleBackColor = true;
            this.radioButtonMerc2.CheckedChanged += new System.EventHandler(this.radioButtonMerc2_CheckedChanged);
            // 
            // radioButtonMerc1
            // 
            this.radioButtonMerc1.AutoSize = true;
            this.radioButtonMerc1.Checked = true;
            this.radioButtonMerc1.Image = null;
            this.radioButtonMerc1.Location = new System.Drawing.Point(6, 30);
            this.radioButtonMerc1.Name = "radioButtonMerc1";
            this.radioButtonMerc1.Size = new System.Drawing.Size(77, 17);
            this.radioButtonMerc1.TabIndex = 0;
            this.radioButtonMerc1.TabStop = true;
            this.radioButtonMerc1.Text = "Receiver 1";
            this.radioButtonMerc1.UseVisualStyleBackColor = true;
            this.radioButtonMerc1.CheckedChanged += new System.EventHandler(this.radioButtonMerc1_CheckedChanged);
            // 
            // btnShiftUp45
            // 
            this.btnShiftUp45.BackColor = System.Drawing.Color.White;
            this.btnShiftUp45.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.btnShiftUp45.Image = null;
            this.btnShiftUp45.Location = new System.Drawing.Point(6, 27);
            this.btnShiftUp45.Name = "btnShiftUp45";
            this.btnShiftUp45.Selectable = true;
            this.btnShiftUp45.Size = new System.Drawing.Size(40, 26);
            this.btnShiftUp45.TabIndex = 58;
            this.btnShiftUp45.Text = "+45";
            this.btnShiftUp45.UseVisualStyleBackColor = false;
            this.btnShiftUp45.Click += new System.EventHandler(this.btnShiftUp45_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.Location = new System.Drawing.Point(5, 244);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 117;
            this.label2.Text = "ctrl click to clear";
            // 
            // DiversityForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(344, 612);
            this.Controls.Add(this.groupBoxTS2);
            this.Controls.Add(this.picRadar);
            this.Controls.Add(this.panelDivControls);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(360, 500);
            this.Name = "DiversityForm";
            this.Text = "Phasing Control";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.DiversityForm_Closing);
            this.Load += new System.EventHandler(this.DiversityForm_Load);
            this.Resize += new System.EventHandler(this.DiversityForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picRadar)).EndInit();
            this.groupBoxTS2.ResumeLayout(false);
            this.groupBoxTS2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udAngle0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCalib)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udAngle)).EndInit();
            this.groupBoxTS1.ResumeLayout(false);
            this.groupBoxTS1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udAntSpacing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udR)).EndInit();
            this.panelDivControls.ResumeLayout(false);
            this.panelDivControls.PerformLayout();
            this.grpRxSource.ResumeLayout(false);
            this.grpRxSource.PerformLayout();
            this.groupBox_refMerc.ResumeLayout(false);
            this.groupBox_refMerc.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udGainMulti)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udFineNull)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udR2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udR1)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        [Serializable]
        private class memorySettings
        {
            public bool enabled = false;
            public int receiverSource = 0; // 0 = 1+2, 1=1, 2=2
            public bool rx1RefSource = true; // false is rx2
            public double rx1Gain = 1.0f;
            public double rx2Gain = 1.0f;
            public double phase = 0.0f;
            public bool lockPhase = false;
            public bool lockGain = false;
            public double gainMulti = 1.0f;
            public double angle = 0.0f;
            public double r = 0.0f;
        }
        public string SerializeObjectToString<T>(T obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, obj);
                string tmp = Convert.ToBase64String(memoryStream.ToArray());
                tmp = tmp.Replace("/", "[backslash]"); // to fix issue with SaveForm/RestoreForm
                return tmp;
            }
        }

        public T DeserializeStringToObject<T>(string str)
        {
            str = str.Replace("[backslash]", "/"); // to fix issue with SaveForm/RestoreForm
            byte[] bytes = Convert.FromBase64String(str);
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
        private memorySettings[] _memories = new memorySettings[8]; //0-7
        private void initMemories()
        {
            for (int i = 0; i < _memories.Length; i++)
            {
                _memories[i] = new memorySettings();
            }
        }
        private Font _font = new Font("Microsoft Sans Serif", 10.0f, FontStyle.Bold);
        private void picRadar_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int size = Math.Min(picRadar.Width, picRadar.Height);
            Pen pen = new Pen(lineColor);
            Pen pen3 = new Pen(Brushes.White);
            // set a couple of graphics properties to make the
            // output image look nice
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.Bicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            // draw the background of the radar
            g.FillEllipse(new LinearGradientBrush(new Point((int)(size / 2), 0), new Point((int)(size / 2), size - 1), topColor, bottomColor), 0, 0, size - 1, size - 1);
            // draw the outer ring (0° elevation)
            g.DrawEllipse(pen, 0, 0, size - 1, size - 1);
            // draw the inner ring (60° elevation)
            int interval = size / 2;
            // draw the middle ring 
            g.DrawEllipse(pen, (size - interval) / 2, (size - interval) / 2, interval, interval);
            // draw the x and y axis lines
            g.DrawLine(pen, new Point(0, (int)(size / 2)), new Point(size - 1, (int)(size / 2)));
            g.DrawLine(pen, new Point((int)(size / 2), 0), new Point((int)(size / 2), size - 1));

            Point pp;
            for (int i = 0; i < _memories.Length; i++)
            {
                if (_memories[i].enabled)
                {
                    pp = PolarToXY(_memories[i].r, -_memories[i].angle);
                    g.FillEllipse(Brushes.Orange, pp.X - 4, pp.Y - 4, 8, 8);
                    g.DrawString((i + 1).ToString(), _font, Brushes.Orange, new PointF(pp.X + 2, pp.Y + 2));
                }
            }

            pp = getControlHandlePoint(); // MW0LGE_21c
            g.FillEllipse(Brushes.White, pp.X - 4, pp.Y - 4, 8, 8);
            g.DrawLine(pen3, new Point((int)(size / 2), (int)(size / 2)), new Point(pp.X, pp.Y));

            //code store
            //int i;
            //Graphics g = e.Graphics;
            //int size = Math.Min(picRadar.Width, picRadar.Height);
            //Pen pen = new Pen(lineColor);
            //Pen pen2 = new Pen(Brushes.Yellow);
            //Pen pen3 = new Pen(Brushes.White);
            //// set a couple of graphics properties to make the
            //// output image look nice
            //g.CompositingQuality = CompositingQuality.HighQuality;
            //g.InterpolationMode = InterpolationMode.Bicubic;
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.TextRenderingHint = TextRenderingHint.AntiAlias;
            //// draw the background of the radar
            //g.FillEllipse(new LinearGradientBrush(new Point((int)(size / 2), 0), new Point((int)(size / 2), size - 1), topColor, bottomColor), 0, 0, size - 1, size - 1);
            //// draw the outer ring (0° elevation)
            //g.DrawEllipse(pen, 0, 0, size - 1, size - 1);
            //// draw the inner ring (60° elevation)
            ////int interval = size / 3;
            ////g.DrawEllipse(pen, (size - interval) / 2, (size - interval) / 2, interval, interval);
            //// draw the middle ring (30° elevation)
            ////interval *= 2;
            ////g.DrawEllipse(pen, (size - interval) / 2, (size - interval) / 2, interval, interval);
            //int interval = size / 2;
            //// draw the middle ring 
            //g.DrawEllipse(pen, (size - interval) / 2, (size - interval) / 2, interval, interval);
            //// draw the x and y axis lines
            //g.DrawLine(pen, new Point(0, (int)(size / 2)), new Point(size - 1, (int)(size / 2)));
            //g.DrawLine(pen, new Point((int)(size / 2), 0), new Point((int)(size / 2), size - 1));
            //// calculate and draw the antenna sensitivity pattern
            ////double power_max = 0;
            ///*for (i = 0; i < 180; i++)
            //{
            //    double theta_a = (double)i * 2 * Math.PI / 180;  //a point every 2 degrees around the compass
            //    double power = CalcVrms(theta_a, -angle);
            //    //Vrms = Vrms * 1.1 / 3; //* (size / 2) / 392;
            //    //if (power > power_max) power_max = power;
            //    //label_d.Visible = true;
            //    //label_d.Text = Vrms_max.ToString();
            //    //double test = 3 * power;
            //    //if (i == 90) label_d.Text = test.ToString();
            //    Point J = PolarToXY(power / 10, -theta_a);
            //    g.FillEllipse(Brushes.LightBlue, J.X, J.Y, 3, 3);
            //}
            //*/
            ////g.FillEllipse(Brushes.White, p.X - 6, p.Y - 6, 12, 12);
            ////g.DrawLine(pen2, new Point((int)(size / 2), (int)(size / 2)), new Point(p.X, p.Y));
            ///*            if (radioButtonMerc1.Checked == true)
            //            {
            //                g.FillEllipse(Brushes.White, p.X - 4, p.Y - 4, 8, 8);
            //                g.DrawLine(pen3, new Point((int)(size / 2), (int)(size / 2)), new Point(p.X, p.Y));
            //            }
            //            if (radioButtonMerc2.Checked == true)
            //            {
            //                g.FillEllipse(Brushes.White, p.X - 4, p.Y - 2, 8, 8);
            //                g.DrawLine(pen3, new Point((int)(size / 2), (int)(size / 2)), new Point(p.X, p.Y));
            //            }
            //*/
            //Point pp = getControlHandlePoint(); // MW0LGE_21c
            ////g.FillEllipse(Brushes.White, p.X - 4, p.Y - 2, 8, 8);
            ////g.DrawLine(pen3, new Point((int)(size / 2), (int)(size / 2)), new Point(p.X, p.Y));
            //g.FillEllipse(Brushes.White, pp.X - 4, pp.Y - 2, 8, 8);
            //g.DrawLine(pen3, new Point((int)(size / 2), (int)(size / 2)), new Point(pp.X, pp.Y));

            ////g.FillEllipse(Brushes.Yellow, p.X - 4, p.Y - 4, 8, 8);
            ////g.DrawLine(pen2, new Point ((int) (size/2), (int) (size/2)), new Point(p.X, p.Y));
        }

        private Point getControlHandlePoint()
        {
            Point retP;
            double r = (double)udR.Value;
            double a = (double)udAngle.Value;

            if (chkLockR.Checked && chkLockAngle.Checked)
            {
                retP = PolarToXY(locked_r, -locked_angle);
            }
            else if (chkLockR.Checked)
            {
                retP = PolarToXY(locked_r, -a);
            }
            else if (chkLockAngle.Checked)
            {
                retP = PolarToXY(r, -locked_angle);
            }
            else
            {
                retP = PolarToXY(r, -a);
            }

            return retP;
        }

        private Point PolarToXY(double r, double angle)
        {
            int L = (int)Math.Min(picRadar.Width, picRadar.Height);
            if (r > 1.0) r = 1.0;
            return new Point((int)(r * Math.Cos(angle) * L / 2) + L / 2, (int)(r * Math.Sin(angle) * L / 2) + L / 2);
        }

        private void picRadar_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_initalising) return;
            // if(!mouse_down) return;

            int W = picRadar.Width;
            int H = picRadar.Height;
            int L = (int)Math.Min(W, H);

            // get coords relative to middle
            int x = e.X - L / 2;
            int y = e.Y - L / 2;

            // change coordinate space form pixels to percentage of half width
            double xf = (double)x / (L / 2);
            double yf = -(double)y / (L / 2);

            double r = Math.Min(Math.Sqrt(Math.Pow(xf, 2.0) + Math.Pow(yf, 2.0)), 2.0);
            if (r > 1.0) r = 1.0;   // don't let the phasing magnitude be greater than the radar display boundary

            //r = 1.0; // fix magnitude at 1
            angle = Math.Atan2(yf, xf);
            //Debug.WriteLine("xf: "+xf.ToString("f2")+"  yf: "+yf.ToString("f2")+"  r: " + r.ToString("f4") + "  angle: " + angle.ToString("f4"));

            if (mouse_down)
            {
                if (chkLockR.Checked && chkLockAngle.Checked) return;
                if (chkLockR.Checked)
                {
                    //p = PolarToXY(locked_r, -angle); //MW0LGE_21c
                    locked_angle = angle;
                    udR.Value = (decimal)locked_r;
                    udAngle0.Value = (decimal)ConvertAngleToAngle0(angle);
                    udAngle.Value = (decimal)angle;
                    //udFineNull.Value = (decimal)angle;
                    udFineNull.Value = (decimal)(180 * angle / Math.PI); //degrees
                }
                else if (chkLockAngle.Checked)
                {
                    //p = PolarToXY(r, -locked_angle); //MW0LGE_21c
                    locked_r = r;
                    udR.Value = (decimal)r;
                    if (radioButtonMerc1.Checked) udR2.Value = (decimal)Math.Round(udR.Value * (decimal)m_dGainMulti, 3);
                    else udR1.Value = (decimal)Math.Round(udR.Value * (decimal)m_dGainMulti, 3);
                    udAngle0.Value = (decimal)ConvertAngleToAngle0(angle);
                    udAngle.Value = (decimal)locked_angle;
                    //udFineNull.Value = (decimal)locked_angle;
                    udFineNull.Value = (decimal)(180 * locked_angle / Math.PI); //degrees
                }
                else
                {
                    //p = new Point(e.X, e.Y);
                    //p = PolarToXY(r, -angle); //MW0LGE_21c
                    locked_r = r;
                    //Debug.WriteLine("locked_r: " + r.ToString("f4"));
                    if (radioButtonMerc1.Checked)
                    {
                        locked_angle = angle;
                        udR.Value = (decimal)r;
                        udR2.Value = (decimal)Math.Round(udR.Value * (decimal)m_dGainMulti, 3);
                        udAngle.Value = (decimal)angle;
                        //udFineNull.Value = udAngle.Value;
                        udFineNull.Value = (decimal)(180 * (double)udAngle.Value / Math.PI); //degrees
                        decimal deg_val = (decimal)ConvertAngleToAngle0(angle);
                        if (deg_val >= 360) deg_val = 0;
                        if (deg_val <= -1) deg_val = 359;
                        udAngle0.Value = deg_val;
                    }
                    if (radioButtonMerc2.Checked)
                    {
                        locked_angle = angle;
                        udR.Value = (decimal)r;
                        udR1.Value = (decimal)Math.Round(udR.Value * (decimal)m_dGainMulti, 3);
                        udAngle.Value = (decimal)angle;
                        //udFineNull.Value = udAngle.Value;
                        udFineNull.Value = (decimal)(180 * (double)udAngle.Value / Math.PI); //degrees
                        decimal deg_val = (decimal)ConvertAngleToAngle0(angle);
                        if (deg_val >= 360) deg_val = 0;
                        if (deg_val <= -1) deg_val = 359;
                        udAngle0.Value = deg_val;
                        //add Merc2 ref stuff
                    }
                }
                //UpdateDiversity();
                //last_p = p;
                picRadar.Invalidate();
            }
        }

        private bool mouse_down = false;

        private void picRadar_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouse_down = true;
            picRadar_MouseMove(sender, e);
        }

        private void picRadar_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouse_down = false;
        }

        private void udR_ValueChanged(object sender, System.EventArgs e)
        {
            if (_initalising) return;
            UpdateDiversity();
        }

        private void udTheta_ValueChanged(object sender, System.EventArgs e)
        {
            if (_initalising) return;
            UpdateDiversity();
        }

        private void UpdateDirection()
        {
            decimal direction = udAngle0.Value;
            if ((direction > 337 & direction <= 360) | (direction >= 0 & direction <= 22))
            {
                labelDirection.Text = "N";
                return;
            }
            if (direction > 22 & direction <= 67)
            {
                labelDirection.Text = "NE";
                return;
            }
            if (direction > 67 & direction <= 112)
            {
                labelDirection.Text = "E";
                return;
            }
            if (direction > 112 & direction < 157)
            {
                labelDirection.Text = "SE";
                return;
            }
            if (direction > 157 & direction <= 202)
            {
                labelDirection.Text = "S";
                return;
            }
            if (direction > 202 & direction <= 247)
            {
                labelDirection.Text = "SW";
                return;
            }
            if (direction > 247 & direction <= 292)
            {
                labelDirection.Text = "W";
                return;
            }
            if (direction > 292 & direction <= 337)
            {
                labelDirection.Text = "NW";
                return;
            }
        }

        private unsafe void UpdateDiversity()
        {
            double c = 2.9979E8;                                //speed of light
            double pi = Math.PI;
            double lambda;
            UpdateDirection();                                  //update front panel coarse-direction indicator
            double r = (double)udR.Value;
            if (r > 1.0) r = 1.0;
            double angle = (double)(udAngle.Value);
            double cal_angle = (double)udCalib.Value;   //add in direction-offset calibration
            lambda = c / (console.VFOAFreq * 1E6);              //convert freq from MHz to Hz
            d_lambda = (double)udAntSpacing.Value / lambda;     //antenna spacing to wavelength ratio
            label_d.Text = d_lambda.ToString("0.00");
            double[] Irotate = new double[2];
            double[] Qrotate = new double[2];
            if (radioButtonMerc1.Checked)
            {
                //calculate phase shift for IQ stream 2 based on antenna spacing, frequency, steering angle, and apply a 180 degree phase
                //shift for "cross-fire" feed that produces a frequency-independent null in the antenna sensitivity pattern
                angle_A = cross_fire + fine_null + 2 * pi * d_lambda * Math.Cos(angle + cal_angle);
                r_A = (double)(udR2.Value);
                double a_A = (double)(r_A * Math.Cos(angle_A));
                double b_A = (double)(r_A * Math.Sin(angle_A));
                //JanusAudio.SetIQ_RotateA(a_A, b_A);
                // console.SetIQ_RotateA(a_A, b_A);   
                // Audio.I_RotateA = a_A;
                // Audio.Q_RotateA = b_A;
                Irotate[0] = 1.0;
                Qrotate[0] = 0.0;
                Irotate[1] = a_A;
                Qrotate[1] = b_A;
                fixed (double* Iptr = &Irotate[0])
                fixed (double* Qptr = &Qrotate[0])
                    WDSP.SetEXTDIVRotate(0, 2, Iptr, Qptr);
            }
            if (radioButtonMerc2.Checked)
            {
                angle_A = pi + fine_null + 2 * pi * d_lambda * Math.Cos(angle + cal_angle);
                r_A = (double)(udR1.Value);
                double a_A = (double)(r_A * Math.Cos(angle_A));
                double b_A = (double)(r_A * Math.Sin(angle_A));
                //JanusAudio.SetIQ_RotateA(a_A, b_A);
                // console.SetIQ_RotateA(a_A, b_A); 
                // Audio.I_RotateA = a_A;
                // Audio.Q_RotateA = b_A;
                Irotate[1] = 1.0;
                Qrotate[1] = 0.0;
                Irotate[0] = a_A;
                Qrotate[0] = b_A;
                fixed (double* Iptr = &Irotate[0])
                fixed (double* Qptr = &Qrotate[0])
                    WDSP.SetEXTDIVRotate(0, 2, Iptr, Qptr);
            }


            //DttSP.SetDiversityScalar((float)((r*1.5)*Math.Cos(angle)), (float)((r*1.5)*Math.Sin(angle)));

            //MW0LGE_21c
            //int L = (int)Math.Min(picRadar.Width, picRadar.Height);
            //p = new Point((int)(r * L / 2 * Math.Cos(angle)) + L / 2, -(int)(r * L / 2 * Math.Sin(angle)) + L / 2);

            picRadar.Invalidate();
        }

        private void chkEnable_CheckedChanged(object sender, System.EventArgs e)
        {
            if (_initalising) return;
            if (chkEnable.Checked) chkEnable.BackColor = console.ButtonSelectedColor;
            else chkEnable.BackColor = SystemColors.Control;
            if (chkEnable.Checked)
            {
                if (!console.RX2Enabled)
                    console.RX2Enabled = true;
            }
            //DttSP.SetDiversity(Convert.ToInt16(chkEnable.Checked));
        }

        private void DiversityForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            txtMemoryDataHidden.Text = SerializeObjectToString<memorySettings[]>(_memories);

            Common.SaveForm(this, "DiversityForm");
        }

        private void btnShiftUp45_Click(object sender, EventArgs e) //shifts -45 degrees wrt the compass angle
        {
            stepAngle(45);
            //double PI = Math.PI;
            //if (chkLockAngle.Checked) return;
            //if (udAngle.Value >= (decimal)0)
            //{
            //    if (udAngle.Value >= (decimal)(3 * PI / 4))
            //    {
            //        udAngle.Value = udAngle.Value - (decimal)(7 * PI / 4);
            //        udAngle0.Value = (decimal)ConvertAngleToAngle0((double)udAngle.Value);
            //        //udFineNull.Value = udAngle.Value;
            //        udFineNull.Value = (decimal)(180 * (double)udAngle.Value / Math.PI); //degrees
            //        UpdateDiversity();
            //        return;
            //    }
            //    else
            //    {
            //        udAngle.Value += (decimal)PI / 4;
            //        udAngle0.Value = (decimal)ConvertAngleToAngle0((double)udAngle.Value);
            //        //udFineNull.Value = udAngle.Value;
            //        udFineNull.Value = (decimal)(180 * (double)udAngle.Value / Math.PI); //degrees
            //        UpdateDiversity();
            //        return;
            //    }
            //}
            //else
            //{ // angle is negative
            //    udAngle.Value += (decimal)PI / 4;
            //    udAngle0.Value = (decimal)ConvertAngleToAngle0((double)udAngle.Value);
            //    //udFineNull.Value = udAngle.Value;
            //    udFineNull.Value = (decimal)(180 * (double)udAngle.Value / Math.PI); //degrees
            //    UpdateDiversity();
            //    return;
            //}
        }

        private void btnShift180_Click(object sender, EventArgs e)
        {
            stepAngle(180);
            //double PI = Math.PI;
            //if (chkLockAngle.Checked) return;
            //if (udAngle.Value > (decimal)0) udAngle.Value -= (decimal)PI;
            //else udAngle.Value += (decimal)PI;
            //udAngle0.Value = (decimal)ConvertAngleToAngle0((double)udAngle.Value);
            ////udFineNull.Value = udAngle.Value;
            //udFineNull.Value = (decimal)(180 * (double)udAngle.Value / Math.PI); //degrees
            //UpdateDiversity();
        }
        private void btnShiftDwn45_Click(object sender, EventArgs e) //shifts +45 degrees wrt compass angle
        {
            stepAngle(-45);
            //double PI = Math.PI;
            //if (chkLockAngle.Checked) return;
            //if (udAngle.Value >= (decimal)0)
            //{
            //    udAngle.Value = udAngle.Value - (decimal)(PI / 4);
            //    udAngle0.Value = (decimal)ConvertAngleToAngle0((double)udAngle.Value);
            //    //udFineNull.Value = udAngle.Value;
            //    udFineNull.Value = (decimal)(180 * (double)udAngle.Value / Math.PI); //degrees
            //    UpdateDiversity();
            //    return;
            //}
            //else
            //{ // angle is negative
            //    if (udAngle.Value <= -(decimal)(3 * PI / 4))
            //    {
            //        udAngle.Value = udAngle.Value + (decimal)(7 * PI / 4);
            //        udAngle0.Value = (decimal)ConvertAngleToAngle0((double)udAngle.Value);
            //        //udFineNull.Value = udAngle.Value;
            //        udFineNull.Value = (decimal)(180 * (double)udAngle.Value / Math.PI); //degrees
            //        UpdateDiversity();
            //        return;
            //    }
            //    else
            //    {
            //        udAngle.Value -= (decimal)(PI / 4);
            //        udAngle0.Value = (decimal)ConvertAngleToAngle0((double)udAngle.Value);
            //        //udFineNull.Value = udAngle.Value;
            //        udFineNull.Value = (decimal)(180 * (double)udAngle.Value / Math.PI); //degrees
            //        UpdateDiversity();
            //        return;
            //    }
            //}
        }

        private void radioButtonMerc1_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (radioButtonMerc1.Checked)
            {
                udR1.Visible = false;
                udR2.Visible = true;
                updateR2 = false;

                udR2_ValueChanged(this, EventArgs.Empty);
                //  chkLockR.Location = new System.Drawing.Point(199, 81);
                //  chkLockR.BackColor = System.Drawing.SystemColors.Control;
                udFineNull.BackColor = System.Drawing.Color.White;
                chkLockAngle.BackColor = System.Drawing.SystemColors.Control;
                if (chkLockR.Checked) udR1.Value = udR.Value;
                // JanusAudio.SetrefMerc(1);
                // Audio.RefMerc = 1;
                //p = PolarToXY((double)udR.Value, -(double)ConvertAngle0ToAngle((double)udAngle0.Value));
                UpdateDiversity();
                //btnShift180.PerformClick();
            }
            console.DiversityRXRef = radioButtonMerc1.Checked;
        }

        private void radioButtonMerc2_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (radioButtonMerc2.Checked)
            {
                udR1.Visible = true;
                udR2.Visible = false;
                updateR1 = false;
                udR1_ValueChanged(this, EventArgs.Empty);
                // chkLockR.Location = new System.Drawing.Point(199, 52);
                //chkLockR.BackColor = System.Drawing.Color.Yellow;
                //udFineNull.BackColor = System.Drawing.Color.Yellow;
                //chkLockAngle.BackColor = System.Drawing.Color.Yellow;
                //JanusAudio.SetrefMerc(2);
                // Audio.RefMerc = 2;
                //p = PolarToXY((double)udR.Value, -(double)ConvertAngle0ToAngle((double)udAngle0.Value));
                UpdateDiversity();
            }
        }

        private void chkLockAngle_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            locked_angle = (double)udAngle.Value;
        }

        private void chkLockR_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            locked_r = (double)udR.Value;
        }

        private void groupBox_refMerc_Enter(object sender, EventArgs e)
        {

        }
        private void groupBox_udPhase_Enter(object sender, EventArgs e)
        {

        }

        private bool updateR2 = true;
        private void udR2_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (radioButtonMerc2.Checked) return; // uses R1 when ref is receiver 2

            //  if (radioButtonMerc1.Checked)
            // {
            if (chkLockR.Checked)
            {
                r_A = locked_r;
                if (udR2.Value != (decimal)locked_r) udR2.Value = (decimal)Math.Round(locked_r * m_dGainMulti, 3); // MW0LGE_21b only assign if different
                                                                                                                   // fix endless event loop and crash
            }
            udR.Value = (decimal)Math.Round(udR2.Value / (decimal)m_dGainMulti, 3); //MW0LGE_21c;
            UpdateDiversity();

            if (updateR2)
            {
                switch (console.RX1Band)
                {
                    case Band.B160M:
                        console.DiversityR2Gain160m = udR2.Value;
                        break;
                    case Band.B80M:
                        console.DiversityR2Gain80m = udR2.Value;
                        break;
                    case Band.B60M:
                        console.DiversityR2Gain60m = udR2.Value;
                        break;
                    case Band.B40M:
                        console.DiversityR2Gain40m = udR2.Value;
                        break;
                    case Band.B30M:
                        console.DiversityR2Gain30m = udR2.Value;
                        break;
                    case Band.B20M:
                        console.DiversityR2Gain20m = udR2.Value;
                        break;
                    case Band.B17M:
                        console.DiversityR2Gain17m = udR2.Value;
                        break;
                    case Band.B15M:
                        console.DiversityR2Gain15m = udR2.Value;
                        break;
                    case Band.B12M:
                        console.DiversityR2Gain12m = udR2.Value;
                        break;
                    case Band.B10M:
                        console.DiversityR2Gain10m = udR2.Value;
                        break;
                    case Band.B6M:
                        console.DiversityR2Gain6m = udR2.Value;
                        break;
                    case Band.WWV:
                        console.DiversityR2GainWWV = udR2.Value;
                        break;
                    case Band.GEN:
                        console.DiversityR2GainGEN = udR2.Value;
                        break;
                    default:
                        console.DiversityR2GainXVTR = udR2.Value;
                        break;
                }
            }
            updateR2 = true;

            //  }
        }

        private bool updateR1 = true;
        private void udR1_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (radioButtonMerc1.Checked) return; // uses R2 when ref is receiver 1

            //  if (radioButtonMerc2.Checked)
            //  {
            if (chkLockR.Checked)
            {
                r_A = locked_r;
                if ((decimal)locked_r != udR1.Value) udR1.Value = (decimal)Math.Round(locked_r * m_dGainMulti, 3); // MW0LGE_21b only assign if different
                                                                                                                   // fix endless event loop and crash
            }
            udR.Value = (decimal)Math.Round(udR1.Value / (decimal)m_dGainMulti, 3); //MW0LGE_21c
            UpdateDiversity();

            if (updateR1)
            {
                switch (console.RX1Band)
                {
                    case Band.B160M:
                        console.DiversityGain160m = udR1.Value;
                        break;
                    case Band.B80M:
                        console.DiversityGain80m = udR1.Value;
                        break;
                    case Band.B60M:
                        console.DiversityGain60m = udR1.Value;
                        break;
                    case Band.B40M:
                        console.DiversityGain40m = udR1.Value;
                        break;
                    case Band.B30M:
                        console.DiversityGain30m = udR1.Value;
                        break;
                    case Band.B20M:
                        console.DiversityGain20m = udR1.Value;
                        break;
                    case Band.B17M:
                        console.DiversityGain17m = udR1.Value;
                        break;
                    case Band.B15M:
                        console.DiversityGain15m = udR1.Value;
                        break;
                    case Band.B12M:
                        console.DiversityGain12m = udR1.Value;
                        break;
                    case Band.B10M:
                        console.DiversityGain10m = udR1.Value;
                        break;
                    case Band.B6M:
                        console.DiversityGain6m = udR1.Value;
                        break;
                    case Band.WWV:
                        console.DiversityGainWWV = udR1.Value;
                        break;
                    case Band.GEN:
                        console.DiversityGainGEN = udR1.Value;
                        break;
                    default:
                        console.DiversityGainXVTR = udR1.Value;
                        break;
                }
            }
            updateR1 = true;

            //  }
        }

        public bool DiversityRXRef
        {
            set
            {
                if (value)
                    radioButtonMerc1.Checked = true;
                else
                    radioButtonMerc2.Checked = true;
            }

            get             // added 31/3/2018 G8NJJ to allow access by CAT commands
            {
                if (radioButtonMerc1.Checked == true)
                    return true;
                else
                    return false;
            }
        }



        public int DiversityRXSource         // added 31/3/2018 G8NJJ to allow access by CAT commands
        {
            set
            {
                if (value == 0)
                    radRxSourceRx1Rx2.Checked = true;
                else if (value == 1)
                    radRxSource1.Checked = true;
                else
                    radRxSource2.Checked = true;
            }

            get
            {
                if (radRxSourceRx1Rx2.Checked)
                    return 0;
                else if (radRxSource1.Checked)
                    return 1;
                else
                    return 2;
            }
        }

        // added 6/8/2019 G8NJJ to allow access by Andromeda. Sets the appropriate gain.
        public decimal CATDiversityGain
        {
            set
            {
                if (radioButtonMerc1.Checked)
                    //udR2.Value = value; //MW0LGE_[2.9.0.7]
                    DiversityR2Gain = value;
                else
                    //udR1.Value = value; //MW0LGE_[2.9.0.7]
                    DiversityGain = value;
            }
            get
            {
                if (radioButtonMerc1.Checked)
                    //return udR2.Value; //MW0LGE_[2.9.0.7]
                    return DiversityR2Gain;
                else
                    //return udR1.Value; //MW0LGE_[2.9.0.7]
                    return DiversityGain;
            }
        }

        public decimal DiversityGain
        {
            set
            {
                bool bOldLock = chkLockR.Checked; //[2.10.3.5]MW0LGE fixes #324
                chkLockR.Checked = false;

                decimal v = Math.Min(value, udR1.Maximum);
                v = Math.Max(v, udR1.Minimum); //MW0LGE_[2.9.0.7] not really needed as min is 0, belts/braces
                _initalising = true;
                udR1.Value = v;
                _initalising = false;
                udR1_ValueChanged(this, EventArgs.Empty);

                chkLockR.Checked = bOldLock;
            }
            get { return udR1.Value; }      // added 31/3/2018 G8NJJ to allow access by CAT commands
        }

        public decimal DiversityR2Gain
        {
            set
            {
                bool bOldLock = chkLockR.Checked; //[2.10.3.5]MW0LGE fixes #324
                chkLockR.Checked = false;

                decimal v = Math.Min(value, udR2.Maximum);
                v = Math.Max(v, udR2.Minimum); //MW0LGE_[2.9.0.7] not really needed as min is 0, belts/braces
                _initalising = true;
                udR2.Value = v;
                _initalising = false;
                udR2_ValueChanged(this, EventArgs.Empty);

                chkLockR.Checked = bOldLock;
            }
            get { return udR2.Value; }      // added 31/3/2018 G8NJJ to allow access by CAT commands
        }

        public decimal DiversityPhase
        {
            set
            {
                bool bOldLockAngle = chkLockAngle.Checked; //[2.10.3.5]MW0LGE fixes #324
                chkLockAngle.Checked = false;

                udFineNull.Value = value;
                udFineNull_ValueChanged(this, EventArgs.Empty);

                chkLockAngle.Checked = bOldLockAngle;
            }
            get { return udFineNull.Value; }        // added 31/3/2018 G8NJJ to allow access by CAT commands
        }

        public bool DiversityEnabled
        {
            set { chkEnableDiversity.Checked = value; }
            get
            {
                if (chkEnableDiversity.Checked)
                    return true;
                else
                    return false;
            }
        }

        private void udCalib_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            UpdateDiversity();
        }


        private void udAngle0_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (udAngle0.Value >= 360) udAngle0.Value = (decimal)0;
            if (udAngle0.Value <= -1) udAngle0.Value = (decimal)359;
            //if (udAngle0.Value == 360) udAngle0.Value = (decimal)0;
            //if (udAngle0.Value == -1) udAngle0.Value = (decimal)359;
            udAngle.Value = (decimal)(ConvertAngle0ToAngle((double)udAngle0.Value));
            angle = (double)udAngle.Value;
            UpdateDiversity();
        }

        private double ConvertAngleToAngle0(double e)
        {
            double rad_deg = e * 180 / Math.PI;   //convert radian angle to degrees
            double new_angle = 0;
            if ((rad_deg >= 0 & rad_deg <= 90) | (rad_deg <= -90 & rad_deg >= -181)) new_angle = 90 - rad_deg;
            if (rad_deg < 0 & rad_deg >= -90) new_angle = 90 - rad_deg;
            if (rad_deg > 90 & rad_deg <= 181) new_angle = 450 - rad_deg;
            return new_angle;                       //in degrees
        }

        private double ConvertAngle0ToAngle(double e)
        {
            double new_angle = 0;
            if (e == 360) new_angle = 0;
            if (e <= 270) new_angle = 90 - e;
            if (e > 270 & e < 360) new_angle = 450 - e;
            new_angle = new_angle * Math.PI / 180;          //convert compass degrees to radians
            return new_angle;                       //in radians
        }

        /*       private void labelTS13_Click(object sender, EventArgs e)
               {

               }
       */
        private void panelDivControls_Enter(object sender, EventArgs e)
        {

        }

        private void udAntSpacing_ValueChanged_1(object sender, EventArgs e)
        {
            if (_initalising) return;
            udAngle.Value = (decimal)(ConvertAngle0ToAngle((double)udAngle0.Value));
            angle = (double)udAngle.Value;
            UpdateDiversity();
        }

        private double CalcVrms(double a, double b)
        {
            int i;
            double freq;
            double dt;
            double phi;
            double v1;
            double v2;
            double rms;
            double pi = Math.PI;
            double theta;
            double angular_freq;
            //double steering_angle;
            double pwr;

            theta = a;
            steering_angle = b;
            freq = 1E6 * (double)console.VFOAFreq;
            dt = 1 / (20 * freq);                  //calc time step; 1/20th of a full cycle of RF
            if (radioButtonMerc1.Checked)
            {
                phi = cross_fire + Math.Cos((theta + steering_angle));
            }
            else
            {
                phi = cross_fire + Math.Cos(theta + steering_angle);
            }
            rms = 0;
            angular_freq = 2 * pi * freq * dt;

            // generate 20 time points within one cycle of the freq and calculate antenna voltages
            // then calculate rms value for these voltages for the specified theta
            for (i = 0; i < 20; i++)
            {
                v1 = (double)Math.Sin(((double)i) * angular_freq);
                v2 = (double)Math.Sin((((double)i) * angular_freq) + phi - 2 * pi * d_lambda);
                double sum = v1 + v2;
                rms = rms + sum * sum;  //no need for sqrt for rms since sensitivity ~ power ~ v^2
            }
            pwr = (rms / 20) * 5.5;          //normalize to 1.0 max
            return pwr;

        }

        private void chkCrossFire_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (chkCrossFire.Checked) cross_fire = Math.PI;
            else
                cross_fire = 0;
            udAngle.Value = (decimal)(ConvertAngle0ToAngle((double)udAngle0.Value));
            angle = (double)udAngle.Value;
            UpdateDiversity();
        }

        private void udFineNull_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (chkLockAngle.Checked)
            {
                angle_A = locked_angle;
                //udFineNull.Value = (decimal)locked_angle;
                double dTmpCalc = (180 * locked_angle / Math.PI);
                if ((decimal)dTmpCalc != udFineNull.Value) // MW0LGE_21a only do if different
                {
                    udFineNull.Value = (decimal)dTmpCalc;  //degrees
                }
            }
            else
            {
                //udAngle.Value = udFineNull.Value;
                udAngle.Value = (decimal)(Math.PI * (double)udFineNull.Value / 180); //radians
                //if (udAngle0.Value >= 360) udAngle0.Value = (decimal)0;
                //if (udAngle0.Value <= -1) udAngle0.Value = (decimal)359;
                //fine_null = (double)udFineNull.Value;
                fine_null = (double)(Math.PI * (double)udFineNull.Value / 180); //radians
                //udAngle.Value = (decimal)(ConvertAngle0ToAngle((double)udAngle0.Value));
                //angle = (double)udAngle.Value;
            }
            UpdateDiversity();

            switch (console.RX1Band)
            {
                case Band.B160M:
                    console.DiversityPhase160m = udFineNull.Value;
                    break;
                case Band.B80M:
                    console.DiversityPhase80m = udFineNull.Value;
                    break;
                case Band.B60M:
                    console.DiversityPhase60m = udFineNull.Value;
                    break;
                case Band.B40M:
                    console.DiversityPhase40m = udFineNull.Value;
                    break;
                case Band.B30M:
                    console.DiversityPhase30m = udFineNull.Value;
                    break;
                case Band.B20M:
                    console.DiversityPhase20m = udFineNull.Value;
                    break;
                case Band.B17M:
                    console.DiversityPhase17m = udFineNull.Value;
                    break;
                case Band.B15M:
                    console.DiversityPhase15m = udFineNull.Value;
                    break;
                case Band.B12M:
                    console.DiversityPhase12m = udFineNull.Value;
                    break;
                case Band.B10M:
                    console.DiversityPhase10m = udFineNull.Value;
                    break;
                case Band.B6M:
                    console.DiversityPhase6m = udFineNull.Value;
                    break;
                case Band.WWV:
                    console.DiversityPhaseWWV = udFineNull.Value;
                    break;
                case Band.GEN:
                    console.DiversityPhaseGEN = udFineNull.Value;
                    break;
                default:
                    console.DiversityPhaseXVTR = udFineNull.Value;
                    break;
            }

        }
        private int _extDivOutput = 0;
        public int EXTDIVOutput
        {
            //0 = rx1
            //1 = rx2
            //2 = rx1+rx2
            get { return _extDivOutput; }
        }
        private void radRxSource1_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (radRxSource1.Checked)
            // Audio.IQSource = 1;
            {
                _extDivOutput = 0;
                //JanusAudio.SetMercSource(1);
                WDSP.SetEXTDIVOutput(0, 0);

                if (!console.IsSetupFormNull) console.SetupForm.UpdateDDCTab(); //[2.10.3.0]MW0LGE
            }
        }

        private void radRxSource2_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (radRxSource2.Checked)
            // Audio.IQSource = 2;
            {
                _extDivOutput = 1;
                //JanusAudio.SetMercSource(2);
                WDSP.SetEXTDIVOutput(0, 1);

                if (!console.IsSetupFormNull) console.SetupForm.UpdateDDCTab(); //[2.10.3.0]MW0LGE
            }
        }

        private void radRxSourceRx1Rx2_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            if (radRxSourceRx1Rx2.Checked)
            // Audio.IQSource = 3;
            {
                _extDivOutput = 2;
                //JanusAudio.SetMercSource(3);
                WDSP.SetEXTDIVOutput(0, 2);

                if (!console.IsSetupFormNull) console.SetupForm.UpdateDDCTab(); //[2.10.3.0]MW0LGE
            }
        }

        private void chkEnableDiversity_CheckedChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            console.Diversity2 = chkEnableDiversity.Checked;
            if (chkEnableDiversity.Checked)
            {
                chkEnableDiversity.BackColor = Color.LimeGreen;
                chkEnableDiversity.Text = "Enabled";
            }
            else
            {
                chkEnableDiversity.BackColor = Color.Red;
                chkEnableDiversity.Text = "Disabled";
            }
        }

        // method called by console encoder event. Provides option of auto-show and auto-hide
        // if form was not shown, mark it as opened by an encoder event
        public void FormEncoderEvent()
        {
            if (!this.Visible)
            {
                FormAutoShown = true;
                this.Show();
            }
            // set timer if form is auto shown
            if (FormAutoShown)
            {
                AutoHideTimer.Enabled = false;
                AutoHideTimer.AutoReset = false;                    // just one callback
                AutoHideTimer.Interval = 10000;                     // 10 seconds
                AutoHideTimer.Enabled = true;

            }
        }

        // callback function when 10 second timer expires; hide the form
        private void Callback(object source, ElapsedEventArgs e)
        {
            FormAutoShown = false;
            AutoHideTimer.Enabled = false;
            this.Hide();
        }

        // check if we want portrait or landscape format. If landscape change form size and panel positions
        private void DiversityForm_Load(object sender, EventArgs e)
        {
            if (!console.IsSetupFormNull)
            {
                if (console.SetupForm.AndromedaDiversityFormLandscape)
                {
                    picRadar.Anchor = (AnchorStyles.None);
                    picRadar.Size = new Size(226, 226);
                    this.Size = new Size(750, 280);
                    picRadar.Location = new Point(470, 1);
                }
            }
        }

        private void DiversityForm_Resize(object sender, EventArgs e)
        {
            picRadar.Invalidate();
        }

        private void udGainMulti_ValueChanged(object sender, EventArgs e)
        {
            if (_initalising) return;
            m_dGainMulti = (double)Math.Round(udGainMulti.Value, 2);
            udR1.Maximum = (decimal)m_dGainMulti;
            udR2.Maximum = (decimal)m_dGainMulti;

            if (chkLockR.Checked)
            {
                if (udGainMulti.Value != (decimal)m_dGainMulti) udGainMulti.Value = (decimal)m_dGainMulti;
                return;
            }

            double r1 = (double)udR1.Value;
            double r2 = (double)udR2.Value;

            r1 /= m_dGainMulti; //norm
            r2 /= m_dGainMulti;

            if (radioButtonMerc1.Checked) // order matters, as udR is updated by each of them and we need the correct one last
            {
                _initalising = true;
                udR1.Value = (decimal)Math.Round((r1 * m_dGainMulti), 3);
                udR2.Value = (decimal)Math.Round((r2 * m_dGainMulti), 3);
                _initalising = false;
                udR2_ValueChanged(this, EventArgs.Empty);
            }
            else if (radioButtonMerc2.Checked)
            {
                _initalising = true;
                udR2.Value = (decimal)Math.Round((r2 * m_dGainMulti), 3);
                udR1.Value = (decimal)Math.Round((r1 * m_dGainMulti), 3);
                _initalising = false;
                udR1_ValueChanged(this, EventArgs.Empty);
            }
        }

        private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkAlwaysOnTop.Checked;
        }

        private void chkNoAttLink_CheckedChanged(object sender, EventArgs e)
        {
            console.DiversityAttLink = !chkNoAttLink.Checked;
        }

        private void chkVFOSync_CheckedChanged(object sender, EventArgs e)
        {
            //note: pointless doing all the rx2 changes as per the commented block below, as everything gets overwritten with the copy anyway
            console.VFOSync = chkVFOSync.Checked;
            chkVFOSync.BackColor = chkVFOSync.Checked ? Color.LimeGreen : SystemColors.Control;
        }

        public bool VFOSync
        {
            get { return chkVFOSync.Checked; }
            set
            {
                if (this.InvokeRequired)
                    this.Invoke((Action)(() => chkVFOSync.Checked = value));
                else
                    chkVFOSync.Checked = value;

            }
        }

        private void btnMemory_Click(object sender, EventArgs e)
        {
            int index = int.Parse(((Control)sender).Name.Substring(4)) - 1;
            if (Common.ShiftKeyDown)
            {
                //store
                _memories[index].enabled = true;
                _memories[index].gainMulti = (double)udGainMulti.Value;
                _memories[index].receiverSource = radRxSourceRx1Rx2.Checked ? 0 : (radRxSource1.Checked ? 1 : 2);
                _memories[index].rx1RefSource = radioButtonMerc1.Checked;
                _memories[index].rx1Gain = (double)udR1.Value;
                _memories[index].rx2Gain = (double)udR2.Value;
                _memories[index].phase = (double)udFineNull.Value;
                _memories[index].lockPhase = chkLockAngle.Checked;
                _memories[index].lockGain = chkLockR.Checked;
                _memories[index].angle = (double)udAngle.Value;
                _memories[index].r = (double)udR.Value;
                picRadar.Invalidate();
            }
            else if (Common.CtrlKeyDown)
            {
                _memories[index].enabled = false;
                picRadar.Invalidate();
            }
            else if (_memories[index].enabled)
            {
                //recall
                memorySettings ms = _memories[index];
                if (ms != null)
                {
                    chkLockAngle.Checked = false; // need these to be unlocked so we can set values, then lock at end if needed
                    chkLockR.Checked = false;
                    _initalising = true;
                    udGainMulti.Value = (decimal)ms.gainMulti; // prevent anything from happening in the changed event
                    _initalising = false;
                    udGainMulti_ValueChanged(this, EventArgs.Empty); // call it here, to ensure it is done, even if numbers the same
                    switch (_memories[index].receiverSource)
                    {
                        case 0:
                            radRxSourceRx1Rx2.Checked = true;
                            break;
                        case 1:
                            radRxSource1.Checked = true;
                            break;
                        case 2:
                            radRxSource2.Checked = true;
                            break;
                    }
                    radioButtonMerc1.Checked = ms.rx1RefSource;
                    radioButtonMerc2.Checked = !ms.rx1RefSource;
                    udR1.Value = (decimal)ms.rx1Gain;
                    udR2.Value = (decimal)ms.rx2Gain;
                    udFineNull.Value = (decimal)ms.phase;
                    //udAngle.Value = (decimal)ms.angle; //not needed, calulated
                    //udR.Value = (decimal)ms.r; //not needed, calulated
                    UpdateDiversity();
                    chkLockAngle.Checked = ms.lockPhase;
                    chkLockR.Checked = ms.lockGain;

                }
            }
        }

        private double NormalizeAngle(double angle)
        {
            double PI = Math.PI;

            angle = angle % (2 * PI);
            if (angle > PI)
            {
                angle -= 2 * PI;
            }
            else if (angle <= -PI)
            {
                angle += 2 * PI;
            }
            return angle;
        }
        private void stepAngle(double degrees)
        {
            double PI = Math.PI;
            double radians = degrees * (PI / 180.0);
            if (chkLockAngle.Checked) return;
            double currentAngleRadians = (double)udAngle.Value;
            currentAngleRadians += radians;
            currentAngleRadians = NormalizeAngle(currentAngleRadians);
            udAngle.Value = (decimal)currentAngleRadians;
            udAngle0.Value = (decimal)ConvertAngleToAngle0(currentAngleRadians);
            udFineNull.Value = (decimal)(180 * currentAngleRadians / Math.PI);
            UpdateDiversity();
        }
        private void btnShiftUp10_Click(object sender, EventArgs e)
        {
            stepAngle(10);
        }

        private void btnShift90_Click(object sender, EventArgs e)
        {
            stepAngle(90);
        }

        private void btnShiftDown10_Click(object sender, EventArgs e)
        {
            stepAngle(-10);
        }

        private void udZoom_ValueChanged(object sender, EventArgs e)
        {

        }

        //[2.10.3.5]MW0LGE old code, kept for reference
        //private void btnSync_Click(object sender, System.EventArgs e)
        //{
        //    console.RX2DSPMode = console.RX1DSPMode;
        //    console.RX2Filter = console.RX1Filter;
        //    console.RX2PreampMode = console.RX1PreampMode;
        //    console.VFOSync = true;
        //    console.radio.GetDSPRX(1, 0).Copy(console.radio.GetDSPRX(0, 0));
        //}

        private void setNewNaming(bool new_naming)
        {
            if(new_naming)
            {            
                grpRxSource.Text = "RX1 Source";

                radRxSourceRx1Rx2.Text = "Sync 1 + 2";
                radRxSource1.Text = "Sync 1";
                radRxSource2.Text = "Sync 2";

                radioButtonMerc1.Text = "Sync 1";
                radioButtonMerc2.Text = "Sync 2";
            }
            else
            {
                grpRxSource.Text = "Receiver Source";

                radRxSourceRx1Rx2.Text = "Rx1 + Rx2";
                radRxSource1.Text = "Receiver 1";
                radRxSource2.Text = "Receiver 2";

                radioButtonMerc1.Text = "Receiver 1";
                radioButtonMerc2.Text = "Receiver 2";
            }
        }
    }
}
