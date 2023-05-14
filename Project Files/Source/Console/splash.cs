//=================================================================
// splash.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
// Copyright (C) 2010-2019  Doug Wigley
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
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Thetis
{


    public class Splash : System.Windows.Forms.Form
    {
        //MW0LGE
        [DllImport("user32.dll")]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        #region Variable Declarations

        // Threading
        static Splash ms_frmSplash = null;
        static Thread ms_oThread = null;

        // Fade in and out.
        private double m_dblOpacityIncrement = .07;
        private double m_dblOpacityDecrement = .035;
        private const int TIMER_INTERVAL = 50;

        // Status and progress bar
        private string m_sStatus;
        private double m_dblCompletionFraction = 0;
        private Rectangle m_rProgress;

        // Progress smoothing
        private double m_dblLastCompletionFraction = 0.0;
        private double m_dblPBIncrementPerTimerInterval = .015;

        // Self-calibration support
        private bool m_bFirstLaunch = false;
        private DateTime m_dtStart;
        private bool m_bDTSet = false;
        private int m_iIndex = 1;
        private int m_iActualTicks = 0;
        private ArrayList m_alPreviousCompletionFraction;
        private ArrayList m_alActualTimes = new ArrayList();
        private const string REG_KEY_INITIALIZATION = "Initialization";
        private const string REGVALUE_PB_MILISECOND_INCREMENT = "Increment";
        private const string REGVALUE_PB_PERCENTS = "Percents";
        private System.Windows.Forms.LabelTS lblTimeRemaining;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.LabelTS lblStatus;
        private System.Windows.Forms.Panel pnlStatus;
        private System.ComponentModel.IContainer components;
        #endregion

        #region Constructor and Destructor

        public Splash()
        {
            InitializeComponent();
            this.Opacity = .00;
            timer1.Interval = TIMER_INTERVAL;
            timer1.Start();
            this.ClientSize = this.BackgroundImage.Size;
            this.ShowInTaskbar = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Splash));
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lblTimeRemaining = new System.Windows.Forms.LabelTS();
            this.lblStatus = new System.Windows.Forms.LabelTS();
            this.SuspendLayout();
            // 
            // pnlStatus
            // 
            this.pnlStatus.BackColor = System.Drawing.Color.SkyBlue;
            this.pnlStatus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pnlStatus.Location = new System.Drawing.Point(59, 270);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(289, 17);
            this.pnlStatus.TabIndex = 2;
            this.pnlStatus.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlStatus_Paint);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lblTimeRemaining
            // 
            this.lblTimeRemaining.BackColor = System.Drawing.Color.Transparent;
            this.lblTimeRemaining.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTimeRemaining.ForeColor = System.Drawing.Color.White;
            this.lblTimeRemaining.Image = null;
            this.lblTimeRemaining.Location = new System.Drawing.Point(256, 290);
            this.lblTimeRemaining.Name = "lblTimeRemaining";
            this.lblTimeRemaining.Size = new System.Drawing.Size(92, 16);
            this.lblTimeRemaining.TabIndex = 1;
            this.lblTimeRemaining.Text = "Starting ...";
            this.lblTimeRemaining.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Image = null;
            this.lblStatus.Location = new System.Drawing.Point(12, 252);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(376, 16);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Welcome to Thetis!";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Splash
            // 
            this.BackgroundImage = global::Thetis.Properties.Resources.thetis_logo1;
            this.ClientSize = new System.Drawing.Size(400, 320);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.lblTimeRemaining);
            this.Controls.Add(this.lblStatus);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Splash";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Splash";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Splash_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Splash_FormClosed);
            this.Load += new System.EventHandler(this.Splash_Load);
            this.VisibleChanged += new System.EventHandler(this.Splash_VisibleChanged);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Splash_MouseDown);
            this.ResumeLayout(false);

        }
        #endregion

        #region Static Methods
        // ************* Static Methods *************** //
        private static Form OwnerForm;
        // A static method to create the thread and 
        // launch the SplashScreen.
        static public void ShowSplashScreen(Form Owner)
        {
            OwnerForm = Owner;
            // Make sure it is only launched once.
            if (ms_frmSplash != null)
                return;
            ms_oThread = new Thread(new ThreadStart(ShowForm))
            {
                IsBackground = true,
                Name = "Splash Screen Thread"
            };
            ms_oThread.Start();
        }

        public new Form Owner
        {
            get => base.Owner;
            set
            {
                base.Owner = value;
            }
        }

        // A property returning the splash screen instance
        static public Splash SplashForm
        {
            get
            {
                return ms_frmSplash;
            }
        }

        // A private entry point for the thread.
        static private void ShowForm()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            ms_frmSplash = new Splash();

            Application.Run(ms_frmSplash);
        }

        // A static method to close the SplashScreen
        static public void CloseForm()
        {
            if (ms_frmSplash != null && ms_frmSplash.IsDisposed == false)
            {
                // Make it start going away.
                ms_frmSplash.m_dblOpacityIncrement = -
                    ms_frmSplash.m_dblOpacityDecrement;
            }
            //Application.Exit();
            ms_oThread = null;  // we do not need these any more.
            ms_frmSplash = null;
        }

        static public void HideForm()
        {
            if (ms_frmSplash != null && ms_frmSplash.IsDisposed == false)
                ms_frmSplash.Hide();
        }

        static public void UnHideForm()
        {
            if (ms_frmSplash != null && ms_frmSplash.IsDisposed == false)
                ms_frmSplash.Show();
        }

        // A static method to set the status and update the reference.
        static public void SetStatus(string newStatus)
        {
            SetStatus(newStatus, true);
        }

        // A static method to set the status and optionally update the reference.
        // This is useful if you are in a section of code that has a variable
        // set of status string updates.  In that case, don't set the reference.
        static public void SetStatus(string newStatus, bool setReference)
        {
            if (ms_frmSplash == null)
                return;
            ms_frmSplash.m_sStatus = newStatus;
            if (setReference)
                ms_frmSplash.SetReferenceInternal();
        }

        // Static method called from the initializing application to 
        // give the splash screen reference points.  Not needed if
        // you are using a lot of status strings.
        static public void SetReferencePoint()
        {
            if (ms_frmSplash == null)
                return;
            ms_frmSplash.SetReferenceInternal();

        }

        // ************ Private methods ************

        // Internal method for setting reference points.
        private void SetReferenceInternal()
        {
            if (m_bDTSet == false)
            {
                m_bDTSet = true;
                m_dtStart = DateTime.Now;
                ReadIncrements();
            }
            double dblMilliseconds = ElapsedMilliSeconds();
            m_alActualTimes.Add(dblMilliseconds);
            m_dblLastCompletionFraction = m_dblCompletionFraction;
            if (m_alPreviousCompletionFraction != null
                && m_iIndex < m_alPreviousCompletionFraction.Count)
                m_dblCompletionFraction =
                    (double)m_alPreviousCompletionFraction[m_iIndex++];
            else
                m_dblCompletionFraction = (m_iIndex > 0) ? 1 : 0;
        }

        // Utility function to return elapsed Milliseconds since the 
        // SplashScreen was launched.
        private double ElapsedMilliSeconds()
        {
            TimeSpan ts = DateTime.Now - m_dtStart;
            return ts.TotalMilliseconds;
        }

        // Function to read the checkpoint intervals 
        // from the previous invocation of the
        // splashscreen from the registry.
        private void ReadIncrements()
        {
            string sPBIncrementPerTimerInterval =
                RegistryAccess.GetStringRegistryValue(
                REGVALUE_PB_MILISECOND_INCREMENT, "0.0015");
            double dblResult;

            if (Double.TryParse(sPBIncrementPerTimerInterval,
                System.Globalization.NumberStyles.Float,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out dblResult))
                m_dblPBIncrementPerTimerInterval = dblResult;
            else
                m_dblPBIncrementPerTimerInterval = .0015;

            string sPBPreviousPctComplete = RegistryAccess.GetStringRegistryValue(
                REGVALUE_PB_PERCENTS, "");

            if (sPBPreviousPctComplete != "")
            {
                string[] aTimes = sPBPreviousPctComplete.Split(null);
                m_alPreviousCompletionFraction = new ArrayList();

                for (int i = 0; i < aTimes.Length; i++)
                {
                    double dblVal;
                    if (Double.TryParse(aTimes[i],
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.NumberFormatInfo.InvariantInfo,
                        out dblVal))
                        m_alPreviousCompletionFraction.Add(dblVal);
                    else
                        m_alPreviousCompletionFraction.Add(1.0);
                }
            }
            else
            {
                m_bFirstLaunch = true;
                lblTimeRemaining.Text = "";
            }
        }

        // Method to store the intervals (in percent complete)
        // from the current invocation of
        // the splash screen to the registry.
        private void StoreIncrements()
        {
            string sPercent = "";
            double dblElapsedMilliseconds = ElapsedMilliSeconds();
            for (int i = 0; i < m_alActualTimes.Count; i++)
                sPercent += ((double)m_alActualTimes[i] /
                    dblElapsedMilliseconds).ToString("0.####",
                    System.Globalization.NumberFormatInfo.InvariantInfo) + " ";

            RegistryAccess.SetStringRegistryValue(
                REGVALUE_PB_PERCENTS, sPercent);

            m_dblPBIncrementPerTimerInterval = 1.0 / (double)m_iActualTicks;
            RegistryAccess.SetStringRegistryValue(
                REGVALUE_PB_MILISECOND_INCREMENT,
                m_dblPBIncrementPerTimerInterval.ToString("#.000000",
                System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        #endregion

        #region Event Handlers

        private int timeWhenLastUpdate = 0;
        private bool idle = false;
        private double idlemax = 0;
        private double idleFraction;
        private double idleIncrement = 0.01;

        private int TimeSinceLastUpdate()
        {
            return Console.timeGetTime() - timeWhenLastUpdate;
        }
        //********* Event Handlers ************

        // Tick Event handler for the Timer control.  
        // Handle fade in and fade out.  Also
        // handle the smoothed progress bar.
        private void timer1_Tick(object sender, System.EventArgs e)
        {
            lblStatus.Text = m_sStatus;

            if (m_dblOpacityIncrement > 0)
            {
                m_iActualTicks++;
                if (this.Opacity < 1)
                    this.Opacity += m_dblOpacityIncrement;
            }
            else
            {
                if (this.Opacity > 0)
                    this.Opacity += m_dblOpacityIncrement;
                else
                {
                    StoreIncrements();
                    this.Close();

                    //MW0LGE interesting, but removed
                    //// for -autostart option we run a task here and wait for CPU usage < 75%
                    //TimeSpan ttime;
                    //TimeSpan ttimeDiff;
                    //var t = Task.Run(async delegate
                    //{
                    //    Process process = Process.GetCurrentProcess();
                    //    do
                    //    {
                    //        ttime = process.TotalProcessorTime;
                    //        await Task.Delay(500);
                    //        TimeSpan ttime2 = process.TotalProcessorTime;
                    //        ttimeDiff = ttime2.Subtract(ttime);
                    //    } while (ttimeDiff.TotalMilliseconds > 400);
                    //    Console.setPowerOn();
                    //});
                }
            }

            if (m_bFirstLaunch == false && m_dblLastCompletionFraction
                                            < m_dblCompletionFraction)
            {
                idle = false;
            }
            else
            {
                // no actual progress, but do not 'freeze' (KLJ)
                // m_dblCompletionFraction += 0.0000001;
                if (timeWhenLastUpdate > 0 && TimeSinceLastUpdate() > 400)
                {
                    if (!idle)
                    {
                        idle = true;
                        idleFraction = m_dblLastCompletionFraction;
                        if (idleFraction < 0.01)
                            idlemax = 0.1;
                        else
                            idlemax = idleFraction;

                        idleIncrement = idlemax / 50;
                        if (idleIncrement <= 0)
                        {
                            idleIncrement = 0.01;
                        }
                    }
                }

            }

            int width = (int)Math.Floor(
                 pnlStatus.ClientRectangle.Width * m_dblLastCompletionFraction);
            int height = pnlStatus.ClientRectangle.Height;
            int x = pnlStatus.ClientRectangle.X;
            int y = pnlStatus.ClientRectangle.Y;

            if (m_bFirstLaunch == false && m_dblLastCompletionFraction
            < m_dblCompletionFraction)
            {
                timeWhenLastUpdate = Console.timeGetTime();
                m_dblLastCompletionFraction += m_dblPBIncrementPerTimerInterval;

                if (width > 0 && height > 0)
                {
                    m_rProgress = new Rectangle(x, y, width, height);
                    //MW0LGE pnlStatus.Invalidate(m_rProgress);
                    // this call supresses background draw, which was causing the flicker
                    // annoying that c# invalidate does not have this option.
                    if (pnlStatus != null && !pnlStatus.IsDisposed) InvalidateRect(pnlStatus.Handle, IntPtr.Zero, false);

                    int iSecondsLeft = 1 + (int)(TIMER_INTERVAL *
                        ((1.0 - m_dblLastCompletionFraction) /
                        m_dblPBIncrementPerTimerInterval)) / 1000;
                    if (iSecondsLeft == 1)
                        lblTimeRemaining.Text = string.Format("1 second");
                    else
                        lblTimeRemaining.Text = string.Format("{0} seconds",
                            iSecondsLeft);
                }
            }
            else
            {

                if (idle)
                {

                    idleFraction += idleIncrement;
                    if (idleFraction >= idlemax)
                        idleFraction = 0.001;

                    width = (int)Math.Floor(
                     pnlStatus.ClientRectangle.Width * idleFraction);

                    if (width <= 0)
                    {
                        width = 1;
                    }
                    m_rProgress = new Rectangle(x, y, width, height);
                    if (pnlStatus != null && !pnlStatus.IsDisposed) InvalidateRect(pnlStatus.Handle, IntPtr.Zero, false);
                    Debug.Print("Width of bar is: " + width.ToString());
                }
            }

        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }



        // Paint the portion of the panel invalidated during the tick event.
        private void pnlStatus_Paint(object sender,
            System.Windows.Forms.PaintEventArgs e)
        {
            if (m_bFirstLaunch == false && e.ClipRectangle.Width > 0
                && m_iActualTicks > 1)
            {
                LinearGradientBrush brBackground =
                    new LinearGradientBrush(m_rProgress,
                    Color.FromArgb(100, 100, 100),
                    Color.FromArgb(130, 255, 130),
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brBackground, m_rProgress);
                //FillRoundedRectangle(e.Graphics, brBackground, m_rProgress, 4);

            }
        }

        // Close the form if they double click on it.
        private void SplashScreen_DoubleClick(object sender, System.EventArgs e)
        {
            CloseForm();
        }

        #endregion

        private void Splash_Load(object sender, EventArgs e)
        {

        }

        // KLJ: Make splash window moveable with the mouse.
        private void Splash_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                // Release the mouse capture started by the mouse down.
                this.Capture = false; //select control

                // Create and send a WM_NCLBUTTONDOWN message.
                const int WM_NCLBUTTONDOWN = 0x00A1;
                const int HTCAPTION = 2;
                Message msg =
                    Message.Create(this.Handle, WM_NCLBUTTONDOWN,
                        new IntPtr(HTCAPTION), IntPtr.Zero);
                this.DefWndProc(ref msg);
            }
        }

        private void Splash_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Splash_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void Splash_VisibleChanged(object sender, EventArgs e)
        {

        }
    }

    #region Registry Access Class

    /// A class for managing registry access.
    public class RegistryAccess
    {
        private const string SOFTWARE_KEY = "Software";
        private const string COMPANY_NAME = "OpenHPSDR";
        private const string APPLICATION_NAME = "Thetis";

        // Method for retrieving a Registry Value.
        static public string GetStringRegistryValue(string key,
            string defaultValue)
        {
            RegistryKey rkCompany;
            RegistryKey rkApplication;

            rkCompany = Registry.CurrentUser.OpenSubKey(SOFTWARE_KEY,
                false).OpenSubKey(COMPANY_NAME, false);
            if (rkCompany != null)
            {
                rkApplication = rkCompany.OpenSubKey(APPLICATION_NAME, true);
                if (rkApplication != null)
                {
                    foreach (string sKey in rkApplication.GetValueNames())
                    {
                        if (sKey == key)
                        {
                            return (string)rkApplication.GetValue(sKey);
                        }
                    }
                }
            }
            return defaultValue;
        }

        // Method for storing a Registry Value.
        static public void SetStringRegistryValue(string key,
            string stringValue)
        {
            RegistryKey rkSoftware;
            RegistryKey rkCompany;
            RegistryKey rkApplication;

            rkSoftware = Registry.CurrentUser.OpenSubKey(SOFTWARE_KEY, true);
            rkCompany = rkSoftware.CreateSubKey(COMPANY_NAME);
            if (rkCompany != null)
            {
                rkApplication = rkCompany.CreateSubKey(APPLICATION_NAME);
                if (rkApplication != null)
                {
                    rkApplication.SetValue(key, stringValue);
                }
            }
        }
    }

    #endregion
}
