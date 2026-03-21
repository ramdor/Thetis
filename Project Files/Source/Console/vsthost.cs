using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thetis
{
    enum VstChainKind
    {
        Rx = 0,
        Tx = 1
    }

    enum VstPluginLoadState
    {
        Failed = -1,
        None = 0,
        DescriptorOnly = 1,
        Active = 2
    }

    enum VstPluginFormat
    {
        Unknown = 0,
        Vst3 = 1,
        Vst2 = 2
    }

    enum VstHostState
    {
        Disabled = 0,
        Starting = 1,
        Running = 2,
        Unavailable = 3,
        Crashed = 4,
        Restarting = 5
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct VstPluginInfoNative
    {
        public int Index;
        public int Enabled;
        public int Bypass;
        public int LoadState;
        public int Format;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string Path;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct VstPluginProbeInfoNative
    {
        public int IsValid;
        public int IsAudioEffect;
        public int HasAudioInput;
        public int HasAudioOutput;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string Path;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Vendor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Version;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Subcategories;
    }

    sealed class VstPluginState
    {
        public string Path { get; set; }
        public bool Enabled { get; set; }
        public bool Bypass { get; set; }
        public string Name { get; set; }
        public VstPluginLoadState LoadState { get; set; }
        public VstPluginFormat Format { get; set; }
        public byte[] StateData { get; set; }
    }

    sealed class VstChainState
    {
        public bool Bypass { get; set; }
        public double Gain { get; set; }
        public int LatencyFloor { get; set; }
        public List<VstPluginState> Plugins { get; set; }

        public VstChainState()
        {
            Bypass = true;
            Gain = 1.0;
            LatencyFloor = 0; // 0 = use native default
            Plugins = new List<VstPluginState>();
        }
    }

    sealed class VstChainInfo
    {
        public VstChainKind Kind { get; set; }
        public VstHostState HostState { get; set; }
        public bool Ready { get; set; }
        public bool Bypass { get; set; }
        public double Gain { get; set; }
        public int SampleRate { get; set; }
        public int BlockSize { get; set; }
        public int LatencyBlocks { get; set; }
        public int LatencyFloorBlocks { get; set; }
        public List<VstPluginState> Plugins { get; set; }

        public VstChainInfo()
        {
            Gain = 1.0;
            Plugins = new List<VstPluginState>();
        }
    }

    sealed class VstOperationResult
    {
        public bool Success { get; set; }
        public bool HasWarning { get; set; }
        public string Message { get; set; }
        public int PluginIndex { get; set; }
        public VstPluginState Plugin { get; set; }

        public VstOperationResult()
        {
            PluginIndex = -1;
        }
    }

    sealed class VstCatalogPlugin
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Vendor { get; set; }
        public string Version { get; set; }
        public string Subcategories { get; set; }
        public bool HasAudioInput { get; set; }
        public bool HasAudioOutput { get; set; }
        public bool Available { get; set; }
        public string Status { get; set; }
        public string ErrorDetail { get; set; }
        public string LastModifiedUtc { get; set; }
    }

    sealed class VstPluginCatalogFile
    {
        public int Version { get; set; }
        public string LastScanUtc { get; set; }
        public List<string> SearchPaths { get; set; }
        public List<VstCatalogPlugin> Plugins { get; set; }

        public VstPluginCatalogFile()
        {
            Version = 1;
            SearchPaths = new List<string>();
            Plugins = new List<VstCatalogPlugin>();
        }
    }

    sealed class VstCatalogScanUpdate
    {
        public string Message { get; set; }
        public VstCatalogPlugin Plugin { get; set; }
    }

    sealed class VstPluginScannerProbeResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int ProbeResultCode { get; set; }
        public int IsValid { get; set; }
        public int IsAudioEffect { get; set; }
        public int HasAudioInput { get; set; }
        public int HasAudioOutput { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Vendor { get; set; }
        public string Version { get; set; }
        public string Subcategories { get; set; }
    }

    sealed class ModuleInfoFile
    {
        public string Name { get; set; }
        public string Version { get; set; }

        [JsonProperty("Factory Info")]
        public ModuleInfoFactoryInfo FactoryInfo { get; set; }

        public List<ModuleInfoClassEntry> Classes { get; set; }
    }

    sealed class ModuleInfoFactoryInfo
    {
        public string Vendor { get; set; }
    }

    sealed class ModuleInfoClassEntry
    {
        public string CID { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Vendor { get; set; }
        public string Version { get; set; }

        [JsonProperty("Sub Categories")]
        public List<string> SubCategories { get; set; }
    }

    sealed class VstStateFile
    {
        public int Version { get; set; }
        public bool SdkAvailable { get; set; }
        public VstChainState Rx { get; set; }
        public VstChainState Tx { get; set; }

        public VstStateFile()
        {
            Version = 3;
            Rx = new VstChainState();
            Tx = new VstChainState();
        }
    }

    static class VstHost
    {
        private const string NativeLibrary = "VstHostBridge.dll";
        private const string ScannerHelperExeName = "VstPluginScanner.exe";
        private const string StateFileName = "vst_chains.json";
        private const string CatalogFileName = "vst_plugin_catalog.json";
        private const string ScanLogFileName = "vst_scan_log.txt";
        private const int DeferredStateSaveDelayMs = 750;
        private const int StructuralStateSaveDelayMs = 1000;
        private const int RetryStateSaveDelayMs = 750;
        private const int PluginProbeTimeoutMs = 15000;
        private const string BundleArchitectureDir = "x86_64-win";
        private const string BundleContentsDir = "Contents";
        private const string BundleResourcesDir = "Resources";
        private const string ModuleInfoFileName = "moduleinfo.json";
        private const string AudioModuleClassCategory = "Audio Module Class";
        private static bool _nativeUnavailable;
        private static bool _nativeCallbackRegistered;
        private static string _stateAppDataPath;
        private static readonly object _stateSaveLock = new object();
        private static Timer _stateSaveTimer;
        private static volatile string _lastPersistenceError;
        private static readonly NativeStateChangedCallback _nativeStateChangedCallback = OnNativeStateChanged;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativeStateChangedCallback(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetSdkAvailable", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetSdkAvailable();

        [DllImport(NativeLibrary, EntryPoint = "VST_ProbePluginMetadataOnly", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int NativeProbePlugin(string pluginPath, out VstPluginProbeInfoNative info);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetChainBypass", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetChainBypass(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_SetChainBypass", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeSetChainBypass(VstChainKind kind, int bypass);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetChainGain", CallingConvention = CallingConvention.Cdecl)]
        private static extern double NativeGetChainGain(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_SetChainGain", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeSetChainGain(VstChainKind kind, double gain);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetChainReady", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetChainReady(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetPipelineLatency", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetPipelineLatency(VstChainKind kind, out int currentBlocks, out int floorBlocks);

        [DllImport(NativeLibrary, EntryPoint = "VST_SetPipelineLatencyFloor", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeSetPipelineLatencyFloor(VstChainKind kind, int floorBlocks);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetSampleRate", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetSampleRate(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetBlockSize", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetBlockSize(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetPluginCount", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetPluginCount(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetPluginInfo", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int NativeGetPluginInfo(VstChainKind kind, int index, out VstPluginInfoNative info);

        [DllImport(NativeLibrary, EntryPoint = "VST_ClearChain", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeClearChain(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_AddPlugin", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int NativeAddPlugin(VstChainKind kind, string pluginPath);

        [DllImport(NativeLibrary, EntryPoint = "VST_RemovePlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeRemovePlugin(VstChainKind kind, int index);

        [DllImport(NativeLibrary, EntryPoint = "VST_MovePlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeMovePlugin(VstChainKind kind, int fromIndex, int toIndex);

        [DllImport(NativeLibrary, EntryPoint = "VST_SetPluginBypass", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeSetPluginBypass(VstChainKind kind, int index, int bypass);

        [DllImport(NativeLibrary, EntryPoint = "VST_SetPluginEnabled", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeSetPluginEnabled(VstChainKind kind, int index, int enabled);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetPluginStateSize", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetPluginStateSize(VstChainKind kind, int index);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetPluginState", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetPluginState(VstChainKind kind, int index, byte[] buffer, int bufferSize, out int bytesWritten);

        [DllImport(NativeLibrary, EntryPoint = "VST_SetPluginState", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeSetPluginState(VstChainKind kind, int index, byte[] buffer, int bufferSize);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetHostState", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetHostState(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_GetChainSnapshotGeneration", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeGetChainSnapshotGeneration(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_RequestHostChainSync", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeRequestHostChainSync(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_SetStateChangedCallback", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeSetStateChangedCallback(NativeStateChangedCallback callback);

        [DllImport(NativeLibrary, EntryPoint = "VST_OpenPluginEditorWindow", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeOpenPluginEditorWindow(VstChainKind kind, int index);

        [DllImport(NativeLibrary, EntryPoint = "VST_Initialize", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeInitialize();

        [DllImport(NativeLibrary, EntryPoint = "VST_Shutdown", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeShutdown();

        [DllImport(NativeLibrary, EntryPoint = "VST_CreateChain", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeCreateChain(VstChainKind kind, int sampleRate, int maxBlockSize, int numChannels);

        [DllImport(NativeLibrary, EntryPoint = "VST_DestroyChain", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NativeDestroyChain(VstChainKind kind);

        [DllImport(NativeLibrary, EntryPoint = "VST_ProcessInterleavedDouble", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int NativeProcessInterleavedDouble(VstChainKind kind, double* buffer, int frames);

        public static void Initialize()
        {
            NativeInitialize();
        }

        public static void Shutdown()
        {
            NativeShutdown();
        }

        public static void CreateRxChain(int sampleRate, int blockSize)
        {
            NativeCreateChain(VstChainKind.Rx, sampleRate, blockSize, 2);
        }

        public static void CreateTxChain(int sampleRate, int blockSize)
        {
            NativeCreateChain(VstChainKind.Tx, sampleRate, blockSize, 2);
        }

        public static void DestroyRxChain()
        {
            NativeDestroyChain(VstChainKind.Rx);
        }

        public static void DestroyTxChain()
        {
            NativeDestroyChain(VstChainKind.Tx);
        }

        public static unsafe void ProcessRxAudio(double* buffer, int frames)
        {
            NativeProcessInterleavedDouble(VstChainKind.Rx, buffer, frames);
        }

        public static unsafe void ProcessTxAudio(double* buffer, int frames)
        {
            NativeProcessInterleavedDouble(VstChainKind.Tx, buffer, frames);
        }

        public static bool NativeAvailable
        {
            get { return EnsureNativeAvailable(); }
        }

        public static bool SdkAvailable
        {
            get
            {
                if (!EnsureNativeAvailable()) return false;
                return NativeGetSdkAvailable() != 0;
            }
        }

        public static string NativeStatusText
        {
            get
            {
                if (!NativeAvailable)
                    return "Native VST bridge is unavailable.";

                if (!SdkAvailable)
                    return "Native VST bridge loaded, but VST3 hosting is unavailable.";

                return "VST3 hosting is available.";
            }
        }

        public static string PersistenceStatusText
        {
            get { return _lastPersistenceError ?? string.Empty; }
        }

        public static VstHostState GetHostState(VstChainKind kind)
        {
            if (!EnsureNativeAvailable())
                return VstHostState.Unavailable;

            return (VstHostState)NativeGetHostState(kind);
        }

        public static string GetHostStateDisplayName(VstHostState state)
        {
            switch (state)
            {
                case VstHostState.Disabled:
                    return "Host disabled";
                case VstHostState.Starting:
                    return "Host starting";
                case VstHostState.Running:
                    return "Host running";
                case VstHostState.Unavailable:
                    return "Host unavailable";
                case VstHostState.Crashed:
                    return "Host crashed";
                case VstHostState.Restarting:
                    return "Host restarting";
                default:
                    return "Host unknown";
            }
        }

        public static List<string> GetDefaultPluginSearchPaths()
        {
            List<string> paths = new List<string>();

            AddSearchPathIfPresent(paths, Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Common Files",
                "VST3"));
            AddSearchPathIfPresent(paths, Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Common Files",
                "Steinberg",
                "VST3"));
            AddSearchPathIfPresent(paths, Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs",
                "Common",
                "VST3"));
            return paths;
        }

        public static string GetChainDisplayName(VstChainKind kind)
        {
            return kind == VstChainKind.Rx ? "RX" : "TX";
        }

        public static string GetLoadStateDisplayName(VstPluginLoadState state)
        {
            switch (state)
            {
                case VstPluginLoadState.Active:
                    return "Loaded";
                case VstPluginLoadState.DescriptorOnly:
                    return "Descriptor only";
                case VstPluginLoadState.Failed:
                    return "Failed";
                default:
                    return "Not loaded";
            }
        }

        public static string GetPluginDisplayName(VstPluginState plugin)
        {
            if (plugin == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(plugin.Name))
                return plugin.Name;

            if (string.IsNullOrWhiteSpace(plugin.Path))
                return "(unnamed plugin)";

            string normalizedPath = NormalizePluginPath(plugin.Path);
            string fileName = Path.GetFileNameWithoutExtension(normalizedPath);
            return string.IsNullOrWhiteSpace(fileName) ? normalizedPath : fileName;
        }

        public static VstChainInfo GetChainInfo(VstChainKind kind)
        {
            if (!EnsureNativeAvailable())
                return new VstChainInfo { Kind = kind };

            return CaptureChainInfo(kind, false, false);
        }

        public static bool GetChainReady(VstChainKind kind)
        {
            if (!EnsureNativeAvailable())
                return false;

            return NativeGetChainReady(kind) != 0;
        }

        public static void LoadState(string appDataPath)
        {
            string stateFilePath;
            VstStateFile state;

            if (!EnsureNativeAvailable()) return;

            RememberAppDataPath(appDataPath);
            stateFilePath = GetStateFilePath(appDataPath);
            if (!File.Exists(stateFilePath)) return;

            try
            {
                state = JsonConvert.DeserializeObject<VstStateFile>(File.ReadAllText(stateFilePath));
                if (state == null) return;

                ApplyChainState(VstChainKind.Rx, state.Rx);
                ApplyChainState(VstChainKind.Tx, state.Tx);
                ClearPersistenceError();
            }
            catch (Exception ex)
            {
                SetPersistenceError("VST state load failed.", ex);
            }
        }

        public static void SaveState(string appDataPath)
        {
            if (!EnsureNativeAvailable()) return;

            RememberAppDataPath(appDataPath);
            lock (_stateSaveLock)
            {
                if (_stateSaveTimer != null)
                {
                    _stateSaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _stateSaveTimer.Dispose();
                    _stateSaveTimer = null;
                }
                SaveStateCore(appDataPath, false);
            }
        }

        public static bool SetChainBypass(VstChainKind kind, bool bypass)
        {
            if (!EnsureNativeAvailable()) return false;

            NativeSetChainBypass(kind, bypass ? 1 : 0);
            ScheduleStateSave(DeferredStateSaveDelayMs);
            return true;
        }

        public static bool SetChainGain(VstChainKind kind, double gain)
        {
            if (!EnsureNativeAvailable()) return false;

            NativeSetChainGain(kind, Math.Max(0.0, gain));
            ScheduleStateSave(DeferredStateSaveDelayMs);
            return true;
        }

        public static bool GetPipelineLatency(VstChainKind kind, out int currentBlocks, out int floorBlocks, out int sampleRate, out int blockSize)
        {
            currentBlocks = 0;
            floorBlocks = 0;
            sampleRate = 0;
            blockSize = 0;

            if (!EnsureNativeAvailable()) return false;

            NativeGetPipelineLatency(kind, out currentBlocks, out floorBlocks);
            sampleRate = NativeGetSampleRate(kind);
            blockSize = NativeGetBlockSize(kind);
            return true;
        }

        public static bool SetPipelineLatencyFloor(VstChainKind kind, int floorBlocks)
        {
            if (!EnsureNativeAvailable()) return false;

            NativeSetPipelineLatencyFloor(kind, Math.Max(1, floorBlocks));
            ScheduleStateSave(DeferredStateSaveDelayMs);
            return true;
        }

        public static VstOperationResult AddPlugin(VstChainKind kind, string pluginPath)
        {
            string normalizedPath;
            VstPluginFormat format;
            int index;

            if (!EnsureNativeAvailable())
            {
                return new VstOperationResult
                {
                    Success = false,
                    Message = "Native VST bridge is unavailable."
                };
            }

            normalizedPath = NormalizePluginPath(pluginPath);
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return new VstOperationResult
                {
                    Success = false,
                    Message = "Select a plugin path first."
                };
            }

            format = DetectPluginFormat(normalizedPath);
            if (format == VstPluginFormat.Unknown)
            {
                return new VstOperationResult
                {
                    Success = false,
                    Message = "Only .vst3 plugin files or bundle paths and .dll VST2 plugin files are supported."
                };
            }

            if (!PluginPathExists(normalizedPath))
            {
                return new VstOperationResult
                {
                    Success = false,
                    Message = format == VstPluginFormat.Vst2
                        ? "The selected VST2 plugin file does not exist."
                        : "The selected VST3 plugin path does not exist."
                };
            }

            if (format == VstPluginFormat.Vst2)
            {
                string probeError = ProbeVst2Plugin(normalizedPath);
                if (probeError != null)
                {
                    return new VstOperationResult
                    {
                        Success = false,
                        Message = probeError
                    };
                }
            }

            index = NativeAddPlugin(kind, normalizedPath);
            if (index < 0)
            {
                return new VstOperationResult
                {
                    Success = false,
                    Message = "The native host rejected the plugin path."
                };
            }

            VstOperationResult result = BuildAddPluginResult(kind, index);
            if (result.Success)
                QueueStructuralStateSave();

            return result;
        }

        public static bool RemovePlugin(VstChainKind kind, int index)
        {
            if (!EnsureNativeAvailable()) return false;
            if (NativeRemovePlugin(kind, index) != 0)
                return false;

            QueueStructuralStateSave();
            return true;
        }

        public static bool MovePlugin(VstChainKind kind, int fromIndex, int toIndex)
        {
            if (!EnsureNativeAvailable()) return false;
            if (NativeMovePlugin(kind, fromIndex, toIndex) != 0)
                return false;

            QueueStructuralStateSave();
            return true;
        }

        public static bool SetPluginBypass(VstChainKind kind, int index, bool bypass)
        {
            if (!EnsureNativeAvailable()) return false;
            if (NativeSetPluginBypass(kind, index, bypass ? 1 : 0) != 0)
                return false;

            ScheduleStateSave(DeferredStateSaveDelayMs);
            return true;
        }

        public static bool SetPluginEnabled(VstChainKind kind, int index, bool enabled)
        {
            if (!EnsureNativeAvailable()) return false;
            if (NativeSetPluginEnabled(kind, index, enabled ? 1 : 0) != 0)
                return false;

            ScheduleStateSave(DeferredStateSaveDelayMs);
            return true;
        }

        public static bool OpenPluginEditorWindow(VstChainKind kind, int index)
        {
            if (!EnsureNativeAvailable()) return false;

            return NativeOpenPluginEditorWindow(kind, index) != 0;
        }

        public static VstPluginCatalogFile LoadPluginCatalog()
        {
            string catalogFilePath;
            VstPluginCatalogFile catalog;

            catalog = new VstPluginCatalogFile();
            catalog.SearchPaths.AddRange(GetDefaultPluginSearchPaths());

            catalogFilePath = GetCatalogFilePath(GetCurrentAppDataPath());
            if (string.IsNullOrWhiteSpace(catalogFilePath) || !File.Exists(catalogFilePath))
                return NormalizeCatalog(catalog);

            try
            {
                VstPluginCatalogFile loadedCatalog = JsonConvert.DeserializeObject<VstPluginCatalogFile>(File.ReadAllText(catalogFilePath));
                if (loadedCatalog != null)
                    catalog = loadedCatalog;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("VST plugin catalog load failed. " + ex);
            }

            return NormalizeCatalog(catalog);
        }

        public static void SavePluginCatalog(VstPluginCatalogFile catalog)
        {
            string appDataPath = GetCurrentAppDataPath();
            string catalogFilePath;
            string directoryPath;

            if (string.IsNullOrWhiteSpace(appDataPath))
                return;

            catalog = NormalizeCatalog(catalog);
            catalogFilePath = GetCatalogFilePath(appDataPath);
            directoryPath = Path.GetDirectoryName(catalogFilePath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
                Directory.CreateDirectory(directoryPath);

            WriteTextFileAtomically(catalogFilePath, JsonConvert.SerializeObject(catalog, Formatting.Indented));
        }

        private static void DeleteScanLog()
        {
            try
            {
                string appDataPath = GetCurrentAppDataPath();
                if (string.IsNullOrWhiteSpace(appDataPath))
                    return;

                string vstDir = GetVstSubdirectory(appDataPath);
                string logPath = Path.Combine(vstDir, ScanLogFileName);
                if (File.Exists(logPath))
                    File.Delete(logPath);

                // Also clean up old location
                string oldLogPath = Path.Combine(appDataPath, ScanLogFileName);
                if (File.Exists(oldLogPath))
                    File.Delete(oldLogPath);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("VST scan log delete failed. " + ex.Message);
            }
        }

        private static void WriteScanLog(VstPluginCatalogFile catalog)
        {
            try
            {
                string appDataPath = GetCurrentAppDataPath();
                if (string.IsNullOrWhiteSpace(appDataPath))
                    return;

                string vstDir = GetVstSubdirectory(appDataPath);
                if (!Directory.Exists(vstDir))
                    Directory.CreateDirectory(vstDir);
                MigrateFileToVstSubdirectory(appDataPath, vstDir, ScanLogFileName);
                string logPath = Path.Combine(vstDir, ScanLogFileName);
                using (StreamWriter writer = new StreamWriter(logPath, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("VST Plugin Scan Log");
                    writer.WriteLine("Generated: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    writer.WriteLine("========================================");
                    writer.WriteLine();

                    if (catalog == null || catalog.Plugins == null || catalog.Plugins.Count == 0)
                    {
                        writer.WriteLine("No plugins found.");
                        return;
                    }

                    int availableCount = 0;
                    int unavailableCount = 0;
                    List<VstCatalogPlugin> failedPlugins = new List<VstCatalogPlugin>();

                    foreach (VstCatalogPlugin plugin in catalog.Plugins)
                    {
                        if (plugin == null) continue;
                        if (plugin.Available)
                            availableCount++;
                        else
                        {
                            unavailableCount++;
                            failedPlugins.Add(plugin);
                        }
                    }

                    writer.WriteLine("Summary: {0} available, {1} unavailable, {2} total",
                        availableCount, unavailableCount, catalog.Plugins.Count);
                    writer.WriteLine();

                    if (failedPlugins.Count > 0)
                    {
                        writer.WriteLine("--- FAILED PLUGINS ---");
                        writer.WriteLine();
                        foreach (VstCatalogPlugin plugin in failedPlugins)
                        {
                            writer.WriteLine("  Name:   {0}", plugin.Name ?? "(unknown)");
                            writer.WriteLine("  Path:   {0}", plugin.Path ?? "(unknown)");
                            writer.WriteLine("  Status: {0}", plugin.Status ?? "Unavailable");
                            if (!string.IsNullOrWhiteSpace(plugin.ErrorDetail))
                                writer.WriteLine("  Detail: {0}", plugin.ErrorDetail);
                            if (!string.IsNullOrWhiteSpace(plugin.Vendor))
                                writer.WriteLine("  Vendor: {0}", plugin.Vendor);
                            writer.WriteLine();
                        }
                    }

                    writer.WriteLine("--- ALL PLUGINS ---");
                    writer.WriteLine();
                    foreach (VstCatalogPlugin plugin in catalog.Plugins)
                    {
                        if (plugin == null) continue;
                        writer.WriteLine("  [{0}] {1}",
                            plugin.Available ? "OK" : "FAIL",
                            plugin.Name ?? "(unknown)");
                        writer.WriteLine("    Path: {0}", plugin.Path ?? "(unknown)");
                        if (!plugin.Available)
                        {
                            writer.WriteLine("    Status: {0}", plugin.Status ?? "Unavailable");
                            if (!string.IsNullOrWhiteSpace(plugin.ErrorDetail))
                                writer.WriteLine("    Detail: {0}", plugin.ErrorDetail);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("VST scan log write failed. " + ex);
            }
        }

        public static VstPluginCatalogFile ScanPluginCatalog(IEnumerable<string> searchPaths, IProgress<VstCatalogScanUpdate> progress, bool forceRescanAll, CancellationToken cancellationToken)
        {
            VstPluginCatalogFile catalog = new VstPluginCatalogFile();
            VstPluginCatalogFile previousCatalog = forceRescanAll ? new VstPluginCatalogFile() : LoadPluginCatalog();
            ConcurrentBag<VstCatalogPlugin> scannedPlugins = new ConcurrentBag<VstCatalogPlugin>();
            Dictionary<string, VstCatalogPlugin> cachedPluginsByPath = BuildCatalogPluginMap(previousCatalog);
            HashSet<string> candidatePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> orderedCandidatePaths;
            int discoveredCount = 0;
            int reusedCount = 0;
            int unavailableCount = 0;
            int workerCount;

            if (!EnsureNativeAvailable() || !SdkAvailable)
                return NormalizeCatalog(catalog);

            cancellationToken.ThrowIfCancellationRequested();
            DeleteScanLog();

            if (searchPaths != null)
            {
                foreach (string path in searchPaths)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    AddSearchPathIfPresent(catalog.SearchPaths, path);
                }
            }

            if (catalog.SearchPaths.Count == 0)
                catalog.SearchPaths.AddRange(GetDefaultPluginSearchPaths());

            foreach (string searchPath in catalog.SearchPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ReportScanProgress(progress, "Scanning path: " + searchPath, null);
                CollectPluginCandidates(searchPath, candidatePaths);
            }

            orderedCandidatePaths = new List<string>(candidatePaths);
            orderedCandidatePaths.Sort(StringComparer.OrdinalIgnoreCase);
            workerCount = 1;
            if (forceRescanAll)
                ReportScanProgress(progress, "Full rescan requested. Existing plugin cache entries will be rebuilt.", null);

            ReportScanProgress(progress, string.Format("Scanning {0} candidate plugin path{1} with {2} worker thread{3}.",
                orderedCandidatePaths.Count,
                orderedCandidatePaths.Count == 1 ? string.Empty : "s",
                workerCount,
                workerCount == 1 ? string.Empty : "s"), null);

            Parallel.ForEach(
                orderedCandidatePaths,
                new ParallelOptions { MaxDegreeOfParallelism = workerCount, CancellationToken = cancellationToken },
                delegate (string candidatePath)
                {
                    try
                    {
                        VstCatalogPlugin plugin;
                        string lastModifiedUtc = GetCatalogPluginLastModifiedUtc(candidatePath);
                        VstCatalogPlugin cachedPlugin;

                        cancellationToken.ThrowIfCancellationRequested();
                        ReportScanProgress(progress, "Scanning plugin: " + candidatePath, null);
                        if (!forceRescanAll && TryGetReusableCatalogPlugin(cachedPluginsByPath, candidatePath, lastModifiedUtc, out cachedPlugin))
                        {
                            plugin = CloneCatalogPlugin(cachedPlugin);
                            scannedPlugins.Add(plugin);
                            if (plugin.Available)
                                Interlocked.Increment(ref discoveredCount);
                            else
                                Interlocked.Increment(ref unavailableCount);
                            Interlocked.Increment(ref reusedCount);
                            ReportScanProgress(progress, "Using cached entry: " + GetCatalogPluginDisplayName(plugin), CloneCatalogPlugin(plugin));
                            return;
                        }

                        plugin = ProbeCatalogPlugin(candidatePath);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (plugin != null)
                        {
                            plugin.LastModifiedUtc = lastModifiedUtc;
                            scannedPlugins.Add(plugin);
                        if (plugin.Available)
                        {
                            int foundCount = Interlocked.Increment(ref discoveredCount);
                            ReportScanProgress(progress, string.Format("Found {0}: {1}", foundCount, GetCatalogPluginDisplayName(plugin)), CloneCatalogPlugin(plugin));
                            ReportScanProgress(progress, "Adding to catalog: " + GetCatalogPluginDisplayName(plugin), null);
                        }
                            else
                            {
                                Interlocked.Increment(ref unavailableCount);
                                string unavailableMessage = string.Format("Marked unavailable: {0} ({1})", GetCatalogPluginDisplayName(plugin), plugin.Status ?? "Unavailable");
                                if (!string.IsNullOrWhiteSpace(plugin.ErrorDetail))
                                    unavailableMessage += " - " + plugin.ErrorDetail;
                                ReportScanProgress(progress, unavailableMessage, CloneCatalogPlugin(plugin));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        VstCatalogPlugin failedPlugin = new VstCatalogPlugin
                        {
                            Path = candidatePath,
                            Name = Path.GetFileNameWithoutExtension(NormalizePluginPath(candidatePath)),
                            Available = false,
                            Status = "Scanner error",
                            ErrorDetail = SummarizeProbeError(ex.Message),
                            LastModifiedUtc = GetCatalogPluginLastModifiedUtc(candidatePath)
                        };

                        scannedPlugins.Add(failedPlugin);
                        Interlocked.Increment(ref unavailableCount);
                        ReportScanProgress(
                            progress,
                            string.Format("Marked unavailable: {0} ({1}) - {2}",
                                GetCatalogPluginDisplayName(failedPlugin),
                                failedPlugin.Status,
                                failedPlugin.ErrorDetail),
                            CloneCatalogPlugin(failedPlugin));
                    }
                });

            foreach (VstCatalogPlugin plugin in scannedPlugins)
            {
                if (plugin != null)
                    catalog.Plugins.Add(plugin);
            }

            catalog.Plugins.Sort(delegate (VstCatalogPlugin left, VstCatalogPlugin right)
            {
                int nameResult = string.Compare(GetCatalogPluginDisplayName(left), GetCatalogPluginDisplayName(right), StringComparison.OrdinalIgnoreCase);
                if (nameResult != 0) return nameResult;

                int vendorResult = string.Compare(left != null ? left.Vendor : string.Empty, right != null ? right.Vendor : string.Empty, StringComparison.OrdinalIgnoreCase);
                if (vendorResult != 0) return vendorResult;

                return string.Compare(left != null ? left.Path : string.Empty, right != null ? right.Path : string.Empty, StringComparison.OrdinalIgnoreCase);
            });

            catalog.LastScanUtc = DateTime.UtcNow.ToString("o");
            cancellationToken.ThrowIfCancellationRequested();
            SavePluginCatalog(catalog);
            WriteScanLog(catalog);
            ReportScanProgress(progress, string.Format(
                "Scan complete. {0} available, {1} unavailable, {2} reused from cache.",
                discoveredCount,
                unavailableCount,
                reusedCount), null);
            return catalog;
        }

        public static VstPluginCatalogFile ScanPluginCatalog(IEnumerable<string> searchPaths, IProgress<VstCatalogScanUpdate> progress, bool forceRescanAll)
        {
            return ScanPluginCatalog(searchPaths, progress, forceRescanAll, CancellationToken.None);
        }

        public static void DeletePluginCatalog()
        {
            string catalogFilePath = GetCatalogFilePath(GetCurrentAppDataPath());

            if (string.IsNullOrWhiteSpace(catalogFilePath))
                return;

            try
            {
                if (File.Exists(catalogFilePath))
                    File.Delete(catalogFilePath);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("VST plugin catalog delete failed. " + ex);
            }
        }

        public static string GetCatalogPluginDisplayName(VstCatalogPlugin plugin)
        {
            if (plugin == null)
                return string.Empty;
            if (!string.IsNullOrWhiteSpace(plugin.Name))
                return plugin.Name;
            if (string.IsNullOrWhiteSpace(plugin.Path))
                return "(unnamed plugin)";

            string fileName = Path.GetFileNameWithoutExtension(NormalizePluginPath(plugin.Path));
            return string.IsNullOrWhiteSpace(fileName) ? plugin.Path : fileName;
        }

        private static string GetVstSubdirectory(string appDataPath)
        {
            if (string.IsNullOrWhiteSpace(appDataPath))
                return string.Empty;

            return Path.Combine(appDataPath, "vst");
        }

        private static string GetStateFilePath(string appDataPath)
        {
            if (string.IsNullOrEmpty(appDataPath))
                return StateFileName;

            string vstDir = GetVstSubdirectory(appDataPath);
            MigrateFileToVstSubdirectory(appDataPath, vstDir, StateFileName);
            return Path.Combine(vstDir, StateFileName);
        }

        private static string GetCatalogFilePath(string appDataPath)
        {
            if (string.IsNullOrWhiteSpace(appDataPath))
                return string.Empty;

            string vstDir = GetVstSubdirectory(appDataPath);
            MigrateFileToVstSubdirectory(appDataPath, vstDir, CatalogFileName);
            return Path.Combine(vstDir, CatalogFileName);
        }

        private static void WriteTextFileAtomically(string filePath, string contents)
        {
            string directoryPath;
            string tempFilePath;

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath");

            directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            tempFilePath = Path.Combine(
                string.IsNullOrWhiteSpace(directoryPath) ? "." : directoryPath,
                Path.GetFileName(filePath) + "." + Guid.NewGuid().ToString("N") + ".tmp");

            try
            {
                File.WriteAllText(tempFilePath, contents, Encoding.UTF8);
                try
                {
                    File.Replace(tempFilePath, filePath, null);
                }
                catch (FileNotFoundException)
                {
                    File.Move(tempFilePath, filePath);
                }
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFilePath))
                        File.Delete(tempFilePath);
                }
                catch
                {
                }
            }
        }

        public static string GetPluginFormatDisplayName(VstPluginFormat format)
        {
            switch (format)
            {
                case VstPluginFormat.Vst3:
                    return "VST3";
                case VstPluginFormat.Vst2:
                    return "VST2";
                default:
                    return "Unknown";
            }
        }

        private static void MigrateFileToVstSubdirectory(string appDataPath, string vstDir, string fileName)
        {
            try
            {
                string oldPath = Path.Combine(appDataPath, fileName);
                if (!File.Exists(oldPath))
                    return;

                string newPath = Path.Combine(vstDir, fileName);
                if (File.Exists(newPath))
                    return;

                if (!Directory.Exists(vstDir))
                    Directory.CreateDirectory(vstDir);

                File.Move(oldPath, newPath);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("VST file migration failed for " + fileName + ": " + ex.Message);
            }
        }

        private static void RememberAppDataPath(string appDataPath)
        {
            if (!string.IsNullOrWhiteSpace(appDataPath))
                _stateAppDataPath = appDataPath;
        }

        private static string GetCurrentAppDataPath()
        {
            if (!string.IsNullOrWhiteSpace(_stateAppDataPath))
                return _stateAppDataPath;

            Thetis.Console console = Thetis.Console.getConsole();
            if (console != null && !string.IsNullOrWhiteSpace(console.AppDataPath))
                return console.AppDataPath;

            return string.Empty;
        }

        private static void QueueStructuralStateSave()
        {
            ScheduleStateSave(StructuralStateSaveDelayMs);
        }

        private static void OnNativeStateChanged(VstChainKind kind)
        {
            try
            {
                ScheduleStateSave(DeferredStateSaveDelayMs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("VST OnNativeStateChanged exception: " + ex.Message);
            }
        }

        private static void ScheduleStateSave(int delayMs)
        {
            string appDataPath = GetCurrentAppDataPath();
            if (string.IsNullOrWhiteSpace(appDataPath))
                return;

            RememberAppDataPath(appDataPath);
            lock (_stateSaveLock)
            {
                if (_stateSaveTimer == null)
                    _stateSaveTimer = new Timer(OnDeferredStateSave, null, Timeout.Infinite, Timeout.Infinite);
                _stateSaveTimer.Change(Math.Max(0, delayMs), Timeout.Infinite);
            }
        }

        private static void OnDeferredStateSave(object state)
        {
            bool needsRetry = false;
            string appDataPath = GetCurrentAppDataPath();
            if (string.IsNullOrWhiteSpace(appDataPath))
                return;

            lock (_stateSaveLock)
            {
                needsRetry = SaveStateCore(appDataPath, true);
            }

            if (needsRetry)
                ScheduleStateSave(RetryStateSaveDelayMs);
        }

        // Returns true if a retry is needed (caller must schedule it outside the lock).
        private static bool SaveStateCore(string appDataPath, bool allowRetry)
        {
            string stateFilePath;
            string directoryPath;
            VstStateFile state;

            try
            {
                RememberAppDataPath(appDataPath);
                if (!TryCapturePersistentState(out state))
                {
                    if (allowRetry)
                        return true;

                    SetPersistenceError("VST state save skipped while a chain was rebuilding.", null);
                    return false;
                }

                stateFilePath = GetStateFilePath(appDataPath);
                directoryPath = Path.GetDirectoryName(stateFilePath);
                if (!string.IsNullOrEmpty(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                WriteTextFileAtomically(stateFilePath, JsonConvert.SerializeObject(state, Formatting.Indented));
                ClearPersistenceError();
            }
            catch (Exception ex)
            {
                SetPersistenceError("VST state save failed.", ex);
            }
            return false;
        }

        private static void ClearPersistenceError()
        {
            _lastPersistenceError = string.Empty;
        }

        private static bool TryCapturePersistentState(out VstStateFile state)
        {
            VstChainInfo rxChain;
            VstChainInfo txChain;

            state = null;
            rxChain = CaptureChainInfo(VstChainKind.Rx, true, true);
            txChain = CaptureChainInfo(VstChainKind.Tx, true, true);

            if (rxChain == null || txChain == null)
                return false;

            if (ChainStateIsTransient(rxChain) || ChainStateIsTransient(txChain))
                return false;

            state = new VstStateFile();
            state.SdkAvailable = SdkAvailable;
            state.Rx = ToPersistentState(rxChain);
            state.Tx = ToPersistentState(txChain);
            return true;
        }

        internal static bool ChainStateIsTransient(VstChainInfo chainInfo)
        {
            if (chainInfo == null)
                return false;
            if (chainInfo.Plugins == null || chainInfo.Plugins.Count == 0)
                return false;

            if (chainInfo.HostState != VstHostState.Running)
                return false;

            for (int i = 0; i < chainInfo.Plugins.Count; i++)
            {
                VstPluginState plugin = chainInfo.Plugins[i];

                if (plugin != null && plugin.LoadState == VstPluginLoadState.DescriptorOnly)
                    return true;
            }

            return false;
        }

        private static void SetPersistenceError(string message, Exception ex)
        {
            _lastPersistenceError = message;
            if (ex != null)
                Trace.WriteLine(message + " " + ex);
            else
                Trace.WriteLine(message);
        }

        private static VstChainInfo CaptureChainInfo(VstChainKind kind, bool includeStateData, bool requireConsistentSnapshot)
        {
            VstChainInfo lastCaptured = null;

            for (int attempt = 0; attempt < 4; attempt++)
            {
                VstChainInfo chainInfo = new VstChainInfo();
                bool captureFailed = false;
                int pluginCount;
                int generationBefore;
                int generationAfter;

                generationBefore = NativeGetChainSnapshotGeneration(kind);

                chainInfo.Kind = kind;
                chainInfo.HostState = GetHostState(kind);
                chainInfo.Ready = NativeGetChainReady(kind) != 0;
                chainInfo.Bypass = NativeGetChainBypass(kind) != 0;
                chainInfo.Gain = NativeGetChainGain(kind);

                pluginCount = NativeGetPluginCount(kind);
                if (pluginCount >= 0)
                {
                    for (int i = 0; i < pluginCount; i++)
                    {
                        VstPluginInfoNative nativeInfo;
                        byte[] stateData;

                        if (NativeGetPluginInfo(kind, i, out nativeInfo) != 0)
                        {
                            captureFailed = true;
                            break;
                        }

                        stateData = null;
                        if (includeStateData && !TryGetPluginStateData(kind, i, out stateData))
                        {
                            captureFailed = true;
                            break;
                        }

                        chainInfo.Plugins.Add(new VstPluginState
                        {
                            Path = nativeInfo.Path,
                            Name = nativeInfo.Name,
                            Enabled = nativeInfo.Enabled != 0,
                            Bypass = nativeInfo.Bypass != 0,
                            LoadState = (VstPluginLoadState)nativeInfo.LoadState,
                            Format = NormalizePluginFormat((VstPluginFormat)nativeInfo.Format, nativeInfo.Path),
                            StateData = stateData
                        });
                    }
                }

                generationAfter = NativeGetChainSnapshotGeneration(kind);
                lastCaptured = chainInfo;
                if (!captureFailed && chainInfo.Plugins.Count == pluginCount && generationBefore == generationAfter)
                {
                    // Read latency and format info outside the consistency
                    // loop — these are not part of the plugin snapshot.
                    int latencyCurrent, latencyFloor;
                    chainInfo.SampleRate = NativeGetSampleRate(kind);
                    chainInfo.BlockSize = NativeGetBlockSize(kind);
                    if (NativeGetPipelineLatency(kind, out latencyCurrent, out latencyFloor) == 0)
                    {
                        chainInfo.LatencyBlocks = latencyCurrent;
                        chainInfo.LatencyFloorBlocks = latencyFloor;
                    }
                    return chainInfo;
                }
            }

            if (requireConsistentSnapshot)
                return null;

            return lastCaptured ?? new VstChainInfo { Kind = kind };
        }

        private static VstPluginState GetPluginState(VstChainKind kind, int index, bool includeStateData)
        {
            VstPluginInfoNative nativeInfo;
            byte[] stateData;

            if (!EnsureNativeAvailable()) return null;
            if (NativeGetPluginInfo(kind, index, out nativeInfo) != 0) return null;

            return new VstPluginState
            {
                Path = nativeInfo.Path,
                Name = nativeInfo.Name,
                Enabled = nativeInfo.Enabled != 0,
                Bypass = nativeInfo.Bypass != 0,
                LoadState = (VstPluginLoadState)nativeInfo.LoadState,
                Format = NormalizePluginFormat((VstPluginFormat)nativeInfo.Format, nativeInfo.Path),
                StateData = includeStateData ? (TryGetPluginStateData(kind, index, out stateData) ? stateData : null) : null
            };
        }

        internal static VstChainState ToPersistentState(VstChainInfo chainInfo)
        {
            VstChainState state = new VstChainState();

            if (chainInfo == null)
                return state;

            state.Bypass = chainInfo.Bypass;
            state.Gain = chainInfo.Gain;
            state.LatencyFloor = chainInfo.LatencyFloorBlocks;
            state.Plugins = chainInfo.Plugins ?? new List<VstPluginState>();
            return state;
        }

        private static void ApplyChainState(VstChainKind kind, VstChainState chainState)
        {
            int[] addedIndexes;

            if (chainState == null) return;

            NativeClearChain(kind);
            NativeSetChainGain(kind, chainState.Gain);
            NativeSetChainBypass(kind, chainState.Bypass ? 1 : 0);
            if (chainState.LatencyFloor > 0)
                NativeSetPipelineLatencyFloor(kind, chainState.LatencyFloor);

            if (chainState.Plugins == null)
                return;

            addedIndexes = new int[chainState.Plugins.Count];
            for (int i = 0; i < addedIndexes.Length; i++)
                addedIndexes[i] = -1;

            for (int i = 0; i < chainState.Plugins.Count; i++)
            {
                VstPluginState pluginState = chainState.Plugins[i];

                if (pluginState == null || string.IsNullOrEmpty(pluginState.Path)) continue;
                addedIndexes[i] = NativeAddPlugin(kind, pluginState.Path);
            }

            for (int i = 0; i < chainState.Plugins.Count; i++)
            {
                VstPluginState pluginState = chainState.Plugins[i];
                int index = addedIndexes[i];

                if (pluginState == null || index < 0)
                    continue;

                if (pluginState.StateData != null && pluginState.StateData.Length > 0)
                    NativeSetPluginState(kind, index, pluginState.StateData, pluginState.StateData.Length);
                NativeSetPluginEnabled(kind, index, pluginState.Enabled ? 1 : 0);
                NativeSetPluginBypass(kind, index, pluginState.Bypass ? 1 : 0);
            }

            EnforceRestoredChainState(kind, chainState, addedIndexes);

            if (NativeRequestHostChainSync(kind) != 0)
            {
                Trace.WriteLine("VST restore host sync deferred for " + GetChainDisplayName(kind) + ".");
            }
        }

        private static void EnforceRestoredChainState(VstChainKind kind, VstChainState chainState, int[] addedIndexes)
        {
            if (chainState == null)
                return;

            NativeSetChainGain(kind, chainState.Gain);
            NativeSetChainBypass(kind, chainState.Bypass ? 1 : 0);

            if (chainState.Plugins == null || addedIndexes == null)
                return;

            for (int i = 0; i < chainState.Plugins.Count && i < addedIndexes.Length; i++)
            {
                VstPluginState desiredState = chainState.Plugins[i];
                int index = addedIndexes[i];
                VstPluginState liveState;

                if (desiredState == null || index < 0)
                    continue;

                NativeSetPluginEnabled(kind, index, desiredState.Enabled ? 1 : 0);
                NativeSetPluginBypass(kind, index, desiredState.Bypass ? 1 : 0);
            }
        }

        private static bool TryGetPluginStateData(VstChainKind kind, int index, out byte[] stateData)
        {
            int size;
            int bytesWritten;
            byte[] buffer;

            stateData = null;
            size = NativeGetPluginStateSize(kind, index);
            if (size <= 0)
                return size == 0;

            buffer = new byte[size];
            if (NativeGetPluginState(kind, index, buffer, buffer.Length, out bytesWritten) != 0)
                return false;
            if (bytesWritten <= 0)
                return false;
            if (bytesWritten == buffer.Length)
            {
                stateData = buffer;
                return true;
            }

            Array.Resize(ref buffer, bytesWritten);
            stateData = buffer;
            return true;
        }

        private static VstOperationResult BuildAddPluginResult(VstChainKind kind, int index)
        {
            VstPluginState plugin = GetPluginState(kind, index, false);
            string chainName = GetChainDisplayName(kind);
            string pluginName = GetPluginDisplayName(plugin);

            if (plugin == null)
            {
                return new VstOperationResult
                {
                    Success = true,
                    PluginIndex = index,
                    Message = string.Format("Plugin entry added to the {0} chain.", chainName)
                };
            }

            switch (plugin.LoadState)
            {
                case VstPluginLoadState.Active:
                    return new VstOperationResult
                    {
                        Success = true,
                        PluginIndex = index,
                        Plugin = plugin,
                        Message = string.Format("{0} loaded in the {1} chain.", pluginName, chainName)
                    };

                case VstPluginLoadState.DescriptorOnly:
                    return new VstOperationResult
                    {
                        Success = true,
                        HasWarning = true,
                        PluginIndex = index,
                        Plugin = plugin,
                        Message = string.Format("{0} was added to the {1} chain, but only descriptor data is available.", pluginName, chainName)
                    };

                case VstPluginLoadState.Failed:
                    return new VstOperationResult
                    {
                        Success = true,
                        HasWarning = true,
                        PluginIndex = index,
                        Plugin = plugin,
                        Message = string.Format("{0} was added to the {1} chain, but failed to load.", pluginName, chainName)
                    };

                default:
                    return new VstOperationResult
                    {
                        Success = true,
                        HasWarning = true,
                        PluginIndex = index,
                        Plugin = plugin,
                        Message = string.Format("{0} was added to the {1} chain, but is not active.", pluginName, chainName)
                    };
            }
        }

        private static bool PluginPathExists(string pluginPath)
        {
            return File.Exists(pluginPath) || Directory.Exists(pluginPath);
        }

        private const int Vst2ProbeDllLoadFailed = -4;
        private const int Vst2ProbeNoEntryPoint = -5;
        private const int Vst2ProbeCreateFailed = -6;
        private const int Vst2ProbeCrashed = -7;

        private static string ProbeVst2Plugin(string pluginPath)
        {
            VstPluginProbeInfoNative probeInfo;
            int probeResult;

            try
            {
                probeResult = NativeProbePlugin(pluginPath, out probeInfo);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("VST2 probe failed: " + ex);
                return "The selected file could not be loaded as a VST2 plugin.";
            }

            if (probeResult == Vst2ProbeNoEntryPoint)
                return "The selected DLL is not a VST2 plugin (no VST2 entry point found).";
            if (probeResult == Vst2ProbeDllLoadFailed)
                return "The selected DLL could not be loaded. It may be a 32-bit plugin (only 64-bit VST2 effects are supported).";
            if (probeResult == Vst2ProbeCreateFailed)
                return "The selected DLL could not create a VST2 plugin instance.";
            if (probeResult == Vst2ProbeCrashed)
                return "The selected file crashed during probing and is not compatible.";
            if (probeResult != 0)
                return string.Format("The selected DLL could not be probed as a VST2 plugin (code {0}).", probeResult);

            if (probeInfo.IsValid == 0)
                return "The selected DLL is not a valid VST2 plugin.";

            if (probeInfo.IsAudioEffect == 0)
            {
                string pluginName = !string.IsNullOrWhiteSpace(probeInfo.Name) ? probeInfo.Name : Path.GetFileNameWithoutExtension(pluginPath);
                bool hasInput = probeInfo.HasAudioInput != 0;
                bool hasOutput = probeInfo.HasAudioOutput != 0;

                if (!hasInput && !hasOutput)
                    return string.Format("\"{0}\" is not an audio effect (no audio input or output). Only VST2 audio effects are supported.", pluginName);
                if (!hasInput)
                    return string.Format("\"{0}\" appears to be an instrument or generator, not an audio effect. Only VST2 audio effects are supported.", pluginName);

                return string.Format("\"{0}\" is not a supported audio effect. Only VST2 audio effects are supported (instruments, MIDI plugins, and offline processors are not supported).", pluginName);
            }

            return null;
        }

        internal static VstPluginFormat DetectPluginFormat(string pluginPath)
        {
            string extension = Path.GetExtension(NormalizePluginPath(pluginPath));

            if (string.Equals(extension, ".vst3", StringComparison.OrdinalIgnoreCase))
                return VstPluginFormat.Vst3;
            if (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
                return VstPluginFormat.Vst2;
            return VstPluginFormat.Unknown;
        }

        private static VstPluginFormat NormalizePluginFormat(VstPluginFormat format, string pluginPath)
        {
            return format != VstPluginFormat.Unknown ? format : DetectPluginFormat(pluginPath);
        }

        internal static bool HasPluginPathExtension(string pluginPath)
        {
            return string.Equals(
                Path.GetExtension(NormalizePluginPath(pluginPath)),
                ".vst3",
                StringComparison.OrdinalIgnoreCase);
        }

        internal static string NormalizePluginPath(string pluginPath)
        {
            if (string.IsNullOrWhiteSpace(pluginPath))
                return string.Empty;

            return pluginPath.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        internal static VstPluginCatalogFile NormalizeCatalog(VstPluginCatalogFile catalog)
        {
            List<string> normalizedPaths = new List<string>();

            if (catalog == null)
                catalog = new VstPluginCatalogFile();
            if (catalog.SearchPaths == null)
                catalog.SearchPaths = new List<string>();
            if (catalog.Plugins == null)
                catalog.Plugins = new List<VstCatalogPlugin>();

            foreach (string path in catalog.SearchPaths)
                AddSearchPathIfPresent(normalizedPaths, path);

            if (normalizedPaths.Count == 0)
                normalizedPaths.AddRange(GetDefaultPluginSearchPaths());

            catalog.SearchPaths = normalizedPaths;
            return catalog;
        }

        internal static Dictionary<string, VstCatalogPlugin> BuildCatalogPluginMap(VstPluginCatalogFile catalog)
        {
            Dictionary<string, VstCatalogPlugin> pluginsByPath = new Dictionary<string, VstCatalogPlugin>(StringComparer.OrdinalIgnoreCase);

            if (catalog == null || catalog.Plugins == null)
                return pluginsByPath;

            for (int i = 0; i < catalog.Plugins.Count; i++)
            {
                VstCatalogPlugin plugin = catalog.Plugins[i];
                string normalizedPath = plugin != null ? NormalizePluginPath(plugin.Path) : string.Empty;

                if (string.IsNullOrWhiteSpace(normalizedPath))
                    continue;

                pluginsByPath[normalizedPath] = plugin;
            }

            return pluginsByPath;
        }

        internal static VstCatalogPlugin CloneCatalogPlugin(VstCatalogPlugin plugin)
        {
            if (plugin == null)
                return null;

            return new VstCatalogPlugin
            {
                Path = plugin.Path,
                Name = plugin.Name,
                Vendor = plugin.Vendor,
                Version = plugin.Version,
                Subcategories = plugin.Subcategories,
                HasAudioInput = plugin.HasAudioInput,
                HasAudioOutput = plugin.HasAudioOutput,
                Available = plugin.Available,
                Status = plugin.Status,
                ErrorDetail = plugin.ErrorDetail,
                LastModifiedUtc = plugin.LastModifiedUtc
            };
        }



        private static string GetCatalogPluginLastModifiedUtc(string pluginPath)
        {
            string normalizedPath = NormalizePluginPath(pluginPath);

            if (string.IsNullOrWhiteSpace(normalizedPath))
                return string.Empty;

            try
            {
                if (Directory.Exists(normalizedPath))
                    return Directory.GetLastWriteTimeUtc(normalizedPath).ToString("o");
                if (File.Exists(normalizedPath))
                    return File.GetLastWriteTimeUtc(normalizedPath).ToString("o");
            }
            catch
            {
            }

            return string.Empty;
        }

        internal static bool TryGetReusableCatalogPlugin(Dictionary<string, VstCatalogPlugin> cachedPluginsByPath, string pluginPath, string lastModifiedUtc, out VstCatalogPlugin plugin)
        {
            string normalizedPath = NormalizePluginPath(pluginPath);
            VstCatalogPlugin cachedPlugin;

            plugin = null;
            if (cachedPluginsByPath == null || string.IsNullOrWhiteSpace(normalizedPath))
                return false;
            if (!cachedPluginsByPath.TryGetValue(normalizedPath, out cachedPlugin))
                return false;
            if (!string.Equals(cachedPlugin.LastModifiedUtc ?? string.Empty, lastModifiedUtc ?? string.Empty, StringComparison.Ordinal))
                return false;

            plugin = cachedPlugin;
            return true;
        }

        private static string GetScannerHelperPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ScannerHelperExeName);
        }

        private static VstPluginScannerProbeResult ProbePluginOutOfProcess(string pluginPath)
        {
            string helperPath = GetScannerHelperPath();
            ProcessStartInfo startInfo;
            string standardOutput;
            string standardError;
            VstPluginScannerProbeResult result;
            StringBuilder standardOutputBuilder = new StringBuilder();
            StringBuilder standardErrorBuilder = new StringBuilder();

            if (!File.Exists(helperPath))
            {
                return new VstPluginScannerProbeResult
                {
                    Success = false,
                    Error = "Scanner helper is missing.",
                    ProbeResultCode = -100,
                    IsAudioEffect = 1,
                    Path = pluginPath
                };
            }

            startInfo = new ProcessStartInfo
            {
                FileName = helperPath,
                Arguments = QuoteCommandLineArgument(pluginPath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(helperPath)
            };

            using (ManualResetEventSlim outputClosed = new ManualResetEventSlim(false))
            using (ManualResetEventSlim errorClosed = new ManualResetEventSlim(false))
            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs args)
                {
                    if (args.Data == null)
                    {
                        outputClosed.Set();
                        return;
                    }

                    lock (standardOutputBuilder)
                        standardOutputBuilder.AppendLine(args.Data);
                };
                process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs args)
                {
                    if (args.Data == null)
                    {
                        errorClosed.Set();
                        return;
                    }

                    lock (standardErrorBuilder)
                        standardErrorBuilder.AppendLine(args.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (!process.WaitForExit(PluginProbeTimeoutMs))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                    }

                    try
                    {
                        process.WaitForExit(1000);
                    }
                    catch
                    {
                    }

                    return new VstPluginScannerProbeResult
                    {
                        Success = false,
                        Error = "Scanner timeout.",
                        ProbeResultCode = -101,
                        IsAudioEffect = 1,
                        Path = pluginPath
                    };
                }

                try
                {
                    process.WaitForExit();
                }
                catch
                {
                }

                outputClosed.Wait(1000);
                errorClosed.Wait(1000);
                standardOutput = standardOutputBuilder.ToString();
                standardError = standardErrorBuilder.ToString();

                if (!string.IsNullOrWhiteSpace(standardOutput))
                {
                    try
                    {
                        result = JsonConvert.DeserializeObject<VstPluginScannerProbeResult>(standardOutput.Trim());
                        if (result != null)
                        {
                            // Preserve helper stderr only as supplemental scan failure detail.
                            if (!result.Success && !string.IsNullOrWhiteSpace(standardError))
                            {
                                string detail = standardError.Trim();
                                if (detail.Length > 300)
                                    detail = detail.Substring(0, 300);
                                result.Error = string.IsNullOrWhiteSpace(result.Error)
                                    ? detail
                                    : result.Error + " | " + detail;
                            }
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("VST scanner output parse failed. " + ex);
                    }
                }

                return new VstPluginScannerProbeResult
                {
                    Success = false,
                    Error = string.IsNullOrWhiteSpace(standardError) ? "Scanner crashed." : standardError.Trim(),
                    ProbeResultCode = process.ExitCode,
                    IsAudioEffect = 1,
                    Path = pluginPath
                };
            }
        }

        internal static string BuildUnavailableStatus(VstPluginScannerProbeResult probeResult)
        {
            if (probeResult == null)
                return "Unavailable";

            switch (probeResult.ProbeResultCode)
            {
                case -7:
                    return "Plugin crashed during scan";
                case -100:
                    return "Scanner missing";
                case -101:
                    return "Scanner timeout";
                case -102:
                    return "Scanner access violation";
            }

            if (!string.IsNullOrWhiteSpace(probeResult.Error))
            {
                if (probeResult.Error.IndexOf("protected memory", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Scanner access violation";
                if (probeResult.Error.IndexOf("access violation", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Scanner access violation";
                if (probeResult.Error.IndexOf("crash", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Scanner crash";
                if (probeResult.Error.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "Scanner timeout";

                // Show the native diagnostic (e.g. Module::create failure reason)
                string trimmed = probeResult.Error.Trim();
                if (trimmed.Length > 120)
                    trimmed = trimmed.Substring(0, 120);
                return "Load failed: " + trimmed;
            }

            return "Unavailable";
        }

        internal static bool HasSubcategoryToken(string subcategories, string token)
        {
            string[] parts;

            if (string.IsNullOrWhiteSpace(subcategories) || string.IsNullOrWhiteSpace(token))
                return false;

            parts = subcategories.Split('|');
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.Equals(parts[i].Trim(), token, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        internal static string SummarizeProbeError(string error)
        {
            string[] lines;
            string summary;

            if (string.IsNullOrWhiteSpace(error))
                return string.Empty;

            if (error.IndexOf("protected memory", StringComparison.OrdinalIgnoreCase) >= 0 ||
                error.IndexOf("access violation", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Scanner access violation.";
            }

            lines = error.Replace("\r\n", "\n").Split('\n');

            // Find the last non-empty "probe:" line — this indicates the
            // last step that ran before the crash/failure.
            string lastProbeLine = null;
            for (int j = lines.Length - 1; j >= 0; j--)
            {
                string trimmed = lines[j].Trim();
                if (trimmed.Length > 0 && trimmed.StartsWith("probe:", StringComparison.Ordinal))
                {
                    lastProbeLine = trimmed;
                    break;
                }
            }

            // Prefer the last probe line, fall back to the first non-empty line.
            if (lastProbeLine != null)
                summary = lastProbeLine;
            else
                summary = lines.Length > 0 ? lines[0].Trim() : error.Trim();

            if (summary.Length > 200)
                summary = summary.Substring(0, 197) + "...";

            return summary;
        }

        private static bool IsBundleArchitectureAvailable(string bundlePath)
        {
            string normalizedPath = NormalizePluginPath(bundlePath);

            if (string.IsNullOrWhiteSpace(normalizedPath))
                return false;

            if (!Directory.Exists(normalizedPath))
                return true;

            try
            {
                // Only reject when Contents/ exists but our architecture
                // directory is missing — that confirms a wrong-arch bundle.
                // If Contents/ doesn't exist the layout is non-standard;
                // include it and let the DLL probe decide.
                string contentsDir = Path.Combine(normalizedPath, BundleContentsDir);
                if (!Directory.Exists(contentsDir))
                    return true;

                string archDir = Path.Combine(contentsDir, BundleArchitectureDir);
                return Directory.Exists(archDir);
            }
            catch
            {
                return true;
            }
        }

        private static string GetModuleInfoPath(string pluginPath)
        {
            string normalizedPath = NormalizePluginPath(pluginPath);

            if (string.IsNullOrWhiteSpace(normalizedPath) || !Directory.Exists(normalizedPath))
                return null;

            string moduleInfoPath = Path.Combine(normalizedPath, BundleContentsDir, BundleResourcesDir, ModuleInfoFileName);

            try
            {
                if (File.Exists(moduleInfoPath))
                    return moduleInfoPath;
            }
            catch
            {
            }

            return null;
        }

        private static bool TryProbeFromModuleInfo(string pluginPath, out VstCatalogPlugin result)
        {
            string moduleInfoPath;
            ModuleInfoFile moduleInfo;
            string factoryVendor;

            result = null;
            moduleInfoPath = GetModuleInfoPath(pluginPath);
            if (moduleInfoPath == null)
                return false;

            try
            {
                string json = File.ReadAllText(moduleInfoPath, Encoding.UTF8);
                moduleInfo = JsonConvert.DeserializeObject<ModuleInfoFile>(json);
            }
            catch
            {
                return false;
            }

            if (moduleInfo == null || moduleInfo.Classes == null || moduleInfo.Classes.Count == 0)
                return false;

            factoryVendor = moduleInfo.FactoryInfo != null ? moduleInfo.FactoryInfo.Vendor : null;

            for (int i = 0; i < moduleInfo.Classes.Count; i++)
            {
                ModuleInfoClassEntry classEntry = moduleInfo.Classes[i];

                if (classEntry == null)
                    continue;
                if (!string.Equals(classEntry.Category, AudioModuleClassCategory, StringComparison.OrdinalIgnoreCase))
                    continue;

                string subcategories = classEntry.SubCategories != null
                    ? string.Join("|", classEntry.SubCategories)
                    : string.Empty;
                bool hasFx = HasSubcategoryToken(subcategories, "Fx");
                bool instrumentOnly = HasSubcategoryToken(subcategories, "Instrument") && !hasFx;

                if (instrumentOnly)
                    continue;

                string vendor = !string.IsNullOrWhiteSpace(classEntry.Vendor)
                    ? classEntry.Vendor
                    : factoryVendor;

                result = new VstCatalogPlugin
                {
                    Path = pluginPath,
                    Name = !string.IsNullOrWhiteSpace(classEntry.Name) ? classEntry.Name : Path.GetFileNameWithoutExtension(NormalizePluginPath(pluginPath)),
                    Vendor = vendor,
                    Version = classEntry.Version,
                    Subcategories = subcategories,
                    HasAudioInput = true,
                    HasAudioOutput = true,
                    Available = true,
                    Status = "Available"
                };
                return true;
            }

            return false;
        }


        internal static VstCatalogPlugin BuildUnavailableCatalogPlugin(string pluginPath, string fallbackName, VstPluginScannerProbeResult probeResult)
        {
            return new VstCatalogPlugin
            {
                Path = !string.IsNullOrWhiteSpace(probeResult != null ? probeResult.Path : null) ? probeResult.Path : pluginPath,
                Name = !string.IsNullOrWhiteSpace(probeResult != null ? probeResult.Name : null) ? probeResult.Name : fallbackName,
                Vendor = probeResult != null ? probeResult.Vendor : null,
                Version = probeResult != null ? probeResult.Version : null,
                Subcategories = probeResult != null ? probeResult.Subcategories : null,
                HasAudioInput = probeResult != null && probeResult.HasAudioInput != 0,
                HasAudioOutput = probeResult != null && probeResult.HasAudioOutput != 0,
                Available = false,
                Status = BuildUnavailableStatus(probeResult),
                ErrorDetail = probeResult != null ? SummarizeProbeError(probeResult.Error) : string.Empty
            };
        }

        internal static VstCatalogPlugin BuildAvailableCatalogPlugin(string pluginPath, string fallbackName, VstPluginScannerProbeResult probeResult)
        {
            return new VstCatalogPlugin
            {
                Path = !string.IsNullOrWhiteSpace(probeResult != null ? probeResult.Path : null) ? probeResult.Path : pluginPath,
                Name = !string.IsNullOrWhiteSpace(probeResult != null ? probeResult.Name : null) ? probeResult.Name : fallbackName,
                Vendor = probeResult != null ? probeResult.Vendor : null,
                Version = probeResult != null ? probeResult.Version : null,
                Subcategories = probeResult != null ? probeResult.Subcategories : null,
                HasAudioInput = probeResult != null && probeResult.HasAudioInput != 0,
                HasAudioOutput = probeResult != null && probeResult.HasAudioOutput != 0,
                Available = true,
                Status = "Available"
            };
        }

        internal static string QuoteCommandLineArgument(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            // Windows MSVC command-line quoting: backslashes are literal
            // unless immediately preceding a double quote. Only escape
            // trailing backslashes (which would escape the closing quote)
            // and embedded double quotes.
            StringBuilder quoted = new StringBuilder(value.Length + 4);
            quoted.Append('"');
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '"')
                {
                    quoted.Append('\\');
                    quoted.Append('"');
                }
                else if (value[i] == '\\' && i + 1 == value.Length)
                {
                    // Trailing backslash must be doubled so it does not
                    // escape the closing quote character.
                    quoted.Append('\\');
                    quoted.Append('\\');
                }
                else
                {
                    quoted.Append(value[i]);
                }
            }
            quoted.Append('"');
            return quoted.ToString();
        }

        internal static void AddSearchPathIfPresent(List<string> searchPaths, string path)
        {
            string normalizedPath;

            if (searchPaths == null)
                return;

            normalizedPath = NormalizePluginPath(path);
            if (string.IsNullOrWhiteSpace(normalizedPath))
                return;
            if (!Directory.Exists(normalizedPath))
                return;
            if (ContainsSearchPath(searchPaths, normalizedPath))
                return;

            searchPaths.Add(normalizedPath);
        }

        internal static bool ContainsSearchPath(List<string> searchPaths, string path)
        {
            string normalizedPath = NormalizePluginPath(path);

            if (searchPaths == null || string.IsNullOrWhiteSpace(normalizedPath))
                return false;

            for (int i = 0; i < searchPaths.Count; i++)
            {
                if (string.Equals(NormalizePluginPath(searchPaths[i]), normalizedPath, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        internal static void CollectPluginCandidates(string searchPath, HashSet<string> candidatePaths)
        {
            Stack<string> pending = new Stack<string>();
            string normalizedSearchPath = NormalizePluginPath(searchPath);

            if (candidatePaths == null || string.IsNullOrWhiteSpace(normalizedSearchPath))
                return;

            if (File.Exists(normalizedSearchPath) && HasPluginPathExtension(normalizedSearchPath))
            {
                candidatePaths.Add(normalizedSearchPath);
                return;
            }

            if (!Directory.Exists(normalizedSearchPath))
                return;
            if (HasPluginPathExtension(normalizedSearchPath))
            {
                candidatePaths.Add(normalizedSearchPath);
                return;
            }

            pending.Push(normalizedSearchPath);
            while (pending.Count > 0)
            {
                string currentPath = pending.Pop();
                string[] entries;

                try
                {
                    entries = Directory.GetFileSystemEntries(currentPath);
                }
                catch
                {
                    continue;
                }

                for (int i = 0; i < entries.Length; i++)
                {
                    string entry = NormalizePluginPath(entries[i]);

                    if (string.IsNullOrWhiteSpace(entry))
                        continue;

                    if (Directory.Exists(entry))
                    {
                        if (HasPluginPathExtension(entry))
                        {
                            candidatePaths.Add(entry);
                            continue;
                        }

                        pending.Push(entry);
                        continue;
                    }

                    if (File.Exists(entry) && HasPluginPathExtension(entry))
                        candidatePaths.Add(entry);
                }
            }
        }

        private static VstCatalogPlugin ProbeCatalogPlugin(string pluginPath)
        {
            string fallbackName = Path.GetFileNameWithoutExtension(NormalizePluginPath(pluginPath));
            VstPluginScannerProbeResult probeResult;
            VstCatalogPlugin moduleInfoPlugin;

            if (TryProbeFromModuleInfo(pluginPath, out moduleInfoPlugin))
                return moduleInfoPlugin;

            try
            {
                probeResult = ProbePluginOutOfProcess(pluginPath);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("VST plugin probe failed. " + ex);
                return new VstCatalogPlugin
                {
                    Path = pluginPath,
                    Name = fallbackName,
                    Available = false,
                    Status = "Scanner error",
                    ErrorDetail = SummarizeProbeError(ex.Message)
                };
            }

            if (probeResult == null)
            {
                return new VstCatalogPlugin
                {
                    Path = pluginPath,
                    Name = fallbackName,
                    Available = false,
                    Status = "Scanner error"
                };
            }

            if (probeResult.IsAudioEffect == 0)
            {
                if (probeResult.ProbeResultCode == -5 || probeResult.ProbeResultCode == -6)
                    return null;

                return BuildUnavailableCatalogPlugin(pluginPath, fallbackName, probeResult);
            }

            if (!probeResult.Success || probeResult.IsValid == 0)
                return BuildUnavailableCatalogPlugin(pluginPath, fallbackName, probeResult);

            return BuildAvailableCatalogPlugin(pluginPath, fallbackName, probeResult);
        }

        private static void ReportScanProgress(IProgress<VstCatalogScanUpdate> progress, string message, VstCatalogPlugin plugin)
        {
            if (progress == null)
                return;
            if (string.IsNullOrWhiteSpace(message) && plugin == null)
                return;

            progress.Report(new VstCatalogScanUpdate
            {
                Message = message,
                Plugin = plugin
            });
        }

        private static bool EnsureNativeAvailable()
        {
            if (_nativeUnavailable) return false;

            try
            {
                NativeGetSdkAvailable();
                if (!_nativeCallbackRegistered)
                {
                    lock (_stateSaveLock)
                    {
                        if (!_nativeCallbackRegistered)
                        {
                            NativeSetStateChangedCallback(_nativeStateChangedCallback);
                            _nativeCallbackRegistered = true;
                        }
                    }
                }
                return true;
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
            catch (BadImageFormatException)
            {
            }

            _nativeUnavailable = true;
            return false;
        }
    }
}
