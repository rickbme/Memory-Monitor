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
echo [1/4] Cleaning previous builds...
if exist "Memory Monitor\bin\%CONFIG%\net8.0-windows\win-x64\publish" (
    rmdir /s /q "Memory Monitor\bin\%CONFIG%\net8.0-windows\win-x64\publish"
)
echo       Done.
echo.

REM Step 2: Build the application
echo [2/4] Building Memory Monitor application...
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
echo [3/4] Publishing self-contained application...
dotnet publish "Memory Monitor\Memory Monitor.csproj" -c %CONFIG% -r win-x64 --self-contained true -o "Memory Monitor\bin\%CONFIG%\net8.0-windows\win-x64\publish"
if errorlevel 1 (
    echo.
    echo ERROR: Publish failed!
    pause
    exit /b 1
)
echo       Done.
echo.

REM Step 4: Build the WiX installer
echo [4/4] Building MSI installer...
dotnet build "MemoryMonitorSetup\MemoryMonitorSetup.wixproj" -c %CONFIG%
if errorlevel 1 (
    echo.
    echo ERROR: Installer build failed!
    echo.
    echo If you see errors about missing files, make sure:
    echo   1. The publish step completed successfully
    echo   2. PublishedFiles.wxs matches the published files
    echo.
    echo You may need to regenerate PublishedFiles.wxs if files changed.
    echo See INSTALLER-README.md for instructions.
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
echo   MemoryMonitorSetup\bin\%CONFIG%\MemoryMonitorSetup.msi
echo.

REM Open the output folder
explorer "MemoryMonitorSetup\bin\%CONFIG%"

pause
