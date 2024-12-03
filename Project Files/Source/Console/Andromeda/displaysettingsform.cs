//=================================================================
// DisplaySettingsForm.cs
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
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    ///  Display settings popup form.
    /// </summary>

    public class DisplaySettingsForm : Form
    {

        #region Variable Declaration

        private System.Windows.Forms.ButtonTS btnClose;
        private Console console;
        private ComboBoxTS comboRX1Meter;
        private LabelTS labelTS1;
        private ComboBoxTS comboRX2Meter;
        private LabelTS labelTS2;
        private ComboBoxTS comboRX1Display;
        private ComboBoxTS comboRX2Display;
        private LabelTS labelTS3;
        private LabelTS labelTS4;
        private ComboBoxTS comboTXMeter;
        private LabelTS labelTS5;
        private CheckBoxTS chkRX1Avg;
        private CheckBoxTS chkRX1Peak;
        private CheckBoxTS chkRX2Avg;
        private CheckBoxTS chkRX2Peak;
        private System.ComponentModel.IContainer components = null;
        
        #endregion


        #region Constructor and Destructor
        public DisplaySettingsForm(Console c)
        {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DisplaySettingsForm));
            this.labelTS5 = new System.Windows.Forms.LabelTS();
            this.comboTXMeter = new System.Windows.Forms.ComboBoxTS();
            this.labelTS4 = new System.Windows.Forms.LabelTS();
            this.labelTS3 = new System.Windows.Forms.LabelTS();
            this.comboRX2Display = new System.Windows.Forms.ComboBoxTS();
            this.comboRX1Display = new System.Windows.Forms.ComboBoxTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.comboRX2Meter = new System.Windows.Forms.ComboBoxTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.comboRX1Meter = new System.Windows.Forms.ComboBoxTS();
            this.btnClose = new System.Windows.Forms.ButtonTS();
            this.chkRX1Avg = new System.Windows.Forms.CheckBoxTS();
            this.chkRX1Peak = new System.Windows.Forms.CheckBoxTS();
            this.chkRX2Avg = new System.Windows.Forms.CheckBoxTS();
            this.chkRX2Peak = new System.Windows.Forms.CheckBoxTS();
            this.SuspendLayout();
            // 
            // labelTS5
            // 
            this.labelTS5.AutoSize = true;
            this.labelTS5.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.labelTS5.Image = null;
            this.labelTS5.Location = new System.Drawing.Point(323, 21);
            this.labelTS5.Name = "labelTS5";
            this.labelTS5.Size = new System.Drawing.Size(51, 13);
            this.labelTS5.TabIndex = 10;
            this.labelTS5.Text = "TX Meter";
            // 
            // comboTXMeter
            // 
            this.comboTXMeter.FormattingEnabled = true;
            this.comboTXMeter.Location = new System.Drawing.Point(323, 40);
            this.comboTXMeter.Name = "comboTXMeter";
            this.comboTXMeter.Size = new System.Drawing.Size(121, 21);
            this.comboTXMeter.TabIndex = 9;
            this.comboTXMeter.SelectedIndexChanged += new System.EventHandler(this.ComboTXMeter_SelectedIndexChanged);
            // 
            // labelTS4
            // 
            this.labelTS4.AutoSize = true;
            this.labelTS4.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.labelTS4.Image = null;
            this.labelTS4.Location = new System.Drawing.Point(174, 88);
            this.labelTS4.Name = "labelTS4";
            this.labelTS4.Size = new System.Drawing.Size(95, 13);
            this.labelTS4.TabIndex = 8;
            this.labelTS4.Text = "RX2 Display Mode";
            // 
            // labelTS3
            // 
            this.labelTS3.AutoSize = true;
            this.labelTS3.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.labelTS3.Image = null;
            this.labelTS3.Location = new System.Drawing.Point(19, 88);
            this.labelTS3.Name = "labelTS3";
            this.labelTS3.Size = new System.Drawing.Size(95, 13);
            this.labelTS3.TabIndex = 7;
            this.labelTS3.Text = "RX1 Display Mode";
            // 
            // comboRX2Display
            // 
            this.comboRX2Display.FormattingEnabled = true;
            this.comboRX2Display.Location = new System.Drawing.Point(174, 107);
            this.comboRX2Display.Name = "comboRX2Display";
            this.comboRX2Display.Size = new System.Drawing.Size(121, 21);
            this.comboRX2Display.TabIndex = 6;
            this.comboRX2Display.SelectedIndexChanged += new System.EventHandler(this.ComboRX2Display_SelectedIndexChanged);
            // 
            // comboRX1Display
            // 
            this.comboRX1Display.FormattingEnabled = true;
            this.comboRX1Display.Location = new System.Drawing.Point(19, 107);
            this.comboRX1Display.Name = "comboRX1Display";
            this.comboRX1Display.Size = new System.Drawing.Size(121, 21);
            this.comboRX1Display.TabIndex = 5;
            this.comboRX1Display.SelectedIndexChanged += new System.EventHandler(this.ComboRX1Display_SelectedIndexChanged);
            // 
            // labelTS2
            // 
            this.labelTS2.AutoSize = true;
            this.labelTS2.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(174, 21);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(58, 13);
            this.labelTS2.TabIndex = 4;
            this.labelTS2.Text = "RX2 Meter";
            // 
            // comboRX2Meter
            // 
            this.comboRX2Meter.FormattingEnabled = true;
            this.comboRX2Meter.Location = new System.Drawing.Point(174, 40);
            this.comboRX2Meter.Name = "comboRX2Meter";
            this.comboRX2Meter.Size = new System.Drawing.Size(116, 21);
            this.comboRX2Meter.TabIndex = 3;
            this.comboRX2Meter.SelectedIndexChanged += new System.EventHandler(this.ComboRX2Meter_SelectedIndexChanged);
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(19, 21);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(58, 13);
            this.labelTS1.TabIndex = 2;
            this.labelTS1.Text = "RX1 Meter";
            // 
            // comboRX1Meter
            // 
            this.comboRX1Meter.FormattingEnabled = true;
            this.comboRX1Meter.Location = new System.Drawing.Point(19, 40);
            this.comboRX1Meter.Name = "comboRX1Meter";
            this.comboRX1Meter.Size = new System.Drawing.Size(122, 21);
            this.comboRX1Meter.TabIndex = 1;
            this.comboRX1Meter.SelectedIndexChanged += new System.EventHandler(this.ComboRX1Meter_SelectedIndexChanged);
            // 
            // btnClose
            // 
            this.btnClose.Image = null;
            this.btnClose.Location = new System.Drawing.Point(326, 153);
            this.btnClose.Name = "btnClose";
            this.btnClose.Selectable = true;
            this.btnClose.Size = new System.Drawing.Size(109, 41);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // chkRX1Avg
            // 
            this.chkRX1Avg.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkRX1Avg.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX1Avg.Image = null;
            this.chkRX1Avg.Location = new System.Drawing.Point(19, 153);
            this.chkRX1Avg.Name = "chkRX1Avg";
            this.chkRX1Avg.Size = new System.Drawing.Size(50, 41);
            this.chkRX1Avg.TabIndex = 11;
            this.chkRX1Avg.Text = "Avg";
            this.chkRX1Avg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX1Avg.UseVisualStyleBackColor = true;
            this.chkRX1Avg.CheckedChanged += new System.EventHandler(this.ChkRX1Avg_CheckedChanged);
            // 
            // chkRX1Peak
            // 
            this.chkRX1Peak.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkRX1Peak.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX1Peak.Image = null;
            this.chkRX1Peak.Location = new System.Drawing.Point(90, 153);
            this.chkRX1Peak.Name = "chkRX1Peak";
            this.chkRX1Peak.Size = new System.Drawing.Size(50, 41);
            this.chkRX1Peak.TabIndex = 12;
            this.chkRX1Peak.Text = "Peak";
            this.chkRX1Peak.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX1Peak.UseVisualStyleBackColor = true;
            this.chkRX1Peak.CheckedChanged += new System.EventHandler(this.ChkRX1Peak_CheckedChanged);
            // 
            // chkRX2Avg
            // 
            this.chkRX2Avg.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkRX2Avg.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX2Avg.Image = null;
            this.chkRX2Avg.Location = new System.Drawing.Point(174, 153);
            this.chkRX2Avg.Name = "chkRX2Avg";
            this.chkRX2Avg.Size = new System.Drawing.Size(50, 41);
            this.chkRX2Avg.TabIndex = 13;
            this.chkRX2Avg.Text = "Avg";
            this.chkRX2Avg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX2Avg.UseVisualStyleBackColor = true;
            this.chkRX2Avg.CheckedChanged += new System.EventHandler(this.ChkRX2Avg_CheckedChanged);
            // 
            // chkRX2Peak
            // 
            this.chkRX2Peak.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkRX2Peak.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX2Peak.Image = null;
            this.chkRX2Peak.Location = new System.Drawing.Point(245, 153);
            this.chkRX2Peak.Name = "chkRX2Peak";
            this.chkRX2Peak.Size = new System.Drawing.Size(50, 41);
            this.chkRX2Peak.TabIndex = 14;
            this.chkRX2Peak.Text = "Peak";
            this.chkRX2Peak.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkRX2Peak.UseVisualStyleBackColor = true;
            this.chkRX2Peak.CheckedChanged += new System.EventHandler(this.ChkRX2Peak_CheckedChanged);
            // 
            // DisplaySettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(454, 204);
            this.Controls.Add(this.chkRX2Peak);
            this.Controls.Add(this.chkRX2Avg);
            this.Controls.Add(this.chkRX1Peak);
            this.Controls.Add(this.chkRX1Avg);
            this.Controls.Add(this.labelTS5);
            this.Controls.Add(this.comboTXMeter);
            this.Controls.Add(this.labelTS4);
            this.Controls.Add(this.labelTS3);
            this.Controls.Add(this.comboRX2Display);
            this.Controls.Add(this.comboRX1Display);
            this.Controls.Add(this.labelTS2);
            this.Controls.Add(this.comboRX2Meter);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.comboRX1Meter);
            this.Controls.Add(this.btnClose);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DisplaySettingsForm";
            this.Text = "Display Settings";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.DisplaySettingsForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DisplaySettingsForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }




        #endregion

        #region Properties
        //
        // copy settings from relevant console controls
        public void RepopulateForm()
        {
            comboRX1Meter.Items.Clear();            // G8NJJ: clear old list before re-adding!
            comboRX1Meter.Items.AddRange(console.MeterRXModeItems.Cast<Object>().ToArray());
            comboRX1Meter.Text = console.CurrentMeterRXModeText;

            comboRX2Meter.Items.Clear();            // G8NJJ: clear old list before re-adding!
            comboRX2Meter.Items.AddRange(console.RX2MeterModeItems.Cast<Object>().ToArray());
            comboRX2Meter.Text = console.RX2MeterModeText;

            comboRX1Display.Items.Clear();            // G8NJJ: clear old list before re-adding!
            comboRX1Display.Items.AddRange(console.DisplayModeItems.Cast<Object>().ToArray());
            comboRX1Display.Text = console.DisplayModeText;

            comboRX2Display.Items.Clear();            // G8NJJ: clear old list before re-adding!
            comboRX2Display.Items.AddRange(console.DisplayRX2ModeItems.Cast<Object>().ToArray());
            comboRX2Display.Text = console.DisplayRX2ModeText;

            comboTXMeter.Items.Clear();            // G8NJJ: clear old list before re-adding!
            comboTXMeter.Items.AddRange(console.MeterTXModeItems.Cast<Object>().ToArray());
            comboTXMeter.Text = console.CurrentMeterTXModeText;

            if (console.CATDisplayAvg == 1)              // for some reason it's a numerical, not bool, property
                chkRX1Avg.Checked = true;
            else
                chkRX1Avg.Checked = false;

            if (console.CATRX2DisplayAvg == 1)
                chkRX2Avg.Checked = true;
            else
                chkRX2Avg.Checked = false;

            if (console.CATDispPeak == "1")              // for some reason it's a string, not bool, property
                chkRX1Peak.Checked = true;
            else
                chkRX1Peak.Checked = false;

            if (console.CATRX2DispPeak == "1")              // for some reason it's a string, not bool, property
                chkRX2Peak.Checked = true;
            else
                chkRX2Peak.Checked = false;

        }
        #endregion

        #region Event Handlers
        #endregion

        // close form button: simply tell the form to close
        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DisplaySettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, "DisplaySettingsForm");

        }

        private void DisplaySettingsForm_Activated(object sender, EventArgs e)
        {
            RepopulateForm();

        }

        private void ComboRX1Meter_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.CurrentMeterRXModeText = comboRX1Meter.Text;
        }

        private void ComboRX2Meter_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.RX2MeterModeText = comboRX2Meter.Text;
        }

        private void ComboTXMeter_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.CurrentMeterTXModeText = comboTXMeter.Text;
        }

        private void ComboRX1Display_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.DisplayModeText = comboRX1Display.Text;
        }

        private void ComboRX2Display_SelectedIndexChanged(object sender, EventArgs e)
        {
            console.DisplayRX2ModeText = comboRX2Display.Text;
        }

        private void ChkRX1Avg_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRX1Avg.Checked)
                console.CATDisplayAvg = 1;
            else
                console.CATDisplayAvg = 0;
        }

        private void ChkRX2Avg_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRX2Avg.Checked)
                console.CATRX2DisplayAvg = 1;
            else
                console.CATRX2DisplayAvg = 0;
        }

        private void ChkRX1Peak_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRX1Peak.Checked)
                console.CATDispPeak = "1";
            else
                console.CATDispPeak = "0";
        }

        private void ChkRX2Peak_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRX2Peak.Checked)
                console.CATRX2DispPeak = "1";
            else
                console.CATRX2DispPeak = "0";
        }
    }
}
