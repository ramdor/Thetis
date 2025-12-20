/*  frmFinder.cs

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
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Xml;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Xml.Linq;

namespace Thetis
{
    public partial class frmFinder : Form
    {
        private class SearchData
        {
            public Control Control { get; set; }
            public string Name { get; set; }
            public string FullName { get; set; }
            public string Text { get; set; }
            public string ToolTip { get; set; }
            public string ShortName {  get; set; }
            public string XMLReplacement { get; set; }
        }

        private Dictionary<string, SearchData> _searchData;
        private object _objLocker;
        private object _objWTLocker;
        private Dictionary<string, Thread> _workerThreads;
        private bool _fullDetails;
        private StringFormat _stringFormat;
        Dictionary<string, string> _xmlData = new Dictionary<string, string>();
        private bool _fullName;
        private bool _highlightResults;
        private bool _keywords;

        public frmFinder()
        {
            _objLocker = new object();
            _objWTLocker = new object();
            _searchData = new Dictionary<string, SearchData>();
            _workerThreads = new Dictionary<string, Thread>();
            _fullName = false;
            _xmlData = new Dictionary<string, string>();

            _stringFormat = new StringFormat(StringFormat.GenericTypographic);
            _stringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;

            InitializeComponent();

            // match inital form state
            _highlightResults = chkHighlight.Checked;
            _fullDetails = chkFullDetails.Checked;
            _keywords = chkKeywords.Checked;
        }
        public void GatherSearchData(Form frm, ToolTip tt)
        {
            if (_workerThreads.ContainsKey(frm.Name)) return;

            Thread worker = new Thread(() =>
            {
                gatherSearchDataThread(frm, tt);
            })
            {
                Name = "Finder Worker Thread for " + frm.Name,
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true,
            };

            lock (_objWTLocker)
            {
                _workerThreads.Add(frm.Name, worker);
            }

            worker.Start();
        }
        public void GatherCATStructData(string file_path)
        {
            string name = "CATSTRUCT_" + file_path;
            if (_workerThreads.ContainsKey(name)) return;

            Thread worker = new Thread(() =>
            {
                gatherCATStructSearchDataThread(file_path);
            })
            {
                Name = "Finder Worker Thread for " + name,
                Priority = ThreadPriority.Highest,
                IsBackground = true,
            };

            lock (_objWTLocker)
            {
                _workerThreads.Add(name, worker);
            }

            worker.Start();
        }

        //
        private class CatStructEntry
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }
        private void gatherCATStructSearchDataThread(string file_path)
        {          
            try
            {
                XDocument document = XDocument.Load(file_path);
                XElement root = document.Element("catstructs");
                List<CatStructEntry> result = new List<CatStructEntry>();

                foreach (XElement element in root.Elements("catstruct"))
                {
                    XAttribute codeAttr = element.Attribute("code");
                    XElement descElem = element.Element("desc");
                    XElement activeElem = element.Element("active");
                    XElement setElem = element.Element("nsetparms");
                    XElement getElem = element.Element("ngetparms");
                    XElement ansElem = element.Element("nansparms");

                    if (codeAttr != null && descElem != null)
                    {
                        string code = codeAttr.Value;
                        string description = descElem.Value;

                        int set = -1;
                        int.TryParse(setElem.Value, out set);
                        int get = -1;
                        int.TryParse(getElem.Value, out get);
                        int ans = -1;
                        int.TryParse(ansElem.Value, out ans);

                        string sVars = "";
                        if (set > 0) sVars += $" set[{set}]";
                        if (get > 0) sVars += $" get[{get}]";
                        if (ans > 0) sVars += $" ans[{ans}]";
                        sVars = sVars.Trim();

                        SearchData sd = new SearchData()
                        {
                            Control = null,
                            Name = "CATcommand",
                            ShortName = "",
                            Text = code + " : " + description + "   " + sVars,
                            ToolTip = code + " : " + description,
                            XMLReplacement = "",
                            FullName = code + " : " + description
                        };

                        lock (_objLocker)
                        {
                            _searchData.Add(Guid.NewGuid().ToString(), sd);
                        }
                    }
                }
            }
            catch { }

            //done
            string name = "CATSTRUCT_" + file_path;
            lock (_objWTLocker)
            {
                _workerThreads.Remove(name);
            }
        }
        //
        private void gatherSearchDataThread(Control frm, ToolTip tt)
        {
            getControlList(frm, tt);

            lock (_objWTLocker)
            {
                _workerThreads.Remove(frm.Name);
            }
        }
        //
        private static readonly HashSet<Type> target_types = new HashSet<Type>
        {
            typeof(CheckBoxTS), typeof(CheckBox),
            typeof(ComboBoxTS), typeof(ComboBox),
            typeof(NumericUpDownTS), typeof(NumericUpDown),
            typeof(RadioButtonTS), typeof(RadioButton),
            typeof(TextBoxTS), typeof(TextBox),
            typeof(TrackBarTS), typeof(TrackBar),
            typeof(ColorButton),
            typeof(ucLGPicker),
            typeof(RichTextBox),
            typeof(LabelTS), typeof(Label),
            typeof(ButtonTS)
        };

        private static string stripPrefix(string name)
        {
            string[] prefixes = new string[] { "clrbtn", "combo", "label", "text", "nud", "lbl", "chk", "rad", "txt", "tb", "ud", "btn" };
            for (int i = 0; i < prefixes.Length; i++)
            {
                string p = prefixes[i];
                if (name.StartsWith(p, StringComparison.OrdinalIgnoreCase)) return name.Substring(p.Length);
            }
            return name;
        }

        private void getControlList(Control root, ToolTip tt)
        {
            HashSet<string> existing_keys;
            lock (_objLocker)
            {
                existing_keys = new HashSet<string>(_searchData.Keys);
            }

            List<SearchData> additions = new List<SearchData>();
            Stack<Control> stack = new Stack<Control>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                Control c = stack.Pop();

                for (int i = 0; i < c.Controls.Count; i++)
                {
                    stack.Push(c.Controls[i]);
                }

                if (!target_types.Contains(c.GetType())) continue;

                string sKey = c.GetFullName();
                if (existing_keys.Contains(sKey)) continue;

                string toolTip = "";
                if (tt != null)
                {
                    string t = tt.GetToolTip(c);
                    if (!string.IsNullOrEmpty(t)) toolTip = t.Replace("\n", " ");
                }

                string sShortName = stripPrefix(c.Name);

                string text = c.Text;
                if (!string.IsNullOrEmpty(text) && text.IndexOf('\n') >= 0) text = text.Replace("\n", " ");

                string xml = "";
                if (_xmlData.ContainsKey(sKey)) xml = _xmlData[sKey];

                SearchData sd = new SearchData()
                {
                    Control = c,
                    Name = c.Name,
                    ShortName = sShortName,
                    Text = string.IsNullOrEmpty(text) ? "" : text,
                    ToolTip = toolTip,
                    XMLReplacement = xml,
                    FullName = sKey
                };

                additions.Add(sd);
                existing_keys.Add(sKey);
            }

            if (additions.Count == 0) return;

            lock (_objLocker)
            {
                for (int i = 0; i < additions.Count; i++)
                {
                    SearchData sd = additions[i];
                    if (!_searchData.ContainsKey(sd.FullName)) _searchData.Add(sd.FullName, sd);
                }
            }
        }
        //
        private bool _ignoreUpdateToList = false;
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                lstResults.DataSource = null;
                lstResults.Items.Clear();
                return;
            }

            lock (_objLocker)
            {
                List<SearchData> searchDataList;
                if (!_keywords)
                {
                    string sSearch = txtSearch.Text.ToLower();
                    Dictionary<string, SearchData> filteredDictionary = _searchData
                        .Where(kv =>
                        kv.Value.Name.Contains(sSearch, StringComparison.OrdinalIgnoreCase) ||
                        kv.Value.Text.Contains(sSearch, StringComparison.OrdinalIgnoreCase) ||
                        kv.Value.ToolTip.Contains(sSearch, StringComparison.OrdinalIgnoreCase) ||
                        kv.Value.XMLReplacement.Contains(sSearch, StringComparison.OrdinalIgnoreCase)
                        ).ToDictionary(kv => kv.Key, kv => kv.Value);

                    searchDataList = filteredDictionary.Values.ToList();
                }
                else
                {
                    string[] search = txtSearch.Text
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.ToLower())
                        .ToArray();

                    Dictionary<string, SearchData> filteredDictionary = _searchData
                        .Where(kv => search.All(term =>
                            kv.Value.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            kv.Value.Text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            kv.Value.ToolTip.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            kv.Value.XMLReplacement.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                        ))
                        .ToDictionary(kv => kv.Key, kv => kv.Value);

                    searchDataList = filteredDictionary.Values.ToList();
                }

                _ignoreUpdateToList = true;
                lstResults.DataSource = null;
                lstResults.Items.Clear();
               
                lstResults.DisplayMember = "Text";
                lstResults.DataSource = searchDataList;
                lstResults.ClearSelected();
                _ignoreUpdateToList = false;
            }
        }

        private SearchData _oldSelectedSearchResult = null;
        private void lstResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_oldSelectedSearchResult != null && _oldSelectedSearchResult.Control != null)
            {
                //clear old one
                Common.HightlightControl(_oldSelectedSearchResult.Control, false, true);
                _oldSelectedSearchResult = null;
            }

            if (lstResults.SelectedItems.Count == 0 || _ignoreUpdateToList) return;

            SearchData sd = lstResults.SelectedItem as SearchData;
            if (sd != null && sd.Control != null)
            {
                // take me to your leader
                showControl(sd.Control);

                // highlight this one
                Common.HightlightControl(sd.Control, true, true);
                _oldSelectedSearchResult = sd;
            }
        }

        private void lstResults_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            ListBox listBox = (ListBox)sender;
            SearchData sd = listBox.Items[e.Index] as SearchData;

            Graphics g = e.Graphics;

            // Set the background color and text color
            Color backgroundColor = e.State.HasFlag(DrawItemState.Selected) ? SystemColors.Highlight : SystemColors.Window;
            Color textColor = e.State.HasFlag(DrawItemState.Selected) ? SystemColors.HighlightText : SystemColors.ControlText;

            // Fill the background
            if (!e.State.HasFlag(DrawItemState.Selected)) {
                backgroundColor = e.Index % 2 == 0 ? backgroundColor : applyTint(backgroundColor, Color.LightGray);
            }
            g.FillRectangle(new SolidBrush(backgroundColor), e.Bounds);

            int yPos = e.Bounds.Y;

            if (_fullDetails)
            {
                string sSearchLower = txtSearch.Text.ToLower();
                if (!string.IsNullOrEmpty(sd.Text))
                {
                    highlight(sSearchLower, sd.Text, listBox, e.Bounds.X, yPos, g);
                    g.DrawString(sd.Text, listBox.Font, new SolidBrush(textColor), e.Bounds.X, yPos, _stringFormat);
                    yPos += 20;
                }

                if (_xmlData.ContainsKey(sd.FullName))
                {
                    string sText = _xmlData[sd.FullName];
                    sText = sText.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                    highlight(sSearchLower, sText, listBox, e.Bounds.X, yPos, g);
                    g.DrawString(sText, listBox.Font, new SolidBrush(textColor), e.Bounds.X, yPos, _stringFormat);
                    yPos += 20;
                }
                else if (!string.IsNullOrEmpty(sd.ToolTip))
                {
                    highlight(sSearchLower, sd.ToolTip, listBox, e.Bounds.X, yPos, g);
                    g.DrawString(sd.ToolTip, listBox.Font, new SolidBrush(textColor), e.Bounds.X, yPos, _stringFormat);
                    yPos += 20;
                }

                highlight(sSearchLower, _fullName ? sd.FullName : sd.Name, listBox, e.Bounds.X, yPos, g);
                g.DrawString(_fullName ? sd.FullName : sd.Name, listBox.Font, new SolidBrush(textColor), e.Bounds.X, yPos, _stringFormat);
            }
            else
            {
                string sText;
                if (_xmlData.ContainsKey(sd.FullName))
                {
                    sText = _xmlData[sd.FullName];
                    sText = sText.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                }
                else if (!string.IsNullOrEmpty(sd.ToolTip))
                {
                    sText = sd.ToolTip;
                }
                else
                {
                    string sTextAddition = "";
                    if (!string.IsNullOrEmpty(sd.Text))
                        sTextAddition = " [" + sd.Text + "]";
                    sText = sd.ShortName + sTextAddition;
                }
                highlight(txtSearch.Text.ToLower(), sText, listBox, e.Bounds.X, e.Bounds.Y, g);
                g.DrawString(sText, listBox.Font, new SolidBrush(textColor), e.Bounds.X, yPos, _stringFormat);
            }
        }
        private void highlight(string sSearchText, string sLineText, ListBox listBox, int xPos, int yPos, Graphics g)
        {
            if (!_highlightResults) return;

            string[] terms = _keywords
                ? sSearchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                : new string[] { sSearchText };

            foreach (string term in terms)
            {
                foreach (Tuple<int, int> t in findSubstringOccurrences(sLineText.ToLower(), term.ToLower()))
                {
                    float start = g.MeasureString(sLineText.Substring(0, t.Item1), listBox.Font, int.MaxValue, _stringFormat).Width;
                    float width = g.MeasureString(sLineText.Substring(t.Item1, term.Length), listBox.Font, int.MaxValue, _stringFormat).Width;
                    Rectangle newRect = new Rectangle(xPos + (int)start, yPos, (int)width, 20);
                    CompositingMode oldMode = g.CompositingMode;
                    g.CompositingMode = CompositingMode.SourceOver;
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(102, Color.Yellow)))
                        g.FillRectangle(brush, newRect);
                    g.CompositingMode = oldMode;
                }
            }
        }

        private List<Tuple<int, int>> findSubstringOccurrences(string inputString, string searchString)
        {
            List<Tuple<int, int>> occurrences = new List<Tuple<int, int>>();
            int index = 0;

            while (index < inputString.Length)
            {
                index = inputString.IndexOf(searchString, index, StringComparison.Ordinal);

                if (index == -1)
                {
                    break;
                }

                int startIndex = index;
                int endIndex = index + searchString.Length - 1;

                occurrences.Add(new Tuple<int, int>(startIndex, endIndex));

                index += searchString.Length;
            }

            return occurrences;
        }
        private Color applyTint(Color baseColor, Color tintColor)
        {
            int r = (baseColor.R + tintColor.R) / 2;
            int g = (baseColor.G + tintColor.G) / 2;
            int b = (baseColor.B + tintColor.B) / 2;

            return Color.FromArgb(r, g, b);
        }

        private void frmFinder_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }

            txtSearch.Text = "";
            Common.SaveForm(this, this.Name);
        }
        public new void Show()
        {
            Common.RestoreForm(this, this.Name, true);
            if (string.IsNullOrEmpty(txtSearch.Text)) txtSearch.Text = "Search";
            base.Show();
            txtSearch.Focus();
        }

        private void lstResults_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            ListBox listBox = (ListBox)sender;
            SearchData sd = listBox.Items[e.Index] as SearchData;

            int height = 20;

            if (sd != null && _fullDetails)
            {
                if (!string.IsNullOrEmpty(sd.Text)) height += 20;
                if (!string.IsNullOrEmpty(sd.ToolTip)) height += 20;
            }

            e.ItemHeight = height;
        }

        private void showControl(Control c)
        {
            if(c == null) return;

            Form f = c.FindForm();
            if(f == null) return;

            if(!f.Visible)
                f.Show();

            f.BringToFront();

            selectRequiredTabs(f, c);

            this.SuspendLayout();
            this.BringToFront();
            lstResults.Focus();
            this.ResumeLayout();
        }
        private void selectRequiredTabs(Control parentControl, Control targetControl)
        {
            if (parentControl == null)
            {
                return;
            }
            if (targetControl == null)
            {
                return;
            }

            List<TabPage> tab_pages = new List<TabPage>();
            HashSet<TabPage> seen = new HashSet<TabPage>();
            Control current = targetControl;

            while (current != null && current != parentControl)
            {
                TabPage tab_page = current as TabPage;
                if (tab_page != null)
                {
                    if (!seen.Contains(tab_page))
                    {
                        tab_pages.Add(tab_page);
                        seen.Add(tab_page);
                    }
                }
                current = current.Parent;
            }

            if (current != parentControl)
            {
                return;
            }

            for (int i = tab_pages.Count - 1; i >= 0; i--)
            {
                TabPage tab_page = tab_pages[i];
                TabControl tab_control = tab_page.Parent as TabControl;
                if (tab_control != null && tab_control.TabPages.Contains(tab_page))
                {
                    tab_control.SelectedTab = tab_page;
                }
            }

            parentControl.PerformLayout(); // forces any pending layout to complete, needed as we have been chaning tabs

            List<ScrollableControl> scrollers = new List<ScrollableControl>();
            Control walker = targetControl.Parent;
            while (walker != null && walker != parentControl)
            {
                ScrollableControl scrollable = walker as ScrollableControl;
                if (scrollable != null && scrollable.AutoScroll) // we need to gat on this otherwise items in ucOtherButtons are found
                {
                    scrollers.Add(scrollable);
                }
                walker = walker.Parent;
            }
            ScrollableControl parent_scrollable = parentControl as ScrollableControl;
            if (parent_scrollable != null && parent_scrollable.AutoScroll) // we need to gat on this otherwise items in ucOtherButtons are found
            {
                scrollers.Add(parent_scrollable);
            }

            for (int i = 0; i < scrollers.Count; i++)
            {
                ScrollableControl s = scrollers[i];
                s.ScrollControlIntoView(targetControl);
                s.Update();
            }
        }

        private void chkFullDetails_CheckedChanged(object sender, EventArgs e)
        {
            lock (_objLocker)
            {
                SearchData sd = lstResults.SelectedItem as SearchData;
                _fullDetails = chkFullDetails.Checked;
                txtSearch_TextChanged(this, EventArgs.Empty);
                if (sd != null)
                    lstResults.SelectedItem = sd;
            }
        }

        public void ReadXmlFinderFile(string directoryPath)
        {
            string filePath = Path.Combine(directoryPath, "Finder.xml");

            _xmlData.Clear();

            if (!File.Exists(filePath)) return;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);                

                XmlNodeList nodes = xmlDoc.SelectNodes("//element");
                foreach (XmlNode node in nodes)
                {
                    string controlName = node.SelectSingleNode("control")?.InnerText;
                    string text = node.SelectSingleNode("text")?.InnerText;

                    if (!string.IsNullOrEmpty(controlName) && !string.IsNullOrEmpty(text))
                    {
                        _xmlData.Add(controlName, text);
                    }
                }
            }
            catch { }
        }

        public void WriteXmlFinderFile(string directoryPath)
        {
            string filePath = Path.Combine(directoryPath, "Finder.xml");

            try
            {
                if (File.Exists(filePath)) return; // only do if not there

                int tries = 0;
                bool workersWorking = false;
                lock (_objWTLocker)
                {
                    workersWorking = _workerThreads.Count > 0;
                }
                while (workersWorking)
                {
                    Thread.Sleep(50);
                    lock (_objWTLocker)
                    {
                        workersWorking = _workerThreads.Count > 0;
                    }
                    tries++;
                    if (tries > 200) return; // give up after 10 seconds total time. If not done in 10 seconds then time to buy a new cpu !
                }

                XmlDocument xmlDoc = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(xmlDeclaration);

                XmlElement rootElement = xmlDoc.CreateElement("root");
                xmlDoc.AppendChild(rootElement);

                lock (_objLocker)
                {
                    foreach (KeyValuePair<string, SearchData> kvp in _searchData)
                    {
                        if (kvp.Value.Control == null) continue;

                        XmlElement elementElement = xmlDoc.CreateElement("element");
                        XmlElement controlElement = xmlDoc.CreateElement("control");
                        controlElement.InnerText = kvp.Key;
                        elementElement.AppendChild(controlElement);
                        XmlElement textElement = xmlDoc.CreateElement("text");
                        textElement.InnerText = null;
                        elementElement.AppendChild(textElement);
                        rootElement.AppendChild(elementElement);
                    }
                }

                xmlDoc.Save(filePath);
            }
            catch {}
        }
        private void frmFinder_KeyDown(object sender, KeyEventArgs e)
        {
            // alt key to show additional info when in full details mode
            // ctrl c to copy full control name to clipboard
            if(e.Alt && _fullDetails)
            {
                lock (_objLocker)
                {
                    _fullName = !_fullName;
                    SearchData sd = lstResults.SelectedItem as SearchData;
                    txtSearch_TextChanged(this, EventArgs.Empty);
                    if (sd != null)
                        lstResults.SelectedItem = sd;
                }
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                SearchData sd = lstResults.SelectedItem as SearchData;
                if(sd != null)
                {
                    try
                    {
                        Clipboard.SetText(sd.FullName);
                        e.Handled = true;
                    }
                    catch { }
                }
            }
        }

        private void chkHighlight_CheckedChanged(object sender, EventArgs e)
        {
            lock (_objLocker)
            {
                SearchData sd = lstResults.SelectedItem as SearchData;
                _highlightResults = chkHighlight.Checked;                
                txtSearch_TextChanged(this, EventArgs.Empty);
                if(sd != null)
                    lstResults.SelectedItem = sd; 
            }
        }

        private void chkKeywords_CheckedChanged(object sender, EventArgs e)
        {
            lock (_objLocker)
            {
                SearchData sd = lstResults.SelectedItem as SearchData;
                _keywords = chkKeywords.Checked;
                txtSearch_TextChanged(this, EventArgs.Empty);
                if (sd != null)
                    lstResults.SelectedItem = sd;
            }
        }
    }
}
