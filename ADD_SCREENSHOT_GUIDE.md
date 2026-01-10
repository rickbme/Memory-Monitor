# Adding Screenshot to GitHub

## File Location
The README.md now references the screenshot at:
```
Resources/MemMonPic.png
```

## Steps to Add the Image

### Option 1: Add to Resources Folder (Recommended)
1. Create a `Resources` folder in the root of your repository (same level as README.md)
2. Copy `MemMonPic.png` (or whatever your screenshot file is named) into this folder
3. Rename the file to `MemMonPic.png` if it has a different name
4. Commit and push:
   ```bash
   git add Resources/MemMonPic.png
   git commit -m "Add Memory Monitor screenshot"
   git push
   ```

### Option 2: Use a Different Path
If you want to keep the image elsewhere, update the README.md line:
```markdown
![Memory Monitor Screenshot](Resources/MemMonPic.png)
```
Change `Resources/MemMonPic.png` to your preferred path, for example:
- `docs/images/MemMonPic.png`
- `assets/MemMonPic.png`
- `Memory Monitor/Resources/MemMonPic.png`

## Image Format Recommendations

### For GitHub README
- **Format:** PNG (recommended) or JPG
- **Size:** Ideally 1920x480 or scaled down to ~1280x320 for faster loading
- **File Size:** Keep under 500KB for best performance

### If Your Image Needs Resizing
If `MemMonPic` is a different format or too large:
1. Convert to PNG if needed
2. Consider resizing to 1280x320 or 1600x400 for faster loading
3. Optimize with a tool like TinyPNG or ImageOptim

## Current README Image Reference
The README currently shows:
```markdown
![Memory Monitor Screenshot](Resources/MemMonPic.png)
*Memory Monitor running on a 1920x480 mini display with real-time CPU, GPU, RAM, VRAM, Disk, and Network monitoring*
```

This will display the screenshot at the top of the README, right after the badges and before the Overview section.

## Verify on GitHub
After committing and pushing, check:
1. Go to https://github.com/rickbme/Memory-Monitor
2. The screenshot should appear near the top of the README
3. If it shows a broken image icon, verify the path and filename match exactly (case-sensitive)

## Alternative: Use GitHub Issues for Hosting
If you don't want to include the image in your repo:
1. Go to your GitHub repo ? Issues
2. Create a new issue (you can close it right away)
3. Drag and drop your screenshot into the issue comment
4. GitHub will upload it and generate a URL like: `https://user-images.githubusercontent.com/...`
5. Copy that URL and use it in the README:
   ```markdown
   ![Memory Monitor Screenshot](https://user-images.githubusercontent.com/...)
   ```
6. Close or delete the issue
