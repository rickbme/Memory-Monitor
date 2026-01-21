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
            
            // Show splash screen with DFS logo
            SplashScreen.ShowSplash();

            // Run the application with our custom context that handles display mode switching
            Application.Run(new MonitorApplicationContext());
        }
    }

    /// <summary>
    /// Custom ApplicationContext that manages display mode switching without exiting the application.
    /// </summary>
    internal class MonitorApplicationContext : ApplicationContext
    {
        private Form? _currentForm;
        private bool _isSwitchingMode = false;

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

            try
            {
                // Get current screen position before closing
                var currentScreen = Screen.FromControl(_currentForm);
                var currentLocation = _currentForm.Location;
                var currentSize = _currentForm.Size;
                bool wasTopMost = _currentForm.TopMost;

                // Create the new form
                Form newForm = CreateFormForMode(e.NewMode);
                newForm.FormClosed += OnFormClosed;

                // Position on same monitor
                newForm.StartPosition = FormStartPosition.Manual;
                newForm.Location = currentScreen.Bounds.Location;
                newForm.Size = currentScreen.Bounds.Size;
                newForm.TopMost = wasTopMost;

                // Hide and close the old form
                Form oldForm = _currentForm;
                _currentForm = newForm;

                oldForm.FormClosed -= OnFormClosed;
                oldForm.Hide();
                oldForm.Close();

                // Show the new form
                newForm.Show();
                newForm.Activate();

                Debug.WriteLine($"Switched display mode from {e.OldMode} to {e.NewMode}");
            }
            finally
            {
                _isSwitchingMode = false;
            }
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