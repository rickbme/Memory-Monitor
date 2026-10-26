using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using Memory_Monitor.Controls;
using Memory_Monitor.Linux;
using System.ComponentModel;
using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// Main window optimized for 1920x480 mini monitor display.
    /// This partial class contains initialization, window management, the right-click menu and display mode switching.
    /// Additional functionality is split into:
    /// - MainWindow.Gauges.cs - Circular gauge updates and device selection
    /// - MainWindow.BarGraph.cs - Bar graph panel updates
    /// </summary>
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
        private AppSettings _settings = new();

        public MainWindow()
        {
            InitializeComponent();

            // The VS Code / IDE previewer runs this constructor just to draw the layout;
            // don't touch hardware, screens or timers there
            if (Design.IsDesignMode)
                return;

            _settings = AppSettings.Load();

            _memoryMonitor = new LinuxMemoryMonitor();
            _cpuMonitor = new LinuxCPUMonitor();
            _gpuMonitor = new LinuxGPUMonitor();
            _diskMonitor = new LinuxDiskMonitor();
            _networkMonitor = new LinuxNetworkMonitor();

            PositionOnTargetMonitor();
            Topmost = _settings.TopMost;

            InitializeGauges();
            ApplyDisplayMode(_settings.DisplayMode);

            _updateTimer.Tick += (_, _) => UpdateAllMetrics();
            _updateTimer.Start();
        }

        private static float GetAutoScale(float currentValue, float peakValue)
        {
            float target = Math.Max(currentValue, peakValue);
            foreach (float scale in SCALE_OPTIONS)
                if (target <= scale * 0.95f) return scale;
            return SCALE_OPTIONS[^1];
        }

        private void UpdateAllMetrics()
        {
            if (_settings.DisplayMode == DisplayMode.BarGraph)
                UpdateBarGraphMetrics();
            else
                UpdateGaugeMetrics();
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

        #region Display Mode

        private void SetDisplayMode(DisplayMode mode)
        {
            if (_settings.DisplayMode == mode)
                return;

            Debug.WriteLine($"Display mode changed: {_settings.DisplayMode} -> {mode}");
            _settings.DisplayMode = mode;
            _settings.Save();
            ApplyDisplayMode(mode);
        }

        private void ApplyDisplayMode(DisplayMode mode)
        {
            gaugeView.IsVisible = mode == DisplayMode.CircularGauges;
            barGraphView.IsVisible = mode == DisplayMode.BarGraph;
            UpdateAllMetrics();
        }

        #endregion

        #region Window Management

        private void PositionOnTargetMonitor()
        {
            Screen? targetScreen = FindTargetMonitor();
            if (targetScreen != null)
                MoveToScreen(targetScreen);
        }

        private void MoveToScreen(Screen screen)
        {
            // Fill the target monitor
            Position = screen.Bounds.Position;
            Width = screen.Bounds.Width / screen.Scaling;
            Height = screen.Bounds.Height / screen.Scaling;

            Debug.WriteLine($"Window positioned on screen at {screen.Bounds}");
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

        private void MoveToNextMonitor()
        {
            var screens = Screens.All;
            if (screens.Count <= 1) return;

            Screen? currentScreen = Screens.ScreenFromWindow(this);
            int currentIndex = currentScreen == null ? -1 : screens.ToList().IndexOf(currentScreen);
            MoveToScreen(screens[(currentIndex + 1) % screens.Count]);
        }

        private void SetTopMost(bool topMost)
        {
            Topmost = topMost;
            _settings.TopMost = topMost;
            _settings.Save();
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
            switch (e.Key)
            {
                case Key.Escape:
                    // Windows hides to the tray here; there's no tray icon on Linux yet, so exit instead
                    Close();
                    e.Handled = true;
                    break;
                case Key.F11:
                    SetTopMost(!Topmost);
                    e.Handled = true;
                    break;
            }
        }

        #endregion

        #region Menu Event Handlers

        private void ContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            // Sync check marks with the current state
            circularGaugesMenuItem.IsChecked = _settings.DisplayMode == DisplayMode.CircularGauges;
            barGraphMenuItem.IsChecked = _settings.DisplayMode == DisplayMode.BarGraph;
            topMostMenuItem.IsChecked = Topmost;
        }

        private void CircularGaugesMenuItem_Click(object? sender, RoutedEventArgs e) => SetDisplayMode(DisplayMode.CircularGauges);

        private void BarGraphMenuItem_Click(object? sender, RoutedEventArgs e) => SetDisplayMode(DisplayMode.BarGraph);

        private void TopMostMenuItem_Click(object? sender, RoutedEventArgs e) => SetTopMost(!Topmost);

        private void MoveToMonitorMenuItem_Click(object? sender, RoutedEventArgs e) => MoveToNextMonitor();

        private void ExitMenuItem_Click(object? sender, RoutedEventArgs e) => Close();

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
