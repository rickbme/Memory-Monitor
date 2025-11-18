# ??? System Monitor

**A modern, feature-rich Windows system monitoring application built with .NET 8**

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6?logo=windows)
![License](https://img.shields.io/badge/license-MIT-green)

---

## ?? Overview

System Monitor is a comprehensive real-time monitoring tool for Windows that provides detailed insights into your system's performance. Built with modern .NET 8 and Windows Forms, it offers accurate hardware monitoring with native GPU support through NVML (NVIDIA) and ADL (AMD) APIs.

### ? Key Features

- **Real-Time CPU Monitoring** - Track CPU usage with performance counters
- **GPU Monitoring** - Native NVIDIA (NVML) and AMD (ADL) GPU support
  - GPU utilization percentage
  - VRAM usage and total memory
  - Direct hardware access for accurate readings
- **System Memory Tracking** - Monitor RAM usage with detailed statistics
- **Process Management** - View processes using more than 400MB RAM
- **Historical Graphs** - 60-second history for all metrics
- **Dark Mode** - Eye-friendly dark theme with persistent preference
- **Resizable Interface** - Flexible window sizing with dynamic layout
- **Modern UI** - Clean, professional design with smooth animations

---

## ?? Features in Detail

### ?? Monitoring Capabilities

#### CPU Usage
- Real-time CPU utilization percentage
- Per-second updates
- Historical trend graph (60 seconds)
- Performance counter-based tracking

#### GPU Monitoring
- **NVIDIA GPUs** (via NVML):
  - Accurate GPU utilization
  - Dedicated VRAM usage (used/total)
  - Direct driver communication
  
- **AMD GPUs** (via ADL):
  - GPU activity percentage
  - Total VRAM detection
  - Native driver integration

- **Intel GPUs**:
  - Performance counter fallback
  - Basic utilization tracking

#### Memory Tracking
- **System RAM**:
  - Used vs. available memory
  - Percentage utilization
  - GB and percentage display
  
- **GPU VRAM**:
  - Dedicated memory usage
  - Total VRAM size
  - Real-time updates

#### Process List
- Displays processes using > 400MB RAM
- Sorted by memory usage (highest first)
- Shows:
  - Process name
  - Memory in GB
  - Memory in MB (formatted)
- Custom-drawn with visual markers
- No grid lines for clean appearance

### ?? User Interface

#### Themes
- **Light Mode**: Bright, professional appearance
- **Dark Mode**: Eye-friendly with reduced brightness
- Persistent theme preference (saved to user settings)
- Instant theme switching via menu

#### Layout
- **2-Column Grid Design**:
  - CPU Usage | GPU Usage
  - System Memory | GPU Memory
  - Process list spans full width
- **Resizable Window**:
  - Minimum size: 680×650 pixels
  - Graphs scale proportionally
  - Process list expands vertically
  - Dynamic layout calculations

#### Graphs
- **Line graphs** for all metrics
- 60-second rolling history
- Smooth anti-aliased rendering
- Color-coded:
  - CPU/System Memory: Blue
  - GPU/GPU Memory: Green
- Grid lines for reference
- Gradient fill under lines

---

## ?? Getting Started

### Requirements

- **Operating System**: Windows 10/11 (x64)
- **.NET Runtime**: .NET 8.0 Desktop Runtime
- **GPU Drivers** (for native GPU monitoring):
  - NVIDIA: Latest GeForce/Quadro drivers
  - AMD: Latest Radeon drivers

### Installation

1. **Download** the latest release from the [Releases](https://github.com/rickbme/Memory-Monitor/releases) page
2. **Extract** the ZIP file to your desired location
3. **Run** `Memory Monitor.exe`

### Building from Source

```bash
# Clone the repository
git clone https://github.com/rickbme/Memory-Monitor.git
cd Memory-Monitor

# Build the project
dotnet build --configuration Release

# Run the application
dotnet run --project "Memory Monitor"
```

---

## ?? Configuration

### Settings
- **Theme Preference**: Automatically saved to `%AppData%\Memory Monitor\`
- **Window Size**: Remembers last size and position
- **Update Interval**: 1 second (hardcoded for optimal performance)

---

## ??? Architecture

### Technology Stack
- **.NET 8**: Modern framework with performance improvements
- **Windows Forms**: Native Windows UI framework
- **System.Management**: WMI access for system information
- **Microsoft.VisualBasic**: ComputerInfo for memory stats

### GPU Monitoring
```
????????????????????????????????
?  GPU Detection (WMI)         ?
?    ?                         ?
?  Vendor Identification       ?
?    ?? NVIDIA ? NVML          ?
?    ?? AMD ? ADL              ?
?    ?? Intel ? Perf Counters  ?
?    ?                         ?
?  Fallback Chain:             ?
?  Native API ? Perf Counter   ?
?  ? WMI Estimation            ?
????????????????????????????????
```

### Project Structure
```
Memory Monitor/
??? Form1.cs                    # Main application window
??? Form1.Designer.cs           # UI layout definition
??? AboutForm.cs                # About dialog
??? CPUMonitor.cs               # CPU monitoring logic
??? GPUMonitor.cs               # GPU detection & monitoring
??? NVMLInterop.cs              # NVIDIA NVML wrapper
??? ADLInterop.cs               # AMD ADL wrapper
??? ThemeManager.cs             # Theme management
??? MemoryGraphControl.cs       # Custom graph control
??? Properties/
    ??? Settings.settings       # User preferences
```

### Key Classes

#### **CPUMonitor**
- Encapsulates CPU monitoring
- Uses PerformanceCounter
- Provides Update() method for readings

#### **GPUMonitor**
- Detects GPU vendor (NVIDIA/AMD/Intel)
- Initializes appropriate native API
- Falls back to performance counters
- Provides UpdateUsage() and UpdateMemory()

#### **NVMLInterop**
- P/Invoke wrapper for nvml.dll
- Direct GPU hardware access
- Accurate memory and utilization tracking

#### **ADLInterop**
- P/Invoke wrapper for atiadlxx.dll
- AMD GPU detection and monitoring
- Adapter enumeration

#### **ThemeManager**
- Static theme state management
- Light and Dark color palettes
- Persistent settings

#### **MemoryGraphControl**
- Custom UserControl
- Renders line graphs
- 60-point rolling buffer
- Anti-aliased drawing

---

## ?? Usage

### Menu Options

#### View Menu
- **Dark Mode**: Toggle between light and dark themes

#### Help Menu
- **About**: Display application information

### Keyboard Shortcuts
- `Alt+V`: Open View menu
- `Alt+H`: Open Help menu

### Resizing
- Drag window edges or corners to resize
- Double-click title bar to maximize
- Minimum size enforced for usability

---

## ?? GPU Support

### NVIDIA GPUs
? **Supported via NVML** (nvml.dll)
- GeForce series (GTX, RTX)
- Quadro professional cards
- Tesla datacenter GPUs

**Requirements**: Latest NVIDIA drivers

### AMD GPUs
? **Supported via ADL** (atiadlxx.dll)
- Radeon RX series
- Radeon Vega
- Radeon RDNA/RDNA2/RDNA3

**Requirements**: Latest AMD Adrenalin drivers

### Intel GPUs
?? **Limited Support**
- Uses Windows Performance Counters
- Basic utilization only
- No native API available

### Fallback Behavior
If native APIs are unavailable:
1. Attempts Performance Counter access
2. Falls back to WMI for basic info
3. Gracefully shows "N/A" if unavailable

---

## ?? Themes

### Light Mode
- Clean, professional appearance
- High contrast for bright environments
- White backgrounds with dark text

### Dark Mode
- Reduced eye strain
- Modern dark gray backgrounds
- Light text for readability
- Perfect for low-light environments

**Theme persistence**: Your preference is saved and restored on next launch.

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

## ????? Author

**DFS - Dad's Fixit Shop**
- Electronics • Software • Game Development
- Making tech accessible, reliable, and fun since 2025

---

## ?? Acknowledgments

- NVIDIA for NVML API documentation
- AMD for ADL SDK
- Microsoft for .NET platform
- Windows Forms community

---

## ?? Support

If you encounter any issues or have questions:
- Open an [Issue](https://github.com/rickbme/Memory-Monitor/issues)
- Check existing issues for solutions
- Provide system information and error details

---

## ?? Version History

### v1.0.0 (2025-11-18)
- Initial release
- CPU, GPU, and memory monitoring
- Native NVIDIA and AMD GPU support
- Dark mode theme
- Resizable interface
- Process list viewer

---

## ?? Future Enhancements

- [ ] Network usage monitoring
- [ ] Disk I/O statistics
- [ ] Temperature sensors
- [ ] Export data to CSV
- [ ] System tray mode
- [ ] Alerts and notifications
- [ ] Multi-language support
- [ ] Customizable update intervals

---

**Made with ?? by DFS - Dad's Fixit Shop**
