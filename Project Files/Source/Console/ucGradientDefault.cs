/*  ucGradientDefault.cs

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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace Thetis
{
    public partial class ucGradientDefault : UserControl
    {
        public const string DEFAULT_GRADIENT_PANADAPTOR = "9|1|0.000|-2147418368|1|0.494|-2130771968|1|0.341|-2147418368|1|0.432|-2130745856|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-2130771968|";
        public const string DEFAULT_GRADIENT_WATERFALL = "9|1|0.000|-16777216|0|0.678|-65536|1|0.545|-16711936|1|0.746|-39424|1|0.223|-13395610|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-65536|";
        public const string DEFAULT_GRADIENT_SPECTRAL_SERVER = "9|1|0.000|-16777216|1|0.494|-65536|1|0.341|-16711936|1|0.432|-39424|1|0.159|-16777216|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-65536|";

        private bool _is_panadaptor = false; // true if waterfall

        private readonly (string name, string gradient, bool is_panadaptor)[] _gradients =
        {
            ("Graphite", "9|1|0.000|-16777216|1|0.181|-8421505|0|0.644|-256|0|0.144|-16777216|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", true),
            ("Lemon", "9|1|0.000|-16777216|1|0.181|-8421632|1|0.644|-256|0|0.144|-16777216|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", true),
            ("Ice", "9|1|0.000|-16777216|1|0.262|-13408513|1|0.877|-1|1|0.458|-16724737|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", true),
            ("Fire", "9|1|0.000|-16777216|1|0.332|-39424|1|0.539|-52480|0|0.569|-19841|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-256|", true),
            ("Rainbow", "9|1|0.000|-16777216|1|0.419|-16711681|1|0.168|-5279256|1|0.712|-256|1|0.859|-39424|1|0.558|-16711936|1|0.288|-6697729|1|0.097|-16777216|1|1.000|-65536|", true),

            //duped for now for waterfall
            ("Graphite", "9|1|0.000|-16777216|1|0.181|-8421505|0|0.644|-256|0|0.144|-16777216|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", false),
            ("Lemon", "9|1|0.000|-16777216|1|0.181|-8421632|1|0.644|-256|0|0.144|-16777216|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", false),
            ("Ice", "9|1|0.000|-16777216|1|0.262|-13408513|1|0.877|-1|1|0.458|-16724737|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", false),
            ("Fire", "9|1|0.000|-16777216|1|0.332|-39424|1|0.539|-52480|0|0.569|-19841|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-256|", false),
            ("Rainbow", "9|1|0.000|-16777216|1|0.419|-16711681|1|0.168|-5279256|1|0.712|-256|1|0.859|-39424|1|0.558|-16711936|1|0.288|-6697729|1|0.097|-16777216|1|1.000|-65536|", false),
        };

        [Category("Appearance")]
        [Description("Determines if the gradient is for a panadaptor or waterfall.")]
        public bool IsPanadaptor
        {
            get { return _is_panadaptor; }
            set
            {
                _is_panadaptor = value;
                populateGradientList();
            }
        }

        public event Action<bool, string> SetGradient;

        public ucGradientDefault()
        {
            InitializeComponent();
            populateGradientList();
        }

        private void populateGradientList()
        {
            comboGradient.Items.Clear();
            foreach (var gradient in _gradients.Where(g => g.is_panadaptor == _is_panadaptor))
            {
                comboGradient.Items.Add(gradient.name);
            }

            if (comboGradient.Items.Count > 0) comboGradient.SelectedIndex = 0;
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            if (comboGradient.SelectedIndex < 0) return;

            string config = _gradients
                .Where(g => g.name.Equals(comboGradient.Text, StringComparison.OrdinalIgnoreCase))
                .Select(g => g.gradient)
                .FirstOrDefault() ?? (_is_panadaptor ? DEFAULT_GRADIENT_PANADAPTOR : DEFAULT_GRADIENT_WATERFALL);

            SetGradient?.Invoke(_is_panadaptor, config);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            setControlState(this, Enabled);
        }

        private void setControlState(Control parent, bool enabled)
        {
            foreach (Control control in parent.Controls)
            {
                control.Enabled = enabled;
                if (!enabled)
                {
                    control.ForeColor = SystemColors.GrayText;
                }
                else
                {
                    control.ForeColor = SystemColors.ControlText;
                }
                setControlState(control, enabled);
            }
        }

    }
}
