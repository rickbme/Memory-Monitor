using Avalonia.Controls;
using Memory_Monitor.Controls;
using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// Partial class containing the circular gauge updates and device selection
    /// </summary>
    public partial class MainWindow
    {
        private float _diskPeakMbps = 0;
        private float _networkPeakMbps = 0;

        private void InitializeGauges()
        {
            gpuVramGauge.MaxValue = _gpuMonitor.IsMemoryAvailable
                ? (float)Math.Ceiling(_gpuMonitor.TotalMemoryGB)
                : 24f;

            gpuUsageGauge.DeviceSelectionRequested += (_, _) => ShowDeviceMenu(gpuUsageGauge, _gpuMonitor);
            gpuVramGauge.DeviceSelectionRequested += (_, _) => ShowDeviceMenu(gpuVramGauge, _gpuMonitor);
            diskGauge.DeviceSelectionRequested += (_, _) => ShowDeviceMenu(diskGauge, _diskMonitor);
            networkGauge.DeviceSelectionRequested += (_, _) => ShowDeviceMenu(networkGauge, _networkMonitor);

            UpdateDeviceNames();
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

        private void UpdateGaugeMetrics()
        {
            SafeUpdate(UpdateRAM);
            SafeUpdate(UpdateCPU);
            SafeUpdate(UpdateGPU);
            SafeUpdate(UpdateDisk);
            SafeUpdate(UpdateNetwork);
            SafeUpdate(UpdateDateTime);
        }

        private void UpdateRAM()
        {
            if (_memoryMonitor.IsAvailable)
            {
                _memoryMonitor.Update();
                ramGauge.SetValue(_memoryMonitor.UsagePercent,
                    $"{_memoryMonitor.UsedMemoryGB:F1}/{_memoryMonitor.TotalMemoryGB:F0}GB");
                UpdateTrayIcon(_memoryMonitor.UsagePercent);
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
    }
}
