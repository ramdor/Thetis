namespace Thetis
{
    partial class ucMeter
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlBar = new System.Windows.Forms.Panel();
            this.btnSettings = new System.Windows.Forms.ButtonTS();
            this.btnPin = new System.Windows.Forms.ButtonTS();
            this.btnAxis = new System.Windows.Forms.ButtonTS();
            this.lblRX = new System.Windows.Forms.LabelTS();
            this.btnFloat = new System.Windows.Forms.ButtonTS();
            this.pbGrab = new System.Windows.Forms.PictureBox();
            this.pnlContainer = new System.Windows.Forms.Panel();
            this.pnlBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGrab)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBar
            // 
            this.pnlBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBar.BackColor = System.Drawing.Color.DimGray;
            this.pnlBar.Controls.Add(this.btnSettings);
            this.pnlBar.Controls.Add(this.btnPin);
            this.pnlBar.Controls.Add(this.btnAxis);
            this.pnlBar.Controls.Add(this.lblRX);
            this.pnlBar.Controls.Add(this.btnFloat);
            this.pnlBar.Location = new System.Drawing.Point(0, 0);
            this.pnlBar.Margin = new System.Windows.Forms.Padding(0);
            this.pnlBar.Name = "pnlBar";
            this.pnlBar.Size = new System.Drawing.Size(400, 18);
            this.pnlBar.TabIndex = 0;
            this.pnlBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlBar_MouseDown);
            this.pnlBar.MouseLeave += new System.EventHandler(this.pnlBar_MouseLeave);
            this.pnlBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlBar_MouseMove);
            this.pnlBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlBar_MouseUp);
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.Color.Transparent;
            this.btnSettings.BackgroundImage = global::Thetis.Properties.Resources.gear;
            this.btnSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnSettings.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettings.Image = null;
            this.btnSettings.Location = new System.Drawing.Point(3, 0);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(0);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Selectable = false;
            this.btnSettings.Size = new System.Drawing.Size(18, 18);
            this.btnSettings.TabIndex = 4;
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            this.btnSettings.MouseLeave += new System.EventHandler(this.btnSettings_MouseLeave);
            // 
            // btnPin
            // 
            this.btnPin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPin.BackColor = System.Drawing.Color.Transparent;
            this.btnPin.BackgroundImage = global::Thetis.Properties.Resources.pin_not_on_top;
            this.btnPin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnPin.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnPin.FlatAppearance.BorderSize = 0;
            this.btnPin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnPin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            this.btnPin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPin.Image = null;
            this.btnPin.Location = new System.Drawing.Point(334, 0);
            this.btnPin.Margin = new System.Windows.Forms.Padding(0);
            this.btnPin.Name = "btnPin";
            this.btnPin.Selectable = false;
            this.btnPin.Size = new System.Drawing.Size(18, 18);
            this.btnPin.TabIndex = 3;
            this.btnPin.UseVisualStyleBackColor = false;
            this.btnPin.Click += new System.EventHandler(this.btnPin_Click);
            this.btnPin.MouseLeave += new System.EventHandler(this.btnPin_MouseLeave);
            // 
            // btnAxis
            // 
            this.btnAxis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAxis.BackColor = System.Drawing.Color.Transparent;
            this.btnAxis.BackgroundImage = global::Thetis.Properties.Resources.dot;
            this.btnAxis.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnAxis.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnAxis.FlatAppearance.BorderSize = 0;
            this.btnAxis.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnAxis.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            this.btnAxis.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAxis.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAxis.Image = null;
            this.btnAxis.Location = new System.Drawing.Point(358, 0);
            this.btnAxis.Margin = new System.Windows.Forms.Padding(0);
            this.btnAxis.Name = "btnAxis";
            this.btnAxis.Selectable = false;
            this.btnAxis.Size = new System.Drawing.Size(18, 18);
            this.btnAxis.TabIndex = 2;
            this.btnAxis.UseVisualStyleBackColor = false;
            this.btnAxis.Click += new System.EventHandler(this.btnAxis_Click);
            this.btnAxis.MouseLeave += new System.EventHandler(this.btnAxis_MouseLeave);
            this.btnAxis.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnAxis_MouseUp);
            // 
            // lblRX
            // 
            this.lblRX.AutoSize = true;
            this.lblRX.BackColor = System.Drawing.Color.DimGray;
            this.lblRX.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRX.ForeColor = System.Drawing.Color.White;
            this.lblRX.Image = null;
            this.lblRX.Location = new System.Drawing.Point(24, 3);
            this.lblRX.Name = "lblRX";
            this.lblRX.Size = new System.Drawing.Size(31, 13);
            this.lblRX.TabIndex = 1;
            this.lblRX.Text = "RX0";
            this.lblRX.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblRX_MouseDown);
            this.lblRX.MouseLeave += new System.EventHandler(this.lblRX_MouseLeave);
            this.lblRX.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblRX_MouseMove);
            this.lblRX.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblRX_MouseUp);
            // 
            // btnFloat
            // 
            this.btnFloat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFloat.BackColor = System.Drawing.Color.Transparent;
            this.btnFloat.BackgroundImage = global::Thetis.Properties.Resources.dockIcon_dock;
            this.btnFloat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnFloat.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnFloat.FlatAppearance.BorderSize = 0;
            this.btnFloat.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnFloat.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            this.btnFloat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFloat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFloat.Image = null;
            this.btnFloat.Location = new System.Drawing.Point(380, 0);
            this.btnFloat.Margin = new System.Windows.Forms.Padding(0);
            this.btnFloat.Name = "btnFloat";
            this.btnFloat.Selectable = false;
            this.btnFloat.Size = new System.Drawing.Size(18, 18);
            this.btnFloat.TabIndex = 0;
            this.btnFloat.UseVisualStyleBackColor = false;
            this.btnFloat.Click += new System.EventHandler(this.btnFloat_Click);
            this.btnFloat.MouseLeave += new System.EventHandler(this.btnFloat_MouseLeave);
            // 
            // pbGrab
            // 
            this.pbGrab.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pbGrab.BackColor = System.Drawing.Color.Transparent;
            this.pbGrab.Image = global::Thetis.Properties.Resources.resizegrab;
            this.pbGrab.Location = new System.Drawing.Point(384, 184);
            this.pbGrab.Name = "pbGrab";
            this.pbGrab.Size = new System.Drawing.Size(16, 16);
            this.pbGrab.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbGrab.TabIndex = 1;
            this.pbGrab.TabStop = false;
            this.pbGrab.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbGrab_MouseDown);
            this.pbGrab.MouseEnter += new System.EventHandler(this.pbGrab_MouseEnter);
            this.pbGrab.MouseLeave += new System.EventHandler(this.pbGrab_MouseLeave);
            this.pbGrab.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbGrab_MouseMove);
            this.pbGrab.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbGrab_MouseUp);
            // 
            // pnlContainer
            // 
            this.pnlContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlContainer.Location = new System.Drawing.Point(102, 75);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(200, 67);
            this.pnlContainer.TabIndex = 2;
            this.pnlContainer.MouseLeave += new System.EventHandler(this.pnlContainer_MouseLeave);
            this.pnlContainer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlContainer_MouseMove);
            // 
            // ucMeter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pnlContainer);
            this.Controls.Add(this.pbGrab);
            this.Controls.Add(this.pnlBar);
            this.Name = "ucMeter";
            this.Size = new System.Drawing.Size(400, 200);
            this.LocationChanged += new System.EventHandler(this.ucMeter_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.ucMeter_SizeChanged);
            this.MouseLeave += new System.EventHandler(this.ucMeter_MouseLeave);
            this.pnlBar.ResumeLayout(false);
            this.pnlBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGrab)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlBar;
        private System.Windows.Forms.ButtonTS btnFloat;
        private System.Windows.Forms.PictureBox pbGrab;
        private System.Windows.Forms.LabelTS lblRX;
        private System.Windows.Forms.ButtonTS btnAxis;
        private System.Windows.Forms.ButtonTS btnPin;
        private System.Windows.Forms.ButtonTS btnSettings;
        private System.Windows.Forms.Panel pnlContainer;
    }
}
