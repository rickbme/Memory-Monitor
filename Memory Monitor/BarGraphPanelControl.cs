using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Bar graph panel control with animated bar history visualization.
    /// Designed to match the futuristic dashboard style with glowing effects.
    /// Used for CPU, GPU, and Drive usage panels.
    /// </summary>
    public class BarGraphPanelControl : Control
    {
        private const int MAX_HISTORY = 12;
        private readonly Queue<float> _valueHistory = new Queue<float>();
        
        private float _currentValue = 0;
        private float _maxValue = 100;
        private string _title = "USAGE";
        private string _primaryText = "0%";
        private string _secondaryText = "";
        private string _tertiaryText = "";
        private Color _accentColor = Color.FromArgb(0, 200, 255);
        private Color _barColor = Color.FromArgb(0, 200, 255);
        private Color _backgroundColor = Color.FromArgb(20, 25, 30);
        private Color _borderColor = Color.FromArgb(60, 70, 80);
        private Color _textColor = Color.White;
        private Color _secondaryTextColor = Color.FromArgb(180, 180, 180);

        public BarGraphPanelControl()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.MinimumSize = new Size(200, 150);

            // Initialize history with zeros
            for (int i = 0; i < MAX_HISTORY; i++)
                _valueHistory.Enqueue(0);
        }

        #region Properties

        public string Title
        {
            get => _title;
            set { _title = value; Invalidate(); }
        }

        public string PrimaryText
        {
            get => _primaryText;
            set { _primaryText = value; Invalidate(); }
        }

        public string SecondaryText
        {
            get => _secondaryText;
            set { _secondaryText = value; Invalidate(); }
        }

        public string TertiaryText
        {
            get => _tertiaryText;
            set { _tertiaryText = value; Invalidate(); }
        }

        public Color AccentColor
        {
            get => _accentColor;
            set
            {
                _accentColor = value;
                _barColor = value;
                Invalidate();
            }
        }

        public Color BarColor
        {
            get => _barColor;
            set { _barColor = value; Invalidate(); }
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

        public Color SecondaryTextColor
        {
            get => _secondaryTextColor;
            set { _secondaryTextColor = value; Invalidate(); }
        }

        public float MaxValue
        {
            get => _maxValue;
            set { _maxValue = Math.Max(1, value); Invalidate(); }
        }

        public float CurrentValue => _currentValue;

        #endregion

        /// <summary>
        /// Updates the value and adds it to the history for bar graph animation
        /// </summary>
        public void SetValue(float value, string primaryText, string secondaryText = "", string tertiaryText = "")
        {
            _currentValue = Math.Max(0, Math.Min(value, _maxValue));
            _primaryText = primaryText;
            _secondaryText = secondaryText;
            _tertiaryText = tertiaryText;

            // Update history
            if (_valueHistory.Count >= MAX_HISTORY)
                _valueHistory.Dequeue();
            _valueHistory.Enqueue(_currentValue);

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawPanel(g);
            DrawTitle(g);
            DrawBarGraph(g);
            DrawPrimaryValue(g);
            DrawSecondaryInfo(g);
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

        private void DrawBarGraph(Graphics g)
        {
            float[] history = _valueHistory.ToArray();
            int barCount = history.Length;
            
            // Calculate bar dimensions
            int graphLeft = 15;
            int graphRight = Width - 15;
            int graphTop = (int)(Height * 0.22f);
            int graphBottom = (int)(Height * 0.50f);
            int graphWidth = graphRight - graphLeft;
            int graphHeight = graphBottom - graphTop;

            int barSpacing = 3;
            int barWidth = (graphWidth - (barSpacing * (barCount - 1))) / barCount;
            barWidth = Math.Max(8, barWidth);

            // Draw bars with glow effect
            for (int i = 0; i < barCount; i++)
            {
                float percentage = _maxValue > 0 ? history[i] / _maxValue : 0;
                int barHeight = (int)(graphHeight * percentage);
                barHeight = Math.Max(2, barHeight);

                int x = graphLeft + i * (barWidth + barSpacing);
                int y = graphBottom - barHeight;

                Rectangle barRect = new Rectangle(x, y, barWidth, barHeight);

                // Draw glow behind bar
                using (var glowBrush = new SolidBrush(Color.FromArgb(40, _barColor)))
                {
                    Rectangle glowRect = new Rectangle(x - 2, y - 2, barWidth + 4, barHeight + 4);
                    g.FillRectangle(glowBrush, glowRect);
                }

                // Draw bar with gradient
                if (barHeight > 4)
                {
                    using (var barBrush = new LinearGradientBrush(
                        new Rectangle(x, y, barWidth, barHeight + 1),
                        Color.FromArgb(255, _barColor),
                        Color.FromArgb(180, _barColor.R / 2, _barColor.G / 2, _barColor.B / 2),
                        LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(barBrush, barRect);
                    }
                }
                else
                {
                    using (var barBrush = new SolidBrush(_barColor))
                    {
                        g.FillRectangle(barBrush, barRect);
                    }
                }

                // Draw highlight at top of bar
                if (barHeight > 6)
                {
                    using (var highlightBrush = new SolidBrush(Color.FromArgb(100, 255, 255, 255)))
                    {
                        g.FillRectangle(highlightBrush, x, y, barWidth, 2);
                    }
                }
            }

            // Draw baseline glow
            using (var baselinePen = new Pen(Color.FromArgb(60, _accentColor), 1))
            {
                g.DrawLine(baselinePen, graphLeft, graphBottom + 2, graphRight, graphBottom + 2);
            }
        }

        private void DrawPrimaryValue(Graphics g)
        {
            // Large percentage display
            float fontSize = Math.Max(24f, Height * 0.20f);
            int valueY = (int)(Height * 0.52f);

            using (var font = new Font("Segoe UI Semibold", fontSize, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Near })
            {
                // Draw glow
                using (var glowBrush = new SolidBrush(Color.FromArgb(50, _accentColor)))
                {
                    g.DrawString(_primaryText, font, glowBrush, 16, valueY, sf);
                }

                // Draw text
                using (var textBrush = new SolidBrush(_accentColor))
                {
                    g.DrawString(_primaryText, font, textBrush, 15, valueY, sf);
                }
            }
        }

        private void DrawSecondaryInfo(Graphics g)
        {
            if (string.IsNullOrEmpty(_secondaryText) && string.IsNullOrEmpty(_tertiaryText))
                return;

            float fontSize = Math.Max(9f, Height * 0.07f);
            int infoY = (int)(Height * 0.82f);

            using (var font = new Font("Segoe UI", fontSize))
            using (var brush = new SolidBrush(_secondaryTextColor))
            {
                if (!string.IsNullOrEmpty(_secondaryText))
                {
                    g.DrawString(_secondaryText, font, brush, 15, infoY);
                }

                if (!string.IsNullOrEmpty(_tertiaryText))
                {
                    // Draw tertiary info on the right or below
                    var size = g.MeasureString(_tertiaryText, font);
                    g.DrawString(_tertiaryText, font, brush, Width - size.Width - 15, infoY);
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

            // Corner accents (small L-shapes in corners)
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
