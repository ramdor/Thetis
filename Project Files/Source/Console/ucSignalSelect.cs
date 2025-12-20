/*  ucSignalSelect.cs

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
    public partial class ucSignalSelect : UserControl
    {
        public class SignalTypeChangedEventArgs : EventArgs
        {
            public Reading SignalType { get; }

            public SignalTypeChangedEventArgs(Reading signalType)
            {
                SignalType = signalType;
            }
        }

        public event EventHandler<SignalTypeChangedEventArgs> SignalTypeChanged;

        public ucSignalSelect()
        {
            InitializeComponent();
        }

        private void radSig_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                onSignalTypeChanged(getSignalTypeFromSelection());
            }
        }

        private void radSigAvg_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                onSignalTypeChanged(getSignalTypeFromSelection());
            }
        }

        private Reading getSignalTypeFromSelection()
        {
            if (radSigAvg.Checked) return Reading.AVG_SIGNAL_STRENGTH;
            if (radSigMaxBin.Checked) return Reading.SIGNAL_MAX_BIN;
            return Reading.SIGNAL_STRENGTH;
        }


        private void onSignalTypeChanged(Reading signalType)
        {
            EventHandler<SignalTypeChangedEventArgs> handler = SignalTypeChanged;
            if (handler != null)
            {
                handler(this, new SignalTypeChangedEventArgs(signalType));
            }
        }

        public Reading SignalType
        {
            get 
            {
                return getSignalTypeFromSelection();
            }
            set 
            {
                switch (value)
                {
                    case Reading.AVG_SIGNAL_STRENGTH:
                        radSigAvg.Checked = true;
                        break;
                    case Reading.SIGNAL_MAX_BIN:
                        radSigMaxBin.Checked = true;
                        break;
                    default:
                        radSig.Checked = true;
                        break;
                }
            }
        }

        private void radSigMaxBin_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                onSignalTypeChanged(getSignalTypeFromSelection());
            }
        }
    }
}
