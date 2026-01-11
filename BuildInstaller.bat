@echo off
REM Build script for Memory Monitor Installer
REM Requires WiX Toolset v5.0 or later

echo ========================================
echo Building Memory Monitor Installer
echo ========================================
echo.

REM Check if WiX is installed
where wix >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: WiX Toolset not found!
    echo.
    echo Please install WiX Toolset v5.0:
    echo   dotnet tool install --global wix
    echo.
    echo Or download from: https://wixtoolset.org/
    pause
    exit /b 1
)

echo [1/3] Building Memory Monitor application...
cd "Memory Monitor"
dotnet build --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build application
    pause
    exit /b 1
)
cd ..

echo.
echo [2/3] Building WiX installer project...
cd MemoryMonitorSetup
dotnet build --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build installer
    pause
    exit /b 1
)
cd ..

echo.
echo [3/3] Locating installer package...
echo.
echo ========================================
echo Build Complete!
echo ========================================
echo.
echo Installer location:
dir /b /s MemoryMonitorSetup\bin\Release\*.msi
echo.
echo Installation package ready for distribution.
echo.
pause
