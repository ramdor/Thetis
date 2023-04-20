using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis.KLJ
{

    public static class GenericToStringExts
    {
        public static string ToStringExt<K, V>(this Dictionary<K, V> list) => "[" + string.Join(",\r\n ", list) + "]";
    }


    internal class Utils
    {


        internal static void PrintZOrder()
        {
            var col = KLJ.Utils.WindowsApi.GetWindowsInOrder();
            Debug.Print("Zorder of windows: \n");
            Debug.Print(col.ToStringExt());
        }

        internal static void OpenInExplorer(string path)
        {
            using (Process.Start(path)) { }
        }

        internal static void FadeIn(Form f, int granularity = 20)
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
                // Common.LogException(ex);
            }
        }


        public static class WindowsApi
        {
            public static Dictionary<IntPtr, string> GetWindowsInOrder()
            {
                return Process
                    .GetProcesses()
                    .Where(process => process.MainWindowHandle != IntPtr.Zero)
                    .Select(process => process.MainWindowHandle)
                    .OrderBy(GetWindowZOrder)
                    .ToDictionary(hWnd => hWnd, GetWindowText);
            }

            public static int GetWindowZOrder(IntPtr hWnd)
            {
                var zOrder = -1;
                while ((hWnd = GetWindow(hWnd, 2 /* GW_HWNDNEXT */)) != IntPtr.Zero) zOrder++;
                return zOrder;
            }

            public static string GetWindowText(IntPtr hWnd)
            {
                var text = new StringBuilder(255);
                GetWindowText(hWnd, text, text.Capacity);
                return text.ToString();
            }

            [DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

            [DllImport("user32.dll", CharSet = CharSet.None, SetLastError = true)]
            static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        }

    }
}
