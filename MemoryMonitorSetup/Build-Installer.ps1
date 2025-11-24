# Build Memory Monitor Installer
# This script builds the application and then creates the MSI installer

Write-Host "Building Memory Monitor Installer..." -ForegroundColor Green
Write-Host ""

$ErrorActionPreference = "Stop"

$SolutionRoot = Split-Path -Parent $PSScriptRoot
$ProjectPath = Join-Path $SolutionRoot "Memory Monitor\Memory Monitor.csproj"
$InstallerPath = Join-Path $SolutionRoot "MemoryMonitorSetup\MemoryMonitorSetup.wixproj"

try {
    # Step 1: Clean previous builds
    Write-Host "Step 1: Cleaning previous builds..." -ForegroundColor Yellow
    dotnet clean $ProjectPath -c Release
    
    # Step 2: Build the application in Release mode
    Write-Host "Step 2: Building Memory Monitor application..." -ForegroundColor Yellow
    dotnet build $ProjectPath -c Release
    
    if ($LASTEXITCODE -ne 0) {
        throw "Application build failed with exit code $LASTEXITCODE"
    }
    
    Write-Host "? Application build successful" -ForegroundColor Green
    Write-Host ""
    
    # Step 3: List files that will be included in installer
    Write-Host "Step 3: Files to be included in installer:" -ForegroundColor Yellow
    $BinPath = Join-Path $SolutionRoot "Memory Monitor\bin\Release\net8.0-windows"
    if (Test-Path $BinPath) {
        Get-ChildItem $BinPath -File | Select-Object Name, @{Name="Size (KB)";Expression={[math]::Round($_.Length/1KB,2)}} | Format-Table
    }
    
    # Step 4: Build the installer
    Write-Host "Step 4: Building WiX installer..." -ForegroundColor Yellow
    dotnet build $InstallerPath -c Release
    
    if ($LASTEXITCODE -ne 0) {
        throw "Installer build failed with exit code $LASTEXITCODE"
    }
    
    Write-Host "? Installer build successful" -ForegroundColor Green
    Write-Host ""
    
    # Step 5: Show output location (try both possible locations)
    $MsiPath = Join-Path $SolutionRoot "MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi"
    $MsiPathAlt = Join-Path $SolutionRoot "MemoryMonitorSetup\bin\Release\en-US\MemoryMonitorSetup.msi"
    
    if (Test-Path $MsiPath) {
        $MsiInfo = Get-Item $MsiPath
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "? BUILD COMPLETE!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "Installer Location: $MsiPath" -ForegroundColor Cyan
        Write-Host "Installer Size: $([math]::Round($MsiInfo.Length/1KB,2)) KB" -ForegroundColor Cyan
        Write-Host "Created: $($MsiInfo.LastWriteTime)" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "To install, run: msiexec /i `"$MsiPath`" /l*v install.log" -ForegroundColor Yellow
    } elseif (Test-Path $MsiPathAlt) {
        $MsiInfo = Get-Item $MsiPathAlt
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "? BUILD COMPLETE!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "Installer Location: $MsiPathAlt" -ForegroundColor Cyan
        Write-Host "Installer Size: $([math]::Round($MsiInfo.Length/1KB,2)) KB" -ForegroundColor Cyan
        Write-Host "Created: $($MsiInfo.LastWriteTime)" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "To install, run: msiexec /i `"$MsiPathAlt`" /l*v install.log" -ForegroundColor Yellow
    } else {
        Write-Warning "MSI file not found in expected locations."
        Write-Host "Searched:" -ForegroundColor Yellow
        Write-Host "  - $MsiPath" -ForegroundColor Yellow
        Write-Host "  - $MsiPathAlt" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "? BUILD FAILED!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "1. Make sure WiX Toolset v4 is installed" -ForegroundColor Yellow
    Write-Host "2. Check that all file paths in Package.wxs exist" -ForegroundColor Yellow
    Write-Host "3. Verify .NET 8 SDK is installed" -ForegroundColor Yellow
    exit 1
}
