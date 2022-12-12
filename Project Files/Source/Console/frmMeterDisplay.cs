using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Thetis
{
    public partial class frmMeterDisplay : Form
    {
        private Console _console;
        private int _rx;
        public frmMeterDisplay(Console c, int rx)
        {
            InitializeComponent();

            _console = c;
            _rx = rx;

            Common.RestoreForm(this, "MeterDisplay_" + _rx.ToString(), true);
            Common.ForceFormOnScreen(this);
        }

        private void frmMeterDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }

            Common.SaveForm(this, "MeterDisplay_" + _rx.ToString());
        }

        public void TakeOwner(ucMeter m)
        {
            m.Parent = this;
            m.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            m.Location = new Point(0, 0);            
            m.Size = new Size(this.Width, this.Height);
            m.BringToFront();
            m.Show();
        }
    }
}
