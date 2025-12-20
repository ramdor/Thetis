namespace Thetis
{
    partial class frmVariablePicker
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
            this.lstVariables = new System.Windows.Forms.ListBox();
            this.btnCancel = new System.Windows.Forms.ButtonTS();
            this.btnSelect = new System.Windows.Forms.ButtonTS();
            this.btnDefault = new System.Windows.Forms.ButtonTS();
            this.SuspendLayout();
            // 
            // lstVariables
            // 
            this.lstVariables.FormattingEnabled = true;
            this.lstVariables.Location = new System.Drawing.Point(12, 12);
            this.lstVariables.Name = "lstVariables";
            this.lstVariables.Size = new System.Drawing.Size(205, 290);
            this.lstVariables.TabIndex = 0;
            this.lstVariables.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstVariables_MouseDoubleClick);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = null;
            this.btnCancel.Location = new System.Drawing.Point(165, 308);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Selectable = true;
            this.btnCancel.Size = new System.Drawing.Size(52, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSelect.Image = null;
            this.btnSelect.Location = new System.Drawing.Point(107, 308);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Selectable = true;
            this.btnSelect.Size = new System.Drawing.Size(52, 23);
            this.btnSelect.TabIndex = 3;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnDefault
            // 
            this.btnDefault.DialogResult = System.Windows.Forms.DialogResult.Ignore;
            this.btnDefault.Image = null;
            this.btnDefault.Location = new System.Drawing.Point(12, 308);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Selectable = true;
            this.btnDefault.Size = new System.Drawing.Size(52, 23);
            this.btnDefault.TabIndex = 5;
            this.btnDefault.Text = "Default";
            this.btnDefault.UseVisualStyleBackColor = true;
            this.btnDefault.Click += new System.EventHandler(this.btnDefault_Click);
            // 
            // frmVariablePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(229, 338);
            this.Controls.Add(this.btnDefault);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.lstVariables);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmVariablePicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Variable Picker";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstVariables;
        private System.Windows.Forms.ButtonTS btnCancel;
        private System.Windows.Forms.ButtonTS btnSelect;
        private System.Windows.Forms.ButtonTS btnDefault;
    }
}