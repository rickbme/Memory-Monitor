@echo off
REM ============================================================================
REM Memory Monitor Installer Build Script
REM ============================================================================
REM This script builds the Memory Monitor application and creates the MSI installer.
REM 
REM Prerequisites:
REM   - .NET 8 SDK installed
REM   - WiX Toolset v5 (installed via dotnet tool or NuGet)
REM
REM Usage:
REM   build-installer.bat [Release|Debug]
REM
REM Output:
REM   MemoryMonitorSetup\bin\Release\MemoryMonitorSetup.msi
REM ============================================================================

setlocal enabledelayedexpansion

REM Set default configuration
set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Release

echo.
echo ============================================
echo   Memory Monitor Installer Build Script
echo ============================================
echo.
echo Configuration: %CONFIG%
echo.

REM Navigate to solution directory
cd /d "%~dp0.."

REM Step 1: Clean previous builds
echo [1/5] Cleaning previous builds...
if exist "Memory Monitor\bin\%CONFIG%\net8.0-windows\win-x64\publish" (
    rmdir /s /q "Memory Monitor\bin\%CONFIG%\net8.0-windows\win-x64\publish"
)
if exist "MemoryMonitorSetup\bin" (
    rmdir /s /q "MemoryMonitorSetup\bin"
)
if exist "MemoryMonitorSetup\obj" (
    rmdir /s /q "MemoryMonitorSetup\obj"
)
echo       Done.
echo.

REM Step 2: Build the application
echo [2/5] Building Memory Monitor application...
dotnet build "Memory Monitor\Memory Monitor.csproj" -c %CONFIG% -r win-x64
if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo       Done.
echo.

REM Step 3: Publish the application (self-contained)
echo [3/5] Publishing self-contained application...
dotnet publish "Memory Monitor\Memory Monitor.csproj" -c %CONFIG% -r win-x64 --self-contained true -o "Memory Monitor\bin\%CONFIG%\net8.0-windows\win-x64\publish"
if errorlevel 1 (
    echo.
    echo ERROR: Publish failed!
    pause
    exit /b 1
)
echo       Done.
echo.

REM Step 4: Regenerate PublishedFiles.wxs
echo [4/5] Regenerating PublishedFiles.wxs...
cd MemoryMonitorSetup
powershell -ExecutionPolicy Bypass -File "Regenerate-PublishedFiles.ps1" -PublishPath "..\Memory Monitor\bin\%CONFIG%\net8.0-windows\win-x64\publish"
if errorlevel 1 (
    echo.
    echo ERROR: Failed to regenerate PublishedFiles.wxs!
    cd ..
    pause
    exit /b 1
)
cd ..
echo       Done.
echo.

REM Step 5: Build the WiX installer
echo [5/5] Building MSI installer...
dotnet build "MemoryMonitorSetup\MemoryMonitorSetup.wixproj" -c %CONFIG%
if errorlevel 1 (
    echo.
    echo ERROR: Installer build failed!
    echo.
    echo If you see errors about missing files, check:
    echo   1. The publish step completed successfully
    echo   2. PublishedFiles.wxs was regenerated
    echo   3. All referenced ComponentGroups exist in Package.wxs
    echo.
    pause
    exit /b 1
)
echo       Done.
echo.

REM Success!
echo ============================================
echo   BUILD SUCCESSFUL!
echo ============================================
echo.
echo Installer created at:
echo   MemoryMonitorSetup\bin\%CONFIG%\en-us\MemoryMonitorSetup.msi
echo.

REM Open the output folder
if exist "MemoryMonitorSetup\bin\%CONFIG%\en-us" (
    explorer "MemoryMonitorSetup\bin\%CONFIG%\en-us"
) else (
    explorer "MemoryMonitorSetup\bin\%CONFIG%"
)

pause
