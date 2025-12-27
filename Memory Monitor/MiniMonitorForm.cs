using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Main form optimized for 1920x480 mini monitor display
    /// </summary>
    public partial class MiniMonitorForm : Form
    {
        private const long BYTES_TO_MB = 1024 * 1024;
        private const long BYTES_TO_GB = 1024 * 1024 * 1024;

        private static readonly float[] SCALE_OPTIONS = { 10f, 25f, 50f, 100f, 250f, 500f, 1000f, 2500f, 5000f, 10000f };

        private CPUMonitor? _cpuMonitor;
        private GPUMonitor? _gpuMonitor;
        private DiskMonitor? _diskMonitor;
        private NetworkMonitor? _networkMonitor;

        private float _diskPeakMbps = 0;
        private float _networkPeakMbps = 0;
        private int _lastCpuUsage = 0;
        private int _lastGpuUsage = 0;

        // CPU temperature warning
        private bool _cpuTempWarningShown = false;
        private int _cpuTempCheckCount = 0;
        private const int CPU_TEMP_CHECK_THRESHOLD = 5; // Check for 5 seconds before showing warning

        // For dragging borderless window
        private bool _isDragging = false;
        private Point _dragStartPoint;

        // For dynamic tray icon
        private Icon? _currentTrayIcon;
        private int _lastMemoryPercentage = 0;

        #region Windows API

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX() { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        #endregion

        public MiniMonitorForm()
        {
            InitializeComponent();
            SetupBorderlessWindow();
            InitializeMonitors();
            LoadDeviceSelections();  // Load saved device selections before UI init
            InitializeUI();
            InitializeDeviceSelection();
            InitializeApplicationIcon();
            InitializeTrayIcon();
            ApplyTheme();
        }

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

        private void InitializeMonitors()
        {
            _cpuMonitor = new CPUMonitor();
            _gpuMonitor = new GPUMonitor();
            _diskMonitor = new DiskMonitor();
            _networkMonitor = new NetworkMonitor();
        }

        private void InitializeUI()
        {
            // Set gauge max values
            ramGauge.MaxValue = 100f;
            cpuGauge.MaxValue = 100f;
            diskGauge.MaxValue = 100f;
            networkGauge.MaxValue = 100f;

            if (_gpuMonitor != null && _gpuMonitor.IsMemoryAvailable)
                gpuVramGauge.MaxValue = (float)Math.Ceiling(_gpuMonitor.TotalMemoryGB);
            else
                gpuVramGauge.MaxValue = 24f;

            if (_gpuMonitor != null && _gpuMonitor.IsUsageAvailable)
                gpuUsageGauge.MaxValue = 100f;
            else
                gpuUsageGauge.MaxValue = 100f;

            // Set initial device names on gauges
            UpdateGaugeDeviceNames();

            updateTimer.Start();
            UpdateAllMetrics();
        }

        /// <summary>
        /// Updates the device names displayed on GPU, Disk, and Network gauges
        /// </summary>
        private void UpdateGaugeDeviceNames()
        {
            // GPU gauges - show short GPU name (e.g., "RTX 3060")
            if (_gpuMonitor != null)
            {
                string gpuShortName = _gpuMonitor.GetShortName();
                gpuUsageGauge.DeviceName = gpuShortName;
                gpuVramGauge.DeviceName = gpuShortName;
            }

            // Disk gauge - show selected disk or "All Disks"
            if (_diskMonitor != null)
            {
                diskGauge.DeviceName = _diskMonitor.CurrentDeviceDisplayName;
            }

            // Network gauge - show selected adapter or "All Networks"
            if (_networkMonitor != null)
            {
                networkGauge.DeviceName = _networkMonitor.CurrentDeviceDisplayName;
            }
        }

        /// <summary>
        /// Initialize device selection for selectable gauges
        /// </summary>
        private void InitializeDeviceSelection()
        {
            // GPU gauges - selectable if multiple GPUs
            if (_gpuMonitor != null)
            {
                gpuUsageGauge.IsSelectable = true;
                gpuUsageGauge.HasMultipleDevices = _gpuMonitor.HasMultipleDevices;
                gpuUsageGauge.DeviceSelectionRequested += GpuGauge_DeviceSelectionRequested;

                gpuVramGauge.IsSelectable = true;
                gpuVramGauge.HasMultipleDevices = _gpuMonitor.HasMultipleDevices;
                gpuVramGauge.DeviceSelectionRequested += GpuGauge_DeviceSelectionRequested;
            }

            // Disk gauge - selectable if multiple disks
            if (_diskMonitor != null)
            {
                diskGauge.IsSelectable = true;
                diskGauge.HasMultipleDevices = _diskMonitor.HasMultipleDevices;
                diskGauge.DeviceSelectionRequested += DiskGauge_DeviceSelectionRequested;
            }

            // Network gauge - selectable if multiple adapters
            if (_networkMonitor != null)
            {
                networkGauge.IsSelectable = true;
                networkGauge.HasMultipleDevices = _networkMonitor.HasMultipleDevices;
                networkGauge.DeviceSelectionRequested += NetworkGauge_DeviceSelectionRequested;
            }
        }

        private void GpuGauge_DeviceSelectionRequested(object? sender, EventArgs e)
        {
            if (_gpuMonitor == null || !_gpuMonitor.HasMultipleDevices)
                return;

            var gauge = sender as CompactGaugeControl;
            if (gauge == null) return;

            var (selected, deviceId) = DeviceSelectionForm.ShowNear(
                gauge,
                _gpuMonitor.AvailableDevices,
                _gpuMonitor.SelectedDevice?.Id,
                "Select GPU"
            );

            if (selected)
            {
                _gpuMonitor.SelectDevice(deviceId);
                
                // Update VRAM gauge max value for new GPU
                if (_gpuMonitor.IsMemoryAvailable)
                {
                    gpuVramGauge.MaxValue = (float)Math.Ceiling(_gpuMonitor.TotalMemoryGB);
                }

                // Update device names on both GPU gauges
                string gpuShortName = _gpuMonitor.GetShortName();
                gpuUsageGauge.DeviceName = gpuShortName;
                gpuVramGauge.DeviceName = gpuShortName;

                // Force immediate update
                UpdateGPUUsage();
                UpdateGPUMemory();

                // Save selection
                SaveDeviceSelection("GPU", deviceId);

                Debug.WriteLine($"GPU selected: {_gpuMonitor.CurrentDeviceDisplayName}");
            }
        }

        private void DiskGauge_DeviceSelectionRequested(object? sender, EventArgs e)
        {
            if (_diskMonitor == null || !_diskMonitor.HasMultipleDevices)
                return;

            var gauge = sender as CompactGaugeControl;
            if (gauge == null) return;

            var (selected, deviceId) = DeviceSelectionForm.ShowNear(
                gauge,
                _diskMonitor.AvailableDevices,
                _diskMonitor.SelectedDevice?.Id,
                "Select Disk"
            );

            if (selected)
            {
                _diskMonitor.SelectDevice(deviceId);
                
                // Update device name on disk gauge
                diskGauge.DeviceName = _diskMonitor.CurrentDeviceDisplayName;
                
                // Reset peak for new selection
                _diskPeakMbps = 0;

                // Force immediate update
                UpdateDisk();

                // Save selection
                SaveDeviceSelection("Disk", deviceId);

                Debug.WriteLine($"Disk selected: {_diskMonitor.CurrentDeviceDisplayName}");
            }
        }

        private void NetworkGauge_DeviceSelectionRequested(object? sender, EventArgs e)
        {
            if (_networkMonitor == null || !_networkMonitor.HasMultipleDevices)
                return;

            var gauge = sender as CompactGaugeControl;
            if (gauge == null) return;

            var (selected, deviceId) = DeviceSelectionForm.ShowNear(
                gauge,
                _networkMonitor.AvailableDevices,
                _networkMonitor.SelectedDevice?.Id,
                "Select Network"
            );

            if (selected)
            {
                _networkMonitor.SelectDevice(deviceId);
                
                // Update device name on network gauge
                networkGauge.DeviceName = _networkMonitor.CurrentDeviceDisplayName;
                
                // Reset peak for new selection
                _networkPeakMbps = 0;

                // Force immediate update
                UpdateNetwork();

                // Save selection
                SaveDeviceSelection("Network", deviceId);

                Debug.WriteLine($"Network selected: {_networkMonitor.CurrentDeviceDisplayName}");
            }
        }

        private void SaveDeviceSelection(string monitorType, string? deviceId)
        {
            try
            {
                switch (monitorType)
                {
                    case "GPU":
                        Properties.Settings.Default.SelectedGPUDevice = deviceId ?? "";
                        break;
                    case "Disk":
                        Properties.Settings.Default.SelectedDiskDevice = deviceId ?? "";
                        break;
                    case "Network":
                        Properties.Settings.Default.SelectedNetworkDevice = deviceId ?? "";
                        break;
                }
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save {monitorType} selection: {ex.Message}");
            }
        }

        private void LoadDeviceSelections()
        {
            try
            {
                // Load GPU selection
                if (_gpuMonitor != null)
                {
                    string gpuId = Properties.Settings.Default.SelectedGPUDevice;
                    if (!string.IsNullOrEmpty(gpuId))
                    {
                        _gpuMonitor.SelectDevice(gpuId);
                        Debug.WriteLine($"Loaded GPU selection: {gpuId}");
                    }
                }

                // Load Disk selection
                if (_diskMonitor != null)
                {
                    string diskId = Properties.Settings.Default.SelectedDiskDevice;
                    if (!string.IsNullOrEmpty(diskId))
                    {
                        _diskMonitor.SelectDevice(diskId);
                        Debug.WriteLine($"Loaded Disk selection: {diskId}");
                    }
                }

                // Load Network selection
                if (_networkMonitor != null)
                {
                    string networkId = Properties.Settings.Default.SelectedNetworkDevice;
                    if (!string.IsNullOrEmpty(networkId))
                    {
                        _networkMonitor.SelectDevice(networkId);
                        Debug.WriteLine($"Loaded Network selection: {networkId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load device selections: {ex.Message}");
            }
        }

        private void InitializeApplicationIcon()
        {
            // Load the form's icon from the embedded resource file
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "mmguage.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
                else
                {
                    // Fall back to system icon if file not found
                    this.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                // Fall back to system icon if loading fails
                this.Icon = SystemIcons.Application;
            }
        }

        private void InitializeTrayIcon()
        {
            // Use the static gauge icon initially, will be replaced with dynamic icon on first update
            notifyIcon.Icon = this.Icon ?? SystemIcons.Application;
            UpdateTrayIconText();
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
            // Only update if percentage changed to avoid unnecessary redraws
            if (memoryPercentage == _lastMemoryPercentage && _currentTrayIcon != null)
                return;

            _lastMemoryPercentage = memoryPercentage;

            // Dispose old icon
            var oldIcon = _currentTrayIcon;

            try
            {
                // Create new dynamic icon showing memory usage
                // Use color coding: green (low) -> yellow (medium) -> red (high)
                Color gaugeColor = GaugeIconGenerator.GetColorForPercentage(memoryPercentage);
                _currentTrayIcon = GaugeIconGenerator.CreateDynamicTrayIcon(memoryPercentage, gaugeColor);
                notifyIcon.Icon = _currentTrayIcon;
            }
            catch
            {
                // If icon creation fails, keep the current icon
            }

            // Dispose old icon after setting new one
            oldIcon?.Dispose();
        }

        private void UpdateTrayIconText()
        {
            string text = $"CPU: {_lastCpuUsage}% GPU: {_lastGpuUsage}%";
            notifyIcon.Text = text.Length > 63 ? text.Substring(0, 63) : text;
        }

        private void ApplyTheme()
        {
            this.BackColor = Color.FromArgb(25, 28, 32);

            Color labelColor = Color.FromArgb(200, 200, 200);
            Color textColor = Color.White;

            // Apply colors to gauges
            ramGauge.GaugeColor = Color.FromArgb(58, 150, 221);
            ramGauge.LabelColor = labelColor;
            ramGauge.TextColor = textColor;

            cpuGauge.GaugeColor = Color.FromArgb(220, 50, 50);
            cpuGauge.LabelColor = labelColor;
            cpuGauge.TextColor = textColor;

            gpuUsageGauge.GaugeColor = Color.FromArgb(255, 140, 0);
            gpuUsageGauge.LabelColor = labelColor;
            gpuUsageGauge.TextColor = textColor;

            gpuVramGauge.GaugeColor = Color.FromArgb(160, 90, 240);
            gpuVramGauge.LabelColor = labelColor;
            gpuVramGauge.TextColor = textColor;

            diskGauge.GaugeColor = Color.FromArgb(50, 200, 80);
            diskGauge.LabelColor = labelColor;
            diskGauge.TextColor = textColor;

            networkGauge.GaugeColor = Color.FromArgb(255, 200, 50);
            networkGauge.LabelColor = labelColor;
            networkGauge.TextColor = textColor;
        }

        private float GetAutoScale(float currentValue, float peakValue)
        {
            float target = Math.Max(currentValue, peakValue);
            foreach (float scale in SCALE_OPTIONS)
                if (target <= scale * 0.95f) return scale;
            return SCALE_OPTIONS[^1];
        }

        #region Update Methods

        private void UpdateTimer_Tick(object? sender, EventArgs e) => UpdateAllMetrics();

        private void UpdateAllMetrics()
        {
            // Wrap each update in try-catch to prevent one failure from stopping all updates
            SafeUpdate(UpdateRAM);
            SafeUpdate(UpdateCPU);
            SafeUpdate(UpdateGPUUsage);
            SafeUpdate(UpdateGPUMemory);
            SafeUpdate(UpdateDisk);
            SafeUpdate(UpdateNetwork);
            UpdateTrayIconText();
        }

        /// <summary>
        /// Safely executes an update action, catching and logging any exceptions.
        /// </summary>
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
            try
            {
                var mem = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(mem))
                {
                    ulong used = mem.ullTotalPhys - mem.ullAvailPhys;
                    double usedGB = used / (double)BYTES_TO_GB;
                    double totalGB = mem.ullTotalPhys / (double)BYTES_TO_GB;
                    int pct = (int)((used * 100) / mem.ullTotalPhys);
                    ramGauge.SetValue(pct, $"{usedGB:F1}/{totalGB:F0}GB");
                    
                    // Update dynamic tray icon with memory percentage
                    UpdateTrayIcon(pct);
                }
            }
            catch { ramGauge.SetValue(0, "ERR"); }
        }

        private void UpdateCPU()
        {
            try
            {
                if (_cpuMonitor?.IsAvailable == true)
                {
                    float usage = _cpuMonitor.Update();
                    _lastCpuUsage = (int)Math.Round(usage);
                    
                    // Try to get temperature - only display if we get a valid reading > 0
                    int temp = _cpuMonitor.UpdateTemperature();
                    if (temp > 0)
                    {
                        cpuGauge.SetValue(usage, $"{_lastCpuUsage}%", $"{temp}°C");
                        _cpuTempCheckCount = 0; // Reset counter when temp is available
                    }
                    else
                    {
                        cpuGauge.SetValue(usage, $"{_lastCpuUsage}%");
                        
                        // Check if we should show the warning
                        _cpuTempCheckCount++;
                        if (!_cpuTempWarningShown && _cpuTempCheckCount >= CPU_TEMP_CHECK_THRESHOLD)
                        {
                            ShowCpuTempWarning();
                        }
                    }
                }
                else
                {
                    cpuGauge.SetValue(0, "N/A");
                }
            }
            catch { cpuGauge.SetValue(0, "ERR"); }
        }

        private void ShowCpuTempWarning()
        {
            _cpuTempWarningShown = true;
            
            var warning = new WarningOverlay(
                "? CPU Temperature Not Available",
                "To enable CPU temperature monitoring:\n" +
                "1. Run HWiNFO (download from hwinfo.com)\n" +
                "2. Go to Settings ? Enable 'Shared Memory Support'\n" +
                "3. Keep HWiNFO running in the background"
            );
            
            warning.ShowOn(this);
        }

        private void UpdateGPUUsage()
        {
            try
            {
                if (_gpuMonitor?.IsUsageAvailable == true)
                {
                    float usage = _gpuMonitor.UpdateUsage();
                    _lastGpuUsage = (int)Math.Round(usage);
                    
                    // Also get temperature if available
                    if (_gpuMonitor.IsTemperatureAvailable)
                    {
                        int temp = _gpuMonitor.UpdateTemperature();
                        gpuUsageGauge.SetValue(usage, $"{_lastGpuUsage}%", $"{temp}°C");
                    }
                    else
                    {
                        gpuUsageGauge.SetValue(usage, $"{_lastGpuUsage}%");
                    }
                }
                else
                {
                    gpuUsageGauge.SetValue(0, "N/A");
                }
            }
            catch { gpuUsageGauge.SetValue(0, "ERR"); }
        }

        private void UpdateGPUMemory()
        {
            try
            {
                if (_gpuMonitor?.IsMemoryAvailable == true)
                {
                    _gpuMonitor.UpdateMemory();
                    double usedGB = _gpuMonitor.UsedMemoryGB;
                    double totalGB = _gpuMonitor.TotalMemoryGB;
                    gpuVramGauge.SetValue((float)usedGB, $"{usedGB:F1}/{totalGB:F0}GB");
                }
                else
                {
                    gpuVramGauge.SetValue(0, "N/A");
                }
            }
            catch { gpuVramGauge.SetValue(0, "ERR"); }
        }

        private void UpdateDisk()
        {
            try
            {
                if (_diskMonitor?.IsAvailable == true)
                {
                    var (_, _, totalMbps) = _diskMonitor.Update();
                    _diskPeakMbps = Math.Max(_diskPeakMbps, totalMbps);
                    diskGauge.MaxValue = GetAutoScale(totalMbps, _diskPeakMbps);
                    diskGauge.SetValue(totalMbps, $"{totalMbps:F0}MB/s");
                }
                else
                {
                    diskGauge.SetValue(0, "N/A");
                }
            }
            catch { diskGauge.SetValue(0, "ERR"); }
        }

        private void UpdateNetwork()
        {
            try
            {
                if (_networkMonitor?.IsAvailable == true)
                {
                    var (_, _, totalMbps) = _networkMonitor.Update();
                    _networkPeakMbps = Math.Max(_networkPeakMbps, totalMbps);
                    networkGauge.MaxValue = GetAutoScale(totalMbps, _networkPeakMbps);
                    networkGauge.SetValue(totalMbps, $"{totalMbps:F0}Mbps");
                }
                else
                {
                    networkGauge.SetValue(0, "N/A");
                }
            }
            catch { networkGauge.SetValue(0, "ERR"); }
        }

        #endregion

        #region Event Handlers

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e) => ShowForm();

        private void ShowToolStripMenuItem_Click(object? sender, EventArgs e) => ShowForm();

        private void ExitToolStripMenuItem_Click(object? sender, EventArgs e) => Application.Exit();

        private void MoveToMonitorToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            MoveToNextMonitor();
        }

        private void TopMostToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            this.TopMost = topMostToolStripMenuItem.Checked;
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer.Stop();
            _cpuMonitor?.Dispose();
            _gpuMonitor?.Dispose();
            _diskMonitor?.Dispose();
            _networkMonitor?.Dispose();
            
            // Dispose the dynamic tray icon
            _currentTrayIcon?.Dispose();
            _currentTrayIcon = null;
            
            base.OnFormClosing(e);
        }

        private void MiniMonitorForm_Resize(object? sender, EventArgs e)
        {
            LayoutGauges();
        }

        private void LayoutGauges()
        {
            int formWidth = ClientSize.Width;
            int formHeight = ClientSize.Height;

            // 6 gauges in a row with minimal spacing for larger gauges
            int gaugeCount = 6;
            int marginX = 10;  // Reduced side margins
            int spacing = 5;   // Small gap between gauges
            
            int availableWidth = formWidth - (marginX * 2) - (spacing * (gaugeCount - 1));
            int gaugeWidth = availableWidth / gaugeCount;
            
            // Use more of the vertical space
            int gaugeHeight = (int)(formHeight * 0.95f); // Use 95% of height
            
            // Center vertically
            int startY = (formHeight - gaugeHeight) / 2;
            int startX = marginX;

            ramGauge.Location = new Point(startX, startY);
            ramGauge.Size = new Size(gaugeWidth, gaugeHeight);

            cpuGauge.Location = new Point(startX + gaugeWidth + spacing, startY);
            cpuGauge.Size = new Size(gaugeWidth, gaugeHeight);

            gpuUsageGauge.Location = new Point(startX + (gaugeWidth + spacing) * 2, startY);
            gpuUsageGauge.Size = new Size(gaugeWidth, gaugeHeight);

            gpuVramGauge.Location = new Point(startX + (gaugeWidth + spacing) * 3, startY);
            gpuVramGauge.Size = new Size(gaugeWidth, gaugeHeight);

            diskGauge.Location = new Point(startX + (gaugeWidth + spacing) * 4, startY);
            diskGauge.Size = new Size(gaugeWidth, gaugeHeight);

            networkGauge.Location = new Point(startX + (gaugeWidth + spacing) * 5, startY);
            networkGauge.Size = new Size(gaugeWidth, gaugeHeight);
        }

        #endregion
    }
}
