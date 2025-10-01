//=================================================================
// ColorButton.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  Thomas Ascher & FlexRadio Systems
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
// Color Selection Button
// Version 1.1
// -------------------------------
// Taken From: http://www.thecodeproject.com/cs/miscctrl/ColorButton.asp
// Author: Thomas Ascher
// eMail: thomasascher@hotmail.com
// Modified By: Eric Wachsmann
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
    using System;
    using System.Drawing;
    using System.ComponentModel;
    using System.Windows.Forms;

    [ DefaultEvent ( "Changed" ) ]
	public class ColorButton : ButtonTS
	{
		private Container components = null;

		private Color buttonColor = Color.Transparent;
		private string autoButton = "Automatic";	// set to "" for not auto button
		private string moreButton = "More Colors...";
		private bool buttonPushed = false;
		private bool panelVisible = false;

		public event EventHandler Changed;

		public Color Color
		{
			get { return buttonColor; }
			set
			{
				buttonColor = value;
				this.Refresh();					// line added 5/11/04  Eric Wachsmann
				OnChanged(EventArgs.Empty);		// line added 5/11/04  Eric Wachsmann
			}
		}

		public string Automatic
		{
			get { return autoButton; }
			set { autoButton = value; }
		}

		public string MoreColors
		{
			get { return moreButton; }
			set { moreButton = value; }
		}

		protected virtual void OnChanged(EventArgs e)
		{
			if(Changed != null)
				Changed(this, e);
		}

		public ColorButton()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if( components != null )
					components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			int offset = 0 ;

			if(panelVisible || (buttonPushed &&
			RectangleToScreen(ClientRectangle).Contains(Cursor.Position)))
			{
				ControlPaint.DrawButton(e.Graphics, e.ClipRectangle, ButtonState.Pushed);
				offset = 1;
			}

			/*Rectangle rc = new Rectangle(e.ClipRectangle.Left + 5 + offset,
										e.ClipRectangle.Top + 5 + offset,
										e.ClipRectangle.Width - 24,
										e.ClipRectangle.Height - 11);*/

			// Changed to fix problem with redraw when only part of the button
			// was in the ClipRectangle.
			Rectangle rc = new Rectangle(5 + offset,
				5 + offset,
				this.Width - 24,
				this.Height - 11);

			Pen darkPen = new Pen(SystemColors.ControlDark);

			if(Enabled)
			{
				e.Graphics.FillRectangle(new SolidBrush(buttonColor), rc);
				e.Graphics.DrawRectangle(darkPen, rc);
			}

			e.Graphics.DrawLine(darkPen, rc.Right + 4, rc.Top, rc.Right + 4, rc.Bottom);
			e.Graphics.DrawLine(new Pen(SystemColors.ControlLightLight),
								rc.Right + 5, rc.Top, rc.Right + 5, rc.Bottom);

			Pen textPen = new Pen(Enabled ? SystemColors.ControlText : SystemColors.GrayText);
            //Point pt = new Point(rc.Right, (e.ClipRectangle.Height + offset) / 2);
            Point pt = new Point(rc.Right, rc.Top + (rc.Height / 2)); //[2.10.3.7]MW0LGE fixed, as it is not a good idea to use the clip rect to position the triangle
            e.Graphics.DrawLine(textPen, pt.X +  9, pt.Y - 1, pt.X + 13, pt.Y - 1);
			e.Graphics.DrawLine(textPen, pt.X + 10, pt.Y,     pt.X + 12, pt.Y    );
			e.Graphics.DrawLine(textPen, pt.X + 11, pt.Y,     pt.X + 11, pt.Y + 1);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			buttonPushed = true;
			base.OnMouseDown( e );
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			buttonPushed = false;
			base.OnMouseUp( e );
		}

		protected override void OnClick(EventArgs e)
		{
			panelVisible = true;
			Refresh();

			Point pt = Parent.PointToScreen(new Point(Left, Bottom));

			ColorPanel panel = new ColorPanel( pt, this );
			panel.Show();
		}

		protected class ColorPanel : System.Windows.Forms.Form
		{
			private ColorButton colorButton;
			private int colorIndex = -1;
			private int keyboardIndex = -50;

			private Color[] colorList = new Color[40]
			{
				Color.FromArgb( 0x00, 0x00, 0x00 ), Color.FromArgb( 0x99, 0x33, 0x00 ),
				Color.FromArgb( 0x33, 0x33, 0x00 ), Color.FromArgb( 0x00, 0x33, 0x00 ),
				Color.FromArgb( 0x00, 0x33, 0x66 ), Color.FromArgb( 0x00, 0x00, 0x80 ),
				Color.FromArgb( 0x33, 0x33, 0x99 ), Color.FromArgb( 0x33, 0x33, 0x33 ),

				Color.FromArgb( 0x80, 0x00, 0x00 ), Color.FromArgb( 0xFF, 0x66, 0x00 ),
				Color.FromArgb( 0x80, 0x80, 0x00 ), Color.FromArgb( 0x00, 0x80, 0x00 ),
				Color.FromArgb( 0x00, 0x80, 0x80 ), Color.FromArgb( 0x00, 0x00, 0xFF ),
				Color.FromArgb( 0x66, 0x66, 0x99 ), Color.FromArgb( 0x80, 0x80, 0x80 ),

				Color.FromArgb( 0xFF, 0x00, 0x00 ), Color.FromArgb( 0xFF, 0x99, 0x00 ),
				Color.FromArgb( 0x99, 0xCC, 0x00 ), Color.FromArgb( 0x33, 0x99, 0x66 ),
				Color.FromArgb( 0x33, 0xCC, 0xCC ), Color.FromArgb( 0x33, 0x66, 0xFF ),
				Color.FromArgb( 0x80, 0x00, 0x80 ), Color.FromArgb( 0x99, 0x99, 0x99 ),

				Color.FromArgb( 0xFF, 0x00, 0xFF ), Color.FromArgb( 0xFF, 0xCC, 0x00 ),
				Color.FromArgb( 0xFF, 0xFF, 0x00 ), Color.FromArgb( 0x00, 0xFF, 0x00 ),
				Color.FromArgb( 0x00, 0xFF, 0xFF ), Color.FromArgb( 0x00, 0xCC, 0xFF ),
				Color.FromArgb( 0x99, 0x33, 0x66 ), Color.FromArgb( 0xC0, 0xC0, 0xC0 ),

				Color.FromArgb( 0xFF, 0x99, 0xCC ), Color.FromArgb( 0xFF, 0xCC, 0x99 ),
				Color.FromArgb( 0xFF, 0xFF, 0x99 ), Color.FromArgb( 0xCC, 0xFF, 0xCC ),
				Color.FromArgb( 0xCC, 0xFF, 0xFF ), Color.FromArgb( 0x99, 0xCC, 0xFF ),
				Color.FromArgb( 0xCC, 0x99, 0xFF ), Color.FromArgb( 0xFF, 0xFF, 0xFF )
			};

			public ColorPanel(Point pt, ColorButton button)
			{
				colorButton = button;

				FormBorderStyle = FormBorderStyle.FixedDialog;
				MinimizeBox = false;
				MaximizeBox = false;
				ControlBox = false;
				ShowInTaskbar = false;
				TopMost = true;

				SetStyle(ControlStyles.DoubleBuffer, true);
				SetStyle(ControlStyles.UserPaint, true);
				SetStyle(ControlStyles.AllPaintingInWmPaint, true);

				Width = 156;
				Height = 100;

				if(colorButton.autoButton != "")
					Height += 23;
				if(colorButton.moreButton != "")
					Height += 23;

				CenterToScreen();
				Location = pt;

				Capture = true;
			}

			protected override void OnClosed(EventArgs e)
			{
				base.OnClosed(e);

				colorButton.panelVisible = false;
				colorButton.Refresh();
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				base.OnPaint(e);

				Pen darkPen = new Pen(SystemColors.ControlDark);
				Pen lightPen = new Pen(SystemColors.ControlLightLight);
				SolidBrush lightBrush = new SolidBrush(SystemColors.ControlLightLight);
				bool selected = false;
				int x = 6, y = 5;

				if(colorButton.autoButton != "")
				{
					selected = colorButton.Color == Color.Transparent;
					DrawButton(e, x, y, colorButton.autoButton, 100, selected);
					y += 23;
				}

				for(int i = 0; i < 40; i++)
				{
					if(colorButton.Color.ToArgb() == colorList[i].ToArgb())
						selected = true ;

					if(colorIndex == i)
					{
						e.Graphics.DrawRectangle(lightPen, x - 3, y - 3, 17, 17);
						e.Graphics.DrawLine(darkPen, x - 2, y + 14, x + 14, y + 14);
						e.Graphics.DrawLine(darkPen, x + 14, y - 2, x + 14, y + 14);
					}
					else if(colorButton.Color.ToArgb() == colorList[i].ToArgb())
					{
						if(keyboardIndex == -50)
							keyboardIndex = i;

						e.Graphics.FillRectangle(lightBrush, x - 3, y - 3,  18, 18);
						e.Graphics.DrawLine(darkPen, x - 3,  y - 3, x + 13, y - 3 );
						e.Graphics.DrawLine(darkPen, x - 3,  y - 3, x - 3,  y + 13);
					}

					e.Graphics.FillRectangle(new SolidBrush(colorList[i]), x, y, 11, 11);
					e.Graphics.DrawRectangle(darkPen, x, y, 11, 11);

					if((i + 1) % 8 == 0)
					{
						x = 6;
						y += 18;
					}
					else
						x += 18;
				}

				if(colorButton.moreButton != "")
					DrawButton(e, x, y, colorButton.moreButton, 101, ! selected);
			}

			protected override void OnKeyDown(KeyEventArgs e)
			{
				if(e.KeyCode == Keys.Escape)
					Close();
				else if(e.KeyCode == Keys.Left)
					MoveIndex(-1);
				else if(e.KeyCode == Keys.Up)
					MoveIndex(-8);
				else if(e.KeyCode == Keys.Down)
					MoveIndex(+8);
				else if(e.KeyCode == Keys.Right)
					MoveIndex(+1);
				else if(e.KeyCode == Keys.Enter ||
						e.KeyCode == Keys.Space)
					OnClick(EventArgs.Empty);
				else
					base.OnKeyDown(e);
			}

			private void MoveIndex(int delta)
			{
				int lbound = (colorButton.autoButton != "" ? -8 : 0);
				int ubound = 39 + (colorButton.moreButton != "" ? 8 : 0);
				int d = ubound - lbound + 1;

				if(delta == -1 && keyboardIndex < 0)
					keyboardIndex = ubound;
				else if(delta == 1 && keyboardIndex > 39)
					keyboardIndex = lbound;
				else if(delta == 1 && keyboardIndex < 0)
					keyboardIndex = 0;
				else if(delta == -1 && keyboardIndex > 39)
					keyboardIndex = 39;
				else
					keyboardIndex += delta;

				if(keyboardIndex < lbound)
					keyboardIndex += d;
				if(keyboardIndex > ubound)
					keyboardIndex -= d;

				if(keyboardIndex < 0)
					colorIndex = 100;
				else if(keyboardIndex > 39)
					colorIndex = 101;
				else
					colorIndex = keyboardIndex;

				Refresh();
			}

			protected override void OnMouseDown(MouseEventArgs e)
			{
				if(RectangleToScreen(ClientRectangle).Contains(Cursor.Position))
					base.OnMouseDown(e);
				else
					Close();
			}

			protected override void OnMouseMove(MouseEventArgs e)
			{
				base.OnMouseMove(e) ;

				if(RectangleToScreen(ClientRectangle).Contains(Cursor.Position))
				{
					Point pt = PointToClient(Cursor.Position) ;
					int x = 6, y = 5 ;

					if(colorButton.autoButton != "")
					{
						if(SetColorIndex(new Rectangle( x - 3, y - 3, 143, 22 ), pt, 100))
							return;

						y += 23;
					}

					for(int i = 0; i < 40; i++)
					{
						if( SetColorIndex(new Rectangle(x - 3, y - 3, 17, 17), pt, i))
							return;

						if(( i + 1 ) % 8 == 0)
						{
							x = 6;
							y += 18;
						}
						else
							x += 18;
					}

					if(colorButton.moreButton != "")
					{
						if(SetColorIndex(new Rectangle(x - 3, y - 3, 143, 22), pt, 101))
							return;
					}
				}

				if(colorIndex != -1)
				{
					colorIndex = -1;
					Invalidate();
				}
			}

			protected override void OnClick(EventArgs e)
			{
				if(colorIndex < 0)
					return;

				if(colorIndex < 40)
					colorButton.Color = colorList[colorIndex];
				else if(colorIndex == 100)
					colorButton.Color = Color.Transparent;
				else
				{
					ColorDialog dlg = new ColorDialog();
					dlg.Color = colorButton.Color;
					dlg.FullOpen = true;

					if(dlg.ShowDialog(this) != DialogResult.OK)
					{
						Close();
						return;
					}

					colorButton.Color = dlg.Color;
				}

				Close();
				colorButton.OnChanged(EventArgs.Empty);
			}

			protected void DrawButton(PaintEventArgs e, int x, int y, string text,
									int index, bool selected)
			{
				Pen darkPen = new Pen(SystemColors.ControlDark);
				Pen lightPen = new Pen(SystemColors.ControlLightLight);
				SolidBrush lightBrush = new SolidBrush(SystemColors.ControlLightLight);

				if(colorIndex == index)
				{
					e.Graphics.DrawRectangle(lightPen, x - 3, y - 3, 143, 22);
					e.Graphics.DrawLine(darkPen, x - 2, y + 19, x + 140, y + 19);
					e.Graphics.DrawLine(darkPen, x + 140, y - 2, x + 140, y + 19);
				}
				else if( selected )
				{
					e.Graphics.FillRectangle(lightBrush, x - 3, y - 3,   144, 23);
					e.Graphics.DrawLine(darkPen,  x - 3, y - 3, x + 139, y - 3  );
					e.Graphics.DrawLine(darkPen,  x - 3, y - 3, x - 3,   y + 18 );
				}

				Rectangle rc = new Rectangle(x, y, 137, 16);
				SolidBrush textBrush = new SolidBrush(SystemColors.ControlText);

				StringFormat textFormat = new StringFormat();
				textFormat.Alignment = StringAlignment.Center;
				textFormat.LineAlignment = StringAlignment.Center;

				e.Graphics.DrawRectangle(darkPen, rc);
				e.Graphics.DrawString(text, colorButton.Font, textBrush, rc, textFormat);
			}

			protected bool SetColorIndex(Rectangle rc, Point pt, int index)
			{
				if(rc.Contains(pt))
				{
					if(colorIndex != index)
					{
						colorIndex = index;
						Invalidate();
					}
					return true;
				}
				return false;
			}
		}
	}
}
