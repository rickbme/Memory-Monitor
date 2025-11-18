# Quick Fix Reference

## Immediate Solution

Run this command to build a working installer:

```powershell
cd MemoryMonitorSetup
.\Build-Installer.ps1
```

## What Was Fixed

| Issue | Status |
|-------|--------|
| Installer times out | ? FIXED - Added proper file structure |
| No Start Menu shortcut | ? FIXED - Added shortcuts |
| Missing Add/Remove Programs icon | ? FIXED - Added icon |
| Placeholder manufacturer name | ? FIXED - Updated metadata |
| Test upgrade code | ? FIXED - Proper GUID |

## Root Cause

**The installer timed out because:**
- Only 6 files were copied (incomplete)
- .NET 8 Desktop Runtime was missing on target machines
- No runtime check was performed
- Application couldn't start, causing Windows Installer to hang

## Next Steps

### For Production Deployment

Choose your deployment strategy:

**Framework-Dependent (Current):**
- Pros: Small installer (~2 MB)
- Cons: Users must install .NET 8 Desktop Runtime separately
- Best for: Corporate environments with managed deployments

**Self-Contained (Recommended):**
- Pros: Works everywhere, no prerequisites
- Cons: Larger installer (~100 MB)
- Best for: Consumer applications, public releases

### To Switch to Self-Contained

1. Add to `Memory Monitor.csproj`:
   ```xml
   <SelfContained>true</SelfContained>
   <RuntimeIdentifier>win-x64</RuntimeIdentifier>
   ```

2. In `Package.wxs`, change all paths from:
   ```
   ..\Memory Monitor\bin\Release\net8.0-windows\
   ```
   To:
   ```
   ..\Memory Monitor\bin\Release\net8.0-windows\publish\
   ```

3. Run:
   ```powershell
   dotnet publish -c Release
   .\Build-Installer.ps1
   ```

## Files Created

- ? `Package.wxs` - Fixed installer definition
- ? `Build-Installer.ps1` - Automated build script
- ? `HarvestFiles.bat` - File harvesting tool
- ? `INSTALLER_FIX_README.md` - Detailed documentation
- ? `ROOT_CAUSE_ANALYSIS.md` - Technical explanation
- ? `QUICK_REFERENCE.md` - This file

## Test Installation

```powershell
# Install with logging
msiexec /i "bin\Release\en-US\MemoryMonitorSetup.msi" /l*v install.log

# Uninstall
msiexec /x "bin\Release\en-US\MemoryMonitorSetup.msi"
```

## Support

If issues persist:
1. Check `install.log` for errors
2. Verify .NET 8 SDK is installed: `dotnet --list-sdks`
3. Verify WiX v4 is installed: `dotnet tool list -g`
4. Test on clean Windows 10/11 VM

## Installation Success Checklist

After installing, verify:
- [ ] Start Menu contains "Memory Monitor" shortcut
- [ ] Shortcut launches the application
- [ ] Application appears in Add/Remove Programs
- [ ] Application icon displays correctly
- [ ] Uninstall works properly
