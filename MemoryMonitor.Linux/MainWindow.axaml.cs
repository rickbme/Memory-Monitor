using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using Memory_Monitor.Controls;
using Memory_Monitor.Linux;
using System.Diagnostics;

namespace Memory_Monitor
{
    public partial class MainWindow : Window
    {
        private static readonly float[] SCALE_OPTIONS = { 10f, 25f, 50f, 100f, 250f, 500f, 1000f, 2500f, 5000f, 10000f };

        // Hardware monitors (created in the constructor, except in the XAML previewer)
        private LinuxMemoryMonitor _memoryMonitor = null!;
        private LinuxCPUMonitor _cpuMonitor = null!;
        private LinuxGPUMonitor _gpuMonitor = null!;
        private LinuxDiskMonitor _diskMonitor = null!;
        private LinuxNetworkMonitor _networkMonitor = null!;

        private readonly DispatcherTimer _updateTimer = new() { Interval = TimeSpan.FromSeconds(1) };

        private float _diskPeakMbps = 0;
        private float _networkPeakMbps = 0;

        public MainWindow()
        {
            InitializeComponent();

            // The VS Code / IDE previewer runs this constructor just to draw the layout;
            // don't touch hardware, screens or timers there
            if (Design.IsDesignMode)
                return;

            _memoryMonitor = new LinuxMemoryMonitor();
            _cpuMonitor = new LinuxCPUMonitor();
            _gpuMonitor = new LinuxGPUMonitor();
            _diskMonitor = new LinuxDiskMonitor();
            _networkMonitor = new LinuxNetworkMonitor();

            PositionOnTargetMonitor();

            gpuVramGauge.MaxValue = _gpuMonitor.IsMemoryAvailable
                ? (float)Math.Ceiling(_gpuMonitor.TotalMemoryGB)
                : 24f;

            gpuUsageGauge.DeviceSelectionRequested += (_, _) => ShowDeviceMenu(gpuUsageGauge, _gpuMonitor);
            gpuVramGauge.DeviceSelectionRequested += (_, _) => ShowDeviceMenu(gpuVramGauge, _gpuMonitor);
            diskGauge.DeviceSelectionRequested += (_, _) => ShowDeviceMenu(diskGauge, _diskMonitor);
            networkGauge.DeviceSelectionRequested += (_, _) => ShowDeviceMenu(networkGauge, _networkMonitor);

            UpdateDeviceNames();
            UpdateAllMetrics();

            _updateTimer.Tick += (_, _) => UpdateAllMetrics();
            _updateTimer.Start();
        }

        #region Window Management

        private void PositionOnTargetMonitor()
        {
            Screen? targetScreen = FindTargetMonitor();
            if (targetScreen == null)
                return;

            // Fill the target monitor
            Position = targetScreen.Bounds.Position;
            Width = targetScreen.Bounds.Width / targetScreen.Scaling;
            Height = targetScreen.Bounds.Height / targetScreen.Scaling;

            Debug.WriteLine($"Window positioned on screen at {targetScreen.Bounds}");
        }

        private Screen? FindTargetMonitor()
        {
            // Look for a monitor that matches mini monitor dimensions (wide and short)
            foreach (Screen screen in Screens.All)
            {
                // Check for typical mini monitor aspect ratios (4:1 or wider)
                double aspectRatio = (double)screen.Bounds.Width / screen.Bounds.Height;

                if (aspectRatio >= 3.5) // Very wide aspect ratio like 1920x480 (4:1)
                {
                    Debug.WriteLine($"Found mini monitor: {screen.Bounds.Width}x{screen.Bounds.Height}");
                    return screen;
                }
            }

            // If no mini monitor found, use secondary monitor if available, else the primary
            return Screens.All.FirstOrDefault(s => !s.IsPrimary) ?? Screens.Primary;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            // Drag the borderless window, except when clicking a gauge's device menu
            if (e.Handled || e.Source is GaugeControl { IsSelectable: true, HasMultipleDevices: true })
                return;

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                WindowState = WindowState.Minimized;
                e.Handled = true;
            }
        }

        #endregion

        private static float GetAutoScale(float currentValue, float peakValue)
        {
            float target = Math.Max(currentValue, peakValue);
            foreach (float scale in SCALE_OPTIONS)
                if (target <= scale * 0.95f) return scale;
            return SCALE_OPTIONS[^1];
        }

        #region Device Selection

        private void UpdateDeviceNames()
        {
            gpuUsageGauge.HasMultipleDevices = _gpuMonitor.HasMultipleDevices;
            gpuVramGauge.HasMultipleDevices = _gpuMonitor.HasMultipleDevices;
            diskGauge.HasMultipleDevices = _diskMonitor.HasMultipleDevices;
            networkGauge.HasMultipleDevices = _networkMonitor.HasMultipleDevices;

            string gpuShortName = _gpuMonitor.IsAvailable ? _gpuMonitor.GetShortName() : "";
            gpuUsageGauge.DeviceName = gpuShortName;
            gpuVramGauge.DeviceName = gpuShortName;
            diskGauge.DeviceName = _diskMonitor.IsAvailable ? _diskMonitor.CurrentDeviceDisplayName : "";
            networkGauge.DeviceName = _networkMonitor.IsAvailable ? _networkMonitor.CurrentDeviceDisplayName : "";
        }

        private void ShowDeviceMenu(GaugeControl gauge, ISelectableMonitor monitor)
        {
            var menu = new MenuFlyout();
            foreach (var device in monitor.AvailableDevices)
            {
                var item = new MenuItem
                {
                    Header = device.DisplayName,
                    Icon = device.Id == monitor.SelectedDevice?.Id ? new TextBlock { Text = "✓" } : null
                };
                item.Click += (_, _) => SelectDevice(monitor, device.Id);
                menu.Items.Add(item);
            }
            menu.ShowAt(gauge);
        }

        private void SelectDevice(ISelectableMonitor monitor, string deviceId)
        {
            if (!monitor.SelectDevice(deviceId))
                return;

            if (monitor == _gpuMonitor && _gpuMonitor.IsMemoryAvailable)
                gpuVramGauge.MaxValue = (float)Math.Ceiling(_gpuMonitor.TotalMemoryGB);
            else if (monitor == _diskMonitor)
                _diskPeakMbps = 0;
            else if (monitor == _networkMonitor)
                _networkPeakMbps = 0;

            UpdateDeviceNames();
            UpdateAllMetrics();
            Debug.WriteLine($"Selected device: {monitor.CurrentDeviceDisplayName}");
        }

        #endregion

        #region Update Methods

        private void UpdateAllMetrics()
        {
            SafeUpdate(UpdateRAM);
            SafeUpdate(UpdateCPU);
            SafeUpdate(UpdateGPU);
            SafeUpdate(UpdateDisk);
            SafeUpdate(UpdateNetwork);
            SafeUpdate(UpdateDateTime);
        }

        private static void SafeUpdate(Action updateAction)
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
            if (_memoryMonitor.IsAvailable)
            {
                _memoryMonitor.Update();
                ramGauge.SetValue(_memoryMonitor.UsagePercent,
                    $"{_memoryMonitor.UsedMemoryGB:F1}/{_memoryMonitor.TotalMemoryGB:F0}GB");
            }
            else
            {
                ramGauge.SetValue(0, "N/A");
            }
        }

        private void UpdateCPU()
        {
            if (_cpuMonitor.IsAvailable)
            {
                float usage = _cpuMonitor.Update();
                int temp = _cpuMonitor.UpdateTemperature();
                if (temp > 0)
                    cpuGauge.SetValue(usage, $"{Math.Round(usage)}%", $"{temp}°C");
                else
                    cpuGauge.SetValue(usage, $"{Math.Round(usage)}%");
            }
            else
            {
                cpuGauge.SetValue(0, "N/A");
            }
        }

        private void UpdateGPU()
        {
            if (_gpuMonitor.IsAvailable)
                _gpuMonitor.Update();

            if (_gpuMonitor.IsUsageAvailable)
            {
                float usage = _gpuMonitor.CurrentUsagePercent;
                if (_gpuMonitor.IsTemperatureAvailable)
                    gpuUsageGauge.SetValue(usage, $"{Math.Round(usage)}%", $"{_gpuMonitor.CurrentTemperatureCelsius}°C");
                else
                    gpuUsageGauge.SetValue(usage, $"{Math.Round(usage)}%");
            }
            else
            {
                gpuUsageGauge.SetValue(0, "N/A");
            }

            if (_gpuMonitor.IsMemoryAvailable)
            {
                double usedGB = _gpuMonitor.UsedMemoryGB;
                double totalGB = _gpuMonitor.TotalMemoryGB;
                gpuVramGauge.SetValue((float)usedGB, $"{usedGB:F1}/{totalGB:F0}GB");
            }
            else
            {
                gpuVramGauge.SetValue(0, "N/A");
            }
        }

        private void UpdateDisk()
        {
            if (_diskMonitor.IsAvailable)
            {
                _diskMonitor.Update();
                float totalMbps = (float)_diskMonitor.CurrentTotalMBps;
                _diskPeakMbps = Math.Max(_diskPeakMbps, totalMbps);
                diskGauge.MaxValue = GetAutoScale(totalMbps, _diskPeakMbps);
                diskGauge.SetValue(totalMbps, $"{totalMbps:F0}MB/s");
            }
            else
            {
                diskGauge.SetValue(0, "N/A");
            }
        }

        private void UpdateNetwork()
        {
            if (_networkMonitor.IsAvailable)
            {
                _networkMonitor.Update();
                float totalMbps = (float)_networkMonitor.CurrentTotalMbps;
                _networkPeakMbps = Math.Max(_networkPeakMbps, totalMbps);
                networkGauge.MaxValue = GetAutoScale(totalMbps, _networkPeakMbps);
                networkGauge.SetValue(totalMbps, $"{totalMbps:F0}Mbps");
            }
            else
            {
                networkGauge.SetValue(0, "N/A");
            }
        }

        private void UpdateDateTime()
        {
            DateTime now = DateTime.Now;

            // 12-hour format without seconds (e.g., "3:45 PM") and month/day (e.g., "January 15")
            lblTime.Text = now.ToString("h:mm tt");
            lblDate.Text = now.ToString("MMMM d");
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            _updateTimer.Stop();

            _cpuMonitor?.Dispose();
            _memoryMonitor?.Dispose();
            _gpuMonitor?.Dispose();
            _diskMonitor?.Dispose();
            _networkMonitor?.Dispose();

            base.OnClosed(e);
        }
    }
}
