# WiX Installer Fix for Memory Monitor

## The Problem
The installer was timing out because it was missing critical .NET 8 runtime files. The original installer only included 6 files, but a .NET 8 Windows Forms application requires many more dependencies.

## The Solution

### 1. Publish the Application First
Before building the installer, you must publish the application to gather all dependencies:

```bash
cd "Memory Monitor"
dotnet publish -c Release -r win-x64 --self-contained false
```

Or for a self-contained deployment (doesn't require .NET runtime on target machine):

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

### 2. What Was Fixed

#### Package.wxs Changes:
- ? Added .NET 8 Desktop Runtime check (Launch Condition)
- ? Changed file sources to use the `publish` folder instead of direct `bin` output
- ? Added Start Menu shortcuts
- ? Added proper Add/Remove Programs integration with icon
- ? Updated manufacturer name and upgrade code
- ? Added MediaTemplate for proper CAB embedding

#### Still Needed:
You have two options to include all files:

**Option A: Manual (Current Approach)**
- Add Component entries for each DLL file in your publish folder
- Labor-intensive but gives full control

**Option B: Automated with Heat (Recommended)**
1. Run the HarvestFiles.bat script after publishing
2. This generates PublishedFiles.wxs with all files
3. Reference it in your Package.wxs

### 3. Recommended: Switch to Self-Contained Deployment

Add this to Memory Monitor.csproj:

```xml
<PropertyGroup>
  <PublishSingleFile>false</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
</PropertyGroup>
```

Then update Package.wxs to remove the .NET runtime check and include ALL files from publish folder.

### 4. Build Process

1. **Publish the app:**
   ```bash
   dotnet publish "Memory Monitor\Memory Monitor.csproj" -c Release
   ```

2. **Build the installer:**
   ```bash
   dotnet build "MemoryMonitorSetup\MemoryMonitorSetup.wixproj" -c Release
   ```

### 5. Testing the Installer

- The MSI will be in: `MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi`
- Install on a clean VM without .NET 8 installed to verify runtime check
- Verify the Start Menu shortcut appears
- Verify the app launches successfully

## Why It Timed Out

The timeout occurred because:
1. Windows Installer was trying to execute the .exe
2. The .exe couldn't find required runtime DLLs
3. The installation service waited for the process to respond
4. Eventually timed out waiting

## Next Steps

Choose your deployment strategy:
- **Framework-dependent**: Smaller installer, requires .NET 8 Desktop Runtime on user's machine
- **Self-contained**: Larger installer (~100MB), includes everything, works on any Windows machine

Let me know which approach you prefer and I can update the configuration accordingly!
