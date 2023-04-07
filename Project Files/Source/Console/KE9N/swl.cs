//=================================================================
// swl.cs
// created by Darrin Kohn ke9ns
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
//
//=================================================================

//=================================================================

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace Thetis
{
    public class SwlControl : System.Windows.Forms.Form
    {

        public SpotControl SpotForm;                     // ke9ns add  communications with spot.cs 
        public ScanControl ScanForm;                            // ke9ns add freq Scanner function

        public static Console console;   // ke9ns mod  to allow console to pass back values to setup screen
        public Setup setupForm;   // ke9ns communications with setupform  (i.e. allow combometertype.text update from inside console.cs) 

        //   private ArrayList file_list;
        private string wave_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Thetis";

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.GroupBoxTS grpPlayback;
        private System.Windows.Forms.GroupBox grpPlaylist;
        private TextBox textBox3;
        private CheckBoxTS chkAlwaysOnTop;
        private Button button1;
        private RichTextBox richTextBox1;
        private Button button2;
        private RichTextBox richTextBox2;
        private IContainer components;

        //   public DXMemList dxmemlist;



        #region Constructor and Destructor

        public SwlControl(Console c)
        {
            InitializeComponent();
            console = c;

            Common.RestoreForm(this, "SwlForm", true);



            //   bandSwlupdate();


        } // swlcontrol


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
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SwlControl));
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.chkAlwaysOnTop = new System.Windows.Forms.CheckBoxTS();
            this.SuspendLayout();
            // 
            // textBox3
            // 
            this.textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.Location = new System.Drawing.Point(12, 12);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(394, 97);
            this.textBox3.TabIndex = 9;
            this.textBox3.TabStop = false;
            this.textBox3.Text = resources.GetString("textBox3.Text");
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.button1.Location = new System.Drawing.Point(12, 509);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 61;
            this.button1.Text = "Update List";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(97, 123);
            this.richTextBox1.MaxLength = 20;
            this.richTextBox1.Multiline = false;
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox1.Size = new System.Drawing.Size(175, 25);
            this.richTextBox1.TabIndex = 62;
            this.richTextBox1.Text = "";
            this.richTextBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richTextBox1_KeyPress);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.button2.Location = new System.Drawing.Point(287, 125);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 63;
            this.button2.Text = "Search";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // richTextBox2
            // 
            this.richTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.richTextBox2.Cursor = System.Windows.Forms.Cursors.Default;
            this.richTextBox2.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox2.Location = new System.Drawing.Point(12, 163);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox2.Size = new System.Drawing.Size(394, 331);
            this.richTextBox2.TabIndex = 64;
            this.richTextBox2.Text = "";
            this.richTextBox2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.richTextBox2_MouseDown);
            this.richTextBox2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.richTextBox2_MouseUp);
            // 
            // chkAlwaysOnTop
            // 
            this.chkAlwaysOnTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkAlwaysOnTop.Image = null;
            this.chkAlwaysOnTop.Location = new System.Drawing.Point(302, 509);
            this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            this.chkAlwaysOnTop.Size = new System.Drawing.Size(104, 24);
            this.chkAlwaysOnTop.TabIndex = 59;
            this.chkAlwaysOnTop.Text = "Always On Top";
            this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(this.chkAlwaysOnTop_CheckedChanged);
            // 
            // SwlControl
            // 
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(418, 542);
            this.Controls.Add(this.richTextBox2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.chkAlwaysOnTop);
            this.Controls.Add(this.textBox3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(434, 200);
            this.Name = "SwlControl";
            this.Text = "SWL";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SwlControl_FormClosing);
            this.Load += new System.EventHandler(this.SwlControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        #endregion

        #region Properties



        #endregion

        #region Event Handlers







        #endregion

        private void SwlControl_Load(object sender, EventArgs e)
        {
            bandSwlupdate();


        }


        //=======================================================================================================================
        private void SwlControl_FormClosing(object sender, FormClosingEventArgs e)
        {

            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, "SwlForm");


        }

        //===================================================================================
        //===================================================================================
        //===================================================================================

        int[] swl_index = new int[20000];

        public void bandSwlupdate()
        {

            int iii = 0;

            string bigmessage = null; // full textbox string (combine 1 and 2)

            Debug.WriteLine("swl index size= " + SpotControl.SWL_Index1);

            richTextBox1.Text = richTextBox1.Text.TrimEnd('\r', '\n');

            DateTime UTCD = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            SpotControl.UTCNEW1 = Convert.ToInt16(UTCD.ToString("HHmm")); // convert 24hr UTC to int

            // Debug.WriteLine("Day: " + SpotControl.UTCDD);
            //  Debug.WriteLine("Day1: " + (byte)UTCD.DayOfWeek);


            for (int ii = 0; ii < SpotControl.SWL_Index1; ii++) // check all spots to see which ones are on at this particular time and day
            {

                // station check 
                if ((SpotControl.SWL_Station[ii].IndexOf(richTextBox1.Text, StringComparison.OrdinalIgnoreCase) >= 0) || (richTextBox1.Text == "") || (richTextBox1.Text == " "))
                {

                    // station check days on air and time on air
                    if (
                        ((SpotControl.SWL_Day1[ii] & SpotControl.UTCDD) > 0) && (((SpotControl.SWL_TimeN[ii] <= SpotControl.UTCNEW1) && (SpotControl.SWL_TimeF[ii] >= SpotControl.UTCNEW1)) ||
                        ((SpotControl.SWL_TimeN[ii] >= SpotControl.UTCNEW1) && (SpotControl.SWL_TimeF[ii] >= SpotControl.UTCNEW1) && (SpotControl.SWL_TimeF[ii] < SpotControl.SWL_TimeN[ii])))
                         )
                    {

                        //    Debug.WriteLine("station found" + SpotControl.SWL_Freq[ii] + " , "+ SpotControl.SWL_Day1[ii]);


                        swl_index[iii++] = ii; // keep track of frequencies on at the moment

                        if (SpotControl.SWL_Station[ii].Length > 25) SpotControl.SWL_Station[ii] = SpotControl.SWL_Station[ii].Substring(0, 25);


                        if ((SpotControl.SWL_Station[ii].IndexOf("volmet", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("DSC-", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("hfdl", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("ALE-", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("ALE4", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("WLO", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("Beacon-", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("Maritime", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("fax", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("marker", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("weather", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("number", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("military", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("stanag", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("uscg", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("gander", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("Meteo", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("propag", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("wx", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("cw", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "CWU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("rtty", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("SSTV", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("olivia", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("navy", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("army", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("force", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "USB";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("digital", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Station[ii].IndexOf("drm", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            SpotControl.SWL_Mode[ii] = "DIGU";
                        }
                        else if ((SpotControl.SWL_Freq[ii] / 1000000) == 8) SpotControl.SWL_Mode[ii] = "USB";

                        else SpotControl.SWL_Mode[ii] = "SAM";


                        bigmessage += (String.Format("{0:00.000000}", (double)(SpotControl.SWL_Freq[ii]) / 1000000.0) +
                            "  " + SpotControl.SWL_Station[ii].PadRight(25, ' ') + " " + SpotControl.SWL_Loc[ii].PadRight(3, ' ') +
                            " " + SpotControl.SWL_TimeN[ii].ToString().PadLeft(4, '0') + ":" + SpotControl.SWL_TimeF[ii].ToString().PadLeft(4, '0') +
                            " " + "\r\n");


                    } // check time
                } // text search to narrow down

            } // for loop through SWL_Index


            Debug.WriteLine("SWL DONE");



            richTextBox2.Text = bigmessage; // update screen




        } // bandSwlupdate


        //======================================================================== 
        //====================================================================================

        private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkAlwaysOnTop.Checked;
        }




        public static int RIndex1 = 0;

        private void button1_Click(object sender, EventArgs e)
        {

            bandSwlupdate();
            richTextBox2.Focus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bandSwlupdate();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

            bandSwlupdate();
        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                bandSwlupdate();
            }
        }

        private void richTextBox2_MouseDown(object sender, MouseEventArgs e)
        {
            richTextBox2.ShortcutsEnabled = false; // added to eliminate the contextmenu from popping up

        }


        int xxx = 0;


        private void richTextBox2_MouseUp(object sender, MouseEventArgs e)
        {
            richTextBox2.ShortcutsEnabled = false;


            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    int ii = richTextBox2.GetCharIndexFromPosition(e.Location);

                    xxx = (ii / 52); //find row 

                    Debug.WriteLine("ii " + ii);
                    Debug.WriteLine("xxx " + xxx);

                    richTextBox2.SelectionStart = (xxx * 52);
                    richTextBox2.SelectionLength = 52;

                    //   console.SaveBand(); // put away last freq you were on before moving
                    Debug.WriteLine("INDEX YOU CLICKED ON " + ii);
                    Debug.WriteLine("INDEX YOU CLICKED ON2 " + xxx);

                    Debug.WriteLine("freq " + SpotControl.SWL_Freq[swl_index[xxx]]);

                    console.VFOAFreq = ((double)SpotControl.SWL_Freq[swl_index[xxx]]) / 1000000.0; // convert to MHZ

                    //   Display.VFOA = ((long)SpotControl.SWL_Freq[swl_index[xxx]]) ; // keep as hz

                    if (SpotControl.SWL_Mode[swl_index[xxx]] == "AM") console.RX1DSPMode = DSPMode.SAM;
                    else if (SpotControl.SWL_Mode[swl_index[xxx]] == "SAM") console.RX1DSPMode = DSPMode.SAM;
                    else if (SpotControl.SWL_Mode[swl_index[xxx]] == "USB") console.RX1DSPMode = DSPMode.USB;
                    else if (SpotControl.SWL_Mode[swl_index[xxx]] == "DIGU") console.RX1DSPMode = DSPMode.DIGU;


                    richTextBox2.Focus();
                    richTextBox2.Show();


                }
                catch
                {
                    Debug.WriteLine("Bad location");

                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // nothing yet
            }



            //  button1.Focus();
        }
    } // Swlcontrol


} // powersdr
