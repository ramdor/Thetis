/*  frmTCIAuthPerms.cs

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
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Thetis
{
    public partial class frmTCIAuthPerms : Form
    {
        private sealed class UserItem
        {
            public string Id;
            public string DisplayName;
            public bool Enabled;
            public string Role;
            public List<string> Permissions = new List<string>();
            public string CreatedUtc;
            public string ModifiedUtc;
            public string Notes;
            public bool HasAccessCode;
            public string AccessCode = string.Empty;
            public string AccessCodeConfirm = string.Empty;
            public bool AccessCodeChanged;

            public override string ToString()
            {
                string name = string.IsNullOrWhiteSpace(DisplayName) ? "Unnamed PIN" : DisplayName;
                return name + (Enabled ? string.Empty : " (disabled)");
            }
        }

        private sealed class RoleItem
        {
            public string Id;
            public string DisplayName;
            public List<string> Permissions = new List<string>();

            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(DisplayName) ? Id : DisplayName;
            }
        }

        private sealed class PermissionItem
        {
            public string Id;
            public string DisplayName;
            public string Description;
            public List<string> Commands = new List<string>();

            public override string ToString()
            {
                return string.IsNullOrWhiteSpace(DisplayName) ? Id : DisplayName;
            }
        }

        private const string PIN_MASK_PLACEHOLDER = "hashed";
        private const string GUEST_USER_ID = "guest";
        private const string GUEST_ROLE_ID = "listener";

        private readonly string _permissions_path;
        private readonly List<UserItem> _users = new List<UserItem>();
        private readonly List<RoleItem> _roles = new List<RoleItem>();
        private readonly List<PermissionItem> _permissions = new List<PermissionItem>();
        private bool _loading;

        public frmTCIAuthPerms(string permissions_path)
        {
            InitializeComponent();
            _permissions_path = permissions_path;
            configureReadOnlyControls();
            loadData();
        }

        private void configureReadOnlyControls()
        {
            lblUserId.Visible = false;
            txtUserId.Visible = false;
            lblRoleId.Visible = false;
            txtRoleId.Visible = false;
            lblPermissionId.Visible = false;
            txtPermissionId.Visible = false;

            btnAddRole.Visible = true;
            btnRemoveRole.Visible = true;
            btnAddPermission.Visible = false;
            btnRemovePermission.Visible = false;

            txtRoleDisplayName.ReadOnly = false;
            txtPermissionDisplayName.ReadOnly = true;
            txtPermissionDescription.ReadOnly = true;
            clbRolePermissions.Enabled = true;
            cmbUserRole.DropDownStyle = ComboBoxStyle.DropDownList;

            lblUserDisplayName.Location = lblUserId.Location;
            txtUserDisplayName.Location = txtUserId.Location;
            lblRoleDisplayName.Location = lblRoleId.Location;
            txtRoleDisplayName.Location = txtRoleId.Location;
            lblPermissionDisplayName.Location = lblPermissionId.Location;
            txtPermissionDisplayName.Location = txtPermissionId.Location;

            txtUserDisplayName.Leave += txtUserDisplayName_Leave;
            txtRoleDisplayName.TextChanged += txtRoleDisplayName_TextChanged;
            txtRoleDisplayName.Leave += txtRoleDisplayName_Leave;
            clbRolePermissions.ItemCheck += clbRolePermissions_ItemCheck;
            btnAddRole.Click += btnAddRole_Click;
            btnRemoveRole.Click += btnRemoveRole_Click;
        }

        private void loadData()
        {
            _loading = true;
            _users.Clear();
            _roles.Clear();
            _permissions.Clear();

            List<TCIpermissions.SetupPermission> setup_permissions = TCIpermissions.GetPermissionsForSetup();
            foreach (TCIpermissions.SetupPermission setup_permission in setup_permissions)
            {
                PermissionItem permission = new PermissionItem();
                permission.Id = setup_permission.Id;
                permission.DisplayName = setup_permission.DisplayName;
                permission.Description = setup_permission.Description;
                permission.Commands = setup_permission.Commands == null ? new List<string>() : new List<string>(setup_permission.Commands);
                _permissions.Add(permission);
            }

            List<TCIpermissions.SetupRole> setup_roles = TCIpermissions.GetRolesForSetup();
            foreach (TCIpermissions.SetupRole setup_role in setup_roles)
            {
                RoleItem role = new RoleItem();
                role.Id = setup_role.Id;
                role.DisplayName = setup_role.DisplayName;
                role.Permissions = setup_role.Permissions == null ? new List<string>() : new List<string>(setup_role.Permissions);
                _roles.Add(role);
            }

            List<TCIpermissions.SetupUser> setup_users = TCIpermissions.GetUsersForSetup();
            foreach (TCIpermissions.SetupUser setup_user in setup_users)
            {
                UserItem user = new UserItem();
                user.Id = setup_user.Id;
                user.DisplayName = setup_user.DisplayName;
                user.Enabled = setup_user.Enabled;
                user.Role = isGuestUser(user) ? GUEST_ROLE_ID : setup_user.Role;
                user.Permissions = setup_user.Permissions == null ? new List<string>() : new List<string>(setup_user.Permissions);
                user.CreatedUtc = setup_user.CreatedUtc;
                user.ModifiedUtc = setup_user.ModifiedUtc;
                user.Notes = setup_user.Notes;
                user.HasAccessCode = setup_user.HasAccessCode;
                _users.Add(user);
            }

            refreshPermissionList();
            refreshRoleList();
            refreshUserList();
            _loading = false;

            if (lstUsers.Items.Count > 0) lstUsers.SelectedIndex = 0;
            if (lstRoles.Items.Count > 0) lstRoles.SelectedIndex = 0;
            if (lstPermissions.Items.Count > 0) lstPermissions.SelectedIndex = 0;
        }

        private UserItem selectedUser()
        {
            return lstUsers.SelectedItem as UserItem;
        }

        private RoleItem selectedRole()
        {
            return lstRoles.SelectedItem as RoleItem;
        }

        private PermissionItem selectedPermission()
        {
            return lstPermissions.SelectedItem as PermissionItem;
        }

        private void refreshUserList()
        {
            object selected = lstUsers.SelectedItem;
            lstUsers.BeginUpdate();
            lstUsers.Items.Clear();
            foreach (UserItem user in _users)
                lstUsers.Items.Add(user);
            if (selected != null && lstUsers.Items.Contains(selected)) lstUsers.SelectedItem = selected;
            lstUsers.EndUpdate();
        }

        private void refreshSelectedUserListItem()
        {
            int index = lstUsers.SelectedIndex;
            if (index < 0) return;
            object item = lstUsers.Items[index];
            lstUsers.Items[index] = item;
            lstUsers.SelectedIndex = index;
        }

        private void refreshSelectedRoleListItem()
        {
            int index = lstRoles.SelectedIndex;
            if (index < 0) return;
            object item = lstRoles.Items[index];
            lstRoles.Items[index] = item;
            lstRoles.SelectedIndex = index;
        }

        private void refreshRoleList()
        {
            object selected = lstRoles.SelectedItem;
            lstRoles.BeginUpdate();
            lstRoles.Items.Clear();
            foreach (RoleItem role in _roles)
                lstRoles.Items.Add(role);
            if (selected != null && lstRoles.Items.Contains(selected)) lstRoles.SelectedItem = selected;
            lstRoles.EndUpdate();
            refreshRoleCombo(selectedUser());
        }

        private void refreshPermissionList()
        {
            object selected = lstPermissions.SelectedItem;
            lstPermissions.BeginUpdate();
            lstPermissions.Items.Clear();
            foreach (PermissionItem permission in _permissions)
                lstPermissions.Items.Add(permission);
            if (selected != null && lstPermissions.Items.Contains(selected)) lstPermissions.SelectedItem = selected;
            lstPermissions.EndUpdate();
        }

        private void refreshRoleCombo(UserItem user)
        {
            if (cmbUserRole == null) return;
            RoleItem selected_role = null;
            string role_id = user == null ? cmbUserRole.Text : user.Role;

            cmbUserRole.BeginUpdate();
            cmbUserRole.Items.Clear();
            foreach (RoleItem role in _roles)
            {
                cmbUserRole.Items.Add(role);
                if (string.Equals(role.Id, role_id, StringComparison.OrdinalIgnoreCase)) selected_role = role;
            }
            cmbUserRole.EndUpdate();

            if (selected_role != null) cmbUserRole.SelectedItem = selected_role;
            else cmbUserRole.SelectedIndex = -1;
        }

        private void refreshPermissionChecks(CheckedListBox list_box, List<string> checked_permissions)
        {
            list_box.BeginUpdate();
            list_box.Items.Clear();
            foreach (PermissionItem permission in _permissions)
            {
                bool check = checked_permissions != null && checked_permissions.Contains(permission.Id);
                list_box.Items.Add(permission, check);
            }
            list_box.EndUpdate();
        }

        private void refreshUserPermissionChecks(UserItem user)
        {
            clbUserPermissions.BeginUpdate();
            clbUserPermissions.Items.Clear();
            if (user != null)
            {
                HashSet<string> role_permissions = getRolePermissions(user.Role);
                foreach (PermissionItem permission in _permissions)
                {
                    if (role_permissions.Contains(permission.Id)) continue;
                    bool check = user.Permissions != null && user.Permissions.Contains(permission.Id);
                    clbUserPermissions.Items.Add(permission, check);
                }
            }
            clbUserPermissions.EndUpdate();
        }

        private HashSet<string> getRolePermissions(string role_id)
        {
            HashSet<string> permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            RoleItem role = findRole(role_id);
            if (role == null || role.Permissions == null) return permissions;

            foreach (string permission in role.Permissions)
            {
                if (!string.IsNullOrWhiteSpace(permission))
                    permissions.Add(permission);
            }

            return permissions;
        }

        private bool isGuestUser(UserItem user)
        {
            return user != null && string.Equals(user.Id, GUEST_USER_ID, StringComparison.OrdinalIgnoreCase);
        }

        private RoleItem findRole(string role_id)
        {
            if (string.IsNullOrWhiteSpace(role_id)) return null;
            foreach (RoleItem role in _roles)
            {
                if (role != null && string.Equals(role.Id, role_id, StringComparison.OrdinalIgnoreCase))
                    return role;
            }
            return null;
        }

        private void removeRolePermissionsFromUser(UserItem user)
        {
            if (user == null || user.Permissions == null) return;
            HashSet<string> role_permissions = getRolePermissions(user.Role);
            if (role_permissions.Count == 0) return;
            user.Permissions.RemoveAll(p => role_permissions.Contains(p));
        }

        private void removeRolePermissionsFromUsers(RoleItem role)
        {
            if (role == null || role.Permissions == null || role.Permissions.Count == 0) return;
            HashSet<string> role_permissions = new HashSet<string>(role.Permissions, StringComparer.OrdinalIgnoreCase);
            foreach (UserItem user in _users)
            {
                if (user == null || user.Permissions == null) continue;
                if (!string.Equals(user.Role, role.Id, StringComparison.OrdinalIgnoreCase)) continue;
                user.Permissions.RemoveAll(p => role_permissions.Contains(p));
            }
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            _loading = true;
            UserItem user = selectedUser();
            bool enabled = user != null;
            pnlUserDetails.Enabled = enabled;
            if (user == null)
            {
                txtUserId.Text = string.Empty;
                txtUserDisplayName.Text = string.Empty;
                chkUserEnabled.Checked = false;
                refreshRoleCombo(null);
                txtUserPin.Text = string.Empty;
                txtUserPinConfirm.Text = string.Empty;
                updatePinPasswordChars();
                txtUserNotes.Text = string.Empty;
                clbUserPermissions.Items.Clear();
                _loading = false;
                return;
            }

            txtUserId.Text = user.Id;
            txtUserDisplayName.Text = user.DisplayName;
            chkUserEnabled.Checked = user.Enabled;
            if (isGuestUser(user)) user.Role = GUEST_ROLE_ID;
            refreshRoleCombo(user);
            cmbUserRole.Enabled = !isGuestUser(user);
            txtUserPin.Text = pinTextForUser(user);
            txtUserPinConfirm.Text = pinConfirmTextForUser(user);
            updatePinPasswordChars();
            txtUserNotes.Text = user.Notes;
            removeRolePermissionsFromUser(user);
            refreshUserPermissionChecks(user);
            _loading = false;
        }

        private void lstRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            _loading = true;
            RoleItem role = selectedRole();
            bool enabled = role != null;
            pnlRoleDetails.Enabled = enabled;
            if (role == null)
            {
                txtRoleId.Text = string.Empty;
                txtRoleDisplayName.Text = string.Empty;
                clbRolePermissions.Items.Clear();
                _loading = false;
                return;
            }

            txtRoleId.Text = role.Id;
            txtRoleDisplayName.Text = role.DisplayName;
            refreshPermissionChecks(clbRolePermissions, role.Permissions);
            _loading = false;
        }

        private void lstPermissions_SelectedIndexChanged(object sender, EventArgs e)
        {
            _loading = true;
            PermissionItem permission = selectedPermission();
            bool enabled = permission != null;
            pnlPermissionDetails.Enabled = enabled;
            if (permission == null)
            {
                txtPermissionId.Text = string.Empty;
                txtPermissionDisplayName.Text = string.Empty;
                txtPermissionDescription.Text = string.Empty;
                _loading = false;
                return;
            }

            txtPermissionId.Text = permission.Id;
            txtPermissionDisplayName.Text = permission.DisplayName;
            txtPermissionDescription.Text = permissionDetails(permission);
            _loading = false;
        }

        private void txtRoleDisplayName_TextChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            RoleItem role = selectedRole();
            if (role == null) return;
            role.DisplayName = txtRoleDisplayName.Text.Trim();
        }

        private void txtRoleDisplayName_Leave(object sender, EventArgs e)
        {
            if (_loading) return;
            refreshSelectedRoleListItem();
            refreshRoleCombo(selectedUser());
        }

        private string permissionDetails(PermissionItem permission)
        {
            if (permission == null) return string.Empty;

            string details = permission.Description ?? string.Empty;
            if (permission.Commands == null || permission.Commands.Count == 0)
                return details;

            if (!string.IsNullOrWhiteSpace(details)) details += Environment.NewLine + Environment.NewLine;
            details += "TCI commands:";
            foreach (string command in permission.Commands)
                details += Environment.NewLine + command;

            return details;
        }

        private void txtUserDisplayName_TextChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            UserItem user = selectedUser();
            if (user == null) return;
            user.DisplayName = txtUserDisplayName.Text.Trim();
        }

        private void txtUserDisplayName_Leave(object sender, EventArgs e)
        {
            if (_loading) return;
            refreshSelectedUserListItem();
        }

        private void chkUserEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            UserItem user = selectedUser();
            if (user == null) return;
            user.Enabled = chkUserEnabled.Checked;
            refreshSelectedUserListItem();
        }

        private void cmbUserRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            userRoleChanged();
        }

        private void cmbUserRole_TextChanged(object sender, EventArgs e)
        {
            userRoleChanged();
        }

        private void userRoleChanged()
        {
            if (_loading) return;
            UserItem user = selectedUser();
            RoleItem role = cmbUserRole.SelectedItem as RoleItem;
            if (user == null || role == null) return;
            if (isGuestUser(user))
            {
                user.Role = GUEST_ROLE_ID;
                _loading = true;
                refreshRoleCombo(user);
                refreshUserPermissionChecks(user);
                _loading = false;
                return;
            }
            user.Role = role.Id;
            removeRolePermissionsFromUser(user);
            _loading = true;
            refreshUserPermissionChecks(user);
            _loading = false;
        }

        private void txtUserPin_TextChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            UserItem user = selectedUser();
            if (user == null) return;
            if (isStoredPinPlaceholder(txtUserPin.Text)) return;
            user.AccessCode = txtUserPin.Text.Trim();
            user.AccessCodeChanged = user.AccessCode.Length > 0 || cleanPinText(txtUserPinConfirm.Text).Length > 0;
            updatePinPasswordChars();
        }

        private void txtUserPinConfirm_TextChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            UserItem user = selectedUser();
            if (user == null) return;
            if (isStoredPinPlaceholder(txtUserPinConfirm.Text)) return;
            user.AccessCodeConfirm = txtUserPinConfirm.Text.Trim();
            user.AccessCodeChanged = cleanPinText(txtUserPin.Text).Length > 0 || user.AccessCodeConfirm.Length > 0;
            updatePinPasswordChars();
        }

        private void txtUserPin_Enter(object sender, EventArgs e)
        {
            clearStoredPinPlaceholder();
        }

        private void txtUserPin_Leave(object sender, EventArgs e)
        {
            restoreStoredPinPlaceholder();
        }

        private void updatePinPasswordChars()
        {
            txtUserPin.PasswordChar = isStoredPinPlaceholder(txtUserPin.Text) ? '\0' : '*';
            txtUserPinConfirm.PasswordChar = isStoredPinPlaceholder(txtUserPinConfirm.Text) ? '\0' : '*';
        }

        private string pinTextForUser(UserItem user)
        {
            if (user == null) return string.Empty;
            if (user.AccessCodeChanged) return user.AccessCode;
            if (user.HasAccessCode) return PIN_MASK_PLACEHOLDER;
            return string.Empty;
        }

        private string pinConfirmTextForUser(UserItem user)
        {
            if (user == null) return string.Empty;
            if (user.AccessCodeChanged) return user.AccessCodeConfirm;
            if (user.HasAccessCode) return PIN_MASK_PLACEHOLDER;
            return string.Empty;
        }

        private bool isStoredPinPlaceholder(string text)
        {
            return string.Equals(text, PIN_MASK_PLACEHOLDER, StringComparison.OrdinalIgnoreCase);
        }

        private string cleanPinText(string text)
        {
            if (isStoredPinPlaceholder(text)) return string.Empty;
            return text == null ? string.Empty : text.Trim();
        }

        private void clearStoredPinPlaceholder()
        {
            if (_loading) return;
            UserItem user = selectedUser();
            if (user == null || user.AccessCodeChanged || !user.HasAccessCode) return;

            _loading = true;
            if (isStoredPinPlaceholder(txtUserPin.Text)) txtUserPin.Text = string.Empty;
            if (isStoredPinPlaceholder(txtUserPinConfirm.Text)) txtUserPinConfirm.Text = string.Empty;
            updatePinPasswordChars();
            _loading = false;
        }

        private void restoreStoredPinPlaceholder()
        {
            if (_loading) return;
            UserItem user = selectedUser();
            if (user == null || user.AccessCodeChanged || !user.HasAccessCode) return;
            if (txtUserPin.Focused || txtUserPinConfirm.Focused) return;
            if (txtUserPin.Text.Trim().Length > 0 || txtUserPinConfirm.Text.Trim().Length > 0) return;

            _loading = true;
            txtUserPin.Text = PIN_MASK_PLACEHOLDER;
            txtUserPinConfirm.Text = PIN_MASK_PLACEHOLDER;
            updatePinPasswordChars();
            _loading = false;
        }

        private void txtUserNotes_TextChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            UserItem user = selectedUser();
            if (user == null) return;
            user.Notes = txtUserNotes.Text;
        }

        private void clbUserPermissions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_loading) return;
            UserItem user = selectedUser();
            if (user == null) return;
            updateCheckedPermissions(user.Permissions, clbUserPermissions, e);
        }

        private void clbRolePermissions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_loading) return;
            RoleItem role = selectedRole();
            if (role == null) return;
            updateCheckedPermissions(role.Permissions, clbRolePermissions, e);
            removeRolePermissionsFromUsers(role);
            UserItem user = selectedUser();
            if (user != null && string.Equals(user.Role, role.Id, StringComparison.OrdinalIgnoreCase))
            {
                _loading = true;
                refreshUserPermissionChecks(user);
                _loading = false;
            }
        }

        private void updateCheckedPermissions(List<string> permissions, CheckedListBox list_box, ItemCheckEventArgs e)
        {
            PermissionItem permission = list_box.Items[e.Index] as PermissionItem;
            if (permission == null) return;
            permissions.RemoveAll(p => string.Equals(p, permission.Id, StringComparison.OrdinalIgnoreCase));
            if (e.NewValue == CheckState.Checked)
                permissions.Add(permission.Id);
            permissions.Sort(StringComparer.OrdinalIgnoreCase);
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            UserItem user = new UserItem();
            user.Id = nextId("pin", existingUserIds());
            user.DisplayName = "New PIN";
            user.Enabled = false;
            user.Role = _roles.Count > 0 ? _roles[0].Id : string.Empty;
            user.Permissions = new List<string>();
            user.Notes = string.Empty;
            user.HasAccessCode = false;
            user.AccessCodeChanged = true;
            _users.Add(user);
            refreshUserList();
            lstUsers.SelectedItem = user;
            txtUserDisplayName.Focus();
        }

        private void btnRemoveUser_Click(object sender, EventArgs e)
        {
            UserItem user = selectedUser();
            if (user == null) return;
            if (string.Equals(user.Id, "guest", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "The built in guest PIN cannot be removed. Disable it instead.", "TCI Auth Permissions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(this, "Remove PIN " + user.DisplayName + "?", "TCI Auth Permissions", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;

            _users.Remove(user);
            refreshUserList();
            if (lstUsers.Items.Count > 0) lstUsers.SelectedIndex = 0;
        }

        private void btnAddRole_Click(object sender, EventArgs e)
        {
            RoleItem role = new RoleItem();
            role.Id = newInternalRoleId();
            role.DisplayName = nextDisplayName("New role");
            role.Permissions = new List<string>();
            _roles.Add(role);
            refreshRoleList();
            lstRoles.SelectedItem = role;
            txtRoleDisplayName.Focus();
        }

        private void btnRemoveRole_Click(object sender, EventArgs e)
        {
            RoleItem role = selectedRole();
            if (role == null) return;
            if (_roles.Count <= 1)
            {
                MessageBox.Show(this, "At least one role is required.", "TCI Auth Permissions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (UserItem user in _users)
            {
                if (string.Equals(user.Role, role.Id, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(this, "This role is in use by one or more PINs and cannot be removed.", "TCI Auth Permissions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            DialogResult result = MessageBox.Show(this, "Remove role " + role.DisplayName + "?", "TCI Auth Permissions", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;

            _roles.Remove(role);
            refreshRoleList();
            if (lstRoles.Items.Count > 0) lstRoles.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string error_message;
            if (!validateData(out error_message))
            {
                MessageBox.Show(this, error_message, "TCI Auth Permissions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<TCIpermissions.SetupRole> setup_roles = new List<TCIpermissions.SetupRole>();
            foreach (RoleItem role in _roles)
            {
                TCIpermissions.SetupRole setup_role = new TCIpermissions.SetupRole();
                setup_role.Id = role.Id;
                setup_role.DisplayName = role.DisplayName;
                setup_role.Permissions = new List<string>(role.Permissions);
                setup_roles.Add(setup_role);
            }

            List<TCIpermissions.SetupUser> setup_users = new List<TCIpermissions.SetupUser>();
            foreach (UserItem user in _users)
            {
                TCIpermissions.SetupUser setup_user = new TCIpermissions.SetupUser();
                setup_user.Id = user.Id;
                setup_user.DisplayName = user.DisplayName;
                setup_user.Enabled = user.Enabled;
                setup_user.Role = user.Role;
                setup_user.Permissions = new List<string>(user.Permissions);
                setup_user.CreatedUtc = user.CreatedUtc;
                setup_user.ModifiedUtc = user.ModifiedUtc;
                setup_user.Notes = user.Notes;
                setup_user.HasAccessCode = user.HasAccessCode;
                setup_user.AccessCode = cleanPinText(user.AccessCode);
                setup_user.AccessCodeChanged = user.AccessCodeChanged;
                setup_users.Add(setup_user);
            }

            if (!TCIpermissions.ApplySetupData(setup_roles, setup_users, out error_message))
            {
                MessageBox.Show(this, error_message, "TCI Auth Permissions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_permissions_path))
                    File.WriteAllText(_permissions_path, TCIpermissions.SaveToJson());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Unable to save TCI permissions." + Environment.NewLine + Environment.NewLine + ex.Message, "TCI Auth Permissions", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool validateData(out string error_message)
        {
            error_message = string.Empty;
            if (_roles.Count == 0)
            {
                error_message = "At least one role is required.";
                return false;
            }
            if (_users.Count == 0)
            {
                error_message = "At least one PIN is required.";
                return false;
            }

            HashSet<string> permission_ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (PermissionItem permission in _permissions)
            {
                if (permission == null || string.IsNullOrWhiteSpace(permission.Id)) continue;
                permission_ids.Add(permission.Id);
            }

            HashSet<string> role_ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> role_names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (RoleItem role in _roles)
            {
                if (role == null) continue;
                role.Id = cleanId(role.Id);
                if (!isValidId(role.Id))
                {
                    error_message = "Invalid internal role id.";
                    return false;
                }
                if (!role_ids.Add(role.Id))
                {
                    error_message = "Duplicate internal role id.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(role.DisplayName))
                {
                    error_message = "Role name is required.";
                    return false;
                }
                role.DisplayName = role.DisplayName.Trim();
                if (!role_names.Add(role.DisplayName))
                {
                    error_message = "Duplicate role name: " + role.DisplayName;
                    return false;
                }
                foreach (string permission in role.Permissions)
                {
                    if (!permission_ids.Contains(permission))
                    {
                        error_message = "Role " + role.DisplayName + " uses a missing permission.";
                        return false;
                    }
                }
            }

            HashSet<string> user_ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> changed_pins = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (UserItem user in _users)
            {
                user.Id = cleanId(user.Id);
                if (!isValidId(user.Id))
                {
                    error_message = "Invalid internal PIN id.";
                    return false;
                }
                if (!user_ids.Add(user.Id))
                {
                    error_message = "Duplicate internal PIN id.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(user.DisplayName)) user.DisplayName = user.Id;
                user.Role = isGuestUser(user) ? GUEST_ROLE_ID : cleanId(user.Role);
                if (!role_ids.Contains(user.Role))
                {
                    error_message = "PIN " + user.DisplayName + " uses a missing role.";
                    return false;
                }
                removeRolePermissionsFromUser(user);
                foreach (string permission in user.Permissions)
                {
                    if (!permission_ids.Contains(permission))
                    {
                        error_message = "PIN " + user.DisplayName + " uses a missing permission.";
                        return false;
                    }
                }

                string pin = user.AccessCode == null ? string.Empty : cleanPinText(user.AccessCode);
                string confirm = getPinConfirmForUser(user);
                if (user.AccessCodeChanged || !user.HasAccessCode)
                {
                    if (!validatePin(user.DisplayName, pin, confirm, out error_message)) return false;
                    string existing_user_name;
                    if (changed_pins.TryGetValue(pin, out existing_user_name))
                    {
                        error_message = "PIN " + user.DisplayName + " has the same number as " + existing_user_name + ".";
                        return false;
                    }
                    changed_pins[pin] = user.DisplayName;
                    user.AccessCode = pin;
                }
            }

            return true;
        }

        private string getPinConfirmForUser(UserItem user)
        {
            UserItem selected = selectedUser();
            if (selected != null && object.ReferenceEquals(selected, user))
                return cleanPinText(txtUserPinConfirm.Text);
            return cleanPinText(user.AccessCodeConfirm);
        }

        private bool validatePin(string name, string pin, string confirm, out string error_message)
        {
            error_message = string.Empty;
            if (string.IsNullOrWhiteSpace(pin))
            {
                error_message = "PIN " + name + " needs a number.";
                return false;
            }
            if (!Regex.IsMatch(pin, "^[0-9]{4,12}$"))
            {
                error_message = "PIN " + name + " must be 4 to 12 digits.";
                return false;
            }
            if (!string.Equals(pin, confirm, StringComparison.Ordinal))
            {
                error_message = "PIN confirmation does not match for " + name + ".";
                return false;
            }
            return true;
        }

        private string cleanId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;
            return id.Trim().ToLowerInvariant();
        }

        private bool isValidId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return Regex.IsMatch(id, "^[a-z0-9_]+$");
        }

        private string newInternalRoleId()
        {
            HashSet<string> existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (RoleItem role in _roles)
                existing.Add(role.Id);

            string id;
            do
            {
                id = Guid.NewGuid().ToString("N");
            }
            while (existing.Contains(id));

            return id;
        }

        private string nextDisplayName(string name)
        {
            HashSet<string> existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (RoleItem role in _roles)
                existing.Add(role.DisplayName);

            if (!existing.Contains(name)) return name;

            for (int i = 2; i < 10000; i++)
            {
                string candidate = name + " " + i.ToString();
                if (!existing.Contains(candidate)) return candidate;
            }

            return name;
        }

        private HashSet<string> existingUserIds()
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (UserItem user in _users)
                ids.Add(user.Id);
            return ids;
        }

        private string nextId(string prefix, HashSet<string> existing)
        {
            for (int i = 1; i < 10000; i++)
            {
                string id = prefix + "_" + i.ToString();
                if (!existing.Contains(id)) return id;
            }
            return prefix + "_new";
        }
    }
}
