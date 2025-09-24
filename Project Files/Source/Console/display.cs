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
//
//=================================================================
// Waterfall AGC Modifications Copyright (C) 2013 Phil Harman (VK6APH)
// Transitions to directX and continual modifications Copyright (C) 2020-2025 Richard Samphire (MW0LGE)
//=================================================================


using System.Linq;

namespace Thetis
{
    using System;
    using System.Runtime.CompilerServices;
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
    // SharpDX clashes
    using AlphaMode = SharpDX.Direct2D1.AlphaMode;
    using Device = SharpDX.Direct3D11.Device;
    using RectangleF = SharpDX.RectangleF;
    using SDXPixelFormat = SharpDX.Direct2D1.PixelFormat;   

    class Display
    {
        #region Variable Declaration

        private const AlphaMode ALPHA_MODE = AlphaMode.Premultiplied; //21k9

        public const float CLEAR_FLAG = -999.999F;				// for resetting buffers
        public const int BUFFER_SIZE = 16384;

        public static Console console;
        public static SpotControl SpotForm;                     // ke9ns add  communications with spot.cs and dx spotter
        public static string background_image = null;

        private static int[] histogram_data = null;				// histogram display buffer
        private static int[] histogram_history;					// histogram counter

        public static float[] new_display_data;					// Buffer used to store the new data from the DSP for the display
        public static float[] current_display_data;				// Buffer used to store the current data for the display
        public static float[] new_display_data_bottom;
        public static float[] current_display_data_bottom;

        public static float[] current_display_data_copy;
        public static float[] current_display_data_bottom_copy;

        //waterfall
        public static float[] new_waterfall_data;
        public static float[] current_waterfall_data;
        public static float[] new_waterfall_data_bottom;
        public static float[] current_waterfall_data_bottom;

        private static float[] waterfall_data;

        public static float[] current_waterfall_data_copy;
        public static float[] current_waterfall_data_bottom_copy;

        private static SharpDX.Direct2D1.Bitmap _waterfall_bmp_dx2d = null;					// MW0LGE
        private static SharpDX.Direct2D1.Bitmap _waterfall_bmp2_dx2d = null;
        #endregion

        #region Properties

        private static bool _testing_imd = false;
        public static bool TestingIMD
        {
            get { return _testing_imd; }
            set {  _testing_imd = value; }
        }
        private static bool _show_imd_measurements = false;
        public static bool ShowIMDMeasurments
        {
            get { return _show_imd_measurements; }
            set {
                if (value) FastAttackNoiseFloorRX1 = true;
                _show_imd_measurements = value; 
            }
        }

        //public static float FrameDelta { get; private set; }

        private static bool _tnf_active = true;
        public static bool TNFActive
        {
            get { return _tnf_active; }
            set
            {
                _tnf_active = value;
            }
        }

        private static bool m_bFrameRateIssue = false;
        public static bool FrameRateIssue
        {
            get { return m_bFrameRateIssue; }
            set { m_bFrameRateIssue = value; }
        }

        private static bool _bGetPixelsIssueRX1 = false;
        public static bool GetPixelsIssueRX1
        {
            get { return _bGetPixelsIssueRX1; }
            set { _bGetPixelsIssueRX1 = value; }
        }
        private static bool _bGetPixelsIssueRX2 = false;
        public static bool GetPixelsIssueRX2
        {
            get { return _bGetPixelsIssueRX2; }
            set { _bGetPixelsIssueRX2 = value; }
        }

        private static bool m_bShowFrameRateIssue = true;
        public static bool ShowFrameRateIssue
        {
            get { return m_bShowFrameRateIssue; }
            set { m_bShowFrameRateIssue = value; }
        }

        private static bool m_bShowGetPixelsIssue = false;
        public static bool ShowGetPixelsIssue
        {
            get { return m_bShowGetPixelsIssue; }
            set { m_bShowGetPixelsIssue = value; }
        }

        private static float m_fRX1WaterfallOpacity = 1f;
        public static float RX1WaterfallOpacity
        {
            get { return m_fRX1WaterfallOpacity; }
            set { m_fRX1WaterfallOpacity = value; }
        }
        private static float m_fRX2WaterfallOpacity = 1f;
        public static float RX2WaterfallOpacity
        {
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

        private static ColorScheme _rx1_color_scheme = ColorScheme.enhanced;
        public static ColorScheme RX1ColorScheme
        {
            get { return _rx1_color_scheme; }

            set 
            {
                _rx1_color_scheme = value; 
            }
        }

        private static ColorScheme _rx2_color_scheme = ColorScheme.enhanced;
        public static ColorScheme RX2ColorScheme
        {
            get { return _rx2_color_scheme; }

            set 
            {
                _rx2_color_scheme = value; 
            }
        }
        private static ColorScheme _tx_color_scheme = ColorScheme.enhanced;
        public static ColorScheme TXColorScheme
        {
            get { return _tx_color_scheme; }

            set
            {
                _tx_color_scheme = value;
            }
        }
        private static bool reverse_waterfall = false;
        public static bool ReverseWaterfall
        {
            get { return reverse_waterfall; }
            set { reverse_waterfall = value; }
        }

        private static bool pan_fill = false;
        public static bool PanFill
        {
            get { return pan_fill; }
            set { pan_fill = value; }
        }

        private static bool m_bSpectralPeakHoldRX1 = false;
        private static bool m_bSpectralPeakHoldRX2 = false;
        public static bool SpectralPeakHoldRX1
        {
            get { return m_bSpectralPeakHoldRX1; }
            set
            {
                m_bSpectralPeakHoldRX1 = value;
                if (m_bSpectralPeakHoldRX1)
                {
                    ResetSpectrumPeaks(1);
                }
            }
        }
        public static bool SpectralPeakHoldRX2
        {
            get { return m_bSpectralPeakHoldRX2; }
            set
            {
                m_bSpectralPeakHoldRX2 = value;
                if (m_bSpectralPeakHoldRX2)
                {
                    ResetSpectrumPeaks(2);
                }
            }
        }

        private static bool tx_pan_fill = false;
        public static bool TXPanFill
        {
            get { return tx_pan_fill; }
            set { tx_pan_fill = value; }
        }

        private static Color pan_fill_color = Color.FromArgb(100, 0, 0, 127);
        public static Color PanFillColor
        {
            get { return pan_fill_color; }
            set { pan_fill_color = value; }
        }

        private static bool _tx_on_vfob = false;
        public static bool TXOnVFOB
        {
            get { return _tx_on_vfob; }
            set
            {
                _tx_on_vfob = value;
            }
        }

        private static bool display_duplex = false;
        public static bool DisplayDuplex
        {
            get { return display_duplex; }
            set
            {
                if (_mox && value != display_duplex)
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
        public static bool SplitDisplay
        {
            get { return split_display; }
            set
            {
                lock (m_objSplitDisplayLock)
                {
                    split_display = value;
                }
            }
        }

        private static DisplayMode current_display_mode_bottom = DisplayMode.PANADAPTER;
        public static DisplayMode CurrentDisplayModeBottom
        {
            get { return current_display_mode_bottom; }
            set
            {
                bool bDifferent = current_display_mode_bottom != value;
                current_display_mode_bottom = value;
                if (bDifferent)
                {
                    lock (_objDX2Lock)
                    {
                        clearBuffers(displayTargetWidth, 2);
                    }
                    if (value == DisplayMode.PANAFALL || value == DisplayMode.WATERFALL)
                        ResetWaterfallBmp2();
                }
            }
        }

        private static int rx1_filter_low;
        public static int RX1FilterLow
        {
            get { return rx1_filter_low; }
            set { rx1_filter_low = value; }
        }

        private static int rx1_filter_high;
        public static int RX1FilterHigh
        {
            get { return rx1_filter_high; }
            set { rx1_filter_high = value; }
        }

        private static int rx2_filter_low;
        public static int RX2FilterLow
        {
            get { return rx2_filter_low; }
            set { rx2_filter_low = value; }
        }

        private static int rx2_filter_high;
        public static int RX2FilterHigh
        {
            get { return rx2_filter_high; }
            set { rx2_filter_high = value; }
        }

        private static int tx_filter_low;
        public static int TXFilterLow
        {
            get { return tx_filter_low; }
            set { tx_filter_low = value; }
        }

        private static int tx_filter_high;
        public static int TXFilterHigh
        {
            get { return tx_filter_high; }
            set { tx_filter_high = value; }
        }

        private static bool sub_rx1_enabled = false;
        public static bool SubRX1Enabled
        {
            get { return sub_rx1_enabled; }
            set
            {
                sub_rx1_enabled = value;
            }
        }

        private static bool split_enabled = false;
        public static bool SplitEnabled
        {
            get { return split_enabled; }
            set
            {
                split_enabled = value;
            }
        }

        private static bool show_freq_offset = false;
        public static bool ShowFreqOffset
        {
            get { return show_freq_offset; }
            set
            {
                show_freq_offset = value;
            }
        }

        private static bool show_zero_line = false;
        public static bool ShowZeroLine
        {
            get { return show_zero_line; }
            set
            {
                show_zero_line = value;
            }
        }

        private static double _mouseFrequency;
        public static double MouseFrequency
        {
            get { return _mouseFrequency; }
            set
            {
                _mouseFrequency = value;
            }
        }


        private static long vfoa_hz;
        public static long VFOA
        {
            get { return vfoa_hz; }
            set
            {
                vfoa_hz = value;
            }
        }

        private static long vfoa_sub_hz;
        public static long VFOASub //multi-rx freq
        {
            get { return vfoa_sub_hz; }
            set
            {
                vfoa_sub_hz = value;
            }
        }

        private static long vfob_hz;
        public static long VFOB //split tx freq
        {
            get { return vfob_hz; }
            set
            {
                vfob_hz = value;
            }
        }

        private static long vfob_sub_hz;
        public static long VFOBSub
        {
            get { return vfob_sub_hz; }
            set
            {
                vfob_sub_hz = value;
            }
        }

        private static int rx_display_bw;
        public static int RXDisplayBW
        {
            get { return rx_display_bw; }
            set
            {
                rx_display_bw = value;
            }
        }

        private static int rit_hz;
        public static int RIT
        {
            get { return rit_hz; }
            set
            {
                rit_hz = value;
            }
        }

        private static int xit_hz;
        public static int XIT
        {
            get { return xit_hz; }
            set
            {
                xit_hz = value;
            }
        }

        private static int freq_diff = 0;
        public static int FreqDiff
        {
            get { return freq_diff; }
            set
            {
                freq_diff = value;
            }
        }

        private static int rx2_freq_diff = 0;
        public static int RX2FreqDiff
        {
            get { return rx2_freq_diff; }
            set
            {
                rx2_freq_diff = value;
            }
        }

        private static double m_dSpecralPeakHoldDelayRX1 = 100;
        private static double m_dSpecralPeakHoldDelayRX2 = 100;
        public static double SpectralPeakHoldDelayRX1
        {
            get { return m_dSpecralPeakHoldDelayRX1; }
            set { m_dSpecralPeakHoldDelayRX1 = value; }
        }
        public static double SpectralPeakHoldDelayRX2
        {
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
            // get initial state data from console that might get modified by delegate
            _rx1ClickDisplayCTUN = console.ClickTuneDisplay;
            _rx2ClickDisplayCTUN = console.ClickTuneRX2Display;
            //

            console.PowerChangeHanders += OnPowerChangeHander;
            console.BandChangeHandlers += OnBandChangeHandler;
            console.AttenuatorDataChangedHandlers += OnAttenuatorDataChanged;
            console.PreampModeChangedHandlers += OnPreampModeChanged;
            console.CentreFrequencyHandlers += OnCentreFrequencyChanged;
            console.CTUNChangedHandlers += OnCTUNChanged;
            console.MinimumRXNotchWidthChangedHandlers += OnMinRXNotchWidthChanged;
            console.MinimumTXNotchWidthChangedHandlers += OnMinTXNotchWidthChanged;

            console.WaterfallRXGradientChangedHandlers += OnWaterfallRXGradientChanged;
            console.WaterfallTXGradientChangedHandlers += OnWaterfallTXGradientChanged;
        }
        public static void RemoveDelegates()
        {
            console.PowerChangeHanders -= OnPowerChangeHander;
            console.BandChangeHandlers -= OnBandChangeHandler;
            console.AttenuatorDataChangedHandlers -= OnAttenuatorDataChanged;
            console.PreampModeChangedHandlers -= OnPreampModeChanged;
            console.CentreFrequencyHandlers -= OnCentreFrequencyChanged;
            console.CTUNChangedHandlers -= OnCTUNChanged;
            console.MinimumRXNotchWidthChangedHandlers -= OnMinRXNotchWidthChanged;
            console.MinimumTXNotchWidthChangedHandlers -= OnMinTXNotchWidthChanged;

            console.WaterfallRXGradientChangedHandlers -= OnWaterfallRXGradientChanged;
            console.WaterfallTXGradientChangedHandlers -= OnWaterfallTXGradientChanged;
        }
        private static void OnMinRXNotchWidthChanged(int rx, double width)
        {
            _mnfMinSizeRX = width;
        }
        private static void OnMinTXNotchWidthChanged(double width)
        {
            _mnfMinSizeTX = width;
        }
        private static bool _rx1ClickDisplayCTUN = false;
        private static bool _rx2ClickDisplayCTUN = false;
        private static void OnCTUNChanged(int rx, bool oldCTUN, bool newCTUN, Band band)
        {
            if (rx == 1)
                _rx1ClickDisplayCTUN = newCTUN;
            else if (rx == 2)
                _rx2ClickDisplayCTUN = newCTUN;
        }
        private static void OnPowerChangeHander(bool oldPower, bool newPower)
        {
            if (newPower)
            {
                 PurgeBuffers();
            }
        }
        private static void OnBandChangeHandler(int rx, Band oldBand, Band newBand)
        {
            if (rx == 1)
            {
                FastAttackNoiseFloorRX1 = true;
                _RX1waterfallPreviousMinValue = 20;
            }
            else
            {
                FastAttackNoiseFloorRX2 = true;
                _RX2waterfallPreviousMinValue = 20;
            }
        }

        private static bool m_bDelayRX1Blobs = false;
        private static bool m_bDelayRX2Blobs = false;
        private static bool m_bDelayRX1SpectrumPeaks = false;
        private static bool m_bDelayRX2SpectrumPeaks = false;
        private static double m_dPeakDelay = 0;

        private static void processBlobsActivePeakDisplayDelay()
        {
            if (!m_bDelayRX1Blobs && !m_bDelayRX2Blobs && !m_bDelayRX1SpectrumPeaks && !m_bDelayRX2SpectrumPeaks) return;

            if (m_dElapsedFrameStart > m_dPeakDelay)
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

        private static void resetPeaksAndNoise(int rx)
        {
            ResetBlobMaximums(rx, true);
            ResetSpectrumPeaks(rx);

            if (rx == 1)
            {
                FastAttackNoiseFloorRX1 = true;
            }
            else if (rx == 2)
            {
                FastAttackNoiseFloorRX2 = true;
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
        private static void OnCentreFrequencyChanged(int rx, double oldFreq, double newFreq, Band band, double offset)
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
            set
            {
                m_bNoiseFloorGoodRX1 = false;
                if(value) _fLastFastAttackEnabledTimeRX1 = m_objFrameStartTimer.ElapsedMsec;
                m_bFastAttackNoiseFloorRX1 = value;
            }
        }
        public static bool FastAttackNoiseFloorRX2
        {
            get { return m_bFastAttackNoiseFloorRX2; }
            set
            {
                m_bNoiseFloorGoodRX2 = false;
                if(value) _fLastFastAttackEnabledTimeRX2 = m_objFrameStartTimer.ElapsedMsec;
                m_bFastAttackNoiseFloorRX2 = value;
            }
        }

        private static double m_dCentreFreqRX1 = 0;
        public static double CentreFreqRX1
        {
            get { return m_dCentreFreqRX1; }
            set
            {
                m_dCentreFreqRX1 = value;
            }
        }

        private static double m_dCentreFreqRX2 = 0;
        public static double CentreFreqRX2
        {
            get { return m_dCentreFreqRX2; }
            set
            {
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
        public static int CWPitch
        {
            get { return cw_pitch; }
            set { cw_pitch = value; }
        }

        private static int m_nPhasePointSize = 1;
        public static int PhasePointSize
        {
            get { return m_nPhasePointSize; }
            set { m_nPhasePointSize = value; }
        }
        private static bool m_bShowFPS = false;
        public static bool ShowFPS
        {
            get { return m_bShowFPS; }
            set { m_bShowFPS = value; }
        }
        private static double _fps_profile_start = double.MinValue;
        private static bool _runningFPSProfile = false;
        public static bool RunningFPSProfile
        {
            get { return _runningFPSProfile; }
            set 
            {
                _runningFPSProfile = value;
                if (_runningFPSProfile)
                {
                    _fps_profile_start = m_dElapsedFrameStart;
                }
                else
                {
                    _fps_profile_start = double.MinValue;
                }
            }
        }
        private static bool m_bShowVisualNotch = false;
        public static bool ShowVisualNotch
        {
            get { return m_bShowVisualNotch; }
            set { m_bShowVisualNotch = value; }
        }
        //=======================================================

        private static bool m_bSpecialPanafall = false; // ke9ns add 1=map mode (panafall but only a small waterfall) and only when just in RX1 mode)

        //========================================================

        public static bool specready = false;
        private static int displayTargetHeight = 0;	// target height
        private static int displayTargetWidth = 0;	// target width
        private static Control displayTarget = null;
        private static double _mnfMinSizeRX = 100;
        private static double _mnfMinSizeTX = 100;

        private static string _cpu;
        private static string _gpu;
        private static string _ram;
        private static string _installed_ram;

        public static Control Target
        {
            get { return displayTarget; }
            set
            {
                lock (_objDX2Lock)
                {
                    _cpu = Common.GetCpuName();
                    //List<string> gpus = Common.GetGpuNames();
                    //_gpu = gpus[0]; // assume we are using the first
                    _ram = Common.GetTotalRam();
                    _installed_ram = Common.GetInstalledRam();

                    displayTarget = value;

                    displayTargetHeight = displayTarget.Height;
                    displayTargetWidth = Math.Min(displayTarget.Width, BUFFER_SIZE);

                    initDisplayArrays(displayTargetWidth, displayTargetHeight);

                    //UpdateMNFminWidth();
                    _mnfMinSizeRX = console.GetMinimumRXNotchWidth(1); // just for rx1
                    _mnfMinSizeTX = console.GetMinimumTXNotchWidth();

                    if (!_bDX2Setup)
                    {
                        initDX2D();
                    }
                    else
                    {
                        resizeDX2D();
                        ResetWaterfallBmp();
                        ResetWaterfallBmp2();
                    }                    

                    Audio.ScopePixelWidth = displayTargetWidth;

                    if (specready)
                    {
                        console.specRX.GetSpecRX(0).Pixels = displayTargetWidth / m_nDecimation;
                        console.specRX.GetSpecRX(1).Pixels = displayTargetWidth / m_nDecimation;
                        console.specRX.GetSpecRX(cmaster.inid(1, 0)).Pixels = displayTargetWidth / m_nDecimation;
                    }

#if SNOWFALL
                    if (_snowFall)
                    {
                        lock (_snowLock)
                        {
                            _snow.Clear();
                            _showSanta = false;
                        }
                    }
#endif
                }
            }
        }
        public static Size TargetSize
        {
            get
            {
                if(displayTarget == null)
                {
                    return Size.Empty;
                }
                else
                {
                    return new Size(displayTargetWidth, displayTargetHeight);
                }
            }
        }
        //public static void UpdateMNFminWidth()
        //{
        //    unsafe
        //    {
        //        fixed (double* ptr = &_mnfMinSize)
        //        {
        //            WDSP.RXANBPGetMinNotchWidth(0, ptr);
        //        }
        //    }
        //    Debug.Print("min notch width = " + _mnfMinSize.ToString());
        //}

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
        public static int RXDisplayLow
        {
            get { return rx_display_low; }
            set
            {
                if (value != rx_display_low)
                {
                    ResetBlobMaximums(1, true);
                    ResetSpectrumPeaks(1);
                }
                rx_display_low = value;
            }
        }

        private static int rx_display_high = 4000;
        public static int RXDisplayHigh
        {
            get { return rx_display_high; }
            set
            {
                if (value != rx_display_high)
                {
                    ResetBlobMaximums(1, true);
                    ResetSpectrumPeaks(1);
                }
                rx_display_high = value;
            }
        }

        private static int rx2_display_low = -4000;
        public static int RX2DisplayLow
        {
            get { return rx2_display_low; }
            set
            {
                if (value != rx2_display_low)
                {
                    ResetBlobMaximums(2, true);
                    ResetSpectrumPeaks(2);
                }
                rx2_display_low = value;
            }
        }

        private static int rx2_display_high = 4000;
        public static int RX2DisplayHigh
        {
            get { return rx2_display_high; }
            set
            {
                if (value != rx2_display_high)
                {
                    ResetBlobMaximums(2, true);
                    ResetSpectrumPeaks(2);
                }
                rx2_display_high = value;
            }
        }

        private static int tx_display_low = -4000;
        public static int TXDisplayLow
        {
            get { return tx_display_low; }
            set { tx_display_low = value; }
        }

        private static int tx_display_high = 4000;
        public static int TXDisplayHigh
        {
            get { return tx_display_high; }
            set { tx_display_high = value; }
        }

        private static int rx_spectrum_display_low = -4000;
        public static int RXSpectrumDisplayLow
        {
            get { return rx_spectrum_display_low; }
            set { rx_spectrum_display_low = value; }
        }

        private static int rx_spectrum_display_high = 4000;
        public static int RXSpectrumDisplayHigh
        {
            get { return rx_spectrum_display_high; }
            set { rx_spectrum_display_high = value; }
        }

        private static int rx2_spectrum_display_low = -4000;
        public static int RX2SpectrumDisplayLow
        {
            get { return rx2_spectrum_display_low; }
            set { rx2_spectrum_display_low = value; }
        }

        private static int rx2_spectrum_display_high = 4000;
        public static int RX2SpectrumDisplayHigh
        {
            get { return rx2_spectrum_display_high; }
            set { rx2_spectrum_display_high = value; }
        }

        private static int tx_spectrum_display_low = -4000;
        public static int TXSpectrumDisplayLow
        {
            get { return tx_spectrum_display_low; }
            set { tx_spectrum_display_low = value; }
        }

        private static int tx_spectrum_display_high = 4000;
        public static int TXSpectrumDisplayHigh
        {
            get { return tx_spectrum_display_high; }
            set { tx_spectrum_display_high = value; }
        }

        private static float rx1_preamp_offset = 0.0f;
        public static float RX1PreampOffset
        {
            get { return rx1_preamp_offset; }
            set { rx1_preamp_offset = value; }
        }

        private static float alex_preamp_offset = 0.0f;
        public static float AlexPreampOffset
        {
            get { return alex_preamp_offset; }
            set { alex_preamp_offset = value; }
        }

        private static float rx2_preamp_offset = 0.0f;
        public static float RX2PreampOffset
        {
            get { return rx2_preamp_offset; }
            set { rx2_preamp_offset = value; }
        }
        private static float tx_attenuator_offset = 0.0f;
        public static float TXAttenuatorOffset
        {
            get { return tx_attenuator_offset; }
            set { tx_attenuator_offset = value; }
        }
        private static bool tx_display_cal_control = false;
        public static bool TXDisplayCalControl
        {
            get { return tx_display_cal_control; }
            set { tx_display_cal_control = value; }
        }

        private static float rx1_display_cal_offset;					// display calibration offset in dB
        public static float RX1DisplayCalOffset
        {
            get { return rx1_display_cal_offset; }
            set
            {
                rx1_display_cal_offset = value;
            }
        }

        private static float rx2_display_cal_offset;					// display calibration offset in dB
        public static float RX2DisplayCalOffset
        {
            get { return rx2_display_cal_offset; }
            set { rx2_display_cal_offset = value; }
        }

        private static float rx1_fft_size_offset;					// display calibration offset in dB
        public static float RX1FFTSizeOffset
        {
            get { return rx1_fft_size_offset; }
            set
            {
                rx1_fft_size_offset = value;
            }
        }

        private static float rx2_fft_size_offset;					// display calibration offset in dB
        public static float RX2FFTSizeOffset
        {
            get { return rx2_fft_size_offset; }
            set { rx2_fft_size_offset = value; }
        }


        private static float tx_display_cal_offset = 0f;					// display calibration offset in dB
        public static float TXDisplayCalOffset
        {
            get { return tx_display_cal_offset; }
            set
            {
                tx_display_cal_offset = value;
            }
        }

        //private static HPSDRModel _current_hpsdr_model = HPSDRModel.ANAN7000D;
        //public static HPSDRModel CurrentHPSDRModel
        //{
        //    get { return _current_hpsdr_model; }
        //    set { _current_hpsdr_model = value; }
        //}

        private static int display_cursor_x;						// x-coord of the cursor when over the display
        public static int DisplayCursorX
        {
            get { return display_cursor_x; }
            set { display_cursor_x = value; }
        }

        private static int display_cursor_y;						// y-coord of the cursor when over the display
        public static int DisplayCursorY
        {
            get { return display_cursor_y; }
            set { display_cursor_y = value; }
        }

        private static bool _grid_control_major = false;
        public static bool GridControlMajor
        {
            get { return _grid_control_major; }
            set { _grid_control_major = value; }
        }
        private static bool _grid_control_minor = false;
        public static bool GridControlMinor
        {
            get { return _grid_control_minor; }
            set { _grid_control_minor = value; }
        }
        private static bool _show_frequency_numbers = true;
        public static bool ShowFrequencyNumbers
        {
            get { return _show_frequency_numbers; }
            set { _show_frequency_numbers = value; }
        }
        private static bool show_agc = false;
        public static bool ShowAGC
        {
            get { return show_agc; }
            set { show_agc = value; }
        }

        private static bool spectrum_line = false;
        public static bool SpectrumLine
        {
            get { return spectrum_line; }
            set { spectrum_line = value; }
        }

        private static bool display_agc_hang_line = false;
        public static bool DisplayAGCHangLine
        {
            get { return display_agc_hang_line; }
            set { display_agc_hang_line = value; }
        }

        private static bool rx1_hang_spectrum_line = false;
        public static bool RX1HangSpectrumLine
        {
            get { return rx1_hang_spectrum_line; }
            set { rx1_hang_spectrum_line = value; }
        }

        private static bool display_rx2_gain_line = false;
        public static bool DisplayRX2GainLine
        {
            get { return display_rx2_gain_line; }
            set { display_rx2_gain_line = value; }
        }

        private static bool rx2_gain_spectrum_line = false;
        public static bool RX2GainSpectrumLine
        {
            get { return rx2_gain_spectrum_line; }
            set { rx2_gain_spectrum_line = value; }
        }

        private static bool display_rx2_hang_line = false;
        public static bool DisplayRX2HangLine
        {
            get { return display_rx2_hang_line; }
            set { display_rx2_hang_line = value; }
        }

        private static bool rx2_hang_spectrum_line = false;
        public static bool RX2HangSpectrumLine
        {
            get { return rx2_hang_spectrum_line; }
            set { rx2_hang_spectrum_line = value; }
        }

        private static bool tx_grid_control = false;
        public static bool TXGridControl
        {
            get { return tx_grid_control; }
            set { tx_grid_control = value; }
        }

        private static ClickTuneMode current_click_tune_mode = ClickTuneMode.Off;
        public static ClickTuneMode CurrentClickTuneMode
        {
            get { return current_click_tune_mode; }
            set { current_click_tune_mode = value; }
        }

        private static int scope_time = 50;
        public static int ScopeTime
        {
            get { return scope_time; }
            set { scope_time = value; }
        }

        private static int sample_rate_rx1 = 384000;
        public static int SampleRateRX1
        {
            get { return sample_rate_rx1; }
            set { sample_rate_rx1 = value; }
        }

        private static int sample_rate_rx2 = 384000;
        public static int SampleRateRX2
        {
            get { return sample_rate_rx2; }
            set { sample_rate_rx2 = value; }
        }

        private static int sample_rate_tx = 192000;
        public static int SampleRateTX
        {
            get { return sample_rate_tx; }
            set { sample_rate_tx = value; }
        }

        private static bool high_swr = false;
        public static bool HighSWR
        {
            get { return high_swr; }
            set { high_swr = value; }
        }
        private static bool _power_folded_back = false;
        public static bool PowerFoldedBack
        {
            get { return _power_folded_back; }
            set { _power_folded_back = value; }
        }        

        private static bool _old_mox = false;
        private static bool _mox = false;
        public static bool MOX
        {
            get { return _mox; }
            set
            {
                lock (_objDX2Lock)
                {
                    if (value != _old_mox)
                    {
                        PurgeBuffers();
                        _old_mox = value;
                    }
                    _mox = value;
                }
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
        public static bool BlankBottomDisplay
        {
            get { return blank_bottom_display; }
            set { blank_bottom_display = value; }
        }

        private static DSPMode rx1_dsp_mode = DSPMode.USB;
        public static DSPMode RX1DSPMode
        {
            get { return rx1_dsp_mode; }
            set { rx1_dsp_mode = value; }
        }

        private static DSPMode rx2_dsp_mode = DSPMode.USB;
        public static DSPMode RX2DSPMode
        {
            get { return rx2_dsp_mode; }
            set { rx2_dsp_mode = value; }
        }

        private static MNotch m_objHightlightedNotch;
        public static MNotch HighlightNotch
        {
            get { return m_objHightlightedNotch; }
            set { m_objHightlightedNotch = value; }
        }

        private static DisplayMode current_display_mode = DisplayMode.PANAFALL;
        public static DisplayMode CurrentDisplayMode
        {
            get { return current_display_mode; }
            set
            {
                bool bDifferent = current_display_mode != value;
                current_display_mode = value;

                if (console.PowerOn)
                    console._pause_DisplayThread = true;

                if (bDifferent)
                {
                    lock (_objDX2Lock)
                    {
                        clearBuffers(displayTargetWidth, 1);
                    }
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
                console._pause_DisplayThread = false;
            }
        }

        private static float max_x;								// x-coord of maxmimum over one display pass
        public static float MaxX
        {
            get { return max_x; }
            set { max_x = value; }
        }

        private static float max_y;								// y-coord of maxmimum over one display pass
        public static float MaxY
        {
            get { return max_y; }
            set { max_y = value; }
        }

        private static bool _rx2_enabled = false;
        public static bool RX2Enabled
        {
            get { return _rx2_enabled; }
            set { _rx2_enabled = value; }
        }

        private static bool _bRebuildRXLinearGradBrush = true;
        public static bool RebuildLinearGradientBrushRX
        {
            get { return _bRebuildRXLinearGradBrush; }
            set
            {
                _bRebuildRXLinearGradBrush = value;
            }
        }
        private static bool _bRebuildTXLinearGradBrush = true;
        public static bool RebuildLinearGradientBrushTX
        {
            get { return _bRebuildTXLinearGradBrush; }
            set
            {
                _bRebuildTXLinearGradBrush = value;
            }
        }
        private static bool data_ready;					// True when there is new display data ready from the DSP
        public static bool DataReady
        {
            get { return data_ready; }
            set { data_ready = value; }
        }

        private static bool data_ready_bottom;
        public static bool DataReadyBottom
        {
            get { return data_ready_bottom; }
            set { data_ready_bottom = value; }
        }

        private static bool waterfall_data_ready_bottom;
        public static bool WaterfallDataReadyBottom
        {
            get { return waterfall_data_ready_bottom; }
            set { waterfall_data_ready_bottom = value; }
        }

        private static bool waterfall_data_ready;
        public static bool WaterfallDataReady
        {
            get { return waterfall_data_ready; }
            set { waterfall_data_ready = value; }
        }        
        private static int spectrum_grid_max = -40;
        public static int SpectrumGridMax
        {
            get { return spectrum_grid_max; }
            set
            {
                if (value != spectrum_grid_max) _bRebuildRXLinearGradBrush = true;
                spectrum_grid_max = value;
            }
        }

        private static int spectrum_grid_min = -140;
        public static int SpectrumGridMin
        {
            get { return spectrum_grid_min; }
            set
            {
                if (value != spectrum_grid_min) _bRebuildRXLinearGradBrush = true;
                spectrum_grid_min = value;
            }
        }

        private static int spectrum_grid_step = 5;
        public static int SpectrumGridStep
        {
            get { return spectrum_grid_step; }
            set
            {
                if (value != spectrum_grid_step) _bRebuildRXLinearGradBrush = true;
                spectrum_grid_step = value;
            }
        }

        public static int SpectrumGridMaxMoxModified
        {
            get
            {
                bool local_mox = localMox(1);
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
                bool local_mox = localMox(1);
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
                bool local_mox = localMox(1);
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
                bool local_mox = localMox(2);
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
                bool local_mox = localMox(2);
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
                bool local_mox = localMox(2);
                if (local_mox)
                    return tx_spectrum_grid_step;
                else
                    return rx2_spectrum_grid_step;
            }
        }
        //

        private static int rx2_spectrum_grid_max = -40;
        public static int RX2SpectrumGridMax
        {
            get { return rx2_spectrum_grid_max; }
            set
            {
                if (value != rx2_spectrum_grid_max) _bRebuildRXLinearGradBrush = true;
                rx2_spectrum_grid_max = value;
            }
        }

        private static int rx2_spectrum_grid_min = -140;
        public static int RX2SpectrumGridMin
        {
            get { return rx2_spectrum_grid_min; }
            set
            {
                if (value != rx2_spectrum_grid_min) _bRebuildRXLinearGradBrush = true;
                rx2_spectrum_grid_min = value;
            }
        }

        private static int rx2_spectrum_grid_step = 5;
        public static int RX2SpectrumGridStep
        {
            get { return rx2_spectrum_grid_step; }
            set
            {
                if (value != rx2_spectrum_grid_step) _bRebuildRXLinearGradBrush = true;
                rx2_spectrum_grid_step = value;
            }
        }

        private static int tx_spectrum_grid_max = 20;
        public static int TXSpectrumGridMax
        {
            get { return tx_spectrum_grid_max; }
            set
            {
                tx_spectrum_grid_max = value;
            }
        }

        private static int tx_spectrum_grid_min = -80;
        public static int TXSpectrumGridMin
        {
            get { return tx_spectrum_grid_min; }
            set
            {
                tx_spectrum_grid_min = value;
            }
        }

        private static int tx_spectrum_grid_step = 5;
        public static int TXSpectrumGridStep
        {
            get { return tx_spectrum_grid_step; }
            set
            {
                tx_spectrum_grid_step = value;
            }
        }

        private static int tx_wf_amp_max = 30;
        public static int TXWFAmpMax
        {
            get { return tx_wf_amp_max; }
            set
            {
                tx_wf_amp_max = value;
                if (console != null)
                {
                    console.CheckForMinMaxWaterfallUpdatesTX();
                }
            }
        }

        private static int tx_wf_amp_min = -70;
        public static int TXWFAmpMin
        {
            get { return tx_wf_amp_min; }
            set
            {
                tx_wf_amp_min = value;
                if (console != null)
                {
                    console.CheckForMinMaxWaterfallUpdatesTX();
                }
            }
        }

        private static Color band_edge_color = Color.Red;
        private static Pen band_edge_pen = new Pen(band_edge_color);
        public static Color BandEdgeColor
        {
            get { return band_edge_color; }
            set
            {
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
        public static Color TXBandEdgeColor
        {
            get { return tx_band_edge_color; }
            set
            {
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
        public static Color SubRXZeroLine
        {
            get { return sub_rx_zero_line_color; }
            set
            {
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
        public static Color SubRXFilterColor
        {
            get { return sub_rx_filter_color; }
            set
            {
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
        public static Color GridTextColor
        {
            get { return grid_text_color; }
            set
            {
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
        public static Color GridTXTextColor
        {
            get { return grid_tx_text_color; }
            set
            {
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
        public static Color GridZeroColor
        {
            get { return grid_zero_color; }
            set
            {
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
        public static Color TXGridZeroColor
        {
            get { return tx_grid_zero_color; }
            set
            {
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
        public static Color GridColor
        {
            get { return grid_color; }
            set
            {
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
        public static Color TXVGridColor
        {
            get { return tx_vgrid_color; }
            set
            {
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
        public static Color HGridColor
        {
            get { return hgrid_color; }
            set
            {
                lock (_objDX2Lock)
                {
                    hgrid_color = value;
                    hgrid_pen.Color = hgrid_color;
                    buildDX2Resources();
                }
            }
        }


        private static Color tx_hgrid_color = Color.White;
        private static Pen tx_hgrid_pen = new Pen(tx_hgrid_color);
        public static Color TXHGridColor
        {
            get { return tx_hgrid_color; }
            set
            {
                lock (_objDX2Lock)
                {
                    tx_hgrid_color = value;
                    tx_hgrid_pen.Color = tx_hgrid_color;
                    buildDX2Resources();
                }
            }
        }

        //MW0LGE
        private static Pen peak_blob_pen = new Pen(Color.OrangeRed);
        private static Pen peak_blob_text_pen = new Pen(Color.YellowGreen);
        private static Color data_fill_color = Color.FromArgb(128, Color.Blue);
        private static Color data_fill_color_tx = Color.FromArgb(128, Color.DarkRed);
        private static Color dataPeaks_fill_color = Color.FromArgb(128, Color.Gray);
        private static Pen data_fill_fpen = new Pen(data_fill_color);
        private static Pen data_fill_fpen_tx = new Pen(data_fill_color_tx);
        private static Pen dataPeaks_fill_fpen = new Pen(dataPeaks_fill_color);
        public static Color DataFillColor
        {
            get { return data_fill_color; }
            set
            {
                lock (_objDX2Lock)
                {
                    data_fill_color = value;
                    data_fill_fpen.Color = data_fill_color;
                    buildDX2Resources();
                }
            }
        }
        public static Color DataFillColorTX
        {
            get { return data_fill_color_tx; }
            set
            {
                lock (_objDX2Lock)
                {
                    data_fill_color_tx = value;
                    data_fill_fpen_tx.Color = data_fill_color_tx;
                    buildDX2Resources();
                }
            }
        }
        public static Color DataPeaksFillColor
        {
            get { return dataPeaks_fill_color; }
            set
            {
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
        public static Color DataLineColor
        {
            get { return data_line_color; }
            set
            {
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
        public static Color TXDataLineColor
        {
            get { return tx_data_line_color; }
            set
            {
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
        public static Color GridPenDark
        {
            get { return grid_pen_dark; }
            set
            {
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
        public static Color TXVGridPenFine
        {
            get { return tx_vgrid_pen_fine; }
            set
            {
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
        private static SolidBrush bandstack_overlay_brush_highlight = new SolidBrush(Color.FromArgb(Math.Min(bandstack_overlay_color.A + 64, 255), bandstack_overlay_color));
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
        public static Color DisplayFilterColor
        {
            get { return display_filter_color; }
            set
            {
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
        public static Color TXFilterColor
        {
            get { return tx_filter_color; }
            set
            {
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
        private static Color noisefloor_color_text = Color.Yellow;
        public static Color NoiseFloorColorText
        {
            get { return noisefloor_color_text; }
            set
            {
                lock (_objDX2Lock)
                {
                    noisefloor_color_text = value;
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
        public static Color DisplayFilterTXColor
        {
            get { return display_filter_tx_color; }
            set
            {
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
        public static Color DisplayBackgroundColor
        {
            get { return display_background_color; }
            set
            {
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
        public static Color TXDisplayBackgroundColor
        {
            get { return tx_display_background_color; }
            set
            {
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
        public static bool ShowTXFilterOnWaterfall
        {
            get { return m_bShowTXFilterOnWaterfall; }
            set
            {
                m_bShowTXFilterOnWaterfall = value;
            }
        }
        private static bool m_bShowRXFilterOnWaterfall = false;
        public static bool ShowRXFilterOnWaterfall
        {
            get { return m_bShowRXFilterOnWaterfall; }
            set
            {
                m_bShowRXFilterOnWaterfall = value;
            }
        }
        private static bool m_bShowTXZeroLineOnWaterfall = false;
        public static bool ShowTXZeroLineOnWaterfall
        {
            get { return m_bShowTXZeroLineOnWaterfall; }
            set
            {
                m_bShowTXZeroLineOnWaterfall = value;
            }
        }
        private static bool m_bShowRXZeroLineOnWaterfall = false;
        public static bool ShowRXZeroLineOnWaterfall
        {
            get { return m_bShowRXZeroLineOnWaterfall; }
            set
            {
                m_bShowRXZeroLineOnWaterfall = value;
            }
        }
        private static bool m_bShowTXFilterOnRXWaterfall = false;
        public static bool ShowTXFilterOnRXWaterfall
        {
            get { return m_bShowTXFilterOnRXWaterfall; }
            set
            {
                m_bShowTXFilterOnRXWaterfall = value;
            }
        }

        private static bool draw_tx_filter = false;
        public static bool DrawTXFilter
        {
            get { return draw_tx_filter; }
            set
            {
                draw_tx_filter = value;
            }
        }

        private static bool show_cwzero_line = false;
        public static bool ShowCWZeroLine
        {
            get { return show_cwzero_line; }
            set
            {
                show_cwzero_line = value;
            }
        }

        private static bool draw_tx_cw_freq = false;
        public static bool DrawTXCWFreq
        {
            get { return draw_tx_cw_freq; }
            set
            {
                draw_tx_cw_freq = value;
            }
        }

        private static Color waterfall_low_color = Color.Black;
        public static Color WaterfallLowColor
        {
            get { return waterfall_low_color; }
            set { waterfall_low_color = value; }
        }
        private static Color waterfall_low_color_tx = Color.Black;
        public static Color WaterfallLowColorTX
        {
            get { return waterfall_low_color_tx; }
            set { waterfall_low_color_tx = value; }
        }
        private static float waterfall_high_threshold = -80.0F;
        public static float WaterfallHighThreshold
        {
            get { return waterfall_high_threshold; }
            set 
            { 
                waterfall_high_threshold = value;
                if (console != null)
                {
                    console.CheckForMinMaxWaterfallUpdatesRX(1);
                }
            }
        }

        private static float waterfall_low_threshold = -130.0F;
        public static float WaterfallLowThreshold
        {
            get { return waterfall_low_threshold; }
            set 
            { 
                waterfall_low_threshold = value;
                if (console != null)
                {
                    console.CheckForMinMaxWaterfallUpdatesRX(1);
                }
            }
        }

        //================================================================
        // ke9ns add signal from console about Grayscale ON/OFF
        private static byte Gray_Scale = 0; //  ke9ns ADD from console 0=RGB  1=Gray
        public static byte GrayScale       // this is called or set in console
        {
            get { return Gray_Scale; }
            set
            {
                Gray_Scale = value;
            }
        }


        //================================================================
        // kes9ns add signal from setup grid lines on/off
        private static byte grid_off = 0; //  ke9ns ADD from setup 0=normal  1=gridlines off
        public static byte GridOff       // this is called or set in setup
        {
            get { return grid_off; }
            set
            {
                grid_off = value;
            }
        }


        private static Color rx2_waterfall_low_color = Color.Black;
        public static Color RX2WaterfallLowColor
        {
            get { return rx2_waterfall_low_color; }
            set { rx2_waterfall_low_color = value; }
        }

        private static float rx2_waterfall_high_threshold = -80.0F;
        public static float RX2WaterfallHighThreshold
        {
            get { return rx2_waterfall_high_threshold; }
            set 
            { 
                rx2_waterfall_high_threshold = value;
                if (console != null)
                {
                    console.CheckForMinMaxWaterfallUpdatesRX(2);
                }
            }
        }

        private static float rx2_waterfall_low_threshold = -130.0F;
        public static float RX2WaterfallLowThreshold
        {
            get { return rx2_waterfall_low_threshold; }
            set 
            { 
                rx2_waterfall_low_threshold = value;
                if (console != null)
                {
                    console.CheckForMinMaxWaterfallUpdatesRX(2);
                }
            }
        }

        private static float _display_line_width = 1.0F;
        public static float DisplayLineWidth
        {
            get { return _display_line_width; }
            set
            {
                lock (_objDX2Lock)
                {
                    _display_line_width = value;
                    data_line_pen.Width = _display_line_width;
                }
            }
        }

        private static float _tx_display_line_width = 1.0F;
        public static float TXDisplayLineWidth
        {
            get { return _tx_display_line_width; }
            set
            {
                lock (_objDX2Lock)
                {
                    _tx_display_line_width = value;
                    tx_data_line_pen.Width = _tx_display_line_width;
                }
            }
        }

        private static DisplayLabelAlignment display_label_align = DisplayLabelAlignment.LEFT;
        public static DisplayLabelAlignment DisplayLabelAlign
        {
            get { return display_label_align; }
            set
            {
                display_label_align = value;
            }
        }

        private static DisplayLabelAlignment tx_display_label_align = DisplayLabelAlignment.CENTER;
        public static DisplayLabelAlignment TXDisplayLabelAlign
        {
            get { return tx_display_label_align; }
            set
            {
                tx_display_label_align = value;
            }
        }

        private static int phase_num_pts = 100;
        public static int PhaseNumPts
        {
            get { return phase_num_pts; }
            set
            {
                lock (_objDX2Lock)
                {
                    phase_num_pts = value;
                }
            }
        }

        private static bool click_tune_filter = false;
        public static bool ClickTuneFilter
        {
            get { return click_tune_filter; }
            set { click_tune_filter = value; }
        }

        private static bool show_cth_line = false;
        public static bool ShowCTHLine
        {
            get { return show_cth_line; }
            set { show_cth_line = value; }
        }

        private static double f_center = vfoa_hz;
        public static double F_Center
        {
            get { return f_center; }
            set
            {
                f_center = value;
            }
        }

        private static int top_size = 0;
        public static int TopSize
        {
            get { return top_size; }
            set { top_size = value; }
        }

        private static int _lin_corr = 2;
        public static int LinCor
        {
            get
            {
                return _lin_corr;
            }
            set
            {
                _lin_corr = value;
            }
        }

        private static int _linlog_corr = -14;
        public static int LinLogCor
        {
            get
            {
                return _linlog_corr;
            }
            set
            {
                _linlog_corr = value;
            }
        }

        private static SolidBrush pana_text_brush = new SolidBrush(Color.Khaki);
        private static Font pana_font = new Font("Tahoma", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

        private static Font m_fntCallOutFont = new Font("Trebuchet MS", 9, FontStyle.Regular);

        private static Pen dhp = new Pen(Color.FromArgb(0, 255, 0)),
                           dhp1 = new Pen(Color.FromArgb(150, 0, 0, 255)),
                           dhp2 = new Pen(Color.FromArgb(150, 255, 0, 0));

        private static Font
                            font1r = new Font("Microsft Sans Serif", 9, FontStyle.Regular),
                            font10 = new Font("Arial", 10),
                            font12 = new Font("Arial", 12),
                            font14b = new Font("Arial", 14, FontStyle.Bold),
                            font9 = new Font("Arial", 9),
                            font9b = new Font("Arial", 9, FontStyle.Bold),
                            font95 = new Font("Arial", 9.5f),
                            font32b = new Font("Arial", 32, FontStyle.Bold);

        #endregion

        #region General Routines
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool localMox(int rx)
        {
            if (rx == 1)
                return _mox && (!_tx_on_vfob || (_tx_on_vfob && !_rx2_enabled));
            else if (rx == 2)
                return _mox && (_tx_on_vfob && _rx2_enabled);

            return false;
        }

        #region GDI+ General Routines

        // use pools to reduce GC
        private static ArrayPool<float> m_objFloatPool = ArrayPool<float>.Shared;
        private static ArrayPool<int> m_objIntPool = ArrayPool<int>.Shared;

        private static bool _ignore_waterfall_rx1_agc = false;
        private static bool _ignore_waterfall_rx2_agc = false;
        private static double _rx1_no_agc_duration = 0;
        private static double _rx2_no_agc_duration = 0;
        private static void clearBuffers(int W, int rx)
        {
            resetPeaksAndNoise(rx);

            if (rx == 1)
            {
                Parallel.For(0, W, (i) =>
                {
                    histogram_data[i] = Int32.MaxValue;
                    histogram_history[i] = 0;
                });
                Parallel.For(0, W, (i) =>
                {
                    new_display_data[i] = -200;
                    current_display_data[i] = -200;
                    new_waterfall_data[i] = -200;
                    current_waterfall_data[i] = -200;
                    current_display_data_copy[i] = -200;
                    current_display_data_bottom_copy[i] = -200;
                });

                //delay waterfall agc
                _rx1_no_agc_duration = m_objFrameStartTimer.ElapsedMsec + _fft_fill_timeRX1 + ((m_nFps / 1000f) * 2); // 2 extra frames
                _ignore_waterfall_rx1_agc = true;
            }
            else
            {
                Parallel.For(0, W, (i) =>
                {
                    new_display_data_bottom[i] = -200;
                    current_display_data_bottom[i] = -200;
                    new_waterfall_data_bottom[i] = -200;
                    current_waterfall_data_bottom[i] = -200;
                    current_waterfall_data_copy[i] = -200;
                    current_waterfall_data_bottom_copy[i] = -200;
                });

                //delay waterfall agc
                _rx2_no_agc_duration = m_objFrameStartTimer.ElapsedMsec + _fft_fill_timeRX2 + ((m_nFps / 1000f) * 2); // 2 extra frames
                _ignore_waterfall_rx2_agc = true;
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

                if (new_display_data != null) m_objFloatPool.Return(new_display_data);
                if (current_display_data != null) m_objFloatPool.Return(current_display_data);

                if (new_display_data_bottom != null) m_objFloatPool.Return(new_display_data_bottom);
                if (current_display_data_bottom != null) m_objFloatPool.Return(current_display_data_bottom);

                if (new_waterfall_data != null) m_objFloatPool.Return(new_waterfall_data);
                if (current_waterfall_data != null) m_objFloatPool.Return(current_waterfall_data);

                if (new_waterfall_data_bottom != null) m_objFloatPool.Return(new_waterfall_data_bottom);
                if (current_waterfall_data_bottom != null) m_objFloatPool.Return(current_waterfall_data_bottom);

                if (current_display_data_copy != null) m_objFloatPool.Return(current_display_data_copy);
                if (current_waterfall_data_copy != null) m_objFloatPool.Return(current_waterfall_data_copy);

                if (current_display_data_bottom_copy != null) m_objFloatPool.Return(current_display_data_bottom_copy);
                if (current_waterfall_data_bottom_copy != null) m_objFloatPool.Return(current_waterfall_data_bottom_copy);

                // cant be W width, as more info can be stored in these, for example scope data
                new_display_data = m_objFloatPool.Rent(BUFFER_SIZE);
                current_display_data = m_objFloatPool.Rent(BUFFER_SIZE);

                new_display_data_bottom = m_objFloatPool.Rent(BUFFER_SIZE);
                current_display_data_bottom = m_objFloatPool.Rent(BUFFER_SIZE);

                current_display_data_copy = m_objFloatPool.Rent(BUFFER_SIZE);
                current_display_data_bottom_copy = m_objFloatPool.Rent(BUFFER_SIZE);

                new_waterfall_data = m_objFloatPool.Rent(W);
                current_waterfall_data = m_objFloatPool.Rent(W);

                new_waterfall_data_bottom = m_objFloatPool.Rent(W);
                current_waterfall_data_bottom = m_objFloatPool.Rent(W);

                current_waterfall_data_copy = m_objFloatPool.Rent(W);
                current_waterfall_data_bottom_copy = m_objFloatPool.Rent(W);

                m_rx1_spectrumPeaks = new Maximums[W];
                m_rx2_spectrumPeaks = new Maximums[W];

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

        public static int[] holder = new int[100];                           // ke9ns add DX Spot used to allow the vertical lines to all be drawn first so the call sign text can draw over the top of it.
        public static int[] holder1 = new int[100];                          // ke9ns add

        private static Pen p1 = new Pen(Color.YellowGreen, 2.0f);             // ke9ns add vert line color and thickness  DXSPOTTER
        private static Pen p3 = new Pen(Color.Blue, 2.5f);                   // ke9ns add vert line color and thickness    MEMORY
        private static Pen p2 = new Pen(Color.Purple, 2.0f);                  // ke9ns add color for vert line of SWL list

        private static bool m_bLSB = false;                                     // ke9ns add true=LSB, false=USB

        private static int VFOLow = 0;                                       // ke9ns low freq (left side of screen) in HZ (used in DX_spot)
        private static int VFOHigh = 0;                                      // ke9ns high freq (right side of screen) in HZ
        private static int VFODiff = 0;                                      // ke9ns diff high-low

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color changeAlpha(Color c, int A)
        {
            return Color.FromArgb(A, c.R, c.G, c.B);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float dBToPixel(float dB, int H, bool tx = false)
        {
            if (!tx)
            {
                return (float)(spectrum_grid_max - dB) * H / (spectrum_grid_max - spectrum_grid_min);
            }
            else
            {
                return (float)(tx_spectrum_grid_max - dB) * H / (tx_spectrum_grid_max - tx_spectrum_grid_min);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float dBToRX2Pixel(float dB, int H, bool tx = false)
        {
            if (!tx)
            {
                return (float)(rx2_spectrum_grid_max - dB) * H / (rx2_spectrum_grid_max - rx2_spectrum_grid_min);
            }
            else
            {
                return (float)(tx_spectrum_grid_max - dB) * H / (tx_spectrum_grid_max - tx_spectrum_grid_min);
            }
        }

        //private static float PixelToDb(float y, int H, bool tx = false)
        //{
        //    return (float)(spectrum_grid_max - y * (double)(spectrum_grid_max - spectrum_grid_min) / H);
        //}

        private static float[] scope_min;
        private static float[] scope_max;
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
        public static float[] Scope2Min
        {
            get { return scope2_min; }
            set { scope2_min = value; }
        }
        private static float[] scope2_max = new float[displayTargetWidth];
        public static float[] Scope2Max
        {
            get { return scope2_max; }
            set { scope2_max = value; }
        }


        //MW0LGE - these properties auto AGC on the waterfall, so that
        //spectrum/grid based max/mins can be used without getting changed by agc
        private static bool m_bRX1_spectrum_thresholds = false;
        public static bool SpectrumBasedThresholdsRX1
        {
            get { return m_bRX1_spectrum_thresholds; }
            set { m_bRX1_spectrum_thresholds = value; }
        }
        private static bool m_bRX2_spectrum_thresholds = false;
        public static bool SpectrumBasedThresholdsRX2
        {
            get { return m_bRX2_spectrum_thresholds; }
            set { m_bRX2_spectrum_thresholds = value; }
        }
        //--

        private static bool rx1_waterfall_agc = false;
        public static bool RX1WaterfallAGC
        {
            get { return rx1_waterfall_agc; }
            set { rx1_waterfall_agc = value; }
        }

        private static bool rx2_waterfall_agc = false;
        public static bool RX2WaterfallAGC
        {
            get { return rx2_waterfall_agc; }
            set { rx2_waterfall_agc = value; }
        }

        private static int waterfall_update_period = 2; // in frame intevals, such that it only gets updated every 2 frame (default)
        public static int WaterfallUpdatePeriod
        {
            get { return waterfall_update_period; }
            set { waterfall_update_period = value; }
        }

        private static int rx2_waterfall_update_period = 2; // in frame intevals, such that it only gets updated every 2 frame (default)
        public static int RX2WaterfallUpdatePeriod
        {
            get { return rx2_waterfall_update_period; }
            set { rx2_waterfall_update_period = value; }
        }

        private static bool m_bStopRX1WaterfallOnTX = false;
        private static bool m_bStopRX2WaterfallOnTX = false;
        public static bool StopRX1WaterfallOnTx
        {
            get { return m_bStopRX1WaterfallOnTX; }
            set { m_bStopRX1WaterfallOnTX = value; }
        }
        public static bool StopRX2WaterfallOnTx
        {
            get { return m_bStopRX2WaterfallOnTX; }
            set { m_bStopRX2WaterfallOnTX = value; }
        }

        private static float _RX1waterfallPreviousMinValue = 20;
        private static float _RX2waterfallPreviousMinValue = 20;
        private static void ResetWaterfallBmp()
        {
            int H = displayTargetHeight;
            if (current_display_mode == DisplayMode.PANAFALL) H /= 2;
            if (_rx2_enabled) H /= 2;

            //override for splitter pos, when only one rx and it is panafall
            if (!_rx2_enabled && current_display_mode == DisplayMode.PANAFALL) H = displayTargetHeight - PanafallSplitBarPos;

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

                            tmp = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2((int)_waterfall_bmp_dx2d.Size.Width, h),
                                    new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, ALPHA_MODE)));

                            tmp.CopyFromBitmap(_waterfall_bmp_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)tmp.Size.Width, (int)tmp.Size.Height));
                            //
                        }
                    }

                    if (_waterfall_bmp_dx2d != null)
                    {
                        Utilities.Dispose(ref _waterfall_bmp_dx2d);
                        _waterfall_bmp_dx2d = null;
                    }
                    _waterfall_bmp_dx2d = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2(displayTargetWidth, H - 20), new BitmapProperties(new SDXPixelFormat(_swapChain.Description.ModeDescription.Format, ALPHA_MODE)));

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
        private static void ResetWaterfallBmp2()
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

                            tmp = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2((int)_waterfall_bmp2_dx2d.Size.Width, h),
                                    new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, ALPHA_MODE)));

                            tmp.CopyFromBitmap(_waterfall_bmp2_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)tmp.Size.Width, (int)tmp.Size.Height));
                            //
                        }
                    }

                    if (_waterfall_bmp2_dx2d != null)
                    {
                        Utilities.Dispose(ref _waterfall_bmp2_dx2d);
                        _waterfall_bmp2_dx2d = null;
                    }
                    _waterfall_bmp2_dx2d = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2(displayTargetWidth, H - 20), new BitmapProperties(new SDXPixelFormat(_swapChain.Description.ModeDescription.Format, ALPHA_MODE)));

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
        public static bool HighlightNumberScaleRX1
        {
            get { return m_bHighlightNumberScaleRX1; }
            set
            {
                m_bHighlightNumberScaleRX1 = value;
            }
        }
        public static bool HighlightNumberScaleRX2
        {
            get { return m_bHighlightNumberScaleRX2; }
            set
            {
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

                    if (_bitmapBackground != null)
                        Utilities.Dispose(ref _bitmapBackground);

#if SNOWFALL
                    santaCleanUp();
#endif

                    Utilities.Dispose(ref _waterfall_bmp_dx2d);
                    Utilities.Dispose(ref _waterfall_bmp2_dx2d);

                    if (_pause_bitmap != null)
                    {
                        Utilities.Dispose(ref _pause_bitmap);
                        _pause_bitmap = null;
                    }

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
                    if (_device != null && !string.IsNullOrEmpty(_device.DebugName))
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
                    MessageBox.Show("Problem Shutting Down DirectX !" + System.Environment.NewLine + System.Environment.NewLine + "[" + e.ToString() + "]", "Thetis DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                }
            }
        }

        private static string[] DX2Adaptors()
        {
            SharpDX.DXGI.Factory factory1 = new SharpDX.DXGI.Factory1();

            int nAdaptorCount = factory1.GetAdapterCount();
            string[] adaptors = new string[nAdaptorCount];

            for (int n = 0; n < nAdaptorCount; n++)
            {
                using (Adapter adapter = factory1.GetAdapter(n))
                {
                    adaptors[n] = adapter.Description.Description;
                }
            }
            Utilities.Dispose(ref factory1);
            factory1 = null;
            return adaptors;
        }
        private static string getGPUNameInUse()
        {
            lock (_objDX2Lock)
            {
                if (_bDX2Setup)
                {
                    //SharpDX.Direct3D11.Device device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
                    SharpDX.DXGI.Device dxgiDevice = _device.QueryInterface<SharpDX.DXGI.Device>();
                    SharpDX.DXGI.Adapter adapter = dxgiDevice.Adapter;
                    string name = adapter.Description.Description;
                    Utilities.Dispose(ref adapter);
                    Utilities.Dispose(ref dxgiDevice);
                    //Utilities.Dispose(ref device);
                    return name;
                }
                else
                    return "Unkown GPU";
            }
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
                    // you need to enable operating system support in the app.manifest file, otherwise majVers will not report 10+
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

                    _device = new Device(driverType, debug | DeviceCreationFlags.PreventAlteringLayerSettingsFromRegistry | DeviceCreationFlags.BgraSupport/* | DeviceCreationFlags.SingleThreaded*/, featureLevels);

                    SharpDX.DXGI.Device1 device1 = _device.QueryInterfaceOrNull<SharpDX.DXGI.Device1>();
                    if (device1 != null)
                    {
                        device1.MaximumFrameLatency = 1;
                        Utilities.Dispose(ref device1);
                        device1 = null;
                    }

                    //this code should ideally be used to prevent use of flip if vsync is 0, but is not used at this time
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
                        if (!_bUseLegacyBuffers) bFlipPresent = true;
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

                    RenderTargetProperties rtp = new RenderTargetProperties(new SDXPixelFormat(_swapChain.Description.ModeDescription.Format, ALPHA_MODE));
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

                    _gpu = getGPUNameInUse(); // get the directX gpu

                    setupAliasing();

                    ResetWaterfallBmp();
                    ResetWaterfallBmp2();

                    buildDX2Resources();
                    buildFontsDX2D();

                    SetDX2BackgoundImage(console.PnlDisplayBackgroundImage);
                }
                catch (Exception e)
                {
                    // issue setting up dx
                    ShutdownDX2D();
                    MessageBox.Show("Problem initialising DirectX !" + System.Environment.NewLine + System.Environment.NewLine + "[" + e.ToString() + "]", "Thetis DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                }
            }
        }
        public static int DXVersion()
        {
            lock (_objDX2Lock)
            {
                if (!_bDX2Setup) return -1;

                try
                {
                    SharpDX.Direct3D.FeatureLevel featureLevel = _device.FeatureLevel;
                    switch (featureLevel)
                    {
                        case SharpDX.Direct3D.FeatureLevel.Level_9_1: return 91;
                        case SharpDX.Direct3D.FeatureLevel.Level_9_2: return 92;
                        case SharpDX.Direct3D.FeatureLevel.Level_9_3: return 93;
                        case SharpDX.Direct3D.FeatureLevel.Level_10_0: return 100;
                        case SharpDX.Direct3D.FeatureLevel.Level_10_1: return 101;
                        case SharpDX.Direct3D.FeatureLevel.Level_11_0: return 110;
                        case SharpDX.Direct3D.FeatureLevel.Level_11_1: return 111;
                        case SharpDX.Direct3D.FeatureLevel.Level_12_0: return 120;
                        case SharpDX.Direct3D.FeatureLevel.Level_12_1: return 121;
                    }
                }
                catch { }
                return -1;
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
                    resizeDX2D();
                }
            }
            catch (Exception e)
            {

            }
        }
        private static void resizeDX2D()
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

                    RenderTargetProperties rtp = new RenderTargetProperties(new SDXPixelFormat(_swapChain.Description.ModeDescription.Format, ALPHA_MODE));
                    _d2dRenderTarget = new RenderTarget(_d2dFactory, _surface, rtp);

                    setupAliasing();

                    //[2.10.1.0] MW0LGE spectrum/bitmaps may be cleared or bad, so wait to settle
                    FastAttackNoiseFloorRX1 = true;
                    if(RX2Enabled) FastAttackNoiseFloorRX2 = true;
                }
            }
            catch (Exception e)
            {
                ShutdownDX2D();
                MessageBox.Show("DirectX resizeDX2D() Meter failure\n\nThis can sometimes be caused by other programs 'hooking' into directX," +
                    "such as GFX card control software (eg, EVGA Precision Xoc). Close down Thetis, quit as many 'system tray' and other\n" +
                    "things as possible and try again.\n\n" + e.Message, "Thetis DirectX", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }
        }

        public static int PanafallSplitBarPos
        {
            get { return (int)(displayTargetHeight * m_fPanafallSplitPerc); }
        }
        private static float m_fPanafallSplitPerc = 0.5f;
        public static float PanafallSplitBarPerc
        {
            get { return m_fPanafallSplitPerc; }
            set
            {
                bool resetBmp = false;

                lock (_objDX2Lock)
                {
                    resetBmp = value != m_fPanafallSplitPerc;
                    m_fPanafallSplitPerc = value;
                }

                if (resetBmp)
                    ResetWaterfallBmp();
            }
        }
        private static bool m_bAntiAlias = false;
        public static bool AntiAlias
        {
            get { return m_bAntiAlias; }
            set
            {
                m_bAntiAlias = value;
                setupAliasing();
            }
        }
        public static bool SpecialPanafall
        {
            get { return m_bSpecialPanafall; }
            set
            {
                m_bSpecialPanafall = value;
                if (m_bSpecialPanafall)
                    PanafallSplitBarPerc = 0.8f;
                else
                    PanafallSplitBarPerc = 0.5f;
            }
        }

        private static int m_nRX1DisplayHeight = 0;
        public static int RX1DisplayHeight
        {
            get { return m_nRX1DisplayHeight; }
            set { }
        }
        private static int m_nRX2DisplayHeight = 0;
        public static int RX2DisplayHeight
        {
            get { return m_nRX2DisplayHeight; }
            set { }
        }

        private static string m_sDebugText = "";
        public static string DebugText
        {
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
        private static bool _maintain_background_aspectratio = false;
        public static bool MaintainBackgroundAspectRatio
        {
            get { return _maintain_background_aspectratio; }
            set { _maintain_background_aspectratio = value; }
        }

        private static SharpDX.Direct2D1.Bitmap _pause_bitmap = null;
        private static bool _paused_display = false;
        private static bool _old_paused_display = false;
        public static bool PausedDisplay
        {
            get { return _paused_display; }
            set 
            {
                lock (_objDX2Lock)
                {
                    _old_paused_display = _paused_display;
                    _paused_display = value;
                    console.SetupInfoBarButton(ucInfoBar.ActionTypes.DisplayPause, _paused_display);
                    pauseDisplay();

                    if (_old_paused_display && !_paused_display)
                    {
                        //was on, now off
                        FastAttackNoiseFloorRX1 = true;
                        FastAttackNoiseFloorRX2 = true;
                    }
                }
            }
        }
        private static void pauseDisplay()
        {
            // free up old
            if(_pause_bitmap != null)
            {
                Utilities.Dispose(ref _pause_bitmap);
                _pause_bitmap = null;
            }

            if (_paused_display)
            {
                // take snap
                Texture2D sourceTexture = _swapChain1.GetBackBuffer<Texture2D>(0);
                Texture2DDescription desc = sourceTexture.Description;
                desc.CpuAccessFlags = CpuAccessFlags.Read;
                desc.Usage = ResourceUsage.Default;
                desc.BindFlags = BindFlags.ShaderResource;
                desc.CpuAccessFlags = CpuAccessFlags.None;
                Texture2D stagingTexture = new Texture2D(_device, desc);

                SharpDX.Direct3D11.DeviceContext context = _device.ImmediateContext;
                context.CopyResource(sourceTexture, stagingTexture);

                Surface dxgiSurface = stagingTexture.QueryInterface<Surface>();
                Size2F dpi = _d2dRenderTarget.DotsPerInch;

                SharpDX.Direct2D1.PixelFormat pixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, ALPHA_MODE);
                BitmapProperties props = new BitmapProperties(pixelFormat, dpi.Width, dpi.Height);

                _pause_bitmap = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, dxgiSurface, props);

                Utilities.Dispose(ref dxgiSurface);
                Utilities.Dispose(ref stagingTexture);
                Utilities.Dispose(ref sourceTexture);
            }
        }

        private static bool _valid_fps_profile = false;
        private static double _last_valid_check = double.MinValue;
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

                    if (_paused_display && _pause_bitmap != null)
                    {
                        RectangleF pr = new RectangleF(0, 0, displayTargetWidth, displayTargetHeight);
                        _d2dRenderTarget.DrawBitmap(_pause_bitmap, pr, 1f, BitmapInterpolationMode.Linear);
                        goto jump;
                    }

                    // middle pixel align shift, NOTE: waterfall will switch internally to identity, and then restore
                    Matrix3x2 t = _d2dRenderTarget.Transform;
                    t.TranslationVector = m_pixelShift;
                    _d2dRenderTarget.Transform = t;

                    //always clear without using alpha
                    _d2dRenderTarget.Clear(m_cDX2_display_background_clear_colour);

                    RectangleF rectDest;

                    if (_bitmapBackground != null) 
                    { 
                        // draw background image                        
                        if (_maintain_background_aspectratio && _bitmapBackground != null)
                        {
                            float imageWidth = _bitmapBackground.PixelSize.Width;
                            float imageHeight = _bitmapBackground.PixelSize.Height;
                            float aspectRatio = imageWidth / imageHeight;

                            float targetAspectRatio = displayTargetWidth / displayTargetHeight;

                            if (aspectRatio > targetAspectRatio)
                            {
                                float scaledHeight = displayTargetWidth / aspectRatio;
                                rectDest = new RectangleF(0, (displayTargetHeight - scaledHeight) / 2, displayTargetWidth, scaledHeight);
                            }
                            else
                            {
                                float scaledWidth = displayTargetHeight * aspectRatio;
                                rectDest = new RectangleF((displayTargetWidth - scaledWidth) / 2, 0, scaledWidth, displayTargetHeight);
                            }
                        }
                        else
                        {
                            rectDest = new RectangleF(0, 0, displayTargetWidth, displayTargetHeight);
                        }

                        _d2dRenderTarget.DrawBitmap(_bitmapBackground, rectDest, 1f, BitmapInterpolationMode.Linear);                        
                    }
                    else
                    {
                        rectDest = new RectangleF(0, 0, displayTargetWidth, displayTargetHeight);
                    }

                    _d2dRenderTarget.FillRectangle(rectDest, m_bDX2_display_background_brush);

                    // LINEAR BRUSH BUILDING
                    if (_bRebuildRXLinearGradBrush || _bRebuildTXLinearGradBrush)
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

                        if (_bRebuildRXLinearGradBrush)
                        {
                            buildLinearGradientBrush(0, tmpHeightRX1, 1);

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

                            buildLinearGradientBrush(nVertShift, tmpHeightRX2 + nVertShift, 2);

                            _bRebuildRXLinearGradBrush = false;
                        }
                        if (_bRebuildTXLinearGradBrush)
                        {
                            // build both, use rx1/rx2 heights
                            buildLinearGradientBrushTX(0, tmpHeightRX1, 1);

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

                            buildLinearGradientBrushTX(nVertShift, tmpHeightRX2 + nVertShift, 2);

                            _bRebuildTXLinearGradBrush = false;
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
                                DrawHistogramDX2D(1, displayTargetWidth, m_nRX1DisplayHeight);
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
                                DrawHistogramDX2D(1, displayTargetWidth, m_nRX1DisplayHeight);
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
                        _bRebuildRXLinearGradBrush = true;
                        _bRebuildTXLinearGradBrush = true;
                    }
                    if (m_nRX2DisplayHeight != _nOldHeightRX2)
                    {
                        _nOldHeightRX2 = m_nRX2DisplayHeight;
                        _bRebuildRXLinearGradBrush = true;
                        _bRebuildTXLinearGradBrush = true;
                    }
                
                    // HIGH swr display warning
                    if (high_swr || _power_folded_back)
                    {
                        if (_power_folded_back)
                        {
                            drawStringDX2D("HIGH SWR\n\nPOWER FOLD BACK", fontDX2d_font14, m_bDX2_Red, 245, 20);
                        }
                        else
                        {
                            drawStringDX2D("HIGH SWR", fontDX2d_font14, m_bDX2_Red, 245, 20);
                        }
                        _d2dRenderTarget.DrawRectangle(new RectangleF(3, 3, displayTargetWidth - 6, displayTargetHeight - 6), m_bDX2_Red, 6f);
                    }

                    if (m_bShowFrameRateIssue && m_bFrameRateIssue) _d2dRenderTarget.FillRectangle(new RectangleF(0, 0, 8, 8), m_bDX2_Red);
                    if (m_bShowGetPixelsIssue && (_bGetPixelsIssueRX1 || _bGetPixelsIssueRX2)) _d2dRenderTarget.FillRectangle(new RectangleF(0, 8, 8, 8), m_bDX2_Yellow);

                    calcFps();
                    if (m_bShowFPS)
                    {
                        if (_runningFPSProfile) showFPSProfile();
                        _d2dRenderTarget.DrawText(m_nFps.ToString(), fontDX2d_callout, new RectangleF(10, 0, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_m_bTextCallOutActive, DrawTextOptions.None);
                    }

                    //MW0LGE_21k8
                    processBlobsActivePeakDisplayDelay();
                    //

                    // some debug text
                    if (!string.IsNullOrEmpty(m_sDebugText))
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

#if SNOWFALL
                    if(_snowFall) letItSnow();
#endif

                    // undo the translate
                    _d2dRenderTarget.Transform = Matrix3x2.Identity;

                jump:

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

                        if (!string.IsNullOrEmpty(sMsg)) throw (new Exception(sMsg));
                    }
                }
            }
            catch (Exception e)
            {
                ShutdownDX2D();
                MessageBox.Show("Problem in DirectX Renderer !" + System.Environment.NewLine + System.Environment.NewLine + "[ " + e.ToString() + " ]", "Thetis DirectX", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }
        }
        private static readonly List<int> _fps_profile_data = new List<int>();
        private static void showFPSProfile()
        {
            if (m_objFrameStartTimer.ElapsedMsec - _last_valid_check >= 5000)
            {
                //recheck every 5 seconds
                _valid_fps_profile = !console.IsSetupFormNull ? console.SetupForm.ValidFpsProfile() : false;
                _last_valid_check = m_objFrameStartTimer.ElapsedMsec;
            }

            RoundedRectangle rr = new RoundedRectangle();
            RectangleF r = new RectangleF(20, 20, 300, _valid_fps_profile ? 170 : 184);
            rr.Rect = r;
            rr.RadiusX = 14f;
            rr.RadiusY = 14f;
            _d2dRenderTarget.FillRoundedRectangle(rr, m_bDX2_m_bHightlightNumberScale);
            _d2dRenderTarget.DrawRoundedRectangle(rr, m_bDX2_Yellow);

            r.Inflate(-6, -6);
            SharpDX.RectangleF clipRect = new SharpDX.RectangleF(r.X, r.Y, r.Width, r.Height);
            _d2dRenderTarget.PushAxisAlignedClip(clipRect, AntialiasMode.Aliased);

            if (_valid_fps_profile)
                _d2dRenderTarget.DrawText($"{m_nFps}", fontDX2d_fps_profile, new RectangleF(50, 20, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
            else
                _d2dRenderTarget.DrawText($"{m_nFps}*", fontDX2d_fps_profile, new RectangleF(50, 20, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);

            _d2dRenderTarget.DrawText($"{_cpu}", fontDX2d_callout, new RectangleF(30, 104, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
            _d2dRenderTarget.DrawText($"{_gpu}", fontDX2d_callout, new RectangleF(30, 118, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
            _d2dRenderTarget.DrawText($"Render Target Dimensions : {displayTargetWidth} x {displayTargetHeight}", fontDX2d_callout, new RectangleF(30, 132, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
            _d2dRenderTarget.DrawText($"Available Physical Ram : {_ram}", fontDX2d_callout, new RectangleF(30, 146, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
            _d2dRenderTarget.DrawText($"Installed Ram : {_installed_ram}", fontDX2d_callout, new RectangleF(30, 160, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Yellow, DrawTextOptions.None);
            if(!_valid_fps_profile) _d2dRenderTarget.DrawText("* Settings have deviated from expected !", fontDX2d_callout, new RectangleF(30, 174, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_Red, DrawTextOptions.None);

            if (_fps_profile_data.Count > 0)
            {
                bool ok = (m_dElapsedFrameStart - _fps_profile_start) >= 10000;
                _d2dRenderTarget.DrawText($"10 seconds", fontDX2d_callout, new RectangleF(220, 40, float.PositiveInfinity, float.PositiveInfinity), ok ? m_bDX2_Yellow : m_bDX2_Gray, DrawTextOptions.None);
                _d2dRenderTarget.DrawText($"Min : {_fps_profile_data.Min()}", fontDX2d_callout, new RectangleF(220, 58, float.PositiveInfinity, float.PositiveInfinity), ok ? m_bDX2_Yellow : m_bDX2_Gray, DrawTextOptions.None);
                _d2dRenderTarget.DrawText($"Max : {_fps_profile_data.Max()}", fontDX2d_callout, new RectangleF(220, 74, float.PositiveInfinity, float.PositiveInfinity), ok ? m_bDX2_Yellow: m_bDX2_Gray, DrawTextOptions.None);
            }

            _d2dRenderTarget.PopAxisAlignedClip();
        }

        private static int m_nVBlanks = 0;
        public static int VerticalBlanks
        {
            get { return m_nVBlanks; }
            set
            {
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
            m_nFrameCount++;
            if (m_dElapsedFrameStart >= m_fLastTime + 1000)
            {
                double late = m_dElapsedFrameStart - (m_fLastTime + 1000);
                if (late > 2000 || late < 0) late = 0; // ignore if too late

                //technically, we have nframes in 1000+late ms, so we should refactor down to 1000
                double frames_per_ms = m_nFrameCount / (1000 + late);
                double frames_in_1000ms = frames_per_ms * 1000;
                int frames = (int)frames_in_1000ms;

                m_nFps = frames;// m_nFrameCount;
                m_nFrameCount = m_nFrameCount - frames;//0;
                m_fLastTime = m_dElapsedFrameStart - late;

                if (_runningFPSProfile)
                {
                    // for fps_profile
                    _fps_profile_data.Add(m_nFps);
                    if (_fps_profile_data.Count > 10) _fps_profile_data.RemoveAt(0);
                }
                else if (_fps_profile_data.Count > 0) _fps_profile_data.Clear();
            }
        }

        public static int CurrentFPS
        {
            get { return m_nFps; }
            // MW0LGE_21k8 used to pre-init fps, before rundisplay has had time to recalculate, called from console mostly when adjusting fps in setup window
            set { m_nFps = value; }
        }

        private static bool m_bPeakBlobMaximums = true;
        public static bool ShowPeakBlobs
        {
            get { return m_bPeakBlobMaximums; }
            set { m_bPeakBlobMaximums = value; }
        }
        private static bool m_bInsideFilterOnly = false;
        public static bool ShowPeakBlobsInsideFilterOnly
        {
            get { return m_bInsideFilterOnly; }
            set { m_bInsideFilterOnly = value; }
        }
        private static int m_nNumberOfMaximums = 3;
        public static int NumberOfPeakBlobs
        {
            get { return m_nNumberOfMaximums; }
            set
            {
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private int isOccupied(int rx, int nX)
        {
            Maximums[] maximums = rx == 1 ? m_nRX1Maximums : m_nRX2Maximums;

            int nRet = -1; // -1 returned if nothing in this area
            for (int n = 0; n < m_nNumberOfMaximums; n++)
            {
                ref Maximums entry = ref maximums[n];

                int p1 = Math.Abs(nX - entry.X);

                if (entry.Enabled && p1 < 10) // 10 being the radius of the ellipse/circle
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private void processMaximums(int rx, float dbm, int nX, int nY)
        {
            Maximums[] maximums = rx == 1 ? m_nRX1Maximums : m_nRX2Maximums;

            int nOccupiedIndex = isOccupied(rx, nX);

            if (nOccupiedIndex >= 0)
            {
                ref Maximums entry = ref maximums[nOccupiedIndex];

                if (dbm >= entry.max_dBm)
                {
                    entry.Enabled = true;
                    entry.max_dBm = dbm;
                    entry.X = nX;
                    entry.MaxY_pixel = nY;
                    entry.Time = m_dElapsedFrameStart;
                    //Array.Sort<Maximums>(maximums, (x, y) => y.max_dBm.CompareTo(x.max_dBm));

                    //bubble up
                    int pos = nOccupiedIndex;
                    while (pos > 0 && maximums[pos].max_dBm > maximums[pos - 1].max_dBm)
                    {
                        Maximums temp = maximums[pos - 1];
                        maximums[pos - 1] = maximums[pos];
                        maximums[pos] = temp;
                        pos--;
                    }
                }
                return;
            }

            for (int n = 0; n < m_nNumberOfMaximums; n++)
            {
                ref Maximums entry = ref maximums[n];

                if (dbm > maximums[n].max_dBm)
                {
                    //move them down
                    for (int nn = m_nNumberOfMaximums - 1; nn > n; nn--)
                    {
                        ref Maximums entryNN = ref maximums[nn];
                        ref Maximums entryNNN = ref maximums[nn - 1];

                        entryNN.Enabled = entryNNN.Enabled;
                        entryNN.max_dBm = entryNNN.max_dBm;
                        entryNN.X = entryNNN.X;
                        entryNN.MaxY_pixel = entryNNN.MaxY_pixel;
                        entryNN.Time = entryNNN.Time;
                    }

                    //add new
                    entry.Enabled = true;
                    entry.max_dBm = dbm;
                    entry.X = nX;
                    entry.MaxY_pixel = nY;
                    entry.Time = m_dElapsedFrameStart;

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

            Parallel.For(0, maximums.Length, (i) =>
            {
                maximums[i].max_dBm = float.MinValue;
            });
        }
        static public void ResetBlobMaximums(int rx, bool bClear = false)
        {
            if (bClear) delayBlobsActivePeakDisplay(rx, true);

            Maximums[] maximums;
            if (rx == 1)
                maximums = m_nRX1Maximums;
            else
                maximums = m_nRX2Maximums;

            int tot = bClear ? maximums.Length : m_nNumberOfMaximums;

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
        static public bool BlobPeakHold
        {
            get { return m_bBlobPeakHold; }
            set { m_bBlobPeakHold = value; }
        }
        static private double m_fBlobPeakHoldMS = 500f;
        static public double BlobPeakHoldMS
        {
            get { return m_fBlobPeakHoldMS; }
            set { m_fBlobPeakHoldMS = value; }
        }
        static private bool m_bBlobPeakHoldDrop = false;
        static public bool BlobPeakHoldDrop
        {
            get { return m_bBlobPeakHoldDrop; }
            set { m_bBlobPeakHoldDrop = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static private bool isRxDuplex(int rx)
        {
            bool displayduplex;

            if (rx == 1)
                displayduplex = display_duplex;
            else
                //RX2 is always duplex off irrespective of front end setting
                displayduplex = false;

            return displayduplex;
        }

        static private SharpDX.Direct2D1.Ellipse m_objEllipse = new SharpDX.Direct2D1.Ellipse(Vector2.Zero, 5f, 5f);

        static private float m_fNoiseFloorRX1 = -200; // the NF exposed outside Display
        static private float m_fNoiseFloorRX2 = -200;
        static private bool m_bNoiseFloorGoodRX1 = false; // is the noisefloor good? can be used outside Display
        static private bool m_bNoiseFloorGoodRX2 = false;

        static private float m_fFFTBinAverageRX1 = -200;    // the average this frame
        static private float m_fFFTBinAverageRX2 = -200;
        static private float m_fLerpAverageRX1 = -200;      // the lerped average, over attacktime
        static private float m_fLerpAverageRX2 = -200;

        static private float m_fAttackTimeInMSForRX1 = 2000;
        static private float m_fAttackTimeInMSForRX2 = 2000;

        static private double _fLastFastAttackEnabledTimeRX1 = 0;
        static private double _fLastFastAttackEnabledTimeRX2 = 0;

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

        private static void modifyDataForNotches(ref float[] data, int rx, bool bottom, bool local_mox, bool displayduplex, int W)
        {
            int Low, High;
            if (rx == 1)
            {
                if (local_mox)
                {
                    if (displayduplex)
                    {
                        Low = rx_display_low;
                        High = rx_display_high;
                    }
                    else
                    {
                        Low = tx_display_low;
                        High = tx_display_high;
                    }
                }
                else
                {
                    Low = rx_display_low;
                    High = rx_display_high;
                }
            }
            else// if (rx == 2)
            {
                if (local_mox)
                {
                    if (displayduplex)
                    {
                        Low = tx_display_low;
                        High = tx_display_high;
                    }
                    else
                    {
                        Low = tx_display_low;
                        High = tx_display_high;
                    }
                }
                else
                {
                    Low = rx2_display_low;
                    High = rx2_display_high;
                }
            }
            float fAttenuation = 100f;
            int width = High - Low;

            // get the notch data
            List<clsNotchCoords> notchData = handleNotches(rx, bottom, getCWSideToneShift(rx), Low, High, 0, 0, width, W, 0, false);

            int nDecimatedWidth = W / m_nDecimation;

            foreach (clsNotchCoords nc in notchData)
            {
                if (!nc._Use) continue; // skip inactive

                // do left
                int wL = nc._c_x - nc._left_x;
                wL = Math.Max(1, wL);
                for (int i = nc._c_x; i > nc._c_x - wL; i--)
                {
                    int xPos = i / m_nDecimation;
                    if (xPos < 0 || xPos > nDecimatedWidth - 1) continue;

                    int x = nc._c_x - i;

                    float fTmp = 1f / ((float)Math.Pow((double)wL / (double)(wL - x), 1.5)); // pow2 quite sharp
                    data[xPos] -= (fAttenuation * fTmp);
                }
                // do right
                int wR = nc._right_x - nc._c_x;
                wR = Math.Max(1, wR);
                for (int i = nc._c_x; i < nc._c_x + wR; i++)
                {
                    int xPos = i / m_nDecimation;
                    if (xPos < 0 || xPos > nDecimatedWidth - 1) continue;

                    int x = i - nc._c_x;

                    float fTmp = 1f / ((float)Math.Pow((double)wR / (double)(wR - x), 1.5)); // pow2 quite sharp
                    data[xPos] -= (fAttenuation * fTmp);
                }
            }
        }
        private readonly static object _rx1_offset_locker = new object();
        private readonly static object _rx2_offset_locker = new object();
        public static float RX1Offset
        {
            get
            {
                lock (_rx1_offset_locker)
                {
                    float fOffset;
                    bool local_mox = localMox(1);
                    bool displayduplex = isRxDuplex(1);

                    if (local_mox)
                    {
                        fOffset = tx_display_cal_offset;
                        if (displayduplex)
                        {
                            fOffset += rx1_display_cal_offset; //[2.10.1.0] MW0LGE fix issue #137
                            fOffset += tx_attenuator_offset; //[2.10.3.6]MW0LGE att_fix // change fixes #482
                        }
                    }
                    else if (_mox && _tx_on_vfob && !displayduplex)
                    {
                        if (console.RX2Enabled) fOffset = rx1_display_cal_offset;
                        else fOffset = tx_display_cal_offset;
                    }
                    else fOffset = rx1_display_cal_offset;

                    if (!local_mox) fOffset += rx1_preamp_offset;

                    return fOffset;
                }
            }
        }
        public static float RX1OffsetWithDup // used by minispec which is always in duplex mode
        {
            get
            {
                lock (_rx1_offset_locker)
                {
                    float fOffset;
                    bool local_mox = localMox(1);
                    bool displayduplex = true;

                    if (local_mox)
                    {
                        fOffset = tx_display_cal_offset;
                        if (displayduplex)
                        {
                            fOffset += rx1_display_cal_offset;
                            fOffset += tx_attenuator_offset;
                        }
                    }
                    else if (_mox && _tx_on_vfob && !displayduplex)
                    {
                        if (console.RX2Enabled) fOffset = rx1_display_cal_offset;
                        else fOffset = tx_display_cal_offset;
                    }
                    else fOffset = rx1_display_cal_offset;

                    if (!local_mox) fOffset += rx1_preamp_offset;

                    return fOffset;
                }
            }
        }
        public static float RX2Offset
        {
            get
            {
                lock (_rx2_offset_locker)
                {
                    float fOffset;
                    bool local_mox = localMox(2);
                    bool displayduplex = isRxDuplex(2); // always returns false

                    if (local_mox)
                    {
                        fOffset = tx_display_cal_offset;
                        if (displayduplex)
                        {
                            fOffset += rx2_display_cal_offset;
                            fOffset += tx_attenuator_offset;
                        }
                    }
                    else fOffset = rx2_display_cal_offset;

                    if (!local_mox) fOffset += rx2_preamp_offset;

                    return fOffset;
                }
            }
        }
        public static float RX2OffsetWithDup // used by minispec which is always in duplex mode
        {
            get
            {
                lock (_rx2_offset_locker)
                {
                    float fOffset;
                    bool local_mox = localMox(2);
                    bool displayduplex = true;

                    if (local_mox)
                    {
                        fOffset = tx_display_cal_offset;
                        if (displayduplex)
                        {
                            fOffset += rx2_display_cal_offset;
                            fOffset += tx_attenuator_offset;
                        }
                    }
                    else fOffset = rx2_display_cal_offset;

                    if (!local_mox) fOffset += rx2_preamp_offset;

                    return fOffset;
                }
            }
        }
        // ExponentialMovingAverage previous values for 2tone calcs
        private static float _ema_dbc = -999; //used as the init state
        private static int _two_tone_readings_X_offset = 50;

        private static float _ema_f0l;
        private static float _ema_f0u;
        private static float _ema_imd3l;
        private static float _ema_imd3u;
        private static float _ema_imd5l;
        private static float _ema_imd5u;

        private static float _ema_f0l_freq;
        private static float _ema_f0h_freq;
        private static float _ema_imd3l_freq;
        private static float _ema_imd3h_freq;
        private static float _ema_imd5l_freq;
        private static float _ema_imd5h_freq;

        private static float _ema_imd3dBc;
        private static float _ema_imd5dBc;
        private static float _ema_oip3;
        private static float _ema_oip5;
        //

        unsafe static private bool DrawPanadapterDX2D(int nVerticalShift, int W, int H, int rx, bool bottom)
        {
            //if (grid_control) //[2.10.3.9]MW0LGE raw grid control option now just turns off the grid, all other elements are shown
            //{
                int centre_x = drawPanadapterAndWaterfallGridDX2D(nVerticalShift, W, H, rx, bottom, out long left_edge, out long right_edge, false);
            //}

            float local_max_y = float.MinValue;
            float local_max_x = float.MinValue;

            bool displayduplex = isRxDuplex(rx);
            bool local_mox = localMox(rx);

            int grid_max = 0;
            int grid_min = 0;

            Maximums[] spectralPeaks = null;
            double dSpectralPeakHoldDelay;
            bool bSpectralPeakHold;
            bool bPeakBlobs;
            float dBmSpectralPeakFall;
            bool bActivePeakFill;

            int nDecimatedWidth = W / m_nDecimation;

            int yRange;
            float[] data;
            float[] dataCopy;

            bool bDoVisualNotch = false;

            bool show_imd_measurements;

            if (rx == 1)
            {
                bSpectralPeakHold = !local_mox && m_bSpectralPeakHoldRX1 && !m_bDelayRX1SpectrumPeaks;
                dSpectralPeakHoldDelay = m_dSpecralPeakHoldDelayRX1;
                bPeakBlobs = m_bPeakBlobMaximums && !m_bDelayRX1Blobs;
                show_imd_measurements = local_mox && _testing_imd && _show_imd_measurements && displayduplex;
                dBmSpectralPeakFall = m_dBmPerSecondSpectralPeakFallRX1;
                bActivePeakFill = m_bActivePeakFillRX1;

                if (local_mox)
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
                    bDoVisualNotch = true;
                    if (!displayduplex && (local_mox || (_mox && _tx_on_vfob)) && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < nDecimatedWidth; i++)
                            current_display_data[i] = grid_min - rx1_display_cal_offset;
                    }
                    else
                    {
                        fixed (void* rptr = &new_display_data[0])
                        fixed (void* wptr = &current_display_data[0])
                            Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));
                    }

                    // make copy of the data so visual notch does not change the average noise floor
                    fixed (void* rptr = &current_display_data[0])
                    fixed (void* wptr = &current_display_data_copy[0])
                        Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));

                    data_ready = false;
                }

                data = current_display_data;
                dataCopy = current_display_data_copy;
            }
            else// rx == 2
            {
                bSpectralPeakHold = !local_mox && m_bSpectralPeakHoldRX2 && !m_bDelayRX2SpectrumPeaks;
                dSpectralPeakHoldDelay = m_dSpecralPeakHoldDelayRX2;
                bPeakBlobs = m_bPeakBlobMaximums && !m_bDelayRX2Blobs;
                show_imd_measurements = false;
                dBmSpectralPeakFall = m_dBmPerSecondSpectralPeakFallRX2;
                bActivePeakFill = m_bActivePeakFillRX2;

                if (local_mox)
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
                    bDoVisualNotch = true;

                    if (blank_bottom_display || (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU)))
                    {
                        for (int i = 0; i < nDecimatedWidth; i++)
                            current_display_data_bottom[i] = grid_min - rx2_display_cal_offset;
                    }
                    else
                    {
                        fixed (void* rptr = &new_display_data_bottom[0])
                        fixed (void* wptr = &current_display_data_bottom[0])
                            Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));
                    }

                    // make copy of the data so visual notch does not change the average noise floor
                    fixed (void* rptr = &current_display_data_bottom[0])
                    fixed (void* wptr = &current_display_data_bottom_copy[0])
                        Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));

                    data_ready_bottom = false;
                }

                data = current_display_data_bottom;
                dataCopy = current_display_data_bottom_copy;
            }

            dBmSpectralPeakFall /= (float)m_nFps;

            float max;
            float max_copy;
            float fOffset;

            fOffset = rx == 1 ? RX1Offset : RX2Offset;

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
            float line_width;

            if (local_mox)
            {
                if (rx == 1)
                {
                    lineBrush = m_bUseLinearGradientForDataLineTX && m_bUseLinearGradientTX ? m_brushLGDataLineTX_RX1 : m_bDX2_data_line_pen_brush_tx;
                    fillBrush = m_bUseLinearGradientTX ? m_brushLGDataFillTX_RX1 : m_bDX2_data_fill_fpen_brush_tx;
                }
                else
                {
                    lineBrush = m_bUseLinearGradientForDataLineTX && m_bUseLinearGradientTX ? m_brushLGDataLineTX_RX2 : m_bDX2_data_line_pen_brush_tx;
                    fillBrush = m_bUseLinearGradientTX ? m_brushLGDataFillTX_RX2 : m_bDX2_data_fill_fpen_brush_tx;
                }

                fillPeaksBrush = m_bDX2_dataPeaks_fill_fpen_brush; //todo
                line_width = _tx_display_line_width;
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
                line_width = _display_line_width;
            }

            float dbmToPixel = H / (float)yRange;

            // calc start pos
            int Y;
            max = data[0] + fOffset;
            Y = (int)((grid_max - max) * dbmToPixel - 0.5f) + nVerticalShift;// -0.5 to mimic floor

            bool bIgnoringPoints = false;
            SharpDX.Vector2 point = new SharpDX.Vector2();
            SharpDX.Vector2 spectralPeakPoint = new SharpDX.Vector2();
            SharpDX.Vector2 lastIgnoredPoint = new SharpDX.Vector2();
            SharpDX.Vector2 bottomPoint = new SharpDX.Vector2(0, nVerticalShift + H);
            SharpDX.Vector2 previousPoint = new SharpDX.Vector2(0, Y);

            float local_max_Pixel_y = float.MinValue;

            int filter_left_x = 0, filter_right_x = 0;
            if (bPeakBlobs)
            {
                ResetBlobMaximums(rx);
                if (!show_imd_measurements && m_bInsideFilterOnly) getFilterXPositions(rx, W, local_mox, displayduplex, out filter_left_x, out filter_right_x);
            }

            SharpDX.Vector2 oldSpectralPeakPoint = new SharpDX.Vector2();
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

                oldSpectralPeakPoint.X = 0;
                oldSpectralPeakPoint.Y = (int)(((grid_max - spectralPeaks[0].max_dBm) * dbmToPixel) - 0.5f);
                if (oldSpectralPeakPoint.Y >= H) oldSpectralPeakPoint.Y = H;
                oldSpectralPeakPoint.Y += nVerticalShift;
            }

            float dbm_min = float.PositiveInfinity;
            float dbm_max = float.NegativeInfinity;
            int dbm_max_ypos = 0;
            int dbm_min_xpos = 0;
            int dbm_max_xpos = 0;
            bool look_for_max = true;
            float trigger_delta = 10; //db

            Rectangle nf_box = new Rectangle(40, 0, 8, 8);

            List<Maximums> imd_measurements = new List<Maximums>();

            unchecked // we dont expect any overflows
            {
                SharpDX.RectangleF clipRect = new SharpDX.RectangleF(0, nVerticalShift, W, H);
                _d2dRenderTarget.PushAxisAlignedClip(clipRect, AntialiasMode.Aliased);

                // modify the data for visual notches
                if (bDoVisualNotch && m_bShowVisualNotch && !local_mox)
                {
                    modifyDataForNotches(ref data, rx, bottom, local_mox, displayduplex, W);
                }

                float averageSum = 0;
                int averageCount = 1;
                float currentAverage = rx == 1 ? m_fFFTBinAverageRX1 + 2 : m_fFFTBinAverageRX2 + 2; // +2 so we dont include samples close to our current average, this perhaps should be configurable, buffer?

                bool peaks_imds = bPeakBlobs || show_imd_measurements;

                // make locals
                int local_Decimation = m_nDecimation;
                double local_frame_start = m_dElapsedFrameStart;
                //

                for (int i = 0; i < nDecimatedWidth; i++)
                {
                    point.X = i * local_Decimation;

                    max = data[i] + fOffset;
                    max_copy = dataCopy[i] + fOffset;

                    // noise floor
                    if (!local_mox && (max_copy < currentAverage))
                    {
                        //averageSum += (float)Math.Pow(10f, max_copy / 10f);
                        averageSum += fastPow10Raw(max_copy);
                        averageCount++;
                    }

                    Y = (int)((grid_max - max) * dbmToPixel - 0.5f) + nVerticalShift;// -0.5 to mimic floor
                    point.Y = Y;

                    if (max > local_max_y)
                    {
                        // store peak
                        local_max_y = max;
                        local_max_x = point.X;
                        local_max_Pixel_y = Y;
                    }

                    // peak blobs
                    if (peaks_imds && (!m_bInsideFilterOnly || (point.X >= filter_left_x && point.X <= filter_right_x) || show_imd_measurements))
                    {
                        if (max > dbm_max)
                        {
                            dbm_max = max;
                            dbm_max_ypos = Y;
                            dbm_max_xpos = i;
                        }
                        if (max < dbm_min)
                        {
                            dbm_min = max;
                            dbm_min_xpos = i;
                        }
                        if (look_for_max)
                        {
                            if (max < dbm_max - trigger_delta)
                            {
                                if (show_imd_measurements)
                                {
                                    Maximums mm = new Maximums();
                                    mm.max_dBm = dbm_max;
                                    mm.X = dbm_max_xpos;
                                    mm.Enabled = true;
                                    mm.MaxY_pixel = dbm_max_ypos;
                                    mm.Time = local_frame_start;

                                    imd_measurements.Add(mm);
                                }
                                else
                                {
                                    processMaximums(rx, dbm_max, dbm_max_xpos, dbm_max_ypos);
                                }
                                dbm_min = max;
                                dbm_min_xpos = i;
                                look_for_max = false;
                            }
                        }
                        else if (max > dbm_min + trigger_delta)
                        {
                            dbm_max = max;
                            dbm_max_ypos = Y;
                            dbm_max_xpos = i;
                            look_for_max = true;
                        }
                    }

                    //pana fill
                    if (pan_fill)
                    {
                        // draw vertical line, this is so much faster than FillGeometry as the geo created would be so complex any fill alogorthm would struggle
                        bottomPoint.X = point.X;
                        _d2dRenderTarget.DrawLine(bottomPoint, point, fillBrush, local_Decimation);
                    }

                    //spectral peak
                    if (bSpectralPeakHold)
                    {
                        ref Maximums peak = ref spectralPeaks[i];

                        if (max >= peak.max_dBm)
                        {
                            peak.max_dBm = max;
                            peak.Time = local_frame_start;
                        }

                        if (peak.max_dBm >= max)
                        {
                            // draw to peak, but re-work Y as we might rescale the spectrum vertically
                            spectralPeakPoint.X = point.X;
                            spectralPeakPoint.Y = (int)((grid_max - peak.max_dBm) * dbmToPixel - 0.5f) + nVerticalShift;// -0.5 to mimic floor

                            if (bActivePeakFill)
                            {
                                _d2dRenderTarget.DrawLine(point, spectralPeakPoint, fillPeaksBrush, local_Decimation);
                            }
                            else
                            {
                                _d2dRenderTarget.DrawLine(oldSpectralPeakPoint, spectralPeakPoint, fillPeaksBrush, line_width);
                                oldSpectralPeakPoint = spectralPeakPoint;
                            }

                            double dElapsed = local_frame_start - peak.Time;
                            if (dElapsed > dSpectralPeakHoldDelay)
                            {
                                peak.max_dBm -= dBmSpectralPeakFall;
                            }
                        }
                    }

                    // ignore point if same Y as last point
                    // lines will get longer if flat, reducing number of total points
                    bool isInteriorDuplicate = (i > 0 && i < nDecimatedWidth - 1) && point.Y == previousPoint.Y;
                    if (isInteriorDuplicate)
                    {
                        lastIgnoredPoint = point;
                        bIgnoringPoints = true;
                        continue;
                    }
                    if (bIgnoringPoints)
                    {
                        _d2dRenderTarget.DrawLine(previousPoint, lastIgnoredPoint, lineBrush, line_width);
                        previousPoint = lastIgnoredPoint;
                        bIgnoringPoints = false;
                    }
                    _d2dRenderTarget.DrawLine(previousPoint, point, lineBrush, line_width);
                    previousPoint = point;
                }

                //noise floor
                if (!local_mox)
                {
                    bool bPreviousRX1 = _bNoiseFloorAlreadyCalculatedRX1; //updated in processNoiseFloor
                    bool bPreviousRX2 = _bNoiseFloorAlreadyCalculatedRX2;
                    processNoiseFloor(rx, averageCount, averageSum, nDecimatedWidth, false);

                    int yPixelLerp;
                    int yPixelActual;
                    float lerp;
                    bool show_noise_floor;

                    if (rx == 1)
                    {
                        if (!m_bFastAttackNoiseFloorRX1 && !bPreviousRX1)
                        {
                            m_fNoiseFloorRX1 = m_fLerpAverageRX1 + _fNFshiftDBM;
                            m_bNoiseFloorGoodRX1 = true;
                        }

                        lerp = m_fLerpAverageRX1 + _fNFshiftDBM;

                        yPixelLerp = (int)dBToPixel(lerp, H);
                        yPixelActual = (int)dBToPixel(m_fFFTBinAverageRX1 + _fNFshiftDBM, H);

                        show_noise_floor = m_bShowRX1NoiseFloor;
                    }
                    else
                    {
                        if (!m_bFastAttackNoiseFloorRX2 && !bPreviousRX2)
                        {
                            m_fNoiseFloorRX2 = m_fLerpAverageRX2 + _fNFshiftDBM;
                            m_bNoiseFloorGoodRX2 = true;
                        }

                        lerp = m_fLerpAverageRX2 + _fNFshiftDBM;

                        yPixelLerp = (int)dBToRX2Pixel(lerp, H);
                        yPixelActual = (int)dBToRX2Pixel(m_fFFTBinAverageRX2 + _fNFshiftDBM, H);

                        show_noise_floor = m_bShowRX2NoiseFloor;
                    }                    

                    if (show_noise_floor)
                    {
                        yPixelLerp += nVerticalShift;

                        bool bFast = rx == 1 ? m_bFastAttackNoiseFloorRX1 : m_bFastAttackNoiseFloorRX2;

                        yPixelActual += nVerticalShift;

                        SharpDX.Direct2D1.Brush nf_colour = bFast ? m_bDX2_Gray : m_bDX2_noisefloor;
                        SharpDX.Direct2D1.Brush nf_colour_text = bFast ? m_bDX2_Gray : m_bDX2_noisefloor_text;

                        int yP = (int)yPixelLerp;

                        nf_box.Y = yP - 8;
                        drawFillRectangleDX2D(nf_colour, nf_box);
                        drawLineDX2D(nf_colour, 40, yP, W - 40, yP, m_styleDots, m_fNoiseFloorLineWidth); // horiz line

                        if (m_bShowNoiseFloorDBM)
                        {
                            drawLineDX2D(nf_colour, nf_box.X - 3, (int)yPixelActual, nf_box.X - 3, yP, 2); // direction up/down line
                            drawStringDX2D(lerp.ToString(_NFDecimal ? "F1" : "F0"), fontDX2d_font9b, nf_colour_text, nf_box.X + nf_box.Width, nf_box.Y - 6);
                        }
                        else
                        {
                            drawStringDX2D("-NF", fontDX2d_panafont, nf_colour_text, nf_box.X + nf_box.Width, nf_box.Y - 4);
                        }
                    }
                }

                // peak blobs
                if (bPeakBlobs || show_imd_measurements)
                {
                    Maximums[] maximums;
                    if (show_imd_measurements)
                    {
                        maximums = imd_measurements
                            .OrderByDescending(m => m.max_dBm)
                            .Take(20)
                            .ToArray();
                    }
                    else
                    {
                        maximums = rx == 1 ? m_nRX1Maximums : m_nRX2Maximums;
                    }

                    int maxblobs = show_imd_measurements ? maximums.Length : m_nNumberOfMaximums;
                    bool blob_drop = m_bBlobPeakHold && m_bBlobPeakHoldDrop;

                    for (int n = 0; n < maxblobs; n++)
                    {
                        ref Maximums entry = ref maximums[n];

                        if (entry.Enabled)
                        {
                            if (blob_drop)
                            {
                                //drop
                                double dElapsed = local_frame_start - entry.Time;
                                if (entry.max_dBm > -200.0 && (dElapsed > m_fBlobPeakHoldMS))
                                {
                                    entry.max_dBm -= m_dBmPerSecondPeakBlobFall / (float)m_nFps;

                                    // recalc Y
                                    entry.MaxY_pixel = (int)(((grid_max - entry.max_dBm) * dbmToPixel) - 0.5f);// -0.5 to mimic floor
                                }
                                else if (entry.max_dBm <= -200.0)
                                {
                                    entry.Enabled = false; // switch any off that fall off the bottom same as resetmaximums
                                    entry.max_dBm = float.MinValue;
                                }
                            }

                            m_objEllipse.Point.X = entry.X * local_Decimation;
                            m_objEllipse.Point.Y = entry.MaxY_pixel;

                            string sAppend;
                            if (rx == 1)
                            {
                                sAppend = m_bShowRX1NoiseFloor && !local_mox ? " (" + (entry.max_dBm - m_fLerpAverageRX1).ToString("f1") + ")" : "";// " (" + (n + 1).ToString() + ")";
                            }
                            else
                            {
                                sAppend = m_bShowRX2NoiseFloor && !local_mox ? " (" + (entry.max_dBm - m_fLerpAverageRX2).ToString("f1") + ")" : "";// " (" + (n + 1).ToString() + ")";
                            }
                            _d2dRenderTarget.DrawEllipse(m_objEllipse, m_bDX2_PeakBlob);
                            _d2dRenderTarget.DrawText(entry.max_dBm.ToString("f1") + sAppend, fontDX2d_callout, new RectangleF(m_objEllipse.Point.X + 6, m_objEllipse.Point.Y - 8, float.PositiveInfinity, float.PositiveInfinity), m_bDX2_PeakBlobText, DrawTextOptions.None);
                        }
                    }

                    if (show_imd_measurements)
                    {
                        Maximums[] sorted = imd_measurements
                            .OrderByDescending(item => item.max_dBm)
                            .ToArray();

                        if (sorted.Length >= 2)
                        {
                            // the box
                            RoundedRectangle rr = new RoundedRectangle();
                            rr.Rect = new RectangleF(_two_tone_readings_X_offset, 50, 260, 180);
                            rr.RadiusX = 14f;
                            rr.RadiusY = 14f;
                            _d2dRenderTarget.FillRoundedRectangle(rr, m_bDX2_m_bHightlightNumberScale);
                            _d2dRenderTarget.DrawRoundedRectangle(rr, m_bDX2_m_bHightlightNumbers);
                            //

                            int pixel_diff = Math.Abs(sorted[0].X - sorted[1].X);
                            if (pixel_diff > 10)
                            {
                                int low_x = sorted[0].X < sorted[1].X ? sorted[0].X : sorted[1].X;
                                int high_x = sorted[0].X > sorted[1].X ? sorted[0].X : sorted[1].X;
                                int mid_x = low_x + (pixel_diff / 2);

                                Maximums[] sortedlow = imd_measurements
                                .OrderByDescending(m => m.X)
                                .Where(m => m.X < mid_x)
                                .ToArray();

                                Maximums[] sortedhigh = sorted
                                .OrderBy(m => m.X)
                                .Where(m => m.X > mid_x)
                                .ToArray();

                                int fL = findImd(sortedlow, 1, pixel_diff, low_x, true, out int fL_X);
                                int fH = findImd(sortedhigh, 1, pixel_diff, high_x, false, out int fH_X);

                                int imd3indexL = findImd(sortedlow, 3, pixel_diff, low_x, true, out int imd3L_X);
                                int imd3indexH = findImd(sortedhigh, 3, pixel_diff, high_x, false, out int imd3H_X);

                                int imd5indexL = findImd(sortedlow, 5, pixel_diff, low_x, true, out int imd5L_X);
                                int imd5indexH = findImd(sortedhigh, 5, pixel_diff, high_x, false, out int imd5H_X);

                                bool ok = fL != -1 && fH != -1 && imd3indexL != -1 && imd3indexH != -1 && imd5indexL != -1 && imd5indexH != -1;

                                if (ok)
                                {
                                    float[] f = new float[] { sortedlow[fL].max_dBm, sortedhigh[fH].max_dBm };
                                    float[] imd3 = new float[] { sortedlow[imd3indexL].max_dBm, sortedhigh[imd3indexH].max_dBm };
                                    float[] imd5 = new float[] { sortedlow[imd5indexL].max_dBm, sortedhigh[imd5indexH].max_dBm };

                                    long low_frequency_edge_hz = (long)(m_dCentreFreqRX1 * 1e6) + left_edge;
                                    long high_frequency_edge_hz = (long)(m_dCentreFreqRX1 * 1e6) + right_edge;

                                    double hz_per_pixel = (high_frequency_edge_hz - low_frequency_edge_hz) / (double)W;

                                    long f0l_freq = low_frequency_edge_hz + (long)(fL_X * hz_per_pixel);
                                    long f0h_freq = low_frequency_edge_hz + (long)(fH_X * hz_per_pixel);
                                    long imd3l_freq = low_frequency_edge_hz + (long)(imd3L_X * hz_per_pixel);
                                    long imd3h_freq = low_frequency_edge_hz + (long)(imd3H_X * hz_per_pixel);
                                    long imd5l_freq = low_frequency_edge_hz + (long)(imd5L_X * hz_per_pixel);
                                    long imd5h_freq = low_frequency_edge_hz + (long)(imd5H_X * hz_per_pixel);

                                    float dbc = Math.Max(f[0], f[1]);
                                    float dbc_min = Math.Min(f[0], f[1]);
                                    float imd3max = Math.Max(imd3[0], imd3[1]);
                                    float imd5max = Math.Max(imd5[0], imd5[1]);
                                    float imd3dBc = dbc_min - imd3max;
                                    float imd5dBc = dbc_min - imd5max;
                                    float oip3 = dbc_min + (imd3dBc / 2f);
                                    float oip5 = dbc_min + (imd5dBc / 2f);

                                    _two_tone_readings_X_offset = imd5L_X - (int)(rr.Rect.Right - rr.Rect.Left) - pixel_diff;
                                    if (_two_tone_readings_X_offset < 50) _two_tone_readings_X_offset = 50;

                                    //ExponentialMovingAverage
                                    //previous = alpha * newValue + (1 - alpha) * previous;

                                    if (_ema_dbc == -999)
                                    {
                                        //init state
                                        _ema_dbc = dbc;

                                        _ema_f0l = f[0];
                                        _ema_f0u = f[1];
                                        _ema_imd3l = imd3[0];
                                        _ema_imd3u = imd3[1];
                                        _ema_imd5l = imd5[0];
                                        _ema_imd5u = imd5[1];

                                        _ema_f0l_freq = f0l_freq;
                                        _ema_f0h_freq = f0h_freq;
                                        _ema_imd3l_freq = imd3l_freq;
                                        _ema_imd3h_freq = imd3h_freq;
                                        _ema_imd5l_freq = imd5l_freq;
                                        _ema_imd5h_freq = imd5h_freq;

                                        _ema_imd3dBc = imd3dBc;
                                        _ema_imd5dBc = imd5dBc;
                                        _ema_oip3 = oip3;
                                        _ema_oip5 = oip5;
                                    }
                                    else
                                    {
                                        float alpha = 0.1f;

                                        _ema_dbc = alpha * dbc + (1 - alpha) * _ema_dbc;

                                        _ema_f0l = alpha * f[0] + (1 - alpha) * _ema_f0l;
                                        _ema_f0u = alpha * f[1] + (1 - alpha) * _ema_f0u;
                                        _ema_imd3l = alpha * imd3[0] + (1 - alpha) * _ema_imd3l;
                                        _ema_imd3u = alpha * imd3[1] + (1 - alpha) * _ema_imd3u;
                                        _ema_imd5l = alpha * imd5[0] + (1 - alpha) * _ema_imd5l;
                                        _ema_imd5u = alpha * imd5[1] + (1 - alpha) * _ema_imd5u;

                                        _ema_f0l_freq = alpha * f0l_freq + (1 - alpha) * _ema_f0l_freq;
                                        _ema_f0h_freq = alpha * f0h_freq + (1 - alpha) * _ema_f0h_freq;
                                        _ema_imd3l_freq = alpha * imd3l_freq + (1 - alpha) * _ema_imd3l_freq;
                                        _ema_imd3h_freq = alpha * imd3h_freq + (1 - alpha) * _ema_imd3h_freq;
                                        _ema_imd5l_freq = alpha * imd5l_freq + (1 - alpha) * _ema_imd5l_freq;
                                        _ema_imd5h_freq = alpha * imd5h_freq + (1 - alpha) * _ema_imd5h_freq;

                                        _ema_imd3dBc = alpha * imd3dBc + (1 - alpha) * _ema_imd3dBc;
                                        _ema_imd5dBc = alpha * imd5dBc + (1 - alpha) * _ema_imd5dBc;
                                        _ema_oip3 = alpha * oip3 + (1 - alpha) * _ema_oip3;
                                        _ema_oip5 = alpha * oip5 + (1 - alpha) * _ema_oip5;
                                    }

                                    float f0l = -(_ema_dbc - _ema_f0l);
                                    float f0u = -(_ema_dbc - _ema_f0u);
                                    float imd3l = -(_ema_dbc - _ema_imd3l);
                                    float imd3u = -(_ema_dbc - _ema_imd3u);
                                    float imd5l = -(_ema_dbc - _ema_imd5l);
                                    float imd5u = -(_ema_dbc - _ema_imd5u);
                                    float diff = (f0h_freq - f0l_freq) / 1000f;
                                    float worst_imd3 = -_ema_imd3dBc;
                                    float worst_imd5 = -_ema_imd5dBc;

                                    string readings =
                                        "    f0 L\n" +
                                        "    f0 U\n" +
                                        "IMD3 L\n" +
                                        "IMD3 U\n" +
                                        "IMD5 L\n" +
                                        "IMD5 U\n\n" +
                                        "        IMD3\n" +
                                        "        IMD5\n" +
                                        "        OIP3\n" +
                                        "        OIP5";

                                    string val1 =
                                        _ema_f0l.ToString("f2") + "\n" +
                                        _ema_f0u.ToString("f2") + "\n" +
                                        _ema_imd3l.ToString("f2") + "\n" +
                                        _ema_imd3u.ToString("f2") + "\n" +
                                        _ema_imd5l.ToString("f2") + "\n" +
                                        _ema_imd5u.ToString("f2") + "\n\n" +
                                        "    " + worst_imd3.ToString("f2") + " dBc\n" +
                                        "    " + worst_imd5.ToString("f2") + " dBc\n" +
                                        "    " + _ema_oip3.ToString("f2") + " dB\n" +
                                        "    " + _ema_oip5.ToString("f2") + " dB";

                                    string val2 =
                                        f0l.ToString("f2") + "\n" +
                                        f0u.ToString("f2") + "\n" +
                                        imd3l.ToString("f2") + "\n" +
                                        imd3u.ToString("f2") + "\n" +
                                        imd5l.ToString("f2") + "\n" +
                                        imd5u.ToString("f2");

                                    string val3 =
                                        (_ema_f0l_freq * 1e-6).ToString("f6") + " MHz\n" +
                                        (_ema_f0h_freq * 1e-6).ToString("f6") + " MHz\n" +
                                        (_ema_imd3l_freq * 1e-6).ToString("f6") + " MHz\n" +
                                        (_ema_imd3h_freq * 1e-6).ToString("f6") + " MHz\n" +
                                        (_ema_imd5l_freq * 1e-6).ToString("f6") + " MHz\n" +
                                        (_ema_imd5h_freq * 1e-6).ToString("f6") + " MHz\n\n\n" +
                                        "  " + diff.ToString("F3") + " kHz";

                                    _d2dRenderTarget.DrawText("dBm        dBc           frequency", fontDX2d_callout, new RectangleF(_two_tone_readings_X_offset + 70, 54, 200, 120), m_bDX2_PeakBlobText, DrawTextOptions.None);
                                    _d2dRenderTarget.DrawText(readings, fontDX2d_callout, new RectangleF(_two_tone_readings_X_offset + 10, 70, 200, 120), m_bDX2_PeakBlobText, DrawTextOptions.None);
                                    _d2dRenderTarget.DrawText(val1, fontDX2d_callout, new RectangleF(_two_tone_readings_X_offset + 64, 70, 200, 120), m_bDX2_PeakBlobText, DrawTextOptions.None);
                                    _d2dRenderTarget.DrawText(val2, fontDX2d_callout, new RectangleF(_two_tone_readings_X_offset + 114, 70, 200, 120), m_bDX2_PeakBlobText, DrawTextOptions.None);
                                    _d2dRenderTarget.DrawText(val3, fontDX2d_callout, new RectangleF(_two_tone_readings_X_offset + 170, 70, 200, 120), m_bDX2_PeakBlobText, DrawTextOptions.None);
                                    _d2dRenderTarget.DrawText("f0 diff", fontDX2d_callout, new RectangleF(_two_tone_readings_X_offset + 190, 166, 200, 120), m_bDX2_PeakBlobText, DrawTextOptions.None);
                                }
                                else
                                {
                                    _two_tone_readings_X_offset = 50;
                                    _d2dRenderTarget.DrawText("Peaks not found !\n\nEnsure that IMD3 lower/upper and\nIMD5 lower/upper are in the display.", fontDX2d_callout, new RectangleF(_two_tone_readings_X_offset + 10, 54, 200, 120), m_bDX2_PeakBlobText, DrawTextOptions.None);
                                }
                            }
                            else
                            {
                                _two_tone_readings_X_offset = 50;
                                _d2dRenderTarget.DrawText("Peaks not found !\n\nTry increasing zoom and/or\nchanging sample rate.\n\nFundamental peak separation needs to be increased.", fontDX2d_callout, new RectangleF(_two_tone_readings_X_offset + 10, 54, 200, 120), m_bDX2_PeakBlobText, DrawTextOptions.None);
                            }
                        }
                    }
                    else if (_ema_dbc != -999) _ema_dbc = -999;
                }

                _d2dRenderTarget.PopAxisAlignedClip();
            }

            if (!bottom)
            {
                max_y = local_max_y;
                max_x = local_max_x;
            }

            if (_showTCISpots) drawSpots(rx, nVerticalShift, W, bottom);

            return true;
        }
        private static int findImd(Maximums[] sorted, int imd, int pixel_jump, int offset, bool low, out int X)
        {
            int jump = (imd - 1) / 2;
            int estimate_pixel_pos;
            if (low)
            {
                estimate_pixel_pos = offset - (jump * pixel_jump);
            }
            else
            {
                estimate_pixel_pos = offset + (jump * pixel_jump);
            }
            int search_range = pixel_jump / 4;

            X = -1;
            int best_index = -1;
            float best_dBm = float.MinValue;
            int best_distance = int.MaxValue;

            for (int i = 0; i < sorted.Length; i++)
            {
                int distance = Math.Abs(sorted[i].X - estimate_pixel_pos);

                if (distance <= search_range)
                {
                    if (sorted[i].max_dBm > best_dBm || (sorted[i].max_dBm == best_dBm && distance < best_distance))
                    {
                        best_dBm = sorted[i].max_dBm;
                        X = sorted[i].X;
                        best_distance = distance;
                        best_index = i;
                    }
                }
            }

            return best_index;
        }

        private static float _fNFshiftDBM = 0;
        public static float NFshiftDBM
        {
            get { return NFshiftDBM; }
            set
            {
                float t = value;
                if (t < -12f) t = 12f;
                if (t > 12f) t = 12f;
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
        //private static void processNoiseFloor(int rx, int averageCount, float averageSum, int width, bool waterfall)
        //{
        //    if (rx == 1 && _bNoiseFloorAlreadyCalculatedRX1) return; // already done, ignore
        //    if (rx == 2 && _bNoiseFloorAlreadyCalculatedRX2) return; // already done, ignore

        //    int fps = waterfall ? m_nFps / waterfall_update_period : m_nFps;

        //    int requireSamples = (int)(width * (_NFsensitivity / 20f));
        //    if (rx == 1)
        //    {
        //        if (averageCount >= requireSamples)
        //        {
        //            float linearAverage = averageSum / (float)averageCount;
        //            //float oldLinear = (float)Math.Pow(10f, m_fFFTBinAverageRX1 / 10f);
        //            float oldLinear = fastPow10Raw(m_fFFTBinAverageRX1);
        //            float newLinear = (linearAverage + oldLinear) / 2f;
        //            float tmp = (float)(10f * Math.Log10(newLinear));
        //            if (!float.IsNaN(tmp)) m_fFFTBinAverageRX1 = tmp;
        //        }
        //        else
        //        {
        //            m_fFFTBinAverageRX1 += m_bFastAttackNoiseFloorRX1 ? 3f : 1f;
        //        }
        //        m_fFFTBinAverageRX1 = m_fFFTBinAverageRX1.Clamp(-200f, 200f);

        //        // so in attackTime period we need to have moved to where we want
        //        int framesInAttackTime = m_bFastAttackNoiseFloorRX1 ? 0 : (int)((fps / 1000f) * (double)m_fAttackTimeInMSForRX1);
        //        framesInAttackTime += 1;

        //        if (m_fLerpAverageRX1 > m_fFFTBinAverageRX1)
        //            m_fLerpAverageRX1 -= (m_fLerpAverageRX1 - m_fFFTBinAverageRX1) / framesInAttackTime;
        //        else if (m_fLerpAverageRX1 < m_fFFTBinAverageRX1)
        //            m_fLerpAverageRX1 += (m_fFFTBinAverageRX1 - m_fLerpAverageRX1) / framesInAttackTime;

        //        if (m_bFastAttackNoiseFloorRX1 && (Math.Abs(m_fFFTBinAverageRX1 - m_fLerpAverageRX1) < 1f))
        //        {
        //            float tmpDelay = Math.Max(1000f, _fft_fill_timeRX1 + (_wdsp_mox_transition_buffer_clear ? _fft_fill_timeRX1 : 0)); // extra
        //            bool bElapsed = (m_objFrameStartTimer.ElapsedMsec - _fLastFastAttackEnabledTimeRX1) > tmpDelay; //[2.10.1.0] MW0LGE change to time related, instead of frame related
        //            if(bElapsed) m_bFastAttackNoiseFloorRX1 = false;
        //        }

        //        _bNoiseFloorAlreadyCalculatedRX1 = true;
        //    }
        //    else
        //    {
        //        if (averageCount >= requireSamples)
        //        {
        //            float linearAverage = averageSum / (float)averageCount;
        //            //float oldLinear = (float)Math.Pow(10f, m_fFFTBinAverageRX2 / 10f);
        //            float oldLinear = fastPow10Raw(m_fFFTBinAverageRX2);
        //            float newLinear = (linearAverage + oldLinear) / 2f;
        //            float tmp = (float)(10f * Math.Log10(newLinear));
        //            if (!float.IsNaN(tmp)) m_fFFTBinAverageRX2 = tmp;
        //        }
        //        else
        //        {
        //            m_fFFTBinAverageRX2 += m_bFastAttackNoiseFloorRX2 ? 3f : 1f;
        //        }
        //        m_fFFTBinAverageRX2 = m_fFFTBinAverageRX2.Clamp(-200f, 200f);

        //        // so in attackTime period we need to have moved to where we want
        //        int framesInAttackTime = m_bFastAttackNoiseFloorRX2 ? 0 : (int)((fps / 1000f) * (double)m_fAttackTimeInMSForRX2);
        //        framesInAttackTime += 1;

        //        if (m_fLerpAverageRX2 > m_fFFTBinAverageRX2)
        //            m_fLerpAverageRX2 -= (m_fLerpAverageRX2 - m_fFFTBinAverageRX2) / framesInAttackTime;
        //        else if (m_fLerpAverageRX2 < m_fFFTBinAverageRX2)
        //            m_fLerpAverageRX2 += (m_fFFTBinAverageRX2 - m_fLerpAverageRX2) / framesInAttackTime;

        //        if (m_bFastAttackNoiseFloorRX2 && (Math.Abs(m_fFFTBinAverageRX2 - m_fLerpAverageRX2) < 1f))
        //        {
        //            float tmpDelay = Math.Max(1000f, _fft_fill_timeRX2 + (_wdsp_mox_transition_buffer_clear ? _fft_fill_timeRX2 : 0)); // extra
        //            bool bElapsed = (m_objFrameStartTimer.ElapsedMsec - _fLastFastAttackEnabledTimeRX2) > tmpDelay; //[2.10.1.0] MW0LGE change to time related, instead of frame related
        //            if(bElapsed) m_bFastAttackNoiseFloorRX2 = false;
        //        }

        //        _bNoiseFloorAlreadyCalculatedRX2 = true;
        //    }
        //}
        private static void processNoiseFloor(int rx, int averageCount, float averageSum, int width, bool waterfall)
        {
            //[2.10.3.9]MW0LGE refactor to use refs, simplifies the code, removes unnecessary branching, general speed improvements
            if (rx != 1 && rx != 2) return;
            ref bool bAlreadyCalculated = ref (rx == 1 ? ref _bNoiseFloorAlreadyCalculatedRX1 : ref _bNoiseFloorAlreadyCalculatedRX2);
            if (bAlreadyCalculated) return;

            int fpsComputed = waterfall ? m_nFps / waterfall_update_period : m_nFps;
            int requireSamples = (int)(width * (_NFsensitivity / 20f));

            ref bool fastAttack = ref (rx == 1 ? ref m_bFastAttackNoiseFloorRX1 : ref m_bFastAttackNoiseFloorRX2);
            ref float fftBinAverage = ref (rx == 1 ? ref m_fFFTBinAverageRX1 : ref m_fFFTBinAverageRX2);
            ref float lerpAverage = ref (rx == 1 ? ref m_fLerpAverageRX1 : ref m_fLerpAverageRX2);
            ref float attackTimeInMs = ref (rx == 1 ? ref m_fAttackTimeInMSForRX1 : ref m_fAttackTimeInMSForRX2);
            ref double lastFastAttackTime = ref (rx == 1 ? ref _fLastFastAttackEnabledTimeRX1 : ref _fLastFastAttackEnabledTimeRX2);
            ref float fftFillTime = ref (rx == 1 ? ref _fft_fill_timeRX1 : ref _fft_fill_timeRX2);

            if (averageCount >= requireSamples)
            {
                float linearAverage = averageSum / (float)averageCount;
                float oldLinear = fastPow10Raw(fftBinAverage);
                float newLinear = (linearAverage + oldLinear) * 0.5f;

                fftBinAverage = 10f * (float)Math.Log10(newLinear + 1e-60); // 1e-60 fix potential NaN
            }
            else
            {
                fftBinAverage += fastAttack ? 3f : 1f;
            }

            fftBinAverage = fftBinAverage < -200f ? -200f : fftBinAverage > 200f ? 200f : fftBinAverage;

            int framesInAttack = fastAttack ? 0 : (int)((fpsComputed / 1000f) * (double)attackTimeInMs);
            framesInAttack++;

            float difference = lerpAverage - fftBinAverage;
            lerpAverage -= difference / framesInAttack;

            if (fastAttack)
            {
                float tmpDelay = Math.Max(1000f, fftFillTime + (_wdsp_mox_transition_buffer_clear ? fftFillTime : 0f));
                double elapsed = m_objFrameStartTimer.ElapsedMsec - lastFastAttackTime;
                if (elapsed > tmpDelay) fastAttack = false;
            }

            bAlreadyCalculated = true;
        }


        public static void ResetWaterfallTimers()
        {
            m_nRX1WaterFallFrameCount = 0;
            m_nRX2WaterFallFrameCount = 0;
        }

        private static int m_nRX1WaterFallFrameCount = 0; // 1=every frame, 2= every other, etc
        private static int m_nRX2WaterFallFrameCount = 0;

        private static Color[] _rx1_waterfall_grad = new Color[101];
        private static Color[] _rx2_waterfall_grad = new Color[101];
        private static bool _rx1_waterfall_grad_ok = false;
        private static bool _rx2_waterfall_grad_ok = false;
        private static Color[] _tx_waterfall_grad = new Color[101];
        private static bool _tx_waterfall_grad_ok = false;

        private static void OnWaterfallRXGradientChanged(int rx, Color[] colours)
        {
            if (colours.Length != 101) return;

            Color[] cols;
            if (rx == 1)
            {
                _rx1_waterfall_grad_ok = false;
                cols = _rx1_waterfall_grad;
            }
            else if (rx == 2)
            {
                _rx2_waterfall_grad_ok = false;
                cols = _rx2_waterfall_grad;
            }
            else
                return;

            for (int perc = 0; perc <= 100; perc++)
            {
                cols[perc] = Color.FromArgb(255, colours[perc]);
            }

            if (rx == 1)
            {
                _rx1_waterfall_grad_ok = true;
            }
            else if (rx == 2)
            {
                _rx2_waterfall_grad_ok = true;
            }
        }
        private static void OnWaterfallTXGradientChanged(Color[] colours)
        {
            if (colours.Length != 101) return;

            _tx_waterfall_grad_ok = false;
            Color[] cols = _tx_waterfall_grad;
            for (int perc = 0; perc <= 100; perc++)
            {
                cols[perc] = Color.FromArgb(255, colours[perc]);
            }
            _tx_waterfall_grad_ok = true;
        }
        private static bool _old_power = false;
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
            bool local_mox = localMox(rx);
            float local_min_y_w3sz = float.MaxValue;
            float display_min_w3sz = float.MaxValue;
            float display_max_w3sz = float.MinValue;
            float min_y_w3sz = float.MaxValue;
            int R = 0, G = 0, B = 0;
            bool displayduplex = isRxDuplex(rx);
            float low_threshold = 0.0f;
            float high_threshold = 0.0f;
            float waterfall_minimum = 200f;
            ColorScheme cScheme = ColorScheme.enhanced;
            Color low_color = Color.Black;

            bool bDoVisualNotch = false;
            int nDecimatedWidth = W / m_nDecimation;

            if (console.PowerOn != _old_power)
            {
                _old_power = console.PowerOn;
                _RX1waterfallPreviousMinValue = 20;
                _RX2waterfallPreviousMinValue = 20;
            }

            if (rx == 2)
            {
                if (local_mox)
                {
                    low_threshold = (float)TXWFAmpMin;
                    high_threshold = (float)TXWFAmpMax;
                    cScheme = _tx_color_scheme;
                    low_color = waterfall_low_color_tx;
                }
                else
                {
                    high_threshold = rx2_waterfall_high_threshold;
                    if (rx2_waterfall_agc && !m_bRX2_spectrum_thresholds)
                    {
                        if (m_bWaterfallUseNFForACGRX2)
                        {
                            if (FastAttackNoiseFloorRX2)
                            {
                                low_threshold = _RX2waterfallPreviousMinValue;
                                //note: no adjust if using old value
                            }
                            else
                            {
                                low_threshold = m_fLerpAverageRX2;
                                low_threshold -= m_fWaterfallAGCOffsetRX2;
                            }
                        }
                        else
                        {
                            low_threshold = _RX2waterfallPreviousMinValue;
                            low_threshold -= m_fWaterfallAGCOffsetRX2;
                        }
                    }
                    else
                    {
                        low_threshold = rx2_waterfall_low_threshold;
                    }
                    cScheme = _rx2_color_scheme;
                    low_color = rx2_waterfall_low_color;
                }                                
            }
            else
            {
                if (local_mox)
                {
                    low_threshold = (float)TXWFAmpMin;
                    high_threshold = (float)TXWFAmpMax;
                    cScheme = _tx_color_scheme;
                    low_color = waterfall_low_color_tx;
                }
                else
                {
                    high_threshold = waterfall_high_threshold;
                    if (rx1_waterfall_agc && !m_bRX1_spectrum_thresholds)
                    {
                        if (m_bWaterfallUseNFForACGRX1)
                        {
                            if (FastAttackNoiseFloorRX1)
                            {
                                low_threshold = _RX1waterfallPreviousMinValue;
                                //note: no adjust if using old value
                            }
                            else
                            {
                                low_threshold = m_fLerpAverageRX1;
                                low_threshold -= m_fWaterfallAGCOffsetRX1;
                            }
                        }
                        else
                        {
                            low_threshold = _RX1waterfallPreviousMinValue;
                            low_threshold -= m_fWaterfallAGCOffsetRX1;
                        }
                    }
                    else
                    {
                        low_threshold = waterfall_low_threshold;
                    }
                    cScheme = _rx1_color_scheme;
                    low_color = waterfall_low_color;
                }                                
            }           

            if (console.PowerOn)
            {
                if (rx == 1 && waterfall_data_ready)
                {
                    bDoVisualNotch = true;
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

                    // make copy of the data so visual notch does not change the average noise floor
                    fixed (void* rptr = &current_waterfall_data[0])
                    fixed (void* wptr = &current_waterfall_data_copy[0])
                        Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));

                    waterfall_data_ready = false;
                }
                else if (rx == 2 && waterfall_data_ready_bottom)
                {
                    bDoVisualNotch = true;
                    if (local_mox && (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        for (int i = 0; i < nDecimatedWidth; i++)
                            current_waterfall_data_bottom[i] = -200.0f;
                    }
                    else
                    {
                        fixed (void* rptr = &new_waterfall_data_bottom[0])
                        fixed (void* wptr = &current_waterfall_data_bottom[0])
                            Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));
                    }

                    // make copy of the data so visual notch does not change the average noise floor
                    fixed (void* rptr = &current_waterfall_data_bottom[0])
                    fixed (void* wptr = &current_waterfall_data_bottom_copy[0])
                        Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));

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

                if (bRXdraw)
                {
                    float[] data;
                    float[] dataCopy;

                    if (rx == 1)
                    {
                        data = current_waterfall_data;
                        dataCopy = current_waterfall_data_copy;
                    }
                    else // rx2
                    {
                        data = current_waterfall_data_bottom;
                        dataCopy = current_waterfall_data_bottom_copy;
                    }

                    float max;
                    float max_copy;                    

                    if (!local_mox)
                    {
                        if (bDoVisualNotch && m_bShowVisualNotch)
                        {
                            // modify the data for visual notches
                            modifyDataForNotches(ref data, rx, bottom, local_mox, displayduplex, W);
                        }
                    }

                    //MW0LGE [2.9.0.7]
                    float fOffset;
                    if (rx == 1)
                        fOffset = RX1Offset;
                    else
                        fOffset = RX2Offset;

                    float averageSum = 0;
                    int averageCount = 0;
                    float currentAverage = rx == 1 ? m_fFFTBinAverageRX1 + 2 : m_fFFTBinAverageRX2 + 2;

                    for (int i = 0; i < nDecimatedWidth; i++)
                    {
                        max = data[i] + fOffset;
                        max_copy = dataCopy[i] + fOffset;

                        // noise floor
                        if (!local_mox && (max_copy < currentAverage))
                        {
                            //averageSum += (float)Math.Pow(10f, max_copy / 10f);
                            averageSum += fastPow10Raw(max_copy);
                            averageCount++;
                        }
                        //

                        if (max_copy > local_max_y)
                        {
                            local_max_y = max_copy; //[2.10.3.9]MW0LGE changed from max
                            max_x = i * m_nDecimation;
                        }

                        //below added by w3sz
                        if (max_copy < local_min_y_w3sz)
                        {
                            local_min_y_w3sz = max_copy; //[2.10.3]MW0LGE use unmodified, not the notced data
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
                                m_fNoiseFloorRX1 = m_fLerpAverageRX1 + _fNFshiftDBM;
                                m_bNoiseFloorGoodRX1 = true;
                            }
                        }
                        else
                        {
                            if (!m_bFastAttackNoiseFloorRX2 && !bPreviousRX2)
                            {
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
                            new BitmapProperties(new SDXPixelFormat(_waterfall_bmp_dx2d.PixelFormat.Format, ALPHA_MODE)));

                        topPixels.CopyFromBitmap(_waterfall_bmp_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)topPixels.Size.Width, (int)topPixels.Size.Height));
                    }
                    else //rx2
                    {
                        topPixels = new SharpDX.Direct2D1.Bitmap(_d2dRenderTarget, new Size2((int)_waterfall_bmp2_dx2d.Size.Width, (int)_waterfall_bmp2_dx2d.Size.Height - 1),
                            new BitmapProperties(new SDXPixelFormat(_waterfall_bmp2_dx2d.PixelFormat.Format, ALPHA_MODE)));

                        topPixels.CopyFromBitmap(_waterfall_bmp2_dx2d, new SharpDX.Point(0, 0), new SharpDX.Rectangle(0, 0, (int)topPixels.Size.Width, (int)topPixels.Size.Height));
                    }

                    #region colours
                    switch (cScheme)
                    {
                        case (ColorScheme.Custom):
                            {
                                Color[] cols;
                                if (local_mox)
                                {
                                    if (!_tx_waterfall_grad_ok) break;
                                    cols = _tx_waterfall_grad;
                                }
                                else
                                {
                                    if (rx == 1)
                                    {
                                        if (!_rx1_waterfall_grad_ok) break;
                                        cols = _rx1_waterfall_grad;
                                    }
                                    else
                                    {
                                        if (!_rx2_waterfall_grad_ok) break;
                                        cols = _rx2_waterfall_grad;
                                    }
                                }
                                
                                for (int i = 0; i < nDecimatedWidth; i++)   // for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = cols[0].R;
                                        G = cols[0].G;
                                        B = cols[0].B;
                                    }
                                    else if (waterfall_data[i] >= high_threshold)
                                    {
                                        R = cols[100].R;
                                        G = cols[100].G;
                                        B = cols[100].B;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = high_threshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold;
                                        float overall_percent = offset / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.
                                        int perc = (int)(overall_percent * 100f);

                                        R = cols[perc].R;
                                        G = cols[perc].G;
                                        B = cols[perc].B;
                                    }

                                    if (waterfall_minimum > dataCopy[i] + fOffset) //[2.10.3]MW0LGE use non notched data
                                    waterfall_minimum = dataCopy[i] + fOffset;

                                    // set pixel color
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)B;    // set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)R;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        case (ColorScheme.original):
                            {

                            }
                            break;

                        case (ColorScheme.enhanced):
                            {
                                // draw new data
                                for (int i = 0; i < nDecimatedWidth; i++)   // for each pixel in the new line
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

                                    if (waterfall_minimum > dataCopy[i] + fOffset) //[2.10.3]MW0LGE use non notched data
                                        waterfall_minimum = dataCopy[i] + fOffset;

                                    // set pixel color
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)B;    // set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)R;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        case (ColorScheme.SPECTRAN):
                            {
                                // draw new data
                                for (int i = 0; i < nDecimatedWidth; i++)   // for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= high_threshold) // white
                                    {
                                        R = 240;
                                        G = 240;
                                        B = 240;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = high_threshold - low_threshold;
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

                                    if (waterfall_minimum > dataCopy[i] + fOffset) //[2.10.3]MW0LGE use non notched data
                                        waterfall_minimum = dataCopy[i] + fOffset;

                                    // set pixel color
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)B;    // set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)R;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        case (ColorScheme.BLACKWHITE):
                            {
                                // draw new data
                                for (int i = 0; i < nDecimatedWidth; i++)   // for each pixel in the new line
                                {
                                    if (waterfall_data[i] <= low_threshold)
                                    {
                                        R = 0;
                                        G = 0;
                                        B = 0;
                                    }
                                    else if (waterfall_data[i] >= high_threshold) // white
                                    {
                                        R = 255;
                                        G = 255;
                                        B = 255;
                                    }
                                    else // value is between low and high
                                    {
                                        float range = high_threshold - low_threshold;
                                        float offset = waterfall_data[i] - low_threshold;
                                        float local_percent = ((100.0f * offset) / range);
                                        R = (int)((local_percent / 100) * 255);
                                        G = R;
                                        B = R;
                                    }

                                    if (waterfall_minimum > dataCopy[i] + fOffset) //[2.10.3]MW0LGE use non notched data
                                        waterfall_minimum = dataCopy[i] + fOffset;

                                    // set pixel color
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)B;    // set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)R;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        case (ColorScheme.LinLog):
                            {
                                for (int i = 0; i < nDecimatedWidth; i++)   // for each pixel in the new line
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

                                    if (waterfall_minimum > dataCopy[i] + fOffset) //[2.10.3]MW0LGE use non notched data
                                        waterfall_minimum = dataCopy[i] + fOffset;

                                    // set pixel color changed by w3sz
                                    //[2.10.3.5]MW0LGE note these are reverse RGB, we normally expect BGRA #289
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)R;    // set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)B;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        //  now Linrad palette without log

                        case (ColorScheme.LinRad):
                            {
                                for (int i = 0; i < nDecimatedWidth; i++)   // for each pixel in the new line
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

                                    if (waterfall_minimum > dataCopy[i] + fOffset) //[2.10.3]MW0LGE use non notched data
                                        waterfall_minimum = dataCopy[i] + fOffset;

                                    //[2.10.3.5]MW0LGE note these are reverse RGB, we normally expect BGRA #289
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)R;    // set color in memory
                                    row[(i * m_nDecimation) * pixel_size + 1] = (byte)G;
                                    row[(i * m_nDecimation) * pixel_size + 2] = (byte)B;
                                    row[(i * m_nDecimation) * pixel_size + 3] = nbBitmapAlpaha;
                                }
                            }
                            break;

                        //  now Linrad palette without log

                        case (ColorScheme.LinAuto):
                            {
                                for (int i = 0; i < nDecimatedWidth; i++)   // for each pixel in the new line
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
                                        float offset = waterfall_data[i] - display_min_w3sz;
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
                                    //[2.10.3.5]MW0LGE note these are reverse RGB, we normally expect BGRA #289
                                    row[(i * m_nDecimation) * pixel_size + 0] = (byte)R;    // set color in memory
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
                        if (!(m_bStopRX1WaterfallOnTX && local_mox))
                        {
                            _waterfall_bmp_dx2d.CopyFromMemory(row, W * pixel_size, new SharpDX.Rectangle(0, 0, W, 1));
                            _waterfall_bmp_dx2d.CopyFromBitmap(topPixels, new SharpDX.Point(0, 1));
                        }
                    }
                    else
                    {
                        if (!(m_bStopRX2WaterfallOnTX && local_mox))
                        { 
                            _waterfall_bmp2_dx2d.CopyFromMemory(row, W * pixel_size, new SharpDX.Rectangle(0, 0, W, 1));
                            _waterfall_bmp2_dx2d.CopyFromBitmap(topPixels, new SharpDX.Point(0, 1));
                        }
                    }

                    Utilities.Dispose(ref topPixels);
                    topPixels = null;

                    bool bIgnoreAgc = (rx == 1 && _ignore_waterfall_rx1_agc && (m_objFrameStartTimer.ElapsedMsec < _rx1_no_agc_duration)) ||
                                        (rx == 2 && _ignore_waterfall_rx2_agc && (m_objFrameStartTimer.ElapsedMsec < _rx2_no_agc_duration));
                    
                    if (!bIgnoreAgc)
                    {
                        if (rx == 1)
                            _ignore_waterfall_rx1_agc = false;
                        else
                            _ignore_waterfall_rx2_agc = false;
                    }

                    if (!local_mox && !bIgnoreAgc)
                    {
                        //if (rx == 1)
                        //    _RX1waterfallPreviousMinValue = (((_RX1waterfallPreviousMinValue * 8) + (waterfall_minimum * 2)) / 10) + 1; //wfagc
                        //else
                        //    _RX2waterfallPreviousMinValue = (((_RX2waterfallPreviousMinValue * 8) + (waterfall_minimum * 2)) / 10) + 1; //wfagc

                        if (rx == 1)
                        {
                            if (rx1_waterfall_agc && !m_bRX1_spectrum_thresholds && m_bWaterfallUseNFForACGRX1)
                            {
                                _RX1waterfallPreviousMinValue = (_RX1waterfallPreviousMinValue * 0.6f) + (low_threshold * 0.4f);
                            }
                            else
                            {
                                _RX1waterfallPreviousMinValue = (_RX1waterfallPreviousMinValue * 0.6f) + (waterfall_minimum * 0.4f);
                            }
                        }
                        else
                        {
                            if (rx2_waterfall_agc && !m_bRX2_spectrum_thresholds && m_bWaterfallUseNFForACGRX2)
                            {
                                _RX2waterfallPreviousMinValue = (_RX2waterfallPreviousMinValue * 0.6f) + (low_threshold * 0.4f);
                            }
                            else
                            {
                                _RX2waterfallPreviousMinValue = (_RX2waterfallPreviousMinValue * 0.6f) + (waterfall_minimum * 0.4f);
                            }
                        }
                    }
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
            //if (grid_control_major)  //[2.10.3.9]MW0LGE
            drawPanadapterAndWaterfallGridDX2D(nVerticalShift, W, H, rx, bottom, out long left_edge, out long right_edge, true);

            if (_showTCISpots) drawSpots(rx, nVerticalShift, W, bottom);

            //DebugText = $"previous low : {_RX1waterfallPreviousMinValue.ToString("F2")}\nlow : {low_threshold.ToString("F2")}\nhigh : {high_threshold.ToString("F2")}";

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

                if (image != null)
                {
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
            BitmapProperties bitmapProperties = new BitmapProperties(new SDXPixelFormat(Format.B8G8R8A8_UNorm, ALPHA_MODE)); //was R8G8B8A8_UNorm  //MW0LGE_21k9
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
        private static SharpDX.Direct2D1.Brush m_bDX2_data_fill_fpen_brush_tx;
        private static SharpDX.Direct2D1.Brush m_bDX2_data_line_pen_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_data_line_pen_brush_tx;
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
        private static SharpDX.Color4 m_cDX2_display_background_clear_colour;

        private static SharpDX.Direct2D1.Brush m_bDX2_y1_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_y2_brush;
        private static SharpDX.Direct2D1.Brush m_bDX2_waveform_line_pen;

        private static SharpDX.Direct2D1.Brush m_bDX2_dhp;
        private static SharpDX.Direct2D1.Brush m_bDX2_dhp1;
        private static SharpDX.Direct2D1.Brush m_bDX2_dhp2;

        private static SharpDX.Direct2D1.StrokeStyle m_styleDots;

        private static SharpDX.Direct2D1.Brush m_bDX2_noisefloor;
        private static SharpDX.Direct2D1.Brush m_bDX2_noisefloor_text;

        private static SharpDX.Direct2D1.Brush m_bDX2_m_bHightlightNumberScale;
        private static SharpDX.Direct2D1.Brush m_bDX2_m_bHightlightNumbers;
        //--------------------------
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataFillRX1 = null;
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataLineRX1 = null;
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataFillRX2 = null;
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataLineRX2 = null;

        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataFillTX_RX1 = null;
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataLineTX_RX1 = null;
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataFillTX_RX2 = null;
        private static SharpDX.Direct2D1.LinearGradientBrush m_brushLGDataLineTX_RX2 = null;
        //
        private static bool m_bUseLinearGradient = false;
        private static bool m_bUseLinearGradientForDataLine = false;
        private static bool m_bUseLinearGradientTX = false;
        private static bool m_bUseLinearGradientForDataLineTX = false;
        public static bool UseLinearGradient
        {
            get { return m_bUseLinearGradient; }
            set
            {
                m_bUseLinearGradient = value;
                if (m_bUseLinearGradient)
                {
                    _bRebuildRXLinearGradBrush = true;
                }
            }
        }
        public static bool UseLinearGradientForDataLine
        {
            get { return m_bUseLinearGradientForDataLine; }
            set
            {
                m_bUseLinearGradientForDataLine = value;
            }
        }
        public static bool UseLinearGradientTX
        {
            get { return m_bUseLinearGradientTX; }
            set
            {
                m_bUseLinearGradientTX = value;
                if (m_bUseLinearGradientTX)
                {
                    _bRebuildTXLinearGradBrush = true;
                }
            }
        }
        public static bool UseLinearGradientForDataLineTX
        {
            get { return m_bUseLinearGradientForDataLineTX; }
            set
            {
                m_bUseLinearGradientForDataLineTX = value;
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
        private static void buildLinearGradientBrushTX(int top, int bottom, int rx)
        {
            int grid_min, grid_max;
            grid_min = tx_spectrum_grid_min;
            grid_max = tx_spectrum_grid_max;

            List<ucLGPicker.ColourGradientData> lst = console.SetupForm.TXGradPicker.GetColourGradientDataForDBMRange(grid_min, grid_max);

            GradientStop[] gradientStopsDataFill = new GradientStop[lst.Count];
            GradientStop[] gradientStopsDataLine = new GradientStop[lst.Count];
            for (int n = 0; n < lst.Count; n++)
            {
                Color dataFillColour = Color.FromArgb(data_fill_color_tx.A, lst[n].color.R, lst[n].color.G, lst[n].color.B);
                Color dataLineColour = Color.FromArgb(tx_data_line_color.A, lst[n].color.R, lst[n].color.G, lst[n].color.B);

                gradientStopsDataFill[n] = new GradientStop() { Color = convertColour(dataFillColour), Position = lst[n].percent };
                gradientStopsDataLine[n] = new GradientStop() { Color = convertColour(dataLineColour), Position = lst[n].percent };
            }
            SharpDX.Direct2D1.GradientStopCollection fill = new SharpDX.Direct2D1.GradientStopCollection(_d2dRenderTarget, gradientStopsDataFill);
            SharpDX.Direct2D1.GradientStopCollection line = new SharpDX.Direct2D1.GradientStopCollection(_d2dRenderTarget, gradientStopsDataLine);

            if (rx == 1)
            {
                if (m_brushLGDataFillTX_RX1 != null)
                {
                    Utilities.Dispose(ref m_brushLGDataFillTX_RX1);
                    m_brushLGDataFillTX_RX1 = null;
                }
                m_brushLGDataFillTX_RX1 = new SharpDX.Direct2D1.LinearGradientBrush(_d2dRenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
                {
                    StartPoint = new Vector2(0, bottom),
                    EndPoint = new Vector2(0, top)
                },
                fill);
                if (m_brushLGDataLineTX_RX1 != null)
                {
                    Utilities.Dispose(ref m_brushLGDataLineTX_RX1);
                    m_brushLGDataLineTX_RX1 = null;
                }
                m_brushLGDataLineTX_RX1 = new SharpDX.Direct2D1.LinearGradientBrush(_d2dRenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
                {
                    StartPoint = new Vector2(0, bottom),
                    EndPoint = new Vector2(0, top)
                },
                line);
            }
            else
            {
                if (m_brushLGDataFillTX_RX2 != null)
                {
                    Utilities.Dispose(ref m_brushLGDataFillTX_RX2);
                    m_brushLGDataFillTX_RX2 = null;
                }
                m_brushLGDataFillTX_RX2 = new SharpDX.Direct2D1.LinearGradientBrush(_d2dRenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
                {
                    StartPoint = new Vector2(0, bottom),
                    EndPoint = new Vector2(0, top)
                },
                fill);
                if (m_brushLGDataLineTX_RX2 != null)
                {
                    Utilities.Dispose(ref m_brushLGDataLineTX_RX2);
                    m_brushLGDataLineTX_RX2 = null;
                }
                m_brushLGDataLineTX_RX2 = new SharpDX.Direct2D1.LinearGradientBrush(_d2dRenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
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
            _bRebuildRXLinearGradBrush = false;

            if (m_brushLGDataFillTX_RX1 != null) Utilities.Dispose(ref m_brushLGDataFillTX_RX1);
            if (m_brushLGDataLineTX_RX1 != null) Utilities.Dispose(ref m_brushLGDataLineTX_RX1);
            if (m_brushLGDataFillTX_RX2 != null) Utilities.Dispose(ref m_brushLGDataFillTX_RX2);
            if (m_brushLGDataLineTX_RX2 != null) Utilities.Dispose(ref m_brushLGDataLineTX_RX2);
            _bRebuildTXLinearGradBrush = false;

            if (m_bDX2_dataPeaks_fill_fpen_brush != null) Utilities.Dispose(ref m_bDX2_dataPeaks_fill_fpen_brush);
            if (m_bDX2_data_fill_fpen_brush != null) Utilities.Dispose(ref m_bDX2_data_fill_fpen_brush);
            if (m_bDX2_data_fill_fpen_brush_tx != null) Utilities.Dispose(ref m_bDX2_data_fill_fpen_brush_tx);
            if (m_bDX2_data_line_pen_brush != null) Utilities.Dispose(ref m_bDX2_data_line_pen_brush);
            if (m_bDX2_data_line_pen_brush_tx != null) Utilities.Dispose(ref m_bDX2_data_line_pen_brush_tx);
            if (m_bDX2_tx_data_line_fpen_brush != null) Utilities.Dispose(ref m_bDX2_tx_data_line_fpen_brush);
            if (m_bDX2_tx_data_line_pen_brush != null) Utilities.Dispose(ref m_bDX2_tx_data_line_pen_brush);

            if (m_bDX2_p1 != null) Utilities.Dispose(ref m_bDX2_p1);
            if (m_bDX2_display_background_brush != null) Utilities.Dispose(ref m_bDX2_display_background_brush);

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
            if (m_bDX2_noisefloor_text != null) Utilities.Dispose(ref m_bDX2_noisefloor_text);

            //
            m_brushLGDataFillRX1 = null;
            m_brushLGDataFillRX2 = null;
            m_brushLGDataLineRX1 = null;
            m_brushLGDataLineRX2 = null;

            m_brushLGDataFillTX_RX1 = null;
            m_brushLGDataLineTX_RX1 = null;
            m_brushLGDataFillTX_RX2 = null;
            m_brushLGDataLineTX_RX2 = null;

            m_bDX2_dataPeaks_fill_fpen_brush = null;
            m_bDX2_data_fill_fpen_brush = null;
            m_bDX2_data_fill_fpen_brush_tx = null;
            m_bDX2_data_line_pen_brush = null;
            m_bDX2_data_line_pen_brush_tx = null;
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
            m_bDX2_noisefloor_text = null;
            //

        }
        private static void buildDX2Resources()
        {
            lock (_objDX2Lock)
            {
                if (!_bDX2Setup) return;

                releaseDX2Resources();

                _bRebuildRXLinearGradBrush = true;
                _bRebuildTXLinearGradBrush = true;

                m_bDX2_dataPeaks_fill_fpen_brush = convertBrush((SolidBrush)dataPeaks_fill_fpen.Brush);
                m_bDX2_data_fill_fpen_brush = convertBrush((SolidBrush)data_fill_fpen.Brush);
                m_bDX2_data_fill_fpen_brush_tx = convertBrush((SolidBrush)data_fill_fpen_tx.Brush);
                m_bDX2_data_line_pen_brush = convertBrush((SolidBrush)data_line_pen.Brush);
                m_bDX2_data_line_pen_brush_tx = convertBrush((SolidBrush)tx_data_line_pen.Brush);
                m_bDX2_tx_data_line_fpen_brush = convertBrush((SolidBrush)tx_data_line_fpen.Brush);
                m_bDX2_tx_data_line_pen_brush = convertBrush((SolidBrush)tx_data_line_pen.Brush);

                m_bDX2_p1 = convertBrush((SolidBrush)p1.Brush);
                m_bDX2_display_background_brush = convertBrush((SolidBrush)display_background_brush);

                m_cDX2_display_background_colour = convertColour(display_background_brush.Color); // does not need dispose as it is a type
                m_cDX2_display_background_clear_colour = convertColour(Color.FromArgb(255, Color.Black)); // does not need dispose as it is a type

                m_bDX2_m_bHightlightNumbers = getDXBrushForColour(Color.FromArgb(255, 255, 255));
                m_bDX2_m_bHightlightNumberScale = getDXBrushForColour(Color.FromArgb(192, 64, 64, 64));

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

                m_bDX2_Red = getDXBrushForColour(Color.Red);
                m_bDX2_Yellow = getDXBrushForColour(Color.Yellow);
                m_bDX2_YellowGreen = getDXBrushForColour(Color.YellowGreen);
                m_bDX2_Gray = getDXBrushForColour(Color.Gray);

                m_bDX2_PeakBlob = getDXBrushForColour(Color.OrangeRed);
                m_bDX2_PeakBlobText = getDXBrushForColour(Color.Chartreuse);

                m_bDX2_y1_brush = getDXBrushForColour(Color.FromArgb(64, 64, 64));
                m_bDX2_y2_brush = getDXBrushForColour(Color.FromArgb(48, 48, 48));
                m_bDX2_waveform_line_pen = getDXBrushForColour(Color.LightGreen);

                m_bDX2_dhp = getDXBrushForColour(Color.FromArgb(0, 255, 0));
                m_bDX2_dhp1 = getDXBrushForColour(Color.FromArgb(150, 0, 0, 255));
                m_bDX2_dhp2 = getDXBrushForColour(Color.FromArgb(150, 255, 0, 0));

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
                m_bDX2_noisefloor_text = convertBrush(new SolidBrush(noisefloor_color_text));
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
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font12;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font14;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font32;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_font1;
        private static SharpDX.DirectWrite.TextFormat fontDX2d_fps_profile;
        //--------------------------
        private static void releaseFonts()
        {
            if (fontDX2d_callout != null) Utilities.Dispose(ref fontDX2d_callout);
            if (fontDX2d_font9 != null) Utilities.Dispose(ref fontDX2d_font9);
            if (fontDX2d_font9b != null) Utilities.Dispose(ref fontDX2d_font9b);
            if (fontDX2d_font9c != null) Utilities.Dispose(ref fontDX2d_font9c);
            if (fontDX2d_panafont != null) Utilities.Dispose(ref fontDX2d_panafont);
            if (fontDX2d_font10 != null) Utilities.Dispose(ref fontDX2d_font10);
            if (fontDX2d_font12 != null) Utilities.Dispose(ref fontDX2d_font12);
            if (fontDX2d_font14 != null) Utilities.Dispose(ref fontDX2d_font14);
            if (fontDX2d_font32 != null) Utilities.Dispose(ref fontDX2d_font32);
            if (fontDX2d_font1 != null) Utilities.Dispose(ref fontDX2d_font1);
            if (fontDX2d_fps_profile != null) Utilities.Dispose(ref fontDX2d_fps_profile);

            if (fontFactory != null) Utilities.Dispose(ref fontFactory);

            fontDX2d_callout = null;
            fontDX2d_font9 = null;
            fontDX2d_font9b = null;
            fontDX2d_font9c = null;
            fontDX2d_panafont = null;
            fontDX2d_font10 = null;
            fontDX2d_font12 = null;
            fontDX2d_font14 = null;
            fontDX2d_font32 = null;
            fontDX2d_font1 = null;
            fontDX2d_fps_profile = null;

            fontFactory = null;
        }
        private static void buildFontsDX2D()
        {
            lock (_objDX2Lock)
            {
                if (!_bDX2Setup) return;

                releaseFonts();

                fontFactory = new SharpDX.DirectWrite.Factory();

                fontDX2d_callout = new SharpDX.DirectWrite.TextFormat(fontFactory, m_fntCallOutFont.FontFamily.Name, (m_fntCallOutFont.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font9 = new SharpDX.DirectWrite.TextFormat(fontFactory, font9.FontFamily.Name, (font9.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font9b = new SharpDX.DirectWrite.TextFormat(fontFactory, font9b.FontFamily.Name, SharpDX.DirectWrite.FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, (font9b.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font9c = new SharpDX.DirectWrite.TextFormat(fontFactory, font95.FontFamily.Name, (font95.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_panafont = new SharpDX.DirectWrite.TextFormat(fontFactory, pana_font.FontFamily.Name, (pana_font.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font10 = new SharpDX.DirectWrite.TextFormat(fontFactory, font10.FontFamily.Name, (font10.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font12 = new SharpDX.DirectWrite.TextFormat(fontFactory, font12.FontFamily.Name, (font12.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font14 = new SharpDX.DirectWrite.TextFormat(fontFactory, font14b.FontFamily.Name, (font14b.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font32 = new SharpDX.DirectWrite.TextFormat(fontFactory, font32b.FontFamily.Name, (font32b.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_font1 = new SharpDX.DirectWrite.TextFormat(fontFactory, font1r.FontFamily.Name, (font1r.Size / 72) * _d2dRenderTarget.DotsPerInch.Width);
                fontDX2d_fps_profile = new SharpDX.DirectWrite.TextFormat(fontFactory, m_fntCallOutFont.FontFamily.Name, (64f / 72) * _d2dRenderTarget.DotsPerInch.Width);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void drawLineDX2D(SharpDX.Direct2D1.Brush b, float x1, float y1, float x2, float y2, float strokeWidth = 1f)
        {
            _d2dRenderTarget.DrawLine(new SharpDX.Vector2(x1, y1), new SharpDX.Vector2(x2, y2), b, strokeWidth);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void drawLineDX2D(SharpDX.Direct2D1.Brush b, float x1, float y1, float x2, float y2, StrokeStyle strokeStyle, float strokeWidth = 1f)
        {
            _d2dRenderTarget.DrawLine(new SharpDX.Vector2(x1, y1), new SharpDX.Vector2(x2, y2), b, strokeWidth, strokeStyle);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void drawFillRectangleDX2D(SharpDX.Direct2D1.Brush b, float x, float y, float w, float h)
        {
            RectangleF rect = new RectangleF(x, y, w, h);
            _d2dRenderTarget.FillRectangle(rect, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void drawRectangleDX2D(SharpDX.Direct2D1.Brush b, float x, float y, float w, float h)
        {
            RectangleF rect = new RectangleF(x, y, w, h);
            _d2dRenderTarget.DrawRectangle(rect, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void drawElipseDX2D(SharpDX.Direct2D1.Brush b, float xMiddle, float yMiddle, float w, float h)
        {
            Ellipse e = new Ellipse(new SharpDX.Vector2(xMiddle, yMiddle), w / 2, h / 2);
            _d2dRenderTarget.DrawEllipse(e, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void drawFillElipseDX2D(SharpDX.Direct2D1.Brush b, float xMiddle, float yMiddle, float w, float h)
        {
            Ellipse e = new Ellipse(new SharpDX.Vector2(xMiddle, yMiddle), w / 2, h / 2);
            _d2dRenderTarget.FillEllipse(e, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void drawRectangleDX2D(SharpDX.Direct2D1.Brush b, Rectangle r, float lineWidth = 1)
        {
            RectangleF rect = new RectangleF(r.X, r.Y, r.Width, r.Height);
            _d2dRenderTarget.DrawRectangle(rect, b, lineWidth);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void drawFillRectangleDX2D(SharpDX.Direct2D1.Brush b, Rectangle r)
        {
            RectangleF rect = new RectangleF(r.X, r.Y, r.Width, r.Height);
            _d2dRenderTarget.FillRectangle(rect, b);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        private const int MAX_STRING_CACHE_ENTRIES = 500;
        private static readonly Dictionary<(string Text, string FontFamily, float Size), System.Drawing.SizeF> m_stringSizeCache = new Dictionary<(string Text, string FontFamily, float Size), System.Drawing.SizeF>(MAX_STRING_CACHE_ENTRIES + 1);
        private static readonly Queue<(string Text, string FontFamily, float Size)> _stringMeasureKeys = new Queue<(string Text, string FontFamily, float Size)>(MAX_STRING_CACHE_ENTRIES + 1);

        private static System.Drawing.SizeF measureStringDX2D(string s, SharpDX.DirectWrite.TextFormat tf, bool cacheStringLength = false)
        {
            // keep cache of calced sizes as this is quite a slow process

            (string Text, string FontFamily, float Size) key;
            if (cacheStringLength)
            {
                key = (s.Length.ToString(), tf.FontFamilyName, tf.FontSize);
            }
            else
            {
                key = (s, tf.FontFamilyName, tf.FontSize);
            }

            if (m_stringSizeCache.TryGetValue(key, out SizeF cached))
                return cached;

            SharpDX.DirectWrite.TextLayout layout = new SharpDX.DirectWrite.TextLayout(fontFactory, s, tf, float.PositiveInfinity, float.PositiveInfinity);
            System.Drawing.SizeF sz = new System.Drawing.SizeF(layout.Metrics.Width, layout.Metrics.Height);
            Utilities.Dispose(ref layout);
            layout = null;

            m_stringSizeCache.Add(key, sz);
            _stringMeasureKeys.Enqueue(key);
            if (m_stringSizeCache.Count > MAX_STRING_CACHE_ENTRIES)
            {
                (string Text, string FontFamily, float Size) oldKey = _stringMeasureKeys.Dequeue();
                m_stringSizeCache.Remove(oldKey); // [2.10.1.0] MW0LGE dictionary is not ordered
            }

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
            set { m_bAlwaysShowCursorInfo = value; }
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
        private class clsNotchCoords
        {
            public int _c_x;
            public int _left_x;
            public int _right_x;
            public bool _Use;
            public int _widthHz;

            public clsNotchCoords(int nC_x, int nLeft_X, int nRight_X, bool bUse, int nWidthHz)
            {
                _c_x = nC_x;
                _left_x = nLeft_X;
                _right_x = nRight_X;
                _Use = bUse;
                _widthHz = nWidthHz;
            }
        }        
        private static List<clsNotchCoords> handleNotches(int rx, bool bottom, int cwSideToneShift, int Low, int High, int nVerticalShift, int top, int width, int W, int H, bool bDraw)//, int expandHz = 0)
        {
            long rf_freq = rx == 1 ? vfoa_hz : vfob_hz;

            int localRit;
            if (rx == 1)
                localRit = _rx1ClickDisplayCTUN ? 0 : rit_hz;
            else
                localRit = 0; // no rit rx2

            rf_freq += cwSideToneShift;

            SharpDX.Direct2D1.Brush p;
            SharpDX.Direct2D1.Brush b;
            SharpDX.Direct2D1.Brush t;

            List<MNotch> notches = MNotchDB.NotchesInBW(rf_freq, Low - console.MaxFilterWidth, High + console.MaxFilterWidth);
            List<clsNotchCoords> notchData = new List<clsNotchCoords>();

            double min_notch_wdith = localMox(rx) ? _mnfMinSizeTX : _mnfMinSizeRX;
            
            foreach (MNotch n in notches)
            {
                int notch_centre_x;
                int notch_left_x;
                int notch_right_x;
                
                if (bDraw)
                {
                    notch_centre_x = (int)((float)((n.FCenter) - rf_freq - Low - localRit) / width * W);
                    notch_left_x = (int)((float)((n.FCenter) - rf_freq - n.FWidth / 2 - Low - localRit) / width * W);
                    notch_right_x = (int)((float)((n.FCenter) - rf_freq + n.FWidth / 2 - Low - localRit) / width * W);
                }
                else
                {
                    double dNewWidth = n.FWidth < min_notch_wdith ? min_notch_wdith : n.FWidth; // use the min width of filter from WDSP
                    dNewWidth += 20; // fudge factor to align better with spectrum notch
                    notch_centre_x = (int)((float)((n.FCenter) - rf_freq - Low - localRit) / width * W);
                    notch_left_x = (int)((float)((n.FCenter) - rf_freq - dNewWidth / 2 - Low - localRit) / width * W);
                    notch_right_x = (int)((float)((n.FCenter) - rf_freq + dNewWidth / 2 - Low - localRit) / width * W);
                }

                clsNotchCoords nc = new clsNotchCoords(notch_centre_x, notch_left_x, notch_right_x, _tnf_active && n.Active, (int)n.FWidth);
                notchData.Add(nc);

                if (bDraw)
                {
                    if (_tnf_active)
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
            }

            return notchData;
        }
        private static bool _joinBandEdges = false;
        public static bool JoinBandEdges
        {
            get { return _joinBandEdges; }
            set { _joinBandEdges = value; }
        }

        private const float Scale10 = 27866352.59211258f;
        private const float Scale = 2786635.259211258f;
        private const int Bias = 0x3f800000;

        // version that expects (dB / 10)
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static unsafe float FastPow10Shifted(float dBdiv10)
        //{
        //    if (dBdiv10 < -20 || dBdiv10 > 20) // mathematical limit calculated: ~ -38.2 to +38.5
        //        return (float)Math.Pow(10.0, dBdiv10);

        //    int bits = (int)(dBdiv10 * Scale10) + Bias;
        //    float ret = *(float*)&bits;

        //    if (float.IsNaN(ret))
        //        ret = (float)Math.Pow(10.0, dBdiv10);

        //    return ret;
        //}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe float fastPow10Shifted(float dBdiv10)
        {
            if (dBdiv10 <= -20f || dBdiv10 >= 20f) return (float)Math.Pow(10.0, dBdiv10);
            int bits = (int)(dBdiv10 * Scale10) + Bias;
            return *(float*)&bits;
        }
        // version that takes the raw dB value (no /10 at call site)
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static unsafe float fastPow10Raw(float dB)
        //{            
        //    if(dB < -200 || dB > 200)  // mathematical limit calculated: ~ -382.3 to +385.3
        //        return (float)Math.Pow(10.0, dB / 10.0);

        //    int bits = (int)(dB * Scale) + Bias;
        //    float ret = *(float*)&bits;

        //    if (float.IsNaN(ret))
        //        ret = (float)Math.Pow(10.0, dB / 10.0);

        //    return ret;
        //}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe float fastPow10Raw(float dB)
        {
            if (dB <= -200f || dB >= 200f) return (float)Math.Pow(10.0, dB / 10.0);
            int bits = (int)(dB * Scale) + Bias;
            return *(float*)&bits;
        }

        private static int drawPanadapterAndWaterfallGridDX2D(int nVerticalShift, int W, int H, int rx, bool bottom, out long left_edge, out long right_edge, bool bIsWaterfall = false)
        {
            // MW0LGE
            // this now draws the grid for either panadapter or waterfall, pass in a bool to pick
            //
            DisplayLabelAlignment label_align = display_label_align;
            bool displayduplex = isRxDuplex(rx);
            bool local_mox = localMox(rx);
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
            int grid_step = 0;
            int f_diff = 0;
            int sample_rate;

            #region setup
            //MW0LGE
            int cwSideToneShift = getCWSideToneShift(rx);
            int cwSideToneShiftInverted = -cwSideToneShift; // invert the sign as cw zero lines/tx lines etc are a shift in opposite direction to the grid

            if (rx == 1)
            {
                if (local_mox)
                {
                    if (displayduplex)
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
            else// rx == 2
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
            else// rx == 2
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

            //-- [2.10.1.0] MW0LGE fix for when split and dup is off. Note dupe always off for rx2
            long localSubDiff;
            if (local_mox)
            {
                if (displayduplex)
                {
                    localSubDiff = vfoa_sub_hz - vfoa_hz;
                }
                else
                {
                    localSubDiff = 0;
                }
            }
            else
            {
                localSubDiff = vfoa_sub_hz - vfoa_hz;
            }

            int local_rit_hz;
            if (local_mox)
            {
                local_rit_hz = 0; // no rit in tx
            }
            else
            {
                if (rx == 1)
                {
                    if (console.CTuneDisplay)
                    {
                        local_rit_hz = 0; // dont move the grid by rit if we have CTUN enabled
                    }
                    else
                    {
                        local_rit_hz = rit_hz;
                    }
                }
                else
                {
                    local_rit_hz = 0; // rit works on rx1 only, does not apply to rx2
                }
            }
            //--
            #endregion

            SharpDX.RectangleF clipRect = new SharpDX.RectangleF(0, nVerticalShift, W, H);
            _d2dRenderTarget.PushAxisAlignedClip(clipRect, AntialiasMode.Aliased);

            #region RX filter, filter lines and sub rx overlay
            if (!local_mox && sub_rx1_enabled && rx == 1) //multi-rx
            {
                int localRit = _rx1ClickDisplayCTUN ? rit_hz : 0;
                if ((bIsWaterfall && m_bShowRXFilterOnWaterfall) || !bIsWaterfall)
                {
                    // draw Sub RX filter
                    // get filter screen coordinates
                    int filter_left_x = (int)((float)(filter_low - Low + localSubDiff + localRit) / width * W);
                    int filter_right_x = (int)((float)(filter_high - Low + localSubDiff + localRit) / width * W);

                    drawFilterOverlayDX2D(m_bDX2_sub_rx_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
                }

                if ((bIsWaterfall && m_bShowRXZeroLineOnWaterfall) || !bIsWaterfall)
                {
                    // draw Sub RX 0Hz line
                    int x = (int)((float)(localSubDiff - Low + localRit) / width * W);
                    drawLineDX2D(m_bDX2_sub_rx_zero_line_pen, x, nVerticalShift + top, x, nVerticalShift + H, 2);
                }
            }

            // RX FILTER overlay + highlight edges
            if ((bIsWaterfall && m_bShowRXFilterOnWaterfall) || !bIsWaterfall)
            {
                if (!local_mox)
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
            if ((rx == 1 && rx1_dsp_mode != DSPMode.CWL && rx1_dsp_mode != DSPMode.CWU) || (rx == 2 && rx2_dsp_mode != DSPMode.CWL && rx2_dsp_mode != DSPMode.CWU)) //MW0LGE [2.9.0.7] +rx2
            {
                if ((bIsWaterfall && m_bShowTXFilterOnRXWaterfall) || !bIsWaterfall)
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

                    int localRit = 0;
                    int localXit;
                    if (local_mox)
                        localXit = displayduplex ? xit_hz : 0;
                    else
                        localXit = xit_hz;
                    if (!split_enabled)
                    {
                        if (!local_mox)
                            localRit = rx == 1 ? rit_hz : 0;
                        filter_left_x = (int)((float)(filter_low_tmp - Low - f_diff + localXit - localRit) / width * W);
                        filter_right_x = (int)((float)(filter_high_tmp - Low - f_diff + localXit - localRit) / width * W);
                    }
                    else // MW0LGE_21k8
                    {
                        if (!local_mox)
                            localRit = rx == 1 && _rx1ClickDisplayCTUN ? 0 : rx == 2 ? 0 : rit_hz;
                        filter_left_x = (int)((float)(filter_low_tmp - Low + localXit + (localSubDiff) - localRit) / width * W);
                        filter_right_x = (int)((float)(filter_high_tmp - Low + localXit + (localSubDiff) - localRit) / width * W);
                    }

                    if (local_mox)
                    {
                        drawFilterOverlayDX2D(m_bDX2_tx_filter_brush, filter_left_x, filter_right_x, W, H, rx, top, bottom, nVerticalShift);
                    }
                    else if (draw_tx_filter)
                    {
                        if ((rx == 2 && _tx_on_vfob) || (rx == 1 && !(_tx_on_vfob && _rx2_enabled)))
                        {
                            drawLineDX2D(m_bDX2_tx_filter_pen, filter_left_x, nVerticalShift + top, filter_left_x, nVerticalShift + H, tx_filter_pen.Width);
                            drawLineDX2D(m_bDX2_tx_filter_pen, filter_right_x, nVerticalShift + top, filter_right_x, nVerticalShift + H, tx_filter_pen.Width);
                        }
                    }
                }
            }
            #endregion

            #region 60m channels
            // draw 60m channels if in view - not on the waterfall //MW0LGE
            if (!bIsWaterfall && (console.CurrentRegion == FRSRegion.US || console.CurrentRegion == FRSRegion.UK))
            {
                foreach (Channel c in Console.Channels60m)
                {
                    long rf_freq = vfoa_hz;
                    int local_rit = _rx1ClickDisplayCTUN ? 0 : rit_hz;
                    if (local_mox) local_rit = 0;

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

                        int chan_left_x = (int)((float)(c.Freq * 1e6 - rf_freq - c.BW / 2 - Low - local_rit) / width * W);
                        int chan_right_x = (int)((float)(c.Freq * 1e6 - rf_freq + c.BW / 2 - Low - local_rit) / width * W);

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

            #region BandStackOverlay
            //MW0LGE_21h
            if (m_bShowBandStackOverlays && m_bandStackOverlays != null && rx == 1 && !local_mox && !bIsWaterfall)
            {
                long rf_freq = vfoa_hz;
                int local_rit = _rx1ClickDisplayCTUN ? 0 : rit_hz;

                SharpDX.Direct2D1.Brush brush;
                for (int n = 0; n < m_bandStackOverlays.Length; n++)
                {
                    int filter_left_x = (int)((float)((((m_bandStackOverlays[n].Frequency * 1e6) - rf_freq) + m_bandStackOverlays[n].LowFilter) - Low - local_rit) / width * W);
                    int filter_right_x = (int)((float)((((m_bandStackOverlays[n].Frequency * 1e6) - rf_freq) + m_bandStackOverlays[n].HighFilter) - Low - local_rit) / width * W);

                    brush = (n == m_nHighlightedBandStackEntryIndex) ? m_bDX2_bandstack_overlay_brush_highlight : m_bDX2_bandstack_overlay_brush;

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
                handleNotches(rx, bottom, cwSideToneShift, Low, High, nVerticalShift, top, width, W, H, true);//, 50); //MW0LGE [2.9.0.7] moved to function so can be used by drawmpana/drawwater                
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
                    if (rx == 1 && !local_mox && !(_rx2_enabled && _tx_on_vfob) &&
                        (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                    {
                        int cw_line_x1;
                        int localRit;
                        if (!split_enabled)
                        {
                            localRit = rit_hz;
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff + xit_hz - localRit) / width * W);
                        }
                        else
                        {
                            localRit = _rx1ClickDisplayCTUN ? 0 : rit_hz;
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + xit_hz - localRit + (localSubDiff)) / width * W);
                        }

                        drawLineDX2D(m_bDX2_tx_filter_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H, tx_filter_pen.Width);
                    }

                    if (rx == 2 && !local_mox && (_rx2_enabled && _tx_on_vfob) &&  //MW0LGE [2.9.0.7] txonb
                        (rx2_dsp_mode == DSPMode.CWL || rx2_dsp_mode == DSPMode.CWU))
                    {
                        int cw_line_x1;
                        if (!split_enabled)
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low - f_diff + xit_hz) / width * W);
                        else
                            cw_line_x1 = (int)((float)(cwSideToneShiftInverted - Low + xit_hz + (localSubDiff)) / width * W);

                        drawLineDX2D(m_bDX2_tx_filter_pen, cw_line_x1, nVerticalShift + top, cw_line_x1, nVerticalShift + H, tx_filter_pen.Width);
                    }
                }
            }
            #endregion

            #region Centre Lines, 0hz line, and Freq offset text
            //      MW0LGE
            if (local_mox)
            {
                int localXit = displayduplex ? xit_hz : 0;

                if (!split_enabled) //MW0LGE_21k8
                {
                    center_line_x = (int)((float)(-f_diff - Low + localXit) / width * W); // locked 0 line
                }
                else
                {
                    center_line_x = (int)((float)(-Low + localXit + (localSubDiff)) / width * W); // locked 0 line
                }
            }
            else
            {
                center_line_x = (int)((float)(-f_diff - Low) / width * W); // locked 0 line
            }

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
            #endregion

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
                    if (split_enabled) vfo = vfoa_sub_hz - (localSubDiff);
                    else vfo = vfoa_hz;

                    vfo += displayduplex ? 0 : xit_hz;
                }
                else if (local_mox && _tx_on_vfob)
                {
                    if (console.RX2Enabled) vfo = vfoa_hz + local_rit_hz;
                    else vfo = vfoa_sub_hz;
                }
                else vfo = vfoa_hz + local_rit_hz;
            }
            else //rx==2
            {
                if (local_mox)
                    vfo = vfob_hz + xit_hz;
                else
                {
                    if (console.VFOSync) vfo = vfob_hz + local_rit_hz;
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

                if (!show_freq_offset)
                {
                    //--------------
                    //MW0LGE
                    bool bBandEdge = false;

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
                    if (_grid_control_major && !bIsWaterfall)
                    {
                        drawLineDX2D(pnMajorLine, vgrid, nVerticalShift + top, vgrid, nVerticalShift + H);

                        if (_grid_control_minor)
                        {
                            int fgrid_2 = ((i + 1) * freq_step_size) + (int)((Low / freq_step_size) * freq_step_size);
                            int x_2 = (int)(((float)(fgrid_2 - vfo_delta - Low) / width * W));
                            float scale = (float)(x_2 - vgrid) / inbetweenies;

                            for (int j = 1; j < inbetweenies; j++)
                            {
                                float x3 = (float)vgrid + (j * scale);

                                drawLineDX2D(pnInbetweenLine, x3, nVerticalShift + top, x3, nVerticalShift + H);
                            }
                        }
                    }

                    if (_show_frequency_numbers)
                    {
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

                        drawStringDX2D(label, fontDX2d_font9, brTextBrush, vgrid - offsetL, nVerticalShift + 4);
                    }
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

                        drawStringDX2D(label, fontDX2d_font9, brBrush, vgrid - offsetL, nVerticalShift + 4);
                    }
                }
            }

            //
            if (!bIsWaterfall && _joinBandEdges)
            { 
                int local_rit_e = rx == 1 ? (_rx1ClickDisplayCTUN ? 0 : rit_hz) : 0;
                long vfoLocal = rx == 1 ? vfoa_hz : vfob_hz;
                SharpDX.Direct2D1.Brush bandEdgeOverheadBrush = local_mox ? m_bDX2_tx_band_edge_pen : m_bDX2_band_edge_pen;

                for (int ii = 0; ii < band_edge_list.Length; ii += 2)
                {
                    int low = band_edge_list[ii];
                    int high = band_edge_list[ii + 1];

                    int lowPixelX = (int)((float)(low - cwSideToneShift - vfoLocal - Low - local_rit_e) / width * W);
                    int highPixelX = (int)((float)(high - cwSideToneShift - vfoLocal - Low - local_rit_e) / width * W);

                    //check if low or high in view
                    bool bLowInPB = (lowPixelX >= 0) && (lowPixelX <= W);
                    bool bHighInPB = (highPixelX >= 0) && (highPixelX <= W);

                    if (bLowInPB)
                    {
                        if (bHighInPB)
                        {
                            // draw between low and high
                            drawLineDX2D(bandEdgeOverheadBrush, lowPixelX, nVerticalShift + 2, highPixelX, nVerticalShift + 2, 1);
                            drawLineDX2D(bandEdgeOverheadBrush, lowPixelX, nVerticalShift + 2, lowPixelX, nVerticalShift + 8, 1);
                            drawLineDX2D(bandEdgeOverheadBrush, highPixelX, nVerticalShift + 2, highPixelX, nVerticalShift + 8, 1);
                        }
                        else
                        {
                            // draw between low and right edge
                            drawLineDX2D(bandEdgeOverheadBrush, lowPixelX, nVerticalShift + 2, W, nVerticalShift + 2, 1);
                            drawLineDX2D(bandEdgeOverheadBrush, lowPixelX, nVerticalShift + 2, lowPixelX, nVerticalShift + 8, 1);
                        }
                    }
                    else if (bHighInPB)
                    {
                        // draw between left edge and high
                        drawLineDX2D(bandEdgeOverheadBrush, 0, nVerticalShift + 2, highPixelX, nVerticalShift + 2, 1);
                        drawLineDX2D(bandEdgeOverheadBrush, highPixelX, nVerticalShift + 2, highPixelX, nVerticalShift + 8, 1);
                    }
                    else
                    {
                        if (lowPixelX < 0 && highPixelX > W)
                        {
                            // draw between Low and High
                            drawLineDX2D(bandEdgeOverheadBrush, 0, nVerticalShift + 2, W, nVerticalShift + 2, 1);
                        }
                    }
                }
            }
            //

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

            if (!bIsWaterfall)
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

                switch (label_align)
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
                        else
                        {
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

                    if (_grid_control_minor)
                    {
                        // MW0LGE
                        drawLineDX2D(local_mox ? m_bDX2_tx_hgrid_pen : m_bDX2_hgrid_pen, 0, nVerticalShift + y, W, nVerticalShift + y);
                    }

                    // Draw horizontal line labels
                    if (i != 1) // avoid intersecting vertical and horizontal labels
                    {
                        num = grid_max - i * grid_step;
                        string label = num.ToString();
                        if (label.Length == 3)
                            xOffset = (int)measureStringDX2D("0", fontDX2d_font9).Width;// use 0 here instead of a - sign
                        SizeF size = measureStringDX2D(label, fontDX2d_font9);

                        int x = 0;
                        switch (label_align)
                        {
                            case DisplayLabelAlignment.LEFT:
                                x = xOffset + 3;
                                break;
                            case DisplayLabelAlignment.CENTER:
                                if (rx == 1 && (rx1_dsp_mode == DSPMode.USB || rx1_dsp_mode == DSPMode.DIGU || rx1_dsp_mode == DSPMode.CWU))
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
                    if (_rx2_enabled)
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
                    double freq_low = _mouseFrequency + filter_low;
                    double freq_high = _mouseFrequency + filter_high;
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

                    if (rx == 1)
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

                        unsafe
                        {
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
                    else// rx == 2
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

                                rx2_agc_hang_y += nVerticalShift;

                                if (display_rx2_hang_line && console.RX2AGCMode != AGCMode.MED && console.RX2AGCMode != AGCMode.FAST)
                                {
                                    AGCRX2Hang.Height = 8; AGCRX2Hang.Width = 8; AGCRX2Hang.X = 40;

                                    AGCRX2Hang.Y = (int)rx2_agc_hang_y - AGCRX2Hang.Height;

                                    drawFillRectangleDX2D(m_bDX2_Yellow, AGCRX2Hang);

                                    drawLineDX2D(m_bDX2_Yellow, x3_rx2_hang, rx2_agc_hang_y/* + 2 * H*/, x2_rx2_hang, rx2_agc_hang_y/* + 2 * H*/, m_styleDots);

                                    drawStringDX2D("-H", fontDX2d_panafont, m_bDX2_pana_text_brush, AGCRX2Hang.X + AGCRX2Hang.Width, AGCRX2Hang.Y - (AGCRX2Hang.Height / 2));
                                }
                                rx2_agc = m_bAutoAGCRX2 ? "-Ga" : "-G";
                                break;
                        }

                        if (display_rx2_gain_line)
                        {
                            rx2_agcknee_y_value += nVerticalShift;

                            AGCRX2Knee.Height = 8; AGCRX2Knee.Width = 8; AGCRX2Knee.X = 40;

                            AGCRX2Knee.Y = (int)rx2_agcknee_y_value - AGCRX2Knee.Height;

                            drawFillRectangleDX2D(m_bDX2_YellowGreen, AGCRX2Knee);

                            drawLineDX2D(m_bDX2_YellowGreen, x1_rx2_gain, rx2_agcknee_y_value/* + 2 * H*/, x2_rx2_gain, rx2_agcknee_y_value/* + 2 * H*/, m_styleDots);

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
                int localRit;
                if (rx == 1)
                    localRit = _rx1ClickDisplayCTUN ? 0 : rit_hz;
                else
                    localRit = 0;

                int iii = 0;                          // ke9ns add stairstep holder

                int kk = 0;                           // ke9ns add index for holder[] after you draw the vert line, then draw calls (so calls can overlap the vert lines)

                int vfo_hz = (int)vfoa_hz;    // vfo freq in hz

                int H1a = H / 2;            // length of vertical line (based on rx1 and rx2 display window configuration)
                int H1b = 20;               // starting point of vertical line

                int rxDisplayLow = RXDisplayLow;
                int rxDisplayHigh = RXDisplayHigh;
                SizeF length;

                if (bottom || (current_display_mode_bottom == DisplayMode.PANAFALL && rx == 2))                 // if your drawing to the bottom 
                {
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
                        int VFO_DXPos = (int)((((float)W / (float)VFODiff) * (float)(SpotControl.DX_Freq[ii] + cwSideToneShiftInverted - VFOLow - localRit))); // determine DX spot line pos on current panadapter screen

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

                        int rx2;
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

                        int rx2;
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

            _d2dRenderTarget.PopAxisAlignedClip();

            left_edge = Low;
            right_edge = High;

            return center_line_x;
        }

        private static void DrawCursorInfo(int W)
        {
            if (_spot_highlighted) return; // ignore if highlighting a spot

            //MHzCursor Display
            if ((m_bAlwaysShowCursorInfo || Common.ShiftKeyDown) && display_cursor_x != -1)
            {
                bool bLeftSide = false;
                int width = 0;
                int xPos;

                if (!string.IsNullOrEmpty(m_sMHzCursorDisplay))
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

                if (!string.IsNullOrEmpty(m_sOtherData1CursorDisplay))
                {
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

                if (!string.IsNullOrEmpty(m_sOtherData2CursorDisplay))
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

            int low = 0;
            int high = 0;
            float local_max_y = float.MinValue;
            int grid_max = 0;
            int grid_min = 0;

            if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
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
                if (_mox && (rx1_dsp_mode == DSPMode.CWL || rx1_dsp_mode == DSPMode.CWU))
                {
                    for (int i = 0; i < nDecimatedWidth; i++)
                        current_display_data[i] = grid_min - rx1_display_cal_offset;
                }
                else
                {
                    fixed (void* rptr = &new_display_data[0])
                    fixed (void* wptr = &current_display_data[0])
                        Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));
                }
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                {
                    fixed (void* rptr = &new_display_data_bottom[0])
                    fixed (void* wptr = &current_display_data_bottom[0])
                        Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));
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

            max += rx == 1 ? RX1Offset : RX2Offset;

            if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
            {
                if (rx == 1) max += rx1_preamp_offset - alex_preamp_offset;   //MW0LGE_21 change to rx==1
                else max += rx2_preamp_offset;
            }

            Y = (int)Math.Min((Math.Floor((grid_max - max) * H / yRange)), H);
            previousPoint.X = 0;// + 0.5f; // the 0.5f pushes it into the middle of a 'pixel', so that it is not drawn half in one, and half in the other
            previousPoint.Y = Y;// + 0.5f;

            float fOffset = rx == 1 ? RX1Offset : RX2Offset;

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

                max += fOffset;

                if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
                {
                    if (rx == 1) max += rx1_preamp_offset - alex_preamp_offset;  //MW0LGE_21 change to rx==1
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

            if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
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

            if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
            {
                grid_max = spectrum_grid_max;
                grid_min = spectrum_grid_min;
                grid_step = spectrum_grid_step;
            }
            else if (_mox)
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

            SharpDX.Vector2 pointMin = new SharpDX.Vector2();
            SharpDX.Vector2 pointMax = new SharpDX.Vector2();

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

                pointMax.Y -= 0.5f;
                pointMin.Y -= 0.5f;

                if (bottom)
                {
                    pointMax.Y += H;
                    pointMin.Y += H;
                }

                if (previousPointMax.Y > pointMin.Y) _d2dRenderTarget.DrawLine(previousPointMax, pointMin, m_bDX2_waveform_line_pen, 1f);
                if (previousPointMin.Y < pointMax.Y) _d2dRenderTarget.DrawLine(previousPointMin, pointMax, m_bDX2_waveform_line_pen, 1f);

                if (pointMin == pointMax)
                    _d2dRenderTarget.FillRectangle(new RectangleF(pointMin.X, pointMin.Y, 1f, 1f), m_bDX2_waveform_line_pen);
                else
                    _d2dRenderTarget.DrawLine(pointMin, pointMax, m_bDX2_waveform_line_pen, 1f);

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

            float yScale = (float)H / 4;

            SharpDX.Vector2 point = new SharpDX.Vector2();
            SharpDX.Vector2 previousPoint = new SharpDX.Vector2();

            // the 0.5f's to move to middle pixel
            // draw the left input samples
            previousPoint.X = 0;
            previousPoint.Y = (int)(y1 - (scope2_max[0] * yScale));
            for (int x = 0; x < nDecimatedWidth; x++)
            {
                int i = x;
                int y = (int)(y1 - (scope2_max[i] * yScale));
                point.X = x * m_nDecimation;
                point.Y = y;

                _d2dRenderTarget.DrawLine(previousPoint, point, m_bDX2_waveform_line_pen);

                previousPoint.X = point.X;
                previousPoint.Y = point.Y;
            }

            // draw the right input samples
            previousPoint.X = 0;
            previousPoint.Y = (int)(y3 - (scope_max[0] * yScale));
            for (int x = 0; x < nDecimatedWidth; x++)
            {
                int i = x;
                int y = (int)(y3 - (scope_max[i] * yScale));
                point.X = x * m_nDecimation;
                point.Y = y;

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
                    Win32.memcpy(wptr, rptr, (num_points * 2) * sizeof(float));
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                // get new data
                fixed (void* rptr = &new_display_data_bottom[0])
                fixed (void* wptr = &current_display_data_bottom[0])
                    Win32.memcpy(wptr, rptr, (num_points * 2) * sizeof(float));
                data_ready_bottom = false;
            }

            int nShift = m_nPhasePointSize / 2;

            SharpDX.Vector2 point = new SharpDX.Vector2();

            double sinSum = 0;
            double cosSum = 0;

            for (int i = 0; i < num_points; i++)	// fill point array
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
                    Win32.memcpy(wptr, rptr, (num_points * 2) * sizeof(float));
                data_ready = false;
            }
            else if (bottom && data_ready_bottom)
            {
                // get new data
                fixed (void* rptr = &new_display_data_bottom[0])
                fixed (void* wptr = &current_display_data_bottom[0])
                    Win32.memcpy(wptr, rptr, (num_points * 2) * sizeof(float));
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

        unsafe static private bool DrawHistogramDX2D(int rx, int W, int H)
        {
            DrawSpectrumGridDX2D(W, H, false);

            updateSharePointsArray(W);

            int low = 0;
            int high = 0;
            float local_max_y = Int32.MinValue;
            int grid_max = 0;
            int grid_min = 0;

            if (!_mox || (_mox && _tx_on_vfob && console.RX2Enabled))
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
                    Win32.memcpy(wptr, rptr, nDecimatedWidth * sizeof(float));

                data_ready = false;
            }

            int sum = 0;
            float fOffset = rx == 1 ? RX1Offset : RX2Offset;

            for (int i = 0; i < nDecimatedWidth; i++)
            {
                float max = max = current_display_data[i];

                max += fOffset;

                if (!_mox) max += (rx1_preamp_offset - alex_preamp_offset);

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
        private static int _new_spot_fade = 255;
        private static double _pulsePhase;
        private static double _lastRenderTime;
        public static bool NoiseFloorDecimal
        {
            get { return _NFDecimal; }
            set { _NFDecimal = value; }
        }

        private static List<int> _spotLayerRightRX1 = new List<int>();
        private static List<int> _spotLayerRightRX2 = new List<int>();
        private static readonly Dictionary<int, SharpDX.Direct2D1.Brush> _DX2Brushes = new Dictionary<int, SharpDX.Direct2D1.Brush>(256);

        private static int getSpotLayer(int rx, int leftX)
        {
            if (rx == 1)
            {
                if (_spotLayerRightRX1 == null)
                    _spotLayerRightRX1 = new List<int>();

                for (int layer = 0; layer < _spotLayerRightRX1.Count; layer++)
                {
                    int layerRightmostX = _spotLayerRightRX1[layer];
                    if (leftX > layerRightmostX)
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
                if (layer < _spotLayerRightRX1.Count)
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
                if (!string.IsNullOrEmpty(spot.spotter))
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
        private static bool _spot_highlighted = false;
        public static void drawSpots(int rx, int nVerticalShift, int W, bool bottom)
        {
            if (bottom) return;

            long vfo_hz;
            int rxDisplayLow;
            int rxDisplayHigh;

            bool local_mox = localMox(rx);
            bool duplex = isRxDuplex(rx);
            int local_rit;

            if (rx == 1)
            {
                vfo_hz = vfoa_hz;
                if (local_mox)
                {
                    if (duplex)
                    {
                        rxDisplayLow = RXDisplayLow;
                        rxDisplayHigh = RXDisplayHigh;
                    }
                    else
                    {
                        rxDisplayLow = TXDisplayLow;
                        rxDisplayHigh = TXDisplayHigh;
                    }
                }
                else
                {
                    rxDisplayLow = RXDisplayLow;
                    rxDisplayHigh = RXDisplayHigh;
                }
                _spotLayerRightRX1.Clear();

                local_rit = _rx1ClickDisplayCTUN ? 0 : rit_hz;
            }
            else// rx == 2
            {
                vfo_hz = vfob_hz;
                if (local_mox)
                {
                    if (duplex)
                    {
                        rxDisplayLow = RX2DisplayLow;
                        rxDisplayHigh = RX2DisplayHigh;
                    }
                    else // always false on rx2
                    {
                        rxDisplayLow = TXDisplayLow;
                        rxDisplayHigh = TXDisplayHigh;
                    }
                }
                else
                {
                    rxDisplayLow = RX2DisplayLow;
                    rxDisplayHigh = RX2DisplayHigh;
                }
                _spotLayerRightRX2.Clear();

                local_rit = 0;
            }

            int yTop = nVerticalShift + 20;

            long vfoLow = vfo_hz + rxDisplayLow;    // low freq (left side) in hz
            long vfoHigh = vfo_hz + rxDisplayHigh; // high freq (right side) in hz
            long vfoDiff = vfoHigh - vfoLow;
            float HzToPixel = W / (float)vfoDiff;
            int cwShift = getCWSideToneShift(rx);

            List<SpotManager2.smSpot> sortedSpots = SpotManager2.GetFrequencySortedSpots(); 

            SharpDX.Direct2D1.Brush spotColour;
            string sDisplayString;

            foreach (SpotManager2.smSpot spot in sortedSpots)
            {
                sDisplayString = getCallsignString(spot);

                spot.Size = measureStringDX2D(sDisplayString, fontDX2d_font9);

                int width = (int)spot.Size.Width;
                int height = (int)spot.Size.Height + 2;

                int halfWidth = width / 2;
                float x = (spot.frequencyHZ - vfoLow - cwShift - local_rit) * HzToPixel;

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
            SharpDX.Direct2D1.Brush whiteBrush = getDXBrushForColour(Color.White, 255);
            SharpDX.Direct2D1.Brush blackBrush = getDXBrushForColour(Color.Black, 255);
            SharpDX.Direct2D1.Brush brightBorder = getDXBrushForColour(Color.Yellow, 255);
            //SharpDX.Direct2D1.Brush brightBorder_new_spot = getDXBrushForColour(Color.Yellow, _new_spot_fade);

            // adjust fade
            double currentTime = m_dElapsedFrameStart;
            double deltaMs = currentTime - _lastRenderTime;
            _lastRenderTime = currentTime;
            _pulsePhase = (_pulsePhase + (Math.PI * 2.0) * deltaMs / 1000.0) % (2.0 * Math.PI);
            double normalised = (Math.Cos(_pulsePhase) + 1.0) * 0.5;
            _new_spot_fade = (int)(normalised * 255.0);
            //

            List<SpotManager2.smSpot> visibleSpots = sortedSpots.Where(o => o.Visible[rx - 1]).ToList();
            SpotManager2.smSpot highLightedSpot = null;
            foreach (SpotManager2.smSpot spot in visibleSpots)
            {
                SharpDX.Direct2D1.Brush brightBorder_new_spot = getDXBrushForColour(spot.colour, _new_spot_fade);

                sDisplayString = getCallsignString(spot);

                int nLuminance = Common.GetLuminance(spot.colour);
                spotColour = getDXBrushForColour(spot.colour, 255);
                textBrush = nLuminance <= 128 ? whiteBrush : blackBrush;

                if (spot.Highlight[rx - 1])
                {
                    highLightedSpot = spot;
                    spot.previously_highlighted = true;
                    spot.flashing = false;
                }
                else
                {
                    drawFillRectangleDX2D(spotColour, spot.BoundingBoxInPixels[rx - 1]);
                    drawStringDX2D(sDisplayString, fontDX2d_font9, textBrush, spot.BoundingBoxInPixels[rx - 1].X + 1, spot.BoundingBoxInPixels[rx - 1].Y + 1);

                    if (!_flashNewTCISpots) continue;

                    // now draw a border around any spot that is <= 2 mins
                    TimeSpan age = DateTime.UtcNow - spot.utc_spot_time;
                    if (age.TotalSeconds <= 120) spot.flashing = true;

                    if (spot.flashing && !spot.IsSWL && !spot.previously_highlighted)
                    {
                        Rectangle r = new Rectangle(spot.BoundingBoxInPixels[rx - 1].X - 2, spot.BoundingBoxInPixels[rx - 1].Y - 2,
                            spot.BoundingBoxInPixels[rx - 1].Width + 4, spot.BoundingBoxInPixels[rx - 1].Height + 4);

                        drawRectangleDX2D(brightBorder_new_spot, r, 4);

                        if (age.TotalSeconds > 120 && _new_spot_fade < 20) spot.flashing = false; // stop flashing when dim
                    }
                }
            }

            if (highLightedSpot != null)
            {
                _spot_highlighted = true;
                sDisplayString = getCallsignString(highLightedSpot);

                int nLuminance = Common.GetLuminance(highLightedSpot.colour);
                spotColour = getDXBrushForColour(highLightedSpot.colour, 255);
                textBrush = nLuminance <= 128 ? whiteBrush : blackBrush;

                Rectangle r = new Rectangle(highLightedSpot.BoundingBoxInPixels[rx - 1].X - 2, highLightedSpot.BoundingBoxInPixels[rx - 1].Y - 2,
                                        highLightedSpot.BoundingBoxInPixels[rx - 1].Width + 4, highLightedSpot.BoundingBoxInPixels[rx - 1].Height + 4);

                drawFillRectangleDX2D(spotColour, r);
                drawRectangleDX2D(brightBorder, r, 2);
                drawStringDX2D(sDisplayString, fontDX2d_font9, textBrush, highLightedSpot.BoundingBoxInPixels[rx - 1].X + 1, highLightedSpot.BoundingBoxInPixels[rx - 1].Y + 1);

                // show additional text in bubble below, and concat parts if available
                string bubble_text = highLightedSpot.additionalText;

                if (!string.IsNullOrEmpty(highLightedSpot.spotter)) bubble_text += "\nSpotter: " + highLightedSpot.spotter;
                if (highLightedSpot.heading >= 0) bubble_text += "\nHeading: " + highLightedSpot.heading.ToString();
                if (!string.IsNullOrEmpty(highLightedSpot.continent)) bubble_text += "\nContinent: " + highLightedSpot.continent;
                if (!string.IsNullOrEmpty(highLightedSpot.country)) bubble_text += "\nCountry: " + highLightedSpot.country;
                // age
                TimeSpan age = DateTime.UtcNow - highLightedSpot.utc_spot_time;
                string ageText;
                if (age.TotalDays > 2)
                {
                    ageText = "Old spot (>2 days)";
                }
                else if (age.TotalDays >= 1)
                {
                    int days = (int)age.TotalDays;
                    ageText = days + " day" + (days == 1 ? "" : "s");
                }
                else if (age.TotalHours >= 1)
                {
                    int hours = (int)age.TotalHours;
                    ageText = hours + " hour" + (hours == 1 ? "" : "s");
                }
                else if (age.TotalMinutes >= 1)
                {
                    int minutes = (int)age.TotalMinutes;
                    ageText = minutes + " minute" + (minutes == 1 ? "" : "s");
                }
                else
                {
                    int seconds = (int)age.TotalSeconds;
                    ageText = seconds + " second" + (seconds == 1 ? "" : "s");
                }
                bubble_text += "\nAge: " + ageText;
                //

                bubble_text = bubble_text.Trim();

                SizeF additionalTextSize = measureStringDX2D(bubble_text, fontDX2d_font12);

                int left = (r.X + (r.Width / 2)) - (int)(additionalTextSize.Width / 2);
                int top = r.Y + r.Height + 6;

                int rectWidth = (int)(additionalTextSize.Width + 8);
                int rectHeight = (int)(additionalTextSize.Height + 8);
                int adjustedLeft = left - 4;
                if (adjustedLeft < 0) adjustedLeft = 0;
                else if (adjustedLeft + rectWidth > W) adjustedLeft = W - rectWidth;

                Rectangle additionalTextRect = new Rectangle(
                    adjustedLeft,
                    top - 4,
                    rectWidth,
                    rectHeight
                );

                RoundedRectangle rr = new RoundedRectangle();
                rr.Rect = new RectangleF(additionalTextRect.Left, additionalTextRect.Top, additionalTextRect.Width, additionalTextRect.Height);
                rr.RadiusX = 8f;
                rr.RadiusY = 8f;

                _d2dRenderTarget.FillRoundedRectangle(rr, getDXBrushForColour(Color.LightGray));
                _d2dRenderTarget.DrawRoundedRectangle(rr, getDXBrushForColour(Color.White), 2f);

                drawStringDX2D(bubble_text, fontDX2d_font12, getDXBrushForColour(Color.Black), additionalTextRect.X + 2, additionalTextRect.Y + 2);
            }
            else
            {
                _spot_highlighted = false;
            }
        }
        private static void clearAllDynamicBrushes()
        {
            if (!_bDX2Setup || _DX2Brushes == null) return;

            foreach (KeyValuePair<int, SharpDX.Direct2D1.Brush> kvp in _DX2Brushes)
            {
                SharpDX.Direct2D1.Brush b = kvp.Value;
                Utilities.Dispose(ref b);
                b = null;
            }

            _DX2Brushes.Clear();
        }
        public static int CachedDXBrushes
        {
            get { return _DX2Brushes == null ? 0 : _DX2Brushes.Count; }
        }
        private static SharpDX.Direct2D1.Brush getDXBrushForColour(Color c, int replaceAlpha = -1)
        {
            if (!_bDX2Setup) return null;

            int alpha = (replaceAlpha >= 0 && replaceAlpha <= 255) ? replaceAlpha : c.A;
            int key = (alpha << 24) | (c.R << 16) | (c.G << 8) | c.B; //Color.FromArgb(alpha, c.R, c.G, c.B).ToArgb();

            SharpDX.Direct2D1.Brush existingBrush;
            if (_DX2Brushes.TryGetValue(key, out existingBrush))
            {
                return existingBrush;
            }

            SharpDX.Mathematics.Interop.RawColor4 rawColor = new SharpDX.Mathematics.Interop.RawColor4(
                (float)c.R / 255.0f,
                (float)c.G / 255.0f,
                (float)c.B / 255.0f,
                (float)alpha / 255.0f
            );

            SolidColorBrush newBrush = new SharpDX.Direct2D1.SolidColorBrush(
                _d2dRenderTarget,
                rawColor
            );

            _DX2Brushes.Add(key, newBrush);
            return newBrush;
        }

        private static bool _showTCISpots = false;
        public static bool ShowTCISpots
        {
            get { return _showTCISpots; }
            set { _showTCISpots = value; }
        }
        private static bool _flashNewTCISpots = false;
        public static bool FlashNewTCISpots
        {
            get { return _flashNewTCISpots; }
            set { _flashNewTCISpots= value; }
        }

        private static bool _bUseLegacyBuffers = false;
        public static bool UseLegacyBuffers
        {
            get { return _bUseLegacyBuffers; }
            set { _bUseLegacyBuffers = value; }
        }

        public static void PurgeBuffers()
        {
            lock (_objDX2Lock)
            { 
                clearBuffers(displayTargetWidth, 1);
                if (_rx2_enabled) clearBuffers(displayTargetWidth, 2);
            }
        }

        private static float _fft_fill_timeRX1 = 0f;
        private static float _fft_fill_timeRX2 = 0f;
        public static float RX1FFTFillTime
        {
            get { return _fft_fill_timeRX1; }
            set { _fft_fill_timeRX1 = value; }
        }
        public static float RX2FFTFillTime
        {
            get { return _fft_fill_timeRX2; }
            set { _fft_fill_timeRX2 = value; }
        }
        private static bool _wdsp_mox_transition_buffer_clear = false;
        public static bool WDSPMOXTransitionBufferClear
        {
            get { return _wdsp_mox_transition_buffer_clear; }
            set { _wdsp_mox_transition_buffer_clear = value; }
        }
#endregion

#if SNOWFALL
        private class SnowFlake
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float FallSpeed { get; set; }
            public float Alpha { get; set; }
            public float XShift { get; set; }
            public bool Settled {  get; set; }
            public float Size { get; set; }
            public bool Finished { get; set; }

            public SnowFlake(int width)
            {
                X = _rnd.Next(width);
                Y = 0;
                FallSpeed = _rnd.NextFloat(0.1f, 1.5f);
                Alpha = _rnd.Next(64, 256);
                XShift = _rnd.NextFloat(-0.5f, 0.5f);
                Settled = false;
                Size = _rnd.NextFloat(1f, 2f);
                Finished = false;
            }

            public void Update()
            {
                if (!Settled)
                {

                    Y += FallSpeed;
                    X += XShift;

                    int dirRand = _rnd.Next(0, 10);
                    if (dirRand == 9 && XShift < 0.5f)
                        XShift += 0.1f;
                    else if (dirRand == 0 && XShift > -0.5f)
                        XShift -= 0.1f;

                    if (Y >= Target.Height - 1)
                    {
                        Y = Target.Height - 1;
                        Settled = true;
                    }

                    if (X > Target.Width - 1)
                        X = 0;
                    else if (X < 0)
                        X = Target.Width - 1;
                }
                else
                {
                    Alpha -= 0.5f;
                    if (Alpha <= 0)
                    {
                        Alpha = 0;
                        Finished = true;
                    }
                }
            }
        }

        private static bool _snowFall = false;
        public static bool SnowFall
        {
            get { return _snowFall; }
            set 
            { 
                _snowFall = value;
                if (!_snowFall)
                {
                    lock (_snowLock)
                    {
                        _snow.Clear();
                        _showSanta = false;
                    }
                }
            }
        }
        private static readonly object _snowLock = new object();
        private static Random _rnd = new Random();
        private static List<SnowFlake> _snow = new List<SnowFlake>();
        private static double _oldSnowFrame = 0;
        private static double _oldSantaFrame = 0;
        private static double _oldSantaXFrame = 0;
        private static SharpDX.Direct2D1.Bitmap[] _santaFrames;
        private static int _santaFrameIndex;
        private static float _santaX;
        private static bool _showSanta;
        private static DateTime _whenToShowSanta;
        private static void letItSnow()
        {
            bool bUpdate = false;
            if (m_dElapsedFrameStart >= _oldSnowFrame + 16)
            {
                //fixed update
                _oldSnowFrame = m_dElapsedFrameStart;
                bUpdate = true;
            }

            lock (_snowLock)
            {
                if (bUpdate)
                {
                    if (_snow.Count < 500)
                    {
                        SnowFlake sf = new SnowFlake(Target.Width);
                        _snow.Add(sf);
                    }

                    foreach (SnowFlake snowflake in _snow)
                        snowflake.Update();

                    _snow.RemoveAll(s => s.Finished);
                }

                Ellipse e = new Ellipse(new SharpDX.Vector2(0, 0), 1, 1);
                foreach (SnowFlake snowflake in _snow)
                {
                    e.Point.X = snowflake.X;
                    e.Point.Y = snowflake.Y;
                    e.RadiusX = snowflake.Size;
                    e.RadiusY = snowflake.Size;
                    _d2dRenderTarget.FillEllipse(e, getDXBrushForColour(Color.White, (int)snowflake.Alpha));
                }
            }

            plotSanta();
        }
        private static void plotSanta()
        {
            if (!_showSanta)
            {
                DateTime now = DateTime.Now;
                if (now > _whenToShowSanta)
                {                    
                    _showSanta = now >= new DateTime(now.Year, 12, 1) && now.Date <= new DateTime(now.Year, 12, 25);
                    _whenToShowSanta = now.AddSeconds(_rnd.Next(120, 600));
                }

                return;
            }

            if (_santaFrames.Length <= 0) return;

            bool bUpdateFrame = false;
            if (m_dElapsedFrameStart >= _oldSantaFrame + 250)
            {
                //fixed update
                _oldSantaFrame = m_dElapsedFrameStart;
                bUpdateFrame = true;
            }

            SharpDX.Direct2D1.Bitmap santa = _santaFrames[_santaFrameIndex];

            float y = displayTargetHeight - santa.Size.Height / 2;
            RectangleF rectDest = new RectangleF(_santaX, y, santa.Size.Width / 2, santa.Size.Height / 2);
            _d2dRenderTarget.DrawBitmap(santa, rectDest, 1f, BitmapInterpolationMode.Linear);

            // xpos shift
            if (m_dElapsedFrameStart >= _oldSantaXFrame + 16)
            {
                if (_santaFrameIndex <= 2) // dig frames 0 and 1
                {
                    lock (_snowLock)
                    {
                        float x = rectDest.X + rectDest.Width / 2;

                        _snow
                            .Where(snowflake => snowflake.Settled && snowflake.X >= x && snowflake.X <= rectDest.X + rectDest.Width)
                            .ToList()
                            .ForEach(snowflake => snowflake.Alpha = 0);
                    }
                }

                //fixed update
                _oldSantaXFrame = m_dElapsedFrameStart;

                _santaX += 0.5f;
                if (_santaX > displayTargetWidth)
                {
                    _santaX = -50;
                    _showSanta = false;
                }
            }

            if (bUpdateFrame)
            {
                _santaFrameIndex++;
                if (_santaFrameIndex >= _santaFrames.Length) _santaFrameIndex = 0;
            }
        }
        public static void SetSantaGif(System.Drawing.Image image)
        {
            lock (_objDX2Lock)
            {
                if (!_bDX2Setup) return;
                if (image== null) return;
                if (image.RawFormat == null || image.RawFormat.Guid != ImageFormat.Gif.Guid) return; // image not a gif
                if (image.FrameDimensionsList == null || image.FrameDimensionsList.Length <= 0) return;

                _santaFrameIndex = 0;
                _santaX = -50;
                _showSanta = false;
                _whenToShowSanta = DateTime.Now.AddSeconds(_rnd.Next(120, 600));

                FrameDimension dimension = new FrameDimension(image.FrameDimensionsList[0]);
                int totalFrames = image.GetFrameCount(dimension);

                // remove any existing santa frames
                santaCleanUp();

                _santaFrames = new SharpDX.Direct2D1.Bitmap[totalFrames];

                for (int i = 0; i < totalFrames; i++)
                {
                    image.SelectActiveFrame(dimension, i);

                    using (Bitmap graphicsImage = new Bitmap(image))
                    {                        
                        _santaFrames[i] = SDXBitmapFromSysBitmap(_d2dRenderTarget, graphicsImage);
                    }
                }
            }
        }
        private static void santaCleanUp()
        {
            if (_santaFrames != null)
            {
                for (int i = 0; i < _santaFrames.Length; i++)
                {
                    if (_santaFrames[i] != null)
                    {
                        Utilities.Dispose(ref _santaFrames[i]);
                        _santaFrames[i] = null;
                    }
                }

                _santaFrames = null;
            }
        }
#endif

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public ushort dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;
            public uint dmDisplayFlags;
            public uint dmDisplayFrequency;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        public static int GetCurrentMonitorRefreshRate(Form form)
        {
            // get the refresh rate of the monitor that the form is on
            Screen screen = Screen.FromControl(form);
            DEVMODE devMode = new DEVMODE();
            devMode.dmDeviceName = new string(new char[32]);

            if (EnumDisplaySettings(screen.DeviceName, -1, ref devMode))
            {
                return (int)devMode.dmDisplayFrequency;
            }
            return 60;
        }
    }
}