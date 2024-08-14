using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Threading;
using System.Xml;
using System.Runtime.InteropServices;

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

            [JsonConverter(typeof(StringEnumConverter))] //this will turn the enum (int) to a string
            public HPSDRModel Model { get; set; }

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
            public string FullFilePath { get; set; }
            public DateTime DateTimeOfBackup { get; set; }
            public long SecondsSinceEpoch { get; set; }
            public TimeSpan AgeSinceBackedUp { get; set; }
        }

        private static frmDBMan _frm_dbman;
        private static string _app_data_path;
        private static string _db_data_path;
        private static DBSettings _dbman_settings;
        private static bool _ignore_written;
        private static string _unique_instance_id;
        static DBMan()
        {
            _ignore_written = false;
            _dbman_settings = null;
            _app_data_path = "";
            _db_data_path = "";
            _unique_instance_id = "";
            _frm_dbman = new frmDBMan();
        }

        public static void ShowDBMan()
        {
            if (_dbman_settings == null) return;

            Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
            _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, Guid.Empty);

            _frm_dbman.Restore();
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
                }
            }

            //
            bool new_db = false;
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
                         MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, Common.MB_TOPMOST);

                    if (dr == DialogResult.Yes)
                        new_db = true;
                }
            }
            //

            bool ok = false;
            bool made_new = false;

            Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();

            if (dbs.Count == 0 || new_db)
            {
                // no dbs, likely due to fresh install, and/or first use of this new db manager
                ok = createNewDB(true, true);
                if (ok)
                {
                    made_new = true;
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

                        ok = createNewDB(false, true);
                        if (ok)
                        {
                            made_new = true;
                            dbs = getAvailableDBs();
                            ok = dbs.Count > 0;
                        }
                    }

                    if (ok)
                    {
                        //check for backup at startup
                        try
                        {
                            string json_file = _db_data_path + _dbman_settings.ActiveDB_GUID.ToString() + "\\dbman.json";
                            if (File.Exists(json_file))
                            {
                                string jsonString = File.ReadAllText(json_file);
                                DatabaseInfo di = JsonConvert.DeserializeObject<DatabaseInfo>(jsonString);
                                if (di.BackupOnStartup)
                                    TakeBackup(Guid.Empty);
                            }
                        }
                        catch { }
                        //

                        DB.FileName = db_xml_file;
                        _ignore_written = true;
                        ok = DB.Init();
                        _ignore_written = false;

                        //check version
                        if(ok) checkVersion(made_new);
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
        private static void checkVersion(bool made_new)
        {
            string version;
            Dictionary<string, string> vals = DB.GetVarsDictionary("State");
            if (vals.ContainsKey("VersionNumber"))
                version = vals["VersionNumber"];
            else
                version = "? version";

            if (made_new || Common.GetVerNum() == version) return; // same version, dont need to do anything

            DialogResult dr = MessageBox.Show("This version [" + Common.GetVerNum() + "] of Thetis requires your database [" + version + "] to be updated.\n\n" +
                "A new updated database will be created, and your old database merged into it. It will be made active.",
                "Database Manager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);

            Guid guid_original = _dbman_settings == null ? Guid.Empty : _dbman_settings.ActiveDB_GUID;

            // need to create new fresh db
            string orginal_db_filename_xml = DB.FileName;
            bool ok = createNewDB(false, true);

            if (ok) {
                // then import the one we were using into that new fresh one we just created
                // we need to ignore merged flag so that we can contine to use and save everyting to this database
                ok = DB.ImportAndMergeDatabase2(orginal_db_filename_xml, out string log, true);
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
                        TakeBackup(Guid.Empty);
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
                    di.Model = Common.StringModelToEnum(options["comboRadioModel"]);
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
                catch (Exception ex)
                {
                }
            }
        }
        private static bool createNewDB(bool check_for_old_db = false, bool make_active = false, string description = "")
        {
            bool ok = true;

            string db_folder = createNewDBFolder(out Guid guid);

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

                File.Move(source_db, source_db_renamed);
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
                //DB.WriteDB();
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
                    di.Model = Common.StringModelToEnum(options["comboRadioModel"]);

                di.VersionString = DB.VersionString;
                di.VersionNumber = DB.VersionNumber;
                
                string jsonString = JsonConvert.SerializeObject(di, Newtonsoft.Json.Formatting.Indented);
                try
                {
                    File.WriteAllText(db_folder + "\\dbman.json", jsonString);
                }
                catch (Exception ex)
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
                    catch (Exception ex)
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

            IOrderedEnumerable<KeyValuePair<Guid, DatabaseInfo>> sortedEntries = foldersInfo
                .OrderByDescending(entry => entry.Value.LastChanged);
            Dictionary<Guid, DatabaseInfo> sortedDictionary = sortedEntries
                .ToDictionary(entry => entry.Key, entry => entry.Value);

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

            string desc = InputBox.Show("Database Description", "Please provide a description for this new database.", "");
            if (string.IsNullOrEmpty(desc)) return;

            createNewDB(false, false, desc);

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

            string desc = "";
            if (!force)
            {
                desc = InputBox.Show("Database Removal", "Please enter the matching description to remove this database.", "");
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

            string desc = InputBox.Show("Database Duplication", "Please enter a description for the duplicate.", "");
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
                List<BackupFileInfo> empty_backups = new List<BackupFileInfo>();
                _frm_dbman.InitBackups(empty_backups);
                return;
            }

            string backup_path = _db_data_path + guid.ToString() + "\\backups";
            List<BackupFileInfo> backups = getOrderedBackupFiles(backup_path);
            _frm_dbman.InitBackups(backups);
        }
        public static bool TakeBackup(Guid highlighted)
        {
            if (_dbman_settings == null) return false;

            bool ok = false;
            Guid guid = _dbman_settings.ActiveDB_GUID;

            try
            {
                // TODO: backup limits
                string db_filename_xml = _db_data_path + guid.ToString() + "\\database.xml";
                if (File.Exists(db_filename_xml))
                {
                    string backup_directory = _db_data_path + guid.ToString() + "\\backups";
                    string backup_filename = createUniqueFilename(backup_directory);

                    File.Copy(db_filename_xml, backup_filename, true);
                    ok = true;
                }
            }
            catch { }

            if (ok && highlighted != Guid.Empty & guid == highlighted)
            {
                getBackups(guid);
            }

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
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] parts = fileName.Split('_');

                if (parts.Length >= 3 && long.TryParse(parts[2], out long secondsSinceEpoch)) // 3 as database_backup_342423432.xml  could also be database_backup_342423432_1.xml
                {
                    DateTime backupDateTimeUtc = epoch.AddSeconds(secondsSinceEpoch);
                    DateTime backupDateTimeLocal = backupDateTimeUtc.ToLocalTime();
                    TimeSpan age = DateTime.UtcNow - backupDateTimeUtc;

                    backupFiles.Add(new BackupFileInfo
                    {
                        FullFilePath = filePath,
                        DateTimeOfBackup = backupDateTimeLocal,
                        SecondsSinceEpoch = secondsSinceEpoch,
                        AgeSinceBackedUp = age
                    });
                }
            }

            return backupFiles.OrderByDescending(f => f.SecondsSinceEpoch).ToList();
        }
        private static string createUniqueFilename(string directoryPath)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long secondsSinceEpoch = (long)(DateTime.UtcNow - epoch).TotalSeconds;

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
        public static void RemoveBackupDB(string file_path)
        {
            DialogResult dr = MessageBox.Show("Do you want to remove this backup?",
            "Remove Backup",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, Common.MB_TOPMOST);

            if (dr == DialogResult.Yes && File.Exists(file_path))
            {
                try
                {
                    File.Delete(file_path);
                }
                catch { }
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
                        ok = DB.ImportAndMergeDatabase2(filename, out string log, false);
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
                            DialogResult dr = MessageBox.Show("There was a problem importing the database.",
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

                string desc = InputBox.Show("Database Change Description", "Please edit the description.", db_info_json.Description);
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

        public static void MakeBackupAvailable(string file_path)
        {
            if (_dbman_settings == null) return;

            if (File.Exists(file_path))
            {
                string desc = InputBox.Show("Make Database Available", "Please enter a description for the database.", "");
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
                        //DB.WriteDB();
                        DB.Exit();
                    }

                    // read db to get some basic info
                    DB.FileName = db_file;
                    bool ok = DB.Init();
                    _ignore_written = false;

                    // get db values
                    Dictionary<string, string> options = DB.GetVarsDictionary("Options");
                    if (options.ContainsKey("comboRadioModel"))
                        model = Common.StringModelToEnum(options["comboRadioModel"]);
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
                    catch (Exception ex)
                    {
                    }

                    Dictionary<Guid, DatabaseInfo> dbs = getAvailableDBs();
                    _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID, Guid.Empty);
                }
                catch { }
            }
        }
    }
}
