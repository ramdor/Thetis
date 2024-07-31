namespace Thetis
{
    partial class frmSerialPortPicker
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
            this.labelTS5 = new System.Windows.Forms.LabelTS();
            this.labelTS4 = new System.Windows.Forms.LabelTS();
            this.labelTS3 = new System.Windows.Forms.LabelTS();
            this.labelTS2 = new System.Windows.Forms.LabelTS();
            this.labelTS1 = new System.Windows.Forms.LabelTS();
            this.comboParity = new System.Windows.Forms.ComboBoxTS();
            this.comboStopBits = new System.Windows.Forms.ComboBoxTS();
            this.comboDataBits = new System.Windows.Forms.ComboBoxTS();
            this.comboBaudRate = new System.Windows.Forms.ComboBoxTS();
            this.comboComPort = new System.Windows.Forms.ComboBoxTS();
            this.btnCancel = new System.Windows.Forms.ButtonTS();
            this.btnSelect = new System.Windows.Forms.ButtonTS();
            this.SuspendLayout();
            // 
            // labelTS5
            // 
            this.labelTS5.AutoSize = true;
            this.labelTS5.Image = null;
            this.labelTS5.Location = new System.Drawing.Point(37, 133);
            this.labelTS5.Name = "labelTS5";
            this.labelTS5.Size = new System.Drawing.Size(36, 13);
            this.labelTS5.TabIndex = 16;
            this.labelTS5.Text = "Parity:";
            // 
            // labelTS4
            // 
            this.labelTS4.AutoSize = true;
            this.labelTS4.Image = null;
            this.labelTS4.Location = new System.Drawing.Point(21, 106);
            this.labelTS4.Name = "labelTS4";
            this.labelTS4.Size = new System.Drawing.Size(52, 13);
            this.labelTS4.TabIndex = 15;
            this.labelTS4.Text = "Stop Bits:";
            // 
            // labelTS3
            // 
            this.labelTS3.AutoSize = true;
            this.labelTS3.Image = null;
            this.labelTS3.Location = new System.Drawing.Point(20, 79);
            this.labelTS3.Name = "labelTS3";
            this.labelTS3.Size = new System.Drawing.Size(53, 13);
            this.labelTS3.TabIndex = 14;
            this.labelTS3.Text = "Data Bits:";
            // 
            // labelTS2
            // 
            this.labelTS2.AutoSize = true;
            this.labelTS2.Image = null;
            this.labelTS2.Location = new System.Drawing.Point(12, 52);
            this.labelTS2.Name = "labelTS2";
            this.labelTS2.Size = new System.Drawing.Size(61, 13);
            this.labelTS2.TabIndex = 13;
            this.labelTS2.Text = "Baud Rate:";
            // 
            // labelTS1
            // 
            this.labelTS1.AutoSize = true;
            this.labelTS1.Image = null;
            this.labelTS1.Location = new System.Drawing.Point(20, 25);
            this.labelTS1.Name = "labelTS1";
            this.labelTS1.Size = new System.Drawing.Size(53, 13);
            this.labelTS1.TabIndex = 12;
            this.labelTS1.Text = "Com Port:";
            // 
            // comboParity
            // 
            this.comboParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboParity.FormattingEnabled = true;
            this.comboParity.Location = new System.Drawing.Point(79, 130);
            this.comboParity.Name = "comboParity";
            this.comboParity.Size = new System.Drawing.Size(121, 21);
            this.comboParity.TabIndex = 11;
            this.comboParity.SelectedIndexChanged += new System.EventHandler(this.comboParity_SelectedIndexChanged);
            // 
            // comboStopBits
            // 
            this.comboStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboStopBits.FormattingEnabled = true;
            this.comboStopBits.Location = new System.Drawing.Point(79, 103);
            this.comboStopBits.Name = "comboStopBits";
            this.comboStopBits.Size = new System.Drawing.Size(121, 21);
            this.comboStopBits.TabIndex = 10;
            this.comboStopBits.SelectedIndexChanged += new System.EventHandler(this.comboStopBits_SelectedIndexChanged);
            // 
            // comboDataBits
            // 
            this.comboDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDataBits.FormattingEnabled = true;
            this.comboDataBits.Location = new System.Drawing.Point(79, 76);
            this.comboDataBits.Name = "comboDataBits";
            this.comboDataBits.Size = new System.Drawing.Size(121, 21);
            this.comboDataBits.TabIndex = 9;
            this.comboDataBits.SelectedIndexChanged += new System.EventHandler(this.comboDataBits_SelectedIndexChanged);
            // 
            // comboBaudRate
            // 
            this.comboBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBaudRate.FormattingEnabled = true;
            this.comboBaudRate.Location = new System.Drawing.Point(79, 49);
            this.comboBaudRate.Name = "comboBaudRate";
            this.comboBaudRate.Size = new System.Drawing.Size(121, 21);
            this.comboBaudRate.TabIndex = 8;
            this.comboBaudRate.SelectedIndexChanged += new System.EventHandler(this.comboBaudRate_SelectedIndexChanged);
            // 
            // comboComPort
            // 
            this.comboComPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboComPort.FormattingEnabled = true;
            this.comboComPort.Location = new System.Drawing.Point(79, 22);
            this.comboComPort.Name = "comboComPort";
            this.comboComPort.Size = new System.Drawing.Size(121, 21);
            this.comboComPort.TabIndex = 7;
            this.comboComPort.SelectedIndexChanged += new System.EventHandler(this.comboComPort_SelectedIndexChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = null;
            this.btnCancel.Location = new System.Drawing.Point(175, 180);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Selectable = true;
            this.btnCancel.Size = new System.Drawing.Size(52, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Image = null;
            this.btnSelect.Location = new System.Drawing.Point(117, 180);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Selectable = true;
            this.btnSelect.Size = new System.Drawing.Size(52, 23);
            this.btnSelect.TabIndex = 5;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // frmSerialPortPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(234, 211);
            this.Controls.Add(this.labelTS5);
            this.Controls.Add(this.labelTS4);
            this.Controls.Add(this.labelTS3);
            this.Controls.Add(this.labelTS2);
            this.Controls.Add(this.labelTS1);
            this.Controls.Add(this.comboParity);
            this.Controls.Add(this.comboStopBits);
            this.Controls.Add(this.comboDataBits);
            this.Controls.Add(this.comboBaudRate);
            this.Controls.Add(this.comboComPort);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximumSize = new System.Drawing.Size(250, 320);
            this.MinimumSize = new System.Drawing.Size(250, 100);
            this.Name = "frmSerialPortPicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Serial Port Picker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ButtonTS btnCancel;
        private System.Windows.Forms.ButtonTS btnSelect;
        private System.Windows.Forms.ComboBoxTS comboComPort;
        private System.Windows.Forms.ComboBoxTS comboBaudRate;
        private System.Windows.Forms.ComboBoxTS comboDataBits;
        private System.Windows.Forms.ComboBoxTS comboStopBits;
        private System.Windows.Forms.ComboBoxTS comboParity;
        private System.Windows.Forms.LabelTS labelTS1;
        private System.Windows.Forms.LabelTS labelTS2;
        private System.Windows.Forms.LabelTS labelTS3;
        private System.Windows.Forms.LabelTS labelTS4;
        private System.Windows.Forms.LabelTS labelTS5;
    }
}