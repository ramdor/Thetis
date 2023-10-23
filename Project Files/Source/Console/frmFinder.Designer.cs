namespace Thetis
{
    partial class frmFinder
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
            this.lstResults = new System.Windows.Forms.ListBox();
            this.chkFullDetails = new System.Windows.Forms.CheckBoxTS();
            this.txtSearch = new System.Windows.Forms.TextBoxTS();
            this.SuspendLayout();
            // 
            // lstResults
            // 
            this.lstResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstResults.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lstResults.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstResults.FormattingEnabled = true;
            this.lstResults.IntegralHeight = false;
            this.lstResults.ItemHeight = 60;
            this.lstResults.Location = new System.Drawing.Point(12, 66);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(280, 304);
            this.lstResults.TabIndex = 0;
            this.lstResults.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstResults_DrawItem);
            this.lstResults.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lstResults_MeasureItem);
            this.lstResults.SelectedIndexChanged += new System.EventHandler(this.lstResults_SelectedIndexChanged);
            // 
            // chkFullDetails
            // 
            this.chkFullDetails.AutoSize = true;
            this.chkFullDetails.Image = null;
            this.chkFullDetails.Location = new System.Drawing.Point(12, 8);
            this.chkFullDetails.Name = "chkFullDetails";
            this.chkFullDetails.Size = new System.Drawing.Size(75, 17);
            this.chkFullDetails.TabIndex = 1;
            this.chkFullDetails.Text = "Full details";
            this.chkFullDetails.UseVisualStyleBackColor = true;
            this.chkFullDetails.CheckedChanged += new System.EventHandler(this.chkFullDetails_CheckedChanged);
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSearch.Location = new System.Drawing.Point(12, 31);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(280, 29);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.Text = "Search";
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // frmFinder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 382);
            this.Controls.Add(this.chkFullDetails);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.txtSearch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(320, 400);
            this.Name = "frmFinder";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Finder";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmFinder_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBoxTS txtSearch;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.CheckBoxTS chkFullDetails;
    }
}