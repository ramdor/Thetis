//MW0LGE 2023
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace Thetis
{
    public partial class frmFinder : Form
    {
        private class SearchData
        {
            public Control Control { get; set; }
            public string Name { get; set; }
            public string Text { get; set; }
            public string ToolTip { get; set; }
            public string ShortName {  get; set; }
        }

        private Dictionary<string, SearchData> _searchData;
        private object _objLocker;
        private Dictionary<string, Thread> _workerThreads;
        private bool _fullDetails;

        public frmFinder()
        {
            _objLocker = new object();
            _searchData = new Dictionary<string, SearchData>();
            _workerThreads = new Dictionary<string, Thread>();
            _fullDetails = false;

            InitializeComponent();
        }
        public void GatherSearchData(Form frm, ToolTip tt)
        {
            if (_workerThreads.ContainsKey(frm.Name)) return;

            Thread worker = new Thread(() =>
            {
                gatherSearchDataThread(frm, tt);
            })
            {
                Name = "Search Worker Thread for " + frm.Name,
                Priority = ThreadPriority.Highest,
                IsBackground = true,
            };

            _workerThreads.Add(frm.Name, worker);

            worker.Start();
        }
        private void gatherSearchDataThread(Control frm, ToolTip tt)
        {
            lock (_objLocker)
            {
                getControlList(frm, ref _searchData, tt);
            }
        }
        private void getControlList(Control c, ref Dictionary<string, SearchData> searchData, ToolTip tt)
        {
            if (c.Controls.Count > 0)
            {
                foreach (Control c2 in c.Controls)
                    getControlList(c2, ref searchData, tt);
            }

            if (c.GetType() == typeof(CheckBoxTS) || c.GetType() == typeof(CheckBox) ||
                c.GetType() == typeof(ComboBoxTS) || c.GetType() == typeof(ComboBox) ||
                c.GetType() == typeof(NumericUpDownTS) || c.GetType() == typeof(NumericUpDown) ||
                c.GetType() == typeof(RadioButtonTS) || c.GetType() == typeof(RadioButton) ||
                c.GetType() == typeof(TextBoxTS) || c.GetType() == typeof(TextBox) ||
                c.GetType() == typeof(TrackBarTS) || c.GetType() == typeof(TrackBar) ||
                c.GetType() == typeof(ColorButton) ||
                c.GetType() == typeof(ucLGPicker) ||
                c.GetType() == typeof(RichTextBox) ||
                c.GetType() == typeof(LabelTS) || c.GetType() == typeof(Label) ||
                c.GetType() == typeof(ButtonTS)// ||
                //c.GetType() == typeof(TabControl) ||
                //c.GetType() == typeof(TabPage)
                )
            {
                string sKey = c.GetFullName();

                if (!searchData.ContainsKey(sKey))
                {
                    string toolTip = "";
                    if (tt != null)
                        toolTip = tt.GetToolTip(c).Replace("\n", " ");

                    // pull off some junk from control names
                    string sShortName = c.Name;
                    if (sShortName.ToLower().StartsWith("lbl")) sShortName = sShortName.Substring(3);
                    else if (sShortName.ToLower().StartsWith("chk")) sShortName = sShortName.Substring(3);
                    else if (sShortName.ToLower().StartsWith("rad")) sShortName = sShortName.Substring(3);
                    else if (sShortName.ToLower().StartsWith("nud")) sShortName = sShortName.Substring(3);
                    else if (sShortName.ToLower().StartsWith("ud")) sShortName = sShortName.Substring(2);
                    else if (sShortName.ToLower().StartsWith("txt")) sShortName = sShortName.Substring(3);
                    else if (sShortName.ToLower().StartsWith("combo")) sShortName = sShortName.Substring(5);
                    else if (sShortName.ToLower().StartsWith("tb")) sShortName = sShortName.Substring(2);
                    else if (sShortName.ToLower().StartsWith("text")) sShortName = sShortName.Substring(4);
                    else if (sShortName.ToLower().StartsWith("btn")) sShortName = sShortName.Substring(3);
                    else if (sShortName.ToLower().StartsWith("clrbtn")) sShortName = sShortName.Substring(6);
                    else if (sShortName.ToLower().StartsWith("text")) sShortName = sShortName.Substring(4);
                    else if (sShortName.ToLower().StartsWith("label")) sShortName = sShortName.Substring(4);

                    SearchData sd = new SearchData()
                    {
                        Control = c,
                        Name = c.Name,
                        ShortName = sShortName,
                        Text = c.Text.Replace("\n", " "),
                        ToolTip = toolTip
                    };

                    searchData.Add(sKey, sd);
                }
            }
        }
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
                string sSearch = txtSearch.Text.ToLower();
                Dictionary<string, SearchData> filteredDictionary = _searchData
                    .Where(kv => 
                    kv.Value.Name.ToLower().Contains(sSearch) ||
                    kv.Value.Text.ToLower().Contains(sSearch) ||
                    kv.Value.ToolTip.ToLower().Contains(sSearch))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                List<SearchData> searchDataList = filteredDictionary.Values.ToList();

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
            if (_oldSelectedSearchResult != null)
            {
                //clear old one
                Common.HightlightControl(_oldSelectedSearchResult.Control, false, true);
                _oldSelectedSearchResult = null;
            }

            if (lstResults.SelectedItems.Count == 0 || _ignoreUpdateToList) return;

            SearchData sd = lstResults.SelectedItem as SearchData;
            if (sd != null)
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
            backgroundColor = e.Index % 2 == 0 ? backgroundColor : applyTint(backgroundColor, Color.LightGray);
            g.FillRectangle(new SolidBrush(backgroundColor), e.Bounds);

            int yPos = e.Bounds.Y;
            List<Tuple<int, int>> lst;
            StringFormat sf = new StringFormat(StringFormat.GenericTypographic);
            sf.FormatFlags = StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;

            if (_fullDetails)
            {
                if (!string.IsNullOrEmpty(sd.Text))
                {
                    lst = findSubstringOccurrences(sd.Text.ToLower(), txtSearch.Text.ToLower());
                    foreach (Tuple<int, int> t in lst)
                    {
                        float start = g.MeasureString(sd.Text.Substring(0, t.Item1), listBox.Font, int.MaxValue, sf).Width;
                        float width = g.MeasureString(sd.Text.Substring(t.Item1, txtSearch.Text.Length), listBox.Font, int.MaxValue, sf).Width;
                        Rectangle newRect = new Rectangle(e.Bounds.X + (int)start, yPos, (int)(width), 20);
                        g.FillRectangle(new SolidBrush(Color.Yellow), newRect);
                    }

                    g.DrawString(sd.Text, listBox.Font, new SolidBrush(textColor), e.Bounds.X, yPos, sf);
                    yPos += 20;
                }
                if (!string.IsNullOrEmpty(sd.ToolTip))
                {
                    lst = findSubstringOccurrences(sd.ToolTip.ToLower(), txtSearch.Text.ToLower());
                    foreach (Tuple<int, int> t in lst)
                    {
                        float start = g.MeasureString(sd.ToolTip.Substring(0, t.Item1), listBox.Font, int.MaxValue, sf).Width;
                        float width = g.MeasureString(sd.ToolTip.Substring(t.Item1, txtSearch.Text.Length), listBox.Font, int.MaxValue, sf).Width;
                        Rectangle newRect = new Rectangle(e.Bounds.X + (int)start, yPos, (int)(width), 20);
                        g.FillRectangle(new SolidBrush(Color.Yellow), newRect);
                    }

                    g.DrawString(sd.ToolTip, listBox.Font, new SolidBrush(textColor), e.Bounds.X, yPos, sf);
                    yPos += 20;
                }

                lst = findSubstringOccurrences(sd.Name.ToLower(), txtSearch.Text.ToLower());
                foreach (Tuple<int, int> t in lst)
                {
                    float start = g.MeasureString(sd.Name.Substring(0, t.Item1), listBox.Font, int.MaxValue, sf).Width;
                    float width = g.MeasureString(sd.Name.Substring(t.Item1, txtSearch.Text.Length), listBox.Font, int.MaxValue, sf).Width;
                    Rectangle newRect = new Rectangle(e.Bounds.X + (int)start, yPos, (int)(width), 20);
                    g.FillRectangle(new SolidBrush(Color.Yellow), newRect);
                }

                g.DrawString(sd.Name, listBox.Font, new SolidBrush(textColor), e.Bounds.X, yPos, sf);
            }
            else
            {
                string sText;
                if (!string.IsNullOrEmpty(sd.ToolTip))
                {
                    sText = sd.ToolTip;
                }
                //else if (!string.IsNullOrEmpty(sd.Text))
                //{
                //    sText = sd.Text;
                //}
                else
                {
                    string sTextAddition = "";
                    if (!string.IsNullOrEmpty(sd.Text))
                        sTextAddition = " [" + sd.Text + "]";
                    sText = sd.ShortName + sTextAddition;
                }

                lst = findSubstringOccurrences(sText.ToLower(), txtSearch.Text.ToLower());
                foreach(Tuple<int, int> t in lst)
                {
                    float start = g.MeasureString(sText.Substring(0, t.Item1), listBox.Font, int.MaxValue, sf).Width;
                    float width = g.MeasureString(sText.Substring(t.Item1, txtSearch.Text.Length), listBox.Font, int.MaxValue, sf).Width;
                    Rectangle newRect = new Rectangle(e.Bounds.X + (int)start, e.Bounds.Y, (int)(width), 20);
                    g.FillRectangle(new SolidBrush(Color.Yellow), newRect);
                }

                g.DrawString(sText, listBox.Font, new SolidBrush(textColor), e.Bounds.X, yPos, sf);
            }
            sf.Dispose();

            //g.DrawRectangle(Pens.Gray, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
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

            this.BringToFront();
            lstResults.Focus();
        }

        private void selectRequiredTabs(Control parentControl, Control targetControl)
        {
            Control currentControl = targetControl;
            while (currentControl != parentControl)
            {
                currentControl = currentControl.Parent;
                if (currentControl is TabPage tabPage)
                {
                    if (tabPage.Parent is TabControl tabControl)
                    {
                        tabControl.SelectedTab = tabPage;
                    }
                }
            }
        }

        private void chkFullDetails_CheckedChanged(object sender, EventArgs e)
        {
            _fullDetails = chkFullDetails.Checked;

            txtSearch_TextChanged(this, EventArgs.Empty);
        }
    }
}
