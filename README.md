Initially I put one of these on my christmas list thinking I would use it for an information display.  Little did i know that this was just a display.  Ya I know, dummy lol.  Anyway, I saw that there was a program that would do what
I wanted called AIDA64.  After looking at it and seeing how involved it was this was not for me.  I just wanted a simple display to show my usage. Plus I didn't want to pay for it :P.  So this was my vacation project!
Feel free to use and modify it to suite your needs!  I may add some different displays with different looks if I get enough interest.  Let me know! And hope this is useful for you!
I've tried to bug check it best I could, but if there is one you find, please let me know and i'll try and get it fixed.

Rick
DFS

# System Monitor

**A modern, feature-rich Windows system monitoring application built with .NET 8**

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6?logo=windows)
![License](https://img.shields.io/badge/license-MIT-green)

---

## 📸 Screenshot

![Memory Monitor Screenshot](MemMonPic.png)
*Memory Monitor running on a 1920x480 mini display with real-time CPU, GPU, RAM, VRAM, Disk, and Network monitoring*
![Memory Monitor Screenshot](MemMonGraph.png)
*Graph Display mode*

---

##  Overview

System Monitor is a comprehensive real-time monitoring tool for Windows designed for **secondary/mini displays (1920x480)**. Built with modern .NET 8 and Windows Forms, it features beautiful circular gauge visualizations with temperature monitoring for both CPU and GPU. Program will automatically detect a 1920x480 display and move itself over to it. No moving the display each time.

### Key Features

- **Automatic move to any 1920x480 display!
- **Two Display Modes** - Switch between Circular Gauges and Bar Graph views
- **Smooth Mode Transitions** - Fade animation when switching display modes
- **Real-Time CPU Monitoring** - Track CPU usage with temperature display
- **GPU Monitoring** - Native NVIDIA (NVML) and AMD (ADL) support with temperature
- **System Memory Tracking** - Monitor RAM usage with detailed statistics
- **GPU VRAM Monitoring** - Video memory usage and total capacity
- **Disk I/O Monitoring** - Read/write throughput with auto-scaling
- **Network Monitoring** - Upload/download speed with auto-scaling
- **Temperature Display** - CPU and GPU temperatures shown inside gauges
- **FPS Monitoring** - Real-time frame rate display with smart game detection
- **Date & Time Display** - Current date and time on mini monitor
- **Touch Gesture Support** - Full touch input for mini monitors with touchscreen
- **Mini Monitor Optimized** - Designed for 1920x480 secondary displays

---

## 🎨 Display Modes

Memory Monitor offers two beautiful display modes:

### Circular Gauges Mode (Default)
- Classic needle gauge visualization
- 6 circular gauges in a horizontal row
- FPS gauge between GPU and VRAM gauges
- Date/time display in corners
- Animated needle movements with glow effects

### Bar Graph Mode
- Modern animated bar graph panels
- 5 panels: CPU, GPU, VRAM, Drive, Network
- Rolling 12-point history animation
- Gradient-filled bars with glow effects
- Futuristic corner accents

### Switching Display Modes
Right-click the **system tray icon** → **Display Mode**:
- **Circular Gauges** - Classic needle gauge display
- **Bar Graph** - Modern animated bar panels

Smooth fade transition (300ms) provides a polished experience when switching.

---

##  Display Layout

### Circular Gauges Mode
The application displays 6 gauges in a horizontal row, optimized for 1920x480 displays:

| Gauge | Color | Information |
|-------|-------|-------------|
| **RAM** | Blue | Memory usage (GB used / total) |
| **CPU** | Red | CPU usage (%) + Temperature |
| **GPU** | Orange | GPU usage (%) + Temperature |
| **FPS** | Color-coded | Frames per second (gaming) |
| **VRAM** | Purple | Video memory (GB used / total) |
| **DISK** | Green | Disk I/O throughput (Mbps) |
| **NET** | Yellow | Network throughput (Mbps) |

**Additional Display Elements:**
- **Date** in the top-left corner
- **Time** in the top-right corner (12-hour format)

### Bar Graph Mode
5 panels displayed horizontally:

| Panel | Color | Information |
|-------|-------|-------------|
| **CPU** | Cyan | CPU usage (%) + Temperature |
| **GPU** | Green | GPU usage (%) + Temperature + FPS |
| **VRAM** | Purple | Video memory usage |
| **DRIVE** | Orange | Disk I/O activity |
| **NETWORK** | Cyan | Upload/Download speeds |

---

##  FPS Monitoring

Memory Monitor displays real-time frames per second with intelligent game detection.

### FPS Gauge Features
- **Color-Coded Quality Indicator:**
  - 🟢 Green (60+ FPS) - Excellent, butter-smooth gaming
  - 🟡 Yellow-Green (45-59 FPS) - Good, very playable
  - 🟠 Orange (30-44 FPS) - Acceptable, playable with some stutter
  - 🔴 Red (<30 FPS) - Poor, significant performance issues
- **Auto-scaling font** for 1-4 digit FPS values
- **Circular Gauges**: Dedicated FPS gauge between GPU and VRAM
- **Bar Graph**: FPS shown in GPU panel

### Smart Game Detection
The FPS display automatically appears when you're gaming and hides when you're not:
- Detects fullscreen and borderless window games
- Monitors sustained high GPU usage (>70% for 5+ seconds)
- Recognizes 60+ popular game processes
- Combines multiple signals for accurate detection

### Display Modes
Right-click tray icon → **FPS Display** submenu (available in both display modes):
- **Auto-detect** (default) - Smart detection based on activity
- **Always Show** - Force FPS display when available
- **Always Hide** - Never show FPS gauge

### Setup Requirements
1. Download and install [HWiNFO](https://www.hwinfo.com/)
2. Download and install [RTSS](https://www.guru3d.com/files-details/rtss-rivatuner-statistics-server-download.html) (RivaTuner Statistics Server)
3. Launch HWiNFO → Settings → Enable **"Shared Memory Support"**
4. Launch your game with RTSS overlay enabled
5. FPS gauge appears automatically when gaming is detected

> **Note:** FPS monitoring requires HWiNFO with "Shared Memory Support" enabled and RTSS or a game overlay that reports FPS to HWiNFO.

---

##  Touch Gesture Support

For mini monitors with touchscreen capability, Memory Monitor supports the following gestures:

### Swipe Gestures
- **Swipe Left/Right** - Switch between monitors
- **Swipe Down** - Minimize to system tray

### Tap Gestures
- **Tap on GPU/Disk/Network Gauge** - Cycle through available devices
  - Each tap advances to the next device (wraps around)
  - Toast notification shows newly selected device
  - Mouse clicks still show full popup menu for precise selection
- **Long Press** - Show context menu at touch location
- **Two-Finger Tap** - Toggle "Always on Top" mode

### Requirements for Touch
- Touchscreen-enabled mini monitor with HID-compliant touch hardware
- USB data cable connection (not power-only)
- Windows-recognized touch drivers

---

##  Temperature Monitoring

### GPU Temperature
- **NVIDIA GPUs** - Native support via NVML (nvml.dll)
- **AMD GPUs** - Native support via ADL (atiadlxx.dll)
- Displayed inside the GPU usage gauge

### CPU Temperature
The application uses multiple methods to obtain CPU temperature:

1. **LibreHardwareMonitor** (Primary) - Works on most systems
2. **HWiNFO Shared Memory** (Fallback) - Required for some Intel 12th/13th/14th gen CPUs

### HWiNFO Setup (For CPU Temperature)

If CPU temperature shows 0Â°C or doesn't appear:

1. **Download HWiNFO** from [https://www.hwinfo.com/](https://www.hwinfo.com/)
2. **Install and run HWiNFO** (Sensors-only mode is sufficient)
3. **Enable Shared Memory Support:**
   - Click the **Settings** button (gear icon)
   - Check **"Shared Memory Support"**
   - Click **OK**
4. **Keep HWiNFO running** in the background

> **Why is HWiNFO needed?**  
> Some modern Intel CPUs require a kernel driver to read temperature sensors. HWiNFO has broader hardware support and exposes sensor data through a shared memory interface.

---

## Requirements

### System Requirements
- **Operating System**: Windows 10/11 (x64)
- **.NET Runtime**: .NET 8.0 Desktop Runtime
- **Privileges**: Administrator (for hardware sensor access)

### GPU Drivers (for GPU monitoring)
- **NVIDIA**: Latest GeForce/Quadro drivers
- **AMD**: Latest Radeon drivers

---

## Getting Started

### Installation

#### Option 1: MSI Installer
1. Download the latest `MemoryMonitorSetup.msi` from [Releases](https://github.com/rickbme/Memory-Monitor/releases)
2. Run the installer
3. Launch "Memory Monitor" from the Start Menu

#### Option 2: Portable
1. Download the portable ZIP from [Releases](https://github.com/rickbme/Memory-Monitor/releases)
2. Extract to desired location
3. Run `Memory Monitor.exe` as Administrator

### Running the Application
- Right-click and select **"Run as administrator"**
- Or the application will prompt for elevation automatically

---

## Building from Source

### Prerequisites
- Visual Studio 2022 or later
- .NET 8.0 SDK
- WiX Toolset v4 (for installer)

### Build Steps
```bash
# Clone the repository
git clone https://github.com/rickbme/Memory-Monitor.git
cd Memory-Monitor

# Restore and build
dotnet restore
dotnet build --configuration Release

# Run
cd "Memory Monitor\bin\Release\net8.0-windows\win-x64"
.\Memory Monitor.exe
```

### Creating Installer
```bash
dotnet build MemoryMonitorSetup\MemoryMonitorSetup.wixproj --configuration Release
```

---

##  Project Structure

```
Memory Monitor/
 Memory Monitor/             # Main application
 CompactGaugeControl.cs      # Circular gauge UI control
 MiniMonitorForm.cs          # Main form (1920x480)
 CPUMonitor.cs               # CPU usage & temp monitoring
 GPUMonitor.cs               # GPU usage, VRAM & temp monitoring
 DiskMonitor.cs              # Disk I/O monitoring
 NetworkMonitor.cs           # Network throughput monitoring
 HardwareMonitorService.cs   # LibreHardwareMonitor wrapper
 HWiNFOReader.cs             # HWiNFO shared memory reader
 NVMLInterop.cs              # NVIDIA NVML interop
 ADLInterop.cs               # AMD ADL interop
 app.manifest                # UAC admin manifest
 MemoryMonitorSetup/         # WiX installer project
 README.md
 CHANGELOG.md
```

---

##  Troubleshooting

### CPU Temperature Not Showing
1. Ensure application is running as **Administrator**
2. Install and configure **HWiNFO** (see setup section above)
3. Make sure HWiNFO is running before starting Memory Monitor

### GPU Temperature Not Showing
- **NVIDIA**: Ensure latest GeForce drivers are installed
- **AMD**: Ensure latest Radeon drivers are installed
- Intel integrated graphics may not support temperature reading

### Application Won't Start
1. Ensure .NET 8.0 Runtime is installed
2. Try running as Administrator
3. Check Windows Event Viewer for error details

---

##  GPU Support

### NVIDIA GPUs 
- GeForce series (GTX, RTX)
- Full utilization, memory, and temperature support

### AMD GPUs 
- Radeon RX series
- Full utilization and temperature support

### Intel GPUs 
- Limited support via Performance Counters
- No temperature reading available

---

##  Version History

### v2.4.4 (2025-01)
- **New**: FPS Display menu now available in Bar Graph mode
- **New**: Smooth fade transitions when switching display modes
- **Improved**: Both display modes now have feature parity for FPS options

### v2.4.3 (2025-01)
- **Fixed**: Display mode switching no longer exits the application
- **Fixed**: Taskbar visibility issues in both display modes
- **Fixed**: Welcome dialog and intro logos only show once per session
- **New**: Bar Graph mode has distinct tray icon style

### v2.4.2 (2025-01)
- **New**: Bar Graph display mode with animated panels
- **New**: Display Mode menu for switching between views
- **New**: NetworkBarPanelControl for dual upload/download display

### v2.4.0 (2025-01-10)
- **New**: Date and time display on mini monitor
- **New**: FPS gauge with intelligent game activity detection
- **New**: Color-coded FPS quality indicator (Green/Yellow/Orange/Red)
- **New**: Touch cycling for device selection (GPU, Disk, Network)
- **New**: FPS display mode menu (Auto-detect, Always Show, Always Hide)
- **Improved**: Aggregate mode always available for Disk and Network
- **Improved**: Touch UX optimized for small displays
- **Removed**: CPU temperature warning popup (cleaner experience)

### v2.3.0 (2025)
- **New**: Splash screen with branded DFS logo
- **New**: Mini monitor intro logo (1920x480)
- **New**: Full touch gesture support (swipe, tap, long-press, two-finger tap)
- **New**: Touch-enabled device selection for gauges
- **New**: Toast notifications for visual feedback
- **Changed**: Application minimizes to system tray instead of taskbar
- **Refactored**: MiniMonitorForm split into partial classes for better organization

### v2.2.0 (2024-12-26)
- **New**: Custom gauge-style application icon
- **New**: Dynamic tray icon with color-coded RAM usage (Green/Yellow/Red)
- **New**: GaugeIconGenerator for programmatic icon creation
- **Improved**: Code quality with SafeUpdate wrapper and proper resource disposal
- **Fixed**: Icon format, build configuration, nullable warnings

### v2.1.0 (2024)
- **New**: Borderless full screen mode for clean display
- **New**: Auto monitor detection (4:1 aspect ratio)
- **New**: Move to Next Monitor tray menu option
- **New**: Always on Top toggle (F11 keyboard shortcut)
- **New**: Window dragging support
- **Changed**: Larger gauges using 95% of vertical space
- **Changed**: Increased label font size for better readability

### v2.0.0 (2024)
- **New**: Mini monitor display support (1920x480)
- **New**: Circular gauge design with needle indicators
- **New**: CPU temperature monitoring via LibreHardwareMonitor/HWiNFO
- **New**: GPU temperature display in gauges
- **New**: Disk I/O and Network monitoring
- **New**: Administrator manifest for sensor access
- **Changed**: Complete UI redesign for horizontal displays

### v1.0.0 (2025)
- Initial release
- CPU, GPU, and memory monitoring
- Native NVIDIA and AMD GPU support
- Dark mode theme

---

## Contributing

Contributions are welcome! Please feel to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

##  License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

##  Acknowledgments

- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) - Hardware monitoring library
- [HWiNFO](https://www.hwinfo.com/) - Hardware information and monitoring tool
- NVIDIA for NVML API
- AMD for ADL SDK

---

##  Support

If you encounter issues or have feature requests:
- Open an [Issue](https://github.com/rickbme/Memory-Monitor/issues) on GitHub
- Check existing issues for solutions
- Provide system information and error details

---

**Made by DFS - Dad's Fixit Shop**

*Electronics repair,  Software, & Game Development*
