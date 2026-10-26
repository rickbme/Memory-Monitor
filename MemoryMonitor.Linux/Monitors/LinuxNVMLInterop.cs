using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Memory_Monitor.Linux
{
    /// <summary>
    /// NVIDIA Management Library (NVML) interop for Linux GPU monitoring
    /// Uses libnvidia-ml.so instead of nvml.dll
    /// </summary>
    public static class LinuxNVMLInterop
    {
        private const string NVML_LIB = "libnvidia-ml.so.1";

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

        // Temperature sensor types
        public enum nvmlTemperatureSensors_t
        {
            NVML_TEMPERATURE_GPU = 0,
            NVML_TEMPERATURE_COUNT
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
            public ulong total;
            public ulong free;
            public ulong used;
        }

        // Utilization rates
        [StructLayout(LayoutKind.Sequential)]
        public struct nvmlUtilization_t
        {
            public uint gpu;
            public uint memory;
        }

        // P/Invoke declarations
        [DllImport(NVML_LIB, EntryPoint = "nvmlInit_v2")]
        private static extern nvmlReturn_t nvmlInit();

        [DllImport(NVML_LIB, EntryPoint = "nvmlShutdown")]
        private static extern nvmlReturn_t nvmlShutdown();

        [DllImport(NVML_LIB, EntryPoint = "nvmlDeviceGetCount_v2")]
        private static extern nvmlReturn_t nvmlDeviceGetCount(out uint deviceCount);

        [DllImport(NVML_LIB, EntryPoint = "nvmlDeviceGetHandleByIndex_v2")]
        private static extern nvmlReturn_t nvmlDeviceGetHandleByIndex(uint index, out nvmlDevice_t device);

        [DllImport(NVML_LIB, EntryPoint = "nvmlDeviceGetName")]
        private static extern nvmlReturn_t nvmlDeviceGetName(nvmlDevice_t device, [Out] byte[] name, uint length);

        [DllImport(NVML_LIB, EntryPoint = "nvmlDeviceGetMemoryInfo")]
        private static extern nvmlReturn_t nvmlDeviceGetMemoryInfo(nvmlDevice_t device, out nvmlMemory_t memory);

        [DllImport(NVML_LIB, EntryPoint = "nvmlDeviceGetUtilizationRates")]
        private static extern nvmlReturn_t nvmlDeviceGetUtilizationRates(nvmlDevice_t device, out nvmlUtilization_t utilization);

        [DllImport(NVML_LIB, EntryPoint = "nvmlDeviceGetTemperature")]
        private static extern nvmlReturn_t nvmlDeviceGetTemperature(nvmlDevice_t device, nvmlTemperatureSensors_t sensorType, out uint temp);

        // High-level wrapper
        private static bool _isInitialized = false;
        private static readonly object _lockObject = new object();

        public static bool Initialize()
        {
            lock (_lockObject)
            {
                if (_isInitialized)
                    return true;

                try
                {
                    var result = nvmlInit();
                    if (result == nvmlReturn_t.NVML_SUCCESS || result == nvmlReturn_t.NVML_ERROR_ALREADY_INITIALIZED)
                    {
                        _isInitialized = true;
                        Debug.WriteLine("NVML initialized successfully on Linux");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"NVML initialization failed: {result}");
                        return false;
                    }
                }
                catch (DllNotFoundException)
                {
                    Debug.WriteLine("NVML library not found - NVIDIA drivers may not be installed");
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"NVML initialization error: {ex.Message}");
                    return false;
                }
            }
        }

        public static void Shutdown()
        {
            lock (_lockObject)
            {
                if (_isInitialized)
                {
                    try
                    {
                        nvmlShutdown();
                        _isInitialized = false;
                        Debug.WriteLine("NVML shutdown successfully");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"NVML shutdown error: {ex.Message}");
                    }
                }
            }
        }

        public static uint GetDeviceCount()
        {
            if (!_isInitialized)
                return 0;

            try
            {
                var result = nvmlDeviceGetCount(out uint count);
                return result == nvmlReturn_t.NVML_SUCCESS ? count : 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting device count: {ex.Message}");
                return 0;
            }
        }

        public static nvmlDevice_t? GetDeviceByIndex(uint index)
        {
            if (!_isInitialized)
                return null;

            try
            {
                var result = nvmlDeviceGetHandleByIndex(index, out nvmlDevice_t device);
                return result == nvmlReturn_t.NVML_SUCCESS ? device : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting device {index}: {ex.Message}");
                return null;
            }
        }

        public static string GetDeviceName(nvmlDevice_t device)
        {
            if (!_isInitialized)
                return "Unknown";

            try
            {
                byte[] nameBuffer = new byte[256];
                var result = nvmlDeviceGetName(device, nameBuffer, (uint)nameBuffer.Length);
                if (result == nvmlReturn_t.NVML_SUCCESS)
                {
                    int nullIndex = Array.IndexOf(nameBuffer, (byte)0);
                    if (nullIndex >= 0)
                        return System.Text.Encoding.ASCII.GetString(nameBuffer, 0, nullIndex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting device name: {ex.Message}");
            }
            return "Unknown";
        }

        public static nvmlMemory_t? GetMemoryInfo(nvmlDevice_t device)
        {
            if (!_isInitialized)
                return null;

            try
            {
                var result = nvmlDeviceGetMemoryInfo(device, out nvmlMemory_t memory);
                return result == nvmlReturn_t.NVML_SUCCESS ? memory : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting memory info: {ex.Message}");
                return null;
            }
        }

        public static nvmlUtilization_t? GetUtilizationRates(nvmlDevice_t device)
        {
            if (!_isInitialized)
                return null;

            try
            {
                var result = nvmlDeviceGetUtilizationRates(device, out nvmlUtilization_t utilization);
                return result == nvmlReturn_t.NVML_SUCCESS ? utilization : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting utilization: {ex.Message}");
                return null;
            }
        }

        public static int? GetTemperature(nvmlDevice_t device)
        {
            if (!_isInitialized)
                return null;

            try
            {
                var result = nvmlDeviceGetTemperature(device, nvmlTemperatureSensors_t.NVML_TEMPERATURE_GPU, out uint temp);
                return result == nvmlReturn_t.NVML_SUCCESS ? (int)temp : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting temperature: {ex.Message}");
                return null;
            }
        }

        public static bool IsAvailable => _isInitialized;
    }
}
