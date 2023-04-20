using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sandbox
{
    public partial class frmSplash : Form
    {

        public frmSplash()
        {
            InitializeComponent();
        }

        public frmSplash(Form owner)
        {
            this.Owner = owner;
            InitializeComponent();
        }

        public void ShowInfo(string info)
        {
            if (info != null)
            {
                this.lblInfo.Text = info;
            }
        }
    }
}
