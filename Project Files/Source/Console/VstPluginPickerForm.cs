using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis
{
    public sealed class VstPluginPickerForm : Form
    {
        private sealed class QueuedProgress : IProgress<VstCatalogScanUpdate>
        {
            private readonly Action<VstCatalogScanUpdate> _reportAction;

            public QueuedProgress(Action<VstCatalogScanUpdate> reportAction)
            {
                _reportAction = reportAction;
            }

            public void Report(VstCatalogScanUpdate value)
            {
                if (_reportAction != null)
                    _reportAction(value);
            }
        }

        private const int MaxStatusLines = 200;
        private const int MaxFlushMessagesPerTick = 40;
        private readonly Label _statusLabel;
        private readonly TextBox _filterTextBox;
        private readonly ListBox _searchPathsListBox;
        private readonly ListView _pluginListView;
        private readonly TextBox _statusTextBox;
        private readonly Button _addPathButton;
        private readonly Button _removePathButton;
        private readonly Button _scanButton;
        private readonly Button _rescanAllButton;
        private readonly Button _toggleUnavailableButton;
        private readonly Button _loadFileButton;
        private readonly Button _loadFolderButton;
        private readonly Button _okButton;
        private readonly Button _cancelButton;
        private readonly System.Windows.Forms.Timer _statusFlushTimer;
        private readonly ConcurrentQueue<string> _pendingStatusMessages;
        private readonly ConcurrentQueue<VstCatalogScanUpdate> _pendingCatalogUpdates;
        private readonly Queue<string> _statusHistory;
        private VstPluginCatalogFile _catalog;
        private CancellationTokenSource _scanCancellation;
        private bool _scanning;
        private bool _showUnavailablePlugins = true;
        private int _sortColumn = -1;
        private bool _sortAscending = true;

        public string SelectedPluginPath { get; private set; }

        public VstPluginPickerForm()
        {
            Text = "Add VST Plugin";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(920, 520);
            Size = new Size(1120, 660);
            ShowInTaskbar = false;

            TableLayoutPanel rootPanel = new TableLayoutPanel();
            rootPanel.ColumnCount = 1;
            rootPanel.RowCount = 6;
            rootPanel.Dock = DockStyle.Fill;
            rootPanel.Padding = new Padding(8);
            rootPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
            rootPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _statusLabel = new Label();
            _statusLabel.AutoSize = true;
            _statusLabel.Dock = DockStyle.Fill;
            _statusLabel.Padding = new Padding(0, 0, 0, 8);

            FlowLayoutPanel filterPanel = new FlowLayoutPanel();
            filterPanel.AutoSize = true;
            filterPanel.Dock = DockStyle.Fill;
            filterPanel.WrapContents = false;
            filterPanel.Controls.Add(new Label { AutoSize = true, Margin = new Padding(0, 8, 6, 0), Text = "Filter:" });

            _filterTextBox = new TextBox();
            _filterTextBox.Width = 320;
            _filterTextBox.TextChanged += delegate { RefreshPluginList(); };
            filterPanel.Controls.Add(_filterTextBox);

            _toggleUnavailableButton = new Button { AutoSize = true, Text = "Hide Unavailable" };
            _toggleUnavailableButton.Click += delegate { ToggleUnavailableVisibility(); };
            filterPanel.Controls.Add(_toggleUnavailableButton);

            TableLayoutPanel pathsPanel = new TableLayoutPanel();
            pathsPanel.ColumnCount = 1;
            pathsPanel.RowCount = 3;
            pathsPanel.Dock = DockStyle.Fill;
            pathsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pathsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pathsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            pathsPanel.Controls.Add(new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 6),
                Text = "VST Search Paths"
            }, 0, 0);

            _searchPathsListBox = new ListBox();
            _searchPathsListBox.Dock = DockStyle.Fill;
            pathsPanel.Controls.Add(_searchPathsListBox, 0, 1);

            FlowLayoutPanel pathButtonPanel = new FlowLayoutPanel();
            pathButtonPanel.AutoSize = true;
            pathButtonPanel.Dock = DockStyle.Fill;
            pathButtonPanel.WrapContents = false;

            _addPathButton = new Button { AutoSize = true, Text = "Add Path..." };
            _addPathButton.Click += delegate { AddSearchPath(); };
            pathButtonPanel.Controls.Add(_addPathButton);

            _removePathButton = new Button { AutoSize = true, Text = "Remove Path" };
            _removePathButton.Click += delegate { RemoveSelectedSearchPath(); };
            pathButtonPanel.Controls.Add(_removePathButton);

            _scanButton = new Button { AutoSize = true, Text = "Scan Plugins" };
            _scanButton.Click += delegate { ScanPlugins(); };
            pathButtonPanel.Controls.Add(_scanButton);

            _rescanAllButton = new Button { AutoSize = true, Text = "Rescan All" };
            _rescanAllButton.Click += delegate { RescanAllPlugins(); };
            pathButtonPanel.Controls.Add(_rescanAllButton);

            pathsPanel.Controls.Add(pathButtonPanel, 0, 2);

            TableLayoutPanel pluginsPanel = new TableLayoutPanel();
            pluginsPanel.ColumnCount = 1;
            pluginsPanel.RowCount = 2;
            pluginsPanel.Dock = DockStyle.Fill;
            pluginsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pluginsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            pluginsPanel.Controls.Add(new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 6),
                Text = "Cached Audio Effects"
            }, 0, 0);

            _pluginListView = new ListView();
            _pluginListView.Dock = DockStyle.Fill;
            _pluginListView.FullRowSelect = true;
            _pluginListView.GridLines = true;
            _pluginListView.HideSelection = false;
            _pluginListView.MultiSelect = false;
            _pluginListView.View = View.Details;
            _pluginListView.Columns.Add("Plugin", 220);
            _pluginListView.Columns.Add("Vendor", 160);
            _pluginListView.Columns.Add("Version", 90);
            _pluginListView.Columns.Add("Status", 90);
            _pluginListView.Columns.Add("Categories", 170);
            _pluginListView.Columns.Add("Path", 420);
            _pluginListView.SelectedIndexChanged += delegate { UpdateSelection(); };
            _pluginListView.DoubleClick += delegate { ConfirmSelection(); };
            _pluginListView.ColumnClick += delegate(object s, ColumnClickEventArgs e) { SortByColumn(e.Column); };
            pluginsPanel.Controls.Add(_pluginListView, 0, 1);

            _statusTextBox = new TextBox();
            _statusTextBox.Dock = DockStyle.Fill;
            _statusTextBox.Multiline = true;
            _statusTextBox.ReadOnly = true;
            _statusTextBox.ScrollBars = ScrollBars.Vertical;
            _statusTextBox.Text = "Ready.";

            _pendingStatusMessages = new ConcurrentQueue<string>();
            _pendingCatalogUpdates = new ConcurrentQueue<VstCatalogScanUpdate>();
            _statusHistory = new Queue<string>();
            _statusFlushTimer = new System.Windows.Forms.Timer();
            _statusFlushTimer.Interval = 100;
            _statusFlushTimer.Tick += delegate { FlushPendingUpdates(); };
            _statusFlushTimer.Start();

            TableLayoutPanel footerPanel = new TableLayoutPanel();
            footerPanel.ColumnCount = 2;
            footerPanel.RowCount = 1;
            footerPanel.AutoSize = true;
            footerPanel.Dock = DockStyle.Fill;
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            GroupBox manualLoadGroup = new GroupBox();
            manualLoadGroup.AutoSize = true;
            manualLoadGroup.Dock = DockStyle.Left;
            manualLoadGroup.Text = "Manual Load";

            FlowLayoutPanel manualLoadPanel = new FlowLayoutPanel();
            manualLoadPanel.AutoSize = true;
            manualLoadPanel.Dock = DockStyle.Fill;
            manualLoadPanel.WrapContents = false;
            manualLoadPanel.Padding = new Padding(6, 4, 6, 6);

            _loadFileButton = new Button { AutoSize = true, Text = "Load VST3 From File..." };
            _loadFileButton.Click += delegate { SelectVst3File(); };
            manualLoadPanel.Controls.Add(_loadFileButton);

            _loadFolderButton = new Button { AutoSize = true, Text = "Load VST3 From Folder..." };
            _loadFolderButton.Click += delegate { SelectVst3Folder(); };
            manualLoadPanel.Controls.Add(_loadFolderButton);

            manualLoadGroup.Controls.Add(manualLoadPanel);
            footerPanel.Controls.Add(manualLoadGroup, 0, 0);

            FlowLayoutPanel actionPanel = new FlowLayoutPanel();
            actionPanel.AutoSize = true;
            actionPanel.Dock = DockStyle.Right;
            actionPanel.FlowDirection = FlowDirection.RightToLeft;
            actionPanel.WrapContents = false;

            _cancelButton = new Button { AutoSize = true, DialogResult = DialogResult.Cancel, Text = "Cancel" };
            actionPanel.Controls.Add(_cancelButton);

            _okButton = new Button { AutoSize = true, Text = "Add VST3" };
            _okButton.Click += delegate { ConfirmSelection(); };
            actionPanel.Controls.Add(_okButton);

            footerPanel.Controls.Add(actionPanel, 1, 0);

            rootPanel.Controls.Add(_statusLabel, 0, 0);
            rootPanel.Controls.Add(filterPanel, 0, 1);
            rootPanel.Controls.Add(pathsPanel, 0, 2);
            rootPanel.Controls.Add(pluginsPanel, 0, 3);
            rootPanel.Controls.Add(_statusTextBox, 0, 4);
            rootPanel.Controls.Add(footerPanel, 0, 5);
            Controls.Add(rootPanel);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;

            Shown += delegate { LoadCatalog(); };
            FormClosing += delegate { CancelScan(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelScan();
                _statusFlushTimer.Stop();
                _statusFlushTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        private void LoadCatalog()
        {
            _catalog = VstHost.LoadPluginCatalog();
            BindCatalog();
        }

        private void BindCatalog()
        {
            _searchPathsListBox.BeginUpdate();
            _searchPathsListBox.Items.Clear();
            if (_catalog != null && _catalog.SearchPaths != null)
            {
                for (int i = 0; i < _catalog.SearchPaths.Count; i++)
                    _searchPathsListBox.Items.Add(_catalog.SearchPaths[i]);
            }
            _searchPathsListBox.EndUpdate();

            RefreshPluginList();
            UpdateStatus();
            UpdateSelection();
        }

        private void RefreshPluginList()
        {
            string filterText = (_filterTextBox.Text ?? string.Empty).Trim();

            _pluginListView.BeginUpdate();
            _pluginListView.Items.Clear();

            if (_catalog != null && _catalog.Plugins != null)
            {
                for (int i = 0; i < _catalog.Plugins.Count; i++)
                {
                    VstCatalogPlugin plugin = _catalog.Plugins[i];

                    if (!_showUnavailablePlugins && !plugin.Available)
                        continue;
                    if (!MatchesFilter(plugin, filterText))
                        continue;

                    ListViewItem item = new ListViewItem(VstHost.GetCatalogPluginDisplayName(plugin));
                    item.SubItems.Add(plugin.Vendor ?? string.Empty);
                    item.SubItems.Add(plugin.Version ?? string.Empty);
                    item.SubItems.Add(plugin.Status ?? string.Empty);
                    item.SubItems.Add(plugin.Subcategories ?? string.Empty);
                    item.SubItems.Add(plugin.Path ?? string.Empty);
                    item.Tag = plugin;
                    if (!plugin.Available)
                        item.ForeColor = SystemColors.GrayText;
                    _pluginListView.Items.Add(item);
                }
            }

            _pluginListView.EndUpdate();
        }

        private void UpdateStatus()
        {
            int pluginCount = _catalog != null && _catalog.Plugins != null ? _catalog.Plugins.Count : 0;
            int availableCount = 0;
            int searchPathCount = _catalog != null && _catalog.SearchPaths != null ? _catalog.SearchPaths.Count : 0;
            string lastScanText = "Not scanned yet.";
            string nativeStatusPrefix = string.Empty;

            if (_catalog != null && !string.IsNullOrWhiteSpace(_catalog.LastScanUtc))
            {
                DateTime lastScanUtc;
                if (DateTime.TryParse(_catalog.LastScanUtc, out lastScanUtc))
                    lastScanText = "Last scan: " + lastScanUtc.ToLocalTime().ToString("g");
            }

            if (_catalog != null && _catalog.Plugins != null)
            {
                for (int i = 0; i < _catalog.Plugins.Count; i++)
                {
                    if (_catalog.Plugins[i] != null && _catalog.Plugins[i].Available)
                        availableCount++;
                }
            }

            if (!VstHost.NativeAvailable || !VstHost.SdkAvailable)
                nativeStatusPrefix = VstHost.NativeStatusText + " ";

            _statusLabel.Text = string.Format(
                "{0}Manage VST3 search paths, scan audio effects, then choose from the cached list. {1} {2} available plugin{3} cached ({4} total entries) across {5} path{6}.",
                nativeStatusPrefix,
                lastScanText,
                availableCount,
                availableCount == 1 ? string.Empty : "s",
                pluginCount,
                searchPathCount,
                searchPathCount == 1 ? string.Empty : "s");
        }

        private void UpdateSelection()
        {
            _removePathButton.Enabled = _searchPathsListBox.SelectedIndex >= 0 && !_scanning;
            _scanButton.Enabled = !_scanning;
            _rescanAllButton.Enabled = !_scanning;
            _addPathButton.Enabled = !_scanning;
            _toggleUnavailableButton.Enabled = !_scanning;
            _loadFileButton.Enabled = !_scanning;
            _loadFolderButton.Enabled = !_scanning;
            _toggleUnavailableButton.Text = _showUnavailablePlugins ? "Hide Unavailable" : "Show Unavailable";
            VstCatalogPlugin selectedPlugin = GetSelectedPlugin();
            _okButton.Enabled = selectedPlugin != null && selectedPlugin.Available && !_scanning;
        }

        private void ToggleUnavailableVisibility()
        {
            _showUnavailablePlugins = !_showUnavailablePlugins;
            RefreshPluginList();
            UpdateSelection();
        }

        private void AddSearchPath()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder to scan for VST3 plugins.";
                dialog.SelectedPath = _searchPathsListBox.SelectedItem as string ?? GetInitialSearchPath();
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                EnsureCatalog();
                string selectedPath = dialog.SelectedPath;
                for (int i = 0; i < _catalog.SearchPaths.Count; i++)
                {
                    if (string.Equals(_catalog.SearchPaths[i], selectedPath, StringComparison.OrdinalIgnoreCase))
                        return;
                }

                if (string.IsNullOrWhiteSpace(selectedPath))
                    return;

                _catalog.SearchPaths.Add(selectedPath);
                VstHost.SavePluginCatalog(_catalog);
                BindCatalog();
            }
        }

        private void RemoveSelectedSearchPath()
        {
            int selectedIndex = _searchPathsListBox.SelectedIndex;
            if (selectedIndex < 0 || _catalog == null || _catalog.SearchPaths == null || selectedIndex >= _catalog.SearchPaths.Count)
                return;

            _catalog.SearchPaths.RemoveAt(selectedIndex);
            VstHost.SavePluginCatalog(_catalog);
            BindCatalog();
        }

        private void ScanPlugins()
        {
            RunScan(false);
        }

        private void RescanAllPlugins()
        {
            if (MessageBox.Show(
                this,
                "This will rebuild the entire VST plugin cache from scratch. Continue?",
                "VST Plugins",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            RunScan(true);
        }

        private void RunScan(bool forceRescanAll)
        {
            List<string> searchPaths = new List<string>();
            IProgress<VstCatalogScanUpdate> scanProgress;
            CancellationTokenSource scanCancellation;
            Task<VstPluginCatalogFile> scanTask;

            EnsureCatalog();
            for (int i = 0; i < _catalog.SearchPaths.Count; i++)
                searchPaths.Add(_catalog.SearchPaths[i]);

            _catalog.Plugins.Clear();
            _catalog.LastScanUtc = null;

            _scanning = true;
            UseWaitCursor = true;
            UpdateSelection();
            RefreshPluginList();
            UpdateStatus();
            _statusLabel.Text = forceRescanAll
                ? "Rescanning all configured VST3 search paths for audio effects..."
                : "Scanning configured VST3 search paths for audio effects...";
            AppendStatus(forceRescanAll ? "Starting full VST rescan..." : "Starting VST scan...");
            scanProgress = new QueuedProgress(EnqueueScanUpdate);
            scanCancellation = new CancellationTokenSource();
            _scanCancellation = scanCancellation;

            scanTask = Task.Run(delegate
            {
                scanCancellation.Token.ThrowIfCancellationRequested();
                if (forceRescanAll)
                    VstHost.DeletePluginCatalog();

                return VstHost.ScanPluginCatalog(searchPaths, scanProgress, forceRescanAll, scanCancellation.Token);
            }, scanCancellation.Token);
            scanTask.ContinueWith(task =>
            {
                if (ReferenceEquals(_scanCancellation, scanCancellation))
                    _scanCancellation = null;
                scanCancellation.Dispose();

                if (IsDisposed || Disposing)
                    return;

                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (IsDisposed || Disposing)
                            return;

                        _scanning = false;
                        UseWaitCursor = false;

                        if (task.IsCanceled)
                        {
                            AppendStatus(forceRescanAll ? "Full rescan canceled." : "Scan canceled.");
                            FlushPendingUpdates();
                            UpdateSelection();
                            return;
                        }

                        if (task.IsFaulted)
                        {
                            string errorText = task.Exception != null && task.Exception.GetBaseException() != null
                                ? task.Exception.GetBaseException().Message
                                : "Unknown scanner error.";
                            AppendStatus((forceRescanAll ? "Full rescan failed: " : "Scan failed: ") + errorText);
                            MessageBox.Show(this, "The VST plugin scan failed.\r\n\r\n" + errorText, "VST Plugins", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            _catalog = task.Result ?? new VstPluginCatalogFile();
                            AppendStatus(forceRescanAll ? "Full rescan complete. Catalog rebuilt." : "Catalog updated.");
                        }

                        BindCatalog();
                        FlushPendingUpdates();
                    });
                }
                catch (InvalidOperationException)
                {
                    // Form was disposed between check and BeginInvoke — expected during shutdown.
                }
            }, TaskScheduler.Default);
        }

        private void CancelScan()
        {
            if (_scanCancellation != null)
            {
                try
                {
                    _scanCancellation.Cancel();
                }
                catch
                {
                }
            }
        }

        private void ConfirmSelection()
        {
            VstCatalogPlugin plugin = GetSelectedPlugin();
            if (plugin == null)
                return;
            if (!plugin.Available)
                return;

            AppendStatus("Adding plugin: " + VstHost.GetCatalogPluginDisplayName(plugin));
            SelectedPluginPath = plugin.Path;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void SelectVst3File()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select a VST3 plugin file";
                dialog.Filter = "VST3 Plugin (*.vst3)|*.vst3|All Files (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (!string.Equals(System.IO.Path.GetExtension(dialog.FileName), ".vst3", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(this, "Select a .vst3 plugin file.", "VST Plugins", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppendStatus("Adding VST3 file: " + dialog.FileName);
                SelectedPluginPath = dialog.FileName;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void SelectVst3Folder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a VST3 plugin bundle folder.";
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (!dialog.SelectedPath.EndsWith(".vst3", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(this, "Select a .vst3 bundle folder.", "VST Plugins", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppendStatus("Adding VST3 folder: " + dialog.SelectedPath);
                SelectedPluginPath = dialog.SelectedPath;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void EnsureCatalog()
        {
            if (_catalog == null)
                _catalog = new VstPluginCatalogFile();
            if (_catalog.SearchPaths == null)
                _catalog.SearchPaths = VstHost.GetDefaultPluginSearchPaths();
            if (_catalog.Plugins == null)
                _catalog.Plugins = new List<VstCatalogPlugin>();
        }

        private string GetInitialSearchPath()
        {
            List<string> defaultPaths = VstHost.GetDefaultPluginSearchPaths();
            return defaultPaths.Count > 0 ? defaultPaths[0] : string.Empty;
        }

        private VstCatalogPlugin GetSelectedPlugin()
        {
            if (_pluginListView.SelectedItems.Count == 0)
                return null;

            return _pluginListView.SelectedItems[0].Tag as VstCatalogPlugin;
        }

        private static bool MatchesFilter(VstCatalogPlugin plugin, string filterText)
        {
            if (plugin == null)
                return false;
            if (string.IsNullOrWhiteSpace(filterText))
                return true;

            return ContainsIgnoreCase(plugin.Name, filterText) ||
                   ContainsIgnoreCase(plugin.Vendor, filterText) ||
                   ContainsIgnoreCase(plugin.Version, filterText) ||
                   ContainsIgnoreCase(plugin.Subcategories, filterText) ||
                   ContainsIgnoreCase(plugin.Path, filterText);
        }

        private static bool ContainsIgnoreCase(string value, string filterText)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(filterText))
                return false;

            return value.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void AppendStatus(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            _pendingStatusMessages.Enqueue(DateTime.Now.ToString("HH:mm:ss") + "  " + message);
        }

        private void EnqueueScanUpdate(VstCatalogScanUpdate update)
        {
            if (update != null)
                _pendingCatalogUpdates.Enqueue(update);
        }

        private void FlushPendingUpdates()
        {
            int flushed = 0;
            bool changed = false;
            bool pluginsChanged = false;

            while (flushed < MaxFlushMessagesPerTick && _pendingCatalogUpdates.TryDequeue(out VstCatalogScanUpdate update))
            {
                if (!string.IsNullOrWhiteSpace(update.Message))
                    _pendingStatusMessages.Enqueue(DateTime.Now.ToString("HH:mm:ss") + "  " + update.Message);

                if (update.Plugin != null)
                {
                    UpsertScannedPlugin(update.Plugin);
                    pluginsChanged = true;
                }

                flushed++;
            }

            flushed = 0;
            while (flushed < MaxFlushMessagesPerTick && _pendingStatusMessages.TryDequeue(out string message))
            {
                _statusHistory.Enqueue(message);
                while (_statusHistory.Count > MaxStatusLines)
                    _statusHistory.Dequeue();
                flushed++;
                changed = true;
            }

            if (!changed && !pluginsChanged)
                return;

            if (changed)
            {
                _statusTextBox.Lines = _statusHistory.ToArray();
                _statusTextBox.SelectionStart = _statusTextBox.TextLength;
                _statusTextBox.ScrollToCaret();
            }

            if (pluginsChanged)
            {
                RefreshPluginList();
                UpdateStatus();
                UpdateSelection();
            }
        }

        private void SortByColumn(int column)
        {
            if (column == _sortColumn)
                _sortAscending = !_sortAscending;
            else
            {
                _sortColumn = column;
                _sortAscending = true;
            }

            _pluginListView.ListViewItemSorter = new ListViewColumnComparer(_sortColumn, _sortAscending);
            _pluginListView.Sort();
        }

        private sealed class ListViewColumnComparer : IComparer
        {
            private readonly int _column;
            private readonly int _direction;

            public ListViewColumnComparer(int column, bool ascending)
            {
                _column = column;
                _direction = ascending ? 1 : -1;
            }

            public int Compare(object x, object y)
            {
                ListViewItem a = x as ListViewItem;
                ListViewItem b = y as ListViewItem;
                if (a == null || b == null) return 0;

                string textA = _column < a.SubItems.Count ? a.SubItems[_column].Text : string.Empty;
                string textB = _column < b.SubItems.Count ? b.SubItems[_column].Text : string.Empty;

                return _direction * string.Compare(textA, textB, StringComparison.OrdinalIgnoreCase);
            }
        }

        private void UpsertScannedPlugin(VstCatalogPlugin plugin)
        {
            int i;

            if (plugin == null)
                return;

            EnsureCatalog();
            for (i = 0; i < _catalog.Plugins.Count; i++)
            {
                if (string.Equals(_catalog.Plugins[i].Path, plugin.Path, StringComparison.OrdinalIgnoreCase))
                {
                    _catalog.Plugins[i] = plugin;
                    return;
                }
            }

            _catalog.Plugins.Add(plugin);
        }
    }
}
