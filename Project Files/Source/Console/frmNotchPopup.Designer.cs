namespace Thetis
{
    partial class frmNotchPopup
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
            this.trkWidth = new System.Windows.Forms.TrackBarTS();
            this.lblWidth = new System.Windows.Forms.LabelTS();
            this.btn200 = new System.Windows.Forms.ButtonTS();
            this.btn100 = new System.Windows.Forms.ButtonTS();
            this.btn50 = new System.Windows.Forms.ButtonTS();
            this.btn25 = new System.Windows.Forms.ButtonTS();
            this.chkActive = new System.Windows.Forms.CheckBoxTS();
            this.btnDelete = new System.Windows.Forms.ButtonTS();
            ((System.ComponentModel.ISupportInitialize)(this.trkWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // trkWidth
            // 
            this.trkWidth.AutoSize = false;
            this.trkWidth.Location = new System.Drawing.Point(5, 86);
            this.trkWidth.Name = "trkWidth";
            this.trkWidth.Size = new System.Drawing.Size(126, 20);
            this.trkWidth.TabIndex = 8;
            this.trkWidth.Scroll += new System.EventHandler(this.TrkWidth_Scroll);
            // 
            // lblWidth
            // 
            this.lblWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWidth.Image = null;
            this.lblWidth.Location = new System.Drawing.Point(5, 62);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(127, 21);
            this.lblWidth.TabIndex = 7;
            this.lblWidth.Text = "10000 Hz";
            this.lblWidth.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // btn200
            // 
            this.btn200.Image = null;
            this.btn200.Location = new System.Drawing.Point(71, 141);
            this.btn200.Name = "btn200";
            this.btn200.Size = new System.Drawing.Size(60, 23);
            this.btn200.TabIndex = 5;
            this.btn200.Text = "200Hz";
            this.btn200.UseVisualStyleBackColor = true;
            this.btn200.Click += new System.EventHandler(this.Btn200_Click);
            // 
            // btn100
            // 
            this.btn100.Image = null;
            this.btn100.Location = new System.Drawing.Point(5, 141);
            this.btn100.Name = "btn100";
            this.btn100.Size = new System.Drawing.Size(60, 23);
            this.btn100.TabIndex = 4;
            this.btn100.Text = "100Hz";
            this.btn100.UseVisualStyleBackColor = true;
            this.btn100.Click += new System.EventHandler(this.Btn100_Click);
            // 
            // btn50
            // 
            this.btn50.Image = null;
            this.btn50.Location = new System.Drawing.Point(71, 112);
            this.btn50.Name = "btn50";
            this.btn50.Size = new System.Drawing.Size(60, 23);
            this.btn50.TabIndex = 3;
            this.btn50.Text = "50Hz";
            this.btn50.UseVisualStyleBackColor = true;
            this.btn50.Click += new System.EventHandler(this.Btn50_Click);
            // 
            // btn25
            // 
            this.btn25.Image = null;
            this.btn25.Location = new System.Drawing.Point(5, 112);
            this.btn25.Name = "btn25";
            this.btn25.Size = new System.Drawing.Size(60, 23);
            this.btn25.TabIndex = 2;
            this.btn25.Text = "25Hz";
            this.btn25.UseVisualStyleBackColor = true;
            this.btn25.Click += new System.EventHandler(this.Btn25_Click);
            // 
            // chkActive
            // 
            this.chkActive.AutoSize = true;
            this.chkActive.Image = null;
            this.chkActive.Location = new System.Drawing.Point(40, 42);
            this.chkActive.Name = "chkActive";
            this.chkActive.Size = new System.Drawing.Size(56, 17);
            this.chkActive.TabIndex = 1;
            this.chkActive.Text = "Active";
            this.chkActive.UseVisualStyleBackColor = true;
            this.chkActive.CheckedChanged += new System.EventHandler(this.ChkActive_CheckedChanged);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnDelete.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnDelete.Image = null;
            this.btnDelete.Location = new System.Drawing.Point(3, 4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(129, 31);
            this.btnDelete.TabIndex = 0;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // frmNotchPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(136, 169);
            this.ControlBox = false;
            this.Controls.Add(this.trkWidth);
            this.Controls.Add(this.lblWidth);
            this.Controls.Add(this.btn200);
            this.Controls.Add(this.btn100);
            this.Controls.Add(this.btn50);
            this.Controls.Add(this.btn25);
            this.Controls.Add(this.chkActive);
            this.Controls.Add(this.btnDelete);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmNotchPopup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "NotchPopup";
            this.Deactivate += new System.EventHandler(this.FrmNotchPopup_Deactivate);
            ((System.ComponentModel.ISupportInitialize)(this.trkWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ButtonTS btnDelete;
        private System.Windows.Forms.CheckBoxTS chkActive;
        private System.Windows.Forms.ButtonTS btn25;
        private System.Windows.Forms.ButtonTS btn50;
        private System.Windows.Forms.ButtonTS btn200;
        private System.Windows.Forms.ButtonTS btn100;
        private System.Windows.Forms.LabelTS lblWidth;
        private System.Windows.Forms.TrackBarTS trkWidth;
    }
}