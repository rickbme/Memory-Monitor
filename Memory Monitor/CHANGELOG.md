# Changelog

All notable changes to the Memory Monitor project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2024

### Added
- **Borderless Full Screen Mode** - Application now runs without title bar for clean display
- **Auto Monitor Detection** - Automatically detects and positions on mini monitor (4:1 aspect ratio)
- **Move to Next Monitor** - Tray menu option to cycle through available monitors
- **Always on Top Toggle** - Tray menu option and F11 keyboard shortcut
- **Window Dragging** - Click and drag anywhere to reposition the borderless window
- **Keyboard Shortcuts**:
  - `Escape` - Minimize to system tray
  - `F11` - Toggle always on top

### Changed
- **Gauge Sizing** - Larger gauges using 95% of vertical space with minimal margins
- **Gauge Centering** - Gauges now properly centered both horizontally and vertically
- **Label Font Size** - Increased gauge labels from 10pt to 14pt for better readability
- **Scaled Elements** - Temperature display, digital readout, and labels now scale proportionally with gauge size
- **Gauge Radius** - Increased from 40% to 42% of available space for larger dials
- **Reduced Margins** - Side margins reduced from 20px to 10px, 5px spacing between gauges

### UI Improvements
- Removed window title bar and borders for dedicated display use
- Form automatically sizes to fill target monitor
- Gauges dynamically resize when window size changes
- All gauge text elements scale with gauge size

## [2.0.0] - 2024

### Added
- **Mini Monitor Display Support** - Redesigned UI optimized for 1920x480 secondary displays
- **Compact Gauge Controls** - New circular gauge design with needle indicators, glow effects, and digital readouts
- **CPU Temperature Monitoring** - Displays CPU temperature inside the CPU gauge
  - Primary: LibreHardwareMonitor integration
  - Fallback: HWiNFO shared memory support
- **GPU Temperature Monitoring** - Displays GPU temperature inside the GPU usage gauge
  - NVIDIA GPUs via NVML (nvml.dll)
  - AMD GPUs via ADL (atiadlxx.dll)
- **HWiNFO Integration** - Fallback temperature source when LibreHardwareMonitor cannot access sensors
- **Temperature Warning Notification** - Popup message when CPU temperature is unavailable with setup instructions
- **Administrator Manifest** - Application now requests admin privileges for hardware sensor access

### Changed
- **Form Layout** - Horizontal 6-gauge layout replacing vertical panels
- **Gauge Design** - Removed square metallic backgrounds, now displays circular dials only
- **Labels** - Moved gauge labels below each dial for cleaner appearance
- **Temperature Display** - Temperature shown inside gauge, below the needle pivot point

### Technical Changes
- Added `CompactGaugeControl.cs` - New gauge control optimized for horizontal displays
- Added `MiniMonitorForm.cs` / `MiniMonitorForm.Designer.cs` - New main form for mini monitor
- Added `HardwareMonitorService.cs` - Wrapper for LibreHardwareMonitor with fallback support
- Added `HWiNFOReader.cs` - Reader for HWiNFO shared memory interface
- Added `app.manifest` - UAC manifest requiring administrator privileges
- Updated `NVMLInterop.cs` - Added temperature reading support
- Updated `ADLInterop.cs` - Added temperature reading support
- Updated `GPUMonitor.cs` - Added temperature properties and methods
- Updated `CPUMonitor.cs` - Added temperature monitoring via HardwareMonitorService

### Dependencies
- Added LibreHardwareMonitorLib project reference
- Updated System.Management to version 10.0.1

## [1.0.0] - Initial Release

### Features
- RAM usage monitoring with progress bar and graph
- CPU usage monitoring with graph
- GPU usage and VRAM monitoring
- Disk I/O monitoring
- Network throughput monitoring
- Process list showing high memory usage applications
- System tray integration
- Dark/Light theme support

---

## Upgrade Notes

### Upgrading to 2.1.0
- Application now runs in borderless full screen mode by default
- Use the system tray icon to access controls (no title bar)
- Press `Escape` to minimize to tray, `F11` to toggle always on top
- Application auto-detects mini monitors with 4:1 aspect ratio

### Upgrading to 2.0.0
- The application now requires **Administrator privileges** to access hardware sensors
- For CPU temperature monitoring, you may need to run **HWiNFO** in the background with "Shared Memory Support" enabled
- The UI has been redesigned for 1920x480 mini monitor displays
