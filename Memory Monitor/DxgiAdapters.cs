using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Memory_Monitor
{
    /// <summary>
    /// Enumerates GPUs through DXGI. Used to link a WMI video controller (by PCI vendor/device ID) to its
    /// adapter LUID, which is how the "GPU Engine" and "GPU Adapter Memory" performance counters name their
    /// instances, and to get the real dedicated VRAM size (WMI's AdapterRAM is 32-bit and caps at 4 GB).
    /// </summary>
    public static class DxgiAdapters
    {
        public class AdapterInfo
        {
            public string Description { get; init; } = "";
            public uint VendorId { get; init; }
            public uint DeviceId { get; init; }
            public ulong DedicatedVideoMemory { get; init; }

            /// <summary>
            /// LUID in the form used by GPU performance counter instance names, e.g. "luid_0x00000000_0x0000D1E3"
            /// </summary>
            public string Luid { get; init; } = "";
        }

        #region DXGI Interop

        private const int DXGI_ERROR_NOT_FOUND = unchecked((int)0x887A0002);
        private const uint DXGI_ADAPTER_FLAG_SOFTWARE = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DXGI_ADAPTER_DESC1
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string Description;
            public uint VendorId;
            public uint DeviceId;
            public uint SubSysId;
            public uint Revision;
            public UIntPtr DedicatedVideoMemory;
            public UIntPtr DedicatedSystemMemory;
            public UIntPtr SharedSystemMemory;
            public LUID AdapterLuid;
            public uint Flags;
        }

        // Only the methods we call are typed; the others are placeholders that keep the vtable slots in order
        [ComImport, Guid("770aae78-f26f-4dba-a829-253c83d1b387"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDXGIFactory1
        {
            void SetPrivateData();
            void SetPrivateDataInterface();
            void GetPrivateData();
            void GetParent();
            void EnumAdapters();
            void MakeWindowAssociation();
            void GetWindowAssociation();
            void CreateSwapChain();
            void CreateSoftwareAdapter();

            [PreserveSig]
            int EnumAdapters1(uint adapterIndex, out IDXGIAdapter1 adapter);
        }

        [ComImport, Guid("29038f61-3839-4626-91fd-086879011a05"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDXGIAdapter1
        {
            void SetPrivateData();
            void SetPrivateDataInterface();
            void GetPrivateData();
            void GetParent();
            void EnumOutputs();
            void GetDesc();
            void CheckInterfaceSupport();

            [PreserveSig]
            int GetDesc1(out DXGI_ADAPTER_DESC1 desc);
        }

        [DllImport("dxgi.dll")]
        private static extern int CreateDXGIFactory1(ref Guid riid, out IDXGIFactory1 factory);

        #endregion

        /// <summary>
        /// Returns all hardware (non-software) adapters, or an empty list if DXGI isn't available
        /// </summary>
        public static List<AdapterInfo> Enumerate()
        {
            var result = new List<AdapterInfo>();
            IDXGIFactory1? factory = null;

            try
            {
                Guid factoryGuid = typeof(IDXGIFactory1).GUID;
                if (CreateDXGIFactory1(ref factoryGuid, out factory) < 0)
                    return result;

                for (uint i = 0; ; i++)
                {
                    int hr = factory.EnumAdapters1(i, out IDXGIAdapter1 adapter);
                    if (hr == DXGI_ERROR_NOT_FOUND || hr < 0)
                        break;

                    try
                    {
                        if (adapter.GetDesc1(out DXGI_ADAPTER_DESC1 desc) >= 0 &&
                            (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE) == 0)
                        {
                            result.Add(new AdapterInfo
                            {
                                Description = desc.Description ?? "",
                                VendorId = desc.VendorId,
                                DeviceId = desc.DeviceId,
                                DedicatedVideoMemory = desc.DedicatedVideoMemory.ToUInt64(),
                                Luid = $"luid_0x{(uint)desc.AdapterLuid.HighPart:X8}_0x{desc.AdapterLuid.LowPart:X8}"
                            });
                            Debug.WriteLine($"DXGI adapter: {desc.Description} ({result[^1].Luid})");
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(adapter);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DXGI adapter enumeration failed: {ex.Message}");
            }
            finally
            {
                if (factory != null)
                    Marshal.ReleaseComObject(factory);
            }

            return result;
        }
    }
}
