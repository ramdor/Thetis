namespace Thetis
{
    partial class ucInfoBar
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
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblFB = new System.Windows.Forms.LabelTS();
            this.lblPS = new System.Windows.Forms.LabelTS();
            this.chkButton2 = new System.Windows.Forms.CheckBoxTS();
            this.chkButton1 = new System.Windows.Forms.CheckBoxTS();
            this.lblSplitter = new System.Windows.Forms.LabelTS();
            this.lblWarning = new System.Windows.Forms.LabelTS();
            this.lblRight3 = new System.Windows.Forms.LabelTS();
            this.lblRight2 = new System.Windows.Forms.LabelTS();
            this.lblRight1 = new System.Windows.Forms.LabelTS();
            this.lblLeft3 = new System.Windows.Forms.LabelTS();
            this.lblLeft2 = new System.Windows.Forms.LabelTS();
            this.lblLeft1 = new System.Windows.Forms.LabelTS();
            this.lblPageNo = new System.Windows.Forms.LabelTS();
            this.SuspendLayout();
            // 
            // lblFB
            // 
            this.lblFB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.lblFB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFB.ForeColor = System.Drawing.Color.Black;
            this.lblFB.Image = null;
            this.lblFB.Location = new System.Drawing.Point(801, 0);
            this.lblFB.MinimumSize = new System.Drawing.Size(44, 24);
            this.lblFB.Name = "lblFB";
            this.lblFB.Size = new System.Drawing.Size(44, 24);
            this.lblFB.TabIndex = 34;
            this.lblFB.Text = "FB";
            this.lblFB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblFB, "Feedback level in order. Blue > 181, Green > 128, Yellow > 90, Red >= 0");
            this.lblFB.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblFB_MouseDown);
            // 
            // lblPS
            // 
            this.lblPS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.lblPS.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPS.ForeColor = System.Drawing.Color.Black;
            this.lblPS.Image = null;
            this.lblPS.Location = new System.Drawing.Point(845, 0);
            this.lblPS.MinimumSize = new System.Drawing.Size(44, 24);
            this.lblPS.Name = "lblPS";
            this.lblPS.Size = new System.Drawing.Size(44, 24);
            this.lblPS.TabIndex = 33;
            this.lblPS.Text = "Pure Signal2";
            this.lblPS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblPS, "PS2 is correcting if \'Correct\' is shown");
            this.lblPS.Click += new System.EventHandler(this.lblPS_Click);
            // 
            // chkButton2
            // 
            this.chkButton2.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkButton2.FlatAppearance.BorderSize = 0;
            this.chkButton2.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
            this.chkButton2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.chkButton2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.chkButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkButton2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.chkButton2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.chkButton2.Image = null;
            this.chkButton2.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.chkButton2.Location = new System.Drawing.Point(59, 1);
            this.chkButton2.Name = "chkButton2";
            this.chkButton2.Size = new System.Drawing.Size(50, 23);
            this.chkButton2.TabIndex = 32;
            this.chkButton2.Text = "Peak";
            this.chkButton2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.chkButton2, "Show/hide active peaks");
            this.chkButton2.CheckedChanged += new System.EventHandler(this.chkButton2_CheckedChanged);
            this.chkButton2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.chkButton2_MouseDown);
            // 
            // chkButton1
            // 
            this.chkButton1.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkButton1.FlatAppearance.BorderSize = 0;
            this.chkButton1.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
            this.chkButton1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.chkButton1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
            this.chkButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.chkButton1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.chkButton1.Image = null;
            this.chkButton1.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.chkButton1.Location = new System.Drawing.Point(3, 1);
            this.chkButton1.Name = "chkButton1";
            this.chkButton1.Size = new System.Drawing.Size(50, 23);
            this.chkButton1.TabIndex = 31;
            this.chkButton1.Text = "Blobs";
            this.chkButton1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.chkButton1, "Show/hide spectrum peak blobs");
            this.chkButton1.CheckedChanged += new System.EventHandler(this.chkButton1_CheckedChanged);
            this.chkButton1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.chkButton1_MouseDown);
            // 
            // lblSplitter
            // 
            this.lblSplitter.BackColor = System.Drawing.Color.White;
            this.lblSplitter.ForeColor = System.Drawing.Color.White;
            this.lblSplitter.Image = null;
            this.lblSplitter.Location = new System.Drawing.Point(796, 0);
            this.lblSplitter.Margin = new System.Windows.Forms.Padding(0);
            this.lblSplitter.Name = "lblSplitter";
            this.lblSplitter.Size = new System.Drawing.Size(5, 24);
            this.lblSplitter.TabIndex = 42;
            this.lblSplitter.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblSplitter_MouseDown);
            this.lblSplitter.MouseEnter += new System.EventHandler(this.lblSplitter_MouseEnter);
            this.lblSplitter.MouseLeave += new System.EventHandler(this.lblSplitter_MouseLeave);
            this.lblSplitter.MouseHover += new System.EventHandler(this.lblSplitter_MouseHover);
            this.lblSplitter.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblSplitter_MouseMove);
            this.lblSplitter.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblSplitter_MouseUp);
            // 
            // lblWarning
            // 
            this.lblWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarning.ForeColor = System.Drawing.Color.Red;
            this.lblWarning.Image = null;
            this.lblWarning.Location = new System.Drawing.Point(138, 0);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(654, 24);
            this.lblWarning.TabIndex = 41;
            this.lblWarning.Text = "Warning";
            this.lblWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblWarning.Visible = false;
            // 
            // lblRight3
            // 
            this.lblRight3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRight3.BackColor = System.Drawing.Color.Black;
            this.lblRight3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRight3.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblRight3.Image = null;
            this.lblRight3.Location = new System.Drawing.Point(680, 3);
            this.lblRight3.Name = "lblRight3";
            this.lblRight3.Size = new System.Drawing.Size(112, 15);
            this.lblRight3.TabIndex = 40;
            this.lblRight3.Text = "000000000000";
            this.lblRight3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRight3.Click += new System.EventHandler(this.InfoBar_Click);
            // 
            // lblRight2
            // 
            this.lblRight2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRight2.BackColor = System.Drawing.Color.Black;
            this.lblRight2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRight2.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblRight2.Image = null;
            this.lblRight2.Location = new System.Drawing.Point(592, 3);
            this.lblRight2.Name = "lblRight2";
            this.lblRight2.Size = new System.Drawing.Size(88, 15);
            this.lblRight2.TabIndex = 39;
            this.lblRight2.Text = "000000000000";
            this.lblRight2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRight2.Click += new System.EventHandler(this.InfoBar_Click);
            // 
            // lblRight1
            // 
            this.lblRight1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRight1.BackColor = System.Drawing.Color.Black;
            this.lblRight1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRight1.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblRight1.Image = null;
            this.lblRight1.Location = new System.Drawing.Point(504, 3);
            this.lblRight1.Name = "lblRight1";
            this.lblRight1.Size = new System.Drawing.Size(88, 15);
            this.lblRight1.TabIndex = 38;
            this.lblRight1.Text = "000000000000";
            this.lblRight1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblRight1.Click += new System.EventHandler(this.InfoBar_Click);
            // 
            // lblLeft3
            // 
            this.lblLeft3.BackColor = System.Drawing.Color.Black;
            this.lblLeft3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLeft3.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblLeft3.Image = null;
            this.lblLeft3.Location = new System.Drawing.Point(294, 4);
            this.lblLeft3.Name = "lblLeft3";
            this.lblLeft3.Size = new System.Drawing.Size(112, 15);
            this.lblLeft3.TabIndex = 37;
            this.lblLeft3.Text = "000000000000";
            this.lblLeft3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLeft3.Click += new System.EventHandler(this.InfoBar_Click);
            // 
            // lblLeft2
            // 
            this.lblLeft2.BackColor = System.Drawing.Color.Black;
            this.lblLeft2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLeft2.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblLeft2.Image = null;
            this.lblLeft2.Location = new System.Drawing.Point(226, 4);
            this.lblLeft2.Name = "lblLeft2";
            this.lblLeft2.Size = new System.Drawing.Size(68, 15);
            this.lblLeft2.TabIndex = 36;
            this.lblLeft2.Text = "0000sec";
            this.lblLeft2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLeft2.Click += new System.EventHandler(this.InfoBar_Click);
            // 
            // lblLeft1
            // 
            this.lblLeft1.BackColor = System.Drawing.Color.Black;
            this.lblLeft1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLeft1.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblLeft1.Image = null;
            this.lblLeft1.Location = new System.Drawing.Point(138, 4);
            this.lblLeft1.Name = "lblLeft1";
            this.lblLeft1.Size = new System.Drawing.Size(88, 15);
            this.lblLeft1.TabIndex = 35;
            this.lblLeft1.Text = "000000000000";
            this.lblLeft1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLeft1.Click += new System.EventHandler(this.InfoBar_Click);
            // 
            // lblPageNo
            // 
            this.lblPageNo.AutoSize = true;
            this.lblPageNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageNo.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblPageNo.Image = null;
            this.lblPageNo.Location = new System.Drawing.Point(115, 6);
            this.lblPageNo.Name = "lblPageNo";
            this.lblPageNo.Size = new System.Drawing.Size(24, 13);
            this.lblPageNo.TabIndex = 43;
            this.lblPageNo.Text = "2/2";
            this.lblPageNo.Click += new System.EventHandler(this.InfoBar_Click);
            // 
            // ucInfoBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.Controls.Add(this.lblPageNo);
            this.Controls.Add(this.lblSplitter);
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.lblRight3);
            this.Controls.Add(this.lblRight2);
            this.Controls.Add(this.lblRight1);
            this.Controls.Add(this.lblLeft3);
            this.Controls.Add(this.lblLeft2);
            this.Controls.Add(this.lblLeft1);
            this.Controls.Add(this.lblFB);
            this.Controls.Add(this.lblPS);
            this.Controls.Add(this.chkButton2);
            this.Controls.Add(this.chkButton1);
            this.Name = "ucInfoBar";
            this.Size = new System.Drawing.Size(889, 24);
            this.Click += new System.EventHandler(this.InfoBar_Click);
            this.Resize += new System.EventHandler(this.InfoBar_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBoxTS chkButton1;
        private System.Windows.Forms.CheckBoxTS chkButton2;
        private System.Windows.Forms.LabelTS lblPS;
        private System.Windows.Forms.LabelTS lblFB;
        private System.Windows.Forms.LabelTS lblLeft1;
        private System.Windows.Forms.LabelTS lblLeft2;
        private System.Windows.Forms.LabelTS lblLeft3;
        private System.Windows.Forms.LabelTS lblRight3;
        private System.Windows.Forms.LabelTS lblRight2;
        private System.Windows.Forms.LabelTS lblRight1;
        private System.Windows.Forms.LabelTS lblWarning;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.LabelTS lblSplitter;
        private System.Windows.Forms.LabelTS lblPageNo;
    }
}
