//=================================================================
// display.cs
//=================================================================
// Thetis is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
// Copyright (C) 2010-2020  Doug Wigley (W5WC)
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
//
// Waterfall AGC Modifications Copyright (C) 2013 Phil Harman (VK6APH)
// MW0LGE all transitions to directX

using System.Linq;

namespace Thetis
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;
    using System.Buffers;
    using System.Diagnostics;
    
    using SharpDX;
    using SharpDX.Direct2D1;
    using SharpDX.Direct3D;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;
    // fix clashes with sharpdx
    using Bitmap = System.Drawing.Bitmap;
    using Rectangle = System.Drawing.Rectangle;
    using Color = System.Drawing.Color;
    using Brush = System.Drawing.Brush;
    using Point = System.Drawing.Point;
    using Pen = System.Drawing.Pen;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;
    using DashStyle = System.Drawing.Drawing2D.DashStyle;
    // SharpDX clashes
    using AlphaMode = SharpDX.Direct2D1.AlphaMode;
    using Device = SharpDX.Direct3D11.Device;
    using RectangleF = SharpDX.RectangleF;
    using SDXPixelFormat = SharpDX.Direct2D1.PixelFormat;
    
    class Display
    {
        #region Variable Declaration

        private const AlphaMode _ALPHA_MODE = AlphaMode.Premultiplied; //21k9

        public const float CLEAR_FLAG = -999.999F;				// for resetting buffers
        public const int BUFFER_SIZE = 16384;

        public static Console console;
        public static SpotControl SpotForm;                     // ke9ns add  communications with spot.cs and dx spotter
        public static string background_image = null;

        private static int[] histogram_data = null;					// histogram display buffer
        private static int[] histogram_history;					// histogram counter

        public static float[] new_display_data;					// Buffer used to store the new data from the DSP for the display
        public static float[] current_display_data;				// Buffer used to store the current data for the display
        public static float[] new_display_data_bottom;
        public static float[] current_display_data_bottom;

        //waterfall
        public static float[] new_waterfall_data;
        public static float[] current_waterfall_data;
        public static float[] new_waterfall_data_bottom;
        public static float[] current_waterfall_data_bottom;

        private static float[] waterfall_data;

        private static SharpDX.Direct2D1.Bitmap _waterfall_bmp_dx2d = null;					// MW0LGE
        private static SharpDX.Direct2D1.Bitmap _waterfall_bmp2_dx2d = null;
        #endregion

        #region Properties

        public static float FrameDelta { get; private set; }

        private static bool tnf_active = true;
        public static bool TNFActive {
            get { return tnf_active; }
            set {
                tnf_active = value;
            }
        }

        private static bool m_bFrameRateIssue = false;
        public static bool FrameRateIssue {
            get { return m_bFrameRateIssue; }
            set { m_bFrameRateIssue = value; }
        }

        private static bool m_bGetPixelsIssue = false;
        public static bool GetPixelsIssue {
            get { return m_bGetPixelsIssue; }
            set { m_bGetPixelsIssue = value; }
        }

        private static bool m_bShowFrameRateIssue = true;
        public static bool ShowFrameRateIssue {
            get { return m_bShowFrameRateIssue; }
            set { m_bShowFrameRateIssue = value; }
        }

        private static bool m_bShowGetPixelsIssue = false;
        public static bool ShowGetPixelsIssue {
            get { return m_bShowGetPixelsIssue; }
            set { m_bShowGetPixelsIssue = value; }
        }

        private static float m_fRX1WaterfallOpacity = 1f;
        public static float RX1WaterfallOpacity {
            get { return m_fRX1WaterfallOpacity; }
            set { m_fRX1WaterfallOpacity = value; }
        }
        private static float m_fRX2WaterfallOpacity = 1f;
        public static float RX2WaterfallOpacity {
            get { return m_fRX2WaterfallOpacity; }
            set { m_fRX2WaterfallOpacity = value; }
        }

        public static Rectangle AGCKnee = new Rectangle();
        public static Rectangle AGCHang = new Rectangle();
        public static Rectangle AGCRX2Knee = new Rectangle();
        public static Rectangle AGCRX2Hang = new Rectangle();

        private static Color notch_callout_active_color = Color.Chartreuse;
        private static Color notch_callout_inactive_color = Color.OrangeRed;

        private static Color notch_highlight_color = Color.Chartreuse;
        private static Color notch_tnf_off_colour = Color.Olive;

        private static Color notch_active_colour = Color.Yellow;
        private static Color notch_inactive_colour = Color.Gray;

        private static Color notch_bw_colour = Color.Yellow;
        private static Color notch_bw_colour_inactive = Color.Gray;

        private static Color channel_background_on = Color.FromArgb(150, Color.DodgerBlue);
        private static Color channel_background_off = Color.FromArgb(100, Color.RoyalBlue);
        private static Color channel_foreground = Color.Cyan;

        private static Pen m_pTNFInactive = new Pen(notch_tnf_off_colour, 1);
        private static Brush m_bTNFInactive = new SolidBrush(changeAlpha(notch_tnf_off_colour, 92));

        private static Pen m_pNotchActive = new Pen(notch_active_colour, 1);
        private static Pen m_pNotchInactive = new Pen(notch_inactive_colour, 1);
        private static Pen m_pHighlighted = new Pen(notch_highlight_color, 1);

        private static Brush m_bBWFillColour = new SolidBrush(changeAlpha(notch_bw_colour, 92));
        private static Brush m_bBWFillColourInactive = new SolidBrush(changeAlpha(notch_bw_colour_inactive, 92));
        private static Brush m_bBWHighlighedFillColour = new SolidBrush(changeAlpha(notch_highlight_color, 92));

        private static Brush m_bTextCallOutActive = new SolidBrush(notch_callout_active_color);
        private static Brush m_bTextCallOutInactive = new SolidBrush(notch_callout_inactive_color);
        private static Font m_fntCallOutFont = new System.Drawing.Font("Trebuchet MS", 9, FontStyle.Regular);

        private static ColorSheme color_sheme = ColorSheme.enhanced;
        public static ColorSheme ColorSheme {
            get { return color_sheme; }

            set { color_sheme = value; }
        }

        private static ColorSheme rx2_color_sheme = ColorSheme.enhanced;
        public static ColorSheme RX2ColorSheme {
            get { return rx2_color_sheme; }

            set { rx2_color_sheme = value; }
        }

        private static bool reverse_waterfall = false;
        public static bool ReverseWaterfall {
            get { return reverse_waterfall; }
            set { reverse_waterfall = value; }
        }

        private static bool pan_fill = false;
        public static bool PanFill {
            get { return pan_fill; }
            set { pan_fill = value; }
        }

        private static bool m_bSpectralPeakHoldRX1 = false;
        private static bool m_bSpectralPeakHoldRX2 = false;
        public static bool SpectralPeakHoldRX1 {
            get { return m_bSpectralPeakHoldRX1; }
            set {
                m_bSpectralPeakHoldRX1 = value;
                if(m_bSpectralPeakHoldRX1)
                {
                    ResetSpectrumPeaks(1);
                }
            }
        }
        public static bool SpectralPeakHoldRX2 {
            get { return m_bSpectralPeakHoldRX2; }
            set {
                m_bSpectralPeakHoldRX2 = value;
                if (m_bSpectralPeakHoldRX2)
                {
                    ResetSpectrumPeaks(2);
                }
            }
        }

        private static bool tx_pan_fill = false;
        public static bool TXPanFill {
            get { return tx_pan_fill; }
            set { tx_pan_fill = value; }
        }

        private static Color pan_fill_color = Color.FromArgb(100, 0, 0, 127);
        public static Color PanFillColor {
            get { return pan_fill_color; }
            set { pan_fill_color = value; }
        }

        private static bool tx_on_vfob = false;
        public static bool TXOnVFOB {
            get { return tx_on_vfob; }
            set {
                tx_on_vfob = value;
            }
        }

        private static bool display_duplex = false;
        public static bool DisplayDuplex {
            get { return display_duplex; }
            set { 
                if(mox && value != display_duplex)
                {
                    // just incase dup is changed whilst tx'ing
                    ResetBlobMaximums(1, true);
                    ResetBlobMaximums(2, true);
                    ResetSpectrumPeaks(1);
                    ResetSpectrumPeaks(2);
                }
                display_duplex = value; 
            }
        }

        private static readonly Object m_objSplitDisplayLock = new Object();
        private static bool split_display = false;
        public static bool SplitDisplay {
            get { return split_display; }
            set {
                lock (m_objSplitDisplayLock)
                {
                    split_display = value;
                }
            }
        }

        private static DisplayMode current_display_mode_bottom = DisplayMode.PANADAPTER;
        public static DisplayMode CurrentDisplayModeBottom {
            get { return current_display_mode_bottom; }
            set {
                bool bDifferent = current_display_mode_bottom != value;
                current_display_mode_bottom = value;
                if (bDifferent)
                {
                    clearBuffers(displayTargetWidth, 2);
                    if (value == DisplayMode.PANAFALL || value == DisplayMode.WATERFALL)
                        ResetWaterfallBmp2();
                }
            }
        }

        private static int rx1_filter_low;
        public static int RX1FilterLow {
            get { return rx1_filter_low; }
            set { rx1_filter_low = value; }
        }

        private static int rx1_filter_high;
        public static int RX1FilterHigh {
            get { return rx1_filter_high; }
            set { rx1_filter_high = value; }
        }

        private static int rx2_filter_low;
        public static int RX2FilterLow {
            get { return rx2_filter_low; }
            set { rx2_filter_low = value; }
        }

        private static int rx2_filter_high;
        public static int RX2FilterHigh {
            get { return rx2_filter_high; }
            set { rx2_filter_high = value; }
        }

        private static int tx_filter_low;
        public static int TXFilterLow {
            get { return tx_filter_low; }
            set { tx_filter_low = value; }
        }

        private static int tx_filter_high;
        public static int TXFilterHigh {
            get { return tx_filter_high; }
            set { tx_filter_high = value; }
        }

        private static bool sub_rx1_enabled = false;
        public static bool SubRX1Enabled {
            get { return sub_rx1_enabled; }
            set {
                sub_rx1_enabled = value;
            }
        }

        private static bool split_enabled = false;
        public static bool SplitEnabled {
            get { return split_enabled; }
            set {
                split_enabled = value;
            }
        }

        private static bool show_freq_offset = false;
        public static bool ShowFreqOffset {
            get { return show_freq_offset; }
            set {
                show_freq_offset = value;
            }
        }

        private static bool show_zero_line = false;
        public static bool ShowZeroLine {
            get { return show_zero_line; }
            set {
                show_zero_line = value;
            }
        }

        private static double freq;
        public static double FREQ {
            get { return freq; }
            set {
                freq = value;
            }
        }


        private static long vfoa_hz;
        public static long VFOA {
            get { return vfoa_hz; }
            set {
                vfoa_hz = value;
            }
        }

        private static long vfoa_sub_hz;
        public static long VFOASub //multi-rx freq
        {
            get { return vfoa_sub_hz; }
            set {
                vfoa_sub_hz = value;
            }
        }

        private static long vfob_hz;
        public static long VFOB //split tx freq
        {
            get { return vfob_hz; }
            set {
                vfob_hz = value;
            }
        }

        private static long vfob_sub_hz;
        public static long VFOBSub {
            get { return vfob_sub_hz; }
            set {
                vfob_sub_hz = value;
            }
        }

        private static int rx_display_bw;
        public static int RXDisplayBW {
            get { return rx_display_bw; }
            set {
                rx_display_bw = value;
            }
        }

        private static int rit_hz;
        public static int RIT {
            get { return rit_hz; }
            set {
                rit_hz = value;
            }
        }

        private static int xit_hz;
        public static int XIT {
            get { return xit_hz; }
            set {
                xit_hz = value;
            }
        }

        private static int freq_diff = 0;
        public static int FreqDiff {
            get { return freq_diff; }
            set {
                freq_diff = value;
            }
        }

        private static int rx2_freq_diff = 0;
        public static int RX2FreqDiff {
            get { return rx2_freq_diff; }
            set {
                rx2_freq_diff = value;
            }
        }

        private static double m_dSpecralPeakHoldDelayRX1 = 100;
        private static double m_dSpecralPeakHoldDelayRX2 = 100;
        public static double SpectralPeakHoldDelayRX1 {
            get { return m_dSpecralPeakHoldDelayRX1;  }
            set { m_dSpecralPeakHoldDelayRX1 = value; }
        }
        public static double SpectralPeakHoldDelayRX2 {
            get { return m_dSpecralPeakHoldDelayRX2; }
            set { m_dSpecralPeakHoldDelayRX2 = value; }
        }

        private static bool m_bAutoAGCRX1 = false;
        private static bool m_bAutoAGCRX2 = false;
        public static bool AutoAGCRX1
        {
            set { m_bAutoAGCRX1 = value; }
        }
        public static bool AutoAGCRX2
        {
            set { m_bAutoAGCRX2 = value; }
        }
        public static void SetupDelegates()
        {
            console.PowerChangeHanders += OnPowerChangeHander;
            console.BandChangeHandlers += OnBandChangeHandler;
            console.MoxChangeHandlers += OnMoxChangeHandler;
            console.AttenuatorDataChangedHandlers += OnAttenuatorDataChanged;
            console.PreampModeChangedHandlers += OnPreampModeChanged;
            console.CentreFrequencyHandlers += OnCentreFrequencyChanged;
        }
        public static void RemoveDelegates()
        {
            console.PowerChangeHanders -= OnPowerChangeHander;
            console.BandChangeHandlers -= OnBandChangeHandler;
            console.MoxChangeHandlers -= OnMoxChangeHandler;
            console.AttenuatorDataChangedHandlers -= OnAttenuatorDataChanged;
            console.PreampModeChangedHandlers -= OnPreampModeChanged;
            console.CentreFrequencyHandlers -= OnCentreFrequencyChanged;
        }

        private static void OnPowerChangeHander(bool oldPower, bool newPower)
        {
            if (newPower)
            {
                FastAttackNoiseFloorRX1 = true;
                FastAttackNoiseFloorRX2 = true;
            }
        }
        private static void OnBandChangeHandler(int rx, Band oldBand, Band newBand)
        {
            if (rx == 1)
                FastAttackNoiseFloorRX1 = true;
            else
                FastAttackNoiseFloorRX2 = true;
        }

        private static bool m_bDelayRX1Blobs = false;
        private static bool m_bDelayRX2Blobs = false;
        private static bool m_bDelayRX1SpectrumPeaks = false;
        private static bool m_bDelayRX2SpectrumPeaks = false;
        private static double m_dPeakDelay = 0;

        //private static HiPerfTimer m_objBlobsActivePeakDisplayDelayTimer = new HiPerfTimer();
        private static void processBlobsActivePeakDisplayDelay()
        {
            if (!m_bDelayRX1Blobs && !m_bDelayRX2Blobs && !m_bDelayRX1SpectrumPeaks && !m_bDelayRX2SpectrumPeaks) return;

            //if (m_objBlobsActivePeakDisplayDelayTimer.ElapsedMsec >= 500)
            if(m_dElapsedFrameStart > m_dPeakDelay)
            {
                if (m_bDelayRX1Blobs) m_bDelayRX1Blobs = false;
                if (m_bDelayRX2Blobs) m_bDelayRX2Blobs = false;
                if (m_bDelayRX1SpectrumPeaks) m_bDelayRX1SpectrumPeaks = false;
                if (m_bDelayRX2SpectrumPeaks) m_bDelayRX2SpectrumPeaks = false;
            }
        }
        private static void delayBlobsActivePeakDisplay(int rx, bool blobs)
        {
            if (rx == 1)
            {
                if (blobs)
                    m_bDelayRX1Blobs = true;
                else
                    m_bDelayRX1SpectrumPeaks = true;
            }
            else
            {
                if (blobs)
                    m_bDelayRX2Blobs = true;
                else
                    m_bDelayRX2SpectrumPeaks = true;
            }

            m_dPeakDelay = m_dElapsedFrameStart + 500;
        }

        private static void OnMoxChangeHandler(int rx, bool oldMox, bool newMox)
        {
            if (oldMox != newMox) resetPeaksAndNoise(); // belts braces
        }

        private static void resetPeaksAndNoise()
        {
            FastAttackNoiseFloorRX1 = true;
            ResetBlobMaximums(1, true);
            ResetSpectrumPeaks(1);

            if (RX2Enabled)
            {
                FastAttackNoiseFloorRX2 = true;
                ResetBlobMaximums(2, true);
                ResetSpectrumPeaks(2);
            }
        }
        private static void OnAttenuatorDataChanged(int rx, int oldAtt, int newAtt)
        {
            if (rx == 1)
                FastAttackNoiseFloorRX1 = true;
            else
                FastAttackNoiseFloorRX2 = true;
        }
        private static void OnPreampModeChanged(int rx, PreampMode oldMode, PreampMode newMode)
        {
            if (rx == 1)
                FastAttackNoiseFloorRX1 = true;
            else
                FastAttackNoiseFloorRX2 = true;
        }
        private static void OnCentreFrequencyChanged(int rx, double oldFreq, double newFreq, Band band)
        {
            if (rx == 1)
            {
                if (Math.Abs(oldFreq - newFreq) > 0.5) FastAttackNoiseFloorRX1 = true;
                ResetBlobMaximums(1, true);
                ResetSpectrumPeaks(1);
            }
            else
            {
                if (Math.Abs(oldFreq - newFreq) > 0.5) FastAttackNoiseFloorRX2 = true;
                ResetBlobMaximums(2, true);
                ResetSpectrumPeaks(2);
            }
        }

        private static bool m_bFastAttackNoiseFloorRX1 = false;
        private static bool m_bFastAttackNoiseFloorRX2 = false;
        public static bool FastAttackNoiseFloorRX1
        {
            get { return m_bFastAttackNoiseFloorRX1; }
            set {
                m_bNoiseFloorGoodRX1 = false;
                m_nAttackFastFramesRX1 = m_nFps;
                m_bFastAttackNoiseFloorRX1 = value; 
            }
        }
        public static bool FastAttackNoiseFloorRX2
        {
            get { return m_bFastAttackNoiseFloorRX2; }
            set {
                m_bNoiseFloorGoodRX2 = false;
                m_nAttackFastFramesRX2 = m_nFps;
                m_bFastAttackNoiseFloorRX2 = value; 
            }
        }

        private static double m_dCentreFreqRX1 = 0;
        public static double CentreFreqRX1 {
            get { return m_dCentreFreqRX1; }
            set {
                m_dCentreFreqRX1 = value;
            }
        }

        private static double m_dCentreFreqRX2 = 0;
        public static double CentreFreqRX2 {
            get { return m_dCentreFreqRX2; }
            set {
                m_dCentreFreqRX2 = value;
            }
        }

        private static int m_nHighlightedBandStackEntryIndex = -1;
        public static int HighlightedBandStackEntryIndex
        {
            get { return m_nHighlightedBandStackEntryIndex; }
            set { m_nHighlightedBandStackEntryIndex = value; }
        }

        private static bool m_bShowBandStackOverlays = false;
        public static bool ShowBandStackOverlays
        {
            get { return m_bShowBandStackOverlays; }
            set { m_bShowBandStackOverlays = value; }
        }

        private static BandStackEntry[] m_bandStackOverlays;
        public static BandStackEntry[] BandStackOverlays
        {
            get { return m_bandStackOverlays; }
            set { m_bandStackOverlays = value; }
        }

        // 0 = none, -1 low edge, +1 high edge, 2 both edges
        private static int m_nHightlightFilterEdgeRX1 = 0;
        public static int HightlightFilterEdgeRX1
        {
            get { return m_nHightlightFilterEdgeRX1; }
            set { m_nHightlightFilterEdgeRX1 = value; }
        }
        private static int m_nHightlightFilterEdgeRX2 = 0;
        public static int HightlightFilterEdgeRX2
        {
            get { return m_nHightlightFilterEdgeRX2; }
            set { m_nHightlightFilterEdgeRX2 = value; }
        }
        private static int m_nHightlightFilterEdgeTX = 0;
        public static int HightlightFilterEdgeTX
        {
            get { return m_nHightlightFilterEdgeTX; }
            set { m_nHightlightFilterEdgeTX = value; }
        }

        private static int cw_pitch = 600;
        public static int CWPitch {
            get { return cw_pitch; }
            set { cw_pitch = value; }
        }

        private static int m_nPhasePointSize = 1;
        public static int PhasePointSize {
            get { return m_nPhasePointSize; }
            set { m_nPhasePointSize = value; }
        }
        private static bool m_bShowFPS = false;
        public static bool ShowFPS {
            get { return m_bShowFPS; }
            set { m_bShowFPS = value; }
        }

        //=======================================================

        private static bool m_bSpecialPanafall = false; // ke9ns add 1=map mode (panafall but only a small waterfall) and only when just in RX1 mode)
        public static int K9 = 0;  // ke9ns add rx1 display mode selector:  1=water,2=pan,3=panfall, 5=panfall with RX2 on any mode, 7=special map viewing panafall
        public static int K10 = 0; // ke9ns add rx2 display mode selector: 0=off 1=water,2=pan, 5=panfall

        //========================================================

        public static bool specready = false;

        private static int displayTargetHeight = 0;	// target height
        private static int displayTargetWidth = 0;	// target width
        private static Control displayTarget = null;
        public static Control Target {
            get { return displayTarget; }
            set {                
                lock (_objDX2Lock)
                {
                    displayTarget = value;

                    displayTargetHeight = displayTarget.Height;
                    displayTargetWidth = Math.Min(displayTarget.Width, BUFFER_SIZE);

                    initDisplayArrays(displayTargetWidth, displayTargetHeight);

                    if (!_bDX2Setup)
                    {
                        initDX2D();
                    }
                    else
                    {
                        ResizeDX2D();
                        ResetWaterfallBmp();
                        ResetWaterfallBmp2();
                    }

                    //Audio.ScopeDisplayWidth = displayTargetWidth / m_nDecimation;

                    if (specready)
                    {
                        console.specRX.GetSpecRX(0).Pixels = displayTargetWidth / m_nDecimation;
                        console.specRX.GetSpecRX(1).Pixels = displayTargetWidth / m_nDecimation;
                        console.specRX.GetSpecRX(cmaster.inid(1, 0)).Pixels = displayTargetWidth / m_nDecimation;
                    }
                }
            }
        }

        private static int m_nDecimation = 1;
        public static int Decimation
        {
            get { return m_nDecimation; }
            set 
            {
                lock (_objDX2Lock)
                {
                    m_nDecimation = value;
                }
            }
        }
        private static int rx_display_low = -4000;
        public static int RXDisplayLow {
            get { return rx_display_low; }
            set {
                if (value != rx_display_low)
                {
                    ResetBlobMaximums(1, true);
                    ResetSpectrumPeaks(1);
                }
                rx_display_low = value; 
            }
        }

        private static int rx_display_high = 4000;
        public static int RXDisplayHigh {
            get { return rx_display_high; }
            set { 
                if(value != rx_display_high)
                {
                    ResetBlobMaximums(1, true);
                    ResetSpectrumPeaks(1);
                }
                rx_display_high = value; 
            }
        }

        private static int rx2_display_low = -4000;
        public static int RX2DisplayLow {
            get { return rx2_display_low; }
            set {
                if (value != rx2_display_low)
                {
                    ResetBlobMaximums(2, true);
                    ResetSpectrumPeaks(2);
                }
                rx2_display_low = value; 
            }
        }

        private static int rx2_display_high = 4000;
        public static int RX2DisplayHigh {
            get { return rx2_display_high; }
            set {
                if (value != rx2_display_high)
                {
                    ResetBlobMaximums(2, true);
                    ResetSpectrumPeaks(2);
                }
                rx2_display_high = value; 
            }
        }

        private static int tx_display_low = -4000;
        public static int TXDisplayLow {
            get { return tx_display_low; }
            set { tx_display_low = value; }
        }

        private static int tx_display_high = 4000;
        public static int TXDisplayHigh {
            get { return tx_display_high; }
            set { tx_display_high = value; }
        }

        private static int rx_spectrum_display_low = -4000;
        public static int RXSpectrumDisplayLow {
            get { return rx_spectrum_display_low; }
            set { rx_spectrum_display_low = value; }
        }

        private static int rx_spectrum_display_high = 4000;
        public static int RXSpectrumDisplayHigh {
            get { return rx_spectrum_display_high; }
            set { rx_spectrum_display_high = value; }
        }

        private static int rx2_spectrum_display_low = -4000;
        public static int RX2SpectrumDisplayLow {
            get { return rx2_spectrum_display_low; }
            set { rx2_spectrum_display_low = value; }
        }

        private static int rx2_spectrum_display_high = 4000;
        public static int RX2SpectrumDisplayHigh {
            get { return rx2_spectrum_display_high; }
            set { rx2_spectrum_display_high = value; }
        }

        private static int tx_spectrum_display_low = -4000;
        public static int TXSpectrumDisplayLow {
            get { return tx_spectrum_display_low; }
            set { tx_spectrum_display_low = value; }
        }

        private static int tx_spectrum_display_high = 4000;
        public static int TXSpectrumDisplayHigh {
            get { return tx_spectrum_display_high; }
            set { tx_spectrum_display_high = value; }
        }

        private static float rx1_preamp_offset = 0.0f;
        public static float RX1PreampOffset {
            get { return rx1_preamp_offset; }
            set { rx1_preamp_offset = value; }
        }

        private static float alex_preamp_offset = 0.0f;
        public static float AlexPreampOffset {
            get { return alex_preamp_offset; }
            set { alex_preamp_offset = value; }
        }

        private static float rx2_preamp_offset = 0.0f;
        public static float RX2PreampOffset {
            get { return rx2_preamp_offset; }
            set { rx2_preamp_offset = value; }
        }

        private static bool tx_display_cal_control = false;
        public static bool TXDisplayCalControl {
            get { return tx_display_cal_control; }
            set { tx_display_cal_control = value; }
        }

        private static float rx1_display_cal_offset;					// display calibration offset in dB
        public static float RX1DisplayCalOffset {
            get { return rx1_display_cal_offset; }
            set {
                rx1_display_cal_offset = value;
            }
        }

        private static float rx2_display_cal_offset;					// display calibration offset in dB
        public static float RX2DisplayCalOffset {
            get { return rx2_display_cal_offset; }
            set { rx2_display_cal_offset = value; }
        }

        private static float rx1_fft_size_offset;					// display calibration offset in dB
        public static float RX1FFTSizeOffset {
            get { return rx1_fft_size_offset; }
            set {
                rx1_fft_size_offset = value;
            }
        }

        private static float rx2_fft_size_offset;					// display calibration offset in dB
        public static float RX2FFTSizeOffset {
            get { return rx2_fft_size_offset; }
            set { rx2_fft_size_offset = value; }
        }


        private static float tx_display_cal_offset = 0f;					// display calibration offset in dB
        public static float TXDisplayCalOffset {
            get { return tx_display_cal_offset; }
            set {
                tx_display_cal_offset = value;
            }
        }

        private static HPSDRModel current_hpsdr_model = HPSDRModel.ANAN7000D;
        public static HPSDRModel CurrentHPSDRModel {
            get { return current_hpsdr_model; }
            set { current_hpsdr_model = value; }
        }

        private static int display_cursor_x;						// x-coord of the cursor when over the display
        public static int DisplayCursorX {
            get { return display_cursor_x; }
            set { display_cursor_x = value; }
        }

        private static int display_cursor_y;						// y-coord of the cursor when over the display
        public static int DisplayCursorY {
            get { return display_cursor_y; }
            set { display_cursor_y = value; }
        }

        private static bool grid_control = false;
        public static bool GridControl {
            get { return grid_control; }
            set { grid_control = value; }
        }

        private static bool show_agc = false;
        public static bool ShowAGC {
            get { return show_agc; }
            set { show_agc = value; }
        }

        private static bool spectrum_line = false;
        public static bool SpectrumLine {
            get { return spectrum_line; }
            set { spectrum_line = value; }
        }

        private static bool display_agc_hang_line = false;
        public static bool DisplayAGCHangLine {
            get { return display_agc_hang_line; }
            set { display_agc_hang_line = value; }
        }

        private static bool rx1_hang_spectrum_line = false;
        public static bool RX1HangSpectrumLine {
            get { return rx1_hang_spectrum_line; }
            set { rx1_hang_spectrum_line = value; }
        }

        private static bool display_rx2_gain_line = false;
        public static bool DisplayRX2GainLine {
            get { return display_rx2_gain_line; }
            set { display_rx2_gain_line = value; }
        }

        private static bool rx2_gain_spectrum_line = false;
        public static bool RX2GainSpectrumLine {
            get { return rx2_gain_spectrum_line; }
            set { rx2_gain_spectrum_line = value; }
        }

        private static bool display_rx2_hang_line = false;
        public static bool DisplayRX2HangLine {
            get { return display_rx2_hang_line; }
            set { display_rx2_hang_line = value; }
        }

        private static bool rx2_hang_spectrum_line = false;
        public static bool RX2HangSpectrumLine {
            get { return rx2_hang_spectrum_line; }
            set { rx2_hang_spectrum_line = value; }
        }

        private static bool tx_grid_control = false;
        public static bool TXGridControl {
            get { return tx_grid_control; }
            set { tx_grid_control = value; }
        }

        private static ClickTuneMode current_click_tune_mode = ClickTuneMode.Off;
        public static ClickTuneMode CurrentClickTuneMode {
            get { return current_click_tune_mode; }
            set { current_click_tune_mode = value; }
        }

        private static int scope_time = 50;
        public static int ScopeTime {
            get { return scope_time; }
            set { scope_time = value; }
        }

        private static int sample_rate_rx1 = 384000;
        public static int SampleRateRX1 {
            get { return sample_rate_rx1; }
            set { sample_rate_rx1 = value; }
        }

        private static int sample_rate_rx2 = 384000;
        public static int SampleRateRX2 {
            get { return sample_rate_rx2; }
            set { sample_rate_rx2 = value; }
        }

        private static int sample_rate_tx = 192000;
        public static int SampleRateTX {
            get { return sample_rate_tx; }
            set { sample_rate_tx = value; }
        }

        private static bool high_swr = false;
        public static bool HighSWR {
            get { return high_swr; }
            set { high_swr = value; }
        }

        //MW0LGE_219k
        //private static DisplayEngine current_display_engine = DisplayEngine.DIRECT_X;//MW0LGE_21g gdi+ DisplayEngine.GDI_PLUS;
        //public static DisplayEngine CurrentDisplayEngine {
        //    get { return current_display_engine; }
        //    set { current_display_engine = value; }
        //}

        private static bool mox = false;
        public static bool MOX {
            get { return mox; }
            set {
                if (value != mox) resetPeaksAndNoise();
                mox = value;               
            }
        }
        
        private static bool m_bShowRX1NoiseFloor = false;
        public static bool ShowRX1NoiseFloor
        {
            get { return m_bShowRX1NoiseFloor; }
            set { m_bShowRX1NoiseFloor = value; }
        }
        private static bool m_bShowRX2NoiseFloor = false;
        public static bool ShowRX2NoiseFloor
        {
            get { return m_bShowRX2NoiseFloor; }
            set { m_bShowRX2NoiseFloor = value; }
        }

        private static bool blank_bottom_display = false;
        public static bool BlankBottomDisplay {
            get { return blank_bottom_display; }
            set { blank_bottom_display = value; }
        }

        private static DSPMode rx1_dsp_mode = DSPMode.USB;
        public static DSPMode RX1DSPMode {
            get { return rx1_dsp_mode; }
            set { rx1_dsp_mode = value; }
        }

        private static DSPMode rx2_dsp_mode = DSPMode.USB;
        public static DSPMode RX2DSPMode {
            get { return rx2_dsp_mode; }
            set { rx2_dsp_mode = value; }
        }

        private static MNotch m_objHightlightedNotch;
        public static MNotch HighlightNotch {
            get { return m_objHightlightedNotch; }
            set { m_objHightlightedNotch = value; }
        }

        private static DisplayMode current_display_mode = DisplayMode.PANAFALL;
        public static DisplayMode CurrentDisplayMode {
            get { return current_display_mode; }
            set {
                bool bDifferent = current_display_mode != value;
                current_display_mode = value;

                if (console.PowerOn)
                    console.pause_DisplayThread = true;

                if (bDifferent)
                {
                    clearBuffers(displayTargetWidth, 1);

                    if (value == DisplayMode.PANAFALL || value == DisplayMode.WATERFALL)
                        ResetWaterfallBmp();
                }

                switch (current_display_mode)
                {
                    case DisplayMode.PHASE2:
                        Audio.phase = true;
                        break;
                    case DisplayMode.SCOPE:
                    case DisplayMode.SCOPE2:
                    case DisplayMode.PANASCOPE:
                    case DisplayMode.SPECTRASCOPE:
                        cmaster.CMSetScopeRun(0, true);
                        break;
                    default:
                        Audio.phase = false;
                        cmaster.CMSetScopeRun(0, false);
                        break;
                }
                console.pause_DisplayThread = false;
            }
        }

        private static float max_x;								// x-coord of maxmimum over one display pass
        public static float MaxX {
            get { return max_x; }
            set { max_x = value; }
        }

        private static float max_y;								// y-coord of maxmimum over one display pass
        public static float MaxY {
            get { return max_y; }
            set { max_y = value; }
        }

        private static bool rx2_enabled = false;
        public static bool RX2Enabled {
            get { return rx2_enabled; }
            set { rx2_enabled = value; }
        }

        private static bool _bRebuildRX1LinearGradBrush = true;
        public static bool RebuildLinearGradientBrushRX1 {
            get { return _bRebuildRX1LinearGradBrush; }
            set {
                _bRebuildRX1LinearGradBrush = value;
            }
        }
        private static bool _bRebuildRX2LinearGradBrush = true;
        public static bool RebuildLinearGradientBrushRX2 {
            get { return _bRebuildRX2LinearGradBrush; }
            set {
                _bRebuildRX2LinearGradBrush = value;
            }
        }
        private static bool data_ready;					// True when there is new display data ready from the DSP
        public static bool DataReady {
            get { return data_ready; }
            set { data_ready = value; }
        }

        private static bool data_ready_bottom;
        public static bool DataReadyBottom {
            get { return data_ready_bottom; }
            set { data_ready_bottom = value; }
        }

        private static bool waterfall_data_ready_bottom;
        public static bool WaterfallDataReadyBottom {
            get { return waterfall_data_ready_bottom; }
            set { waterfall_data_ready_bottom = value; }
        }

        private static bool waterfall_data_ready;
        public static bool WaterfallDataReady {
            get { return waterfall_data_ready; }
            set { waterfall_data_ready = value; }
        }

        private static int spectrum_grid_max = -40;
        public static int SpectrumGridMax {
            get { return spectrum_grid_max; }
            set {
                if (value != spectrum_grid_max) _bRebuildRX1LinearGradBrush = true;
                spectrum_grid_max = value;
            }
        }

        private static int spectrum_grid_min = -140;
        public static int SpectrumGridMin {
            get { return spectrum_grid_min; }
            set {
                if (value != spectrum_grid_min) _bRebuildRX1LinearGradBrush = true;
                spectrum_grid_min = value;
            }
        }

        private static int spectrum_grid_step = 5;
        public static int SpectrumGridStep {
            get { return spectrum_grid_step; }
            set {
                if (value != spectrum_grid_step) _bRebuildRX1LinearGradBrush = true;
                spectrum_grid_step = value;
            }
        }

        public static int SpectrumGridMaxMoxModified
        {
            get
            {
                bool local_mox = mox && (!tx_on_vfob || (tx_on_vfob && !rx2_enabled));
                if (local_mox)
                    return tx_spectrum_grid_max;
                else
                    return spectrum_grid_max;
            }
        }

        public static int SpectrumGridMinMoxModified
        {
            get
            {
                bool local_mox = mox && (!tx_on_vfob || (tx_on_vfob && !rx2_enabled));
                if (local_mox)
                    return tx_spectrum_grid_min;
                else
                    return spectrum_grid_min;
            }
        }

        public static int SpectrumGridStepMoxModified
        {
            get
            {
                bool local_mox = mox && (!tx_on_vfob || (tx_on_vfob && !rx2_enabled));
                if (local_mox)
                    return tx_spectrum_grid_step;
                else
                    return spectrum_grid_step;
            }
        }
        public static int RX2SpectrumGridMaxMoxModified
        {
            get
            {
                bool local_mox = mox && (tx_on_vfob && rx2_enabled);
                if (local_mox)
                    return tx_spectrum_grid_max;
                else
                    return rx2_spectrum_grid_max;
            }
        }

        public static int RX2SpectrumGridMinMoxModified
        {
            get
            {
                bool local_mox = mox && (tx_on_vfob && rx2_enabled);
                if (local_mox)
                    return tx_spectrum_grid_min;
                else
                    return rx2_spectrum_grid_min;
            }
        }

        public static int RX2SpectrumGridStepMoxModified
        {
            get
            {
                bool local_mox = mox && (tx_on_vfob && rx2_enabled);
                if (local_mox)
                    return tx_spectrum_grid_step;
                else
                    return rx2_spectrum_grid_step;
            }
        }
        //

        private static int rx2_spectrum_grid_max = -40;
        public static int RX2SpectrumGridMax {
            get { return rx2_spectrum_grid_max; }
            set {
                if (value != rx2_spectrum_grid_max) _bRebuildRX2LinearGradBrush = true;
                rx2_spectrum_grid_max = value;
            }
        }

        private static int rx2_spectrum_grid_min = -140;
        public static int RX2SpectrumGridMin {
            get { return rx2_spectrum_grid_min; }
            set {
                if (value != rx2_spectrum_grid_min) _bRebuildRX2LinearGradBrush = true;
                rx2_spectrum_grid_min = value;
            }
        }

        private static int rx2_spectrum_grid_step = 5;
        public static int RX2SpectrumGridStep {
            get { return rx2_spectrum_grid_step; }
            set {
                if (value != rx2_spectrum_grid_step) _bRebuildRX2LinearGradBrush = true;
                rx2_spectrum_grid_step = value;
            }
        }

        private static int tx_spectrum_grid_max = 20;
        public static int TXSpectrumGridMax {
            get { return tx_spectrum_grid_max; }
            set {
                tx_spectrum_grid_max = value;
            }
        }

        private static int tx_spectrum_grid_min = -80;
        public static int TXSpectrumGridMin {
            get { return tx_spectrum_grid_min; }
            set {
                tx_spectrum_grid_min = value;
            }
        }

        private static int tx_spectrum_grid_step = 5;
        public static int TXSpectrumGridStep {
            get { return tx_spectrum_grid_step; }
            set {
                tx_spectrum_grid_step = value;
            }
        }

        private static int tx_wf_amp_max = 30;
        public static int TXWFAmpMax {
            get { return tx_wf_amp_max; }
            set {
                tx_wf_amp_max = value;
            }
        }

        private static int tx_wf_amp_min = -70;
        public static int TXWFAmpMin {
            get { return tx_wf_amp_min; }
            set {
                tx_wf_amp_min = value;
            }
        }

        private static Color band_edge_color = Color.Red;
        private static Pen band_edge_pen = new Pen(band_edge_color);
        public static Color BandEdgeColor {
            get { return band_edge_color; }
            set {
                lock (_objDX2Lock)
                {
                    band_edge_color = value;
                    band_edge_pen.Color = band_edge_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color tx_band_edge_color = Color.Red;
        private static Pen tx_band_edge_pen = new Pen(tx_band_edge_color);
        public static Color TXBandEdgeColor {
            get { return tx_band_edge_color; }
            set {
                lock (_objDX2Lock)
                {
                    tx_band_edge_color = value;
                    tx_band_edge_pen.Color = tx_band_edge_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color sub_rx_zero_line_color = Color.LightSkyBlue;
        private static Pen sub_rx_zero_line_pen = new Pen(sub_rx_zero_line_color, 2); // MW0LGE width 2
        public static Color SubRXZeroLine {
            get { return sub_rx_zero_line_color; }
            set {
                lock (_objDX2Lock)
                {
                    sub_rx_zero_line_color = value;
                    sub_rx_zero_line_pen.Color = sub_rx_zero_line_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color sub_rx_filter_color = Color.Blue;
        private static SolidBrush sub_rx_filter_brush = new SolidBrush(sub_rx_filter_color);
        public static Color SubRXFilterColor {
            get { return sub_rx_filter_color; }
            set {
                lock (_objDX2Lock)
                {
                    sub_rx_filter_color = value;
                    sub_rx_filter_brush.Color = sub_rx_filter_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color grid_text_color = Color.Yellow;
        private static SolidBrush grid_text_brush = new SolidBrush(grid_text_color);
        private static Pen grid_text_pen = new Pen(grid_text_color);
        public static Color GridTextColor {
            get { return grid_text_color; }
            set {
                lock (_objDX2Lock)
                {
                    grid_text_color = value;
                    grid_text_brush.Color = grid_text_color;
                    grid_text_pen.Color = grid_text_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color grid_tx_text_color = Color.FromArgb(255, Color.Yellow);
        private static SolidBrush grid_tx_text_brush = new SolidBrush(Color.FromArgb(255, grid_tx_text_color));
        public static Color GridTXTextColor {
            get { return grid_tx_text_color; }
            set {
                lock (_objDX2Lock)
                {
                    grid_tx_text_color = value;
                    grid_tx_text_brush.Color = grid_tx_text_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color grid_zero_color = Color.Red;
        private static Pen grid_zero_pen = new Pen(grid_zero_color, 2); // MW0LGE width 2
        public static Color GridZeroColor {
            get { return grid_zero_color; }
            set {
                lock (_objDX2Lock)
                {
                    grid_zero_color = value;
                    grid_zero_pen.Color = grid_zero_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color tx_grid_zero_color = Color.FromArgb(255, Color.Red);
        private static Pen tx_grid_zero_pen = new Pen(Color.FromArgb(255, tx_grid_zero_color), 2); //MW0LGE width 2
        public static Color TXGridZeroColor {
            get { return tx_grid_zero_color; }
            set {
                lock (_objDX2Lock)
                {
                    tx_grid_zero_color = value;
                    tx_grid_zero_pen.Color = tx_grid_zero_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color grid_color = Color.FromArgb(65, 255, 255, 255);
        private static Pen grid_pen = new Pen(grid_color);
        public static Color GridColor {
            get { return grid_color; }
            set {
                lock (_objDX2Lock)
                {
                    grid_color = value;
                    grid_pen.Color = grid_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color tx_vgrid_color = Color.FromArgb(65, 255, 255, 255);
        private static Pen tx_vgrid_pen = new Pen(tx_vgrid_color);
        public static Color TXVGridColor {
            get { return tx_vgrid_color; }
            set {
                lock (_objDX2Lock)
                {
                    tx_vgrid_color = value;
                    tx_vgrid_pen.Color = tx_vgrid_color;
                    buildDX2Resources();
                }
            }
        }


        private static Color hgrid_color = Color.White;
        private static Pen hgrid_pen = new Pen(hgrid_color);
        public static Color HGridColor {
            get { return hgrid_color; }
            set {
                lock (_objDX2Lock)
                {
                    hgrid_color = value;
                    hgrid_pen.Color = hgrid_color;//new Pen(hgrid_color);
                    buildDX2Resources();
                }
            }
        }


        private static Color tx_hgrid_color = Color.White;
        private static Pen tx_hgrid_pen = new Pen(tx_hgrid_color);
        public static Color TXHGridColor {
            get { return tx_hgrid_color; }
            set {
                lock (_objDX2Lock)
                {
                    tx_hgrid_color = value;
                    tx_hgrid_pen.Color = tx_hgrid_color;//new Pen(tx_hgrid_color);
                    buildDX2Resources();
                }
            }
        }

        //MW0LGE
        private static Pen peak_blob_pen = new Pen(Color.OrangeRed);
        private static Pen peak_blob_text_pen = new Pen(Color.YellowGreen);
        private static Color data_fill_color = Color.FromArgb(128, Color.Blue);
        private static Color dataPeaks_fill_color = Color.FromArgb(128, Color.Gray);
        private static Pen data_fill_fpen = new Pen(data_fill_color);
        private static Pen dataPeaks_fill_fpen = new Pen(dataPeaks_fill_color);
        public static Color DataFillColor {
            get { return data_fill_color; }
            set {
                lock (_objDX2Lock)
                {
                    data_fill_color = value;
                    data_fill_fpen.Color = data_fill_color;
                    buildDX2Resources();
                }
            }
        }
        public static Color DataPeaksFillColor {
            get { return dataPeaks_fill_color; }
            set {
                lock (_objDX2Lock)
                {
                    dataPeaks_fill_color = value;
                    dataPeaks_fill_fpen.Color = dataPeaks_fill_color;
                    buildDX2Resources();
                }
            }
        }
        private static Color data_line_color = Color.White;
        private static Pen data_line_pen = new Pen(new SolidBrush(data_line_color), 1.0F);
        public static Color DataLineColor {
            get { return data_line_color; }
            set {
                lock (_objDX2Lock)
                {
                    data_line_color = value;
                    data_line_pen.Color = data_line_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color tx_data_line_color = Color.White;
        private static Pen tx_data_line_pen = new Pen(new SolidBrush(tx_data_line_color), 1.0F);
        private static Pen tx_data_line_fpen = new Pen(Color.FromArgb(100, tx_data_line_color));
        public static Color TXDataLineColor {
            get { return tx_data_line_color; }
            set {
                lock (_objDX2Lock)
                {
                    tx_data_line_color = value;
                    tx_data_line_pen.Color = tx_data_line_color;
                    tx_data_line_fpen.Color = Color.FromArgb(100, tx_data_line_color);
                    buildDX2Resources();
                }
            }
        }

        private static Color grid_pen_dark = Color.FromArgb(65, 255, 255, 255);
        private static Pen grid_pen_inb = new Pen(grid_pen_dark);
        public static Color GridPenDark {
            get { return grid_pen_dark; }
            set {
                lock (_objDX2Lock)
                {
                    grid_pen_dark = value;
                    grid_pen_inb.Color = grid_pen_dark;
                    buildDX2Resources();
                }
            }
        }

        private static Color tx_vgrid_pen_fine = Color.FromArgb(65, 255, 255, 255);
        private static Pen tx_vgrid_pen_inb = new Pen(tx_vgrid_pen_fine);
        public static Color TXVGridPenFine {
            get { return tx_vgrid_pen_fine; }
            set {
                lock (_objDX2Lock)
                {
                    tx_vgrid_pen_fine = value;
                    tx_vgrid_pen_inb.Color = tx_vgrid_pen_fine;
                    buildDX2Resources();
                }
            }
        }

        private static Color bandstack_overlay_color = Color.FromArgb(192, 192, 64, 0);
        private static SolidBrush bandstack_overlay_brush = new SolidBrush(bandstack_overlay_color);
        private static SolidBrush bandstack_overlay_brush_lines = new SolidBrush(Color.FromArgb(Math.Min(bandstack_overlay_color.A + 64, 255), bandstack_overlay_color));
        private static SolidBrush bandstack_overlay_brush_highlight = new SolidBrush(Color.FromArgb(Math.Min(bandstack_overlay_color.A + 64,255), bandstack_overlay_color));
        public static Color BandstackOverlayColor
        {
            get { return bandstack_overlay_color; }
            set
            {
                lock (_objDX2Lock)
                {
                    bandstack_overlay_color = value;
                    bandstack_overlay_brush.Color = bandstack_overlay_color;
                    bandstack_overlay_brush_lines.Color = Color.FromArgb(Math.Min(bandstack_overlay_color.A + 64, 255), bandstack_overlay_color);
                    bandstack_overlay_brush_highlight.Color = Color.FromArgb(Math.Min(bandstack_overlay_color.A + 64, 255), bandstack_overlay_color);
                    buildDX2Resources();
                }
            }
        }

        private static Color display_filter_color = Color.FromArgb(65, 255, 255, 255);
        private static SolidBrush display_filter_brush = new SolidBrush(display_filter_color);
        private static Pen cw_zero_pen = new Pen(Color.FromArgb(255, display_filter_color), 2); // MW0LGE width 2
        public static Color DisplayFilterColor {
            get { return display_filter_color; }
            set {
                lock (_objDX2Lock)
                {
                    display_filter_color = value;
                    display_filter_brush.Color = display_filter_color;
                    cw_zero_pen.Color = Color.FromArgb(255, display_filter_color);
                    buildDX2Resources();
                }
            }
        }

        private static Color tx_filter_color = Color.FromArgb(65, 255, 255, 255);
        private static SolidBrush tx_filter_brush = new SolidBrush(tx_filter_color);
        public static Color TXFilterColor {
            get { return tx_filter_color; }
            set {
                lock (_objDX2Lock)
                {
                    tx_filter_color = value;
                    tx_filter_brush.Color = tx_filter_color;
                    buildDX2Resources();
                }
            }
        }

        private static bool m_bShowNoiseFloorDBM = true;
        public static bool ShowNoiseFloorDBM
        {
            get { return m_bShowNoiseFloorDBM; }
            set { m_bShowNoiseFloorDBM = value; }
        }
        private static float m_fNoiseFloorLineWidth = 1.0f;
        public static float NoiseFloorLineWidth
        {
            get { return m_fNoiseFloorLineWidth; }
            set { m_fNoiseFloorLineWidth = value; }
        }
        private static Color noisefloor_color = Color.Red;
        public static Color NoiseFloorColor
        {
            get { return noisefloor_color; }
            set
            {
                lock (_objDX2Lock)
                {
                    noisefloor_color = value;
                    buildDX2Resources();
                }
            }
        }
        private static float m_fWaterfallAGCOffsetRX1 = 0.0f;
        private static float m_fWaterfallAGCOffsetRX2 = 0.0f;
        public static float WaterfallAGCOffsetRX1
        {
            get { return m_fWaterfallAGCOffsetRX1; }
            set { m_fWaterfallAGCOffsetRX1 = value; }
        }
        public static float WaterfallAGCOffsetRX2
        {
            get { return m_fWaterfallAGCOffsetRX2; }
            set { m_fWaterfallAGCOffsetRX2 = value; }
        }
        private static bool m_bWaterfallUseNFForACGRX1 = false;
        private static bool m_bWaterfallUseNFForACGRX2 = false;
        public static bool WaterfallUseNFForACGRX1
        {
            get { return m_bWaterfallUseNFForACGRX1; }
            set { m_bWaterfallUseNFForACGRX1 = value; }
        }
        public static bool WaterfallUseNFForACGRX2
        {
            get { return m_bWaterfallUseNFForACGRX2; }
            set { m_bWaterfallUseNFForACGRX2 = value; }
        }

        private static Color display_filter_tx_color = Color.Yellow;
        private static Pen tx_filter_pen = new Pen(display_filter_tx_color, 2); // width 2 MW0LGE
        public static Color DisplayFilterTXColor {
            get { return display_filter_tx_color; }
            set {
                lock (_objDX2Lock)
                {
                    display_filter_tx_color = value;
                    tx_filter_pen.Color = display_filter_tx_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color display_background_color = Color.Black;
        private static SolidBrush display_background_brush = new SolidBrush(display_background_color);
        public static Color DisplayBackgroundColor {
            get { return display_background_color; }
            set {
                lock (_objDX2Lock)
                {
                    display_background_color = value;
                    display_background_brush.Color = display_background_color;
                    buildDX2Resources();
                }
            }
        }

        private static Color tx_display_background_color = Color.Black;
        private static SolidBrush tx_display_background_brush = new SolidBrush(tx_display_background_color);
        public static Color TXDisplayBackgroundColor {
            get { return tx_display_background_color; }
            set {
                lock (_objDX2Lock)
                {
                    tx_display_background_color = value;
                    tx_display_background_brush.Color = tx_display_background_color;
                    buildDX2Resources();
                }
            }
        }

        //MW0LGE
        private static bool m_bShowTXFilterOnWaterfall = false;
        public static bool ShowTXFilterOnWaterfall {
            get { return m_bShowTXFilterOnWaterfall; }
            set {
                m_bShowTXFilterOnWaterfall = value;
            }
        }
        private static bool m_bShowRXFilterOnWaterfall = false;
        public static bool ShowRXFilterOnWaterfall {
            get { return m_bShowRXFilterOnWaterfall; }
            set {
                m_bShowRXFilterOnWaterfall = value;
            }
        }
        private static bool m_bShowTXZeroLineOnWaterfall = false;
        public static bool ShowTXZeroLineOnWaterfall {
            get { return m_bShowTXZeroLineOnWaterfall; }
            set {
                m_bShowTXZeroLineOnWaterfall = value;
            }
        }
        private static bool m_bShowRXZeroLineOnWaterfall = false;
        public static bool ShowRXZeroLineOnWaterfall {
            get { return m_bShowRXZeroLineOnWaterfall; }
            set {
                m_bShowRXZeroLineOnWaterfall = value;
            }
        }
        private static bool m_bShowTXFilterOnRXWaterfall = false;
        public static bool ShowTXFilterOnRXWaterfall {
            get { return m_bShowTXFilterOnRXWaterfall; }
            set {
                m_bShowTXFilterOnRXWaterfall = value;
            }
        }

        private static bool draw_tx_filter = false;
        public static bool DrawTXFilter {
            get { return draw_tx_filter; }
            set {
                draw_tx_filter = value;
            }
        }

        private static bool show_cwzero_line = false;
        public static bool ShowCWZeroLine {
            get { return show_cwzero_line; }
            set {
                show_cwzero_line = value;
            }
        }

        private static bool draw_tx_cw_freq = false;
        public static bool DrawTXCWFreq {
            get { return draw_tx_cw_freq; }
            set {
                draw_tx_cw_freq = value;
            }
        }

        private static Color waterfall_low_color = Color.Black;
        public static Color WaterfallLowColor {
            get { return waterfall_low_color; }
            set { waterfall_low_color = value; }
        }

        private static Color waterfall_mid_color = Color.Red;
        public static Color WaterfallMidColor {
            get { return waterfall_mid_color; }
            set { waterfall_mid_color = value; }
        }

        private static Color waterfall_high_color = Color.Yellow;
        public static Color WaterfallHighColor {
            get { return waterfall_high_color; }
            set { waterfall_high_color = value; }
        }

        private static float waterfall_high_threshold = -80.0F;
        public static float WaterfallHighThreshold {
            get { return waterfall_high_threshold; }
            set { waterfall_high_threshold = value; }
        }

        private static float waterfall_low_threshold = -130.0F;
        public static float WaterfallLowThreshold {
            get { return waterfall_low_threshold; }
            set { waterfall_low_threshold = value; }
        }

        //================================================================
        // ke9ns add signal from console about Grayscale ON/OFF
        private static byte Gray_Scale = 0; //  ke9ns ADD from console 0=RGB  1=Gray
        public static byte GrayScale       // this is called or set in console
        {
            get { return Gray_Scale; }
            set {
                Gray_Scale = value;
            }
        }


        //================================================================
        // kes9ns add signal from setup grid lines on/off
        private static byte grid_off = 0; //  ke9ns ADD from setup 0=normal  1=gridlines off
        public static byte GridOff       // this is called or set in setup
        {
            get { return grid_off; }
            set {
                grid_off = value;
            }
        }


        private static Color rx2_waterfall_low_color = Color.Black;
        public static Color RX2WaterfallLowColor {
            get { return rx2_waterfall_low_color; }
            set { rx2_waterfall_low_color = value; }
        }

        private static Color rx2_waterfall_mid_color = Color.Red;
        public static Color RX2WaterfallMidColor {
            get { return rx2_waterfall_mid_color; }
            set { rx2_waterfall_mid_color = value; }
        }

        private static Color rx2_waterfall_high_color = Color.Yellow;
        public static Color RX2WaterfallHighColor {
            get { return rx2_waterfall_high_color; }
            set { rx2_waterfall_high_color = value; }
        }

        private static float rx2_waterfall_high_threshold = -80.0F;
        public static float RX2WaterfallHighThreshold {
            get { return rx2_waterfall_high_threshold; }
            set { rx2_waterfall_high_threshold = value; }
        }

        private static float rx2_waterfall_low_threshold = -130.0F;
        public static float RX2WaterfallLowThreshold {
            get { return rx2_waterfall_low_threshold; }
            set { rx2_waterfall_low_threshold = value; }
        }

        private static float display_line_width = 1.0F;
        public static float DisplayLineWidth {
            get { return display_line_width; }
            set {
                lock (_objDX2Lock)
                {
                    display_line_width = value;
                    data_line_pen.Width = display_line_width;
                }
            }
        }

        private static float tx_display_line_width = 1.0F;
        public static float TXDisplayLineWidth {
            get { return tx_display_line_width; }
            set {
                lock (_objDX2Lock)
                {
                    tx_display_line_width = value;
                    tx_data_line_pen.Width = tx_display_line_width;
                }
            }
        }

        private static DisplayLabelAlignment display_label_align = DisplayLabelAlignment.LEFT;
        public static DisplayLabelAlignment DisplayLabelAlign {
            get { return display_label_align; }
            set {
                display_label_align = value;
            }
        }

        private static DisplayLabelAlignment tx_display_label_align = DisplayLabelAlignment.CENTER;
        public static DisplayLabelAlignment TXDisplayLabelAlign {
            get { return tx_display_label_align; }
            set {
                tx_display_label_align = value;
            }
        }

        private static int phase_num_pts = 100;
        public static int PhaseNumPts {
            get { return phase_num_pts; }
            set {
                lock (_objDX2Lock)
                {
                    phase_num_pts = value;
                }
            }
        }

        private static bool click_tune_filter = false;
        public static bool ClickTuneFilter {
            get { return click_tune_filter; }
            set { click_tune_filter = value; }
        }

        private static bool show_cth_line = false;
        public static bool ShowCTHLine {
            get { return show_cth_line; }
            set { show_cth_line = value; }
        }

        private static double f_center = vfoa_hz;
        public static double F_Center {
            get { return f_center; }
            set {
                f_center = value;
            }
        }

        private static int top_size = 0;
        public static int TopSize {
            get { return top_size; }
            set { top_size = value; }
        }

        private static int _lin_corr = 2;
        public static int LinCor {
            get {
                return _lin_corr;
            }
            set {
                _lin_corr = value;
            }
        }

        private static int _linlog_corr = -14;
        public static int LinLogCor {
            get {
                return _linlog_corr;
            }
            set {
                _linlog_corr = value;
            }
        }

        private static SolidBrush pana_text_brush = new SolidBrush(Color.Khaki);
        private static System.Drawing.Font pana_font = new System.Drawing.Font("Tahoma", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

        private static Pen dhp = new Pen(Color.FromArgb(0, 255, 0)),
                           dhp1 = new Pen(Color.FromArgb(150, 0, 0, 255)),
                           dhp2 = new Pen(Color.FromArgb(150, 255, 0, 0));

        private static System.Drawing.Font
                            font10 = new System.Drawing.Font("Arial", 10),
                            font14 = new System.Drawing.Font("Arial", 14, FontStyle.Bold),
                            font9 = new System.Drawing.Font("Arial", 9),
                            font9b = new System.Drawing.Font("Arial", 9, FontStyle.Bold),
                            font95 = new System.Drawing.Font("Arial", 9.5f),
                            font32b = new System.Drawing.Font("Arial", 32, FontStyle.Bold);

        #endregion

        #region General Routines


        #region GDI+ General Routines

        // use pools to reduce GC
        private static ArrayPool<float> m_objFloatPool = ArrayPool<float>.Shared;
        private static ArrayPool<int> m_objIntPool = ArrayPool<int>.Shared;

        private static void clearBuffers(int W, int rx)
        {
            ResetBlobMaximums(rx, true);
            ResetSpectrumPeaks(rx);

            if (rx == 1)
            {
                Parallel.For(0, W, (i) => //for (int i = 0; i < displayTargetWidth; i++)
                {
                    histogram_data[i] = Int32.MaxValue;
                    histogram_history[i] = 0;
                });
                Parallel.For(0, W, (i) => //for (int i = 0; i < displayTargetWidth; i++)
                {
                    new_display_data[i] = -200.0f;
                    current_display_data[i] = -200.0f;
                    new_waterfall_data[i] = -200.0f;
                    current_waterfall_data[i] = -200.0f;
                });

                FastAttackNoiseFloorRX1 = true;
            }
            else
            {
                Parallel.For(0, W, (i) => //for (int i = 0; i < displayTargetWidth; i++)
                {
                    new_display_data_bottom[i] = -200.0f;
                    current_display_data_bottom[i] = -200.0f;
                    new_waterfall_data_bottom[i] = -200.0f;
                    current_waterfall_data_bottom[i] = -200.0f;
                });

                FastAttackNoiseFloorRX2 = true;
            }
        }

        private static void initDisplayArrays(int W, int H)
        {
            lock (_objDX2Lock)
            {
                if (histogram_data != null) m_objIntPool.Return(histogram_data);
                if (histogram_history != null) m_objIntPool.Return(histogram_history);

                histogram_data = m_objIntPool.Rent(W);
                histogram_history = m_objIntPool.Rent(W);

                //Parallel.For(0, W, (i) => //for (int i = 0; i < displayTargetWidth; i++)
                //{
                //    histogram_data[i] = Int32.MaxValue;
                //    histogram_history[i] = 0;
                //});

                if (new_display_data != null) m_objFloatPool.Return(new_display_data);
                if (current_display_data != null) m_objFloatPool.Return(current_display_data);

                if (new_display_data_bottom != null) m_objFloatPool.Return(new_display_data_bottom);
                if (current_display_data_bottom != null) m_objFloatPool.Return(current_display_data_bottom);

                if (new_waterfall_data != null) m_objFloatPool.Return(new_waterfall_data);
                if (current_waterfall_data != null) m_objFloatPool.Return(current_waterfall_data);

                if (new_waterfall_data_bottom != null) m_objFloatPool.Return(new_waterfall_data_bottom);
                if (current_waterfall_data_bottom != null) m_objFloatPool.Return(current_waterfall_data_bottom);

                // cant be W width, as more info can be stored in these, for example scope data
                new_display_data = m_objFloatPool.Rent(BUFFER_SIZE);
                current_display_data = m_objFloatPool.Rent(BUFFER_SIZE);

                new_display_data_bottom = m_objFloatPool.Rent(BUFFER_SIZE);
                current_display_data_bottom = m_objFloatPool.Rent(BUFFER_SIZE);

                new_waterfall_data = m_objFloatPool.Rent(W);
                current_waterfall_data = m_objFloatPool.Rent(W);

                new_waterfall_data_bottom = m_objFloatPool.Rent(W);
                current_waterfall_data_bottom = m_objFloatPool.Rent(W);

                m_rx1_spectrumPeaks = new Maximums[W];
                m_rx2_spectrumPeaks = new Maximums[W];

                //ResetBlobMaximums(1, true);
                //ResetBlobMaximums(2, true);
                //ResetSpectrumPeaks(1);
                //ResetSpectrumPeaks(2);
                //FastAttackNoiseFloorRX1 = true;
                //FastAttackNoiseFloorRX2 = true;

                //Parallel.For(0, W, (i) => //for (int i = 0; i < displayTargetWidth; i++)
                //{
                //    new_display_data[i] = -200.0f;
                //    current_display_data[i] = -200.0f;
                //    new_display_data_bottom[i] = -200.0f;
                //    current_display_data_bottom[i] = -200.0f;
                //    new_waterfall_data[i] = -200.0f;
                //    current_waterfall_data[i] = -200.0f;
                //    new_waterfall_data_bottom[i] = -200.0f;
                //    current_waterfall_data_bottom[i] = -200.0f;
                //});

                clearBuffers(W, 1);
                clearBuffers(W, 2);
            }
        }

        #region Drawing Routines
        // ======================================================
        // Drawing Routines
        // ======================================================


        //=========================================================
        // ke9ns draw panadapter grid
        //=========================================================

        public static int[] holder2 = new int[100];                           // ke9ns add MEMORY Spot used to allow the vertical lines to all be drawn first so the call sign text can draw over the top of it.
        public static int[] holder3 = new int[100];                          // ke9ns add

        public static int[] holder = new int[100];                           // ke9ns add DX Spot used to allow the vertical lines to all be drawn first so the call sign text can draw over the top of it.
        public static int[] holder1 = new int[100];                          // ke9ns add
        private static Font font1 = new Font("Ariel", 9, FontStyle.Regular);  // ke9ns add dx spot call sign font style

        private static Pen p1 = new Pen(Color.YellowGreen, 2.0f);             // ke9ns add vert line color and thickness  DXSPOTTER
        private static Pen p3 = new Pen(Color.Blue, 2.5f);                   // ke9ns add vert line color and thickness    MEMORY
        private static Pen p2 = new Pen(Color.Purple, 2.0f);                  // ke9ns add color for vert line of SWL list

        private static bool m_bLSB = false;                                     // ke9ns add true=LSB, false=USB
        private static int rx2 = 0;                                          // ke9ns add 0-49 spots for rx1 panadapter window for qrz callup  (50-100 for rx2)

        public static int VFOLow = 0;                                       // ke9ns low freq (left side of screen) in HZ (used in DX_spot)
        public static int VFOHigh = 0;                                      // ke9ns high freq (right side of screen) in HZ
        public static int VFODiff = 0;                                      // ke9ns diff high-low

        private static Color changeAlpha(Color c, int A)
        {
            return Color.FromArgb(A, c.R, c.G, c.B);
        }

        private static float dBToPixel(float dB, int H)
        {
            return (float)(spectrum_grid_max - dB) * H / (spectrum_grid_max - spectrum_grid_min);
        }

        private static float dBToRX2Pixel(float dB, int H)
        {
            return (float)(rx2_spectrum_grid_max - dB) * H / (rx2_spectrum_grid_max - rx2_spectrum_grid_min);
        }

        private static float PixelToDb(float y, int H)
        {
            //commented out MW0LGE_21k9
            //if (y <= H / 2) y *= 2.0f;
            //else y = (y - H / 2) * 2.0f;

            return (float)(spectrum_grid_max - y * (double)(spectrum_grid_max - spectrum_grid_min) / H);
        }

        private static float[] scope_min;// = new float[displayTargetWidth];
        //public static float[] ScopeMin {
        //    get { return scope_min; }
        //    set { scope_min = value; }
        //}
        private static float[] scope_max;// = new float[displayTargetWidth];
        //public static float[] ScopeMax {
        //    get { return scope_max; }
        //    set { scope_max = value; }
        //}

        private static Point[] points;
        private static Point[] pointsStore1;
        private static Point[] pointsStore2;
        private static int lastResize = -999; // -999 startup state, ensures arrays will be generated

        private static void updateSharePointsArray(int nW)
        {
            // purpose of function is to limit the number of times the arrays are built, to reduce GC work
            if (nW == lastResize)
            {
                points = pointsStore1;
            }
            else if (nW == lastResize + 2)
            {
                points = pointsStore2;
            }
            else
            {
                pointsStore1 = new Point[nW];
                pointsStore2 = new Point[nW + 2];
                points = pointsStore1;
                lastResize = nW;
            }
        }

        private static float[] scope2_min = new float[displayTargetWidth];
        public static float[] Scope2Min {
            get { return scope2_min; }
            set { scope2_min = value; }
        }
        private static float[] scope2_max = new float[displayTargetWidth];
        public static float[] Scope2Max {
            get { return scope2_max; }
            set { scope2_max = value; }
        }

 
        //MW0LGE - these properties auto AGC on the waterfall, so that
        //spectrum/grid based max/mins can be used without getting changed by agc
        private static bool m_bRX1_spectrum_thresholds = false;
        public static bool SpectrumBasedThresholdsRX1 {
            get { return m_bRX1_spectrum_thresholds; }
            set { m_bRX1_spectrum_thresholds = value; }
        }
        private static bool m_bRX2_spectrum_thresholds = false;
        public static bool SpectrumBasedThresholdsRX2 {
            get { return m_bRX2_spectrum_thresholds; }
            set { m_bRX2_spectrum_thresholds = value; }
        }
        //--

        private static bool rx1_waterfall_agc = false;
        public static bool RX1WaterfallAGC {
            get { return rx1_waterfall_agc; }
            set { rx1_waterfall_agc = value; }
        }

        private static bool rx2_waterfall_agc = false;
        public static bool RX2WaterfallAGC {
            get { return rx2_waterfall_agc; }
            set { rx2_waterfall_agc = value; }
        }

        private static int waterfall_update_period = 2; // in frame intevals, such that it only gets updated every 2 frame (default)
        public static int WaterfallUpdatePeriod {
            get { return waterfall_update_period; }
            set { waterfall_update_period = value; }
        }

        private static int rx2_waterfall_update_period = 2; // in frame intevals, such that it only gets updated every 2 frame (default)
        public static int RX2WaterfallUpdatePeriod {
            get { return rx2_waterfall_update_period; }
            set { rx2_waterfall_update_period = value; }
        }

        private static float RX1waterfallPreviousMinValue = 0.0f;
        private static float RX2waterfallPreviousMinValue = 0.0f;
        private static void ResetWaterfallBmp(/*int scale*/)
        {
            int H = displayTargetHeight;
            if (current_display_mode == DisplayMode.PANAFALL) H /= 2;
            if (rx2_enabled) H /= 2;

            //override for splitter pos, when only one rx and it is panafall
            if (!rx2_enabled && current_display_mode == DisplayMode.PANAFALL) H = displayTargetHeight - PanafallSplitBarPos;

            lock (_objDX2Lock)
            {
                if (_bDX2Setup)
                {
                    SharpDX.Direct2D1.Bitmap tmp = null;

                    if (_waterfall_bmp_dx2d != null && !_waterfall_bmp_dx2d.IsDisposed)
                    {
                        if (displayTargetWidth == _waterfall_bmp_dx2d.Size.Width)
                        {
                            // make copy only if widths equal
                            int h = Math.Min(H - 20, (int)_waterfall_bmp_dx2d.Size.Height);

                            tmp = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2((int)_waterfall_bmp_dx2d.Size.Width, h/*(int)_waterfall_bmp_dx2d.Size.Height*/),
                                    new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, _ALPHA_MODE)));

                            tmp.CopyFromBitmap(_waterfall_bmp_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)tmp.Size.Width, (int)tmp.Size.Height));
                            //
                        }
                    }

                    if (_waterfall_bmp_dx2d != null)
                    {
                        Utilities.Dispose(ref _waterfall_bmp_dx2d);
                        _waterfall_bmp_dx2d = null;
                    }
                    _waterfall_bmp_dx2d = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2(displayTargetWidth, /*(displayTargetHeight / scale)*/ H - 20), new BitmapProperties(new SDXPixelFormat(_swapChain.Description.ModeDescription.Format, _ALPHA_MODE)));

                    if (tmp != null)
                    {
                        byte[] zeroed = new byte[displayTargetWidth * (H - 20) * 4];
                        unsafe
                        {
                            fixed (void* wptr = &zeroed[0])
                                Win32.memset(wptr, 0, zeroed.Length);
                        }
                        _waterfall_bmp_dx2d.CopyFromMemory(zeroed, 4);

                        // copy old waterfall into new bitmap
                        _waterfall_bmp_dx2d.CopyFromBitmap(tmp, new SharpDX.Point(0, 0)); // anything outside will be 'ignored'
                        Utilities.Dispose(ref tmp);
                        tmp = null;
                    }
                }
            }
        }
        private static void ResetWaterfallBmp2(/*int scale*/)
        {
            int H = displayTargetHeight;
            if (current_display_mode_bottom == DisplayMode.PANAFALL) H /= 2;
            H /= 2; // it will always be

            lock (_objDX2Lock)
            {
                if (_bDX2Setup)
                {
                    SharpDX.Direct2D1.Bitmap tmp = null;

                    if (_waterfall_bmp2_dx2d != null && !_waterfall_bmp2_dx2d.IsDisposed)
                    {
                        if (displayTargetWidth == _waterfall_bmp2_dx2d.Size.Width)
                        {
                            // make copy only if widths equal
                            int h = Math.Min(H - 20, (int)_waterfall_bmp2_dx2d.Size.Height);

                            tmp = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2((int)_waterfall_bmp2_dx2d.Size.Width, h/*(int)_waterfall_bmp2_dx2d.Size.Height*/),
                                    new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, _ALPHA_MODE)));

                            tmp.CopyFromBitmap(_waterfall_bmp2_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)tmp.Size.Width, (int)tmp.Size.Height));
                            //
                        }
                    }

                    if (_waterfall_bmp2_dx2d != null)
                    {
                        Utilities.Dispose(ref _waterfall_bmp2_dx2d);
                        _waterfall_bmp2_dx2d = null;
                    }
                    _waterfall_bmp2_dx2d = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2(displayTargetWidth, /*(displayTargetHeight / scale)*/ H - 20), new BitmapProperties(new SDXPixelFormat(_swapChain.Description.ModeDescription.Format, _ALPHA_MODE)));

                    if (tmp != null)
                    {
                        byte[] zeroed = new byte[displayTargetWidth * (H - 20) * 4];
                        unsafe
                        {
                            fixed (void* wptr = &zeroed[0])
                                Win32.memset(wptr, 0, zeroed.Length);
                        }
                        _waterfall_bmp2_dx2d.CopyFromMemory(zeroed, 4);

                        // copy old waterfall into new bitmap
                        _waterfall_bmp2_dx2d.CopyFromBitmap(tmp, new SharpDX.Point(0, 0)); // anything outside will be 'ignored'
                        Utilities.Dispose(ref tmp);
                        tmp = null;
                    }
                }
            }
        }

        #endregion

        #endregion
        #endregion

        #region DirectX
        // directx mw0lge
        private static bool _bDX2Setup = false;
        private static Surface _surface;
        private static SwapChain _swapChain;
        private static SwapChain1 _swapChain1;
        private static RenderTarget _d2dRenderTarget;
        private static SharpDX.Direct2D1.Factory _d2dFactory;
        private static Device _device;
        private static SharpDX.DXGI.Factory1 _factory1;
        private static readonly Object _objDX2Lock = new Object();
        private static Vector2 m_pixelShift = new Vector2(0.5f, 0.5f);
        private static int _nOldHeightRX1 = -1;
        private static int _nOldHeightRX2 = -1;
        private static bool _bNoiseFloorAlreadyCalculatedRX1 = false;
        private static bool _bNoiseFloorAlreadyCalculatedRX2 = false;
        private static PresentFlags _NoVSYNCpresentFlag = PresentFlags.None;
        private static int _nBufferCount = 1;

        private static bool m_bHighlightNumberScaleRX1 = false;
        private static bool m_bHighlightNumberScaleRX2 = false;
        public static bool HighlightNumberScaleRX1 {
            get { return m_bHighlightNumberScaleRX1; }
            set {
                m_bHighlightNumberScaleRX1 = value;
            }
        }
        public static bool HighlightNumberScaleRX2 {
            get { return m_bHighlightNumberScaleRX2; }
            set {
                m_bHighlightNumberScaleRX2 = value;
            }
        }

        public static void ShutdownDX2D()
        {
            lock (_objDX2Lock)
            {
                if (!_bDX2Setup) return;                

                try
                {
                    if (_device != null && _device.ImmediateContext != null)
                    {
                        _device.ImmediateContext.ClearState();
                        _device.ImmediateContext.Flush();
                    }

                    releaseFonts();
                    releaseDX2Resources();

                    if(_bitmapBackground != null)
                        Utilities.Dispose(ref _bitmapBackground);

                    Utilities.Dispose(ref _waterfall_bmp_dx2d);
                    Utilities.Dispose(ref _waterfall_bmp2_dx2d);

                    Utilities.Dispose(ref _d2dRenderTarget);
                    Utilities.Dispose(ref _swapChain1);
                    Utilities.Dispose(ref _swapChain);
                    Utilities.Dispose(ref _surface);
                    Utilities.Dispose(ref _d2dFactory);
                    Utilities.Dispose(ref _factory1);

                    _bitmapBackground = null;
                    _waterfall_bmp_dx2d = null;
                    _waterfall_bmp2_dx2d = null;

                    _d2dRenderTarget = null;
                    _swapChain1 = null;
                    _swapChain = null;
                    _surface = null;
                    _d2dFactory = null;
                    _factory1 = null;

                    if (_device != null && _device.ImmediateContext != null)
                    {
                        SharpDX.Direct3D11.DeviceContext dc = _device.ImmediateContext;
                        Utilities.Dispose(ref dc);
                        dc = null;
                    }

                    SharpDX.Direct3D11.DeviceDebug ddb = null;
                    if (_device != null && _device.DebugName != "")
                    {
                        ddb = new SharpDX.Direct3D11.DeviceDebug(_device);
                        ddb.ReportLiveDeviceObjects(ReportingLevel.Detail);
                    }

                    if (ddb != null)
                    {
                        Utilities.Dispose(ref ddb);
                        ddb = null;
                    }
                    Utilities.Dispose(ref _device);
                    _device = null;

                    _bDX2Setup = false;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Problem Shutting Down DirectX !" + System.Environment.NewLine + System.Environment.NewLine + "[" + e.ToString() + "]", "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        
        private static string[] DX2Adaptors()
        {
            SharpDX.DXGI.Factory factory1 = new SharpDX.DXGI.Factory1();

            int nAdaptorCount = factory1.GetAdapterCount();
            string[] adaptors = new string[nAdaptorCount];

            for(int n = 0; n<nAdaptorCount; n++)
            {
                using(Adapter adapter = factory1.GetAdapter(n))
                {
                    adaptors[n] = adapter.Description.Description;
                }                
            }
            Utilities.Dispose(ref factory1);
            factory1 = null;
            return adaptors;
        }

        public static bool IsDX2DSetup
        {
            get { return _bDX2Setup; }
        }

        private static void initDX2D(DriverType driverType = DriverType.Hardware)
        {
            lock (_objDX2Lock)
            {
                if (_bDX2Setup || displayTarget == null) return;

                try
                {
                    DeviceCreationFlags debug = DeviceCreationFlags.None;//.Debug;  //MW0LGE_21k9 enabled debug to obtain list of objects that are still referenced

                    // to get this to work, need to target the os
                    // https://www.prugg.at/2019/09/09/properly-detect-windows-version-in-c-net-even-windows-10/
                    // you need to enable operating system support in the app1.manifest file, otherwise majVers will not report 10+
                    // note: windows 10, 11, server 2016, server 2019, server 2022 all use the windows 10 os id in the manifest file at this current time
                    int majVers = Environment.OSVersion.Version.Major;
                    int minVers = Environment.OSVersion.Version.Minor;

                    SharpDX.Direct3D.FeatureLevel[] featureLevels;

                    if (majVers >= 10) // win10 + 11
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_12_1,
                            SharpDX.Direct3D.FeatureLevel.Level_12_0,
                            SharpDX.Direct3D.FeatureLevel.Level_11_1, // windows 8 and up
                            SharpDX.Direct3D.FeatureLevel.Level_11_0, // windows 7 and up (level 11 was only partial on 7, not 11_1)
                            SharpDX.Direct3D.FeatureLevel.Level_10_1,
                            SharpDX.Direct3D.FeatureLevel.Level_10_0,
                            SharpDX.Direct3D.FeatureLevel.Level_9_3,
                            SharpDX.Direct3D.FeatureLevel.Level_9_2,
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.DoNotWait;
                    }
                    else if (majVers == 6 && minVers >= 2) // windows 8, windows 8.1
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_11_1, // windows 8 and up
                            SharpDX.Direct3D.FeatureLevel.Level_11_0, // windows 7 and up (level 11 was only partial on 7, not 11_1)
                            SharpDX.Direct3D.FeatureLevel.Level_10_1,
                            SharpDX.Direct3D.FeatureLevel.Level_10_0,
                            SharpDX.Direct3D.FeatureLevel.Level_9_3,
                            SharpDX.Direct3D.FeatureLevel.Level_9_2,
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.DoNotWait;
                    }
                    else if (majVers == 6 && minVers < 2) // windows 7, 2008 R2, 2008, vista
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_11_0, // windows 7 and up (level 11 was only partial on 7, not 11_1)
                            SharpDX.Direct3D.FeatureLevel.Level_10_1,
                            SharpDX.Direct3D.FeatureLevel.Level_10_0,
                            SharpDX.Direct3D.FeatureLevel.Level_9_3,
                            SharpDX.Direct3D.FeatureLevel.Level_9_2,
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.None;
                    }
                    else
                    {
                        featureLevels = new SharpDX.Direct3D.FeatureLevel[] {
                            SharpDX.Direct3D.FeatureLevel.Level_9_1
                        };
                        _NoVSYNCpresentFlag = PresentFlags.None;
                    }

                    _factory1 = new SharpDX.DXGI.Factory1();
                    
                    _device = new Device(driverType, debug | DeviceCreationFlags.PreventAlteringLayerSettingsFromRegistry | DeviceCreationFlags.BgraSupport | DeviceCreationFlags.SingleThreaded, featureLevels);

                    SharpDX.DXGI.Device1 device1 = _device.QueryInterfaceOrNull<SharpDX.DXGI.Device1>();
                    if (device1 != null)
                    {
                        device1.MaximumFrameLatency = 1;
                        Utilities.Dispose(ref device1);
                        device1 = null;
                    }

                    //this code should ideally be used to prevent use of flip if vsync is 0
                    //but is not used at this time
                    //SharpDX.DXGI.Factory5 f5 = factory.QueryInterfaceOrNull<SharpDX.DXGI.Factory5>();
                    //bool bAllowTearing = false;
                    //if(f5 != null)
                    //{
                    //    int size = Marshal.SizeOf(typeof(bool));
                    //    IntPtr pBool = Marshal.AllocHGlobal(size);

                    //    f5.CheckFeatureSupport(SharpDX.DXGI.Feature.PresentAllowTearing, pBool, size);

                    //    bAllowTearing = Marshal.ReadInt32(pBool) == 1;

                    //    Marshal.FreeHGlobal(pBool);
                    //}
                    //

                    // check if the device has a factory4 interface
                    // if not, then we need to use old bitplit swapeffect
                    SwapEffect swapEffect;

                    SharpDX.DXGI.Factory4 factory4 = _factory1.QueryInterfaceOrNull<SharpDX.DXGI.Factory4>();
                    bool bFlipPresent = false;
                    if (factory4 != null)
                    {
                        if(!_bUseLegacyBuffers) bFlipPresent = true;
                        Utilities.Dispose(ref factory4);
                        factory4 = null;
                    }

                    //https://walbourn.github.io/care-and-feeding-of-modern-swapchains/
                    swapEffect = bFlipPresent ? SwapEffect.FlipDiscard : SwapEffect.Discard; //NOTE: FlipSequential should work, but is mostly used for storeapps
                    _nBufferCount = bFlipPresent ? 2 : 1;

                    //int multiSample = 8; // eg 2 = MSAA_2, 2 times multisampling
                    //int maxQuality = device.CheckMultisampleQualityLevels(Format.B8G8R8A8_UNorm, multiSample) - 1; 
                    //maxQuality = Math.Max(0, maxQuality);

                    ModeDescription md = new ModeDescription(displayTarget.Width, displayTarget.Height,
                                                               new Rational(console.DisplayFPS, 1), Format.B8G8R8A8_UNorm);
                    md.ScanlineOrdering = DisplayModeScanlineOrder.Progressive;
                    md.Scaling = DisplayModeScaling.Centered;

                    SwapChainDescription desc = new SwapChainDescription()
                    {
                        BufferCount = _nBufferCount,
                        ModeDescription = md,
                        IsWindowed = true,
                        OutputHandle = displayTarget.Handle,
                        //SampleDescription = new SampleDescription(multiSample, maxQuality),
                        SampleDescription = new SampleDescription(1, 0), // no multi sampling (1 sample), no antialiasing
                        SwapEffect = swapEffect,
                        Usage = Usage.RenderTargetOutput,// | Usage.BackBuffer,  // dont need usage.backbuffer as it is implied
                        Flags = SwapChainFlags.None,
                    };

                    _factory1.MakeWindowAssociation(displayTarget.Handle, WindowAssociationFlags.IgnoreAll);

                    _swapChain = new SwapChain(_factory1, _device, desc);
                    _swapChain1 = _swapChain.QueryInterface<SwapChain1>();

                    _d2dFactory = new SharpDX.Direct2D1.Factory(FactoryType.SingleThreaded, DebugLevel.None);

                    _surface = _swapChain1.GetBackBuffer<Surface>(0);

                    RenderTargetProperties rtp = new RenderTargetProperties(new SDXPixelFormat(_swapChain.Description.ModeDescription.Format, _ALPHA_MODE));
                    _d2dRenderTarget = new RenderTarget(_d2dFactory, _surface, rtp);                    

                    if (debug == DeviceCreationFlags.Debug)
                    {
                        _device.DebugName = "DeviceDB";
                        _swapChain.DebugName = "SwapChainDB";
                        _swapChain1.DebugName = "SwapChain1DB";
                        _surface.DebugName = "SurfaceDB";
                    }
                    else
                    {
                        _device.DebugName = ""; // used in shutdown
                    }

                    _bDX2Setup = true;

                    setupAliasing();

                    ResetWaterfallBmp();
                    ResetWaterfallBmp2();

                    buildDX2Resources();
                    buildFontsDX2D();

                    SetDX2BackgoundImage(console.PicDisplayBackgroundImage);
                }
                catch (Exception e)
                {
                    // issue setting up dx
                    ShutdownDX2D();
                    MessageBox.Show("Problem initialising DirectX !" + System.Environment.NewLine + System.Environment.NewLine + "[" + e.ToString() + "]", "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        public static void ResetDX2DModeDescription()
        {
            // used to reset the FPS on the swapChain
            try
            {
                lock (_objDX2Lock)
                {
                    if (!_bDX2Setup) return;
                    ModeDescription modeDesc = new ModeDescription(displayTargetWidth, displayTargetHeight,
                                                       new Rational(console.DisplayFPS, 1), Format.B8G8R8A8_UNorm);                    
                    _swapChain1.ResizeTarget(ref modeDesc);

                    // MW0LGE_21k9 must resize the back buffers, belts and braces because width/height not likely to change
                    ResizeDX2D();
                }
            }
            catch (Exception e)
            {

            }
        }
        public static void ResizeDX2D()
        {
            try
            {
                lock (_objDX2Lock)
                {
                    if (!_bDX2Setup) return;

                    Utilities.Dispose(ref _d2dRenderTarget);
                    Utilities.Dispose(ref _surface);

                    _d2dRenderTarget = null;
                    _surface = null;

                    _device.ImmediateContext.ClearState();
                    _device.ImmediateContext.Flush();

                    _swapChain1.ResizeBuffers(_nBufferCount, displayTargetWidth, displayTargetHeight, _swapChain.Description.ModeDescription.Format, SwapChainFlags.None);

                    _surface = _swapChain1.GetBackBuffer<Surface>(0);

                    RenderTargetProperties rtp = new RenderTargetProperties(new SDXPixelFormat(_swapChain.Description.ModeDescription.Format, _ALPHA_MODE));
                    _d2dRenderTarget = new RenderTarget(_d2dFactory, _surface, rtp);

                    setupAliasing();
                }
            }
            catch (Exception e)
            {
                ShutdownDX2D();
                MessageBox.Show("DirectX ResizeDX2D() failure\n" + e.Message, "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static int PanafallSplitBarPos {
            get { return (int)(displayTargetHeight * m_fPanafallSplitPerc); }
        }
        private static float m_fPanafallSplitPerc = 0.5f;
        public static float PanafallSplitBarPerc {
            get { return m_fPanafallSplitPerc; }
            set {
                bool resetBmp = value != m_fPanafallSplitPerc;
                m_fPanafallSplitPerc = value;
                if (resetBmp)
                    ResetWaterfallBmp();
            }
        }
        private static bool m_bAntiAlias = false;
        public static bool AntiAlias {
            get { return m_bAntiAlias; }
            set
            { 
                m_bAntiAlias = value;
                setupAliasing();
            }
        }
        public static bool SpecialPanafall {
            get { return m_bSpecialPanafall; }
            set {
                m_bSpecialPanafall = value;
                if (m_bSpecialPanafall)
                {
                    m_fPanafallSplitPerc = 0.8f;
                }
                else
                {
                    m_fPanafallSplitPerc = 0.5f;
                }
                ResetWaterfallBmp();
            }
        }

        private static int m_nRX1DisplayHeight = 0;
        public static int RX1DisplayHeight {
            get { return m_nRX1DisplayHeight; }
            set { }
        }
        private static int m_nRX2DisplayHeight = 0;
        public static int RX2DisplayHeight {
            get { return m_nRX2DisplayHeight; }
            set { }
        }

        private static string m_sDebugText = "";
        public static string DebugText {
            get { return m_sDebugText; }
            set { m_sDebugText = value; }
        }

        private static void setupAliasing()
        {
            lock (_objDX2Lock)
            {
                if (!_bDX2Setup) return;

                if (m_bAntiAlias)
                    _d2dRenderTarget.AntialiasMode = AntialiasMode.PerPrimitive; // this will antialias even if multisampling is off
                else
                    _d2dRenderTarget.AntialiasMode = AntialiasMode.Aliased; // this will result in non antialiased lines only if multisampling = 1

                _d2dRenderTarget.TextAntialiasMode = TextAntialiasMode.Default;
            }
        }
        public static void RenderDX2D()
        {
            try
            {
                lock (_objDX2Lock)
                {
                    if (!_bDX2Setup) return; // moved inside the lock so that a change in state by shutdown becomes thread safe

                    m_dElapsedFrameStart = m_objFrameStartTimer.ElapsedMsec;

                    _bNoiseFloorAlreadyCalculatedRX1 = false; // keeps track of noise floor processing, only want to do it once, even if pana + water shown
                    _bNoiseFloorAlreadyCalculatedRX2 = false;

                    _d2dRenderTarget.BeginDraw();

                    // middle pixel align shift, NOTE: waterfall will switch internally to identity, and then restore
                    Matrix3x2 t = _d2dRenderTarget.Transform;
                    t.TranslationVector = m_pixelShift;
                    _d2dRenderTarget.Transform = t;
                    
                    if (_bitmapBackground == null)
                    {
                        _d2dRenderTarget.Clear(m_cDX2_display_background_colour);
                    }
                    else
                    {
                        // draw background image
                        RectangleF rectDest = new RectangleF(0, 0, displayTargetWidth, displayTargetHeight);
                        _d2dRenderTarget.DrawBitmap(_bitmapBackground, rectDest, 1f, BitmapInterpolationMode.Linear);
                        _d2dRenderTarget.FillRectangle(rectDest, m_bDX2_display_background_brush);
                    }

                    // LINEAR BRUSH BUILDING
                    if (_bRebuildRX1LinearGradBrush || _bRebuildRX2LinearGradBrush)
                    {
                        int tmpHeightRX1 = displayTargetHeight;
                        int tmpHeightRX2 = displayTargetHeight;
                        if (!split_display)
                        {
                            switch (current_display_mode)
                            {
                                case DisplayMode.PANAFALL:
                                    tmpHeightRX1 = (int)(tmpHeightRX1 * m_fPanafallSplitPerc);
                                    break;
                                case DisplayMode.PANASCOPE:
                                case DisplayMode.SPECTRASCOPE:
                                    tmpHeightRX1 /= 2;
                                    break;
                            }
                        }
                        else
                        {
                            tmpHeightRX1 /= 2;
                            tmpHeightRX2 /= 2;

                            switch (current_display_mode)
                            {
                                case DisplayMode.PANAFALL:
                                case DisplayMode.PANASCOPE:
                                case DisplayMode.SPECTRASCOPE:
                                    tmpHeightRX1 /= 2;
                                    break;
                            }

                            switch (current_display_mode_bottom)
                            {
                                case DisplayMode.PANAFALL:
                                case DisplayMode.PANASCOPE:
                                case DisplayMode.SPECTRASCOPE:
                                    tmpHeightRX2 /= 2;
                                    break;
                            }

                        }

                        if (_bRebuildRX1LinearGradBrush)
                        {
                            //int s9PixelPos = GetDBMPixelPos(0, -73, 1, m_nRX1DisplayHeight, false);
                            buildLinearGradientBrush(0, tmpHeightRX1, 1);

                            _bRebuildRX1LinearGradBrush = false;
                        }
                        if (_bRebuildRX2LinearGradBrush)
                        {
                            int nVertShift = 0;

                            if (split_display)
                            {
                                switch (current_display_mode_bottom)
                                {
                                    case DisplayMode.PANADAPTER:
                                    case DisplayMode.WATERFALL:
                                        nVertShift = tmpHeightRX2;
                                        break;
                                    case DisplayMode.PANAFALL:
                                        nVertShift = tmpHeightRX2 * 2;
                                        break;
                                }
                            }

                            //int s9PixelPos = GetDBMPixelPos(nVertShift, -73, 2, m_nRX2DisplayHeight, false);
                            buildLinearGradientBrush(nVertShift, tmpHeightRX2 + nVertShift, 2);

                            _bRebuildRX2LinearGradBrush = false;
                        }
                    }
                    //

                    if (!split_display)
                    {
                        m_nRX1DisplayHeight = displayTargetHeight;

                        switch (current_display_mode)
                        {
                            case DisplayMode.SPECTRUM:
                                DrawSpectrumDX2D(1, displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PANADAPTER:
                                DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                break;
                            case DisplayMode.SCOPE:
                                DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.SCOPE2:
                                DrawScope2DX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PHASE:
                                DrawPhaseDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PHASE2:
                                DrawPhase2DX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.WATERFALL:
                                DrawWaterfallDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                break;
                            case DisplayMode.HISTOGRAM:
                                DrawHistogramDX2D(displayTargetWidth, m_nRX1DisplayHeight);
                                break;
                            case DisplayMode.PANAFALL:
                                lock (m_objSplitDisplayLock)
                                {
                                    m_nRX1DisplayHeight = (int)(displayTargetHeight * m_fPanafallSplitPerc);
                                    split_display = PanafallSplitBarPos <= (displayTargetHeight / 2); // add more granularity, TODO change based on avaialble height
                                    DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                    DrawWaterfallDX2D(PanafallSplitBarPos, displayTargetWidth, displayTargetHeight - m_nRX1DisplayHeight, 1, true);
                                    split_display = false;
                                }
                                break;
                            case DisplayMode.PANASCOPE:
                                lock (m_objSplitDisplayLock)
                                {
                                    m_nRX1DisplayHeight = displayTargetHeight / 2;
                                    split_display = true;
                                    DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                    DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, true);
                                    split_display = false;
                                }
                                break;
                            case DisplayMode.SPECTRASCOPE:
                                lock (m_objSplitDisplayLock)
                                {
                                    m_nRX1DisplayHeight = displayTargetHeight / 2;
                                    split_display = true;
                                    DrawSpectrumDX2D(1, displayTargetWidth, m_nRX1DisplayHeight, false);
                                    DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, true);
                                    split_display = false;
                                }
                                break;
                        }
                    }
                    else
                    {
                        m_nRX1DisplayHeight = displayTargetHeight / 2;

                        switch (current_display_mode)
                        {
                            case DisplayMode.SPECTRUM:
                                DrawSpectrumDX2D(1, displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.SCOPE:
                                DrawScopeDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.SCOPE2:
                                DrawScope2DX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PHASE:
                                DrawPhaseDX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PHASE2:
                                DrawPhase2DX2D(displayTargetWidth, m_nRX1DisplayHeight, false);
                                break;
                            case DisplayMode.PANADAPTER:
                                DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                break;
                            case DisplayMode.WATERFALL:
                                DrawWaterfallDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                break;
                            case DisplayMode.HISTOGRAM:
                                DrawHistogramDX2D(displayTargetWidth, m_nRX1DisplayHeight);
                                break;
                            case DisplayMode.PANAFALL:
                                m_nRX1DisplayHeight = displayTargetHeight / 4;
                                DrawPanadapterDX2D(0, displayTargetWidth, m_nRX1DisplayHeight, 1, false);
                                DrawWaterfallDX2D(m_nRX1DisplayHeight, displayTargetWidth, m_nRX1DisplayHeight, 1, true);
                                break;
                        }

                        m_nRX2DisplayHeight = displayTargetHeight / 2;

                        switch (current_display_mode_bottom)
                        {

                            case DisplayMode.PANADAPTER:
                                DrawPanadapterDX2D(m_nRX2DisplayHeight, displayTargetWidth, m_nRX2DisplayHeight, 2, true);
                                break;
                            case DisplayMode.WATERFALL:
                                DrawWaterfallDX2D(m_nRX2DisplayHeight, displayTargetWidth, m_nRX2DisplayHeight, 2, true);
                                break;
                            case DisplayMode.PANAFALL:
                                m_nRX2DisplayHeight = displayTargetHeight / 4;
                                DrawPanadapterDX2D(m_nRX2DisplayHeight * 2, displayTargetWidth, m_nRX2DisplayHeight, 2, false);
                                DrawWaterfallDX2D(m_nRX2DisplayHeight * 3, displayTargetWidth, m_nRX2DisplayHeight, 2, true);
                                break;
                        }
                    }

                    // for linear grad brush rebuilding. Do these all the time, even if m_bUseLinearGradient=false, so we can rebuild if need be
                    if (m_nRX1DisplayHeight != _nOldHeightRX1)
                    {
                        _nOldHeightRX1 = m_nRX1DisplayHeight;
                        _bRebuildRX1LinearGradBrush = true;
                    }
                    if (m_nRX2DisplayHeight != _nOldHeightRX2)
                    {
                        _nOldHeightRX2 = m_nRX2DisplayHeight;
                        _bRebuildRX2LinearGradBrush = true;
                    }

                    // HIGH swr display warning
                    if (high_swr)
                    {
                        drawStringDX2D("High SWR", fontDX2d_font14, m_bDX2_Red, 245, 20);
                        _d2dRenderTarget.DrawRectangle(new RectangleF(3, 3, displayTargetWidth - 6, displayTargetHeight - 6), m_bDX2_Red, 6f);
                    }

                    if (m_bShowFrameRateIssue && m_bFrameRateIssue) _d2dRenderTarget.FillRectangle(new RectangleF(0, 0, 8, 8), m_bDX2_Red);
                    if (m_bShowGetPixelsIssue && m_bGetPixelsIssue) _d2dRenderTarget.FillRectangle(new RectangleF(0, 8, 8, 8), m_bDX2_Yellow);

                    calcFps();
                    if (m_bShowFPS) _d2dRenderTarget.DrawText(m_nFps.ToString(), fontDX2d_callout, new RectangleF(10, 0, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_m_bTextCallOutActive, DrawTextOptions.None);

                    //MW0LGE_21k8
                    processBlobsActivePeakDisplayDelay();
                    //

                    // some debug text
                    if (m_sDebugText != "")
                    {
                        string[] lines = m_sDebugText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        int xStartX = 32;
                        for (int i = 0; i < lines.Length; i++)
                        {
                            _d2dRenderTarget.DrawText(lines[i].ToString(), fontDX2d_callout, new RectangleF(64, xStartX, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_m_bTextCallOutActive, DrawTextOptions.None);
                            xStartX += 12;
                        }
                    }

                    DrawCursorInfo(displayTargetWidth);

                    ukraineFlag();

                    // undo the translate
                    _d2dRenderTarget.Transform = Matrix3x2.Identity;
                    
                    _d2dRenderTarget.EndDraw();

                    // render
                    // note: the only way to have Present non block when using vsync number of blanks 0 , is to use DoNotWait
                    // however the gpu will error if it is busy doing something and the data can not be queued
                    // It will error and just ignore everything, we try present and ignore the 0x887A000A error
                    PresentFlags pf = m_nVBlanks == 0 ? _NoVSYNCpresentFlag : PresentFlags.None;
                    Result r = _swapChain1.TryPresent(m_nVBlanks, pf);
                    
                    if (r != Result.Ok && r != 0x887A000A)
                    {
                        string sMsg = "";
                        if (r == 0x887A0001) sMsg = "Present Device Invalid Call" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";    //DXGI_ERROR_INVALID_CALL
                        if (r == 0x887A0007) sMsg = "Present Device Reset" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";           //DXGI_ERROR_DEVICE_RESET
                        if (r == 0x887A0005) sMsg = "Present Device Removed" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";         //DXGI_ERROR_DEVICE_REMOVED
                        if (r == 0x88760870) sMsg = "Present Device DD3DDI Removed" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";  //D3DDDIERR_DEVICEREMOVED
                        //if (r == 0x087A0001) sMsg = "Present Device Occluded" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]";      //DXGI_STATUS_OCCLUDED
                        //(ignored in the preceding if statement) if (r == 0x887A000A) sMsg = "Present Device Still Drawping" + Environment.NewLine + "" + Environment.NewLine + "[ " + r.ToString() + " ]"; //DXGI_ERROR_WAS_STILL_DRAWING

                        if (sMsg != "") throw (new Exception(sMsg));
                    }
                }
            }
            catch (Exception e)
            {
                ShutdownDX2D();
                MessageBox.Show("Problem in DirectX Renderer !" + System.Environment.NewLine + System.Environment.NewLine + "[ " + e.ToString() + " ]", "DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private static int m_nVBlanks = 0;
        public static int VerticalBlanks {
            get { return m_nVBlanks; }
            set {
                int v = value;
                if (v < 0) v = 0;
                if (v > 4) v = 4;
                m_nVBlanks = v; 
            }
        }

        private static int m_nFps = 0;
        private static int m_nFrameCount = 0;
        private static HiPerfTimer m_objFrameStartTimer = new HiPerfTimer();
        private static double m_fLastTime = m_objFrameStartTimer.ElapsedMsec;
        private static double m_dElapsedFrameStart = m_objFrameStartTimer.ElapsedMsec;
        private static void calcFps()
        {
            //double t = m_dElapsedFrameStart;// m_objFrameStartTimer.ElapsedMsec;
            m_nFrameCount++;
            if (m_dElapsedFrameStart >= m_fLastTime + 1000)
            {
                double late = m_dElapsedFrameStart - (m_fLastTime + 1000);
                if (late > 2000 || late < 0) late = 0; // ignore if too late
                m_nFps = m_nFrameCount;
                m_nFrameCount = 0;
                m_fLastTime = m_dElapsedFrameStart - late;
            }
        }

        public static int CurrentFPS
        {
            get { return m_nFps; }
            // MW0LGE_21k8 used to pre-init fps, before rundisplay has had time to recalculate, called from console mostly when adjusting fps in setup window
            set { m_nFps = value; }
        }

        private static bool m_bPeakBlobMaximums = true;
        public static bool ShowPeakBlobs {
            get { return m_bPeakBlobMaximums; }
            set { m_bPeakBlobMaximums = value; }
        }
        private static bool m_bInsideFilterOnly = false;
        public static bool ShowPeakBlobsInsideFilterOnly {
            get { return m_bInsideFilterOnly; }
            set { m_bInsideFilterOnly = value; }
        }
        private static int m_nNumberOfMaximums = 3;
        public static int NumberOfPeakBlobs {
            get { return m_nNumberOfMaximums; }
            set {
                int t = value;
                if (t < 1) t = 1;
                // just use rx1 as both same length
                if (t > m_nRX1Maximums.Length) t = m_nRX1Maximums.Length;
                m_nNumberOfMaximums = t; 
            }
        }

        private struct Maximums
        {
            public float max_dBm;
            public int X;
            public int MaxY_pixel;
            public bool Enabled;
            public double Time;
        }
        static private int isOccupied(int rx, int nX)
        {
            Maximums[] maximums;
            if (rx == 1)
                maximums = m_nRX1Maximums;
            else
                maximums = m_nRX2Maximums;

            int nRet = -1; // -1 returned if nothing in this area

            for (int n = 0; n < m_nNumberOfMaximums; n++)
            {
                int p1 = Math.Abs(nX - maximums[n].X);

                if (maximums[n].Enabled && p1 < 10) // 10 being the radius of the ellipse/circle
                { 
                    nRet = n;
                    break;
                }
            }
            return nRet;
        }

        //static private HiPerfTimer m_objPeakHoldTimer = new HiPerfTimer();
        static private Maximums[] m_nRX1Maximums = new Maximums[20]; // max of 20 blobs
        static private Maximums[] m_nRX2Maximums = new Maximums[20]; // max of 20 blobs
        private static Maximums[] m_rx1_spectrumPeaks;
        private static Maximums[] m_rx2_spectrumPeaks;
        static private void processMaximums(int rx, float dbm, int nX, int nMaxY_pixel)
        {
            Maximums[] maximums;
            if (rx == 1)
                maximums = m_nRX1Maximums;
            else
                maximums = m_nRX2Maximums;

            int nOccupiedIndex = isOccupied(rx, nX);

            if (nOccupiedIndex >= 0)
            {
                if (dbm >= maximums[nOccupiedIndex].max_dBm)
                {
                    maximums[nOccupiedIndex].Enabled = true;
                    maximums[nOccupiedIndex].max_dBm = dbm;
                    maximums[nOccupiedIndex].X = nX;
                    maximums[nOccupiedIndex].MaxY_pixel = nMaxY_pixel;
                    maximums[nOccupiedIndex].Time = m_dElapsedFrameStart;
                    Array.Sort<Maximums>(maximums, (x, y) => y.max_dBm.CompareTo(x.max_dBm));
                }
                return;
            }

            for (int n = 0; n < m_nNumberOfMaximums; n++)
            {
                if (dbm > maximums[n].max_dBm)
                {
                    //move them down
                    for (int nn = m_nNumberOfMaximums - 1; nn > n; nn--)
                    {
                        maximums[nn].Enabled = maximums[nn - 1].Enabled;
                        maximums[nn].max_dBm = maximums[nn - 1].max_dBm;
                        maximums[nn].X = maximums[nn - 1].X;
                        maximums[nn].MaxY_pixel = maximums[nn - 1].MaxY_pixel;
                        maximums[nn].Time = maximums[nn - 1].Time;
                    }

                    //add new
                    maximums[n].Enabled = true;
                    maximums[n].max_dBm = dbm;
                    maximums[n].X = nX;
                    maximums[n].MaxY_pixel = nMaxY_pixel;
                    maximums[n].Time = m_dElapsedFrameStart;

                    break;
                }
            }
        }


        static public void ResetSpectrumPeaks(int rx)
        {
            delayBlobsActivePeakDisplay(rx, false);

            Maximums[] maximums;
            if (rx == 1)
                maximums = m_rx1_spectrumPeaks;
            else
                maximums = m_rx2_spectrumPeaks;

            //for (int n = 0; n < maximums.Length; n++)
            //{
            //    maximums[n].max_dBm = float.MinValue;
            //}
            Parallel.For(0, maximums.Length, (i) =>
            {
                maximums[i].max_dBm = float.MinValue;
            });
        }
        static public void ResetBlobMaximums(int rx, bool bClear = false)
        {
            if(bClear) delayBlobsActivePeakDisplay(rx, true);

            Maximums[] maximums;
            if (rx == 1)
                maximums = m_nRX1Maximums;
            else
                maximums = m_nRX2Maximums;

            int tot = bClear ? maximums.Length : m_nNumberOfMaximums; //MW0LGE_21d

            for (int n = 0; n < tot; n++)
            {
                if (bClear || !m_bBlobPeakHold || (m_bBlobPeakHold && !m_bBlobPeakHoldDrop && m_dElapsedFrameStart >= maximums[n].Time + m_fBlobPeakHoldMS))
                {
                    maximums[n].Enabled = false;
                    maximums[n].max_dBm = float.MinValue;
                }
            }
        }

        static private void getFilterXPositions(int rx, int W, bool local_mox, bool displayduplex, out int filter_left_x, out int filter_right_x)
        {
            int Low, High, f_diff, filter_low, filter_high;

            if (rx == 1)
            {
                Low = rx_display_low;
                High = rx_display_high;
                f_diff = freq_diff;
                filter_low = rx1_filter_low;
                filter_high = rx1_filter_high;
            }
            else
            {
                Low = rx2_display_low;
                High = rx2_display_high;
                f_diff = rx2_freq_diff;
                filter_low = rx2_filter_low;
                filter_high = rx2_filter_high;
            }
            if (local_mox)  // && !tx_on_vfob)
            {
                if (!displayduplex)
                {
                    Low = tx_display_low;
                    High = tx_display_high;
                }
                filter_low = tx_filter_low;
                filter_high = tx_filter_high;
            }
            int width = High - Low;
            filter_left_x = (int)((float)(filter_low - Low - f_diff) / width * W);
            filter_right_x = (int)((float)(filter_high - Low - f_diff) / width * W);
        }

        static private bool m_bBlobPeakHold = false;
        static public bool BlobPeakHold {
            get { return m_bBlobPeakHold; }
            set { m_bBlobPeakHold = value; }
        }
        static private double m_fBlobPeakHoldMS = 500f;
        static public double BlobPeakHoldMS {
            get { return m_fBlobPeakHoldMS; }
            set { m_fBlobPeakHoldMS = value; }
        }
        static private bool m_bBlobPeakHoldDrop = false;
        static public bool BlobPeakHoldDrop {
            get { return m_bBlobPeakHoldDrop; }
            set { m_bBlobPeakHoldDrop = value; }
        }

        static private bool isRxDuplex(int rx)
        {
            bool displayduplex;

            //MW0LGE_21k8 commented out
            //if (rx == 1)
            //{
            //    if ((CurrentDisplayMode == DisplayMode.PANAFALL && display_duplex) ||
            //            (CurrentDisplayMode == DisplayMode.WATERFALL && display_duplex) ||
            //            (CurrentDisplayMode == DisplayMode.PANADAPTER && display_duplex) ||
            //            (CurrentDisplayMode == DisplayMode.PANASCOPE && display_duplex) //MW0LGE_21k8
            //            ) displayduplex = true;
            //}
            //else
            //{
            //    if ((CurrentDisplayModeBottom == DisplayMode.PANAFALL && display_duplex) ||
            //            (CurrentDisplayModeBottom == DisplayMode.WATERFALL && display_duplex) ||
            //            (CurrentDisplayModeBottom == DisplayMode.PANADAPTER && display_duplex)
            //            ) displayduplex = true;
            //}

            if (rx == 1)
                displayduplex = display_duplex;
            else
                //RX2 is always duplex off irrespective of front end setting
                displayduplex = false;

            return displayduplex;
        }

        static private int GetDBMPixelPos(int nVerticalShift, float dBm, int rx, int H, bool bIsWaterfall)
        {
            float fOffset;
            bool displayduplex = isRxDuplex(rx);

            int grid_max;
            int grid_min;
            int grid_step;
            int yRange;

            bool local_mox = mox && ((rx == 1 && !tx_on_vfob) || (rx == 2 && tx_on_vfob) || (rx == 1 && tx_on_vfob && !console.RX2Enabled));

            if (rx == 1)
            {
                if (local_mox)  // && !tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    grid_step = tx_spectrum_grid_step;
                }
                else
                {
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                    grid_step = spectrum_grid_step;
                }

                yRange = grid_max - grid_min;                
            }
            else// if(rx == 2)
            {
                if (local_mox)// && tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    grid_step = tx_spectrum_grid_step;
                }
                else
                {
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                    grid_step = rx2_spectrum_grid_step;
                }

                yRange = grid_max - grid_min;
            }

            if (rx == 1)
            {
                if (local_mox) fOffset = tx_display_cal_offset;
                else if (mox && tx_on_vfob && !displayduplex)
                {
                    if (console.RX2Enabled) fOffset = rx1_display_cal_offset;
                    else fOffset = tx_display_cal_offset;
                }
                else fOffset = rx1_display_cal_offset;
            }
            else //if (rx == 2)
            {
                if (local_mox) fOffset = tx_display_cal_offset;
                else fOffset = rx2_display_cal_offset;
            }

            if (!local_mox || (local_mox && displayduplex))
            {
                if (rx == 1) fOffset += rx1_preamp_offset;
                else if (rx == 2) fOffset += rx2_preamp_offset;
            }

            int Y;
            dBm += fOffset;
            //Y = (int)(Math.Floor((grid_max - max) * H / yRange));
            Y = (int)(((grid_max - dBm) * H / yRange) - 0.5f); // -0.5 to mimic floor
            //Y = Math.Min(Y, H);
            Y = Y < H ? Y : H;
            Y += nVerticalShift;

            int top; // a shift to include the gap at the top of the grid lines. This will make sure that this dbm point will match the scales
            if (bIsWaterfall) top = 20; //change top so that the filter gap doesnt change, inore grid spacing
            else top = (int)((double)grid_step * H / yRange); // top is based on grid spacing

            return Y + top;
        }
        static private SharpDX.Direct2D1.Ellipse m_objEllipse = new SharpDX.Direct2D1.Ellipse(Vector2.Zero, 5f, 5f);

        // using -160 instead of -200 as somewhat closer to reality
        static private float m_fNoiseFloorRX1 = -160; // the NF exposed outside Display
        static private float m_fNoiseFloorRX2 = -160;
        static private bool m_bNoiseFloorGoodRX1 = false; // is the noisefloor good? can be used outside Display
        static private bool m_bNoiseFloorGoodRX2 = false;

        static private float m_fFFTBinAverageRX1 = -200;    // the average this frame
        static private float m_fFFTBinAverageRX2 = -200;
        static private float m_fLerpAverageRX1 = -200;      // the lerped average, over attacktime
        static private float m_fLerpAverageRX2 = -200;

        static private float m_fAttackTimeInMSForRX1 = 2000;
        static private float m_fAttackTimeInMSForRX2 = 2000;
        static private int m_nAttackFastFramesRX1 = 60;       // frames neeeded to complete the lerp. As display is entirely fps base, then
        static private int m_nAttackFastFramesRX2 = 60;       // need to caclulate this each time fast attack is enabled

        public static int AttackFastFramesRX1
        {
            get { return m_nAttackFastFramesRX1; }
        }
        public static int AttackFastFramesRX2
        {
            get { return m_nAttackFastFramesRX2; }
        }
        public static float AttackTimeInMSForRX1
        {
            get { return m_fAttackTimeInMSForRX1; }
            set { m_fAttackTimeInMSForRX1 = value; }
        }
        public static float AttackTimeInMSForRX2
        {
            get { return m_fAttackTimeInMSForRX2; }
            set { m_fAttackTimeInMSForRX2 = value; }
        }
        public static bool IsNoiseFloorGoodRX1
        {
            get { return m_bNoiseFloorGoodRX1; }
            set { }
        }
        public static bool IsNoiseFloorGoodRX2
        {
            get { return m_bNoiseFloorGoodRX2; }
            set { }
        }
        public static float NoiseFloorRX1
        {
            get
            {
                m_bNoiseFloorGoodRX1 = false;
                return m_fNoiseFloorRX1;
            }
        }
        public static float NoiseFloorRX2
        {
            get
            {
                m_bNoiseFloorGoodRX2 = false;
                return m_fNoiseFloorRX2;
            }
        }
        public static float ActiveNoiseFloorRX1
        {
            get
            {
                return m_fLerpAverageRX1 + _fNFshiftDBM;
            }
        }
        public static float ActiveNoiseFloorRX2
        {
            get
            {
                return m_fLerpAverageRX2 + _fNFshiftDBM;
            }
        }

        private static float m_dBmPerSecondSpectralPeakFallRX1 = 6.0f;
        private static float m_dBmPerSecondSpectralPeakFallRX2 = 6.0f;
        private static float m_dBmPerSecondPeakBlobFall = 6.0f;
        private static bool m_bActivePeakFillRX1 = false;
        private static bool m_bActivePeakFillRX2 = false;

        public static float SpectralPeakFallRX1
        {
            get { return m_dBmPerSecondSpectralPeakFallRX1; }
            set { m_dBmPerSecondSpectralPeakFallRX1 = value; }
        }
        public static float SpectralPeakFallRX2
        {
            get { return m_dBmPerSecondSpectralPeakFallRX2; }
            set { m_dBmPerSecondSpectralPeakFallRX2 = value; }
        }
        public static float PeakBlobFall
        {
            get { return m_dBmPerSecondPeakBlobFall; }
            set { m_dBmPerSecondPeakBlobFall = value; }
        }
        public static bool ActivePeakFillRX1
        {
            get { return m_bActivePeakFillRX1; }
            set { m_bActivePeakFillRX1 = value; }
        }
        public static bool ActivePeakFillRX2
        {
            get { return m_bActivePeakFillRX2; }
            set { m_bActivePeakFillRX2 = value; }
        }

        private static float m_fActiveDisplayOffsetRX1 = 0;
        private static float m_fActiveDisplayOffsetRX2 = 0;
        public static float ActiveDisplayOffsetRX1
        {
            get { return m_fActiveDisplayOffsetRX1; }
        }
        public static float ActiveDisplayOffsetRX2
        {
            get { return m_fActiveDisplayOffsetRX2; }
        }
        unsafe static private bool DrawPanadapterDX2D(int nVerticalShift, int W, int H, int rx, bool bottom)
        {
            if (grid_control)
            {
                //clearBackgroundDX2D(rx, W, H, bottom);
                drawPanadapterAndWaterfallGridDX2D(nVerticalShift, W, H, rx, bottom, false);
            }

            float local_max_y = float.MinValue;
            float local_max_x = float.MinValue;

            bool displayduplex = isRxDuplex(rx);

            int grid_max = 0;
            int grid_min = 0;

            Maximums[] spectralPeaks = m_rx1_spectrumPeaks;
            double dSpectralPeakHoldDelay;// = 100;
            bool bSpectralPeakHold;// = false;
            bool bPeakBlobs;// = false;
            float dBmSpectralPeakFall;
            bool bActivePeakFill;

            bool local_mox = mox && ((rx == 1 && !tx_on_vfob) || (rx == 2 && tx_on_vfob) || (rx == 1 && tx_on_vfob && !console.RX2Enabled));

            int nDecimatedWidth = W / m_nDecimation;

            int yRange;
            float[] data;

            if (rx == 1)
            {
                bSpectralPeakHold = m_bSpectralPeakHoldRX1 && !m_bDelayRX1SpectrumPeaks;
                dSpectralPeakHoldDelay = m_dSpecralPeakHoldDelayRX1;
                bPeakBlobs = m_bPeakBlobMaximums && !m_bDelayRX1Blobs;
                dBmSpectralPeakFall = m_dBmPerSecondSpectralPeakFallRX1;
                bActivePeakFill = m_bActivePeakFillRX1;

                if (local_mox)  // && !tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                }
                else
                {
                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                }

                yRange = grid_max - grid_min;

                if (data_ready)
                {
                    if (!displayduplex && (local_mox || (mox && tx_on_vfob)) && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < nDecimatedWidth/*current_display_data.Length*/; i++)
                            current_display_data[i] = grid_min - rx1_display_cal_offset;
                    }
                    else
                    {
                        fixed (void* rptr = &new_display_data[0])
                        fixed (void* wptr = &current_display_data[0])
                            Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/nDecimatedWidth * sizeof(float));
                    }
                    data_ready = false;
                }
                data = current_display_data;
            }
            else// if(rx == 2)
            {
                bSpectralPeakHold = m_bSpectralPeakHoldRX2 && !m_bDelayRX2SpectrumPeaks;
                dSpectralPeakHoldDelay = m_dSpecralPeakHoldDelayRX2;
                bPeakBlobs = m_bPeakBlobMaximums && !m_bDelayRX2Blobs;
                dBmSpectralPeakFall = m_dBmPerSecondSpectralPeakFallRX2;
                bActivePeakFill = m_bActivePeakFillRX2;

                if (local_mox)// && tx_on_vfob)
                {
                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                }
                else
                {
                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                }

                yRange = grid_max - grid_min;

                if (data_ready_bottom)
                {

                    //MW0LGE//if (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    if (blank_bottom_display || (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU)))
                    {
                        for (int i = 0; i < nDecimatedWidth/*current_display_data_bottom.Length*/; i++)
                            current_display_data_bottom[i] = grid_min - rx2_display_cal_offset;
                    }
                    else
                    {
                        fixed (void* rptr = &new_display_data_bottom[0])
                        fixed (void* wptr = &current_display_data_bottom[0])
                            Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/nDecimatedWidth * sizeof(float));
                    }

                    data_ready_bottom = false;
                }
                data = current_display_data_bottom;
            }

            dBmSpectralPeakFall /= (float)m_nFps;

            float max;
            float fOffset;

            if (rx == 1)
            {
                if (local_mox) fOffset = tx_display_cal_offset;
                else if (mox && tx_on_vfob && !displayduplex)
                {
                    if (console.RX2Enabled) fOffset = rx1_display_cal_offset;
                    else fOffset = tx_display_cal_offset;
                }
                else fOffset = rx1_display_cal_offset;
            }
            else //if (rx == 2)
            {
                if (local_mox) fOffset = tx_display_cal_offset;
                else fOffset = rx2_display_cal_offset;
            }

            if (!local_mox || (local_mox && displayduplex))
            {
                if (rx == 1) fOffset += rx1_preamp_offset;
                else if (rx == 2) fOffset += rx2_preamp_offset;
            }

            //MW0LGE not used, as filling vertically with lines is faster than a filled very detailed
            //geometry. Just kept for reference
            //PathGeometry sharpGeometry = new PathGeometry(d2dRenderTarget.Factory);
            //using (GeometrySink geo = sharpGeometry.Open())
            //geo.BeginFigure(new SharpDX.Vector2(0, lowerH), FigureBegin.Filled);
            //geo.AddLine(new SharpDX.Vector2(i, Y));
            //        geo.EndFigure(FigureEnd.Closed);
            //        geo.Close();
            //        geo.Dispose();
            //    }
            //sharpGeometry.Dispose();

            SharpDX.Direct2D1.Brush lineBrush;
            SharpDX.Direct2D1.Brush fillBrush;
            SharpDX.Direct2D1.Brush fillPeaksBrush;

            if (local_mox)
            {
                lineBrush = m_bDX2_tx_data_line_pen_brush;
                fillBrush = m_bDX2_tx_data_line_fpen_brush;
                fillPeaksBrush = m_bDX2_dataPeaks_fill_fpen_brush; //todo
            }
            else
            {
                if (rx == 1)
                {
                    lineBrush = m_bUseLinearGradientForDataLine && m_bUseLinearGradient ? m_brushLGDataLineRX1 : m_bDX2_data_line_pen_brush;
                    fillBrush = m_bUseLinearGradient ? m_brushLGDataFillRX1 : m_bDX2_data_fill_fpen_brush;
                }
                else
                {
                    lineBrush = m_bUseLinearGradientForDataLine && m_bUseLinearGradient ? m_brushLGDataLineRX2 : m_bDX2_data_line_pen_brush;
                    fillBrush = m_bUseLinearGradient ? m_brushLGDataFillRX2 : m_bDX2_data_fill_fpen_brush;
                }
                fillPeaksBrush = m_bDX2_dataPeaks_fill_fpen_brush;
            }
            float penWidth = data_line_pen.Width;

            float dbmToPixel = H / (float)yRange;

            // calc start pos
            int Y;
            max = data[0] + fOffset;
            Y = (int)(((grid_max - max) * dbmToPixel) - 0.5f); // -0.5 to mimic floor
            if (Y >= H) Y = H;
            Y += nVerticalShift;
            if (Y < nVerticalShift) Y = nVerticalShift; // crop top

            bool bIgnoringPoints = false;
            bool bOldSpectralPeakPointGood = false;
            SharpDX.Vector2 point = new SharpDX.Vector2();
            SharpDX.Vector2 spectralPeakPoint = new SharpDX.Vector2();
            SharpDX.Vector2 oldSpectralPeakPoint = new SharpDX.Vector2();
            SharpDX.Vector2 lastIgnoredPoint = new SharpDX.Vector2();
            SharpDX.Vector2 bottomPoint = new SharpDX.Vector2(0, nVerticalShift + H);
            SharpDX.Vector2 previousPoint = new SharpDX.Vector2(0, Y);

            float local_max_Pixel_y = float.MinValue;

            int filter_left_x=0, filter_right_x=0;
            if (bPeakBlobs)
            {
                ResetBlobMaximums(rx);
                if (m_bInsideFilterOnly) getFilterXPositions(rx, W, local_mox, displayduplex, out filter_left_x, out filter_right_x);
            }
            if (bSpectralPeakHold)
            {

                if (rx == 1)
                {
                    if (W > m_rx1_spectrumPeaks.Length)
                    {
                        m_rx1_spectrumPeaks = new Maximums[W];
                        ResetSpectrumPeaks(1);
                    }

                    spectralPeaks = m_rx1_spectrumPeaks;
                }
                else
                {
                    if (W > m_rx2_spectrumPeaks.Length)
                    {
                        m_rx2_spectrumPeaks = new Maximums[W];
                        ResetSpectrumPeaks(2);
                    }

                    spectralPeaks = m_rx2_spectrumPeaks;
                }
            }
            
            float mn = float.PositiveInfinity;
            float mx = float.NegativeInfinity;
            int YPosForMx = 0;
            int mnpos = 0;
            int mxpos = 0;
            bool lookformax = true;
            //List <(int pos, float val)> maxtab_tmp = new List<(int pos, float val)>();
            float triggerDelta = 10; //db

            unchecked // we dont expect any overflows
            {
                float averageSum = 0;
                int averageCount = 0;
                float currentAverage = rx == 1 ? m_fFFTBinAverageRX1 + 2 : m_fFFTBinAverageRX2 + 2; // +2db to add some extras above the average

                for (int i = 0; i < nDecimatedWidth; i++)
                {
                    max = data[i] + fOffset;

                    // noise floor
                    if (!local_mox && (max < currentAverage))
                    {
                        averageSum += max;
                        averageCount++;
                    }
                    //

                    Y = (int)(((grid_max - max) * dbmToPixel) - 0.5f); // -0.5 to mimic floor
                    if (Y >= H) Y = H;
                    Y += nVerticalShift;
                    if (Y < nVerticalShift) Y = nVerticalShift; // crop top

                    point.X = i * m_nDecimation;
                    point.Y = Y;

                    if (max > local_max_y)
                    {
                        // store peak
                        local_max_y = max;
                        local_max_x = i * m_nDecimation;
                        local_max_Pixel_y = Y;
                    }

                    if(!local_mox && bSpectralPeakHold && max >= spectralPeaks[i].max_dBm)
                    {
                        spectralPeaks[i].max_dBm = max;
                        spectralPeaks[i].Time = m_dElapsedFrameStart;
                    }

                    ///
                    /// new peak blob code MW0LGE_21b
                    ///
                    if (bPeakBlobs)
                    {
                        bool bInsideFilter = ((i * m_nDecimation) >= filter_left_x) && ((i * m_nDecimation) <= filter_right_x);
                        if (!m_bInsideFilterOnly || (m_bInsideFilterOnly && bInsideFilter))
                        {
                            if (max > mx)
                            {
                                mx = max;
                                YPosForMx = Y;
                                mxpos = i;
                            }
                            if (max < mn)
                            {
                                mn = max;
                                mnpos = i;
                            }
                            if (lookformax)
                            {
                                if (max < mx - triggerDelta)
                                {
                                    processMaximums(rx, mx, mxpos, YPosForMx);
                                    mn = max;
                                    mnpos = i;
                                    lookformax = false;
                                }
                            }
                            else
                            {
                                if (max > mn + triggerDelta)
                                {
                                    mx = max;
                                    YPosForMx = Y;
                                    mxpos = i;
                                    lookformax = true;
                                }
                            }
                        }
                    }
                    ///

                    if (pan_fill) // 0 is top
                    {
                        // draw vertical line, this is so much faster than FillGeometry as the geo created would be so complex any fill alogorthm would struggle
                        bottomPoint.X = point.X;
                        _d2dRenderTarget.DrawLine(bottomPoint, point, fillBrush, m_nDecimation);
                    }

                    //spectral peak
                    if (!local_mox && bSpectralPeakHold && spectralPeaks[i].max_dBm >= max)
                    {
                        // draw to peak, but re-work Y as we might rescale the spectrum vertically
                        spectralPeakPoint.X = point.X;
                        spectralPeakPoint.Y = (int)(((grid_max - spectralPeaks[i].max_dBm) * dbmToPixel) - 0.5f);
                        if (spectralPeakPoint.Y >= H) spectralPeakPoint.Y = H;
                        spectralPeakPoint.Y += nVerticalShift;

                        if (spectralPeakPoint.Y < nVerticalShift) spectralPeakPoint.Y = nVerticalShift; // crop top

                        if (bActivePeakFill)
                        {
                            _d2dRenderTarget.DrawLine(point, spectralPeakPoint, fillPeaksBrush, m_nDecimation);
                        }
                        else
                        {
                            if (!bOldSpectralPeakPointGood)
                            {
                                oldSpectralPeakPoint = spectralPeakPoint;
                                bOldSpectralPeakPointGood = true;
                            }

                            _d2dRenderTarget.DrawLine(oldSpectralPeakPoint, spectralPeakPoint, fillPeaksBrush, data_line_pen.Width/*display_line_width*/);
                            oldSpectralPeakPoint = spectralPeakPoint;
                        }

                        double dElapsed = m_dElapsedFrameStart - spectralPeaks[i].Time;
                        if (spectralPeaks[i].max_dBm > max && (dElapsed > dSpectralPeakHoldDelay))
                        {
                            spectralPeaks[i].max_dBm -= dBmSpectralPeakFall;
                        }
                    }
                    else
                        bOldSpectralPeakPointGood = false;

                    // ignore point if same Y as last point
                    // lines will get longer if flat, reducing number of total points
                    bool bIncludeLinePoint = true;
                    if (point.Y == previousPoint.Y)
                    {
                        if (i > 0 && i < nDecimatedWidth - 1)
                        {
                            bIncludeLinePoint = false;
                            lastIgnoredPoint = point;
                            bIgnoringPoints = true;
                        }
                    }
                    else
                    {
                        if (bIgnoringPoints)
                        {
                            _d2dRenderTarget.DrawLine(previousPoint, lastIgnoredPoint, lineBrush, penWidth);
                            previousPoint = lastIgnoredPoint;
                            bIgnoringPoints = false;
                        }
                    }
                    //

                    if (bIncludeLinePoint)
                    {
                        _d2dRenderTarget.DrawLine(previousPoint, point, lineBrush, penWidth);
                        previousPoint = point;
                    }
                }
                
                //noise floor
                if (!local_mox)
                {
                    bool bPreviousRX1 = _bNoiseFloorAlreadyCalculatedRX1;
                    bool bPreviousRX2 = _bNoiseFloorAlreadyCalculatedRX2;
                    processNoiseFloor(rx, averageCount, averageSum, nDecimatedWidth, false);

                    int yPixelLerp;
                    int yPixelActual;
                    float lerp;

                    if (rx == 1)
                    {
                        if (!m_bFastAttackNoiseFloorRX1 && !bPreviousRX1)
                        {
                            m_fActiveDisplayOffsetRX1 = fOffset;
                            m_fNoiseFloorRX1 = m_fLerpAverageRX1 + _fNFshiftDBM;
                            m_bNoiseFloorGoodRX1 = true;
                        }

                        yPixelLerp = (int)dBToPixel(m_fLerpAverageRX1 + _fNFshiftDBM, H);
                        yPixelActual = (int)dBToPixel(m_fFFTBinAverageRX1 + _fNFshiftDBM, H);

                        lerp = m_fLerpAverageRX1 + _fNFshiftDBM;
                    }
                    else
                    {
                        if (!m_bFastAttackNoiseFloorRX2 && !bPreviousRX2)
                        {
                            m_fActiveDisplayOffsetRX2 = fOffset;
                            m_fNoiseFloorRX2 = m_fLerpAverageRX2 + _fNFshiftDBM;
                            m_bNoiseFloorGoodRX2 = true;
                        }

                        yPixelLerp = (int)dBToRX2Pixel(m_fLerpAverageRX2 + _fNFshiftDBM, H);
                        yPixelActual = (int)dBToRX2Pixel(m_fFFTBinAverageRX2 + _fNFshiftDBM, H);

                        lerp = m_fLerpAverageRX2 + _fNFshiftDBM;
                    }

                    if ((rx == 1 && m_bShowRX1NoiseFloor) || (rx == 2 && m_bShowRX2NoiseFloor))
                    {
                        yPixelLerp = yPixelLerp < H ? yPixelLerp : H;
                        yPixelLerp += nVerticalShift;

                        bool bDraw = !(yPixelLerp < nVerticalShift || yPixelLerp >= nVerticalShift + H); // crop anything off the top

                        if (bDraw)
                        {
                            bool bFast = rx == 1 ? m_bFastAttackNoiseFloorRX1 : m_bFastAttackNoiseFloorRX2;

                            yPixelActual = yPixelActual < H ? yPixelActual : H;
                            yPixelActual += nVerticalShift;

                            SharpDX.Direct2D1.Brush brYG = bFast ? m_bDX2_Gray : m_bDX2_noisefloor;
                            SharpDX.Direct2D1.Brush brY = bFast ? m_bDX2_Gray : m_bDX2_Yellow;

                            int yP = (int)yPixelLerp;

                            Rectangle box = new Rectangle(40, yP - 8, 8, 8);
                            drawFillRectangleDX2D(brYG, box);
                            drawLineDX2D(brYG, 40, yP, W - 40, yP, m_styleDots, m_fNoiseFloorLineWidth); // horiz line

                            if (m_bShowNoiseFloorDBM)
                            {
                                drawLineDX2D(brY, box.X - 3, (int)yPixelActual, box.X - 3, yP, 2); // direction up/down line
                                drawStringDX2D(lerp.ToString(_NFDecimal ? "F1" : "F0"), fontDX2d_font9b, brY, box.X + box.Width, box.Y - 6);
                            }
                            else
                            {
                                drawStringDX2D("-NF", fontDX2d_panafont, brYG, box.X + box.Width, box.Y - 4);
                            }
                        }
                    }
                }

                // peak blobs
                if (bPeakBlobs)
                {
                    Maximums[] maximums;
                    if (rx == 1)
                        maximums = m_nRX1Maximums;
                    else
                        maximums = m_nRX2Maximums;

                    for (int n = 0; n < m_nNumberOfMaximums; n++)
                    {
                        if (maximums[n].Enabled)
                        {
                            if (m_bBlobPeakHold && m_bBlobPeakHoldDrop)
                            {
                                //drop
                                double dElapsed = m_dElapsedFrameStart - maximums[n].Time;
                                if (maximums[n].max_dBm > -200.0 && (dElapsed > m_fBlobPeakHoldMS))
                                {
                                    maximums[n].max_dBm -= m_dBmPerSecondPeakBlobFall / (float)m_nFps;

                                    // recalc Y
                                    int nNewY = (int)(((grid_max - maximums[n].max_dBm) * dbmToPixel) - 0.5f);
                                    nNewY = nNewY < H ? nNewY + nVerticalShift : H + nVerticalShift;
                                    maximums[n].MaxY_pixel = nNewY;
                                }
                                else if (maximums[n].max_dBm <= -200.0)
                                {
                                    maximums[n].Enabled = false; // switch any off that fall off the bottom same as resetmaximums
                                    maximums[n].max_dBm = float.MinValue;
                                }
                            }

                            m_objEllipse.Point.X = maximums[n].X * m_nDecimation;
                            m_objEllipse.Point.Y = maximums[n].MaxY_pixel;

                            bool bDraw = true;
                            if (m_objEllipse.Point.Y < nVerticalShift) bDraw = false; // crop top
                            if (m_objEllipse.Point.Y >= nVerticalShift + H) bDraw = false; // crop top

                            if (bDraw)
                            {
                                string sAppend;
                                if (rx == 1)
                                {
                                    sAppend = m_bShowRX1NoiseFloor && !local_mox ? " (" + (maximums[n].max_dBm - m_fLerpAverageRX1).ToString("f1") + ")" : " (" + (n + 1).ToString() + ")";
                                }
                                else
                                {
                                    sAppend = m_bShowRX2NoiseFloor && !local_mox ? " (" + (maximums[n].max_dBm - m_fLerpAverageRX2).ToString("f1") + ")" : " (" + (n + 1).ToString() + ")";
                                }
                                _d2dRenderTarget.DrawEllipse(m_objEllipse, m_bDX2_PeakBlob);
                                _d2dRenderTarget.DrawText(maximums[n].max_dBm.ToString("f1") + sAppend, fontDX2d_callout, new RectangleF(m_objEllipse.Point.X + 6, m_objEllipse.Point.Y - 8, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_PeakBlobText, DrawTextOptions.None);
                            }
                        }
                    }                    
                }
            }

            if (!bottom)
            {
                max_y = local_max_y;
                max_x = local_max_x;
            }

            if (_showTCISpots) drawSpots(rx, nVerticalShift, W, bottom);

            return true;
        }
        private static int _fNFshiftDBM = 0;
        public static int NFshiftDBM
        {
            get { return NFshiftDBM; }
            set
            {
                int t = value;
                if (t < -6) t = 6;
                if (t > 6) t = 6;
                _fNFshiftDBM = t;
            }
        }
        private static int _NFsensitivity = 3;
        public static int NFsensitivity
        {
            get { return _NFsensitivity; }
            set 
            {
                int t = value;
                if (t < 1) t = 1;
                if (t > 19) t = 19;
                _NFsensitivity = t;
            }
        }
        private static void processNoiseFloor(int rx, int averageCount, float averageSum, int width, bool waterfall)
        {
            if (rx == 1 && _bNoiseFloorAlreadyCalculatedRX1) return; // already done, ignore
            if (rx == 2 && _bNoiseFloorAlreadyCalculatedRX2) return; // already done, ignore

            int fps = waterfall ? m_nFps / waterfall_update_period : m_nFps;

            int requireSamples = (int)(width * (_NFsensitivity / 20f));
            if (rx == 1)
            {
                if (averageCount >= requireSamples)//(width / _NFsensitivity))
                {
                    m_fFFTBinAverageRX1 = (m_fFFTBinAverageRX1 + (averageSum / (float)averageCount)) / 2f;//averageSum / (float)averageCount;
                }
                else
                {
                    m_fFFTBinAverageRX1 += m_bFastAttackNoiseFloorRX1 ? 3f : 1f;
                    if (m_fFFTBinAverageRX1 > 200) m_fFFTBinAverageRX1 = 200;
                }

                // so in attackTime period we need to have moved to where we want
                int framesInAttackTime = m_bFastAttackNoiseFloorRX1 ? 0 : (int)((fps / 1000f) * (double)m_fAttackTimeInMSForRX1);
                framesInAttackTime += 1;

                if (m_fLerpAverageRX1 > m_fFFTBinAverageRX1)
                    m_fLerpAverageRX1 -= (m_fLerpAverageRX1 - m_fFFTBinAverageRX1) / framesInAttackTime;
                else if (m_fLerpAverageRX1 < m_fFFTBinAverageRX1)
                    m_fLerpAverageRX1 += (m_fFFTBinAverageRX1 - m_fLerpAverageRX1) / framesInAttackTime;

                if (m_nAttackFastFramesRX1 > 0) m_nAttackFastFramesRX1--;
                if (m_bFastAttackNoiseFloorRX1 && (Math.Abs(m_fFFTBinAverageRX1 - m_fLerpAverageRX1) < 1f) && (m_nAttackFastFramesRX1 == 0))
                {
                    m_bFastAttackNoiseFloorRX1 = false;
                }

                _bNoiseFloorAlreadyCalculatedRX1 = true;
            }
            else
            {
                if (averageCount >= requireSamples)//(width / _NFsensitivity))
                {
                    m_fFFTBinAverageRX2 = (m_fFFTBinAverageRX2 + (averageSum / (float)averageCount)) / 2f;//averageSum / (float)averageCount;
                }
                else
                {
                    m_fFFTBinAverageRX2 += m_bFastAttackNoiseFloorRX2 ? 3f : 1f;
                    if (m_fFFTBinAverageRX2 > 200) m_fFFTBinAverageRX2 = 200;
                }

                // so in attackTime period we need to have moved to where we want
                int framesInAttackTime = m_bFastAttackNoiseFloorRX2 ? 0 : (int)((fps / 1000f) * (double)m_fAttackTimeInMSForRX2);
                framesInAttackTime += 1;

                if (m_fLerpAverageRX2 > m_fFFTBinAverageRX2)
                    m_fLerpAverageRX2 -= (m_fLerpAverageRX2 - m_fFFTBinAverageRX2) / framesInAttackTime;
                else if (m_fLerpAverageRX2 < m_fFFTBinAverageRX2)
                    m_fLerpAverageRX2 += (m_fFFTBinAverageRX2 - m_fLerpAverageRX2) / framesInAttackTime;

                if (m_nAttackFastFramesRX2 > 0) m_nAttackFastFramesRX2--;
                if (m_bFastAttackNoiseFloorRX2 && (Math.Abs(m_fFFTBinAverageRX2 - m_fLerpAverageRX2) < 1f) && (m_nAttackFastFramesRX2 == 0))
                {
                    m_bFastAttackNoiseFloorRX2 = false;
                }

                _bNoiseFloorAlreadyCalculatedRX2 = true;
            }
        }

        public static void ResetWaterfallTimers()
        {
            m_nRX1WaterFallFrameCount = 0;
            m_nRX2WaterFallFrameCount = 0;
        }       

        private static int m_nRX1WaterFallFrameCount = 0; // 1=every frame, 2= every other, etc
        private static int m_nRX2WaterFallFrameCount = 0;

        unsafe static private bool DrawWaterfallDX2D(int nVerticalShift, int W, int H, int rx, bool bottom)
        {
            // undo the rendertarget transform that is used to move linedraws to middle of pixel grid
            Matrix3x2 originalTransform = _d2dRenderTarget.Transform;
            _d2dRenderTarget.Transform = Matrix3x2.Identity;

            if (waterfall_data == null || waterfall_data.Length < W)
            {
                waterfall_data = new float[W];		// array of points to display
            }

            float local_max_y = float.MinValue;
            bool local_mox = mox && ((rx == 1 && !tx_on_vfob) || (rx == 2 && tx_on_vfob) || (rx == 1 && tx_on_vfob && !console.RX2Enabled));
            float local_min_y_w3sz = float.MaxValue;
            float display_min_w3sz = float.MaxValue;
            float display_max_w3sz = float.MinValue;
            float min_y_w3sz = float.MaxValue;
            int R = 0, G = 0, B = 0;
            bool displayduplex = isRxDuplex(rx);
            float low_threshold = 0.0f;
            float high_threshold = 0.0f;
            float waterfall_minimum = 0.0f;
            ColorSheme cSheme = ColorSheme.enhanced;
            Color low_color = Color.Black;
            Color mid_color = Color.Red;
            Color high_color = Color.Blue;

            int nDecimatedWidth = W / m_nDecimation;

            if (rx == 2)
            {
                if (local_mox)
                {
                    low_threshold = (float)TXWFAmpMin;
                    high_threshold = (float)TXWFAmpMax;
                }
                else
                {
                    high_threshold = rx2_waterfall_high_threshold;
                    if (rx2_waterfall_agc && !m_bRX2_spectrum_thresholds)
                    {
                        if (m_bWaterfallUseNFForACGRX2)
                        {
                            low_threshold = m_fLerpAverageRX2;
                        }
                        else
                        {
                            low_threshold = RX2waterfallPreviousMinValue;
                        }
                        low_threshold -= m_fWaterfallAGCOffsetRX2;
                    }
                    else low_threshold = rx2_waterfall_low_threshold;
                }
                cSheme = rx2_color_sheme;
                low_color = rx2_waterfall_low_color;
                mid_color = rx2_waterfall_mid_color;
                high_color = rx2_waterfall_high_color;
            }
            else
            {
                if (local_mox)
                {
                    low_threshold = (float)TXWFAmpMin;
                    high_threshold = (float)TXWFAmpMax;
                }
                else
                {
                    low_threshold = RX1waterfallPreviousMinValue;
                    high_threshold = waterfall_high_threshold;
                    if (rx1_waterfall_agc && !m_bRX1_spectrum_thresholds)
                    {
                        if (m_bWaterfallUseNFForACGRX1)
                        {
                            low_threshold = m_fLerpAverageRX1;
                        }
                        else
                        {
                            low_threshold = RX1waterfallPreviousMinValue;
                        }
                        low_threshold -= m_fWaterfallAGCOffsetRX1;
                    }
                    else low_threshold = waterfall_low_threshold;
                }
                cSheme = color_sheme;
                low_color = waterfall_low_color;
                mid_color = waterfall_mid_color;
                high_color = waterfall_high_color;
            }

            if (console.PowerOn)
            {
                if (rx == 1 && waterfall_data_ready)
                {
                    if (!displayduplex && local_mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < nDecimatedWidth; i++)
                            current_waterfall_data[i] = -200.0f;
                    }
                    else
                    {
                        fixed (void* rptr = &new_waterfall_data[0])
                        fixed (void* wptr = &current_waterfall_data[0])
                            Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));

                    }
                    waterfall_data_ready = false;
                }
                else if (rx == 2 && waterfall_data_ready_bottom)
                {
                    if (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < nDecimatedWidth; i++)
                            current_waterfall_data_bottom[i] = -200.0f;
                    }
                    else
                    {
                        fixed (void* rptr = &new_waterfall_data_bottom[0])
                        fixed (void* wptr = &current_waterfall_data_bottom[0])
                            Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/nDecimatedWidth * sizeof(float));
                    }

                    waterfall_data_ready_bottom = false;
                }

                bool bRXdraw = false;

                if (rx == 1)
                {
                    m_nRX1WaterFallFrameCount++;
                    if (m_nRX1WaterFallFrameCount >= waterfall_update_period)
                    {
                        m_nRX1WaterFallFrameCount = 0;
                        bRXdraw = true;
                    }
                }
                else
                {
                    m_nRX2WaterFallFrameCount++;
                    if (m_nRX2WaterFallFrameCount >= rx2_waterfall_update_period)
                    {
                        m_nRX2WaterFallFrameCount = 0;
                        bRXdraw = true;
                    }
                }

                if(bRXdraw)
                { 
                    float[] data;                    

                    if (rx == 1)
                        data = current_waterfall_data;
                    else // rx2
                        data = current_waterfall_data_bottom;

                    float max;
                    float fOffset = 0; ///MW0LGE - block of code moved out of for loop +- for now, TODO

                    if (!local_mox)
                    {
                        if (rx == 1) fOffset += rx1_display_cal_offset + (rx1_preamp_offset - alex_preamp_offset);
                        else if (rx == 2) fOffset += rx2_display_cal_offset + (rx2_preamp_offset);
                    }
                    else
                    {
                        if (rx == 1) fOffset += tx_display_cal_offset + (rx1_preamp_offset - alex_preamp_offset);
                        else if (rx == 2) fOffset += tx_display_cal_offset + (rx2_preamp_offset);
                    }

                    float averageSum = 0;
                    int averageCount = 0;
                    float currentAverage = rx == 1 ? m_fFFTBinAverageRX1 + 2 : m_fFFTBinAverageRX2 + 2;

                    for (int i = 0; i < nDecimatedWidth; i++)
                    {
                        max = data[i] + fOffset; //MW0LGE

                        // noise floor
                        if (!local_mox && (max < currentAverage))
                        {
                            averageSum += max;
                            averageCount++;
                        }
                        //

                        if (max > local_max_y)
                        {
                            local_max_y = max;
                            max_x = i * m_nDecimation;
                        }

                        //below added by w3sz
                        if (max < local_min_y_w3sz)
                        {
                            local_min_y_w3sz = max;
                        }
                        //end of addition by w3sz

                        waterfall_data[i] = max;
                    }

                    if (!local_mox)
                    {
                        bool bPreviousRX1 = _bNoiseFloorAlreadyCalculatedRX1;
                        bool bPreviousRX2 = _bNoiseFloorAlreadyCalculatedRX2;
                        processNoiseFloor(rx, averageCount, averageSum, nDecimatedWidth, true);

                        if (rx == 1)
                        {
                            if (!m_bFastAttackNoiseFloorRX1 && !bPreviousRX1)
                            {
                                m_fActiveDisplayOffsetRX1 = fOffset;
                                m_fNoiseFloorRX1 = m_fLerpAverageRX1 + _fNFshiftDBM;
                                m_bNoiseFloorGoodRX1 = true;
                            }
                        }
                        else
                        {
                            if (!m_bFastAttackNoiseFloorRX2 && !bPreviousRX2)
                            {
                                m_fActiveDisplayOffsetRX2 = fOffset;
                                m_fNoiseFloorRX2 = m_fLerpAverageRX2 + _fNFshiftDBM;
                                m_bNoiseFloorGoodRX2 = true;
                            }
                        }
                    }

                    max_y = local_max_y;
                    min_y_w3sz = local_min_y_w3sz;

                    byte nbBitmapAlpaha = 255;
                    int pixel_size = 4;
                    byte[] row = new byte[W * pixel_size];

                    SharpDX.Direct2D1.Bitmap topPixels;

                    // get top pixels, store into new bitmap, ready to display them lower down by 1 pixel
                    if (rx == 1)
                    {
                        topPixels = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2((int)_waterfall_bmp_dx2d.Size.Width, (int)_waterfall_bmp_dx2d.Size.Height - 1), 
                            new BitmapProperties(new SDXPixelFormat(_waterfall_bmp_dx2d.PixelFormat.Format, _ALPHA_MODE)));

                        topPixels.CopyFromBitmap(_waterfall_bmp_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)topPixels.Size.Width, (int)topPixels.Size.Height));                        
                    }
                    else //rx2
                    {
                        topPixels = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2((int)_waterfall_bmp2_dx2d.Size.Width, (int)_waterfall_bmp2_dx2d.Size.Height - 1),
                            new BitmapProperties(new SDXPixelFormat(_waterfall_bmp2_dx2d.PixelFormat.Format, _ALPHA_MODE)));

                        topPixels.CopyFromBitmap(_waterfall_bmp2_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)topPixels.Size.Width, (int)topPixels.Size.Height));
                    }

                    #region colours
                    switch (cSheme)
                    {
                        case (ColorSheme.original):
                            {
                                //// draw new data
                                //for (int i = 0; i < nDecimatedWidth; i++)	// for each pixel in the new line
                                //{
                                //    if (waterfall_data[i] <= low_threshold)		// if less than low threshold, just use low color
                                //    {
                                //        R = low_color.R;
                                //        G = low_color.G;
                                //        B = low_color.B;
                                //    }
                                //    else if (waterfall_data[i] >= high_threshold)// if more than high threshold, just use high color
                                //    {
                                //        R = high_color.R;
                                //        G = high_color.G;
                                //        B = high_color.B;
                                //    }
                                //    else // use a color between high and low
                                //    {
                                //        float percent = (waterfall_data[i] - low_threshold) / (high_threshold - low_threshold);
                                //        if (percent <= 0.5)	// use a gradient between low and mid colors
                                //        {
                                //            percent *= 2;

                                //            R = (int)((1 - percent) * low_color.R + percent * mid_color.R);
                                //            G = (int)((1 - percent) * low_color.G + percent * mid_color.G);
                                //            B = (int)((1 - percent) * low_color.B + percent * mid_color.B);
                                //        }
                                //        else				// use a gradient between mid and high colors
                                //        {
                                //            percent = (float)(percent - 0.5) * 2;

                                //            R = (int)((1 - percent) * mid_color.R + percent * high_color.R);
                                //            G = (int)((1 - percent) * mid_color.G + percent * high_color.G);
                                //            B = (int)((1 - percent) * mid_color.B + percent * high_color.B);
                                //        }
                                //    }
                                //    // set pixel color
                                //    row[(i * m_nDecimation) * pixel_size + 0] = (byte)B;  // set color in memory
                                //    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                //    row[(i * m_nDecimation) * pixel_size + 2] = (byte)R;
                                //    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                //}
                            }
                            break;

                        case (ColorSheme.enhanced):
                            {
                                // draw new data
                                for (int i = 0; i < nDecimatedWidth; i++)	// for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = low_color.R;
                                        G = low_color.G;
                                        B = low_color.B;
                                    }
                                    else if (waterfall_data[i] >= high_threshold)
                                    {
                                        R = 192;
                                        G = 124;
                                        B = 255;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = high_threshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold;
                                        float overall_percent = offset / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.

                                        if (overall_percent < (float)2 / 9) // background to blue
                                        {
                                            float local_percent = overall_percent / ((float)2 / 9);
                                            R = (int)((1.0 - local_percent) * low_color.R);
                                            G = (int)((1.0 - local_percent) * low_color.G);
                                            B = (int)(low_color.B + local_percent * (255 - low_color.B));
                                        }
                                        else if (overall_percent < (float)3 / 9) // blue to blue-green
                                        {
                                            float local_percent = (overall_percent - (float)2 / 9) / ((float)1 / 9);
                                            R = 0;
                                            G = (int)(local_percent * 255);
                                            B = 255;
                                        }
                                        else if (overall_percent < (float)4 / 9) // blue-green to green
                                        {
                                            float local_percent = (overall_percent - (float)3 / 9) / ((float)1 / 9);
                                            R = 0;
                                            G = 255;
                                            B = (int)((1.0 - local_percent) * 255);
                                        }
                                        else if (overall_percent < (float)5 / 9) // green to red-green
                                        {
                                            float local_percent = (overall_percent - (float)4 / 9) / ((float)1 / 9);
                                            R = (int)(local_percent * 255);
                                            G = 255;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)7 / 9) // red-green to red
                                        {
                                            float local_percent = (overall_percent - (float)5 / 9) / ((float)2 / 9);
                                            R = 255;
                                            G = (int)((1.0 - local_percent) * 255);
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)8 / 9) // red to red-blue
                                        {
                                            float local_percent = (overall_percent - (float)7 / 9) / ((float)1 / 9);
                                            R = 255;
                                            G = 0;
                                            B = (int)(local_percent * 255);
                                        }
                                        else // red-blue to purple end
                                        {
                                            float local_percent = (overall_percent - (float)8 / 9) / ((float)1 / 9);
                                            R = (int)((0.75 + 0.25 * (1.0 - local_percent)) * 255);
                                            G = (int)(local_percent * 255 * 0.5);
                                            B = 255;
                                        }
                                    }

                                    if (waterfall_minimum > waterfall_data[i]) //wfagc
                                        waterfall_minimum = waterfall_data[i];

                                    // set pixel color
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)B;	// set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)R;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        case (ColorSheme.SPECTRAN):
                            {
                                // draw new data
                                for (int i = 0; i < nDecimatedWidth; i++)	// for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= WaterfallHighThreshold) // white
                                    {
                                        R = 240;
                                        G = 240;
                                        B = 240;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = WaterfallHighThreshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold;
                                        float local_percent = ((100.0f * offset) / range);

                                        if (local_percent < 5.0f)
                                        {
                                            R = G = 0;
                                            B = (int)local_percent * 5;
                                        }
                                        else if (local_percent < 11.0f)
                                        {
                                            R = G = 0;
                                            B = (int)local_percent * 5;
                                        }
                                        else if (local_percent < 22.0f)
                                        {
                                            R = G = 0;
                                            B = (int)local_percent * 5;
                                        }
                                        else if (local_percent < 44.0f)
                                        {
                                            R = G = 0;
                                            B = (int)local_percent * 5;
                                        }
                                        else if (local_percent < 51.0f)
                                        {
                                            R = G = 0;
                                            B = (int)local_percent * 5;
                                        }
                                        else if (local_percent < 66.0f)
                                        {
                                            R = G = (int)(local_percent - 50) * 2;
                                            B = 255;
                                        }
                                        else if (local_percent < 77.0f)
                                        {
                                            R = G = (int)(local_percent - 50) * 3;
                                            B = 255;
                                        }
                                        else if (local_percent < 88.0f)
                                        {
                                            R = G = (int)(local_percent - 50) * 4;
                                            B = 255;
                                        }
                                        else if (local_percent < 99.0f)
                                        {
                                            R = G = (int)(local_percent - 50) * 5;
                                            B = 255;
                                        }
                                    }

                                    if (waterfall_minimum > waterfall_data[i]) //wfagc
                                        waterfall_minimum = waterfall_data[i];

                                    // set pixel color
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)B;	// set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)R;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        case (ColorSheme.BLACKWHITE):
                            {
                                // draw new data
                                for (int i = 0; i < nDecimatedWidth; i++)	// for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= WaterfallHighThreshold) // white
                                    {
                                        R = 255;
                                        G = 255;
                                        B = 255;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = WaterfallHighThreshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold;
                                        float local_percent = ((100.0f * offset) / range);
                                        R = (int)((local_percent / 100) * 255);
                                        G = R;
                                        B = R;
                                    }

                                    if (waterfall_minimum > waterfall_data[i]) //wfagc
                                        waterfall_minimum = waterfall_data[i];

                                    // set pixel color
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)B;	// set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)R;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        case (ColorSheme.LinLog):
                            {
                                for (int i = 0; i < nDecimatedWidth; i++)	// for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= high_threshold)
                                    {
                                        R = 252;
                                        G = 252;
                                        B = 252;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = high_threshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold + LinLogCor;
                                        float spec_bits = 1024;
                                        float overall_percent = (spec_bits * offset) / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.
                                        float log_fract = (float)(Math.Log10(spec_bits));
                                        if (overall_percent == 0)
                                        {
                                            overall_percent = (float)0.001;
                                        }
                                        overall_percent = (float)(Math.Log10(overall_percent));

                                        if (overall_percent < log_fract / 23)
                                        {
                                            //			float local_percent = overall_percent / ((float)1/23);
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)2 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)1/23) / ((float)1/23);
                                            R = 32;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)3 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)2/23) / ((float)1/23);
                                            R = 64;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)4 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)3/23) / ((float)1/23);
                                            R = 96;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)5 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)4/23) / ((float)1/23);
                                            R = 104;
                                            G = 40;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)6 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)5/23) / ((float)1/23);
                                            R = 112;
                                            G = 60;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)7 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)6/23) / ((float)1/23);
                                            R = 116;
                                            G = 88;
                                            B = 0;
                                        }


                                        else if (overall_percent < (float)8 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)7/23) / ((float)1/23);
                                            R = 92;
                                            G = 112;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)9 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)8/23) / ((float)1/23);
                                            R = 80;
                                            G = 132;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)10 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)9/23) / ((float)1/23);
                                            R = 20;
                                            G = 140;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)11 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)10/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 40;
                                        }
                                        else if (overall_percent < (float)12 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)11/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 120;
                                        }

                                        else if (overall_percent < (float)13 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)12/23) / ((float)1/23);
                                            R = 0;
                                            G = 140;
                                            B = 148;
                                        }
                                        else if (overall_percent < (float)14 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)13/23) / ((float)1/23);
                                            R = 0;
                                            G = 132;
                                            B = 192;
                                        }
                                        else if (overall_percent < (float)15 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)14/23) / ((float)1/23);
                                            R = 0;
                                            G = 112;
                                            B = 200;
                                        }
                                        else if (overall_percent < (float)16 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)15/23) / ((float)1/23);
                                            R = 0;
                                            G = 88;
                                            B = 208;
                                        }
                                        else if (overall_percent < (float)17 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)16/23) / ((float)1/23);
                                            R = 0;
                                            G = 60;
                                            B = 232;
                                        }
                                        else if (overall_percent < (float)18 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)17/23) / ((float)1/23);
                                            R = 0;
                                            G = 40;
                                            B = 252;
                                        }
                                        else if (overall_percent < (float)19 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)18/23) / ((float)1/23);
                                            R = 80;
                                            G = 80;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)20 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)19/23) / ((float)1/23);
                                            R = 124;
                                            G = 124;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)21 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)20/23) / ((float)1/23);
                                            R = 172;
                                            G = 172;
                                            B = 252;
                                        }

                                        else if (overall_percent >= (float)21 * log_fract / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)22/23) / ((float)1/23);
                                            R = 252;
                                            G = 252;
                                            B = 252;
                                        }
                                        else
                                        {
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                    }

                                    if (waterfall_minimum > waterfall_data[i]) //wfagc
                                        waterfall_minimum = waterfall_data[i];

                                    // set pixel color changed by w3sz

                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)R;	// set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)B;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        //  now Linrad palette without log

                        case (ColorSheme.LinRad):
                            {
                                for (int i = 0; i < nDecimatedWidth; i++)	// for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= high_threshold)
                                    {
                                        R = 252;
                                        G = 252;
                                        B = 252;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = high_threshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold + LinCor;
                                        float overall_percent = (offset) / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.


                                        if (overall_percent < (float)1 / 23)
                                        {
                                            //			float local_percent = overall_percent / ((float)1/23);
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)2 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)1/23) / ((float)1/23);
                                            R = 32;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)3 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)2/23) / ((float)1/23);
                                            R = 64;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)4 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)3/23) / ((float)1/23);
                                            R = 96;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)5 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)4/23) / ((float)1/23);
                                            R = 104;
                                            G = 40;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)6 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)5/23) / ((float)1/23);
                                            R = 112;
                                            G = 60;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)7 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)6/23) / ((float)1/23);
                                            R = 116;
                                            G = 88;
                                            B = 0;
                                        }


                                        else if (overall_percent < (float)8 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)7/23) / ((float)1/23);
                                            R = 92;
                                            G = 112;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)9 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)8/23) / ((float)1/23);
                                            R = 80;
                                            G = 132;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)10 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)9/23) / ((float)1/23);
                                            R = 20;
                                            G = 140;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)11 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)10/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 40;
                                        }
                                        else if (overall_percent < (float)12 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)11/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 120;
                                        }

                                        else if (overall_percent < (float)13 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)12/23) / ((float)1/23);
                                            R = 0;
                                            G = 140;
                                            B = 148;
                                        }
                                        else if (overall_percent < (float)14 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)13/23) / ((float)1/23);
                                            R = 0;
                                            G = 132;
                                            B = 192;
                                        }
                                        else if (overall_percent < (float)15 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)14/23) / ((float)1/23);
                                            R = 0;
                                            G = 112;
                                            B = 200;
                                        }
                                        else if (overall_percent < (float)16 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)15/23) / ((float)1/23);
                                            R = 0;
                                            G = 88;
                                            B = 208;
                                        }
                                        else if (overall_percent < (float)17 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)16/23) / ((float)1/23);
                                            R = 0;
                                            G = 60;
                                            B = 232;
                                        }
                                        else if (overall_percent < (float)18 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)17/23) / ((float)1/23);
                                            R = 0;
                                            G = 40;
                                            B = 252;
                                        }
                                        else if (overall_percent < (float)19 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)18/23) / ((float)1/23);
                                            R = 80;
                                            G = 80;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)20 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)19/23) / ((float)1/23);
                                            R = 124;
                                            G = 124;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)21 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)20/23) / ((float)1/23);
                                            R = 172;
                                            G = 172;
                                            B = 252;
                                        }

                                        else if (overall_percent >= (float)21 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)22/23) / ((float)1/23);
                                            R = 252;
                                            G = 252;
                                            B = 252;
                                        }
                                        else
                                        {
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                    }

                                    if (waterfall_minimum > waterfall_data[i])
                                        waterfall_minimum = waterfall_data[i];

                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)R;	// set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)B;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        //  now Linrad palette without log

                        case (ColorSheme.LinAuto):
                            {
                                for (int i = 0; i < nDecimatedWidth; i++)	// for each pixel in the new line
                                {
                                    display_min_w3sz = min_y_w3sz - 5; //for histogram equilization
                                    display_max_w3sz = max_y; //for histogram equalization

                                    if (waterfall_data[i] <= display_min_w3sz)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= display_max_w3sz)
                                    {
                                        R = 252;
                                        G = 252;
                                        B = 252;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = display_max_w3sz - display_min_w3sz;
                                        float offset = waterfall_data[i] - display_min_w3sz; // + GlobalClass.LinCor;
                                        float overall_percent = (offset) / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.


                                        if (overall_percent < (float)1 / 23)
                                        {
                                            //			float local_percent = overall_percent / ((float)1/23);
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)2 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)1/23) / ((float)1/23);
                                            R = 32;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)3 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)2/23) / ((float)1/23);
                                            R = 64;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)4 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)3/23) / ((float)1/23);
                                            R = 96;
                                            G = 0;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)5 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)4/23) / ((float)1/23);
                                            R = 104;
                                            G = 40;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)6 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)5/23) / ((float)1/23);
                                            R = 112;
                                            G = 60;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)7 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)6/23) / ((float)1/23);
                                            R = 116;
                                            G = 88;
                                            B = 0;
                                        }


                                        else if (overall_percent < (float)8 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)7/23) / ((float)1/23);
                                            R = 92;
                                            G = 112;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)9 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)8/23) / ((float)1/23);
                                            R = 80;
                                            G = 132;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)10 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)9/23) / ((float)1/23);
                                            R = 20;
                                            G = 140;
                                            B = 0;
                                        }
                                        else if (overall_percent < (float)11 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)10/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 40;
                                        }
                                        else if (overall_percent < (float)12 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)11/23) / ((float)1/23);
                                            R = 0;
                                            G = 160;
                                            B = 120;
                                        }

                                        else if (overall_percent < (float)13 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)12/23) / ((float)1/23);
                                            R = 0;
                                            G = 140;
                                            B = 148;
                                        }
                                        else if (overall_percent < (float)14 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)13/23) / ((float)1/23);
                                            R = 0;
                                            G = 132;
                                            B = 192;
                                        }
                                        else if (overall_percent < (float)15 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)14/23) / ((float)1/23);
                                            R = 0;
                                            G = 112;
                                            B = 200;
                                        }
                                        else if (overall_percent < (float)16 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)15/23) / ((float)1/23);
                                            R = 0;
                                            G = 88;
                                            B = 208;
                                        }
                                        else if (overall_percent < (float)17 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)16/23) / ((float)1/23);
                                            R = 0;
                                            G = 60;
                                            B = 232;
                                        }
                                        else if (overall_percent < (float)18 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)17/23) / ((float)1/23);
                                            R = 0;
                                            G = 40;
                                            B = 252;
                                        }
                                        else if (overall_percent < (float)19 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)18/23) / ((float)1/23);
                                            R = 80;
                                            G = 80;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)20 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)19/23) / ((float)1/23);
                                            R = 124;
                                            G = 124;
                                            B = 252;
                                        }

                                        else if (overall_percent < (float)21 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)20/23) / ((float)1/23);
                                            R = 172;
                                            G = 172;
                                            B = 252;
                                        }

                                        else if (overall_percent >= (float)21 / 23)
                                        {
                                            //			float local_percent = (overall_percent - (float)22/23) / ((float)1/23);
                                            R = 252;
                                            G = 252;
                                            B = 252;
                                        }
                                        else
                                        {
                                            R = 0;
                                            G = 0;
                                            B = 0;
                                        }
                                    }

                                    // set pixel color changed by w3sz
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)R;	// set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)B;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;
                    }
                    #endregion

                    // fill pixels into decimation spaces so we dont have gaps
                    for (int i = 0; i < nDecimatedWidth; i++)
                    {
                        for (int j = 1; j < m_nDecimation; j++)
                        {
                            row[((i * m_nDecimation) + j) * pixel_size + 0] = row[(i * m_nDecimation) * pixel_size + 0];
                            row[((i * m_nDecimation) + j) * pixel_size + 1] = row[(i * m_nDecimation) * pixel_size + 1];
                            row[((i * m_nDecimation) + j) * pixel_size + 2] = row[(i * m_nDecimation) * pixel_size + 2];
                            row[((i * m_nDecimation) + j) * pixel_size + 3] = row[(i * m_nDecimation) * pixel_size + 3];
                        }
                    }

                    if (rx == 1)
                    {
                        _waterfall_bmp_dx2d.CopyFromMemory(row, W * pixel_size, new SharpDX.Rectangle(0, 0, W, 1));
                        _waterfall_bmp_dx2d.CopyFromBitmap(topPixels, new SharpDX.Point(0, 1));
                    }
                    else
                    {
                        _waterfall_bmp2_dx2d.CopyFromMemory(row, W * pixel_size, new SharpDX.Rectangle(0, 0, W, 1));
                        _waterfall_bmp2_dx2d.CopyFromBitmap(topPixels, new SharpDX.Point(0, 1));
                    }

                    Utilities.Dispose(ref topPixels);
                    topPixels = null;

                    if (rx == 1)
                        RX1waterfallPreviousMinValue = (((RX1waterfallPreviousMinValue * 8) + (waterfall_minimum * 2)) / 10) + 1; //wfagc
                    else
                        RX2waterfallPreviousMinValue = ((RX2waterfallPreviousMinValue * 8) + (waterfall_minimum * 2)) / 10 + 1; //wfagc
                }

                if (rx == 1)
                {
                    _d2dRenderTarget.DrawBitmap(_waterfall_bmp_dx2d, new RectangleF(0, nVerticalShift + 20, _waterfall_bmp_dx2d.Size.Width, _waterfall_bmp_dx2d.Size.Height), m_fRX1WaterfallOpacity, BitmapInterpolationMode.Linear);
                }
                else
                {
                    _d2dRenderTarget.DrawBitmap(_waterfall_bmp2_dx2d, new RectangleF(0, nVerticalShift + 20, _waterfall_bmp2_dx2d.Size.Width, _waterfall_bmp2_dx2d.Size.Height), m_fRX2WaterfallOpacity, BitmapInterpolationMode.Linear);
                }                
            }

            // return the transform to what it was
            _d2dRenderTarget.Transform = originalTransform;

            // MW0LGE now draw any grid/labels/scales over the top of waterfall
            if (grid_control) drawPanadapterAndWaterfallGridDX2D(nVerticalShift, W, H, rx, bottom, true);

            if (_showTCISpots) drawSpots(rx, nVerticalShift, W, bottom);

            return true;
        }

        private static Color4 convertColour(Color c)
        {
            return new Color4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
        private static SharpDX.Direct2D1.SolidColorBrush convertBrush(SolidBrush b)
        {
            return new SharpDX.Direct2D1.SolidColorBrush(_d2dRenderTarget, convertColour(b.Color));
        }

        public static void SetDX2BackgoundImage(System.Drawing.Image image)
        {
            lock (_objDX2Lock)
            {
                if (!_bDX2Setup) return;

                if (_bitmapBackground != null)
                {
                    Utilities.Dispose(ref _bitmapBackground);
                    _bitmapBackground = null;
                }

                if (image != null) {
                    using (Bitmap graphicsImage = new Bitmap(image))
                    {
                        _bitmapBackground = SDXBitmapFromSysBitmap(_d2dRenderTarget, graphicsImage);
                    }
                }
            }
        }

        private static SharpDX.Direct2D1.Bitmap _bitmapBackground;
        private static SharpDX.Direct2D1.Bitmap SDXBitmapFromSysBitmap(RenderTarget rt, System.Drawing.Bitmap bitmap)
        {
            Rectangle sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapProperties bitmapProperties = new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, _ALPHA_MODE)); //was R8G8B8A8_UNorm  //MW0LGE_21k9
            Size2 size = new Size2(bitmap.Width, bitmap.Height);
            
            // Transform pixels from BGRA to RGBA
            int stride = bitmap.Width * sizeof(int);
            DataStream tempStream = new DataStream(bitmap.Height * stride, true, true);

            // Lock System.Drawing.Bitmap
            BitmapData bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            // Convert all pixels 
            for (int y = 0; y < bitmap.Height; y++)
            {
                int offset = bitmapData.Stride * y;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                    byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                    byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                    byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                    //int rgba = R | (G << 8) | (B << 16) | (A << 24); //MW0LGE_21k9
                    //tempStream.Write(rgba);
                    int bgra = B | (G << 8) | (R << 16) | (A << 24);
                    tempStream.Write(bgra);
                }

            }
            bitmap.UnlockBits(bitmapData);
            
            tempStream.Position = 0;

            SharpDX.Direct2D1.Bitmap dx2bitmap = new SharpDX.Direct2D1.Bitmap(rt, size, tempStream, stride, bitmapProperties);

            Utilities.Dispose(ref tempStream);
            tempStream = null;

            return dx2bitmap;
        }

        //--------------------------
        
        private static SharpDX.Direct2D1.Brush m_bDX2_dataPeaks_fill_fpen_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_data_fill_fpen_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_data_line_pen_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_data_line_fpen_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_data_line_pen_brush;

        private static SharpDX.Direct2D1.Brush m_bDX2_sub_rx_filter_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_sub_rx_zero_line_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_filter_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_cw_zero_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_pNotchActive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bBWFillColour;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_pNotchInactive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bBWFillColourInactive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_pTNFInactive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bTNFInactive;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_grid_zero_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_grid_zero_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_vgrid_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_grid_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_hgrid_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_hgrid_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_grid_text_pen;

        private static SharpDX.Direct2D1.Brush m_bDX2_bandstack_overlay_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_bandstack_overlay_brush_lines;
        private static SharpDX.Direct2D1.Brush m_bDX2_bandstack_overlay_brush_highlight;

        private static SharpDX.Direct2D1.Brush m_bDX2_display_filter_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_filter_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bTextCallOutActive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bTextCallOutInactive;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_pHighlighted;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bBWHighlighedFillColour;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_band_edge_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_tx_vgrid_pen_inb;
        private static SharpDX.Direct2D1.Brush m_bDX2_band_edge_pen;
        private static SharpDX.Direct2D1.Brush m_bDX2_grid_pen_inb;
        private static SharpDX.Direct2D1.Brush m_bDX2_Red;
        private static SharpDX.Direct2D1.Brush m_bDX2_Yellow;
        private static SharpDX.Direct2D1.Brush m_bDX2_YellowGreen;
        private static SharpDX.Direct2D1.Brush m_bDX2_Gray;

        private static SharpDX.Direct2D1.Brush m_bDX2_PeakBlob;
        private static SharpDX.Direct2D1.Brush m_bDX2_PeakBlobText;

        private static SharpDX.Direct2D1.Brush m_bDX2_grid_tx_text_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_grid_text_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_pana_text_brush;

        private static SharpDX.Direct2D1.Brush m_bDX2_p1;

        private static SharpDX.Direct2D1.Brush m_bDX2_display_background_brush;

        private static SharpDX.Color4 m_cDX2_display_background_colour;

        private static SharpDX.Direct2D1.Brush m_bDX2_y1_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_y2_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_waveform_line_pen;

        private static SharpDX.Direct2D1.Brush m_bDX2_dhp;
        private static SharpDX.Direct2D1.Brush m_bDX2_dhp1;
        private static SharpDX.Direct2D1.Brush m_bDX2_dhp2;

        private static SharpDX.Direct2D1.StrokeStyle m_styleDots;

        private static SharpDX.Direct2D1.Brush m_bDX2_noisefloor;

        private static SharpDX.Direct2D1.Brush m_bDX2_m_bHightlightNumberScale;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bHightlightNumbers;
        //--------------------------
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataFillRX1 = null;
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataLineRX1 = null;
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataFillRX2 = null;
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataLineRX2 = null;
        //
        private static bool m_bUseLinearGradient = false;
        private static bool m_bUseLinearGradientForDataLine = false;
        public static bool UseLinearGradient {
            get { return m_bUseLinearGradient; }
            set { 
                m_bUseLinearGradient = value;
                if (m_bUseLinearGradient)
                {
                    _bRebuildRX1LinearGradBrush = true;
                    _bRebuildRX2LinearGradBrush = true;
                }
            }
        }
        public static bool UseLinearGradientForDataLine {
            get { return m_bUseLinearGradientForDataLine; }
            set {
                m_bUseLinearGradientForDataLine = value;
            }
        }
        private static void buildLinearGradientBrush(int top, int bottom, int rx)
        {
            int grid_min, grid_max;
            if (rx == 1)
            {
                grid_min = spectrum_grid_min;
                grid_max = spectrum_grid_max;
            }
            else
            {
                grid_min = rx2_spectrum_grid_min;
                grid_max = rx2_spectrum_grid_max;
            }

            List<ucLGPicker.ColourGradientData> lst = console.SetupForm.RX1GradPicker.GetColourGradientDataForDBMRange(grid_min, grid_max);

            GradientStop[] gradientStopsDataFill = new GradientStop[lst.Count];
            GradientStop[] gradientStopsDataLine = new GradientStop[lst.Count];
            for (int n = 0; n < lst.Count; n++)
            {
                Color dataFillColour = Color.FromArgb(data_fill_color.A, lst[n].color.R, lst[n].color.G, lst[n].color.B);
                Color dataLineColour = Color.FromArgb(data_line_color.A, lst[n].color.R, lst[n].color.G, lst[n].color.B);

                gradientStopsDataFill[n] = new GradientStop() { Color = convertColour(dataFillColour), Position = lst[n].percent };
                gradientStopsDataLine[n] = new GradientStop() { Color = convertColour(dataLineColour), Position = lst[n].percent };
            }
            SharpDX.Direct2D1.GradientStopCollection fill = new SharpDX.Direct2D1.GradientStopCollection(_d2dRenderTarget, gradientStopsDataFill);
            SharpDX.Direct2D1.GradientStopCollection line = new SharpDX.Direct2D1.GradientStopCollection(_d2dRenderTarget, gradientStopsDataLine);

            if (rx == 1)
            {
                if (m_brushLGDataFillRX1 != null)
                {
                    Utilities.Dispose(ref m_brushLGDataFillRX1);
                    m_brushLGDataFillRX1 = null;
                }
                m_brushLGDataFillRX1 = new SharpDX.Direct2D1.LinearGradientBrush(_d2dRenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
                {
                    StartPoint = new Vector2(0, bottom),
                    EndPoint = new Vector2(0, top)
                },
                fill);
                if (m_brushLGDataLineRX1 != null)
                {
                    Utilities.Dispose(ref m_brushLGDataLineRX1);
                    m_brushLGDataLineRX1 = null;
                }
                m_brushLGDataLineRX1 = new SharpDX.Direct2D1.LinearGradientBrush(_d2dRenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
                {
                    StartPoint = new Vector2(0, bottom),
                    EndPoint = new Vector2(0, top)
                },
                line);
            }
            else
            {
                if (m_brushLGDataFillRX2 != null)
                {
                    Utilities.Dispose(ref m_brushLGDataFillRX2);
                    m_brushLGDataFillRX2 = null;
                }
                m_brushLGDataFillRX2 = new SharpDX.Direct2D1.LinearGradientBrush(_d2dRenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
                {
                    StartPoint = new Vector2(0, bottom),
                    EndPoint = new Vector2(0, top)
                },
                fill);
                if (m_brushLGDataLineRX2 != null)
                {
                    Utilities.Dispose(ref m_brushLGDataLineRX2);
                    m_brushLGDataLineRX2 = null;
                }
                m_brushLGDataLineRX2 = new SharpDX.Direct2D1.LinearGradientBrush(_d2dRenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
                {
                    StartPoint = new Vector2(0, bottom),
                    EndPoint = new Vector2(0, top)
                },
                line);
            }

            // clear up
            Utilities.Dispose(ref fill);
            Utilities.Dispose(ref line);

            fill = null;
            line = null;
        }

        private static void releaseDX2Resources()
        {
            clearAllDynamicBrushes();

            if (m_brushLGDataFillRX1 != null) Utilities.Dispose(ref m_brushLGDataFillRX1);
            if (m_brushLGDataFillRX2 != null) Utilities.Dispose(ref m_brushLGDataFillRX2);
            if (m_brushLGDataLineRX1 != null) Utilities.Dispose(ref m_brushLGDataLineRX1);
            if (m_brushLGDataLineRX2 != null) Utilities.Dispose(ref m_brushLGDataLineRX2);
            _bRebuildRX1LinearGradBrush = false;
            _bRebuildRX2LinearGradBrush = false;

            if (m_bDX2_dataPeaks_fill_fpen_brush != null) Utilities.Dispose(ref m_bDX2_dataPeaks_fill_fpen_brush);
            if (m_bDX2_data_fill_fpen_brush != null) Utilities.Dispose(ref m_bDX2_data_fill_fpen_brush);
            if (m_bDX2_data_line_pen_brush != null) Utilities.Dispose(ref m_bDX2_data_line_pen_brush);
            if (m_bDX2_tx_data_line_fpen_brush != null) Utilities.Dispose(ref m_bDX2_tx_data_line_fpen_brush);
            if (m_bDX2_tx_data_line_pen_brush != null) Utilities.Dispose(ref m_bDX2_tx_data_line_pen_brush);

            if (m_bDX2_p1 != null) Utilities.Dispose(ref m_bDX2_p1);
            if (m_bDX2_display_background_brush != null) Utilities.Dispose(ref m_bDX2_display_background_brush);

            //MW0LGE_21k9rc6 now disposed in clearAllDynamicBrushes
            //if (m_bDX2_m_bHightlightNumbers != null) Utilities.Dispose(ref m_bDX2_m_bHightlightNumbers);
            //if (m_bDX2_m_bHightlightNumberScale != null) Utilities.Dispose(ref m_bDX2_m_bHightlightNumberScale);

            if (m_bDX2_grid_tx_text_brush != null) Utilities.Dispose(ref m_bDX2_grid_tx_text_brush);
            if (m_bDX2_grid_text_brush != null) Utilities.Dispose(ref m_bDX2_grid_text_brush);
            if (m_bDX2_pana_text_brush != null) Utilities.Dispose(ref m_bDX2_pana_text_brush);

            if (m_bDX2_bandstack_overlay_brush != null) Utilities.Dispose(ref m_bDX2_bandstack_overlay_brush);
            if (m_bDX2_bandstack_overlay_brush_lines != null) Utilities.Dispose(ref m_bDX2_bandstack_overlay_brush_lines);
            if (m_bDX2_bandstack_overlay_brush_highlight != null) Utilities.Dispose(ref m_bDX2_bandstack_overlay_brush_highlight);

            if (m_bDX2_display_filter_brush != null) Utilities.Dispose(ref m_bDX2_display_filter_brush);
            if (m_bDX2_tx_filter_brush != null) Utilities.Dispose(ref m_bDX2_tx_filter_brush);
            if (m_bDX2_m_bTextCallOutActive != null) Utilities.Dispose(ref m_bDX2_m_bTextCallOutActive);
            if (m_bDX2_m_bTextCallOutInactive != null) Utilities.Dispose(ref m_bDX2_m_bTextCallOutInactive);
            if (m_bDX2_m_pHighlighted != null) Utilities.Dispose(ref m_bDX2_m_pHighlighted);
            if (m_bDX2_m_bBWHighlighedFillColour != null) Utilities.Dispose(ref m_bDX2_m_bBWHighlighedFillColour);
            if (m_bDX2_tx_band_edge_pen != null) Utilities.Dispose(ref m_bDX2_tx_band_edge_pen);
            if (m_bDX2_tx_vgrid_pen_inb != null) Utilities.Dispose(ref m_bDX2_tx_vgrid_pen_inb);
            if (m_bDX2_band_edge_pen != null) Utilities.Dispose(ref m_bDX2_band_edge_pen);
            if (m_bDX2_grid_pen_inb != null) Utilities.Dispose(ref m_bDX2_grid_pen_inb);

            //MW0LGE_21k9rc6 now disposed in clearAllDynamicBrushes
            //if (m_bDX2_Red != null) Utilities.Dispose(ref m_bDX2_Red);
            //if (m_bDX2_Yellow != null) Utilities.Dispose(ref m_bDX2_Yellow);
            //if (m_bDX2_YellowGreen != null) Utilities.Dispose(ref m_bDX2_YellowGreen);
            //if (m_bDX2_Gray != null) Utilities.Dispose(ref m_bDX2_Gray);

            //if (m_bDX2_PeakBlob != null) Utilities.Dispose(ref m_bDX2_PeakBlob);
            //if (m_bDX2_PeakBlobText != null) Utilities.Dispose(ref m_bDX2_PeakBlobText);

            //if (m_bDX2_y1_brush != null) Utilities.Dispose(ref m_bDX2_y1_brush);
            //if (m_bDX2_y2_brush != null) Utilities.Dispose(ref m_bDX2_y2_brush);
            //if (m_bDX2_waveform_line_pen != null) Utilities.Dispose(ref m_bDX2_waveform_line_pen);

            //if (m_bDX2_dhp != null) Utilities.Dispose(ref m_bDX2_dhp);
            //if (m_bDX2_dhp1 != null) Utilities.Dispose(ref m_bDX2_dhp1);
            //if (m_bDX2_dhp2 != null) Utilities.Dispose(ref m_bDX2_dhp2);

            if (m_bDX2_sub_rx_filter_brush != null) Utilities.Dispose(ref m_bDX2_sub_rx_filter_brush);
            if (m_bDX2_sub_rx_zero_line_pen != null) Utilities.Dispose(ref m_bDX2_sub_rx_zero_line_pen);
            if (m_bDX2_tx_filter_pen != null) Utilities.Dispose(ref m_bDX2_tx_filter_pen);
            if (m_bDX2_cw_zero_pen != null) Utilities.Dispose(ref m_bDX2_cw_zero_pen);
            if (m_bDX2_m_pNotchActive != null) Utilities.Dispose(ref m_bDX2_m_pNotchActive);
            if (m_bDX2_m_bBWFillColour != null) Utilities.Dispose(ref m_bDX2_m_bBWFillColour);
            if (m_bDX2_m_pNotchInactive != null) Utilities.Dispose(ref m_bDX2_m_pNotchInactive);
            if (m_bDX2_m_bBWFillColourInactive != null) Utilities.Dispose(ref m_bDX2_m_bBWFillColourInactive);
            if (m_bDX2_m_pTNFInactive != null) Utilities.Dispose(ref m_bDX2_m_pTNFInactive);
            if (m_bDX2_m_bTNFInactive != null) Utilities.Dispose(ref m_bDX2_m_bTNFInactive);
            if (m_bDX2_tx_grid_zero_pen != null) Utilities.Dispose(ref m_bDX2_tx_grid_zero_pen);
            if (m_bDX2_grid_zero_pen != null) Utilities.Dispose(ref m_bDX2_grid_zero_pen);

            if (m_bDX2_tx_vgrid_pen != null) Utilities.Dispose(ref m_bDX2_tx_vgrid_pen);
            if (m_bDX2_grid_pen != null) Utilities.Dispose(ref m_bDX2_grid_pen);
            if (m_bDX2_tx_hgrid_pen != null) Utilities.Dispose(ref m_bDX2_tx_hgrid_pen);
            if (m_bDX2_hgrid_pen != null) Utilities.Dispose(ref m_bDX2_hgrid_pen);
            if (m_bDX2_grid_text_pen != null) Utilities.Dispose(ref m_bDX2_grid_text_pen);

            if (m_styleDots != null) Utilities.Dispose(ref m_styleDots);

            if (m_bDX2_noisefloor != null) Utilities.Dispose(ref m_bDX2_noisefloor);

            //
            m_brushLGDataFillRX1 = null;
            m_brushLGDataFillRX2 = null; 
            m_brushLGDataLineRX1 = null; 
            m_brushLGDataLineRX2 = null;

            m_bDX2_dataPeaks_fill_fpen_brush = null; 
            m_bDX2_data_fill_fpen_brush = null;
            m_bDX2_data_line_pen_brush = null; 
            m_bDX2_tx_data_line_fpen_brush = null;
            m_bDX2_tx_data_line_pen_brush = null;

            m_bDX2_p1 = null;
            m_bDX2_display_background_brush = null;

            //MW0LGE_21k9rc6 just assign to null, but they are disposed of in clearAllDynamicBrushes
            m_bDX2_m_bHightlightNumbers = null;
            m_bDX2_m_bHightlightNumberScale = null;

            m_bDX2_grid_tx_text_brush = null;
            m_bDX2_grid_text_brush = null; 
            m_bDX2_pana_text_brush = null; 

            m_bDX2_bandstack_overlay_brush = null; 
            m_bDX2_bandstack_overlay_brush_lines = null; 
            m_bDX2_bandstack_overlay_brush_highlight = null; 

            m_bDX2_display_filter_brush = null; 
            m_bDX2_tx_filter_brush = null; 
            m_bDX2_m_bTextCallOutActive = null;
            m_bDX2_m_bTextCallOutInactive = null;
            m_bDX2_m_pHighlighted = null;
            m_bDX2_m_bBWHighlighedFillColour = null;
            m_bDX2_tx_band_edge_pen = null;
            m_bDX2_tx_vgrid_pen_inb = null;
            m_bDX2_band_edge_pen = null;
            m_bDX2_grid_pen_inb = null;

            //MW0LGE_21k9rc6 just assign to null, but they are disposed of in clearAllDynamicBrushes
            m_bDX2_Red = null;
            m_bDX2_Yellow = null;
            m_bDX2_YellowGreen = null;
            m_bDX2_Gray = null;

            m_bDX2_PeakBlob = null;
            m_bDX2_PeakBlobText = null;

            m_bDX2_y1_brush = null;
            m_bDX2_y2_brush = null;
            m_bDX2_waveform_line_pen = null;

            m_bDX2_dhp = null;
            m_bDX2_dhp1 = null;
            m_bDX2_dhp2 = null;

            m_bDX2_sub_rx_filter_brush = null;
            m_bDX2_sub_rx_zero_line_pen = null;
            m_bDX2_tx_filter_pen = null;
            m_bDX2_cw_zero_pen = null;
            m_bDX2_m_pNotchActive = null;
            m_bDX2_m_bBWFillColour = null;
            m_bDX2_m_pNotchInactive = null;
            m_bDX2_m_bBWFillColourInactive = null;
            m_bDX2_m_pTNFInactive = null;
            m_bDX2_m_bTNFInactive = null;
            m_bDX2_tx_grid_zero_pen = null;
            m_bDX2_grid_zero_pen = null;

            m_bDX2_tx_vgrid_pen = null;
            m_bDX2_grid_pen = null;
            m_bDX2_tx_hgrid_pen = null;
            m_bDX2_hgrid_pen = null;
            m_bDX2_grid_text_pen = null;

            m_styleDots = null;

            m_bDX2_noisefloor = null;
            //

        }
        private static void buildDX2Resources()
        {
            lock (_objDX2Lock)
            {
                if (!_bDX2Setup) return;

                releaseDX2Resources();

                _bRebuildRX1LinearGradBrush = true;
                _bRebuildRX2LinearGradBrush = true;

                m_bDX2_dataPeaks_fill_fpen_brush = convertBrush((SolidBrush)dataPeaks_fill_fpen.Brush);
                m_bDX2_data_fill_fpen_brush = convertBrush((SolidBrush)data_fill_fpen.Brush);
                m_bDX2_data_line_pen_brush = convertBrush((SolidBrush)data_line_pen.Brush);
                m_bDX2_tx_data_line_fpen_brush = convertBrush((SolidBrush)tx_data_line_fpen.Brush);
                m_bDX2_tx_data_line_pen_brush = convertBrush((SolidBrush)tx_data_line_pen.Brush);

                m_bDX2_p1 = convertBrush((SolidBrush)p1.Brush);
                m_bDX2_display_background_brush = convertBrush((SolidBrush)display_background_brush);
                
                m_cDX2_display_background_colour = convertColour(display_background_brush.Color); // does not need dispose as it is a type

                m_bDX2_m_bHightlightNumbers = getDXBrushForColour(Color.FromArgb(255, 255, 255));       //convertBrush(new SolidBrush(Color.FromArgb(255, 255, 255)));
                m_bDX2_m_bHightlightNumberScale = getDXBrushForColour(Color.FromArgb(192, 64, 64, 64)); //convertBrush(new SolidBrush(Color.FromArgb(192, 64, 64, 64)));

                m_bDX2_grid_tx_text_brush = convertBrush((SolidBrush)grid_tx_text_brush);
                m_bDX2_grid_text_brush = convertBrush((SolidBrush)grid_text_brush);
                m_bDX2_pana_text_brush = convertBrush((SolidBrush)pana_text_brush);

                m_bDX2_bandstack_overlay_brush = convertBrush((SolidBrush)bandstack_overlay_brush);
                m_bDX2_bandstack_overlay_brush_lines = convertBrush((SolidBrush)bandstack_overlay_brush_lines);
                m_bDX2_bandstack_overlay_brush_highlight = convertBrush((SolidBrush)bandstack_overlay_brush_highlight);

                m_bDX2_display_filter_brush = convertBrush((SolidBrush)display_filter_brush);
                m_bDX2_tx_filter_brush = convertBrush((SolidBrush)tx_filter_brush);
                m_bDX2_m_bTextCallOutActive = convertBrush((SolidBrush)m_bTextCallOutActive);
                m_bDX2_m_bTextCallOutInactive = convertBrush((SolidBrush)m_bTextCallOutInactive);
                m_bDX2_m_pHighlighted = convertBrush((SolidBrush)m_pHighlighted.Brush);
                m_bDX2_m_bBWHighlighedFillColour = convertBrush((SolidBrush)m_bBWHighlighedFillColour);
                m_bDX2_tx_band_edge_pen = convertBrush((SolidBrush)tx_band_edge_pen.Brush);
                m_bDX2_tx_vgrid_pen_inb = convertBrush((SolidBrush)tx_vgrid_pen_inb.Brush);
                m_bDX2_band_edge_pen = convertBrush((SolidBrush)band_edge_pen.Brush);
                m_bDX2_grid_pen_inb = convertBrush((SolidBrush)grid_pen_inb.Brush);

                m_bDX2_Red = getDXBrushForColour(Color.Red);                    //convertBrush(new SolidBrush(Color.Red));  //MW0LGE_21k9rc6
                m_bDX2_Yellow = getDXBrushForColour(Color.Yellow);              //convertBrush(new SolidBrush(Color.Yellow));
                m_bDX2_YellowGreen = getDXBrushForColour(Color.YellowGreen);    //convertBrush(new SolidBrush(Color.YellowGreen));
                m_bDX2_Gray = getDXBrushForColour(Color.Gray);                  //convertBrush(new SolidBrush(Color.Gray));

                m_bDX2_PeakBlob = getDXBrushForColour(Color.OrangeRed);      //convertBrush(new SolidBrush(Color.OrangeRed));
                m_bDX2_PeakBlobText = getDXBrushForColour(Color.Chartreuse); //convertBrush(new SolidBrush(Color.Chartreuse));

                m_bDX2_y1_brush = getDXBrushForColour(Color.FromArgb(64, 64, 64));  //convertBrush(new SolidBrush(Color.FromArgb(64, 64, 64)));
                m_bDX2_y2_brush = getDXBrushForColour(Color.FromArgb(48, 48, 48));  //convertBrush(new SolidBrush(Color.FromArgb(48, 48, 48)));
                m_bDX2_waveform_line_pen = getDXBrushForColour(Color.LightGreen);   //convertBrush(new SolidBrush(Color.LightGreen));

                m_bDX2_dhp = getDXBrushForColour(Color.FromArgb(0, 255, 0));        //convertBrush(new SolidBrush(Color.FromArgb(0, 255, 0)));
                m_bDX2_dhp1 = getDXBrushForColour(Color.FromArgb(150, 0, 0, 255));  //convertBrush(new SolidBrush(Color.FromArgb(150, 0, 0, 255)));
                m_bDX2_dhp2 = getDXBrushForColour(Color.FromArgb(150, 255, 0, 0));  //convertBrush(new SolidBrush(Color.FromArgb(150, 255, 0, 0)));

                m_bDX2_sub_rx_filter_brush = convertBrush((SolidBrush)sub_rx_filter_brush);
                m_bDX2_sub_rx_zero_line_pen = convertBrush((SolidBrush)sub_rx_zero_line_pen.Brush);
                m_bDX2_tx_filter_pen = convertBrush((SolidBrush)tx_filter_pen.Brush);
                m_bDX2_cw_zero_pen = convertBrush((SolidBrush)cw_zero_pen.Brush);
                m_bDX2_m_pNotchActive = convertBrush((SolidBrush)m_pNotchActive.Brush);
                m_bDX2_m_bBWFillColour = convertBrush((SolidBrush)m_bBWFillColour);
                m_bDX2_m_pNotchInactive = convertBrush((SolidBrush)m_pNotchInactive.Brush);
                m_bDX2_m_bBWFillColourInactive = convertBrush((SolidBrush)m_bBWFillColourInactive);
                m_bDX2_m_pTNFInactive = convertBrush((SolidBrush)m_pTNFInactive.Brush);
                m_bDX2_m_bTNFInactive = convertBrush((SolidBrush)m_bTNFInactive);
                m_bDX2_tx_grid_zero_pen = convertBrush((SolidBrush)tx_grid_zero_pen.Brush);
                m_bDX2_grid_zero_pen = convertBrush((SolidBrush)grid_zero_pen.Brush);

                m_bDX2_tx_vgrid_pen = convertBrush((SolidBrush)tx_vgrid_pen.Brush);
                m_bDX2_grid_pen = convertBrush((SolidBrush)grid_pen.Brush);
                m_bDX2_tx_hgrid_pen = convertBrush((SolidBrush)tx_hgrid_pen.Brush);
                m_bDX2_hgrid_pen = convertBrush((SolidBrush)hgrid_pen.Brush);
                m_bDX2_grid_text_pen = convertBrush((SolidBrush)grid_text_pen.Brush);

                StrokeStyleProperties ssp = new StrokeStyleProperties() { DashOffset = 2, DashStyle = SharpDX.Direct2D1.DashStyle.Dash };
                m_styleDots = new StrokeStyle(_d2dFactory, ssp);
                
                m_bDX2_noisefloor = convertBrush(new SolidBrush(noisefloor_color));
            }
        }
        //--------------------------
        private static SharpDX.DirectWrite.Factory fontFactory;
        //
        private static SharpDX.DirectWrite.TextFormat fontDX2d_callout;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font9;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font9b;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font9c;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_panafont;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font10;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font14;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font32;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font1;
        //--------------------------
        private static void releaseFonts()
        {
            if (fontDX2d_callout != null) Utilities.Dispose(ref fontDX2d_callout);
            if (fontDX2d_font9 != null) Utilities.Dispose(ref fontDX2d_font9);
            if (fontDX2d_font9b != null) Utilities.Dispose(ref fontDX2d_font9b);
            if (fontDX2d_font9c != null) Utilities.Dispose(ref fontDX2d_font9c);
            if (fontDX2d_panafont != null) Utilities.Dispose(ref fontDX2d_panafont);
            if (fontDX2d_font10 != null) Utilities.Dispose(ref fontDX2d_font10);
            if (fontDX2d_font14 != null) Utilities.Dispose(ref fontDX2d_font14);
            if (fontDX2d_font32 != null) Utilities.Dispose(ref fontDX2d_font32);
            if (fontDX2d_font1 != null) Utilities.Dispose(ref fontDX2d_font1);

            if (fontFactory != null) Utilities.Dispose(ref fontFactory);

            fontDX2d_callout = null;
            fontDX2d_font9 = null;
            fontDX2d_font9b = null;
            fontDX2d_font9c = null;
            fontDX2d_panafont = null;
            fontDX2d_font10 = null;
            fontDX2d_font14 = null;
            fontDX2d_font32 = null;
            fontDX2d_font1 = null;
            fontFactory = null;
        }
        private static void buildFontsDX2D()
        {
            lock (_objDX2Lock)
            {
                if (!_bDX2Setup) return;

                releaseFonts();

                fontFactory = new SharpDX.DirectWrite.Factory();

                fontDX2d_callout = new SharpDX.DirectWrite.TextFormat(fontFactory, m_fntCallOutFont.FontFamily.Name, (m_fntCallOutFont.Size/72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font9 = new SharpDX.DirectWrite.TextFormat(fontFactory, font9.FontFamily.Name, (font9.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font9b = new SharpDX.DirectWrite.TextFormat(fontFactory, font9b.FontFamily.Name,SharpDX.DirectWrite.FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, (font9b.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font9c = new SharpDX.DirectWrite.TextFormat(fontFactory, font95.FontFamily.Name, (font95.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_panafont = new SharpDX.DirectWrite.TextFormat(fontFactory, pana_font.FontFamily.Name, (pana_font.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font10 = new SharpDX.DirectWrite.TextFormat(fontFactory, font10.FontFamily.Name, (font10.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font14 = new SharpDX.DirectWrite.TextFormat(fontFactory, font14.FontFamily.Name, (font14.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font32 = new SharpDX.DirectWrite.TextFormat(fontFactory, font32b.FontFamily.Name, (font32b.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font1 = new SharpDX.DirectWrite.TextFormat(fontFactory, font1.FontFamily.Name, (font1.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
            }
        }
        static void clearBackgroundDX2D(int rx, int W, int H, bool bottom)
        {
            // MW0LGE
            if (rx == 1)
            {
                if (bottom)
                {
                    _d2dRenderTarget.FillRectangle(new RectangleF(0, H, W, H), m_bDX2_display_background_brush);
                }
                else
                {
                    _d2dRenderTarget.FillRectangle(new RectangleF(0, 0, W, H), m_bDX2_display_background_brush);
                }
            }
            else if (rx == 2)
            {
                if (bottom)
                {
                    if (current_display_mode_bottom == DisplayMode.PANAFALL)
                    {
                        _d2dRenderTarget.FillRectangle(new RectangleF(0, H * 3, W, H), m_bDX2_display_background_brush);
                    }
                    else _d2dRenderTarget.FillRectangle(new RectangleF(0, H, W, H), m_bDX2_display_background_brush);
                }
                else
                {
                    _d2dRenderTarget.FillRectangle(new RectangleF(0, H * 2, W, H), m_bDX2_display_background_brush);
                }
            }
        }
        private static void drawLineDX2D(SharpDX.Direct2D1.Brush b, float x1, float y1, float x2, float y2, float strokeWidth = 1f )
        {
            //0.5f's to move into 'centre' of desired pixel
            _d2dRenderTarget.DrawLine(new SharpDX.Vector2(x1/* + 0.5f*/, y1/* + 0.5f*/), new SharpDX.Vector2(x2/* + 0.5f*/, y2/* + 0.5f*/), b, strokeWidth);
        }
        private static void drawLineDX2D(SharpDX.Direct2D1.Brush b, float x1, float y1, float x2, float y2, StrokeStyle strokeStyle, float strokeWidth = 1f)
        {
            //0.5f's to move into 'centre' of desired pixel
            _d2dRenderTarget.DrawLine(new SharpDX.Vector2(x1/* + 0.5f*/, y1/* + 0.5f*/), new SharpDX.Vector2(x2/* + 0.5f*/, y2/* + 0.5f*/), b, strokeWidth, strokeStyle);
        }
        private static void drawFillRectangleDX2D(SharpDX.Direct2D1.Brush b, float x, float y, float w, float h)
        {
            RectangleF rect = new RectangleF(x, y, w, h);

            _d2dRenderTarget.FillRectangle(rect, b);
        }
        private static void drawRectangleDX2D(SharpDX.Direct2D1.Brush b, float x, float y, float w, float h)
        {
            RectangleF rect = new RectangleF(x, y, w, h);

            _d2dRenderTarget.DrawRectangle(rect, b);
        }
        private static void drawElipseDX2D(SharpDX.Direct2D1.Brush b, float xMiddle, float yMiddle, float w, float h)
        {
            Ellipse e = new Ellipse(new SharpDX.Vector2(xMiddle/* + 0.5f*/, yMiddle/* + 0.5f*/), w / 2, h / 2);

            _d2dRenderTarget.DrawEllipse(e, b);
        }
        private static void drawFillElipseDX2D(SharpDX.Direct2D1.Brush b, float xMiddle, float yMiddle, float w, float h)
        {
            Ellipse e = new Ellipse(new SharpDX.Vector2(xMiddle/* + 0.5f*/, yMiddle/* + 0.5f*/), w / 2, h / 2);

            _d2dRenderTarget.FillEllipse(e, b);
        }
        private static void drawRectangleDX2D(SharpDX.Direct2D1.Brush b, Rectangle r, float lineWidth = 1)
        {
            RectangleF rect = new RectangleF(r.X, r.Y, r.Width, r.Height);

            _d2dRenderTarget.DrawRectangle(rect, b, lineWidth);
        }
        private static void drawFillRectangleDX2D(SharpDX.Direct2D1.Brush b, Rectangle r)
        {
            RectangleF rect = new RectangleF(r.X, r.Y, r.Width, r.Height);

            _d2dRenderTarget.FillRectangle(rect, b);
        }
        private static void drawStringDX2D(string s, SharpDX.DirectWrite.TextFormat tf, SharpDX.Direct2D1.Brush b, float x, float y)
        {
            RectangleF rect = new RectangleF(x, y, float.PositiveInfinity, float.PositiveInfinity);
            _d2dRenderTarget.DrawText(s, tf, rect, b, DrawTextOptions.None);
        }
        private static void drawFilterOverlayDX2D(SharpDX.Direct2D1.Brush brush, int filter_left_x, int filter_right_x, int W, int H, int rx, int top, bool bottom, int nVerticalShfit)
        {
            // make sure something visible
            if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

            // draw rx filter
            int nWidth = filter_right_x - filter_left_x;

            RectangleF rect = new RectangleF(filter_left_x, nVerticalShfit + top, nWidth, H - top);
            _d2dRenderTarget.FillRectangle(rect, brush);
        }
        private static void drawChannelBarDX2D(Channel chan, int left, int right, int top, int height, Color c, Color h)
        {
            int width = right - left;

            // shade in the channel
            drawFillRectangleDX2D(convertBrush(new SolidBrush(c)), left, top, width, height);

            // draw a left and right line on the side of the rectancle if wide enough
            if (width > 2)
            {
                using (Pen p = new Pen(h, 1))
                {
                    drawLineDX2D(convertBrush((SolidBrush)p.Brush), left, top, left, top + height - 1, p.Width);
                    drawLineDX2D(convertBrush((SolidBrush)p.Brush), right, top, right, top + height - 1, p.Width);
                }
            }
        }
        private static Dictionary<string, System.Drawing.SizeF> m_stringSizeCache = new Dictionary<string, System.Drawing.SizeF>();
        private static System.Drawing.SizeF measureStringDX2D(string s, SharpDX.DirectWrite.TextFormat tf, bool cacheStringLength = false)
        {
            // keep cache of calced sizes as this is quite a slow process
            string key;

            if (cacheStringLength)
                key = s.Length.ToString() + tf.FontFamilyName + tf.FontSize.ToString();
            else
                key = s + tf.FontFamilyName + tf.FontSize.ToString();

            if (m_stringSizeCache.ContainsKey(key)) return m_stringSizeCache[key];

            SharpDX.DirectWrite.TextLayout layout = new SharpDX.DirectWrite.TextLayout(fontFactory, s, tf, float.PositiveInfinity, float.PositiveInfinity);
            System.Drawing.SizeF sz = new System.Drawing.SizeF(layout.Metrics.Width, layout.Metrics.Height);
            Utilities.Dispose(ref layout);
            layout = null;

            if (m_stringSizeCache.Count > 500) m_stringSizeCache.Remove(m_stringSizeCache.Keys.Last()); // dump oldest, as new one are inserted at the head of dictionary list
            m_stringSizeCache.Add(key, sz);

            return sz;
        }
        public static int CachedMeasureStringsCount
        {
            get { return m_stringSizeCache.Count; }
        }
        //--------------------------

        private static bool m_bAlwaysShowCursorInfo = false;
        public static bool AlwaysShowCursorInfo
        {
            get { return m_bAlwaysShowCursorInfo; }
            set { m_bAlwaysShowCursorInfo = value;}
        }
        private static string m_sMHzCursorDisplay = "";
        public static string MHzCursorDisplay
        {
            set { m_sMHzCursorDisplay = value; }
        }
        private static string m_sOtherData1CursorDisplay = "";
        public static string OtherData1CursorDisplay
        {
            set { m_sOtherData1CursorDisplay = value; }
        }
        private static string m_sOtherData2CursorDisplay = "";
        public static string OtherData2CursorDisplay
        {
            set { m_sOtherData2CursorDisplay = value; }
        }
        private static int getCWSideToneShift(int rx, DSPMode forceMode = DSPMode.FIRST)
        {
            int nRet = 0;
            DSPMode mode;

            if (forceMode != DSPMode.FIRST)
            {
                mode = forceMode;
            }
            else
            {
                mode = (rx == 1) ? rx1_dsp_mode : rx2_dsp_mode;
            }

            switch (mode)
            {
                case (DSPMode.CWL):
                    nRet = cw_pitch;
                    break;
                case (DSPMode.CWU):
                    nRet = -cw_pitch;
                    break;
            }

            return nRet;
        }
        private static void drawPanadapterAndWaterfallGridDX2D(int nVerticalShift, int W, int H, int rx, bool bottom, bool bIsWaterfall = false)
        {
            // MW0LGE
            // this now draws the grid for either panadapter or waterfall, pass in a bool to pick
            //
            DisplayLabelAlignment label_align = display_label_align;
            bool displayduplex = isRxDuplex(rx);
            bool local_mox = mox && ((rx == 1 && !tx_on_vfob) || (rx == 2 && tx_on_vfob) || (rx == 1 && tx_on_vfob && !console.RX2Enabled));
            int Low = 0;					// initialize variables
            int High = 0;
            int mid_w = W / 2;
            int[] step_list = { 10, 20, 25, 50 };
            int step_power = 1;
            int step_index = 0;
            int freq_step_size = 50;
            int inbetweenies = 5;
            int grid_max = 0;
            int grid_min = 0;
            int grid_step = 0; // spectrum_grid_step;
            int f_diff = 0;
            int sample_rate;

            #region setup
            //MW0LGE
            int cwSideToneShift = getCWSideToneShift(rx);
            int cwSideToneShiftInverted = cwSideToneShift * -1; // invert the sign as cw zero lines/tx lines etc are a shift in opposite direction to the grid

            if (rx == 1)
            {
                if (local_mox)
                {
                    if(displayduplex)
                    {
                        Low = rx_display_low;
                        High = rx_display_high;

                        sample_rate = sample_rate_rx1;
                    }
                    else
                    {
                        Low = tx_display_low;
                        High = tx_display_high;

                        sample_rate = sample_rate_tx;
                    }

                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    grid_step = tx_spectrum_grid_step;

                    label_align = tx_display_label_align;
                }
                else
                {
                    Low = rx_display_low;
                    High = rx_display_high;

                    grid_max = spectrum_grid_max;
                    grid_min = spectrum_grid_min;
                    grid_step = spectrum_grid_step;

                    sample_rate = sample_rate_rx1;
                }
                
                f_diff = freq_diff;
            }
            else// if (rx == 2)
            {
                if (local_mox) 
                {
                    if (displayduplex)
                    {
                        Low = tx_display_low;
                        High = tx_display_high;

                        sample_rate = sample_rate_tx;
                    }
                    else
                    {
                        Low = tx_display_low;
                        High = tx_display_high;

                        sample_rate = sample_rate_tx;
                    }

                    grid_max = tx_spectrum_grid_max;
                    grid_min = tx_spectrum_grid_min;
                    grid_step = tx_spectrum_grid_step;

                    label_align = tx_display_label_align;
                }
                else
                {
                    Low = rx2_display_low;
                    High = rx2_display_high;

                    grid_max = rx2_spectrum_grid_max;
                    grid_min = rx2_spectrum_grid_min;
                    grid_step = rx2_spectrum_grid_step;

                    sample_rate = sample_rate_rx2;
                }
                
                f_diff = rx2_freq_diff;
            }

            int y_range = grid_max - grid_min;
            if (split_display) grid_step *= 2;

            int filter_low, filter_high;
            int center_line_x;

            if (rx == 1)
            {
                if (local_mox)
                {
                    filter_low = tx_filter_low;
                    filter_high = tx_filter_high;
                }
                else
                {
                    filter_low = rx1_filter_low;
                    filter_high = rx1_filter_high;
                }
            }
            else// if (rx == 2)
            {
                if (local_mox)
                {
                    filter_low = tx_filter_low;
                    filter_high = tx_filter_high;
                }
                else
                {
                    filter_low = rx2_filter_low;
                    filter_high = rx2_filter_high;
                }
            }
            
            if ((rx1_dsp_mode == DSPMode.DRM && rx == 1) ||
                (rx2_dsp_mode == DSPMode.DRM && rx == 2))
            {
                filter_low = -6000;
                filter_high = 6000;
            }

            int width = High - Low;

            // Calculate horizontal step size
            while (width / freq_step_size > 10)
            {
                freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                step_index = (step_index + 1) % 4;
                if (step_index == 0) step_power++;
            }

            //MW0LGE
            // calculate vertical step size
            int h_steps = (grid_max - grid_min) / grid_step;
            int top;

            if (bIsWaterfall) top = 20; //change top so that the filter gap doesnt change, inore grid spacing
            else top = (int)((double)grid_step * H / y_range); // top is based on grid spacing
            #endregion

            #region RX filter and filter lines
            if (!local_mox && sub_rx1_enabled && rx == 1) //multi-rx
            {
                if ((bIsWaterfall && m_bShowRXFilterOnWaterfall) || !bIsWaterfall)
                {
                    // draw Sub RX filter
                    // get filter screen coordinates
                    int filter_left_x = (int)((float)(filter_low - Low + vfoa_sub_hz - vfoa_hz - rit_hz) / width * W);
                    int filter_right_x = (int)((float)(filter_high - Low + vfoa_sub_hz - vfoa_hz - rit_hz) / width * W);

                    drawFilterOverlayDX2D(m_bDX2_sub_rx_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
                }

                if ((bIsWaterfall && m_bShowRXZeroLineOnWaterfall) || !bIsWaterfall)
                {
                    // draw Sub RX 0Hz line
                    int x = (int)((float)(vfoa_sub_hz - vfoa_hz - Low) / width * W);
                    drawLineDX2D(m_bDX2_sub_rx_zero_line_pen, x, nVerticalShift + top, x, nVerticalShift + H, 2);
                }
            }

            // RX FILTER overlay + highlight edges
            if ((bIsWaterfall && m_bShowRXFilterOnWaterfall) || !bIsWaterfall)
            {
                if (!local_mox)// && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                {
                    // draw RX filter
                    int filter_left_x = (int)((float)(filter_low - Low - f_diff) / width * W);
                    int filter_right_x = (int)((float)(filter_high - Low - f_diff) / width * W);

                    drawFilterOverlayDX2D(m_bDX2_display_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);

                    if (!bIsWaterfall)
                    {
                        int nFilterEdge = 0;

                        if (rx == 1)
                            nFilterEdge = m_nHightlightFilterEdgeRX1;
                        else if (rx == 2)
                            nFilterEdge = m_nHightlightFilterEdgeRX2;

                        switch (nFilterEdge)
                        {
                            case -1:
                                drawLineDX2D(m_bDX2_cw_zero_pen, filter_left_x, nVerticalShift + top, filter_left_x, nVerticalShift + H, 2);
                                break;
                            case 1:
                                drawLineDX2D(m_bDX2_cw_zero_pen, filter_right_x, nVerticalShift + top, filter_right_x, nVerticalShift + H, 2);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            //
            #endregion

            #region Tx filter and tx lines

            //MW0LGE_21k8 reworked
            if(rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU)
            {
                if ((bIsWaterfall && m_bShowTXFilterOnRXWaterfall/*m_bShowTXFilterOnWaterfall*/) || !bIsWaterfall)
                {
                    int filter_left_x;
                    int filter_right_x;
                    int filter_low_tmp;
                    int filter_high_tmp;

                    if (local_mox)
                    {
                        filter_low_tmp = filter_low;
                        filter_high_tmp = filter_high;
                    }
                    else
                    {
                        filter_low_tmp = tx_filter_low;
                        filter_high_tmp = tx_filter_high;
                    }

                    if (!split_enabled)
                    {
                        filter_left_x = (int)((float)(filter_low_tmp - Low - f_diff + xit_hz) / width * W);
                        filter_right_x = (int)((float)(filter_high_tmp - Low - f_diff + xit_hz) / width * W);
                    }
                    else // MW0LGE_21k8
                    {
                        filter_left_x = (int)((float)(filter_low_tmp - Low + xit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
                        filter_right_x = (int)((float)(filter_high_tmp - Low + xit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
                    }

                    if (local_mox)
                    {
                        drawFilterOverlayDX2D(m_bDX2_tx_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
                    }
                    else if(draw_tx_filter)
                    {
                        if ((rx == 2 && tx_on_vfob) || (rx == 1 && !(tx_on_vfob && rx2_enabled)))
                        {
                            drawLineDX2D(m_bDX2_tx_filter_pen, filter_left_x, nVerticalShift + top, filter_left_x, nVerticalShift + H, tx_filter_pen.Width);
                            drawLineDX2D(m_bDX2_tx_filter_pen, filter_right_x, nVerticalShift + top, filter_right_x, nVerticalShift + H, tx_filter_pen.Width);
                        }
                    }
                }
            }

            ////if ((bIsWaterfall && m_bShowTXFilterOnWaterfall) || !bIsWaterfall)
            ////{
            ////    if (local_mox && (rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU))
            ////    {
            ////        // draw TX filter
            ////        int filter_left_x;
            ////        int filter_right_x;

            ////        if (!split_enabled)
            ////        {
            ////            filter_left_x = (int)((float)(filter_low - Low - f_diff + xit_hz) / width * W);
            ////            filter_right_x = (int)((float)(filter_high - Low - f_diff + xit_hz) / width * W);
            ////        }
            ////        else // MW0LGE_21k8
            ////        {
            ////            filter_left_x = (int)((float)(filter_low - Low + xit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
            ////            filter_right_x = (int)((float)(filter_high - Low + xit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
            ////        }

            ////        drawFilterOverlayDX2D(m_bDX2_tx_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
            ////    }
            ////}

            ////if ((!bIsWaterfall || (bIsWaterfall && m_bShowTXFilterOnRXWaterfall)) && //MW0LGE
            ////    !/*mox*/local_mox && draw_tx_filter &&
            ////    (rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU))
            ////{
            ////    // get tx filter limits
            ////    int filter_left_x;
            ////    int filter_right_x;

            ////    if (tx_on_vfob)
            ////    {
            ////        if (!split_enabled)
            ////        {
            ////            // MW0LGE - f_diff
            ////            filter_left_x = (int)((float)(tx_filter_low - Low - f_diff + xit_hz - rit_hz) / width * W);
            ////            filter_right_x = (int)((float)(tx_filter_high - Low - f_diff + xit_hz - rit_hz) / width * W);
            ////        }
            ////        else
            ////        {
            ////            filter_left_x = (int)((float)(tx_filter_low - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
            ////            filter_right_x = (int)((float)(tx_filter_high - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
            ////        }
            ////    }
            ////    else
            ////    {
            ////        if (!split_enabled)
            ////        {
            ////            filter_left_x = (int)((float)(tx_filter_low - Low - f_diff + xit_hz - rit_hz) / width * W);
            ////            filter_right_x = (int)((float)(tx_filter_high - Low - f_diff + xit_hz - rit_hz) / width * W);
            ////        }
            ////        else
            ////        {
            ////            filter_left_x = (int)((float)(tx_filter_low - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
            ////            filter_right_x = (int)((float)(tx_filter_high - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
            ////        }
            ////    }

            ////    if ((rx == 2 && tx_on_vfob) || (rx == 1 && !(tx_on_vfob && rx2_enabled)))
            ////    {
            ////        drawLineDX2D(m_bDX2_tx_filter_pen, filter_left_x, nVerticalShift + top, filter_left_x, nVerticalShift + H, tx_filter_pen.Width);        // draw tx filter overlay
            ////        drawLineDX2D(m_bDX2_tx_filter_pen, filter_right_x, nVerticalShift + top, filter_right_x, nVerticalShift + H, tx_filter_pen.Width);	// draw tx filter overlay
            ////    }
            ////}

            //if (rx == 1 && !local_mox /* && !tx_on_vfob */ && draw_tx_cw_freq &&
            //    (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
            //{
            //    //int pitch = cw_pitch;
            //    //if (rx1_dsp_mode == DSPMode.CWL)
            //    //    pitch = -cw_pitch;

            //    int cw_line_x;
            //    if (!split_enabled)
            //        cw_line_x = (int)((float)(cwSideToneShiftInverted - Low - f_diff + xit_hz - rit_hz) / width * W);
            //    else
            //        cw_line_x = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

            //    drawLineDX2D(m_bDX2_tx_filter_pen, cw_line_x, nVerticalShift + top, cw_line_x, nVerticalShift + H, tx_filter_pen.Width); //MW0LGE
            //}

            //if (rx==2 && !local_mox && tx_on_vfob && draw_tx_cw_freq &&
            //    (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
            //{
            //    //int pitch = cw_pitch;
            //    //if (rx2_dsp_mode == DSPMode.CWL)
            //    //    pitch = -cw_pitch;

            //    int cw_line_x;
            //    if (!split_enabled)
            //        cw_line_x = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz) / width * W);
            //    else
            //        cw_line_x = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

            //    drawLineDX2D(m_bDX2_tx_filter_pen, cw_line_x, nVerticalShift + top, cw_line_x, nVerticalShift + H, tx_filter_pen.Width); //MW0LGE
            //}
            #endregion

            #region 60m channels
            // draw 60m channels if in view - not on the waterfall //MW0LGE
            if (!bIsWaterfall && (console.CurrentRegion == FRSRegion.US || console.CurrentRegion == FRSRegion.UK))
            {
                foreach (Channel c in Console.Channels60m)
                {
                    long rf_freq = vfoa_hz;
                    int rit = rit_hz;
                    if (local_mox) rit = 0;

                    if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) // MW0LGE
                    {
                        rf_freq = vfob_hz;
                    }

                    if (c.InBW((rf_freq + Low) * 1e-6, (rf_freq + High) * 1e-6)) // is channel visible?
                    {
                        bool on_channel = console.RX1IsIn60mChannel(c); // only true if you are on channel and are in an acceptable mode
                        DSPMode mode = rx1_dsp_mode;

                        if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))
                        {
                            on_channel = console.RX2IsIn60mChannel(c);
                            mode = rx2_dsp_mode;
                        }

                        switch (mode)
                        {
                            case DSPMode.USB:
                            case DSPMode.DIGU:
                            case DSPMode.CWL:
                            case DSPMode.CWU:
                            case DSPMode.AM:
                            case DSPMode.SAM:
                                break;
                            default:
                                on_channel = false; // make sure other modes do not look as if they could transmit
                                break;
                        }

                        // offset for CW Pitch to align display
                        rf_freq += cwSideToneShift;

                        int chan_left_x = (int)((float)(c.Freq * 1e6 - rf_freq - c.BW / 2 - Low - rit) / width * W);
                        int chan_right_x = (int)((float)(c.Freq * 1e6 - rf_freq + c.BW / 2 - Low - rit) / width * W);

                        if (chan_right_x == chan_left_x)
                            chan_right_x = chan_left_x + 1;

                        // decide colors to draw notch
                        Color c1 = on_channel ? channel_background_on : channel_background_off;
                        Color c2 = channel_foreground;

                        //MW0LGE
                        drawChannelBarDX2D(c, chan_left_x, chan_right_x, nVerticalShift + top, H - top, c1, c2);
                    }
                }
            }
            #endregion

            //MW0LGE_21h
            #region BandStackOverlay
            if(m_bShowBandStackOverlays && m_bandStackOverlays != null && rx == 1 && !local_mox && !bIsWaterfall )
            {
                long rf_freq = vfoa_hz;
                int rit = rit_hz;

                SharpDX.Direct2D1.Brush brush;
                for (int n = 0; n < m_bandStackOverlays.Length; n++)
                {
                    int filter_left_x = (int)((float)((((m_bandStackOverlays[n].Frequency * 1e6) - rf_freq) + m_bandStackOverlays[n].LowFilter) - Low - rit) / width * W);
                    int filter_right_x = (int)((float)((((m_bandStackOverlays[n].Frequency * 1e6) - rf_freq) + m_bandStackOverlays[n].HighFilter) - Low - rit) / width * W);

                    brush = (n == m_nHighlightedBandStackEntryIndex) ? m_bDX2_bandstack_overlay_brush_highlight: m_bDX2_bandstack_overlay_brush;

                    // filled rect
                    drawFilterOverlayDX2D(brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);

                    // line either side
                    drawLineDX2D(m_bDX2_bandstack_overlay_brush_lines, filter_left_x, nVerticalShift + top, filter_left_x, nVerticalShift + H, 2);
                    drawLineDX2D(m_bDX2_bandstack_overlay_brush_lines, filter_right_x, nVerticalShift + top, filter_right_x, nVerticalShift + H, 2);
                }
            }
            #endregion

            #region notches
            // draw notches if in RX
            if (!local_mox && !bIsWaterfall)
            {
                long rf_freq = vfoa_hz;
                int rit = rit_hz;

                if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) // MW0LGE
                {
                    rf_freq = vfob_hz;
                }

                rf_freq += cwSideToneShift;

                SharpDX.Direct2D1.Brush p;
                SharpDX.Direct2D1.Brush b;
                SharpDX.Direct2D1.Brush t;

                List<MNotch> notches = MNotchDB.NotchesInBW(rf_freq, Low, High);

                foreach (MNotch n in notches)
                {

                    int notch_centre_x = (int)((float)((n.FCenter) - rf_freq - Low - rit) / width * W);
                    int notch_left_x = (int)((float)((n.FCenter) - rf_freq - n.FWidth / 2 - Low - rit) / width * W);
                    int notch_right_x = (int)((float)((n.FCenter) - rf_freq + n.FWidth / 2 - Low - rit) / width * W);

                    if (tnf_active)
                    {
                        if (n.Active)
                        {
                            p = m_bDX2_m_pNotchActive;
                            b = m_bDX2_m_bBWFillColour;
                        }
                        else
                        {
                            p = m_bDX2_m_pNotchInactive;
                            b = m_bDX2_m_bBWFillColourInactive;
                        }
                    }
                    else
                    {
                        p = m_bDX2_m_pTNFInactive;
                        b = m_bDX2_m_bTNFInactive;
                    }

                    //overide if highlighed
                    if (n == m_objHightlightedNotch)
                    {
                        if (n.Active)
                        {
                            t = m_bDX2_m_bTextCallOutActive;
                        }
                        else
                        {
                            t = m_bDX2_m_bTextCallOutInactive;
                        }
                        p = m_bDX2_m_pHighlighted;
                        b = m_bDX2_m_bBWHighlighedFillColour;

                        //display text callout info 1/4 the way down the notch when being highlighted
                        //TODO: check right edge of screen, put on left of notch if no room
                        string temp_text = ((n.FCenter) / 1e6).ToString("f6") + "MHz";
                        int nTmp = temp_text.IndexOf(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) + 4;
                        
                        drawStringDX2D("F: " + temp_text.Insert(nTmp, " "), fontDX2d_callout, t, notch_right_x + 4, nVerticalShift + top + (H / 4));
                        drawStringDX2D("W: " + n.FWidth.ToString("f0") + "Hz", fontDX2d_callout, t, notch_right_x + 4, nVerticalShift + top + (H / 4) + 12);
                    }

                    // the middle notch line
                    drawLineDX2D(p, notch_centre_x, nVerticalShift + top, notch_centre_x, nVerticalShift + H);

                    // only draw area fill if wide enough
                    if (notch_left_x != notch_right_x)
                    {
                        drawFillRectangleDX2D(b, notch_left_x, nVerticalShift + top, notch_right_x - notch_left_x, H - top);
                    }
                }
            }// END NOTCH
            #endregion

            #region CW zero and tx lines
            // Draw a CW Zero Beat + TX line on CW filter
            if (!bIsWaterfall)
            {
                if (show_cwzero_line)
                {
                    if (rx == 1 && !local_mox &&
                        (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        int cw_line_x1;
                        cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff) / width * W);

                        drawLineDX2D(m_bDX2_cw_zero_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H, cw_zero_pen.Width);
                    }

                    if (rx == 2 && !local_mox &&
                        (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        int cw_line_x1;
                        cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff) / width * W);

                        drawLineDX2D(m_bDX2_cw_zero_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H, cw_zero_pen.Width);
                    }
                }
                if (draw_tx_cw_freq)
                {
                    if (rx == 1 && !local_mox &&
                        (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        int cw_line_x1;
                        if (!split_enabled)
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff + xit_hz - rit_hz) / width * W);
                        else
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

                        drawLineDX2D(m_bDX2_tx_filter_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H, tx_filter_pen.Width);
                    }

                    if (rx == 2 && !local_mox &&
                        (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        int cw_line_x1;
                        if (!split_enabled)
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff + xit_hz - rit_hz) / width * W);
                        else
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

                        drawLineDX2D(m_bDX2_tx_filter_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H, tx_filter_pen.Width);
                    }
                }
            }
            #endregion

            //      MW0LGE
            if (local_mox)
            {
                if (!split_enabled) //MW0LGE_21k8
                {
                    center_line_x = (int)((float)(-f_diff - Low + xit_hz) / width * W); // locked 0 line
                }
                else
                {
                    center_line_x = (int)((float)(-Low + xit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W); // locked 0 line
                }
            }
            else
                center_line_x = (int)((float)(-f_diff - Low) / width * W); // locked 0 line

            // Draw 0Hz vertical line if visible
            if ((!bIsWaterfall && show_zero_line) |
                (bIsWaterfall && ((m_bShowRXZeroLineOnWaterfall & !local_mox) || (m_bShowTXZeroLineOnWaterfall & local_mox)))) // MW0LGE
            {
                if (center_line_x >= 0 && center_line_x <= W)
                {
                    float pw = local_mox ? tx_grid_zero_pen.Width : grid_zero_pen.Width;
                    SharpDX.Direct2D1.Brush pnPen = local_mox ? m_bDX2_tx_grid_zero_pen : m_bDX2_grid_zero_pen;

                    drawLineDX2D(pnPen, center_line_x, nVerticalShift + top, center_line_x, nVerticalShift + H, pw);
                }
            }

            if (show_freq_offset)
            {
                SharpDX.Direct2D1.Brush brBrush = local_mox ? m_bDX2_tx_grid_zero_pen : m_bDX2_grid_zero_pen;

                drawStringDX2D("0", fontDX2d_font9, brBrush, center_line_x - 5, nVerticalShift + 4/*(float)Math.Floor(H * .01)*/);
            }

            #region Band edges, H+V lines and labels
            //MW0LGE
            int[] band_edge_list;
            switch (console.CurrentRegion)
            {
                case FRSRegion.US:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
            5330500, 5406400, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Germany:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000,
            5351500, 5366500, 7000000, 7200000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 51000000, 144000000, 146000000 };
                    break;
                case FRSRegion.Region1:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000,
            5351500, 5366500, 7000000, 7200000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 146000000 };
                    break;
                case FRSRegion.Region2:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
            5351500, 5366500, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Region3:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 3900000,
            7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Spain:
                    band_edge_list = new int[] { 135700, 137800, 472000, 479000, 1810000, 1850000, 3500000, 3800000,
            7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000,
            21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 52000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Australia:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 1875000,
             3500000, 3800000, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000,
             18168000, 21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.UK:
                    band_edge_list = new int[] { 135700, 137800, 472000, 479000, 1810000, 2000000, 3500000, 3800000,
            5258500, 5406500, 7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000,
            21000000, 21450000, 24890000, 24990000, 28000000, 29700000, 50000000, 52000000, 144000000, 148000000 };
                    break;
                case FRSRegion.India:
                    band_edge_list = new int[]{ 1810000, 1860000, 3500000, 3900000, 7000000, 7200000,
            10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Norway:
                    band_edge_list = new int[]{ 1800000, 2000000, 3500000, 4000000, 5260000, 5410000,
            7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.Japan:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 1825000, 1907500, 1912500,
                        3500000, 3575000, 3599000, 3612000, 3680000, 3687000, 3702000, 3716000, 3745000, 3770000, 3791000, 3805000,
            7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 146000000 };
                    break;
                default: // same as region3 but with extended 80m out to 4mhz
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
            7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
            24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
            }
            //--

            double vfo;

            if (rx == 1)
            {
                if (local_mox)
                {
                    //if (split_enabled) vfo = vfoa_sub_hz;//MW0LGE_21k8
                    if (split_enabled) vfo = vfoa_sub_hz - (vfoa_sub_hz - vfoa_hz);
                    else vfo = vfoa_hz;
                    vfo += xit_hz;
                }
                else if (/*mox*/local_mox && tx_on_vfob)
                {
                    if (console.RX2Enabled) vfo = vfoa_hz + rit_hz;
                    else vfo = vfoa_sub_hz;
                }
                else vfo = vfoa_hz + rit_hz;
            }
            else //if(rx==2)
            {
                if (local_mox)
                    vfo = vfob_hz + xit_hz;
                else
                {
                    if (console.VFOSync) vfo = vfob_hz + rit_hz;
                    else vfo = vfob_hz;
                }
            }

            vfo += cwSideToneShift;

            long vfo_round = ((long)(vfo / freq_step_size)) * freq_step_size;
            long vfo_delta = (long)(vfo - vfo_round);

            int f_steps = (width / freq_step_size) + 1;

            // Draw vertical lines - band edge markers and freq text
            SharpDX.Direct2D1.Brush pnMajorLine;
            SharpDX.Direct2D1.Brush pnInbetweenLine;
            SharpDX.Direct2D1.Brush brTextBrush;

            for (int i = -1; i < f_steps + 1; i++) // MW0LGE was from i=0, fixes inbetweenies not drawn if major is < 0
            {
                string label;
                int offsetL;
                int offsetR;
                int fgrid = i * freq_step_size + (Low / freq_step_size) * freq_step_size;
                double actual_fgrid = ((double)(vfo_round + fgrid)) / 1000000;
                int vgrid = (int)((double)(fgrid - vfo_delta - Low) / width * W);
                //string freq_num = actual_fgrid.ToString();

                bool bBandEdge = false;

                if (!show_freq_offset)
                {

                    //--------------
                    //MW0LGE
                    for (int ii = 0; ii < band_edge_list.Length; ii++)
                    {
                        if (actual_fgrid == (double)band_edge_list[ii] / 1000000)
                        {
                            bBandEdge = true;
                            break;
                        }
                    }

                    if (bBandEdge)
                    {
                        if (local_mox)
                        {
                            pnMajorLine = m_bDX2_tx_band_edge_pen;
                            pnInbetweenLine = m_bDX2_tx_vgrid_pen_inb;
                            brTextBrush = m_bDX2_tx_band_edge_pen;
                        }
                        else
                        {
                            pnMajorLine = m_bDX2_band_edge_pen;
                            pnInbetweenLine = m_bDX2_grid_pen_inb;
                            brTextBrush = m_bDX2_band_edge_pen;
                        }
                    }
                    else
                    {
                        if (local_mox)
                        {
                            pnMajorLine = m_bDX2_tx_vgrid_pen;
                            pnInbetweenLine = m_bDX2_tx_vgrid_pen_inb;
                            brTextBrush = m_bDX2_grid_tx_text_brush;
                        }
                        else
                        {
                            pnMajorLine = m_bDX2_grid_pen;
                            pnInbetweenLine = m_bDX2_grid_pen_inb;
                            brTextBrush = m_bDX2_grid_text_brush;
                        }
                    }
                    //--

                    //draw vertical in between lines
                    if (grid_control && !bIsWaterfall)
                    {
                        drawLineDX2D(pnMajorLine, vgrid, nVerticalShift + top, vgrid, nVerticalShift + H);

                        int fgrid_2 = ((i + 1) * freq_step_size) + (int)((Low / freq_step_size) * freq_step_size);
                        int x_2 = (int)(((float)(fgrid_2 - vfo_delta - Low) / width * W));
                        float scale = (float)(x_2 - vgrid) / inbetweenies;

                        for (int j = 1; j < inbetweenies; j++)
                        {
                            float x3 = (float)vgrid + (j * scale);

                            drawLineDX2D(pnInbetweenLine, x3, nVerticalShift + top, x3, nVerticalShift + H);
                        }
                    }

                    if (((double)((int)(actual_fgrid * 1000))) == actual_fgrid * 1000)
                    {
                        label = actual_fgrid.ToString("f3");
                        if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                        else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                        else offsetL = (int)((label.Length + 1) * 4.1) - 8;
                    }
                    else
                    {
                        //display freqencies
                        int jper;
                        label = actual_fgrid.ToString("f4");
                        jper = label.IndexOf('.') + 4;
                        label = label.Insert(jper, " ");

                        if (actual_fgrid < 10) offsetL = (int)((label.Length) * 4.1) - 14;
                        else if (actual_fgrid < 100.0) offsetL = (int)((label.Length) * 4.1) - 11;
                        else offsetL = (int)((label.Length) * 4.1) - 8;
                    }

                    drawStringDX2D(label, fontDX2d_font9, brTextBrush, vgrid - offsetL, nVerticalShift + 4/*(float)Math.Floor(H * .01)*/);
                    //--------------
                }
                else
                {
                    vgrid = Convert.ToInt32((double)-(fgrid - Low) / (Low - High) * W);	//wa6ahl

                    if (!bIsWaterfall)
                    {
                        SharpDX.Direct2D1.Brush pnPen = local_mox ? m_bDX2_tx_vgrid_pen : m_bDX2_grid_pen;

                        drawLineDX2D(pnPen, vgrid, nVerticalShift + top, vgrid, nVerticalShift + H);
                    }

                    label = fgrid.ToString();
                    offsetL = (int)((label.Length + 1) * 4.1);
                    offsetR = (int)(label.Length * 4.1);
                    if ((vgrid - offsetL >= 0) && (vgrid + offsetR < W) && (fgrid != 0))
                    {
                        SharpDX.Direct2D1.Brush brBrush = local_mox ? m_bDX2_grid_tx_text_brush : m_bDX2_grid_text_brush;

                        drawStringDX2D(label, fontDX2d_font9, brBrush, vgrid - offsetL, nVerticalShift + 4/*(float)Math.Floor(H * .01)*/);
                    }
                }
            }

            if (!bIsWaterfall)
            {
                // This block of code draws any band edge lines that might not be shown
                // because of the stepped nature of the code above

                //--------------
                //MW0LGE

                if (local_mox)
                {
                    pnMajorLine = m_bDX2_tx_band_edge_pen;
                }
                else
                {
                    pnMajorLine = m_bDX2_band_edge_pen;
                }
                //--

                for (int i = 0; i < band_edge_list.Length; i++)
                {
                    double band_edge_offset = band_edge_list[i] - vfo;
                    if (band_edge_offset >= Low && band_edge_offset <= High)
                    {
                        int temp_vline = (int)((double)(band_edge_offset - Low) / width * W);//wa6ahl

                        //MW0LGE
                        drawLineDX2D(pnMajorLine, temp_vline, nVerticalShift + top, temp_vline, nVerticalShift + H);
                    }
                }
            }
            //--

            if (grid_control && !bIsWaterfall)
            {
                SharpDX.Direct2D1.Brush brTextLabel;

                // highlight the number/scales
                if ((m_bHighlightNumberScaleRX1 && rx == 1) || (m_bHighlightNumberScaleRX2 && rx == 2))
                {
                    if (rx == 1)
                    {
                        drawFillRectangleDX2D(m_bDX2_m_bHightlightNumberScale, console.RX1DisplayGridX, nVerticalShift + top, console.RX1DisplayGridW - console.RX1DisplayGridX, H - top);
                    }
                    else
                    {
                        drawFillRectangleDX2D(m_bDX2_m_bHightlightNumberScale, console.RX2DisplayGridX, nVerticalShift + top, console.RX2DisplayGridW - console.RX2DisplayGridX, H - top);

                    }
                    brTextLabel = m_bDX2_m_bHightlightNumbers;
                }
                else
                {
                    if (local_mox)
                    {
                        brTextLabel = m_bDX2_grid_tx_text_brush;
                    }
                    else
                    {
                        brTextLabel = m_bDX2_grid_text_brush;
                    }

                }
                // Draw horizontal lines
                int nLeft = 0;
                int nRight = 0;
                int nW = (int)measureStringDX2D("-999", fontDX2d_font9).Width + 12;

                switch (label_align)//MW0LGE display_label_align)
                {
                    case DisplayLabelAlignment.LEFT:
                        nLeft = 0;
                        nRight = nW;
                        break;
                    case DisplayLabelAlignment.CENTER:
                        if (rx == 1 && (rx1_dsp_mode == DSPMode.USB || rx1_dsp_mode == DSPMode.DIGU || rx1_dsp_mode == DSPMode.CWU))
                        {
                            nLeft = center_line_x - nW;
                            nRight = nLeft + nW;
                        }
                        else if (rx == 2 && (rx2_dsp_mode == DSPMode.USB || rx2_dsp_mode == DSPMode.DIGU || rx2_dsp_mode == DSPMode.CWU))
                        {
                            nLeft = center_line_x - nW;
                            nRight = nLeft + nW;
                        }
                        else {
                            nLeft = center_line_x;
                            nRight = nLeft + nW;
                        }
                        break;
                    case DisplayLabelAlignment.RIGHT:
                        nLeft = W - nW;
                        nRight = W;
                        break;
                    case DisplayLabelAlignment.AUTO:
                        nLeft = 0;
                        nRight = nW;
                        break;
                    case DisplayLabelAlignment.OFF:
                        nLeft = W;
                        nRight = W + nW;
                        break;
                }

                for (int i = 1; i < h_steps; i++)
                {
                    int xOffset = 0;
                    int num = grid_max - i * grid_step;
                    int y = (int)((double)(grid_max - num) * H / y_range);

                    // MW0LGE
                    drawLineDX2D(local_mox ? m_bDX2_tx_hgrid_pen : m_bDX2_hgrid_pen, 0, nVerticalShift + y, W, nVerticalShift + y);

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        num = grid_max - i * grid_step;
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)measureStringDX2D("0", fontDX2d_font9).Width;// use 0 here instead of a - sign
                        SizeF size = measureStringDX2D(label, fontDX2d_font9);

                        int x = 0;
                        switch (label_align)//MW0LGE display_label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                if(rx == 1 && (rx1_dsp_mode==DSPMode.USB || rx1_dsp_mode == DSPMode.DIGU || rx1_dsp_mode == DSPMode.CWU))
                                {
                                    x = center_line_x - xOffset - (int)size.Width;
                                }
                                else if (rx == 2 && (rx2_dsp_mode == DSPMode.USB || rx2_dsp_mode == DSPMode.DIGU || rx2_dsp_mode == DSPMode.CWU))
                                {
                                    x = center_line_x - xOffset - (int)size.Width;
                                }
                                else x = center_line_x + xOffset;
                                break;
                            case DisplayLabelAlignment.RIGHT:
                                x = (int)(W - size.Width - 3);
                                break;
                            case DisplayLabelAlignment.AUTO:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.OFF:
                                x = W;
                                break;
                        }
                        y -= 8;
                        if (y + 9 < H)
                        {
                            drawStringDX2D(label, fontDX2d_font9, brTextLabel, x, nVerticalShift + y);
                        }
                    }
                }

                // assign back to console so that it knows where we need to be mouse over
                if (rx == 1)
                {
                    console.RX1DisplayGridX = nLeft;
                    console.RX1DisplayGridW = nRight;
                }
                else
                {
                    console.RX2DisplayGridX = nLeft;
                    console.RX2DisplayGridW = nRight;
                }
            }
            #endregion

            #region long cursor and right click overlay
            // draw long cursor & filter overlay
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                bool bShow;

                SharpDX.Direct2D1.Brush p;
                // if we are sub tx then the cross will be red
                p = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_pen : m_bDX2_Red;

                int y1 = nVerticalShift + top;
                int y2 = H;

                if (rx == 1)
                {
                    if (rx2_enabled)
                    {
                        bShow = (current_display_mode == DisplayMode.PANAFALL) ? display_cursor_y <= 2 * H : display_cursor_y <= H;
                    }
                    else
                    {
                        bShow = true;
                    }
                }
                else
                {
                    bShow = (current_display_mode_bottom == DisplayMode.PANAFALL) ? display_cursor_y > 2 * H : display_cursor_y > H;
                }

                if (bShow)
                {
                    double freq_low = freq + filter_low;
                    double freq_high = freq + filter_high;
                    int x1 = (int)((freq_low - Low) / width * W);
                    int x2 = (int)((freq_high - Low) / width * W);

                    if (ClickTuneFilter)
                    { // only show filter if option set MW0LGE
                        if (((rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU) && rx == 1) || ((rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU) && rx == 2))
                        {
                            drawFillRectangleDX2D(m_bDX2_display_filter_brush, display_cursor_x -
                                ((x2 - x1) / 2), y1, x2 - x1, y2 - top);
                        }
                        else
                        {
                            drawFillRectangleDX2D(m_bDX2_display_filter_brush, x1, y1, x2 - x1, y2 - top);
                        }
                    }

                    drawLineDX2D(p, display_cursor_x, y1 - top, display_cursor_x, (y1 - top) + y2);

                    // draw horiz cursor line
                    if (ShowCTHLine) drawLineDX2D(p, 0, display_cursor_y, W, display_cursor_y);
                }               
            }
            #endregion

            #region F/G/H line and grabs
            // MW0LGE all the code for F/G/H overlay line/grab boxes
            if (!bIsWaterfall && !local_mox)
            {
                //MW0LGE include bottom check
                if (console.PowerOn && (((current_display_mode == DisplayMode.PANADAPTER ||
                    current_display_mode == DisplayMode.PANAFALL ||
                    current_display_mode == DisplayMode.PANASCOPE) && rx == 1)
                    || ((current_display_mode_bottom == DisplayMode.PANADAPTER ||
                    current_display_mode_bottom == DisplayMode.PANAFALL ||
                    current_display_mode_bottom == DisplayMode.PANASCOPE) && rx == 2)))
                {
                    // get filter screen coordinates
                    int filter_left_x = (int)((float)(filter_low - Low) / width * W);
                    int filter_right_x = (int)((float)(filter_high - Low) / width * W);
                    if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

                    int x1_rx1_gain = 0, x2_rx1_gain = 0, x3_rx1_gain = 0, x1_rx1_hang = 0, x2_rx1_hang = 0, x3_rx1_hang = 0;
                    int x1_rx2_gain = 0, x2_rx2_gain = 0, x3_rx2_gain = 0, x1_rx2_hang = 0, x2_rx2_hang = 0, x3_rx2_hang = 0;

                    if (rx == 1)
                    {
                        if (spectrum_line)
                        {
                            x1_rx1_gain = 40;
                            x2_rx1_gain = W - 40;
                            x3_rx1_gain = 50;
                        }
                        else
                        {
                            x1_rx1_gain = filter_left_x;
                            x2_rx1_gain = filter_right_x;
                            x3_rx1_gain = x1_rx1_gain;
                        }

                        if (rx1_hang_spectrum_line)
                        {
                            x1_rx1_hang = 40;
                            x2_rx1_hang = W - 40;
                            x3_rx1_hang = 50;
                        }
                        else
                        {
                            x1_rx1_hang = filter_left_x;
                            x2_rx1_hang = filter_right_x;
                            x3_rx1_hang = x1_rx1_hang;
                        }
                    }
                    else
                    {
                        if (rx2_gain_spectrum_line)
                        {
                            x1_rx2_gain = 40;
                            x2_rx2_gain = W - 40;
                            x3_rx2_gain = 50;
                        }
                        else
                        {
                            x1_rx2_gain = filter_left_x;
                            x2_rx2_gain = filter_right_x;
                            x3_rx2_gain = x1_rx2_gain;
                        }

                        if (rx2_hang_spectrum_line)
                        {
                            x1_rx2_hang = 40;
                            x2_rx2_hang = W - 40;
                            x3_rx2_hang = 50;
                        }
                        else
                        {
                            x1_rx2_hang = filter_left_x;
                            x2_rx2_hang = filter_right_x;
                            x3_rx2_hang = x1_rx2_hang;
                        }
                    }

                    if (rx == 1/* && !local_mox*/)
                    {
                        float rx1_cal_offset = 0.0f;
                        switch (console.RX1AGCMode)
                        {
                            case AGCMode.FIXD:
                                rx1_cal_offset = -18.0f;
                                break;
                            default:
                                rx1_cal_offset = 2.0f + (rx1_display_cal_offset +
                                    (rx1_preamp_offset - alex_preamp_offset) - rx1_fft_size_offset);
                                break;
                        }
                        // get AGC-T level
                        // get Hang Threshold level
                        double rx1_thresh = 0.0;
                        float rx1_agcknee_y_value = 0.0f;
                        double rx1_hang = 0.0;
                        float rx1_agc_hang_y = 0.0f;

                        unsafe {
                            double size = (double)console.specRX.GetSpecRX(0).FFTSize;//MW0LGE_21k7
                            WDSP.GetRXAAGCThresh(WDSP.id(0, 0), &rx1_thresh, size/*4096.0*/, sample_rate);
                            WDSP.GetRXAAGCHangLevel(WDSP.id(0, 0), &rx1_hang);
                        }
                        rx1_thresh = Math.Round(rx1_thresh);
                        int rx1_agc_fixed_gain = console.SetupForm.AGCFixedGain;
                        string rx1_agc = "";
                        switch (console.RX1AGCMode)
                        {
                            case AGCMode.FIXD:
                                rx1_agcknee_y_value = dBToPixel(-(float)rx1_agc_fixed_gain + rx1_cal_offset, H);
                                // Debug.WriteLine("agcknee_y_D:" + agcknee_y_value);
                                rx1_agc = m_bAutoAGCRX1 ? "-Fa" : "-F";
                                break;
                            default:
                                rx1_agcknee_y_value = dBToPixel((float)rx1_thresh + rx1_cal_offset, H);
                                rx1_agc_hang_y = dBToPixel((float)rx1_hang + rx1_cal_offset, H);

                                //if (console.RX2Enabled && CurrentDisplayMode == DisplayMode.PANAFALL)
                                //    rx1_agc_hang_y = rx1_agc_hang_y / 4;
                                //else if (console.RX2Enabled || split_display)
                                //    rx1_agc_hang_y = rx1_agc_hang_y / 2;

                                rx1_agc_hang_y += nVerticalShift;

                                //show hang line
                                if (display_agc_hang_line && console.RX1AGCMode != AGCMode.MED && console.RX1AGCMode != AGCMode.FAST)
                                {
                                    AGCHang.Height = 8; AGCHang.Width = 8; AGCHang.X = 40;
                                    AGCHang.Y = (int)rx1_agc_hang_y - AGCHang.Height;
                                    drawFillRectangleDX2D(m_bDX2_Yellow, AGCHang);
                                    drawLineDX2D(m_bDX2_Yellow, x3_rx1_hang, rx1_agc_hang_y, x2_rx1_hang, rx1_agc_hang_y, m_styleDots);
                                    drawStringDX2D("-H", fontDX2d_panafont, m_bDX2_pana_text_brush, AGCHang.X + AGCHang.Width, AGCHang.Y - (AGCHang.Height / 2));
                                }
                                rx1_agc = m_bAutoAGCRX1 ? "-Ga" : "-G";
                                break;
                        }

                        //if (console.RX2Enabled && CurrentDisplayMode == DisplayMode.PANAFALL)
                        //    rx1_agcknee_y_value = rx1_agcknee_y_value / 4;
                        //else if (console.RX2Enabled || split_display)
                        //    rx1_agcknee_y_value = rx1_agcknee_y_value / 2;

                        rx1_agcknee_y_value += nVerticalShift;

                        // show agc line
                        if (show_agc)
                        {
                            AGCKnee.Height = 8; AGCKnee.Width = 8; AGCKnee.X = 40;
                            AGCKnee.Y = (int)rx1_agcknee_y_value - AGCKnee.Height;
                            drawFillRectangleDX2D(m_bDX2_YellowGreen, AGCKnee);
                            drawLineDX2D(m_bDX2_YellowGreen, x1_rx1_gain, rx1_agcknee_y_value, x2_rx1_gain, rx1_agcknee_y_value, m_styleDots);
                            drawStringDX2D(rx1_agc, fontDX2d_panafont, m_bDX2_pana_text_brush, AGCKnee.X + AGCKnee.Width, AGCKnee.Y - (AGCKnee.Height / 2));
                        }
                    }
                    else// if (rx == 2/* && !local_mox*/)
                    {
                        float rx2_cal_offset = 0.0f;
                        double rx2_thresh = 0.0;
                        float rx2_agcknee_y_value = 0.0f;
                        double rx2_hang = 0.0;
                        float rx2_agc_hang_y = 0.0f;
                        string rx2_agc = "";
                        int rx2_agc_fixed_gain = 0;

                        switch (console.RX2AGCMode)
                        {
                            case AGCMode.FIXD:
                                rx2_cal_offset = -18.0f;
                                break;
                            default:
                                rx2_cal_offset = 2.0f + (rx2_display_cal_offset +
                                      rx2_preamp_offset) - rx2_fft_size_offset;
                                break;
                        }
                        unsafe
                        {
                            double size = (double)console.specRX.GetSpecRX(1).FFTSize;//MW0LGE_21k7
                            // get AGC-T level
                            WDSP.GetRXAAGCThresh(WDSP.id(2, 0), &rx2_thresh, size/*4096.0*/, sample_rate);
                            rx2_thresh = Math.Round(rx2_thresh);
                            WDSP.GetRXAAGCHangLevel(WDSP.id(2, 0), &rx2_hang);
                            rx2_agc_fixed_gain = console.SetupForm.AGCRX2FixedGain;
                        }
                        switch (console.RX2AGCMode)
                        {
                            case AGCMode.FIXD:
                                rx2_agcknee_y_value = dBToRX2Pixel(-(float)rx2_agc_fixed_gain + rx2_cal_offset, H);
                                // Debug.WriteLine("agcknee_y_D:" + agcknee_y_value);
                                rx2_agc = m_bAutoAGCRX2 ? "-Fa" : "-F";
                                break;
                            default:
                                rx2_agcknee_y_value = dBToRX2Pixel((float)rx2_thresh + rx2_cal_offset, H);
                                rx2_agc_hang_y = dBToRX2Pixel((float)rx2_hang + rx2_cal_offset, H);// + rx2_fft_size_offset);  MW0LGE   NOT IN RX1 WHY?  TODO CHECK

                                //if ((console.RX2Enabled || split_display) && current_display_mode_bottom != DisplayMode.PANAFALL)
                                //    rx2_agc_hang_y = rx2_agc_hang_y / 2;
                                //else
                                //    rx2_agc_hang_y = rx2_agc_hang_y / 4;

                                rx2_agc_hang_y += nVerticalShift;

                                if (display_rx2_hang_line && console.RX2AGCMode != AGCMode.MED && console.RX2AGCMode != AGCMode.FAST)
                                {
                                    AGCRX2Hang.Height = 8; AGCRX2Hang.Width = 8; AGCRX2Hang.X = 40;

                                    //MW0LGE
                                    //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                                    //    AGCRX2Hang.Y = ((int)rx2_agc_hang_y + 2 * H) - AGCRX2Hang.Height;
                                    //else
                                    //    AGCRX2Hang.Y = ((int)rx2_agc_hang_y + H) - AGCRX2Hang.Height;
                                    AGCRX2Hang.Y = (int)rx2_agc_hang_y - AGCRX2Hang.Height;

                                    drawFillRectangleDX2D(m_bDX2_Yellow, AGCRX2Hang);
                                    //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                                    //{
                                    drawLineDX2D(m_bDX2_Yellow, x3_rx2_hang, rx2_agc_hang_y/* + 2 * H*/, x2_rx2_hang, rx2_agc_hang_y/* + 2 * H*/, m_styleDots);
                                    //}
                                    //else
                                    //{
                                    //    drawLineDX2D(m_bDX2_Yellow, x3_rx2_hang, rx2_agc_hang_y + H, x2_rx2_hang, rx2_agc_hang_y + H, m_styleDots);
                                    //}
                                    drawStringDX2D("-H", fontDX2d_panafont, m_bDX2_pana_text_brush, AGCRX2Hang.X + AGCRX2Hang.Width, AGCRX2Hang.Y - (AGCRX2Hang.Height / 2));
                                }
                                rx2_agc = m_bAutoAGCRX2 ? "-Ga" : "-G";
                                break;
                        }

                        //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                        //    rx2_agcknee_y_value = rx2_agcknee_y_value / 4;
                        //else
                        //    rx2_agcknee_y_value = rx2_agcknee_y_value / 2;

                        if (display_rx2_gain_line)
                        {
                            rx2_agcknee_y_value += nVerticalShift;

                            AGCRX2Knee.Height = 8; AGCRX2Knee.Width = 8; AGCRX2Knee.X = 40;

                            //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                            //    AGCRX2Knee.Y = ((int)rx2_agcknee_y_value + 2 * H) - AGCRX2Knee.Height;
                            //else
                            //    AGCRX2Knee.Y = ((int)rx2_agcknee_y_value + H) - AGCRX2Knee.Height;
                            AGCRX2Knee.Y = (int)rx2_agcknee_y_value - AGCRX2Knee.Height;

                            drawFillRectangleDX2D(m_bDX2_YellowGreen, AGCRX2Knee);
                            //if (current_display_mode_bottom == DisplayMode.PANAFALL)
                            //{
                            drawLineDX2D(m_bDX2_YellowGreen, x1_rx2_gain, rx2_agcknee_y_value/* + 2 * H*/, x2_rx2_gain, rx2_agcknee_y_value/* + 2 * H*/, m_styleDots);
                            //}
                            //else
                            //{
                            //    drawLineDX2D(m_bDX2_YellowGreen, x1_rx2_gain, rx2_agcknee_y_value + H, x2_rx2_gain, rx2_agcknee_y_value + H, m_styleDots);
                            //}
                            drawStringDX2D(rx2_agc, fontDX2d_panafont, m_bDX2_pana_text_brush, AGCRX2Knee.X + AGCRX2Knee.Width, AGCRX2Knee.Y - (AGCRX2Knee.Height / 2));
                        }
                    }
                }
            }
            #endregion

            #region Spots
            // ke9ns add draw DX SPOTS on pandapter
            //=====================================================================
            //=====================================================================

            if (!bIsWaterfall && SpotControl.SP_Active != 0)
            {

                int iii = 0;                          // ke9ns add stairstep holder

                int kk = 0;                           // ke9ns add index for holder[] after you draw the vert line, then draw calls (so calls can overlap the vert lines)

                int vfo_hz = (int)vfoa_hz;    // vfo freq in hz

                int H1a = H / 2;            // length of vertical line (based on rx1 and rx2 display window configuration)
                int H1b = 20;               // starting point of vertical line

                int rxDisplayLow = RXDisplayLow;
                int rxDisplayHigh = RXDisplayHigh;
                SizeF length;

                // RX1/RX2 PanF/Pan = 5,2 (K9,K10)(short)  PanF/PanF = 5,5, (short) Pan/Pan 2,2 (long)
                if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))                 // if your drawing to the bottom 
                {
                    //if ((K9 == 2) && (K10 == 2)) H1a = H + (H / 2); // long
                    //else H1a = H + (H / 4); // short

                    //H1b = H + 20;

                    vfo_hz = (int)vfob_hz;
                    rxDisplayLow = RX2DisplayLow;
                    rxDisplayHigh = RX2DisplayHigh;

                    Console.DXK2 = 0;        // RX2 index to allow call signs to draw after all the vert lines on the screen
                }
                else
                {
                    Console.DXK = 0;        // RX1 index to allow call signs to draw after all the vert lines on the screen
                }

                VFOLow = vfo_hz + rxDisplayLow;    // low freq (left side) in hz
                VFOHigh = vfo_hz + rxDisplayHigh; // high freq (right side) in hz
                VFODiff = VFOHigh - VFOLow;       // diff in hz

                if ((vfo_hz < 5000000) || ((vfo_hz > 6000000) && (vfo_hz < 8000000))) m_bLSB = true; // LSB
                else m_bLSB = false;     // usb

                //-------------------------------------------------------------------------------------------------
                //-------------------------------------------------------------------------------------------------
                // draw DX spots
                //-------------------------------------------------------------------------------------------------
                //-------------------------------------------------------------------------------------------------

                for (int ii = 0; ii < SpotControl.DX_Index; ii++)     // Index through entire DXspot to find what is on this panadapter (draw vert lines first)
                {
                    if ((SpotControl.DX_Freq[ii] >= VFOLow) && (SpotControl.DX_Freq[ii] <= VFOHigh))
                    {
                        int VFO_DXPos = (int)((((float)W / (float)VFODiff) * (float)(SpotControl.DX_Freq[ii] + cwSideToneShiftInverted - VFOLow))); // determine DX spot line pos on current panadapter screen

                        holder[kk] = ii;                    // ii is the actual DX_INdex pos the the KK holds
                        holder1[kk] = VFO_DXPos;

                        kk++;

                        drawLineDX2D(m_bDX2_p1, VFO_DXPos, H1b + nVerticalShift, VFO_DXPos, H1a + nVerticalShift);   // draw vertical line

                    }

                } // for loop through DX_Index


                int bb = 0;
                if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))
                {
                    Console.DXK2 = kk; // keep a count for the bottom QRZ hyperlink
                    bb = Console.MMK4;
                }
                else
                {
                    Console.DXK = kk; // count of spots in current panadapter
                    bb = Console.MMK3;
                }


                //--------------------------------------------------------------------------------------------
                for (int ii = 0; ii < kk; ii++) // draw call signs to screen in order to draw over the vert lines
                {
                    // font
                    if (m_bLSB) // 1=LSB so draw on left side of line
                    {

                        if (Console.DisplaySpot) // display Spotted on Pan
                        {
                            length = measureStringDX2D(SpotControl.DX_Station[holder[ii]], fontDX2d_font1); //  temp used to determine the size of the string when in LSB and you need to reserve a certain space//  (cl.Width);

                            if ((bb > 0) && (SpotControl.SP6_Active != 0))
                            {
                                int x2 = holder1[ii] - (int)length.Width;
                                int y2 = H1b + iii;

                                for (int jj = 0; jj < bb; jj++)
                                {

                                    if (((x2 + length.Width) >= Console.MMX[jj]) && (x2 < (Console.MMX[jj] + Console.MMW[jj])))
                                    {
                                        if (((y2 + length.Height) >= Console.MMY[jj]) && (y2 < (Console.MMY[jj] + Console.MMH[jj])))
                                        {
                                            iii = iii + 33;
                                            break;
                                        }
                                    }

                                } // for loop to check if DX text will draw over top of Memory text
                            }

                            drawStringDX2D(SpotControl.DX_Station[holder[ii]], fontDX2d_font1, m_bDX2_grid_text_brush, holder1[ii] - (int)length.Width, H1b + iii + nVerticalShift);
                        }
                        else // display SPOTTER on Pan (not the Spotted)
                        {
                            length = measureStringDX2D(SpotControl.DX_Spotter[holder[ii]], fontDX2d_font1); //  temp used to determine the size of the string when in LSB and you need to reserve a certain space//  (cl.Width);

                            if ((bb > 0) && (SpotControl.SP6_Active != 0))
                            {
                                int x2 = holder1[ii] - (int)length.Width;
                                int y2 = H1b + iii;

                                for (int jj = 0; jj < bb; jj++)
                                {

                                    if (((x2 + length.Width) >= Console.MMX[jj]) && (x2 < (Console.MMX[jj] + Console.MMW[jj])))
                                    {
                                        if (((y2 + length.Height) >= Console.MMY[jj]) && (y2 < (Console.MMY[jj] + Console.MMH[jj])))
                                        {
                                            iii = iii + 33;
                                            break;
                                        }
                                    }

                                } // for loop to check if DX text will draw over top of Memory text
                            }

                            drawStringDX2D(SpotControl.DX_Spotter[holder[ii]], fontDX2d_font1, m_bDX2_grid_text_brush, holder1[ii] - (int)length.Width, H1b + iii + nVerticalShift);

                        }

                        if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) rx2 = 50; // allow only 50 qrz spots per Receiver
                        else rx2 = 0;

                        if (!/*mox*/local_mox) // only do when not transmitting
                        {
                            Console.DXW[ii + rx2] = (int)length.Width;    // this is all for QRZ hyperlinking 
                            Console.DXH[ii + rx2] = (int)length.Height;
                            Console.DXX[ii + rx2] = holder1[ii] - (int)length.Width;
                            Console.DXY[ii + rx2] = H1b + iii;
                            Console.DXS[ii + rx2] = SpotControl.DX_Station[holder[ii]];

                        }


                    } // LSB side


                    else   // 0=usb so draw on righ side of line (normal)
                    {
                        if (Console.DisplaySpot) // spot
                        {
                            length = measureStringDX2D(SpotControl.DX_Station[holder[ii]], fontDX2d_font1); //  not needed here but used for qrz hyperlinking

                            if ((bb > 0) && (SpotControl.SP6_Active != 0))
                            {
                                int x2 = holder1[ii];
                                int y2 = H1b + iii;

                                for (int jj = 0; jj < bb; jj++)
                                {

                                    if (((x2 + length.Width) >= Console.MMX[jj]) && (x2 < (Console.MMX[jj] + Console.MMW[jj])))
                                    {
                                        if (((y2 + length.Height) >= Console.MMY[jj]) && (y2 < (Console.MMY[jj] + Console.MMH[jj])))
                                        {
                                            iii = iii + 33;
                                            break;
                                        }
                                    }

                                } // for loop to check if DX text will draw over top of Memory text
                            }

                            drawStringDX2D(SpotControl.DX_Station[holder[ii]], fontDX2d_font1, m_bDX2_grid_text_brush, holder1[ii], H1b + iii + nVerticalShift); // DX station name
                        }
                        else // spotter
                        {
                            length = measureStringDX2D(SpotControl.DX_Spotter[holder[ii]], fontDX2d_font1); //  not needed here but used for qrz hyperlinking

                            if ((bb > 0) && (SpotControl.SP6_Active != 0))
                            {
                                int x2 = holder1[ii];
                                int y2 = H1b + iii;

                                for (int jj = 0; jj < bb; jj++)
                                {

                                    if (((x2 + length.Width) >= Console.MMX[jj]) && (x2 < (Console.MMX[jj] + Console.MMW[jj])))
                                    {
                                        if (((y2 + length.Height) >= Console.MMY[jj]) && (y2 < (Console.MMY[jj] + Console.MMH[jj])))
                                        {
                                            iii = iii + 33;
                                            break;
                                        }
                                    }

                                } // for loop to check if DX text will draw over top of Memory text
                            }

                            drawStringDX2D(SpotControl.DX_Spotter[holder[ii]], fontDX2d_font1, m_bDX2_grid_text_brush, holder1[ii], H1b + iii + nVerticalShift); // DX station name

                        }

                        if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2)) rx2 = 50;
                        else rx2 = 0;

                        if (!/*mox*/local_mox) // only do when not transmitting
                        {
                            Console.DXW[ii + rx2] = (int)length.Width;   // this is all for QRZ hyperlinking 
                            Console.DXH[ii + rx2] = (int)length.Height;
                            Console.DXX[ii + rx2] = holder1[ii];
                            Console.DXY[ii + rx2] = H1b + iii;
                            Console.DXS[ii + rx2] = SpotControl.DX_Station[holder[ii]];
                        }

                        if (vfo_hz >= 50000000) // 50000000 or 50mhz
                        {
                            iii = iii + 11;
                            drawStringDX2D(SpotControl.DX_Grid[holder[ii]], fontDX2d_font1, m_bDX2_grid_text_brush, holder1[ii], H1b + iii + nVerticalShift); // DX grid name
                        }

                    } // USB side

                    iii = iii + 11;
                    if (iii > 90) iii = 0;


                }// for loop through DX_Index
            }
            #endregion           
        }

        private static void DrawCursorInfo(int W)
        {
            //MHzCursor Display
            if ((m_bAlwaysShowCursorInfo || Common.ShiftKeyDown) && display_cursor_x != -1)
            {
                bool bLeftSide = false;
                int width = 0;
                int xPos;

                if (m_sMHzCursorDisplay != "")
                {
                    width = (int)measureStringDX2D(m_sMHzCursorDisplay, fontDX2d_callout, true).Width;
                    xPos = display_cursor_x + 12;
                    if (xPos + width > W)
                    {
                        xPos -= width + 24;
                        bLeftSide = true;
                    }
                    drawStringDX2D(m_sMHzCursorDisplay, fontDX2d_callout, m_bDX2_m_bTextCallOutActive, xPos, display_cursor_y - 18);
                }

                if (m_sOtherData1CursorDisplay != "") {
                    xPos = display_cursor_x + 12;
                    if (bLeftSide)
                    {
                        xPos -= width + 24;
                    }
                    else if (xPos + width > W)
                    {
                        width = (int)measureStringDX2D(m_sOtherData1CursorDisplay, fontDX2d_callout, true).Width;
                        xPos -= width + 24;
                        bLeftSide = true;
                    }
                    drawStringDX2D(m_sOtherData1CursorDisplay, fontDX2d_callout, m_bDX2_m_bTextCallOutActive, xPos, display_cursor_y + 2);
                }

                if (m_sOtherData2CursorDisplay != "")
                {
                    xPos = display_cursor_x + 12;
                    if (bLeftSide)
                    {
                        xPos -= width + 24;
                    }
                    else if (xPos + width > W)
                    {
                        width = (int)measureStringDX2D(m_sOtherData2CursorDisplay, fontDX2d_callout, true).Width;
                        xPos -= width + 24;
                        bLeftSide = true;
                    }
                    drawStringDX2D(m_sOtherData2CursorDisplay, fontDX2d_callout, m_bDX2_m_bTextCallOutActive, xPos, display_cursor_y + 18); 
                }
            }
            //
        }

        unsafe static private bool DrawSpectrumDX2D(int rx, int W, int H, bool bottom)
        {
            DrawSpectrumGridDX2D(W, H, bottom);

            //updateSharePointsArray(W);

            int low = 0;
            int high = 0;
            float local_max_y = float.MinValue;
            int grid_max = 0;
            int grid_min = 0;

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                low = rx_spectrum_display_low;
                high = rx_spectrum_display_high;
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
            }
            else
            {
                low = tx_spectrum_display_low;
                high = tx_spectrum_display_high;
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
            }

            if (rx1_dsp_mode == DSPMode.DRM)
            {
                low = 2500;
                high = 21500;
            }

            int yRange = grid_max - grid_min;

            int nDecimatedWidth = W / m_nDecimation;

            if (!bottom && data_ready)
            {
                if (mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                {
                    for (int i = 0; i < nDecimatedWidth/*current_display_data.Length*/; i++)
                        current_display_data[i] = grid_min - rx1_display_cal_offset;
                }
                else
                {
                    fixed (void* rptr = &new_display_data[0])
                    fixed (void* wptr = &current_display_data[0])
                        Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/nDecimatedWidth * sizeof(float));
                }
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                {
                    fixed (void* rptr = &new_display_data_bottom[0])
                    fixed (void* wptr = &current_display_data_bottom[0])
                        Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/nDecimatedWidth * sizeof(float));
                }
                data_ready_bottom = false;
            }

            int Y;
            SharpDX.Vector2 point = new SharpDX.Vector2();
            SharpDX.Vector2 previousPoint = new SharpDX.Vector2();

            //inital state for X,Y, so we dont get a line from 0,0
            float max;
            if (!bottom)
            {
                max = current_display_data[0];
            }
            else
            {
                max = current_display_data_bottom[0];
            }

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled)) 
            {
                if (rx==1/*!bottom*/) max += rx1_display_cal_offset; //MW0LGE_21g
                else max += rx2_display_cal_offset;
            }
            else
            {
                max += tx_display_cal_offset;
            }

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                if (rx==1/*!bottom*/) max += rx1_preamp_offset - alex_preamp_offset;   //MW0LGE_21 change to rx==1
                else max += rx2_preamp_offset;
            }

            Y = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);
            previousPoint.X = 0;// + 0.5f; // the 0.5f pushes it into the middle of a 'pixel', so that it is not drawn half in one, and half in the other
            previousPoint.Y = Y;// + 0.5f;

            for (int i = 0; i < nDecimatedWidth; i++)
            {
                if (!bottom)
                {
                    max = current_display_data[i];
                }
                else
                {
                    max = current_display_data_bottom[i];
                }

                if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
                {
                    if (rx==1/*!bottom*/) max += rx1_display_cal_offset; //MW0LGE_21g
                    else max += rx2_display_cal_offset;
                }
                else
                {
                    max += tx_display_cal_offset;
                }

                if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
                {
                    if (rx==1/*!bottom*/) max += rx1_preamp_offset - alex_preamp_offset;  //MW0LGE_21 change to rx==1
                    else max += rx2_preamp_offset;
                }

                if (max > local_max_y)
                {
                    local_max_y = max;
                    max_x = i * m_nDecimation;
                }

                Y = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);
                point.X = i * m_nDecimation;// + 0.5f; // the 0.5f pushes it into the middle of a 'pixel', so that it is not drawn half in one, and half in the other
                point.Y = Y;// + 0.5f;

                _d2dRenderTarget.DrawLine(previousPoint, point, m_bDX2_data_line_pen_brush, data_line_pen.Width);

                previousPoint.X = point.X;
                previousPoint.Y = point.Y;
            }

            max_y = local_max_y;

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                SharpDX.Direct2D1.Brush b = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_brush : m_bDX2_Red;
                if (bottom)
                {
                    drawLineDX2D(b, display_cursor_x, H, display_cursor_x, H + H, grid_text_pen.Width);
                    drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y, grid_text_pen.Width);
                }
                else
                {
                    drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H, grid_text_pen.Width);
                    drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y, grid_text_pen.Width);
                }
            }

            return true;
        }

        private static void DrawSpectrumGridDX2D(int W, int H, bool bottom)
        {
            // draw background
            drawFillRectangleDX2D(m_bDX2_display_background_brush, 0, bottom ? H : 0, W, H);

            int low = 0;								// init limit variables
            int high = 0;

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                low = rx_spectrum_display_low;				// get RX display limits
                high = rx_spectrum_display_high;
            }
            else
            {
                if (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU)
                {
                    low = rx_spectrum_display_low;
                    high = rx_spectrum_display_high;
                }
                else
                {
                    low = tx_spectrum_display_low;			// get RX display limits
                    high = tx_spectrum_display_high;
                }
            }

            int center_line_x = (int)(-(double)low / (high - low) * W);

            int mid_w = W / 2;
            int[] step_list = { 10, 20, 25, 50 };
            int step_power = 1;
            int step_index = 0;
            int freq_step_size = 50;
            int grid_max = 0;
            int grid_min = 0;
            int grid_step = 0;

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))// || (mox && tx_on_vfob))
            {
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
                grid_step = spectrum_grid_step;
            }
            else if (mox)
            {
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
                grid_step = tx_spectrum_grid_step;
            }

            int y_range = grid_max - grid_min;
            if (split_display) grid_step *= 2;

            if (high == 0)
            {
                int f = -low;
                // Calculate horizontal step size
                while (f / freq_step_size > 7)
                {
                    freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                    step_index = (step_index + 1) % 4;
                    if (step_index == 0) step_power++;
                }
                float pixel_step_size = (float)(W * freq_step_size / f);

                int num_steps = f / freq_step_size;

                // Draw vertical lines
                for (int i = 1; i <= num_steps; i++)
                {
                    int x = W - (int)Math.Floor(i * pixel_step_size);   // for negative numbers

                    if (bottom) drawLineDX2D(m_bDX2_grid_pen, x, H, x, H + H);
                    else drawLineDX2D(m_bDX2_grid_pen, x, 0, x, H);				// draw right line

                    // Draw vertical line labels
                    int num = i * freq_step_size;
                    string label = num.ToString();
                    int offset = (int)((label.Length + 1) * 4.1);
                    if (x - offset >= 0)
                    {
                        if (bottom) drawStringDX2D("-" + label, fontDX2d_font9, m_bDX2_grid_text_brush, x - offset, H + (float)Math.Floor(H * .01));
                        else drawStringDX2D("-" + label, fontDX2d_font9, m_bDX2_grid_text_brush, x - offset, (float)Math.Floor(H * .01));
                    }
                }

                // Draw horizontal lines
                int V = (int)(grid_max - grid_min);
                num_steps = V / grid_step;
                pixel_step_size = H / num_steps;

                for (int i = 1; i < num_steps; i++)
                {
                    int xOffset = 0;
                    int num = grid_max - i * grid_step;
                    int y = (int)Math.Floor((double)(grid_max - num) * H / y_range);

                    if (bottom) drawLineDX2D(m_bDX2_hgrid_pen, 0, H + y, W, H + y);
                    else drawLineDX2D(m_bDX2_hgrid_pen, 0, y, W, y);

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)measureStringDX2D("0", fontDX2d_font9).Width;// use 0 here instead of a - sign
                        //int offset = (int)(label.Length * 4.1);
                        SizeF size = measureStringDX2D(label, fontDX2d_font9);

                        int x = 0;
                        switch (display_label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                x = center_line_x + xOffset;
                                break;
                            case DisplayLabelAlignment.RIGHT:
                                x = (int)(W - size.Width - 3);
                                break;
                            case DisplayLabelAlignment.AUTO:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.OFF:
                                x = W;
                                break;
                        }
                        console.RX1DisplayGridX = x;
                        console.RX1DisplayGridW = (int)(x + size.Width);
                        y -= 8;
                        if (y + 9 < H)
                        {
                            if (bottom) drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, H + y);
                            else drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, y);
                        }
                    }
                }

                // Draw middle vertical line
                if (rx1_dsp_mode == DSPMode.AM ||
                    rx1_dsp_mode == DSPMode.SAM ||
                    rx1_dsp_mode == DSPMode.FM ||
                    rx1_dsp_mode == DSPMode.DSB ||
                    rx1_dsp_mode == DSPMode.SPEC)
                    if (bottom)
                    {
                        drawLineDX2D(m_bDX2_grid_zero_pen, W - 1, H, W - 1, H + H);
                        drawLineDX2D(m_bDX2_grid_zero_pen, W - 2, H, W - 2, H + H);
                    }
                    else
                    {
                        drawLineDX2D(m_bDX2_grid_zero_pen, W - 1, 0, W - 1, H);
                        drawLineDX2D(m_bDX2_grid_zero_pen, W - 2, 0, W - 2, H);
                    }
            }
            else if (low == 0)
            {
                int f = high;
                // Calculate horizontal step size
                while (f / freq_step_size > 7)
                {
                    freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                    step_index = (step_index + 1) % 4;
                    if (step_index == 0) step_power++;
                }
                float pixel_step_size = (float)(W * freq_step_size / f);
                int num_steps = f / freq_step_size;

                // Draw vertical lines
                for (int i = 1; i <= num_steps; i++)
                {
                    int x = (int)Math.Floor(i * pixel_step_size);// for positive numbers

                    if (bottom) drawLineDX2D(m_bDX2_grid_pen, x, H, x, H + H);
                    else drawLineDX2D(m_bDX2_grid_pen, x, 0, x, H);				// draw right line

                    // Draw vertical line labels
                    int num = i * freq_step_size;
                    string label = num.ToString();
                    int offset = (int)(label.Length * 4.1);
                    if (x - offset + label.Length * 7 < W)
                    {
                        if (bottom) drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x - offset, H + (float)Math.Floor(H * .01));
                        else drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x - offset, (float)Math.Floor(H * .01));
                    }
                }

                // Draw horizontal lines
                int V = (int)(grid_max - grid_min);
                int numSteps = V / grid_step;
                pixel_step_size = H / numSteps;
                for (int i = 1; i < numSteps; i++)
                {
                    int xOffset = 0;
                    int num = grid_max - i * grid_step;
                    int y = (int)Math.Floor((double)(grid_max - num) * H / y_range);

                    if (bottom) drawLineDX2D(m_bDX2_hgrid_pen, 0, H + y, W, H + y);
                    else drawLineDX2D(m_bDX2_hgrid_pen, 0, y, W, y);

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)measureStringDX2D("-", fontDX2d_font9).Width - 2;
                        int offset = (int)(label.Length * 4.1);
                        SizeF size = measureStringDX2D(label, fontDX2d_font9);

                        int x = 0;
                        switch (display_label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                x = center_line_x + xOffset;
                                break;
                            case DisplayLabelAlignment.RIGHT:
                                x = (int)(W - size.Width - 3);
                                break;
                            case DisplayLabelAlignment.AUTO:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.OFF:
                                x = W;
                                break;
                        }
                        console.RX1DisplayGridX = x;
                        console.RX1DisplayGridW = (int)(x + size.Width);
                        y -= 8;
                        if (y + 9 < H)
                        {
                            if (bottom) drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, H + y);
                            drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, y);

                        }
                    }
                }

                // Draw middle vertical line
                if (rx1_dsp_mode == DSPMode.AM ||
                   rx1_dsp_mode == DSPMode.SAM ||
                   rx1_dsp_mode == DSPMode.FM ||
                   rx1_dsp_mode == DSPMode.DSB ||
                   rx1_dsp_mode == DSPMode.SPEC)
                    if (bottom)
                    {
                        drawLineDX2D(m_bDX2_grid_pen, 0, H, 0, H + H);
                        drawLineDX2D(m_bDX2_grid_pen, 1, H, 1, H + H);
                    }
                    else
                    {
                        drawLineDX2D(m_bDX2_grid_pen, 0, 0, 0, H);
                        drawLineDX2D(m_bDX2_grid_pen, 1, 0, 1, H);
                    }
            }
            else if (low < 0 && high > 0)
            {
                int f = high;

                // Calculate horizontal step size
                while (f / freq_step_size > 4)
                {
                    freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                    step_index = (step_index + 1) % 4;
                    if (step_index == 0) step_power++;
                }
                int pixel_step_size = W / 2 * freq_step_size / f;
                int num_steps = f / freq_step_size;

                // Draw vertical lines
                for (int i = 1; i <= num_steps; i++)
                {
                    int xLeft = mid_w - (i * pixel_step_size);			// for negative numbers
                    int xRight = mid_w + (i * pixel_step_size);		// for positive numbers
                    if (bottom)
                    {
                        drawLineDX2D(m_bDX2_grid_pen, xLeft, H, xLeft, H + H);		// draw left line
                        drawLineDX2D(m_bDX2_grid_pen, xRight, H, xRight, H + H);		// draw right line
                    }
                    else
                    {
                        drawLineDX2D(m_bDX2_grid_pen, xLeft, 0, xLeft, H);		// draw left line
                        drawLineDX2D(m_bDX2_grid_pen, xRight, 0, xRight, H);		// draw right line
                    }

                    // Draw vertical line labels
                    int num = i * freq_step_size;
                    string label = num.ToString();
                    int offsetL = (int)((label.Length + 1) * 4.1);
                    int offsetR = (int)(label.Length * 4.1);
                    if (xLeft - offsetL >= 0)
                    {
                        if (bottom)
                        {
                            drawStringDX2D("-" + label, fontDX2d_font9, m_bDX2_grid_text_brush, xLeft - offsetL, H + (float)Math.Floor(H * .01));
                            drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, xRight - offsetR, H + (float)Math.Floor(H * .01));
                        }
                        else
                        {
                            drawStringDX2D("-" + label, fontDX2d_font9, m_bDX2_grid_text_brush, xLeft - offsetL, (float)Math.Floor(H * .01));
                            drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, xRight - offsetR, (float)Math.Floor(H * .01));
                        }
                    }
                }

                // Draw horizontal lines
                int V = (int)(grid_max - grid_min);
                int numSteps = V / grid_step;
                pixel_step_size = H / numSteps;
                for (int i = 1; i < numSteps; i++)
                {
                    int xOffset = 0;
                    int num = grid_max - i * grid_step;
                    int y = (int)Math.Floor((double)(grid_max - num) * H / y_range);
                    if (bottom) drawLineDX2D(m_bDX2_grid_pen, 0, H + y, W, H + y);
                    else drawLineDX2D(m_bDX2_grid_pen, 0, y, W, y);

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)measureStringDX2D("-", fontDX2d_font9).Width - 2;
                        int offset = (int)(label.Length * 4.1);
                        SizeF size = measureStringDX2D(label, fontDX2d_font9);

                        int x = 0;
                        switch (display_label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                x = center_line_x + xOffset;
                                break;
                            case DisplayLabelAlignment.RIGHT:
                                x = (int)(W - size.Width - 3);
                                break;
                            case DisplayLabelAlignment.AUTO:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.OFF:
                                x = W;
                                break;
                        }

                        console.RX1DisplayGridX = x;
                        console.RX1DisplayGridW = (int)(x + size.Width);
                        y -= 8;
                        if (y + 9 < H)
                        {
                            if (bottom) drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, H + y);
                            drawStringDX2D(label, fontDX2d_font9, m_bDX2_grid_text_brush, x, y);
                        }
                    }
                }

                // Draw middle vertical line
                if (rx1_dsp_mode == DSPMode.AM ||
                   rx1_dsp_mode == DSPMode.SAM ||
                   rx1_dsp_mode == DSPMode.FM ||
                   rx1_dsp_mode == DSPMode.DSB ||
                   rx1_dsp_mode == DSPMode.SPEC)
                    if (bottom)
                    {
                        drawLineDX2D(m_bDX2_grid_zero_pen, mid_w, H, mid_w, H + H);
                        drawLineDX2D(m_bDX2_grid_zero_pen, mid_w - 1, H, mid_w - 1, H + H);
                    }
                    else
                    {
                        drawLineDX2D(m_bDX2_grid_zero_pen, mid_w, 0, mid_w, H);
                        drawLineDX2D(m_bDX2_grid_zero_pen, mid_w - 1, 0, mid_w - 1, H);
                    }
            }
        }

        unsafe private static bool DrawScopeDX2D(int W, int H, bool bottom)
        {
            int pixel;
            int nDecimatedWidth = W / m_nDecimation;

            if (scope_min == null || scope_min.Length != nDecimatedWidth)
            {
                scope_min = new float[nDecimatedWidth];
                Audio.ScopeMin = scope_min;
                return false;
            }
            if (scope_max == null || scope_max.Length != nDecimatedWidth)
            {
                scope_max = new float[nDecimatedWidth];
                Audio.ScopeMax = scope_max;
                return false;
            }

            drawFillRectangleDX2D(m_bDX2_display_background_brush, 0, bottom ? H : 0, W, H);

            SharpDX.Vector2 pointMin = new SharpDX.Vector2();
            SharpDX.Vector2 pointMax = new SharpDX.Vector2();
            //SharpDX.Vector2 previousPointMin = new SharpDX.Vector2();
            //SharpDX.Vector2 previousPointMax = new SharpDX.Vector2();

            //int y;

            // int startY;
            //int endY;

            ////// inital state
            ////pixel = (int)(H / 2 * scope_max[0]);
            ////startY = H / 2 - pixel;
            ////if (bottom) startY += H;

            ////pixel = (int)(H / 2 * scope_min[0]);
            ////endY = H / 2 - pixel;
            ////if (bottom) endY += H;

            //PathGeometry sharpGeometry = new PathGeometry(d2dRenderTarget.Factory);
            //GeometrySink geo = sharpGeometry.Open();
            //geo.BeginFigure(new SharpDX.Vector2(0, startY), FigureBegin.Filled);

            //// the 0.5f's to move to centre pixel
            //for (int i = 0; i < W; i++)						// fill point array
            //{
            //    pixel = (int)(H / 2 * scope_max[i]);
            //    y = H / 2 - pixel;
            //    pointMax.X = i;// + 0.5f;
            //    pointMax.Y = y;// + 0.5f;
            //    if (bottom) pointMax.Y += H;

            //    geo.AddLine(new SharpDX.Vector2(i, pointMax.Y));
            //}

            //for (int i = W - 1; i >= 0; i--)                     // fill point array                
            //{
            //    pixel = (int)(H / 2 * scope_min[i]);
            //    y = H / 2 - pixel;
            //    pointMin.X = i;// + 0.5f;
            //    pointMin.Y = y;// + 0.5f;
            //    if (bottom) pointMin.Y += H;

            //    geo.AddLine(new SharpDX.Vector2(i, pointMin.Y));
            //}

            //geo.EndFigure(FigureEnd.Closed);
            //geo.Close();
            //geo.Dispose();

            //d2dRenderTarget.FillGeometry(sharpGeometry, m_bDX2_data_line_pen_brush); // sometimes when filled is over top of each other, it doesnt show, so do outline as well below
            //d2dRenderTarget.DrawGeometry(sharpGeometry, m_bDX2_data_line_pen_brush);

            //sharpGeometry.Dispose();

            int y2 = (int)(H * 0.5f);

            SharpDX.Vector2 previousPointMax = new SharpDX.Vector2();
            SharpDX.Vector2 previousPointMin = new SharpDX.Vector2();

            previousPointMax.X = previousPointMin.X = 0;

            pixel = (int)(H / 2 * scope_max[0]);
            previousPointMax.Y = H / 2 - pixel;
            pixel = (int)(H / 2 * scope_min[0]);
            previousPointMin.Y = H / 2 - pixel;

            if (bottom)
            {
                previousPointMax.Y += H;
                previousPointMin.Y += H;
                y2 += H;
            }

            drawLineDX2D(m_bDX2_y2_brush, 0, y2, W, y2); // Middle line

            for (int i = 1; i < nDecimatedWidth; i++)
            {
                pointMax.X = i * m_nDecimation;
                pointMin.X = pointMax.X;

                pixel = (int)(H / 2 * scope_max[i]);
                pointMax.Y = H / 2 - pixel;

                pixel = (int)(H / 2 * scope_min[i]);
                pointMin.Y = H / 2 - pixel;

                if (bottom)
                {
                    pointMax.Y += H;
                    pointMin.Y += H;
                }

                _d2dRenderTarget.DrawLine(previousPointMax, pointMax, m_bDX2_waveform_line_pen);
                _d2dRenderTarget.DrawLine(previousPointMin, pointMin, m_bDX2_waveform_line_pen);
                _d2dRenderTarget.DrawLine(pointMin, pointMax, m_bDX2_waveform_line_pen, m_nDecimation);

                previousPointMax.X = i * m_nDecimation;
                previousPointMin.X = previousPointMax.X;

                previousPointMax.Y = pointMax.Y;
                previousPointMin.Y = pointMin.Y;
            }

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                SharpDX.Direct2D1.Brush b = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_pen : m_bDX2_Red;
                if (bottom) drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H + H);
                else drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H);
                drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y);
            }

            return true;
        }

        unsafe private static bool DrawScope2DX2D(int W, int H, bool bottom)
        {
            int nDecimatedWidth = W / m_nDecimation;

            if (scope_min == null || scope_min.Length != nDecimatedWidth)
            {
                scope_min = new float[nDecimatedWidth];
                Audio.ScopeMin = scope_min;
            }

            if (scope_max == null || scope_max.Length != nDecimatedWidth)
            {
                scope_max = new float[nDecimatedWidth];
                Audio.ScopeMax = scope_max;
            }

            if (scope2_min == null || scope2_min.Length != nDecimatedWidth)
            {
                scope2_min = new float[nDecimatedWidth];
                Audio.Scope2Min = scope2_min;
            }
            if (scope2_max == null || scope2_max.Length != nDecimatedWidth)
            {
                scope2_max = new float[nDecimatedWidth];
                Audio.Scope2Max = scope2_max;
            }

            int y1 = (int)(H * 0.25f);
            int y2 = (int)(H * 0.5f);
            int y3 = (int)(H * 0.75f);
            drawLineDX2D(m_bDX2_y1_brush, 0, y1, W, y1);
            drawLineDX2D(m_bDX2_y2_brush, 0, y2, W, y2);
            drawLineDX2D(m_bDX2_y1_brush, 0, y3, W, y3);

            //int samples = nDecimatedWidth;// scope2_max.Length;   //MW0LGE blimey
            //float xScale = 1;// (float)samples / W;
            float yScale = (float)H / 4;

            SharpDX.Vector2 point = new SharpDX.Vector2();
            SharpDX.Vector2 previousPoint = new SharpDX.Vector2();

            // the 0.5f's to move to middle pixel
            // draw the left input samples
            previousPoint.X = 0;
            previousPoint.Y = (int)(y1 - (scope2_max[0] * yScale));
            for (int x = 0; x < nDecimatedWidth; x++)
            {
                int i = x;// (int)(x * xScale);// (int)Math.Truncate((float)x * xScale);
                int y = (int)(y1 - (scope2_max[i] * yScale));
                point.X = x * m_nDecimation;// + 0.5f;
                point.Y = y;// + 0.5f;

                _d2dRenderTarget.DrawLine(previousPoint, point, m_bDX2_waveform_line_pen);

                previousPoint.X = point.X;
                previousPoint.Y = point.Y;
            }

            // draw the right input samples
            previousPoint.X = 0;
            previousPoint.Y = (int)(y3 - (scope_max[0] * yScale));
            for (int x = 0; x < nDecimatedWidth; x++)
            {
                int i = x;//  (int)(x * xScale);//(int)Math.Truncate((float)x * xScale);
                int y = (int)(y3 - (scope_max[i] * yScale));
                point.X = x * m_nDecimation;// + 0.5f;
                point.Y = y;// + 0.5f;

                _d2dRenderTarget.DrawLine(previousPoint, point, m_bDX2_waveform_line_pen);

                previousPoint.X = point.X;
                previousPoint.Y = point.Y;
            }

            return true;
        }

        private static bool m_bShowPhaseAngularMean = false;
        public static bool ShowPhaseAngularMean
        {
            get { return m_bShowPhaseAngularMean; }
            set { m_bShowPhaseAngularMean = value; }
        }

        private static float lerp(float first, float second, float by)
        {
            return first * (1 - by) + second * by;
        }
        private static PointF lerpPointF(PointF first, PointF second, float by)
        {
            return new PointF(lerp(first.X, second.X, by), lerp(first.Y, second.Y, by));
        }

        private static double m_dLastAngleLerp = 0;
        private static PointF m_dOldCM = new PointF(0, 0);

        unsafe private static bool DrawPhaseDX2D(int W, int H, bool bottom)
        {
            DrawPhaseGridDX2D(W, H, bottom);
            int num_points = phase_num_pts;

            if (!bottom && data_ready)
            {
                // get new data
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/(num_points * 2) * sizeof(float));
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                // get new data
                fixed (void* rptr = &new_display_data_bottom[0])
                fixed (void* wptr = &current_display_data_bottom[0])
                    Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/(num_points * 2) * sizeof(float));
                data_ready_bottom = false;
            }

            int nShift = m_nPhasePointSize / 2;

            SharpDX.Vector2 point = new SharpDX.Vector2();

            double sinSum = 0;
            double cosSum = 0;

            for (int i = 0/*, j = 0*/; i < num_points; i++/*, j += 8*/)	// fill point array
            {
                int x = 0;
                int y = 0;
                if (bottom)
                {
                    sinSum += (double)current_display_data_bottom[i * 2];
                    cosSum += (double)current_display_data_bottom[i * 2 + 1];

                    x = (int)(current_display_data_bottom[i * 2] * H / 2);
                    y = (int)(current_display_data_bottom[i * 2 + 1] * H / 2);
                }
                else
                {
                    sinSum += (double)current_display_data[i * 2];
                    cosSum += (double)current_display_data[i * 2 + 1];

                    x = (int)(current_display_data[i * 2] * H / 2);
                    y = (int)(current_display_data[i * 2 + 1] * H / 2);
                }

                point.X = W / 2 + x;
                point.Y = H / 2 + y;
                if (bottom) point.Y += H;

                drawFillRectangleDX2D(m_bDX2_data_line_pen_brush, point.X - nShift, point.Y - nShift, m_nPhasePointSize, m_nPhasePointSize);
            }

            //
            double dCircularMean = Math.Atan2(sinSum, cosSum); // dont need /n as circular nature of sin/cos

            float xx = (float)Math.Sin(dCircularMean);
            float yy = (float)Math.Cos(dCircularMean);

            PointF p = lerpPointF(m_dOldCM, new PointF(xx, yy), (float)((m_dElapsedFrameStart - m_dLastAngleLerp) / 50f));

            m_dOldCM.X = p.X;
            m_dOldCM.Y = p.Y;
            m_dLastAngleLerp = m_dElapsedFrameStart;

            if (m_bShowPhaseAngularMean)
            {
                double lerped = Math.Atan2(p.X, p.Y);

                xx = (float)Math.Sin(lerped) * H / 2;
                yy = (float)Math.Cos(lerped) * H / 2;
                xx += W / 2;
                yy += H / 2;
                if (bottom) yy += H;

                SharpDX.Vector2 pMiddle = new SharpDX.Vector2(W / 2, H / 2);
                SharpDX.Vector2 pEnd = new SharpDX.Vector2((float)xx, (float)yy);

                _d2dRenderTarget.DrawLine(pMiddle, pEnd, m_bDX2_Red, 3);
            }
            //

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                SharpDX.Direct2D1.Brush b = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_pen : m_bDX2_Red;
                if (bottom) drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H + H);
                else drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H);
                drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y);
            }

            return true;
        }

        private static void DrawPhaseGridDX2D(int W, int H, bool bottom)
        {
            // draw background
            drawFillRectangleDX2D(m_bDX2_display_background_brush, 0, bottom ? H : 0, W, H);

            for (double i = 0.50; i < 3; i += .50)	// draw 3 concentric circles
            {
                if (bottom) drawElipseDX2D(m_bDX2_grid_pen, (int)(W / 2), H + (int)(H / 2), (int)(H * i), (int)(H * i));
                else drawElipseDX2D(m_bDX2_grid_pen, (int)(W / 2), (int)(H / 2), (int)(H * i), (int)(H * i));
            }
        }

        unsafe private static void DrawPhase2DX2D(int W, int H, bool bottom)
        {
            DrawPhaseGridDX2D(W, H, bottom);
            int num_points = phase_num_pts;

            if (!bottom && data_ready)
            {
                // get new data
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/(num_points * 2) * sizeof(float));
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                // get new data
                fixed (void* rptr = &new_display_data_bottom[0])
                fixed (void* wptr = &current_display_data_bottom[0])
                    Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/(num_points * 2) * sizeof(float));
                data_ready_bottom = false;
            }

            int nShift = m_nPhasePointSize / 2;

            SharpDX.Vector2 point = new SharpDX.Vector2();

            for (int i = 0; i < num_points; i++)
            {
                int x = 0;
                int y = 0;
                if (bottom)
                {
                    x = (int)(current_display_data_bottom[i * 2] * H * 0.5 * 500);
                    y = (int)(current_display_data_bottom[i * 2 + 1] * H * 0.5 * 500);
                }
                else
                {
                    x = (int)(current_display_data[i * 2] * H * 0.5 * 500);
                    y = (int)(current_display_data[i * 2 + 1] * H * 0.5 * 500);
                }
                point.X = (int)(W * 0.5 + x);
                point.Y = (int)(H * 0.5 + y);
                if (bottom) point.Y += H;

                drawFillRectangleDX2D(m_bDX2_data_line_pen_brush, point.X - nShift, point.Y - nShift, m_nPhasePointSize, m_nPhasePointSize);
            }

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                SharpDX.Direct2D1.Brush b = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_pen : m_bDX2_Red;
                if (bottom) drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H + H);
                else drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H);
                drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y);
            }
        }

        unsafe static private bool DrawHistogramDX2D(int W, int H)
        {
            DrawSpectrumGridDX2D(W, H, false);

            updateSharePointsArray(W);

            int low = 0;
            int high = 0;
            float local_max_y = Int32.MinValue;
            int grid_max = 0;
            int grid_min = 0;

            if (!mox || (mox && tx_on_vfob && console.RX2Enabled))
            {
                low = rx_spectrum_display_low;
                high = rx_spectrum_display_high;
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
            }
            else
            {
                low = tx_spectrum_display_low;
                high = tx_spectrum_display_high;
                grid_max = tx_spectrum_grid_max;
                grid_min = tx_spectrum_grid_min;
            }

            if (rx1_dsp_mode == DSPMode.DRM)
            {
                low = 2500;
                high = 21500;
            }

            int nDecimatedWidth = W / m_nDecimation;

            int yRange = grid_max - grid_min;
            if (data_ready)
            {
                // get new data
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, /*BUFFER_SIZE*/nDecimatedWidth * sizeof(float));

                data_ready = false;
            }

            int sum = 0;

            for (int i = 0; i < nDecimatedWidth; i++)
            {
                float max = max = current_display_data[i];

                if (!mox)
                {
                    max += rx1_display_cal_offset;
                }
                else
                {
                    max += tx_display_cal_offset;
                }

                if (!mox) max += (rx1_preamp_offset - alex_preamp_offset);

                switch (rx1_dsp_mode)
                {
                    case DSPMode.SPEC:
                        max += 6.0F;
                        break;
                }
                if (max > local_max_y)
                {
                    local_max_y = max;
                    max_x = i * m_nDecimation;
                }

                points[i].X = i * m_nDecimation;
                points[i].Y = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);

                sum += points[i].Y;
            }

            max_y = local_max_y;

            // get the average
            float avg = 0.0F;            
            avg = (float)((float)sum / nDecimatedWidth / 1.12);

            for (int i = 0; i < nDecimatedWidth; i++)
            {
                if (points[i].Y < histogram_data[i])
                {
                    histogram_history[i] = 0;
                    histogram_data[i] = points[i].Y;
                }
                else
                {
                    histogram_history[i]++;
                    if (histogram_history[i] > 51)
                    {
                        histogram_history[i] = 0;
                        histogram_data[i] = points[i].Y;
                    }

                    int alpha = Math.Max(255 - histogram_history[i] * 5, 0);
                    int height = points[i].Y - histogram_data[i];

                    m_bDX2_dhp.Opacity = alpha / 255f;
                    drawFillRectangleDX2D(m_bDX2_dhp, i * m_nDecimation, histogram_data[i], m_nDecimation, height);
                }
                if (points[i].Y >= avg)		// value is below the average
                {
                    drawFillRectangleDX2D(m_bDX2_dhp1, points[i].X, points[i].Y, m_nDecimation, H - points[i].Y);
                }
                else
                {
                    drawFillRectangleDX2D(m_bDX2_dhp1, points[i].X, (int)Math.Floor(avg), m_nDecimation, H - (int)Math.Floor(avg));
                    drawFillRectangleDX2D(m_bDX2_dhp2, points[i].X, points[i].Y, m_nDecimation, (int)Math.Floor(avg) - points[i].Y);
                }
            }

            // draw long cursor
            if (current_click_tune_mode != ClickTuneMode.Off)
            {
                SharpDX.Direct2D1.Brush b = current_click_tune_mode == ClickTuneMode.VFOA ? m_bDX2_grid_text_pen : m_bDX2_Red;
                drawLineDX2D(b, display_cursor_x, 0, display_cursor_x, H);
                drawLineDX2D(b, 0, display_cursor_y, W, display_cursor_y);
            }

            return true;
        }

        ////MW0LGE_21b the new peak detect code, here as reference
        ////
        ////https://gist.github.com/mvacha/410e25ded1f66d9bcff33325b80cfca9
        ////http://www.billauer.co.il/peakdet.html
        //private static List<(int pos, double val)> PeakDet(IList<double> vector, double triggerDelta)
        //{
        //    double mn = Double.PositiveInfinity;
        //    double mx = Double.NegativeInfinity;
        //    int mnpos = 0;
        //    int mxpos = 0;
        //    bool lookformax = true;

        //    var maxtab_tmp = new List<(int pos, double val)>();
        //    //var mintab_tmp = new List<(int pos, double val)>();

        //    for (int i = 0; i < vector.Count; i++)
        //    {
        //        double a = vector[i];
        //        if (a > mx)
        //        {
        //            mx = a;
        //            mxpos = i;
        //        }
        //        if (a < mn)
        //        {
        //            mn = a;
        //            mnpos = i;
        //        }
        //        if (lookformax)
        //        {
        //            if (a < mx - triggerDelta)
        //            {
        //                maxtab_tmp.Add((mxpos, mx));
        //                mn = a;
        //                mnpos = i;
        //                lookformax = false;
        //            }
        //        }
        //        else
        //        {
        //            if (a > mn + triggerDelta)
        //            {
        //                //mintab_tmp.Add((mnpos, ns));
        //                mx = a;
        //                mxpos = i;
        //                lookformax = true;
        //            }
        //        }
        //    }
        //
        //    return maxtab_tmp;
        //}       


        // spot draw2

        private static bool _NFDecimal = false;
        public static bool NoiseFloorDecimal
        {
            get { return _NFDecimal; }
            set { _NFDecimal = value; }
        }

        private static List<int> _spotLayerRightRX1 = new List<int>();
        private static List<int> _spotLayerRightRX2 = new List<int>();
        private static Dictionary<Color, SharpDX.Direct2D1.Brush> _DX2Brushes = null;


        private static int getSpotLayer(int rx, int leftX)
        {
            if (rx == 1)
            {
                if (_spotLayerRightRX1 == null)
                    _spotLayerRightRX1 = new List<int>();

                for (int layer = 0;layer < _spotLayerRightRX1.Count;layer++)
                {
                    int layerRightmostX = _spotLayerRightRX1[layer];
                    if(leftX > layerRightmostX)
                        return layer;
                }
                // none found, need to add another
                _spotLayerRightRX1.Add(int.MinValue);
                return _spotLayerRightRX1.Count - 1;
            }
            else
            {
                if (_spotLayerRightRX2 == null)
                    _spotLayerRightRX2 = new List<int>();

                for (int layer = 0; layer < _spotLayerRightRX2.Count; layer++)
                {
                    int layerRightmostX = _spotLayerRightRX2[layer];
                    if (leftX > layerRightmostX)
                        return layer;
                }
                // none found, need to add another
                _spotLayerRightRX2.Add(int.MinValue);
                return _spotLayerRightRX2.Count - 1;
            }
        }
        private static void updateLayer(int rx, int layer, int rightX)
        {
            if (layer < 0) return;
            if (rx == 1)
            {
                if(layer < _spotLayerRightRX1.Count)
                    _spotLayerRightRX1[layer] = rightX;
            }
            else
            {
                if (layer < _spotLayerRightRX2.Count)
                    _spotLayerRightRX2[layer] = rightX;
            }
        }
        private static bool _bShiftKeyDown = false;
        public static bool DisplayShiftKeyDown
        {
            get { return _bShiftKeyDown; }
            set { _bShiftKeyDown = value; }
        }
        private static string getCallsignString(SpotManager2.smSpot spot)
        {
            string sDisplayString;

            if (_bShiftKeyDown)
            {
                if (spot.spotter != "")
                    sDisplayString = spot.spotter;
                else
                    sDisplayString = spot.callsign;
            }
            else
            {
                sDisplayString = spot.callsign;
            }
            return sDisplayString;
        }
        public static void drawSpots(int rx, int nVerticalShift, int W, bool bottom)
        {
            if (bottom) return;

            long vfo_hz;
            int rxDisplayLow; 
            int rxDisplayHigh;

            bool local_mox = mox && ((rx == 1 && !tx_on_vfob) || (rx == 2 && tx_on_vfob) || (rx == 1 && tx_on_vfob && !console.RX2Enabled));

            if (rx == 1)
            {
                vfo_hz = (int)vfoa_hz;
                rxDisplayLow = RXDisplayLow;
                rxDisplayHigh = RXDisplayHigh;
                _spotLayerRightRX1.Clear();
            }
            else// if (rx == 2)
            {
                vfo_hz = (int)vfob_hz;
                rxDisplayLow = RX2DisplayLow;
                rxDisplayHigh = RX2DisplayHigh;
                _spotLayerRightRX2.Clear();
            }

            int yTop = nVerticalShift + 20;

            long vfoLow = vfo_hz + rxDisplayLow;    // low freq (left side) in hz
            long vfoHigh = vfo_hz + rxDisplayHigh; // high freq (right side) in hz
            long vfoDiff = vfoHigh - vfoLow;
            float HzToPixel = W / (float)vfoDiff;
            int cwShift = getCWSideToneShift(rx);

            List<SpotManager2.smSpot> sortedSpots = SpotManager2.GetFrequencySortedSpots(); //_spots.OrderBy(o => o.frequencyHZ).ToList();//.Where(f => f.frequencyHZ >= vfoLow && f.frequencyHZ <= vfoHigh && f.Enabled == true).ToList();

            SharpDX.Direct2D1.Brush spotColour;
            string sDisplayString;

            foreach (SpotManager2.smSpot spot in sortedSpots)
            {
                sDisplayString = getCallsignString(spot);

                spot.Size = measureStringDX2D(sDisplayString, fontDX2d_font9);

                int width = (int)spot.Size.Width;
                int height = (int)spot.Size.Height + 2;

                int halfWidth = width / 2;
                float x = (spot.frequencyHZ - vfoLow - cwShift) * HzToPixel;

                int leftX;
                int rightX;

                switch (spot.mode)
                {
                    case DSPMode.CWL:
                    case DSPMode.LSB:
                    case DSPMode.DIGL:
                    case DSPMode.AM_LSB:
                        leftX = (int)(x - width);
                        rightX = (int)x + 4;
                        break;
                    case DSPMode.CWU:
                    case DSPMode.USB:
                    case DSPMode.DIGU:
                    case DSPMode.AM_USB:
                        leftX = (int)x;
                        rightX = (int)(x + width + 4);
                        break;
                    default:
                        leftX = (int)(x - halfWidth - 2);
                        rightX = (int)(x + halfWidth + 2);
                        break;
                }

                int layer = getSpotLayer(rx, leftX);
                if (layer > -1)
                {
                    updateLayer(rx, layer, rightX);

                    // draw only if in view
                    if (spot.frequencyHZ >= vfoLow && spot.frequencyHZ <= vfoHigh)
                    {
                        int textY = yTop + 20 + (layer * height);

                        // used for mouse over + rectangle tag
                        spot.BoundingBoxInPixels[rx - 1].X = leftX - 1;
                        spot.BoundingBoxInPixels[rx - 1].Y = textY - 1;
                        spot.BoundingBoxInPixels[rx - 1].Width = (int)(spot.Size.Width + 2);
                        spot.BoundingBoxInPixels[rx - 1].Height = (int)(spot.Size.Height + 2);

                        if (spot.Highlight[rx - 1])
                        {
                            spotColour = getDXBrushForColour(spot.colour, 255);

                            drawLineDX2D(spotColour, x, yTop, x, textY, 3);
                            drawFillElipseDX2D(spotColour, x, yTop, 6, 6);
                        }
                        else
                        {
                            spotColour = getDXBrushForColour(spot.colour, 192);

                            drawLineDX2D(spotColour, x, yTop, x, textY, 1);
                            drawFillElipseDX2D(spotColour, x, yTop, 4, 4);
                        }
                        spot.Visible[rx - 1] = true;
                    }
                    else
                        spot.Visible[rx - 1] = false;
                }
            }


            // now plot all the tags over the lines if the spot is visible
            SharpDX.Direct2D1.Brush textBrush;
            SharpDX.Direct2D1.Brush whiteBrush = getDXBrushForColour(Color.White,255);
            SharpDX.Direct2D1.Brush blackBrush = getDXBrushForColour(Color.Black,255);
            SharpDX.Direct2D1.Brush brightBorder = getDXBrushForColour(Color.Yellow, 255);

            List<SpotManager2.smSpot> visibleSpots = sortedSpots.Where(o => o.Visible[rx - 1]).ToList();
            SpotManager2.smSpot highLightedSpot = null;
            foreach(SpotManager2.smSpot spot in visibleSpots)
            {
                sDisplayString = getCallsignString(spot);

                int nLuminance = getLuminance(spot.colour);
                spotColour = getDXBrushForColour(spot.colour, 255);
                textBrush = nLuminance <= 128 ? whiteBrush : blackBrush;

                if (spot.Highlight[rx - 1])
                {
                    highLightedSpot = spot;
                }
                else
                {
                    drawFillRectangleDX2D(spotColour, spot.BoundingBoxInPixels[rx - 1]);
                    drawStringDX2D(sDisplayString, fontDX2d_font9, textBrush, spot.BoundingBoxInPixels[rx - 1].X + 1, spot.BoundingBoxInPixels[rx - 1].Y + 1);
                }
            }

            if(highLightedSpot != null)
            {
                sDisplayString = getCallsignString(highLightedSpot);

                int nLuminance = getLuminance(highLightedSpot.colour);
                spotColour = getDXBrushForColour(highLightedSpot.colour, 255);
                textBrush = nLuminance <= 128 ? whiteBrush : blackBrush;

                Rectangle r = new Rectangle(highLightedSpot.BoundingBoxInPixels[rx - 1].X - 2, highLightedSpot.BoundingBoxInPixels[rx - 1].Y - 2,
                                        highLightedSpot.BoundingBoxInPixels[rx - 1].Width + 4, highLightedSpot.BoundingBoxInPixels[rx - 1].Height + 4);

                drawFillRectangleDX2D(spotColour, r);
                drawRectangleDX2D(brightBorder, r, 2);
                drawStringDX2D(sDisplayString, fontDX2d_font9, textBrush, highLightedSpot.BoundingBoxInPixels[rx - 1].X + 1, highLightedSpot.BoundingBoxInPixels[rx - 1].Y + 1);

                if (/*_bShiftKeyDown && */highLightedSpot.additionalText != "")
                {
                    // show additional text in bubble below
                    SizeF additionalTextSize = measureStringDX2D(highLightedSpot.additionalText, fontDX2d_font10);

                    int left = (r.X + (r.Width / 2)) - (int)(additionalTextSize.Width / 2);
                    int top = r.Y + r.Height + 6;

                    Rectangle additionalTextRect = new Rectangle(left - 2,
                                                top - 2,
                                                (int)(additionalTextSize.Width + 4),
                                                (int)(additionalTextSize.Height + 4)
                                               );

                    drawFillRectangleDX2D(getDXBrushForColour(Color.LightGray), additionalTextRect);
                    drawRectangleDX2D(getDXBrushForColour(Color.Black), additionalTextRect, 2);
                    drawStringDX2D(highLightedSpot.additionalText, fontDX2d_font10, getDXBrushForColour(Color.Black), additionalTextRect.X + 2, additionalTextRect.Y + 2);
                }
            }
        }
        private static int getLuminance(Color c)
        {
            //https://stackoverflow.com/questions/596216/formula-to-determine-perceived-brightness-of-rgb-color
            //return (int)(0.2126 * (float)c.R + 0.7152 * (float)c.G + 0.0722 * (float)c.B);
            int r = rGBtoLin(c.R);
            int g = rGBtoLin(c.G);
            int b = rGBtoLin(c.B);
            return (r + r + b + g + g + g) / 6; //(fast)
        }
        private static int rGBtoLin(int col)
        {
            float colorChannel = col / 255f;

            if (colorChannel <= 0.04045)
            {
                return (int)((colorChannel / 12.92) * 255f);
            }
            else
            {
                return (int)(Math.Pow(((colorChannel + 0.055) / 1.055), 2.4) * 255f);
            }
        }
        private static void clearAllDynamicBrushes()
        {
            if (!_bDX2Setup || _DX2Brushes == null) return;

            foreach (KeyValuePair<Color, SharpDX.Direct2D1.Brush> kvp in _DX2Brushes)
            {
                SharpDX.Direct2D1.Brush b = kvp.Value;
                Utilities.Dispose(ref b);
                b = null;
            }

            _DX2Brushes.Clear();
            _DX2Brushes = null;
        }
        public static int CachedDXBrushes
        {
            get { return _DX2Brushes == null ? 0 : _DX2Brushes.Count; }
        }
        private static SharpDX.Direct2D1.Brush getDXBrushForColour(Color c, int replaceAlpha = -1)
        {
            if (!_bDX2Setup) return null;

            if(_DX2Brushes == null) _DX2Brushes = new Dictionary<Color, SharpDX.Direct2D1.Brush>();

            Color newC;
            if (replaceAlpha >=0 && replaceAlpha <= 255)
                newC = Color.FromArgb(replaceAlpha, c.R, c.G, c.B); // override the alpha
            else
                newC = c;

            if (_DX2Brushes.ContainsKey(newC)) return _DX2Brushes[newC];

            SharpDX.Direct2D1.Brush b = convertBrush(new SolidBrush(newC));

            _DX2Brushes.Add(newC, b);

            return b;
        }

        private static bool _showTCISpots = false;
        public static bool ShowTCISpots
        {
            get { return _showTCISpots; }
            set { _showTCISpots = value; }
        }

        private static bool _bUseLegacyBuffers = false;
        public static bool UseLegacyBuffers
        {
            get { return _bUseLegacyBuffers; }
            set { _bUseLegacyBuffers = value; }
        }

        #region Ukraine
        private static bool _showFlag = true; // always shown at startup
        private static DateTime _lastFlagCheck = DateTime.MinValue;
        public static bool FlagShown
        {
            get { return _showFlag; }
            set { _showFlag = value; }
        }
        public static bool FlagHitBox(int x, int y)
        {
            return _showFlag && (x <= 175) && (y > displayTargetHeight - 100);
        }
        private static void ukraineFlag()
        {
            //#UKRAINE
            if ((DateTime.Now - _lastFlagCheck).TotalMinutes >= 60)
            {
                _lastFlagCheck = DateTime.Now;

                console.CheckIfRussian();

                if (console.IsRussian)
                    _showFlag = true; // every hour, replace the flag if russian
            }

            if (_showFlag)
            {
                RectangleF rectDestTop = new RectangleF(0, displayTargetHeight - 100, 175, 50);
                RectangleF rectDestBottom = new RectangleF(0, displayTargetHeight - 50, 175, 50);

                Color blue = Color.FromArgb(192, 56, 91, 185);
                Color yellow = Color.FromArgb(192, 241, 214, 36);
                _d2dRenderTarget.FillRectangle(rectDestTop, getDXBrushForColour(blue));
                _d2dRenderTarget.FillRectangle(rectDestBottom, getDXBrushForColour(yellow));
                drawStringDX2D("Stand up for Ukraine", fontDX2d_font9b, getDXBrushForColour(Color.White, 192), 30, displayTargetHeight - 75 - 10);
                drawStringDX2D("click to hide", fontDX2d_font9b, getDXBrushForColour(Color.White, 48), 54, displayTargetHeight - 55 - 10);
            }
        }
        #endregion
        #endregion
    }
}