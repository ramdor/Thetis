namespace Thetis
{
    partial class rxa
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(rxa));
            this.panelPanDisplay = new System.Windows.Forms.PanelTS();
            this.panDisplay = new Thetis.PanDisplay();
            this.udRXAAGCGain = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS5 = new System.Windows.Forms.LabelTS();
            this.labelTS4 = new System.Windows.Forms.LabelTS();
            this.udRXAMode = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS3 = new System.Windows.Forms.LabelTS();
            this.udRXAVolume = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.udRXAFreq = new System.Windows.Forms.NumericUpDownTS();
            this.panelPanDisplay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panDisplay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRXAAGCGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRXAMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRXAVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRXAFreq)).BeginInit();
            this.SuspendLayout();
            // 
            // panelPanDisplay
            // 
            this.panelPanDisplay.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.panelPanDisplay.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.panelPanDisplay.BackColor = System.Drawing.Color.Transparent;
            this.panelPanDisplay.Controls.Add(this.panDisplay);
            this.panelPanDisplay.Location = new System.Drawing.Point(0, 0);
            this.panelPanDisplay.Name = "panelPanDisplay";
            this.panelPanDisplay.Size = new System.Drawing.Size(700, 285);
            this.panelPanDisplay.TabIndex = 10;
            // 
            // panDisplay
            // 
            this.panDisplay.AGCHang = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.panDisplay.AGCKnee = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.panDisplay.AlexPreampOffset = 0F;
            this.panDisplay.AverageOn = true;
            this.panDisplay.AvTau = 0.12D;
            this.panDisplay.BackColor = System.Drawing.Color.Black;
            this.panDisplay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panDisplay.BandEdgeColor = System.Drawing.Color.Red;
            this.panDisplay.ClickTuneFilter = true;
            this.panDisplay.ColorScheme = Thetis.ColorScheme.enhanced;
            this.panDisplay.cOnsole = null;
            this.panDisplay.CurrentClickTuneMode = Thetis.ClickTuneMode.Off;
            this.panDisplay.CurrentDisplayMode = Thetis.DisplayMode.PANADAPTER;
            this.panDisplay.CurrentModel = Thetis.HPSDRModel.HERMES;
            this.panDisplay.CurrentRegion = Thetis.FRSRegion.US;
            this.panDisplay.CWPitch = 600;
            this.panDisplay.DataLineColor = System.Drawing.Color.White;
            this.panDisplay.DataReady = false;
            this.panDisplay.DataType = 1;
            this.panDisplay.DBMScalePanRect = new System.Drawing.Rectangle(0, 0, 35, 15);
            this.panDisplay.DisplayAGCHangLine = true;
            this.panDisplay.DisplayAvgBlocks = 5;
            this.panDisplay.DisplayBackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.panDisplay.DisplayCursorX = 0;
            this.panDisplay.DisplayCursorY = 0;
            this.panDisplay.DisplayDuplex = false;
            this.panDisplay.DisplayFilterColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.panDisplay.DisplayID = 0;
            this.panDisplay.DisplayLabelAlign = Thetis.DisplayLabelAlignment.LEFT;
            this.panDisplay.DisplayLineWidth = 1F;
            this.panDisplay.FFTSize = 4096;
            this.panDisplay.FilterRect = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.panDisplay.FrameRate = 15;
            this.panDisplay.FREQ = 0D;
            this.panDisplay.FreqDiff = 0;
            this.panDisplay.FreqOffset = 0D;
            this.panDisplay.FreqRulerPosition = 0.5F;
            this.panDisplay.FreqScalePanRect = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.panDisplay.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.panDisplay.GridControl = true;
            this.panDisplay.GridPenDark = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.panDisplay.GridTextColor = System.Drawing.Color.Yellow;
            this.panDisplay.GridZeroColor = System.Drawing.Color.Red;
            this.panDisplay.HGridColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.panDisplay.HighFreq = 0;
            this.panDisplay.HighSWR = false;
            this.panDisplay.KaiserPi = 14D;
            this.panDisplay.LinCor = 2;
            this.panDisplay.LinLogCor = -14;
            this.panDisplay.Location = new System.Drawing.Point(0, 0);
            this.panDisplay.LowFreq = 0;
            this.panDisplay.MaxX = 0F;
            this.panDisplay.MaxY = 0F;
            this.panDisplay.MOX = false;
            this.panDisplay.Name = "panDisplay";
            this.panDisplay.NReceivers = 2;
            this.panDisplay.PanFill = true;
            this.panDisplay.PanFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(127)))));
            this.panDisplay.PanRect = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.panDisplay.PanSlider = 0.5D;
            this.panDisplay.PeakOn = false;
            this.panDisplay.Pixels = 2048;
            this.panDisplay.PreampOffset = 0F;
            this.panDisplay.ReverseWaterfall = false;
            this.panDisplay.RIT = 0;
            this.panDisplay.RX1HangSpectrumLine = true;
            this.panDisplay.RXDisplayCalOffset = 0F;
            this.panDisplay.RXDSPMode = Thetis.DSPMode.USB;
            this.panDisplay.RXFFTSizeOffset = 0F;
            this.panDisplay.RXFilterHigh = 0;
            this.panDisplay.RXFilterLow = 0;
            this.panDisplay.SampleRate = 192000;
            this.panDisplay.SecScalePanRect = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.panDisplay.ShowAGC = true;
            this.panDisplay.ShowCTHLine = false;
            this.panDisplay.ShowCWZeroLine = false;
            this.panDisplay.ShowFreqOffset = false;
            this.panDisplay.Size = new System.Drawing.Size(700, 285);
            this.panDisplay.SpectrumGridMax = -50;
            this.panDisplay.SpectrumGridMin = -170;
            this.panDisplay.SpectrumGridStep = 10;
            this.panDisplay.SpectrumLine = true;
            this.panDisplay.SplitDisplay = false;
            this.panDisplay.SplitEnabled = false;
            this.panDisplay.SubRX1Enabled = false;
            this.panDisplay.SubRXFilterColor = System.Drawing.Color.Blue;
            this.panDisplay.SubRXZeroLine = System.Drawing.Color.LightSkyBlue;
            this.panDisplay.TabIndex = 0;
            this.panDisplay.TabStop = false;
            this.panDisplay.Target = null;
            this.panDisplay.TopSize = 0;
            this.panDisplay.VFOASub = ((long)(0));
            this.panDisplay.VFOHz = ((long)(10000000));
            this.panDisplay.WaterfallAGC = false;
            this.panDisplay.WaterfallAvgBlocks = 18;
            this.panDisplay.WaterfallDataReady = false;
            this.panDisplay.WaterfallHighColor = System.Drawing.Color.Yellow;
            this.panDisplay.WaterfallHighThreshold = -80F;
            this.panDisplay.WaterfallLowColor = System.Drawing.Color.Black;
            this.panDisplay.WaterfallLowThreshold = -130F;
            this.panDisplay.WaterfallMidColor = System.Drawing.Color.Red;
            this.panDisplay.WaterfallRect = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.panDisplay.WaterfallUpdatePeriod = 100;
            this.panDisplay.WindowType = 6;
            this.panDisplay.ZoomFactor = 0.5D;
            this.panDisplay.Resize += new System.EventHandler(this.panDisplay_Resize);
            // 
            // udRXAAGCGain
            // 
            this.udRXAAGCGain.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udRXAAGCGain.Location = new System.Drawing.Point(60, 343);
            this.udRXAAGCGain.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.udRXAAGCGain.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            -2147483648});
            this.udRXAAGCGain.Name = "udRXAAGCGain";
            this.udRXAAGCGain.Size = new System.Drawing.Size(120, 20);
            this.udRXAAGCGain.TabIndex = 9;
            this.udRXAAGCGain.TinyStep = false;
            this.udRXAAGCGain.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udRXAAGCGain.ValueChanged += new System.EventHandler(this.udRXAAGCGain_ValueChanged);
            // 
            // labelTS5
            // 
            this.labelTS5.AutoSize = true;
            this.labelTS5.Image = null;
            this.labelTS5.Location = new System.Drawing.Point(-3, 345);
            this.labelTS5.Name = "labelTS5";
            this.labelTS5.Size = new System.Drawing.Size(54, 13);
            this.labelTS5.TabIndex = 8;
            this.labelTS5.Text = "AGC Gain";
            // 
            // labelTS4
            // 
            this.labelTS4.AutoSize = true;
            this.labelTS4.Image = null;
            this.labelTS4.Location = new System.Drawing.Point(55, 419);
            this.labelTS4.Name = "labelTS4";
            this.labelTS4.Size = new System.Drawing.Size(132, 13);
            this.labelTS4.TabIndex = 7;
            this.labelTS4.Text = "(LSB=0, USB=1, SAM=10)";
            // 
            // udRXAMode
            // 
            this.udRXAMode.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udRXAMode.Location = new System.Drawing.Point(60, 395);
            this.udRXAMode.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.udRXAMode.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udRXAMode.Name = "udRXAMode";
            this.udRXAMode.Size = new System.Drawing.Size(120, 20);
            this.udRXAMode.TabIndex = 6;
            this.udRXAMode.TinyStep = false;
            this.udRXAMode.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udRXAMode.ValueChanged += new System.EventHandler(this.udRXAMode_ValueChanged);
            // 
            // labelTS3
            // 
            this.labelTS3.AutoSize = true;
            this.labelTS3.Image = null;
            this.labelTS3.Location = new System.Drawing.Point(-3, 397);
            this.labelTS3.Name = "labelTS3";
            this.labelTS3.Size = new System.Drawing.Size(34, 13);
            this.labelTS3.TabIndex = 5;
            this.labelTS3.Text = "Mode";
            // 
            // udRXAVolume
            // 
            this.udRXAVolume.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udRXAVolume.Location = new System.Drawing.Point(60, 369);
            this.udRXAVolume.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udRXAVolume.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udRXAVolume.Name = "udRXAVolume";
            this.udRXAVolume.Size = new System.Drawing.Size(120, 20);
            this.udRXAVolume.TabIndex = 4;
            this.udRXAVolume.TinyStep = false;
            this.udRXAVolume.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.udRXAVolume.ValueChanged += new System.EventHandler(this.udRXAVolume_ValueChanged);
            // 
            // labelTS2
            // 
            this.labelTS2.AutoSize = true;
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(-3, 371);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(42, 13);
            this.labelTS2.TabIndex = 3;
            this.labelTS2.Text = "Volume";
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(-3, 319);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(57, 13);
            this.labelTS1.TabIndex = 1;
            this.labelTS1.Text = "Frequency";
            // 
            // udRXAFreq
            // 
            this.udRXAFreq.DecimalPlaces = 6;
            this.udRXAFreq.Increment = new decimal(new int[] {
            1,
            0,
            0,
            393216});
            this.udRXAFreq.Location = new System.Drawing.Point(60, 317);
            this.udRXAFreq.Maximum = new decimal(new int[] {
            54,
            0,
            0,
            0});
            this.udRXAFreq.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.udRXAFreq.Name = "udRXAFreq";
            this.udRXAFreq.Size = new System.Drawing.Size(120, 20);
            this.udRXAFreq.TabIndex = 0;
            this.udRXAFreq.TinyStep = false;
            this.udRXAFreq.Value = new decimal(new int[] {
            142,
            0,
            0,
            65536});
            this.udRXAFreq.ValueChanged += new System.EventHandler(this.udRXAFreq_ValueChanged);
            // 
            // rxa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 451);
            this.Controls.Add(this.panelPanDisplay);
            this.Controls.Add(this.udRXAAGCGain);
            this.Controls.Add(this.labelTS5);
            this.Controls.Add(this.labelTS4);
            this.Controls.Add(this.udRXAMode);
            this.Controls.Add(this.labelTS3);
            this.Controls.Add(this.udRXAVolume);
            this.Controls.Add(this.labelTS2);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.udRXAFreq);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "rxa";
            this.Text = "rxa";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.rxa_FormClosing);
            this.Resize += new System.EventHandler(this.rxa_Resize);
            this.panelPanDisplay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panDisplay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRXAAGCGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRXAMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRXAVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRXAFreq)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDownTS udRXAFreq;
        private System.Windows.Forms.LabelTS labelTS1;
        private System.Windows.Forms.LabelTS labelTS2;
        private System.Windows.Forms.NumericUpDownTS udRXAVolume;
        private System.Windows.Forms.NumericUpDownTS udRXAMode;
        private System.Windows.Forms.LabelTS labelTS3;
        private System.Windows.Forms.LabelTS labelTS4;
        private System.Windows.Forms.NumericUpDownTS udRXAAGCGain;
        private System.Windows.Forms.LabelTS labelTS5;
        private System.Windows.Forms.PanelTS panelPanDisplay;
        private PanDisplay panDisplay;
       // private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
       // private WeifenLuo.WinFormsUI.Docking.VS2012LightTheme vS2012LightTheme1;
    }
}