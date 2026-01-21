using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Partial class containing UI layout, theming, and tray icon management
    /// </summary>
    public partial class MiniMonitorForm
    {
        // Intro logo display
        private PictureBox? _introLogo;
        private System.Windows.Forms.Timer? _introTimer;
        private const int INTRO_DISPLAY_MS = 2000; // Show logo for 2 seconds

        private void InitializeUI()
        {
            // Initially hide all gauges for intro (only if logos haven't been shown yet)
            if (!Program.IntroLogosShown)
            {
                SetGaugesVisible(false);
            }

            ramGauge.MaxValue = 100f;
            cpuGauge.MaxValue = 100f;
            diskGauge.MaxValue = 100f;
            networkGauge.MaxValue = 100f;

            if (_gpuMonitor != null && _gpuMonitor.IsMemoryAvailable)
                gpuVramGauge.MaxValue = (float)Math.Ceiling(_gpuMonitor.TotalMemoryGB);
            else
                gpuVramGauge.MaxValue = 24f;

            if (_gpuMonitor != null && _gpuMonitor.IsUsageAvailable)
                gpuUsageGauge.MaxValue = 100f;
            else
                gpuUsageGauge.MaxValue = 100f;

            UpdateGaugeDeviceNames();

            // Show intro logo first, then start the gauges (only on first load)
            if (!Program.IntroLogosShown)
            {
                ShowIntroLogo();
            }
            else
            {
                // Skip intro, go straight to gauges
                SetGaugesVisible(true);
                LayoutGauges();
                updateTimer.Start();
            }
        }

        private void SetGaugesVisible(bool visible)
        {
            ramGauge.Visible = visible;
            cpuGauge.Visible = visible;
            gpuUsageGauge.Visible = visible;
            gpuVramGauge.Visible = visible;
            diskGauge.Visible = visible;
            networkGauge.Visible = visible;
            // FPS gauge visibility is controlled by UpdateFps based on game detection
            lblDate.Visible = visible;
            lblTime.Visible = visible;
        }

        private void ShowIntroLogo()
        {
            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "dfs_logo_1920.png");

                if (File.Exists(logoPath))
                {
                    using var image = Image.FromFile(logoPath);

                    // Scale to fit the form while maintaining aspect ratio
                    float scaleX = (float)(this.ClientSize.Width - 40) / image.Width;
                    float scaleY = (float)(this.ClientSize.Height - 40) / image.Height;
                    float scale = Math.Min(scaleX, scaleY);

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

                    _introLogo = new PictureBox
                    {
                        Image = scaledImage,
                        SizeMode = PictureBoxSizeMode.AutoSize,
                        BackColor = Color.Transparent,
                        Location = new Point(
                            (this.ClientSize.Width - newWidth) / 2,
                            (this.ClientSize.Height - newHeight) / 2
                        )
                    };

                    this.Controls.Add(_introLogo);
                    _introLogo.BringToFront();

                    Debug.WriteLine("Intro logo displayed");
                }

                // Start timer to transition to gauges
                _introTimer = new System.Windows.Forms.Timer { Interval = INTRO_DISPLAY_MS };
                _introTimer.Tick += IntroTimer_Tick;
                _introTimer.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to show intro logo: {ex.Message}");
                // If logo fails, just show gauges immediately
                TransitionToGauges();
            }
        }

        private void IntroTimer_Tick(object? sender, EventArgs e)
        {
            _introTimer?.Stop();
            _introTimer?.Dispose();
            _introTimer = null;

            TransitionToGauges();
        }

        private void TransitionToGauges()
        {
            // Remove intro logo
            if (_introLogo != null)
            {
                this.Controls.Remove(_introLogo);
                _introLogo.Image?.Dispose();
                _introLogo.Dispose();
                _introLogo = null;
            }

            // Mark that intro logos have been shown for this session
            Program.IntroLogosShown = true;

            // Show gauges and start updates
            SetGaugesVisible(true);
            LayoutGauges();
            updateTimer.Start();
            UpdateAllMetrics();

            Debug.WriteLine("Transitioned to gauge display");
        }

        private void InitializeApplicationIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "mmguage.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
                else
                {
                    this.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                this.Icon = SystemIcons.Application;
            }
        }

        private void InitializeTrayIcon()
        {
            notifyIcon.Icon = this.Icon ?? SystemIcons.Application;
            UpdateTrayIconText();
            this.Resize += (s, e) =>
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    Hide();
                    ShowInTaskbar = false;
                }
            };
        }

        private void UpdateTrayIcon(int memoryPercentage)
        {
            if (memoryPercentage == _lastMemoryPercentage && _currentTrayIcon != null)
                return;

            _lastMemoryPercentage = memoryPercentage;
            var oldIcon = _currentTrayIcon;

            try
            {
                Color gaugeColor = GaugeIconGenerator.GetColorForPercentage(memoryPercentage);
                _currentTrayIcon = GaugeIconGenerator.CreateDynamicTrayIcon(memoryPercentage, gaugeColor);
                notifyIcon.Icon = _currentTrayIcon;
            }
            catch
            {
                // Keep current icon if creation fails
            }

            oldIcon?.Dispose();
        }

        private void UpdateTrayIconText()
        {
            string text = $"CPU: {_lastCpuUsage}% GPU: {_lastGpuUsage}%";
            notifyIcon.Text = text.Length > 63 ? text.Substring(0, 63) : text;
        }

        private void ApplyTheme()
        {
            this.BackColor = Color.FromArgb(25, 28, 32);

            Color labelColor = Color.FromArgb(200, 200, 200);
            Color textColor = Color.White;

            ramGauge.GaugeColor = Color.FromArgb(58, 150, 221);
            ramGauge.LabelColor = labelColor;
            ramGauge.TextColor = textColor;

            cpuGauge.GaugeColor = Color.FromArgb(220, 50, 50);
            cpuGauge.LabelColor = labelColor;
            cpuGauge.TextColor = textColor;

            gpuUsageGauge.GaugeColor = Color.FromArgb(255, 140, 0);
            gpuUsageGauge.LabelColor = labelColor;
            gpuUsageGauge.TextColor = textColor;

            gpuVramGauge.GaugeColor = Color.FromArgb(160, 90, 240);
            gpuVramGauge.LabelColor = labelColor;
            gpuVramGauge.TextColor = textColor;

            diskGauge.GaugeColor = Color.FromArgb(50, 200, 80);
            diskGauge.LabelColor = labelColor;
            diskGauge.TextColor = textColor;

            networkGauge.GaugeColor = Color.FromArgb(255, 200, 50);
            networkGauge.LabelColor = labelColor;
            networkGauge.TextColor = textColor;
        }

        private void MiniMonitorForm_Resize(object? sender, EventArgs e)
        {
            LayoutGauges();
        }

        private void LayoutGauges()
        {
            int formWidth = ClientSize.Width;
            int formHeight = ClientSize.Height;

            int gaugeCount = 6;
            int marginX = 10;
            int spacing = 5;

            int availableWidth = formWidth - (marginX * 2) - (spacing * (gaugeCount - 1));
            int gaugeWidth = availableWidth / gaugeCount;
            int gaugeHeight = (int)(formHeight * 0.95f);

            int startY = (formHeight - gaugeHeight) / 2;
            int startX = marginX;

            ramGauge.Location = new Point(startX, startY);
            ramGauge.Size = new Size(gaugeWidth, gaugeHeight);

            cpuGauge.Location = new Point(startX + gaugeWidth + spacing, startY);
            cpuGauge.Size = new Size(gaugeWidth, gaugeHeight);

            gpuUsageGauge.Location = new Point(startX + (gaugeWidth + spacing) * 2, startY);
            gpuUsageGauge.Size = new Size(gaugeWidth, gaugeHeight);

            gpuVramGauge.Location = new Point(startX + (gaugeWidth + spacing) * 3, startY);
            gpuVramGauge.Size = new Size(gaugeWidth, gaugeHeight);

            diskGauge.Location = new Point(startX + (gaugeWidth + spacing) * 4, startY);
            diskGauge.Size = new Size(gaugeWidth, gaugeHeight);

            networkGauge.Location = new Point(startX + (gaugeWidth + spacing) * 5, startY);
            networkGauge.Size = new Size(gaugeWidth, gaugeHeight);

            // Position FPS gauge between GPU and VRAM gauges, at the bottom near the labels
            int gpuGaugeRight = gpuUsageGauge.Right;
            int vramGaugeLeft = gpuVramGauge.Left;
            int fpsCenterX = (gpuGaugeRight + vramGaugeLeft) / 2;
            
            // Size: 30% of the main gauge height
            int fpsGaugeSize = (int)(gaugeHeight * 0.30f);
            fpsGaugeSize = Math.Max(80, fpsGaugeSize); // Minimum size of 80px
            
            // Position at the bottom, adjusted for perfect placement
            int fpsY = startY + gaugeHeight - fpsGaugeSize - 10;
            
            fpsGauge.Location = new Point(fpsCenterX - fpsGaugeSize / 2, fpsY);
            fpsGauge.Size = new Size(fpsGaugeSize, fpsGaugeSize);
            fpsGauge.BringToFront();

            // Layout date/time labels
            LayoutDateTimeLabels();
        }

        /// <summary>
        /// Shows a brief toast-style notification on the form
        /// </summary>
        private void ShowToastNotification(string message)
        {
            var toast = new Label
            {
                Text = message,
                AutoSize = false,
                Size = new Size(300, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(200, 40, 44, 52),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                BorderStyle = BorderStyle.None
            };

            toast.Location = new Point(
                (this.ClientSize.Width - toast.Width) / 2,
                (this.ClientSize.Height - toast.Height) / 2
            );

            this.Controls.Add(toast);
            toast.BringToFront();

            var fadeTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            fadeTimer.Tick += (s, args) =>
            {
                fadeTimer.Stop();
                fadeTimer.Dispose();
                if (this.Controls.Contains(toast))
                {
                    this.Controls.Remove(toast);
                    toast.Dispose();
                }
            };
            fadeTimer.Start();
        }
    }
}
