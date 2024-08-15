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
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("test");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDBMan));
            this.lstActiveDBs = new System.Windows.Forms.ListView();
            this.colDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHardware = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colChanged = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAge = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colBackupOnStartup = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colBackupOnShutdown = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFolder = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lstBackups = new System.Windows.Forms.ListView();
            this.colTimeDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colBackupAge = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colBackupFilename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnRename = new System.Windows.Forms.ButtonTS();
            this.btnExport = new System.Windows.Forms.ButtonTS();
            this.btnImport = new System.Windows.Forms.ButtonTS();
            this.btnDuplicateDB = new System.Windows.Forms.ButtonTS();
            this.btnMakeActive = new System.Windows.Forms.ButtonTS();
            this.btnMakeBackupAvailable = new System.Windows.Forms.ButtonTS();
            this.btnRemoveBackup = new System.Windows.Forms.ButtonTS();
            this.btnBackupOnShutdown = new System.Windows.Forms.ButtonTS();
            this.btnBackupOnStart = new System.Windows.Forms.ButtonTS();
            this.btnTakeBackupNow = new System.Windows.Forms.ButtonTS();
            this.btnRemoveDB = new System.Windows.Forms.ButtonTS();
            this.btnNewDB = new System.Windows.Forms.ButtonTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
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
            this.colVersion,
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
            // colVersion
            // 
            this.colVersion.Text = "Version";
            // 
            // colFolder
            // 
            this.colFolder.Text = "Folder";
            // 
            // lstBackups
            // 
            this.lstBackups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstBackups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTimeDate,
            this.colBackupAge,
            this.colBackupFilename});
            this.lstBackups.FullRowSelect = true;
            this.lstBackups.GridLines = true;
            this.lstBackups.HideSelection = false;
            this.lstBackups.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem2});
            this.lstBackups.Location = new System.Drawing.Point(12, 382);
            this.lstBackups.Name = "lstBackups";
            this.lstBackups.ShowItemToolTips = true;
            this.lstBackups.Size = new System.Drawing.Size(524, 207);
            this.lstBackups.TabIndex = 8;
            this.lstBackups.UseCompatibleStateImageBehavior = false;
            this.lstBackups.View = System.Windows.Forms.View.Details;
            this.lstBackups.SelectedIndexChanged += new System.EventHandler(this.lstBackups_SelectedIndexChanged);
            // 
            // colTimeDate
            // 
            this.colTimeDate.Text = "TimeDate";
            // 
            // colBackupAge
            // 
            this.colBackupAge.Text = "Age";
            // 
            // colBackupFilename
            // 
            this.colBackupFilename.Text = "Filename";
            // 
            // btnRename
            // 
            this.btnRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRename.Image = global::Thetis.Properties.Resources.Data_Edit_32;
            this.btnRename.Location = new System.Drawing.Point(542, 248);
            this.btnRename.Name = "btnRename";
            this.btnRename.Selectable = true;
            this.btnRename.Size = new System.Drawing.Size(42, 42);
            this.btnRename.TabIndex = 24;
            this.toolTip1.SetToolTip(this.btnRename, "Change description for the selected database");
            this.btnRename.UseVisualStyleBackColor = true;
            this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.Image = global::Thetis.Properties.Resources.Data_Export_32;
            this.btnExport.Location = new System.Drawing.Point(638, 200);
            this.btnExport.Name = "btnExport";
            this.btnExport.Selectable = true;
            this.btnExport.Size = new System.Drawing.Size(42, 42);
            this.btnExport.TabIndex = 23;
            this.toolTip1.SetToolTip(this.btnExport, "Export the selected database");
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnImport
            // 
            this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnImport.Image = global::Thetis.Properties.Resources.Data_Import_32;
            this.btnImport.Location = new System.Drawing.Point(590, 134);
            this.btnImport.Name = "btnImport";
            this.btnImport.Selectable = true;
            this.btnImport.Size = new System.Drawing.Size(42, 42);
            this.btnImport.TabIndex = 22;
            this.toolTip1.SetToolTip(this.btnImport, "Import into the active database");
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnDuplicateDB
            // 
            this.btnDuplicateDB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDuplicateDB.Image = global::Thetis.Properties.Resources.Data_Copy_32;
            this.btnDuplicateDB.Location = new System.Drawing.Point(638, 70);
            this.btnDuplicateDB.Name = "btnDuplicateDB";
            this.btnDuplicateDB.Selectable = true;
            this.btnDuplicateDB.Size = new System.Drawing.Size(42, 42);
            this.btnDuplicateDB.TabIndex = 21;
            this.toolTip1.SetToolTip(this.btnDuplicateDB, "Duplicate the database");
            this.btnDuplicateDB.UseVisualStyleBackColor = true;
            this.btnDuplicateDB.Click += new System.EventHandler(this.btnDuplicateBD_Click);
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
            this.toolTip1.SetToolTip(this.btnMakeActive, "Make the database active");
            this.btnMakeActive.UseVisualStyleBackColor = true;
            this.btnMakeActive.Click += new System.EventHandler(this.btnMakeActive_Click);
            // 
            // btnMakeBackupAvailable
            // 
            this.btnMakeBackupAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMakeBackupAvailable.Image = global::Thetis.Properties.Resources.Data_Refresh_32;
            this.btnMakeBackupAvailable.Location = new System.Drawing.Point(542, 382);
            this.btnMakeBackupAvailable.Name = "btnMakeBackupAvailable";
            this.btnMakeBackupAvailable.Selectable = true;
            this.btnMakeBackupAvailable.Size = new System.Drawing.Size(42, 42);
            this.btnMakeBackupAvailable.TabIndex = 6;
            this.toolTip1.SetToolTip(this.btnMakeBackupAvailable, "Make the selected backup available");
            this.btnMakeBackupAvailable.UseVisualStyleBackColor = true;
            this.btnMakeBackupAvailable.Click += new System.EventHandler(this.btnMakeBackupAvailable_Click);
            // 
            // btnRemoveBackup
            // 
            this.btnRemoveBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveBackup.Image = global::Thetis.Properties.Resources.Data_Delete_32;
            this.btnRemoveBackup.Location = new System.Drawing.Point(542, 430);
            this.btnRemoveBackup.Name = "btnRemoveBackup";
            this.btnRemoveBackup.Selectable = true;
            this.btnRemoveBackup.Size = new System.Drawing.Size(42, 42);
            this.btnRemoveBackup.TabIndex = 5;
            this.toolTip1.SetToolTip(this.btnRemoveBackup, "Remove the selected backup");
            this.btnRemoveBackup.UseVisualStyleBackColor = true;
            this.btnRemoveBackup.Click += new System.EventHandler(this.btnRemoveBackup_Click);
            // 
            // btnBackupOnShutdown
            // 
            this.btnBackupOnShutdown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBackupOnShutdown.Image = global::Thetis.Properties.Resources.Data_Down_32;
            this.btnBackupOnShutdown.Location = new System.Drawing.Point(590, 200);
            this.btnBackupOnShutdown.Name = "btnBackupOnShutdown";
            this.btnBackupOnShutdown.Selectable = true;
            this.btnBackupOnShutdown.Size = new System.Drawing.Size(42, 42);
            this.btnBackupOnShutdown.TabIndex = 4;
            this.toolTip1.SetToolTip(this.btnBackupOnShutdown, "Toggle Backup On Shut-Down");
            this.btnBackupOnShutdown.UseVisualStyleBackColor = true;
            this.btnBackupOnShutdown.Click += new System.EventHandler(this.btnBackupOnShutdown_Click);
            // 
            // btnBackupOnStart
            // 
            this.btnBackupOnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBackupOnStart.Image = global::Thetis.Properties.Resources.Data_Up_32;
            this.btnBackupOnStart.Location = new System.Drawing.Point(542, 200);
            this.btnBackupOnStart.Name = "btnBackupOnStart";
            this.btnBackupOnStart.Selectable = true;
            this.btnBackupOnStart.Size = new System.Drawing.Size(42, 42);
            this.btnBackupOnStart.TabIndex = 3;
            this.toolTip1.SetToolTip(this.btnBackupOnStart, "Toggle Backup On Start-Up");
            this.btnBackupOnStart.UseVisualStyleBackColor = true;
            this.btnBackupOnStart.Click += new System.EventHandler(this.btnBackupOnStart_Click);
            // 
            // btnTakeBackupNow
            // 
            this.btnTakeBackupNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTakeBackupNow.Image = global::Thetis.Properties.Resources.Archive_32;
            this.btnTakeBackupNow.Location = new System.Drawing.Point(542, 134);
            this.btnTakeBackupNow.Name = "btnTakeBackupNow";
            this.btnTakeBackupNow.Selectable = true;
            this.btnTakeBackupNow.Size = new System.Drawing.Size(42, 42);
            this.btnTakeBackupNow.TabIndex = 2;
            this.toolTip1.SetToolTip(this.btnTakeBackupNow, "Take backup now of the active database");
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
            this.toolTip1.SetToolTip(this.btnRemoveDB, "Remove the database");
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
            this.toolTip1.SetToolTip(this.btnNewDB, "New database");
            this.btnNewDB.UseVisualStyleBackColor = true;
            this.btnNewDB.Click += new System.EventHandler(this.btnNewDB_Click);
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
            // frmDBMan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(704, 601);
            this.Controls.Add(this.btnRename);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnDuplicateDB);
            this.Controls.Add(this.btnMakeActive);
            this.Controls.Add(this.labelTS2);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.lstBackups);
            this.Controls.Add(this.lstActiveDBs);
            this.Controls.Add(this.btnMakeBackupAvailable);
            this.Controls.Add(this.btnRemoveBackup);
            this.Controls.Add(this.btnBackupOnShutdown);
            this.Controls.Add(this.btnBackupOnStart);
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ButtonTS btnNewDB;
        private System.Windows.Forms.ButtonTS btnRemoveDB;
        private System.Windows.Forms.ButtonTS btnTakeBackupNow;
        private System.Windows.Forms.ButtonTS btnBackupOnStart;
        private System.Windows.Forms.ButtonTS btnBackupOnShutdown;
        private System.Windows.Forms.ButtonTS btnRemoveBackup;
        private System.Windows.Forms.ButtonTS btnMakeBackupAvailable;
        private System.Windows.Forms.ListView lstActiveDBs;
        private System.Windows.Forms.ListView lstBackups;
        private System.Windows.Forms.LabelTS labelTS1;
        private System.Windows.Forms.LabelTS labelTS2;
        private System.Windows.Forms.ButtonTS btnMakeActive;
        private System.Windows.Forms.ButtonTS btnDuplicateDB;
        private System.Windows.Forms.ColumnHeader colDesc;
        private System.Windows.Forms.ColumnHeader colChanged;
        private System.Windows.Forms.ColumnHeader colAge;
        private System.Windows.Forms.ColumnHeader colSize;
        private System.Windows.Forms.ColumnHeader colFolder;
        private System.Windows.Forms.ColumnHeader colHardware;
        private System.Windows.Forms.ColumnHeader colBackupOnStartup;
        private System.Windows.Forms.ColumnHeader colBackupOnShutdown;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ColumnHeader colTimeDate;
        private System.Windows.Forms.ColumnHeader colBackupAge;
        private System.Windows.Forms.ColumnHeader colBackupFilename;
        private System.Windows.Forms.ColumnHeader colVersion;
        private System.Windows.Forms.ButtonTS btnImport;
        private System.Windows.Forms.ButtonTS btnExport;
        private System.Windows.Forms.ButtonTS btnRename;
    }
}