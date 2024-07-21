//=================================================================
// MemoryForm.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2003-2013  FlexRadio Systems
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 
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
// You may contact us via email at: gpl@flexradio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
//=================================================================

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

//reference Nuget Package NAudio.Lame
//using NAudio;
//using NAudio.Wave;
//using NAudio.Lame;


namespace Thetis
{
    public partial class MemoryForm : Form
    {
        #region Variable Declaration

        private Console console;
        public WaveControl WaveForm;                       // ke9ns    communication with the waveform (i.e. allows audio to be played from console.cs)

      
        #endregion

        #region Constructor

        /*
      <Group>Am Broadcast</Group>
      <RXFreq>0.78</RXFreq>
      <Name>WBBM 780</Name>
      <DSPMode>SAM</DSPMode>
      <Scan>true</Scan>
      <TuneStep>500Hz</TuneStep>
      <RPTR>Low</RPTR>
      <RPTROffset>0</RPTROffset>
      <CTCSSOn>true</CTCSSOn>
      <CTCSSFreq>114.8</CTCSSFreq>
      <Deviation>2500</Deviation>
      <Power>64</Power>
      <Split>false</Split>
      <TXFreq>0.78</TXFreq>
      <RXFilter>VAR1</RXFilter>
      <RXFilterLow>-5000</RXFilterLow>
      <RXFilterHigh>3407</RXFilterHigh>
      <Comments>http://chicago.cbslocal.com/station/wbbm-newsradio-780-and-1059fm/</Comments>
      <AGCMode>MED</AGCMode>
      <AGCT>76</AGCT>

      */



        public MemoryForm(Console c)
        {
            InitializeComponent();
            console = c;
            Common.RestoreForm(this, "MemoryForm", true); // ke9ns bring up memory window in place you left it last time

            dataGridView1.RowHeadersVisible = true;

            dataGridView1.DataSource = console.MemoryList.List; // ke9ns get list of memories from memorylist.cs is where the file is opened and saved

            dataGridView1.RowHeadersWidthSizeMode =  DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

            dataGridView1.ColumnHeadersHeightSizeMode =  DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            dataGridView1.AutoSizeColumnsMode =   DataGridViewAutoSizeColumnsMode.AllCells;

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AutoGenerateColumns = false;

            // Create ComboBox Column
            // DSPMode
            DataGridViewComboBoxColumn comboboxColumnDSPMode = new DataGridViewComboBoxColumn();            
            comboboxColumnDSPMode.DataPropertyName = "DSPMode";
            comboboxColumnDSPMode.Name = "DSPMode";
            comboboxColumnDSPMode.HeaderText = "DSP Mode";
            comboboxColumnDSPMode.ValueType = typeof(DSPMode);

            // Tune Step
            DataGridViewComboBoxColumn comboboxColumnTuneStep = new DataGridViewComboBoxColumn();
            comboboxColumnTuneStep.DataPropertyName = "TuneStep";
            comboboxColumnTuneStep.Name = "TuneStep";
            comboboxColumnTuneStep.HeaderText = "Tune Step";
            comboboxColumnTuneStep.ValueType = typeof(string);
            
            // RPT repeater mode
            DataGridViewComboBoxColumn comboboxColumnRPTR = new DataGridViewComboBoxColumn();
            comboboxColumnRPTR.DataPropertyName = "RPTR";
            comboboxColumnRPTR.Name = "RPTR";
            comboboxColumnRPTR.HeaderText = "RPTR";
            comboboxColumnRPTR.ValueType = typeof(FMTXMode);

            // FM CTCSS
            DataGridViewComboBoxColumn comboboxColumnCTCSS = new DataGridViewComboBoxColumn();
            comboboxColumnCTCSS.DataPropertyName = "CTCSSFreq";
            comboboxColumnCTCSS.Name = "CTCSSFreq";
            comboboxColumnCTCSS.HeaderText = "CTCSS Freq";
            comboboxColumnCTCSS.ValueType = typeof(double);            

            // Dev
            DataGridViewComboBoxColumn comboboxColumnDeviation = new DataGridViewComboBoxColumn();
            comboboxColumnDeviation.DataPropertyName = "Deviation";
            comboboxColumnDeviation.Name = "Deviation";
            comboboxColumnDeviation.HeaderText = "Deviation";
            comboboxColumnDeviation.ValueType = typeof(double);

            // Filter
            DataGridViewComboBoxColumn comboboxColumnFilter = new DataGridViewComboBoxColumn();
            comboboxColumnFilter.DataPropertyName = "RXFilter";
            comboboxColumnFilter.Name = "RXFilter";
            comboboxColumnFilter.HeaderText = "RXFilter";
            comboboxColumnFilter.ValueType = typeof(Filter);

            // AGCMode
            DataGridViewComboBoxColumn comboboxColumnAGCMode = new DataGridViewComboBoxColumn();
            comboboxColumnAGCMode.DataPropertyName = "AGCMode";
            comboboxColumnAGCMode.Name = "AGCMode";
            comboboxColumnAGCMode.HeaderText = "AGC Mode";
            comboboxColumnAGCMode.ValueType = typeof(AGCMode);
            

            // populate combobox items -- type is important here!
            comboboxColumnDSPMode.Items.Add(DSPMode.LSB);
            comboboxColumnDSPMode.Items.Add(DSPMode.USB);
            comboboxColumnDSPMode.Items.Add(DSPMode.DSB);
            comboboxColumnDSPMode.Items.Add(DSPMode.CWL);
            comboboxColumnDSPMode.Items.Add(DSPMode.CWU);
            comboboxColumnDSPMode.Items.Add(DSPMode.FM);
            comboboxColumnDSPMode.Items.Add(DSPMode.AM);
            comboboxColumnDSPMode.Items.Add(DSPMode.SAM);
            comboboxColumnDSPMode.Items.Add(DSPMode.SPEC);
            comboboxColumnDSPMode.Items.Add(DSPMode.DIGL);
            comboboxColumnDSPMode.Items.Add(DSPMode.DIGU);
            comboboxColumnDSPMode.Items.Add(DSPMode.DRM);

            for (int i = 0; i < console.TuneStepList.Count; i++)
            {
                comboboxColumnTuneStep.Items.Add(console.TuneStepList[i].Name);
            }

            comboboxColumnRPTR.Items.Add(FMTXMode.High);
            comboboxColumnRPTR.Items.Add(FMTXMode.Simplex);
            comboboxColumnRPTR.Items.Add(FMTXMode.Low);

            for (int i = 0; i < console.CTCSS_array.Length; i++)
                comboboxColumnCTCSS.Items.Add((double)console.CTCSS_array[i]);

            for (int i = 0; i < console.FM_deviation_array.Length; i++)
                comboboxColumnDeviation.Items.Add((int)console.FM_deviation_array[i]);

            for (int i = (int)(Filter.F1); i < (int)Filter.LAST; i++)
                comboboxColumnFilter.Items.Add((Filter)i);

            for (int i = 0; i < (int)AGCMode.LAST; i++)
                comboboxColumnAGCMode.Items.Add((AGCMode)i);  


            // Remove the default DSPMode column (remember index first), and add new combobox column
            int index = dataGridView1.Columns["DSPMode"].Index;
            dataGridView1.Columns.Remove("DSPMode");
            dataGridView1.Columns.Insert(index, comboboxColumnDSPMode);

            index = dataGridView1.Columns["TuneStep"].Index;
            dataGridView1.Columns.Remove("TuneStep");
            dataGridView1.Columns.Insert(index, comboboxColumnTuneStep);

            index = dataGridView1.Columns["RPTR"].Index;
            dataGridView1.Columns.Remove("RPTR");
            dataGridView1.Columns.Insert(index, comboboxColumnRPTR);


            
            index = dataGridView1.Columns["CTCSSFreq"].Index;
            dataGridView1.Columns.Remove("CTCSSFreq");
            dataGridView1.Columns.Insert(index, comboboxColumnCTCSS);

            index = dataGridView1.Columns["Deviation"].Index;
            dataGridView1.Columns.Remove("Deviation");
            dataGridView1.Columns.Insert(index, comboboxColumnDeviation);

            index = dataGridView1.Columns["RXFilter"].Index;
            dataGridView1.Columns.Remove("RXFilter");
            dataGridView1.Columns.Insert(index, comboboxColumnFilter);

            index = dataGridView1.Columns["AGCMode"].Index;
            dataGridView1.Columns.Remove("AGCMode");
            dataGridView1.Columns.Insert(index, comboboxColumnAGCMode);


            // clean up column names for auto-generated fields
            dataGridView1.Columns["RXFreq"].HeaderText = "RX Freq";
            dataGridView1.Columns["RPTROffset"].HeaderText = "RPTR Offset";
            dataGridView1.Columns["CTCSSOn"].HeaderText = "CTCSS";
            dataGridView1.Columns["TXFreq"].HeaderText = "TX Freq";
            dataGridView1.Columns["RXFilterLow"].HeaderText = "RX Filter Low";
            dataGridView1.Columns["RXFilterHigh"].HeaderText = "RX Filter High";
            dataGridView1.Columns["AGCT"].HeaderText = "AGC-T";

            //---------------------------------------------------------------------------------------------------
            dataGridView1.Columns["StartDate"].HeaderText = "Schedule Start"; // ke9ns add
            dataGridView1.Columns["Repeating"].HeaderText = "Weekly"; // ke9ns add
            dataGridView1.Columns["Repeatingm"].HeaderText = "Monthly"; // ke9ns add



            // set the default display for floating point values
            dataGridView1.Columns["RXFreq"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["RXFreq"].DefaultCellStyle.Format = "f6";
            dataGridView1.Columns["TXFreq"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["TXFreq"].DefaultCellStyle.Format = "f6";
            dataGridView1.Columns["RPTROffset"].DefaultCellStyle.Format = "f";

            this.dataGridView1.Columns["Scan"].Visible = false;//

            dataGridView1.CellValidating += new DataGridViewCellValidatingEventHandler(dataGridView1_CellValidating);

            // dataGridView1.CurrentCell = dataGridView1[0, Convert.ToInt16(portBox2.Text)];


            //----------------------------------------------------------------------------------
            ScheduleUpdate(); // ke9ns add update schedule boxes from selected memory

          //  if (!Directory.Exists(wave_folder))
           // {
                // create PowerSDR audio folder if it does not exist
             //   Directory.CreateDirectory(wave_folder);
           // }
            // openFileDialog1.InitialDirectory = console.AppDataPath;
           // openFileDialog1.InitialDirectory = String.Empty;
           // openFileDialog1.InitialDirectory = wave_folder;



            //-------------------------------------------------------------------
            Thread t = new Thread(new ThreadStart(SCHEDULER))
            {
                CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US"),
                CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US"),
                Name = "Scheduler Thread",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            t.Start();


        } //memoryform




        //==============================================================================================
        //ke9ns add AS YOU DRAG YOUR URL ALONG WITH YOUR MOUSE INTO THE MEMORY FORM WINDOW
        public static string URLTEXT; // ke9ns add
        public string droppedUrl;   // ke9ns add
        public static string[] filename; // ke9ns add


        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {

          //  Trace.WriteLine("Dragenter");
           
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) // ke9ns check for file dragdrop
            {
              
                e.Effect = DragDropEffects.Copy;
                e.Effect = DragDropEffects.All;

                filename = (string[])e.Data.GetData(DataFormats.FileDrop);
     
                URLTEXT = filename[0]; // grab file name

                return;
            }


            droppedUrl = ReadURL(e.Data); // ke9ns check for URL e.data is the data received during the drag/drop

            if ((droppedUrl != null) && ( droppedUrl.Trim().Length != 0))
            {
               //  Trace.WriteLine("dragdrop URL>" + droppedUrl);
                URLTEXT = droppedUrl;
                e.Effect = DragDropEffects.Link; // must activate the EFFECT before the _dragdrop will work
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }


        } //   Trace.WriteLine("Dragover");


         //==============================================================================================
        //ke9ns add ONCE DRAGENTER VALIDATES YOUR URL, YOU RELEASE YOUR MOUSE OVER THE WINDOW AND COMMENT FIELD IS UPDATED WITH THE URL
        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
        //    Trace.WriteLine("datagrid drag and drop");
       
                dataGridView1["comments",RIndex].Value = URLTEXT;
  
        } // dataGridView1_DragDrop


        //==============================================================================================
        //ke9ns add USED TO OPEN WEB BROWSER if comment field has URL
        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {

            MouseEventArgs me = (MouseEventArgs)e;
         
            if (me.Button == System.Windows.Forms.MouseButtons.Right)
            {
  
                try
                {
                  
                    System.Diagnostics.Process.Start((string)dataGridView1["comments", RIndex].Value);   // System.Diagnostics.Process.Start("http://www.microsoft.com");
                }
                catch
                {
                    
                }


            } // right mouse button click
            else if (me.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //   Trace.WriteLine("left click ");

            }

            ScheduleUpdate(); // ke9ns add update schedule boxes from selected memory


        } //dataGridView1_CellMouseDown


        public static int RIndex = 0;
        public static int CIndex = 0;

        //============================================================================================================================================
        // ke9ns add COMES HERE AFTER YOU CLICK ON A FIELD BOX TO DETERMINE WHICH ROW YOU ARE WORKING IN
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
        //    Trace.WriteLine("cell click");

       //     Trace.WriteLine("Cell Name " + dataGridView1.Columns[e.ColumnIndex].Name);  // this causes fault if you click in the far left column
       //    Trace.WriteLine("Call Value " + dataGridView1[e.ColumnIndex, e.RowIndex].Value); // 

            RIndex = e.RowIndex; // last row you clicked on 
                                 //   CIndex = e.ColumnIndex; // last column you clicked on 

            ScheduleUpdate(); // ke9ns add update schedule boxes from selected memory

        } // dataGridView1_CellClick


        //============================================================================================================================================
        void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
     
            // handle floating point fields
            if (dataGridView1.Columns[e.ColumnIndex].Name == "RXFreq" ||
                dataGridView1.Columns[e.ColumnIndex].Name == "TXFreq" ||
                dataGridView1.Columns[e.ColumnIndex].Name == "RPTROffset")
            {
                double temp; 
                if(!double.TryParse((string)e.FormattedValue, out temp))  dataGridView1[e.ColumnIndex, e.RowIndex].Value = 0.0;
                return;
            }

            // handle int fields
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Power" ||
                dataGridView1.Columns[e.ColumnIndex].Name == "FilterLow" ||
                dataGridView1.Columns[e.ColumnIndex].Name == "FilterHigh" ||
                dataGridView1.Columns[e.ColumnIndex].Name == "AGCT")
            {
                int temp;
                if (!int.TryParse((string)e.FormattedValue, out temp))   dataGridView1[e.ColumnIndex, e.RowIndex].Value = 0;
                return;
            }

            ScheduleUpdate(); // ke9ns add update schedule boxes from selected memory

        } // dataGridView1_CellValidating

        #endregion

        #region Event Handlers


        //============================================================================================================================================
        // ke9ns add YOU DRAG YOUR URL ALONG WITH YOUR MOUSE OVER THE ADD BUTTON
        private void MemoryRecordAdd_DragEnter(object sender, DragEventArgs e)
        {

            //  Trace.WriteLine("add dragenter");

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) // ke9ns check for file dragdrop
            {

                e.Effect = DragDropEffects.Copy;
                e.Effect = DragDropEffects.All;

                filename = (string[])e.Data.GetData(DataFormats.FileDrop);

                URLTEXT = filename[0]; // grab file name

                return;
            }


            droppedUrl = ReadURL(e.Data);

            if (droppedUrl != null && droppedUrl.Trim().Length != 0)
            {
                  URLTEXT = droppedUrl;
             
                e.Effect = DragDropEffects.Link; // got a URL so activate the drop event when mouse button released
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

            ScheduleUpdate(); // ke9ns add update schedule boxes from selected memory

        } //MemoryRecordAdd_DragEnter

     

        //=========================================================================================================================================
        // Ke9ns add YOUR URL (after being VERIFIED) YOU LET GO THE LEFT MOUSE BUTTON TO DROP ONTO THE ADD BUTTON
        private void MemoryRecordAdd_DragDrop(object sender, DragEventArgs e)
        {
                    
                string mem_name = Convert.ToString(console.VFOAFreq);   //W4TME

                // ke9ns Below URLTEXT goes where normally the comment goes with a ""

            console.MemoryList.List.Add(new MemoryRecord("", console.VFOAFreq, mem_name, console.RX1DSPMode, true, console.TuneStepList[console.TuneStepIndex].Name,
                console.CurrentFMTXMode, console.FMTXOffsetMHz, console.radio.GetDSPTX(0).CTCSSFlag, console.radio.GetDSPTX(0).CTCSSFreqHz, console.PWR,
                (int)console.radio.GetDSPTX(0).TXFMDeviation, console.VFOSplit, console.TXFreq, console.RX1Filter, console.RX1FilterLow,
                console.RX1FilterHigh, URLTEXT, console.radio.GetDSPRX(0, 0).RXAGCMode, console.RF,
                DateTime.Now, ScheduleOn.Checked,(int)ScheduleDurationTime.Value, ScheduleRepeat.Checked, ScheduleRecord.Checked, ScheduleRepeatm.Checked, (int)ScheduleExtra.Value

                ));

            Common.SaveForm(this, "MemoryForm");    // w4tme
            console.MemoryList.Save();              // w4tme 



            ScheduleUpdate(); // ke9ns add update schedule boxes from selected memory


        } // MemoryRecordAdd_DragDrop


     

        //=========================================================================================================================================
        // Ke9ns  this is the ADD button
        /// <summary>
        /// Add a new Memory entry based on the current console settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MemoryRecordAdd_Click(object sender, EventArgs e)
        {
            string mem_name = Convert.ToString(console.VFOAFreq);   //W4TME

            if (Console.ALTM == true) // ke9ns add  add memory from ALT + M keys 
            {
                console.MemoryList.List.Add(new MemoryRecord("New Spot", console.VFOAFreq, mem_name, console.RX1DSPMode, true, console.TuneStepList[console.TuneStepIndex].Name,
                   console.CurrentFMTXMode, console.FMTXOffsetMHz, console.radio.GetDSPTX(0).CTCSSFlag, console.radio.GetDSPTX(0).CTCSSFreqHz, console.PWR,
                   (int)console.radio.GetDSPTX(0).TXFMDeviation, console.VFOSplit, console.TXFreq, console.RX1Filter, console.RX1FilterLow,
                   console.RX1FilterHigh, "", console.radio.GetDSPRX(0, 0).RXAGCMode, console.RF,
                   DateTime.Now, ScheduleOn.Checked, (int)ScheduleDurationTime.Value, ScheduleRepeat.Checked, ScheduleRecord.Checked, ScheduleRepeatm.Checked, (int)ScheduleExtra.Value


                   ));

            }
            else
            {
                console.MemoryList.List.Add(new MemoryRecord("", console.VFOAFreq, mem_name, console.RX1DSPMode, true, console.TuneStepList[console.TuneStepIndex].Name,
                    console.CurrentFMTXMode, console.FMTXOffsetMHz, console.radio.GetDSPTX(0).CTCSSFlag, console.radio.GetDSPTX(0).CTCSSFreqHz, console.PWR,
                    (int)console.radio.GetDSPTX(0).TXFMDeviation, console.VFOSplit, console.TXFreq, console.RX1Filter, console.RX1FilterLow,
                    console.RX1FilterHigh, "", console.radio.GetDSPRX(0, 0).RXAGCMode, console.RF,
                     DateTime.Now, ScheduleOn.Checked, (int)ScheduleDurationTime.Value, ScheduleRepeat.Checked, ScheduleRecord.Checked, ScheduleRepeatm.Checked, (int)ScheduleExtra.Value



                   ));
            }

            ScheduleUpdate(); // ke9ns add update schedule boxes from selected memory

            Console.ALTM = false; // alt+M memory add from display and keyboard instead of being in the Memory screen

            Common.SaveForm(this, "MemoryForm");    // w4tme
            console.MemoryList.Save();              // w4tme 

        } // MemoryRecordAdd_Click


        //=========================================================================================================================================

        /// <summary>
        /// Copy an existing row into a new one.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMemoryRecordCopy_Click(object sender, EventArgs e)
        {
            if (console.MemoryList.List.Count == 0) return;
            console.MemoryList.List.Add(new MemoryRecord(console.MemoryList.List[dataGridView1.CurrentCell.RowIndex]));

            Common.SaveForm(this, "MemoryForm");    // w4tme
            console.MemoryList.Save();              // w4tme 
        }


        //=========================================================================================================================================
        /// <summary>
        /// Delete the current row (after confirmation).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMemoryRecordDelete_Click(object sender, EventArgs e)
        {
            if (console.MemoryList.List.Count == 0) return; // nothing in the list to copy, exit
            if (dataGridView1.SelectedRows.Count == 0) // no row selected -- use current cell
            {
                if (dataGridView1.CurrentCell.RowIndex < 0 ||
                    dataGridView1.CurrentCell.RowIndex > console.MemoryList.List.Count - 1)
                    return;
            }

            DialogResult dr = MessageBox.Show("Are you sure you want to remove the selected row(s)?",
                "Remove Row(s)?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr == DialogResult.No) return;

            if (dataGridView1.SelectedRows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.SelectedRows.Count; ) // no i++ because the selected rows count gets decremented
                    console.MemoryList.List.Remove(console.MemoryList.List[dataGridView1.SelectedRows[i].Index]);
            }
            else // no rows selected, use current cell
            {
                console.MemoryList.List.Remove(console.MemoryList.List[dataGridView1.CurrentCell.RowIndex]);
            }

            Common.SaveForm(this, "MemoryForm");    // w4tme
            console.MemoryList.Save();              // w4tme 
           
        } // delete memory


        //===========================================================================================================================
        // ke9ns  memory  when you select an index line of your memory list

        /// <summary>
        /// Makes the selected row active -- sends it to console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, EventArgs e)
        {

            if (console.MemoryList.List.Count == 0) return; // nothing in the list, exit


            int index = dataGridView1.CurrentCell.RowIndex;


            if (index < 0 || index > console.MemoryList.List.Count - 1) // index out of range
                return;

            console.changeComboFMMemory(index); // ke9ns this will call recallmemory in console 

            Debug.WriteLine("INDEX clicked "+index);
           
          //  Debug.WriteLine("INDEX clicked " + dataGridView1.);

            if (chkMemoryFormClose.Checked) // ke9ns this saves position of memory form window on your screen when you closed it.
            {
                Common.SaveForm(this, "MemoryForm");    // w4tme
                console.MemoryList.Save();              // w4tme 
                this.Close();
            }

            //console.RecallMemory(MemoryList.List[index]);

        } // btnselect_click




        /// <summary>
        /// Don't actually close the form, just hide it and save the position/size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MemoryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            Common.SaveForm(this, "MemoryForm");
            console.MemoryList.Save();
        }

        #endregion


    






        //===================================================================================================
        //===================================================================================================
        //KE9NS ADD below is used to determine the URL from a drag and drop onto the memory form
        //===================================================================================================
        //===================================================================================================

        private const string _asciiUrlDataFormatName = "UniformResourceLocator";
        private static readonly Encoding _asciiUrlEncoding = Encoding.ASCII;

        private const string _unicodeUrlDataFormatName = "UniformResourceLocatorW";
        private static readonly Encoding _unicodeUrlEncoding = Encoding.Unicode;


        //==================================================================================================
        // ke9ns look for URL or file
        private string ReadURL(IDataObject data)  // try reading as unicode URL  (data comes from the e.data of the drag/drop operation and is supposed to contain a URL or FILE
        {
           // try unicode first then ascii
            string unicodetest = Readurl(data, _unicodeUrlDataFormatName, _unicodeUrlEncoding); // _unicodeUrlDataFormatName = "UniformResourceLocatorW"; Encoding _unicodeUrlEncoding = Encoding.Unicode;

            if (unicodetest != null) 
            {
                return unicodetest;   // Unicode URL found from the data 
            }
               
            return Readurl(data, _asciiUrlDataFormatName, _asciiUrlEncoding); // ASCII URL found _asciiUrlDataFormatName = "UniformResourceLocator";   Encoding _asciiUrlEncoding = Encoding.ASCII

        } // UNICODE & ASCII testing
        
        //==================================================================================================
        private string Readurl(IDataObject data, string urlDataFormatName, Encoding urlEncoding)    // try reading as ASCII URL
        {
            // Check whether the data contains a URL

            // dataformat is either   UniformResourceLocator     or      UniformResourceLocatorW 
            // encoding is either     Encoding.ASCII           or       Encoding.Unicode;

            if (!DoesDragDropDataContainUrl1(data, urlDataFormatName))
            {
                return null;
            }

            string url;

            using (Stream urlStream = (Stream)data.GetData(urlDataFormatName)) // use drap/drop data
            {
                using (TextReader reader = new StreamReader(urlStream, urlEncoding))
                {
                    url = reader.ReadToEnd();  // Read the URL from the data
                }
            }
           
            return url.TrimEnd('\0');  // URLs in drag/drop data are often padded with null characters so remove these

        } // ASCII URL

      
        //==================================================================================================
        private static bool DoesDragDropDataContainUrl1(IDataObject data, string urlDataFormatName)
        {
            
            return (data != null) && (data.GetDataPresent(urlDataFormatName)); // return true = yes URL or file is in specified format

        }




        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void MemoryForm_Load(object sender, EventArgs e)
        {

        }


        //===============================================================================
        //===============================================================================
        //===============================================================================
        // ke9ns add Schedule duration of recording if enabled
        private void ScheduleDurationTime_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                LASTUTC = 0;
                dataGridView1["Duration", RIndex].Value = ScheduleDurationTime.Value; // ke9ns add put schedule start duration in selected in field box
                console.MemoryList.Save();
            }
            catch(Exception)
            {

            }


        } // ScheduleDurationTime_ValueChanged


        //===============================================================================
        // ke9ns add Schedule weekly ON/OFF
        private void ScheduleRepeat_CheckedChanged(object sender, EventArgs e)
        {
            if (ScheduleRepeat.Checked == true) ScheduleRepeatm.Checked = false; // only allow either weekly or monthly

            poweroff = 0; // reset flag

            try
            {
                 dataGridView1["Repeating", RIndex].Value = ScheduleRepeat.Checked; // ke9ns add put schedule start duration in selected in field box

                if ((ScheduleRepeat.Checked == false) && (ScheduleRepeatm.Checked == false))
                {
                    if (DurationCount > 1)
                    {
                        LASTUTC = UTCNEW;
                        DurationCount = 1; // to turn things off immediatly if you toggle on/off into off

                    }
                }
                LASTUTC = 0; // force a schedule refresh
                console.MemoryList.Save();
            }
            catch (Exception)
            {

            }
        } // ScheduleRepeat_CheckedChanged

        //===============================================================================
        // ke9ns add Schedule monthly ON/OFF
        private void ScheduleRepeatm_CheckedChanged(object sender, EventArgs e)
        {
            if (ScheduleRepeatm.Checked == true) ScheduleRepeat.Checked = false;    // only allow either weekly or monthly

            poweroff = 0; // reset flag

            try
            {
               
                dataGridView1["Repeatingm", RIndex].Value = ScheduleRepeatm.Checked; // ke9ns add put schedule start duration in selected in field box

                if ((ScheduleRepeat.Checked == false) && (ScheduleRepeatm.Checked == false))
                {
                    if (DurationCount > 1)
                    {
                        LASTUTC = UTCNEW;
                        DurationCount = 1; // to turn things off immediatly if you toggle on/off into off

                    }
                }
                LASTUTC = 0; // force a schedule refresh
                console.MemoryList.Save();
            }
            catch (Exception)
            {

            }

        } // ScheduleRepeatm_CheckedChanged


        //===============================================================================
        // ke9ns add Schedule Record ON/OFF
        private void ScheduleRecord_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                LASTUTC = 0;
                dataGridView1["Recording", RIndex].Value = ScheduleRecord.Checked; // ke9ns add put schedule start duration in selected in field box
                console.MemoryList.Save();
            }
            catch (Exception)
            {

            }
        }

        //===============================================================================
        // ke9ns add Schedule ON/OFF (NOT USED AT THIS TIME)
        private void ScheduleOn_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
               
                if (ScheduleOn.Checked == false)
                {
                    if (DurationCount > 1)
                    {
                        LASTUTC = UTCNEW;
                        DurationCount = 1; // to turn things off immediatly if you toggle on/off into off
                        
                    }
                }

                LASTUTC = 0;
                dataGridView1["ScheduleOn", RIndex].Value = ScheduleOn.Checked; // ke9ns add put schedule ON/OFF in selected in field box
                console.MemoryList.Save();
            }
            catch (Exception)
            {

            }
        }


        //===============================================================================
        // ke9ns add DATE for datetime for schedule
        private void ScheduleStartDate_ValueChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("startdate value changed");

            ScheduleStartDate.Value = ScheduleStartDate.Value.Date + ScheduleStartTime.Value.TimeOfDay;

            try
            {
                LASTUTC = 0;
                dataGridView1["StartDate", RIndex].Value = ScheduleStartDate.Value;           // ke9ns schedule date and time (local format) 
                console.MemoryList.Save();

            }
            catch (Exception)
            {

            }

        } // ScheduleStartDate_ValueChanged



        /*
     <Group>Am Broadcast</Group>
     <RXFreq>0.78</RXFreq>
     <Name>WBBM 780</Name>
     <DSPMode>SAM</DSPMode>
     <Scan>true</Scan>
     <TuneStep>500Hz</TuneStep>
     <RPTR>Low</RPTR>
     <RPTROffset>0</RPTROffset>
     <CTCSSOn>true</CTCSSOn>
     <CTCSSFreq>114.8</CTCSSFreq>
     <Deviation>2500</Deviation>
     <Power>64</Power>
     <Split>false</Split>
     <TXFreq>0.78</TXFreq>
     <RXFilter>VAR1</RXFilter>
     <RXFilterLow>-5000</RXFilterLow>
     <RXFilterHigh>3407</RXFilterHigh>
     <Comments>http://chicago.cbslocal.com/station/wbbm-newsradio-780-and-1059fm/</Comments>
     <AGCMode>MED</AGCMode>
     <AGCT>76</AGCT>
     
     <StartDate>2016-09-11T21:40:37.8856177-05:00</StartDate>
      <ScheduleOn>false</ScheduleOn>
      <Duration>30</Duration>
      <Recording>false</Recording>
      <Repeating>false</Repeating>
      <Repeatingm>false</Repeatingm>
      <Extra>0</Extra>


          
  int temp;
                if (!int.TryParse((string)e.FormattedValue, out temp))   dataGridView1[e.ColumnIndex, e.RowIndex].Value = 0;

     */

        //=========================================================================================
        // ke9ns add  update the boxes at the bottom of the memory screen
        public void ScheduleUpdate()
        {

            if (dataGridView1.Rows.Count < 1) return; // dont update if you have no memories to update
            if (RIndex < 0) return; // dont update if your clicking on the headers and not a memory

            MemComments.Text = (string)dataGridView1["comments", RIndex].Value; // ke9ns add put comments selected in field box

           
            MemGroup.Text = (string)dataGridView1["Group", RIndex].Value;
            MemName.Text = (string)dataGridView1["Name", RIndex].Value;


            double s = (double)dataGridView1["RXFreq", RIndex].Value;

            MemFreq.Text = s.ToString("F6");

            try // ke9ns upgrading an old database may fail before its upgraded
            {
                ScheduleStartDate.ValueChanged -= new System.EventHandler(ScheduleStartDate_ValueChanged);  // ke9ns turn off checkchanged temporarily    // ke9ns turn off valuechanged temporarily 
                ScheduleStartTime.ValueChanged -= new System.EventHandler(ScheduleStartDate_ValueChanged); //MW0LGE_21k9

                ScheduleStartDate.Value = (DateTime)dataGridView1["StartDate", RIndex].Value; // ke9ns add put schedule start date in selected in field box
                ScheduleStartTime.Value = (DateTime)dataGridView1["StartDate", RIndex].Value; // ke9ns add put schedule start date in selected in field box

                ScheduleStartDate.ValueChanged += new System.EventHandler(ScheduleStartDate_ValueChanged);  // ke9ns turn off checkchanged temporarily    // ke9ns turn off valuechanged temporarily 
                ScheduleStartTime.ValueChanged += new System.EventHandler(ScheduleStartDate_ValueChanged); //MW0LGE_21k9

                ScheduleOn.Checked = (bool)dataGridView1["ScheduleOn", RIndex].Value; // ke9ns add put schedule ON/OFF in selected in field box

                if ((int)dataGridView1["Duration", RIndex].Value > 120) dataGridView1["Duration", RIndex].Value = 120;
                else if ((int)dataGridView1["Duration", RIndex].Value < 0) dataGridView1["Duration", RIndex].Value = 0;

                ScheduleDurationTime.Value = (int)dataGridView1["Duration", RIndex].Value; // ke9ns add put schedule start duration in selected in field box

                ScheduleRepeat.Checked = (bool)dataGridView1["Repeating", RIndex].Value; // ke9ns add put schedule repeat in selected in field box
                ScheduleRecord.Checked = (bool)dataGridView1["Recording", RIndex].Value; // ke9ns add put schedule recording in selected in field box
                ScheduleRepeatm.Checked = (bool)dataGridView1["Repeatingm", RIndex].Value; // ke9ns add put schedule repeat in selected in field box

            }
            catch (Exception)
            {
                ScheduleStartDate.Value = DateTime.Now;
                ScheduleStartTime.Value = DateTime.Now;

                ScheduleOn.Checked = false;
                ScheduleDurationTime.Value = 0;
                ScheduleRepeat.Checked = false;
                ScheduleRecord.Checked = false;
                ScheduleRepeatm.Checked = false;
            }

        } //ScheduleUpdate()

        private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = chkAlwaysOnTop.Checked;
        }


        public static DateTime UTCD = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

        public static string FD = UTCD.ToString("HHmm");                                       // get 24hr 4 digit UTC NOW

        public static int UTCNEW = Convert.ToInt16(FD);                                       // convert 24hr UTC to int

        public static int LASTUTC = 0;   // ke9ns update 1 time per minute
        public static int DurationCount = 0; // ke9ns duration audio recording counter

        private int daycheck = 0; // ke9ns temp day of week repeat
        private int ScheduleOnce = 0; // ke9ns 1=already scheduled
        private int poweroff = 0; // ke9ns 1=power was off at start of recording so turn it back off when done.

        //====================================================================================================
        //====================================================================================================
        //====================================================================================================
        //====================================================================================================
        //====================================================================================================
        // ke9ns add Thread routine (checks the scheduler)
        private void SCHEDULER()  // ke9ns Thread opeation (runs in en-us culture) runs the scheduler 
        {
            Debug.WriteLine("SCHEDULER==========================================");

            for (;;)
            {
                UTCD = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                FD = UTCD.ToString("HHmm");
                UTCNEW = Convert.ToInt16(FD);

                try
                {

                    if ((UTCNEW != LASTUTC) && (dataGridView1.Rows.Count > 0)) // check 1 time per minute
                    {
                       
                        LASTUTC = UTCNEW;

                       
                        if (DurationCount > 1) // check audio recording start/stop
                        {

                            DurationCount--;
                           
                            ScheduleRemain.Text = DurationCount.ToString();

                            Debug.WriteLine("Audio countdown" + DurationCount);

                            continue; // skip checking schedule while recording

                            // record audio here
                        }
                        else if (DurationCount == 1) // only turn off recording if 1 time
                        {
                            console.RECPOST = false; // turn off audio recording

                            ScheduleRecord.ForeColor = Color.Black;
                            ScheduleRemain.ForeColor = Color.Black;
                            DurationCount = 0;
                            ScheduleRemain.Text = DurationCount.ToString();  // ke9ns add put schedule start duration in selected in field box

                            console.REC1 = false; // turn off RED sign over Wave item
                            console.SCHED1 = false; // turn off RED sign over Memory item

                            console.RECPOST1 = true; // restore SR back to original

                            Thread t1 = new Thread(new ThreadStart(TOMP3))
                            {
                                CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US"),
                                CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US"),
                                Name = "mp3 Thread",
                                IsBackground = true,
                                Priority = ThreadPriority.Normal
                            };
                            t1.Start();

                            if (poweroff == 1) // power was off when recording started, so turn power off when done
                            {
                                console.PowerOn = false; // turn off Radio
                                poweroff = 0; // reset flag
                            }

                          
                         //   Audio.RecordRXPreProcessed = temp_record; //return to original state
                         //   WaveOptions.comboSampleRate.Text = quickmp3SR; // restore file size


                        } // duration==1

                        if (ScheduleOnce == UTCNEW) continue; // if you turn off a scheduled event, then wait 1 minute before checking anything again

                        Debug.WriteLine("Total Memories " + dataGridView1.Rows.Count);

                        for (int aa = 0; aa < dataGridView1.Rows.Count; aa++) // get current # of memories we have available; ii++)   
                        {
                            daycheck = 0;

                            if (((bool)dataGridView1["Repeating", aa].Value == true) || ((bool)dataGridView1["Repeatingm", aa].Value == true)) // check only memories that the schedule is enabled
                            {

                              //  Debug.WriteLine("Date and Time " + dataGridView1["StartDate", aa].Value);

                                DateTime temp1 = (DateTime)dataGridView1["StartDate", aa].Value; // save date and time for checking


                                //-----------------------------------------------------------------------------
                                // ke9ns check if Memory repeats every week 
                                if ((bool)dataGridView1["Repeating", aa].Value == true) // check every week
                                {
                                    Debug.WriteLine("Weekly Enabled current day: " + DateTime.Now.DayOfWeek + " Day recorded: "+temp1.DayOfWeek);

                                    if (DateTime.Now.DayOfWeek == temp1.DayOfWeek)
                                    {
                                        daycheck = 1; // matches the day of week
                                    }
                                }

                                //-------------------------------------------------------------------------------------------
                                // ke9ns check if Memory repeats every month on the same day (like 3rd thursday) 
                                if ((bool)dataGridView1["Repeatingm", aa].Value == true) // check every week
                                {
                                    Debug.WriteLine("Monthly Enabled current day: " + DateTime.Now.DayOfWeek + "Day recorded: " + temp1.DayOfWeek);

                                    if (DateTime.Now.DayOfWeek == temp1.DayOfWeek) // check first to see you are on the correct day (like thrusday)
                                    {
                                        DateTime temp2 = temp1; // temp holder for current temp1 we are trying to check out


                                        //---------------------------------------------------------------------------------------------------------
                                        //---------------------------------------------------------------------------------------------------------
                                        //-------------------------------------------------------------------------------------------------------------
                                        // ke9ns check how many weeks (with the scheduled day) in the ORIGINAL sheduled month
                                        int totalmonthweeks = 0;
                                        int originalweek = 0;

                                        for (int x=1;x < 32;x++) // find what week your day of the week was in to check for repeats
                                        {
                                            try
                                            {
                                                temp2 = new DateTime(temp1.Year, temp1.Month, x);
                                                if (temp2.DayOfWeek == temp1.DayOfWeek) // check for the day of week match (Like Monday)
                                                {
                                                    totalmonthweeks++;
                                                    Debug.WriteLine("ORINGAL Day of week match: " + x);

                                                    if (temp2.Day == temp1.Day) // check day of the month (1 to 31) that the Monday occured
                                                    {
                                                        // Y now represents the week of the month that the original schedule was set for (1st week, 2nd week, 3rd week, 4th of month
                                                        // 4th week must be considered just the Last week of month since some months wont have a 4th week for that particular day.
                                                        Debug.WriteLine("Found week of month of original schedule date: " + totalmonthweeks);
                                                        originalweek = totalmonthweeks; // week of month

                                                    } //if (temp2.Day == temp1.Day)

                                                } //if (temp2.DayOfWeek == temp1.DayOfWeek) 
                                          
                                            }
                                            catch(Exception) // exceeded days of month
                                            {
                                                Debug.WriteLine("End of Month before 31 days.");
                                                break;
                                            }
       
                                        } // for x loop through entire month

                                        Debug.WriteLine("End of Month found # of weeks of the scheduled day: " + totalmonthweeks);

                                        if (originalweek == totalmonthweeks)
                                        {
                                            Debug.WriteLine("Original Month day of week was Last week of that month");
                                            originalweek = 10; // signifies its the last week of the month
                                        }



                                        //---------------------------------------------------------------------------------------------------------
                                        //---------------------------------------------------------------------------------------------------------
                                        //-------------------------------------------------------------------------------------------------------------
                                        // ke9ns check how many weeks (with the scheduled day) in the current month
                                        int totalcurrentmonthweeks = 0;
                                        int[] week = new int[6]; // could be up to 5 weeks of a particular day of week (monday)

                                        for (int x = 1; x < 32; x++) // find what week your day of the week was in to check for repeats
                                        {
                                            try
                                            {
                                                temp2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, x); // current month

                                                if (temp2.DayOfWeek == temp1.DayOfWeek) // check for the day of week match (Like Monday)
                                                {
                                                    totalcurrentmonthweeks++;
                                                    week[totalcurrentmonthweeks] = x; // record the day of the month for every week that matches the day of week (like Monday)

                                                    Debug.WriteLine("Day of week match: " + week[totalcurrentmonthweeks]);

                                                } //if (temp2.DayOfWeek == temp1.DayOfWeek) 


                                            }
                                            catch (Exception) // exceeded days of month
                                            {
                                                Debug.WriteLine("End of month before 31 days");
                                                break;
                                            }


                                        } // for x loop through entire month

                                        Debug.WriteLine("End of Current Month found # of weeks of the scheduled day: " + totalcurrentmonthweeks);



                                        //---------------------------------------------------------------------------------------------------------
                                        //---------------------------------------------------------------------------------------------------------
                                        //---------------------------------------------------------------------------------------------------------
                                        // now check the if today is the matching week for the original schedule day of week (monday) (1st,2nd,3rd, or last week)
                                       
                                        if (originalweek == 10) // looking for last week of the current month
                                        {
                                            if (totalcurrentmonthweeks == 3) // only 3 weeks in current month
                                            {
                                                if (week[3] == DateTime.Now.Day) // if last week of this month matches the current Day#
                                                {
                                                    Debug.WriteLine("LAST(3rd) WEEK OF THE MONTH MATCHES GOOD " + DateTime.Now.Day);
                                                   daycheck = 1; // matches the day of week
                                                }
                                            }
                                            else
                                            {
                                                if (week[4] == DateTime.Now.Day) // if last week of this month matches the current Day#
                                                {
                                                    Debug.WriteLine("LAST (4th) WEEK OF THE MONTH MATCHES GOOD " + DateTime.Now.Day);
                                                    daycheck = 1; // matches the day of week
                                                }
                                            }

                                        } // last week of month
                                        else if (originalweek == 1)
                                        {
                                            if (week[1] == DateTime.Now.Day) // if last week of this month matches the current Day#
                                            {
                                                Debug.WriteLine("First WEEK OF THE MONTH MATCHES GOOD " + DateTime.Now.Day);
                                                daycheck = 1; // matches the day of week
                                            }
                                        }
                                        else if (originalweek == 2)
                                        {
                                            if (week[2] == DateTime.Now.Day) // if last week of this month matches the current Day#
                                            {
                                                Debug.WriteLine("Second WEEK OF THE MONTH MATCHES GOOD " + DateTime.Now.Day);
                                                daycheck = 1; // matches the day of week
                                            }
                                            else
                                            {
                                                Debug.WriteLine("Second WEEK OF THE MONTH MATCHES BAD " + DateTime.Now.Day + " week[2] "+week[2]);
                                            }
                                        }
                                        else if (originalweek == 3)
                                        {
                                            if (week[3] == DateTime.Now.Day) // if last week of this month matches the current Day#
                                            {
                                                Debug.WriteLine("3rd WEEK OF THE MONTH MATCHES GOOD " + DateTime.Now.Day);
                                                daycheck = 1; // matches the day of week
                                            }
                                        }
                                        else if (originalweek == 4)
                                        {
                                            if (week[4] == DateTime.Now.Day) // if last week of this month matches the current Day#
                                            {
                                                Debug.WriteLine("4th WEEK OF THE MONTH MATCHES GOOD " + DateTime.Now.Day);
                                                daycheck = 1; // matches the day of week
                                            }
                                        }

                                    } // if (DateTime.Now.DayOfWeek == temp1.DayOfWeek) Monday = Monday


                                } // if ((bool)dataGridView1["Repeatingm", aa].Value == true) 


                                //-----------------------------------------------------------------------------
                                // ke9ns check for Date and Time Matching with schedule of particular memory
                                if ((temp1.Date == DateTime.Now.Date) || (daycheck == 1)) // check for Schedule DATE matchup
                                {
                                    Debug.WriteLine("DATE Match " + temp1.Date);
                               
                                    if ((temp1.TimeOfDay.Hours == DateTime.Now.TimeOfDay.Hours) && (temp1.TimeOfDay.Minutes == DateTime.Now.TimeOfDay.Minutes))
                                    {

                                        Debug.WriteLine("TIME match=" + aa);


                                        if (console.PowerOn == false) // was power OFF?
                                        {
                                            console.PowerOn = true; // turn on Radio
                                            poweroff = 1; // power was off at time of recording
                                        }
                                        else poweroff = 0; // power was ON at time of recording

                                        if (!console.MOX) // only change freq if not transmitting
                                        {
                                            ScheduleOnce = UTCNEW; // record the time you selected a scheduled event

                                            int index = aa;        //dataGridView1.CurrentCell.RowIndex;

                                            if (index < 0 || index > console.MemoryList.List.Count - 1) // index out of range
                                                continue;

                                            console.changeComboFMMemory(index); // ke9ns this will call recallmemory in console 

                                            Debug.WriteLine("INDEX clicked " + index);

                                            //  Debug.WriteLine("INDEX clicked " + dataGridView1.);

                                            if (chkMemoryFormClose.Checked) // ke9ns this saves position of memory form window on your screen when you closed it.
                                            {
                                                Common.SaveForm(this, "MemoryForm");    // w4tme
                                                console.MemoryList.Save();              // w4tme 
                                                this.Close();
                                            }

                                            console.SCHED1 = true;
                                            DurationCount = (int)dataGridView1["Duration", aa].Value;
                                            ScheduleRemain.Text = DurationCount.ToString();

                                            if ((bool)dataGridView1["Recording", aa].Value == true)
                                            {
                                                AutoClosingMessageBox.Show("A Scheduled Recording has been started.\nYou can end the recording early by Checking OFF both Weekly & Monthly boxes. ", "Scheduled Recording Started", 4000);

                                                Debug.WriteLine("Start AUDIO RECORDING1 ");

                                                console.RECPOST = true; // force audio recorder into POST and 48k  (but save original values) start WAV/MP3 recording

                                                ScheduleRecord.ForeColor = Color.Red;
                                                ScheduleRemain.ForeColor = Color.Red;
    
                                                console.REC1 = true; // red sign over Wave menu item

                                            } // Recording ON
                                            else
                                            {
                                                AutoClosingMessageBox.Show("Scheduled Frequency Change has occured\n", "Scheduled Frequency change", 4000);
                                                
                                            }

                                            // check for audio recording

                                        } // !MOX
                                        else // transmitting so pop up message box
                                        {
                                            AutoClosingMessageBox.Show("Scheduled Frequency change and/or Recording could not take place while Transmitting", "Scheduled Memory Event", 4000);


                                            /*
                                            MessageBox.Show("You were transmitting during a Scheduled Frequency change.\n" +
                                               "You will need to manually go to the Frequency to keep your Schedule.",
                                               "Frequency: " + dataGridView1["RXFreq", aa].Value + " Mhz.",
                                               MessageBoxButtons.OK,
                                               MessageBoxIcon.Error);
                                               */

                                        } // MOX

                                        continue; // no need to check further in memories, found one on your schedule
                                    } // TIME matches

                                } // DATE matches

                            } // found schedule ON in a memory

                        } // loop aa through memories

                    } //wait time to increment by 1 minute before checking

                } // try
                catch(Exception)
                {
                    // thread not ready yet since database doesnt contain schedule data yet
                    Debug.WriteLine("Thread not ready3===============");
                    ScheduleRecord.ForeColor = Color.Black;
                    ScheduleRemain.ForeColor = Color.Black;
                    console.RECPOST = false; // force audio recorder into POST

                    console.REC1 = false;
                    console.SCHED1 = false;


                }

                Thread.Sleep(50); // slow down the thread here
            } // for loop (main)

        } // thread SCHEDULER() 

     
        private string wave_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Thetis";

        private void buttonTS1_Click(object sender, EventArgs e)
        {
            //[2.10.3.6]MW0LGE uncommented so that the recording folder is shown. Fixes #457
            string argument = @"/root," + wave_folder;
           // Debug.WriteLine("path===:" + wave_folder);

            System.Diagnostics.Process.Start("explorer.exe", argument);

          //  Debug.WriteLine("WaveControl.scheduleName " + WaveControl.scheduleName);
          //  WaveToMP3(WaveControl.scheduleName, WaveControl.scheduleName1, 128);
          //  Debug.WriteLine("WaveControl.scheduleNameMP3 " + WaveControl.scheduleName1);

        }

        // ke9ns add  NOT USED AT THE MOMENT
        public static void WaveToMP3(string waveFileName, string mp3FileName, int bitRate = 128)
        {
           // using (var reader = new WaveFileReader(waveFileName))
           // using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, LAMEPreset.VBR_90))
            //    reader.CopyTo(writer);
        }

        //=======================================================================================================================
        // ke9ns add
        //Thread
        public void TOMP3()
        {
            try
            {
               // using (var reader = new WaveFileReader(WaveControl.scheduleName)) // closes reader when done using
               // using (var writer = new LameMP3FileWriter(WaveControl.scheduleName1, reader.WaveFormat, LAMEPreset.VBR_90)) // closes writer when done using
                {
                //    reader.CopyTo(writer);
                }
            }
            catch(Exception)
            {

            }
            Debug.WriteLine("DONE WITH MP3 CREATION" + WaveControl.scheduleName1);

            //[2.10.3.5]MW0LGE it looks like MP3 support has been removed and commented out, above, about 5 years ago.
            //This used to remove the wav file after converting over to an MP3. This is now also commented so the recording
            //is made available. Fixes #457

            //try
            //{

            //    System.IO.File.Delete(WaveControl.scheduleName);

            //    Debug.WriteLine("DEL the WAV FILE" + WaveControl.scheduleName);


            //}
            //catch (Exception)
            //{

            //}

        } // MP3 conversion thread. ends when conversion from wav to mp3 is done.


        //  MemoryStream ms = new MemoryStream();
        // ke9ns add
        public static void ConvertWavStreamToMp3File(ref MemoryStream ms, string savetofilename)
        {
            //rewind to beginning of stream
          //  ms.Seek(0, SeekOrigin.Begin);

         //   using (var retMs = new MemoryStream())
           // using (var rdr = new WaveFileReader(ms))
           // using (var wtr = new LameMP3FileWriter(savetofilename, rdr.WaveFormat, LAMEPreset.VBR_90))
          //  {
           //     rdr.CopyTo(wtr);
           // }
        }
    

       
     
        
        



        //====================================================================================================
        public class AutoClosingMessageBox
        {
            System.Threading.Timer _timeoutTimer;
            string _caption;

            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                using (_timeoutTimer)
                    MessageBox.Show(text, caption);
            }
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        } // AutoClosingMessageBox

      
    } // memoryform

    } // powerSDR
