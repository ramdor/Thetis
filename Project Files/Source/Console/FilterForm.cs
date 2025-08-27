//=================================================================
// FilterForm.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2011  FlexRadio Systems
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
// You may contact us via email at: gpl@flexradio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
//=================================================================

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
	/// <summary>
	/// Summary description for FilterForm.
	/// </summary>
	public class FilterForm : System.Windows.Forms.Form
	{
		#region Variable Declaration 

		private Console console;
		private FilterPreset[] preset;
		private bool rx2;
		private System.Windows.Forms.ComboBox comboDSPMode;
		private System.Windows.Forms.RadioButtonTS radFilter1;
		private System.Windows.Forms.RadioButtonTS radFilter2;
		private System.Windows.Forms.RadioButtonTS radFilter3;
		private System.Windows.Forms.RadioButtonTS radFilter4;
		private System.Windows.Forms.RadioButtonTS radFilter5;
		private System.Windows.Forms.RadioButtonTS radFilter6;
		private System.Windows.Forms.RadioButtonTS radFilter7;
		private System.Windows.Forms.RadioButtonTS radFilter8;
		private System.Windows.Forms.RadioButtonTS radFilter9;
		private System.Windows.Forms.RadioButtonTS radFilter10;
		private System.Windows.Forms.RadioButtonTS radFilterVar1;
		private System.Windows.Forms.RadioButtonTS radFilterVar2;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Label lblMode;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.NumericUpDown udLow;
		private System.Windows.Forms.NumericUpDown udHigh;
		private System.Windows.Forms.Label lblLow;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.PictureBox picDisplay;
		private System.Windows.Forms.Label lblWidth;
		private System.Windows.Forms.NumericUpDown udWidth;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		#region Constructor and Destructor

		public FilterForm(Console c, FilterPreset[] fp, bool _rx2)
		{
			//
			// Required for Windows Form Designer support
			//
			console = c;
			preset = fp;
			InitializeComponent();
			comboDSPMode.SelectedIndex = 0;	
			radFilter1.Checked = true;
			rx2 = _rx2;
			if(rx2)
			{
				radFilter8.Enabled = false;
				radFilter9.Enabled = false;
				radFilter10.Enabled = false;
			}
			Common.RestoreForm(this, "FilterForm", false);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterForm));
            this.comboDSPMode = new System.Windows.Forms.ComboBox();
            this.radFilter1 = new System.Windows.Forms.RadioButtonTS();
            this.radFilter2 = new System.Windows.Forms.RadioButtonTS();
            this.radFilter3 = new System.Windows.Forms.RadioButtonTS();
            this.radFilter4 = new System.Windows.Forms.RadioButtonTS();
            this.radFilter5 = new System.Windows.Forms.RadioButtonTS();
            this.radFilter6 = new System.Windows.Forms.RadioButtonTS();
            this.radFilter7 = new System.Windows.Forms.RadioButtonTS();
            this.radFilter8 = new System.Windows.Forms.RadioButtonTS();
            this.radFilter9 = new System.Windows.Forms.RadioButtonTS();
            this.radFilter10 = new System.Windows.Forms.RadioButtonTS();
            this.radFilterVar1 = new System.Windows.Forms.RadioButtonTS();
            this.radFilterVar2 = new System.Windows.Forms.RadioButtonTS();
            this.lblMode = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.udLow = new System.Windows.Forms.NumericUpDown();
            this.udHigh = new System.Windows.Forms.NumericUpDown();
            this.lblLow = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.udWidth = new System.Windows.Forms.NumericUpDown();
            this.lblWidth = new System.Windows.Forms.Label();
            this.picDisplay = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.udLow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udHigh)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // comboDSPMode
            // 
            this.comboDSPMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDSPMode.Items.AddRange(new object[] {
            "LSB",
            "USB",
            "DSB",
            "CWL",
            "CWU",
            "AM",
            "SAM",
            "DIGL",
            "DIGU"});
            this.comboDSPMode.Location = new System.Drawing.Point(64, 16);
            this.comboDSPMode.Name = "comboDSPMode";
            this.comboDSPMode.Size = new System.Drawing.Size(64, 21);
            this.comboDSPMode.TabIndex = 0;
            this.comboDSPMode.SelectedIndexChanged += new System.EventHandler(this.comboDSPMode_SelectedIndexChanged);
            // 
            // radFilter1
            // 
            this.radFilter1.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilter1.Image = null;
            this.radFilter1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilter1.Location = new System.Drawing.Point(8, 48);
            this.radFilter1.Name = "radFilter1";
            this.radFilter1.Size = new System.Drawing.Size(48, 18);
            this.radFilter1.TabIndex = 37;
            this.radFilter1.Text = "6.0k";
            this.radFilter1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilter1.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilter2
            // 
            this.radFilter2.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilter2.Image = null;
            this.radFilter2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilter2.Location = new System.Drawing.Point(56, 48);
            this.radFilter2.Name = "radFilter2";
            this.radFilter2.Size = new System.Drawing.Size(48, 18);
            this.radFilter2.TabIndex = 39;
            this.radFilter2.Text = "4.0k";
            this.radFilter2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilter2.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilter3
            // 
            this.radFilter3.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilter3.Image = null;
            this.radFilter3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilter3.Location = new System.Drawing.Point(104, 48);
            this.radFilter3.Name = "radFilter3";
            this.radFilter3.Size = new System.Drawing.Size(48, 18);
            this.radFilter3.TabIndex = 38;
            this.radFilter3.Text = "2.6k";
            this.radFilter3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilter3.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilter4
            // 
            this.radFilter4.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilter4.Image = null;
            this.radFilter4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilter4.Location = new System.Drawing.Point(8, 66);
            this.radFilter4.Name = "radFilter4";
            this.radFilter4.Size = new System.Drawing.Size(48, 18);
            this.radFilter4.TabIndex = 40;
            this.radFilter4.Text = "2.1k";
            this.radFilter4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilter4.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilter5
            // 
            this.radFilter5.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilter5.Image = null;
            this.radFilter5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilter5.Location = new System.Drawing.Point(56, 66);
            this.radFilter5.Name = "radFilter5";
            this.radFilter5.Size = new System.Drawing.Size(48, 18);
            this.radFilter5.TabIndex = 41;
            this.radFilter5.Text = "1.0k";
            this.radFilter5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilter5.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilter6
            // 
            this.radFilter6.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilter6.Image = null;
            this.radFilter6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilter6.Location = new System.Drawing.Point(104, 66);
            this.radFilter6.Name = "radFilter6";
            this.radFilter6.Size = new System.Drawing.Size(48, 18);
            this.radFilter6.TabIndex = 42;
            this.radFilter6.Text = "500";
            this.radFilter6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilter6.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilter7
            // 
            this.radFilter7.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilter7.Image = null;
            this.radFilter7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilter7.Location = new System.Drawing.Point(8, 84);
            this.radFilter7.Name = "radFilter7";
            this.radFilter7.Size = new System.Drawing.Size(48, 18);
            this.radFilter7.TabIndex = 43;
            this.radFilter7.Text = "250";
            this.radFilter7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilter7.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilter8
            // 
            this.radFilter8.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilter8.Image = null;
            this.radFilter8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilter8.Location = new System.Drawing.Point(56, 84);
            this.radFilter8.Name = "radFilter8";
            this.radFilter8.Size = new System.Drawing.Size(48, 18);
            this.radFilter8.TabIndex = 44;
            this.radFilter8.Text = "100";
            this.radFilter8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilter8.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilter9
            // 
            this.radFilter9.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilter9.Image = null;
            this.radFilter9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilter9.Location = new System.Drawing.Point(104, 84);
            this.radFilter9.Name = "radFilter9";
            this.radFilter9.Size = new System.Drawing.Size(48, 18);
            this.radFilter9.TabIndex = 45;
            this.radFilter9.Text = "50";
            this.radFilter9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilter9.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilter10
            // 
            this.radFilter10.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilter10.Image = null;
            this.radFilter10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilter10.Location = new System.Drawing.Point(8, 102);
            this.radFilter10.Name = "radFilter10";
            this.radFilter10.Size = new System.Drawing.Size(48, 18);
            this.radFilter10.TabIndex = 46;
            this.radFilter10.Text = "25";
            this.radFilter10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilter10.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilterVar1
            // 
            this.radFilterVar1.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilterVar1.Image = null;
            this.radFilterVar1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilterVar1.Location = new System.Drawing.Point(56, 102);
            this.radFilterVar1.Name = "radFilterVar1";
            this.radFilterVar1.Size = new System.Drawing.Size(48, 18);
            this.radFilterVar1.TabIndex = 47;
            this.radFilterVar1.Text = "Var 1";
            this.radFilterVar1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilterVar1.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // radFilterVar2
            // 
            this.radFilterVar2.Appearance = System.Windows.Forms.Appearance.Button;
            this.radFilterVar2.Image = null;
            this.radFilterVar2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFilterVar2.Location = new System.Drawing.Point(104, 102);
            this.radFilterVar2.Name = "radFilterVar2";
            this.radFilterVar2.Size = new System.Drawing.Size(48, 18);
            this.radFilterVar2.TabIndex = 48;
            this.radFilterVar2.Text = "Var 2";
            this.radFilterVar2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radFilterVar2.CheckedChanged += new System.EventHandler(this.radFilter_CheckedChanged);
            // 
            // lblMode
            // 
            this.lblMode.Location = new System.Drawing.Point(24, 16);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(40, 23);
            this.lblMode.TabIndex = 49;
            this.lblMode.Text = "Mode:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(72, 16);
            this.txtName.MaxLength = 6;
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(56, 20);
            this.txtName.TabIndex = 50;
            this.txtName.LostFocus += new System.EventHandler(this.txtName_LostFocus);
            // 
            // lblName
            // 
            this.lblName.Location = new System.Drawing.Point(8, 16);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(48, 23);
            this.lblName.TabIndex = 51;
            this.lblName.Text = "Name:";
            // 
            // udLow
            // 
            this.udLow.Location = new System.Drawing.Point(72, 64);
            this.udLow.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udLow.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.udLow.Name = "udLow";
            this.udLow.Size = new System.Drawing.Size(64, 20);
            this.udLow.TabIndex = 52;
            this.udLow.ValueChanged += new System.EventHandler(this.udLow_ValueChanged);
            this.udLow.LostFocus += new System.EventHandler(this.udLow_LostFocus);
            // 
            // udHigh
            // 
            this.udHigh.Location = new System.Drawing.Point(72, 40);
            this.udHigh.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.udHigh.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.udHigh.Name = "udHigh";
            this.udHigh.Size = new System.Drawing.Size(64, 20);
            this.udHigh.TabIndex = 53;
            this.udHigh.ValueChanged += new System.EventHandler(this.udHigh_ValueChanged);
            this.udHigh.LostFocus += new System.EventHandler(this.udHigh_LostFocus);
            // 
            // lblLow
            // 
            this.lblLow.Location = new System.Drawing.Point(8, 64);
            this.lblLow.Name = "lblLow";
            this.lblLow.Size = new System.Drawing.Size(48, 23);
            this.lblLow.TabIndex = 54;
            this.lblLow.Text = "Low:";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 23);
            this.label1.TabIndex = 55;
            this.label1.Text = "High:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radFilter10);
            this.groupBox1.Controls.Add(this.radFilter2);
            this.groupBox1.Controls.Add(this.radFilterVar1);
            this.groupBox1.Controls.Add(this.comboDSPMode);
            this.groupBox1.Controls.Add(this.radFilter1);
            this.groupBox1.Controls.Add(this.radFilterVar2);
            this.groupBox1.Controls.Add(this.lblMode);
            this.groupBox1.Controls.Add(this.radFilter3);
            this.groupBox1.Controls.Add(this.radFilter4);
            this.groupBox1.Controls.Add(this.radFilter5);
            this.groupBox1.Controls.Add(this.radFilter6);
            this.groupBox1.Controls.Add(this.radFilter7);
            this.groupBox1.Controls.Add(this.radFilter8);
            this.groupBox1.Controls.Add(this.radFilter9);
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(160, 128);
            this.groupBox1.TabIndex = 56;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.udWidth);
            this.groupBox2.Controls.Add(this.lblWidth);
            this.groupBox2.Controls.Add(this.udHigh);
            this.groupBox2.Controls.Add(this.lblLow);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtName);
            this.groupBox2.Controls.Add(this.lblName);
            this.groupBox2.Controls.Add(this.udLow);
            this.groupBox2.Location = new System.Drawing.Point(176, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(160, 128);
            this.groupBox2.TabIndex = 57;
            this.groupBox2.TabStop = false;
            // 
            // udWidth
            // 
            this.udWidth.Location = new System.Drawing.Point(72, 88);
            this.udWidth.Maximum = new decimal(new int[] {
            20000,
            0,
            0,
            0});
            this.udWidth.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.udWidth.Name = "udWidth";
            this.udWidth.Size = new System.Drawing.Size(64, 20);
            this.udWidth.TabIndex = 56;
            this.udWidth.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.udWidth.ValueChanged += new System.EventHandler(this.udWidth_ValueChanged);
            // 
            // lblWidth
            // 
            this.lblWidth.Location = new System.Drawing.Point(8, 88);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(64, 23);
            this.lblWidth.TabIndex = 57;
            this.lblWidth.Text = "Width:";
            // 
            // picDisplay
            // 
            this.picDisplay.BackColor = System.Drawing.SystemColors.ControlText;
            this.picDisplay.Location = new System.Drawing.Point(8, 144);
            this.picDisplay.Name = "picDisplay";
            this.picDisplay.Size = new System.Drawing.Size(328, 50);
            this.picDisplay.TabIndex = 58;
            this.picDisplay.TabStop = false;
            this.picDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.picDisplay_Paint);
            this.picDisplay.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDisplay_MouseDown);
            this.picDisplay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picDisplay_MouseMove);
            this.picDisplay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picDisplay_MouseUp);
            // 
            // FilterForm
            // 
            this.ClientSize = new System.Drawing.Size(344, 206);
            this.Controls.Add(this.picDisplay);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FilterForm";
            this.Text = "Filter Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FilterForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.udLow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udHigh)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDisplay)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		#region Properties

		private Filter current_filter = Filter.F1;
		public Filter CurrentFilter
		{
			get { return current_filter; }
			set
			{
				current_filter = value;

				switch(current_filter)
				{
					case Filter.F1:
						radFilter1.Checked = true;
						break;
					case Filter.F2:
						radFilter2.Checked = true;
						break;
					case Filter.F3:
						radFilter3.Checked = true;
						break;
					case Filter.F4:
						radFilter4.Checked = true;
						break;
					case Filter.F5:
						radFilter5.Checked = true;
						break;
					case Filter.F6:
						radFilter6.Checked = true;
						break;
					case Filter.F7:
						radFilter7.Checked = true;
						break;
					case Filter.F8:
						radFilter8.Checked = true;
						break;
					case Filter.F9:
						radFilter9.Checked = true;
						break;
					case Filter.F10:
						radFilter10.Checked = true;
						break;
					case Filter.VAR1:
						radFilterVar1.Checked = true;
						break;
					case Filter.VAR2:
						radFilterVar2.Checked = true;
						break;
				}

				GetFilterInfo();
			}
		}

		private DSPMode dsp_mode = DSPMode.FIRST;
		public DSPMode DSPMode
		{
			get { return dsp_mode; }
			set
			{
				dsp_mode = value;

				switch(dsp_mode)
				{
					case DSPMode.LSB:
						comboDSPMode.Text = "LSB";
						break;
					case DSPMode.USB:
						comboDSPMode.Text = "USB";
						break;
					case DSPMode.DSB:
						comboDSPMode.Text = "DSB";
						break;
					case DSPMode.CWL:
						comboDSPMode.Text = "CWL";
						break;
					case DSPMode.CWU:
						comboDSPMode.Text = "CWU";
						break;
					case DSPMode.FM:
						comboDSPMode.Text = "FMN";
						break;
					case DSPMode.AM:
						comboDSPMode.Text = "AM";
						break;
					case DSPMode.SAM:
						comboDSPMode.Text = "SAM";
						break;
					case DSPMode.DIGL:
						comboDSPMode.Text = "DIGL";
						break;
					case DSPMode.DIGU:
						comboDSPMode.Text = "DIGU";
						break;
				}

				radFilter1.Text = preset[(int)value].GetName(Filter.F1);
				radFilter2.Text = preset[(int)value].GetName(Filter.F2);
				radFilter3.Text = preset[(int)value].GetName(Filter.F3);
				radFilter4.Text = preset[(int)value].GetName(Filter.F4);
				radFilter5.Text = preset[(int)value].GetName(Filter.F5);
				radFilter6.Text = preset[(int)value].GetName(Filter.F6);
				radFilter7.Text = preset[(int)value].GetName(Filter.F7);
				radFilter8.Text = preset[(int)value].GetName(Filter.F8);
				radFilter9.Text = preset[(int)value].GetName(Filter.F9);
				radFilter10.Text = preset[(int)value].GetName(Filter.F10);
				radFilterVar1.Text = preset[(int)value].GetName(Filter.VAR1);
				radFilterVar2.Text = preset[(int)value].GetName(Filter.VAR2);
				GetFilterInfo();
			}
		}

		#endregion

		#region Misc Routines

		private void GetFilterInfo()
		{
			DSPMode m = DSPMode.FIRST;
			Filter f = Filter.FIRST;

			m = (DSPMode)Enum.Parse(typeof(DSPMode), comboDSPMode.Text);
			f = current_filter;

			txtName.Text = preset[(int)m].GetName(f);
			UpdateFilter(preset[(int)m].GetLow(f), preset[(int)m].GetHigh(f));
		}

		private int HzToPixel(float freq)
		{
			int	low = (int)(-10000*console.SampleRateRX1/48000.0);
			int	high = (int)(10000*console.SampleRateRX1/48000.0);

			return picDisplay.Width/2+(int)(freq/(high-low)*picDisplay.Width);
		}

		private float PixelToHz(float x)
		{
			int low = (int)(-10000*console.SampleRateRX1/48000.0);
			int high = (int)(10000*console.SampleRateRX1/48000.0);
			
			return (float)(low + ((double)x*(high - low)/picDisplay.Width));
		}

		private bool _filter_updating = false;
		private void UpdateFilter(int low, int high)
		{
			if (_filter_updating) return; //[2.10.3.9]MW0LGE prevent update if already happening

			_filter_updating = true;

			if(low < udLow.Minimum) low = (int)udLow.Minimum;
			if(low > udLow.Maximum) low = (int)udLow.Maximum;
			if(high < udHigh.Minimum) high = (int)udHigh.Minimum;
			if(high > udHigh.Maximum) high = (int)udHigh.Maximum;
			
			udLow.Value = low;
			udHigh.Value = high;

			udWidth.Value = high - low;

			_filter_updating = false;
		}

		#endregion

		#region Event Handlers

		private void radFilter_CheckedChanged(object sender, System.EventArgs e)
		{
			RadioButtonTS r = (RadioButtonTS)sender;
			if(((RadioButtonTS)sender).Checked)
			{
				string filter = r.Name.Substring(r.Name.IndexOf("Filter")+6);
				if(!filter.StartsWith("V")) filter = "F"+filter;
				else filter = filter.ToUpper();
				CurrentFilter = (Filter)Enum.Parse(typeof(Filter), filter);
				r.BackColor = console.ButtonSelectedColor;
			}
			else
			{
				r.BackColor = SystemColors.Control;
			}
		}

		private void comboDSPMode_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			DSPMode = (DSPMode)Enum.Parse(typeof(DSPMode), comboDSPMode.Text);
		}

		private void txtName_LostFocus(object sender, System.EventArgs e)
		{
			string old_name = preset[(int)dsp_mode].GetName(current_filter);
            preset[(int)dsp_mode].SetName(current_filter, txtName.Text);
			GetFilterInfo();
			if(!rx2)
			{
				if(console.RX1DSPMode == dsp_mode)
					console.UpdateRX1FilterNames(current_filter, old_name, txtName.Text);
			}
			else
			{
				if(console.RX2DSPMode == dsp_mode)
					console.UpdateRX2FilterNames(current_filter, old_name, txtName.Text);
			}

			switch(current_filter)
			{
				case Filter.F1:
					radFilter1.Text = preset[(int)dsp_mode].GetName(Filter.F1);
					break;
				case Filter.F2:
					radFilter2.Text = preset[(int)dsp_mode].GetName(Filter.F2);
					break;
				case Filter.F3:
					radFilter3.Text = preset[(int)dsp_mode].GetName(Filter.F3);
					break;
				case Filter.F4:
					radFilter4.Text = preset[(int)dsp_mode].GetName(Filter.F4);
					break;
				case Filter.F5:
					radFilter5.Text = preset[(int)dsp_mode].GetName(Filter.F5);
					break;
				case Filter.F6:
					radFilter6.Text = preset[(int)dsp_mode].GetName(Filter.F6);
					break;
				case Filter.F7:
					radFilter7.Text = preset[(int)dsp_mode].GetName(Filter.F7);
					break;
				case Filter.F8:
					radFilter8.Text = preset[(int)dsp_mode].GetName(Filter.F8);
					break;
				case Filter.F9:
					radFilter9.Text = preset[(int)dsp_mode].GetName(Filter.F9);
					break;
				case Filter.F10:
					radFilter10.Text = preset[(int)dsp_mode].GetName(Filter.F10);
					break;
				case Filter.VAR1:
					radFilterVar1.Text = preset[(int)dsp_mode].GetName(Filter.VAR1);
					break;
				case Filter.VAR2:
					radFilterVar2.Text = preset[(int)dsp_mode].GetName(Filter.VAR2);
					break;
			}
		}

		private void udLow_ValueChanged(object sender, System.EventArgs e)
		{
			if(udLow.Value + 10 > udHigh.Value/* && !_filter_updating*/) udLow.Value = udHigh.Value - 10;
			preset[(int)dsp_mode].SetLow(current_filter, (int)udLow.Value);
			if(!rx2)
			{
				if(console.RX1DSPMode == dsp_mode &&
					console.RX1Filter == current_filter)
					console.UpdateRX1FilterPresetLow((int)udLow.Value);
			}
			else
			{
				if(console.RX2DSPMode == dsp_mode &&
					console.RX2Filter == current_filter)
					console.UpdateRX2FilterPresetLow((int)udLow.Value);
			}
			/*if(!_filter_updating) */UpdateFilter((int)udLow.Value, (int)udHigh.Value);
			picDisplay.Invalidate();
		}

		private void udHigh_ValueChanged(object sender, System.EventArgs e)
		{
			if(udHigh.Value - 10 < udLow.Value/* && !_filter_updating*/) udHigh.Value = udLow.Value + 10;
			preset[(int)dsp_mode].SetHigh(current_filter, (int)udHigh.Value);
			if(!rx2)
			{
				if(console.RX1DSPMode == dsp_mode &&
					console.RX1Filter == current_filter)
					console.UpdateRX1FilterPresetHigh((int)udHigh.Value);
			}
			else
			{
				if(console.RX2DSPMode == dsp_mode &&
					console.RX2Filter == current_filter)
					console.UpdateRX2FilterPresetHigh((int)udHigh.Value);
			}
			/*if(!_filter_updating) */UpdateFilter((int)udLow.Value, (int)udHigh.Value);
			picDisplay.Invalidate();
		}	

		private void udLow_LostFocus(object sender, EventArgs e)
		{
			udLow_ValueChanged(sender, e);
		}

		private void udHigh_LostFocus(object sender, EventArgs e)
		{
			udHigh_ValueChanged(sender, e);
		}

		private void picDisplay_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			// draw background
			e.Graphics.FillRectangle(
				new SolidBrush(Display.DisplayBackgroundColor),
				0, 0, picDisplay.Width, picDisplay.Height);

			e.Graphics.FillRectangle(
				new SolidBrush(Display.DisplayFilterColor),
				HzToPixel((int)udLow.Value), 0, 
				Math.Max(1, HzToPixel((int)udHigh.Value)-HzToPixel((int)udLow.Value)), picDisplay.Height);

			// draw center line
			e.Graphics.DrawLine(new Pen(Display.GridZeroColor, 1.0f),
				picDisplay.Width/2, 0, picDisplay.Width/2, picDisplay.Height);
		}

		private void picDisplay_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			int low = HzToPixel((float)udLow.Value);
			int high = HzToPixel((float)udHigh.Value);

			if(Math.Abs(e.X - low) < 2 || Math.Abs(e.X - high) < 2)
				Cursor = Cursors.SizeWE;
			else if(e.X > low && e.X < high)
				Cursor = Cursors.NoMoveHoriz;
			else 
				Cursor = Cursors.Arrow;

			if(drag_low) udLow.Value = Math.Max(Math.Min(udLow.Maximum, (int)PixelToHz((float)e.X)), udLow.Minimum);
			if(drag_high) udHigh.Value = Math.Max(Math.Min(udHigh.Maximum, (int)PixelToHz((float)e.X)), udHigh.Minimum);;
			if(drag_filter)
			{
				int delta = (int)(PixelToHz((float)e.X) - PixelToHz(drag_filter_start));
				udLow.Value = Math.Max(Math.Min(udLow.Maximum, drag_filter_low + delta), udLow.Minimum);
				udHigh.Value = Math.Max(Math.Min(udHigh.Maximum, drag_filter_high + delta), udHigh.Minimum);
			}
		}

		private bool drag_low = false;
		private bool drag_high = false;
		private bool drag_filter = false;
		private int drag_filter_low = -1;
		private int drag_filter_high = -1;
		private int drag_filter_start = -1;

		private void picDisplay_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left)
			{
				int low = HzToPixel((float)udLow.Value);
				int high = HzToPixel((float)udHigh.Value);

				if(Math.Abs(e.X - low) < 2)
					drag_low = true;
				else if(Math.Abs(e.X - high) < 2)
					drag_high = true;
				else if(e.X > low && e.X < high)
				{
					drag_filter = true;
					drag_filter_low = (int)udLow.Value;
					drag_filter_high = (int)udHigh.Value;
					drag_filter_start = e.X;
				}
			}
		}

		private void picDisplay_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left)
			{
				drag_low = false;
				drag_high = false;
				drag_filter = false;
				drag_filter_low = -1;
				drag_filter_high = -1;
				drag_filter_start = -1;
			}
		}

		private void udWidth_ValueChanged(object sender, System.EventArgs e)
		{
			if(udWidth.Focused)
			{
				int low = 0, high = 0;
				switch(comboDSPMode.Text)
				{
					case "CWL":
						low = (int)(-console.CWPitch - udWidth.Value/2);
						high = (int)(-console.CWPitch + udWidth.Value/2);
						break;
					case "CWU":
						low = (int)(console.CWPitch - udWidth.Value/2);
						high = (int)(console.CWPitch + udWidth.Value/2);
						break;
                    case "DIGL":
                        low = (int)(-console.DIGLClickTuneOffset - udWidth.Value / 2);  //W4TME
                        high = (int)(-console.DIGLClickTuneOffset + udWidth.Value / 2); //W4TME
                        break;
                    case "DIGU":
                        low = (int)(console.DIGUClickTuneOffset - udWidth.Value / 2);  //W4TME
                        high = (int)(console.DIGUClickTuneOffset + udWidth.Value / 2); //W4TME
                        break;
					case "LSB":
						high = -console.DefaultLowCut;
						low = high - (int)udWidth.Value;						
						break;
					case "USB":
						low = console.DefaultLowCut;
						high = low + (int)udWidth.Value;
						break;
					case "AM":
					case "SAM":
					case "FMN":
						low = -(int)udWidth.Value/2;
						high = (int)udWidth.Value/2;
						break;
				}

				/*if(!_filter_updating) */UpdateFilter(low, high);
			}
			
		}
        private void FilterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Common.SaveForm(this, "FilterForm");
        }
        #endregion
    }
}
