using System.Runtime.InteropServices;

namespace Memory_Monitor
{
    /// <summary>
    /// Factory for creating platform-specific monitor implementations
    /// </summary>
    public static class MonitorFactory
    {
        private static readonly bool _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static IMonitor CreateCPUMonitor()
        {
            if (_isLinux)
                return new Linux.LinuxCPUMonitor();
#if WINDOWS
            else if (_isWindows)
                return new CPUMonitor();
#endif
            else
                throw new PlatformNotSupportedException("CPU monitoring not supported on this platform");
        }

        public static IMonitor CreateMemoryMonitor()
        {
            if (_isLinux)
                return new Linux.LinuxMemoryMonitor();
#if WINDOWS
            else if (_isWindows)
                return new SystemMemoryMonitor();
#endif
            else
                throw new PlatformNotSupportedException("Memory monitoring not supported on this platform");
        }

        public static ISelectableMonitor CreateGPUMonitor()
        {
            if (_isLinux)
                return new Linux.LinuxGPUMonitor();
#if WINDOWS
            else if (_isWindows)
                return new GPUMonitor();
#endif
            else
                throw new PlatformNotSupportedException("GPU monitoring not supported on this platform");
        }

        public static ISelectableMonitor CreateDiskMonitor()
        {
            if (_isLinux)
                return new Linux.LinuxDiskMonitor();
#if WINDOWS
            else if (_isWindows)
                return new DiskMonitor();
#endif
            else
                throw new PlatformNotSupportedException("Disk monitoring not supported on this platform");
        }

        public static ISelectableMonitor CreateNetworkMonitor()
        {
            if (_isLinux)
                return new Linux.LinuxNetworkMonitor();
#if WINDOWS
            else if (_isWindows)
                return new NetworkMonitor();
#endif
            else
                throw new PlatformNotSupportedException("Network monitoring not supported on this platform");
        }

        public static bool IsLinux => _isLinux;
        public static bool IsWindows => _isWindows;
    }
}
