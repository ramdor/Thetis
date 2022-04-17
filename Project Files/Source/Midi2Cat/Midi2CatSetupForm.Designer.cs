using Midi2Cat.IO;

namespace Thetis.Midi2Cat
{
    partial class Midi2CatSetupForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Midi2CatSetupForm));
            this.devicesTabControl = new System.Windows.Forms.TabControl();
            this.saveButton = new System.Windows.Forms.Button();
            this.startupPanel = new System.Windows.Forms.Panel();
            this.progressLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.startTimer = new System.Windows.Forms.Timer();
            this.startupPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // devicesTabControl
            // 
            this.devicesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.devicesTabControl.Location = new System.Drawing.Point(0, 0);
            this.devicesTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.devicesTabControl.Name = "devicesTabControl";
            this.devicesTabControl.SelectedIndex = 0;
            this.devicesTabControl.Size = new System.Drawing.Size(748, 630);
            this.devicesTabControl.TabIndex = 1;
            this.devicesTabControl.Visible = false;
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location = new System.Drawing.Point(667, 635);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // startupPanel
            // 
            this.startupPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.startupPanel.Controls.Add(this.progressLabel);
            this.startupPanel.Controls.Add(this.label1);
            this.startupPanel.Location = new System.Drawing.Point(162, 0);
            this.startupPanel.Name = "startupPanel";
            this.startupPanel.Size = new System.Drawing.Size(433, 83);
            this.startupPanel.TabIndex = 3;
            // 
            // progressLabel
            // 
            this.progressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressLabel.Location = new System.Drawing.Point(3, 56);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(425, 20);
            this.progressLabel.TabIndex = 2;
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(7, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(421, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "Midi Controller Setup Is Initialising Please Wait...";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // startTimer
            // 
            this.startTimer.Interval = 1000;
            this.startTimer.Tick += new System.EventHandler(this.startTimer_Tick);
            // 
            // Midi2CatSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 661);
            this.Controls.Add(this.startupPanel);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.devicesTabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "Midi2CatSetupForm";
            this.Text = "Midi Controller Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Midi2CatSetupForm_FormClosing);
            this.Load += new System.EventHandler(this.Midi2CatSetupForm_Load);
            this.startupPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl devicesTabControl;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Panel startupPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer startTimer;
        private System.Windows.Forms.Label progressLabel;
    }
}

