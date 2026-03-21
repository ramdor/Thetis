using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Thetis.Tests
{
    public class NormalizePluginPathTests
    {
        [Fact]
        public void Null_returns_empty()
        {
            Assert.Equal(string.Empty, VstHost.NormalizePluginPath(null));
        }

        [Fact]
        public void Whitespace_returns_empty()
        {
            Assert.Equal(string.Empty, VstHost.NormalizePluginPath("   "));
        }

        [Fact]
        public void Trims_whitespace()
        {
            Assert.Equal(@"C:\Plugins\test.vst3", VstHost.NormalizePluginPath("  C:\\Plugins\\test.vst3  "));
        }

        [Fact]
        public void Strips_trailing_backslash()
        {
            Assert.Equal(@"C:\Plugins\MyPlugin", VstHost.NormalizePluginPath(@"C:\Plugins\MyPlugin\"));
        }

        [Fact]
        public void Strips_trailing_forward_slash()
        {
            Assert.Equal(@"C:\Plugins\MyPlugin", VstHost.NormalizePluginPath(@"C:\Plugins\MyPlugin/"));
        }

        [Fact]
        public void Preserves_path_without_trailing_separator()
        {
            Assert.Equal(@"C:\Plugins\test.vst3", VstHost.NormalizePluginPath(@"C:\Plugins\test.vst3"));
        }
    }

    public class DetectPluginFormatTests
    {
        [Fact]
        public void Vst3_extension_detected()
        {
            Assert.Equal(VstPluginFormat.Vst3, VstHost.DetectPluginFormat(@"C:\Plugins\reverb.vst3"));
        }

        [Fact]
        public void Vst3_case_insensitive()
        {
            Assert.Equal(VstPluginFormat.Vst3, VstHost.DetectPluginFormat(@"C:\Plugins\reverb.VST3"));
        }

        [Fact]
        public void Dll_detected_as_Vst2()
        {
            Assert.Equal(VstPluginFormat.Vst2, VstHost.DetectPluginFormat(@"C:\Plugins\compressor.dll"));
        }

        [Fact]
        public void Dll_case_insensitive()
        {
            Assert.Equal(VstPluginFormat.Vst2, VstHost.DetectPluginFormat(@"C:\Plugins\compressor.DLL"));
        }

        [Fact]
        public void Unknown_extension_returns_Unknown()
        {
            Assert.Equal(VstPluginFormat.Unknown, VstHost.DetectPluginFormat(@"C:\Plugins\readme.txt"));
        }

        [Fact]
        public void No_extension_returns_Unknown()
        {
            Assert.Equal(VstPluginFormat.Unknown, VstHost.DetectPluginFormat(@"C:\Plugins\nope"));
        }

        [Fact]
        public void Empty_path_returns_Unknown()
        {
            Assert.Equal(VstPluginFormat.Unknown, VstHost.DetectPluginFormat(""));
        }

        [Fact]
        public void Path_with_trailing_slash_still_detects()
        {
            // VST3 bundles are directories ending in .vst3
            Assert.Equal(VstPluginFormat.Vst3, VstHost.DetectPluginFormat(@"C:\Plugins\MyReverb.vst3\"));
        }
    }

    public class HasPluginPathExtensionTests
    {
        [Fact]
        public void Vst3_returns_true()
        {
            Assert.True(VstHost.HasPluginPathExtension(@"C:\Plugins\test.vst3"));
        }

        [Fact]
        public void Dll_returns_false()
        {
            Assert.False(VstHost.HasPluginPathExtension(@"C:\Plugins\test.dll"));
        }

        [Fact]
        public void No_extension_returns_false()
        {
            Assert.False(VstHost.HasPluginPathExtension(@"C:\Plugins\test"));
        }

        [Fact]
        public void Case_insensitive()
        {
            Assert.True(VstHost.HasPluginPathExtension(@"C:\Plugins\test.VST3"));
        }
    }

    public class HasSubcategoryTokenTests
    {
        [Fact]
        public void Finds_single_token()
        {
            Assert.True(VstHost.HasSubcategoryToken("Fx", "Fx"));
        }

        [Fact]
        public void Finds_token_in_pipe_delimited_list()
        {
            Assert.True(VstHost.HasSubcategoryToken("Fx|EQ|Dynamics", "EQ"));
        }

        [Fact]
        public void Case_insensitive_match()
        {
            Assert.True(VstHost.HasSubcategoryToken("Fx|EQ|Dynamics", "eq"));
        }

        [Fact]
        public void Trims_whitespace_around_tokens()
        {
            Assert.True(VstHost.HasSubcategoryToken("Fx | EQ | Dynamics", "EQ"));
        }

        [Fact]
        public void No_match_returns_false()
        {
            Assert.False(VstHost.HasSubcategoryToken("Fx|EQ|Dynamics", "Reverb"));
        }

        [Fact]
        public void Null_subcategories_returns_false()
        {
            Assert.False(VstHost.HasSubcategoryToken(null, "Fx"));
        }

        [Fact]
        public void Null_token_returns_false()
        {
            Assert.False(VstHost.HasSubcategoryToken("Fx|EQ", null));
        }

        [Fact]
        public void Empty_subcategories_returns_false()
        {
            Assert.False(VstHost.HasSubcategoryToken("", "Fx"));
        }

        [Fact]
        public void Partial_match_does_not_match()
        {
            // "EQ" should not match "EQualizer"
            Assert.False(VstHost.HasSubcategoryToken("FxEQualizer|Dynamics", "EQ"));
        }
    }

    public class QuoteCommandLineArgumentTests
    {
        [Fact]
        public void Null_returns_empty_quotes()
        {
            Assert.Equal("\"\"", VstHost.QuoteCommandLineArgument(null));
        }

        [Fact]
        public void Empty_returns_empty_quotes()
        {
            Assert.Equal("\"\"", VstHost.QuoteCommandLineArgument(""));
        }

        [Fact]
        public void Simple_value_is_quoted()
        {
            Assert.Equal("\"hello\"", VstHost.QuoteCommandLineArgument("hello"));
        }

        [Fact]
        public void Path_with_spaces_is_quoted()
        {
            Assert.Equal(@"""C:\Program Files\plugin.vst3""", VstHost.QuoteCommandLineArgument(@"C:\Program Files\plugin.vst3"));
        }

        [Fact]
        public void Embedded_quote_is_escaped()
        {
            Assert.Equal(@"""say \""hello\""""", VstHost.QuoteCommandLineArgument("say \"hello\""));
        }

        [Fact]
        public void Trailing_backslash_is_doubled()
        {
            // A trailing backslash would escape the closing quote without doubling
            Assert.Equal(@"""path\\""", VstHost.QuoteCommandLineArgument(@"path\"));
        }

        [Fact]
        public void Interior_backslash_is_not_doubled()
        {
            // Only trailing backslashes need doubling per MSVC rules
            Assert.Equal(@"""C:\path\to\file""", VstHost.QuoteCommandLineArgument(@"C:\path\to\file"));
        }
    }

    public class SummarizeProbeErrorTests
    {
        [Fact]
        public void Null_returns_empty()
        {
            Assert.Equal(string.Empty, VstHost.SummarizeProbeError(null));
        }

        [Fact]
        public void Whitespace_returns_empty()
        {
            Assert.Equal(string.Empty, VstHost.SummarizeProbeError("   "));
        }

        [Fact]
        public void Access_violation_returns_scanner_message()
        {
            Assert.Equal("Scanner access violation.", VstHost.SummarizeProbeError("Attempted to read or write protected memory"));
        }

        [Fact]
        public void Access_violation_keyword_returns_scanner_message()
        {
            Assert.Equal("Scanner access violation.", VstHost.SummarizeProbeError("access violation at 0x00000"));
        }

        [Fact]
        public void Prefers_last_probe_line()
        {
            string error = "probe: init\nsome noise\nprobe: load factory\nmore noise";
            Assert.Equal("probe: load factory", VstHost.SummarizeProbeError(error));
        }

        [Fact]
        public void Falls_back_to_first_line_when_no_probe_lines()
        {
            string error = "Something went wrong\nDetails here";
            Assert.Equal("Something went wrong", VstHost.SummarizeProbeError(error));
        }

        [Fact]
        public void Truncates_at_200_chars()
        {
            string longLine = new string('x', 250);
            string result = VstHost.SummarizeProbeError(longLine);
            Assert.Equal(200, result.Length);
            Assert.EndsWith("...", result);
        }

        [Fact]
        public void Handles_crlf_line_endings()
        {
            string error = "probe: step1\r\nprobe: step2\r\n";
            Assert.Equal("probe: step2", VstHost.SummarizeProbeError(error));
        }
    }

    public class BuildUnavailableStatusTests
    {
        [Fact]
        public void Null_result_returns_Unavailable()
        {
            Assert.Equal("Unavailable", VstHost.BuildUnavailableStatus(null));
        }

        [Fact]
        public void Crash_code_minus7()
        {
            var result = new VstPluginScannerProbeResult { ProbeResultCode = -7 };
            Assert.Equal("Plugin crashed during scan", VstHost.BuildUnavailableStatus(result));
        }

        [Fact]
        public void Scanner_missing_code_minus100()
        {
            var result = new VstPluginScannerProbeResult { ProbeResultCode = -100 };
            Assert.Equal("Scanner missing", VstHost.BuildUnavailableStatus(result));
        }

        [Fact]
        public void Scanner_timeout_code_minus101()
        {
            var result = new VstPluginScannerProbeResult { ProbeResultCode = -101 };
            Assert.Equal("Scanner timeout", VstHost.BuildUnavailableStatus(result));
        }

        [Fact]
        public void Scanner_access_violation_code_minus102()
        {
            var result = new VstPluginScannerProbeResult { ProbeResultCode = -102 };
            Assert.Equal("Scanner access violation", VstHost.BuildUnavailableStatus(result));
        }

        [Fact]
        public void Error_with_protected_memory()
        {
            var result = new VstPluginScannerProbeResult { Error = "protected memory fault" };
            Assert.Equal("Scanner access violation", VstHost.BuildUnavailableStatus(result));
        }

        [Fact]
        public void Error_with_crash_keyword()
        {
            var result = new VstPluginScannerProbeResult { Error = "plugin crash detected" };
            Assert.Equal("Scanner crash", VstHost.BuildUnavailableStatus(result));
        }

        [Fact]
        public void Error_with_timeout_keyword()
        {
            var result = new VstPluginScannerProbeResult { Error = "operation timeout exceeded" };
            Assert.Equal("Scanner timeout", VstHost.BuildUnavailableStatus(result));
        }

        [Fact]
        public void Generic_error_shows_load_failed_with_message()
        {
            var result = new VstPluginScannerProbeResult { Error = "Module::create failed" };
            Assert.StartsWith("Load failed: ", VstHost.BuildUnavailableStatus(result));
            Assert.Contains("Module::create failed", VstHost.BuildUnavailableStatus(result));
        }

        [Fact]
        public void Long_error_is_truncated_in_load_failed()
        {
            var result = new VstPluginScannerProbeResult { Error = new string('x', 200) };
            string status = VstHost.BuildUnavailableStatus(result);
            Assert.StartsWith("Load failed: ", status);
            // "Load failed: " is 14 chars + up to 120 chars = max 134
            Assert.True(status.Length <= 134);
        }

        [Fact]
        public void No_error_no_code_returns_Unavailable()
        {
            var result = new VstPluginScannerProbeResult();
            Assert.Equal("Unavailable", VstHost.BuildUnavailableStatus(result));
        }
    }

    public class DisplayNameTests
    {
        [Fact]
        public void HostState_Disabled() { Assert.Equal("Host disabled", VstHost.GetHostStateDisplayName(VstHostState.Disabled)); }
        [Fact]
        public void HostState_Starting() { Assert.Equal("Host starting", VstHost.GetHostStateDisplayName(VstHostState.Starting)); }
        [Fact]
        public void HostState_Running() { Assert.Equal("Host running", VstHost.GetHostStateDisplayName(VstHostState.Running)); }
        [Fact]
        public void HostState_Unavailable() { Assert.Equal("Host unavailable", VstHost.GetHostStateDisplayName(VstHostState.Unavailable)); }
        [Fact]
        public void HostState_Crashed() { Assert.Equal("Host crashed", VstHost.GetHostStateDisplayName(VstHostState.Crashed)); }
        [Fact]
        public void HostState_Restarting() { Assert.Equal("Host restarting", VstHost.GetHostStateDisplayName(VstHostState.Restarting)); }
        [Fact]
        public void HostState_unknown_value() { Assert.Equal("Host unknown", VstHost.GetHostStateDisplayName((VstHostState)99)); }

        [Fact]
        public void LoadState_Active() { Assert.Equal("Loaded", VstHost.GetLoadStateDisplayName(VstPluginLoadState.Active)); }
        [Fact]
        public void LoadState_DescriptorOnly() { Assert.Equal("Descriptor only", VstHost.GetLoadStateDisplayName(VstPluginLoadState.DescriptorOnly)); }
        [Fact]
        public void LoadState_Failed() { Assert.Equal("Failed", VstHost.GetLoadStateDisplayName(VstPluginLoadState.Failed)); }
        [Fact]
        public void LoadState_None() { Assert.Equal("Not loaded", VstHost.GetLoadStateDisplayName(VstPluginLoadState.None)); }

        [Fact]
        public void PluginFormat_Vst3() { Assert.Equal("VST3", VstHost.GetPluginFormatDisplayName(VstPluginFormat.Vst3)); }
        [Fact]
        public void PluginFormat_Vst2() { Assert.Equal("VST2", VstHost.GetPluginFormatDisplayName(VstPluginFormat.Vst2)); }
        [Fact]
        public void PluginFormat_Unknown() { Assert.Equal("Unknown", VstHost.GetPluginFormatDisplayName(VstPluginFormat.Unknown)); }

        [Fact]
        public void PluginDisplayName_prefers_name()
        {
            var plugin = new VstPluginState { Name = "My Reverb", Path = @"C:\reverb.vst3" };
            Assert.Equal("My Reverb", VstHost.GetPluginDisplayName(plugin));
        }

        [Fact]
        public void PluginDisplayName_falls_back_to_filename()
        {
            var plugin = new VstPluginState { Name = null, Path = @"C:\Plugins\compressor.vst3" };
            Assert.Equal("compressor", VstHost.GetPluginDisplayName(plugin));
        }

        [Fact]
        public void PluginDisplayName_null_name_and_path()
        {
            var plugin = new VstPluginState { Name = null, Path = null };
            Assert.Equal("(unnamed plugin)", VstHost.GetPluginDisplayName(plugin));
        }

        [Fact]
        public void PluginDisplayName_null_plugin()
        {
            Assert.Equal(string.Empty, VstHost.GetPluginDisplayName(null));
        }

        [Fact]
        public void CatalogPluginDisplayName_prefers_name()
        {
            var plugin = new VstCatalogPlugin { Name = "EQ Eight", Path = @"C:\eq.vst3" };
            Assert.Equal("EQ Eight", VstHost.GetCatalogPluginDisplayName(plugin));
        }

        [Fact]
        public void CatalogPluginDisplayName_falls_back_to_filename()
        {
            var plugin = new VstCatalogPlugin { Name = null, Path = @"C:\Plugins\delay.vst3" };
            Assert.Equal("delay", VstHost.GetCatalogPluginDisplayName(plugin));
        }

        [Fact]
        public void CatalogPluginDisplayName_null()
        {
            Assert.Equal(string.Empty, VstHost.GetCatalogPluginDisplayName(null));
        }
    }

    public class CatalogPluginMapTests
    {
        [Fact]
        public void Null_catalog_returns_empty_map()
        {
            var map = VstHost.BuildCatalogPluginMap(null);
            Assert.Empty(map);
        }

        [Fact]
        public void Empty_plugins_returns_empty_map()
        {
            var catalog = new VstPluginCatalogFile();
            var map = VstHost.BuildCatalogPluginMap(catalog);
            Assert.Empty(map);
        }

        [Fact]
        public void Plugins_indexed_by_normalized_path()
        {
            var plugin = new VstCatalogPlugin { Path = @"C:\Plugins\reverb.vst3", Name = "Reverb" };
            var catalog = new VstPluginCatalogFile { Plugins = new List<VstCatalogPlugin> { plugin } };

            var map = VstHost.BuildCatalogPluginMap(catalog);

            Assert.Single(map);
            Assert.True(map.ContainsKey(@"C:\Plugins\reverb.vst3"));
        }

        [Fact]
        public void Case_insensitive_lookup()
        {
            var plugin = new VstCatalogPlugin { Path = @"C:\Plugins\Reverb.vst3", Name = "Reverb" };
            var catalog = new VstPluginCatalogFile { Plugins = new List<VstCatalogPlugin> { plugin } };

            var map = VstHost.BuildCatalogPluginMap(catalog);

            Assert.True(map.ContainsKey(@"c:\plugins\reverb.vst3"));
        }

        [Fact]
        public void Duplicate_paths_last_wins()
        {
            var first = new VstCatalogPlugin { Path = @"C:\Plugins\reverb.vst3", Name = "First" };
            var second = new VstCatalogPlugin { Path = @"C:\Plugins\reverb.vst3", Name = "Second" };
            var catalog = new VstPluginCatalogFile { Plugins = new List<VstCatalogPlugin> { first, second } };

            var map = VstHost.BuildCatalogPluginMap(catalog);

            Assert.Single(map);
            Assert.Equal("Second", map[@"C:\Plugins\reverb.vst3"].Name);
        }

        [Fact]
        public void Null_plugin_entry_skipped()
        {
            var catalog = new VstPluginCatalogFile
            {
                Plugins = new List<VstCatalogPlugin> { null, new VstCatalogPlugin { Path = @"C:\a.vst3" } }
            };

            var map = VstHost.BuildCatalogPluginMap(catalog);
            Assert.Single(map);
        }

        [Fact]
        public void Plugin_with_empty_path_skipped()
        {
            var catalog = new VstPluginCatalogFile
            {
                Plugins = new List<VstCatalogPlugin> { new VstCatalogPlugin { Path = "" } }
            };

            var map = VstHost.BuildCatalogPluginMap(catalog);
            Assert.Empty(map);
        }
    }

    public class CloneCatalogPluginTests
    {
        [Fact]
        public void Null_returns_null()
        {
            Assert.Null(VstHost.CloneCatalogPlugin(null));
        }

        [Fact]
        public void Clone_is_independent_copy()
        {
            var original = new VstCatalogPlugin
            {
                Path = @"C:\reverb.vst3",
                Name = "Reverb",
                Vendor = "Acme",
                Version = "1.0",
                Subcategories = "Fx|Reverb",
                HasAudioInput = true,
                HasAudioOutput = true,
                Available = true,
                Status = "Available",
                ErrorDetail = null,
                LastModifiedUtc = "2025-01-01T00:00:00Z"
            };

            var clone = VstHost.CloneCatalogPlugin(original);

            Assert.NotSame(original, clone);
            Assert.Equal(original.Path, clone.Path);
            Assert.Equal(original.Name, clone.Name);
            Assert.Equal(original.Vendor, clone.Vendor);
            Assert.Equal(original.Version, clone.Version);
            Assert.Equal(original.Subcategories, clone.Subcategories);
            Assert.Equal(original.HasAudioInput, clone.HasAudioInput);
            Assert.Equal(original.HasAudioOutput, clone.HasAudioOutput);
            Assert.Equal(original.Available, clone.Available);
            Assert.Equal(original.Status, clone.Status);
            Assert.Equal(original.LastModifiedUtc, clone.LastModifiedUtc);
        }

        [Fact]
        public void Modifying_clone_does_not_affect_original()
        {
            var original = new VstCatalogPlugin { Name = "Original", Available = true };
            var clone = VstHost.CloneCatalogPlugin(original);

            clone.Name = "Modified";
            clone.Available = false;

            Assert.Equal("Original", original.Name);
            Assert.True(original.Available);
        }
    }

    public class TryGetReusableCatalogPluginTests
    {
        [Fact]
        public void Returns_false_for_null_map()
        {
            VstCatalogPlugin plugin;
            Assert.False(VstHost.TryGetReusableCatalogPlugin(null, @"C:\a.vst3", "ts", out plugin));
            Assert.Null(plugin);
        }

        [Fact]
        public void Returns_false_when_path_not_found()
        {
            var map = new Dictionary<string, VstCatalogPlugin>(StringComparer.OrdinalIgnoreCase);
            VstCatalogPlugin plugin;
            Assert.False(VstHost.TryGetReusableCatalogPlugin(map, @"C:\missing.vst3", "ts", out plugin));
        }

        [Fact]
        public void Returns_false_when_timestamp_differs()
        {
            var cached = new VstCatalogPlugin { Path = @"C:\a.vst3", LastModifiedUtc = "2025-01-01" };
            var map = new Dictionary<string, VstCatalogPlugin>(StringComparer.OrdinalIgnoreCase)
            {
                { @"C:\a.vst3", cached }
            };

            VstCatalogPlugin plugin;
            Assert.False(VstHost.TryGetReusableCatalogPlugin(map, @"C:\a.vst3", "2025-06-01", out plugin));
        }

        [Fact]
        public void Returns_true_when_path_and_timestamp_match()
        {
            var cached = new VstCatalogPlugin { Path = @"C:\a.vst3", LastModifiedUtc = "2025-01-01" };
            var map = new Dictionary<string, VstCatalogPlugin>(StringComparer.OrdinalIgnoreCase)
            {
                { @"C:\a.vst3", cached }
            };

            VstCatalogPlugin plugin;
            Assert.True(VstHost.TryGetReusableCatalogPlugin(map, @"C:\a.vst3", "2025-01-01", out plugin));
            Assert.Same(cached, plugin);
        }

        [Fact]
        public void Both_null_timestamps_match()
        {
            var cached = new VstCatalogPlugin { Path = @"C:\a.vst3", LastModifiedUtc = null };
            var map = new Dictionary<string, VstCatalogPlugin>(StringComparer.OrdinalIgnoreCase)
            {
                { @"C:\a.vst3", cached }
            };

            VstCatalogPlugin plugin;
            Assert.True(VstHost.TryGetReusableCatalogPlugin(map, @"C:\a.vst3", null, out plugin));
        }
    }

    public class SearchPathTests
    {
        [Fact]
        public void ContainsSearchPath_case_insensitive()
        {
            var paths = new List<string> { @"C:\VST3" };
            Assert.True(VstHost.ContainsSearchPath(paths, @"c:\vst3"));
        }

        [Fact]
        public void ContainsSearchPath_normalizes_trailing_slash()
        {
            var paths = new List<string> { @"C:\VST3" };
            Assert.True(VstHost.ContainsSearchPath(paths, @"C:\VST3\"));
        }

        [Fact]
        public void ContainsSearchPath_null_list_returns_false()
        {
            Assert.False(VstHost.ContainsSearchPath(null, @"C:\VST3"));
        }

        [Fact]
        public void ContainsSearchPath_empty_path_returns_false()
        {
            var paths = new List<string> { @"C:\VST3" };
            Assert.False(VstHost.ContainsSearchPath(paths, ""));
        }

        [Fact]
        public void AddSearchPath_skips_null_list()
        {
            // Should not throw
            VstHost.AddSearchPathIfPresent(null, @"C:\VST3");
        }

        [Fact]
        public void AddSearchPath_skips_empty_path()
        {
            var paths = new List<string>();
            VstHost.AddSearchPathIfPresent(paths, "");
            Assert.Empty(paths);
        }

        [Fact]
        public void AddSearchPath_skips_nonexistent_directory()
        {
            var paths = new List<string>();
            VstHost.AddSearchPathIfPresent(paths, @"C:\This\Path\Definitely\Does\Not\Exist\VST3");
            Assert.Empty(paths);
        }

        [Fact]
        public void AddSearchPath_adds_existing_directory()
        {
            var paths = new List<string>();
            // Use a directory we know exists
            string tempDir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
            VstHost.AddSearchPathIfPresent(paths, tempDir);
            Assert.Single(paths);
            Assert.Equal(tempDir, paths[0]);
        }

        [Fact]
        public void AddSearchPath_rejects_duplicate()
        {
            var paths = new List<string>();
            string tempDir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
            VstHost.AddSearchPathIfPresent(paths, tempDir);
            VstHost.AddSearchPathIfPresent(paths, tempDir);
            Assert.Single(paths);
        }
    }

    public class ToPersistentStateTests
    {
        [Fact]
        public void Null_returns_default_state()
        {
            var state = VstHost.ToPersistentState(null);
            Assert.True(state.Bypass);
            Assert.Equal(1.0, state.Gain);
            Assert.NotNull(state.Plugins);
            Assert.Empty(state.Plugins);
        }

        [Fact]
        public void Copies_bypass_and_gain()
        {
            var info = new VstChainInfo { Bypass = false, Gain = 3.5 };
            var state = VstHost.ToPersistentState(info);
            Assert.False(state.Bypass);
            Assert.Equal(3.5, state.Gain);
        }

        [Fact]
        public void Copies_plugin_list()
        {
            var plugins = new List<VstPluginState>
            {
                new VstPluginState { Path = @"C:\a.vst3", Name = "A", Enabled = true },
                new VstPluginState { Path = @"C:\b.dll", Name = "B", Bypass = true }
            };
            var info = new VstChainInfo { Plugins = plugins };

            var state = VstHost.ToPersistentState(info);

            Assert.Equal(2, state.Plugins.Count);
            Assert.Same(plugins, state.Plugins);
        }

        [Fact]
        public void Null_plugins_list_gets_empty_list()
        {
            var info = new VstChainInfo { Plugins = null };
            var state = VstHost.ToPersistentState(info);
            Assert.NotNull(state.Plugins);
            Assert.Empty(state.Plugins);
        }
    }

    public class ChainStateIsTransientTests
    {
        [Fact]
        public void Null_returns_false()
        {
            Assert.False(VstHost.ChainStateIsTransient(null));
        }

        [Fact]
        public void No_plugins_returns_false()
        {
            var info = new VstChainInfo { HostState = VstHostState.Running };
            Assert.False(VstHost.ChainStateIsTransient(info));
        }

        [Fact]
        public void All_active_plugins_returns_false()
        {
            var info = new VstChainInfo
            {
                HostState = VstHostState.Running,
                Plugins = new List<VstPluginState>
                {
                    new VstPluginState { LoadState = VstPluginLoadState.Active }
                }
            };
            Assert.False(VstHost.ChainStateIsTransient(info));
        }

        [Fact]
        public void DescriptorOnly_plugin_while_running_is_transient()
        {
            var info = new VstChainInfo
            {
                HostState = VstHostState.Running,
                Plugins = new List<VstPluginState>
                {
                    new VstPluginState { LoadState = VstPluginLoadState.DescriptorOnly }
                }
            };
            Assert.True(VstHost.ChainStateIsTransient(info));
        }

        [Fact]
        public void DescriptorOnly_but_not_running_returns_false()
        {
            var info = new VstChainInfo
            {
                HostState = VstHostState.Starting,
                Plugins = new List<VstPluginState>
                {
                    new VstPluginState { LoadState = VstPluginLoadState.DescriptorOnly }
                }
            };
            Assert.False(VstHost.ChainStateIsTransient(info));
        }

        [Fact]
        public void Mixed_plugins_with_one_descriptoronly_is_transient()
        {
            var info = new VstChainInfo
            {
                HostState = VstHostState.Running,
                Plugins = new List<VstPluginState>
                {
                    new VstPluginState { LoadState = VstPluginLoadState.Active },
                    new VstPluginState { LoadState = VstPluginLoadState.DescriptorOnly },
                    new VstPluginState { LoadState = VstPluginLoadState.Active }
                }
            };
            Assert.True(VstHost.ChainStateIsTransient(info));
        }
    }

    public class BuildCatalogPluginTests
    {
        [Fact]
        public void Available_plugin_with_probe_result()
        {
            var probe = new VstPluginScannerProbeResult
            {
                Path = @"C:\probed.vst3",
                Name = "Probed Reverb",
                Vendor = "Acme",
                Version = "2.0",
                Subcategories = "Fx|Reverb",
                HasAudioInput = 1,
                HasAudioOutput = 1
            };

            var plugin = VstHost.BuildAvailableCatalogPlugin(@"C:\fallback.vst3", "Fallback", probe);

            Assert.Equal(@"C:\probed.vst3", plugin.Path);
            Assert.Equal("Probed Reverb", plugin.Name);
            Assert.Equal("Acme", plugin.Vendor);
            Assert.Equal("2.0", plugin.Version);
            Assert.True(plugin.Available);
            Assert.Equal("Available", plugin.Status);
            Assert.True(plugin.HasAudioInput);
            Assert.True(plugin.HasAudioOutput);
        }

        [Fact]
        public void Available_plugin_null_probe_uses_fallbacks()
        {
            var plugin = VstHost.BuildAvailableCatalogPlugin(@"C:\fallback.vst3", "Fallback Name", null);

            Assert.Equal(@"C:\fallback.vst3", plugin.Path);
            Assert.Equal("Fallback Name", plugin.Name);
            Assert.Null(plugin.Vendor);
            Assert.True(plugin.Available);
            Assert.False(plugin.HasAudioInput);
        }

        [Fact]
        public void Unavailable_plugin_has_status_and_error()
        {
            var probe = new VstPluginScannerProbeResult
            {
                ProbeResultCode = -7,
                Name = "Bad Plugin"
            };

            var plugin = VstHost.BuildUnavailableCatalogPlugin(@"C:\bad.vst3", "Bad", probe);

            Assert.False(plugin.Available);
            Assert.Equal("Plugin crashed during scan", plugin.Status);
        }

        [Fact]
        public void Unavailable_plugin_null_probe()
        {
            var plugin = VstHost.BuildUnavailableCatalogPlugin(@"C:\missing.vst3", "Missing", null);

            Assert.False(plugin.Available);
            Assert.Equal("Unavailable", plugin.Status);
            Assert.Equal(string.Empty, plugin.ErrorDetail);
        }
    }

    public class NormalizeCatalogTests
    {
        [Fact]
        public void Null_catalog_creates_new_one()
        {
            var result = VstHost.NormalizeCatalog(null);
            Assert.NotNull(result);
            Assert.NotNull(result.SearchPaths);
            Assert.NotNull(result.Plugins);
        }

        [Fact]
        public void Null_lists_are_initialized()
        {
            var catalog = new VstPluginCatalogFile { SearchPaths = null, Plugins = null };
            var result = VstHost.NormalizeCatalog(catalog);
            Assert.NotNull(result.SearchPaths);
            Assert.NotNull(result.Plugins);
        }

        [Fact]
        public void Empty_search_paths_get_defaults()
        {
            var catalog = new VstPluginCatalogFile();
            var result = VstHost.NormalizeCatalog(catalog);
            // Should have default paths (those that exist on this machine)
            // We can't assert exact count since it depends on the machine,
            // but the method should not throw
            Assert.NotNull(result.SearchPaths);
        }
    }

    public class CollectPluginCandidatesTests
    {
        [Fact]
        public void Null_candidates_set_does_not_throw()
        {
            VstHost.CollectPluginCandidates(@"C:\Plugins", null);
        }

        [Fact]
        public void Empty_path_collects_nothing()
        {
            var candidates = new HashSet<string>();
            VstHost.CollectPluginCandidates("", candidates);
            Assert.Empty(candidates);
        }

        [Fact]
        public void Nonexistent_path_collects_nothing()
        {
            var candidates = new HashSet<string>();
            VstHost.CollectPluginCandidates(@"C:\This\Does\Not\Exist\Anywhere", candidates);
            Assert.Empty(candidates);
        }

        [Fact]
        public void Discovers_vst3_files_in_temp_directory()
        {
            // Create a temp structure with .vst3 files
            string testDir = Path.Combine(Path.GetTempPath(), "ThetisTestCollect_" + Guid.NewGuid().ToString("N"));
            string subDir = Path.Combine(testDir, "SubFolder");
            Directory.CreateDirectory(subDir);

            try
            {
                string vst3File = Path.Combine(subDir, "TestPlugin.vst3");
                string txtFile = Path.Combine(subDir, "readme.txt");
                File.WriteAllText(vst3File, "fake");
                File.WriteAllText(txtFile, "fake");

                var candidates = new HashSet<string>();
                VstHost.CollectPluginCandidates(testDir, candidates);

                Assert.Single(candidates);
                Assert.Contains(candidates, p => p.EndsWith("TestPlugin.vst3", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                Directory.Delete(testDir, true);
            }
        }

        [Fact]
        public void Discovers_vst3_bundle_directories()
        {
            string testDir = Path.Combine(Path.GetTempPath(), "ThetisTestBundle_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDir);

            try
            {
                // Create a .vst3 directory (bundle format)
                string bundleDir = Path.Combine(testDir, "MyPlugin.vst3");
                Directory.CreateDirectory(bundleDir);

                var candidates = new HashSet<string>();
                VstHost.CollectPluginCandidates(testDir, candidates);

                Assert.Single(candidates);
                Assert.Contains(candidates, p => p.EndsWith("MyPlugin.vst3", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                Directory.Delete(testDir, true);
            }
        }

        [Fact]
        public void Does_not_recurse_into_vst3_bundles()
        {
            string testDir = Path.Combine(Path.GetTempPath(), "ThetisTestNoRecurse_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testDir);

            try
            {
                // Create a .vst3 bundle with content inside
                string bundleDir = Path.Combine(testDir, "Outer.vst3");
                string innerDir = Path.Combine(bundleDir, "Contents");
                Directory.CreateDirectory(innerDir);
                File.WriteAllText(Path.Combine(innerDir, "Inner.vst3"), "fake");

                var candidates = new HashSet<string>();
                VstHost.CollectPluginCandidates(testDir, candidates);

                // Should only find the outer bundle, not recurse inside
                Assert.Single(candidates);
                Assert.Contains(candidates, p => p.EndsWith("Outer.vst3", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    public class DefaultStateTests
    {
        [Fact]
        public void VstChainState_defaults_to_bypassed()
        {
            var state = new VstChainState();
            Assert.True(state.Bypass);
        }

        [Fact]
        public void VstChainState_defaults_gain_to_unity()
        {
            var state = new VstChainState();
            Assert.Equal(1.0, state.Gain);
        }

        [Fact]
        public void VstChainState_defaults_empty_plugin_list()
        {
            var state = new VstChainState();
            Assert.NotNull(state.Plugins);
            Assert.Empty(state.Plugins);
        }

        [Fact]
        public void VstStateFile_defaults_version_3()
        {
            var file = new VstStateFile();
            Assert.Equal(3, file.Version);
        }

        [Fact]
        public void VstStateFile_rx_and_tx_chains_created()
        {
            var file = new VstStateFile();
            Assert.NotNull(file.Rx);
            Assert.NotNull(file.Tx);
        }

        [Fact]
        public void VstStateFile_both_chains_default_bypassed()
        {
            var file = new VstStateFile();
            Assert.True(file.Rx.Bypass);
            Assert.True(file.Tx.Bypass);
        }

        [Fact]
        public void VstChainInfo_defaults_gain_to_unity()
        {
            var info = new VstChainInfo();
            Assert.Equal(1.0, info.Gain);
            Assert.NotNull(info.Plugins);
        }
    }
}
