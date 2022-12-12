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
            this.btnAxis = new System.Windows.Forms.ButtonTS();
            this.lblRX = new System.Windows.Forms.LabelTS();
            this.btnFloat = new System.Windows.Forms.ButtonTS();
            this.pbGrab = new System.Windows.Forms.PictureBox();
            this.picContainer = new System.Windows.Forms.PictureBox();
            this.pnlBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGrab)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picContainer)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlBar
            // 
            this.pnlBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBar.BackColor = System.Drawing.Color.DimGray;
            this.pnlBar.Controls.Add(this.btnAxis);
            this.pnlBar.Controls.Add(this.lblRX);
            this.pnlBar.Controls.Add(this.btnFloat);
            this.pnlBar.Location = new System.Drawing.Point(0, 0);
            this.pnlBar.Margin = new System.Windows.Forms.Padding(0);
            this.pnlBar.Name = "pnlBar";
            this.pnlBar.Size = new System.Drawing.Size(407, 18);
            this.pnlBar.TabIndex = 0;
            this.pnlBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlBar_MouseDown);
            this.pnlBar.MouseLeave += new System.EventHandler(this.pnlBar_MouseLeave);
            this.pnlBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlBar_MouseMove);
            this.pnlBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlBar_MouseUp);
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
            this.btnAxis.Location = new System.Drawing.Point(365, 0);
            this.btnAxis.Margin = new System.Windows.Forms.Padding(0);
            this.btnAxis.Name = "btnAxis";
            this.btnAxis.Selectable = false;
            this.btnAxis.Size = new System.Drawing.Size(18, 18);
            this.btnAxis.TabIndex = 2;
            this.btnAxis.UseVisualStyleBackColor = false;
            this.btnAxis.Click += new System.EventHandler(this.btnAxis_Click);
            // 
            // lblRX
            // 
            this.lblRX.AutoSize = true;
            this.lblRX.BackColor = System.Drawing.Color.DimGray;
            this.lblRX.ForeColor = System.Drawing.Color.White;
            this.lblRX.Image = null;
            this.lblRX.Location = new System.Drawing.Point(3, 3);
            this.lblRX.Name = "lblRX";
            this.lblRX.Size = new System.Drawing.Size(28, 13);
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
            this.btnFloat.Location = new System.Drawing.Point(387, 0);
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
            this.pbGrab.Location = new System.Drawing.Point(391, 387);
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
            // picContainer
            // 
            this.picContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picContainer.BackColor = System.Drawing.Color.Black;
            this.picContainer.Location = new System.Drawing.Point(0, 0);
            this.picContainer.Margin = new System.Windows.Forms.Padding(0);
            this.picContainer.Name = "picContainer";
            this.picContainer.Size = new System.Drawing.Size(407, 403);
            this.picContainer.TabIndex = 1;
            this.picContainer.TabStop = false;
            this.picContainer.MouseLeave += new System.EventHandler(this.picContainer_MouseLeave);
            this.picContainer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picContainer_MouseMove);
            // 
            // ucMeter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.pbGrab);
            this.Controls.Add(this.pnlBar);
            this.Controls.Add(this.picContainer);
            this.Name = "ucMeter";
            this.Size = new System.Drawing.Size(407, 403);
            this.LocationChanged += new System.EventHandler(this.ucMeter_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.ucMeter_SizeChanged);
            this.pnlBar.ResumeLayout(false);
            this.pnlBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbGrab)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picContainer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlBar;
        private System.Windows.Forms.ButtonTS btnFloat;
        private System.Windows.Forms.PictureBox pbGrab;
        private System.Windows.Forms.PictureBox picContainer;
        private System.Windows.Forms.LabelTS lblRX;
        private System.Windows.Forms.ButtonTS btnAxis;
    }
}
