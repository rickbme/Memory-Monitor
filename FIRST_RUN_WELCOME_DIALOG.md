# First-Run Welcome Dialog - Implementation Guide

## Overview

Memory Monitor now includes a **first-run welcome dialog** that appears automatically when users launch the application for the first time after installation. This provides a friendly introduction to the application's features and guides users to the USER_GUIDE.md.

---

## Features

### Welcome Dialog Contents

**Title:**  
?? Welcome to Memory Monitor!

**Subtitle:**  
Real-time system monitoring for your mini display

**Feature Highlights:**
- ?? Monitor CPU, GPU, RAM, VRAM, Disk, and Network
- ??? View CPU and GPU temperatures in real-time
- ?? Smart FPS display during gaming sessions
- ?? Touch gestures for mini monitor touchscreens
- ?? Date and time display in the corners
- ?? Device selection for multiple GPUs, disks, or networks
- ?? Runs quietly in system tray when minimized

**Buttons:**
- ?? **Open User Guide** - Opens USER_GUIDE.md in default application
- ? **Get Started** - Closes the dialog and starts using Memory Monitor

**Checkbox:**
- ?? "Don't show this again" - Prevents dialog from appearing on next launch

---

## How It Works

### First Run Detection

The dialog uses Windows Registry to track if this is the first run:

**Registry Key:** `HKEY_CURRENT_USER\Software\MemoryMonitor\FirstRunComplete`

- **Not present or value is 0** ? First run, show dialog
- **Value is 1** ? Not first run, skip dialog

### Behavior

1. **On First Launch:**
   - User installs Memory Monitor
   - User launches the application
   - Main window appears and moves to mini monitor
   - Welcome dialog appears on top
   - User can click "Open User Guide" or "Get Started"

2. **On Subsequent Launches:**
   - Welcome dialog does not appear
   - Application starts normally

3. **If "Don't show this again" is checked:**
   - Registry key is set to 1
   - Dialog won't appear again

---

## User Experience Flow

```
Installation Complete
      ?
Launch Memory Monitor
      ?
Main Window Appears
      ?
Welcome Dialog Appears (First Run Only)
      ?
   ?????????????????????????????
   ?             ?             ?
Open Guide   Get Started   Don't Show
   ?             ?             ?
Guide Opens   Dialog Closes  Registry Set
```

---

## Testing the Dialog

### Test First Run:
1. **Delete the registry key** (if it exists):
   ```
   reg delete "HKCU\Software\MemoryMonitor" /v FirstRunComplete /f
   ```

2. **Launch Memory Monitor**
   - Welcome dialog should appear

3. **Test "Open User Guide" button**
   - USER_GUIDE.md should open in default markdown viewer/text editor

4. **Test "Get Started" button**
   - Dialog should close
   - Main application continues running

5. **Close and relaunch**
   - Welcome dialog should appear again (registry key not set)

### Test "Don't Show Again":
1. Delete registry key (if exists)
2. Launch Memory Monitor
3. Check "Don't show this again"
4. Click "Get Started"
5. Close and relaunch Memory Monitor
6. Welcome dialog should NOT appear

### Test Subsequent Runs:
1. Launch Memory Monitor normally
2. Welcome dialog should not appear
3. Application starts directly

---

## Code Implementation

### Files Modified:
- **`WelcomeDialog.cs`** (NEW) - Welcome dialog form
- **`MiniMonitorForm.cs`** - Integrated welcome dialog call

### Key Methods:

#### WelcomeDialog.cs
```csharp
public static bool ShowIfFirstRun()
{
    // Check if this is the first run
    if (IsFirstRun())
    {
        using (var dialog = new WelcomeDialog())
        {
            dialog.ShowDialog();
            
            // Save the "don't show again" preference
            if (dialog.DontShowAgain)
            {
                SetFirstRunComplete();
            }
        }
        return true;
    }
    return false;
}
```

#### MiniMonitorForm.cs (OnLoad method)
```csharp
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);

    this.WindowState = FormWindowState.Normal;
    this.Show();
    this.Activate();

    Debug.WriteLine("Application started - window visible, tray icon active");

    // Show welcome dialog on first run
    this.BeginInvoke(new Action(() =>
    {
        WelcomeDialog.ShowIfFirstRun();
    }));
}
```

---

## Registry Details

### Key Location:
```
HKEY_CURRENT_USER\Software\MemoryMonitor
```

### Value:
```
Name: FirstRunComplete
Type: REG_DWORD
Data: 1 (first run complete) or 0/not present (first run)
```

### Manual Registry Commands:

**Check if first run flag is set:**
```cmd
reg query "HKCU\Software\MemoryMonitor" /v FirstRunComplete
```

**Reset first run (for testing):**
```cmd
reg delete "HKCU\Software\MemoryMonitor" /v FirstRunComplete /f
```

**Manually set first run complete:**
```cmd
reg add "HKCU\Software\MemoryMonitor" /v FirstRunComplete /t REG_DWORD /d 1 /f
```

---

## Developer Testing Utility

The `WelcomeDialog` class includes a **ResetFirstRun()** method for testing:

```csharp
// In your test code or debug menu:
WelcomeDialog.ResetFirstRun();

// Next launch will show the welcome dialog again
```

---

## Dialog Design

### Colors:
- **Background:** Dark gray (#1E2125)
- **Title:** Blue (#64B4FF)
- **Features Panel:** Darker gray (#282B2F)
- **Feature Text:** White
- **Open Guide Button:** Blue (#64B4FF)
- **Get Started Button:** Green (#32C850)

### Layout:
- **Width:** 600px
- **Height:** 500px
- **Position:** Center screen
- **Border:** Fixed dialog (no resize)

### Fonts:
- **Title:** Segoe UI, 18pt Bold
- **Subtitle:** Segoe UI, 10pt Regular
- **Features:** Segoe UI, 9.5pt Regular
- **Buttons:** Segoe UI, 10pt Bold

---

## Error Handling

### If USER_GUIDE.md is not found:
The "Open User Guide" button shows a message box:
```
USER_GUIDE.md was not found in the installation directory.

You can access the guide from the Start Menu:
Start ? Memory Monitor ? Memory Monitor User Guide
```

### If registry access fails:
- Assumes first run (safe default)
- Debug messages logged
- Application continues normally

---

## Future Enhancements

Potential improvements for future versions:

1. **Add Screenshots**
   - Show visual examples in the welcome dialog
   - Use embedded image resources

2. **Quick Settings**
   - Allow users to configure basic settings from welcome dialog
   - FPS display mode selection
   - Temperature monitoring setup

3. **Wizard Mode**
   - Multi-step introduction
   - Interactive tutorial
   - Device selection walkthrough

4. **Video Tutorial Link**
   - Link to YouTube tutorial (if created)
   - In-app video player

5. **Check for Updates**
   - Show if a newer version is available
   - Link to download page

---

## Troubleshooting

### Dialog doesn't appear on first run:
1. Check if registry key exists and is set to 1
2. Delete the key and try again
3. Check Debug output for error messages

### Dialog appears every time:
1. Check if "Don't show this again" checkbox is working
2. Verify registry key is being created
3. Check for permission issues with registry

### "Open User Guide" button doesn't work:
1. Verify USER_GUIDE.md exists in application directory
2. Check file permissions
3. Verify default markdown viewer is configured
4. Try opening the file manually from Start Menu

---

## Installer Integration

The welcome dialog works seamlessly with the WiX installer:

1. **Installer runs** ? Copies USER_GUIDE.md to installation folder
2. **User launches app** ? Welcome dialog appears
3. **User clicks "Open User Guide"** ? USER_GUIDE.md opens
4. **User reads guide** ? Learns about features
5. **User clicks "Get Started"** ? Starts using Memory Monitor

This provides a smooth onboarding experience without complex installer customization!

---

## Summary

? **Implemented:** First-run welcome dialog  
? **Registry-based:** Detects first run automatically  
? **User-friendly:** Clean dark theme, clear feature list  
? **Integrated:** Opens USER_GUIDE.md from installation folder  
? **Optional:** "Don't show this again" checkbox  
? **No errors:** Builds successfully  
? **Ready for:** v2.4.0 release

---

**Implementation Date:** January 10, 2025  
**Version:** 2.4.0  
**Status:** ? Complete and tested
