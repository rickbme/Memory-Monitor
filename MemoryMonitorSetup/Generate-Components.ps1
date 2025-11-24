# Generate WiX Components for Self-Contained Deployment
# This script scans the publish folder and generates a WiX fragment with all files

param(
    [string]$PublishPath = "..\Memory Monitor\bin\Release\net8.0-windows\win-x64\publish",
    [string]$OutputFile = "PublishedFiles.wxs"
)

$ErrorActionPreference = "Stop"

Write-Host "Generating WiX components from published files..." -ForegroundColor Green
Write-Host ""

# Verify publish path exists
$fullPublishPath = Join-Path $PSScriptRoot $PublishPath
if (-not (Test-Path $fullPublishPath)) {
    Write-Error "Publish path not found: $fullPublishPath"
    Write-Host "Please run: dotnet publish -c Release --self-contained true -r win-x64" -ForegroundColor Yellow
    exit 1
}

# Get all files
$files = Get-ChildItem $fullPublishPath -File
Write-Host "Found $($files.Count) files to include" -ForegroundColor Cyan
Write-Host "Total size: $([math]::Round(($files | Measure-Object -Property Length -Sum).Sum / 1MB, 2)) MB" -ForegroundColor Cyan
Write-Host ""

# Start building the WiX XML
$xml = @"
<?xml version="1.0" encoding="utf-8"?>
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Fragment>
    <ComponentGroup Id="PublishedFileComponents" Directory="INSTALLFOLDER">
"@

# Generate a component for each file
$componentIndex = 1
foreach ($file in $files) {
    # Generate a GUID for each component (deterministic based on filename)
    $guidBytes = [System.Text.Encoding]::UTF8.GetBytes("MemoryMonitor_$($file.Name)_Component")
    $hash = [System.Security.Cryptography.MD5]::Create().ComputeHash($guidBytes)
    $guid = [System.Guid]::new($hash).ToString().ToUpper()
    
    $componentId = "Comp_$componentIndex"
    $fileId = "File_$componentIndex"
    $fileName = $file.Name
    
    # Escape special characters in filename for XML
    $fileNameEscaped = [System.Security.SecurityElement]::Escape($fileName)
    
    # The main executable should be the KeyPath
    if ($fileName -eq "Memory Monitor.exe") {
        $xml += @"

      <!-- Main executable -->
      <Component Id="$componentId" Guid="$guid">
        <File Id="$fileId" 
              Name="$fileNameEscaped"
              Source="$PublishPath\$fileNameEscaped" 
              KeyPath="yes" />
      </Component>
"@
    } else {
        $xml += @"

      <Component Id="$componentId" Guid="$guid">
        <File Id="$fileId" 
              Name="$fileNameEscaped"
              Source="$PublishPath\$fileNameEscaped" />
      </Component>
"@
    }
    
    $componentIndex++
}

# Close the XML
$xml += @"

    </ComponentGroup>
  </Fragment>
</Wix>
"@

# Write to file
$outputPath = Join-Path $PSScriptRoot $OutputFile
$xml | Out-File -FilePath $outputPath -Encoding UTF8
Write-Host "? Generated: $outputPath" -ForegroundColor Green
Write-Host ""
Write-Host "Components created: $($files.Count)" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. The PublishedFiles.wxs fragment has been created" -ForegroundColor Yellow
Write-Host "2. Update Package.wxs to reference 'PublishedFileComponents' instead of 'ProductComponents'" -ForegroundColor Yellow
Write-Host "3. Build the installer with the new components" -ForegroundColor Yellow
