namespace Thetis
{
    partial class RAForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
//        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
/*        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
*/
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RAForm));
            this.RA_timer = new System.Windows.Forms.Timer(this.components);
            this.picRAGraph = new System.Windows.Forms.PictureBox();
            this.openFileDialog3 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.textBox_file_date_time = new System.Windows.Forms.TextBox();
            this.textBox_file_comment = new System.Windows.Forms.TextBox();
            this.groupBox_Rx_select = new System.Windows.Forms.GroupBoxTS();
            this.radioButton_Rx2_only = new System.Windows.Forms.RadioButtonTS();
            this.radioButton_Rx1_only = new System.Windows.Forms.RadioButtonTS();
            this.radioButton_both = new System.Windows.Forms.RadioButtonTS();
            this.groupBox_Rx2 = new System.Windows.Forms.GroupBoxTS();
            this.textBox_Rx2 = new System.Windows.Forms.TextBoxTS();
            this.button_writeFile = new System.Windows.Forms.ButtonTS();
            this.button_readFile = new System.Windows.Forms.ButtonTS();
            this.labelTS13 = new System.Windows.Forms.LabelTS();
            this.labelTS12 = new System.Windows.Forms.LabelTS();
            this.labelTS10 = new System.Windows.Forms.LabelTS();
            this.labelTS9 = new System.Windows.Forms.LabelTS();
            this.labelTS8 = new System.Windows.Forms.LabelTS();
            this.groupBoxTS8 = new System.Windows.Forms.GroupBoxTS();
            this.labelTS7 = new System.Windows.Forms.LabelTS();
            this.manual_xmin = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS6 = new System.Windows.Forms.LabelTS();
            this.manual_xmax = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS5 = new System.Windows.Forms.LabelTS();
            this.labelTS4 = new System.Windows.Forms.LabelTS();
            this.txtCursorTime = new System.Windows.Forms.LabelTS();
            this.txtCursorPower = new System.Windows.Forms.LabelTS();
            this.groupBoxTS6 = new System.Windows.Forms.GroupBoxTS();
            this.groupBoxTS7 = new System.Windows.Forms.GroupBoxTS();
            this.manual_rescale = new System.Windows.Forms.RadioButtonTS();
            this.auto_rescale = new System.Windows.Forms.RadioButtonTS();
            this.groupBox_scaling = new System.Windows.Forms.GroupBoxTS();
            this.manual_ymax = new System.Windows.Forms.NumericUpDownTS();
            this.manual_ymin = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.labelTS3 = new System.Windows.Forms.LabelTS();
            this.groupBoxTS5 = new System.Windows.Forms.GroupBoxTS();
            this.button_linear = new System.Windows.Forms.RadioButtonTS();
            this.button_dBm = new System.Windows.Forms.RadioButtonTS();
            this.groupBox_signal = new System.Windows.Forms.GroupBoxTS();
            this.textBox_Rx1 = new System.Windows.Forms.TextBoxTS();
            this.groupBoxTS4 = new System.Windows.Forms.GroupBoxTS();
            this.textBoxTS3 = new System.Windows.Forms.TextBoxTS();
            this.groupBoxTS3 = new System.Windows.Forms.GroupBoxTS();
            this.numericUpDown_mSec_between_measurements = new System.Windows.Forms.NumericUpDownTS();
            this.groupBoxTS2 = new System.Windows.Forms.GroupBoxTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.textBox_pts_collected = new System.Windows.Forms.TextBoxTS();
            this.groupBoxTS1 = new System.Windows.Forms.GroupBoxTS();
            this.numericUpDown_measurements_per_point = new System.Windows.Forms.NumericUpDownTS();
            this.groupBox2 = new System.Windows.Forms.GroupBoxTS();
            this.RArecordCheckBox = new System.Windows.Forms.CheckBoxTS();
            ((System.ComponentModel.ISupportInitialize)(this.picRAGraph)).BeginInit();
            this.groupBox_Rx_select.SuspendLayout();
            this.groupBox_Rx2.SuspendLayout();
            this.groupBoxTS8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.manual_xmin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.manual_xmax)).BeginInit();
            this.groupBoxTS6.SuspendLayout();
            this.groupBoxTS7.SuspendLayout();
            this.groupBox_scaling.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.manual_ymax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.manual_ymin)).BeginInit();
            this.groupBoxTS5.SuspendLayout();
            this.groupBox_signal.SuspendLayout();
            this.groupBoxTS4.SuspendLayout();
            this.groupBoxTS3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_mSec_between_measurements)).BeginInit();
            this.groupBoxTS2.SuspendLayout();
            this.groupBoxTS1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_measurements_per_point)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // RA_timer
            // 
            this.RA_timer.Interval = 50;
            this.RA_timer.Tick += new System.EventHandler(this.RA_timer_Tick);
            // 
            // picRAGraph
            // 
            this.picRAGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picRAGraph.Location = new System.Drawing.Point(196, 76);
            this.picRAGraph.Name = "picRAGraph";
            this.picRAGraph.Size = new System.Drawing.Size(738, 429);
            this.picRAGraph.TabIndex = 17;
            this.picRAGraph.TabStop = false;
            this.picRAGraph.Paint += new System.Windows.Forms.PaintEventHandler(this.picRAGraph_Paint);
            this.picRAGraph.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picRAGraph_MouseMove);
            // 
            // openFileDialog3
            // 
            this.openFileDialog3.FileName = "openFileDialog3";
            // 
            // textBox_file_date_time
            // 
            this.textBox_file_date_time.Location = new System.Drawing.Point(226, 77);
            this.textBox_file_date_time.Name = "textBox_file_date_time";
            this.textBox_file_date_time.Size = new System.Drawing.Size(181, 20);
            this.textBox_file_date_time.TabIndex = 40;
            this.textBox_file_date_time.Visible = false;
            // 
            // textBox_file_comment
            // 
            this.textBox_file_comment.Location = new System.Drawing.Point(226, 98);
            this.textBox_file_comment.Multiline = true;
            this.textBox_file_comment.Name = "textBox_file_comment";
            this.textBox_file_comment.Size = new System.Drawing.Size(707, 20);
            this.textBox_file_comment.TabIndex = 41;
            this.textBox_file_comment.Visible = false;
            // 
            // groupBox_Rx_select
            // 
            this.groupBox_Rx_select.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBox_Rx_select.Controls.Add(this.radioButton_Rx2_only);
            this.groupBox_Rx_select.Controls.Add(this.radioButton_Rx1_only);
            this.groupBox_Rx_select.Controls.Add(this.radioButton_both);
            this.groupBox_Rx_select.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox_Rx_select.Location = new System.Drawing.Point(12, 271);
            this.groupBox_Rx_select.Name = "groupBox_Rx_select";
            this.groupBox_Rx_select.Size = new System.Drawing.Size(90, 83);
            this.groupBox_Rx_select.TabIndex = 39;
            this.groupBox_Rx_select.TabStop = false;
            this.groupBox_Rx_select.Text = "display";
            // 
            // radioButton_Rx2_only
            // 
            this.radioButton_Rx2_only.AutoSize = true;
            this.radioButton_Rx2_only.Image = null;
            this.radioButton_Rx2_only.Location = new System.Drawing.Point(6, 55);
            this.radioButton_Rx2_only.Name = "radioButton_Rx2_only";
            this.radioButton_Rx2_only.Size = new System.Drawing.Size(66, 17);
            this.radioButton_Rx2_only.TabIndex = 10;
            this.radioButton_Rx2_only.Text = "Rx2 only";
            this.radioButton_Rx2_only.UseVisualStyleBackColor = true;
            // 
            // radioButton_Rx1_only
            // 
            this.radioButton_Rx1_only.AutoSize = true;
            this.radioButton_Rx1_only.Image = null;
            this.radioButton_Rx1_only.Location = new System.Drawing.Point(5, 33);
            this.radioButton_Rx1_only.Name = "radioButton_Rx1_only";
            this.radioButton_Rx1_only.Size = new System.Drawing.Size(66, 17);
            this.radioButton_Rx1_only.TabIndex = 9;
            this.radioButton_Rx1_only.Text = "Rx1 only";
            this.radioButton_Rx1_only.UseVisualStyleBackColor = true;
            // 
            // radioButton_both
            // 
            this.radioButton_both.AutoSize = true;
            this.radioButton_both.Checked = true;
            this.radioButton_both.Image = null;
            this.radioButton_both.Location = new System.Drawing.Point(4, 14);
            this.radioButton_both.Name = "radioButton_both";
            this.radioButton_both.Size = new System.Drawing.Size(47, 17);
            this.radioButton_both.TabIndex = 3;
            this.radioButton_both.TabStop = true;
            this.radioButton_both.Text = "Both";
            this.radioButton_both.UseVisualStyleBackColor = true;
            // 
            // groupBox_Rx2
            // 
            this.groupBox_Rx2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBox_Rx2.Controls.Add(this.textBox_Rx2);
            this.groupBox_Rx2.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox_Rx2.Location = new System.Drawing.Point(734, 14);
            this.groupBox_Rx2.Name = "groupBox_Rx2";
            this.groupBox_Rx2.Size = new System.Drawing.Size(200, 56);
            this.groupBox_Rx2.TabIndex = 38;
            this.groupBox_Rx2.TabStop = false;
            this.groupBox_Rx2.Text = "Rx2 signal (dBm)";
            // 
            // textBox_Rx2
            // 
            this.textBox_Rx2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Rx2.Location = new System.Drawing.Point(38, 17);
            this.textBox_Rx2.Name = "textBox_Rx2";
            this.textBox_Rx2.Size = new System.Drawing.Size(124, 29);
            this.textBox_Rx2.TabIndex = 13;
            this.textBox_Rx2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button_writeFile
            // 
            this.button_writeFile.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button_writeFile.Image = null;
            this.button_writeFile.Location = new System.Drawing.Point(12, 482);
            this.button_writeFile.Name = "button_writeFile";
            this.button_writeFile.Size = new System.Drawing.Size(127, 23);
            this.button_writeFile.TabIndex = 36;
            this.button_writeFile.Text = "write  data  file";
            this.button_writeFile.UseVisualStyleBackColor = false;
            this.button_writeFile.Click += new System.EventHandler(this.button_writeFile_Click);
            // 
            // button_readFile
            // 
            this.button_readFile.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button_readFile.Image = null;
            this.button_readFile.Location = new System.Drawing.Point(12, 457);
            this.button_readFile.Name = "button_readFile";
            this.button_readFile.Size = new System.Drawing.Size(127, 23);
            this.button_readFile.TabIndex = 35;
            this.button_readFile.Text = "read  data  file";
            this.button_readFile.UseVisualStyleBackColor = false;
            this.button_readFile.Click += new System.EventHandler(this.button_readFile_Click);
            // 
            // labelTS13
            // 
            this.labelTS13.AutoSize = true;
            this.labelTS13.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.labelTS13.ForeColor = System.Drawing.SystemColors.Control;
            this.labelTS13.Image = null;
            this.labelTS13.Location = new System.Drawing.Point(560, 514);
            this.labelTS13.Name = "labelTS13";
            this.labelTS13.Size = new System.Drawing.Size(79, 13);
            this.labelTS13.TabIndex = 33;
            this.labelTS13.Text = "Time (seconds)";
            this.labelTS13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTS12
            // 
            this.labelTS12.AutoSize = true;
            this.labelTS12.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.labelTS12.ForeColor = System.Drawing.SystemColors.Control;
            this.labelTS12.Image = null;
            this.labelTS12.Location = new System.Drawing.Point(64, 301);
            this.labelTS12.Name = "labelTS12";
            this.labelTS12.Size = new System.Drawing.Size(126, 13);
            this.labelTS12.TabIndex = 32;
            this.labelTS12.Text = "                    Signal (dBm)";
            this.labelTS12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTS10
            // 
            this.labelTS10.AutoSize = true;
            this.labelTS10.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.labelTS10.ForeColor = System.Drawing.SystemColors.Highlight;
            this.labelTS10.Image = null;
            this.labelTS10.Location = new System.Drawing.Point(9, 512);
            this.labelTS10.Name = "labelTS10";
            this.labelTS10.Size = new System.Drawing.Size(55, 13);
            this.labelTS10.TabIndex = 30;
            this.labelTS10.Text = "labelTS10";
            this.labelTS10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelTS9
            // 
            this.labelTS9.AutoSize = true;
            this.labelTS9.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.labelTS9.ForeColor = System.Drawing.SystemColors.Control;
            this.labelTS9.Image = null;
            this.labelTS9.Location = new System.Drawing.Point(911, 512);
            this.labelTS9.Name = "labelTS9";
            this.labelTS9.Size = new System.Drawing.Size(49, 13);
            this.labelTS9.TabIndex = 29;
            this.labelTS9.Text = "labelTS9";
            this.labelTS9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTS8
            // 
            this.labelTS8.AutoSize = true;
            this.labelTS8.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.labelTS8.ForeColor = System.Drawing.SystemColors.Control;
            this.labelTS8.Image = null;
            this.labelTS8.Location = new System.Drawing.Point(197, 512);
            this.labelTS8.Name = "labelTS8";
            this.labelTS8.Size = new System.Drawing.Size(49, 13);
            this.labelTS8.TabIndex = 28;
            this.labelTS8.Text = "labelTS8";
            this.labelTS8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxTS8
            // 
            this.groupBoxTS8.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBoxTS8.Controls.Add(this.labelTS7);
            this.groupBoxTS8.Controls.Add(this.manual_xmin);
            this.groupBoxTS8.Controls.Add(this.labelTS6);
            this.groupBoxTS8.Controls.Add(this.manual_xmax);
            this.groupBoxTS8.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBoxTS8.Location = new System.Drawing.Point(948, 388);
            this.groupBoxTS8.Name = "groupBoxTS8";
            this.groupBoxTS8.Size = new System.Drawing.Size(122, 117);
            this.groupBoxTS8.TabIndex = 27;
            this.groupBoxTS8.TabStop = false;
            this.groupBoxTS8.Text = "X axis";
            // 
            // labelTS7
            // 
            this.labelTS7.AutoSize = true;
            this.labelTS7.Image = null;
            this.labelTS7.Location = new System.Drawing.Point(18, 66);
            this.labelTS7.Name = "labelTS7";
            this.labelTS7.Size = new System.Drawing.Size(79, 13);
            this.labelTS7.TabIndex = 11;
            this.labelTS7.Text = "Xmin (seconds)";
            // 
            // manual_xmin
            // 
            this.manual_xmin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.manual_xmin.Location = new System.Drawing.Point(15, 81);
            this.manual_xmin.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.manual_xmin.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.manual_xmin.Name = "manual_xmin";
            this.manual_xmin.Size = new System.Drawing.Size(86, 20);
            this.manual_xmin.TabIndex = 10;
            this.manual_xmin.TinyStep = false;
            this.manual_xmin.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.manual_xmin.ValueChanged += new System.EventHandler(this.manual_xmin_ValueChanged);
            // 
            // labelTS6
            // 
            this.labelTS6.AutoSize = true;
            this.labelTS6.Image = null;
            this.labelTS6.Location = new System.Drawing.Point(16, 19);
            this.labelTS6.Name = "labelTS6";
            this.labelTS6.Size = new System.Drawing.Size(82, 13);
            this.labelTS6.TabIndex = 9;
            this.labelTS6.Text = "Xmax (seconds)";
            // 
            // manual_xmax
            // 
            this.manual_xmax.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.manual_xmax.Location = new System.Drawing.Point(14, 35);
            this.manual_xmax.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.manual_xmax.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.manual_xmax.Name = "manual_xmax";
            this.manual_xmax.Size = new System.Drawing.Size(86, 20);
            this.manual_xmax.TabIndex = 8;
            this.manual_xmax.TinyStep = false;
            this.manual_xmax.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.manual_xmax.ValueChanged += new System.EventHandler(this.manual_xmax_ValueChanged);
            // 
            // labelTS5
            // 
            this.labelTS5.AutoSize = true;
            this.labelTS5.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.labelTS5.ForeColor = System.Drawing.SystemColors.Control;
            this.labelTS5.Image = null;
            this.labelTS5.Location = new System.Drawing.Point(145, 492);
            this.labelTS5.Name = "labelTS5";
            this.labelTS5.Size = new System.Drawing.Size(49, 13);
            this.labelTS5.TabIndex = 26;
            this.labelTS5.Text = "labelTS5";
            this.labelTS5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTS4
            // 
            this.labelTS4.AutoSize = true;
            this.labelTS4.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.labelTS4.ForeColor = System.Drawing.SystemColors.Control;
            this.labelTS4.Image = null;
            this.labelTS4.Location = new System.Drawing.Point(154, 74);
            this.labelTS4.Name = "labelTS4";
            this.labelTS4.Size = new System.Drawing.Size(49, 13);
            this.labelTS4.TabIndex = 25;
            this.labelTS4.Text = "labelTS4";
            this.labelTS4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCursorTime
            // 
            this.txtCursorTime.AutoSize = true;
            this.txtCursorTime.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.txtCursorTime.Image = null;
            this.txtCursorTime.Location = new System.Drawing.Point(252, 513);
            this.txtCursorTime.Name = "txtCursorTime";
            this.txtCursorTime.Size = new System.Drawing.Size(10, 13);
            this.txtCursorTime.TabIndex = 22;
            this.txtCursorTime.Text = " ";
            // 
            // txtCursorPower
            // 
            this.txtCursorPower.AutoSize = true;
            this.txtCursorPower.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.txtCursorPower.Image = null;
            this.txtCursorPower.Location = new System.Drawing.Point(378, 513);
            this.txtCursorPower.Name = "txtCursorPower";
            this.txtCursorPower.Size = new System.Drawing.Size(10, 13);
            this.txtCursorPower.TabIndex = 21;
            this.txtCursorPower.Text = " ";
            // 
            // groupBoxTS6
            // 
            this.groupBoxTS6.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBoxTS6.Controls.Add(this.groupBoxTS7);
            this.groupBoxTS6.Controls.Add(this.groupBox_scaling);
            this.groupBoxTS6.Controls.Add(this.groupBoxTS5);
            this.groupBoxTS6.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBoxTS6.Location = new System.Drawing.Point(954, 74);
            this.groupBoxTS6.Name = "groupBoxTS6";
            this.groupBoxTS6.Size = new System.Drawing.Size(122, 267);
            this.groupBoxTS6.TabIndex = 20;
            this.groupBoxTS6.TabStop = false;
            this.groupBoxTS6.Text = "Y axis";
            // 
            // groupBoxTS7
            // 
            this.groupBoxTS7.Controls.Add(this.manual_rescale);
            this.groupBoxTS7.Controls.Add(this.auto_rescale);
            this.groupBoxTS7.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBoxTS7.Location = new System.Drawing.Point(8, 81);
            this.groupBoxTS7.Name = "groupBoxTS7";
            this.groupBoxTS7.Size = new System.Drawing.Size(108, 59);
            this.groupBoxTS7.TabIndex = 10;
            this.groupBoxTS7.TabStop = false;
            this.groupBoxTS7.Text = "scaling mode";
            // 
            // manual_rescale
            // 
            this.manual_rescale.AutoSize = true;
            this.manual_rescale.Checked = true;
            this.manual_rescale.Image = null;
            this.manual_rescale.Location = new System.Drawing.Point(5, 32);
            this.manual_rescale.Name = "manual_rescale";
            this.manual_rescale.Size = new System.Drawing.Size(59, 17);
            this.manual_rescale.TabIndex = 9;
            this.manual_rescale.TabStop = true;
            this.manual_rescale.Text = "manual";
            this.manual_rescale.UseVisualStyleBackColor = true;
            // 
            // auto_rescale
            // 
            this.auto_rescale.AutoSize = true;
            this.auto_rescale.Image = null;
            this.auto_rescale.Location = new System.Drawing.Point(4, 14);
            this.auto_rescale.Name = "auto_rescale";
            this.auto_rescale.Size = new System.Drawing.Size(71, 17);
            this.auto_rescale.TabIndex = 3;
            this.auto_rescale.Text = "automatic";
            this.auto_rescale.UseVisualStyleBackColor = true;
            // 
            // groupBox_scaling
            // 
            this.groupBox_scaling.Controls.Add(this.manual_ymax);
            this.groupBox_scaling.Controls.Add(this.manual_ymin);
            this.groupBox_scaling.Controls.Add(this.labelTS2);
            this.groupBox_scaling.Controls.Add(this.labelTS3);
            this.groupBox_scaling.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox_scaling.Location = new System.Drawing.Point(10, 146);
            this.groupBox_scaling.Name = "groupBox_scaling";
            this.groupBox_scaling.Size = new System.Drawing.Size(106, 110);
            this.groupBox_scaling.TabIndex = 8;
            this.groupBox_scaling.TabStop = false;
            this.groupBox_scaling.Text = "manual scaling";
            // 
            // manual_ymax
            // 
            this.manual_ymax.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.manual_ymax.Location = new System.Drawing.Point(9, 36);
            this.manual_ymax.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.manual_ymax.Minimum = new decimal(new int[] {
            140,
            0,
            0,
            -2147483648});
            this.manual_ymax.Name = "manual_ymax";
            this.manual_ymax.Size = new System.Drawing.Size(86, 20);
            this.manual_ymax.TabIndex = 32;
            this.manual_ymax.TinyStep = false;
            this.manual_ymax.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.manual_ymax.ValueChanged += new System.EventHandler(this.manual_ymax_ValueChanged);
            // 
            // manual_ymin
            // 
            this.manual_ymin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.manual_ymin.Location = new System.Drawing.Point(7, 79);
            this.manual_ymin.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.manual_ymin.Minimum = new decimal(new int[] {
            150,
            0,
            0,
            -2147483648});
            this.manual_ymin.Name = "manual_ymin";
            this.manual_ymin.Size = new System.Drawing.Size(86, 20);
            this.manual_ymin.TabIndex = 4;
            this.manual_ymin.TinyStep = false;
            this.manual_ymin.Value = new decimal(new int[] {
            150,
            0,
            0,
            -2147483648});
            this.manual_ymin.ValueChanged += new System.EventHandler(this.manual_ymin_ValueChanged);
            // 
            // labelTS2
            // 
            this.labelTS2.AutoSize = true;
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(5, 65);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(97, 13);
            this.labelTS2.TabIndex = 5;
            this.labelTS2.Text = "manual Ymin (dBm)";
            // 
            // labelTS3
            // 
            this.labelTS3.AutoSize = true;
            this.labelTS3.Image = null;
            this.labelTS3.Location = new System.Drawing.Point(2, 21);
            this.labelTS3.Name = "labelTS3";
            this.labelTS3.Size = new System.Drawing.Size(100, 13);
            this.labelTS3.TabIndex = 6;
            this.labelTS3.Text = "manual Ymax (dBm)";
            // 
            // groupBoxTS5
            // 
            this.groupBoxTS5.Controls.Add(this.button_linear);
            this.groupBoxTS5.Controls.Add(this.button_dBm);
            this.groupBoxTS5.Location = new System.Drawing.Point(7, 13);
            this.groupBoxTS5.Name = "groupBoxTS5";
            this.groupBoxTS5.Size = new System.Drawing.Size(106, 66);
            this.groupBoxTS5.TabIndex = 2;
            this.groupBoxTS5.TabStop = false;
            // 
            // button_linear
            // 
            this.button_linear.AutoSize = true;
            this.button_linear.Image = null;
            this.button_linear.Location = new System.Drawing.Point(5, 34);
            this.button_linear.Name = "button_linear";
            this.button_linear.Size = new System.Drawing.Size(50, 17);
            this.button_linear.TabIndex = 0;
            this.button_linear.Text = "linear";
            this.button_linear.UseVisualStyleBackColor = true;
            this.button_linear.CheckedChanged += new System.EventHandler(this.button_linear_CheckedChanged);
            // 
            // button_dBm
            // 
            this.button_dBm.AutoSize = true;
            this.button_dBm.Checked = true;
            this.button_dBm.Image = null;
            this.button_dBm.Location = new System.Drawing.Point(5, 12);
            this.button_dBm.Name = "button_dBm";
            this.button_dBm.Size = new System.Drawing.Size(46, 17);
            this.button_dBm.TabIndex = 1;
            this.button_dBm.TabStop = true;
            this.button_dBm.Text = "dBm";
            this.button_dBm.UseVisualStyleBackColor = true;
            this.button_dBm.CheckedChanged += new System.EventHandler(this.button_dBm_CheckedChanged);
            // 
            // groupBox_signal
            // 
            this.groupBox_signal.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBox_signal.Controls.Add(this.textBox_Rx1);
            this.groupBox_signal.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox_signal.Location = new System.Drawing.Point(512, 13);
            this.groupBox_signal.Name = "groupBox_signal";
            this.groupBox_signal.Size = new System.Drawing.Size(200, 56);
            this.groupBox_signal.TabIndex = 19;
            this.groupBox_signal.TabStop = false;
            this.groupBox_signal.Text = "Rx1 signal (dBm)";
            // 
            // textBox_Rx1
            // 
            this.textBox_Rx1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Rx1.Location = new System.Drawing.Point(38, 17);
            this.textBox_Rx1.Name = "textBox_Rx1";
            this.textBox_Rx1.Size = new System.Drawing.Size(124, 29);
            this.textBox_Rx1.TabIndex = 13;
            this.textBox_Rx1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBoxTS4
            // 
            this.groupBoxTS4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBoxTS4.Controls.Add(this.textBoxTS3);
            this.groupBoxTS4.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBoxTS4.Location = new System.Drawing.Point(200, 8);
            this.groupBoxTS4.Name = "groupBoxTS4";
            this.groupBoxTS4.Size = new System.Drawing.Size(184, 56);
            this.groupBoxTS4.TabIndex = 18;
            this.groupBoxTS4.TabStop = false;
            this.groupBoxTS4.Text = "elapsed time (sec)";
            // 
            // textBoxTS3
            // 
            this.textBoxTS3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTS3.Location = new System.Drawing.Point(26, 17);
            this.textBoxTS3.Name = "textBoxTS3";
            this.textBoxTS3.Size = new System.Drawing.Size(124, 29);
            this.textBoxTS3.TabIndex = 14;
            this.textBoxTS3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBoxTS3
            // 
            this.groupBoxTS3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBoxTS3.Controls.Add(this.numericUpDown_mSec_between_measurements);
            this.groupBoxTS3.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBoxTS3.Location = new System.Drawing.Point(12, 121);
            this.groupBoxTS3.Name = "groupBoxTS3";
            this.groupBoxTS3.Size = new System.Drawing.Size(127, 59);
            this.groupBoxTS3.TabIndex = 12;
            this.groupBoxTS3.TabStop = false;
            this.groupBoxTS3.Text = "mSec between measurements";
            // 
            // numericUpDown_mSec_between_measurements
            // 
            this.numericUpDown_mSec_between_measurements.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_mSec_between_measurements.Location = new System.Drawing.Point(27, 31);
            this.numericUpDown_mSec_between_measurements.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_mSec_between_measurements.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDown_mSec_between_measurements.Name = "numericUpDown_mSec_between_measurements";
            this.numericUpDown_mSec_between_measurements.Size = new System.Drawing.Size(63, 20);
            this.numericUpDown_mSec_between_measurements.TabIndex = 11;
            this.numericUpDown_mSec_between_measurements.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_mSec_between_measurements.TinyStep = false;
            this.numericUpDown_mSec_between_measurements.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDown_mSec_between_measurements.ValueChanged += new System.EventHandler(this.numericUpDownTS2_ValueChanged);
            // 
            // groupBoxTS2
            // 
            this.groupBoxTS2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBoxTS2.Controls.Add(this.labelTS1);
            this.groupBoxTS2.Controls.Add(this.textBox_pts_collected);
            this.groupBoxTS2.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBoxTS2.Location = new System.Drawing.Point(12, 375);
            this.groupBoxTS2.Name = "groupBoxTS2";
            this.groupBoxTS2.Size = new System.Drawing.Size(127, 76);
            this.groupBoxTS2.TabIndex = 10;
            this.groupBoxTS2.TabStop = false;
            this.groupBoxTS2.Text = "# of averaged  points collected/saved";
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(22, 53);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(83, 13);
            this.labelTS1.TabIndex = 20;
            this.labelTS1.Text = "(2,000,000 max)";
            // 
            // textBox_pts_collected
            // 
            this.textBox_pts_collected.Location = new System.Drawing.Point(13, 29);
            this.textBox_pts_collected.Name = "textBox_pts_collected";
            this.textBox_pts_collected.Size = new System.Drawing.Size(100, 20);
            this.textBox_pts_collected.TabIndex = 9;
            this.textBox_pts_collected.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBoxTS1
            // 
            this.groupBoxTS1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBoxTS1.Controls.Add(this.numericUpDown_measurements_per_point);
            this.groupBoxTS1.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBoxTS1.Location = new System.Drawing.Point(12, 187);
            this.groupBoxTS1.Name = "groupBoxTS1";
            this.groupBoxTS1.Size = new System.Drawing.Size(127, 65);
            this.groupBoxTS1.TabIndex = 8;
            this.groupBoxTS1.TabStop = false;
            this.groupBoxTS1.Text = "# of measurements to average per point";
            // 
            // numericUpDown_measurements_per_point
            // 
            this.numericUpDown_measurements_per_point.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_measurements_per_point.Location = new System.Drawing.Point(27, 35);
            this.numericUpDown_measurements_per_point.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDown_measurements_per_point.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_measurements_per_point.Name = "numericUpDown_measurements_per_point";
            this.numericUpDown_measurements_per_point.Size = new System.Drawing.Size(69, 20);
            this.numericUpDown_measurements_per_point.TabIndex = 0;
            this.numericUpDown_measurements_per_point.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown_measurements_per_point.TinyStep = false;
            this.numericUpDown_measurements_per_point.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_measurements_per_point.ValueChanged += new System.EventHandler(this.numericUpDownTS1_ValueChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.groupBox2.Controls.Add(this.RArecordCheckBox);
            this.groupBox2.ForeColor = System.Drawing.SystemColors.Control;
            this.groupBox2.Location = new System.Drawing.Point(12, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(127, 64);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Acquire Data";
            // 
            // RArecordCheckBox
            // 
            this.RArecordCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.RArecordCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.RArecordCheckBox.ForeColor = System.Drawing.SystemColors.Control;
            this.RArecordCheckBox.Image = null;
            this.RArecordCheckBox.Location = new System.Drawing.Point(9, 17);
            this.RArecordCheckBox.Name = "RArecordCheckBox";
            this.RArecordCheckBox.Size = new System.Drawing.Size(106, 39);
            this.RArecordCheckBox.TabIndex = 0;
            this.RArecordCheckBox.Text = "Start";
            this.RArecordCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.RArecordCheckBox.CheckedChanged += new System.EventHandler(this.RArecordCheckBox_CheckedChanged);
            // 
            // RAForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.ClientSize = new System.Drawing.Size(1088, 562);
            this.Controls.Add(this.textBox_file_comment);
            this.Controls.Add(this.textBox_file_date_time);
            this.Controls.Add(this.groupBox_Rx_select);
            this.Controls.Add(this.groupBox_Rx2);
            this.Controls.Add(this.button_writeFile);
            this.Controls.Add(this.button_readFile);
            this.Controls.Add(this.labelTS13);
            this.Controls.Add(this.labelTS12);
            this.Controls.Add(this.labelTS10);
            this.Controls.Add(this.labelTS9);
            this.Controls.Add(this.labelTS8);
            this.Controls.Add(this.groupBoxTS8);
            this.Controls.Add(this.labelTS5);
            this.Controls.Add(this.labelTS4);
            this.Controls.Add(this.txtCursorTime);
            this.Controls.Add(this.txtCursorPower);
            this.Controls.Add(this.groupBoxTS6);
            this.Controls.Add(this.groupBox_signal);
            this.Controls.Add(this.groupBoxTS4);
            this.Controls.Add(this.picRAGraph);
            this.Controls.Add(this.groupBoxTS3);
            this.Controls.Add(this.groupBoxTS2);
            this.Controls.Add(this.groupBoxTS1);
            this.Controls.Add(this.groupBox2);
            this.ForeColor = System.Drawing.SystemColors.Control;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "RAForm";
            this.Text = "Radio Astromony data collection utility   v1.3 (16 Feb 2016)";
            this.Load += new System.EventHandler(this.RAForm_Load);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RAForm_MouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.picRAGraph)).EndInit();
            this.groupBox_Rx_select.ResumeLayout(false);
            this.groupBox_Rx_select.PerformLayout();
            this.groupBox_Rx2.ResumeLayout(false);
            this.groupBox_Rx2.PerformLayout();
            this.groupBoxTS8.ResumeLayout(false);
            this.groupBoxTS8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.manual_xmin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.manual_xmax)).EndInit();
            this.groupBoxTS6.ResumeLayout(false);
            this.groupBoxTS7.ResumeLayout(false);
            this.groupBoxTS7.PerformLayout();
            this.groupBox_scaling.ResumeLayout(false);
            this.groupBox_scaling.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.manual_ymax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.manual_ymin)).EndInit();
            this.groupBoxTS5.ResumeLayout(false);
            this.groupBoxTS5.PerformLayout();
            this.groupBox_signal.ResumeLayout(false);
            this.groupBox_signal.PerformLayout();
            this.groupBoxTS4.ResumeLayout(false);
            this.groupBoxTS4.PerformLayout();
            this.groupBoxTS3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_mSec_between_measurements)).EndInit();
            this.groupBoxTS2.ResumeLayout(false);
            this.groupBoxTS2.PerformLayout();
            this.groupBoxTS1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_measurements_per_point)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBoxTS groupBox2;
        private System.Windows.Forms.CheckBoxTS RArecordCheckBox;
        private System.Windows.Forms.GroupBoxTS groupBoxTS1;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.NumericUpDownTS numericUpDown_measurements_per_point;
        private System.Windows.Forms.Timer RA_timer;
        private System.Windows.Forms.TextBoxTS textBox_pts_collected;
        private System.Windows.Forms.GroupBoxTS groupBoxTS2;
        private System.Windows.Forms.NumericUpDownTS numericUpDown_mSec_between_measurements;
        private System.Windows.Forms.GroupBoxTS groupBoxTS3;
        private System.Windows.Forms.TextBoxTS textBox_Rx1;
        private System.Windows.Forms.TextBoxTS textBoxTS3;
        private System.Windows.Forms.PictureBox picRAGraph;
        private System.Windows.Forms.GroupBoxTS groupBoxTS4;
        private System.Windows.Forms.GroupBoxTS groupBox_signal;
        private System.Windows.Forms.LabelTS labelTS1;
        private System.Windows.Forms.GroupBoxTS groupBoxTS6;
        private System.Windows.Forms.RadioButtonTS button_dBm;
        private System.Windows.Forms.RadioButtonTS button_linear;
        private System.Windows.Forms.LabelTS txtCursorPower;
        private System.Windows.Forms.LabelTS txtCursorTime;
        private System.Windows.Forms.LabelTS labelTS3;
        private System.Windows.Forms.LabelTS labelTS2;
        private System.Windows.Forms.NumericUpDownTS manual_ymin;
        private System.Windows.Forms.RadioButtonTS auto_rescale;
        private System.Windows.Forms.GroupBoxTS groupBoxTS5;
        private System.Windows.Forms.GroupBoxTS groupBox_scaling;
        private System.Windows.Forms.GroupBoxTS groupBoxTS7;
        private System.Windows.Forms.RadioButtonTS manual_rescale;
        private System.Windows.Forms.LabelTS labelTS4;
        private System.Windows.Forms.LabelTS labelTS5;
        private System.Windows.Forms.GroupBoxTS groupBoxTS8;
        private System.Windows.Forms.LabelTS labelTS7;
        private System.Windows.Forms.NumericUpDownTS manual_xmin;
        private System.Windows.Forms.LabelTS labelTS6;
        private System.Windows.Forms.NumericUpDownTS manual_xmax;
        private System.Windows.Forms.LabelTS labelTS8;
        private System.Windows.Forms.LabelTS labelTS9;
        private System.Windows.Forms.LabelTS labelTS10;
        private System.Windows.Forms.NumericUpDownTS manual_ymax;
        private System.Windows.Forms.LabelTS labelTS12;
        private System.Windows.Forms.LabelTS labelTS13;
        private System.Windows.Forms.ButtonTS button_readFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog3;
        private System.Windows.Forms.ButtonTS button_writeFile;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.GroupBoxTS groupBox_Rx2;
        private System.Windows.Forms.TextBoxTS textBox_Rx2;
        private System.Windows.Forms.GroupBoxTS groupBox_Rx_select;
        private System.Windows.Forms.RadioButtonTS radioButton_Rx2_only;
        private System.Windows.Forms.RadioButtonTS radioButton_Rx1_only;
        private System.Windows.Forms.RadioButtonTS radioButton_both;
        private System.Windows.Forms.TextBox textBox_file_date_time;
        private System.Windows.Forms.TextBox textBox_file_comment;

    }
}