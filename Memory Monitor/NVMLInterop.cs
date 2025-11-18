using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// NVIDIA Management Library (NVML) interop for direct GPU monitoring
    /// </summary>
    public static class NVMLInterop
    {
        private const string NVML_DLL = "nvml.dll";

        // NVML Return codes
        public enum nvmlReturn_t
        {
            NVML_SUCCESS = 0,
            NVML_ERROR_UNINITIALIZED = 1,
            NVML_ERROR_INVALID_ARGUMENT = 2,
            NVML_ERROR_NOT_SUPPORTED = 3,
            NVML_ERROR_NO_PERMISSION = 4,
            NVML_ERROR_ALREADY_INITIALIZED = 5,
            NVML_ERROR_NOT_FOUND = 6,
            NVML_ERROR_INSUFFICIENT_SIZE = 7,
            NVML_ERROR_INSUFFICIENT_POWER = 8,
            NVML_ERROR_DRIVER_NOT_LOADED = 9,
            NVML_ERROR_TIMEOUT = 10,
            NVML_ERROR_IRQ_ISSUE = 11,
            NVML_ERROR_LIBRARY_NOT_FOUND = 12,
            NVML_ERROR_FUNCTION_NOT_FOUND = 13,
            NVML_ERROR_CORRUPTED_INFOROM = 14,
            NVML_ERROR_GPU_IS_LOST = 15,
            NVML_ERROR_RESET_REQUIRED = 16,
            NVML_ERROR_OPERATING_SYSTEM = 17,
            NVML_ERROR_LIB_RM_VERSION_MISMATCH = 18,
            NVML_ERROR_IN_USE = 19,
            NVML_ERROR_MEMORY = 20,
            NVML_ERROR_NO_DATA = 21,
            NVML_ERROR_VGPU_ECC_NOT_SUPPORTED = 22,
            NVML_ERROR_INSUFFICIENT_RESOURCES = 23,
            NVML_ERROR_UNKNOWN = 999
        }

        // Device handle
        [StructLayout(LayoutKind.Sequential)]
        public struct nvmlDevice_t
        {
            public IntPtr Handle;
        }

        // Memory info
        [StructLayout(LayoutKind.Sequential)]
        public struct nvmlMemory_t
        {
            public ulong total;     // Total memory in bytes
            public ulong free;      // Free memory in bytes
            public ulong used;      // Used memory in bytes
        }

        // Utilization rates
        [StructLayout(LayoutKind.Sequential)]
        public struct nvmlUtilization_t
        {
            public uint gpu;        // GPU utilization (0-100%)
            public uint memory;     // Memory utilization (0-100%)
        }

        // P/Invoke declarations
        [DllImport(NVML_DLL, EntryPoint = "nvmlInit_v2")]
        private static extern nvmlReturn_t nvmlInit();

        [DllImport(NVML_DLL, EntryPoint = "nvmlShutdown")]
        private static extern nvmlReturn_t nvmlShutdown();

        [DllImport(NVML_DLL, EntryPoint = "nvmlDeviceGetCount_v2")]
        private static extern nvmlReturn_t nvmlDeviceGetCount(out uint deviceCount);

        [DllImport(NVML_DLL, EntryPoint = "nvmlDeviceGetHandleByIndex_v2")]
        private static extern nvmlReturn_t nvmlDeviceGetHandleByIndex(uint index, out nvmlDevice_t device);

        [DllImport(NVML_DLL, EntryPoint = "nvmlDeviceGetName")]
        private static extern nvmlReturn_t nvmlDeviceGetName(nvmlDevice_t device, [Out] byte[] name, uint length);

        [DllImport(NVML_DLL, EntryPoint = "nvmlDeviceGetMemoryInfo")]
        private static extern nvmlReturn_t nvmlDeviceGetMemoryInfo(nvmlDevice_t device, out nvmlMemory_t memory);

        [DllImport(NVML_DLL, EntryPoint = "nvmlDeviceGetUtilizationRates")]
        private static extern nvmlReturn_t nvmlDeviceGetUtilizationRates(nvmlDevice_t device, out nvmlUtilization_t utilization);

        // High-level wrapper
        private static bool _isInitialized = false;
        private static bool _isAvailable = false;

        public static bool IsAvailable => _isAvailable;

        /// <summary>
        /// Initialize NVML library
        /// </summary>
        public static bool Initialize()
        {
            if (_isInitialized)
                return _isAvailable;

            try
            {
                nvmlReturn_t result = nvmlInit();
                _isAvailable = (result == nvmlReturn_t.NVML_SUCCESS);
                _isInitialized = true;

                if (_isAvailable)
                {
                    Debug.WriteLine("NVML initialized successfully");
                }
                else
                {
                    Debug.WriteLine($"NVML initialization failed: {result}");
                }

                return _isAvailable;
            }
            catch (DllNotFoundException)
            {
                Debug.WriteLine("NVML library (nvml.dll) not found - NVIDIA GPU driver may not be installed");
                _isAvailable = false;
                _isInitialized = true;
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVML initialization error: {ex.Message}");
                _isAvailable = false;
                _isInitialized = true;
                return false;
            }
        }

        /// <summary>
        /// Shutdown NVML library
        /// </summary>
        public static void Shutdown()
        {
            if (_isInitialized && _isAvailable)
            {
                try
                {
                    nvmlShutdown();
                    Debug.WriteLine("NVML shutdown successfully");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"NVML shutdown error: {ex.Message}");
                }

                _isInitialized = false;
                _isAvailable = false;
            }
        }

        /// <summary>
        /// Get number of NVIDIA GPUs
        /// </summary>
        public static uint GetDeviceCount()
        {
            if (!_isAvailable) return 0;

            try
            {
                nvmlReturn_t result = nvmlDeviceGetCount(out uint count);
                if (result == nvmlReturn_t.NVML_SUCCESS)
                {
                    return count;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVML GetDeviceCount error: {ex.Message}");
            }

            return 0;
        }

        /// <summary>
        /// Get device handle by index
        /// </summary>
        public static nvmlDevice_t? GetDeviceByIndex(uint index)
        {
            if (!_isAvailable) return null;

            try
            {
                nvmlReturn_t result = nvmlDeviceGetHandleByIndex(index, out nvmlDevice_t device);
                if (result == nvmlReturn_t.NVML_SUCCESS)
                {
                    return device;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVML GetDeviceByIndex error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get device name
        /// </summary>
        public static string? GetDeviceName(nvmlDevice_t device)
        {
            if (!_isAvailable) return null;

            try
            {
                byte[] name = new byte[256];
                nvmlReturn_t result = nvmlDeviceGetName(device, name, 256);
                if (result == nvmlReturn_t.NVML_SUCCESS)
                {
                    return System.Text.Encoding.ASCII.GetString(name).TrimEnd('\0');
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVML GetDeviceName error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get memory information
        /// </summary>
        public static nvmlMemory_t? GetMemoryInfo(nvmlDevice_t device)
        {
            if (!_isAvailable) return null;

            try
            {
                nvmlReturn_t result = nvmlDeviceGetMemoryInfo(device, out nvmlMemory_t memory);
                if (result == nvmlReturn_t.NVML_SUCCESS)
                {
                    return memory;
                }
                else
                {
                    Debug.WriteLine($"NVML GetMemoryInfo failed: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVML GetMemoryInfo error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get utilization rates (GPU and memory)
        /// </summary>
        public static nvmlUtilization_t? GetUtilization(nvmlDevice_t device)
        {
            if (!_isAvailable) return null;

            try
            {
                nvmlReturn_t result = nvmlDeviceGetUtilizationRates(device, out nvmlUtilization_t utilization);
                if (result == nvmlReturn_t.NVML_SUCCESS)
                {
                    return utilization;
                }
                else
                {
                    Debug.WriteLine($"NVML GetUtilization failed: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NVML GetUtilization error: {ex.Message}");
            }

            return null;
        }
    }
}
