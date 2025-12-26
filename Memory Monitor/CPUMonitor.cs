using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// Monitors CPU usage using performance counters and temperature via LibreHardwareMonitor
    /// </summary>
    public class CPUMonitor : IMonitor
    {
        private PerformanceCounter? _cpuCounter;
        private HardwareMonitorService? _hardwareMonitor;
        private bool _isAvailable;

        public bool IsAvailable => _isAvailable;
        public bool IsTemperatureAvailable => _hardwareMonitor?.IsCpuTemperatureAvailable ?? false;
        public float CurrentUsage { get; private set; }
        public int CurrentTemperatureCelsius { get; private set; }

        public CPUMonitor()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // First call returns 0, so we call it once to initialize
                _isAvailable = true;
                Debug.WriteLine("CPU performance counter initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize CPU counter: {ex.Message}");
                _isAvailable = false;
            }

            // Initialize hardware monitor for temperature
            try
            {
                _hardwareMonitor = new HardwareMonitorService();
                Debug.WriteLine($"CPU temperature available: {IsTemperatureAvailable}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize hardware monitor: {ex.Message}");
                _hardwareMonitor = null;
            }
        }

        /// <summary>
        /// Updates and returns the current CPU usage percentage
        /// </summary>
        public float Update()
        {
            try
            {
                if (_cpuCounter != null && _isAvailable)
                {
                    CurrentUsage = _cpuCounter.NextValue();
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
                if (_hardwareMonitor != null)
                {
                    var temp = _hardwareMonitor.GetCpuTemperature();
                    if (temp.HasValue && temp.Value > 0)
                    {
                        CurrentTemperatureCelsius = temp.Value;
                        return CurrentTemperatureCelsius;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating CPU temperature: {ex.Message}");
            }

            CurrentTemperatureCelsius = 0;
            return 0;
        }

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _cpuCounter = null;

            _hardwareMonitor?.Dispose();
            _hardwareMonitor = null;
        }
    }
}
