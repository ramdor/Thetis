//=================================================================
// Skin.cs
//=================================================================
// Provides a way to easily save and restore appearance of common
// .NET controls to xml.
// Copyright (C) 2009  FlexRadio Systems
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

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Thetis
{
    /// <summary>
    /// Designed to allow easy saving/restoring of common .NET controls to xml.
    /// </summary>
    public class Skin
    {
        #region Private Variables

        private enum ImageState
        {
            NormalUp,
            NormalDown,
            DisabledUp,
            DisabledDown,
            FocusedUp,
            FocusedDown,
            MouseOverUp,
            MouseOverDown,
        }

        private static string name;
        private static string path;
        private const string pic_file_ext = ".png";

        private static Console m_objConsole;

        private static string app_data_path = "";
        public static string AppDataPath
        {
            set { app_data_path = value; }
        }

        #endregion

        #region Main
        public static void SetConsole(Console objConsole)
        {
            m_objConsole = objConsole;
        }

        /// <summary>
        /// Saves a forms appearance including properties of the form and its controls to xml.
        /// </summary>
        /// <param name="name">name of file to be saved</param>
        /// <param name="path">path to save file</param>
        /// <param name="f">Form to save</param>
        public static void Save(string name, string p, Form f)
        {
            Skin.path = p + "\\" + name;
            Skin.name = name;
            XmlTextWriter writer = new XmlTextWriter(path + "\\" + name + ".xml", Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("Form");

            SaveForm(f, writer);
            
            writer.WriteStartElement("Controls");

            foreach (Control c in f.Controls)
                Save(c, writer);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();    
        }

        /// <summary>
        /// Restores a forms appearance including properties of the form and its controls from xml.
        /// </summary>
        /// <param name="name">name of file to be used</param>
        /// <param name="path">path to file</param>
        /// <param name="f">Form to restore</param>
        /// <returns></returns>
        public static bool Restore(string name, string p, Form f)
        {
            path = p + "\\" + name;
            Skin.name = name;

           // f.BackgroundImage = File.Exists(path + "\\" + f.Name + "\\" + f.Name + pic_file_ext) ? /*Image.FromFile*/loadImage(path + "\\" + f.Name + "\\" + f.Name + pic_file_ext) : null;

            if (File.Exists(path + "\\" + f.Name + "\\" + f.Name + pic_file_ext))
            {
                f.BackgroundImage = /*Image.FromFile*/loadImage(path + "\\" + f.Name + "\\" + f.Name + pic_file_ext);
            }
            else if (File.Exists(path + "\\" + "Console" + "\\" + "Console" + pic_file_ext))
            {
                f.BackgroundImage = /*Image.FromFile*/loadImage(path + "\\" + "Console" + "\\" + "Console" + pic_file_ext);
            }
            else f.BackgroundImage = null;

            foreach (Control c in f.Controls) // load in images
                ReadImages(c);

            if (!File.Exists(path + "\\" + name + ".xml"))
            {
                /*TextWriter writer = new StreamWriter(app_data_path + "\\xml_error.log", true);
               writer.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " +
                   "File not found ("+path + ".xml.\n");
               writer.Close();
               MessageBox.Show("Error reading skin.  File not found.",
                   "Skin file error",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error);*/
                return true;
            }

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(path + "\\" + name + ".xml");
            }
            catch (Exception ex)
            {
                TextWriter writer = new StreamWriter(app_data_path + "\\xml_error.log", true);
                writer.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " " +
                    ex.Message + "\n\n" + ex.StackTrace + "\n");
                writer.Close();
                MessageBox.Show("Error reading Skin file.\n\n" + ex.Message + "\n\n" + ex.StackTrace,
                    "Skin file error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            XmlNodeList matches = doc.GetElementsByTagName("Form");

            RestoreForm(f, doc);

            foreach (Control c in f.Controls)
                Restore(c, doc);

            return true;
        }

        private static void Save(Control c, XmlTextWriter writer)
        {
            Control temp = c as Button;
            if (temp != null)
            {
                SaveButton((Button)c, writer);
                return;
            }

            temp = c as CheckBox;
            if (temp != null)
            {
                SaveCheckBox((CheckBox)c, writer);
                return;
            }

            temp = c as ComboBox;
            if (temp != null)
            {
                SaveComboBox((ComboBox)c, writer);
                return;
            }

            temp = c as Label;
            if (temp != null)
            {
                SaveLabel((Label)c, writer);
                return;
            }

            temp = c as NumericUpDown;
            if (temp != null)
            {
                SaveNumericUpDown((NumericUpDown)c, writer);
                return;
            }

            temp = c as PrettyTrackBar;
            if (temp != null)
            {
                SavePrettyTrackBar((PrettyTrackBar)c, writer);
                return;
            }

            temp = c as PictureBox;
            if (temp != null)
            {
                SavePictureBox((PictureBox)c, writer);
                return;
            }

            temp = c as RadioButton;
            if (temp != null)
            {
                SaveRadioButton((RadioButton)c, writer);
                return;
            }

            temp = c as TextBox;
            if (temp != null)
            {
                SaveTextBox((TextBox)c, writer);
                return;
            }

            temp = c as GroupBox;
            if (temp != null)
            {
                GroupBox grp = (GroupBox)c;
                writer.WriteStartElement(c.Name);
                SaveGroupBox(grp, writer);
                writer.WriteStartElement("Controls");
                foreach (Control c2 in grp.Controls)
                    Save(c2, writer);
                writer.WriteEndElement(); // controls
                writer.WriteEndElement(); // groupbox
                return;
            }

            temp = c as Panel;
            if (temp != null)
            {
                Panel pnl = (Panel)c;
                writer.WriteStartElement(c.Name);
                SavePanel(pnl, writer);
                writer.WriteStartElement("Controls");
                foreach (Control c2 in pnl.Controls)
                    Save(c2, writer);
                writer.WriteEndElement(); // controls
                writer.WriteEndElement(); // panel
                return;
            }
        }

        private static void Restore(Control c, XmlDocument doc)
        {
            Control temp;

            temp = c as GroupBox;
            if (temp != null)
            {
                GroupBox grp = (GroupBox)c;
                RestoreGroupBox(grp, doc);
                foreach (Control c2 in grp.Controls)
                    Restore(c2, doc);
                return;
            }

            temp = c as Panel;
            if (temp != null)
            {
                Panel pnl = (Panel)c;
                RestorePanel(pnl, doc);
                foreach (Control c2 in pnl.Controls)
                    Restore(c2, doc);
                return;
            }

            temp = c as Button;
            if (temp != null)
            {
                RestoreButton((Button)c, doc);
                return;
            }

            temp = c as CheckBox;
            if (temp != null)
            {
                RestoreCheckBox((CheckBox)c, doc);
                return;
            }

            temp = c as ComboBox;
            if (temp != null)
            {
                RestoreComboBox((ComboBox)c, doc);
                return;
            }

            temp = c as Label;
            if (temp != null)
            {
                RestoreLabel((Label)c, doc);
                return;
            }

            temp = c as NumericUpDown;
            if (temp != null)
            {
                RestoreNumericUpDown((NumericUpDown)c, doc);
                return;
            }

            temp = c as PrettyTrackBar;
            if (temp != null)
            {
                RestorePrettyTrackBar((PrettyTrackBar)c, doc);
                return;
            }

            temp = c as PictureBox;
            if (temp != null)
            {
                RestorePictureBox((PictureBox)c, doc);
                return;
            }

            temp = c as RadioButton;
            if (temp != null)
            {
                RestoreRadioButton((RadioButton)c, doc);
                return;
            }

            temp = c as TextBox;
            if (temp != null)
            {
                RestoreTextBox((TextBox)c, doc);
                return;
            }
        }

        private static void ReadImages(Control c)
        {
            Control temp = c as GroupBox;
            if (temp != null)
            {
                GroupBox grp = (GroupBox)c;
                SetBackgroundImage(c);
                foreach (Control c2 in grp.Controls)
                    ReadImages(c2);
                return;
            }

            temp = c as Panel;
            if (temp != null)
            {
                Panel pnl = (Panel)c;
                SetBackgroundImage(c);
                foreach (Control c2 in pnl.Controls)
                    ReadImages(c2);
                return;
            }

            temp = c as Button;
            if (temp != null)
            {
                SetupButtonImages((Button)c);
                return;
            }

            temp = c as CheckBox;
            if (temp != null)
            {
                if(((CheckBox)c).Appearance == Appearance.Button)
                    SetupCheckBoxImages((CheckBox)c);
                return;
            }

            temp = c as Label;
            if (temp != null)
            {
                SetBackgroundImage(c);
                return;
            }

            temp = c as PrettyTrackBar;
            if (temp != null)
            {
                SetupPrettyTrackBarImages((PrettyTrackBar)c);
                return;
            }

            temp = c as PictureBox;
            if (temp != null)
            {
                SetBackgroundImage((PictureBox)c);
                return;
            }

            temp = c as RadioButton;
            if (temp != null)
            {
                if (((RadioButton)c).Appearance == Appearance.Button)
                    SetupRadioButtonImages((RadioButton)c);
                return;
            }

            temp = c as ucQuickRecall;
            if (temp != null)
            {
                SetupQuickRecallImages((ucQuickRecall)c);
            }

            temp = c as ucInfoBar;
            if (temp != null)
            {
                SetupInfoBar((ucInfoBar)c);
            }
        }

        #endregion

        #region Control Specific

        #region Form

        private static void SaveForm(Form ctrl, XmlTextWriter writer)
        {
            writer.WriteElementString("Name", ctrl.Name);            
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackgroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            SaveFont(ctrl.Font, writer);
            writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);           
            SaveSize(ctrl.Size, writer);
            writer.WriteElementString("Text", ctrl.Text);
            writer.WriteElementString("TransparencyKey", ctrl.TransparencyKey.Name);
        }

        private static void RestoreForm(Form ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName("Form");
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "Font":
                        ctrl.Font = RestoreFont(node);
                        break;
                    case "ForeColor":
                        ctrl.ForeColor = StringToColor(node.InnerText);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                    case "Text":
                        ctrl.Text = node.InnerText;
                        break;
                    /*case "TransparencyKey":
                        ctrl.TransparencyKey = StringToColor(node.InnerText);
                        break;*/
                }
            }
        }

        #endregion

        #region GroupBox

        private static void SaveGroupBox(GroupBox ctrl, XmlTextWriter writer)
        {
            writer.WriteElementString("Type", "GroupBox");
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            SaveFont(ctrl.Font, writer);
            writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
            writer.WriteElementString("Text", ctrl.Text);
        }

        private static void RestoreGroupBox(GroupBox ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "Font":
                        ctrl.Font = RestoreFont(node);
                        break;
                    case "ForeColor":
                        ctrl.ForeColor = StringToColor(node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                    case "Text":
                        ctrl.Text = node.InnerText;
                        break;
                }
            }
        }

        #endregion

        #region Panel

        private static void SavePanel(Panel ctrl, XmlTextWriter writer)
        {            
            writer.WriteElementString("Type", "Panel");
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            writer.WriteElementString("BorderStyle", ctrl.BorderStyle.ToString());
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
        }

        private static void RestorePanel(Panel ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                    case "Text":
                        ctrl.Text = node.InnerText;
                        break;
                }
            }
        }

        #endregion

        #region Button

        private static void SaveButton(Button ctrl, XmlTextWriter writer)
        {
            writer.WriteStartElement(ctrl.Name);
            writer.WriteElementString("Type", "Button");
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            SaveFlatAppearance(ctrl.FlatAppearance, writer);
            writer.WriteElementString("FlatStyle", ctrl.FlatStyle.ToString());            
            SaveFont(ctrl.Font, writer);
            writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
            writer.WriteElementString("Text", ctrl.Text);
            writer.WriteElementString("UseVisualStyleBackColor", ctrl.UseVisualStyleBackColor.ToString());
            writer.WriteEndElement();
        }

        private static void RestoreButton(Button ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "FlatAppearance":
                        foreach(XmlNode x in node.ChildNodes)
                        {
                            switch (x.LocalName)
                            {
                                case "BorderColor":
                                    ctrl.FlatAppearance.BorderColor = StringToColor(x.InnerText);
                                    break;
                                case "BorderSize":
                                    ctrl.FlatAppearance.BorderSize = int.Parse(x.InnerText);
                                    break;
                            }
                        }
                        break;
                    case "FlatStyle":
                        ctrl.FlatStyle = (FlatStyle)Enum.Parse(typeof(FlatStyle), node.InnerText);
                        break;
                    case "Font":
                        ctrl.Font = RestoreFont(node);
                        break;
                    case "ForeColor":
                        ctrl.ForeColor = StringToColor(node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                    case "Text":
                        ctrl.Text = node.InnerText;
                        break;
                    case "UseVisualStyleBackColor":
                        ctrl.UseVisualStyleBackColor = bool.Parse(node.InnerText);
                        break;
                }
            }
        }
        private static void SetupQuickRecallImages(ucQuickRecall ctrl)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                Button b;
                string sBut;
                switch (ii)
                {
                    case 0:
                        sBut = "_previous";
                        b = ctrl.PreviousButton;
                        break;
                    case 1:
                        sBut = "_list";
                        b = ctrl.ListButton;
                        break;
                    case 2:
                        sBut = "_next";
                        b = ctrl.NextButton;
                        break;
                    default:
                        sBut = "_previous";
                        b = ctrl.PreviousButton;
                        break;
                }

                if (b.ImageList == null)
                    b.ImageList = new ImageList();
                else b.ImageList.Images.Clear();
                b.ImageList.ImageSize = b.Size; // may be an issue with smaller images
                b.ImageList.ColorDepth = ColorDepth.Depth32Bit;

                for (int i = 0; i < 8; i++)
                {
                    if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + sBut + "-" + i.ToString() + pic_file_ext))
                        b.ImageList.Images.Add(((ImageState)i).ToString(), /*Image.FromFile*/loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + sBut + "-" + i.ToString() + pic_file_ext));
                    else
                    {
                        if (File.Exists(path + "\\" + "Console" + "\\" + ctrl.Name + sBut + "-" + i.ToString() + pic_file_ext))
                            b.ImageList.Images.Add(((ImageState)i).ToString(), /*Image.FromFile*/loadImage(path + "\\" + "Console" + "\\" + ctrl.Name + sBut + "-" + i.ToString() + pic_file_ext));
                    }
                }

                if (b.ImageList.Images.Count > 0) b.Text = ""; // clear the text if using an image //MW0LGE_21f
                setupButtonHandlers(b);
                Button_StateChanged(b, EventArgs.Empty);
            }
        }
        private static void setupButtonHandlers(Button ctrl)
        {
            EventHandler handler = new EventHandler(Button_StateChanged);
            ctrl.Click -= handler; // remove handlers first to ensure they don't get added multiple times
            ctrl.Click += handler;
            ctrl.EnabledChanged -= handler;
            ctrl.EnabledChanged += handler;
            ctrl.MouseEnter -= new EventHandler(Button_MouseEnter);
            ctrl.MouseEnter += new EventHandler(Button_MouseEnter);
            ctrl.MouseLeave -= handler;
            ctrl.MouseLeave += handler;
            ctrl.MouseDown -= new MouseEventHandler(Button_MouseDown);
            ctrl.MouseDown += new MouseEventHandler(Button_MouseDown);
            ctrl.MouseUp -= new MouseEventHandler(Button_MouseUp);
            ctrl.MouseUp += new MouseEventHandler(Button_MouseUp);
            ctrl.GotFocus -= handler;
            ctrl.GotFocus += handler;
            ctrl.LostFocus -= handler;
            ctrl.LostFocus += handler;

            ctrl.BackgroundImage = null;
        }

        private static void SetupButtonImages(Button ctrl)
        {
            if (ctrl.ImageList == null)
                ctrl.ImageList = new ImageList();
            else ctrl.ImageList.Images.Clear();
            ctrl.ImageList.ImageSize = ctrl.Size; // may be an issue with smaller images
            ctrl.ImageList.ColorDepth = ColorDepth.Depth32Bit;

            // load images into image list property
           // string s = path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-";
            for (int i = 0; i < 8; i++)
            {
                if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext))
                    ctrl.ImageList.Images.Add(((ImageState)i).ToString(), /*Image.FromFile*/loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext));
                else
                {
                    if (File.Exists(path + "\\" + "Console" + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext))
                        ctrl.ImageList.Images.Add(((ImageState)i).ToString(), /*Image.FromFile*/loadImage(path + "\\" + "Console" + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext));
                }
            }
            //EventHandler handler = new EventHandler(Button_StateChanged);
            //ctrl.Click -= handler; // remove handlers first to ensure they don't get added multiple times
            //ctrl.Click += handler;
            //ctrl.EnabledChanged -= handler;
            //ctrl.EnabledChanged += handler;
            //ctrl.MouseEnter -= new EventHandler(Button_MouseEnter);
            //ctrl.MouseEnter += new EventHandler(Button_MouseEnter);
            //ctrl.MouseLeave -= handler;
            //ctrl.MouseLeave += handler;
            //ctrl.MouseDown -= new MouseEventHandler(Button_MouseDown);
            //ctrl.MouseDown += new MouseEventHandler(Button_MouseDown);
            //ctrl.MouseUp -= new MouseEventHandler(Button_MouseUp);
            //ctrl.MouseUp += new MouseEventHandler(Button_MouseUp);
            //ctrl.GotFocus -= handler;
            //ctrl.GotFocus += handler;
            //ctrl.LostFocus -= handler;
            //ctrl.LostFocus += handler;

            //ctrl.BackgroundImage = null;
            setupButtonHandlers(ctrl);
            Button_StateChanged(ctrl, EventArgs.Empty);
        }

        private static void Button_StateChanged(object sender, EventArgs e)
        {
            Button ctrl = (Button)sender;
            ImageState state = ImageState.NormalUp;

            if (!ctrl.Enabled &&
                ctrl.ImageList.Images.IndexOfKey(ImageState.DisabledUp.ToString()) >= 0)
            {
                state = ImageState.DisabledUp;
            }
            else if (ctrl.Focused &&
                ctrl.ImageList.Images.IndexOfKey(ImageState.FocusedUp.ToString()) >= 0)
            {
                state = ImageState.FocusedUp;
            }
            else
            {
                state = ImageState.NormalUp;
            }

            SetButtonImageState(ctrl, state);
        }

        private static void Button_MouseEnter(object sender, EventArgs e)
        {
            Button ctrl = (Button)sender;
            if (!ctrl.Enabled) return;

            ImageState state = ImageState.MouseOverUp;

            SetButtonImageState(ctrl, state);
        }

        private static void Button_MouseDown(object sender, MouseEventArgs e)
        {
            Button ctrl = (Button)sender;
            if (!ctrl.Enabled) return;

            ImageState state = ImageState.NormalDown;

            SetButtonImageState(ctrl, state);
        }

        private static void Button_MouseUp(object sender, MouseEventArgs e)
        {
            Button_StateChanged(sender, EventArgs.Empty);
        }

        private static void SetButtonImageState(Button ctrl, ImageState state)
        {
            if (ctrl.ImageList == null) return;
            int index = ctrl.ImageList.Images.IndexOfKey(state.ToString());
            if (index < 0) return;
            ctrl.BackgroundImage = ctrl.ImageList.Images[index];
        }

        #endregion

        #region CheckBox

        private static void SaveCheckBox(CheckBox ctrl, XmlTextWriter writer)
        {
            writer.WriteStartElement(ctrl.Name);
            writer.WriteElementString("Type", "CheckBox");
            writer.WriteElementString("Appearance", ctrl.Appearance.ToString());
            writer.WriteElementString("AutoSize", ctrl.AutoSize.ToString());
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            SaveFlatAppearance(ctrl.FlatAppearance, writer);
            writer.WriteElementString("FlatStyle", ctrl.FlatStyle.ToString());
            SaveFont(ctrl.Font, writer);
            writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
            writer.WriteElementString("Text", ctrl.Text);
            writer.WriteElementString("UseVisualStyleBackColor", ctrl.UseVisualStyleBackColor.ToString());
            writer.WriteEndElement();
        }

        private static void RestoreCheckBox(CheckBox ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "Appearance":
                        ctrl.Appearance = (Appearance)Enum.Parse(typeof(Appearance), node.InnerText);
                        break;
                    case "AutoSize":
                        ctrl.AutoSize = bool.Parse(node.InnerText);
                        break;
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "FlatAppearance":
                        foreach (XmlNode x in node.ChildNodes)
                        {
                            switch (x.LocalName)
                            {
                                case "BorderColor":
                                    ctrl.FlatAppearance.BorderColor = StringToColor(x.InnerText);
                                    break;
                                case "BorderSize":
                                    ctrl.FlatAppearance.BorderSize = int.Parse(x.InnerText);
                                    break;
                            }
                        }
                        break;
                    case "FlatStyle":
                        ctrl.FlatStyle = (FlatStyle)Enum.Parse(typeof(FlatStyle), node.InnerText);
                        break;
                    case "Font":
                        ctrl.Font = RestoreFont(node);
                        break;
                    case "ForeColor":
                        ctrl.ForeColor = StringToColor(node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                    case "Text":
                        ctrl.Text = node.InnerText;
                        break;
                    case "UseVisualStyleBackColor":
                        ctrl.UseVisualStyleBackColor = bool.Parse(node.InnerText);
                        break;
                }
            }
        }

        private static void SetupCheckBoxImages(CheckBox ctrl)
        {
            if (ctrl.ImageList == null)
                ctrl.ImageList = new ImageList();
            else ctrl.ImageList.Images.Clear();
            ctrl.ImageList.ImageSize = ctrl.Size; // may be an issue with smaller images
            ctrl.ImageList.ColorDepth = ColorDepth.Depth32Bit;

            // load images into image list property
            //string s = path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-";
            for(int i=0; i<8; i++)
            {
                if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext))
                    ctrl.ImageList.Images.Add(((ImageState)i).ToString(), /*Image.FromFile*/loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext));
                else
                {
                    if (File.Exists(path + "\\" + "Console" + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext))
                        ctrl.ImageList.Images.Add(((ImageState)i).ToString(), /*Image.FromFile*/loadImage(path + "\\" + "Console" + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext));
                }
            }

            setupCheckBoxHandlers(ctrl);
           
            CheckBox_StateChanged(ctrl, EventArgs.Empty);
        }

        private static void setupCheckBoxHandlers(CheckBox ctrl)
        {
            EventHandler handler = new EventHandler(CheckBox_StateChanged);
            ctrl.CheckedChanged -= handler; // remove handlers first to ensure they don't get added multiple times
            ctrl.CheckedChanged += handler;
            ctrl.EnabledChanged -= handler;
            ctrl.EnabledChanged += handler;
            ctrl.MouseEnter -= new EventHandler(CheckBox_MouseEnter);
            ctrl.MouseEnter += new EventHandler(CheckBox_MouseEnter);
            ctrl.MouseLeave -= handler;
            ctrl.MouseLeave += handler;
            ctrl.GotFocus -= handler;
            ctrl.GotFocus += handler;
            ctrl.LostFocus -= handler;
            ctrl.LostFocus += handler;

            ctrl.BackgroundImage = null;
        }
        private static void SetupInfoBar(ucInfoBar ctrl)
        {
            for (int ii = 0; ii < 2; ii++)
            {
                CheckBox b;
                string sBut = "_button" + (ii+1).ToString();

                if (ii == 0)
                    b = ctrl.Button1;
                else
                    b = ctrl.Button2;

                if (b.ImageList == null)
                    b.ImageList = new ImageList();
                else b.ImageList.Images.Clear();

                b.ImageList.ImageSize = b.Size; // may be an issue with smaller images
                b.ImageList.ColorDepth = ColorDepth.Depth32Bit;

                for (int i = 0; i < 8; i++)
                {
                    if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + sBut + "-" + i.ToString() + pic_file_ext))
                        b.ImageList.Images.Add(((ImageState)i).ToString(), /*Image.FromFile*/loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + sBut + "-" + i.ToString() + pic_file_ext));
                    else
                    {
                        if (File.Exists(path + "\\" + "Console" + "\\" + ctrl.Name + sBut + "-" + i.ToString() + pic_file_ext))
                            b.ImageList.Images.Add(((ImageState)i).ToString(), /*Image.FromFile*/loadImage(path + "\\" + "Console" + "\\" + ctrl.Name + sBut + "-" + i.ToString() + pic_file_ext));
                    }
                }

                if (b.ImageList.Images.Count > 0)
                {
                    b.BackColor = Color.Transparent;
                    b.FlatAppearance.CheckedBackColor = Color.Transparent;
                    b.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    b.FlatAppearance.MouseDownBackColor = Color.Transparent;
                }
                else
                {
                    b.BackColor = Color.Transparent;
                    b.FlatAppearance.CheckedBackColor = Color.Silver;
                    b.FlatAppearance.MouseOverBackColor = Color.Gray;
                    b.FlatAppearance.MouseDownBackColor = Color.Gray;
                }

                setupCheckBoxHandlers(b);
                CheckBox_StateChanged(b, EventArgs.Empty);

                //setup the 1-8 buttons
                for(int n = 0; n < 8; n++)
                {
                    CheckBox popupButton = ctrl.GetPopupButton(ii + 1, n);
                    if (popupButton != null)
                    {
                        if (popupButton.ImageList == null)
                            popupButton.ImageList = new ImageList();
                        else popupButton.ImageList.Images.Clear();
                        popupButton.ImageList.ImageSize = popupButton.Size;
                        popupButton.ImageList.ColorDepth = ColorDepth.Depth32Bit;

                        if (b.ImageList.Images.Count > 0)
                        {
                            // copy images !
                            for (int nn = 0;nn < b.ImageList.Images.Count; nn++)
                            {
                                popupButton.ImageList.Images.Add(b.ImageList.Images.Keys[nn], b.ImageList.Images[nn]);
                            }
                            //
                            popupButton.BackColor = Color.Transparent;
                            popupButton.FlatAppearance.CheckedBackColor = Color.Transparent;
                            popupButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
                            popupButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
                        }
                        else
                        {
                            popupButton.BackColor = Color.Transparent;
                            popupButton.FlatAppearance.CheckedBackColor = Color.Silver;
                            popupButton.FlatAppearance.MouseOverBackColor = Color.Gray;
                            popupButton.FlatAppearance.MouseDownBackColor = Color.Gray;
                        }
                        setupCheckBoxHandlers(popupButton);
                        CheckBox_StateChanged(popupButton, EventArgs.Empty);
                    }
                }
            }
        }
        private static void CheckBox_StateChanged(object sender, EventArgs e)
        {
            CheckBox ctrl = (CheckBox)sender;
            ImageState state = ImageState.NormalUp;

            if (!ctrl.Enabled &&
                ctrl.ImageList.Images.IndexOfKey(ImageState.DisabledDown.ToString()) >= 0 &&
                ctrl.ImageList.Images.IndexOfKey(ImageState.DisabledUp.ToString()) >= 0)
            {
                state = ctrl.Checked ? ImageState.DisabledDown : ImageState.DisabledUp;
            }
            else if (ctrl.Focused && 
                ctrl.ImageList.Images.IndexOfKey(ImageState.FocusedDown.ToString()) >= 0 &&
                ctrl.ImageList.Images.IndexOfKey(ImageState.FocusedUp.ToString()) >= 0)
            {
                state = ctrl.Checked ? ImageState.FocusedDown : ImageState.FocusedUp;
            }
            else
            {
                state = ctrl.Checked ? ImageState.NormalDown : ImageState.NormalUp;
            }

            SetCheckBoxImageState(ctrl, state);
        }

        private static void CheckBox_MouseEnter(object sender, EventArgs e)
        {
            CheckBox ctrl = (CheckBox)sender;
            if (!ctrl.Enabled) return;

            ImageState state = ImageState.MouseOverUp;
            if (ctrl.Checked) state = ImageState.MouseOverDown;

            SetCheckBoxImageState(ctrl, state);
        }

        private static void SetCheckBoxImageState(CheckBox ctrl, ImageState state)
        {
            if (ctrl.ImageList == null) return;
            int index = ctrl.ImageList.Images.IndexOfKey(state.ToString());
            if (index < 0) return;
            ctrl.BackgroundImage = ctrl.ImageList.Images[index];
        }

#endregion

        #region ComboBox

        private static void SaveComboBox(ComboBox ctrl, XmlTextWriter writer)
        {
            writer.WriteStartElement(ctrl.Name);
            writer.WriteElementString("Type", "ComboBox");
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            writer.WriteElementString("FlatStyle", ctrl.FlatStyle.ToString());
            SaveFont(ctrl.Font, writer);
            writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
            writer.WriteEndElement();
        }

        private static void RestoreComboBox(ComboBox ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "FlatStyle":
                        ctrl.FlatStyle = (FlatStyle)Enum.Parse(typeof(FlatStyle), node.InnerText);
                        break;
                    case "Font":
                        ctrl.Font = RestoreFont(node);
                        break;
                    case "ForeColor":
                        ctrl.ForeColor = StringToColor(node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                }
            }
        }

        #endregion

        #region Label

        private static void SaveLabel(Label ctrl, XmlTextWriter writer)
        {
            writer.WriteStartElement(ctrl.Name);
            writer.WriteElementString("Type", "Label");
            writer.WriteElementString("AutoSize", ctrl.AutoSize.ToString());
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            SaveFont(ctrl.Font, writer);
            writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
            writer.WriteElementString("Text", ctrl.Text);
            writer.WriteEndElement();
        }

        private static void RestoreLabel(Label ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "AutoSize":
                        ctrl.AutoSize = bool.Parse(node.InnerText);
                        break;
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "Font":
                        ctrl.Font = RestoreFont(node);
                        break;
                    case "ForeColor":
                        ctrl.ForeColor = StringToColor(node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                    case "Text":
                        ctrl.Text = node.InnerText;
                        break;
                }
            }
        }

        #endregion

        #region NumericUpDown

        private static void SaveNumericUpDown(NumericUpDown ctrl, XmlTextWriter writer)
        {
            writer.WriteStartElement(ctrl.Name);
            writer.WriteElementString("Type", "NumericUpDown");
            writer.WriteElementString("AutoSize", ctrl.AutoSize.ToString());
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BorderStyle", ctrl.BorderStyle.ToString());
            SaveFont(ctrl.Font, writer);
            writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
            writer.WriteEndElement();
        }

        private static void RestoreNumericUpDown(NumericUpDown ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "AutoSize":
                        ctrl.AutoSize = bool.Parse(node.InnerText);
                        break;
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BorderStyle":
                        ctrl.BorderStyle = (BorderStyle)Enum.Parse(typeof(BorderStyle), node.InnerText);
                        break;
                    case "Font":
                        ctrl.Font = RestoreFont(node);
                        break;
                    case "ForeColor":
                        ctrl.ForeColor = StringToColor(node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                }
            }
        }

        #endregion

        #region PictureBox

        private static void SavePictureBox(PictureBox ctrl, XmlTextWriter writer)
        {
            writer.WriteStartElement(ctrl.Name);
            writer.WriteElementString("Type", "PictureBox");
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            writer.WriteElementString("BorderStyle", ctrl.BorderStyle.ToString());
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
            writer.WriteEndElement();
        }

        private static void RestorePictureBox(PictureBox ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                    case "Text":
                        ctrl.Text = node.InnerText;
                        break;
                }
            }
        }

        #endregion

        #region RadioButton

        private static void SaveRadioButton(RadioButton ctrl, XmlTextWriter writer)
        {
            writer.WriteStartElement(ctrl.Name);
            writer.WriteElementString("Type", "RadioButton");
            writer.WriteElementString("Appearance", ctrl.Appearance.ToString());
            writer.WriteElementString("AutoSize", ctrl.AutoSize.ToString());
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            SaveFlatAppearance(ctrl.FlatAppearance, writer);
            writer.WriteElementString("FlatStyle", ctrl.FlatStyle.ToString());
            SaveFont(ctrl.Font, writer);
            writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
            writer.WriteElementString("Text", ctrl.Text);
            writer.WriteElementString("UseVisualStyleBackColor", ctrl.UseVisualStyleBackColor.ToString());
            writer.WriteEndElement();
        }

        private static void RestoreRadioButton(RadioButton ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "Appearance":
                        ctrl.Appearance = (Appearance)Enum.Parse(typeof(Appearance), node.InnerText);
                        break;
                    case "AutoSize":
                        ctrl.AutoSize = bool.Parse(node.InnerText);
                        break;
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "FlatAppearance":
                        foreach (XmlNode x in node.ChildNodes)
                        {
                            switch (x.LocalName)
                            {
                                case "BorderColor":
                                    ctrl.FlatAppearance.BorderColor = StringToColor(x.InnerText);
                                    break;
                                case "BorderSize":
                                    ctrl.FlatAppearance.BorderSize = int.Parse(x.InnerText);
                                    break;
                            }
                        }
                        break;
                    case "FlatStyle":
                        ctrl.FlatStyle = (FlatStyle)Enum.Parse(typeof(FlatStyle), node.InnerText);
                        break;
                    case "Font":
                        ctrl.Font = RestoreFont(node);
                        break;
                    case "ForeColor":
                        ctrl.ForeColor = StringToColor(node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                    case "Text":
                        ctrl.Text = node.InnerText;
                        break;
                    case "UseVisualStyleBackColor":
                        ctrl.UseVisualStyleBackColor = bool.Parse(node.InnerText);
                        break;
                }
            }
        }

        private static void SetupRadioButtonImages(RadioButton ctrl)
        {
            if (ctrl.ImageList == null)
                ctrl.ImageList = new ImageList();
            else ctrl.ImageList.Images.Clear();
            ctrl.ImageList.ImageSize = ctrl.Size; // may be an issue with smaller images
            ctrl.ImageList.ColorDepth = ColorDepth.Depth32Bit;

            // load images into image list property
           // string s = path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-";
            for (int i = 0; i < 8; i++)
            {
                if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext))
                    ctrl.ImageList.Images.Add(((ImageState)i).ToString(), /*Image.FromFile*/loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext));
                else if (File.Exists(path + "\\" + "Console" + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext))
                    ctrl.ImageList.Images.Add(((ImageState)i).ToString(), /*Image.FromFile*/loadImage(path + "\\" + "Console" + "\\" + ctrl.Name + "-" + i.ToString() + pic_file_ext));
                else
                {
                    if (ctrl.ImageList.Images.ContainsKey(((ImageState)i).ToString()))
                        ctrl.ImageList.Images.RemoveByKey(((ImageState)i).ToString());
                }
            }
            EventHandler handler = new EventHandler(RadioButton_StateChanged);
            ctrl.CheckedChanged -= handler; // remove handlers first to ensure they don't get added multiple times
            ctrl.CheckedChanged += handler;
            ctrl.EnabledChanged -= handler;
            ctrl.EnabledChanged += handler;
            ctrl.MouseEnter -= new EventHandler(RadioButton_MouseEnter);
            ctrl.MouseEnter += new EventHandler(RadioButton_MouseEnter);
            //MW0LGE_21d surely we want to add handlers?
            //ctrl.GotFocus += handler;
            //ctrl.GotFocus -= handler;
            //ctrl.LostFocus += handler;
            //ctrl.LostFocus -= handler;
            ctrl.GotFocus -= handler;
            ctrl.GotFocus += handler;
            ctrl.LostFocus -= handler;
            ctrl.LostFocus += handler;

            ctrl.BackgroundImage = null;
            RadioButton_StateChanged(ctrl, EventArgs.Empty);
        }

        private static void RadioButton_StateChanged(object sender, EventArgs e)
        {
            RadioButton ctrl = (RadioButton)sender;
            ImageState state = ImageState.NormalUp;
            if (!ctrl.Enabled &&
                ctrl.ImageList.Images.IndexOfKey(ImageState.DisabledDown.ToString()) >= 0 &&
                ctrl.ImageList.Images.IndexOfKey(ImageState.DisabledUp.ToString()) >= 0)
            {
                state = ctrl.Checked ? ImageState.DisabledDown : ImageState.DisabledUp;
            }
            else if (ctrl.Focused &&
                ctrl.ImageList.Images.IndexOfKey(ImageState.FocusedDown.ToString()) >= 0 &&
                ctrl.ImageList.Images.IndexOfKey(ImageState.FocusedUp.ToString()) >= 0)
            {
                if (ctrl.Checked)
                    state = ImageState.FocusedDown;
                else
                    state = ImageState.FocusedUp;
            }
            else
            {
                state = ctrl.Checked ? ImageState.NormalDown : ImageState.NormalUp;
            }

            SetRadioButtonImageState(ctrl, state);
        }

        private static void RadioButton_MouseEnter(object sender, EventArgs e)
        {
            RadioButton ctrl = (RadioButton)sender;
            if (!ctrl.Enabled) return;

            ImageState state = ImageState.MouseOverUp;
            if (ctrl.Checked) state = ImageState.MouseOverDown;

            SetRadioButtonImageState(ctrl, state);
        }

        private static void SetRadioButtonImageState(RadioButton ctrl, ImageState state)
        {
            if (ctrl.ImageList == null) return;
            int index = ctrl.ImageList.Images.IndexOfKey(state.ToString());
            if (index < 0) return;
            ctrl.BackgroundImage = ctrl.ImageList.Images[index];
        }

        #endregion

        #region TextBox

        private static void SaveTextBox(TextBox ctrl, XmlTextWriter writer)
        {
            writer.WriteStartElement(ctrl.Name);
            writer.WriteElementString("Type", "CheckBox");
            writer.WriteElementString("AutoSize", ctrl.AutoSize.ToString());
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            writer.WriteElementString("BorderStyle", ctrl.BorderStyle.ToString());
            SaveFont(ctrl.Font, writer);
            writer.WriteElementString("ForeColor", ctrl.ForeColor.Name);
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
            writer.WriteElementString("Text", ctrl.Text);
            writer.WriteEndElement();
        }

        private static void RestoreTextBox(TextBox ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "AutoSize":
                        ctrl.AutoSize = bool.Parse(node.InnerText);
                        break;
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "BorderStyle":
                        ctrl.BorderStyle = (BorderStyle)Enum.Parse(typeof(BorderStyle), node.InnerText);
                        break;
                    case "Font":
                        ctrl.Font = RestoreFont(node);
                        break;
                    case "ForeColor":
                        ctrl.ForeColor = StringToColor(node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                    case "Text":
                        ctrl.Text = node.InnerText;
                        break;
                }
            }
        }

        #endregion

        #region PrettyTrackBar

        private static void SavePrettyTrackBar(PrettyTrackBar ctrl, XmlTextWriter writer)
        {
            writer.WriteStartElement(ctrl.Name);
            writer.WriteElementString("Type", "PrettyTrackBar");
            writer.WriteElementString("BackColor", ctrl.BackColor.Name);
            writer.WriteElementString("BackGroundImageLayout", ctrl.BackgroundImageLayout.ToString());
            SaveLocation(ctrl.Location, writer);
            SaveSize(ctrl.Size, writer);
            writer.WriteEndElement();
        }

        private static void RestorePrettyTrackBar(PrettyTrackBar ctrl, XmlDocument doc)
        {
            XmlNodeList matches = doc.GetElementsByTagName(ctrl.Name);
            if (matches.Count == 0) // not found
                return;

            Debug.Assert(matches.Count == 1);

            foreach (XmlNode node in matches[0].ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "BackColor":
                        ctrl.BackColor = StringToColor(node.InnerText);
                        break;
                    case "BackgroundImageLayout":
                        ctrl.BackgroundImageLayout = (ImageLayout)Enum.Parse(typeof(ImageLayout), node.InnerText);
                        break;
                    case "Location":
                        ctrl.Location = RestoreLocation(node);
                        break;
                    case "Size":
                        ctrl.Size = RestoreSize(node);
                        break;
                }
            }
        }

        private static void SetupPrettyTrackBarImages(PrettyTrackBar ctrl)
        {
            // load images
           // string s = path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-";

            if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + "back" + pic_file_ext))
            {
                ctrl.BackgroundImage = /*Image.FromFile*/loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + "back" + pic_file_ext);
            }
            else if (File.Exists(path + "\\" + "Console" + "\\" + ctrl.Name + "-" + "back" + pic_file_ext))
            {
                ctrl.BackgroundImage = /*Image.FromFile*/loadImage(path + "\\" + "Console" + "\\" + ctrl.Name + "-" + "back" + pic_file_ext);
            }
            else ctrl.BackgroundImage = null;

            //             if (File.Exists(s + "back" + pic_file_ext))
            //                 ctrl.BackgroundImage = /*Image.FromFile*/loadImage(s + "back" + pic_file_ext);
            //             else ctrl.BackgroundImage = null;

            if (File.Exists(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + "head" + pic_file_ext))
            {
                ctrl.HeadImage = /*Image.FromFile*/loadImage(path + "\\" + ctrl.TopLevelControl.Name + "\\" + ctrl.Name + "-" + "head" + pic_file_ext);
            }
            else if (File.Exists(path + "\\" + "Console" + "\\" + ctrl.Name + "-" + "head" + pic_file_ext))
            {             
                ctrl.HeadImage = /*Image.FromFile*/loadImage(path + "\\" + "Console" + "\\" + ctrl.Name + "-" + "head" + pic_file_ext);
            }
            else ctrl.HeadImage = null;

           // ctrl.HeadImage = File.Exists(s + "head" + pic_file_ext) ? /*Image.FromFile*/loadImage(s + "head" + pic_file_ext) : null;

            ctrl.Invalidate();
        }

        #endregion

        #endregion

        #region Utility

        private static void SaveSize(Size s, XmlTextWriter writer)
        {
            writer.WriteStartElement("Size");
            writer.WriteElementString("Width", s.Width.ToString());
            writer.WriteElementString("Height", s.Height.ToString());
            writer.WriteEndElement();
        }

        private static Size RestoreSize(XmlNode node)
        {
            Size s = new Size(0, 0);
            foreach (XmlNode x in node.ChildNodes)
            {
                switch (x.LocalName)
                {
                    case "Width":
                        s.Width = int.Parse(x.InnerText);
                        break;
                    case "Height":
                        s.Height = int.Parse(x.InnerText);
                        break;
                }
            }
            return s;
        }

        private static void SaveFont(Font f, XmlTextWriter writer)
        {
            writer.WriteStartElement("Font");
            writer.WriteElementString("FontFamily", f.FontFamily.Name);
            writer.WriteElementString("Size", f.Size.ToString());
            writer.WriteElementString("Bold", f.Bold.ToString());
            writer.WriteElementString("Italic", f.Italic.ToString());
            writer.WriteElementString("Underline", f.Underline.ToString());
            writer.WriteEndElement();
        }

        private static Font RestoreFont(XmlNode node)
        {
            string family = "Arial";
            float size = 8.25f;
            bool bold = false, italic = false, underline = false;

            foreach (XmlNode x in node.ChildNodes)
            {
                switch (x.LocalName)
                {
                    case "FontFamily":
                        family = x.InnerText;
                        break;
                    case "Size":
                        size = float.Parse(x.InnerText);
                        break;
                    case "Bold":
                        bold = bool.Parse(x.InnerText);
                        break;
                    case "Italic":
                        italic = bool.Parse(x.InnerText);
                        break;
                    case "Underline":
                        underline = bool.Parse(x.InnerText);
                        break;
                }
            }

            FontStyle style = FontStyle.Regular;
            if (bold) style |= FontStyle.Bold;
            if (italic) style |= FontStyle.Italic;
            if (underline) style |= FontStyle.Underline;
            return new Font(family, size, style);
        }

        private static void SaveLocation(Point p, XmlTextWriter writer)
        {
            writer.WriteStartElement("Location");
            writer.WriteElementString("X", p.X.ToString());
            writer.WriteElementString("Y", p.Y.ToString());
            writer.WriteEndElement();
        }

        private static Point RestoreLocation(XmlNode node)
        {
            Point p = new Point();
            foreach (XmlNode x in node.ChildNodes)
            {
                switch (x.LocalName)
                {
                    case "X":
                        p.X = int.Parse(x.InnerText);
                        break;
                    case "Y":
                        p.Y = int.Parse(x.InnerText);
                        break;
                }
            }
            return p;
        }

        private static void SaveFlatAppearance(FlatButtonAppearance fa, XmlTextWriter writer)
        {
            writer.WriteStartElement("FlatAppearance");
            writer.WriteElementString("BorderColor", fa.BorderColor.Name);
            writer.WriteElementString("BorderSize", fa.BorderSize.ToString());
            writer.WriteEndElement();
        }

        private static Color StringToColor(string s)
        {
            Color c = Color.FromName(s);
            if(!c.IsKnownColor)
                c = Color.FromArgb(int.Parse(s, NumberStyles.HexNumber));
            return c;
        }

        private static void SetBackgroundImage(Control c)
        {
            Image objImg; // = File.Exists(path + "\\" + c.TopLevelControl.Name + "\\" + c.Name + pic_file_ext) ? /*Image.FromFile*/loadImage(path + "\\" + c.TopLevelControl.Name + "\\" + c.Name + pic_file_ext) : null;

            if (File.Exists(path + "\\" + c.TopLevelControl.Name + "\\" + c.Name + pic_file_ext))
            {
                objImg = /*Image.FromFile*/loadImage(path + "\\" + c.TopLevelControl.Name + "\\" + c.Name + pic_file_ext);
            }
            else if (File.Exists(path + "\\" + "Console" + "\\" + c.Name + pic_file_ext))
            {
                objImg = /*Image.FromFile*/loadImage(path + "\\" + "Console" + "\\" + c.Name + pic_file_ext);
            }
            else objImg = null;

            if (c.Name.Equals("picDisplay")) // special case
            {
                m_objConsole.PicDisplayBackgroundImage = objImg;
            }
            else
            {
                c.BackgroundImage = objImg;
            }
        }

        private static Image loadImage(string path)
        {
            //[2.10.2.2] MW0LGE
            //notes: Image.LoadFile leaves open file handles, until app is closed, meaning skins can not be deleted.
            //They get released when the images in each ImageList are removed or .clear() however there were some left over from time to time.
            //Tried all the methods below. Solved by using this bitmap hack.

            Image img;
            try
            {
                using (Image imgFile = Image.FromFile(path))
                {
                    img = new Bitmap(imgFile);
                }
            }
            catch (Exception ex)
            {
                img = null;
            }
            return img;

            ////TEST1
            //byte[] data = File.ReadAllBytes(path);
            //Image img = null;
            //if (data.Length > 0)
            //{
            //    using (MemoryStream ms = new MemoryStream(data))
            //    {
            //        img = Image.FromStream(ms);
            //    }
            //}
            //return img;

            ////TEST2
            //FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);            
            //byte[] data;
            //data = new byte[fileStream.Length];
            //fileStream.Read(data, 0, data.Length);
            //fileStream.Close();
            //fileStream.Dispose();
            //MemoryStream ms = new MemoryStream(data);
            //Image img = Image.FromStream(ms);
            //ms.Close();
            //ms.Dispose();

            //return img;
        }
        #endregion
    }
}
