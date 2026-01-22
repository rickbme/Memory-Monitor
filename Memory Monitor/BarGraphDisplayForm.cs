using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Alternate display form showing system metrics in a bar graph style.
    /// Designed to match the futuristic dashboard aesthetic with CPU, GPU, Drive, and Network panels.
    /// </summary>
    public partial class BarGraphDisplayForm : Form
    {
        // Hardware monitors (shared with main form via DisplayModeManager)
        private SystemMemoryMonitor? _memoryMonitor;
        private CPUMonitor? _cpuMonitor;
        private GPUMonitor? _gpuMonitor;
        private DiskMonitor? _diskMonitor;
        private NetworkMonitor? _networkMonitor;
        private HWiNFOReader? _hwInfoReader;
        private GameActivityDetector? _gameActivityDetector;

        // Update tracking
        private int _fpsRefreshCounter = 0;
        private const int FPS_SENSOR_REFRESH_INTERVAL = 10;
        private float _diskPeakMbps = 0;
        private float _networkPeakMbps = 0;
        private int _lastCpuUsage = 0;
        private int _lastGpuUsage = 0;

        // For dragging borderless window
        private bool _isDragging = false;
        private Point _dragStartPoint;

        // Tray icon
        private Icon? _currentTrayIcon;
        private int _lastMemoryPercentage = -1;

        // Intro logo display
        private PictureBox? _introLogo;
        private System.Windows.Forms.Timer? _introTimer;
        private const int INTRO_DISPLAY_MS = 2000; // Show logo for 2 seconds

        public BarGraphDisplayForm()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;

            SetupBorderlessWindow();
            InitializeMonitors();
            InitializeGameDetection();
            InitializeApplicationIcon();
            InitializeTrayIcon();
            ApplyTheme();
            UpdateDisplayModeMenuState();
            
            // Initially hide panels for intro (only if logos haven't been shown yet)
            if (!Program.IntroLogosShown)
            {
                SetPanelsVisible(false);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Ensure form is in normal state and active (no need to call Show() - already shown by ApplicationContext)
            this.WindowState = FormWindowState.Normal;
            this.Activate();

            // Show intro logo first, then start the panels (only on first load)
            if (!Program.IntroLogosShown)
            {
                ShowIntroLogo();
            }
            else
            {
                // Skip intro, go straight to panels
                SetPanelsVisible(true);
                LayoutPanels();
                updateTimer.Start();
            }

            Debug.WriteLine("Bar Graph Display started");
        }

        #region Intro Logo

        private void SetPanelsVisible(bool visible)
        {
            cpuPanel.Visible = visible;
            gpuPanel.Visible = visible;
            vramPanel.Visible = visible;
            drivePanel.Visible = visible;
            networkPanel.Visible = visible;
        }

        private void ShowIntroLogo()
        {
            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "dfs_logo_1920.png");

                if (File.Exists(logoPath))
                {
                    using var image = Image.FromFile(logoPath);

                    // Scale to fit the form while maintaining aspect ratio
                    float scaleX = (float)(this.ClientSize.Width - 40) / image.Width;
                    float scaleY = (float)(this.ClientSize.Height - 40) / image.Height;
                    float scale = Math.Min(scaleX, scaleY);

                    int newWidth = (int)(image.Width * scale);
                    int newHeight = (int)(image.Height * scale);

                    // Create a scaled bitmap
                    var scaledImage = new Bitmap(newWidth, newHeight);
                    using (var g = Graphics.FromImage(scaledImage))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.DrawImage(image, 0, 0, newWidth, newHeight);
                    }

                    _introLogo = new PictureBox
                    {
                        Image = scaledImage,
                        SizeMode = PictureBoxSizeMode.AutoSize,
                        BackColor = Color.Transparent,
                        Location = new Point(
                            (this.ClientSize.Width - newWidth) / 2,
                            (this.ClientSize.Height - newHeight) / 2
                        )
                    };

                    this.Controls.Add(_introLogo);
                    _introLogo.BringToFront();

                    Debug.WriteLine("Bar Graph intro logo displayed");
                }

                // Start timer to transition to panels
                _introTimer = new System.Windows.Forms.Timer { Interval = INTRO_DISPLAY_MS };
                _introTimer.Tick += IntroTimer_Tick;
                _introTimer.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show intro logo: {ex.Message}");
                // If logo fails, just show panels immediately
                TransitionToPanels();
            }
        }

        private void IntroTimer_Tick(object? sender, EventArgs e)
        {
            _introTimer?.Stop();
            _introTimer?.Dispose();
            _introTimer = null;

            TransitionToPanels();
        }

        private void TransitionToPanels()
        {
            // Remove intro logo
            if (_introLogo != null)
            {
                this.Controls.Remove(_introLogo);
                _introLogo.Image?.Dispose();
                _introLogo.Dispose();
                _introLogo = null;
            }

            // Mark that intro logos have been shown for this session
            Program.IntroLogosShown = true;

            // Show panels and start updates
            SetPanelsVisible(true);
            LayoutPanels();
            updateTimer.Start();
            UpdateAllMetrics();

            Debug.WriteLine("Transitioned to bar graph panels");
        }

        #endregion

        #region Window Setup

        private void SetupBorderlessWindow()
        {
            Screen targetScreen = FindTargetMonitor();

            this.StartPosition = FormStartPosition.Manual;
            this.Location = targetScreen.Bounds.Location;
            this.Size = targetScreen.Bounds.Size;

            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;

            this.KeyPreview = true;
            this.KeyDown += Form_KeyDown;

            Debug.WriteLine($"Bar Graph window positioned on: {targetScreen.DeviceName} at {targetScreen.Bounds}");
        }

        private Screen FindTargetMonitor()
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                float aspectRatio = (float)screen.Bounds.Width / screen.Bounds.Height;
                if (aspectRatio >= 3.5f)
                {
                    return screen;
                }
            }

            if (Screen.AllScreens.Length > 1)
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (!screen.Primary)
                        return screen;
                }
            }

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
            Debug.WriteLine("Game activity detector initialized for bar graph display");
        }

        #endregion

        #region UI Initialization

        private void InitializeApplicationIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "mmguage.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
                else
                {
                    this.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                this.Icon = SystemIcons.Application;
            }
        }

        private void InitializeTrayIcon()
        {
            // Start with a bar graph style icon
            try
            {
                _currentTrayIcon = GaugeIconGenerator.CreateBarGraphTrayIcon(0, Color.FromArgb(0, 200, 220));
                notifyIcon.Icon = _currentTrayIcon;
            }
            catch
            {
                notifyIcon.Icon = this.Icon ?? SystemIcons.Application;
            }

            notifyIcon.Text = "Memory Monitor - Bar Graph";

            this.Resize += (s, e) =>
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    Hide();
                    ShowInTaskbar = false;
                }
            };
        }

        private void UpdateTrayIcon(int memoryPercentage)
        {
            if (memoryPercentage == _lastMemoryPercentage && _currentTrayIcon != null)
                return;

            _lastMemoryPercentage = memoryPercentage;
            var oldIcon = _currentTrayIcon;

            try
            {
                // Use bar graph style icon with color based on percentage
                Color barColor = GaugeIconGenerator.GetColorForPercentage(memoryPercentage);
                _currentTrayIcon = GaugeIconGenerator.CreateBarGraphTrayIcon(memoryPercentage, barColor);
                notifyIcon.Icon = _currentTrayIcon;
            }
            catch
            {
                // Keep current icon if creation fails
            }

            oldIcon?.Dispose();
        }

        private void UpdateDisplayModeMenuState()
        {
            circularGaugesToolStripMenuItem.Checked = false;
            barGraphToolStripMenuItem.Checked = true;
        }

        private void ApplyTheme()
        {
            this.BackColor = Color.FromArgb(15, 18, 22);

            // CPU Panel - Cyan/Blue theme
            cpuPanel.AccentColor = Color.FromArgb(0, 180, 255);
            cpuPanel.Title = "CPU USAGE";

            // GPU Panel - Green theme
            gpuPanel.AccentColor = Color.FromArgb(100, 200, 80);
            gpuPanel.Title = "GPU USAGE";

            // VRAM Panel - Purple theme (matches circular gauge mode)
            vramPanel.AccentColor = Color.FromArgb(160, 90, 240);
            vramPanel.Title = "VRAM USAGE";

            // Drive Panel - Orange theme
            drivePanel.AccentColor = Color.FromArgb(255, 160, 50);
            drivePanel.Title = "DRIVE USAGE";

            // Network Panel - Cyan theme
            networkPanel.AccentColor = Color.FromArgb(0, 200, 220);
            networkPanel.Title = "NETWORK";
        }

        private void LayoutPanels()
        {
            int formWidth = ClientSize.Width;
            int formHeight = ClientSize.Height;

            int panelCount = 5;
            int marginX = 15;
            int marginY = 10;
            int spacing = 10;

            int availableWidth = formWidth - (marginX * 2) - (spacing * (panelCount - 1));
            int panelWidth = availableWidth / panelCount;
            int panelHeight = formHeight - (marginY * 2);

            cpuPanel.Location = new Point(marginX, marginY);
            cpuPanel.Size = new Size(panelWidth, panelHeight);

            gpuPanel.Location = new Point(marginX + panelWidth + spacing, marginY);
            gpuPanel.Size = new Size(panelWidth, panelHeight);

            vramPanel.Location = new Point(marginX + (panelWidth + spacing) * 2, marginY);
            vramPanel.Size = new Size(panelWidth, panelHeight);

            drivePanel.Location = new Point(marginX + (panelWidth + spacing) * 3, marginY);
            drivePanel.Size = new Size(panelWidth, panelHeight);

            networkPanel.Location = new Point(marginX + (panelWidth + spacing) * 4, marginY);
            networkPanel.Size = new Size(panelWidth, panelHeight);
        }

        #endregion

        #region Update Methods

        private void UpdateTimer_Tick(object? sender, EventArgs e) => UpdateAllMetrics();

        private void UpdateAllMetrics()
        {
            SafeUpdate(UpdateRAM);
            SafeUpdate(UpdateCPU);
            SafeUpdate(UpdateGPU);
            SafeUpdate(UpdateVRAM);
            SafeUpdate(UpdateDrive);
            SafeUpdate(UpdateNetwork);
            SafeUpdate(UpdateTrayIconText);
        }

        private void SafeUpdate(Action updateAction)
        {
            try
            {
                updateAction();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update error in {updateAction.Method.Name}: {ex.Message}");
            }
        }

        private void UpdateRAM()
        {
            if (_memoryMonitor != null)
            {
                int pct = _memoryMonitor.Update();
                UpdateTrayIcon(pct);
            }
        }

        private void UpdateCPU()
        {
            if (_cpuMonitor?.IsAvailable == true)
            {
                float usage = _cpuMonitor.Update();
                _lastCpuUsage = (int)Math.Round(usage);
                int temp = _cpuMonitor.UpdateTemperature();
                
                string tempText = temp > 0 ? $"TEMP: {temp}°C" : "";
                cpuPanel.SetValue(usage, $"{_lastCpuUsage}%", tempText);
            }
            else
            {
                cpuPanel.SetValue(0, "N/A");
            }
        }

        private void UpdateGPU()
        {
            if (_gpuMonitor?.IsUsageAvailable == true)
            {
                float usage = _gpuMonitor.UpdateUsage();
                _lastGpuUsage = (int)Math.Round(usage);
                
                // Get FPS if available
                _fpsRefreshCounter++;
                if (_fpsRefreshCounter >= FPS_SENSOR_REFRESH_INTERVAL)
                {
                    _fpsRefreshCounter = 0;
                    _hwInfoReader?.RefreshSensors();
                }

                int? fps = _hwInfoReader?.IsFpsAvailable == true ? _hwInfoReader?.GetFps() : null;
                bool shouldShowFps = _gameActivityDetector?.ShouldShowFps(_hwInfoReader?.IsFpsAvailable == true, fps, _lastGpuUsage) ?? false;

                string fpsText = shouldShowFps && fps.HasValue && fps.Value > 0 ? $"FPS: {fps.Value}" : "";
                string tempText = "";
                
                if (_gpuMonitor.IsTemperatureAvailable)
                {
                    int temp = _gpuMonitor.UpdateTemperature();
                    tempText = $"TEMP: {temp}°C";
                }

                gpuPanel.SetValue(usage, $"{_lastGpuUsage}%", tempText, fpsText);
            }
            else
            {
                gpuPanel.SetValue(0, "N/A");
            }
        }

        private void UpdateVRAM()
        {
            if (_gpuMonitor?.IsMemoryAvailable == true)
            {
                _gpuMonitor.UpdateMemory();  // Update internal state (returns bytes, not GB)
                double usedGB = _gpuMonitor.UsedMemoryGB;  // Read the GB property
                double totalGB = _gpuMonitor.TotalMemoryGB;
                
                // Calculate percentage
                float percentage = totalGB > 0 ? (float)(usedGB / totalGB * 100) : 0;
                
                // Format the display text
                string valueText = $"{usedGB:F1} GB";
                string capacityText = $"TOTAL: {totalGB:F1} GB";

                vramPanel.MaxValue = 100;
                vramPanel.SetValue(percentage, valueText, capacityText);
            }
            else
            {
                vramPanel.SetValue(0, "N/A");
            }
        }

        private void UpdateDrive()
        {
            if (_diskMonitor?.IsAvailable == true)
            {
                var (readMbps, writeMbps, totalMbps) = _diskMonitor.Update();
                
                // For bar graph, show as percentage of capacity (estimate based on peak)
                _diskPeakMbps = Math.Max(_diskPeakMbps, totalMbps);
                float estimatedMax = GetAutoScale(totalMbps, _diskPeakMbps);
                float percentage = estimatedMax > 0 ? (totalMbps / estimatedMax) * 100 : 0;

                // Show capacity info from first available drive
                string capacityText = "";
                try
                {
                    DriveInfo[] drives = DriveInfo.GetDrives();
                    foreach (var drive in drives)
                    {
                        if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                        {
                            long usedBytes = drive.TotalSize - drive.AvailableFreeSpace;
                            double usedGB = usedBytes / (1024.0 * 1024.0 * 1024.0);
                            double totalGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                            capacityText = $"DISK: {usedGB:F0} GB / {totalGB:F0} GB";
                            break;
                        }
                    }
                }
                catch { }

                drivePanel.MaxValue = 100;
                drivePanel.SetValue(percentage, $"{(int)Math.Round(percentage)}%", capacityText);
            }
            else
            {
                drivePanel.SetValue(0, "N/A");
            }
        }

        private void UpdateNetwork()
        {
            if (_networkMonitor?.IsAvailable == true)
            {
                var (uploadMbps, downloadMbps, totalMbps) = _networkMonitor.Update();
                
                _networkPeakMbps = Math.Max(_networkPeakMbps, Math.Max(uploadMbps, downloadMbps));
                float maxSpeed = GetAutoScale(Math.Max(uploadMbps, downloadMbps), _networkPeakMbps);

                string uploadText = FormatSpeed(uploadMbps);
                string downloadText = FormatSpeed(downloadMbps);

                networkPanel.MaxSpeed = maxSpeed;
                networkPanel.SetSpeeds(uploadMbps, downloadMbps, uploadText, downloadText);
            }
            else
            {
                networkPanel.SetSpeeds(0, 0, "N/A", "N/A");
            }
        }

        private string FormatSpeed(float mbps)
        {
            if (mbps >= 1000)
                return $"{mbps / 1000:F1} GB/s";
            else if (mbps >= 1)
                return $"{mbps:F1} MB/s";
            else
                return $"{mbps * 1000:F0} KB/s";
        }

        private static readonly float[] SCALE_OPTIONS = { 10f, 25f, 50f, 100f, 250f, 500f, 1000f, 2500f, 5000f, 10000f };

        private float GetAutoScale(float currentValue, float peakValue)
        {
            float target = Math.Max(currentValue, peakValue);
            foreach (float scale in SCALE_OPTIONS)
                if (target <= scale * 0.95f) return scale;
            return SCALE_OPTIONS[^1];
        }

        private void UpdateTrayIconText()
        {
            string text = $"CPU: {_lastCpuUsage}% GPU: {_lastGpuUsage}% - Bar Graph";
            notifyIcon.Text = text.Length > 63 ? text.Substring(0, 63) : text;
        }

        #endregion

        #region Input Handlers

        private void Form_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = e.Location;
            }
        }

        private void Form_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point newLocation = this.Location;
                newLocation.X += e.X - _dragStartPoint.X;
                newLocation.Y += e.Y - _dragStartPoint.Y;
                this.Location = newLocation;
            }
        }

        private void Form_MouseUp(object? sender, MouseEventArgs e)
        {
            _isDragging = false;
        }

        private void Form_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.WindowState = FormWindowState.Minimized;
                    this.Hide();
                    this.ShowInTaskbar = false;
                    e.Handled = true;
                    break;

                case Keys.F11:
                    this.TopMost = !this.TopMost;
                    topMostToolStripMenuItem.Checked = this.TopMost;
                    e.Handled = true;
                    break;
            }
        }

        #endregion

        #region Menu Event Handlers

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e) => ShowForm();

        private void ShowToolStripMenuItem_Click(object? sender, EventArgs e) => ShowForm();

        private void ExitToolStripMenuItem_Click(object? sender, EventArgs e) => Application.Exit();

        private void MoveToMonitorToolStripMenuItem_Click(object? sender, EventArgs e) => MoveToNextMonitor();

        private void TopMostToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            this.TopMost = topMostToolStripMenuItem.Checked;
        }

        private void CircularGaugesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            SwitchToCircularGauges();
        }

        private void BarGraphToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            // Already in bar graph mode
            barGraphToolStripMenuItem.Checked = true;
            circularGaugesToolStripMenuItem.Checked = false;
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
        }

        private void SwitchToCircularGauges()
        {
            DisplayModeManager.SetDisplayMode(DisplayMode.CircularGauges);
        }

        private void MoveToNextMonitor()
        {
            if (Screen.AllScreens.Length <= 1) return;

            Screen currentScreen = Screen.FromControl(this);
            int currentIndex = Array.IndexOf(Screen.AllScreens, currentScreen);
            int nextIndex = (currentIndex + 1) % Screen.AllScreens.Length;
            Screen nextScreen = Screen.AllScreens[nextIndex];

            this.Location = nextScreen.Bounds.Location;
            this.Size = nextScreen.Bounds.Size;

            LayoutPanels();

            Debug.WriteLine($"Bar Graph moved to monitor: {nextScreen.DeviceName}");
        }

        private void ShowForm()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = false;
            Activate();
        }

        #endregion

        #region Resize

        private void BarGraphDisplayForm_Resize(object? sender, EventArgs e)
        {
            LayoutPanels();
        }

        #endregion

        #region Cleanup

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer.Stop();
            
            _introTimer?.Stop();
            _introTimer?.Dispose();
            _introTimer = null;

            if (_introLogo != null)
            {
                _introLogo.Image?.Dispose();
                _introLogo.Dispose();
                _introLogo = null;
            }

            _memoryMonitor?.Dispose();
            _cpuMonitor?.Dispose();
            _gpuMonitor?.Dispose();
            _diskMonitor?.Dispose();
            _networkMonitor?.Dispose();
            _hwInfoReader?.Dispose();
            _gameActivityDetector?.Dispose();

            _currentTrayIcon?.Dispose();
            _currentTrayIcon = null;

            base.OnFormClosing(e);
        }

        #endregion
    }
}
