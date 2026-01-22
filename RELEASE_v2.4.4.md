# Memory Monitor v2.4.4 - Display Mode Improvements

**Release Date:** January 2025

This release adds FPS display options to Bar Graph mode and introduces smooth fade transitions when switching display modes.

---

## ? New Features

### FPS Display Mode in Bar Graph
- **New**: FPS Display menu now available in Bar Graph mode
- **Menu Location**: Right-click tray icon ? **FPS Display** submenu
- **Options**:
  - **Auto-detect** (default) - Shows FPS only when gaming activity detected
  - **Always Show** - Force FPS display when available
  - **Always Hide** - Never show FPS
- FPS appears in the GPU panel when gaming is detected
- Matches functionality already available in Circular Gauges mode

### Smooth Display Mode Transitions
- **New**: Fade animation when switching between Circular Gauges and Bar Graph
- **Animation**: Fade out (150ms) ? Fade in (150ms)
- **Total Time**: ~300ms for complete transition
- **Result**: Professional, polished appearance that's easy on the eyes
- No more jarring instant switches between display modes

---

## ?? Display Modes

Memory Monitor now offers two fully-featured display modes:

### Circular Gauges Mode
- Classic needle gauge display
- 6 circular gauges in a horizontal row
- FPS gauge between GPU and VRAM gauges
- Date/time display in corners

### Bar Graph Mode
- Modern animated bar graph panels
- 5 panels: CPU, GPU, VRAM, Drive, Network
- Rolling history animation (12 data points)
- FPS shown in GPU panel

**Both modes now support:**
- ? FPS Display settings (Auto-detect, Always Show, Always Hide)
- ? Move to Next Monitor
- ? Always on Top toggle
- ? System tray integration
- ? Touch gestures (if available)

---

## ?? Files Changed

| File | Changes |
|------|---------|
| `Program.cs` | Added fade transition methods |
| `BarGraphDisplayForm.cs` | Added FPS display mode handlers |
| `BarGraphDisplayForm.Designer.cs` | Added FPS Display menu items |
| `MemoryMonitor.csproj` | Version bump to 2.4.4 |
| `Package.wxs` | Installer version bump to 2.4.4.0 |

---

## ?? Technical Details

### Fade Transition Implementation
```csharp
// Animation constants
FADE_DURATION_MS = 150   // Duration for each fade
FADE_INTERVAL_MS = 15    // Timer tick interval
OPACITY_STEP = 0.1       // Opacity change per tick

// Sequence:
1. FadeOutAndSwitch() - Decrease old form opacity to 0
2. Create new form with opacity 0
3. FadeIn() - Increase new form opacity to 1
```

### FPS Menu Integration
- Added `fpsDisplayModeToolStripMenuItem` with 3 sub-items
- Event handlers call `SetFpsDisplayMode()` method
- Updates `GameActivityDetector.DisplayMode` property
- Menu checkmarks update to reflect current selection

---

## ?? Version Info

| Component | Version |
|-----------|---------|
| Application | 2.4.4 |
| Assembly | 2.4.4.0 |
| Installer | 2.4.4.0 |
| .NET Target | .NET 8.0 |

---

## ?? Downloads

| File | Description |
|------|-------------|
| [MemoryMonitorSetup.msi](https://github.com/rickbme/Memory-Monitor/releases/download/v2.4.4/MemoryMonitorSetup.msi) | Windows Installer (x64) |
| [Source code (zip)](https://github.com/rickbme/Memory-Monitor/archive/refs/tags/v2.4.4.zip) | Source archive |

---

## ?? Upgrade Notes

- Seamless upgrade from v2.4.3
- All settings preserved
- Display mode preference maintained
- No action required after installation

---

**Full Changelog:** [v2.4.3...v2.4.4](https://github.com/rickbme/Memory-Monitor/compare/v2.4.3...v2.4.4)

**Previous Release:** [v2.4.3](https://github.com/rickbme/Memory-Monitor/releases/tag/v2.4.3)
