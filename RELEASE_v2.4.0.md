#  Memory Monitor v2.4.0 - Touch-Optimized Gaming Monitor

**Release Date:** January 10, 2025

We're excited to announce Memory Monitor v2.4.0, featuring intelligent FPS monitoring with game detection, date/time display, and touch-optimized device selection for mini monitors!

---

##  Highlights

###  Date & Time Display
Your mini monitor now shows the current date and time at a glance:
- **Date** in top-left corner (e.g., "January 10")
- **Time** in top-right corner, 12-hour format (e.g., "2:30 PM")
- Large, bold 22pt font for easy readability
- Updates every second

###  Smart FPS Monitoring
A brand-new FPS gauge with intelligent game detection:

**Color-Coded Performance Indicator:**
-  **Green (?60 FPS)** - Excellent, butter-smooth gaming
-  **Yellow-Green (45-59 FPS)** - Good, very playable
-  **Orange (30-44 FPS)** - Acceptable, playable with some stutter
-  **Red (<30 FPS)** - Poor, significant performance issues

**Intelligent Auto-Detection:**
The FPS gauge automatically appears when you're gaming and hides when you're not:
- Detects fullscreen and borderless window games
- Monitors sustained high GPU usage (>70% for 5+ seconds)
- Recognizes 60+ popular game processes
- Combines multiple signals for accurate detection

**Manual Control Options:**
Right-click the tray icon ? FPS Display:
- **Auto-detect** (default) - Smart detection based on activity
- **Always Show** - Force FPS display when available
- **Always Hide** - Never show FPS gauge

###  Touch Cycling for Device Selection
Mini monitor owners with touchscreens get a major UX upgrade:

**Before:** Tapping a gauge showed a tiny popup menu (hard to use on small displays)  
**Now:** Each tap cycles to the next device option

**How It Works:**
- Tap **GPU gauge**  Cycles through available GPUs
- Tap **Disk gauge**  Cycles through disks (All Disks ? Disk 0 ? Disk 1 ? etc.)
- Tap **Network gauge**  Cycles through adapters (All Networks ? WiFi ? Ethernet ? etc.)
- Visual **toast notification** shows newly selected device
- **Mouse users** still get the full popup menu

**Perfect for:**
- 1920x480 touchscreen mini monitors
- Touch-based device switching without tiny menus
- Quick cycling with visual feedback

---

##  Improvements

### Always-Available Aggregate Mode
- "All Disks" and "All Networks" options now **always visible** in device selection
- Previously only shown when multiple devices detected
- Easy switching between individual devices and total system throughput
- Works on single-device systems for consistency

### Enhanced Touch UX
- Touch cycling eliminates hard-to-tap small popup menus
- Optimized for compact 1920x480 touchscreen displays
- Seamless integration with existing swipe/tap/long-press gestures

### Cleaner User Experience
- **Removed** repetitive CPU temperature warning popups
- Temperature monitoring still works when HWiNFO is running
- Setup instructions available in README (no more annoying notifications)

---

##  Technical Details

### New Components
- **`FpsGaugeControl.cs`** - Custom circular gauge with color-coded quality rings
- **`GameActivityDetector.cs`** - Intelligent game detection using Win32 APIs and heuristics
- **`MiniMonitorForm.DateTime.cs`** - Dedicated partial class for date/time display
- **`CycleToNextDevice()`** - Touch-optimized device navigation method

### Architecture Updates
- Enhanced `ISelectableMonitor` interface with cycling support
- Touch/mouse mode detection in `CompactGaugeControl`
- Event-driven device cycling with visual feedback
- GPU usage history tracking for sustained load detection
- Win32 API integration for fullscreen window detection

### Modified Files
- `ISelectableMonitor.cs` - Added `CycleToNextDevice()` method
- `DiskMonitor.cs`, `NetworkMonitor.cs`, `GPUMonitor.cs` - Implemented cycling
- `CompactGaugeControl.cs` - Touch/mouse mode distinction
- `MiniMonitorForm.DeviceSelection.cs` - Cycling event handlers
- `MiniMonitorForm.Layout.cs` - FPS gauge and date/time layout
- `MiniMonitorForm.Monitors.cs` - FPS/DateTime updates

---

##  Requirements

### Minimum Requirements
- **OS:** Windows 10/11 (64-bit)
- **Privileges:** Administrator (for hardware sensor access)
- **.NET Runtime:** .NET 8.0 or later

### Optional Features

**For FPS Monitoring:**
- [HWiNFO](https://www.hwinfo.com/) with "Shared Memory Support" enabled
- [RTSS](https://www.guru3d.com/files-details/rtss-rivatuner-statistics-server-download.html) (RivaTuner Statistics Server) **OR**
- Any game overlay that reports FPS to HWiNFO

**For Touch Features:**
- Touchscreen-enabled mini monitor (HID-compliant touch hardware)
- USB data cable (not power-only)
- Windows-recognized touch drivers

---

##  Installation

### Fresh Installation
1. Download **`MemoryMonitorSetup.msi`** from the [Releases](https://github.com/rickbme/Memory-Monitor/releases/tag/v2.4.0) page
2. Run the installer (requires Administrator privileges)
3. Launch "Memory Monitor" from Start Menu or Desktop shortcut
4. For FPS monitoring, install and configure HWiNFO (see [Setup Guide](https://github.com/rickbme/Memory-Monitor/blob/master/README.md))

### Upgrading from Previous Versions
The installer will automatically upgrade your existing installation while preserving settings.

**From v2.3.x:**
- Date/time display appears automatically
- FPS gauge shows when gaming (if HWiNFO/RTSS configured)
- Touch cycling activates immediately on first tap
- All previous features remain functional

**From v2.2.x or earlier:**
- See the [full changelog](https://github.com/rickbme/Memory-Monitor/blob/master/Memory%20Monitor/CHANGELOG.md)
- Touch features may require touchscreen recalibration
- FPS monitoring requires HWiNFO configuration

---

##  Quick Start

### Enable FPS Monitoring (Optional)
1. Download and install [HWiNFO](https://www.hwinfo.com/)
2. Download and install [RTSS](https://www.guru3d.com/files-details/rtss-rivatuner-statistics-server-download.html)
3. Launch HWiNFO ? Settings ? Enable "Shared Memory Support"
4. Launch your game with RTSS overlay enabled
5. FPS gauge appears automatically when gaming detected

### Using Touch Cycling
1. Tap any **GPU**, **Disk**, or **Network** gauge
2. Watch the toast notification show the next device
3. Tap again to cycle to the next option
4. Selection wraps around (e.g., Disk 2 ? All Disks)

### Controlling FPS Display
1. Right-click the **system tray icon**
2. Navigate to **FPS Display** submenu
3. Choose your preferred mode:
   - **Auto-detect** - Show only when gaming (recommended)
   - **Always Show** - Force display when FPS available
   - **Always Hide** - Never show FPS

---

##  Known Issues

- **Intel GPU** native monitoring has limited support (uses performance counters as fallback)
- **Touch support** requires Windows-recognized HID-compliant touch hardware
- Some **antivirus software** may flag the administrator privilege requirement (false positive)
- **RTSS** must be running and reporting FPS for game detection to work optimally

---

##  Documentation

- - **[README.md](https://github.com/rickbme/Memory-Monitor/blob/master/README.md)** - Complete feature documentation
- **[CHANGELOG.md](https://github.com/rickbme/Memory-Monitor/blob/master/Memory%20Monitor/CHANGELOG.md)** - Full version history
- **[INSTALLER_SETUP.md](https://github.com/rickbme/Memory-Monitor/blob/master/INSTALLER_SETUP.md)** - Building the MSI installer
- **[BUILD_INSTALLER_GUIDE.md](https://github.com/rickbme/Memory-Monitor/blob/master/BUILD_INSTALLER_GUIDE.md)** - Detailed build instructions

---

##  Acknowledgments

Special thanks to:
- **LibreHardwareMonitor** team for the excellent sensor library
- **NVIDIA** and **AMD** for native GPU APIs
- **HWiNFO** team for the shared memory interface
- **Community contributors** for touch UX feedback and suggestions
- **Beta testers** for game detection testing

---

##  Feedback & Support

- **Report Issues:** [GitHub Issues](https://github.com/rickbme/Memory-Monitor/issues)
- **Feature Requests:** [Discussions](https://github.com/rickbme/Memory-Monitor/discussions)
- **Source Code:** [View on GitHub](https://github.com/rickbme/Memory-Monitor)

---

## Downloads

| File | Description | Size |
|------|-------------|------|
| [MemoryMonitorSetup.msi](https://github.com/rickbme/Memory-Monitor/releases/download/v2.4.0/MemoryMonitorSetup.msi) | Windows Installer (x64) | ~XMB |
| [Source code (zip)](https://github.com/rickbme/Memory-Monitor/archive/refs/tags/v2.4.0.zip) | Source code archive | - |
| [Source code (tar.gz)](https://github.com/rickbme/Memory-Monitor/archive/refs/tags/v2.4.0.tar.gz) | Source code archive | - |

---

##  Checksums

SHA256 checksums for release files:
```
TBD - Generate after release build
```

---

**Full Changelog:** [v2.3.0...v2.4.0](https://github.com/rickbme/Memory-Monitor/compare/v2.3.0...v2.4.0)

**Enjoy the new features!**

✅ Build: Successful
✅ Errors: None
✅ Warnings: None
