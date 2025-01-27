using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Path_Illustrator : Form
    {
        Console console;

        // define internal variables that link to relevant Thetis control properties
        private bool bool_HPSDR = false;
        private bool bool_HERMES = false;
        private bool bool_ANAN_10E = false;
        private bool bool_ANAN_100_PA_rev15 = false;
        private bool bool_ANAN_100_PA_rev24 = false;
        private bool bool_ANAN_100D_PA_rev15 = false;
        private bool bool_ANAN_100D_PA_rev24 = false;
        private bool bool_rx = false;
        private bool bool_ALEX_manual = false;
        private bool bool_ANT1 = false;
        private bool bool_ANT1_TX = false;
        private bool bool_ANT2 = false;
        private bool bool_ANT2_TX = false;
        private bool bool_ANT3 = false;
        private bool bool_ANT3_TX = false;
        private bool bool_DUAL_MERCURY_ALEX = false;
        private bool bool_MON = false;
        private bool bool_RX1_MUTE = false;
        private bool bool_RX2_MUTE = false;
        private bool bool_duplex = false;
        private bool bool_PureSignal = false;
        private bool bool_diversity = false;
        private bool bool_XVTR = false;
        private bool bool_EXT1 = false;
        private bool bool_EXT1_on_TX = false;
        private bool bool_EXT2 = false;
        private bool bool_EXT2_on_TX = false;
        private bool bool_BYPASS = false;
        private bool bool_disable_BYPASS = false;
        private bool bool_HPF_BYPASS = false;
        private bool bool_DisableHPFOnTx = false;
        private bool bool_BYPASS_on_TX = false;
        private bool bool_Rx0_0 = true;
        private bool bool_Rx0_1 = false;
        private bool bool_Rx1_0 = false;
        private bool bool_Rx1_1 = false;
        private bool bool_Rx2_0 = false;
        private bool bool_Rx2_1 = false;
        private bool bool_Rx3_0 = false;
        private bool bool_Rx3_1 = false;
        private bool bool_Rx4_0 = false;
        private bool bool_Rx4_1 = false;
        private bool bool_Rx5_0 = false;
        private bool bool_Rx5_1 = false;
        private bool bool_Rx6_0 = false;
        private bool bool_Rx6_1 = false;
        private bool bool_RX1_OUT_on_TX = false;
        private bool bool_RX1_IN_on_TX = false;
        private bool bool_RX2_IN_on_TX = false;
        private int int_RxAnt_switch;
        private int int_TxAnt_switch;


        // define some pens to draw with
        Pen blackPen = new Pen(Color.Black);
        Pen blackPen2 = new Pen(Color.Black);           // for double width line
        Pen bluePen = new Pen(Color.Blue);
        Pen indianredPen = new Pen(Color.IndianRed);
        Pen redPen = new Pen(Color.Red);
        Pen myPen = new Pen(Color.Black);

        Graphics g = null;                      // define a null graphics object

        static bool update_diagram = true;      // controls whether the routing diagram is to be updated or not

        // assign sizes for the fixed rectangles
        static int std_size = 50;           // size of most square boxes in the diagram
        static int SDR_width = 665;          // width of SDR rectangle
        static int SDR_height = 625;          // height of SDR rectangle
        static int FPGA_width = 230;
        static int FPGA_height = 600;
        static int PC_width = 390;
        static int PC_height = 350;
        static int DSP_width = 50;
        static int DSP_height = 260;
        static int RX_Display_width = 200;     // width of RXn display rectangles
        static int RX_Display_height = 80;     // height of RXn display rectangles

        // define rectangles used in the diagram

        // define ALEX rectangle
        static int ALEX_offset_x = 65;
        static int ALEX_offset_y = 35;
        static Rectangle ALEX = new Rectangle(ALEX_offset_x, ALEX_offset_y, 100, 275);

        // define ALEX_HPF rectangle
        static int ALEX_HPF_offset_x = ALEX.X + 10;
        static int ALEX_HPF_offset_y = ALEX.Y + 25;
        static Rectangle ALEX_HPF = new Rectangle(ALEX_HPF_offset_x, ALEX_HPF_offset_y, 80, 115);

        // define ALEX_LPF rectangle
        static int ALEX_LPF_offset_x = ALEX.X + 10;
        static int ALEX_LPF_offset_y = ALEX.Y + 150;
        static Rectangle ALEX_LPF = new Rectangle(ALEX_LPF_offset_x, ALEX_LPF_offset_y, 80, 115);

        // define ALEX 2 rectangle
        static int ALEX_2_offset_x = 65;
        static int ALEX_2_offset_y = 390;
        static Rectangle ALEX_2 = new Rectangle(ALEX_2_offset_x, ALEX_2_offset_y, 100, 275);

        // define ALEX_2_HPF rectangle
        static int ALEX_2_HPF_offset_x = ALEX_2.X + 10;
        static int ALEX_2_HPF_offset_y = ALEX_2.Y + 25;
        static Rectangle ALEX_2_HPF = new Rectangle(ALEX_2_HPF_offset_x, ALEX_2_HPF_offset_y, 80, 115);

        // define ALEX_2_LPF rectangle
        static int ALEX_2_LPF_offset_x = ALEX_2.X + 10;
        static int ALEX_2_LPF_offset_y = ALEX_2.Y + 150;
        static Rectangle ALEX_2_LPF = new Rectangle(ALEX_2_LPF_offset_x, ALEX_2_LPF_offset_y, 80, 115);

        // define SDR rectangle
        static int SDR_offset_x = 80;
        static int SDR_offset_y = 30;
        static Rectangle SDR = new Rectangle(SDR_offset_x, SDR_offset_y, SDR_width, SDR_height);

        // define FPGA_rectangle
        static int FPGA_offset_x = 500;
        static int FPGA_offset_y = 40;
        static Rectangle FPGA = new Rectangle(FPGA_offset_x, FPGA_offset_y, FPGA_width, FPGA_height);

        // define PC rectangle
        static int PC_offset_x = 760;
        static int PC_offset_y = 30;
        static Rectangle PC = new Rectangle(PC_offset_x, PC_offset_y, PC_width, PC_height);

        // define DSP rectangle
        static int DSP_offset_x = PC.X + 30;
        static int DSP_offset_y = PC.Y + 80;
        static Rectangle DSP = new Rectangle(DSP_offset_x, DSP_offset_y, DSP_width, DSP_height);

        // define DSP_HPSDR rectangle
        static int DSP_HPSDR_offset_x = PC.X + 30;
        static int DSP_HPSDR_offset_y = PC.Y + 30;
        static Rectangle DSP_HPSDR = new Rectangle(DSP_HPSDR_offset_x, DSP_HPSDR_offset_y, 50, 260);

        // define DSP_HERMES rectangle
        static Rectangle DSP_HERMES = new Rectangle(DSP_HPSDR_offset_x, DSP_HPSDR_offset_y, 50, 300);

        // define the HPF rectangle
        static int HPF_offset_x = 230;
        static int HPF_offset_y = 180;
        static Rectangle HPF = new Rectangle(HPF_offset_x, HPF_offset_y, std_size, std_size);

        // define the HPF2 rectangle
        static int HPF2_offset_x = 230;
        static int HPF2_offset_y = 530;
        static Rectangle HPF2 = new Rectangle(HPF2_offset_x, HPF2_offset_y, std_size, std_size);

        // define the LPF rectangle
        static int LPF_offset_x = 310;
        static int LPF_offset_y = 280;
        static Rectangle LPF = new Rectangle(LPF_offset_x, LPF_offset_y, std_size, std_size);

        // define the LPF2 rectangle
        static int LPF2_offset_x = 230;
        static int LPF2_offset_y = 605;
        static Rectangle LPF2 = new Rectangle(LPF2_offset_x, LPF2_offset_y, std_size, std_size);

        // define Mercury rectangle
        static int MERCURY_offset_x = 275;
        static int MERCURY_offset_y = 35;
        static Rectangle MERCURY = new Rectangle(MERCURY_offset_x, MERCURY_offset_y, 160, 235);

        // define Mercury ADC rectangle
        static int MERCURY_ADC_offset_x = MERCURY.X + 10;
        static int MERCURY_ADC_offset_y = MERCURY.Y + 40;
        static Rectangle MERCURY_ADC = new Rectangle(MERCURY_ADC_offset_x, MERCURY_ADC_offset_y, 50, 50);

        // define Mercury CODEC rectangle
        static int MERCURY_CODEC_offset_x = MERCURY.X + 10;
        static int MERCURY_CODEC_offset_y = MERCURY.Y + 180;
        static Rectangle MERCURY_CODEC = new Rectangle(MERCURY_CODEC_offset_x, MERCURY_CODEC_offset_y, 50, 50);

        // define Mercury DDC0 rectangle
        static int MERCURY_DDC0_offset_x = MERCURY.X + 95;
        static int MERCURY_DDC0_offset_y = MERCURY.Y + 40;
        static Rectangle MERCURY_DDC0 = new Rectangle(MERCURY_DDC0_offset_x, MERCURY_DDC0_offset_y, 50, 50);

        // define Mercury DDC1 rectangle
        static int MERCURY_DDC1_offset_x = MERCURY.X + 95;
        static int MERCURY_DDC1_offset_y = MERCURY.Y + 100;
        static Rectangle MERCURY_DDC1 = new Rectangle(MERCURY_DDC1_offset_x, MERCURY_DDC1_offset_y, 50, 50);

        // define Mercury DDC2 rectangle
        static int MERCURY_DDC2_offset_x = MERCURY.X + 95;
        static int MERCURY_DDC2_offset_y = MERCURY.Y + 160;
        static Rectangle MERCURY_DDC2 = new Rectangle(MERCURY_DDC2_offset_x, MERCURY_DDC2_offset_y, 50, 50);

        // define Mercury FPGA rectangle
        static int MERCURY_FPGA_offset_x = MERCURY.X + 75;
        static int MERCURY_FPGA_offset_y = MERCURY.Y + 20;
        static Rectangle MERCURY_FPGA = new Rectangle(MERCURY_FPGA_offset_x, MERCURY_FPGA_offset_y, 75, 207);

        // define Mercury_2 rectangle
        static int MERCURY_2_offset_x = 275;
        static int MERCURY_2_offset_y = 430;
        static Rectangle MERCURY_2 = new Rectangle(MERCURY_2_offset_x, MERCURY_2_offset_y, 160, 235);

        // define Mercury_2 ADC rectangle
        static int MERCURY_2_ADC_offset_x = MERCURY_2.X + 10;
        static int MERCURY_2_ADC_offset_y = MERCURY_2.Y + 40;
        static Rectangle MERCURY_2_ADC = new Rectangle(MERCURY_2_ADC_offset_x, MERCURY_2_ADC_offset_y, 50, 50);

        // define Mercury 2 CODEC rectangle
        static int MERCURY_2_CODEC_offset_x = MERCURY_2.X + 10;
        static int MERCURY_2_CODEC_offset_y = MERCURY_2.Y + 180;
        static Rectangle MERCURY_2_CODEC = new Rectangle(MERCURY_2_CODEC_offset_x, MERCURY_2_CODEC_offset_y, 50, 50);

        // define Mercury 2 DDC0 rectangle
        static int MERCURY_2_DDC0_offset_x = MERCURY_2.X + 95;
        static int MERCURY_2_DDC0_offset_y = MERCURY_2.Y + 40;
        static Rectangle MERCURY_2_DDC0 = new Rectangle(MERCURY_2_DDC0_offset_x, MERCURY_2_DDC0_offset_y, 50, 50);

        // define Mercury 2 DDC1 rectangle
        static int MERCURY_2_DDC1_offset_x = MERCURY_2.X + 95;
        static int MERCURY_2_DDC1_offset_y = MERCURY_2.Y + 100;
        static Rectangle MERCURY_2_DDC1 = new Rectangle(MERCURY_2_DDC1_offset_x, MERCURY_2_DDC1_offset_y, 50, 50);

        // define Mercury 2 DDC2 rectangle
        static int MERCURY_2_DDC2_offset_x = MERCURY_2.X + 95;
        static int MERCURY_2_DDC2_offset_y = MERCURY_2.Y + 160;
        static Rectangle MERCURY_2_DDC2 = new Rectangle(MERCURY_2_DDC2_offset_x, MERCURY_2_DDC2_offset_y, 50, 50);

        // define Mercury 2 FPGA rectangle
        static int MERCURY_2_FPGA_offset_x = MERCURY_2.X + 75;
        static int MERCURY_2_FPGA_offset_y = MERCURY_2.Y + 20;
        static Rectangle MERCURY_2_FPGA = new Rectangle(MERCURY_2_FPGA_offset_x, MERCURY_2_FPGA_offset_y, 75, 207);

        // define METIS rectangle
        static int METIS_offset_x = 620;
        static int METIS_offset_y = 30;
        static Rectangle METIS = new Rectangle(METIS_offset_x, METIS_offset_y, 80, 600);

        // define METIS FPGA rectangle
        static int METIS_FPGA_offset_x = METIS.X + 15;
        static int METIS_FPGA_offset_y = METIS.Y + 30;
        static Rectangle METIS_FPGA = new Rectangle(METIS_FPGA_offset_x, METIS_FPGA_offset_y, 50, 550);

        // define the PA rectangle
        static int PA_offset_x = 230;
        static int PA_offset_y = 380;
        static Rectangle PA = new Rectangle(PA_offset_x, PA_offset_y, std_size, std_size);

        // define the PENELOPE rectangle
        static int PENELOPE_offset_x = 275;
        static int PENELOPE_offset_y = 280;
        static Rectangle PENELOPE = new Rectangle(PENELOPE_offset_x, PENELOPE_offset_y, 295, 142);

        // define the PENELOPE_FPGA rectangle
        static int PENELOPE_FPGA_offset_x = PENELOPE.X + 210;
        static int PENELOPE_FPGA_offset_y = PENELOPE.Y + 10;
        static Rectangle PENELOPE_FPGA = new Rectangle(PENELOPE_FPGA_offset_x, PENELOPE_FPGA_offset_y, 70, 120);

        // define PENELOPE PA
        static int PENELOPE_PA_offset_x = PENELOPE.X + 10;
        static int PENELOPE_PA_offset_y = PENELOPE.Y + 23;
        static Rectangle PENELOPE_PA = new Rectangle(PENELOPE_PA_offset_x, PENELOPE_PA_offset_y, 50, 50);

        // define PENELOPE AMPF
        static int PENELOPE_AMPF_offset_x = PENELOPE.X + 80;
        static int PENELOPE_AMPF_offset_y = PENELOPE.Y + 23;
        static Rectangle PENELOPE_AMPF = new Rectangle(PENELOPE_AMPF_offset_x, PENELOPE_AMPF_offset_y, 50, 50);

        // define PENELOPE DAC
        static int PENELOPE_DAC_offset_x = PENELOPE.X + 145;
        static int PENELOPE_DAC_offset_y = PENELOPE.Y + 23;
        static Rectangle PENELOPE_DAC = new Rectangle(PENELOPE_DAC_offset_x, PENELOPE_DAC_offset_y, 50, 50);

        // define PENELOPE DUC
        static int PENELOPE_DUC_offset_x = PENELOPE.X + 220;
        static int PENELOPE_DUC_offset_y = PENELOPE.Y + 30;
        static Rectangle PENELOPE_DUC = new Rectangle(PENELOPE_DUC_offset_x, PENELOPE_DUC_offset_y, 50, 50);

        // define PENELOPE CODEC
        static int PENELOPE_CODEC_offset_x = PENELOPE.X + 10;
        static int PENELOPE_CODEC_offset_y = PENELOPE.Y + 86;
        static Rectangle PENELOPE_CODEC = new Rectangle(PENELOPE_CODEC_offset_x, PENELOPE_CODEC_offset_y, 50, 50);

        // define the AMP rectangle
        static int AMP_offset_x = 230;
        static int AMP_offset_y = 480;
        static Rectangle AMP = new Rectangle(AMP_offset_x, AMP_offset_y, std_size, std_size);

        // define the AUDIO_AMP rectangle
        static int AUDIO_AMP_offset_x = 200;
        static int AUDIO_AMP_offset_y = 540;
        static Rectangle AUDIO_AMP = new Rectangle(AUDIO_AMP_offset_x, AUDIO_AMP_offset_y, std_size, std_size);

        // define the AMP/FILTER rectangle
        static int AMPF_offset_x = 315;
        static int AMPF_offset_y = 480;
        static Rectangle AMPF = new Rectangle(AMPF_offset_x, AMPF_offset_y, std_size, std_size);

        // define the AUDIO MIXER rectangle
        static int AUDIO_MIXER_offset_x = 935;
        static int AUDIO_MIXER_offset_y = 235;
        static Rectangle AUDIO_MIXER = new Rectangle(AUDIO_MIXER_offset_x, AUDIO_MIXER_offset_y, 200, 50);

        // define the DAC0 rectangle
        static int DAC0_offset_x = 400;
        static int DAC0_offset_y = 480;
        static Rectangle DAC0 = new Rectangle(DAC0_offset_x, DAC0_offset_y, std_size, std_size);

        // define CODEC rectangle
        static int CODEC_offset_x = 400;
        static int CODEC_offset_y = 600;
        static Rectangle CODEC = new Rectangle(CODEC_offset_x, CODEC_offset_y, std_size, std_size);

        // define CODEC2 rectangle
        static int CODEC2_offset_x = 400;
        static int CODEC2_offset_y = 540;
        static Rectangle CODEC2 = new Rectangle(CODEC2_offset_x, CODEC2_offset_y, std_size, std_size);


        // define the DUC0 rectangle
        static int DUC0_offset_x = 550;
        static int DUC0_offset_y = 480;
        static Rectangle DUC0 = new Rectangle(DUC0_offset_x, DUC0_offset_y, std_size, std_size);

        // define the ADC0 rectangle
        static int ADC0_offset_x = 400;
        static int ADC0_offset_y = 50;
        static Rectangle ADC0 = new Rectangle(ADC0_offset_x, ADC0_offset_y, std_size, std_size);

        // define the ADC1 rectangle
        static int ADC1_offset_x = 400;
        static int ADC1_offset_y = 138;
        static Rectangle ADC1 = new Rectangle(ADC1_offset_x, ADC1_offset_y, std_size, std_size);

        // define the ADC2 rectangle
        static int ADC2_offset_x = 400;
        static int ADC2_offset_y = 250;
        static Rectangle ADC2 = new Rectangle(ADC2_offset_x, ADC2_offset_y, std_size, std_size);

        // define the ADC3 rectangle
        static int ADC3_offset_x = 400;
        static int ADC3_offset_y = 350;
        static Rectangle ADC3 = new Rectangle(ADC3_offset_x, ADC3_offset_y, std_size, std_size);

        // define the Rx0 rectangle
        static int Rx0_offset_x = 650;
        static int Rx0_offset_y = 50;
        static Rectangle Rx0 = new Rectangle(Rx0_offset_x, Rx0_offset_y, std_size, std_size);

        // define the Rx1 rectangle
        static int Rx1_offset_x = 650;
        static int Rx1_offset_y = 125;
        static Rectangle Rx1 = new Rectangle(Rx1_offset_x, Rx1_offset_y, std_size, std_size);

        // define the Rx2 rectangle
        static int Rx2_offset_x = 650;
        static int Rx2_offset_y = 200;
        static Rectangle Rx2 = new Rectangle(Rx2_offset_x, Rx2_offset_y, std_size, std_size);

        // define the Rx3 rectangle
        static int Rx3_offset_x = 650;
        static int Rx3_offset_y = 275;
        static Rectangle Rx3 = new Rectangle(Rx3_offset_x, Rx3_offset_y, std_size, std_size);

        // define the Rx4 rectangle
        static int Rx4_offset_x = 650;
        static int Rx4_offset_y = 350;
        static Rectangle Rx4 = new Rectangle(Rx4_offset_x, Rx4_offset_y, std_size, std_size);

        // define the Rx5 rectangle
        static int Rx5_offset_x = 650;
        static int Rx5_offset_y = 429;
        static Rectangle Rx5 = new Rectangle(Rx5_offset_x, Rx5_offset_y, std_size, std_size);

        // define the Rx6 rectangle
        static int Rx6_offset_x = 650;
        static int Rx6_offset_y = 509;
        static Rectangle Rx6 = new Rectangle(Rx6_offset_x, Rx6_offset_y, std_size, std_size);

        // define the RXn_Display rectangles
        static int RXn_Display_offset_x = PC.X + 175;
        static int RXn_Display_offset_y = PC.Y + 15;
        static int RX1_DISPLAY_x = RXn_Display_offset_x;
        static int RX1_DISPLAY_y = RXn_Display_offset_y;
        static int RX2_DISPLAY_x = RX1_DISPLAY_x;
        static int RX2_DISPLAY_y = RX1_DISPLAY_y + 95;
        static Rectangle RX1_DISPLAY = new Rectangle(RX1_DISPLAY_x, RX1_DISPLAY_y, RX_Display_width, RX_Display_height);
        static Rectangle RX2_DISPLAY = new Rectangle(RX2_DISPLAY_x, RX2_DISPLAY_y, RX_Display_width, RX_Display_height);

        // define SWR rectangle
        static int SWR_offset_x = LPF.X - 80;
        static int SWR_offset_y = LPF.Y;
        static Rectangle SWR = new Rectangle(SWR_offset_x, SWR_offset_y, 50, 50);


        // *************************************************************************************************************************
        // define specific diagram points, list in roughly alphabetical order to be able to find entries quickly during development 
        // *************************************************************************************************************************

        // ADC0 points
        static Point ADC0_L = new Point(ADC0.X, ADC0.Y + 25);                       // ADC0 left side, input
        static Point ADC0_L_c = new Point(ADC0.X - 20, ADC0.Y + 25);                  // ADC0 left side connection pt
        static Point ADC0_R = new Point(ADC0.X + 50, ADC0.Y + 25);                  // ADC0 right side
        static Point ADC0_R_c = new Point(ADC0.X + 70, ADC0.Y + 25);                  // ADC0 right side connection pt
        static Point ADC0_corner1_Rx1 = new Point(FPGA.X + 20, ADC0_R.Y);             // corner1 to Rx1 
        static Point ADC0_corner2_Rx1 = new Point(FPGA.X + 20, Rx1.Y + 25);            // corner2 to Rx1
        static Point ADC0_corner1_Rx2 = new Point(FPGA.X + 40, ADC0_R.Y);
        static Point ADC0_corner2_Rx2 = new Point(FPGA.X + 40, Rx2.Y + 25);
        static Point ADC0_corner1_Rx3 = new Point(FPGA.X + 60, ADC0_R.Y);
        static Point ADC0_corner2_Rx3 = new Point(FPGA.X + 60, Rx3.Y + 25);
        static Point ADC0_corner1_Rx4 = new Point(FPGA.X + 80, ADC0_R.Y);
        static Point ADC0_corner2_Rx4 = new Point(FPGA.X + 80, Rx4.Y + 25);
        static Point ADC0_corner1_Rx5 = new Point(FPGA.X + 100, ADC0_R.Y);
        static Point ADC0_corner2_Rx5 = new Point(FPGA.X + 100, Rx5.Y + 25);
        static Point ADC0_corner1_Rx6 = new Point(FPGA.X + 120, ADC0.Y + 25);
        static Point ADC0_corner2_Rx6 = new Point(FPGA.X + 120, Rx6.Y + 25);
        static Point ADC0_R_corner = new Point(DSP.X - 20, ADC0_R.Y);                  // R side corner to DSP

        // ADC1 points
        static Point ADC1_L = new Point(ADC1.X, ADC1.Y + 25);                       // ADC1 left side
        static Point ADC1_L_c = new Point(ADC1.X - 20, ADC1.Y + 25);                  // ADC1 left side connection pt
        static Point ADC1_R = new Point(ADC1.X + 50, ADC1.Y + 25);                  // ADC1 right side
        static Point ADC1_R_c = new Point(ADC1.X + 70, ADC1.Y + 25);                  // ADC1 right side connection pt
        static Point ADC1_L_corner1 = new Point(ADC1.X - 255, ADC1.Y + 25);
        static Point ADC1_L_corner2 = new Point(ADC1.X - 255, SDR.Y + 45);
        static Point ADC1_L_corner3 = new Point(ADC1.X - 220, ADC1.Y + 25);
        static Point ADC1_corner1_Rx0 = new Point(FPGA.X + 10, ADC1_R.Y);             // corner1 to Rx0 
        static Point ADC1_corner2_Rx0 = new Point(FPGA.X + 10, Rx0.Y + 35);            // corner2 to Rx0
        static Point ADC1_corner1_Rx1 = new Point(FPGA.X + 30, ADC1_R.Y);             // corner1 to Rx1 
        static Point ADC1_corner2_Rx1 = new Point(FPGA.X + 30, Rx1.Y + 25);            // corner2 to Rx1
        static Point ADC1_corner1_Rx2 = new Point(FPGA.X + 50, ADC1_R.Y);
        static Point ADC1_corner2_Rx2 = new Point(FPGA.X + 50, Rx2.Y + 25);
        static Point ADC1_corner1_Rx3 = new Point(FPGA.X + 70, ADC1_R.Y);
        static Point ADC1_corner2_Rx3 = new Point(FPGA.X + 70, Rx3.Y + 25);
        static Point ADC1_corner1_Rx4 = new Point(FPGA.X + 90, ADC1_R.Y);
        static Point ADC1_corner2_Rx4 = new Point(FPGA.X + 90, Rx4.Y + 25);
        static Point ADC1_corner1_Rx5 = new Point(FPGA.X + 110, ADC1_R.Y);
        static Point ADC1_corner2_Rx5 = new Point(FPGA.X + 110, Rx5.Y + 25);
        static Point ADC1_corner1_Rx6 = new Point(FPGA.X + 130, ADC1.Y + 25);
        static Point ADC1_corner2_Rx6 = new Point(FPGA.X + 130, Rx6.Y + 25);

        // ADC2 points
        static Point ADC2_L = new Point(ADC2.X, ADC2.Y + 25);                       // ADC2 left side
        static Point ADC2_L_c = new Point(ADC2.X - 20, ADC2.Y + 25);                  // ADC2 left side connection pt
        static Point ADC2_R = new Point(ADC2.X + 50, ADC2.Y + 25);                  // ADC2 right side
        static Point ADC2_R_c = new Point(ADC2.X + 70, ADC2.Y + 25);                  // ADC2 right side connection pt

        // ALEX points
        static Point ALEX_label = new Point(ALEX.X + 5, ALEX.Y + 5);
        static Point ALEX_HPF_label = new Point(ALEX_HPF.X + 5, ALEX_HPF.Y + 5);
        static Point ALEX_HPF_corner1 = new Point(ALEX_HPF.X + 40, ALEX.Y + 65);
        static Point ALEX_HPF_B = new Point(ALEX.X + 50, ALEX.Y + 140);
        static Point ALEX_HPF_corner2 = new Point(ALEX_HPF.X + 40, ALEX.Y + 125);
        static Point ALEX_RX1_out = new Point(ALEX.X, ALEX.Y + 125);
        static Point ALEX_HPF_corner3 = new Point(ALEX_HPF.X + 40, ALEX.Y + 50);
        static Point ALEX_XV_RX_IN = new Point(ALEX.X, ALEX.Y + 50);
        static Point ALEX_HPF_corner4 = new Point(ALEX_HPF.X + 40, ALEX.Y + 80);
        static Point ALEX_RX_2_IN = new Point(ALEX.X, ALEX.Y + 80);
        static Point ALEX_HPF_corner5 = new Point(ALEX_HPF.X + 40, ALEX.Y + 105);
        static Point ALEX_RX_1_IN = new Point(ALEX.X, ALEX.Y + 105);

        static Point ALEX_LPF_label = new Point(ALEX_LPF.X + 5, ALEX_LPF.Y + 5);
        static Point ALEX_LPF_corner1 = new Point(ALEX_LPF.X + 40, ALEX_LPF.Y + 27);
        static Point ALEX_LPF_ANT1 = new Point(ALEX.X, ALEX_LPF.Y + 27);
        static Point ALEX_LPF_corner2 = new Point(ALEX_LPF.X + 40, ALEX_LPF.Y + 52);
        static Point ALEX_LPF_ANT2 = new Point(ALEX.X, ALEX_LPF.Y + 52);
        static Point ALEX_LPF_corner3 = new Point(ALEX_LPF.X + 40, ALEX_LPF.Y + 78);
        static Point ALEX_LPF_ANT3 = new Point(ALEX.X, ALEX_LPF.Y + 78);

        static Point ALEX_To_RX_label = new Point(ALEX.X + 103, ALEX.Y + 45);
        static Point ALEX_RX_out = new Point(ALEX.X + 100, ALEX.Y + 65);
        static Point ALEX_ANT1 = new Point(ALEX.X, ALEX.Y + 175);
        static Point ALEX_ANT1_corner = new Point(ALEX.X + 50, ALEX.Y + 175);
        static Point ALEX_ANT2 = new Point(ALEX.X, ALEX.Y + 200);
        static Point ALEX_ANT2_corner = new Point(ALEX.X + 50, ALEX.Y + 200);
        static Point ALEX_ANT3 = new Point(ALEX.X, ALEX.Y + 225);
        static Point ALEX_ANT3_corner = new Point(ALEX.X + 50, ALEX.Y + 225);

        // ALEX 2 points
        static Point ALEX_2_label = new Point(ALEX_2.X + 5, ALEX_2.Y + 5);
        static Point ALEX_2_HPF_label = new Point(ALEX_2_HPF.X + 5, ALEX_2_HPF.Y + 5);
        static Point ALEX_2_HPF_corner1 = new Point(ALEX_2_HPF.X + 40, ALEX_2.Y + 105);
        static Point ALEX_2_HPF_B = new Point(ALEX_2.X + 50, ALEX_2.Y + 140);
        static Point ALEX_2_HPF_corner2 = new Point(ALEX_2_HPF.X + 40, ALEX_2.Y + 125);
        static Point ALEX_2_RX1_out = new Point(ALEX_2.X, ALEX_2.Y + 125);
        static Point ALEX_2_HPF_corner3 = new Point(ALEX_2_HPF.X + 40, ALEX_2.Y + 50);
        static Point ALEX_2_XV_RX_IN = new Point(ALEX_2.X, ALEX_2.Y + 50);
        static Point ALEX_2_HPF_corner4 = new Point(ALEX_2_HPF.X + 40, ALEX_2.Y + 80);
        static Point ALEX_2_RX_2_IN = new Point(ALEX_2.X, ALEX_2.Y + 80);
        static Point ALEX_2_HPF_corner5 = new Point(ALEX_2_HPF.X + 40, ALEX_2.Y + 105);
        static Point ALEX_2_RX_1_IN = new Point(ALEX_2.X, ALEX_2.Y + 105);

        static Point ALEX_2_LPF_label = new Point(ALEX_2_LPF.X + 5, ALEX_2_LPF.Y + 5);
        static Point ALEX_2_To_RX_label = new Point(ALEX_2.X + 105, ALEX_2.Y + 85);
        static Point ALEX_2_RX_out = new Point(ALEX_2.X + 100, ALEX_2.Y + 105);
        static Point ALEX_2_RX_out_corner1 = new Point(ALEX_2.X + 150, ALEX_2.Y + 105);
        static Point ALEX_2_ANT1 = new Point(ALEX_2.X, ALEX_2.Y + 175);
        static Point ALEX_2_ANT1_corner = new Point(ALEX_2.X + 50, ALEX_2.Y + 175);
        static Point ALEX_2_ANT2 = new Point(ALEX_2.X, ALEX_2.Y + 200);
        static Point ALEX_2_ANT2_corner = new Point(ALEX_2.X + 50, ALEX_2.Y + 200);
        static Point ALEX_2_ANT3 = new Point(ALEX_2.X, ALEX_2.Y + 225);
        static Point ALEX_2_ANT3_corner = new Point(ALEX_2.X + 50, ALEX_2.Y + 225);

        // AMP/FILTER points
        static Point AMPF_R = new Point(AMPF.X + 50, AMPF.Y + 25);                  // AMPF right side
        static Point AMPF_L = new Point(AMPF.X, AMPF.Y + 25);                      // AMPF left side
        static Point AMPF_L_PA15 = new Point(AMPF.X - 60, AMPF_L.Y);

        // AUDIO_AMP points
        static Point AUDIO_AMP_L = new Point(AUDIO_AMP.X, AUDIO_AMP.Y + 25);                     // AUDIO_AMP left side
        static Point AUDIO_AMP_L_c = new Point(AUDIO_AMP.X - 20, AUDIO_AMP.Y + 25);                // AUDIO_AMP left connection pt
        static Point AUDIO_AMP_R = new Point(AUDIO_AMP.X + 50, AUDIO_AMP.Y + 25);                // AUDIO_AMP right side
        static Point AUDIO_AMP_R_c = new Point(AUDIO_AMP.X + 70, AUDIO_AMP.Y + 25);                // AUDIO_AMP right connection pt

        // AUDIO_MIXER points
        static Point AUDIO_MIXER_L_1 = new Point(AUDIO_MIXER.X, AUDIO_MIXER.Y + 20);                     // AUDIO_MIXER input 1 left side
        static Point AUDIO_MIXER_L_2 = new Point(AUDIO_MIXER.X, AUDIO_MIXER.Y + 30);                // AUDIO_MIXER input 2 left side
        static Point AUDIO_MIXER_B = new Point(AUDIO_MIXER.X + 100, AUDIO_MIXER.Y + 50);                // AUDIO_MIXER bottom center
        static Point AUDIO_MIXER_external_corner = new Point(AUDIO_MIXER.X + 100, CODEC2_offset_y + 25);
        static Point AUDIO_MIXER_internal_corner1 = new Point(AUDIO_MIXER.X + 100, AUDIO_MIXER_L_1.Y);
        static Point AUDIO_MIXER_internal_corner2 = new Point(AUDIO_MIXER.X + 100, AUDIO_MIXER_L_2.Y);
        static Point AUDIO_MIXER_external_corner2 = new Point(AUDIO_MIXER.X + 100, AUDIO_MIXER.Y + 130);

        // BYPASS points
        static Point BYPASS_corner1 = new Point(SDR.X + 200, SDR.Y + 170);
        static Point BYPASS_corner2 = new Point(SDR.X + 200, SDR.Y + 45);

        //C1 points
        static Point C1 = new Point(SDR.X, SDR.Y + 20);                         //rear panel C1 connector 
        static Point C1_c = new Point(SDR.X + 20, SDR.Y + 20);                    //C1 connector, right connection pt
        static Point C1_label = new Point(C1.X - 50, C1.Y + 5);

        //C2 points
        static Point C2 = new Point(SDR.X, SDR.Y + 45);                         // C2 connector 
        static Point C2_c = new Point(SDR.X + 20, SDR.Y + 45);                    // C2 connector, right connection pt
        static Point C2_corner = new Point(SDR.X + 120, C2.Y);
        static Point C2_label = new Point(C2.X - 50, C2.Y + 5);
        static Point C2_label_HPSDR = new Point(C2.X - 73, C2.Y + 5);

        // C3 points
        static Point C3 = new Point(SDR.X, SDR.Y + 70);                         // C3 connector 
        static Point C3_c = new Point(SDR.X + 20, SDR.Y + 70);                    // C3 connector, right connection pt
        static Point C3_corner = new Point(SDR.X + 120, C3.Y);
        static Point C3_corner2 = new Point(SDR.X + 220, C3.Y);
        static Point C3_label = new Point(C3.X - 50, C3.Y + 5);
        static Point C3_corner3 = new Point(SDR.X + 220, ADC0_L.Y);
        static Point C3_label_HPSDR = new Point(C3.X - 65, C3.Y + 5);


        // C4 points
        static Point C4 = new Point(SDR.X, SDR.Y + 95);                         // C4 connector 
        static Point C4_c = new Point(SDR.X + 20, SDR.Y + 95);                    // C4 connector, right connection pt
        static Point C4_corner = new Point(SDR.X + 120, C4.Y);
        static Point C4_corner2 = new Point(SDR.X + 220, C4.Y);
        static Point C4_corner3 = new Point(SDR.X + 220, ADC0_L.Y);
        static Point C4_label = new Point(C4.X - 50, C4.Y + 5);
        static Point C4_label_HPSDR = new Point(C4.X - 65, C4.Y + 5);

        // C5 points
        static Point C5 = new Point(SDR.X, SDR.Y + 120);                        // C5 connector 
        static Point C5_c = new Point(SDR.X + 20, SDR.Y + 120);                   // C5 connector, right connection pt
        static Point C5_corner = new Point(LPF.X + 70, C5.Y);                    // C5 corner
        static Point C5_riser = new Point(LPF.X + 70, ADC0_L.Y);                   // C5 riser pt at ADC0 level
        static Point C5_label = new Point(C5.X - 50, C5.Y + 5);
        static Point C5_label_HPSDR = new Point(C5.X - 79, C5.Y + 5);

        // C6 points
        static Point C6 = new Point(SDR.X, SDR.Y + 145);                        // C6 connector 
        static Point C6_c = new Point(SDR.X + 20, SDR.Y + 145);                   // C6 connector, right connection pt
        static Point C6_label = new Point(C6.X - 50, C6.Y + 5);

        // C7 points
        static Point C7 = new Point(SDR.X, SDR.Y + 170);                        // C7 connector 
        static Point C7_c = new Point(SDR.X + 20, SDR.Y + 170);                   // C7 connector, right connection pt
        static Point C7_label = new Point(C7.X - 50, C7.Y + 5);
        static Point C7_label_HPSDR = new Point(C7.X - 65, C7.Y + 5);

        // C8 points
        static Point C8 = new Point(SDR.X, SDR.Y + 195);                        // C8 connector 
        static Point C8_c = new Point(SDR.X + 20, SDR.Y + 195);                   // C8 connector, right connection pt
        static Point C8_label = new Point(C8.X - 50, C8.Y + 5);
        static Point C8_label_HPSDR = new Point(C8.X - 65, C8.Y + 5);

        // C9 points
        static Point C9 = new Point(SDR.X, SDR.Y + 220);                        // C9 connector 
        static Point C9_c = new Point(SDR.X + 20, SDR.Y + 220);                   // C9 connector, right connection pt
        static Point C9_label = new Point(C9.X - 50, C9.Y + 5);
        static Point C9_label_HPSDR = new Point(C9.X - 65, C9.Y + 5);

        // C10 points
        static Point C10 = new Point(SDR.X, SDR.Y + 245);                        // C10 connector 
        static Point C10_c = new Point(SDR.X + 20, SDR.Y + 245);                   // C10 connector, right connection pt
        static Point C10_label = new Point(C10.X - 50, C10.Y + 5);
        static Point C10_label_HPSDR = new Point(C10.X - 78, C10.Y + 5);
        static Point C10_label_ALEX_TX_IN = new Point(C10.X - 78, C10.Y - 1);
        // C11 points
        static Point C11 = new Point(SDR.X, SDR.Y + 275);                        // C11 connector 
        static Point C11_c = new Point(SDR.X + 20, SDR.Y + 275);                   // C11 connector, right connection pt
        static Point C11_label = new Point(C11.X - 50, C11.Y + 5);

        // C12 points
        static Point C12 = new Point(SDR.X, SDR.Y + 295);                        // C12 connector 
        static Point C12_c = new Point(SDR.X + 20, SDR.Y + 295);                   // C12 connector, right connection pt
        static Point C12_label = new Point(C12.X - 50, C12.Y + 5);

        // C13 points
        static Point C13 = new Point(SDR.X, SDR.Y + 320);                        // C13 connector 
        static Point C13_c = new Point(SDR.X + 20, SDR.Y + 320);                   // C13 connector, right connection pt
        static Point C13_label = new Point(C13.X - 50, C13.Y + 5);

        // C14 points
        static Point C14 = new Point(SDR.X, SDR.Y + 345);                        // C14 connector 
        static Point C14_c = new Point(SDR.X + 20, SDR.Y + 345);                   // C14 connector, right connection pt
        static Point C14_label = new Point(C14.X - 50, C14.Y + 5);

        // C15 points
        static Point C15 = new Point(SDR.X, SDR.Y + 370);                        // C15 connector 
        static Point C15_c = new Point(SDR.X + 20, SDR.Y + 370);                   // C15 connector, right connection pt
        static Point C15_label = new Point(C15.X - 50, C15.Y + 5);

        // C16 points
        static Point C16 = new Point(SDR.X, SDR.Y + 395);                        // C16 connector 
        static Point C16_c = new Point(SDR.X + 20, SDR.Y + 395);                   // C16 connector, right connection pt
        static Point C16_label = new Point(C16.X - 50, C16.Y + 5);
        static Point C16_ALEX_2_label = new Point(5, 435);

        // C17 points
        static Point C17 = new Point(SDR.X, SDR.Y + 420);                        // C17 connector 
        static Point C17_c = new Point(SDR.X + 20, SDR.Y + 420);                   // C17 connector, right connection pt
        static Point C17_label = new Point(C17.X - 50, C17.Y + 5);
        static Point C17_ALEX_2_label = new Point(15, 460);

        // C18 points
        static Point C18 = new Point(SDR.X, SDR.Y + 445);                        // C18 connector 
        static Point C18_c = new Point(SDR.X + 20, SDR.Y + 445);                   // C18 connector, right connection pt
        static Point C18_label = new Point(C18.X - 50, C18.Y + 5);
        static Point C18_ALEX_2_label = new Point(15, 485);

        // C19 points
        static Point C19 = new Point(SDR.X, SDR.Y + 470);                        // C19 connector 
        static Point C19_c = new Point(SDR.X + 20, SDR.Y + 470);                   // C19 connector, right connection pt
        static Point C19_label = new Point(C19.X - 50, C19.Y + 5);
        static Point C19_ALEX_2_label = new Point(3, 510);

        // C20 points
        static Point C20 = new Point(SDR.X, SDR.Y + 495);                        // C20 connector 
        static Point C20_c = new Point(SDR.X + 20, SDR.Y + 495);                   // C20 connector, right connection pt
        static Point C20_label = new Point(C20.X - 50, C20.Y + 5);
        static Point C20_ALEX_2_label = new Point(20, 560);

        // C21 points
        //static Point C21 = new Point(SDR.X, CODEC.Y + 25);                       // C21 connector 
        //static Point C21_c = new Point(SDR.X + 20, CODEC.Y + 25);                  // C21 connector, right connection pt
        //static Point C21_label = new Point(C21.X - 50, C21.Y + 5);


        // C24 points
        static Point C24 = new Point(SDR.X, SDR.Y + 535);                       // C24 connector 
        static Point C24_c = new Point(SDR.X + 20, CODEC.Y + 25);                  // C24 connector, right connection pt
        static Point C24_label = new Point(C24.X - 50, C24.Y + 5);
        static Point C24_ALEX_2_label = new Point(20, 585);

        // C25 points
        static Point C25 = new Point(SDR.X, SDR.Y + 575);                       // C25 connector 
        static Point C25_c = new Point(SDR.X + 20, CODEC2.Y + 25);                  // C25 connector, right connection pt
        static Point C25_label = new Point(C25.X - 95, C25.Y + 5);
        static Point C25_ALEX_2_label = new Point(20, 610);

        // C26 points
        static Point C26 = new Point(SDR.X, CODEC.Y + 25);                       // C26 connector 
        static Point C26_c = new Point(SDR.X + 20, CODEC.Y + 25);                  // C26 connector, right connection pt
        static Point C26_label = new Point(C26.X - 50, C26.Y + 5);
        static Point C26_ALEX_2_label = new Point(5, 630);

        // CODEC points
        static Point CODEC_L = new Point(CODEC.X, CODEC.Y + 25);                     // CODEC left side
        static Point CODEC_L_c = new Point(CODEC.X - 20, CODEC.Y + 25);                // CODEC left connection pt
        static Point CODEC_R = new Point(CODEC.X + 50, CODEC.Y + 25);                // CODEC right side
        static Point CODEC_R_c = new Point(CODEC.X + 70, CODEC.Y + 25);                // CODEC right connection pt

        // CODEC2 points
        static Point CODEC2_L = new Point(CODEC2.X, CODEC2.Y + 25);                     // CODEC2 left side
        static Point CODEC2_L_c = new Point(CODEC2.X - 20, CODEC2.Y + 25);                // CODEC2 left connection pt
        static Point CODEC2_R = new Point(CODEC2.X + 50, CODEC2.Y + 25);                // CODEC2 right side
        static Point CODEC2_R_c = new Point(CODEC2.X + 70, CODEC2.Y + 25);                // CODEC2 right connection pt

        // DAC0 points
        static Point DAC0_L = new Point(DAC0.X, DAC0.Y + 25);                       // DAC0 left side
        static Point DAC0_L_c = new Point(DAC0.X - 20, DAC0.Y + 25);                  // DAC0 left side connection pt
        static Point DAC0_R = new Point(DAC0.X + 50, DAC0.Y + 25);                  // DAC0 right side
        static Point DAC0_R_c = new Point(DAC0.X + 70, DAC0.Y + 25);                  // DAC0 right side connection pt

        // DSP points
        static Point DSP_B_1 = new Point(DSP.X + 10, DSP.Y + 220);                   // DSP bottom side, input 1
        static Point DSP_B_1_c = new Point(DSP.X + 10, DSP.Y + 135);                   // DSP bottom side input 1 connection pt
        static Point DSP_B_2 = new Point(DSP.X + 40, DSP.Y + 220);                   // DSP bottom side, input 2
        static Point DSP_B_2_c = new Point(DSP.X + 40, DSP.Y + 135);                   // DSP bottom side input 2 connection pt
        static Point DSP_L_1 = new Point(DSP.X, DSP.Y + 20);                         // DSP left side input 1
        static Point DSP_L_1_c = new Point(DSP.X - 20, DSP.Y + 20);                    // DSP left connection pt input 1
        static Point DSP_R_1 = new Point(DSP.X + 50, DSP.Y + 20);                    // DSP right side output 1
        static Point DSP_R_1_c = new Point(DSP.X + 70, DSP.Y + 20);                    // DSP right connection pt output 1
        static Point DSP_L_2 = new Point(DSP.X, DSP.Y + 40);                         // DSP left side input 2
        static Point DSP_L_2_c = new Point(DSP.X - 20, DSP.Y + 40);                    // DSP left connection pt input 2
        static Point DSP_R_2 = new Point(DSP.X + 50, DSP.Y + 40);                    // DSP right side output 2
        static Point DSP_R_2_c = new Point(DSP.X + 70, DSP.Y + 40);                    // DSP right connection pt output 2
        static Point DSP_L_3 = new Point(DSP.X, DSP.Y + 115);                        // DSP left side input 3 (Rx2)
        static Point DSP_R_3 = new Point(DSP.X + 70, DSP.Y + 115);                   // DSP right connection pt output 3 (Rx2)
        static Point DSP_L_4 = new Point(DSP.X, DSP.Y + 190);                        // DSP left side input 4 (Rx3)
        static Point DSP_R_4 = new Point(DSP.X + 90, DSP.Y + 190);                   // DSP right side connection pt output 4 (Rx3)

        static Point DSP_R_3_RX1_corner = new Point(DSP_R_3.X, DSP.Y - 40);          // DSP right side corner for Rx2 at RX1 DISPLAY input y level
        static Point DSP_R_4_RX2_corner = new Point(DSP_R_4.X, DSP.Y + 55);          // DSP right side corner for Rx3 at RX2 DISPLAY input y level


        static Point DSP_bottom_corner = new Point(DSP.X + 40, CODEC_R.Y);
        static Point DSP_loopback_1 = new Point(DSP_B_2.X, DSP.Y + 220);
        static Point DSP_loopback_2 = new Point(DSP_B_1.X, DSP.Y + 220);
        static Point DSP_looback_center = new Point(DSP.X + 25, DSP.Y + 270);
        static Point DSP_L_corner_Rx0 = new Point(DSP_L_1_c.X, DSP.Y - 35);
        static Point DSP_L_corner2_Rx0 = new Point(DSP_L_1_c.X, DSP_L_1_c.Y);

        static Point DSP_MIXER_1 = new Point(DSP.X + 50, AUDIO_MIXER_L_1.Y);
        static Point DSP_MIXER_2 = new Point(DSP.X + 50, AUDIO_MIXER_L_2.Y);
        static Point DSP_HPSDR_label = new Point(DSP_HPSDR.X + 10, DSP_HPSDR.Y + 5);
        static Point DSP_internal_MIXER1_1 = new Point(DSP.X + 40, AUDIO_MIXER_L_1.Y);
        static Point DSP_internal_MIXER1_2 = new Point(DSP.X + 40, DSP_R_1.Y);
        static Point DSP_internal_MIXER1_3 = new Point(DSP.X + 40, DSP_internal_MIXER1_2.Y - 30);
        static Point DSP_internal_MIXER2_1 = new Point(DSP.X + 10, AUDIO_MIXER_L_2.Y);
        static Point DSP_internal_MIXER2_2 = new Point(DSP.X + 10, DSP_R_2.Y);
        static Point DSP_internal_MIXER2_3 = new Point(DSP.X + 10, DSP_internal_MIXER2_2.Y + 10);
        static Point DSP_internal_Rx2_audio_connection = new Point(DSP_internal_MIXER1_1.X, DSP_L_3.Y);
        static Point DSP_internal_Rx3_audio_connection = new Point(DSP_internal_MIXER2_1.X, DSP_L_4.Y);

        static Point DSP_internal_diversity_corner1 = new Point(DSP_L_2.X + 10, DSP_L_2.Y);
        static Point DSP_internal_diversity_corner2 = new Point(DSP_L_2.X + 10, DSP_L_1.Y);

        // DUC0 points
        static Point DUC0_T = new Point(DUC0.X + 25, DUC0.Y);                       // DUC0 top side
        static Point DUC0_T_c = new Point(DUC0.X + 25, DUC0.Y - 20);                  // DUC0 top connection pt
        static Point DUC0_L = new Point(DUC0.X, DUC0.Y + 25);                       // DUC0 left side
        static Point DUC0_L_c = new Point(DUC0.X - 20, DUC0.Y + 25);                  // DUC0 left side connection pt
        static Point DUC0_R = new Point(DUC0.X + 50, DUC0.Y + 25);                  // DUC0 right side
        static Point DUC0_R_c = new Point(DUC0.X + 70, DUC0.Y + 25);                  // DUC0 right side connection pt

        static Point DUC0_R_corner = new Point(DSP_B_1.X, DUC0_R.Y);
        static Point DUC0_L_corner = new Point(FPGA.X + 20, Rx1.Y + 25);
        static Point DUC0_L_corner_lower_pt = new Point(DUC0_L_corner.X, DUC0_L.Y);

        // EXT1 points
        static Point EXT1_HPF_corner = new Point(HPF_B.X, C5.Y);

        static Point ext_amp_label1 = new Point(ALEX.X - 40, ALEX.Y + 300);
        static Point ext_amp_label2 = new Point(ALEX.X - 40, ALEX.Y + 315);

        // EXT2 points
        static Point EXT2_HPF_corner = new Point(HPF_B.X, C6.Y);

        // HPF ground points
        static Point HPF_GROUND1 = new Point(SDR.X + 100, SDR.Y + 45);
        static Point HPF_GROUND2 = new Point(SDR.X + 100, SDR.Y + 195);
        static Point HPF_GROUND3 = new Point(SDR.X + 85, SDR.Y + 195);
        static Point HPF_GROUND4 = new Point(SDR.X + 115, SDR.Y + 195);
        static Point HPF_GROUND5 = new Point(SDR.X + 90, SDR.Y + 200);
        static Point HPF_GROUND6 = new Point(SDR.X + 110, SDR.Y + 200);
        static Point HPF_GROUND7 = new Point(SDR.X + 95, SDR.Y + 205);
        static Point HPF_GROUND8 = new Point(SDR.X + 105, SDR.Y + 205);
        static Point HPF_GROUND9 = new Point(SDR.X + 100, SDR.Y + 133);
        static Point HPF_GROUND10 = new Point(SDR.X + 100, C7.Y);

        // ground points
        static Point GROUND1 = new Point(SDR.X + 100, SDR.Y + 120);
        static Point GROUND2 = new Point(SDR.X + 100, SDR.Y + 185);
        static Point GROUND3 = new Point(SDR.X + 85, SDR.Y + 185);
        static Point GROUND4 = new Point(SDR.X + 115, SDR.Y + 185);
        static Point GROUND5 = new Point(SDR.X + 90, SDR.Y + 190);
        static Point GROUND6 = new Point(SDR.X + 110, SDR.Y + 190);
        static Point GROUND7 = new Point(SDR.X + 95, SDR.Y + 195);
        static Point GROUND8 = new Point(SDR.X + 105, SDR.Y + 195);

        // HEADPHONES points
        static Point HEADPHONES = new Point(C25.X, C25.Y);
        static Point HEADPHONES_1 = new Point(C25.X + 240, C25.Y);
        static Point HEADPHONES_2 = new Point(C25.X + 240, AUDIO_AMP_L.Y);

        // HERMES points
        static Point HERMES1 = new Point(80, 660);
        static Point HERMES2 = new Point(80, 370);
        static Point HERMES3 = new Point(230, 370);
        static Point HERMES4 = new Point(230, 30);
        static Point HERMES5 = new Point(740, 30);
        static Point HERMES6 = new Point(740, 660);
        static Point HERMES_corner1 = new Point(ALEX_RX_out.X + 150, ALEX_RX_out.Y);
        static Point HERMES_corner2 = new Point(ALEX_RX_out.X + 150, ADC0_L.Y);
        static Point HERMES_label = new Point(HERMES4.X + 5, HERMES4.Y + 5);
        static Point HERMES_corner3 = new Point(PA.X + 25, PA.Y - 50);
        static Point HERMES_RX_IN_label = new Point(HERMES4.X - 38, HERMES4.Y + 75);
        static Point HERMES_J5_label = new Point(HERMES4.X - 33, HERMES4.Y + 88);
        static Point HERMES_TX_OUT_label = new Point(HERMES4.X - 51, HERMES4.Y + 263);
        static Point HERMES_J3_label = new Point(HERMES4.X - 31, HERMES4.Y + 278);
        static Point HERMES_XVTR_TX_label = new Point(18, 490);
        static Point HERMES_J1_label = new Point(45, 502);
        static Point HERMES_XVTR_TX = new Point(80, 505);

        // HPF points
        static Point HPF_L = new Point(HPF.X, HPF.Y + 25);                         // HPF left side
        static Point HPF_L_c = new Point(HPF.X - 20, HPF.Y + 25);                    // HPF left side connection pt
        static Point HPF_R = new Point(HPF.X + 50, HPF.Y + 25);                    // HPF right side
        static Point HPF_R_c = new Point(HPF.X + 70, HPF.Y + 25);                    // HPF right side connection pt
        static Point HPF_label = new Point(HPF.X + 10, HPF.Y + 17);
        static Point HPF_B = new Point(HPF.X + 25, HPF.Y + 50);
        static Point HPF_center = new Point(HPF.X + 25, ADC0.Y + 25);

        static Point HPF_TX_corner1 = new Point(SDR.X + 100, SDR.Y + 95);
        static Point HPF_TX_corner2 = new Point(SDR.X + 100, SDR.Y + 45);
        static Point HPF_TX_corner3 = new Point(SDR.X + 100, SDR.Y + 120);
        static Point HPF_TX_corner4 = new Point(SDR.X + 100, SDR.Y + 145);
        static Point HPF_TX_corner5 = new Point(SDR.X + 50, SDR.Y + 45);
        static Point HPF_TX_corner6 = new Point(SDR.X + 50, SDR.Y + 145);

        // HPF2 points
        static Point HPF2_L = new Point(HPF2.X, HPF2.Y + 25);                       // HPF2 left side
        static Point HPF2_L_c = new Point(HPF2.X - 20, HPF2.Y + 25);                  // HPF2 left side connection pt
        static Point HPF2_R = new Point(HPF2.X + 50, HPF2.Y + 25);                  // HPF2 right side
        static Point HPF2_R_c = new Point(HPF2.X + 70, HPF2.Y + 25);                  // HPF2 right side connection pt

        // loopback pts
        static Point loopback_center = new Point(DSP.X + 25, DSP.Y + 220);
        static Point loopback_center2 = new Point(DSP.X + 25, DSP_R_1.Y);

        // LPF points
        static Point LPF_L = new Point(LPF.X, LPF.Y + 25);                         // LPF left side
        static Point LPF_L_c = new Point(LPF.X - 20, LPF.Y + 25);                    // LPF left side connection pt
        static Point LPF_R = new Point(LPF.X + 50, LPF.Y + 25);                    // LPF right side
        static Point LPF_R_c = new Point(LPF.X + 70, LPF.Y + 25);                    // LPF right side connection pt
        static Point LPF_B = new Point(LPF.X + 25, LPF.Y + 50);
        static Point LPF_T = new Point(LPF.X + 25, LPF.Y);
        static Point LPF_in_corner = new Point(SDR.X + 120, LPF_L.Y);
        static Point LPF_R_corner = new Point(PA_T.X, LPF_R.Y);

        static Point LPF_corner_C2 = new Point(LPF.X + 25, C2.Y);
        static Point LPF_corner_C3 = new Point(LPF.X + 25, C3.Y);
        static Point LPF_corner_C4 = new Point(LPF.X + 25, C4.Y);
        static Point LPF_label = new Point(LPF.X + 10, LPF.Y + 17);
        static Point LPF_BYPASS_corner = new Point(LPF.X + 25, C7.Y);
        static Point LPF_HPF_corner = new Point(HPF.X + 25, LPF.Y + 25);

        // LPF2 points
        static Point LPF2_L = new Point(LPF2.X, LPF2.Y + 25);                       // LPF2 left side
        static Point LPF2_L_c = new Point(LPF2.X - 20, LPF2.Y + 25);                  // LPF2 left side connection pt
        static Point LPF2_R = new Point(LPF2.X + 50, LPF2.Y + 25);                  // LPF2 right side
        static Point LPF2_R_c = new Point(LPF2.X + 70, LPF2.Y + 25);                  // LPF2 right side connection pt

        // MERCURY points
        static Point MERCURY_label = new Point(MERCURY.X + 5, MERCURY.Y + 5);
        static Point MERCURY_ADC_label = new Point(MERCURY_ADC.X + 10, MERCURY_ADC.Y + 17);
        static Point MERCURY_ADC_in = new Point(MERCURY_ADC.X, MERCURY_ADC.Y + 25);
        static Point MERCURY_ADC_out = new Point(MERCURY_ADC.X + 50, MERCURY_ADC.Y + 25);
        static Point MERCURY_DDC0_label = new Point(MERCURY_DDC0.X + 5, MERCURY_DDC0.Y + 10);
        static Point MERCURY_DDC0_Rx0_label = new Point(MERCURY_DDC0.X + 5, MERCURY_DDC0.Y + 27);
        static Point MERCURY_DDC0_in = new Point(MERCURY_DDC0.X, MERCURY_DDC0.Y + 25);
        static Point MERCURY_DDC0_out = new Point(MERCURY_DDC0.X + 50, MERCURY_DDC0.Y + 25);
        static Point MERCURY_DDC1_label = new Point(MERCURY_DDC1.X + 5, MERCURY_DDC1.Y + 10);
        static Point MERCURY_DDC1_in = new Point(MERCURY_DDC1.X, MERCURY_DDC1.Y + 25);
        static Point MERCURY_DDC1_Rx1_label = new Point(MERCURY_DDC1.X + 5, MERCURY_DDC1.Y + 27);
        static Point MERCURY_DDC1_out = new Point(MERCURY_DDC1.X + 50, MERCURY_DDC1.Y + 25);
        static Point MERCURY_DDC2_label = new Point(MERCURY_DDC2.X + 5, MERCURY_DDC2.Y + 10);
        static Point MERCURY_DDC2_in = new Point(MERCURY_DDC2.X, MERCURY_DDC2.Y + 25);
        static Point MERCURY_DDC2_Rx2_label = new Point(MERCURY_DDC2.X + 5, MERCURY_DDC2.Y + 27);
        static Point MERCURY_CODEC_label = new Point(MERCURY_CODEC.X + 3, MERCURY_CODEC.Y + 17);
        static Point MERCURY_CODEC_LINEOUT = new Point(MERCURY.X, MERCURY_CODEC.Y + 10);
        static Point MERCURY_CODEC_OUT1 = new Point(MERCURY_CODEC.X, MERCURY_CODEC.Y + 10);
        static Point MERCURY_CODEC_PHONES = new Point(MERCURY.X, MERCURY_CODEC.Y + 40);
        static Point MERCURY_CODEC_OUT2 = new Point(MERCURY_CODEC.X, MERCURY_CODEC.Y + 40);
        static Point MERCURY_CODEC_IN = new Point(MERCURY_CODEC.X + 50, MERCURY_CODEC.Y + 40);
        static Point MERCURY_CODEC_corner1 = new Point(PC.X + 15, MERCURY_CODEC.Y + 40);
        static Point MERCURY_CODEC_corner2 = new Point(PC.X + 15, MERCURY_CODEC.Y + 150);
        static Point MERCURY_FPGA_label = new Point(MERCURY_FPGA.X + 5, MERCURY_FPGA.Y + 3);
        static Point MERCURY_FPGA_corner1 = new Point(MERCURY_FPGA.X + 5, MERCURY_DDC0_in.Y);
        static Point MERCURY_FPGA_corner2 = new Point(MERCURY_FPGA.X + 5, MERCURY_DDC1_in.Y);
        static Point MERCURY_FPGA_corner3 = new Point(MERCURY_FPGA.X + 5, MERCURY_DDC2_in.Y);

        static Point MERCURY_PHONES_label = new Point(MERCURY.X - 59, MERCURY.Y + 210);
        static Point MERCURY_P_OUT_label = new Point(MERCURY.X - 35, MERCURY.Y + 222);
        static Point MERCURY_LINE_label = new Point(MERCURY.X - 35, MERCURY.Y + 178);
        static Point MERCURY_OUT_label = new Point(MERCURY.X - 35, MERCURY.Y + 190);
        static Point MERCURY_ANT_label = new Point(MERCURY.X - 35, MERCURY.Y + 45);

        // MERCURY 2 points
        static Point MERCURY_2_label = new Point(MERCURY_2.X + 5, MERCURY_2.Y + 2);
        static Point MERCURY_2_ADC_label = new Point(MERCURY_2_ADC.X + 10, MERCURY_2_ADC.Y + 17);
        static Point MERCURY_2_ADC_in = new Point(MERCURY_2_ADC.X, MERCURY_2_ADC.Y + 25);
        static Point MERCURY_2_ADC_in_corner1 = new Point(MERCURY_2_ADC.X - 70, MERCURY_2_ADC.Y + 25);
        static Point MERCURY_2_ADC_out = new Point(MERCURY_2_ADC.X + 50, MERCURY_2_ADC.Y + 25);
        static Point MERCURY_2_DDC0_label = new Point(MERCURY_2_DDC0.X + 10, MERCURY_2_DDC0.Y + 10);
        static Point MERCURY_2_DDC0_Rx0_label = new Point(MERCURY_2_DDC0.X + 5, MERCURY_2_DDC0.Y + 27);
        static Point MERCURY_2_DDC0_in = new Point(MERCURY_2_DDC0.X, MERCURY_2_DDC0.Y + 25);
        static Point MERCURY_2_DDC0_out = new Point(MERCURY_2_DDC0.X + 50, MERCURY_2_DDC0.Y + 25);
        static Point MERCURY_2_DDC0_out_corner1 = new Point(MERCURY_2_DDC0.X + 210, MERCURY_2_DDC0.Y + 25);
        static Point MERCURY_2_DDC0_out_corner2 = new Point(MERCURY_2_DDC0.X + 210, DSP_internal_MIXER2_3.Y);

        static Point MERCURY_2_DDC1_label = new Point(MERCURY_2_DDC1.X + 10, MERCURY_2_DDC1.Y + 10);
        static Point MERCURY_2_DDC1_Rx1_label = new Point(MERCURY_2_DDC1.X + 5, MERCURY_2_DDC1.Y + 27);
        static Point MERCURY_2_DDC1_in = new Point(MERCURY_2_DDC1.X, MERCURY_2_DDC1.Y + 25);
        static Point MERCURY_2_DDC2_label = new Point(MERCURY_2_DDC2.X + 10, MERCURY_2_DDC2.Y + 10);
        static Point MERCURY_2_DDC2_in = new Point(MERCURY_2_DDC2.X, MERCURY_2_DDC2.Y + 25);
        static Point MERCURY_2_DDC2_Rx2_label = new Point(MERCURY_2_DDC2.X + 5, MERCURY_2_DDC2.Y + 27);
        static Point MERCURY_2_CODEC_label = new Point(MERCURY_2_CODEC.X + 3, MERCURY_2_CODEC.Y + 17);
        static Point MERCURY_2_CODEC_LINEOUT = new Point(MERCURY_2.X, MERCURY_2_CODEC.Y + 10);
        static Point MERCURY_2_CODEC_OUT1 = new Point(MERCURY_CODEC.X, MERCURY_2_CODEC.Y + 10);
        static Point MERCURY_2_CODEC_PHONES = new Point(MERCURY_2.X, MERCURY_2_CODEC.Y + 40);
        static Point MERCURY_2_CODEC_OUT2 = new Point(MERCURY_2_CODEC.X, MERCURY_2_CODEC.Y + 40);
        static Point MERCURY_2_CODEC_out = new Point(MERCURY_2_CODEC.X + 50, MERCURY_2_CODEC.Y + 38);
        static Point MERCURY_2_CODEC_corner1 = new Point(MERCURY_2_CODEC.X + 310, MERCURY_2_CODEC.Y + 38);
        static Point MERCURY_2_CODEC_corner2 = new Point(MERCURY_2_CODEC.X + 310, MERCURY_CODEC_IN.Y);
        static Point MERCURY_2_FPGA_label = new Point(MERCURY_2_FPGA.X + 5, MERCURY_2_FPGA.Y + 3);
        static Point MERCURY_2_FPGA_corner1 = new Point(MERCURY_2_FPGA.X + 5, MERCURY_2_DDC0_in.Y);
        static Point MERCURY_2_FPGA_corner2 = new Point(MERCURY_2_FPGA.X + 5, MERCURY_2_DDC1_in.Y);
        static Point MERCURY_2_FPGA_corner3 = new Point(MERCURY_2_FPGA.X + 5, MERCURY_2_DDC2_in.Y);

        static Point MERCURY_2_PHONES_label = new Point(MERCURY_2.X - 59, MERCURY_2.Y + 210);
        static Point MERCURY_2_P_OUT_label = new Point(MERCURY_2.X - 35, MERCURY_2.Y + 222);
        static Point MERCURY_2_LINE_label = new Point(MERCURY_2.X - 35, MERCURY_2.Y + 178);
        static Point MERCURY_2_OUT_label = new Point(MERCURY_2.X - 35, MERCURY_2.Y + 190);
        static Point MERCURY_2_ANT_label = new Point(MERCURY_2.X - 35, MERCURY_2.Y + 45);

        // METIS points
        static Point METIS_label = new Point(METIS.X + 5, METIS.Y + 5);
        static Point METIS_FPGA_label = new Point(METIS_FPGA.X + 5, METIS_FPGA.Y + 3);
        static Point METIS_corner1 = new Point(METIS.X + 40, MERCURY_2_DDC0_out_corner2.Y);
        static Point METIS_corner2 = new Point(METIS.X + 40, METIS.Y + 70);

        // MIC points
        static Point MIC_R = new Point(SDR.X, CODEC.Y + 25);                                // MIC output pt
        static Point MIC_R_c = new Point(SDR.X + 20, CODEC.Y + 25);                           // MIC output connection pt

        // PA points
        static Point PA_B = new Point(PA.X + 25, PA.Y + 50);                      // PA input pt
        static Point PA_T = new Point(PA.X + 25, PA.Y);                           // PA output pt
        static Point PA_corner = new Point(PA.X + 25, AMPF_L.Y);
        static Point PA15_corner = new Point(PA_T.X, LPF_R.Y);
        static Point PA15_corner2 = new Point(PA_B.X, AMPF_L.Y);
        static Point PA15_corner3 = new Point(PA_T.X, LPF_R.Y);

        // PC points
        static Point PC_PENELOPE_corner1 = new Point(PC.X + 40, PC.Y + 305);
        static Point PC_PENELOPE_corner2 = new Point(PC.X + 40, PC.Y + 275);
        static Point PC_PENELOPE_corner3 = new Point(PC.X + 70, PC.Y + 275);
        static Point PC_PENELOPE_corner4 = new Point(PC.X + 70, PC.Y + 361);

        //PENELOPE/PENNYLANE points
        static Point PENELOPE_label = new Point(PENELOPE.X + 5, PENELOPE.Y + 5);
        static Point PENELOPE_FPGA_label = new Point(PENELOPE_FPGA.X + 5, PENELOPE_FPGA.Y + 3);
        static Point PENELOPE_DAC_label = new Point(PENELOPE_DAC.X + 10, PENELOPE_DAC.Y + 17);
        static Point PENELOPE_DUC_label = new Point(PENELOPE_DUC.X + 10, PENELOPE_DUC.Y + 17);
        static Point PENELOPE_AMPF_label = new Point(PENELOPE_AMPF.X + 5, PENELOPE_AMPF.Y + 10);
        static Point PENELOPE_FILTER_label = new Point(PENELOPE_AMPF.X + 5, PENELOPE_AMPF.Y + 27);
        static Point PENELOPE_PA_label = new Point(PENELOPE_PA.X + 15, PENELOPE_PA.Y + 17);
        static Point PENELOPE_CODEC_label = new Point(PENELOPE_CODEC.X + 3, PENELOPE_CODEC.Y + 17);
        static Point PENELOPE_CODEC_LINE_IN_1 = new Point(PENELOPE_CODEC.X, PENELOPE_CODEC.Y + 15);
        static Point PENELOPE_CODEC_LINE_IN_2 = new Point(PENELOPE.X, PENELOPE_CODEC.Y + 15);
        static Point PENELOPE_CODEC_MIC_1 = new Point(PENELOPE_CODEC.X, PENELOPE_CODEC.Y + 40);
        static Point PENELOPE_CODEC_MIC_2 = new Point(PENELOPE.X, PENELOPE_CODEC.Y + 40);
        static Point PENELOPE_CODEC_R = new Point(PENELOPE_CODEC.X + 50, PENELOPE_CODEC.Y + 25);
        static Point PENELOPE_CODEC_L = new Point(PENELOPE_CODEC.X, PENELOPE_CODEC.Y + 25);
        static Point PENELOPE_ANT_label = new Point(PENELOPE.X - 33, PENELOPE.Y + 28);
        static Point PENELOPE_LINE_label = new Point(PENELOPE.X - 35, PENELOPE.Y + 90);
        static Point PENELOPE_IN_label = new Point(PENELOPE.X - 23, PENELOPE.Y + 102);
        static Point PENELOPE_MIC_label = new Point(PENELOPE.X - 30, PENELOPE.Y + 120);
        static Point PENELOPE_XVTR_label = new Point(PENELOPE.X - 40, PENELOPE.Y + 70);

        static Point PENELOPE_PA_out = new Point(PENELOPE_PA.X, PENELOPE_PA.Y + 25);
        static Point PENELOPE_PA_out_corner1 = new Point(PENELOPE.X - 230, PENELOPE_PA.Y + 25);
        static Point PENELOPE_PA_out_corner2 = new Point(PENELOPE.X - 230, PENELOPE_PA.Y - 10);
        static Point PENELOPE_PA_out_corner3 = new Point(ALEX_LPF.X + 40, PENELOPE_PA.Y - 10);
        static Point PENELOPE_PA_R = new Point(PENELOPE_PA.X + 50, PENELOPE_PA.Y + 25);
        static Point PENELOPE_PA_XVTR_1 = new Point(PENELOPE_PA.X + 60, PENELOPE_PA.Y + 25);
        static Point PENELOPE_PA_XVTR_2 = new Point(PENELOPE_PA.X + 60, PENELOPE_PA.Y + 56);
        static Point PENELOPE_PA_XVTR_3 = new Point(PENELOPE.X, PENELOPE_PA.Y + 56);
        static Point PENELOPE_AMPF_L = new Point(PENELOPE_AMPF.X, PENELOPE_AMPF.Y + 25);
        static Point PENELOPE_AMPF_R = new Point(PENELOPE_AMPF.X + 50, PENELOPE_AMPF.Y + 25);
        static Point PENELOPE_DAC_L = new Point(PENELOPE_DAC.X, PENELOPE_DAC.Y + 25);
        static Point PENELOPE_DAC_R = new Point(PENELOPE_DAC.X + 50, PENELOPE_DUC.Y + 25);
        static Point PENELOPE_DUC_L = new Point(PENELOPE_DUC.X, PENELOPE_DUC.Y + 25);
        static Point PENELOPE_DUC_R = new Point(PENELOPE_DUC.X + 50, PENELOPE_DUC.Y + 25);
        static Point PENELOPE_DSP_mid_1 = new Point(PC.X + 55, PENELOPE_DUC_R.Y - 30);
        static Point PENELOPE_DSP_mid_2 = new Point(PC.X + 55, MERCURY_DDC0_out.Y);

        static Point HERMES_PA_corner1 = new Point(PA_T.X, PENELOPE_PA_out_corner1.Y);


        // RX1 Display points
        static Point RX1_DISPLAY_L = new Point(RX1_DISPLAY.X, RX1_DISPLAY.Y + 25);         // RX1 Display input
        static Point RX1_DISPLAY_L_c = new Point(RX1_DISPLAY.X - 20, RX1_DISPLAY.Y + 25);    // Rx1 Display input connection pt
        static Point RX1_DISPLAY_corner1 = new Point(DSP_R_1_c.X, RX1_DISPLAY_L.Y);
        static Point RX1_DISPLAY_HPSDR_DDC0 = new Point(RX1_DISPLAY.X, MERCURY_DDC0_out.Y);

        // RX2 Display points
        static Point RX2_DISPLAY_L = new Point(RX2_DISPLAY.X, RX2_DISPLAY.Y + 25);         // RX2 Display input
        static Point RX2_DISPLAY_L_c = new Point(RX2_DISPLAY.X - 40, RX2_DISPLAY.Y + 25);    // Rx2 Display input connection pt
        static Point RX2_L_DSP_2 = new Point(RX2_DISPLAY.X, DSP_R_2.Y);
        static Point RX2_DISPLAY_HPSDR_DDC1 = new Point(RX2_DISPLAY.X, MERCURY_DDC1_out.Y);

        // Rx0 points
        static Point Rx0_L = new Point(Rx0.X, Rx0.Y + 25);                         // Rx0 left side
        static Point Rx0_L_c = new Point(Rx0.X - 20, Rx0.Y + 25);                    // Rx0 left side connection pt
        static Point Rx0_R = new Point(Rx0.X + 50, Rx0.Y + 25);                    // Rx0 right side
        static Point Rx0_R_c = new Point(Rx0.X + 70, Rx0.Y + 25);                    // Rx0 right side connection pt
        static Point Rx0_ADC1_input = new Point(Rx0.X, Rx0.Y + 35);                 // input point for connections to ADC1

        // Rx1 points
        static Point Rx1_L = new Point(Rx1.X, Rx1.Y + 25);                         // Rx1 left side
        static Point Rx1_L_c = new Point(Rx1.X - 20, Rx1.Y + 25);                    // Rx1 left side connection pt
        static Point Rx1_R = new Point(Rx1.X + 50, Rx1.Y + 25);                    // Rx1 right side
        static Point Rx1_R_c = new Point(Rx1.X + 70, Rx1.Y + 25);                    // Rx1 right side connection pt

        // Rx2 points
        static Point Rx2_L = new Point(Rx2.X, Rx2.Y + 25);                         // Rx2 left side
        static Point Rx2_L_c = new Point(Rx2.X - 20, Rx2.Y + 25);                    // Rx2 left side connection pt
        static Point Rx2_R = new Point(Rx2.X + 50, Rx2.Y + 25);                    // Rx2 right side
        static Point Rx2_R_c = new Point(Rx2.X + 70, Rx2.Y + 25);                    // Rx2 right side connection pt

        // Rx3 points
        static Point Rx3_L = new Point(Rx3.X, Rx3.Y + 25);                         // Rx3 left side
        static Point Rx3_L_c = new Point(Rx3.X - 20, Rx3.Y + 25);                    // Rx3 left side connection pt
        static Point Rx3_R = new Point(Rx3.X + 50, Rx3.Y + 25);                    // Rx3 right side
        static Point Rx3_R_c = new Point(Rx3.X + 70, Rx3.Y + 25);                    // Rx3 right side connection pt

        // Rx4 points
        static Point Rx4_L = new Point(Rx4.X, Rx4.Y + 25);                         // Rx4 left side
        static Point Rx4_L_c = new Point(Rx4.X - 20, Rx4.Y + 25);                    // Rx4 left side connection pt
        static Point Rx4_R = new Point(Rx4.X + 50, Rx4.Y + 25);                    // Rx4 right side
        static Point Rx4_R_c = new Point(Rx4.X + 70, Rx4.Y + 25);                    // Rx4 right side connection pt

        // Rx5 points
        static Point Rx5_L = new Point(Rx5.X, Rx5.Y + 25);                         // Rx5 left side
        static Point Rx5_L_c = new Point(Rx5.X - 20, Rx5.Y + 25);                    // Rx5 left side connection pt
        static Point Rx5_R = new Point(Rx5.X + 50, Rx5.Y + 25);                    // Rx5 right side
        static Point Rx5_R_c = new Point(Rx5.X + 70, Rx5.Y + 25);                    // Rx5 right side connection pt

        // Rx6 points
        static Point Rx6_L = new Point(Rx6.X, Rx6.Y + 25);                         // Rx6 left side
        static Point Rx6_L_c = new Point(Rx6.X - 20, Rx6.Y + 25);                    // Rx6 left side connection pt
        static Point Rx6_R = new Point(Rx6.X + 50, Rx6.Y + 25);                    // Rx6 right side
        static Point Rx6_R_c = new Point(Rx6.X + 70, Rx6.Y + 25);                    // Rx6 right side connection pt

        static Point TX_AMP_corner = new Point(C6.X + 20, AMPF_L.Y);
        static Point TX_corner_PA10 = new Point(C6.X + 20, C6.Y);
        static Point TX_corner_PA15 = new Point(C13.X + 20, C13.Y);

        // SPKR points
        static Point SPKR_corner1 = new Point(RX1_DISPLAY_L_c.X, C24.Y);
        static Point SPKR_corner2 = new Point(RX2_DISPLAY_L_c.X, C24.Y);
        static Point SPKR_DSP1 = new Point(SPKR_corner1.X, DSP_L_1.Y);
        static Point SPKR_DSP2 = new Point(SPKR_corner1.X, DSP_L_2.Y);

        // SWR points
        static Point SWR_L = new Point(SWR.X, SWR.Y + 25);
        static Point SWR_R = new Point(SWR.X + 50, SWR.Y + 25);
        static Point SWR_T = new Point(SWR.X + 25, SWR.Y);
        static Point SWR_corner_ADC0 = new Point(SDR.X + 75, ADC0_L.Y);

        //XVTR points
        static Point XVTR_HPF_corner = new Point(HPF_B.X, C4.Y);


        // END OF PT DEFINITIONS *****************************************************************************************
        
        
        
        public Path_Illustrator(Console c)
        {
            console = c;

            this.Resize += PI_Resize;
            this.Disposed += PI_Disposed;

            InitializeComponent();
            g = canvas.CreateGraphics();    // create a graphics object called canvas
            redPen.Width = 3;               // use a bold red pen for signal path segments
            bluePen.Width = 3;
            blackPen2.Width = 2;

            // place labels into position
            label_LPF.Location = new Point(LPF.X + 12, LPF.Y + 15);
            label_LPF2.Location = new Point(LPF2.X + 12, LPF2.Y + 15);
            label_HPF.Location = new Point(HPF.X + 12, HPF.Y + 15);
            label_HPF2.Location = new Point(HPF2.X + 12, HPF2.Y + 15);
            label_ADC0.Location = new Point(ADC0.X + 5, ADC0.Y + 25);
            label_ADC0_atten.Location = new Point(ADC0.X + 1, ADC0.Y + 7);
            label_ADC1.Location = new Point(ADC1.X + 5, ADC1.Y + 25);
            label_ADC1_atten.Location = new Point(ADC1.X + 1, ADC1.Y + 7);
            label_ADC2.Location = new Point(ADC2.X + 5, ADC2.Y + 25);
            label_ADC2_atten.Location = new Point(ADC2.X + 1, ADC2.Y + 7);
            label_AUDIO_MIXER.Location = new Point(AUDIO_MIXER.X + 110, AUDIO_MIXER.Y + 18);
            label_RX1_LR_audio.Location = new Point(AUDIO_MIXER.X + 2, AUDIO_MIXER.Y + 2);
            label_RX2_LR_audio.Location = new Point(AUDIO_MIXER.X + 2, AUDIO_MIXER.Y + 33);
            label_Rx0.Location = new Point(Rx0.X + 6, Rx0.Y + 28);
            label_DDC0.Location = new Point(Rx0.X + 8, Rx0.Y + 11);
            label_Rx1.Location = new Point(Rx1.X + 6, Rx1.Y + 28);
            label_DDC1.Location = new Point(Rx1.X + 8, Rx1.Y + 11);
            label_Rx2.Location = new Point(Rx2.X + 6, Rx2.Y + 28);
            label_DDC2.Location = new Point(Rx2.X + 8, Rx2.Y + 11);
            label_Rx3.Location = new Point(Rx3.X + 6, Rx3.Y + 28);
            label_DDC3.Location = new Point(Rx3.X + 8, Rx3.Y + 11);
            label_Rx4.Location = new Point(Rx4.X + 6, Rx4.Y + 28);
            label_DDC4.Location = new Point(Rx4.X + 8, Rx4.Y + 11);
            label_Rx5.Location = new Point(Rx5.X + 6, Rx5.Y + 28);
            label_DDC5.Location = new Point(Rx5.X + 8, Rx5.Y + 11);
            label_Rx6.Location = new Point(Rx6.X + 6, Rx6.Y + 28);
            label_DDC6.Location = new Point(Rx6.X + 8, Rx6.Y + 11);
            label_RX1_DISPLAY.Location = new Point(RX1_DISPLAY_x + 70, RX1_DISPLAY_y + 35);
            label_RX2_DISPLAY.Location = new Point(RX2_DISPLAY_x + 70, RX2_DISPLAY_y + 35);
            label_DAC0.Location = new Point(DAC0.X + 9, DAC0.Y + 18);
            label_DUC0.Location = new Point(DUC0.X + 9, DUC0.Y + 18);
            label_PA.Location = new Point(PA.X + 15, PA.Y + 17);
            label_AMP.Location = new Point(AMPF.X + 5, AMPF.Y + 7);
            label_FILTER.Location = new Point(AMPF.X + 5, AMPF.Y + 22);
            label_SDR_Hardware.Location = new Point(SDR.X + 5, SDR.Y + 5);
            label_CODEC.Location = new Point(CODEC.X + 3, CODEC.Y + 18);
            label_CODEC2.Location = new Point(CODEC2.X + 3, CODEC2.Y + 18);
            label_FPGA.Location = new Point(FPGA.X + 5, FPGA.Y + 5);
            label_FPGA.ForeColor = System.Drawing.Color.IndianRed;
            label_PC.Location = new Point(PC.X + 5, PC.Y + 5);
            label_DSP.Location = new Point(DSP.X + 10, DSP.Y + 1);
            label_front_panel.Location = new Point(9, 580);

            do_platform_prep();
            update_diagram = true;
            canvas.Invalidate();
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            Update_control_settings();

            if (bool_HPSDR & update_diagram) draw_HPSDR();
            if (bool_HERMES & update_diagram) draw_HERMES();
            if (bool_ANAN_10E & update_diagram) draw_ANAN_10E();                            // draw ANAN-10E
            //if (rb_ANAN_10.Checked & update_diagram) draw_ANAN_10();                              // draw ANAN-10
            if (bool_ANAN_100_PA_rev15 & update_diagram) draw_ANAN_100_PA_rev15();          // draw ANAN-100_PA_rev15/16
            if (bool_ANAN_100_PA_rev24 & update_diagram) draw_ANAN_100_PA_rev24();          // draw ANAN_100_PA_rev24
            if (bool_ANAN_100D_PA_rev15 & update_diagram) draw_ANAN_100D_PA_rev15();        // draw ANAN_100D_PA_rev15
            if (bool_ANAN_100D_PA_rev24 & update_diagram) draw_ANAN_100D_PA_rev24();        // draw ANAN_100D_PA_rev24

        }

        private void Update_control_settings()
        {
            bool_HPSDR = HardwareSpecific.Model == HPSDRModel.HPSDR;
            bool_HERMES = HardwareSpecific.Model == HPSDRModel.HERMES;
            bool_disable_BYPASS = console.SetupForm.ChkDisableRXOut;                                    // ALEX reg bit 11, also used as primary indicator of whether PA24 or PA15/16 is present
            bool_ANAN_10E = HardwareSpecific.Model == HPSDRModel.ANAN10 || HardwareSpecific.Model == HPSDRModel.ANAN10E;                                           // covers both 10/10E
            bool_ANAN_100_PA_rev15 = ((HardwareSpecific.Model == HPSDRModel.ANAN100 || HardwareSpecific.Model == HPSDRModel.ANAN100B) & !bool_disable_BYPASS);     // covers 100/100B w/PA_rev15/16
            bool_ANAN_100_PA_rev24 = ((HardwareSpecific.Model == HPSDRModel.ANAN100 || HardwareSpecific.Model == HPSDRModel.ANAN100B) & bool_disable_BYPASS);      // covers 100/100B w/PA_rev24
            bool_ANAN_100D_PA_rev15 = ((HardwareSpecific.Model == HPSDRModel.ANAN100D || HardwareSpecific.Model == HPSDRModel.ANAN200D) & !bool_disable_BYPASS);   // covers 100D/200D w/PA_rev15/16
            bool_ANAN_100D_PA_rev24 = ((HardwareSpecific.Model == HPSDRModel.ANAN100D || HardwareSpecific.Model == HPSDRModel.ANAN200D) & bool_disable_BYPASS);    // covers 100D/200D w/PA_rev24

            bool_rx = rb_rx.Checked;                                                            // Rx/Tx local control for display

            // handle ANT selections
            int_RxAnt_switch = console.SetupForm.PI_RxAnt;
            int_TxAnt_switch = console.SetupForm.PI_TxAnt;

            if (bool_rx)
            {
                switch (int_RxAnt_switch)
                {
                    case 0:
                    case 1: bool_ANT1 = true;
                        bool_ANT2 = false;
                        bool_ANT3 = false;
                        break;

                    case 2: bool_ANT1 = false;
                        bool_ANT2 = true;
                        bool_ANT3 = false;
                        break;

                    case 3: bool_ANT1 = false;
                        bool_ANT2 = false;
                        bool_ANT3 = true;
                        break;

                    default: bool_ANT1 = true;
                        bool_ANT2 = false;
                        bool_ANT3 = false;
                        break;

                }
            }
            else
            {
                switch (int_TxAnt_switch)
                {
                    case 0:
                    case 1: bool_ANT1_TX = true;
                        bool_ANT2_TX = false;
                        bool_ANT3_TX = false;
                        break;

                    case 2: bool_ANT1_TX = false;
                        bool_ANT2_TX = true;
                        bool_ANT3_TX = false;
                        break;

                    case 3: bool_ANT1_TX = false;
                        bool_ANT2_TX = false;
                        bool_ANT3_TX = true;
                        break;

                    default: bool_ANT1_TX = true;
                        bool_ANT2_TX = false;
                        bool_ANT3_TX = false;
                        break;
                }

            }

            // additional user option selections
            bool_MON = console.MON;
            bool_RX1_MUTE = console.MUT;
            bool_RX2_MUTE = console.MUT2;
            bool_duplex = Display.DisplayDuplex;
            // bool_PureSignal = !console.SetupForm.PureSignalDisabled;
            bool_PureSignal = console.psform.PSEnabled;
            bool_diversity = console.Diversity2;
            bool_DUAL_MERCURY_ALEX = cb_DUAL_MERCURY_ALEX.Checked;

            int RX_ONLY = console.SetupForm.Alex_EXT2EXT1XVTR;
            if (RX_ONLY == 0)
            {
                bool_XVTR = false;
                bool_EXT1 = false;
                bool_EXT2 = false;
            }
 
            if (RX_ONLY == 1)
            {
                bool_XVTR = false;
                bool_EXT1 = false;
                bool_EXT2 = true;
            }

            if (RX_ONLY == 2)
            {
                bool_XVTR = false;
                bool_EXT1 = true;
                bool_EXT2 = false;
            }

            if (RX_ONLY == 3)
            {
                bool_XVTR = true;
                bool_EXT1 = false;
                bool_EXT2 = false;
            }

            bool_RX1_OUT_on_TX = console.SetupForm.ChkRxOutOnTx;
            bool_RX1_IN_on_TX = console.SetupForm.ChkRx1InOnTx;
            bool_RX2_IN_on_TX = console.SetupForm.ChkRx2InOnTx;

            bool_HPF_BYPASS = console.SetupForm.RadBPHPFled | console.AlexHPFBypass;
            bool_DisableHPFOnTx = console.SetupForm.ChkDisableHPFOnTx;

            if (bool_rx)
            {

            }
            else
            {
                if (bool_RX1_OUT_on_TX)
                {
                    bool_BYPASS_on_TX = true;
                    bool_EXT1_on_TX = false;
                    bool_EXT2_on_TX = false;
                }
                if (bool_RX1_IN_on_TX)
                {
                    bool_BYPASS_on_TX = false;
                    bool_EXT1_on_TX = true;
                    bool_EXT2_on_TX = false;
                }
                if (bool_RX2_IN_on_TX)
                {
                    bool_BYPASS_on_TX = false;
                    bool_EXT1_on_TX = false;
                    bool_EXT2_on_TX = true;
                }
                if (!bool_RX1_OUT_on_TX & !bool_RX1_IN_on_TX & !bool_RX2_IN_on_TX)
                {
                    bool_BYPASS_on_TX = false;
                    bool_EXT1_on_TX = false;
                    bool_EXT2_on_TX = false;
                }
            }

            bool_Rx0_0 = console.SetupForm.RadRX1ADC1;
            bool_Rx0_1 = console.SetupForm.RadRX1ADC2;

            bool_Rx1_0 = console.SetupForm.RadRX2ADC1;
            bool_Rx1_1 = console.SetupForm.RadRX2ADC2;

            bool_Rx2_0 = console.SetupForm.RadRX3ADC1;
            bool_Rx2_1 = console.SetupForm.RadRX3ADC2;

            bool_Rx3_0 = console.SetupForm.RadRX4ADC1;
            bool_Rx3_1 = console.SetupForm.RadRX4ADC2;

            bool_Rx4_0 = console.SetupForm.RadRX5ADC1;
            bool_Rx4_1 = console.SetupForm.RadRX5ADC2;

            bool_Rx5_0 = console.SetupForm.RadRX6ADC1;
            bool_Rx5_1 = console.SetupForm.RadRX6ADC2;

            bool_Rx6_0 = console.SetupForm.RadRX7ADC1;
            bool_Rx6_1 = console.SetupForm.RadRX7ADC2;

        }

        // begin hardware-specific diagram drawing methods *******************************************************************

        private void draw_HPSDR()
        {
            hide_controls();
            cb_DUAL_MERCURY_ALEX.Visible = true;
            cb_DUAL_MERCURY_ALEX.Text = "DUAL MERCURY/ALEX";

            Update_control_settings();

            // EXT2 and EXT1 are switched on this model in Thetis checkbox logic, so switch them here
            bool bool_temp = bool_EXT1;
            bool_EXT1 = bool_EXT2;
            bool_EXT2 = bool_temp;

            hide_all_labels();
            label_hardware_selected.Text = "Routing for HPSDR";
            label_PC.Visible = true;
            label_RX1_DISPLAY.Visible = true;
            //label_RX2_DISPLAY.Visible = true;
            if (bool_XVTR) label_XVTR_VHF.Visible = true;

            // label ALEX rectangles            
            label_C2.Text = "XV RX IN";
            label_C2.Visible = true;
            label_C2.Location = C2_label_HPSDR;

            label_C3.Text = "RX 2 IN";
            label_C3.Visible = true;
            label_C3.Location = C3_label_HPSDR;

            label_C4.Text = "RX 1 IN";
            label_C4.Visible = true;
            label_C4.Location = C4_label_HPSDR;

            label_C5.Text = "RX 1 OUT";
            label_C5.Visible = true;
            label_C5.Location = C5_label_HPSDR;

            label_C7.Text = "ANT 1";
            label_C7.Visible = true;
            label_C7.Location = C7_label_HPSDR;

            label_C8.Text = "ANT 2";
            label_C8.Visible = true;
            label_C8.Location = C8_label_HPSDR;

            label_C9.Text = "ANT 3";
            label_C9.Visible = true;
            label_C9.Location = C9_label_HPSDR;

            label_C10.Text = "From TX*";
            label_C10.Visible = true;
            label_C10.Location = C10_label_ALEX_TX_IN;

            label_DSP_HPSDR.Visible = true;
            label_DSP_HPSDR.Location = DSP_HPSDR_label;
            if (!bool_rx)
            {
                label_ext_amp1.Visible = true;
                label_ext_amp1.Location = ext_amp_label1;
                label_ext_amp2.Visible = true;
                label_ext_amp2.Location = ext_amp_label2;
            }

            label_ALEX.Visible = true;
            label_ALEX.Location = ALEX_label;
            if (!bool_HPF_BYPASS) label_ALEX_HPF.Visible = true;
            label_ALEX_HPF.Location = ALEX_HPF_label;
            label_ALEX_LPF.Visible = true;
            label_ALEX_LPF.Location = ALEX_LPF_label;
            label_ALEX_To_RX.Visible = true;
            label_ALEX_To_RX.Location = ALEX_To_RX_label;

            if (cb_DUAL_MERCURY_ALEX.Checked)
            {
                // label ALEX 2 rectangle
                label_C16.Text = "XV RX IN";
                label_C16.Visible = true;
                label_C16.Location = C16_ALEX_2_label;

                label_C17.Text = "RX 2 IN";
                label_C17.Visible = true;
                label_C17.Location = C17_ALEX_2_label;

                label_C18.Text = "RX 1 IN";
                label_C18.Visible = true;
                label_C18.Location = C18_ALEX_2_label;

                label_C19.Text = "RX 1 OUT";
                label_C19.Visible = true;
                label_C19.Location = C19_ALEX_2_label;

                label_C20.Text = "ANT 1";
                label_C20.Visible = true;
                label_C20.Location = C20_ALEX_2_label;

                label_C24.Text = "ANT 2";
                label_C24.Visible = true;
                label_C24.Location = C24_ALEX_2_label;

                label_C25.Text = "ANT 3";
                label_C25.Visible = true;
                label_C25.Location = C25_ALEX_2_label;

                label_C26.Text = "From Tx";
                label_C26.Visible = true;
                label_C26.Location = C26_ALEX_2_label;

                label_ALEX_2.Visible = true;
                label_ALEX_2.Location = ALEX_2_label;
                if (!bool_HPF_BYPASS) label_ALEX_2_HPF.Visible = true;
                label_ALEX_2_HPF.Location = ALEX_2_HPF_label;
                label_ALEX_2_LPF.Visible = true;
                label_ALEX_2_LPF.Location = ALEX_2_LPF_label;
                label_ALEX_2_To_RX.Visible = true;
                label_ALEX_2_To_RX.Location = ALEX_2_To_RX_label;

                cb_DUAL_MERCURY_ALEX.Text = "DUAL MERCURY/ALEX *";
                label_DUAL_MERCURY.Visible = true;
            }

            // label Mercury rectangles
            label_MERCURY.Visible = true;
            label_MERCURY.Location = MERCURY_label;
            label_MERCURY_FPGA.Visible = true;
            label_MERCURY_FPGA.Visible = true;
            label_MERCURY_FPGA.ForeColor = System.Drawing.Color.IndianRed;
            label_MERCURY_FPGA.Location = MERCURY_FPGA_label;
            label_MERCURY_ADC.Visible = true;
            label_MERCURY_ADC.Location = MERCURY_ADC_label;
            label_MERCURY_DDC0.Visible = true;
            label_MERCURY_DDC0.Location = MERCURY_DDC0_label;
            label_MERCURY_Rx0.Visible = true;
            label_MERCURY_Rx0.Location = MERCURY_DDC0_Rx0_label;
            label_MERCURY_DDC1.Visible = true;
            label_MERCURY_DDC1.Location = MERCURY_DDC1_label;
            label_MERCURY_Rx1.Visible = true;
            label_MERCURY_Rx1.Location = MERCURY_DDC1_Rx1_label;
            label_MERCURY_DDC2.Visible = true;
            label_MERCURY_DDC2.Location = MERCURY_DDC2_label;
            label_MERCURY_Rx2.Visible = true;
            label_MERCURY_Rx2.Location = MERCURY_DDC2_Rx2_label;
            label_MERCURY_CODEC.Visible = true;
            label_MERCURY_CODEC.Location = MERCURY_CODEC_label;
            label_MERCURY_PHONES.Visible = true;
            label_MERCURY_PHONES.Location = MERCURY_PHONES_label;
            label_MERCURY_P_OUT.Visible = true;
            label_MERCURY_P_OUT.Location = MERCURY_P_OUT_label;
            label_MERCURY_LINE.Visible = true;
            label_MERCURY_LINE.Location = MERCURY_LINE_label;
            label_MERCURY_OUT.Visible = true;
            label_MERCURY_OUT.Location = MERCURY_OUT_label;
            label_MERCURY_ANT.Visible = true;
            label_MERCURY_ANT.Location = MERCURY_ANT_label;

            if (cb_DUAL_MERCURY_ALEX.Checked)
            {
                // label MERCURY 2 rectangles
                label_MERCURY_2.Visible = true;
                label_MERCURY_2.Location = MERCURY_2_label;
                label_MERCURY_2_FPGA.Visible = true;
                label_MERCURY_2_FPGA.ForeColor = System.Drawing.Color.IndianRed;
                label_MERCURY_2_FPGA.Location = MERCURY_2_FPGA_label;
                label_MERCURY_2_ADC.Visible = true;
                label_MERCURY_2_ADC.Location = MERCURY_2_ADC_label;
                label_MERCURY_2_DDC0.Visible = true;
                label_MERCURY_2_DDC0.Location = MERCURY_2_DDC0_label;
                label_MERCURY_2_Rx0.Visible = true;
                label_MERCURY_2_Rx0.Location = MERCURY_2_DDC0_Rx0_label;
                label_MERCURY_2_DDC1.Visible = true;
                label_MERCURY_2_DDC1.Location = MERCURY_2_DDC1_label;
                label_MERCURY_2_Rx1.Visible = true;
                label_MERCURY_2_Rx1.Location = MERCURY_2_DDC1_Rx1_label;
                label_MERCURY_2_DDC2.Visible = true;
                label_MERCURY_2_DDC2.Location = MERCURY_2_DDC2_label;
                label_MERCURY_2_Rx2.Visible = true;
                label_MERCURY_2_Rx2.Location = MERCURY_2_DDC2_Rx2_label;
                label_MERCURY_2_CODEC.Visible = true;
                label_MERCURY_2_CODEC.Location = MERCURY_2_CODEC_label;
                label_MERCURY_2_PHONES.Visible = true;
                label_MERCURY_2_PHONES.Location = MERCURY_2_PHONES_label;
                label_MERCURY_2_P_OUT.Visible = true;
                label_MERCURY_2_P_OUT.Location = MERCURY_2_P_OUT_label;
                label_MERCURY_2_LINE.Visible = true;
                label_MERCURY_2_LINE.Location = MERCURY_2_LINE_label;
                label_MERCURY_2_OUT.Visible = true;
                label_MERCURY_2_OUT.Location = MERCURY_2_OUT_label;
                label_MERCURY_2_ANT.Visible = true;
                label_MERCURY_2_ANT.Location = MERCURY_2_ANT_label;
            }


            //label METIS rectangle
            label_METIS.Visible = true;
            label_METIS.Location = METIS_label;
            label_METIS_FPGA.Visible = true;
            label_METIS_FPGA.ForeColor = System.Drawing.Color.IndianRed;
            label_METIS_FPGA.Location = METIS_FPGA_label;


            // label PENELOPE rectangles
            label_PENELOPE.Visible = true;
            label_PENELOPE.Location = PENELOPE_label;
            label_PENELOPE_FPGA.Visible = true;
            label_PENELOPE_FPGA.ForeColor = System.Drawing.Color.IndianRed;
            label_PENELOPE_FPGA.Location = PENELOPE_FPGA_label;
            label_PENELOPE_DAC.Visible = true;
            label_PENELOPE_DAC.Location = PENELOPE_DAC_label;
            label_PENELOPE_DUC.Visible = true;
            label_PENELOPE_DUC.Location = PENELOPE_DUC_label;
            label_PENELOPE_AMPF.Visible = true;
            label_PENELOPE_AMPF.Location = PENELOPE_AMPF_label;
            label_PENELOPE_FILTER.Visible = true;
            label_PENELOPE_FILTER.Location = PENELOPE_FILTER_label;
            label_PENELOPE_PA.Visible = true;
            label_PENELOPE_PA.Location = PENELOPE_PA_label;
            label_PENELOPE_CODEC.Visible = true;
            label_PENELOPE_CODEC.Location = PENELOPE_CODEC_label;
            label_PENELOPE_ANT.Visible = true;
            label_PENELOPE_ANT.Location = PENELOPE_ANT_label;
            label_PENELOPE_LINE.Visible = true;
            label_PENELOPE_LINE.Location = PENELOPE_LINE_label;
            label_PENELOPE_IN.Visible = true;
            label_PENELOPE_IN.Location = PENELOPE_IN_label;
            label_PENELOPE_MIC.Visible = true;
            label_PENELOPE_MIC.Location = PENELOPE_MIC_label;
            label_PENELOPE_XVTR.Visible = true;
            label_PENELOPE_XVTR.Location = PENELOPE_XVTR_label;


            // draw HPSDR rectangles
            g.DrawRectangle(blackPen2, ALEX);
            if (!bool_HPF_BYPASS) g.DrawRectangle(blackPen, ALEX_HPF);
            g.DrawRectangle(blackPen, ALEX_LPF);
            g.DrawRectangle(blackPen2, MERCURY);
            g.DrawRectangle(indianredPen, MERCURY_FPGA);
            g.DrawRectangle(blackPen, MERCURY_DDC0);
            g.DrawRectangle(blackPen, MERCURY_DDC1);
            g.DrawRectangle(blackPen, MERCURY_DDC2);
            g.DrawRectangle(blackPen, MERCURY_ADC);
            g.DrawRectangle(blackPen, MERCURY_CODEC);
            g.DrawRectangle(blackPen2, PENELOPE);
            g.DrawRectangle(indianredPen, PENELOPE_FPGA);
            g.DrawRectangle(blackPen, PENELOPE_PA);
            g.DrawRectangle(blackPen, PENELOPE_AMPF);
            g.DrawRectangle(blackPen, PENELOPE_DAC);
            g.DrawRectangle(blackPen, PENELOPE_DUC);
            g.DrawRectangle(blackPen, PENELOPE_CODEC);

            if (cb_DUAL_MERCURY_ALEX.Checked)
            {
                g.DrawRectangle(blackPen2, ALEX_2);
                if (!bool_HPF_BYPASS) g.DrawRectangle(blackPen, ALEX_2_HPF);
                g.DrawRectangle(blackPen, ALEX_2_LPF);
                g.DrawRectangle(blackPen2, MERCURY_2);
                g.DrawRectangle(indianredPen, MERCURY_2_FPGA);
                g.DrawRectangle(blackPen, MERCURY_2_DDC0);
                g.DrawRectangle(blackPen, MERCURY_2_DDC1);
                g.DrawRectangle(blackPen, MERCURY_2_DDC2);
                g.DrawRectangle(blackPen, MERCURY_2_ADC);
                g.DrawRectangle(blackPen, MERCURY_2_CODEC);
            }

            // draw PC rectangles
            g.DrawRectangle(blackPen2, PC);
            g.DrawRectangle(blackPen, DSP_HPSDR);
            g.DrawRectangle(blackPen, RX1_DISPLAY);
            //g.DrawRectangle(blackPen, AUDIO_MIXER);
            g.DrawRectangle(blackPen2, METIS);
            g.DrawRectangle(indianredPen, METIS_FPGA);

            if (bool_rx)          // RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX
            {
                ALEX_ANT_to_HPF_B(bluePen);
                g.DrawLine(bluePen, ALEX_RX_out, MERCURY_ADC_in);

                // show only RX components
                // handle rf paths from DDCs
                MERCURY_ADC_to_DDCs(bluePen);
                MERCURY_DDC0_to_RX1_DISPLAY(bluePen);
                if (bool_DUAL_MERCURY_ALEX)                     // dual Mercury
                {
                    ALEX_2_ANT_to_HPF_B(bluePen);
                    ALEX_2_RX_out_to_ADC(bluePen);
                    MERCURY_2_ADC_to_DDCs(bluePen);
                    MERCURY_2_AUDIO_INPUT(bluePen);
                    if (bool_diversity)                       // diversity
                    {
                        MERCURY_2_DDC0_to_METIS_diversity(bluePen);
                    }
                    else                                       // not diversity
                    {
                        g.DrawRectangle(blackPen, RX2_DISPLAY);
                        label_RX2_DISPLAY.Visible = true;
                        MERCURY_2_DDC0_to_RX2_DISPLAY(bluePen);
                    }
                }
                else                                           // single Mercury
                {
                    MERCURY_DDC1_to_RX2_DISPLAY(bluePen);
                    g.DrawRectangle(blackPen, RX2_DISPLAY);
                    label_RX2_DISPLAY.Visible = true;
                }

                // handle audio paths to Mercury(s)
                if (!bool_RX1_MUTE)
                {
                    label_RX1_LR_audio.Visible = true;
                    MERCURY_RX1_to_AUDIO_MIXER(bluePen);
                    HPSDR_DSP_to_AUDIO_MIXER_input_1(bluePen);
                }
                else label_RX1_LR_audio.Visible = false;
                if (!bool_RX2_MUTE)
                {
                    if (!bool_diversity)
                    {
                        label_RX2_LR_audio.Visible = true;
                        MERCURY_RX2_to_AUDIO_MIXER(bluePen);
                        HPSDR_DSP_to_AUDIO_MIXER_input_2(bluePen);
                    }
                }
                else label_RX2_LR_audio.Visible = false;

                if (bool_RX1_MUTE & bool_RX2_MUTE)
                {
                    label_AUDIO_MIXER.Visible = false;
                    label_RX1_LR_audio.Visible = false;
                    label_RX2_LR_audio.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                }

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2)
                {
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_HPF_B);
                    if (bool_DUAL_MERCURY_ALEX)
                    {
                        g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
                        g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_HPF_B);
                    }
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2)
                {
                    if (bool_disable_BYPASS)
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
                    }
                    else
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
                        g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
                    }
                    g.DrawLine(bluePen, ALEX_HPF_corner3, ALEX_HPF_corner1);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_XV_RX_IN, ALEX_HPF_corner3);
                    if (bool_DUAL_MERCURY_ALEX)
                    {
                        if (bool_disable_BYPASS)
                        {
                            g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner1);
                        }
                        else
                        {
                            g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner2);
                            g.DrawLine(bluePen, ALEX_2_HPF_corner2, ALEX_2_RX1_out);
                        }
                        g.DrawLine(bluePen, ALEX_2_HPF_corner3, ALEX_2_HPF_corner1);
                        g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
                        g.DrawLine(bluePen, ALEX_2_XV_RX_IN, ALEX_2_HPF_corner3);
                    }
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2)
                {
                    if (bool_disable_BYPASS)
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
                    }
                    else
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
                        g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
                    }
                    g.DrawLine(bluePen, ALEX_HPF_corner5, ALEX_HPF_corner1);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_RX_1_IN, ALEX_HPF_corner5);
                    if (bool_DUAL_MERCURY_ALEX)
                    {
                        if (bool_disable_BYPASS)
                        {
                            g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner1);
                        }
                        else
                        {
                            g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner2);
                            g.DrawLine(bluePen, ALEX_2_HPF_corner2, ALEX_2_RX1_out);
                        }
                        g.DrawLine(bluePen, ALEX_2_HPF_corner5, ALEX_2_HPF_corner1);
                        g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
                        g.DrawLine(bluePen, ALEX_2_RX_1_IN, ALEX_2_HPF_corner5);
                    }
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2)
                {
                    if (bool_disable_BYPASS)
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
                    }
                    else
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
                        g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
                    }
                    g.DrawLine(bluePen, ALEX_HPF_corner4, ALEX_HPF_corner1);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_RX_2_IN, ALEX_HPF_corner4);
                    if (bool_DUAL_MERCURY_ALEX)
                    {
                        if (bool_disable_BYPASS)
                        {
                            g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner1);
                        }
                        else
                        {
                            g.DrawLine(bluePen, ALEX_2_HPF_B, ALEX_2_HPF_corner2);
                            g.DrawLine(bluePen, ALEX_2_HPF_corner2, ALEX_2_RX1_out);
                        }
                        g.DrawLine(bluePen, ALEX_2_HPF_corner4, ALEX_2_HPF_corner1);
                        g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
                        g.DrawLine(bluePen, ALEX_2_RX_2_IN, ALEX_2_HPF_corner4);
                    }
                }

            }
            else                       // TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX
            {
                PENELOPE_PA_to_ALEX_LPF(redPen);
                ALEX_TX_ANT(redPen);
                PENELOPE_PA_to_DSP(redPen);
                PENELOPE_DSP_to_CODEC(redPen);

                if (bool_MON)
                {
                    //handle audio mute paths
                    if (!bool_RX1_MUTE)
                    {
                        label_RX1_LR_audio.Visible = true;
                        MERCURY_RX1_to_AUDIO_MIXER(bluePen);
                        HPSDR_DSP_to_AUDIO_MIXER_input_1(bluePen);
                    }
                    else label_RX1_LR_audio.Visible = false;

                    if (bool_RX1_MUTE & bool_RX2_MUTE)
                    {
                        label_AUDIO_MIXER.Visible = false;
                        label_RX1_LR_audio.Visible = false;
                        label_RX2_LR_audio.Visible = false;
                    }
                    else
                    {
                        if (!bool_RX1_MUTE)
                        {
                            g.DrawRectangle(blackPen, AUDIO_MIXER);
                            label_AUDIO_MIXER.Visible = true;
                        }
                    }

                    if (bool_DUAL_MERCURY_ALEX & !bool_RX1_MUTE) MERCURY_2_AUDIO_INPUT(bluePen);    // additional audio connection if dual MERCURY/ALEX present


                } // if (bool_MON)


                if (bool_EXT1_on_TX)
                {
                    g.DrawLine(bluePen, ALEX_HPF_corner5, ALEX_HPF_corner1);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_RX_1_IN, ALEX_HPF_corner5);
                    if (bool_DUAL_MERCURY_ALEX)
                    {
                        g.DrawLine(bluePen, ALEX_2_HPF_corner5, ALEX_2_HPF_corner1);
                        g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
                        g.DrawLine(bluePen, ALEX_2_RX_1_IN, ALEX_2_HPF_corner5);
                    }

                }

                if (bool_EXT2_on_TX)
                {
                    g.DrawLine(bluePen, ALEX_HPF_corner4, ALEX_HPF_corner1);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_RX_2_IN, ALEX_HPF_corner4);
                    if (bool_DUAL_MERCURY_ALEX)
                    {
                        g.DrawLine(bluePen, ALEX_2_HPF_corner4, ALEX_2_HPF_corner1);
                        g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
                        g.DrawLine(bluePen, ALEX_2_RX_2_IN, ALEX_2_HPF_corner4);
                    }

                }

                if (bool_duplex)
                {
                    g.DrawLine(bluePen, ALEX_RX_out, MERCURY_ADC_in);
                    MERCURY_ADC_to_DDCs(bluePen);
                    MERCURY_DDC0_to_RX1_DISPLAY(bluePen);
                    if (bool_DUAL_MERCURY_ALEX)                     // dual Mercury
                    {
                        ALEX_2_RX_out_to_ADC(bluePen);
                        MERCURY_2_ADC_to_DDCs(bluePen);
                        MERCURY_2_AUDIO_INPUT(bluePen);
                        if (bool_diversity)                       // diversity
                        {
                            MERCURY_2_DDC0_to_METIS_diversity(bluePen);
                        }
                        else                                       // not diversity
                        {
                            g.DrawRectangle(blackPen, RX2_DISPLAY);
                            label_RX2_DISPLAY.Visible = true;
                            MERCURY_2_DDC0_to_RX2_DISPLAY(bluePen);
                        }
                    }
                    else                                           // single Mercury
                    {
                        MERCURY_DDC1_to_RX2_DISPLAY(bluePen);
                        g.DrawRectangle(blackPen, RX2_DISPLAY);
                        label_RX2_DISPLAY.Visible = true;
                    }

                    // handle audio paths to Mercury(s)
                    if (!bool_RX1_MUTE)
                    {
                        label_RX1_LR_audio.Visible = true;
                        MERCURY_RX1_to_AUDIO_MIXER(bluePen);
                        HPSDR_DSP_to_AUDIO_MIXER_input_1(bluePen);
                    }
                    else label_RX1_LR_audio.Visible = false;
                    if (!bool_RX2_MUTE)
                    {
                        if (!bool_diversity)
                        {
                            label_RX2_LR_audio.Visible = true;
                            MERCURY_RX2_to_AUDIO_MIXER(bluePen);
                            HPSDR_DSP_to_AUDIO_MIXER_input_2(bluePen);
                        }
                    }
                    else label_RX2_LR_audio.Visible = false;

                    if (bool_RX1_MUTE & bool_RX2_MUTE)
                    {
                        label_AUDIO_MIXER.Visible = false;
                        label_RX1_LR_audio.Visible = false;
                        label_RX2_LR_audio.Visible = false;
                    }
                    else
                    {
                        g.DrawRectangle(blackPen, AUDIO_MIXER);
                        label_AUDIO_MIXER.Visible = true;
                    }

                }
                else
                {
                    // draw Tx DUC connection to RX1 DISPLAY
                    g.DrawLine(redPen, PENELOPE_DSP_mid_1, PENELOPE_DSP_mid_2);
                    g.DrawLine(redPen, PENELOPE_DSP_mid_2, RX1_DISPLAY_HPSDR_DDC0);
                }

            } // end TX section

            update_diagram = false;

        }  // end draw_HPSDR()



        private void draw_HERMES()
        {
            hide_controls();

            Update_control_settings();

            // EXT2 and EXT1 are switched on this model in Thetis checkbox logic, so switch them here
            bool bool_temp = bool_EXT1;
            bool_EXT1 = bool_EXT2;
            bool_EXT2 = bool_temp;

            hide_all_labels();
            label_hardware_selected.Text = "Routing for HERMES";
            label_PC.Visible = true;
            label_RX1_DISPLAY.Visible = true;
            //label_RX2_DISPLAY.Visible = true;
            if (bool_XVTR) label_XVTR_VHF.Visible = true;
            label_ADC0.Visible = true;
            label_ADC0_atten.Visible = true;
            label_C26.Visible = true;
            label_C25.Visible = true;
            label_C24.Visible = true;

            // label ALEX rectangles            
            label_C2.Text = "XV RX IN";
            label_C2.Visible = true;
            label_C2.Location = C2_label_HPSDR;

            label_C3.Text = "RX 2 IN";
            label_C3.Visible = true;
            label_C3.Location = C3_label_HPSDR;

            label_C4.Text = "RX 1 IN";
            label_C4.Visible = true;
            label_C4.Location = C4_label_HPSDR;

            label_C5.Text = "RX 1 OUT";
            label_C5.Visible = true;
            label_C5.Location = C5_label_HPSDR;

            label_C7.Text = "ANT 1";
            label_C7.Visible = true;
            label_C7.Location = C7_label_HPSDR;

            label_C8.Text = "ANT 2";
            label_C8.Visible = true;
            label_C8.Location = C8_label_HPSDR;

            label_C9.Text = "ANT 3";
            label_C9.Visible = true;
            label_C9.Location = C9_label_HPSDR;

            label_C10.Text = "From TX*";
            label_C10.Visible = true;
            label_C10.Location = C10_label_ALEX_TX_IN;

            label_DSP_HPSDR.Visible = true;
            label_DSP_HPSDR.Location = DSP_HPSDR_label;

            label_ALEX.Visible = true;
            label_ALEX.Location = ALEX_label;
            label_HERMES.Visible = true;
            label_HERMES.Location = HERMES_label;
            label_HERMES_RX_IN.Visible = true;
            label_HERMES_RX_IN.Location = HERMES_RX_IN_label;
            label_HERMES_J5.Visible = true;
            label_HERMES_J5.Location = HERMES_J5_label;
            label_HERMES_TX_OUT.Visible = true;
            label_HERMES_TX_OUT.Location = HERMES_TX_OUT_label;
            label_HERMES_J3.Visible = true;
            label_HERMES_J3.Location = HERMES_J3_label;
            label_HERMES_XVTR_TX.Visible = true;
            label_HERMES_XVTR_TX.Location = HERMES_XVTR_TX_label;
            label_HERMES_J1.Visible = true;
            label_HERMES_J1.Location = HERMES_J1_label;
            if (!bool_HPF_BYPASS) label_ALEX_HPF.Visible = true;
            label_ALEX_HPF.Location = ALEX_HPF_label;
            label_ALEX_LPF.Visible = true;
            label_ALEX_LPF.Location = ALEX_LPF_label;
            label_ALEX_To_RX.Visible = true;
            label_ALEX_To_RX.Location = ALEX_To_RX_label;
            label_FPGA.Visible = true;
            label_DDC0.Visible = true;
            label_DDC1.Visible = true;
            label_Rx0.Visible = true;
            label_Rx1.Visible = true;

            if (!bool_rx)
            {
                label_ext_amp1.Visible = true;
                label_ext_amp1.Location = ext_amp_label1;
                label_ext_amp2.Visible = true;
                label_ext_amp2.Location = ext_amp_label2;
            }

            if (bool_diversity) bool_diversity = false;  // diversity unavailable in ANAN-10/10E (Hermes 1 ADC limitation)

            label_C24.Text = "SPKR";
            C24_label.X = C24.X - 45;
            C24_label.Y = C24.Y - 6;
            label_C24.Location = C24_label;


            label_C26.Text = "MIC";
            C26_label.X = C26.X - 35;
            C26_label.Y = C26.Y - 6;
            label_C26.Location = C26_label;

            label_C25.Text = "HDPHONES";
            C25_label.X = C25.X - 75;
            C25_label.Y = C25.Y - 6;
            label_C25.Location = C25_label;

            // draw HERMES boundaries
            blackPen.Width = 2;
            g.DrawLine(blackPen, HERMES1, HERMES2);
            g.DrawLine(blackPen, HERMES2, HERMES3);
            g.DrawLine(blackPen, HERMES3, HERMES4);
            g.DrawLine(blackPen, HERMES4, HERMES5);
            g.DrawLine(blackPen, HERMES5, HERMES6);
            g.DrawLine(blackPen, HERMES6, HERMES1);
            blackPen.Width = 1;

            g.DrawRectangle(blackPen2, PC);
            g.DrawRectangle(blackPen, DSP_HERMES);
            g.DrawRectangle(blackPen, RX1_DISPLAY);
            g.DrawRectangle(blackPen2, ALEX);
            if (!bool_HPF_BYPASS) g.DrawRectangle(blackPen, ALEX_HPF);
            g.DrawRectangle(blackPen, ALEX_LPF);
            g.DrawRectangle(indianredPen, FPGA);
            g.DrawRectangle(blackPen, Rx0);
            g.DrawRectangle(blackPen, Rx1);
            g.DrawRectangle(blackPen, ADC0);


            if (bool_rx)           // RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX
            {
                ALEX_ANT_to_HPF_B(bluePen);
                g.DrawLine(bluePen, ALEX_RX_out, HERMES_corner1);
                g.DrawLine(bluePen, HERMES_corner1, HERMES_corner2);
                g.DrawLine(bluePen, HERMES_corner2, ADC0_L);

                g.DrawRectangle(blackPen, RX2_DISPLAY);
                label_RX2_DISPLAY.Visible = true;

                label_PA.Visible = false;
                label_LPF.Visible = false;
                label_AMP.Visible = false;
                label_FILTER.Visible = false;
                label_DAC0.Visible = false;
                label_DUC0.Visible = false;

                ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
                ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1
                Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                DSP_in1_to_out1_crossconnect(bluePen);
                DSP_in2_to_out2_crossconnect(bluePen);
                DSP_out1_to_RX1_DISPLAY(bluePen);
                DSP_out2_to_RX2_DISPLAY(bluePen);

                if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY(bluePen);

                if (!bool_RX1_MUTE) label_RX1_LR_audio.Visible = true;
                else label_RX1_LR_audio.Visible = false;
                if (!bool_RX2_MUTE) label_RX2_LR_audio.Visible = true;
                else label_RX2_LR_audio.Visible = false;
                if (bool_RX1_MUTE & bool_RX2_MUTE)
                {
                    label_AUDIO_AMP.Visible = false;
                    label_CODEC2.Visible = false;
                    label_L_audio_only.Visible = false;
                    label_LR_audio.Visible = false;
                    label_AUDIO_MIXER.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    label_CODEC.Visible = false;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_L_audio_only.Visible = true;
                    label_LR_audio.Visible = true;
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2)
                {
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_HPF_B);
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2)
                {
                    if (bool_disable_BYPASS)
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
                    }
                    else
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
                        g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
                    }
                    g.DrawLine(bluePen, ALEX_HPF_corner3, ALEX_HPF_corner1);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_XV_RX_IN, ALEX_HPF_corner3);
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2)
                {
                    if (bool_disable_BYPASS)
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
                    }
                    else
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
                        g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
                    }
                    g.DrawLine(bluePen, ALEX_HPF_corner5, ALEX_HPF_corner1);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_RX_1_IN, ALEX_HPF_corner5);
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2)
                {
                    if (bool_disable_BYPASS)
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner1);
                    }
                    else
                    {
                        g.DrawLine(bluePen, ALEX_HPF_B, ALEX_HPF_corner2);
                        g.DrawLine(bluePen, ALEX_HPF_corner2, ALEX_RX1_out);
                    }
                    g.DrawLine(bluePen, ALEX_HPF_corner4, ALEX_HPF_corner1);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_RX_2_IN, ALEX_HPF_corner4);
                }



            }
            else                   // TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX 
            {
                label_FILTER.Visible = true;
                g.DrawRectangle(blackPen, AMPF);
                g.DrawLine(redPen, HERMES_XVTR_TX, AMPF_L_PA15);
                label_DAC0.Visible = true;
                g.DrawRectangle(blackPen, DAC0);
                label_DUC0.Visible = true;
                g.DrawRectangle(blackPen, DUC0);
                label_CODEC.Visible = true;
                g.DrawRectangle(blackPen, CODEC);
                label_L_audio_only.Visible = false;
                label_AUDIO_AMP.Visible = false;
                label_CODEC2.Visible = false;
                label_LR_audio.Visible = false;
                label_RX1_LR_audio.Visible = false;
                label_RX2_LR_audio.Visible = false;
                label_AUDIO_MIXER.Visible = false;

                basic_Tx_path(redPen);
                ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1

                TX_AUDIO_OUT_2_RX_MODELS();                   // Tx mode logic for audio ouput portions of the diagram

                if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    label_PA.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    HERMES_PA_to_ALEX_LPF(redPen);
                    ALEX_TX_ANT(redPen);
                    label_AMP.Visible = true;
                    g.DrawLine(redPen, PA_corner, PA_B);
                    loopback_to_RX1_DISPLAY(redPen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);
                }


                if (!bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_PA.Visible = false;
                    label_LPF.Visible = false;
                    loopback_to_RX1_DISPLAY(bluePen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);
                }

                if (!bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    label_PA.Visible = false;
                    label_LPF.Visible = false;

                    if (!bool_EXT1_on_TX & !bool_EXT2_on_TX)
                    {
                        g.DrawLine(bluePen, ALEX_HPF_corner3, ALEX_HPF_corner1);
                        g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                        g.DrawLine(bluePen, ALEX_XV_RX_IN, ALEX_HPF_corner3);
                    }
                    g.DrawLine(bluePen, ALEX_RX_out, HERMES_corner1);
                    g.DrawLine(bluePen, HERMES_corner1, HERMES_corner2);
                    g.DrawLine(bluePen, HERMES_corner2, ADC0_L);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    DSP_in1_to_out1_crossconnect(bluePen);
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY(bluePen);
                }

                if (!bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    label_PA.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    label_AMP.Visible = true;
                    HERMES_PA_to_ALEX_LPF(redPen);
                    ALEX_TX_ANT(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    g.DrawLine(bluePen, ALEX_RX_out, HERMES_corner1);
                    g.DrawLine(bluePen, HERMES_corner1, HERMES_corner2);
                    g.DrawLine(bluePen, HERMES_corner2, ADC0_L);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_in2_to_out2_crossconnect(bluePen);
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    DSP_in1_to_out1_crossconnect(bluePen);
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                }

                if (bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    g.DrawLine(bluePen, ALEX_RX_out, HERMES_corner1);
                    g.DrawLine(bluePen, HERMES_corner1, HERMES_corner2);
                    g.DrawLine(bluePen, HERMES_corner2, ADC0_L);
                    loopback_to_RX1_DISPLAY(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    label_PA.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    HERMES_PA_to_ALEX_LPF(redPen);
                    ALEX_TX_ANT(redPen);
                    label_AMP.Visible = true;
                }

                if (bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    label_PA.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    label_LPF.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_AMP.Visible = true;
                    PA_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                }

                if (bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_PA.Visible = false;
                    label_LPF.Visible = false;
                    loopback_to_RX1_DISPLAY(redPen);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                }

                if (bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    label_PA.Visible = false;
                    label_LPF.Visible = false;
                    DUC0_to_Rx1(redPen);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                }


                if (bool_EXT1_on_TX)
                {
                    g.DrawLine(bluePen, ALEX_HPF_corner5, ALEX_HPF_corner1);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_RX_1_IN, ALEX_HPF_corner5);
                    if (bool_DUAL_MERCURY_ALEX)
                    {
                        g.DrawLine(bluePen, ALEX_2_HPF_corner5, ALEX_2_HPF_corner1);
                        g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
                        g.DrawLine(bluePen, ALEX_2_RX_1_IN, ALEX_2_HPF_corner5);
                    }

                }

                if (bool_EXT2_on_TX)
                {
                    g.DrawLine(bluePen, ALEX_HPF_corner4, ALEX_HPF_corner1);
                    g.DrawLine(bluePen, ALEX_HPF_corner1, ALEX_RX_out);
                    g.DrawLine(bluePen, ALEX_RX_2_IN, ALEX_HPF_corner4);
                    if (bool_DUAL_MERCURY_ALEX)
                    {
                        g.DrawLine(bluePen, ALEX_2_HPF_corner4, ALEX_2_HPF_corner1);
                        g.DrawLine(bluePen, ALEX_2_HPF_corner1, ALEX_2_RX_out);
                        g.DrawLine(bluePen, ALEX_2_RX_2_IN, ALEX_2_HPF_corner4);
                    }

                }

            }


            update_diagram = false;

        } // end draw_HERMES()


        private void draw_ANAN_10E()
        {
            hide_all_labels();
            label_hardware_selected.Text = "Routing for ANAN-10, ANAN-10E";
            if (bool_XVTR) label_XVTR_VHF.Visible = true;
            label_ADC0.Visible = true;
            label_ADC0_atten.Visible = true;
            label_SDR_Hardware.Visible = true;
            label_C26.Visible = true;
            label_C25.Visible = true;
            label_C24.Visible = true;
            label_FPGA.Visible = true;
            label_DDC0.Visible = true;
            label_DDC1.Visible = true;
            label_Rx0.Visible = true;
            label_Rx1.Visible = true;
            label_PC.Visible = true;
            label_RX1_DISPLAY.Visible = true;
            label_DSP.Visible = true;


            // show only controls that are relevant for this SDR platform
            hide_controls();
            cb_DUAL_MERCURY_ALEX.Visible = false;
            Update_control_settings();

            // draw the rectangles
            g.DrawRectangle(blackPen2, SDR);
            g.DrawRectangle(blackPen2, PC);
            g.DrawRectangle(indianredPen, FPGA);
            g.DrawRectangle(blackPen, DSP);
            g.DrawRectangle(blackPen, RX1_DISPLAY);
            g.DrawRectangle(blackPen, Rx0);
            g.DrawRectangle(blackPen, Rx1);
            g.DrawRectangle(blackPen, ADC0);
            // relocate LPF
            LPF.X = 230;
            LPF.Y = 280;
            LPF_L_c.X = LPF.X - 20;
            LPF_L_c.Y = LPF_L.Y;
            LPF_R.X = LPF.X + 50;
            LPF_R.Y = LPF.Y + 25;
            LPF_R_c.X = LPF.Y + 25;
            LPF_T.X = LPF.X + 25;
            LPF_T.Y = LPF.Y;
            LPF_B.X = LPF.X + 25;
            LPF_B.Y = LPF.Y + 50;
            LPF_corner_C2.X = LPF_T.X;
            LPF_corner_C2.Y = C2.Y;
            LPF_corner_C3.X = LPF.X + 25;
            LPF_corner_C3.Y = C3.Y;
            LPF_corner_C4.X = LPF.X + 25;
            LPF_corner_C4.Y = C4.Y;

            update_labels_PA10();

            if (bool_rx)              // draw the path segments for RECEIVE mode  RX RX RX RX RX RX RX RX RX RX RX RX
            {
                g.DrawRectangle(blackPen, RX2_DISPLAY);
                label_RX2_DISPLAY.Visible = true;

                label_PA.Visible = false;
                label_LPF.Visible = false;
                label_AMP.Visible = false;
                label_FILTER.Visible = false;
                label_DAC0.Visible = false;
                label_DUC0.Visible = false;



                if (bool_ANT1) C2_to_Rx0(bluePen);
                if (bool_ANT2) C3_to_Rx0(bluePen);
                if (bool_ANT3) C4_to_Rx0(bluePen);

                C5_to_ADC0(bluePen);                        // draw RX to ADC0 input in blue
                ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
                ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1
                Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                DSP_in1_to_out1_crossconnect(bluePen);
                DSP_in2_to_out2_crossconnect(bluePen);
                DSP_out1_to_RX1_DISPLAY(bluePen);
                DSP_out2_to_RX2_DISPLAY(bluePen);

                g.DrawRectangle(blackPen2, SDR);            // to fix blank spot in initial rendering

                if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY(bluePen);

                if (!bool_RX1_MUTE) label_RX1_LR_audio.Visible = true;
                else label_RX1_LR_audio.Visible = false;
                if (!bool_RX2_MUTE) label_RX2_LR_audio.Visible = true;
                else label_RX2_LR_audio.Visible = false;
                if (bool_RX1_MUTE & bool_RX2_MUTE)
                {
                    label_AUDIO_AMP.Visible = false;
                    label_CODEC2.Visible = false;
                    label_L_audio_only.Visible = false;
                    label_LR_audio.Visible = false;
                    label_AUDIO_MIXER.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    label_CODEC.Visible = false;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_L_audio_only.Visible = true;
                    label_LR_audio.Visible = true;
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }
            }
            else                           // draw the paths for TRANSMIT mode   TX TX TX TX TX TX TX TX TX TX TX TX TX TX 
            {
                label_FILTER.Visible = true;
                g.DrawRectangle(blackPen, AMPF);
                label_DAC0.Visible = true;
                g.DrawRectangle(blackPen, DAC0);
                label_DUC0.Visible = true;
                g.DrawRectangle(blackPen, DUC0);
                label_CODEC.Visible = true;
                g.DrawRectangle(blackPen, CODEC);
                label_L_audio_only.Visible = false;
                label_AUDIO_AMP.Visible = false;
                label_CODEC2.Visible = false;
                label_LR_audio.Visible = false;
                label_RX1_LR_audio.Visible = false;
                label_RX2_LR_audio.Visible = false;
                label_AUDIO_MIXER.Visible = false;

                C5_to_ADC0(bluePen);                        // draw RX to ADC0 input in blue
                basic_Tx_path(redPen);
                AMPF_TX_path_PA10(redPen);
                ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1

                TX_AUDIO_OUT_2_RX_MODELS();                   // Tx mode logic for audio ouput portions of the diagram

                if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    label_PA.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    label_LPF.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_AMP.Visible = true;
                    line_to_ground(bluePen);                    // draw ground on line
                    PA_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    if (bool_ANT1_TX) LPF_to_C2(redPen);
                    if (bool_ANT2_TX) LPF_to_C3(redPen);
                    if (bool_ANT3_TX) LPF_to_C4(redPen);
                    loopback_to_RX1_DISPLAY(redPen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);

                }

                if (!bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_PA.Visible = false;
                    label_LPF.Visible = false;
                    loopback_to_RX1_DISPLAY(redPen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);
                }

                if (!bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    label_PA.Visible = false;
                    label_LPF.Visible = false;
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    DSP_in1_to_out1_crossconnect(bluePen);
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY(bluePen);
                }

                if (!bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    label_PA.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    label_LPF.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_AMP.Visible = true;
                    line_to_ground(bluePen);                    // draw ground on line
                    PA_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    if (bool_ANT1_TX) LPF_to_C2(redPen);
                    if (bool_ANT2_TX) LPF_to_C3(redPen);
                    if (bool_ANT3_TX) LPF_to_C4(redPen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_in2_to_out2_crossconnect(bluePen);
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    DSP_in1_to_out1_crossconnect(bluePen);
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                }

                if (bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    loopback_to_RX1_DISPLAY(redPen);
                    line_to_ground(bluePen);                    // draw ground on line
                    PA_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    if (bool_ANT1_TX) LPF_to_C2(redPen);
                    if (bool_ANT2_TX) LPF_to_C3(redPen);
                    if (bool_ANT3_TX) LPF_to_C4(redPen);
                    label_PA.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    label_LPF.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_AMP.Visible = true;
                }

                if (bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    label_PA.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    label_LPF.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_AMP.Visible = true;
                    line_to_ground(bluePen);                    // draw ground on line
                    PA_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    if (bool_ANT1_TX) LPF_to_C2(redPen);
                    if (bool_ANT2_TX) LPF_to_C3(redPen);
                    if (bool_ANT3_TX) LPF_to_C4(redPen);
                }

                if (bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_PA.Visible = false;
                    label_LPF.Visible = false;
                    loopback_to_RX1_DISPLAY(redPen);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                }

                if (bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    label_PA.Visible = false;
                    label_LPF.Visible = false;
                    DUC0_to_Rx1(redPen);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                }
            }

            update_diagram = false;                     // prevents unnecessary automatic updating

        } // end draw_ANAN-10E()


        private void draw_ANAN_100_PA_rev15()
        {
            hide_all_labels();
            label_hardware_selected.Text = "Routing for ANAN-100 (PA rev_15/16)";
            if (bool_XVTR) label_XVTR_VHF.Visible = true; 
            label_ADC0.Visible = true;
            label_ADC0_atten.Visible = true;
            label_SDR_Hardware.Visible = true;
            label_C26.Visible = true;
            label_C25.Visible = true;
            label_C24.Visible = true;
            label_FPGA.Visible = true;
            label_DDC0.Visible = true;
            label_DDC1.Visible = true;
            label_Rx0.Visible = true;
            label_Rx1.Visible = true;
            label_PC.Visible = true;
            label_RX1_DISPLAY.Visible = true;
            label_DSP.Visible = true;
            label_NOTE.Visible = true;
            label_UNCHECK.Visible = true;


            // show only relevant controls for this SDR platform
            hide_controls();

            cb_DUAL_MERCURY_ALEX.Visible = false;

            Update_control_settings();

            // relocate LPF
            LPF.X = C10.X + 110;
            LPF.Y = C10.Y - 25;
            LPF_L.X = LPF.X;
            LPF_L.Y = LPF.Y + 25;
            LPF_L_c.X = LPF.X - 20;
            LPF_L_c.Y = LPF_L.Y;
            LPF_R.X = LPF.X + 50;
            LPF_R.Y = LPF.Y + 25;
            LPF_R_c.X = LPF.Y + 25;
            LPF_T.X = LPF.X + 25;
            LPF_T.Y = LPF.Y;
            LPF_BYPASS_corner.X = LPF.X + 25;
            LPF_BYPASS_corner.Y = C7.Y;

            // relocate SWR
            SWR.X = C10.X + 50;
            SWR.Y = C10.Y - 25;
            SWR_L.X = SWR.X;
            SWR_L.Y = SWR.Y + 25;
            SWR_R.X = SWR.X + 50;
            SWR_R.Y = SWR.Y + 25;
            SWR_T.X = SWR.X + 25;
            SWR_T.Y = SWR.Y;

            // relocate HPF 
            HPF.X = PA.X;
            HPF.Y = ADC0.Y;
            HPF_R.X = HPF.X + 50;
            HPF_R.Y = HPF.Y + 25;
            HPF_B.X = HPF.X + 25;
            HPF_B.Y = HPF.Y + 50;
            HPF_L.X = HPF.X;
            HPF_L.Y = HPF.Y + 25;

            LPF_HPF_corner.X = HPF.X + 25;
            LPF_HPF_corner.Y = C10.Y;
            XVTR_HPF_corner.X = HPF_B.X;
            XVTR_HPF_corner.Y = C4.Y;
            EXT1_HPF_corner.X = HPF_B.X;
            EXT1_HPF_corner.Y = C5.Y;
            EXT2_HPF_corner.X = HPF_B.X;
            EXT2_HPF_corner.Y = C6.Y;

            update_labels_PA();

            // draw the rectangles
            g.DrawRectangle(blackPen2, PC);
            g.DrawRectangle(indianredPen, FPGA);
            g.DrawRectangle(blackPen, DSP);
            g.DrawRectangle(blackPen, RX1_DISPLAY);
            g.DrawRectangle(blackPen, Rx0);
            g.DrawRectangle(blackPen, Rx1);
            g.DrawRectangle(blackPen, ADC0);


            if (bool_rx)              // draw the path segments for RECEIVE mode  RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX 
            {
                // show only RX components
                g.DrawRectangle(blackPen, RX2_DISPLAY);
                label_RX2_DISPLAY.Visible = true;
                label_PA.Visible = false;
                label_AMP.Visible = false;
                label_FILTER.Visible = false;
                label_DAC0.Visible = false;
                label_DUC0.Visible = false;

                if (!bool_HPF_BYPASS)
                {
                    g.DrawRectangle(blackPen, HPF);
                    label_HPF.Visible = true;
                }
                else
                {
                    g.DrawLine(bluePen, HPF_B, HPF_center);
                    g.DrawLine(bluePen, HPF_center, HPF_R);
                }

                if (!bool_RX1_MUTE) label_RX1_LR_audio.Visible = true;
                else label_RX1_LR_audio.Visible = false;
                if (!bool_RX2_MUTE) label_RX2_LR_audio.Visible = true;
                else label_RX2_LR_audio.Visible = false;
                if (bool_RX1_MUTE & bool_RX2_MUTE)
                {
                    label_AUDIO_AMP.Visible = false;
                    label_CODEC2.Visible = false;
                    label_L_audio_only.Visible = false;
                    label_LR_audio.Visible = false;
                    label_AUDIO_MIXER.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    label_CODEC.Visible = false;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_L_audio_only.Visible = true;
                    label_LR_audio.Visible = true;
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }

                g.DrawLine(bluePen, HPF_R, ADC0_L);

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_HPF_PA15(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    XVTR_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    EXT1_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & !bool_BYPASS)
                {
                    EXT2_to_HPF_PA15(bluePen);
                }

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_BYPASS(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue

                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    XVTR_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_BYPASS(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    EXT1_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_BYPASS(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    EXT2_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_BYPASS(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }

                ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
                ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1
                Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                DSP_in1_to_out1_crossconnect(bluePen);
                DSP_in2_to_out2_crossconnect(bluePen);
                DSP_out1_to_RX1_DISPLAY(bluePen);
                DSP_out2_to_RX2_DISPLAY(bluePen);
                if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY(bluePen);
            }
            else                           // draw the paths for TRANSMIT mode   TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX
            {
                g.DrawRectangle(blackPen, AMPF);
                g.DrawRectangle(blackPen, DAC0);
                g.DrawRectangle(blackPen, DUC0);

                if (!bool_HPF_BYPASS)
                {
                    g.DrawRectangle(blackPen, HPF);
                    label_HPF.Visible = true;
                }
                else
                {
                    g.DrawLine(bluePen, HPF_L, HPF_center);
                    g.DrawLine(bluePen, HPF_center, HPF_R);
                }

                g.DrawLine(bluePen, HPF_R, ADC0_L);

                g.DrawRectangle(blackPen, CODEC);
                label_PA.Visible = true;
                label_AMP.Visible = true;
                label_FILTER.Visible = true;
                label_DAC0.Visible = true;
                label_DUC0.Visible = true;
                label_CODEC.Visible = true;
                label_L_audio_only.Visible = false;
                label_AUDIO_AMP.Visible = false;
                label_CODEC2.Visible = false;
                label_LR_audio.Visible = false;
                label_RX1_LR_audio.Visible = false;
                label_RX2_LR_audio.Visible = false;
                label_AUDIO_MIXER.Visible = false;


                // handle HPF input circuitry

                // EXT2_on_TX and EXT1_on_TX are switched on this model in PowerSDR checkbox logic, so switch them here
                bool bool_temp = bool_EXT1_on_TX;
                bool_EXT1_on_TX = bool_EXT2_on_TX;
                bool_EXT2_on_TX = bool_temp;
 
                if (!bool_BYPASS_on_TX & !bool_EXT1_on_TX & !bool_EXT2_on_TX) HPF_to_ground(bluePen);
                if (bool_BYPASS_on_TX)
                {
                    C7_to_ground(bluePen);
                }
                if (bool_EXT1_on_TX)
                {
                    C5_to_HPF_PA15_TX(bluePen);
                    C7_to_ground(bluePen);
                }
                if (bool_EXT2_on_TX)
                {
                    C6_to_HPF_PA15_TX(bluePen);
                    C7_to_ground(bluePen);
                }

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    label_LPF.Visible = false;
                }


                if (bool_MON)
                {
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY(bluePen);

                    if (!bool_RX1_MUTE) label_RX1_LR_audio.Visible = true;
                    else label_RX1_LR_audio.Visible = false;
                    if (!bool_RX2_MUTE) label_RX2_LR_audio.Visible = true;
                    else label_RX2_LR_audio.Visible = false;
                    if (bool_RX1_MUTE & bool_RX2_MUTE)
                    {
                        label_AUDIO_AMP.Visible = false;
                        label_CODEC2.Visible = false;
                        label_L_audio_only.Visible = false;
                        label_LR_audio.Visible = false;
                        label_AUDIO_MIXER.Visible = false;
                    }
                    else
                    {
                        g.DrawRectangle(blackPen, AUDIO_MIXER);
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_L_audio_only.Visible = true;
                        label_LR_audio.Visible = true;
                        g.DrawRectangle(blackPen, AUDIO_AMP);

                    }
                }

                basic_Tx_path(redPen);
                ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1

                TX_AUDIO_OUT_2_RX_MODELS();                   // Tx logic for audio ouput portions of the diagram

                if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    PA15_to_LPF(redPen);
                    AMPF_XVTR_TX(redPen);
                    AMPF_to_PA15(redPen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red
                    loopback_to_RX1_DISPLAY(redPen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);

                }

                if (!bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    AMPF_XVTR_TX(redPen);
                    loopback_to_RX1_DISPLAY(redPen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);

                }

                if (!bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    AMPF_XVTR_TX(redPen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    DSP_in1_to_out1_crossconnect(bluePen);
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);

                }

                if (!bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_in2_to_out2_crossconnect(bluePen);
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    DSP_in1_to_out1_crossconnect(bluePen);
                    DSP_out2_to_RX2_DISPLAY(bluePen);

                }

                if (bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    loopback_to_RX1_DISPLAY(redPen);
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red

                }

                if (bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in blue
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in blue
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in blue
                }

                if (bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_RX2_DISPLAY.Visible = false;
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    loopback_to_RX1_DISPLAY(redPen);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    AMPF_XVTR_TX(redPen);
                }


                if (bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    if (bool_RX1_MUTE & bool_RX2_MUTE)
                    {

                    }
                    else
                    {
                        label_SWR.Visible = false;
                        label_LPF.Visible = false;
                        label_PA.Visible = false;
                        DUC0_to_Rx1(redPen);
                        Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                        AMPF_XVTR_TX(redPen);
                    }
                }
            }
            g.DrawRectangle(blackPen, SDR);
            update_diagram = false;                     // prevents unnecessary automatic updating

        } // end ANAN-100 w/PA_rev15/16


        private void draw_ANAN_100_PA_rev24()
        {
            hide_all_labels();
            label_hardware_selected.Text = "Routing for ANAN-100, ANAN-100B (PA rev_24)";
            if (bool_XVTR) label_XVTR_VHF.Visible = true; 
            label_ADC0.Visible = true;
            label_ADC0_atten.Visible = true;
            label_SDR_Hardware.Visible = true;
            label_C26.Visible = true;
            label_C25.Visible = true;
            label_C24.Visible = true;
            label_FPGA.Visible = true;
            label_DDC0.Visible = true;
            label_DDC1.Visible = true;
            label_Rx0.Visible = true;
            label_Rx1.Visible = true;
            label_PC.Visible = true;
            label_RX1_DISPLAY.Visible = true;
            label_DSP.Visible = true;
            label_NOTE.Visible = true;
            label_UNCHECK.Visible = true;

            hide_controls();
            cb_DUAL_MERCURY_ALEX.Visible = false;

            Update_control_settings();

            // relocate LPF
            LPF.X = C10.X + 110;
            LPF.Y = C10.Y - 25;
            LPF_L.X = LPF.X;
            LPF_L.Y = LPF.Y + 25;
            LPF_L_c.X = LPF.X - 20;
            LPF_L_c.Y = LPF_L.Y;
            LPF_R.X = LPF.X + 50;
            LPF_R.Y = LPF.Y + 25;
            LPF_R_c.X = LPF.Y + 25;
            LPF_T.X = LPF.X + 25;
            LPF_T.Y = LPF.Y;
            LPF_BYPASS_corner.X = LPF.X + 25;
            LPF_BYPASS_corner.Y = C7.Y;

            // relocate SWR
            SWR.X = C10.X + 50;
            SWR.Y = C10.Y - 25;
            SWR_L.X = SWR.X;
            SWR_L.Y = SWR.Y + 25;
            SWR_R.X = SWR.X + 50;
            SWR_R.Y = SWR.Y + 25;
            SWR_T.X = SWR.X + 25;
            SWR_T.Y = SWR.Y;

            // relocate HPF 
            HPF.X = PA.X;
            HPF.Y = ADC0.Y;
            HPF_R.X = HPF.X + 50;
            HPF_R.Y = HPF.Y + 25;
            HPF_B.X = HPF.X + 25;
            HPF_B.Y = HPF.Y + 50;
            HPF_L.X = HPF.X;
            HPF_L.Y = HPF.Y + 25;

            LPF_HPF_corner.X = HPF.X + 25;
            LPF_HPF_corner.Y = C10.Y;
            XVTR_HPF_corner.X = HPF_B.X;
            XVTR_HPF_corner.Y = C4.Y;
            EXT1_HPF_corner.X = HPF_B.X;
            EXT1_HPF_corner.Y = C5.Y;
            EXT2_HPF_corner.X = HPF_B.X;
            EXT2_HPF_corner.Y = C6.Y;

            update_labels_PA();
            label_HPF.Visible = false;


            // draw some of the rectangles
            g.DrawRectangle(blackPen2, PC);
            g.DrawRectangle(indianredPen, FPGA);
            g.DrawRectangle(blackPen, DSP);
            g.DrawRectangle(blackPen, RX1_DISPLAY);
            g.DrawRectangle(blackPen, Rx0);
            g.DrawRectangle(blackPen, Rx1);
            g.DrawRectangle(blackPen, ADC0);
            Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1



            if (bool_rx)              // draw the path segments for RECEIVE mode  RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX 
            {
                // show only RX components
                g.DrawRectangle(blackPen, RX2_DISPLAY);
                label_RX2_DISPLAY.Visible = true;
                label_PA.Visible = false;
                label_AMP.Visible = false;
                label_FILTER.Visible = false;
                label_DAC0.Visible = false;
                label_DUC0.Visible = false;

                if (!bool_HPF_BYPASS)
                {
                    g.DrawRectangle(blackPen, HPF);
                    label_HPF.Visible = true;
                }
                else
                {
                    g.DrawLine(bluePen, HPF_B, HPF_center);
                    g.DrawLine(bluePen, HPF_center, HPF_R);
                }

                if (!bool_RX1_MUTE) label_RX1_LR_audio.Visible = true;
                else label_RX1_LR_audio.Visible = false;
                if (!bool_RX2_MUTE) label_RX2_LR_audio.Visible = true;
                else label_RX2_LR_audio.Visible = false;
                if (bool_RX1_MUTE & bool_RX2_MUTE)
                {
                    label_AUDIO_AMP.Visible = false;
                    label_CODEC2.Visible = false;
                    label_L_audio_only.Visible = false;
                    label_LR_audio.Visible = false;
                    label_AUDIO_MIXER.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    label_CODEC.Visible = false;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_L_audio_only.Visible = true;
                    label_LR_audio.Visible = true;
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }

                g.DrawLine(bluePen, HPF_R, ADC0_L);

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_HPF_PA15(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    XVTR_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    EXT1_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & !bool_BYPASS)
                {
                    EXT2_to_HPF_PA15(bluePen);
                }

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    BYPASS_to_ADC0(bluePen);
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    return;
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    return;

                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
                {
                    return;

                }

                ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
                ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                DSP_in1_to_out1_crossconnect(bluePen);
                DSP_in2_to_out2_crossconnect(bluePen);
                DSP_out1_to_RX1_DISPLAY(bluePen);
                DSP_out2_to_RX2_DISPLAY(bluePen);
                if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY(bluePen);
            }
            else                           // draw the paths for TRANSMIT mode   TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX
            {
                if (bool_EXT1_on_TX) bool_EXT1_on_TX = false;           // EXT1_on_TX not allowed with ANAN_PA_rev24
                if (bool_EXT2_on_TX) bool_EXT2_on_TX = false;           // EXT2_on_TX not allowed with ANAN_PA_rev24
                g.DrawRectangle(blackPen, AMPF);
                g.DrawRectangle(blackPen, DAC0);
                g.DrawRectangle(blackPen, DUC0);
                g.DrawRectangle(blackPen, CODEC);
                label_PA.Visible = true;
                label_AMP.Visible = true;
                label_FILTER.Visible = true;
                label_DAC0.Visible = true;
                label_DUC0.Visible = true;
                label_CODEC.Visible = true;
                label_L_audio_only.Visible = false;
                label_AUDIO_AMP.Visible = false;
                label_CODEC2.Visible = false;
                label_LR_audio.Visible = false;
                label_RX1_LR_audio.Visible = false;
                label_RX2_LR_audio.Visible = false;
                label_AUDIO_MIXER.Visible = false;

                if (bool_EXT1_on_TX)
                {
                    return;
                }
                if (bool_EXT2_on_TX)
                {
                    return;
                }
                if (bool_BYPASS_on_TX)
                {
                    BYPASS_to_ADC0(bluePen);
                }
                if (!bool_EXT1_on_TX & !bool_EXT2_on_TX & !bool_BYPASS_on_TX) SWR_to_ADC0(bluePen);

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                }

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    BYPASS_to_ADC0(bluePen);
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    return;

                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    return;

                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
                {
                    return;

                }

                if (bool_MON)
                {
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY(bluePen);

                    if (!bool_RX1_MUTE) label_RX1_LR_audio.Visible = true;
                    else label_RX1_LR_audio.Visible = false;
                    if (!bool_RX2_MUTE) label_RX2_LR_audio.Visible = true;
                    else label_RX2_LR_audio.Visible = false;
                    if (bool_RX1_MUTE & bool_RX2_MUTE)
                    {
                        label_AUDIO_AMP.Visible = false;
                        label_CODEC2.Visible = false;
                        label_L_audio_only.Visible = false;
                        label_LR_audio.Visible = false;
                        label_AUDIO_MIXER.Visible = false;
                    }
                    else
                    {
                        g.DrawRectangle(blackPen, AUDIO_MIXER);
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        //label_CODEC.Visible = true;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_L_audio_only.Visible = true;
                        label_LR_audio.Visible = true;
                        g.DrawRectangle(blackPen, AUDIO_AMP);

                    }
                }

                basic_Tx_path(redPen);
                ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1

                TX_AUDIO_OUT_2_RX_MODELS();                   // Tx logic for audio ouput portions of the diagram

                if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    PA15_to_LPF(redPen);
                    AMPF_XVTR_TX(redPen);
                    AMPF_to_PA15(redPen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red
                    loopback_to_RX1_DISPLAY(redPen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);
                }

                if (!bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    AMPF_XVTR_TX(redPen);
                    loopback_to_RX1_DISPLAY(redPen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);
                }

                if (!bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    AMPF_XVTR_TX(redPen);
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    DSP_in1_to_out1_crossconnect(bluePen);
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);

                }

                if (!bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red
                    ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_in2_to_out2_crossconnect(bluePen);
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    DSP_in1_to_out1_crossconnect(bluePen);
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                }

                if (bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    SWR_to_ADC0(bluePen);
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    loopback_to_RX1_DISPLAY(redPen);
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red
                }

                if (bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in blue
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in blue
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in blue
                }

                if (bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_RX2_DISPLAY.Visible = false;
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    loopback_to_RX1_DISPLAY(redPen);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    AMPF_XVTR_TX(redPen);
                }


                if (bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    if (bool_RX1_MUTE & bool_RX2_MUTE)
                    {

                    }
                    else
                    {
                        label_SWR.Visible = false;
                        label_LPF.Visible = false;
                        label_PA.Visible = false;
                        DUC0_to_Rx1(redPen);
                        Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                        AMPF_XVTR_TX(redPen);
                    }
                }
            }
            g.DrawRectangle(blackPen, SDR);
            update_diagram = false;                     // prevents unnecessary automatic updating


        } // end of draw_ANAN_100_PA_rev24()

        private void draw_ANAN_100D_PA_rev15()
        {
            hide_all_labels();
            label_hardware_selected.Text = "Routing for ANAN-100D, ANAN-200D (PA rev_15/16)";
            label_SDR_Hardware.Visible = true;
            label_FPGA.Visible = true;
            label_PC.Visible = true;
            label_DSP.Visible = true;
            label_RX1_DISPLAY.Visible = true;
            if (bool_XVTR) label_XVTR_VHF.Visible = true; 
            label_NOTE.Visible = true;
            label_UNCHECK.Visible = true;

            // CONTROL PANEL items
            // show relevant controls for this SDR platform
            hide_controls();

            Update_control_settings();

            // relocate LPF
            LPF.X = C10.X + 110;
            LPF.Y = C10.Y - 25;
            LPF_L.X = LPF.X;
            LPF_L.Y = LPF.Y + 25;
            LPF_L_c.X = LPF.X - 20;
            LPF_L_c.Y = LPF_L.Y;
            LPF_R.X = LPF.X + 50;
            LPF_R.Y = LPF.Y + 25;
            LPF_R_c.X = LPF.Y + 25;
            LPF_T.X = LPF.X + 25;
            LPF_T.Y = LPF.Y;
            LPF_BYPASS_corner.X = LPF.X + 25;
            LPF_BYPASS_corner.Y = C7.Y;

            // relocate SWR
            SWR.X = C10.X + 50;
            SWR.Y = C10.Y - 25;
            SWR_L.X = SWR.X;
            SWR_L.Y = SWR.Y + 25;
            SWR_R.X = SWR.X + 50;
            SWR_R.Y = SWR.Y + 25;
            SWR_T.X = SWR.X + 25;
            SWR_T.Y = SWR.Y;

            // relocate HPF 
            HPF.X = PA.X;
            HPF.Y = ADC0.Y;
            HPF_R.X = HPF.X + 50;
            HPF_R.Y = HPF.Y + 25;
            HPF_B.X = HPF.X + 25;
            HPF_B.Y = HPF.Y + 50;
            HPF_L.X = HPF.X;
            HPF_L.Y = HPF.Y + 25;

            LPF_HPF_corner.X = HPF.X + 25;
            LPF_HPF_corner.Y = C10.Y;
            XVTR_HPF_corner.X = HPF_B.X;
            XVTR_HPF_corner.Y = C4.Y;
            EXT1_HPF_corner.X = HPF_B.X;
            EXT1_HPF_corner.Y = C5.Y;
            EXT2_HPF_corner.X = HPF_B.X;
            EXT2_HPF_corner.Y = C6.Y;

            label_HPF.Visible = true;
            label_ADC0_atten.Visible = true;
            label_ADC0.Visible = true;
            label_DDC0.Visible = true;
            label_Rx0.Visible = true;
            label_DDC1.Visible = true;
            label_Rx1.Visible = true;

            // draw the rectangles
            g.DrawRectangle(blackPen2, PC);
            g.DrawRectangle(indianredPen, FPGA);
            g.DrawRectangle(blackPen, DSP);
            g.DrawRectangle(blackPen, RX1_DISPLAY);
            g.DrawRectangle(blackPen, RX2_DISPLAY);
            label_RX2_DISPLAY.Visible = true;
            g.DrawRectangle(blackPen, Rx0);
            g.DrawRectangle(blackPen, Rx1);
            g.DrawRectangle(blackPen, Rx2);
            g.DrawRectangle(blackPen, Rx3);
            g.DrawRectangle(blackPen, Rx4);
            g.DrawRectangle(blackPen, Rx5);
            g.DrawRectangle(blackPen, Rx6);
            g.DrawRectangle(blackPen, ADC0);
            g.DrawRectangle(blackPen, ADC1);

            update_labels_PA();                         // relevant for the PA_rev15/16 and PA_rev24 platforms
            label_ADC1_atten.Visible = true;
            label_ADC1.Visible = true;

            // draw the rectangles
            g.DrawRectangle(blackPen2, PC);
            g.DrawRectangle(indianredPen, FPGA);
            g.DrawRectangle(blackPen, DSP);
            g.DrawRectangle(blackPen, RX1_DISPLAY);
            g.DrawRectangle(blackPen, Rx0);
            g.DrawRectangle(blackPen, Rx1);
            g.DrawRectangle(blackPen, Rx2);
            label_Rx2.Visible = true;
            label_DDC2.Visible = true;
            g.DrawRectangle(blackPen, Rx3);
            label_Rx3.Visible = true;
            label_DDC3.Visible = true;
            g.DrawRectangle(blackPen, Rx4);
            label_Rx4.Visible = true;
            label_DDC4.Visible = true;
            g.DrawRectangle(blackPen, Rx5);
            label_Rx5.Visible = true;
            label_DDC5.Visible = true;
            g.DrawRectangle(blackPen, Rx6);
            label_Rx6.Visible = true;
            label_DDC6.Visible = true;
            g.DrawRectangle(blackPen, ADC0);

            // do for tx and rx
            if (bool_Rx0_0 == true) ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
            if (bool_Rx0_1) ADC1_to_Rx0(bluePen);                       // draw ADC1 out to Rx0 input
            if (bool_Rx3_0) ADC0_to_Rx3(bluePen);                       // draw ADC0 out to Rx3 input // skip Rx1 and Rx2, assign them later
            if (bool_Rx3_1) ADC1_to_Rx3(bluePen);                       // draw ADC1 out to Rx3 input
            if (bool_Rx4_0) ADC0_to_Rx4(bluePen);                       // draw ADC0 out to Rx4 input
            if (bool_Rx4_1) ADC1_to_Rx4(bluePen);                       // draw ADC1 out to Rx4 input
            if (bool_Rx5_0) ADC0_to_Rx5(bluePen);                       // draw ADC0 out to Rx5 input
            if (bool_Rx5_1) ADC1_to_Rx5(bluePen);                       // draw ADC1 out to Rx5 input
            if (bool_Rx6_0) ADC0_to_Rx6(bluePen);                       // draw ADC0 out to Rx6 input
            if (bool_Rx6_1) ADC1_to_Rx6(bluePen);                       // draw ADC1 out to Rx6 input

            
            if (bool_rx)              // draw the path segments for RECEIVE mode  RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX 
            {
                // show only RX components
                g.DrawRectangle(blackPen, RX2_DISPLAY);
                label_RX2_DISPLAY.Visible = true;
                label_PA.Visible = false;
                label_AMP.Visible = false;
                label_FILTER.Visible = false;
                label_DAC0.Visible = false;
                label_DUC0.Visible = false;

                if (!bool_HPF_BYPASS)
                {
                    g.DrawRectangle(blackPen, HPF);
                    label_HPF.Visible = true;
                }
                else
                {
                    g.DrawLine(bluePen, HPF_B, HPF_center);
                    g.DrawLine(bluePen, HPF_center, HPF_R);
                }

                if (!bool_RX1_MUTE) label_RX1_LR_audio.Visible = true;
                else label_RX1_LR_audio.Visible = false;
                if (!bool_RX2_MUTE) label_RX2_LR_audio.Visible = true;
                else label_RX2_LR_audio.Visible = false;
                if (bool_RX1_MUTE & bool_RX2_MUTE)
                {
                    label_AUDIO_AMP.Visible = false;
                    label_CODEC2.Visible = false;
                    label_L_audio_only.Visible = false;
                    label_LR_audio.Visible = false;
                    label_AUDIO_MIXER.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    label_CODEC.Visible = false;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_L_audio_only.Visible = true;
                    label_LR_audio.Visible = true;
                    g.DrawRectangle(blackPen, AUDIO_AMP);
                }

                g.DrawLine(bluePen, HPF_R, ADC0_L);
                ADC1_to_RX2(bluePen);

                // for all conditions
                if (bool_diversity)
                {
                    ADC1_to_Rx1(bluePen);
                }

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_HPF_PA15(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    label_SWR.Visible = false;
                    XVTR_to_HPF_PA15(bluePen);
                    label_LPF.Visible = false;
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    label_SWR.Visible = false;
                    EXT1_to_HPF_PA15(bluePen);
                    label_LPF.Visible = false;
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & !bool_BYPASS)
                {
                    label_SWR.Visible = false;
                    EXT2_to_HPF_PA15(bluePen);
                    label_LPF.Visible = false;
                }

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_BYPASS(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;

                    XVTR_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_BYPASS(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    EXT1_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_BYPASS(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    EXT2_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_BYPASS(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }


                if (bool_Rx1_0) ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                if (bool_Rx1_1) ADC1_to_Rx1(bluePen);                       // draw ADC1 out to Rx1 input
                if (bool_Rx2_0) ADC0_to_Rx2(bluePen);                       // draw ADC0 out to Rx2 input
                if (bool_Rx2_1) ADC1_to_Rx2(bluePen);                       // draw ADC1 out to Rx2 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1
                Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                Rx2_to_DSP(bluePen);
                Rx3_to_DSP(bluePen);

                if (!bool_PureSignal & !bool_diversity)
                {
                    DSP_Rx2_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);

                }

                if (!bool_PureSignal & bool_diversity)
                {
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    DSP_in1_to_out1_crossconnect(bluePen);
                    draw_diversity_connection(bluePen);
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                }

                if (bool_PureSignal & !bool_diversity)
                {
                    DSP_Rx2_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                }

                if (bool_PureSignal & bool_diversity)
                {
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    DSP_in1_to_out1_crossconnect(bluePen);
                    draw_diversity_connection(bluePen);
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                }

                g.DrawRectangle(blackPen, ADC1);

            }
            else                           // draw the paths for TRANSMIT mode   TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX
            {
                g.DrawRectangle(blackPen, AMPF);
                g.DrawRectangle(blackPen, DAC0);
                g.DrawRectangle(blackPen, DUC0);

                if (!bool_HPF_BYPASS)
                {
                    g.DrawRectangle(blackPen, HPF);
                    label_HPF.Visible = true;
                }
                else
                {
                    g.DrawLine(bluePen, HPF_L, HPF_center);
                    g.DrawLine(bluePen, HPF_center, HPF_R);
                }

                g.DrawLine(bluePen, HPF_R, ADC0_L);

                g.DrawRectangle(blackPen, CODEC);
                label_PA.Visible = true;
                label_AMP.Visible = true;
                label_FILTER.Visible = true;
                label_DAC0.Visible = true;
                label_DUC0.Visible = true;
                label_CODEC.Visible = true;
                label_L_audio_only.Visible = false;
                label_AUDIO_AMP.Visible = false;
                label_CODEC2.Visible = false;
                label_LR_audio.Visible = false;
                label_RX1_LR_audio.Visible = false;
                label_RX2_LR_audio.Visible = false;
                label_AUDIO_MIXER.Visible = false;

                // handle HPF input circuitry                

                // EXT2_on_TX and EXT1_on_TX are switched on this model in PowerSDR checkbox logic, so switch them here
                bool bool_temp = bool_EXT1_on_TX;
                bool_EXT1_on_TX = bool_EXT2_on_TX;
                bool_EXT2_on_TX = bool_temp;

                if (!bool_BYPASS_on_TX & !bool_EXT1_on_TX & !bool_EXT2_on_TX) HPF_to_ground(bluePen);
                if (bool_BYPASS_on_TX) g.DrawLine(bluePen, C7, HPF_GROUND10);
                if (bool_EXT1_on_TX)
                {
                    C5_to_HPF_PA15_TX(bluePen);
                    g.DrawLine(bluePen, C7, HPF_GROUND10);          // BYPASS connector to RX2 ground
                }
                if (bool_EXT2_on_TX)
                {
                    C6_to_HPF_PA15_TX(bluePen);
                    g.DrawLine(bluePen, C7, HPF_GROUND10);          // BYPASS connector RX2 to ground
                }

                ADC1_to_ground(bluePen);

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    label_LPF.Visible = false;
                }

                if (!bool_PureSignal & !bool_diversity)
                {
                    DSP_Rx2_to_RX1_DISPLAY(bluePen);
                    if (bool_MON)
                    {
                        if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                        if (!bool_RX2_MUTE & bool_MON) SPKR_to_RX2_DISPLAY_2(bluePen);
                        label_L_audio_only.Visible = true;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_LR_audio.Visible = true;
                        label_RX1_LR_audio.Visible = true;
                        label_RX2_LR_audio.Visible = true;
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        g.DrawRectangle(blackPen, AUDIO_AMP);
                    }
                }

                if (!bool_PureSignal & bool_diversity)
                {
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    DSP_in1_to_out1_crossconnect(bluePen);
                    draw_diversity_connection(bluePen);
                    if (bool_MON | bool_duplex)
                    {
                        if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                        if (!bool_RX2_MUTE & bool_MON) SPKR_to_RX2_DISPLAY_2(bluePen);
                        label_L_audio_only.Visible = true;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_LR_audio.Visible = true;
                        label_RX1_LR_audio.Visible = true;
                        label_RX2_LR_audio.Visible = true;
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        g.DrawRectangle(blackPen, AUDIO_AMP);
                    }
                }

                if (bool_PureSignal & !bool_diversity)
                {
                    DSP_Rx2_to_RX1_DISPLAY(bluePen);
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);

                    if (bool_MON)
                    {
                        if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                        if (!bool_RX2_MUTE & bool_MON) SPKR_to_RX2_DISPLAY_2(bluePen);
                        label_L_audio_only.Visible = true;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_LR_audio.Visible = true;
                        label_RX1_LR_audio.Visible = true;
                        label_RX2_LR_audio.Visible = true;
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        g.DrawRectangle(blackPen, AUDIO_AMP);
                    }
                }

                if (bool_PureSignal & bool_diversity)
                {
                    DSP_Rx2_to_RX1_DISPLAY(bluePen);
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);

                    if (bool_MON | bool_duplex)
                    {
                        if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                        if (!bool_RX2_MUTE & bool_MON) SPKR_to_RX2_DISPLAY_2(bluePen);
                        label_L_audio_only.Visible = true;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_LR_audio.Visible = true;
                        label_RX1_LR_audio.Visible = true;
                        label_RX2_LR_audio.Visible = true;
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        g.DrawRectangle(blackPen, AUDIO_AMP);
                    }
                }

                basic_Tx_path(redPen);
                ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1

                if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    PA15_to_LPF(redPen);
                    AMPF_XVTR_TX(redPen);
                    AMPF_to_PA15(redPen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red
                    if (bool_Rx1_0) ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    if (bool_Rx1_1) ADC1_to_Rx1(bluePen);                       // draw ADC1 out to Rx1 input
                    if (bool_Rx2_0) ADC0_to_Rx2(bluePen);                       // draw ADC0 out to Rx2 input
                    if (bool_Rx2_1) ADC1_to_Rx2(bluePen);                       // draw ADC1 out to Rx2 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);
                }

                if (!bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    AMPF_XVTR_TX(redPen);
                    if (bool_Rx1_0) ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    if (bool_Rx1_1) ADC1_to_Rx1(bluePen);                       // draw ADC1 out to Rx1 input
                    if (bool_Rx2_0) ADC0_to_Rx2(bluePen);                       // draw ADC0 out to Rx2 input
                    if (bool_Rx2_1) ADC1_to_Rx2(bluePen);                       // draw ADC1 out to Rx2 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);

                }

                if (!bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    AMPF_XVTR_TX(redPen);
                    if (bool_Rx1_0) ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    if (bool_Rx1_1) ADC1_to_Rx1(bluePen);                       // draw ADC1 out to Rx1 input
                    if (bool_Rx2_0) ADC0_to_Rx2(bluePen);                       // draw ADC0 out to Rx2 input
                    if (bool_Rx2_1) ADC1_to_Rx2(bluePen);                       // draw ADC1 out to Rx2 input
                    DSP_in1_to_out1_crossconnect(bluePen);
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);

                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);

                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                    label_L_audio_only.Visible = true;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_LR_audio.Visible = true;
                    label_RX1_LR_audio.Visible = true;
                    label_RX2_LR_audio.Visible = true;
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    g.DrawRectangle(blackPen, AUDIO_AMP);
                }

                if (!bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red
                    if (bool_Rx1_0) ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    if (bool_Rx1_1) ADC1_to_Rx1(bluePen);                       // draw ADC1 out to Rx1 input
                    if (bool_Rx2_0) ADC0_to_Rx2(bluePen);                       // draw ADC0 out to Rx2 input
                    if (bool_Rx2_1) ADC1_to_Rx2(bluePen);                       // draw ADC1 out to Rx2 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);

                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                    label_L_audio_only.Visible = true;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_LR_audio.Visible = true;
                    label_RX1_LR_audio.Visible = true;
                    label_RX2_LR_audio.Visible = true;
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    g.DrawRectangle(blackPen, AUDIO_AMP);
                }

                if (bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    ADC1_to_Rx2(bluePen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red
                }

                if (bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    ADC1_to_Rx2(bluePen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in blue
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in blue
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in blue

                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);

                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                    label_L_audio_only.Visible = true;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_LR_audio.Visible = true;
                    label_RX1_LR_audio.Visible = true;
                    label_RX2_LR_audio.Visible = true;
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    g.DrawRectangle(blackPen, AUDIO_AMP);
                }

                if (bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                   label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    AMPF_XVTR_TX(redPen);
                    ADC1_to_Rx2(bluePen);
                }


                if (bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    ADC1_to_Rx2(bluePen);

                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);

                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                    label_L_audio_only.Visible = true;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_LR_audio.Visible = true;
                    label_RX1_LR_audio.Visible = true;
                    label_RX2_LR_audio.Visible = true;
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    g.DrawRectangle(blackPen, AUDIO_AMP);


                    if (bool_RX1_MUTE & bool_RX2_MUTE)
                    {

                    }
                    else
                    {
                        label_SWR.Visible = false;
                        label_LPF.Visible = false;
                        label_PA.Visible = false;
                        DUC0_to_Rx1(redPen);
                        Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                        AMPF_XVTR_TX(redPen);
                    }
                }
            }

            g.DrawRectangle(blackPen, SDR);
            update_diagram = false;

        } // end of draw_ANAN_100D_PA_rev15()


        private void draw_ANAN_100D_PA_rev24()
        {
            hide_all_labels();
            label_hardware_selected.Text = "Routing for ANAN-100D, ANAN-200D (PA rev_24)";
            label_SDR_Hardware.Visible = true;
            label_FPGA.Visible = true;
            label_PC.Visible = true;
            label_DSP.Visible = true;
            label_RX1_DISPLAY.Visible = true;
            if (bool_XVTR) label_XVTR_VHF.Visible = true; 
            label_NOTE.Visible = true;
            label_UNCHECK.Visible = true;

            hide_controls();

            Update_control_settings();

            // relocate LPF
            LPF.X = C10.X + 110;
            LPF.Y = C10.Y - 25;
            LPF_L.X = LPF.X;
            LPF_L.Y = LPF.Y + 25;
            LPF_L_c.X = LPF.X - 20;
            LPF_L_c.Y = LPF_L.Y;
            LPF_R.X = LPF.X + 50;
            LPF_R.Y = LPF.Y + 25;
            LPF_R_c.X = LPF.Y + 25;
            LPF_T.X = LPF.X + 25;
            LPF_T.Y = LPF.Y;
            LPF_BYPASS_corner.X = LPF.X + 25;
            LPF_BYPASS_corner.Y = C7.Y;

            // relocate SWR
            SWR.X = C10.X + 50;
            SWR.Y = C10.Y - 25;
            SWR_L.X = SWR.X;
            SWR_L.Y = SWR.Y + 25;
            SWR_R.X = SWR.X + 50;
            SWR_R.Y = SWR.Y + 25;
            SWR_T.X = SWR.X + 25;
            SWR_T.Y = SWR.Y;

            // relocate HPF 
            HPF.X = PA.X;
            HPF.Y = ADC0.Y;
            HPF_R.X = HPF.X + 50;
            HPF_R.Y = HPF.Y + 25;
            HPF_B.X = HPF.X + 25;
            HPF_B.Y = HPF.Y + 50;
            HPF_L.X = HPF.X;
            HPF_L.Y = HPF.Y + 25;

            LPF_HPF_corner.X = HPF.X + 25;
            LPF_HPF_corner.Y = C10.Y;
            XVTR_HPF_corner.X = HPF_B.X;
            XVTR_HPF_corner.Y = C4.Y;
            EXT1_HPF_corner.X = HPF_B.X;
            EXT1_HPF_corner.Y = C5.Y;
            EXT2_HPF_corner.X = HPF_B.X;
            EXT2_HPF_corner.Y = C6.Y;

            update_labels_PA();

            label_HPF.Visible = false;
            label_ADC0_atten.Visible = true;
            label_ADC0.Visible = true;
            label_DDC0.Visible = true;
            label_Rx0.Visible = true;
            label_DDC1.Visible = true;
            label_Rx1.Visible = true;
            label_ADC1_atten.Visible = true;
            label_ADC1.Visible = true;

            // draw some of the rectangles
            g.DrawRectangle(blackPen2, PC);
            g.DrawRectangle(indianredPen, FPGA);
            g.DrawRectangle(blackPen, DSP);
            g.DrawRectangle(blackPen, RX1_DISPLAY);
            g.DrawRectangle(blackPen, RX2_DISPLAY);
            label_RX2_DISPLAY.Visible = true;
            g.DrawRectangle(blackPen, Rx0);
            g.DrawRectangle(blackPen, Rx1);
            g.DrawRectangle(blackPen, Rx2);
            label_Rx2.Visible = true;
            label_DDC2.Visible = true;
            g.DrawRectangle(blackPen, Rx3);
            label_Rx3.Visible = true;
            label_DDC3.Visible = true;
            g.DrawRectangle(blackPen, Rx4);
            label_Rx4.Visible = true;
            label_DDC4.Visible = true;
            g.DrawRectangle(blackPen, Rx5);
            label_Rx5.Visible = true;
            label_DDC5.Visible = true;
            g.DrawRectangle(blackPen, Rx6);
            label_Rx6.Visible = true;
            label_DDC6.Visible = true;
            g.DrawRectangle(blackPen, ADC0);
            g.DrawRectangle(blackPen, ADC1);

            // do for both tx and rx
            ADC1_to_RX2(bluePen);
            if (bool_Rx0_0 == true) ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
            if (bool_Rx0_1) ADC1_to_Rx0(bluePen);                       // draw ADC1 out to Rx0 input
            if (bool_Rx3_0) ADC0_to_Rx3(bluePen);                       // draw ADC0 out to Rx3 input // skip Rx1 and Rx2, assign them later
            if (bool_Rx3_1) ADC1_to_Rx3(bluePen);                       // draw ADC1 out to Rx3 input
            if (bool_Rx4_0) ADC0_to_Rx4(bluePen);                       // draw ADC0 out to Rx4 input
            if (bool_Rx4_1) ADC1_to_Rx4(bluePen);                       // draw ADC1 out to Rx4 input
            if (bool_Rx5_0) ADC0_to_Rx5(bluePen);                       // draw ADC0 out to Rx5 input
            if (bool_Rx5_1) ADC1_to_Rx5(bluePen);                       // draw ADC1 out to Rx5 input
            if (bool_Rx6_0) ADC0_to_Rx6(bluePen);                       // draw ADC0 out to Rx6 input
            if (bool_Rx6_1) ADC1_to_Rx6(bluePen);                       // draw ADC1 out to Rx6 input



            if (bool_rx)              // draw the path segments for RECEIVE mode  RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX RX 
            {
                label_PA.Visible = false;
                label_AMP.Visible = false;
                label_FILTER.Visible = false;
                label_DAC0.Visible = false;
                label_DUC0.Visible = false;
                if (!bool_RX1_MUTE) label_RX1_LR_audio.Visible = true;
                else label_RX1_LR_audio.Visible = false;
                if (!bool_RX2_MUTE) label_RX2_LR_audio.Visible = true;
                else label_RX2_LR_audio.Visible = false;
                if (bool_RX1_MUTE & bool_RX2_MUTE)
                {
                    label_AUDIO_AMP.Visible = false;
                    label_CODEC2.Visible = false;
                    label_L_audio_only.Visible = false;
                    label_LR_audio.Visible = false;
                    label_AUDIO_MIXER.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    label_CODEC.Visible = false;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_L_audio_only.Visible = true;
                    label_LR_audio.Visible = true;
                    g.DrawRectangle(blackPen, AUDIO_AMP);
                }

                g.DrawLine(bluePen, HPF_R, ADC0_L);

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    if (bool_HPF_BYPASS)
                    {
                        g.DrawLine(bluePen, HPF_B, HPF_center);
                        g.DrawLine(bluePen, HPF_center, HPF_R);
                    }
                    else
                    {
                        g.DrawRectangle(blackPen, HPF);
                        label_HPF.Visible = true;
                    }
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    LPF_to_HPF_PA15(bluePen);
                    if (bool_ANT1) C9_to_LPF_L(bluePen);    // draw C9 to LPF input in blue
                    if (bool_ANT2) C10_to_LPF_L(bluePen);    // draw C10 to LPF input in blue
                    if (bool_ANT3) C11_to_LPF_L(bluePen);    // draw C11 to LPF input in blue
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2/* & !bool_BYPASS*/)
                {
                    g.DrawRectangle(blackPen, HPF);
                    label_HPF.Visible = true;
                    XVTR_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2/* & !bool_BYPASS*/)
                {
                    g.DrawRectangle(blackPen, HPF);
                    label_HPF.Visible = true;
                    EXT1_to_HPF_PA15(bluePen);
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2/* & !bool_BYPASS*/)
                {
                    g.DrawRectangle(blackPen, HPF);
                    label_HPF.Visible = true;
                    EXT2_to_HPF_PA15(bluePen);
                }
                if (bool_Rx1_0) ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                if (bool_Rx1_1) ADC1_to_Rx1(bluePen);                       // draw ADC1 out to Rx1 input
                if (bool_Rx2_0) ADC0_to_Rx2(bluePen);                       // draw ADC0 out to Rx2 input
                if (bool_Rx2_1) ADC1_to_Rx2(bluePen);                       // draw ADC1 out to Rx2 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1
                Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                Rx2_to_DSP(bluePen);
                Rx3_to_DSP(bluePen);

                if (!bool_PureSignal & !bool_diversity)
                {
                    DSP_Rx2_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                    label_L_audio_only.Visible = true;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_LR_audio.Visible = true;
                    label_RX1_LR_audio.Visible = true;
                    label_RX2_LR_audio.Visible = true;
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    g.DrawRectangle(blackPen, AUDIO_AMP);
                }

                if (!bool_PureSignal & bool_diversity)
                {
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    DSP_in1_to_out1_crossconnect(bluePen);
                    draw_diversity_connection(bluePen);
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                    label_L_audio_only.Visible = true;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_LR_audio.Visible = true;
                    label_RX1_LR_audio.Visible = true;
                    label_RX2_LR_audio.Visible = true;
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    g.DrawRectangle(blackPen, AUDIO_AMP);
                }

                if (bool_PureSignal & !bool_diversity)
                {
                    DSP_Rx2_to_RX1_DISPLAY(bluePen);
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                    label_L_audio_only.Visible = true;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_LR_audio.Visible = true;
                    label_RX1_LR_audio.Visible = true;
                    label_RX2_LR_audio.Visible = true;
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    g.DrawRectangle(blackPen, AUDIO_AMP);
                }

                if (bool_PureSignal & bool_diversity)
                {
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    DSP_in1_to_out1_crossconnect(bluePen);
                    draw_diversity_connection(bluePen);
                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                }

                g.DrawRectangle(blackPen, ADC1);

            }
            else                           // draw the paths for TRANSMIT mode   TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX TX
            {
                if (bool_EXT1_on_TX) bool_EXT1_on_TX = false;           // EXT1_on_TX not allowed with ANAN_PA_rev24
                if (bool_EXT2_on_TX) bool_EXT2_on_TX = false;           // EXT2_on_TX not allowed with ANAN_PA_rev24
                g.DrawRectangle(blackPen, AMPF);
                g.DrawRectangle(blackPen, DAC0);
                g.DrawRectangle(blackPen, DUC0);
                g.DrawRectangle(blackPen, CODEC);
                label_PA.Visible = true;
                label_AMP.Visible = true;
                label_FILTER.Visible = true;
                label_DAC0.Visible = true;
                label_DUC0.Visible = true;
                label_CODEC.Visible = true;
                label_L_audio_only.Visible = false;   // default no audio out on Tx
                label_AUDIO_AMP.Visible = false;
                label_CODEC2.Visible = false;
                label_LR_audio.Visible = false;
                label_RX1_LR_audio.Visible = false;
                label_RX2_LR_audio.Visible = false;
                label_AUDIO_MIXER.Visible = false;

                if (bool_EXT1_on_TX)
                {
                    return;
                }
                if (bool_EXT2_on_TX)
                {
                    return;
                }
                if (bool_BYPASS_on_TX)
                {
                    BYPASS_to_ADC0(bluePen);
                }
                if (!bool_EXT1_on_TX & !bool_EXT2_on_TX & !bool_BYPASS_on_TX) SWR_to_ADC0(bluePen);
                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & !bool_BYPASS)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                }

                if (!bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    BYPASS_to_ADC0(bluePen);
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                }

                if (bool_XVTR & !bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    return;
                }

                if (!bool_XVTR & bool_EXT1 & !bool_EXT2 & bool_BYPASS)
                {
                    return;
                }

                if (!bool_XVTR & !bool_EXT1 & bool_EXT2 & bool_BYPASS)
                {
                    return;
                }


                if (!bool_PureSignal & !bool_diversity)
                {
                    DSP_Rx2_to_RX1_DISPLAY(bluePen);

                    if (bool_MON)
                    {
                        if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                        if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                        label_L_audio_only.Visible = true;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_LR_audio.Visible = true;
                        label_RX1_LR_audio.Visible = true;
                        label_RX2_LR_audio.Visible = true;
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        g.DrawRectangle(blackPen, AUDIO_AMP);
                    }
                }

                if (!bool_PureSignal & bool_diversity)
                {
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    DSP_in1_to_out1_crossconnect(bluePen);
                    draw_diversity_connection(bluePen);

                    if (bool_MON | bool_duplex)
                    {
                        if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY(bluePen);
                        if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                        label_L_audio_only.Visible = true;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_LR_audio.Visible = true;
                        label_RX1_LR_audio.Visible = true;
                        label_RX2_LR_audio.Visible = true;
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        g.DrawRectangle(blackPen, AUDIO_AMP);
                    }
                }

                if (bool_PureSignal & !bool_diversity)
                {
                    DSP_Rx2_to_RX1_DISPLAY(bluePen);
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);

                    if (bool_MON)
                    {
                        if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                        if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                        label_L_audio_only.Visible = true;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_LR_audio.Visible = true;
                        label_RX1_LR_audio.Visible = true;
                        label_RX2_LR_audio.Visible = true;
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        g.DrawRectangle(blackPen, AUDIO_AMP);
                    }

                }

                if (bool_PureSignal & bool_diversity)
                {
                    DSP_Rx2_to_RX1_DISPLAY(bluePen);
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);

                    if (bool_MON | bool_duplex)
                    {
                        if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                        if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                        label_L_audio_only.Visible = true;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_LR_audio.Visible = true;
                        label_RX1_LR_audio.Visible = true;
                        label_RX2_LR_audio.Visible = true;
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        g.DrawRectangle(blackPen, AUDIO_AMP);
                    }
                }

                basic_Tx_path(redPen);
                ADC0_to_Rx0(bluePen);                       // draw ADC0 out to Rx0 input
                Rx0_to_DSP(bluePen);                        // draw Rx0 out to DSP input 1

                if (!bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    PA15_to_LPF(redPen);
                    AMPF_XVTR_TX(redPen);
                    AMPF_to_PA15(redPen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red
                    if (bool_Rx1_0) ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    if (bool_Rx1_1) ADC1_to_Rx1(bluePen);                       // draw ADC1 out to Rx1 input
                    if (bool_Rx2_0) ADC0_to_Rx2(bluePen);                       // draw ADC0 out to Rx2 input
                    if (bool_Rx2_1) ADC1_to_Rx2(bluePen);                       // draw ADC1 out to Rx2 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);

                }

                if (!bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    AMPF_XVTR_TX(redPen);
                    if (bool_Rx1_0) ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    if (bool_Rx1_1) ADC1_to_Rx1(bluePen);                       // draw ADC1 out to Rx1 input
                    if (bool_Rx2_0) ADC0_to_Rx2(bluePen);                       // draw ADC0 out to Rx2 input
                    if (bool_Rx2_1) ADC1_to_Rx2(bluePen);                       // draw ADC1 out to Rx2 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);
                }

                if (!bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    AMPF_XVTR_TX(redPen);
                    if (bool_Rx1_0) ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    if (bool_Rx1_1) ADC1_to_Rx1(bluePen);                       // draw ADC1 out to Rx1 input
                    if (bool_Rx2_0) ADC0_to_Rx2(bluePen);                       // draw ADC0 out to Rx2 input
                    if (bool_Rx2_1) ADC1_to_Rx2(bluePen);                       // draw ADC1 out to Rx2 input
                    DSP_in1_to_out1_crossconnect(bluePen);
                    DSP_out1_to_RX1_DISPLAY(bluePen);
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    DSP_out2_to_RX2_DISPLAY(bluePen);
                    DSP_in2_to_out2_crossconnect(bluePen);

                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                    label_L_audio_only.Visible = true;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_LR_audio.Visible = true;
                    label_RX1_LR_audio.Visible = true;
                    label_RX2_LR_audio.Visible = true;
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }

                if (!bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    PA15_to_LPF(redPen);
                    AMPF_XVTR_TX(redPen);
                    AMPF_to_PA15(redPen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red
                    if (bool_Rx1_0) ADC0_to_Rx1(bluePen);                       // draw ADC0 out to Rx1 input
                    if (bool_Rx1_1) ADC1_to_Rx1(bluePen);                       // draw ADC1 out to Rx1 input
                    if (bool_Rx2_0) ADC0_to_Rx2(bluePen);                       // draw ADC0 out to Rx2 input
                    if (bool_Rx2_1) ADC1_to_Rx2(bluePen);                       // draw ADC1 out to Rx2 input
                    Rx1_to_DSP(bluePen);                        // draw Rx1 out to DSP input 2
                    Rx2_to_DSP(bluePen);
                    Rx3_to_DSP(bluePen);

                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                    label_L_audio_only.Visible = true;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_LR_audio.Visible = true;
                    label_RX1_LR_audio.Visible = true;
                    label_RX2_LR_audio.Visible = true;
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }

                if (bool_PureSignal & !bool_duplex & !bool_XVTR)
                {
                    if (!bool_BYPASS) SWR_to_ADC0(bluePen);
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    ADC1_to_Rx2(bluePen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in red
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in red
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in red

                }

                if (bool_PureSignal & bool_duplex & !bool_XVTR)
                {
                    g.DrawRectangle(blackPen, SWR);
                    label_SWR.Visible = true;
                    g.DrawRectangle(blackPen, PA);
                    g.DrawRectangle(blackPen, LPF);
                    label_LPF.Visible = true;
                    label_PA.Visible = true;
                    AMPF_XVTR_TX(redPen);
                    PA15_to_LPF(redPen);
                    g.DrawLine(redPen, PA_corner, PA_B);
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    ADC1_to_Rx2(bluePen);
                    if (bool_ANT1_TX) C9_to_LPF_L(redPen);    // draw C9 to LPF input in blue
                    if (bool_ANT2_TX) C10_to_LPF_L(redPen);    // draw C10 to LPF input in blue
                    if (bool_ANT3_TX) C11_to_LPF_L(redPen);    // draw C11 to LPF input in blue

                    if (!bool_RX1_MUTE) SPKR_to_RX1_DISPLAY_2(bluePen);
                    if (!bool_RX2_MUTE) SPKR_to_RX2_DISPLAY_2(bluePen);
                    label_L_audio_only.Visible = true;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_LR_audio.Visible = true;
                    label_RX1_LR_audio.Visible = true;
                    label_RX2_LR_audio.Visible = true;
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }

                if (bool_PureSignal & !bool_duplex & bool_XVTR)
                {
                    label_SWR.Visible = false;
                    label_LPF.Visible = false;
                    label_PA.Visible = false;
                    Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                    DUC0_to_Rx1(redPen);
                    AMPF_XVTR_TX(redPen);
                    ADC1_to_Rx2(bluePen);
                }


                if (bool_PureSignal & bool_duplex & bool_XVTR)
                {
                    ADC1_to_Rx2(bluePen);

                    if (bool_RX1_MUTE & bool_RX2_MUTE)
                    {

                    }
                    else
                    {
                        label_SWR.Visible = false;
                        label_LPF.Visible = false;
                        label_PA.Visible = false;
                        DUC0_to_Rx1(redPen);
                        Rx1_to_DSP(redPen);                        // draw Rx1 out to DSP input 2
                        AMPF_XVTR_TX(redPen);
                    }
                }
            }
            g.DrawRectangle(blackPen, SDR);
            update_diagram = false;                     // prevents unnecessary automatic updating

        } // end of draw_ANAN_100D_PA_rev24()


        // end hardware-specific diagram drawing methods ******************************************************************** 

        // begin segment drawing methods ************************************************************************************

        private void ADC0_to_Rx0(Pen pen)
        {
            g.DrawLine(pen, ADC0_R, Rx0_L);
        }

        private void ADC0_to_Rx1(Pen pen)
        {
            g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx1);
            g.DrawLine(pen, ADC0_corner1_Rx1, ADC0_corner2_Rx1);
            g.DrawLine(pen, ADC0_corner2_Rx1, Rx1_L);
        }

        private void ADC0_to_Rx2(Pen pen)
        {
            g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx2);
            g.DrawLine(pen, ADC0_corner1_Rx2, ADC0_corner2_Rx2);
            g.DrawLine(pen, ADC0_corner2_Rx2, Rx2_L);
        }

        private void ADC0_to_Rx3(Pen pen)
        {
            g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx1);
            g.DrawLine(pen, ADC0_corner1_Rx3, ADC0_corner2_Rx3);
            g.DrawLine(pen, ADC0_corner2_Rx3, Rx3_L);
        }

        private void ADC0_to_Rx4(Pen pen)
        {
            g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx4);
            g.DrawLine(pen, ADC0_corner1_Rx4, ADC0_corner2_Rx4);
            g.DrawLine(pen, ADC0_corner2_Rx4, Rx4_L);
        }

        private void ADC0_to_Rx5(Pen pen)
        {
            g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx5);
            g.DrawLine(pen, ADC0_corner1_Rx5, ADC0_corner2_Rx5);
            g.DrawLine(pen, ADC0_corner2_Rx5, Rx5_L);
        }

        private void ADC0_to_Rx6(Pen pen)
        {
            g.DrawLine(pen, ADC0_R, ADC0_corner1_Rx6);
            g.DrawLine(pen, ADC0_corner1_Rx6, ADC0_corner2_Rx6);
            g.DrawLine(pen, ADC0_corner2_Rx6, Rx6_L);
        }

        private void ADC1_to_ground(Pen pen)
        {
            g.DrawLine(pen, ADC1_L, ADC1_L_corner3);
            g.DrawLine(pen, HPF_GROUND9, HPF_GROUND2);
            g.DrawLine(pen, HPF_GROUND3, HPF_GROUND4);
            g.DrawLine(pen, HPF_GROUND5, HPF_GROUND6);
            g.DrawLine(pen, HPF_GROUND7, HPF_GROUND8);

        }

        private void ADC1_to_Rx0(Pen pen)
        {
            g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx0);
            g.DrawLine(pen, ADC1_corner1_Rx0, ADC1_corner2_Rx0);
            g.DrawLine(pen, ADC1_corner2_Rx0, Rx0_ADC1_input);
        }

        private void ADC1_to_Rx1(Pen pen)
        {
            g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx1);
            g.DrawLine(pen, ADC1_corner1_Rx1, ADC1_corner2_Rx1);
            g.DrawLine(pen, ADC1_corner2_Rx1, Rx1_L);
        }

        private void ADC1_to_Rx2(Pen pen)
        {
            g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx2);
            g.DrawLine(pen, ADC1_corner1_Rx2, ADC1_corner2_Rx2);
            g.DrawLine(pen, ADC1_corner2_Rx2, Rx2_L);
        }

        private void ADC1_to_Rx3(Pen pen)
        {
            g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx3);
            g.DrawLine(pen, ADC1_corner1_Rx3, ADC1_corner2_Rx3);
            g.DrawLine(pen, ADC1_corner2_Rx3, Rx3_L);
        }

        private void ADC1_to_Rx4(Pen pen)
        {
            g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx4);
            g.DrawLine(pen, ADC1_corner1_Rx4, ADC1_corner2_Rx4);
            g.DrawLine(pen, ADC1_corner2_Rx4, Rx4_L);
        }

        private void ADC1_to_Rx5(Pen pen)
        {
            g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx5);
            g.DrawLine(pen, ADC1_corner1_Rx5, ADC1_corner2_Rx5);
            g.DrawLine(pen, ADC1_corner2_Rx5, Rx5_L);
        }

        private void ADC1_to_Rx6(Pen pen)
        {
            g.DrawLine(pen, ADC1_R, ADC1_corner1_Rx6);
            g.DrawLine(pen, ADC1_corner1_Rx6, ADC1_corner2_Rx6);
            g.DrawLine(pen, ADC1_corner2_Rx6, Rx6_L);
        }

        private void ADC1_to_RX2(Pen pen)
        {
            g.DrawLine(pen, ADC1_L, ADC1_L_corner1);
            g.DrawLine(pen, ADC1_L_corner1, ADC1_L_corner2);
            g.DrawLine(pen, ADC1_L_corner2, C2);
        }

        private void ALEX_ANT_to_HPF_B(Pen pen)
        {
            if (bool_ANT1)
            {
                g.DrawLine(pen, ALEX_ANT1, ALEX_ANT1_corner);
                g.DrawLine(pen, ALEX_ANT1_corner, ALEX_HPF_B);
            }
            if (bool_ANT2)
            {
                g.DrawLine(pen, ALEX_ANT2, ALEX_ANT2_corner);
                g.DrawLine(pen, ALEX_ANT2_corner, ALEX_HPF_B);
            }
            if (bool_ANT3)
            {
                g.DrawLine(pen, ALEX_ANT3, ALEX_ANT3_corner);
                g.DrawLine(pen, ALEX_ANT3_corner, ALEX_HPF_B);
            }
        }

        private void ALEX_2_ANT_to_HPF_B(Pen pen)
        {
            if (bool_ANT1)
            {
                g.DrawLine(pen, ALEX_2_ANT1, ALEX_2_ANT1_corner);
                g.DrawLine(pen, ALEX_2_ANT1_corner, ALEX_2_HPF_B);
            }
            if (bool_ANT2)
            {
                g.DrawLine(pen, ALEX_2_ANT2, ALEX_2_ANT2_corner);
                g.DrawLine(pen, ALEX_2_ANT2_corner, ALEX_2_HPF_B);
            }
            if (bool_ANT3)
            {
                g.DrawLine(pen, ALEX_2_ANT3, ALEX_2_ANT3_corner);
                g.DrawLine(pen, ALEX_2_ANT3_corner, ALEX_2_HPF_B);
            }
        }

        private void ALEX_TX_ANT(Pen pen)
        {
            if (bool_ANT1_TX)
            {
                g.DrawLine(pen, PENELOPE_PA_out_corner3, ALEX_LPF_corner1);
                g.DrawLine(pen, ALEX_LPF_corner1, ALEX_LPF_ANT1);
            }
            if (bool_ANT2_TX)
            {
                g.DrawLine(pen, PENELOPE_PA_out_corner3, ALEX_LPF_corner2);
                g.DrawLine(pen, ALEX_LPF_corner2, ALEX_LPF_ANT2);

            }
            if (bool_ANT3_TX)
            {
                g.DrawLine(pen, PENELOPE_PA_out_corner3, ALEX_LPF_corner3);
                g.DrawLine(pen, ALEX_LPF_corner3, ALEX_LPF_ANT3);

            }
        }

        private void ALEX_2_RX_out_to_ADC(Pen pen)
        {
            g.DrawLine(pen, ALEX_2_RX_out, ALEX_2_RX_out_corner1);
            g.DrawLine(pen, ALEX_2_RX_out_corner1, MERCURY_2_ADC_in_corner1);
            g.DrawLine(pen, MERCURY_2_ADC_in_corner1, MERCURY_2_ADC_in);
        }

        private void AMPF_TX_path_PA10(Pen pen)
        {
            g.DrawLine(pen, AMPF_L, TX_AMP_corner);
            g.DrawLine(pen, TX_AMP_corner, TX_corner_PA10);
            g.DrawLine(pen, TX_corner_PA10, C6);
        }

        private void AMPF_to_PA15(Pen pen)
        {
            g.DrawLine(pen, AMPF_L, AMPF_L_PA15);
            g.DrawLine(pen, AMPF_L_PA15, PA_B);
        }

        private void AMPF_TX_path_PA15(Pen pen)
        {
            g.DrawLine(pen, AMPF_L, TX_AMP_corner);
            g.DrawLine(pen, TX_AMP_corner, TX_corner_PA15);
            g.DrawLine(pen, TX_corner_PA15, C13);
        }

        private void AMPF_XVTR_TX(Pen pen)
        {
            g.DrawLine(pen, AMPF_L, TX_AMP_corner);
            g.DrawLine(pen, TX_AMP_corner, TX_corner_PA15);
            g.DrawLine(pen, TX_corner_PA15, C13);
        }


        private void basic_Tx_path(Pen pen)
        {
            g.DrawLine(pen, MIC_R, CODEC_L);
            g.DrawLine(pen, CODEC_R, DSP_bottom_corner);
            g.DrawLine(pen, DSP_bottom_corner, DSP_B_2);
            g.DrawLine(pen, DSP_B_2, DSP_loopback_1);
            g.DrawLine(pen, DSP_loopback_1, DSP_loopback_2);
            g.DrawLine(pen, DSP_loopback_2, DSP_B_1);
            g.DrawLine(pen, DSP_B_1, DUC0_R_corner);
            g.DrawLine(pen, DUC0_R_corner, DUC0_R);
            g.DrawLine(pen, DUC0_L, DAC0_R);
            g.DrawLine(pen, DAC0_L, AMPF_R);
            g.DrawLine(pen, AMPF_L, PA_corner);
        }

        private void BYPASS_to_ADC0(Pen pen)
        {
            g.DrawLine(pen, C7, BYPASS_corner1);
            g.DrawLine(pen, BYPASS_corner1, BYPASS_corner2);
            g.DrawLine(pen, BYPASS_corner2, ADC0_L);
        }

        private void C2_to_Rx0(Pen pen)
        {
            g.DrawLine(pen, C2, ADC0_L);
        }

        private void C3_to_Rx0(Pen pen)
        {
            g.DrawLine(pen, C3, C3_corner2);
            g.DrawLine(pen, C3_corner2, C3_corner3);
            g.DrawLine(pen, C3_corner3, ADC0_L);

        }

        private void C4_to_HPF_PA15_TX(Pen pen)
        {
            g.DrawLine(pen, C4, HPF_TX_corner1);
            g.DrawLine(pen, HPF_TX_corner1, HPF_TX_corner2);
            g.DrawLine(pen, HPF_TX_corner2, HPF_L);
        }

        private void C4_to_Rx0(Pen pen)
        {
            g.DrawLine(pen, C4, C4_corner2);
            g.DrawLine(pen, C4_corner2, C4_corner3);
            g.DrawLine(pen, C4_corner3, ADC0_L);
        }

        private void C2_to_LPF(Pen pen)
        {
            g.DrawLine(pen, C2, C2_c);
            g.DrawLine(pen, C2_c, C2_corner);
            g.DrawLine(pen, C2_corner, LPF_in_corner);
            g.DrawLine(pen, LPF_in_corner, LPF_L);
        }

        private void C3_to_LPF(Pen pen)
        {
            g.DrawLine(pen, C3, C3_c);
            g.DrawLine(pen, C3_c, C3_corner);
            g.DrawLine(pen, C3_corner, LPF_in_corner);
            g.DrawLine(pen, LPF_in_corner, LPF_L);
        }

        private void C4_to_LPF(Pen pen)
        {
            g.DrawLine(pen, C4, C4_c);
            g.DrawLine(pen, C4_c, C4_corner);
            g.DrawLine(pen, C4_corner, LPF_in_corner);
            g.DrawLine(pen, LPF_in_corner, LPF_L);
        }

        private void C5_to_ADC0(Pen pen)
        {
            g.DrawLine(pen, C5, C5_corner);
            g.DrawLine(pen, C5_corner, C5_riser);
            g.DrawLine(pen, C5_riser, ADC0_L);
        }

        private void C5_to_HPF_PA15_TX(Pen pen)
        {
            g.DrawLine(pen, C5, HPF_TX_corner3);
            g.DrawLine(pen, HPF_TX_corner3, HPF_TX_corner2);
            g.DrawLine(pen, HPF_TX_corner2, HPF_L);
        }

        private void C6_to_HPF_PA15_TX(Pen pen)
        {
            g.DrawLine(pen, C6, HPF_TX_corner6);
            g.DrawLine(pen, HPF_TX_corner6, HPF_TX_corner5);
            g.DrawLine(pen, HPF_TX_corner5, HPF_L);
        }


        private void C7_to_ground(Pen pen)
        {
            g.DrawLine(bluePen, C7, HPF_GROUND10);
            g.DrawLine(bluePen, HPF_GROUND10, HPF_GROUND2);
            g.DrawLine(bluePen, HPF_GROUND3, HPF_GROUND4);
            g.DrawLine(bluePen, HPF_GROUND5, HPF_GROUND6);
            g.DrawLine(bluePen, HPF_GROUND7, HPF_GROUND8);
        }

        private void C9_to_LPF_L(Pen pen)
        {
            g.DrawLine(pen, C9, C9_c);
            g.DrawLine(pen, C9_c, C10_c);
            g.DrawLine(pen, C10_c, SWR_L);
            g.DrawLine(pen, SWR_R, LPF_L);
        }

        private void C10_to_LPF_L(Pen pen)
        {
            g.DrawLine(pen, C10, SWR_L);
            g.DrawLine(pen, SWR_R, LPF_L);
        }

        private void C11_to_LPF_L(Pen pen)
        {
            g.DrawLine(pen, C11, C11_c);
            g.DrawLine(pen, C11_c, C10_c);
            g.DrawLine(pen, C10_c, SWR_L);
            g.DrawLine(pen, SWR_R, LPF_L);
        }

        private void CODEC2_to_AUDIO_MIXER(Pen pen)
        {
            g.DrawLine(pen, CODEC2_R, AUDIO_MIXER_external_corner);
            g.DrawLine(pen, AUDIO_MIXER_external_corner, AUDIO_MIXER_B);
        }

        private void draw_diversity_connection(Pen pen)
        {
            g.DrawLine(pen, DSP_L_2, DSP_internal_diversity_corner1);
            g.DrawLine(pen, DSP_internal_diversity_corner1, DSP_internal_diversity_corner2);
        }

        private void DSP_in1_to_out1_crossconnect(Pen pen)
        {
            g.DrawLine(pen, DSP_L_1, DSP_R_1);
        }

        private void DSP_in2_to_out2_crossconnect(Pen pen)
        {
            g.DrawLine(pen, DSP_L_2, DSP_R_2);
        }

        private void DSP_out1_to_RX1_DISPLAY(Pen pen)
        {
            g.DrawLine(pen, DSP_R_1, DSP_R_1_c);
            g.DrawLine(pen, DSP_R_1_c, RX1_DISPLAY_corner1);
            g.DrawLine(pen, RX1_DISPLAY_corner1, RX1_DISPLAY_L);
        }

        private void DSP_out2_to_RX2_DISPLAY(Pen pen)
        {
            g.DrawLine(pen, DSP_R_2, RX2_L_DSP_2);
        }

        private void DUC0_to_Rx1(Pen pen)
        {
            g.DrawLine(pen, DUC0_L_corner_lower_pt, DUC0_L_corner);
            g.DrawLine(pen, DUC0_L_corner, Rx1_L);
        }

        private void EXT1_to_HPF_PA15(Pen pen)
        {
            g.DrawLine(pen, C5, EXT1_HPF_corner);
            g.DrawLine(pen, EXT1_HPF_corner, HPF_B);
        }

        private void EXT2_to_HPF_PA15(Pen pen)
        {
            g.DrawLine(pen, C6, EXT2_HPF_corner);
            g.DrawLine(pen, EXT2_HPF_corner, HPF_B);
        }

        private void HEADPHONES_to_CODEC2(Pen pen)
        {
            g.DrawLine(pen, HEADPHONES, HEADPHONES_1);
            g.DrawLine(pen, HEADPHONES_1, HEADPHONES_2);
            g.DrawLine(pen, HEADPHONES_2, CODEC2_L);
        }

        private void HERMES_PA_to_ALEX_LPF(Pen pen)
        {
            g.DrawLine(pen, PA_T, HERMES_PA_corner1);
            g.DrawLine(pen, HERMES_PA_corner1, PENELOPE_PA_out_corner1);
            g.DrawLine(pen, PENELOPE_PA_out_corner1, PENELOPE_PA_out_corner2);
            g.DrawLine(pen, PENELOPE_PA_out_corner2, PENELOPE_PA_out_corner3);
        }

        private void HPF_to_ground(Pen pen)
        {
            HPF_L.X = HPF.X;
            HPF_L.Y = HPF.Y + 25;

            g.DrawLine(pen, HPF_L, HPF_GROUND1);
            g.DrawLine(pen, HPF_GROUND1, HPF_GROUND2);
            g.DrawLine(pen, HPF_GROUND3, HPF_GROUND4);
            g.DrawLine(pen, HPF_GROUND5, HPF_GROUND6);
            g.DrawLine(pen, HPF_GROUND7, HPF_GROUND8);

        }

        private void HPSDR_DSP_to_AUDIO_MIXER_input_1(Pen pen)
        {
            g.DrawLine(pen, AUDIO_MIXER_L_1, DSP_internal_MIXER1_1);
            g.DrawLine(pen, DSP_internal_MIXER1_1, DSP_internal_MIXER1_3);
        }

        private void HPSDR_DSP_to_AUDIO_MIXER_input_2(Pen pen)
        {
            g.DrawLine(pen, AUDIO_MIXER_L_2, DSP_internal_MIXER2_1);
            g.DrawLine(pen, DSP_internal_MIXER2_1, DSP_internal_MIXER2_3);
        }

        private void line_to_ground(Pen pen)
        {
            g.DrawLine(pen, GROUND1, GROUND2);
            g.DrawLine(pen, GROUND3, GROUND4);
            g.DrawLine(pen, GROUND5, GROUND6);
            g.DrawLine(pen, GROUND7, GROUND8);
        }

        private void loopback_to_RX1_DISPLAY(Pen pen)
        {
            g.DrawLine(pen, loopback_center, loopback_center2);
            g.DrawLine(pen, loopback_center2, DSP_R_1_c);
            g.DrawLine(pen, DSP_R_1_c, RX1_DISPLAY_corner1);
            g.DrawLine(pen, RX1_DISPLAY_corner1, RX1_DISPLAY_L);
        }

        private void LPF_to_ADC0(Pen pen)
        {
            g.DrawLine(pen, LPF_R, LPF_R_c);
            g.DrawLine(pen, LPF_R_c, C5_riser);
            g.DrawLine(pen, C5_riser, ADC0_L);
        }

        private void LPF_to_C2(Pen pen)
        {
            g.DrawLine(pen, LPF_T, LPF_corner_C2);
            g.DrawLine(pen, LPF_corner_C2, C2);
        }

        private void LPF_to_C3(Pen pen)
        {
            g.DrawLine(pen, LPF_T, LPF_corner_C3);
            g.DrawLine(pen, LPF_corner_C3, C3);

        }

        private void LPF_to_C4(Pen pen)
        {
            g.DrawLine(pen, LPF_T, LPF_corner_C4);
            g.DrawLine(pen, LPF_corner_C4, C4);

        }

        private void LPF_to_HPF_PA15(Pen pen)
        {
            g.DrawLine(pen, LPF_R, LPF_HPF_corner);
            g.DrawLine(pen, LPF_HPF_corner, HPF_B);

        }

        private void MERCURY_ADC_to_DDCs(Pen pen)
        {
            g.DrawLine(pen, MERCURY_ADC_out, MERCURY_DDC0_in);
            g.DrawLine(pen, MERCURY_FPGA_corner1, MERCURY_FPGA_corner2);
            g.DrawLine(pen, MERCURY_FPGA_corner2, MERCURY_DDC1_in);
            g.DrawLine(pen, MERCURY_FPGA_corner2, MERCURY_FPGA_corner3);
            g.DrawLine(pen, MERCURY_FPGA_corner3, MERCURY_DDC2_in);
        }

        private void MERCURY_DDC0_to_RX1_DISPLAY(Pen pen)
        {
            g.DrawLine(pen, MERCURY_DDC0_out, RX1_DISPLAY_HPSDR_DDC0);
        }

        private void MERCURY_DDC1_to_RX2_DISPLAY(Pen pen)
        {
            g.DrawLine(pen, MERCURY_DDC1_out, RX2_DISPLAY_HPSDR_DDC1);
        }

        private void MERCURY_2_ADC_to_DDCs(Pen pen)
        {
            g.DrawLine(pen, MERCURY_2_ADC_out, MERCURY_2_DDC0_in);
            g.DrawLine(pen, MERCURY_2_FPGA_corner1, MERCURY_2_FPGA_corner2);
            g.DrawLine(pen, MERCURY_2_FPGA_corner2, MERCURY_2_DDC1_in);
            g.DrawLine(pen, MERCURY_2_FPGA_corner2, MERCURY_2_FPGA_corner3);
            g.DrawLine(pen, MERCURY_2_FPGA_corner3, MERCURY_2_DDC2_in);
        }

        private void MERCURY_2_DDC0_to_METIS_diversity(Pen pen)
        {
            g.DrawLine(pen, MERCURY_2_DDC0_out, MERCURY_2_DDC0_out_corner1);
            g.DrawLine(pen, MERCURY_2_DDC0_out_corner1, MERCURY_2_DDC0_out_corner2);
            g.DrawLine(pen, MERCURY_2_DDC0_out_corner2, METIS_corner1);
            g.DrawLine(pen, METIS_corner1, METIS_corner2);

        }

        private void MERCURY_2_DDC0_to_RX2_DISPLAY(Pen pen)
        {
            g.DrawLine(pen, MERCURY_2_DDC0_out, MERCURY_2_DDC0_out_corner1);
            g.DrawLine(pen, MERCURY_2_DDC0_out_corner1, MERCURY_2_DDC0_out_corner2);
            g.DrawLine(pen, MERCURY_2_DDC0_out_corner2, RX2_DISPLAY_HPSDR_DDC1);
        }

        private void MERCURY_2_AUDIO_INPUT(Pen pen)
        {
            g.DrawLine(pen, MERCURY_2_CODEC_out, MERCURY_2_CODEC_corner1);
            g.DrawLine(pen, MERCURY_2_CODEC_corner1, MERCURY_2_CODEC_corner2);
            g.DrawLine(pen, MERCURY_2_CODEC_LINEOUT, MERCURY_2_CODEC_OUT1);
            g.DrawLine(pen, MERCURY_2_CODEC_PHONES, MERCURY_2_CODEC_OUT2);
        }

        private void MERCURY_RX1_to_AUDIO_MIXER(Pen pen)
        {
            g.DrawLine(pen, MERCURY_CODEC_LINEOUT, MERCURY_CODEC_OUT1);
            g.DrawLine(pen, MERCURY_CODEC_PHONES, MERCURY_CODEC_OUT2);
            g.DrawLine(pen, MERCURY_CODEC_IN, MERCURY_CODEC_corner1);
            g.DrawLine(pen, MERCURY_CODEC_corner1, MERCURY_CODEC_corner2);
            g.DrawLine(pen, MERCURY_CODEC_corner2, AUDIO_MIXER_external_corner2);
            g.DrawLine(pen, AUDIO_MIXER_external_corner2, AUDIO_MIXER_internal_corner1);
            g.DrawLine(pen, AUDIO_MIXER_internal_corner1, AUDIO_MIXER_L_1);
        }

        private void MERCURY_RX2_to_AUDIO_MIXER(Pen pen)
        {
            g.DrawLine(pen, MERCURY_CODEC_LINEOUT, MERCURY_CODEC_OUT1);
            g.DrawLine(pen, MERCURY_CODEC_PHONES, MERCURY_CODEC_OUT2);
            g.DrawLine(pen, MERCURY_CODEC_IN, MERCURY_CODEC_corner1);
            g.DrawLine(pen, MERCURY_CODEC_corner1, MERCURY_CODEC_corner2);
            g.DrawLine(pen, MERCURY_CODEC_corner2, AUDIO_MIXER_external_corner2);
            g.DrawLine(pen, AUDIO_MIXER_external_corner2, AUDIO_MIXER_internal_corner2);
            g.DrawLine(pen, AUDIO_MIXER_internal_corner2, AUDIO_MIXER_L_2);
        }
        private void PA_to_LPF(Pen pen)
        {
            g.DrawLine(pen, PA_T, LPF_B);
        }

        private void LPF_to_BYPASS(Pen pen)
        {
            g.DrawLine(pen, LPF_T, LPF_BYPASS_corner);
            g.DrawLine(pen, LPF_BYPASS_corner, C7);
        }

        private void PA15_to_LPF(Pen pen)
        {
            LPF_R.X = LPF.X + 50;
            LPF_R.Y = LPF.Y + 25;
            PA15_corner3.X = PA_T.X;
            PA15_corner3.Y = LPF_R.Y;
            g.DrawLine(pen, PA_T, PA15_corner3);
            g.DrawLine(pen, PA15_corner3, LPF_R);
        }

        private void PENELOPE_PA_to_ALEX_LPF(Pen pen)
        {
            g.DrawLine(pen, PENELOPE_PA_out, PENELOPE_PA_out_corner1);
            g.DrawLine(pen, PENELOPE_PA_out_corner1, PENELOPE_PA_out_corner2);
            g.DrawLine(pen, PENELOPE_PA_out_corner2, PENELOPE_PA_out_corner3);

        }

        private void PENELOPE_PA_to_DSP(Pen pen)
        {
            g.DrawLine(pen, PENELOPE_PA_R, PENELOPE_AMPF_L);
            g.DrawLine(pen, PENELOPE_AMPF_R, PENELOPE_DAC_L);
            g.DrawLine(pen, PENELOPE_DAC_R, PENELOPE_DUC_L);
            g.DrawLine(pen, PENELOPE_DUC_R, PC_PENELOPE_corner1);
            g.DrawLine(pen, PC_PENELOPE_corner1, PC_PENELOPE_corner2);
            g.DrawLine(pen, PC_PENELOPE_corner2, PC_PENELOPE_corner3);
            g.DrawLine(pen, PC_PENELOPE_corner3, PC_PENELOPE_corner4);

            g.DrawLine(pen, PENELOPE_PA_XVTR_1, PENELOPE_PA_XVTR_2);
            g.DrawLine(pen, PENELOPE_PA_XVTR_2, PENELOPE_PA_XVTR_3);

        }

        private void PENELOPE_DSP_to_CODEC(Pen pen)
        {
            g.DrawLine(pen, PC_PENELOPE_corner4, PENELOPE_CODEC_R);
            g.DrawLine(pen, PENELOPE_CODEC_MIC_1, PENELOPE_CODEC_MIC_2);
            g.DrawLine(pen, PENELOPE_CODEC_LINE_IN_1, PENELOPE_CODEC_LINE_IN_2);
        }

        private void Rx0_to_DSP(Pen pen)
        {
            g.DrawLine(pen, Rx0_R, DSP_L_corner_Rx0);
            g.DrawLine(pen, DSP_L_corner_Rx0, DSP_L_corner2_Rx0);
            g.DrawLine(pen, DSP_L_corner2_Rx0, DSP_L_1);

        }

        private void Rx1_to_DSP(Pen pen)
        {
            g.DrawLine(pen, Rx1_R, DSP_L_2);
        }

        private void Rx2_to_DSP(Pen pen)
        {
            g.DrawLine(pen, Rx2_R, DSP_L_3);
        }

        private void DSP_Rx2_to_RX1_DISPLAY(Pen pen)
        {
            g.DrawLine(pen, DSP_L_3, DSP_R_3);
            g.DrawLine(pen, DSP_R_3, DSP_R_3_RX1_corner);
            g.DrawLine(pen, DSP_R_3_RX1_corner, RX1_DISPLAY_L);
        }

        private void Rx3_to_DSP(Pen pen)
        {
            g.DrawLine(pen, Rx3_R, DSP_L_4);
            g.DrawLine(pen, DSP_L_4, DSP_R_4);
            g.DrawLine(pen, DSP_R_4, DSP_R_4_RX2_corner);
            g.DrawLine(pen, DSP_R_4_RX2_corner, RX2_DISPLAY_L);

        }

        private void SPKR_to_DSP1(Pen pen)
        {
            g.DrawLine(pen, C24, SPKR_corner1);
            g.DrawLine(pen, SPKR_corner1, SPKR_DSP1);
            g.DrawLine(pen, SPKR_DSP1, DSP_R_1);
        }

        private void SPKR_to_DSP2(Pen pen)
        {
            g.DrawLine(pen, C24, SPKR_corner1);
            g.DrawLine(pen, SPKR_corner1, SPKR_DSP2);
            g.DrawLine(pen, SPKR_DSP2, DSP_R_2);
        }

        private void SPKR_to_RX1_DISPLAY(Pen pen)
        {
            g.DrawLine(pen, C24, AUDIO_AMP_L);
            g.DrawLine(pen, AUDIO_AMP_R, CODEC2_L);
            HEADPHONES_to_CODEC2(bluePen);
            CODEC2_to_AUDIO_MIXER(bluePen);
            if (!bool_RX1_MUTE)
            {
                g.DrawLine(pen, DSP_MIXER_1, AUDIO_MIXER_L_1);
                g.DrawLine(pen, AUDIO_MIXER_B, AUDIO_MIXER_internal_corner1);
                g.DrawLine(pen, AUDIO_MIXER_internal_corner1, AUDIO_MIXER_L_1);
                g.DrawLine(pen, AUDIO_MIXER_L_1, DSP_internal_MIXER1_1);
                g.DrawLine(pen, DSP_internal_MIXER1_1, DSP_internal_MIXER1_2);
            }
        }

        private void SPKR_to_RX1_DISPLAY_2(Pen pen)
        {
            g.DrawLine(pen, C24, AUDIO_AMP_L);
            g.DrawLine(pen, AUDIO_AMP_R, CODEC2_L);
            HEADPHONES_to_CODEC2(bluePen);
            CODEC2_to_AUDIO_MIXER(bluePen);
            if (!bool_RX1_MUTE)
            {
                g.DrawLine(pen, DSP_MIXER_1, AUDIO_MIXER_L_1);
                g.DrawLine(pen, AUDIO_MIXER_B, AUDIO_MIXER_internal_corner1);
                g.DrawLine(pen, AUDIO_MIXER_internal_corner1, AUDIO_MIXER_L_1);
                g.DrawLine(pen, AUDIO_MIXER_L_1, DSP_internal_MIXER1_1);
                g.DrawLine(pen, DSP_internal_MIXER1_1, DSP_internal_Rx2_audio_connection);
            }
        }

        private void SPKR_to_RX2_DISPLAY(Pen pen)
        {
            g.DrawLine(pen, C24, AUDIO_AMP_L);
            g.DrawLine(pen, AUDIO_AMP_R, CODEC2_L);
            HEADPHONES_to_CODEC2(bluePen);
            CODEC2_to_AUDIO_MIXER(bluePen);
            if (!bool_RX2_MUTE)
            {
                g.DrawLine(pen, DSP_MIXER_2, AUDIO_MIXER_L_2);
                g.DrawLine(pen, AUDIO_MIXER_B, AUDIO_MIXER_internal_corner2);
                g.DrawLine(pen, AUDIO_MIXER_internal_corner2, AUDIO_MIXER_L_2);
                g.DrawLine(pen, AUDIO_MIXER_L_2, DSP_internal_MIXER2_1);
                g.DrawLine(pen, DSP_internal_MIXER2_1, DSP_internal_MIXER2_2);
            }
        }

        private void SPKR_to_RX2_DISPLAY_2(Pen pen)
        {
            g.DrawLine(pen, C24, AUDIO_AMP_L);
            g.DrawLine(pen, AUDIO_AMP_R, CODEC2_L);
            HEADPHONES_to_CODEC2(bluePen);
            CODEC2_to_AUDIO_MIXER(bluePen);
            if (!bool_RX2_MUTE)
            {
                g.DrawLine(pen, DSP_MIXER_2, AUDIO_MIXER_L_2);
                g.DrawLine(pen, AUDIO_MIXER_B, AUDIO_MIXER_internal_corner2);
                g.DrawLine(pen, AUDIO_MIXER_internal_corner2, AUDIO_MIXER_L_2);
                g.DrawLine(pen, AUDIO_MIXER_L_2, DSP_internal_MIXER2_1);
                g.DrawLine(pen, DSP_internal_MIXER2_1, DSP_internal_Rx3_audio_connection);
            }
        }

        private void SWR_to_ADC0(Pen pen)
        {
            g.DrawLine(pen, SWR_T, SWR_corner_ADC0);
            g.DrawLine(pen, SWR_corner_ADC0, ADC0_L);
        }

        private void update_labels_PA10()
        {
            // initially hide all rear panel labels in the display 
            hide_rear_panel_labels();

            //activate and re-label the relevant rear panel labels
            label_rear_panel.Location = new Point(8, 40);
            label_C2.Text = "ANT1";
            C2_label.X = C2.X - 45;
            C2_label.Y = C2.Y - 6;
            label_C2.Location = C2_label;
            label_C2.Visible = true;

            label_C3.Text = "ANT2";
            C3_label.X = C3.X - 45;
            C3_label.Y = C3.Y - 6;
            label_C3.Location = C3_label;
            label_C3.Visible = true;

            label_C4.Text = "ANT3";
            C4_label.X = C4.X - 45;
            C4_label.Y = C4.Y - 6;
            label_C4.Location = C4_label;
            label_C4.Visible = true;

            label_C5.Text = "RX (SMA)";
            C5_label.X = C5.X - 65;
            C5_label.Y = C5.Y - 6;
            label_C5.Location = C5_label;
            label_C5.Visible = true;

            label_C6.Text = "TX (SMA)";
            C6_label.X = C6.X - 65;
            C6_label.Y = C6.Y - 6;
            label_C6.Location = C6_label;
            label_C6.Visible = true;

            label_C7.Text = "XVRT (SMA)";
            C7_label.X = C7.X - 80;
            C7_label.Y = C7.Y - 6;
            label_C7.Location = C7_label;
            label_C7.Visible = true;

            label_C24.Text = "SPKR";
            C24_label.X = C24.X - 45;
            C24_label.Y = C24.Y - 6;
            label_C24.Location = C24_label;


            label_C26.Text = "MIC";
            C26_label.X = C26.X - 35;
            C26_label.Y = C26.Y - 6;
            label_C26.Location = C26_label;

            label_C25.Text = "HDPHONES";
            C25_label.X = C25.X - 75;
            C25_label.Y = C25.Y - 6;
            label_C25.Location = C25_label;

            LPF_label.X = LPF.X + 10;
            LPF_label.Y = LPF.Y + 17;
            label_LPF.Location = LPF_label;

            label_SWR.Visible = false;

        }

        private void update_labels_PA()
        {
            // initially hide all rear panel labels in the display 
            hide_rear_panel_labels();

            //activate and re-label the relevant rear panel labels
            label_rear_panel.Location = new Point(8, 40);

            label_C2.Text = "RX 2";
            C2_label.X = C2.X - 35;
            C2_label.Y = C2.Y - 6;
            label_C2.Location = C2_label;
            label_C2.Visible = true;

            label_C4.Text = "XVRT RX";
            C4_label.X = C4.X - 60;
            C4_label.Y = C4.Y - 5;
            label_C4.Location = C4_label;
            label_C4.Visible = true;

            label_C5.Text = "EXT1";
            C5_label.X = C5.X - 40;
            C5_label.Y = C5.Y - 5;
            label_C5.Location = C5_label;
            label_C5.Visible = true;

            label_C6.Text = "EXT2";
            C6_label.X = C6.X - 40;
            C6_label.Y = C6.Y - 5;
            label_C6.Location = C6_label;
            label_C6.Visible = true;

            label_C7.Text = "BYPASS";
            C7_label.X = C7.X - 57;
            C7_label.Y = C7.Y - 5;
            label_C7.Location = C7_label;
            label_C7.Visible = true;

            label_C9.Text = "ANT1";
            C9_label.X = C9.X - 40;
            C9_label.Y = C9.Y - 5;
            label_C9.Location = C9_label;
            label_C9.Visible = true;

            label_C10.Text = "ANT2";
            C10_label.X = C10.X - 41;
            C10_label.Y = C10.Y - 5;
            label_C10.Location = C10_label;
            label_C10.Visible = true;

            label_C11.Text = "ANT3";
            C11_label.X = C11.X - 42;
            C11_label.Y = C11.Y - 5;
            label_C11.Location = C11_label;
            label_C11.Visible = true;

            label_C13.Text = "XVRT TX";
            C13_label.X = C13.X - 60;
            C13_label.Y = C13.Y - 5;
            label_C13.Location = C13_label;
            label_C13.Visible = true;

            label_C24.Text = "SPKR";
            C24_label.X = C24.X - 45;
            C24_label.Y = C24.Y - 6;
            label_C24.Location = C24_label;
            label_C24.Visible = true;

            label_C25.Text = "HDPHONES";
            C25_label.X = C25.X - 75;
            C25_label.Y = C25.Y - 6;
            label_C25.Location = C25_label;
            label_C25.Visible = true;


            label_C26.Text = "MIC";
            C26_label.X = C26.X - 35;
            C26_label.Y = C26.Y - 6;
            label_C26.Location = C26_label;
            label_C26.Visible = true;

            LPF_label.X = LPF.X + 10;
            LPF_label.Y = LPF.Y + 17;
            label_LPF.Location = LPF_label;

            HPF_label.X = HPF.X + 10;
            HPF_label.Y = HPF.Y + 17;
            label_HPF.Location = HPF_label;

        }

        private void XVTR_to_HPF_PA15(Pen pen)
        {
            g.DrawLine(pen, C4, XVTR_HPF_corner);
            //g.DrawLine(pen, HPF_TX_corner1, HPF_TX_corner2);
            g.DrawLine(pen, XVTR_HPF_corner, HPF_B);
        }

        // end segment drawing methods **************************************************************************************


        // begin user-control event handlers ********************************************************************************

        public void pi_Changed()
        {
            do_platform_prep();
            update_diagram = true;
            canvas.Invalidate();
        }

        // **** end of user control event handlers **************************************************************************


        // begin misc methods ***********************************************************************************************

        private void do_platform_prep()
        {
            if (bool_HPSDR) draw_HPSDR();
            if (bool_HERMES) draw_HERMES();
            if (bool_ANAN_10E) draw_ANAN_10E();
            //if (rb_ANAN_10.Checked) draw_ANAN_10();
            if (bool_ANAN_100_PA_rev15) draw_ANAN_100_PA_rev15();
            if (bool_ANAN_100_PA_rev24) draw_ANAN_100_PA_rev24();
            if (bool_ANAN_100D_PA_rev15) draw_ANAN_100D_PA_rev15();
            if (bool_ANAN_100D_PA_rev24) draw_ANAN_100D_PA_rev24();
        }

        private void hide_all_labels()
        {
            label_ADC0.Visible = false;
            label_ADC0_atten.Visible = false;
            label_ADC1.Visible = false;
            label_ADC1_atten.Visible = false;
            label_ADC2.Visible = false;
            label_ADC2_atten.Visible = false;
            label_ALEX.Visible = false;
            label_ALEX_HPF.Visible = false;
            label_ALEX_LPF.Visible = false;
            label_ALEX_To_RX.Visible = false;
            label_ALEX_2.Visible = false;
            label_ALEX_2_HPF.Visible = false;
            label_ALEX_2_LPF.Visible = false;
            label_ALEX_2_To_RX.Visible = false;
            label_AMP.Visible = false;
            label_AUDIO_AMP.Visible = false;
            label_AUDIO_MIXER.Visible = false;
            label_C1.Visible = false;
            label_C2.Visible = false;
            label_C3.Visible = false;
            label_C4.Visible = false;
            label_C5.Visible = false;
            label_C6.Visible = false;
            label_C7.Visible = false;
            label_C8.Visible = false;
            label_C9.Visible = false;
            label_C10.Visible = false;
            label_C11.Visible = false;
            label_SMA.Visible = false;
            label_C12.Visible = false;
            label_C13.Visible = false;
            label_C14.Visible = false;
            label_C15.Visible = false;
            label_C16.Visible = false;
            label_C17.Visible = false;
            label_C18.Visible = false;
            label_C19.Visible = false;
            label_C20.Visible = false;
            label_C20.Visible = false;
            label_C24.Visible = false;
            label_C25.Visible = false;
            label_C26.Visible = false;
            label_CODEC.Visible = false;
            label_CODEC2.Visible = false;
            label_DAC0.Visible = false;
            label_DDC0.Visible = false;
            label_DDC1.Visible = false;
            label_DDC2.Visible = false;
            label_DDC3.Visible = false;
            label_DDC4.Visible = false;
            label_DDC5.Visible = false;
            label_DDC6.Visible = false;
            label_DUAL_MERCURY.Visible = false;
            label_DSP.Visible = false;
            label_DSP_HPSDR.Visible = false;
            label_DUC0.Visible = false;
            label_ext_amp1.Visible = false;
            label_ext_amp2.Visible = false;
            label_FILTER.Visible = false;
            label_FPGA.Visible = false;
            label_front_panel.Visible = false;
            label_HERMES.Visible = false;
            label_HERMES_RX_IN.Visible = false;
            label_HERMES_J5.Visible = false;
            label_HERMES_TX_OUT.Visible = false;
            label_HERMES_J3.Visible = false;
            label_HERMES_XVTR_TX.Visible = false;
            label_HERMES_J1.Visible = false;
            label_HPF.Visible = false;
            label_HPF2.Visible = false;
            label_L_audio_only.Visible = false;
            label_LPF.Visible = false;
            label_LPF2.Visible = false;
            label_LR_audio.Visible = false;
            label_MERCURY.Visible = false;
            label_MERCURY_FPGA.Visible = false;
            label_MERCURY_ADC.Visible = false;
            label_MERCURY_DDC0.Visible = false;
            label_MERCURY_Rx0.Visible = false;
            label_MERCURY_DDC1.Visible = false;
            label_MERCURY_Rx1.Visible = false;
            label_MERCURY_DDC2.Visible = false;
            label_MERCURY_Rx2.Visible = false;
            label_MERCURY_CODEC.Visible = false;
            label_MERCURY_PHONES.Visible = false;
            label_MERCURY_P_OUT.Visible = false;
            label_MERCURY_LINE.Visible = false;
            label_MERCURY_OUT.Visible = false;
            label_MERCURY_ANT.Visible = false;
            label_MERCURY_2.Visible = false;
            label_MERCURY_2_FPGA.Visible = false;
            label_MERCURY_2_ADC.Visible = false;
            label_MERCURY_2_DDC0.Visible = false;
            label_MERCURY_2_Rx0.Visible = false;
            label_MERCURY_2_DDC1.Visible = false;
            label_MERCURY_2_Rx1.Visible = false;
            label_MERCURY_2_DDC2.Visible = false;
            label_MERCURY_2_Rx2.Visible = false;
            label_MERCURY_2_CODEC.Visible = false;
            label_MERCURY_2_PHONES.Visible = false;
            label_MERCURY_2_P_OUT.Visible = false;
            label_MERCURY_2_LINE.Visible = false;
            label_MERCURY_2_OUT.Visible = false;
            label_MERCURY_2_ANT.Visible = false;
            label_METIS.Visible = false;
            label_METIS_FPGA.Visible = false;
            label_PA.Visible = false;
            label_PENELOPE.Visible = false;
            label_PENELOPE_FPGA.Visible = false;
            label_PENELOPE_DAC.Visible = false;
            label_PENELOPE_DUC.Visible = false;
            label_PENELOPE_AMPF.Visible = false;
            label_PENELOPE_FILTER.Visible = false;
            label_PENELOPE_PA.Visible = false;
            label_PENELOPE_CODEC.Visible = false;
            label_PENELOPE_ANT.Visible = false;
            label_PENELOPE_LINE.Visible = false;
            label_PENELOPE_IN.Visible = false;
            label_PENELOPE_MIC.Visible = false;
            label_PENELOPE_XVTR.Visible = false;
            label_PC.Visible = false;
            label_rear_panel.Visible = false;
            label_Rx0.Visible = false;
            label_Rx1.Visible = false;
            label_RX1_DISPLAY.Visible = false;
            label_RX1_LR_audio.Visible = false;
            label_Rx2.Visible = false;
            label_RX2_DISPLAY.Visible = false;
            label_RX2_LR_audio.Visible = false;
            label_Rx3.Visible = false;
            label_Rx4.Visible = false;
            label_Rx5.Visible = false;
            label_Rx6.Visible = false;
            label_SDR_Hardware.Visible = false;
            label_SMA.Visible = false;
            label_SWR.Visible = false;
            label_NOTE.Visible = false;
            label_UNCHECK.Visible = false;
            label_XVTR_VHF.Visible = false;

        }

        private void hide_rear_panel_labels()
        {
            label_C1.Visible = false;
            label_C2.Visible = false;
            label_C3.Visible = false;
            label_C4.Visible = false;
            label_C5.Visible = false;
            label_C6.Visible = false;
            label_C7.Visible = false;
            label_C8.Visible = false;
            label_C9.Visible = false;
            label_C10.Visible = false;
            label_C11.Visible = false;
            label_SMA.Visible = false;
            label_C12.Visible = false;
            label_C13.Visible = false;
            label_C14.Visible = false;
            label_C15.Visible = false;
            label_C16.Visible = false;
            label_C17.Visible = false;
            label_C18.Visible = false;
            label_C19.Visible = false;
            label_C20.Visible = false;
            label_HPF.Visible = false;
            label_HPF2.Visible = false;
            label_LPF2.Visible = false;
            label_ADC1.Visible = false;
            label_ADC1_atten.Visible = false;
            label_ADC2.Visible = false;
            label_ADC2_atten.Visible = false;
            label_Rx2.Visible = false;
            label_Rx3.Visible = false;
            label_Rx4.Visible = false;
            label_Rx5.Visible = false;
            label_Rx6.Visible = false;
            label_DDC2.Visible = false;
            label_DDC3.Visible = false;
            label_DDC4.Visible = false;
            label_DDC5.Visible = false;
            label_DDC6.Visible = false;
        }

        private void hide_controls()
        {
            cb_DUAL_MERCURY_ALEX.Visible = false;
        }

        private void TX_AUDIO_OUT_2_RX_MODELS()
        {
            if (bool_PureSignal == false & bool_duplex == false & bool_XVTR == false)
            {
                g.DrawRectangle(blackPen, RX2_DISPLAY);
                label_RX2_DISPLAY.Visible = true;

                if (bool_MON == true | bool_duplex == true)
                {
                    if (bool_RX1_MUTE == false) SPKR_to_RX1_DISPLAY(bluePen);
                    if (bool_RX2_MUTE == false) SPKR_to_RX2_DISPLAY(bluePen);

                    if (bool_RX1_MUTE == false) label_RX1_LR_audio.Visible = true;
                    else label_RX1_LR_audio.Visible = false;
                    if (bool_RX2_MUTE == false) label_RX2_LR_audio.Visible = true;
                    else label_RX2_LR_audio.Visible = false;
                    if (bool_RX1_MUTE == true & bool_RX2_MUTE == true)
                    {
                        label_AUDIO_AMP.Visible = false;
                        label_CODEC2.Visible = false;
                        label_L_audio_only.Visible = false;
                        label_LR_audio.Visible = false;
                        label_AUDIO_MIXER.Visible = false;
                    }
                    else
                    {
                        g.DrawRectangle(blackPen, AUDIO_MIXER);
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        //label_CODEC.Visible = true;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_L_audio_only.Visible = true;
                        label_LR_audio.Visible = true;
                        g.DrawRectangle(blackPen, AUDIO_AMP);

                    }
                } // if (bool_MON == true)
            }

            if (bool_PureSignal == false & bool_duplex == false & bool_XVTR == true)
            {
                g.DrawRectangle(blackPen, RX2_DISPLAY);
                label_RX2_DISPLAY.Visible = true;

                if (bool_MON == true)
                {
                    if (bool_RX1_MUTE == false) SPKR_to_RX1_DISPLAY(bluePen);
                    if (bool_RX2_MUTE == false) SPKR_to_RX2_DISPLAY(bluePen);

                    if (bool_RX1_MUTE == false) label_RX1_LR_audio.Visible = true;
                    else label_RX1_LR_audio.Visible = false;
                    if (bool_RX2_MUTE == false) label_RX2_LR_audio.Visible = true;
                    else label_RX2_LR_audio.Visible = false;
                    if (bool_RX1_MUTE == true & bool_RX2_MUTE == true)
                    {
                        label_AUDIO_AMP.Visible = false;
                        label_CODEC2.Visible = false;
                        label_L_audio_only.Visible = false;
                        label_LR_audio.Visible = false;
                        label_AUDIO_MIXER.Visible = false;
                    }
                    else
                    {
                        g.DrawRectangle(blackPen, AUDIO_MIXER);
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        //label_CODEC.Visible = false;
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_L_audio_only.Visible = true;
                        label_LR_audio.Visible = true;
                        g.DrawRectangle(blackPen, AUDIO_AMP);

                    }
                }
            }

            if (bool_PureSignal == false & bool_duplex == true & bool_XVTR == true)
            {
                g.DrawRectangle(blackPen, RX2_DISPLAY);
                label_RX2_DISPLAY.Visible = true;
                if (bool_RX1_MUTE == false) SPKR_to_RX1_DISPLAY(bluePen);
                if (bool_RX2_MUTE == false) SPKR_to_RX2_DISPLAY(bluePen);

                if (bool_RX1_MUTE == false) label_RX1_LR_audio.Visible = true;
                else label_RX1_LR_audio.Visible = false;
                if (bool_RX2_MUTE == false) label_RX2_LR_audio.Visible = true;
                else label_RX2_LR_audio.Visible = false;
                if (bool_RX1_MUTE == true & bool_RX2_MUTE == true)
                {
                    label_AUDIO_AMP.Visible = false;
                    label_CODEC2.Visible = false;
                    label_L_audio_only.Visible = false;
                    label_LR_audio.Visible = false;
                    label_AUDIO_MIXER.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    //label_CODEC.Visible = false;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_L_audio_only.Visible = true;
                    label_LR_audio.Visible = true;
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }
            }

            if (bool_PureSignal == false & bool_duplex == true & bool_XVTR == false)
            {
                g.DrawRectangle(blackPen, RX2_DISPLAY);
                label_RX2_DISPLAY.Visible = true;
                if (bool_RX1_MUTE == false) SPKR_to_RX1_DISPLAY(bluePen);
                if (bool_RX2_MUTE == false) SPKR_to_RX2_DISPLAY(bluePen);

                if (bool_RX1_MUTE == false) label_RX1_LR_audio.Visible = true;
                else label_RX1_LR_audio.Visible = false;
                if (bool_RX2_MUTE == false) label_RX2_LR_audio.Visible = true;
                else label_RX2_LR_audio.Visible = false;
                if (bool_RX1_MUTE == true & bool_RX2_MUTE == true)
                {
                    label_AUDIO_AMP.Visible = false;
                    label_CODEC2.Visible = false;
                    label_L_audio_only.Visible = false;
                    label_LR_audio.Visible = false;
                    label_AUDIO_MIXER.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    //label_CODEC.Visible = false;
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_L_audio_only.Visible = true;
                    label_LR_audio.Visible = true;
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }
            }

            if (bool_PureSignal == true & bool_duplex == false & bool_XVTR == false)
            {
                if (bool_RX2_MUTE == false) bool_RX2_MUTE = true;
                if (bool_duplex) bool_duplex = false;
                label_RX2_DISPLAY.Visible = false;
                if (bool_MON == true)
                {
                    if (bool_RX1_MUTE == false) SPKR_to_RX1_DISPLAY(bluePen);
                    label_RX2_LR_audio.Visible = false;


                    if (bool_RX1_MUTE == false) label_RX1_LR_audio.Visible = true;
                    else label_RX1_LR_audio.Visible = false;
                    if (bool_RX1_MUTE == true & bool_RX2_MUTE == true)
                    {
                        label_AUDIO_AMP.Visible = false;
                        label_CODEC2.Visible = false;
                        label_L_audio_only.Visible = false;
                        label_LR_audio.Visible = false;
                        label_AUDIO_MIXER.Visible = false;
                    }
                    else
                    {
                        g.DrawRectangle(blackPen, AUDIO_MIXER);
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_L_audio_only.Visible = true;
                        label_LR_audio.Visible = true;
                        g.DrawRectangle(blackPen, AUDIO_AMP);

                    }
                } // if (bool_MON == true)
            }

            if (bool_PureSignal == true & bool_duplex == true & bool_XVTR == false)
            {
                label_RX2_DISPLAY.Visible = false;
                if (bool_RX2_MUTE == false) bool_RX2_MUTE = true;
                if (bool_duplex) bool_duplex = false;
                if (bool_RX1_MUTE == false) SPKR_to_RX1_DISPLAY_2(bluePen);

                if (bool_RX1_MUTE == false) SPKR_to_RX1_DISPLAY_2(bluePen);

                if (bool_RX1_MUTE == false) label_RX1_LR_audio.Visible = true;
                else label_RX1_LR_audio.Visible = false;
                if (bool_RX1_MUTE == true & bool_RX2_MUTE == true)
                {
                    label_AUDIO_AMP.Visible = false;
                    label_CODEC2.Visible = false;
                    label_L_audio_only.Visible = false;
                    label_LR_audio.Visible = false;
                    label_AUDIO_MIXER.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_L_audio_only.Visible = true;
                    label_LR_audio.Visible = true;
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }
            }

            if (bool_PureSignal == true & bool_duplex == false & bool_XVTR == true)
            {
                label_RX2_DISPLAY.Visible = false;
                if (bool_RX2_MUTE == false) bool_RX2_MUTE = true;
                if (bool_MON == true)
                {
                    if (bool_RX1_MUTE == false) SPKR_to_RX1_DISPLAY(bluePen);
                    label_RX2_LR_audio.Visible = false;

                    if (bool_RX1_MUTE == false) label_RX1_LR_audio.Visible = true;
                    else label_RX1_LR_audio.Visible = false;
                    if (bool_RX1_MUTE == true & bool_RX2_MUTE == true)
                    {
                        label_AUDIO_AMP.Visible = false;
                        label_CODEC2.Visible = false;
                        label_L_audio_only.Visible = false;
                        label_LR_audio.Visible = false;
                        label_AUDIO_MIXER.Visible = false;
                    }
                    else
                    {
                        g.DrawRectangle(blackPen, AUDIO_MIXER);
                        label_AUDIO_MIXER.Visible = true;
                        g.DrawRectangle(blackPen, CODEC2);
                        label_AUDIO_AMP.Visible = true;
                        label_CODEC2.Visible = true;
                        label_L_audio_only.Visible = true;
                        label_LR_audio.Visible = true;
                        g.DrawRectangle(blackPen, AUDIO_AMP);

                    }
                }
            }

            if (bool_PureSignal == true & bool_duplex == true & bool_XVTR == true)
            {
                label_RX2_DISPLAY.Visible = false;
                if (bool_RX2_MUTE == false) bool_RX2_MUTE = true;
                if (bool_duplex) bool_duplex = false;
                if (bool_RX1_MUTE == false) SPKR_to_RX1_DISPLAY(bluePen);
                if (bool_RX2_MUTE == false) SPKR_to_RX2_DISPLAY(bluePen);

                if (bool_RX1_MUTE == false) label_RX1_LR_audio.Visible = true;
                else label_RX1_LR_audio.Visible = false;
                if (bool_RX2_MUTE == false) label_RX2_LR_audio.Visible = true;
                else label_RX2_LR_audio.Visible = false;
                if (bool_RX1_MUTE == true & bool_RX2_MUTE == true)
                {
                    label_AUDIO_AMP.Visible = false;
                    label_CODEC2.Visible = false;
                    label_L_audio_only.Visible = false;
                    label_LR_audio.Visible = false;
                    label_AUDIO_MIXER.Visible = false;
                }
                else
                {
                    g.DrawRectangle(blackPen, AUDIO_MIXER);
                    label_AUDIO_MIXER.Visible = true;
                    g.DrawRectangle(blackPen, CODEC2);
                    label_AUDIO_AMP.Visible = true;
                    label_CODEC2.Visible = true;
                    label_L_audio_only.Visible = true;
                    label_LR_audio.Visible = true;
                    g.DrawRectangle(blackPen, AUDIO_AMP);

                }

            }
        }

        private void cb_DUAL_MERCURY_ALEX_CheckedChanged(object sender, EventArgs e)
        {
            do_platform_prep();
            update_diagram = true;
            canvas.Invalidate();
        }

        private void rb_rx_CheckedChanged(object sender, EventArgs e)
        {
            do_platform_prep();
            update_diagram = true;
            canvas.Invalidate();
        }

        private void rb_tx_CheckedChanged(object sender, EventArgs e)
        {
            do_platform_prep();
            update_diagram = true;
            canvas.Invalidate();
        }

        private void PI_Resize(object sender, EventArgs e)
        {
            do_platform_prep();
            update_diagram = true;
            canvas.Invalidate();
        }

        private void PI_Disposed(object sender, EventArgs e)
        {
            console.path_Illustrator = null;
        }

        // end misc methods *************************************************************************************************

    }
}
