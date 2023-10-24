using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis
{
    public partial class ucProgress : UserControl
    {
        private int _value = 0;
        private int _min = 0;
        private int _max = 100;

        [Browsable(true)]
        [DefaultValue(0)]
        public int Value { get { return _value; } set { _value = Math.Max(_min, Math.Min(_max, value)); Refresh(); } }
        [Browsable(true)]
        [DefaultValue(100)]
        public int Maximum { get { return _max; } set { _max = value; Refresh(); } }
        [Browsable(true)]
        [DefaultValue(0)]
        public int Minimum { get { return _min; } set { _min = value; Refresh(); } }

        public ucProgress()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int barWidth = (int)(Width * (_value - _min) / (double)(_max - _min));

            using (Pen blackPen = new Pen(Color.Black, 1))
            using (Brush greenBrush = new SolidBrush(Color.Green))
            {
                e.Graphics.FillRectangle(greenBrush, 0, 0, barWidth, Height);
                e.Graphics.DrawRectangle(blackPen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
}
