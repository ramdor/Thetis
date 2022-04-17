namespace Midi2Cat.IO
{
    partial class PickDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PickDialog));
            this.doneButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.promptLabel = new System.Windows.Forms.Label();
            this.mappingsLB = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // doneButton
            // 
            this.doneButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.doneButton.Enabled = false;
            this.doneButton.Location = new System.Drawing.Point(12, 240);
            this.doneButton.Name = "doneButton";
            this.doneButton.Size = new System.Drawing.Size(75, 23);
            this.doneButton.TabIndex = 2;
            this.doneButton.Text = "Done";
            this.doneButton.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(293, 239);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // promptLabel
            // 
            this.promptLabel.AutoSize = true;
            this.promptLabel.Location = new System.Drawing.Point(12, 9);
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Size = new System.Drawing.Size(138, 13);
            this.promptLabel.TabIndex = 4;
            this.promptLabel.Text = "Pick the mappings to export";
            // 
            // mappingsLB
            // 
            this.mappingsLB.FormattingEnabled = true;
            this.mappingsLB.Location = new System.Drawing.Point(15, 26);
            this.mappingsLB.Name = "mappingsLB";
            this.mappingsLB.Size = new System.Drawing.Size(353, 199);
            this.mappingsLB.TabIndex = 5;
            this.mappingsLB.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.mappingsLB_ItemCheck);
            // 
            // PickDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 268);
            this.Controls.Add(this.mappingsLB);
            this.Controls.Add(this.promptLabel);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.doneButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PickDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mapping Picker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button doneButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label promptLabel;
        private System.Windows.Forms.CheckedListBox mappingsLB;
    }
}