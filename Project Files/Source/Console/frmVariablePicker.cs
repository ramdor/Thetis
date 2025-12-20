/*  frmVariablePicker.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2025 Richard Samphire MW0LGE

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
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

namespace Thetis
{
    public partial class frmVariablePicker : Form
    {
        private bool _textoverlay_led_picker;
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
            internal enum VariableListItemType
            {
                MMIO=0,
                VARS,
                CAT,

                TEXT_ONLY = 999
            }
            private Guid _guid;
            private string _variable;
            private VariableListItemType _type;

            public clsVariableListItems(Guid guid, string variable, VariableListItemType listItemType)
            {
                _guid = guid;
                _variable = variable;
                _type = listItemType;
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
            public VariableListItemType ListItemType
            {
                get { return _type; }
                set { _type = value; }
            }
            public override string ToString()
            {
                return _variable;
            }
        }        
        public frmVariablePicker()
        {
            _textoverlay_led_picker = false;
            InitializeComponent();

            lstVariables.DrawMode = DrawMode.OwnerDrawFixed;
            Size text_size = TextRenderer.MeasureText("Ag", lstVariables.Font);
            int vertical_pad = 2;
            int item_height = text_size.Height + vertical_pad;
            if (item_height < 20) item_height = 20;
            lstVariables.ItemHeight = item_height;
            lstVariables.DrawItem += list_box_DrawItem;
        }

        private Color colour_for_type(clsVariableListItems.VariableListItemType t)
        {
            switch (t)
            {
                case clsVariableListItems.VariableListItemType.MMIO: return Color.Teal;
                case clsVariableListItems.VariableListItemType.VARS: return Color.Tan;
                case clsVariableListItems.VariableListItemType.CAT: return Color.Gray;
                case clsVariableListItems.VariableListItemType.TEXT_ONLY: return Color.Transparent;
                default: return Color.Gray;
            }
        }

        private void list_box_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            ListBox lb = (ListBox)sender;
            clsVariableListItems item = (clsVariableListItems)lb.Items[e.Index];

            Rectangle rect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 4, 6, e.Bounds.Height - 8);
            Color c = colour_for_type(item.ListItemType);

            if (c.A > 0)
            {
                using (Brush b = new SolidBrush(c)) e.Graphics.FillRectangle(b, rect);
                using (Pen p = new Pen(Color.Black)) e.Graphics.DrawRectangle(p, rect);
            }

            Point text_pos = new Point(rect.Right + 6, e.Bounds.Y + (e.Bounds.Height - e.Font.Height) / 2);
            TextRenderer.DrawText(e.Graphics, item.ToString(), e.Font, text_pos, e.ForeColor, TextFormatFlags.NoPadding);

            e.DrawFocusRectangle();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            clsVariableListItems vli = lstVariables.SelectedItem as clsVariableListItems;
            if (vli == null)
            {
                _guid = Guid.Empty;
                _variable = _textoverlay_led_picker ? "" : "--DEFAULT--";
                return;
            }

            _guid = _textoverlay_led_picker ? Guid.Empty : vli.Guid;
            _variable = vli.Variable;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _guid = Guid.Empty;
            _variable = _textoverlay_led_picker ? "" : "--DEFAULT--";
        }

        public void Init(int variable, Guid g, string current, bool textoverlay_led_picker = false)
        {
            _textoverlay_led_picker = textoverlay_led_picker;
            btnDefault.Visible = !textoverlay_led_picker;

            string title = textoverlay_led_picker ? "Variable Picker to Clipboard" : "Variable Picker (" + variable.ToString() + ")";
            this.Text = title;

            lstVariables.Items.Clear();

            int selected = -1;
            int index;

            if (textoverlay_led_picker)
            {
                index = -1;
                selected = -1;
                g = Guid.Empty;
            }
            else
            {
                index = lstVariables.Items.Add(new clsVariableListItems(Guid.Empty, "--DEFAULT--", clsVariableListItems.VariableListItemType.TEXT_ONLY));
                if (g == Guid.Empty/* && current == "--DEFAULT--"*/) selected = index;
            }
            foreach (KeyValuePair<Guid, MultiMeterIO.clsMMIO> kvp in MultiMeterIO.Data)
            {
                MultiMeterIO.clsMMIO mmio = kvp.Value;

                foreach (KeyValuePair<string, object> kvp2 in mmio.Variables())
                {
                    index = lstVariables.Items.Add(new clsVariableListItems(mmio.Guid, kvp2.Key, clsVariableListItems.VariableListItemType.MMIO));
                    if (mmio.Guid == g && current == kvp2.Key) selected = index;
                }
            }

            if (textoverlay_led_picker)
            {
                // add string variables, readings etc, rx1 is ok, as all the same
                List<string> vars = MeterManager.ReadingsCustom(1).GetAvailableReadings();
                foreach (string var in vars)
                {
                    lstVariables.Items.Add(new clsVariableListItems(Guid.Empty, var, clsVariableListItems.VariableListItemType.VARS));
                }

                // add cat variables
                vars = MeterManager.CatVariables();
                foreach (string var in vars)
                {
                    lstVariables.Items.Add(new clsVariableListItems(Guid.Empty, var.Trim('%'), clsVariableListItems.VariableListItemType.CAT));
                }

                lstVariables.SelectedIndex = 0;
            }
            else 
            { 
                lstVariables.SelectedIndex = selected;
            }
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            _guid = Guid.Empty;
            _variable = "--DEFAULT--";
        }

        private void lstVariables_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            int index = lb.IndexFromPoint(e.Location);
            if (index == ListBox.NoMatches) return;

            clsVariableListItems vbi = lb.Items[index] as clsVariableListItems;
            if (vbi != null)
            {
                btnSelect_Click(this, EventArgs.Empty);
                DialogResult = DialogResult.OK;
            }
        }
    }
}
