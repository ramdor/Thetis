namespace Thetis
{
    partial class ucGradientDefault
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
            this.comboGradient = new System.Windows.Forms.ComboBoxTS();
            this.btnSet = new System.Windows.Forms.ButtonTS();
            this.SuspendLayout();
            // 
            // comboGradient
            // 
            this.comboGradient.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboGradient.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboGradient.FormattingEnabled = true;
            this.comboGradient.Items.AddRange(new object[] {
            "graphite",
            "lemon",
            "ice",
            "fire",
            "rainbow"});
            this.comboGradient.Location = new System.Drawing.Point(0, 0);
            this.comboGradient.Name = "comboGradient";
            this.comboGradient.Size = new System.Drawing.Size(161, 21);
            this.comboGradient.TabIndex = 0;
            // 
            // btnSet
            // 
            this.btnSet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSet.Image = null;
            this.btnSet.Location = new System.Drawing.Point(167, 0);
            this.btnSet.Name = "btnSet";
            this.btnSet.Selectable = true;
            this.btnSet.Size = new System.Drawing.Size(64, 21);
            this.btnSet.TabIndex = 1;
            this.btnSet.Text = "Set";
            this.btnSet.UseVisualStyleBackColor = true;
            this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
            // 
            // ucGradientDefault
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnSet);
            this.Controls.Add(this.comboGradient);
            this.Name = "ucGradientDefault";
            this.Size = new System.Drawing.Size(231, 21);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBoxTS comboGradient;
        private System.Windows.Forms.ButtonTS btnSet;
    }
}
