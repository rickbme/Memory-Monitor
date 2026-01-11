# WiX Installer Setup Instructions

## Quick Start (for developers who already have WiX installed)

Simply run:
```bash
BuildInstaller.bat
```

This will:
1. Build the Memory Monitor application
2. Build the WiX installer
3. Show you where the MSI file is located

---

## Full Setup (First Time)

### Step 1: Install Prerequisites

#### Install .NET 8.0 SDK
```bash
# Download from:
https://dotnet.microsoft.com/download/dotnet/8.0

# Or use winget:
winget install Microsoft.DotNet.SDK.8
```

#### Install WiX Toolset v5.0
```bash
# Method 1: Using dotnet tool (Recommended)
dotnet tool install --global wix

# Method 2: Download installer
# Visit: https://wixtoolset.org/
```

#### Verify Installation
```bash
# Check .NET SDK
dotnet --version
# Should show: 8.0.x

# Check WiX
wix --version
# Should show: 5.0.x
```

### Step 2: Create Application Icon (Optional)

If you want a custom icon in Add/Remove Programs:

1. Create or obtain an `.ico` file (256x256 recommended)
2. Save it as `Memory Monitor\icon.ico`
3. The installer will automatically use it

**OR** remove icon references from `Product.wxs`:
```xml
<!-- Remove or comment out these lines -->
<Icon Id="AppIcon.ico" SourceFile="..\Memory Monitor\icon.ico" />
<Property Id="ARPPRODUCTICON" Value="AppIcon.ico" />
```

### Step 3: Build the Installer

#### Option A: Using the Build Script (Easiest)
```bash
# Simply double-click or run:
BuildInstaller.bat
```

#### Option B: Manual Build
```bash
# 1. Build the application
cd "Memory Monitor"
dotnet build --configuration Release
cd ..

# 2. Build the installer
cd MemoryMonitorSetup
dotnet build --configuration Release
cd ..
```

#### Option C: Using Visual Studio
1. Open `Memory Monitor.sln` in Visual Studio
2. Set configuration to **Release**
3. Build Solution (Ctrl+Shift+B)
4. The MSI will be in `MemoryMonitorSetup\bin\Release\`

### Step 4: Locate the Installer

The MSI file will be at:
```
MemoryMonitorSetup\bin\Release\net8.0\MemoryMonitorSetup.msi
```

---

## Testing the Installer

### Install
```bash
# Double-click the MSI file
# Or run from command line:
msiexec /i MemoryMonitorSetup.msi
```

### Silent Install (for IT deployment)
```bash
msiexec /i MemoryMonitorSetup.msi /qn
```

### Uninstall
```bash
# Via Control Panel:
# Settings > Apps > System Monitor > Uninstall

# Or silent uninstall:
msiexec /x MemoryMonitorSetup.msi /qn
```

### Install with Logging (for troubleshooting)
```bash
msiexec /i MemoryMonitorSetup.msi /l*v install.log
```

---

## Troubleshooting

### Build Errors

**"wix: command not found"**
```bash
# Solution: Install WiX toolset
dotnet tool install --global wix

# Then restart your terminal
```

**"Cannot find file: Memory Monitor.exe"**
```bash
# Solution: Build the main project first
cd "Memory Monitor"
dotnet build --configuration Release
cd ..
```

**"Cannot find file: icon.ico"**
```bash
# Solution 1: Create icon file at Memory Monitor\icon.ico
# Solution 2: Remove icon references from Product.wxs
```

**"The system cannot find the path specified"**
```bash
# Solution: Make sure you're in the solution root directory
# Should see both "Memory Monitor" and "MemoryMonitorSetup" folders
```

### Installation Errors

**"Another version is already installed"**
```bash
# Solution: Uninstall existing version first
# Control Panel > Programs > Uninstall System Monitor
```

**".NET 8.0 Desktop Runtime is required"**
```bash
# Solution: Install .NET 8.0 Desktop Runtime
# Download: https://dotnet.microsoft.com/download/dotnet/8.0/runtime
# Choose: ".NET Desktop Runtime 8.0.x (x64)"
```

**"Installation failed with error 2502/2503"**
```bash
# Solution: Run as administrator
# Right-click MSI > Run as administrator
```

---

## Customization

### Change Version Number
Edit `MemoryMonitorSetup\Product.wxs`:
```xml
<Package Name="System Monitor"
         Version="1.0.0.0"  <!-- Change here -->
```

### Change Installation Directory
Edit `MemoryMonitorSetup\Product.wxs`:
```xml
<Directory Id="INSTALLFOLDER" Name="System Monitor" />
<!-- Change "System Monitor" to your preferred folder name -->
```

### Remove Desktop Shortcut
Edit `MemoryMonitorSetup\Product.wxs`, remove:
```xml
<Shortcut Id="DesktopShortcut" ... />
```

### Add More Files
Edit `MemoryMonitorSetup\Product.wxs`, add to `ProductComponents`:
```xml
<Component Id="YourComponent" Guid="NEW-GUID-HERE">
  <File Id="YourFile"
        Source="path\to\your\file.ext"
        KeyPath="yes" />
</Component>
```

---

## Distribution

### For End Users
Distribute only the `.msi` file:
- `MemoryMonitorSetup.msi` (typically 2-5 MB)
- Users can double-click to install
- No additional files needed

### For IT Departments
Provide:
1. The MSI file
2. Silent install command:
   ```bash
   msiexec /i MemoryMonitorSetup.msi /qn
   ```
3. System requirements:
   - Windows 10/11 (x64)
   - .NET 8.0 Desktop Runtime

### Digitally Sign (Recommended)
```bash
# If you have a code signing certificate:
signtool sign /f certificate.pfx /p password MemoryMonitorSetup.msi
```

---

## Additional Resources

- [WiX Documentation](https://wixtoolset.org/docs/)
- [.NET 8.0 Downloads](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MSI Command Line Options](https://docs.microsoft.com/windows/win32/msi/command-line-options)

---

**DFS - Dad's Fixit Shop**
Making tech accessible, reliable, and fun since 2020
