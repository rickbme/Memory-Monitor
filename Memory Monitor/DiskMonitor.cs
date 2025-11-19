using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// Monitors disk read/write throughput using performance counters
    /// </summary>
    public class DiskMonitor : IDisposable
    {
        private const long BYTES_TO_MB = 1024 * 1024;

        private class DiskCounters
        {
            public string DiskName { get; set; } = "";
            public PerformanceCounter? ReadCounter { get; set; }
            public PerformanceCounter? WriteCounter { get; set; }
            public float LastReadMBps { get; set; }
            public float LastWriteMBps { get; set; }
        }

        private List<DiskCounters> _diskCounters = new List<DiskCounters>();
        private bool _isAvailable;

        public bool IsAvailable => _isAvailable;
        public float TotalReadMBps { get; private set; }
        public float TotalWriteMBps { get; private set; }
        public float TotalThroughputMBps => TotalReadMBps + TotalWriteMBps;

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
        /// Updates and returns the current disk throughput
        /// </summary>
        public (float readMBps, float writeMBps, float totalMBps) Update()
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
                            // Get bytes/sec and convert to MB/sec
                            float readBytes = disk.ReadCounter.NextValue();
                            float writeBytes = disk.WriteCounter.NextValue();

                            disk.LastReadMBps = readBytes / BYTES_TO_MB;
                            disk.LastWriteMBps = writeBytes / BYTES_TO_MB;

                            totalRead += disk.LastReadMBps;
                            totalWrite += disk.LastWriteMBps;
                        }
                    }

                    TotalReadMBps = totalRead;
                    TotalWriteMBps = totalWrite;

                    return (TotalReadMBps, TotalWriteMBps, TotalThroughputMBps);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating disk throughput: {ex.Message}");
            }

            TotalReadMBps = 0;
            TotalWriteMBps = 0;
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
