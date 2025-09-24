namespace Thetis
{
    partial class AmpView
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series7 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AmpView));
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.chkAVShowGain = new System.Windows.Forms.CheckBoxTS();
            this.chkAVLowRes = new System.Windows.Forms.CheckBoxTS();
            this.chkAVPhaseZoom = new System.Windows.Forms.CheckBoxTS();
            this.chkStayOnTop = new System.Windows.Forms.CheckBoxTS();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chart1.BackColor = System.Drawing.Color.Black;
            chartArea1.AxisX.InterlacedColor = System.Drawing.Color.White;
            chartArea1.AxisX.LineColor = System.Drawing.Color.DimGray;
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.DimGray;
            chartArea1.AxisY.LineColor = System.Drawing.Color.DimGray;
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.DimGray;
            chartArea1.AxisY2.LineColor = System.Drawing.Color.DimGray;
            chartArea1.AxisY2.MajorGrid.LineColor = System.Drawing.Color.DimGray;
            chartArea1.BackColor = System.Drawing.Color.Black;
            chartArea1.BackSecondaryColor = System.Drawing.Color.White;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.ForeColor = System.Drawing.Color.LightSalmon;
            legend1.Name = "Legend1";
            legend1.Position.Auto = false;
            legend1.Position.Height = 15F;
            legend1.Position.Width = 16.99463F;
            legend1.Position.X = 70.5F;
            legend1.Position.Y = 70.5F;
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(0, 2);
            this.chart1.Name = "chart1";
            series1.BackSecondaryColor = System.Drawing.Color.White;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.Color = System.Drawing.Color.DimGray;
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.Name = "Ref";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series2.Color = System.Drawing.Color.DodgerBlue;
            series2.LabelForeColor = System.Drawing.Color.LightSalmon;
            series2.Legend = "Legend1";
            series2.LegendText = "Mag Amp";
            series2.Name = "_magamp";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            series3.Color = System.Drawing.Color.DodgerBlue;
            series3.IsVisibleInLegend = false;
            series3.Legend = "Legend1";
            series3.LegendText = "Mag Amp";
            series3.MarkerSize = 2;
            series3.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series3.Name = "MagAmp";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series4.Color = System.Drawing.Color.Gold;
            series4.Legend = "Legend1";
            series4.LegendText = "Phs Amp";
            series4.Name = "_phsamp";
            series5.ChartArea = "ChartArea1";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            series5.Color = System.Drawing.Color.Gold;
            series5.IsVisibleInLegend = false;
            series5.Legend = "Legend1";
            series5.LegendText = "Phs Amp";
            series5.MarkerSize = 2;
            series5.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series5.Name = "PhsAmp";
            series5.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series6.ChartArea = "ChartArea1";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series6.Color = System.Drawing.Color.Crimson;
            series6.Legend = "Legend1";
            series6.LegendText = "Mag Corr";
            series6.Name = "MagCorr";
            series7.ChartArea = "ChartArea1";
            series7.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series7.Color = System.Drawing.Color.Lime;
            series7.Legend = "Legend1";
            series7.LegendText = "Phs Corr";
            series7.Name = "PhsCorr";
            series7.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Series.Add(series3);
            this.chart1.Series.Add(series4);
            this.chart1.Series.Add(series5);
            this.chart1.Series.Add(series6);
            this.chart1.Series.Add(series7);
            this.chart1.Size = new System.Drawing.Size(564, 377);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // chkAVShowGain
            // 
            this.chkAVShowGain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkAVShowGain.AutoSize = true;
            this.chkAVShowGain.BackColor = System.Drawing.Color.Black;
            this.chkAVShowGain.ForeColor = System.Drawing.Color.LightSalmon;
            this.chkAVShowGain.Image = null;
            this.chkAVShowGain.Location = new System.Drawing.Point(7, 378);
            this.chkAVShowGain.Name = "chkAVShowGain";
            this.chkAVShowGain.Size = new System.Drawing.Size(78, 17);
            this.chkAVShowGain.TabIndex = 1;
            this.chkAVShowGain.Text = "Show Gain";
            this.chkAVShowGain.UseVisualStyleBackColor = false;
            this.chkAVShowGain.CheckedChanged += new System.EventHandler(this.chkAVShowGain_CheckedChanged);
            // 
            // chkAVLowRes
            // 
            this.chkAVLowRes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAVLowRes.AutoSize = true;
            this.chkAVLowRes.BackColor = System.Drawing.Color.Black;
            this.chkAVLowRes.Checked = true;
            this.chkAVLowRes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAVLowRes.ForeColor = System.Drawing.Color.LightSalmon;
            this.chkAVLowRes.Image = null;
            this.chkAVLowRes.Location = new System.Drawing.Point(404, 378);
            this.chkAVLowRes.Name = "chkAVLowRes";
            this.chkAVLowRes.Size = new System.Drawing.Size(68, 17);
            this.chkAVLowRes.TabIndex = 2;
            this.chkAVLowRes.Text = "Low Res";
            this.chkAVLowRes.UseVisualStyleBackColor = false;
            this.chkAVLowRes.CheckedChanged += new System.EventHandler(this.chkAVLowRes_CheckedChanged);
            // 
            // chkAVPhaseZoom
            // 
            this.chkAVPhaseZoom.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.chkAVPhaseZoom.AutoSize = true;
            this.chkAVPhaseZoom.BackColor = System.Drawing.Color.Black;
            this.chkAVPhaseZoom.ForeColor = System.Drawing.Color.LightSalmon;
            this.chkAVPhaseZoom.Image = null;
            this.chkAVPhaseZoom.Location = new System.Drawing.Point(242, 378);
            this.chkAVPhaseZoom.Name = "chkAVPhaseZoom";
            this.chkAVPhaseZoom.Size = new System.Drawing.Size(86, 17);
            this.chkAVPhaseZoom.TabIndex = 3;
            this.chkAVPhaseZoom.Text = "Phase Zoom";
            this.chkAVPhaseZoom.UseVisualStyleBackColor = false;
            this.chkAVPhaseZoom.CheckedChanged += new System.EventHandler(this.chkAVPhaseZoom_CheckedChanged);
            // 
            // chkStayOnTop
            // 
            this.chkStayOnTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkStayOnTop.AutoSize = true;
            this.chkStayOnTop.BackColor = System.Drawing.Color.Black;
            this.chkStayOnTop.ForeColor = System.Drawing.Color.LightSalmon;
            this.chkStayOnTop.Image = null;
            this.chkStayOnTop.Location = new System.Drawing.Point(490, 378);
            this.chkStayOnTop.Name = "chkStayOnTop";
            this.chkStayOnTop.Size = new System.Drawing.Size(62, 17);
            this.chkStayOnTop.TabIndex = 4;
            this.chkStayOnTop.Text = "On Top";
            this.chkStayOnTop.UseVisualStyleBackColor = false;
            this.chkStayOnTop.CheckedChanged += new System.EventHandler(this.chkStayOnTop_CheckedChanged);
            // 
            // AmpView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(564, 401);
            this.Controls.Add(this.chkStayOnTop);
            this.Controls.Add(this.chkAVPhaseZoom);
            this.Controls.Add(this.chkAVLowRes);
            this.Controls.Add(this.chkAVShowGain);
            this.Controls.Add(this.chart1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(440, 380);
            this.Name = "AmpView";
            this.Text = "AmpView 1.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AmpView_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AmpView_FormClosed);
            this.Load += new System.EventHandler(this.AmpView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBoxTS chkAVShowGain;
        private System.Windows.Forms.CheckBoxTS chkAVLowRes;
        private System.Windows.Forms.CheckBoxTS chkAVPhaseZoom;
        private System.Windows.Forms.CheckBoxTS chkStayOnTop;
    }
}