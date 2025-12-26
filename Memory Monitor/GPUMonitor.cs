using System.Diagnostics;
using System.Management;

namespace Memory_Monitor
{
    /// <summary>
    /// Monitors GPU usage and memory using native APIs (NVML/ADL) with fallback to WMI
    /// </summary>
    public class GPUMonitor : IMonitor, IDisposable
    {
        public enum GPUVendor
        {
            Unknown,
            NVIDIA,
            AMD,
            Intel
        }

        private PerformanceCounter? _gpuUsageCounter;
        private bool _usageCounterAvailable = false;
        private string _gpuEngineInstance = "";

        private GPUVendor _vendor = GPUVendor.Unknown;
        private NVMLInterop.nvmlDevice_t? _nvmlDevice = null;
        private int _adlAdapterIndex = -1;
        private bool _useNativeAPI = false;

        public string GPUName { get; private set; } = "Unknown GPU";
        public ulong TotalMemoryBytes { get; private set; } = 0;
        public bool IsMemoryAvailable { get; private set; } = false;
        public bool IsUsageAvailable { get; private set; } = false;
        public bool IsTemperatureAvailable { get; private set; } = false;
        public bool IsAvailable => IsUsageAvailable || IsMemoryAvailable;
        public GPUVendor Vendor => _vendor;

        public float CurrentUsagePercent { get; private set; }
        public ulong CurrentMemoryUsedBytes { get; private set; }
        public int CurrentTemperatureCelsius { get; private set; }

        public double TotalMemoryGB => TotalMemoryBytes / (double)Constants.BYTES_TO_GB;
        public double UsedMemoryGB => CurrentMemoryUsedBytes / (double)Constants.BYTES_TO_GB;
        public int MemoryUsagePercent => TotalMemoryBytes > 0
            ? Math.Min((int)((CurrentMemoryUsedBytes * 100) / TotalMemoryBytes), 100)
            : 0;

        public GPUMonitor()
        {
            DetectGPU();
            InitializeNativeAPIs();
            
            // Only initialize performance counters if native APIs failed
            if (!_useNativeAPI)
            {
                InitializeUsageCounter();
            }
        }

        /// <summary>
        /// Detects GPU hardware via WMI and determines vendor
        /// </summary>
        private void DetectGPU()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT Name, AdapterRAM, VideoProcessor, PNPDeviceID FROM Win32_VideoController"))
                {
                    bool foundDedicatedGPU = false;

                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString() ?? "Unknown";
                        string pnpId = obj["PNPDeviceID"]?.ToString() ?? "";
                        object adapterRAM = obj["AdapterRAM"];

                        Debug.WriteLine($"Found GPU: {name}");
                        Debug.WriteLine($"  PNP ID: {pnpId}");

                        // Skip virtual/software renderers
                        if (name.Contains("Microsoft Basic") || name.Contains("Remote") ||
                            name.Contains("Virtual") || name.Contains("Software"))
                        {
                            Debug.WriteLine($"  Skipping virtual/software GPU");
                            continue;
                        }

                        // Determine vendor
                        GPUVendor vendor = GPUVendor.Unknown;
                        if (name.Contains("NVIDIA") || name.Contains("GeForce") || name.Contains("RTX") || name.Contains("GTX") || 
                            pnpId.Contains("VEN_10DE"))
                        {
                            vendor = GPUVendor.NVIDIA;
                        }
                        else if (name.Contains("AMD") || name.Contains("Radeon") || name.Contains("RX ") || 
                                 pnpId.Contains("VEN_1002"))
                        {
                            vendor = GPUVendor.AMD;
                        }
                        else if (name.Contains("Intel") && (name.Contains("Arc") || name.Contains("Xe") || name.Contains("Iris")))
                        {
                            vendor = GPUVendor.Intel;
                        }

                        bool isDedicated = vendor != GPUVendor.Unknown;

                        if (adapterRAM != null)
                        {
                            if (ulong.TryParse(adapterRAM.ToString(), out ulong ram) && ram > 0)
                            {
                                if (!foundDedicatedGPU || isDedicated)
                                {
                                    GPUName = name;
                                    TotalMemoryBytes = ram;
                                    IsMemoryAvailable = true;
                                    _vendor = vendor;
                                    foundDedicatedGPU = isDedicated;

                                    Debug.WriteLine($"  Selected GPU: {GPUName}");
                                    Debug.WriteLine($"  Vendor: {_vendor}");
                                    Debug.WriteLine($"  Total Memory: {TotalMemoryGB:F2} GB");

                                    if (isDedicated)
                                        break; // Stop at first dedicated GPU
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
        /// Initializes native GPU monitoring APIs (NVML for NVIDIA, ADL for AMD)
        /// </summary>
        private void InitializeNativeAPIs()
        {
            switch (_vendor)
            {
                case GPUVendor.NVIDIA:
                    InitializeNVML();
                    break;

                case GPUVendor.AMD:
                    InitializeADL();
                    break;

                case GPUVendor.Intel:
                    // Intel doesn't have a public monitoring API yet, fall back to performance counters
                    Debug.WriteLine("Intel GPU detected - using performance counters");
                    break;

                default:
                    Debug.WriteLine("Unknown GPU vendor - using performance counters");
                    break;
            }
        }

        /// <summary>
        /// Initialize NVIDIA NVML
        /// </summary>
        private void InitializeNVML()
        {
            try
            {
                if (NVMLInterop.Initialize())
                {
                    uint deviceCount = NVMLInterop.GetDeviceCount();
                    Debug.WriteLine($"NVML found {deviceCount} NVIDIA GPU(s)");

                    if (deviceCount > 0)
                    {
                        // Use first device
                        _nvmlDevice = NVMLInterop.GetDeviceByIndex(0);
                        
                        if (_nvmlDevice.HasValue)
                        {
                            string? nvmlName = NVMLInterop.GetDeviceName(_nvmlDevice.Value);
                            if (nvmlName != null)
                            {
                                GPUName = nvmlName;
                                Debug.WriteLine($"NVML Device Name: {GPUName}");
                            }

                            // Get accurate memory size from NVML
                            var memInfo = NVMLInterop.GetMemoryInfo(_nvmlDevice.Value);
                            if (memInfo.HasValue)
                            {
                                TotalMemoryBytes = memInfo.Value.total;
                                Debug.WriteLine($"NVML Total Memory: {TotalMemoryGB:F2} GB");
                            }

                            // Check if temperature is available
                            var tempCheck = NVMLInterop.GetTemperature(_nvmlDevice.Value);
                            IsTemperatureAvailable = tempCheck.HasValue;
                            if (IsTemperatureAvailable)
                            {
                                Debug.WriteLine($"NVML Temperature available: {tempCheck.Value}°C");
                            }

                            _useNativeAPI = true;
                            IsUsageAvailable = true;
                            IsMemoryAvailable = true;
                            Debug.WriteLine("NVML monitoring enabled");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVML initialization failed: {ex.Message}");
                _useNativeAPI = false;
            }
        }

        /// <summary>
        /// Initialize AMD ADL
        /// </summary>
        private void InitializeADL()
        {
            try
            {
                if (ADLInterop.Initialize())
                {
                    var adapters = ADLInterop.GetAdapterInfo();
                    if (adapters != null && adapters.Length > 0)
                    {
                        Debug.WriteLine($"ADL found {adapters.Length} AMD adapter(s)");

                        // Find first active adapter
                        for (int i = 0; i < adapters.Length; i++)
                        {
                            if (ADLInterop.IsAdapterActive(adapters[i].AdapterIndex))
                            {
                                _adlAdapterIndex = adapters[i].AdapterIndex;
                                GPUName = adapters[i].AdapterName;
                                
                                Debug.WriteLine($"ADL Active Adapter: {GPUName} (Index: {_adlAdapterIndex})");

                                // Get memory info
                                var memInfo = ADLInterop.GetMemoryInfo(_adlAdapterIndex);
                                if (memInfo.HasValue && memInfo.Value.MemorySize > 0)
                                {
                                    TotalMemoryBytes = (ulong)memInfo.Value.MemorySize;
                                    Debug.WriteLine($"ADL Total Memory: {TotalMemoryGB:F2} GB");
                                }

                                // Check if temperature is available
                                var tempCheck = ADLInterop.GetTemperature(_adlAdapterIndex);
                                IsTemperatureAvailable = tempCheck.HasValue;
                                if (IsTemperatureAvailable)
                                {
                                    Debug.WriteLine($"ADL Temperature available: {tempCheck.Value}°C");
                                }

                                _useNativeAPI = true;
                                IsUsageAvailable = true;
                                IsMemoryAvailable = true;
                                Debug.WriteLine("ADL monitoring enabled");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADL initialization failed: {ex.Message}");
                _useNativeAPI = false;
            }
        }

        /// <summary>
        /// Initializes GPU usage performance counter (fallback method)
        /// </summary>
        private void InitializeUsageCounter()
        {
            try
            {
                PerformanceCounterCategory category = new PerformanceCounterCategory("GPU Engine");
                string[] instanceNames = category.GetInstanceNames();

                Debug.WriteLine($"Found {instanceNames.Length} GPU Engine instances (fallback)");

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
                    IsUsageAvailable = true;
                    Debug.WriteLine("GPU usage performance counter initialized (fallback)");
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
                if (_useNativeAPI)
                {
                    switch (_vendor)
                    {
                        case GPUVendor.NVIDIA:
                            return UpdateUsageNVML();

                        case GPUVendor.AMD:
                            return UpdateUsageADL();
                    }
                }

                // Fallback to performance counter
                if (_usageCounterAvailable && _gpuUsageCounter != null)
                {
                    CurrentUsagePercent = _gpuUsageCounter.NextValue();
                    return CurrentUsagePercent;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating GPU usage: {ex.Message}");
            }

            CurrentUsagePercent = 0;
            return 0;
        }

        private float UpdateUsageNVML()
        {
            if (_nvmlDevice.HasValue)
            {
                var utilization = NVMLInterop.GetUtilization(_nvmlDevice.Value);
                if (utilization.HasValue)
                {
                    CurrentUsagePercent = utilization.Value.gpu;
                    return CurrentUsagePercent;
                }
            }
            return 0;
        }

        private float UpdateUsageADL()
        {
            if (_adlAdapterIndex >= 0)
            {
                var activity = ADLInterop.GetCurrentActivity(_adlAdapterIndex);
                if (activity.HasValue)
                {
                    CurrentUsagePercent = activity.Value.ActivityPercent;
                    return CurrentUsagePercent;
                }
            }
            return 0;
        }

        /// <summary>
        /// Updates GPU memory usage
        /// </summary>
        public ulong UpdateMemory()
        {
            try
            {
                if (_useNativeAPI)
                {
                    switch (_vendor)
                    {
                        case GPUVendor.NVIDIA:
                            return UpdateMemoryNVML();

                        case GPUVendor.AMD:
                            return UpdateMemoryADL();
                    }
                }

                // Fallback to performance counter method
                if (IsMemoryAvailable && TotalMemoryBytes > 0)
                {
                    CurrentMemoryUsedBytes = GetGPUMemoryUsageFromPerfCounter();
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

        private ulong UpdateMemoryNVML()
        {
            if (_nvmlDevice.HasValue)
            {
                var memInfo = NVMLInterop.GetMemoryInfo(_nvmlDevice.Value);
                if (memInfo.HasValue)
                {
                    CurrentMemoryUsedBytes = memInfo.Value.used;
                    return CurrentMemoryUsedBytes;
                }
            }
            return 0;
        }

        private ulong UpdateMemoryADL()
        {
            // ADL doesn't provide used memory, only total
            // We would need to estimate or use other methods
            // For now, return 0 to indicate unavailable
            Debug.WriteLine("ADL does not provide used memory information");
            return 0;
        }

        /// <summary>
        /// Updates and returns GPU temperature in Celsius
        /// </summary>
        public int UpdateTemperature()
        {
            try
            {
                if (_useNativeAPI)
                {
                    switch (_vendor)
                    {
                        case GPUVendor.NVIDIA:
                            return UpdateTemperatureNVML();

                        case GPUVendor.AMD:
                            return UpdateTemperatureADL();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating GPU temperature: {ex.Message}");
            }

            CurrentTemperatureCelsius = 0;
            return 0;
        }

        private int UpdateTemperatureNVML()
        {
            if (_nvmlDevice.HasValue)
            {
                var temp = NVMLInterop.GetTemperature(_nvmlDevice.Value);
                if (temp.HasValue)
                {
                    CurrentTemperatureCelsius = (int)temp.Value;
                    return CurrentTemperatureCelsius;
                }
            }
            return 0;
        }

        private int UpdateTemperatureADL()
        {
            if (_adlAdapterIndex >= 0)
            {
                var temp = ADLInterop.GetTemperature(_adlAdapterIndex);
                if (temp.HasValue)
                {
                    CurrentTemperatureCelsius = temp.Value;
                    return CurrentTemperatureCelsius;
                }
            }
            return 0;
        }

        /// <summary>
        /// Fallback: Get GPU memory from performance counters
        /// </summary>
        private ulong GetGPUMemoryUsageFromPerfCounter()
        {
            try
            {
                if (!string.IsNullOrEmpty(_gpuEngineInstance))
                {
                    string adapterId = ExtractAdapterIdFromEngineInstance(_gpuEngineInstance);

                    if (!string.IsNullOrEmpty(adapterId))
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
                                return (ulong)(usageMB * 1024 * 1024);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting GPU memory from perf counter: {ex.Message}");
            }

            return 0;
        }

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
            catch { }

            return "";
        }

        public string GetShortName()
        {
            string[] removeWords = { "NVIDIA", "GeForce", "AMD", "Radeon", "Intel", "Graphics", "(TM)", "\u2122", "\u00AE" };
            string shortened = GPUName;

            foreach (string word in removeWords)
            {
                shortened = shortened.Replace(word, "").Trim();
            }

            while (shortened.Contains("  "))
                shortened = shortened.Replace("  ", " ");

            if (shortened.Length < 3)
                return GPUName;

            return shortened;
        }

        public void Dispose()
        {
            _gpuUsageCounter?.Dispose();
            _gpuUsageCounter = null;

            // Cleanup native APIs
            if (_useNativeAPI)
            {
                if (_vendor == GPUVendor.NVIDIA)
                {
                    NVMLInterop.Shutdown();
                }
                else if (_vendor == GPUVendor.AMD)
                {
                    ADLInterop.Shutdown();
                }
            }
        }
    }
}
