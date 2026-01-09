# Manual Emoji Fix Guide

## Files That Need Manual Emoji Fixes

The following files have corrupted emoji characters that display as `???` or `??`:

### 1. README.md

**Line 1:** 
```markdown
# ??? System Monitor
```
Should be:
```markdown
# ?? System Monitor
```

**Line ~12:**
```markdown
## ?? Overview
```
Should be:
```markdown
## ?? Overview
```

**Line ~16:**
```markdown
### ? Key Features
```
Should be:
```markdown
### ? Key Features
```

**Line ~30:**
```markdown
## ?? Display Layout
```
Should be:
```markdown
## ??? Display Layout
```

Search for other instances of:
- `???` (3 question marks) ? Usually a dashboard/chart emoji: ??
- `??` (2 question marks) ? Usually a tool/settings emoji: ?? or ??
- `?` (single question mark in emoji context) ? Usually a star: ?

### 2. INSTALLER_BUILD_SYSTEM_SUMMARY.md

Look for section headers with `??` or `???` and replace with appropriate emoji:
- Build/Setup sections: ??
- Status/Success: ?
- Features: ?
- Steps: ??
- Folders: ??

### 3. INSTALLER_QUICKSTART.md

Similar pattern - headers likely have `??` that should be:
- Quick Start: ??
- Build: ??
- Install: ??
- Check: ?

## How to Fix Manually

### Option 1: Using Visual Studio Code
1. Open the file in VS Code
2. Use Find & Replace (Ctrl+H)
3. Find: `# ??? System Monitor`
4. Replace with: `# ?? System Monitor`
5. Repeat for each pattern
6. Save as UTF-8 (bottom right of VS Code should show "UTF-8")

### Option 2: Using Visual Studio 2022
1. Open the file in Visual Studio
2. Use Find & Replace (Ctrl+H)
3. Replace patterns as above
4. File ? Advanced Save Options ? Encoding: Unicode (UTF-8 without signature)
5. Save

### Option 3: Using Notepad++ (If Available)
1. Open file
2. Encoding ? Convert to UTF-8 (without BOM)
3. Manually replace `???` and `??` with appropriate emoji
4. Copy emoji from: https://emojipedia.org/
5. Save

## Common Emoji Replacements

For quick copy-paste:

```
?? - Dashboard/Chart (for "System Monitor" title)
?? - Overview/Clipboard
? - Key Features/Important
??? - Display/Monitor
?? - Build/Tools
? - Success/Complete
?? - Quick Start/Launch
?? - Install/Package
?? - Folder/Directory
?? - Steps/Notes
?? - Build Process
? - Checkmark
? - Arrow right
? - Arrow left
• - Bullet point
```

## Why This Happened

These files were originally created with emoji characters in Windows-1252 encoding,
which can't properly represent Unicode emoji. When converted to UTF-8, the emoji
became corrupted into `?` characters.

The solution is to:
1. Ensure files are UTF-8 encoded ? (Done by Convert-MarkdownToUTF8.ps1)
2. Manually re-add the emoji characters (Needs to be done in editor)

## Verification

After fixing, verify by:
1. Opening the file on GitHub (after committing)
2. Emoji should display correctly
3. No `?` question marks in place of emoji

## Automation Note

We attempted to automate this with PowerShell scripts, but the scripts themselves
have encoding issues when trying to handle emoji. Manual editing in a proper
UTF-8 aware editor is the most reliable approach.
