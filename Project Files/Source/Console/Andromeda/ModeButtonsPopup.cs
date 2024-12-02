//=================================================================
// ModeButtonsPopup.cs
//=================================================================
// Thetis is a C# implementation of a Software Defined Radio.
// Copyright (C) 2019  Laurence Barker, G8NJJ
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// The author can be reached by email at  
//
// laurence@nicklebyhouse.co.uk
//=================================================================

namespace Thetis
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// Mode setting popup form.
    /// </summary>

    public class ModeButtonsPopup : Form
    {
        #region Variable Declaration
        private Console console;
        private ButtonTS btnClose;
        private GroupBoxTS groupBoxTS1;
        private RadioButtonTS radBtn2;
        private RadioButtonTS radBtn1;
        private RadioButtonTS radBtn12;
        private RadioButtonTS radBtn11;
        private RadioButtonTS radBtn10;
        private RadioButtonTS radBtn9;
        private RadioButtonTS radBtn8;
        private RadioButtonTS radBtn7;
        private RadioButtonTS radBtn6;
        private RadioButtonTS radBtn5;
        private RadioButtonTS radBtn4;
        private RadioButtonTS radBtn3;
        private System.ComponentModel.IContainer components = null;
        #endregion

        #region Constructor and Destructor

        public ModeButtonsPopup(Console c)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            console = c;
        }

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

        #endregion

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModeButtonsPopup));
            this.btnClose = new System.Windows.Forms.ButtonTS();
            this.groupBoxTS1 = new System.Windows.Forms.GroupBoxTS();
            this.radBtn12 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn11 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn10 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn9 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn8 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn7 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn6 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn5 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn4 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn3 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn2 = new System.Windows.Forms.RadioButtonTS();
            this.radBtn1 = new System.Windows.Forms.RadioButtonTS();
            this.groupBoxTS1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Image = null;
            this.btnClose.Location = new System.Drawing.Point(346, 205);
            this.btnClose.Name = "btnClose";
            this.btnClose.Selectable = true;
            this.btnClose.Size = new System.Drawing.Size(100, 40);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // groupBoxTS1
            // 
            this.groupBoxTS1.Controls.Add(this.radBtn12);
            this.groupBoxTS1.Controls.Add(this.radBtn11);
            this.groupBoxTS1.Controls.Add(this.radBtn10);
            this.groupBoxTS1.Controls.Add(this.radBtn9);
            this.groupBoxTS1.Controls.Add(this.radBtn8);
            this.groupBoxTS1.Controls.Add(this.radBtn7);
            this.groupBoxTS1.Controls.Add(this.radBtn6);
            this.groupBoxTS1.Controls.Add(this.radBtn5);
            this.groupBoxTS1.Controls.Add(this.radBtn4);
            this.groupBoxTS1.Controls.Add(this.radBtn3);
            this.groupBoxTS1.Controls.Add(this.radBtn2);
            this.groupBoxTS1.Controls.Add(this.radBtn1);
            this.groupBoxTS1.Location = new System.Drawing.Point(12, 12);
            this.groupBoxTS1.Name = "groupBoxTS1";
            this.groupBoxTS1.Size = new System.Drawing.Size(328, 246);
            this.groupBoxTS1.TabIndex = 1;
            this.groupBoxTS1.TabStop = false;
            // 
            // radBtn12
            // 
            this.radBtn12.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn12.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn12.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn12.Image = null;
            this.radBtn12.Location = new System.Drawing.Point(218, 193);
            this.radBtn12.Name = "radBtn12";
            this.radBtn12.Size = new System.Drawing.Size(100, 40);
            this.radBtn12.TabIndex = 11;
            this.radBtn12.TabStop = true;
            this.radBtn12.Text = "DRM";
            this.radBtn12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn12.UseVisualStyleBackColor = true;
            this.radBtn12.Click += new System.EventHandler(this.radBtn12_Click);
            // 
            // radBtn11
            // 
            this.radBtn11.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn11.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn11.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn11.Image = null;
            this.radBtn11.Location = new System.Drawing.Point(112, 193);
            this.radBtn11.Name = "radBtn11";
            this.radBtn11.Size = new System.Drawing.Size(100, 40);
            this.radBtn11.TabIndex = 10;
            this.radBtn11.TabStop = true;
            this.radBtn11.Text = "DIGU";
            this.radBtn11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn11.UseVisualStyleBackColor = true;
            this.radBtn11.Click += new System.EventHandler(this.radBtn11_Click);
            // 
            // radBtn10
            // 
            this.radBtn10.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn10.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn10.Image = null;
            this.radBtn10.Location = new System.Drawing.Point(6, 193);
            this.radBtn10.Name = "radBtn10";
            this.radBtn10.Size = new System.Drawing.Size(100, 40);
            this.radBtn10.TabIndex = 9;
            this.radBtn10.TabStop = true;
            this.radBtn10.Text = "DIGL";
            this.radBtn10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn10.UseVisualStyleBackColor = true;
            this.radBtn10.Click += new System.EventHandler(this.radBtn10_Click);
            // 
            // radBtn9
            // 
            this.radBtn9.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn9.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn9.Image = null;
            this.radBtn9.Location = new System.Drawing.Point(218, 135);
            this.radBtn9.Name = "radBtn9";
            this.radBtn9.Size = new System.Drawing.Size(100, 40);
            this.radBtn9.TabIndex = 8;
            this.radBtn9.TabStop = true;
            this.radBtn9.Text = "SPEC";
            this.radBtn9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn9.UseVisualStyleBackColor = true;
            this.radBtn9.Click += new System.EventHandler(this.radBtn9_Click);
            // 
            // radBtn8
            // 
            this.radBtn8.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn8.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn8.Image = null;
            this.radBtn8.Location = new System.Drawing.Point(112, 135);
            this.radBtn8.Name = "radBtn8";
            this.radBtn8.Size = new System.Drawing.Size(100, 40);
            this.radBtn8.TabIndex = 7;
            this.radBtn8.TabStop = true;
            this.radBtn8.Text = "SAM";
            this.radBtn8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn8.UseVisualStyleBackColor = true;
            this.radBtn8.Click += new System.EventHandler(this.radBtn8_Click);
            // 
            // radBtn7
            // 
            this.radBtn7.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn7.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn7.Image = null;
            this.radBtn7.Location = new System.Drawing.Point(6, 135);
            this.radBtn7.Name = "radBtn7";
            this.radBtn7.Size = new System.Drawing.Size(100, 40);
            this.radBtn7.TabIndex = 6;
            this.radBtn7.TabStop = true;
            this.radBtn7.Text = "AM";
            this.radBtn7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn7.UseVisualStyleBackColor = true;
            this.radBtn7.Click += new System.EventHandler(this.radBtn7_Click);
            // 
            // radBtn6
            // 
            this.radBtn6.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn6.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn6.Image = null;
            this.radBtn6.Location = new System.Drawing.Point(218, 77);
            this.radBtn6.Name = "radBtn6";
            this.radBtn6.Size = new System.Drawing.Size(100, 40);
            this.radBtn6.TabIndex = 5;
            this.radBtn6.TabStop = true;
            this.radBtn6.Text = "FM";
            this.radBtn6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn6.UseVisualStyleBackColor = true;
            this.radBtn6.Click += new System.EventHandler(this.radBtn6_Click);
            // 
            // radBtn5
            // 
            this.radBtn5.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn5.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn5.Image = null;
            this.radBtn5.Location = new System.Drawing.Point(112, 77);
            this.radBtn5.Name = "radBtn5";
            this.radBtn5.Size = new System.Drawing.Size(100, 40);
            this.radBtn5.TabIndex = 4;
            this.radBtn5.TabStop = true;
            this.radBtn5.Text = "CWU";
            this.radBtn5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn5.UseVisualStyleBackColor = true;
            this.radBtn5.Click += new System.EventHandler(this.radBtn5_Click);
            // 
            // radBtn4
            // 
            this.radBtn4.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn4.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn4.Image = null;
            this.radBtn4.Location = new System.Drawing.Point(6, 77);
            this.radBtn4.Name = "radBtn4";
            this.radBtn4.Size = new System.Drawing.Size(100, 40);
            this.radBtn4.TabIndex = 3;
            this.radBtn4.TabStop = true;
            this.radBtn4.Text = "CWL";
            this.radBtn4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn4.UseVisualStyleBackColor = true;
            this.radBtn4.Click += new System.EventHandler(this.radBtn4_Click);
            // 
            // radBtn3
            // 
            this.radBtn3.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn3.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn3.Image = null;
            this.radBtn3.Location = new System.Drawing.Point(218, 19);
            this.radBtn3.Name = "radBtn3";
            this.radBtn3.Size = new System.Drawing.Size(100, 40);
            this.radBtn3.TabIndex = 2;
            this.radBtn3.TabStop = true;
            this.radBtn3.Text = "DSB";
            this.radBtn3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn3.UseVisualStyleBackColor = true;
            this.radBtn3.Click += new System.EventHandler(this.radBtn3_Click);
            // 
            // radBtn2
            // 
            this.radBtn2.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn2.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn2.Image = null;
            this.radBtn2.Location = new System.Drawing.Point(112, 19);
            this.radBtn2.Name = "radBtn2";
            this.radBtn2.Size = new System.Drawing.Size(100, 40);
            this.radBtn2.TabIndex = 1;
            this.radBtn2.TabStop = true;
            this.radBtn2.Text = "USB";
            this.radBtn2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn2.UseVisualStyleBackColor = true;
            this.radBtn2.Click += new System.EventHandler(this.radBtn2_Click);
            // 
            // radBtn1
            // 
            this.radBtn1.Appearance = System.Windows.Forms.Appearance.Button;
            this.radBtn1.BackColor = System.Drawing.SystemColors.Control;
            this.radBtn1.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
            this.radBtn1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radBtn1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.radBtn1.Image = null;
            this.radBtn1.Location = new System.Drawing.Point(6, 19);
            this.radBtn1.Name = "radBtn1";
            this.radBtn1.Size = new System.Drawing.Size(100, 40);
            this.radBtn1.TabIndex = 0;
            this.radBtn1.TabStop = true;
            this.radBtn1.Text = "LSB";
            this.radBtn1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radBtn1.UseVisualStyleBackColor = true;
            this.radBtn1.Click += new System.EventHandler(this.radBtn1_Click);
            // 
            // ModeButtonsPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(461, 264);
            this.Controls.Add(this.groupBoxTS1);
            this.Controls.Add(this.btnClose);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModeButtonsPopup";
            this.Text = "Set Mode";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.ModeButtonsPopup_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModeButtonsPopup_FormClosing);
            this.groupBoxTS1.ResumeLayout(false);
            this.ResumeLayout(false);

        }



        #endregion

        #region Properties
        //
        // reloads the current button settings
        //
        public void RepopulateForm()
        {
            DSPMode currentMode;

            if (console.ShowRX1)
            {
                this.Text = "set RX1 Mode";
                currentMode = console.RX1DSPMode;
            }

            else
            {
                this.Text = "set RX2 Mode";
                currentMode = console.RX2DSPMode;
            }

            switch (currentMode)
            {
                case DSPMode.LSB:
                    radBtn1.Checked = true;
                    break;
                case DSPMode.USB:
                    radBtn2.Checked = true;
                    break;
                case DSPMode.DSB:
                    radBtn3.Checked = true;
                    break;
                case DSPMode.CWL:
                    radBtn4.Checked = true;
                    break;
                case DSPMode.CWU:
                    radBtn5.Checked = true;
                    break;
                case DSPMode.FM:
                    radBtn6.Checked = true;
                    break;
                case DSPMode.AM:
                    radBtn7.Checked = true;
                    break;
                case DSPMode.SAM:
                    radBtn8.Checked = true;
                    break;
                case DSPMode.SPEC:
                    radBtn9.Checked = true;
                    break;
                case DSPMode.DIGL:
                    radBtn10.Checked = true;
                    break;
                case DSPMode.DIGU:
                    radBtn11.Checked = true;
                    break;
                case DSPMode.DRM:
                    radBtn12.Checked = true;
                    break;
            }
        }
            #endregion

            #region Event Handlers
            #endregion

            private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ModeButtonsPopup_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, "ModeButtonsPopup");
        }

        private void ModeButtonsPopup_Activated(object sender, EventArgs e)
        {
                RepopulateForm();
        }

        private void radBtn1_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.LSB;
            else
                console.RX2DSPMode = DSPMode.LSB;
        }

        private void radBtn2_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.USB;
            else
                console.RX2DSPMode = DSPMode.USB;
        }

        private void radBtn3_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.DSB;
            else
                console.RX2DSPMode = DSPMode.DSB;
        }

        private void radBtn4_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.CWL;
            else
                console.RX2DSPMode = DSPMode.CWL;
        }

        private void radBtn5_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.CWU;
            else
                console.RX2DSPMode = DSPMode.CWU;
        }

        private void radBtn6_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.FM;
            else
                console.RX2DSPMode = DSPMode.FM;
        }

        private void radBtn7_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.AM;
            else
                console.RX2DSPMode = DSPMode.AM;
        }

        private void radBtn8_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.SAM;
            else
                console.RX2DSPMode = DSPMode.SAM;
        }

        private void radBtn9_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.SPEC;
            else
                console.RX2DSPMode = DSPMode.SPEC;
        }

        private void radBtn10_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.DIGL;
            else
                console.RX2DSPMode = DSPMode.DIGL;
        }

        private void radBtn11_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.DIGU;
            else
                console.RX2DSPMode = DSPMode.DIGU;
        }

        private void radBtn12_Click(object sender, EventArgs e)
        {
            if (console.ShowRX1)
                console.RX1DSPMode = DSPMode.DRM;
            else
                console.RX2DSPMode = DSPMode.DRM;
        }
    }
}
