using System.Diagnostics;

namespace Memory_Monitor.Linux
{
    /// <summary>
    /// Monitors system memory usage on Linux using /proc/meminfo
    /// </summary>
    public class LinuxMemoryMonitor : IMonitor
    {
        private bool _isAvailable;

        public bool IsAvailable => _isAvailable;
        public ulong TotalMemoryBytes { get; private set; }
        public ulong AvailableMemoryBytes { get; private set; }
        public ulong UsedMemoryBytes => TotalMemoryBytes > AvailableMemoryBytes 
            ? TotalMemoryBytes - AvailableMemoryBytes 
            : 0;

        public double TotalMemoryGB => TotalMemoryBytes / (double)Constants.BYTES_TO_GB;
        public double AvailableMemoryGB => AvailableMemoryBytes / (double)Constants.BYTES_TO_GB;
        public double UsedMemoryGB => UsedMemoryBytes / (double)Constants.BYTES_TO_GB;

        public int UsagePercent => TotalMemoryBytes > 0
            ? Math.Min((int)((UsedMemoryBytes * 100) / TotalMemoryBytes), 100)
            : 0;

        public LinuxMemoryMonitor()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Check if /proc/meminfo exists
                if (File.Exists("/proc/meminfo"))
                {
                    Update();
                    _isAvailable = TotalMemoryBytes > 0;
                    Debug.WriteLine($"Linux Memory monitor initialized: {TotalMemoryGB:F2} GB total");
                }
                else
                {
                    Debug.WriteLine("Failed to access /proc/meminfo");
                    _isAvailable = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize memory monitor: {ex.Message}");
                _isAvailable = false;
            }
        }

        /// <summary>
        /// Updates memory statistics from /proc/meminfo
        /// </summary>
        public void Update()
        {
            try
            {
                if (!File.Exists("/proc/meminfo"))
                {
                    _isAvailable = false;
                    return;
                }

                var lines = File.ReadAllLines("/proc/meminfo");
                ulong memTotal = 0;
                ulong memFree = 0;
                ulong memAvailable = 0;
                ulong buffers = 0;
                ulong cached = 0;

                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        var key = parts[0];
                        if (ulong.TryParse(parts[1], out ulong value))
                        {
                            // Values are in kB, convert to bytes
                            value *= 1024;

                            switch (key)
                            {
                                case "MemTotal":
                                    memTotal = value;
                                    break;
                                case "MemFree":
                                    memFree = value;
                                    break;
                                case "MemAvailable":
                                    memAvailable = value;
                                    break;
                                case "Buffers":
                                    buffers = value;
                                    break;
                                case "Cached":
                                    cached = value;
                                    break;
                            }
                        }
                    }
                }

                TotalMemoryBytes = memTotal;

                // Use MemAvailable if available (kernel 3.14+), otherwise estimate
                if (memAvailable > 0)
                {
                    AvailableMemoryBytes = memAvailable;
                }
                else
                {
                    // Estimate available memory: MemFree + Buffers + Cached
                    AvailableMemoryBytes = memFree + buffers + cached;
                }

                _isAvailable = TotalMemoryBytes > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating memory usage: {ex.Message}");
                _isAvailable = false;
            }
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
