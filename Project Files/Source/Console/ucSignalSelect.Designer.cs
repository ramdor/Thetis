namespace Thetis
{
    partial class ucSignalSelect
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucSignalSelect));
            this.radSig = new System.Windows.Forms.RadioButtonTS();
            this.radSigAvg = new System.Windows.Forms.RadioButtonTS();
            this.radSigMaxBin = new System.Windows.Forms.RadioButtonTS();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // radSig
            // 
            this.radSig.AutoSize = true;
            this.radSig.Image = null;
            this.radSig.Location = new System.Drawing.Point(3, 3);
            this.radSig.Name = "radSig";
            this.radSig.Size = new System.Drawing.Size(40, 17);
            this.radSig.TabIndex = 0;
            this.radSig.TabStop = true;
            this.radSig.Text = "Sig";
            this.toolTip1.SetToolTip(this.radSig, "Signal Peak");
            this.radSig.UseVisualStyleBackColor = true;
            this.radSig.CheckedChanged += new System.EventHandler(this.radSig_CheckedChanged);
            // 
            // radSigAvg
            // 
            this.radSigAvg.AutoSize = true;
            this.radSigAvg.Image = null;
            this.radSigAvg.Location = new System.Drawing.Point(49, 3);
            this.radSigAvg.Name = "radSigAvg";
            this.radSigAvg.Size = new System.Drawing.Size(44, 17);
            this.radSigAvg.TabIndex = 1;
            this.radSigAvg.TabStop = true;
            this.radSigAvg.Text = "Avg";
            this.toolTip1.SetToolTip(this.radSigAvg, "Signal Average");
            this.radSigAvg.UseVisualStyleBackColor = true;
            this.radSigAvg.CheckedChanged += new System.EventHandler(this.radSigAvg_CheckedChanged);
            // 
            // radSigMaxBin
            // 
            this.radSigMaxBin.AutoSize = true;
            this.radSigMaxBin.Image = null;
            this.radSigMaxBin.Location = new System.Drawing.Point(99, 3);
            this.radSigMaxBin.Name = "radSigMaxBin";
            this.radSigMaxBin.Size = new System.Drawing.Size(63, 17);
            this.radSigMaxBin.TabIndex = 2;
            this.radSigMaxBin.TabStop = true;
            this.radSigMaxBin.Text = "Max Bin";
            this.toolTip1.SetToolTip(this.radSigMaxBin, resources.GetString("radSigMaxBin.ToolTip"));
            this.radSigMaxBin.UseVisualStyleBackColor = true;
            this.radSigMaxBin.CheckedChanged += new System.EventHandler(this.radSigMaxBin_CheckedChanged);
            // 
            // ucSignalSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.radSigMaxBin);
            this.Controls.Add(this.radSigAvg);
            this.Controls.Add(this.radSig);
            this.Name = "ucSignalSelect";
            this.Size = new System.Drawing.Size(162, 24);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButtonTS radSig;
        private System.Windows.Forms.RadioButtonTS radSigAvg;
        private System.Windows.Forms.RadioButtonTS radSigMaxBin;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
