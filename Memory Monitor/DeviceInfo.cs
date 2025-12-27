namespace Memory_Monitor
{
    /// <summary>
    /// Represents a selectable hardware device (GPU, Disk, or Network adapter)
    /// </summary>
    public record DeviceInfo
    {
        /// <summary>
        /// Unique identifier for the device (used for selection and persistence)
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Display name shown to the user
        /// </summary>
        public required string DisplayName { get; init; }

        /// <summary>
        /// Short name for compact display (e.g., in gauge labels)
        /// </summary>
        public string ShortName { get; init; } = "";

        /// <summary>
        /// Optional description or additional info about the device
        /// </summary>
        public string Description { get; init; } = "";

        /// <summary>
        /// Device type category
        /// </summary>
        public DeviceType Type { get; init; } = DeviceType.Unknown;

        /// <summary>
        /// Whether this device is currently active/operational
        /// </summary>
        public bool IsActive { get; init; } = true;

        /// <summary>
        /// Optional icon key for UI display
        /// </summary>
        public string IconKey { get; init; } = "";

        /// <summary>
        /// Gets the effective short name (falls back to DisplayName if not set)
        /// </summary>
        public string EffectiveShortName => string.IsNullOrEmpty(ShortName) ? DisplayName : ShortName;
    }

    /// <summary>
    /// Type of hardware device
    /// </summary>
    public enum DeviceType
    {
        Unknown,
        GPU,
        Disk,
        NetworkAdapter,
        Aggregate  // Represents "All Devices" option
    }
}
