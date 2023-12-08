using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public enum Axis
    {
        LEFT = 0,
        TOPLEFT,
        TOP,
        TOPRIGHT,
        RIGHT,
        BOTTOMRIGHT,
        BOTTOM,
        BOTTOMLEFT
    }
    public partial class ucMeter : UserControl
    {
        [Browsable(true)]
        [Category("Action")]
        public event EventHandler FloatingDockedClicked;

        [Browsable(true)]
        [Category("Action")]
        public event EventHandler SettingsClicked;

        public event EventHandler DockedMoved;

        public ucMeter()
        {
            InitializeComponent();

            picContainer.Location = new Point(0, 0);
            picContainer.Size = new Size(Size.Width, Size.Height);

            _console = null;
            _id = System.Guid.NewGuid().ToString();
            _border = true;
            _noTitleBar = false;
            _enabled = true;

            btnFloat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;

            _axisLock = Axis.TOPLEFT;
            _delta = new Point(0, 0);
            _pinOnTop = false;

            storeLocation();
            setTopBarButtons();
            setTitle();
            setupBorder();

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
        private bool _pinOnTop;
        private Console _console;
        private bool _mox;
        private string _id;
        private bool _border;
        private bool _noTitleBar;
        private bool _enabled;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Console Console
        {
            set 
            {
                _console = value;
                if (_console == null) return;

                _mox = (_console.RX2Enabled && _console.VFOBTX && _console.MOX) || (!_console.RX2Enabled && _console.MOX);
                setTitle();

                addDelegates();
            }
        }
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }
        private void addDelegates()
        {
            if (_console == null) return;

            _console.MoxChangeHandlers += OnMoxChangeHandler;
        }
        public void RemoveDelegates()
        {
            if (_console == null) return;

            _console.MoxChangeHandlers -= OnMoxChangeHandler;
        }
        private void OnMoxChangeHandler(int rx, bool oldMox, bool newMox)
        {
            if (rx != _rx) return;

            _mox = newMox;
            setTitle();
        }
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
            uiComponentMouseLeave();
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
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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
                if (x < 100) x = 100; // these match max size of parent when floating
                if (y < 32) y = 32;

                if (_floating)
                {
                    Parent.Size = new Size(x, y);
                }
                else
                {
                    if (this.Left + x > Parent.ClientSize.Width) x = Parent.ClientSize.Width - this.Left;
                    if (this.Top + y > Parent.ClientSize.Height) y = Parent.ClientSize.Height - this.Top;

                    this.Size = new Size(x, y);
                }
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void RestoreLocation()
        {
            this.Location = _dockedLocation;
            //this.Location = new Point(_dockedLocation.X + _delta.X, _dockedLocation.Y + _delta.Y);
            this.Size = _dockedSize;
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool Floating
        {
            get { return _floating; }
            set 
            { 
                _floating = value;
                setTopBarButtons();
                setTopMost();
            }
        }
        private void setTopBarButtons()
        {
            if (_floating)
            {
                btnFloat.BackgroundImage = Properties.Resources.dockIcon_dock;
                btnPin.Left = btnAxis.Left; // move to fill the gap
                btnAxis.Visible = false;
                btnPin.Visible = true;
            }
            else
            {
                btnFloat.BackgroundImage = Properties.Resources.dockIcon_float;
                btnPin.Left = btnAxis.Left - btnPin.Width; // put back (dont really need to as invis)
                btnAxis.Visible = true;
                btnPin.Visible = false;
            }

            setAxisButton();
            setPinOnTopButton();
        }
        private void setTitle()
        {
            string sPrefix = _mox ? "TX" : "RX";
            lblRX.Text = sPrefix + _rx.ToString();
        }
        private void setupBorder()
        {
            this.BorderStyle = _border ? BorderStyle.FixedSingle : BorderStyle.None;
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

            if (!_resizing && (!pbGrab.ClientRectangle.Contains(pbGrab.PointToClient(Control.MousePosition)) || !this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))) //[2.10.3.4]MW0LGE added 'this' incase we are totally outside, fix issue where ui items get left visible
                mouseLeave();
        }

        private void picContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
            {
                bool noBar = _noTitleBar && !Common.ShiftKeyDown; //[2.10.3.4]MW0LGE no title, override by holding shift
                bool bContains = !noBar && pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition));
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
                bool bContains = pbGrab.ClientRectangle.Contains(pbGrab.PointToClient(Control.MousePosition));
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
            uiComponentMouseLeave();
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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
            uiComponentMouseLeave();
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
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Point Delta
        {
            get { return _delta; }
            set { _delta = value; } 
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool PinOnTop
        {
            get { return _pinOnTop; }
            set { 
                _pinOnTop = value;
                setPinOnTopButton();
                setTopMost();
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Axis AxisLock
        {
            get { return _axisLock; }
            set 
            { 
                _axisLock = value; 
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool UCBorder
        {
            get { return _border; }
            set 
            { 
                _border = value;
                setupBorder();
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool MeterEnabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;                
            }
        }
        public bool NoTitle
        {
            get { return _noTitleBar; }
            set
            {
                _noTitleBar = value;
            }
        }
        private void btnAxis_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;

            int n = (int)_axisLock;
            if (me.Button == MouseButtons.Right)
                n--;
            else
                n++;

            if (n > (int)Axis.BOTTOMLEFT) n = (int)Axis.LEFT;
            if (n < (int)Axis.LEFT) n = (int)Axis.BOTTOMLEFT;

            _axisLock = (Axis)n;

            setAxisButton();

            // reset this data when the lock is changed
            if(_console != null)
            {
                Delta = new Point(_console.HDelta, _console.VDelta);
                DockedLocation = new Point(this.Left, this.Top);
            }
        }
        private void setAxisButton()
        {
            switch (_axisLock)
            {
                //case Axis.NONE:
                //    btnAxis.BackgroundImage = Properties.Resources.dot;
                //    break;
                case Axis.LEFT:
                    btnAxis.BackgroundImage = Properties.Resources.arrow_left;
                    break;
                case Axis.TOPLEFT:
                    btnAxis.BackgroundImage = Properties.Resources.arrow_topleft;
                    break;
                case Axis.TOP:
                    btnAxis.BackgroundImage = Properties.Resources.arrow_up;
                    break;
                case Axis.TOPRIGHT:
                    btnAxis.BackgroundImage = Properties.Resources.arrow_topright;
                    break;
                case Axis.RIGHT:
                    btnAxis.BackgroundImage = Properties.Resources.arrow_right;
                    break;
                case Axis.BOTTOMRIGHT:
                    btnAxis.BackgroundImage = Properties.Resources.arrow_bottomright;
                    break;
                case Axis.BOTTOM:
                    btnAxis.BackgroundImage = Properties.Resources.down;
                    break;
                case Axis.BOTTOMLEFT:
                    btnAxis.BackgroundImage = Properties.Resources.arrow_bottomleft;
                    break;
            }
        }
        private void setPinOnTopButton()
        {
            btnPin.BackgroundImage = _pinOnTop ? Properties.Resources.pin_on_top : Properties.Resources.pin_not_on_top;
        }

        private void btnPin_Click(object sender, EventArgs e)
        {
            _pinOnTop = !_pinOnTop;
            setPinOnTopButton();
            setTopMost();
        }
        private void setTopMost()
        {
            if (_floating)
            {
                frmMeterDisplay md = this.Parent as frmMeterDisplay;
                if (md != null)
                {
                    md.TopMost = _pinOnTop;
                }
            }
        }
        public override string ToString()
        {
            return
                ID + "|" +
                RX.ToString() + "|" +
                DockedLocation.X.ToString() + "|" +
                DockedLocation.Y.ToString() + "|" +
                DockedSize.Width.ToString() + "|" +
                DockedSize.Height.ToString() + "|" +
                Floating.ToString() + "|" +
                Delta.X.ToString() + "|" +
                Delta.Y.ToString() + "|" +
                AxisLock.ToString() + "|" +
                PinOnTop.ToString() + "|" +
                UCBorder.ToString() + "|" +
                Common.ColourToString(this.BackColor) + "|" +
                NoTitle.ToString() + "|" +
                MeterEnabled.ToString();
        }
        public bool TryParse(string str)
        {
            bool bOk = false;
            int x = 0, y = 0, w = 0, h = 0, rx = 0;
            bool floating = false;
            bool pinOnTop = false;
            bool border = false;
            bool noTitleBar = false;
            bool enabled = true;

            if (str != "")
            {
                string[] tmp = str.Split('|');
                if(tmp.Length >= 13 && tmp.Length <= 15)
                {
                    bOk = tmp[0] != "";
                    if (bOk) ID = tmp[0];
                    if (bOk) int.TryParse(tmp[1], out rx);
                    if (bOk) RX = rx;
                    if (bOk) bOk = int.TryParse(tmp[2], out x);
                    if (bOk) bOk = int.TryParse(tmp[3], out y);
                    if (bOk) bOk = int.TryParse(tmp[4], out w);
                    if (bOk) bOk = int.TryParse(tmp[5], out h);
                    if (bOk)
                    {
                        DockedLocation = new Point(x, y);
                        DockedSize = new Size(w, h);
                    }

                    if (bOk) bOk = bool.TryParse(tmp[6], out floating);
                    if (bOk) Floating = floating;

                    if (bOk) bOk = int.TryParse(tmp[7], out x);
                    if (bOk) bOk = int.TryParse(tmp[8], out y);
                    if (bOk) Delta = new Point(x, y);

                    if (bOk)
                    {
                        try
                        {
                            AxisLock = (Axis)Enum.Parse(typeof(Axis), tmp[9]);
                        }
                        catch
                        {
                            bOk = false;
                        }
                    }

                    if (bOk) bOk = bool.TryParse(tmp[10], out pinOnTop);
                    if (bOk) PinOnTop = pinOnTop;
                    if (bOk) bOk = bool.TryParse(tmp[11], out border);
                    if (bOk) UCBorder = border;
                    Color c = Common.ColourFromString(tmp[12]);
                    bOk = c != System.Drawing.Color.Empty;
                    if(bOk) this.BackColor = c;

                    if(bOk && tmp.Length > 13) // we also have the new for [2.10.1.0] the notitleifpined option
                    {
                        bOk = bool.TryParse(tmp[13], out noTitleBar);
                        if (bOk) NoTitle = noTitleBar;
                    }

                    if (bOk && tmp.Length > 14) // we also have the new for [2.10.3.5] the show option
                    {
                        bOk = bool.TryParse(tmp[14], out enabled);
                        if (bOk) MeterEnabled = enabled;
                    }
                }
            }

            return bOk;
        }
        private void btnAxis_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) btnAxis_Click(sender, e);
        }

        private void btnAxis_MouseLeave(object sender, EventArgs e)
        {
            uiComponentMouseLeave();
        }

        private void btnPin_MouseLeave(object sender, EventArgs e)
        {
            uiComponentMouseLeave();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            SettingsClicked?.Invoke(this, e);
        }

        private void btnSettings_MouseLeave(object sender, EventArgs e)
        {
            uiComponentMouseLeave();
        }

        private void uiComponentMouseLeave()
        {
            if (!_dragging && (!pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition)) || !this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))) //[2.10.3.4]MW0LGE added 'this' incase we are totally outside, fix issue where ui items get left visible
                mouseLeave();
        }

        private void ucMeter_MouseLeave(object sender, EventArgs e)
        {
            if (!(_dragging || _resizing) && !picContainer.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
                mouseLeave();
        }
    }
}

