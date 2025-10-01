/*  clsDPISafeTools.cs

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
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Thetis
{
    internal static class SafeScreens
    {
        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        [DllImport("Shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetMonitorInfo")]
        private static extern bool GetMonitorInfoEx(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);


        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public override string ToString()
            {
                return "{" + $"X={left},Y={top},Width={right - left},Height={bottom - top}" + "}";
            }
        }

        private enum MONITOR_DPI_TYPE
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFOEX
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        private struct monitor_info
        {
            public Rectangle rect_monitor;
            public Rectangle rect_work;
            public int scale_percent;
            public int index;
            public int display_number;
        }

        public static (Rectangle adjusted, bool resized, bool repositioned) EnsureRectangleWithinNearestScreen(Rectangle? rect = null, Form form = null, bool keep_on_screen = false, bool use_working_area = false)
        {
            // keep_on_screen - if true, will try to keep the window on the screen containing the mouse cursor
            // use_working_area - if true, will use the working area of the screen(s) (i.e. excluding taskbar and docked windows and docked toolbars)

            if (rect == null && form == null) return (Rectangle.Empty, false, false);

            if (form != null && form.WindowState == FormWindowState.Maximized)
            {
                Rectangle ext;
                if (tryGetExtendedFrameBounds(form, out ext)) return (ext, false, false);
                Rectangle sr = use_working_area ? Screen.FromHandle(form.Handle).WorkingArea : Screen.FromHandle(form.Handle).Bounds;
                return (sr, false, false);
            }

            List<Rectangle> monitors = getMonitorRects(use_working_area);
            if (monitors.Count == 0)
            {
                for (int i = 0; i < Screen.AllScreens.Length; i++) monitors.Add(use_working_area ? Screen.AllScreens[i].WorkingArea : Screen.AllScreens[i].Bounds);
            }

            Rectangle source;
            if (form != null)
            {
                source = form.Bounds;
            }
            else
            {
                source = (Rectangle)rect;
            }

            int margin_left = 0;
            int margin_top = 0;
            int margin_right = 0;
            int margin_bottom = 0;
            if (form != null) getDwmShadowMargins(form, out margin_left, out margin_top, out margin_right, out margin_bottom);

            bool fully_on = isFullyOnMonitors(source, monitors);
            bool contained = isContainedByAnyScreen(source, monitors);
            bool need_adjust = !fully_on || (keep_on_screen && !contained);

            Rectangle target = chooseTargetMonitor(source, monitors, keep_on_screen);

            bool resized = false;
            bool repositioned = false;
            Rectangle adjusted = source;

            if (need_adjust)
            {
                int nx = source.X;
                int ny = source.Y;
                int nw = source.Width;
                int nh = source.Height;

                int l = target.Left - margin_left;
                int t = target.Top - margin_top;
                int r = target.Right + margin_right;
                int b = target.Bottom + margin_bottom;

                int max_w = Math.Max(1, r - l);
                int max_h = Math.Max(1, b - t);

                if (nw > max_w) { nw = max_w; resized = true; }
                if (nh > max_h) { nh = max_h; resized = true; }

                if (nx < l) nx = l;
                if (ny < t) ny = t;
                if (nx + nw > r) nx = r - nw;
                if (ny + nh > b) ny = b - nh;

                if (nx != source.X || ny != source.Y) repositioned = true;

                adjusted = new Rectangle(nx, ny, nw, nh);
            }

            if (form != null && need_adjust)
            {
                if (form.WindowState != FormWindowState.Normal) form.WindowState = FormWindowState.Normal;
                form.SetBounds(adjusted.X, adjusted.Y, adjusted.Width, adjusted.Height);
            }

            return (adjusted, resized, repositioned);
        }

        private static List<Rectangle> getMonitorRects(bool use_working_area)
        {
            List<Rectangle> list = new List<Rectangle>();
            for (int i = 0; i < Screen.AllScreens.Length; i++) list.Add(use_working_area ? Screen.AllScreens[i].WorkingArea : Screen.AllScreens[i].Bounds);
            return list;
        }

        private static Rectangle chooseTargetMonitor(Rectangle r, List<Rectangle> monitors, bool keep_on_screen)
        {
            if (keep_on_screen)
            {
                Point p = Cursor.Position;
                for (int i = 0; i < monitors.Count; i++)
                {
                    if (monitors[i].Contains(p)) return monitors[i];
                }
            }
            int best_i = -1;
            long best_area = -1;
            for (int i = 0; i < monitors.Count; i++)
            {
                Rectangle inter = Rectangle.Intersect(r, monitors[i]);
                long area = inter.Width > 0 && inter.Height > 0 ? (long)inter.Width * (long)inter.Height : 0;
                if (area > best_area)
                {
                    best_area = area;
                    best_i = i;
                }
            }
            if (best_i >= 0) return monitors[best_i];
            long best_dist2 = long.MaxValue;
            int best_j = 0;
            for (int j = 0; j < monitors.Count; j++)
            {
                Rectangle m = monitors[j];
                int cx = clamp(r.X + r.Width / 2, m.Left, m.Right);
                int cy = clamp(r.Y + r.Height / 2, m.Top, m.Bottom);
                long dx = (long)(r.X + r.Width / 2) - (long)cx;
                long dy = (long)(r.Y + r.Height / 2) - (long)cy;
                long d2 = dx * dx + dy * dy;
                if (d2 < best_dist2)
                {
                    best_dist2 = d2;
                    best_j = j;
                }
            }
            return monitors[best_j];
        }

        private static bool isFullyOnMonitors(Rectangle r, List<Rectangle> monitors)
        {
            long src = (long)r.Width * (long)r.Height;
            long covered = 0;
            for (int i = 0; i < monitors.Count; i++)
            {
                Rectangle inter = Rectangle.Intersect(r, monitors[i]);
                if (inter.Width > 0 && inter.Height > 0) covered += (long)inter.Width * (long)inter.Height;
            }
            return covered >= src;
        }

        private static bool isContainedByAnyScreen(Rectangle r, List<Rectangle> monitors)
        {
            for (int i = 0; i < monitors.Count; i++)
            {
                if (monitors[i].Contains(r)) return true;
            }
            return false;
        }

        private static Rectangle getUnion(List<Rectangle> rects)
        {
            if (rects.Count == 0) return new Rectangle(0, 0, 1, 1);
            Rectangle u = rects[0];
            for (int i = 1; i < rects.Count; i++)
            {
                Rectangle b = rects[i];
                int left = Math.Min(u.Left, b.Left);
                int top = Math.Min(u.Top, b.Top);
                int right = Math.Max(u.Right, b.Right);
                int bottom = Math.Max(u.Bottom, b.Bottom);
                u = Rectangle.FromLTRB(left, top, right, bottom);
            }
            return u;
        }

        private static int clamp(int v, int a, int b)
        {
            if (v < a) return a;
            if (v > b) return b;
            return v;
        }

        private static Color colorFromHue(double hue_deg, double saturation, double value)
        {
            double c = value * saturation;
            double x = c * (1.0 - Math.Abs((hue_deg / 60.0) % 2.0 - 1.0));
            double m = value - c;
            double r1 = 0.0;
            double g1 = 0.0;
            double b1 = 0.0;
            if (hue_deg < 60.0) { r1 = c; g1 = x; b1 = 0.0; }
            else if (hue_deg < 120.0) { r1 = x; g1 = c; b1 = 0.0; }
            else if (hue_deg < 180.0) { r1 = 0.0; g1 = c; b1 = x; }
            else if (hue_deg < 240.0) { r1 = 0.0; g1 = x; b1 = c; }
            else if (hue_deg < 300.0) { r1 = x; g1 = 0.0; b1 = c; }
            else { r1 = c; g1 = 0.0; b1 = x; }
            int r = (int)Math.Round((r1 + m) * 255.0);
            int g = (int)Math.Round((g1 + m) * 255.0);
            int b = (int)Math.Round((b1 + m) * 255.0);
            if (r < 0) r = 0; if (r > 255) r = 255;
            if (g < 0) g = 0; if (g > 255) g = 255;
            if (b < 0) b = 0; if (b > 255) b = 255;
            return Color.FromArgb(r, g, b);
        }

        private static bool tryGetExtendedFrameBounds(Form form, out Rectangle rect)
        {
            rect = Rectangle.Empty;
            if (form == null) return false;
            RECT r;
            int hr = DwmGetWindowAttribute(form.Handle, DWMWA_EXTENDED_FRAME_BOUNDS, out r, Marshal.SizeOf(typeof(RECT)));
            if (hr != 0) return false;
            rect = Rectangle.FromLTRB(r.left, r.top, r.right, r.bottom);
            return true;
        }

        private static void getDwmShadowMargins(Form form, out int left, out int top, out int right, out int bottom)
        {
            left = 0;
            top = 0;
            right = 0;
            bottom = 0;
            if (form == null) return;

            Rectangle ext;
            if (!tryGetExtendedFrameBounds(form, out ext)) return;

            RECT wr;
            if (!tryGetWindowRectPhysical(form.Handle, out wr))
            {
                if (!GetWindowRect(form.Handle, out wr)) return;
            }

            Rectangle win_phys = Rectangle.FromLTRB(wr.left, wr.top, wr.right, wr.bottom);

            int ml_ph = ext.Left - win_phys.Left;
            int mt_ph = ext.Top - win_phys.Top;
            int mr_ph = win_phys.Right - ext.Right;
            int mb_ph = win_phys.Bottom - ext.Bottom;

            double scale = 1.0;
            int src_w = form.Bounds.Width;
            if (src_w > 0) scale = (double)win_phys.Width / (double)src_w;
            if (scale <= 0.01) scale = 1.0;

            left = (int)Math.Round(ml_ph / scale);
            top = (int)Math.Round(mt_ph / scale);
            right = (int)Math.Round(mr_ph / scale);
            bottom = (int)Math.Round(mb_ph / scale);

            if (left < 0) left = 0;
            if (top < 0) top = 0;
            if (right < 0) right = 0;
            if (bottom < 0) bottom = 0;
        }

        private static bool tryGetWindowRectPhysical(IntPtr hwnd, out RECT rect)
        {
            rect = new RECT();
            IntPtr prev = IntPtr.Zero;
            bool changed = false;
            try
            {
                prev = SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                changed = prev != IntPtr.Zero;
            }
            catch
            {
                changed = false;
            }

            bool ok = GetWindowRect(hwnd, out rect);

            if (changed)
            {
                try { SetThreadDpiAwarenessContext(prev); } catch { }
            }

            return ok;
        }

        public static Bitmap createScreensBitmap(Size target_size, IEnumerable<Form> forms, bool use_working_area = false)
        {
            if (target_size.Width <= 0 || target_size.Height <= 0) return new Bitmap(1, 1);

            List<monitor_info> mons = getMonitorInfosPhysical(use_working_area);
            if (mons.Count == 0)
            {
                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    monitor_info info = new monitor_info();
                    Rectangle rb = Screen.AllScreens[i].Bounds;
                    Rectangle rw = Screen.AllScreens[i].WorkingArea;
                    info.rect_monitor = rb;
                    info.rect_work = rw;
                    info.scale_percent = 100;
                    info.index = i + 1;
                    info.display_number = info.index;
                    mons.Add(info);
                }
            }

            List<Rectangle> rects = new List<Rectangle>();
            for (int i = 0; i < mons.Count; i++) rects.Add(mons[i].rect_monitor);
            Rectangle union = getUnion(rects);

            float sx = (float)(target_size.Width - 1) / (float)union.Width;
            float sy = (float)(target_size.Height - 1) / (float)union.Height;
            float scale = sx < sy ? sx : sy;

            int cw = (int)Math.Round(union.Width * scale);
            int ch = (int)Math.Round(union.Height * scale);
            int ox = (target_size.Width - cw) / 2;
            int oy = (target_size.Height - ch) / 2;

            Bitmap bmp = new Bitmap(target_size.Width, target_size.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            int n = mons.Count;
            for (int i = 0; i < n; i++)
            {
                Rectangle rect_mon = mons[i].rect_monitor;
                int rx = ox + (int)Math.Round((rect_mon.Left - union.Left) * scale);
                int ry = oy + (int)Math.Round((rect_mon.Top - union.Top) * scale);
                int rw = Math.Max(1, (int)Math.Round(rect_mon.Width * scale));
                int rh = Math.Max(1, (int)Math.Round(rect_mon.Height * scale));
                Rectangle r = new Rectangle(rx, ry, rw, rh);

                Color fill = colorFromHue((i * 360.0 / Math.Max(1, n)) % 360.0, 0.55, 0.95);
                using (SolidBrush brush = new SolidBrush(fill))
                using (Pen pen = new Pen(Color.Black, 1f))
                {
                    pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                    g.FillRectangle(brush, r);
                    g.DrawRectangle(pen, r);
                }

                if (use_working_area)
                {
                    Rectangle work_mon = mons[i].rect_work;
                    int wx = ox + (int)Math.Round((work_mon.Left - union.Left) * scale);
                    int wy = oy + (int)Math.Round((work_mon.Top - union.Top) * scale);
                    int ww = Math.Max(1, (int)Math.Round(work_mon.Width * scale));
                    int wh = Math.Max(1, (int)Math.Round(work_mon.Height * scale));
                    using (Pen pen_work = new Pen(Color.Black, 1f))
                    {
                        pen_work.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        if (work_mon.Left > rect_mon.Left) g.DrawLine(pen_work, wx, wy, wx, wy + wh);
                        if (work_mon.Top > rect_mon.Top) g.DrawLine(pen_work, wx, wy, wx + ww, wy);
                        if (work_mon.Right < rect_mon.Right) g.DrawLine(pen_work, wx + ww, wy, wx + ww, wy + wh);
                        if (work_mon.Bottom < rect_mon.Bottom) g.DrawLine(pen_work, wx, wy + wh, wx + ww, wy + wh);
                    }
                }

                string label_num = (mons[i].display_number > 0 ? mons[i].display_number : mons[i].index).ToString();
                string label_scale = mons[i].scale_percent.ToString() + "%";
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Near;
                    float cx = r.Left + r.Width / 2f;
                    float cy = r.Top + r.Height / 2f;
                    using (Font font_big = new Font(SystemFonts.DefaultFont.FontFamily, Math.Max(8f, r.Height * 0.2f), FontStyle.Bold, GraphicsUnit.Pixel))
                    using (Font font_small = new Font(SystemFonts.DefaultFont.FontFamily, Math.Max(6f, r.Height * 0.1f), FontStyle.Regular, GraphicsUnit.Pixel))
                    {
                        SizeF size_num = g.MeasureString(label_num, font_big);
                        SizeF size_scale = g.MeasureString(label_scale, font_small);
                        float y0 = cy - (size_num.Height + size_scale.Height) / 2f;
                        RectangleF rect_num = new RectangleF(cx - size_num.Width / 2f, y0, size_num.Width, size_num.Height);
                        RectangleF rect_scale = new RectangleF(cx - size_scale.Width / 2f, y0 + size_num.Height, size_scale.Width, size_scale.Height);
                        using (SolidBrush text_brush = new SolidBrush(Color.Black))
                        {
                            g.DrawString(label_num, font_big, text_brush, rect_num, sf);
                            g.DrawString(label_scale, font_small, text_brush, rect_scale, sf);
                        }
                    }
                }
            }

            if (forms != null)
            {
                foreach (Form f in forms)
                {
                    if (f == null) continue;
                    Rectangle ext;
                    if (!tryGetExtendedFrameBounds(f, out ext))
                    {
                        RECT wr;
                        if (tryGetWindowRectPhysical(f.Handle, out wr)) ext = Rectangle.FromLTRB(wr.left, wr.top, wr.right, wr.bottom);
                        else ext = f.Bounds;
                    }

                    int fx = ox + (int)Math.Round((ext.Left - union.Left) * scale);
                    int fy = oy + (int)Math.Round((ext.Top - union.Top) * scale);
                    int fw = Math.Max(1, (int)Math.Round(ext.Width * scale));
                    int fh = Math.Max(1, (int)Math.Round(ext.Height * scale));
                    Rectangle fr = new Rectangle(fx, fy, fw, fh);

                    using (SolidBrush brush_overlay = new SolidBrush(Color.FromArgb(128, 0, 200, 0)))
                    using (Pen pen_overlay = new Pen(Color.Black, 2f))
                    {
                        pen_overlay.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                        g.FillRectangle(brush_overlay, fr);
                        g.DrawRectangle(pen_overlay, fr);
                    }
                }
            }

            g.Dispose();
            return bmp;
        }

        public static void RenderScreensToPictureBox(PictureBox picture_box, IEnumerable<Form> forms = null, bool use_working_area = false)
        {
            if (picture_box == null) return;
            Size target = picture_box.ClientSize;
            Bitmap bmp = createScreensBitmap(target, forms, use_working_area);
            Image old = picture_box.Image;
            picture_box.Image = bmp;
            if (old != null) old.Dispose();
        }
        public static void RenderScreensToPictureBox(PictureBox picture_box, Form form = null, bool use_working_area = false)
        {
            if (picture_box == null) return;
            if (form == null)
            {
                RenderScreensToPictureBox(picture_box, (IEnumerable<Form>)null, use_working_area);
            }
            else
            {
                List<Form> list = new List<Form>();
                list.Add(form);
                RenderScreensToPictureBox(picture_box, list, use_working_area);
            }
        }

        private static List<monitor_info> getMonitorInfosPhysical(bool use_working_area)
        {
            List<monitor_info> list = new List<monitor_info>();
            IntPtr prev = IntPtr.Zero;
            bool changed = false;
            try
            {
                prev = SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                if (prev != IntPtr.Zero) changed = true;
            }
            catch
            {
                changed = false;
            }

            try
            {
                int idx = 1;
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMon, IntPtr hdc, IntPtr lprc, IntPtr data)
                {
                    MONITORINFOEX mi = new MONITORINFOEX();
                    mi.cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFOEX));
                    if (GetMonitorInfoEx(hMon, ref mi))
                    {
                        Rectangle bmon = Rectangle.FromLTRB(mi.rcMonitor.left, mi.rcMonitor.top, mi.rcMonitor.right, mi.rcMonitor.bottom);
                        Rectangle bwrk = Rectangle.FromLTRB(mi.rcWork.left, mi.rcWork.top, mi.rcWork.right, mi.rcWork.bottom);

                        uint dpi_x = 96;
                        uint dpi_y = 96;
                        int percent = 100;
                        try
                        {
                            if (GetDpiForMonitor(hMon, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpi_x, out dpi_y) == 0)
                            {
                                double sc = (double)dpi_x / 96.0;
                                percent = (int)Math.Round(sc * 100.0);
                            }
                        }
                        catch
                        {
                            percent = 100;
                        }

                        string dev = mi.szDevice == null ? "" : mi.szDevice.TrimEnd('\0');
                        int disp_num = parseDisplayNumber(dev);
                        if (disp_num <= 0) disp_num = idx;

                        monitor_info info = new monitor_info();
                        info.rect_monitor = bmon;
                        info.rect_work = bwrk;
                        info.scale_percent = percent;
                        info.index = idx;
                        info.display_number = disp_num;
                        list.Add(info);
                        idx++;
                    }
                    return true;
                }, IntPtr.Zero);
            }
            finally
            {
                if (changed)
                {
                    try { SetThreadDpiAwarenessContext(prev); } catch { }
                }
            }

            return list;
        }

        private static int parseDisplayNumber(string device)
        {
            if (string.IsNullOrEmpty(device)) return -1;
            int i = device.Length - 1;
            while (i >= 0 && char.IsDigit(device[i])) i--;
            if (i == device.Length - 1) return -1;
            string digits = device.Substring(i + 1);
            int n;
            if (int.TryParse(digits, out n)) return n;
            return -1;
        }
    }
}
