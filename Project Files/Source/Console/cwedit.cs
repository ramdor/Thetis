//=================================================================
// cwedit.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2005, 2006  Richard Allen
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

namespace Thetis
{
    using System.Diagnostics;
    using System.Windows.Forms;

	/// <summary>
	/// Summary description for cwedit.
	/// </summary>
	public class cwedit : Form
	{
		#region startup/exit stuff
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.TextBox txtElements;
		private System.Windows.Forms.TextBox txtComments;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button saveButton;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.TextBox txtOriginal;
		private System.Windows.Forms.TextBox txtCurrent;
		private Console console;

		public cwedit(Console c)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			console = c;
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.saveButton = new System.Windows.Forms.Button();
			this.txtElements = new System.Windows.Forms.TextBox();
			this.txtComments = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.cancelButton = new System.Windows.Forms.Button();
			this.txtOriginal = new System.Windows.Forms.TextBox();
			this.txtCurrent = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// saveButton
			// 
			this.saveButton.Location = new System.Drawing.Point(192, 200);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(64, 24);
			this.saveButton.TabIndex = 0;
			this.saveButton.Text = "Save";
			this.toolTip1.SetToolTip(this.saveButton, " Save new definition and exit.");
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// txtElements
			// 
			this.txtElements.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.txtElements.Location = new System.Drawing.Point(40, 152);
			this.txtElements.MaxLength = 10;
			this.txtElements.Name = "txtElements";
			this.txtElements.Size = new System.Drawing.Size(80, 21);
			this.txtElements.TabIndex = 1;
			this.txtElements.Text = "---...---";
			this.toolTip1.SetToolTip(this.txtElements, " The Morse dots and dashes up to nine.");
			this.txtElements.Leave += new System.EventHandler(this.txtElements_Leave);
			// 
			// txtComments
			// 
			this.txtComments.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.txtComments.Location = new System.Drawing.Point(40, 208);
			this.txtComments.MaxLength = 11;
			this.txtComments.Name = "txtComments";
			this.txtComments.Size = new System.Drawing.Size(80, 21);
			this.txtComments.TabIndex = 2;
			this.txtComments.Text = "0123456789";
			this.toolTip1.SetToolTip(this.txtComments, " Any comments up to ten characters.");
			this.txtComments.Leave += new System.EventHandler(this.txtComments_Leave);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(40, 176);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 16);
			this.label2.TabIndex = 4;
			this.label2.Text = "Elements";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(40, 232);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "Comments";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(40, 52);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(120, 16);
			this.label4.TabIndex = 6;
			this.label4.Text = "Original Definition";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(40, 104);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(96, 16);
			this.label5.TabIndex = 8;
			this.label5.Text = "Current Definition";
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(192, 160);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(64, 24);
			this.cancelButton.TabIndex = 9;
			this.cancelButton.Text = "Cancel";
			this.toolTip1.SetToolTip(this.cancelButton, "Cancel and quite without changes.");
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// txtOriginal
			// 
			this.txtOriginal.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.txtOriginal.Location = new System.Drawing.Point(40, 32);
			this.txtOriginal.Name = "txtOriginal";
			this.txtOriginal.ReadOnly = true;
			this.txtOriginal.Size = new System.Drawing.Size(216, 21);
			this.txtOriginal.TabIndex = 10;
			this.txtOriginal.Text = "txtOriginal";
			this.toolTip1.SetToolTip(this.txtOriginal, " The original definition line.");
			// 
			// txtCurrent
			// 
			this.txtCurrent.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.txtCurrent.Location = new System.Drawing.Point(40, 80);
			this.txtCurrent.Name = "txtCurrent";
			this.txtCurrent.ReadOnly = true;
			this.txtCurrent.Size = new System.Drawing.Size(216, 21);
			this.txtCurrent.TabIndex = 11;
			this.txtCurrent.Text = "txtCurrent";
			this.toolTip1.SetToolTip(this.txtCurrent, " The current definition line.");
			// 
			// cwedit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(5, 13);
			this.ClientSize = new System.Drawing.Size(290, 258);
			this.ControlBox = false;
			this.Controls.Add(this.txtCurrent);
			this.Controls.Add(this.txtOriginal);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtComments);
			this.Controls.Add(this.txtElements);
			this.Controls.Add(this.saveButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "cwedit";
			this.Text = " CW definition editor ...";
			this.Load += new System.EventHandler(this.cwedit_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Morse code definition editor

		private string sedit;

           		   //           11111111112222222222
		           // 012345678901234567890123456789
	//	sw.WriteLine("37|%|.-...    | [AS]      ");
	//	sw.WriteLine("38|&|.........| 0123456789");
	//	sw.WriteLine("39|'|         |           ");
	//	sw.WriteLine("40|(|-.--.    | [KN]      ");
	//	sw.WriteLine("41|)|         |           ");

        private string id,els,cmnts;

		private void extract_fields()
		{
			id = sedit.Substring(0,5);
			els = sedit.Substring(5,9);
		//	Debug.WriteLine(sedit.Length);
			cmnts = sedit.Substring(16,10);
		}

		private void make_current()
		{
			txtCurrent.Text = id + els + "| " + cmnts;
//			Debug.WriteLine("'" + txtCurrent.Text + "' " + txtCurrent.Text.Length);
		}

		private void cwedit_Load(object sender, System.EventArgs e)
		{
			sedit = console.CWXForm.editline;

			txtOriginal.Text = sedit;
			
	//		Debug.WriteLine("enter '" + sedit + "' "+ sedit.Length);
			extract_fields();
			txtElements.Text = els;
			txtComments.Text = cmnts;
			make_current();
		}

		private void saveButton_Click(object sender, System.EventArgs e)
		{
			console.CWXForm.editline = txtCurrent.Text;
			this.Close();
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			if (MessageBox.Show ("Do you want to exit without saving?", " CW Editor",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question)
				== DialogResult.Yes)
			{
				console.CWXForm.editline = "";
				this.Close();
			}
		}


		private string slen(string s,int len)
		{
			if (s.Length < len) return (s.PadRight(len,' '));
			if (s.Length > len) return (s.Substring(0,len));
			return (s);
		}

		private void txtComments_Leave(object sender, System.EventArgs e)
		{
			string s;

			s = txtComments.Text;
			s = slen(s,10);
			txtComments.Text = s;
			
			cmnts = s;
		//	Debug.WriteLine(s + " " + s.Length);
			make_current();
		}

		private void txtElements_Leave(object sender, System.EventArgs e)
		{
			string s;

			s = txtElements.Text;
			s = slen(s,9);
			txtElements.Text = s;
			
			els = s;
				Debug.WriteLine(s + " " + s.Length);
			make_current();
		}
		#endregion	
		
	} // end class
} // end namespace

