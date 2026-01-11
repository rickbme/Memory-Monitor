# WiX Installer - Issues Fixed

## Problems Found and Resolved

### Issue 1: Advanced UI Features Not Compatible with WiX v5
**Problem:**
- Used `xmlns:ui` namespace for advanced UI features
- Attempted to use `ui:WixUI Id="WixUI_InstallDir"` 
- Referenced optional checkbox properties that aren't supported in basic WiX v5 setup

**Solution:**
- Removed advanced UI namespace
- Removed custom finish dialog properties
- Simplified to basic WiX v5 Package structure
- Installer now uses default WiX dialogs

### Issue 2: Missing Optional Image Files
**Problem:**
- Referenced `Banner.bmp` and `Dialog.bmp` that don't exist
- WiX build would fail looking for these files

**Solution:**
- Removed WixVariable references to optional images
- Installer will use default WiX dialog images

### Issue 3: Custom Actions and Properties
**Problem:**
- Custom action `LaunchReadme` used immediate execution
- Properties `LAUNCH_README` and `WIXUI_EXITDIALOGOPTIONALCHECKBOX` not standard

**Solution:**
- Removed custom actions for now
- Removed non-standard properties
- USER_GUIDE.md is still installed but won't auto-launch

## Current Installer Behavior

### What It Does:
1. ? Installs Memory Monitor to `C:\Program Files\Memory Monitor\`
2. ? Installs USER_GUIDE.md to same folder
3. ? Creates Start Menu folder with 2 shortcuts:
   - Memory Monitor (launches app)
   - Memory Monitor User Guide (opens USER_GUIDE.md)
4. ? Creates Desktop shortcut
5. ? Standard WiX installation dialogs

### What Was Removed (Temporarily):
1. ? Custom finish dialog with checkboxes
2. ? Optional desktop shortcut (now always created)
3. ? Auto-launch USER_GUIDE.md after install
4. ? Custom installer branding images

## How to Use

### Build the Installer:
```powershell
cd MemoryMonitorSetup
.\Build-Installer.ps1
```

### Install Memory Monitor:
1. Run `MemoryMonitorSetup.msi`
2. Follow standard installation wizard
3. Desktop shortcut is created automatically
4. Start Menu shortcuts are created
5. USER_GUIDE.md is installed but won't auto-open
6. User can open guide from Start Menu later

### Access User Guide:
**Option 1:** Start Menu ? Memory Monitor ? Memory Monitor User Guide  
**Option 2:** Navigate to `C:\Program Files\Memory Monitor\USER_GUIDE.md`  
**Option 3:** Right-click desktop shortcut ? Open File Location ? USER_GUIDE.md

## Future Enhancements

For a future release (v2.5.0), consider:

### Option A: Custom WiX UI Dialog (Complex)
Create custom WiX v5 dialogs with:
- Checkbox for desktop shortcut
- Checkbox for opening guide
- Custom branding

**Effort:** High  
**Complexity:** Requires WiX v5 custom UI XML

### Option B: Post-Install Script (Medium)
Add a post-install VBScript or PowerShell:
- Show custom dialog after install
- Ask if user wants to open guide
- Create desktop shortcut optionally

**Effort:** Medium  
**Complexity:** Requires CustomAction with scripts

### Option C: First-Run Experience (Simple)
Modify Memory Monitor app:
- Detect first run
- Show welcome dialog with link to guide
- Offer to create desktop shortcut

**Effort:** Low  
**Complexity:** Add to application code

**Recommendation:** Option C (First-Run Experience) is easiest and most user-friendly.

## Testing

### Before Installing:
- [ ] Uninstall any previous version of Memory Monitor
- [ ] Close Memory Monitor if running

### Install Test:
- [ ] Run MemoryMonitorSetup.msi
- [ ] Complete installation wizard
- [ ] Verify desktop shortcut created
- [ ] Verify Start Menu shortcuts created
- [ ] Verify USER_GUIDE.md installed
- [ ] Launch Memory Monitor from desktop
- [ ] Open User Guide from Start Menu

### Uninstall Test:
- [ ] Uninstall via Control Panel
- [ ] Verify desktop shortcut removed
- [ ] Verify Start Menu folder removed
- [ ] Verify installation folder removed

## Known Limitations

1. **Desktop Shortcut Always Created**
   - No option to skip during install
   - User can delete manually after install

2. **User Guide Doesn't Auto-Open**
   - Not launched after installation
   - User must open from Start Menu or file explorer

3. **No Custom Branding**
   - Uses default WiX dialog images
   - No custom installer graphics

4. **No Finish Dialog Customization**
   - Standard WiX finish screen
   - No custom message or options

## Workaround for Users

If users want a simpler setup:

### Delete Desktop Shortcut:
Right-click desktop shortcut ? Delete (after install)

### First-Time Setup:
1. Install Memory Monitor
2. Open Start Menu ? Memory Monitor ? Memory Monitor User Guide
3. Read the guide
4. Close guide
5. Launch Memory Monitor from Start Menu or desktop

---

**Fixed Version:** 2.4.0  
**Date:** January 10, 2025  
**Status:** Working - Basic Features Only  
**Next Steps:** Add first-run welcome dialog to application (v2.5.0)
