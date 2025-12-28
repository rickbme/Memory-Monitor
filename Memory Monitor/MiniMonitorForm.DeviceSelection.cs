using System;
using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// Partial class containing device selection logic
    /// </summary>
    public partial class MiniMonitorForm
    {
        /// <summary>
        /// Updates the device names displayed on GPU, Disk, and Network gauges
        /// </summary>
        private void UpdateGaugeDeviceNames()
        {
            if (_gpuMonitor != null)
            {
                string gpuShortName = _gpuMonitor.GetShortName();
                gpuUsageGauge.DeviceName = gpuShortName;
                gpuVramGauge.DeviceName = gpuShortName;
            }

            if (_diskMonitor != null)
            {
                diskGauge.DeviceName = _diskMonitor.CurrentDeviceDisplayName;
            }

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
            if (_gpuMonitor != null)
            {
                gpuUsageGauge.IsSelectable = true;
                gpuUsageGauge.HasMultipleDevices = _gpuMonitor.HasMultipleDevices;
                gpuUsageGauge.DeviceSelectionRequested += GpuGauge_DeviceSelectionRequested;

                gpuVramGauge.IsSelectable = true;
                gpuVramGauge.HasMultipleDevices = _gpuMonitor.HasMultipleDevices;
                gpuVramGauge.DeviceSelectionRequested += GpuGauge_DeviceSelectionRequested;
            }

            if (_diskMonitor != null)
            {
                diskGauge.IsSelectable = true;
                diskGauge.HasMultipleDevices = _diskMonitor.HasMultipleDevices;
                diskGauge.DeviceSelectionRequested += DiskGauge_DeviceSelectionRequested;
            }

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

                if (_gpuMonitor.IsMemoryAvailable)
                {
                    gpuVramGauge.MaxValue = (float)Math.Ceiling(_gpuMonitor.TotalMemoryGB);
                }

                string gpuShortName = _gpuMonitor.GetShortName();
                gpuUsageGauge.DeviceName = gpuShortName;
                gpuVramGauge.DeviceName = gpuShortName;

                UpdateGPUUsage();
                UpdateGPUMemory();

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
                diskGauge.DeviceName = _diskMonitor.CurrentDeviceDisplayName;
                _diskPeakMbps = 0;
                UpdateDisk();
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
                networkGauge.DeviceName = _networkMonitor.CurrentDeviceDisplayName;
                _networkPeakMbps = 0;
                UpdateNetwork();
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
                if (_gpuMonitor != null)
                {
                    string gpuId = Properties.Settings.Default.SelectedGPUDevice;
                    if (!string.IsNullOrEmpty(gpuId))
                    {
                        _gpuMonitor.SelectDevice(gpuId);
                        Debug.WriteLine($"Loaded GPU selection: {gpuId}");
                    }
                }

                if (_diskMonitor != null)
                {
                    string diskId = Properties.Settings.Default.SelectedDiskDevice;
                    if (!string.IsNullOrEmpty(diskId))
                    {
                        _diskMonitor.SelectDevice(diskId);
                        Debug.WriteLine($"Loaded Disk selection: {diskId}");
                    }
                }

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
    }
}
