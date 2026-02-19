/*  clsDiscoveredRadioPicker.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2026 Richard Samphire MW0LGE

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
using System.Net;
using System.Windows.Forms;
using System.Globalization;

namespace Thetis
{
    public sealed class clsDiscoveredRadioPicker
    {
        private sealed class RowRef
        {
            public NicRadioScanResult Nic;
            public RadioInfo Radio;
        }

        public List<NicRadioScanResult> PickRadios(IWin32Window owner, List<NicRadioScanResult> discovered)
        {
            if (discovered == null || discovered.Count == 0)
            {                
                return new List<NicRadioScanResult>();
            }

            List<NicRadioScanResult> picked = null;

            using (Form f = new Form())
            using (DataGridView grid = new DataGridView())
            using (Button btnAdd = new Button())
            using (Button btnCancel = new Button())
            using (Panel bottom = new Panel())
            {
                f.Text = "Discovered radios";
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.ShowInTaskbar = false;
                f.StartPosition = owner != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
                f.ClientSize = new Size(720, 330);

                grid.Dock = DockStyle.Fill;
                grid.AllowUserToAddRows = false;
                grid.AllowUserToDeleteRows = false;
                grid.AllowUserToResizeRows = false;
                grid.AllowUserToResizeColumns = false;
                grid.MultiSelect = false;
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grid.RowHeadersVisible = false;
                grid.AutoGenerateColumns = false;
                grid.ScrollBars = ScrollBars.Vertical;
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                grid.EnableHeadersVisualStyles = false;
                grid.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Control;
                grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = SystemColors.ControlText;

                DataGridViewCheckBoxColumn colPick = new DataGridViewCheckBoxColumn();
                colPick.Name = "Pick";
                colPick.HeaderText = "";
                colPick.Width = 42;
                colPick.FillWeight = 10f;
                colPick.FalseValue = false;
                colPick.TrueValue = true;
                colPick.IndeterminateValue = false;
                colPick.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(colPick);

                DataGridViewTextBoxColumn colHardware = new DataGridViewTextBoxColumn();
                colHardware.Name = "Hardware";
                colHardware.HeaderText = "Hardware";
                colHardware.ReadOnly = true;
                colHardware.FillWeight = 15f;
                colHardware.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(colHardware);

                DataGridViewTextBoxColumn colIp = new DataGridViewTextBoxColumn();
                colIp.Name = "IP";
                colIp.HeaderText = "IP";
                colIp.ReadOnly = true;
                colIp.FillWeight = 15f;
                colIp.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(colIp);

                DataGridViewTextBoxColumn colBasePort = new DataGridViewTextBoxColumn();
                colBasePort.Name = "Port";
                colBasePort.HeaderText = "Base Port";
                colBasePort.ReadOnly = true;
                colBasePort.FillWeight = 15f;
                colBasePort.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(colBasePort);

                DataGridViewTextBoxColumn mac = new DataGridViewTextBoxColumn();
                mac.Name = "Mac";
                mac.HeaderText = "Mac Address";
                mac.ReadOnly = true;
                mac.FillWeight = 20f;
                mac.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(mac);

                DataGridViewTextBoxColumn colProto = new DataGridViewTextBoxColumn();
                colProto.Name = "Protocol";
                colProto.HeaderText = "Protocol";
                colProto.ReadOnly = true;
                colProto.FillWeight = 10f;
                colProto.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(colProto);

                DataGridViewTextBoxColumn colVersion = new DataGridViewTextBoxColumn();
                colVersion.Name = "Version";
                colVersion.HeaderText = "Version";
                colVersion.ReadOnly = true;
                colVersion.FillWeight = 15f;
                colVersion.SortMode = DataGridViewColumnSortMode.NotSortable;
                grid.Columns.Add(colVersion);

                bottom.Dock = DockStyle.Bottom;
                bottom.Height = 42;

                btnAdd.Text = "Add";
                btnAdd.Width = 90;
                btnAdd.Height = 26;
                btnAdd.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                btnAdd.Enabled = false;

                btnCancel.Text = "Cancel";
                btnCancel.Width = 90;
                btnCancel.Height = 26;
                btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                btnCancel.DialogResult = DialogResult.Cancel;

                bottom.Controls.Add(btnAdd);
                bottom.Controls.Add(btnCancel);

                f.Controls.Add(grid);
                f.Controls.Add(bottom);

                f.AcceptButton = btnAdd;
                f.CancelButton = btnCancel;

                btnCancel.Location = new Point(bottom.Width - btnCancel.Width - 12, 8);
                btnAdd.Location = new Point(btnCancel.Left - btnAdd.Width - 8, 8);

                bottom.Resize += (s, e) =>
                {
                    btnCancel.Location = new Point(bottom.Width - btnCancel.Width - 12, 8);
                    btnAdd.Location = new Point(btnCancel.Left - btnAdd.Width - 8, 8);
                };

                Action updateAddEnabled = () =>
                {
                    bool any = false;

                    for (int i = 0; i < grid.Rows.Count; i++)
                    {
                        DataGridViewRow row = grid.Rows[i];
                        RowRef rr = row.Tag as RowRef;
                        if (rr == null)
                        {
                            continue;
                        }

                        object val = row.Cells[0].Value;
                        bool selected = val is bool && (bool)val;
                        if (selected)
                        {
                            any = true;
                            break;
                        }
                    }

                    btnAdd.Enabled = any;
                };

                Func<DataGridViewRow, bool> isHeaderRow = row => row != null && row.Tag is NicRadioScanResult;

                Func<NicRadioScanResult, string> getHeaderText = nic =>
                {
                    string desc = nic != null ? (nic.NicDescription ?? "") : "";
                    string ip = nic != null && nic.LocalIPv4 != null ? nic.LocalIPv4.ToString() : "";
                    string mask = nic != null && nic.LocalMaskIPv4 != null ? nic.LocalMaskIPv4.ToString() : "";
                    string type = nic != null ? (nic.IsEthernet ? "Ethernet" : (nic.IsWireless ? "WiFi" : nic.NicInterfaceTypeString)) : "";
                    type = nic.NicInterfaceTypeString;
                    string apipa = nic != null && nic.IsApipaLocal ? " APIPA" : "";

                    if (ip.Length == 0)
                    {
                        return "NIC: " + desc + " [" + type + apipa + "]";
                    }

                    if (mask.Length == 0)
                    {
                        return "NIC: " + desc + " [" + type + apipa + "]  " + ip;
                    }

                    return "NIC: " + desc + " [" + type + apipa + "]  " + ip + " / " + mask;
                };

                Action<DataGridViewRow, NicRadioScanResult> styleHeaderRow = (row, nic) =>
                {
                    row.Tag = nic;
                    row.ReadOnly = true;
                    row.Height = 24;

                    row.DefaultCellStyle.BackColor = SystemColors.ControlLight;
                    row.DefaultCellStyle.ForeColor = SystemColors.ControlText;
                    row.DefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
                    row.DefaultCellStyle.SelectionBackColor = row.DefaultCellStyle.BackColor;
                    row.DefaultCellStyle.SelectionForeColor = row.DefaultCellStyle.ForeColor;

                    DataGridViewCell oldCell = row.Cells[0];
                    DataGridViewTextBoxCell noCheckCell = new DataGridViewTextBoxCell();
                    noCheckCell.Style = oldCell.Style;
                    row.Cells[0] = noCheckCell;
                    row.Cells[0].Value = "";

                    row.Cells[1].Value = "";
                    row.Cells[2].Value = "";
                    row.Cells[3].Value = "";
                    row.Cells[4].Value = "";
                    row.Cells[5].Value = "";
                    row.Cells[6].Value = "";

                    row.Cells[0].Tag = getHeaderText(nic);
                };

                for (int i = 0; i < discovered.Count; i++)
                {
                    NicRadioScanResult nic = discovered[i];
                    if (nic == null || nic.Radios == null || nic.Radios.Count == 0)
                    {
                        continue;
                    }

                    int headerIndex = grid.Rows.Add();
                    DataGridViewRow headerRow = grid.Rows[headerIndex];
                    styleHeaderRow(headerRow, nic);

                    for (int r = 0; r < nic.Radios.Count; r++)
                    {
                        RadioInfo radio = nic.Radios[r];
                        if (radio == null || radio.IpAddress == null)
                        {
                            continue;
                        }

                        int port = radio.DiscoveryPortBase;
                        if (port < 1)
                        {
                            port = 1024;
                        }

                        string version;
                        switch (radio.DeviceType)
                        {
                            case HPSDRHW.Saturn:
                                version = "fpga=" + radio.CodeVersion;
                                if (radio.BetaVersion >= 39)
                                {
                                    version += " p2app=" + radio.BetaVersion.ToString() + "";
                                }
                                break;

                            case HPSDRHW.HermesLite:
                                version = (radio.CodeVersion / 10.0f).ToString("F1");
                                if (radio.BetaVersion > 0)
                                    version += "." + radio.BetaVersion.ToString();
                                break;

                            default:
                                version = (radio.CodeVersion / 10.0f).ToString("F1");
                                if(radio.Protocol == RadioDiscoveryRadioProtocol.P2 && radio.BetaVersion > 0)
                                {
                                    version += "." + radio.BetaVersion.ToString();
                                }
                                break;
                        }

                        string protocol = "?";
                        if(radio.Protocol == RadioDiscoveryRadioProtocol.P1)
                        {
                            protocol = "1";
                        }
                        else if(radio.Protocol == RadioDiscoveryRadioProtocol.P2)
                        {
                            protocol = "2";
                            //if (radio.ProtocolSupported > 0)
                            //{
                            //    string p2ver = (radio.ProtocolSupported / 10.0f).ToString("0.0", CultureInfo.InvariantCulture);
                            //    protocol += " (v" + p2ver + ")";
                            //}
                        }

                        int rowIndex = grid.Rows.Add(false, radio.DeviceType.ToString(), radio.IpAddress.ToString(), port.ToString(), radio.IsCustom ? "Custom" : radio.MacAddress, protocol, version);
                        DataGridViewRow row = grid.Rows[rowIndex];

                        RowRef rr = new RowRef();
                        rr.Nic = nic;
                        rr.Radio = radio;
                        row.Tag = rr;
                    }
                }

                if (grid.Rows.Count == 0)
                {
                    //MessageBox.Show(owner, "No radios found.", "Discovered radios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return new List<NicRadioScanResult>();
                }

                grid.CellPainting += (s, e) =>
                {
                    if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    {
                        return;
                    }

                    DataGridViewRow row = grid.Rows[e.RowIndex];
                    if (!isHeaderRow(row))
                    {
                        return;
                    }

                    e.Handled = true;

                    if (e.ColumnIndex != 0)
                    {
                        return;
                    }

                    int left = grid.GetCellDisplayRectangle(0, e.RowIndex, true).Left;
                    int top = e.CellBounds.Top;
                    int height = e.CellBounds.Height;

                    int width = 0;
                    for (int c = 0; c <= 6; c++)
                    {
                        width += grid.GetCellDisplayRectangle(c, e.RowIndex, true).Width;
                    }

                    Rectangle merged = new Rectangle(left, top, width, height);

                    using (SolidBrush b = new SolidBrush(row.DefaultCellStyle.BackColor))
                    {
                        e.Graphics.FillRectangle(b, merged);
                    }

                    using (Pen p = new Pen(grid.GridColor))
                    {
                        e.Graphics.DrawRectangle(p, new Rectangle(merged.Left, merged.Top, merged.Width - 1, merged.Height - 1));
                    }

                    string text = row.Cells[0].Tag as string ?? "";
                    Rectangle textRect = new Rectangle(merged.Left + 6, merged.Top + 2, merged.Width - 12, merged.Height - 4);

                    TextRenderer.DrawText(
                        e.Graphics,
                        text,
                        row.DefaultCellStyle.Font ?? grid.Font,
                        textRect,
                        row.DefaultCellStyle.ForeColor,
                        TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.Left
                    );
                };

                bool suppressSelectionChanged = false;

                grid.SelectionChanged += (s, e) =>
                {
                    if (suppressSelectionChanged)
                    {
                        return;
                    }

                    DataGridViewRow row = grid.CurrentRow;
                    if (row == null)
                    {
                        return;
                    }

                    if (!isHeaderRow(row))
                    {
                        return;
                    }

                    int idx = row.Index;

                    int next = idx + 1;
                    while (next < grid.Rows.Count && isHeaderRow(grid.Rows[next]))
                    {
                        next++;
                    }

                    if (next < grid.Rows.Count)
                    {
                        suppressSelectionChanged = true;
                        try
                        {
                            grid.CurrentCell = grid.Rows[next].Cells[1];
                        }
                        finally
                        {
                            suppressSelectionChanged = false;
                        }
                        return;
                    }

                    int prev = idx - 1;
                    while (prev >= 0 && isHeaderRow(grid.Rows[prev]))
                    {
                        prev--;
                    }

                    if (prev >= 0)
                    {
                        suppressSelectionChanged = true;
                        try
                        {
                            grid.CurrentCell = grid.Rows[prev].Cells[1];
                        }
                        finally
                        {
                            suppressSelectionChanged = false;
                        }
                    }
                };

                grid.CurrentCellDirtyStateChanged += (s, e) =>
                {
                    if (grid.IsCurrentCellDirty)
                    {
                        grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    }
                };

                grid.CellValueChanged += (s, e) =>
                {
                    if (e.RowIndex < 0 || e.ColumnIndex != 0)
                    {
                        return;
                    }

                    updateAddEnabled();
                };

                grid.CellDoubleClick += (s, e) =>
                {
                    if (e.RowIndex < 0)
                    {
                        return;
                    }

                    DataGridViewRow row = grid.Rows[e.RowIndex];
                    RowRef rr = row.Tag as RowRef;
                    if (rr == null)
                    {
                        return;
                    }

                    object v = row.Cells[0].Value;
                    bool b = v is bool && (bool)v;
                    row.Cells[0].Value = !b;
                    updateAddEnabled();
                };

                btnAdd.Click += (s, e) =>
                {
                    Dictionary<string, NicRadioScanResult> map = new Dictionary<string, NicRadioScanResult>(StringComparer.OrdinalIgnoreCase);

                    for (int i = 0; i < grid.Rows.Count; i++)
                    {
                        DataGridViewRow row = grid.Rows[i];

                        RowRef rr = row.Tag as RowRef;
                        if (rr == null || rr.Nic == null || rr.Radio == null)
                        {
                            continue;
                        }

                        object val = row.Cells[0].Value;
                        bool selected = val is bool && (bool)val;
                        if (!selected)
                        {
                            continue;
                        }

                        string nicKey = buildNicKey(rr.Nic);

                        NicRadioScanResult outNic;
                        if (!map.TryGetValue(nicKey, out outNic))
                        {
                            outNic = cloneNicWithoutRadios(rr.Nic);
                            map[nicKey] = outNic;
                        }

                        outNic.Radios.Add(rr.Radio);
                    }

                    picked = new List<NicRadioScanResult>(map.Values);
                    f.DialogResult = DialogResult.OK;
                    f.Close();
                };

                updateAddEnabled();

                DialogResult dr = owner != null ? f.ShowDialog(owner) : f.ShowDialog();

                if (dr != DialogResult.OK)
                {
                    return null;
                }

                if (picked == null)
                {
                    return new List<NicRadioScanResult>();
                }

                return picked;
            }
        }

        private string buildNicKey(NicRadioScanResult nic)
        {
            string id = nic.NicId ?? "";
            string ip = nic.LocalIPv4 != null ? nic.LocalIPv4.ToString() : "";
            return id + "|" + ip;
        }

        private NicRadioScanResult cloneNicWithoutRadios(NicRadioScanResult src)
        {
            NicRadioScanResult dst = new NicRadioScanResult();

            dst.NicId = src.NicId;
            dst.NicName = src.NicName;
            dst.NicDescription = src.NicDescription;
            dst.NicSpeedBitsPerSecond = src.NicSpeedBitsPerSecond;

            dst.NicInterfaceType = src.NicInterfaceType;
            dst.IsEthernet = src.IsEthernet;
            dst.IsWireless = src.IsWireless;
            dst.IsApipaLocal = src.IsApipaLocal;

            dst.LocalIPv4 = src.LocalIPv4;
            dst.LocalMaskIPv4 = src.LocalMaskIPv4;
            dst.NicMacAddress = src.NicMacAddress;
            dst.IsApipaLocal = src.IsApipaLocal;
            dst.IsLoopbackLocal = src.IsLoopbackLocal;

            dst.GatewayIPv4 = src.GatewayIPv4;

            if (src.DnsServersIPv4 != null)
            {
                dst.DnsServersIPv4 = new List<IPAddress>(src.DnsServersIPv4);
            }
            else
            {
                dst.DnsServersIPv4 = new List<IPAddress>();
            }

            dst.IsDhcpEnabled = src.IsDhcpEnabled;
            dst.NicStatus = src.NicStatus;
            dst.Mtu = src.Mtu;
            dst.Diagnostics = src.Diagnostics;

            dst.Radios = new List<RadioInfo>();

            return dst;
        }
    }
}
