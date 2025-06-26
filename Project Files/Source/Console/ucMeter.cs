/*  ucMeter.cs

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
using System;
using System.ComponentModel;
using System.Diagnostics;
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
        public const int MIN_CONTAINER_WIDTH = 24;
        public const int MIN_CONTAINER_HEIGHT = 24;

        [Browsable(true)]
        [Category("Action")]
        public event EventHandler FloatingDockedClicked;

        [Browsable(true)]
        [Category("Action")]
        public event EventHandler SettingsClicked;

        public event EventHandler DockedMoved;

        private int _sequence;
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
        private bool _no_controls;
        private bool _locked;
        private bool _enabled;
        private bool _show_on_rx;
        private bool _show_on_tx;
        private bool _container_minimises;
        private bool _container_hides_when_rx_not_used;
        private string _notes;
        private int _height;
        private bool _autoHeight;
        private ToolTip _tool_tip;
        private IWin32Window _tool_tip_owner;
        private Guid _touch_guid;

        public ucMeter()
        {
            InitializeComponent();

            Common.DoubleBufferAll(this, true);

            _sequence = 0;

            pnlContainer.Location = new Point(0, 0);
            pnlContainer.Size = new Size(ClientSize.Width, ClientSize.Height);

            _height = MIN_CONTAINER_HEIGHT;
            _autoHeight = false;

            _touch_guid = Guid.Empty;
            _console = null;
            _id = System.Guid.NewGuid().ToString();
            _border = true;
            _no_controls = false;
            _enabled = true;
            _show_on_rx = true;
            _show_on_tx = true;
            _container_minimises = true;
            _container_hides_when_rx_not_used = true;
            _notes = "";

            _tool_tip = new ToolTip();
            _tool_tip_owner = null;
            _tool_tip.InitialDelay = 0;
            _tool_tip.ReshowDelay = 0;
            _tool_tip.UseFading = true;
            _tool_tip.ShowAlways = true;

            this.Name = "UCMeter_" + _id;

            btnFloat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;

            _axisLock = Axis.TOPLEFT;
            _delta = new Point(0, 0);
            _pinOnTop = false;

            storeLocation();
            setTopBarButtons();
            setTitle();
            setupBorder();

            btnAxis.BringToFront();
            btnFloat.BringToFront();
            btnPin.BringToFront();
            btnSettings.BringToFront();

            btnAxis.Hide();

            _cursor = Cursor.Current;
            pnlBar.Hide();
            pbGrab.Hide();
        }
        ~ucMeter()
        {
            if (_touch_guid != Guid.Empty) TouchHandler.DisableTouchSupport(_touch_guid);
        }
        private void HandleTouchDown(int x, int y)
        {
            if (pnlBar.Visible && pnlBar.Bounds.Contains(x, y))
            {
                pnlBar_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
            }
            else if (pbGrab.Visible && pbGrab.Bounds.Contains(x, y))
            {
                pbGrab_MouseEnter(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
                pbGrab_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
            }
            else if (lblRX.Visible && lblRX.Bounds.Contains(x, y))
            {
                lblRX_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
            }
            else if (pnlContainer.Bounds.Contains(x, y))
            {
                pnlContainer_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
            }
        }
        private void HandleTouchMove(int x, int y)
        {
            if (pnlBar.Visible && pnlBar.Bounds.Contains(x, y))
            {
                pnlBar_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
            }
            else if (pbGrab.Visible && pbGrab.Bounds.Contains(x, y))
            {
                pbGrab_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
            }
            else if (lblRX.Visible && lblRX.Bounds.Contains(x, y))
            {
                lblRX_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
            }
            if (pnlContainer.Bounds.Contains(x, y))
            {
                pnlContainer_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
            }
        }
        private void HandleTouchUp(int x, int y)
        {
            if (pnlBar.Visible && pnlBar.Bounds.Contains(x, y))
            {
                pnlBar_MouseUp(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
                pnlBar_MouseLeave(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
            }
            else if (pbGrab.Visible && pbGrab.Bounds.Contains(x, y))
            {
                pbGrab_MouseUp(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
                pbGrab_MouseLeave(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
            }
            else if (lblRX.Visible && lblRX.Bounds.Contains(x, y))
            {
                lblRX_MouseUp(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
                lblRX_MouseLeave(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
            }
            else if (pnlContainer.Bounds.Contains(x, y))
            {
                pnlContainer_MouseLeave(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Console Console
        {
            set
            {
                _console = value;
                if (_console == null) return;

                if (_touch_guid != Guid.Empty) TouchHandler.DisableTouchSupport(_touch_guid);

                if (_console.TouchSupport)
                    _touch_guid = TouchHandler.EnableTouchSupport(pnlContainer, HandleTouchDown, HandleTouchMove, HandleTouchUp, TouchHandler.TOUCHEVENTF_DOWN | TouchHandler.TOUCHEVENTF_MOVE | TouchHandler.TOUCHEVENTF_UP);
                else
                    _touch_guid = Guid.Empty;

                _mox = (_console.RX2Enabled && _console.VFOBTX && _console.MOX) || (!_console.RX2Enabled && _console.MOX);
                setTitle();

                addDelegates();
            }
        }
        public int Sequence
        {
            get { return _sequence; }
            set { _sequence = value; }
        }
        public string ID
        {
            get { return _id; }
            set { _id = value.Replace("|", ""); }
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
        public void Repaint()
        {
            if (_floating) return;
            if (this.Parent != null)
            {
                this.Parent.Invalidate(this.Bounds, true);
                this.Parent.Update();
            }
        }
        private void pnlBar_MouseLeave(object sender, EventArgs e)
        {
            uiComponentMouseLeave();
        }

        private void pnlBar_MouseUp(object sender, MouseEventArgs e)
        {
            _point = Point.Empty;
            _dragging = false;
            hideToolTip();
            DockedMoved?.Invoke(this, e);
        }

        private void pnlBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {                
                if (_floating)
                {
                    Point clientPos = Parent.PointToClient(Cursor.Position);

                    int x = clientPos.X - _point.X;
                    int y = clientPos.Y - _point.Y;

                    Point newPos = new Point(Parent.Left + x, Parent.Top + y);

                    if (Common.CtrlKeyDown)
                    {
                        newPos.X = roundToNearestTen(newPos.X);
                        newPos.Y = roundToNearestTen(newPos.Y);
                    }

                    if (this.Parent != null)
                    {
                        if (newPos != Parent.Location)
                        {
                            Parent.Location = newPos;
                            showToolTip($"{newPos.X}, {newPos.Y}", this.Parent);
                        }                        
                    }
                }
                else
                {
                    Point clientPos = e.Location;

                    int x = clientPos.X - _point.X;
                    int y = clientPos.Y - _point.Y;

                    x += this.Location.X;
                    y += this.Location.Y;

                    if (Common.CtrlKeyDown)
                    {
                        x = roundToNearestTen(x);
                        y = roundToNearestTen(y);
                    }

                    if (x < 0) x = 0;
                    if (y < 0) y = 0;
                    if (x > Parent.ClientSize.Width - this.Width) x = Parent.ClientSize.Width - this.Width;
                    if (y > Parent.ClientSize.Height - this.Height) y = Parent.ClientSize.Height - this.Height;

                    Point newPos = new Point(x, y);
                    if (newPos != this.Location)
                    {
                        this.Location = newPos;
                        Repaint();
                        showToolTip($"{newPos.X}, {newPos.Y}", this);
                    }                    
                }
            }
        }
        private void showToolTip(string msg, Control window, bool is_resize = false)
        {
            _tool_tip_owner = window;
            if(is_resize)
                _tool_tip.Show(msg + "\nctrl to lock", _tool_tip_owner, new Point(window.Size.Width, window.Size.Height - (pnlBar.Height * 2)));
            else
                _tool_tip.Show(msg + "\nctrl to lock", _tool_tip_owner, new Point(0, pnlBar.Height));
        }
        private void hideToolTip()
        {
            if (_tool_tip == null || _tool_tip_owner == null) return;
            _tool_tip.Hide(_tool_tip_owner);
            _tool_tip_owner = null;
        }
        private int roundToNearestTen(int number)
        {
            return ((number + 5) / 10) * 10;
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Panel DisplayContainer
        {
            get { return pnlContainer; }
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
            hideToolTip();
            forceResize();
        }
        private void forceResize(bool shrink = false)
        {
            _resizing = true;
            if (_autoHeight)
            {
                if(_floating)
                {
                    if (this.Parent != null)
                    {
                        if (this.Parent.ClientSize.Height != _height)
                        {
                            resize(this.Parent.ClientSize.Width, _height, shrink);
                        }
                    }
                }
                else
                {
                    if (this.Size.Height != _height)
                        resize(this.Size.Width, _height);
                }
            }
            else
            {
                if (_floating)
                {
                    if (this.Parent != null)
                    {
                        resize(this.Parent.ClientSize.Width, this.Parent.ClientSize.Height, shrink);
                    }
                }
                else
                    resize(this.Size.Width, this.Size.Height);
            }
            _resizing = false;
        }
        public void ChangeHeight(int height)
        {
            if (!_autoHeight) return;
            _height = height;
            if (_resizing) return;
            if (_dragging) return;

            if (_floating)
            {
                if (this.Parent != null && this.Parent.IsHandleCreated)
                {
                    bool shrink = false;
                    int screenHeight = Screen.FromControl(this.Parent).WorkingArea.Height;
                    if (screenHeight < height)
                    {
                        Point newPos = new Point(this.Parent.Location.X, 0);
                        if(newPos != this.Parent.Location) this.Parent.Location = newPos;
                        shrink = true;
                    }

                    height = Math.Min(height, screenHeight);

                    if (this.Parent.ClientSize.Height != height)
                    {
                        _height = height;
                        forceResize(shrink);
                    }
                }
            }
            else
            {
                if (this.Size.Height != height)
                {
                    //_height = height;
                    forceResize();
                }
            }
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

                if (Common.CtrlKeyDown)
                {
                    x = roundToNearestTen(x);
                    y = roundToNearestTen(y);
                }

                resize(x, y);

                if (_floating)
                {
                    if (this.Parent != null)
                    {
                        showToolTip($"{this.Parent.Size.Width}, {this.Parent.Size.Height}", this.Parent, true);
                    }
                }
                else
                    showToolTip($"{this.Size.Width}, {this.Size.Height}", this, true);
            }
        }
        private void resize(int x, int y, bool shrink = false)
        {
            if(Parent == null) return;

            if (x < MIN_CONTAINER_WIDTH) x = MIN_CONTAINER_WIDTH; // these match max size of parent when floating
            if (y < MIN_CONTAINER_HEIGHT) y = MIN_CONTAINER_HEIGHT;

            if (_floating)
            {
                Parent.ClientSize = new Size(x, y);
                Parent.PerformLayout();
                if (this.Parent != null)
                {
                    (bool was_relocated, bool was_shrunk) = Common.ForceFormOnScreen((Form)this.Parent, shrink);
                }
            }
            else
            {
                if (this.Left + x > Parent.ClientSize.Width) x = Parent.ClientSize.Width - this.Left;
                if (this.Top + y > Parent.ClientSize.Height) y = Parent.ClientSize.Height - this.Top;

                Size newSize = new Size(x, y);
                if (newSize != this.Size)
                {
                    this.Size = newSize;
                    this.PerformLayout();
                    Repaint();
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
            bool moved = false;
            if (_dockedLocation != this.Location)
            {
                this.Location = _dockedLocation;
                moved = true;
            }
            //this.Location = new Point(_dockedLocation.X + _delta.X, _dockedLocation.Y + _delta.Y);
            if (_dockedSize != this.Size)
            {
                this.Size = _dockedSize;
                moved = true;
            }
            if (moved)
            {
                this.PerformLayout();
                Repaint();
            }
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
            string sNotes = getFirstLineOrWholeString(_notes);
            lblRX.Text = sPrefix + _rx.ToString() + (sNotes != "" ? " " + sNotes : "");
        }
        private string getFirstLineOrWholeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            return lines[0];
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
            hideToolTip();
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

                    if (Common.CtrlKeyDown)
                    {
                        newPos.X = roundToNearestTen(newPos.X);
                        newPos.Y = roundToNearestTen(newPos.Y);
                    }

                    if(newPos != Parent.Location) Parent.Location = newPos;

                    if (this.Parent != null)
                    {
                        showToolTip($"{newPos.X}, {newPos.Y}", this.Parent);
                    }
                }
                else
                {
                    x -= lblRX.Left;
                    y -= lblRX.Top;

                    if (Common.CtrlKeyDown)
                    {
                        x = roundToNearestTen(x);
                        y = roundToNearestTen(y);
                    }

                    if (x < 0) x = 0;
                    if (y < 0) y = 0;
                    if (x > Parent.ClientSize.Width - this.Width) x = Parent.ClientSize.Width - this.Width;
                    if (y > Parent.ClientSize.Height - this.Height) y = Parent.ClientSize.Height - this.Height;

                    Point newPos = new Point(x, y);
                    if (newPos != this.Location)
                    {
                        this.Location = newPos;
                        Repaint();
                    }

                    showToolTip($"{newPos.X}, {newPos.Y}", this);
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
                if(_dockedLocation != this.Location) _dockedLocation = this.Location;
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
        public bool Locked
        {
            get { return _locked; }
            set
            {
                _locked = value;
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
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShowOnRX
        {
            get { return _show_on_rx; }
            set
            {
                _show_on_rx = value;
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShowOnTX
        {
            get { return _show_on_tx; }
            set
            {
                _show_on_tx = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool ContainerMinimises
        {
            get { return _container_minimises; }
            set
            {
                _container_minimises = value;
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool ContainerHidesWhenRXNotUsed
        {
            get { return _container_hides_when_rx_not_used; }
            set
            {
                _container_hides_when_rx_not_used = value;
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string Notes
        {
            get { return _notes; }
            set
            {
                _notes = value.Replace("|",""); // need to replace this as used in split parsing
                setTitle();
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool NoControls
        {
            get { return _no_controls; }
            set
            {
                _no_controls = value;
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool AutoHeight
        {
            get { return _autoHeight; }
            set
            {
                _autoHeight = value;
                if (_autoHeight && !_resizing) forceResize();
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
                if (this.Parent != null)
                {
                    frmMeterDisplay md = this.Parent as frmMeterDisplay;
                    if (md != null)
                    {
                        md.TopMost = _pinOnTop;
                    }
                }
            }
        }
        public bool IsTopMost
        {
            get
            {
                if (this.Parent != null)
                {
                    frmMeterDisplay md = this.Parent as frmMeterDisplay;
                    if (md != null)
                    {
                        return md.TopMost;
                    }
                    else
                        return false;
                }
                else
                    return false;
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
                Floating.ToString().ToLower() + "|" +
                Delta.X.ToString() + "|" +
                Delta.Y.ToString() + "|" +
                AxisLock.ToString() + "|" +
                PinOnTop.ToString().ToLower() + "|" +
                UCBorder.ToString().ToLower() + "|" +
                Common.ColourToString(this.BackColor) + "|" +
                NoControls.ToString().ToLower() + "|" +
                MeterEnabled.ToString().ToLower() + "|" +
                Notes + "|" +
                ContainerMinimises.ToString().ToLower() + "|" +
                AutoHeight.ToString().ToLower() + "|" +
                ShowOnRX.ToString().ToLower() + "|" +
                ShowOnTX.ToString().ToLower() + "|" +
                Locked.ToString().ToLower() + "|" +
                ContainerHidesWhenRXNotUsed.ToString().ToLower();
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
            bool minimises = true;
            bool auto_height = false;
            bool show_on_rx = true;
            bool show_on_tx = true;
            bool locked = false;
            bool hides_when_not_used = true;

            if (str != "")
            {
                string[] tmp = str.Split('|');
                if(tmp.Length >= 13)// && tmp.Length <= 21)  //[2.10.3.6_rc4] MW0LGE removed so that clients going forward can use older data as long as 13 entries exist
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
                        if (bOk) NoControls = noTitleBar;
                    }

                    if (bOk && tmp.Length > 14) // we also have the new for [2.10.3.5] the show option
                    {
                        bOk = bool.TryParse(tmp[14], out enabled);
                        if (bOk) MeterEnabled = enabled;
                    }

                    if (bOk && tmp.Length > 15) // we also have the new for [2.10.3.6] notes
                    {
                        Notes = tmp[15];
                    }

                    if (bOk && tmp.Length > 16) // we also have the new for [2.10.3.6]
                    {
                        if (bOk) bOk = bool.TryParse(tmp[16], out minimises);
                        if (bOk) ContainerMinimises = minimises;
                    }

                    if(bOk && tmp.Length > 17) // also auto height for [2.10.3.6]
                    {
                        bOk = bool.TryParse(tmp[17], out auto_height);
                        if (bOk) AutoHeight = auto_height;
                    }

                    if (bOk && tmp.Length > 18) // also showonrx and showontx for [2.10.3.6]
                    {
                        bOk = bool.TryParse(tmp[18], out show_on_rx);
                        if (bOk) ShowOnRX = show_on_rx;
                        bOk = bool.TryParse(tmp[19], out show_on_tx);
                        if (bOk) ShowOnTX = show_on_tx;
                    }

                    if(bOk && tmp.Length > 20) // also for [2.10.3.6]
                    {
                        bOk = bool.TryParse(tmp[20], out locked);
                        if (bOk) Locked = locked;
                    }

                    if (bOk && tmp.Length > 21) // for [2.10.3.9]
                    {
                        bOk = bool.TryParse(tmp[21], out hides_when_not_used);
                        if (bOk) ContainerHidesWhenRXNotUsed = hides_when_not_used;
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
            if (!(_dragging || _resizing) && !pnlContainer.ClientRectangle.Contains(this.PointToClient(Control.MousePosition)))
                mouseLeave();
        }

        private void pnlContainer_MouseMove(object sender, MouseEventArgs e)
        {
            bool no_controls = _no_controls && !Common.ShiftKeyDown; //[2.10.3.6]MW0LGE no title or resize grabber, override by holding shift

            if (!_dragging)
            {
                bool bContains = !no_controls && pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition));
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
                bool bContains = !no_controls && pbGrab.ClientRectangle.Contains(pbGrab.PointToClient(Control.MousePosition));
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

        private void pnlContainer_MouseLeave(object sender, EventArgs e)
        {
            if (!(_dragging || _resizing) && !pnlContainer.ClientRectangle.Contains(pnlContainer.PointToClient(Control.MousePosition)))
                mouseLeave();
        }
    }
}

