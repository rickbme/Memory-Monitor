# Memory Monitor - User Guide

**Welcome to Memory Monitor!** ??

Memory Monitor is your real-time system performance dashboard designed for mini displays (1920×480). Keep an eye on your PC's vital stats while you work, game, or browse.

---

## ?? Quick Start

### First Launch
1. Find **"Memory Monitor"** in your Start Menu
2. Click to launch (the app will request Administrator privileges)
3. The application will automatically move to your mini monitor if detected
4. That's it! Your system stats are now visible at a glance

### What You'll See

Your mini monitor will display **6 circular gauges** showing:

| Gauge | What It Shows |
|-------|---------------|
| ?? **RAM** (Blue) | How much memory your PC is using |
| ?? **CPU** (Red) | Processor usage + temperature |
| ?? **GPU** (Orange) | Graphics card usage + temperature |
| ?? **FPS** (Color-coded) | Gaming frame rate (appears during games) |
| ?? **VRAM** (Purple) | Graphics card memory usage |
| ?? **DISK** (Green) | Hard drive/SSD read/write speed |
| ?? **NETWORK** (Yellow) | Internet upload/download speed |

Plus:
- **Date** in the top-left corner
- **Time** in the top-right corner (12-hour format)

---

## ?? Gaming Features

### FPS Display
When you launch a game, Memory Monitor **automatically shows your frame rate** (FPS):
- ?? **Green (60+ FPS)** - Excellent, smooth gameplay
- ?? **Yellow (45-59 FPS)** - Good performance
- ?? **Orange (30-44 FPS)** - Playable but may stutter
- ?? **Red (Below 30 FPS)** - Performance issues

### Control FPS Display
Right-click the **system tray icon** ? **FPS Display**:
- **Auto-detect** - Shows FPS only when gaming (default)
- **Always Show** - Display FPS all the time
- **Always Hide** - Never show FPS

---

## ?? Touchscreen Support

If your mini monitor has a touchscreen, you can use gestures:

### Gestures
- **Tap a gauge** - Cycle through options (GPU, Disk, Network)
- **Swipe left/right** - Switch to another monitor
- **Swipe down** - Minimize to system tray
- **Long press** - Show menu
- **Two-finger tap** - Toggle "Always on Top"

---

## ?? System Tray Controls

Click the **Memory Monitor icon** in your system tray (near the clock):

- **Show** - Restore window if minimized
- **Move to Next Monitor** - Switch to another display
- **Always on Top** - Keep window above other programs
- **FPS Display** - Control when FPS gauge appears
- **Exit** - Close Memory Monitor

---

## ??? Temperature Monitoring

### GPU Temperature
Your graphics card temperature appears **inside the GPU gauge** automatically:
- ? **NVIDIA cards** - Works out of the box
- ? **AMD cards** - Works out of the box
- ?? **Intel integrated graphics** - Limited support

### CPU Temperature
For CPU temperature, you may need to install **HWiNFO** (free):

**If your CPU temperature shows 0°C:**
1. Download **HWiNFO** from https://www.hwinfo.com/
2. Install and run HWiNFO
3. Click the **Settings** button (gear icon)
4. Check **"Shared Memory Support"**
5. Click **OK**
6. Keep HWiNFO running in the background
7. Restart Memory Monitor

> This is especially important for newer Intel CPUs (12th, 13th, 14th gen).

---

## ?? FPS Monitoring Setup (Optional)

To see your gaming frame rate:

1. **Download and install HWiNFO** (if not already done)
   - Get it from https://www.hwinfo.com/
   
2. **Download and install RTSS**
   - Get RivaTuner Statistics Server from https://www.guru3d.com/files-details/rtss-rivatuner-statistics-server-download.html
   
3. **Configure HWiNFO**
   - Launch HWiNFO
   - Go to Settings ? Enable **"Shared Memory Support"**
   - Click OK
   
4. **Launch your game**
   - RTSS will display FPS in-game
   - Memory Monitor will automatically show FPS on your mini display

---

## ??? Multiple GPUs, Disks, or Networks?

If you have multiple graphics cards, hard drives, or network adapters:

### Using Touch
- **Tap the gauge** repeatedly to cycle through options
- A notification shows which device is selected

### Using Mouse
- **Click the gauge** to open a menu
- Select the device you want to monitor

### Examples
- Multiple GPUs: Choose which graphics card to monitor
- Multiple Disks: Switch between "All Disks" or specific drives
- WiFi + Ethernet: Choose which network adapter to track

---

## ?? Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **Escape** | Minimize to system tray |
| **F11** | Toggle "Always on Top" |

---

## ??? Troubleshooting

### "Application won't start"
- Right-click **Memory Monitor** ? **Run as administrator**
- Make sure you have **.NET 8.0 Desktop Runtime** installed
- Download from https://dotnet.microsoft.com/download/dotnet/8.0

### "CPU temperature shows 0°C"
- Install and configure **HWiNFO** (see Temperature Monitoring section)
- Make sure HWiNFO is running before starting Memory Monitor
- Enable "Shared Memory Support" in HWiNFO settings

### "FPS gauge doesn't appear"
- Install both **HWiNFO** and **RTSS** (see FPS Monitoring Setup)
- Make sure HWiNFO has "Shared Memory Support" enabled
- Launch a game with RTSS overlay active
- Check tray menu ? FPS Display is set to "Auto-detect" or "Always Show"

### "Window is on the wrong monitor"
- Right-click **system tray icon** ? **Move to Next Monitor**
- Or drag the window to your preferred display

### "Gauges look too small/large"
- The app automatically sizes for 1920×480 displays
- Try maximizing the window or resizing manually

---

## ?? Updating Memory Monitor

When a new version is released:
1. Download the new **MemoryMonitorSetup.msi**
2. Run the installer
3. It will automatically upgrade your existing installation
4. Your settings and preferences are preserved

---

## ?? What Each Gauge Means

### RAM (Blue Gauge)
- Shows how much memory your programs are using
- Example: "12.3/32" means 12.3 GB used out of 32 GB total
- Higher usage is normal when running many programs

### CPU (Red Gauge)
- Shows processor workload percentage
- Temperature appears below the needle
- High usage during gaming/video editing is normal

### GPU (Orange Gauge)
- Shows graphics card workload
- Temperature appears below the needle
- Spikes during gaming, video playback, or 3D rendering

### VRAM (Purple Gauge)
- Shows graphics card memory usage
- Example: "8.2/24" means 8.2 GB used out of 24 GB total
- Increases with high-resolution textures and multiple monitors

### DISK (Green Gauge)
- Shows hard drive/SSD read and write speed
- Measured in Mbps (megabits per second)
- Spikes when copying files or loading games

### NETWORK (Yellow Gauge)
- Shows internet upload and download speed
- Measured in Mbps (megabits per second)
- Spikes when streaming, downloading, or uploading

### FPS (Color-Coded Gauge)
- Shows gaming frame rate
- Only appears when playing games (by default)
- Green = smooth, Yellow = good, Orange = acceptable, Red = choppy

---

## ?? Tips & Tricks

### Best Placement
- Position your mini monitor **below your main display**
- Or to the **side** for easy glancing during games

### Multi-Monitor Setup
- Memory Monitor auto-detects 1920×480 displays
- Manual switching: Tray icon ? Move to Next Monitor

### System Tray Only
- Press **Escape** to minimize Memory Monitor
- It continues monitoring in the background
- Double-click the tray icon to restore

### Touch-Friendly
- If your mini monitor has touch, use **gestures** instead of mouse clicks
- Much easier for small displays!

---

## ?? Getting Help

### Need Support?
- Check the **Troubleshooting** section above
- Visit the GitHub page: https://github.com/rickbme/Memory-Monitor
- Report issues: https://github.com/rickbme/Memory-Monitor/issues

### Found a Bug?
Please report it with:
- Windows version (e.g., "Windows 11 23H2")
- What went wrong
- Steps to reproduce the issue

---

## ?? Requirements

### Minimum System Requirements
- **Windows 10** or **Windows 11** (64-bit)
- **.NET 8.0 Desktop Runtime** (installer includes this)
- **Administrator privileges** (for sensor access)

### Optional for Full Features
- **HWiNFO** - For CPU temperature and FPS monitoring
- **RTSS** - For FPS display in games
- **Touchscreen mini monitor** - For touch gestures

---

## ?? License & Credits

Memory Monitor is **free and open source** software.

**Created by:** DFS - Dad's Fixit Shop  
**License:** MIT License  
**GitHub:** https://github.com/rickbme/Memory-Monitor

### Special Thanks
- LibreHardwareMonitor team
- HWiNFO developers
- NVIDIA and AMD for GPU APIs

---

## ?? Enjoy Monitoring!

Memory Monitor runs quietly in the background, giving you instant visibility into your PC's performance. Perfect for gamers, content creators, and power users who want to keep tabs on their system.

**Questions? Issues? Suggestions?**  
Visit our GitHub page or open an issue. We're here to help!

---

**Version:** 2.4.0  
**Release Date:** January 10, 2025  
**Made with ?? by Dad's Fixit Shop**
