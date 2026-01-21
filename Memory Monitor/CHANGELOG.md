# Changelog

All notable changes to the Memory Monitor project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.4.2] - 2025-01

### Added
- **Bar Graph Display Mode** - New alternate display option with animated bar graph panels
  - **BarGraphPanelControl** - Custom control for CPU/GPU/Drive panels with:
    - 12-point rolling bar history animation
    - Gradient-filled bars with glow effects
    - Title, percentage, and secondary info (temperature/speed)
    - Futuristic corner accents and accent lines
  - **NetworkBarPanelControl** - Specialized network panel with:
    - Dual horizontal progress bars for upload/download
    - Speed labels positioned below each bar
    - Segmented bar design for tech aesthetic
    - Auto-scaling based on peak speeds
  - **BarGraphDisplayForm** - Main form hosting the bar graph layout
    - 4 panels: CPU, GPU, Drive, Network
    - Full tray icon support with Display Mode menu
    - Same hardware monitoring as circular gauge mode
  - **DisplayModeManager** - Static manager for display mode state
    - `DisplayMode` enum (CircularGauges, BarGraph)
    - Persists user preference to application settings
    - `DisplayModeChanged` event for runtime form switching
    - `SetDisplayMode()` and `ToggleDisplayMode()` methods

- **Display Mode Menu** - New tray menu option to switch between display modes
  - Right-click tray icon ? **Display Mode** submenu
  - **Circular Gauges** - Classic needle gauge display (default)
  - **Bar Graph** - New animated bar graph display
  - Preference saved and restored between sessions
  - Instant switching with no restart required

### Changed
- **Program.cs** - Now handles display mode switching at startup and runtime
  - Starts with saved display mode preference
  - Subscribes to `DisplayModeChanged` event for runtime switching
  - Creates appropriate form based on current mode

### Technical Changes
- Added `BarGraphPanelControl.cs`:
  - `Queue<float>` for 12-point value history
  - `SetValue()` adds to history and triggers repaint
  - Custom painting with gradients, glows, and accents
- Added `NetworkBarPanelControl.cs`:
  - `SetSpeeds()` for upload/download values
  - Dual horizontal bar rendering
  - Speed text positioned below each bar (fixed overlap issue)
- Added `BarGraphDisplayForm.cs` and `BarGraphDisplayForm.Designer.cs`:
  - 4-panel layout (CPU, GPU, Drive, Network)
  - Hardware monitor initialization
  - Tray menu with Display Mode submenu
- Added `DisplayModeManager.cs`:
  - Static class for display mode state management
  - Settings persistence via `Properties.Settings.Default.DisplayMode`
  - Event-based notification for mode changes
- Updated `MiniMonitorForm.Designer.cs`:
  - Added `displayModeToolStripMenuItem` submenu
  - Added `circularGaugesToolStripMenuItem` and `barGraphToolStripMenuItem`
- Updated `MiniMonitorForm.cs`:
  - Added `CircularGaugesToolStripMenuItem_Click` handler
  - Added `BarGraphToolStripMenuItem_Click` handler
  - Added `UpdateDisplayModeMenuState()` method
- Updated `Properties\Settings.Designer.cs`:
  - Added `DisplayMode` string setting (default: "CircularGauges")

## [2.4.1] - 2025-01-10

### Fixed
- **Clock Display Not Updating** - Fixed issue where date/time labels were not updating after startup
  - Added `UpdateDateTime()` call to the main timer update cycle
  - Clock now updates correctly every second
- **Display Flicker/Blips** - Optimized rendering to reduce unnecessary screen repaints
  - `CompactGaugeControl.SetValue()` now only invalidates when values actually change
  - `FpsGaugeControl.SetFps()` now only invalidates when FPS value changes
  - FPS gauge visibility only toggles when transitioning to/from gaming state
  - Date/time labels only update when text actually changes (every minute for time)
  - Property setters now check for value changes before calling `Invalidate()`

### Added
- **Installer UI Dialogs** - Full Windows Installer UI experience
  - Welcome dialog with product information
  - Installation directory selection
  - Ready to install confirmation
  - Progress bar during installation
  - Completion dialog with success message and next steps
- **Maintenance Mode** - When product is already installed, offers Repair/Remove options
  - Accessible by running installer again or from Add/Remove Programs
  - Repair reinstalls all files to fix corrupted installation
  - Remove completely uninstalls the application

### Changed
- **Performance Optimization** - Reduced CPU usage and GDI resource consumption
  - Gauges no longer repaint every second if values haven't changed
  - Estimated 60-80% reduction in unnecessary `Invalidate()` calls
- **Installer Behavior** - Removed ARPNOREPAIR and ARPNOMODIFY restrictions
  - Users can now Repair or Modify from Add/Remove Programs

### Technical Changes
- Updated `MiniMonitorForm.Monitors.cs`:
  - Added `SafeUpdate(UpdateDateTime)` to `UpdateAllMetrics()`
  - Optimized `UpdateFps()` to only change visibility when needed
- Updated `MiniMonitorForm.DateTime.cs`:
  - `UpdateDateTime()` now compares text before updating labels
- Updated `CompactGaugeControl.cs`:
  - All property setters now check if value changed before invalidating
  - `SetValue()` methods use tolerance-based comparison for floats
- Updated `FpsGaugeControl.cs`:
  - `SetFps()` only invalidates when value actually changes
- Updated `MemoryMonitorSetup.wixproj`:
  - Added `WixToolset.UI.wixext` package for standard dialogs
  - Added `WixToolset.Util.wixext` package for utility functions
- Updated `Package.wxs`:
  - Added `ui:WixUI Id="WixUI_InstallDir"` for full dialog set
  - Added `WIXUI_EXITDIALOGOPTIONALTEXT` for completion message
  - Removed `ARPNOREPAIR` and `ARPNOMODIFY` to enable maintenance mode

## [2.4.0] - 2025-01-10

### Added
- **Date & Time Display** - Current date and time shown on the mini monitor
  - Date displayed in top-left corner (e.g., "December 28")
  - Time displayed in top-right corner in 12-hour format (e.g., "2:00 PM")
  - 22pt Segoe UI Bold font for excellent readability
  - Updates every second with the main timer
- **FPS Gauge Control** - New dedicated circular gauge for FPS display
  - Compact circular design positioned between GPU and VRAM gauges
  - Color-coded ring based on FPS quality:
    - Green (60 FPS) - Excellent
    - Yellow-Green (45-59 FPS) - Good
    - Orange (30-44 FPS) - Acceptable
    - Red (<30 FPS) - Poor
  - Auto-scaling font for 1-4 digit FPS values
  - "FPS" label below the number
- **Game Activity Detection** - Smart detection system to show FPS only when gaming
  - Weighted scoring system combining multiple signals:
    - FPS data available (strongest signal)
    - Fullscreen/borderless window detection
    - Sustained high GPU usage (>70% for 5+ seconds)
    - Known game process detection (~60+ popular games)
  - Uses Win32 APIs for fullscreen and process detection
- **FPS Display Mode Menu** - Tray menu option to control FPS visibility
  - **Auto-detect** (default) - Shows FPS only when game activity detected
  - **Always Show** - Manual override to always display FPS when available
  - **Always Hide** - Never show FPS gauge
  - Visual toast notification when mode changes
- **Touch Cycling for Device Selection** - Touch-friendly navigation for disk and network gauges
  - Touch tap on disk/ethernet gauge cycles through available devices instead of showing popup menu
  - Each tap advances to the next device (wraps around to beginning)
  - Mouse clicks still show popup menu for precise selection
  - Visual toast notification shows newly selected device name
  - Popup menu automatically reflects current device when opened with mouse
  - Works for GPU, disk, and network gauges on touch-enabled displays
- **First-Run Welcome Dialog** - Friendly introduction for new users
  - Appears automatically on first launch after installation
  - Highlights key features with visual icons
  - "Open User Guide" button to launch USER_GUIDE.md
  - "Get Started" button to begin using the application
  - "Don't show this again" checkbox for experienced users
  - Registry-based first-run detection
  - Dark-themed design matching application style
  - Shown only once by default, can be reopened from the Help menu

### Changed
- **FPS Display** - Replaced simple text label with dedicated FPS gauge control
- **MiniMonitorForm.DateTime.cs** - New partial class for date/time display functionality
- **GameActivityDetector.cs** - New class for intelligent game activity detection
- **FpsGaugeControl.cs** - New custom control for FPS visualization
- **Touch Gesture Behavior** - Tapping selectable gauges now cycles through options instead of showing menu
- **FirstRunDialog** - New dialog class for the first-run welcome screen

### Improved
- **Device Selection - Aggregate Mode Always Available** - Disk and Network monitors now always show "All Disks" and "All Networks" options
  - Previously only shown when multiple devices detected
  - Users can now switch back to combined/total view after selecting a specific device
  - Single-device systems still show the aggregate option for consistency
  - Improves user experience by always providing access to total system throughput
- **Touch UX for Mini Displays** - Device selection now optimized for touch on small screens
  - Cycling avoids the need for small, hard-to-tap popup menus
  - Better suited for 1920x480 mini monitor touch displays
  - Mouse users still get full popup menu with all options visible

### Removed
- **CPU Temperature Warning Popup** - Removed the notification that appeared when CPU temperature was unavailable
  - Temperature monitoring still works when HWiNFO is running
  - Setup instructions moved to README files to avoid repetitive popups
  - Users can now run the application without HWiNFO without seeing warnings

### Technical Changes
- Added `MiniMonitorForm.DateTime.cs`:
  - `InitializeDateTimeDisplay()` - Configures date/time labels
  - `UpdateDateTime()` - Updates time (12-hour) and date (Month Day)
  - `LayoutDateTimeLabels()` - Positions labels in corners
- Added `GameActivityDetector.cs`:
  - Win32 API integration for window and process detection
  - `FpsDisplayMode` enum (AutoDetect, AlwaysShow, AlwaysHide)
  - GPU usage history tracking for sustained usage detection
  - Known game process list with partial matching
- Added `FpsGaugeControl.cs`:
  - Custom circular gauge with color-coded ring
  - Dynamic font scaling based on digit count
  - `SetFps()` method for updating display
- Added `WelcomeDialog.cs`:
  - First-run welcome dialog for new users
  - Registry-based first-run detection (`HKCU\Software\MemoryMonitor\FirstRunComplete`)
  - Feature highlights with visual icons
  - Opens USER_GUIDE.md from installation directory
  - "Don't show again" checkbox option
  - `ShowIfFirstRun()` static method for easy integration
  - `ResetFirstRun()` utility method for testing
- Updated `ISelectableMonitor.cs`:
  - Added `CycleToNextDevice()` method for touch-based navigation
- Updated `DiskMonitor.cs`, `NetworkMonitor.cs`, `GPUMonitor.cs`:
  - Implemented `CycleToNextDevice()` to cycle through available devices
- Updated `CompactGaugeControl.cs`:
  - Added `DeviceCycleRequested` event for touch cycling
  - Added `_isTouchMode` flag to distinguish touch taps from mouse clicks
  - Updated `PerformClick()` to raise `DeviceCycleRequested` for touch
  - Mouse clicks still raise `DeviceSelectionRequested` for popup menu
- Updated `MiniMonitorForm.DeviceSelection.cs`:
  - Added `GpuGauge_DeviceCycleRequested()` event handler
  - Added `DiskGauge_DeviceCycleRequested()` event handler
  - Added `NetworkGauge_DeviceCycleRequested()` event handler
  - Each handler shows toast notification with device name
- Updated `MiniMonitorForm.cs`:
  - Added `_gameActivityDetector` field
  - Added `InitializeGameDetection()` method
  - Added FPS menu click handlers
  - Added `SetFpsDisplayMode()` helper method
  - Integrated `WelcomeDialog.ShowIfFirstRun()` in `OnLoad()` method
- Updated `MiniMonitorForm.Designer.cs`:
  - Added `lblDate` and `lblTime` label controls
  - Added `fpsGauge` FPS gauge control
  - Added FPS Display submenu with mode options
- Updated `MiniMonitorForm.Layout.cs`:
  - Added `LayoutDateTimeLabels()` for corner positioning
  - Updated `LayoutGauges()` for FPS gauge placement (30% of main gauge size)
  - Updated `SetGaugesVisible()` for date/time labels
- Updated `MiniMonitorForm.Monitors.cs`:
  - `UpdateFps()` now uses GameActivityDetector and FpsGaugeControl
  - Added `UpdateDateTime()` to timer update cycle
  - Removed CPU temperature warning fields and `ShowCpuTempWarning()` method
  - Simplified `UpdateCPU()` to display temperature when available without notifications
- Added `FirstRunDialog.cs`:
  - New dialog class for displaying the first-run welcome screen
  - Checkbox to suppress future displays
  - Button to open the user guide
  - Button to start using the application

### Notes
- Touch cycling is designed for small touch displays where popup menus are difficult to use
- Mouse users still have access to the full popup menu for quick device selection
- Each touch tap cycles forward through the device list (All ? Device 1 ? Device 2 ? etc. ? All)
- Device selection is saved and restored between application sessions
- The first-run welcome dialog is shown only once by default and can be suppressed
- New users are encouraged to explore the user guide for detailed information

## [2.3.0] - 2025-01-XX

### Added
- **Splash Screen** - Application now displays a branded splash screen on startup
  - Round DFS logo displayed centered on screen with fade in/out effect
  - Displays for 1.5 seconds before transitioning
  - Can be dismissed by clicking or pressing any key
- **Mini Monitor Intro Logo** - Wide DFS logo displayed on the 1920x480 mini monitor
  - Shows for 2 seconds before gauges appear
  - Provides branded startup experience on the dedicated display
- **Touch Gesture Support** - Full touch input support for mini monitors with touchscreen capability
  - **Swipe Left/Right** - Switch between monitors
  - **Swipe Down** - Minimize to system tray
  - **Tap on Gauge** - Open device selection popup (for GPU, Disk, Network gauges)
  - **Long Press** - Show context menu at touch location
  - **Two-Finger Tap** - Toggle "Always on Top" mode
- **FPS Monitoring** - Display frames per second when HWiNFO with RTSS integration is running
  - Shows FPS between GPU and VRAM gauges (e.g., "FPS - 89fps")
  - Automatically hidden when FPS data is not available
  - Requires HWiNFO with "Shared Memory Support" enabled and RTSS or game overlay reporting FPS
- **SystemMemoryMonitor** - New dedicated class for RAM monitoring with Windows API
- **TouchGestureHandler** - New component using Windows Touch API (`WM_TOUCH`, `WM_GESTURE`)
- **Visual Feedback** - Toast notifications for gesture actions (monitor switching, always-on-top toggle)
- **Graceful Degradation** - Application works normally on systems without touch support

### Changed
- **Minimize to Tray** - Application now minimizes to the system tray instead of the taskbar
  - Window shows normally on startup
  - When minimized (Escape key, swipe down, or minimize), hides to system tray
  - Double-click tray icon or use "Show" menu to restore the window
  - Keeps the taskbar clean while monitoring continues in background
- **CompactGaugeControl** - Added `PerformClick()` method to support touch tap gestures
- **MiniMonitorForm** - Added `WndProc` override for touch message processing
- **HWiNFOReader** - Extended to detect and read FPS sensor data from RTSS/game overlays

### Refactored
- **MiniMonitorForm Split into Partial Classes** - Improved code organization and maintainability
  - `MiniMonitorForm.cs` - Core initialization, window management, event handlers (~300 lines)
  - `MiniMonitorForm.Monitors.cs` - Metric update methods and shared fields (~235 lines)
  - `MiniMonitorForm.DeviceSelection.cs` - Device selection logic (~210 lines)
  - `MiniMonitorForm.Layout.cs` - UI layout, theming, intro logo, toast notifications (~280 lines)
- **Removed Legacy Code** - Deleted unused `Form1.cs` and `Form1.Designer.cs`

### Technical Changes
- Added `SplashScreen.cs`:
  - Displays `dfslogo_round.png` with fade in/out animation
  - Modal dialog blocks until dismissed or timeout
  - Dark theme background matching application
- Added `SystemMemoryMonitor.cs`:
  - Encapsulates Windows `GlobalMemoryStatusEx` API
  - Provides `Update()`, `GetDisplayText()`, and memory properties
  - Implements `IMonitor` interface
- Added `TouchGestureHandler.cs`:
  - Windows Touch API P/Invoke declarations
  - Gesture detection (swipe, tap, long-press, two-finger tap, pinch/zoom)
  - `IsTouchAvailable` property for capability detection
  - Proper resource cleanup via `IDisposable`
- Added event argument classes:
  - `SwipeEventArgs` with direction and distance
  - `TapEventArgs` with location and tapped control
  - `LongPressEventArgs` with location and control
  - `PointEventArgs` for two-finger tap
  - `ZoomEventArgs` for pinch gestures
- Updated `HWiNFOReader.cs`:
  - Added FPS sensor detection (`IsFpsAvailable`, `FpsSensorName`)
  - Added `GetFps()` method to read current FPS value
  - Added `RefreshSensors()` method for dynamic sensor detection
- Updated `MiniMonitorForm.Layout.cs`:
  - Added `ShowIntroLogo()` for mini monitor branding
  - Added `TransitionToGauges()` for smooth startup sequence
  - Added `SetGaugesVisible()` helper method
- Updated `MiniMonitorForm.Designer.cs`:
  - Added `lblFps` label control with cyan color styling
- Updated `Program.cs`:
  - Added `SplashScreen.ShowSplash()` before main form
- Updated `Memory Monitor.csproj`:
  - Added `dfslogo_round.png` and `dfs_logo_1920.png` as content items

### Notes
- Touch support requires the mini monitor to be connected via USB data cable (not power-only)
- The monitor must have HID-compliant touch hardware recognized by Windows
- FPS monitoring requires:
  1. HWiNFO running with "Shared Memory Support" enabled
  2. RTSS (RivaTuner Statistics Server) or a game reporting FPS to HWiNFO
- All features continue to work with mouse/keyboard when touch/FPS is unavailable
- Splash screen images should be placed in the `Resources` folder

## [2.2.0] - 2024-12-26

### Added
- **Custom Application Icon** - New gauge-style icon (`mmguage.ico`) for the executable and desktop shortcut
- **Dynamic Tray Icon** - System tray icon now shows live RAM usage as a color-coded gauge
  - Green (0-50%), Yellow (50-80%), Red (80-100%)
  - Updates in real-time as memory usage changes
- **GaugeIconGenerator** - New utility class for generating gauge-style icons programmatically
- **WarningOverlay** - Reusable warning dialog component with proper resource disposal

### Changed
- **Icon System** - Hybrid approach: static icon for desktop, dynamic icon for system tray
- **Code Quality Improvements**:
  - Extracted `WarningOverlay` class from `MiniMonitorForm` for reusability
  - Added `SafeUpdate()` wrapper to prevent one failed metric from stopping all updates
  - Fixed font memory leaks in warning dialogs
  - Fixed nullable warnings in GPU temperature checks
- **AMD GPU Memory** - Now falls back to Windows performance counters since ADL doesn't provide used memory
- **Intel GPU Support** - Added clear debug messaging about native monitoring limitations
- **MaxValue Safety** - `CompactGaugeControl.MaxValue` now enforces minimum of 0.001 to prevent division by zero

### Fixed
- **Icon Format** - Converted PNG-based icon to proper multi-resolution ICO format (16-256px)
- **Build Configuration** - Fixed LibreHardwareMonitor x64 platform requirement with `SetPlatform` attribute
- **Resource Disposal** - Fonts in warning dialogs are now properly disposed when dismissed
- **Nullable Warnings** - Fixed CS8629 warnings in GPU temperature initialization

### Technical Changes
- Added `GaugeIconGenerator.cs` - Dynamic tray icon generation with color coding
- Added `WarningOverlay.cs` - Reusable warning panel with proper disposal pattern
- Updated `MiniMonitorForm.cs`:
  - Replaced inline warning panel with `WarningOverlay` class
  - Added `SafeUpdate()` method for resilient metric updates
  - Added dynamic tray icon support via `UpdateTrayIcon()`
- Updated `GPUMonitor.cs`:
  - AMD memory now uses performance counter fallback
  - Intel GPU messaging improved
  - Nullable warning fixes
- Updated `CompactGaugeControl.cs`:
  - `MaxValue` minimum changed from 1 to 0.001f
- Updated `Memory Monitor.csproj`:
  - Added `<ApplicationIcon>` for exe icon
  - Added `SetPlatform="Platform=x64"` for LibreHardwareMonitor reference
  - Added icon copy to output directory

## [2.1.0] - 2024

### Added
- **Borderless Full Screen Mode** - Application now runs without title bar for clean display
- **Auto Monitor Detection** - Automatically detects and positions on mini monitor (4:1 aspect ratio)
- **Move to Next Monitor** - Tray menu option to cycle through available monitors
- **Always on Top Toggle** - Tray menu option and F11 keyboard shortcut
- **Window Dragging** - Click and drag anywhere to reposition the borderless window
- **Keyboard Shortcuts**:
  - `Escape` - Minimize to system tray
  - `F11` - Toggle always on top

### Changed
- **Gauge Sizing** - Larger gauges using 95% of vertical space with minimal margins
- **Gauge Centering** - Gauges now properly centered both horizontally and vertically
- **Label Font Size** - Increased gauge labels from 10pt to 14pt for better readability
- **Scaled Elements** - Temperature display, digital readout, and labels now scale proportionally with gauge size
- **Gauge Radius** - Increased from 40% to 42% of available space for larger dials
- **Reduced Margins** - Side margins reduced from 20px to 10px, 5px spacing between gauges

### UI Improvements
- Removed window title bar and borders for dedicated display use
- Form automatically sizes to fill target monitor
- Gauges dynamically resize when window size changes
- All gauge text elements scale with gauge size

## [2.0.0] - 2024

### Added
- **Mini Monitor Display Support** - Redesigned UI optimized for 1920x480 secondary displays
- **Compact Gauge Controls** - New circular gauge design with needle indicators, glow effects, and digital readouts
- **CPU Temperature Monitoring** - Displays CPU temperature inside the CPU gauge
  - Primary: LibreHardwareMonitor integration
  - Fallback: HWiNFO shared memory support
- **GPU Temperature Monitoring** - Displays GPU temperature inside the GPU usage gauge
  - NVIDIA GPUs via NVML (nvml.dll)
  - AMD GPUs via ADL (atiadlxx.dll)
- **HWiNFO Integration** - Fallback temperature source when LibreHardwareMonitor cannot access sensors
- **Administrator Manifest** - Application now requests admin privileges for hardware sensor access

### Changed
- **Form Layout** - Horizontal 6-gauge layout replacing vertical panels
- **Gauge Design** - Removed square metallic backgrounds, now displays circular dials only
- **Labels** - Moved gauge labels below each dial for cleaner appearance
- **Temperature Display** - Temperature shown inside gauge, below the needle pivot point

### Technical Changes
- Added `CompactGaugeControl.cs` - New gauge control optimized for horizontal displays
- Added `MiniMonitorForm.cs` / `MiniMonitorForm.Designer.cs` - New main form for mini monitor
- Added `HardwareMonitorService.cs` - Wrapper for LibreHardwareMonitor with fallback support
- Added `HWiNFOReader.cs` - Reader for HWiNFO shared memory interface
- Added `app.manifest` - UAC manifest requiring administrator privileges
- Updated `NVMLInterop.cs` - Added temperature reading support
- Updated `ADLInterop.cs` - Added temperature reading support
- Updated `GPUMonitor.cs` - Added temperature properties and methods
- Updated `CPUMonitor.cs` - Added temperature monitoring via HardwareMonitorService

### Dependencies
- Added LibreHardwareMonitorLib project reference
- Updated System.Management to version 10.0.1

## [1.0.0] - Initial Release

### Features
- RAM usage monitoring with progress bar and graph
- CPU usage monitoring with graph
- GPU usage and VRAM monitoring
- Disk I/O monitoring
- Network throughput monitoring
- Process list showing high memory usage applications
- System tray integration
- Dark/Light theme support

---

## Upgrade Notes

### Upgrading to 2.4.0
- Date and time are now displayed on the mini monitor, updating every second
- New FPS gauge control shows real-time frames per second with color-coded quality indicators
- Game activity detection intelligently shows or hides the FPS gauge based on current activity
- FPS display mode menu allows manual control over FPS visibility settings
- Touch cycling feature provides touch-friendly navigation for device selection on mini monitors
- First-run welcome dialog introduced to guide new users through key features

### Upgrading to 2.3.0
- Full touch gesture support for mini monitors with touchscreen capability
- Touch gestures for switching monitors, minimizing, opening device selection, and toggling always-on-top mode
- Visual feedback for gesture actions via toast notifications
- Application now starts minimized to the system tray by default
- FPS monitoring feature to display frames per second from HWiNFO/RTSS
- Branded splash screen and mini monitor intro logo for enhanced branding

### Upgrading to 2.2.0
- New gauge-style application icon and color-coded dynamic tray icon for RAM usage
- Tray icon updates in real-time to reflect current RAM usage
- Experimental support for Intel GPU memory monitoring

### Upgrading to 2.1.0
- Application now runs in borderless full screen mode by default
- Use the system tray icon to access controls (no title bar)
- Press `Escape` to minimize to tray, `F11` to toggle always on top
- Application auto-detects mini monitors with 4:1 aspect ratio

### Upgrading to 2.0.0
- The application now requires **Administrator privileges** to access hardware sensors
- For CPU temperature monitoring, you may need to run **HWiNFO** in the background with "Shared Memory Support" enabled
- The UI has been redesigned for 1920x480 mini monitor displays
