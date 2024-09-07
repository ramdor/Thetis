using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Thetis
{
    public partial class ucTunestepOptionsGrid : UserControl
    {
        private List<CheckBox> checkboxes;
        private bool _init;
        public event EventHandler checkbox_changed;

        public ucTunestepOptionsGrid()
        {
            _init = false;
            InitializeComponent();
            initialize_checkboxes();
            hook_up_checkbox_events();
        }

        private void initialize_checkboxes()
        {
            checkboxes = pnlButtonBox_tunestep_toggles.Controls
                        .OfType<CheckBox>()
                        .OrderBy(c => int.Parse(c.Name.Split('_').Last()))
                        .ToList();
        }

        public int Bitfield
        {
            get
            {
                int bitfield_value = 0;

                for (int index = 0; index < checkboxes.Count; index++)
                {
                    CheckBox checkbox = checkboxes[index];
                    if (checkbox.Checked)
                    {
                        bitfield_value |= (1 << index);
                    }
                }

                return bitfield_value;
            }
            set
            {
                for (int index = 0; index < checkboxes.Count; index++)
                {
                    CheckBox checkbox = checkboxes[index];
                    bool is_checked = (value & (1 << index)) != 0;
                    checkbox.Checked = is_checked;
                }
            }
        }

        public void Init(List<TuneStep> tune_steps)
        {
            if (_init) return;

            int step_count = tune_steps.Count;

            for (int index = 0; index < checkboxes.Count; index++)
            {
                CheckBox checkbox = checkboxes[index];

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
            foreach (CheckBox checkbox in checkboxes)
            {
                checkbox.CheckedChanged += checkbox_checked_changed;
            }
        }

        private void checkbox_checked_changed(object sender, EventArgs e)
        {
            checkbox_changed?.Invoke(this, EventArgs.Empty);
        }

        public int GetCheckedCount()
        {
            int count = 0;

            foreach (CheckBoxTS checkbox in checkboxes)
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
