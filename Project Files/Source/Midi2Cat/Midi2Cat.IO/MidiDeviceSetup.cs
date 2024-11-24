/*  Midi2Cat

Description: A subsystem that facilitates mapping Windows MIDI devices to CAT commands.
 
Copyright (C) 2016 Andrew Mansfield, M0YGG

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

The author can be reached by email at:  midi2cat@cametrix.com

Modifications to support the Behringer CMD PL-1 controller
by Chris Codella, W2PA, Feb 2017.  Indicated by //-W2PA comment lines.

*/

using System.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Midi2Cat.Helpers;
using Midi2Cat.IO;
using Midi2Cat.Data;
using System.Data;

namespace Midi2Cat.IO
{
    public partial class MidiDeviceSetup : UserControl
    {
        public MidiDeviceSetup(string DbFile, string deviceName, int deviceIndex)
        {
            InitializeComponent();
            this.DbFile = DbFile;
            this.DeviceName = deviceName;
            this.DeviceIndex = deviceIndex;
            mapInCtrl2CmdPanel.Parent = this;
            mapInCtrl2CmdPanel.Visible = false;
            tabControl.TabPages.Remove(mapInDialogTabPage);
            advancedLinkLabel.Text = showAdvancedSettings;
            OpenMidiDevice();
        }


        private void MidiDeviceSetup_Load(object sender, EventArgs e)
        {
            var mappings = DB.GetMappings(DeviceName, MappingFilter.None);
            foreach (var mapping in mappings)
            {
                if (mapping.CatCmdId != CatCmd.None)
                    _enumsDb.SetCatCmdInUse(mapping.CatCmdId, true);
            }
            mapControlToCommandGrid.Sort(mapControlToCommandGrid.Columns[0], ListSortDirection.Ascending);
            promptPanel.Visible = (mappings.Count == 0);
            LoadedMappingLabel.Text = DB.GetLoadedMappingName(DeviceName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseMidiDevice();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Enums and constants and private vars

        private const string MapCtrl2CmdMsg1 = "You must now give this control a name and then select one of the options in the \"Control Type\" dropdown box above.\n\nChoose the option that most closely matches the type of control you moved, pressed or slid.";
        private const string showAdvancedSettings = "Show Advanced Options";
        private const string hideAdvancedSettings = "Hide Advanced Options";

        private string DbFile;
        private bool AddingControl = false;
        private int AddingControlId = int.MinValue;
        private bool DialogValid = false;
        EnumsDB _enumsDb = null;
        Midi2CatDatabase _db = null;
        MidiDevice midiDevice;
        CatCmd CatCmdToUse = CatCmd.None;
        ControlType ControlTypeToUse = ControlType.Unknown;
        MidiDiagList _diagList;
        DateTime ignoreMidiMessagesUntil=DateTime.Now.AddSeconds(5);

        public Midi2CatDatabase DB
        {
            get
            {
                if (_db == null)
                {
                    _db = new Midi2CatDatabase(DbFile);
                    _db.BindToDataSource(controllerMappingBindingSource1, DeviceName);
                    _enumsDb = new EnumsDB();
                    _enumsDb.BindToDataSource(controlTypesBindingSource, "ControlTypes");
                    midiControlTypeColumn.DataSource = controlTypesBindingSource;
                    midiControlTypeColumn.ValueMember = "ControlId";
                    midiControlTypeColumn.DisplayMember = "ControlDescription";

                    _enumsDb.BindToDataSource(catCmdsBindingSource, "CatCmds");
                    CatCmdIdColumn.DataSource = catCmdsBindingSource;
                    CatCmdIdColumn.ValueMember = "CmdId";
                    CatCmdIdColumn.DisplayMember = "CmdDescription";

                    _enumsDb.BindToDataSource(catCmdsFilteredBindingSource, "CatCmds");

                    mappingAvailableCmdsLB.DataSource = catCmdsFilteredBindingSource;
                    mappingAvailableCmdsLB.DisplayMember = "CmdDescription";
                    mappingAvailableCmdsLB.ValueMember = "CmdId";

                    mappingControlTypeCB2.DataBindings.Add(new Binding("SelectedValue", this.controllerMappingBindingSource1, "MidiControlType", true));
                    mappingControlTypeCB2.DataSource = controlTypesBindingSource;
                    mappingControlTypeCB2.DisplayMember = "ControlDescription";
                    mappingControlTypeCB2.ValueMember = "ControlId";

                    MappedCommands mappedCommands = new MappedCommands(_enumsDb.ds.Tables["CatCmds"], _db.ds.Tables[DeviceName]);
                    mappedCommandsBindingSource.DataSource = mappedCommands;

                    _diagList = new MidiDiagList();
                    midiDiagListBindingSource.DataSource = _diagList;
                }
                return _db;
            }
        }

        #endregion

        #region public properties

        private string DeviceName { get; set; }
        private int DeviceIndex { get; set; }

        private void OpenMidiDevice()
        {
            if (midiDevice == null)
            {
                midiDevice = new MidiDevice();
                midiDevice.onMidiDebugMessage += onMidiDebugMsg;
                midiDevice.onMidiInput += OnMidiInput;
            }
            midiDevice.OpenMidiIn(DeviceIndex, DeviceName);
            ignoreMidiMessagesUntil = DateTime.Now.AddSeconds(5);
        }

        private void CloseMidiDevice()
        {
            if (midiDevice != null)
            {
                midiDevice.CloseMidiIn();
                midiDevice.CloseMidiOut();
                midiDevice = null;
            }
        }

        #endregion


        void OnMidiInput(MidiDevice Device, int DeviceIdx, int ControlId, int Data, int Status, int Event, int Channel)
        {
            // Following opening the midi controller most controllers will immediatly send current status of any knobs and sliders
            // We are not interested in these messages when in setup. The Only way to ignore them is to introduce a delay following the open command.
            if (DateTime.Now < ignoreMidiMessagesUntil)
                return;

            //ControlId = FixBehringerCtlID(ControlId, Status); //-W2PA Disambiguate messages from Behringer controllers  //[2.10.3.5]MW0LGE lets not implent this twice right?
            ControlId = Device.FixBehringerCtlID(ControlId, Status);

            this.InvokeIfRequired(p =>
            {
                if (tabControl.SelectedTab == debugTab)
                {
                    int unmappedControlId = Device.UnmapControlID(ControlId, out int byteCount); //[2.10.3.5]MW0LGE so we can get the original control id
                    string sControlID;
                    if (ControlId != unmappedControlId) // if we changed the control id, then display orginal in ()
                    {
                        string sLenX = "X" + (byteCount * 2).ToString();
                        sControlID = ControlId.ToString(sLenX) + " (" + unmappedControlId.ToString("X2") + ")";
                    }
                    else
                        sControlID = ControlId.ToString("X2");

                    _diagList.Add(new MidiDiagItem { Device = DeviceIdx.ToString("X2"), ControlId = sControlID, Data = Data.ToString("X2"), Status = Status.ToString("X2"), Voice = ((MidiEvent)Event).ToString().Replace("_", " "), Channel = (Channel+1).ToString("X2") });
                    if (_diagList.Count > 100)
                    {
                        _diagList.RemoveAt(0);
                    }
                    midiDiagDataGrid.CurrentCell = midiDiagDataGrid.Rows[_diagList.Count - 1].Cells[0];
                }

                if (tabControl.SelectedTab == commandsTabPage) 
                {
                    tabControl.SelectedTab = mappedControlsTab;
                    if (mappedCommandsGridView.SelectedRows.Count == 1)
                    {
                        var row = mappedCommandsGridView.SelectedRows[0];
                        int cmdIdx = mappedCommandsGridView.Columns["cmdIdDataGridViewTextBoxColumn"].Index;
                        CatCmdToUse = (CatCmd)row.Cells[cmdIdx].Value;
                        int ctIdx = mappedCommandsGridView.Columns["controlTypeDataGridViewTextBoxColumn"].Index;
                        ControlTypeToUse = (ControlType)row.Cells[ctIdx].Value;
                    }
                }

                if (tabControl.SelectedTab == mappedControlsTab)
                {
                    ControllerMapping mapping = DB.GetMapping(DeviceName, ControlId);
                    if (AddingControl && AddingControlId != ControlId)
                    {
                        MappingDone();
                    }
                    if (mapping == null)
                    {
                        mapping = new ControllerMapping { MidiControlId = ControlId, MaxValue = int.MinValue, MinValue = int.MaxValue, MidiControlType = ControlTypeToUse, MidiControlName = "" };
                        mapping.CatCmd = CatCmdDb.Get(CatCmd.None);
                        DataRowView newView = (DataRowView)controllerMappingBindingSource1.AddNew();
                        DB.PopulateRow(newView.Row, mapping);
                        DB.AddRow(DeviceName, newView.Row);
                        newView.Row.AcceptChanges();
                        mapControlToCommandGrid.Refresh();
                        DB.SaveChanges(DeviceName);
                        AddingControl = true;
                        AddingControlId = ControlId;
                    }

                    int Idx = controllerMappingBindingSource1.Find("MidiControlId", mapping.MidiControlId);
                    controllerMappingBindingSource1.Position = Idx;
                    ShowMapInCtrl2CmdDialog(ControlId, Data);
                    ValidateDialogInput();
                    SendMidiCommand(Channel, Data, Status, ControlId, mapping);
                }
            });
        }

        //private int FixBehringerCtlID(int ControlId, int Status) //-W2PA Test for DeviceName is a Behringer type, and disambiguate the messages if necessary
        //{
        //    if (DeviceName == "CMD PL-1")
        //    {
        //        if (Status == 0xE0) //-W2PA Trap Status E0 from Behringer PL-1 slider, change the ID to something that doesn't conflict with other controls
        //        {
        //            ControlId = 73;  //-W2PA I don't think this corresponds to the ID of any other control on the PL-1
        //        }

        //        if (ControlId == 0x1F && MidiDevice.VFOSelect == 2) //-W2PA Trap PL-1 main wheel, change the ID to something that doesn't conflict with other controls, indicating VFO number (0=A, 1=B)
        //        {
        //            ControlId = 77;  //-W2PA I don't think this corresponds to the ID of any other control on the PL-1, so use for VFOB
        //        }
        //    }
        //    if (DeviceName == "CMD Micro")
        //    {
        //        if (Status == 0xB0) //-W2PA Trap Status E0 from Behringer CMD Micro controls, change the ID to something that doesn't conflict with buttons
        //        {
        //            if (ControlId == 0x10) ControlId = 73; // sliders
        //            if (ControlId == 0x12) ControlId = 74;
        //            if (ControlId == 0x22) ControlId = 75;
        //            if (ControlId == 0x20) ControlId = 76;

        //            if (ControlId == 0x11) ControlId = 77; // large wheels
        //            if (ControlId == 0x21) ControlId = 78;

        //            if (ControlId == 0x30) ControlId = 79; // small wheel-knobs
        //            if (ControlId == 0x31) ControlId = 80;
        //        }
        //    }
        //    return ControlId; 
        //}

        void onMidiDebugMsg(int Device, Direction direction, Status status, string msg1, string msg2)
        {
            string msg = null;
            if (!string.IsNullOrEmpty(msg1) || !string.IsNullOrEmpty(msg2))
            {
                msg = string.Format("{0} {1}:{2} {3}\n", status.ToString(), direction.ToString(), msg1, msg2);
            }

            if (this.Visible)
            {
                switch (direction)
                {
                    case Direction.In:
                        midiInStatusLabel.Text = status.ToString();
                        break;
                    case Direction.Out:
                        midiOutStatusLabel.Text = status.ToString();
                        break;
                }
                if (msg != null)
                    errorListBox.Items.Insert(0, msg);
                else if (errorListBox.Items.Count == 0)
                    errorListBox.Items.Add("No errors detected.");
            }
            if (msg != null)
                Debug.WriteLine(msg);
        }

        private void ShowMapInCtrl2CmdDialog(int ctrlId, int value)
        {
            tabControl.Visible = false;
            LoadNonDataBoundControls(ctrlId, value);
            ShowAddPrompt();
            mapInCtrl2CmdPanel.Visible = true;
            ResizeDialogs();
            if (string.IsNullOrWhiteSpace(mappingControlNameTB2.Text))
                mappingControlNameTB2.Focus();
            mapInCtrl2CmdPanel.Focus();
        }

        private void SetMapInCtrl2CmdDialog(int ctrlId, int value)
        {
            LoadNonDataBoundControls(ctrlId, value);
            ShowAddPrompt();
            mapInCtrl2CmdPanel.Visible = true;
        }

        private void ShowAddPrompt()
        {
            bool ShowPrompt = (mappingControlTypeCB2.SelectedValue != null && ((ControlType)mappingControlTypeCB2.SelectedValue) == ControlType.Unknown) || string.IsNullOrWhiteSpace(mappingControlNameTB2.Text);
            if (ShowPrompt == true)
            {
                mappingPromptLabel2.Text = MapCtrl2CmdMsg1;
                mappingPromptPanel2.Visible = true;
                mappingAvailableCmdsLB.Visible = false;
            }
            else
            {
                mappingPromptPanel2.Visible = false;
                mappingAvailableCmdsLB.Visible = true;
            }
            promptPanel.Visible = false;
        }

        private void LoadNonDataBoundControls(int ctrlId, int value)
        {
            DataRowView view = (DataRowView)controllerMappingBindingSource1.Current;
            CatCmd currentCatCmdId = (CatCmd)view["CatCmdId"];
            if (CatCmdToUse != CatCmd.None)
            {
                currentCatCmdId = CatCmdToUse;
                mappingControlTypeCB2.SelectedValue = ControlTypeToUse;
                if (value == 0)
                {
                    CatCmdToUse = CatCmd.None;
                    ControlTypeToUse = ControlType.Unknown;
                }
            }
            if (value > int.MinValue)
            {
                int currentMinValue = (int)view["MinValue"];
                int currentMaxValue = (int)view["MaxValue"];
                if (value < currentMinValue)
                {
                    view["MinValue"] = value;
                    view.Row.AcceptChanges();
                }
                if (value > currentMaxValue)
                {
                    view["MaxValue"] = value;
                    view.Row.AcceptChanges();
                }
            }
            mappingValueLabel2.Text = value.ToString();
            mappingMessageLabel2.Text = CatCmdDb.Get(currentCatCmdId).Desc;
            mappingControlNameTB2.Text = (string)view["MidiControlName"];
            mappingMaxValueLabel2.Text = ((int)view["MaxValue"]).ToString();
            mappingMinValueLabel2.Text = ((int)view["MinValue"]).ToString();
            mappingControlIdLabel2.Text = ((int)view["MidiControlId"]).ToString();
            buttonDownEventTB.Text = _db.ConvertFromDBVal<string>(view["MidiOutCmdDown"]);
            buttonUpEventTB.Text = _db.ConvertFromDBVal<string>(view["MidiOutCmdUp"]);
            newValueReceivedEventTB.Text = _db.ConvertFromDBVal<string>(view["MidiOutCmdSetValue"]);

            if (string.IsNullOrWhiteSpace(buttonDownEventTB.Text) && string.IsNullOrWhiteSpace(buttonUpEventTB.Text) && string.IsNullOrWhiteSpace(newValueReceivedEventTB.Text))
                advancedLinkLabel.Text = showAdvancedSettings;
            else
                advancedLinkLabel.Text = hideAdvancedSettings;

            ControlType currentControlType = (ControlType)mappingControlTypeCB2.SelectedValue;
            buttonDownEventTB.Enabled = (currentControlType == ControlType.Button);
            buttonUpEventTB.Enabled = (currentControlType == ControlType.Button);

            string filter = "( InUse = False AND ControlType = " + mappingControlTypeCB2.SelectedValue.ToString() + " OR ControlType = 0 )";
            filter += " OR ( CmdId = " + ((int)currentCatCmdId) + ")";
            if (filter != catCmdsFilteredBindingSource.Filter)
            {
                catCmdsFilteredBindingSource.Filter = filter;
            }
            mappingAvailableCmdsLB.SelectedValue = currentCatCmdId;
        }

        private void HideMapInDialogs()
        {
            mapInCtrl2CmdPanel.Visible = false;
            tabControl.Visible = true;
        }


        private void MidiDeviceSetup_Resize(object sender, EventArgs e)
        {
            ResizeDialogs();
        }

        private void advancedLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (advancedLinkLabel.Text == showAdvancedSettings)
            {
                advancedLinkLabel.Text = hideAdvancedSettings;
            }
            else
            {
                advancedLinkLabel.Text = showAdvancedSettings;
            }
            ResizeDialogs();
        }

        private void ResizeDialogs()
        {
            if (mapInCtrl2CmdPanel.Visible)
            {
                if (advancedLinkLabel.Text == showAdvancedSettings)
                {
                    advancedMappingPanel.Visible = false;
                    mapInCtrl2CmdPanel.Width = 396;
                    mapInCtrl2CmdPanel.Left = this.Width / 2 - mapInCtrl2CmdPanel.Width / 2;
                }
                else
                {
                    advancedMappingPanel.Visible = true;
                    mapInCtrl2CmdPanel.Width = this.Width - 16;
                    mapInCtrl2CmdPanel.Left = 8;
                }

                mapInCtrl2CmdPanel.Top = 10;
                mapInCtrl2CmdPanel.Height = this.Height - 20;

            }
        }

        private void mappingControlTypeCB2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mapInCtrl2CmdPanel.Visible == false)
                return;
            if (mappingControlTypeCB2.SelectedIndex == 0)
            {
                mappingPromptPanel2.Visible = true;
                mappingAvailableCmdsLB.Visible = false;
            }
            else
            {
                mappingPromptPanel2.Visible = false;
                mappingAvailableCmdsLB.Visible = true;
            }

            if (mappingControlTypeCB2.SelectedValue != null)
            {
                ControlType currentControlType = (ControlType)mappingControlTypeCB2.SelectedValue;
                buttonDownEventTB.Enabled = (currentControlType == ControlType.Button);
                buttonUpEventTB.Enabled = (currentControlType == ControlType.Button);

                string filter = "( InUse = False AND ControlType = " + mappingControlTypeCB2.SelectedValue.ToString() + " OR ControlType = 0 )";
                if (mappingAvailableCmdsLB.SelectedValue != null)
                    filter += " OR ( CmdId = " + mappingAvailableCmdsLB.SelectedValue.ToString() + ")";
                if (catCmdsFilteredBindingSource.Filter != filter)
                    catCmdsFilteredBindingSource.Filter = filter;
            }
        }

        private void mappingAvailableCmdsLB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValidateDialogInput();
        }

        private void mappingDoneButton2_Click(object sender, EventArgs e)
        {
            MappingDone();
            HideMapInDialogs();
        }

        private void MappingDone()
        {
            if (AddingControl && AddingControlId > int.MinValue && DialogValid == false)
            {
                RemoveInvalidMidiAddedControlType();
            }
            else
            {
                DataRowView view = (DataRowView)controllerMappingBindingSource1.Current;
                if (view != null && DialogValid)
                {
                    DataRowView CatCmdrow = (DataRowView)mappingAvailableCmdsLB.SelectedItem;
                    CatCmdrow["InUse"] = true;
                    CatCmdrow.Row.AcceptChanges();

                    view["CatCmdId"] = (CatCmd)CatCmdrow["CmdId"];
                    view["MidiControlName"] = mappingControlNameTB2.Text;
                    view["MidiControlType"] = (CatCmd)mappingControlTypeCB2.SelectedValue;
                    view["MidiOutCmdDown"] = buttonDownEventTB.Text.Trim().ToUpper();
                    view["MidiOutCmdUp"] = buttonUpEventTB.Text.Trim().ToUpper();
                    view["MidiOutCmdSetValue"] = newValueReceivedEventTB.Text.Trim().ToUpper();
                    view.Row.AcceptChanges();
                    DB.SaveChanges(DeviceName);
                    AddingControl = false;
                    AddingControlId = int.MinValue;
                }
            }
        }

        private void mappingBackButton2_Click(object sender, EventArgs e)
        {
            if (AddingControl && AddingControlId > int.MinValue)
            {
                RemoveInvalidMidiAddedControlType();
            }
            HideMapInDialogs();
        }

        private void mappingControlNameTB2_KeyPress(object sender, KeyPressEventArgs e)
        {
            ValidateDialogInput();
        }


        private void mapControlToCommandGrid_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                /*
                var cell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.IsInEditMode == true)
                {
                    mapControlToCommandGrid.CommitEdit(0);
                    mapControlToCommandGrid.EndEdit();
                    mapControlToCommandGrid.CurrentCell = null;
                    if (cell.Value.ToString() != (string)cell.Tag)
                    {
                        mapControlToCommandGrid_CellValueChanged(sender, e);
                        cell.Tag = cell.Value.ToString();
                    }
                    mapControlToCommandGrid.CurrentCell = null;
                }
                 */
            }
        }

        private void mapControlToCommandGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                /*
                if (cell.IsInEditMode == false && cell.ReadOnly == false)
                {
                    //mapControlToCommandGrid.CurrentCell = cell;
                    // cell.Selected = true;
                    if (cell.EditType != typeof(DataGridViewComboBoxEditingControl))
                    {
                        mapControlToCommandGrid.CurrentCell = cell;
                        cell.Tag = cell.Value.ToString();
                        mapControlToCommandGrid.BeginEdit(false);
                    }
                }
                 */
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex < 0)
            {
                mapControlToCommandGrid.CurrentCell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[0];
            }
        }

        private void mapControlToCommandGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var Cell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            var Row = Cell.OwningRow;
            var Col = Cell.OwningColumn;
            if (Col.IsDataBound && Col.Name == "CatCmdIdColumn")
            {
                if (mapControlToCommandGrid.CurrentCell.EditType == typeof(DataGridViewComboBoxEditingControl))
                {
                    DataGridViewComboBoxCell cb = (DataGridViewComboBoxCell)mapControlToCommandGrid[e.ColumnIndex, e.RowIndex];
                    cb.DataSource = catCmdsFilteredBindingSource;
                    var TypeValue = Row.Cells["midiControlTypeColumn"].Value;

                    //string filter = "( InUse = False AND ControlType = " + TypeValue.ToString() + " OR ControlType = 0 )";
                    string filter = "( ControlType = " + TypeValue.ToString() + " OR ControlType = 0 )";
                    if (string.IsNullOrEmpty(Cell.Value.ToString()) == false)
                        filter += " OR ( CmdId = " + Cell.Value.ToString() + ")";


                    //System.Diagnostics.Debug.WriteLine(filter);
                    catCmdsFilteredBindingSource.Filter = filter;

                    bool valueOk = false;
                    foreach (DataRowView rowView in catCmdsFilteredBindingSource)
                    {
                        //System.Diagnostics.Debug.WriteLine("Required=" + ((CatCmd)Cell.Value) + "(" + Cell.Value.ToString() + ")" + " cmd=" + ((CatCmd)rowView["CmdId"]) + " val=" + rowView["CmdId"].ToString()+" Inuse=" + rowView["InUse"].ToString());
                        if (rowView["CmdId"].ToString() == Cell.Value.ToString())
                        {
                            valueOk = true;
                            break;
                        }
                    }
                    if (valueOk == false)
                    {
                        System.Data.DataRowView obj = (System.Data.DataRowView)mapControlToCommandGrid.CurrentRow.DataBoundItem;
                        obj.BeginEdit();
                        obj.Row["CatCmdId"] = 0;
                        obj.Row.EndEdit();
                    }
                    cb.DataSource = catCmdsFilteredBindingSource;
                }
            }
        }

        private void mapControlToCommandGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var Cell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            var Row = Cell.OwningRow;
            var Col = Cell.OwningColumn;
            if (Col.IsDataBound && Col.Name == "CatCmdIdColumn")
            {
                if (mapControlToCommandGrid.CurrentCell.EditType == typeof(DataGridViewComboBoxEditingControl))
                {
                    DataGridViewComboBoxCell cb = (DataGridViewComboBoxCell)mapControlToCommandGrid[e.ColumnIndex, e.RowIndex];
                    cb.DataSource = catCmdsBindingSource;
                }
            }
            else if (Col.IsDataBound && Col.Name == "midiControlTypeColumn")
            {
                ControlType ctrlType = (ControlType)Row.Cells["midiControlTypeColumn"].Value;
                CatCmd CatCmdValue = (CatCmd)Row.Cells["CatCmdIdColumn"].Value;
                var attr = CatCmdDb.Get(CatCmdValue);
                if (attr.ControlType != ctrlType)
                {
                    System.Data.DataRowView obj = (System.Data.DataRowView)mapControlToCommandGrid.CurrentRow.DataBoundItem;
                    obj.BeginEdit();
                    obj.Row["CatCmdId"] = 0;
                    obj.EndEdit();
                }
            }
        }

        private void mapControlToCommandGrid_Click(object sender, EventArgs e)
        {
            if (mapControlToCommandGrid.IsCurrentCellInEditMode)
                mapControlToCommandGrid.EndEdit();
        }

        private void mapControlToCommandGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Does nothing but suppress the default error handling of the grid. Do not remove!
        }

        private void mapControlToCommandGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var Cell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            var Row = Cell.OwningRow;
            var Col = Cell.OwningColumn;
            string sCurrentValue = Cell.Value.ToString();
            string sPreviousValue = (string)Cell.Tag;

            if (Col.Name == "CatCmdIdColumn")
            {
                CatCmd currentCmd = (CatCmd)Convert.ToInt32(sCurrentValue);
                CatCmd previousCmd = (CatCmd)Convert.ToInt32(sPreviousValue);
                _enumsDb.SetCatCmdInUse(previousCmd, false);
                _enumsDb.SetCatCmdInUse(currentCmd, true);
            }
            DB.SaveChanges(DeviceName);
        }

        private void mapControlToCommandGrid_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            ShowMapInCtrl2CmdDialog(((int)mapControlToCommandGrid.CurrentRow.Cells[0].Value), int.MinValue);
        }

        private void validateDialogInput(object sender, CancelEventArgs e)
        {
            ValidateDialogInput();
        }

        private void ValidateDialogInput()
        {
            DialogValid = (!string.IsNullOrWhiteSpace(mappingControlNameTB2.Text) && mappingControlTypeCB2.Text != "Unknown" && mappingAvailableCmdsLB.SelectedItem != null);
            mappingDoneButton2.Enabled = DialogValid;
        }

        private void RemoveInvalidMidiAddedControlType()
        {
            if (AddingControl && AddingControlId >= int.MinValue)
            {
                DB.DeleteRow(DeviceName, AddingControlId);
                DB.SaveChanges(DeviceName);
                AddingControl = false;
                AddingControlId = int.MinValue;
            }
        }

        private void mapControlToCommandGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.OwningColumn.Name == "EditColumn")
                {
                    ShowMapInCtrl2CmdDialog(((int)mapControlToCommandGrid.CurrentRow.Cells[0].Value), int.MinValue);

                }
                if (cell.OwningColumn.Name == "deleteColumn")
                {
                    mapControlToCommandGrid.Rows.RemoveAt(e.RowIndex);
                    DB.SaveChanges(DeviceName);
                }
            }
        }

        private void mappedCommandsGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                try
                {
                    var cell = mappedCommandsGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    if (cell.OwningColumn.Name == "removeColumn")
                    {
                        var row = mappedCommandsGridView.Rows[e.RowIndex];
                        int dnIdx = mappedCommandsGridView.Columns["controllerDataGridViewTextBoxColumn"].Index;
                        int cmdIdx = mappedCommandsGridView.Columns["cmdIdDataGridViewTextBoxColumn"].Index;

                        string MidiDeviceName = (string)row.Cells[dnIdx].Value;
                        int catCmd = (int)row.Cells[cmdIdx].Value;
                        DB.RemoveMapping(MidiDeviceName, catCmd);
                        DB.SaveChanges(DeviceName);
                        _enumsDb.SetCatCmdInUse((CatCmd)catCmd, false);
                        MappedCommands mappedCommands = new MappedCommands(_enumsDb.ds.Tables["CatCmds"], _db.ds.Tables[DeviceName]);
                        mappedCommandsBindingSource.DataSource = mappedCommands;
                    }
                }
                catch { }
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == commandsTabPage)
            {
                MappedCommands mappedCommands = new MappedCommands(_enumsDb.ds.Tables["CatCmds"], _db.ds.Tables[DeviceName]);
                mappedCommandsBindingSource.DataSource = mappedCommands;
            }
            else if (tabControl.SelectedTab == mappedControlsTab)
            {
                CatCmdToUse = CatCmd.None;
            }
            else if (tabControl.SelectedTab == debugTab)
            {
                CatCmdToUse = CatCmd.None;
            }
        }

        private void validateDialogInput(object sender, EventArgs e)
        {
            ShowAddPrompt();
            ValidateDialogInput();
            eventMappingPrompt.Text = "";
            
            if (string.IsNullOrWhiteSpace(newValueReceivedEventTB.Text) == false)
            {
                string[] midiMsgErrors=midiDevice.ValidateMidiMessages(newValueReceivedEventTB.Text);
                if ( midiMsgErrors.Length==0 )
                {
                    eventMappingPrompt.Text += "New Value Received Event(s) formatted correctly.\n";
                }
                else
                {
                    eventMappingPrompt.Text += "New Value Received Event(s) NOT formatted correctly.\n";
                    foreach (string err in midiMsgErrors)
                    {
                        eventMappingPrompt.Text += err+"\n";
                    }
                }
            }

            if ( string.IsNullOrWhiteSpace(buttonDownEventTB.Text) == false)
            {
                string[] midiMsgErrors = midiDevice.ValidateMidiMessages(buttonDownEventTB.Text);
                if (midiMsgErrors.Length == 0)
                {
                    eventMappingPrompt.Text += "Button Down Event(s) formatted correctly.\n";
                }
                else
                {
                    eventMappingPrompt.Text += "Button Down Event(s) NOT formatted correctly.\n";
                    foreach (string err in midiMsgErrors)
                    {
                        eventMappingPrompt.Text += err + "\n";
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(buttonUpEventTB.Text) == false)
            {
                string[] midiMsgErrors = midiDevice.ValidateMidiMessages(buttonUpEventTB.Text);
                if (midiMsgErrors.Length == 0)
                {
                    eventMappingPrompt.Text += "Button Up Event(s) formatted correctly.";
                }
                else
                {
                    eventMappingPrompt.Text += "Button Up Event NOT formatted correctly.\n";
                    foreach (string err in midiMsgErrors)
                    {
                        eventMappingPrompt.Text += err + "\n";
                    }
                }
            }            
        }

        private void mappedCommandsGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void SendMidiCommand(int inChannel, int inValue, int inStatus, int inControl, ControllerMapping mapping)
        {
            if ((string.IsNullOrWhiteSpace(mapping.MidiOutCmdDown) == false) && inValue > 0)
            {
                midiDevice.SendMsg(inChannel, inValue, inStatus,inControl, mapping.MidiOutCmdDown);
            }
            else if ((string.IsNullOrWhiteSpace(mapping.MidiOutCmdUp) == false) && inValue <= 0)
            {
                midiDevice.SendMsg(inChannel, inValue, inStatus, inControl, mapping.MidiOutCmdUp);
            }
            else if ( (string.IsNullOrWhiteSpace(mapping.MidiOutCmdSetValue) == false) )
            {
                midiDevice.SendMsg(inChannel, inValue, inStatus, inControl, mapping.MidiOutCmdSetValue);
            }
        }

        private void mapControlToCommandGrid_Leave(object sender, EventArgs e)
        {

        }

        private void saveMappingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsDialog dlg = new SaveAsDialog();
            dlg.ExistingMappings = DB.GetSavedMappings();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
               DB.SaveMappingAs(DeviceName, dlg.MappingName, true);
               LoadedMappingLabel.Text = DB.GetLoadedMappingName(DeviceName);
            }
        }

        private void loadMappingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadDialog dlg = new LoadDialog();
            dlg.ExistingMappings = DB.GetSavedMappings();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tabControl.SelectedTab = mappedControlsTab;
                DB.LoadMapping(DeviceName, dlg.MappingName);
                LoadedMappingLabel.Text = DB.GetLoadedMappingName(DeviceName);
                promptPanel.Visible = false;
            }
        }

        private void exportMappingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PickDialog pickDlg = new PickDialog();
            pickDlg.Prompt="Pick the mappings to export";
            string[] existingMappings=DB.GetSavedMappings();
            if (existingMappings.Length <= 0)
            {
                MessageBox.Show("You can only export saved mappings,\nYou must save your current mapping before it can be exported", "Unable To Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            pickDlg.ExistingMappings = existingMappings;
            if (pickDlg.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = "";
                dlg.Filter = "Midi2Cat Files | *.m2c";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    DB.ExportMappings(dlg.FileName, pickDlg.Mappings);
                }
            }
        }

        private void importMappingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "";
            dlg.Filter = "Midi2Cat Files | *.m2c";
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (DB.ImportMappings(dlg.FileName) == false)
                {
                    MessageBox.Show("The import file is invalid or corrupt", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                PickDialog pickDlg = new PickDialog();
                pickDlg.Prompt = "Pick the mappings to import from the file.";
                pickDlg.ExistingMappings = DB.GetImportedMappings();
                if (pickDlg.ShowDialog() == DialogResult.OK)
                {
                    DB.AddFromImport(pickDlg.Mappings);
                }
            }
        }

        private void organiseMappingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OrganiseDialog dlg = new OrganiseDialog(DB, DeviceName);
            dlg.ExistingMappings =  DB.GetSavedMappings();
            dlg.ShowDialog();
            LoadedMappingLabel.Text = DB.GetLoadedMappingName(DeviceName);
        }


    }// class
}//namespace
