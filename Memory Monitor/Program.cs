using System.Diagnostics;

namespace Memory_Monitor
{
    internal static class Program
    {
        /// <summary>
        /// Indicates whether the intro logos have already been shown during this session.
        /// Used to skip logos when switching display modes.
        /// </summary>
        public static bool IntroLogosShown { get; set; } = false;

        /// <summary>
        /// Indicates whether the welcome dialog has already been shown during this session.
        /// Prevents showing the dialog again when switching display modes.
        /// </summary>
        public static bool WelcomeDialogShown { get; set; } = false;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            UpgradeSettingsFromPreviousVersion();

            // Show splash screen with DFS logo
            SplashScreen.ShowSplash();

            // Run the application with our custom context that handles display mode switching
            Application.Run(new MonitorApplicationContext());
        }

        /// <summary>
        /// User settings are stored in a folder per app version, so a new version starts with defaults.
        /// On the first run of each version, copy the settings over from the previous version.
        /// </summary>
        private static void UpgradeSettingsFromPreviousVersion()
        {
            try
            {
                var settings = Properties.Settings.Default;
                if (settings.SettingsUpgradeRequired)
                {
                    settings.Upgrade();
                    settings.SettingsUpgradeRequired = false;
                    settings.Save();
                    Debug.WriteLine("User settings upgraded from the previous version");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to upgrade user settings: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Custom ApplicationContext that manages display mode switching without exiting the application.
    /// </summary>
    internal class MonitorApplicationContext : ApplicationContext
    {
        private Form? _currentForm;
        private bool _isSwitchingMode = false;

        // Fade animation settings
        private const int FADE_DURATION_MS = 150;
        private const int FADE_INTERVAL_MS = 15;
        private const double OPACITY_STEP = (double)FADE_INTERVAL_MS / FADE_DURATION_MS;

        public MonitorApplicationContext()
        {
            // Subscribe to display mode changes
            DisplayModeManager.DisplayModeChanged += OnDisplayModeChanged;

            // Create and show the initial form based on saved preference
            _currentForm = CreateFormForMode(DisplayModeManager.CurrentMode);
            _currentForm.FormClosed += OnFormClosed;
            _currentForm.Show();
        }

        private Form CreateFormForMode(DisplayMode mode)
        {
            return mode switch
            {
                DisplayMode.BarGraph => new BarGraphDisplayForm(),
                _ => new MiniMonitorForm()
            };
        }

        private void OnDisplayModeChanged(object? sender, DisplayModeChangedEventArgs e)
        {
            if (_currentForm == null || _currentForm.IsDisposed)
                return;

            _isSwitchingMode = true;

            // Get current screen position before transitioning
            var currentScreen = Screen.FromControl(_currentForm);
            bool wasTopMost = _currentForm.TopMost;

            // Start the fade transition
            FadeOutAndSwitch(currentScreen, wasTopMost, e);
        }

        private void FadeOutAndSwitch(Screen targetScreen, bool topMost, DisplayModeChangedEventArgs e)
        {
            Form oldForm = _currentForm!;
            var fadeOutTimer = new System.Windows.Forms.Timer { Interval = FADE_INTERVAL_MS };

            fadeOutTimer.Tick += (s, args) =>
            {
                oldForm.Opacity -= OPACITY_STEP;

                if (oldForm.Opacity <= 0)
                {
                    fadeOutTimer.Stop();
                    fadeOutTimer.Dispose();

                    // Close the old form BEFORE creating the new one. Its monitors release process-wide state
                    // when disposed (LibreHardwareMonitor's static OpCode/Mutexes, NVML, ADL), which would
                    // break the new form's monitors if they had already been created.
                    oldForm.FormClosed -= OnFormClosed;
                    oldForm.Hide();
                    oldForm.Close();

                    // Create and show the new form
                    Form newForm = CreateFormForMode(e.NewMode);
                    newForm.FormClosed += OnFormClosed;
                    newForm.StartPosition = FormStartPosition.Manual;
                    newForm.Location = targetScreen.Bounds.Location;
                    newForm.Size = targetScreen.Bounds.Size;
                    newForm.TopMost = topMost;
                    newForm.Opacity = 0;

                    _currentForm = newForm;
                    newForm.Show();

                    // Fade in the new form
                    FadeIn(newForm, e);
                }
            };

            fadeOutTimer.Start();
        }

        private void FadeIn(Form form, DisplayModeChangedEventArgs e)
        {
            var fadeInTimer = new System.Windows.Forms.Timer { Interval = FADE_INTERVAL_MS };

            fadeInTimer.Tick += (s, args) =>
            {
                form.Opacity += OPACITY_STEP;

                if (form.Opacity >= 1)
                {
                    form.Opacity = 1;
                    fadeInTimer.Stop();
                    fadeInTimer.Dispose();

                    form.Activate();
                    _isSwitchingMode = false;

                    Debug.WriteLine($"Switched display mode from {e.OldMode} to {e.NewMode} with fade transition");
                }
            };

            fadeInTimer.Start();
        }

        private void OnFormClosed(object? sender, FormClosedEventArgs e)
        {
            // Only exit the application if we're not switching modes
            if (!_isSwitchingMode)
            {
                DisplayModeManager.DisplayModeChanged -= OnDisplayModeChanged;
                ExitThread();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisplayModeManager.DisplayModeChanged -= OnDisplayModeChanged;
            }
            base.Dispose(disposing);
        }
    }
}