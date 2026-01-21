using System;
using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// Defines the available display modes for the monitor application
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// Classic circular gauge display with needles
        /// </summary>
        CircularGauges,

        /// <summary>
        /// Modern bar graph display with animated history
        /// </summary>
        BarGraph
    }

    /// <summary>
    /// Manages the display mode state and switching between different display forms.
    /// Persists user preference and provides events for mode changes.
    /// </summary>
    public static class DisplayModeManager
    {
        private static DisplayMode _currentMode = DisplayMode.CircularGauges;
        private static bool _isInitialized = false;

        /// <summary>
        /// Event raised when the display mode is changed
        /// </summary>
        public static event EventHandler<DisplayModeChangedEventArgs>? DisplayModeChanged;

        /// <summary>
        /// Gets the current display mode
        /// </summary>
        public static DisplayMode CurrentMode
        {
            get
            {
                if (!_isInitialized)
                {
                    LoadPreference();
                }
                return _currentMode;
            }
        }

        /// <summary>
        /// Sets the display mode and raises the DisplayModeChanged event
        /// </summary>
        /// <param name="mode">The new display mode</param>
        public static void SetDisplayMode(DisplayMode mode)
        {
            if (_currentMode != mode)
            {
                DisplayMode oldMode = _currentMode;
                _currentMode = mode;
                SavePreference();

                Debug.WriteLine($"Display mode changed: {oldMode} -> {mode}");

                DisplayModeChanged?.Invoke(null, new DisplayModeChangedEventArgs(oldMode, mode));
            }
        }

        /// <summary>
        /// Toggles between available display modes
        /// </summary>
        public static void ToggleDisplayMode()
        {
            DisplayMode newMode = _currentMode == DisplayMode.CircularGauges
                ? DisplayMode.BarGraph
                : DisplayMode.CircularGauges;

            SetDisplayMode(newMode);
        }

        /// <summary>
        /// Loads the display mode preference from settings
        /// </summary>
        private static void LoadPreference()
        {
            try
            {
                string savedMode = Properties.Settings.Default.DisplayMode;
                if (!string.IsNullOrEmpty(savedMode) && Enum.TryParse<DisplayMode>(savedMode, out var mode))
                {
                    _currentMode = mode;
                    Debug.WriteLine($"Loaded display mode preference: {mode}");
                }
                else
                {
                    _currentMode = DisplayMode.CircularGauges; // Default
                    Debug.WriteLine("Using default display mode: CircularGauges");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load display mode preference: {ex.Message}");
                _currentMode = DisplayMode.CircularGauges;
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Saves the display mode preference to settings
        /// </summary>
        private static void SavePreference()
        {
            try
            {
                Properties.Settings.Default.DisplayMode = _currentMode.ToString();
                Properties.Settings.Default.Save();
                Debug.WriteLine($"Saved display mode preference: {_currentMode}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save display mode preference: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the display name for a display mode
        /// </summary>
        public static string GetDisplayName(DisplayMode mode)
        {
            return mode switch
            {
                DisplayMode.CircularGauges => "Circular Gauges",
                DisplayMode.BarGraph => "Bar Graph",
                _ => mode.ToString()
            };
        }
    }

    /// <summary>
    /// Event arguments for display mode changes
    /// </summary>
    public class DisplayModeChangedEventArgs : EventArgs
    {
        public DisplayMode OldMode { get; }
        public DisplayMode NewMode { get; }

        public DisplayModeChangedEventArgs(DisplayMode oldMode, DisplayMode newMode)
        {
            OldMode = oldMode;
            NewMode = newMode;
        }
    }
}
