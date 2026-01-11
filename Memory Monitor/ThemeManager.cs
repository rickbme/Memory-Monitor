namespace Memory_Monitor
{
    public record ThemeColors(
        Color FormBackground,
        Color ControlBackground,
        Color TextPrimary,
        Color TextSecondary,
        Color GraphBackground,
        Color GraphGrid,
        Color CPUColor,
        Color GPUColor,
        Color DiskColor,
        Color NetworkColor,
        Color ListViewBackground,
        Color ListViewText,
        Color ListViewGrid,
        Color MenuBackground,
        Color MenuText
    );

    public static class ThemeManager
    {
        public enum Theme
        {
            Light,
            Dark
        }

        public static Theme CurrentTheme { get; private set; } = Theme.Light;

        private static readonly ThemeColors LightColors = new(
            FormBackground: Color.FromArgb(240, 240, 240),
            ControlBackground: Color.White,
            TextPrimary: Color.FromArgb(30, 30, 30),
            TextSecondary: Color.FromArgb(100, 100, 100),
            GraphBackground: Color.FromArgb(245, 245, 245),
            GraphGrid: Color.FromArgb(200, 200, 200),
            CPUColor: Color.FromArgb(0, 120, 215),
            GPUColor: Color.FromArgb(0, 200, 80),
            DiskColor: Color.FromArgb(255, 140, 0),
            NetworkColor: Color.FromArgb(138, 43, 226),
            ListViewBackground: Color.White,
            ListViewText: Color.Black,
            ListViewGrid: Color.FromArgb(230, 230, 230),
            MenuBackground: SystemColors.Control,
            MenuText: SystemColors.ControlText
        );

        private static readonly ThemeColors DarkColors = new(
            FormBackground: Color.FromArgb(30, 30, 30),
            ControlBackground: Color.FromArgb(45, 45, 45),
            TextPrimary: Color.FromArgb(220, 220, 220),
            TextSecondary: Color.FromArgb(160, 160, 160),
            GraphBackground: Color.FromArgb(20, 20, 20),
            GraphGrid: Color.FromArgb(60, 60, 60),
            CPUColor: Color.FromArgb(58, 150, 221),
            GPUColor: Color.FromArgb(39, 221, 111),
            DiskColor: Color.FromArgb(255, 165, 50),
            NetworkColor: Color.FromArgb(160, 90, 240),
            ListViewBackground: Color.FromArgb(35, 35, 35),
            ListViewText: Color.FromArgb(220, 220, 220),
            ListViewGrid: Color.FromArgb(60, 60, 60),
            MenuBackground: Color.FromArgb(45, 45, 45),
            MenuText: Color.FromArgb(220, 220, 220)
        );

        public static ThemeColors GetCurrentColors()
        {
            return CurrentTheme == Theme.Dark ? DarkColors : LightColors;
        }

        public static void SetTheme(Theme theme)
        {
            CurrentTheme = theme;
        }

        public static void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == Theme.Light ? Theme.Dark : Theme.Light;
        }

        public static bool IsDarkMode()
        {
            return CurrentTheme == Theme.Dark;
        }
    }
}
