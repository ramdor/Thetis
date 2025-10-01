/*  ucOtherButtonsOptionsGrid.cs

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
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public enum OtherButtonId
    {
        POWER = 0,
        RX_2,
        MON,
        TUN,
        MOX,
        TWOTON,
        DUP,
        PS_A,
        XPA,
        REC,
        PLAY,
        NR,
        ANF,
        NB,
        SNB,
        MNF,
        MNF_PLUS,
        SPLT,
        A_TO_B,
        ZERO_BEAT,
        B_TO_A,
        IF_TO_V,
        SWAP_AB,
        AVG,
        PEAK,
        CTUN,
        VAC1,
        VAC2,
        MUTE,
        BIN,
        SUBRX,
        PAN_SWAP,
        NR1,
        NR2,
        NR3,
        NR4,
        NB1,
        NB2,
        SPECTRUM,
        PANADAPTER,
        SCOPE,
        SCOPE2,
        PHASE,
        WATERFALL,
        HISTOGRAM,
        PANAFALL,
        PANASCOPE,
        SPECTRASCOPE,
        DISPLAY_OFF,
        PAUSE,
        PEAK_BLOBS,
        CURSOR_INFO,
        SPOTS,
        FILL_SPECTRUM,
        SQL,
        SQL_SQL,
        SQL_VSQL,
        RIT,
        RIT0,
        XIT,
        XIT0,
        MIC,
        COMP,
        VOX,
        DEXP,
        RX_EQ,
        TX_EQ,
        TX_FILTER,
        CFC,
        CFC_EQ,
        LEVELER,
        AGC_FIXED,
        AGC_LONG,
        AGC_SLOW,
        AGC_MEDIUM,
        AGC_FAST,
        AGC_CUSTOM,
        AGC_AUTO,
        DITHER,
        RANDOM,
        SR_48000,
        SR_96000,
        SR_192000,
        SR_384000,
        SR_768000,
        SR_1536000,

        INFO_TEXT = 998,
        SPLITTER = 999,

        UNKNOWN = 1000
    }

    public static class OtherButtonIdHelpers
    {
        public static readonly (OtherButtonId id, int bit_group, int bit_number, string caption, string icon_on, string icon_off)[] CheckBoxData =
        new (OtherButtonId, int, int, string, string, string)[]
        {
            (OtherButtonId.INFO_TEXT, -1, -1, "General", "", ""),
            (OtherButtonId.SPLITTER,  -1, -1, "", "", ""),
            (OtherButtonId.POWER,     0,  0, "Power", "power", ""),
            (OtherButtonId.RX_2,      0,  1, "RX 2", "", ""),
            (OtherButtonId.MON,       0,  2, "MON", "", ""),
            (OtherButtonId.TUN,       0,  3, "TUN", "", ""),
            (OtherButtonId.MOX,       0,  4, "MOX", "", ""),
            (OtherButtonId.TWOTON,    0,  5, "2TON", "", ""),
            (OtherButtonId.DUP,       0,  6, "DUP", "", ""),
            (OtherButtonId.PS_A,      0,  7, "PS-A", "", ""),
            (OtherButtonId.XPA,       0,  8, "xPA", "", ""),
            (OtherButtonId.REC,       0,  9, "Rec", "record", ""),
            (OtherButtonId.PLAY,      0, 10, "Play", "play", ""),
            (OtherButtonId.INFO_TEXT, -1, -1, "Noise", "", ""),
            (OtherButtonId.SPLITTER, -1, -1, "", "", ""),
            (OtherButtonId.NR,        1, 0, "NR", "", ""),
            (OtherButtonId.NR1,       1, 1, "NR1", "", ""),
            (OtherButtonId.NR2,       1, 2, "NR2", "", ""),
            (OtherButtonId.NR3,       1, 3, "NR3", "", ""),
            (OtherButtonId.NR4,       1, 4, "NR4", "", ""),
            (OtherButtonId.ANF,       1, 5, "ANF", "", ""),
            (OtherButtonId.NB,        1, 6, "NB", "", ""),
            (OtherButtonId.NB1,       1, 7, "NB1", "", ""),
            (OtherButtonId.NB2,       1, 8, "NB2", "", ""),
            (OtherButtonId.SNB,       1, 9, "SNB", "", ""),
            (OtherButtonId.MNF,       1, 10, "MNF", "", ""),
            (OtherButtonId.MNF_PLUS,  1, 11, "MNF+", "", ""),
            (OtherButtonId.INFO_TEXT, -1, -1, "VFOs", "", ""),
            (OtherButtonId.SPLITTER, -1, -1, "", "", ""),
            (OtherButtonId.SPLT,      2, 0, "Split", "", ""),
            (OtherButtonId.A_TO_B,    2, 1, "A > B", "", ""),
            (OtherButtonId.ZERO_BEAT, 2, 2, "0 Beat", "", ""),
            (OtherButtonId.B_TO_A,    2, 3, "A < B", "", ""),
            (OtherButtonId.IF_TO_V,   2, 4, "IF > V", "", ""),
            (OtherButtonId.SWAP_AB,   2, 5, "A <> B", "", ""),
            (OtherButtonId.RIT,       2, 6, "RIT", "", ""),
            (OtherButtonId.RIT0,      2, 7, "RIT0", "", ""),
            (OtherButtonId.XIT,       2, 8, "XIT", "", ""),
            (OtherButtonId.XIT0,      2, 9, "XIT0", "", ""),
            (OtherButtonId.INFO_TEXT, -1, -1, "Display", "", ""),
            (OtherButtonId.SPLITTER,  -1, -1, "", "", ""),
            (OtherButtonId.AVG,           3, 0, "Avg", "", ""),
            (OtherButtonId.PEAK,          3, 1, "Peak", "", ""),
            (OtherButtonId.CTUN,          3, 2, "CTUN", "", ""),
            (OtherButtonId.SPECTRUM,      3, 3, "Spectrum", "spectrum", ""),
            (OtherButtonId.PANADAPTER,    3, 4, "Panadapter", "panadapter", ""),
            (OtherButtonId.SCOPE,         3, 5, "Scope", "scope", ""),
            (OtherButtonId.SCOPE2,        3, 6, "Scope2", "scope2", ""),
            (OtherButtonId.PHASE,         3, 7, "Phase", "phase", ""),
            (OtherButtonId.WATERFALL,     3, 8, "Waterfall", "waterfall", ""),
            (OtherButtonId.HISTOGRAM,     3, 9, "Histogram", "histogram", ""),
            (OtherButtonId.PANAFALL,      3, 10, "Panafall", "panafall", ""),
            (OtherButtonId.PANASCOPE,     3, 11, "Panascope", "panascope", ""),
            (OtherButtonId.SPECTRASCOPE,  3, 12, "Spectrascope", "spectrascope", ""),
            (OtherButtonId.DISPLAY_OFF,   3, 13, "Off", "display_off", ""),
            (OtherButtonId.PAUSE,         3, 14, "Pause", "", ""),
            (OtherButtonId.PEAK_BLOBS,    3, 15, "Peak Blobs", "", ""),
            (OtherButtonId.CURSOR_INFO,   3, 16, "Cur Info", "", ""),
            (OtherButtonId.SPOTS,         3, 17, "Spots", "", ""),
            (OtherButtonId.FILL_SPECTRUM, 3, 18, "Fill", "", ""),
            (OtherButtonId.INFO_TEXT, -1, -1, "Audio / DSP", "", ""),
            (OtherButtonId.SPLITTER, -1, -1, "", "", ""),
            (OtherButtonId.VAC1,      4, 0, "Vac1", "", ""),
            (OtherButtonId.VAC2,      4, 1, "Vac2", "", ""),
            (OtherButtonId.MUTE,      4, 2, "Mute", "mute_on", "mute_off"),
            (OtherButtonId.BIN,       4, 3, "Bin", "", ""),
            (OtherButtonId.SUBRX,     4, 4, "SubRX", "", ""),
            (OtherButtonId.PAN_SWAP,  4, 5, "SwapLR", "", ""),
            (OtherButtonId.SQL,       4, 6, "SQL", "", ""),
            (OtherButtonId.SQL_SQL,   4, 7, "Regular SQL", "", ""),
            (OtherButtonId.SQL_VSQL,  4, 8, "Voice SQL", "", ""),
            (OtherButtonId.MIC,       4, 9, "MIC", "mic_on", "mic_off"),
            (OtherButtonId.COMP,      4, 10, "COMP", "", ""),
            (OtherButtonId.VOX,       4, 11, "VOX", "", ""),
            (OtherButtonId.DEXP,      4, 12, "DEXP", "", ""),
            (OtherButtonId.RX_EQ,     4, 13, "RX EQ", "", ""),
            (OtherButtonId.TX_EQ,     4, 14, "TX EQ", "", ""),
            (OtherButtonId.TX_FILTER, 4, 15, "TX Filter", "", ""),
            (OtherButtonId.CFC,       4, 16, "CFC", "", ""),
            (OtherButtonId.CFC_EQ,    4, 17, "CFC EQ", "", ""),
            (OtherButtonId.LEVELER,   4, 18, "Leveler", "", ""),
            (OtherButtonId.INFO_TEXT, -1, -1, "AGC", "", ""),
            (OtherButtonId.SPLITTER, -1, -1, "", "", ""),
            (OtherButtonId.AGC_FIXED, 5, 0, "FIXED", "", ""),
            (OtherButtonId.AGC_LONG,  5, 1, "LONG", "", ""),
            (OtherButtonId.AGC_SLOW,  5, 2, "SLOW", "", ""),
            (OtherButtonId.AGC_MEDIUM,5, 3, "MEDIUM", "", ""),
            (OtherButtonId.AGC_FAST,  5, 4, "FAST", "", ""),
            (OtherButtonId.AGC_CUSTOM,5, 5, "CUSTOM", "", ""),
            (OtherButtonId.AGC_AUTO,  5, 6, "AUTO", "", ""),
            (OtherButtonId.INFO_TEXT, -1, -1, "Hardware", "", ""),
            (OtherButtonId.SPLITTER, -1, -1, "", "", ""),
            (OtherButtonId.DITHER,    6, 0, "Dither", "", ""),
            (OtherButtonId.RANDOM,    6, 1, "Random", "", ""),
            (OtherButtonId.SR_48000,    6, 2, "48k", "", ""),
            (OtherButtonId.SR_96000,    6, 3, "96k", "", ""),
            (OtherButtonId.SR_192000,    6, 4, "192k", "", ""),
            (OtherButtonId.SR_384000,    6, 5, "384k", "", ""),
            (OtherButtonId.SR_768000,    6, 6, "768k", "", ""),
            (OtherButtonId.SR_1536000,    6, 7, "1536k", "", "")
        };

        public static string OtherButtonIDToText(OtherButtonId id)
        {
            for (int i = 0; i < CheckBoxData.Length; i++)
            {
                if (CheckBoxData[i].id != id) continue;
                if (!string.IsNullOrEmpty(CheckBoxData[i].caption)) return CheckBoxData[i].caption;
            }
            return id.ToString();
        }
        public static string OtherButtonIDToIconOn(OtherButtonId id)
        {
            for (int i = 0; i < CheckBoxData.Length; i++)
            {
                if (CheckBoxData[i].id != id) continue;
                if (!string.IsNullOrEmpty(CheckBoxData[i].icon_on)) return CheckBoxData[i].icon_on;
            }
            return string.Empty;
        }
        public static string OtherButtonIDToIconOff(OtherButtonId id)
        {
            for (int i = 0; i < CheckBoxData.Length; i++)
            {
                if (CheckBoxData[i].id != id) continue;
                if (!string.IsNullOrEmpty(CheckBoxData[i].icon_off)) return CheckBoxData[i].icon_off;
            }
            return string.Empty;
        }


        public static OtherButtonId BitToID(int bit_group, int bit_number)
        {
            for (int i = 0; i < CheckBoxData.Length; i++)
            {
                if (CheckBoxData[i].bit_group != bit_group) continue;
                if (CheckBoxData[i].bit_number != bit_number) continue;
                return CheckBoxData[i].id;
            }
            return OtherButtonId.UNKNOWN;
        }

        public static (int bit_group, int bit) BitFromID(OtherButtonId id)
        {
            for (int i = 0; i < CheckBoxData.Length; i++)
            {
                if (CheckBoxData[i].id == id) return (CheckBoxData[i].bit_group, CheckBoxData[i].bit_number);
            }
            return (-1, -1);
        }

        public static string BitToText(int bit_group, int bit_number)
        {
            OtherButtonId id = BitToID(bit_group, bit_number);
            return OtherButtonIDToText(id);
        }
        public static (string, string) BitToIcon(int bit_group, int bit_number)
        {
            // on, off
            OtherButtonId id = BitToID(bit_group, bit_number);
            return (OtherButtonIDToIconOn(id), OtherButtonIDToIconOff(id));
        }
    }

    public partial class ucOtherButtonsOptionsGrid : UserControl
    {
        private List<CheckBox> _check_boxes;
        private bool _init;
        public event EventHandler CheckboxChanged;

        private TableLayoutPanel _table;

        public ucOtherButtonsOptionsGrid()
        {
            _init = false;
            InitializeComponent();

            this.Size = new Size(173, 182);
            this.scrollableControl1.Location = new Point(0, 0);
            this.scrollableControl1.Size = new Size(170, 178);
            this.scrollableControl1.AutoScroll = true;

            initialise_checkboxes();
        }

        private void initialise_checkboxes()
        {
            if (_check_boxes == null)
            {
                _check_boxes = new List<CheckBox>();
            }
            else
            {
                for (int i = 0; i < _check_boxes.Count; i++)
                    _check_boxes[i].CheckedChanged -= checkbox_checked_changed;

                _check_boxes.Clear();
            }

            scrollableControl1.Controls.Clear();

            TableLayoutPanel table = new TableLayoutPanel();
            table.Name = "tbl_other_buttons";
            table.AutoSize = true;
            table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            table.Dock = DockStyle.Top;
            table.ColumnCount = 2;
            table.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            scrollableControl1.Controls.Add(table);

            int row = 0;
            int col = 0;
            (OtherButtonId id, int bit_group, int bit_number, string caption, string icon_on, string icon_off)[] data = OtherButtonIdHelpers.CheckBoxData;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].id == OtherButtonId.INFO_TEXT)
                {
                    if (col != 0)
                    {
                        col = 0;
                        row++;
                    }

                    LabelTS lbl = new LabelTS();
                    lbl.Name = "lbl_" + i.ToString();
                    lbl.AutoSize = true;
                    lbl.Margin = new Padding(0, 2, 0, 0);
                    lbl.Font = new Font(Font, FontStyle.Bold);
                    lbl.Text = data[i].caption;
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.Controls.Add(lbl, 0, row);
                    table.SetColumnSpan(lbl, 2);
                    row++;
                    continue;
                }

                if (data[i].id == OtherButtonId.SPLITTER)
                {
                    if (col != 0)
                    {
                        col = 0;
                        row++;
                    }

                    Panel sep = new Panel();
                    sep.Name = "sep_" + i.ToString();
                    sep.Height = 1;
                    sep.Dock = DockStyle.Fill;
                    sep.Margin = new Padding(0, 2, 0, 4);
                    sep.BackColor = SystemColors.ControlDark;
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.Controls.Add(sep, 0, row);
                    table.SetColumnSpan(sep, 2);
                    row++;
                    continue;
                }

                CheckBox chk = new CheckBox();
                chk.Name = "chk_" + ((int)data[i].id).ToString();
                chk.AutoSize = true;
                chk.Margin = new Padding(0, 0, 0, 0);
                chk.Text = OtherButtonIdHelpers.OtherButtonIDToText(data[i].id);
                chk.Tag = new ValueTuple<OtherButtonId, int, int>(data[i].id, data[i].bit_group, data[i].bit_number);
                chk.CheckedChanged += checkbox_checked_changed;

                if (col == 0)
                {
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.RowCount = row + 1;
                }

                table.Controls.Add(chk, col, row);
                _check_boxes.Add(chk);

                col++;
                if (col > 1)
                {
                    col = 0;
                    row++;
                }
            }

            _init = true;
        }

        private void checkbox_checked_changed(object sender, EventArgs e)
        {
            if (!_init) return;
            if (CheckboxChanged != null) CheckboxChanged(this, EventArgs.Empty);
        }

        public int GetBitfield(int bit_group)
        {
            int bitfield_value = 0;

            for (int i = 0; i < _check_boxes.Count; i++)
            {
                CheckBox checkbox = _check_boxes[i];
                ValueTuple<OtherButtonId, int, int> meta = (ValueTuple<OtherButtonId, int, int>)checkbox.Tag;
                if (meta.Item2 != bit_group) continue;
                if (meta.Item3 < 0) continue;
                if (checkbox.Checked) bitfield_value |= (1 << meta.Item3);
            }

            return bitfield_value;
        }

        public void SetBitfield(int bit_group, int value)
        {
            bool old_init = _init;
            _init = false;

            for (int i = 0; i < _check_boxes.Count; i++)
            {
                CheckBox checkbox = _check_boxes[i];
                ValueTuple<OtherButtonId, int, int> meta = (ValueTuple<OtherButtonId, int, int>)checkbox.Tag;
                if (meta.Item2 != bit_group) continue;
                if (meta.Item3 < 0) continue;
                bool is_checked = (value & (1 << meta.Item3)) != 0;
                checkbox.Checked = is_checked;
            }

            _init = old_init;
        }

        public int GetCheckedCount()
        {
            int count = 0;
            for (int i = 0; i < _check_boxes.Count; i++)
            {
                CheckBox checkbox = _check_boxes[i];
                if (checkbox.Checked) count++;
            }
            return count;
        }

        public int GetCheckedCount(int bit_group)
        {
            int count = 0;
            for (int i = 0; i < _check_boxes.Count; i++)
            {
                CheckBox checkbox = _check_boxes[i];
                ValueTuple<OtherButtonId, int, int> meta = (ValueTuple<OtherButtonId, int, int>)checkbox.Tag;
                if (meta.Item2 != bit_group) continue;
                if (meta.Item3 < 0) continue;
                if (checkbox.Checked) count++;
            }
            return count;
        }
    }
}

