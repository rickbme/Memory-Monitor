using System.Diagnostics;
using System.Management;

namespace Memory_Monitor
{
    /// <summary>
    /// Monitors GPU usage and memory using WMI and performance counters
    /// </summary>
    public class GPUMonitor : IDisposable
    {
        private const long BYTES_TO_MB = 1024 * 1024;
        private const long BYTES_TO_GB = 1024 * 1024 * 1024;

        private PerformanceCounter? _gpuUsageCounter;
        private bool _usageCounterAvailable = false;
        private string _gpuEngineInstance = "";

        public string GPUName { get; private set; } = "Unknown GPU";
        public ulong TotalMemoryBytes { get; private set; } = 0;
        public bool IsMemoryAvailable { get; private set; } = false;
        public bool IsUsageAvailable => _usageCounterAvailable;
        
        public float CurrentUsagePercent { get; private set; }
        public ulong CurrentMemoryUsedBytes { get; private set; }

        public double TotalMemoryGB => TotalMemoryBytes / (double)BYTES_TO_GB;
        public double UsedMemoryGB => CurrentMemoryUsedBytes / (double)BYTES_TO_GB;
        public int MemoryUsagePercent => TotalMemoryBytes > 0 
            ? Math.Min((int)((CurrentMemoryUsedBytes * 100) / TotalMemoryBytes), 100) 
            : 0;

        public GPUMonitor()
        {
            DetectGPU();
            InitializeUsageCounter();
        }

        /// <summary>
        /// Detects GPU hardware and retrieves total memory
        /// </summary>
        private void DetectGPU()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT Name, AdapterRAM, VideoProcessor FROM Win32_VideoController"))
                {
                    bool foundDedicatedGPU = false;

                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString() ?? "Unknown";
                        string processor = obj["VideoProcessor"]?.ToString() ?? "";
                        object adapterRAM = obj["AdapterRAM"];

                        Debug.WriteLine($"Found GPU: {name}, Processor: {processor}");

                        // Skip Microsoft Basic Display Adapter and other software renderers
                        if (name.Contains("Microsoft Basic") || name.Contains("Remote") ||
                            name.Contains("Virtual") || name.Contains("Software"))
                        {
                            Debug.WriteLine($"  Skipping virtual/software GPU: {name}");
                            continue;
                        }

                        // Prefer dedicated GPUs (NVIDIA, AMD, Intel Arc)
                        bool isDedicated = name.Contains("NVIDIA") || name.Contains("AMD") ||
                                         name.Contains("Radeon") || name.Contains("GeForce") ||
                                         name.Contains("Intel Arc") || name.Contains("RTX") ||
                                         name.Contains("GTX") || name.Contains("RX ");

                        if (adapterRAM != null)
                        {
                            if (ulong.TryParse(adapterRAM.ToString(), out ulong ram) && ram > 0)
                            {
                                if (!foundDedicatedGPU || isDedicated)
                                {
                                    GPUName = name;
                                    TotalMemoryBytes = ram;
                                    IsMemoryAvailable = true;
                                    foundDedicatedGPU = isDedicated;

                                    Debug.WriteLine($"  Selected GPU: {GPUName}");
                                    Debug.WriteLine($"  GPU Total Memory: {TotalMemoryGB:F2} GB");

                                    if (!isDedicated)
                                        break; // Keep looking for dedicated GPU
                                }
                            }
                        }
                    }
                }

                if (!IsMemoryAvailable)
                {
                    Debug.WriteLine("GPU memory information not available via WMI");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to detect GPU: {ex.Message}");
                IsMemoryAvailable = false;
            }
        }

        /// <summary>
        /// Initializes GPU usage performance counter
        /// </summary>
        private void InitializeUsageCounter()
        {
            try
            {
                PerformanceCounterCategory category = new PerformanceCounterCategory("GPU Engine");
                string[] instanceNames = category.GetInstanceNames();

                Debug.WriteLine($"Found {instanceNames.Length} GPU Engine instances:");
                foreach (string instance in instanceNames)
                {
                    Debug.WriteLine($"  - {instance}");
                }

                // Look for instances containing "engtype_3D" (3D rendering engine)
                var engine3DInstances = instanceNames.Where(i => i.Contains("engtype_3D")).ToArray();

                string selectedInstance = "";

                if (engine3DInstances.Length > 0)
                {
                    selectedInstance = engine3DInstances[0];
                    Debug.WriteLine($"Selected GPU Engine instance: {selectedInstance}");
                }
                else if (instanceNames.Length > 0)
                {
                    selectedInstance = instanceNames[0];
                    Debug.WriteLine($"Using fallback GPU Engine instance: {selectedInstance}");
                }

                if (!string.IsNullOrEmpty(selectedInstance))
                {
                    _gpuEngineInstance = selectedInstance;
                    _gpuUsageCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", selectedInstance);
                    _gpuUsageCounter.NextValue();
                    _usageCounterAvailable = true;
                    Debug.WriteLine("GPU usage performance counter initialized successfully");
                }
                else
                {
                    Debug.WriteLine("No suitable GPU Engine instance found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize GPU usage counter: {ex.Message}");
                _usageCounterAvailable = false;
            }
        }

        /// <summary>
        /// Updates GPU usage percentage
        /// </summary>
        public float UpdateUsage()
        {
            try
            {
                if (_usageCounterAvailable && _gpuUsageCounter != null)
                {
                    CurrentUsagePercent = _gpuUsageCounter.NextValue();
                    return CurrentUsagePercent;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating GPU usage: {ex.Message}");
                _usageCounterAvailable = false;
            }

            CurrentUsagePercent = 0;
            return 0;
        }

        /// <summary>
        /// Updates GPU memory usage
        /// </summary>
        public ulong UpdateMemory()
        {
            try
            {
                if (IsMemoryAvailable && TotalMemoryBytes > 0)
                {
                    CurrentMemoryUsedBytes = GetGPUMemoryUsage();
                    return CurrentMemoryUsedBytes;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating GPU memory: {ex.Message}");
            }

            CurrentMemoryUsedBytes = 0;
            return 0;
        }

        /// <summary>
        /// Retrieves current GPU memory usage from performance counters
        /// </summary>
        private ulong GetGPUMemoryUsage()
        {
            try
            {
                if (!string.IsNullOrEmpty(_gpuEngineInstance))
                {
                    string adapterId = ExtractAdapterIdFromEngineInstance(_gpuEngineInstance);

                    if (!string.IsNullOrEmpty(adapterId))
                    {
                        try
                        {
                            PerformanceCounterCategory memCategory = new PerformanceCounterCategory("GPU Adapter Memory");
                            string[] memInstances = memCategory.GetInstanceNames();

                            var matchingInstance = memInstances.FirstOrDefault(i => i.Contains(adapterId));

                            if (!string.IsNullOrEmpty(matchingInstance))
                            {
                                using (PerformanceCounter memCounter = new PerformanceCounter(
                                    "GPU Adapter Memory", "Dedicated Usage", matchingInstance))
                                {
                                    float usageMB = memCounter.NextValue();
                                    return (ulong)(usageMB * 1024 * 1024); // Convert MB to bytes
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"GPU Adapter Memory counter failed: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting GPU memory usage: {ex.Message}");
            }

            return 0;
        }

        /// <summary>
        /// Extracts adapter ID from GPU Engine instance name
        /// </summary>
        private string ExtractAdapterIdFromEngineInstance(string engineInstance)
        {
            try
            {
                int luidIndex = engineInstance.IndexOf("luid_");
                if (luidIndex >= 0)
                {
                    int endIndex = engineInstance.IndexOf("_", luidIndex + 5);
                    if (endIndex > luidIndex)
                    {
                        return engineInstance.Substring(luidIndex, endIndex - luidIndex);
                    }
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return "";
        }

        /// <summary>
        /// Returns a shortened version of the GPU name for display
        /// </summary>
        public string GetShortName()
        {
            string[] removeWords = { "NVIDIA", "GeForce", "AMD", "Radeon", "Intel", "Graphics", "(TM)", "®", "™" };
            string shortened = GPUName;

            foreach (string word in removeWords)
            {
                shortened = shortened.Replace(word, "").Trim();
            }

            // Clean up extra spaces
            while (shortened.Contains("  "))
                shortened = shortened.Replace("  ", " ");

            // If shortened name is too short, use original
            if (shortened.Length < 3)
                return GPUName;

            return shortened;
        }

        public void Dispose()
        {
            _gpuUsageCounter?.Dispose();
            _gpuUsageCounter = null;
        }
    }
}
