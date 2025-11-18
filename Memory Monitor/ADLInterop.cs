using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// AMD Display Library (ADL) interop for AMD GPU monitoring
    /// </summary>
    public static class ADLInterop
    {
        private const string ATI_ADL_DLL = "atiadlxx.dll";
        private const int ADL_MAX_PATH = 256;
        private const int ADL_OK = 0;
        private const int ADL_ERR = -1;

        // ADL adapter info structure
        [StructLayout(LayoutKind.Sequential)]
        public struct ADLAdapterInfo
        {
            public int Size;
            public int AdapterIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
            public string UDID;
            public int BusNumber;
            public int DeviceNumber;
            public int FunctionNumber;
            public int VendorID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
            public string AdapterName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
            public string DisplayName;
            public int Present;
            public int Exist;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
            public string DriverPath;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
            public string DriverPathExt;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
            public string PNPString;
            public int OSDisplayIndex;
        }

        // ADL memory info
        [StructLayout(LayoutKind.Sequential)]
        public struct ADLMemoryInfo
        {
            public long MemorySize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL_MAX_PATH)]
            public string MemoryType;
            public long MemoryBandwidth;
        }

        // ADL PM Activity
        [StructLayout(LayoutKind.Sequential)]
        public struct ADLPMActivity
        {
            public int Size;
            public int EngineClock;
            public int MemoryClock;
            public int Vddc;
            public int ActivityPercent;
            public int CurrentPerformanceLevel;
            public int CurrentBusSpeed;
            public int CurrentBusLanes;
            public int MaximumBusLanes;
            public int Reserved;
        }

        // Memory allocation callback
        private delegate IntPtr ADL_Main_Memory_Alloc(int size);

        // P/Invoke declarations
        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Main_Control_Create(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Main_Control_Destroy();

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Adapter_NumberOfAdapters_Get(ref int numAdapters);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Adapter_AdapterInfo_Get(IntPtr info, int size);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Adapter_Active_Get(int adapterIndex, ref int status);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Adapter_MemoryInfo_Get(int adapterIndex, ref ADLMemoryInfo memoryInfo);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL_Overdrive5_CurrentActivity_Get(int adapterIndex, ref ADLPMActivity activity);

        [DllImport(ATI_ADL_DLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ADL2_Overdrive6_CurrentStatus_Get(IntPtr context, int adapterIndex, ref int currentStatus);

        // High-level wrapper
        private static bool _isInitialized = false;
        private static bool _isAvailable = false;

        public static bool IsAvailable => _isAvailable;

        // Memory allocation callback
        private static IntPtr ADL_Main_Memory_Alloc_Callback(int size)
        {
            return Marshal.AllocCoTaskMem(size);
        }

        /// <summary>
        /// Initialize ADL library
        /// </summary>
        public static bool Initialize()
        {
            if (_isInitialized)
                return _isAvailable;

            try
            {
                int result = ADL_Main_Control_Create(ADL_Main_Memory_Alloc_Callback, 1);
                _isAvailable = (result == ADL_OK);
                _isInitialized = true;

                if (_isAvailable)
                {
                    Debug.WriteLine("ADL initialized successfully");
                }
                else
                {
                    Debug.WriteLine($"ADL initialization failed with code: {result}");
                }

                return _isAvailable;
            }
            catch (DllNotFoundException)
            {
                Debug.WriteLine("ADL library (atiadlxx.dll) not found - AMD GPU driver may not be installed");
                _isAvailable = false;
                _isInitialized = true;
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADL initialization error: {ex.Message}");
                _isAvailable = false;
                _isInitialized = true;
                return false;
            }
        }

        /// <summary>
        /// Shutdown ADL library
        /// </summary>
        public static void Shutdown()
        {
            if (_isInitialized && _isAvailable)
            {
                try
                {
                    ADL_Main_Control_Destroy();
                    Debug.WriteLine("ADL shutdown successfully");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ADL shutdown error: {ex.Message}");
                }

                _isInitialized = false;
                _isAvailable = false;
            }
        }

        /// <summary>
        /// Get number of AMD adapters
        /// </summary>
        public static int GetAdapterCount()
        {
            if (!_isAvailable) return 0;

            try
            {
                int numAdapters = 0;
                int result = ADL_Adapter_NumberOfAdapters_Get(ref numAdapters);
                if (result == ADL_OK)
                {
                    return numAdapters;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADL GetAdapterCount error: {ex.Message}");
            }

            return 0;
        }

        /// <summary>
        /// Get adapter information for all adapters
        /// </summary>
        public static ADLAdapterInfo[]? GetAdapterInfo()
        {
            if (!_isAvailable) return null;

            try
            {
                int numAdapters = GetAdapterCount();
                if (numAdapters <= 0) return null;

                int size = Marshal.SizeOf(typeof(ADLAdapterInfo));
                IntPtr buffer = Marshal.AllocCoTaskMem(size * numAdapters);

                try
                {
                    int result = ADL_Adapter_AdapterInfo_Get(buffer, size * numAdapters);
                    if (result == ADL_OK)
                    {
                        ADLAdapterInfo[] adapters = new ADLAdapterInfo[numAdapters];
                        for (int i = 0; i < numAdapters; i++)
                        {
                            IntPtr ptr = new IntPtr(buffer.ToInt64() + (i * size));
                            adapters[i] = (ADLAdapterInfo)Marshal.PtrToStructure(ptr, typeof(ADLAdapterInfo))!;
                        }
                        return adapters;
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(buffer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADL GetAdapterInfo error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Check if adapter is active
        /// </summary>
        public static bool IsAdapterActive(int adapterIndex)
        {
            if (!_isAvailable) return false;

            try
            {
                int status = 0;
                int result = ADL_Adapter_Active_Get(adapterIndex, ref status);
                return (result == ADL_OK && status == 1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADL IsAdapterActive error: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Get memory information
        /// </summary>
        public static ADLMemoryInfo? GetMemoryInfo(int adapterIndex)
        {
            if (!_isAvailable) return null;

            try
            {
                ADLMemoryInfo memInfo = new ADLMemoryInfo();
                int result = ADL_Adapter_MemoryInfo_Get(adapterIndex, ref memInfo);
                if (result == ADL_OK)
                {
                    return memInfo;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADL GetMemoryInfo error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get current GPU activity (usage)
        /// </summary>
        public static ADLPMActivity? GetCurrentActivity(int adapterIndex)
        {
            if (!_isAvailable) return null;

            try
            {
                ADLPMActivity activity = new ADLPMActivity
                {
                    Size = Marshal.SizeOf(typeof(ADLPMActivity))
                };

                int result = ADL_Overdrive5_CurrentActivity_Get(adapterIndex, ref activity);
                if (result == ADL_OK)
                {
                    return activity;
                }
                else
                {
                    Debug.WriteLine($"ADL GetCurrentActivity failed with code: {result}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADL GetCurrentActivity error: {ex.Message}");
            }

            return null;
        }
    }
}
