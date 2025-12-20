//=================================================================
// progress.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
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
// You may contact us via email at: sales@flex-radio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    8900 Marybank Dr.
//    Austin, TX 78750
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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Thetis
{
	public class Progress : System.Windows.Forms.Form
	{
		#region Variable Declaration

		private float percent_done;

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ButtonTS btnAbort;
		private System.ComponentModel.Container components = null;

		#endregion

		#region Constructor and Destructor

		public Progress(string s)
		{
			InitializeComponent();
            Common.DoubleBufferAll(this, true);
            this.Text = s;
			percent_done = 0.0f;
		}

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

		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Progress));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnAbort = new System.Windows.Forms.ButtonTS();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Location = new System.Drawing.Point(16, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(208, 24);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // btnAbort
            // 
            this.btnAbort.Image = null;
            this.btnAbort.Location = new System.Drawing.Point(240, 16);
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.Size = new System.Drawing.Size(75, 23);
            this.btnAbort.TabIndex = 1;
            this.btnAbort.Text = "Abort";
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // Progress
            // 
            this.ClientSize = new System.Drawing.Size(330, 56);
            this.Controls.Add(this.btnAbort);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Progress";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "progress";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Progress_Closing);
            this.ResumeLayout(false);

		}
		#endregion

		#region Misc Routines

		private delegate void Invoker(float f);

		public void SetPercent(float f)
		{
			percent_done = f * 100;;
			panel1.Invalidate();
		}

		#endregion

		#region Event Handlers

		private void btnAbort_Click(object sender, System.EventArgs e)
		{
			this.Hide();
			this.Visible = false;
		}

		private string digits = "f1";
		private int percent_digits = 1;
		public int PercentDigits
		{
			get { return percent_digits; }
			set
			{
				percent_digits = value;
				digits = "f"+percent_digits.ToString();
			}
		}

		private string percent_symbol = "%";
		public string PercentSymbol
		{
			get { return percent_symbol; }
			set { percent_symbol = value; }
		}

		private void panel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			int bar = (int)Math.Floor(panel1.Width * percent_done / 100);
			if(bar == 0) return;
			Graphics g = e.Graphics;
			g.SmoothingMode = SmoothingMode.HighQuality;
			LinearGradientBrush b = new LinearGradientBrush(
				new Rectangle(0, 0, bar, panel1.Height),
				Color.Green,
				Color.Lime,
				LinearGradientMode.Horizontal);
			
			g.FillRectangle(b, 0, 0, bar, panel1.Height);

			string s = percent_done.ToString(digits) + percent_symbol;
			SolidBrush b2 = new SolidBrush(Color.Black);
			Font f = new Font("Microsoft Sans Serif", 10);
			StringFormat sf = new StringFormat();
			sf.Alignment = StringAlignment.Center;
			sf.LineAlignment = StringAlignment.Center;

			g.DrawString(s,	f, b2, new RectangleF(0, 0, panel1.Width, panel1.Height), sf);

			//clear up //MW0LGE_[2.9.0.6]
			b.Dispose();
			b2.Dispose();
			f.Dispose();
			sf.Dispose();
		}

		private void Progress_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.Hide();
			e.Cancel = true;
		}

		#endregion		
	}
}
