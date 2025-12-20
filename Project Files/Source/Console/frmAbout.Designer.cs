namespace Thetis
{
    partial class frmAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
            this.lstContributors = new System.Windows.Forms.ListBox();
            this.lstVersions = new System.Windows.Forms.ListBox();
            this.lnkLicence = new System.Windows.Forms.LinkLabel();
            this.lstLinks = new System.Windows.Forms.ListBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnReleaseNotes = new System.Windows.Forms.ButtonTS();
            this.btnUpdatedRelease = new System.Windows.Forms.ButtonTS();
            this.btnVisit = new System.Windows.Forms.ButtonTS();
            this.btnDXDiag = new System.Windows.Forms.ButtonTS();
            this.btnSysInfo = new System.Windows.Forms.ButtonTS();
            this.btnCopyContributors = new System.Windows.Forms.ButtonTS();
            this.labelTS4 = new System.Windows.Forms.LabelTS();
            this.labelTS3 = new System.Windows.Forms.LabelTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.btnOK = new System.Windows.Forms.ButtonTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.SuspendLayout();
            // 
            // lstContributors
            // 
            this.lstContributors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstContributors.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstContributors.FormattingEnabled = true;
            this.lstContributors.ItemHeight = 16;
            this.lstContributors.Items.AddRange(new object[] {
            "NR0V, Warren (WDSP & too many other contributions to list)",
            "G8NJJ, Laurence (G2, Andromeda & protocols)",
            "N1GP, Rick (Firmware related changes)",
            "W4WMT, Bryan (Resampler, VAC & cmASIO)",
            "MI0BOT, Reid (Hermes Lite 2)",
            "MW0LGE, Richie (UI & various)",
            "W5WC, Doug (UI, ChannelMaster, various & Thetis naming)",
            "W2PA, Chris (QSK & MIDI)",
            "WD5Y, Joe (UI tweaks and fixes)",
            "M0YGG, Andrew (MIDI & various)",
            "",
            "VK6PH, Phil (Firmware, Protocols & other)",
            "KD5TFD, Bill (Protocol 1 initial implementation, UI & various)",
            "K5SO, Joe (Diversity Reception & firmware)",
            "WA8YWQ, Dave (various)",
            "KE9NS, Darrin (spot & various)",
            "",
            "WU2O, Scott (forum admin & ideas/feedback)",
            "NC3Z, Gary (forum mod)",
            "OE3IDE, Ernst (skins & primary tester)",
            "W1AEX, Rob (skins & audio information)",
            "DH1KLM, Sigi (midi, skins & UI improvements)",
            "",
            "and indirectly, all the testers"});
            this.lstContributors.Location = new System.Drawing.Point(19, 267);
            this.lstContributors.Name = "lstContributors";
            this.lstContributors.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lstContributors.Size = new System.Drawing.Size(490, 162);
            this.lstContributors.TabIndex = 0;
            // 
            // lstVersions
            // 
            this.lstVersions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstVersions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstVersions.FormattingEnabled = true;
            this.lstVersions.ItemHeight = 16;
            this.lstVersions.Items.AddRange(new object[] {
            "Version : 2.10.3.6 WWWWWWWWWWWWWWWW",
            "Database : 2.10.3",
            "Radio Model : ANAN7000",
            "Andromeda Info:",
            "Firmware Version : ?",
            "Protocol : 2 (v1.1)",
            "WDSP Version :",
            "ChannelMaster Version :",
            "cmASIO Version :",
            "PortAudio Version :",
            "DirectX Version: 12.1"});
            this.lstVersions.Location = new System.Drawing.Point(19, 54);
            this.lstVersions.Name = "lstVersions";
            this.lstVersions.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lstVersions.Size = new System.Drawing.Size(317, 176);
            this.lstVersions.TabIndex = 3;
            // 
            // lnkLicence
            // 
            this.lnkLicence.AutoSize = true;
            this.lnkLicence.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkLicence.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkLicence.Location = new System.Drawing.Point(297, 31);
            this.lnkLicence.Name = "lnkLicence";
            this.lnkLicence.Size = new System.Drawing.Size(96, 16);
            this.lnkLicence.TabIndex = 5;
            this.lnkLicence.TabStop = true;
            this.lnkLicence.Text = "License Terms";
            this.lnkLicence.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkLicence_LinkClicked);
            // 
            // lstLinks
            // 
            this.lstLinks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstLinks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstLinks.FormattingEnabled = true;
            this.lstLinks.ItemHeight = 16;
            this.lstLinks.Items.AddRange(new object[] {
            "Thetis Releases on GitHub",
            "Join Thetis Discord Server",
            "Apache Labs Community Forums",
            "Apache Labs Home Page",
            "Protocol1 Programmers on GitHub",
            "Protocol2 Programmers on GitHub",
            "Firmware Discussions (all)",
            "Protocol1 Firmware (7000/8000)",
            "Protocol2 Firmware (7000/8000) RF fix",
            "G2 Firmware & Software \"p2app\"",
            "Thetis for Hermes Lite 2 on GitHub",
            "WDSP Documentation on GitHub",
            "OE3IDE\'s (Ernst) Connectors & Tools",
            "---MANUALS---",
            "Thetis",
            "Cat Command Reference",
            "PureSignal",
            "Midi",
            "cmASIO",
            "Behringer",
            "APF Types"});
            this.lstLinks.Location = new System.Drawing.Point(345, 108);
            this.lstLinks.Name = "lstLinks";
            this.lstLinks.Size = new System.Drawing.Size(267, 130);
            this.lstLinks.TabIndex = 9;
            this.lstLinks.SelectedIndexChanged += new System.EventHandler(this.lstLinks_SelectedIndexChanged);
            // 
            // btnReleaseNotes
            // 
            this.btnReleaseNotes.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnReleaseNotes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReleaseNotes.Image = null;
            this.btnReleaseNotes.Location = new System.Drawing.Point(524, 388);
            this.btnReleaseNotes.Name = "btnReleaseNotes";
            this.btnReleaseNotes.Selectable = true;
            this.btnReleaseNotes.Size = new System.Drawing.Size(88, 41);
            this.btnReleaseNotes.TabIndex = 14;
            this.btnReleaseNotes.Text = "Release\r\nNotes";
            this.toolTip1.SetToolTip(this.btnReleaseNotes, "Show the release notes");
            this.btnReleaseNotes.UseVisualStyleBackColor = false;
            this.btnReleaseNotes.Click += new System.EventHandler(this.btnReleaseNotes_Click);
            // 
            // btnUpdatedRelease
            // 
            this.btnUpdatedRelease.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btnUpdatedRelease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdatedRelease.Image = null;
            this.btnUpdatedRelease.Location = new System.Drawing.Point(416, 8);
            this.btnUpdatedRelease.Name = "btnUpdatedRelease";
            this.btnUpdatedRelease.Selectable = true;
            this.btnUpdatedRelease.Size = new System.Drawing.Size(196, 58);
            this.btnUpdatedRelease.TabIndex = 13;
            this.btnUpdatedRelease.Text = "Version 2.10.3.8\r\nhas been released";
            this.toolTip1.SetToolTip(this.btnUpdatedRelease, "New release available");
            this.btnUpdatedRelease.UseVisualStyleBackColor = false;
            this.btnUpdatedRelease.Click += new System.EventHandler(this.btnUpdatedRelease_Click);
            // 
            // btnVisit
            // 
            this.btnVisit.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnVisit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVisit.Image = null;
            this.btnVisit.Location = new System.Drawing.Point(524, 78);
            this.btnVisit.Name = "btnVisit";
            this.btnVisit.Selectable = true;
            this.btnVisit.Size = new System.Drawing.Size(88, 23);
            this.btnVisit.TabIndex = 10;
            this.btnVisit.Text = "View";
            this.toolTip1.SetToolTip(this.btnVisit, "Visit he selected link");
            this.btnVisit.UseVisualStyleBackColor = false;
            this.btnVisit.Click += new System.EventHandler(this.btnVisit_Click);
            // 
            // btnDXDiag
            // 
            this.btnDXDiag.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnDXDiag.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDXDiag.Image = null;
            this.btnDXDiag.Location = new System.Drawing.Point(524, 325);
            this.btnDXDiag.Name = "btnDXDiag";
            this.btnDXDiag.Selectable = true;
            this.btnDXDiag.Size = new System.Drawing.Size(88, 23);
            this.btnDXDiag.TabIndex = 8;
            this.btnDXDiag.Text = "DxDiag";
            this.toolTip1.SetToolTip(this.btnDXDiag, "Run dxDiag");
            this.btnDXDiag.UseVisualStyleBackColor = false;
            this.btnDXDiag.Click += new System.EventHandler(this.btnDXDiag_Click);
            // 
            // btnSysInfo
            // 
            this.btnSysInfo.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnSysInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSysInfo.Image = null;
            this.btnSysInfo.Location = new System.Drawing.Point(524, 296);
            this.btnSysInfo.Name = "btnSysInfo";
            this.btnSysInfo.Selectable = true;
            this.btnSysInfo.Size = new System.Drawing.Size(88, 23);
            this.btnSysInfo.TabIndex = 7;
            this.btnSysInfo.Text = "System Info";
            this.toolTip1.SetToolTip(this.btnSysInfo, "Show system info");
            this.btnSysInfo.UseVisualStyleBackColor = false;
            this.btnSysInfo.Click += new System.EventHandler(this.btnSysInfo_Click);
            // 
            // btnCopyContributors
            // 
            this.btnCopyContributors.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnCopyContributors.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopyContributors.Image = null;
            this.btnCopyContributors.Location = new System.Drawing.Point(524, 267);
            this.btnCopyContributors.Name = "btnCopyContributors";
            this.btnCopyContributors.Selectable = true;
            this.btnCopyContributors.Size = new System.Drawing.Size(88, 23);
            this.btnCopyContributors.TabIndex = 6;
            this.btnCopyContributors.Text = "Copy Info";
            this.toolTip1.SetToolTip(this.btnCopyContributors, "Copy the version info to the clipboard");
            this.btnCopyContributors.UseVisualStyleBackColor = false;
            this.btnCopyContributors.Click += new System.EventHandler(this.btnCopyContributors_Click);
            // 
            // labelTS4
            // 
            this.labelTS4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTS4.Image = null;
            this.labelTS4.Location = new System.Drawing.Point(16, 441);
            this.labelTS4.Name = "labelTS4";
            this.labelTS4.Size = new System.Drawing.Size(493, 86);
            this.labelTS4.TabIndex = 12;
            this.labelTS4.Text = resources.GetString("labelTS4.Text");
            // 
            // labelTS3
            // 
            this.labelTS3.AutoSize = true;
            this.labelTS3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTS3.Image = null;
            this.labelTS3.Location = new System.Drawing.Point(342, 85);
            this.labelTS3.Name = "labelTS3";
            this.labelTS3.Size = new System.Drawing.Size(102, 16);
            this.labelTS3.TabIndex = 11;
            this.labelTS3.Text = "Links / Manuals:";
            // 
            // labelTS2
            // 
            this.labelTS2.AutoSize = true;
            this.labelTS2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(16, 244);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(81, 16);
            this.labelTS2.TabIndex = 4;
            this.labelTS2.Text = "Contributors:";
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.Image = null;
            this.btnOK.Location = new System.Drawing.Point(524, 499);
            this.btnOK.Name = "btnOK";
            this.btnOK.Selectable = true;
            this.btnOK.Size = new System.Drawing.Size(88, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTS1.ForeColor = System.Drawing.Color.Teal;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(12, 9);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(126, 42);
            this.labelTS1.TabIndex = 1;
            this.labelTS1.Text = "Thetis";
            // 
            // frmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(624, 534);
            this.Controls.Add(this.btnReleaseNotes);
            this.Controls.Add(this.btnUpdatedRelease);
            this.Controls.Add(this.labelTS4);
            this.Controls.Add(this.labelTS3);
            this.Controls.Add(this.btnVisit);
            this.Controls.Add(this.lstLinks);
            this.Controls.Add(this.btnDXDiag);
            this.Controls.Add(this.btnSysInfo);
            this.Controls.Add(this.btnCopyContributors);
            this.Controls.Add(this.lnkLicence);
            this.Controls.Add(this.labelTS2);
            this.Controls.Add(this.lstVersions);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.lstContributors);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAbout";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Thetis";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstContributors;
        private System.Windows.Forms.LabelTS labelTS1;
        private System.Windows.Forms.ButtonTS btnOK;
        private System.Windows.Forms.ListBox lstVersions;
        private System.Windows.Forms.LabelTS labelTS2;
        private System.Windows.Forms.LinkLabel lnkLicence;
        private System.Windows.Forms.ButtonTS btnCopyContributors;
        private System.Windows.Forms.ButtonTS btnSysInfo;
        private System.Windows.Forms.ButtonTS btnDXDiag;
        private System.Windows.Forms.ListBox lstLinks;
        private System.Windows.Forms.ButtonTS btnVisit;
        private System.Windows.Forms.LabelTS labelTS3;
        private System.Windows.Forms.LabelTS labelTS4;
        private System.Windows.Forms.ButtonTS btnUpdatedRelease;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ButtonTS btnReleaseNotes;
    }
}