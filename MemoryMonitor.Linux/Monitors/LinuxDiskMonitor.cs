using System.Diagnostics;

namespace Memory_Monitor.Linux
{
    /// <summary>
    /// Monitors disk I/O on Linux using /proc/diskstats
    /// </summary>
    public class LinuxDiskMonitor : ISelectableMonitor, IDisposable
    {
        private class DiskDevice
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public long PreviousReadBytes { get; set; } = 0;
            public long PreviousWriteBytes { get; set; } = 0;
            public DateTime PreviousTime { get; set; } = DateTime.Now;
        }

        private List<DiskDevice> _detectedDisks = new List<DiskDevice>();
        private DiskDevice? _selectedDisk = null;
        private List<DeviceInfo> _availableDevices = new List<DeviceInfo>();

        public bool IsAvailable => _selectedDisk != null;
        public double CurrentReadMBps { get; private set; }
        public double CurrentWriteMBps { get; private set; }
        public double CurrentTotalMBps => CurrentReadMBps + CurrentWriteMBps;

        // ISelectableMonitor implementation
        public IReadOnlyList<DeviceInfo> AvailableDevices => _availableDevices;
        public DeviceInfo? SelectedDevice => _selectedDisk != null
            ? _availableDevices.FirstOrDefault(d => d.Id == _selectedDisk.Id)
            : null;
        public bool HasMultipleDevices => _detectedDisks.Count > 1;
        public string CurrentDeviceDisplayName => _selectedDisk?.Name ?? "No Disk";

        public LinuxDiskMonitor()
        {
            DetectDisks();
            BuildDeviceList();
            
            // Select first disk by default
            if (_detectedDisks.Count > 0)
            {
                SelectDevice(_detectedDisks[0].Id);
            }
        }

        private void DetectDisks()
        {
            try
            {
                if (!File.Exists("/proc/diskstats"))
                {
                    Debug.WriteLine("Failed to access /proc/diskstats");
                    return;
                }

                // Read block device information
                var lines = File.ReadAllLines("/proc/diskstats");
                foreach (var line in lines)
                {
                    var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 14)
                    {
                        string deviceName = parts[2];
                        
                        // Physical disks (sda, nvme0n1, vda, etc.) are listed in /sys/block with a
                        // backing device; partitions aren't in /sys/block, and loop/zram/dm have no device
                        if (Directory.Exists($"/sys/block/{deviceName}/device"))
                        {
                            var disk = new DiskDevice
                            {
                                Id = deviceName,
                                Name = deviceName,
                                PreviousTime = DateTime.Now
                            };

                            _detectedDisks.Add(disk);
                            Debug.WriteLine($"Detected disk: {deviceName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error detecting disks: {ex.Message}");
            }
        }

        private void BuildDeviceList()
        {
            _availableDevices.Clear();
            foreach (var disk in _detectedDisks)
            {
                _availableDevices.Add(new DeviceInfo
                {
                    Id = disk.Id,
                    DisplayName = disk.Name,
                    Description = $"Disk: {disk.Name}"
                });
            }
        }

        /// <summary>
        /// Cycles to the next available disk, wrapping around at the end of the list
        /// </summary>
        public DeviceInfo? CycleToNextDevice()
        {
            if (_availableDevices.Count <= 1)
                return SelectedDevice;

            int currentIndex = _availableDevices.FindIndex(d => d.Id == _selectedDisk?.Id);
            var nextDevice = _availableDevices[(currentIndex + 1) % _availableDevices.Count];
            SelectDevice(nextDevice.Id);

            Debug.WriteLine($"Cycled to disk: {CurrentDeviceDisplayName}");
            return SelectedDevice;
        }

        public bool SelectDevice(string? deviceId)
        {
            var disk = _detectedDisks.FirstOrDefault(d => d.Id == deviceId);
            if (disk != null)
            {
                _selectedDisk = disk;
                // Initialize baseline values
                ReadDiskStats(_selectedDisk);
                Debug.WriteLine($"Selected disk: {deviceId}");
                return true;
            }
            return false;
        }

        private bool ReadDiskStats(DiskDevice disk)
        {
            try
            {
                var lines = File.ReadAllLines("/proc/diskstats");
                foreach (var line in lines)
                {
                    var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 14 && parts[2] == disk.Id)
                    {
                        // Field 5: sectors read
                        // Field 9: sectors written
                        // Sector size is typically 512 bytes
                        if (long.TryParse(parts[5], out long sectorsRead) &&
                            long.TryParse(parts[9], out long sectorsWritten))
                        {
                            disk.PreviousReadBytes = sectorsRead * 512;
                            disk.PreviousWriteBytes = sectorsWritten * 512;
                            disk.PreviousTime = DateTime.Now;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading disk stats: {ex.Message}");
            }
            return false;
        }

        public void Update()
        {
            if (_selectedDisk == null)
            {
                CurrentReadMBps = 0;
                CurrentWriteMBps = 0;
                return;
            }

            try
            {
                var lines = File.ReadAllLines("/proc/diskstats");
                foreach (var line in lines)
                {
                    var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 14 && parts[2] == _selectedDisk.Id)
                    {
                        if (long.TryParse(parts[5], out long sectorsRead) &&
                            long.TryParse(parts[9], out long sectorsWritten))
                        {
                            long currentReadBytes = sectorsRead * 512;
                            long currentWriteBytes = sectorsWritten * 512;
                            DateTime currentTime = DateTime.Now;

                            // Calculate time delta in seconds
                            double timeDelta = (currentTime - _selectedDisk.PreviousTime).TotalSeconds;
                            if (timeDelta > 0)
                            {
                                // Calculate bytes per second, then convert to MB/s
                                long readDelta = currentReadBytes - _selectedDisk.PreviousReadBytes;
                                long writeDelta = currentWriteBytes - _selectedDisk.PreviousWriteBytes;

                                CurrentReadMBps = (readDelta / timeDelta) / (1024.0 * 1024.0);
                                CurrentWriteMBps = (writeDelta / timeDelta) / (1024.0 * 1024.0);

                                // Ensure non-negative values
                                CurrentReadMBps = Math.Max(0, CurrentReadMBps);
                                CurrentWriteMBps = Math.Max(0, CurrentWriteMBps);
                            }

                            // Update previous values
                            _selectedDisk.PreviousReadBytes = currentReadBytes;
                            _selectedDisk.PreviousWriteBytes = currentWriteBytes;
                            _selectedDisk.PreviousTime = currentTime;
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating disk stats: {ex.Message}");
            }

            CurrentReadMBps = 0;
            CurrentWriteMBps = 0;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
