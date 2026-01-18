using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Compact circular gauge control designed specifically for FPS display.
    /// Shows just the FPS number prominently with a small "FPS" label below.
    /// </summary>
    public class FpsGaugeControl : Control
    {
        private int _fpsValue = 0;
        private Color _gaugeColor = Color.FromArgb(0, 200, 150);
        private Color _textColor = Color.White;
        private Color _labelColor = Color.FromArgb(180, 180, 180);

        public FpsGaugeControl()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.MinimumSize = new Size(80, 80);
        }

        #region Properties

        public Color GaugeColor
        {
            get => _gaugeColor;
            set { _gaugeColor = value; Invalidate(); }
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

        public int FpsValue => _fpsValue;

        #endregion

        /// <summary>
        /// Sets the FPS value to display
        /// </summary>
        public void SetFps(int fps)
        {
            int newValue = Math.Max(0, fps);
            // Only invalidate if value actually changed (reduces flicker)
            if (_fpsValue != newValue)
            {
                _fpsValue = newValue;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int size = Math.Min(Width, Height);
            int centerX = Width / 2;
            int centerY = Height / 2;
            int gaugeRadius = (int)(size * 0.45f);

            if (gaugeRadius < 20) return;

            DrawGaugeFace(g, centerX, centerY, gaugeRadius);
            DrawFpsRing(g, centerX, centerY, gaugeRadius);
            DrawFpsValue(g, centerX, centerY, gaugeRadius);
            DrawFpsLabel(g, centerX, centerY, gaugeRadius);
        }

        private void DrawGaugeFace(Graphics g, int cx, int cy, int radius)
        {
            // Shadow
            using (var shadowPath = new GraphicsPath())
            {
                int outerR = radius + 6;
                shadowPath.AddEllipse(cx - outerR, cy - outerR, outerR * 2, outerR * 2);
                using (var shadowBrush = new PathGradientBrush(shadowPath))
                {
                    shadowBrush.CenterColor = Color.FromArgb(40, 0, 0, 0);
                    shadowBrush.SurroundColors = new[] { Color.Transparent };
                    g.FillPath(shadowBrush, shadowPath);
                }
            }

            // Main face
            Rectangle faceRect = new Rectangle(cx - radius, cy - radius, radius * 2, radius * 2);
            using (var facePath = new GraphicsPath())
            {
                facePath.AddEllipse(faceRect);
                using (var faceBrush = new PathGradientBrush(facePath))
                {
                    faceBrush.CenterColor = Color.FromArgb(45, 48, 52);
                    faceBrush.SurroundColors = new[] { Color.FromArgb(28, 31, 35) };
                    g.FillEllipse(faceBrush, faceRect);
                }
            }

            // Chrome rim
            using (var rimPen = new Pen(Color.FromArgb(80, 85, 90), 2))
                g.DrawEllipse(rimPen, cx - radius - 2, cy - radius - 2, (radius + 2) * 2, (radius + 2) * 2);
        }

        private void DrawFpsRing(Graphics g, int cx, int cy, int radius)
        {
            // Draw a colored ring around the gauge that pulses based on FPS quality
            int ringRadius = radius - 8;
            Rectangle ringRect = new Rectangle(cx - ringRadius, cy - ringRadius, ringRadius * 2, ringRadius * 2);

            // Color based on FPS value (green = good, yellow = ok, red = bad)
            Color ringColor = GetFpsColor();

            // Glow effect
            for (int i = 3; i > 0; i--)
            {
                using (var glowPen = new Pen(Color.FromArgb(30, ringColor), i * 2))
                {
                    g.DrawEllipse(glowPen, ringRect);
                }
            }

            // Main ring
            using (var ringPen = new Pen(ringColor, 4))
            {
                g.DrawEllipse(ringPen, ringRect);
            }
        }

        private Color GetFpsColor()
        {
            // Color coding based on FPS quality
            if (_fpsValue >= 60)
                return Color.FromArgb(0, 200, 100);  // Green - excellent
            else if (_fpsValue >= 45)
                return Color.FromArgb(150, 200, 50); // Yellow-green - good
            else if (_fpsValue >= 30)
                return Color.FromArgb(255, 180, 0);  // Orange - acceptable
            else
                return Color.FromArgb(255, 80, 80);  // Red - poor
        }

        private void DrawFpsValue(Graphics g, int cx, int cy, int radius)
        {
            // Draw the FPS number prominently in the center
            string fpsText = _fpsValue.ToString();
            
            // Scale font based on radius and number of digits
            // Start with base size proportional to gauge radius
            float baseFontSize = radius * 0.55f;
            
            // Reduce font size for larger numbers to prevent overflow
            if (_fpsValue >= 1000)
                baseFontSize *= 0.50f;  // 4+ digits: 50% of base
            else if (_fpsValue >= 100)
                baseFontSize *= 0.70f;  // 3 digits: 70% of base
            // 1-2 digits use full base size

            float fontSize = Math.Max(10f, baseFontSize);

            using (var font = new Font("Consolas", fontSize, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Offset up slightly to make room for "FPS" label below
                int textY = cy - (int)(radius * 0.1f);

                // Draw glow
                using (var glowBrush = new SolidBrush(Color.FromArgb(60, GetFpsColor())))
                {
                    g.DrawString(fpsText, font, glowBrush, cx + 1, textY + 1, sf);
                }

                // Draw text
                using (var textBrush = new SolidBrush(_textColor))
                {
                    g.DrawString(fpsText, font, textBrush, cx, textY, sf);
                }
            }
        }

        private void DrawFpsLabel(Graphics g, int cx, int cy, int radius)
        {
            // Draw "FPS" label below the number
            float fontSize = Math.Max(8f, radius * 0.2f);
            int labelY = cy + (int)(radius * 0.35f);

            using (var font = new Font("Segoe UI", fontSize, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var brush = new SolidBrush(_labelColor))
            {
                g.DrawString("FPS", font, brush, cx, labelY, sf);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
