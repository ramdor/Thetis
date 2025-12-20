/*  frmInfoBarPopup.cs

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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis
{
    public partial class frmInfoBarPopup : Form
    {
        public class PopupActionSelected : EventArgs
        {
            public ucInfoBar.ActionTypes Action;
            public bool ButtonState;
            public MouseButtons Button;
        }

        public event EventHandler<PopupActionSelected> ActionClicked;
        private bool _bHasButtons = false;
        private Dictionary<ucInfoBar.ActionTypes, ucInfoBar.ActionState> _states;

        public frmInfoBarPopup()
        {
            InitializeComponent();
        }
        public bool HasButtons
        {
            get { return _bHasButtons; }
        }

        public void SetStates(Dictionary<ucInfoBar.ActionTypes, ucInfoBar.ActionState> states, ucInfoBar.ActionState b1, ucInfoBar.ActionState b2)
        {
            if (states == null) return;

            int added = 0;
            _states = states;

            Dictionary<string, CheckBoxTS> chkBoxes = getCheckboxesDictionary();
            foreach(CheckBoxTS chkBox in chkBoxes.Values)
            {
                chkBox.Visible = false;
            }
            _bHasButtons = false;

            foreach (KeyValuePair<ucInfoBar.ActionTypes, ucInfoBar.ActionState> kvp in _states)
            {
                ucInfoBar.ActionState state = kvp.Value;

                if (state != null && state.Action != b1.Action && state.Action != b2.Action)
                {
                    string s = "chkButton" + (added + 1).ToString();
                    if (chkBoxes.ContainsKey(s))
                    {
                        CheckBoxTS cb = chkBoxes[s];
                        cb.Tag = (int)state.Action; // used in click
                        cb.Text = state.DisplayString;
                        cb.Checked = state.Checked;
                        toolTip1.SetToolTip(cb, state.TipString);
                        cb.Visible = true;
                        added++;
                    }
                }
            }

            if (added > 0)
            {
                _bHasButtons = true;
                this.Height = (added * (chkButton1.Size.Height + 1)) + 4;
            }
            else
            {                
                // none added, set size to something?
            }
        }
      
        private Dictionary<string, CheckBoxTS> getCheckboxesDictionary()
        {
            // get all chk
            Dictionary<string, CheckBoxTS> chkBoxes = new Dictionary<string, CheckBoxTS>();
            foreach (Control c in this.Controls)
            {
                if (c.GetType() == typeof(CheckBoxTS))
                {
                    chkBoxes.Add(c.Name, (CheckBoxTS)c);
                }
            }
            return chkBoxes;
        }
        private void chkButton1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseButtons mb = MouseButtons.None;

            CheckBoxTS cb = sender as CheckBoxTS;
            MouseEventArgs me = e as MouseEventArgs;

            if (me != null)
                mb = me.Button;

            if (cb != null)
            {
                int index = int.Parse(cb.Name.Substring(9, 1)) - 1;

                ActionClicked?.Invoke(sender, new PopupActionSelected()
                {
                    Action = (ucInfoBar.ActionTypes)cb.Tag,
                    ButtonState = cb.Checked,
                    Button = e.Button
                });
            }
        }
        public CheckBoxTS GetPopupButton(int index)
        {
            // use for skins
            Dictionary<string, CheckBoxTS> chkBoxes = getCheckboxesDictionary();
            if (chkBoxes == null) return null;

            string s = "chkButton" + (index + 1).ToString();
            if (chkBoxes.ContainsKey(s))
                return chkBoxes[s];

            return null;
        }
    }
}
