/*  clsProgressLog.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2025 Richard Samphire MW0LGE

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at

mw0lge@grange-lane.co.uk
*/
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Thetis
{
    public static class LogTool
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        static IntPtr setWindowLongAuto(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (Common.Is64Bit) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return SetWindowLong32(hWnd, nIndex, dwNewLong);
        }

        const int GWLP_HWNDPARENT = -8;

        struct Entry
        {
            public string base_text;
            public ListViewItem item;
            public DateTime start_utc;
            public bool colour_warn;
            public bool completed;
            public long completed_ms;
        }

        static LogForm _form;
        static readonly object _sync = new object();
        static readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();
        static DateTime _total_start_utc;
        static int _seq = 1;
        static readonly string _reg_subkey = @"Software\OpenHPSDR\Thetis-x64";

        [DllImport("user32.dll")]
        static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        public static void ShowNewLog(IntPtr ownerHandle)
        {
            ensureForm();
            setOwner(ownerHandle);
            runOnUiThreadSync(delegate
            {
                _form.list.Items.Clear();
                lock (_sync) { _entries.Clear(); }
                _form.close_panel.Visible = false;
                _total_start_utc = DateTime.UtcNow;
                _form.time_label.Text = "Completed in 0.0s";
                Point loc;
                if (tryReadLocation(out loc))
                {
                    _form.StartPosition = FormStartPosition.Manual;
                    _form.Location = loc;
                    Common.ForceFormOnScreen(_form, false, false);
                }
                bool ok = readRegistryShow(out bool show);
                if (ok && show)
                {
                    _form.Show();
                    _form.BringToFront();
                }
                else
                {
                    _form.Hide();
                }
            });
        }

        public static void ShowLog(IntPtr ownerHandle)
        {
            ensureForm();
            setOwner(ownerHandle);
            runOnUiThread(delegate
            {
                _form.Show();
                _form.BringToFront();
            });
        }

        private static void setOwner(IntPtr ownerHandle)
        {
            runOnUiThreadSync(delegate
            {
                if (_form.IsHandleCreated) setWindowLongAuto(_form.Handle, GWLP_HWNDPARENT, ownerHandle);
            });
        }

        public static string AddLogEntry(string text)
        {
            return AddLogEntry(text, true);
        }

        public static string AddLogEntry(string text, bool colour_warn)
        {
            ensureForm();
            string id;
            lock (_sync)
            {
                id = _seq.ToString();
                _seq++;
            }
            addCore(text, id, colour_warn);
            return id;
        }

        public static bool AddLogEntry(string text, string id, bool colour_warn = true)
        {
            ensureForm();
            if (string.IsNullOrEmpty(id)) return false;
            lock (_sync)
            {
                if (_entries.ContainsKey(id)) return false;
            }
            addCore(text, id, colour_warn);
            return true;
        }

        public static void Shutdown()
        {
            if (_form == null || _form.IsDisposed) return;
            runOnUiThread(delegate
            {
                writeLocation(_form.Location);
                _form.Close();
                _form.Dispose();
                _form = null;
            });
        }

        public static void Completed(string id)
        {
            ensureForm();
            Entry entry;
            bool has;
            lock (_sync)
            {
                has = _entries.TryGetValue(id, out entry);
                if (has)
                {
                    TimeSpan diff = DateTime.UtcNow - entry.start_utc;
                    long ms = diff.TotalMilliseconds > 0 ? (long)diff.TotalMilliseconds : 0;
                    entry.completed = true;
                    entry.completed_ms = ms;
                    _entries[id] = entry;
                }
            }
            if (!has) return;
            runOnUiThread(delegate
            {
                Entry e;
                if (!_entries.TryGetValue(id, out e)) return;
                if (e.item != null)
                {
                    e.item.Text = e.base_text + " " + e.completed_ms.ToString() + "ms";
                    if (e.colour_warn)
                    {
                        if (e.completed_ms > 8000) e.item.ForeColor = Color.Red;
                        else if (e.completed_ms > 2000) e.item.ForeColor = Color.Orange;
                        else e.item.ForeColor = Color.Lime;
                    }
                    else
                    {
                        e.item.ForeColor = Color.Lime;
                    }
                }
                double secs = (DateTime.UtcNow - _total_start_utc).TotalSeconds;
                _form.time_label.Text = "Completed in " + secs.ToString("0.0") + "s";
            });
        }

        public static void Finish()
        {
            ensureForm();
            runOnUiThread(delegate
            {
                double secs = (DateTime.UtcNow - _total_start_utc).TotalSeconds;
                _form.time_label.Text = "Completed in " + secs.ToString("0.0") + "s";
                _form.close_panel.Visible = true;
            });
        }

        public static void HideAndSave()
        {
            ensureForm();
            runOnUiThread(delegate
            {
                writeLocation(_form.Location);
                _form.Hide();
            });
        }

        public static void SetRegistryToShow(bool show)
        {
            writeRegistryShow(show);
        }
        public static bool GetRegistryToShow(out bool show)
        {
            return readRegistryShow(out show);
        }

        static void addCore(string text, string id, bool colour_warn)
        {
            DateTime start = DateTime.UtcNow;
            lock (_sync)
            {
                Entry e0 = new Entry();
                e0.base_text = text;
                e0.item = null;
                e0.start_utc = start;
                e0.colour_warn = colour_warn;
                e0.completed = false;
                e0.completed_ms = 0;
                _entries[id] = e0;
            }
            runOnUiThread(delegate
            {
                Entry e;
                if (!_entries.TryGetValue(id, out e)) return;
                ListViewItem lvi = new ListViewItem("");
                _form.list.Items.Add(lvi);
                e.item = lvi;
                if (e.completed)
                {
                    lvi.Text = e.base_text + " " + e.completed_ms.ToString() + "ms";
                    if (e.colour_warn)
                    {
                        if (e.completed_ms > 4000) lvi.ForeColor = Color.Red;
                        else if (e.completed_ms > 2000) lvi.ForeColor = Color.Orange;
                        else lvi.ForeColor = Color.Lime;
                    }
                    else
                    {
                        lvi.ForeColor = Color.Lime;
                    }
                }
                else
                {
                    lvi.Text = e.base_text;
                    lvi.ForeColor = Color.Lime;
                }
                lock (_sync) { _entries[id] = e; }
                if (_form.list.Items.Count > 0) _form.list.EnsureVisible(_form.list.Items.Count - 1);
                hideHorizontalScrollBar(_form.list);
            });
        }

        static void ensureForm()
        {
            if (_form != null && !_form.IsDisposed) return;
            ManualResetEvent created = new ManualResetEvent(false);
            Exception init_ex = null;
            Thread t = new Thread(new ThreadStart(() =>
            {
                try
                {
                    _form = new LogForm();
                    _form.CreateControl();
                    IntPtr h = _form.Handle;
                    created.Set();
                    Application.Run();
                }
                catch (Exception ex)
                {
                    init_ex = ex;
                    created.Set();
                }
            }));
            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            created.WaitOne();
            if (init_ex != null) throw init_ex;
        }

        static void runOnUiThreadSync(MethodInvoker a)
        {
            if (_form == null || _form.IsDisposed) return;
            if (!_form.IsHandleCreated) return;
            if (_form.InvokeRequired) _form.Invoke(a);
            else a();
        }
        static void runOnUiThread(MethodInvoker a)
        {
            if (_form == null || _form.IsDisposed) return;
            if (!_form.IsHandleCreated) return;
            _form.BeginInvoke(a);
        }

        static bool readRegistryShow(out bool show)
        {
            bool ok = false;
            show = false;

            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(_reg_subkey, false);
                if (key != null)
                {
                    object o = key.GetValue("ShowLog");
                    key.Close();
                    if (o != null)
                    {
                        if (o is int)
                        {
                            show = (int)o == 1;
                            ok = true;
                        }
                        else if (o is string)
                        {
                            show = string.Equals((string)o, "1", StringComparison.OrdinalIgnoreCase);
                            ok = true;
                        }
                    }
                    else
                    {
                        // if key is not there, then we are happy, but do not show
                        show = false;
                        ok = true;
                    }
                }
            }
            catch { };

            return ok;
        }

        static void writeRegistryShow(bool show)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(_reg_subkey);
            key.SetValue("ShowLog", show ? 1 : 0, RegistryValueKind.DWord);
            key.Close();
        }

        static bool tryReadLocation(out Point p)
        {
            p = new Point(0, 0);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(_reg_subkey, false);
            if (key == null) return false;
            object lx = key.GetValue("LogLeft");
            object ly = key.GetValue("LogTop");
            key.Close();
            if (lx == null || ly == null) return false;
            int left;
            int top;
            if (!int.TryParse(lx.ToString(), out left)) return false;
            if (!int.TryParse(ly.ToString(), out top)) return false;
            p = new Point(left, top);
            return true;
        }

        static void writeLocation(Point p)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(_reg_subkey);
            key.SetValue("LogLeft", p.X, RegistryValueKind.DWord);
            key.SetValue("LogTop", p.Y, RegistryValueKind.DWord);
            key.Close();
        }

        static void hideHorizontalScrollBar(ListView lv)
        {
            if (lv.IsHandleCreated) ShowScrollBar(lv.Handle, 0, false);
        }

        sealed class LogForm : Form
        {
            sealed class NoSelectListView : ListView
            {
                [DllImport("user32.dll")]
                static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

                const int WM_UPDATEUISTATE = 0x0128;
                const int UIS_SET = 1;
                const int UISF_HIDEFOCUS = 0x1;

                protected override void OnCreateControl()
                {
                    base.OnCreateControl();
                    if (IsHandleCreated) SendMessage(Handle, WM_UPDATEUISTATE, (IntPtr)(UIS_SET | (UISF_HIDEFOCUS << 16)), IntPtr.Zero);
                }

                protected override void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
                {
                    if (e.IsSelected) e.Item.Selected = false;
                    base.OnItemSelectionChanged(e);
                }

                protected override void OnGotFocus(EventArgs e)
                {
                    base.OnGotFocus(e);
                    if (IsHandleCreated) SendMessage(Handle, WM_UPDATEUISTATE, (IntPtr)(UIS_SET | (UISF_HIDEFOCUS << 16)), IntPtr.Zero);
                    if (SelectedIndices.Count != 0) SelectedIndices.Clear();
                    if (FocusedItem != null) FocusedItem.Focused = false;
                }

                protected override void OnMouseDown(MouseEventArgs e)
                {
                    base.OnMouseDown(e);
                    if (SelectedIndices.Count != 0) SelectedIndices.Clear();
                    if (FocusedItem != null) FocusedItem.Focused = false;
                }

                protected override void OnKeyDown(KeyEventArgs e)
                {
                    e.Handled = true;
                    base.OnKeyDown(e);
                }
            }


            public readonly ListView list;
            public readonly Panel close_panel;
            public readonly Label time_label;
            readonly Button close_button;

            public LogForm()
            {
                Text = "Thetis Startup Log";
                StartPosition = FormStartPosition.Manual;
                Size = new Size(400, 680);
                FormBorderStyle = FormBorderStyle.FixedToolWindow;
                ShowInTaskbar = false;
                BackColor = Color.Black;

                list = new NoSelectListView();
                list.View = View.Details;
                list.HeaderStyle = ColumnHeaderStyle.None;
                list.Columns.Add("", -2, HorizontalAlignment.Left);
                list.Dock = DockStyle.Fill;
                list.FullRowSelect = true;
                list.BackColor = Color.Black;
                list.ForeColor = Color.Lime;
                list.Font = new Font("Courier New", 10f, FontStyle.Regular);
                list.BorderStyle = BorderStyle.None;
                list.HandleCreated += delegate { hideHorizontalScrollBar(list); };
                list.SizeChanged += delegate { hideHorizontalScrollBar(list); };
                list.MultiSelect = false;
                list.HideSelection = true;

                close_panel = new Panel();
                close_panel.Dock = DockStyle.Bottom;
                close_panel.Height = 80;
                close_panel.BackColor = Color.Black;
                close_panel.Visible = false;

                time_label = new Label();
                time_label.AutoSize = true;
                time_label.Text = "Completed in 0.0s";
                time_label.ForeColor = Color.Lime;
                time_label.BackColor = Color.Black;
                time_label.Font = new Font("Courier New", 10f, FontStyle.Regular);
                time_label.Left = 6;
                time_label.Top = 6;

                close_button = new Button();
                close_button.Text = "Close";
                close_button.AutoSize = true;
                close_button.Anchor = AnchorStyles.None;
                close_button.FlatStyle = FlatStyle.System;
                close_button.Click += delegate { LogTool.HideAndSave(); };

                close_panel.Controls.Add(time_label);
                close_panel.Controls.Add(close_button);
                Controls.Add(list);
                Controls.Add(close_panel);

                Resize += delegate { layoutColumns(); centerClose(); };
                Shown += delegate { layoutColumns(); centerClose(); };
                close_panel.Resize += delegate { centerClose(); };

                Common.DoubleBufferAll(this, true);

                Common.UseImmersiveDarkMode(this.Handle, true);
            }

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                e.Cancel = true;
                LogTool.HideAndSave();
            }

            void layoutColumns()
            {
                if (list.Columns.Count == 0) return;
                int w = list.ClientSize.Width - 4;
                if (w < 50) w = 50;
                list.Columns[0].Width = w;
                if (list.IsHandleCreated) ShowScrollBar(list.Handle, 0, false);
            }

            void centerClose()
            {
                Control host = close_panel;
                int x = (host.ClientSize.Width - close_button.Width) / 2;
                int y = host.ClientSize.Height - close_button.Height - 8;
                if (x < 0) x = 0;
                if (y < 0) y = 0;
                close_button.Left = x;
                close_button.Top = y;
            }
        }
    }
}
