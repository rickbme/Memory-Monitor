# Memory Monitor Windows Installer

How the MSI installer for the Windows version is built, what it installs, and how to release a new
version. The installer is built with [WiX Toolset v5](https://wixtoolset.org/), which is restored
automatically from NuGet. There is nothing extra to install.

The Linux version has its own packaging; see [MemoryMonitor.Linux/README.md](../MemoryMonitor.Linux/README.md).

---

## Building the installer

### Prerequisites
- **Windows 10/11 x64**
- **.NET 8 SDK** - [download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **LibreHardwareMonitor source** checked out next to this repository
  (`..\LibreHardwareMonitor\LibreHardwareMonitorLib`). The app references it as a project.

### One step

Double-click `MemoryMonitorSetup\build-installer.bat`, or from a command prompt:

```batch
MemoryMonitorSetup\build-installer.bat           & REM Release (default)
MemoryMonitorSetup\build-installer.bat Debug
```

From PowerShell (no pause at the end, handy for scripts):

```powershell
.\MemoryMonitorSetup\Build-Installer.ps1 [-Configuration Release|Debug]
```

Both run the same four steps:

1. **Clean** the previous publish folder and installer output
2. **Publish** the app self-contained for win-x64, to
   `Memory Monitor\bin\Release\net8.0-windows\win-x64\publish`
3. **Regenerate `PublishedFiles.wxs`**, the list of every published file
4. **Build the MSI**

### Output

```
MemoryMonitorSetup\bin\x64\Release\MemoryMonitorSetup.msi     (about 63 MB)
```

### Checking the result

```powershell
.\MemoryMonitorSetup\Verify-Installer.ps1
```

This checks that:
- the MSI's version matches the published app;
- the MSI contains every published file plus `USER_GUIDE.md`;
- the main app, runtime and resource files are present.

It exits with an error if anything is wrong. After that, install the MSI on a test PC and launch the app.

---

## What the installer does

| | |
|---|---|
| **Install folder** | `C:\Program Files\Memory Monitor\` (64-bit; can be changed in the installer) |
| **Contents** | The app and the full .NET 8 runtime (self-contained, so users don't need .NET), plus `USER_GUIDE.md` |
| **Shortcuts** | Start Menu: *Memory Monitor* and *Memory Monitor User Guide*. Desktop: *Memory Monitor* |
| **Add/Remove Programs** | *Memory Monitor* by DFS, with the app icon and a link to the GitHub page |
| **Scope** | Per machine (all users). Installing asks for administrator approval (UAC) |
| **Dialogs** | Welcome → install folder → install → finish. No license page |
| **Repair / remove** | Available from Add/Remove Programs or by running the MSI again |

The app itself asks for administrator rights each time it starts (via `app.manifest`), because the
hardware sensors need them.

### Upgrades

- **Any newer version** replaces the installed one automatically. The old version is removed first, then
  the new one is installed, so there's always exactly one entry in Add/Remove Programs.
- **The same version** (a rebuilt installer) also replaces the installed copy, instead of adding a
  second entry.
- **Older versions** won't install over a newer one. The installer shows a message instead.
- Upgrade detection uses the `UpgradeCode` in `Package.wxs`. **Never change it**, or new versions will
  install alongside old ones instead of replacing them.
- User settings (display mode, FPS mode, selected devices) are stored per user, not in the install
  folder, so they survive upgrades.

> **Versions before 2.4.5** were packaged as 32-bit and installed to `C:\Program Files (x86)\Memory Monitor`.
> Upgrading to 2.4.5 or later removes that copy and installs to `C:\Program Files\Memory Monitor`.
> A very early build (shown as version **1.0.0.0**) used a different UpgradeCode. If it's still listed in
> Add/Remove Programs, uninstall it there; no newer installer can remove it.

---

## Releasing a new version

The version is set in **one place**: `Memory Monitor\MemoryMonitor.csproj`.

```xml
<Version>2.4.5</Version>
<AssemblyVersion>2.4.5.0</AssemblyVersion>
<FileVersion>2.4.5.0</FileVersion>
```

Update all three, then build. The installer reads its version (and the "successfully installed"
message) from the published `MemoryMonitor.dll`, so `Package.wxs` doesn't need editing. The build log
shows the version it used: `Building Memory Monitor Installer v2.4.5.0 (x64)`.

Release checklist:
1. Update the versions in `MemoryMonitor.csproj` (and `<Version>` in
   `MemoryMonitor.Linux\MemoryMonitor.Linux.csproj` if the Linux version is released too)
2. Add the release to `Memory Monitor\CHANGELOG.md` and the version history in `README.md`
3. Run `build-installer.bat`, then `Verify-Installer.ps1`
4. Test an upgrade install over the previous release
5. Attach `MemoryMonitorSetup.msi` to the GitHub release

---

## Files in this folder

```
MemoryMonitorSetup/
├── MemoryMonitorSetup.wixproj      # WiX project: x64 platform, reads the version from the published app
├── Package.wxs                     # Installer definition: upgrade rules, folders, shortcuts, UI
├── PublishedFiles.wxs              # GENERATED list of every published file - don't edit by hand
├── NoLicense.rtf                   # Placeholder for WiX's license page (the page isn't shown)
├── build-installer.bat             # Double-click wrapper for Build-Installer.ps1
├── Build-Installer.ps1             # Publish + regenerate file list + build MSI
├── Regenerate-PublishedFiles.ps1   # Rebuilds PublishedFiles.wxs from the publish folder
├── Verify-Installer.ps1            # Checks a built MSI against the publish folder
└── INSTALLER-README.md             # This file
```

### How `PublishedFiles.wxs` works

A self-contained publish has about 480 files, so they aren't listed by hand. `Regenerate-PublishedFiles.ps1`
scans the publish folder and writes:

- `PublishedFileComponents` - every file in the install folder itself;
- one group per subfolder (`ResourcesComponents`, `deComponents`, `zh_HansComponents`, ...);
- `PublishedSubdirectoryComponents` - a group that references all of the subfolder groups.

`Package.wxs` only references `PublishedFileComponents` and `PublishedSubdirectoryComponents`, so new or
removed files and folders need no manual changes. Just rebuild.

Each file's component GUID is derived from its path, so it stays the same from build to build, which
Windows Installer requires for clean upgrades. A runtime file whose name includes a version number
(e.g. `mscordaccore_amd64_amd64_8.0.x.dll`) gets a new GUID when the .NET runtime updates, which is correct.
The script supports one level of subfolders and stops with an error if a publish ever contains deeper ones.

---

## Building manually

The scripts do exactly this, from the repository root:

```powershell
dotnet publish "Memory Monitor\MemoryMonitor.csproj" -c Release -r win-x64 --self-contained true `
    -o "Memory Monitor\bin\Release\net8.0-windows\win-x64\publish"
powershell -ExecutionPolicy Bypass -File MemoryMonitorSetup\Regenerate-PublishedFiles.ps1
dotnet build MemoryMonitorSetup\MemoryMonitorSetup.wixproj -c Release
```

In Visual Studio, publish the **Memory Monitor** project to that folder first, run
`Regenerate-PublishedFiles.ps1`, then build **MemoryMonitorSetup**.

---

## Troubleshooting

### "Publish directory ... not found" or "MemoryMonitor.dll not found"
The installer is built from the publish folder. Use `build-installer.bat`, which publishes first.

### "PublishedFiles.wxs not found"
Run `Regenerate-PublishedFiles.ps1` (the build scripts do this automatically).

### The build fails with errors about missing files
`PublishedFiles.wxs` is out of date with the publish folder, e.g. after a .NET runtime update renamed a
file. Rebuild with `build-installer.bat`, which regenerates it.

### WiX can't be found
It's restored from NuGet on build. If that fails, check your NuGet sources and run
`dotnet restore MemoryMonitorSetup\MemoryMonitorSetup.wixproj`.

### Installing: troubleshoot with a log

```batch
msiexec /i MemoryMonitorSetup.msi /l*v install.log
```

Search the log for `Return value 3` to find the step that failed.

### Silent install / uninstall

```batch
msiexec /i MemoryMonitorSetup.msi /qn
msiexec /x MemoryMonitorSetup.msi /qn
```

Both need an elevated command prompt.

---

## Support

Open an issue at https://github.com/rickbme/Memory-Monitor/issues with the build or install log, your
Windows version, and whether it was a fresh install or an upgrade.
