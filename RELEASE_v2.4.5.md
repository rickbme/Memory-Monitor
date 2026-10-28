# Memory Monitor v2.4.5 - Reliability Fixes and 64-bit Installer

**Release Date:** October 2026

A bug-fix release. A full review of the Windows and Linux code turned up a number of problems that made
readings drop out, settings get lost, or FPS and touch behave oddly. This release fixes them and rebuilds
the Windows installer as a proper 64-bit package.

---

## Fixes

### Readings
- **GPU usage, VRAM and temperature no longer drop to 0** after switching between Circular Gauges and Bar Graph
- **CPU temperature no longer stops** after switching display mode
- **AMD GPUs** now show VRAM usage (it always read 0)
- **VRAM total** is correct on cards with more than 4 GB, for GPUs without NVIDIA/AMD driver support
- **Intel and other GPUs** show real whole-GPU usage instead of one program's usage
- **Network** - adapters with names like "Intel(R) ..." are found, and virtual adapters (Hyper-V, WSL,
  VirtualBox, VMware) no longer double-count traffic

### Settings that stick
- **Selected GPU, disk and network** are remembered between sessions (they were silently forgotten)
- **FPS Display** (Auto-detect / Always Show / Always Hide) is remembered across display mode switches and restarts
- **Settings carry over to new versions** - previously every upgrade reset them to defaults

### FPS and touch
- **FPS keeps working** if HWiNFO reorders its sensors, e.g. when RTSS restarts
- **Touch taps on gauges** cycle devices as intended, without also opening the mouse popup
- **Two-finger tap** toggles Always on Top (it never fired before)

### Smaller fixes
- F11 updates the Always on Top check mark
- The welcome dialog appears on first run in Bar Graph mode too
- Missing characters restored in the swipe notification and welcome dialog

### Linux
- CPU temperature on AMD Ryzen
- `eth0` no longer confused with `veth0`-style interfaces
- The tray check can't freeze the window
- Log cleanup only touches MangoHud's own log files

---

## Installer

- **64-bit**: installs to `C:\Program Files\Memory Monitor`. Older versions used `C:\Program Files (x86)`.
  Upgrading removes the old copy automatically.
- **Clean upgrades**: reinstalling or upgrading always leaves exactly one Memory Monitor entry in
  Add/Remove Programs.
- **Includes the .NET 8 runtime** - nothing else to install.

---

## Upgrade Notes

- Run the new installer over your current version - no need to uninstall first.
- Because the install folder moves, your **display mode resets once**, and **selected devices** (GPU, disk,
  network) reset once because of the device ID fix. Set them again and they're kept from now on.
- If **Memory Monitor 1.0.0.0** is still listed in **Settings → Apps**, uninstall it there. It's from a very
  early build that installers can't detect.

---

## Version Info

| Component | Version |
|-----------|---------|
| Application (Windows) | 2.4.5 |
| Application (Linux) | 2.4.5 |
| Installer | 2.4.5.0 (x64) |
| .NET Target | .NET 8.0 |

---

## Downloads

| File | Description |
|------|-------------|
| [MemoryMonitorSetup-2.4.5.msi](https://github.com/rickbme/Memory-Monitor/releases/download/v2.4.5/MemoryMonitorSetup-2.4.5.msi) | Windows installer (x64, includes .NET 8) - just download and run |
| `memory-monitor-2.4.5-linux-x64.tar.gz` | Linux package (see the [Linux README](MemoryMonitor.Linux/README.md)) |

---

**Full Changelog:** [v2.4.4...v2.4.5](https://github.com/rickbme/Memory-Monitor/compare/v2.4.4...v2.4.5)

**Previous Release:** [v2.4.4](https://github.com/rickbme/Memory-Monitor/releases/tag/v2.4.4)
