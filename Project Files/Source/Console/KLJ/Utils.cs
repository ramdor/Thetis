using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Thetis.KLJ
{

    public static class GenericToStringExts
    {
        public static string ToStringExt<K, V>(this Dictionary<K, V> list) => "[" + string.Join(",\r\n ", list) + "]";
        public static string ToStringExt<Control>(this IEnumerable<Control> list) => "[" + string.Join(",\r\n ", list) + "]";
    }

    public static class GenericEnumerable
    {
        public static IEnumerable<Control> GetAllControls(Control container)
        {
            List<Control> controlList = new List<Control>();
            foreach (Control c in container.Controls)
            {
                controlList.AddRange(GetAllControls(c));
                controlList.Add(c);
            }
            return controlList;
        }

        public static IEnumerable<Control> GetAllControlsOfType(Control control, Type type, int depth = -1, int mydepth = -1)
        {
            List<Control> controlList = new List<Control>();
            foreach (Control c in control.Controls)
            {
                if (c.GetType() == type)
                {
                    if (depth == -1)
                    {
                        controlList.AddRange(GetAllControlsOfType(c, type));
                    }
                    else
                    {
                        if (depth > 0)
                        {
                            mydepth--;
                        }
                        if (mydepth <= 0)
                        {
                            return controlList;
                        }
                        else
                        {
                            controlList.AddRange(GetAllControlsOfType(c, type));
                        }
                    }
                    controlList.Add(c);
                }
            }
            return controlList;
        }
    }



    internal class Utils
    {


        internal static void TreeViewFromTabControl(TreeView tv, TabControl tc)
        {
            /*/

           List<Control> controlList = new List<Control>();
           tv.Nodes.Clear();
           List<Control> tabChildren = new List<Control>();

           var colPages = GenericEnumerable.GetAllControlsOfType(tc, typeof(TabPage));
           // col contains any tabControl inside tc, though it may be some levels deep.
           foreach (Control pagein colPages)
           {
               if (tabc.Parent == tc)
               {
                   // only top-level here
                   controlList.AddRange(GenericEnumerable.GetAllControlsOfType(tabc, typeof(TabControl), 1));
                   controlList.Add(tabc);
               }
               else
               {
                   tabChildren.Add(tabc);
               }
           }

           foreach (Control tabPage in col)
           {
               var displayName = GetDisplayName(tabPage.Name, "tp");
               var topLevel = tv.Nodes.Add(tabPage.Name, displayName);
               var subTabControls = GenericEnumerable.GetAllControlsOfType((Control)tabPage, typeof(TabControl));

               foreach (Control tabControl in subTabControls)
               {
                   displayName = GetDisplayName(tabControl.Name, "tc");
                   var top = topLevel.Nodes.Add(tabControl.Name, displayName);
                   var tabPageControls = GenericEnumerable.GetAllControls(tabControl);
                   foreach (Control ctl in tabPageControls)
                   {
                       //if (ctl.Visible)
                       {
                           displayName = ctl.Name.Count() > 0 ? ctl.Name : ctl.Text;
                           top.Nodes.Add(ctl.Name, ctl.Text);
                       }
                   }
               }
               if (subTabControls.Count() > 0)
               {
                   PrintContainer(subTabControls);
               }



       PrintContainer(controlList);
           foreach (Control tp in controlList)
           {

           }

           foreach (Control tp in controlList) // for each TabControl inside of the list, 
           {
               var list = GenericEnumerable.GetAllControlsOfType(tp, typeof(TabControl)); // look for embedded tabcontrols inside tabpages
       PrintContainer(list);
       Debug.Print("\n");
           }

   PrintContainer(controlList);
   Debug.Print("\n");
               }
           /*/

        }

        static string GetDisplayName(string name, string chop)
        {
            if (string.IsNullOrEmpty(name))
                return "";
            if (name.StartsWith(chop))
                name = name.Substring(2);
            else
                Debug.Assert(false); // were you expecting this? chop is not found in na,e

            return name;
        }

        static void PrintContainer<T>(IEnumerable<T> items)
        {
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                Debug.Write("{0}\t", prop.Name);
            }
            Debug.WriteLine("");

            foreach (var item in items)
            {
                foreach (var prop in props)
                {
                    var s = "";
                    var it = prop.GetValue(item, null);
                    if (it != null)
                        s = it.ToString();
                    if (!string.IsNullOrEmpty(s))
                        Debug.Write("{0}\t", s);
                }
                Debug.WriteLine("");
            }
        }

        internal static void OpenInExplorer(string path)
        {
            using (Process.Start(path)) { }
        }

        internal static void FadeIn(Form f, int granularity = 30)
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

            [DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

            [DllImport("user32.dll", CharSet = CharSet.None, SetLastError = true)]
            static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        }

    }
}
