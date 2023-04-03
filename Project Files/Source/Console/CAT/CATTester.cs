using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Thetis
{
    /// <summary>
    /// Summary description for tester.
    /// </summary>
    public class CATTester : System.Windows.Forms.Form
    {

        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.TextBoxTS txtInput;
        private System.Windows.Forms.TextBoxTS txtResult;
        private Console console;
        private CATParser parser;
        private System.Windows.Forms.LabelTS label1;
        private System.Windows.Forms.LabelTS label2;
        private DataSet ds;
        private System.Windows.Forms.DataGridView dataGrid1;
        private System.Windows.Forms.Button btnExecute;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public CATTester(Console c)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            console = c;
            parser = new CATParser(console);
            ds = new DataSet();
            Setup();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void Setup()
        {
            try
            {
                ds.ReadXml(Application.StartupPath + "\\CATStructs.xml");
                dataGrid1.DataSource = ds.Tables[0];  //MW0LGE [2.9.0.7] added .Tables[0]          
            }
            catch
            {

                MessageBox.Show(
                    "Issue loading CATStructs.xml",
                    "Database Error: CATStructs", MessageBoxButtons.OK, MessageBoxIcon.Stop,
                    MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }

            txtInput.Focus();
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CATTester));
            this.btnExit = new System.Windows.Forms.Button();
            this.txtInput = new System.Windows.Forms.TextBoxTS();
            this.txtResult = new System.Windows.Forms.TextBoxTS();
            this.label1 = new System.Windows.Forms.LabelTS();
            this.label2 = new System.Windows.Forms.LabelTS();
            this.dataGrid1 = new System.Windows.Forms.DataGridView();
            this.btnExecute = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(593, 336);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(119, 33);
            this.btnExit.TabIndex = 2;
            this.btnExit.Text = "Exit";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // txtInput
            // 
            this.txtInput.Location = new System.Drawing.Point(115, 251);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(168, 20);
            this.txtInput.TabIndex = 0;
            this.txtInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyUp);
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(115, 291);
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(392, 20);
            this.txtResult.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Image = null;
            this.label1.Location = new System.Drawing.Point(11, 251);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "CAT Command";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // label2
            // 
            this.label2.Image = null;
            this.label2.Location = new System.Drawing.Point(11, 291);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "CAT Response";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // dataGrid1
            // 
            this.dataGrid1.Location = new System.Drawing.Point(8, 10);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.Size = new System.Drawing.Size(704, 224);
            this.dataGrid1.TabIndex = 6;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(307, 251);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 7;
            this.btnExecute.Text = "Execute";
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // CATTester
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(724, 381);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.dataGrid1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.btnExit);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(740, 420);
            this.MinimumSize = new System.Drawing.Size(740, 420);
            this.Name = "CATTester";
            this.Text = "CAT Command Tester";
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion


        private void btnExit_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void txtInput_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CheckText();
            }
        }

        private void ExecuteCommand()
        {
            string answer = parser.Get(txtInput.Text);
            txtResult.Text = answer;
            txtInput.Clear();

        }

        private void btnExecute_Click(object sender, System.EventArgs e)
        {
            CheckText();
        }

        private void CheckText()
        {
            if (!txtInput.Text.EndsWith(";"))
                txtInput.Text += ";";
            ExecuteCommand();
        }



    }
}
