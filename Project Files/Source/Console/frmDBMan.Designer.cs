namespace Thetis
{
    partial class frmDBMan
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "test",
            "sub1"}, -1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDBMan));
            this.lstActiveDBs = new System.Windows.Forms.ListView();
            this.colDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHardware = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colChanged = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAge = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colBackupOnStartup = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colBackupOnShutdown = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFolder = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listView2 = new System.Windows.Forms.ListView();
            this.numericUpDownTS1 = new System.Windows.Forms.NumericUpDownTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.radioButtonTS2 = new System.Windows.Forms.RadioButtonTS();
            this.radioButtonTS1 = new System.Windows.Forms.RadioButtonTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.buttonTS7 = new System.Windows.Forms.ButtonTS();
            this.buttonTS6 = new System.Windows.Forms.ButtonTS();
            this.buttonTS5 = new System.Windows.Forms.ButtonTS();
            this.buttonTS4 = new System.Windows.Forms.ButtonTS();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnDuplicateBD = new System.Windows.Forms.ButtonTS();
            this.btnMakeActive = new System.Windows.Forms.ButtonTS();
            this.btnTakeBackupNow = new System.Windows.Forms.ButtonTS();
            this.btnRemoveDB = new System.Windows.Forms.ButtonTS();
            this.btnNewDB = new System.Windows.Forms.ButtonTS();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTS1)).BeginInit();
            this.SuspendLayout();
            // 
            // lstActiveDBs
            // 
            this.lstActiveDBs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstActiveDBs.CheckBoxes = true;
            this.lstActiveDBs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDesc,
            this.colHardware,
            this.colChanged,
            this.colAge,
            this.colSize,
            this.colBackupOnStartup,
            this.colBackupOnShutdown,
            this.colFolder});
            this.lstActiveDBs.FullRowSelect = true;
            this.lstActiveDBs.GridLines = true;
            this.lstActiveDBs.HideSelection = false;
            listViewItem1.StateImageIndex = 0;
            this.lstActiveDBs.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.lstActiveDBs.Location = new System.Drawing.Point(12, 22);
            this.lstActiveDBs.MultiSelect = false;
            this.lstActiveDBs.Name = "lstActiveDBs";
            this.lstActiveDBs.ShowItemToolTips = true;
            this.lstActiveDBs.Size = new System.Drawing.Size(524, 334);
            this.lstActiveDBs.TabIndex = 7;
            this.lstActiveDBs.UseCompatibleStateImageBehavior = false;
            this.lstActiveDBs.View = System.Windows.Forms.View.Details;
            this.lstActiveDBs.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstActiveDBs_ItemCheck);
            this.lstActiveDBs.SelectedIndexChanged += new System.EventHandler(this.lstActiveDBs_SelectedIndexChanged);
            // 
            // colDesc
            // 
            this.colDesc.Text = "Description";
            this.colDesc.Width = 128;
            // 
            // colHardware
            // 
            this.colHardware.Text = "Hardware";
            this.colHardware.Width = 66;
            // 
            // colChanged
            // 
            this.colChanged.Text = "Last Changed";
            this.colChanged.Width = 70;
            // 
            // colAge
            // 
            this.colAge.Text = "Age";
            // 
            // colSize
            // 
            this.colSize.Text = "Size";
            // 
            // colBackupOnStartup
            // 
            this.colBackupOnStartup.Text = "Startup Backup";
            // 
            // colBackupOnShutdown
            // 
            this.colBackupOnShutdown.Text = "Shutdown Backup";
            // 
            // colFolder
            // 
            this.colFolder.Text = "Folder";
            // 
            // listView2
            // 
            this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView2.HideSelection = false;
            this.listView2.Location = new System.Drawing.Point(12, 382);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(524, 207);
            this.listView2.TabIndex = 8;
            this.listView2.UseCompatibleStateImageBehavior = false;
            // 
            // numericUpDownTS1
            // 
            this.numericUpDownTS1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDownTS1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTS1.Location = new System.Drawing.Point(559, 520);
            this.numericUpDownTS1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownTS1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTS1.Name = "numericUpDownTS1";
            this.numericUpDownTS1.Size = new System.Drawing.Size(61, 20);
            this.numericUpDownTS1.TabIndex = 13;
            this.numericUpDownTS1.TinyStep = false;
            this.numericUpDownTS1.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // labelTS2
            // 
            this.labelTS2.AutoSize = true;
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(12, 6);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(104, 13);
            this.labelTS2.TabIndex = 12;
            this.labelTS2.Text = "Available Databases";
            // 
            // radioButtonTS2
            // 
            this.radioButtonTS2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonTS2.AutoSize = true;
            this.radioButtonTS2.Image = null;
            this.radioButtonTS2.Location = new System.Drawing.Point(542, 474);
            this.radioButtonTS2.Name = "radioButtonTS2";
            this.radioButtonTS2.Size = new System.Drawing.Size(113, 17);
            this.radioButtonTS2.TabIndex = 11;
            this.radioButtonTS2.TabStop = true;
            this.radioButtonTS2.Text = "Unlimited Backups";
            this.radioButtonTS2.UseVisualStyleBackColor = true;
            // 
            // radioButtonTS1
            // 
            this.radioButtonTS1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButtonTS1.AutoSize = true;
            this.radioButtonTS1.Image = null;
            this.radioButtonTS1.Location = new System.Drawing.Point(542, 497);
            this.radioButtonTS1.Name = "radioButtonTS1";
            this.radioButtonTS1.Size = new System.Drawing.Size(100, 17);
            this.radioButtonTS1.TabIndex = 10;
            this.radioButtonTS1.TabStop = true;
            this.radioButtonTS1.Text = "Max # Backups";
            this.radioButtonTS1.UseVisualStyleBackColor = true;
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(12, 366);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(98, 13);
            this.labelTS1.TabIndex = 9;
            this.labelTS1.Text = "Database Backups";
            // 
            // buttonTS7
            // 
            this.buttonTS7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTS7.Image = null;
            this.buttonTS7.Location = new System.Drawing.Point(542, 382);
            this.buttonTS7.Name = "buttonTS7";
            this.buttonTS7.Selectable = true;
            this.buttonTS7.Size = new System.Drawing.Size(150, 40);
            this.buttonTS7.TabIndex = 6;
            this.buttonTS7.Text = "Make Backup Available";
            this.buttonTS7.UseVisualStyleBackColor = true;
            // 
            // buttonTS6
            // 
            this.buttonTS6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTS6.Image = null;
            this.buttonTS6.Location = new System.Drawing.Point(542, 428);
            this.buttonTS6.Name = "buttonTS6";
            this.buttonTS6.Selectable = true;
            this.buttonTS6.Size = new System.Drawing.Size(150, 40);
            this.buttonTS6.TabIndex = 5;
            this.buttonTS6.Text = "Remove Backup";
            this.buttonTS6.UseVisualStyleBackColor = true;
            // 
            // buttonTS5
            // 
            this.buttonTS5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTS5.Image = null;
            this.buttonTS5.Location = new System.Drawing.Point(542, 316);
            this.buttonTS5.Name = "buttonTS5";
            this.buttonTS5.Selectable = true;
            this.buttonTS5.Size = new System.Drawing.Size(150, 40);
            this.buttonTS5.TabIndex = 4;
            this.buttonTS5.Text = "Backup On Shut-Down";
            this.buttonTS5.UseVisualStyleBackColor = true;
            // 
            // buttonTS4
            // 
            this.buttonTS4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTS4.Image = null;
            this.buttonTS4.Location = new System.Drawing.Point(542, 270);
            this.buttonTS4.Name = "buttonTS4";
            this.buttonTS4.Selectable = true;
            this.buttonTS4.Size = new System.Drawing.Size(150, 40);
            this.buttonTS4.TabIndex = 3;
            this.buttonTS4.Text = "Backup On Start-Up";
            this.buttonTS4.UseVisualStyleBackColor = true;
            // 
            // btnDuplicateBD
            // 
            this.btnDuplicateBD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDuplicateBD.Image = global::Thetis.Properties.Resources.Data_Copy_32;
            this.btnDuplicateBD.Location = new System.Drawing.Point(638, 70);
            this.btnDuplicateBD.Name = "btnDuplicateBD";
            this.btnDuplicateBD.Selectable = true;
            this.btnDuplicateBD.Size = new System.Drawing.Size(42, 42);
            this.btnDuplicateBD.TabIndex = 21;
            this.toolTip1.SetToolTip(this.btnDuplicateBD, "Duplicate Database");
            this.btnDuplicateBD.UseVisualStyleBackColor = true;
            this.btnDuplicateBD.Click += new System.EventHandler(this.btnDuplicateBD_Click);
            // 
            // btnMakeActive
            // 
            this.btnMakeActive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMakeActive.Image = global::Thetis.Properties.Resources.Data_Replace_32;
            this.btnMakeActive.Location = new System.Drawing.Point(542, 22);
            this.btnMakeActive.Name = "btnMakeActive";
            this.btnMakeActive.Selectable = true;
            this.btnMakeActive.Size = new System.Drawing.Size(42, 42);
            this.btnMakeActive.TabIndex = 14;
            this.toolTip1.SetToolTip(this.btnMakeActive, "Make Active");
            this.btnMakeActive.UseVisualStyleBackColor = true;
            this.btnMakeActive.Click += new System.EventHandler(this.btnMakeActive_Click);
            // 
            // btnTakeBackupNow
            // 
            this.btnTakeBackupNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTakeBackupNow.Image = global::Thetis.Properties.Resources.Archive_32;
            this.btnTakeBackupNow.Location = new System.Drawing.Point(542, 152);
            this.btnTakeBackupNow.Name = "btnTakeBackupNow";
            this.btnTakeBackupNow.Selectable = true;
            this.btnTakeBackupNow.Size = new System.Drawing.Size(42, 42);
            this.btnTakeBackupNow.TabIndex = 2;
            this.toolTip1.SetToolTip(this.btnTakeBackupNow, "Take Backup Now");
            this.btnTakeBackupNow.UseVisualStyleBackColor = true;
            this.btnTakeBackupNow.Click += new System.EventHandler(this.btnTakeBackupNow_Click);
            // 
            // btnRemoveDB
            // 
            this.btnRemoveDB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveDB.Image = global::Thetis.Properties.Resources.Data_Delete_32;
            this.btnRemoveDB.Location = new System.Drawing.Point(590, 70);
            this.btnRemoveDB.Name = "btnRemoveDB";
            this.btnRemoveDB.Selectable = true;
            this.btnRemoveDB.Size = new System.Drawing.Size(42, 42);
            this.btnRemoveDB.TabIndex = 1;
            this.toolTip1.SetToolTip(this.btnRemoveDB, "Remove Database");
            this.btnRemoveDB.UseVisualStyleBackColor = true;
            this.btnRemoveDB.Click += new System.EventHandler(this.btnRemoveDB_Click);
            // 
            // btnNewDB
            // 
            this.btnNewDB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNewDB.Image = global::Thetis.Properties.Resources.Data_Add_32;
            this.btnNewDB.Location = new System.Drawing.Point(542, 70);
            this.btnNewDB.Name = "btnNewDB";
            this.btnNewDB.Selectable = true;
            this.btnNewDB.Size = new System.Drawing.Size(42, 42);
            this.btnNewDB.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnNewDB, "New Database");
            this.btnNewDB.UseVisualStyleBackColor = true;
            this.btnNewDB.Click += new System.EventHandler(this.btnNewDB_Click);
            // 
            // frmDBMan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(704, 601);
            this.Controls.Add(this.btnDuplicateBD);
            this.Controls.Add(this.btnMakeActive);
            this.Controls.Add(this.numericUpDownTS1);
            this.Controls.Add(this.labelTS2);
            this.Controls.Add(this.radioButtonTS2);
            this.Controls.Add(this.radioButtonTS1);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.listView2);
            this.Controls.Add(this.lstActiveDBs);
            this.Controls.Add(this.buttonTS7);
            this.Controls.Add(this.buttonTS6);
            this.Controls.Add(this.buttonTS5);
            this.Controls.Add(this.buttonTS4);
            this.Controls.Add(this.btnTakeBackupNow);
            this.Controls.Add(this.btnRemoveDB);
            this.Controls.Add(this.btnNewDB);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(720, 640);
            this.Name = "frmDBMan";
            this.Text = "Database Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDBMan_FormClosing);
            this.Shown += new System.EventHandler(this.frmDBMan_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTS1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ButtonTS btnNewDB;
        private System.Windows.Forms.ButtonTS btnRemoveDB;
        private System.Windows.Forms.ButtonTS btnTakeBackupNow;
        private System.Windows.Forms.ButtonTS buttonTS4;
        private System.Windows.Forms.ButtonTS buttonTS5;
        private System.Windows.Forms.ButtonTS buttonTS6;
        private System.Windows.Forms.ButtonTS buttonTS7;
        private System.Windows.Forms.ListView lstActiveDBs;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.LabelTS labelTS1;
        private System.Windows.Forms.RadioButtonTS radioButtonTS1;
        private System.Windows.Forms.RadioButtonTS radioButtonTS2;
        private System.Windows.Forms.LabelTS labelTS2;
        private System.Windows.Forms.NumericUpDownTS numericUpDownTS1;
        private System.Windows.Forms.ButtonTS btnMakeActive;
        private System.Windows.Forms.ButtonTS btnDuplicateBD;
        private System.Windows.Forms.ColumnHeader colDesc;
        private System.Windows.Forms.ColumnHeader colChanged;
        private System.Windows.Forms.ColumnHeader colAge;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.ColumnHeader colFolder;
        private System.Windows.Forms.ColumnHeader colHardware;
        private System.Windows.Forms.ColumnHeader colBackupOnStartup;
        private System.Windows.Forms.ColumnHeader colBackupOnShutdown;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}