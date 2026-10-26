using System.Diagnostics;

namespace Memory_Monitor.Linux
{
    /// <summary>
    /// Monitors network traffic on Linux using /proc/net/dev
    /// </summary>
    public class LinuxNetworkMonitor : ISelectableMonitor, IDisposable
    {
        private class NetworkInterface
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public long PreviousReceivedBytes { get; set; } = 0;
            public long PreviousSentBytes { get; set; } = 0;
            public DateTime PreviousTime { get; set; } = DateTime.Now;
        }

        private List<NetworkInterface> _detectedInterfaces = new List<NetworkInterface>();
        private NetworkInterface? _selectedInterface = null;
        private List<DeviceInfo> _availableDevices = new List<DeviceInfo>();

        public bool IsAvailable => _selectedInterface != null;
        public double CurrentDownloadMbps { get; private set; }
        public double CurrentUploadMbps { get; private set; }
        public double CurrentTotalMbps => CurrentDownloadMbps + CurrentUploadMbps;

        // ISelectableMonitor implementation
        public IReadOnlyList<DeviceInfo> AvailableDevices => _availableDevices;
        public DeviceInfo? SelectedDevice => _selectedInterface != null
            ? _availableDevices.FirstOrDefault(d => d.Id == _selectedInterface.Id)
            : null;
        public bool HasMultipleDevices => _detectedInterfaces.Count > 1;
        public string CurrentDeviceDisplayName => _selectedInterface?.Name ?? "No Network";

        public LinuxNetworkMonitor()
        {
            DetectInterfaces();
            BuildDeviceList();
            
            // Select first non-loopback interface by default
            var defaultInterface = _detectedInterfaces.FirstOrDefault(i => i.Id != "lo");
            if (defaultInterface != null)
            {
                SelectDevice(defaultInterface.Id);
            }
        }

        private void DetectInterfaces()
        {
            try
            {
                if (!File.Exists("/proc/net/dev"))
                {
                    Debug.WriteLine("Failed to access /proc/net/dev");
                    return;
                }

                var lines = File.ReadAllLines("/proc/net/dev");
                // Skip first two header lines
                for (int i = 2; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var colonIndex = line.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        string interfaceName = line.Substring(0, colonIndex).Trim();
                        
                        var netInterface = new NetworkInterface
                        {
                            Id = interfaceName,
                            Name = interfaceName,
                            PreviousTime = DateTime.Now
                        };

                        _detectedInterfaces.Add(netInterface);
                        Debug.WriteLine($"Detected network interface: {interfaceName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error detecting network interfaces: {ex.Message}");
            }
        }

        private void BuildDeviceList()
        {
            _availableDevices.Clear();
            foreach (var iface in _detectedInterfaces)
            {
                string description = iface.Id == "lo" ? "Loopback" : $"Network: {iface.Name}";
                _availableDevices.Add(new DeviceInfo
                {
                    Id = iface.Id,
                    DisplayName = iface.Name,
                    Description = description
                });
            }
        }

        /// <summary>
        /// Cycles to the next available network interface, wrapping around at the end of the list
        /// </summary>
        public DeviceInfo? CycleToNextDevice()
        {
            if (_availableDevices.Count <= 1)
                return SelectedDevice;

            int currentIndex = _availableDevices.FindIndex(d => d.Id == _selectedInterface?.Id);
            var nextDevice = _availableDevices[(currentIndex + 1) % _availableDevices.Count];
            SelectDevice(nextDevice.Id);

            Debug.WriteLine($"Cycled to network interface: {CurrentDeviceDisplayName}");
            return SelectedDevice;
        }

        public bool SelectDevice(string? deviceId)
        {
            var iface = _detectedInterfaces.FirstOrDefault(i => i.Id == deviceId);
            if (iface != null)
            {
                _selectedInterface = iface;
                // Initialize baseline values
                ReadNetworkStats(_selectedInterface);
                Debug.WriteLine($"Selected network interface: {deviceId}");
                return true;
            }
            return false;
        }

        private bool ReadNetworkStats(NetworkInterface iface)
        {
            try
            {
                var lines = File.ReadAllLines("/proc/net/dev");
                foreach (var line in lines)
                {
                    if (line.Contains(iface.Id + ":"))
                    {
                        var colonIndex = line.IndexOf(':');
                        if (colonIndex > 0)
                        {
                            var data = line.Substring(colonIndex + 1).Trim();
                            var parts = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            
                            // Format: bytes packets errs drop fifo frame compressed multicast ...
                            // Field 0: received bytes
                            // Field 8: transmitted bytes
                            if (parts.Length >= 9)
                            {
                                if (long.TryParse(parts[0], out long receivedBytes) &&
                                    long.TryParse(parts[8], out long sentBytes))
                                {
                                    iface.PreviousReceivedBytes = receivedBytes;
                                    iface.PreviousSentBytes = sentBytes;
                                    iface.PreviousTime = DateTime.Now;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading network stats: {ex.Message}");
            }
            return false;
        }

        public void Update()
        {
            if (_selectedInterface == null)
            {
                CurrentDownloadMbps = 0;
                CurrentUploadMbps = 0;
                return;
            }

            try
            {
                var lines = File.ReadAllLines("/proc/net/dev");
                foreach (var line in lines)
                {
                    if (line.Contains(_selectedInterface.Id + ":"))
                    {
                        var colonIndex = line.IndexOf(':');
                        if (colonIndex > 0)
                        {
                            var data = line.Substring(colonIndex + 1).Trim();
                            var parts = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            
                            if (parts.Length >= 9)
                            {
                                if (long.TryParse(parts[0], out long receivedBytes) &&
                                    long.TryParse(parts[8], out long sentBytes))
                                {
                                    DateTime currentTime = DateTime.Now;
                                    
                                    // Calculate time delta in seconds
                                    double timeDelta = (currentTime - _selectedInterface.PreviousTime).TotalSeconds;
                                    if (timeDelta > 0)
                                    {
                                        // Calculate bytes per second, then convert to Mbps
                                        long receivedDelta = receivedBytes - _selectedInterface.PreviousReceivedBytes;
                                        long sentDelta = sentBytes - _selectedInterface.PreviousSentBytes;

                                        // Convert bytes/sec to Mbps: (bytes/sec * 8) / (1000 * 1000)
                                        CurrentDownloadMbps = (receivedDelta * 8.0) / (timeDelta * 1000000.0);
                                        CurrentUploadMbps = (sentDelta * 8.0) / (timeDelta * 1000000.0);

                                        // Ensure non-negative values
                                        CurrentDownloadMbps = Math.Max(0, CurrentDownloadMbps);
                                        CurrentUploadMbps = Math.Max(0, CurrentUploadMbps);
                                    }

                                    // Update previous values
                                    _selectedInterface.PreviousReceivedBytes = receivedBytes;
                                    _selectedInterface.PreviousSentBytes = sentBytes;
                                    _selectedInterface.PreviousTime = currentTime;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating network stats: {ex.Message}");
            }

            CurrentDownloadMbps = 0;
            CurrentUploadMbps = 0;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
