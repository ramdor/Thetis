//=================================================================
// wave_options.cs
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
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//

using System.Windows.Forms;

namespace Thetis
{
	/// <summary>
	/// Summary description for WaveOptions.
	/// </summary>
	public class WaveOptions : Form
	{
		#region Variable Declaration

		private System.Windows.Forms.GroupBoxTS grpReceive;
		public System.Windows.Forms.RadioButtonTS radRXPreProcessed;
		private System.Windows.Forms.GroupBoxTS groupBox1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.RadioButtonTS radRXPostProcessed;
		private System.Windows.Forms.RadioButtonTS radTXPostProcessed;
		public System.Windows.Forms.RadioButtonTS radTXPreProcessed;
		private System.Windows.Forms.GroupBoxTS grpAudioSampleRate1;
		public System.Windows.Forms.ComboBoxTS comboSampleRate;
        private GroupBoxTS grpBitDepth;
        private RadioButtonTS radBitDepth8PCM;
        private RadioButtonTS radBitDepth16PCM;
        private RadioButtonTS radBitDepth24PCM;
        private RadioButtonTS radBitDepth32PCM;
        private RadioButtonTS radBitDepthIEEE;
        private GroupBoxTS grpWaveDither;
        private CheckBoxTS chkWaveDither;
        private NumericUpDownTS udWaveDitherBits;
		private System.ComponentModel.IContainer components;

		#endregion

		#region Constructor and Destructor

		public WaveOptions()
		{
			//
			// Required for Windows Form Designer support
			//
            InitializeComponent();
            if (comboSampleRate.Text == "") 
			comboSampleRate.Text = "48000";
			Common.RestoreForm(this, "WaveOptions", false);

            radRXPreProcessed_CheckedChanged(this, System.EventArgs.Empty);
            radRXPostProcessed_CheckedChanged(this, System.EventArgs.Empty);
            radTXPreProcessed_CheckedChanged(this, System.EventArgs.Empty);
            radTXPostProcessed_CheckedChanged(this, System.EventArgs.Empty);
            radBitDepthIEEE_CheckedChanged(this, System.EventArgs.Empty);
            radBitDepth32PCM_CheckedChanged(this, System.EventArgs.Empty);
            radBitDepth24PCM_CheckedChanged(this, System.EventArgs.Empty);
            radBitDepth16PCM_CheckedChanged(this, System.EventArgs.Empty);
            radBitDepth8PCM_CheckedChanged(this, System.EventArgs.Empty);
            chkWaveDither_CheckedChanged(this, System.EventArgs.Empty);
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaveOptions));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.chkWaveDither = new System.Windows.Forms.CheckBoxTS();
            this.comboSampleRate = new System.Windows.Forms.ComboBoxTS();
            this.radTXPostProcessed = new System.Windows.Forms.RadioButtonTS();
            this.radTXPreProcessed = new System.Windows.Forms.RadioButtonTS();
            this.radRXPostProcessed = new System.Windows.Forms.RadioButtonTS();
            this.radRXPreProcessed = new System.Windows.Forms.RadioButtonTS();
            this.grpWaveDither = new System.Windows.Forms.GroupBoxTS();
            this.udWaveDitherBits = new System.Windows.Forms.NumericUpDownTS();
            this.grpBitDepth = new System.Windows.Forms.GroupBoxTS();
            this.radBitDepth8PCM = new System.Windows.Forms.RadioButtonTS();
            this.radBitDepth16PCM = new System.Windows.Forms.RadioButtonTS();
            this.radBitDepth24PCM = new System.Windows.Forms.RadioButtonTS();
            this.radBitDepth32PCM = new System.Windows.Forms.RadioButtonTS();
            this.radBitDepthIEEE = new System.Windows.Forms.RadioButtonTS();
            this.grpAudioSampleRate1 = new System.Windows.Forms.GroupBoxTS();
            this.groupBox1 = new System.Windows.Forms.GroupBoxTS();
            this.grpReceive = new System.Windows.Forms.GroupBoxTS();
            this.grpWaveDither.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udWaveDitherBits)).BeginInit();
            this.grpBitDepth.SuspendLayout();
            this.grpAudioSampleRate1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grpReceive.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkWaveDither
            // 
            this.chkWaveDither.AutoSize = true;
            this.chkWaveDither.Image = null;
            this.chkWaveDither.Location = new System.Drawing.Point(6, 19);
            this.chkWaveDither.Name = "chkWaveDither";
            this.chkWaveDither.Size = new System.Drawing.Size(59, 17);
            this.chkWaveDither.TabIndex = 0;
            this.chkWaveDither.Text = "Enable";
            this.toolTip1.SetToolTip(this.chkWaveDither, "Enables Dithering for Recording");
            this.chkWaveDither.UseVisualStyleBackColor = true;
            this.chkWaveDither.CheckedChanged += new System.EventHandler(this.chkWaveDither_CheckedChanged);
            // 
            // comboSampleRate
            // 
            this.comboSampleRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSampleRate.DropDownWidth = 64;
            this.comboSampleRate.Items.AddRange(new object[] {
            "6000",
            "12000",
            "24000",
            "48000",
            "96000",
            "192000",
            "384000",
            "768000",
            "1536000"});
            this.comboSampleRate.Location = new System.Drawing.Point(16, 24);
            this.comboSampleRate.Name = "comboSampleRate";
            this.comboSampleRate.Size = new System.Drawing.Size(64, 21);
            this.comboSampleRate.TabIndex = 4;
            this.toolTip1.SetToolTip(this.comboSampleRate, "Sample Rate -- Range is dependent on selected sound card! ");
            // 
            // radTXPostProcessed
            // 
            this.radTXPostProcessed.Image = null;
            this.radTXPostProcessed.Location = new System.Drawing.Point(16, 48);
            this.radTXPostProcessed.Name = "radTXPostProcessed";
            this.radTXPostProcessed.Size = new System.Drawing.Size(144, 24);
            this.radTXPostProcessed.TabIndex = 1;
            this.radTXPostProcessed.Text = "Transmitter Output I/Q";
            this.toolTip1.SetToolTip(this.radTXPostProcessed, "Transmitter I/Q data outbound to the hardware.");
            this.radTXPostProcessed.CheckedChanged += new System.EventHandler(this.radTXPostProcessed_CheckedChanged);
            // 
            // radTXPreProcessed
            // 
            this.radTXPreProcessed.Checked = true;
            this.radTXPreProcessed.Image = null;
            this.radTXPreProcessed.Location = new System.Drawing.Point(16, 24);
            this.radTXPreProcessed.Name = "radTXPreProcessed";
            this.radTXPreProcessed.Size = new System.Drawing.Size(144, 24);
            this.radTXPreProcessed.TabIndex = 0;
            this.radTXPreProcessed.TabStop = true;
            this.radTXPreProcessed.Text = "MIC Audio";
            this.toolTip1.SetToolTip(this.radTXPreProcessed, "Raw MIC Audio Input coming from the hardware or from VAC input.");
            this.radTXPreProcessed.CheckedChanged += new System.EventHandler(this.radTXPreProcessed_CheckedChanged);
            // 
            // radRXPostProcessed
            // 
            this.radRXPostProcessed.Checked = true;
            this.radRXPostProcessed.Image = null;
            this.radRXPostProcessed.Location = new System.Drawing.Point(16, 48);
            this.radRXPostProcessed.Name = "radRXPostProcessed";
            this.radRXPostProcessed.Size = new System.Drawing.Size(144, 24);
            this.radRXPostProcessed.TabIndex = 1;
            this.radRXPostProcessed.TabStop = true;
            this.radRXPostProcessed.Text = "Receiver Output Audio";
            this.toolTip1.SetToolTip(this.radRXPostProcessed, "The demodulated filtered audio you listen to.");
            this.radRXPostProcessed.CheckedChanged += new System.EventHandler(this.radRXPostProcessed_CheckedChanged);
            // 
            // radRXPreProcessed
            // 
            this.radRXPreProcessed.Image = null;
            this.radRXPreProcessed.Location = new System.Drawing.Point(16, 24);
            this.radRXPreProcessed.Name = "radRXPreProcessed";
            this.radRXPreProcessed.Size = new System.Drawing.Size(144, 24);
            this.radRXPreProcessed.TabIndex = 0;
            this.radRXPreProcessed.Text = "Receiver Input I/Q";
            this.toolTip1.SetToolTip(this.radRXPreProcessed, "Raw I/Q Data coming from the hardware.");
            this.radRXPreProcessed.CheckedChanged += new System.EventHandler(this.radRXPreProcessed_CheckedChanged);
            // 
            // grpWaveDither
            // 
            this.grpWaveDither.Controls.Add(this.udWaveDitherBits);
            this.grpWaveDither.Controls.Add(this.chkWaveDither);
            this.grpWaveDither.Location = new System.Drawing.Point(295, 120);
            this.grpWaveDither.Name = "grpWaveDither";
            this.grpWaveDither.Size = new System.Drawing.Size(96, 69);
            this.grpWaveDither.TabIndex = 38;
            this.grpWaveDither.TabStop = false;
            this.grpWaveDither.Text = "Dither";
            // 
            // udWaveDitherBits
            // 
            this.udWaveDitherBits.DecimalPlaces = 1;
            this.udWaveDitherBits.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.udWaveDitherBits.Location = new System.Drawing.Point(6, 41);
            this.udWaveDitherBits.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            this.udWaveDitherBits.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.udWaveDitherBits.Name = "udWaveDitherBits";
            this.udWaveDitherBits.Size = new System.Drawing.Size(40, 20);
            this.udWaveDitherBits.TabIndex = 1;
            this.udWaveDitherBits.TinyStep = false;
            this.udWaveDitherBits.Value = new decimal(new int[] {
            8,
            0,
            0,
            65536});
            // 
            // grpBitDepth
            // 
            this.grpBitDepth.Controls.Add(this.radBitDepth8PCM);
            this.grpBitDepth.Controls.Add(this.radBitDepth16PCM);
            this.grpBitDepth.Controls.Add(this.radBitDepth24PCM);
            this.grpBitDepth.Controls.Add(this.radBitDepth32PCM);
            this.grpBitDepth.Controls.Add(this.radBitDepthIEEE);
            this.grpBitDepth.Location = new System.Drawing.Point(207, 8);
            this.grpBitDepth.Name = "grpBitDepth";
            this.grpBitDepth.Size = new System.Drawing.Size(168, 106);
            this.grpBitDepth.TabIndex = 37;
            this.grpBitDepth.TabStop = false;
            this.grpBitDepth.Text = "Bit Depth";
            // 
            // radBitDepth8PCM
            // 
            this.radBitDepth8PCM.Image = null;
            this.radBitDepth8PCM.Location = new System.Drawing.Point(15, 83);
            this.radBitDepth8PCM.Name = "radBitDepth8PCM";
            this.radBitDepth8PCM.Size = new System.Drawing.Size(144, 18);
            this.radBitDepth8PCM.TabIndex = 4;
            this.radBitDepth8PCM.Text = "8-Bit UnSigned PCM";
            this.radBitDepth8PCM.CheckedChanged += new System.EventHandler(this.radBitDepth8PCM_CheckedChanged);
            // 
            // radBitDepth16PCM
            // 
            this.radBitDepth16PCM.Image = null;
            this.radBitDepth16PCM.Location = new System.Drawing.Point(15, 66);
            this.radBitDepth16PCM.Name = "radBitDepth16PCM";
            this.radBitDepth16PCM.Size = new System.Drawing.Size(144, 18);
            this.radBitDepth16PCM.TabIndex = 3;
            this.radBitDepth16PCM.Text = "16-Bit Signed PCM";
            this.radBitDepth16PCM.CheckedChanged += new System.EventHandler(this.radBitDepth16PCM_CheckedChanged);
            // 
            // radBitDepth24PCM
            // 
            this.radBitDepth24PCM.Image = null;
            this.radBitDepth24PCM.Location = new System.Drawing.Point(15, 49);
            this.radBitDepth24PCM.Name = "radBitDepth24PCM";
            this.radBitDepth24PCM.Size = new System.Drawing.Size(144, 18);
            this.radBitDepth24PCM.TabIndex = 2;
            this.radBitDepth24PCM.Text = "24-Bit Signed PCM";
            this.radBitDepth24PCM.CheckedChanged += new System.EventHandler(this.radBitDepth24PCM_CheckedChanged);
            // 
            // radBitDepth32PCM
            // 
            this.radBitDepth32PCM.Image = null;
            this.radBitDepth32PCM.Location = new System.Drawing.Point(15, 32);
            this.radBitDepth32PCM.Name = "radBitDepth32PCM";
            this.radBitDepth32PCM.Size = new System.Drawing.Size(144, 18);
            this.radBitDepth32PCM.TabIndex = 1;
            this.radBitDepth32PCM.Text = "32-Bit Signed PCM";
            this.radBitDepth32PCM.CheckedChanged += new System.EventHandler(this.radBitDepth32PCM_CheckedChanged);
            // 
            // radBitDepthIEEE
            // 
            this.radBitDepthIEEE.Checked = true;
            this.radBitDepthIEEE.Image = null;
            this.radBitDepthIEEE.Location = new System.Drawing.Point(15, 15);
            this.radBitDepthIEEE.Name = "radBitDepthIEEE";
            this.radBitDepthIEEE.Size = new System.Drawing.Size(144, 16);
            this.radBitDepthIEEE.TabIndex = 0;
            this.radBitDepthIEEE.TabStop = true;
            this.radBitDepthIEEE.Text = "32-Bit IEEE Floats *";
            this.radBitDepthIEEE.CheckedChanged += new System.EventHandler(this.radBitDepthIEEE_CheckedChanged);
            // 
            // grpAudioSampleRate1
            // 
            this.grpAudioSampleRate1.Controls.Add(this.comboSampleRate);
            this.grpAudioSampleRate1.Location = new System.Drawing.Point(182, 120);
            this.grpAudioSampleRate1.Name = "grpAudioSampleRate1";
            this.grpAudioSampleRate1.Size = new System.Drawing.Size(96, 56);
            this.grpAudioSampleRate1.TabIndex = 36;
            this.grpAudioSampleRate1.TabStop = false;
            this.grpAudioSampleRate1.Text = "Sample Rate";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radTXPostProcessed);
            this.groupBox1.Controls.Add(this.radTXPreProcessed);
            this.groupBox1.Location = new System.Drawing.Point(8, 109);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(168, 80);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "When Transmitting, Record ...";
            // 
            // grpReceive
            // 
            this.grpReceive.Controls.Add(this.radRXPostProcessed);
            this.grpReceive.Controls.Add(this.radRXPreProcessed);
            this.grpReceive.Location = new System.Drawing.Point(8, 8);
            this.grpReceive.Name = "grpReceive";
            this.grpReceive.Size = new System.Drawing.Size(168, 80);
            this.grpReceive.TabIndex = 0;
            this.grpReceive.TabStop = false;
            this.grpReceive.Text = "When Receiving, Record ...";
            // 
            // WaveOptions
            // 
            this.ClientSize = new System.Drawing.Size(403, 201);
            this.Controls.Add(this.grpWaveDither);
            this.Controls.Add(this.grpBitDepth);
            this.Controls.Add(this.grpAudioSampleRate1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grpReceive);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WaveOptions";
            this.Text = "Wave Record Options";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.WaveOptions_Closing);
            this.grpWaveDither.ResumeLayout(false);
            this.grpWaveDither.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udWaveDitherBits)).EndInit();
            this.grpBitDepth.ResumeLayout(false);
            this.grpAudioSampleRate1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.grpReceive.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		#region Properties

		public int SampleRate
		{
			get { return int.Parse(comboSampleRate.Text); }
		}
        public bool temp_record; // ke9ns add save the status of pre to put back when done
        //-------------------------------------------------------------------------------
        // ke9ns add  to force audio into POST mode (for quick audio and TX waterfall ID and scheduler)
        public bool RECPLAY1
        {
            get { return false; }
            set
            {

                temp_record = Audio.RecordRXPreProcessed; // save original value


                radRXPostProcessed.Checked = value;
                radTXPostProcessed.Checked = value;

                Audio.RecordRXPreProcessed = false;
                Audio.RecordTXPreProcessed = false;
            }

        }

		#endregion

		#region Event Handler

		private void WaveOptions_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.Hide();
			e.Cancel = true;
			Common.SaveForm(this, "WaveOptions");
		}

		private void radRXPreProcessed_CheckedChanged(object sender, System.EventArgs e)
		{
			if(radRXPreProcessed.Checked)
			{
				Audio.RecordRXPreProcessed = true;
			}
		}

		private void radRXPostProcessed_CheckedChanged(object sender, System.EventArgs e)
		{
			if(radRXPostProcessed.Checked)
			{
				Audio.RecordRXPreProcessed = false;
			}
		}

		private void radTXPreProcessed_CheckedChanged(object sender, System.EventArgs e)
		{
			if(radTXPreProcessed.Checked)
			{
				Audio.RecordTXPreProcessed = true;
			}
		}

		private void radTXPostProcessed_CheckedChanged(object sender, System.EventArgs e)
		{
			if(radTXPostProcessed.Checked)
			{
				Audio.RecordTXPreProcessed = false;
			}
		}

		#endregion


        private void radBitDepthIEEE_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radBitDepthIEEE.Checked)
            {
                Audio.BitDepth = 32;
                Audio.FormatTag = 3;
            }
        }

        private void radBitDepth32PCM_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radBitDepth32PCM.Checked)
            {
                Audio.BitDepth = 32;
                Audio.FormatTag = 1;
            }
        }

        private void radBitDepth24PCM_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radBitDepth24PCM.Checked)
            {
                Audio.BitDepth = 24;
                Audio.FormatTag = 1;
            }
        }

        private void radBitDepth16PCM_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radBitDepth16PCM.Checked)
            {
                Audio.BitDepth = 16;
                Audio.FormatTag = 1;
            }
        }

        private void radBitDepth8PCM_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radBitDepth8PCM.Checked)
            {
                Audio.BitDepth = 8;
                Audio.FormatTag = 1;
            }
        }

        private void chkWaveDither_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chkWaveDither.Checked)
                WaveFileWriter.dither = true;
            else
                WaveFileWriter.dither = false;
        }
	}
}
