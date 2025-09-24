namespace Thetis
{
    partial class wideband
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(wideband));
            this.contextMenuStripWideBand = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.canceltoolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.wbAvgtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wbUpdatetoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wbUpdatetoolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.wbFrameSizetoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wbFrameSizetoolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.panelwbDisplay = new System.Windows.Forms.PanelTS();
            this.wbdisplay = new Thetis.wbDisplay();
            this.contextMenuStripWideBand.SuspendLayout();
            this.panelwbDisplay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wbdisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStripWideBand
            // 
            this.contextMenuStripWideBand.BackColor = System.Drawing.Color.Black;
            this.contextMenuStripWideBand.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.contextMenuStripWideBand.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.contextMenuStripWideBand.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.canceltoolStripMenuItem1,
            this.toolStripSeparator1,
            this.wbAvgtoolStripMenuItem,
            this.wbUpdatetoolStripMenuItem,
            this.wbFrameSizetoolStripMenuItem});
            this.contextMenuStripWideBand.Name = "contextMenuStripWideBand";
            this.contextMenuStripWideBand.ShowCheckMargin = true;
            this.contextMenuStripWideBand.ShowImageMargin = false;
            this.contextMenuStripWideBand.Size = new System.Drawing.Size(136, 98);
            this.contextMenuStripWideBand.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.ContextMenuStrip_Closing);
            this.contextMenuStripWideBand.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripWideBand_Opening);
            // 
            // canceltoolStripMenuItem1
            // 
            this.canceltoolStripMenuItem1.BackColor = System.Drawing.Color.Black;
            this.canceltoolStripMenuItem1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.canceltoolStripMenuItem1.ForeColor = System.Drawing.Color.White;
            this.canceltoolStripMenuItem1.Name = "canceltoolStripMenuItem1";
            this.canceltoolStripMenuItem1.Size = new System.Drawing.Size(135, 22);
            this.canceltoolStripMenuItem1.Text = "Cancel";
            this.canceltoolStripMenuItem1.MouseEnter += new System.EventHandler(this.ToolStripMenuItem_MouseEnter);
            this.canceltoolStripMenuItem1.MouseLeave += new System.EventHandler(this.ToolStripMenuItem_MouseLeave);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(132, 6);
            // 
            // wbAvgtoolStripMenuItem
            // 
            this.wbAvgtoolStripMenuItem.BackColor = System.Drawing.Color.Black;
            this.wbAvgtoolStripMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.wbAvgtoolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.wbAvgtoolStripMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wbAvgtoolStripMenuItem.ForeColor = System.Drawing.Color.Goldenrod;
            this.wbAvgtoolStripMenuItem.Name = "wbAvgtoolStripMenuItem";
            this.wbAvgtoolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.wbAvgtoolStripMenuItem.Text = "Average";
            this.wbAvgtoolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.wbAvgtoolStripMenuItem.Click += new System.EventHandler(this.wbAvgtoolStripMenuItem_Click);
            this.wbAvgtoolStripMenuItem.MouseEnter += new System.EventHandler(this.ToolStripMenuItem_MouseEnter);
            this.wbAvgtoolStripMenuItem.MouseLeave += new System.EventHandler(this.ToolStripMenuItem_MouseLeave);
            // 
            // wbUpdatetoolStripMenuItem
            // 
            this.wbUpdatetoolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.wbUpdatetoolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wbUpdatetoolStripComboBox});
            this.wbUpdatetoolStripMenuItem.ForeColor = System.Drawing.Color.Goldenrod;
            this.wbUpdatetoolStripMenuItem.Name = "wbUpdatetoolStripMenuItem";
            this.wbUpdatetoolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.wbUpdatetoolStripMenuItem.Text = "Update Rate";
            this.wbUpdatetoolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.wbUpdatetoolStripMenuItem.MouseEnter += new System.EventHandler(this.ToolStripMenuItem_MouseEnter);
            this.wbUpdatetoolStripMenuItem.MouseLeave += new System.EventHandler(this.ToolStripMenuItem_MouseLeave);
            // 
            // wbUpdatetoolStripComboBox
            // 
            this.wbUpdatetoolStripComboBox.AutoSize = false;
            this.wbUpdatetoolStripComboBox.DropDownWidth = 20;
            this.wbUpdatetoolStripComboBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wbUpdatetoolStripComboBox.Items.AddRange(new object[] {
            "10",
            "15",
            "20",
            "25",
            "30",
            "35",
            "40",
            "45",
            "50",
            "55",
            "60"});
            this.wbUpdatetoolStripComboBox.Name = "wbUpdatetoolStripComboBox";
            this.wbUpdatetoolStripComboBox.Size = new System.Drawing.Size(40, 21);
            this.wbUpdatetoolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.wbUpdatetoolStripComboBox_SelectedIndexChanged);
            this.wbUpdatetoolStripComboBox.MouseEnter += new System.EventHandler(this.ToolStripMenuItem_MouseEnter);
            this.wbUpdatetoolStripComboBox.MouseLeave += new System.EventHandler(this.ToolStripMenuItem_MouseLeave);
            // 
            // wbFrameSizetoolStripMenuItem
            // 
            this.wbFrameSizetoolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wbFrameSizetoolStripComboBox});
            this.wbFrameSizetoolStripMenuItem.ForeColor = System.Drawing.Color.Goldenrod;
            this.wbFrameSizetoolStripMenuItem.Name = "wbFrameSizetoolStripMenuItem";
            this.wbFrameSizetoolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.wbFrameSizetoolStripMenuItem.Text = "Frame Size";
            this.wbFrameSizetoolStripMenuItem.MouseEnter += new System.EventHandler(this.ToolStripMenuItem_MouseEnter);
            this.wbFrameSizetoolStripMenuItem.MouseLeave += new System.EventHandler(this.ToolStripMenuItem_MouseLeave);
            // 
            // wbFrameSizetoolStripComboBox
            // 
            this.wbFrameSizetoolStripComboBox.AutoSize = false;
            this.wbFrameSizetoolStripComboBox.DropDownWidth = 50;
            this.wbFrameSizetoolStripComboBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wbFrameSizetoolStripComboBox.Items.AddRange(new object[] {
            "8",
            "32"});
            this.wbFrameSizetoolStripComboBox.Name = "wbFrameSizetoolStripComboBox";
            this.wbFrameSizetoolStripComboBox.Size = new System.Drawing.Size(40, 21);
            this.wbFrameSizetoolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.wbFrameSizetoolStripComboBox_SelectedIndexChanged);
            this.wbFrameSizetoolStripComboBox.MouseEnter += new System.EventHandler(this.ToolStripMenuItem_MouseEnter);
            this.wbFrameSizetoolStripComboBox.MouseLeave += new System.EventHandler(this.ToolStripMenuItem_MouseLeave);
            // 
            // panelwbDisplay
            // 
            this.panelwbDisplay.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.panelwbDisplay.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.panelwbDisplay.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelwbDisplay.BackColor = System.Drawing.Color.Transparent;
            this.panelwbDisplay.Controls.Add(this.wbdisplay);
            this.panelwbDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelwbDisplay.Location = new System.Drawing.Point(0, 0);
            this.panelwbDisplay.Name = "panelwbDisplay";
            this.panelwbDisplay.Size = new System.Drawing.Size(801, 302);
            this.panelwbDisplay.TabIndex = 0;
            // 
            // wbdisplay
            // 
            this.wbdisplay.ADC = 0;
            this.wbdisplay.AGCHang = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.wbdisplay.AGCKnee = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.wbdisplay.AlexPreampOffset = 0F;
            this.wbdisplay.AverageOn = true;
            this.wbdisplay.AvTau = 0.12D;
            this.wbdisplay.BackColor = System.Drawing.Color.Black;
            this.wbdisplay.BandEdgeColor = System.Drawing.Color.Red;
            this.wbdisplay.ClickTuneFilter = true;
            this.wbdisplay.ColorScheme = Thetis.ColorScheme.enhanced;
            this.wbdisplay.cOnsole = null;
            this.wbdisplay.ContextMenuStrip = this.contextMenuStripWideBand;
            this.wbdisplay.CurrentClickTuneMode = Thetis.ClickTuneMode.Off;
            this.wbdisplay.CurrentDisplayMode = Thetis.DisplayMode.PANADAPTER;
            this.wbdisplay.CurrentModel = Thetis.HPSDRModel.HERMES;
            this.wbdisplay.CurrentRegion = Thetis.FRSRegion.US;
            this.wbdisplay.CWPitch = 600;
            this.wbdisplay.DataLineColor = System.Drawing.Color.White;
            this.wbdisplay.DataReady = false;
            this.wbdisplay.DBMScalePanRect = new System.Drawing.Rectangle(65, 0, 35, 30);
            this.wbdisplay.DisplayAGCHangLine = true;
            this.wbdisplay.DisplayAvgBlocks = 5;
            this.wbdisplay.DisplayBackgroundColor = System.Drawing.Color.Black;
            this.wbdisplay.DisplayCursorX = 0;
            this.wbdisplay.DisplayCursorY = 0;
            this.wbdisplay.DisplayDuplex = false;
            this.wbdisplay.DisplayFilterColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.wbdisplay.DisplayLabelAlign = Thetis.DisplayLabelAlignment.LEFT;
            this.wbdisplay.DisplayLineWidth = 1F;
            this.wbdisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wbdisplay.FFTSize = 16384;
            this.wbdisplay.FilterRect = new System.Drawing.Rectangle(0, 0, 0, 0);
            this.wbdisplay.FrameRate = 15;
            this.wbdisplay.FrameSize = "32";
            this.wbdisplay.FREQ = 0D;
            this.wbdisplay.FreqDiff = 0;
            this.wbdisplay.FreqRulerPosition = 1F;
            this.wbdisplay.FreqScalePanRect = new System.Drawing.Rectangle(0, 282, 801, 20);
            this.wbdisplay.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.wbdisplay.GridControl = true;
            this.wbdisplay.GridPenDark = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.wbdisplay.GridTextColor = System.Drawing.Color.Yellow;
            this.wbdisplay.GridZeroColor = System.Drawing.Color.Red;
            this.wbdisplay.HGridColor = System.Drawing.Color.White;
            this.wbdisplay.HighSWR = false;
            this.wbdisplay.KaiserPi = 14D;
            this.wbdisplay.LinCor = 2;
            this.wbdisplay.LinLogCor = -14;
            this.wbdisplay.Location = new System.Drawing.Point(0, 0);
            this.wbdisplay.MaxX = 0F;
            this.wbdisplay.MaxY = 0F;
            this.wbdisplay.MOX = false;
            this.wbdisplay.Name = "wbdisplay";
            this.wbdisplay.NReceivers = 2;
            this.wbdisplay.PanFill = true;
            this.wbdisplay.PanFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(127)))));
            this.wbdisplay.PanRect = new System.Drawing.Rectangle(0, 0, 801, 282);
            this.wbdisplay.PanSlider = 0.5D;
            this.wbdisplay.PeakOn = false;
            this.wbdisplay.Pixels = 801;
            this.wbdisplay.PreampOffset = 0F;
            this.wbdisplay.ReverseWaterfall = false;
            this.wbdisplay.RIT = 0;
            this.wbdisplay.RX1HangSpectrumLine = true;
            this.wbdisplay.RXDisplayCalOffset = -40.1F;
            this.wbdisplay.RXDSPMode = Thetis.DSPMode.USB;
            this.wbdisplay.RXFFTSizeOffset = 0F;
            this.wbdisplay.RXFilterHigh = 0;
            this.wbdisplay.RXFilterLow = 0;
            this.wbdisplay.SampleRate = 122880000;
            this.wbdisplay.SecScalePanRect = new System.Drawing.Rectangle(0, 302, 45, 0);
            this.wbdisplay.ShowAGC = true;
            this.wbdisplay.ShowCTHLine = false;
            this.wbdisplay.ShowCWZeroLine = false;
            this.wbdisplay.ShowFreqOffset = false;
            this.wbdisplay.Size = new System.Drawing.Size(801, 302);
            this.wbdisplay.SpectrumGridMax = -50;
            this.wbdisplay.SpectrumGridMin = -170;
            this.wbdisplay.SpectrumGridStep = 10;
            this.wbdisplay.SpectrumLine = true;
            this.wbdisplay.SplitDisplay = false;
            this.wbdisplay.SplitEnabled = false;
            this.wbdisplay.SubRX1Enabled = false;
            this.wbdisplay.SubRXFilterColor = System.Drawing.Color.Blue;
            this.wbdisplay.SubRXZeroLine = System.Drawing.Color.LightSkyBlue;
            this.wbdisplay.TabIndex = 0;
            this.wbdisplay.TabStop = false;
            this.wbdisplay.Target = null;
            this.wbdisplay.TopSize = 0;
            this.wbdisplay.UpdateRate = "15";
            this.wbdisplay.VFOASub = ((long)(0));
            this.wbdisplay.VFOHz = ((long)(10000000));
            this.wbdisplay.WaterfallAGC = false;
            this.wbdisplay.WaterfallAvgBlocks = 18;
            this.wbdisplay.WaterfallDataReady = false;
            this.wbdisplay.WaterfallHighColor = System.Drawing.Color.Yellow;
            this.wbdisplay.WaterfallHighThreshold = -80F;
            this.wbdisplay.WaterfallLowColor = System.Drawing.Color.Black;
            this.wbdisplay.WaterfallLowThreshold = -130F;
            this.wbdisplay.WaterfallMidColor = System.Drawing.Color.Red;
            this.wbdisplay.WaterfallRect = new System.Drawing.Rectangle(0, 302, 801, 1);
            this.wbdisplay.WaterfallUpdatePeriod = 100;
            this.wbdisplay.WindowType = 6;
            this.wbdisplay.ZoomSlider = 0D;
            this.wbdisplay.Resize += new System.EventHandler(this.wbdisplay_Resize);
            // 
            // wideband
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(801, 302);
            this.Controls.Add(this.panelwbDisplay);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "wideband";
            this.Text = "wideband";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.wideband_FormClosing);
            this.Resize += new System.EventHandler(this.wideband_Resize);
            this.contextMenuStripWideBand.ResumeLayout(false);
            this.panelwbDisplay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.wbdisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PanelTS panelwbDisplay;
        private wbDisplay wbdisplay;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripWideBand;
        private System.Windows.Forms.ToolStripMenuItem canceltoolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem wbAvgtoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wbUpdatetoolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox wbUpdatetoolStripComboBox;
        private System.Windows.Forms.ToolStripMenuItem wbFrameSizetoolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox wbFrameSizetoolStripComboBox;
    }
}