/*  frmVariablePicker.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2024 Richard Samphire MW0LGE

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at

mw0lge@grange-lane.co.uk
*/

using System;
using System.Windows.Forms;
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
