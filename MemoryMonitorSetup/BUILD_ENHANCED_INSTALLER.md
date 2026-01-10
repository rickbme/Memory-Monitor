# Building the Enhanced Installer

## Quick Start

### 1. Copy USER_GUIDE.md to Root
The installer expects `USER_GUIDE.md` in the repository root:
```powershell
# If USER_GUIDE.md is not in root, copy it:
Copy-Item ".\USER_GUIDE.md" ".." -Force
```

### 2. Build the Installer
From the repository root or MemoryMonitorSetup directory:
```powershell
cd MemoryMonitorSetup
.\Build-Installer.ps1
```

Or manually:
```powershell
# 1. Publish the application
dotnet publish "..\Memory Monitor\Memory Monitor.csproj" -c Release -r win-x64 --self-contained true

# 2. Generate file list
powershell -ExecutionPolicy Bypass -File .\Regenerate-PublishedFiles.ps1

# 3. Build installer
dotnet build MemoryMonitorSetup.wixproj --configuration Release
```

## What the Installer Does

### During Installation:
1. Shows standard WiX installation dialogs
2. Allows user to choose installation directory
3. Installs all application files to `C:\Program Files\Memory Monitor\`
4. Creates Start Menu folder with 2 shortcuts

### On Finish Dialog:
User sees two checkboxes:
- ? **"Create a desktop shortcut"** (checked by default)
- ? **"View User Guide"** (checked by default)

### After Installation:
- If "Create desktop shortcut" was checked ? Desktop shortcut appears
- If "View User Guide" was checked ? USER_GUIDE.md opens automatically
- Start Menu folder contains:
  - Memory Monitor (launches application)
  - Memory Monitor User Guide (opens USER_GUIDE.md)

## Testing Checklist

### Test 1: Default Installation
- [ ] Run installer
- [ ] Leave both checkboxes checked
- [ ] Click Finish
- [ ] Verify desktop shortcut created
- [ ] Verify USER_GUIDE.md opens
- [ ] Verify Start Menu shortcuts work

### Test 2: No Desktop Shortcut
- [ ] Run installer
- [ ] Uncheck "Create desktop shortcut"
- [ ] Keep "View User Guide" checked
- [ ] Click Finish
- [ ] Verify NO desktop shortcut
- [ ] Verify USER_GUIDE.md opens
- [ ] Verify Start Menu shortcuts work

### Test 3: No User Guide Launch
- [ ] Run installer
- [ ] Keep "Create desktop shortcut" checked
- [ ] Uncheck "View User Guide"
- [ ] Click Finish
- [ ] Verify desktop shortcut created
- [ ] Verify USER_GUIDE.md does NOT open
- [ ] Verify Start Menu shortcuts work

### Test 4: Neither Option
- [ ] Run installer
- [ ] Uncheck both checkboxes
- [ ] Click Finish
- [ ] Verify NO desktop shortcut
- [ ] Verify USER_GUIDE.md does NOT open
- [ ] Verify Start Menu shortcuts work

### Test 5: Upgrade Install
- [ ] Install version 2.3.0 (if available)
- [ ] Install version 2.4.0 over it
- [ ] Verify upgrade completes successfully
- [ ] Verify USER_GUIDE.md is added
- [ ] Verify existing shortcuts still work

## Troubleshooting Build Issues

### Error: "Publish directory not found"
**Solution:**
```powershell
dotnet publish "..\Memory Monitor\Memory Monitor.csproj" -c Release -r win-x64 --self-contained true
```

### Error: "PublishedFiles.wxs not found"
**Solution:**
```powershell
powershell -ExecutionPolicy Bypass -File .\Regenerate-PublishedFiles.ps1
```

### Error: "USER_GUIDE.md not found"
**Solution:**
```powershell
# Make sure USER_GUIDE.md is in the repository root
Copy-Item ".\USER_GUIDE.md" ".." -Force
```

### Error: "Banner.bmp or Dialog.bmp not found"
**Note:** These are optional. If missing, WiX uses default images.
To add custom images:
- Create `Banner.bmp` (493×58 pixels)
- Create `Dialog.bmp` (493×312 pixels)
- Place in MemoryMonitorSetup folder

### Error: WiX namespace issues
If you see errors related to `xmlns:ui`, try building with verbose output:
```powershell
dotnet build MemoryMonitorSetup.wixproj -v detailed
```

## File Locations

### Source Files:
```
Repository Root/
?? USER_GUIDE.md                    ? User-friendly guide (NEW)
?? Memory Monitor/
?  ?? bin/Release/.../publish/      ? Published application files
?? MemoryMonitorSetup/
   ?? Package.wxs                   ? Updated with finish dialog
   ?? PublishedFiles.wxs            ? Auto-generated file list
   ?? License.rtf                   ? MIT license
   ?? Banner.bmp                    ? Optional installer banner
   ?? Dialog.bmp                    ? Optional installer background
   ?? Build-Installer.ps1           ? Automated build script
```

### Installation Locations:
```
C:\Program Files\Memory Monitor\
?? Memory Monitor.exe
?? USER_GUIDE.md                    ? New in v2.4.0
?? (all DLLs and dependencies)
?? (language folders)

Start Menu > Memory Monitor\
?? Memory Monitor                   ? Launch app
?? Memory Monitor User Guide        ? Open guide

Desktop (optional)
?? Memory Monitor                   ? Desktop shortcut
```

## Known Issues

### Issue: Custom checkboxes not visible
**WiX v5 Limitation:** The standard WixUI dialogs may not support custom checkbox text in all scenarios.

**Workaround:** The installer uses:
- `WIXUI_EXITDIALOGOPTIONALCHECKBOXTEXT` for desktop shortcut
- `WIXUI_EXITDIALOGOPTIONALCHECKBOX` property (1=checked, 0=unchecked)
- `LAUNCH_README` custom property for README launch

If checkboxes don't appear, this is a WiX v5 UI limitation. The installer will still work, creating shortcuts and opening the guide by default.

**Future Fix:** Consider creating custom WiX UI dialogs in a future release.

### Issue: USER_GUIDE.md opens in Notepad
**Cause:** Windows doesn't have a default markdown viewer installed.

**Solutions for Users:**
1. Install a markdown viewer (e.g., Typora, MarkdownPad, VS Code)
2. View in browser using a markdown preview extension
3. Right-click ? Open with ? Select preferred editor

**Future Enhancement:** Convert USER_GUIDE.md to HTML for v2.5.0

## Advanced Customization

### Change Default Checkbox States
Edit `Package.wxs`:
```xml
<!-- Desktop shortcut checked by default -->
<Property Id="WIXUI_EXITDIALOGOPTIONALCHECKBOX" Value="1" />

<!-- Change to unchecked by default: -->
<Property Id="WIXUI_EXITDIALOGOPTIONALCHECKBOX" Value="0" />

<!-- View guide checked by default -->
<Property Id="LAUNCH_README" Value="1" />

<!-- Change to unchecked by default: -->
<Property Id="LAUNCH_README" Value="0" />
```

### Disable Desktop Shortcut Completely
Remove the `DesktopShortcut` feature from Package.wxs.

### Always Create Desktop Shortcut
Change the `DesktopShortcut` feature Level to 1:
```xml
<Feature Id="DesktopShortcut" 
         Title="Desktop Shortcut" 
         Level="1">  <!-- Changed from 1000 to 1 -->
```

---

**Updated:** January 10, 2025  
**Version:** 2.4.0  
**WiX Version:** 5.0.2
