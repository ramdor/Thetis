using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Thetis
{
    public partial class ucBandwidthView : UserControl
    {
        public enum BandwidthUnits
        {
            KBps,
            Mbitps
        }

        private BandwidthUnits _display_units;
        private bool _show_grid;
        private int _history_seconds;

        private double[] _in_bps;
        private double[] _out_bps;
        private double[] _tot_bps;

        private int _head;
        private int _count;

        private double _scale_max_display;
        private int _scale_hold_ticks;

        private double _last_in_bps;
        private double _last_out_bps;
        private double _last_tot_bps;

        private bool _enable_smoothing;
        private double _smoothing_factor;

        private double _smooth_in;
        private double _smooth_out;
        private double _smooth_tot;
        private bool _smoothing_ready;

        public BandwidthUnits DisplayUnits
        {
            get { return _display_units; }
            set
            {
                if (_display_units == value)
                    return;

                _display_units = value;

                _scale_max_display = 1.0;
                _scale_hold_ticks = 0;

                updateScale();
                Invalidate();
            }
        }

        public bool ShowGrid
        {
            get { return _show_grid; }
            set
            {
                if (_show_grid == value)
                    return;

                _show_grid = value;
                Invalidate();
            }
        }

        public int HistorySeconds
        {
            get { return _history_seconds; }
            set
            {
                int v = value;
                if (v < 10)
                    v = 10;
                if (v > 600)
                    v = 600;

                if (_history_seconds == v)
                    return;

                _history_seconds = v;
                resizeBuffers(_history_seconds);
                Invalidate();
            }
        }

        public bool EnableSmoothing
        {
            get { return _enable_smoothing; }
            set
            {
                if (_enable_smoothing == value)
                    return;

                _enable_smoothing = value;
                resetSmoothing();
                Invalidate();
            }
        }

        public double SmoothingFactor
        {
            get { return _smoothing_factor; }
            set
            {
                double v = value;
                if (v < 0.01)
                    v = 0.01;
                if (v > 1.0)
                    v = 1.0;

                if (Math.Abs(_smoothing_factor - v) < 0.0000001)
                    return;

                _smoothing_factor = v;
                resetSmoothing();
                Invalidate();
            }
        }

        public ucBandwidthView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Black;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);

            _display_units = BandwidthUnits.Mbitps;
            _history_seconds = 60;
            _show_grid = true;
            _enable_smoothing = false;
            _smoothing_factor = 0.5;

            resizeBuffers(_history_seconds);
            resetSmoothing();
        }

        public void Reset()
        {
            Array.Clear(_in_bps, 0, _in_bps.Length);
            Array.Clear(_out_bps, 0, _out_bps.Length);
            Array.Clear(_tot_bps, 0, _tot_bps.Length);

            _head = 0;
            _count = 0;

            _scale_max_display = 1.0;
            _scale_hold_ticks = 0;

            _last_in_bps = 0.0;
            _last_out_bps = 0.0;
            _last_tot_bps = 0.0;

            resetSmoothing();

            Invalidate();
        }

        public void PushSample(double inbound_bps, double outbound_bps)
        {
            if (inbound_bps < 0.0)
                inbound_bps = 0.0;
            if (outbound_bps < 0.0)
                outbound_bps = 0.0;

            double total_bps = inbound_bps + outbound_bps;

            if (_enable_smoothing)
            {
                if (!_smoothing_ready)
                {
                    _smooth_in = inbound_bps;
                    _smooth_out = outbound_bps;
                    _smooth_tot = total_bps;
                    _smoothing_ready = true;
                }
                else
                {
                    double a = _smoothing_factor;
                    _smooth_in = (_smooth_in * (1.0 - a)) + (inbound_bps * a);
                    _smooth_out = (_smooth_out * (1.0 - a)) + (outbound_bps * a);
                    _smooth_tot = (_smooth_tot * (1.0 - a)) + (total_bps * a);
                }

                inbound_bps = _smooth_in;
                outbound_bps = _smooth_out;
                total_bps = _smooth_tot;
            }

            _in_bps[_head] = inbound_bps;
            _out_bps[_head] = outbound_bps;
            _tot_bps[_head] = total_bps;

            _last_in_bps = inbound_bps;
            _last_out_bps = outbound_bps;
            _last_tot_bps = total_bps;

            _head++;
            if (_head >= _in_bps.Length)
                _head = 0;

            if (_count < _in_bps.Length)
                _count++;

            updateScale();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            Rectangle client = ClientRectangle;
            if (client.Width < 10 || client.Height < 10)
                return;

            int pad_left = 54;
            int pad_right = 10;
            int pad_top = 8;
            int pad_bottom = 18;

            Rectangle plot = new Rectangle(
                client.Left + pad_left,
                client.Top + pad_top,
                client.Width - pad_left - pad_right,
                client.Height - pad_top - pad_bottom);

            if (plot.Width <= 5 || plot.Height <= 5)
                return;

            double max_display = _scale_max_display;
            if (max_display < 0.000001)
                max_display = 1.0;

            if (_show_grid)
                drawGrid(g, plot);

            drawAxisLeft(g, plot, max_display);

            drawLine(g, plot, _in_bps, _count, _head, max_display, Color.DeepSkyBlue, 1.6f);
            drawLine(g, plot, _out_bps, _count, _head, max_display, Color.Orange, 1.6f);
            drawLine(g, plot, _tot_bps, _count, _head, max_display, Color.LimeGreen, 2.0f);

            drawOverlay(g, plot);
        }

        private void drawGrid(Graphics g, Rectangle plot)
        {
            using (Pen p = new Pen(Color.FromArgb(35, 255, 255, 255), 1f))
            {
                p.DashStyle = DashStyle.Solid;

                int vlines = 6;
                int hlines = 4;

                for (int i = 1; i < vlines; i++)
                {
                    float x = plot.Left + (plot.Width * i) / (float)vlines;
                    g.DrawLine(p, x, plot.Top, x, plot.Bottom);
                }

                for (int i = 1; i < hlines; i++)
                {
                    float y = plot.Top + (plot.Height * i) / (float)hlines;
                    g.DrawLine(p, plot.Left, y, plot.Right, y);
                }
            }

            using (Pen border = new Pen(Color.FromArgb(90, 255, 255, 255), 1f))
                g.DrawRectangle(border, plot);
        }

        private void drawAxisLeft(Graphics g, Rectangle plot, double max_display)
        {
            using (Brush b = new SolidBrush(ForeColor))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Far;
                sf.LineAlignment = StringAlignment.Center;

                int ticks = 4;
                for (int i = 0; i <= ticks; i++)
                {
                    double t = max_display * (ticks - i) / (double)ticks;
                    string label = formatAxisValue(t);

                    float y = plot.Top + (plot.Height * i) / (float)ticks;
                    RectangleF r = new RectangleF(0, y - 10, plot.Left - 6, 20);
                    g.DrawString(label, Font, b, r, sf);
                }

                using (Brush ub = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                {
                    string unit = _display_units == BandwidthUnits.KBps ? "kB/s" : "Mbit/s";

                    GraphicsState state = g.Save();

                    float cx = 12f;
                    float cy = plot.Top + (plot.Height * 0.5f);

                    g.TranslateTransform(cx, cy);
                    g.RotateTransform(-90f);

                    SizeF us = g.MeasureString(unit, Font);
                    g.DrawString(unit, Font, ub, -us.Width * 0.5f, -us.Height * 0.5f);

                    g.Restore(state);
                }
            }
        }

        private void drawLine(Graphics g, Rectangle plot, double[] buf, int count, int head, double max_display, Color color, float width)
        {
            if (count < 2)
                return;

            int n = Math.Min(count, buf.Length);
            float dx = plot.Width / (float)(buf.Length - 1);

            PointF[] pts = new PointF[n];

            int start = (head - n);
            if (start < 0)
                start += buf.Length;

            for (int i = 0; i < n; i++)
            {
                int idx = start + i;
                if (idx >= buf.Length)
                    idx -= buf.Length;

                double v_display = toDisplayUnits(buf[idx]);
                if (v_display < 0.0)
                    v_display = 0.0;

                float x = plot.Left + (dx * i);
                float y = plot.Bottom - (float)(v_display / max_display * plot.Height);

                if (y < plot.Top)
                    y = plot.Top;
                if (y > plot.Bottom)
                    y = plot.Bottom;

                pts[i] = new PointF(x, y);
            }

            using (Pen p = new Pen(color, width))
            {
                p.LineJoin = LineJoin.Round;
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                g.DrawLines(p, pts);
            }
        }

        private void drawOverlay(Graphics g, Rectangle plot)
        {
            using (Brush b = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
            using (Brush inb = new SolidBrush(Color.DeepSkyBlue))
            using (Brush outb = new SolidBrush(Color.Orange))
            using (Brush totb = new SolidBrush(Color.LimeGreen))
            {
                double in_disp = toDisplayUnits(_last_in_bps);
                double out_disp = toDisplayUnits(_last_out_bps);
                double tot_disp = toDisplayUnits(_last_tot_bps);

                string unit = _display_units == BandwidthUnits.KBps ? "kB/s" : "Mbit/s";

                string line1 = formatOverlayLine("In:", in_disp, unit);
                string line2 = formatOverlayLine("Out:", out_disp, unit);
                string line3 = formatOverlayLine("Tot:", tot_disp, unit);

                SizeF s1 = g.MeasureString(line1, Font);
                SizeF s2 = g.MeasureString(line2, Font);
                SizeF s3 = g.MeasureString(line3, Font);

                float x = plot.Left + 8;
                float y = plot.Top + 6;

                g.DrawString(line1, Font, inb, x, y);
                g.DrawString(line2, Font, outb, x, y + s1.Height);
                g.DrawString(line3, Font, totb, x, y + s1.Height + s2.Height);
            }
        }

        private string formatOverlayLine(string prefix, double value_display, string unit)
        {
            if (_display_units == BandwidthUnits.KBps)
            {
                int v = (int)Math.Ceiling(value_display);
                return prefix + " " + v.ToString() + " " + unit;
            }

            double v1 = Math.Ceiling(value_display * 10.0) / 10.0;
            return prefix + " " + v1.ToString("F1") + " " + unit;
        }

        private string formatAxisValue(double value_display)
        {
            if (_display_units == BandwidthUnits.KBps)
            {
                int v = (int)Math.Ceiling(value_display);
                return v.ToString();
            }

            double v1 = Math.Ceiling(value_display * 10.0) / 10.0;
            return v1.ToString("F1");
        }

        private double toDisplayUnits(double bytes_per_second)
        {
            if (_display_units == BandwidthUnits.KBps)
                return bytes_per_second / 1024.0;

            return bytes_per_second * 8.0 / 1_000_000.0;
        }

        private void updateScale()
        {
            if (_count <= 0)
                return;

            double max_bps = 0.0;

            int n = Math.Min(_count, _tot_bps.Length);
            for (int i = 0; i < n; i++)
            {
                double v = _tot_bps[i];
                if (v > max_bps)
                    max_bps = v;

                v = _in_bps[i];
                if (v > max_bps)
                    max_bps = v;

                v = _out_bps[i];
                if (v > max_bps)
                    max_bps = v;
            }

            double max_display = toDisplayUnits(max_bps);
            if (max_display < 0.000001)
                max_display = 1.0;

            double target = max_display * 1.2;
            if (target < 1.0)
                target = 1.0;

            if (target > _scale_max_display)
            {
                _scale_max_display = target;
                _scale_hold_ticks = 6;
                return;
            }

            if (_scale_hold_ticks > 0)
            {
                _scale_hold_ticks--;
                return;
            }

            double fall = _scale_max_display * 0.92;
            if (fall < target)
                fall = target;

            if (fall < 1.0)
                fall = 1.0;

            _scale_max_display = fall;
        }

        private void resizeBuffers(int seconds)
        {
            _in_bps = new double[seconds];
            _out_bps = new double[seconds];
            _tot_bps = new double[seconds];

            _head = 0;
            _count = 0;

            _scale_max_display = 1.0;
            _scale_hold_ticks = 0;

            _last_in_bps = 0.0;
            _last_out_bps = 0.0;
            _last_tot_bps = 0.0;

            resetSmoothing();
        }

        private void resetSmoothing()
        {
            _smooth_in = 0.0;
            _smooth_out = 0.0;
            _smooth_tot = 0.0;
            _smoothing_ready = false;
        }
    }
}
