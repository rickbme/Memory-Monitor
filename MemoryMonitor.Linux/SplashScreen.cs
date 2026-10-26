using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System.Diagnostics;

namespace Memory_Monitor
{
    /// <summary>
    /// Splash screen displaying the DFS round logo on application startup.
    /// Avalonia port of the Windows SplashScreen form.
    /// </summary>
    public class SplashScreen : Window
    {
        private const int FADE_STEP_MS = 30;
        private const double FADE_INCREMENT = 0.08;
        private const int DISPLAY_DURATION_MS = 1500; // Show for 1.5 seconds

        private readonly DispatcherTimer _fadeTimer = new() { Interval = TimeSpan.FromMilliseconds(FADE_STEP_MS) };
        private readonly DispatcherTimer _displayTimer = new() { Interval = TimeSpan.FromMilliseconds(DISPLAY_DURATION_MS) };
        private bool _fadeIn = true;

        public SplashScreen()
        {
            Width = 400;
            Height = 400;
            CanResize = false;
            SystemDecorations = SystemDecorations.None;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ShowInTaskbar = false;
            Topmost = true;
            Background = new SolidColorBrush(Color.FromRgb(25, 28, 32)); // Match app dark theme
            Opacity = 0;

            Content = LoadLogo();

            _fadeTimer.Tick += FadeTimer_Tick;
            _displayTimer.Tick += DisplayTimer_Tick;

            // Allow click or key press to dismiss
            PointerPressed += (_, _) => CloseSplash();
            KeyDown += (_, _) => CloseSplash();
        }

        private static Control? LoadLogo()
        {
            try
            {
                // Use the round logo for the splash screen
                string logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "dfslogo_round.png");
                if (!File.Exists(logoPath))
                    return null;

                // Scale to fit the splash screen (30px margin) while maintaining aspect ratio
                return new Image
                {
                    Source = new Bitmap(logoPath),
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(30)
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load splash logo: {ex.Message}");
                return null;
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            // Start fade in
            _fadeIn = true;
            _fadeTimer.Start();
        }

        private void FadeTimer_Tick(object? sender, EventArgs e)
        {
            if (_fadeIn)
            {
                Opacity = Math.Min(1.0, Opacity + FADE_INCREMENT);
                if (Opacity >= 1.0)
                {
                    _fadeTimer.Stop();
                    _displayTimer.Start(); // Start display duration timer
                }
            }
            else
            {
                Opacity = Math.Max(0.0, Opacity - FADE_INCREMENT);
                if (Opacity <= 0)
                {
                    CloseSplash();
                }
            }
        }

        private void DisplayTimer_Tick(object? sender, EventArgs e)
        {
            _displayTimer.Stop();

            // Start fade out
            _fadeIn = false;
            _fadeTimer.Start();
        }

        private void CloseSplash()
        {
            _displayTimer.Stop();
            _fadeTimer.Stop();
            Close();
        }
    }
}
