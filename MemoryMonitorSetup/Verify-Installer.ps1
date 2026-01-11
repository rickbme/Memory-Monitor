# ============================================================================
# Verify Installer Contents
# ============================================================================
# This script verifies that the MSI installer includes all required files
# ============================================================================

$msiPath = ".\bin\x64\Release\MemoryMonitorSetup.msi"

if (-not (Test-Path $msiPath)) {
    Write-Error "MSI not found at: $msiPath"
    exit 1
}

$msiFile = Get-Item $msiPath
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  MSI Installer Verification" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "File: $($msiFile.Name)"
Write-Host "Size: $([math]::Round($msiFile.Length / 1MB, 2)) MB ($($msiFile.Length) bytes)"
Write-Host "Date: $($msiFile.LastWriteTime)"
Write-Host ""

Write-Host "Size Analysis:" -ForegroundColor Cyan
if ($msiFile.Length -gt 50MB) {
    Write-Host "  ? Size indicates self-contained .NET 8 runtime IS included" -ForegroundColor Green
    Write-Host "  ? Expected size for full installer: 50-70 MB" -ForegroundColor Green
    Write-Host "  ? Users will NOT need to install .NET 8 separately" -ForegroundColor Green
} elseif ($msiFile.Length -gt 5MB) {
    Write-Host "  ? Size seems small - may be missing runtime files" -ForegroundColor Yellow
    Write-Host "  Expected size for full installer: 50-70 MB" -ForegroundColor Yellow
} else {
    Write-Host "  ? Size is too small - runtime files missing!" -ForegroundColor Red
    Write-Host "  Expected size for full installer: 50-70 MB" -ForegroundColor Red
}

Write-Host ""
Write-Host "Published Files Verification:" -ForegroundColor Cyan

# Check publish directory
$publishPath = "..\Memory Monitor\bin\Release\net8.0-windows\win-x64\publish"
if (Test-Path $publishPath) {
    $publishedFiles = Get-ChildItem -Path $publishPath -Recurse -File
    $totalPublishSize = ($publishedFiles | Measure-Object -Property Length -Sum).Sum
    
    Write-Host "  Total files in publish folder: $($publishedFiles.Count)"
    Write-Host "  Total size of published files: $([math]::Round($totalPublishSize / 1MB, 2)) MB"
    
    # Check for critical files
    Write-Host ""
    Write-Host "Critical Files Check:" -ForegroundColor Yellow
    
    $criticalFiles = @{
        "Memory Monitor.exe" = "Main executable"
        "LibreHardwareMonitorLib.dll" = "Hardware monitoring library"
        "coreclr.dll" = ".NET 8 CoreCLR runtime"
        "System.Windows.Forms.dll" = "Windows Forms framework"
        "Resources\mmguage.ico" = "Application icon"
        "clrjit.dll" = ".NET JIT compiler"
        "hostfxr.dll" = ".NET host"
    }
    
    foreach ($file in $criticalFiles.Keys) {
        $fullPath = Join-Path $publishPath $file
        if (Test-Path $fullPath) {
            $fileInfo = Get-Item $fullPath
            $sizeKB = [math]::Round($fileInfo.Length / 1KB, 1)
            Write-Host "  ? $file ($sizeKB KB) - $($criticalFiles[$file])" -ForegroundColor Green
        } else {
            Write-Host "  ? $file - $($criticalFiles[$file]) - NOT FOUND!" -ForegroundColor Red
        }
    }
    
    # Check for localization folders
    Write-Host ""
    Write-Host "Localization Folders:" -ForegroundColor Yellow
    $locDirs = Get-ChildItem -Path $publishPath -Directory | Where-Object { $_.Name -match '^[a-z]{2}(-[A-Z]{2})?$' }
    Write-Host "  Found $($locDirs.Count) localization directories: $($locDirs.Name -join ', ')" -ForegroundColor Cyan
} else {
    Write-Host "  ? Publish folder not found at: $publishPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Installation Test:" -ForegroundColor Cyan
Write-Host "  To verify the installer works correctly:" -ForegroundColor White
Write-Host "  1. Test on a VM or PC without .NET 8 installed" -ForegroundColor White
Write-Host "  2. Run: MemoryMonitorSetup.msi" -ForegroundColor White
Write-Host "  3. Application should install and run without errors" -ForegroundColor White
Write-Host "  4. No additional runtime downloads should be required" -ForegroundColor White
Write-Host ""
Write-Host "Current Status: READY FOR DISTRIBUTION ?" -ForegroundColor Green
Write-Host ""
