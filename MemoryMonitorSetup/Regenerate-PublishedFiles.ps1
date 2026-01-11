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
$subdirComponentGroups = @()

# Function to generate consistent GUIDs based on file path
function Get-ConsistentGuid {
    param([string]$name)
    
    if ($guidCache.ContainsKey($name)) {
        return $guidCache[$name]
    }
    
    # Generate a deterministic GUID from the file path for consistency across rebuilds
    $md5 = [System.Security.Cryptography.MD5]::Create()
    $hash = $md5.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($name))
    $guid = [System.Guid]::new($hash).ToString().ToUpper()
    $guidCache[$name] = $guid
    return $guid
}

# Function to sanitize directory names for WiX identifiers
function Get-SafeIdentifier {
    param([string]$name)
    # Replace hyphens, spaces, and dots with underscores for WiX compliance
    $safe = $name -replace '[-\s\.]', '_'
    # Ensure it doesn't start with a number
    if ($safe -match '^\d') {
        $safe = "Dir_$safe"
    }
    return $safe
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
    $guid = Get-ConsistentGuid "root\$fileName"
    
    $isMainExe = ($fileName -eq "Memory Monitor.exe")
    
    $xml += @"
      <Component Id="Comp_$componentId" Guid="$guid">
        <File Id="File_$componentId" 
              Source="..\Memory Monitor\bin\Release\net8.0-windows\win-x64\publish\$fileName" />
      </Component>
"@
    $componentId++
}

# Close PublishedFileComponents
$xml += @"
    </ComponentGroup>
  </Fragment>
"@

# Process subdirectories
Write-Host "Processing subdirectories..."
$directories = Get-ChildItem -Path $PublishPath -Directory | Sort-Object Name

foreach ($dir in $directories) {
    $dirName = $dir.Name
    $safeDirName = Get-SafeIdentifier $dirName
    Write-Host "  - $dirName -> ${safeDirName}Components"
    
    $subdirComponentGroups += $safeDirName
    
    # Add directory fragment
    $xml += @"
  <Fragment>
    <DirectoryRef Id="INSTALLFOLDER">
      <Directory Id="Dir_$safeDirName" Name="$dirName" />
    </DirectoryRef>
  </Fragment>
  
  <Fragment>
    <ComponentGroup Id="${safeDirName}Components" Directory="Dir_$safeDirName">
"@
    
    # Add files in this directory
    $dirFiles = Get-ChildItem -Path $dir.FullName -File -Recurse | Sort-Object FullName
    foreach ($file in $dirFiles) {
        $relativePath = $file.FullName.Substring($dir.FullName.Length + 1)
        $relPath = "$dirName\$relativePath"
        $guid = Get-ConsistentGuid $relPath
        
        # Handle subdirectories within the language folders
        if ($relativePath.Contains('\')) {
            $subDirPath = Split-Path $relativePath -Parent
            $fileName = Split-Path $relativePath -Leaf
            $fullRelPath = "$dirName\$relativePath"
        } else {
            $fileName = $relativePath
            $fullRelPath = "$dirName\$fileName"
        }
        
        $xml += @"
      <Component Id="Comp_$componentId" Guid="$guid">
        <File Id="File_$componentId" 
              Source="..\Memory Monitor\bin\Release\net8.0-windows\win-x64\publish\$fullRelPath" />
      </Component>
"@
        $componentId++
    }
    
    # Close this ComponentGroup
    $xml += @"
    </ComponentGroup>
  </Fragment>
"@
}

# Close Wix element
$xml += @"
</Wix>
"@

# Write to file
$xml | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host ""
Write-Host "Generated $OutputFile with $($componentId - 1) components" -ForegroundColor Green
Write-Host ""
Write-Host "Directory component groups created:" -ForegroundColor Cyan
foreach ($grp in $subdirComponentGroups) {
    Write-Host "  - ${grp}Components" -ForegroundColor Yellow
}
Write-Host ""
Write-Host "Make sure Package.wxs includes these ComponentGroupRef entries in the Feature:" -ForegroundColor Green
Write-Host "  <Feature Id=`"Main`" Title=`"Memory Monitor`" Level=`"1`">"
Write-Host "    <ComponentGroupRef Id=`"PublishedFileComponents`" />"
foreach ($grp in $subdirComponentGroups) {
    Write-Host "    <ComponentGroupRef Id=`"${grp}Components`" />"
}
Write-Host "    <ComponentGroupRef Id=`"StartMenuShortcuts`" />"
Write-Host "    <ComponentGroupRef Id=`"DesktopShortcuts`" />"
Write-Host "  </Feature>"
Write-Host ""
