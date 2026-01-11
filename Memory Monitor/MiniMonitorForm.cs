using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Main form optimized for 1920x480 mini monitor display.
    /// This partial class contains core initialization, window management, and event handlers.
    /// Additional functionality is split into:
    /// - MiniMonitorForm.Monitors.cs - Metric update methods and shared fields
    /// - MiniMonitorForm.DeviceSelection.cs - Device selection logic  
    /// - MiniMonitorForm.Layout.cs - UI layout, theming, and toast notifications
    /// </summary>
    public partial class MiniMonitorForm : Form
    {
        // Hardware monitors (defined here, used in other partial classes)
        private SystemMemoryMonitor? _memoryMonitor;
        private CPUMonitor? _cpuMonitor;
        private GPUMonitor? _gpuMonitor;
        private DiskMonitor? _diskMonitor;
        private NetworkMonitor? _networkMonitor;
        private HWiNFOReader? _hwInfoReader;

        // Game activity detection for FPS display
        private GameActivityDetector? _gameActivityDetector;

        // Touch gesture support
        private TouchGestureHandler? _touchHandler;

        // For dragging borderless window
        private bool _isDragging = false;
        private Point _dragStartPoint;

        public MiniMonitorForm()
        {
            InitializeComponent();

            this.ShowInTaskbar = false;

            SetupBorderlessWindow();
            InitializeMonitors();
            InitializeGameDetection();
            LoadDeviceSelections();
            InitializeUI();
            InitializeDeviceSelection();
            InitializeApplicationIcon();
            InitializeTrayIcon();
            InitializeTouchSupport();
            InitializeDateTimeDisplay();
            ApplyTheme();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.WindowState = FormWindowState.Normal;
            this.Show();
            this.Activate();

            Debug.WriteLine("Application started - window visible, tray icon active");

            // Show welcome dialog on first run
            this.BeginInvoke(new Action(() =>
            {
                WelcomeDialog.ShowIfFirstRun();
            }));
        }

        #region Window Setup

        private void SetupBorderlessWindow()
        {
            // Find the target monitor (prefer secondary monitor with 1920x480 or similar)
            Screen targetScreen = FindTargetMonitor();
            
            // Position window to fill the target monitor
            this.StartPosition = FormStartPosition.Manual;
            this.Location = targetScreen.Bounds.Location;
            this.Size = targetScreen.Bounds.Size;
            
            // Enable mouse events for dragging
            this.MouseDown += MiniMonitorForm_MouseDown;
            this.MouseMove += MiniMonitorForm_MouseMove;
            this.MouseUp += MiniMonitorForm_MouseUp;
            
            // Add keyboard shortcut to close (Escape key)
            this.KeyPreview = true;
            this.KeyDown += MiniMonitorForm_KeyDown;

            Debug.WriteLine($"Window positioned on: {targetScreen.DeviceName} at {targetScreen.Bounds}");
        }

        private Screen FindTargetMonitor()
        {
            // Look for a monitor that matches mini monitor dimensions (wide and short)
            foreach (Screen screen in Screen.AllScreens)
            {
                // Check for typical mini monitor aspect ratios (4:1 or wider)
                float aspectRatio = (float)screen.Bounds.Width / screen.Bounds.Height;
                
                if (aspectRatio >= 3.5f) // Very wide aspect ratio like 1920x480 (4:1)
                {
                    Debug.WriteLine($"Found mini monitor: {screen.DeviceName} - {screen.Bounds.Width}x{screen.Bounds.Height}");
                    return screen;
                }
            }

            // If no mini monitor found, use secondary monitor if available
            if (Screen.AllScreens.Length > 1)
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (!screen.Primary)
                    {
                        Debug.WriteLine($"Using secondary monitor: {screen.DeviceName}");
                        return screen;
                    }
                }
            }

            // Fall back to primary monitor
            Debug.WriteLine("Using primary monitor");
            return Screen.PrimaryScreen ?? Screen.AllScreens[0];
        }

        #endregion

        #region Monitor Initialization

        private void InitializeMonitors()
        {
            _memoryMonitor = new SystemMemoryMonitor();
            _cpuMonitor = new CPUMonitor();
            _gpuMonitor = new GPUMonitor();
            _diskMonitor = new DiskMonitor();
            _networkMonitor = new NetworkMonitor();
            
            // Initialize HWiNFO reader for FPS monitoring
            try
            {
                _hwInfoReader = new HWiNFOReader();
                if (_hwInfoReader.IsAvailable)
                {
                    Debug.WriteLine($"HWiNFO connected - FPS available: {_hwInfoReader.IsFpsAvailable}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize HWiNFO reader: {ex.Message}");
                _hwInfoReader = null;
            }
        }

        private void InitializeGameDetection()
        {
            _gameActivityDetector = new GameActivityDetector(updateTimer.Interval);
            Debug.WriteLine("Game activity detector initialized");
        }

        #endregion

        #region Touch Support

        private void InitializeTouchSupport()
        {
            try
            {
                _touchHandler = new TouchGestureHandler(this);

                if (_touchHandler.IsTouchAvailable)
                {
                    // Handle horizontal swipes to switch monitors
                    _touchHandler.SwipeDetected += TouchHandler_SwipeDetected;

                    // Handle taps on gauges for device selection
                    _touchHandler.TapDetected += TouchHandler_TapDetected;

                    // Handle long press to show context menu
                    _touchHandler.LongPressDetected += TouchHandler_LongPressDetected;

                    // Handle two-finger tap to toggle always-on-top
                    _touchHandler.TwoFingerTapDetected += TouchHandler_TwoFingerTapDetected;

                    Debug.WriteLine("Touch gesture support initialized successfully");
                }
                else
                {
                    Debug.WriteLine("Touch input not available - running without touch support");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize touch support: {ex.Message}");
                // Continue without touch support - application works fine with mouse/keyboard
            }
        }

        /// <summary>
        /// Process Windows messages for touch input
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            // Let touch handler process touch/gesture messages first
            if (_touchHandler?.ProcessMessage(ref m) == true)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
        }

        private void TouchHandler_SwipeDetected(object? sender, SwipeEventArgs e)
        {
            Debug.WriteLine($"Swipe detected: {e.Direction}, distance: {e.Distance}px");

            switch (e.Direction)
            {
                case SwipeDirection.Left:
                case SwipeDirection.Right:
                    // Swipe horizontally to switch monitors
                    MoveToNextMonitor();
                    string arrow = e.Direction == SwipeDirection.Left ? "?" : "?";
                    ShowToastNotification($"{arrow} Monitor switched");
                    break;

                case SwipeDirection.Down:
                    // Swipe down to minimize to tray
                    this.WindowState = FormWindowState.Minimized;
                    this.Hide();
                    this.ShowInTaskbar = false;
                    break;

                case SwipeDirection.Up:
                    // Swipe up could restore if minimized (for future use)
                    break;
            }
        }

        private void TouchHandler_TapDetected(object? sender, TapEventArgs e)
        {
            Debug.WriteLine($"Tap detected at {e.Location}, control: {e.TappedControl?.Name ?? "none"}");

            // If tapped on a gauge, trigger device selection
            if (e.TappedControl is CompactGaugeControl gauge)
            {
                gauge.PerformClick();
            }
        }

        private void TouchHandler_LongPressDetected(object? sender, LongPressEventArgs e)
        {
            Debug.WriteLine($"Long press detected at {e.Location}");

            // Show context menu at touch location
            Point screenPoint = this.PointToScreen(e.Location);
            trayContextMenu.Show(screenPoint);
        }

        private void TouchHandler_TwoFingerTapDetected(object? sender, PointEventArgs e)
        {
            Debug.WriteLine($"Two-finger tap detected at {e.Location}");

            // Toggle always-on-top
            this.TopMost = !this.TopMost;
            topMostToolStripMenuItem.Checked = this.TopMost;

            // Show visual feedback
            ShowToastNotification(this.TopMost ? "Always on Top: ON" : "Always on Top: OFF");
        }

        #endregion

        #region Input Handlers

        private void MiniMonitorForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = e.Location;
            }
        }

        private void MiniMonitorForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point newLocation = this.Location;
                newLocation.X += e.X - _dragStartPoint.X;
                newLocation.Y += e.Y - _dragStartPoint.Y;
                this.Location = newLocation;
            }
        }

        private void MiniMonitorForm_MouseUp(object? sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void MiniMonitorForm_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    // Minimize to tray on Escape
                    this.WindowState = FormWindowState.Minimized;
                    this.Hide();
                    this.ShowInTaskbar = false;
                    e.Handled = true;
                    break;
                    
                case Keys.F11:
                    // Toggle TopMost
                    this.TopMost = !this.TopMost;
                    e.Handled = true;
                    break;
            }
        }

        #endregion

        #region Menu and Tray Event Handlers

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e) => ShowForm();

        private void ShowToolStripMenuItem_Click(object? sender, EventArgs e) => ShowForm();

        private void ExitToolStripMenuItem_Click(object? sender, EventArgs e) => Application.Exit();

        private void MoveToMonitorToolStripMenuItem_Click(object? sender, EventArgs e) => MoveToNextMonitor();

        private void TopMostToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            this.TopMost = topMostToolStripMenuItem.Checked;
        }

        private void FpsAutoDetectToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            SetFpsDisplayMode(GameActivityDetector.FpsDisplayMode.AutoDetect);
        }

        private void FpsAlwaysShowToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            SetFpsDisplayMode(GameActivityDetector.FpsDisplayMode.AlwaysShow);
        }

        private void FpsAlwaysHideToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            SetFpsDisplayMode(GameActivityDetector.FpsDisplayMode.AlwaysHide);
        }

        private void SetFpsDisplayMode(GameActivityDetector.FpsDisplayMode mode)
        {
            if (_gameActivityDetector != null)
            {
                _gameActivityDetector.DisplayMode = mode;
            }

            // Update menu checkmarks
            fpsAutoDetectToolStripMenuItem.Checked = mode == GameActivityDetector.FpsDisplayMode.AutoDetect;
            fpsAlwaysShowToolStripMenuItem.Checked = mode == GameActivityDetector.FpsDisplayMode.AlwaysShow;
            fpsAlwaysHideToolStripMenuItem.Checked = mode == GameActivityDetector.FpsDisplayMode.AlwaysHide;

            // Show feedback
            string modeName = mode switch
            {
                GameActivityDetector.FpsDisplayMode.AutoDetect => "Auto-detect",
                GameActivityDetector.FpsDisplayMode.AlwaysShow => "Always Show",
                GameActivityDetector.FpsDisplayMode.AlwaysHide => "Always Hide",
                _ => mode.ToString()
            };
            ShowToastNotification($"FPS Display: {modeName}");
        }

        private void MoveToNextMonitor()
        {
            if (Screen.AllScreens.Length <= 1) return;

            // Find current screen
            Screen currentScreen = Screen.FromControl(this);
            int currentIndex = Array.IndexOf(Screen.AllScreens, currentScreen);
            
            // Move to next screen (wrap around)
            int nextIndex = (currentIndex + 1) % Screen.AllScreens.Length;
            Screen nextScreen = Screen.AllScreens[nextIndex];

            // Position and resize to fill the next screen
            this.Location = nextScreen.Bounds.Location;
            this.Size = nextScreen.Bounds.Size;
            
            // Re-layout gauges for new size
            LayoutGauges();

            Debug.WriteLine($"Moved to monitor: {nextScreen.DeviceName} at {nextScreen.Bounds}");
        }

        private void ShowForm()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = false; // Keep out of taskbar for borderless mode
            Activate();
        }

        #endregion

        #region Cleanup

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer.Stop();

            _memoryMonitor?.Dispose();
            _cpuMonitor?.Dispose();
            _gpuMonitor?.Dispose();
            _diskMonitor?.Dispose();
            _networkMonitor?.Dispose();
            _hwInfoReader?.Dispose();
            _touchHandler?.Dispose();
            _gameActivityDetector?.Dispose();

            _currentTrayIcon?.Dispose();
            _currentTrayIcon = null;

            base.OnFormClosing(e);
        }

        #endregion
    }
}
