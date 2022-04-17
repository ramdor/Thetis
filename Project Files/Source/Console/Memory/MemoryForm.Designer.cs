//=================================================================
// MEmoryForm.Designer.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2003-2013  FlexRadio Systems
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
// You may contact us via email at: gpl@flexradio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
//=================================================================

namespace Thetis
{
    partial class MemoryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MemoryForm));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.ScheduleStartDate = new System.Windows.Forms.DateTimePicker();
            this.ScheduleStartTime = new System.Windows.Forms.DateTimePicker();
            this.MemComments = new System.Windows.Forms.TextBox();
            this.ScheduleRemain = new System.Windows.Forms.TextBox();
            this.MemFreq = new System.Windows.Forms.TextBox();
            this.MemGroup = new System.Windows.Forms.TextBox();
            this.MemName = new System.Windows.Forms.TextBox();
            this.buttonTS1 = new System.Windows.Forms.ButtonTS();
            this.ScheduleRepeatm = new System.Windows.Forms.CheckBoxTS();
            this.ScheduleOn = new System.Windows.Forms.CheckBoxTS();
            this.ScheduleDurationTime = new System.Windows.Forms.NumericUpDownTS();
            this.ScheduleRecord = new System.Windows.Forms.CheckBoxTS();
            this.ScheduleRepeat = new System.Windows.Forms.CheckBoxTS();
            this.chkMemoryFormClose = new System.Windows.Forms.CheckBoxTS();
            this.btnSelect = new System.Windows.Forms.ButtonTS();
            this.btnMemoryRecordDelete = new System.Windows.Forms.ButtonTS();
            this.btnMemoryRecordCopy = new System.Windows.Forms.ButtonTS();
            this.MemoryRecordAdd = new System.Windows.Forms.ButtonTS();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.ScheduleExtra = new System.Windows.Forms.NumericUpDownTS();
            this.chkAlwaysOnTop = new System.Windows.Forms.CheckBoxTS();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScheduleDurationTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScheduleExtra)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowDrop = true;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(951, 389);
            this.dataGridView1.TabIndex = 1;
            this.toolTip1.SetToolTip(this.dataGridView1, resources.GetString("dataGridView1.ToolTip"));
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dataGridView1.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDown);
            this.dataGridView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.dataGridView1_DragDrop);
            this.dataGridView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.dataGridView1_DragEnter);
            this.dataGridView1.DoubleClick += new System.EventHandler(this.btnSelect_Click);
            // 
            // textBox4
            // 
            this.textBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Location = new System.Drawing.Point(12, 395);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(219, 13);
            this.textBox4.TabIndex = 17;
            this.textBox4.Text = "Schedule Start Date for selected Memory\r\n";
            this.toolTip1.SetToolTip(this.textBox4, "Schedule Start Date to change Frequency and optionally record");
            // 
            // textBox6
            // 
            this.textBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox6.Location = new System.Drawing.Point(237, 395);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(90, 13);
            this.textBox6.TabIndex = 19;
            this.textBox6.Text = "Start Time (local)";
            this.toolTip1.SetToolTip(this.textBox6, "Schedule Start Time to change Frequency and optionally record");
            // 
            // textBox8
            // 
            this.textBox8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox8.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox8.Location = new System.Drawing.Point(347, 395);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(147, 13);
            this.textBox8.TabIndex = 21;
            this.textBox8.Text = "Set <- Duration ->Remaining";
            this.toolTip1.SetToolTip(this.textBox8, "Duration of Scheduled recording (if Enabled)");
            // 
            // ScheduleStartDate
            // 
            this.ScheduleStartDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScheduleStartDate.Location = new System.Drawing.Point(12, 411);
            this.ScheduleStartDate.Name = "ScheduleStartDate";
            this.ScheduleStartDate.Size = new System.Drawing.Size(219, 20);
            this.ScheduleStartDate.TabIndex = 60;
            this.toolTip1.SetToolTip(this.ScheduleStartDate, "Initial Date of Schedule for this Selected Memory\r\n\r\nCheck boxes below determine " +
        "if Schedule Event is turned ON\r\nand if its Weekly or Monthly");
            this.ScheduleStartDate.ValueChanged += new System.EventHandler(this.ScheduleStartDate_ValueChanged);
            // 
            // ScheduleStartTime
            // 
            this.ScheduleStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScheduleStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.ScheduleStartTime.Location = new System.Drawing.Point(237, 412);
            this.ScheduleStartTime.Name = "ScheduleStartTime";
            this.ScheduleStartTime.ShowUpDown = true;
            this.ScheduleStartTime.Size = new System.Drawing.Size(90, 20);
            this.ScheduleStartTime.TabIndex = 61;
            this.toolTip1.SetToolTip(this.ScheduleStartTime, "Initial Time of Schedule for this Selected Memory.\r\nIgnores the seconds.");
            this.ScheduleStartTime.ValueChanged += new System.EventHandler(this.ScheduleStartDate_ValueChanged);
            // 
            // MemComments
            // 
            this.MemComments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MemComments.Location = new System.Drawing.Point(516, 454);
            this.MemComments.Name = "MemComments";
            this.MemComments.Size = new System.Drawing.Size(435, 20);
            this.MemComments.TabIndex = 14;
            this.toolTip1.SetToolTip(this.MemComments, "Comments of currently selected Memory. Including any Hyperlinks");
            // 
            // ScheduleRemain
            // 
            this.ScheduleRemain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScheduleRemain.Location = new System.Drawing.Point(447, 410);
            this.ScheduleRemain.Name = "ScheduleRemain";
            this.ScheduleRemain.Size = new System.Drawing.Size(47, 20);
            this.ScheduleRemain.TabIndex = 63;
            this.toolTip1.SetToolTip(this.ScheduleRemain, "Time Remaining in Recording");
            // 
            // MemFreq
            // 
            this.MemFreq.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MemFreq.Location = new System.Drawing.Point(516, 411);
            this.MemFreq.Name = "MemFreq";
            this.MemFreq.Size = new System.Drawing.Size(133, 20);
            this.MemFreq.TabIndex = 67;
            this.toolTip1.SetToolTip(this.MemFreq, "Frequency of currently selected Memory");
            // 
            // MemGroup
            // 
            this.MemGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MemGroup.Location = new System.Drawing.Point(655, 411);
            this.MemGroup.Name = "MemGroup";
            this.MemGroup.Size = new System.Drawing.Size(163, 20);
            this.MemGroup.TabIndex = 68;
            this.toolTip1.SetToolTip(this.MemGroup, "Group name of currently selected Memory");
            // 
            // MemName
            // 
            this.MemName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MemName.Location = new System.Drawing.Point(824, 411);
            this.MemName.Name = "MemName";
            this.MemName.Size = new System.Drawing.Size(127, 20);
            this.MemName.TabIndex = 69;
            this.toolTip1.SetToolTip(this.MemName, "Name of currently selected Memory");
            // 
            // buttonTS1
            // 
            this.buttonTS1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonTS1.Image = null;
            this.buttonTS1.Location = new System.Drawing.Point(347, 465);
            this.buttonTS1.Name = "buttonTS1";
            this.buttonTS1.Size = new System.Drawing.Size(131, 23);
            this.buttonTS1.TabIndex = 72;
            this.buttonTS1.Text = "Open Rec Folder";
            this.toolTip1.SetToolTip(this.buttonTS1, "Make the selected memory active ");
            this.buttonTS1.UseVisualStyleBackColor = true;
            this.buttonTS1.Click += new System.EventHandler(this.buttonTS1_Click);
            // 
            // ScheduleRepeatm
            // 
            this.ScheduleRepeatm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScheduleRepeatm.Image = null;
            this.ScheduleRepeatm.Location = new System.Drawing.Point(133, 436);
            this.ScheduleRepeatm.Name = "ScheduleRepeatm";
            this.ScheduleRepeatm.Size = new System.Drawing.Size(116, 23);
            this.ScheduleRepeatm.TabIndex = 70;
            this.ScheduleRepeatm.Text = "Schedule Monthly";
            this.toolTip1.SetToolTip(this.ScheduleRepeatm, "Check to Schedule every Month. \r\nWill auto check for Last Week of the month. \r\n\r\n" +
        "Turn Both off to turn of Memory Schedule.");
            this.ScheduleRepeatm.CheckedChanged += new System.EventHandler(this.ScheduleRepeatm_CheckedChanged);
            // 
            // ScheduleOn
            // 
            this.ScheduleOn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScheduleOn.Image = null;
            this.ScheduleOn.Location = new System.Drawing.Point(237, 454);
            this.ScheduleOn.Name = "ScheduleOn";
            this.ScheduleOn.Size = new System.Drawing.Size(101, 23);
            this.ScheduleOn.TabIndex = 62;
            this.ScheduleOn.Text = "Schedule On";
            this.toolTip1.SetToolTip(this.ScheduleOn, "Check box to turn of Scheduler for this Selected Memory.");
            this.ScheduleOn.UseCompatibleTextRendering = true;
            this.ScheduleOn.Visible = false;
            this.ScheduleOn.CheckedChanged += new System.EventHandler(this.ScheduleOn_CheckedChanged);
            // 
            // ScheduleDurationTime
            // 
            this.ScheduleDurationTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScheduleDurationTime.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.ScheduleDurationTime.Location = new System.Drawing.Point(347, 411);
            this.ScheduleDurationTime.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.ScheduleDurationTime.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.ScheduleDurationTime.Name = "ScheduleDurationTime";
            this.ScheduleDurationTime.Size = new System.Drawing.Size(56, 20);
            this.ScheduleDurationTime.TabIndex = 24;
            this.ScheduleDurationTime.TinyStep = false;
            this.toolTip1.SetToolTip(this.ScheduleDurationTime, "Duration of Scheduled recording (if Enabled)");
            this.ScheduleDurationTime.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.ScheduleDurationTime.ValueChanged += new System.EventHandler(this.ScheduleDurationTime_ValueChanged);
            // 
            // ScheduleRecord
            // 
            this.ScheduleRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScheduleRecord.Image = null;
            this.ScheduleRecord.Location = new System.Drawing.Point(363, 437);
            this.ScheduleRecord.Name = "ScheduleRecord";
            this.ScheduleRecord.Size = new System.Drawing.Size(127, 21);
            this.ScheduleRecord.TabIndex = 23;
            this.ScheduleRecord.Text = "Record on Schedule";
            this.toolTip1.SetToolTip(this.ScheduleRecord, "Check to record audio at scheduled time for the set Duration");
            this.ScheduleRecord.CheckedChanged += new System.EventHandler(this.ScheduleRecord_CheckedChanged);
            // 
            // ScheduleRepeat
            // 
            this.ScheduleRepeat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScheduleRepeat.Image = null;
            this.ScheduleRepeat.Location = new System.Drawing.Point(12, 437);
            this.ScheduleRepeat.Name = "ScheduleRepeat";
            this.ScheduleRepeat.Size = new System.Drawing.Size(116, 23);
            this.ScheduleRepeat.TabIndex = 22;
            this.ScheduleRepeat.Text = "Schedule Weekly";
            this.toolTip1.SetToolTip(this.ScheduleRepeat, "Check to Schedule every Week.\r\nTurn Both off to turn of Memory Schedule.");
            this.ScheduleRepeat.CheckedChanged += new System.EventHandler(this.ScheduleRepeat_CheckedChanged);
            // 
            // chkMemoryFormClose
            // 
            this.chkMemoryFormClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkMemoryFormClose.Image = null;
            this.chkMemoryFormClose.Location = new System.Drawing.Point(432, 518);
            this.chkMemoryFormClose.Name = "chkMemoryFormClose";
            this.chkMemoryFormClose.Size = new System.Drawing.Size(89, 32);
            this.chkMemoryFormClose.TabIndex = 12;
            this.chkMemoryFormClose.Text = "Close after selection";
            this.toolTip1.SetToolTip(this.chkMemoryFormClose, "Check to close the Memory window after an entry has been selected");
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSelect.Image = null;
            this.btnSelect.Location = new System.Drawing.Point(266, 520);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 5;
            this.btnSelect.Text = "Select";
            this.toolTip1.SetToolTip(this.btnSelect, "Make the selected memory active ");
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnMemoryRecordDelete
            // 
            this.btnMemoryRecordDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMemoryRecordDelete.Image = null;
            this.btnMemoryRecordDelete.Location = new System.Drawing.Point(174, 520);
            this.btnMemoryRecordDelete.Name = "btnMemoryRecordDelete";
            this.btnMemoryRecordDelete.Size = new System.Drawing.Size(75, 23);
            this.btnMemoryRecordDelete.TabIndex = 4;
            this.btnMemoryRecordDelete.Text = "Delete";
            this.toolTip1.SetToolTip(this.btnMemoryRecordDelete, "Delete the current row");
            this.btnMemoryRecordDelete.UseVisualStyleBackColor = true;
            this.btnMemoryRecordDelete.Click += new System.EventHandler(this.btnMemoryRecordDelete_Click);
            // 
            // btnMemoryRecordCopy
            // 
            this.btnMemoryRecordCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnMemoryRecordCopy.Image = null;
            this.btnMemoryRecordCopy.Location = new System.Drawing.Point(93, 520);
            this.btnMemoryRecordCopy.Name = "btnMemoryRecordCopy";
            this.btnMemoryRecordCopy.Size = new System.Drawing.Size(75, 23);
            this.btnMemoryRecordCopy.TabIndex = 3;
            this.btnMemoryRecordCopy.Text = "Copy";
            this.toolTip1.SetToolTip(this.btnMemoryRecordCopy, "Create a new row with the same values as the currently selected row");
            this.btnMemoryRecordCopy.UseVisualStyleBackColor = true;
            this.btnMemoryRecordCopy.Click += new System.EventHandler(this.btnMemoryRecordCopy_Click);
            // 
            // MemoryRecordAdd
            // 
            this.MemoryRecordAdd.AllowDrop = true;
            this.MemoryRecordAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.MemoryRecordAdd.Image = null;
            this.MemoryRecordAdd.Location = new System.Drawing.Point(12, 520);
            this.MemoryRecordAdd.Name = "MemoryRecordAdd";
            this.MemoryRecordAdd.Size = new System.Drawing.Size(75, 23);
            this.MemoryRecordAdd.TabIndex = 2;
            this.MemoryRecordAdd.Text = "Add";
            this.toolTip1.SetToolTip(this.MemoryRecordAdd, resources.GetString("MemoryRecordAdd.ToolTip"));
            this.MemoryRecordAdd.UseVisualStyleBackColor = true;
            this.MemoryRecordAdd.Click += new System.EventHandler(this.MemoryRecordAdd_Click);
            this.MemoryRecordAdd.DragDrop += new System.Windows.Forms.DragEventHandler(this.MemoryRecordAdd_DragDrop);
            this.MemoryRecordAdd.DragEnter += new System.Windows.Forms.DragEventHandler(this.MemoryRecordAdd_DragEnter);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(516, 480);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(435, 68);
            this.textBox1.TabIndex = 13;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.Location = new System.Drawing.Point(516, 440);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(435, 13);
            this.textBox3.TabIndex = 15;
            this.textBox3.Text = "Comments:";
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Location = new System.Drawing.Point(516, 395);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(133, 13);
            this.textBox2.TabIndex = 64;
            this.textBox2.Text = "Frequency: (mhz)";
            // 
            // textBox5
            // 
            this.textBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox5.Location = new System.Drawing.Point(655, 395);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(163, 13);
            this.textBox5.TabIndex = 65;
            this.textBox5.Text = "Group:";
            // 
            // textBox7
            // 
            this.textBox7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox7.Location = new System.Drawing.Point(824, 395);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(127, 13);
            this.textBox7.TabIndex = 66;
            this.textBox7.Text = "Name:";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "WAV files (*.wav)|*.wav|All files (*.*)|*.*";
            this.openFileDialog1.Multiselect = true;
            // 
            // ScheduleExtra
            // 
            this.ScheduleExtra.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ScheduleExtra.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.ScheduleExtra.Location = new System.Drawing.Point(237, 466);
            this.ScheduleExtra.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.ScheduleExtra.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.ScheduleExtra.Name = "ScheduleExtra";
            this.ScheduleExtra.Size = new System.Drawing.Size(56, 20);
            this.ScheduleExtra.TabIndex = 71;
            this.ScheduleExtra.TinyStep = false;
            this.ScheduleExtra.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.ScheduleExtra.Visible = false;
            // 
            // chkAlwaysOnTop
            // 
            this.chkAlwaysOnTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkAlwaysOnTop.Image = null;
            this.chkAlwaysOnTop.Location = new System.Drawing.Point(347, 516);
            this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            this.chkAlwaysOnTop.Size = new System.Drawing.Size(79, 36);
            this.chkAlwaysOnTop.TabIndex = 59;
            this.chkAlwaysOnTop.Text = "Always On Top";
            this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(this.chkAlwaysOnTop_CheckedChanged);
            // 
            // MemoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(975, 558);
            this.Controls.Add(this.buttonTS1);
            this.Controls.Add(this.ScheduleExtra);
            this.Controls.Add(this.ScheduleRepeatm);
            this.Controls.Add(this.MemName);
            this.Controls.Add(this.MemGroup);
            this.Controls.Add(this.MemFreq);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.ScheduleRemain);
            this.Controls.Add(this.ScheduleOn);
            this.Controls.Add(this.ScheduleStartTime);
            this.Controls.Add(this.ScheduleStartDate);
            this.Controls.Add(this.chkAlwaysOnTop);
            this.Controls.Add(this.ScheduleDurationTime);
            this.Controls.Add(this.ScheduleRecord);
            this.Controls.Add(this.ScheduleRepeat);
            this.Controls.Add(this.textBox8);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.MemComments);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.chkMemoryFormClose);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnMemoryRecordDelete);
            this.Controls.Add(this.btnMemoryRecordCopy);
            this.Controls.Add(this.MemoryRecordAdd);
            this.Controls.Add(this.dataGridView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(526, 367);
            this.Name = "MemoryForm";
            this.Text = "Memory Interface";
            this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MemoryForm_FormClosing);
            this.Load += new System.EventHandler(this.MemoryForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScheduleDurationTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ScheduleExtra)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        } //INITCOMPOENT

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ButtonTS btnMemoryRecordCopy;
        private System.Windows.Forms.ButtonTS btnMemoryRecordDelete;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ButtonTS btnSelect;
        private System.Windows.Forms.CheckBoxTS chkMemoryFormClose;
        public System.Windows.Forms.ButtonTS MemoryRecordAdd;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox MemComments;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.CheckBoxTS ScheduleRepeat;
        private System.Windows.Forms.CheckBoxTS ScheduleRecord;
        private System.Windows.Forms.NumericUpDownTS ScheduleDurationTime;
        private System.Windows.Forms.CheckBoxTS chkAlwaysOnTop;
        private System.Windows.Forms.DateTimePicker ScheduleStartDate;
        private System.Windows.Forms.DateTimePicker ScheduleStartTime;
        private System.Windows.Forms.CheckBoxTS ScheduleOn;
        private System.Windows.Forms.TextBox ScheduleRemain;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.TextBox MemFreq;
        private System.Windows.Forms.TextBox MemGroup;
        private System.Windows.Forms.TextBox MemName;
        private System.Windows.Forms.CheckBoxTS ScheduleRepeatm;
        private System.Windows.Forms.NumericUpDownTS ScheduleExtra;
        private System.Windows.Forms.ButtonTS buttonTS1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Timer timer1;
    } //memoryform

} //PowerSDR