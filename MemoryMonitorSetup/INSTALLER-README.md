# Memory Monitor Installer Build Guide

This document explains how to build the MSI installer for Memory Monitor using WiX Toolset v5.

## Prerequisites

1. **.NET 8 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **WiX Toolset v5** - Automatically restored via NuGet when building the wixproj
3. **PowerShell** - For automatic PublishedFiles.wxs generation (included with Windows)

## Quick Start

### Use the Build Script (Recommended)

From the solution root or MemoryMonitorSetup folder, run:

```batch
MemoryMonitorSetup\build-installer.bat
```

Or specify a configuration:

```batch
MemoryMonitorSetup\build-installer.bat Release
MemoryMonitorSetup\build-installer.bat Debug
```

The script will **automatically**:
1. Clean previous builds
2. Build the Memory Monitor application
3. Publish as self-contained (includes .NET runtime)
4. **Regenerate PublishedFiles.wxs** to match current files
5. Build the MSI installer

## Output

After a successful build, the installer will be located at:

```
MemoryMonitorSetup\bin\Release\en-us\MemoryMonitorSetup.msi
```

## What the Installer Does

When users run the MSI installer, it will:

- Install Memory Monitor to `C:\Program Files\Memory Monitor\`
- Create a Start Menu shortcut under "Memory Monitor"
- Create a Desktop shortcut
- Add an entry to Add/Remove Programs with the app icon
- **Automatically upgrade** existing installations (removes old version first)
- Support uninstall through Windows Settings

## Upgrade Behavior

The installer is configured to **automatically detect and upgrade** previous installations:

- Same `UpgradeCode` across versions enables upgrade detection
- `MajorUpgrade` element automatically removes old versions
- Users can install new versions without manually uninstalling
- Downgrades are blocked (prevents installing older versions over newer ones)

**See `VERSION-UPDATE-GUIDE.md` for details on releasing new versions.**

## Project Structure

```
MemoryMonitorSetup/
??? MemoryMonitorSetup.wixproj      # WiX project file
??? Package.wxs                      # Main installer definition (version, features, shortcuts)
??? PublishedFiles.wxs               # Auto-generated file list (DO NOT EDIT MANUALLY)
??? build-installer.bat              # Automated build script
??? Regenerate-PublishedFiles.ps1    # PowerShell script to regenerate file list
??? INSTALLER-README.md              # This file
??? VERSION-UPDATE-GUIDE.md          # Version update instructions
```

## Manual Build (Advanced)

If you need to build manually without the script:

```batch
REM From solution root directory

REM 1. Build the application
dotnet build "Memory Monitor\Memory Monitor.csproj" -c Release -r win-x64

REM 2. Publish self-contained
dotnet publish "Memory Monitor\Memory Monitor.csproj" -c Release -r win-x64 --self-contained true -o "Memory Monitor\bin\Release\net8.0-windows\win-x64\publish"

REM 3. Regenerate PublishedFiles.wxs
cd MemoryMonitorSetup
powershell -ExecutionPolicy Bypass -File "Regenerate-PublishedFiles.ps1"
cd ..

REM 4. Build the installer
dotnet build "MemoryMonitorSetup\MemoryMonitorSetup.wixproj" -c Release
```

## Troubleshooting

### Error: "Publish directory not found"

The application must be published before building the installer.

**Solution:** Run the full build-installer.bat script, which handles this automatically.

### Error: "PublishedFiles.wxs not found"

The file list must be generated before building.

**Solution:** Run `Regenerate-PublishedFiles.ps1` or use build-installer.bat.

### Error: Component group not found (e.g., "csComponents")

PublishedFiles.wxs contains ComponentGroups for subdirectories (language folders, Resources, etc.). If you see errors about missing component groups:

1. Check that PublishedFiles.wxs was regenerated
2. Verify the ComponentGroupRef entries in Package.wxs match the generated groups
3. See the output from Regenerate-PublishedFiles.ps1 for the list of generated groups

### Files Added/Removed in Application

If you add or remove files from the application:

1. Run `build-installer.bat` (it regenerates PublishedFiles.wxs automatically)
2. Check the console output for new ComponentGroup names
3. Update Package.wxs if new subdirectories were added

**Note:** The build script handles this automatically, but if you see errors about missing components, compare the `<ComponentGroupRef>` entries in Package.wxs with the output of Regenerate-PublishedFiles.ps1.

### WiX Toolset not found

The WiX SDK should be restored automatically via NuGet. If you see errors:

```batch
dotnet restore "MemoryMonitorSetup\MemoryMonitorSetup.wixproj"
```

If this fails, ensure you have .NET 8 SDK installed and your NuGet sources are configured.

## Version Updates

To release a new version (e.g., 2.4.0 ? 2.5.0):

1. Update `Version` attribute in `Package.wxs`
2. **DO NOT** change the `UpgradeCode` (must remain stable)
3. Run `build-installer.bat`

For detailed version update instructions, see **`VERSION-UPDATE-GUIDE.md`**.

## Self-Contained Deployment

The installer includes the .NET 8 runtime, so users don't need to install .NET separately. This results in a larger installer (~60-80 MB) but provides:

- No dependency on user's installed .NET version
- Works on any Windows 10/11 x64 system
- Consistent runtime behavior
- Simpler installation experience

## Administrator Privileges

Memory Monitor requires administrator privileges to access hardware sensors. The `app.manifest` in the main project requests elevation. The installer itself runs with normal privileges, but the installed application will request admin rights when launched.

## Building from Visual Studio

If you prefer to build from Visual Studio:

1. Right-click on **Memory Monitor** project ? **Publish**
2. Publish to `bin\Release\net8.0-windows\win-x64\publish`
3. Run `MemoryMonitorSetup\Regenerate-PublishedFiles.ps1` from PowerShell
4. Right-click on **MemoryMonitorSetup** project ? **Build**

**Tip:** It's easier to just run `build-installer.bat` which automates all these steps.

## Support

For issues with the installer, please open an issue at:
https://github.com/rickbme/Memory-Monitor/issues

Include:
- Build script output (copy/paste the console output)
- MSI build log (if available)
- Windows version
- Whether this is a fresh install or upgrade
