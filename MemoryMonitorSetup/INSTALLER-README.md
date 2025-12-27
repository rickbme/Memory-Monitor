# Memory Monitor Installer Build Guide

This document explains how to build the MSI installer for Memory Monitor using WiX Toolset v5.

## Prerequisites

1. **.NET 8 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **WiX Toolset v5** - Automatically restored via NuGet when building the wixproj

## Quick Start

### Option 1: Use the Build Script (Recommended)

Simply run the batch file from the `MemoryMonitorSetup` folder:

```batch
build-installer.bat
```

Or specify a configuration:

```batch
build-installer.bat Release
build-installer.bat Debug
```

The script will:
1. Clean previous builds
2. Build the Memory Monitor application
3. Publish as self-contained (no .NET runtime required on target machine)
4. Build the MSI installer

### Option 2: Manual Build

From the solution root directory, run these commands in order:

```batch
REM 1. Build the application
dotnet build "Memory Monitor\Memory Monitor.csproj" -c Release -r win-x64

REM 2. Publish self-contained
dotnet publish "Memory Monitor\Memory Monitor.csproj" -c Release -r win-x64 --self-contained true -o "Memory Monitor\bin\Release\net8.0-windows\win-x64\publish"

REM 3. Build the installer
dotnet build "MemoryMonitorSetup\MemoryMonitorSetup.wixproj" -c Release
```

## Output

After a successful build, the installer will be located at:

```
MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi
```

## What the Installer Does

When users run the MSI installer, it will:

- Install Memory Monitor to `C:\Program Files\Memory Monitor\`
- Create a Start Menu shortcut under "Memory Monitor"
- Create a Desktop shortcut
- Add an entry to Add/Remove Programs with the app icon
- Support upgrade/uninstall through Windows Settings

## Project Structure

```
MemoryMonitorSetup/
??? MemoryMonitorSetup.wixproj  # WiX project file
??? Package.wxs                  # Main installer definition (shortcuts, features)
??? PublishedFiles.wxs           # Auto-generated file list from publish folder
??? build-installer.bat          # Build automation script
??? INSTALLER-README.md          # This file
```

## Troubleshooting

### Error: "Source file not found"

This error occurs when `PublishedFiles.wxs` references files that don't exist in the publish folder.

**Solution:** Make sure you publish the application before building the installer:

```batch
dotnet publish "Memory Monitor\Memory Monitor.csproj" -c Release -r win-x64 --self-contained true -o "Memory Monitor\bin\Release\net8.0-windows\win-x64\publish"
```

### Error: WiX Toolset not found

The WiX SDK is referenced in `MemoryMonitorSetup.wixproj`. If NuGet restore fails:

```batch
dotnet restore "MemoryMonitorSetup\MemoryMonitorSetup.wixproj"
```

### Regenerating PublishedFiles.wxs

If you add or remove files from the application, you need to regenerate `PublishedFiles.wxs`:

1. First, publish the application to get the current file list
2. Use the WiX `heat` tool or manually update the file:

```batch
REM Using heat tool (if installed globally)
heat dir "Memory Monitor\bin\Release\net8.0-windows\win-x64\publish" -cg PublishedFileComponents -dr INSTALLFOLDER -srd -sfrag -ag -var var.PublishDir -out "MemoryMonitorSetup\PublishedFiles.wxs"
```

Or create a PowerShell script to regenerate it (see `regenerate-filelist.ps1` if available).

### Updating Version Number

To update the installer version, edit `Package.wxs`:

```xml
<Package Name="Memory Monitor" 
         Version="1.0.0.0"   <!-- Change this -->
         ...>
```

**Important:** When releasing a new version:
- Update the `Version` attribute
- Keep the same `UpgradeCode` GUID (this enables upgrades)
- The `MajorUpgrade` element handles uninstalling old versions automatically

## Self-Contained Deployment

The installer includes the .NET runtime, so users don't need to install .NET separately. This results in a larger installer (~55MB) but provides:

- No dependency on user's installed .NET version
- Works on any Windows 10/11 x64 system
- Consistent runtime behavior

## Building from Visual Studio

1. Right-click on `Memory Monitor` project ? **Publish**
2. Create/select a folder publish profile with:
   - Target: `win-x64`
   - Deployment mode: `Self-contained`
   - Target location: `bin\Release\net8.0-windows\win-x64\publish`
3. Click **Publish**
4. Right-click on `MemoryMonitorSetup` project ? **Build**

## Administrator Privileges

Memory Monitor requires administrator privileges to access hardware sensors. The `app.manifest` in the main project requests elevation. The installer itself runs with normal privileges but the installed application will request admin rights when launched.

## Support

For issues with the installer, please open an issue at:
https://github.com/rickbme/Memory-Monitor/issues
