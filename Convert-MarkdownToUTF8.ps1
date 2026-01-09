# Convert all Markdown files to UTF-8 encoding (without BOM) for GitHub compatibility
# This fixes display issues with Unicode characters like arrows, checkmarks, and bullets

$ErrorActionPreference = "Stop"

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " Converting Markdown Files to UTF-8 (no BOM)" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

# Get all markdown files
$markdownFiles = Get-ChildItem -Path "." -Include "*.md" -Recurse

Write-Host "Found $($markdownFiles.Count) markdown files to convert" -ForegroundColor Yellow
Write-Host ""

$convertedCount = 0
$skippedCount = 0

foreach ($file in $markdownFiles) {
    try {
        Write-Host "Processing: $($file.Name)" -ForegroundColor Gray
        
        # Read file content as bytes first to detect encoding
        $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
        
        # Try to detect if it's already UTF-8 without BOM
        $isUTF8NoBOM = ($bytes.Length -ge 3 -and 
                       $bytes[0] -ne 0xEF -and 
                       $bytes[1] -ne 0xBB -and 
                       $bytes[2] -ne 0xBF)
        
        # Read as text using Windows default encoding (will handle Windows-1252)
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::Default)
        
        # Create UTF8 encoding without BOM
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        
        # Write the file back with UTF-8 no BOM
        [System.IO.File]::WriteAllText($file.FullName, $content, $utf8NoBom)
        
        Write-Host "  -> Converted to UTF-8 (no BOM)" -ForegroundColor Green
        $convertedCount++
    }
    catch {
        Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " Conversion Complete!" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Converted: $convertedCount files" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Review the changes with: git diff" -ForegroundColor White
Write-Host "2. Test that files display correctly on GitHub" -ForegroundColor White
Write-Host "3. Commit with: git commit -am 'Fix encoding for markdown files'" -ForegroundColor White
Write-Host ""
