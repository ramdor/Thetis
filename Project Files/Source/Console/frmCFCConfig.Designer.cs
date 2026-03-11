namespace Thetis
{
    partial class frmCFCConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.btnResetEQ = new System.Windows.Forms.ButtonTS();
            this.ucCFC_comp = new Thetis.ucParametricEq();
            this.labelTS673 = new System.Windows.Forms.LabelTS();
            this.nudCFC_c = new System.Windows.Forms.NumericUpDownTS();
            this.chkCFC_PanaEQ_live = new System.Windows.Forms.CheckBoxTS();
            this.labelTS672 = new System.Windows.Forms.LabelTS();
            this.nudCFC_f = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS670 = new System.Windows.Forms.LabelTS();
            this.labelTS664 = new System.Windows.Forms.LabelTS();
            this.labelTS671 = new System.Windows.Forms.LabelTS();
            this.labelTS667 = new System.Windows.Forms.LabelTS();
            this.nudCFC_posteqgain = new System.Windows.Forms.NumericUpDownTS();
            this.nudCFC_selected_band = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS668 = new System.Windows.Forms.LabelTS();
            this.nudCFC_precomp = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS669 = new System.Windows.Forms.LabelTS();
            this.udCFC_low = new System.Windows.Forms.NumericUpDownTS();
            this.radCFC_10 = new System.Windows.Forms.RadioButtonTS();
            this.chkCFC_UseQFactors = new System.Windows.Forms.CheckBoxTS();
            this.labelTS662 = new System.Windows.Forms.LabelTS();
            this.btnResetComp = new System.Windows.Forms.ButtonTS();
            this.radCFC_18 = new System.Windows.Forms.RadioButtonTS();
            this.labelTS665 = new System.Windows.Forms.LabelTS();
            this.radCFC_5 = new System.Windows.Forms.RadioButtonTS();
            this.nudCFC_gain = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS661 = new System.Windows.Forms.LabelTS();
            this.nudCFC_q = new System.Windows.Forms.NumericUpDownTS();
            this.udCFC_high = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS666 = new System.Windows.Forms.LabelTS();
            this.ucCFC_eq = new Thetis.ucParametricEq();
            this.labelTS663 = new System.Windows.Forms.LabelTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.nudCFC_cq = new System.Windows.Forms.NumericUpDownTS();
            this.lblOGGuide = new System.Windows.Forms.LinkLabel();
            this.chkLogScale = new System.Windows.Forms.CheckBoxTS();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_c)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_f)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_posteqgain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_selected_band)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_precomp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCFC_low)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_gain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_q)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCFC_high)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_cq)).BeginInit();
            this.SuspendLayout();
            // 
            // btnResetEQ
            // 
            this.btnResetEQ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetEQ.Image = null;
            this.btnResetEQ.Location = new System.Drawing.Point(532, 387);
            this.btnResetEQ.Name = "btnResetEQ";
            this.btnResetEQ.Selectable = true;
            this.btnResetEQ.Size = new System.Drawing.Size(84, 32);
            this.btnResetEQ.TabIndex = 202;
            this.btnResetEQ.Text = "Reset";
            this.btnResetEQ.UseVisualStyleBackColor = true;
            this.btnResetEQ.Click += new System.EventHandler(this.btnResetEQ_Click);
            // 
            // ucCFC_comp
            // 
            this.ucCFC_comp.AllowPointReorder = true;
            this.ucCFC_comp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ucCFC_comp.AxisTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.ucCFC_comp.AxisTickColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.ucCFC_comp.AxisTickLength = 6;
            this.ucCFC_comp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.ucCFC_comp.BandShadeAlpha = 70;
            this.ucCFC_comp.BandShadeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.ucCFC_comp.BandShadeWeightCutoff = 0.002D;
            this.ucCFC_comp.BarChartFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(255)))));
            this.ucCFC_comp.BarChartPeakColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(210)))), ((int)(((byte)(255)))));
            this.ucCFC_comp.DbMax = 16D;
            this.ucCFC_comp.DbMin = 0D;
            this.ucCFC_comp.ForeColor = System.Drawing.Color.Gainsboro;
            this.ucCFC_comp.FrequencyMaxHz = 4000D;
            this.ucCFC_comp.FrequencyMinHz = 0D;
            this.ucCFC_comp.GlobalGainDb = 0D;
            this.ucCFC_comp.GlobalGainIsHorizLine = true;
            this.ucCFC_comp.Location = new System.Drawing.Point(12, 36);
            this.ucCFC_comp.MinPointSpacingHz = 5D;
            this.ucCFC_comp.Name = "ucCFC_comp";
            this.ucCFC_comp.ParametricEQ = true;
            this.ucCFC_comp.QMax = 20D;
            this.ucCFC_comp.QMin = 0.2D;
            this.ucCFC_comp.SelectedIndex = -1;
            this.ucCFC_comp.ShowAxisScales = true;
            this.ucCFC_comp.ShowBandShading = true;
            this.ucCFC_comp.ShowReadout = false;
            this.ucCFC_comp.Size = new System.Drawing.Size(509, 320);
            this.ucCFC_comp.TabIndex = 201;
            this.ucCFC_comp.UsePerBandColours = true;
            this.ucCFC_comp.YAxisStepDb = 2D;
            this.ucCFC_comp.PointsChanged += new System.EventHandler<Thetis.ucParametricEq.EqDraggingEventArgs>(this.ucCFC_comp_PointsChanged);
            this.ucCFC_comp.GlobalGainChanged += new System.EventHandler<Thetis.ucParametricEq.EqDraggingEventArgs>(this.ucCFC_comp_GlobalGainChanged);
            this.ucCFC_comp.PointDataChanged += new System.EventHandler<Thetis.ucParametricEq.EqPointDataChangedEventArgs>(this.ucCFC_comp_PointDataChanged);
            this.ucCFC_comp.PointSelected += new System.EventHandler<Thetis.ucParametricEq.EqPointSelectionChangedEventArgs>(this.ucCFC_comp_PointSelected);
            this.ucCFC_comp.PointUnselected += new System.EventHandler<Thetis.ucParametricEq.EqPointSelectionChangedEventArgs>(this.ucCFC_comp_PointUnselected);
            // 
            // labelTS673
            // 
            this.labelTS673.AutoSize = true;
            this.labelTS673.Image = null;
            this.labelTS673.Location = new System.Drawing.Point(396, 11);
            this.labelTS673.Name = "labelTS673";
            this.labelTS673.Size = new System.Drawing.Size(20, 13);
            this.labelTS673.TabIndex = 200;
            this.labelTS673.Text = "dB";
            // 
            // nudCFC_c
            // 
            this.nudCFC_c.DecimalPlaces = 1;
            this.nudCFC_c.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudCFC_c.Location = new System.Drawing.Point(341, 7);
            this.nudCFC_c.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.nudCFC_c.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudCFC_c.Name = "nudCFC_c";
            this.nudCFC_c.Size = new System.Drawing.Size(49, 20);
            this.nudCFC_c.TabIndex = 198;
            this.nudCFC_c.TinyStep = false;
            this.nudCFC_c.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudCFC_c.ValueChanged += new System.EventHandler(this.nudCFC_c_ValueChanged);
            // 
            // chkCFC_PanaEQ_live
            // 
            this.chkCFC_PanaEQ_live.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCFC_PanaEQ_live.Image = null;
            this.chkCFC_PanaEQ_live.Location = new System.Drawing.Point(527, 201);
            this.chkCFC_PanaEQ_live.Name = "chkCFC_PanaEQ_live";
            this.chkCFC_PanaEQ_live.Size = new System.Drawing.Size(84, 17);
            this.chkCFC_PanaEQ_live.TabIndex = 188;
            this.chkCFC_PanaEQ_live.Text = "Live Update";
            this.chkCFC_PanaEQ_live.UseVisualStyleBackColor = true;
            // 
            // labelTS672
            // 
            this.labelTS672.AutoSize = true;
            this.labelTS672.Image = null;
            this.labelTS672.Location = new System.Drawing.Point(302, 11);
            this.labelTS672.Name = "labelTS672";
            this.labelTS672.Size = new System.Drawing.Size(34, 13);
            this.labelTS672.TabIndex = 199;
            this.labelTS672.Text = "Comp";
            // 
            // nudCFC_f
            // 
            this.nudCFC_f.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudCFC_f.Location = new System.Drawing.Point(91, 7);
            this.nudCFC_f.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.nudCFC_f.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudCFC_f.Name = "nudCFC_f";
            this.nudCFC_f.Size = new System.Drawing.Size(50, 20);
            this.nudCFC_f.TabIndex = 186;
            this.nudCFC_f.TinyStep = false;
            this.nudCFC_f.Value = new decimal(new int[] {
            16000,
            0,
            0,
            0});
            this.nudCFC_f.ValueChanged += new System.EventHandler(this.nudCFC_f_ValueChanged);
            // 
            // labelTS670
            // 
            this.labelTS670.AutoSize = true;
            this.labelTS670.Image = null;
            this.labelTS670.Location = new System.Drawing.Point(163, 367);
            this.labelTS670.Name = "labelTS670";
            this.labelTS670.Size = new System.Drawing.Size(20, 13);
            this.labelTS670.TabIndex = 197;
            this.labelTS670.Text = "dB";
            // 
            // labelTS664
            // 
            this.labelTS664.AutoSize = true;
            this.labelTS664.Image = null;
            this.labelTS664.Location = new System.Drawing.Point(76, 11);
            this.labelTS664.Name = "labelTS664";
            this.labelTS664.Size = new System.Drawing.Size(10, 13);
            this.labelTS664.TabIndex = 187;
            this.labelTS664.Text = "f";
            // 
            // labelTS671
            // 
            this.labelTS671.AutoSize = true;
            this.labelTS671.Image = null;
            this.labelTS671.Location = new System.Drawing.Point(53, 367);
            this.labelTS671.Name = "labelTS671";
            this.labelTS671.Size = new System.Drawing.Size(46, 13);
            this.labelTS671.TabIndex = 196;
            this.labelTS671.Text = "Post-EQ";
            // 
            // labelTS667
            // 
            this.labelTS667.AutoSize = true;
            this.labelTS667.Image = null;
            this.labelTS667.Location = new System.Drawing.Point(9, 11);
            this.labelTS667.Name = "labelTS667";
            this.labelTS667.Size = new System.Drawing.Size(14, 13);
            this.labelTS667.TabIndex = 180;
            this.labelTS667.Text = "#";
            // 
            // nudCFC_posteqgain
            // 
            this.nudCFC_posteqgain.DecimalPlaces = 1;
            this.nudCFC_posteqgain.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudCFC_posteqgain.Location = new System.Drawing.Point(108, 363);
            this.nudCFC_posteqgain.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.nudCFC_posteqgain.Minimum = new decimal(new int[] {
            24,
            0,
            0,
            -2147483648});
            this.nudCFC_posteqgain.Name = "nudCFC_posteqgain";
            this.nudCFC_posteqgain.Size = new System.Drawing.Size(49, 20);
            this.nudCFC_posteqgain.TabIndex = 195;
            this.nudCFC_posteqgain.TinyStep = false;
            this.nudCFC_posteqgain.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudCFC_posteqgain.ValueChanged += new System.EventHandler(this.nudCFC_posteqgain_ValueChanged);
            // 
            // nudCFC_selected_band
            // 
            this.nudCFC_selected_band.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudCFC_selected_band.Location = new System.Drawing.Point(29, 7);
            this.nudCFC_selected_band.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudCFC_selected_band.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudCFC_selected_band.Name = "nudCFC_selected_band";
            this.nudCFC_selected_band.ReadOnly = true;
            this.nudCFC_selected_band.Size = new System.Drawing.Size(38, 20);
            this.nudCFC_selected_band.TabIndex = 179;
            this.nudCFC_selected_band.TinyStep = false;
            this.nudCFC_selected_band.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudCFC_selected_band.ValueChanged += new System.EventHandler(this.nudCFC_selected_band_ValueChanged);
            // 
            // labelTS668
            // 
            this.labelTS668.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTS668.AutoSize = true;
            this.labelTS668.Image = null;
            this.labelTS668.Location = new System.Drawing.Point(529, 95);
            this.labelTS668.Name = "labelTS668";
            this.labelTS668.Size = new System.Drawing.Size(27, 13);
            this.labelTS668.TabIndex = 193;
            this.labelTS668.Text = "Low";
            // 
            // nudCFC_precomp
            // 
            this.nudCFC_precomp.DecimalPlaces = 1;
            this.nudCFC_precomp.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudCFC_precomp.Location = new System.Drawing.Point(218, 7);
            this.nudCFC_precomp.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.nudCFC_precomp.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudCFC_precomp.Name = "nudCFC_precomp";
            this.nudCFC_precomp.Size = new System.Drawing.Size(49, 20);
            this.nudCFC_precomp.TabIndex = 189;
            this.nudCFC_precomp.TinyStep = false;
            this.nudCFC_precomp.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudCFC_precomp.ValueChanged += new System.EventHandler(this.nudCFC_precomp_ValueChanged);
            // 
            // labelTS669
            // 
            this.labelTS669.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTS669.AutoSize = true;
            this.labelTS669.Image = null;
            this.labelTS669.Location = new System.Drawing.Point(527, 121);
            this.labelTS669.Name = "labelTS669";
            this.labelTS669.Size = new System.Drawing.Size(29, 13);
            this.labelTS669.TabIndex = 194;
            this.labelTS669.Text = "High";
            // 
            // udCFC_low
            // 
            this.udCFC_low.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.udCFC_low.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udCFC_low.Location = new System.Drawing.Point(562, 93);
            this.udCFC_low.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udCFC_low.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udCFC_low.Name = "udCFC_low";
            this.udCFC_low.Size = new System.Drawing.Size(50, 20);
            this.udCFC_low.TabIndex = 177;
            this.udCFC_low.TinyStep = false;
            this.udCFC_low.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udCFC_low.ValueChanged += new System.EventHandler(this.udCFC_low_ValueChanged);
            // 
            // radCFC_10
            // 
            this.radCFC_10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radCFC_10.AutoSize = true;
            this.radCFC_10.Checked = true;
            this.radCFC_10.Image = null;
            this.radCFC_10.Location = new System.Drawing.Point(553, 35);
            this.radCFC_10.Name = "radCFC_10";
            this.radCFC_10.Size = new System.Drawing.Size(64, 17);
            this.radCFC_10.TabIndex = 174;
            this.radCFC_10.TabStop = true;
            this.radCFC_10.Text = "10-band";
            this.radCFC_10.CheckedChanged += new System.EventHandler(this.radCFC_bands_CheckedChanged);
            // 
            // chkCFC_UseQFactors
            // 
            this.chkCFC_UseQFactors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCFC_UseQFactors.Checked = true;
            this.chkCFC_UseQFactors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCFC_UseQFactors.Image = null;
            this.chkCFC_UseQFactors.Location = new System.Drawing.Point(527, 178);
            this.chkCFC_UseQFactors.Name = "chkCFC_UseQFactors";
            this.chkCFC_UseQFactors.Size = new System.Drawing.Size(94, 17);
            this.chkCFC_UseQFactors.TabIndex = 172;
            this.chkCFC_UseQFactors.Text = "Use Q Factors";
            this.chkCFC_UseQFactors.UseVisualStyleBackColor = true;
            this.chkCFC_UseQFactors.CheckedChanged += new System.EventHandler(this.chkCFC_UseQFactors_CheckedChanged);
            // 
            // labelTS662
            // 
            this.labelTS662.AutoSize = true;
            this.labelTS662.Image = null;
            this.labelTS662.Location = new System.Drawing.Point(159, 11);
            this.labelTS662.Name = "labelTS662";
            this.labelTS662.Size = new System.Drawing.Size(53, 13);
            this.labelTS662.TabIndex = 190;
            this.labelTS662.Text = "Pre-Comp";
            // 
            // btnResetComp
            // 
            this.btnResetComp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetComp.Image = null;
            this.btnResetComp.Location = new System.Drawing.Point(533, 324);
            this.btnResetComp.Name = "btnResetComp";
            this.btnResetComp.Selectable = true;
            this.btnResetComp.Size = new System.Drawing.Size(84, 32);
            this.btnResetComp.TabIndex = 173;
            this.btnResetComp.Text = "Reset";
            this.btnResetComp.UseVisualStyleBackColor = true;
            this.btnResetComp.Click += new System.EventHandler(this.btnResetComp_Click);
            // 
            // radCFC_18
            // 
            this.radCFC_18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radCFC_18.AutoSize = true;
            this.radCFC_18.Image = null;
            this.radCFC_18.Location = new System.Drawing.Point(553, 58);
            this.radCFC_18.Name = "radCFC_18";
            this.radCFC_18.Size = new System.Drawing.Size(64, 17);
            this.radCFC_18.TabIndex = 175;
            this.radCFC_18.Text = "18-band";
            this.radCFC_18.CheckedChanged += new System.EventHandler(this.radCFC_bands_CheckedChanged);
            // 
            // labelTS665
            // 
            this.labelTS665.AutoSize = true;
            this.labelTS665.Image = null;
            this.labelTS665.Location = new System.Drawing.Point(304, 367);
            this.labelTS665.Name = "labelTS665";
            this.labelTS665.Size = new System.Drawing.Size(15, 13);
            this.labelTS665.TabIndex = 185;
            this.labelTS665.Text = "Q";
            // 
            // radCFC_5
            // 
            this.radCFC_5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radCFC_5.AutoSize = true;
            this.radCFC_5.Image = null;
            this.radCFC_5.Location = new System.Drawing.Point(553, 12);
            this.radCFC_5.Name = "radCFC_5";
            this.radCFC_5.Size = new System.Drawing.Size(58, 17);
            this.radCFC_5.TabIndex = 176;
            this.radCFC_5.Text = "5-band";
            this.radCFC_5.CheckedChanged += new System.EventHandler(this.radCFC_bands_CheckedChanged);
            // 
            // nudCFC_gain
            // 
            this.nudCFC_gain.DecimalPlaces = 1;
            this.nudCFC_gain.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nudCFC_gain.Location = new System.Drawing.Point(224, 363);
            this.nudCFC_gain.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.nudCFC_gain.Minimum = new decimal(new int[] {
            24,
            0,
            0,
            -2147483648});
            this.nudCFC_gain.Name = "nudCFC_gain";
            this.nudCFC_gain.Size = new System.Drawing.Size(49, 20);
            this.nudCFC_gain.TabIndex = 181;
            this.nudCFC_gain.TinyStep = false;
            this.nudCFC_gain.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nudCFC_gain.ValueChanged += new System.EventHandler(this.nudCFC_gain_ValueChanged);
            // 
            // labelTS661
            // 
            this.labelTS661.AutoSize = true;
            this.labelTS661.Image = null;
            this.labelTS661.Location = new System.Drawing.Point(270, 11);
            this.labelTS661.Name = "labelTS661";
            this.labelTS661.Size = new System.Drawing.Size(20, 13);
            this.labelTS661.TabIndex = 191;
            this.labelTS661.Text = "dB";
            // 
            // nudCFC_q
            // 
            this.nudCFC_q.DecimalPlaces = 2;
            this.nudCFC_q.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudCFC_q.Location = new System.Drawing.Point(325, 363);
            this.nudCFC_q.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudCFC_q.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.nudCFC_q.Name = "nudCFC_q";
            this.nudCFC_q.Size = new System.Drawing.Size(49, 20);
            this.nudCFC_q.TabIndex = 184;
            this.nudCFC_q.TinyStep = false;
            this.nudCFC_q.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nudCFC_q.ValueChanged += new System.EventHandler(this.nudCFC_q_ValueChanged);
            // 
            // udCFC_high
            // 
            this.udCFC_high.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.udCFC_high.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udCFC_high.Location = new System.Drawing.Point(562, 119);
            this.udCFC_high.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udCFC_high.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udCFC_high.Name = "udCFC_high";
            this.udCFC_high.Size = new System.Drawing.Size(50, 20);
            this.udCFC_high.TabIndex = 178;
            this.udCFC_high.TinyStep = false;
            this.udCFC_high.Value = new decimal(new int[] {
            16000,
            0,
            0,
            0});
            this.udCFC_high.ValueChanged += new System.EventHandler(this.udCFC_high_ValueChanged);
            // 
            // labelTS666
            // 
            this.labelTS666.AutoSize = true;
            this.labelTS666.Image = null;
            this.labelTS666.Location = new System.Drawing.Point(189, 367);
            this.labelTS666.Name = "labelTS666";
            this.labelTS666.Size = new System.Drawing.Size(29, 13);
            this.labelTS666.TabIndex = 182;
            this.labelTS666.Text = "Gain";
            // 
            // ucCFC_eq
            // 
            this.ucCFC_eq.AllowPointReorder = true;
            this.ucCFC_eq.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ucCFC_eq.AxisTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.ucCFC_eq.AxisTickColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.ucCFC_eq.AxisTickLength = 6;
            this.ucCFC_eq.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
            this.ucCFC_eq.BandShadeAlpha = 70;
            this.ucCFC_eq.BandShadeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.ucCFC_eq.BandShadeWeightCutoff = 0.002D;
            this.ucCFC_eq.BarChartFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(255)))));
            this.ucCFC_eq.BarChartPeakColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(210)))), ((int)(((byte)(255)))));
            this.ucCFC_eq.DbMax = 24D;
            this.ucCFC_eq.DbMin = -24D;
            this.ucCFC_eq.ForeColor = System.Drawing.Color.Gainsboro;
            this.ucCFC_eq.FrequencyMaxHz = 4000D;
            this.ucCFC_eq.FrequencyMinHz = 0D;
            this.ucCFC_eq.GlobalGainDb = 0D;
            this.ucCFC_eq.Location = new System.Drawing.Point(12, 387);
            this.ucCFC_eq.MinPointSpacingHz = 5D;
            this.ucCFC_eq.Name = "ucCFC_eq";
            this.ucCFC_eq.ParametricEQ = true;
            this.ucCFC_eq.QMax = 20D;
            this.ucCFC_eq.QMin = 0.2D;
            this.ucCFC_eq.SelectedIndex = -1;
            this.ucCFC_eq.ShowAxisScales = true;
            this.ucCFC_eq.ShowBandShading = true;
            this.ucCFC_eq.ShowReadout = false;
            this.ucCFC_eq.Size = new System.Drawing.Size(509, 320);
            this.ucCFC_eq.TabIndex = 0;
            this.ucCFC_eq.UsePerBandColours = true;
            this.ucCFC_eq.PointsChanged += new System.EventHandler<Thetis.ucParametricEq.EqDraggingEventArgs>(this.ucCFC_eq_PointsChanged);
            this.ucCFC_eq.GlobalGainChanged += new System.EventHandler<Thetis.ucParametricEq.EqDraggingEventArgs>(this.ucCFC_eq_GlobalGainChanged);
            this.ucCFC_eq.PointDataChanged += new System.EventHandler<Thetis.ucParametricEq.EqPointDataChangedEventArgs>(this.ucCFC_eq_PointDataChanged);
            this.ucCFC_eq.PointSelected += new System.EventHandler<Thetis.ucParametricEq.EqPointSelectionChangedEventArgs>(this.ucCFC_eq_PointSelected);
            this.ucCFC_eq.PointUnselected += new System.EventHandler<Thetis.ucParametricEq.EqPointSelectionChangedEventArgs>(this.ucCFC_eq_PointUnselected);
            // 
            // labelTS663
            // 
            this.labelTS663.AutoSize = true;
            this.labelTS663.Image = null;
            this.labelTS663.Location = new System.Drawing.Point(279, 367);
            this.labelTS663.Name = "labelTS663";
            this.labelTS663.Size = new System.Drawing.Size(20, 13);
            this.labelTS663.TabIndex = 183;
            this.labelTS663.Text = "dB";
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(423, 11);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(15, 13);
            this.labelTS1.TabIndex = 204;
            this.labelTS1.Text = "Q";
            // 
            // nudCFC_cq
            // 
            this.nudCFC_cq.DecimalPlaces = 2;
            this.nudCFC_cq.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudCFC_cq.Location = new System.Drawing.Point(444, 7);
            this.nudCFC_cq.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudCFC_cq.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.nudCFC_cq.Name = "nudCFC_cq";
            this.nudCFC_cq.Size = new System.Drawing.Size(49, 20);
            this.nudCFC_cq.TabIndex = 203;
            this.nudCFC_cq.TinyStep = false;
            this.nudCFC_cq.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nudCFC_cq.ValueChanged += new System.EventHandler(this.nudCFC_cq_ValueChanged);
            // 
            // lblOGGuide
            // 
            this.lblOGGuide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOGGuide.Location = new System.Drawing.Point(527, 666);
            this.lblOGGuide.Name = "lblOGGuide";
            this.lblOGGuide.Size = new System.Drawing.Size(89, 32);
            this.lblOGGuide.TabIndex = 205;
            this.lblOGGuide.TabStop = true;
            this.lblOGGuide.Text = "OG CFC Guide\r\nby W1AEX";
            this.lblOGGuide.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lblOGGuide.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblOGGuide_LinkClicked);
            // 
            // chkLogScale
            // 
            this.chkLogScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkLogScale.Image = null;
            this.chkLogScale.Location = new System.Drawing.Point(527, 155);
            this.chkLogScale.Name = "chkLogScale";
            this.chkLogScale.Size = new System.Drawing.Size(84, 17);
            this.chkLogScale.TabIndex = 206;
            this.chkLogScale.Text = "Log scale";
            this.chkLogScale.UseVisualStyleBackColor = true;
            this.chkLogScale.CheckedChanged += new System.EventHandler(this.chkLogScale_CheckedChanged);
            // 
            // frmCFCConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 717);
            this.Controls.Add(this.chkLogScale);
            this.Controls.Add(this.lblOGGuide);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.nudCFC_cq);
            this.Controls.Add(this.btnResetEQ);
            this.Controls.Add(this.ucCFC_comp);
            this.Controls.Add(this.labelTS673);
            this.Controls.Add(this.nudCFC_c);
            this.Controls.Add(this.chkCFC_PanaEQ_live);
            this.Controls.Add(this.labelTS672);
            this.Controls.Add(this.nudCFC_f);
            this.Controls.Add(this.labelTS670);
            this.Controls.Add(this.labelTS664);
            this.Controls.Add(this.labelTS671);
            this.Controls.Add(this.labelTS667);
            this.Controls.Add(this.nudCFC_posteqgain);
            this.Controls.Add(this.nudCFC_selected_band);
            this.Controls.Add(this.labelTS668);
            this.Controls.Add(this.nudCFC_precomp);
            this.Controls.Add(this.labelTS669);
            this.Controls.Add(this.udCFC_low);
            this.Controls.Add(this.radCFC_10);
            this.Controls.Add(this.chkCFC_UseQFactors);
            this.Controls.Add(this.labelTS662);
            this.Controls.Add(this.btnResetComp);
            this.Controls.Add(this.radCFC_18);
            this.Controls.Add(this.labelTS665);
            this.Controls.Add(this.radCFC_5);
            this.Controls.Add(this.nudCFC_gain);
            this.Controls.Add(this.labelTS661);
            this.Controls.Add(this.nudCFC_q);
            this.Controls.Add(this.udCFC_high);
            this.Controls.Add(this.labelTS666);
            this.Controls.Add(this.ucCFC_eq);
            this.Controls.Add(this.labelTS663);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximumSize = new System.Drawing.Size(4096, 756);
            this.MinimumSize = new System.Drawing.Size(640, 756);
            this.Name = "frmCFCConfig";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "CFC Config";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCFCConfig_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.frmCFCConfig_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_c)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_f)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_posteqgain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_selected_band)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_precomp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCFC_low)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_gain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_q)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udCFC_high)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCFC_cq)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.LabelTS labelTS673;
        private System.Windows.Forms.NumericUpDownTS nudCFC_c;
        private System.Windows.Forms.LabelTS labelTS672;
        private System.Windows.Forms.LabelTS labelTS670;
        private System.Windows.Forms.LabelTS labelTS671;
        private System.Windows.Forms.NumericUpDownTS nudCFC_posteqgain;
        private System.Windows.Forms.LabelTS labelTS668;
        private System.Windows.Forms.LabelTS labelTS669;
        private System.Windows.Forms.NumericUpDownTS udCFC_low;
        private System.Windows.Forms.CheckBoxTS chkCFC_UseQFactors;
        private System.Windows.Forms.ButtonTS btnResetComp;
        private System.Windows.Forms.LabelTS labelTS665;
        private System.Windows.Forms.NumericUpDownTS nudCFC_gain;
        private System.Windows.Forms.NumericUpDownTS nudCFC_q;
        private System.Windows.Forms.LabelTS labelTS666;
        private System.Windows.Forms.LabelTS labelTS663;
        private ucParametricEq ucCFC_eq;
        private System.Windows.Forms.NumericUpDownTS udCFC_high;
        private System.Windows.Forms.LabelTS labelTS661;
        private System.Windows.Forms.RadioButtonTS radCFC_5;
        private System.Windows.Forms.CheckBoxTS chkCFC_PanaEQ_live;
        private System.Windows.Forms.RadioButtonTS radCFC_18;
        private System.Windows.Forms.LabelTS labelTS662;
        private System.Windows.Forms.RadioButtonTS radCFC_10;
        private System.Windows.Forms.NumericUpDownTS nudCFC_precomp;
        private System.Windows.Forms.NumericUpDownTS nudCFC_selected_band;
        private System.Windows.Forms.LabelTS labelTS667;
        private System.Windows.Forms.LabelTS labelTS664;
        private System.Windows.Forms.NumericUpDownTS nudCFC_f;
        private ucParametricEq ucCFC_comp;
        private System.Windows.Forms.ButtonTS btnResetEQ;
        private System.Windows.Forms.LabelTS labelTS1;
        private System.Windows.Forms.NumericUpDownTS nudCFC_cq;
        private System.Windows.Forms.LinkLabel lblOGGuide;
        private System.Windows.Forms.CheckBoxTS chkLogScale;
    }
}