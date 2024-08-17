//=================================================================
// common.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2012  FlexRadio Systems
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
//
//=================================================================
// Continual modifications Copyright (C) 2019-2024 Richard Samphire (MW0LGE)
//=================================================================

using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.IO.Ports;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security.Principal;
using System.Globalization;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace Thetis
{
	public static class StringExtensions
	{
        // extend contains to be able to ignore case etc MW0LGE
        public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (toCheck == null)
            {
                throw new ArgumentNullException(nameof(toCheck));
            }

            return source?.IndexOf(toCheck, comp) >= 0;
		}

        public static string Left(this string source, int length)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
            }

            return source.Length > length ? source.Substring(0, length) : source;
        }
        public static string Right(this string source, int length)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
            }

            return length >= source.Length ? source : source.Substring(source.Length - length);
        }
    }
    public static class ControlExtentions
    {
        public static string GetFullName(this Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            if (control.Parent == null) return control.Name;
            return control.Parent.GetFullName() + "." + control.Name;
        }
    }
    //public static class Extensions
    //{
    //    private const double Epsilon = 1e-10;

    //    public static bool IsZero(this double d)
    //    {
    //        return Math.Abs(d) < Epsilon;
    //    }
    //}
    public class Common
	{
		private const bool ENABLE_VERSION_TIMEOUT = false;
		private static DateTime _versionTimeout = new DateTime(2022, 06, 01, 00, 00, 00); // june 1st 2022 00:00:00
		private static bool _bypassTimeout = false;

		public static MessageBoxOptions MB_TOPMOST = (MessageBoxOptions)0x00040000L; //MW0LGE_21g TOPMOST for MessageBox

		#region HiglightControls

		private class HighlightData
		{
			public Color BackgroundColour { get; set; }
			public Color ForegroundColour { get; set; }
			public FlatStyle FlatStyle { get; set; }
			public Image BackgroundImage { get; set; }
		}

		private static Dictionary<string, HighlightData> _hightlightData = new Dictionary<string, HighlightData>();

		public static void HightlightControl(Control c, bool bHighlight, bool bFromFinder = false)
		{
			string sKey = c.GetFullName(); //[2.10.1.0] added because control with same name can be in different forms/containers

			HighlightData hd;
			bool bAdd = false;

            if (!_hightlightData.ContainsKey(sKey))
			{
				hd = new HighlightData();
				hd.BackgroundColour = c.BackColor;
				hd.ForegroundColour = c.ForeColor;
				hd.BackgroundImage = c.BackgroundImage;
				hd.FlatStyle = FlatStyle.Flat;

				_hightlightData.Add(sKey, hd);
				bAdd = true;
			}

			hd = _hightlightData[sKey];

            c.BackColor = bHighlight ? Color.Yellow : hd.BackgroundColour;
            c.ForeColor = bHighlight ? Color.Black : hd.ForegroundColour;
            c.BackgroundImage = bHighlight ? null : hd.BackgroundImage;

			//if (c.GetType() == typeof(NumericUpDownTS))
			//{
			//}
			//else
			if (c.GetType() == typeof(CheckBoxTS))
			{
				CheckBoxTS cb = c as CheckBoxTS;
				if (bAdd) hd.FlatStyle = cb.FlatStyle;
				cb.FlatStyle = bHighlight ? FlatStyle.Flat : hd.FlatStyle;
			}
			//else if (c.GetType() == typeof(TrackBarTS))
			//{
			//}
			//else if (c.GetType() == typeof(PrettyTrackBar))
			//{
			//}
			else if (c.GetType() == typeof(ComboBoxTS))
			{
				ComboBoxTS cb = c as ComboBoxTS;
                if (bAdd) hd.FlatStyle = cb.FlatStyle;
                cb.FlatStyle = bHighlight ? FlatStyle.Flat : hd.FlatStyle;
            }
			else if (c.GetType() == typeof(RadioButtonTS))
			{
				RadioButtonTS cb = c as RadioButtonTS;
                if (bAdd) hd.FlatStyle = cb.FlatStyle;
                cb.FlatStyle = bHighlight ? FlatStyle.Flat : hd.FlatStyle;
            }
			//else if (c.GetType() == typeof(TextBoxTS))
			//{
			//}
			//else if (c.GetType() == typeof(LabelTS))
			//{
			//}

			if (!bHighlight)
			{
				if(_hightlightData.ContainsKey(sKey))
				{
					_hightlightData.Remove(sKey);
				}
			}

			c.Invalidate();
		}
		#endregion
		#region WindowDropShadow
		private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
		[DllImport("dwmapi.dll")]
		private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);
		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}
		
		public static Size DropShadowSize(Form f)
		{
			// this only works on a visibile form
			if (!f.Visible) return new Size(0, 0);

			Size sz;
			RECT rectWithShadow;
			if (Environment.OSVersion.Version.Major < 6)
			{
				sz = new Size(0, 0);
			}
			else if (DwmGetWindowAttribute(f.Handle, DWMWA_EXTENDED_FRAME_BOUNDS, out rectWithShadow, Marshal.SizeOf(typeof(RECT))) == 0)
			{
				sz = new Size(f.Width - (rectWithShadow.right - rectWithShadow.left), f.Height - (rectWithShadow.bottom - rectWithShadow.top));
			}
			else
			{
				sz = new Size(0, 0);
			}

			return sz;
		}
		#endregion

		public static void ControlList(Control c, ref ArrayList a)
		{
			if(c.Controls.Count > 0)
			{
                foreach (Control c2 in c.Controls)
                {
                    ControlList(c2, ref a);
                }
			}

			if(c.GetType() == typeof(CheckBoxTS) || c.GetType() == typeof(CheckBoxTS) ||
				c.GetType() == typeof(ComboBoxTS) || c.GetType() == typeof(ComboBox) ||
				c.GetType() == typeof(NumericUpDownTS) || c.GetType() == typeof(NumericUpDown) ||
				c.GetType() == typeof(RadioButtonTS) || c.GetType() == typeof(RadioButton) ||
				c.GetType() == typeof(TextBoxTS) || c.GetType() == typeof(TextBox) ||
				c.GetType() == typeof(TrackBarTS) || c.GetType() == typeof(TrackBar) ||
				c.GetType() == typeof(ColorButton))
				a.Add(c);

		}
        
        public static void SaveForm(Form form, string tablename)
		{
            if (DB.ds == null) return;

			ArrayList a = new ArrayList();
			ArrayList temp = new ArrayList();

			ControlList(form, ref temp);

			foreach(Control c in temp)				// For each control
			{
				if(c.GetType() == typeof(CheckBoxTS))
					a.Add(c.Name+"/"+((CheckBoxTS)c).Checked.ToString());
				else if(c.GetType() == typeof(ComboBoxTS))
				{
					//if(((ComboBox)c).SelectedIndex >= 0)
					a.Add(c.Name+"/"+((ComboBoxTS)c).Text);
				}
				else if(c.GetType() == typeof(NumericUpDownTS))
					a.Add(c.Name+"/"+((NumericUpDownTS)c).Value.ToString());
				else if(c.GetType() == typeof(RadioButtonTS))
					a.Add(c.Name+"/"+((RadioButtonTS)c).Checked.ToString());
				else if(c.GetType() == typeof(TextBoxTS))
					a.Add(c.Name+"/"+((TextBoxTS)c).Text);
				else if(c.GetType() == typeof(TrackBarTS))
					a.Add(c.Name+"/"+((TrackBarTS)c).Value.ToString());
				else if(c.GetType() == typeof(ColorButton))
				{
					Color clr = ((ColorButton)c).Color;
					a.Add(c.Name+"/"+clr.R+"."+clr.G+"."+clr.B+"."+clr.A);
				}
#if(DEBUG)
				else if(c.GetType() == typeof(GroupBox) ||
					c.GetType() == typeof(CheckBoxTS) ||
					c.GetType() == typeof(ComboBox) ||
					c.GetType() == typeof(NumericUpDown) ||
					c.GetType() == typeof(RadioButton) ||
					c.GetType() == typeof(TextBox) ||
					c.GetType() == typeof(TrackBar))
					Debug.WriteLine(form.Name + " -> " + c.Name+" needs to be converted to a Thread Safe control.");
#endif
			}
			a.Add("Top/"+form.Top);
			a.Add("Left/"+form.Left);
			a.Add("Width/"+form.Width);
			a.Add("Height/"+form.Height);

			DB.SaveVars(tablename, ref a);		// save the values to the DB
		}

		public static void RestoreForm(Form form, string tablename, bool restore_size)
		{
            if (DB.ds == null) return;

            ArrayList temp = new ArrayList();		// list of all first level controls
			ControlList(form, ref temp);

			//ArrayList checkbox_list = new ArrayList();
			//ArrayList combobox_list = new ArrayList();
			//ArrayList numericupdown_list = new ArrayList();
			//ArrayList radiobutton_list = new ArrayList();
			//ArrayList textbox_list = new ArrayList();
			//ArrayList trackbar_list = new ArrayList();
			//ArrayList colorbutton_list = new ArrayList();

			//[2.10.2.3]MW0LGE change to single dictionary of controls
			Dictionary<string, Control> ctrls = new Dictionary<string, Control>();

			//ArrayList controls = new ArrayList();	// list of controls to restore
			foreach(Control c in temp)
			{
                //if(c.GetType() == typeof(CheckBoxTS))			// the control is a CheckBoxTS
                //	checkbox_list.Add(c);
                //else if(c.GetType() == typeof(ComboBoxTS))		// the control is a ComboBox
                //	combobox_list.Add(c);
                //else if(c.GetType() == typeof(NumericUpDownTS))	// the control is a NumericUpDown
                //	numericupdown_list.Add(c);
                //else if(c.GetType() == typeof(RadioButtonTS))	// the control is a RadioButton
                //	radiobutton_list.Add(c);
                //else if(c.GetType() == typeof(TextBoxTS))		// the control is a TextBox
                //	textbox_list.Add(c);
                //else if(c.GetType() == typeof(TrackBarTS))		// the control is a TrackBar (slider)
                //	trackbar_list.Add(c);
                //else if(c.GetType() == typeof(ColorButton))
                //	colorbutton_list.Add(c);

                ctrls.Add(c.Name, c); //[2.10.2.3]MW0LGE yes, control names are unique per form, and to create and search each list is madness
            }
			temp.Clear();	// now that we have the controls we want, delete first list 

			ArrayList a = DB.GetVars(tablename);						// Get the saved list of controls
			a.Sort();
			
			// restore saved values to the controls
			foreach(string s in a)				// string is in the format "name,value"
			{
				string[] vals = s.Split('/');
				if(vals.Length > 2)
				{
					for(int i=2; i<vals.Length; i++)
						vals[1] += "/"+vals[i];
				}

				string name = vals[0];
				string val = vals[1];

				switch(name)
				{
					case "Top":
						form.StartPosition = FormStartPosition.Manual;
						int top = int.Parse(val);
						/*if(top < 0) top = 0;
						if(top > Screen.PrimaryScreen.Bounds.Height-form.Height && Screen.AllScreens.Length == 1)
							top = Screen.PrimaryScreen.Bounds.Height-form.Height;*/
						form.Top = top;
						break;
					case "Left":
						form.StartPosition = FormStartPosition.Manual;
						int left = int.Parse(val);
						/*if(left < 0) left = 0;
						if(left > Screen.PrimaryScreen.Bounds.Width-form.Width && Screen.AllScreens.Length == 1)
							left = Screen.PrimaryScreen.Bounds.Width-form.Width;*/
						form.Left = left;
						break;
					case "Width":
						if(restore_size)
						{
							int width = int.Parse(val);
							/*if(width + form.Left > Screen.PrimaryScreen.Bounds.Width && Screen.AllScreens.Length == 1)
								form.Left -= (width+form.Left-Screen.PrimaryScreen.Bounds.Width);*/
							form.Width = width;
						}
						break;
					case "Height":
						if(restore_size)
						{
							int height = int.Parse(val);
							/*if(height + form.Top > Screen.PrimaryScreen.Bounds.Height && Screen.AllScreens.Length == 1)
								form.Top -= (height+form.Top-Screen.PrimaryScreen.Bounds.Height);*/
							form.Height = height;
						}
						break;
				}

				if(s.StartsWith("chk"))			// control is a CheckBoxTS
				{
					//for(int i=0; i<checkbox_list.Count; i++)
					//{	// look through each control to find the matching name
					//	CheckBoxTS c = (CheckBoxTS)checkbox_list[i];
					//	if(c.Name.Equals(name))		// name found
					//	{
					//		c.Checked = bool.Parse(val);	// restore value
					//		i = checkbox_list.Count+1;
					//	}
					//	if(i == checkbox_list.Count)
					//		MessageBox.Show("Control not found: "+name);
					//}
					if (ctrls.ContainsKey(name)) ((CheckBoxTS)ctrls[name]).Checked = bool.Parse(val);
                }
				else if(s.StartsWith("combo"))	// control is a ComboBox
				{
					//for(int i=0; i<combobox_list.Count; i++)
					//{	// look through each control to find the matching name
					//	ComboBoxTS c = (ComboBoxTS)combobox_list[i];
					//	if(c.Name.Equals(name))		// name found
					//	{
					//		c.Text = val;	// restore value
					//		i = combobox_list.Count+1;
					//		if(c.Text != val) Debug.WriteLine("Warning: "+form.Name+"."+name+" did not set to "+val);
					//	}
					//	if(i == combobox_list.Count)
					//		MessageBox.Show("Control not found: "+name);
					//}
					if (ctrls.ContainsKey(name)) ((ComboBoxTS)ctrls[name]).Text = val;
                }
				else if(s.StartsWith("ud"))
				{
                    //for(int i=0; i<numericupdown_list.Count; i++)
                    //{	// look through each control to find the matching name
                    //	NumericUpDownTS c = (NumericUpDownTS)numericupdown_list[i];
                    //	if(c.Name.Equals(name))		// name found
                    //	{
                    //		decimal num = decimal.Parse(val);

                    //		if(num > c.Maximum) num = c.Maximum;		// check endpoints
                    //		else if(num < c.Minimum) num = c.Minimum;
                    //		c.Value = num;			// restore value
                    //		i = numericupdown_list.Count+1;
                    //	}
                    //	if(i == numericupdown_list.Count)
                    //		MessageBox.Show("Control not found: "+name);	
                    //}
                    if (ctrls.ContainsKey(name))
                    {
                        NumericUpDownTS c = (NumericUpDownTS)ctrls[name];
                        decimal dnum = decimal.Parse(val);
                        if (dnum > c.Maximum) dnum = c.Maximum;
                        else if (dnum < c.Minimum) dnum = c.Minimum;
                        c.Value = dnum;
                    }
                }
				else if(s.StartsWith("rad"))
				{   // look through each control to find the matching name
                    //for(int i=0; i<radiobutton_list.Count; i++)
                    //{
                    //	RadioButtonTS c = (RadioButtonTS)radiobutton_list[i];
                    //	if(c.Name.Equals(name))		// name found
                    //	{
                    //		if(!val.ToLower().Equals("true") && !val.ToLower().Equals("false"))
                    //			val = "True";
                    //		c.Checked = bool.Parse(val);	// restore value
                    //		i = radiobutton_list.Count+1;
                    //	}
                    //	if(i == radiobutton_list.Count)
                    //		MessageBox.Show("Control not found: "+name);
                    //}
                    if (ctrls.ContainsKey(name))
                    {
                        RadioButtonTS c = (RadioButtonTS)ctrls[name];
                        if (!val.ToLower().Equals("true") && !val.ToLower().Equals("false")) val = "True";
                        c.Checked = bool.Parse(val);
                    }
                }
				else if(s.StartsWith("txt"))
				{   // look through each control to find the matching name
                    //for(int i=0; i<textbox_list.Count; i++)
                    //{
                    //	TextBoxTS c = (TextBoxTS)textbox_list[i];
                    //	if(c.Name.Equals(name))		// name found
                    //	{
                    //		c.Text = val;	// restore value
                    //		i = textbox_list.Count+1;
                    //	}
                    //	if(i == textbox_list.Count)
                    //		MessageBox.Show("Control not found: "+name);
                    //}
                    if (ctrls.ContainsKey(name)) ((TextBoxTS)ctrls[name]).Text = val;
                }
				else if(s.StartsWith("tb"))
				{
					//// look through each control to find the matching name
					//for(int i=0; i<trackbar_list.Count; i++)
					//{
					//	TrackBarTS c = (TrackBarTS)trackbar_list[i];
					//	if(c.Name.Equals(name))		// name found
					//	{
					//		int num = int.Parse(val);
					//		if(num > c.Maximum) num = c.Maximum;
					//		if(num < c.Minimum) num = c.Minimum;
					//		c.Value = num;
					//		i = trackbar_list.Count+1;
					//	}
					//	if(i == trackbar_list.Count)
					//		MessageBox.Show("Control not found: "+name);
					//}
					if (ctrls.ContainsKey(name))
					{
						TrackBarTS c = (TrackBarTS)ctrls[name];
						int num = int.Parse(val);
						if (num > c.Maximum) num = c.Maximum;
						if (num < c.Minimum) num = c.Minimum;
                        c.Value = num;
                    }
                }
				else if(s.StartsWith("clrbtn"))
				{
					//string[] colors = val.Split('.');
					//if(colors.Length == 4)
					//{
					//	int R,G,B,A;
					//	R = Int32.Parse(colors[0]);
					//	G = Int32.Parse(colors[1]);
					//	B = Int32.Parse(colors[2]);
					//	A = Int32.Parse(colors[3]);
					//	for(int i=0; i<colorbutton_list.Count; i++)
					//	{
					//		ColorButton c = (ColorButton)colorbutton_list[i];
					//		if(c.Name.Equals(name))		// name found
					//		{
					//			c.Color = Color.FromArgb(A, R, G, B);
					//			i = colorbutton_list.Count+1;
					//		}
					//		if(i == colorbutton_list.Count)
					//			MessageBox.Show("Control not found: "+name);
					//	}
					//}
					if (ctrls.ContainsKey(name))
					{
                        string[] colors = val.Split('.');
						if (colors.Length == 4)
						{
							int R, G, B, A;
							R = Int32.Parse(colors[0]);
							G = Int32.Parse(colors[1]);
							B = Int32.Parse(colors[2]);
							A = Int32.Parse(colors[3]);
							ColorButton c = (ColorButton)ctrls[name];
                            c.Color = Color.FromArgb(A, R, G, B);
                        }
					}
				}
			}

			ForceFormOnScreen(form);
		}

		public static void ForceFormOnScreen(Form f)
		{
            Screen[] screens = Screen.AllScreens;

            if (screens.Length == 0)
            {
                f.Location = new Point(0, 0);
                return;
            }

            int left = int.MaxValue, top = int.MaxValue;
            int right = int.MinValue, bottom = int.MinValue;

            foreach (Screen screen in screens)
            {
                if (screen.Bounds.Left < left)
                    left = screen.Bounds.Left;
                if (screen.Bounds.Top < top)
                    top = screen.Bounds.Top;
                if (screen.Bounds.Right > right)
                    right = screen.Bounds.Right;
                if (screen.Bounds.Bottom > bottom)
                    bottom = screen.Bounds.Bottom;
            }

            bool onScreen = f.Left >= left &&
                            f.Top >= top &&
                            f.Right <= right &&
                            f.Bottom <= bottom;

            if (!onScreen)
            {
                if (f.Left < left)
                    f.Left = left;
                if (f.Top < top)
                    f.Top = top;
                if (f.Right > right)
                    f.Left = right - f.Width;
                if (f.Bottom > bottom)
                    f.Top = bottom - f.Height;
            }

            //         Screen[] screens = Screen.AllScreens;
            //bool on_screen = false;

            //int left = 0, right = 0, top = 0, bottom = 0;

            //for(int i=0; i<screens.Length; i++)
            //{
            //	if(screens[i].Bounds.Left < left)
            //		left = screens[i].Bounds.Left;

            //	if(screens[i].Bounds.Top < top)
            //		top = screens[i].Bounds.Top;

            //	if(screens[i].Bounds.Bottom > bottom)
            //		bottom = screens[i].Bounds.Bottom;

            //	if(screens[i].Bounds.Right > right)
            //		right = screens[i].Bounds.Right;
            //}

            ////MW0LGE_21d >= and <= all over
            //if(f.Left >= left &&
            //	f.Top >= top &&
            //	f.Right <= right &&
            //	f.Bottom <= bottom)
            //         	on_screen = true;				

            //if(!on_screen)
            //{
            //	//f.Location = new Point(0, 0);

            //	if(f.Left < left)
            //		f.Left = left;

            //	if(f.Top < top)
            //		f.Top = top;

            //	if(f.Bottom > bottom)
            //	{
            //		if((f.Top - (f.Bottom-bottom)) >= top)
            //			f.Top -= (f.Bottom-bottom);
            //		else f.Top = 0;
            //	}

            //	if(f.Right > right)
            //	{
            //		if((f.Left - (f.Right-right)) >= left)
            //			f.Left -= (f.Right-right);
            //		else f.Left = 0;
            //	}
            //}
        }

		public static void TabControlInsert(TabControl tc, TabPage tp, int index)
		{
			tc.SuspendLayout();
			// temp storage to rearrange tabs
			TabPage[] temp = new TabPage[tc.TabPages.Count+1];

			// copy pages in order and insert new page when needed
			for(int i=0; i<tc.TabPages.Count+1; i++)
			{
				if(i < index) temp[i] = tc.TabPages[i];
				else if(i == index) temp[i] = tp;
				else if(i > index) temp[i] = tc.TabPages[i-1];
			}
			
			// erase all tab pages
			while(tc.TabPages.Count > 0)
				tc.TabPages.RemoveAt(0);

			// add them back with new page inserted
			for(int i=0; i<temp.Length; i++)
				tc.TabPages.Add(temp[i]);

			tc.ResumeLayout();
		}

        public static string[] SortedComPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Array.Sort<string>(ports, delegate(string strA, string strB)
            {
                try
                {
                    int idA = int.Parse(strA.Substring(3));
                    int idB = int.Parse(strB.Substring(3));

                    return idA.CompareTo(idB);
                }
                catch (Exception)
                {
                    return strA.CompareTo(strB);
                }
            });
            return ports;
        }

        public static string RevToString(uint rev)
        {
            return ((byte)(rev >> 24)).ToString() + "." +
                ((byte)(rev >> 16)).ToString() + "." +
                ((byte)(rev >> 8)).ToString() + "." +
                ((byte)(rev >> 0)).ToString();
        }

        private static string m_sLogPath = "";
        public static void SetLogPath(string sPath)
        {
            m_sLogPath = sPath;
        }
        public static void LogString(string entry)
        {
            // MW0LGE very simple logger
            if (m_sLogPath == "") return;
            if (entry == "") return;

            try
            {
                using (StreamWriter w = File.AppendText(m_sLogPath + "\\ErrorLog.txt"))
                {
                    //using block will auto close stream
                    w.Write("\r\nEntry : ");
                    w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                    w.WriteLine(entry);
                    w.WriteLine("-------------------------------");
                }
            }
            catch
            {

            }
        }
        public static void LogException(Exception e)
        {
            // MW0LGE very simple logger
            if (m_sLogPath == "") return;
            if (e == null) return;

            try
            {
                using (StreamWriter w = File.AppendText(m_sLogPath + "\\ErrorLog.txt"))
                {
                    //using block will auto close stream
                    w.Write("\r\nEntry : ");
                    w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                    w.WriteLine(e.Message);
                    if (e.StackTrace != "")
                    {
#if DEBUG
                        StackTrace st = new StackTrace(e, true);
                        StackFrame sf = st.GetFrames().Last();
                        w.WriteLine("File : " + sf.GetFileName() + " ... line : " + sf.GetFileLineNumber().ToString());
#endif
                        w.WriteLine("---------stacktrace------------");
                        w.WriteLine(e.StackTrace);
                    }
                    w.WriteLine("-------------------------------");
                }
            }
            catch
            {

            }
        }

		// returns the Thetis version number in "a.b.c" format
		// MW0LGE moved here from titlebar.cs, and used by console.cs and others
		private static string m_sVersionNumber = "";
		private static string m_sFileVersion = "";
		private static string m_sRevision = "";
		public static string GetVerNum()
		{
			if (m_sVersionNumber != "") return m_sVersionNumber;

			setupVersions();

			return m_sVersionNumber;
		}
		public static string GetFileVersion()
		{
			if (m_sFileVersion != "") return m_sFileVersion;

			setupVersions();

			return m_sFileVersion;
		}
		public static string GetRevision()
		{
			if (m_sRevision != "") return m_sRevision;

			setupVersions();

			return m_sRevision;
		}
		private static void setupVersions()
		{
			//MW0LGE build version number string once and return that
			// if called again. Issue reported by NJ2US where assembly.Location
			// passed into GetVersionInfo failed. Perhaps because norton or something
			// moved the file after it was accessed. The version isn't going to
			// change anyway while running, so obtaining it once is fine.
			if (m_sVersionNumber!="" && m_sFileVersion!="") return; // already setup

			Assembly assembly = Assembly.GetExecutingAssembly();
			FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
			m_sVersionNumber = fvi.FileVersion.Substring(0, fvi.FileVersion.LastIndexOf("."));
			m_sFileVersion = fvi.FileVersion;
			m_sRevision = fvi.FileVersion.Substring(fvi.FileVersion.LastIndexOf(".") + 1);
		}

		public static int DaysToTimeOut()
		{
			int days = (int)(_versionTimeout - DateTime.Now).TotalDays;
			if (days < 0) days = 0;
			return days;
		}
		public static bool IsVersionTimedOut
		{
			get
			{
				if (ENABLE_VERSION_TIMEOUT && !_bypassTimeout)
					return DaysToTimeOut() == 0;
				else
					return false;
			}
		}
		public static bool IsTimeOutEnabled
		{
			get { return ENABLE_VERSION_TIMEOUT && !_bypassTimeout; }
		}
		public static bool BypassTimeOut
        {
            set { _bypassTimeout = value; }
        }
		public static bool IsAdministrator()
		{
			using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
			{
				WindowsPrincipal principal = new WindowsPrincipal(identity);
				return principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
		}

		public static bool ShiftKeyDown
		{
			get
			{
				return Keyboard.IsKeyDown(Keys.LShiftKey) || Keyboard.IsKeyDown(Keys.RShiftKey);
			}
		}
		public static bool CtrlKeyDown
		{
			get
			{
				return Keyboard.IsKeyDown(Keys.LControlKey) || Keyboard.IsKeyDown(Keys.RControlKey);
			}
		}
		public static bool Is64Bit
        {
            get
            {
				return System.IntPtr.Size == 8 ? true : false;
			}
        }
		public static void DoubleBuffered(Control c, bool bEnabled)
        {
			// MW0LGE_[2.9.0.6]
			// not all controls (such as panels) have double buffered property
			// try to use reflection, so we can keep the base panel
			try
			{
				c.GetType().InvokeMember("DoubleBuffered",
								System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
								null, c, new object[] { bEnabled });
			}
			catch 
			{ 
			}
		}

		public static int FiveDigitHash(string str)
		{
			if(str == "") return 0;

            // Jenkins one_at_a_time hash function
            // https://en.wikipedia.org/wiki/Jenkins_hash_function
            uint hash = 0;
            foreach (byte b in System.Text.Encoding.Unicode.GetBytes(str))
            {
                hash += b;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);

            return (int)(hash % 99999);
        }
        public static string ColourToString(System.Drawing.Color c)
        {
            return c.A.ToString() + "." + c.R.ToString() + "." + c.G.ToString() + "." + c.B.ToString();
        }
        public static System.Drawing.Color ColourFromString(string str)
        {
			//format of string "A.R.G.B"
            string[] tmp = str.Split('.');
            if (tmp.Length == 4)
            {
                bool bOk;
                int A = 0, R = 0, G = 0, B = 0;

				bOk = int.TryParse(tmp[0], out A);
				if (bOk) bOk = int.TryParse(tmp[1], out R);
                if (bOk) bOk = int.TryParse(tmp[2], out G);
                if (bOk) bOk = int.TryParse(tmp[3], out B);

				if (bOk) return System.Drawing.Color.FromArgb(A, R, G, B);
            }
            return System.Drawing.Color.Empty;
        }

        public static double UVfromDBM(double dbm)
        {
            //return uV (rms) from dBm (50 ohms)
            return Math.Sqrt(Math.Pow(10, dbm / 10) * 50 * 1e-3) * 1e6;
        }
		public static string SMeterFromDBM(double dbm, bool bAboveS9Frequency)
        {
            string sRet;

            if (bAboveS9Frequency)
            {
                if (dbm <= -144.0f) sRet = "S 0";
                else if (dbm > -144.0f & dbm <= -138.0f) sRet = "S 1";
                else if (dbm > -138.0f & dbm <= -132.0f) sRet = "S 2";
                else if (dbm > -132.0f & dbm <= -126.0f) sRet = "S 3";
                else if (dbm > -126.0f & dbm <= -120.0f) sRet = "S 4";
                else if (dbm > -120.0f & dbm <= -114.0f) sRet = "S 5";
                else if (dbm > -114.0f & dbm <= -108.0f) sRet = "S 6";
                else if (dbm > -108.0f & dbm <= -102.0f) sRet = "S 7";
                else if (dbm > -102.0f & dbm <= -96.0f) sRet = "S 8";
                else if (dbm > -96.0f & dbm <= -90.0f) sRet = "S 9";
                else if (dbm > -90.0f & dbm <= -86.0f) sRet = "S 9 + 5";
                else if (dbm > -86.0f & dbm <= -80.0f) sRet = "S 9 + 10";
                else if (dbm > -80.0f & dbm <= -76.0f) sRet = "S 9 + 15";
                else if (dbm > -76.0f & dbm <= -66.0f) sRet = "S 9 + 20";
                else if (dbm > -66.0f & dbm <= -56.0f) sRet = "S 9 + 30";
                else if (dbm > -56.0f & dbm <= -46.0f) sRet = "S 9 + 40";
                else if (dbm > -46.0f & dbm <= -36.0f) sRet = "S 9 + 50";
                else sRet = "S 9 + 60";
            }
            else
            {
                if (dbm <= -124.0f) sRet = "S 0";
                else if (dbm > -124.0f & dbm <= -118.0f) sRet = "S 1";
                else if (dbm > -118.0f & dbm <= -112.0f) sRet = "S 2";
                else if (dbm > -112.0f & dbm <= -106.0f) sRet = "S 3";
                else if (dbm > -106.0f & dbm <= -100.0f) sRet = "S 4";
                else if (dbm > -100.0f & dbm <= -94.0f) sRet = "S 5";
                else if (dbm > -94.0f & dbm <= -88.0f) sRet = "S 6";
                else if (dbm > -88.0f & dbm <= -82.0f) sRet = "S 7";
                else if (dbm > -82.0f & dbm <= -76.0f) sRet = "S 8";
                else if (dbm > -76.0f & dbm <= -70.0f) sRet = "S 9";
                else if (dbm > -70.0f & dbm <= -66.0f) sRet = "S 9 + 5";
                else if (dbm > -66.0f & dbm <= -60.0f) sRet = "S 9 + 10";
                else if (dbm > -60.0f & dbm <= -56.0f) sRet = "S 9 + 15";
                else if (dbm > -56.0f & dbm <= -46.0f) sRet = "S 9 + 20";
                else if (dbm > -46.0f & dbm <= -36.0f) sRet = "S 9 + 30";
                else if (dbm > -36.0f & dbm <= -26.0f) sRet = "S 9 + 40";
                else if (dbm > -26.0f & dbm <= -16.0f) sRet = "S 9 + 50";
                else sRet = "S 9 + 60";
            }
            return "    " + sRet;
        }
        public static double GetSMeterUnits(double dbm, bool bAboveS9Frequency)
        {
            if (bAboveS9Frequency)
                return 9 + ((dbm + 93) / 6f); //MW0LGE_[2.9.0.7] fixed to 93
            else
                return 9 + ((dbm + 73) / 6f);
        }
        public static void SMeterFromDBM2(double dbm, bool bAboveS9Frequency, out int S, out int over9dBm)
        {
			// version that returns via out parameters the S reading, and the dbm over reading

            if (bAboveS9Frequency)
            {
				if (dbm <= -144.0f) { S = 0; over9dBm = 0; }
                else if (dbm > -144.0f & dbm <= -138.0f) { S = 1; over9dBm = 0; }
				else if (dbm > -138.0f & dbm <= -132.0f) { S = 2; over9dBm = 0; }
                else if (dbm > -132.0f & dbm <= -126.0f) { S = 3; over9dBm = 0; }
                else if (dbm > -126.0f & dbm <= -120.0f) { S = 4; over9dBm = 0; }
                else if (dbm > -120.0f & dbm <= -114.0f) { S = 5; over9dBm = 0; }
                else if (dbm > -114.0f & dbm <= -108.0f) { S = 6; over9dBm = 0; }
                else if (dbm > -108.0f & dbm <= -102.0f) { S = 7; over9dBm = 0; }
                else if (dbm > -102.0f & dbm <= -96.0f) { S = 8; over9dBm = 0; }
                else if (dbm > -96.0f & dbm <= -90.0f) { S = 9; over9dBm = 0; }
                else if (dbm > -90.0f & dbm <= -86.0f) { S = 9; over9dBm = 5; }
                else if (dbm > -86.0f & dbm <= -80.0f) { S = 9; over9dBm = 10; }
                else if (dbm > -80.0f & dbm <= -76.0f) { S = 9; over9dBm = 15; }
                else if (dbm > -76.0f & dbm <= -66.0f) { S = 9; over9dBm = 20; }
                else if (dbm > -66.0f & dbm <= -56.0f) { S = 9; over9dBm = 30; }
                else if (dbm > -56.0f & dbm <= -46.0f) { S = 9; over9dBm = 40; }
                else if (dbm > -46.0f & dbm <= -36.0f) { S = 9; over9dBm = 50; }
                else { S = 9; over9dBm = 60; }
            }
            else
            {
                if (dbm <= -124.0f) { S = 0; over9dBm = 0; }
                else if (dbm > -124.0f & dbm <= -118.0f) { S = 1; over9dBm = 0; }
                else if (dbm > -118.0f & dbm <= -112.0f) { S = 2; over9dBm = 0; }
                else if (dbm > -112.0f & dbm <= -106.0f) { S = 3; over9dBm = 0; }
                else if (dbm > -106.0f & dbm <= -100.0f) { S = 4; over9dBm = 0; }
                else if (dbm > -100.0f & dbm <= -94.0f) { S = 5; over9dBm = 0; }
                else if (dbm > -94.0f & dbm <= -88.0f) { S = 6; over9dBm = 0; }
                else if (dbm > -88.0f & dbm <= -82.0f) { S = 7; over9dBm = 0; }
                else if (dbm > -82.0f & dbm <= -76.0f) { S = 8; over9dBm = 0; }
                else if (dbm > -76.0f & dbm <= -70.0f) { S = 9; over9dBm = 0; }
                else if (dbm > -70.0f & dbm <= -66.0f) { S = 9; over9dBm = 5; }
                else if (dbm > -66.0f & dbm <= -60.0f) { S = 9; over9dBm = 10; }
                else if (dbm > -60.0f & dbm <= -56.0f) { S = 9; over9dBm = 15; }
                else if (dbm > -56.0f & dbm <= -46.0f) { S = 9; over9dBm = 20; }
                else if (dbm > -46.0f & dbm <= -36.0f) { S = 9; over9dBm = 30; }
                else if (dbm > -36.0f & dbm <= -26.0f) { S = 9; over9dBm = 40; }
                else if (dbm > -26.0f & dbm <= -16.0f) { S = 9; over9dBm = 50; }
                else { S = 9; over9dBm = 60; }
            }
        }

        #region DarkMode
        //MW0LGE [2.9.0.8]
        //https://stackoverflow.com/questions/57124243/winforms-dark-title-bar-on-windows-10
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                int attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }
        public static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

		public static string DateTimeStringForFile(string cultureName = "")
		{
			CultureInfo ci;

			if (cultureName == "")
                ci = CultureInfo.InstalledUICulture;
			else
                ci = CultureInfo.GetCultureInfo(cultureName); //"en-US"

			DateTime now = DateTime.Now;
            string sDate = now.ToString(ci.DateTimeFormat.ShortDatePattern, ci) + "_" + now.ToString(ci.DateTimeFormat.ShortTimePattern, ci);

            sDate = sDate.Replace("/", "_");
            sDate = sDate.Replace(":", "_");
            sDate = sDate.Replace(".", "_");

            // replace any non valid filename chars with _
            string sRet = string.Join("_", sDate.Split(Path.GetInvalidFileNameChars()));

			return sRet;
        }
        #endregion

        #region WindowFade
        public static async void FadeIn(Form frm, int msTimeToFade = 500, int steps = 20)
        {
			float stepSize = 1 / (float)steps;
			float interval = msTimeToFade / (float)steps;
            while (frm.Opacity < 1.0)
            {
                await Task.Delay((int)interval);
                frm.Opacity += stepSize;
            }
            frm.Opacity = 1;
        }

        public static async void FadeOut(Form frm, int msTimeToFade = 500, int steps = 20)
        {
            float stepSize = 1 / (float)steps;
            float interval = msTimeToFade / (float)steps;
            while (frm.Opacity > 0.0)
            {
                await Task.Delay((int)interval);
                frm.Opacity -= stepSize;
            }
            frm.Opacity = 0;
        }
		#endregion

        public static int CompareVersions(string version1, string version2)
        {
            string[] v1Parts = version1.Split('.').Select(part => tryParseVersionPart(part)).ToArray();
            string[] v2Parts = version2.Split('.').Select(part => tryParseVersionPart(part)).ToArray();

            int maxLength = Math.Max(v1Parts.Length, v2Parts.Length);

            for (int i = 0; i < maxLength; i++)
            {
                int v1Part = (i < v1Parts.Length) ? int.Parse(v1Parts[i]) : 0;
                int v2Part = (i < v2Parts.Length) ? int.Parse(v2Parts[i]) : 0;

                int comparison = v1Part.CompareTo(v2Part);
                if (comparison != 0)
                {
                    return comparison;
                }
            }

            return 0; // Versions are equal
        }

        private static string tryParseVersionPart(string part)
        {
            if (int.TryParse(part, out int result))
            {
                return result.ToString();
            }
            return "-1"; // Invalid version part, treat as lower priority
        }

        public static bool IsValidUri(string uri)
        {
			if (uri == "") return false;

			try
			{
				if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
					return false;
				Uri tmp;
				if (!Uri.TryCreate(uri, UriKind.Absolute, out tmp))
					return false;
				return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
			}
			catch { return false; }
        }

        public static bool OpenUri(string uri)
        {
			try
			{
				if (!IsValidUri(uri))
					return false;

				Task.Run(() => System.Diagnostics.Process.Start(uri));

				return true;
			}
			catch { return false; }
        }

        public static int FindNextPowerOf2(int n)
        {
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return ++n;
        }
        public static int FindPreviousPowerOf2(int n)
        {
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n - (n >> 1);
        }

        public static bool IsIpv4Valid(string ip, int port)
        {
            IPAddress address;
            if (!IPAddress.TryParse(ip, out address))
            {
                return false; // IP address format is not valid
            }
            // Ensure the address is an IPv4 address
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return false; // Not an IPv4 address
            }
            if (port < 1 || port > 65535)
            {
                return false; // Port number is out of range
            }

			// Check if x.x.x.x format, where x can be 0>255
            string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
			if (!Regex.IsMatch(ip, pattern)) return false;

            return true; // IP and port are valid
        }
        public static string SerializeToBase64<T>(T obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(gzipStream, obj);
                }
                byte[] compressedArray = memoryStream.ToArray();
                return Convert.ToBase64String(compressedArray);
            }
        }
        public static T DeserializeFromBase64<T>(string base64String)
        {
            byte[] compressedArray = Convert.FromBase64String(base64String);
            using (MemoryStream memoryStream = new MemoryStream(compressedArray))
            {
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    IFormatter formatter = new BinaryFormatter();
                    return (T)formatter.Deserialize(gzipStream);
                }
            }
        }
        //
        public static bool HasArg(string[] args, string arg)
        {
            if (args == null || args.Length < 1 || string.IsNullOrEmpty(arg)) return false;

            //return args[0].Contains(arg, StringComparison.OrdinalIgnoreCase);
            foreach (string s in args)
            {
                if (s.Contains(arg, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static HPSDRModel StringModelToEnum(string sModel)
        {
            switch (sModel.ToUpper())
            {
                case "HERMES":
                    return HPSDRModel.HERMES;
                case "HERMES LITE":
                    return HPSDRModel.HERMESLITE;
                case "ANAN-10":
                    return HPSDRModel.ANAN10;
                case "ANAN-10E":
                    return HPSDRModel.ANAN10E;
                case "ANAN-100":
                    return HPSDRModel.ANAN100;
                case "ANAN-100B":
                    return HPSDRModel.ANAN100B;
                case "ANAN-100D":
                    return HPSDRModel.ANAN100D;
                case "ANAN-200D":
                    return HPSDRModel.ANAN200D;
                case "ANAN-7000DLE":
                    return HPSDRModel.ANAN7000D;
                case "ANAN-8000DLE":
                    return HPSDRModel.ANAN8000D;
                case "ANAN-G2":
                    return HPSDRModel.ANAN_G2;
                case "ANAN-G2-1K":
                    return HPSDRModel.ANAN_G2_1K;
            }

            return HPSDRModel.FIRST;
        }
    }
}