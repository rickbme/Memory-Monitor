using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Memory_Monitor
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Exit when the main window closes, not when the splash screen (the first window) does
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

                // Show splash screen with DFS logo, then the main window
                var splash = new SplashScreen();
                splash.Closed += (_, _) =>
                {
                    desktop.MainWindow = new MainWindow();
                    desktop.MainWindow.Show();
                };
                splash.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
