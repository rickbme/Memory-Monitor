# Memory Monitor - User Guide

**Welcome to Memory Monitor!**

Memory Monitor is your real-time system performance dashboard designed for mini displays (1920×480). Keep an eye on your PC's vital stats while you work, game, or browse.

---

## Quick Start

### First Launch
1. Find **"Memory Monitor"** in your Start Menu (or use the desktop shortcut)
2. Click to launch and approve the administrator prompt (needed to read hardware sensors)
3. The application automatically moves to your mini monitor if one is detected
4. That's it! Your system stats are now visible at a glance

### What You'll See

Memory Monitor offers two display modes. By default, you'll see **Circular Gauges**:

| Gauge | What It Shows |
|-------|---------------|
| **RAM** (Blue) | How much memory your PC is using |
| **CPU** (Red) | Processor usage + temperature |
| **GPU** (Orange) | Graphics card usage + temperature |
| **FPS** (Color-coded) | Game frame rate (appears while a game reports it) |
| **VRAM** (Purple) | Graphics card memory usage |
| **DISK** (Green) | Hard drive/SSD read + write speed |
| **NETWORK** (Yellow) | Upload + download speed |

Plus:
- **Date** in the top-left corner
- **Time** in the top-right corner (12-hour format)

---

## Display Modes

Memory Monitor offers two ways to view your system stats:

### Circular Gauges Mode (Default)
- Classic needle gauge visualization
- 6 circular gauges in a horizontal row
- FPS gauge appears between GPU and VRAM when gaming
- Date and time displayed in corners
- Animated needle movements with glow effects

### Bar Graph Mode
- Modern animated bar graph panels
- 5 panels: CPU, GPU, VRAM, Drive, Network
- Rolling history animation showing recent activity
- FPS shown in the GPU panel when gaming
- Gradient-filled bars with futuristic styling

### Switching Between Modes
1. Right-click the **Memory Monitor icon** in your system tray
2. Click **Display Mode**
3. Choose **Circular Gauges** or **Bar Graph**

The switch uses a smooth fade animation, and Memory Monitor remembers your choice for next time.

---

## Gaming Features

### FPS Display
While a game is running, Memory Monitor **automatically shows your frame rate** (FPS):
- **Green (60+ FPS)** - Excellent, smooth gameplay
- **Yellow (45-59 FPS)** - Good performance
- **Orange (30-44 FPS)** - Playable but may stutter
- **Red (Below 30 FPS)** - Performance issues

**Where FPS appears:**
- **Circular Gauges mode**: Dedicated FPS gauge between GPU and VRAM
- **Bar Graph mode**: FPS shown in the GPU panel

FPS needs **HWiNFO** and **RTSS** - see [FPS Monitoring Setup](#fps-monitoring-setup-optional).

### Control FPS Display
Right-click the **system tray icon** → **FPS Display**:
- **Auto-detect** (default) - Shows FPS whenever a game is reporting it
- **Always Show** - Shows FPS whenever HWiNFO has a reading
- **Always Hide** - Never show FPS

Your choice is remembered, including when you switch display modes or restart.

---

## Touchscreen Support

If your mini monitor has a touchscreen, you can use gestures:

- **Tap a gauge** - Cycle through devices (GPU, Disk, Network) - a notification shows the new one
- **Swipe left/right** - Switch to another monitor
- **Swipe down** - Minimize to system tray
- **Long press** - Show the menu
- **Two-finger tap** - Toggle "Always on Top"

> Touch needs the monitor's USB data cable connected (not just power) so Windows sees the touchscreen.

---

## System Tray Controls

Right-click the **Memory Monitor icon** in your system tray (near the clock):

- **Show** - Restore the window if minimized
- **Move to Next Monitor** - Switch to another display
- **Always on Top** - Keep the window above other programs
- **Display Mode** - Switch between Circular Gauges and Bar Graph
- **FPS Display** - Control when FPS appears
- **Exit** - Close Memory Monitor

Double-click the tray icon to bring the window back. The tray icon itself shows your RAM usage.

---

## Temperature Monitoring

### GPU Temperature
Your graphics card temperature appears **inside the GPU gauge** automatically:
- **NVIDIA cards** - Works out of the box
- **AMD cards** - Works out of the box
- **Intel and other graphics** - Usage and memory work; temperature isn't available

### CPU Temperature
For CPU temperature, you may need to install **HWiNFO** (free):

**If your CPU temperature shows 0°C or doesn't appear:**
1. Download **HWiNFO** from https://www.hwinfo.com/
2. Install and run HWiNFO
3. Click the **Settings** button (gear icon)
4. Check **"Shared Memory Support"**
5. Click **OK**
6. Keep HWiNFO running in the background
7. Restart Memory Monitor

> This is especially important for newer Intel CPUs (12th, 13th, 14th gen).

---

## FPS Monitoring Setup (Optional)

To see your gaming frame rate:

1. **Download and install HWiNFO** (if not already done)
   - Get it from https://www.hwinfo.com/

2. **Download and install RTSS**
   - Get RivaTuner Statistics Server from https://www.guru3d.com/files-details/rtss-rivatuner-statistics-server-download.html

3. **Configure HWiNFO**
   - Launch HWiNFO
   - Go to **Settings** → enable **"Shared Memory Support"**
   - Click **OK**

4. **Launch your game**
   - RTSS reports the game's FPS to HWiNFO
   - Memory Monitor shows it on your mini display

---

## Multiple GPUs, Disks, or Networks?

If you have multiple graphics cards, drives, or network adapters:

### Using Touch
- **Tap the gauge** repeatedly to cycle through options
- A notification shows which device is selected

### Using Mouse
- **Click the gauge** to open a menu
- Select the device you want to monitor

### Examples
- Multiple GPUs: Choose which graphics card to monitor
- Multiple Disks: Switch between "All Disks" or a specific drive
- WiFi + Ethernet: Choose "All Networks" or a specific adapter

Memory Monitor remembers your selection the next time it starts.

---

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **Escape** | Minimize to system tray |
| **F11** | Toggle "Always on Top" |

---

## Troubleshooting

### "Application won't start"
- Approve the administrator prompt when it appears - Memory Monitor needs it to read sensors
- Reinstall with the latest installer, which includes everything the app needs (no separate .NET install)

### "CPU temperature shows 0°C"
- Install and configure **HWiNFO** (see [Temperature Monitoring](#temperature-monitoring))
- Make sure HWiNFO is running before starting Memory Monitor
- Enable "Shared Memory Support" in HWiNFO settings

### "FPS gauge doesn't appear"
- Install both **HWiNFO** and **RTSS** (see [FPS Monitoring Setup](#fps-monitoring-setup-optional))
- Make sure HWiNFO has "Shared Memory Support" enabled
- Launch a game with RTSS running
- Check tray menu → **FPS Display** isn't set to "Always Hide"

### "Window is on the wrong monitor"
- Right-click **system tray icon** → **Move to Next Monitor**
- Or drag the window to your preferred display

### "Gauges look too small/large"
- The app sizes itself to fill the display it's on, and is designed for 1920×480
- Use **Move to Next Monitor** to put it on your mini display

### "Touch doesn't work"
- Connect the mini monitor's USB data cable, not just power
- Check that Windows lists a touchscreen under **Settings → Bluetooth & devices**

---

## Updating Memory Monitor

When a new version is released:
1. Download the new installer (`.msi`) from https://github.com/rickbme/Memory-Monitor/releases
2. Run the installer
3. It automatically upgrades your existing installation - no need to uninstall first
4. Your settings (display mode, FPS display, selected devices) are kept

> **Upgrading to 2.4.5 from an older version:** older versions installed to `C:\Program Files (x86)`; 2.4.5
> moves Memory Monitor to `C:\Program Files`. Because of the move, your display mode and device choices
> start fresh once. Set them again and they'll be kept from then on. If **Memory Monitor 1.0.0.0** is still
> listed in **Settings → Apps**, uninstall it there; it's left over from a very early version.

---

## What Each Gauge Means

### RAM (Blue Gauge)
- Shows how much memory your programs are using
- Example: "12.3/32GB" means 12.3 GB used out of 32 GB total
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
- Example: "8.2/24GB" means 8.2 GB used out of 24 GB total
- Increases with high-resolution textures and multiple monitors

### DISK (Green Gauge)
- Shows combined drive read and write speed
- Measured in **MB/s** (megabytes per second)
- The scale adjusts itself to your drive's speed
- Spikes when copying files or loading games

### NETWORK (Yellow Gauge)
- Shows combined upload and download speed
- Measured in **Mbps** (megabits per second, the unit internet plans use)
- The scale adjusts itself to your connection
- Spikes when streaming, downloading, or uploading

### FPS (Color-Coded Gauge)
- Shows your game's frame rate
- Appears while a game is reporting FPS (by default)
- Green = smooth, Yellow = good, Orange = acceptable, Red = choppy

---

## Tips & Tricks

### Best Placement
- Position your mini monitor **below your main display**
- Or to the **side** for easy glancing during games

### Multi-Monitor Setup
- Memory Monitor auto-detects 1920×480 (and other very wide) displays
- Manual switching: tray icon → **Move to Next Monitor**

### System Tray Only
- Press **Escape** to minimize Memory Monitor
- It continues monitoring in the background
- Double-click the tray icon to restore

### Touch-Friendly
- If your mini monitor has touch, use **gestures** instead of mouse clicks
- Much easier for small displays!

---

## Getting Help

### Need Support?
- Check the [Troubleshooting](#troubleshooting) section above
- Visit the GitHub page: https://github.com/rickbme/Memory-Monitor
- Report issues: https://github.com/rickbme/Memory-Monitor/issues

### Found a Bug?
Please report it with:
- Windows version (e.g., "Windows 11 24H2")
- Memory Monitor version (shown in **Settings → Apps**)
- What went wrong
- Steps to reproduce the issue

---

## Requirements

### Minimum System Requirements
- **Windows 10** or **Windows 11** (64-bit)
- **Administrator privileges** (for sensor access)
- No separate .NET install needed - the installer includes it

### Optional for Full Features
- **HWiNFO** - For CPU temperature (some CPUs) and FPS monitoring
- **RTSS** - For FPS display in games
- **Touchscreen mini monitor** - For touch gestures

---

## License & Credits

Memory Monitor is **free and open source** software.

**Created by:** DFS - Dad's Fixit Shop
**License:** MIT License
**GitHub:** https://github.com/rickbme/Memory-Monitor

### Special Thanks
- LibreHardwareMonitor team
- HWiNFO developers
- NVIDIA and AMD for GPU APIs

---

## Enjoy Monitoring!

Memory Monitor runs quietly in the background, giving you instant visibility into your PC's performance. Perfect for gamers, content creators, and power users who want to keep tabs on their system.

**Questions? Issues? Suggestions?**
Visit our GitHub page or open an issue. We're here to help!

---

**Version:** 2.4.5
**Made by Dad's Fixit Shop**
