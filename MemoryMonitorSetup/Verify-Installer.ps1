# ============================================================================
# Verify Installer Contents
# ============================================================================
# Checks a built MSI against the publish folder it was made from:
#   - the MSI's version matches the published MemoryMonitor.dll
#   - the MSI contains every published file (plus USER_GUIDE.md)
#   - key application, runtime and resource files were published
#
# Usage:
#   .\Verify-Installer.ps1 [-Configuration Release|Debug]
#
# Exits with code 1 if any check fails.
# ============================================================================

param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release"
)

$msiPath = Join-Path $PSScriptRoot "bin\x64\$Configuration\MemoryMonitorSetup.msi"
$publishPath = Join-Path $PSScriptRoot "..\Memory Monitor\bin\$Configuration\net8.0-windows\win-x64\publish"
$failures = 0

function Report([bool]$ok, [string]$message) {
    if ($ok) {
        Write-Host "  [OK]   $message" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] $message" -ForegroundColor Red
        $script:failures++
    }
}

# Runs a query against the MSI database and returns the first column of each row
function Get-MsiColumn([string]$path, [string]$sql) {
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $database = $installer.GetType().InvokeMember("OpenDatabase", "InvokeMethod", $null, $installer, @($path, [int]0))
    $view = $database.GetType().InvokeMember("OpenView", "InvokeMethod", $null, $database, @($sql))
    [void]$view.GetType().InvokeMember("Execute", "InvokeMethod", $null, $view, $null)
    $values = @()
    while ($record = $view.GetType().InvokeMember("Fetch", "InvokeMethod", $null, $view, $null)) {
        $values += $record.GetType().InvokeMember("StringData", "GetProperty", $null, $record, @([int]1))
    }
    [void]$view.GetType().InvokeMember("Close", "InvokeMethod", $null, $view, $null)
    [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($database)
    return ,$values
}

Write-Host ""
Write-Host "Memory Monitor installer verification ($Configuration)" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $msiPath)) {
    Write-Host "MSI not found: $msiPath" -ForegroundColor Red
    Write-Host "Build it first with build-installer.bat or Build-Installer.ps1" -ForegroundColor Yellow
    exit 1
}
if (-not (Test-Path $publishPath)) {
    Write-Host "Publish folder not found: $publishPath" -ForegroundColor Red
    exit 1
}

$msi = Get-Item $msiPath
$publishedFiles = Get-ChildItem -Path $publishPath -Recurse -File
Write-Host ("MSI:     {0} ({1:N1} MB, built {2})" -f $msi.FullName, ($msi.Length / 1MB), $msi.LastWriteTime)
Write-Host ("Publish: {0} files, {1:N1} MB" -f $publishedFiles.Count, (($publishedFiles | Measure-Object Length -Sum).Sum / 1MB))
Write-Host ""

Write-Host "Installer:" -ForegroundColor Yellow
$msiVersion = (Get-MsiColumn $msi.FullName "SELECT Value FROM Property WHERE Property='ProductVersion'")[0]
$appVersion = [System.Reflection.AssemblyName]::GetAssemblyName((Join-Path $publishPath "MemoryMonitor.dll")).Version.ToString()
Report ($msiVersion -eq $appVersion) "Version $msiVersion matches the published app ($appVersion)"

$msiFileCount = (Get-MsiColumn $msi.FullName "SELECT File FROM File").Count
$expectedCount = $publishedFiles.Count + 1   # + USER_GUIDE.md
Report ($msiFileCount -eq $expectedCount) "Contains $msiFileCount files (publish folder + USER_GUIDE.md = $expectedCount)"

Report ($msi.LastWriteTime -ge ($publishedFiles | Measure-Object LastWriteTime -Maximum).Maximum) "MSI is newer than the published files"

Write-Host ""
Write-Host "Published files:" -ForegroundColor Yellow
$criticalFiles = [ordered]@{
    "MemoryMonitor.exe"           = "Main executable"
    "MemoryMonitor.dll"           = "Application code"
    "LibreHardwareMonitorLib.dll" = "Hardware monitoring library"
    "System.Management.dll"       = "WMI access (GPU detection)"
    "coreclr.dll"                 = ".NET 8 runtime"
    "hostfxr.dll"                 = ".NET host"
    "System.Windows.Forms.dll"    = "Windows Forms"
    "Resources\mmguage.ico"       = "Application icon"
    "Resources\dfs_logo_1920.png" = "Intro logo"
    "Resources\dfslogo_round.png" = "Splash logo"
}
foreach ($file in $criticalFiles.Keys) {
    Report (Test-Path (Join-Path $publishPath $file)) "$file - $($criticalFiles[$file])"
}

Write-Host ""
if ($failures -eq 0) {
    Write-Host "All checks passed." -ForegroundColor Green
    Write-Host "Final check: install it on a test PC (ideally one without .NET 8) and launch the app." -ForegroundColor White
    exit 0
} else {
    Write-Host "$failures check(s) failed." -ForegroundColor Red
    exit 1
}
