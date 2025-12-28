using System;
using System.Runtime.InteropServices;

namespace Memory_Monitor
{
    /// <summary>
    /// Monitors system RAM usage using Windows API
    /// </summary>
    public class SystemMemoryMonitor : IMonitor
    {
        #region Windows API

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX() { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        #endregion

        private const long BYTES_TO_GB = 1024L * 1024 * 1024;

        public bool IsAvailable => true;

        /// <summary>
        /// Total physical memory in bytes
        /// </summary>
        public ulong TotalMemoryBytes { get; private set; }

        /// <summary>
        /// Available physical memory in bytes
        /// </summary>
        public ulong AvailableMemoryBytes { get; private set; }

        /// <summary>
        /// Used physical memory in bytes
        /// </summary>
        public ulong UsedMemoryBytes => TotalMemoryBytes - AvailableMemoryBytes;

        /// <summary>
        /// Total physical memory in GB
        /// </summary>
        public double TotalMemoryGB => TotalMemoryBytes / (double)BYTES_TO_GB;

        /// <summary>
        /// Used physical memory in GB
        /// </summary>
        public double UsedMemoryGB => UsedMemoryBytes / (double)BYTES_TO_GB;

        /// <summary>
        /// Memory usage percentage (0-100)
        /// </summary>
        public int UsagePercent { get; private set; }

        /// <summary>
        /// Updates memory statistics and returns the usage percentage
        /// </summary>
        public int Update()
        {
            var mem = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(mem))
            {
                TotalMemoryBytes = mem.ullTotalPhys;
                AvailableMemoryBytes = mem.ullAvailPhys;
                UsagePercent = (int)((UsedMemoryBytes * 100) / TotalMemoryBytes);
            }
            return UsagePercent;
        }

        /// <summary>
        /// Gets a formatted display string for memory usage
        /// </summary>
        public string GetDisplayText()
        {
            return $"{UsedMemoryGB:F1}/{TotalMemoryGB:F0}GB";
        }

        public void Dispose()
        {
            // No resources to dispose
        }
    }
}
