# System Tray Functionality

## Overview

The application now supports minimizing to the system tray with real-time CPU and GPU usage displayed in the tray icon tooltip.

## Features

### 1. **Minimize to Tray**
- When the window is minimized, it hides from the taskbar
- Application remains running in the system tray
- Icon appears in the notification area (system tray)

### 2. **Tray Icon Tooltip**
The tray icon displays abbreviated usage statistics:
```
CPU: 45 GPU: 32
```

**Format:**
- **CPU**: 0-100 (percentage)
- **GPU**: 0-100 (percentage)
- Updates every second with the main timer

### 3. **Restore Window**
Multiple ways to restore the window:
- **Double-click** the tray icon
- **Right-click**  Select "Show"
- Window restores to normal state
- Re-appears in taskbar

### 4. **Tray Context Menu**
Right-click the tray icon for options:
```
????????????????
? Show         ?
????????????????
? Exit         ?
????????????????
```

## Implementation Details

### Tray Icon Components

#### NotifyIcon Control:
- **Icon**: Uses form icon or SystemIcons.Application as fallback
- **Text**: Updates every second with CPU/GPU values
- **Visible**: Always visible when application runs
- **Context Menu**: Right-click menu for Show/Exit

#### Context Menu Items:
1. **Show**: Restores the main window
2. **Exit**: Closes the application completely

### Update Flow

```
Timer Tick (1 second)
    ?
UpdateCPUUsage()
    - Reads CPU percentage
    - Stores in _lastCpuUsage
    ?
UpdateGPUUsage()
    - Reads GPU percentage
    - Stores in _lastGpuUsage
    - Calls UpdateTrayIconText()
        ?
        Format: "CPU: {cpu} GPU: {gpu}"
        ?
        notifyIcon.Text = formatted string
```

### Code Structure

#### Private Fields:
```csharp
private int _lastCpuUsage = 0;  // CPU percentage (0-100)
private int _lastGpuUsage = 0;  // GPU percentage (0-100)
```

#### Key Methods:

**InitializeTrayIcon():**
```csharp
- Sets up the tray icon
- Configures icon from form or system
- Initializes tooltip text
- Attaches resize event handler
```

**UpdateTrayIconText():**
```csharp
- Formats CPU/GPU values
- Updates notifyIcon.Text
- Truncates to 63 characters (Windows limit)
```

**Form1_ResizeMinimizeToTray():**
```csharp
- Detects window minimization
- Hides form from taskbar
- Leaves tray icon visible
```

**ShowForm():**
```csharp
- Shows the form
- Restores to normal state
- Brings to taskbar
- Activates window
```

## User Experience

### Minimizing:
```
1. User clicks minimize button
2. Window minimizes as usual
3. Event handler detects minimized state
4. Form hides from taskbar
5. Icon remains in system tray
6. Tooltip shows: "CPU: 45 GPU: 32"
```

### Monitoring While Minimized:
```
Time  | CPU | GPU | Tray Display
------|-----|-----|-------------
0s    | 25  | 15  | CPU: 25 GPU: 15
1s    | 28  | 18  | CPU: 28 GPU: 18
2s    | 45  | 32  | CPU: 45 GPU: 32
3s    | 52  | 40  | CPU: 52 GPU: 40
```

### Restoring:
```
1. User double-clicks tray icon (or right-click ? Show)
2. ShowForm() method called
3. Window restored to normal state
4. Window shown in taskbar
5. Window activated (brought to front)
```

### Exiting:
```
Option 1: Right-click tray icon ? Exit
Option 2: Restore window ? Close normally
Option 3: Task Manager ? End process

All options properly cleanup resources
```

## Display Format

### Abbreviated Numbers (0-100):
```
CPU: 0     GPU: 0      (Idle)
CPU: 15    GPU: 8      (Light load)
CPU: 45    GPU: 32     (Moderate load)
CPU: 78    GPU: 65     (Heavy load)
CPU: 100   GPU: 100    (Maximum load)
```

### Tooltip Length Limit:
- Windows tooltips limited to 63 characters
- Format: "CPU: XX GPU: YY" = 15 characters max
- Well within limit, even with full text

### Special Cases:
```
CPU: N/A   GPU: N/A    (Monitor unavailable)
CPU: 0     GPU: 0      (Error state)
```

## Benefits

### For Users:
1. **Quick Monitoring**: Glance at system tray for usage
2. **Clean Taskbar**: Window hidden when not needed
3. **Always Running**: Continuous monitoring in background
4. **Easy Access**: Double-click to restore anytime

### For System:
1. **Low Overhead**: Only tooltip text updates
2. **No Drawing**: No UI rendering when minimized
3. **Efficient**: Single timer for all updates
4. **Clean**: Proper disposal of resources

## Technical Notes

### Icon Selection Priority:
```csharp
1. Form.Icon (if available)
2. SystemIcons.Application (fallback)
```

### Event Handling:
```csharp
notifyIcon.DoubleClick ? ShowForm()
showToolStripMenuItem.Click ? ShowForm()
exitToolStripMenuItem.Click ? Application.Exit()
this.Resize ? Check if minimized ? Hide()
```

### Tooltip Update Frequency:
- Updates every 1 second (same as main timer)
- Only updates when GPU values are retrieved
- Minimal performance impact

## Customization

### Changing Update Interval:
The tooltip updates with the main timer (1000ms):
```csharp
this.updateTimer.Interval = 1000;  // Change to desired milliseconds
```

### Adding More Metrics:
To add disk or network to tray tooltip:
```csharp
private void UpdateTrayIconText()
{
    string trayText = $"C:{_lastCpuUsage} G:{_lastGpuUsage} D:{_lastDisk} N:{_lastNetwork}";
    if (trayText.Length > 63)
    {
        trayText = trayText.Substring(0, 63);
    }
    notifyIcon.Text = trayText;
}
```

### Custom Formatting:
```csharp
// More compact:
trayText = $"C:{_lastCpuUsage}|G:{_lastGpuUsage}";

// With symbols:
trayText = $"?{_lastCpuUsage}% ??{_lastGpuUsage}%";

// Percentage signs:
trayText = $"CPU:{_lastCpuUsage}% GPU:{_lastGpuUsage}%";
```

## Testing

### Verify Functionality:
1. **Minimize Test**:
   - Click minimize button
   - ? Window disappears from taskbar
   - ? Icon appears in system tray

2. **Tooltip Test**:
   - Hover over tray icon
   - ? See "CPU: XX GPU: YY"
   - Wait 1 second
   - ? Values update

3. **Restore Test**:
   - Double-click tray icon
   - ? Window restores
   - ? Window in taskbar
   - ? Window has focus

4. **Context Menu Test**:
   - Right-click tray icon
   - ? Menu appears
   - Click "Show"
   - ? Window restores
   - Right-click again
   - Click "Exit"
   - ? Application closes

### Performance Testing:
1. Minimize window
2. Monitor task manager
3. ? CPU usage should be minimal
4. ? Memory usage stable
5. ? No UI rendering overhead

## Troubleshooting

### Icon Not Appearing:
- Check system tray settings in Windows
- Verify notifyIcon.Visible = true
- Check if icon resource exists

### Tooltip Not Updating:
- Verify updateTimer is running
- Check UpdateTrayIconText() is being called
- Debug _lastCpuUsage and _lastGpuUsage values

### Can't Restore Window:
- Ensure ShowForm() method is connected
- Check DoubleClick event handler
- Verify context menu items are wired up

### Window Reappears in Taskbar:
- Check if ShowInTaskbar is being set correctly
- Verify Resize event handler is attached
- Ensure Hide() is being called on minimize

## Future Enhancements

### Possible Additions:
1. **Dynamic Icons**: Different icons based on load levels
2. **Notifications**: Alert on high usage
3. **History Graph**: Right-click to see mini-graph
4. **Multiple Tooltips**: Rotate between different metrics
5. **Click Actions**: Single-click for different behavior
6. **Balloon Tips**: Show warnings for critical levels

### Icon Customization:
```csharp
// Load custom icons based on CPU usage
if (_lastCpuUsage > 80)
    notifyIcon.Icon = highLoadIcon;
else if (_lastCpuUsage > 50)
    notifyIcon.Icon = mediumLoadIcon;
else
    notifyIcon.Icon = normalIcon;
```

## Summary

The system tray functionality provides:
- ? **Quick monitoring**: CPU/GPU at a glance
- ? **Clean interface**: Hidden when not needed
- ? **Easy access**: Double-click to restore
- ? **Low overhead**: Minimal resource usage
- ? **User-friendly**: Intuitive behavior

Perfect for users who want to monitor system performance without keeping the window visible all the time!
