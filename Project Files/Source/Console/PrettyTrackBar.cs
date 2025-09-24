//=================================================================
// PrettyTrackBar.cs
//=================================================================
// PrettyTrackBar implements a TrackBar with a background and thumb
// image for improved aesthetics compared to the .NET TrackBar.
// Copyright (C) 2009  FlexRadio Systems
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// You may contact us via email at: sales@flex-radio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    8900 Marybank Dr.
//    Austin, TX 78750
//    USA
//=================================================================

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;

namespace Thetis
{
    [DefaultEvent("Scroll")]
    public class PrettyTrackBar : PictureBox
    {
        #region Variable Declaration

        private Rectangle _head_rect;
        private bool sliding = false;
        private bool _limitSliding = false;
        private int down_x;
        private int down_y;
        private Rectangle _limitBar_rect;

        public class LimitConstraint : EventArgs
        {
            public int LimitValue;
            public bool MouseWheel;
        }
        #endregion

        #region Constructor and Destructor

        public PrettyTrackBar()
        {

            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
            _head_rect = new Rectangle(0, 0, 1, 1);
        }

        #endregion

        #region Misc Routines

        private void UpdateHeadRectPos()
        {
            int head_x = 0;
            int head_y = 0;

            switch (orientation)
            {
                case Orientation.Horizontal:
                    int width;
                    if (_head_image == null)
                    {
                        width = this.Width;
                        head_x = (int)((val - min) / (double)(max - min) * width);
                        head_y = (int)(this.Height / 2);
                    }
                    else
                    {
                        width = this.Width - _head_rect.Width - Padding.Horizontal;
                        head_x = (int)Math.Round((val - min) / (double)(max - min) * width) + Padding.Left;
                        head_y = (int)((this.Height - Padding.Vertical) / 2 - _head_rect.Height / 2) + Padding.Top;
                    }
                    break;
                case Orientation.Vertical:
                    int height;
                    if (_head_image == null)
                    {
                        height = this.Height;
                        head_x = (int)(this.Width / 2);
                        head_y = (int)((val - min) / (double)(max - min) * height);
                    }
                    else
                    {
                        height = this.Height - _head_rect.Height - Padding.Vertical;
                        head_x = (int)((this.Width - Padding.Horizontal) / 2 - _head_rect.Width / 2) + Padding.Top;
                        head_y = (int)(height - (val - min) / (double)(max - min) * height) + Padding.Top;
                    }
                    break;
            }

            _head_rect.X = head_x;
            _head_rect.Y = head_y;
        }
        private void UpdateLimitBar()
        {
            // limit bar is a rectangle
            int startPos;
            int endEdge;
            int height;
            int width;
            int offset;

            switch (orientation)
            {
                case Orientation.Horizontal:
                    int headWidth = _head_rect.Width;// _head_image != null ? _head_rect.Width : 1;
                    
                    width = this.Width - headWidth - Padding.Horizontal;
                    startPos = (int)Math.Round((_nLimitValue - min) / (double)(max - min) * width);
                    endEdge = width;

                    offset = (this.Width - width) / 2;

                    _limitBar_rect.X = startPos + offset;
                    _limitBar_rect.Width = endEdge - startPos;
                    _limitBar_rect.Y = (this.Height / 2) - 1;
                    _limitBar_rect.Height = this.Height % 2 == 0 ? 2 : 3;

                    break;
                case Orientation.Vertical:
                    int headHeight = _head_rect.Height;// _head_image != null ? _head_rect.Height : 0;

                    height = this.Height - headHeight - Padding.Vertical;
                    startPos = (int)((_nLimitValue - min) / (double)(max - min) * height);
                    endEdge = height;

                    offset = (this.Height - height) / 2;

                    _limitBar_rect.X = (this.Width / 2) - 1;
                    _limitBar_rect.Width = this.Width % 2 == 0 ? 2 : 3;
                    _limitBar_rect.Y = startPos + offset;
                    _limitBar_rect.Height = endEdge - startPos;

                    break;
            }
        }

        #endregion

        #region Properties

        private Image _head_image = null;
        public Image HeadImage
        {
            get { return _head_image; }
            set
            {
                _head_image = value;
                if (_head_image != null)
                {
                    _head_rect.Width = _head_image.Width;
                    _head_rect.Height = _head_image.Height;
                }
                else
                {
                    _head_rect.Width = 1;
                    _head_rect.Height = 1;
                }
                UpdateHeadRectPos();
                UpdateLimitBar();
                this.Invalidate();
            }
        }

        private int min = 0;
        public int Minimum
        {
            get { return min; }
            set
            {
                min = value;
                if (val < min) val = min;
                if (_nLimitValue < min) _nLimitValue = min;
                UpdateHeadRectPos();
                UpdateLimitBar();
                this.Invalidate();
            }
        }

        private int max = 100;
        public int Maximum
        {
            get { return max; }
            set
            {
                max = value;
                if (val > max) val = max;
                if (_nLimitValue > max) _nLimitValue = max;
                UpdateHeadRectPos();
                UpdateLimitBar();
                this.Invalidate();
            }
        }

        private int val = 0;
        public int Value
        {
            get { return val; }
            set
            {
                val = value;
                if (val < min) val = min;
                if (val > max) val = max;
                UpdateHeadRectPos();
                this.Invalidate();
            }
        }
        private Color _limitBarColor = Color.Red;
        public Color LimitBarColor
        {
            get { return _limitBarColor; }
            set { 
                _limitBarColor = value;
                this.Invalidate();
            }
        }
        public bool IsConstrained
        {
            get
            {
                return _bLimitEnabled && (val > _nLimitValue);
            }
        }
        public int ConstrainedValue
        {
            get
            {
                if (!_bLimitEnabled || (val <= _nLimitValue))
                    return val;
                else
                    return _nLimitValue;
            }
        }
        public int ConstrainAValue(int value)
        {
            // takes the int passed and runs it through the current limit
            if (!_bLimitEnabled || (value <= _nLimitValue))
                return value;
            else
                return _nLimitValue;
        }

        private int _nLimitValue = 0;
        public int LimitValue
        {
            get { return _nLimitValue; }
            set
            {
                _nLimitValue = value;
                if(_nLimitValue < min) _nLimitValue = min;
                if(_nLimitValue > max) _nLimitValue = max;
                UpdateLimitBar();
                this.Invalidate();
            }
        }
        private bool _bLimitEnabled = false;
        public bool LimitEnabled
        {
            get { return _bLimitEnabled; }
            set
            {
                _bLimitEnabled = value;
                UpdateLimitBar();
                this.Invalidate();
            }
        }

        private int small_change = 1;
        public int SmallChange
        {
            get { return small_change; }
            set { small_change = value; }
        }

        private int large_change = 5;
        public int LargeChange
        {
            get { return large_change; }
            set { large_change = value; }
        }

        private Orientation orientation = Orientation.Horizontal;
        public Orientation Orientation
        {
            get { return orientation; }
            set
            {
                orientation = value;
                this.Invalidate();
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnEnabledChanged(EventArgs e)
        {
            this.Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e); // MW0LGE_21k8, so we can use this if needed

            if (!(e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)) return;
            if (_head_rect.IsEmpty) return;

            if (this.Enabled)
            {
                if (e.Button == MouseButtons.Left) // the regular thumb/head control
                {
                    if (_head_rect.Contains(e.X, e.Y))
                    {
                        down_x = e.X;
                        down_y = e.Y;
                        sliding = true;
                    }
                    else
                    {
                        int old_val = val;
                        int new_val;
                        switch (orientation)
                        {
                            case Orientation.Horizontal:
                                if (e.Y >= _head_rect.Y && e.Y <= _head_rect.Y + _head_rect.Height)
                                {
                                    if (e.X < _head_rect.X) new_val = old_val - large_change;
                                    else new_val = old_val + large_change;

                                    if (new_val < min) new_val = min;
                                    if (new_val > max) new_val = max;

                                    Value = new_val;
                                    OnScroll(this, e);
                                }
                                break;
                            case Orientation.Vertical:
                                if (e.X >= _head_rect.X && e.X <= _head_rect.X + _head_rect.Width)
                                {
                                    if (e.Y > _head_rect.Y) new_val = old_val - large_change;
                                    else new_val = old_val + large_change;

                                    if (new_val < min) new_val = min;
                                    if (new_val > max) new_val = max;

                                    Value = new_val;
                                    OnScroll(this, e);
                                }
                                break;
                        }
                    }
                }
                else if(e.Button == MouseButtons.Right && _bLimitEnabled) // the limit drag
                {
                    int old_val;
                    int new_val;
                    double percent;
                    int width;
                    int height;

                    down_x = e.X;
                    down_y = e.Y;
                    _limitSliding = true; // we dont have anything to grab, so just slide if we hold the button

                    old_val = _nLimitValue;
                    new_val = old_val;

                    switch (orientation)
                    {
                        case Orientation.Horizontal:
                            int headWidth = _head_rect.Width;// _head_image != null ? _head_rect.Width : 1;
                            width = this.Width - headWidth - Padding.Horizontal;

                            if (down_x < Padding.Left + (headWidth / 2)) down_x = Padding.Left + (headWidth / 2);
                            if (down_x > this.Width - (headWidth / 2) - Padding.Right) down_x = this.Width - (headWidth / 2) - Padding.Right;

                            percent = (down_x - Padding.Left - (headWidth / 2)) / (double)width;
                            new_val = min + (int)Math.Round((percent * (max - min)));
                            if (new_val < min) new_val = min;
                            if (new_val > max) new_val = max;

                            break;
                        case Orientation.Vertical:
                            int headHeight = _head_rect.Height;// _head_image != null ? _head_rect.Height : 0;
                            height = this.Height - headHeight - Padding.Vertical;

                            if (down_y < Padding.Top + (headHeight / 2)) down_y = Padding.Top + (headHeight / 2);
                            if (down_y > this.Height - (headHeight / 2) - Padding.Bottom) down_y = this.Height - (headHeight / 2) - Padding.Bottom;

                            percent = 1.0 - (down_y - Padding.Top - (headHeight / 2)) / (double)height;
                            new_val = min + (int)(percent * (max - min));
                            if (new_val < min) new_val = min;
                            if (new_val > max) new_val = max;

                            break;
                    }

                    if (new_val != old_val)
                    {
                        _nLimitValue = new_val;
                        UpdateLimitBar();
                    }
                    OnScroll(this, new LimitConstraint() { LimitValue = _nLimitValue, MouseWheel = false }); // always
                    this.Invalidate();
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_head_rect.IsEmpty) return;

            if (this.Enabled)
            {
                int delta;
                int old_val;
                int new_val;
                double percent;
                int width;
                int height;

                if (sliding)
                {
                    old_val = val;
                    new_val = old_val;

                    switch (orientation)
                    {
                        case Orientation.Horizontal:
                            delta = e.X - down_x;
                            width = this.Width - _head_rect.Width - Padding.Horizontal;

                            if (_head_rect.X <= Padding.Left && delta < 0) return;
                            else if (_head_rect.X >= (width + Padding.Left) && delta > 0) return;

                            percent = (_head_rect.X - Padding.Left + delta) / (double)width;
                            new_val = min + (int)Math.Round((percent * (max - min)));
                            if (new_val < min) new_val = min;
                            if (new_val > max) new_val = max;

                            down_x = e.X;
                            if (down_x < Padding.Left) down_x = Padding.Left;
                            if (down_x > width + Padding.Left + _head_rect.Width) down_x = width + Padding.Left + _head_rect.Width;

                            _head_rect.X += delta;
                            if (_head_rect.X < Padding.Left) _head_rect.X = Padding.Left;
                            if (_head_rect.X > width + Padding.Left) _head_rect.X = width + Padding.Left;
                            break;
                        case Orientation.Vertical:
                            delta = e.Y - down_y;
                            height = this.Height - _head_rect.Height - Padding.Vertical;

                            if (_head_rect.Y <= Padding.Top && delta < 0) return;
                            else if (_head_rect.Y >= (height + Padding.Top) && delta > 0) return;

                            percent = 1.0 - (_head_rect.Y - Padding.Top + delta) / (double)height;
                            new_val = min + (int)(percent * (max - min));
                            if (new_val < min) new_val = min;
                            if (new_val > max) new_val = max;

                            down_y = e.Y;
                            if (down_y < Padding.Top) down_y = Padding.Top;
                            if (down_y > height + Padding.Top + _head_rect.Height) down_y = height + Padding.Top + _head_rect.Height;

                            _head_rect.Y += delta;
                            if (_head_rect.Y < Padding.Top) _head_rect.Y = Padding.Top;
                            if (_head_rect.Y > height + Padding.Top) _head_rect.Y = height + Padding.Top;
                            break;
                    }

                    if (new_val != old_val)
                    {
                        val = new_val;
                        OnScroll(this, e);
                    }
                    this.Invalidate();
                    //Debug.WriteLine("delta: "+delta+"  percent: "+percent.ToString("f4")+"  new_val: " + new_val+"  down_x: "+down_x+"  head_x: "+head_rect.X);
                }
                else if (_limitSliding)
                {
                    old_val = _nLimitValue;
                    new_val = old_val;

                    switch (orientation)
                    {
                        case Orientation.Horizontal:
                            int headWidth = _head_rect.Width;// _head_image != null ? _head_rect.Width : 1;

                            delta = e.X - down_x;                            
                            width = this.Width - headWidth - Padding.Horizontal;

                            //if (_limitBar_rect.X <= Padding.Left + (headWidth / 2) && delta < 0) return;
                            //else if (_limitBar_rect.X >= (width - (headWidth / 2) - Padding.Right) && delta > 0) return;

                            percent = (_limitBar_rect.X - Padding.Left - (headWidth / 2) + delta) / (double)width;
                            new_val = min + (int)Math.Round((percent * (max - min)));
                            if (new_val < min) new_val = min;
                            if (new_val > max) new_val = max;

                            down_x = e.X;
                            if (down_x < Padding.Left + (headWidth / 2)) down_x = Padding.Left + (headWidth / 2);
                            if (down_x > this.Width - (headWidth / 2) - Padding.Right) down_x = this.Width - (headWidth / 2) - Padding.Right;

                            break;
                        case Orientation.Vertical:
                            int headHeight = _head_rect.Height;// _head_image != null ? _head_rect.Height : 0;

                            delta = e.Y - down_y;
                            height = this.Height - headHeight - Padding.Vertical;

                            //if (_limitBar_rect.Y <= Padding.Top + (headHeight / 2) && delta < 0) return;
                            //else if (_limitBar_rect.Y >= (height - (headHeight / 2) - Padding.Bottom) && delta > 0) return;

                            percent = 1.0 - (_limitBar_rect.Y - Padding.Top - (headHeight / 2) + delta) / (double)height;
                            new_val = min + (int)(percent * (max - min));
                            if (new_val < min) new_val = min;
                            if (new_val > max) new_val = max;

                            down_y = e.Y;
                            if (down_y < Padding.Top + (headHeight / 2)) down_y = Padding.Top + (headHeight / 2);
                            if (down_y > this.Height - (headHeight / 2) - Padding.Bottom) down_y = this.Height - (headHeight / 2) - Padding.Bottom;

                            break;
                    }

                    if (new_val != old_val)
                    {
                        _nLimitValue = new_val;
                        UpdateLimitBar();
                        OnScroll(this, new LimitConstraint() { LimitValue = _nLimitValue, MouseWheel = false });
                    }

                    this.Invalidate();
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!(e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)) return;
            if (_head_rect.IsEmpty) return;

            sliding = false;
            _limitSliding = false;

            this.Invalidate();

            base.OnMouseUp(e);
        }

        private bool m_bGreenThumb = false;
        public bool GreenThumb
        {
            get { return m_bGreenThumb; }
            set { m_bGreenThumb = value; this.Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;

            if (this.Enabled)
            {
                // draw background
                /*if(base.BackgroundImage != null)
                    g.DrawImage(base.BackgroundImage, 0, 0);*/

                float brightness = 1.0f;    // no change in brightness
                float contrast = m_bGreenThumb ? 0.5f : 1f;      // half the contrast
                float gamma = 1.0f;         // no change in gamma
                float newBrightness = brightness - 1.0f;
                float[][] ptsArray ={
                    new float[] {contrast, 0, 0, 0, 0}, // scale red
                    new float[] {0, 1, 0, 0, 0}, // scale green
                    new float[] {0, 0, contrast, 0, 0}, // scale blue
                    new float[] {0, 0, 0, 1.0f, 0},     // don't scale alpha
                    new float[] {newBrightness, newBrightness, newBrightness, 0, 1}};

                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.ClearColorMatrix();
                imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);

                // draw limit bar
                if (_bLimitEnabled)
                {
                    Brush b;
                    if (_limitSliding)
                        b = new SolidBrush(Color.FromArgb(255, _limitBarColor));
                    else if (val > _nLimitValue)
                    {
                        float ratio = 170 / 255f;
                        b = new SolidBrush(Color.FromArgb(255, (int)(_limitBarColor.R * ratio), (int)(_limitBarColor.G * ratio), (int)(_limitBarColor.B * ratio)));
                    }
                    else
                    {
                        float ratio = 110 / 255f;
                        b = new SolidBrush(Color.FromArgb(255, (int)(_limitBarColor.R * ratio), (int)(_limitBarColor.G * ratio), (int)(_limitBarColor.B * ratio)));
                    }

                    g.FillRectangle(b, _limitBar_rect);

                    b.Dispose();
                }
                //

                // draw head
                if (_head_image != null)
                    //g.DrawImage(head_image, head_rect.X, head_rect.Y, head_image.Width, head_image.Height);
                    g.DrawImage(_head_image,
                        new Rectangle(_head_rect.X, _head_rect.Y, _head_rect.Width, _head_rect.Height),
                        0, 0, _head_rect.Width, _head_rect.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes);

                imageAttributes.Dispose();
                imageAttributes = null;
            }
            else
            {
                // disabled version of the thumb - same but dimmer
                float brightness = 1.0f;    // no change in brightness
                float contrast = 0.5f;      // half the contrast
                float gamma = 1.0f;         // no change in gamma
                float newBrightness = brightness - 1.0f;
                float[][] ptsArray ={
                    new float[] {contrast, 0, 0, 0, 0}, // scale red
                    new float[] {0, contrast, 0, 0, 0}, // scale green
                    new float[] {0, 0, contrast, 0, 0}, // scale blue
                    new float[] {0, 0, 0, 1.0f, 0},     // don't scale alpha
                    new float[] {newBrightness, newBrightness, newBrightness, 0, 1}};

                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.ClearColorMatrix();
                imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);

                // draw background
                /*if(base.BackgroundImage != null)
                    g.DrawImage(base.BackgroundImage,
                        new Rectangle(0, 0, this.Width, this.Height),
                        0, 0, base.BackgroundImage.Width, base.BackgroundImage.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes);*/

                // grey limit bar
                if (_bLimitEnabled)
                {
                    using (Brush b = new SolidBrush(Color.FromArgb(255, 64, 64, 64)))
                    {
                        g.FillRectangle(b, _limitBar_rect);
                    }
                }

                // draw head
                if (_head_image != null)
                    g.DrawImage(_head_image,
                        new Rectangle(_head_rect.X, _head_rect.Y, _head_rect.Width, _head_rect.Height),
                        0, 0, _head_rect.Width, _head_rect.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes);

                imageAttributes.Dispose();
                imageAttributes = null;
            }
        }

        public delegate void ScrollHandler(object sender, EventArgs e);
        public event ScrollHandler Scroll;
        protected virtual void OnScroll(object sender, EventArgs e)
        {
            if (Scroll != null)
            {
                Scroll(sender, e);
            }
        }

        #endregion
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (Common.ShiftKeyDown)
            {
                if (e.Delta >= 120 && this.LimitValue + 1 <= this.Maximum)
                {
                    LimitValue += 1;
                    OnScroll(this, new LimitConstraint() { LimitValue = _nLimitValue, MouseWheel = true });
                }
                else if (e.Delta <= -120 && this.LimitValue - 1 >= this.Minimum)
                {
                    LimitValue -= 1;
                    OnScroll(this, new LimitConstraint() { LimitValue = _nLimitValue, MouseWheel = true });
                }
            }
            else
            {
                if (e.Delta >= 120 && this.Value + large_change <= this.Maximum)
                {
                    Value += large_change;
                    OnScroll(this, e);//MW0LGE_22b EventArgs.Empty); // needed for filter width slider so we know if we are scrolling up or down based on the delta
                }
                else if (e.Delta <= -120 && this.Value - large_change >= this.Minimum)
                {
                    Value -= large_change;
                    OnScroll(this, e);//MW0LGE_22b EventArgs.Empty); // needed for filter width slider so we know if we are scrolling up or down based on the delta
                }
            }
            base.OnMouseWheel(e);
        }
    }
}