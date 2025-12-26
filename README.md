# ??? System Monitor

**A modern, feature-rich Windows system monitoring application built with .NET 8**

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6?logo=windows)
![License](https://img.shields.io/badge/license-MIT-green)

---

## ?? Overview

System Monitor is a comprehensive real-time monitoring tool for Windows designed for **secondary/mini displays (1920x480)**. Built with modern .NET 8 and Windows Forms, it features beautiful circular gauge visualizations with temperature monitoring for both CPU and GPU.

### ? Key Features

- **Real-Time CPU Monitoring** - Track CPU usage with temperature display
- **GPU Monitoring** - Native NVIDIA (NVML) and AMD (ADL) support with temperature
- **System Memory Tracking** - Monitor RAM usage with detailed statistics
- **GPU VRAM Monitoring** - Video memory usage and total capacity
- **Disk I/O Monitoring** - Read/write throughput with auto-scaling
- **Network Monitoring** - Upload/download speed with auto-scaling
- **Temperature Display** - CPU and GPU temperatures shown inside gauges
- **Mini Monitor Optimized** - Designed for 1920x480 secondary displays
- **Circular Gauge Design** - Beautiful animated needle gauges with glow effects

---

## ?? Display Layout

The application displays 6 gauges in a horizontal row, optimized for 1920x480 displays:

| Gauge | Color | Information |
|-------|-------|-------------|
| **RAM** | Blue | Memory usage (GB used / total) |
| **CPU** | Red | CPU usage (%) + Temperature |
| **GPU** | Orange | GPU usage (%) + Temperature |
| **VRAM** | Purple | Video memory (GB used / total) |
| **DISK** | Green | Disk I/O throughput (MB/s) |
| **NET** | Yellow | Network throughput (Mbps) |

---

## ??? Temperature Monitoring

### GPU Temperature
- **NVIDIA GPUs** - Native support via NVML (nvml.dll)
- **AMD GPUs** - Native support via ADL (atiadlxx.dll)
- Displayed inside the GPU usage gauge

### CPU Temperature
The application uses multiple methods to obtain CPU temperature:

1. **LibreHardwareMonitor** (Primary) - Works on most systems
2. **HWiNFO Shared Memory** (Fallback) - Required for some Intel 12th/13th/14th gen CPUs

### HWiNFO Setup (For CPU Temperature)

If CPU temperature shows 0°C or doesn't appear:

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

## ?? Requirements

### System Requirements
- **Operating System**: Windows 10/11 (x64)
- **.NET Runtime**: .NET 8.0 Desktop Runtime
- **Privileges**: Administrator (for hardware sensor access)

### GPU Drivers (for GPU monitoring)
- **NVIDIA**: Latest GeForce/Quadro drivers
- **AMD**: Latest Radeon drivers

---

## ?? Getting Started

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

## ?? Building from Source

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

## ??? Project Structure

```
Memory Monitor/
??? Memory Monitor/               # Main application
?   ??? CompactGaugeControl.cs   # Circular gauge UI control
?   ??? MiniMonitorForm.cs       # Main form (1920x480)
?   ??? CPUMonitor.cs            # CPU usage & temp monitoring
?   ??? GPUMonitor.cs            # GPU usage, VRAM & temp monitoring
?   ??? DiskMonitor.cs           # Disk I/O monitoring
?   ??? NetworkMonitor.cs        # Network throughput monitoring
?   ??? HardwareMonitorService.cs # LibreHardwareMonitor wrapper
?   ??? HWiNFOReader.cs          # HWiNFO shared memory reader
?   ??? NVMLInterop.cs           # NVIDIA NVML interop
?   ??? ADLInterop.cs            # AMD ADL interop
?   ??? app.manifest             # UAC admin manifest
??? MemoryMonitorSetup/          # WiX installer project
??? README.md
??? CHANGELOG.md
```

---

## ?? Troubleshooting

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

## ?? GPU Support

### NVIDIA GPUs ?
- GeForce series (GTX, RTX)
- Full utilization, memory, and temperature support

### AMD GPUs ?
- Radeon RX series
- Full utilization and temperature support

### Intel GPUs ??
- Limited support via Performance Counters
- No temperature reading available

---

## ?? Version History

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

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## ?? Acknowledgments

- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) - Hardware monitoring library
- [HWiNFO](https://www.hwinfo.com/) - Hardware information and monitoring tool
- NVIDIA for NVML API
- AMD for ADL SDK

---

## ?? Support

If you encounter issues or have feature requests:
- Open an [Issue](https://github.com/rickbme/Memory-Monitor/issues) on GitHub
- Check existing issues for solutions
- Provide system information and error details

---

**Made with ?? by DFS - Dad's Fixit Shop**

*Electronics • Software • Game Development*
