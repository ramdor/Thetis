/*  ucInfoBar.cs

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
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Thetis
{
    public partial class ucInfoBar : UserControl
    {
        private const int WM_SETREDRAW = 11;
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        private const int MAX_FLIP = 2;
        public enum ActionTypes
        {
            Blobs = 0,
            ActivePeaks,
            CursorInfo,
            ShowSpots,
            DisplayFill,
            CFC,
            CFCeq,
            Leveler,
            DisplayPause,
            LAST
        }

        public class InfoBarAction : EventArgs
        {
            public ActionTypes Action { get; set; }
            public bool ButtonState;
            public MouseButtons Button;
        }

        public event EventHandler<InfoBarAction> Button1Clicked;
        public event EventHandler<InfoBarAction> Button2Clicked;
        public event EventHandler<InfoBarAction> Button1MouseDown;
        public event EventHandler<InfoBarAction> Button2MouseDown;

        public event EventHandler SwapRedBlueChanged;
        public event EventHandler HideFeedbackChanged;

        private Console _console;
        private bool _mox;
        private bool _psEnabled = false;
        private System.Timers.Timer _psTimer;
        private System.Timers.Timer _warningTimer;
        private bool _preventClickEvents = false;
        private bool _shutDown = false;
        private int _currentFlip = 0; // index into the string array to display
        private bool _hideFeedback = false;
        private bool _dragging = false;
        private int _startX;
        private float _splitterRatio = 1;
        private bool _okToResize = false;

        private string[] _left1;
        private string[] _left2;
        private string[] _left3;
        private string[] _right1;
        private string[] _right2;
        private string[] _right3;

        private string[,] _leftToolTip;
        private string[,] _rightToolTip;

        private int[] _left1BaseWidth;
        private int[] _left2BaseWidth;
        private int[] _left3BaseWidth;
        private int[] _left1Width;
        private int[] _left2Width;
        private int[] _left3Width;

        private int[] _right1BaseWidth;
        private int[] _right2BaseWidth;
        private int[] _right3BaseWidth;
        private int[] _right1Width;
        private int[] _right2Width;
        private int[] _right3Width;

        private frmInfoBarPopup _frmInfoBarPopup_Button1;
        private ToolStripDropDown _toolStripForm_Button1;
        private ToolStripControlHost _host_Button1;
        private frmInfoBarPopup _frmInfoBarPopup_Button2;
        private ToolStripDropDown _toolStripForm_Button2;
        private ToolStripControlHost _host_Button2;
        private Cursor _oldCursor;

        private Font _normalPSFont;// = new Font("Microsft Sans Serif", 9f, FontStyle.Bold);
        private Font _smallPSFont;// = new Font("Microsft Sans Serif", 6.75f, FontStyle.Regular);

        public class ActionState
        {
            public bool Checked;
            public ActionTypes Action;
            
            public string DisplayString
            {
                get
                {
                    switch (Action)
                    {
                        case ActionTypes.Blobs:
                            return "Blobs";
                        case ActionTypes.ActivePeaks:
                            return "Peak";
                        case ActionTypes.CFC:
                            return "CFC";
                        case ActionTypes.CursorInfo:
                            return "Info";
                        case ActionTypes.Leveler:
                            return "Lev";
                        case ActionTypes.CFCeq:
                            return "CFCeq";
                        case ActionTypes.ShowSpots:
                            return "Spots";
                        case ActionTypes.DisplayFill:
                            return "Fill";
                        case ActionTypes.DisplayPause:
                            return "Pause";

                    }
                    return "?";
                }
            }

            public string TipString
            {
                get
                {
                    switch (Action)
                    {
                        case ActionTypes.Blobs:
                            return "Show peak blobs";
                        case ActionTypes.ActivePeaks:
                            return "Show active peak hold";
                        case ActionTypes.CFC:
                            return "Enable CFC";
                        case ActionTypes.CursorInfo:
                            return "Show information on cursor";
                        case ActionTypes.Leveler:
                            return "Enable the leveler";
                        case ActionTypes.CFCeq:
                            return "Enable Post CFC EQ";
                        case ActionTypes.ShowSpots:
                            return "Show spots";
                        case ActionTypes.DisplayFill:
                            return "Fill the panadaptor";
                        case ActionTypes.DisplayPause:
                            return "Pause the display";
                    }
                    return "";
                }
            }
        }

        private Dictionary<ActionTypes, ActionState> _button1Actions = new Dictionary<ActionTypes, ActionState>();
        private Dictionary<ActionTypes, ActionState> _button2Actions = new Dictionary<ActionTypes, ActionState>();
        private ActionState _button1Action = new ActionState();
        private ActionState _button2Action = new ActionState();

        public ucInfoBar()
        {
            InitializeComponent();

            _oldCursor = Cursor.Current;

            _normalPSFont = new Font(lblPS.Font.FontFamily, 9f, FontStyle.Bold);
            _smallPSFont= new Font(lblPS.Font.FontFamily, 6.75f, FontStyle.Regular);

            _psTimer = new System.Timers.Timer();
            _psTimer.AutoReset = false;
            _psTimer.Interval = 50;
            _psTimer.Elapsed += onTick;

            _warningTimer = new System.Timers.Timer();
            _warningTimer.AutoReset = false;
            _warningTimer.Interval = 2000;
            _warningTimer.Elapsed += onWarning;

            _left1 = new string[MAX_FLIP];
            _left2 = new string[MAX_FLIP];
            _left3 = new string[MAX_FLIP];
            _right1 = new string[MAX_FLIP];
            _right2 = new string[MAX_FLIP];
            _right3 = new string[MAX_FLIP];

            _left1BaseWidth = new int[MAX_FLIP];
            _left2BaseWidth = new int[MAX_FLIP];
            _left3BaseWidth = new int[MAX_FLIP];
            _left1Width = new int[MAX_FLIP];
            _left2Width = new int[MAX_FLIP];
            _left3Width = new int[MAX_FLIP];

            _right1BaseWidth = new int[MAX_FLIP];
            _right2BaseWidth = new int[MAX_FLIP];
            _right3BaseWidth = new int[MAX_FLIP];
            _right1Width = new int[MAX_FLIP];
            _right2Width = new int[MAX_FLIP];
            _right3Width = new int[MAX_FLIP];

            _leftToolTip = new string[MAX_FLIP, 3];
            _rightToolTip = new string[MAX_FLIP, 3];

            for (int n=0; n<MAX_FLIP; n++)
            {
                _left1BaseWidth[n] = lblLeft1.Width;
                _left2BaseWidth[n] = lblLeft2.Width;
                _left3BaseWidth[n] = lblLeft3.Width;
                _left1Width[n] = lblLeft1.Width;
                _left2Width[n] = lblLeft2.Width;
                _left3Width[n] = lblLeft3.Width;

                _right1BaseWidth[n] = lblRight1.Width;
                _right2BaseWidth[n] = lblRight2.Width;
                _right3BaseWidth[n] = lblRight3.Width;
                _right1Width[n] = lblRight1.Width;
                _right2Width[n] = lblRight2.Width;
                _right3Width[n] = lblRight3.Width;

                for (int i = 0; i < 3; i++)
                {
                    _leftToolTip[n, i] = "";
                    _rightToolTip[n, i] = "";
                }
            }

            // add the actions
            for (int n = 0; n < (int)ActionTypes.LAST; n++)
            {
                _button1Actions.Add((ActionTypes)n, new ActionState() { Action = (ActionTypes)n });
                _button2Actions.Add((ActionTypes)n, new ActionState() { Action = (ActionTypes)n });
            }

            _button1Action = new ActionState() { Action = ActionTypes.Blobs };
            _button2Action = new ActionState() { Action = ActionTypes.ActivePeaks };
            //

            _frmInfoBarPopup_Button1 = new frmInfoBarPopup();
            _frmInfoBarPopup_Button1.ActionClicked += OnActionClicked_Button1;
            _frmInfoBarPopup_Button1.TopLevel = false;

            _frmInfoBarPopup_Button2 = new frmInfoBarPopup();
            _frmInfoBarPopup_Button2.ActionClicked += OnActionClicked_Button2;
            _frmInfoBarPopup_Button2.TopLevel = false;

            _host_Button1 = new ToolStripControlHost(_frmInfoBarPopup_Button1);
            _toolStripForm_Button1 = new ToolStripDropDown();
            _host_Button2 = new ToolStripControlHost(_frmInfoBarPopup_Button2);
            _toolStripForm_Button2 = new ToolStripDropDown();
            //

            lblSplitter.BackColor = Color.Silver;
            lblFB.Font = _normalPSFont;

            _okToResize = true;
            repositionControls();
        }

        public ActionTypes Button1Action
        {
            get { return _button1Action.Action; }
            set
            {
                _button1Action.Action = value;
            }
        }
        public ActionTypes Button2Action
        {
            get { return _button2Action.Action; }
            set
            {
                _button2Action.Action = value;
            }
        }
        private string actionString(ActionTypes action)
        {
            ActionState tmp = new ActionState() { Action = action };
            return tmp.DisplayString;
        }
        private void OnActionClicked_Button1(object sender, frmInfoBarPopup.PopupActionSelected e)
        {
            if(e.Button == MouseButtons.Left)
            {
                doAction(1, e.Action, e.ButtonState, e.Button);
            }
            else if(e.Button == MouseButtons.Right)
            {
                if (Common.ShiftKeyDown)
                {
                    // setup form?
                }
                else
                {
                    replaceMainButton(1, e.Action, e.ButtonState, e.Button);
                }
            }

            _toolStripForm_Button1.Hide();
        }
        private void OnActionClicked_Button2(object sender, frmInfoBarPopup.PopupActionSelected e)
        {
            if (e.Button == MouseButtons.Left)
            {
                doAction(2, e.Action, e.ButtonState, e.Button);
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (Common.ShiftKeyDown)
                {
                    // setup form?
                }
                else
                {
                    replaceMainButton(2, e.Action, e.ButtonState, e.Button);
                }
            }

            _toolStripForm_Button2.Hide();
        }
        private void doAction(int button, ActionTypes action, bool bState, MouseButtons mouseButton)
        {
            if (_preventClickEvents) return;

            if (button == 1)
            {
                _button1Actions[action].Checked = bState;

                Button1Clicked?.Invoke(this, new InfoBarAction
                {
                    Action = action,
                    ButtonState = bState,
                    Button = mouseButton
                });
            }
            else if (button == 2)
            {
                _button2Actions[action].Checked = bState;

                Button2Clicked?.Invoke(this, new InfoBarAction
                {
                    Action = action,
                    ButtonState = bState,
                    Button = mouseButton
                });
            }
        }

        private void addPopup(frmInfoBarPopup frm, ToolStripControlHost host, ToolStripDropDown dropDown) 
        {
            // build the popup
            host.AutoSize = false;
            host.Margin = Padding.Empty;
            host.Padding = Padding.Empty;
            host.Width = frm.Width;
            host.Height = frm.Height;

            dropDown.AutoSize = false;
            dropDown.Margin = Padding.Empty;
            dropDown.Padding = Padding.Empty;
            dropDown.Width = host.Width;
            dropDown.Height = host.Height;
            dropDown.Items.Add(host);

            dropDown.Closed += OnPopupClosed;
        }

        ~ucInfoBar()
        {
            ShutDown();
        }
        private void OnPopupClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
        }
        public void ShutDown()
        {
            if(_console != null) _console.MoxChangeHandlers -= OnMoxChangeHandler;

            _shutDown = true;
            if (_psTimer != null)
            {
                _psTimer.Stop();
                _psTimer.Elapsed -= onTick;
                _psTimer = null;
            }
            if (_warningTimer != null)
            {
                _warningTimer.Stop();
                _warningTimer.Elapsed -= onWarning;
                _warningTimer = null;
            }
            if (_frmInfoBarPopup_Button1 != null)
            {
                _frmInfoBarPopup_Button1.Close();
                _frmInfoBarPopup_Button1 = null;
            }
        }
        private void onWarning(object sender, System.Timers.ElapsedEventArgs e)
        {
            _warningTimer.Interval = 2000;
            if (_shutDown) return;
            if (this.IsDisposed || this.Disposing) return;
            if (lblWarning.IsDisposed || lblWarning.Disposing) return;

            lblWarning.Visible = false;
        }
        private Color _lastColor = Color.SeaGreen;
        private void onTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_psEnabled || _shutDown) return;
            if (this.IsDisposed || this.Disposing) return;

            Color c = lblFB.BackColor;

            int r = (int)(c.R * 0.95);
            int g = (int)(c.G * 0.95);
            int b = (int)(c.B * 0.95);

            //bool bDone = r < 96 && g < 96 && b < 96;
            bool bDone = false;

            if(_lastColor == Color.Red)
            {
                bDone = (r <= 128);
            }
            else
            if (_lastColor == Color.SeaGreen)
            {
                bDone = (g <= Color.SeaGreen.G);
                if (r < Color.SeaGreen.R) r = Color.SeaGreen.R;
                if (g < Color.SeaGreen.G) g = Color.SeaGreen.G;
                if (b < Color.SeaGreen.B) b = Color.SeaGreen.B;
            }
            else
            if (_lastColor == Color.Yellow)
            {
                bDone = (r <= 96) && (b <= 96);
            }
            else
            if (_lastColor == Color.DodgerBlue)
            {
                bDone = (b <= 128);
            }
            else
            if (_lastColor == Color.Lime)
            {
                bDone = (g <= Color.SeaGreen.G);
                if (r < Color.SeaGreen.R) r = Color.SeaGreen.R;
                if (g < Color.SeaGreen.G) g = Color.SeaGreen.G;
                if (b < Color.SeaGreen.B) b = Color.SeaGreen.B;
            }

            //bool bDone = (r <= Color.MediumSeaGreen.R) && (g <= Color.MediumSeaGreen.G) && (b <= Color.MediumSeaGreen.B);
            //if (r < Color.MediumSeaGreen.R) r = Color.MediumSeaGreen.R;
            //if (g < Color.MediumSeaGreen.G) g = Color.MediumSeaGreen.G;
            //if (b < Color.MediumSeaGreen.B) b = Color.MediumSeaGreen.B;

            lblFB.BackColor = Color.FromArgb(255, r, g, b);

            if (bDone)
            {
                _feedbackColour = Color.SeaGreen;
                if (_useSmallFonts)
                    lblFB.Text = "FB";
                else
                    lblFB.Text = "Feedback";

                _psTimer.Stop();
            }
            else
                if(_psTimer != null) _psTimer.Start(); // fade more
        }

        public void LateInit(Console c)
        {
            _console = c;

            _console.MoxChangeHandlers += OnMoxChangeHandler;

            // clear everything
            for (int i = 0; i < MAX_FLIP; i++)
            {
                _left1[i] = "";
                _left2[i] = "";
                _left3[i] = "";
                _right1[i] = "";
                _right2[i] = "";
                _right3[i] = "";
            }

            lblFB.ForeColor = Color.Black;
            lblPS.ForeColor = Color.Black;

            _shutDown = false;
            lblWarning.Visible = false;
            _preventClickEvents = false;
            PSAEnabled = false;

            _frmInfoBarPopup_Button1.SetStates(_button1Actions, _button1Action, _button2Action);
            _frmInfoBarPopup_Button2.SetStates(_button2Actions, _button1Action, _button2Action);

            addPopup(_frmInfoBarPopup_Button1, _host_Button1, _toolStripForm_Button1);
            addPopup(_frmInfoBarPopup_Button2, _host_Button2, _toolStripForm_Button2);

            updateLabels();
            setToolTips();
        }

        private void OnMoxChangeHandler(int rx, bool oldMox, bool newMox)
        {
            _mox = newMox;
            if (!_mox)
                setPSboolsToFalse();

            updatePSDisplay();
        }
        private void setPSboolsToFalse()
        {
            _bCalibrationAttemptsChanged = false;
            _bCorrectionsBeingApplied = false;
            _bFeedbackLevelOk = false;            
        }
        private void chkButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (_preventClickEvents) return;
            Button1Clicked?.Invoke(this, new InfoBarAction { 
                Action = _button1Action.Action,
                ButtonState = chkButton1.Checked,
                Button = MouseButtons.None
            });
        }

        private void chkButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (_preventClickEvents) return;
            Button2Clicked?.Invoke(this, new InfoBarAction { 
                Action = _button2Action.Action,
                ButtonState = chkButton2.Checked,
                Button = MouseButtons.None
            });;
        }

        public void UpdateButtonState(ActionTypes action, bool bEnabled, bool bIncludePopup = true)
        {
            if(!_button1Actions.ContainsKey(action) || !_button2Actions.ContainsKey(action)) return;

            _preventClickEvents = true; // so that events are not fired when the button state is initialised

            _button1Actions[action].Checked = bEnabled;
            _button2Actions[action].Checked = bEnabled;

            if (bIncludePopup)
            {
                // udpate the drop menus, if any have this action
                _frmInfoBarPopup_Button1.SetStates(_button1Actions, _button1Action, _button2Action);
                _frmInfoBarPopup_Button2.SetStates(_button2Actions, _button1Action, _button2Action);
            }

            // set the state of the main infobar buttons1 and button2 if they are this action
            if (_button1Action.Action == action)
            {
                chkButton1.Text = actionString(action);
                chkButton1.Checked = bEnabled;
                toolTip1.SetToolTip(chkButton1, _button1Actions[action].TipString);
            }
            if (_button2Action.Action == action)
            {
                chkButton2.Text = actionString(action);
                chkButton2.Checked = bEnabled;
                toolTip1.SetToolTip(chkButton2, _button2Actions[action].TipString);
            }

            _preventClickEvents = false;
        }
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                // set all the labels
                lblPageNo.BackColor = value;
                lblLeft1.BackColor = value;
                lblLeft2.BackColor = value;
                lblLeft3.BackColor = value;
                lblRight1.BackColor = value;
                lblRight2.BackColor = value;
                lblRight3.BackColor = value;
                lblWarning.BackColor = value;

                if(_frmInfoBarPopup_Button1 != null) _frmInfoBarPopup_Button1.BackColor = value;
                if (_frmInfoBarPopup_Button2 != null) _frmInfoBarPopup_Button2.BackColor = value;
            }
        }
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                base.ForeColor = value;
                // set all the labels
                lblPageNo.ForeColor = value;
                lblLeft1.ForeColor = value;
                lblLeft2.ForeColor = value;
                lblLeft3.ForeColor = value;
                lblRight1.ForeColor = value;
                lblRight2.ForeColor = value;
                lblRight3.ForeColor = value;
            }
        }

        public void Left1(int flipLayer, string value, int width = -1)
        {
            if (flipLayer < 0 || flipLayer > MAX_FLIP - 1) return;
            _left1[flipLayer] = value;

            if (width == -1)
                _left1Width[flipLayer] = _left1BaseWidth[flipLayer];
            else
                _left1Width[flipLayer] = width;

            if (_currentFlip != flipLayer) return; //MW0LGE [2.9.0.7]

            lblLeft1.Text = _left1[_currentFlip];

            repositionControls();
        }
        public void Left2(int flipLayer, string value, int width = -1)
        {
            if (flipLayer < 0 || flipLayer > MAX_FLIP - 1) return;
            _left2[flipLayer] = value;

            if (width == -1)
                _left2Width[flipLayer] = _left2BaseWidth[flipLayer];
            else
                _left2Width[flipLayer] = width;

            if (_currentFlip != flipLayer) return; //MW0LGE [2.9.0.7]

            lblLeft2.Text = _left2[_currentFlip];

            repositionControls();
        }
        public void Left3(int flipLayer, string value, int width = -1)
        {
            if (flipLayer < 0 || flipLayer > MAX_FLIP - 1) return;
            _left3[flipLayer] = value;

            if (width == -1)
                _left3Width[flipLayer] = _left3BaseWidth[flipLayer];
            else
                _left3Width[flipLayer] = width;

            if (_currentFlip != flipLayer) return; //MW0LGE [2.9.0.7]

            lblLeft3.Text = _left3[_currentFlip];

            repositionControls();
        }
        public void Right1(int flipLayer, string value, int width = -1)
        {
            if (flipLayer < 0 || flipLayer > MAX_FLIP - 1) return;
            _right1[flipLayer] = value;

            if (width == -1)
                _right1Width[flipLayer] = _right1BaseWidth[flipLayer];
            else
                _right1Width[flipLayer] = width;

            if (_currentFlip != flipLayer) return; //MW0LGE [2.9.0.7]

            lblRight1.Text = _right1[_currentFlip];

            repositionControls();// MW0LGE [2.9.0.7]
        }
        public void Right2(int flipLayer, string value, int width = -1)
        {
            if (flipLayer < 0 || flipLayer > MAX_FLIP - 1) return;
            _right2[flipLayer] = value;

            if (width == -1)
                _right2Width[flipLayer] = _right2BaseWidth[flipLayer];
            else
                _right2Width[flipLayer] = width;

            if (_currentFlip != flipLayer) return; //MW0LGE [2.9.0.7]

            lblRight2.Text = _right2[_currentFlip];

            repositionControls();// MW0LGE [2.9.0.7]
        }
        public void Right3(int flipLayer, string value, int width = -1)
        {
            if (flipLayer < 0 || flipLayer > MAX_FLIP - 1) return;
            _right3[flipLayer] = value;

            if (width == -1)
                _right3Width[flipLayer] = _right3BaseWidth[flipLayer];
            else
                _right3Width[flipLayer] = width;

            if (_currentFlip != flipLayer) return; //MW0LGE [2.9.0.7]

            lblRight3.Text = _right3[_currentFlip];

            repositionControls();// MW0LGE [2.9.0.7]
        }
        public void SetToolTipLeft(int flipLayer, int labelIndex, string text)
        {
            if (flipLayer < 0 || flipLayer > MAX_FLIP - 1 || labelIndex < 1 || labelIndex > 3) return;

            LabelTS lbl = null;
            _leftToolTip[flipLayer, labelIndex - 1] = text;

            switch (labelIndex)
            {
                case 1:                   
                    lbl = lblLeft1;
                    break;
                case 2:
                    lbl = lblLeft2;
                    break;
                case 3:
                    lbl = lblLeft3;
                    break;
                default:
                    break;
            }

            if (_currentFlip == flipLayer && lbl != null) toolTip1.SetToolTip(lbl, text);
        }
        public void SetToolTipRight(int flipLayer, int labelIndex, string text)
        {
            if (flipLayer < 0 || flipLayer > MAX_FLIP - 1 || labelIndex < 1 || labelIndex > 3) return;

            LabelTS lbl = null;
            _rightToolTip[flipLayer, labelIndex - 1] = text;

            switch (labelIndex)
            {
                case 1:
                    lbl = lblRight1;
                    break;
                case 2:
                    lbl = lblRight2;
                    break;
                case 3:
                    lbl = lblRight3;
                    break;
                default:
                    break;
            }

            if (_currentFlip == flipLayer && lbl != null) toolTip1.SetToolTip(lbl, text);
        }

        private bool _bCorrectionsBeingApplied = false;
        private bool _bCalibrationAttemptsChanged = false;
        private bool _bFeedbackLevelOk = false;
        private Color _feedbackColour = Color.Black;
        private int _nFeedbackLevel = 0;

        public void PSInfo(int level, bool bFeedbackLevelOk, bool bCorrectionsBeingApplied, bool bCalibrationAttemptsChanged, Color feedbackColour)
        {
            if (_shutDown) return;

            _bCalibrationAttemptsChanged = bCalibrationAttemptsChanged;

            if (_bCalibrationAttemptsChanged && _mox)
            {
                _nFeedbackLevel = level;
                _feedbackColour = feedbackColour;
                _bCorrectionsBeingApplied = bCorrectionsBeingApplied;
                _bFeedbackLevelOk = bFeedbackLevelOk;

                updatePSDisplay();

                _psTimer.Start();
            }
        }

        public bool PSAEnabled
        {
            set
            {                
                _psEnabled = value;
                if (!_psEnabled)
                    setPSboolsToFalse();

                updatePSDisplay();
            }
        }

        private void updatePSDisplay()
        {
            if (!_psEnabled)
            {
                lblFB.BackColor = Color.FromArgb(255, Color.DimGray);
                lblPS.BackColor = Color.FromArgb(255, Color.DimGray);
                _lastColor = Color.DimGray;
                if (_useSmallFonts)
                    lblFB.Text = "FB";
                else
                    lblFB.Text = "Feedback";
                lblPS.Text = "Pure Signal2";
            }
            else
            {
                if (_mox)
                {
                    if (_bCorrectionsBeingApplied)
                    {
                        lblPS.Text = _useSmallFonts ? "Correct" : "Correcting";
                        lblPS.BackColor = Color.FromArgb(255, Color.Lime);
                    }
                    else
                    {
                        lblPS.Text = "Pure Signal2";
                        lblPS.BackColor = Color.FromArgb(255, Color.SeaGreen);
                    }

                    lblFB.BackColor = _feedbackColour;
                    _lastColor = _feedbackColour;

                    if (_hideFeedback || !_bCalibrationAttemptsChanged)
                    {
                        if (_useSmallFonts)
                            lblFB.Text = "FB";
                        else
                            lblFB.Text = "Feedback";
                    }
                    else
                    {
                        lblFB.Text = _nFeedbackLevel.ToString();
                    }
                }
                else
                {
                    _psTimer.Stop();

                    _lastColor = Color.SeaGreen;
                    _feedbackColour = Color.SeaGreen;

                    lblPS.Text = "Pure Signal2";
                    lblPS.BackColor = Color.FromArgb(255, Color.SeaGreen);

                    lblFB.BackColor = Color.SeaGreen;
                    if (_useSmallFonts)
                        lblFB.Text = "FB";
                    else
                        lblFB.Text = "Feedback";
                }
            }
        }

        public CheckBoxTS Button1
        {
            get { return this.chkButton1; }
        }
        public CheckBoxTS Button2
        {
            get { return this.chkButton2; }
        }

        //public void Warning(string msg, int nOverloadColourCount = -1, bool bExtendedShow = false)
        public void Warning(string msg, bool red_warning = false, int show_duration = 2000)
        {
            if (_shutDown) return;

            //if (nOverloadColourCount > -1)
            //{
            //    switch (nOverloadColourCount)
            //    {
            //        case 0:
            //            lblWarning.ForeColor = Color.Red;
            //            break;
            //        case 1:
            //            lblWarning.ForeColor = Color.Yellow;
            //            break;
            //    }
            //}
            _warningTimer.Stop();
            lblWarning.ForeColor = red_warning ? Color.Red : Color.Yellow;
            lblWarning.Text = msg;
            lblWarning.Visible = true;
            _warningTimer.Interval = show_duration;
            _warningTimer.Start();
        }
        private void InfoBar_Resize(object sender, EventArgs e)
        {
            //splitter
            int leftStop = (int)(this.Width * 0.7f);
            int span = this.Width - 88 - leftStop; // see lblSplitter_MouseMove for 88
            float shift = span - (span * _splitterRatio);
            lblSplitter.Left = this.Width - 88 - (int)shift;
            //

            repositionControls();
        }

        private void InfoBar_Click(object sender, EventArgs e)
        {
            flip();
        }    
        private void flip()
        {
            _currentFlip++;
            if (_currentFlip > MAX_FLIP - 1) _currentFlip = 0;

            updateLabels();
        }

        private void updateLabels()
        {
            lblPageNo.Text = (_currentFlip + 1).ToString() + "/" + MAX_FLIP.ToString();
            lblLeft1.Text = _left1[_currentFlip];
            lblLeft2.Text = _left2[_currentFlip];
            lblLeft3.Text = _left3[_currentFlip];
            lblRight1.Text = _right1[_currentFlip];
            lblRight2.Text = _right2[_currentFlip];
            lblRight3.Text = _right3[_currentFlip];

            //tool tips
            for (int i = 0; i < 3; i++)
            {
                SetToolTipLeft(_currentFlip, i+1, _leftToolTip[_currentFlip, i]);  //+1 as we know them as right1 left1
                SetToolTipRight(_currentFlip, i+1, _rightToolTip[_currentFlip, i]);
            }
            //

            repositionControls();
        }

        public int CurrentFlip
        {
            get { return _currentFlip; }
            set
            {
                _currentFlip = value;
                if (_currentFlip < 0 || _currentFlip > MAX_FLIP - 1) _currentFlip = 0;
            }
        }

        private void chkButton1_MouseDown(object sender, MouseEventArgs e)
        {
            if (IsRightButton(e))
            {
                if (Common.ShiftKeyDown)
                {
                    Button1MouseDown?.Invoke(this, new InfoBarAction
                    {
                        Action = _button1Action.Action,
                        ButtonState = chkButton1.Checked,
                        Button = e.Button
                    });
                }
                else
                {
                    if (_frmInfoBarPopup_Button1 != null)
                    {
                        if (_frmInfoBarPopup_Button1 != null && _frmInfoBarPopup_Button1.HasButtons)
                            _toolStripForm_Button1.Show(this, new Point((chkButton1.Left + chkButton1.Width / 2) - (_frmInfoBarPopup_Button1.Width / 2), chkButton1.Top + chkButton1.Height));
                    }
                }
            }
        }

        private void chkButton2_MouseDown(object sender, MouseEventArgs e)
        {
            if (IsRightButton(e))
            {
                if (Common.ShiftKeyDown)
                {
                    Button2MouseDown?.Invoke(this, new InfoBarAction
                    {
                        Action = _button2Action.Action,
                        ButtonState = chkButton2.Checked,
                        Button = e.Button
                    });
                }
                else
                {
                    if (_frmInfoBarPopup_Button2 != null)
                    {
                        if (_frmInfoBarPopup_Button2 != null && _frmInfoBarPopup_Button2.HasButtons)
                            _toolStripForm_Button2.Show(this, new Point((chkButton2.Left + chkButton2.Width / 2) - (_frmInfoBarPopup_Button2.Width / 2), chkButton2.Top + chkButton2.Height));
                    }
                }
            }
        }

        private bool IsRightButton(MouseEventArgs e)
        {
            return e.Button == MouseButtons.Right;
        }

        private void lblFB_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                // swap red/blue
                SwapRedBlue = !puresignal.InvertRedBlue;
            }
            else if(e.Button == MouseButtons.Right)
            {
                // disable/enabled feedback numbers
                HideFeedback = !HideFeedback;
            }
        }

        public bool SwapRedBlue
        {
            get { return puresignal.InvertRedBlue; }
            set
            {
                bool bChanged = puresignal.InvertRedBlue != value;
                puresignal.InvertRedBlue = value;
                setToolTips();

                if (bChanged) SwapRedBlueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public bool HideFeedback
        {
            get { return _hideFeedback; }
            set 
            { 
                bool bChanged = _hideFeedback != value;
                _hideFeedback = value;
                setToolTips();

                if (bChanged) HideFeedbackChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private void setToolTips()
        {
            string fb = "";

            if (!HideFeedback)
                fb = "Showing level, ";

            if (puresignal.InvertRedBlue)
            {
                toolTip1.SetToolTip(lblFB, fb + "Blue 0-90, Yellow 91-128, Green 129-181, Red 182+");
            }
            else
            {
                toolTip1.SetToolTip(lblFB, fb + "Red 0-90, Yellow 91-128, Green 129-181, Blue 182+");
            }
        }

        private void replaceMainButton(int button, ActionTypes action, bool bState, MouseButtons mouseButton)
        {
            _preventClickEvents = true;

            if (button == 1)
            {
                _button1Action.Action = action; // new action
            }
            else if (button == 2)
            {
                _button2Action.Action = action; // new action
            }            

            _preventClickEvents = false;

            UpdateButtonState(action, bState);
        }
        public CheckBoxTS GetPopupButton(int infoBarButton, int index)
        {
            if(infoBarButton == 1)
            {
                if (_frmInfoBarPopup_Button1 == null) return null;
                return _frmInfoBarPopup_Button1.GetPopupButton(index);
            }
            else if(infoBarButton == 2)
            {
                if (_frmInfoBarPopup_Button2 == null) return null;
                return _frmInfoBarPopup_Button2.GetPopupButton(index);
            }
            return null;
        }

        private void lblSplitter_MouseDown(object sender, MouseEventArgs e)
        {
            if (_dragging) return;

            _dragging = true;
            _startX = e.X;
        }

        private void lblSplitter_MouseEnter(object sender, EventArgs e)
        {
            lblSplitter.BackColor = Color.White;
            
            _oldCursor = Cursor.Current;
            this.Cursor = Cursors.SizeWE;
        }

        private void lblSplitter_MouseHover(object sender, EventArgs e)
        {
            lblSplitter.BackColor = Color.White;
        }

        private void lblSplitter_MouseLeave(object sender, EventArgs e)
        {
            lblSplitter.BackColor = Color.Silver;
            this.Cursor = _oldCursor;
        }

        private void lblSplitter_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                SendMessage(this.Handle, WM_SETREDRAW, false, 0);

                int nDelta = e.X - _startX;

                int oldLeft = lblSplitter.Left;
                int newLeft = nDelta + lblSplitter.Left;

                int leftStop = (int)(this.Width * 0.7f);

                if (newLeft < leftStop)
                    newLeft = leftStop;

                if (newLeft > this.Width - 88 - 5)  // 88 width of base lblFB + lblPS, 5 is splitter width
                    newLeft = this.Width - 88 - 5;

                lblSplitter.Left = newLeft;

                _splitterRatio = (newLeft - leftStop) / (float)(this.Width - 88 - leftStop);

                repositionControls();

                SendMessage(this.Handle, WM_SETREDRAW, true, 0);
                this.Refresh();
            }
        }
        private bool _useSmallFonts = false;
        private void repositionControls()
        {
            // return if any control is null, this should not happen  // MW0LGE [2.9.0.7]
            // added some more checking due to https://github.com/ramdor/Thetis/issues/149
            if (!_okToResize || _shutDown || _currentFlip < 0 || _currentFlip > MAX_FLIP -1 ||
                lblFB is null || lblPS is null ||
                lblLeft1 is null || lblLeft2 is null || lblLeft3 is null ||
                lblRight1 is null || lblRight2 is null || lblRight3 is null ||
                lblWarning is null || lblSplitter is null ||
                _left1Width is null || _left2Width is null || _left3Width is null ||
                _right1Width is null || _right2Width is null || _right3Width is null
                )
            {
                Debug.WriteLine("no repositionControls()");
                return;
            }

            int newLeftFB = lblSplitter.Left + lblSplitter.Width;
            int newSpan = this.Width - newLeftFB;
            int halfSpan = (int)Math.Ceiling(newSpan / 2f);

            // spread FB and PS to fill the space 50/50
            lblFB.Left = newLeftFB;
            lblFB.Width = halfSpan;

            lblPS.Left = newLeftFB + halfSpan;
            lblPS.Width = halfSpan;

            // align left labels
            lblLeft1.Width = _left1Width[_currentFlip];
            lblLeft2.Width = _left2Width[_currentFlip];
            lblLeft3.Width = _left3Width[_currentFlip];

            lblLeft2.Left = lblLeft1.Left + lblLeft1.Width;
            lblLeft3.Left = lblLeft2.Left + lblLeft2.Width;

            // now the right labels
            lblRight1.Width = _right1Width[_currentFlip];
            lblRight2.Width = _right2Width[_currentFlip];
            lblRight3.Width = _right3Width[_currentFlip];

            int shift = lblRight1.Width + lblRight2.Width + lblRight3.Width + 4;
            lblRight1.Left = lblFB.Left - shift;
            lblRight2.Left = lblRight1.Left + lblRight1.Width;
            lblRight3.Left = lblRight1.Left + lblRight1.Width + lblRight2.Width;

            //warning
            lblWarning.Width = lblSplitter.Left - lblWarning.Left - 4;

            _useSmallFonts = newSpan <= 180; // if space is too small, use small fonts

            if (_useSmallFonts)
            {
                if (lblPS.Font != _smallPSFont) lblPS.Font = _smallPSFont;
                if (lblFB.Text == "Feedback") lblFB.Text = "FB";
                if (lblPS.Text == "Correcting") lblPS.Text = "Correct";
            }
            else
            {
                if (lblPS.Font != _normalPSFont) lblPS.Font = _normalPSFont;
                if (lblFB.Text == "FB") lblFB.Text = "Feedback";
                if (lblPS.Text == "Correct") lblPS.Text = "Correcting";
            }

            // check for overlapping anything on left
            lblLeft1.Visible = !lblSplitter.Bounds.IntersectsWith(lblLeft1.Bounds);
            lblLeft2.Visible = lblLeft1.Visible && !lblSplitter.Bounds.IntersectsWith(lblLeft2.Bounds);
            lblLeft3.Visible = lblLeft2.Visible && !lblSplitter.Bounds.IntersectsWith(lblLeft3.Bounds);

            // check for right hand side overlapping left hand side
            lblRight1.Visible = !((lblLeft3.Text != "" && lblRight1.Bounds.IntersectsWith(lblLeft3.Bounds)) || (lblLeft2.Text != "" && lblRight1.Bounds.IntersectsWith(lblLeft2.Bounds)) || (lblLeft1.Text != "" && lblRight1.Bounds.IntersectsWith(lblLeft1.Bounds)));
            lblRight2.Visible = !((lblLeft3.Text != "" && lblRight2.Bounds.IntersectsWith(lblLeft3.Bounds)) || (lblLeft2.Text != "" && lblRight2.Bounds.IntersectsWith(lblLeft2.Bounds)) || (lblLeft1.Text != "" && lblRight2.Bounds.IntersectsWith(lblLeft1.Bounds)));
            lblRight3.Visible = !((lblLeft3.Text != "" && lblRight3.Bounds.IntersectsWith(lblLeft3.Bounds)) || (lblLeft2.Text != "" && lblRight3.Bounds.IntersectsWith(lblLeft2.Bounds)) || (lblLeft1.Text != "" && lblRight3.Bounds.IntersectsWith(lblLeft1.Bounds)));
        }
        private void lblSplitter_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }
        public float SplitterRatio
        {
            get { return _splitterRatio; }
            set { _splitterRatio = value; }
        }
    }
}
