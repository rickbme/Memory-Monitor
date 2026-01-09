# Memory Monitor v2.4.0 Release Notes

**Release Date:** January 10, 2025  
**Version:** 2.4.0.0  
**Installer Size:** 62.97 MB (self-contained with .NET 8 runtime)

---

## ?? What's New in 2.4.0

### Major Features

#### ?? Date & Time Display
- Current date and time shown on the mini monitor
- Date in top-left corner (e.g., "January 10")
- Time in top-right corner in 12-hour format (e.g., "2:30 PM")
- 22pt Segoe UI Bold font for excellent readability
- Updates every second

#### ?? FPS Gauge Control
- Dedicated circular gauge for frames per second
- Positioned between GPU and VRAM gauges
- Color-coded ring based on FPS quality:
  - **Green** (?60 FPS) - Excellent
  - **Yellow-Green** (45-59 FPS) - Good
  - **Orange** (30-44 FPS) - Acceptable
  - **Red** (<30 FPS) - Poor
- Auto-scaling font for 1-4 digit values

#### ?? Game Activity Detection
- Smart system to show FPS only during gaming
- Weighted scoring using multiple signals:
  - FPS data availability
  - Fullscreen/borderless window detection
  - Sustained high GPU usage (>70% for 5+ seconds)
  - Known game process detection (~60+ popular games)
- Uses Win32 APIs for accurate detection

#### ?? FPS Display Mode Menu
- New tray menu option for FPS control
- **Auto-detect** (default) - Shows FPS when gaming
- **Always Show** - Manual override
- **Always Hide** - Never show FPS
- Visual toast notification on mode change

### Improvements

#### ?? Enhanced Device Selection
- "All Disks" and "All Networks" options always available
- Users can switch back to aggregate view anytime
- Consistent experience for single and multiple device systems
- Easier access to total system throughput

### Quality of Life

#### ?? Removed Annoying Popups
- CPU temperature warning removed
- No more popups when running without HWiNFO
- Setup instructions moved to documentation
- Cleaner user experience

---

## ?? Installation

### System Requirements
- **OS:** Windows 10 (64-bit) or Windows 11
- **.NET Runtime:** Included (self-contained deployment)
- **Display:** 1920x480 mini monitor recommended (works on any display)
- **Privileges:** Administrator rights for hardware sensor access

### Install Instructions

**Option 1 - Double-click:**
```
1. Download MemoryMonitorSetup.msi
2. Double-click to install
3. Follow the installation wizard
4. Launch from Start Menu
```

**Option 2 - Silent Install (IT):**
```cmd
msiexec /i MemoryMonitorSetup.msi /qn
```

**Option 3 - With Logging:**
```cmd
msiexec /i MemoryMonitorSetup.msi /l*v install.log
```

### Installation Location
- **Program Files:** `C:\Program Files\Memory Monitor\`
- **Start Menu:** Memory Monitor shortcut
- **Desktop:** Memory Monitor shortcut (optional)

---

## ?? Upgrading from Previous Versions

### Automatic Upgrade
The installer automatically detects and upgrades previous installations:

```
1. Run the new MemoryMonitorSetup.msi
2. Installer removes old version automatically
3. New version installs
4. Shortcuts remain intact
5. Settings preserved
```

### Supported Upgrade Paths
- ? v2.3.0 ? v2.4.0
- ? v2.2.0 ? v2.4.0  
- ? v2.1.0 ? v2.4.0
- ? v2.0.0 ? v2.4.0
- ? v2.4.0 ? v2.3.0 (downgrade blocked)

---

## ?? Key Features Summary

### Real-Time Monitoring
- ? CPU usage with temperature
- ? GPU usage with temperature
- ? System RAM usage
- ? GPU VRAM usage
- ? Disk I/O throughput
- ? Network throughput
- ? FPS (frames per second)
- ? Date and time

### Display Features
- ? Optimized for 1920x480 mini monitors
- ? Circular gauges with needle indicators
- ? Color-coded FPS quality indicator
- ? Auto-scaling disk/network gauges
- ? Temperature inside gauges
- ? Date/time in corners

### Hardware Support
- ? NVIDIA GPUs (via NVML)
- ? AMD GPUs (via ADL)
- ? Intel CPUs (temperature via HWiNFO)
- ? Multi-disk systems
- ? Multi-network adapter systems

### User Experience
- ? Touch gesture support (mini monitor with touch)
- ? Device selection popups
- ? System tray integration
- ? Always-on-top mode
- ? Borderless full-screen
- ? Multi-monitor support

---

## ?? Configuration

### FPS Monitoring
Requires HWiNFO with RTSS integration:

```
1. Download HWiNFO: https://www.hwinfo.com/
2. Run HWiNFO
3. Settings ? Enable "Shared Memory Support"
4. Install RTSS (RivaTuner Statistics Server)
5. Launch game with overlay enabled
6. FPS appears on Memory Monitor
```

### Temperature Monitoring

**CPU Temperature:**
- Primary: LibreHardwareMonitor (built-in)
- Fallback: HWiNFO shared memory
- Requires admin privileges

**GPU Temperature:**
- NVIDIA: Direct via NVML (no setup needed)
- AMD: Direct via ADL (no setup needed)

### Device Selection
Click on Disk, GPU, or Network gauges to select specific devices:
- **All Disks** - Combined throughput
- **Specific Disk** - Individual disk stats
- **All Networks** - Combined throughput
- **Specific Network** - Individual adapter stats

---

## ?? Troubleshooting

### Application Won't Start
```
Problem: Application requires admin rights
Solution: Right-click ? Run as Administrator
         (or installer should set this automatically)
```

### No CPU Temperature
```
Problem: LibreHardwareMonitor can't access sensors
Solution: 
  1. Install HWiNFO
  2. Enable "Shared Memory Support"
  3. Keep HWiNFO running in background
```

### No FPS Display
```
Problem: HWiNFO/RTSS not configured
Solution:
  1. Install HWiNFO with shared memory enabled
  2. Install RTSS (RivaTuner Statistics Server)
  3. Enable FPS in game overlay
  4. Check FPS display mode in tray menu
```

### Installer Errors
```
Problem: "Another version is already installed"
Solution: Uninstall old version first, then reinstall

Problem: "Requires .NET 8"
Solution: Should not occur - installer is self-contained
         Contact support if this happens
```

---

## ?? What's Changed Since v2.3.0

### Added
- Date & time display in corners
- FPS circular gauge control
- Game activity detection system
- FPS display mode menu
- Always-available aggregate device options

### Removed
- CPU temperature warning popup
- Repetitive HWiNFO setup notifications

### Improved
- Device selection UX
- FPS display intelligence
- User experience polish

---

## ?? Documentation

- **README.md** - Main documentation
- **CHANGELOG.md** - Complete version history
- **BUILD_INSTALLER_GUIDE.md** - Build instructions
- **INSTALLER-README.md** - Installer documentation
- **PRE-RELEASE-CHECKLIST.md** - Testing guide

---

## ?? Known Issues

None at this time! ??

Report issues at: https://github.com/rickbme/Memory-Monitor/issues

---

## ?? Credits

**Developed by:** DFS (Dad's Fixit Shop)  
**License:** MIT  
**Repository:** https://github.com/rickbme/Memory-Monitor

### Third-Party Libraries
- **LibreHardwareMonitor** - Hardware sensor access
- **WiX Toolset** - Windows installer creation
- **.NET 8** - Application framework

---

## ?? Download

**Installer:** MemoryMonitorSetup.msi (62.97 MB)  
**SHA256:** (To be calculated after release)

### Checksums
To verify integrity after download:
```powershell
Get-FileHash MemoryMonitorSetup.msi -Algorithm SHA256
```

---

## ?? Next Steps After Installation

1. **Launch Application**
   - Start Menu ? Memory Monitor
   - Or Desktop shortcut

2. **Position on Mini Monitor**
   - Application auto-detects 1920x480 displays
   - Or use tray menu ? Move to Next Monitor

3. **Configure FPS (Optional)**
   - Install HWiNFO + RTSS for FPS monitoring
   - Right-click tray icon ? FPS Display ? Auto-detect

4. **Select Devices (Optional)**
   - Click gauges to select specific disks/networks
   - Or leave on "All" for total throughput

5. **Enjoy Monitoring!**
   - Always-on-top for persistent display
   - Touch gestures if your mini monitor supports it
   - Escape to minimize to tray

---

**Memory Monitor v2.4.0 - Making system monitoring beautiful! ??**
