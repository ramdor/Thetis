/*  clsTCIpermissions.cs

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
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Thetis
{
    public static class TCIpermissions
    {
        public const string VIEW_STATUS = "view_status";
        public const string VIEW_SPECTRUM = "view_spectrum";
        public const string LISTEN_RX_AUDIO = "listen_rx_audio";
        public const string CHANGE_RX_AUDIO = "change_rx_audio";
        public const string CHANGE_FREQUENCY = "change_frequency";
        public const string CHANGE_MODE = "change_mode";
        public const string CHANGE_RX_FILTER = "change_rx_filter";
        public const string CHANGE_RX_DSP = "change_rx_dsp";
        public const string CHANGE_RX_FRONTEND = "change_rx_frontend";
        public const string CHANGE_DISPLAY = "change_display";
        public const string TX_PTT = "tx_ptt";
        public const string TX_TUNE = "tx_tune";
        public const string TX_POWER = "tx_power";
        public const string TX_SETTINGS = "tx_settings";
        public const string TX_AUDIO = "tx_audio";
        public const string CW_KEYER = "cw_keyer";
        public const string SPOTS = "spots";
        public const string RADIO_POWER = "radio_power";
        public const string ADMIN = "admin";

        private const int SCHEMA_VERSION = 1;
        private const string HASH_ALGORITHM = "PBKDF2-SHA256";
        private const int HASH_ITERATIONS = 100000;
        private const int SALT_BYTES = 16;
        private const int HASH_BYTES = 32;
        private const int MAX_ATTEMPTS = 5;
        private const int LOCKOUT_SECONDS = 300;

        private sealed class TCIHashDefaults
        {
            [JsonProperty("algorithm")]
            public string Algorithm = HASH_ALGORITHM;

            [JsonProperty("iterations")]
            public int Iterations = HASH_ITERATIONS;

            [JsonProperty("saltBytes")]
            public int SaltBytes = SALT_BYTES;

            [JsonProperty("hashBytes")]
            public int HashBytes = HASH_BYTES;
        }

        private sealed class TCIRole
        {
            [JsonProperty("id")]
            public string Id;

            [JsonProperty("displayName")]
            public string DisplayName;

            [JsonProperty("permissions")]
            public List<string> Permissions = new List<string>();
        }

        private sealed class TCIUser
        {
            [JsonProperty("id")]
            public string Id;

            [JsonProperty("displayName")]
            public string DisplayName;

            [JsonProperty("enabled")]
            public bool Enabled;

            [JsonProperty("role")]
            public string Role;

            [JsonProperty("permissions")]
            public List<string> Permissions = new List<string>();

            [JsonProperty("accessCodeHash")]
            public string AccessCodeHash;

            [JsonProperty("accessCodeSalt")]
            public string AccessCodeSalt;

            [JsonProperty("hashAlgorithm")]
            public string HashAlgorithm = HASH_ALGORITHM;

            [JsonProperty("hashIterations")]
            public int HashIterations = HASH_ITERATIONS;

            [JsonProperty("createdUtc")]
            public string CreatedUtc;

            [JsonProperty("modifiedUtc")]
            public string ModifiedUtc;

            [JsonProperty("notes")]
            public string Notes;

            [JsonProperty("accessCode", NullValueHandling = NullValueHandling.Ignore)]
            public string AccessCode;
        }

        private sealed class TCIConfig
        {
            [JsonProperty("schemaVersion")]
            public int SchemaVersion = SCHEMA_VERSION;

            [JsonProperty("hashDefaults")]
            public TCIHashDefaults HashDefaults = new TCIHashDefaults();

            [JsonProperty("roles")]
            public List<TCIRole> Roles = new List<TCIRole>();

            [JsonProperty("users")]
            public List<TCIUser> Users = new List<TCIUser>();
        }

        private sealed class RuntimeCounter
        {
            public int FailedAttempts;
            public DateTime LockedUntilUtc = DateTime.MinValue;
        }

        private sealed class RuntimeSession
        {
            public string ConnectionId;
            public string EffectiveClientIp;
            public bool Authenticated;
            public string UserId;
            public HashSet<string> Permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public sealed class AuthenticationResult
        {
            public bool Success;
            public bool Locked;
            public int AttemptsLeft;
            public int LockedSeconds;
            public string UserId;
            public string PermissionsCsv;
        }

        public sealed class SetupUser
        {
            public string Id;
            public string DisplayName;
            public bool Enabled;
            public string Role;
            public List<string> Permissions;
            public string CreatedUtc;
            public string ModifiedUtc;
            public string Notes;
            public bool HasAccessCode;
        }

        public sealed class SetupRole
        {
            public string Id;
            public string DisplayName;
            public List<string> Permissions;
        }

        private static readonly object _lockObj = new object();
        private static TCIConfig _config;
        private static Dictionary<string, TCIRole> _rolesById = new Dictionary<string, TCIRole>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, TCIUser> _usersById = new Dictionary<string, TCIUser>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, RuntimeCounter> _ipCounters = new Dictionary<string, RuntimeCounter>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, RuntimeCounter> _userCounters = new Dictionary<string, RuntimeCounter>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, RuntimeCounter> _sessionCounters = new Dictionary<string, RuntimeCounter>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, RuntimeSession> _sessions = new Dictionary<string, RuntimeSession>(StringComparer.OrdinalIgnoreCase);

        static TCIpermissions()
        {
            LoadFromJson(null);
        }

        public static string CreateDefaultJson()
        {
            lock (_lockObj)
            {
                TCIConfig previousConfig = _config;
                Dictionary<string, TCIRole> previousRoles = _rolesById;
                Dictionary<string, TCIUser> previousUsers = _usersById;

                try
                {
                    _config = createDefaultConfig();
                    rebuildLookupsLocked();
                    return SaveToJson();
                }
                finally
                {
                    _config = previousConfig;
                    _rolesById = previousRoles;
                    _usersById = previousUsers;
                }
            }
        }

        public static void LoadFromJson(string json)
        {
            lock (_lockObj)
            {
                TCIConfig newConfig = null;

                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        newConfig = JsonConvert.DeserializeObject<TCIConfig>(json);
                    }
                    catch
                    {
                        newConfig = null;
                    }
                }

                if (newConfig == null || newConfig.SchemaVersion != SCHEMA_VERSION)
                    newConfig = createDefaultConfig();

                normaliseConfigLocked(newConfig);
                _config = newConfig;
                rebuildLookupsLocked();
                ResetRuntimeLockouts();
                _sessions.Clear();
            }
        }

        public static string SaveToJson()
        {
            lock (_lockObj)
            {
                TCIConfig saveConfig = cloneConfigForSaveLocked();
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                settings.NullValueHandling = NullValueHandling.Ignore;
                return JsonConvert.SerializeObject(saveConfig, settings);
            }
        }

        public static bool AddUser(string id, string displayName, string role, string accessCode, string notes)
        {
            lock (_lockObj)
            {
                id = normaliseId(id);
                if (string.IsNullOrEmpty(id) || _usersById.ContainsKey(id)) return false;
                role = normaliseId(role);
                if (!isValidRoleLocked(role)) role = "viewer";

                string utc = utcNowString();
                TCIUser user = new TCIUser();
                user.Id = id;
                user.DisplayName = cleanDisplayName(displayName, id);
                user.Enabled = false;
                user.Role = role;
                user.Permissions = new List<string>();
                user.CreatedUtc = utc;
                user.ModifiedUtc = utc;
                user.Notes = notes ?? string.Empty;
                setUserAccessCodeLocked(user, accessCode ?? string.Empty);
                _config.Users.Add(user);
                rebuildLookupsLocked();
                return true;
            }
        }

        public static bool RemoveUser(string id)
        {
            lock (_lockObj)
            {
                id = normaliseId(id);
                if (!_usersById.ContainsKey(id)) return false;
                _config.Users.RemoveAll(u => string.Equals(u.Id, id, StringComparison.OrdinalIgnoreCase));
                rebuildLookupsLocked();
                ResetUserLockout(id);
                return true;
            }
        }

        public static bool UpdateUser(string id, string displayName, string notes)
        {
            lock (_lockObj)
            {
                TCIUser user;
                if (!tryGetUserLocked(id, out user)) return false;
                user.DisplayName = cleanDisplayName(displayName, user.Id);
                user.Notes = notes ?? string.Empty;
                user.ModifiedUtc = utcNowString();
                return true;
            }
        }

        public static bool SetUserEnabled(string id, bool enabled)
        {
            lock (_lockObj)
            {
                TCIUser user;
                if (!tryGetUserLocked(id, out user)) return false;
                user.Enabled = enabled;
                user.ModifiedUtc = utcNowString();
                if (!enabled) ResetUserLockout(user.Id);
                return true;
            }
        }

        public static bool SetUserRole(string id, string role)
        {
            lock (_lockObj)
            {
                TCIUser user;
                if (!tryGetUserLocked(id, out user)) return false;
                role = normaliseId(role);
                if (!isValidRoleLocked(role)) return false;
                user.Role = role;
                user.ModifiedUtc = utcNowString();
                return true;
            }
        }

        public static bool SetUserPermissions(string id, IEnumerable<string> permissions)
        {
            lock (_lockObj)
            {
                TCIUser user;
                if (!tryGetUserLocked(id, out user)) return false;
                user.Permissions = cleanPermissionList(permissions);
                user.ModifiedUtc = utcNowString();
                return true;
            }
        }

        public static bool SetUserAccessCode(string id, string accessCode)
        {
            lock (_lockObj)
            {
                TCIUser user;
                if (!tryGetUserLocked(id, out user)) return false;
                setUserAccessCodeLocked(user, accessCode ?? string.Empty);
                user.ModifiedUtc = utcNowString();
                ResetUserLockout(user.Id);
                return true;
            }
        }

        public static AuthenticationResult Authenticate(string connectionId, string effectiveClientIp, string accessCode)
        {
            lock (_lockObj)
            {
                connectionId = normaliseConnectionId(connectionId);
                effectiveClientIp = normaliseIp(effectiveClientIp);

                AuthenticationResult result = new AuthenticationResult();
                result.AttemptsLeft = MAX_ATTEMPTS;

                RuntimeCounter ipCounter = getCounterLocked(_ipCounters, effectiveClientIp);
                RuntimeCounter sessionCounter = getCounterLocked(_sessionCounters, connectionId);

                int lockedSeconds = getLockedSeconds(ipCounter);
                if (lockedSeconds <= 0)
                    lockedSeconds = getLockedSeconds(sessionCounter);

                if (lockedSeconds > 0)
                {
                    result.Locked = true;
                    result.LockedSeconds = lockedSeconds;
                    result.AttemptsLeft = 0;
                    return result;
                }

                if (!string.IsNullOrEmpty(accessCode))
                {
                    foreach (TCIUser user in _config.Users)
                    {
                        if (user == null || !user.Enabled) continue;
                        if (!hasStoredAccessCode(user)) continue;
                        if (!verifyAccessCode(user, accessCode)) continue;

                        RuntimeCounter userCounter = getCounterLocked(_userCounters, user.Id);
                        lockedSeconds = getLockedSeconds(userCounter);
                        if (lockedSeconds > 0)
                        {
                            result.Locked = true;
                            result.LockedSeconds = lockedSeconds;
                            result.AttemptsLeft = 0;
                            return result;
                        }

                        HashSet<string> permissions = getEffectivePermissionsLocked(user);
                        RuntimeSession session = new RuntimeSession();
                        session.ConnectionId = connectionId;
                        session.EffectiveClientIp = effectiveClientIp;
                        session.Authenticated = true;
                        session.UserId = user.Id;
                        session.Permissions = permissions;
                        _sessions[connectionId] = session;
                        ipCounter.FailedAttempts = 0;
                        sessionCounter.FailedAttempts = 0;
                        userCounter.FailedAttempts = 0;
                        result.Success = true;
                        result.UserId = user.Id;
                        result.PermissionsCsv = permissionsToCsv(permissions);
                        result.AttemptsLeft = MAX_ATTEMPTS;
                        return result;
                    }
                }

                int failedAttempts = Math.Max(++ipCounter.FailedAttempts, ++sessionCounter.FailedAttempts);
                int attemptsLeft = Math.Max(0, MAX_ATTEMPTS - failedAttempts);
                result.AttemptsLeft = attemptsLeft;

                if (attemptsLeft <= 0)
                {
                    DateTime lockedUntilUtc = DateTime.UtcNow.AddSeconds(LOCKOUT_SECONDS);
                    ipCounter.LockedUntilUtc = lockedUntilUtc;
                    sessionCounter.LockedUntilUtc = lockedUntilUtc;
                    result.Locked = true;
                    result.LockedSeconds = LOCKOUT_SECONDS;
                }

                return result;
            }
        }

        public static bool IsAllowed(string connectionId, string command, string[] args)
        {
            lock (_lockObj)
            {
                RuntimeSession session;
                if (!_sessions.TryGetValue(normaliseConnectionId(connectionId), out session)) return false;
                if (session == null || !session.Authenticated) return false;

                string permission = GetPermissionForCommand(command, args);
                if (string.IsNullOrEmpty(permission)) return false;
                return session.Permissions.Contains(permission);
            }
        }

        public static string GetPermissionForCommand(string command, string[] args)
        {
            if (string.IsNullOrWhiteSpace(command)) return null;

            string cmd = command.Trim().ToLowerInvariant();
            int argCount = args == null ? 0 : args.Length;

            switch (cmd)
            {
                case "start":
                case "stop":
                    return RADIO_POWER;
                case "shutdown_ex":
                case "run_cat_ex":
                    return ADMIN;
                case "authentication_pincode":
                    return null;
                case "modulation":
                    return CHANGE_MODE;
                case "dds":
                case "if":
                case "vfo":
                case "vfo_swap_ex":
                case "spot_simulate_click":
                    return CHANGE_FREQUENCY;
                case "rx_filter_band":
                    return CHANGE_RX_FILTER;
                case "display_start":
                case "iq_start":
                    return VIEW_SPECTRUM;
                case "display_stop":
                case "iq_stop":
                    return VIEW_SPECTRUM;
                case "display_zoom":
                case "display_pan":
                case "display_pixels":
                case "display_delta_mode":
                case "display_block_delta_mode":
                case "display_accuracy_mode":
                case "display_adaptive_blocks":
                case "display_keyframe_interval":
                case "display_quant_bits":
                case "display_framerate":
                case "display_fftsize":
                case "display_window":
                case "display_detector":
                case "display_averaging":
                case "display_average_time":
                case "display_one_hz":
                case "display_dbm_range":
                case "iq_samplerate":
                    return CHANGE_DISPLAY;
                case "audio_start":
                case "audio_stop":
                case "line_out_start":
                case "line_out_stop":
                    return LISTEN_RX_AUDIO;
                case "audio_samplerate":
                case "audio_codec":
                case "audio_reconfigure_begin":
                case "audio_reconfigure_end":
                case "audio_stream_sample_type":
                case "audio_stream_channels":
                case "audio_stream_samples":
                case "audio_opus_application":
                case "audio_opus_signal":
                case "audio_opus_bandwidth":
                case "audio_opus_bitrate":
                case "audio_opus_vbr":
                case "audio_opus_cvbr":
                case "audio_opus_complexity":
                case "audio_opus_fec":
                case "audio_opus_dtx":
                case "audio_opus_packet_loss":
                case "audio_opus_frame_ms":
                case "tx_stream_audio_buffering":
                case "mute":
                case "volume":
                case "rx_mute":
                case "mon_volume":
                case "mon_enable":
                case "rx_volume":
                case "rx_balance":
                case "sql_enable":
                case "sql_level":
                    return CHANGE_RX_AUDIO;
                case "rx_nr_enable":
                case "rx_nr_enable_ex":
                case "rx_nb_enable":
                case "rx_nb_enable_ex":
                case "rx_anf_enable":
                case "rx_bin_enable":
                case "rx_apf_enable":
                case "rx_nf_enable":
                case "agc_auto_ex":
                case "agc_mode":
                case "agc_gain":
                    return CHANGE_RX_DSP;
                case "rx_enable":
                case "rx_channel_enable":
                case "rx_step_att_enabled_ex":
                case "rx_step_att_ex":
                case "rx_preamp_att_ex":
                case "vfo_sync_ex":
                case "fm_deviation_ex":
                case "rx_ctun_ex":
                    return CHANGE_RX_FRONTEND;
                case "trx":
                    return TX_PTT;
                case "tune":
                    return TX_TUNE;
                case "drive":
                case "tune_drive":
                    return TX_POWER;
                case "split_enable":
                case "tx_filter_band_ex":
                case "tx_profile_ex":
                    return TX_SETTINGS;
                case "binary_tx_audio":
                    return TX_AUDIO;
                case "cw_macros_speed":
                case "cw_macros_delay":
                case "cw_keyer_speed":
                case "cw_macros_speed_up":
                case "cw_macros_speed_down":
                case "cw_macros":
                case "cw_terminal":
                case "cw_msg":
                case "keyer":
                case "cw_macros_stop":
                case "callsign_send":
                    return CW_KEYER;
                case "spot":
                case "spot_delete":
                case "spot_clear":
                    return SPOTS;
                case "rit_enable":
                case "xit_enable":
                case "rit_offset":
                case "xit_offset":
                case "lock":
                case "vfo_lock":
                case "set_in_focus":
                case "rx_sensors_enable":
                case "rx_sensors_enable_ex":
                case "tx_sensors_enable":
                case "tx_profiles_ex":
                case "calibration_ex":
                case "rt":
                    return VIEW_STATUS;
                case "digl_offset":
                case "digu_offset":
                    return argCount > 0 ? CHANGE_FREQUENCY : VIEW_STATUS;
            }

            return null;
        }

        public static string GetPermissionsForSession(string connectionId)
        {
            lock (_lockObj)
            {
                RuntimeSession session;
                if (!_sessions.TryGetValue(normaliseConnectionId(connectionId), out session)) return string.Empty;
                if (session == null || !session.Authenticated) return string.Empty;
                return permissionsToCsv(session.Permissions);
            }
        }

        public static List<SetupUser> GetUsersForSetup()
        {
            lock (_lockObj)
            {
                List<SetupUser> users = new List<SetupUser>();
                foreach (TCIUser user in _config.Users.OrderBy(u => u.Id, StringComparer.OrdinalIgnoreCase))
                {
                    SetupUser setupUser = new SetupUser();
                    setupUser.Id = user.Id;
                    setupUser.DisplayName = user.DisplayName;
                    setupUser.Enabled = user.Enabled;
                    setupUser.Role = user.Role;
                    setupUser.Permissions = new List<string>(cleanPermissionList(user.Permissions));
                    setupUser.CreatedUtc = user.CreatedUtc;
                    setupUser.ModifiedUtc = user.ModifiedUtc;
                    setupUser.Notes = user.Notes;
                    setupUser.HasAccessCode = hasStoredAccessCode(user);
                    users.Add(setupUser);
                }

                return users;
            }
        }

        public static List<SetupRole> GetRolesForSetup()
        {
            lock (_lockObj)
            {
                List<SetupRole> roles = new List<SetupRole>();
                foreach (TCIRole role in _config.Roles.OrderBy(r => r.Id, StringComparer.OrdinalIgnoreCase))
                {
                    SetupRole setupRole = new SetupRole();
                    setupRole.Id = role.Id;
                    setupRole.DisplayName = role.DisplayName;
                    setupRole.Permissions = new List<string>(cleanPermissionList(role.Permissions));
                    roles.Add(setupRole);
                }

                return roles;
            }
        }

        public static void ResetRuntimeLockouts()
        {
            lock (_lockObj)
            {
                _ipCounters.Clear();
                _userCounters.Clear();
                _sessionCounters.Clear();
            }
        }

        public static void ResetUserLockout(string userId)
        {
            lock (_lockObj)
            {
                userId = normaliseId(userId);
                if (string.IsNullOrEmpty(userId)) return;
                _userCounters.Remove(userId);
            }
        }

        public static void ResetIpLockout(string ipAddress)
        {
            lock (_lockObj)
            {
                ipAddress = normaliseIp(ipAddress);
                _ipCounters.Remove(ipAddress);
            }
        }

        public static void CloseSession(string connectionId)
        {
            lock (_lockObj)
            {
                _sessions.Remove(normaliseConnectionId(connectionId));
                _sessionCounters.Remove(normaliseConnectionId(connectionId));
            }
        }

        private static TCIConfig createDefaultConfig()
        {
            TCIConfig config = new TCIConfig();
            config.SchemaVersion = SCHEMA_VERSION;
            config.HashDefaults = new TCIHashDefaults();
            config.Roles = createDefaultRoles();
            config.Users = new List<TCIUser>();

            string utc = utcNowString();
            TCIUser guest = new TCIUser();
            guest.Id = "guest";
            guest.DisplayName = "Guest";
            guest.Enabled = true;
            guest.Role = "listener";
            guest.Permissions = new List<string>();
            guest.CreatedUtc = utc;
            guest.ModifiedUtc = utc;
            guest.Notes = "Built in disabled guest account";
            setUserAccessCodeLocked(guest, "123456");
            config.Users.Add(guest);
            return config;
        }

        private static List<TCIRole> createDefaultRoles()
        {
            List<TCIRole> roles = new List<TCIRole>();
            roles.Add(createRole("viewer", "Viewer", new string[]                               { VIEW_STATUS, VIEW_SPECTRUM }));
            roles.Add(createRole("listener", "Listener", new string[]                           { VIEW_STATUS, VIEW_SPECTRUM, LISTEN_RX_AUDIO }));
            roles.Add(createRole("rx_operator", "RX Operator", new string[]                     { VIEW_STATUS, VIEW_SPECTRUM, LISTEN_RX_AUDIO, CHANGE_RX_AUDIO, CHANGE_FREQUENCY, CHANGE_MODE, CHANGE_RX_FILTER, CHANGE_RX_DSP, CHANGE_DISPLAY }));
            roles.Add(createRole("advanced_rx_operator", "Advanced RX Operator", new string[]   { VIEW_STATUS, VIEW_SPECTRUM, LISTEN_RX_AUDIO, CHANGE_RX_AUDIO, CHANGE_FREQUENCY, CHANGE_MODE, CHANGE_RX_FILTER, CHANGE_RX_DSP, CHANGE_DISPLAY, CHANGE_RX_FRONTEND }));
            roles.Add(createRole("tx_operator", "TX Operator", new string[]                     { VIEW_STATUS, VIEW_SPECTRUM, LISTEN_RX_AUDIO, CHANGE_RX_AUDIO, CHANGE_FREQUENCY, CHANGE_MODE, CHANGE_RX_FILTER, CHANGE_RX_DSP, CHANGE_DISPLAY, CHANGE_RX_FRONTEND, TX_PTT, TX_TUNE, TX_POWER, TX_SETTINGS, CW_KEYER }));
            roles.Add(createRole("admin", "Admin", new string[]                                 { VIEW_STATUS, VIEW_SPECTRUM, LISTEN_RX_AUDIO, CHANGE_RX_AUDIO, CHANGE_FREQUENCY, CHANGE_MODE, CHANGE_RX_FILTER, CHANGE_RX_DSP, CHANGE_DISPLAY, CHANGE_RX_FRONTEND, TX_PTT, TX_TUNE, TX_POWER, TX_SETTINGS, TX_AUDIO, CW_KEYER, SPOTS, RADIO_POWER, ADMIN }));
            return roles;
        }

        private static TCIRole createRole(string id, string displayName, IEnumerable<string> permissions)
        {
            TCIRole role = new TCIRole();
            role.Id = id;
            role.DisplayName = displayName;
            role.Permissions = cleanPermissionList(permissions);
            return role;
        }

        private static void normaliseConfigLocked(TCIConfig config)
        {
            if (config.HashDefaults == null)
                config.HashDefaults = new TCIHashDefaults();

            if (config.Roles == null || config.Roles.Count == 0)
                config.Roles = createDefaultRoles();

            if (config.Users == null)
                config.Users = new List<TCIUser>();

            Dictionary<string, bool> roleIds = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            List<TCIRole> roles = new List<TCIRole>();
            foreach (TCIRole role in config.Roles)
            {
                if (role == null) continue;
                role.Id = normaliseId(role.Id);
                if (string.IsNullOrEmpty(role.Id)) continue;
                if (roleIds.ContainsKey(role.Id)) continue;
                role.DisplayName = cleanDisplayName(role.DisplayName, role.Id);
                role.Permissions = cleanPermissionList(role.Permissions);
                roles.Add(role);
                roleIds[role.Id] = true;
            }

            if (roles.Count == 0)
                roles = createDefaultRoles();

            config.Roles = roles;

            Dictionary<string, bool> userIds = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            List<TCIUser> users = new List<TCIUser>();
            foreach (TCIUser user in config.Users)
            {
                if (user == null) continue;
                user.Id = normaliseId(user.Id);
                if (string.IsNullOrEmpty(user.Id)) continue;
                if (userIds.ContainsKey(user.Id)) continue;
                user.DisplayName = cleanDisplayName(user.DisplayName, user.Id);
                user.Role = normaliseId(user.Role);
                if (string.IsNullOrWhiteSpace(user.Role) || !roleIds.ContainsKey(user.Role))
                    user.Enabled = false;
                user.Permissions = cleanPermissionList(user.Permissions);
                if (string.IsNullOrWhiteSpace(user.CreatedUtc)) user.CreatedUtc = utcNowString();
                if (string.IsNullOrWhiteSpace(user.ModifiedUtc)) user.ModifiedUtc = user.CreatedUtc;
                if (user.Notes == null) user.Notes = string.Empty;
                if (!string.IsNullOrEmpty(user.AccessCode) && !hasStoredAccessCode(user))
                {
                    setUserAccessCodeLocked(user, user.AccessCode);
                    user.AccessCode = null;
                }
                if (!hasStoredAccessCode(user)) user.Enabled = false;
                users.Add(user);
                userIds[user.Id] = true;
            }

            if (users.Count == 0)
            {
                TCIConfig defaultConfig = createDefaultConfig();
                users.AddRange(defaultConfig.Users);
            }

            config.Users = users;
        }

        private static void rebuildLookupsLocked()
        {
            _rolesById = new Dictionary<string, TCIRole>(StringComparer.OrdinalIgnoreCase);
            _usersById = new Dictionary<string, TCIUser>(StringComparer.OrdinalIgnoreCase);

            if (_config == null) return;

            foreach (TCIRole role in _config.Roles)
            {
                if (role != null && !string.IsNullOrEmpty(role.Id))
                    _rolesById[role.Id] = role;
            }

            foreach (TCIUser user in _config.Users)
            {
                if (user != null && !string.IsNullOrEmpty(user.Id))
                    _usersById[user.Id] = user;
            }
        }

        private static TCIConfig cloneConfigForSaveLocked()
        {
            TCIConfig saveConfig = new TCIConfig();
            saveConfig.SchemaVersion = SCHEMA_VERSION;
            saveConfig.HashDefaults = new TCIHashDefaults();
            saveConfig.Roles = new List<TCIRole>();
            saveConfig.Users = new List<TCIUser>();

            foreach (TCIRole role in _config.Roles.OrderBy(r => r.Id, StringComparer.OrdinalIgnoreCase))
            {
                TCIRole saveRole = new TCIRole();
                saveRole.Id = role.Id;
                saveRole.DisplayName = role.DisplayName;
                saveRole.Permissions = cleanPermissionList(role.Permissions);
                saveConfig.Roles.Add(saveRole);
            }

            foreach (TCIUser user in _config.Users.OrderBy(u => u.Id, StringComparer.OrdinalIgnoreCase))
            {
                TCIUser saveUser = new TCIUser();
                saveUser.Id = user.Id;
                saveUser.DisplayName = user.DisplayName;
                saveUser.Enabled = user.Enabled;
                saveUser.Role = user.Role;
                saveUser.Permissions = cleanPermissionList(user.Permissions);
                saveUser.AccessCodeHash = user.AccessCodeHash;
                saveUser.AccessCodeSalt = user.AccessCodeSalt;
                saveUser.HashAlgorithm = string.IsNullOrWhiteSpace(user.HashAlgorithm) ? HASH_ALGORITHM : user.HashAlgorithm;
                saveUser.HashIterations = user.HashIterations <= 0 ? HASH_ITERATIONS : user.HashIterations;
                saveUser.CreatedUtc = user.CreatedUtc;
                saveUser.ModifiedUtc = user.ModifiedUtc;
                saveUser.Notes = user.Notes;
                saveUser.AccessCode = null;
                saveConfig.Users.Add(saveUser);
            }

            return saveConfig;
        }

        private static bool tryGetUserLocked(string id, out TCIUser user)
        {
            return _usersById.TryGetValue(normaliseId(id), out user);
        }

        private static bool isValidRoleLocked(string role)
        {
            return !string.IsNullOrWhiteSpace(role) && _rolesById.ContainsKey(role);
        }

        private static HashSet<string> getEffectivePermissionsLocked(TCIUser user)
        {
            HashSet<string> permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            TCIRole role;
            if (!string.IsNullOrWhiteSpace(user.Role) && _rolesById.TryGetValue(user.Role, out role))
            {
                foreach (string permission in cleanPermissionList(role.Permissions))
                    permissions.Add(permission);
            }

            foreach (string permission in cleanPermissionList(user.Permissions))
                permissions.Add(permission);

            return permissions;
        }

        private static List<string> cleanPermissionList(IEnumerable<string> permissions)
        {
            List<string> result = new List<string>();
            if (permissions == null) return result;

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string permission in permissions)
            {
                string cleaned = normalisePermission(permission);
                if (string.IsNullOrEmpty(cleaned)) continue;
                if (!isKnownPermission(cleaned)) continue;
                if (seen.Add(cleaned)) result.Add(cleaned);
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        private static bool isKnownPermission(string permission)
        {
            switch (permission)
            {
                case VIEW_STATUS:
                case VIEW_SPECTRUM:
                case LISTEN_RX_AUDIO:
                case CHANGE_RX_AUDIO:
                case CHANGE_FREQUENCY:
                case CHANGE_MODE:
                case CHANGE_RX_FILTER:
                case CHANGE_RX_DSP:
                case CHANGE_RX_FRONTEND:
                case CHANGE_DISPLAY:
                case TX_PTT:
                case TX_TUNE:
                case TX_POWER:
                case TX_SETTINGS:
                case TX_AUDIO:
                case CW_KEYER:
                case SPOTS:
                case RADIO_POWER:
                case ADMIN:
                    return true;
                default:
                    return false;
            }
        }

        private static string normalisePermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission)) return string.Empty;
            return permission.Trim().ToLowerInvariant();
        }

        private static string normaliseId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return string.Empty;
            return id.Trim().ToLowerInvariant();
        }

        private static string normaliseConnectionId(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId)) return "unknown";
            return connectionId.Trim();
        }

        private static string normaliseIp(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) return "unknown";
            return ipAddress.Trim();
        }

        private static string cleanDisplayName(string displayName, string fallback)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return fallback;
            return displayName.Trim();
        }

        private static string utcNowString()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        }

        private static void setUserAccessCodeLocked(TCIUser user, string accessCode)
        {
            byte[] salt = new byte[SALT_BYTES];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = hashAccessCode(accessCode ?? string.Empty, salt, HASH_ITERATIONS, HASH_BYTES);
            user.AccessCodeHash = Convert.ToBase64String(hash);
            user.AccessCodeSalt = Convert.ToBase64String(salt);
            user.HashAlgorithm = HASH_ALGORITHM;
            user.HashIterations = HASH_ITERATIONS;
            user.AccessCode = null;
        }

        private static bool hasStoredAccessCode(TCIUser user)
        {
            return user != null &&
                   !string.IsNullOrWhiteSpace(user.AccessCodeHash) &&
                   !string.IsNullOrWhiteSpace(user.AccessCodeSalt) &&
                   string.Equals(user.HashAlgorithm, HASH_ALGORITHM, StringComparison.OrdinalIgnoreCase) &&
                   user.HashIterations > 0;
        }

        private static bool verifyAccessCode(TCIUser user, string accessCode)
        {
            try
            {
                byte[] salt = Convert.FromBase64String(user.AccessCodeSalt);
                byte[] expectedHash = Convert.FromBase64String(user.AccessCodeHash);
                byte[] actualHash = hashAccessCode(accessCode ?? string.Empty, salt, user.HashIterations, expectedHash.Length);
                return fixedTimeEquals(expectedHash, actualHash);
            }
            catch
            {
                return false;
            }
        }

        private static byte[] hashAccessCode(string accessCode, byte[] salt, int iterations, int hashBytes)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(accessCode ?? string.Empty, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(hashBytes);
            }
        }

        private static bool fixedTimeEquals(byte[] expected, byte[] actual)
        {
            if (expected == null || actual == null) return false;
            int difference = expected.Length ^ actual.Length;
            int length = Math.Min(expected.Length, actual.Length);
            for (int i = 0; i < length; i++)
                difference |= expected[i] ^ actual[i];
            return difference == 0;
        }

        private static RuntimeCounter getCounterLocked(Dictionary<string, RuntimeCounter> counters, string key)
        {
            RuntimeCounter counter;
            if (!counters.TryGetValue(key, out counter))
            {
                counter = new RuntimeCounter();
                counters[key] = counter;
            }

            return counter;
        }

        private static int getLockedSeconds(RuntimeCounter counter)
        {
            if (counter == null) return 0;
            if (counter.LockedUntilUtc <= DateTime.UtcNow) return 0;
            return Math.Max(1, (int)Math.Ceiling((counter.LockedUntilUtc - DateTime.UtcNow).TotalSeconds));
        }

        private static string permissionsToCsv(IEnumerable<string> permissions)
        {
            if (permissions == null) return string.Empty;
            List<string> ordered = new List<string>(permissions);
            ordered.Sort(StringComparer.OrdinalIgnoreCase);
            return string.Join(",", ordered.ToArray());
        }
    }
}
