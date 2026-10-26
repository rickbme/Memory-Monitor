namespace Memory_Monitor
{
    /// <summary>
    /// Partial class containing the bar graph panel updates
    /// (Linux counterpart of the Windows BarGraphDisplayForm update methods)
    /// </summary>
    public partial class MainWindow
    {
        private float _barDiskPeakMBps = 0;
        private float _barNetworkPeakMBps = 0;

        #region Update Methods

        private void UpdateBarGraphMetrics()
        {
            SafeUpdate(UpdateMemoryTrayIcon);
            SafeUpdate(UpdateCPUPanel);
            SafeUpdate(UpdateGPUPanels);
            SafeUpdate(UpdateDrivePanel);
            SafeUpdate(UpdateNetworkPanel);
        }

        /// <summary>
        /// The bar graph has no RAM panel; memory only drives the tray icon here
        /// </summary>
        private void UpdateMemoryTrayIcon()
        {
            if (_memoryMonitor.IsAvailable)
            {
                _memoryMonitor.Update();
                UpdateTrayIcon(_memoryMonitor.UsagePercent);
            }
        }

        private void UpdateCPUPanel()
        {
            if (_cpuMonitor.IsAvailable)
            {
                float usage = _cpuMonitor.Update();
                int temp = _cpuMonitor.UpdateTemperature();

                string tempText = temp > 0 ? $"TEMP: {temp}°C" : "";
                cpuPanel.SetValue(usage, $"{Math.Round(usage)}%", tempText);
            }
            else
            {
                cpuPanel.SetValue(0, "N/A");
            }
        }

        private void UpdateGPUPanels()
        {
            if (_gpuMonitor.IsAvailable)
                _gpuMonitor.Update();

            if (_gpuMonitor.IsUsageAvailable)
            {
                float usage = _gpuMonitor.CurrentUsagePercent;
                string tempText = _gpuMonitor.IsTemperatureAvailable
                    ? $"TEMP: {_gpuMonitor.CurrentTemperatureCelsius}°C"
                    : "";
                gpuPanel.SetValue(usage, $"{Math.Round(usage)}%", tempText);
            }
            else
            {
                gpuPanel.SetValue(0, "N/A");
            }

            if (_gpuMonitor.IsMemoryAvailable)
            {
                double usedGB = _gpuMonitor.UsedMemoryGB;
                double totalGB = _gpuMonitor.TotalMemoryGB;
                float percentage = totalGB > 0 ? (float)(usedGB / totalGB * 100) : 0;

                vramPanel.SetValue(percentage, $"{usedGB:F1} GB", $"TOTAL: {totalGB:F1} GB");
            }
            else
            {
                vramPanel.SetValue(0, "N/A");
            }
        }

        private void UpdateDrivePanel()
        {
            if (_diskMonitor.IsAvailable)
            {
                _diskMonitor.Update();
                float totalMBps = (float)_diskMonitor.CurrentTotalMBps;

                // Show activity as a percentage of the auto-scaled peak, like the Windows version
                _barDiskPeakMBps = Math.Max(_barDiskPeakMBps, totalMBps);
                float estimatedMax = GetAutoScale(totalMBps, _barDiskPeakMBps);
                float percentage = estimatedMax > 0 ? (totalMBps / estimatedMax) * 100 : 0;

                drivePanel.SetValue(percentage, $"{(int)Math.Round(percentage)}%", GetRootCapacityText());
            }
            else
            {
                drivePanel.SetValue(0, "N/A");
            }
        }

        /// <summary>
        /// Used / total space of the root filesystem (the Windows version shows the first fixed drive, usually C:)
        /// </summary>
        private static string GetRootCapacityText()
        {
            try
            {
                var root = new DriveInfo("/");
                double usedGB = (root.TotalSize - root.AvailableFreeSpace) / (double)Constants.BYTES_TO_GB;
                double totalGB = root.TotalSize / (double)Constants.BYTES_TO_GB;
                return $"DISK: {usedGB:F0} GB / {totalGB:F0} GB";
            }
            catch
            {
                return "";
            }
        }

        private void UpdateNetworkPanel()
        {
            if (_networkMonitor.IsAvailable)
            {
                _networkMonitor.Update();

                // The Linux monitor reports megabits/s; the panel shows bytes/s (KB/s, MB/s, GB/s)
                float uploadMBps = (float)(_networkMonitor.CurrentUploadMbps / 8);
                float downloadMBps = (float)(_networkMonitor.CurrentDownloadMbps / 8);

                _barNetworkPeakMBps = Math.Max(_barNetworkPeakMBps, Math.Max(uploadMBps, downloadMBps));
                networkPanel.MaxSpeed = GetAutoScale(Math.Max(uploadMBps, downloadMBps), _barNetworkPeakMBps);
                networkPanel.SetSpeeds(uploadMBps, downloadMBps, FormatSpeed(uploadMBps), FormatSpeed(downloadMBps));
            }
            else
            {
                networkPanel.SetSpeeds(0, 0, "N/A", "N/A");
            }
        }

        private static string FormatSpeed(float megabytesPerSecond)
        {
            if (megabytesPerSecond >= 1000)
                return $"{megabytesPerSecond / 1000:F1} GB/s";
            else if (megabytesPerSecond >= 1)
                return $"{megabytesPerSecond:F1} MB/s";
            else
                return $"{megabytesPerSecond * 1000:F0} KB/s";
        }

        #endregion
    }
}
