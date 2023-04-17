namespace Thetis.KLJ
{
    partial class frmKLJ
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
            this.button1 = new System.Windows.Forms.Button();
            this.ipAddressControl1 = new NullFX.Controls.IPAddressControl();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(391, 26);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ipAddressControl1
            // 
            this.ipAddressControl1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ipAddressControl1.Location = new System.Drawing.Point(157, 26);
            this.ipAddressControl1.Name = "ipAddressControl1";
            this.ipAddressControl1.Size = new System.Drawing.Size(163, 22);
            this.ipAddressControl1.TabIndex = 2;
            this.ipAddressControl1.Text = "0.0.0.0";
            // 
            // frmKLJ
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 155);
            this.Controls.Add(this.ipAddressControl1);
            this.Controls.Add(this.button1);
            this.Name = "frmKLJ";
            this.Text = "frmKLJ";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private NullFX.Controls.IPAddressControl ipAddressControl1;
    }
}