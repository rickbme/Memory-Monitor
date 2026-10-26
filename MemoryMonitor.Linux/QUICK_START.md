# Quick Start Guide - Linux Port

## What Was Done

Your Memory Monitor Windows application has been **completely ported to Linux**! Here's what changed:

### 🔄 Major Changes

1. **UI Framework**: Windows Forms → **Avalonia UI** (cross-platform)
2. **CPU Monitoring**: PerformanceCounter → **/proc/stat** + **/sys/class/thermal**
3. **Memory Monitoring**: WMI → **/proc/meminfo**
4. **GPU Monitoring (NVIDIA)**: nvml.dll → **libnvidia-ml.so.1**
5. **GPU Monitoring (AMD)**: ADL → **/sys/class/drm** (sysfs)
6. **Disk Monitoring**: PerformanceCounter → **/proc/diskstats**
7. **Network Monitoring**: PerformanceCounter → **/proc/net/dev**

All core monitoring features work on Linux using native APIs!

## 📁 New Files Created

### Linux Monitors (in `Memory Monitor/Linux/`)
- `LinuxCPUMonitor.cs` - CPU usage & temperature
- `LinuxMemoryMonitor.cs` - System memory stats
- `LinuxGPUMonitor.cs` - GPU monitoring (NVIDIA/AMD)
- `LinuxNVMLInterop.cs` - NVIDIA GPU library interface
- `LinuxDiskMonitor.cs` - Disk I/O stats
- `LinuxNetworkMonitor.cs` - Network traffic stats

### Platform Abstraction
- `MonitorFactory.cs` - Auto-detects OS and creates appropriate monitors

### Avalonia UI
- `App.axaml` + `App.axaml.cs` - Application definition
- `MainWindow.axaml` + `MainWindow.axaml.cs` - Main monitoring window
- `Program.cs` - Updated entry point for Avalonia

### Build & Deploy
- `build-linux.sh` - Automated build script
- `memory-monitor.desktop` - Linux application launcher
- `LINUX_PORT_README.md` - Comprehensive documentation
- `LINUX_PORT_SUMMARY.md` - Complete migration summary

## 🚀 How to Build & Run

### Step 1: Install .NET 8 SDK

```bash
# Download and install .NET 8
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Add to PATH (if needed)
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc
```

### Step 2: Install System Dependencies

```bash
# Debian/Ubuntu
sudo apt-get update
sudo apt-get install libx11-dev libice-dev libsm-dev libfontconfig1-dev

# Fedora
sudo dnf install libX11-devel libICE-devel libSM-devel fontconfig-devel

# Arch Linux
sudo pacman -S libx11 libice libsm fontconfig
```

### Step 3: Build the Application

```bash
# Navigate to project directory
cd /home/rick/repos/Memory-Monitor

# Run build script
./build-linux.sh
```

### Step 4: Run the Application

```bash
# Navigate to output directory
cd bin/Release/net8.0/linux-x64/publish

# Run the application
./Memory\ Monitor
```

## 🎯 What Works

✅ **CPU Monitoring**
- Usage percentage (all cores)
- Temperature reading

✅ **Memory Monitoring**
- Total, used, available memory
- Usage percentage

✅ **GPU Monitoring**
- NVIDIA GPUs (via NVML)
- AMD GPUs (via sysfs)
- GPU usage, memory, temperature
- Multi-GPU support

✅ **Disk Monitoring**
- Read/write speeds (MB/s)
- Multiple disk support

✅ **Network Monitoring**
- Download/upload speeds (Mbps)
- Multiple interface support

✅ **UI**
- Real-time updates (1 second interval)
- Dark theme
- All metrics displayed

## ⚙️ Optional: GPU Monitoring Setup

### For NVIDIA GPUs

NVIDIA monitoring requires proprietary drivers:

```bash
# Check if drivers are installed
nvidia-smi

# If not installed, install NVIDIA drivers
# (Command varies by distribution)
# Ubuntu:
sudo ubuntu-drivers autoinstall

# Fedora:
sudo dnf install akmod-nvidia

# Arch:
sudo pacman -S nvidia nvidia-utils
```

### For AMD GPUs

AMD monitoring works with open-source drivers (usually pre-installed):

```bash
# Verify AMD GPU is detected
ls /sys/class/drm/card*/device/vendor

# Should show "0x1002" for AMD cards

# If permission issues, add user to video group
sudo usermod -a -G video $USER
# Log out and back in
```

## 📊 How It Works

The application uses a **factory pattern** to detect your OS:

```csharp
// Automatically detects Linux and creates Linux monitors
var cpuMonitor = MonitorFactory.CreateCPUMonitor();
// → Creates LinuxCPUMonitor on Linux
// → Creates CPUMonitor on Windows
```

Each monitor reads from native Linux interfaces:
- **CPU**: Parses `/proc/stat` for usage, `/sys/class/thermal` for temp
- **Memory**: Parses `/proc/meminfo`
- **GPU (NVIDIA)**: Uses NVML library (`libnvidia-ml.so.1`)
- **GPU (AMD)**: Reads from `/sys/class/drm/cardX/device/*`
- **Disk**: Parses `/proc/diskstats`
- **Network**: Parses `/proc/net/dev`

## 🔧 Troubleshooting

### .NET SDK not found
```bash
# Verify installation
dotnet --version

# Should show 8.0.x or higher
```

### Build errors about LibreHardwareMonitor
```bash
# The project references LibreHardwareMonitor from parent directory
# Make sure it exists at:
ls ../../LibreHardwareMonitor/LibreHardwareMonitorLib/

# If missing, you can:
# 1. Clone LibreHardwareMonitor repo
# 2. Or remove the project reference and comment out temperature code
```

### GPU not detected
```bash
# NVIDIA: Check drivers
nvidia-smi

# AMD: Check sysfs
ls /sys/class/drm/card*/device/vendor

# Permissions
sudo usermod -a -G video,render $USER
```

### Application won't start
```bash
# Run with error output
cd bin/Release/net8.0/linux-x64/publish
./Memory\ Monitor 2>&1 | tee error.log

# Check the error.log file for details
```

## 📖 Documentation

- **LINUX_PORT_README.md** - Comprehensive guide with architecture details
- **LINUX_PORT_SUMMARY.md** - Complete list of changes and migration summary
- **QUICK_START.md** - This file!

## 🎉 Success!

You now have a fully functional Linux version of Memory Monitor that:
- Uses native Linux APIs for all monitoring
- Has a cross-platform UI with Avalonia
- Supports NVIDIA and AMD GPUs
- Can be built and run on any modern Linux distribution

Enjoy your newly ported application! 🐧
