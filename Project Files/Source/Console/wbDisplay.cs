//=================================================================
// pandisplay.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
// Copyright (C) 2010-2016 Doug Wigley (W5WC)
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
//
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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security;
using System.Linq;

namespace Thetis
{
    public class wbDisplay : PictureBox
    {
        #region Variable Declaration

        public const float CLEAR_FLAG = -999.999F;				// for resetting buffers
        public const int BUFFER_SIZE = 4096;

        //private  Mutex background_image_mutex;			// used to lock the base display image
        //private  Bitmap background_bmp;					// saved background picture for display
        //private  Bitmap display_bmp;					// Bitmap for use when drawing
        public string background_image = null;

        //  private int[] histogram_data = null;					// histogram display buffer
        // private int[] histogram_history;					// histogram counter

        public float[] new_display_data;					// Buffer used to store the new data from the DSP for the display
        public float[] current_display_data;				// Buffer used to store the current data for the display
        // public float[] new_display_data_bottom;
        //  public float[] current_display_data;

        // public float[] new_waterfall_data;
        //  public float[] current_waterfall_data;
        // public float[] waterfall_display_data;
        //  public float[] average_waterfall_buffer;

        //  public float[] new_scope_data;
        //  public float[] current_scope_data;

        //public float[] rx1_average_buffer;					// Averaged display data buffer
        //public float[] rx2_average_buffer;
        //public float[] rx1_peak_buffer;						// Peak hold display data buffer
        //public float[] rx2_peak_buffer;

        // public Mutex render_mutex = new Mutex();
        private Point[] points = null;

        //waterfall
        //private int waterfall_counter;
        //private Bitmap waterfall_bmp = null;					// saved waterfall picture for display
        //  private Bitmap waterfall_bmp2 = null;
        // private Bitmap panadapter_bmp;

        //private float[] waterfall_data;

        private Task draw_display_task;					// draws the main display 
        private Bitmap wbDisplay_buffer;
        public bool pauseDisplayThread = false;
        #endregion

        public wbDisplay()
        {
            this.MouseEnter += new System.EventHandler(this.PanDisplay_MouseEnter);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.PanDisplay_MouseWheel);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanDisplay_MouseMove);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanDisplay_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PanDisplay_MouseUp);
        }

        #region Properties

        private string update_rate = "15";
        public string UpdateRate
        {
            get { return update_rate; }
            set
            {
                update_rate = value;
                int rate = 1000 / Int32.Parse(value);
                NetworkIO.SetWBUpdateRate(rate);
            }
        }

        private string frame_size = "32";
        public string FrameSize
        {
            get { return frame_size; }
            set
            {
                frame_size = value;
                int size = Int32.Parse(value);
                NetworkIO.SetWBPacketsPerFrame(size);
            }
        }

        public float FrameDelta { get; private set; }

        private FRSRegion current_region = Thetis.FRSRegion.US;
        public FRSRegion CurrentRegion
        {
            get { return current_region; }
            set { current_region = value; }
        }

        private float freq_ruler_position = 1.0f;
        public float FreqRulerPosition
        {
            get { return freq_ruler_position; }
            set
            {
                freq_ruler_position = value;
                CreateDisplayRegions();
            }
        }

        private int nreceivers = 2;
        public int NReceivers
        {
            get { return nreceivers; }
            set { nreceivers = value; }
        }

        private Console console;
        public Console cOnsole
        {
            get { return console; }
            set { console = value; }
        }

        private bool refresh_panadapter_grid = true;                 // yt7pwr
        public bool RefreshPanadapterGrid
        {
            set { refresh_panadapter_grid = value; }
        }

        private Rectangle agcKnee;
        public Rectangle AGCKnee
        {
            get { return agcKnee; }
            set { agcKnee = value; }
        }

        private Rectangle agcHang;
        public Rectangle AGCHang
        {
            get { return agcHang; }
            set { agcHang = value; }
        }

        private Rectangle filterRect;
        public Rectangle FilterRect
        {
            get { return filterRect; }
            set { filterRect = value; }
        }

        private int filterLeft;
        private int filterRight;
        private int filterTop;
        private int filterBottom;

        //Color notch_on_color = Color.DarkGreen;
        //Color notch_highlight_color = Color.Chartreuse;
        //Color notch_perm_on_color = Color.DarkRed;
        //Color notch_perm_highlight_color = Color.DeepPink;
        private Color notch_on_color = Color.Olive;
        private Color notch_on_color_zoomed = Color.FromArgb(190, 128, 128, 0);
        private Color notch_highlight_color = Color.YellowGreen;
        private Color notch_highlight_color_zoomed = Color.FromArgb(190, 154, 205, 50);
        private Color notch_perm_on_color = Color.DarkGreen;
        private Color notch_perm_highlight_color = Color.Chartreuse;
        private Color notch_off_color = Color.Gray;

        private Color channel_background_on = Color.FromArgb(150, Color.DodgerBlue);
        private Color channel_background_off = Color.FromArgb(100, Color.RoyalBlue);
        private Color channel_foreground = Color.Cyan;

        private ColorScheme color_scheme = ColorScheme.enhanced;
        public ColorScheme ColorScheme
        {
            get { return color_scheme; }

            set { color_scheme = value; }
        }

        private bool reverse_waterfall = false;
        public bool ReverseWaterfall
        {
            get { return reverse_waterfall; }
            set { reverse_waterfall = value; }
        }

        private bool pan_fill = true;
        public bool PanFill
        {
            get { return pan_fill; }
            set { pan_fill = value; }
        }

        private Color pan_fill_color = Color.FromArgb(100, 0, 0, 127);
        public Color PanFillColor
        {
            get { return pan_fill_color; }
            set { pan_fill_color = value; }
        }

        private bool display_duplex = false;
        public bool DisplayDuplex
        {
            get { return display_duplex; }
            set { display_duplex = value; }
        }

        private bool split_display = false;
        public bool SplitDisplay
        {
            get { return split_display; }
            set
            {
                split_display = value;
                refresh_panadapter_grid = true;
            }
        }

        private int rx_filter_low;
        public int RXFilterLow
        {
            get { return rx_filter_low; }
            set { rx_filter_low = value; }
        }

        private int rx_filter_high;
        public int RXFilterHigh
        {
            get { return rx_filter_high; }
            set { rx_filter_high = value; }
        }

        private bool sub_rx1_enabled = false;
        public bool SubRX1Enabled
        {
            get { return sub_rx1_enabled; }
            set
            {
                sub_rx1_enabled = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private bool split_enabled = false;
        public bool SplitEnabled
        {
            get { return split_enabled; }
            set
            {
                split_enabled = value;

            }
        }

        private bool show_freq_offset = false;
        public bool ShowFreqOffset
        {
            get { return show_freq_offset; }
            set
            {
                show_freq_offset = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private double freq;
        public double FREQ
        {
            get { return freq; }
            set
            {
                freq = value;
            }
        }

        private long _vfo_hz = 10000000;
        public long VFOHz
        {
            get { return _vfo_hz; }
            set
            {
                _vfo_hz = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private long vfoa_sub_hz;
        public long VFOASub //multi-rx freq
        {
            get { return vfoa_sub_hz; }
            set
            {
                vfoa_sub_hz = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private int rit_hz;
        public int RIT
        {
            get { return rit_hz; }
            set
            {
                rit_hz = value;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private int freq_diff = 0;
        public int FreqDiff
        {
            get { return freq_diff; }
            set
            {
                freq_diff = value;
            }
        }

        private int cw_pitch = 600;
        public int CWPitch
        {
            get { return cw_pitch; }
            set { cw_pitch = value; }
        }

        public bool specready = false;

        //  private static int H = 0;	// target height
        //  private static int W = 0;	// target width

        private Control target = null;
        public Control Target
        {
            get { return target; }
            set
            {
                target = value;
                // H = target.Height;
                //int W = target.Width;
                //Audio.ScopeDisplayWidth = W;
                //if (specready)
                //{
                //    // if (rx_dsp_mode == DSPMode.DRM)
                //    //   console.specRX.GetSpecRX(0).Pixels = 4096;
                //    // else
                //    //console.specRX.GetSpecRX(0).Pixels = W;
                //    //console.specRX.GetSpecRX(1).Pixels = W;
                //}
            }
        }

        //private int rx_display_low =  -4000;//-23197;
        //public int RXDisplayLow
        //{
        //    get { return rx_display_low; }
        //    set { rx_display_low = value; }
        //}

        //private int rx_display_high =  4000;//23185;
        //public int RXDisplayHigh
        //{
        //    get { return rx_display_high; }
        //    set { rx_display_high = value; }
        //}

        private float alex_preamp_offset = 0.0f;
        public float AlexPreampOffset
        {
            get { return alex_preamp_offset; }
            set { alex_preamp_offset = value; }
        }

        private float preamp_offset = 0.0f;
        public float PreampOffset
        {
            get { return preamp_offset; }
            set { preamp_offset = value; }
        }

        private float rx_display_cal_offset = -40.1f;					// display calibration offset in dB
        public float RXDisplayCalOffset
        {
            get { return rx_display_cal_offset; }
            set
            {
                rx_display_cal_offset = value;
            }
        }

        private float rx_fft_size_offset;					// display calibration offset in dB
        public float RXFFTSizeOffset
        {
            get { return rx_fft_size_offset; }
            set
            {
                rx_fft_size_offset = value;
            }
        }

        private HPSDRModel current_model = HPSDRModel.HERMES;
        public HPSDRModel CurrentModel
        {
            get { return current_model; }
            set { current_model = value; }
        }

        private int display_cursor_x;						// x-coord of the cursor when over the display
        public int DisplayCursorX
        {
            get { return display_cursor_x; }
            set { display_cursor_x = value; }
        }

        private int display_cursor_y;						// y-coord of the cursor when over the display
        public int DisplayCursorY
        {
            get { return display_cursor_y; }
            set { display_cursor_y = value; }
        }

        private bool grid_control = true;
        public bool GridControl
        {
            get { return grid_control; }
            set { grid_control = value; }
        }

        private bool show_agc = true;
        public bool ShowAGC
        {
            get { return show_agc; }
            set { show_agc = value; }
        }

        private bool spectrum_line = true;
        public bool SpectrumLine
        {
            get { return spectrum_line; }
            set { spectrum_line = value; }
        }

        private bool display_agc_hang_line = true;
        public bool DisplayAGCHangLine
        {
            get { return display_agc_hang_line; }
            set { display_agc_hang_line = value; }
        }

        private bool rx1_hang_spectrum_line = true;
        public bool RX1HangSpectrumLine
        {
            get { return rx1_hang_spectrum_line; }
            set { rx1_hang_spectrum_line = value; }
        }

        private ClickTuneMode current_click_tune_mode = ClickTuneMode.Off;
        public ClickTuneMode CurrentClickTuneMode
        {
            get { return current_click_tune_mode; }
            set { current_click_tune_mode = value; }
        }

        //private int sample_rate = 48000;
        //public int SampleRate
        //{
        //    get { return sample_rate; }
        //    set { sample_rate = value; }
        //}

        private bool high_swr = false;
        public bool HighSWR
        {
            get { return high_swr; }
            set { high_swr = value; }
        }

        private bool mox = false;
        public bool MOX
        {
            get { return mox; }
            set { mox = value; }
        }

        private DSPMode rx_dsp_mode = DSPMode.USB;
        public DSPMode RXDSPMode
        {
            get { return rx_dsp_mode; }
            set { rx_dsp_mode = value; }
        }

        private DisplayMode current_display_mode = DisplayMode.PANADAPTER;
        public DisplayMode CurrentDisplayMode
        {
            get { return current_display_mode; }
            set
            {
                //PrepareDisplayVars(value);

                current_display_mode = value;

                //if (console.PowerOn)
                //{
                //    //console.pause_DisplayThread = true;
                //   // console.pause_DisplayThread2 = true;
                //}

                /*switch(current_display_mode)
                {
                    case DisplayMode.PANADAPTER:
                    case DisplayMode.WATERFALL:
                        DttSP.SetPWSmode(0, 0, 1);
                        DttSP.NotPan = false;
                        break;
                    default:
                        DttSP.SetPWSmode(0, 0, 0);
                        DttSP.NotPan = true;
                        break;
                }*/

                //switch (current_display_mode)
                //{
                //    case DisplayMode.PHASE2:
                //        Audio.phase = true;
                //        break;
                //    case DisplayMode.SCOPE:
                //    case DisplayMode.SCOPE2:
                //    case DisplayMode.PANASCOPE:
                //    case DisplayMode.SPECTRASCOPE:
                //        Audio.scope = true;
                //        break;
                //    default:
                //        Audio.phase = false;
                //        Audio.scope = false;
                //        break;
                //}
                //if (average_on) ResetRX1DisplayAverage();
                //if (peak_on) ResetRX1DisplayPeak();

                // if (draw_display_thread != null)
                //   draw_display_thread.Abort();
                //  DirectXRelease();
                //  Target = picDisplay;
                // WaterfallTarget = picWaterfall;
                // DirectXInit();

                // if (PowerOn)
                // {
                //   Thread.Sleep(100);
                //   draw_display_thread = new Thread(new ThreadStart(RunDisplay_DirectX));
                //   draw_display_thread.Name = "Draw Display Thread";
                //  draw_display_thread.Priority = ThreadPriority.Normal;
                //  draw_display_thread.IsBackground = true;
                // draw_display_thread.Start();
                //  }
                refresh_panadapter_grid = true;
                // console.pause_DisplayThread = false;
                // console.pause_DisplayThread2 = false;
            }
        }

        private float max_x;								// x-coord of maxmimum over one display pass
        public float MaxX
        {
            get { return max_x; }
            set { max_x = value; }
        }

        private float max_y;								// y-coord of maxmimum over one display pass
        public float MaxY
        {
            get { return max_y; }
            set { max_y = value; }
        }

        //private bool average_on;							// True if the Average button is pressed
        //public bool AverageOn
        //{
        //    get { return average_on; }
        //    set
        //    {
        //        average_on = value;
        //        if (!average_on) ResetRX1DisplayAverage();
        //    }
        //}

        //private bool peak_on;							// True if the Peak button is pressed
        //public bool PeakOn
        //{
        //    get { return peak_on; }
        //    set
        //    {
        //        peak_on = value;
        //        if (!peak_on) ResetRX1DisplayPeak();
        //    }
        //}

        private bool data_ready;					// True when there is new display data ready from the DSP
        public bool DataReady
        {
            get { return data_ready; }
            set { data_ready = value; }
        }

        private bool waterfall_data_ready;
        public bool WaterfallDataReady
        {
            get { return waterfall_data_ready; }
            set { waterfall_data_ready = value; }
        }

        public float display_avg_mult_old = 1 - (float)1 / 5;
        public float display_avg_mult_new = (float)1 / 5;
        private int display_avg_num_blocks = 5;
        public int DisplayAvgBlocks
        {
            get { return display_avg_num_blocks; }
            set
            {
                display_avg_num_blocks = value;
                display_avg_mult_old = 1 - (float)1 / display_avg_num_blocks;
                display_avg_mult_new = (float)1 / display_avg_num_blocks;
            }
        }

        public float waterfall_avg_mult_old = 1 - (float)1 / 18;
        public float waterfall_avg_mult_new = (float)1 / 18;
        private int waterfall_avg_num_blocks = 18;
        public int WaterfallAvgBlocks
        {
            get { return waterfall_avg_num_blocks; }
            set
            {
                waterfall_avg_num_blocks = value;
                waterfall_avg_mult_old = 1 - (float)1 / waterfall_avg_num_blocks;
                waterfall_avg_mult_new = (float)1 / waterfall_avg_num_blocks;
            }
        }

        private int spectrum_grid_max = -50;
        public int SpectrumGridMax
        {
            get { return spectrum_grid_max; }
            set
            {
                spectrum_grid_max = value;
                // Thread.Sleep(100);
                refresh_panadapter_grid = true;
            }
        }

        private int spectrum_grid_min = -170;
        public int SpectrumGridMin
        {
            get { return spectrum_grid_min; }
            set
            {
                spectrum_grid_min = value;
                // Thread.Sleep(100);
                refresh_panadapter_grid = true;
            }
        }

        private int spectrum_grid_step = 10;
        public int SpectrumGridStep
        {
            get { return spectrum_grid_step; }
            set
            {
                spectrum_grid_step = value;
                refresh_panadapter_grid = true;
            }
        }

        private static Color band_edge_color = Color.Red;
        private Pen band_edge_pen = new Pen(band_edge_color);
        public Color BandEdgeColor
        {
            get { return band_edge_color; }
            set
            {
                band_edge_color = value;
                band_edge_pen.Color = band_edge_color;
                if (current_display_mode == DisplayMode.PANADAPTER)
                    refresh_panadapter_grid = true;
            }
        }

        private static Color sub_rx_zero_line_color = Color.LightSkyBlue;
        private Pen sub_rx_zero_line_pen = new Pen(sub_rx_zero_line_color);
        public Color SubRXZeroLine
        {
            get { return sub_rx_zero_line_color; }
            set
            {
                sub_rx_zero_line_color = value;
                sub_rx_zero_line_pen.Color = sub_rx_zero_line_color;
                if (current_display_mode == DisplayMode.PANADAPTER && sub_rx1_enabled)
                    refresh_panadapter_grid = true;
            }
        }

        private static Color sub_rx_filter_color = Color.Blue;
        private SolidBrush sub_rx_filter_brush = new SolidBrush(sub_rx_filter_color);
        public Color SubRXFilterColor
        {
            get { return sub_rx_filter_color; }
            set
            {
                sub_rx_filter_color = value;
                sub_rx_filter_brush.Color = sub_rx_filter_color;
                if (current_display_mode == DisplayMode.PANADAPTER && sub_rx1_enabled)
                    refresh_panadapter_grid = true;
            }
        }

        private static Color grid_text_color = Color.Yellow;
        private SolidBrush grid_text_brush = new SolidBrush(grid_text_color);
        private Pen grid_text_pen = new Pen(grid_text_color);
        public Color GridTextColor
        {
            get { return grid_text_color; }
            set
            {
                grid_text_color = value;
                grid_text_brush.Color = grid_text_color;
                grid_text_pen.Color = grid_text_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color grid_zero_color = Color.Red;
        private Pen grid_zero_pen = new Pen(grid_zero_color);
        public Color GridZeroColor
        {
            get { return grid_zero_color; }
            set
            {
                grid_zero_color = value;
                grid_zero_pen.Color = grid_zero_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color grid_color = Color.FromArgb(65, 255, 255, 255);
        private Pen grid_pen = new Pen(grid_color);
        public Color GridColor
        {
            get { return grid_color; }
            set
            {
                grid_color = value;
                grid_pen.Color = grid_color;
                refresh_panadapter_grid = true;
            }
        }

        private static Color hgrid_color = Color.White;
        private Pen hgrid_pen = new Pen(hgrid_color);
        public Color HGridColor
        {
            get { return hgrid_color; }
            set
            {
                hgrid_color = value;
                hgrid_pen = new Pen(hgrid_color);
                refresh_panadapter_grid = true;
            }
        }

        private static Color data_line_color = Color.White;
        private Pen data_line_pen = new Pen(new SolidBrush(data_line_color), 1.0F);
        private Pen data_line_fpen = new Pen(Color.FromArgb(100, data_line_color));
        public Color DataLineColor
        {
            get { return data_line_color; }
            set
            {
                data_line_color = value;
                data_line_pen.Color = data_line_color;
                data_line_fpen.Color = Color.FromArgb(100, data_line_color);
                refresh_panadapter_grid = true;
            }
        }

        private static Color grid_pen_dark = Color.FromArgb(65, 255, 255, 255);
        private Pen grid_pen_inb = new Pen(grid_pen_dark);
        public Color GridPenDark
        {
            get { return grid_pen_dark; }
            set
            {
                grid_pen_dark = value;
                grid_pen_inb.Color = grid_pen_dark;
                refresh_panadapter_grid = true;
            }
        }

        private static Color display_filter_color = Color.FromArgb(65, 255, 255, 255);
        private SolidBrush display_filter_brush = new SolidBrush(display_filter_color);
        private Pen cw_zero_pen = new Pen(Color.FromArgb(255, display_filter_color));
        public Color DisplayFilterColor
        {
            get { return display_filter_color; }
            set
            {
                display_filter_color = value;
                display_filter_brush.Color = display_filter_color;
                cw_zero_pen.Color = Color.FromArgb(255, display_filter_color);
                refresh_panadapter_grid = true;
            }
        }

        private static Color display_background_color = Color.Black;
        private SolidBrush display_background_brush = new SolidBrush(display_background_color);
        public Color DisplayBackgroundColor
        {
            get { return display_background_color; }
            set
            {
                display_background_color = value;
                display_background_brush.Color = display_background_color;
                refresh_panadapter_grid = true;
            }
        }

        private bool show_cwzero_line = false;
        public bool ShowCWZeroLine
        {
            get { return show_cwzero_line; }
            set
            {
                show_cwzero_line = value;
            }
        }

        private Color waterfall_low_color = Color.Black;
        public Color WaterfallLowColor
        {
            get { return waterfall_low_color; }
            set { waterfall_low_color = value; }
        }

        private Color waterfall_mid_color = Color.Red;
        public Color WaterfallMidColor
        {
            get { return waterfall_mid_color; }
            set { waterfall_mid_color = value; }
        }

        private Color waterfall_high_color = Color.Yellow;
        public Color WaterfallHighColor
        {
            get { return waterfall_high_color; }
            set { waterfall_high_color = value; }
        }

        private float waterfall_high_threshold = -80.0F;
        public float WaterfallHighThreshold
        {
            get { return waterfall_high_threshold; }
            set { waterfall_high_threshold = value; }
        }

        private float waterfall_low_threshold = -130.0F;
        public float WaterfallLowThreshold
        {
            get { return waterfall_low_threshold; }
            set { waterfall_low_threshold = value; }
        }

        private float display_line_width = 1.0F;
        public float DisplayLineWidth
        {
            get { return display_line_width; }
            set
            {
                display_line_width = value;
                data_line_pen.Width = display_line_width;
            }
        }

        private DisplayLabelAlignment display_label_align = DisplayLabelAlignment.LEFT;
        public DisplayLabelAlignment DisplayLabelAlign
        {
            get { return display_label_align; }
            set
            {
                display_label_align = value;
                refresh_panadapter_grid = true;
            }
        }

        private bool click_tune_filter = true;
        public bool ClickTuneFilter
        {
            get { return click_tune_filter; }
            set { click_tune_filter = value; }
        }

        private bool show_cth_line = false;
        public bool ShowCTHLine
        {
            get { return show_cth_line; }
            set { show_cth_line = value; }
        }

        //private double f_center = _vfo_hz;
        //public double F_Center
        //{
        //    get { return f_center; }
        //    set
        //    {
        //        // if(current_click_tune_mode == ClickTuneMode.VFOA)
        //        f_center = value;
        //        // else
        //        //    f_center = vfoa_hz;

        //    }
        //}

        private int top_size = 0;
        public int TopSize
        {
            get { return top_size; }
            set { top_size = value; }
        }

        private int _lin_corr = 2;
        public int LinCor
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

        private int _linlog_corr = -14;
        public int LinLogCor
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

        private SolidBrush pana_text_brush = new SolidBrush(Color.Khaki);
        private System.Drawing.Font pana_font = new System.Drawing.Font("Tahoma", 7F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));

        private Pen dhp = new Pen(Color.FromArgb(0, 255, 0)),
                           dhp1 = new Pen(Color.FromArgb(150, 0, 0, 255)),
                           dhp2 = new Pen(Color.FromArgb(150, 255, 0, 0));

        private System.Drawing.Font font14 = new System.Drawing.Font("Arial", 14, FontStyle.Bold),
                            font9 = new System.Drawing.Font("Arial", 9);


        #endregion

        #region General Routines


        #region Drawing Routines
        // ======================================================
        // Drawing Routines
        // ======================================================

        #region GDI+ General Routines

        public void Init()
        {
            int W = this.Width; // target.Width;
            int H = this.Height; // target.Height;

            CreateDisplayRegions();

            new_display_data = new float[BUFFER_SIZE];
            current_display_data = new float[BUFFER_SIZE];

            for (int i = 0; i < BUFFER_SIZE; i++)
            {
                new_display_data[i] = -200.0f;
                current_display_data[i] = -200.0f;
            }

            Pixels = W;
        }

        public void DrawBackground()
        {
            // draws the background image for the display based
            // on the current selected display mode.

            this.Invalidate();
        }

        /// <summary>
        /// draws the vertical bar to highlight where a channel is on the panadapter
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="n">Channel object reference</param>
        /// <param name="left">left side of notch in pixel location</param>
        /// <param name="right">right side of notch, pixel location</param>
        /// <param name="top">top of bar</param>
        /// <param name="H">height of bar</param>
        /// <param name="on">color for notch on</param>
        /// <param name="off">color for notch off</param>
        /// <param name="highlight">highlight color to draw highlights on bar</param>
        /// <param name="active">true if notches are turned on</param>
        void drawChannelBar(Graphics g, Channel chan, int left, int right, int top, int height, Color c, Color h)
        {
            int width = right - left;

            // get a purty pen to draw with 
            Pen p = new Pen(h, 1);

            // shade in the notch
            g.FillRectangle(new SolidBrush(c), left, top, width, height);

            // draw a left and right line on the side of the rectancle if wide enough
            if (width > 2)
            {
                //g.DrawLine(p, left - 1, top, left - 1, top + height - 1);
                g.DrawLine(p, left, top, left, top + height - 1);
                g.DrawLine(p, right, top, right, top + height - 1);
                //g.DrawLine(p, right+1, top, right+1, top + height - 1);
            }
        }

        #endregion

        #region GDI+

        unsafe public void RenderGDIPlus(int rx, Graphics e)
        {
            DrawWideBand(e, rx);
        }

        private void UpdateDisplayPeak(float[] buffer, float[] new_data)
        {
            if (buffer[0] == CLEAR_FLAG)
            {
                //Debug.WriteLine("Clearing peak buf"); 
                for (int i = 0; i < BUFFER_SIZE; i++)
                    buffer[i] = new_data[i];
            }
            else
            {
                for (int i = 0; i < BUFFER_SIZE; i++)
                {
                    if (new_data[i] > buffer[i])
                        buffer[i] = new_data[i];
                    new_data[i] = buffer[i];
                }
            }
        }

        #region Drawing Routines
        // ======================================================
        // Drawing Routines
        // ======================================================

        float zoom_height = 1.5f;   // Should be > 1.  H = H/zoom_height
        unsafe private void DrawWideBandGrid(Graphics g, int rx)
        {
            int W = panRect.Width;
            int H = panRect.Height;
            g.FillRectangle(Brushes.Black, freqScalePanRect);
            g.DrawRectangle(new Pen(Color.AntiqueWhite, 2), freqScalePanRect);

            // draw background
            // g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);

            bool local_mox = false;
            bool displayduplex = false;
            // if (mox && rx == 1 && !tx_on_vfob) local_mox = true;
            // if (mox && rx == 2 && tx_on_vfob) local_mox = true;
            //if (rx == 1 && tx_on_vfob && mox && !rx2_enabled) local_mox = true;
            int Low = 0;// = rx_display_low;					// initialize variables
            int High = 0;// = rx_display_high;
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
            long vfo_hz = _vfo_hz; //cmaster.Getrxa(display_id + 2).RXFreq;// 

            if ((CurrentDisplayMode == DisplayMode.PANAFALL && (nreceivers <= 2 && display_duplex)) ||
                (CurrentDisplayMode == DisplayMode.PANAFALL && nreceivers > 2) ||
               (CurrentDisplayMode == DisplayMode.PANADAPTER && display_duplex)) displayduplex = true;

            //if (local_mox && !displayduplex)// || (mox && tx_on_vfob))
            //{
            //    Low = tx_display_low;
            //    High = tx_display_high;
            //    grid_max = tx_spectrum_grid_max;
            //    grid_min = tx_spectrum_grid_min;
            //    grid_step = tx_spectrum_grid_step;
            //    g.FillRectangle(tx_display_background_brush, 0, 0, W, H);
            //}
            //else
            //{
            Low = low_freq;
            High = high_freq;
            grid_max = spectrum_grid_max;
            grid_min = spectrum_grid_min;
            grid_step = spectrum_grid_step;
            g.FillRectangle(display_background_brush, 0, 0, W, H);
            // }
            f_diff = freq_diff;

            int y_range = grid_max - grid_min;
            if (split_display) grid_step *= 2;

            //int filter_low, filter_high;
            //int center_line_x;
            int[] band_edge_list;

            //filter_low = rx_filter_low;
            // filter_high = rx_filter_high;

            //if (rx_dsp_mode == DSPMode.DRM)
            //{
            //    filter_low = -5000;
            //    filter_high = 5000;
            //}

            int width = High - Low;

            // Calculate horizontal step size
            while (width / freq_step_size > 10)
            {
                freq_step_size = step_list[step_index] * (int)Math.Pow(10.0, step_power);
                step_index = (step_index + 1) % 4;
                if (step_index == 0) step_power++;
            }
            double w_pixel_step = (double)W * freq_step_size / width;
            int w_steps = width / freq_step_size;

            // calculate vertical step size
            int h_steps = (grid_max - grid_min) / grid_step;
            double h_pixel_step = (double)H / h_steps;
            int top = 0;// (int)((double)grid_step * H / y_range);

            // if (!local_mox && sub_rx1_enabled && rx == 1) //multi-rx
            // {
            // draw Sub RX filter
            // get filter screen coordinates
            //int filter_left_x = (int)((float)(filter_low - Low + vfoa_sub_hz - vfo_hz - rit_hz) / width * W);
            //int filter_right_x = (int)((float)(filter_high - Low + vfoa_sub_hz - vfo_hz - rit_hz) / width * W);

            // make the filter display at least one pixel wide.
            //if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

            //g.FillRectangle(sub_rx_filter_brush,	// draw filter overlay
            //  filter_left_x, top, filter_right_x - filter_left_x, H - top);

            // draw Sub RX 0Hz line
            // int x = (int)((float)(vfoa_sub_hz - vfo_hz - Low) / width * W);

            // g.DrawLine(sub_rx_zero_line_pen, x, top, x, H);
            //  g.DrawLine(sub_rx_zero_line_pen, x - 1, top, x - 1, H);
            // }
            // draw RX filter
            //if (!local_mox)// && (rx_dsp_mode == DSPMode.CWL || rx_dsp_mode == DSPMode.CWU))
            //{
            //    //filterLeft = (int)((float)(filter_low - Low - f_diff) / width * W);
            //    //filterRight = (int)((float)(filter_high - Low - f_diff) / width * W);
            //    //filterTop = (int)panRect.Top + 1;
            //    //filterBottom = (int)(panRect.Top + panRect.Height - 1);

            //    if (filterLeft == filterRight) filterRight = filterLeft + 1;
            //    // make the filter display at least one pixel wide.
            //    filterRect = new Rectangle(filterLeft,
            //                               filterTop,
            //                               filterRight - filterLeft,
            //                               filterBottom - filterTop);

            //    if ((filterLeft >= panRect.Left && filterLeft <= panRect.Right) ||
            //       (filterRight >= panRect.Left && filterRight <= panRect.Right) ||
            //       (filterLeft < panRect.Left && filterRight > panRect.Right))
            //    {
            //        g.FillRectangle(display_filter_brush,	// draw filter overlay
            //                         filterLeft,
            //                         top,
            //                         filterRight - filterLeft,
            //                         H - top);
            //    }
            //}

            //if (local_mox && (rx_dsp_mode != DSPMode.CWL && rx_dsp_mode != DSPMode.CWU))
            //{
            //    int filter_left_x = (int)((float)(filter_low - Low - f_diff) / width * W);
            //    int filter_right_x = (int)((float)(filter_high - Low - f_diff) / width * W);

            //    // make the filter display at least one pixel wide.
            //    if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

            //    if (vfoa_tx || (!displayduplex && vfob_tx))
            //    {
            //        g.FillRectangle(tx_filter_brush,	// draw filter overlay
            //          filter_left_x, top, filter_right_x - filter_left_x, H - top);
            //    }
            //}

            //if (!mox && draw_tx_filter &&
            //    (rx_dsp_mode != DSPMode.CWL && rx_dsp_mode != DSPMode.CWU))
            //{
            //    // get tx filter limits
            //    int filter_left_x;
            //    int filter_right_x;

            //    if (tx_on_vfob)
            //    {
            //        if (!split_enabled)
            //        {
            //            filter_left_x = (int)((float)(tx_filter_low - Low + xit_hz - rit_hz) / width * W);
            //            filter_right_x = (int)((float)(tx_filter_high - Low + xit_hz - rit_hz) / width * W);
            //        }
            //        else
            //        {
            //            filter_left_x = (int)((float)(tx_filter_low - Low + xit_hz - rit_hz + (vfob_sub_hz - vfoa_hz)) / width * W);
            //            filter_right_x = (int)((float)(tx_filter_high - Low + xit_hz - rit_hz + (vfob_sub_hz - vfoa_hz)) / width * W);
            //        }
            //    }
            //    else
            //    {
            //        if (!split_enabled)
            //        {
            //            filter_left_x = (int)((float)(tx_filter_low - Low - f_diff + xit_hz - rit_hz) / width * W);
            //            filter_right_x = (int)((float)(tx_filter_high - Low - f_diff + xit_hz - rit_hz) / width * W);
            //        }
            //        else
            //        {
            //            filter_left_x = (int)((float)(tx_filter_low - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
            //            filter_right_x = (int)((float)(tx_filter_high - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);
            //        }
            //    }

            //    g.DrawLine(tx_filter_pen, filter_left_x, top, filter_left_x, H);		// draw tx filter overlay
            //    g.DrawLine(tx_filter_pen, filter_left_x + 1, top, filter_left_x + 1, H);	// draw tx filter overlay
            //    g.DrawLine(tx_filter_pen, filter_right_x, top, filter_right_x, H);	// draw tx filter overlay
            //    g.DrawLine(tx_filter_pen, filter_right_x + 1, top, filter_right_x + 1, H);// draw tx filter overlay
            //}

            //if (!local_mox && !tx_on_vfob && draw_tx_cw_freq &&
            //    (rx_dsp_mode == DSPMode.CWL || rx_dsp_mode == DSPMode.CWU))
            //{
            //    int pitch = cw_pitch;
            //    if (rx_dsp_mode == DSPMode.CWL)
            //        pitch = -cw_pitch;

            //    int cw_line_x;
            //    if (!split_enabled)
            //        cw_line_x = (int)((float)(pitch - Low - f_diff + xit_hz - rit_hz) / width * W);
            //    else
            //        cw_line_x = (int)((float)(pitch - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

            //    g.DrawLine(tx_filter_pen, cw_line_x, top, cw_line_x, H);
            //    g.DrawLine(tx_filter_pen, cw_line_x + 1, top, cw_line_x + 1, H);
            //}

            //if (!local_mox && tx_on_vfob && draw_tx_cw_freq &&
            //    (rx_dsp_mode == DSPMode.CWL || rx_dsp_mode == DSPMode.CWU))
            //{
            //    int pitch = cw_pitch;
            //    if (rx_dsp_mode == DSPMode.CWL)
            //        pitch = -cw_pitch;

            //    int cw_line_x;
            //    if (!split_enabled)
            //        cw_line_x = (int)((float)(pitch - Low + xit_hz - rit_hz) / width * W);
            //    else
            //        cw_line_x = (int)((float)(pitch - Low + xit_hz - rit_hz + (vfoa_sub_hz - vfoa_hz)) / width * W);

            //    g.DrawLine(tx_filter_pen, cw_line_x, H + top, cw_line_x, H + H);
            //    g.DrawLine(tx_filter_pen, cw_line_x + 1, H + top, cw_line_x + 1, H + H);

            //}

            // draw 60m channels if in view
            //if (current_region == FRSRegion.US || current_region == FRSRegion.UK)
            //{
            //    foreach (Channel c in Console.Channels60m)
            //    {
            //        long rf_freq = vfo_hz;
            //        int rit = rit_hz;

            //        if (c.InBW((rf_freq + Low) * 1e-6, (rf_freq + High) * 1e-6)) // is channel visible?
            //        {
            //            bool on_channel = console.RX1IsOn60mChannel(c); // only true if you are on channel and are in an acceptable mode
            //            if (rx == 2) on_channel = console.RX2IsOn60mChannel(c);

            //            DSPMode mode = rx_dsp_mode;
            //            switch (mode)
            //            {
            //                case DSPMode.USB:
            //                case DSPMode.DIGU:
            //                case DSPMode.CWL:
            //                case DSPMode.CWU:
            //                case DSPMode.AM:
            //                case DSPMode.SAM:
            //                    break;
            //                default:
            //                    on_channel = false; // make sure other modes do not look as if they could transmit
            //                    break;
            //            }

            //            // offset for CW Pitch to align display
            //            switch (rx_dsp_mode)
            //            {
            //                case (DSPMode.CWL):
            //                    rf_freq += cw_pitch;
            //                    break;
            //                case (DSPMode.CWU):
            //                    rf_freq -= cw_pitch;
            //                    break;
            //            }

            //            int chan_left_x = (int)((float)(c.Freq * 1e6 - rf_freq - c.BW / 2 - Low - rit) / width * W);
            //            int chan_right_x = (int)((float)(c.Freq * 1e6 - rf_freq + c.BW / 2 - Low - rit) / width * W);

            //            if (chan_right_x == chan_left_x)
            //                chan_right_x = chan_left_x + 1;

            //            // decide colors to draw notch
            //            Color c1 = channel_background_off;
            //            Color c2 = channel_foreground;

            //            if (on_channel)
            //            {
            //                c1 = channel_background_on;
            //            }

            //            drawChannelBar(g, c, chan_left_x, chan_right_x, top, H - top, c1, c2);
            //        }
            //    }
            //}

            // Draw a Zero Beat line on CW filter
            //if (!local_mox && show_cwzero_line &&
            //    (rx_dsp_mode == DSPMode.CWL || rx_dsp_mode == DSPMode.CWU))
            //{
            //    int pitch = cw_pitch;
            //    if (rx_dsp_mode == DSPMode.CWL)
            //        pitch = -cw_pitch;

            //    int cw_line_x1;
            //    if (!split_enabled)
            //        cw_line_x1 = (int)((float)(pitch - Low - f_diff) / width * W);
            //    else
            //        cw_line_x1 = (int)((float)(pitch - Low + (vfoa_sub_hz - vfo_hz)) / width * W);

            //    g.DrawLine(cw_zero_pen, cw_line_x1, top, cw_line_x1, H);
            //    g.DrawLine(cw_zero_pen, cw_line_x1 + 1, top, cw_line_x1 + 1, H);

            //}

            //if (!local_mox && show_cwzero_line &&
            //    (rx_dsp_mode == DSPMode.CWL || rx_dsp_mode == DSPMode.CWU))
            //{
            //    int pitch = cw_pitch;
            //    if (rx_dsp_mode == DSPMode.CWL)
            //        pitch = -cw_pitch;

            //    int cw_line_x1;
            //    if (!split_enabled)
            //        cw_line_x1 = (int)((float)(pitch - Low - f_diff) / width * W);
            //    else
            //        cw_line_x1 = (int)((float)(pitch - Low + (vfoa_sub_hz - vfoa_hz)) / width * W);

            //    g.DrawLine(tx_filter_pen, cw_line_x1, H + top, cw_line_x1, H + H);
            //    g.DrawLine(tx_filter_pen, cw_line_x1 + 1, H + top, cw_line_x1 + 1, H + H);
            //}

            // Draw 0Hz vertical line if visible
            //center_line_x = (int)((float)(-f_diff - Low) / width * W); // locked 0 line

            //if (center_line_x >= 0 && center_line_x <= W)
            //{            
            //        g.DrawLine(grid_zero_pen, center_line_x, top, center_line_x, H);
            //        g.DrawLine(grid_zero_pen, center_line_x + 1, top, center_line_x + 1, H);
            //}

            //if (show_freq_offset)
            //{               
            //        g.DrawString("0", font9, grid_zero_pen.Brush, center_line_x - 5, (float)Math.Floor(H * .01));
            //}

            double vfo = Low;
            //int rxn = rx;
            //int rn = 0;
            //if (rx == 4)
            //    rn = 4;
            //if (rx == 5)
            //    rn = 5;
            //if (rx == 6)
            //    rn = 6;


            //switch (rx_dsp_mode)
            //{
            //    case DSPMode.CWL:
            //        vfo += cw_pitch;
            //        break;
            //    case DSPMode.CWU:
            //        vfo -= cw_pitch;
            //        break;
            //    default:
            //        break;
            //}

            long vfo_round = ((long)(vfo / freq_step_size)) * freq_step_size;
            long vfo_delta = (long)(vfo - vfo_round);

            int f_steps = (width / freq_step_size) + 1;

            // Draw vertical lines - band edge markers and freq text
            for (int i = 0; i < f_steps + 1; i++)
            {
                string label;
                int offsetL;
                int offsetR;

                int fgrid = i * freq_step_size + (Low / freq_step_size) * freq_step_size;
                double actual_fgrid = ((double)(vfo_round + fgrid)) / 1000000;
                int vgrid = (int)((double)(fgrid - vfo_delta - Low) / width * W);
                string freq_num = actual_fgrid.ToString();

                if (!show_freq_offset)
                {
                    switch (current_region)
                    {
                        case FRSRegion.LAST:
                            if (!show_freq_offset)
                            {
                                g.DrawLine(band_edge_pen, vgrid, top, vgrid, H);

                                label = actual_fgrid.ToString("f1");
                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;

                                // g.DrawString(label, font9, band_edge_pen.Brush, vgrid - offsetL, freqScalePanRect.Top + 3);//(float)Math.Floor(H * .01));

                                int fgrid_2 = ((i + 1) * freq_step_size) + (int)((Low / freq_step_size) * freq_step_size);
                                int x_2 = (int)(((float)(fgrid_2 - vfo_delta - Low) / width * W));
                                float scale = (float)(x_2 - vgrid) / inbetweenies;

                                for (int j = 1; j < inbetweenies; j++)
                                {
                                    float x3 = (float)vgrid + (j * scale);
                                    g.DrawLine(grid_pen_inb, x3, top, x3, H);
                                }
                                break;
                            }
                            break;
                        case FRSRegion.US:
                        case FRSRegion.Extended:
                            goto default;
                        //if (actual_fgrid == 0.1357 || actual_fgrid == 0.1378 ||
                        //    actual_fgrid == 0.472 || actual_fgrid == 0.479 ||
                        //    actual_fgrid == 1.8 || actual_fgrid == 2.0 ||
                        //             actual_fgrid == 3.5 || actual_fgrid == 4.0 ||
                        //             actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
                        //             actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                        //             actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                        //             actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                        //             actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                        //             actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                        //    actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                        //    actual_fgrid == 50.0 || actual_fgrid == 54.0 ||
                        //    actual_fgrid == 144.0 || actual_fgrid == 148.0)
                        //{

                        //    goto case FRSRegion.LAST;
                        //}
                        //else goto default;

                        case FRSRegion.Spain:
                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.India:
                            if (actual_fgrid == 1.81 || actual_fgrid == 1.86 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.9 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Europe:
                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.08 || actual_fgrid == 51.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.UK:
                            if (actual_fgrid == 0.472 || actual_fgrid == 0.479 ||
                                actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 5.2585 || actual_fgrid == 5.4065 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 52.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Italy_Plus:
                            if (actual_fgrid == 1.81 || actual_fgrid == 2.0 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.8 ||
                                actual_fgrid == 6.975 || actual_fgrid == 7.2 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.08 || actual_fgrid == 51.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Japan:
                            if (actual_fgrid == .1357 || actual_fgrid == .1378 ||
                            actual_fgrid == 1.81 || actual_fgrid == 1.825 ||
                            actual_fgrid == 1.9075 || actual_fgrid == 1.9125 ||
                            actual_fgrid == 3.5 || actual_fgrid == 3.575 ||
                            actual_fgrid == 3.599 || actual_fgrid == 3.612 ||
                            actual_fgrid == 3.68 || actual_fgrid == 3.687 ||
                            actual_fgrid == 3.702 || actual_fgrid == 3.716 ||
                            actual_fgrid == 3.745 || actual_fgrid == 3.77 ||
                            actual_fgrid == 3.791 || actual_fgrid == 3.805 ||
                            actual_fgrid == 7.0 || actual_fgrid == 7.2 ||
                            actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                            actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                            actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                            actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                            actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                            actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                            actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Australia:
                            if (actual_fgrid == .1357 || actual_fgrid == .1378 ||
                                actual_fgrid == 0.472 || actual_fgrid == 0.479 ||
                                actual_fgrid == 1.8 || actual_fgrid == 1.875 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.7 ||
                                actual_fgrid == 3.776 || actual_fgrid == 3.8 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;

                        case FRSRegion.Norway:
                            if (actual_fgrid == 1.8 || actual_fgrid == 1.875 ||
                                actual_fgrid == 3.5 || actual_fgrid == 3.7 ||
                                actual_fgrid == 3.776 || actual_fgrid == 3.8 ||
                                actual_fgrid == 5.26 || actual_fgrid == 5.41 ||
                                actual_fgrid == 7.0 || actual_fgrid == 7.3 ||
                                actual_fgrid == 10.1 || actual_fgrid == 10.15 ||
                                actual_fgrid == 14.0 || actual_fgrid == 14.35 ||
                                actual_fgrid == 18.068 || actual_fgrid == 18.168 ||
                                actual_fgrid == 21.0 || actual_fgrid == 21.45 ||
                                actual_fgrid == 24.89 || actual_fgrid == 24.99 ||
                                actual_fgrid == 28.0 || actual_fgrid == 29.7 ||
                                actual_fgrid == 50.0 || actual_fgrid == 54.0)
                            {
                                goto case FRSRegion.LAST;
                            }
                            else goto default;
                        default:
                            //draw vertical in between lines
                            if (grid_control)
                            {

                                g.DrawLine(grid_pen, vgrid, top, vgrid, H);			//wa6ahl

                                int fgrid_2 = ((i + 1) * freq_step_size) + (int)((Low / freq_step_size) * freq_step_size);
                                int x_2 = (int)(((float)(fgrid_2 - vfo_delta - Low) / width * W));
                                float scale = (float)(x_2 - vgrid) / inbetweenies;

                                for (int j = 1; j < inbetweenies; j++)
                                {
                                    float x3 = (float)vgrid + (j * scale);
                                    g.DrawLine(grid_pen_inb, x3, top, x3, H);
                                }
                            }

                            if (((double)((int)(actual_fgrid * 1000))) == actual_fgrid * 1000)
                            {
                                label = actual_fgrid.ToString("f1"); //wa6ahl

                                // if(actual_fgrid > 1300.0)
                                //label = label.Substring(label.Length-4);

                                if (actual_fgrid < 10) offsetL = (int)((label.Length + 1) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length + 1) * 4.1) - 11;
                                else offsetL = (int)((label.Length + 1) * 4.1) - 8;
                            }
                            else
                            {
                                //display freqencies
                                string temp_string;
                                int jper;
                                //label = actual_fgrid.ToString("f5"); //x100
                                label = actual_fgrid.ToString("f4");
                                temp_string = label;
                                jper = label.IndexOf('.') + 4;
                                label = label.Insert(jper, " ");

                                //if(actual_fgrid > 1300.0)
                                //	label = label.Substring(label.Length-4);

                                if (actual_fgrid < 10) offsetL = (int)((label.Length) * 4.1) - 14;
                                else if (actual_fgrid < 100.0) offsetL = (int)((label.Length) * 4.1) - 11;
                                else offsetL = (int)((label.Length) * 4.1) - 8;
                            }

                            g.DrawString(label, font9, grid_text_brush, vgrid - offsetL, freqScalePanRect.Top + 3);//(float)Math.Floor(H * .01));

                            break;
                    }
                }
                else
                {
                    vgrid = Convert.ToInt32((double)-(fgrid - Low) / (Low - High) * W);	//wa6ahl
                    g.DrawLine(grid_pen, vgrid, top, vgrid, H);			//wa6ahl

                    label = fgrid.ToString();
                    offsetL = (int)((label.Length + 1) * 4.1);
                    offsetR = (int)(label.Length * 4.1);
                    if ((vgrid - offsetL >= 0) && (vgrid + offsetR < W) && (fgrid != 0))
                    {
                        g.DrawString(label, font9, grid_text_brush, vgrid - offsetL, freqScalePanRect.Top + 3);//(float)Math.Floor(H * .01));
                    }
                }
            }

            switch (current_region)
            {
                case FRSRegion.Australia:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 1875000, 
                 3500000, 3800000, 7000000, 7300000, 10100000, 10150000, 14000000, 14350000, 18068000, 
                 18168000, 21000000, 21450000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                case FRSRegion.UK:
                    band_edge_list = new int[] { 472000, 479000, 1810000, 2000000, 3500000, 3800000,
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
                case FRSRegion.US:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1800000, 2000000, 3500000, 4000000,
                7000000, 7300000, 10100000, 10150000, 14000000, 14350000,  18068000, 18168000, 21000000, 21450000,
                24890000, 24990000, 28000000, 29700000, 50000000, 54000000 };
                    break;
                case FRSRegion.Japan:
                    band_edge_list = new int[]{ 135700, 137800, 472000, 479000, 1810000, 1810000, 1907500, 1912500, 
                3500000, 3575000, 3599000, 3612000, 3687000, 3702000, 3716000, 3745000, 3770000, 3791000, 3805000,
                7000000, 7200000, 10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
                24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
                default:
                    band_edge_list = new int[]{1800000, 2000000, 3500000, 4000000, 7000000, 7300000, 
                10100000, 10150000, 14000000, 14350000, 18068000, 18168000, 21000000, 21450000,
                24890000, 24990000, 28000000, 29700000, 50000000, 54000000, 144000000, 148000000 };
                    break;
            }

            for (int i = 0; i < band_edge_list.Length; i++)
            {
                double band_edge_offset = band_edge_list[i] - vfo;
                if (band_edge_offset >= Low && band_edge_offset <= High)
                {
                    int temp_vline = (int)((double)(band_edge_offset - Low) / width * W);//wa6ahl

                    g.DrawLine(band_edge_pen, temp_vline, top, temp_vline, H);//wa6ahl
                }

            }

            // if (grid_control)
            // {
            // Draw horizontal lines
            for (int i = 1; i < h_steps; i++)
            {
                int xOffset = 0;
                int num = grid_max - i * grid_step;
                int y = (int)((double)(grid_max - num) * H / y_range);

                g.DrawLine(hgrid_pen, 0, y, W, y);

                // Draw horizontal line labels
                if (i != 0) // avoid intersecting vertical and horizontal labels
                {
                    num = grid_max - i * grid_step;
                    string label = num.ToString();
                    if (label.Length == 3)
                        xOffset = (int)g.MeasureString("-", font9).Width - 2;
                    // int offset = (int)(label.Length * 4.1);
                    SizeF size = g.MeasureString(label, font9);

                    int x = (int)(W - size.Width - 3); // 0;
                    //switch (display_label_align)
                    //{
                    //    case DisplayLabelAlignment.LEFT:
                    //        x = xOffset + 3;
                    //        break;
                    //    case DisplayLabelAlignment.CENTER:
                    //        x = center_line_x + xOffset;
                    //        break;
                    //    case DisplayLabelAlignment.RIGHT:
                    //        x = (int)(W - size.Width - 3);
                    //        break;
                    //    case DisplayLabelAlignment.AUTO:
                    //        x = xOffset + 3;
                    //        break;
                    //    case DisplayLabelAlignment.OFF:
                    //        x = W;
                    //        break;
                    //}
                    // console.DisplayGridX = x;
                    // console.DisplayGridW = (int)(x + size.Width);

                    y -= 8;
                    if (y + 9 < H)
                    {
                        g.DrawString(label, font9, grid_text_brush, x, y);
                    }
                }
            }
            //  }

            // draw long cursor & filter overlay
            //if (current_click_tune_mode != ClickTuneMode.Off)
            //{
            //    Pen p;
            //    p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;

            //    if (ClickTuneFilter)
            //    {
            //        if (display_cursor_y <= H)
            //        {
            //            double freq_low = freq + filter_low;
            //            double freq_high = freq + filter_high;
            //            int x1 = (int)((freq_low - Low) / width * W);
            //            int x2 = (int)((freq_high - Low) / width * W);
            //            //g.FillRectangle(display_filter_brush, x1, top, x2 - x1, H - top);

            //            if (rx_dsp_mode == DSPMode.CWL || rx_dsp_mode == DSPMode.CWU)
            //            {
            //                g.FillRectangle(display_filter_brush, display_cursor_x -
            //                    ((x2 - x1) / 2), top, x2 - x1, H - top);
            //            }
            //            else
            //            {
            //                g.FillRectangle(display_filter_brush, x1, top, x2 - x1, H - top);
            //            }
            //        }

            //        if (display_cursor_y <= H)
            //        {
            //            g.DrawLine(p, display_cursor_x, top, display_cursor_x, H);
            //            if (ShowCTHLine) g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
            //        }
            //    }
            //    //}
            //}

            //if (console.PowerOn && current_display_mode == DisplayMode.PANADAPTER)
            //{
            //    // get filter screen coordinates
            //    int filter_left_x = (int)((float)(filter_low - Low) / width * W);
            //    int filter_right_x = (int)((float)(filter_high - Low) / width * W);
            //    if (filter_left_x == filter_right_x) filter_right_x = filter_left_x + 1;

            //    int x1_rx_gain = 0, x2_rx_gain = 0, x3_rx_gain = 0, x1_rx_hang = 0, x2_rx_hang = 0, x3_rx_hang = 0;

            //    if (spectrum_line)
            //    {
            //        x1_rx_gain = 40;
            //        x2_rx_gain = W - 40;
            //        x3_rx_gain = 50;
            //    }
            //    else
            //    {
            //        x1_rx_gain = filter_left_x;
            //        x2_rx_gain = filter_right_x;
            //        x3_rx_gain = x1_rx_gain;
            //    }

            //    if (rx1_hang_spectrum_line)
            //    {
            //        x1_rx_hang = 40;
            //        x2_rx_hang = W - 40;
            //        x3_rx_hang = 50;
            //    }
            //    else
            //    {
            //        x1_rx_hang = filter_left_x;
            //        x2_rx_hang = filter_right_x;
            //        x3_rx_hang = x1_rx_hang;
            //    }

            //    if (!local_mox)
            //    {
            //        double rx_thresh = 0.0;
            //        float rx_agcknee_y_value = 0.0f;
            //        float rx_cal_offset = 0.0f;
            //        double rx_hang = 0.0;
            //        float rx_agc_hang_y = 0.0f;
            //        AGCMode mode;
            //        int rx_agc_fixed_gain = 0;

            //        if (rx == 1)
            //        {
            //            mode = console.RX1AGCMode;
            //            rx_agc_fixed_gain = console.SetupForm.AGCFixedGain;
            //            WDSP.GetRXAAGCThresh(WDSP.id(0, 0), &rx_thresh, 4096.0, sample_rate);
            //            WDSP.GetRXAAGCHangLevel(WDSP.id(0, 0), &rx_hang);
            //        }
            //        else
            //        {
            //            mode = console.RX2AGCMode;
            //            rx_agc_fixed_gain = console.SetupForm.AGCRX2FixedGain;
            //            WDSP.GetRXAAGCThresh(WDSP.id(2, 0), &rx_thresh, 4096.0, sample_rate);
            //            WDSP.GetRXAAGCHangLevel(WDSP.id(2, 0), &rx_hang);
            //        }
            //        rx_thresh = Math.Round(rx_thresh);

            //        switch (mode)
            //        {
            //            case AGCMode.FIXD:
            //                rx_cal_offset = -18.0f;
            //                break;
            //            default:
            //                rx_cal_offset = 2.0f + (rx_display_cal_offset +
            //                    (preamp_offset - alex_preamp_offset) - rx_fft_size_offset);
            //                break;
            //        }

            //        string rx_agc = "";

            //        switch (mode)
            //        {
            //            case AGCMode.FIXD:
            //                rx_agcknee_y_value = dBToPixel(-(float)rx_agc_fixed_gain + rx_cal_offset);
            //                rx_agc = "-F";
            //                break;
            //            default:
            //                rx_agcknee_y_value = dBToPixel((float)rx_thresh + rx_cal_offset);
            //                rx_agc_hang_y = dBToPixel((float)rx_hang + rx_cal_offset);

            //                //show hang line
            //                if (display_agc_hang_line && mode != AGCMode.MED && mode != AGCMode.FAST)
            //                {
            //                    agcHang = new Rectangle();
            //                    agcHang.Height = 8; agcHang.Width = 8; agcHang.X = 40;
            //                    agcHang.Y = (int)rx_agc_hang_y - agcHang.Height;
            //                    g.FillRectangle(Brushes.Yellow, agcHang);
            //                    using (Pen p = new Pen(Color.Yellow))
            //                    {
            //                        p.DashStyle = DashStyle.Dot;
            //                        g.DrawLine(p, x3_rx_hang, rx_agc_hang_y, x2_rx_hang, rx_agc_hang_y);
            //                        g.DrawString("-H", pana_font, pana_text_brush, agcKnee.X + agcHang.Width, agcHang.Y - (agcHang.Height));
            //                    }
            //                }
            //                rx_agc = "-G";
            //                break;
            //        }

            //        // show agc line
            //        if (show_agc)
            //        {
            //            agcKnee = new Rectangle();
            //            agcKnee.Height = 8; agcKnee.Width = 8; agcKnee.X = 40;
            //            agcKnee.Y = (int)rx_agcknee_y_value - agcKnee.Height;
            //            g.FillRectangle(Brushes.YellowGreen, agcKnee);

            //            using (Pen p = new Pen(Color.YellowGreen))
            //            {
            //                p.DashStyle = DashStyle.Dot;
            //                g.DrawLine(p, x1_rx_gain, rx_agcknee_y_value, x2_rx_gain, rx_agcknee_y_value);
            //                g.DrawString(rx_agc, pana_font, pana_text_brush, agcKnee.X + agcKnee.Width, agcKnee.Y - (agcKnee.Height));
            //            }
            //        }
            //    }

            //}
            //if (high_swr && rx == 1)
            //    g.DrawString("High SWR", font14, Brushes.Red, 245, 20);

        }

        private float dBToPixel(float dB)
        {
            return (float)(spectrum_grid_max - dB) * panRect.Height / (spectrum_grid_max - spectrum_grid_min);
        }

        private void DrawOffBackground(Graphics g, int W, int H, bool bottom)
        {
            // draw background
            g.FillRectangle(display_background_brush, 0, bottom ? H : 0, W, H);

            if (high_swr && !bottom)
                g.DrawString("High SWR", font14, Brushes.Red, 245, 20);
        }

        unsafe private bool DrawWideBand(Graphics g, int rx)
        {
            int W = panRect.Width;
            int H = panRect.Height;

            DrawWideBandGrid(g, rx);

            if (pan_fill)
            {
                if (points == null || points.Length < W + 2)
                    points = new Point[W + 2];
            }
            else
            {
                if (points == null || points.Length < W)
                    points = new Point[W];			// array of points to display
            }
            float slope = 0.0F;						// samples to process per pixel
            int num_samples = 0;					// number of samples to process
            int start_sample_index = 0;				// index to begin looking at samples
            int Low = 0;// rx_display_low;
            int High = 0;// rx_display_high;
            // int yRange = spectrum_grid_max - spectrum_grid_min;
            float local_max_y = float.MinValue;
            bool local_mox = false;
            bool displayduplex = false;

            int grid_max = 0;
            int grid_min = 0;

            if ((CurrentDisplayMode == DisplayMode.PANAFALL && (nreceivers <= 2 && display_duplex)) ||
                (CurrentDisplayMode == DisplayMode.PANAFALL && nreceivers > 2) ||
               (CurrentDisplayMode == DisplayMode.PANADAPTER && display_duplex)) displayduplex = true;


            //    Low = rx_display_low;
            High = high_freq;
            grid_max = spectrum_grid_max;
            grid_min = spectrum_grid_min;
            // }

            if (rx_dsp_mode == DSPMode.DRM)
            {
                Low += 12000;
                High += 12000;
            }

            int yRange = grid_max - grid_min;
            int width = High - Low;

            if (data_ready)
            {
                fixed (void* rptr = &new_display_data[0])
                fixed (void* wptr = &current_display_data[0])
                    Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));

                data_ready = false;
            }

            try
            {
                Parallel.For(0, W, (i) =>    //for(int i = 0; i < W; i++)
                 {
                     float max = float.MinValue;

                     max = current_display_data[i];
                     max += rx_display_cal_offset;
                     max += preamp_offset;

                     if (max > local_max_y)
                     {
                         local_max_y = max;
                         max_x = i;
                     }

                     points[i].X = i;
                     points[i].Y = (int)(Math.Floor((grid_max - max) * H / yRange));
                     points[i].Y = Math.Min(points[i].Y, H);
                 });
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            max_y = local_max_y;

            try
            {
                if (pan_fill)
                {
                    points[W].X = W; points[W].Y = H;
                    points[W + 1].X = 0; points[W + 1].Y = H;

                    g.FillPolygon(data_line_fpen.Brush, points);

                    points[W] = points[W - 1];
                    points[W + 1] = points[W - 1];
                    data_line_pen.Color = data_line_color;
                    g.DrawLines(data_line_pen, points);
                }
                else
                {
                    g.DrawLines(data_line_pen, points);
                }

            }
            catch (Exception)
            {
                // Trace.WriteLine(ex);
            }

            points = null;

            // draw long cursor
            try
            {
                if (current_click_tune_mode != ClickTuneMode.Off)
                {
                    Pen p;
                    if (current_click_tune_mode == ClickTuneMode.VFOA)
                        p = new Pen(grid_text_color);
                    else p = new Pen(Color.Red);

                    if (display_cursor_y <= H)
                    {
                        g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H);
                        g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
                    }
                }
            }
            catch (Exception)
            {
            }

            return true;
        }

        private bool waterfall_agc = false;
        public bool WaterfallAGC
        {
            get { return waterfall_agc; }
            set { waterfall_agc = value; }
        }

        private int waterfall_update_period = 100; // in ms
        public int WaterfallUpdatePeriod
        {
            get { return waterfall_update_period; }
            set { waterfall_update_period = value; }
        }

        //private readonly HiPerfTimer timer_waterfall = new HiPerfTimer();
        ////  private readonly HiPerfTimer timer_waterfall2 = new HiPerfTimer();
        //private float waterfallPreviousMinValue = 0.0f;
        //unsafe private bool DrawWaterfall(Graphics g, int rx)
        //{
        //    int W = waterfallRect.Width;
        //    int H = waterfallRect.Height;

        //    // if (grid_control) DrawWaterfallGrid(ref g, rx, bottom);

        //    if (waterfall_data == null || waterfall_data.Length < W)
        //    {
        //        waterfall_data = new float[W];		// array of points to display
        //    }
        //    float slope = 0.0F;						// samples to process per pixel
        //    int num_samples = 0;					// number of samples to process
        //    int start_sample_index = 0;				// index to begin looking at samples
        //    int Low = 0;
        //    int High = 0;
        //    float local_max_y = float.MinValue;
        //    bool local_mox = false;
        //    //if (rx == 1 && !tx_on_vfob && mox) local_mox = true;
        //    //if (rx == 2 && tx_on_vfob && mox) local_mox = true;
        //    float local_min_y_w3sz = float.MaxValue;  //added by w3sz
        //    float display_min_w3sz = float.MaxValue; //added by w3sz
        //    float display_max_w3sz = float.MinValue; //added by w3sz
        //    float min_y_w3sz = float.MaxValue;  //w3sz
        //    int R = 0, G = 0, B = 0;
        //    int grid_max = 0;
        //    int grid_min = 0;
        //    bool displayduplex = false;
        //    float low_threshold = 0.0f;
        //    float high_threshold = 0.0f;
        //    float waterfall_minimum = 0.0f;
        //    float rx1_waterfall_minimum = 0.0f;
        //    //  float rx2_waterfall_minimum = 0.0f;
        //    ColorSheme cScheme = ColorScheme.enhanced;
        //    Color low_color = Color.Black;
        //    Color mid_color = Color.Red;
        //    Color high_color = Color.Blue;

        //    if ((CurrentDisplayMode == DisplayMode.PANAFALL && (nreceivers <= 2 && display_duplex)) ||
        //        (CurrentDisplayMode == DisplayMode.PANAFALL && nreceivers > 2) ||
        //       (CurrentDisplayMode == DisplayMode.PANADAPTER && display_duplex)) displayduplex = true;

        //    ccScheme = color_scheme;
        //    low_color = waterfall_low_color;
        //    mid_color = waterfall_mid_color;
        //    high_color = waterfall_high_color;
        //    Low = low_freq;
        //    High = high_freq;
        //    grid_max = spectrum_grid_max;
        //    grid_min = spectrum_grid_min;
        //    high_threshold = waterfall_high_threshold;

        //    if (waterfall_agc)
        //    {
        //        waterfall_minimum = rx1_waterfall_minimum;
        //        low_threshold = waterfallPreviousMinValue;
        //    }
        //    else
        //        low_threshold = waterfall_low_threshold;

        //    if (console.PowerOn)
        //    {
        //        if (rx_dsp_mode == DSPMode.DRM) //||
        //        // (rx_dsp_mode == DSPMode.DRM && rx == 2))
        //        {
        //            Low += 12000;
        //            High += 12000;
        //        }

        //        if (data_ready)
        //        {
        //            if (!displayduplex && local_mox && (rx_dsp_mode == DSPMode.CWL || rx_dsp_mode == DSPMode.CWU))
        //            {
        //                for (int i = 0; i < current_display_data.Length; i++)
        //                    current_display_data[i] = -200.0f;
        //            }
        //            else
        //            {
        //                fixed (void* rptr = &new_display_data[0])
        //                fixed (void* wptr = &current_display_data[0])
        //                    Win32.memcpy(wptr, rptr, BUFFER_SIZE * sizeof(float));

        //            }
        //            data_ready = false;
        //        }

        //        //if (average_on && local_mox && !displayduplex)
        //        //    console.UpdateRX1DisplayAverage(rx1_average_buffer, current_display_data);               

        //        //if (peak_on && local_mox && !displayduplex)
        //        //    UpdateDisplayPeak(rx1_peak_buffer, current_display_data);

        //        int duration = 0;
        //        timer_waterfall.Stop();
        //        duration = (int)timer_waterfall.DurationMsec;

        //        if (duration > waterfall_update_period || duration < 0)
        //        {
        //            timer_waterfall.Start();
        //            num_samples = (High - Low);
        //            start_sample_index = (BUFFER_SIZE >> 1) + ((Low * BUFFER_SIZE) / sample_rate);
        //            num_samples = ((High - Low) * BUFFER_SIZE / sample_rate);
        //            if (start_sample_index < 0) start_sample_index += 4096;
        //            if ((num_samples - start_sample_index) > (BUFFER_SIZE + 1))
        //                num_samples = BUFFER_SIZE - start_sample_index;

        //            slope = (float)num_samples / (float)W;

        //            for (int i = 0; i < W; i++)
        //            {
        //                float max = float.MinValue;
        //                float dval = i * slope + start_sample_index;
        //                int lindex = (int)Math.Floor(dval);
        //                int rindex = (int)Math.Floor(dval + slope);

        //                // if (rx == 1)
        //                // {
        //                if (local_mox && !displayduplex)
        //                {
        //                    if (slope <= 1.0 || lindex == rindex)
        //                    {
        //                        max = current_display_data[lindex % 4096] * ((float)lindex - dval + 1) + current_display_data[(lindex + 1) % 4096] * (dval - (float)lindex);
        //                    }
        //                    else
        //                    {
        //                        for (int j = lindex; j < rindex; j++)
        //                            if (current_display_data[j % 4096] > max) max = current_display_data[j % 4096];
        //                    }
        //                }
        //                else
        //                    max = current_display_data[i];

        //                max += rx_display_cal_offset;

        //                max += (preamp_offset - alex_preamp_offset);

        //                if (max > local_max_y)
        //                {
        //                    local_max_y = max;
        //                    max_x = i;
        //                }

        //                //below added by w3sz
        //                if (max < local_min_y_w3sz)
        //                {
        //                    local_min_y_w3sz = max;
        //                }
        //                //end of addition by w3sz

        //                waterfall_data[i] = max;
        //            }

        //            max_y = local_max_y;
        //            min_y_w3sz = local_min_y_w3sz;

        //            BitmapData bitmapData;
        //            bitmapData = waterfall_bmp.LockBits(
        //                new Rectangle(0, 0, waterfall_bmp.Width, waterfall_bmp.Height),
        //                ImageLockMode.ReadWrite,
        //                waterfall_bmp.PixelFormat);

        //            int pixel_size = 3;
        //            byte* row = null;

        //            // first scroll image
        //            int total_size = bitmapData.Stride * bitmapData.Height;		// find buffer size
        //            Win32.memcpy(new IntPtr((int)bitmapData.Scan0 + bitmapData.Stride).ToPointer(),
        //                bitmapData.Scan0.ToPointer(),
        //                total_size - bitmapData.Stride);

        //            row = (byte*)bitmapData.Scan0;

        //            //int i = 0;
        //            switch (cScheme)
        //            {
        //                case (ColorScheme.original):
        //                    {
        //                        // draw new data
        //                        for (int i = 0; i < W; i++)	// for each pixel in the new line
        //                        {
        //                            //int R, G, B;		// variables to save Red, Green and Blue component values

        //                            if (waterfall_data[i] <= low_threshold)		// if less than low threshold, just use low color
        //                            {
        //                                R = low_color.R;
        //                                G = low_color.G;
        //                                B = low_color.B;
        //                            }
        //                            else if (waterfall_data[i] >= high_threshold)// if more than high threshold, just use high color
        //                            {
        //                                R = high_color.R;
        //                                G = high_color.G;
        //                                B = high_color.B;
        //                            }
        //                            else // use a color between high and low
        //                            {
        //                                float percent = (waterfall_data[i] - low_threshold) / (high_threshold - low_threshold);
        //                                if (percent <= 0.5)	// use a gradient between low and mid colors
        //                                {
        //                                    percent *= 2;

        //                                    R = (int)((1 - percent) * low_color.R + percent * mid_color.R);
        //                                    G = (int)((1 - percent) * low_color.G + percent * mid_color.G);
        //                                    B = (int)((1 - percent) * low_color.B + percent * mid_color.B);
        //                                }
        //                                else				// use a gradient between mid and high colors
        //                                {
        //                                    percent = (float)(percent - 0.5) * 2;

        //                                    R = (int)((1 - percent) * mid_color.R + percent * high_color.R);
        //                                    G = (int)((1 - percent) * mid_color.G + percent * high_color.G);
        //                                    B = (int)((1 - percent) * mid_color.B + percent * high_color.B);
        //                                }
        //                            }
        //                            // set pixel color
        //                            row[i * pixel_size + 0] = (byte)B;	// set color in memory
        //                            row[i * pixel_size + 1] = (byte)G;
        //                            row[i * pixel_size + 2] = (byte)R;
        //                        }
        //                    }
        //                    //waterfall_data[i] = (float)i/W*(high_threshold - low_threshold) + low_threshold;
        //                    break;

        //                case (ColorScheme.enhanced):
        //                    {
        //                        // draw new data
        //                        for (int i = 0; i < W; i++)	// for each pixel in the new line
        //                        {
        //                            if (waterfall_data[i] <= low_threshold)
        //                            {
        //                                R = low_color.R;
        //                                G = low_color.G;
        //                                B = low_color.B;
        //                            }
        //                            else if (waterfall_data[i] >= high_threshold)
        //                            {
        //                                R = 192;
        //                                G = 124;
        //                                B = 255;
        //                            }
        //                            else // value is between low and high
        //                            {
        //                                float range = high_threshold - low_threshold;
        //                                float offset = waterfall_data[i] - low_threshold;
        //                                float overall_percent = offset / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.

        //                                if (overall_percent < (float)2 / 9) // background to blue
        //                                {
        //                                    float local_percent = overall_percent / ((float)2 / 9);
        //                                    R = (int)((1.0 - local_percent) * low_color.R);
        //                                    G = (int)((1.0 - local_percent) * low_color.G);
        //                                    B = (int)(low_color.B + local_percent * (255 - low_color.B));
        //                                }
        //                                else if (overall_percent < (float)3 / 9) // blue to blue-green
        //                                {
        //                                    float local_percent = (overall_percent - (float)2 / 9) / ((float)1 / 9);
        //                                    R = 0;
        //                                    G = (int)(local_percent * 255);
        //                                    B = 255;
        //                                }
        //                                else if (overall_percent < (float)4 / 9) // blue-green to green
        //                                {
        //                                    float local_percent = (overall_percent - (float)3 / 9) / ((float)1 / 9);
        //                                    R = 0;
        //                                    G = 255;
        //                                    B = (int)((1.0 - local_percent) * 255);
        //                                }
        //                                else if (overall_percent < (float)5 / 9) // green to red-green
        //                                {
        //                                    float local_percent = (overall_percent - (float)4 / 9) / ((float)1 / 9);
        //                                    R = (int)(local_percent * 255);
        //                                    G = 255;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)7 / 9) // red-green to red
        //                                {
        //                                    float local_percent = (overall_percent - (float)5 / 9) / ((float)2 / 9);
        //                                    R = 255;
        //                                    G = (int)((1.0 - local_percent) * 255);
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)8 / 9) // red to red-blue
        //                                {
        //                                    float local_percent = (overall_percent - (float)7 / 9) / ((float)1 / 9);
        //                                    R = 255;
        //                                    G = 0;
        //                                    B = (int)(local_percent * 255);
        //                                }
        //                                else // red-blue to purple end
        //                                {
        //                                    float local_percent = (overall_percent - (float)8 / 9) / ((float)1 / 9);
        //                                    R = (int)((0.75 + 0.25 * (1.0 - local_percent)) * 255);
        //                                    G = (int)(local_percent * 255 * 0.5);
        //                                    B = 255;
        //                                }
        //                            }

        //                            if (waterfall_minimum > waterfall_data[i]) //wfagc
        //                                waterfall_minimum = waterfall_data[i];

        //                            // set pixel color
        //                            row[i * pixel_size + 0] = (byte)B;	// set color in memory
        //                            row[i * pixel_size + 1] = (byte)G;
        //                            row[i * pixel_size + 2] = (byte)R;
        //                        }
        //                    }
        //                    break;

        //                case (ColorScheme.SPECTRAN):
        //                    {
        //                        // draw new data
        //                        for (int i = 0; i < W; i++)	// for each pixel in the new line
        //                        {
        //                            if (waterfall_data[i] <= low_threshold)
        //                            {
        //                                R = 0;
        //                                G = 0;
        //                                B = 0;
        //                            }
        //                            else if (waterfall_data[i] >= WaterfallHighThreshold) // white
        //                            {
        //                                R = 240;
        //                                G = 240;
        //                                B = 240;
        //                            }
        //                            else // value is between low and high
        //                            {
        //                                float range = WaterfallHighThreshold - low_threshold;
        //                                float offset = waterfall_data[i] - low_threshold;
        //                                float local_percent = ((100.0f * offset) / range);

        //                                if (local_percent < 5.0f)
        //                                {
        //                                    R = G = 0;
        //                                    B = (int)local_percent * 5;
        //                                }
        //                                else if (local_percent < 11.0f)
        //                                {
        //                                    R = G = 0;
        //                                    B = (int)local_percent * 5;
        //                                }
        //                                else if (local_percent < 22.0f)
        //                                {
        //                                    R = G = 0;
        //                                    B = (int)local_percent * 5;
        //                                }
        //                                else if (local_percent < 44.0f)
        //                                {
        //                                    R = G = 0;
        //                                    B = (int)local_percent * 5;
        //                                }
        //                                else if (local_percent < 51.0f)
        //                                {
        //                                    R = G = 0;
        //                                    B = (int)local_percent * 5;
        //                                }
        //                                else if (local_percent < 66.0f)
        //                                {
        //                                    R = G = (int)(local_percent - 50) * 2;
        //                                    B = 255;
        //                                }
        //                                else if (local_percent < 77.0f)
        //                                {
        //                                    R = G = (int)(local_percent - 50) * 3;
        //                                    B = 255;
        //                                }
        //                                else if (local_percent < 88.0f)
        //                                {
        //                                    R = G = (int)(local_percent - 50) * 4;
        //                                    B = 255;
        //                                }
        //                                else if (local_percent < 99.0f)
        //                                {
        //                                    R = G = (int)(local_percent - 50) * 5;
        //                                    B = 255;
        //                                }
        //                            }

        //                            if (waterfall_minimum > waterfall_data[i]) //wfagc
        //                                waterfall_minimum = waterfall_data[i];

        //                            // set pixel color
        //                            row[i * pixel_size + 0] = (byte)B;	// set color in memory
        //                            row[i * pixel_size + 1] = (byte)G;
        //                            row[i * pixel_size + 2] = (byte)R;
        //                        }
        //                    }
        //                    break;

        //                case (ColorScheme.BLACKWHITE):
        //                    {
        //                        // draw new data
        //                        for (int i = 0; i < W; i++)	// for each pixel in the new line
        //                        {
        //                            if (waterfall_data[i] <= low_threshold)
        //                            {
        //                                R = 0;
        //                                G = 0;
        //                                B = 0;
        //                            }
        //                            else if (waterfall_data[i] >= WaterfallHighThreshold) // white
        //                            {
        //                                R = 255;
        //                                G = 255;
        //                                B = 255;
        //                            }
        //                            else // value is between low and high
        //                            {
        //                                float range = WaterfallHighThreshold - low_threshold;
        //                                float offset = waterfall_data[i] - low_threshold;
        //                                float local_percent = ((100.0f * offset) / range);
        //                                R = (int)((local_percent / 100) * 255);
        //                                G = R;
        //                                B = R;
        //                            }

        //                            if (waterfall_minimum > waterfall_data[i]) //wfagc
        //                                waterfall_minimum = waterfall_data[i];

        //                            // set pixel color
        //                            row[i * pixel_size + 0] = (byte)B;	// set color in memory
        //                            row[i * pixel_size + 1] = (byte)G;
        //                            row[i * pixel_size + 2] = (byte)R;
        //                        }
        //                    }
        //                    break;

        //                case (ColorScheme.LinLog):
        //                    {
        //                        for (int i = 0; i < W; i++)	// for each pixel in the new line
        //                        {
        //                            if (waterfall_data[i] <= low_threshold)
        //                            {
        //                                R = 0;
        //                                G = 0;
        //                                B = 0;
        //                            }
        //                            else if (waterfall_data[i] >= high_threshold)
        //                            {
        //                                R = 252;
        //                                G = 252;
        //                                B = 252;
        //                            }
        //                            else // value is between low and high
        //                            {
        //                                float range = high_threshold - low_threshold;
        //                                float offset = waterfall_data[i] - low_threshold + LinLogCor;
        //                                float spec_bits = 1024;
        //                                float overall_percent = (spec_bits * offset) / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.
        //                                float log_fract = (float)(Math.Log10(spec_bits));
        //                                if (overall_percent == 0)
        //                                {
        //                                    overall_percent = (float)0.001;
        //                                }
        //                                overall_percent = (float)(Math.Log10(overall_percent));

        //                                if (overall_percent < log_fract / 23)
        //                                {
        //                                    //			float local_percent = overall_percent / ((float)1/23);
        //                                    R = 0;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)2 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)1/23) / ((float)1/23);
        //                                    R = 32;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)3 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)2/23) / ((float)1/23);
        //                                    R = 64;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)4 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)3/23) / ((float)1/23);
        //                                    R = 96;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)5 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)4/23) / ((float)1/23);
        //                                    R = 104;
        //                                    G = 40;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)6 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)5/23) / ((float)1/23);
        //                                    R = 112;
        //                                    G = 60;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)7 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)6/23) / ((float)1/23);
        //                                    R = 116;
        //                                    G = 88;
        //                                    B = 0;
        //                                }


        //                                else if (overall_percent < (float)8 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)7/23) / ((float)1/23);
        //                                    R = 92;
        //                                    G = 112;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)9 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)8/23) / ((float)1/23);
        //                                    R = 80;
        //                                    G = 132;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)10 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)9/23) / ((float)1/23);
        //                                    R = 20;
        //                                    G = 140;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)11 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)10/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 160;
        //                                    B = 40;
        //                                }
        //                                else if (overall_percent < (float)12 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)11/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 160;
        //                                    B = 120;
        //                                }

        //                                else if (overall_percent < (float)13 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)12/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 140;
        //                                    B = 148;
        //                                }
        //                                else if (overall_percent < (float)14 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)13/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 132;
        //                                    B = 192;
        //                                }
        //                                else if (overall_percent < (float)15 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)14/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 112;
        //                                    B = 200;
        //                                }
        //                                else if (overall_percent < (float)16 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)15/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 88;
        //                                    B = 208;
        //                                }
        //                                else if (overall_percent < (float)17 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)16/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 60;
        //                                    B = 232;
        //                                }
        //                                else if (overall_percent < (float)18 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)17/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 40;
        //                                    B = 252;
        //                                }
        //                                else if (overall_percent < (float)19 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)18/23) / ((float)1/23);
        //                                    R = 80;
        //                                    G = 80;
        //                                    B = 252;
        //                                }

        //                                else if (overall_percent < (float)20 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)19/23) / ((float)1/23);
        //                                    R = 124;
        //                                    G = 124;
        //                                    B = 252;
        //                                }

        //                                else if (overall_percent < (float)21 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)20/23) / ((float)1/23);
        //                                    R = 172;
        //                                    G = 172;
        //                                    B = 252;
        //                                }

        //                                else if (overall_percent >= (float)21 * log_fract / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)22/23) / ((float)1/23);
        //                                    R = 252;
        //                                    G = 252;
        //                                    B = 252;
        //                                }
        //                                else
        //                                {
        //                                    R = 0;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                            }

        //                            if (waterfall_minimum > waterfall_data[i]) //wfagc
        //                                waterfall_minimum = waterfall_data[i];

        //                            // set pixel color changed by w3sz

        //                            row[i * pixel_size + 0] = (byte)R;	// set color in memory
        //                            row[i * pixel_size + 1] = (byte)G;
        //                            row[i * pixel_size + 2] = (byte)B;
        //                        }
        //                    }
        //                    break;

        //                //  now Linrad palette without log

        //                case (ColorScheme.LinRad):
        //                    {
        //                        for (int i = 0; i < W; i++)	// for each pixel in the new line
        //                        {
        //                            if (waterfall_data[i] <= low_threshold)
        //                            {
        //                                R = 0;
        //                                G = 0;
        //                                B = 0;
        //                            }
        //                            else if (waterfall_data[i] >= high_threshold)
        //                            {
        //                                R = 252;
        //                                G = 252;
        //                                B = 252;
        //                            }
        //                            else // value is between low and high
        //                            {
        //                                float range = high_threshold - low_threshold;
        //                                float offset = waterfall_data[i] - low_threshold + LinCor;
        //                                float overall_percent = (offset) / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.


        //                                if (overall_percent < (float)1 / 23)
        //                                {
        //                                    //			float local_percent = overall_percent / ((float)1/23);
        //                                    R = 0;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)2 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)1/23) / ((float)1/23);
        //                                    R = 32;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)3 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)2/23) / ((float)1/23);
        //                                    R = 64;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)4 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)3/23) / ((float)1/23);
        //                                    R = 96;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)5 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)4/23) / ((float)1/23);
        //                                    R = 104;
        //                                    G = 40;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)6 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)5/23) / ((float)1/23);
        //                                    R = 112;
        //                                    G = 60;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)7 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)6/23) / ((float)1/23);
        //                                    R = 116;
        //                                    G = 88;
        //                                    B = 0;
        //                                }


        //                                else if (overall_percent < (float)8 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)7/23) / ((float)1/23);
        //                                    R = 92;
        //                                    G = 112;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)9 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)8/23) / ((float)1/23);
        //                                    R = 80;
        //                                    G = 132;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)10 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)9/23) / ((float)1/23);
        //                                    R = 20;
        //                                    G = 140;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)11 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)10/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 160;
        //                                    B = 40;
        //                                }
        //                                else if (overall_percent < (float)12 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)11/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 160;
        //                                    B = 120;
        //                                }

        //                                else if (overall_percent < (float)13 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)12/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 140;
        //                                    B = 148;
        //                                }
        //                                else if (overall_percent < (float)14 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)13/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 132;
        //                                    B = 192;
        //                                }
        //                                else if (overall_percent < (float)15 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)14/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 112;
        //                                    B = 200;
        //                                }
        //                                else if (overall_percent < (float)16 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)15/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 88;
        //                                    B = 208;
        //                                }
        //                                else if (overall_percent < (float)17 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)16/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 60;
        //                                    B = 232;
        //                                }
        //                                else if (overall_percent < (float)18 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)17/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 40;
        //                                    B = 252;
        //                                }
        //                                else if (overall_percent < (float)19 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)18/23) / ((float)1/23);
        //                                    R = 80;
        //                                    G = 80;
        //                                    B = 252;
        //                                }

        //                                else if (overall_percent < (float)20 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)19/23) / ((float)1/23);
        //                                    R = 124;
        //                                    G = 124;
        //                                    B = 252;
        //                                }

        //                                else if (overall_percent < (float)21 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)20/23) / ((float)1/23);
        //                                    R = 172;
        //                                    G = 172;
        //                                    B = 252;
        //                                }

        //                                else if (overall_percent >= (float)21 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)22/23) / ((float)1/23);
        //                                    R = 252;
        //                                    G = 252;
        //                                    B = 252;
        //                                }
        //                                else
        //                                {
        //                                    R = 0;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                            }

        //                            if (waterfall_minimum > waterfall_data[i]) //wfagc
        //                                waterfall_minimum = waterfall_data[i];

        //                            // set pixel color changed by w3sz
        //                            row[i * pixel_size + 0] = (byte)R;	// set color in memory
        //                            row[i * pixel_size + 1] = (byte)G;
        //                            row[i * pixel_size + 2] = (byte)B;
        //                        }
        //                    }
        //                    break;

        //                //  now Linrad palette without log

        //                case (ColorScheme.LinAuto):
        //                    {
        //                        for (int i = 0; i < W; i++)	// for each pixel in the new line
        //                        {
        //                            display_min_w3sz = min_y_w3sz - 5; //w3sz for histogram equilization
        //                            display_max_w3sz = max_y; //w3sz for histogram equalization
        //                            //	if (i == (W - 1))
        //                            //	{
        //                            //		System.Console.WriteLine("Low is" + low_threshold + "   " + display_min_w3sz);
        //                            //	
        //                            //		System.Console.WriteLine("High is" + high_threshold + "   " + max_y);
        //                            //}	

        //                            if (waterfall_data[i] <= display_min_w3sz)
        //                            {
        //                                R = 0;
        //                                G = 0;
        //                                B = 0;
        //                            }
        //                            else if (waterfall_data[i] >= display_max_w3sz)
        //                            {
        //                                R = 252;
        //                                G = 252;
        //                                B = 252;
        //                            }
        //                            else // value is between low and high
        //                            {
        //                                float range = display_max_w3sz - display_min_w3sz;
        //                                float offset = waterfall_data[i] - display_min_w3sz; // + GlobalClass.LinCor;
        //                                float overall_percent = (offset) / range; // value from 0.0 to 1.0 where 1.0 is high and 0.0 is low.


        //                                if (overall_percent < (float)1 / 23)
        //                                {
        //                                    //			float local_percent = overall_percent / ((float)1/23);
        //                                    R = 0;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)2 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)1/23) / ((float)1/23);
        //                                    R = 32;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)3 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)2/23) / ((float)1/23);
        //                                    R = 64;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)4 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)3/23) / ((float)1/23);
        //                                    R = 96;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)5 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)4/23) / ((float)1/23);
        //                                    R = 104;
        //                                    G = 40;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)6 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)5/23) / ((float)1/23);
        //                                    R = 112;
        //                                    G = 60;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)7 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)6/23) / ((float)1/23);
        //                                    R = 116;
        //                                    G = 88;
        //                                    B = 0;
        //                                }


        //                                else if (overall_percent < (float)8 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)7/23) / ((float)1/23);
        //                                    R = 92;
        //                                    G = 112;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)9 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)8/23) / ((float)1/23);
        //                                    R = 80;
        //                                    G = 132;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)10 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)9/23) / ((float)1/23);
        //                                    R = 20;
        //                                    G = 140;
        //                                    B = 0;
        //                                }
        //                                else if (overall_percent < (float)11 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)10/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 160;
        //                                    B = 40;
        //                                }
        //                                else if (overall_percent < (float)12 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)11/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 160;
        //                                    B = 120;
        //                                }

        //                                else if (overall_percent < (float)13 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)12/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 140;
        //                                    B = 148;
        //                                }
        //                                else if (overall_percent < (float)14 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)13/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 132;
        //                                    B = 192;
        //                                }
        //                                else if (overall_percent < (float)15 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)14/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 112;
        //                                    B = 200;
        //                                }
        //                                else if (overall_percent < (float)16 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)15/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 88;
        //                                    B = 208;
        //                                }
        //                                else if (overall_percent < (float)17 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)16/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 60;
        //                                    B = 232;
        //                                }
        //                                else if (overall_percent < (float)18 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)17/23) / ((float)1/23);
        //                                    R = 0;
        //                                    G = 40;
        //                                    B = 252;
        //                                }
        //                                else if (overall_percent < (float)19 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)18/23) / ((float)1/23);
        //                                    R = 80;
        //                                    G = 80;
        //                                    B = 252;
        //                                }

        //                                else if (overall_percent < (float)20 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)19/23) / ((float)1/23);
        //                                    R = 124;
        //                                    G = 124;
        //                                    B = 252;
        //                                }

        //                                else if (overall_percent < (float)21 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)20/23) / ((float)1/23);
        //                                    R = 172;
        //                                    G = 172;
        //                                    B = 252;
        //                                }

        //                                else if (overall_percent >= (float)21 / 23)
        //                                {
        //                                    //			float local_percent = (overall_percent - (float)22/23) / ((float)1/23);
        //                                    R = 252;
        //                                    G = 252;
        //                                    B = 252;
        //                                }
        //                                else
        //                                {
        //                                    R = 0;
        //                                    G = 0;
        //                                    B = 0;
        //                                }
        //                            }

        //                            // set pixel color changed by w3sz
        //                            row[i * pixel_size + 0] = (byte)R;	// set color in memory
        //                            row[i * pixel_size + 1] = (byte)G;
        //                            row[i * pixel_size + 2] = (byte)B;
        //                        }
        //                    }
        //                    break;
        //            }

        //            waterfall_bmp.UnlockBits(bitmapData);

        //            // if (rx == 1)
        //            waterfallPreviousMinValue = (((waterfallPreviousMinValue * 8) + (waterfall_minimum * 2)) / 10) + 1; //wfagc
        //            // else
        //            //  RX2waterfallPreviousMinValue = ((RX2waterfallPreviousMinValue * 8) + (waterfall_minimum * 2)) / 10 + 1; //wfagc
        //        }

        //        g.DrawImageUnscaled(waterfall_bmp, 0, waterfallRect.Top);	// draw the image on the background	
        //    }
        //    waterfall_counter++;

        //    // draw long cursor
        //    if (current_click_tune_mode != ClickTuneMode.Off)
        //    {
        //        Pen p = current_click_tune_mode == ClickTuneMode.VFOA ? grid_text_pen : Pens.Red;

        //        if (display_cursor_y <= H)
        //        {
        //            g.DrawLine(p, display_cursor_x, 0, display_cursor_x, H);
        //            if (ShowCTHLine) g.DrawLine(p, 0, display_cursor_y, W, display_cursor_y);
        //        }

        //    }

        //    return true;
        //}

        //public void ResetRX1DisplayAverage()
        //{
        //    if (rx1_average_buffer != null)
        //        rx1_average_buffer[0] = CLEAR_FLAG;	// set reset flag
        //}

        //public void ResetRX2DisplayAverage()
        //{
        //    if (rx2_average_buffer != null)
        //        rx2_average_buffer[0] = CLEAR_FLAG;	// set reset flag
        //}

        //public void ResetRX1DisplayPeak()
        //{
        //    if (rx1_peak_buffer != null)
        //        rx1_peak_buffer[0] = CLEAR_FLAG; // set reset flag
        //}

        //public void ResetRX2DisplayPeak()
        //{
        //    if (rx2_peak_buffer != null)
        //        rx2_peak_buffer[0] = CLEAR_FLAG; // set reset flag
        //}

        private Rectangle freqScalePanRect;
        public Rectangle FreqScalePanRect
        {
            get { return freqScalePanRect; }
            set { freqScalePanRect = value; }
        }

        private Rectangle panRect;
        public Rectangle PanRect
        {
            get { return panRect; }
            set { panRect = value; }
        }

        private Rectangle waterfallRect;
        public Rectangle WaterfallRect {
            get { return waterfallRect; }
            set { waterfallRect = value; }
        }

        private Rectangle dBmScalePanRect;
        public Rectangle DBMScalePanRect
        {
            get { return dBmScalePanRect; }
            set { dBmScalePanRect = value; }
        }

        private Rectangle secScaleWaterfallRect;
        public Rectangle SecScalePanRect {
            get { return secScaleWaterfallRect; }
            set { secScaleWaterfallRect = value; }
        }

        private int displayTop;

        public void CreateDisplayRegions()
        {
            int width = this.Width; //target.Width;
            int height = this.Height; //target.Height;

            displayTop = 0;
            int freqScaleRectHeight = 20;

            freqScalePanRect = new Rectangle(
                    0,
                    displayTop +
                    (int)Math.Round((height - displayTop - freqScaleRectHeight) * freq_ruler_position),
                    width,
                    freqScaleRectHeight);

            panRect = new Rectangle(
                    0,
                    displayTop,
                    width,
                    freqScalePanRect.Top - displayTop);

            //waterfallRect = new Rectangle(
            //        freqScalePanRect.Left,
            //        freqScalePanRect.Top + freqScalePanRect.Height,
            //        freqScalePanRect.Width,
            //        height - freqScalePanRect.Top - freqScalePanRect.Height);

            dBmScalePanRect = new Rectangle(
                    panRect.Right - 35,
                    displayTop,
                    35,
                    panRect.Height);

            //secScaleWaterfallRect = new Rectangle(
            //        waterfallRect.Left,
            //        freqScalePanRect.Top + freqScalePanRect.Height,
            //        45,
            //        waterfallRect.Height);

            //if (waterfallRect.Height == 0) waterfallRect.Height = 1;
            //waterfall_bmp = new Bitmap(waterfallRect.Width, waterfallRect.Height, PixelFormat.Format24bppRgb);	// initialize waterfall display
            // panadapter_bmp = new Bitmap(panRect.Width, panRect.Height, PixelFormat.Format24bppRgb);	// initialize waterfall display
        }

        private int snapMouse = 3;
        public DisplayRegion mouseRegion;

        private bool rx1_low_filter_drag = false;
        private bool rx1_high_filter_drag = false;
        private bool rx1_whole_filter_drag = false;
        private bool rx1_sub_drag = false;
        private bool rx1_spectrum_drag = false;

        private int whole_filter_start_x = 0;
        private int whole_filter_start_low = 0;
        private int whole_filter_start_high = 0;
        private int sub_drag_last_x = 0;
        private int spectrum_drag_last_x = 0;
        private double sub_drag_start_freq = 0.0;

        private bool rx1_click_tune_drag = false;
        private bool rx2_click_tune_drag = false;

        private Point grid_minmax_drag_start_point = new Point(0, 0);
        //  private int grid_minmax_drag_max_delta_x = 0;
        private decimal grid_minmax_max_y = 0;
        private decimal grid_minmax_min_y = 0;
        // private Cursor grab = new Cursor(msgrab);
        // private Cursor grabbing = new Cursor(msgrabbing);
        private bool moveX = false;
        private bool moveY = false;

        private bool rx1_grid_adjust = false;
        private bool gridmaxadjust = false;
        private bool wfmaxadjust = false;
        private bool wfminadjust = false;
        private bool gridminmaxadjust = false;

        private void getRegion(Point p)
        {
            //if (this.FreqScalePanRect.Contains(p))
            //{
            //    mouseRegion = DisplayRegion.freqScalePanadapterRegion;
            //}
            //else 
            if (this.DBMScalePanRect.Contains(p))
            {
                mouseRegion = DisplayRegion.dBmScalePanadapterRegion;
            }
            //else if (Math.Abs(p.X - this.FilterRect.Left) < snapMouse && this.PanRect.Contains(p))
            //{
            //    mouseRegion = DisplayRegion.filterRegionLow;
            //    // mouseDownFilterFrequencyLo = filterLowerFrequency;
            //}
            //else if (Math.Abs(p.X - this.FilterRect.Right) < snapMouse && this.PanRect.Contains(p))
            //{
            //    mouseRegion = DisplayRegion.filterRegionHigh;
            //    //  mouseDownFilterFrequencyHi = filterUpperFrequency;
            //}
            //else if (this.FilterRect.Contains(p))
            //{
            //    mouseRegion = DisplayRegion.filterRegion;

            //    //    if (!wholeFilterDrag)
            //    //    {
            //    //        mouseDownFilterFrequencyLo = filterLowerFrequency;
            //    //        mouseDownFilterFrequencyHi = filterUpperFrequency;
            //    //    }
            //}
            //else if (this.PanRect.Contains(p))
            //{
            //    mouseRegion = DisplayRegion.panadapterRegion;
            //}
            //else if (this.WaterfallRect.Contains(p))
            //{
            //    mouseRegion = DisplayRegion.waterfallRegion;
            //}
            else
                mouseRegion = DisplayRegion.elsewhere;

        }

        public void UpdateGraphicsBuffer()
        {
            int w, h;
            w = Math.Max(1, this.Width);
            h = Math.Max(1, this.Height);

            wbDisplay_buffer = new Bitmap(w, h); 
        }

        private CancellationTokenSource cancelTokenSource;
        public void Cancel_Display()
        {
            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
                cancelTokenSource.Dispose();
                cancelTokenSource = null;
            }

           // Halted = true;

        }

        private readonly Object m_objBufferLock = new Object();

        private Thread draw_display_thread;
        public void StartDisplay(int rx)
        {
            //Halted = false;

            //if (draw_display_thread == null || !draw_display_thread.IsAlive)
            //{
            //    draw_display_thread = new Thread(() => RunDisplay(rx));
            //    draw_display_thread.Priority = ThreadPriority.BelowNormal;
            //    draw_display_thread.IsBackground = true;
            //    draw_display_thread.Start();
            //}

            if (cancelTokenSource != null) Cancel_Display();
            cancelTokenSource = new CancellationTokenSource();
            UpdateGraphicsBuffer();

            draw_display_task = Task.Factory.StartNew(() =>
            {
                while (cancelTokenSource != null && !cancelTokenSource.IsCancellationRequested)
                {

                    RunDisplay(rx); // get pixels

                    if (!pauseDisplayThread)
                    {
                        using (Graphics grSrc = Graphics.FromImage(wbDisplay_buffer))
                        {
                            grSrc.Clear(Color.Transparent);
                            if (DrawWideBand(grSrc, rx))
                            {
                                this.Invoke(new Action(() =>
                                    {
                                        this.Image = wbDisplay_buffer;
                                        this.Refresh();
                                    }));
                            }
                        }
                    }

                }

            }, cancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        }

        bool Halted = false;
        static readonly object wbMonitor = new object();
        unsafe private void RunDisplay(int rx)
        {
            int flag;

           // UpdateGraphicsBuffer();

          //  while (!Halted)
          //  {
                if (!this.DataReady)
                {
                    flag = 0;

                    fixed (float* ptr = &new_display_data[0])
                        SpecHPSDRDLL.GetPixels(rx, 0, ptr, ref flag);

                    this.DataReady = true;
                }

            //Thread.Sleep(70);
            Thread.Sleep(16);

            //    if (!pauseDisplayThread)
            //    {
            //        Monitor.Enter(wbMonitor);
            //        try
            //        {
            //            using (Graphics grSrc = Graphics.FromImage(wbDisplay_buffer))
            //            {
            //                grSrc.Clear(Color.Transparent);
            //                if (DrawWideBand(grSrc, rx))
            //                {
            //                    //  this.Invoke(new Action(() =>
            //                    //  {
            //                    this.Image = wbDisplay_buffer;
            //                    this.Refresh();
            //                    // }));
            //                }
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            System.Console.WriteLine(ex.Message);
            //        }

            //        finally
            //        {
            //            Monitor.Exit(wbMonitor);
            //        }

            //    }

            //}
        }

        private Point mousePos;
        private Point mouseDownPos;
        private Point rulerMouseDownPos;

        private void PanDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            Size pos = new Size(-1, -1);
            pos = new Size(e.X, e.Y);
            mousePos = new Point(pos);

            if (e.Button != MouseButtons.Left &&
                e.Button != MouseButtons.Right &&
                e.Button != MouseButtons.Middle)
                getRegion(mousePos);

            // Cursor next_cursor = null;

            switch (mouseRegion)
            {
                case DisplayRegion.freqScalePanadapterRegion:
                    if (e.Button == MouseButtons.Left)
                    {
                        Point dPos = Point.Subtract(mouseDownPos, pos);

                        if (dPos.Y != 0 && !moveY && !moveX)
                        {
                            moveY = true;
                            moveX = false;
                        }

                        if (moveY)
                        {
                            int bottom_y = (int)(this.Height - this.FreqScalePanRect.Height);
                            int new_y = (int)(rulerMouseDownPos.Y - dPos.Y);

                            if (new_y < (int)(this.PanRect.Top))// + picDisplay.panSpectrumMinimumHeight))
                                new_y = (int)(this.PanRect.Top);// + picDisplay.panSpectrumMinimumHeight);
                            if (new_y > bottom_y)
                                new_y = bottom_y;

                            FreqRulerPosition = (float)(new_y - this.PanRect.Top) / (bottom_y - this.PanRect.Top);
                            // picDisplay.Init();
                            //picDisplay.CreateDisplayRegions();
                        }
                        //  }
                        //  else if (e.Button == System.Windows.Forms.MouseButtons.Left)
                        // {
                        //  Point dPos = Point.Subtract(m_mouseDownPos, pos);
                        if (dPos.X != 0 && !moveY && !moveX)
                        {
                            moveX = true;
                            moveY = false;
                        }

                        if (moveX)
                        {
                            int displaySpan = HighFreq - LowFreq;
                            double unit = (double)(displaySpan / (double)FreqScalePanRect.Width);
                            double deltaFreq = unit * dPos.X;

                            //if (rx1_spectrum_tune_drag)
                            //{
                            //    // if (!mox || (rx2_enabled && chkVFOBTX.Checked))
                            //    // {
                            //    float start_freq = PixelToHz(spectrum_drag_last_x);
                            //    float end_freq = PixelToHz(e.X);
                            //    spectrum_drag_last_x = e.X;
                            //    float delta = end_freq - start_freq;
                            //    CenterFrequency -= delta * 0.0000010;
                            //    txtVFOAFreq_LostFocus(this, EventArgs.Empty);
                            //    // }
                            //}
                            //else
                            // VFOFreq += deltaFreq * 0.0000010;
                            // cmaster.Getrxa(adc).RXFreq += (long)deltaFreq;
                            // cmaster.Getrxa(display_id + 2).RXFreq += (deltaFreq * 0.0000010);
                            //VFOHz += (long)deltaFreq;
                            //JanusAudio.SetVFOfreq(display_id+2, JanusAudio.Freq2PW((int)VFOHz), 0);

                            if (dPos.X > 0)
                                PanSlider += 0.001;
                            else if (dPos.X < 0)
                                PanSlider -= 0.001;

                            mouseDownPos = mousePos;
                        }
                    }

                    if (e.Button == MouseButtons.Right)
                    {
                        Point dPos = (Point)Point.Subtract(mouseDownPos, pos);
                        if (dPos.X > 0)
                            ZoomSlider += 0.001;
                        else if (dPos.X < 0)
                            ZoomSlider -= 0.001;

                        // if (freqScaleZoomFactor > 1.0) freqScaleZoomFactor = 1.0;
                        // if (freqScaleZoomFactor < 0.05) freqScaleZoomFactor = 0.05;

                        //  specRX.GetSpecRX(0).ZoomSlider = freqScaleZoomFactor;
                        // rx1_zoom_factor_by_band[(int)rx1_band] = rx1_zoom_factor;

                        mouseDownPos = mousePos;
                        //CalcDisplayFreq();
                    }

                    break;

                case DisplayRegion.dBmScalePanadapterRegion:

                    if (gridminmaxadjust)
                    {
                        int delta_y = e.Y - grid_minmax_drag_start_point.Y;
                        double delta_db = (delta_y / 10) * 5;
                        decimal val = grid_minmax_max_y;
                        val += (decimal)delta_db;
                        decimal min_val = grid_minmax_min_y;
                        min_val += (decimal)delta_db;

                        if (val > 200) val = 200;
                        if (min_val < -200) min_val = -200;

                        SpectrumGridMax = (int)val;
                        SpectrumGridMin = (int)min_val;
                    }

                    if (gridmaxadjust)
                    {
                        int delta_y = e.Y - grid_minmax_drag_start_point.Y;
                        double delta_db = (delta_y / 10) * 5;
                        decimal val = grid_minmax_max_y;
                        val += (decimal)delta_db;

                        if (val > 200) val = 200;

                        SpectrumGridMax = (int)val;
                    }
                    break;
                case DisplayRegion.filterRegionLow:
                    // next_cursor = Cursors.SizeWE;
                    //  m_showFilterLeftBoundary = true;
                    if (e.Button == MouseButtons.Left)
                    {
                        // Point dPos = Point.Subtract(mouseDownPos, pos);
                        // double dFreq = (double)(dPos.X * displaySpan) / (double)panRect.Width;

                        // filterLowerFrequency = Math.Round(mouseDownFilterFrequencyLo - dFreq);
                        //settings.setRXFilter(this, receiver, filterLowerFrequency, filterUpperFrequency);
                        // SelectRX1VarFilter();
                        // int new_low = (int)Math.Min(PixelToHz(e.X), radio.GetDSPRX(0, 0).RXFilterHigh - 10);
                        // UpdateRX1Filters(new_low, radio.GetDSPRX(0, 0).RXFilterHigh);
                    }

                    // m_highlightFilter = false;
                    break;
                case DisplayRegion.filterRegionHigh:
                    // next_cursor = Cursors.SizeWE;
                    if (e.Button == MouseButtons.Left)
                    {
                        // SelectRX1VarFilter();
                        // int new_high = (int)Math.Max(PixelToHz(e.X), radio.GetDSPRX(0, 0).RXFilterLow + 10);
                        //UpdateRX1Filters(radio.GetDSPRX(0, 0).RXFilterLow, new_high);
                    }
                    break;
                case DisplayRegion.filterRegion:
                    // next_cursor = Cursors.NoMoveHoriz;
                    if (e.Button == MouseButtons.Left)
                    {
                        // SelectRX1VarFilter();
                        // int diff = (int)(PixelToHz(e.X) - PixelToHz(whole_filter_start_x));
                        //UpdateRX1Filters(whole_filter_start_low + diff, whole_filter_start_high + diff);
                    }
                    break;

            }
        }

        private void PanDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            Point pos = new Point(-1, -1); // = e.GetPosition(console.openGLRX1);
            pos = new Point(e.X, e.Y);

            mousePos = pos;
            mouseDownPos = mousePos;

            getRegion(mousePos);

            if (e.Button == MouseButtons.Left)
            {
                //switch (picDisplay.CurrentDisplayMode)
                //{
                //    case DisplayMode.SPECTRUM:
                //    case DisplayMode.PANADAPTER:
                //    case DisplayMode.WATERFALL:
                //    case DisplayMode.PANAFALL:
                //    case DisplayMode.PANASCOPE:
                //    case DisplayMode.HISTOGRAM:
                rx1_low_filter_drag = false;
                rx1_high_filter_drag = false;
                rx1_whole_filter_drag = false;
                // rx2_low_filter_drag = false;
                // rx2_high_filter_drag = false;
                // rx2_whole_filter_drag = false;
                //tx_low_filter_drag = false;
                //tx_high_filter_drag = false;
                //tx_whole_filter_drag = false;
                //rx1_click_tune_drag = false;
                //rx1_spectrum_tune_drag = false;

                //agc_knee_drag = false;
                //agc_hang_drag = false;
                // agc_knee_drag_max_delta_x = 0;
                // agc_knee_drag_max_delta_y = 0;
                gridminmaxadjust = false;
                rx1_grid_adjust = false;
                // rx2_grid_adjust = false;
                // tx1_grid_adjust = false;
                // tx2_grid_adjust = false;

                // grid_minmax_drag_max_delta_y = 0;

                // notch_drag = false;
                // notch_drag_max_delta_x = 0;
                //  notch_drag_max_delta_y = 0;
                // timerNotchZoom.Enabled = false;
                // notch_zoom = false;
                // if (picDisplay.TNFZoom)
                // {
                //    picDisplay.TNFZoom = false;
                //  }
                // stop showing details for this notch in the panadapter
                //  if (notch_drag_active != null)
                //  {
                //   notch_drag_active.Details = false;
                //   notch_drag_active = null;
                // }
                //rx2_sub_drag = false;

                moveX = false;
                moveY = false;

                // break;
                //}

                if (rx1_sub_drag)
                {
                    rx1_sub_drag = false;
                    // if (rx2_enabled) txtVFOABand_LostFocus(this, EventArgs.Empty);
                    // else 
                    // txtVFOBFreq_LostFocus(this, EventArgs.Empty);
                }

                if (rx1_spectrum_drag)
                {
                    rx1_spectrum_drag = false;
                    // txtVFOAFreq_LostFocus(this, EventArgs.Empty);
                }
                // rx2_spectrum_drag = false;
                //Cursor = Cursors.Default;
            }

            if (e.Button == MouseButtons.Right)
            {
                //switch (picDisplay.CurrentDisplayMode)
                //{
                //    case DisplayMode.PANADAPTER:
                //    case DisplayMode.PANAFALL:
                //    case DisplayMode.HISTOGRAM:
                //    case DisplayMode.SPECTRUM:
                gridminmaxadjust = false;
                gridmaxadjust = false;
                rx1_grid_adjust = false;
                // rx2_grid_adjust = false;
                // tx1_grid_adjust = false;
                // tx2_grid_adjust = false;
                // break;
                //}
            }

        }

        //private void picDisplay_DoubleClick(object sender, EventArgs e)
        //{
        //    int new_val = (int)PixelToDb(display_cursor_y);
        //    if (!(rx1_grid_adjust || gridmaxadjust))
        //    {
        //        if (!mox) //RX1
        //        {
        //            if (rx1_dsp_mode == DSPMode.FM)
        //                return;

        //            if (new_val > ptbSquelch.Maximum) new_val = ptbSquelch.Maximum;
        //            if (new_val < ptbSquelch.Minimum) new_val = ptbSquelch.Minimum;
        //            ptbSquelch.Value = new_val;
        //            ptbSquelch_Scroll(this, EventArgs.Empty);
        //        }
        //        else // TX
        //        {
        //            new_val += 24;
        //            if (new_val > ptbNoiseGate.Maximum) new_val = ptbNoiseGate.Maximum;
        //            if (new_val < ptbNoiseGate.Minimum) new_val = ptbNoiseGate.Minimum;
        //            ptbNoiseGate.Value = new_val;
        //            ptbNoiseGate_Scroll(this, EventArgs.Empty);
        //        }
        //    }

        //}

        private void PanDisplay_MouseEnter(object sender, EventArgs e)
        {
            if (!this.Focused)
                this.Focus();
        }

        private void PanDisplay_MouseWheel(object sender, MouseEventArgs e)
        {
            //			if(this.ActiveControl is TextBoxTS && this.ActiveControl != txtVFOAFreq
            //				&& this.ActiveControl != txtVFOBFreq) return;
            //			if(this.ActiveControl is NumericUpDownTS) return;
            //if (console.ActiveControl is TextBoxTS ||
            //    console.ActiveControl is NumericUpDownTS ||
            //    console.ActiveControl is TrackBarTS)
            //{
            //    console.Console_KeyPress(this, new KeyPressEventArgs((char)Keys.Enter));
            //    return;
            //}

            if (e.Delta == 0) return;

            //int num_steps = (e.Delta > 0 ? 1 : -1);	// 1 per click
            ////int numberToMove = e.Delta / 120;	// 1 per click

            //if (console.vfo_char_width == 0)
            //    console.GetVFOCharWidth();

            //if (num_steps == 0) return;
            //int step = console.CurrentTuneStepHz;
            //if (console.shift_down && step >= 10) step /= 10;

            //   if (current_click_tune_mode == ClickTuneMode.VFOB && wheel_tunes_vfob)
            //  {
            //  if (rx2_enabled && chkVFOSplit.Checked)
            //    VFOASubFreq = SnapTune(VFOASubFreq, step, num_steps);
            // else
            //    VFOBFreq = SnapTune(VFOBFreq, step, num_steps);
            // }
            // else 

            //switch (receiver)
            //{
            //    case 0:
            //        console.VFOAFreq = console.SnapTune(console.VFOAFreq, step, num_steps);
            //        break;
            //    case 1:
            //        console.VFOBFreq = console.SnapTune(console.VFOBFreq, step, num_steps);
            //        break;
            //}


            //switch (TuneHitTest(e.X, e.Y))
            //{
            //    case TuneLocation.VFOA:
            //        double freq = double.Parse(txtVFOAFreq.Text);
            //        double mult = 1000.0;
            //        int right = grpVFOA.Left + txtVFOAFreq.Left + txtVFOAFreq.Width;
            //        if (vfoa_hover_digit < 0)
            //        {
            //            int x = right + 2 - (vfo_pixel_offset - 5);
            //            while (x < e.X && mult > 0.0000011)
            //            {
            //                mult /= 10.0;
            //                x += vfo_char_width;
            //                if (mult == 1.0)
            //                    x += vfo_decimal_space;
            //                else x += vfo_char_space;
            //            }
            //        }
            //        else
            //        {
            //            mult = Math.Pow(10, -vfoa_hover_digit) * 1000.0;
            //        }

            //        if (mult <= 1.0)
            //        {
            //            freq += mult * num_steps;
            //            //Debug.WriteLine("freq: "+freq.ToString("f6"));
            //            VFOAFreq = freq;
            //        }
            //        break;

            //    case TuneLocation.VFOB:
            //        freq = double.Parse(txtVFOBFreq.Text);
            //        mult = 1000.0;
            //        right = grpVFOB.Left + txtVFOBFreq.Left + txtVFOBFreq.Width;
            //        if (vfob_hover_digit < 0)
            //        {
            //            int x = right + 2 - (vfo_pixel_offset - 5);
            //            while (x < e.X && mult > 0.0000011)
            //            {
            //                mult /= 10;
            //                x += vfo_char_width;
            //                if (mult == 1.0)
            //                    x += vfo_decimal_space;
            //                else x += vfo_char_space;
            //            }
            //        }
            //        else
            //        {
            //            mult = Math.Pow(10, -vfob_hover_digit) * 1000.0;
            //        }

            //        if (mult <= 1.0)
            //        {
            //            freq += mult * num_steps;
            //            VFOBFreq = freq;
            //        }
            //        break;

            //    case TuneLocation.VFOASub:
            //        if (rx2_enabled && (chkEnableMultiRX.Checked || chkVFOSplit.Checked))
            //        {
            //            freq = VFOASubFreq;
            //            mult = 1000.0;
            //            right = grpVFOA.Left + txtVFOABand.Left + txtVFOABand.Width;
            //            if (vfoa_sub_hover_digit < 0)
            //            {
            //                int x = right + 2 - (vfo_sub_pixel_offset - 5);
            //                while (x < e.X && mult > 0.0000011)
            //                {
            //                    mult /= 10;
            //                    x += vfo_sub_char_width;
            //                    if (mult == 1.0)
            //                        x += vfo_sub_decimal_space;
            //                    else x += vfo_sub_char_space;
            //                }
            //            }
            //            else
            //            {
            //                mult = Math.Pow(10, -vfoa_sub_hover_digit) * 1000.0;
            //            }

            //            if (mult <= 1.0)
            //            {
            //                freq += mult * num_steps;
            //                VFOASubFreq = freq;
            //            }
            //        }
            //        else
            //        {
            //            VFOAFreq = SnapTune(VFOAFreq, step, num_steps);
            //        }
            //        break;

            //    case TuneLocation.DisplayBottom:
            //        //if (rx2_enabled && chkVFOSplit.Checked && current_click_tune_mode == ClickTuneMode.VFOB && wheel_tunes_vfob)
            //        //    VFOASubFreq = SnapTune(VFOASubFreq, step, num_steps);
            //        //else if (rx2_enabled || (current_click_tune_mode == ClickTuneMode.VFOB && wheel_tunes_vfob))
            //        //    VFOBFreq = SnapTune(VFOBFreq, step, num_steps);
            //        //else
            //        //    VFOAFreq = SnapTune(VFOAFreq, step, num_steps);
            //        break;

            //    case TuneLocation.Other:
            //        if (current_click_tune_mode == ClickTuneMode.VFOB && wheel_tunes_vfob)
            //        {
            //            if (rx2_enabled && chkVFOSplit.Checked)
            //                VFOASubFreq = SnapTune(VFOASubFreq, step, num_steps);
            //            else
            //                VFOBFreq = SnapTune(VFOBFreq, step, num_steps);
            //        }
            //        else VFOAFreq = SnapTune(VFOAFreq, step, num_steps);
            //        break;
            //}
        }

        private void PanDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            Point pos = new Point(-1, -1);
            pos = new Point(e.X, e.Y);

            mousePos = pos;
            mouseDownPos = mousePos;

            getRegion(mousePos);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    //switch (CurrentDisplayMode)
                    //{
                    //    case DisplayMode.PANADAPTER:
                    //    case DisplayMode.PANAFALL:
                    //    case DisplayMode.HISTOGRAM:
                    //    case DisplayMode.SPECTRUM:
                    if (mouseRegion == DisplayRegion.filterRegion)
                    {
                        //  m_highlightFilter = true;
                        // wholeFilterDrag = true;

                        //whole_filter_start_x = e.X;

                        //if (!mox)
                        //{
                        //    rx1_whole_filter_drag = true;
                        //    whole_filter_start_low = radio.GetDSPRX(0, 0).RXFilterLow;
                        //    whole_filter_start_high = radio.GetDSPRX(0, 0).RXFilterHigh;
                        //}
                        //else
                        //{
                        //    tx_whole_filter_drag = true;
                        //    whole_filter_start_low = SetupForm.TXFilterLow;
                        //    whole_filter_start_high = SetupForm.TXFilterHigh;
                        //}
                    }
                    else if (mouseRegion == DisplayRegion.freqScalePanadapterRegion)
                    {
                        rulerMouseDownPos = new Point(FreqScalePanRect.Left, FreqScalePanRect.Top);

                        spectrum_drag_last_x = e.X;
                        //if (click_tune_display)
                        //{
                        //    rx1_spectrum_tune_drag = true;

                        //    Cursor = Cursors.SizeWE;
                        //}

                        //  if (e.Button == System.Windows.Forms.MouseButtons.Right) next_cursor = Cursors.HSplit;
                        //  return;
                    }
                    else if (mouseRegion == DisplayRegion.dBmScalePanadapterRegion)
                    {
                        rulerMouseDownPos = new Point(DBMScalePanRect.Left, DBMScalePanRect.Top);

                        // if (!mox)
                        // {
                        grid_minmax_drag_start_point = pos;
                        gridminmaxadjust = true;
                        // tx1_grid_adjust = false;
                        grid_minmax_max_y = (decimal)SpectrumGridMax;
                        grid_minmax_min_y = (decimal)SpectrumGridMin;
                        // Cursor = grabbing;
                        //}
                        //else
                        //{
                        //    if (((rx1_grid_adjust && !TXOnVFOB) ||
                        //        (rx1_grid_adjust && TXOnVFOB && !RX2Enabled)) &&
                        //        CurrentDisplayMode != DisplayMode.PANAFALL)
                        //    {
                        //        grid_minmax_drag_start_point = pos;
                        //        gridminmaxadjust = true;
                        //        tx1_grid_adjust = true;
                        //        grid_minmax_max_y = (decimal)picDisplay.TXSpectrumGridMax;
                        //        grid_minmax_min_y = (decimal)picDisplay.TXSpectrumGridMin;
                        //        Cursor = grabbing;
                        //    }
                        //    else if (rx1_grid_adjust && picDisplay.TXOnVFOB)
                        //    {
                        //        grid_minmax_drag_start_point = pos;
                        //        gridminmaxadjust = true;
                        //        tx1_grid_adjust = false;
                        //        grid_minmax_max_y = (decimal)SpectrumGridMax;
                        //        grid_minmax_min_y = (decimal)SpectrumGridMin;
                        //        Cursor = grabbing;
                        //    }
                        //}
                    }

                    // break;
                    //    case DisplayMode.WATERFALL:
                    //        break;
                    // }
                    // }

                    if (mouseRegion == DisplayRegion.panadapterRegion || mouseRegion == DisplayRegion.waterfallRegion)
                    {
                        spectrum_drag_last_x = e.X;

                        //if (!mox)
                        //{
                        //    switch (CurrentDisplayMode)
                        //    {
                        //        case DisplayMode.PANADAPTER:
                        //            if (picDisplay.AGCKnee.Contains(e.X, e.Y) && show_agc)
                        //            {
                        //                agc_knee_drag = true;
                        //                Cursor = grabbing;// Cursors.HSplit;
                        //                // Debug.WriteLine("AGCKnee Y:" + picDisplay.AGCKnee.Y);

                        //            }
                        //            else
                        //                if (picDisplay.AGCHang.Contains(e.X, e.Y) && show_agc)
                        //                {
                        //                    agc_hang_drag = true;
                        //                    Cursor = grabbing; // Cursors.HSplit;
                        //                    // Debug.WriteLine("AGCKnee Y:" + picDisplay.AGCKnee.Y);

                        //                }
                        //                else
                        //                {
                        //                    agc_knee_drag = false;
                        //                    agc_hang_drag = false;
                        //                    // Cursor = Cursors.Cross;
                        //                }
                        //            // }
                        //            break;
                        //    }
                        //}

                        //if (//!near_notch &&
                        //!agc_knee_drag &&
                        //!agc_hang_drag &&
                        //!gridminmaxadjust &&
                        //!gridmaxadjust &&
                        //    (current_click_tune_mode != ClickTuneMode.Off || click_tune_display))
                        //{
                        //    switch (picDisplay.CurrentDisplayMode)
                        //    {
                        //        case DisplayMode.SPECTRUM:
                        //        case DisplayMode.WATERFALL:
                        //        case DisplayMode.HISTOGRAM:
                        //        case DisplayMode.PANADAPTER:
                        //        case DisplayMode.PANAFALL:
                        //        case DisplayMode.PANASCOPE:
                        //            float x = PixelToHz(e.X);
                        //            double freq;
                        //            if (!click_tune_display)
                        //                freq = double.Parse(txtVFOAFreq.Text) + (double)x * 0.0000010; // click tune w/x-hairs
                        //            else if (click_tune_drag) freq = center_frequency + (double)x * 0.0000010; // click tune & drag vfo
                        //            else freq = double.Parse(txtVFOAFreq.Text); // click & drag vfo

                        //            switch (rx1_dsp_mode)
                        //            {
                        //                case DSPMode.CWL:
                        //                    freq += (float)cw_pitch * 0.0000010;
                        //                    break;
                        //                case DSPMode.CWU:
                        //                    freq -= (float)cw_pitch * 0.0000010;
                        //                    break;
                        //                case DSPMode.DIGL:
                        //                    if (!ClickTuneFilter) freq += (float)digl_click_tune_offset * 0.0000010;
                        //                    break;
                        //                case DSPMode.DIGU:
                        //                    if (!ClickTuneFilter) freq -= (float)digu_click_tune_offset * 0.0000010;
                        //                    break;
                        //            }

                        //            if (snap_to_click_tuning &&
                        //                 (current_click_tune_mode != ClickTuneMode.Off || click_tune_drag) &&
                        //                rx1_dsp_mode != DSPMode.CWL &&
                        //                rx1_dsp_mode != DSPMode.CWU &&
                        //                rx1_dsp_mode != DSPMode.DIGL &&
                        //                rx1_dsp_mode != DSPMode.DIGU &&
                        //                Audio.WavePlayback == false)
                        //            {
                        //                // round freq to the nearest tuning step
                        //                long f = (long)(freq * 1000000.0);
                        //                int mult = CurrentTuneStepHz; //(int)(wheel_tune_list[wheel_tune_index] * 1000000.0);
                        //                if (f % mult > mult / 2) f += (mult - f % mult);
                        //                else f -= f % mult;
                        //                freq = (double)f * 0.0000010;
                        //            }
                        //            // }

                        //            if (click_tune_display)
                        //            {
                        //                // spectrum_drag_last_x = e.X;

                        //                //if (rx2_enabled && e.Y > picDisplay.Height / 2)
                        //                //{
                        //                //    spectrum_drag_last_x = e.X;
                        //                //    if (click_tune_rx2_display)
                        //                //    {
                        //                //        if (e.Y < ((picDisplay.Height / 2) + 15))
                        //                //        {
                        //                //            rx2_spectrum_tune_drag = true;
                        //                //            Cursor = Cursors.SizeWE;
                        //                //        }
                        //                //        else
                        //                //        {
                        //                //            rx2_click_tune_drag = true;
                        //                //            Cursor = grabbing;
                        //                //        }
                        //                //    }
                        //                //    else rx2_spectrum_drag = true;
                        //                //}
                        //                //else
                        //                //{
                        //                //  spectrum_drag_last_x = e.X; // change to freq rectangle
                        //                // if (click_tune_display)
                        //                // {
                        //                //  if (e.Y < 15)
                        //                //  {
                        //                //     rx1_spectrum_tune_drag = true;
                        //                //     Cursor = Cursors.SizeWE;
                        //                //  }
                        //                // else
                        //                //  {
                        //                rx1_click_tune_drag = true;
                        //                Cursor = grabbing;
                        //                //  }
                        //                // }

                        //                // }
                        //            }
                        //            else rx1_spectrum_drag = true;

                        //            //spectrum_drag_last_x = e.X;
                        //            //if (click_tune_display)
                        //            //{
                        //            //    if (!rx1_spectrum_tune_drag)
                        //            //    {
                        //            //        rx1_click_tune_drag = true;
                        //            //        Cursor = grabbing;
                        //            //    }
                        //            //}
                        //            //else rx1_spectrum_drag = true;

                        //            if (!rx1_spectrum_drag)
                        //            {
                        //                if (!rx2_enabled)
                        //                {
                        //                    if (!rx1_spectrum_tune_drag)
                        //                    {
                        //                        if (current_click_tune_mode == ClickTuneMode.VFOA ||
                        //                            (click_tune_display && current_click_tune_mode != ClickTuneMode.VFOB))
                        //                        {
                        //                            VFOAFreq = Math.Round(freq, 6);
                        //                        }
                        //                        else
                        //                            VFOBFreq = Math.Round(freq, 6);
                        //                    }
                        //                }
                        //                else
                        //                {

                        //                    if (current_click_tune_mode == ClickTuneMode.VFOB && // red cross hairs
                        //                        (chkVFOSplit.Checked || chkEnableMultiRX.Checked))
                        //                    {
                        //                        VFOASubFreq = Math.Round(freq, 6);
                        //                    }
                        //                    else
                        //                    {
                        //                        if (e.Y <= picDisplay.Height / 2)
                        //                        {
                        //                            if (!rx1_spectrum_tune_drag)
                        //                                VFOAFreq = Math.Round(freq, 6);
                        //                        }

                        //                        else
                        //                        {
                        //                            if (!rx2_spectrum_tune_drag)
                        //                                VFOBFreq = Math.Round(freq, 6);
                        //                        }
                        //                    }
                        //                }
                        //            }

                        //            if (!chkMOX.Checked && (chkRIT.Checked && current_click_tune_mode == ClickTuneMode.VFOA))
                        //                udRIT.Value = 0;
                        //            else if (chkMOX.Checked && chkXIT.Checked && current_click_tune_mode == ClickTuneMode.VFOB)
                        //                udXIT.Value = 0;
                        //            break;
                        //        default:
                        //            break;
                        //    }
                        //}
                        //else if (//!near_notch &&
                        //      !agc_knee_drag &&
                        //      !agc_hang_drag &&
                        //      !gridminmaxadjust &&
                        //      !gridmaxadjust)// current_click_tune_mode == ClickTuneMode.Off) 
                        //{
                        //    switch (picDisplay.CurrentDisplayMode)
                        //    {
                        //        case DisplayMode.PANADAPTER:
                        //        case DisplayMode.WATERFALL:
                        //        case DisplayMode.PANAFALL:
                        //        case DisplayMode.PANASCOPE:
                        //            int low_x = 0, high_x = 0;
                        //            int vfoa_sub_x = 0;
                        //            int vfoa_sub_low_x = 0;
                        //            int vfoa_sub_high_x = 0;
                        //            if (mox)// && chkVFOATX.Checked)
                        //            {
                        //                low_x = HzToPixel(radio.GetDSPTX(0).TXFilterLow);
                        //                high_x = HzToPixel(radio.GetDSPTX(0).TXFilterHigh);
                        //            }
                        //            else if (rx1_dsp_mode != DSPMode.DRM)
                        //            {
                        //                low_x = HzToPixel(radio.GetDSPRX(0, 0).RXFilterLow);
                        //                high_x = HzToPixel(radio.GetDSPRX(0, 0).RXFilterHigh);
                        //            }

                        //            if (chkEnableMultiRX.Checked && !mox)
                        //            {
                        //                if (!rx2_enabled)
                        //                {
                        //                    vfoa_sub_x = HzToPixel((float)((VFOBFreq - VFOAFreq) * 1000000.0));
                        //                    vfoa_sub_low_x = vfoa_sub_x + (HzToPixel((int)udFilterLow.Value) - HzToPixel(0.0f));
                        //                    vfoa_sub_high_x = vfoa_sub_x + (HzToPixel((int)udFilterHigh.Value) - HzToPixel(0.0f));
                        //                }
                        //                else
                        //                {
                        //                    vfoa_sub_x = HzToPixel((float)((VFOASubFreq - VFOAFreq) * 1000000.0));
                        //                    vfoa_sub_low_x = vfoa_sub_x + (HzToPixel((int)udFilterLow.Value) - HzToPixel(0.0f));
                        //                    vfoa_sub_high_x = vfoa_sub_x + (HzToPixel((int)udFilterHigh.Value) - HzToPixel(0.0f));
                        //                }
                        //            }
                        //              if (chkEnableMultiRX.Checked && !mox &&
                        //            (e.X > vfoa_sub_low_x - 3 && e.X < vfoa_sub_high_x + 3))
                        //            {
                        //                sub_drag_last_x = e.X;
                        //                if (rx2_enabled) sub_drag_start_freq = VFOASubFreq;
                        //                else sub_drag_start_freq = VFOBFreq;
                        //                rx1_sub_drag = true;
                        //            }
                        //            else
                        //            {
                        //                spectrum_drag_last_x = e.X;
                        //                if (rx2_enabled && e.Y > picDisplay.Height / 2) rx2_spectrum_drag = true;
                        //                else rx1_spectrum_drag = true;
                        //            }

                        //            break;
                        //    }
                        //}
                    }

                    break;
                case MouseButtons.Right:
                    //double cfreq;

                    //cfreq = VFOAFreq;
                    //switch (rx1_dsp_mode)
                    //{
                    //    case DSPMode.CWU: cfreq -= cw_pitch * 1e-6; break;
                    //    case DSPMode.CWL: cfreq += cw_pitch * 1e-6; break;
                    //}
                    ////  }

                    //int clow = (int)PixelToHz(e.X - 3);
                    //int chigh = (int)PixelToHz(e.X + 3);

                    //// NEW !!!!
                    //double Freq = double.Parse(txtVFOAFreq.Text);
                    //if (click_tune_display)    // Correct Notch frequency when CTUN on -G3OQD
                    //    cfreq = cfreq + (center_frequency - Freq);

                    if (mouseRegion == DisplayRegion.dBmScalePanadapterRegion)
                    {
                        // if (!mox)
                        // {
                        grid_minmax_drag_start_point = new Point(e.X, e.Y);
                        gridmaxadjust = true;
                        // tx1_grid_adjust = false;
                        grid_minmax_max_y = (decimal)SpectrumGridMax;
                        // Cursor = grabbing;
                        // }
                        //else if (mox)
                        //{
                        //    if ((rx1_grid_adjust && !picDisplay.TXOnVFOB) ||
                        //        (rx1_grid_adjust && picDisplay.TXOnVFOB && !RX2Enabled))
                        //    {
                        //        grid_minmax_drag_start_point = new Point(e.X, e.Y);
                        //        gridmaxadjust = true;
                        //        tx1_grid_adjust = true;
                        //        grid_minmax_max_y = (decimal)picDisplay.TXSpectrumGridMax;
                        //        Cursor = grabbing;
                        //    }
                        //    else if (picDisplay.TXOnVFOB)
                        //    {
                        //        grid_minmax_drag_start_point = new Point(e.X, e.Y);
                        //        gridmaxadjust = true;
                        //        tx1_grid_adjust = false;
                        //        grid_minmax_max_y = (decimal)picDisplay.SpectrumGridMax;
                        //        Cursor = grabbing;
                        //    }
                        //}
                    }
                    else if (mouseRegion == DisplayRegion.freqScalePanadapterRegion)
                    {
                        // CurrentClickTuneMode = ClickTuneMode.Off;
                    }
                    //else
                    //{
                    //    switch (current_click_tune_mode)
                    //    {
                    //        case ClickTuneMode.Off:
                    //            CurrentClickTuneMode = ClickTuneMode.VFOA;
                    //            break;
                    //        case ClickTuneMode.VFOA:
                    //            if (chkVFOSplit.Checked || chkEnableMultiRX.Checked)
                    //                CurrentClickTuneMode = ClickTuneMode.VFOB;
                    //            else
                    //                CurrentClickTuneMode = ClickTuneMode.Off;
                    //            break;
                    //        case ClickTuneMode.VFOB:
                    //            CurrentClickTuneMode = ClickTuneMode.Off;
                    //            break;
                    //    }
                    //}
                    break;
                //case MouseButtons.Middle:
                //    if (mouse_tune_step)
                //    {
                //        if (shift_down) ChangeTuneStepDown();
                //        else ChangeTuneStepUp();
                //    }
                //    break;
            }
        }


        /********************************************************************************************************
         *																										*
         *											Display Code												*
         *																										*
         ********************************************************************************************************/

        private int adc = 0;
        public int ADC
        {
            get { return adc; }
            set
            {
                adc = value;
            }
        }

        private int sample_rate = 122880000;
        public int SampleRate
        {
            get { return sample_rate; }
            set
            {
                sample_rate = value;
                initWideband();
            }
        }

        private int fft_size = 16384; //4096;
        public int FFTSize
        {
            get { return fft_size; }
            set
            {
                fft_size = value;
                initWideband();
            }
        }

        private int window_type = 6;
        public int WindowType                           // set from Window Type control
        {
            get { return window_type; }
            set
            {
                window_type = value;
                initWideband();
            }
        }

        private double kaiser_pi = 14.0;
        public double KaiserPi                          // set from Kaiser PiAlpha control
        {
            get { return kaiser_pi; }
            set
            {
                kaiser_pi = value;
                initWideband();
            }
        }

        private int avm = 0;
        private bool average_on = true;				// true if the Average button is depressed
        public bool AverageOn                           // set from Average button
        {
            get { return average_on; }
            set
            {
                average_on = value;
                if (peak_on) avm = -1;                  // -1 => peak_detect
                else if (average_on) avm = 3;           //  6 => low_noise_floor, time_weighted, log_data
                else avm = 0;                           //  0 => averaging & peak_detect are both OFF
                initWideband();
            }
        }

        private bool peak_on = false;					// true if the Peak button is depressed
        public bool PeakOn                              // set from Peak button
        {
            get { return peak_on; }
            set
            {
                peak_on = value;
                if (peak_on) avm = -1;
                else if (average_on) avm = 3;
                else avm = 0;
                initWideband();
            }
        }

        private double tau = 0.120;                     // time-constant for averaging
        public double AvTau                             // set from Averaging Time Constant control
        {
            get { return tau; }
            set
            {
                tau = value;
                initWideband();
            }
        }

        private int frame_rate = 15;
        public int FrameRate                            // set from Frame Rate control
        {
            get { return frame_rate; }
            set
            {
                frame_rate = value;
                initWideband();
            }
        }

        private int pixels = 2048;
        public int Pixels                               // display code must set the number of pixel values it needs
        {
            get { return pixels; }
            set
            {
                pixels = value;
                initWideband();
            }
        }

        private double z_slider = 0.0;                  // range is 0.0 to 1.0
        public double ZoomSlider                        // set by Zoom Slider position
        {
            get { return z_slider; }
            set
            {
                z_slider = value;
                initWideband();
            }
        }

        private double p_slider = 0.5;                  // range is 0.0 to 1.0
        public double PanSlider                         // set by Pan Slider position
        {
            get { return p_slider; }
            set
            {
                p_slider = value;
                initWideband();
            }
        }

        private int low_freq;
        public int LowFreq                              // get the lowest freq that's being displayed
        {
            get { return low_freq; }
        }

        private int high_freq;
        public int HighFreq                             // get the highest freq that's being displayed
        {
            get { return high_freq; }
        }

        public void create_wideband(int adc)
        {
            int success = 0;
            int wbid = 32 + adc;
            SpecHPSDRDLL.XCreateAnalyzer(wbid, ref success, 16384, 1, 1, ""); //16384
        }

        public bool init = false;
        public void initWideband()
        {
            if (!init) return;
            int wbid = adc + 32;
            int clip = 0, span_clip_l = 0, span_clip_h = 0, max_w = 0;

            // no spur elimination => only one spur_elim_fft and it's spectrum is not flipped
            int[] flip = { 0 };
            GCHandle handle = GCHandle.Alloc(flip, GCHandleType.Pinned);
            IntPtr h_flip = handle.AddrOfPinnedObject();

            //compute multiplier for weighted averaging
            double avb = Math.Exp(-1.0 / (frame_rate * tau));

            // fraction of the spectrum to clip off each side
            const double CLIP_FRACTION = 0.0;

            // clip is the number of bins to clip off each side of each sub-span
            clip = (int)Math.Floor(CLIP_FRACTION * fft_size);

            // the amount of frequency in each fft bin (for real samples) is given by:
            double bin_width = (double)sample_rate / (double)fft_size;

            // the number of useable bins
            int bins = fft_size / 2 - 2 * clip;

            // the amount of useable bandwidth we get is:
            double bw = bins * bin_width;

            // apply log function to zoom slider value
            double zoom_slider = Math.Log10(9.0 * z_slider + 1.0);

            // limits how much you can zoom in; higher value means you zoom more
            const double zoom_limit = 100.0;

            // width = number of bins to use AFTER zooming
            int width = (int)(bins * (1.0 - (1.0 - 1.0 / zoom_limit) * zoom_slider));

            // FSCLIPL is 0 if pan_slider is 0; it's bins-width if pan_slider is 1
            // FSCLIPH is bins-width if pan_slider is 0; it's 0 if pan_slider is 1
            span_clip_l = (int)Math.Floor(p_slider * (bins - width));
            span_clip_h = bins - width - span_clip_l;

            // the low and high frequencies that are being displayed:
            low_freq = sample_rate / 4 - (int)(0.5 * bw - (double)span_clip_l * bin_width);
            high_freq = sample_rate / 4 + (int)(0.5 * bw - (double)span_clip_h * bin_width);

            SpecHPSDRDLL.SetAnalyzer(
                wbid,                       // id of this analyzer
                2,                          // number of outputs desired for this display
                1,                          // one fft, no spur elimination for this application
                0,                          // REAL data
                h_flip,                     // flip settings for spur elimination ffts
                fft_size,                   // fft_size
                512,                        // input buffer size
                window_type,                // window_type
                kaiser_pi,                  // piAlpha for Kaiser window
                0,                          // overlap can't be used for discontinuous data
                clip,                       // bins to clip on EACH side
                span_clip_l,                // additional bins to clip on the low side
                span_clip_h,                // additional bins to clip on the high side
                pixels,                     // number of pixel values to generate
                1,                          // number of sub-spans to stitch
                0,                          // calibration set
                0.0,                        // calibration fmin
                0.0,                        // calibration fmax
                2 * fft_size                // maximum write-ahead
                );

            SpecHPSDRDLL.SetDisplayAverageMode(wbid, 0, avm);
            SpecHPSDRDLL.SetDisplayAvBackmult(wbid, 0, avb);
        }

        #endregion

        #endregion
        #endregion
        #endregion

    }
}