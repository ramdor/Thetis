using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Thetis
{
    public partial class wideband : Form
    {
        public wideband(int i)
        {
            InitializeComponent();

            this.MaximumSize = new Size(4096, 4096);

            Common.DoubleBufferAll(this, true);
            wbdisplay.init = true;
            wbdisplay.ADC = i;
            wbdisplay.create_wideband(i);
            wbdisplay.initWideband();
            GetWideBand();
        }

        public wbDisplay WBdisplay
        {
            get { return wbdisplay; }
            set { wbdisplay = value; }
        }

        private void wideband_Resize(object sender, EventArgs e)
        {
            wbdisplay.pauseDisplayThread = true;

            if (this.WindowState == FormWindowState.Minimized) return; // MW0LGE pause the display thread and return

            // System.Threading.Thread.Sleep(100);
            wbdisplay.Init();
            wbdisplay.UpdateGraphicsBuffer();
            // wbdisplay.Invalidate();
            wbdisplay.pauseDisplayThread = false;
        }

        private void wideband_FormClosing(object sender, FormClosingEventArgs e)
        {
            wbdisplay.Cancel_Display();
            NetworkIO.SetWBEnable(0, 0);
            Console.getConsole().wbClosing();
            this.Hide();
            e.Cancel = true;
            SaveWideBand();
        }

        private void contextMenuStripWideBand_Opening(object sender, CancelEventArgs e)
        {
            if (wbdisplay.mouseRegion == DisplayRegion.dBmScalePanadapterRegion)
                e.Cancel = true;

            wbAvgtoolStripMenuItem.Checked = wbdisplay.AverageOn;
            wbUpdatetoolStripComboBox.Text = wbdisplay.UpdateRate;
            wbFrameSizetoolStripComboBox.Text = wbdisplay.FrameSize;
        }

        private ToolStripMenuItem ContextMenuToolStripMenuItem = null;
        private Color ContextMenuToolStripMenuItemFontColor = Color.Empty;

        private void ToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            if (sender == null) return;
            if (sender.GetType() != typeof(ToolStripMenuItem)) return;
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            ContextMenuToolStripMenuItem = item;
            ContextMenuToolStripMenuItemFontColor = item.ForeColor;
            item.ForeColor = Color.Black;
        }

        private void ToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            if (sender == null) return;
            if (sender.GetType() != typeof(ToolStripMenuItem)) return;
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            if (ContextMenuToolStripMenuItem == null) return;
            item.ForeColor = ContextMenuToolStripMenuItemFontColor;
            ContextMenuToolStripMenuItem = null;
            ContextMenuToolStripMenuItemFontColor = Color.Empty;
        }

        private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (sender == null) return;
            if (sender.GetType() != typeof(ContextMenuStrip)) return;
            ContextMenuStrip menu = (ContextMenuStrip)sender;

            if (ContextMenuToolStripMenuItem == null) return;
            ContextMenuToolStripMenuItem.ForeColor = ContextMenuToolStripMenuItemFontColor;
            ContextMenuToolStripMenuItem = null;
            ContextMenuToolStripMenuItemFontColor = Color.Empty;
        }

        private void wbAvgtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            wbdisplay.AverageOn = !wbAvgtoolStripMenuItem.Checked;
        }

        private void wbdisplay_Resize(object sender, EventArgs e)
        {

        }

        private void wbUpdatetoolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (wbUpdatetoolStripComboBox.SelectedIndex < 0) return;
            wbdisplay.UpdateRate = wbUpdatetoolStripComboBox.Text;
        }

        private void wbFrameSizetoolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (wbFrameSizetoolStripComboBox.SelectedIndex < 0) return;
            wbdisplay.FrameSize = wbFrameSizetoolStripComboBox.Text;
        }

        public void SaveWideBand()
        {
            List<string> a = new List<string>();

            a.Add("average_on/" + wbdisplay.AverageOn);
            a.Add("update_rate/" + wbdisplay.UpdateRate);
            a.Add("frame_size/" + wbdisplay.FrameSize);
            a.Add("spectrum_grid_max/" + wbdisplay.SpectrumGridMax);
            a.Add("spectrum_grid_min/" + wbdisplay.SpectrumGridMin);

            a.Add("Top/" + this.Top);
            a.Add("Left/" + this.Left);
            a.Add("Width/" + this.Width);
            a.Add("Height/" + this.Height);

            DB.SaveVars("WideBand", a);
        }

        public void GetWideBand()
        {
            List<string> a = DB.GetVars("WideBand");
            a.Sort();

            foreach (string s in a)
            {
                string[] vals = s.Split('/');
                if (vals.Length > 2)
                {
                    for (int i = 2; i < vals.Length; i++)
                        vals[1] += "/" + vals[i];
                }

                string name = vals[0];
                string val = vals[1];

                switch (name)
                {
                    case "Top":
                        this.StartPosition = FormStartPosition.Manual;
                        int top = int.Parse(val);
                        this.Top = top;
                        break;
                    case "Left":
                        this.StartPosition = FormStartPosition.Manual;
                        int left = int.Parse(val);
                        this.Left = left;
                        break;
                    case "Width":
                        int width = int.Parse(val);
                        this.Width = width;
                        break;
                    case "Height":
                        int height = int.Parse(val);
                        this.Height = height;
                        break;
                    case "average_on":
                        wbdisplay.AverageOn = bool.Parse(val);
                        break;
                    case "update_rate":
                        wbdisplay.UpdateRate = val;
                        break;
                    case "frame_size":
                        wbdisplay.FrameSize = val;
                        break;
                    case "spectrum_grid_max":
                        wbdisplay.SpectrumGridMax = int.Parse(val);
                        break;
                    case "spectrum_grid_min":
                        wbdisplay.SpectrumGridMin = int.Parse(val);
                        break;
                }

            }

        }
    }
}
