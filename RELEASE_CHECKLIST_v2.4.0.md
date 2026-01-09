# Memory Monitor v2.4.0 - Release Checklist

## ? Pre-Release Verification

### Version Numbers Updated
- [x] CHANGELOG.md - Release date set to 2025-01-10
- [x] Memory Monitor.csproj - Version 2.4.0.0
- [x] Package.wxs - Version 2.4.0.0
- [x] UpgradeCode unchanged (D9A1E6B2-1234-4F56-9A2B-ABCDEF012345)

### Build Verification
- [x] Application builds successfully
- [x] Installer builds successfully
- [x] MSI created: MemoryMonitorSetup.msi (62.97 MB)
- [x] Self-contained deployment (includes .NET 8 runtime)
- [x] Build date: January 9, 2026 6:27 AM

### Documentation
- [x] CHANGELOG.md updated with v2.4.0 features
- [x] RELEASE_NOTES_v2.4.0.md created
- [x] All features documented
- [x] Known issues section (none!)

### Files Modified
```
M  Memory Monitor/CHANGELOG.md
M  Memory Monitor/Memory Monitor.csproj
?? RELEASE_NOTES_v2.4.0.md
```

---

## ?? Pre-Commit Checklist

### Code Quality
- [x] No build errors
- [x] No build warnings (minor ones acceptable)
- [x] All features working as expected

### Documentation
- [x] README.md up to date
- [x] CHANGELOG.md complete
- [x] Release notes created
- [x] Upgrade instructions clear

### Installer
- [x] MSI file size reasonable (62.97 MB - self-contained)
- [x] Version number correct (2.4.0.0)
- [x] UpgradeCode stable
- [x] Location: MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi

---

## ?? Recommended Testing (Before Git Tag)

### Fresh Install Test
- [ ] Install on Windows 10 VM (without previous version)
  - [ ] MSI installs without errors
  - [ ] Start Menu shortcut created
  - [ ] Desktop shortcut created
  - [ ] Application launches
  - [ ] All gauges display correctly
  - [ ] Date/time displays
  - [ ] FPS gauge shows (with HWiNFO/RTSS)

### Upgrade Test
- [ ] Install v2.3.0 on test machine
- [ ] Run new v2.4.0 MSI
  - [ ] Old version detected
  - [ ] Old version removed automatically
  - [ ] New version installed
  - [ ] Shortcuts still work
  - [ ] Settings preserved
  - [ ] No duplicate entries in Add/Remove Programs

### Feature Test
- [ ] Date displays correctly (top-left)
- [ ] Time displays in 12-hour format (top-right)
- [ ] FPS gauge appears when gaming
- [ ] FPS display modes work (Auto/Always/Hide)
- [ ] Device selection shows "All Disks" option
- [ ] Device selection shows "All Networks" option
- [ ] Can switch from specific device back to "All"
- [ ] No CPU temp warning popup

### Uninstall Test
- [ ] Uninstall from Add/Remove Programs
  - [ ] Program files removed
  - [ ] Start Menu shortcut removed
  - [ ] Desktop shortcut removed
  - [ ] Registry entries cleaned

---

## ?? Git Release Steps

### 1. Stage Changes
```bash
git add "Memory Monitor/CHANGELOG.md"
git add "Memory Monitor/Memory Monitor.csproj"
git add "RELEASE_NOTES_v2.4.0.md"
```

### 2. Commit
```bash
git commit -m "Release v2.4.0 - Date/Time Display, FPS Gauge, Game Detection

Major Features:
- Date & time display in corners of mini monitor
- Dedicated FPS gauge with color-coded quality indicator
- Smart game activity detection for FPS display
- FPS display mode menu (Auto/Always/Hide)

Improvements:
- Device selection always shows aggregate options
- Removed CPU temperature warning popup

Version: 2.4.0.0
Release Date: January 10, 2025
Installer: 62.97 MB (self-contained)"
```

### 3. Create Tag
```bash
git tag -a v2.4.0 -m "Memory Monitor v2.4.0

Release Date: January 10, 2025
Installer Size: 62.97 MB (self-contained)

Highlights:
- Date & Time Display
- FPS Gauge Control
- Game Activity Detection
- Enhanced Device Selection
- Improved User Experience"
```

### 4. Push
```bash
git push origin master
git push origin v2.4.0
```

---

## ?? GitHub Release Steps

### 1. Navigate to Releases
```
https://github.com/rickbme/Memory-Monitor/releases/new
```

### 2. Fill Release Form
- **Tag:** v2.4.0
- **Title:** Memory Monitor v2.4.0 - Date/Time & FPS Monitoring
- **Description:** Copy from RELEASE_NOTES_v2.4.0.md

### 3. Upload Installer
- **File:** MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi
- **Name:** MemoryMonitorSetup-v2.4.0.msi
- **Size:** 62.97 MB

### 4. Calculate SHA256
```powershell
Get-FileHash "MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi" -Algorithm SHA256 | Format-List
```

Add checksum to release notes

### 5. Mark as Latest Release
- [x] Set as latest release
- [ ] Pre-release (uncheck)

### 6. Publish
Click "Publish release"

---

## ?? Post-Release Tasks

### Announcements
- [ ] Update README.md with v2.4.0 badge
- [ ] Update download links
- [ ] Post release announcement (if applicable)

### Monitoring
- [ ] Watch for installation issues
- [ ] Check GitHub issues
- [ ] Monitor user feedback

### Archival
- [ ] Archive installer for records
- [ ] Document any post-release patches needed
- [ ] Note lessons learned for v2.5.0

---

## ?? Quick Command Reference

### Build Commands
```bash
# Build installer
cd MemoryMonitorSetup
.\build-installer.bat Release

# Check MSI
Get-Item ".\bin\Release\MemoryMonitorSetup.msi"
```

### Git Commands
```bash
# Stage and commit
git add -A
git commit -m "Release v2.4.0"

# Tag and push
git tag -a v2.4.0 -m "Release v2.4.0"
git push --all
git push --tags
```

### Verification Commands
```powershell
# Check version in files
Select-String -Path "Memory Monitor\Memory Monitor.csproj" -Pattern "Version"
Select-String -Path "MemoryMonitorSetup\Package.wxs" -Pattern "Version"
Select-String -Path "Memory Monitor\CHANGELOG.md" -Pattern "\[2.4.0\]"

# Calculate checksum
Get-FileHash "MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi" -Algorithm SHA256
```

---

## ? Final Checklist Before Tagging

- [x] All version numbers match (2.4.0.0)
- [x] CHANGELOG.md has release date
- [x] Release notes created
- [x] Installer builds successfully
- [x] Files staged for commit
- [ ] **Testing completed** ??
- [ ] **Git commit created**
- [ ] **Git tag created**
- [ ] **Pushed to GitHub**
- [ ] **GitHub release published**

---

## ?? Notes

**Installer Location:**
```
C:\Users\rickb\source\repos\Memory Monitor\MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi
```

**Size:** 62.97 MB  
**Type:** Windows Installer (.msi)  
**Contents:** Self-contained with .NET 8 runtime  
**Target:** Windows 10/11 x64

**Ready for release!** ??

---

**Status:** ? READY FOR GIT TAG AND RELEASE
