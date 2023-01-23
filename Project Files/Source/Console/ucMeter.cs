using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Thetis
{
    public enum Axis
    {
        NONE = 0,
        LEFT,
        TOP,
        TOPLEFT
    }
    public partial class ucMeter : UserControl
    {
        [Browsable(true)]
        [Category("Action")]
        public event EventHandler FloatingDockedClicked;

        public event EventHandler DockedMoved;

        public ucMeter()
        {
            InitializeComponent();

            btnFloat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;

            _axisLock = Axis.NONE;
            _delta = new Point(0, 0);

            storeLocation();
            setFloatingDockButton();
            setTitle();
            setAxisButton();

            //btnFloat.foc
            //btnFloat.SetStyle(ControlStyles.Selectable, false);

            btnAxis.Hide();

            _cursor = Cursor.Current;
            pnlBar.Hide();
            pbGrab.Hide();
        }

        private bool _dragging = false;
        private bool _resizing = false;
        private bool _floating = false;
        private Point _point;
        private Point _clientPos;
        private Size _size;
        private Point _dockedLocation;
        private Size _dockedSize;
        private Cursor _cursor;
        private int _rx = 0;
        private Point _delta;
        private Axis _axisLock;

        private void pnlBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (_floating)
            {
                _point = Parent.PointToClient(Cursor.Position);
            }
            else
            {
                this.BringToFront();
                _point.X = e.X;
                _point.Y = e.Y;
            }
            _dragging = true;
        }

        private void pnlBar_MouseLeave(object sender, EventArgs e)
        {
            if (!_dragging && !pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition)))
                mouseLeave();
        }

        private void pnlBar_MouseUp(object sender, MouseEventArgs e)
        {
            _point = Point.Empty;
            _dragging = false;
            DockedMoved?.Invoke(this, e);
        }

        private void pnlBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point clientPos = Parent.PointToClient(Cursor.Position);

                int x = clientPos.X - _point.X;
                int y = clientPos.Y - _point.Y;

                if (_floating)
                {
                    Point newPos = new Point(Parent.Left + x, Parent.Top + y);
                    Parent.Location = newPos;
                }
                else
                {
                    if (x < 0) x = 0;
                    if (y < 0) y = 0;
                    if (x > Parent.ClientSize.Width - this.Width) x = Parent.ClientSize.Width - this.Width;
                    if (y > Parent.ClientSize.Height - this.Height) y = Parent.ClientSize.Height - this.Height;

                    Point newPos = new Point(x, y);
                    this.Location = newPos;
                }
            }
        }
        public PictureBox DisplayContainer
        {
            get { return picContainer; }
        }

        private void pbGrab_MouseDown(object sender, MouseEventArgs e)
        {
            _clientPos = Parent.PointToClient(Cursor.Position);
            _size.Width = this.Size.Width;
            _size.Height = this.Size.Height;
            _resizing = true;
            this.BringToFront();
        }

        private void pbGrab_MouseUp(object sender, MouseEventArgs e)
        {
            _clientPos = Point.Empty;
            _resizing = false;
        }

        private void pbGrab_MouseMove(object sender, MouseEventArgs e)
        {
            if (_resizing)
            {
                Point newPos = Parent.PointToClient(Cursor.Position);

                int dX = newPos.X - _clientPos.X;
                int dY = newPos.Y - _clientPos.Y;

                int x = _size.Width + dX;
                int y = _size.Height + dY;
                if (x < 80) x = 80;
                if (y < 80) y = 80;

                if (_floating)
                {
                    Size newSize = new Size(x, y);
                    Parent.Size = newSize;
                }
                else
                {
                    if (this.Left + x > Parent.ClientSize.Width) x = Parent.ClientSize.Width - this.Left;
                    if (this.Top + y > Parent.ClientSize.Height) y = Parent.ClientSize.Height - this.Top;

                    Size newSize = new Size(x, y);
                    this.Size = newSize;
                }
            }
        }
        public Point DockedLocation
        {
            get 
            {
                return _dockedLocation;
            }
            set {
                _dockedLocation = value;
            }
        }
        public Size DockedSize
        {
            get { return _dockedSize; }
            set { _dockedSize = value; }
        }
        private void storeLocation()
        {
            _dockedLocation = this.Location;
            //_dockedLocation = new Point(this.Location.X - _delta.X, this.Location.Y - _delta.Y);
            _dockedSize = this.Size;
        }
        public void RestoreLocation()
        {
            this.Location = _dockedLocation;
            //this.Location = new Point(_dockedLocation.X + _delta.X, _dockedLocation.Y + _delta.Y);
            this.Size = _dockedSize;
        }
        public bool Floating
        {
            get { return _floating; }
            set 
            { 
                _floating = value;
                btnAxis.Visible = !_floating;
                setFloatingDockButton();
            }
        }
        private void setFloatingDockButton()
        {
            if (_floating)
                btnFloat.BackgroundImage = Properties.Resources.dockIcon_dock;
            else
                btnFloat.BackgroundImage = Properties.Resources.dockIcon_float;
        }
        private void setTitle()
        {
            lblRX.Text = "RX" + _rx.ToString();
        }

        private void btnFloat_Click(object sender, EventArgs e)
        {
            FloatingDockedClicked?.Invoke(this, e);
        }

        private void pbGrab_MouseEnter(object sender, EventArgs e)
        {
            _cursor = Cursor.Current;

            Cursor = Cursors.SizeNWSE;
        }

        private void pbGrab_MouseLeave(object sender, EventArgs e)
        {
            Cursor = _cursor;

            if (!_resizing && !pbGrab.ClientRectangle.Contains(picContainer.PointToClient(Control.MousePosition)))
                mouseLeave();
        }

        private void picContainer_MouseMove(object sender, MouseEventArgs e)
        {
            bool bContains;

            if (!_dragging)
            {
                bContains = pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition));
                if (bContains && !pnlBar.Visible)
                {
                    pnlBar.BringToFront();
                    pnlBar.Show();
                }
                else if (!bContains && pnlBar.Visible)
                {
                    pnlBar.Hide();
                }
            }

            if (!_resizing)
            {
                bContains = pbGrab.ClientRectangle.Contains(pbGrab.PointToClient(Control.MousePosition));
                if (bContains && !pbGrab.Visible)
                {
                    pbGrab.BringToFront();
                    pbGrab.Show();
                }
                else if (!bContains && pbGrab.Visible)
                {
                    pbGrab.Hide();
                }
            }
        }

        private void picContainer_MouseLeave(object sender, EventArgs e)
        {
            if (!(_dragging || _resizing) && !picContainer.ClientRectangle.Contains(picContainer.PointToClient(Control.MousePosition)))
                mouseLeave();
        }
        private void mouseLeave()
        {
            if (pnlBar.Visible)
                pnlBar.Hide();
            if (pbGrab.Visible)
                pbGrab.Hide();
        }

        private void btnFloat_MouseLeave(object sender, EventArgs e)
        {
            if (!_dragging && !pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition)))
                mouseLeave();
        }
        public int RX
        {
            get { return _rx; }
            set 
            {
                _rx = value;
                setTitle();
            }
        }

        private void lblRX_MouseDown(object sender, MouseEventArgs e)
        {
            if (_floating)
            {
                _point = Parent.PointToClient(Cursor.Position);
            }
            else
            {
                this.BringToFront();
                _point.X = e.X;
                _point.Y = e.Y;
            }
            _dragging = true;
        }

        private void lblRX_MouseUp(object sender, MouseEventArgs e)
        {
            _point = Point.Empty;
            _dragging = false;
            DockedMoved?.Invoke(this, e);
        }

        private void lblRX_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point clientPos = Parent.PointToClient(Cursor.Position);

                int x = clientPos.X - _point.X;
                int y = clientPos.Y - _point.Y;

                if (_floating)
                {
                    Point newPos = new Point(Parent.Left + x, Parent.Top + y);
                    Parent.Location = newPos;
                }
                else
                {
                    if (x < 0) x = 0;
                    if (y < 0) y = 0;
                    if (x > Parent.ClientSize.Width - this.Width) x = Parent.ClientSize.Width - this.Width;
                    if (y > Parent.ClientSize.Height - this.Height) y = Parent.ClientSize.Height - this.Height;

                    Point newPos = new Point(x, y);
                    this.Location = newPos;
                }
            }
        }

        private void lblRX_MouseLeave(object sender, EventArgs e)
        {
            if (!_dragging && !pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition)))
                mouseLeave();
        }

        private void ucMeter_LocationChanged(object sender, EventArgs e)
        {
            if (!_floating && _dragging)
            {
                _dockedLocation = this.Location;
            }
        }

        private void ucMeter_SizeChanged(object sender, EventArgs e)
        {
            if (!_floating && _resizing)
            {
                _dockedSize = this.Size;
            }
        }

        public Point Delta
        {
            get { return _delta; }
            set { _delta = value; } 
        }

        public Axis AxisLock
        {
            get { return _axisLock; }
            set 
            { 
                _axisLock = value; 
            }
        }

        private void btnAxis_Click(object sender, EventArgs e)
        {
            int n = (int)_axisLock;
            n++;
            if (n > (int)Axis.TOPLEFT) n = (int)Axis.NONE;

            _axisLock = (Axis)n;

            setAxisButton();
        }
        private void setAxisButton()
        {
            switch (_axisLock)
            {
                case Axis.NONE:
                    btnAxis.BackgroundImage = Properties.Resources.dot;
                    break;
                case Axis.LEFT:
                    btnAxis.BackgroundImage = Properties.Resources.arrow_left;
                    break;
                case Axis.TOP:
                    btnAxis.BackgroundImage = Properties.Resources.arrow_up;
                    break;
                case Axis.TOPLEFT:
                    btnAxis.BackgroundImage = Properties.Resources.arrow_topleft;
                    break;
            }
        }
    }
}

