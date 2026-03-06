using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Thetis
{
    public sealed class ucParametricEq : UserControl
    {
        public sealed class EqPoint
        {
            private readonly int _band_id;
            private Color _band_color;

            private double _frequency_hz;
            private double _gain_db;
            private double _q;

            public int BandId
            {
                get { return _band_id; }
            }

            public Color BandColor
            {
                get { return _band_color; }
                set { _band_color = value; }
            }

            public double FrequencyHz
            {
                get { return _frequency_hz; }
                set { _frequency_hz = value; }
            }

            public double GainDb
            {
                get { return _gain_db; }
                set { _gain_db = value; }
            }

            public double Q
            {
                get { return _q; }
                set { _q = value; }
            }

            public EqPoint(double frequency_hz, double gain_db, double q)
                : this(0, Color.Empty, frequency_hz, gain_db, q)
            {
            }

            public EqPoint(int band_id, Color band_color, double frequency_hz, double gain_db, double q)
            {
                _band_id = band_id;
                _band_color = band_color;
                _frequency_hz = frequency_hz;
                _gain_db = gain_db;
                _q = q;
            }
        }

        public sealed class EqDraggingEventArgs : EventArgs
        {
            private readonly bool _is_dragging;

            public bool IsDragging
            {
                get { return _is_dragging; }
            }

            public EqDraggingEventArgs(bool is_dragging)
            {
                _is_dragging = is_dragging;
            }
        }

        public sealed class EqPointDataChangedEventArgs : EventArgs
        {
            private readonly int _index;
            private readonly double _frequency_hz;
            private readonly double _gain_db;
            private readonly double _q;
            private readonly bool _is_dragging;

            public int Index
            {
                get { return _index; }
            }

            public double FrequencyHz
            {
                get { return _frequency_hz; }
            }

            public double GainDb
            {
                get { return _gain_db; }
            }

            public double Q
            {
                get { return _q; }
            }

            public bool IsDragging
            {
                get { return _is_dragging; }
            }

            public EqPointDataChangedEventArgs(int index, double frequency_hz, double gain_db, double q)
                : this(index, frequency_hz, gain_db, q, false)
            {
            }

            public EqPointDataChangedEventArgs(int index, double frequency_hz, double gain_db, double q, bool is_dragging)
            {
                _index = index;
                _frequency_hz = frequency_hz;
                _gain_db = gain_db;
                _q = q;
                _is_dragging = is_dragging;
            }
        }

        public sealed class EqPointSelectionChangedEventArgs : EventArgs
        {
            private readonly int _index;
            private readonly double _frequency_hz;
            private readonly double _gain_db;
            private readonly double _q;

            public int Index
            {
                get { return _index; }
            }

            public double FrequencyHz
            {
                get { return _frequency_hz; }
            }

            public double GainDb
            {
                get { return _gain_db; }
            }

            public double Q
            {
                get { return _q; }
            }

            public EqPointSelectionChangedEventArgs(int index, double frequency_hz, double gain_db, double q)
            {
                _index = index;
                _frequency_hz = frequency_hz;
                _gain_db = gain_db;
                _q = q;
            }
        }
        private sealed class EqJsonState
        {
            [JsonProperty("parametric_eq")]
            public bool ParametricEQ { get; set; }

            [JsonProperty("global_gain_db")]
            public double GlobalGainDb { get; set; }

            [JsonProperty("points")]
            public List<EqJsonPoint> Points { get; set; }
        }

        private sealed class EqJsonPoint
        {
            [JsonProperty("frequency_hz")]
            public double FrequencyHz { get; set; }

            [JsonProperty("gain_db")]
            public double GainDb { get; set; }

            [JsonProperty("q")]
            public double Q { get; set; }
        }

        private static readonly Color[] _default_band_palette = new Color[]
        {
            Color.FromArgb(0, 190, 255),
            Color.FromArgb(0, 220, 130),
            Color.FromArgb(255, 210, 0),
            Color.FromArgb(255, 140, 0),
            Color.FromArgb(255, 80, 80),
            Color.FromArgb(255, 0, 180),
            Color.FromArgb(170, 90, 255),
            Color.FromArgb(70, 120, 255),
            Color.FromArgb(0, 200, 200),
            Color.FromArgb(180, 255, 90),
            Color.FromArgb(255, 105, 180),
            Color.FromArgb(255, 215, 120),
            Color.FromArgb(120, 255, 255),
            Color.FromArgb(140, 200, 255),
            Color.FromArgb(220, 160, 255),
            Color.FromArgb(255, 120, 40),
            Color.FromArgb(120, 255, 160),
            Color.FromArgb(255, 60, 120)
        };

        private readonly List<EqPoint> _points;
        private readonly ReadOnlyCollection<EqPoint> _points_readonly;

        private int _band_count;

        private double _frequency_min_hz;
        private double _frequency_max_hz;

        private double _db_min;
        private double _db_max;

        private double _global_gain_db;

        private int _selected_index;
        private int _drag_index;

        private bool _dragging_global_gain;
        private bool _dragging_point;

        private EqPoint _drag_point_ref;
        private bool _drag_dirty_point;
        private bool _drag_dirty_global_gain;
        private bool _drag_dirty_selected_index;

        private int _plot_margin_left;
        private int _plot_margin_right;
        private int _plot_margin_top;
        private int _plot_margin_bottom;

        private int _point_radius;
        private int _hit_radius;

        private double _q_min;
        private double _q_max;

        private double _min_point_spacing_hz;

        private bool _allow_point_reorder;

        private bool _parametric_eq;

        private bool _show_readout;

        private int _global_handle_x_offset;
        private int _global_handle_size;
        private int _global_hit_extra;

        private bool _show_band_shading;
        private bool _use_per_band_colours;
        private Color _band_shade_color;
        private int _band_shade_alpha;
        private double _band_shade_weight_cutoff;

        private bool _show_axis_scales;
        private Color _axis_text_color;
        private Color _axis_tick_color;
        private int _axis_tick_length;

        public event EventHandler<EqDraggingEventArgs> PointsChanged;
        public event EventHandler<EqDraggingEventArgs> GlobalGainChanged;
        public event EventHandler<EqDraggingEventArgs> SelectedIndexChanged;
        public event EventHandler<EqPointDataChangedEventArgs> PointDataChanged;
        public event EventHandler<EqPointSelectionChangedEventArgs> PointSelected;
        public event EventHandler<EqPointSelectionChangedEventArgs> PointUnselected;

        public ucParametricEq()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);

            _band_count = 10;

            _frequency_min_hz = 0.0;
            _frequency_max_hz = 4000.0;

            _db_min = -24.0;
            _db_max = 24.0;

            _global_gain_db = 0.0;

            _plot_margin_left = 30;
            _plot_margin_right = 18;
            _plot_margin_top = 14;
            _plot_margin_bottom = 62;

            _point_radius = 5;
            _hit_radius = 11;

            _q_min = 0.2;
            _q_max = 30.0;

            _min_point_spacing_hz = 5.0;

            _allow_point_reorder = true;

            _parametric_eq = true;

            _show_readout = true;

            _global_handle_x_offset = 6;
            _global_handle_size = 10;
            _global_hit_extra = 6;

            _show_band_shading = true;
            _use_per_band_colours = true;
            _band_shade_color = Color.FromArgb(200, 200, 200);
            _band_shade_alpha = 70;
            _band_shade_weight_cutoff = 0.002;

            _show_axis_scales = true;
            _axis_text_color = Color.FromArgb(170, 170, 170);
            _axis_tick_color = Color.FromArgb(80, 80, 80);
            _axis_tick_length = 6;

            _points = new List<EqPoint>();
            _points_readonly = new ReadOnlyCollection<EqPoint>(_points);

            _selected_index = -1;
            _drag_index = -1;

            _drag_point_ref = null;
            _drag_dirty_point = false;
            _drag_dirty_global_gain = false;
            _drag_dirty_selected_index = false;

            BackColor = Color.FromArgb(25, 25, 25);
            ForeColor = Color.Gainsboro;

            resetPointsDefault();
        }

        [Category("EQ")]
        [DefaultValue(10)]
        public int BandCount
        {
            get { return _band_count; }
            set
            {
                int v = value;
                if (v < 2) v = 2;
                if (v > 256) v = 256;
                if (v == _band_count) return;

                _band_count = v;

                bool had_selection = _selected_index != -1;
                resetPointsDefault();

                if (had_selection)
                {
                    _selected_index = -1;
                    raiseSelectedIndexChanged(false);
                }

                raisePointsChanged(false);
                Invalidate();
            }
        }

        [Category("EQ")]
        public double FrequencyMinHz
        {
            get { return _frequency_min_hz; }
            set
            {
                double new_min = value;
                if (double.IsNaN(new_min) || double.IsInfinity(new_min)) return;
                if (new_min >= _frequency_max_hz) return;

                double old_min = _frequency_min_hz;
                double old_max = _frequency_max_hz;

                _frequency_min_hz = new_min;
                rescaleFrequencies(old_min, old_max, _frequency_min_hz, _frequency_max_hz);
                enforceOrdering(true);
                raisePointsChanged(false);
                Invalidate();
            }
        }

        [Category("EQ")]
        public double FrequencyMaxHz
        {
            get { return _frequency_max_hz; }
            set
            {
                double new_max = value;
                if (double.IsNaN(new_max) || double.IsInfinity(new_max)) return;
                if (new_max <= _frequency_min_hz) return;

                double old_min = _frequency_min_hz;
                double old_max = _frequency_max_hz;

                _frequency_max_hz = new_max;
                rescaleFrequencies(old_min, old_max, _frequency_min_hz, _frequency_max_hz);
                enforceOrdering(true);
                raisePointsChanged(false);
                Invalidate();
            }
        }

        [Category("EQ")]
        public double DbMin
        {
            get { return _db_min; }
            set
            {
                double v = value;
                if (double.IsNaN(v) || double.IsInfinity(v)) return;
                if (v >= _db_max) return;
                _db_min = v;
                clampAllGains();
                raisePointsChanged(false);
                Invalidate();
            }
        }

        [Category("EQ")]
        public double DbMax
        {
            get { return _db_max; }
            set
            {
                double v = value;
                if (double.IsNaN(v) || double.IsInfinity(v)) return;
                if (v <= _db_min) return;
                _db_max = v;
                clampAllGains();
                raisePointsChanged(false);
                Invalidate();
            }
        }

        [Category("EQ")]
        public double GlobalGainDb
        {
            get { return _global_gain_db; }
            set
            {
                double v = clamp(value, _db_min, _db_max);
                if (Math.Abs(v - _global_gain_db) < 0.000001) return;

                _global_gain_db = v;

                if (_dragging_global_gain) _drag_dirty_global_gain = true;

                raiseGlobalGainChanged(isDraggingNow());
                Invalidate();
            }
        }

        [Category("EQ")]
        public bool ShowReadout
        {
            get { return _show_readout; }
            set
            {
                _show_readout = value;
                Invalidate();
            }
        }

        [Category("EQ")]
        public double MinPointSpacingHz
        {
            get { return _min_point_spacing_hz; }
            set
            {
                double v = value;
                if (double.IsNaN(v) || double.IsInfinity(v)) return;
                if (v < 0.0) v = 0.0;
                _min_point_spacing_hz = v;
                enforceOrdering(true);
                raisePointsChanged(false);
                Invalidate();
            }
        }

        [Category("EQ")]
        public bool AllowPointReorder
        {
            get { return _allow_point_reorder; }
            set
            {
                bool v = value;
                if (v == _allow_point_reorder) return;
                _allow_point_reorder = v;
                enforceOrdering(true);
                raisePointsChanged(false);
                Invalidate();
            }
        }

        [Category("EQ")]
        public bool ParametricEQ
        {
            get { return _parametric_eq; }
            set
            {
                bool v = value;
                if (v == _parametric_eq) return;
                _parametric_eq = v;
                Invalidate();
            }
        }

        [Category("EQ")]
        public double QMin
        {
            get { return _q_min; }
            set
            {
                double v = value;
                if (double.IsNaN(v) || double.IsInfinity(v)) return;
                if (v <= 0.0) return;
                _q_min = v;
                if (_q_max < _q_min) _q_max = _q_min;
                clampAllQ();
                raisePointsChanged(false);
                Invalidate();
            }
        }

        [Category("EQ")]
        public double QMax
        {
            get { return _q_max; }
            set
            {
                double v = value;
                if (double.IsNaN(v) || double.IsInfinity(v)) return;
                if (v <= 0.0) return;
                _q_max = v;
                if (_q_min > _q_max) _q_min = _q_max;
                clampAllQ();
                raisePointsChanged(false);
                Invalidate();
            }
        }

        [Category("EQ")]
        public bool ShowBandShading
        {
            get { return _show_band_shading; }
            set
            {
                _show_band_shading = value;
                Invalidate();
            }
        }

        [Category("EQ")]
        public bool UsePerBandColours
        {
            get { return _use_per_band_colours; }
            set
            {
                _use_per_band_colours = value;
                Invalidate();
            }
        }

        [Category("EQ")]
        public Color BandShadeColor
        {
            get { return _band_shade_color; }
            set
            {
                _band_shade_color = value;
                Invalidate();
            }
        }

        [Category("EQ")]
        public int BandShadeAlpha
        {
            get { return _band_shade_alpha; }
            set
            {
                int v = value;
                if (v < 0) v = 0;
                if (v > 255) v = 255;
                _band_shade_alpha = v;
                Invalidate();
            }
        }

        [Category("EQ")]
        public double BandShadeWeightCutoff
        {
            get { return _band_shade_weight_cutoff; }
            set
            {
                double v = value;
                if (double.IsNaN(v) || double.IsInfinity(v)) return;
                if (v < 0.0) v = 0.0;
                _band_shade_weight_cutoff = v;
                Invalidate();
            }
        }

        [Category("EQ")]
        public bool ShowAxisScales
        {
            get { return _show_axis_scales; }
            set
            {
                _show_axis_scales = value;
                Invalidate();
            }
        }

        [Category("EQ")]
        public int AxisTickLength
        {
            get { return _axis_tick_length; }
            set
            {
                int v = value;
                if (v < 2) v = 2;
                if (v > 20) v = 20;
                _axis_tick_length = v;
                Invalidate();
            }
        }

        [Category("EQ")]
        public Color AxisTextColor
        {
            get { return _axis_text_color; }
            set
            {
                _axis_text_color = value;
                Invalidate();
            }
        }

        [Category("EQ")]
        public Color AxisTickColor
        {
            get { return _axis_tick_color; }
            set
            {
                _axis_tick_color = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        public IReadOnlyList<EqPoint> Points
        {
            get { return _points_readonly; }
        }

        [Browsable(false)]
        public int SelectedIndex
        {
            get { return _selected_index; }
            set
            {
                int v = value;
                if (v < -1) v = -1;
                if (v >= _points.Count) v = _points.Count - 1;
                if (v == _selected_index) return;

                EqPoint old_point = null;
                int old_index = _selected_index;

                if (old_index >= 0 && old_index < _points.Count)
                {
                    old_point = _points[old_index];
                }

                _selected_index = v;

                if (old_point != null)
                {
                    raisePointUnselected(old_index, old_point);
                }

                if (_selected_index >= 0 && _selected_index < _points.Count)
                {
                    EqPoint new_point = _points[_selected_index];
                    raisePointSelected(_selected_index, new_point);
                }

                raiseSelectedIndexChanged(isDraggingNow());
                Invalidate();
            }
        }
        private void raisePointSelected(int index, EqPoint p)
        {
            if (p == null) return;

            double q = _parametric_eq ? p.Q : 0.0;

            EventHandler<EqPointSelectionChangedEventArgs> h = PointSelected;
            if (h != null) h(this, new EqPointSelectionChangedEventArgs(index, p.FrequencyHz, p.GainDb, q));
        }

        private void raisePointUnselected(int index, EqPoint p)
        {
            if (p == null) return;

            double q = _parametric_eq ? p.Q : 0.0;

            EventHandler<EqPointSelectionChangedEventArgs> h = PointUnselected;
            if (h != null) h(this, new EqPointSelectionChangedEventArgs(index, p.FrequencyHz, p.GainDb, q));
        }
        public void ResetPoints()
        {
            resetPointsDefault();
            raisePointsChanged(false);
            Invalidate();
        }

        public void GetPointData(int index, out double frequency_hz, out double gain_db, out double q)
        {
            frequency_hz = 0.0;
            gain_db = 0.0;
            q = 0.0;

            if (index < 0 || index >= _points.Count) return;

            EqPoint p = _points[index];
            frequency_hz = Math.Round(p.FrequencyHz, 3);
            gain_db = Math.Round(p.GainDb, 1);
            q = _parametric_eq ? Math.Round(p.Q, 2) : 0.0;
        }

        public bool SetPointData(int index, double frequency_hz, double gain_db, double q)
        {
            if (index < 0 || index >= _points.Count) return false;

            EqPoint p = _points[index];

            double old_f = p.FrequencyHz;
            double old_g = p.GainDb;
            double old_q = p.Q;

            p.FrequencyHz = clamp(frequency_hz, _frequency_min_hz, _frequency_max_hz);
            p.GainDb = clamp(gain_db, _db_min, _db_max);
            p.Q = clamp(q, _q_min, _q_max);

            enforceOrdering(true);

            if (Math.Abs(p.FrequencyHz - old_f) > 0.000001 || Math.Abs(p.GainDb - old_g) > 0.000001 || Math.Abs(p.Q - old_q) > 0.000001)
            {
                raisePointsChanged(false);
                raisePointDataChangedForPoint(p, false);
                Invalidate();
            }

            return true;
        }

        public void GetPointsData(out double[] frequency_hz, out double[] gain_db, out double[] q)
        {
            int n = _points.Count;

            double[] f = new double[n];
            double[] g = new double[n];
            double[] qq = new double[n];

            for (int i = 0; i < n; i++)
            {
                EqPoint p = _points[i];
                f[i] = p.FrequencyHz;
                g[i] = p.GainDb;
                qq[i] = _parametric_eq ? p.Q : 0.0;
            }

            frequency_hz = f;
            gain_db = g;
            q = qq;
        }

        public bool SetPointsData(double[] frequency_hz, double[] gain_db, double[] q)
        {
            if (frequency_hz == null || gain_db == null || q == null) return false;
            if (frequency_hz.Length != _points.Count) return false;
            if (gain_db.Length != _points.Count) return false;
            if (q.Length != _points.Count) return false;

            bool any_changed = false;

            for (int i = 0; i < _points.Count; i++)
            {
                EqPoint p = _points[i];

                double old_f = p.FrequencyHz;
                double old_g = p.GainDb;
                double old_q = p.Q;

                p.FrequencyHz = clamp(frequency_hz[i], _frequency_min_hz, _frequency_max_hz);
                p.GainDb = clamp(gain_db[i], _db_min, _db_max);
                p.Q = clamp(q[i], _q_min, _q_max);

                if (Math.Abs(p.FrequencyHz - old_f) > 0.000001 || Math.Abs(p.GainDb - old_g) > 0.000001 || Math.Abs(p.Q - old_q) > 0.000001)
                {
                    any_changed = true;
                }
            }

            enforceOrdering(true);

            if (any_changed)
            {
                raisePointsChanged(false);
                Invalidate();
            }

            return true;
        }

        public string SaveToJson()
        {
            EqJsonState state = new EqJsonState();
            state.ParametricEQ = _parametric_eq;
            state.GlobalGainDb = Math.Round(_global_gain_db, 1);

            List<EqJsonPoint> pts = new List<EqJsonPoint>(_points.Count);
            for (int i = 0; i < _points.Count; i++)
            {
                EqPoint p = _points[i];
                EqJsonPoint jp = new EqJsonPoint();
                jp.FrequencyHz = Math.Round(p.FrequencyHz, 3);
                jp.GainDb = Math.Round(p.GainDb, 1);
                jp.Q = Math.Round(p.Q, 2);
                pts.Add(jp);
            }

            state.Points = pts;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;

            return JsonConvert.SerializeObject(state, settings);
        }

        public bool LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;

            EqJsonState state;

            try
            {
                state = JsonConvert.DeserializeObject<EqJsonState>(json);
            }
            catch
            {
                return false;
            }

            if (state == null) return false;
            if (state.Points == null) return false;

            if (state.Points.Count != _points.Count)
            {
                int new_count = state.Points.Count;
                if (new_count < 2) return false;
                if (new_count > 256) return false;

                _band_count = new_count;
                resetPointsDefault();
            }

            bool any_changed = false;

            bool old_param = _parametric_eq;
            double old_global = _global_gain_db;

            _parametric_eq = state.ParametricEQ;
            _global_gain_db = clamp(state.GlobalGainDb, _db_min, _db_max);

            if (old_param != _parametric_eq) any_changed = true;
            if (Math.Abs(old_global - _global_gain_db) > 0.000001) any_changed = true;

            for (int i = 0; i < _points.Count; i++)
            {
                EqPoint p = _points[i];
                EqJsonPoint jp = state.Points[i];

                double old_f = p.FrequencyHz;
                double old_g = p.GainDb;
                double old_q = p.Q;

                p.FrequencyHz = clamp(jp.FrequencyHz, _frequency_min_hz, _frequency_max_hz);
                p.GainDb = clamp(jp.GainDb, _db_min, _db_max);
                p.Q = clamp(jp.Q, _q_min, _q_max);

                if (Math.Abs(p.FrequencyHz - old_f) > 0.000001 || Math.Abs(p.GainDb - old_g) > 0.000001 || Math.Abs(p.Q - old_q) > 0.000001)
                {
                    any_changed = true;
                }
            }

            enforceOrdering(true);

            if (any_changed)
            {
                raisePointsChanged(false);
                raiseGlobalGainChanged(false);
                Invalidate();
            }

            return true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle client = ClientRectangle;
            if (client.Width < 2 || client.Height < 2) return;

            using (SolidBrush back = new SolidBrush(BackColor))
            {
                g.FillRectangle(back, client);
            }

            Rectangle plot = getPlotRect();
            if (plot.Width < 2 || plot.Height < 2) return;

            drawGrid(g, plot);

            Region old_clip = g.Clip;
            g.SetClip(plot);

            if (_show_band_shading) drawBandShading(g, plot);
            drawCurve(g, plot);
            drawPoints(g, plot);

            g.Clip = old_clip;

            if (_show_axis_scales) drawAxisScales(g, plot);

            drawGlobalGainHandle(g, plot);
            drawBorder(g, plot);
            if (_show_readout) drawReadout(g, plot);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            Rectangle plot = getPlotRect();

            if (hitTestGlobalGainHandle(plot, e.Location))
            {
                _dragging_global_gain = true;
                _dragging_point = false;
                _drag_index = -1;

                _drag_point_ref = null;
                _drag_dirty_point = false;
                _drag_dirty_global_gain = false;
                _drag_dirty_selected_index = false;

                Capture = true;
                Invalidate();
                return;
            }

            if (plot.Contains(e.Location))
            {
                int idx = hitTestPoint(plot, e.Location);
                if (idx >= 0)
                {
                    SelectedIndex = idx;

                    _dragging_point = true;
                    _dragging_global_gain = false;
                    _drag_index = idx;

                    _drag_point_ref = _points[idx];
                    _drag_dirty_point = false;
                    _drag_dirty_global_gain = false;
                    _drag_dirty_selected_index = false;

                    Capture = true;
                    Invalidate();
                    return;
                }
            }

            SelectedIndex = -1;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Rectangle plot = getPlotRect();

            if (_dragging_global_gain)
            {
                if (!Capture) return;
                double db = dbFromY(plot, e.Location.Y);
                GlobalGainDb = db;
                return;
            }

            if (_dragging_point)
            {
                if (!Capture) return;
                if (_drag_index < 0 || _drag_index >= _points.Count) return;

                EqPoint p = _points[_drag_index];

                double old_f = p.FrequencyHz;
                double old_g = p.GainDb;
                double old_q = p.Q;

                double freq = freqFromX(plot, e.Location.X);
                double gain = dbFromY(plot, e.Location.Y);

                freq = clamp(freq, _frequency_min_hz, _frequency_max_hz);
                gain = clamp(gain, _db_min, _db_max);

                if (!_allow_point_reorder)
                {
                    double min_f;
                    double max_f;

                    if (_drag_index == 0)
                    {
                        min_f = _frequency_min_hz;
                        max_f = _points[1].FrequencyHz - _min_point_spacing_hz;
                    }
                    else if (_drag_index == _points.Count - 1)
                    {
                        min_f = _points[_points.Count - 2].FrequencyHz + _min_point_spacing_hz;
                        max_f = _frequency_max_hz;
                    }
                    else
                    {
                        min_f = _points[_drag_index - 1].FrequencyHz + _min_point_spacing_hz;
                        max_f = _points[_drag_index + 1].FrequencyHz - _min_point_spacing_hz;
                    }

                    if (max_f < min_f) max_f = min_f;
                    freq = clamp(freq, min_f, max_f);
                }

                bool changed = false;

                if (Math.Abs(p.FrequencyHz - freq) > 0.000001)
                {
                    p.FrequencyHz = freq;
                    changed = true;
                }

                if (Math.Abs(p.GainDb - gain) > 0.000001)
                {
                    p.GainDb = gain;
                    changed = true;
                }

                if (changed)
                {
                    if (_allow_point_reorder)
                    {
                        enforceOrdering(false);

                        int idx = _drag_index;
                        double min_f = _frequency_min_hz;
                        double max_f = _frequency_max_hz;

                        if (_points.Count > 1)
                        {
                            if (idx > 0) min_f = _points[idx - 1].FrequencyHz + _min_point_spacing_hz;
                            if (idx < _points.Count - 1) max_f = _points[idx + 1].FrequencyHz - _min_point_spacing_hz;
                            if (max_f < min_f) max_f = min_f;
                        }

                        double clamped_freq = clamp(p.FrequencyHz, min_f, max_f);
                        if (Math.Abs(clamped_freq - p.FrequencyHz) > 0.000001) p.FrequencyHz = clamped_freq;

                        enforceOrdering(false);
                    }

                    _drag_dirty_point = true;

                    raisePointsChanged(true);
                    if (Math.Abs(p.FrequencyHz - old_f) > 0.000001 || Math.Abs(p.GainDb - old_g) > 0.000001 || Math.Abs(p.Q - old_q) > 0.000001)
                    {
                        raisePointDataChangedForPoint(p, true);
                    }
                    Invalidate();
                }

                return;
            }

            bool want_hand = false;

            if (hitTestGlobalGainHandle(plot, e.Location))
            {
                want_hand = true;
            }
            else if (plot.Contains(e.Location))
            {
                int idx = hitTestPoint(plot, e.Location);
                if (idx >= 0) want_hand = true;
            }

            Cursor = want_hand ? Cursors.Hand : Cursors.Default;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            bool was_dragging_point = _dragging_point;
            bool was_dragging_global_gain = _dragging_global_gain;

            EqPoint drag_point_ref = _drag_point_ref;

            bool point_dirty = _drag_dirty_point;
            bool global_dirty = _drag_dirty_global_gain;
            bool selected_dirty = _drag_dirty_selected_index;

            if (Capture) Capture = false;

            _dragging_global_gain = false;
            _dragging_point = false;
            _drag_index = -1;

            _drag_point_ref = null;
            _drag_dirty_point = false;
            _drag_dirty_global_gain = false;
            _drag_dirty_selected_index = false;

            if (was_dragging_point && point_dirty)
            {
                raisePointsChanged(false);
                if (drag_point_ref != null) raisePointDataChangedForPoint(drag_point_ref, false);
            }

            if (was_dragging_global_gain && global_dirty)
            {
                raiseGlobalGainChanged(false);
            }

            if (selected_dirty)
            {
                raiseSelectedIndexChanged(false);
            }

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Rectangle plot = getPlotRect();

            double steps = (double)e.Delta / 120.0;
            if (steps == 0.0) return;

            if (_selected_index < 0 || _selected_index >= _points.Count)
            {
                if (hitTestGlobalGainHandle(plot, e.Location))
                {
                    GlobalGainDb = clamp(_global_gain_db + (steps * 0.5), _db_min, _db_max);
                }
                return;
            }

            if (!plot.Contains(e.Location)) return;

            EqPoint p = _points[_selected_index];

            double old_f = p.FrequencyHz;
            double old_g = p.GainDb;
            double old_q = p.Q;

            bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;

            if (ctrl)
            {
                double step_hz = chooseFrequencyStep((_frequency_max_hz - _frequency_min_hz)) / 5.0;
                if (step_hz < 1.0) step_hz = 1.0;

                double freq = p.FrequencyHz + (steps * step_hz);
                freq = clamp(freq, _frequency_min_hz, _frequency_max_hz);

                if (!_allow_point_reorder)
                {
                    double min_f;
                    double max_f;

                    if (_selected_index == 0)
                    {
                        min_f = _frequency_min_hz;
                        max_f = _points[1].FrequencyHz - _min_point_spacing_hz;
                    }
                    else if (_selected_index == _points.Count - 1)
                    {
                        min_f = _points[_points.Count - 2].FrequencyHz + _min_point_spacing_hz;
                        max_f = _frequency_max_hz;
                    }
                    else
                    {
                        min_f = _points[_selected_index - 1].FrequencyHz + _min_point_spacing_hz;
                        max_f = _points[_selected_index + 1].FrequencyHz - _min_point_spacing_hz;
                    }

                    if (max_f < min_f) max_f = min_f;

                    freq = clamp(freq, min_f, max_f);
                }

                if (Math.Abs(freq - p.FrequencyHz) > 0.000001)
                {
                    p.FrequencyHz = freq;

                    if (_allow_point_reorder)
                    {
                        enforceOrdering(false);

                        int idx = _selected_index;
                        double min_f = _frequency_min_hz;
                        double max_f = _frequency_max_hz;

                        if (_points.Count > 1)
                        {
                            if (idx > 0) min_f = _points[idx - 1].FrequencyHz + _min_point_spacing_hz;
                            if (idx < _points.Count - 1) max_f = _points[idx + 1].FrequencyHz - _min_point_spacing_hz;
                            if (max_f < min_f) max_f = min_f;
                        }

                        double clamped_freq = clamp(p.FrequencyHz, min_f, max_f);
                        if (Math.Abs(clamped_freq - p.FrequencyHz) > 0.000001) p.FrequencyHz = clamped_freq;

                        enforceOrdering(false);
                    }

                    raisePointsChanged(false);
                    if (Math.Abs(p.FrequencyHz - old_f) > 0.000001 || Math.Abs(p.GainDb - old_g) > 0.000001 || Math.Abs(p.Q - old_q) > 0.000001)
                    {
                        raisePointDataChangedForPoint(p, false);
                    }
                    Invalidate();
                }

                return;
            }

            if (!_parametric_eq)
            {
                double gain = clamp(p.GainDb + (steps * 0.5), _db_min, _db_max);
                if (Math.Abs(gain - p.GainDb) > 0.000001)
                {
                    p.GainDb = gain;
                    raisePointsChanged(false);
                    if (Math.Abs(p.FrequencyHz - old_f) > 0.000001 || Math.Abs(p.GainDb - old_g) > 0.000001 || Math.Abs(p.Q - old_q) > 0.000001)
                    {
                        raisePointDataChangedForPoint(p, false);
                    }
                    Invalidate();
                }
                return;
            }

            if (shift)
            {
                double gain = clamp(p.GainDb + (steps * 0.5), _db_min, _db_max);
                if (Math.Abs(gain - p.GainDb) > 0.000001)
                {
                    p.GainDb = gain;
                    raisePointsChanged(false);
                    if (Math.Abs(p.FrequencyHz - old_f) > 0.000001 || Math.Abs(p.GainDb - old_g) > 0.000001 || Math.Abs(p.Q - old_q) > 0.000001)
                    {
                        raisePointDataChangedForPoint(p, false);
                    }
                    Invalidate();
                }
                return;
            }

            double factor = Math.Pow(1.12, steps);
            double qv = clamp(p.Q * factor, _q_min, _q_max);

            if (Math.Abs(qv - p.Q) > 0.000001)
            {
                p.Q = qv;
                raisePointsChanged(false);
                if (Math.Abs(p.FrequencyHz - old_f) > 0.000001 || Math.Abs(p.GainDb - old_g) > 0.000001 || Math.Abs(p.Q - old_q) > 0.000001)
                {
                    raisePointDataChangedForPoint(p, false);
                }
                Invalidate();
            }
        }

        private bool isDraggingNow()
        {
            return _dragging_global_gain || _dragging_point;
        }

        private int getAxisLabelMaxWidth()
        {
            string s1 = formatDbTick(_db_min);
            string s2 = formatDbTick(_db_max);

            Size sz1 = TextRenderer.MeasureText(s1, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            Size sz2 = TextRenderer.MeasureText(s2, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);

            return Math.Max(sz1.Width, sz2.Width);
        }

        private int getComputedPlotMarginLeft()
        {
            int m = _plot_margin_left;

            if (_show_axis_scales)
            {
                int label_w = getAxisLabelMaxWidth();
                int need = _axis_tick_length + 4 + label_w + 8;
                if (need > m) m = need;
            }

            if (m < 10) m = 10;
            return m;
        }

        private int getComputedPlotMarginRight()
        {
            int m = _plot_margin_right;

            int need = _global_handle_x_offset + (_global_handle_size * 2) + _global_hit_extra + 6;
            if (need > m) m = need;

            if (m < 10) m = 10;
            return m;
        }

        private Rectangle getPlotRect()
        {
            Rectangle r = ClientRectangle;

            int left = getComputedPlotMarginLeft();
            int right = getComputedPlotMarginRight();
            int bottom = getComputedPlotMarginBottom();

            int x = r.X + left;
            int y = r.Y + _plot_margin_top;
            int w = r.Width - left - right;
            int h = r.Height - _plot_margin_top - bottom;

            if (w < 1) w = 1;
            if (h < 1) h = 1;

            return new Rectangle(x, y, w, h);
        }

        private void drawGrid(Graphics g, Rectangle plot)
        {
            using (SolidBrush plot_back = new SolidBrush(Color.FromArgb(18, 18, 18)))
            {
                g.FillRectangle(plot_back, plot);
            }

            using (Pen grid_pen = new Pen(Color.FromArgb(45, 45, 45), 1f))
            {
                int v_lines = 10;
                for (int i = 0; i <= v_lines; i++)
                {
                    float t = (float)i / (float)v_lines;
                    int x = plot.Left + (int)Math.Round(t * plot.Width);
                    g.DrawLine(grid_pen, x, plot.Top, x, plot.Bottom);
                }

                double step_db = 6.0;
                double start = Math.Ceiling(_db_min / step_db) * step_db;

                for (double db = start; db <= _db_max + 0.000001; db += step_db)
                {
                    float y = yFromDb(plot, db);
                    g.DrawLine(grid_pen, plot.Left, y, plot.Right, y);
                }
            }

            using (Pen zero_pen = new Pen(Color.FromArgb(75, 75, 75), 1.5f))
            {
                float y0 = yFromDb(plot, 0.0);
                g.DrawLine(zero_pen, plot.Left, y0, plot.Right, y0);
            }
        }

        private void drawBandShading(Graphics g, Rectangle plot)
        {
            if (_points.Count == 0) return;

            if (_parametric_eq)
            {
                int samples = plot.Width;
                if (samples < 64) samples = 64;

                double span = _frequency_max_hz - _frequency_min_hz;
                if (span <= 0.0) span = 1.0;

                float baseline_y = yFromDb(plot, 0.0);

                for (int band = 0; band < _points.Count; band++)
                {
                    EqPoint p = _points[band];
                    if (Math.Abs(p.GainDb) < 0.000001) continue;

                    double q = clamp(p.Q, _q_min, _q_max);

                    double fwhm = span / (q * 3.0);
                    double min_fwhm = span / 6000.0;
                    if (fwhm < min_fwhm) fwhm = min_fwhm;

                    double sigma = fwhm / 2.3548200450309493;

                    Color base_col = p.BandColor;
                    if (base_col == Color.Empty) base_col = getBandBaseColor(band);

                    Color fill_col = Color.FromArgb(_band_shade_alpha, base_col.R, base_col.G, base_col.B);
                    if (!_use_per_band_colours) fill_col = Color.FromArgb(_band_shade_alpha, _band_shade_color.R, _band_shade_color.G, _band_shade_color.B);

                    PointF[] poly = new PointF[samples + 2];
                    poly[0] = new PointF(plot.Left, baseline_y);

                    for (int i = 0; i < samples; i++)
                    {
                        double t = (double)i / (double)(samples - 1);
                        double f = _frequency_min_hz + t * span;

                        double d = (f - p.FrequencyHz) / sigma;
                        double w = Math.Exp(-0.5 * d * d);

                        double band_db = 0.0;
                        if (w >= _band_shade_weight_cutoff) band_db = p.GainDb * w;

                        float x = plot.Left + (float)(t * plot.Width);
                        float y = yFromDb(plot, band_db);
                        poly[i + 1] = new PointF(x, y);
                    }

                    poly[samples + 1] = new PointF(plot.Right, baseline_y);

                    using (SolidBrush shade = new SolidBrush(fill_col))
                    {
                        g.FillPolygon(shade, poly, FillMode.Winding);
                    }
                }

                return;
            }

            float base_y = yFromDb(plot, 0.0);

            double prev_f = _frequency_min_hz;
            double prev_db = 0.0;
            Color prev_col = Color.Empty;

            if (_points.Count > 0)
            {
                prev_col = _points[0].BandColor;
                if (prev_col == Color.Empty) prev_col = getBandBaseColor(0);
            }
            else
            {
                prev_col = _band_shade_color;
            }

            for (int i = 0; i <= _points.Count; i++)
            {
                double next_f;
                double next_db;
                Color next_col;

                if (i < _points.Count)
                {
                    EqPoint p = _points[i];
                    next_f = p.FrequencyHz;
                    next_db = p.GainDb;
                    next_col = p.BandColor;
                    if (next_col == Color.Empty) next_col = getBandBaseColor(i);
                }
                else
                {
                    next_f = _frequency_max_hz;
                    next_db = 0.0;
                    if (_points.Count > 0)
                    {
                        next_col = _points[_points.Count - 1].BandColor;
                        if (next_col == Color.Empty) next_col = getBandBaseColor(_points.Count - 1);
                    }
                    else
                    {
                        next_col = _band_shade_color;
                    }
                }

                if (next_f < _frequency_min_hz) next_f = _frequency_min_hz;
                if (next_f > _frequency_max_hz) next_f = _frequency_max_hz;

                float x0 = xFromFreq(plot, prev_f);
                float x1 = xFromFreq(plot, next_f);
                if (Math.Abs(x1 - x0) < 0.5f)
                {
                    prev_f = next_f;
                    prev_db = next_db;
                    prev_col = next_col;
                    continue;
                }

                float y0 = yFromDb(plot, prev_db);
                float y1 = yFromDb(plot, next_db);

                PointF[] poly = new PointF[4];
                poly[0] = new PointF(x0, base_y);
                poly[1] = new PointF(x0, y0);
                poly[2] = new PointF(x1, y1);
                poly[3] = new PointF(x1, base_y);

                Color c0 = prev_col;
                Color c1 = next_col;
                if (!_use_per_band_colours)
                {
                    c0 = _band_shade_color;
                    c1 = _band_shade_color;
                }

                Color a0 = Color.FromArgb(_band_shade_alpha, c0.R, c0.G, c0.B);
                Color a1 = Color.FromArgb(_band_shade_alpha, c1.R, c1.G, c1.B);

                using (LinearGradientBrush br = new LinearGradientBrush(new PointF(x0, 0f), new PointF(x1, 0f), a0, a1))
                {
                    g.FillPolygon(br, poly, FillMode.Winding);
                }

                prev_f = next_f;
                prev_db = next_db;
                prev_col = next_col;
            }
        }

        private void drawCurve(Graphics g, Rectangle plot)
        {
            if (_points.Count == 0) return;

            int samples = plot.Width;
            if (samples < 64) samples = 64;

            PointF[] pts = new PointF[samples];

            for (int i = 0; i < samples; i++)
            {
                double t = (double)i / (double)(samples - 1);
                double f = _frequency_min_hz + t * (_frequency_max_hz - _frequency_min_hz);

                double db = responseDbAtFrequency(f);
                db = db + _global_gain_db;

                if (i == 0 || i == samples - 1) db = _global_gain_db;

                float x = plot.Left + (float)(t * plot.Width);
                float y = yFromDb(plot, db);
                pts[i] = new PointF(x, y);
            }

            using (Pen curve_pen = new Pen(Color.White, 2f))
            {
                g.DrawLines(curve_pen, pts);
            }
        }

        private void drawGlobalGainHandle(Graphics g, Rectangle plot)
        {
            float y = yFromDb(plot, _global_gain_db);

            int hx = plot.Right + _global_handle_x_offset;
            int s = _global_handle_size;

            Point[] tri = new Point[3];
            tri[0] = new Point(hx, (int)Math.Round(y));
            tri[1] = new Point(hx + s, (int)Math.Round(y) - s);
            tri[2] = new Point(hx + s, (int)Math.Round(y) + s);

            using (SolidBrush b = new SolidBrush(Color.White))
            {
                g.FillPolygon(b, tri);
            }

            using (Pen p = new Pen(Color.FromArgb(40, 40, 40), 1f))
            {
                g.DrawPolygon(p, tri);
            }
        }

        private void drawPoints(Graphics g, Rectangle plot)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                EqPoint p = _points[i];

                float x = xFromFreq(plot, p.FrequencyHz);
                float y = yFromDb(plot, p.GainDb);

                bool selected = (i == _selected_index);

                Color base_col = p.BandColor;
                if (base_col == Color.Empty) base_col = getBandBaseColor(i);
                Color dot_col = _use_per_band_colours ? base_col : Color.FromArgb(90, 200, 255);

                if (selected) dot_col = Color.FromArgb(255, 200, 80);

                float r = _point_radius;
                if (selected) r = r + 1f;

                using (SolidBrush b = new SolidBrush(dot_col))
                {
                    g.FillEllipse(b, x - r, y - r, r * 2f, r * 2f);
                }

                using (Pen outline = new Pen(Color.FromArgb(35, 35, 35), 1f))
                {
                    g.DrawEllipse(outline, x - r, y - r, r * 2f, r * 2f);
                }
            }
        }

        private void drawAxisScales(Graphics g, Rectangle plot)
        {
            using (Pen tick_pen = new Pen(_axis_tick_color, 1f))
            using (SolidBrush text_brush = new SolidBrush(_axis_text_color))
            {
                double step_db = 6.0;
                double start_db = Math.Ceiling(_db_min / step_db) * step_db;

                bool drew_min = false;
                bool drew_max = false;

                for (double db = start_db; db <= _db_max + 0.000001; db += step_db)
                {
                    if (Math.Abs(db - _db_min) < 0.000001) drew_min = true;
                    if (Math.Abs(db - _db_max) < 0.000001) drew_max = true;

                    float y = yFromDb(plot, db);
                    g.DrawLine(tick_pen, plot.Left - _axis_tick_length, y, plot.Left, y);

                    string s = formatDbTick(db);
                    SizeF sz = g.MeasureString(s, Font);
                    float tx = plot.Left - _axis_tick_length - 4f - sz.Width;
                    float ty = y - (sz.Height * 0.5f);
                    g.DrawString(s, Font, text_brush, tx, ty);
                }

                if (!drew_min)
                {
                    double db = _db_min;
                    float y = yFromDb(plot, db);
                    g.DrawLine(tick_pen, plot.Left - _axis_tick_length, y, plot.Left, y);

                    string s = formatDbTick(db);
                    SizeF sz = g.MeasureString(s, Font);
                    float tx = plot.Left - _axis_tick_length - 4f - sz.Width;
                    float ty = y - (sz.Height * 0.5f);
                    g.DrawString(s, Font, text_brush, tx, ty);
                }

                if (!drew_max)
                {
                    double db = _db_max;
                    float y = yFromDb(plot, db);
                    g.DrawLine(tick_pen, plot.Left - _axis_tick_length, y, plot.Left, y);

                    string s = formatDbTick(db);
                    SizeF sz = g.MeasureString(s, Font);
                    float tx = plot.Left - _axis_tick_length - 4f - sz.Width;
                    float ty = y - (sz.Height * 0.5f);
                    g.DrawString(s, Font, text_brush, tx, ty);
                }

                double span = _frequency_max_hz - _frequency_min_hz;
                if (span <= 0.0) span = 1.0;

                double step_f = chooseFrequencyStep(span);
                double first = Math.Ceiling(_frequency_min_hz / step_f) * step_f;

                float tick_top = plot.Bottom;
                float tick_bottom = plot.Bottom + _axis_tick_length;

                float labels_y = plot.Bottom + _axis_tick_length + 2f;

                for (double f = first; f <= _frequency_max_hz + 0.000001; f += step_f)
                {
                    float x = xFromFreq(plot, f);
                    g.DrawLine(tick_pen, x, tick_top, x, tick_bottom);

                    string s = formatHzTick(f);
                    SizeF sz = g.MeasureString(s, Font);
                    float tx = x - (sz.Width * 0.5f);
                    float ty = labels_y;
                    g.DrawString(s, Font, text_brush, tx, ty);
                }
            }
        }

        private string formatDbTick(double db)
        {
            if (Math.Abs(db) < 0.000001) return "0";
            if (db > 0.0) return "+" + db.ToString("0");
            return db.ToString("0");
        }

        private string formatHzTick(double hz)
        {
            if (hz >= 1000.0) return (hz / 1000.0).ToString("0.#") + "k";
            return hz.ToString("0");
        }

        private double chooseFrequencyStep(double span)
        {
            double s = span;
            if (s <= 300.0) return 25.0;
            if (s <= 600.0) return 50.0;
            if (s <= 1200.0) return 100.0;
            if (s <= 2500.0) return 250.0;
            if (s <= 6000.0) return 500.0;
            if (s <= 12000.0) return 1000.0;
            if (s <= 24000.0) return 2000.0;
            return 5000.0;
        }

        private void drawBorder(Graphics g, Rectangle plot)
        {
            using (Pen border = new Pen(Color.FromArgb(70, 70, 70), 1f))
            {
                g.DrawRectangle(border, plot);
            }
        }

        private void drawReadout(Graphics g, Rectangle plot)
        {
            int readout_y = ClientRectangle.Bottom - (Font.Height + 4);
            int x = plot.Left;

            string s;

            if (_selected_index >= 0 && _selected_index < _points.Count)
            {
                EqPoint p = _points[_selected_index];
                int band_id = _selected_index + 1;

                if (_parametric_eq)
                {
                    s = "P" + band_id.ToString() +
                        "  F " + formatHz(p.FrequencyHz) +
                        "  G " + formatDb(p.GainDb) +
                        "  Q " + p.Q.ToString("0.00") +
                        "     Global " + formatDb(_global_gain_db);
                }
                else
                {
                    s = "P" + band_id.ToString() +
                        "  F " + formatHz(p.FrequencyHz) +
                        "  G " + formatDb(p.GainDb) +
                        "     Global " + formatDb(_global_gain_db);
                }
            }
            else
            {
                s = "Global " + formatDb(_global_gain_db);
            }

            using (SolidBrush tb = new SolidBrush(ForeColor))
            {
                g.DrawString(s, Font, tb, x, readout_y);
            }
        }

        private double responseDbAtFrequency(double frequency_hz)
        {
            if (!_parametric_eq)
            {
                if (_points.Count == 0) return 0.0;

                double f = frequency_hz;

                if (f <= _frequency_min_hz) return 0.0;
                if (f >= _frequency_max_hz) return 0.0;

                double left_f = _frequency_min_hz;
                double left_g = 0.0;

                for (int i = 0; i < _points.Count; i++)
                {
                    EqPoint p = _points[i];

                    double right_f = p.FrequencyHz;
                    double right_g = p.GainDb;

                    if (f <= right_f)
                    {
                        double denom = right_f - left_f;
                        if (denom <= 0.0000001) return right_g;

                        double t = (f - left_f) / denom;
                        if (t < 0.0) t = 0.0;
                        if (t > 1.0) t = 1.0;

                        return left_g + ((right_g - left_g) * t);
                    }

                    left_f = right_f;
                    left_g = right_g;
                }

                double last_f = _frequency_max_hz;
                double last_g = 0.0;

                double last_denom = last_f - left_f;
                if (last_denom <= 0.0000001) return last_g;

                double last_t = (f - left_f) / last_denom;
                if (last_t < 0.0) last_t = 0.0;
                if (last_t > 1.0) last_t = 1.0;

                return left_g + ((last_g - left_g) * last_t);
            }

            double span = _frequency_max_hz - _frequency_min_hz;
            if (span <= 0.0) span = 1.0;

            double sum = 0.0;

            for (int i = 0; i < _points.Count; i++)
            {
                EqPoint p = _points[i];

                double q = clamp(p.Q, _q_min, _q_max);

                double fwhm = span / (q * 3.0);
                double min_fwhm = span / 6000.0;
                if (fwhm < min_fwhm) fwhm = min_fwhm;

                double sigma = fwhm / 2.3548200450309493;
                double d = (frequency_hz - p.FrequencyHz) / sigma;
                double w = Math.Exp(-0.5 * d * d);

                sum += p.GainDb * w;
            }

            return sum;
        }

        private Color getBandBaseColor(int index)
        {
            int n = _default_band_palette.Length;
            if (n <= 0) return Color.FromArgb(200, 200, 200);
            int idx = index % n;
            if (idx < 0) idx = 0;
            return _default_band_palette[idx];
        }

        private string formatHz(double hz)
        {
            if (hz >= 1000.0) return (hz / 1000.0).ToString("0.###") + " kHz";
            return hz.ToString("0") + " Hz";
        }

        private string formatDb(double db)
        {
            string sign = db >= 0.0 ? "+" : "";
            return sign + db.ToString("0.0") + " dB";
        }

        private int hitTestPoint(Rectangle plot, Point pt)
        {
            int best = -1;
            double best_d2 = double.MaxValue;

            for (int i = 0; i < _points.Count; i++)
            {
                EqPoint p = _points[i];
                float x = xFromFreq(plot, p.FrequencyHz);
                float y = yFromDb(plot, p.GainDb);

                double dx = (double)pt.X - (double)x;
                double dy = (double)pt.Y - (double)y;
                double d2 = dx * dx + dy * dy;

                double r = (double)_hit_radius;
                if (d2 <= r * r && d2 < best_d2)
                {
                    best_d2 = d2;
                    best = i;
                }
            }

            return best;
        }

        private bool hitTestGlobalGainHandle(Rectangle plot, Point pt)
        {
            float y = yFromDb(plot, _global_gain_db);

            int hx = plot.Right + _global_handle_x_offset;
            int s = _global_handle_size;

            Rectangle r = new Rectangle(hx - _global_hit_extra,
                                        (int)Math.Round(y) - (s + _global_hit_extra),
                                        (s + _global_hit_extra) * 2,
                                        (s + _global_hit_extra) * 2);

            return r.Contains(pt);
        }

        private float xFromFreq(Rectangle plot, double frequency_hz)
        {
            double span = _frequency_max_hz - _frequency_min_hz;
            if (span <= 0.0) span = 1.0;
            double t = (frequency_hz - _frequency_min_hz) / span;
            if (t < 0.0) t = 0.0;
            if (t > 1.0) t = 1.0;
            return plot.Left + (float)(t * plot.Width);
        }

        private double freqFromX(Rectangle plot, int x)
        {
            double span = _frequency_max_hz - _frequency_min_hz;
            if (span <= 0.0) span = 1.0;
            double t = ((double)x - (double)plot.Left) / (double)plot.Width;
            if (t < 0.0) t = 0.0;
            if (t > 1.0) t = 1.0;
            return _frequency_min_hz + t * span;
        }

        private float yFromDb(Rectangle plot, double db)
        {
            double span = _db_max - _db_min;
            if (span <= 0.0) span = 1.0;
            double t = (db - _db_min) / span;
            return plot.Bottom - (float)(t * plot.Height);
        }

        private double dbFromY(Rectangle plot, int y)
        {
            double span = _db_max - _db_min;
            if (span <= 0.0) span = 1.0;
            double t = ((double)plot.Bottom - (double)y) / (double)plot.Height;
            if (t < 0.0) t = 0.0;
            if (t > 1.0) t = 1.0;
            return _db_min + t * span;
        }

        private double clamp(double v, double min, double max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        private void resetPointsDefault()
        {
            _points.Clear();

            int count = _band_count;
            if (count < 2) count = 2;

            _selected_index = -1;
            _drag_index = -1;
            _dragging_point = false;
            _dragging_global_gain = false;

            _drag_point_ref = null;
            _drag_dirty_point = false;
            _drag_dirty_global_gain = false;
            _drag_dirty_selected_index = false;

            double span = _frequency_max_hz - _frequency_min_hz;
            if (span <= 0.0) span = 1.0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / (double)(count - 1);
                double f = _frequency_min_hz + t * span;
                double g = 0.0;
                double q = 4.0;
                int band_id = i + 1;
                Color band_col = getBandBaseColor(i);
                _points.Add(new EqPoint(band_id, band_col, f, g, q));
            }

            enforceOrdering(true);
            clampAllGains();
            clampAllQ();
        }

        private void rescaleFrequencies(double old_min, double old_max, double new_min, double new_max)
        {
            double old_span = old_max - old_min;
            double new_span = new_max - new_min;

            if (old_span <= 0.0) old_span = 1.0;
            if (new_span <= 0.0) new_span = 1.0;

            for (int i = 0; i < _points.Count; i++)
            {
                EqPoint p = _points[i];

                double t = (p.FrequencyHz - old_min) / old_span;
                if (t < 0.0) t = 0.0;
                if (t > 1.0) t = 1.0;

                double f = new_min + t * new_span;
                p.FrequencyHz = f;
            }
        }

        private void enforceOrdering(bool enforce_spacing_all)
        {
            if (_points.Count == 0) return;

            EqPoint selected_point = null;
            if (_selected_index >= 0 && _selected_index < _points.Count) selected_point = _points[_selected_index];

            EqPoint drag_point = null;
            if (_drag_index >= 0 && _drag_index < _points.Count) drag_point = _points[_drag_index];

            if (_allow_point_reorder && _points.Count > 1)
            {
                _points.Sort((EqPoint a, EqPoint b) =>
                {
                    int c = a.FrequencyHz.CompareTo(b.FrequencyHz);
                    if (c != 0) return c;
                    return a.BandId.CompareTo(b.BandId);
                });
            }

            if (selected_point != null)
            {
                int new_selected_index = _points.IndexOf(selected_point);
                if (new_selected_index != _selected_index)
                {
                    _selected_index = new_selected_index;
                    raiseSelectedIndexChanged(isDraggingNow());
                }
            }
            else
            {
                if (_selected_index != -1)
                {
                    _selected_index = -1;
                    raiseSelectedIndexChanged(isDraggingNow());
                }
            }

            if (drag_point != null)
            {
                _drag_index = _points.IndexOf(drag_point);
            }
            else
            {
                _drag_index = -1;
            }

            for (int i = 0; i < _points.Count; i++)
            {
                EqPoint p = _points[i];
                p.FrequencyHz = clamp(p.FrequencyHz, _frequency_min_hz, _frequency_max_hz);
            }

            if (!enforce_spacing_all) return;
            if (_points.Count < 2) return;

            for (int i = 0; i < _points.Count; i++)
            {
                double min_f;
                double max_f;

                if (i == 0)
                {
                    min_f = _frequency_min_hz;
                    max_f = _points[1].FrequencyHz - _min_point_spacing_hz;
                }
                else if (i == _points.Count - 1)
                {
                    min_f = _points[_points.Count - 2].FrequencyHz + _min_point_spacing_hz;
                    max_f = _frequency_max_hz;
                }
                else
                {
                    min_f = _points[i - 1].FrequencyHz + _min_point_spacing_hz;
                    max_f = _points[i + 1].FrequencyHz - _min_point_spacing_hz;
                }

                if (max_f < min_f) max_f = min_f;

                EqPoint p = _points[i];
                p.FrequencyHz = clamp(p.FrequencyHz, min_f, max_f);
            }

            for (int i = 1; i < _points.Count; i++)
            {
                double want_min = _points[i - 1].FrequencyHz + _min_point_spacing_hz;
                if (_points[i].FrequencyHz < want_min) _points[i].FrequencyHz = want_min;
            }

            for (int i = _points.Count - 2; i >= 0; i--)
            {
                double want_max = _points[i + 1].FrequencyHz - _min_point_spacing_hz;
                if (_points[i].FrequencyHz > want_max) _points[i].FrequencyHz = want_max;
            }

            if (_points[0].FrequencyHz < _frequency_min_hz) _points[0].FrequencyHz = _frequency_min_hz;
            if (_points[_points.Count - 1].FrequencyHz > _frequency_max_hz) _points[_points.Count - 1].FrequencyHz = _frequency_max_hz;
        }

        private void clampAllGains()
        {
            for (int i = 0; i < _points.Count; i++)
            {
                EqPoint p = _points[i];
                p.GainDb = clamp(p.GainDb, _db_min, _db_max);
            }

            _global_gain_db = clamp(_global_gain_db, _db_min, _db_max);
        }

        private void clampAllQ()
        {
            for (int i = 0; i < _points.Count; i++)
            {
                EqPoint p = _points[i];
                p.Q = clamp(p.Q, _q_min, _q_max);
            }
        }

        private void raisePointsChanged(bool is_dragging)
        {
            EventHandler<EqDraggingEventArgs> h = PointsChanged;
            if (h != null) h(this, new EqDraggingEventArgs(is_dragging));
        }

        private void raiseGlobalGainChanged(bool is_dragging)
        {
            EventHandler<EqDraggingEventArgs> h = GlobalGainChanged;
            if (h != null) h(this, new EqDraggingEventArgs(is_dragging));
        }

        private void raiseSelectedIndexChanged(bool is_dragging)
        {
            if (is_dragging) _drag_dirty_selected_index = true;

            EventHandler<EqDraggingEventArgs> h = SelectedIndexChanged;
            if (h != null) h(this, new EqDraggingEventArgs(is_dragging));
        }

        private void raisePointDataChangedForPoint(EqPoint p, bool is_dragging)
        {
            if (p == null) return;

            int idx = _points.IndexOf(p);
            if (idx < 0) return;

            double q = _parametric_eq ? p.Q : 0.0;

            EventHandler<EqPointDataChangedEventArgs> h = PointDataChanged;
            if (h != null) h(this, new EqPointDataChangedEventArgs(idx, p.FrequencyHz, p.GainDb, q, is_dragging));
        }

        private int getComputedPlotMarginBottom()
        {
            if (_show_readout) return _plot_margin_bottom;

            int m = 8;

            if (_show_axis_scales)
            {
                m += _axis_tick_length + 2 + Font.Height + 4;
            }
            else
            {
                m += 8;
            }

            if (m < 10) m = 10;
            return m;
        }
    }
}