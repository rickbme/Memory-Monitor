# ============================================================================
# Regenerate PublishedFiles.wxs
# ============================================================================
# This script scans the publish directory and generates a complete WiX
# component file including all files and subdirectories.
# ============================================================================

param(
    [string]$PublishPath = "..\Memory Monitor\bin\Release\net8.0-windows\win-x64\publish",
    [string]$OutputFile = "PublishedFiles.wxs"
)

# Ensure publish directory exists
if (-not (Test-Path $PublishPath)) {
    Write-Error "Publish directory not found: $PublishPath"
    Write-Host "Please run: dotnet publish '..\Memory Monitor\Memory Monitor.csproj' -c Release"
    exit 1
}

Write-Host "Scanning publish directory: $PublishPath"

# Initialize counters and collections
$componentId = 1
$guidCache = @{}

# Function to generate consistent GUIDs
function Get-ConsistentGuid {
    param([string]$name)
    
    if ($guidCache.ContainsKey($name)) {
        return $guidCache[$name]
    }
    
    $guid = [guid]::NewGuid().ToString().ToUpper()
    $guidCache[$name] = $guid
    return $guid
}

# Function to sanitize directory names for WiX identifiers
function Get-SafeIdentifier {
    param([string]$name)
    # Replace hyphens with underscores for WiX compliance
    return $name -replace '-', '_'
}

# Start building XML
$xml = @"
<?xml version="1.0" encoding="utf-8"?>
<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">
  <Fragment>
    <ComponentGroup Id="PublishedFileComponents" Directory="INSTALLFOLDER">
"@

# Process root-level files
Write-Host "Processing root files..."
$rootFiles = Get-ChildItem -Path $PublishPath -File | Sort-Object Name
foreach ($file in $rootFiles) {
    $fileName = $file.Name
    $guid = Get-ConsistentGuid $fileName
    
    $isMainExe = ($fileName -eq "Memory Monitor.exe")
    $keyPath = if ($isMainExe) { ' KeyPath="yes"' } else { '' }
    
    $xml += @"
      <Component Id="Comp_$componentId" Guid="$guid">
        <File Id="File_$componentId" 
              Name="$fileName"
              Source="..\Memory Monitor\bin\Release\net8.0-windows\win-x64\publish\$fileName"$keyPath />
      </Component>
"@
    $componentId++
}

# Process subdirectories
Write-Host "Processing subdirectories..."
$directories = Get-ChildItem -Path $PublishPath -Directory | Sort-Object Name

foreach ($dir in $directories) {
    $dirName = $dir.Name
    $safeDirName = Get-SafeIdentifier $dirName
    Write-Host "  - $dirName"
    
    # Add directory component group
    $xml += @"

    </ComponentGroup>
  </Fragment>
  
  <Fragment>
    <DirectoryRef Id="INSTALLFOLDER">
      <Directory Id="Dir_$safeDirName" Name="$dirName" />
    </DirectoryRef>
  </Fragment>
  
  <Fragment>
    <ComponentGroup Id="${safeDirName}Components" Directory="Dir_$safeDirName">
"@
    
    # Add files in this directory
    $dirFiles = Get-ChildItem -Path $dir.FullName -File | Sort-Object Name
    foreach ($file in $dirFiles) {
        $fileName = $file.Name
        $relPath = "$dirName\$fileName"
        $guid = Get-ConsistentGuid $relPath
        
        $xml += @"
      <Component Id="Comp_$componentId" Guid="$guid">
        <File Id="File_$componentId" 
              Name="$fileName"
              Source="..\Memory Monitor\bin\Release\net8.0-windows\win-x64\publish\$relPath" />
      </Component>
"@
        $componentId++
    }
}

# Close final ComponentGroup
$xml += @"

    </ComponentGroup>
  </Fragment>
</Wix>
"@

# Write to file
$xml | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host ""
Write-Host "Generated $OutputFile with $($componentId - 1) components" -ForegroundColor Green
Write-Host ""
Write-Host "Directory components created:"
foreach ($dir in $directories) {
    $safeDirName = Get-SafeIdentifier $dir.Name
    Write-Host "  - ${safeDirName}Components" -ForegroundColor Cyan
}
Write-Host ""
Write-Host "IMPORTANT: Update Package.wxs to include subdirectory components:" -ForegroundColor Yellow
Write-Host "  <Feature Id=`"Main`" Title=`"Memory Monitor`" Level=`"1`">"
Write-Host "    <ComponentGroupRef Id=`"PublishedFileComponents`" />"
foreach ($dir in $directories) {
    $safeDirName = Get-SafeIdentifier $dir.Name
    Write-Host "    <ComponentGroupRef Id=`"${safeDirName}Components`" />"
}
Write-Host "    <ComponentGroupRef Id=`"StartMenuShortcuts`" />"
Write-Host "    <ComponentGroupRef Id=`"DesktopShortcuts`" />"
Write-Host "  </Feature>"
