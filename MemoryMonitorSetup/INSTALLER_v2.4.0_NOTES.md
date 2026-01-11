# WiX Installer v2.4.0 - Enhanced Finish Dialog

## What's New in the Installer

The Memory Monitor installer now includes an enhanced finish dialog with user-friendly options after installation completes.

### Finish Dialog Features

#### 1. Desktop Shortcut Option
- **Checkbox:** "Create a desktop shortcut"
- **Default:** Checked (enabled by default)
- **Behavior:** 
  - If checked, creates a Memory Monitor icon on the desktop
  - If unchecked, only creates Start Menu shortcut
  - Can be manually created later from Start Menu

#### 2. View User Guide Option
- **Checkbox:** "View User Guide"
- **Default:** Checked (enabled by default)
- **Behavior:**
  - If checked, opens `USER_GUIDE.md` in default markdown viewer (or text editor)
  - If unchecked, user can access guide later from Start Menu
  - User guide is installed to: `C:\Program Files\Memory Monitor\USER_GUIDE.md`

### User Guide

The installer now includes a comprehensive, user-friendly guide specifically written for end users (not developers):

**Location:** `C:\Program Files\Memory Monitor\USER_GUIDE.md`

**Contents:**
- ?? Quick Start guide
- ?? Gaming features (FPS monitoring)
- ?? Touchscreen gesture support
- ??? Temperature monitoring setup
- ??? Troubleshooting common issues
- ?? Tips and tricks
- ?? System tray controls

**Access Methods:**
- Finish dialog checkbox (opens automatically if checked)
- Start Menu ? "Memory Monitor User Guide" shortcut
- Directly open: `C:\Program Files\Memory Monitor\USER_GUIDE.md`

### Start Menu Structure

The installer creates the following Start Menu items:

```
Start Menu > Memory Monitor >
  ?? Memory Monitor (launches application)
  ?? Memory Monitor User Guide (opens USER_GUIDE.md)
```

### Desktop Shortcut

If enabled on finish dialog:
- **Icon:** Memory Monitor gauge icon
- **Name:** "Memory Monitor"
- **Target:** `C:\Program Files\Memory Monitor\Memory Monitor.exe`
- **Behavior:** Launches with Administrator privileges

### Technical Implementation

#### WiX Features Used:
1. **WixUI_InstallDir** - Standard installation dialog set
2. **Custom Properties:**
   - `WIXUI_EXITDIALOGOPTIONALCHECKBOXTEXT` - Desktop shortcut text
   - `WIXUI_EXITDIALOGOPTIONALCHECKBOX` - Desktop shortcut checkbox state
   - `LAUNCH_README` - View User Guide checkbox state
3. **CustomAction:**
   - `LaunchReadme` - Opens USER_GUIDE.md in default application
4. **Conditional Feature:**
   - `DesktopShortcut` feature only installs if checkbox is checked

#### File Structure:
```
INSTALLFOLDER (C:\Program Files\Memory Monitor\)
?? Memory Monitor.exe
?? USER_GUIDE.md          ? New user-friendly guide
?? (all other application files)
?? (language resource folders)
```

### Building the Installer

#### Prerequisites:
1. USER_GUIDE.md must exist in repository root
2. License.rtf must exist in MemoryMonitorSetup folder
3. Application must be published to expected location

#### Build Command:
```powershell
# From MemoryMonitorSetup directory
dotnet build MemoryMonitorSetup.wixproj --configuration Release
```

#### Or use the automated script:
```powershell
.\Build-Installer.ps1
```

### Testing the Installer

1. **Install with defaults:**
   - Both checkboxes should be checked
   - Desktop shortcut created
   - USER_GUIDE.md opens automatically

2. **Install without desktop shortcut:**
   - Uncheck "Create a desktop shortcut"
   - Only Start Menu shortcut created
   - USER_GUIDE.md still opens (if checked)

3. **Install without viewing guide:**
   - Uncheck "View User Guide"
   - Desktop shortcut created (if checked)
   - USER_GUIDE.md does not auto-open

4. **Verify Start Menu:**
   - Check Start Menu > Memory Monitor folder
   - Should have both app and guide shortcuts

### Upgrade Behavior

When upgrading from previous versions:
- Existing desktop shortcut is preserved
- Start Menu folder is updated with new guide shortcut
- USER_GUIDE.md is added/updated
- User preferences are maintained

### Uninstallation

The uninstaller removes:
- All installed files including USER_GUIDE.md
- Desktop shortcut (if created)
- Start Menu folder and shortcuts
- Registry keys

### Known Limitations

1. **Markdown Rendering:**
   - USER_GUIDE.md opens in default `.md` file handler
   - May open in text editor if no markdown viewer installed
   - Consider HTML version for future releases

2. **Administrator Prompt:**
   - Desktop shortcut doesn't elevate automatically
   - User must right-click ? "Run as administrator"
   - Could be improved with manifest in future

### Future Enhancements

Potential improvements for future versions:
- [ ] Convert USER_GUIDE.md to HTML for better rendering
- [ ] Add "Launch Memory Monitor" checkbox on finish dialog
- [ ] Include screenshots in installer dialog
- [ ] Create elevated desktop shortcut automatically
- [ ] Add uninstaller survey or feedback link

---

**Version:** 2.4.0  
**Last Updated:** January 10, 2025  
**WiX Toolset:** v5.0.2
