# Fix specific markdown files that still have encoding issues
# Targets: INSTALLER_BUILD_SYSTEM_SUMMARY.md, INSTALLER_QUICKSTART.md, README.md

$ErrorActionPreference = "Stop"

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " Fixing Specific Markdown Files" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

# Target files
$targetFiles = @(
    "INSTALLER_BUILD_SYSTEM_SUMMARY.md",
    "INSTALLER_QUICKSTART.md",
    "README.md"
)

Write-Host "Target files to fix: $($targetFiles.Count)" -ForegroundColor Yellow
Write-Host ""

$fixedCount = 0

foreach ($fileName in $targetFiles) {
    # Find the file
    $file = Get-ChildItem -Path "." -Filter $fileName -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if ($null -eq $file) {
        Write-Host "File not found: $fileName" -ForegroundColor Red
        continue
    }
    
    try {
        Write-Host "Processing: $($file.Name)" -ForegroundColor Gray
        Write-Host "  Location: $($file.DirectoryName)" -ForegroundColor DarkGray
        
        # Read file content using Windows default encoding (handles Windows-1252)
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::Default)
        
        # Create UTF8 encoding without BOM
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        
        # Write the file back with UTF-8 no BOM
        [System.IO.File]::WriteAllText($file.FullName, $content, $utf8NoBom)
        
        Write-Host "  -> Converted to UTF-8 (no BOM)" -ForegroundColor Green
        $fixedCount++
    }
    catch {
        Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
}

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " Fix Complete!" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Fixed: $fixedCount files" -ForegroundColor Green
Write-Host ""
