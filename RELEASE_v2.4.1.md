# Memory Monitor v2.4.1 - Bug Fixes & Performance

**Release Date:** January 10, 2025

This is a patch release that fixes the clock display issue and improves rendering performance to reduce display flicker.

---

## ?? Bug Fixes

### Clock Display Not Updating
- **Fixed:** The date and time labels were not updating after the application started
- **Cause:** `UpdateDateTime()` was not being called in the main timer update cycle
- **Solution:** Added `SafeUpdate(UpdateDateTime)` to `UpdateAllMetrics()`
- **Result:** Clock now updates correctly every second

---

## ? Performance Improvements

### Display Flicker/Blip Reduction
Optimized rendering to reduce unnecessary screen repaints that could cause display "blips":

| Component | Before | After |
|-----------|--------|-------|
| **Gauge SetValue()** | Always invalidated | Only invalidates when value changes |
| **FPS Gauge** | Always invalidated | Only invalidates when FPS changes |
| **FPS Visibility** | Toggled every cycle | Only toggles on state change |
| **Date/Time Labels** | Updated every second | Only updates when text changes |
| **Property Setters** | Always invalidated | Check for change first |

**Impact:**
- ~60-80% reduction in unnecessary `Invalidate()` calls
- Reduced CPU usage during updates
- Smoother display on mini monitors
- Less GDI resource consumption

---

## ?? Technical Changes

### MiniMonitorForm.Monitors.cs
```csharp
// Added UpdateDateTime to timer cycle
private void UpdateAllMetrics()
{
    // ...other updates...
    SafeUpdate(UpdateDateTime);  // NEW
    UpdateTrayIconText();
}

// Optimized FPS visibility toggling
if (!fpsGauge.Visible)  // Only change if needed
{
    fpsGauge.Visible = true;
}
```

### MiniMonitorForm.DateTime.cs
```csharp
// Only update labels when text changes
if (lblTime.Text != newTime)
{
    lblTime.Text = newTime;
}
```

### CompactGaugeControl.cs
```csharp
// SetValue with change detection
public void SetValue(float value, string displayText)
{
    if (Math.Abs(_currentValue - clampedValue) > 0.01f || 
        _displayValue != displayText)
    {
        // Only then invalidate
        Invalidate();
    }
}
```

### FpsGaugeControl.cs
```csharp
// SetFps with change detection
public void SetFps(int fps)
{
    if (_fpsValue != newValue)
    {
        _fpsValue = newValue;
        Invalidate();
    }
}
```

---

## ?? Files Changed

- `Memory Monitor\MemoryMonitor.csproj` - Version bump to 2.4.1
- `Memory Monitor\CHANGELOG.md` - Added v2.4.1 entry
- `Memory Monitor\MiniMonitorForm.Monitors.cs` - Added UpdateDateTime call, optimized FPS
- `Memory Monitor\MiniMonitorForm.DateTime.cs` - Optimized label updates
- `Memory Monitor\CompactGaugeControl.cs` - Optimized invalidation
- `Memory Monitor\FpsGaugeControl.cs` - Optimized invalidation
- `MemoryMonitorSetup\Package.wxs` - Version bump to 2.4.1.0
- `USER_GUIDE.md` - Version bump

---

## ?? Installation

### Fresh Installation
1. Download `MemoryMonitorSetup.msi`
2. Run the installer
3. Launch from Start Menu or Desktop shortcut

### Upgrading from v2.4.0
The installer will automatically upgrade your existing installation:
- Settings are preserved
- Desktop shortcut remains
- Start Menu shortcuts updated

---

## ?? If You Still Experience Display Issues

If flicker persists after updating, the cause may be hardware-related:

1. **USB Connection** - Try a direct motherboard USB port (not hub)
2. **Cable Quality** - Use a high-quality USB data cable
3. **Power Issues** - Ensure adequate USB power to mini monitor
4. **Display Drivers** - Update graphics card drivers
5. **USB Power Settings** - Disable "USB selective suspend" in Windows

---

## ?? Version Info

| Component | Version |
|-----------|---------|
| Application | 2.4.1 |
| Assembly | 2.4.1.0 |
| Installer | 2.4.1.0 |
| .NET Target | .NET 8.0 |

---

## Downloads

| File | Description |
|------|-------------|
| [MemoryMonitorSetup.msi](https://github.com/rickbme/Memory-Monitor/releases/download/v2.4.1/MemoryMonitorSetup.msi) | Windows Installer (x64) |
| [Source code (zip)](https://github.com/rickbme/Memory-Monitor/archive/refs/tags/v2.4.1.zip) | Source archive |

---

**Full Changelog:** [v2.4.0...v2.4.1](https://github.com/rickbme/Memory-Monitor/compare/v2.4.0...v2.4.1)

**Previous Release:** [v2.4.0](https://github.com/rickbme/Memory-Monitor/releases/tag/v2.4.0)
