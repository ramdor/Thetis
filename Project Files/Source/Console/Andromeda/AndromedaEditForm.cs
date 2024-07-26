//=================================================================
// AndromedaEditForm.cs
//=================================================================
// Thetis is a C# implementation of a Software Defined Radio.
// Copyright (C) 2019  Laurence Barker, G8NJJ
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
// The author can be reached by email at  
//
// laurence@nicklebyhouse.co.uk
//=================================================================

namespace Thetis
{
    using System;
    using System.Data;
    using System.Windows.Forms;

    /// <summary>
    /// User editor for the Andromeda Front Panel data set.
    /// </summary>

    public class AndromedaEditForm : Form
    {
        #region Variable Declaration
        private readonly Console console;
        private Button BtnClose;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private DataGridView EncoderDataGridView;
        private TabPage tabPage2;
        private DataGridView ButtonDataGridView;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private System.ComponentModel.IContainer components = null;
        private DataGridView IndicatorDataGridView;
        private DataGridView MenuDataGridView;
        DataSet UserData;
        DataSet StringData;
        private Button btnSave;
        private Button btnReset;
        int NumEncoders;                    // number of encoder data records
        int NumPushbuttons;                 // number of pushbutton data records
        private Button btnDelete;
        private Button btnInsert;
        int NumMenuItems;                   // number of menu item records (this will be variable)
        private bool FormInitialised = false;
        private Button btnG2Reset;
        private bool IsVisible = false;

        #endregion

        #region Constructor and Destructor
        public AndromedaEditForm(Console c)
        {
            InitializeComponent();
            console = c;
        }

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
        #endregion


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AndromedaEditForm));
            this.BtnClose = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.EncoderDataGridView = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ButtonDataGridView = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.IndicatorDataGridView = new System.Windows.Forms.DataGridView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.MenuDataGridView = new System.Windows.Forms.DataGridView();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnG2Reset = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EncoderDataGridView)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonDataGridView)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IndicatorDataGridView)).BeginInit();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MenuDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnClose
            // 
            this.BtnClose.Location = new System.Drawing.Point(390, 348);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(98, 38);
            this.BtnClose.TabIndex = 0;
            this.BtnClose.Text = "Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(780, 330);
            this.tabControl1.TabIndex = 1;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.EncoderDataGridView);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(772, 301);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Encoders";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // EncoderDataGridView
            // 
            this.EncoderDataGridView.AllowUserToAddRows = false;
            this.EncoderDataGridView.AllowUserToDeleteRows = false;
            this.EncoderDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.EncoderDataGridView.Location = new System.Drawing.Point(0, 0);
            this.EncoderDataGridView.Name = "EncoderDataGridView";
            this.EncoderDataGridView.RowHeadersWidth = 60;
            this.EncoderDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.EncoderDataGridView.Size = new System.Drawing.Size(769, 302);
            this.EncoderDataGridView.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.ButtonDataGridView);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(772, 301);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Pushbuttons";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // ButtonDataGridView
            // 
            this.ButtonDataGridView.AllowUserToAddRows = false;
            this.ButtonDataGridView.AllowUserToDeleteRows = false;
            this.ButtonDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ButtonDataGridView.Location = new System.Drawing.Point(0, 0);
            this.ButtonDataGridView.Name = "ButtonDataGridView";
            this.ButtonDataGridView.RowHeadersWidth = 60;
            this.ButtonDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ButtonDataGridView.Size = new System.Drawing.Size(769, 310);
            this.ButtonDataGridView.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.IndicatorDataGridView);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(772, 301);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Indicators";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // IndicatorDataGridView
            // 
            this.IndicatorDataGridView.AllowUserToAddRows = false;
            this.IndicatorDataGridView.AllowUserToDeleteRows = false;
            this.IndicatorDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.IndicatorDataGridView.Location = new System.Drawing.Point(0, 0);
            this.IndicatorDataGridView.Name = "IndicatorDataGridView";
            this.IndicatorDataGridView.RowHeadersWidth = 60;
            this.IndicatorDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.IndicatorDataGridView.Size = new System.Drawing.Size(769, 301);
            this.IndicatorDataGridView.TabIndex = 0;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.MenuDataGridView);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(772, 301);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Menus";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // MenuDataGridView
            // 
            this.MenuDataGridView.AllowUserToAddRows = false;
            this.MenuDataGridView.AllowUserToDeleteRows = false;
            this.MenuDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.MenuDataGridView.Location = new System.Drawing.Point(0, 0);
            this.MenuDataGridView.Name = "MenuDataGridView";
            this.MenuDataGridView.RowHeadersWidth = 60;
            this.MenuDataGridView.Size = new System.Drawing.Size(769, 301);
            this.MenuDataGridView.TabIndex = 0;
            this.MenuDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.MenuDataGridView_CellValueChanged);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(682, 348);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(86, 38);
            this.btnDelete.TabIndex = 5;
            this.btnDelete.Text = "Delete Menu";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Visible = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnInsert
            // 
            this.btnInsert.Location = new System.Drawing.Point(570, 348);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(86, 38);
            this.btnInsert.TabIndex = 4;
            this.btnInsert.Text = "Insert Menu";
            this.btnInsert.UseVisualStyleBackColor = true;
            this.btnInsert.Visible = false;
            this.btnInsert.Click += new System.EventHandler(this.BtnInsert_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(273, 348);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 38);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(33, 348);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(114, 38);
            this.btnReset.TabIndex = 3;
            this.btnReset.Text = "Reset Andromeda Data";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.BtnReset_Click);
            // 
            // btnG2Reset
            // 
            this.btnG2Reset.Location = new System.Drawing.Point(161, 348);
            this.btnG2Reset.Name = "btnG2Reset";
            this.btnG2Reset.Size = new System.Drawing.Size(96, 38);
            this.btnG2Reset.TabIndex = 6;
            this.btnG2Reset.Text = "Reset G2 Panel Data";
            this.btnG2Reset.UseVisualStyleBackColor = true;
            this.btnG2Reset.Click += new System.EventHandler(this.btnG2Reset_Click);
            // 
            // AndromedaEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 392);
            this.Controls.Add(this.btnG2Reset);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnInsert);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.BtnClose);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AndromedaEditForm";
            this.Text = "Andromeda Settings Editor";
            this.Activated += new System.EventHandler(this.AndromedaEditForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AndromedaEditForm_FormClosing);
            this.Load += new System.EventHandler(this.AndromedaEditForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.EncoderDataGridView)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ButtonDataGridView)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IndicatorDataGridView)).EndInit();
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MenuDataGridView)).EndInit();
            this.ResumeLayout(false);

        }



        #endregion

        #region properties

        public bool AndromedaEditorVisible
        {
            get { return IsVisible; }
        }

        #endregion

        #region Event Handlers


        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        private void AndromedaEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            IsVisible = false;
            e.Cancel = true;
//            Common.SaveForm(this, "AndromedaEditForm");
        }

        private void AndromedaEditForm_Activated(object sender, EventArgs e)
        {
            UserData = console.AndromedaSettings;
            StringData = console.AndromedaComboStrings;
            if (!FormInitialised)
            {
                // for encoder view: add new combo columns
                EncoderDataGridView.AutoGenerateColumns = false;
                EncoderDataGridView.DataSource = UserData.Tables[2];
                EncoderDataGridView.TopLeftHeaderCell.Value = "Encoder";
                NumEncoders = UserData.Tables[2].Rows.Count;
                DataGridViewComboBoxColumn ColumnEncoderAction = new DataGridViewComboBoxColumn
                {
                    DataPropertyName = "Encoder Action",
                    HeaderText = "Encoder Action",
                    Width = 200,
                    DataSource = StringData.Tables["Encoder Combo Strings"],
                    ValueMember = "ActionId",
                    DisplayMember = "ActionString",
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
                };
                EncoderDataGridView.Columns.Add(ColumnEncoderAction);

                DataGridViewComboBoxColumn ColumnEncoderRXOverride = new DataGridViewComboBoxColumn
                {
                    DataPropertyName = "Encoder RX Selector",
                    HeaderText = "Selected RX",
                    Width = 160,
                    DataSource = StringData.Tables["Encoder RX Override Strings"],
                    ValueMember = "OvrId",
                    DisplayMember = "OvrString",
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
                };
                EncoderDataGridView.Columns.Add(ColumnEncoderRXOverride);
                DisplayRowNumbers(EncoderDataGridView);


                // for pushbutton view: add new combo columns
                ButtonDataGridView.AutoGenerateColumns = false;
                ButtonDataGridView.DataSource = UserData.Tables[1];
                ButtonDataGridView.TopLeftHeaderCell.Value = "Button";
                NumPushbuttons = UserData.Tables[1].Rows.Count;
                DataGridViewComboBoxColumn ColumnButtonAction = new DataGridViewComboBoxColumn
                {
                    DataPropertyName = "Pushbutton Action",
                    HeaderText = "Pushbutton Action",
                    Width = 240,
                    DataSource = StringData.Tables["Pushbutton Combo Strings"],
                    ValueMember = "ActionId",
                    DisplayMember = "ActionString",
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
                };
                ButtonDataGridView.Columns.Add(ColumnButtonAction);

                DataGridViewComboBoxColumn ColumnButtonRXOverride = new DataGridViewComboBoxColumn
                {
                    DataPropertyName = "Pushbutton RX Selector",
                    HeaderText = "Selected RX",
                    Width = 160,
                    DataSource = StringData.Tables["Pushbutton RX Override Strings"],           // reused the same table
                    ValueMember = "OvrId",
                    DisplayMember = "OvrString",
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
                };
                ButtonDataGridView.Columns.Add(ColumnButtonRXOverride);
                DisplayRowNumbers(ButtonDataGridView);

                // for indicator view: add new combo columns
                IndicatorDataGridView.AutoGenerateColumns = false;
                IndicatorDataGridView.DataSource = UserData.Tables[0];
                IndicatorDataGridView.TopLeftHeaderCell.Value = "Indicator";
                DataGridViewComboBoxColumn ColumnIndicatorAction = new DataGridViewComboBoxColumn
                {
                    DataPropertyName = "Indicator Action",
                    HeaderText = "Indicators shows:",
                    Width = 200,
                    DataSource = StringData.Tables["Indicator Combo Strings"],
                    ValueMember = "ActionId",
                    DisplayMember = "ActionString",
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
                };
                IndicatorDataGridView.Columns.Add(ColumnIndicatorAction);

                DataGridViewComboBoxColumn ColumnIndicatorRXOverride = new DataGridViewComboBoxColumn
                {
                    DataPropertyName = "Indicator RX Selector",
                    HeaderText = "Selected RX",
                    Width = 200,
                    DataSource = StringData.Tables["Indicator RX Override Strings"],
                    ValueMember = "OvrId",
                    DisplayMember = "OvrString",
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
                };
                IndicatorDataGridView.Columns.Add(ColumnIndicatorRXOverride);
                DisplayRowNumbers(IndicatorDataGridView);

                // for menu view: create new columns
                MenuDataGridView.AutoGenerateColumns = false;
                MenuDataGridView.DataSource = UserData.Tables[4];
                NumMenuItems = UserData.Tables[4].Rows.Count;
                MenuDataGridView.TopLeftHeaderCell.Value = "Menu";
                DataGridViewComboBoxColumn ColumnMenuAction = new DataGridViewComboBoxColumn
                {
                    DataPropertyName = "Menu Action",
                    HeaderText = "Menu button action",
                    Width = 250,
                    DataSource = StringData.Tables["Pushbutton Combo Strings"],
                    ValueMember = "ActionId",
                    DisplayMember = "ActionString",
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
                };
                MenuDataGridView.Columns.Add(ColumnMenuAction);

                DataGridViewTextBoxColumn ColumnMenuText = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Menu Text",
                    HeaderText = "Button Text:",
                    Width = 220
                };
                MenuDataGridView.Columns.Add(ColumnMenuText);

                DataGridViewComboBoxColumn ColumnMenuRXOverride = new DataGridViewComboBoxColumn
                {
                    DataPropertyName = "Menu RX Selector",
                    HeaderText = "Selected RX",
                    Width = 120,
                    DataSource = StringData.Tables["Encoder RX Override Strings"],           // reused the same table
                    ValueMember = "OvrId",
                    DisplayMember = "OvrString",
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
                };
                MenuDataGridView.Columns.Add(ColumnMenuRXOverride);

                DataGridViewTextBoxColumn ColumnMenuLink = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Menu Number",
                    HeaderText = "Link to Menu:",
                    Width = 80
                };
                MenuDataGridView.Columns.Add(ColumnMenuLink);
                DisplayMenuNumbers(MenuDataGridView);
            }
            FormInitialised = true;
            IsVisible = true;
        }

        // this displays the row numbers in the row headers to show encoder/button/indicator number
        private void DisplayRowNumbers(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.HeaderCell.Value = (row.Index + 1).ToString();
            }
        }

        // this displays the menu numbers in the row headers for a menu
        private void DisplayMenuNumbers(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.HeaderCell.Value = ((row.Index/8) + 1).ToString();
            }
        }


        // function to select an encoder row. Used to allow an encoder turn to highlight its data.
        // parameter is encoder number (0-19)
        public void SetEncoderNumber(int Encoder)
        {
            if(Encoder < NumEncoders)
                EncoderDataGridView.CurrentCell = EncoderDataGridView[1, Encoder];
        }


        // function to select a pushbutton row. Used to allow a button press to highlight its data.
        // parameter is button number (0-49)
        public void SetPushbuttonNumber(int Button)
        {
            if (Button < NumPushbuttons)
                ButtonDataGridView.CurrentCell = ButtonDataGridView[1, Button];
        }


        private void BtnSave_Click(object sender, EventArgs e)
        {
            console.AndromedaSettings = UserData;
        }


        private void BtnReset_Click(object sender, EventArgs e)
        {
            console.ResetAndromedaDataset();
            AndromedaEditForm_Activated(null, null);
        }


        // I don't know why but we have to set the row numbers when the tab is selected
        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int Index = tabControl1.SelectedIndex;
            switch(Index)                                       // switch based on tab number
            {
                case 0:                 // encoders
                    DisplayRowNumbers(EncoderDataGridView);
                    btnInsert.Visible = false;
                    btnDelete.Visible = false;
                    break;

                case 1:                 // pushbuttons
                    DisplayRowNumbers(ButtonDataGridView);
                    btnInsert.Visible = false;
                    btnDelete.Visible = false;
                    break;

                case 2:                 // indicators
                    DisplayRowNumbers(IndicatorDataGridView);
                    btnInsert.Visible = false;
                    btnDelete.Visible = false;
                    break;

                case 3:                 // menus
                    DisplayMenuNumbers(MenuDataGridView);
                    btnInsert.Visible = true;
                    btnDelete.Visible = true;
                    break;

            }

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int MenuNumber;                     // number of menu being deleted
            int Index;                          // current pointer position in table
            int StartRow;                       // start row number to delete
            int Cntr;
            int CellCount;                      // number of rows in table
            int NewMenuCount;                   // number of menus after delete
            int MenuLink;

            Index = MenuDataGridView.CurrentRow.Index;
            MenuNumber = (Index / 8) + 1;
            StartRow = (MenuNumber - 1) * 8;
            // delete 8 rows, starting at the start of the menu
            for (Cntr = 0; Cntr < 8; Cntr++)
                UserData.Tables[4].Rows[StartRow].Delete();
            // now try to renumber the links to other menus
            CellCount = UserData.Tables[4].Rows.Count;
            NewMenuCount = CellCount / 8;
            for (Cntr=0; Cntr < CellCount; Cntr++)
            {
                MenuLink = (int)UserData.Tables[4].Rows[Cntr]["Menu Number"];
                if (MenuLink > NewMenuCount)
                    UserData.Tables[4].Rows[Cntr]["Menu Number"] = 1;                   // if pointing past end of menus
                else if (MenuLink > MenuNumber)
                    UserData.Tables[4].Rows[Cntr]["Menu Number"] = MenuLink - 1;        // if past where we deleted, decrement
            }
            MenuDataGridView.Refresh();
            DisplayMenuNumbers(MenuDataGridView);
        }

        // this is called when a cell value is changed in the menu window
        // if it is the combo box column, also retrieve text.
        private void MenuDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int Row;
            int MenuEnumValue;
            int Column = e.ColumnIndex;
            int CombooValuesRow;                    // row number in the combo box possible values
            int ComboValuesCount;
            int Cntr;

            if(Column == 0)
            {
                Row = e.RowIndex;
                MenuEnumValue = (int)MenuDataGridView.Rows[Row].Cells[Column].Value;
                ComboValuesCount = StringData.Tables["Pushbutton Combo Strings"].Rows.Count;
                for(Cntr=0; Cntr < ComboValuesCount; Cntr++)
                {
                    if ((int)StringData.Tables["Pushbutton Combo Strings"].Rows[Cntr]["ActionId"] == MenuEnumValue)
                        UserData.Tables[4].Rows[Row]["Menu Text"] = StringData.Tables["Pushbutton Combo Strings"].Rows[Cntr]["MenuText"];

                }
            }
        }

        // if inserting at the end, simply add new rows; otherwise they will need to be inserted
        private void BtnInsert_Click(object sender, EventArgs e)
        {
            int MenuNumber;                     // number of menu being deleted
            int Index;                          // current pointer position in table
            int StartRow;                       // start row number to delete
            int Cntr;
            int CellCount;                      // number of rows in table
            int NewMenuCount;                      // number of menus after delete
            int MenuLink;
            int CurrentRowCount;
            DataRow NewRow;

            CurrentRowCount = MenuLink = (int)UserData.Tables[4].Rows.Count;
            Index = MenuDataGridView.CurrentRow.Index;
            MenuNumber = (Index / 8) + 1;
            StartRow = (MenuNumber - 1) * 8;
            if (Index - StartRow >= 4)
                StartRow += 8;
            // if we are inserting inside the table, use insert rows;
            // otherwise use add to add them at the end
            if (StartRow < CurrentRowCount)
            {
                for(Cntr=0; Cntr < 8; Cntr++)
                {
                    NewRow = UserData.Tables[4].NewRow();
                    NewRow["Menu button Number"] = 0;
                    NewRow["Menu Action"] = 0;
                    NewRow["Menu Text"] = "---";
                    NewRow["Menu RX Selector"] = 0;
                    NewRow["Menu Number"] = 0;
                    UserData.Tables[4].Rows.InsertAt(NewRow, StartRow); ;
                }
                // now move up links to higher menus by one
                for (Cntr = 0; Cntr < CurrentRowCount; Cntr++)
                {
                    MenuLink = (int)UserData.Tables[4].Rows[Cntr]["Menu Number"];
                    if (MenuLink >= MenuNumber)                                            // if pact the end, link back to start menu
                        UserData.Tables[4].Rows[Cntr]["Menu Number"] = MenuLink + 1;
                }
            }
            else
            {
                UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);             // numerical paramter only for "menu" entries - points to next
                UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
                UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
                UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
                UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
                UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
                UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
                UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
            }
            MenuDataGridView.Refresh();
            DisplayMenuNumbers(MenuDataGridView);
        }

        private void AndromedaEditForm_Load(object sender, EventArgs e)
        {

        }

        private void btnG2Reset_Click(object sender, EventArgs e)
        {
            console.ResetG2PanelDataset();
            AndromedaEditForm_Activated(null, null);
        }
    }
}
