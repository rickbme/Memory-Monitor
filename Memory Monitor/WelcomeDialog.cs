using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// First-run welcome dialog that introduces users to Memory Monitor
    /// </summary>
    public partial class WelcomeDialog : Form
    {
        private readonly string _userGuidePath;

        public WelcomeDialog()
        {
            InitializeComponent();
            
            // Find USER_GUIDE.md in application directory
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            _userGuidePath = Path.Combine(appDir, "USER_GUIDE.md");
            
            // Check if user guide exists
            if (!File.Exists(_userGuidePath))
            {
                btnOpenGuide.Enabled = false;
                btnOpenGuide.Text = "Guide Not Found";
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.ClientSize = new Size(600, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Welcome to Memory Monitor!";
            this.BackColor = Color.FromArgb(30, 33, 37);
            this.Font = new Font("Segoe UI", 9F);

            // Title label
            var lblTitle = new Label
            {
                Text = "Welcome to Memory Monitor!",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 180, 255),
                Location = new Point(20, 20),
                Size = new Size(560, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Subtitle label
            var lblSubtitle = new Label
            {
                Text = "Real-time system monitoring for your mini display",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(200, 200, 200),
                Location = new Point(20, 65),
                Size = new Size(560, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblSubtitle);

            // Features panel
            var pnlFeatures = new Panel
            {
                Location = new Point(20, 100),
                Size = new Size(560, 280),
                BackColor = Color.FromArgb(40, 43, 47),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Feature items
            string[] features = new[]
            {
                " Monitor CPU, GPU, RAM, VRAM, Disk, and Network",
                " View CPU and GPU temperatures in real-time",
                " Smart FPS display during gaming sessions",
                " Touch gestures for mini monitor touchscreens",
                " Date and time display in the corners",
                " Device selection for multiple GPUs, disks, or networks",
                " Runs quietly in system tray when minimized"
            };

            int yPos = 10;
            foreach (var feature in features)
            {
                var lblFeature = new Label
                {
                    Text = feature,
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = Color.White,
                    Location = new Point(15, yPos),
                    Size = new Size(530, 30),
                    AutoSize = false
                };
                pnlFeatures.Controls.Add(lblFeature);
                yPos += 35;
            }

            this.Controls.Add(pnlFeatures);

            // Info label
            var lblInfo = new Label
            {
                Text = "Tip: Right-click the system tray icon for quick access to settings.",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(20, 390),
                Size = new Size(560, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblInfo);

            // Open User Guide button
            btnOpenGuide = new Button
            {
                Text = "Open User Guide",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(20, 440),
                Size = new Size(260, 40),
                BackColor = Color.FromArgb(100, 180, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOpenGuide.FlatAppearance.BorderSize = 0;
            btnOpenGuide.Click += BtnOpenGuide_Click;
            this.Controls.Add(btnOpenGuide);

            // Get Started button
            var btnGetStarted = new Button
            {
                Text = "Get Started",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(320, 440),
                Size = new Size(260, 40),
                BackColor = Color.FromArgb(50, 200, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnGetStarted.FlatAppearance.BorderSize = 0;
            btnGetStarted.Click += (s, e) => this.Close();
            this.Controls.Add(btnGetStarted);

            this.ResumeLayout(false);
        }

        private Button btnOpenGuide;

        public bool DontShowAgain => false;

        private void BtnOpenGuide_Click(object? sender, EventArgs e)
        {
            try
            {
                if (File.Exists(_userGuidePath))
                {
                    // Open USER_GUIDE.md in default application
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _userGuidePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(
                        "USER_GUIDE.md was not found in the installation directory.\n\n" +
                        "You can access the guide from the Start Menu:\n" +
                        "Start  Memory Monitor  Memory Monitor User Guide",
                        "User Guide Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open User Guide:\n{ex.Message}\n\n" +
                    "You can access the guide from the Start Menu:\n" +
                    "Start  Memory Monitor  Memory Monitor User Guide",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Shows the welcome dialog if this is the first run
        /// </summary>
        /// <returns>True if this was the first run, false otherwise</returns>
        public static bool ShowIfFirstRun()
        {
            // Skip if already shown this session (e.g., when switching display modes)
            if (Program.WelcomeDialogShown)
            {
                return false;
            }

            // Check if this is the first run
            if (IsFirstRun())
            {
                Program.WelcomeDialogShown = true;
                
                using (var dialog = new WelcomeDialog())
                {
                    dialog.ShowDialog();
                    
                    // Always mark first run as complete after showing the dialog
                    // The "Don't show again" checkbox is no longer needed since we only show once
                    SetFirstRunComplete();
                }
                return true;
            }
            
            // Not first run, but mark as shown for this session anyway
            Program.WelcomeDialogShown = true;
            return false;
        }

        /// <summary>
        /// Checks if this is the first run of the application
        /// </summary>
        private static bool IsFirstRun()
        {
            try
            {
                // Check registry for first run flag
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\MemoryMonitor", false))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("FirstRunComplete");
                        return value == null || value.ToString() != "1";
                    }
                }
                return true; // No registry key = first run
            }
            catch
            {
                return true; // If error, assume first run
            }
        }

        /// <summary>
        /// Marks the first run as complete in the registry
        /// </summary>
        private static void SetFirstRunComplete()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\MemoryMonitor"))
                {
                    key?.SetValue("FirstRunComplete", 1, Microsoft.Win32.RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save first run state: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets the first run flag (useful for testing)
        /// </summary>
        public static void ResetFirstRun()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\MemoryMonitor", true))
                {
                    key?.DeleteValue("FirstRunComplete", false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to reset first run state: {ex.Message}");
            }
        }
    }
}
