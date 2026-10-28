using System.Diagnostics;
using System.Management;

namespace Memory_Monitor
{
    /// <summary>
    /// Monitors GPU usage and memory using native APIs (NVML/ADL) with fallback to WMI
    /// </summary>
    public class GPUMonitor : ISelectableMonitor, IDisposable
    {
        public enum GPUVendor
        {
            Unknown,
            NVIDIA,
            AMD,
            Intel
        }

        /// <summary>
        /// Internal representation of a detected GPU
        /// </summary>
        private class GPUDevice
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "Unknown GPU";
            public GPUVendor Vendor { get; set; } = GPUVendor.Unknown;
            public ulong TotalMemoryBytes { get; set; } = 0;
            public bool IsMemoryAvailable { get; set; } = false;
            public bool IsUsageAvailable { get; set; } = false;
            public bool IsTemperatureAvailable { get; set; } = false;
            public bool UseNativeAPI { get; set; } = false;
            
            // NVIDIA specific
            public NVMLInterop.nvmlDevice_t? NvmlDevice { get; set; } = null;
            
            // AMD specific  
            public int AdlAdapterIndex { get; set; } = -1;
            
            // Performance counter fallback: the adapter's LUID ("luid_0x..._0x...") names its counter instances
            public string AdapterLuid { get; set; } = "";
            public PerformanceCounter? DedicatedUsageCounter { get; set; }
        }

        /// <summary>
        /// Samples the "GPU Engine" counters for all processes at once. Its instances are per process and engine
        /// ("pid_1234_luid_..._eng_0_engtype_3D"), so a GPU's 3D usage is the sum over every instance with its LUID.
        /// </summary>
        private class GpuEngineSampler
        {
            private Dictionary<string, CounterSample> _previousSamples = new();

            /// <summary>
            /// Returns the 3D engine usage (0-100) of the adapter with this LUID since the previous call
            /// </summary>
            public float GetUtilization(string luid)
            {
                var category = new PerformanceCounterCategory("GPU Engine");
                InstanceDataCollection instances = category.ReadCategory()["Utilization Percentage"];

                var currentSamples = new Dictionary<string, CounterSample>();
                float total = 0;

                foreach (InstanceData instance in instances.Values)
                {
                    currentSamples[instance.InstanceName] = instance.Sample;

                    if (instance.InstanceName.Contains(luid, StringComparison.OrdinalIgnoreCase) &&
                        instance.InstanceName.EndsWith("engtype_3D", StringComparison.OrdinalIgnoreCase) &&
                        _previousSamples.TryGetValue(instance.InstanceName, out CounterSample previous))
                    {
                        total += CounterSample.Calculate(previous, instance.Sample);
                    }
                }

                _previousSamples = currentSamples;
                return Math.Clamp(total, 0f, 100f);
            }
        }

        private List<GPUDevice> _detectedGPUs = new List<GPUDevice>();
        private GPUDevice? _selectedGPU = null;
        private List<DeviceInfo> _availableDevices = new List<DeviceInfo>();
        private readonly GpuEngineSampler _engineSampler = new GpuEngineSampler();

        // Whether this instance holds a reference on the shared NVML/ADL libraries (released in Dispose)
        private bool _nvmlInitialized = false;
        private bool _adlInitialized = false;

        public string GPUName => _selectedGPU?.Name ?? "Unknown GPU";
        public ulong TotalMemoryBytes => _selectedGPU?.TotalMemoryBytes ?? 0;
        public bool IsMemoryAvailable => _selectedGPU?.IsMemoryAvailable ?? false;
        public bool IsUsageAvailable => _selectedGPU?.IsUsageAvailable ?? false;
        public bool IsTemperatureAvailable => _selectedGPU?.IsTemperatureAvailable ?? false;
        public bool IsAvailable => IsUsageAvailable || IsMemoryAvailable;
        public GPUVendor Vendor => _selectedGPU?.Vendor ?? GPUVendor.Unknown;

        public float CurrentUsagePercent { get; private set; }
        public ulong CurrentMemoryUsedBytes { get; private set; }
        public int CurrentTemperatureCelsius { get; private set; }

        public double TotalMemoryGB => TotalMemoryBytes / (double)Constants.BYTES_TO_GB;
        public double UsedMemoryGB => CurrentMemoryUsedBytes / (double)Constants.BYTES_TO_GB;
        public int MemoryUsagePercent => TotalMemoryBytes > 0
            ? Math.Min((int)((CurrentMemoryUsedBytes * 100) / TotalMemoryBytes), 100)
            : 0;

        // ISelectableMonitor implementation
        public IReadOnlyList<DeviceInfo> AvailableDevices => _availableDevices;
        public DeviceInfo? SelectedDevice => _selectedGPU != null 
            ? _availableDevices.FirstOrDefault(d => d.Id == _selectedGPU.Id) 
            : null;
        public bool HasMultipleDevices => _detectedGPUs.Count > 1;
        public string CurrentDeviceDisplayName => _selectedGPU?.Name ?? "No GPU";

        public GPUMonitor()
        {
            DetectAllGPUs();
            InitializeAllNativeAPIs();
            BuildDeviceList();
            
            // Select first available GPU by default
            if (_detectedGPUs.Count > 0)
            {
                SelectDevice(_detectedGPUs[0].Id);
            }
        }

        /// <summary>
        /// Detects all GPU hardware via WMI
        /// </summary>
        private void DetectAllGPUs()
        {
            try
            {
                var dxgiAdapters = DxgiAdapters.Enumerate();

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT Name, AdapterRAM, VideoProcessor, PNPDeviceID FROM Win32_VideoController"))
                {
                    int gpuIndex = 0;
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString() ?? "Unknown";
                        string pnpId = obj["PNPDeviceID"]?.ToString() ?? "";
                        object? adapterRAM = obj["AdapterRAM"];

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

                        var gpu = new GPUDevice
                        {
                            // Must be stable across launches so the saved selection can be restored
                            // (string.GetHashCode is randomized per process in .NET, so it can't be used here)
                            Id = string.IsNullOrEmpty(pnpId) ? $"gpu_{gpuIndex}" : $"gpu_{pnpId}",
                            Name = name,
                            Vendor = vendor
                        };

                        // AdapterRAM is a 32-bit value (caps at 4 GB); LinkDxgiAdapter replaces it with the real size
                        if (adapterRAM != null && ulong.TryParse(adapterRAM.ToString(), out ulong ram) && ram > 0)
                        {
                            gpu.TotalMemoryBytes = ram;
                            gpu.IsMemoryAvailable = true;
                        }

                        LinkDxgiAdapter(gpu, pnpId, dxgiAdapters);

                        _detectedGPUs.Add(gpu);
                        Debug.WriteLine($"  Added GPU #{gpuIndex}: {name} ({vendor})");
                        gpuIndex++;
                    }
                }

                Debug.WriteLine($"Total GPUs detected: {_detectedGPUs.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to detect GPUs: {ex.Message}");
            }
        }

        /// <summary>
        /// Links a WMI GPU to its DXGI adapter by PCI vendor/device ID, giving it the adapter LUID used by the
        /// GPU performance counters and the 64-bit dedicated VRAM size. Linked adapters are removed from the
        /// list so two identical cards are matched to different adapters (in enumeration order).
        /// </summary>
        private static void LinkDxgiAdapter(GPUDevice gpu, string pnpId, List<DxgiAdapters.AdapterInfo> dxgiAdapters)
        {
            var match = System.Text.RegularExpressions.Regex.Match(pnpId, @"VEN_([0-9A-F]{4})&DEV_([0-9A-F]{4})",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success)
                return;

            uint vendorId = Convert.ToUInt32(match.Groups[1].Value, 16);
            uint deviceId = Convert.ToUInt32(match.Groups[2].Value, 16);

            var adapter = dxgiAdapters.FirstOrDefault(a => a.VendorId == vendorId && a.DeviceId == deviceId);
            if (adapter == null)
                return;

            dxgiAdapters.Remove(adapter);
            gpu.AdapterLuid = adapter.Luid;

            if (adapter.DedicatedVideoMemory > 0)
            {
                gpu.TotalMemoryBytes = adapter.DedicatedVideoMemory;
                gpu.IsMemoryAvailable = true;
            }

            Debug.WriteLine($"  Linked to DXGI adapter {adapter.Luid}, {adapter.DedicatedVideoMemory / Constants.BYTES_TO_GB} GB VRAM");
        }

        /// <summary>
        /// Initializes native APIs for all detected GPUs
        /// </summary>
        private void InitializeAllNativeAPIs()
        {
            // Initialize NVML for NVIDIA GPUs
            InitializeNVMLForAll();
            
            // Initialize ADL for AMD GPUs
            InitializeADLForAll();
            
            // Initialize performance counters for any GPU without native API
            foreach (var gpu in _detectedGPUs.Where(g => !g.UseNativeAPI))
            {
                InitializeUsageCounterForGPU(gpu);
            }
        }

        private void InitializeNVMLForAll()
        {
            var nvidiaGPUs = _detectedGPUs.Where(g => g.Vendor == GPUVendor.NVIDIA).ToList();
            if (nvidiaGPUs.Count == 0) return;

            try
            {
                _nvmlInitialized = NVMLInterop.Initialize();
                if (_nvmlInitialized)
                {
                    uint deviceCount = NVMLInterop.GetDeviceCount();
                    Debug.WriteLine($"NVML found {deviceCount} NVIDIA GPU(s)");

                    for (uint i = 0; i < deviceCount && i < nvidiaGPUs.Count; i++)
                    {
                        var gpu = nvidiaGPUs[(int)i];
                        var nvmlDevice = NVMLInterop.GetDeviceByIndex(i);

                        if (nvmlDevice.HasValue)
                        {
                            gpu.NvmlDevice = nvmlDevice;

                            string? nvmlName = NVMLInterop.GetDeviceName(nvmlDevice.Value);
                            if (nvmlName != null)
                            {
                                gpu.Name = nvmlName;
                            }

                            var memInfo = NVMLInterop.GetMemoryInfo(nvmlDevice.Value);
                            if (memInfo.HasValue)
                            {
                                gpu.TotalMemoryBytes = memInfo.Value.total;
                                gpu.IsMemoryAvailable = true;
                            }

                            var tempCheck = NVMLInterop.GetTemperature(nvmlDevice.Value);
                            gpu.IsTemperatureAvailable = tempCheck.HasValue;

                            gpu.UseNativeAPI = true;
                            gpu.IsUsageAvailable = true;
                            Debug.WriteLine($"NVML initialized for GPU: {gpu.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVML initialization failed: {ex.Message}");
            }
        }

        private void InitializeADLForAll()
        {
            var amdGPUs = _detectedGPUs.Where(g => g.Vendor == GPUVendor.AMD).ToList();
            if (amdGPUs.Count == 0) return;

            try
            {
                _adlInitialized = ADLInterop.Initialize();
                if (_adlInitialized)
                {
                    var adapters = ADLInterop.GetAdapterInfo();
                    if (adapters != null && adapters.Length > 0)
                    {
                        Debug.WriteLine($"ADL found {adapters.Length} AMD adapter(s)");

                        int amdIndex = 0;
                        for (int i = 0; i < adapters.Length && amdIndex < amdGPUs.Count; i++)
                        {
                            if (ADLInterop.IsAdapterActive(adapters[i].AdapterIndex))
                            {
                                var gpu = amdGPUs[amdIndex];
                                gpu.AdlAdapterIndex = adapters[i].AdapterIndex;
                                gpu.Name = adapters[i].AdapterName;

                                var memInfo = ADLInterop.GetMemoryInfo(gpu.AdlAdapterIndex);
                                if (memInfo.HasValue && memInfo.Value.MemorySize > 0)
                                {
                                    gpu.TotalMemoryBytes = (ulong)memInfo.Value.MemorySize;
                                    gpu.IsMemoryAvailable = true;
                                }

                                var tempCheck = ADLInterop.GetTemperature(gpu.AdlAdapterIndex);
                                gpu.IsTemperatureAvailable = tempCheck.HasValue;

                                gpu.UseNativeAPI = true;
                                gpu.IsUsageAvailable = true;
                                Debug.WriteLine($"ADL initialized for GPU: {gpu.Name}");
                                amdIndex++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADL initialization failed: {ex.Message}");
            }
        }

        private void InitializeUsageCounterForGPU(GPUDevice gpu)
        {
            // Without the adapter's LUID there's no way to tell which GPU's counters are which
            if (string.IsNullOrEmpty(gpu.AdapterLuid))
                return;

            try
            {
                // Take a first sample so the next call can compute a rate
                _engineSampler.GetUtilization(gpu.AdapterLuid);
                gpu.IsUsageAvailable = true;
                Debug.WriteLine($"Performance counter usage initialized for GPU: {gpu.Name} ({gpu.AdapterLuid})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize usage counter for {gpu.Name}: {ex.Message}");
            }
        }

        private void BuildDeviceList()
        {
            _availableDevices.Clear();

            foreach (var gpu in _detectedGPUs)
            {
                _availableDevices.Add(new DeviceInfo
                {
                    Id = gpu.Id,
                    DisplayName = gpu.Name,
                    ShortName = GetShortName(gpu.Name),
                    Description = $"{gpu.Vendor} • {gpu.TotalMemoryBytes / Constants.BYTES_TO_GB:F0} GB",
                    Type = DeviceType.GPU,
                    IsActive = gpu.IsUsageAvailable || gpu.IsMemoryAvailable
                });
            }
        }

        public bool SelectDevice(string? deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                // Select first GPU if null/empty
                _selectedGPU = _detectedGPUs.FirstOrDefault();
                return _selectedGPU != null;
            }

            var gpu = _detectedGPUs.FirstOrDefault(g => g.Id == deviceId);
            if (gpu != null)
            {
                _selectedGPU = gpu;
                Debug.WriteLine($"Selected GPU: {gpu.Name}");
                return true;
            }

            return false;
        }

        public DeviceInfo? CycleToNextDevice()
        {
            if (_detectedGPUs.Count <= 1)
                return SelectedDevice;

            // Find current GPU index
            int currentIndex = _detectedGPUs.FindIndex(g => g.Id == _selectedGPU?.Id);
            if (currentIndex < 0)
                currentIndex = 0;

            // Move to next GPU (wrap around)
            int nextIndex = (currentIndex + 1) % _detectedGPUs.Count;
            var nextGPU = _detectedGPUs[nextIndex];

            // Select the next GPU
            SelectDevice(nextGPU.Id);

            Debug.WriteLine($"Cycled to GPU: {CurrentDeviceDisplayName}");
            return SelectedDevice;
        }

        /// <summary>
        /// Updates GPU usage percentage
        /// </summary>
        public float UpdateUsage()
        {
            if (_selectedGPU == null) return 0;

            try
            {
                if (_selectedGPU.UseNativeAPI)
                {
                    switch (_selectedGPU.Vendor)
                    {
                        case GPUVendor.NVIDIA:
                            return UpdateUsageNVML(_selectedGPU);
                        case GPUVendor.AMD:
                            return UpdateUsageADL(_selectedGPU);
                    }
                }

                if (_selectedGPU.IsUsageAvailable && !string.IsNullOrEmpty(_selectedGPU.AdapterLuid))
                {
                    CurrentUsagePercent = _engineSampler.GetUtilization(_selectedGPU.AdapterLuid);
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

        private float UpdateUsageNVML(GPUDevice gpu)
        {
            if (gpu.NvmlDevice.HasValue)
            {
                var utilization = NVMLInterop.GetUtilization(gpu.NvmlDevice.Value);
                if (utilization.HasValue)
                {
                    CurrentUsagePercent = utilization.Value.gpu;
                    return CurrentUsagePercent;
                }
            }
            return 0;
        }

        private float UpdateUsageADL(GPUDevice gpu)
        {
            if (gpu.AdlAdapterIndex >= 0)
            {
                var activity = ADLInterop.GetCurrentActivity(gpu.AdlAdapterIndex);
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
            if (_selectedGPU == null) return 0;

            try
            {
                if (_selectedGPU.UseNativeAPI)
                {
                    switch (_selectedGPU.Vendor)
                    {
                        case GPUVendor.NVIDIA:
                            return UpdateMemoryNVML(_selectedGPU);
                        case GPUVendor.AMD:
                            return UpdateMemoryADL(_selectedGPU);
                    }
                }

                if (_selectedGPU.IsMemoryAvailable && _selectedGPU.TotalMemoryBytes > 0)
                {
                    CurrentMemoryUsedBytes = GetGPUMemoryUsageFromPerfCounter(_selectedGPU);
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

        private ulong UpdateMemoryNVML(GPUDevice gpu)
        {
            if (gpu.NvmlDevice.HasValue)
            {
                var memInfo = NVMLInterop.GetMemoryInfo(gpu.NvmlDevice.Value);
                if (memInfo.HasValue)
                {
                    CurrentMemoryUsedBytes = memInfo.Value.used;
                    return CurrentMemoryUsedBytes;
                }
            }
            return 0;
        }

        private ulong UpdateMemoryADL(GPUDevice gpu)
        {
            if (gpu.AdlAdapterIndex >= 0)
            {
                CurrentMemoryUsedBytes = GetGPUMemoryUsageFromPerfCounter(gpu);
                if (CurrentMemoryUsedBytes > 0)
                {
                    return CurrentMemoryUsedBytes;
                }
            }
            return 0;
        }

        /// <summary>
        /// Updates and returns GPU temperature in Celsius
        /// </summary>
        public int UpdateTemperature()
        {
            if (_selectedGPU == null) return 0;

            try
            {
                if (_selectedGPU.UseNativeAPI)
                {
                    switch (_selectedGPU.Vendor)
                    {
                        case GPUVendor.NVIDIA:
                            return UpdateTemperatureNVML(_selectedGPU);
                        case GPUVendor.AMD:
                            return UpdateTemperatureADL(_selectedGPU);
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

        private int UpdateTemperatureNVML(GPUDevice gpu)
        {
            if (gpu.NvmlDevice.HasValue)
            {
                var temp = NVMLInterop.GetTemperature(gpu.NvmlDevice.Value);
                if (temp.HasValue)
                {
                    CurrentTemperatureCelsius = (int)temp.Value;
                    return CurrentTemperatureCelsius;
                }
            }
            return 0;
        }

        private int UpdateTemperatureADL(GPUDevice gpu)
        {
            if (gpu.AdlAdapterIndex >= 0)
            {
                var temp = ADLInterop.GetTemperature(gpu.AdlAdapterIndex);
                if (temp.HasValue)
                {
                    CurrentTemperatureCelsius = temp.Value;
                    return CurrentTemperatureCelsius;
                }
            }
            return 0;
        }

        private ulong GetGPUMemoryUsageFromPerfCounter(GPUDevice gpu)
        {
            if (string.IsNullOrEmpty(gpu.AdapterLuid))
                return 0;

            try
            {
                if (gpu.DedicatedUsageCounter == null)
                {
                    // Instance names look like "luid_0x00000000_0x0000D1E3_phys_0"
                    var memCategory = new PerformanceCounterCategory("GPU Adapter Memory");
                    string? instance = memCategory.GetInstanceNames()
                        .FirstOrDefault(i => i.StartsWith(gpu.AdapterLuid, StringComparison.OrdinalIgnoreCase));
                    if (instance == null)
                        return 0;

                    gpu.DedicatedUsageCounter = new PerformanceCounter("GPU Adapter Memory", "Dedicated Usage", instance);
                }

                // "Dedicated Usage" is a plain count of bytes
                return (ulong)Math.Max(0, gpu.DedicatedUsageCounter.RawValue);
            }
            catch (Exception ex)
            {
                // The instance can vanish (e.g. after a driver reset); look it up again next time
                Debug.WriteLine($"Error getting GPU memory from perf counter: {ex.Message}");
                gpu.DedicatedUsageCounter?.Dispose();
                gpu.DedicatedUsageCounter = null;
            }

            return 0;
        }

        public string GetShortName() => GetShortName(GPUName);

        private string GetShortName(string gpuName)
        {
            string[] removeWords = { "NVIDIA", "GeForce", "AMD", "Radeon", "Intel", "Graphics", "(TM)", "\u2122", "\u00AE" };
            string shortened = gpuName;

            foreach (string word in removeWords)
            {
                shortened = shortened.Replace(word, "").Trim();
            }

            while (shortened.Contains("  "))
                shortened = shortened.Replace("  ", " ");

            if (shortened.Length < 3)
                return gpuName;

            return shortened;
        }

        public void Dispose()
        {
            foreach (var gpu in _detectedGPUs)
            {
                gpu.DedicatedUsageCounter?.Dispose();
            }
            _detectedGPUs.Clear();

            // Release this instance's reference on the native APIs; the library itself only shuts down
            // once no other GPUMonitor (e.g. the form being switched to) is still using it
            if (_nvmlInitialized)
            {
                NVMLInterop.Shutdown();
                _nvmlInitialized = false;
            }
            if (_adlInitialized)
            {
                ADLInterop.Shutdown();
                _adlInitialized = false;
            }
        }
    }
}
