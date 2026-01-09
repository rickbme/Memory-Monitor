# Fix emoji and special characters in markdown files
# Replaces placeholder characters with proper Unicode equivalents

$ErrorActionPreference = "Stop"

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " Fixing Emoji and Special Characters" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

# Define character replacements
# These are common emoji/symbol patterns that get corrupted
$replacements = @{
    # Dashboard/Gauge icons
    'â†'' = '?'  # Right arrow
    'â†µ' = '?'  # Down-left arrow  
    'â†"' = '?'  # Down arrow
    'â†'' = '?'  # Up arrow
    'â†?' = '?'  # Left arrow
    
    # Checkmarks
    'âœ"' = '?'  # Checkmark
    'âœ…' = '?' # White check mark
    'â?Œ' = '?' # Cross mark
    
    # Other symbols
    'ðŸ"Š' = '??' # Bar chart
    'ðŸ"§' = '??' # Wrench
    'ðŸ'»' = '??' # Computer
    'ðŸš€' = '??' # Rocket
    'âš™' = '?'  # Gear
    'ðŸŽ¯' = '??' # Target
    'ðŸ"¥' = '??' # Fire
    'â­'' = '?' # Star
    'ðŸ"' = '??' # Chart increasing
    
    # Bullets
    'â€¢' = '•'  # Bullet
    'â—¦' = '?'  # White bullet
    
    # Math symbols
    'â‰¥' = '?'  # Greater than or equal
    'â‰¤' = '?'  # Less than or equal
    
    # Common emoji that might be in markdown
    '???' = '??' # Dashboard (3 question marks often means emoji)
    '??' = '??'  # Tool (2 question marks)
}

# Target files
$targetFiles = @(
    "INSTALLER_BUILD_SYSTEM_SUMMARY.md",
    "INSTALLER_QUICKSTART.md", 
    "README.md"
)

Write-Host "Files to process: $($targetFiles.Count)" -ForegroundColor Yellow
Write-Host ""

$fixedCount = 0

foreach ($fileName in $targetFiles) {
    $file = Get-ChildItem -Path "." -Filter $fileName -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if ($null -eq $file) {
        Write-Host "File not found: $fileName" -ForegroundColor Red
        continue
    }
    
    try {
        Write-Host "Processing: $($file.Name)" -ForegroundColor Gray
        
        # Read file as UTF-8
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
        
        $originalContent = $content
        
        # Apply replacements
        foreach ($key in $replacements.Keys) {
            if ($content.Contains($key)) {
                $content = $content.Replace($key, $replacements[$key])
                Write-Host "  Replaced: $key -> $($replacements[$key])" -ForegroundColor Yellow
            }
        }
        
        if ($content -ne $originalContent) {
            # Create UTF8 encoding without BOM
            $utf8NoBom = New-Object System.Text.UTF8Encoding $false
            
            # Write the file back
            [System.IO.File]::WriteAllText($file.FullName, $content, $utf8NoBom)
            
            Write-Host "  -> Fixed and saved" -ForegroundColor Green
            $fixedCount++
        }
        else {
            Write-Host "  No changes needed" -ForegroundColor DarkGray
        }
    }
    catch {
        Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
}

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " Character Fix Complete!" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Files modified: $fixedCount" -ForegroundColor Green
Write-Host ""
Write-Host "Note: If you still see issues, the original file may need" -ForegroundColor Yellow
Write-Host "manual editing to replace corrupted emoji with proper ones." -ForegroundColor Yellow
Write-Host ""
