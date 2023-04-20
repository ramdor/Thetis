using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis.KLJ
{
    internal class Utils
    {
        internal static void OpenInExplorer(string path)
        {
            using (Process.Start(path)) { }
        }

        internal static void FadeIn(Form f, int granularity = 10)
        {
            try
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
            catch (Exception ex)
            {
                Common.LogException(ex);
            }
        }
    }
}
