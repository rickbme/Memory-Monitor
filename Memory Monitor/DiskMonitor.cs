using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// Monitors disk read/write throughput using performance counters
    /// </summary>
    public class DiskMonitor : IDisposable
    {
        private const long BYTES_TO_MEGABITS = 1024 * 1024 / 8; // Convert bytes to megabits (1 MB = 8 Mb)

        private class DiskCounters
        {
            public string DiskName { get; set; } = "";
            public PerformanceCounter? ReadCounter { get; set; }
            public PerformanceCounter? WriteCounter { get; set; }
            public float LastReadMbps { get; set; }
            public float LastWriteMbps { get; set; }
        }

        private List<DiskCounters> _diskCounters = new List<DiskCounters>();
        private bool _isAvailable;

        public bool IsAvailable => _isAvailable;
        public float TotalReadMbps { get; private set; }
        public float TotalWriteMbps { get; private set; }
        public float TotalThroughputMbps => TotalReadMbps + TotalWriteMbps;

        public DiskMonitor()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                PerformanceCounterCategory category = new PerformanceCounterCategory("PhysicalDisk");
                string[] instanceNames = category.GetInstanceNames();

                Debug.WriteLine($"Found {instanceNames.Length} PhysicalDisk instances");

                foreach (string instanceName in instanceNames)
                {
                    // Skip the "_Total" instance and focus on actual disks
                    if (instanceName == "_Total" || string.IsNullOrWhiteSpace(instanceName))
                        continue;

                    try
                    {
                        var diskCounter = new DiskCounters
                        {
                            DiskName = instanceName,
                            ReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", instanceName),
                            WriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", instanceName)
                        };

                        // Initialize counters
                        diskCounter.ReadCounter.NextValue();
                        diskCounter.WriteCounter.NextValue();

                        _diskCounters.Add(diskCounter);
                        Debug.WriteLine($"Initialized disk monitor for: {instanceName}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to initialize disk monitor for {instanceName}: {ex.Message}");
                    }
                }

                _isAvailable = _diskCounters.Count > 0;
                
                if (_isAvailable)
                {
                    Debug.WriteLine($"Disk monitor initialized successfully with {_diskCounters.Count} disk(s)");
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

        /// <summary>
        /// Updates and returns the current disk throughput in Mbps (megabits per second)
        /// </summary>
        public (float readMbps, float writeMbps, float totalMbps) Update()
        {
            try
            {
                if (_isAvailable && _diskCounters.Count > 0)
                {
                    float totalRead = 0;
                    float totalWrite = 0;

                    foreach (var disk in _diskCounters)
                    {
                        if (disk.ReadCounter != null && disk.WriteCounter != null)
                        {
                            // Get bytes/sec and convert to Mbps (megabits per second)
                            float readBytes = disk.ReadCounter.NextValue();
                            float writeBytes = disk.WriteCounter.NextValue();

                            disk.LastReadMbps = (readBytes / BYTES_TO_MEGABITS) * 8;
                            disk.LastWriteMbps = (writeBytes / BYTES_TO_MEGABITS) * 8;

                            totalRead += disk.LastReadMbps;
                            totalWrite += disk.LastWriteMbps;
                        }
                    }

                    TotalReadMbps = totalRead;
                    TotalWriteMbps = totalWrite;

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
            foreach (var disk in _diskCounters)
            {
                disk.ReadCounter?.Dispose();
                disk.WriteCounter?.Dispose();
            }
            _diskCounters.Clear();
        }
    }
}
