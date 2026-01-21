using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Specialized bar panel control for network monitoring with dual horizontal progress bars.
    /// Shows upload and download speeds with separate bars and labels.
    /// </summary>
    public class NetworkBarPanelControl : Control
    {
        private float _uploadSpeed = 0;
        private float _downloadSpeed = 0;
        private float _maxSpeed = 100;
        private string _uploadText = "0 MB/s";
        private string _downloadText = "0 MB/s";
        private string _title = "NETWORK";
        private Color _uploadColor = Color.FromArgb(0, 200, 255);
        private Color _downloadColor = Color.FromArgb(0, 200, 255);
        private Color _backgroundColor = Color.FromArgb(20, 25, 30);
        private Color _borderColor = Color.FromArgb(60, 70, 80);
        private Color _textColor = Color.White;
        private Color _labelColor = Color.FromArgb(180, 180, 180);
        private Color _accentColor = Color.FromArgb(0, 200, 255);

        public NetworkBarPanelControl()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.MinimumSize = new Size(200, 150);
        }

        #region Properties

        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        public Color UploadColor
        {
            get => _uploadColor;
            set { _uploadColor = value; Invalidate(); }
        }

        public Color DownloadColor
        {
            get => _downloadColor;
            set { _downloadColor = value; Invalidate(); }
        }

        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                _accentColor = value;
                _uploadColor = value;
                _downloadColor = value;
                Invalidate();
            }
        }

        public Color PanelBackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }

        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; Invalidate(); }
        }

        public Color LabelColor
        {
            get => _labelColor;
            set { _labelColor = value; Invalidate(); }
        }

        public float MaxSpeed
        {
            get => _maxSpeed;
            set { _maxSpeed = Math.Max(1, value); Invalidate(); }
        }

        #endregion

        /// <summary>
        /// Updates the upload and download speeds
        /// </summary>
        public void SetSpeeds(float uploadMbps, float downloadMbps, string uploadText, string downloadText)
        {
            _uploadSpeed = Math.Max(0, uploadMbps);
            _downloadSpeed = Math.Max(0, downloadMbps);
            _uploadText = uploadText;
            _downloadText = downloadText;

            // Auto-scale max value
            float maxValue = Math.Max(_uploadSpeed, _downloadSpeed);
            if (maxValue > _maxSpeed * 0.9f)
            {
                _maxSpeed = GetAutoScale(maxValue);
            }

            Invalidate();
        }

        private float GetAutoScale(float value)
        {
            float[] scales = { 10f, 25f, 50f, 100f, 250f, 500f, 1000f, 2500f, 5000f, 10000f };
            foreach (float scale in scales)
            {
                if (value <= scale * 0.95f) return scale;
            }
            return scales[scales.Length - 1];
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawPanel(g);
            DrawTitle(g);
            DrawUploadSection(g);
            DrawDownloadSection(g);
            DrawBorderAccents(g);
        }

        private void DrawPanel(Graphics g)
        {
            Rectangle panelRect = new Rectangle(2, 2, Width - 4, Height - 4);

            // Draw main background with gradient
            using (var bgBrush = new LinearGradientBrush(panelRect,
                Color.FromArgb(30, 35, 42),
                Color.FromArgb(18, 22, 28),
                LinearGradientMode.Vertical))
            {
                using (var path = CreateRoundedRectangle(panelRect, 8))
                {
                    g.FillPath(bgBrush, path);
                }
            }

            // Draw border
            using (var borderPen = new Pen(_borderColor, 2))
            {
                using (var path = CreateRoundedRectangle(panelRect, 8))
                {
                    g.DrawPath(borderPen, path);
                }
            }
        }

        private void DrawTitle(Graphics g)
        {
            float fontSize = Math.Max(10f, Height * 0.08f);
            using (var font = new Font("Segoe UI", fontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(_textColor))
            {
                g.DrawString(_title, font, brush, 15, 10);
            }
        }

        private void DrawUploadSection(Graphics g)
        {
            int barLeft = 15;
            int barRight = Width - 15;
            int barWidth = barRight - barLeft;
            int barHeight = (int)(Height * 0.10f);
            barHeight = Math.Max(14, Math.Min(barHeight, 24));

            // Layout: Label at top, bar below label, speed value below bar
            int labelY = (int)(Height * 0.18f);
            int barY = labelY + (int)(Height * 0.10f);
            int speedY = barY + barHeight + 4;

            float labelFontSize = Math.Max(9f, Height * 0.06f);
            float speedFontSize = Math.Max(14f, Height * 0.12f);

            // Draw "UPLOAD" label on the left
            using (var font = new Font("Segoe UI", labelFontSize))
            using (var brush = new SolidBrush(_labelColor))
            {
                g.DrawString("UPLOAD", font, brush, barLeft, labelY);
            }

            // Draw background bar
            Rectangle bgRect = new Rectangle(barLeft, barY, barWidth, barHeight);
            using (var bgBrush = new SolidBrush(Color.FromArgb(40, 50, 60)))
            {
                using (var path = CreateRoundedRectangle(bgRect, 4))
                {
                    g.FillPath(bgBrush, path);
                }
            }

            // Draw filled bar
            float percentage = _maxSpeed > 0 ? _uploadSpeed / _maxSpeed : 0;
            int filledWidth = (int)(barWidth * Math.Min(1, percentage));
            filledWidth = Math.Max(4, filledWidth);

            if (filledWidth > 0 && percentage > 0.001f)
            {
                Rectangle fillRect = new Rectangle(barLeft, barY, filledWidth, barHeight);

                // Glow effect
                using (var glowBrush = new SolidBrush(Color.FromArgb(30, _uploadColor)))
                {
                    Rectangle glowRect = new Rectangle(barLeft - 2, barY - 2, filledWidth + 4, barHeight + 4);
                    g.FillRectangle(glowBrush, glowRect);
                }

                // Filled bar with gradient
                using (var fillBrush = new LinearGradientBrush(fillRect,
                    Color.FromArgb(255, _uploadColor),
                    Color.FromArgb(200, _uploadColor.R * 2 / 3, _uploadColor.G * 2 / 3, _uploadColor.B * 2 / 3),
                    LinearGradientMode.Vertical))
                {
                    using (var path = CreateRoundedRectangle(fillRect, 4))
                    {
                        g.FillPath(fillBrush, path);
                    }
                }

                // Highlight at top
                if (filledWidth > 6)
                {
                    using (var highlightBrush = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
                    {
                        g.FillRectangle(highlightBrush, barLeft + 2, barY + 1, filledWidth - 4, 2);
                    }
                }

                // Animated segments (tick marks)
                DrawBarSegments(g, barLeft, barY, filledWidth, barHeight, _uploadColor);
            }

            // Draw speed value below the bar, aligned right
            using (var font = new Font("Segoe UI Semibold", speedFontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(_uploadColor))
            {
                var size = g.MeasureString(_uploadText, font);
                g.DrawString(_uploadText, font, brush, barRight - size.Width, speedY);
            }
        }

        private void DrawDownloadSection(Graphics g)
        {
            int barLeft = 15;
            int barRight = Width - 15;
            int barWidth = barRight - barLeft;
            int barHeight = (int)(Height * 0.10f);
            barHeight = Math.Max(14, Math.Min(barHeight, 24));

            // Layout: Label at top, bar below label, speed value below bar
            int labelY = (int)(Height * 0.52f);
            int barY = labelY + (int)(Height * 0.10f);
            int speedY = barY + barHeight + 4;

            float labelFontSize = Math.Max(9f, Height * 0.06f);
            float speedFontSize = Math.Max(14f, Height * 0.12f);

            // Draw "DOWNLOAD" label on the left
            using (var font = new Font("Segoe UI", labelFontSize))
            using (var brush = new SolidBrush(_labelColor))
            {
                g.DrawString("DOWNLOAD", font, brush, barLeft, labelY);
            }

            // Draw background bar
            Rectangle bgRect = new Rectangle(barLeft, barY, barWidth, barHeight);
            using (var bgBrush = new SolidBrush(Color.FromArgb(40, 50, 60)))
            {
                using (var path = CreateRoundedRectangle(bgRect, 4))
                {
                    g.FillPath(bgBrush, path);
                }
            }

            // Draw filled bar
            float percentage = _maxSpeed > 0 ? _downloadSpeed / _maxSpeed : 0;
            int filledWidth = (int)(barWidth * Math.Min(1, percentage));
            filledWidth = Math.Max(4, filledWidth);

            if (filledWidth > 0 && percentage > 0.001f)
            {
                Rectangle fillRect = new Rectangle(barLeft, barY, filledWidth, barHeight);

                // Glow effect
                using (var glowBrush = new SolidBrush(Color.FromArgb(30, _downloadColor)))
                {
                    Rectangle glowRect = new Rectangle(barLeft - 2, barY - 2, filledWidth + 4, barHeight + 4);
                    g.FillRectangle(glowBrush, glowRect);
                }

                // Filled bar with gradient
                using (var fillBrush = new LinearGradientBrush(fillRect,
                    Color.FromArgb(255, _downloadColor),
                    Color.FromArgb(200, _downloadColor.R * 2 / 3, _downloadColor.G * 2 / 3, _downloadColor.B * 2 / 3),
                    LinearGradientMode.Vertical))
                {
                    using (var path = CreateRoundedRectangle(fillRect, 4))
                    {
                        g.FillPath(fillBrush, path);
                    }
                }

                // Highlight at top
                if (filledWidth > 6)
                {
                    using (var highlightBrush = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
                    {
                        g.FillRectangle(highlightBrush, barLeft + 2, barY + 1, filledWidth - 4, 2);
                    }
                }

                // Animated segments
                DrawBarSegments(g, barLeft, barY, filledWidth, barHeight, _downloadColor);
            }

            // Draw speed value below the bar, aligned right
            using (var font = new Font("Segoe UI Semibold", speedFontSize, FontStyle.Bold))
            using (var brush = new SolidBrush(_downloadColor))
            {
                var size = g.MeasureString(_downloadText, font);
                g.DrawString(_downloadText, font, brush, barRight - size.Width, speedY);
            }
        }

        private void DrawBarSegments(Graphics g, int x, int y, int width, int height, Color color)
        {
            // Draw vertical segment lines for a "tech" look
            int segmentWidth = 8;
            int gap = 2;

            using (var segmentPen = new Pen(Color.FromArgb(40, 0, 0, 0), 1))
            {
                for (int sx = x + segmentWidth; sx < x + width - gap; sx += segmentWidth + gap)
                {
                    g.DrawLine(segmentPen, sx, y + 2, sx, y + height - 2);
                }
            }
        }

        private void DrawBorderAccents(Graphics g)
        {
            // Top accent line
            using (var accentPen = new Pen(_accentColor, 2))
            {
                g.DrawLine(accentPen, 8, 3, Width - 8, 3);
            }

            // Corner accents
            int cornerSize = 15;
            using (var cornerPen = new Pen(Color.FromArgb(150, _accentColor), 2))
            {
                // Bottom-left
                g.DrawLine(cornerPen, 3, Height - cornerSize - 3, 3, Height - 3);
                g.DrawLine(cornerPen, 3, Height - 3, cornerSize + 3, Height - 3);

                // Bottom-right
                g.DrawLine(cornerPen, Width - 3, Height - cornerSize - 3, Width - 3, Height - 3);
                g.DrawLine(cornerPen, Width - 3, Height - 3, Width - cornerSize - 3, Height - 3);
            }
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
