using System.Diagnostics;

namespace Memory_Monitor.Linux
{
    /// <summary>
    /// Monitors CPU usage on Linux using /proc/stat and temperature via thermal zones
    /// </summary>
    public class LinuxCPUMonitor : IMonitor
    {
        private bool _isAvailable;
        private long _previousIdleTime;
        private long _previousTotalTime;
        private DateTime _lastUpdateTime;

        public bool IsAvailable => _isAvailable;
        public bool IsTemperatureAvailable { get; private set; }
        public float CurrentUsage { get; private set; }
        public int CurrentTemperatureCelsius { get; private set; }

        public LinuxCPUMonitor()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Check if /proc/stat exists
                if (File.Exists("/proc/stat"))
                {
                    // Initialize baseline CPU times
                    ReadCpuTimes(out _previousIdleTime, out _previousTotalTime);
                    _lastUpdateTime = DateTime.Now;
                    _isAvailable = true;
                    Debug.WriteLine("Linux CPU monitor initialized successfully");
                }
                else
                {
                    Debug.WriteLine("Failed to access /proc/stat");
                    _isAvailable = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize CPU monitor: {ex.Message}");
                _isAvailable = false;
            }

            IsTemperatureAvailable = ReadThermalZone() > 0;
            Debug.WriteLine($"CPU temperature available: {IsTemperatureAvailable}");
        }

        /// <summary>
        /// Reads CPU times from /proc/stat
        /// </summary>
        private bool ReadCpuTimes(out long idleTime, out long totalTime)
        {
            idleTime = 0;
            totalTime = 0;

            try
            {
                var lines = File.ReadAllLines("/proc/stat");
                foreach (var line in lines)
                {
                    if (line.StartsWith("cpu ")) // Overall CPU line
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 5)
                        {
                            // CPU times: user, nice, system, idle, iowait, irq, softirq, ...
                            long user = long.Parse(parts[1]);
                            long nice = long.Parse(parts[2]);
                            long system = long.Parse(parts[3]);
                            long idle = long.Parse(parts[4]);
                            long iowait = parts.Length > 5 ? long.Parse(parts[5]) : 0;
                            long irq = parts.Length > 6 ? long.Parse(parts[6]) : 0;
                            long softirq = parts.Length > 7 ? long.Parse(parts[7]) : 0;

                            idleTime = idle + iowait;
                            totalTime = user + nice + system + idle + iowait + irq + softirq;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading CPU times: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Updates and returns the current CPU usage percentage
        /// </summary>
        public float Update()
        {
            try
            {
                if (!_isAvailable)
                {
                    CurrentUsage = 0;
                    return 0;
                }

                // Read current CPU times
                if (ReadCpuTimes(out long currentIdleTime, out long currentTotalTime))
                {
                    // Calculate differences
                    long idleDelta = currentIdleTime - _previousIdleTime;
                    long totalDelta = currentTotalTime - _previousTotalTime;

                    if (totalDelta > 0)
                    {
                        // Calculate CPU usage percentage
                        float usage = 100.0f * (1.0f - ((float)idleDelta / totalDelta));
                        CurrentUsage = Math.Max(0, Math.Min(100, usage));
                    }

                    // Update previous values
                    _previousIdleTime = currentIdleTime;
                    _previousTotalTime = currentTotalTime;
                    _lastUpdateTime = DateTime.Now;

                    return CurrentUsage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating CPU usage: {ex.Message}");
                _isAvailable = false;
            }

            CurrentUsage = 0;
            return 0;
        }

        /// <summary>
        /// Updates and returns the current CPU temperature in Celsius
        /// </summary>
        public int UpdateTemperature()
        {
            try
            {
                CurrentTemperatureCelsius = ReadThermalZone();
                return CurrentTemperatureCelsius;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating CPU temperature: {ex.Message}");
            }

            CurrentTemperatureCelsius = 0;
            return 0;
        }

        /// <summary>
        /// Reads temperature from Linux thermal zones
        /// </summary>
        private int ReadThermalZone()
        {
            try
            {
                // Look for CPU thermal zones
                var thermalPath = "/sys/class/thermal";
                if (Directory.Exists(thermalPath))
                {
                    var zones = Directory.GetDirectories(thermalPath, "thermal_zone*");
                    foreach (var zone in zones)
                    {
                        var typePath = Path.Combine(zone, "type");
                        var tempPath = Path.Combine(zone, "temp");

                        if (File.Exists(typePath) && File.Exists(tempPath))
                        {
                            var type = File.ReadAllText(typePath).Trim();
                            // Look for CPU-related thermal zones
                            if (type.Contains("cpu", StringComparison.OrdinalIgnoreCase) ||
                                type.Contains("x86_pkg_temp", StringComparison.OrdinalIgnoreCase) ||
                                type.Contains("coretemp", StringComparison.OrdinalIgnoreCase))
                            {
                                var tempStr = File.ReadAllText(tempPath).Trim();
                                if (int.TryParse(tempStr, out int tempMilliC))
                                {
                                    // Temperature is in millidegrees Celsius
                                    return tempMilliC / 1000;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading thermal zone: {ex.Message}");
            }

            return 0;
        }

        public void Dispose()
        {
        }
    }
}
