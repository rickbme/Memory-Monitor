# Memory Monitor v2.4.2 - Bar Graph Display Mode

**Release Date:** January 2025

This release introduces an alternate **Bar Graph Display Mode** that provides a modern, futuristic dashboard view of your system metrics.

---

## ? New Features

### Alternate Bar Graph Display Mode
A completely new display option featuring animated bar graph panels instead of circular gauges:

| Panel | Description |
|-------|-------------|
| **CPU Usage** | Animated bar history with percentage and temperature |
| **GPU Usage** | Bar history with percentage, temperature, and FPS overlay |
| **Drive Usage** | Disk activity bar with capacity info |
| **Network** | Dual horizontal bars for upload/download speeds |

#### Design Features
- **Animated Bar History** - 12-point rolling history visualization
- **Futuristic Styling** - Glowing accent lines, gradient effects, corner accents
- **Color-Coded Panels** - Cyan (CPU), Green (GPU), Orange (Drive), Cyan (Network)
- **Auto-Scaling** - Network and disk speeds auto-scale based on peak values

### Display Mode Switching
- Right-click **system tray icon** ? **Display Mode**
  - **Circular Gauges** - Classic needle gauge display (default)
  - **Bar Graph** - New animated bar graph display
- Preference is saved and restored between sessions
- Switching is instant with no restart required

---

## ?? New Files

| File | Description |
|------|-------------|
| `BarGraphPanelControl.cs` | Custom control for CPU/GPU/Drive bar panels |
| `NetworkBarPanelControl.cs` | Specialized panel with dual upload/download bars |
| `BarGraphDisplayForm.cs` | Main form for bar graph display mode |
| `BarGraphDisplayForm.Designer.cs` | Designer file for bar graph form |
| `DisplayModeManager.cs` | Static manager for display mode state and persistence |

---

## ?? Technical Changes

### New Components
- **BarGraphPanelControl** - Renders animated bar history with:
  - 12-point value queue for history animation
  - Gradient-filled bars with glow effects
  - Title, primary value (percentage), secondary info (temp/speed)
  - Futuristic corner accents and accent lines

- **NetworkBarPanelControl** - Specialized network panel with:
  - Dual horizontal progress bars (upload/download)
  - Speed labels positioned below each bar
  - Segmented bar design for tech aesthetic
  - Auto-scaling based on peak speeds

- **DisplayModeManager** - Manages display mode state:
  - `DisplayMode` enum (CircularGauges, BarGraph)
  - Persists preference to application settings
  - `DisplayModeChanged` event for form switching
  - `SetDisplayMode()` and `ToggleDisplayMode()` methods

### Modified Files
- `MiniMonitorForm.Designer.cs` - Added Display Mode submenu
- `MiniMonitorForm.cs` - Added display mode menu handlers
- `Program.cs` - Handles display mode switching at startup and runtime
- `Properties\Settings.Designer.cs` - Added `DisplayMode` setting

---

## ??? Screenshots

The new bar graph display shows:
```
??????????????? ??????????????? ??????????????? ???????????????
? CPU USAGE   ? ? GPU USAGE   ? ? DRIVE USAGE ? ? NETWORK     ?
? ??????????? ? ? ??????????? ? ? ?????????? ? ? UPLOAD      ?
?             ? ?     FPS: 60 ? ?             ? ? ???????     ?
?    72%      ? ?    48%      ? ?    55%      ? ?    15.3 MB/s?
? TEMP: 58°C  ? ? TEMP: 64°C  ? ? 342GB/1TB   ? ? DOWNLOAD    ?
??????????????? ??????????????? ??????????????? ? ?????????????
                                                ?    92.6 MB/s?
                                                ???????????????
```

---

## ?? Installation

### Fresh Installation
1. Download `MemoryMonitorSetup.msi`
2. Run the installer
3. Launch from Start Menu or Desktop shortcut

### Upgrading from v2.4.1
The installer will automatically upgrade your existing installation:
- Settings are preserved
- Display mode preference defaults to Circular Gauges
- Switch to Bar Graph mode via tray menu ? Display Mode

---

## ?? How to Use

1. **Access Display Mode Menu:**
   - Right-click the Memory Monitor tray icon
   - Select **Display Mode**

2. **Choose Your Preferred Mode:**
   - **Circular Gauges** - Traditional needle gauge display
   - **Bar Graph** - Modern animated bar graph panels

3. **Your Choice is Remembered:**
   - The application saves your preference
   - Next launch uses your selected mode

---

## ?? Version Info

| Component | Version |
|-----------|---------|
| Application | 2.4.2 |
| Assembly | 2.4.2.0 |
| Installer | 2.4.2.0 |
| .NET Target | .NET 8.0 |

---

## Downloads

| File | Description |
|------|-------------|
| [MemoryMonitorSetup.msi](https://github.com/rickbme/Memory-Monitor/releases/download/v2.4.2/MemoryMonitorSetup.msi) | Windows Installer (x64) |
| [Source code (zip)](https://github.com/rickbme/Memory-Monitor/archive/refs/tags/v2.4.2.zip) | Source archive |

---

**Full Changelog:** [v2.4.1...v2.4.2](https://github.com/rickbme/Memory-Monitor/compare/v2.4.1...v2.4.2)

**Previous Release:** [v2.4.1](https://github.com/rickbme/Memory-Monitor/releases/tag/v2.4.1)
