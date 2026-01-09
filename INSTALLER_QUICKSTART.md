# Quick Start: Building the Installer

##  Super Quick (One Command)

Just double-click:
```
Build-Installer.bat
```

Or from command line:
```cmd
Build-Installer.bat
```

**That's it!** The installer will be created at:
```
MemoryMonitorSetup\bin\Release\en-US\MemoryMonitorSetup.msi
```

---

## What You Need

Before running, make sure you have:

- [x] **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- [x] **WiX Toolset v4** - Auto-installs if missing, or run: `dotnet tool install --global wix`

---

##  Usage Options

### Standard Build
```cmd
Build-Installer.bat
```
? Builds app + creates installer

### Clean Previous Builds
```cmd
Build-Installer.bat clean
```
? Removes old build files

### Full Rebuild
```cmd
Build-Installer.bat rebuild
```
? Clean + Build from scratch

### Build & Test Install
```cmd
Build-Installer.bat test
```
? Build + Option to install immediately

---

## ?? What Gets Built

### Application (Step 1)
```
Memory Monitor\bin\Release\net8.0-windows\
??? Memory Monitor.exe
??? Memory Monitor.dll
??? [dependencies]
```

### Installer (Step 3)
```
MemoryMonitorSetup\bin\Release\en-US\
??? MemoryMonitorSetup.msi  ? Your installer!
```

---

## ? Success Looks Like This

```
============================================================================
 BUILD COMPLETE
============================================================================

[SUCCESS] Installer created successfully!

Location: MemoryMonitorSetup\bin\Release\en-US\MemoryMonitorSetup.msi
Size: 2 MB
Created: 12/15/2024 10:30 AM

To install:
  msiexec /i "MemoryMonitorSetup.msi"
```

---

## ?? Installing the MSI

### Interactive Install
```cmd
msiexec /i MemoryMonitorSetup.msi
```

### Silent Install
```cmd
msiexec /i MemoryMonitorSetup.msi /quiet
```

### Install with Log
```cmd
msiexec /i MemoryMonitorSetup.msi /l*v install.log
```

---

## ??? Uninstalling

### Interactive
```cmd
msiexec /x MemoryMonitorSetup.msi
```

### Silent
```cmd
msiexec /x MemoryMonitorSetup.msi /quiet
```

---

## ? If Something Goes Wrong

### Missing .NET SDK
```
[ERROR] .NET SDK not found!
```
**Fix**: Install from https://dotnet.microsoft.com/download

### Missing WiX
```
[WARNING] WiX Toolset not found!
```
**Fix**: Script auto-installs, or manually: `dotnet tool install --global wix`

### Build Failed
```
[ERROR] Application build failed!
```
**Fix**: 
1. Open solution in Visual Studio
2. Check for errors
3. Fix and try again

---

##  More Help

- **Detailed Guide**: `BUILD_INSTALLER_GUIDE.md`
- **Installer Docs**: `MemoryMonitorSetup\INSTALLER_FIX_README.md`
- **Quick Reference**: `MemoryMonitorSetup\QUICK_REFERENCE.md`

---

##  That's It!

Three steps to a working installer:

1. **Run**: `Build-Installer.bat`
2. **Wait**: ~30 seconds for build
3. **Done**: Find MSI in `MemoryMonitorSetup\bin\Release\en-US\`

Simple as that!
