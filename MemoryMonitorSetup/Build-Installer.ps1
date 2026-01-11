# Build Memory Monitor Installer (Self-Contained with .NET Runtime)
# This script builds a self-contained application and creates the MSI installer

Write-Host "Building Memory Monitor Self-Contained Installer..." -ForegroundColor Green
Write-Host ""

$ErrorActionPreference = "Stop"

$SolutionRoot = Split-Path -Parent $PSScriptRoot
$ProjectPath = Join-Path $SolutionRoot "Memory Monitor\Memory Monitor.csproj"
$InstallerPath = Join-Path $SolutionRoot "MemoryMonitorSetup\MemoryMonitorSetup.wixproj"
$GenerateComponentsScript = Join-Path $PSScriptRoot "Generate-Components.ps1"

try {
    # Step 1: Clean previous builds
    Write-Host "Step 1: Cleaning previous builds..." -ForegroundColor Yellow
    dotnet clean $ProjectPath -c Release
    
    # Step 2: Publish the application with .NET runtime included
    Write-Host "Step 2: Publishing self-contained application (this may take a minute)..." -ForegroundColor Yellow
    dotnet publish $ProjectPath -c Release --self-contained true -r win-x64
    
    if ($LASTEXITCODE -ne 0) {
        throw "Application publish failed with exit code $LASTEXITCODE"
    }
    
    Write-Host "? Application published successfully" -ForegroundColor Green
    Write-Host ""
    
    # Step 3: Show publish statistics
    Write-Host "Step 3: Self-contained deployment statistics:" -ForegroundColor Yellow
    $PublishPath = Join-Path $SolutionRoot "Memory Monitor\bin\Release\net8.0-windows\win-x64\publish"
    if (Test-Path $PublishPath) {
        $files = Get-ChildItem $PublishPath -File
        $totalSize = ($files | Measure-Object -Property Length -Sum).Sum
        Write-Host "  Files: $($files.Count)" -ForegroundColor Cyan
        Write-Host "  Total Size: $([math]::Round($totalSize / 1MB, 2)) MB" -ForegroundColor Cyan
        Write-Host "  Note: Includes full .NET 8 runtime - no separate download needed!" -ForegroundColor Green
        Write-Host ""
    }
    
    # Step 4: Generate WiX components from published files
    Write-Host "Step 4: Generating WiX components from published files..." -ForegroundColor Yellow
    & $GenerateComponentsScript
    
    if ($LASTEXITCODE -ne 0) {
        throw "Component generation failed"
    }
    
    # Step 5: Build the installer
    Write-Host "Step 5: Building WiX installer..." -ForegroundColor Yellow
    dotnet build $InstallerPath -c Release
    
    if ($LASTEXITCODE -ne 0) {
        throw "Installer build failed with exit code $LASTEXITCODE"
    }
    
    Write-Host "? Installer build successful" -ForegroundColor Green
    Write-Host ""
    
    # Step 6: Show output location (try both possible locations)
    $MsiPath = Join-Path $SolutionRoot "MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi"
    $MsiPathAlt = Join-Path $SolutionRoot "MemoryMonitorSetup\bin\Release\en-US\MemoryMonitorSetup.msi"
    
    if (Test-Path $MsiPath) {
        $MsiInfo = Get-Item $MsiPath
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "? SELF-CONTAINED BUILD COMPLETE!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "Installer Location: $MsiPath" -ForegroundColor Cyan
        Write-Host "Installer Size: $([math]::Round($MsiInfo.Length/1MB,2)) MB" -ForegroundColor Cyan
        Write-Host "Created: $($MsiInfo.LastWriteTime)" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "? This installer includes the .NET 8 runtime!" -ForegroundColor Green
        Write-Host "? No additional downloads required for end users!" -ForegroundColor Green
        Write-Host ""
        Write-Host "To install, run: msiexec /i `"$MsiPath`" /l*v install.log" -ForegroundColor Yellow
    } elseif (Test-Path $MsiPathAlt) {
        $MsiInfo = Get-Item $MsiPathAlt
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "? SELF-CONTAINED BUILD COMPLETE!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "Installer Location: $MsiPathAlt" -ForegroundColor Cyan
        Write-Host "Installer Size: $([math]::Round($MsiInfo.Length/1MB,2)) MB" -ForegroundColor Cyan
        Write-Host "Created: $($MsiInfo.LastWriteTime)" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "? This installer includes the .NET 8 runtime!" -ForegroundColor Green
        Write-Host "? No additional downloads required for end users!" -ForegroundColor Green
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
    Write-Host "2. Check that all file paths exist" -ForegroundColor Yellow
    Write-Host "3. Verify .NET 8 SDK is installed" -ForegroundColor Yellow
    Write-Host "4. Ensure enough disk space (~150MB for self-contained build)" -ForegroundColor Yellow
    exit 1
}
