namespace Thetis
{
    partial class frmBandwidth
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
            this.timerReadBandwidth = new System.Windows.Forms.Timer(this.components);
            this.radMbit = new System.Windows.Forms.RadioButtonTS();
            this.radKB = new System.Windows.Forms.RadioButtonTS();
            this.ucBandwidthView = new Thetis.ucBandwidthView();
            this.SuspendLayout();
            // 
            // timerReadBandwidth
            // 
            this.timerReadBandwidth.Interval = 500;
            this.timerReadBandwidth.Tick += new System.EventHandler(this.timerReadBandwidth_Tick);
            // 
            // radMbit
            // 
            this.radMbit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radMbit.AutoSize = true;
            this.radMbit.Checked = true;
            this.radMbit.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.radMbit.Image = null;
            this.radMbit.Location = new System.Drawing.Point(63, 139);
            this.radMbit.Name = "radMbit";
            this.radMbit.Size = new System.Drawing.Size(55, 17);
            this.radMbit.TabIndex = 2;
            this.radMbit.TabStop = true;
            this.radMbit.Text = "Mbit/s";
            this.radMbit.UseVisualStyleBackColor = true;
            this.radMbit.CheckedChanged += new System.EventHandler(this.radUnits_CheckedChanged);
            // 
            // radKB
            // 
            this.radKB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radKB.AutoSize = true;
            this.radKB.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.radKB.Image = null;
            this.radKB.Location = new System.Drawing.Point(9, 139);
            this.radKB.Name = "radKB";
            this.radKB.Size = new System.Drawing.Size(48, 17);
            this.radKB.TabIndex = 1;
            this.radKB.Text = "kB/s";
            this.radKB.UseVisualStyleBackColor = true;
            this.radKB.CheckedChanged += new System.EventHandler(this.radUnits_CheckedChanged);
            // 
            // ucBandwidthView
            // 
            this.ucBandwidthView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ucBandwidthView.BackColor = System.Drawing.Color.Black;
            this.ucBandwidthView.DisplayUnits = Thetis.ucBandwidthView.BandwidthUnits.Mbitps;
            this.ucBandwidthView.EnableSmoothing = false;
            this.ucBandwidthView.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.ucBandwidthView.ForeColor = System.Drawing.Color.White;
            this.ucBandwidthView.HistorySeconds = 60;
            this.ucBandwidthView.Location = new System.Drawing.Point(0, 6);
            this.ucBandwidthView.Name = "ucBandwidthView";
            this.ucBandwidthView.ShowGrid = true;
            this.ucBandwidthView.Size = new System.Drawing.Size(304, 127);
            this.ucBandwidthView.SmoothingFactor = 0.2D;
            this.ucBandwidthView.TabIndex = 0;
            // 
            // frmBandwidth
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(304, 161);
            this.Controls.Add(this.radMbit);
            this.Controls.Add(this.radKB);
            this.Controls.Add(this.ucBandwidthView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximumSize = new System.Drawing.Size(640, 480);
            this.MinimumSize = new System.Drawing.Size(320, 200);
            this.Name = "frmBandwidth";
            this.Text = "Bandwidth";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmBandwidth_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ucBandwidthView ucBandwidthView;
        private System.Windows.Forms.RadioButtonTS radKB;
        private System.Windows.Forms.RadioButtonTS radMbit;
        private System.Windows.Forms.Timer timerReadBandwidth;
    }
}