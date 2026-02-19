/*  clsAudioRecordPlayback.cs

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
using System.Text;
using System.Threading;
using NAudio.Wave;
using NAudio.MediaFoundation;
using Newtonsoft.Json;
using System.Globalization;

namespace Thetis
{
    public enum AudioRecordRxSource
    {
        ReceiverInputIQ = 0,
        ReceiverOutputAudio = 1
    }

    public enum AudioRecordTxSource
    {
        MicAudio = 0,
        TransmitterOutputIQ = 1
    }

    public enum AudioBitDepthMode
    {
        IeeeFloat32 = 0,
        Pcm32 = 1,
        Pcm24 = 2,
        Pcm16 = 3,
        Pcm8 = 4
    }

    public sealed class RecordingDetails
    {
        public DateTime UtcTime { get; set; }
        public string Frequency { get; set; } = "";
        public string Mode { get; set; } = "";
        public string Band { get; set; } = "";

        public string WavFile { get; set; } = "";
        public long? WavFileSizeBytes { get; set; }
        public DateTime? WavFileLastWriteUtc { get; set; }
        public double? PlayDurationSeconds { get; set; }

        public int SampleRate { get; set; }
        public short BitDepth { get; set; }
        public short Channels { get; set; }
        public short FormatTag { get; set; }

        public string Mp3File { get; set; } = "";
        public long? Mp3FileSizeBytes { get; set; }
    }

    public sealed class AudioDeviceInfo
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public AudioDeviceInfo(int id, string name)
        {
            Id = id;
            Name = name ?? string.Empty;
        }

        public override string ToString()
        {
            return Name;
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Name)) return Name;
                return "Device " + Id.ToString();
            }
        }
    }

    public sealed class clsAudioRecordPlayback : IDisposable
    {
        public const bool PlaybackCosineFadeEnabled = true;
        public const int PlaybackCosineFadeMs = 50;

        private readonly object _sync = new object();

        private string _audio_folder;

        private bool _is_recording;
        private bool _is_playing;

        private WaveInEvent _pc_wave_in;
        private NAudio.Wave.WaveFileWriter _pc_wave_writer;

        private string _active_record_id;
        private string _active_record_filename;
        private string _active_record_json_filename;
        private string _active_record_mp3_filename;
        private RecordingDetails _active_record_details;
        private int _active_record_sample_rate;

        private string _active_play_id;
        private string _active_play_filename;

        private WaveOutEvent _pc_wave_out;
        private AudioFileReader _pc_audio_reader;

        private int _active_record_wfw_id;
        private int _active_playback_wfw_id;

        private bool _disposed;

        public event Action<bool, string, string> RecordingChanged;
        public event Action<bool, string, string, bool> PlayingChanged;
        public event Action<string, RecordingJsonModel> RecordingJsonWritten;

        private bool _is_wdsp_playing;

        private float _pc_input_gain;
        private float _pc_playback_gain;
        private byte[] _pc_record_gain_buffer;

        public AudioRecordRxSource RxSource { get; set; }
        public AudioRecordTxSource TxSource { get; set; }
        public int SampleRate { get; set; }
        public AudioBitDepthMode BitDepthMode { get; set; }
        public bool DitherEnabled { get; set; }
        public float DitherAmount { get; set; }

        public bool MoxOnPlayback { get; set; } = true;

        public bool GenerateMP3File { get; set; } = false;
        public bool GenerateJSON { get; set; } = false;

        public string StorageFolder { get; set; } = null;
        public int InputPCDeviceID { get; set; } = -1;
        public int OutputPCDeviceID { get; set; } = -1;

        private Console _console;
        private Dictionary<string, bool> _playbackSetting;
        private Dictionary<string, bool> _prePlaybackSetting;

        private readonly SynchronizationContext _sync_context;

        private static readonly object _mf_sync = new object();
        private static bool _mf_started;

        public clsAudioRecordPlayback(Console c)
        {
            _sync_context = SynchronizationContext.Current;

            _console = c;
            _console.MoxPreChangeHandlers += OnPreMox;

            _playbackSetting = new Dictionary<string, bool>();
            _prePlaybackSetting = new Dictionary<string, bool>();

            _is_wdsp_playing = false;
            _active_record_wfw_id = -1;
            _active_playback_wfw_id = -1;

            _pc_input_gain = 1.0f;
            _pc_playback_gain = 1.0f;

            _audio_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Thetis");

            RxSource = AudioRecordRxSource.ReceiverOutputAudio;
            TxSource = AudioRecordTxSource.MicAudio;
            SampleRate = 48000;
            BitDepthMode = AudioBitDepthMode.IeeeFloat32;
            DitherEnabled = false;
            DitherAmount = 0.8f;

            ensureFolderExists(_audio_folder);
        }

        private void initPlaybackSettings()
        {
            if (_playbackSetting.Count < 1)
            {
                _playbackSetting["TXEQ"] = _console.TXEQ;
                _playbackSetting["COMP"] = _console.CPDR;
                _playbackSetting["CFC"] = _console.CFCEnabled;
                _playbackSetting["PHASE"] = _console.PhaseRotEnabled;
                _playbackSetting["MON"] = _console.MON;
                _playbackSetting["BYPASS_VAC"] = _console.BypassVACWhenPlayingWAV;
                _playbackSetting["MOX"] = _console.MOX;
            }
        }

        private static double? tryGetWavDurationSeconds(string wavPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(wavPath)) return null;
                if (!File.Exists(wavPath)) return null;

                using (BinaryReader br = new BinaryReader(File.Open(wavPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    int fmtTag;
                    int sr;
                    int ch;
                    int bps;
                    long ds;
                    long dl;
                    string hdrErr;

                    if (!tryParseWaveHeader(br, out fmtTag, out sr, out ch, out bps, out ds, out dl, out hdrErr)) return null;
                    if (sr <= 0) return null;

                    int bytesPerSample;
                    if (fmtTag == 3) bytesPerSample = 4;
                    else
                    {
                        bytesPerSample = bps / 8;
                        if (bytesPerSample < 1) bytesPerSample = 1;
                    }

                    int bytesPerFrame = ch * bytesPerSample;
                    if (bytesPerFrame < 1) bytesPerFrame = 1;

                    if (dl <= 0) return 0.0;

                    double frames = dl / (double)bytesPerFrame;
                    double seconds = frames / (double)sr;

                    if (seconds < 0.0) seconds = 0.0;
                    return Math.Round(seconds, 3);
                }
            }
            catch
            {
                return null;
            }
        }

        private static bool tryParseUtcStamp(string s, out DateTime utc)
        {
            utc = default(DateTime);
            if (string.IsNullOrWhiteSpace(s)) return false;

            return DateTime.TryParseExact(
                s.Trim(),
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out utc);
        }

        private void refreshExistingJsonFromWavIfNeeded(string unique_id, string wavPath, int formatTag, int sampleRate, int channels, int bitsPerSample)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(wavPath)) return;
                if (!File.Exists(wavPath)) return;

                string jsonPath = Path.ChangeExtension(wavPath, ".json");
                if (!File.Exists(jsonPath)) return;

                DateTime wavLast = DateTime.SpecifyKind(File.GetLastWriteTimeUtc(wavPath), DateTimeKind.Utc);
                string wavStamp = wavLast.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                RecordingJsonModel existing = null;
                try
                {
                    string existingText = File.ReadAllText(jsonPath, Encoding.UTF8);
                    existing = JsonConvert.DeserializeObject<RecordingJsonModel>(existingText);
                }
                catch
                {
                    existing = null;
                }

                string existingStamp = existing != null ? existing.wav_file_last_write_utc : null;
                if (string.Equals((existingStamp ?? string.Empty).Trim(), wavStamp, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                RecordingDetails d = new RecordingDetails();

                DateTime utcTime = DateTime.UtcNow;
                if (existing != null)
                {
                    DateTime parsedUtc;
                    if (tryParseUtcStamp(existing.utc_time, out parsedUtc)) utcTime = parsedUtc;

                    d.Frequency = existing.frequency ?? "";
                    d.Mode = existing.mode ?? "";
                    d.Band = existing.band ?? "";

                    d.Mp3File = existing.mp3_file ?? "";
                    d.Mp3FileSizeBytes = existing.mp3_file_size_bytes;
                }

                d.UtcTime = utcTime;

                d.WavFile = Path.GetFileName(wavPath) ?? "";
                try
                {
                    FileInfo fi = new FileInfo(wavPath);
                    d.WavFileSizeBytes = fi.Length;
                    d.WavFileLastWriteUtc = DateTime.SpecifyKind(fi.LastWriteTimeUtc, DateTimeKind.Utc);
                    d.PlayDurationSeconds = tryGetWavDurationSeconds(wavPath);
                }
                catch
                {
                    d.WavFileSizeBytes = null;
                    d.WavFileLastWriteUtc = null;
                    d.PlayDurationSeconds = null;
                }

                d.SampleRate = sampleRate;
                d.BitDepth = (short)bitsPerSample;
                d.Channels = (short)channels;
                d.FormatTag = (short)formatTag;

                if (!string.IsNullOrWhiteSpace(d.Mp3File))
                {
                    try
                    {
                        string dir = Path.GetDirectoryName(wavPath);
                        string mp3Full = Path.Combine(dir ?? "", d.Mp3File);

                        if (File.Exists(mp3Full))
                        {
                            FileInfo fi2 = new FileInfo(mp3Full);
                            d.Mp3FileSizeBytes = fi2.Length;
                        }
                        else
                        {
                            d.Mp3FileSizeBytes = null;
                        }
                    }
                    catch
                    {
                        d.Mp3FileSizeBytes = null;
                    }
                }
                else
                {
                    d.Mp3FileSizeBytes = null;
                }

                writeRecordingJson(unique_id, jsonPath, d);
            }
            catch { }
        }

        public void SetPlaybackSetting(string setting, bool value)
        {
            initPlaybackSettings();
            setting = (setting ?? string.Empty).ToUpper();
            _playbackSetting[setting] = value;
        }

        public bool GetPlaybackSetting(string setting)
        {
            initPlaybackSettings();
            setting = (setting ?? string.Empty).ToUpper();
            if (!_playbackSetting.ContainsKey(setting)) return false;
            return _playbackSetting[setting];
        }

        private void storeRestoreSettings(bool store, bool playback)
        {
            if (store)
            {
                if (playback)
                {
                    _prePlaybackSetting["TXEQ"] = _console.TXEQ;
                    _prePlaybackSetting["COMP"] = _console.CPDR;
                    _prePlaybackSetting["CFC"] = _console.CFCEnabled;
                    _prePlaybackSetting["PHASE"] = _console.PhaseRotEnabled;
                    _prePlaybackSetting["MON"] = _console.MON;
                    _prePlaybackSetting["BYPASS_VAC"] = Audio.VACBypass;
                    _prePlaybackSetting["MOX"] = _console.MOX;
                }
                else
                {
                    _prePlaybackSetting["RXEQ"] = _console.RXEQ;
                }
            }
            else
            {
                if (playback)
                {
                    if (_prePlaybackSetting.ContainsKey("TXEQ") && _console.TXEQ != _prePlaybackSetting["TXEQ"]) _console.TXEQ = _prePlaybackSetting["TXEQ"];
                    if (_prePlaybackSetting.ContainsKey("COMP") && _console.CPDR != _prePlaybackSetting["COMP"]) _console.CPDR = _prePlaybackSetting["COMP"];
                    if (_prePlaybackSetting.ContainsKey("CFC") && _console.CFCEnabled != _prePlaybackSetting["CFC"]) _console.CFCEnabled = _prePlaybackSetting["CFC"];
                    if (_prePlaybackSetting.ContainsKey("PHASE") && _console.PhaseRotEnabled != _prePlaybackSetting["PHASE"]) _console.PhaseRotEnabled = _prePlaybackSetting["PHASE"];
                    if (_prePlaybackSetting.ContainsKey("MON") && _console.MON != _prePlaybackSetting["MON"]) _console.MON = _prePlaybackSetting["MON"];
                    if (_prePlaybackSetting.ContainsKey("BYPASS_VAC") && Audio.VACBypass != _prePlaybackSetting["BYPASS_VAC"]) Audio.VACBypass = _prePlaybackSetting["BYPASS_VAC"];
                    if (_prePlaybackSetting.ContainsKey("MOX") && _console.MOX != _prePlaybackSetting["MOX"]) _console.MOX = _prePlaybackSetting["MOX"];
                }
                else
                {
                    if (_prePlaybackSetting.ContainsKey("RXEQ") && _console.RXEQ != _prePlaybackSetting["RXEQ"]) _console.RXEQ = _prePlaybackSetting["RXEQ"];
                }
            }
        }

        private void activatePlaybackRecordSettings(bool playback)
        {
            if (playback)
            {
                if (GetPlaybackSetting("TXEQ") && _console.TXEQ) _console.TXEQ = false;
                if (GetPlaybackSetting("COMP") && _console.CPDR) _console.CPDR = false;
                if (GetPlaybackSetting("CFC") && _console.CFCEnabled) _console.CFCEnabled = false;
                if (GetPlaybackSetting("PHASE") && _console.PhaseRotEnabled) _console.PhaseRotEnabled = false;
                if (GetPlaybackSetting("MON") && !_console.MON) _console.MON = true;
                Audio.VACBypass = _console.BypassVACWhenPlayingWAV;
                if (!_console.MOX && MoxOnPlayback) _console.MOX = true;
            }
            else
            {
                if (GetPlaybackSetting("RXEQ") && _console.RXEQ) _console.RXEQ = false;
            }
        }

        private void OnPreMox(int rx, bool oldMox, bool newMox)
        {
            if (oldMox != newMox)
            {
                try { StopRecord(out string _); } catch { }
                try { StopPlayback(out string _); } catch { }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { StopRecord(out string _); } catch { }
            try { StopPlayback(out string _); } catch { }

            lock (_sync)
            {
                if (_pc_wave_in != null)
                {
                    try { _pc_wave_in.DataAvailable -= onPcDataAvailable; } catch { }
                    try { _pc_wave_in.RecordingStopped -= onPcRecordingStopped; } catch { }
                    try { _pc_wave_in.Dispose(); } catch { }
                    _pc_wave_in = null;
                }

                if (_pc_wave_writer != null)
                {
                    try { _pc_wave_writer.Dispose(); } catch { }
                    _pc_wave_writer = null;
                }
            }

            _console.MoxPreChangeHandlers -= OnPreMox;
        }

        public string AudioFolder
        {
            get { return _audio_folder; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _audio_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Thetis");
                }
                else
                {
                    _audio_folder = value.Trim();
                }

                ensureFolderExists(_audio_folder);
            }
        }

        public List<AudioDeviceInfo> GetPcInputDevices()
        {
            List<AudioDeviceInfo> list = new List<AudioDeviceInfo>();
            int count = WaveIn.DeviceCount;
            for (int i = 0; i < count; i++)
            {
                WaveInCapabilities caps = WaveIn.GetCapabilities(i);
                list.Add(new AudioDeviceInfo(i, caps.ProductName));
            }
            return list;
        }

        public List<AudioDeviceInfo> GetPcOutputDevices()
        {
            List<AudioDeviceInfo> list = new List<AudioDeviceInfo>();
            int count = WaveOut.DeviceCount;
            for (int i = 0; i < count; i++)
            {
                WaveOutCapabilities caps = WaveOut.GetCapabilities(i);
                list.Add(new AudioDeviceInfo(i, caps.ProductName));
            }
            return list;
        }

        public bool DeleteRecording(string full_path, out string error, bool delete_containing_folder_if_empty = false)
        {
            error = null;

            try
            {
                string resolved = resolvePlayPath(full_path, out error);
                if (resolved == null) return false;

                HashSet<string> targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                bool resolvedIsFolder = Directory.Exists(resolved);
                string folderToDelete = null;

                if (resolvedIsFolder)
                {
                    folderToDelete = resolved;

                    string[] wavs = Directory.GetFiles(resolved, "*.wav", SearchOption.TopDirectoryOnly);
                    for (int i = 0; i < wavs.Length; i++)
                    {
                        string wav = wavs[i];
                        targets.Add(wav);
                        targets.Add(Path.ChangeExtension(wav, ".json"));
                        targets.Add(Path.ChangeExtension(wav, ".mp3"));
                    }
                }
                else
                {
                    string basePath = resolved;

                    if (hasRealExtension(resolved))
                    {
                        string ext = (Path.GetExtension(resolved) ?? string.Empty).ToLowerInvariant();
                        if (ext != ".wav" && ext != ".json" && ext != ".mp3")
                        {
                            error = "Unsupported file type.";
                            return false;
                        }

                        string dir = Path.GetDirectoryName(resolved);
                        string name = Path.GetFileNameWithoutExtension(resolved);

                        if (string.IsNullOrWhiteSpace(dir) || string.IsNullOrWhiteSpace(name))
                        {
                            return true;
                        }

                        basePath = Path.Combine(dir, name);
                    }

                    string wav = Path.ChangeExtension(basePath, ".wav");
                    string json = Path.ChangeExtension(basePath, ".json");
                    string mp3 = Path.ChangeExtension(basePath, ".mp3");

                    targets.Add(wav);
                    targets.Add(json);
                    targets.Add(mp3);

                    bool anyExists = false;
                    if (File.Exists(wav)) anyExists = true;
                    else if (File.Exists(json)) anyExists = true;
                    else if (File.Exists(mp3)) anyExists = true;

                    if (!anyExists) return true;

                    folderToDelete = Path.GetDirectoryName(wav);
                }

                string activeRec = null;
                string activePlay = null;
                bool isRec = false;
                bool isPlay = false;

                lock (_sync)
                {
                    activeRec = _active_record_filename;
                    activePlay = _active_play_filename;
                    isRec = _is_recording;
                    isPlay = _is_playing;
                }

                if (isRec && !string.IsNullOrWhiteSpace(activeRec) && targets.Contains(activeRec))
                {
                    error = "Cannot delete an active recording.";
                    return false;
                }

                if (isPlay && !string.IsNullOrWhiteSpace(activePlay) && targets.Contains(activePlay))
                {
                    error = "Cannot delete an active playback file.";
                    return false;
                }

                foreach (string path in targets)
                {
                    if (string.IsNullOrWhiteSpace(path)) continue;
                    if (!File.Exists(path)) continue;
                    try { File.Delete(path); } catch { }
                }

                if (delete_containing_folder_if_empty && !string.IsNullOrWhiteSpace(folderToDelete) && Directory.Exists(folderToDelete))
                {
                    bool canDelete = false;
                    bool sameAsBase = false;

                    try { canDelete = isUnderBaseFolder(_audio_folder, folderToDelete); } catch { canDelete = false; }

                    try
                    {
                        string baseFull = Path.GetFullPath(_audio_folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        string folderFull = Path.GetFullPath(folderToDelete).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        sameAsBase = string.Equals(baseFull, folderFull, StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        sameAsBase = false;
                    }

                    if (canDelete && !sameAsBase)
                    {
                        try { Directory.Delete(folderToDelete, false); } catch { }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool hasTrailingSeparator(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            char last = path[path.Length - 1];
            return last == Path.DirectorySeparatorChar || last == Path.AltDirectorySeparatorChar;
        }

        private static bool containsDirectorySeparator(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            return path.IndexOf(Path.DirectorySeparatorChar) >= 0 || path.IndexOf(Path.AltDirectorySeparatorChar) >= 0;
        }

        private static bool hasRealExtension(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            string ext = Path.GetExtension(path.Trim());
            if (string.IsNullOrEmpty(ext)) return false;
            if (ext == ".") return false;
            return true;
        }

        private static string ensureTrailingSeparator(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            if (hasTrailingSeparator(path)) return path;
            return path + Path.DirectorySeparatorChar;
        }

        private static bool isUnderBaseFolder(string baseFolder, string candidatePath)
        {
            if (string.IsNullOrWhiteSpace(baseFolder) || string.IsNullOrWhiteSpace(candidatePath)) return false;
            string baseFull = Path.GetFullPath(baseFolder);
            string candFull = Path.GetFullPath(candidatePath);
            baseFull = ensureTrailingSeparator(baseFull);
            return candFull.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase);
        }

        private string makeUniquePathInFolder(string folder, string prefix, out string filename)
        {
            ensureFolderExists(folder);

            int suffix_count = 0;
            while (true)
            {
                filename = generateFilename(prefix, suffix_count, "wav");
                string candidate = Path.Combine(folder, filename);
                if (!File.Exists(candidate)) return candidate;

                suffix_count++;
            }
        }

        private string resolveRecordPath(string record_id, string input_path, string prefix, out string filename, out string error)
        {
            filename = null;
            error = null;

            string p = null;
            if (!string.IsNullOrWhiteSpace(input_path)) p = input_path.Trim();

            if (string.IsNullOrEmpty(p))
            {
                return makeUniquePathInFolder(_audio_folder, prefix, out filename);
            }

            bool rooted = Path.IsPathRooted(p);
            bool hasSep = containsDirectorySeparator(p);
            bool endsSep = hasTrailingSeparator(p);
            bool endsLikeFile = !endsSep && hasRealExtension(p);

            if (rooted)
            {
                if (endsLikeFile)
                {
                    string dirAbs = Path.GetDirectoryName(p);
                    if (string.IsNullOrWhiteSpace(dirAbs))
                    {
                        error = "Invalid file path.";
                        return null;
                    }

                    ensureFolderExists(dirAbs);
                    filename = Path.GetFileName(p);
                    return p;
                }

                ensureFolderExists(p);
                return makeUniquePathInFolder(p, prefix, out filename);
            }

            if (!hasSep)
            {
                if (endsLikeFile)
                {
                    ensureFolderExists(_audio_folder);
                    filename = p;
                    return Path.Combine(_audio_folder, p);
                }

                string folder1 = Path.Combine(_audio_folder, p);
                if (!isUnderBaseFolder(_audio_folder, folder1))
                {
                    error = "Invalid path.";
                    return null;
                }
                return makeUniquePathInFolder(folder1, prefix, out filename);
            }

            if (endsLikeFile)
            {
                string full = Path.Combine(_audio_folder, p);
                if (!isUnderBaseFolder(_audio_folder, full))
                {
                    error = "Invalid path.";
                    return null;
                }

                string dir = Path.GetDirectoryName(full);
                if (string.IsNullOrWhiteSpace(dir))
                {
                    error = "Invalid file path.";
                    return null;
                }

                ensureFolderExists(dir);
                filename = Path.GetFileName(full);
                return full;
            }
            else
            {
                string folder2 = Path.Combine(_audio_folder, p);
                if (!isUnderBaseFolder(_audio_folder, folder2))
                {
                    error = "Invalid path.";
                    return null;
                }

                ensureFolderExists(folder2);
                return makeUniquePathInFolder(folder2, prefix, out filename);
            }
        }

        private string resolvePlayPath(string input_path, out string error)
        {
            error = null;

            string p = null;
            if (!string.IsNullOrWhiteSpace(input_path)) p = input_path.Trim();

            if (string.IsNullOrEmpty(p))
            {
                error = "Path is empty.";
                return null;
            }

            if (Path.IsPathRooted(p))
            {
                return p;
            }

            string combined = Path.Combine(_audio_folder, p);
            if (!isUnderBaseFolder(_audio_folder, combined))
            {
                error = "Invalid path.";
                return null;
            }

            return combined;
        }

        private static DateTime ensureUtc(DateTime dt)
        {
            if (dt == default(DateTime)) return DateTime.UtcNow;
            if (dt.Kind == DateTimeKind.Utc) return dt;
            if (dt.Kind == DateTimeKind.Local) return dt.ToUniversalTime();
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }

        private static RecordingDetails ensureDetails(RecordingDetails details, bool needed)
        {
            if (!needed) return details;
            if (details == null)
            {
                RecordingDetails d = new RecordingDetails();
                d.UtcTime = DateTime.UtcNow;
                return d;
            }
            if (details.UtcTime == default(DateTime)) details.UtcTime = DateTime.UtcNow;
            return details;
        }

        public bool GetJSONDetailsFromFile(string full_path_file, out RecordingJsonModel json_data)
        {
            json_data = null;

            try
            {
                if (string.IsNullOrWhiteSpace(full_path_file)) return false;

                string wav_path = full_path_file;
                string ext = (Path.GetExtension(wav_path) ?? string.Empty).ToLowerInvariant();
                if (ext != ".wav") wav_path = Path.ChangeExtension(wav_path, ".wav");

                if (!File.Exists(wav_path))
                {
                    json_data = null;
                    return false;
                }

                string json_path = Path.ChangeExtension(wav_path, ".json");
                if (string.IsNullOrWhiteSpace(json_path) || !File.Exists(json_path))
                {
                    json_data = null;
                    return true;
                }

                string text = File.ReadAllText(json_path, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(text))
                {
                    json_data = null;
                    return true;
                }

                RecordingJsonModel m = null;
                try
                {
                    m = JsonConvert.DeserializeObject<RecordingJsonModel>(text);
                }
                catch
                {
                    m = null;
                }

                json_data = m;
                return true;
            }
            catch
            {
                json_data = null;
                return false;
            }
        }

        public string RecordToFileFromWDSP(string record_id, string full_path, int wfw_id, out string error, bool remove_if_file_exists = false, RecordingDetails details = null)
        {
            error = null;

            lock (_sync)
            {
                if (wfw_id < 0 || wfw_id >= WaveThing.wave_file_writer.Length)
                {
                    error = "Invalid WDSP ID.";
                    return null;
                }

                if (_is_recording)
                {
                    error = "Already recording.";
                    return null;
                }

                if (SampleRate < 6000)
                {
                    error = "SampleRate is invalid.";
                    return null;
                }

                try
                {
                    string filename;
                    string full_target = resolveRecordPath(record_id, full_path, "wdsp", out filename, out error);
                    if (full_target == null) return null;

                    if (remove_if_file_exists && File.Exists(full_target))
                    {
                        try { File.Delete(full_target); } catch { }
                    }

                    if (!Common.CanCreateFile(full_target))
                    {
                        error = $"Unable to create file\n{full_target}\n\nThis may be due to controlled folder access or some other reason.";
                        return null;
                    }

                    short bitDepth = 32;
                    short formatTag = 3;
                    switch (BitDepthMode)
                    {
                        case AudioBitDepthMode.IeeeFloat32:
                            bitDepth = 32;
                            formatTag = 3;
                            break;
                        case AudioBitDepthMode.Pcm32:
                            bitDepth = 32;
                            formatTag = 1;
                            break;
                        case AudioBitDepthMode.Pcm24:
                            bitDepth = 24;
                            formatTag = 1;
                            break;
                        case AudioBitDepthMode.Pcm16:
                            bitDepth = 16;
                            formatTag = 1;
                            break;
                        case AudioBitDepthMode.Pcm8:
                            bitDepth = 8;
                            formatTag = 1;
                            break;
                    }

                    bool needDetails = GenerateJSON || GenerateMP3File;
                    RecordingDetails d = ensureDetails(details, needDetails);

                    if (d != null)
                    {
                        d.UtcTime = ensureUtc(d.UtcTime);
                        d.SampleRate = SampleRate;
                        d.BitDepth = bitDepth;
                        d.FormatTag = formatTag;
                        d.Channels = 2;
                        d.WavFile = Path.GetFileName(full_target) ?? "";
                        d.WavFileSizeBytes = null;
                        d.WavFileLastWriteUtc = null;
                        d.PlayDurationSeconds = null;
                        d.Mp3File = "";
                        d.Mp3FileSizeBytes = null;
                    }

                    _active_record_wfw_id = wfw_id;
                    _active_record_id = record_id ?? string.Empty;
                    _active_record_filename = full_target;
                    _active_record_details = d;
                    _active_record_sample_rate = SampleRate;

                    _active_record_json_filename = null;
                    _active_record_mp3_filename = null;

                    if (GenerateJSON && _active_record_details != null)
                    {
                        _active_record_json_filename = Path.ChangeExtension(full_target, ".json");
                    }

                    if (GenerateMP3File && _active_record_details != null)
                    {
                        _active_record_mp3_filename = Path.ChangeExtension(full_target, ".mp3");
                    }

                    bool recRXPreProc = RxSource == AudioRecordRxSource.ReceiverInputIQ;
                    bool recTXPreProc = TxSource == AudioRecordTxSource.MicAudio;

                    storeRestoreSettings(true, false);
                    activatePlaybackRecordSettings(false);

                    Thread.Sleep(50);

                    WaveThing.wave_file_writer[_active_record_wfw_id] = new WaveFileWriter(
                        _active_record_wfw_id,
                        2,
                        SampleRate,
                        full_target,
                        recRXPreProc,
                        recTXPreProc,
                        formatTag,
                        bitDepth,
                        onWdspRecordFinished);

                    WaveThing.wave_file_writer[_active_record_wfw_id].DitherEnabled = DitherEnabled;
                    WaveThing.wave_file_writer[_active_record_wfw_id].DitherAmount = DitherAmount;

                    Audio.WaveRecord = true;
                    setRecordingState(true);

                    return full_target;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    try { Audio.WaveRecord = false; } catch { }

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(_active_record_filename) && File.Exists(_active_record_filename))
                        {
                            try { File.Delete(_active_record_filename); } catch { }
                        }
                    }
                    catch { }

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(_active_record_json_filename) && File.Exists(_active_record_json_filename))
                        {
                            try { File.Delete(_active_record_json_filename); } catch { }
                        }
                    }
                    catch { }

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(_active_record_mp3_filename) && File.Exists(_active_record_mp3_filename))
                        {
                            try { File.Delete(_active_record_mp3_filename); } catch { }
                        }
                    }
                    catch { }

                    setRecordingState(false);
                    clearActiveRecordLocked();
                    return null;
                }
            }
        }

        public string RecordToFileFromPCAudio(string record_id, string full_path, int pcAudioDeviceInputId, out string error, bool remove_if_file_exists = false, RecordingDetails details = null)
        {
            error = null;

            lock (_sync)
            {
                if (_is_recording)
                {
                    error = "Already recording.";
                    return null;
                }

                if (SampleRate < 6000)
                {
                    error = "SampleRate is invalid.";
                    return null;
                }

                try
                {
                    string filename;
                    string full_target = resolveRecordPath(record_id, full_path, "pc", out filename, out error);
                    if (full_target == null) return null;

                    if (remove_if_file_exists && File.Exists(full_target))
                    {
                        try { File.Delete(full_target); } catch { }
                    }

                    if (!Common.CanCreateFile(full_target))
                    {
                        error = "Unable to create file.";
                        return null;
                    }

                    int channels = 2;
                    int bits = 16;
                    WaveFormat fmt;

                    switch (BitDepthMode)
                    {
                        case AudioBitDepthMode.IeeeFloat32:
                            bits = 32;
                            fmt = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, channels);
                            break;
                        case AudioBitDepthMode.Pcm32:
                            bits = 32;
                            fmt = new WaveFormat(SampleRate, bits, channels);
                            break;
                        case AudioBitDepthMode.Pcm24:
                            bits = 24;
                            fmt = new WaveFormat(SampleRate, bits, channels);
                            break;
                        case AudioBitDepthMode.Pcm16:
                            bits = 16;
                            fmt = new WaveFormat(SampleRate, bits, channels);
                            break;
                        case AudioBitDepthMode.Pcm8:
                            bits = 8;
                            fmt = new WaveFormat(SampleRate, bits, channels);
                            break;
                        default:
                            bits = 16;
                            fmt = new WaveFormat(SampleRate, bits, channels);
                            break;
                    }

                    bool needDetails = GenerateJSON || GenerateMP3File;
                    RecordingDetails d = ensureDetails(details, needDetails);

                    if (d != null)
                    {
                        d.UtcTime = ensureUtc(d.UtcTime);
                        d.SampleRate = SampleRate;
                        d.BitDepth = (short)bits;
                        d.FormatTag = (short)(BitDepthMode == AudioBitDepthMode.IeeeFloat32 ? 3 : 1);
                        d.Channels = (short)channels;
                        d.WavFile = Path.GetFileName(full_target) ?? "";
                        d.WavFileSizeBytes = null;
                        d.WavFileLastWriteUtc = null;
                        d.PlayDurationSeconds = null;
                        d.Mp3File = "";
                        d.Mp3FileSizeBytes = null;
                    }

                    _active_record_wfw_id = -1;
                    _active_record_id = record_id ?? string.Empty;
                    _active_record_filename = full_target;
                    _active_record_details = d;
                    _active_record_sample_rate = SampleRate;

                    _active_record_json_filename = null;
                    _active_record_mp3_filename = null;

                    if (GenerateJSON && _active_record_details != null)
                    {
                        _active_record_json_filename = Path.ChangeExtension(full_target, ".json");
                    }

                    if (GenerateMP3File && _active_record_details != null)
                    {
                        _active_record_mp3_filename = Path.ChangeExtension(full_target, ".mp3");
                    }

                    _pc_wave_in = new WaveInEvent();
                    _pc_wave_in.DeviceNumber = pcAudioDeviceInputId;
                    _pc_wave_in.WaveFormat = fmt;
                    _pc_wave_in.BufferMilliseconds = 50;
                    _pc_wave_in.NumberOfBuffers = 3;
                    _pc_wave_in.DataAvailable += onPcDataAvailable;
                    _pc_wave_in.RecordingStopped += onPcRecordingStopped;

                    _pc_wave_writer = new NAudio.Wave.WaveFileWriter(full_target, fmt);

                    Thread.Sleep(50);

                    _pc_wave_in.StartRecording();
                    setRecordingState(true);

                    return full_target;
                }
                catch (Exception ex)
                {
                    error = ex.Message;

                    try { cleanupPcRecording(); } catch { }

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(_active_record_filename) && File.Exists(_active_record_filename))
                        {
                            try { File.Delete(_active_record_filename); } catch { }
                        }
                    }
                    catch { }

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(_active_record_json_filename) && File.Exists(_active_record_json_filename))
                        {
                            try { File.Delete(_active_record_json_filename); } catch { }
                        }
                    }
                    catch { }

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(_active_record_mp3_filename) && File.Exists(_active_record_mp3_filename))
                        {
                            try { File.Delete(_active_record_mp3_filename); } catch { }
                        }
                    }
                    catch { }

                    setRecordingState(false);
                    clearActiveRecordLocked();
                    return null;
                }
            }
        }

        public double WDSPTXAudioGainInDb
        {
            get
            {
                if (Audio.WavePreamp <= 0.0) return -70.0;
                double db = 20.0 * Math.Log10(Audio.WavePreamp);
                if (db < -70.0) return -70.0;
                if (db > 70.0) return 70.0;
                return db;
            }
            set
            {
                double clamped = value;
                if (clamped < -70.0) clamped = -70.0;
                if (clamped > 70.0) clamped = 70.0;

                Audio.WavePreamp = Math.Pow(10.0, clamped / 20.0);
            }
        }

        public float PCInputGain
        {
            get { return _pc_input_gain; }
            set
            {
                float v = value;
                if (float.IsNaN(v) || float.IsInfinity(v)) v = 1.0f;
                if (v < 0.0f) v = 0.0f;
                if (v > 8.0f) v = 8.0f;
                lock (_sync)
                {
                    _pc_input_gain = v;
                }
            }
        }

        public float PCOutputGain
        {
            get { return _pc_playback_gain; }
            set
            {
                float v = value;
                if (float.IsNaN(v) || float.IsInfinity(v)) v = 1.0f;
                if (v < 0.0f) v = 0.0f;
                if (v > 8.0f) v = 8.0f;

                lock (_sync)
                {
                    _pc_playback_gain = v;
                    if (_pc_audio_reader != null) _pc_audio_reader.Volume = _pc_playback_gain;
                }
            }
        }

        public bool StopRecord(out string error)
        {
            error = null;

            lock (_sync)
            {
                if (!_is_recording) return true;

                try
                {
                    Audio.WaveRecord = false;

                    if (_active_record_wfw_id >= 0)
                    {
                        WaveFileWriter writer = WaveThing.wave_file_writer[_active_record_wfw_id];
                        if (writer != null)
                        {
                            writer.Stop();
                        }
                    }

                    if (_pc_wave_in != null)
                    {
                        try { _pc_wave_in.StopRecording(); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(_active_record_json_filename) && File.Exists(_active_record_json_filename))
                        {
                            try { File.Delete(_active_record_json_filename); } catch { }
                        }
                    }
                    catch { }

                    setRecordingState(false);
                    clearActiveRecordLocked();
                }
            }

            storeRestoreSettings(false, false);
            return error == null;
        }

        public bool PlayFileViaWDSP(string play_id, string full_path, int wfw_id, out string error)
        {
            bool ret = false;
            bool restore = false;
            error = null;

            lock (_sync)
            {
                if (wfw_id < 0 || wfw_id >= WaveThing.wave_file_reader.Length)
                {
                    error = "Invalid WDSP ID.";
                    return false;
                }

                if (_is_playing)
                {
                    error = "Already playing.";
                    return false;
                }

                try
                {
                    string fullPath = resolvePlayPath(full_path, out error);
                    if (fullPath == null) return false;

                    if (!File.Exists(fullPath))
                    {
                        error = "File does not exist.";
                        return false;
                    }

                    BinaryReader reader = null;
                    try
                    {
                        reader = new BinaryReader(File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read));
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                        return false;
                    }

                    if (!tryParseWaveHeader(reader, out int formatTag, out int sampleRate, out int channels, out int bitsPerSample, out long dataStart, out long dataLengthBytes, out string headerError))
                    {
                        try { reader.Close(); } catch { }
                        error = headerError;
                        return false;
                    }

                    try
                    {
                        refreshExistingJsonFromWavIfNeeded(play_id, fullPath, formatTag, sampleRate, channels, bitsPerSample);
                    }
                    catch { }

                    storeRestoreSettings(true, true);

                    double mic = Audio.console.radio.GetDSPTX(0).MicGain;
                    Audio.console.radio.GetDSPTX(0).MicGain = 0.0;

                    activatePlaybackRecordSettings(true);

                    Thread.Sleep(50);

                    _active_playback_wfw_id = wfw_id;
                    _active_play_id = play_id ?? string.Empty;
                    _active_play_filename = fullPath;
                    _is_wdsp_playing = true;

                    WaveThing.wave_file_reader[_active_playback_wfw_id] = new WaveFileReader1(
                        _active_playback_wfw_id,
                        formatTag,
                        sampleRate,
                        channels,
                        bitsPerSample,
                        dataLengthBytes,
                        PlaybackCosineFadeEnabled,
                        PlaybackCosineFadeMs,
                        reader,
                        onWdspPlaybackFinished);

                    _console.SetWavePlayback(wfw_id, true);
                    Audio.WavePlayback = true;
                    setPlayingState(true);

                    Audio.console.radio.GetDSPTX(0).MicGain = mic;

                    ret = true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    try
                    {
                        _console.SetWavePlayback(wfw_id, false);
                        Audio.WavePlayback = false;
                    }
                    catch { }
                    setPlayingState(false);
                    restore = true;
                }
            }

            if (restore) storeRestoreSettings(false, true);
            return ret;
        }

        public bool PlayFileViaPCAudio(string play_id, string full_path, int pcAudioDeviceOutputId, out string error)
        {
            bool ret = false;
            error = null;

            lock (_sync)
            {
                if (_is_playing)
                {
                    error = "Already playing.";
                    return false;
                }

                try
                {
                    string fullPath = resolvePlayPath(full_path, out error);
                    if (fullPath == null) return false;

                    if (!File.Exists(fullPath))
                    {
                        error = "File does not exist.";
                        return false;
                    }

                    if (string.Equals((Path.GetExtension(fullPath) ?? string.Empty), ".wav", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            using (BinaryReader br = new BinaryReader(File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                            {
                                if (tryParseWaveHeader(br, out int fmtTag, out int sr, out int ch, out int bps, out long ds, out long dl, out string hdrErr))
                                {
                                    refreshExistingJsonFromWavIfNeeded(play_id, fullPath, fmtTag, sr, ch, bps);
                                }
                            }
                        }
                        catch { }
                    }

                    _active_play_id = play_id ?? string.Empty;
                    _active_play_filename = fullPath;
                    _is_wdsp_playing = false;

                    _pc_audio_reader = new AudioFileReader(fullPath);
                    _pc_audio_reader.Volume = _pc_playback_gain;
                    _pc_wave_out = new WaveOutEvent();
                    _pc_wave_out.DeviceNumber = pcAudioDeviceOutputId;
                    _pc_wave_out.PlaybackStopped += pcWaveOut_PlaybackStopped;
                    _pc_wave_out.Init(_pc_audio_reader);
                    _pc_wave_out.Play();

                    setPlayingState(true);

                    ret = true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    try { cleanupPcPlayback(); } catch { }
                    setPlayingState(false);
                }
            }
            return ret;
        }

        private void pcWaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            string error;
            StopPlayback(out error);
        }

        private void cleanupPcPlayback()
        {
            if (_pc_wave_out != null)
            {
                try { _pc_wave_out.PlaybackStopped -= pcWaveOut_PlaybackStopped; } catch { }
                try { _pc_wave_out.Stop(); } catch { }
                try { _pc_wave_out.Dispose(); } catch { }
                _pc_wave_out = null;
            }

            if (_pc_audio_reader != null)
            {
                try { _pc_audio_reader.Dispose(); } catch { }
                _pc_audio_reader = null;
            }
        }

        public bool IsPlaying
        {
            get
            {
                bool ret;
                lock (_sync)
                {
                    ret = _is_playing;
                }
                return ret;
            }
        }

        public bool IsRecording
        {
            get
            {
                bool ret;
                lock (_sync)
                {
                    ret = _is_recording;
                }
                return ret;
            }
        }
        public bool IsWDSPBusy
        {
            get
            {
                bool ret;
                lock (_sync)
                {
                    ret = (_active_playback_wfw_id > -1 && _is_playing) || (_active_record_wfw_id > -1 && _is_recording);
                }
                return ret;
            }
        }
        public bool IsBusy
        {
            get
            {
                bool ret;
                lock (_sync)
                {
                    ret = _is_recording || _is_playing;
                }
                return ret;
            }
        }

        public bool StopPlayback(out string error)
        {
            error = null;
            bool wdsp = false;

            lock (_sync)
            {
                if (!_is_playing) return true;                
                try
                {
                    if (_active_playback_wfw_id >= 0)
                    {
                        _console.SetWavePlayback(_active_playback_wfw_id, false);
                        Audio.WavePlayback = false;

                        WaveFileReader1 reader = WaveThing.wave_file_reader[_active_playback_wfw_id];
                        if (reader != null)
                        {
                            reader.Stop();
                            WaveThing.wave_file_reader[_active_playback_wfw_id] = null;
                        }

                        _active_playback_wfw_id = -1;
                        wdsp = true;
                    }

                    cleanupPcPlayback();
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    try { cleanupPcPlayback(); } catch { }
                }
            }
            
            if(wdsp) storeRestoreSettings(false, true);
            setPlayingState(false);
            return error == null;
        }

        private void onPcDataAvailable(object sender, WaveInEventArgs e)
        {
            lock (_sync)
            {
                if (_pc_wave_writer == null) return;

                float gain = _pc_input_gain;
                if (gain <= 0.0f)
                {
                    if (_pc_record_gain_buffer == null || _pc_record_gain_buffer.Length < e.BytesRecorded)
                    {
                        _pc_record_gain_buffer = new byte[e.BytesRecorded];
                    }
                    if (BitDepthMode == AudioBitDepthMode.Pcm8)
                    {
                        int i = 0;
                        while (i < e.BytesRecorded)
                        {
                            _pc_record_gain_buffer[i] = 128;
                            i++;
                        }
                    }
                    else
                    {
                        Array.Clear(_pc_record_gain_buffer, 0, e.BytesRecorded);
                    }
                    _pc_wave_writer.Write(_pc_record_gain_buffer, 0, e.BytesRecorded);
                    _pc_wave_writer.Flush();
                    return;
                }

                if (gain == 1.0f)
                {
                    _pc_wave_writer.Write(e.Buffer, 0, e.BytesRecorded);
                    _pc_wave_writer.Flush();
                    return;
                }

                if (_pc_record_gain_buffer == null || _pc_record_gain_buffer.Length < e.BytesRecorded)
                {
                    _pc_record_gain_buffer = new byte[e.BytesRecorded];
                }

                Buffer.BlockCopy(e.Buffer, 0, _pc_record_gain_buffer, 0, e.BytesRecorded);

                int max = e.BytesRecorded;

                if (BitDepthMode == AudioBitDepthMode.IeeeFloat32)
                {
                    int i = 0;
                    while (i + 3 < max)
                    {
                        float sample = BitConverter.ToSingle(_pc_record_gain_buffer, i);
                        float scaled = sample * gain;
                        if (scaled > 1.0f) scaled = 1.0f;
                        if (scaled < -1.0f) scaled = -1.0f;
                        byte[] b = BitConverter.GetBytes(scaled);
                        _pc_record_gain_buffer[i] = b[0];
                        _pc_record_gain_buffer[i + 1] = b[1];
                        _pc_record_gain_buffer[i + 2] = b[2];
                        _pc_record_gain_buffer[i + 3] = b[3];
                        i += 4;
                    }
                }
                else if (BitDepthMode == AudioBitDepthMode.Pcm32)
                {
                    int i = 0;
                    while (i + 3 < max)
                    {
                        int sample = BitConverter.ToInt32(_pc_record_gain_buffer, i);
                        double scaled = sample * (double)gain;
                        if (scaled > int.MaxValue) scaled = int.MaxValue;
                        if (scaled < int.MinValue) scaled = int.MinValue;
                        int out_sample = (int)scaled;
                        _pc_record_gain_buffer[i] = (byte)(out_sample & 0xFF);
                        _pc_record_gain_buffer[i + 1] = (byte)((out_sample >> 8) & 0xFF);
                        _pc_record_gain_buffer[i + 2] = (byte)((out_sample >> 16) & 0xFF);
                        _pc_record_gain_buffer[i + 3] = (byte)((out_sample >> 24) & 0xFF);
                        i += 4;
                    }
                }
                else if (BitDepthMode == AudioBitDepthMode.Pcm24)
                {
                    int i = 0;
                    while (i + 2 < max)
                    {
                        int sample = _pc_record_gain_buffer[i] | (_pc_record_gain_buffer[i + 1] << 8) | (_pc_record_gain_buffer[i + 2] << 16);
                        if ((sample & 0x800000) != 0) sample |= unchecked((int)0xFF000000);

                        double scaled = sample * (double)gain;
                        if (scaled > 8388607.0) scaled = 8388607.0;
                        if (scaled < -8388608.0) scaled = -8388608.0;

                        int out_sample = (int)scaled;
                        _pc_record_gain_buffer[i] = (byte)(out_sample & 0xFF);
                        _pc_record_gain_buffer[i + 1] = (byte)((out_sample >> 8) & 0xFF);
                        _pc_record_gain_buffer[i + 2] = (byte)((out_sample >> 16) & 0xFF);
                        i += 3;
                    }
                }
                else if (BitDepthMode == AudioBitDepthMode.Pcm8)
                {
                    int i = 0;
                    while (i < max)
                    {
                        int sample = _pc_record_gain_buffer[i] - 128;
                        double scaled = sample * (double)gain;
                        if (scaled > 127.0) scaled = 127.0;
                        if (scaled < -128.0) scaled = -128.0;
                        int out_sample = (int)scaled + 128;
                        _pc_record_gain_buffer[i] = (byte)out_sample;
                        i++;
                    }
                }
                else
                {
                    int i = 0;
                    while (i + 1 < max)
                    {
                        short sample = (short)(_pc_record_gain_buffer[i] | (_pc_record_gain_buffer[i + 1] << 8));
                        float scaled = sample * gain;
                        if (scaled > 32767.0f) scaled = 32767.0f;
                        if (scaled < -32768.0f) scaled = -32768.0f;
                        short out_sample = (short)scaled;
                        _pc_record_gain_buffer[i] = (byte)(out_sample & 0xFF);
                        _pc_record_gain_buffer[i + 1] = (byte)((out_sample >> 8) & 0xFF);
                        i += 2;
                    }
                }

                _pc_wave_writer.Write(_pc_record_gain_buffer, 0, e.BytesRecorded);
                _pc_wave_writer.Flush();
            }
        }

        private void onPcRecordingStopped(object sender, StoppedEventArgs e)
        {
            string wav;
            string json;
            string mp3;
            RecordingDetails details;
            string unique_id;

            lock (_sync)
            {
                wav = _active_record_filename;
                json = _active_record_json_filename;
                mp3 = _active_record_mp3_filename;
                details = _active_record_details;
                unique_id = _active_record_id;

                cleanupPcRecording();
            }

            recordCompleted(unique_id, wav, json, mp3, details);
        }

        private void cleanupPcRecording()
        {
            if (_pc_wave_in != null)
            {
                try { _pc_wave_in.DataAvailable -= onPcDataAvailable; } catch { }
                try { _pc_wave_in.RecordingStopped -= onPcRecordingStopped; } catch { }
                try { _pc_wave_in.Dispose(); } catch { }
                _pc_wave_in = null;
            }

            if (_pc_wave_writer != null)
            {
                try { _pc_wave_writer.Dispose(); } catch { }
                _pc_wave_writer = null;
            }
        }

        private void onWdspPlaybackFinished()
        {
            string error;
            StopPlayback(out error);
        }

        private void onWdspRecordFinished(string wavPath)
        {
            string wav;
            string json;
            string mp3;
            RecordingDetails details;
            string unique_id;

            lock (_sync)
            {
                if (string.IsNullOrWhiteSpace(_active_record_filename))
                {
                    return;
                }

                if (!string.Equals(_active_record_filename, wavPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (_active_record_wfw_id >= 0 && _active_record_wfw_id < WaveThing.wave_file_writer.Length)
                {
                    WaveThing.wave_file_writer[_active_record_wfw_id] = null;
                }

                wav = _active_record_filename;
                json = _active_record_json_filename;
                mp3 = _active_record_mp3_filename;
                details = _active_record_details;
                unique_id = _active_record_id;
            }

            recordCompleted(unique_id, wav, json, mp3, details);
        }

        private void recordCompleted(string unique_id, string wav, string json, string mp3, RecordingDetails details)
        {
            Action a = delegate
            {
                if (details != null)
                {
                    details.UtcTime = ensureUtc(details.UtcTime);

                    if (!string.IsNullOrWhiteSpace(wav))
                    {
                        details.WavFile = Path.GetFileName(wav) ?? "";
                        try
                        {
                            if (File.Exists(wav))
                            {
                                FileInfo fi = new FileInfo(wav);
                                details.WavFileSizeBytes = fi.Length;
                                details.WavFileLastWriteUtc = DateTime.SpecifyKind(fi.LastWriteTimeUtc, DateTimeKind.Utc);
                                details.PlayDurationSeconds = tryGetWavDurationSeconds(wav);
                            }
                            else
                            {
                                details.WavFileSizeBytes = null;
                                details.WavFileLastWriteUtc = null;
                            }
                        }
                        catch
                        {
                            details.WavFileSizeBytes = null;
                        }
                    }
                }

                string mp3Path = null;

                if (GenerateMP3File && details != null && !string.IsNullOrWhiteSpace(wav) && File.Exists(wav))
                {
                    mp3Path = Path.ChangeExtension(wav, ".mp3");
                    try { if (File.Exists(mp3Path)) File.Delete(mp3Path); } catch { }

                    bool mp3Ok = generateMp3FromWav(wav, mp3Path);
                    if (!mp3Ok)
                    {
                        try { if (File.Exists(mp3Path)) File.Delete(mp3Path); } catch { }
                        mp3Path = null;
                    }
                    else
                    {
                        details.Mp3File = Path.GetFileName(mp3Path) ?? "";
                        try
                        {
                            if (File.Exists(mp3Path))
                            {
                                FileInfo fi = new FileInfo(mp3Path);
                                details.Mp3FileSizeBytes = fi.Length;
                            }
                            else
                            {
                                details.Mp3FileSizeBytes = null;
                            }
                        }
                        catch
                        {
                            details.Mp3FileSizeBytes = null;
                        }
                    }
                }

                if (GenerateJSON && details != null && !string.IsNullOrWhiteSpace(json))
                {
                    try { if (File.Exists(json)) File.Delete(json); } catch { }
                    bool ok = writeRecordingJson(unique_id, json, details);
                    if (!ok)
                    {
                        try { if (File.Exists(json)) File.Delete(json); } catch { }
                    }
                }

                setRecordingState(false);

                lock (_sync)
                {
                    clearActiveRecordLocked();
                }
            };

            if (_sync_context != null)
            {
                _sync_context.Post(delegate { a(); }, null);
            }
            else
            {
                a();
            }
        }

        private bool writeRecordingJson(string unique_id, string jsonPath, RecordingDetails details)
        {
            try
            {
                DateTime utc = ensureUtc(details.UtcTime);

                RecordingJsonModel m = new RecordingJsonModel();
                m.utc_time = utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                m.frequency = details.Frequency ?? "";
                m.mode = details.Mode ?? "";
                m.band = details.Band ?? "";

                m.wav_file = details.WavFile ?? "";
                m.wav_file_size_bytes = details.WavFileSizeBytes;

                DateTime? wavLast = details.WavFileLastWriteUtc;

                if (!wavLast.HasValue)
                {
                    try
                    {
                        string wavGuess = Path.ChangeExtension(jsonPath, ".wav");
                        if (File.Exists(wavGuess))
                        {
                            wavLast = DateTime.SpecifyKind(File.GetLastWriteTimeUtc(wavGuess), DateTimeKind.Utc);
                        }
                    }
                    catch
                    {
                        wavLast = null;
                    }
                }

                m.wav_file_last_write_utc = wavLast.HasValue ? ensureUtc(wavLast.Value).ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : "";

                m.sample_rate = details.SampleRate;
                m.bit_depth = details.BitDepth;
                m.channels = details.Channels;
                m.format_tag = details.FormatTag;
                string fmt = details.FormatTag == 3 ? "IEEE_FLOAT" : (details.FormatTag == 1 ? "PCM" : details.FormatTag.ToString());
                m.tag_description = fmt + " " + details.BitDepth.ToString() + "-bit";

                m.mp3_file = details.Mp3File ?? "";
                m.mp3_file_size_bytes = details.Mp3FileSizeBytes;

                double? dur = details.PlayDurationSeconds;

                if (!dur.HasValue)
                {
                    try
                    {
                        string wavGuess = Path.ChangeExtension(jsonPath, ".wav");
                        dur = tryGetWavDurationSeconds(wavGuess);
                    }
                    catch
                    {
                        dur = null;
                    }
                }

                m.play_duration_seconds = dur;

                string s = JsonConvert.SerializeObject(m, Formatting.Indented);

                string dir = Path.GetDirectoryName(jsonPath);
                if (!string.IsNullOrWhiteSpace(dir)) ensureFolderExists(dir);

                File.WriteAllText(jsonPath, s, Encoding.UTF8);
                raiseRecordingJsonWritten(unique_id, m);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void raiseRecordingJsonWritten(string unique_id, RecordingJsonModel json_data)
        {
            Action<string, RecordingJsonModel> h = RecordingJsonWritten;
            if (h == null) return;

            string uid = unique_id ?? string.Empty;

            Delegate[] list = h.GetInvocationList();
            for (int i = 0; i < list.Length; i++)
            {
                try
                {
                    ((Action<string, RecordingJsonModel>)list[i])(uid, json_data);
                }
                catch { }
            }
        }
        private static void ensureMediaFoundation()
        {
            lock (_mf_sync)
            {
                if (_mf_started) return;
                try
                {
                    MediaFoundationApi.Startup();
                    _mf_started = true;
                }
                catch
                {
                    _mf_started = false;
                }
            }
        }

        private bool generateMp3FromWav(string wavPath, string mp3Path)
        {
            try
            {
                ensureMediaFoundation();
                if (!_mf_started) return false;

                using (AudioFileReader reader = new AudioFileReader(wavPath))
                {
                    MediaFoundationEncoder.EncodeToMp3(reader, mp3Path, 192000);
                }
                return File.Exists(mp3Path);
            }
            catch
            {
                return false;
            }
        }

        private void clearActiveRecordLocked()
        {
            _active_record_id = null;
            _active_record_filename = null;
            _active_record_json_filename = null;
            _active_record_mp3_filename = null;
            _active_record_details = null;
            _active_record_wfw_id = -1;
            _active_record_sample_rate = 0;
        }

        private void setRecordingState(bool recording)
        {
            bool raise = false;
            string record_id = null;
            string filename = null;

            lock (_sync)
            {
                if (_is_recording != recording)
                {
                    _is_recording = recording;
                    record_id = _active_record_id;
                    filename = _active_record_filename;
                    raise = true;

                    if (!recording)
                    {
                        _active_record_wfw_id = -1;
                    }
                }
            }

            if (raise)
            {
                Action<bool, string, string> h = RecordingChanged;
                if (h != null)
                {
                    Delegate[] list = h.GetInvocationList();
                    for (int i = 0; i < list.Length; i++)
                    {
                        try
                        {
                            ((Action<bool, string, string>)list[i])(recording, record_id, filename);
                        }
                        catch { }
                    }
                }
            }
        }

        private void setPlayingState(bool playing)
        {
            bool raise = false;
            string play_id = null;
            string filename = null;
            bool isWdsp = false;

            lock (_sync)
            {
                if (_is_playing != playing)
                {
                    _is_playing = playing;
                    play_id = _active_play_id;
                    filename = _active_play_filename;
                    isWdsp = _is_wdsp_playing;
                    raise = true;

                    if (!playing)
                    {
                        _active_play_id = null;
                        _active_play_filename = null;
                        _is_wdsp_playing = false;
                        _active_playback_wfw_id = -1;
                    }
                }
            }

            if (raise)
            {
                Action<bool, string, string, bool> h = PlayingChanged;
                if (h != null)
                {
                    Delegate[] list = h.GetInvocationList();
                    for (int i = 0; i < list.Length; i++)
                    {
                        try
                        {
                            ((Action<bool, string, string, bool>)list[i])(playing, play_id, filename, isWdsp);
                        }
                        catch { }
                    }
                }
            }
        }

        private static string generateFilename(string prefix, int suffixNumber, string ext)
        {
            DateTime now = DateTime.Now;
            string stamp = now.ToString("yyyyMMdd_HHmmss");
            return prefix + "_" + stamp + "_" + suffixNumber.ToString() + "." + ext;
        }

        private static void ensureFolderExists(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) return;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        }

        private static bool tryParseWaveHeader(BinaryReader reader, out int formatTag, out int sampleRate, out int channels, out int bitsPerSample, out long dataStart, out long dataLengthBytes, out string error)
        {
            formatTag = 0;
            sampleRate = 0;
            channels = 0;
            bitsPerSample = 0;
            dataStart = 0;
            dataLengthBytes = 0;
            error = null;

            if (reader == null)
            {
                error = "Reader is null.";
                return false;
            }

            try
            {
                reader.BaseStream.Position = 0;

                RIFFChunk riff = null;
                fmtChunk fmt = null;
                dataChunk data = null;
                long data_start = 0;

                bool ok = true;

                if (reader.BaseStream.Length < 16)
                {
                    error = "File is too small.";
                    return false;
                }

                if (reader.PeekChar() != 'R')
                {
                    error = "Not a RIFF file.";
                    return false;
                }

                while (ok && (data == null || riff == null || fmt == null) && reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    try
                    {
                        Chunk c = Chunk.ReadChunk(ref reader);
                        if (c is RIFFChunk) riff = (RIFFChunk)c;
                        else if (c is fmtChunk) fmt = (fmtChunk)c;
                        else if (c is dataChunk)
                        {
                            data = (dataChunk)c;
                            data_start = reader.BaseStream.Position;
                        }
                        else
                        {
                            int size = 0;
                            if (reader.BaseStream.Position + 4 <= reader.BaseStream.Length)
                            {
                                size = reader.ReadInt32();
                                if (size < 0) size = 0;
                                long next = reader.BaseStream.Position + size;
                                if (next > reader.BaseStream.Length) next = reader.BaseStream.Length;
                                reader.BaseStream.Position = next;
                            }
                            else
                            {
                                ok = false;
                            }
                        }
                    }
                    catch
                    {
                        ok = false;
                    }
                }

                if (!ok || riff == null || fmt == null)
                {
                    error = "Unable to read wave header.";
                    return false;
                }

                if (riff.riff_type != 0x45564157)
                {
                    error = "Not a WAVE file.";
                    return false;
                }

                formatTag = fmt.format;
                sampleRate = fmt.sample_rate;
                channels = fmt.channels;
                bitsPerSample = fmt.bits_per_sample;

                if (channels < 1 || channels > 2)
                {
                    error = "Unsupported channel count.";
                    return false;
                }

                if (formatTag == 3 && bitsPerSample != 32)
                {
                    error = "IEEE float must be 32-bit.";
                    return false;
                }

                if (formatTag == 1)
                {
                    if (bitsPerSample != 8 && bitsPerSample != 16 && bitsPerSample != 24 && bitsPerSample != 32)
                    {
                        error = "Unsupported PCM bit depth.";
                        return false;
                    }
                }

                if (data != null)
                {
                    dataLengthBytes = data.chunk_size;
                    if (dataLengthBytes < 0) dataLengthBytes = 0;
                }

                dataStart = data_start;
                if (dataStart < 0) dataStart = 0;
                if (dataStart > reader.BaseStream.Length) dataStart = reader.BaseStream.Length;
                reader.BaseStream.Position = dataStart;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public sealed class RecordingJsonModel
        {
            public string utc_time { get; set; }
            public string frequency { get; set; }
            public string mode { get; set; }
            public string band { get; set; }

            public string wav_file { get; set; }
            public long? wav_file_size_bytes { get; set; }
            public string wav_file_last_write_utc { get; set; }
            public double? play_duration_seconds { get; set; }

            public int sample_rate { get; set; }
            public short bit_depth { get; set; }
            public short channels { get; set; }
            public short format_tag { get; set; }
            public string tag_description { get; set; }

            public string mp3_file { get; set; }
            public long? mp3_file_size_bytes { get; set; }
        }
    }

    public class Chunk
    {
        public int chunk_id;

        public static Chunk ReadChunk(ref BinaryReader reader)
        {
            int data = reader.ReadInt32();

            if (data == 0x46464952)
            {
                RIFFChunk riff = new RIFFChunk();
                riff.chunk_id = data;
                riff.file_size = reader.ReadInt32();
                riff.riff_type = reader.ReadInt32();
                return riff;
            }

            if (data == 0x20746D66)
            {
                fmtChunk fmt = new fmtChunk();
                fmt.chunk_id = data;
                fmt.chunk_size = reader.ReadInt32();
                fmt.format = reader.ReadInt16();
                fmt.channels = reader.ReadInt16();
                fmt.sample_rate = reader.ReadInt32();
                fmt.bytes_per_sec = reader.ReadInt32();
                fmt.block_align = reader.ReadInt16();
                fmt.bits_per_sample = reader.ReadInt16();

                long remaining = fmt.chunk_size - 16;
                if (remaining > 0)
                {
                    long next = reader.BaseStream.Position + remaining;
                    if (next > reader.BaseStream.Length) next = reader.BaseStream.Length;
                    reader.BaseStream.Position = next;
                }

                return fmt;
            }

            if (data == 0x61746164)
            {
                dataChunk dc = new dataChunk();
                dc.chunk_id = data;
                dc.chunk_size = reader.ReadInt32();
                return dc;
            }

            Chunk c2 = new Chunk();
            c2.chunk_id = data;
            return c2;
        }
    }

    public class RIFFChunk : Chunk
    {
        public int file_size;
        public int riff_type;
    }

    public class fmtChunk : Chunk
    {
        public int chunk_size;
        public short format;
        public short channels;
        public int sample_rate;
        public int bytes_per_sec;
        public short block_align;
        public short bits_per_sample;
    }

    public class dataChunk : Chunk
    {
        public int chunk_size;
    }

    unsafe public class WaveFileWriter
    {
        public bool DitherEnabled;
        public float DitherAmount;

        private readonly Random rnd = new Random();

        private int _id;
        private BinaryWriter _writer;
        private bool _record;
        private short _channels;
        private short _format_tag;
        private short _bit_depth;
        private int _length_counter;
        private RingBufferFloat _rb_l;
        private RingBufferFloat _rb_r;
        private float[] _in_buf_l;
        private float[] _in_buf_r;
        private float[] _out_buf_l;
        private float[] _out_buf_r;
        private float[] _out_buf;
        private byte[] _byte_buf;
        private const int IN_BLOCK = 2048;

        unsafe private void* _rcvr_resamp_l = null;
        unsafe public void* RcvrResampL { get { return _rcvr_resamp_l; } }

        unsafe private void* _rcvr_resamp_r = null;
        unsafe public void* RcvrResampR { get { return _rcvr_resamp_r; } }

        unsafe private void* _xmtr_resamp_l = null;
        unsafe public void* XmtrResampL { get { return _xmtr_resamp_l; } }

        unsafe private void* _xmtr_resamp_r = null;
        unsafe public void* XmtrResampR { get { return _xmtr_resamp_r; } }

        private int _sample_rate;
        public int BaseRate { get { return _sample_rate; } }

        private int _rcvr_rate;
        public int RcvrRate { get { return _rcvr_rate; } }

        private int _rcvr_size;
        public int RcvrSize { get { return _rcvr_size; } }

        private int _xmtr_rate;
        public int XmtrRate { get { return _xmtr_rate; } }

        private int _xmtr_size;
        public int XmtrSize { get { return _xmtr_size; } }

        private volatile bool _mox = false;
        private volatile float _fInverseGain = 1f;

        private readonly string _file;
        private readonly Action<string> _finished;

        public WaveFileWriter(int wfw_id, short chan, int samp_rate, string file, bool recordRxPreProcessed, bool recordTxPreProcessed, short formatTag, short bitDepth, Action<string> finished)
        {
            _id = wfw_id;
            _file = file;
            _finished = finished;

            if (recordRxPreProcessed)
            {
                _rcvr_rate = cmaster.GetInputRate(0, _id);
                _rcvr_size = cmaster.GetBuffSize(_rcvr_rate);
            }
            else
            {
                _rcvr_rate = cmaster.GetChannelOutputRate(0, _id);
                _rcvr_size = cmaster.GetBuffSize(_rcvr_rate);
            }

            if (recordTxPreProcessed)
            {
                _xmtr_rate = cmaster.GetInputRate(1, 0);
                _xmtr_size = cmaster.GetBuffSize(_xmtr_rate);
            }
            else
            {
                _xmtr_rate = cmaster.GetChannelOutputRate(1, 0);
                _xmtr_size = cmaster.GetBuffSize(_xmtr_rate);
            }

            if (wfw_id == 0) RecordGain = (float)Audio.console.radio.GetDSPRX(0, 0).RXOutputGain;
            else if (wfw_id == 1) RecordGain = (float)Audio.console.radio.GetDSPRX(1, 0).RXOutputGain;

            _channels = chan;
            _sample_rate = samp_rate;
            _format_tag = formatTag;
            _bit_depth = bitDepth;

            int outBlock = (int)Math.Ceiling(IN_BLOCK * (double)_sample_rate / (double)Math.Min(_rcvr_size, _xmtr_size));
            _rb_l = new RingBufferFloat(IN_BLOCK * 16);
            _rb_r = new RingBufferFloat(IN_BLOCK * 16);
            _in_buf_l = new float[IN_BLOCK];
            _in_buf_r = new float[IN_BLOCK];
            _out_buf_l = new float[outBlock];
            _out_buf_r = new float[outBlock];
            _out_buf = new float[outBlock * 2];
            _byte_buf = new byte[outBlock * 2 * 4];

            _length_counter = 0;
            _record = true;

            if (_sample_rate != _rcvr_rate)
            {
                _rcvr_resamp_l = WDSP.create_resampleFV(_rcvr_rate, _sample_rate);
                _rcvr_resamp_r = WDSP.create_resampleFV(_rcvr_rate, _sample_rate);
            }

            if (_sample_rate != _xmtr_rate)
            {
                _xmtr_resamp_l = WDSP.create_resampleFV(_xmtr_rate, _sample_rate);
                _xmtr_resamp_r = WDSP.create_resampleFV(_xmtr_rate, _sample_rate);
            }

            _writer = new BinaryWriter(File.Open(file, FileMode.Create));

            Thread t = new Thread(new ThreadStart(ProcessRecordBuffers));
            t.Name = "Wave File Write Thread";
            t.IsBackground = true;
            t.Priority = ThreadPriority.Normal;
            t.Start();
        }

        public float RecordGain
        {
            set
            {
                UpdateMox();

                if (value <= 0)
                {
                    _fInverseGain = 0;
                    return;
                }

                if (value > 1f) value = 1f;

                _fInverseGain = 1 / value;
            }
        }

        public void UpdateMox()
        {
            switch (_id)
            {
                case 0:
                    _mox = Audio.MOX && (Audio.console.VFOATX || (Audio.console.VFOBTX && !Audio.console.RX2Enabled));
                    break;
                case 1:
                    _mox = Audio.MOX && Audio.console.RX2Enabled && Audio.console.VFOBTX;
                    break;
                default:
                    _mox = Audio.MOX;
                    break;
            }
        }

        private void ProcessRecordBuffers()
        {
            WriteWaveHeader(ref _writer, _channels, _sample_rate, _format_tag, _bit_depth, 0);

            while (_record == true || _rb_l.ReadSpace() > 0)
            {
                while (_rb_l.ReadSpace() > IN_BLOCK || (_record == false && _rb_l.ReadSpace() > 0))
                {
                    WriteBuffer(ref _writer, ref _length_counter);
                }
                Thread.Sleep(3);
            }

            try
            {
                _writer.Seek(0, SeekOrigin.Begin);
                WriteWaveHeader(ref _writer, _channels, _sample_rate, _format_tag, _bit_depth, _length_counter);
                _writer.Flush();
                _writer.Close();
            }
            catch
            {
                try { _writer.Close(); } catch { }
            }

            try
            {
                Action<string> f = _finished;
                if (f != null) f(_file);
            }
            catch { }
        }

        unsafe public void AddWriteBuffer(float* left, float* right, int nsamps)
        {
            _rb_l.WritePtr(left, nsamps);
            _rb_r.WritePtr(right, nsamps);
        }

        public void Stop()
        {
            _record = false;
        }

        private void WriteBuffer(ref BinaryWriter w, ref int count)
        {
            float fGain = (!_mox) ? _fInverseGain : 1f;

            int cntL = _rb_l.Read(_in_buf_l, IN_BLOCK);
            int cntR = _rb_r.Read(_in_buf_r, IN_BLOCK);
            if (cntL != cntR) return;

            int out_cnt = cntL;

            _in_buf_l.CopyTo(_out_buf_l, 0);
            _in_buf_r.CopyTo(_out_buf_r, 0);

            if (_channels > 1)
            {
                for (int i = 0; i < out_cnt; i++)
                {
                    _out_buf[i * 2] = _out_buf_l[i] * fGain;
                    if (_out_buf[i * 2] > 1.0f) _out_buf[i * 2] = 1.0f;
                    else if (_out_buf[i * 2] < -1.0f) _out_buf[i * 2] = -1.0f;

                    _out_buf[i * 2 + 1] = _out_buf_r[i] * fGain;
                    if (_out_buf[i * 2 + 1] > 1.0f) _out_buf[i * 2 + 1] = 1.0f;
                    else if (_out_buf[i * 2 + 1] < -1.0f) _out_buf[i * 2 + 1] = -1.0f;
                }
            }
            else
            {
                for (int i = 0; i < out_cnt; i++)
                {
                    _out_buf_l[i] = _out_buf_l[i] * fGain;
                    if (_out_buf_l[i] > 1.0f) _out_buf_l[i] = 1.0f;
                    else if (_out_buf_l[i] < -1.0f) _out_buf_l[i] = -1.0f;
                }

                _out_buf_l.CopyTo(_out_buf, 0);
            }

            int length = out_cnt;
            if (_channels > 1) length *= 2;

            if (_bit_depth == 32) Write_32(length, ref count, out_cnt, w);
            else if (_bit_depth == 24) Write_24(length, ref count, out_cnt, w);
            else if (_bit_depth == 16) Write_16(length, ref count, out_cnt, w);
            else if (_bit_depth == 8) Write_8(length, ref count, out_cnt, w);
        }

        private void Write_32(int length, ref int count, int out_cnt, BinaryWriter w)
        {
            for (int i = 0; i < length; i++)
            {
                if (_format_tag == 3)
                {
                    byte[] temp = BitConverter.GetBytes(_out_buf[i]);
                    _byte_buf[i * 4] = temp[0];
                    _byte_buf[i * 4 + 1] = temp[1];
                    _byte_buf[i * 4 + 2] = temp[2];
                    _byte_buf[i * 4 + 3] = temp[3];
                }
                else
                {
                    int intSample = dither32(_out_buf[i] * 0x80000000);
                    _byte_buf[i * 4 + 3] = (byte)(intSample >> 24);
                    _byte_buf[i * 4 + 2] = (byte)(((uint)intSample >> 16) & 0xFF);
                    _byte_buf[i * 4 + 1] = (byte)(((uint)intSample >> 8) & 0xFF);
                    _byte_buf[i * 4] = (byte)(intSample & 0xFF);
                }
            }

            w.Write(_byte_buf, 0, out_cnt * 2 * 4);
            count += out_cnt * 2 * 4;
        }

        private void Write_24(int length, ref int count, int out_cnt, BinaryWriter w)
        {
            for (int i = 0; i < length; i++)
            {
                int intSample = dither24(_out_buf[i] * 0x800000);
                _byte_buf[i * 3 + 2] = (byte)(intSample >> 16);
                _byte_buf[i * 3 + 1] = (byte)(((uint)intSample >> 8) & 0xFF);
                _byte_buf[i * 3] = (byte)(intSample & 0xFF);
            }

            w.Write(_byte_buf, 0, out_cnt * 2 * 3);
            count += out_cnt * 2 * 3;
        }

        private void Write_16(int length, ref int count, int out_cnt, BinaryWriter w)
        {
            for (int i = 0; i < length; i++)
            {
                int intSample = dither16(_out_buf[i] * 0x8000);
                _byte_buf[i * 2 + 1] = (byte)(intSample >> 8);
                _byte_buf[i * 2] = (byte)(intSample & 0xFF);
            }

            w.Write(_byte_buf, 0, out_cnt * 2 * 2);
            count += out_cnt * 2 * 2;
        }

        private void Write_8(int length, ref int count, int out_cnt, BinaryWriter w)
        {
            for (int i = 0; i < length; i++)
            {
                sbyte v = dither8(_out_buf[i] * 0x80);
                _byte_buf[i] = (byte)(v + 128);
            }

            w.Write(_byte_buf, 0, out_cnt * 2);
            count += out_cnt * 2;
        }

        private float getDitherAmp()
        {
            float a = DitherAmount;
            if (a < 0.1f) a = 0.1f;
            if (a > 1f) a = 1f;
            return a;
        }

        private int dither32(float sample)
        {
            if (DitherEnabled)
            {
                sample += (float)rnd.NextDouble() * getDitherAmp();
            }

            if (sample >= 2147483647.0f) return 2147483647;
            if (sample <= -2147483648.0f) return -2147483648;

            return (int)(sample < 0 ? (sample - 0.5f) : (sample + 0.5f));
        }

        private int dither24(float sample)
        {
            if (DitherEnabled)
            {
                sample += (float)rnd.NextDouble() * getDitherAmp();
            }

            if (sample >= 8388607.0f) return 8388607;
            if (sample <= -8388608.0f) return -8388608;

            return (int)(sample < 0 ? (sample - 0.5f) : (sample + 0.5f));
        }

        private int dither16(float sample)
        {
            if (DitherEnabled)
            {
                sample += (float)rnd.NextDouble() * getDitherAmp();
            }

            if (sample >= 32767.0f) return 32767;
            if (sample <= -32768.0f) return -32768;

            return (int)(sample < 0 ? (sample - 0.5f) : (sample + 0.5f));
        }

        private sbyte dither8(float sample)
        {
            if (DitherEnabled)
            {
                sample += (float)rnd.NextDouble() * getDitherAmp();
            }

            if (sample >= 127.0f) return (sbyte)127;
            if (sample <= -128.0f) return (sbyte)-128;

            return (sbyte)(sample < 0 ? (sample - 0.5f) : (sample + 0.5f));
        }

        private static void WriteWaveHeader(ref BinaryWriter w, short channels, int sample_rate, short format_tag, short bit_depth, int data_length)
        {
            w.Write(0x46464952);
            w.Write(data_length + 36);
            w.Write(0x45564157);
            w.Write(0x20746d66);
            w.Write(16);
            w.Write(format_tag);
            w.Write(channels);
            w.Write(sample_rate);
            w.Write((int)(channels * sample_rate * bit_depth / 8));
            w.Write((short)(channels * bit_depth / 8));
            w.Write(bit_depth);
            w.Write(0x61746164);
            w.Write(data_length);
            w.Flush();
        }
    }

    unsafe public class WaveFileReader1
    {
        private int id;
        private int rcvr_rate;
        private int xmtr_rate;
        private int rcvr_size;
        private int xmtr_size;

        private BinaryReader reader;
        private int format;
        private int sample_rate;
        private int channels;
        private int bitdepth;

        private bool playback;

        private RingBufferFloat rb_l;
        private RingBufferFloat rb_r;

        private float[] buf_l_in;
        private float[] buf_r_in;
        private float[] buf_l_out;
        private float[] buf_r_out;

        private int IN_BLOCK;
        private int OUT_BLOCK;

        private byte[] io_buf;
        private int io_buf_size;

        private bool eof;
        private int total_samps_written;
        private int total_samps_read;

        unsafe private void* rcvr_resamp_l;
        unsafe private void* rcvr_resamp_r;
        unsafe private void* xmtr_resamp_l;
        unsafe private void* xmtr_resamp_r;

        private readonly Action _finished;

        private bool _fade_enabled;
        private int _fade_ms;
        private int _fade_frames;
        private float[] _fade_gain;
        private long _total_frames;
        private long _frames_read;
        private int _bytes_per_frame;

        public WaveFileReader1(int wfr_id, int fmt, int samp_rate, int chan, int bit_depth, long data_length_bytes, bool fade_enabled, int fade_ms, BinaryReader binread, Action finished)
        {
            id = wfr_id;
            format = fmt;
            sample_rate = samp_rate;
            channels = chan;
            bitdepth = bit_depth;
            reader = binread;
            _finished = finished;

            _fade_enabled = fade_enabled;
            _fade_ms = fade_ms;
            if (_fade_ms < 0) _fade_ms = 0;

            if (channels < 1) channels = 1;
            if (channels > 2) channels = 2;

            if (format == 3)
            {
                _bytes_per_frame = channels * 4;
            }
            else
            {
                int bytes_per_sample = bitdepth / 8;
                if (bytes_per_sample < 1) bytes_per_sample = 1;
                _bytes_per_frame = channels * bytes_per_sample;
            }

            if (_bytes_per_frame < 1) _bytes_per_frame = 1;

            _total_frames = 0;
            if (data_length_bytes > 0) _total_frames = data_length_bytes / (long)_bytes_per_frame;

            _frames_read = 0;

            _fade_frames = 0;
            _fade_gain = null;

            if (_fade_enabled && _fade_ms > 0 && sample_rate > 0 && _total_frames > 0)
            {
                _fade_frames = (int)Math.Round(sample_rate * (_fade_ms / 1000.0));
                if (_fade_frames < 0) _fade_frames = 0;

                if (_fade_frames * 2 > _total_frames)
                {
                    _fade_frames = (int)(_total_frames / 2);
                    if (_fade_frames < 0) _fade_frames = 0;
                }

                if (_fade_frames > 0)
                {
                    _fade_gain = buildCosineFade(_fade_frames);
                }
            }

            rcvr_rate = cmaster.GetInputRate(0, id);
            xmtr_rate = cmaster.GetInputRate(1, 0);

            int max_rate = (rcvr_rate >= xmtr_rate) ? rcvr_rate : xmtr_rate;

            rcvr_size = cmaster.GetBuffSize(rcvr_rate);
            xmtr_size = cmaster.GetBuffSize(xmtr_rate);

            IN_BLOCK = 2048;
            OUT_BLOCK = (int)Math.Ceiling(IN_BLOCK * (double)max_rate / (double)sample_rate);

            rb_l = new RingBufferFloat(16 * OUT_BLOCK);
            rb_r = new RingBufferFloat(16 * OUT_BLOCK);

            buf_l_in = new float[IN_BLOCK];
            buf_r_in = new float[IN_BLOCK];
            buf_l_out = new float[OUT_BLOCK];
            buf_r_out = new float[OUT_BLOCK];

            if (format == 1)
            {
                if (bitdepth == 32) io_buf_size = IN_BLOCK * 4 * 2;
                else if (bitdepth == 24) io_buf_size = IN_BLOCK * 3 * 2;
                else if (bitdepth == 16) io_buf_size = IN_BLOCK * 2 * 2;
                else if (bitdepth == 8) io_buf_size = IN_BLOCK * 2;
                else io_buf_size = IN_BLOCK * 2 * 2;
            }
            else
            {
                io_buf_size = IN_BLOCK * 4 * 2;
            }

            if (sample_rate != rcvr_rate)
            {
                rcvr_resamp_l = WDSP.create_resampleFV(sample_rate, rcvr_rate);
                if (channels > 1) rcvr_resamp_r = WDSP.create_resampleFV(sample_rate, rcvr_rate);
            }

            if (sample_rate != xmtr_rate)
            {
                xmtr_resamp_l = WDSP.create_resampleFV(sample_rate, xmtr_rate);
                if (channels > 1) xmtr_resamp_r = WDSP.create_resampleFV(sample_rate, xmtr_rate);
            }

            io_buf = new byte[io_buf_size];

            playback = true;
            eof = false;
            total_samps_written = 0;
            total_samps_read = 0;

            do
            {
                ReadBuffer(ref reader);
            } while (rb_l.WriteSpace() > OUT_BLOCK && !eof);

            Thread t = new Thread(new ThreadStart(ProcessBuffers));
            t.Name = "Wave File Read Thread";
            t.IsBackground = true;
            t.Priority = ThreadPriority.Normal;
            t.Start();
        }

        private static float[] buildCosineFade(int frames)
        {
            if (frames < 1) return new float[0];

            float[] g = new float[frames];

            if (frames == 1)
            {
                g[0] = 1.0f;
                return g;
            }

            int n = 0;
            while (n < frames)
            {
                double x = (double)n / (double)(frames - 1);
                double v = 0.5 - 0.5 * Math.Cos(Math.PI * x);
                g[n] = (float)v;
                n++;
            }

            return g;
        }

        public void Stop()
        {
            playback = false;
        }

        private void ProcessBuffers()
        {
            while (playback)
            {
                while (rb_l.WriteSpace() >= OUT_BLOCK && !eof)
                {
                    ReadBuffer(ref reader);
                    if (!playback) break;
                }

                if (!playback) break;

                Thread.Sleep(10);
            }

            try { reader.Close(); } catch { }
        }

        private void ReadBuffer(ref BinaryReader r)
        {
            int num_reads = IN_BLOCK;
            int val = r.Read(io_buf, 0, io_buf_size);

            if (val < io_buf_size)
            {
                eof = true;

                if (format == 1)
                {
                    if (bitdepth == 32) num_reads = val / 8;
                    else if (bitdepth == 24) num_reads = val / 6;
                    else if (bitdepth == 16) num_reads = val / 4;
                    else if (bitdepth == 8) num_reads = val / 2;
                    else num_reads = val / 4;
                }
                else
                {
                    num_reads = val / 8;
                }
            }

            for (int i = 0; i < num_reads; i++)
            {
                if (format == 1)
                {
                    if (bitdepth == 32)
                    {
                        buf_l_in[i] = ((io_buf[i * 8 + 3] << 24)
                                       | ((io_buf[i * 8 + 2] & 0xFF) << 16)
                                       | ((io_buf[i * 8 + 1] & 0xFF) << 8)
                                       | (io_buf[i * 8] & 0xFF))
                                       / 2147483648.0f;

                        buf_r_in[i] = ((io_buf[i * 8 + 7] << 24)
                                       | ((io_buf[i * 8 + 6] & 0xFF) << 16)
                                       | ((io_buf[i * 8 + 5] & 0xFF) << 8)
                                       | (io_buf[i * 8 + 4] & 0xFF))
                                       / 2147483648.0f;
                    }
                    else if (bitdepth == 24)
                    {
                        buf_l_in[i] = (((io_buf[i * 6 + 2] << 24)
                                      | ((io_buf[i * 6 + 1] & 0xFF) << 16)
                                      | ((io_buf[i * 6] & 0xFF) << 8)) >> 8)
                                      / 8388608.0f;

                        buf_r_in[i] = (((io_buf[i * 6 + 5] << 24)
                                       | ((io_buf[i * 6 + 4] & 0xFF) << 16)
                                       | ((io_buf[i * 6 + 3] & 0xFF) << 8)) >> 8)
                                       / 8388608.0f;
                    }
                    else if (bitdepth == 16)
                    {
                        buf_l_in[i] = (float)((double)BitConverter.ToInt16(io_buf, i * 4) / 32767.0);
                        buf_r_in[i] = (float)((double)BitConverter.ToInt16(io_buf, i * 4 + 2) / 32767.0);
                    }
                    else
                    {
                        buf_l_in[i] = ((io_buf[i * 2] & 0xFF) - 128) / 128.0f;
                        buf_r_in[i] = ((io_buf[i * 2 + 1] & 0xFF) - 128) / 128.0f;
                    }
                }
                else
                {
                    buf_l_in[i] = BitConverter.ToSingle(io_buf, i * 8);
                    buf_r_in[i] = BitConverter.ToSingle(io_buf, i * 8 + 4);
                }
            }

            if (_fade_enabled && _fade_frames > 0 && _fade_gain != null && _fade_gain.Length == _fade_frames && _total_frames > 0)
            {
                long tail_start = _total_frames - _fade_frames;

                int i = 0;
                while (i < num_reads)
                {
                    long frame_index = _frames_read + i;
                    float gain = 1.0f;

                    if (frame_index < _fade_frames)
                    {
                        gain *= _fade_gain[(int)frame_index];
                    }

                    if (frame_index >= tail_start)
                    {
                        long tail_index = _total_frames - 1 - frame_index;
                        if (tail_index < 0) tail_index = 0;
                        if (tail_index >= _fade_frames) tail_index = _fade_frames - 1;
                        gain *= _fade_gain[(int)tail_index];
                    }

                    buf_l_in[i] *= gain;
                    if (channels > 1) buf_r_in[i] *= gain;
                    i++;
                }
            }

            _frames_read += num_reads;

            if (num_reads < IN_BLOCK)
            {
                for (int j = num_reads; j < IN_BLOCK; j++)
                {
                    buf_l_in[j] = 0.0f;
                    buf_r_in[j] = 0.0f;
                }

                playback = false;
                try { r.Close(); } catch { }
            }

            int out_cnt = IN_BLOCK;

            if (!Audio.MOX)
            {
                if (sample_rate != rcvr_rate)
                {
                    fixed (float* in_ptr = &buf_l_in[0], out_ptr = &buf_l_out[0])
                        WDSP.xresampleFV(in_ptr, out_ptr, IN_BLOCK, &out_cnt, rcvr_resamp_l);

                    if (channels > 1)
                    {
                        fixed (float* in_ptr = &buf_r_in[0], out_ptr = &buf_r_out[0])
                            WDSP.xresampleFV(in_ptr, out_ptr, IN_BLOCK, &out_cnt, rcvr_resamp_r);
                    }
                }
                else
                {
                    buf_l_in.CopyTo(buf_l_out, 0);
                    buf_r_in.CopyTo(buf_r_out, 0);
                }
            }
            else
            {
                if (sample_rate != xmtr_rate)
                {
                    fixed (float* in_ptr = &buf_l_in[0], out_ptr = &buf_l_out[0])
                        WDSP.xresampleFV(in_ptr, out_ptr, IN_BLOCK, &out_cnt, xmtr_resamp_l);

                    if (channels > 1)
                    {
                        fixed (float* in_ptr = &buf_r_in[0], out_ptr = &buf_r_out[0])
                            WDSP.xresampleFV(in_ptr, out_ptr, IN_BLOCK, &out_cnt, xmtr_resamp_r);
                    }
                }
                else
                {
                    buf_l_in.CopyTo(buf_l_out, 0);
                    buf_r_in.CopyTo(buf_r_out, 0);
                }
            }

            rb_l.Write(buf_l_out, out_cnt);
            if (channels > 1) rb_r.Write(buf_r_out, out_cnt);
            else rb_r.Write(buf_l_out, out_cnt);

            total_samps_written += out_cnt;
        }

        unsafe public void GetPlayBuffer(float* left, float* right)
        {
            int count = rb_l.ReadSpace();

            if (count == 0) return;

            int size = (!Audio.MOX) ? rcvr_size : xmtr_size;

            if (count > size) count = size;

            rb_l.ReadPtr(left, count);
            rb_r.ReadPtr(right, count);

            if (count < size)
            {
                for (int i = count; i < size; i++)
                {
                    left[i] = 0.0f;
                    right[i] = 0.0f;
                }
            }

            total_samps_read += size;

            if (total_samps_read >= total_samps_written)
            {
                Thread t = new Thread(new ThreadStart(finishPlayback));
                t.Name = "Wave File Finish Playback";
                t.IsBackground = true;
                t.Priority = ThreadPriority.Normal;
                t.Start();
            }
        }

        private void finishPlayback()
        {
            Thread.Sleep(50);
            Action a = _finished;
            if (a != null) a();
        }
    }
}
