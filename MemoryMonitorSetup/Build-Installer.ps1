# ============================================================================
# Build Memory Monitor Installer (Self-Contained with .NET Runtime)
# ============================================================================
# PowerShell version of build-installer.bat (same steps, no pauses, suitable for scripts/CI):
#   1. Clean previous publish and installer output
#   2. Publish the app self-contained for win-x64
#   3. Regenerate PublishedFiles.wxs from the publish folder
#   4. Build the MSI
#
# Usage:
#   .\Build-Installer.ps1 [-Configuration Release|Debug]
#
# Output:
#   MemoryMonitorSetup\bin\x64\<Configuration>\MemoryMonitorSetup.msi
# ============================================================================

param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$SolutionRoot = Split-Path -Parent $PSScriptRoot
$ProjectPath = Join-Path $SolutionRoot "Memory Monitor\MemoryMonitor.csproj"
$InstallerProject = Join-Path $PSScriptRoot "MemoryMonitorSetup.wixproj"
$PublishPath = Join-Path $SolutionRoot "Memory Monitor\bin\$Configuration\net8.0-windows\win-x64\publish"
$MsiPath = Join-Path $PSScriptRoot "bin\x64\$Configuration\MemoryMonitorSetup.msi"

function Invoke-Step([string]$Description, [scriptblock]$Action) {
    Write-Host $Description -ForegroundColor Yellow
    & $Action
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE"
    }
    Write-Host "      Done." -ForegroundColor Green
    Write-Host ""
}

try {
    Write-Host ""
    Write-Host "Building Memory Monitor installer ($Configuration)..." -ForegroundColor Green
    Write-Host ""

    Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
    foreach ($path in @($PublishPath, (Join-Path $PSScriptRoot "bin"), (Join-Path $PSScriptRoot "obj"))) {
        if (Test-Path $path) { Remove-Item -Recurse -Force $path }
    }
    Write-Host "      Done." -ForegroundColor Green
    Write-Host ""

    Invoke-Step "[2/4] Publishing self-contained application..." {
        dotnet publish $ProjectPath -c $Configuration -r win-x64 --self-contained true -o $PublishPath
    }

    $files = Get-ChildItem $PublishPath -Recurse -File
    Write-Host ("      {0} files, {1:N1} MB (includes the .NET 8 runtime)" -f $files.Count, (($files | Measure-Object Length -Sum).Sum / 1MB)) -ForegroundColor Cyan
    Write-Host ""

    Invoke-Step "[3/4] Regenerating PublishedFiles.wxs..." {
        powershell -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot "Regenerate-PublishedFiles.ps1") `
            -PublishPath "..\Memory Monitor\bin\$Configuration\net8.0-windows\win-x64\publish"
    }

    Invoke-Step "[4/4] Building MSI installer..." {
        dotnet build $InstallerProject -c $Configuration
    }

    if (-not (Test-Path $MsiPath)) {
        throw "Build reported success but the MSI was not found at $MsiPath"
    }

    $msi = Get-Item $MsiPath
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "  BUILD SUCCESSFUL" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "Installer: $MsiPath" -ForegroundColor Cyan
    Write-Host ("Size:      {0:N1} MB" -f ($msi.Length / 1MB)) -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To install with a log: msiexec /i `"$MsiPath`" /l*v install.log" -ForegroundColor Yellow
}
catch {
    Write-Host ""
    Write-Host "BUILD FAILED: $_" -ForegroundColor Red
    exit 1
}
