# Memory Monitor - Linux Port

This is a full Linux port of the Memory Monitor application, originally designed for Windows. The application has been completely rewritten to use cross-platform technologies while maintaining all core functionality.

## Architecture Changes

### UI Framework: Windows Forms → Avalonia

The application now uses **Avalonia UI** instead of Windows Forms:
- **Avalonia** is a modern, cross-platform XAML-based UI framework
- Supports Linux, macOS, and Windows
- Similar to WPF/WinForms but cross-platform
- Native look and feel on each platform

### Hardware Monitoring: Platform-Specific Implementations

All hardware monitoring has been rewritten for Linux using native system APIs:

#### CPU Monitoring
- **Windows**: `PerformanceCounter` API
- **Linux**: `/proc/stat` for CPU usage, `/sys/class/thermal` for temperature
- Supports multi-core CPU monitoring
- Real-time temperature readings via thermal zones

#### Memory Monitoring
- **Windows**: `System.Management` WMI queries
- **Linux**: `/proc/meminfo` parsing
- Tracks total, used, and available memory
- Accurate memory statistics

#### GPU Monitoring
- **NVIDIA**:
  - **Windows**: `nvml.dll`
  - **Linux**: `libnvidia-ml.so.1` (NVML)
  - Requires NVIDIA proprietary drivers
- **AMD**:
  - **Windows**: `atiadlxx.dll` (ADL)
  - **Linux**: `/sys/class/drm/cardX` sysfs interface
  - Works with open-source AMDGPU drivers
- Monitors GPU usage, memory, and temperature

#### Disk Monitoring
- **Windows**: `PerformanceCounter` for disk I/O
- **Linux**: `/proc/diskstats` parsing
- Tracks read/write speeds in MB/s
- Supports multiple disks

#### Network Monitoring
- **Windows**: `PerformanceCounter` for network stats
- **Linux**: `/proc/net/dev` parsing
- Monitors download/upload speeds
- Supports multiple network interfaces

## Building on Linux

### Prerequisites

1. **.NET 8 SDK** or later:
   ```bash
   wget https://dot.net/v1/dotnet-install.sh
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 8.0
   ```

2. **Development libraries**:
   ```bash
   # Debian/Ubuntu
   sudo apt-get update
   sudo apt-get install libx11-dev libice-dev libsm-dev libfontconfig1-dev

   # Fedora/RHEL
   sudo dnf install libX11-devel libICE-devel libSM-devel fontconfig-devel

   # Arch Linux
   sudo pacman -S libx11 libice libsm fontconfig
   ```

3. **NVIDIA Drivers** (optional, for NVIDIA GPU monitoring):
   ```bash
   # Check if NVIDIA drivers are installed
   nvidia-smi
   
   # If not installed, follow your distribution's guide to install NVIDIA drivers
   ```

### Building the Application

1. **Clone the repository** (or navigate to the project directory):
   ```bash
   cd /home/rick/repos/Memory-Monitor
   ```

2. **Run the build script**:
   ```bash
   ./build-linux.sh
   ```

   This will:
   - Restore NuGet packages
   - Build the project in Release mode
   - Create a self-contained Linux binary

3. **Output location**:
   ```
   bin/Release/net8.0/linux-x64/publish/
   ```

### Manual Build (Alternative)

If you prefer to build manually:

```bash
cd "Memory Monitor"

# Restore dependencies
dotnet restore

# Build
dotnet build -c Release

# Publish self-contained binary
dotnet publish -c Release -r linux-x64 --self-contained true
```

## Running the Application

### From Build Directory

```bash
cd bin/Release/net8.0/linux-x64/publish
./Memory\ Monitor
```

### Installing System-Wide

1. **Copy to system directory**:
   ```bash
   sudo mkdir -p /usr/local/bin/memory-monitor
   sudo cp -r bin/Release/net8.0/linux-x64/publish/* /usr/local/bin/memory-monitor/
   sudo chmod +x /usr/local/bin/memory-monitor/Memory\ Monitor
   ```

2. **Install desktop entry**:
   ```bash
   sudo cp memory-monitor.desktop /usr/share/applications/
   sudo chmod +x /usr/share/applications/memory-monitor.desktop
   ```

3. **Launch from application menu** or run:
   ```bash
   /usr/local/bin/memory-monitor/Memory\ Monitor
   ```

## Permissions

### GPU Monitoring

For GPU monitoring to work properly, you may need additional permissions:

#### NVIDIA
The application reads from NVML library which typically requires no special permissions.

#### AMD
AMD GPU monitoring uses sysfs, which may require read permissions:
```bash
# Add your user to the video group
sudo usermod -a -G video $USER

# Log out and log back in for changes to take effect
```

### System Monitoring

Most system monitoring (`/proc` filesystem) works without special permissions, but some features may require:
- **Temperature sensors**: Usually accessible to all users
- **Hardware info**: May require membership in `video` or `render` groups

## Platform Abstraction

The application uses a factory pattern to provide platform-specific implementations:

```csharp
// MonitorFactory.cs automatically detects the platform
var cpuMonitor = MonitorFactory.CreateCPUMonitor();
var gpuMonitor = MonitorFactory.CreateGPUMonitor();
// ...
```

On Linux, this creates Linux-specific monitor classes:
- `LinuxCPUMonitor`
- `LinuxMemoryMonitor`
- `LinuxGPUMonitor`
- `LinuxDiskMonitor`
- `LinuxNetworkMonitor`

On Windows, it would create the original Windows implementations.

## Dependencies

### Runtime Dependencies
- .NET 8 Runtime (included in self-contained build)
- X11 or Wayland display server
- FontConfig
- Basic system libraries (libX11, libICE, libSM)

### Optional Dependencies
- **NVIDIA GPU Monitoring**: NVIDIA proprietary drivers with libnvidia-ml.so.1
- **AMD GPU Monitoring**: AMDGPU kernel drivers (usually included in modern kernels)
- **LibreHardwareMonitor**: For advanced hardware monitoring (temperature sensors, etc.)

## NuGet Packages Used

- **Avalonia** (11.0.10): Cross-platform UI framework
- **Avalonia.Desktop** (11.0.10): Desktop-specific features
- **Avalonia.Themes.Fluent** (11.0.10): Modern Fluent theme
- **Avalonia.Fonts.Inter** (11.0.10): Inter font family
- **Avalonia.Diagnostics** (11.0.10): Development tools

## Known Limitations

1. **Multi-monitor support**: Currently optimized for single monitor or primary display
2. **Touch gestures**: Not yet implemented for Linux (was Windows-specific)
3. **System tray**: Needs Linux-specific implementation
4. **Game detection**: FPS overlay feature not yet ported to Linux

## Troubleshooting

### Application doesn't start
- Check if .NET 8 runtime is installed: `dotnet --version`
- Verify required libraries are installed
- Check console output for error messages: `./Memory\ Monitor 2>&1 | tee error.log`

### GPU not detected
- **NVIDIA**: Verify drivers are installed: `nvidia-smi`
- **AMD**: Check if card is visible: `ls /sys/class/drm/card*`
- **Intel**: Intel GPU monitoring is limited

### Permission denied errors
- Add user to appropriate groups: `sudo usermod -a -G video,render $USER`
- Log out and back in for group changes to take effect

### High CPU usage
- Increase update interval in the code (default is 1000ms)
- Disable unused monitors

## Development

### Project Structure
```
Memory Monitor/
├── Linux/                    # Linux-specific implementations
│   ├── LinuxCPUMonitor.cs
│   ├── LinuxMemoryMonitor.cs
│   ├── LinuxGPUMonitor.cs
│   ├── LinuxDiskMonitor.cs
│   ├── LinuxNetworkMonitor.cs
│   └── LinuxNVMLInterop.cs
├── MonitorFactory.cs         # Platform detection and factory
├── App.axaml                 # Avalonia application XAML
├── App.axaml.cs              # Avalonia application code-behind
├── MainWindow.axaml          # Main window XAML
├── MainWindow.axaml.cs       # Main window code-behind
├── Program.cs                # Entry point (Avalonia)
└── ... (original files)
```

### Adding New Features

1. **Linux-specific features**: Add to `Memory Monitor/Linux/` directory
2. **Cross-platform features**: Add to root directory and use `MonitorFactory` for platform-specific parts
3. **UI changes**: Modify Avalonia XAML files and code-behind

### Testing
- Test on multiple Linux distributions (Ubuntu, Fedora, Arch)
- Test with different hardware configurations
- Verify GPU monitoring with NVIDIA and AMD cards

## Future Improvements

- [ ] Advanced gauge controls (port from Windows version)
- [ ] System tray support for Linux
- [ ] Multi-monitor configuration
- [ ] Custom themes and layouts
- [ ] Configuration file support
- [ ] Packaging (.deb, .rpm, AppImage, Flatpak)
- [ ] Intel GPU monitoring improvements
- [ ] Wayland-native support

## Contributing

Contributions are welcome! Areas that need work:
- UI/UX improvements
- Additional hardware support
- Packaging for different distributions
- Documentation improvements
- Bug fixes

## License

[Your license information here]

## Credits

- Original Windows version: [Original author]
- Linux port: [Your name]
- Avalonia UI: https://avaloniaui.net/
- LibreHardwareMonitor: https://github.com/LibreHardwareMonitor/LibreHardwareMonitor
