using System.Diagnostics;

namespace Memory_Monitor
{
    public class DiskMonitor : ISelectableMonitor
    {
        private class DiskCounters
        {
            public string Id { get; set; } = "";
            public string DiskName { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public PerformanceCounter? ReadCounter { get; set; }
            public PerformanceCounter? WriteCounter { get; set; }
            public float LastReadMbps { get; set; }
            public float LastWriteMbps { get; set; }
        }

        private List<DiskCounters> _allDisks = new List<DiskCounters>();
        private string? _selectedDiskId = null; // null = aggregate all disks
        private List<DeviceInfo> _availableDevices = new List<DeviceInfo>();
        private bool _isAvailable;

        public bool IsAvailable => _isAvailable;
        public float TotalReadMbps { get; private set; }
        public float TotalWriteMbps { get; private set; }
        public float TotalThroughputMbps => TotalReadMbps + TotalWriteMbps;

        // ISelectableMonitor implementation
        public IReadOnlyList<DeviceInfo> AvailableDevices => _availableDevices;
        public DeviceInfo? SelectedDevice => _selectedDiskId != null 
            ? _availableDevices.FirstOrDefault(d => d.Id == _selectedDiskId) 
            : _availableDevices.FirstOrDefault(d => d.Type == DeviceType.Aggregate);
        public bool HasMultipleDevices => _availableDevices.Count > 1; // True if we have choices (All + specific devices)
        public string CurrentDeviceDisplayName => _selectedDiskId != null
            ? _allDisks.FirstOrDefault(d => d.Id == _selectedDiskId)?.DisplayName ?? "Unknown Disk"
            : "All Disks";

        public DiskMonitor()
        {
            Initialize();
            BuildDeviceList();
        }

        private void Initialize()
        {
            try
            {
                PerformanceCounterCategory category = new PerformanceCounterCategory("PhysicalDisk");
                string[] instanceNames = category.GetInstanceNames();

                Debug.WriteLine($"Found {instanceNames.Length} PhysicalDisk instances");

                int diskIndex = 0;
                foreach (string instanceName in instanceNames)
                {
                    if (instanceName == "_Total" || string.IsNullOrWhiteSpace(instanceName))
                        continue;

                    try
                    {
                        // Parse disk name - format is usually "0 C: D:" or "1 E:"
                        string displayName = ParseDiskDisplayName(instanceName);
                        
                        var diskCounter = new DiskCounters
                        {
                            Id = $"disk_{diskIndex}_{instanceName.GetHashCode():X8}",
                            DiskName = instanceName,
                            DisplayName = displayName,
                            ReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", instanceName),
                            WriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", instanceName)
                        };

                        diskCounter.ReadCounter.NextValue();
                        diskCounter.WriteCounter.NextValue();

                        _allDisks.Add(diskCounter);
                        Debug.WriteLine($"Initialized disk monitor for: {instanceName} -> {displayName}");
                        diskIndex++;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to initialize disk monitor for {instanceName}: {ex.Message}");
                    }
                }

                _isAvailable = _allDisks.Count > 0;
                
                if (_isAvailable)
                {
                    Debug.WriteLine($"Disk monitor initialized successfully with {_allDisks.Count} disk(s)");
                }
                else
                {
                    Debug.WriteLine("No disk counters available");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize disk monitor: {ex.Message}");
                _isAvailable = false;
            }
        }

        private string ParseDiskDisplayName(string instanceName)
        {
            // Instance name format: "0 C: D:" or "1 E:" etc.
            // Extract drive letters if present
            var parts = instanceName.Split(' ');
            if (parts.Length > 1)
            {
                string driveLetters = string.Join(" ", parts.Skip(1));
                return $"Disk {parts[0]} ({driveLetters.Trim()})";
            }
            return $"Disk {instanceName}";
        }

        private void BuildDeviceList()
        {
            _availableDevices.Clear();

            // Always add "All Disks" aggregate option (even for single disk)
            // This allows users to switch back to aggregate mode after selecting a specific disk
            _availableDevices.Add(new DeviceInfo
            {
                Id = "",
                DisplayName = _allDisks.Count > 1 ? "All Disks" : "All Disks",
                ShortName = "All",
                Description = _allDisks.Count > 1 
                    ? $"Combined throughput from {_allDisks.Count} disks" 
                    : "Total disk throughput",
                Type = DeviceType.Aggregate,
                IsActive = true
            });

            // Add individual disks
            foreach (var disk in _allDisks)
            {
                _availableDevices.Add(new DeviceInfo
                {
                    Id = disk.Id,
                    DisplayName = disk.DisplayName,
                    ShortName = GetShortDiskName(disk.DisplayName),
                    Description = disk.DiskName,
                    Type = DeviceType.Disk,
                    IsActive = true
                });
            }
        }

        private string GetShortDiskName(string displayName)
        {
            // Extract just drive letters for short name
            int parenStart = displayName.IndexOf('(');
            int parenEnd = displayName.IndexOf(')');
            if (parenStart >= 0 && parenEnd > parenStart)
            {
                return displayName.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();
            }
            return displayName;
        }

        public bool SelectDevice(string? deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                // Aggregate mode - use all disks
                _selectedDiskId = null;
                Debug.WriteLine("Selected: All Disks (aggregate)");
                return true;
            }

            var disk = _allDisks.FirstOrDefault(d => d.Id == deviceId);
            if (disk != null)
            {
                _selectedDiskId = deviceId;
                Debug.WriteLine($"Selected disk: {disk.DisplayName}");
                return true;
            }

            return false;
        }

        public (float readMbps, float writeMbps, float totalMbps) Update()
        {
            try
            {
                if (_isAvailable && _allDisks.Count > 0)
                {
                    // Update all disk counters first
                    foreach (var disk in _allDisks)
                    {
                        if (disk.ReadCounter != null && disk.WriteCounter != null)
                        {
                            float readBytes = disk.ReadCounter.NextValue();
                            float writeBytes = disk.WriteCounter.NextValue();

                            disk.LastReadMbps = (float)(readBytes / Constants.BYTES_TO_MEGABITS);
                            disk.LastWriteMbps = (float)(writeBytes / Constants.BYTES_TO_MEGABITS);
                        }
                    }

                    // Calculate totals based on selection
                    if (_selectedDiskId == null)
                    {
                        // Aggregate mode - sum all disks
                        TotalReadMbps = _allDisks.Sum(d => d.LastReadMbps);
                        TotalWriteMbps = _allDisks.Sum(d => d.LastWriteMbps);
                    }
                    else
                    {
                        // Single disk mode
                        var selectedDisk = _allDisks.FirstOrDefault(d => d.Id == _selectedDiskId);
                        if (selectedDisk != null)
                        {
                            TotalReadMbps = selectedDisk.LastReadMbps;
                            TotalWriteMbps = selectedDisk.LastWriteMbps;
                        }
                        else
                        {
                            TotalReadMbps = 0;
                            TotalWriteMbps = 0;
                        }
                    }

                    return (TotalReadMbps, TotalWriteMbps, TotalThroughputMbps);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating disk throughput: {ex.Message}");
            }

            TotalReadMbps = 0;
            TotalWriteMbps = 0;
            return (0, 0, 0);
        }

        public void Dispose()
        {
            foreach (var disk in _allDisks)
            {
                disk.ReadCounter?.Dispose();
                disk.WriteCounter?.Dispose();
            }
            _allDisks.Clear();
        }
    }
}
