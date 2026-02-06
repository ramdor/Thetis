
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Discord;
using NAudio.Wave;

namespace Thetis
{
    public enum AudioRecordRxSource // when receiving, record...
    {
        ReceiverInputIQ = 0, // raw IQ data coming from the hardware
        ReceiverOutputAudio = 1 // demodulated audio you listen to
    }

    public enum AudioRecordTxSource // when transmitting, record...
    {
        MicAudio = 0, // mic audio coming from the hardware, or from the VAC input
        TransmitterOutputIQ = 1 // transmitter IQ that is outbound to the hardware
    }

    public enum AudioBitDepthMode
    {
        IeeeFloat32 = 0,
        Pcm32 = 1, //all signed pcm
        Pcm24 = 2,
        Pcm16 = 3,
        Pcm8 = 4
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
        private readonly object _sync = new object();

        private string _audio_folder;
        private int _wdsp_record_id;
        private int _wdsp_play_id;

        private bool _is_recording;
        private bool _is_playing;

        private WaveInEvent _pc_wave_in;
        private NAudio.Wave.WaveFileWriter _pc_wave_writer;

        private int _active_record_folder_id;
        private string _active_record_filename;

        private int _active_play_folder_id;
        private string _active_play_filename;

        private WaveOutEvent _pc_wave_out;
        private AudioFileReader _pc_audio_reader;

        private int _active_record_wfw_id;
        private int _active_playback_wfw_id;

        private bool _disposed;

        public event Action<bool, int, string> RecordingChanged;
        public event Action<bool, int, string, bool> PlayingChanged;

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

        // general settings
        public bool MoxOnPlayback { get; set; } = true;
        public bool KeepQuickRecords { get; set; } = false;
        public bool GenerateMP3s { get; set; } = false;
        public bool GenerateJSONs { get; set; } = false;
        public string StorageFolder { get; set; } = null;
        public int InputPCDeviceID { get; set; } = -1;
        public int OutputCDeviceID { get; set; } = -1;
        //

        public clsAudioRecordPlayback()
        {
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


        public bool DeleteRecording(int folderId, string filename, out string error)
        {
            error = null;

            try
            {
                if (string.IsNullOrWhiteSpace(filename))
                {
                    error = "Filename is empty.";
                    return false;
                }

                string folder = getFolderPath(folderId);
                string full = Path.Combine(folder, filename);

                if (!File.Exists(full))
                {
                    error = "File does not exist.";
                    return false;
                }

                File.Delete(full);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool DeleteAllRecordings(int folderId, out string error)
        {
            error = null;

            try
            {
                string folder = getFolderPath(folderId);
                if (!Directory.Exists(folder)) return true;

                string[] files = Directory.GetFiles(folder, "*.wav", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    File.Delete(files[i]);
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public string RecordToFileFromWDSP(int folderId, int wfw_id, out string error, string full_file_path = null, bool remove_if_file_exists = false)
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
                    string full_path = "";
                    string filename = "";
                    if (string.IsNullOrEmpty(full_file_path))
                    {                        
                        string folder = getFolderPath(folderId);                        
                        int suffix_count = 0;
                        bool exists = true;
                        while (exists)
                        {
                            filename = generateFilename("wdsp", suffix_count.ToString(), "wav");
                            full_path = Path.Combine(folder, filename);
                            exists = File.Exists(full_path);
                            suffix_count++;
                        }
                    }
                    else
                    {
                        filename = Path.GetFileName(full_file_path);
                        full_path = full_file_path;                        
                    }

                    if(remove_if_file_exists && File.Exists(full_path))
                    {
                        try
                        {
                            File.Delete(full_path);
                        }
                        catch { }
                    }

                    if (!Common.CanCreateFile(full_path))
                    {
                        error = $"Unable to create file\n{full_path}\n\nThis may be due to controlled folder access or some other reason.";
                        return null;
                    }

                    _active_record_wfw_id = wfw_id;
                    _active_record_folder_id = folderId;
                    _active_record_filename = filename;

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

                    bool recRXPreProc = RxSource == AudioRecordRxSource.ReceiverInputIQ;
                    bool recTXPreProc = TxSource == AudioRecordTxSource.MicAudio;

                    WaveThing.wave_file_writer[_wdsp_record_id] = new WaveFileWriter(wfw_id, 2, SampleRate, full_path, recRXPreProc, recTXPreProc, formatTag, bitDepth);
                    WaveThing.wave_file_writer[_wdsp_record_id].DitherEnabled = DitherEnabled;
                    WaveThing.wave_file_writer[_wdsp_record_id].DitherAmount = DitherAmount;                    

                    Audio.WaveRecord = true;
                    setRecordingState(true);

                    return filename;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    try { Audio.WaveRecord = false; } catch { }
                    setRecordingState(false);
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
                if (!_is_recording)
                {
                    return true;
                }

                try
                {
                    Audio.WaveRecord = false;

                    WaveFileWriter writer = WaveThing.wave_file_writer[_wdsp_record_id];
                    if (writer != null)
                    {
                        writer.Stop();
                        WaveThing.wave_file_writer[_wdsp_record_id] = null;
                    }

                    if (_pc_wave_in != null)
                    {
                        try { _pc_wave_in.StopRecording(); } catch { }
                    }

                    setRecordingState(false);
                    return true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    setRecordingState(false);
                    return false;
                }
            }
        }

        public string RecordToFileFromPCAudio(int folderId, int pcAudioDeviceInputId, out string error, string full_file_path = null, bool remove_if_file_exists = false)
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
                    string full_path = "";
                    string filename = "";
                    if (string.IsNullOrEmpty(full_file_path))
                    {
                        string folder = getFolderPath(folderId);
                        int suffix_count = 0;
                        bool exists = true;
                        while (exists)
                        {
                            filename = generateFilename("pc", suffix_count.ToString(), "wav");
                            full_path = Path.Combine(folder, filename);
                            exists = File.Exists(full_path);
                            suffix_count++;
                        }
                    }
                    else
                    {
                        filename = Path.GetFileName(full_file_path);
                        full_path = full_file_path;
                    }

                    if (remove_if_file_exists && File.Exists(full_path))
                    {
                        try
                        {
                            File.Delete(full_path);
                        }
                        catch { }
                    }

                    if (!Common.CanCreateFile(full_path))
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

                    _pc_wave_in = new WaveInEvent();
                    _pc_wave_in.DeviceNumber = pcAudioDeviceInputId;
                    _pc_wave_in.WaveFormat = fmt;
                    _pc_wave_in.BufferMilliseconds = 50;
                    _pc_wave_in.NumberOfBuffers = 3;
                    _pc_wave_in.DataAvailable += onPcDataAvailable;
                    _pc_wave_in.RecordingStopped += onPcRecordingStopped;

                    _pc_wave_writer = new NAudio.Wave.WaveFileWriter(full_path, fmt);

                    _pc_wave_in.StartRecording();
                    setRecordingState(true);

                    return filename;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    cleanupPcRecording();
                    setRecordingState(false);
                    return null;
                }
            }
        }

        public bool PlayFileViaWDSP(int folderId, string filename, int wfw_id, out string error, string full_file_path = null)
        {
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

                if (string.IsNullOrWhiteSpace(filename) && string.IsNullOrEmpty(full_file_path))
                {
                    error = "Filename is empty.";
                    return false;
                }

                try
                {
                    string folder = getFolderPath(folderId);
                    string fullPath = string.IsNullOrEmpty(full_file_path) ? Path.Combine(folder, filename) : full_file_path;

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

                    if (!tryParseWaveHeader(reader, out int formatTag, out int sampleRate, out int channels, out int bitsPerSample, out long dataStart, out string headerError))
                    {
                        try { reader.Close(); } catch { }
                        error = headerError;
                        return false;
                    }

                    _active_playback_wfw_id = wfw_id;
                    _active_play_folder_id = folderId;
                    _active_play_filename = filename;
                    _is_wdsp_playing = true;                    

                    WaveThing.wave_file_reader[_active_playback_wfw_id] = new WaveFileReader1(
                        _wdsp_play_id,
                        formatTag,
                        sampleRate,
                        channels,
                        bitsPerSample,
                        reader,
                        onWdspPlaybackFinished);

                    Audio.WavePlayback = true;
                    setPlayingState(true);

                    return true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    try { Audio.WavePlayback = false; } catch { }
                    setPlayingState(false);
                    return false;
                }
            }
        }

        public bool PlayFileViaPCAudio(int folderId, string filename, int pcAudioDeviceOutputId, out string error, string full_file_path = null)
        {
            error = null;

            lock (_sync)
            {
                if (_is_playing)
                {
                    error = "Already playing.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(filename) && string.IsNullOrWhiteSpace(full_file_path))
                {
                    error = "Filename is empty.";
                    return false;
                }

                try
                {
                    string folder = getFolderPath(folderId);
                    string fullPath = string.IsNullOrEmpty(full_file_path) ? Path.Combine(folder, filename) : full_file_path;

                    if (!File.Exists(fullPath))
                    {
                        error = "File does not exist.";
                        return false;
                    }

                    _active_play_folder_id = folderId;
                    _active_play_filename = filename;
                    _is_wdsp_playing = false;

                    _pc_audio_reader = new AudioFileReader(fullPath);
                    _pc_audio_reader.Volume = _pc_playback_gain;
                    _pc_wave_out = new WaveOutEvent();
                    _pc_wave_out.DeviceNumber = pcAudioDeviceOutputId;
                    _pc_wave_out.PlaybackStopped += pcWaveOut_PlaybackStopped;
                    _pc_wave_out.Init(_pc_audio_reader);
                    _pc_wave_out.Play();

                    setPlayingState(true);

                    return true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    try { cleanupPcPlayback(); } catch { }
                    setPlayingState(false);
                    return false;
                }
            }
        }

        private void pcWaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            string error;
            StopPlayback(out error);
        }

        void cleanupPcPlayback()
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

        public bool StopPlayback(out string error)
        {
            error = null;

            lock (_sync)
            {
                if (!_is_playing) return true;

                try
                {
                    if (_active_playback_wfw_id >= 0)
                    {
                        //playing via wdsp
                        Audio.WavePlayback = false;

                        WaveFileReader1 reader = WaveThing.wave_file_reader[_active_playback_wfw_id];
                        if (reader != null)
                        {
                            reader.Stop();
                            WaveThing.wave_file_reader[_active_playback_wfw_id] = null;
                        }

                        _active_playback_wfw_id = -1;
                    }

                    cleanupPcPlayback();
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    try { cleanupPcPlayback(); } catch { }
                }
            }

            setPlayingState(false);
            return error == null;
        }

        void onPcDataAvailable(object sender, WaveInEventArgs e)
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
            lock (_sync)
            {
                cleanupPcRecording();
            }
            setRecordingState(false);
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
            //lock (_sync)
            //{
            //    if (_active_playback_wfw_id < 0) return;

            //    try
            //    {
            //        Audio.WavePlayback = false;
            //    }
            //    catch { }

            //    try
            //    {
            //        WaveThing.wave_file_reader[_active_playback_wfw_id] = null;
            //    }
            //    catch { }

            //    _active_playback_wfw_id = -1;
            //}

            //setPlayingState(false);
        }

        private void setRecordingState(bool recording)
        {
            bool raise = false;
            int folderId = 0;
            string filename = null;

            lock (_sync)
            {
                if (_is_recording != recording)
                {
                    _is_recording = recording;
                    folderId = _active_record_folder_id;
                    filename = _active_record_filename;
                    raise = true;

                    if (!recording)
                    {
                        _active_record_folder_id = 0;
                        _active_record_filename = null;
                        _active_record_wfw_id = -1;
                    }
                }
            }

            if (raise)
            {
                Action<bool, int, string> h = RecordingChanged;
                if (h != null)
                {
                    Delegate[] list = h.GetInvocationList();
                    for (int i = 0; i < list.Length; i++)
                    {
                        try
                        {
                            ((Action<bool, int, string>)list[i])(recording, folderId, filename);
                        }
                        catch { }
                    }
                }
            }
        }

        private void setPlayingState(bool playing)
        {
            bool raise = false;
            int folderId = 0;
            string filename = null;
            bool isWdsp = false;

            lock (_sync)
            {
                if (_is_playing != playing)
                {
                    _is_playing = playing;
                    folderId = _active_play_folder_id;
                    filename = _active_play_filename;
                    isWdsp = _is_wdsp_playing;
                    raise = true;

                    if (!playing)
                    {
                        _active_play_folder_id = 0;
                        _active_play_filename = null;
                        _is_wdsp_playing = false;
                        _active_playback_wfw_id = -1;
                    }
                }
            }

            if (raise)
            {
                Action<bool, int, string, bool> h = PlayingChanged;
                if (h != null)
                {
                    Delegate[] list = h.GetInvocationList();
                    for (int i = 0; i < list.Length; i++)
                    {
                        try
                        {
                            ((Action<bool, int, string, bool>)list[i])(playing, folderId, filename, isWdsp);
                        }
                        catch { }
                    }
                }
            }
        }

        private static string generateFilename(string prefix, string suffix, string ext)
        {
            DateTime now = DateTime.Now;
            string stamp = now.ToString("yyyyMMdd_HHmmss");
            return prefix + "_" + stamp + "_" + suffix + "." + ext;
        }

        private string getFolderPath(int folderId)
        {
            if (folderId < 0) folderId = 0;

            string folder = Path.Combine(_audio_folder, folderId.ToString());
            ensureFolderExists(folder);
            return folder;
        }

        private static void ensureFolderExists(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) return;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        }

        private static bool tryParseWaveHeader(BinaryReader reader, out int formatTag, out int sampleRate, out int channels, out int bitsPerSample, out long dataStart, out string error)
        {
            formatTag = 0;
            sampleRate = 0;
            channels = 0;
            bitsPerSample = 0;
            dataStart = 0;
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

        public WaveFileWriter(int wfw_id, short chan, int samp_rate, string file, bool recordRxPreProcessed, bool recordTxPreProcessed, short formatTag, short bitDepth)
        {
            _id = wfw_id;

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
            if (_id == 0) _mox = Audio.MOX && (Audio.console.VFOATX || (Audio.console.VFOBTX && !Audio.console.RX2Enabled));
            else if (_id == 1) _mox = Audio.MOX && Audio.console.RX2Enabled && Audio.console.VFOBTX;
            else _mox = Audio.MOX;
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

            _writer.Seek(0, SeekOrigin.Begin);
            WriteWaveHeader(ref _writer, _channels, _sample_rate, _format_tag, _bit_depth, _length_counter);
            _writer.Flush();
            _writer.Close();
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

        public WaveFileReader1(int wfr_id, int fmt, int samp_rate, int chan, int bit_depth, BinaryReader binread, Action finished)
        {
            id = wfr_id;
            format = fmt;
            sample_rate = samp_rate;
            channels = chan;
            bitdepth = bit_depth;
            reader = binread;
            _finished = finished;

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
