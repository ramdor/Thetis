/*  frmAbout.cs

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
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Thetis
{
    public partial class frmAbout : Form
    {
        private const string GITHUB_VERSION_JSON_RAW = @"https://raw.githubusercontent.com/ramdor/Thetis/refs/heads/master/version.json";

        private class ThetisVersionInfo
        {
            public string ReleaseVersion { get; set; }
            public string ReleaseBuild { get; set; }
            public string ReleaseURL { get; set; }
            public string ReleaseName { get; set; }
            public string DevelopmentVersion { get; set; }
            public string DevelopmentBuild { get; set; }
            public string DevelopmentURL { get; set; }
            public string DevelopmentName { get; set; }
        }

        private ThetisVersionInfo _versionInfo;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _fetchJsonTask;
        private readonly object _version_info_lock = new object();
        private string _version;
        private string _build;
        private bool _update_available;
        private Console _console;
        private bool _check_dev_version;
        private string _exe_path;

        public frmAbout(Console console, bool check_dev_version)
        {
            InitializeComponent();

            this.TopMost = true;

            _exe_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
            _console = console;
            _update_available = false;
            _versionInfo = null;
            _version = "";
            _build = "";
            _check_dev_version = check_dev_version;

            btnVisit.Enabled = false;
            btnUpdatedRelease.Visible = false;

            _cancellationTokenSource = new CancellationTokenSource();
            _fetchJsonTask = fetchJsonAsync(_cancellationTokenSource.Token);
        }
        public void InitVersions(string version, string build, string db_version, string dx_version, string radio_model, string firmware_version, string protocol, string supported_protocol, string wdsp_version, string channel_master_version, string cmASIO_version, string portAudio_version, string andromeda_version)
        {
            _version = version;
            _build = build;

            cancelFetchJsonTask();

            if (_cancellationTokenSource == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _fetchJsonTask = fetchJsonAsync(_cancellationTokenSource.Token);
            }

            lstLinks.ClearSelected();
            btnVisit.Enabled = false;

            if (string.IsNullOrEmpty(firmware_version)) firmware_version = "?";
            if (string.IsNullOrEmpty(radio_model)) firmware_version = "?";

            if (radio_model == HPSDRModel.FIRST.ToString() || radio_model == HPSDRModel.LAST.ToString()) radio_model = "?";

            if (!string.IsNullOrEmpty(build))
            {
                build = build.Left(16);
                version += $" [build {build}]";
            }

            lstVersions.Items.Clear();
            lstVersions.Items.Add("Version: " + version);
            lstVersions.Items.Add("Database Version: " + db_version);
            lstVersions.Items.Add("Radio Model: " + radio_model);
            if (!string.IsNullOrEmpty(andromeda_version)) lstVersions.Items.Add(andromeda_version); // includes the version: preamble in the string
            lstVersions.Items.Add("Firmware Version: " + firmware_version);
            string support = !string.IsNullOrEmpty(supported_protocol) ? $" (v{supported_protocol})" : "";
            lstVersions.Items.Add("Protocol: " + protocol + support);
            lstVersions.Items.Add("WDSP Version: " + wdsp_version);
            lstVersions.Items.Add("ChannelMaster: " + channel_master_version);
            lstVersions.Items.Add("cmASIO Version: " + cmASIO_version);
            lstVersions.Items.Add("PortAudio Version: " + portAudio_version);
            if(!string.IsNullOrEmpty(firmware_version)) lstVersions.Items.Add("DirectX Version: " + dx_version);
        }
        private bool UpdateAvaialble
        {
            get
            {
                lock (_version_info_lock)
                {
                    return _update_available;
                }
            }
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void btnCopyContributors_Click(object sender, EventArgs e)
        {
            string text = "Thetis\n";
            foreach(string s in lstVersions.Items)
            {
                text += s + "\n";
            }
            
            try
            {
                Clipboard.SetText(text);
            }
            catch { }
        }

        private void btnSysInfo_Click(object sender, EventArgs e)
        {
            Common.OpenUri("msinfo32.exe", false);
        }

        private void btnDXDiag_Click(object sender, EventArgs e)
        {
            Common.OpenUri("dxdiag.exe", false);
        }

        private void lnkLicence_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkLicence.LinkVisited = Common.OpenUri(@"https://github.com/ramdor/Thetis/blob/master/LICENSE");
        }

        private void btnVisit_Click(object sender, EventArgs e)
        {
            if (lstLinks.SelectedItems.Count == 0)
            {
                btnVisit.Enabled = false;
                return;
            }

            switch (lstLinks.SelectedIndex)
            {
                case 0: Common.OpenUri("https://github.com/ramdor/Thetis/releases"); break;
                case 1: Common.OpenUri("https://discord.gg/6fHCRKnDc9"); break;
                case 2: Common.OpenUri("https://community.apache-labs.com/index.php"); break;
                case 3: Common.OpenUri("https://apache-labs.com/"); break;
                case 4: Common.OpenUri("https://github.com/TAPR/OpenHPSDR-Protocol1-Programmers"); break;
                case 5: Common.OpenUri("https://github.com/TAPR/OpenHPSDR-Protocol2-Programmers"); break;
                case 6: Common.OpenUri("https://community.apache-labs.com/viewforum.php?f=23"); break;
                case 7: Common.OpenUri("https://community.apache-labs.com/viewtopic.php?f=27&t=3080"); break;
                case 8: Common.OpenUri("https://community.apache-labs.com/viewtopic.php?f=32&t=4972"); break;
                case 9: Common.OpenUri("https://github.com/laurencebarker/Saturn"); break;
                case 10: Common.OpenUri("https://github.com/mi0bot/OpenHPSDR-Thetis/releases"); break;
                case 11: Common.OpenUri("https://github.com/TAPR/OpenHPSDR-wdsp"); break;
                case 12: Common.OpenUri("https://www.oe3ide.com/wp/software/"); break;
                case 13: break; // splitter
                case 14: Common.OpenUri($"file://{_exe_path}Thetis manual.pdf", false); break;
                case 15: Common.OpenUri($"file://{_exe_path}Thetis-CAT-Command-Reference-Guide-V3.pdf", false); break;
                case 16: Common.OpenUri($"file://{_exe_path}PureSignal.pdf", false); break;
                case 17: Common.OpenUri($"file://{_exe_path}Midi2Cat_Instructions_V3.pdf", false); break;
                case 18: Common.OpenUri($"file://{_exe_path}cmASIO Guide.pdf", false); break;
                case 19: Common.OpenUri($"file://{_exe_path}BehringerMods_Midi2Cat_v2.pdf", false); break;
                case 20: Common.OpenUri($"file://{_exe_path}APFtypes.pdf", false); break;
            }

            lstLinks.ClearSelected();
            btnVisit.Enabled = false;
        }

        private void lstLinks_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnVisit.Enabled = lstLinks.SelectedItems.Count > 0;
        }
        private void handleVersionInfo()
        {
            lock (_version_info_lock)
            {
                int release_version = Common.CompareVersions(_version, _versionInfo.ReleaseVersion);
                bool different_release_build = !string.IsNullOrEmpty(_versionInfo.ReleaseBuild) && _versionInfo.ReleaseBuild != _build;
                if (release_version < 0 || (release_version == 0 && different_release_build))
                {
                    btnUpdatedRelease.Text = $"Release version [{_versionInfo.ReleaseVersion}]\n{_versionInfo.ReleaseName}\nClick to view on GitHub";
                    btnUpdatedRelease.Tag = _versionInfo.ReleaseURL;
                    btnUpdatedRelease.Visible = true;
                    _update_available = true;
                }
                else if(_check_dev_version)
                {
                    // check development
                    int development_version = Common.CompareVersions(_version, _versionInfo.DevelopmentVersion);
                    bool different_dev_build = !string.IsNullOrEmpty(_versionInfo.DevelopmentBuild) && _versionInfo.DevelopmentBuild != _build;
                    if (development_version < 0 || (development_version == 0 && different_dev_build))
                    {
                        string build = string.IsNullOrEmpty(_versionInfo.DevelopmentBuild) ? "" : $" {_versionInfo.DevelopmentBuild}";
                        string where = _versionInfo.DevelopmentURL != null && _versionInfo.DevelopmentURL.Contains("discord", StringComparison.OrdinalIgnoreCase) ? "Discord" : "GitHub";
                        btnUpdatedRelease.Text = $"Dev version [{_versionInfo.DevelopmentVersion}{build}]\n{_versionInfo.DevelopmentName}\nClick to view on {where}";
                        btnUpdatedRelease.Tag = _versionInfo.DevelopmentURL;
                        btnUpdatedRelease.Visible = true;
                        _update_available = true;
                    }
                    else
                    {
                        btnUpdatedRelease.Visible = false;
                        _update_available = false;
                    }
                }
            }
        }
        private void cancelFetchJsonTask()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
        private async Task fetchJsonAsync(CancellationToken cancellationToken)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
                };

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(GITHUB_VERSION_JSON_RAW, cancellationToken);
                        response.EnsureSuccessStatusCode();

                        string jsonData = await response.Content.ReadAsStringAsync();

                        lock (_version_info_lock)
                        {
                            _versionInfo = JsonConvert.DeserializeObject<ThetisVersionInfo>(jsonData);
                        }

                        if (this.IsHandleCreated)
                        {
                            this.Invoke(new Action(() =>
                            {
                                handleVersionInfo();
                            }));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(30), cancellationToken); // check every 30 mins
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        private void btnUpdatedRelease_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(btnUpdatedRelease.Tag.ToString())) return;
            Common.OpenUri(btnUpdatedRelease.Tag.ToString());
        }

        private void btnReleaseNotes_Click(object sender, EventArgs e)
        {
            if (_console != null)
                _console.ShowReleaseNotes();
        }
    }
}
