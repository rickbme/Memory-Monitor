namespace Memory_Monitor
{
    /// <summary>
    /// Manages application themes (Light and Dark mode)
    /// </summary>
    public static class ThemeManager
    {
        public enum Theme
        {
            Light,
            Dark
        }

        public static Theme CurrentTheme { get; private set; } = Theme.Light;

        // Light Theme Colors
        public static class LightTheme
        {
            public static Color FormBackground => Color.FromArgb(240, 240, 240);
            public static Color ControlBackground => Color.White;
            public static Color TextPrimary => Color.FromArgb(30, 30, 30);
            public static Color TextSecondary => Color.FromArgb(100, 100, 100);
            public static Color GraphBackground => Color.FromArgb(245, 245, 245);
            public static Color GraphGrid => Color.FromArgb(200, 200, 200);
            public static Color CPUColor => Color.FromArgb(0, 120, 215);
            public static Color GPUColor => Color.FromArgb(0, 200, 80);
            public static Color DiskColor => Color.FromArgb(255, 140, 0); // Dark Orange
            public static Color NetworkColor => Color.FromArgb(138, 43, 226); // Blue Violet
            public static Color ListViewBackground => Color.White;
            public static Color ListViewText => Color.Black;
            public static Color ListViewGrid => Color.FromArgb(230, 230, 230);
            public static Color MenuBackground => SystemColors.Control;
            public static Color MenuText => SystemColors.ControlText;
        }

        // Dark Theme Colors
        public static class DarkTheme
        {
            public static Color FormBackground => Color.FromArgb(30, 30, 30);
            public static Color ControlBackground => Color.FromArgb(45, 45, 45);
            public static Color TextPrimary => Color.FromArgb(220, 220, 220);
            public static Color TextSecondary => Color.FromArgb(160, 160, 160);
            public static Color GraphBackground => Color.FromArgb(20, 20, 20);
            public static Color GraphGrid => Color.FromArgb(60, 60, 60);
            public static Color CPUColor => Color.FromArgb(58, 150, 221);
            public static Color GPUColor => Color.FromArgb(39, 221, 111);
            public static Color DiskColor => Color.FromArgb(255, 165, 50); // Orange
            public static Color NetworkColor => Color.FromArgb(160, 90, 240); // Purple
            public static Color ListViewBackground => Color.FromArgb(35, 35, 35);
            public static Color ListViewText => Color.FromArgb(220, 220, 220);
            public static Color ListViewGrid => Color.FromArgb(60, 60, 60);
            public static Color MenuBackground => Color.FromArgb(45, 45, 45);
            public static Color MenuText => Color.FromArgb(220, 220, 220);
        }

        /// <summary>
        /// Gets the current theme's colors
        /// </summary>
        public static (Color FormBackground, Color ControlBackground, Color TextPrimary, Color TextSecondary,
                       Color GraphBackground, Color GraphGrid, Color CPUColor, Color GPUColor,
                       Color DiskColor, Color NetworkColor,
                       Color ListViewBackground, Color ListViewText, Color ListViewGrid,
                       Color MenuBackground, Color MenuText) GetCurrentColors()
        {
            if (CurrentTheme == Theme.Dark)
            {
                return (DarkTheme.FormBackground, DarkTheme.ControlBackground, DarkTheme.TextPrimary,
                        DarkTheme.TextSecondary, DarkTheme.GraphBackground, DarkTheme.GraphGrid,
                        DarkTheme.CPUColor, DarkTheme.GPUColor, DarkTheme.DiskColor, DarkTheme.NetworkColor,
                        DarkTheme.ListViewBackground, DarkTheme.ListViewText, DarkTheme.ListViewGrid, 
                        DarkTheme.MenuBackground, DarkTheme.MenuText);
            }
            else
            {
                return (LightTheme.FormBackground, LightTheme.ControlBackground, LightTheme.TextPrimary,
                        LightTheme.TextSecondary, LightTheme.GraphBackground, LightTheme.GraphGrid,
                        LightTheme.CPUColor, LightTheme.GPUColor, LightTheme.DiskColor, LightTheme.NetworkColor,
                        LightTheme.ListViewBackground, LightTheme.ListViewText, LightTheme.ListViewGrid, 
                        LightTheme.MenuBackground, LightTheme.MenuText);
            }
        }

        /// <summary>
        /// Sets the current theme
        /// </summary>
        public static void SetTheme(Theme theme)
        {
            CurrentTheme = theme;
        }

        /// <summary>
        /// Toggles between light and dark theme
        /// </summary>
        public static void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == Theme.Light ? Theme.Dark : Theme.Light;
        }

        /// <summary>
        /// Returns true if dark mode is active
        /// </summary>
        public static bool IsDarkMode()
        {
            return CurrentTheme == Theme.Dark;
        }
    }
}
