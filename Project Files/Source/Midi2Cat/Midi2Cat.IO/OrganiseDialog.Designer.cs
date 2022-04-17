namespace Midi2Cat.IO
{
    partial class OrganiseDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrganiseDialog));
            this.loadButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.mappingsLB = new System.Windows.Forms.ListBox();
            this.deleteButton = new System.Windows.Forms.Button();
            this.renameButton = new System.Windows.Forms.Button();
            this.renameTB = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // loadButton
            // 
            this.loadButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.loadButton.Location = new System.Drawing.Point(15, 298);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(75, 23);
            this.loadButton.TabIndex = 2;
            this.loadButton.Text = "Done";
            this.loadButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Mappings";
            // 
            // mappingsLB
            // 
            this.mappingsLB.FormattingEnabled = true;
            this.mappingsLB.Location = new System.Drawing.Point(12, 25);
            this.mappingsLB.Name = "mappingsLB";
            this.mappingsLB.Size = new System.Drawing.Size(356, 264);
            this.mappingsLB.TabIndex = 5;
            this.mappingsLB.SelectedIndexChanged += new System.EventHandler(this.mappingsLB_SelectedIndexChanged);
            this.mappingsLB.DoubleClick += new System.EventHandler(this.mappingsLB_DoubleClick);
            // 
            // deleteButton
            // 
            this.deleteButton.Enabled = false;
            this.deleteButton.Location = new System.Drawing.Point(212, 298);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 6;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // renameButton
            // 
            this.renameButton.Enabled = false;
            this.renameButton.Location = new System.Drawing.Point(293, 298);
            this.renameButton.Name = "renameButton";
            this.renameButton.Size = new System.Drawing.Size(75, 23);
            this.renameButton.TabIndex = 7;
            this.renameButton.Text = "Rename";
            this.renameButton.UseVisualStyleBackColor = true;
            this.renameButton.Click += new System.EventHandler(this.renameButton_Click);
            // 
            // renameTB
            // 
            this.renameTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.renameTB.Location = new System.Drawing.Point(15, 45);
            this.renameTB.Name = "renameTB";
            this.renameTB.Size = new System.Drawing.Size(272, 13);
            this.renameTB.TabIndex = 1;
            this.renameTB.Visible = false;
            this.renameTB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.renameTB_KeyDown);
            this.renameTB.Leave += new System.EventHandler(this.renameTB_Leave);
            // 
            // OrganiseDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(380, 328);
            this.Controls.Add(this.renameTB);
            this.Controls.Add(this.renameButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.mappingsLB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.loadButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OrganiseDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Organise Mappings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox mappingsLB;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button renameButton;
        private System.Windows.Forms.TextBox renameTB;
    }
}