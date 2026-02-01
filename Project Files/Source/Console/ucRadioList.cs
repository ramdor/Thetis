using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Globalization;

namespace Thetis
{
    public sealed partial class ucRadioList : UserControl
    {
        private static readonly NumberFormatInfo _nfi = NumberFormatInfo.InvariantInfo;

        private sealed class RowItem
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
            public RadioDiscoveryRadioProtocol RadioProtocol;
            public string RadioVersionText;
            public string RadioMac;

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
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public string SelectedKey
        {
            get { return _selected_key; }
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

            if (_items[idx].IsConnected)
            {
                _items[idx].IsConnected = false;
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

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].IsConnected)
                {
                    _items[i].IsConnected = false;
                    changed = true;
                }
            }

            if (changed)
            {
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
            string key = buildKey(mac, radioIp, port, nic.NicId);

            if (DoesRadioExist(key))
            {
                return key;
            }

            string nicType;
            if (nic.IsEthernet) nicType = "Ethernet";
            else if (nic.IsWireless) nicType = "WiFi";
            else nicType = nic.NicInterfaceType.ToString();

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

            RowItem item = new RowItem();
            item.Key = key;

            item.NicId = safe(nic.NicId);
            item.NicName = safe(nic.NicName);
            item.NicDescription = safe(nic.NicDescription);
            item.NicType = nicType;
            item.NicIp = (nicIp != null) ? nicIp.ToString() : "";
            item.NicMask = (nicMask != null) ? nicMask.ToString() : "";

            item.RadioModel = radio.DeviceType.ToString();
            item.RadioIp = (radioIp != null) ? radioIp.ToString() : "";
            item.RadioPort = port;
            item.RadioProtocol = radio.Protocol;
            item.RadioVersionText = versionText;
            item.RadioMac = mac;

            item.IsConnected = false;
            item.PllLocked = false;

            _items.Add(item);

            if (string.IsNullOrEmpty(_selected_key))
            {
                _selected_key = key;
                raiseSelectedChanged();
            }

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

            int idx = indexOfKey(radioKey);
            if (idx < 0) return false;

            bool removedSelected = string.Equals(_selected_key, _items[idx].Key, StringComparison.OrdinalIgnoreCase);

            _items.RemoveAt(idx);

            if (removedSelected)
            {
                _selected_key = null;

                if (_items.Count > 0)
                {
                    int newIdx = idx;
                    if (newIdx >= _items.Count) newIdx = _items.Count - 1;
                    if (newIdx >= 0) _selected_key = _items[newIdx].Key;
                }

                raiseSelectedChanged();
            }

            updateScroll();
            clampScrollValue();
            Invalidate();
            raiseListChanged();

            return true;
        }

        public void ClearRadios()
        {
            if (_items.Count == 0)
            {
                _selected_key = null;
                _hover_index = -1;
                _hover_trash = false;
                _trash_down = false;
                _trash_down_index = -1;
                setScrollValue(0);
                Invalidate();
                raiseSelectedChanged();
                raiseListChanged();
                return;
            }

            _items.Clear();
            _selected_key = null;
            _hover_index = -1;
            _hover_trash = false;
            _trash_down = false;
            _trash_down_index = -1;

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
            int rowH = rowHeight();
            int yTop = idx * rowH;
            int yBottom = yTop + rowH;

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
            model.Version = 2;
            model.SelectedKey = enc(_selected_key);
            model.Items = new List<PersistRow>();

            for (int i = 0; i < _items.Count; i++)
            {
                RowItem src = _items[i];
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

            for (int i = 0; i < model.Items.Count; i++)
            {
                PersistRow r = model.Items[i];
                if (r == null) continue;

                RowItem item = new RowItem();

                item.Key = dec(r.Key);

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

                item.IsConnected = false;
                item.PllLocked = false;

                if (string.IsNullOrWhiteSpace(item.Key) || isLegacyMacOnlyKey(item.Key))
                {
                    IPAddress ip = null;
                    if (!string.IsNullOrWhiteSpace(item.RadioIp))
                    {
                        IPAddress.TryParse(item.RadioIp, out ip);
                    }

                    item.Key = buildKey(item.RadioMac, ip, item.RadioPort, item.NicId);
                }

                if (!DoesRadioExist(item.Key))
                {
                    _items.Add(item);
                }
            }

            _selected_key = null;

            string sel = dec(model.SelectedKey);

            if (!string.IsNullOrWhiteSpace(sel) && DoesRadioExist(sel))
            {
                _selected_key = sel;
            }
            else if (_items.Count > 0)
            {
                _selected_key = _items[0].Key;
            }

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
                if (idx >= 0)
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

            int rowH = rowHeight();
            int move;

            if (e.Delta > 0) move = -rowH;
            else if (e.Delta < 0) move = rowH;
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

            int rowH = rowHeight();
            if (rowH <= 0) return;

            int scrollY = _scroll.Value;
            int firstRow = scrollY / rowH;
            int yOffset = -(scrollY % rowH);

            int visibleRows = (viewport.Height + rowH - 1) / rowH + 1;
            int lastRow = firstRow + visibleRows;
            if (lastRow > _items.Count) lastRow = _items.Count;

            for (int i = firstRow; i < lastRow; i++)
            {
                int y = viewport.Top + yOffset + (i - firstRow) * rowH;
                Rectangle rowRect = new Rectangle(viewport.Left, y, viewport.Width, rowH);

                bool selected = !string.IsNullOrWhiteSpace(_selected_key) &&
                                string.Equals(_selected_key, _items[i].Key, StringComparison.OrdinalIgnoreCase);

                bool hovered = (i == _hover_index);

                bool trashHot = (hovered && _hover_trash) || (_trash_down && _trash_down_index == i);
                drawRow(g, rowRect, _items[i], selected, hovered, trashHot);
            }

            using (Pen border = new Pen(Color.FromArgb(210, 210, 210)))
            {
                g.DrawRectangle(border, new Rectangle(viewport.Left, viewport.Top, viewport.Width - 1, viewport.Height - 1));
            }
        }

        private void drawRow(Graphics g, Rectangle rowRect, RowItem item, bool selected, bool hovered, bool hoverTrash)
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
            drawTrashGlyph(g, trashRect, hoverTrash);

            int textLeft = radioRect.Right + scale(10);
            int textRight = trashRect.Left - scale(10);
            if (textRight < textLeft) return;

            Rectangle textRect = new Rectangle(textLeft, rowRect.Top + scale(6), textRight - textLeft, rowRect.Height - scale(12));

            string line1 = buildLine1(item);
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
                using (SolidBrush b = new SolidBrush(Color.FromArgb(25, 220, 70, 70)))
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
            string model = safe(item.RadioModel);
            string ip = safe(item.RadioIp);
            string mac = safe(item.RadioMac);

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
            string protoText;

            if (item.RadioProtocol == RadioDiscoveryRadioProtocol.P1) protoText = "Protocol-1";
            else if (item.RadioProtocol == RadioDiscoveryRadioProtocol.P2) protoText = "Protocol-2";
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
            string desc = safe(item.NicDescription);
            if (!string.IsNullOrWhiteSpace(desc)) return desc;

            string name = safe(item.NicName);
            return name;
        }

        private string buildNicLine4(RowItem item)
        {
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

            int rowH = rowHeight();
            if (rowH <= 0) return r;

            int y = p.Y - viewport.Top + _scroll.Value;
            int idx = y / rowH;

            if (idx < 0 || idx >= _items.Count) return r;

            int pad = scale(10);
            int trashHit = scale(28);

            int rowTop = viewport.Top + (idx * rowH) - _scroll.Value;
            Rectangle rowRect = new Rectangle(viewport.Left, rowTop, viewport.Width, rowH);
            Rectangle trashRect = new Rectangle(rowRect.Right - pad - trashHit, rowRect.Top + (rowRect.Height - trashHit) / 2, trashHit, trashHit);

            r.RowIndex = idx;
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

        private int rowHeight()
        {
            int h = scale(78);
            int min = scale(62);
            if (h < min) h = min;
            return h;
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
            int content = _items.Count * rowHeight();

            int large = viewport.Height;
            if (large < 1) large = 1;

            _scroll.Minimum = 0;
            _scroll.LargeChange = large;

            int small = rowHeight();
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

        private string buildKey(string mac, IPAddress ip, int port, string nicId)
        {
            string m = safe(mac).Trim().ToUpperInvariant();
            string i = (ip != null) ? ip.ToString() : "";
            string po = port.ToString();
            string n = safe(nicId);

            if (!string.IsNullOrWhiteSpace(m))
            {
                return "mac:" + m + "|ip:" + i + "|port:" + po + "|nic:" + n;
            }

            return "ip:" + i + "|port:" + po + "|nic:" + n;
        }


        private bool isLegacyMacOnlyKey(string key)
        {
            string k = safe(key);
            if (k.Length == 0) return false;

            int idx = k.IndexOf("|", StringComparison.Ordinal);
            if (idx >= 0) return false;

            return k.StartsWith("mac:", StringComparison.OrdinalIgnoreCase);
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
