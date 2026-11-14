using System.Diagnostics;

namespace Memory_Monitor.Linux
{
    /// <summary>
    /// One GPU's readings from the latest update
    /// </summary>
    public class GpuReading
    {
        public string Id { get; init; } = "";
        public string ShortName { get; init; } = "";
        public bool IsUsageAvailable { get; init; }
        public bool IsMemoryAvailable { get; init; }
        public bool IsTemperatureAvailable { get; init; }
        public float UsagePercent { get; init; }
        public int TemperatureCelsius { get; init; }
        public double UsedMemoryGB { get; init; }
        public double TotalMemoryGB { get; init; }
    }

    /// <summary>
    /// Monitors GPU usage and memory on Linux using NVML for NVIDIA and sysfs for AMD
    /// </summary>
    public class LinuxGPUMonitor : ISelectableMonitor, IDisposable
    {
        public enum GPUVendor
        {
            Unknown,
            NVIDIA,
            AMD,
            Intel
        }

        private class GPUDevice
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "Unknown GPU";
            public GPUVendor Vendor { get; set; } = GPUVendor.Unknown;
            public ulong TotalMemoryBytes { get; set; } = 0;
            public bool IsMemoryAvailable { get; set; } = false;
            public bool IsUsageAvailable { get; set; } = false;
            public bool IsTemperatureAvailable { get; set; } = false;

            // Latest readings
            public float UsagePercent { get; set; }
            public ulong MemoryUsedBytes { get; set; }
            public int TemperatureCelsius { get; set; }
            
            // NVIDIA specific
            public LinuxNVMLInterop.nvmlDevice_t? NvmlDevice { get; set; } = null;
            
            // AMD specific  
            public string? SysfsPath { get; set; } = null;
            public int CardIndex { get; set; } = -1;
        }

        private List<GPUDevice> _detectedGPUs = new List<GPUDevice>();
        private GPUDevice? _selectedGPU = null;
        private List<DeviceInfo> _availableDevices = new List<DeviceInfo>();

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
        public int GpuCount => _detectedGPUs.Count;
        public string CurrentDeviceDisplayName => _selectedGPU?.Name ?? "No GPU";

        public LinuxGPUMonitor()
        {
            DetectAllGPUs();
            InitializeAllAPIs();
            BuildDeviceList();
            
            // Select first available GPU by default
            if (_detectedGPUs.Count > 0)
            {
                SelectDevice(_detectedGPUs[0].Id);
            }
        }

        private void DetectAllGPUs()
        {
            DetectNVIDIAGPUs();
            DetectAMDGPUs();
        }

        private void DetectNVIDIAGPUs()
        {
            try
            {
                if (LinuxNVMLInterop.Initialize())
                {
                    uint deviceCount = LinuxNVMLInterop.GetDeviceCount();
                    Debug.WriteLine($"NVML found {deviceCount} NVIDIA GPU(s)");

                    for (uint i = 0; i < deviceCount; i++)
                    {
                        var nvmlDevice = LinuxNVMLInterop.GetDeviceByIndex(i);
                        if (nvmlDevice.HasValue)
                        {
                            string name = LinuxNVMLInterop.GetDeviceName(nvmlDevice.Value);
                            var gpu = new GPUDevice
                            {
                                Id = $"nvidia_{i}",
                                Name = name,
                                Vendor = GPUVendor.NVIDIA,
                                NvmlDevice = nvmlDevice.Value
                            };

                            var memInfo = LinuxNVMLInterop.GetMemoryInfo(nvmlDevice.Value);
                            if (memInfo.HasValue)
                            {
                                gpu.TotalMemoryBytes = memInfo.Value.total;
                                gpu.IsMemoryAvailable = true;
                            }

                            var tempCheck = LinuxNVMLInterop.GetTemperature(nvmlDevice.Value);
                            gpu.IsTemperatureAvailable = tempCheck.HasValue;
                            gpu.IsUsageAvailable = true;

                            _detectedGPUs.Add(gpu);
                            Debug.WriteLine($"Added NVIDIA GPU: {name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to detect NVIDIA GPUs: {ex.Message}");
            }
        }

        private void DetectAMDGPUs()
        {
            try
            {
                // AMD GPUs are exposed via /sys/class/drm/cardX
                var drmPath = "/sys/class/drm";
                if (!Directory.Exists(drmPath))
                    return;

                var cards = Directory.GetDirectories(drmPath, "card*")
                    .Where(d => !d.Contains("-"))  // Skip card0-HDMI-A-1 etc
                    .OrderBy(d => d)
                    .ToList();

                foreach (var cardPath in cards)
                {
                    try
                    {
                        var vendorPath = Path.Combine(cardPath, "device", "vendor");
                        if (!File.Exists(vendorPath))
                            continue;

                        var vendor = File.ReadAllText(vendorPath).Trim();
                        // AMD vendor ID is 0x1002
                        if (vendor != "0x1002")
                            continue;

                        var namePath = Path.Combine(cardPath, "device", "product_name");
                        var modelPath = Path.Combine(cardPath, "device", "model");
                        
                        string name = "AMD GPU";
                        if (File.Exists(namePath))
                            name = File.ReadAllText(namePath).Trim();
                        else if (File.Exists(modelPath))
                            name = File.ReadAllText(modelPath).Trim();

                        var cardName = Path.GetFileName(cardPath);
                        var cardIndex = int.Parse(cardName.Substring(4)); // "card0" -> 0

                        var gpu = new GPUDevice
                        {
                            Id = $"amd_{cardIndex}",
                            Name = name,
                            Vendor = GPUVendor.AMD,
                            SysfsPath = cardPath,
                            CardIndex = cardIndex
                        };

                        // Try to get VRAM info
                        var vramTotalPath = Path.Combine(cardPath, "device", "mem_info_vram_total");
                        if (File.Exists(vramTotalPath))
                        {
                            var vramStr = File.ReadAllText(vramTotalPath).Trim();
                            if (ulong.TryParse(vramStr, out ulong vram))
                            {
                                gpu.TotalMemoryBytes = vram;
                                gpu.IsMemoryAvailable = true;
                            }
                        }

                        // Check for usage availability
                        var gpuBusyPath = Path.Combine(cardPath, "device", "gpu_busy_percent");
                        gpu.IsUsageAvailable = File.Exists(gpuBusyPath);

                        // Check for temperature availability
                        var tempPath = Path.Combine(cardPath, "device", "hwmon");
                        if (Directory.Exists(tempPath))
                        {
                            var hwmonDirs = Directory.GetDirectories(tempPath);
                            if (hwmonDirs.Length > 0)
                            {
                                var temp1Path = Path.Combine(hwmonDirs[0], "temp1_input");
                                gpu.IsTemperatureAvailable = File.Exists(temp1Path);
                            }
                        }

                        _detectedGPUs.Add(gpu);
                        Debug.WriteLine($"Added AMD GPU: {name}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing {cardPath}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to detect AMD GPUs: {ex.Message}");
            }
        }

        private void InitializeAllAPIs()
        {
            // APIs are initialized during detection on Linux
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
                    Description = $"{gpu.Vendor} GPU"
                });
            }
        }

        /// <summary>
        /// Cycles to the next available GPU, wrapping around at the end of the list
        /// </summary>
        public DeviceInfo? CycleToNextDevice()
        {
            if (_availableDevices.Count <= 1)
                return SelectedDevice;

            int currentIndex = _availableDevices.FindIndex(d => d.Id == _selectedGPU?.Id);
            var nextDevice = _availableDevices[(currentIndex + 1) % _availableDevices.Count];
            SelectDevice(nextDevice.Id);

            Debug.WriteLine($"Cycled to GPU: {CurrentDeviceDisplayName}");
            return SelectedDevice;
        }

        public bool SelectDevice(string? deviceId)
        {
            var gpu = _detectedGPUs.FirstOrDefault(g => g.Id == deviceId);
            if (gpu != null)
            {
                _selectedGPU = gpu;
                Debug.WriteLine($"Selected GPU: {deviceId}");
                return true;
            }
            return false;
        }

        public void Update()
        {
            if (_selectedGPU == null)
            {
                CurrentUsagePercent = 0;
                CurrentMemoryUsedBytes = 0;
                CurrentTemperatureCelsius = 0;
                return;
            }

            UpdateDevice(_selectedGPU);
            CurrentUsagePercent = _selectedGPU.UsagePercent;
            CurrentMemoryUsedBytes = _selectedGPU.MemoryUsedBytes;
            CurrentTemperatureCelsius = _selectedGPU.TemperatureCelsius;
        }

        /// <summary>
        /// Updates every detected GPU and returns their readings in detection order (NVIDIA cards by
        /// NVML index, which follows PCI bus order, then AMD cards)
        /// </summary>
        public IReadOnlyList<GpuReading> UpdateAll()
        {
            var readings = new List<GpuReading>(_detectedGPUs.Count);
            foreach (var gpu in _detectedGPUs)
            {
                UpdateDevice(gpu);
                readings.Add(new GpuReading
                {
                    Id = gpu.Id,
                    ShortName = GetShortName(gpu.Name),
                    IsUsageAvailable = gpu.IsUsageAvailable,
                    IsMemoryAvailable = gpu.IsMemoryAvailable,
                    IsTemperatureAvailable = gpu.IsTemperatureAvailable,
                    UsagePercent = gpu.UsagePercent,
                    TemperatureCelsius = gpu.TemperatureCelsius,
                    UsedMemoryGB = gpu.MemoryUsedBytes / (double)Constants.BYTES_TO_GB,
                    TotalMemoryGB = gpu.TotalMemoryBytes / (double)Constants.BYTES_TO_GB
                });
            }
            return readings;
        }

        private static void UpdateDevice(GPUDevice gpu)
        {
            try
            {
                if (gpu.Vendor == GPUVendor.NVIDIA && gpu.NvmlDevice.HasValue)
                {
                    UpdateNVIDIA(gpu);
                }
                else if (gpu.Vendor == GPUVendor.AMD && gpu.SysfsPath != null)
                {
                    UpdateAMD(gpu);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating GPU stats for {gpu.Id}: {ex.Message}");
            }
        }

        private static void UpdateNVIDIA(GPUDevice gpu)
        {
            if (!gpu.NvmlDevice.HasValue)
                return;

            var device = gpu.NvmlDevice.Value;

            // Get utilization
            var util = LinuxNVMLInterop.GetUtilizationRates(device);
            if (util.HasValue)
            {
                gpu.UsagePercent = util.Value.gpu;
            }

            // Get memory
            var mem = LinuxNVMLInterop.GetMemoryInfo(device);
            if (mem.HasValue)
            {
                gpu.MemoryUsedBytes = mem.Value.used;
            }

            // Get temperature
            var temp = LinuxNVMLInterop.GetTemperature(device);
            if (temp.HasValue)
            {
                gpu.TemperatureCelsius = temp.Value;
            }
        }

        private static void UpdateAMD(GPUDevice gpu)
        {
            if (gpu.SysfsPath == null)
                return;

            // Read GPU usage
            var gpuBusyPath = Path.Combine(gpu.SysfsPath, "device", "gpu_busy_percent");
            if (File.Exists(gpuBusyPath))
            {
                var busyStr = File.ReadAllText(gpuBusyPath).Trim();
                if (int.TryParse(busyStr, out int busy))
                {
                    gpu.UsagePercent = Math.Min(100, Math.Max(0, busy));
                }
            }

            // Read VRAM usage
            var vramUsedPath = Path.Combine(gpu.SysfsPath, "device", "mem_info_vram_used");
            if (File.Exists(vramUsedPath))
            {
                var vramStr = File.ReadAllText(vramUsedPath).Trim();
                if (ulong.TryParse(vramStr, out ulong vramUsed))
                {
                    gpu.MemoryUsedBytes = vramUsed;
                }
            }

            // Read temperature
            var tempPath = Path.Combine(gpu.SysfsPath, "device", "hwmon");
            if (Directory.Exists(tempPath))
            {
                var hwmonDirs = Directory.GetDirectories(tempPath);
                if (hwmonDirs.Length > 0)
                {
                    var temp1Path = Path.Combine(hwmonDirs[0], "temp1_input");
                    if (File.Exists(temp1Path))
                    {
                        var tempStr = File.ReadAllText(temp1Path).Trim();
                        if (int.TryParse(tempStr, out int tempMilliC))
                        {
                            gpu.TemperatureCelsius = tempMilliC / 1000;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a shortened GPU name for display under the gauge (e.g., "NVIDIA GeForce RTX 3060" -> "RTX 3060")
        /// </summary>
        public string GetShortName() => GetShortName(GPUName);

        private static string GetShortName(string gpuName)
        {
            string[] removeWords = { "NVIDIA", "GeForce", "AMD", "Radeon", "Intel", "Graphics", "(TM)", "™", "®" };
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
            if (_detectedGPUs.Any(g => g.Vendor == GPUVendor.NVIDIA))
            {
                LinuxNVMLInterop.Shutdown();
            }
        }
    }
}
