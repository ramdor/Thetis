# VST Architecture

## What This Provides

Real-time VST2/VST3 audio effect chain support for the Thetis SDR application. Two independent plugin chains — RX (receive) and TX (transmit) — process audio through user-configured effect plugins, enabling features like noise reduction, equalization, compression, and other audio processing directly within the radio's signal path.

Key capabilities:

- **Out-of-process hosting** — plugins run in separate helper processes so a plugin crash never takes down the radio
- **Shared-memory audio transport** — low-latency ring buffer design keeps DSP threads non-blocking with adaptive pipeline latency
- **Automatic recovery** — crashed hosts restart and replay their full plugin chain state transparently
- **Persistent state** — plugin configurations, bypass/gain settings, and plugin editor state survive app restarts
- **VAC integration** — per-VAC controls allow independent routing of VST-processed audio to virtual audio cables
- **Plugin catalog** — out-of-process scanner builds a searchable catalog of installed VST3 plugins with crash-resilient scanning
- **Chain manager UI** — tabbed RX/TX interface for adding, ordering, enabling, bypassing, and editing up to 16 plugins per chain

The system defaults to a safe state: both chains start bypassed and VAC TX paths skip the VST chain until explicitly configured, so enabling VST support has zero impact on audio until the operator chooses to use it.

## Overview

Thetis currently hosts RX and TX audio effect chains out of process.

Live hosting currently supports:

- VST3 plugins
- VST2 plugins loaded manually from `.dll` files

Plugin discovery/cataloging is still primarily a VST3 workflow. VST2 support exists in the live host/runtime path, not in the scanner/catalog path.

The main design goals are:

- keep plugin crashes from taking down `Thetis.exe`
- keep the WinForms UI responsive
- persist plugin state across app restarts
- allow plugin editor windows to work without WinForms embedding
- keep the DSP thread non-blocking
- fail open to dry audio when the host is unavailable

This document describes the implementation as it exists now.

## System Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│  Thetis.exe                                                                     │
│                                                                                 │
│  ┌──────────────────────────────────────────────────────────────────────────┐   │
│  │  ChannelMaster (native DSP)                                              │   │
│  │                                                                          │   │
│  │  RX Path (aamix.c)                     TX Path (cmaster.c)               │   │
│  │  ┌─────────┐                           ┌─────────────┐                   │   │
│  │  │ xaamix  │                           │ xpipe/xdexp │                   │   │
│  │  └────┬────┘                           └──────┬──────┘                   │   │
│  │       │                                       │                          │   │
│  │       ▼                                       ▼                          │   │
│  │  ┌──────────────────────┐             ┌─────────────────────────┐        │   │
│  │  │ VST_ProcessInterl... │             │ GetTXVACVstBypass(tx)?  │        │   │
│  │  │ (VST_CHAIN_RX)       │             │  yes → skip VST         │        │   │
│  │  └────┬─────────────────┘             │  no  → VST_Process...   │        │   │
│  │       │                               │        (VST_CHAIN_TX)   │        │   │
│  │       ├──────────────────┐            └──────┬──────────────────┘        │   │
│  │       │                  │                   │                           │   │
│  │       ▼                  ▼                   ▼                           │   │
│  │  ┌──────────┐   ┌──────────────────┐   ┌──────────┐                      │   │
│  │  │ Outbound │   │ FeedIVACPostRx   │   │ fexchange│                      │   │
│  │  │ (main    │   │ Audio() → VACs   │   │ (TX DSP) │                      │   │
│  │  │  output) │   │ with Apply RX    │   └──────────┘                      │   │
│  │  └──────────┘   │ VST enabled      │                                     │   │
│  │                 └──────────────────┘                                     │   │
│  └──────────────────────────────────────────────────────────────────────────┘   │
│                                                                                 │
│  ┌────────────────────────────────────────┐  ┌──────────────────────────────┐   │
│  │  VstHostBridge.dll (native, in-proc)   │  │  Console (managed, C#)       │   │
│  │                                        │  │                              │   │
│  │  • C ABI for DSP + P/Invoke            │  │  vsthost.cs                  │   │
│  │  • shared-mem audio transport          │  │   • P/Invoke wrapper         │   │
│  │  • control IPC client (named pipes)    │  │   • JSON persistence         │   │
│  │  • authoritative bridge cache          │  │   • deferred save scheduler  │   │
│  │  • host launch / restart / shutdown    │  │   • state-changed callback   │   │
│  │  • adaptive pipeline latency           │  │                              │   │
│  │  • chain bypass (transport-level)      │  │  VstChainManagerForm.cs      │   │
│  │  • snapshot generation tracking        │  │   • RX/TX tabs, 16-slot max  │   │
│  │                                        │  │   • plugin add/remove/move   │   │
│  │  Per chain:                            │  │   • bypass/gain/enable       │   │
│  │  ┌──────────────────────────────┐      │  │   • open editor              │   │
│  │  │ shared-mem ring (32 slots)   │      │  │                              │   │
│  │  │ request event ──►            │      │  │  VstPluginPickerForm.cs      │   │
│  │  │ dirty event   ◄──            │      │  │   • scan/catalog UI          │   │
│  │  │ named pipe (control)         │      │  │   • search path mgmt         │   │
│  │  └──────────────────────────────┘      │  │   • filter/sort              │   │
│  └────────────────────────────────────────┘  └──────────────────────────────┘   │
│                                                                                 │
└─────────────────────┬───────────────────────────────────┬───────────────────────┘
                      │ IPC (per chain)                   │ OOP scan
                      ▼                                   ▼
        ┌──────────────────────────┐        ┌──────────────────────────┐
        │  VstAudioHost.exe (×2)   │        │  VstPluginScanner.exe    │
        │                          │        │                          │
        │  One per chain (RX, TX)  │        │  • metadata-only probe   │
        │  • VstChain runtime      │        │  • suppress crash dlgs   │
        │  • plugin instances      │        │  • JSON result output    │
        │  • editor windows        │        │  • OLE/COM STA init      │
        │  • audio worker thread   │        └──────────────────────────┘
        │  • control server        │
        │  • dirty-state publish   │
        │                          │
        │  Runtime families:       │
        │  • VST3 (vst_runtime)    │
        │  • VST2 (vst2_runtime)   │
        └──────────────────────────┘
```

## High-Level Topology

The implementation is split across five layers:

1. DSP/audio patch points in `ChannelMaster`
2. Native bridge DLL `VstHostBridge.dll`
3. Out-of-process runtime host `VstAudioHost.exe`
4. Managed UI/persistence layer in `Console/vsthost.cs`
5. Scanner/catalog flow for plugin discovery

At runtime there are two independent host processes:

- one for the RX chain
- one for the TX chain

The bridge launches and manages both. The host owns live plugin execution and editor windows. The bridge owns the authoritative cached model used for UI, persistence, and restart replay.

## Main Components

### 1. Audio Patch Points

Main integration points:

- RX: `Project Files/Source/ChannelMaster/aamix.c`
- TX: `Project Files/Source/ChannelMaster/cmaster.c`
- VAC routing: `Project Files/Source/ChannelMaster/ivac.c`
- TX VAC bypass: `Project Files/Source/ChannelMaster/pipe.c`

Current placement:

- RX VST processing is applied near the end of the RX mixer path, before output distribution
- TX VST processing is applied in the transmitter path before TX DSP exchange

#### VAC VST Integration

VAC (Virtual Audio Cable) paths have their own VST controls, separate from the main chain bypass:

**RX side — Apply RX VST:**

When `VAC_Apply_RX_VST` is enabled for a VAC, the post-VST RX audio is fed directly to that VAC via `FeedIVACPostRxAudio()` instead of the VAC receiving pre-VST audio through its normal mixer path. This avoids running the RX chain a second time. The flag is per-VAC (VAC1 and VAC2 independently).

**TX side — Bypass TX VST:**

When `VAC_Bypass_TX_VST` is enabled for a VAC, the TX VST chain is skipped entirely for audio arriving from that VAC. This is checked in `cmaster.c` via `GetTXVACVstBypass()` before calling `VST_ProcessInterleavedDouble()`. The flag is per-VAC.

**How this differs from chain bypass:**

| Mechanism | Scope | Effect |
|-----------|-------|--------|
| Chain bypass (`SetChainBypass`) | Global, transport-level | Skips OOP transport entirely for all sources |
| VAC Bypass TX VST | Per-VAC, conditional | Skips TX chain only when that VAC is the active TX input |
| VAC Apply RX VST | Per-VAC, additive | Feeds post-VST RX audio to that VAC |

### 2. Native Bridge: `VstHostBridge.dll`

Primary files:

- `Project Files/Source/VstHostBridge/vst_host_bridge.h`
- `Project Files/Source/VstHostBridge/vst_host_bridge.cpp`

Responsibilities:

- exported C ABI used by native DSP code and C# P/Invoke
- host process launch/shutdown
- control IPC client
- shared-memory audio transport producer/consumer
- authoritative cached chain model
- host liveness tracking and restart
- editor-open forwarding
- metadata-only native plugin probe used by the scanner helper

The bridge exports operations such as:

- create/destroy chain
- set/get bypass and gain
- add/remove/move plugins
- get plugin info and state
- set plugin state
- open plugin editor window
- process interleaved audio
- query host state
- query cached snapshot generation
- register a state-changed callback

### 3. Runtime Host: `VstAudioHost.exe`

Primary files:

- `Project Files/Source/VstAudioHost/host_process.h`
- `Project Files/Source/VstAudioHost/host_process.cpp`
- `Project Files/Source/VstAudioHost/main.cpp`

Responsibilities:

- own the live `VstChain` runtime for one chain
- own actual plugin runtime instances
- own plugin editor windows
- run the host-side control server
- run the host-side audio worker
- publish dirty-state notifications
- exit on fatal runtime/control failure so the bridge can restart it

There is one host process per chain. The host is intentionally disposable.

Runtime families currently used inside the host:

- VST3 through `vst_runtime.*`
- VST2 through `vst2_runtime.*`

### 4. Managed Host/UI Layer

Primary files:

- `Project Files/Source/Console/vsthost.cs`
- `Project Files/Source/Console/VstChainManagerForm.cs`
- `Project Files/Source/Console/VstPluginPickerForm.cs`

Responsibilities:

- P/Invoke wrapper over `VstHostBridge.dll`
- chain persistence to JSON
- plugin catalog persistence to JSON
- deferred save scheduling
- chain manager UI
- plugin picker/scanner UI
- state-changed callback registration

### 5. Scanner Helper

Primary files:

- `Project Files/Source/VstPluginScanner/Program.cs`
- native probe entry in `Project Files/Source/VstHostBridge/vst_host_bridge.cpp`

Responsibilities:

- isolate scanning from Thetis
- read metadata for VST3 candidates
- suppress Windows crash dialogs during scanning
- return structured JSON results to the managed scanner

The scanner is separate from the audio host. Scanning and live hosting are intentionally different concerns.

## Runtime Model

### OOP-Only Live Hosting

Live plugin execution is out of process only.

That means:

- `Thetis.exe` does not run the live plugin chain in process
- `VstAudioHost.exe` is the live execution environment
- the bridge cache is the authoritative persistence/restart/UI model

Authoritative split:

- host process: live execution truth
- bridge cache: persistence/restart/UI model truth

The bridge cache is not a second runtime. It exists to support:

- UI reads without live-host round-trips everywhere
- persistence writes
- restart replay after host failure
- transient operation while the host is down

### Chain Model

There are exactly two chain kinds:

- `VST_CHAIN_RX`
- `VST_CHAIN_TX`

Each chain supports up to 16 plugins (enforced by the chain manager UI).

Each host process owns one `VstChainState`. The bridge keeps matching cached fields:

- chain created/not created
- sample rate
- max block size
- number of channels
- ready
- bypass
- gain (0.0 to 8.0, step 0.05)
- ordered plugin list
- plugin enabled/bypass
- plugin load state
- plugin state blob

### Default State

When no saved state exists (new profile or first run):

- RX chain: bypassed
- TX chain: bypassed
- VAC1 Apply RX VST: disabled
- VAC1 Bypass TX VST: enabled
- VAC2 Apply RX VST: disabled
- VAC2 Bypass TX VST: enabled

This means chains start silent (bypassed) by default and VAC TX paths skip the TX chain until explicitly configured.

### Plugin Load States

Plugin load state is surfaced as:

- `Failed`
- `None`
- `DescriptorOnly`
- `Active`

Meaning:

- `DescriptorOnly` means the plugin is known in the chain model but not fully active as a live runtime
- `Active` means the plugin is loaded and participating normally

## Control Plane

### Transport

Control IPC is defined in:

- `Project Files/Source/VstCommon/vst_ipc.h`

The bridge and host communicate over a named pipe using fixed-size `VstHostIpcPacket` messages.

Important message types:

- handshake: `HELLO`, `PING`, `PONG`, `SHUTDOWN`
- chain control: create/destroy/bypass/gain/ready/clear
- plugin control: add/remove/move/info/state/editor
- snapshot control: `GET_CHAIN_SNAPSHOT_INFO`

### Control Flow

The bridge launches a host, connects to its control pipe, validates the handshake, and issues synchronous requests with bounded I/O timeouts.

The host control server:

- blocks on pipe traffic using overlapped I/O
- handles one command at a time
- maps commands into `VstChain_*` operations
- bumps a host-side snapshot generation when state changes

The control plane is intentionally separate from the audio thread.

### Bounded Waits

Critical control/runtime operations use bounded waits rather than infinite waits.

This currently applies to:

- bridge pipe I/O
- host pipe I/O
- runtime host-thread command execution
- runtime creation and destruction

If a plugin wedges badly enough that the host can no longer service control safely, the host exits and the bridge recovery path takes over.

## Audio Plane

### Shared Memory Transport

Audio IPC is defined by `VstHostAudioSharedBlock` in:

- `Project Files/Source/VstCommon/vst_ipc.h`

Each chain gets:

- a named file mapping for shared audio buffers
- a request event used by the bridge to signal new work
- a dirty-state event used by the host to notify state changes

The shared block contains:

- request sequence
- sample rate / max block size / channel count
- last process result
- dirty generation
- per-slot input sequence and frame counts
- per-slot processed sequence and frame counts
- per-slot interleaved input/output buffers

### Ring Buffer Model

The transport uses a ring of `32` audio slots.

The bridge:

- copies the dry input block into the next slot
- publishes the request sequence
- signals the host request event
- consumes processed output from an earlier sequence after a configured pipeline latency

The host:

- wakes on the request event
- drains pending sequences in order
- detects ring overruns
- resynchronizes if it falls behind
- writes processed output back into the matching slot

### Adaptive Pipeline Latency

The bridge maintains per-chain adaptive pipeline latency.

Goals:

- avoid crackle caused by late processed blocks
- tolerate host scheduling jitter
- avoid staying at the worst-case latency forever after a transient miss

Current behavior:

- latency increases when processed output is not ready
- RX starts with a more conservative floor than TX
- after long stable runs, current latency decays slowly
- after even longer stable runs, the latency floor also decays slowly

Current defaults:

- RX default floor: `8` blocks
- TX default floor: `2` blocks

This is a tuning tradeoff, not a strict correctness mechanism.

### Late-Block Handling

The bridge never blocks the DSP thread waiting on processed output.

Current behavior differs by chain:

- RX: if a processed block is late and a previous wet block is available, the bridge reuses the last good wet block as concealment
- TX: late blocks fall back to the current dry input path

This is intentional:

- the audio thread must not block on IPC
- host failure or lateness must degrade safely

### True Chain Bypass

Chain bypass is now a true transport bypass, not just a chain-internal soft bypass.

When chain bypass is enabled:

- ChannelMaster still calls the bridge entry point
- the bridge short-circuits before submitting work to the OOP host
- buffered transport state is flushed so stale wet blocks are not replayed when bypass is turned back off

That means chain bypass avoids:

- plugin processing
- OOP transport latency

Per-plugin bypass still works inside the chain processor and only skips the selected plugin.

## Host Cache and Snapshot Model

### Why the Cache Exists

The bridge cache is used for:

- restart replay
- persistence capture
- low-cost UI reads
- transient operation when the host is down

### Snapshot Coherency

The host maintains a snapshot generation counter per chain.

The bridge refreshes its cache by:

1. requesting chain snapshot info including generation
2. requesting plugin info/state using that expected generation
3. retrying up to 4 times if the host reports `SNAPSHOT_CHANGED`
4. swapping the completed snapshot into the cache under lock

This prevents the bridge from persisting or replaying a mixed chain state that never actually existed in the host.

The host snapshot payload now includes:

- created
- sample rate
- max block size
- number of channels
- ready
- bypass
- gain
- plugin count
- generation

### Cache Hardening

The cache/host boundary has been hardened so that:

- bridge mutations are serialized under a per-chain mutation lock
- cache refresh and replay are serialized with that same lock
- replay uses the actual host index returned from `ADD_PLUGIN` rather than assuming slot identity
- dirty-refresh only advances its observed generation after a successful cache refresh
- persistence capture retries rather than silently writing partial plugin lists when a plugin read fails

### Cache Generation

The bridge also maintains its own cache generation counter.

Managed persistence uses this to detect mid-capture changes and reject inconsistent snapshots when required.

## Failure Handling and Recovery

### Host States

Host state is surfaced as:

- `Disabled`
- `Starting`
- `Running`
- `Unavailable`
- `Crashed`
- `Restarting`

### Automatic Restart

The bridge performs one automatic restart attempt per host failure.

Restart flow:

1. detect control/audio/runtime failure
2. mark host `Restarting`
3. tear down old process and IPC objects
4. relaunch `VstAudioHost.exe`
5. replay the cached chain snapshot into the new host
6. return to `Running` on success
7. otherwise settle to `Unavailable`

Replay includes:

- chain format
- bypass
- gain
- plugin list
- plugin enabled/bypass
- plugin state blobs

### Dirty-State Notification

The host increments a dirty generation and signals a dirty event when plugin state changes.

This is used for:

- editor-originated parameter changes
- plugin state persistence updates

The bridge runs a watcher thread per chain that:

- waits on the dirty event
- coalesces bursts for a short interval
- refreshes the cached snapshot from the host
- invokes a managed callback

The managed side responds by scheduling the normal deferred save path.

## Editor Hosting

Plugin editors are host-owned top-level windows.

Common properties:

- Thetis requests `VST_OpenPluginEditorWindow(kind, index)`
- the bridge forwards the request to the host
- the host owns the actual editor window lifetime
- editors are not embedded into WinForms

Important implications:

- no cross-process parenting into `Thetis.exe`
- editor crashes/hangs are isolated to the host process
- editor failures are treated as host/runtime failures, not as a reason to fall back to stale local execution

### VST2 Editor Model

VST2 editors use the VST2 runtime and host-thread ownership model in `vst2_runtime.*`.

Important properties:

- VST2 plugin creation and editor work happen on a persistent per-runtime host thread
- VST2 audio/state/editor API access is serialized with an explicit runtime API lock
- the editor window is host-owned and top-level

This model was introduced to handle thread-sensitive VST2 plugins more safely.

### VST3 Editor Model

VST3 editors share the live audio runtime's component and controller instance.

Current behavior:

- the editor view is created on the live runtime's `IEditController`
- output parameter changes (metering, spectrum, visualization) flow naturally from `process()` to the controller
- thread safety between the audio thread and editor is handled by the existing try-lock in `VstRuntime_Process` — if a control-path operation holds the API lock, the audio thread skips the block and falls back to dry audio

This single-instance model matches standard DAW practice and ensures real-time plugin visualizations work correctly.

## Scanner and Catalog

### Goals

The scanner is not part of live audio hosting. Its purpose is to build a reusable catalog of candidate audio-effect plugins for the picker UI.

The scanner currently concerns itself with VST3 audio-effect-style plugins. Instruments and non-audio plugins are intentionally filtered out.

Current scope:

- VST3 scanner/catalog support: yes
- VST2 scanner/catalog support: no
- VST2 manual load by file path: yes

### Scanner Architecture

Main pieces:

- `Console/VstPluginPickerForm.cs`
- `Console/vsthost.cs`
- `VstPluginScanner.exe`
- `VST_ProbePluginMetadataOnly`

### Search Path Model

The picker maintains a list of search paths, pre-populated with common VST3 locations such as:

- `Program Files/Common Files/VST3`
- `Program Files/Common Files/Steinberg/VST3`
- `LocalAppData/Programs/Common/VST3`

It supports:

- add path
- remove path
- incremental scan
- full rescan
- cached results
- hide/show unavailable plugins

### Candidate Discovery

The managed scanner recursively enumerates `.vst3` files and bundle directories under the configured search paths.

### Metadata Strategy

Scan classification uses a layered approach:

1. `moduleinfo.json` fast path when present
2. out-of-process scanner helper calling `VST_ProbePluginMetadataOnly`

The current probe is metadata-only during scanning.

### Cache Reuse

Catalog entries store `LastModifiedUtc`.

If a plugin path is unchanged and a rescan is not forced, the existing cached result can be reused instead of rescanned.

### Scan Resilience

Bad plugins should not crash Thetis during scan.

The helper:

- runs out of process
- suppresses Windows crash dialogs
- initializes OLE/COM STA for plugin compatibility
- returns structured JSON on success or failure

If a plugin scan fails:

- the entry is marked unavailable
- scanning continues
- the failure is recorded in the catalog and scan log

## Persistence

### State File

Chain state is persisted to `vst_chains.json` in the application data directory.

The file contains:

- version field (currently `3`)
- SDK availability flag
- RX chain state (bypass, gain, ordered plugin list with state blobs)
- TX chain state (same structure)

Serialization uses Newtonsoft JSON.

### Deferred Save Scheduling

State saves are coalesced to avoid excessive disk writes during rapid changes:

- property changes (bypass, gain, enable, plugin bypass): 750ms delay
- structural changes (add, remove, move plugin): 1000ms delay
- retry on transient capture failure: 750ms delay

If a persistence capture detects an inconsistent snapshot (mid-capture generation change), it retries rather than writing partial state.

### Catalog File

Plugin catalog is persisted separately to `vst_plugin_catalog.json` with:

- catalog version
- last scan timestamp (UTC)
- search paths
- per-plugin metadata (name, vendor, version, categories, availability, last modified)

## UI Architecture

### Chain Manager

`VstChainManagerForm` is the main live management UI.

Key behavior:

- separate RX and TX tabs
- 16-slot plugin grid per chain (empty slots shown as disabled rows)
- async full refresh of chain lists
- lighter periodic status refresh
- deferred refresh for chatty toggle operations
- status labels for host state and chain readiness

Available operations:

- add plugin from catalog
- add plugin manually from file path
- remove plugin
- move up/down
- enable/disable
- bypass/unbypass
- chain bypass
- chain gain
- open editor

### Picker UI

`VstPluginPickerForm` is a cached picker over scanned plugins.

Key behavior:

- full-width plugin list
- separate search-path list
- live status/progress area
- incremental list fill during scan
- hide/show unavailable filter
- sorting/filtering

## Threading Model

Major threads involved:

- Thetis DSP/audio threads
- WinForms UI thread
- bridge dirty-watch thread per chain
- bridge restart thread per chain
- host control thread
- host audio worker thread
- host editor worker thread(s)
- per-plugin runtime host thread where required by the runtime family

Priority strategy:

- host process: high priority class
- host audio thread: highest priority plus MMCSS `Pro Audio`
- host control thread: lower priority than audio
- editor worker: lowest priority

Runtime-specific notes:

- VST2 uses explicit runtime API serialization between audio and control/editor/state paths
- VST3 now uses a dedicated runtime API lock, with audio taking a try-lock so control/editor/state work cannot block the DSP thread indefinitely

## Diagnostics

Optional runtime tracing is controlled by:

- `THETIS_VST_OOP_TRACE=1`

When enabled, bridge and host append trace lines to:

- `%TEMP%\\ThetisVstOop.log`

Trace output includes:

- submitted/consumed/fallback block counts
- concealed block counts
- not-ready counts
- outstanding sequences
- adaptive latency
- host-side processing timing
- restart activity

## Initialization and Shutdown

### Startup

Typical startup sequence:

1. `VST_Initialize()` launches RX and TX hosts
2. `ChannelMaster` creates RX and TX chains with current sample rate/block sizes
3. managed state load restores saved chain definitions
4. bridge cache and host runtime converge on the restored chain

### Shutdown

Typical shutdown sequence:

1. managed save writes current persistent state
2. `VST_DestroyChain()` tears down TX and RX chains
3. `VST_Shutdown()` requests host shutdown and closes IPC objects

If a host does not exit promptly during shutdown, it can be terminated as a final recovery action.

## Current Limitations

Known limitations of the current implementation:

- OOP audio transport still adds real latency relative to fully in-process hosting
- adaptive latency tuning is heuristic and still machine/plugin dependent
- editor open or other control activity can still influence transport latency heuristics, especially on RX
- there is no embedded editor model; editors are separate host-owned windows by design
- VST2 is manual-load only and is not integrated into the scanner/catalog flow
- there is no plugin delay compensation layer

## Design Summary

In one sentence:

Thetis now hosts RX and TX VST2/VST3 chains in separate helper processes, uses a native bridge and shared-memory audio transport for realtime processing, keeps a hardened bridge-side cache as the authoritative persistence/restart model, and exposes the system through managed scanner and chain-manager UIs.

That split remains the key architectural decision:

- isolate live plugin execution in disposable host processes
- keep stable control, persistence, and UX logic in Thetis
