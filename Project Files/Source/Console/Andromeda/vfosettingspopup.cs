//=================================================================
// VFOSettingsPopup.cs
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
    using System.Collections.Generic;
    using System.Windows.Forms;

    /// <summary>
    ///  VFO settings popup form.
    /// </summary>

    public class VFOSettingsPopup : Form
    {
        #region Variable Declaration
        private Console console;
        private TextBoxTS txtBoxTuneStep;
        private ButtonTS buttonMinus;
        private ButtonTS buttonPlus;
        private LabelTS labelTS1;
        private ButtonTS buttonClose;
        private System.ComponentModel.IContainer components = null;
        #endregion

        #region Constructor and Destructor

        public VFOSettingsPopup(Console c)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VFOSettingsPopup));
            this.buttonClose = new System.Windows.Forms.ButtonTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.buttonPlus = new System.Windows.Forms.ButtonTS();
            this.buttonMinus = new System.Windows.Forms.ButtonTS();
            this.txtBoxTuneStep = new System.Windows.Forms.TextBoxTS();
            this.SuspendLayout();
            // 
            // buttonClose
            // 
            this.buttonClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClose.Image = null;
            this.buttonClose.Location = new System.Drawing.Point(238, 74);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Selectable = true;
            this.buttonClose.Size = new System.Drawing.Size(75, 32);
            this.buttonClose.TabIndex = 4;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.ButtonClose_Click);
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(12, 30);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(57, 13);
            this.labelTS1.TabIndex = 3;
            this.labelTS1.Text = "Tune Step";
            // 
            // buttonPlus
            // 
            this.buttonPlus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPlus.Image = null;
            this.buttonPlus.Location = new System.Drawing.Point(261, 24);
            this.buttonPlus.Name = "buttonPlus";
            this.buttonPlus.Selectable = true;
            this.buttonPlus.Size = new System.Drawing.Size(52, 32);
            this.buttonPlus.TabIndex = 2;
            this.buttonPlus.Text = "+";
            this.buttonPlus.UseVisualStyleBackColor = true;
            this.buttonPlus.Click += new System.EventHandler(this.ButtonPlus_Click);
            // 
            // buttonMinus
            // 
            this.buttonMinus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonMinus.Image = null;
            this.buttonMinus.Location = new System.Drawing.Point(75, 24);
            this.buttonMinus.Name = "buttonMinus";
            this.buttonMinus.Selectable = true;
            this.buttonMinus.Size = new System.Drawing.Size(52, 32);
            this.buttonMinus.TabIndex = 1;
            this.buttonMinus.Text = "-";
            this.buttonMinus.UseVisualStyleBackColor = true;
            this.buttonMinus.Click += new System.EventHandler(this.ButtonMinus_Click);
            // 
            // txtBoxTuneStep
            // 
            this.txtBoxTuneStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBoxTuneStep.Location = new System.Drawing.Point(133, 27);
            this.txtBoxTuneStep.Name = "txtBoxTuneStep";
            this.txtBoxTuneStep.Size = new System.Drawing.Size(122, 26);
            this.txtBoxTuneStep.TabIndex = 0;
            this.txtBoxTuneStep.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TextBoxTuneStep_MouseDown);
            // 
            // VFOSettingsPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(334, 127);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.buttonPlus);
            this.Controls.Add(this.buttonMinus);
            this.Controls.Add(this.txtBoxTuneStep);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VFOSettingsPopup";
            this.Text = "VFO Settings";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VFOSettingsPopup_FormClosing);
            this.Load += new System.EventHandler(this.VFOSettingsPopup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void VFOSettingsPopup_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, "VFOSettingsPopup");

        }

        private void TextBoxTuneStep_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ButtonPlus_Click(null, null);

        }

        private void ButtonMinus_Click(object sender, EventArgs e)
        {
            List<TuneStep> tune_step_list = console.TuneStepList;
            int tune_step_index = console.TuneStepIndex;

            tune_step_index = (tune_step_index - 1 + tune_step_list.Count) % tune_step_list.Count;
            txtBoxTuneStep.Text = tune_step_list[tune_step_index].Name;
            console.TuneStepIndex = tune_step_index;

        }

        private void ButtonPlus_Click(object sender, EventArgs e)
        {
            List<TuneStep> tune_step_list = console.TuneStepList;
            int tune_step_index = console.TuneStepIndex;

            tune_step_index = (tune_step_index + 1) % tune_step_list.Count;
            txtBoxTuneStep.Text = tune_step_list[tune_step_index].Name;
            console.TuneStepIndex = tune_step_index;
        }

        private void VFOSettingsPopup_Load(object sender, EventArgs e)
        {
            List<TuneStep> tune_step_list = console.TuneStepList;
            int tune_step_index = console.TuneStepIndex;

            txtBoxTuneStep.Text = tune_step_list[tune_step_index].Name;
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
