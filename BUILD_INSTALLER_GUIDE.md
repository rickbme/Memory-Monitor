# Build-Installer.bat Usage Guide

## Quick Start

Simply double-click `Build-Installer.bat` or run from command line:

```cmd
Build-Installer.bat
```

This will:
1. ? Verify prerequisites (.NET SDK, WiX Toolset)
2. ? Build the Memory Monitor application
3. ? Create the MSI installer
4. ? Display the installer location

## Command Line Options

### Standard Build
```cmd
Build-Installer.bat
```
Builds the application and creates the installer.

### Clean Build
```cmd
Build-Installer.bat clean
```
Removes all previous build output.

### Rebuild
```cmd
Build-Installer.bat rebuild
```
Cleans then builds everything from scratch.

### Test Build and Install
```cmd
Build-Installer.bat test
```
Builds the installer and offers to install it immediately for testing.

## Prerequisites

The batch file will automatically check for:

### 1. .NET 8 SDK
- **Required**: Yes
- **Auto-install**: No
- **Download**: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. WiX Toolset v4
- **Required**: Yes
- **Auto-install**: Yes (if missing)
- **Manual install**: `dotnet tool install --global wix`

### 3. Project Files
- **Checked**: Memory Monitor.csproj
- **Checked**: MemoryMonitorSetup.wixproj
- **Checked**: Package.wxs

## Output

### Build Success
```
============================================================================
 BUILD COMPLETE
============================================================================

[SUCCESS] Installer created successfully!

Location: C:\...\MemoryMonitorSetup\bin\Release\en-US\MemoryMonitorSetup.msi
Size: 2 MB
Created: 12/15/2024 10:30 AM

============================================================================

To install:
  msiexec /i "MemoryMonitorSetup.msi"

To install with logging:
  msiexec /i "MemoryMonitorSetup.msi" /l*v install.log

To uninstall:
  msiexec /x "MemoryMonitorSetup.msi"
```

### Build Failure
```
============================================================================
 BUILD FAILED
============================================================================

Please check the error messages above and try again.

For help, see:
  - MemoryMonitorSetup\INSTALLER_FIX_README.md
  - MemoryMonitorSetup\QUICK_REFERENCE.md
```

## What Gets Built

### Application Files (Step 1):
```
Memory Monitor\bin\Release\net8.0-windows\
??? Memory Monitor.exe          (Main application)
??? Memory Monitor.dll          (Application library)
??? Memory Monitor.deps.json    (Dependencies)
??? Memory Monitor.runtimeconfig.json
??? System.CodeDom.dll          (Dependency)
??? System.Management.dll       (Dependency)
```

### Installer Output (Step 3):
```
MemoryMonitorSetup\bin\Release\en-US\
??? MemoryMonitorSetup.msi      (Windows Installer package)
```

## Installation Targets

The installer will place files at:

### Program Files:
```
C:\Program Files\Memory Monitor\
 Memory Monitor.exe
 Memory Monitor.dll
 Memory Monitor.deps.json
 Memory Monitor.runtimeconfig.json
 System.CodeDom.dll
 System.Management.dll
```

### Start Menu:
```
Start Menu\Programs\Memory Monitor\
 Memory Monitor (shortcut)
```

### Registry:
```
HKCU\Software\MemoryMonitor
 installed = 1
```

## Common Issues and Solutions

### Issue 1: .NET SDK Not Found
```
[ERROR] .NET SDK not found!
```
**Solution**: Install .NET 8 SDK from https://dotnet.microsoft.com/download

### Issue 2: WiX Toolset Not Found
```
[WARNING] WiX Toolset not found!
Installing WiX Toolset...
```
**Solution**: The script will auto-install. If it fails, manually run:
```cmd
dotnet tool install --global wix
```

### Issue 3: Project File Not Found
```
[ERROR] Project file not found
```
**Solution**: 
- Run the batch file from the solution root directory
- Verify the project structure matches the expected layout

### Issue 4: Build Failed
```
[ERROR] Application build failed!
```
**Solution**: 
1. Open the solution in Visual Studio
2. Check for compilation errors
3. Fix errors and try again
4. Or build from Visual Studio directly first

### Issue 5: Installer Build Failed
```
[ERROR] Installer build failed!
```
**Solution**:
1. Check that all files in Package.wxs exist in bin folder
2. Verify Package.wxs has no syntax errors
3. Look for specific error messages in output
4. Check that all GUIDs in Package.wxs are unique

### Issue 6: MSI Not Found
```
[WARNING] MSI file not found at expected location
```
**Solution**:
- Check alternate locations in `MemoryMonitorSetup\bin\`
- Review build output for actual output path
- Verify WiX build completed without errors

## Build Process Details

### Step 0: Verify Prerequisites
- Checks for .NET SDK
- Checks for WiX Toolset
- Verifies project files exist
- Auto-installs WiX if missing

### Step 1: Build Application
```cmd
dotnet build "Memory Monitor\Memory Monitor.csproj" -c Release
```
- Compiles C# code
- Resolves dependencies
- Creates executable and DLLs
- Output: `Memory Monitor\bin\Release\net8.0-windows\`

### Step 2: Verify Build Output
- Checks main executable exists
- Lists all files to be included
- Verifies required dependencies present

### Step 3: Build Installer
```cmd
dotnet build "MemoryMonitorSetup\MemoryMonitorSetup.wixproj" -c Release
```
- Processes Package.wxs
- Creates MSI database
- Embeds application files
- Output: `MemoryMonitorSetup\bin\Release\en-US\MemoryMonitorSetup.msi`

### Step 4: Display Results
- Shows installer location
- Displays file size and timestamp
- Provides installation commands
- Offers test installation (if test mode)

## Advanced Usage

### Building from PowerShell
The original PowerShell script is still available:
```powershell
.\MemoryMonitorSetup\Build-Installer.ps1
```

### Manual Build Steps
If the batch file doesn't work, build manually:

```cmd
REM 1. Build application
cd "Memory Monitor"
dotnet build -c Release

REM 2. Build installer
cd ..\MemoryMonitorSetup
dotnet build -c Release

REM 3. Find MSI
dir /s /b *.msi
```

### Continuous Integration
For CI/CD pipelines:
```cmd
Build-Installer.bat rebuild
if errorlevel 1 exit /b 1
copy "MemoryMonitorSetup\bin\Release\en-US\*.msi" "%ARTIFACT_DIR%"
```

## Testing the Installer

### Test Installation
```cmd
Build-Installer.bat test
```
Then respond "Y" when prompted.

### Manual Test Installation
```cmd
msiexec /i "MemoryMonitorSetup.msi" /l*v install.log
```

### Check Installation
1. Look for Start Menu shortcut: "Memory Monitor"
2. Check Program Files: `C:\Program Files\Memory Monitor\`
3. Verify application runs: Click Start Menu shortcut

### Test Uninstallation
```cmd
msiexec /x "MemoryMonitorSetup.msi" /l*v uninstall.log
```

### Verify Clean Uninstall
1. Check Program Files folder removed
2. Check Start Menu shortcut removed
3. Check registry key removed (optional)

## Versioning

To change the installer version:

1. Open `MemoryMonitorSetup\Package.wxs`
2. Update the Version attribute:
```xml
<Package Version="1.0.0.0" ... >
```
3. Rebuild installer

Version format: `Major.Minor.Build.Revision`
- Example: `1.0.0.0` ? `1.1.0.0` (minor update)
- Example: `1.0.0.0` ? `2.0.0.0` (major update)

## Troubleshooting Build Logs

### Enable Detailed Logging
Edit the batch file, change:
```cmd
dotnet build ... -v minimal
```
To:
```cmd
dotnet build ... -v detailed
```

### Check Build Logs
Application build log:
```cmd
dotnet build "Memory Monitor\Memory Monitor.csproj" -c Release > app_build.log 2>&1
```

Installer build log:
```cmd
dotnet build "MemoryMonitorSetup\MemoryMonitorSetup.wixproj" -c Release > installer_build.log 2>&1
```

## Environment Variables

The batch file uses these variables:

| Variable | Purpose | Example |
|----------|---------|---------|
| SOLUTION_ROOT | Root directory | `C:\...\Memory-Monitor\` |
| PROJECT_PATH | Application project | `Memory Monitor\Memory Monitor.csproj` |
| INSTALLER_PROJECT | Installer project | `MemoryMonitorSetup\...wixproj` |
| BIN_PATH | Build output | `Memory Monitor\bin\Release\...` |
| MSI_PATH | Installer output | `MemoryMonitorSetup\bin\...msi` |

## File Structure

Required files for building installer:

```
Memory-Monitor\
??? Build-Installer.bat              ? New easy-to-use builder
??? Memory Monitor\
?   ??? Memory Monitor.csproj        ? Application project
?   ??? [source files]
??? MemoryMonitorSetup\
    ??? MemoryMonitorSetup.wixproj   ? Installer project
    ??? Package.wxs                  ? Installer definition
    ??? Build-Installer.ps1          ? PowerShell version
    ??? HarvestFiles.bat             ? File harvester (optional)
```

## Quick Reference

### Build Commands
```cmd
Build-Installer.bat          # Build
Build-Installer.bat clean    # Clean
Build-Installer.bat rebuild  # Clean + Build
Build-Installer.bat test     # Build + Test Install
```

### Install Commands
```cmd
msiexec /i MemoryMonitorSetup.msi              # Interactive install
msiexec /i MemoryMonitorSetup.msi /quiet       # Silent install
msiexec /i MemoryMonitorSetup.msi /l*v log.txt # With logging
```

### Uninstall Commands
```cmd
msiexec /x MemoryMonitorSetup.msi              # Interactive uninstall
msiexec /x MemoryMonitorSetup.msi /quiet       # Silent uninstall
```

## Support

If you encounter issues:

1. Review error messages carefully
2. Check prerequisites are installed
3. Verify project structure is correct
4. Check build logs for detailed errors

## Summary

The `Build-Installer.bat` file provides:

? **Easy to use**: Just double-click to build
? **Automatic checks**: Verifies prerequisites
? **Auto-install**: Installs WiX if missing
? **Clear output**: Shows what's happening
? **Error handling**: Helpful error messages
? **Multiple modes**: Build, clean, rebuild, test
? **Test installation**: Quick testing built-in

Perfect for developers who want a simple one-click build process without dealing with command-line details!
