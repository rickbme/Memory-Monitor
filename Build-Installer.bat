@echo off
REM ============================================================================
REM Memory Monitor Installer Builder
REM ============================================================================
REM This batch file builds the Memory Monitor application and creates the MSI installer
REM
REM Prerequisites:
REM   - .NET 8 SDK installed
REM   - WiX Toolset v4 installed (dotnet tool install --global wix)
REM   - Visual Studio 2022 (or Build Tools) recommended
REM
REM Usage:
REM   Build-Installer.bat                  - Build installer
REM   Build-Installer.bat clean            - Clean previous builds
REM   Build-Installer.bat rebuild          - Clean then build
REM   Build-Installer.bat test             - Build and test install
REM ============================================================================

setlocal EnableDelayedExpansion

REM Set console colors
color 0A

echo.
echo ============================================================================
echo  Memory Monitor Installer Builder
echo ============================================================================
echo.

REM Set paths
set "SOLUTION_ROOT=%~dp0"
set "PROJECT_PATH=%SOLUTION_ROOT%Memory Monitor\Memory Monitor.csproj"
set "INSTALLER_PROJECT=%SOLUTION_ROOT%MemoryMonitorSetup\MemoryMonitorSetup.wixproj"
set "BIN_PATH=%SOLUTION_ROOT%Memory Monitor\bin\Release\net8.0-windows"
set "MSI_PATH=%SOLUTION_ROOT%MemoryMonitorSetup\bin\Release\en-US\MemoryMonitorSetup.msi"

REM Check command line arguments
set "BUILD_MODE=build"
if /I "%1"=="clean" set "BUILD_MODE=clean"
if /I "%1"=="rebuild" set "BUILD_MODE=rebuild"
if /I "%1"=="test" set "BUILD_MODE=test"

REM ============================================================================
REM Step 0: Verify Prerequisites
REM ============================================================================
echo [Step 0] Verifying prerequisites...
echo.

REM Check for .NET SDK
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK not found!
    echo Please install .NET 8 SDK from: https://dotnet.microsoft.com/download
    goto :error
)
echo [OK] .NET SDK found: 
dotnet --version

REM Check for WiX
dotnet tool list -g | findstr "wix" >nul 2>&1
if errorlevel 1 (
    echo [WARNING] WiX Toolset not found!
    echo Installing WiX Toolset...
    dotnet tool install --global wix
    if errorlevel 1 (
        echo [ERROR] Failed to install WiX Toolset
        goto :error
    )
)
echo [OK] WiX Toolset found

REM Check if project files exist
if not exist "%PROJECT_PATH%" (
    echo [ERROR] Project file not found: %PROJECT_PATH%
    goto :error
)
if not exist "%INSTALLER_PROJECT%" (
    echo [ERROR] Installer project not found: %INSTALLER_PROJECT%
    goto :error
)
echo [OK] Project files found
echo.

REM ============================================================================
REM Handle Clean Mode
REM ============================================================================
if /I "%BUILD_MODE%"=="clean" (
    echo [Clean] Removing previous builds...
    dotnet clean "%PROJECT_PATH%" -c Release -v quiet
    dotnet clean "%INSTALLER_PROJECT%" -c Release -v quiet
    
    if exist "%BIN_PATH%" (
        echo [Clean] Removing bin folder...
        rmdir /s /q "%BIN_PATH%" 2>nul
    )
    
    if exist "%SOLUTION_ROOT%MemoryMonitorSetup\bin" (
        echo [Clean] Removing installer bin folder...
        rmdir /s /q "%SOLUTION_ROOT%MemoryMonitorSetup\bin" 2>nul
    )
    
    echo.
    echo [SUCCESS] Clean complete!
    goto :end
)

REM ============================================================================
REM Handle Rebuild Mode
REM ============================================================================
if /I "%BUILD_MODE%"=="rebuild" (
    echo [Rebuild] Cleaning previous builds...
    dotnet clean "%PROJECT_PATH%" -c Release -v quiet
    dotnet clean "%INSTALLER_PROJECT%" -c Release -v quiet
    echo.
)

REM ============================================================================
REM Step 1: Build Application
REM ============================================================================
echo [Step 1] Building Memory Monitor application...
echo.

dotnet build "%PROJECT_PATH%" -c Release -v minimal
if errorlevel 1 (
    echo.
    echo [ERROR] Application build failed!
    goto :error
)

echo.
echo [OK] Application build successful
echo.

REM ============================================================================
REM Step 2: Verify Build Output
REM ============================================================================
echo [Step 2] Verifying build output...
echo.

if not exist "%BIN_PATH%\Memory Monitor.exe" (
    echo [ERROR] Main executable not found: %BIN_PATH%\Memory Monitor.exe
    goto :error
)

echo Files to be included in installer:
echo.
dir /b "%BIN_PATH%\*.exe" 2>nul
dir /b "%BIN_PATH%\*.dll" 2>nul
dir /b "%BIN_PATH%\*.json" 2>nul
echo.
echo [OK] Build output verified
echo.

REM ============================================================================
REM Step 3: Build Installer
REM ============================================================================
echo [Step 3] Building WiX installer...
echo.

dotnet build "%INSTALLER_PROJECT%" -c Release -v minimal
if errorlevel 1 (
    echo.
    echo [ERROR] Installer build failed!
    echo.
    echo Common issues:
    echo   1. Make sure all files listed in Package.wxs exist in the bin folder
    echo   2. Check for syntax errors in Package.wxs
    echo   3. Verify WiX Toolset is properly installed
    goto :error
)

echo.
echo [OK] Installer build successful
echo.

REM ============================================================================
REM Step 4: Display Results
REM ============================================================================
echo ============================================================================
echo  BUILD COMPLETE
echo ============================================================================
echo.

if exist "%MSI_PATH%" (
    echo [SUCCESS] Installer created successfully!
    echo.
    echo Location: %MSI_PATH%
    
    REM Get file size
    for %%A in ("%MSI_PATH%") do set "FILE_SIZE=%%~zA"
    set /a "FILE_SIZE_MB=!FILE_SIZE! / 1048576"
    echo Size: !FILE_SIZE_MB! MB
    
    REM Get timestamp
    for %%A in ("%MSI_PATH%") do echo Created: %%~tA
    echo.
    echo ============================================================================
    echo.
    echo To install:
    echo   msiexec /i "%MSI_PATH%"
    echo.
    echo To install with logging:
    echo   msiexec /i "%MSI_PATH%" /l*v install.log
    echo.
    echo To uninstall:
    echo   msiexec /x "%MSI_PATH%"
    echo.
    
    REM Test mode - offer to install
    if /I "%BUILD_MODE%"=="test" (
        echo ============================================================================
        echo.
        set /p "INSTALL_NOW=Install now? (Y/N): "
        if /I "!INSTALL_NOW!"=="Y" (
            echo.
            echo Installing Memory Monitor...
            msiexec /i "%MSI_PATH%" /l*v install.log
            if errorlevel 1 (
                echo [ERROR] Installation failed. Check install.log for details.
            ) else (
                echo [OK] Installation complete!
            )
        )
    )
) else (
    echo [WARNING] MSI file not found at expected location:
    echo %MSI_PATH%
    echo.
    echo The build may have completed but the output location is different.
    echo Check: %SOLUTION_ROOT%MemoryMonitorSetup\bin\
    goto :error
)

goto :end

:error
echo.
echo ============================================================================
echo  BUILD FAILED
echo ============================================================================
echo.
echo Please check the error messages above and try again.
echo.
echo For help, see:
echo   - MemoryMonitorSetup\INSTALLER_FIX_README.md
echo   - MemoryMonitorSetup\QUICK_REFERENCE.md
echo.
exit /b 1

:end
echo ============================================================================
echo.
pause
exit /b 0
