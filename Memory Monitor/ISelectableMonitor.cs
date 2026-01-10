namespace Memory_Monitor
{
    /// <summary>
    /// Interface for monitors that support multiple selectable devices
    /// </summary>
    public interface ISelectableMonitor : IMonitor
    {
        /// <summary>
        /// Gets all available devices that can be monitored
        /// </summary>
        IReadOnlyList<DeviceInfo> AvailableDevices { get; }

        /// <summary>
        /// Gets the currently selected device, or null if using aggregate/all devices
        /// </summary>
        DeviceInfo? SelectedDevice { get; }

        /// <summary>
        /// Gets whether multiple devices are available for selection
        /// </summary>
        bool HasMultipleDevices { get; }

        /// <summary>
        /// Selects a specific device by its ID
        /// </summary>
        /// <param name="deviceId">The unique identifier of the device to select, or null/empty for aggregate mode</param>
        /// <returns>True if the device was successfully selected</returns>
        bool SelectDevice(string? deviceId);

        /// <summary>
        /// Cycles to the next available device in the list.
        /// Used for touch-based navigation to avoid small popup menus.
        /// </summary>
        /// <returns>The newly selected device info, or null if cycling failed</returns>
        DeviceInfo? CycleToNextDevice();

        /// <summary>
        /// Gets the display name for the currently selected device or aggregate mode
        /// </summary>
        string CurrentDeviceDisplayName { get; }
    }
}
