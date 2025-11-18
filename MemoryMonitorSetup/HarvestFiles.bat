@echo off
REM This script harvests all files from the publish directory
REM Run this before building the installer if you add new dependencies

heat dir "..\Memory Monitor\bin\Release\net8.0-windows\publish" -cg PublishedFiles -gg -sfrag -srd -dr INSTALLFOLDER -var var.PublishDir -o PublishedFiles.wxs
echo File harvest complete. Include PublishedFiles.wxs in your installer project.
