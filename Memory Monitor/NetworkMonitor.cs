using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Memory_Monitor
{
    /// <summary>
    /// Monitors network upload/download speeds for ethernet adapters
    /// </summary>
    public class NetworkMonitor : IDisposable
    {
        private const long BYTES_TO_MEGABITS = 1024 * 1024 / 8; // Convert bytes to megabits (1 MB = 8 Mb)

        private class NetworkAdapterInfo
        {
            public string Name { get; set; } = "";
            public string InstanceName { get; set; } = "";
            public PerformanceCounter? BytesSentCounter { get; set; }
            public PerformanceCounter? BytesReceivedCounter { get; set; }
            public long LastBytesSent { get; set; }
            public long LastBytesReceived { get; set; }
            public DateTime LastUpdate { get; set; } = DateTime.Now;
            public float UploadMbps { get; set; }
            public float DownloadMbps { get; set; }
        }

        private List<NetworkAdapterInfo> _adapters = new List<NetworkAdapterInfo>();
        private bool _isAvailable;

        public bool IsAvailable => _isAvailable;
        public float TotalUploadMbps { get; private set; }
        public float TotalDownloadMbps { get; private set; }
        public float TotalThroughputMbps => TotalUploadMbps + TotalDownloadMbps;

        public NetworkMonitor()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Get active network interfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                
                Debug.WriteLine($"Found {interfaces.Length} network interfaces");

                foreach (NetworkInterface ni in interfaces)
                {
                    // Only monitor Ethernet and Wireless adapters that are up
                    if (ni.OperationalStatus != OperationalStatus.Up)
                        continue;

                    NetworkInterfaceType type = ni.NetworkInterfaceType;
                    
                    // Filter for physical network adapters
                    if (type != NetworkInterfaceType.Ethernet && 
                        type != NetworkInterfaceType.Wireless80211 &&
                        type != NetworkInterfaceType.GigabitEthernet)
                    {
                        continue;
                    }

                    // Skip loopback and tunnel adapters
                    string name = ni.Name.ToLower();
                    if (name.Contains("loopback") || name.Contains("tunnel") || 
                        name.Contains("virtual") || name.Contains("vmware") ||
                        name.Contains("virtualbox") || name.Contains("hyper-v"))
                    {
                        continue;
                    }

                    try
                    {
                        string instanceName = FindPerformanceCounterInstance(ni);
                        
                        if (!string.IsNullOrEmpty(instanceName))
                        {
                            var adapter = new NetworkAdapterInfo
                            {
                                Name = ni.Name,
                                InstanceName = instanceName,
                                BytesSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instanceName),
                                BytesReceivedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", instanceName)
                            };

                            // Initialize counters
                            adapter.BytesSentCounter.NextValue();
                            adapter.BytesReceivedCounter.NextValue();

                            _adapters.Add(adapter);
                            Debug.WriteLine($"Initialized network monitor for: {ni.Name} ({type})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to initialize network monitor for {ni.Name}: {ex.Message}");
                    }
                }

                _isAvailable = _adapters.Count > 0;
                
                if (_isAvailable)
                {
                    Debug.WriteLine($"Network monitor initialized successfully with {_adapters.Count} adapter(s)");
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

        private string FindPerformanceCounterInstance(NetworkInterface ni)
        {
            try
            {
                PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
                string[] instanceNames = category.GetInstanceNames();

                // Try to match by name
                string niName = ni.Name;
                string niDesc = ni.Description;

                // Look for exact match first
                foreach (string instanceName in instanceNames)
                {
                    if (instanceName.Equals(niName, StringComparison.OrdinalIgnoreCase) ||
                        instanceName.Equals(niDesc, StringComparison.OrdinalIgnoreCase))
                    {
                        return instanceName;
                    }
                }

                // Try partial match
                foreach (string instanceName in instanceNames)
                {
                    if (instanceName.Contains(niName, StringComparison.OrdinalIgnoreCase) ||
                        instanceName.Contains(niDesc, StringComparison.OrdinalIgnoreCase) ||
                        niDesc.Contains(instanceName, StringComparison.OrdinalIgnoreCase))
                    {
                        return instanceName;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding performance counter instance: {ex.Message}");
            }

            return "";
        }

        /// <summary>
        /// Updates and returns the current network throughput in Mbps (megabits per second)
        /// </summary>
        public (float uploadMbps, float downloadMbps, float totalMbps) Update()
        {
            try
            {
                if (_isAvailable && _adapters.Count > 0)
                {
                    float totalUpload = 0;
                    float totalDownload = 0;

                    foreach (var adapter in _adapters)
                    {
                        if (adapter.BytesSentCounter != null && adapter.BytesReceivedCounter != null)
                        {
                            // Get bytes/sec and convert to Mbps (megabits per second)
                            float sentBytes = adapter.BytesSentCounter.NextValue();
                            float receivedBytes = adapter.BytesReceivedCounter.NextValue();

                            adapter.UploadMbps = (sentBytes / BYTES_TO_MEGABITS) * 8;
                            adapter.DownloadMbps = (receivedBytes / BYTES_TO_MEGABITS) * 8;

                            totalUpload += adapter.UploadMbps;
                            totalDownload += adapter.DownloadMbps;
                        }
                    }

                    TotalUploadMbps = totalUpload;
                    TotalDownloadMbps = totalDownload;

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
            foreach (var adapter in _adapters)
            {
                adapter.BytesSentCounter?.Dispose();
                adapter.BytesReceivedCounter?.Dispose();
            }
            _adapters.Clear();
        }
    }
}
