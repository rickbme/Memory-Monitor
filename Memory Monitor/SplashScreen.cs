using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Splash screen displaying the DFS round logo on application startup.
    /// </summary>
    public class SplashScreen : Form
    {
        private readonly System.Windows.Forms.Timer _fadeTimer;
        private readonly System.Windows.Forms.Timer _displayTimer;
        private float _opacity = 0f;
        private bool _fadeIn = true;
        private const int FADE_STEP_MS = 30;
        private const float FADE_INCREMENT = 0.08f;
        private const int DISPLAY_DURATION_MS = 1500; // Show for 1.5 seconds

        public SplashScreen()
        {
            InitializeForm();
            LoadLogo();

            // Timer for fade in/out effect
            _fadeTimer = new System.Windows.Forms.Timer { Interval = FADE_STEP_MS };
            _fadeTimer.Tick += FadeTimer_Tick;

            // Timer for display duration
            _displayTimer = new System.Windows.Forms.Timer { Interval = DISPLAY_DURATION_MS };
            _displayTimer.Tick += DisplayTimer_Tick;
        }

        private void InitializeForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(25, 28, 32); // Match app dark theme
            this.Size = new Size(400, 400);
            this.DoubleBuffered = true;
            this.Opacity = 0;

            // Allow click to dismiss
            this.Click += (s, e) => CloseSplash();
            this.KeyPress += (s, e) => CloseSplash();
        }

        private void LoadLogo()
        {
            try
            {
                // Use the round logo for the splash screen
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "dfslogo_round.png");
                
                if (File.Exists(logoPath))
                {
                    using var image = Image.FromFile(logoPath);
                    
                    // Scale to fit the splash screen while maintaining aspect ratio
                    int maxSize = Math.Min(this.ClientSize.Width, this.ClientSize.Height) - 60;
                    float scale = Math.Min(
                        (float)maxSize / image.Width,
                        (float)maxSize / image.Height
                    );
                    
                    int newWidth = (int)(image.Width * scale);
                    int newHeight = (int)(image.Height * scale);
                    
                    // Create a scaled bitmap
                    var scaledImage = new Bitmap(newWidth, newHeight);
                    using (var g = Graphics.FromImage(scaledImage))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.DrawImage(image, 0, 0, newWidth, newHeight);
                    }

                    // Create picture box to display the logo
                    var pictureBox = new PictureBox
                    {
                        Image = scaledImage,
                        SizeMode = PictureBoxSizeMode.AutoSize,
                        BackColor = Color.Transparent
                    };

                    // Center the picture box
                    pictureBox.Location = new Point(
                        (this.ClientSize.Width - newWidth) / 2,
                        (this.ClientSize.Height - newHeight) / 2
                    );

                    pictureBox.Click += (s, e) => CloseSplash();
                    this.Controls.Add(pictureBox);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load splash logo: {ex.Message}");
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            // Start fade in
            _fadeIn = true;
            _fadeTimer.Start();
        }

        private void FadeTimer_Tick(object? sender, EventArgs e)
        {
            if (_fadeIn)
            {
                _opacity += FADE_INCREMENT;
                if (_opacity >= 1.0f)
                {
                    _opacity = 1.0f;
                    _fadeTimer.Stop();
                    _displayTimer.Start(); // Start display duration timer
                }
            }
            else
            {
                _opacity -= FADE_INCREMENT;
                if (_opacity <= 0f)
                {
                    _opacity = 0f;
                    _fadeTimer.Stop();
                    this.Close();
                    return;
                }
            }

            this.Opacity = _opacity;
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
            this.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _fadeTimer.Dispose();
            _displayTimer.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>
        /// Shows the splash screen and waits for it to complete.
        /// </summary>
        public static void ShowSplash()
        {
            using var splash = new SplashScreen();
            splash.ShowDialog();
        }
    }
}
