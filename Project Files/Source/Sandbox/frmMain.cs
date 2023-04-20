using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sandbox
{
    public partial class frmMain : Form
    {

        public frmMain()
        {
            InitializeComponent();

        }
        public frmMain(frmSplash mySplash)
        {
            this.Opacity = 0.00;
            this.Visible = false;
            Splash = mySplash;
            InitializeComponent();
            SomeLongOperation();
            Splash.Close();
            FadeIn(this);
        }

        static void FadeIn(Form f, int granularity = 10)
        {
            f.Opacity = 0.00;
            f.Show();
            float step = granularity / 1000.0f;
            while (f.Opacity < 1)
            {
                f.Opacity += step;
                Thread.Sleep(granularity);
                Application.DoEvents();
            }
        }

        void SomeLongOperation(uint seconds = 2)
        {
            var i = 0;
            var remain = (float)seconds;
            while (i < seconds * 1000)
            {
                Splash.ShowInfo("Please wait " + " another " + remain.ToString() + " seconds");
                Application.DoEvents();
                Thread.Sleep(10);
                remain -= 0.01f;

                i += 10;
            }
        }

        public frmSplash Splash { get; set; }
    }
}
