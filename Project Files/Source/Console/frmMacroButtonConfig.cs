/*  frmMacroButtonConfig.cs

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
using System.Collections.Generic;
using System.Windows.Forms;
using CatAtonic;

namespace Thetis
{
    public partial class frmMacroButtonConfig : Form
    {
        private class clsContainerComboboxItem
        {
            public string Text { get; set; }
            public string ID { get; set; }
            public int ListIndex { get; set; }

            public override string ToString()
            {
                return ListIndex.ToString() + " - " + Text;
            }
        }

        //private bool _init;
        private string _base_title;
        private OtherButtonMacroSettings _settings;
        private CATScriptInterpreter _si;
        private CATTester _cat_tester;
        private Console _console;

        public frmMacroButtonConfig(CATScriptInterpreter si)
        {
            InitializeComponent();

            _console = null;
            _cat_tester = null;
            _base_title = this.Text;
            picError.Visible = false;
            lblErrorText.Visible = false;

            _settings = null;
            _si = si;
            //_init = true;
        }

        public DialogResult InitAndShow(OtherButtonMacroSettings settings, Dictionary<string, string> containers, ref OtherButtonMacroSettings working_set, Console c)
        {
            //_init = true;
            _console = c;
            _settings = working_set;

            this.Text = _base_title + $" - Macro {_settings.Number + 1}";

            //
            txtON.Text = _settings.OnText;
            txtOFF.Text = _settings.OffText;
            txtNotes.Text = _settings.Notes;
            chkClosesParent.Checked = _settings.ClosesParent;

            chkClosesContainer_1.Checked = _settings.ClosesContainer[0];
            chkClosesContainer_2.Checked = _settings.ClosesContainer[1];
            chkClosesContainer_3.Checked = _settings.ClosesContainer[2];
            chkClosesContainer_4.Checked = _settings.ClosesContainer[3];

            chkOpensContainer_1.Checked = _settings.OpensContainer[0];
            chkOpensContainer_2.Checked = _settings.OpensContainer[1];
            chkOpensContainer_3.Checked = _settings.OpensContainer[2];
            chkOpensContainer_4.Checked = _settings.OpensContainer[3];

            chkUseParent_loc_size_1.Checked = _settings.OpenUsesLocation[0];
            chkUseParent_loc_size_2.Checked = _settings.OpenUsesLocation[1];
            chkUseParent_loc_size_3.Checked = _settings.OpenUsesLocation[2];
            chkUseParent_loc_size_4.Checked = _settings.OpenUsesLocation[3];

            chkSendMssageViaMMIO_1.Checked = _settings.SendsViaMMIO[0];
            chkSendMssageViaMMIO_2.Checked = _settings.SendsViaMMIO[1];
            chkSendMssageViaMMIO_3.Checked = _settings.SendsViaMMIO[2];
            chkSendMssageViaMMIO_4.Checked = _settings.SendsViaMMIO[3];

            txtMMIO_4char_1.Text = _settings.MMICFourChar[0];
            txtMMIO_4char_2.Text = _settings.MMICFourChar[1];
            txtMMIO_4char_3.Text = _settings.MMICFourChar[2];
            txtMMIO_4char_4.Text = _settings.MMICFourChar[3];

            txtMMIO_message_1.Text = _settings.MMIOMessageON[0];
            txtMMIO_message_2.Text = _settings.MMIOMessageON[1];
            txtMMIO_message_3.Text = _settings.MMIOMessageON[2];
            txtMMIO_message_4.Text = _settings.MMIOMessageON[3];

            txtMMIO_message_off_1.Text = _settings.MMIOMessageOFF[0];
            txtMMIO_message_off_2.Text = _settings.MMIOMessageOFF[1];
            txtMMIO_message_off_3.Text = _settings.MMIOMessageOFF[2];
            txtMMIO_message_off_4.Text = _settings.MMIOMessageOFF[3];

            txtButtonState_led_4char.Text = _settings.LedIndiciatorFourChar;

            txtButtonState_cat_on_reply.Text = _settings.ButtonStateCatReply;

            chkRunStateCommandOnVisible.Checked = _settings.RunStateCommandOnVisible;

            switch (_settings.ButtonStateType)
            {
                case OtherButtonMacroSettings.OB_ButtonState.OFF:
                    radButtonState_off.Checked = true;
                    break;
                case OtherButtonMacroSettings.OB_ButtonState.ON:
                    radButtonState_on.Checked = true;
                    break;
                case OtherButtonMacroSettings.OB_ButtonState.TOGGLE:
                    radButtonState_toggle.Checked = true;
                    break;
                case OtherButtonMacroSettings.OB_ButtonState.LED:
                    radButtonState_led.Checked = true;
                    break;
                case OtherButtonMacroSettings.OB_ButtonState.CONT_VIS:
                    radButtonState_container_visible.Checked = true;
                    break;
                case OtherButtonMacroSettings.OB_ButtonState.CAT:
                    radButtonState_catstate.Checked = true;
                    break;
            }

            chkCatSend_1.Checked = _settings.CatMacroSend[0];

            txtCatMacro.Text = _settings.CatMacro;

            int index = 1;
            foreach(KeyValuePair<string, string> pair in containers)
            {
                clsContainerComboboxItem cci = new clsContainerComboboxItem();

                cci.ID = pair.Key;
                cci.ListIndex = index;
                cci.Text = pair.Value;

                int idx;
                idx = comboCloseContainer_1.Items.Add(cci);
                if(cci.ID == _settings.CloseContainerID[0]) comboCloseContainer_1.SelectedIndex = idx;
                idx = comboCloseContainer_2.Items.Add(cci);
                if (cci.ID == _settings.CloseContainerID[1]) comboCloseContainer_2.SelectedIndex = idx;
                idx = comboCloseContainer_3.Items.Add(cci);
                if (cci.ID == _settings.CloseContainerID[2]) comboCloseContainer_3.SelectedIndex = idx;
                idx = comboCloseContainer_4.Items.Add(cci);
                if (cci.ID == _settings.CloseContainerID[3]) comboCloseContainer_4.SelectedIndex = idx;

                idx = comboOpenContainer_1.Items.Add(cci);
                if (cci.ID == _settings.OpenContainerID[0]) comboOpenContainer_1.SelectedIndex = idx;
                idx = comboOpenContainer_2.Items.Add(cci);
                if (cci.ID == _settings.OpenContainerID[1]) comboOpenContainer_2.SelectedIndex = idx;
                idx = comboOpenContainer_3.Items.Add(cci);
                if (cci.ID == _settings.OpenContainerID[2]) comboOpenContainer_3.SelectedIndex = idx;
                idx = comboOpenContainer_4.Items.Add(cci);
                if (cci.ID == _settings.OpenContainerID[3]) comboOpenContainer_4.SelectedIndex = idx;

                comboButtonState_container_visibility.Items.Add(cci);
                if (cci.ID == _settings.ContainerVisibleID) comboButtonState_container_visibility.SelectedIndex = idx;

                index++;
            }

            if (comboCloseContainer_1.Items.Count > 0 && comboCloseContainer_1.SelectedIndex == -1) comboCloseContainer_1.SelectedIndex = 0;
            if (comboCloseContainer_2.Items.Count > 0 && comboCloseContainer_2.SelectedIndex == -1) comboCloseContainer_2.SelectedIndex = 0;
            if (comboCloseContainer_3.Items.Count > 0 && comboCloseContainer_3.SelectedIndex == -1) comboCloseContainer_3.SelectedIndex = 0;
            if (comboCloseContainer_4.Items.Count > 0 && comboCloseContainer_4.SelectedIndex == -1) comboCloseContainer_4.SelectedIndex = 0;

            if (comboOpenContainer_1.Items.Count > 0 && comboOpenContainer_1.SelectedIndex == -1) comboOpenContainer_1.SelectedIndex = 0;
            if (comboOpenContainer_2.Items.Count > 0 && comboOpenContainer_2.SelectedIndex == -1) comboOpenContainer_2.SelectedIndex = 0;
            if (comboOpenContainer_3.Items.Count > 0 && comboOpenContainer_3.SelectedIndex == -1) comboOpenContainer_3.SelectedIndex = 0;
            if (comboOpenContainer_4.Items.Count > 0 && comboOpenContainer_4.SelectedIndex == -1) comboOpenContainer_4.SelectedIndex = 0;

            if (comboButtonState_container_visibility.Items.Count > 0 && comboButtonState_container_visibility.SelectedIndex == -1) comboButtonState_container_visibility.SelectedIndex = 0;

            updateUseParent(0);
            updateUseParent(1);
            updateUseParent(2);
            updateUseParent(3);
            //

            //_init = false;

            DialogResult dr = this.ShowDialog();

            if(_cat_tester != null && !_cat_tester.IsDisposed)
            {
                _cat_tester.Close();
            }

            return dr;
        }

        private void txtON_TextChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            _settings.OnText = txtON.Text;
        }

        private void txtOFF_TextChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            _settings.OffText = txtOFF.Text;
        }

        private void txtNotes_TextChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            _settings.Notes = txtNotes.Text;
        }

        private void chkClosesParent_CheckedChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            _settings.ClosesParent = chkClosesParent.Checked;
        }

        private int getIndexFromName(object sender)
        {
            if (! (sender.GetType() == typeof(CheckBoxTS) || sender.GetType() == typeof(TextBoxTS) || sender.GetType() == typeof(ComboBoxTS)) ) return -1;

            Control c = sender as Control;
            if (c == null) return -1;

            string name = c.Name;
            if (string.IsNullOrEmpty(name)) return -1;

            int underscore_index = name.LastIndexOf('_');
            if (underscore_index < 0 || underscore_index == name.Length - 1) return -1;
            string suffix = name.Substring(underscore_index + 1).Trim();

            int index;
            if (int.TryParse(suffix, out index)) return index - 1;

            return -1;
        }
        private void chkClosesContainer_n_CheckedChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            int idx = getIndexFromName(sender); if(idx == -1) return;
            _settings.ClosesContainer[idx] = (sender as CheckBoxTS).Checked;

            updateUseParent(idx);
        }

        private void comboOpenContainer_n_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            int idx = getIndexFromName(sender); if (idx == -1) return;
            clsContainerComboboxItem cci = (sender as ComboBoxTS).SelectedItem as clsContainerComboboxItem;
            _settings.OpenContainerID[idx] = cci.ID;

            updateUseParent(idx);
        }

        private void chkOpensContainer_n_CheckedChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            int idx = getIndexFromName(sender); if (idx == -1) return;
            _settings.OpensContainer[idx] = (sender as CheckBoxTS).Checked;

            updateUseParent(idx);
        }

        private void comboCloseContainer_n_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            int idx = getIndexFromName(sender); if (idx == -1) return;
            clsContainerComboboxItem cci = (sender as ComboBoxTS).SelectedItem as clsContainerComboboxItem;
            _settings.CloseContainerID[idx] = cci.ID;

            updateUseParent(idx);
        }
        private void updateUseParent(int idx)
        {
            //if open and close are the same, disable the use parent checkbox
            clsContainerComboboxItem cci;
            clsContainerComboboxItem cci2;
            string idClose;
            string idOpen;
            bool enabled;
            switch (idx)
            {
                case 0:
                    cci = comboCloseContainer_1.SelectedItem as clsContainerComboboxItem;
                    cci2 = comboOpenContainer_1.SelectedItem as clsContainerComboboxItem;
                    idClose = cci == null ? "" : cci.ID;
                    idOpen = cci2 == null ? "" : cci2.ID;
                    enabled = (idClose == "" || idOpen == "") || !(_settings.ClosesContainer[0] && (cci.ID == cci2.ID) && _settings.OpensContainer[0]);
                    chkUseParent_loc_size_1.Enabled = enabled;
                    break;
                case 1:
                    cci = comboCloseContainer_2.SelectedItem as clsContainerComboboxItem;
                    cci2 = comboOpenContainer_2.SelectedItem as clsContainerComboboxItem;
                    idClose = cci == null ? "" : cci.ID;
                    idOpen = cci2 == null ? "" : cci2.ID;
                    enabled = (idClose == "" || idOpen == "") || !(_settings.ClosesContainer[1] && (cci.ID == cci2.ID) && _settings.OpensContainer[1]);
                    chkUseParent_loc_size_2.Enabled = enabled;
                    break;
                case 2:
                    cci = comboCloseContainer_3.SelectedItem as clsContainerComboboxItem;
                    cci2 = comboOpenContainer_3.SelectedItem as clsContainerComboboxItem;
                    idClose = cci == null ? "" : cci.ID;
                    idOpen = cci2 == null ? "" : cci2.ID;
                    enabled = (idClose == "" || idOpen == "") || !(_settings.ClosesContainer[2] && (cci.ID == cci2.ID) && _settings.OpensContainer[2]);
                    chkUseParent_loc_size_3.Enabled = enabled;
                    break;
                case 3:
                    cci = comboCloseContainer_4.SelectedItem as clsContainerComboboxItem;
                    cci2 = comboOpenContainer_4.SelectedItem as clsContainerComboboxItem;
                    idClose = cci == null ? "" : cci.ID;
                    idOpen = cci2 == null ? "" : cci2.ID;
                    enabled = (idClose == "" || idOpen == "") || !(_settings.ClosesContainer[3] && (cci.ID == cci2.ID) && _settings.OpensContainer[3]);
                    chkUseParent_loc_size_4.Enabled = enabled;
                    break;
            }
        }
        private void chkUseParentCoodsForOpen_n_CheckedChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            int idx = getIndexFromName(sender); if (idx == -1) return;
            _settings.OpenUsesLocation[idx] = (sender as CheckBoxTS).Checked;
        }

        private void chkSendMssageViaMMIO_n_CheckedChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            int idx = getIndexFromName(sender); if (idx == -1) return;
            _settings.SendsViaMMIO[idx] = (sender as CheckBoxTS).Checked;
        }

        private void txtMMIO_4char_n_TextChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            int idx = getIndexFromName(sender); if (idx == -1) return;
            _settings.MMICFourChar[idx] = (sender as TextBoxTS).Text;
        }

        private void txtMMIO_message_n_TextChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            int idx = getIndexFromName(sender); if (idx == -1) return;
            _settings.MMIOMessageON[idx] = (sender as TextBoxTS).Text;
        }
        private void txtMMIO_message_n_off_TextChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            int idx = getIndexFromName(sender); if (idx == -1) return;
            _settings.MMIOMessageOFF[idx] = (sender as TextBoxTS).Text;
        }
        private void radButtonState_n_CheckedChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            if (!(sender as RadioButtonTS).Checked) return;

            Control c = sender as Control;
            if (c == null) return;

            OtherButtonMacroSettings.OB_ButtonState state = OtherButtonMacroSettings.OB_ButtonState.OFF;

            string name = c.Name;

            int underscore_index = name.LastIndexOf('_');
            if (underscore_index < 0 || underscore_index == name.Length - 1) return;
            string suffix = name.Substring(underscore_index + 1).Trim().ToLower();

            switch (suffix)
            {
                case "off":
                    _settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.OFF;
                    break;
                case "on":
                    _settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.ON;
                    break;
                case "toggle":
                    _settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.TOGGLE;
                    break;
                case "led":
                    _settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.LED;
                    break;
                case "visible":
                    _settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.CONT_VIS;
                    break;
                case "catstate":
                    _settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.CAT;
                    break;
            }
        }

        private void txtButtonState_led_4char_TextChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            _settings.LedIndiciatorFourChar = txtButtonState_led_4char.Text;
        }

        private void comboButtonState_container_visibility_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            if (comboButtonState_container_visibility.SelectedIndex == -1) return;
            clsContainerComboboxItem cci = comboButtonState_container_visibility.SelectedItem as clsContainerComboboxItem;
            _settings.ContainerVisibleID = cci == null ? "" : cci.ID;
        }

        private void txtButtonState_cat_on_reply_TextChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            _settings.ButtonStateCatReply = txtButtonState_cat_on_reply.Text;
        }

        private void txtCatMacro_TextChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            ScriptResult res = _si.run(txtCatMacro.Text);

            if (!res.is_valid)
            {
                lblErrorText.Text = res.error_message;
                txtTokens.Text = "";
            }
            else
            {
                string token_text = "";
                int i = 1;
                foreach (ScriptCommand sc in res.commands)
                {
                    token_text += $"{i}) " + sc.text + Environment.NewLine;
                    i++;
                }

                txtTokens.Text = token_text;
            }

            lblErrorText.Visible = !res.is_valid;
            picError.Visible = !res.is_valid;

            _settings.CatMacro = txtCatMacro.Text;
        }

        private void chkCatSend_CheckedChanged(object sender, EventArgs e)
        {
            //if (_init) return;
            int idx = getIndexFromName(sender); if (idx == -1) return;
            _settings.CatMacroSend[idx] = (sender as CheckBoxTS).Checked;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            
        }

        private void btnCatTest_Click(object sender, EventArgs e)
        {
            if (_cat_tester == null || _cat_tester.IsDisposed)
            {
                _cat_tester = new CATTester(_console);
            }

            _cat_tester.Show();
            _cat_tester.Focus();
        }

        private void chkRunStateCommandOnVisible_CheckedChanged(object sender, EventArgs e)
        {
            _settings.RunStateCommandOnVisible = chkRunStateCommandOnVisible.Checked;
        }
    }
}
