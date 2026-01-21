# Memory Monitor v2.4.3 - Bug Fixes & Improvements

**Release Date:** January 2025

This release fixes several issues with display mode switching and improves the overall user experience.

---

## ?? Bug Fixes

### Display Mode Switching
- **Fixed**: Application no longer exits when switching between Circular Gauges and Bar Graph modes
- **Fixed**: Seamless transition between display modes without restart
- **Fixed**: Form position and TopMost state preserved when switching modes

### Taskbar Visibility
- **Fixed**: Application now correctly stays out of taskbar in both display modes
- **Fixed**: `TouchGestureHandler` no longer forces early window handle creation
- **Root Cause**: Touch registration was accessing `Form.Handle` before the form was shown, causing `ShowInTaskbar` to be ignored

### Welcome Dialog
- **Fixed**: Welcome dialog no longer appears when switching display modes
- **Fixed**: Dialog now only shows once on first-ever run (not every session)
- **Fixed**: Welcome dialog no longer appears in taskbar

### Intro Logo
- **Fixed**: Bar Graph mode now shows intro logo on startup (same as Circular Gauges)
- **Fixed**: Intro logos only show once per session, not when switching modes

---

## ? Improvements

### Tray Icon for Bar Graph Mode
- **New**: Bar Graph mode now has a distinct tray icon style
- **Visual**: Shows 4 vertical bars instead of circular arc gauge
- **Color-Coded**: Bars change color based on RAM usage (green ? yellow ? red)

### Display Mode Persistence
- **New**: Display mode preference is saved and restored between sessions
- **New**: Custom `MonitorApplicationContext` manages form lifecycle

---

## ?? Technical Changes

### Program.cs
- Added `MonitorApplicationContext` class for managing display mode switching
- Added `IntroLogosShown` static flag to prevent repeated logo display
- Added `WelcomeDialogShown` static flag to prevent repeated welcome dialog

### TouchGestureHandler.cs
- Deferred touch registration until `HandleCreated` event
- Prevents forcing early window handle creation
- Fixes `ShowInTaskbar = false` being ignored

### GaugeIconGenerator.cs
- Added `CreateBarGraphTrayIcon()` method for bar graph mode
- Creates vertical bar visualization for tray icon
- Uses color coding based on RAM percentage

### WelcomeDialog.cs
- Added `ShowInTaskbar = false` to prevent taskbar appearance
- Checks `Program.WelcomeDialogShown` before showing
- Always marks first run complete after showing

### BarGraphDisplayForm.cs
- Added intro logo support matching `MiniMonitorForm`
- Uses bar graph style tray icon
- Removed redundant `Show()` call from `OnLoad`

### MiniMonitorForm.cs
- Removed redundant `Show()` call from `OnLoad`
- Checks `Program.IntroLogosShown` before showing intro

### Designer Files
- Added `ShowInTaskbar = false` to both form designers

---

## ?? Files Changed

| File | Changes |
|------|---------|
| `Program.cs` | Added `MonitorApplicationContext`, session flags |
| `TouchGestureHandler.cs` | Deferred touch registration |
| `GaugeIconGenerator.cs` | Added bar graph icon method |
| `WelcomeDialog.cs` | Session flag check, taskbar fix |
| `BarGraphDisplayForm.cs` | Intro logo, bar graph tray icon |
| `MiniMonitorForm.cs` | Removed redundant Show() |
| `MiniMonitorForm.Layout.cs` | Intro logo session check |
| `MiniMonitorForm.Designer.cs` | ShowInTaskbar = false |
| `BarGraphDisplayForm.Designer.cs` | ShowInTaskbar = false |

---

## ?? Version Info

| Component | Version |
|-----------|---------|
| Application | 2.4.3 |
| Assembly | 2.4.3.0 |
| Installer | 2.4.3.0 |
| .NET Target | .NET 8.0 |

---

## Downloads

| File | Description |
|------|-------------|
| [MemoryMonitorSetup.msi](https://github.com/rickbme/Memory-Monitor/releases/download/v2.4.3/MemoryMonitorSetup.msi) | Windows Installer (x64) |
| [Source code (zip)](https://github.com/rickbme/Memory-Monitor/archive/refs/tags/v2.4.3.zip) | Source archive |

---

**Full Changelog:** [v2.4.2...v2.4.3](https://github.com/rickbme/Memory-Monitor/compare/v2.4.2...v2.4.3)

**Previous Release:** [v2.4.2](https://github.com/rickbme/Memory-Monitor/releases/tag/v2.4.2)
