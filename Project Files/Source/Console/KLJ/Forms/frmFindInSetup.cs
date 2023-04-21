using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ListBox;

namespace Thetis.KLJ.Forms
{
    public partial class frmFindInSetup : Form
    {
        public Console console { get; set; }
        public frmFindInSetup(Console c)
        {
            this.Owner = c;
            console = c;
            InitializeComponent();


        }

        struct ControlDetails
        {
            public ControlDetails(Control ctl)
            {
                m_ctl = ctl;
            }
            Control m_ctl;
            public override string ToString()
            {
                if (!string.IsNullOrEmpty(m_ctl.Name))
                    return m_ctl.Name;
                if (!string.IsNullOrEmpty(m_ctl.Text))
                    return m_ctl.Text;
                var t = m_ctl.GetType().Name;
                return "";
            }
        }


        private void btnFind_Click(object sender, EventArgs e)
        {
            var ctrls = KLJ.Utils.GetAllControls(console.SetupForm);
            var mylist = new ObjectCollection(listBox1);
            listBox1.Items.Clear();
            foreach (var ctrl in ctrls)
            {
                mylist.Add(new ControlDetails(ctrl));
            }
            this.listBox1.Items.AddRange(mylist);
        }
    }
}
