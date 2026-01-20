# WiX Installer UI Improvements

## Changes Made (v2.4.1)

The installer now includes a full Windows Installer UI experience with standard dialogs.

### New Features

1. **Welcome Dialog**
   - Displays product name and version
   - Provides overview of what will be installed

2. **Installation Directory Selection**
   - Users can choose custom install location
   - Default: `C:\Program Files\Memory Monitor`

3. **Ready to Install Confirmation**
   - Shows summary before installation begins
   - User can go back and change settings

4. **Progress Bar**
   - Visual progress during file installation
   - Shows current operation being performed

5. **Completion Dialog**
   - Confirms successful installation
   - Shows custom message with next steps:
     - How to launch the application
     - Note about Administrator privileges requirement

6. **Maintenance Mode (when already installed)**
   - **Repair** - Reinstall all files to fix issues
   - **Remove** - Uninstall the application completely
   - Automatically detected when running installer with product already installed

### Technical Changes

#### MemoryMonitorSetup.wixproj
- Added `WixToolset.UI.wixext` package for standard dialogs
- Added `WixToolset.Util.wixext` package for utility functions

#### Package.wxs
- Added `ui:WixUI Id="WixUI_InstallDir"` for full dialog set
- Added `xmlns:ui` namespace for UI extension
- Added `xmlns:util` namespace for utility functions
- Set `WIXUI_INSTALLDIR` property for directory selection
- Added `WIXUI_EXITDIALOGOPTIONALTEXT` for completion message
- Removed `ARPNOREPAIR` and `ARPNOMODIFY` to enable maintenance mode

### Dialog Sequence

**Fresh Install:**
```
Welcome ? Install Directory ? Ready to Install ? Progress ? Complete
```

**Already Installed (Maintenance Mode):**
```
Welcome ? Repair/Remove Selection ? Progress ? Complete
```

### Completion Message

After installation, users see:
```
Memory Monitor v2.4.1 has been successfully installed.

You can launch it from the Start Menu or Desktop shortcut.

Note: The application requires Administrator privileges to access hardware sensors.
```

### Add/Remove Programs

Users can now:
- **Repair** - Fix corrupted installation
- **Uninstall** - Remove completely
- **Modify** - Change installed features (if applicable)

## Build Instructions

```powershell
# 1. Publish the application
dotnet publish "..\Memory Monitor\Memory Monitor.csproj" -c Release -r win-x64 --self-contained true

# 2. Regenerate file list
.\Regenerate-PublishedFiles.ps1

# 3. Build installer
dotnet build MemoryMonitorSetup.wixproj -c Release
```

Output: `bin\Release\MemoryMonitorSetup.msi`
