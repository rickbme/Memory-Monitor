using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// Monitors CPU usage using performance counters
    /// </summary>
    public class CPUMonitor : IMonitor
    {
        private PerformanceCounter? _cpuCounter;
        private bool _isAvailable;

        public bool IsAvailable => _isAvailable;
        public float CurrentUsage { get; private set; }

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

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _cpuCounter = null;
        }
    }
}
