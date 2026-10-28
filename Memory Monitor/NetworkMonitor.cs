using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Memory_Monitor
{
    public class NetworkMonitor : ISelectableMonitor
    {
        private class NetworkAdapterInfo
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string InstanceName { get; set; } = "";
            public NetworkInterfaceType InterfaceType { get; set; }
            public PerformanceCounter? BytesSentCounter { get; set; }
            public PerformanceCounter? BytesReceivedCounter { get; set; }
            public long LastBytesSent { get; set; }
            public long LastBytesReceived { get; set; }
            public DateTime LastUpdate { get; set; } = DateTime.Now;
            public float UploadMbps { get; set; }
            public float DownloadMbps { get; set; }
        }

        private List<NetworkAdapterInfo> _allAdapters = new List<NetworkAdapterInfo>();
        private string? _selectedAdapterId = null; // null = aggregate all adapters
        private List<DeviceInfo> _availableDevices = new List<DeviceInfo>();
        private bool _isAvailable;

        public bool IsAvailable => _isAvailable;
        public float TotalUploadMbps { get; private set; }
        public float TotalDownloadMbps { get; private set; }
        public float TotalThroughputMbps => TotalUploadMbps + TotalDownloadMbps;

        // ISelectableMonitor implementation
        public IReadOnlyList<DeviceInfo> AvailableDevices => _availableDevices;
        public DeviceInfo? SelectedDevice => _selectedAdapterId != null 
            ? _availableDevices.FirstOrDefault(d => d.Id == _selectedAdapterId) 
            : _availableDevices.FirstOrDefault(d => d.Type == DeviceType.Aggregate);
        public bool HasMultipleDevices => _availableDevices.Count > 1; // True if we have choices (All + specific adapters)
        public string CurrentDeviceDisplayName => _selectedAdapterId != null
            ? _allAdapters.FirstOrDefault(a => a.Id == _selectedAdapterId)?.Name ?? "Unknown Adapter"
            : "All Networks";

        public NetworkMonitor()
        {
            Initialize();
            BuildDeviceList();
        }

        private void Initialize()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                
                Debug.WriteLine($"Found {interfaces.Length} network interfaces");

                string[] instanceNames = new PerformanceCounterCategory("Network Interface").GetInstanceNames();
                var usedInstances = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.OperationalStatus != OperationalStatus.Up)
                        continue;

                    NetworkInterfaceType type = ni.NetworkInterfaceType;
                    
                    if (type != NetworkInterfaceType.Ethernet && 
                        type != NetworkInterfaceType.Wireless80211 &&
                        type != NetworkInterfaceType.GigabitEthernet)
                    {
                        continue;
                    }

                    // Check the description too: Hyper-V switch adapters are named "vEthernet (...)" but described
                    // as "Hyper-V Virtual Ethernet Adapter"; counting them as well double-counts the physical traffic
                    string name = ni.Name.ToLower();
                    string description = ni.Description.ToLower();
                    string[] virtualMarkers = { "loopback", "tunnel", "virtual", "vmware", "virtualbox", "hyper-v", "vethernet" };
                    if (virtualMarkers.Any(m => name.Contains(m) || description.Contains(m)))
                    {
                        continue;
                    }

                    try
                    {
                        string instanceName = FindPerformanceCounterInstance(ni, instanceNames, usedInstances);

                        if (!string.IsNullOrEmpty(instanceName))
                        {
                            usedInstances.Add(instanceName);
                            var adapter = new NetworkAdapterInfo
                            {
                                // ni.Id is the adapter's GUID, stable across launches so the saved selection
                                // can be restored (string.GetHashCode is randomized per process in .NET)
                                Id = $"net_{ni.Id}",
                                Name = ni.Name,
                                Description = ni.Description,
                                InstanceName = instanceName,
                                InterfaceType = type,
                                BytesSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instanceName),
                                BytesReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", instanceName)
                            };

                            // Initialize counters
                            adapter.BytesSentCounter.NextValue();
                            adapter.BytesReceivedCounter.NextValue();

                            _allAdapters.Add(adapter);
                            Debug.WriteLine($"Initialized network monitor for: {ni.Name} ({type})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to initialize network monitor for {ni.Name}: {ex.Message}");
                    }
                }

                _isAvailable = _allAdapters.Count > 0;
                
                if (_isAvailable)
                {
                    Debug.WriteLine($"Network monitor initialized successfully with {_allAdapters.Count} adapter(s)");
                }
                else
                {
                    Debug.WriteLine("No network adapters available for monitoring");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize network monitor: {ex.Message}");
                _isAvailable = false;
            }
        }

        private void BuildDeviceList()
        {
            _availableDevices.Clear();

            // Always add "All Networks" aggregate option (even for single adapter)
            // This allows users to switch back to aggregate mode after selecting a specific adapter
            _availableDevices.Add(new DeviceInfo
            {
                Id = "",
                DisplayName = _allAdapters.Count > 1 ? "All Networks" : "All Networks",
                ShortName = "All",
                Description = _allAdapters.Count > 1 
                    ? $"Combined throughput from {_allAdapters.Count} adapters" 
                    : "Total network throughput",
                Type = DeviceType.Aggregate,
                IsActive = true
            });

            // Add individual adapters
            foreach (var adapter in _allAdapters)
            {
                string typeLabel = adapter.InterfaceType switch
                {
                    NetworkInterfaceType.Wireless80211 => "WiFi",
                    NetworkInterfaceType.Ethernet => "Ethernet",
                    NetworkInterfaceType.GigabitEthernet => "Gigabit",
                    _ => "Network"
                };

                _availableDevices.Add(new DeviceInfo
                {
                    Id = adapter.Id,
                    DisplayName = adapter.Name,
                    ShortName = GetShortAdapterName(adapter.Name, typeLabel),
                    Description = $"{typeLabel} • {adapter.Description}",
                    Type = DeviceType.NetworkAdapter,
                    IsActive = true
                });
            }
        }

        private string GetShortAdapterName(string name, string typeLabel)
        {
            // Try to create a short meaningful name
            if (name.Length <= 12)
                return name;

            // Check for common patterns
            if (name.Contains("Ethernet"))
                return typeLabel;
            if (name.Contains("Wi-Fi") || name.Contains("WiFi") || name.Contains("Wireless"))
                return "WiFi";
            
            // Truncate if needed
            return name.Length > 15 ? name.Substring(0, 12) + "..." : name;
        }

        public bool SelectDevice(string? deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                // Aggregate mode - use all adapters
                _selectedAdapterId = null;
                Debug.WriteLine("Selected: All Networks (aggregate)");
                return true;
            }

            var adapter = _allAdapters.FirstOrDefault(a => a.Id == deviceId);
            if (adapter != null)
            {
                _selectedAdapterId = deviceId;
                Debug.WriteLine($"Selected network adapter: {adapter.Name}");
                return true;
            }

            return false;
        }

        public DeviceInfo? CycleToNextDevice()
        {
            if (_availableDevices.Count <= 1)
                return SelectedDevice;

            // Find current device index
            int currentIndex = -1;
            for (int i = 0; i < _availableDevices.Count; i++)
            {
                var device = _availableDevices[i];
                if ((device.Type == DeviceType.Aggregate && _selectedAdapterId == null) ||
                    (device.Id == _selectedAdapterId))
                {
                    currentIndex = i;
                    break;
                }
            }

            // Move to next device (wrap around)
            int nextIndex = (currentIndex + 1) % _availableDevices.Count;
            var nextDevice = _availableDevices[nextIndex];

            // Select the next device
            SelectDevice(nextDevice.Type == DeviceType.Aggregate ? null : nextDevice.Id);

            Debug.WriteLine($"Cycled to network: {CurrentDeviceDisplayName}");
            return SelectedDevice;
        }

        /// <summary>
        /// Finds the "Network Interface" counter instance for an adapter. Instances are named after the adapter's
        /// description with characters that are reserved in counter paths replaced, e.g.
        /// "Intel(R) Ethernet Connection #2" becomes "Intel[R] Ethernet Connection _2". Only exact matches are
        /// accepted, and an instance already taken by another adapter is skipped, so two adapters can never
        /// share (and double-count) one counter.
        /// </summary>
        private static string FindPerformanceCounterInstance(NetworkInterface ni, string[] instanceNames, HashSet<string> usedInstances)
        {
            string[] candidates =
            {
                ToCounterInstanceName(ni.Description),
                ni.Description,
                ni.Name
            };

            foreach (string candidate in candidates)
            {
                string? match = instanceNames.FirstOrDefault(i =>
                    i.Equals(candidate, StringComparison.OrdinalIgnoreCase) && !usedInstances.Contains(i));
                if (match != null)
                    return match;
            }

            Debug.WriteLine($"No performance counter instance found for {ni.Name} ({ni.Description})");
            return "";
        }

        private static string ToCounterInstanceName(string name)
        {
            return name.Replace('(', '[').Replace(')', ']').Replace('#', '_').Replace('/', '_').Replace('\\', '_');
        }

        /// <summary>
        /// Updates and returns the current network throughput in Mbps (megabits per second)
        /// </summary>
        public (float uploadMbps, float downloadMbps, float totalMbps) Update()
        {
            try
            {
                if (_isAvailable && _allAdapters.Count > 0)
                {
                    // Update all adapter counters first
                    foreach (var adapter in _allAdapters)
                    {
                        if (adapter.BytesSentCounter != null && adapter.BytesReceivedCounter != null)
                        {
                            float sentBytes = adapter.BytesSentCounter.NextValue();
                            float receivedBytes = adapter.BytesReceivedCounter.NextValue();

                            adapter.UploadMbps = (float)(sentBytes / Constants.BYTES_TO_MEGABITS);
                            adapter.DownloadMbps = (float)(receivedBytes / Constants.BYTES_TO_MEGABITS);
                        }
                    }

                    // Calculate totals based on selection
                    if (_selectedAdapterId == null)
                    {
                        // Aggregate mode - sum all adapters
                        TotalUploadMbps = _allAdapters.Sum(a => a.UploadMbps);
                        TotalDownloadMbps = _allAdapters.Sum(a => a.DownloadMbps);
                    }
                    else
                    {
                        // Single adapter mode
                        var selectedAdapter = _allAdapters.FirstOrDefault(a => a.Id == _selectedAdapterId);
                        if (selectedAdapter != null)
                        {
                            TotalUploadMbps = selectedAdapter.UploadMbps;
                            TotalDownloadMbps = selectedAdapter.DownloadMbps;
                        }
                        else
                        {
                            TotalUploadMbps = 0;
                            TotalDownloadMbps = 0;
                        }
                    }

                    return (TotalUploadMbps, TotalDownloadMbps, TotalThroughputMbps);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating network throughput: {ex.Message}");
            }

            TotalUploadMbps = 0;
            TotalDownloadMbps = 0;
            return (0, 0, 0);
        }

        public void Dispose()
        {
            foreach (var adapter in _allAdapters)
            {
                adapter.BytesSentCounter?.Dispose();
                adapter.BytesReceivedCounter?.Dispose();
            }
            _allAdapters.Clear();
        }
    }
}
