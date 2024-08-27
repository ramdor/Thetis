
namespace Thetis
{
    partial class frmBandStack2
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnHideSelected = new System.Windows.Forms.ButtonTS();
            this.chkAlwaysOnTop = new System.Windows.Forms.CheckBoxTS();
            this.btnAddStackEntry = new System.Windows.Forms.ButtonTS();
            this.lblFilterName = new System.Windows.Forms.LabelTS();
            this.btnShowBandStackFilterManager = new System.Windows.Forms.ButtonTS();
            this.btnDeleteSelected = new System.Windows.Forms.ButtonTS();
            this.btnLockSelected = new System.Windows.Forms.ButtonTS();
            this.btnOptions = new System.Windows.Forms.ButtonTS();
            this.radioLastUsed = new System.Windows.Forms.RadioButtonTS();
            this.btnSetSpecific = new System.Windows.Forms.ButtonTS();
            this.radioSpecific = new System.Windows.Forms.RadioButtonTS();
            this.radioLastUsedEntry = new System.Windows.Forms.RadioButtonTS();
            this.btnUpdateEntry = new System.Windows.Forms.ButtonTS();
            this.chkShowHidden = new System.Windows.Forms.CheckBoxTS();
            this.chkIgnoreDuplicates = new System.Windows.Forms.CheckBoxTS();
            this.chkHideOnSelect = new System.Windows.Forms.CheckBoxTS();
            this.chkShowInSpectrum = new System.Windows.Forms.CheckBoxTS();
            this.lblFilterNameCaption = new System.Windows.Forms.LabelTS();
            this.bandStackListBox = new Thetis.BandStackListBox();
            this.SuspendLayout();
            // 
            // btnHideSelected
            // 
            this.btnHideSelected.Enabled = false;
            this.btnHideSelected.Image = null;
            this.btnHideSelected.Location = new System.Drawing.Point(246, 318);
            this.btnHideSelected.Name = "btnHideSelected";
            this.btnHideSelected.Selectable = true;
            this.btnHideSelected.Size = new System.Drawing.Size(117, 31);
            this.btnHideSelected.TabIndex = 13;
            this.btnHideSelected.Text = "Hide Selected (wip)";
            this.toolTip1.SetToolTip(this.btnHideSelected, "Hides this entry from the results of this filter ONLY");
            this.btnHideSelected.UseVisualStyleBackColor = true;
            this.btnHideSelected.Visible = false;
            // 
            // chkAlwaysOnTop
            // 
            this.chkAlwaysOnTop.AutoSize = true;
            this.chkAlwaysOnTop.Image = null;
            this.chkAlwaysOnTop.Location = new System.Drawing.Point(386, 12);
            this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
            this.chkAlwaysOnTop.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkAlwaysOnTop.Size = new System.Drawing.Size(98, 17);
            this.chkAlwaysOnTop.TabIndex = 12;
            this.chkAlwaysOnTop.Text = "Always On Top";
            this.toolTip1.SetToolTip(this.chkAlwaysOnTop, "This window is on top of all others always");
            this.chkAlwaysOnTop.UseVisualStyleBackColor = true;
            this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(this.chkAlwaysOnTop_CheckedChanged);
            // 
            // btnAddStackEntry
            // 
            this.btnAddStackEntry.Image = null;
            this.btnAddStackEntry.Location = new System.Drawing.Point(246, 157);
            this.btnAddStackEntry.Name = "btnAddStackEntry";
            this.btnAddStackEntry.Selectable = true;
            this.btnAddStackEntry.Size = new System.Drawing.Size(117, 64);
            this.btnAddStackEntry.TabIndex = 11;
            this.btnAddStackEntry.Text = "Add New Entry";
            this.toolTip1.SetToolTip(this.btnAddStackEntry, "Adds a new entry");
            this.btnAddStackEntry.UseVisualStyleBackColor = true;
            this.btnAddStackEntry.Click += new System.EventHandler(this.btnAddStackEntry_Click);
            // 
            // lblFilterName
            // 
            this.lblFilterName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilterName.Image = null;
            this.lblFilterName.Location = new System.Drawing.Point(61, 12);
            this.lblFilterName.Name = "lblFilterName";
            this.lblFilterName.Size = new System.Drawing.Size(86, 16);
            this.lblFilterName.TabIndex = 10;
            this.lblFilterName.Text = "xxxxxx";
            this.toolTip1.SetToolTip(this.lblFilterName, "The current filter or description if one has been given");
            // 
            // btnShowBandStackFilterManager
            // 
            this.btnShowBandStackFilterManager.Enabled = false;
            this.btnShowBandStackFilterManager.Image = null;
            this.btnShowBandStackFilterManager.Location = new System.Drawing.Point(369, 355);
            this.btnShowBandStackFilterManager.Name = "btnShowBandStackFilterManager";
            this.btnShowBandStackFilterManager.Selectable = true;
            this.btnShowBandStackFilterManager.Size = new System.Drawing.Size(117, 31);
            this.btnShowBandStackFilterManager.TabIndex = 9;
            this.btnShowBandStackFilterManager.Text = "Filter Manager (wip)";
            this.toolTip1.SetToolTip(this.btnShowBandStackFilterManager, "Show the filter manager");
            this.btnShowBandStackFilterManager.UseVisualStyleBackColor = true;
            this.btnShowBandStackFilterManager.Visible = false;
            // 
            // btnDeleteSelected
            // 
            this.btnDeleteSelected.Image = null;
            this.btnDeleteSelected.Location = new System.Drawing.Point(246, 355);
            this.btnDeleteSelected.Name = "btnDeleteSelected";
            this.btnDeleteSelected.Selectable = true;
            this.btnDeleteSelected.Size = new System.Drawing.Size(117, 31);
            this.btnDeleteSelected.TabIndex = 8;
            this.btnDeleteSelected.Text = "Delete Selected";
            this.toolTip1.SetToolTip(this.btnDeleteSelected, "Removes this entry. It can not appear in any filter results.");
            this.btnDeleteSelected.UseVisualStyleBackColor = true;
            this.btnDeleteSelected.Click += new System.EventHandler(this.btnDeleteSelected_Click);
            // 
            // btnLockSelected
            // 
            this.btnLockSelected.Image = null;
            this.btnLockSelected.Location = new System.Drawing.Point(246, 120);
            this.btnLockSelected.Name = "btnLockSelected";
            this.btnLockSelected.Selectable = true;
            this.btnLockSelected.Size = new System.Drawing.Size(117, 31);
            this.btnLockSelected.TabIndex = 7;
            this.btnLockSelected.Text = "Lock Selected";
            this.toolTip1.SetToolTip(this.btnLockSelected, "Locks the currently selected entry so that changes to it are prevented");
            this.btnLockSelected.UseVisualStyleBackColor = true;
            this.btnLockSelected.Click += new System.EventHandler(this.btnLockSelected_Click);
            // 
            // btnOptions
            // 
            this.btnOptions.Image = null;
            this.btnOptions.Location = new System.Drawing.Point(153, 5);
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Selectable = true;
            this.btnOptions.Size = new System.Drawing.Size(75, 31);
            this.btnOptions.TabIndex = 6;
            this.btnOptions.Text = "Options >>";
            this.toolTip1.SetToolTip(this.btnOptions, "Expand/shrink the options");
            this.btnOptions.UseVisualStyleBackColor = true;
            this.btnOptions.Click += new System.EventHandler(this.btnOptions_Click);
            // 
            // radioLastUsed
            // 
            this.radioLastUsed.AutoSize = true;
            this.radioLastUsed.Image = null;
            this.radioLastUsed.Location = new System.Drawing.Point(246, 88);
            this.radioLastUsed.Name = "radioLastUsed";
            this.radioLastUsed.Size = new System.Drawing.Size(164, 17);
            this.radioLastUsed.TabIndex = 5;
            this.radioLastUsed.TabStop = true;
            this.radioLastUsed.Text = "Return to last used frequency";
            this.toolTip1.SetToolTip(this.radioLastUsed, "You will return to the last used frequency.");
            this.radioLastUsed.UseVisualStyleBackColor = true;
            this.radioLastUsed.CheckedChanged += new System.EventHandler(this.radioLastUsed_CheckedChanged);
            // 
            // btnSetSpecific
            // 
            this.btnSetSpecific.Image = null;
            this.btnSetSpecific.Location = new System.Drawing.Point(386, 61);
            this.btnSetSpecific.Name = "btnSetSpecific";
            this.btnSetSpecific.Selectable = true;
            this.btnSetSpecific.Size = new System.Drawing.Size(45, 24);
            this.btnSetSpecific.TabIndex = 4;
            this.btnSetSpecific.Text = "Set";
            this.toolTip1.SetToolTip(this.btnSetSpecific, "Set the specific selected entry");
            this.btnSetSpecific.UseVisualStyleBackColor = true;
            this.btnSetSpecific.Click += new System.EventHandler(this.btnSetSpecific_Click);
            // 
            // radioSpecific
            // 
            this.radioSpecific.AutoSize = true;
            this.radioSpecific.Image = null;
            this.radioSpecific.Location = new System.Drawing.Point(246, 65);
            this.radioSpecific.Name = "radioSpecific";
            this.radioSpecific.Size = new System.Drawing.Size(134, 17);
            this.radioSpecific.TabIndex = 3;
            this.radioSpecific.TabStop = true;
            this.radioSpecific.Text = "Return to specific entry";
            this.toolTip1.SetToolTip(this.radioSpecific, "You will return to the specific entry. If it has been removed you will return to " +
        "last use frequency.");
            this.radioSpecific.UseVisualStyleBackColor = true;
            this.radioSpecific.CheckedChanged += new System.EventHandler(this.radioSpecific_CheckedChanged);
            // 
            // radioLastUsedEntry
            // 
            this.radioLastUsedEntry.AutoSize = true;
            this.radioLastUsedEntry.Image = null;
            this.radioLastUsedEntry.Location = new System.Drawing.Point(246, 42);
            this.radioLastUsedEntry.Name = "radioLastUsedEntry";
            this.radioLastUsedEntry.Size = new System.Drawing.Size(140, 17);
            this.radioLastUsedEntry.TabIndex = 2;
            this.radioLastUsedEntry.TabStop = true;
            this.radioLastUsedEntry.Text = "Return to last used entry";
            this.toolTip1.SetToolTip(this.radioLastUsedEntry, "You will return to the currently selected entry if you leave and come back. If th" +
        "e entry has been remove you will return to last used frequency.");
            this.radioLastUsedEntry.UseVisualStyleBackColor = true;
            this.radioLastUsedEntry.CheckedChanged += new System.EventHandler(this.radioLastUsedEntry_CheckedChanged);
            // 
            // btnUpdateEntry
            // 
            this.btnUpdateEntry.Image = null;
            this.btnUpdateEntry.Location = new System.Drawing.Point(369, 157);
            this.btnUpdateEntry.Name = "btnUpdateEntry";
            this.btnUpdateEntry.Selectable = true;
            this.btnUpdateEntry.Size = new System.Drawing.Size(62, 64);
            this.btnUpdateEntry.TabIndex = 15;
            this.btnUpdateEntry.Text = "Update Entry";
            this.toolTip1.SetToolTip(this.btnUpdateEntry, "Update an existing unlocked entry");
            this.btnUpdateEntry.UseVisualStyleBackColor = true;
            this.btnUpdateEntry.Click += new System.EventHandler(this.btnUpdateEntry_Click);
            // 
            // chkShowHidden
            // 
            this.chkShowHidden.AutoSize = true;
            this.chkShowHidden.Enabled = false;
            this.chkShowHidden.Image = null;
            this.chkShowHidden.Location = new System.Drawing.Point(369, 326);
            this.chkShowHidden.Name = "chkShowHidden";
            this.chkShowHidden.Size = new System.Drawing.Size(88, 17);
            this.chkShowHidden.TabIndex = 14;
            this.chkShowHidden.Text = "Show hidden";
            this.toolTip1.SetToolTip(this.chkShowHidden, "Show any that have been hidden for this filter.");
            this.chkShowHidden.UseVisualStyleBackColor = true;
            this.chkShowHidden.Visible = false;
            // 
            // chkIgnoreDuplicates
            // 
            this.chkIgnoreDuplicates.AutoSize = true;
            this.chkIgnoreDuplicates.Image = null;
            this.chkIgnoreDuplicates.Location = new System.Drawing.Point(369, 236);
            this.chkIgnoreDuplicates.Name = "chkIgnoreDuplicates";
            this.chkIgnoreDuplicates.Size = new System.Drawing.Size(88, 17);
            this.chkIgnoreDuplicates.TabIndex = 16;
            this.chkIgnoreDuplicates.Text = "Ignore dupes";
            this.toolTip1.SetToolTip(this.chkIgnoreDuplicates, "Do not update if the current VFO frequency already exists");
            this.chkIgnoreDuplicates.UseVisualStyleBackColor = true;
            this.chkIgnoreDuplicates.CheckedChanged += new System.EventHandler(this.chkIgnoreDuplicates_CheckedChanged);
            // 
            // chkHideOnSelect
            // 
            this.chkHideOnSelect.AutoSize = true;
            this.chkHideOnSelect.Image = null;
            this.chkHideOnSelect.Location = new System.Drawing.Point(369, 259);
            this.chkHideOnSelect.Name = "chkHideOnSelect";
            this.chkHideOnSelect.Size = new System.Drawing.Size(94, 17);
            this.chkHideOnSelect.TabIndex = 17;
            this.chkHideOnSelect.Text = "Hide on select";
            this.toolTip1.SetToolTip(this.chkHideOnSelect, "Close the band stack window when an item is selected");
            this.chkHideOnSelect.UseVisualStyleBackColor = true;
            this.chkHideOnSelect.CheckedChanged += new System.EventHandler(this.chkHideOnSelect_CheckedChanged);
            // 
            // chkShowInSpectrum
            // 
            this.chkShowInSpectrum.AutoSize = true;
            this.chkShowInSpectrum.Image = null;
            this.chkShowInSpectrum.Location = new System.Drawing.Point(369, 282);
            this.chkShowInSpectrum.Name = "chkShowInSpectrum";
            this.chkShowInSpectrum.Size = new System.Drawing.Size(126, 17);
            this.chkShowInSpectrum.TabIndex = 18;
            this.chkShowInSpectrum.Text = "Show on Panadapter";
            this.toolTip1.SetToolTip(this.chkShowInSpectrum, "Show the entries as overlays on the panadapter. Click them to select.");
            this.chkShowInSpectrum.UseVisualStyleBackColor = true;
            this.chkShowInSpectrum.CheckedChanged += new System.EventHandler(this.chkShowInSpectrum_CheckedChanged);
            // 
            // lblFilterNameCaption
            // 
            this.lblFilterNameCaption.AutoSize = true;
            this.lblFilterNameCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFilterNameCaption.Image = null;
            this.lblFilterNameCaption.Location = new System.Drawing.Point(12, 15);
            this.lblFilterNameCaption.Name = "lblFilterNameCaption";
            this.lblFilterNameCaption.Size = new System.Drawing.Size(42, 16);
            this.lblFilterNameCaption.TabIndex = 1;
            this.lblFilterNameCaption.Text = "Filter :";
            this.lblFilterNameCaption.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // bandStackListBox
            // 
            this.bandStackListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.bandStackListBox.BackColor = System.Drawing.Color.DarkGray;
            this.bandStackListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bandStackListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.bandStackListBox.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bandStackListBox.FormattingEnabled = true;
            this.bandStackListBox.ItemHeight = 19;
            this.bandStackListBox.Location = new System.Drawing.Point(12, 42);
            this.bandStackListBox.LockImageLocked = global::Thetis.Properties.Resources.lock_locked_red;
            this.bandStackListBox.LockImageUnLocked = global::Thetis.Properties.Resources.lock_unlocked_grey;
            this.bandStackListBox.Memory = null;
            this.bandStackListBox.Name = "bandStackListBox";
            this.bandStackListBox.Size = new System.Drawing.Size(216, 344);
            this.bandStackListBox.SpecificReturnImage = global::Thetis.Properties.Resources.return_green;
            this.bandStackListBox.SpecificReturnIndex = -1;
            this.bandStackListBox.TabIndex = 0;
            this.bandStackListBox.SelectedIndexChanged += new System.EventHandler(this.bandStackListBox_SelectedIndexChanged);
            // 
            // frmBandStack2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(496, 401);
            this.Controls.Add(this.chkShowInSpectrum);
            this.Controls.Add(this.chkHideOnSelect);
            this.Controls.Add(this.chkIgnoreDuplicates);
            this.Controls.Add(this.btnUpdateEntry);
            this.Controls.Add(this.chkShowHidden);
            this.Controls.Add(this.btnHideSelected);
            this.Controls.Add(this.chkAlwaysOnTop);
            this.Controls.Add(this.btnAddStackEntry);
            this.Controls.Add(this.lblFilterName);
            this.Controls.Add(this.btnShowBandStackFilterManager);
            this.Controls.Add(this.btnDeleteSelected);
            this.Controls.Add(this.btnLockSelected);
            this.Controls.Add(this.btnOptions);
            this.Controls.Add(this.radioLastUsed);
            this.Controls.Add(this.btnSetSpecific);
            this.Controls.Add(this.radioSpecific);
            this.Controls.Add(this.radioLastUsedEntry);
            this.Controls.Add(this.lblFilterNameCaption);
            this.Controls.Add(this.bandStackListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmBandStack2";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ShowInTaskbar = false;
            this.Text = "Band Stack 2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmBandStack2_FormClosing);
            this.LocationChanged += new System.EventHandler(this.frmBandStack2_LocationChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BandStackListBox bandStackListBox;
        private System.Windows.Forms.LabelTS lblFilterNameCaption;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.RadioButtonTS radioLastUsedEntry;
        private System.Windows.Forms.RadioButtonTS radioSpecific;
        private System.Windows.Forms.ButtonTS btnSetSpecific;
        private System.Windows.Forms.RadioButtonTS radioLastUsed;
        private System.Windows.Forms.ButtonTS btnOptions;
        private System.Windows.Forms.ButtonTS btnLockSelected;
        private System.Windows.Forms.ButtonTS btnDeleteSelected;
        private System.Windows.Forms.ButtonTS btnShowBandStackFilterManager;
        private System.Windows.Forms.LabelTS lblFilterName;
        private System.Windows.Forms.ButtonTS btnAddStackEntry;
        private System.Windows.Forms.CheckBoxTS chkAlwaysOnTop;
        private System.Windows.Forms.ButtonTS btnHideSelected;
        private System.Windows.Forms.CheckBoxTS chkShowHidden;
        private System.Windows.Forms.ButtonTS btnUpdateEntry;
        private System.Windows.Forms.CheckBoxTS chkIgnoreDuplicates;
        private System.Windows.Forms.CheckBoxTS chkHideOnSelect;
        private System.Windows.Forms.CheckBoxTS chkShowInSpectrum;
    }
}