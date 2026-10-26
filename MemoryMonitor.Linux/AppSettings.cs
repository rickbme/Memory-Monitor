using System.Diagnostics;
using System.Text.Json;

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
    /// User preferences, saved as JSON in ~/.config/memory-monitor/settings.json.
    /// Linux counterpart of the Windows Properties.Settings.
    /// </summary>
    public class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "memory-monitor", "settings.json");

        public DisplayMode DisplayMode { get; set; } = DisplayMode.CircularGauges;
        public bool TopMost { get; set; } = true;

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath)) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }
}
