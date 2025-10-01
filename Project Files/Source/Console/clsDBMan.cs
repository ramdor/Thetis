/*  clsDBMan.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2000-2025 Original authors
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
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using System.Globalization;

namespace Thetis
{
    internal static class DBMan
    {
        private class DBSettings
        {
            public Guid ActiveDB_GUID { get; set; }
            public string ActiveDB_File {  get; set; }
            public DBSettings() 
            {
                ActiveDB_GUID = Guid.Empty;
                ActiveDB_File = "";
            }
        }
        public class DatabaseInfo
        {
            public Guid GUID { get; set; }
            public string FullPath { get; set; }
            public DateTime FolderCreationTime { get; set; }
            public long TotalContentsSize { get; set; }
            public long Size {  get; set; }
            public string Description { get; set; }
            public DateTime LastChanged {  get; set; }
            public DateTime CreationTime { get; set; }
            public string VersionString {  get; set; }
            public string VersionNumber { get; set; }
            public bool BackupOnStartup {  get; set; }
            public bool BackupOnShutdown {  get; set; }

            //[JsonConverter(typeof(StringEnumConverter))] //this will turn the enum (int) to a string
            [JsonConverter(typeof(DatabaseInfoDefaultStringEnumConverter), HPSDRModel.HERMES)] //[2.10.3.8]MW0LGE a custom string converter, that will default to HERMES if the model is not in the enum
            public HPSDRModel Model { get; set; }

            public class DatabaseInfoDefaultStringEnumConverter : StringEnumConverter
            {
                private readonly HPSDRModel _defaultValue;

                public DatabaseInfoDefaultStringEnumConverter(HPSDRModel defaultValue)
                {
                    _defaultValue = defaultValue;
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    try
                    {
                        return base.ReadJson(reader, objectType, existingValue, serializer);
                    }
                    catch
                    {
                        return _defaultValue;
                    }
                }
            }

            public DatabaseInfo()
            {
                GUID = Guid.Empty;
                FullPath = "";
                FolderCreationTime = DateTime.Now;
                TotalContentsSize = 0;
                //
                Size = 0;
                Description = "";
                LastChanged = DateTime.Now;
                Model = HPSDRModel.HERMES;
                CreationTime = DateTime.Now;
                VersionString = "unknown";
                VersionNumber = "unknown";
                //
                BackupOnStartup = false;
                BackupOnShutdown = false;
            }
        }
        public class BackupFileInfo
        {
            [JsonIgnore]
            public string FullFilePath { get; set; }
            [JsonIgnore]
            public DateTime DateTimeOfBackup { get; set; }
            [JsonIgnore]
            public long SecondsSinceEpoch { get; set; }
            [JsonIgnore]
            public TimeSpan AgeSinceBackedUp { get; set; }
            public string Description { get; set; }
            public bool Auto { get; set; }
            public BackupFileInfo()
            {
                Auto = false;
                Description = "Default";
            }
        }

        private static frmDBMan _frm_dbman;
        private static string _app_data_path;
        private static string _db_data_path;
        private static DBSettings _dbman_settings;
        private static bool _ignore_written;
        private static string _unique_instance_id;
        private static bool _prune_backups;
        static DBMan()
        {
            _ignore_written = false;
            _dbman_settings = null;
            _app_data_path = "";
            _db_data_path = "";
            _unique_instance_id = "";
            _prune_backups = false;
            _frm_dbman = new frmDBMan();
        }
        public static bool IsVisible
        {
            get
            {
                if (_frm_dbman == null) return false;
                return _frm_dbman.Visible;
            }
        }
        public static void ShowDBMan()
        {
            if (_dbman_settings == null) return;

            Console c = Console.getConsole();
            if (c.IsSetupFormNull) return;

            if (c.PowerOn)
            {
                DialogResult dr = MessageBox.Show("The Database Manager can not be used whilst the radio is powered on. The radio will be powered off.",
                "Database Manager Issue",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button2, Common.MB_TOPMOST);
                if (dr == DialogResult.OK)
                {
                    c.PowerOn = false;
                    if (c.PowerOn)
                    {
                        MessageBox.Show("Unable to power off the radio. You will need to do it manually and then try again.",
                        "Database Manager Issue",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                        return;
                    }
                }
                else return;
            }

            if (c.SetupForm.Visible)
            {
                DialogResult dr = MessageBox.Show("The Database Manager can not be used whilst the Setup window is shown. Please close it and try again.",
                "Database Manager Issue",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                return;
            }

            Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
            _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, Guid.Empty);

            _frm_dbman.Restore();
            _frm_dbman.PruneBackups = _prune_backups;
            _frm_dbman.ShowDialog();
        }

        public static string AppDataPath
        {
            set 
            { 
                _app_data_path = value;
                _db_data_path = _app_data_path + "DB\\";

                if (!Directory.Exists(_db_data_path))
                    Directory.CreateDirectory(_db_data_path);
            }
        }

        public static bool LoadDB(string[] args, out string broken_folder)
        {
            _dbman_settings = null;
            broken_folder = "";

            if (!Common.IsValidPath(_db_data_path))
            {
                DialogResult dr = MessageBox.Show("There is an issue with the database data path.\n\n" +
                "[" + _db_data_path + "]\n\n" + 
                "It is not valid. Please fix and try again.",
                "Database Manager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                return false;
            }
            foreach (string s in args)
            {
                if (s.StartsWith("-dbfilename:"))
                {
                    DialogResult dr = MessageBox.Show("-dbfilename: command line option is no longer supported.\n" +
                        "Please use -dbid: to provide the Database Manager with a unique ID to use for this instance.\n" +
                        "You can import your existing database using the Database Manager.",
                        "Database Manager",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                    break;
                }
                if (s.StartsWith("-dbid:"))
                {
                    _unique_instance_id = s.Trim().Substring(s.Trim().IndexOf(":") + 1) + "_";
                    // check if ok
                    if (!Common.IsValidFilename(_unique_instance_id + "dbman_settings.json"))
                    {
                        DialogResult dr = MessageBox.Show("There is an issue with the database dbman_settings file name.\n\n" +
                        "[" + _unique_instance_id + "dbman_settings.json" + "]\n\n" +
                        "It is not valid. Please fix and try again.",
                        "Database Manager",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                        return false;
                    }
                }
            }

            // shift key reset
            bool new_db_from_reset = false;
            if (Keyboard.IsKeyDown(Keys.LShiftKey) || Keyboard.IsKeyDown(Keys.RShiftKey))
            {
                Thread.Sleep(500); // ensure this is intentional
                if (Keyboard.IsKeyDown(Keys.LShiftKey) || Keyboard.IsKeyDown(Keys.RShiftKey))
                {
                    DialogResult dr = MessageBox.Show(
                         "The database reset function has been triggered. Would you like to use a fresh new database?\n\n" +
                         "Your existing database will be untouched, and a new one will be used.\n\n" +
                         "It will have the description 'Default' in the Database Manager.",
                         "New Database?",
                         MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, Common.MB_TOPMOST);

                    if (dr == DialogResult.Yes)
                        new_db_from_reset = true;
                }
            }
            // ctrl key upgrade
            bool ctrl_key_force_update = false;
            if (!new_db_from_reset && (Keyboard.IsKeyDown(Keys.LControlKey) || Keyboard.IsKeyDown(Keys.RControlKey)))
            {
                Thread.Sleep(500); // ensure this is intentional
                if (Keyboard.IsKeyDown(Keys.LControlKey) || Keyboard.IsKeyDown(Keys.RControlKey))
                {
                    DialogResult dr = MessageBox.Show(
                         "The database force update has been triggered. Do you want to do this?\n\n",                         
                         "Force Update Database?",
                         MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, Common.MB_TOPMOST);

                    if (dr == DialogResult.Yes)
                        ctrl_key_force_update = true;
                }
            }

            bool ok = false;
            bool made_new = false;
            bool old_db_found = false;

            Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();

            if (dbs.Count == 0 || new_db_from_reset)
            {
                // no dbs, likely due to fresh install, and/or first use of this new db manager
                ok = createNewDB(true, true, out old_db_found);
                if (ok)
                {
                    if(!old_db_found) made_new = true;
                    dbs = getAvailableDBs();
                    ok = dbs.Count > 0;
                }
            }            
            else
                ok = true;

            if (ok) {
                _dbman_settings = getActiveDB();
                if (_dbman_settings != null)
                {
                    //check this exists, if not?
                    string db_xml_file = _db_data_path + _dbman_settings.ActiveDB_GUID.ToString() + "\\database.xml";
                    if (!File.Exists(db_xml_file))
                    {
                        DialogResult dr = MessageBox.Show("The last active Database could not be located. Using a blank new one.",
                        "Database Manager Issue",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);

                        ok = createNewDB(false, true, out bool _);
                        if (ok)
                        {
                            made_new = true;
                            dbs = getAvailableDBs();
                            ok = dbs.Count > 0;// just a check to make sure at leas one exists
                            if (ok)
                            {
                                if (_dbman_settings != null)
                                {
                                    db_xml_file = _db_data_path + _dbman_settings.ActiveDB_GUID + "\\database.xml";
                                    try
                                    {
                                        ok = File.Exists(db_xml_file);
                                    }
                                    catch { ok = false; }
                                }
                                else
                                    ok = false;
                            }
                        }
                    }

                    if (ok)
                    {
                        bool did_backup = false;
                        //check for backup at startup
                        try
                        {
                            string json_file = _db_data_path + _dbman_settings.ActiveDB_GUID.ToString() + "\\dbman.json";
                            if (File.Exists(json_file))
                            {
                                string jsonString = File.ReadAllText(json_file);
                                DatabaseInfo di = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);
                                if (di.BackupOnStartup)
                                {
                                    did_backup = true;
                                    TakeBackup(Guid.Empty, "Startup", true);
                                }
                            }
                        }
                        catch { }
                        //

                        DB.FileName = db_xml_file;
                        _ignore_written = true;
                        ok = DB.Init();
                        _ignore_written = false;

                        //check version
                        if(ok) checkVersion(made_new, ctrl_key_force_update);

                        if(ok) // note, the TakeBackup above will not prune, as the DB has not been recovered for the flag PruneBackups
                        {
                            // prune
                            Dictionary<string, string> vals = DB.GetVarsDictionary("State");
                            bool prune = false;
                            if (vals.ContainsKey("PruneBackups"))
                                bool.TryParse(vals["PruneBackups"], out prune);

                            _frm_dbman.PruneBackups = prune;

                            if (prune && did_backup)
                            {
                                string directory_path = Path.GetDirectoryName(db_xml_file) + "\\backups";
                                pruneForGFS(directory_path);
                            }
                        }
                    }
                }
                else
                    ok = false;
            }
            
            if (!ok)
            {
                // try to move to broken folder
                if (_dbman_settings != null)
                {
                    broken_folder = _dbman_settings.ActiveDB_GUID.ToString();
                    moveToBroken(_dbman_settings.ActiveDB_GUID);
                }
            }

            return ok;
        }
        private static void checkVersion(bool made_new, bool force_upgrade = false)
        {
            string version;
            Dictionary<string, string> vals = DB.GetVarsDictionary("State");
            if (vals.ContainsKey("VersionNumber"))
                version = vals["VersionNumber"];
            else
                version = "? version";

            if (made_new) return;
            if (!force_upgrade && Common.GetVerNum() == version) return; // same version, dont need to do anything

            string force_info = force_upgrade ? "CTRL Key force DB update. " : "";
            
            DialogResult dr = MessageBox.Show(force_info + "This version [" + Common.GetVerNum() + "] of Thetis requires your database [" + version + "] to be updated.\n\n" +
                "A new updated database will be created, and your old database merged into it. It will be made active.",
                "Database Manager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);

            Guid guid_original = _dbman_settings == null ? Guid.Empty : _dbman_settings.ActiveDB_GUID;

            // need to create new fresh db
            string orginal_db_filename_xml = DB.FileName;
            bool ok = createNewDB(false, true, out bool _);

            if (ok) {
                // then import the one we were using into that new fresh one we just created
                // we need to ignore merged flag so that we can contine to use and save everyting to this database
                ok = DB.ImportAndMergeDatabase(orginal_db_filename_xml, out string log, true);
                try
                {
                    File.WriteAllText(_app_data_path + "ImportLog_dbupdate.txt", log);
                }
                catch { }
            }

            if (ok)
            {
                DBWritten(); // update json to reflect the merged info, like model etc

                // not sure we want to do this
                //// then delete the orginal
                //RemoveDB(guid_original, true);

                dr = MessageBox.Show("The database update was completed sucessfully.",
                    "Database Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }
            else
            {
                dr = MessageBox.Show("The database update did not complete.",
                    "Database Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }
        }
        private static void moveToBroken(Guid guid)
        {
            try
            {
                string db_folder = _db_data_path + guid.ToString();
                string dest_folder = _db_data_path + "broken";

                if (Directory.Exists(db_folder))
                {
                    if (!Directory.Exists(dest_folder))
                        Directory.CreateDirectory(dest_folder);

                    dest_folder += "\\" + guid.ToString();
                    Directory.Move(db_folder, dest_folder);
                }
            }
            catch { }
        }
        public static void Shutdown()
        {
            if (_dbman_settings == null) return;

            //check for backup at shutdown
            try
            {
                string json_file = _db_data_path + _dbman_settings.ActiveDB_GUID.ToString() + "\\dbman.json";
                if (File.Exists(json_file))
                {
                    string jsonString = File.ReadAllText(json_file);
                    DatabaseInfo di = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);
                    if (di.BackupOnShutdown)
                        TakeBackup(Guid.Empty, "Shutdown", true);
                }
            }
            catch { }
        }
        public static void DBWritten()
        {
            if (_ignore_written || _dbman_settings == null) return;

            // called by DB after a write
            // need to update json, as things will have changed
            string db_folder = _db_data_path + _dbman_settings.ActiveDB_GUID.ToString();
            string json_file = db_folder + "\\dbman.json";

            if (File.Exists(json_file))
            {
                // read current settings
                string jsonString = File.ReadAllText(json_file);
                DatabaseInfo di = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);

                // get db values
                Dictionary<string, string> options = DB.GetVarsDictionary("Options");
                if (options.ContainsKey("comboRadioModel"))
                    di.Model = HardwareSpecific.StringModelToEnum(options["comboRadioModel"]);
                else
                    di.Model = HPSDRModel.HERMES;

                di.VersionString = DB.VersionString;
                di.VersionNumber = DB.VersionNumber;

                // get whole folder size
                DirectoryInfo folderInfo = new DirectoryInfo(db_folder);
                di.TotalContentsSize = calculateFolderSize(folderInfo);

                // update db details
                FileInfo fi = new FileInfo(json_file);
                di.Size = fi.Length;
                di.LastChanged = fi.LastWriteTime;

                //write the updated version
                jsonString = JsonConvert.SerializeObject(di, Newtonsoft.Json.Formatting.Indented);
                try
                {
                    File.WriteAllText(json_file, jsonString);
                }
                catch (Exception)
                {
                }
            }
        }
        private static bool createNewDB(bool check_for_old_db, bool make_active, out bool old_db_found, string description = "")
        {
            bool ok = true;

            string db_folder = createNewDBFolder(out Guid guid);

            old_db_found = false;

            ok = !string.IsNullOrEmpty(db_folder);
            if (!ok) return false;

            string source_db = _app_data_path + "database.xml";
            string dest_db = db_folder + "\\database.xml";

            if (check_for_old_db && File.Exists(source_db))
            {
                // copy old db to new system, and then rename
                File.Copy(source_db, dest_db, true);

                string source_db_renamed = _app_data_path + "old_database.xml";
                int counter = 1;
                while (File.Exists(source_db_renamed))
                {
                    source_db_renamed = Path.Combine(_app_data_path, $"old{counter}_database.xml");
                    counter++;
                }

                //rename to old
                File.Move(source_db, source_db_renamed);

                old_db_found = true;
            }
            else
            {
                // make new db in new system, description Default, add to dbs
                // this will be done by the DB.Init() below if no db there
            }

            _ignore_written = true;
            // write and store existing
            string old_db_filename = "";
            if (!string.IsNullOrEmpty(DB.FileName))
            {
                old_db_filename = DB.FileName;
                DB.Exit();
            }

            // read db to get some basic info
            DB.FileName = dest_db;
            ok = DB.Init();
            _ignore_written = false;

            if (ok)
            {
                Dictionary<string, string> options = DB.GetVarsDictionary("Options");

                // add the new dbman.json
                DatabaseInfo di = new DatabaseInfo();
                di.GUID = guid;
                di.FullPath = db_folder;
                di.Description = string.IsNullOrEmpty(description) ? "Default" : description;

                if (options.ContainsKey("comboRadioModel"))
                    di.Model = HardwareSpecific.StringModelToEnum(options["comboRadioModel"]);

                di.VersionString = DB.VersionString;
                di.VersionNumber = DB.VersionNumber;
                
                string jsonString = JsonConvert.SerializeObject(di, Newtonsoft.Json.Formatting.Indented);
                try
                {
                    File.WriteAllText(db_folder + "\\dbman.json", jsonString);
                }
                catch (Exception)
                {
                    ok = false;
                }

                if (make_active && ok)
                {
                    //need to make this active
                    ok = makeDBActive(guid);
                    _dbman_settings = getActiveDB();
                }
            }

            if (!make_active && !string.IsNullOrEmpty(old_db_filename))
            {
                _ignore_written = true;
                DB.FileName = old_db_filename;
                DB.Init();
                _ignore_written = false;
            }

            return ok;
        }
        private static DBSettings getActiveDB()
        {
            bool ok = false;
            DBSettings dbs = null;
            try
            {
                string dbman_settings_file = _db_data_path + _unique_instance_id + "dbman_settings.json";
                if (File.Exists(dbman_settings_file))
                {
                    string jsonString = File.ReadAllText(dbman_settings_file);
                    dbs = JsonConvert.DeserializeObject<DBSettings>(jsonString);
                    ok = true;
                }
            }
            catch { }

            if (ok)
                return dbs;
            else
                return null;
        }
        private static bool makeDBActive(Guid guid)
        {
            bool ok = true;

            try
            {
                string db_filename = _db_data_path + guid.ToString() + "\\database.xml";
                if (File.Exists(db_filename))
                {
                    DBSettings dbs = new DBSettings();
                    dbs.ActiveDB_GUID = guid;
                    dbs.ActiveDB_File = guid.ToString() + "\\database.xml";

                    //write json
                    string jsonString = JsonConvert.SerializeObject(dbs, Newtonsoft.Json.Formatting.Indented);
                    try
                    {
                        string dbman_settings_file = _db_data_path + _unique_instance_id + "dbman_settings.json";
                        File.WriteAllText(dbman_settings_file, jsonString);
                    }
                    catch (Exception)
                    {
                        ok = false;
                    }
                }
                else
                    ok = false;
            }
            catch { ok = false; }

            return ok;
        }
        private static string createNewDBFolder(out Guid guid)
        {
            try
            {
                guid = Guid.NewGuid();
                string db_folder = _db_data_path + guid.ToString();
                Directory.CreateDirectory(db_folder);
                Directory.CreateDirectory(db_folder + "\\backups");

                return db_folder;
            }
            catch
            {
                guid = Guid.Empty;
                return "";
            }
        }
        private static Dictionary<Guid, DatabaseInfo> getAvailableDBs()
        {
            // find all active databases
            List<Guid> toJunk = new List<Guid>();
            Dictionary<Guid, DatabaseInfo> foldersInfo = new Dictionary<Guid, DatabaseInfo>();

            //check if we have a good dbman_settings.json, if not, return empty
            //will cause a new empty db to be made
            string db_man_settings_path = _db_data_path + _unique_instance_id + "dbman_settings.json";
            if (!File.Exists(db_man_settings_path)) return foldersInfo;

            // get all the active dbs from any .json file. We wont add any of these
            // folders if they are active with exception of ourself
            List<Guid> active_dbs = getAllActiveDBGUIDs(_db_data_path);

            try
            {
                string[] directories = Directory.GetDirectories(_db_data_path);

                foreach (string directory in directories)
                {
                    string folderName = Path.GetFileName(directory);
                    // check that the folder is a vaild guid
                    if (Guid.TryParse(folderName, out Guid folderGuid))
                    {
                        if (active_dbs.Contains(folderGuid)) continue; // skip this one as in use by another instance

                        DirectoryInfo folderInfo = new DirectoryInfo(directory);

                        //get info from database.xml
                        long db_size = 0;
                        DateTime last_change_time = DateTime.Now;
                        DateTime creation_time = DateTime.Now;
                        try
                        {
                            string db_filename_xml = directory + "\\database.xml";
                            if (File.Exists(db_filename_xml))
                            {
                                FileInfo fi = new FileInfo(db_filename_xml);
                                db_size = fi.Length;
                                last_change_time = fi.LastWriteTime;
                                creation_time = fi.CreationTime;
                            }
                        }
                        catch { continue; }

                        // get info from dbman.json
                        string version = "unknown";
                        string version_number = "unknown";
                        string desc = "";
                        HPSDRModel model = HPSDRModel.FIRST;
                        bool backup_on_startup = false;
                        bool backup_on_shutdown = false;
                        try
                        {
                            string dbman_json = directory + "\\dbman.json";
                            if (File.Exists(dbman_json))
                            {
                                string jsonString = File.ReadAllText(dbman_json);
                                DatabaseInfo db_info_json = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);

                                if (db_info_json.GUID != folderGuid)
                                {
                                    toJunk.Add(folderGuid);
                                    continue; // skip as, guids do not match, folder renamed? json edited?
                                }

                                desc = db_info_json.Description;
                                model = db_info_json.Model;
                                version = db_info_json.VersionString;
                                version_number = db_info_json.VersionNumber;
                                backup_on_startup = db_info_json.BackupOnStartup;
                                backup_on_shutdown = db_info_json.BackupOnShutdown;
                            }
                        }
                        catch { continue; }

                        //add info to the dict
                        DatabaseInfo folderInfoObject = new DatabaseInfo
                        {
                            GUID = folderGuid,
                            FullPath = folderInfo.FullName,
                            FolderCreationTime = folderInfo.CreationTime,
                            TotalContentsSize = calculateFolderSize(folderInfo),
                            //
                            Size = db_size,
                            Description = desc,
                            Model = model,
                            LastChanged = last_change_time,
                            CreationTime = creation_time,
                            VersionString = version,
                            VersionNumber = version_number,
                            BackupOnStartup = backup_on_startup,
                            BackupOnShutdown = backup_on_shutdown
                        };
                        foldersInfo.Add(folderGuid, folderInfoObject);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"An error occurred: {ex.Message}");
            }  
            
            foreach(Guid guid in toJunk)
            {
                moveToBroken(guid);
            }

            // if we have an active db remove it
            DatabaseInfo activeDB = null;
            if (_dbman_settings != null)
            {
                if (foldersInfo.ContainsKey(_dbman_settings.ActiveDB_GUID))
                {
                    activeDB = foldersInfo[_dbman_settings.ActiveDB_GUID];
                    foldersInfo.Remove(_dbman_settings.ActiveDB_GUID);
                }
            }

            // order the list
            IOrderedEnumerable<KeyValuePair<Guid, DatabaseInfo>> sortedEntries = foldersInfo
                .OrderByDescending(entry => entry.Value.LastChanged);
            Dictionary<Guid, DatabaseInfo> sortedDictionary = sortedEntries
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            // prepend the active so that it is always at top, irrespective of last changed
            if (activeDB != null)
            {
                sortedDictionary = sortedDictionary
                    .Prepend(new KeyValuePair<Guid, DatabaseInfo>(activeDB.GUID, activeDB))
                    .ToDictionary(entry => entry.Key, entry => entry.Value);
            }

            return sortedDictionary;
        }
        private static List<Guid> getAllActiveDBGUIDs(string path)
        {
            List<Guid> activeDbGuids = new List<Guid>();

            try
            {
                string[] dbmanSettingsFiles = Directory.GetFiles(path, "*dbman_settings.json", SearchOption.TopDirectoryOnly);
                string us = _unique_instance_id + "dbman_settings.json";

                foreach (string file in dbmanSettingsFiles)
                {
                    // ignore us
                    string fileName = Path.GetFileName(file);
                    if (fileName == us) continue;

                    try
                    {
                        string jsonString = File.ReadAllText(file);
                        DBSettings dbSettings = JsonConvert.DeserializeObject<DBSettings>(jsonString);
                        if (dbSettings != null && dbSettings.ActiveDB_GUID != Guid.Empty)
                        {
                            activeDbGuids.Add(dbSettings.ActiveDB_GUID);
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return activeDbGuids;
        }
        private static long calculateFolderSize(DirectoryInfo directoryInfo)
        {
            long totalSize = 0;
            try
            {
                FileInfo[] files = directoryInfo.GetFiles();
                foreach (FileInfo file in files)
                    totalSize += file.Length;

                DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
                foreach (DirectoryInfo dir in subDirectories)
                    totalSize += calculateFolderSize(dir);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.Print($"Access denied to directory: {directoryInfo.FullName}. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.Print($"Error processing directory: {directoryInfo.FullName}. Error: {ex.Message}");
            }

            return totalSize;
        }

        public static void MakeActiveDB(Guid guid)
        {
            DialogResult dr = MessageBox.Show("Do you want to activate the selected database? This will cause Thetis to restart.",
            "Database Manager Issue",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, Common.MB_TOPMOST);

            if(dr == DialogResult.Yes)
            {
                // update dbman_settings.json in DB folder
                bool ok = makeDBActive(guid);

                if (ok)
                {
                    _frm_dbman.Hide();

                    Console.getConsole().Restart = true;
                    Console.getConsole().Close();
                }
                else
                {
                    dr = MessageBox.Show("There was an issue making the database active.",
                    "Database Manager Issue",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                }
            }
        }
        public static void NewDB()
        {
            if (_dbman_settings == null) return;

            string desc = InputBox.Show("Database Description", "Please provide a description for this new database.", "", true);
            if (string.IsNullOrEmpty(desc)) return;

            createNewDB(false, false, out bool _, desc);

            Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
            _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
        }
        public static void BackupOnStartUpToggle(Guid guid)
        {
            if (_dbman_settings == null) return;

            string dbman_json = _db_data_path + guid.ToString() + "\\dbman.json";
            if (File.Exists(dbman_json))
            {
                try
                {
                    string jsonString = File.ReadAllText(dbman_json);
                    DatabaseInfo db_info_json = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);
                    //toggle
                    db_info_json.BackupOnStartup = !db_info_json.BackupOnStartup;

                    jsonString = JsonConvert.SerializeObject(db_info_json, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(dbman_json, jsonString);

                    Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
                    _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, guid);
                }
                catch { }
            }
        }
        public static void BackupOnShutDownToggle(Guid guid)
        {
            if (_dbman_settings == null) return;

            string dbman_json = _db_data_path + guid.ToString() + "\\dbman.json";
            if (File.Exists(dbman_json))
            {
                try
                {
                    string jsonString = File.ReadAllText(dbman_json);
                    DatabaseInfo db_info_json = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);
                    //toggle
                    db_info_json.BackupOnShutdown = !db_info_json.BackupOnShutdown;

                    jsonString = JsonConvert.SerializeObject(db_info_json, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(dbman_json, jsonString);

                    Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
                    _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, guid);
                }
                catch { }
            }
        }
        public static void RemoveDB(Guid guid, bool force = false)
        {
            if (_dbman_settings == null) return;

            bool key_force = Common.ShiftKeyDown;// && Common.CtrlKeyDown;
            if (key_force)
            {
                DialogResult dr = MessageBox.Show("Force delete detected. Are you sure?",
                "Database Manager Issue",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, Common.MB_TOPMOST);
                if (dr == DialogResult.Yes)
                    force = true;
            }
            string desc = "";
            if (!force)
            {
                desc = InputBox.Show("Database Removal", "Please enter the matching description to remove this database.", "", true);
                if (string.IsNullOrEmpty(desc)) return;
            }

            bool ok = false;
            string dbman_json = _db_data_path + guid.ToString() + "\\dbman.json";
            if (File.Exists(dbman_json))
            {
                string jsonString = File.ReadAllText(dbman_json);
                DatabaseInfo db_info_json = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);
                if (force || (desc == db_info_json.Description))
                {
                    try
                    {
                        Directory.Delete(_db_data_path + guid.ToString(), true);
                        ok = true;

                        Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
                        _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
                    }
                    catch { }
                }
                else
                    ok = true; // just to bypass the error
            }
            else if(key_force)
            {
                if (Directory.Exists(_db_data_path + guid.ToString()))
                {
                    // just delete it
                    try
                    {
                        Directory.Delete(_db_data_path + guid.ToString(), true);
                        ok = true;

                        Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
                        _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
                    }
                    catch { }
                }
            }
            if (!ok)
            {
                DialogResult dr = MessageBox.Show("There was an issue removing the database.",
                "Database Manager Issue",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }
        }
        public static void DuplicateDB(Guid guid)
        {
            if (_dbman_settings == null) return;

            string desc = InputBox.Show("Database Duplication", "Please enter a description for the duplicate.", "", true);
            if (string.IsNullOrEmpty(desc)) return;

            // make new folder with new guid
            // copy over contents, read in, then update and write back out
            Guid new_guid = Guid.NewGuid();
            string source_folder = _db_data_path + guid.ToString();
            string dest_folder = _db_data_path + new_guid.ToString();

            bool ok = copyFolder(source_folder, dest_folder);
            if (ok)
            {
                try
                {
                    string dbman_json = dest_folder + "\\dbman.json";
                    if (File.Exists(dbman_json))
                    {
                        string jsonString = File.ReadAllText(dbman_json);
                        DatabaseInfo db_info_json = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);

                        db_info_json.GUID = new_guid;
                        db_info_json.FullPath = dest_folder;
                        db_info_json.CreationTime = DateTime.Now;
                        db_info_json.FolderCreationTime = DateTime.Now;
                        db_info_json.Description = desc;
                        db_info_json.LastChanged = DateTime.Now;

                        jsonString = JsonConvert.SerializeObject(db_info_json, Newtonsoft.Json.Formatting.Indented);
                        File.WriteAllText(dbman_json, jsonString);
                    }
                }
                catch { ok = false; }

                if (ok)
                {
                    Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
                    _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
                }
            }

            if(!ok)
            {
                DialogResult dr = MessageBox.Show("There was an issue duplicating the database.",
                "Database Manager Issue",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }
        }

        public static bool copyFolder(string sourceFolder, string destinationFolder)
        {
            try
            {
                Directory.CreateDirectory(destinationFolder);

                string[] files = Directory.GetFiles(sourceFolder);
                for (int i = 0; i < files.Length; i++)
                {
                    string filePath = files[i];
                    string fileName = Path.GetFileName(filePath);
                    string destFile = Path.Combine(destinationFolder, fileName);
                    File.Copy(filePath, destFile);
                }

                string[] subdirectories = Directory.GetDirectories(sourceFolder);
                for (int i = 0; i < subdirectories.Length; i++)
                {
                    string subdirPath = subdirectories[i];
                    string subdirName = Path.GetFileName(subdirPath);
                    string destSubdir = Path.Combine(destinationFolder, subdirName);
                    copyFolder(subdirPath, destSubdir);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static void SelectedAvailable(Guid guid)
        {
            if(guid == Guid.Empty)
            {
                if (_dbman_settings != null)
                {
                    guid = _dbman_settings.ActiveDB_GUID;
                }
                else
                {
                    List<BackupFileInfo> empty_backups = new List<BackupFileInfo>();
                    _frm_dbman.InitBackups(empty_backups);
                    return;
                }
            }

            string backup_path = _db_data_path + guid.ToString() + "\\backups";
            List<BackupFileInfo> backups = getOrderedBackupFiles(backup_path);
            _frm_dbman.InitBackups(backups);
        }
        public static bool TakeBackup(Guid highlighted, string description = "", bool auto = false)
        {
            if (_dbman_settings == null) return false;

            string desc;
            if (string.IsNullOrEmpty(description))
            {
                desc = InputBox.Show("Database Backup", "Please enter a description for the backup.", "", true);
                if (string.IsNullOrEmpty(desc)) return false;
            }
            else
                desc = description;

            bool ok = false;
            Guid guid = _dbman_settings.ActiveDB_GUID;

            string backup_directory = _db_data_path + guid.ToString() + "\\backups";
            try
            {
                // TODO: backup limits GFS
                string backup_filename = "";                
                string db_filename_xml = _db_data_path + guid.ToString() + "\\database.xml";
                if (File.Exists(db_filename_xml) && Directory.Exists(backup_directory))
                {
                    backup_filename = createUniqueFilename(backup_directory);

                    File.Copy(db_filename_xml, backup_filename, true);
                    ok = true;
                }
                if (ok)
                {
                    //copied ok, make json to store desc
                    //same as backup_filename, but with .json extension instead
                    BackupFileInfo bfi = new BackupFileInfo()
                    {
                        Auto = auto,
                        Description = desc
                    };
                    string jsonFilePath = System.IO.Path.ChangeExtension(backup_filename, ".json");
                    string jsonString = JsonConvert.SerializeObject(bfi, Newtonsoft.Json.Formatting.Indented);
                    try
                    {
                        File.WriteAllText(jsonFilePath, jsonString);
                    }
                    catch
                    {
                        ok = false;
                    }
                }
            }
            catch { }

            if (ok && _prune_backups)
            {
                pruneForGFS(backup_directory);
            }

            if (highlighted != Guid.Empty)
                guid = highlighted;

            getBackups(guid);

            return ok;
        }
        private static void getBackups(Guid guid)
        {
            string backup_path = _db_data_path + guid.ToString() + "\\backups";
            List<BackupFileInfo> backups = getOrderedBackupFiles(backup_path);
            _frm_dbman.InitBackups(backups);
        }
        private static List<BackupFileInfo> getOrderedBackupFiles(string backupFolderPath)
        {            
            List<BackupFileInfo> backupFiles = new List<BackupFileInfo>();
            if (!Directory.Exists(backupFolderPath)) return backupFiles;
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            foreach (string filePath in Directory.GetFiles(backupFolderPath, "database_backup_*.xml"))
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string[] parts = fileName.Split('_');

                    if (parts.Length >= 3 && long.TryParse(parts[2], out long secondsSinceEpoch)) // 3 as database_backup_342423432.xml  could also be database_backup_342423432_1.xml
                    {
                        DateTime backupDateTimeUtc = epoch.AddSeconds(secondsSinceEpoch);
                        DateTime backupDateTimeLocal = backupDateTimeUtc.ToLocalTime();
                        TimeSpan age = DateTime.UtcNow - backupDateTimeUtc;

                        string jsonFilePath = System.IO.Path.ChangeExtension(filePath, ".json");
                        string desc = "Default";
                        bool auto = false;
                        if (File.Exists(jsonFilePath))
                        {
                            string jsonString = File.ReadAllText(jsonFilePath);
                            BackupFileInfo backup_info_json = JsonConvert.DeserializeObject<BackupFileInfo>(jsonString);
                            desc = backup_info_json.Description;
                            auto = backup_info_json.Auto;
                        }
                        backupFiles.Add(new BackupFileInfo
                        {
                            FullFilePath = filePath,
                            DateTimeOfBackup = backupDateTimeLocal,
                            SecondsSinceEpoch = secondsSinceEpoch,
                            AgeSinceBackedUp = age,
                            Description = desc,
                            Auto = auto
                        });
                    }
                }
                catch { }
            }

            return backupFiles.OrderByDescending(f => f.SecondsSinceEpoch).ToList();
        }
        private static string createUniqueFilename(string directoryPath)
        {
            //DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //long secondsSinceEpoch = (long)(DateTime.UtcNow - epoch).TotalSeconds;
            DateTimeOffset now = DateTimeOffset.UtcNow;
            long secondsSinceEpoch = now.ToUnixTimeSeconds();

            string baseFilename = $"database_backup_{secondsSinceEpoch}";
            string fullPath = Path.Combine(directoryPath, $"{baseFilename}.xml");

            int counter = 1;
            while (File.Exists(fullPath))
            {
                fullPath = Path.Combine(directoryPath, $"{baseFilename}_{counter}.xml");
                counter++;
            }

            return fullPath;
        }
        public static void RemoveBackupDB(List<string> file_paths)
        {
            if(file_paths.Count < 1) return;

            string msg = file_paths.Count == 1 ? "Do you want to remove this backup?" : $"Do you want to remove these {file_paths.Count} backups?";
            DialogResult dr = MessageBox.Show(msg,
            "Remove Backup",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, Common.MB_TOPMOST);

            if (dr == DialogResult.Yes)
            {
                foreach (string file_path in file_paths)
                {
                    if (File.Exists(file_path))
                    {
                        try
                        {
                            File.Delete(file_path);
                        }
                        catch { }
                    }
                    string jsonFilePath = System.IO.Path.ChangeExtension(file_path, ".json");
                    if (File.Exists(jsonFilePath))
                    {
                        try
                        {
                            File.Delete(jsonFilePath);
                        }
                        catch { }
                    }
                }
            }
        }
        public static void Import() 
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            openFileDialog.Title = "Select an XML file";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                if (Path.GetExtension(filename).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    bool ok;
                    try
                    {
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.Load(filename);
                        ok = true;
                    }
                    catch
                    {
                        ok = false;
                    }
                    if (ok)
                    {
                        ok = DB.ImportAndMergeDatabase(filename, out string log, false);
                        try
                        {
                            File.WriteAllText(_app_data_path + "ImportLog.txt", log);
                        }
                        catch { }
                        if (ok)
                        {
                            DialogResult dr = MessageBox.Show("The database was imported sucessfully. Thetis will now restart.",
                            "Database Manager",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);

                            _frm_dbman.Hide();

                            Console.getConsole().Restart = true;
                            Console.getConsole().Close();
                        }
                        else
                        {
                            DialogResult dr = MessageBox.Show("There was a problem importing the database. The database file seems to be corrupt.",
                            "Database Manager",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                        }
                    }
                    else
                    {
                        DialogResult dr = MessageBox.Show("There was a problem importing the database. The xml file seems to be corrupt.",
                        "Database Manager",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                    }
                }
                else
                {
                    DialogResult dr = MessageBox.Show("The database file needs a .xml file extension.",
                    "Database Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                }
            }
        }
        public static void ImportAsAvailable(Guid selected)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            openFileDialog.Title = "Select an XML file";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                if (Path.GetExtension(filename).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    bool ok;
                    try
                    {
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.Load(filename);
                        ok = true;
                    }
                    catch
                    {
                        ok = false;
                    }
                    if (ok)
                    {
                        string desc = InputBox.Show("Database Import", "Please enter a description for the imported database.", "", true);
                        if (string.IsNullOrEmpty(desc)) return;

                        // get some basic info from the db that is being imported
                        _ignore_written = true;
                        // write and store existing
                        string old_db_filename = "";
                        if (!string.IsNullOrEmpty(DB.FileName))
                        {
                            old_db_filename = DB.FileName;
                            DB.Exit();
                        }
                        // read db to get some basic info
                        DB.FileName = filename;
                        ok = DB.Init();
                        _ignore_written = false;

                        HPSDRModel model = HPSDRModel.HERMES;
                        string version = "";
                        string version_number = "";
                        if (ok)
                        {
                            // get db values
                            Dictionary<string, string> options = DB.GetVarsDictionary("Options");
                            if (options.ContainsKey("comboRadioModel"))
                                model = HardwareSpecific.StringModelToEnum(options["comboRadioModel"]);
                            else
                                model = HPSDRModel.HERMES;

                            version = DB.VersionString;
                            version_number = DB.VersionNumber;
                        }

                        // restore DB
                        if (!string.IsNullOrEmpty(old_db_filename))
                        {
                            _ignore_written = true;
                            DB.FileName = old_db_filename;
                            DB.Init();
                            _ignore_written = false;
                        }

                        if (ok)
                        {
                            string dest_file = "";
                            string path = createNewDBFolder(out Guid guid);
                            ok = !string.IsNullOrEmpty(path);
                            if (ok)
                            {
                                //copy
                                try
                                {
                                    dest_file = path + "\\database.xml";
                                    File.Copy(filename, dest_file);
                                    ok = File.Exists(dest_file);
                                }
                                catch
                                {
                                    ok = false;
                                }

                                if (ok)
                                {
                                    // create new json file
                                    DirectoryInfo folderInfo = new DirectoryInfo(path);
                                    FileInfo fileInfo = new FileInfo(dest_file);

                                    DatabaseInfo di = new DatabaseInfo
                                    {
                                        GUID = guid,
                                        FullPath = path,
                                        FolderCreationTime = DateTime.Now,
                                        TotalContentsSize = calculateFolderSize(folderInfo),
                                        //
                                        Size = fileInfo.Length,
                                        Description = desc,
                                        Model = model,
                                        LastChanged = fileInfo.LastWriteTime,
                                        CreationTime = fileInfo.CreationTime,
                                        VersionString = version,
                                        VersionNumber = version_number,
                                        BackupOnStartup = false,
                                        BackupOnShutdown = false
                                    };

                                    //write the updated version
                                    string json_file = path + "\\dbman.json";
                                    string jsonString = JsonConvert.SerializeObject(di, Newtonsoft.Json.Formatting.Indented);
                                    try
                                    {
                                        File.WriteAllText(json_file, jsonString);
                                    }
                                    catch (Exception)
                                    {
                                        ok = false;
                                        DialogResult dr = MessageBox.Show("There was a problem writing the database info. Unable to copy the source database file.",
                                        "Database Manager",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                                    }
                                    if (ok)
                                    {
                                        Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
                                        _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
                                    }
                                }
                                else
                                {
                                    DialogResult dr = MessageBox.Show("There was a problem importing the database. Unable to copy the source database file.",
                                    "Database Manager",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                                }
                            }
                            else
                            {
                                DialogResult dr = MessageBox.Show("There was a problem importing the database.",
                                "Database Manager",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                            }
                        }
                        else
                        {
                            DialogResult dr = MessageBox.Show("There was a problem importing the database. The database file seems to be corrupt.",
                            "Database Manager",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                        }
                    }
                    else
                    {
                        DialogResult dr = MessageBox.Show("There was a problem importing the database. The xml file seems to be corrupt.",
                        "Database Manager",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                    }
                }
                else
                {
                    DialogResult dr = MessageBox.Show("The database file needs a .xml file extension.",
                    "Database Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                }
            }
        }
        public static void Rename(Guid guid)
        {
            if (_dbman_settings == null) return;

            string db_path = _db_data_path + guid.ToString();
            string dbman_json = db_path + "\\dbman.json";
            if (!File.Exists(dbman_json)) return;

            try
            {
                string jsonString = File.ReadAllText(dbman_json);
                DatabaseInfo db_info_json = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);

                string desc = InputBox.Show("Database Change Description", "Please edit the description.", db_info_json.Description, true);
                if (string.IsNullOrEmpty(desc) || desc == db_info_json.Description) return;

                db_info_json.Description = desc;

                //write the updated version
                jsonString = JsonConvert.SerializeObject(db_info_json, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(dbman_json, jsonString);

                Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
                _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, guid);
            }
            catch { }
        }
        public static void RenameBackup(Guid guid, string file_path)
        {
            string jsonFilePath = System.IO.Path.ChangeExtension(file_path, ".json");
            bool file_to_read = File.Exists(jsonFilePath);

            try
            {
                string tmp_desc;
                BackupFileInfo backup_info_json;
                string jsonString;
                bool auto;

                if (file_to_read)
                {
                    jsonString = File.ReadAllText(jsonFilePath);
                    backup_info_json = JsonConvert.DeserializeObject<BackupFileInfo>(jsonString);
                    tmp_desc = backup_info_json.Description;
                    auto = backup_info_json.Auto;
                }
                else
                {
                    auto = false;
                    tmp_desc = "Default";
                }

                string desc = InputBox.Show("Database Change Description", "Please edit the description.", tmp_desc, true);
                if (string.IsNullOrEmpty(desc) || desc == tmp_desc) return;

                backup_info_json = new BackupFileInfo();
                backup_info_json.Description = desc;
                backup_info_json.Auto = auto;

                //write the updated version
                jsonString = JsonConvert.SerializeObject(backup_info_json, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonFilePath, jsonString);

                if (guid == Guid.Empty)
                {
                    if (_dbman_settings != null)
                    {
                        guid = _dbman_settings.ActiveDB_GUID;
                    }
                    else
                    {
                        List<BackupFileInfo> empty_backups = new List<BackupFileInfo>();
                        _frm_dbman.InitBackups(empty_backups);
                        return;
                    }
                }

                string backup_path = _db_data_path + guid + "\\backups";
                List<BackupFileInfo> backups = getOrderedBackupFiles(backup_path);
                _frm_dbman.InitBackups(backups);
            }
            catch { }
        }
        public static void OpenFolder(Guid guid)
        {
            if (guid == Guid.Empty && _dbman_settings != null )
                guid = _dbman_settings.ActiveDB_GUID;

            if (guid == Guid.Empty) return;

            string folder_path = _db_data_path + guid.ToString();
            try
            {
                if (Directory.Exists(folder_path))
                {
                    Process.Start("explorer.exe", folder_path);
                }
            }
            catch { }
        }
        public static void Export(Guid guid)
        {
            string db_path = _db_data_path + guid.ToString();
            string dbman_json = db_path + "\\dbman.json";
            if (File.Exists(dbman_json))
            {
                string jsonString = File.ReadAllText(dbman_json);
                DatabaseInfo db_info_json = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);
                string desc = db_info_json.Description;
                string datetime = Common.DateTimeStringForFile();
                string save_file = $"Thetis_database_export_{desc}_{datetime}.xml";
                string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                    DefaultExt = "xml",
                    FileName = save_file,
                    Title = "Export Database",
                    InitialDirectory = myDocumentsPath
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName;

                    try
                    {
                        if (File.Exists(filename))
                            File.Delete(filename);
                    }
                    catch { }

                    if (guid == _dbman_settings.ActiveDB_GUID)
                    {
                        try
                        {
                            bool ok = DB.WriteDB(filename);
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            File.Copy(db_path + "\\database.xml", filename);
                        }
                        catch { }
                    }
                }
            }
        }
        public static void ExportBackup(string desc, string path)
        {
            if (File.Exists(path))
            {
                string datetime = Common.DateTimeStringForFile();
                string save_file;
                if(string.IsNullOrEmpty(desc))
                    save_file = $"Thetis_database_export_backup_{datetime}.xml";
                else
                    save_file = $"Thetis_database_export_backup_{desc}_{datetime}.xml";

                string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                    DefaultExt = "xml",
                    FileName = save_file,
                    Title = "Export Database",
                    InitialDirectory = myDocumentsPath
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName;

                    try
                    {
                        if (File.Exists(filename))
                            File.Delete(filename);
                    }
                    catch { }
                    try
                    {
                        File.Copy(path, filename);
                    }
                    catch { }
                }
            }
        }
        public static void MakeBackupAvailable(string file_path)
        {
            if (_dbman_settings == null) return;

            if (File.Exists(file_path))
            {
                string desc = InputBox.Show("Make Database Available", "Please enter a description for the database.", "", true);
                if (string.IsNullOrEmpty(desc)) return;

                try
                {
                    // make new guid folder + backup folder
                    Guid guid = Guid.NewGuid();
                    string db_folder = _db_data_path + guid.ToString();
                    string db_folder_backups = _db_data_path + guid.ToString() + "\\backups";
                    string db_file = db_folder + "\\database.xml";
                    string json_file = db_folder + "\\dbman.json";

                    if (!Directory.Exists(db_folder))
                        Directory.CreateDirectory(db_folder);

                    if (!Directory.Exists(db_folder_backups))
                        Directory.CreateDirectory(db_folder_backups);

                    // copy over xml
                    File.Copy(file_path, db_file, true);

                    // obtain db imfo
                    HPSDRModel model = HPSDRModel.HERMES;
                    string version = "unknown";
                    string version_number = "unknown";

                    _ignore_written = true;
                    // write and store existing
                    string old_db_filename = "";
                    if (!string.IsNullOrEmpty(DB.FileName))
                    {
                        old_db_filename = DB.FileName;
                        DB.Exit();
                    }

                    // read db to get some basic info
                    DB.FileName = db_file;
                    bool ok = DB.Init();
                    _ignore_written = false;

                    // get db values
                    Dictionary<string, string> options = DB.GetVarsDictionary("Options");
                    if (options.ContainsKey("comboRadioModel"))
                        model = HardwareSpecific.StringModelToEnum(options["comboRadioModel"]);
                    else
                        model = HPSDRModel.HERMES;

                    version = DB.VersionString;
                    version_number = DB.VersionNumber;

                    // restore DB
                    if (!string.IsNullOrEmpty(old_db_filename))
                    {
                        _ignore_written = true;
                        DB.FileName = old_db_filename;
                        DB.Init();
                        _ignore_written = false;
                    }

                    // setup database info json
                    DirectoryInfo folderInfo = new DirectoryInfo(db_folder);
                    FileInfo fi = new FileInfo(db_file);
                    DatabaseInfo di = new DatabaseInfo
                    {
                        GUID = guid,
                        FullPath = folderInfo.FullName,
                        FolderCreationTime = folderInfo.CreationTime,
                        TotalContentsSize = calculateFolderSize(folderInfo),
                        //
                        Size = fi.Length,
                        Description = desc,
                        Model = model,
                        LastChanged = fi.LastWriteTime,
                        CreationTime = fi.CreationTime,
                        VersionString = version,
                        VersionNumber = version_number
                    };

                    //write
                    string jsonString = JsonConvert.SerializeObject(di, Newtonsoft.Json.Formatting.Indented);
                    try
                    {
                        File.WriteAllText(json_file, jsonString);
                    }
                    catch (Exception)
                    {
                    }

                    Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
                    _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
                }
                catch { }
            }
        }

        private static int getWeekOfYear(DateTime date)
        {
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.CurrentCulture;
            return cul.Calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        public static bool PruneBackups
        {
            get { return _prune_backups; }
            set { _prune_backups = value; }
        }
        private static void pruneForGFS(string backup_folder_path)
        {
            if (!_prune_backups || !Directory.Exists(backup_folder_path)) return;

            bool ok = false;
            List<FileInfo> keep_backups = null;
            List<FileInfo> backups = null;

            try
            {
                backups = new DirectoryInfo(backup_folder_path).GetFiles("*.xml")
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                DateTime now = DateTime.Now;

                List<FileInfo> last_7_days = backups
                    .Where(f => (now - f.CreationTime).TotalDays <= 7)
                    .ToList();

                List<FileInfo> weekly_backups = backups
                    .Where(f => (now - f.CreationTime).TotalDays > 7)
                    .GroupBy(f => new { Year = f.CreationTime.Year, Week = getWeekOfYear(f.CreationTime) })
                    .Select(g => g.OrderByDescending(f => f.CreationTime).First())
                    .ToList();

                List<FileInfo> monthly_backups = backups
                    .Where(f => (now - f.CreationTime).TotalDays > 30)
                    .GroupBy(f => new { Year = f.CreationTime.Year, Month = f.CreationTime.Month })
                    .Select(g => g.OrderByDescending(f => f.CreationTime).First())
                    .ToList();

                List<FileInfo> yearly_backups = backups
                    .Where(f => (now - f.CreationTime).TotalDays > 365)
                    .GroupBy(f => f.CreationTime.Year)
                    .Select(g => g.OrderByDescending(f => f.CreationTime).First())
                    .ToList();

                keep_backups = last_7_days
                    .Concat(weekly_backups)
                    .Concat(monthly_backups)
                    .Concat(yearly_backups)
                    .Distinct()
                    .ToList();

                ok = true;
            }
            catch { }

            if (ok)
            {
                foreach (FileInfo backup in backups)
                {
                    if (!keep_backups.Contains(backup))
                    {
                        try
                        {
                            bool is_auto = false;
                            string json_file_path = Path.ChangeExtension(backup.FullName, ".json");
                            if (File.Exists(json_file_path))
                            {
                                string jsonString = File.ReadAllText(json_file_path);
                                BackupFileInfo backup_info_json = JsonConvert.DeserializeObject<BackupFileInfo>(jsonString);

                                is_auto = backup_info_json.Auto || backup_info_json.Description == "Startup" || backup_info_json.Description == "Shutdown";
                            }

                            if (is_auto)
                            {
                                backup.Delete();

                                if (File.Exists(json_file_path))
                                {
                                    File.Delete(json_file_path);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
