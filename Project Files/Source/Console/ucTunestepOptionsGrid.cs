/*  ucTunestepOptionsGrid.cs

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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Thetis
{
    public partial class ucTunestepOptionsGrid : UserControl
    {
        private List<CheckBox> _check_boxes;
        private bool _init;
        public event EventHandler CheckboxChanged;

        public ucTunestepOptionsGrid()
        {
            _init = false;
            InitializeComponent();
            initialize_checkboxes();
            hook_up_checkbox_events();
        }

        private void initialize_checkboxes()
        {
            // this obtains a list of the checkboxes, sorted by the terminating number in the .Name field
            _check_boxes = pnlButtonBox_tunestep_toggles.Controls
                        .OfType<CheckBox>()
                        .OrderBy(c => int.Parse(c.Name.Split('_').Last()))
                        .ToList();
        }

        public int Bitfield
        {
            get
            {
                int bitfield_value = 0;

                for (int index = 0; index < _check_boxes.Count; index++)
                {
                    CheckBox checkbox = _check_boxes[index];
                    if (checkbox.Checked)
                    {
                        bitfield_value |= (1 << index);
                    }
                }

                return bitfield_value;
            }
            set
            {
                for (int index = 0; index < _check_boxes.Count; index++)
                {
                    CheckBox checkbox = _check_boxes[index];
                    bool is_checked = (value & (1 << index)) != 0;
                    checkbox.Checked = is_checked;
                }
            }
        }

        public void Init(List<TuneStep> tune_steps)
        {
            if (_init) return;

            int step_count = tune_steps.Count;

            for (int index = 0; index < _check_boxes.Count; index++)
            {
                CheckBox checkbox = _check_boxes[index];

                if (index < step_count)
                {
                    checkbox.Text = tune_steps[index].Name.Replace("Hz", "");
                    checkbox.Visible = true;
                }
                else
                {
                    checkbox.Visible = false;
                }
            }
            _init = true;
        }

        private void hook_up_checkbox_events()
        {
            foreach (CheckBox checkbox in _check_boxes)
            {
                checkbox.CheckedChanged += checkbox_checked_changed;
            }
        }

        private void checkbox_checked_changed(object sender, EventArgs e)
        {
            CheckboxChanged?.Invoke(this, EventArgs.Empty);
        }

        public int GetCheckedCount()
        {
            int count = 0;

            foreach (CheckBoxTS checkbox in _check_boxes)
            {
                if (checkbox.Checked)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
