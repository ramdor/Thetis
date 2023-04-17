using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis.KLJ
{
    public partial class frmKLJ : Form
    {
        public frmKLJ()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // this.Text = iPv4Control1.Text;
            this.Text = this.ipAddressControl1.Text;
        }
    }
}
