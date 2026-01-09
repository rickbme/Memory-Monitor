# Version Update Guide for Memory Monitor Installer

## Quick Reference: Releasing a New Version

When releasing a new version of Memory Monitor (e.g., from 2.4.0 to 2.5.0), follow these steps:

### 1. Update Version in Package.wxs

Edit `MemoryMonitorSetup/Package.wxs` and change the `Version` attribute:

```xml
<Package Name="Memory Monitor" 
         Manufacturer="DFS" 
         Version="2.5.0.0"  ? UPDATE THIS
         UpgradeCode="D9A1E6B2-1234-4F56-9A2B-ABCDEF012345"  ? DO NOT CHANGE
         ...>
```

**CRITICAL:**
- ? **DO** update the `Version` attribute for each release
- ? **DO NOT** change the `UpgradeCode` GUID (this must remain stable forever)
- ? **DO NOT** add `Id="*"` to the Package element (WiX v5 handles this automatically)

### 2. Update Version in Application

Update the version in your application's `.csproj` file:

```xml
<PropertyGroup>
  <Version>2.5.0</Version>
  <AssemblyVersion>2.5.0.0</AssemblyVersion>
  <FileVersion>2.5.0.0</FileVersion>
</PropertyGroup>
```

### 3. Build the Installer

Run the build script:

```batch
cd MemoryMonitorSetup
build-installer.bat Release
```

The script will automatically:
1. Build the application
2. Publish it as self-contained
3. Regenerate PublishedFiles.wxs
4. Build the MSI installer

### 4. Test the Upgrade

1. Install the old version (if not already installed)
2. Install the new MSI
3. Verify that:
   - The installer detects the old version
   - The old version is automatically removed
   - The new version installs successfully
   - Application shortcuts still work

## How Windows Installer Upgrades Work

### UpgradeCode (NEVER CHANGE)
- Unique GUID that identifies your product family
- Must remain **identical** across all versions
- Changing this breaks the upgrade path

### Version Number
- Format: `Major.Minor.Build.Revision` (e.g., `2.4.0.0`)
- Windows compares the first **three** fields for upgrades
- `2.4.0.0` ? `2.5.0.0` is a major upgrade (allowed)
- `2.5.0.0` ? `2.4.0.0` is a downgrade (blocked by default)

### MajorUpgrade Element
```xml
<MajorUpgrade AllowDowngrades="no" 
              DowngradeErrorMessage="..." />
```
- Automatically removes old versions before installing new ones
- `AllowDowngrades="no"` prevents users from installing older versions

## Troubleshooting Upgrades

### "Another version is already installed"
- Check that the `UpgradeCode` in Package.wxs matches previous releases
- Verify that the new `Version` is higher than the installed version

### "Error 1638: Another version of this product is already installed"
- This means Windows detected the same version number
- Increment the `Version` number in Package.wxs

### Installer doesn't detect old version
- The `UpgradeCode` may have been changed (bad!)
- Check registry: `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall`
- Find the old version's entry and compare UpgradeCode

## Component Rules (for developers adding/removing files)

### When adding new files:
1. Publish the application
2. Run `Regenerate-PublishedFiles.ps1`
3. Check the output for new ComponentGroup IDs
4. Add the new ComponentGroupRef entries to Package.wxs Feature section

### When removing files:
1. Regenerate PublishedFiles.wxs (will exclude removed files)
2. Remove obsolete ComponentGroupRef entries from Package.wxs

### Stable Component GUIDs
- The `Regenerate-PublishedFiles.ps1` script generates **deterministic GUIDs** based on file paths
- Same file path = same GUID (even across regenerations)
- This ensures Windows Installer can track file changes properly

## Quick Checklist for Release

- [ ] Version updated in `Package.wxs`
- [ ] Version updated in application `.csproj`
- [ ] CHANGELOG.md updated with new version notes
- [ ] `build-installer.bat` runs without errors
- [ ] MSI created successfully in `MemoryMonitorSetup\bin\Release\en-us\`
- [ ] Upgrade test performed (install old version, then new MSI)
- [ ] Desktop and Start Menu shortcuts work
- [ ] Uninstall test performed

## Reference Links

- WiX v5 Documentation: https://wixtoolset.org/docs/
- Windows Installer Versioning: https://learn.microsoft.com/en-us/windows/win32/msi/version
- Semantic Versioning: https://semver.org/
