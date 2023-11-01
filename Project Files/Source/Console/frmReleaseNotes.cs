using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Markdig;

namespace Thetis
{
    public partial class frmReleaseNotes : Form
    {
        private string _releaseNotesPath;

        public frmReleaseNotes()
        {
            InitializeComponent();
            webBrowser1.Navigating += WebBrowser1_Navigating;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void InitPath(string directoryPath)
        {
            _releaseNotesPath = directoryPath;//Path.Combine(directoryPath, "ReleaseNotes.txt");
        }

        public void ShowReleaseNotes()
        {
            try
            {
                Opacity = 0f;
                string file = Path.Combine(_releaseNotesPath, "ReleaseNotes.txt");
                if (!File.Exists(file)) return;
                string releaseNotesText = File.ReadAllText(file);
                string formattedText = Markdown.ToHtml(releaseNotesText);
                string finalHtml = $"<html><head><style>body{{font-family: Arial, sans-serif; background-color: black; color: white;}}</style></head><body>{formattedText}</body></html>";
                webBrowser1.DocumentText = finalHtml;
                Show();
                Common.FadeIn(this);
            }
            catch(Exception ex) 
            {
                MessageBox.Show("Issue showing Release Notes",
                     ex.Message,
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }
        }
        private void WebBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.ToString().ToLower() == "about:blank") return;
            e.Cancel = true;
            string url = e.Url.ToString();
            Common.OpenUri(url);
        }

        private void frmReleaseNotes_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }
    }
}
