@echo off
REM ============================================================================
REM Memory Monitor Installer Build Script
REM ============================================================================
REM Double-click friendly wrapper around Build-Installer.ps1, which does the work:
REM publish the app (self-contained, win-x64), regenerate PublishedFiles.wxs and
REM build the MSI. See INSTALLER-README.md.
REM
REM Usage:
REM   build-installer.bat [Release|Debug]
REM
REM Output:
REM   MemoryMonitorSetup\bin\x64\Release\MemoryMonitorSetup.msi
REM ============================================================================

setlocal

set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Release

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Build-Installer.ps1" -Configuration %CONFIG%
set RESULT=%ERRORLEVEL%

if %RESULT%==0 (
    explorer "%~dp0bin\x64\%CONFIG%"
)

pause
exit /b %RESULT%
