using Avalonia;
using Avalonia.Controls;
using Memory_Monitor.Controls;
using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// Partial class containing the system tray icon and its menu.
    /// Uses the StatusNotifierItem tray (shown by Ubuntu's AppIndicator extension, KDE, XFCE and others).
    /// </summary>
    public partial class MainWindow
    {
        private TrayIcon? _trayIcon;
        private int _lastTrayPercentage = -1;
        private DisplayMode? _lastTrayDisplayMode;

        private NativeMenuItem? _trayCircularGaugesItem;
        private NativeMenuItem? _trayBarGraphItem;
        private NativeMenuItem? _trayTopMostItem;
        private NativeMenuItem? _trayShowFpsItem;

        private void InitializeTrayIcon()
        {
            // Opened fires again every time the window is shown from the tray; create the icon only once
            if (_trayIcon != null)
                return;

            // Workaround: Avalonia 11's Linux tray copies ToolTipText into the StatusNotifierItem's Status
            // and never sets Status itself. Status must be "Active" for Ubuntu's AppIndicator extension to
            // show the icon, so the tooltip has to be exactly that (GNOME doesn't display tray tooltips)
            _trayIcon = new TrayIcon { ToolTipText = "Active", Menu = BuildTrayMenu() };
            _trayIcon.Clicked += (_, _) => ShowWindow();

            TrayIcon.SetIcons(Application.Current!, new TrayIcons { _trayIcon });
            UpdateTrayMenuState();
        }

        private NativeMenu BuildTrayMenu()
        {
            var showItem = new NativeMenuItem("Show");
            showItem.Click += (_, _) => ShowWindow();

            var moveToMonitorItem = new NativeMenuItem("Move to Next Monitor");
            moveToMonitorItem.Click += (_, _) => MoveToNextMonitor();

            _trayTopMostItem = new NativeMenuItem("Always on Top") { ToggleType = NativeMenuItemToggleType.CheckBox };
            _trayTopMostItem.Click += (_, _) => SetTopMost(!Topmost);

            _trayShowFpsItem = new NativeMenuItem("Show Game FPS (MangoHud)") { ToggleType = NativeMenuItemToggleType.CheckBox };
            _trayShowFpsItem.Click += (_, _) => SetShowFps(!_settings.ShowFps);

            _trayCircularGaugesItem = new NativeMenuItem("Circular Gauges") { ToggleType = NativeMenuItemToggleType.Radio };
            _trayCircularGaugesItem.Click += (_, _) => SetDisplayMode(DisplayMode.CircularGauges);

            _trayBarGraphItem = new NativeMenuItem("Bar Graph") { ToggleType = NativeMenuItemToggleType.Radio };
            _trayBarGraphItem.Click += (_, _) => SetDisplayMode(DisplayMode.BarGraph);

            var displayModeItem = new NativeMenuItem("Display Mode")
            {
                Menu = new NativeMenu { _trayCircularGaugesItem, _trayBarGraphItem }
            };

            var exitItem = new NativeMenuItem("Exit");
            exitItem.Click += (_, _) => Close();

            return new NativeMenu
            {
                showItem,
                new NativeMenuItemSeparator(),
                moveToMonitorItem,
                _trayTopMostItem,
                displayModeItem,
                _trayShowFpsItem,
                new NativeMenuItemSeparator(),
                exitItem
            };
        }

        /// <summary>
        /// Syncs the tray menu check marks with the current state
        /// </summary>
        private void UpdateTrayMenuState()
        {
            if (_trayCircularGaugesItem == null) return;

            _trayCircularGaugesItem.IsChecked = _settings.DisplayMode == DisplayMode.CircularGauges;
            _trayBarGraphItem!.IsChecked = _settings.DisplayMode == DisplayMode.BarGraph;
            _trayTopMostItem!.IsChecked = Topmost;
            _trayShowFpsItem!.IsChecked = _settings.ShowFps;
        }

        private void UpdateTrayIcon(int memoryPercentage)
        {
            if (_trayIcon == null)
                return;

            if (memoryPercentage == _lastTrayPercentage && _settings.DisplayMode == _lastTrayDisplayMode)
                return;

            _lastTrayPercentage = memoryPercentage;
            _lastTrayDisplayMode = _settings.DisplayMode;

            try
            {
                var color = GaugeIconGenerator.GetColorForPercentage(memoryPercentage);
                _trayIcon.Icon = _settings.DisplayMode == DisplayMode.BarGraph
                    ? GaugeIconGenerator.CreateBarGraphTrayIcon(memoryPercentage, color)
                    : GaugeIconGenerator.CreateDynamicTrayIcon(memoryPercentage, color);
            }
            catch (Exception ex)
            {
                // Keep current icon if creation fails
                Debug.WriteLine($"Failed to update tray icon: {ex.Message}");
            }
        }

        /// <summary>
        /// Hides the window to the tray (Escape key), or exits when our icon isn't in a tray to restore it from
        /// </summary>
        private void HideToTray()
        {
            if (IsTrayIconRegistered())
                Hide();
            else
                Close();
        }

        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        /// <summary>
        /// Asks the tray host (e.g. Ubuntu's AppIndicator extension) whether this process's icon is registered.
        /// Avalonia names it "org.kde.StatusNotifierItem-{pid}-{n}".
        /// </summary>
        private static bool IsTrayIconRegistered()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo("gdbus",
                    "call --session --dest org.kde.StatusNotifierWatcher --object-path /StatusNotifierWatcher " +
                    "--method org.freedesktop.DBus.Properties.Get org.kde.StatusNotifierWatcher RegisteredStatusNotifierItems")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
                if (process == null)
                    return false;

                // Read both streams asynchronously so the 2 second timeout also covers a gdbus that hangs
                // (a blocking ReadToEnd would wait for it forever and freeze the UI)
                var outputTask = process.StandardOutput.ReadToEndAsync();
                _ = process.StandardError.ReadToEndAsync();

                if (!process.WaitForExit(2000))
                {
                    Debug.WriteLine("gdbus did not respond, assuming there is no system tray");
                    process.Kill();
                    return false;
                }

                return outputTask.Result.Contains($"org.kde.StatusNotifierItem-{Environment.ProcessId}-");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not check for a system tray: {ex.Message}");
                return false;
            }
        }
    }
}
