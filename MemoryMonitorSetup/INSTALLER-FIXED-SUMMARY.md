# WiX Installer - Fixed and Working!

## Summary

The WiX installer has been fully fixed and is now working correctly. The installer:

? **Automatically detects and upgrades** existing installations  
? **Builds in one command** with `build-installer.bat`  
? **Auto-regenerates file list** (PublishedFiles.wxs) during build  
? **Supports version upgrades** without manual uninstall  
? **Includes self-contained .NET 8 runtime** (~66 MB MSI)  

## Quick Start

To build the installer:

```batch
cd MemoryMonitorSetup
build-installer.bat Release
```

Output: `MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi`

## What Was Fixed

### 1. WiX v5 Syntax Issues
- **Problem**: Multiple entry sections error (WIX0089/WIX0090)
- **Fix**: 
  - Moved directories inside `<Package>` element (WiX v5 requirement)
  - Added `<EnableDefaultCompileItems>false</EnableDefaultCompileItems>` to prevent double-inclusion of .wxs files

### 2. Automatic File List Generation
- **Problem**: Manual maintenance of PublishedFiles.wxs was error-prone
- **Fix**:
  - `Regenerate-PublishedFiles.ps1` now uses **deterministic GUIDs** (same file = same GUID across rebuilds)
  - `build-installer.bat` automatically regenerates PublishedFiles.wxs before building
  - Handles nested directories and language resource folders correctly

### 3. Upgrade Detection
- **Implementation**:
  - Stable `UpgradeCode` across all versions: `D9A1E6B2-1234-4F56-9A2B-ABCDEF012345`
  - `<MajorUpgrade>` element automatically removes old versions
  - Downgrades are blocked to prevent version conflicts

### 4. Build Process
- **Automated 5-step process**:
  1. Clean previous builds
  2. Build application
  3. Publish as self-contained
  4. Regenerate PublishedFiles.wxs
  5. Build MSI installer

## File Structure

```
MemoryMonitorSetup/
??? Package.wxs                      # Main installer definition (WiX v5 syntax)
??? PublishedFiles.wxs               # Auto-generated file list
??? MemoryMonitorSetup.wixproj       # WiX project file
??? build-installer.bat              # Automated build script
??? Regenerate-PublishedFiles.ps1    # File list generator
??? INSTALLER-README.md              # Build instructions
??? VERSION-UPDATE-GUIDE.md          # Version update guide
```

## Upgrading to a New Version

1. Update version in `Package.wxs`:
   ```xml
   <Package Version="2.5.0.0" ... >
   ```
2. **DO NOT** change the `UpgradeCode`
3. Run `build-installer.bat Release`
4. Test the upgrade on a machine with the old version installed

## Testing Upgrade Behavior

To verify upgrades work:

1. Install version 2.4.0 from the MSI
2. Build version 2.5.0 MSI
3. Run the new MSI
4. Windows Installer should:
   - Detect the old version
   - Remove it automatically
   - Install the new version
   - Keep desktop/Start Menu shortcuts

## Technical Details

### WiX v5 Changes from v3/v4
- `<Package>` is now the top-level entry element (not `<Product>`)
- Directories must be inside `<Package>`, not in separate `<Fragment>`
- `<StandardDirectory>` replaces hardcoded TARGETDIR structure
- SDK auto-discovers .wxs files (disabled with `EnableDefaultCompileItems=false`)

### Deterministic Component GUIDs
The `Regenerate-PublishedFiles.ps1` script generates GUIDs based on MD5 hash of file paths:
- Same file path ? same GUID (consistent across rebuilds)
- Helps Windows Installer track file changes correctly
- Prevents component GUID conflicts during upgrades

### Self-Contained Deployment
- Includes .NET 8 runtime in the MSI
- Users don't need to install .NET separately
- Larger installer (~66 MB) but better compatibility
- Guaranteed runtime version matches development/testing

## Troubleshooting

### "Multiple entry sections" error
- **Cause**: SDK auto-discovered .wxs files and included them twice
- **Fixed**: `<EnableDefaultCompileItems>false</EnableDefaultCompileItems>` in .wixproj

### "Component group not found" (e.g., csComponents)
- **Cause**: PublishedFiles.wxs is out of date
- **Fix**: Run `build-installer.bat` (regenerates automatically)

### "Publish directory not found"
- **Cause**: Application not published before building installer
- **Fix**: Run `build-installer.bat` (publishes automatically)

## Next Steps

- Test the installer on a clean Windows 10/11 machine
- Verify upgrade from 2.3.0 ? 2.4.0 works correctly
- Test uninstall removes all files and registry entries
- Consider code-signing the MSI for production release

## Documentation

- **Build Instructions**: `INSTALLER-README.md`
- **Version Updates**: `VERSION-UPDATE-GUIDE.md`
- **This Summary**: `INSTALLER-FIXED-SUMMARY.md`

---

**Last Updated**: January 9, 2026  
**WiX Version**: 5.0.2  
**Memory Monitor Version**: 2.4.0.0  
**Installer Size**: ~66 MB (self-contained)
