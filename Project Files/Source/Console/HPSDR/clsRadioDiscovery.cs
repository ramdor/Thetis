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
// Safe default   3                 5                        150                       ~1800 ms
// Very tolerant  3                 6                        300                       ~5400 ms
//
// Notes:
// - Lower values = faster scans, but higher risk of missing slow replies
// - Higher values = slower scans, but better on busy networks or slow switches
// - For known fixed IP, you can safely use Ultra fast or Very fast
// - For broadcast discovery on large networks, use Balanced or Safe default
// -----------------------------------------------------------------------------

namespace Thetis
{
    //IfTypeSyntax from https://www.iana.org/assignments/ianaiftype-mib/ianaiftype-mib
    public enum IfTypeSyntax : int
    {
        Other = 1, // none of the following
        Regular1822 = 2,
        Hdh1822 = 3,
        DdnX25 = 4,
        Rfc877X25 = 5,
        EthernetCsmacd = 6, // for all ethernet-like interfaces, regardless of speed, as per RFC3635
        Iso88023Csmacd = 7, // Deprecated via RFC3635; ethernetCsmacd (6) should be used instead
        Iso88024TokenBus = 8,
        Iso88025TokenRing = 9,
        Iso88026Man = 10,
        StarLan = 11, // Deprecated via RFC3635; ethernetCsmacd (6) should be used instead
        Proteon10Mbit = 12,
        Proteon80Mbit = 13,
        Hyperchannel = 14,
        Fddi = 15,
        Lapb = 16,
        Sdlc = 17,
        Ds1 = 18, // DS1-MIB
        E1 = 19, // Obsolete see DS1-MIB
        BasicIsdn = 20, // no longer used; see also RFC2127
        PrimaryIsdn = 21, // no longer used; see also RFC2127
        PropPointToPointSerial = 22, // proprietary serial
        Ppp = 23,
        SoftwareLoopback = 24,
        Eon = 25, // CLNP over IP
        Ethernet3Mbit = 26,
        Nsip = 27, // XNS over IP
        Slip = 28, // generic SLIP
        Ultra = 29, // ULTRA technologies
        Ds3 = 30, // DS3-MIB
        Sip = 31, // SMDS, coffee
        FrameRelay = 32, // DTE only
        Rs232 = 33,
        Para = 34, // parallel-port
        Arcnet = 35, // arcnet
        ArcnetPlus = 36, // arcnet plus
        Atm = 37, // ATM cells
        Miox25 = 38,
        Sonet = 39, // SONET or SDH
        X25Ple = 40,
        Iso88022Llc = 41,
        LocalTalk = 42,
        SmdsDxi = 43,
        FrameRelayService = 44, // FRNETSERV-MIB
        V35 = 45,
        Hssi = 46,
        Hippi = 47,
        Modem = 48, // Generic modem
        Aal5 = 49, // AAL5 over ATM
        SonetPath = 50,
        SonetVt = 51,
        SmdsIcip = 52, // SMDS InterCarrier Interface
        PropVirtual = 53, // proprietary virtual/internal
        PropMultiplexor = 54, // proprietary multiplexing
        Ieee80212 = 55, // 100BaseVG
        FibreChannel = 56, // Fibre Channel
        HippiInterface = 57, // HIPPI interfaces
        FrameRelayInterconnect = 58, // Obsolete, use either frameRelay(32) or frameRelayService(44)
        Aflane8023 = 59, // ATM Emulated LAN for 802.3
        Aflane8025 = 60, // ATM Emulated LAN for 802.5
        CctEmul = 61, // ATM Emulated circuit
        FastEther = 62, // Obsoleted via RFC3635; ethernetCsmacd (6) should be used instead
        Isdn = 63, // ISDN and X.25
        V11 = 64, // CCITT V.11/X.21
        V36 = 65, // CCITT V.36
        G703At64k = 66, // CCITT G703 at 64Kbps
        G703At2mb = 67, // Obsolete see DS1-MIB
        Qllc = 68, // SNA QLLC
        FastEtherFx = 69, // Obsoleted via RFC3635; ethernetCsmacd (6) should be used instead
        Channel = 70, // channel
        Ieee80211 = 71, // radio spread spectrum
        Ibm370ParChan = 72, // IBM System 360/370 OEMI Channel
        Escon = 73, // IBM Enterprise Systems Connection
        Dlsw = 74, // Data Link Switching
        Isdns = 75, // ISDN S/T interface
        Isdnu = 76, // ISDN U interface
        Lapd = 77, // Link Access Protocol D
        IpSwitch = 78, // IP Switching Objects
        Rsrb = 79, // Remote Source Route Bridging
        AtmLogical = 80, // ATM Logical Port
        Ds0 = 81, // Digital Signal Level 0
        Ds0Bundle = 82, // group of ds0s on the same ds1
        Bsc = 83, // Bisynchronous Protocol
        Async = 84, // Asynchronous Protocol
        Cnr = 85, // Combat Net Radio
        Iso88025Dtr = 86, // ISO 802.5r DTR
        Eplrs = 87, // Ext Pos Loc Report Sys
        Arap = 88, // Appletalk Remote Access Protocol
        PropCnls = 89, // Proprietary Connectionless Protocol
        HostPad = 90, // CCITT-ITU X.29 PAD Protocol
        TermPad = 91, // CCITT-ITU X.3 PAD Facility
        FrameRelayMpi = 92, // Multiproto Interconnect over FR
        X213 = 93, // CCITT-ITU X213
        Adsl = 94, // Asymmetric Digital Subscriber Loop
        Radsl = 95, // Rate-Adapt. Digital Subscriber Loop
        Sdsl = 96, // Symmetric Digital Subscriber Loop
        Vdsl = 97, // Very H-Speed Digital Subscrib. Loop
        Iso88025CrfpInt = 98, // ISO 802.5 CRFP
        Myrinet = 99, // Myricom Myrinet
        VoiceEm = 100, // voice recEive and transMit
        VoiceFxo = 101, // voice Foreign Exchange Office
        VoiceFxs = 102, // voice Foreign Exchange Station
        VoiceEncap = 103, // voice encapsulation
        VoiceOverIp = 104, // voice over IP encapsulation
        AtmDxi = 105, // ATM DXI
        AtmFuni = 106, // ATM FUNI
        AtmIma = 107, // ATM IMA
        PppMultilinkBundle = 108, // PPP Multilink Bundle
        IpOverCdlc = 109, // IBM ipOverCdlc
        IpOverClaw = 110, // IBM Common Link Access to Workstn
        StackToStack = 111, // IBM stackToStack
        VirtualIpAddress = 112, // IBM VIPA
        Mpc = 113, // IBM multi-protocol channel support
        IpOverAtm = 114, // IBM ipOverAtm
        Iso88025Fiber = 115, // ISO 802.5j Fiber Token Ring
        Tdlc = 116, // IBM twinaxial data link control
        GigabitEthernet = 117, // Obsoleted via RFC3635; ethernetCsmacd (6) should be used instead
        Hdlc = 118, // HDLC
        Lapf = 119, // LAP F
        V37 = 120, // V.37
        X25Mlp = 121, // Multi-Link Protocol
        X25HuntGroup = 122, // X25 Hunt Group
        TranspHdlc = 123, // Transp HDLC
        Interleave = 124, // Interleave channel
        Fast = 125, // Fast channel
        Ip = 126, // IP (for APPN HPR in IP networks)
        DocsCableMaclayer = 127, // CATV Mac Layer
        DocsCableDownstream = 128, // CATV Downstream interface
        DocsCableUpstream = 129, // CATV Upstream interface
        A12MppSwitch = 130, // Avalon Parallel Processor
        Tunnel = 131, // Encapsulation interface
        Coffee = 132, // coffee pot
        Ces = 133, // Circuit Emulation Service
        AtmSubInterface = 134, // ATM Sub Interface
        L2Vlan = 135, // Layer 2 Virtual LAN using 802.1Q
        L3Ipvlan = 136, // Layer 3 Virtual LAN using IP
        L3Ipxvlan = 137, // Layer 3 Virtual LAN using IPX
        DigitalPowerline = 138, // IP over Power Lines
        MediaMailOverIp = 139, // Multimedia Mail over IP
        Dtm = 140, // Dynamic syncronous Transfer Mode
        Dcn = 141, // Data Communications Network
        IpForward = 142, // IP Forwarding Interface
        Msdsl = 143, // Multi-rate Symmetric DSL
        Ieee1394 = 144, // IEEE1394 High Performance Serial Bus
        IfGsn = 145, // HIPPI-6400
        DvbRccMacLayer = 146, // DVB-RCC MAC Layer
        DvbRccDownstream = 147, // DVB-RCC Downstream Channel
        DvbRccUpstream = 148, // DVB-RCC Upstream Channel
        AtmVirtual = 149, // ATM Virtual Interface
        MplsTunnel = 150, // MPLS Tunnel Virtual Interface
        Srp = 151, // Spatial Reuse Protocol
        VoiceOverAtm = 152, // Voice Over ATM
        VoiceOverFrameRelay = 153, // Voice Over Frame Relay
        Idsl = 154, // Digital Subscriber Loop over ISDN
        CompositeLink = 155, // Avici Composite Link Interface
        Ss7SigLink = 156, // SS7 Signaling Link
        PropWirelessP2P = 157, // Prop. P2P wireless interface
        FrForward = 158, // Frame Forward Interface
        Rfc1483 = 159, // Multiprotocol over ATM AAL5
        Usb = 160, // USB Interface
        Ieee8023AdLag = 161, // IEEE 802.3ad Link Aggregate
        Bgppolicyaccounting = 162, // BGP Policy Accounting
        Frf16MfrBundle = 163, // FRF .16 Multilink Frame Relay
        H323Gatekeeper = 164, // H323 Gatekeeper
        H323Proxy = 165, // H323 Voice and Video Proxy
        Mpls = 166, // MPLS
        MfSigLink = 167, // Multi-frequency signaling link
        Hdsl2 = 168, // High Bit-Rate DSL - 2nd generation
        Shdsl = 169, // Multirate HDSL2
        Ds1Fdl = 170, // Facility Data Link 4Kbps on a DS1
        Pos = 171, // Packet over SONET/SDH Interface
        DvbAsiIn = 172, // DVB-ASI Input
        DvbAsiOut = 173, // DVB-ASI Output
        Plc = 174, // Power Line Communtications
        Nfas = 175, // Non Facility Associated Signaling
        Tr008 = 176, // TR008
        Gr303Rdt = 177, // Remote Digital Terminal
        Gr303Idt = 178, // Integrated Digital Terminal
        Isup = 179, // ISUP
        PropDocsWirelessMaclayer = 180, // Cisco proprietary Maclayer
        PropDocsWirelessDownstream = 181, // Cisco proprietary Downstream
        PropDocsWirelessUpstream = 182, // Cisco proprietary Upstream
        Hiperlan2 = 183, // HIPERLAN Type 2 Radio Interface
        PropBWAp2Mp = 184, // PropBroadbandWirelessAccesspt2multipt; IEEE 802.16f use deprecated; ifType 237 should be used instead
        SonetOverheadChannel = 185, // SONET Overhead Channel
        DigitalWrapperOverheadChannel = 186, // Digital Wrapper
        Aal2 = 187, // ATM adaptation layer 2
        RadioMac = 188, // MAC layer over radio links
        AtmRadio = 189, // ATM over radio links
        Imt = 190, // Inter Machine Trunks
        Mvl = 191, // Multiple Virtual Lines DSL
        ReachDsl = 192, // Long Reach DSL
        FrDlciEndPt = 193, // Frame Relay DLCI End Point
        AtmVciEndPt = 194, // ATM VCI End Point
        OpticalChannel = 195, // Optical Channel
        OpticalTransport = 196, // Optical Transport
        PropAtm = 197, // Proprietary ATM
        VoiceOverCable = 198, // Voice Over Cable Interface
        Infiniband = 199, // Infiniband
        TeLink = 200, // TE Link
        Q2931 = 201, // Q.2931
        VirtualTg = 202, // Virtual Trunk Group
        SipTg = 203, // SIP Trunk Group
        SipSig = 204, // SIP Signaling
        DocsCableUpstreamChannel = 205, // CATV Upstream Channel
        Econet = 206, // Acorn Econet
        Pon155 = 207, // FSAN 155Mb Symetrical PON interface
        Pon622 = 208, // FSAN622Mb Symetrical PON interface
        Bridge = 209, // Transparent bridge interface
        Linegroup = 210, // Interface common to multiple lines
        VoiceEmFgd = 211, // voice E&M Feature Group D
        VoiceFgdEana = 212, // voice FGD Exchange Access North American
        VoiceDid = 213, // voice Direct Inward Dialing
        MpegTransport = 214, // MPEG transport interface
        SixToFour = 215, // 6to4 interface (DEPRECATED)
        Gtp = 216, // GTP (GPRS Tunneling Protocol)
        PdnEtherLoop1 = 217, // Paradyne EtherLoop 1
        PdnEtherLoop2 = 218, // Paradyne EtherLoop 2
        OpticalChannelGroup = 219, // Optical Channel Group
        Homepna = 220, // HomePNA ITU-T G.989
        Gfp = 221, // Generic Framing Procedure (GFP)
        CiscoIslVlan = 222, // Layer 2 Virtual LAN using Cisco ISL
        ActelisMetaLoop = 223, // Acteleis proprietary MetaLOOP High Speed Link
        FcipLink = 224, // FCIP Link
        Rpr = 225, // Resilient Packet Ring Interface Type
        Qam = 226, // RF Qam Interface
        Lmp = 227, // Link Management Protocol
        CblVectaStar = 228, // Cambridge Broadband Networks Limited VectaStar
        DocsCableMCmtsDownstream = 229, // CATV Modular CMTS Downstream Interface
        Adsl2 = 230, // Asymmetric Digital Subscriber Loop Version 2 (DEPRECATED/OBSOLETED - please use adsl2plus 238 instead)
        MacSecControlledIf = 231, // MACSecControlled
        MacSecUncontrolledIf = 232, // MACSecUncontrolled
        AviciOpticalEther = 233, // Avici Optical Ethernet Aggregate
        Atmbond = 234, // atmbond
        VoiceFgdOs = 235, // voice FGD Operator Services
        MocaVersion1 = 236, // MultiMedia over Coax Alliance (MoCA) Interface; documented privately to IANA
        Ieee80216Wman = 237, // IEEE 802.16 WMAN interface
        Adsl2Plus = 238, // Asymmetric Digital Subscriber Loop Version 2, Version 2 Plus and all variants
        DvbRcsMacLayer = 239, // DVB-RCS MAC Layer
        DvbTdm = 240, // DVB Satellite TDM
        DvbRcsTdma = 241, // DVB-RCS TDMA
        X86Laps = 242, // LAPS based on ITU-T X.86/Y.1323
        WwanPp = 243, // 3GPP WWAN
        WwanPp2 = 244, // 3GPP2 WWAN
        VoiceEbs = 245, // voice P-phone EBS physical interface
        IfPwType = 246, // Pseudowire interface type
        Ilan = 247, // Internal LAN on a bridge per IEEE 802.1ap
        Pip = 248, // Provider Instance Port on a bridge per IEEE 802.1ah PBB
        AluElp = 249, // Alcatel-Lucent Ethernet Link Protection
        Gpon = 250, // Gigabit-capable passive optical networks (G-PON) as per ITU-T G.984
        Vdsl2 = 251, // Very high speed digital subscriber line Version 2 (as per ITU-T Recommendation G.993.2)
        CapwapDot11Profile = 252, // WLAN Profile Interface
        CapwapDot11Bss = 253, // WLAN BSS Interface
        CapwapWtpVirtualRadio = 254, // WTP Virtual Radio Interface
        Bits = 255, // bitsport
        DocsCableUpstreamRfPort = 256, // DOCSIS CATV Upstream RF Port
        CableDownstreamRfPort = 257, // CATV downstream RF port
        VmwareVirtualNic = 258, // VMware Virtual Network Interface
        Ieee802154 = 259, // IEEE 802.15.4 WPAN interface
        OtnOdu = 260, // OTN Optical Data Unit
        OtnOtu = 261, // OTN Optical channel Transport Unit
        IfVfiType = 262, // VPLS Forwarding Instance Interface Type
        G9981 = 263, // G.998.1 bonded interface
        G9982 = 264, // G.998.2 bonded interface
        G9983 = 265, // G.998.3 bonded interface
        AluEpon = 266, // Ethernet Passive Optical Networks (E-PON)
        AluEponOnu = 267, // EPON Optical Network Unit
        AluEponPhysicalUni = 268, // EPON physical User to Network interface
        AluEponLogicalLink = 269, // The emulation of a point-to-point link over the EPON layer
        AluGponOnu = 270, // GPON Optical Network Unit
        AluGponPhysicalUni = 271, // GPON physical User to Network interface
        VmwareNicTeam = 272, // VMware NIC Team
        DocsOfdmDownstream = 277, // CATV Downstream OFDM interface
        DocsOfdmaUpstream = 278, // CATV Upstream OFDMA interface
        Gfast = 279, // G.fast port
        Sdci = 280, // SDCI (IO-Link)
        XboxWireless = 281, // Xbox wireless
        FastDsl = 282, // FastDSL
        DocsCableScte55d1FwdOob = 283, // Cable SCTE 55-1 OOB Forward Channel
        DocsCableScte55d1RetOob = 284, // Cable SCTE 55-1 OOB Return Channel
        DocsCableScte55d2DsOob = 285, // Cable SCTE 55-2 OOB Downstream Channel
        DocsCableScte55d2UsOob = 286, // Cable SCTE 55-2 OOB Upstream Channel
        DocsCableNdf = 287, // Cable Narrowband Digital Forward
        DocsCableNdr = 288, // Cable Narrowband Digital Return
        Ptm = 289, // Packet Transfer Mode
        Ghn = 290, // G.hn port
        OtnOtsi = 291, // Optical Tributary Signal
        OtnOtuc = 292, // OTN OTUCn
        OtnOduc = 293, // OTN ODUC
        OtnOtsig = 294, // OTN OTUC Signal
        MicrowaveCarrierTermination = 295, // air interface of a single microwave carrier
        MicrowaveRadioLinkTerminal = 296, // radio link interface for one or several aggregated microwave carriers
        Ieee8021AxDrni = 297, // IEEE 802.1AX Distributed Resilient Network Interface
        Ax25 = 298, // AX.25 network interfaces
        Ieee19061Nanocom = 299, // Nanoscale and Molecular Communication
        Cpri = 300, // Common Public Radio Interface
        Omni = 301, // Overlay Multilink Network Interface (OMNI)
        Roe = 302, // Radio over Ethernet Interface
        P2POverLan = 303 // Point to Point over LAN interface
    }

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
        public bool IncludeGeneralBroadcast { get; set; }

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

            IncludeGeneralBroadcast = true;

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
                    QuietPollsBeforeResend = 4;
                    PollTimeoutMilliseconds = 150;
                    break;

                case ScanPerformanceProfile.VeryTolerant:
                    AttemptsPerNic = 3;
                    QuietPollsBeforeResend = 6;
                    PollTimeoutMilliseconds = 300;
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
        public byte Protocol2Supported { get; set; }
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

        public bool IsCustom { get; set; }
        public string CustomGuid { get; set; }
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

        public bool SocketError { get; set; }
    }

    public sealed class NicRadioScanResult
    {
        public string NicId { get; set; }
        public string NicName { get; set; }
        public string NicDescription { get; set; }
        public long NicSpeedBitsPerSecond { get; set; }

        public NetworkInterfaceType NicInterfaceType { get; set; }
        public string NicInterfaceTypeString
        {
            get
            {
                bool is_valid;
                int ifType = (int)NicInterfaceType;                

                is_valid = Enum.IsDefined(typeof(NetworkInterfaceType), ifType);
                if (is_valid) return ((NetworkInterfaceType)ifType).ToString();
                is_valid = Enum.IsDefined(typeof(IfTypeSyntax), ifType);
                if (is_valid) return ((IfTypeSyntax)ifType).ToString();
                return ifType.ToString();
            }
        }
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
                        info.Protocol2Supported = parsed.ProtocolSupported;
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

                        info.IsCustom = false;
                        info.CustomGuid = "";

                        radios.Add(info);
                        seen.Add(key);
                    }
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                d.SocketError = true;
                diagnostics = d;
                return radios;
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

        //private List<IPEndPoint> buildTargets(IPAddress localIPv4, IPAddress mask, RadioDiscoveryOptions options)
        //{
        //    List<IPEndPoint> targets = new List<IPEndPoint>();

        //    int port = options.DiscoveryPortBase;
        //    if (port < 1) port = 1024;

        //    if (options.FixedTargetIp != null)
        //    {
        //        targets.Add(new IPEndPoint(options.FixedTargetIp, port));
        //    }

        //    IPAddress directedSubnetBroadcast = getBroadcastAddress(localIPv4, mask);
        //    bool haveDirectedSubnetBroadcast = false;
        //    for (int i = 0; i < targets.Count; i++)
        //    {
        //        if (targets[i].Address != null && targets[i].Address.Equals(directedSubnetBroadcast))
        //        {
        //            haveDirectedSubnetBroadcast = true;
        //            break;
        //        }
        //    }
        //    if (!haveDirectedSubnetBroadcast)
        //    {
        //        targets.Add(new IPEndPoint(directedSubnetBroadcast, port));
        //    }

        //    if (options.IncludeGeneralBroadcast) // to 255.255.255.255
        //    {
        //        IPAddress generalBroadcast = IPAddress.Broadcast;
        //        bool haveGeneralBroadcast = false;
        //        for (int i = 0; i < targets.Count; i++)
        //        {
        //            if (targets[i].Address != null && targets[i].Address.Equals(generalBroadcast))
        //            {
        //                haveGeneralBroadcast = true;
        //                break;
        //            }
        //        }
        //        if (!haveGeneralBroadcast)
        //        {
        //            targets.Add(new IPEndPoint(generalBroadcast, port));
        //        }
        //    }

        //    return targets;
        //}

        private List<IPEndPoint> buildTargets(IPAddress localIPv4, IPAddress mask, RadioDiscoveryOptions options)
        {
            List<IPEndPoint> targets = new List<IPEndPoint>();

            int port = options.DiscoveryPortBase;
            if (port < 1) port = 1024;

            if (options.FixedTargetIp != null)
            {
                targets.Add(new IPEndPoint(options.FixedTargetIp, port));
                if(options.ProtocolMode == RadioDiscoveryProtocolMode.P2Only) return targets; // P2 can have a directed discover, P1 can not, so add in the directed subnet and 255.255.255.255
            }

            IPAddress directedBroadcast = getBroadcastAddress(localIPv4, mask);
            targets.Add(new IPEndPoint(directedBroadcast, port));

            if (options.IncludeGeneralBroadcast)
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
