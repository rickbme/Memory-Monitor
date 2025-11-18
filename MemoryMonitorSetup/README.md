# System Monitor Installer

This directory contains the WiX installer project for System Monitor.

## Prerequisites

1. **WiX Toolset v5.0** or later
   - Download from: https://wixtoolset.org/
   - Or install via: `dotnet tool install --global wix`

2. **.NET 8.0 SDK**
   - Required to build the installer

## Building the Installer

### Option 1: Using Visual Studio
1. Open the solution in Visual Studio
2. Set build configuration to **Release**
3. Build the main Memory Monitor project first
4. Right-click the MemoryMonitorSetup project
5. Click "Build"
6. MSI will be in `MemoryMonitorSetup\bin\Release\`

### Option 2: Using Command Line

```bash
# Build the main application first
cd "Memory Monitor"
dotnet build --configuration Release

# Build the installer
cd ..\MemoryMonitorSetup
dotnet build --configuration Release
```

The installer MSI will be generated at:
```
MemoryMonitorSetup\bin\Release\net8.0\MemoryMonitorSetup.msi
```

## Installer Features

### Installation Options
- **Desktop Shortcut**: Creates a shortcut on the desktop
- **Start Menu Entry**: Creates folder in Start Menu
- **Installation Directory**: Default: `C:\Program Files\DFS\System Monitor`

### What Gets Installed
- System Monitor executable
- All required DLL dependencies
- Configuration files
- Desktop and Start Menu shortcuts

### Requirements Check
- Verifies .NET 8.0 Desktop Runtime is installed
- If not installed, provides download link to Microsoft

### Upgrade Behavior
- Automatically upgrades previous versions
- Preserves user settings (stored in AppData)
- Prevents downgrade to older versions

## Customization

### Change Installation Directory
Edit `Product.wxs`, find:
```xml
<Directory Id="INSTALLFOLDER" Name="System Monitor" />
```

### Remove Desktop Shortcut
In `Product.wxs`, remove or comment out:
```xml
<Shortcut Id="DesktopShortcut" ... />
```

### Change Product GUID
**Important**: Only change for major version updates
```xml
UpgradeCode="A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D"
```

### Update Version
Edit in `Product.wxs`:
```xml
Version="1.0.0.0"
```

## Troubleshooting

### Build Errors

**Error**: "Cannot find file: icon.ico"
- **Solution**: Create an icon file or remove Icon references from Product.wxs

**Error**: ".NET 8.0 Desktop Runtime dependency not found"
- **Solution**: Ensure you have .NET 8.0 SDK installed

**Error**: "File not found: Memory Monitor.exe"
- **Solution**: Build the main project in Release mode first

### Installation Errors

**Error during install**: "Another version is already installed"
- **Solution**: Uninstall existing version first via Control Panel

**Error**: ".NET 8.0 Desktop Runtime required"
- **Solution**: Install from https://dotnet.microsoft.com/download/dotnet/8.0

## File Structure

```
MemoryMonitorSetup/
??? MemoryMonitorSetup.wixproj  # WiX project file
??? Product.wxs                  # Main installer definition
??? License.rtf                  # MIT License text
??? README.md                    # This file
```

## Additional Resources

- [WiX Toolset Documentation](https://wixtoolset.org/docs/)
- [WiX Tutorial](https://www.firegiant.com/wix/tutorial/)
- [WiX on GitHub](https://github.com/wixtoolset/wix)

## Notes

- The installer is built as a single MSI file
- All dependencies are embedded (no separate cab files)
- Installer supports both install and uninstall
- Settings are preserved during upgrades
- Uninstall removes all files except user settings

---

**DFS - Dad's Fixit Shop**
Making tech accessible, reliable, and fun since 2025
