/*
 *
 * Copyright (C) 2010-2018  Doug Wigley
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Thetis {
public class SpecRX {
    private const int NUM_RX_DISP = 8;
    private SpecHPSDR[] spec_rx;

    public SpecRX() {
        spec_rx = new SpecHPSDR[NUM_RX_DISP];

        // for (int i = 0; i < NUM_RX_DISP; i++)
        // {
        spec_rx[cmaster.inid(0, 0)] = new SpecHPSDR(cmaster.inid(0, 0));
        spec_rx[cmaster.inid(0, 1)] = new SpecHPSDR(cmaster.inid(0, 1));
        spec_rx[cmaster.inid(1, 0)] = new SpecHPSDR(cmaster.inid(1, 0));
        // }
    }

    public SpecHPSDR GetSpecRX(int disp) { return spec_rx[disp]; }
}

public class SpecHPSDR {
    private int disp;
    public SpecHPSDR(int d) { disp = d; }

    private bool update = false;
    public bool Update {
        get { return update; }
        set { update = value; }
    }

    private int spur_eliminationtion_ffts = 1;
    public int SpurEliminationFFTS {
        get { return spur_eliminationtion_ffts; }
        set { spur_eliminationtion_ffts = value; }
    }

    private int data_type = 1;
    public int DataType {
        get { return data_type; }
        set { data_type = value; }
    }

    private int fft_size = 4096;
    public int FFTSize {
        get { return fft_size; }
        set {
            fft_size = value;
            if (update) initAnalyzer();
        }
    }

    private int blocksize;
    public int BlockSize {
        get { return blocksize; }
        set {
            blocksize = value;
            if (update) initAnalyzer();
            if (disp == 0) {
                // cmaster.SetRCVRANBBuffsize(0, 0, blocksize);
                // cmaster.SetRCVRANBBuffsize(2, 0, blocksize);
                // cmaster.SetRCVRANBBuffsize(2, 1, blocksize);
                // cmaster.SetRCVRNOBBuffsize(0, 0, blocksize);
                // cmaster.SetRCVRNOBBuffsize(2, 0, blocksize);
                // cmaster.SetRCVRNOBBuffsize(2, 1, blocksize);
                // for (int i = 0; i < 3; i++)
                // {
                // SpecHPSDRDLL.SetEXTANBBuffsize(i, blocksize);
                // SpecHPSDRDLL.SetEXTNOBBuffsize(i, blocksize);
                // }
            }
            if (disp == 1) {
                // cmaster.SetRCVRANBBuffsize(0, 1, blocksize);
                // cmaster.SetRCVRNOBBuffsize(0, 1, blocksize);
                // SpecHPSDRDLL.SetEXTANBBuffsize(3, blocksize);
                // SpecHPSDRDLL.SetEXTNOBBuffsize(3, blocksize);
            }
        }
    }

    private int window_type = 4;
    public int WindowType {
        get { return window_type; }
        set {
            window_type = value;
            if (update) initAnalyzer();
        }
    }

    private double kaiser_pi = 14.0;
    public double KaiserPi {
        get { return kaiser_pi; }
        set { kaiser_pi = value; }
    }

    private int overlap = 30000;
    public int Overlap {
        get { return overlap; }
        set { overlap = value; }
    }

    private int clip = 0;
    public int Clip {
        get { return clip; }
        set { clip = value; }
    }

    private double span_clip_l = 0;
    public double SpanClipL {
        get { return span_clip_l; }
        set { span_clip_l = value; }
    }

    private double span_clip_h = 0;
    public double SpanClipH {
        get { return span_clip_h; }
        set { span_clip_h = value; }
    }

    private int pixels = 2048;
    public int Pixels {
        get { return pixels; }
        set {
            pixels = value;
            if (update) initAnalyzer();
        }
    }

    private int stitches = 1;
    public int Stitches {
        get { return stitches; }
        set { stitches = value; }
    }

    private int calibration_data_set = 0;
    public int CalibrationDataSet {
        get { return calibration_data_set; }
        set { calibration_data_set = value; }
    }

    private double span_min_freq = 0.0;
    public double SpanMinFreq {
        get { return span_min_freq; }
        set { span_min_freq = value; }
    }

    private double span_max_freq = 0.0;
    public double SpanMaxFreq {
        get { return span_max_freq; }
        set { span_max_freq = value; }
    }

    private bool average_on; // True if the Average button is pressed
    public bool AverageOn {
        get { return average_on; }
        set {
            int avm;
            int avm_wf;
            average_on = value;
            if (peak_on)
                avm = avm_wf = -1;
            else if (average_on) {
                avm = av_mode;
                avm_wf = av_mode_wf;
            } else
                avm = avm_wf = 0;
            SpecHPSDRDLL.SetDisplayAverageMode(disp, 0, avm);
            SpecHPSDRDLL.SetDisplayAverageMode(disp, 1, avm_wf);
        }
    }

    private bool peak_on; // True if the Peak button is pressed
    public bool PeakOn {
        get { return peak_on; }
        set {
            int avm;
            int avm_wf;
            peak_on = value;
            if (peak_on)
                avm = avm_wf = -1;
            else if (average_on) {
                avm = av_mode;
                avm_wf = av_mode_wf;
            } else
                avm = avm_wf = 0;
            SpecHPSDRDLL.SetDisplayAverageMode(disp, 0, avm);
            SpecHPSDRDLL.SetDisplayAverageMode(disp, 1, avm_wf);
        }
    }

    void updateNormalizePan() {
        if (norm_oneHz_pan
            && (det_type_pan == 2 || det_type_pan == 3 || det_type_pan == 4))
            SpecHPSDRDLL.SetDisplayNormOneHz(disp, 0, true);
        else
            SpecHPSDRDLL.SetDisplayNormOneHz(disp, 0, false);
    }

    public double DisplayENB {
        get { return SpecHPSDRDLL.GetDisplayENB(disp); }
    }

    private int det_type_pan;
    public int DetTypePan {
        get { return det_type_pan; }
        set {
            det_type_pan = value;
            SpecHPSDRDLL.SetDisplayDetectorMode(disp, 0, value);
            updateNormalizePan();
        }
    }

    private int det_type_wf;
    public int DetTypeWF {
        get { return det_type_wf; }
        set {
            det_type_wf = value;
            SpecHPSDRDLL.SetDisplayDetectorMode(disp, 1, value);
        }
    }

    private bool norm_oneHz_pan;
    public bool NormOneHzPan {
        get { return norm_oneHz_pan; }
        set {
            norm_oneHz_pan = value;
            updateNormalizePan();
        }
    }

    private int frame_rate = 15;
    public int FrameRate {
        get { return frame_rate; }
        set {
            frame_rate = value;

            if (update) initAnalyzer();
        }
    }

    // maximum number of frames of pixels to average
    const int MAX_AV_FRAMES = 60;

    private double tau; // time-constant for averaging panadapter
    public double AvTau {
        get { return tau; }
        set {
            tau = value;
            // compute multiplier for weighted averaging
            double avb = Math.Exp(-1.0 / (frame_rate * tau));
            // compute number of frames to average for window averaging
            int display_average
                = Math.Max(2, (int)Math.Min(MAX_AV_FRAMES, frame_rate* tau));
            if (disp == 5) {
                disp = 1;
            }
            SpecHPSDRDLL.SetDisplayAvBackmult(disp, 0, avb);
            SpecHPSDRDLL.SetDisplayNumAverage(disp, 0, display_average);
        }
    }

    private double tau_wf; // time-constant for averaging waterfall
    public double AvTauWF {
        get { return tau_wf; }
        set {
            tau_wf = value;
            // compute multiplier for weighted averaging
            double avb = Math.Exp(-1.0 / (frame_rate * tau_wf));
            // compute number of frames to average for window averaging
            int display_average
                = Math.Max(2, (int)Math.Min(MAX_AV_FRAMES, frame_rate* tau_wf));
            SpecHPSDRDLL.SetDisplayAvBackmult(disp, 1, avb);
            SpecHPSDRDLL.SetDisplayNumAverage(disp, 1, display_average);
        }
    }

    private int av_mode; // Averaging Mode for panadapter
    public int AverageMode {
        get { return av_mode; }
        set {
            int avm;
            av_mode = value;
            if (peak_on)
                avm = -1;
            else if (average_on)
                avm = av_mode;
            else
                avm = 0;

            if (update) {
                SpecHPSDRDLL.SetDisplayAverageMode(disp, 0, avm);
            }
        }
    }

    private int av_mode_wf; // Averaging Mode for waterfall
    public int AverageModeWF {
        get { return av_mode_wf; }
        set {
            int avm;
            av_mode_wf = value;
            if (peak_on)
                avm = -1;
            else if (average_on)
                avm = av_mode_wf;
            else
                avm = 0;

            if (update) {
                SpecHPSDRDLL.SetDisplayAverageMode(disp, 1, avm);
            }
        }
    }

    private double z_slider;
    public double ZoomSlider {
        get { return z_slider; }
        set {
            z_slider = value;

            if (update) initAnalyzer();
        }
    }

    private double pan_slider;
    public double PanSlider {
        get { return pan_slider; }
        set {
            pan_slider = value;

            if (update) initAnalyzer();
        }
    }

    private int sample_rate;
    public int SampleRate {
        get { return sample_rate; }
        set {
            sample_rate = value;

            if (update) initAnalyzer();
            SpecHPSDRDLL.SetDisplaySampleRate(disp, sample_rate);
        }
    }

    private bool nb_on = false;
    public bool NBOn {
        get { return nb_on; }
        set { nb_on = value; }
    }

    private bool nb2_on = false;
    public bool NB2On {
        get { return nb2_on; }
        set { nb2_on = value; }
    }

    const double KEEP_TIME = 0.1;
    private int max_w;
    private double freq_offset = 12000.0;
    // private struct FlipStruct
    //{
    //     public int[] flip;
    // }

    private bool _analyzerInitialised = false;
    public bool AnalyzerInitialised {
        get { return _analyzerInitialised; }
        set { /*_analyzerInitialised = true;*/
        }
    }
    public void initAnalyzer() {
        _analyzerInitialised = false;

        // no spur elimination => only one spur_elim_fft and it's spectrum is
        // not flipped
        int[] flip = { 0 };
        GCHandle handle = GCHandle.Alloc(flip, GCHandleType.Pinned);
        IntPtr h_flip = handle.AddrOfPinnedObject();
        // PinnedObject<FlipStruct> h_flip = new PinnedObject<FlipStruct>();
        // FlipStruct fs = new FlipStruct();
        // fs.flip = new int[] { 0 };
        // h_flip.ManangedObject = fs;

        int low = 0;
        int high = 0;
        double bw_per_subspan = 0.0;

        switch (data_type) {
            case 0: // real fft - in case we want to use for wideband data in
                    // the future
            {

                break;
            }
            case 1: // complex fft
            {
                // fraction of the spectrum to clip off each side of each
                // sub-span
                const double CLIP_FRACTION = 0.04;

                // set overlap as needed to achieve the desired frame_rate
                overlap = (int)Math.Max(0.0,
                    Math.Ceiling(
                        fft_size - (double)sample_rate / (double)frame_rate));

                // clip is the number of bins to clip off each side of each
                // sub-span
                clip = (int)Math.Floor(CLIP_FRACTION * fft_size);

                // the amount of frequency in each fft bin (for complex samples)
                // is given by:
                //    this is also equal to the interval width!
                double bin_width = (double)sample_rate / (double)fft_size;
                double bin_width_tx = 96000.0 / (double)fft_size;

                // the number of useable bins per subspan is
                //    the '-1' is due to clipping the Nyquist bin
                int bins_per_subspan = fft_size - 1 - 2 * clip;

                // the amount of useable bandwidth we get from each subspan is:
                //   we'd subtract '1' from 'bins_per_subspan' if we wanted the
                //   interval_width_per_subspan
                bw_per_subspan = bins_per_subspan * bin_width;

                // the total number of bins available to display is:
                int bins = stitches * bins_per_subspan;

                // the number of intervals among all the bins equals 'bins - 1'
                double intervals = (double)(bins - 1);

                // apply log function to zoom slider value
                double zoom_slider = Math.Log10(9.0 * z_slider + 1.0);

                // limits how much you can zoom in; higher value means you zoom
                // more
                const double zoom_limit = 100;

                // calculate the width in intervals after applying zoom
                double width = intervals
                    * (1.0 - (1.0 - 1.0 / zoom_limit) * zoom_slider);

                // span_clip_l is 0 if pan_slider is 0; it's 'intervals - width'
                // if pan_slider is 1 span_clip_h is 'intervals - width' if
                // pan_slider is 0; it's 0 if pan_slider is 1
                span_clip_l = pan_slider * (intervals - width);
                span_clip_h = intervals - width - span_clip_l;

                // MW0LGE_21a
                // this was causing an odd warping in the spectrum when zooming
                // fully out if (Display.RX1DSPMode == DSPMode.DRM)
                //{
                //     //Apply any desired frequency offset
                //     int bin_offset = (int)(freq_offset / bin_width);
                //     if ((span_clip_h -= bin_offset) < 0) span_clip_h = 0;
                //     span_clip_l = bins - width - span_clip_h;
                // }

                // As for the low and high frequencies that are being displayed:
                //    The Thetis grid interface is limited to integer Hertz
                //    values.
                low = -(int)((intervals / 2.0 - span_clip_l) * bin_width);
                high = +(int)((intervals / 2.0 - span_clip_h) * bin_width);

                max_w = fft_size
                    + (int)Math.Min(KEEP_TIME * sample_rate,
                        KEEP_TIME * fft_size * frame_rate);
                break;
            }
        }

        switch (disp) {
            case 0:
                Display.RXDisplayLow = low;
                Display.RXDisplayHigh = high;
                break;
            case 1:
                Display.RX2DisplayLow = low;
                Display.RX2DisplayHigh = high;
                break;
            case 2:
            case 3:
            case 4:
            case 5:
                Display.TXDisplayLow = low;
                Display.TXDisplayHigh = high;
                break;
        }

        NetworkIO.LowFreqOffset = bw_per_subspan;
        NetworkIO.HighFreqOffset = bw_per_subspan;

        if (disp == 0) {
            if (Display.CurrentDisplayMode != DisplayMode.PANADAPTER
                && Display.CurrentDisplayMode != DisplayMode.WATERFALL
                && Display.CurrentDisplayMode != DisplayMode.PANAFALL
                && Display.CurrentDisplayMode != DisplayMode.PANASCOPE)
                return;
        }

        SpecHPSDRDLL.SetAnalyzer(disp, 2, spur_eliminationtion_ffts, data_type,
            h_flip, fft_size, blocksize, window_type, kaiser_pi, overlap, clip,
            span_clip_l, span_clip_h, pixels, stitches, calibration_data_set,
            span_min_freq, span_max_freq, max_w);

        _analyzerInitialised = true;
    }

    public void CalcSpectrum(
        int filter_low, int filter_high, int spec_blocksize, int sample_rate) {
        // filter_low is the low frequency setting for the filter
        // filter_high is the high frequency setting for the filter
        // samplerate is the current samplerate
        // fft_size is the current FFT size

        // no spur elimination => only one spur_elim_fft and it's spectrum is
        // not flipped
        int[] flip = { 0 };
        GCHandle handle = GCHandle.Alloc(flip, GCHandleType.Pinned);
        IntPtr h_flip = handle.AddrOfPinnedObject();
        // PinnedObject<FlipStruct> h_flip = new PinnedObject<FlipStruct>();
        // FlipStruct fs = new FlipStruct();
        // fs.flip = new int[] { 0 };
        // h_flip.ManangedObject = fs;

        // const int extra = 1000;
        // if we allow a little extra spectrum to be displayed on each side of
        //  the filter settings, then, you can look at filter rolloff.  This
        //  seems to happen at least some of the time with the old spectrum
        //  display. "extra" is the amount extra to leave on each side of the
        //  filter bandwidth and is in Hertz.

        // the upper and lower limits of the displayed spectrum would be
        int upper_freq = filter_high; // +extra;
        int lower_freq = filter_low; // -extra;

        // bandwidth to clip off on the high and low sides
        double high_clip_bw = 0.5 * sample_rate - upper_freq;
        double low_clip_bw = 0.5 * sample_rate + lower_freq;

        // calculate the width of each frequency bin
        double bin_width = (double)sample_rate / fft_size;

        // calculate span clip parameters
        int fsclipH = (int)Math.Floor(high_clip_bw / bin_width);
        int fsclipL = (int)Math.Ceiling(low_clip_bw / bin_width);

        // no need for any symmetrical clipping
        int sclip = 0;
        int stitch = 1;
        max_w = fft_size
            + (int)Math.Min(
                KEEP_TIME * sample_rate, KEEP_TIME * fft_size * frame_rate);
        // Display.RXSpectrumDisplayLow = lower_freq;
        // Display.RXSpectrumDisplayHigh = upper_freq;

        // set overlap as needed to achieve the desired frame rate
        overlap = (int)Math.Max(0.0,
            Math.Ceiling(fft_size - (double)sample_rate / (double)frame_rate));

        SpecHPSDRDLL.SetAnalyzer(disp, 2, spur_eliminationtion_ffts, data_type,
            h_flip, fft_size, spec_blocksize, window_type, kaiser_pi, overlap,
            sclip, fsclipL, fsclipH, pixels, stitch, calibration_data_set,
            span_min_freq, span_max_freq, max_w);
    }
}

unsafe class SpecHPSDRDLL {
#region DLL Method Declarations
    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetAnalyzer(int disp, // display identifier
        int n_pixout, // number of pixel outputs per display
        int n_fft, // number of LO frequencies = number of ffts used in
                   // elimination
        int typ, // 0 for real input data (I only); 1 for complex input data (I
                 // & Q)
        IntPtr flp, // vector with one elt for each LO frequency, 1 if high-side
                    // LO, 0 otherwise
        int sz, // size of the fft, i.e., number of input samples
        int bf_sz, // number of samples transferred for each
                   // OpenBuffer()/CloseBuffer()
        int win_type, // integer specifying which window function to use
        double pi, // PiAlpha parameter for Kaiser window
        int ovrlp, // number of samples each fft (other than the first) is to
                   // re-use from the previous
        int clp, // number of fft output bins to be clipped from EACH side of
                 // each sub-span
        double fscLin, // number of bins to clip from low end of entire span
        double fscHin, // number of bins to clip from high end of entire span
        int n_pix, // number of pixel values to return.  may be either <= or >
                   // number of bins
        int n_stch, // number of sub-spans to concatenate to form a complete
                    // span
        int calset, // identifier of which set of calibration data to use
        double fmin, // frequency at first pixel value
        double fmax, // frequency at last pixel value
        int max_w);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void XCreateAnalyzer(int disp, ref int success,
        int m_size, int m_LO, int m_stitch, string app_data_path);
    // public static extern void XCreateAnalyzer(int disp, ref int success, int
    // m_size, int m_LO, int m_stitch);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DestroyAnalyzer(int disp);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetPixels(
        int disp, int pixout, IntPtr pix, ref int flag);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetPixels(
        int disp, int pixout, float* pix, ref int flag);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Spectrum(
        int disp, int ss, int LO, float* pI, float* pQ);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetCalibration(
        int disp, int set, int points, IntPtr cal);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SnapSpectrum(
        int disp, int ss, int LO, double* snap_buff);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetDisplayDetectorMode(
        int disp, int pixout, int mode);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetDisplayAverageMode(
        int disp, int pixout, int mode);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetDisplayNumAverage(
        int disp, int pixout, int num);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetDisplayAvBackmult(
        int disp, int pixout, double mult);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetDisplayNormOneHz(
        int disp, int pixout, bool norm);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern double GetDisplayENB(int disp);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetDisplaySampleRate(int disp, int rate);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void create_nobEXT(int id, int run, int mode,
        int buffsize, double samplerate, double tau, double hangtime,
        double advtime, double backtau, double threshold);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void destroy_nobEXT(int id);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void xnobEXTF(int id, float* I, float* Q);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTNOBBuffsize(int id, int size);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTNOBSamplerate(int id, int rate);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTNOBTau(int id, double tau);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTNOBHangtime(int id, double time);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTNOBAdvtime(int id, double time);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTNOBBacktau(int id, double tau);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTNOBThreshold(int id, double thresh);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTNOBMode(int id, int mode);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void create_anbEXT(int id, int run, int buffsize,
        double samplerate, double tau, double hangtime, double advtime,
        double backtau, double threshold);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void destroy_anbEXT(int id);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void xanbEXTF(int id, float* I, float* Q);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTANBBuffsize(int id, int size);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTANBSamplerate(int id, int rate);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTANBTau(int id, double tau);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTANBHangtime(int id, double time);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTANBAdvtime(int id, double time);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTANBBacktau(int id, double tau);

    [DllImport("WDSP.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetEXTANBThreshold(int id, double thresh);

#endregion
}
}
