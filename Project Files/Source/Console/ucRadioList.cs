/*  ucRadioList.cs

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
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Thetis
{
    public sealed partial class ucRadioList : UserControl
    {
        private static readonly NumberFormatInfo _nfi = NumberFormatInfo.InvariantInfo;

        private const string _auto_key = "auto:first_radio";
        private const string _auto_text = "Use the first radio found using the settings above";

        private sealed class RowItem
        {
            public string Key;

            public string NicId;
            public string NicName;
            public string NicDescription;
            public string NicType;
            public string NicIp;
            public string NicMask;

            public NetworkInterfaceType NicInterfaceType;
            public bool NicIsEthernet;
            public bool NicIsWireless;
            public long NicSpeedBitsPerSecond;
            public string NicMacAddress;
            public bool NicIsApipaLocal;
            public bool NicIsLoopbackLocal;
            public bool NicIsDhcpEnabled;
            public OperationalStatus NicStatus;
            public int NicMtu;

            public string RadioModel;
            public string RadioIp;
            public int RadioPort;
            public RadioDiscoveryRadioProtocol RadioProtocol;
            public string RadioVersionText;
            public string RadioMac;
            public bool RadioIsCustom;
            public string RadioGuid;

            public byte RadioCodeVersion;
            public byte RadioBetaVersion;
            public byte RadioProtocol2Supported;
            public byte RadioNumRxs;
            public byte RadioMercuryVersion0;
            public byte RadioMercuryVersion1;
            public byte RadioMercuryVersion2;
            public byte RadioMercuryVersion3;
            public byte RadioPennyVersion;
            public byte RadioMetisVersion;
            public bool RadioIsBusy;            

            public int RadioDiscoveryPortBase;
            public int RadioPortCount;
            public bool RadioIsApipaRadio;

            public bool IsConnected;
            public bool PllLocked;
        }

        private sealed class PersistModel
        {
            public int Version;
            public string SelectedKey;
            public List<PersistRow> Items;
        }

        private sealed class PersistRow
        {
            public string Key;

            public string NicId;
            public string NicName;
            public string NicDescription;
            public string NicType;
            public string NicIp;
            public string NicMask;

            public string RadioModel;
            public string RadioIp;
            public int RadioPort;
            public string RadioProtocolEnum;
            public string RadioVersionText;
            public string RadioMac;
            public bool RadioIsCustom;
            public string RadioGuid;
public byte RadioProtocol2Supported;
        }

        private sealed class HitTestResult
        {
            public int RowIndex;
            public bool IsTrash;
        }

        private readonly List<RowItem> _items;
        private readonly VScrollBar _scroll;

        private string _selected_key;
        private int _hover_index;
        private bool _hover_trash;

        public event EventHandler SelectedRadioChanged;
        public event EventHandler RadioListChanged;

        private int _trash_down_index;
        private bool _trash_down;

        private bool _init_done;

        public ucRadioList()
        {
            _init_done = false;

            InitializeComponent();

            Font = new Font("Consolas", 9f, FontStyle.Regular, GraphicsUnit.Point);

            _items = new List<RowItem>();
            _scroll = new VScrollBar();
            _hover_index = -1;
            _trash_down_index = -1;
            _trash_down = false;

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, true);

            TabStop = true;

            _scroll.Dock = DockStyle.Right;
            _scroll.ValueChanged += scroll_ValueChanged;
            Controls.Add(_scroll);

            BackColor = SystemColors.Window;
            ForeColor = SystemColors.WindowText;

            SizeChanged += ucRadioList_SizeChanged;

            _init_done = true;

            ensureAutoEntry(true);
            updateScroll();
            clampScrollValue();
            Invalidate();
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public string SelectedKey
        {
            get { return _selected_key; }
        }

        public bool IsFirstRadioFoundSelected
        {
            get { return isAutoKey(_selected_key); }
        }

        public string SelectedRadioIp
        {
            get
            {
                RowItem item = getSelectedItem();
                return item != null ? item.RadioIp : null;
            }
        }

        public int SelectedRadioPort
        {
            get
            {
                RowItem item = getSelectedItem();
                return item != null ? item.RadioPort : 0;
            }
        }

        public string SelectedRadioMac
        {
            get
            {
                RowItem item = getSelectedItem();
                return item != null ? item.RadioMac : null;
            }
        }

        public RadioDiscoveryRadioProtocol SelectedRadioProtocol
        {
            get
            {
                RowItem item = getSelectedItem();
                return item != null ? item.RadioProtocol : RadioDiscoveryRadioProtocol.P1;
            }
        }

        public string SelectedNicIp
        {
            get
            {
                RowItem item = getSelectedItem();
                return item != null ? item.NicIp : null;
            }
        }

        public string SelectedNicMask
        {
            get
            {
                RowItem item = getSelectedItem();
                return item != null ? item.NicMask : null;
            }
        }

        public NicRadioScanResult SelectedNICDetails
        {
            get
            {
                RowItem item = getSelectedItem();
                if (item == null) return null;

                RadioInfo radio = SelectedRadioDetails;

                bool hasAnyNic = !string.IsNullOrWhiteSpace(item.NicId) ||
                                 !string.IsNullOrWhiteSpace(item.NicName) ||
                                 !string.IsNullOrWhiteSpace(item.NicDescription) ||
                                 !string.IsNullOrWhiteSpace(item.NicIp) ||
                                 !string.IsNullOrWhiteSpace(item.NicMask) ||
                                 !string.IsNullOrWhiteSpace(item.NicMacAddress);

                bool hasRadio = radio != null;

                if (!hasAnyNic && !hasRadio) return null;

                NicRadioScanResult nic = new NicRadioScanResult();
                nic.NicId = safe(item.NicId);
                nic.NicName = safe(item.NicName);
                nic.NicDescription = safe(item.NicDescription);
                nic.NicSpeedBitsPerSecond = item.NicSpeedBitsPerSecond;

                nic.NicInterfaceType = item.NicInterfaceType;
                nic.IsEthernet = item.NicIsEthernet;
                nic.IsWireless = item.NicIsWireless;

                IPAddress ip = null;
                IPAddress mask = null;

                if (!string.IsNullOrWhiteSpace(item.NicIp)) IPAddress.TryParse(item.NicIp, out ip);
                if (!string.IsNullOrWhiteSpace(item.NicMask)) IPAddress.TryParse(item.NicMask, out mask);

                nic.LocalIPv4 = ip;
                nic.LocalMaskIPv4 = mask;

                nic.NicMacAddress = safe(item.NicMacAddress);
                nic.IsApipaLocal = item.NicIsApipaLocal;
                nic.IsLoopbackLocal = item.NicIsLoopbackLocal;

                nic.IsDhcpEnabled = item.NicIsDhcpEnabled;
                nic.NicStatus = item.NicStatus;
                nic.Mtu = item.NicMtu;

                if (radio != null)
                {
                    nic.Radios.Add(radio);
                }

                return nic;
            }
        }

        public RadioInfo SelectedRadioDetails
        {
            get
            {
                RowItem item = getSelectedItem();
                if (item == null) return null;

                bool hasAnyRadio = !string.IsNullOrWhiteSpace(item.RadioIp) ||
                                   !string.IsNullOrWhiteSpace(item.RadioMac) ||
                                   !string.IsNullOrWhiteSpace(item.RadioModel);

                if (!hasAnyRadio) return null;

                IPAddress ip = null;
                if (!string.IsNullOrWhiteSpace(item.RadioIp))
                {
                    IPAddress.TryParse(item.RadioIp, out ip);
                }

                RadioInfo info = new RadioInfo();
                info.Protocol = item.RadioProtocol;
                info.IpAddress = ip;
                info.IsCustom = item.RadioIsCustom;
                info.CustomGuid = safe(item.RadioGuid);
                info.MacAddress = item.RadioIsCustom ? "" : safe(item.RadioMac);
HPSDRHW hw = (HPSDRHW)0;
                string model = safe(item.RadioModel);

                if (!string.IsNullOrWhiteSpace(model))
                {
                    try
                    {
                        hw = (HPSDRHW)Enum.Parse(typeof(HPSDRHW), model, true);
                    }
                    catch
                    {
                        hw = (HPSDRHW)0;
                    }
                }

                info.DeviceType = hw;

                info.CodeVersion = item.RadioCodeVersion;
                info.BetaVersion = item.RadioBetaVersion;
                info.Protocol2Supported = item.RadioProtocol2Supported;
                info.NumRxs = item.RadioNumRxs;
                info.MercuryVersion0 = item.RadioMercuryVersion0;
                info.MercuryVersion1 = item.RadioMercuryVersion1;
                info.MercuryVersion2 = item.RadioMercuryVersion2;
                info.MercuryVersion3 = item.RadioMercuryVersion3;
                info.PennyVersion = item.RadioPennyVersion;
                info.MetisVersion = item.RadioMetisVersion;
                info.IsBusy = item.RadioIsBusy;

                info.DiscoveryPortBase = item.RadioDiscoveryPortBase > 0 ? item.RadioDiscoveryPortBase : item.RadioPort;
                info.PortCount = item.RadioPortCount;
                info.IsApipaRadio = item.RadioIsApipaRadio;

                return info;
            }
        }

        public bool DoesRadioExist(string radioKey)
        {
            if (string.IsNullOrWhiteSpace(radioKey)) return false;

            for (int i = 0; i < _items.Count; i++)
            {
                if (string.Equals(_items[i].Key, radioKey, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void RadioConnected(string radioKey)
        {
            int idx = indexOfKey(radioKey);
            if (idx < 0) return;

            bool changed = false;

            for (int i = 0; i < _items.Count; i++)
            {
                bool shouldBeConnected = (i == idx);
                if (_items[i].IsConnected != shouldBeConnected)
                {
                    _items[i].IsConnected = shouldBeConnected;
                    changed = true;
                }
            }

            if (changed)
            {
                Invalidate();
            }
        }

        public void RadioDisconnected(string radioKey)
        {
            int idx = indexOfKey(radioKey);
            if (idx < 0) return;

            bool changed = false;

            if (isAutoKey(radioKey))
            {
                resetAutoItem(_items[idx]);
                changed = true;
            }

            if (_items[idx].IsConnected)
            {
                _items[idx].IsConnected = false;
                changed = true;
            }

            if(changed)
            {
                updateScroll();
                clampScrollValue();
                Invalidate();
            }
        }

        public void RadioConnected()
        {
            if (string.IsNullOrWhiteSpace(_selected_key)) return;
            RadioConnected(_selected_key);
        }

        public void RadioDisconnected()
        {
            if (string.IsNullOrWhiteSpace(_selected_key)) return;
            RadioDisconnected(_selected_key);
        }

        public void DisconnectAll()
        {
            bool changed = false;
            bool reset_first = true;

            for (int i = 0; i < _items.Count; i++)
            {
                if (reset_first && isAutoKey(_items[i].Key))
                {
                    reset_first = false;
                    resetAutoItem(_items[i]);
                    changed = true;
                }

                if (_items[i].IsConnected)
                {
                    _items[i].IsConnected = false;
                    changed = true;
                }
            }

            if (changed)
            {
                updateScroll();
                clampScrollValue();
                Invalidate();
            }
        }


        public void PLLLocked(string radioKey, bool locked)
        {
            int idx = indexOfKey(radioKey);
            if (idx < 0) return;

            RowItem item = _items[idx];

            if (item.RadioProtocol != RadioDiscoveryRadioProtocol.P2)
            {
                if (item.PllLocked)
                {
                    item.PllLocked = false;
                    Invalidate();
                }
                return;
            }

            if (item.PllLocked != locked)
            {
                item.PllLocked = locked;
                Invalidate();
            }
        }

        public void PLLLocked(bool locked)
        {
            if (string.IsNullOrWhiteSpace(_selected_key)) return;
            PLLLocked(_selected_key, locked);
        }

        public void UpdateSelectedDetails(NicRadioScanResult nic, RadioInfo radio)
        {
            if (nic == null) return;
            if (radio == null) return;

            RowItem item = getSelectedItem();
            if (item == null) return;

            fillItemFromInfo(item, nic, radio);

            updateScroll();
            clampScrollValue();

            Invalidate();
            raiseSelectedChanged();
            raiseListChanged();
        }

        public string AddRadio(NicRadioScanResult nic, RadioInfo radio)
        {
            if (nic == null) return null;
            if (radio == null) return null;

            IPAddress radioIp = radio.IpAddress;
            IPAddress nicIp = nic.LocalIPv4;
            IPAddress nicMask = nic.LocalMaskIPv4;

            int port = radio.DiscoveryPortBase;
            if (port < 1) port = 1024;

            string mac = safe(radio.MacAddress);
            bool isCustom = radio.IsCustom || string.IsNullOrWhiteSpace(mac);

            if (isCustom)
            {
                string ipText = (radioIp != null) ? radioIp.ToString() : "";

                for (int i = 0; i < _items.Count; i++)
                {
                    RowItem existing = _items[i];
                    if (existing == null) continue;
                    if (isAutoKey(existing.Key)) continue;

                    if (existing.RadioIsCustom &&
                        string.Equals(existing.RadioIp, ipText, StringComparison.OrdinalIgnoreCase) &&
                        existing.RadioPort == port &&
                        existing.RadioProtocol == radio.Protocol)
                    {
                        return existing.Key;
                    }
                }
            }

            string guid = "";
            if (isCustom)
            {
                guid = ensureGuidString(radio.CustomGuid);
                radio.CustomGuid = guid;
                radio.IsCustom = true;
                mac = "";
            }

            string key = buildKey(mac, guid, isCustom, radioIp, port, nic.NicId, radio.Protocol);
            if (DoesRadioExist(key))
            {
                return key;
            }

            string nicType;
            if (nic.IsEthernet) nicType = "Ethernet";
            else if (nic.IsWireless) nicType = "WiFi";
            else nicType = nic.NicInterfaceType.ToString();

            string versionText = buildVersionText(radio);

            RowItem item = new RowItem();
            item.Key = key;

            item.NicId = safe(nic.NicId);
            item.NicName = safe(nic.NicName);
            item.NicDescription = safe(nic.NicDescription);
            item.NicType = nicType;
            item.NicIp = (nicIp != null) ? nicIp.ToString() : "";
            item.NicMask = (nicMask != null) ? nicMask.ToString() : "";

            item.NicInterfaceType = nic.NicInterfaceType;
            item.NicIsEthernet = nic.IsEthernet;
            item.NicIsWireless = nic.IsWireless;
            item.NicSpeedBitsPerSecond = nic.NicSpeedBitsPerSecond;
            item.NicMacAddress = safe(nic.NicMacAddress);
            item.NicIsApipaLocal = nic.IsApipaLocal;
            item.NicIsLoopbackLocal = nic.IsLoopbackLocal;
            item.NicIsDhcpEnabled = nic.IsDhcpEnabled;
            item.NicStatus = nic.NicStatus;
            item.NicMtu = nic.Mtu;

            item.RadioModel = radio.DeviceType.ToString();
            item.RadioIp = (radioIp != null) ? radioIp.ToString() : "";
            item.RadioPort = port;
            item.RadioProtocol = radio.Protocol;
            item.RadioVersionText = versionText;
            item.RadioMac = mac;


            item.RadioIsCustom = isCustom;
            item.RadioGuid = isCustom ? guid : "";
            item.RadioCodeVersion = radio.CodeVersion;
            item.RadioBetaVersion = radio.BetaVersion;
            item.RadioProtocol2Supported = radio.Protocol2Supported;
            item.RadioNumRxs = radio.NumRxs;
            item.RadioMercuryVersion0 = radio.MercuryVersion0;
            item.RadioMercuryVersion1 = radio.MercuryVersion1;
            item.RadioMercuryVersion2 = radio.MercuryVersion2;
            item.RadioMercuryVersion3 = radio.MercuryVersion3;
            item.RadioPennyVersion = radio.PennyVersion;
            item.RadioMetisVersion = radio.MetisVersion;
            item.RadioIsBusy = radio.IsBusy;

            item.RadioDiscoveryPortBase = radio.DiscoveryPortBase;
            item.RadioPortCount = radio.PortCount;
            item.RadioIsApipaRadio = radio.IsApipaRadio;

            item.IsConnected = false;
            item.PllLocked = false;

            ensureAutoEntry(false);
            _items.Add(item);

            updateScroll();
            Invalidate();
            raiseListChanged();

            return key;
        }

        public bool RemoveRadio(string radioKey)
        {
            _trash_down = false;
            _trash_down_index = -1;

            if (string.IsNullOrWhiteSpace(radioKey)) return false;
            if (isAutoKey(radioKey)) return false;

            int idx = indexOfKey(radioKey);
            if (idx < 0) return false;
            if (idx == 0) return false;

            bool removedSelected = string.Equals(_selected_key, _items[idx].Key, StringComparison.OrdinalIgnoreCase);

            _items.RemoveAt(idx);

            if (removedSelected)
            {
                _selected_key = _auto_key;
                raiseSelectedChanged();
            }

            ensureAutoEntry(true);

            updateScroll();
            clampScrollValue();
            Invalidate();
            raiseListChanged();

            return true;
        }

        public void ClearRadios()
        {
            _hover_index = -1;
            _hover_trash = false;
            _trash_down = false;
            _trash_down_index = -1;

            _items.Clear();
            _selected_key = null;

            ensureAutoEntry(true);

            updateScroll();
            setScrollValue(0);

            Invalidate();
            raiseSelectedChanged();
            raiseListChanged();
        }

        public bool MakeRadioVisible(string radioKey)
        {
            int idx = indexOfKey(radioKey);
            if (idx < 0) return false;

            Rectangle viewport = getViewportRect();

            int yTop = rowTopForIndex(idx);
            int yBottom = yTop + rowHeightForIndex(idx);

            int viewTop = _scroll.Value;
            int viewBottom = viewTop + viewport.Height;

            if (yTop < viewTop)
            {
                setScrollValue(yTop);
                return true;
            }

            if (yBottom > viewBottom)
            {
                int newTop = yBottom - viewport.Height;
                setScrollValue(newTop);
                return true;
            }

            return true;
        }

        public string SaveToJson()
        {
            PersistModel model = new PersistModel();
            model.Version = 5;
            model.SelectedKey = isAutoKey(_selected_key) ? "" : enc(_selected_key);
            model.Items = new List<PersistRow>();

            for (int i = 0; i < _items.Count; i++)
            {
                RowItem src = _items[i];
                if (isAutoKey(src.Key)) continue;

                PersistRow row = new PersistRow();

                row.Key = enc(src.Key);

                row.NicId = enc(src.NicId);
                row.NicName = enc(src.NicName);
                row.NicDescription = enc(src.NicDescription);
                row.NicType = enc(src.NicType);
                row.NicIp = enc(src.NicIp);
                row.NicMask = enc(src.NicMask);

                row.RadioModel = enc(src.RadioModel);
                row.RadioIp = enc(src.RadioIp);
                row.RadioPort = src.RadioPort;
                row.RadioProtocolEnum = enc(src.RadioProtocol.ToString());
                row.RadioVersionText = enc(src.RadioVersionText);
                row.RadioMac = enc(src.RadioMac);
                row.RadioIsCustom = src.RadioIsCustom;
                row.RadioGuid = enc(src.RadioGuid);
                row.RadioProtocol2Supported = src.RadioProtocol2Supported;
                model.Items.Add(row);
            }

            string json = JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings { Culture = CultureInfo.InvariantCulture });
            return json;
        }

        public void LoadFromJson(string json)
        {
            ClearRadios();

            if (string.IsNullOrWhiteSpace(json)) return;

            PersistModel model = null;

            try
            {
                model = JsonConvert.DeserializeObject<PersistModel>(json);
            }
            catch
            {
                return;
            }

            if (model == null) return;
            if (model.Items == null) return;


            string desiredSel = dec(model.SelectedKey);
            for (int i = 0; i < model.Items.Count; i++)
            {
                PersistRow r = model.Items[i];
                if (r == null) continue;

                RowItem item = new RowItem();

                item.Key = dec(r.Key);
                if (isAutoKey(item.Key)) continue;

                item.NicId = dec(r.NicId);
                item.NicName = dec(r.NicName);
                item.NicDescription = dec(r.NicDescription);
                item.NicType = dec(r.NicType);
                item.NicIp = dec(r.NicIp);
                item.NicMask = dec(r.NicMask);

                item.RadioModel = dec(r.RadioModel);
                item.RadioIp = dec(r.RadioIp);
                item.RadioPort = r.RadioPort;
                item.RadioVersionText = dec(r.RadioVersionText);
                item.RadioMac = dec(r.RadioMac);                


                item.RadioIsCustom = r.RadioIsCustom;
                item.RadioGuid = dec(r.RadioGuid);

                RadioDiscoveryRadioProtocol proto = RadioDiscoveryRadioProtocol.P1;
                string protoText = dec(r.RadioProtocolEnum);

                if (!string.IsNullOrWhiteSpace(protoText))
                {
                    try
                    {
                        proto = (RadioDiscoveryRadioProtocol)Enum.Parse(typeof(RadioDiscoveryRadioProtocol), protoText, true);
                    }
                    catch
                    {
                        proto = RadioDiscoveryRadioProtocol.P1;
                    }
                }

                item.RadioProtocol = proto;
                item.RadioProtocol2Supported = r.RadioProtocol2Supported;

                item.IsConnected = false;
                item.PllLocked = false;
                string oldKey = item.Key;

                IPAddress ip = null;
                if (!string.IsNullOrWhiteSpace(item.RadioIp))
                {
                    IPAddress.TryParse(item.RadioIp, out ip);
                }

                bool isCustom = item.RadioIsCustom || string.IsNullOrWhiteSpace(item.RadioMac);
                item.RadioIsCustom = isCustom;

                if (isCustom)
                {
                    item.RadioGuid = ensureGuidString(item.RadioGuid);
                    item.RadioMac = "";

                }
                else
                {
                    item.RadioGuid = "";
                }

                item.Key = buildKey(item.RadioMac, item.RadioGuid, item.RadioIsCustom, ip, item.RadioPort, item.NicId, proto);

                if (!string.IsNullOrWhiteSpace(desiredSel) && string.Equals(desiredSel, oldKey, StringComparison.OrdinalIgnoreCase))
                {
                    desiredSel = item.Key;
                }
if (!DoesRadioExist(item.Key))
                {
                    ensureAutoEntry(false);
                    _items.Add(item);
                }
            }

            _selected_key = _auto_key;
            if (!string.IsNullOrWhiteSpace(desiredSel) && DoesRadioExist(desiredSel))
            {
                _selected_key = desiredSel;
            }

            ensureAutoEntry(true);

            updateScroll();
            clampScrollValue();
            Invalidate();

            raiseSelectedChanged();
            raiseListChanged();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            if (k == Keys.Up || k == Keys.Down || k == Keys.Delete) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (_items.Count == 0) return;

            if (e.KeyCode == Keys.Up)
            {
                int idx = selectedIndex();
                if (idx > 0)
                {
                    setSelectedByIndex(idx - 1, true);
                }
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Down)
            {
                int idx = selectedIndex();
                if (idx < 0) idx = 0;
                if (idx < _items.Count - 1)
                {
                    setSelectedByIndex(idx + 1, true);
                }
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Delete)
            {
                int idx = selectedIndex();
                if (idx > 0 && idx < _items.Count)
                {
                    string key = _items[idx].Key;
                    RemoveRadio(key);
                }
                e.Handled = true;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            int move;

            if (e.Delta > 0) move = -normalRowHeight();
            else if (e.Delta < 0) move = normalRowHeight();
            else move = 0;

            if (move != 0)
            {
                setScrollValue(_scroll.Value + move);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            HitTestResult hit = hitTest(e.Location);
            int newHover = hit.RowIndex;
            bool newHoverTrash = hit.IsTrash;

            if (newHover != _hover_index || newHoverTrash != _hover_trash)
            {
                _hover_index = newHover;
                _hover_trash = newHoverTrash;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_hover_index != -1 || _hover_trash)
            {
                _hover_index = -1;
                _hover_trash = false;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            if (e.Button != MouseButtons.Left) return;

            HitTestResult hit = hitTest(e.Location);
            if (hit.RowIndex < 0) return;

            if (hit.IsTrash)
            {
                _trash_down = true;
                _trash_down_index = hit.RowIndex;
                Capture = true;
                Invalidate();
                return;
            }

            setSelectedByIndex(hit.RowIndex, false);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left) return;

            bool wasDown = _trash_down;
            int downIdx = _trash_down_index;

            _trash_down = false;
            _trash_down_index = -1;

            if (Capture) Capture = false;

            if (!wasDown || downIdx < 0)
            {
                Invalidate();
                return;
            }

            HitTestResult hit = hitTest(e.Location);

            if (hit.IsTrash && hit.RowIndex == downIdx && downIdx < _items.Count)
            {
                string key = _items[downIdx].Key;
                RemoveRadio(key);
                return;
            }

            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (!_init_done) return;

            updateScroll();
            clampScrollValue();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle viewport = getViewportRect();

            using (SolidBrush bg = new SolidBrush(BackColor))
            {
                g.FillRectangle(bg, viewport);
            }

            if (_items.Count == 0) return;

            int scrollY = _scroll.Value;

            int firstRow = indexFromContentY(scrollY);
            if (firstRow < 0) return;

            int firstTop = rowTopForIndex(firstRow);
            int yOffset = -(scrollY - firstTop);

            int y = viewport.Top + yOffset;

            for (int i = firstRow; i < _items.Count; i++)
            {
                int h = rowHeightForIndex(i);
                Rectangle rowRect = new Rectangle(viewport.Left, y, viewport.Width, h);

                if (rowRect.Bottom >= viewport.Top && rowRect.Top <= viewport.Bottom)
                {
                    bool selected = !string.IsNullOrWhiteSpace(_selected_key) &&
                                    string.Equals(_selected_key, _items[i].Key, StringComparison.OrdinalIgnoreCase);

                    bool hovered = (i == _hover_index);

                    bool canRemove = !isAutoKey(_items[i].Key);
                    bool trashHot = canRemove && ((hovered && _hover_trash) || (_trash_down && _trash_down_index == i));

                    bool compact = (i == 0) && autoRowIsCompact(_items[i]);

                    drawRow(g, rowRect, _items[i], selected, hovered, trashHot, canRemove, compact);
                }

                y += h;
                if (y > viewport.Bottom) break;
            }

            using (Pen border = new Pen(Color.FromArgb(210, 210, 210)))
            {
                g.DrawRectangle(border, new Rectangle(viewport.Left, viewport.Top, viewport.Width - 1, viewport.Height - 1));
            }
        }

        private void drawRow(Graphics g, Rectangle rowRect, RowItem item, bool selected, bool hovered, bool hoverTrash, bool canRemove, bool compact)
        {
            Color baseFill = BackColor;
            Color hoverFill = Color.FromArgb(240, 247, 255);
            Color selectedFill = Color.FromArgb(225, 240, 255);
            Color connectedFill = Color.FromArgb(235, 248, 235);
            Color selectedConnectedFill = Color.FromArgb(222, 246, 230);

            Color fill = baseFill;

            if (selected && item.IsConnected) fill = selectedConnectedFill;
            else if (selected) fill = selectedFill;
            else if (hovered) fill = hoverFill;
            else if (item.IsConnected) fill = connectedFill;

            using (SolidBrush b = new SolidBrush(fill))
            {
                g.FillRectangle(b, rowRect);
            }

            using (Pen sep = new Pen(Color.FromArgb(225, 225, 225)))
            {
                g.DrawLine(sep, rowRect.Left, rowRect.Bottom - 1, rowRect.Right, rowRect.Bottom - 1);
            }

            int pad = scale(10);
            int radioSize = scale(16);
            int trashHit = scale(28);

            Rectangle radioRect = new Rectangle(rowRect.Left + pad, rowRect.Top + (rowRect.Height - radioSize) / 2, radioSize, radioSize);
            Rectangle trashRect = new Rectangle(rowRect.Right - pad - trashHit, rowRect.Top + (rowRect.Height - trashHit) / 2, trashHit, trashHit);

            drawRadioGlyph(g, radioRect, selected);

            int textLeft = radioRect.Right + scale(10);
            int textRight;

            if (canRemove)
            {
                drawTrashGlyph(g, trashRect, hoverTrash);
                textRight = trashRect.Left - scale(10);
            }
            else
            {
                textRight = rowRect.Right - pad;
            }

            if (textRight < textLeft) return;

            Rectangle textRect = new Rectangle(textLeft, rowRect.Top + scale(6), textRight - textLeft, rowRect.Height - scale(12));

            string line1 = buildLine1(item);

            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;

                if (compact)
                {
                    using (Font f1 = new Font(Font, FontStyle.Bold))
                    using (SolidBrush t1 = new SolidBrush(ForeColor))
                    {
                        g.DrawString(line1, f1, t1, textRect, sf);
                    }
                    return;
                }
            }

            string line2 = buildLine2(item);
            string line3 = buildNicLine3(item);
            string line4 = buildNicLine4(item);

            int h = textRect.Height;
            int h1 = (int)Math.Round(h * 0.28);
            int h2 = (int)Math.Round(h * 0.22);
            int h3 = (int)Math.Round(h * 0.25);
            int h4 = h - h1 - h2 - h3;

            if (h1 < 1) h1 = 1;
            if (h2 < 1) h2 = 1;
            if (h3 < 1) h3 = 1;
            if (h4 < 1) h4 = 1;

            Rectangle r1 = new Rectangle(textRect.Left, textRect.Top, textRect.Width, h1);
            Rectangle r2 = new Rectangle(textRect.Left, textRect.Top + h1, textRect.Width, h2);
            Rectangle r3 = new Rectangle(textRect.Left, textRect.Top + h1 + h2, textRect.Width, h3);
            Rectangle r4 = new Rectangle(textRect.Left, textRect.Top + h1 + h2 + h3, textRect.Width, h4);

            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;

                using (Font f1 = new Font(Font, FontStyle.Bold))
                using (SolidBrush t1 = new SolidBrush(ForeColor))
                {
                    g.DrawString(line1, f1, t1, r1, sf);
                }

                using (SolidBrush t2 = new SolidBrush(Color.FromArgb(70, 70, 70)))
                {
                    g.DrawString(line2, Font, t2, r2, sf);
                }

                using (SolidBrush t3 = new SolidBrush(Color.FromArgb(60, 60, 60)))
                {
                    g.DrawString(line3, Font, t3, r3, sf);
                }

                using (SolidBrush t4 = new SolidBrush(Color.FromArgb(110, 110, 110)))
                {
                    g.DrawString(line4, Font, t4, r4, sf);
                }
            }
        }

        private void drawRadioGlyph(Graphics g, Rectangle rect, bool selected)
        {
            using (Pen p = new Pen(Color.FromArgb(110, 110, 110)))
            {
                g.DrawEllipse(p, rect);
            }

            if (selected)
            {
                Rectangle inner = rect;
                int inset = Math.Max(2, rect.Width / 4);
                inner.Inflate(-inset, -inset);

                using (SolidBrush b = new SolidBrush(Color.FromArgb(40, 120, 200)))
                {
                    g.FillEllipse(b, inner);
                }
            }
        }

        private void drawTrashGlyph(Graphics g, Rectangle rect, bool hot)
        {
            Image img = Properties.Resources.trash_black;
            if (img == null) return;

            if (hot)
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(64, 240, 70, 70)))
                {
                    g.FillEllipse(b, rect);
                }
            }

            int w = img.Width;
            int h = img.Height;

            int x = rect.Left + (rect.Width - w) / 2;
            int y = rect.Top + (rect.Height - h) / 2;

            g.DrawImage(img, new Rectangle(x, y, w, h));
        }

        private string buildLine1(RowItem item)
        {
            if (isAutoKey(item.Key))
            {
                bool hasRadio = !string.IsNullOrWhiteSpace(item.RadioIp) ||
                                !string.IsNullOrWhiteSpace(item.RadioMac) ||
                                !string.IsNullOrWhiteSpace(item.RadioModel);

                bool hasNic = !string.IsNullOrWhiteSpace(item.NicDescription) ||
                              !string.IsNullOrWhiteSpace(item.NicName) ||
                              !string.IsNullOrWhiteSpace(item.NicIp) ||
                              !string.IsNullOrWhiteSpace(item.NicMask);

                if (!hasRadio && !hasNic)
                {
                    return _auto_text;
                }
            }

            string model = safe(item.RadioModel);
            string ip = safe(item.RadioIp);
            string mac = item.RadioIsCustom ? "Custom" : safe(item.RadioMac);

            string port = item.RadioPort > 0 ? item.RadioPort.ToString() : "";
            string ipPort = string.IsNullOrWhiteSpace(port) ? ip : (ip + ":" + port);

            string s = "";

            if (!string.IsNullOrWhiteSpace(model))
            {
                s = model;
            }

            if (!string.IsNullOrWhiteSpace(ipPort))
            {
                if (s.Length > 0) s += "  ";
                s += ipPort;
            }

            if (!string.IsNullOrWhiteSpace(mac))
            {
                if (s.Length > 0) s += "  ";
                s += mac;
            }

            return s;
        }

        private string buildLine2(RowItem item)
        {
            if (isAutoKey(item.Key))
            {
                bool hasRadio = !string.IsNullOrWhiteSpace(item.RadioIp) ||
                                !string.IsNullOrWhiteSpace(item.RadioMac) ||
                                !string.IsNullOrWhiteSpace(item.RadioModel);

                bool hasNic = !string.IsNullOrWhiteSpace(item.NicDescription) ||
                              !string.IsNullOrWhiteSpace(item.NicName) ||
                              !string.IsNullOrWhiteSpace(item.NicIp) ||
                              !string.IsNullOrWhiteSpace(item.NicMask);

                if (!hasRadio && !hasNic) return "";
            }

            string protoText;

            if (item.RadioProtocol == RadioDiscoveryRadioProtocol.P1) protoText = "Protocol-1";
            else if (item.RadioProtocol == RadioDiscoveryRadioProtocol.P2)
            {
                protoText = "Protocol-2";
                if(item.RadioProtocol2Supported > 0)
                {
                    string p2ver = (item.RadioProtocol2Supported / 10.0f).ToString("0.0", CultureInfo.InvariantCulture);
                    protoText += " (v" + p2ver + ")";
                }
            }
            else protoText = item.RadioProtocol.ToString();

            string ver = safe(item.RadioVersionText);

            string s;

            if (!string.IsNullOrWhiteSpace(protoText) && !string.IsNullOrWhiteSpace(ver))
            {
                s = protoText + "     Version " + ver;
            }
            else if (!string.IsNullOrWhiteSpace(protoText))
            {
                s = protoText;
            }
            else
            {
                s = "Version " + ver;
            }

            if (item.RadioProtocol != RadioDiscoveryRadioProtocol.P2) return s;

            if (!item.IsConnected)
            {
                s += "     PLL Unknown";
                return s;
            }

            s += item.PllLocked ? "     PLL Locked" : "     PLL Not Locked";
            return s;
        }

        private string buildNicLine3(RowItem item)
        {
            if (isAutoKey(item.Key))
            {
                bool hasRadio = !string.IsNullOrWhiteSpace(item.RadioIp) ||
                                !string.IsNullOrWhiteSpace(item.RadioMac) ||
                                !string.IsNullOrWhiteSpace(item.RadioModel);

                bool hasNic = !string.IsNullOrWhiteSpace(item.NicDescription) ||
                              !string.IsNullOrWhiteSpace(item.NicName) ||
                              !string.IsNullOrWhiteSpace(item.NicIp) ||
                              !string.IsNullOrWhiteSpace(item.NicMask);

                if (!hasRadio && !hasNic) return "";
            }

            string desc = safe(item.NicDescription);
            if (!string.IsNullOrWhiteSpace(desc)) return desc;

            string name = safe(item.NicName);
            return name;
        }

        private string buildNicLine4(RowItem item)
        {
            if (isAutoKey(item.Key))
            {
                bool hasRadio = !string.IsNullOrWhiteSpace(item.RadioIp) ||
                                !string.IsNullOrWhiteSpace(item.RadioMac) ||
                                !string.IsNullOrWhiteSpace(item.RadioModel);

                bool hasNic = !string.IsNullOrWhiteSpace(item.NicDescription) ||
                              !string.IsNullOrWhiteSpace(item.NicName) ||
                              !string.IsNullOrWhiteSpace(item.NicIp) ||
                              !string.IsNullOrWhiteSpace(item.NicMask);

                if (!hasRadio && !hasNic) return "";
            }

            string ip = safe(item.NicIp);
            string mask = safe(item.NicMask);
            string type = safe(item.NicType);

            string ipMask = "";
            if (!string.IsNullOrWhiteSpace(ip) && !string.IsNullOrWhiteSpace(mask))
            {
                ipMask = ip + " / " + mask;
            }
            else if (!string.IsNullOrWhiteSpace(ip))
            {
                ipMask = ip;
            }
            else if (!string.IsNullOrWhiteSpace(mask))
            {
                ipMask = mask;
            }

            if (!string.IsNullOrWhiteSpace(ipMask) && !string.IsNullOrWhiteSpace(type))
            {
                return ipMask + "  " + type;
            }

            if (!string.IsNullOrWhiteSpace(ipMask))
            {
                return ipMask;
            }

            return type;
        }

        private HitTestResult hitTest(Point p)
        {
            HitTestResult r = new HitTestResult();
            r.RowIndex = -1;
            r.IsTrash = false;

            Rectangle viewport = getViewportRect();
            if (!viewport.Contains(p)) return r;

            int yContent = p.Y - viewport.Top + _scroll.Value;
            int idx = indexFromContentY(yContent);

            if (idx < 0 || idx >= _items.Count) return r;

            int rowTop = viewport.Top + rowTopForIndex(idx) - _scroll.Value;
            int rowH = rowHeightForIndex(idx);

            Rectangle rowRect = new Rectangle(viewport.Left, rowTop, viewport.Width, rowH);

            r.RowIndex = idx;

            if (idx == 0) return r;

            int pad = scale(10);
            int trashHit = scale(28);

            Rectangle trashRect = new Rectangle(rowRect.Right - pad - trashHit, rowRect.Top + (rowRect.Height - trashHit) / 2, trashHit, trashHit);
            r.IsTrash = trashRect.Contains(p);

            return r;
        }

        private void setSelectedByIndex(int idx, bool ensureVisible)
        {
            if (idx < 0 || idx >= _items.Count) return;

            string key = _items[idx].Key;

            if (!string.Equals(_selected_key, key, StringComparison.OrdinalIgnoreCase))
            {
                _selected_key = key;
                raiseSelectedChanged();
                Invalidate();
            }

            if (ensureVisible)
            {
                MakeRadioVisible(key);
            }
        }

        private int selectedIndex()
        {
            if (string.IsNullOrWhiteSpace(_selected_key)) return -1;

            for (int i = 0; i < _items.Count; i++)
            {
                if (string.Equals(_items[i].Key, _selected_key, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private RowItem getSelectedItem()
        {
            if (string.IsNullOrWhiteSpace(_selected_key)) return null;

            for (int i = 0; i < _items.Count; i++)
            {
                if (string.Equals(_items[i].Key, _selected_key, StringComparison.OrdinalIgnoreCase))
                {
                    return _items[i];
                }
            }

            return null;
        }

        private int indexOfKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return -1;

            for (int i = 0; i < _items.Count; i++)
            {
                if (string.Equals(_items[i].Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private Rectangle getViewportRect()
        {
            Rectangle r = ClientRectangle;

            if (_scroll.Visible)
            {
                r.Width = Math.Max(0, r.Width - _scroll.Width);
            }

            return r;
        }

        private int normalRowHeight()
        {
            int h = scale(78);
            int min = scale(62);
            if (h < min) h = min;
            return h;
        }

        private int compactRowHeight()
        {
            int h = scale(30);
            int min = scale(26);
            if (h < min) h = min;
            return h;
        }

        private bool autoRowIsCompact(RowItem item)
        {
            if (item == null) return false;
            if (!isAutoKey(item.Key)) return false;

            bool hasRadio = !string.IsNullOrWhiteSpace(item.RadioIp) ||
                            !string.IsNullOrWhiteSpace(item.RadioMac) ||
                            !string.IsNullOrWhiteSpace(item.RadioModel);

            bool hasNic = !string.IsNullOrWhiteSpace(item.NicDescription) ||
                          !string.IsNullOrWhiteSpace(item.NicName) ||
                          !string.IsNullOrWhiteSpace(item.NicIp) ||
                          !string.IsNullOrWhiteSpace(item.NicMask);

            return !hasRadio && !hasNic;
        }

        private int rowHeightForIndex(int idx)
        {
            if (idx == 0 && _items.Count > 0)
            {
                return autoRowIsCompact(_items[0]) ? compactRowHeight() : normalRowHeight();
            }

            return normalRowHeight();
        }

        private int contentHeight()
        {
            int total = 0;

            for (int i = 0; i < _items.Count; i++)
            {
                total += rowHeightForIndex(i);
            }

            return total;
        }

        private int rowTopForIndex(int idx)
        {
            if (idx <= 0) return 0;

            int y = 0;

            for (int i = 0; i < idx && i < _items.Count; i++)
            {
                y += rowHeightForIndex(i);
            }

            return y;
        }

        private int indexFromContentY(int y)
        {
            if (y < 0) return -1;

            int top = 0;

            for (int i = 0; i < _items.Count; i++)
            {
                int h = rowHeightForIndex(i);
                if (y < top + h) return i;
                top += h;
            }

            return -1;
        }

        private int scale(int px)
        {
            int dpi;
            try
            {
                dpi = DeviceDpi;
            }
            catch
            {
                dpi = 96;
            }

            int v = (int)Math.Round((double)px * (double)dpi / 96.0);
            if (v < 1) v = 1;
            return v;
        }

        private void updateScroll()
        {
            Rectangle viewport = getViewportRect();
            int content = contentHeight();

            int large = viewport.Height;
            if (large < 1) large = 1;

            _scroll.Minimum = 0;
            _scroll.LargeChange = large;

            int small = normalRowHeight();
            if (small < 1) small = 1;
            _scroll.SmallChange = small;

            int maxScroll = content - viewport.Height;
            if (maxScroll < 0) maxScroll = 0;

            _scroll.Maximum = Math.Max(0, content - 1);

            bool shouldShow = maxScroll > 0;
            if (_scroll.Visible != shouldShow)
            {
                _scroll.Visible = shouldShow;
            }

            clampScrollValue();
        }

        private void clampScrollValue()
        {
            int max = _scroll.Maximum - _scroll.LargeChange + 1;
            if (max < 0) max = 0;

            int v = _scroll.Value;
            if (v < 0) v = 0;
            if (v > max) v = max;

            if (_scroll.Value != v)
            {
                _scroll.Value = v;
            }
        }

        private void setScrollValue(int value)
        {
            int max = _scroll.Maximum - _scroll.LargeChange + 1;
            if (max < 0) max = 0;

            int v = value;
            if (v < 0) v = 0;
            if (v > max) v = max;

            if (_scroll.Value != v)
            {
                _scroll.Value = v;
            }
            else
            {
                Invalidate();
            }
        }

        private void scroll_ValueChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void ucRadioList_SizeChanged(object sender, EventArgs e)
        {
            updateScroll();
            clampScrollValue();
            Invalidate();
        }

        private void raiseSelectedChanged()
        {
            EventHandler h = SelectedRadioChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        private void raiseListChanged()
        {
            EventHandler h = RadioListChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        private string ensureGuidString(string guid)
        {
            string t = safe(guid).Trim();
            Guid parsed;

            if (t.Length > 0 && Guid.TryParse(t, out parsed))
            {
                return parsed.ToString("D").ToUpperInvariant();
            }

            Guid g = Guid.NewGuid();
            return g.ToString("D").ToUpperInvariant();
        }

        private string buildKey(string mac, string guid, bool isCustom, IPAddress ip, int port, string nicId, RadioDiscoveryRadioProtocol proto)
        {
            if (isCustom)
            {
                string g = ensureGuidString(guid);
                return "guid:" + g;
            }

            string m = safe(mac).Trim().ToUpperInvariant();
            string i = (ip != null) ? ip.ToString() : "";
            string po = port.ToString();
            string n = safe(nicId);
            string p = proto.ToString().ToString();

            if (!string.IsNullOrWhiteSpace(m))
            {
                return "mac:" + m + "|ip:" + i + "|port:" + po + "|nic:" + n + "|proto:" + p;
            }

            return "ip:" + i + "|port:" + po + "|nic:" + n + "|proto:" + p;
        }


        private bool isLegacyMacOnlyKey(string key)
        {
            string k = safe(key);
            if (k.Length == 0) return false;

            int idx = k.IndexOf("|", StringComparison.Ordinal);
            if (idx >= 0) return false;

            return k.StartsWith("mac:", StringComparison.OrdinalIgnoreCase);
        }

        private bool isAutoKey(string key)
        {
            return string.Equals(safe(key), _auto_key, StringComparison.OrdinalIgnoreCase);
        }

        private void ensureAutoEntry(bool selectAutoIfMissingSelection)
        {
            if (_items.Count == 0)
            {
                RowItem item = new RowItem();
                item.Key = _auto_key;
                resetAutoItem(item);
                _items.Add(item);
            }
            else
            {
                if (!isAutoKey(_items[0].Key))
                {
                    RowItem item = new RowItem();
                    item.Key = _auto_key;
                    resetAutoItem(item);
                    _items.Insert(0, item);
                }
            }

            if (selectAutoIfMissingSelection)
            {
                if (string.IsNullOrWhiteSpace(_selected_key) || !DoesRadioExist(_selected_key))
                {
                    _selected_key = _auto_key;
                }
            }
        }

        private void resetAutoItem(RowItem item)
        {
            item.NicId = "";
            item.NicName = "";
            item.NicDescription = "";
            item.NicType = "";
            item.NicIp = "";
            item.NicMask = "";

            item.NicInterfaceType = NetworkInterfaceType.Unknown;
            item.NicIsEthernet = false;
            item.NicIsWireless = false;
            item.NicSpeedBitsPerSecond = 0;
            item.NicMacAddress = "";
            item.NicIsApipaLocal = false;
            item.NicIsLoopbackLocal = false;
            item.NicIsDhcpEnabled = false;
            item.NicStatus = OperationalStatus.Unknown;
            item.NicMtu = 0;

            item.RadioModel = "";
            item.RadioIp = "";
            item.RadioPort = 0;
            item.RadioProtocol = RadioDiscoveryRadioProtocol.P1;
            item.RadioVersionText = "";
            item.RadioMac = "";


            item.RadioIsCustom = false;
            item.RadioGuid = "";
            item.RadioCodeVersion = 0;
            item.RadioBetaVersion = 0;
            item.RadioProtocol2Supported = 0;
            item.RadioNumRxs = 0;
            item.RadioMercuryVersion0 = 0;
            item.RadioMercuryVersion1 = 0;
            item.RadioMercuryVersion2 = 0;
            item.RadioMercuryVersion3 = 0;
            item.RadioPennyVersion = 0;
            item.RadioMetisVersion = 0;
            item.RadioIsBusy = false;

            item.RadioDiscoveryPortBase = 0;
            item.RadioPortCount = 0;
            item.RadioIsApipaRadio = false;

            item.IsConnected = false;
            item.PllLocked = false;
        }

        private void fillItemFromInfo(RowItem item, NicRadioScanResult nic, RadioInfo radio)
        {
            IPAddress radioIp = radio.IpAddress;
            IPAddress nicIp = nic.LocalIPv4;
            IPAddress nicMask = nic.LocalMaskIPv4;

            int port = radio.DiscoveryPortBase;
            if (port < 1) port = 1024;

            string mac = safe(radio.MacAddress);

            bool isCustom = radio.IsCustom || string.IsNullOrWhiteSpace(mac);
            string guid = "";

            if (isCustom)
            {
                guid = ensureGuidString(radio.CustomGuid);
                radio.CustomGuid = guid;
                radio.IsCustom = true;
                mac = "";
            }

            string nicType;
            if (nic.IsEthernet) nicType = "Ethernet";
            else if (nic.IsWireless) nicType = "WiFi";
            else nicType = nic.NicInterfaceType.ToString();

            string versionText = buildVersionText(radio);

            item.NicId = safe(nic.NicId);
            item.NicName = safe(nic.NicName);
            item.NicDescription = safe(nic.NicDescription);
            item.NicType = nicType;
            item.NicIp = (nicIp != null) ? nicIp.ToString() : "";
            item.NicMask = (nicMask != null) ? nicMask.ToString() : "";

            item.NicInterfaceType = nic.NicInterfaceType;
            item.NicIsEthernet = nic.IsEthernet;
            item.NicIsWireless = nic.IsWireless;
            item.NicSpeedBitsPerSecond = nic.NicSpeedBitsPerSecond;
            item.NicMacAddress = safe(nic.NicMacAddress);
            item.NicIsApipaLocal = nic.IsApipaLocal;
            item.NicIsLoopbackLocal = nic.IsLoopbackLocal;
            item.NicIsDhcpEnabled = nic.IsDhcpEnabled;
            item.NicStatus = nic.NicStatus;
            item.NicMtu = nic.Mtu;

            item.RadioModel = radio.DeviceType.ToString();
            item.RadioIp = (radioIp != null) ? radioIp.ToString() : "";
            item.RadioPort = port;
            item.RadioProtocol = radio.Protocol;
            item.RadioVersionText = versionText;
            item.RadioMac = mac;


            item.RadioIsCustom = isCustom;
            item.RadioGuid = isCustom ? guid : "";
            item.RadioCodeVersion = radio.CodeVersion;
            item.RadioBetaVersion = radio.BetaVersion;
            item.RadioProtocol2Supported = radio.Protocol2Supported;
            item.RadioNumRxs = radio.NumRxs;
            item.RadioMercuryVersion0 = radio.MercuryVersion0;
            item.RadioMercuryVersion1 = radio.MercuryVersion1;
            item.RadioMercuryVersion2 = radio.MercuryVersion2;
            item.RadioMercuryVersion3 = radio.MercuryVersion3;
            item.RadioPennyVersion = radio.PennyVersion;
            item.RadioMetisVersion = radio.MetisVersion;
            item.RadioIsBusy = radio.IsBusy;

            item.RadioDiscoveryPortBase = radio.DiscoveryPortBase;
            item.RadioPortCount = radio.PortCount;
            item.RadioIsApipaRadio = radio.IsApipaRadio;
        }

        private string buildVersionText(RadioInfo radio)
        {
            string versionText;

            switch (radio.DeviceType)
            {
                case HPSDRHW.Saturn:
                    versionText = "fpga=" + radio.CodeVersion.ToString();
                    if (radio.BetaVersion >= 39)
                    {
                        versionText += " p2app=" + radio.BetaVersion.ToString();
                    }
                    break;

                default:
                    versionText = (radio.CodeVersion / 10.0f).ToString("F1", _nfi);
                    if (radio.Protocol == RadioDiscoveryRadioProtocol.P2 && radio.BetaVersion > 0)
                    {
                        versionText += "." + radio.BetaVersion.ToString();
                    }
                    break;
            }

            return versionText;
        }

        private string safe(string s)
        {
            if (s == null) return "";
            return s.Trim();
        }

        private string enc(string s)
        {
            string t = safe(s);
            if (t.Length == 0) return "";
            return t.Replace("/", "%2F");
        }

        private string dec(string s)
        {
            string t = safe(s);
            if (t.Length == 0) return "";
            return t.Replace("%2F", "/");
        }
    }
}
