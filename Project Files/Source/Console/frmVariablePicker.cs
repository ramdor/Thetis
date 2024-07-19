//=================================================================
// frmVariablePicker.cs - MW0LGE 2024
//=================================================================

using System;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace Thetis
{
    public partial class frmVariablePicker : Form
    {
        private Guid _guid;
        private string _variable;
        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
        public string Variable
        {
            get { return _variable; }
            set { _variable = value; }
        }

        private class clsVariableListItems
        {
            private Guid _guid;
            private string _variable;
            public clsVariableListItems(Guid guid, string variable)
            {
                _guid = guid;
                _variable = variable;
            }

            public Guid Guid
            {
                get { return _guid; }
                set { _guid = value; }
            }
            public string Variable
            {
                get { return _variable; }
                set { _variable = value; }
            }
            public override string ToString()
            {
                return _variable;
            }
        }        
        public frmVariablePicker()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            clsVariableListItems vli = lstVariables.SelectedItem as clsVariableListItems;
            if (vli == null)
            {
                _guid = Guid.Empty;
                _variable = "--DEFAULT--";
                return;
            }

            _guid = vli.Guid;
            _variable = vli.Variable;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _guid = Guid.Empty;
            _variable = "--DEFAULT--";
        }

        public void Init(int variable, Guid g, string current)
        {
            string title = "Variable Picker (" + variable.ToString() + ")";
            this.Text = title;

            lstVariables.Items.Clear();

            int selected = -1;
            int index = lstVariables.Items.Add(new clsVariableListItems(Guid.Empty, "--DEFAULT--"));
            if (g == Guid.Empty/* && current == "--DEFAULT--"*/) selected = index;

            foreach (KeyValuePair<Guid, MultiMeterIO.clsMMIO> kvp in MultiMeterIO.Data)
            {
                MultiMeterIO.clsMMIO mmio = kvp.Value;

                foreach (KeyValuePair<string, object> kvp2 in mmio.Variables())
                {
                    index = lstVariables.Items.Add(new clsVariableListItems(mmio.Guid, kvp2.Key));
                    if (mmio.Guid == g && current == kvp2.Key) selected = index;
                }
            }

            lstVariables.SelectedIndex = selected;
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            _guid = Guid.Empty;
            _variable = "--DEFAULT--";
        }
    }
}
