# WiX Installer - Pre-Release Checklist

Use this checklist before releasing a new version of Memory Monitor.

## Before Building

- [ ] Update version number in `Package.wxs` (e.g., `Version="2.5.0.0"`)
- [ ] Update version number in `Memory Monitor\Memory Monitor.csproj`
- [ ] Update `CHANGELOG.md` with release notes
- [ ] Verify `UpgradeCode` in Package.wxs **has NOT changed** (should always be `D9A1E6B2-1234-4F56-9A2B-ABCDEF012345`)

## Build Process

- [ ] Run `MemoryMonitorSetup\build-installer.bat Release`
- [ ] Verify build completes without errors
- [ ] Check output exists: `MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi`
- [ ] Verify MSI file size is reasonable (~60-80 MB for self-contained)

## Testing - Fresh Install

- [ ] Copy MSI to test machine (Windows 10/11 without Memory Monitor)
- [ ] Double-click MSI to install
- [ ] Verify installation completes successfully
- [ ] Check installed location: `C:\Program Files\Memory Monitor\`
- [ ] Verify Start Menu shortcut exists and works
- [ ] Verify Desktop shortcut exists and works
- [ ] Run Memory Monitor and verify it works correctly
- [ ] Check Add/Remove Programs entry:
  - [ ] Correct name "Memory Monitor"
  - [ ] Correct version (e.g., 2.5.0.0)
  - [ ] Correct publisher "DFS"
  - [ ] App icon displays correctly

## Testing - Upgrade

- [ ] Install previous version (e.g., 2.4.0) on test machine
- [ ] Run the application to verify it works
- [ ] Note the installation directory and shortcuts
- [ ] Run new MSI (e.g., 2.5.0)
- [ ] Verify upgrade process:
  - [ ] No error messages
  - [ ] Old version removed automatically
  - [ ] New version installed
  - [ ] Shortcuts still work
  - [ ] No duplicate entries in Add/Remove Programs
- [ ] Run upgraded application and verify it works
- [ ] Check version in Add/Remove Programs matches new version

## Testing - Uninstall

- [ ] Go to Windows Settings ? Apps ? Installed apps
- [ ] Find "Memory Monitor" and click Uninstall
- [ ] Verify uninstall completes successfully
- [ ] Check that program files are removed: `C:\Program Files\Memory Monitor\`
- [ ] Check that Start Menu shortcut is removed
- [ ] Check that Desktop shortcut is removed
- [ ] Check that registry keys are removed: `HKCU\Software\MemoryMonitor`

## Testing - Edge Cases

- [ ] Test downgrade prevention (install new version, try to install older version)
  - [ ] Should show error: "A newer version of Memory Monitor is already installed"
- [ ] Test installation with spaces in username/path
- [ ] Test on both Windows 10 and Windows 11
- [ ] Test on machine **without** .NET 8 installed (should work - self-contained)

## Code Signing (Optional for Production)

- [ ] Sign the MSI with code signing certificate
- [ ] Verify signature is valid after signing
- [ ] Test installation with signed MSI
- [ ] Verify Windows SmartScreen doesn't block installation

## Documentation

- [ ] Update version number in documentation
- [ ] Tag release in Git: `git tag v2.5.0`
- [ ] Create GitHub release with release notes
- [ ] Attach MSI file to GitHub release

## Distribution

- [ ] Upload MSI to distribution location
- [ ] Update download links on website/README
- [ ] Announce release (if applicable)

## Post-Release

- [ ] Monitor for installation issues
- [ ] Check for upgrade reports from users
- [ ] Document any issues for next release

---

## Quick Test Commands

```powershell
# Check installed version
Get-ItemProperty "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*" | Where-Object { $_.DisplayName -eq "Memory Monitor" } | Select-Object DisplayName, DisplayVersion

# Check if application is running
Get-Process -Name "Memory Monitor" -ErrorAction SilentlyContinue

# Verify installation directory
Test-Path "C:\Program Files\Memory Monitor\Memory Monitor.exe"

# Check shortcuts
Test-Path "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Memory Monitor\Memory Monitor.lnk"
Test-Path "$env:USERPROFILE\Desktop\Memory Monitor.lnk"
```

## Notes

- Always test upgrades from the **previous public release** version
- Test on a clean machine that matches your minimum supported OS
- Self-contained installer means .NET runtime is included - no separate install needed
- Keep the `UpgradeCode` stable - it's the key to successful upgrades
- Version format is `Major.Minor.Build.Revision` (all four numbers matter)

---

**Template Version**: 1.0  
**Last Updated**: January 9, 2026
