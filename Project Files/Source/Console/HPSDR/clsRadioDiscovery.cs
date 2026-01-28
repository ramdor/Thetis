/*  clsRadioDiscovery.cs

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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;

// -----------------------------------------------------------------------------
// Discovery timing guide
// -----------------------------------------------------------------------------
// AttemptsPerNic = how many send/wait cycles per NIC
// QuietPollsBeforeResend = how many consecutive empty polls before retry/give up
// PollTimeoutMilliseconds = how long each poll waits
//
// Effective max wait per NIC ≈ AttemptsPerNic × QuietPollsBeforeResend × PollTimeoutMilliseconds
//
// Profile        AttemptsPerNic   QuietPollsBeforeResend   PollTimeoutMilliseconds   Approx time per NIC
// -----------    ---------------   ----------------------   -----------------------   -------------------
// Ultra fast     1                 2                        40                        ~80 ms
// Very fast      1                 3                        60                        ~180 ms
// Fast           2                 3                        80                        ~480 ms
// Balanced       2                 4                        100                       ~800 ms
// Safe default   3                 5                        120                       ~1800 ms
// Very tolerant  4                 6                        150                       ~3600 ms
//
// Notes:
// - Lower values = faster scans, but higher risk of missing slow replies
// - Higher values = slower scans, but better on busy networks or slow switches
// - For known fixed IP, you can safely use Ultra fast or Very fast
// - For broadcast discovery on large networks, use Balanced or Safe default
// -----------------------------------------------------------------------------

namespace Thetis
{
    public enum RadioDiscoveryProtocolMode
    {
        Auto = 0,
        P1Only = 1,
        P2Only = 2
    }

    public enum RadioDiscoveryRadioProtocol
    {
        Unknown = 0,
        P1 = 1,
        P2 = 2
    }

    public enum ScanPerformanceProfile
    {
        UltraFast = 0,
        VeryFast = 1,
        Fast = 2,
        Balanced = 3,
        Safe = 4,
        VeryTolerant = 5
    }

    public sealed class RadioDiscoveryOptions
    {
        public bool IgnoreSubnetCheck { get; set; }
        public bool IncludeWireless { get; set; }
        public bool IncludeEthernet { get; set; }
        public bool IncludeOtherInterfaceTypes { get; set; }
        public bool AllowLoopback { get; set; }
        public bool AllowAPIPA { get; set; }

        public int DiscoveryPortBase { get; set; }
        public int BindLocalPort { get; set; }

        public int AttemptsPerNic { get; set; }
        public int PollTimeoutMilliseconds { get; set; }
        public int QuietPollsBeforeResend { get; set; }
        public bool Include255255255255Broadcast { get; set; }

        private ScanPerformanceProfile _scanPerformance = ScanPerformanceProfile.Balanced;
        public ScanPerformanceProfile ScanPerformance
        {
            get { return _scanPerformance; }
            set
            {
                _scanPerformance = value;
                applyScanPerformanceProfile(value);
            }
        }

        public RadioDiscoveryProtocolMode ProtocolMode { get; set; }

        public IPAddress FixedTargetIp { get; set; }
        public IPAddress FixedLocalIp { get; set; }

        public RadioDiscoveryOptions()
        {
            IgnoreSubnetCheck = false;
            IncludeWireless = true;
            IncludeEthernet = true;
            IncludeOtherInterfaceTypes = false;
            AllowLoopback = false;
            AllowAPIPA = true;

            DiscoveryPortBase = 1024;
            BindLocalPort = 0;

            Include255255255255Broadcast = true;

            //AttemptsPerNic = 4;
            //PollTimeoutMilliseconds = 150;
            //QuietPollsBeforeResend = 6;
            ScanPerformance = ScanPerformanceProfile.Balanced;

            ProtocolMode = RadioDiscoveryProtocolMode.Auto;

            FixedTargetIp = null;
            FixedLocalIp = null;
        }

        private void applyScanPerformanceProfile(ScanPerformanceProfile profile)
        {
            switch (profile)
            {
                case ScanPerformanceProfile.UltraFast:
                    AttemptsPerNic = 1;
                    QuietPollsBeforeResend = 2;
                    PollTimeoutMilliseconds = 40;
                    break;

                case ScanPerformanceProfile.VeryFast:
                    AttemptsPerNic = 1;
                    QuietPollsBeforeResend = 3;
                    PollTimeoutMilliseconds = 60;
                    break;

                case ScanPerformanceProfile.Fast:
                    AttemptsPerNic = 2;
                    QuietPollsBeforeResend = 3;
                    PollTimeoutMilliseconds = 80;
                    break;

                case ScanPerformanceProfile.Balanced:
                    AttemptsPerNic = 2;
                    QuietPollsBeforeResend = 4;
                    PollTimeoutMilliseconds = 100;
                    break;

                case ScanPerformanceProfile.Safe:
                    AttemptsPerNic = 3;
                    QuietPollsBeforeResend = 5;
                    PollTimeoutMilliseconds = 120;
                    break;

                case ScanPerformanceProfile.VeryTolerant:
                    AttemptsPerNic = 4;
                    QuietPollsBeforeResend = 6;
                    PollTimeoutMilliseconds = 150;
                    break;

                default:
                    AttemptsPerNic = 2;
                    QuietPollsBeforeResend = 4;
                    PollTimeoutMilliseconds = 100;
                    break;
            }
        }
    }

    public sealed class RadioInfo
    {
        public RadioDiscoveryRadioProtocol Protocol { get; set; }
        public IPAddress IpAddress { get; set; }
        public string MacAddress { get; set; }
        public HPSDRHW DeviceType { get; set; }
        public byte CodeVersion { get; set; }
        public byte BetaVersion { get; set; }
        public byte ProtocolSupported { get; set; }
        public byte NumRxs { get; set; }
        public byte MercuryVersion0 { get; set; }
        public byte MercuryVersion1 { get; set; }
        public byte MercuryVersion2 { get; set; }
        public byte MercuryVersion3 { get; set; }
        public byte PennyVersion { get; set; }
        public byte MetisVersion { get; set; }
        public bool IsBusy { get; set; }

        public int DiscoveryPortBase { get; set; }
        public int PortCount { get; set; }

        public bool IsApipaRadio { get; set; }
    }

    public sealed class DiscoveryDiagnostics
    {
        public long DurationMilliseconds { get; set; }
        public int AttemptsUsed { get; set; }
        public int Polls { get; set; }
        public int QuietPolls { get; set; }

        public int DiscoverySends { get; set; }
        public int DiscoveryReceives { get; set; }
        public int UniqueRadios { get; set; }

        public int RejectedSubnet { get; set; }
        public int RejectedDuplicate { get; set; }
        public int RejectedMacInvalid { get; set; }
        public int RejectedFixedTargetMismatch { get; set; }
        public int RejectedProtocolModeMismatch { get; set; }
    }

    public sealed class NicRadioScanResult
    {
        public string NicId { get; set; }
        public string NicName { get; set; }
        public string NicDescription { get; set; }
        public long NicSpeedBitsPerSecond { get; set; }

        public NetworkInterfaceType NicInterfaceType { get; set; }
        public bool IsEthernet { get; set; }
        public bool IsWireless { get; set; }

        public IPAddress LocalIPv4 { get; set; }
        public IPAddress LocalMaskIPv4 { get; set; }
        public string NicMacAddress { get; set; }
        public bool IsApipaLocal { get; set; }
        public bool IsLoopbackLocal { get; set; }

        public List<RadioInfo> Radios { get; set; }

        public IPAddress GatewayIPv4 { get; set; }
        public List<IPAddress> DnsServersIPv4 { get; set; }
        public bool IsDhcpEnabled { get; set; }
        public OperationalStatus NicStatus { get; set; }
        public int Mtu { get; set; }
        public DiscoveryDiagnostics Diagnostics { get; set; }

        public NicRadioScanResult()
        {
            Radios = new List<RadioInfo>();
            DnsServersIPv4 = new List<IPAddress>();
        }

        public string DisplayText
        {
            get
            {
                string type = IsEthernet ? "Ethernet" : (IsWireless ? "WiFi" : NicInterfaceType.ToString());
                string apipa = IsApipaLocal ? " APIPA" : "";
                string ip = LocalIPv4 != null ? LocalIPv4.ToString() : "";
                return NicDescription + " [" + type + apipa + "] " + ip;
            }
        }
    }

    public sealed class RadioDiscoveryService
    {
        private const int P1DefaultPortCount = 1;
        private const int P2DefaultPortCount = 18;

        private sealed class NicIpv4Binding
        {
            public NetworkInterface Nic;
            public IPAddress LocalIp;
            public IPAddress Mask;
        }

        public List<NicRadioScanResult> DiscoverUsingAllNics(RadioDiscoveryOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            List<NicRadioScanResult> results = new List<NicRadioScanResult>();
            List<NicIpv4Binding> bindings = enumerateNicIpv4Bindings(options);

            for (int i = 0; i < bindings.Count; i++)
            {
                NicIpv4Binding b = bindings[i];

                NicRadioScanResult nicResult = createNicResultSkeleton(b);
                hydrateNicNetworkProps(b, nicResult);
                sanitizeLoopbackNicFields(nicResult);

                DiscoveryDiagnostics diag;
                List<RadioInfo> radios = discoverOnNic(b.LocalIp, b.Mask, options, out diag);
                nicResult.Diagnostics = diag;

                for (int r = 0; r < radios.Count; r++)
                {
                    nicResult.Radios.Add(radios[r]);
                }

                results.Add(nicResult);
            }

            return results;
        }

        public NicRadioScanResult DiscoverUsingSingleNic(RadioDiscoveryOptions options, IPAddress localIPv4)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (localIPv4 == null)
            {
                throw new ArgumentNullException(nameof(localIPv4));
            }

            RadioDiscoveryOptions o = options;
            o.FixedLocalIp = localIPv4;

            List<NicIpv4Binding> bindings = enumerateNicIpv4Bindings(o);
            if (bindings.Count == 0)
            {
                return null;
            }

            NicIpv4Binding b = bindings[0];

            NicRadioScanResult nicResult = createNicResultSkeleton(b);
            hydrateNicNetworkProps(b, nicResult);
            sanitizeLoopbackNicFields(nicResult);

            DiscoveryDiagnostics diag;
            List<RadioInfo> radios = discoverOnNic(b.LocalIp, b.Mask, o, out diag);
            nicResult.Diagnostics = diag;

            for (int r = 0; r < radios.Count; r++)
            {
                nicResult.Radios.Add(radios[r]);
            }

            return nicResult;
        }

        public List<NicRadioScanResult> ListUsableNics(RadioDiscoveryOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            RadioDiscoveryOptions o = options;
            o.FixedLocalIp = null;

            List<NicRadioScanResult> results = new List<NicRadioScanResult>();
            List<NicIpv4Binding> bindings = enumerateNicIpv4Bindings(o);

            for (int i = 0; i < bindings.Count; i++)
            {
                NicIpv4Binding b = bindings[i];

                NicRadioScanResult nicResult = createNicResultSkeleton(b);
                hydrateNicNetworkProps(b, nicResult);
                sanitizeLoopbackNicFields(nicResult);
                nicResult.Diagnostics = null;

                results.Add(nicResult);
            }

            return results;
        }

        private NicRadioScanResult createNicResultSkeleton(NicIpv4Binding b)
        {
            NicRadioScanResult nicResult = new NicRadioScanResult();
            nicResult.NicId = b.Nic.Id;
            nicResult.NicName = b.Nic.Name;
            nicResult.NicDescription = b.Nic.Description;
            nicResult.NicSpeedBitsPerSecond = b.Nic.Speed;
            nicResult.LocalIPv4 = b.LocalIp;
            nicResult.LocalMaskIPv4 = b.Mask;
            nicResult.NicMacAddress = formatNicMac(b.Nic.GetPhysicalAddress());
            nicResult.NicInterfaceType = b.Nic.NetworkInterfaceType;
            nicResult.IsEthernet = b.Nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet;
            nicResult.IsWireless = b.Nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;
            nicResult.IsLoopbackLocal = IPAddress.IsLoopback(b.LocalIp);
            nicResult.IsApipaLocal = isApipa(b.LocalIp);
            nicResult.NicStatus = b.Nic.OperationalStatus;
            return nicResult;
        }

        private void hydrateNicNetworkProps(NicIpv4Binding b, NicRadioScanResult nicResult)
        {
            if (b == null || b.Nic == null || nicResult == null)
            {
                return;
            }

            IPInterfaceProperties props = b.Nic.GetIPProperties();
            if (props == null)
            {
                return;
            }

            if (props.GatewayAddresses != null)
            {
                for (int g = 0; g < props.GatewayAddresses.Count; g++)
                {
                    IPAddress gw = props.GatewayAddresses[g].Address;
                    if (gw != null && gw.AddressFamily == AddressFamily.InterNetwork)
                    {
                        nicResult.GatewayIPv4 = gw;
                        break;
                    }
                }
            }

            if (props.DnsAddresses != null)
            {
                for (int di = 0; di < props.DnsAddresses.Count; di++)
                {
                    IPAddress dns = props.DnsAddresses[di];
                    if (dns != null && dns.AddressFamily == AddressFamily.InterNetwork)
                    {
                        nicResult.DnsServersIPv4.Add(dns);
                    }
                }
            }

            IPv4InterfaceProperties v4 = props.GetIPv4Properties();
            if (v4 != null)
            {
                nicResult.IsDhcpEnabled = v4.IsDhcpEnabled;
                nicResult.Mtu = v4.Mtu;
            }
        }

        private void sanitizeLoopbackNicFields(NicRadioScanResult nicResult)
        {
            if (nicResult == null)
            {
                return;
            }

            if (!nicResult.IsLoopbackLocal && nicResult.NicInterfaceType != NetworkInterfaceType.Loopback)
            {
                return;
            }

            nicResult.NicSpeedBitsPerSecond = 0;
            nicResult.Mtu = 0;
            nicResult.GatewayIPv4 = null;

            if (nicResult.DnsServersIPv4 == null)
            {
                nicResult.DnsServersIPv4 = new List<IPAddress>();
            }
            else
            {
                nicResult.DnsServersIPv4.Clear();
            }
        }

        private string formatNicMac(PhysicalAddress pa)
        {
            if (pa == null)
            {
                return "N/A";
            }

            byte[] bytes = pa.GetAddressBytes();
            if (bytes == null || bytes.Length == 0)
            {
                return "N/A";
            }

            return BitConverter.ToString(bytes);
        }

        private List<NicIpv4Binding> enumerateNicIpv4Bindings(RadioDiscoveryOptions options)
        {
            List<NicIpv4Binding> list = new List<NicIpv4Binding>();

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            for (int i = 0; i < nics.Length; i++)
            {
                NetworkInterface nic = nics[i];
                if (!isNicCandidate(nic, options))
                {
                    continue;
                }

                IPInterfaceProperties props = nic.GetIPProperties();
                if (props == null)
                {
                    continue;
                }

                UnicastIPAddressInformationCollection unicast = props.UnicastAddresses;
                if (unicast == null)
                {
                    continue;
                }

                for (int j = 0; j < unicast.Count; j++)
                {
                    UnicastIPAddressInformation u = unicast[j];
                    if (u == null || u.Address == null)
                    {
                        continue;
                    }

                    if (u.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    if (!options.AllowLoopback && IPAddress.IsLoopback(u.Address))
                    {
                        continue;
                    }

                    if (!options.AllowAPIPA && isApipa(u.Address))
                    {
                        continue;
                    }

                    if (options.FixedLocalIp != null)
                    {
                        if (!u.Address.Equals(options.FixedLocalIp))
                        {
                            continue;
                        }
                    }

                    IPAddress mask = u.IPv4Mask;
                    if (mask == null)
                    {
                        mask = IPAddress.Parse("255.255.255.0");
                    }

                    NicIpv4Binding b = new NicIpv4Binding();
                    b.Nic = nic;
                    b.LocalIp = u.Address;
                    b.Mask = mask;

                    list.Add(b);
                }
            }

            return list;
        }

        private List<RadioInfo> discoverOnNic(IPAddress localIPv4, IPAddress localMaskIPv4, RadioDiscoveryOptions options, out DiscoveryDiagnostics diagnostics)
        {
            DiscoveryDiagnostics d = new DiscoveryDiagnostics();
            Stopwatch sw = Stopwatch.StartNew();

            List<RadioInfo> radios = new List<RadioInfo>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            Socket s = null;
            try
            {
                s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.EnableBroadcast = true;

                IPEndPoint bindEp = new IPEndPoint(localIPv4, options.BindLocalPort);
                s.Bind(bindEp);

                byte[] p1 = buildDiscoveryPacketP1();
                byte[] p2 = buildDiscoveryPacketP2();

                int attempts = options.AttemptsPerNic;
                if (attempts < 1) attempts = 1;

                int pollMs = options.PollTimeoutMilliseconds;
                if (pollMs < 10) pollMs = 10;

                int quietBeforeResend = options.QuietPollsBeforeResend;
                if (quietBeforeResend < 1) quietBeforeResend = 1;

                for (int attempt = 0; attempt < attempts; attempt++)
                {
                    d.AttemptsUsed++;

                    List<IPEndPoint> targets = buildTargets(localIPv4, localMaskIPv4, options);

                    for (int t = 0; t < targets.Count; t++)
                    {
                        IPEndPoint dest = targets[t];

                        if (options.ProtocolMode == RadioDiscoveryProtocolMode.Auto || options.ProtocolMode == RadioDiscoveryProtocolMode.P1Only)
                        {
                            s.SendTo(p1, dest);
                            d.DiscoverySends++;
                        }

                        if (options.ProtocolMode == RadioDiscoveryProtocolMode.Auto || options.ProtocolMode == RadioDiscoveryProtocolMode.P2Only)
                        {
                            s.SendTo(p2, dest);
                            d.DiscoverySends++;
                        }
                    }

                    int quietPolls = 0;

                    while (quietPolls < quietBeforeResend)
                    {
                        d.Polls++;

                        bool readable = s.Poll(pollMs * 1000, SelectMode.SelectRead);
                        if (!readable)
                        {
                            quietPolls++;
                            d.QuietPolls++;
                            continue;
                        }

                        EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                        byte[] rx = new byte[256];
                        int recv = s.ReceiveFrom(rx, ref remote);
                        if (recv <= 0)
                        {
                            continue;
                        }

                        d.DiscoveryReceives++;

                        IPEndPoint rep = remote as IPEndPoint;
                        if (rep == null || rep.Address == null)
                        {
                            continue;
                        }

                        DiscoveryParseResult parsed = parseDiscoveryReply(rx, recv, rep.Address, options);

                        if (!parsed.IsDiscovery && !parsed.IsBusy)
                        {
                            continue;
                        }

                        if (options.ProtocolMode == RadioDiscoveryProtocolMode.P1Only && parsed.Protocol != RadioDiscoveryRadioProtocol.P1)
                        {
                            d.RejectedProtocolModeMismatch++;
                            continue;
                        }

                        if (options.ProtocolMode == RadioDiscoveryProtocolMode.P2Only && parsed.Protocol != RadioDiscoveryRadioProtocol.P2)
                        {
                            d.RejectedProtocolModeMismatch++;
                            continue;
                        }

                        if (!options.IgnoreSubnetCheck)
                        {
                            bool okSubnet = sameSubnet(rep.Address, localIPv4, localMaskIPv4);
                            if (!okSubnet)
                            {
                                d.RejectedSubnet++;
                                continue;
                            }
                        }

                        if (parsed.MacAddress == null || parsed.MacAddress.Length == 0 || parsed.MacAddress.Equals("00-00-00-00-00-00", StringComparison.OrdinalIgnoreCase))
                        {
                            d.RejectedMacInvalid++;
                            continue;
                        }

                        if (options.FixedTargetIp != null && !rep.Address.Equals(options.FixedTargetIp))
                        {
                            d.RejectedFixedTargetMismatch++;
                            continue;
                        }

                        string key = parsed.Protocol.ToString() + "|" + rep.Address.ToString() + "|" + parsed.MacAddress;
                        if (seen.Contains(key))
                        {
                            d.RejectedDuplicate++;
                            continue;
                        }

                        RadioInfo info = new RadioInfo();
                        info.Protocol = parsed.Protocol;
                        info.IpAddress = rep.Address;
                        info.MacAddress = parsed.MacAddress;
                        info.DeviceType = parsed.DeviceType;
                        info.CodeVersion = parsed.CodeVersion;
                        info.BetaVersion = parsed.BetaVersion;
                        info.ProtocolSupported = parsed.ProtocolSupported;
                        info.NumRxs = parsed.NumRxs;
                        info.MercuryVersion0 = parsed.MercuryVersion0;
                        info.MercuryVersion1 = parsed.MercuryVersion1;
                        info.MercuryVersion2 = parsed.MercuryVersion2;
                        info.MercuryVersion3 = parsed.MercuryVersion3;
                        info.PennyVersion = parsed.PennyVersion;
                        info.MetisVersion = parsed.MetisVersion;
                        info.IsBusy = parsed.IsBusy;
                        info.DiscoveryPortBase = options.DiscoveryPortBase;
                        info.PortCount = parsed.Protocol == RadioDiscoveryRadioProtocol.P2 ? P2DefaultPortCount : P1DefaultPortCount;
                        info.IsApipaRadio = isApipa(rep.Address);

                        radios.Add(info);
                        seen.Add(key);
                    }
                }
            }
            catch
            {
                diagnostics = d;
                return radios;
            }
            finally
            {
                sw.Stop();
                d.DurationMilliseconds = sw.ElapsedMilliseconds;
                d.UniqueRadios = radios.Count;

                if (s != null)
                {
                    try
                    {
                        s.Close();
                    }
                    catch
                    {
                    }
                }
            }

            diagnostics = d;
            return radios;
        }

        private sealed class DiscoveryParseResult
        {
            public bool IsDiscovery { get; set; }
            public bool IsBusy { get; set; }
            public RadioDiscoveryRadioProtocol Protocol { get; set; }
            public string MacAddress { get; set; }

            public HPSDRHW DeviceType { get; set; }
            public byte CodeVersion { get; set; }
            public byte BetaVersion { get; set; }
            public byte ProtocolSupported { get; set; }
            public byte NumRxs { get; set; }
            public byte MercuryVersion0 { get; set; }
            public byte MercuryVersion1 { get; set; }
            public byte MercuryVersion2 { get; set; }
            public byte MercuryVersion3 { get; set; }
            public byte PennyVersion { get; set; }
            public byte MetisVersion { get; set; }
        }

        private DiscoveryParseResult parseDiscoveryReply(byte[] data, int len, IPAddress senderIp, RadioDiscoveryOptions options)
        {
            DiscoveryParseResult r = new DiscoveryParseResult();
            r.IsDiscovery = false;
            r.IsBusy = false;
            r.Protocol = RadioDiscoveryRadioProtocol.Unknown;

            if (data == null || len < 24)
            {
                return r;
            }

            bool p1 = (data[0] == 0xef && data[1] == 0xfe && (data[2] == 0x2 || data[2] == 0x3));
            bool p2 = (data[0] == 0x0 && data[1] == 0x0 && data[2] == 0x0 && data[3] == 0x0 && (data[4] == 0x2 || data[4] == 0x3));

            if (!p1 && !p2)
            {
                return r;
            }

            r.IsDiscovery = true;

            if (p1)
            {
                r.Protocol = RadioDiscoveryRadioProtocol.P1;
                r.IsBusy = (data[2] == 0x3);

                byte[] mac = new byte[6];
                Array.Copy(data, 3, mac, 0, 6);
                r.MacAddress = BitConverter.ToString(mac);

                r.DeviceType = mapP1DeviceType(data[10]);
                r.ProtocolSupported = 0;
                r.CodeVersion = data[9];
                r.BetaVersion = 0;

                if (len > 20)
                {
                    r.MercuryVersion0 = data[14];
                    r.MercuryVersion1 = data[15];
                    r.MercuryVersion2 = data[16];
                    r.MercuryVersion3 = data[17];
                    r.PennyVersion = data[18];
                    r.MetisVersion = data[19];
                    r.NumRxs = data[20];
                }

                return r;
            }

            if (p2)
            {
                r.Protocol = RadioDiscoveryRadioProtocol.P2;
                r.IsBusy = (data[4] == 0x3);

                byte[] mac = new byte[6];
                Array.Copy(data, 5, mac, 0, 6);
                r.MacAddress = BitConverter.ToString(mac);

                r.DeviceType = (HPSDRHW)data[11];
                r.ProtocolSupported = data[12];
                r.CodeVersion = data[13];
                r.BetaVersion = len > 23 ? data[23] : (byte)0;

                if (len > 20)
                {
                    r.MercuryVersion0 = data[14];
                    r.MercuryVersion1 = data[15];
                    r.MercuryVersion2 = data[16];
                    r.MercuryVersion3 = data[17];
                    r.PennyVersion = data[18];
                    r.MetisVersion = data[19];
                    r.NumRxs = data[20];
                }

                return r;
            }

            return r;
        }

        private HPSDRHW mapP1DeviceType(byte boardId)
        {
            if (boardId == 0) return HPSDRHW.Atlas;
            if (boardId == 1) return HPSDRHW.Hermes;
            if (boardId == 2) return HPSDRHW.HermesII;
            if (boardId == 4) return HPSDRHW.Angelia;
            if (boardId == 5) return HPSDRHW.Orion;
            if (boardId == 10) return HPSDRHW.OrionMKII;
            return (HPSDRHW)boardId;
        }

        private List<IPEndPoint> buildTargets(IPAddress localIPv4, IPAddress mask, RadioDiscoveryOptions options)
        {
            List<IPEndPoint> targets = new List<IPEndPoint>();

            int port = options.DiscoveryPortBase;
            if (port < 1) port = 1024;

            if (options.FixedTargetIp != null)
            {
                targets.Add(new IPEndPoint(options.FixedTargetIp, port));
                return targets;
            }

            IPAddress directedBroadcast = getBroadcastAddress(localIPv4, mask);
            targets.Add(new IPEndPoint(directedBroadcast, port));

            if (options.Include255255255255Broadcast)
            {
                IPAddress limitedBroadcast = IPAddress.Broadcast;
                bool alreadyAdded = false;
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].Address != null && targets[i].Address.Equals(limitedBroadcast))
                    {
                        alreadyAdded = true;
                        break;
                    }
                }

                if (!alreadyAdded)
                {
                    targets.Add(new IPEndPoint(limitedBroadcast, port));
                }
            }

            return targets;
        }


        private byte[] buildDiscoveryPacketP1()
        {
            byte[] p = new byte[63];
            Array.Clear(p, 0, p.Length);
            p[0] = 0xef;
            p[1] = 0xfe;
            p[2] = 0x02;
            return p;
        }

        private byte[] buildDiscoveryPacketP2()
        {
            byte[] p = new byte[60];
            Array.Clear(p, 0, p.Length);
            p[4] = 0x02;
            return p;
        }

        private bool isNicCandidate(NetworkInterface nic, RadioDiscoveryOptions options)
        {
            if (nic == null)
            {
                return false;
            }

            if (nic.OperationalStatus != OperationalStatus.Up && nic.OperationalStatus != OperationalStatus.Unknown)
            {
                return false;
            }

            NetworkInterfaceType t = nic.NetworkInterfaceType;

            if (t == NetworkInterfaceType.Loopback)
            {
                return options.AllowLoopback;
            }

            if (t == NetworkInterfaceType.Wireless80211)
            {
                return options.IncludeWireless;
            }

            if (t == NetworkInterfaceType.Ethernet)
            {
                return options.IncludeEthernet;
            }

            if (options.IncludeOtherInterfaceTypes)
            {
                return true;
            }

            return false;
        }

        private bool sameSubnet(IPAddress a, IPAddress b, IPAddress mask)
        {
            if (a == null || b == null || mask == null)
            {
                return false;
            }

            byte[] ab = a.GetAddressBytes();
            byte[] bb = b.GetAddressBytes();
            byte[] mb = mask.GetAddressBytes();

            if (ab.Length != 4 || bb.Length != 4 || mb.Length != 4)
            {
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                int ax = ab[i] & mb[i];
                int bx = bb[i] & mb[i];
                if (ax != bx)
                {
                    return false;
                }
            }

            return true;
        }

        private bool isApipa(IPAddress ip)
        {
            if (ip == null)
            {
                return false;
            }

            byte[] b = ip.GetAddressBytes();
            if (b.Length != 4)
            {
                return false;
            }

            return (b[0] == 169 && b[1] == 254);
        }

        private IPAddress getBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            if (address == null)
            {
                return IPAddress.Broadcast;
            }

            if (subnetMask == null)
            {
                return IPAddress.Broadcast;
            }

            byte[] ip = address.GetAddressBytes();
            byte[] mask = subnetMask.GetAddressBytes();

            if (ip.Length != 4 || mask.Length != 4)
            {
                return IPAddress.Broadcast;
            }

            byte[] broadcast = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                broadcast[i] = (byte)(ip[i] | (mask[i] ^ 255));
            }

            return new IPAddress(broadcast);
        }
    }
}
