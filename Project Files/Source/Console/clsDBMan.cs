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
        public class DabataseInfo
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

            [JsonConverter(typeof(StringEnumConverter))] //this will turn the enum (int) to a string
            public HPSDRModel Model { get; set; }

            public DabataseInfo()
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
            }
        }

        private static frmDBMan _frm_dbman;
        private static string _app_data_path;
        private static string _db_data_path;
        private static DBSettings _dbman_settings;
        private static bool _ignore_written;

        static DBMan()
        {
            _ignore_written = false;
            _dbman_settings = null;
            _app_data_path = "";
            _db_data_path = "";
            _frm_dbman = new frmDBMan();
        }

        public static void ShowDBMan()
        {
            Dictionary<Guid, DabataseInfo> dbs = getAvailableDBs();
            _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID);

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

        public static bool LoadDB(string[] args)
        {
            //string db_filname = "";
            foreach (string s in args)
            {
                if (s.StartsWith("-dbfilename:"))
                {
                    DialogResult dr = MessageBox.Show("-dbfilename: command line option is no longer supported.\n" +
                        "Please use -dbid: to provide the Database Manager with a unique ID to use for this instance.\n" +
                        "You can import your existing database using the Database Manager.",
                        "Database Manager",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                    break;
                }
                if (s.StartsWith("-dbid:"))
                {
                    string id = s.Trim().Substring(s.Trim().IndexOf(":") + 1);

                }
            }

            bool ok = false;

            Dictionary<Guid, DabataseInfo> dbs = getAvailableDBs();

            if (dbs.Count == 0)
            {
                // no dbs, likely due to fresh install, and/or first use of this new db manager
                ok = createNewDB(true, true);
                if (ok)
                {
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
                    _dbman_settings.ActiveDB_File = _dbman_settings.ActiveDB_File;
                    _dbman_settings.ActiveDB_GUID = _dbman_settings.ActiveDB_GUID;

                    DB.FileName = _dbman_settings.ActiveDB_File;
                    _ignore_written = true;
                    ok = DB.Init();
                    _ignore_written = false;
                }
                else
                    ok = false;
            }

            return ok;
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
                DabataseInfo di = JsonConvert.DeserializeObject<DabataseInfo>(jsonString);

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
                jsonString = JsonConvert.SerializeObject(di, Formatting.Indented);
                try
                {
                    File.WriteAllText(json_file, jsonString);
                }
                catch (Exception ex)
                {
                }
            }
        }
        private static bool createNewDB(bool check_for_original = false, bool make_active = false, string description = "")
        {
            bool ok = true;

            string db_folder = createNewDBFolder(out Guid guid);

            ok = !string.IsNullOrEmpty(db_folder);
            if (!ok) return false;

            string source_db = _app_data_path + "database.xml";
            string dest_db = db_folder + "\\database.xml";

            if (check_for_original && File.Exists(source_db))
            {
                // move old db to new system               
                File.Move(source_db, dest_db);
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
                DB.WriteDB();
                old_db_filename = DB.FileName;
            }

            // read db to get some basic info
            DB.FileName = dest_db;
            ok = DB.Init();
            _ignore_written = false;

            if (ok)
            {
                Dictionary<string, string> options = DB.GetVarsDictionary("Options");

                // add the new dbman.json
                DabataseInfo di = new DabataseInfo();
                di.GUID = guid;
                di.FullPath = db_folder;
                di.Description = string.IsNullOrEmpty(description) ? "Default" : description;

                if (options.ContainsKey("comboRadioModel"))
                    di.Model = Common.StringModelToEnum(options["comboRadioModel"]);

                di.VersionString = DB.VersionString;
                di.VersionNumber = DB.VersionNumber;
                
                string jsonString = JsonConvert.SerializeObject(di, Formatting.Indented);
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
                string dbman_settings_file = _db_data_path + "dbman_settings.json";
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
                    dbs.ActiveDB_File = db_filename;

                    //write json
                    string jsonString = JsonConvert.SerializeObject(dbs, Formatting.Indented);
                    try
                    {
                        string dbman_settings_file = _db_data_path + "dbman_settings.json";
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
        private static Dictionary<Guid, DabataseInfo> getAvailableDBs()
        {
            // find all active databases
            Dictionary<Guid, DabataseInfo> foldersInfo = new Dictionary<Guid, DabataseInfo>();
            try
            {
                string[] directories = Directory.GetDirectories(_db_data_path);

                foreach (string directory in directories)
                {
                    string folderName = Path.GetFileName(directory);
                    // check that the folder is a vaild guid
                    if (Guid.TryParse(folderName, out Guid folderGuid))
                    {
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
                        try
                        {
                            string dbman_json = directory + "\\dbman.json";
                            if (File.Exists(dbman_json))
                            {
                                string jsonString = File.ReadAllText(dbman_json);
                                DabataseInfo db_info_json = JsonConvert.DeserializeObject<DabataseInfo>(jsonString);
                                desc = db_info_json.Description;
                                model = db_info_json.Model;
                                version = db_info_json.VersionString;
                                version_number = db_info_json.VersionNumber;
                            }
                        }
                        catch { continue; }

                        //add info to the dict
                        DabataseInfo folderInfoObject = new DabataseInfo
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
                            VersionString = version
                        };
                        foldersInfo.Add(folderGuid, folderInfoObject);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"An error occurred: {ex.Message}");
            }            

            return foldersInfo;
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
            string desc = InputBox.Show("Database Description", "Please provide a description for this new database.", "");
            if (string.IsNullOrEmpty(desc)) return;

            createNewDB(false, false, desc);

            Dictionary<Guid, DabataseInfo> dbs = getAvailableDBs();
            _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID);
        }
        public static void RemoveDB(Guid guid)
        {
            string desc = InputBox.Show("Database Removal", "Please enter the matching description to remove this database.", "");
            if (string.IsNullOrEmpty(desc)) return;

            bool ok = false;
            string dbman_json = _db_data_path + guid.ToString() + "\\dbman.json";
            if (File.Exists(dbman_json))
            {
                string jsonString = File.ReadAllText(dbman_json);
                DabataseInfo db_info_json = JsonConvert.DeserializeObject<DabataseInfo>(jsonString);
                if (desc == db_info_json.Description)
                {
                    try
                    {
                        Directory.Delete(db_info_json.FullPath, true);
                        ok = true;

                        Dictionary<Guid, DabataseInfo> dbs = getAvailableDBs();
                        _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID);
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
                        DabataseInfo db_info_json = JsonConvert.DeserializeObject<DabataseInfo>(jsonString);

                        db_info_json.GUID = new_guid;
                        db_info_json.FullPath = dest_folder;
                        db_info_json.CreationTime = DateTime.Now;
                        db_info_json.FolderCreationTime = DateTime.Now;
                        db_info_json.Description = desc;
                        db_info_json.LastChanged = DateTime.Now;

                        jsonString = JsonConvert.SerializeObject(db_info_json, Formatting.Indented);
                        File.WriteAllText(dbman_json, jsonString);
                    }
                }
                catch { ok = false; }

                if (ok)
                {
                    Dictionary<Guid, DabataseInfo> dbs = getAvailableDBs();
                    _frm_dbman.InitAvailableDBs(dbs, _dbman_settings.ActiveDB_GUID);
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
    }
}
