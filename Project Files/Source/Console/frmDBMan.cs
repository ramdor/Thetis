/*  frmDBMan.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2024 Richard Samphire MW0LGE

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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis
{
    public partial class frmDBMan : Form
    {
        private bool _allow_check_change;

        public frmDBMan()
        {
            InitializeComponent();
            _allow_check_change = true;
        }
        public void Restore()
        {
            Common.RestoreForm(this, "DBManForm", true);
        }
        internal void InitAvailableDBs(Dictionary<Guid, DBMan.DabataseInfo> dbs, Guid active_guid)
        {
            bool old_allow_check = _allow_check_change;
            _allow_check_change = true;

            lstActiveDBs.Items.Clear();
            foreach (KeyValuePair<Guid, DBMan.DabataseInfo> kvp in dbs)
            {
                DBMan.DabataseInfo dbi = kvp.Value;

                ListViewItem lvi = new ListViewItem(dbi.Description);

                lvi.Checked = dbi.GUID == active_guid;

                lvi.SubItems.Add(dbi.Model.ToString());
                lvi.SubItems.Add(dbi.LastChanged.ToString("G"));

                TimeSpan difference = DateTime.Now - dbi.CreationTime;
                string age;
                if(difference.Days > 0)
                    age = $"{difference.Days}d {difference.Hours.ToString("00")}:{difference.Minutes.ToString("00")}:{difference.Seconds.ToString("00")}";
                else
                    age = $"{difference.Hours.ToString("00")}:{difference.Minutes.ToString("00")}:{difference.Seconds.ToString("00")}";

                lvi.SubItems.Add(age);
                lvi.SubItems.Add(getReadableFileSize(dbi.Size));
                lvi.SubItems.Add("no");
                lvi.SubItems.Add("no");
                lvi.SubItems.Add(dbi.FullPath);

                lvi.Tag = dbi.GUID.ToString();

                lstActiveDBs.Items.Add(lvi);
            }
            lstActiveDBs_SelectedIndexChanged(this, EventArgs.Empty);
            _allow_check_change = old_allow_check;
        }

        private string getReadableFileSize(long byteSize)
        {
            if (byteSize >= 1024 * 1024)
            {
                return string.Format("{0:0.00}MB", byteSize / (1024.0 * 1024.0));
            }
            else if (byteSize >= 1024)
            {
                return string.Format("{0:0.00}KB", byteSize / 1024.0);
            }
            else
            {
                return string.Format("{0}B", byteSize);
            }
        }

        private void lstActiveDBs_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // prevent user from clicking the checkbox
            if (_allow_check_change) return;

            e.NewValue = e.CurrentValue;
        }

        private void frmDBMan_Shown(object sender, EventArgs e)
        {
            _allow_check_change = false;
        }

        private void frmDBMan_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Common.SaveForm(this, "DBManForm");

                _allow_check_change = true;
            }
        }

        private void btnMakeActive_Click(object sender, EventArgs e)
        {
            if (lstActiveDBs.SelectedItems.Count != 1) return;

            ListViewItem lvi = lstActiveDBs.SelectedItems[0];

            Guid guid = new Guid(lvi.Tag.ToString());

            DBMan.MakeActiveDB(guid);
        }

        private void btnNewDB_Click(object sender, EventArgs e)
        {
            DBMan.NewDB();
        }

        private void btnDuplicateBD_Click(object sender, EventArgs e)
        {
            if (lstActiveDBs.SelectedItems.Count != 1) return;

            ListViewItem lvi = lstActiveDBs.SelectedItems[0];

            Guid guid = new Guid(lvi.Tag.ToString());

            DBMan.DuplicateDB(guid);
        }

        private void btnRemoveDB_Click(object sender, EventArgs e)
        {
            if (lstActiveDBs.SelectedItems.Count != 1) return;

            ListViewItem lvi = lstActiveDBs.SelectedItems[0];

            Guid guid = new Guid(lvi.Tag.ToString());

            DBMan.RemoveDB(guid);
        }

        private void lstActiveDBs_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enabled = lstActiveDBs.SelectedItems.Count != 0;

            btnNewDB.Enabled = true;
            btnTakeBackupNow.Enabled = true;
            btnDuplicateBD.Enabled = enabled;

            //force disable if on current active
            if (lstActiveDBs.SelectedItems.Count == 1 && lstActiveDBs.SelectedItems[0].Checked)
                enabled = false;

            btnMakeActive.Enabled = enabled;
            btnRemoveDB.Enabled = enabled;
        }

        private void btnTakeBackupNow_Click(object sender, EventArgs e)
        {
            Guid highlighted = Guid.Empty;
            if (lstActiveDBs.SelectedItems.Count == 1)
            {
                ListViewItem lvi = lstActiveDBs.SelectedItems[0];
                highlighted = new Guid(lvi.Tag.ToString());
            }

            DBMan.TakeBackup(highlighted);
        }
    }
}
