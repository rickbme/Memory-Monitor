using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Compact circular gauge control optimized for horizontal mini monitor displays (1920x480)
    /// </summary>
    public class CompactGaugeControl : Control
    {
        private float _currentValue = 0;
        private float _maxValue = 100;
        private string _displayValue = "0";
        private string _secondaryValue = "";  // For temperature display
        private string _label = "";
        private string _deviceName = "";  // Device name shown above label
        private string _unit = "%";
        private Color _gaugeColor = Color.FromArgb(58, 150, 221);
        private Color _needleColor = Color.FromArgb(255, 80, 80);
        private Color _textColor = Color.White;
        private Color _labelColor = Color.White;
        private Color _secondaryColor = Color.FromArgb(255, 100, 100); // Temperature color (warm red)
        
        // Device selection support
        private bool _isSelectable = false;
        private bool _hasMultipleDevices = false;
        private bool _isHovered = false;

        public CompactGaugeControl()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.MinimumSize = new Size(120, 120);
        }

        #region Properties

        public string Label
        {
            get => _label;
            set { _label = value; Invalidate(); }
        }

        /// <summary>
        /// Gets or sets the device name displayed above the label (e.g., "RTX 3060", "C: D:", "WiFi")
        /// </summary>
        public string DeviceName
        {
            get => _deviceName;
            set { _deviceName = value; Invalidate(); }
        }

        public string Unit
        {
            get => _unit;
            set { _unit = value; Invalidate(); }
        }

        public Color GaugeColor
        {
            get => _gaugeColor;
            set { _gaugeColor = value; _needleColor = value; Invalidate(); }
        }

        public Color NeedleColor
        {
            get => _needleColor;
            set { _needleColor = value; Invalidate(); }
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

        public Color SecondaryColor
        {
            get => _secondaryColor;
            set { _secondaryColor = value; Invalidate(); }
        }

        public float MaxValue
        {
            get => _maxValue;
            set { _maxValue = Math.Max(0.001f, value); Invalidate(); } // Prevent zero/negative to avoid division errors
        }

        public float CurrentValue => _currentValue;

        /// <summary>
        /// Gets or sets whether this gauge supports device selection
        /// </summary>
        public bool IsSelectable
        {
            get => _isSelectable;
            set 
            { 
                _isSelectable = value;
                Cursor = (_isSelectable && _hasMultipleDevices) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets whether multiple devices are available for selection
        /// </summary>
        public bool HasMultipleDevices
        {
            get => _hasMultipleDevices;
            set 
            { 
                _hasMultipleDevices = value;
                Cursor = (_isSelectable && _hasMultipleDevices) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event raised when the gauge is clicked for device selection
        /// </summary>
        public event EventHandler? DeviceSelectionRequested;

        protected virtual void OnDeviceSelectionRequested()
        {
            DeviceSelectionRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public void SetValue(float value, string displayText)
        {
            _currentValue = Math.Max(0, Math.Min(value, _maxValue));
            _displayValue = displayText;
            _secondaryValue = "";
            Invalidate();
        }

        /// <summary>
        /// Set value with secondary display (e.g., temperature above usage)
        /// </summary>
        public void SetValue(float value, string displayText, string secondaryText)
        {
            _currentValue = Math.Max(0, Math.Min(value, _maxValue));
            _displayValue = displayText;
            _secondaryValue = secondaryText;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (_isSelectable && _hasMultipleDevices)
            {
                _isHovered = true;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_isHovered)
            {
                _isHovered = false;
                Invalidate();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_isSelectable && _hasMultipleDevices)
            {
                OnDeviceSelectionRequested();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Calculate size to fit gauge, device name, and label
            int labelAreaHeight = string.IsNullOrEmpty(_deviceName) ? 25 : 45; // More space if device name shown
            int availableHeight = Height - labelAreaHeight;
            int size = Math.Min(Width, availableHeight);
            
            // Center the gauge horizontally and vertically within available space
            int centerX = Width / 2;
            int centerY = availableHeight / 2;
            int gaugeRadius = (int)(size * 0.42f);

            if (gaugeRadius < 15) return;

            // Draw hover highlight if selectable
            if (_isHovered && _isSelectable && _hasMultipleDevices)
            {
                DrawHoverHighlight(g, centerX, centerY, gaugeRadius);
            }

            DrawGaugeFace(g, centerX, centerY, gaugeRadius);
            DrawColoredArc(g, centerX, centerY, gaugeRadius);
            DrawTickMarks(g, centerX, centerY, gaugeRadius);
            DrawNeedle(g, centerX, centerY, gaugeRadius);
            DrawCenterHub(g, centerX, centerY);
            
            // Draw secondary value (temperature) if present
            if (!string.IsNullOrEmpty(_secondaryValue))
            {
                DrawSecondaryValue(g, centerX, centerY, gaugeRadius);
            }
            
            DrawDigitalValue(g, centerX, centerY, gaugeRadius);
            DrawLabel(g, centerX, availableHeight);

            // Draw selection indicator if selectable with multiple devices
            if (_isSelectable && _hasMultipleDevices)
            {
                DrawSelectionIndicator(g, centerX, centerY, gaugeRadius);
            }
        }

        private void DrawHoverHighlight(Graphics g, int cx, int cy, int radius)
        {
            int highlightRadius = radius + 15;
            using (var brush = new SolidBrush(Color.FromArgb(20, _gaugeColor)))
            {
                g.FillEllipse(brush, cx - highlightRadius, cy - highlightRadius, 
                              highlightRadius * 2, highlightRadius * 2);
            }
        }

        private void DrawSelectionIndicator(Graphics g, int cx, int cy, int radius)
        {
            // Draw a small dropdown arrow indicator at the bottom of the gauge
            int indicatorY = cy + radius + 5;
            int arrowSize = 6;
            
            Color indicatorColor = _isHovered 
                ? Color.FromArgb(200, _gaugeColor) 
                : Color.FromArgb(100, 150, 150, 150);

            PointF[] arrow = new PointF[]
            {
                new PointF(cx - arrowSize, indicatorY),
                new PointF(cx + arrowSize, indicatorY),
                new PointF(cx, indicatorY + arrowSize)
            };

            using (var brush = new SolidBrush(indicatorColor))
            {
                g.FillPolygon(brush, arrow);
            }
        }

        private void DrawGaugeFace(Graphics g, int cx, int cy, int radius)
        {
            // Shadow
            using (var shadowPath = new GraphicsPath())
            {
                int outerR = radius + 10;
                shadowPath.AddEllipse(cx - outerR, cy - outerR, outerR * 2, outerR * 2);
                using (var shadowBrush = new PathGradientBrush(shadowPath))
                {
                    shadowBrush.CenterColor = Color.FromArgb(50, 0, 0, 0);
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
                    faceBrush.CenterColor = Color.FromArgb(50, 53, 57);
                    faceBrush.SurroundColors = new[] { Color.FromArgb(30, 33, 37) };
                    g.FillEllipse(faceBrush, faceRect);
                }
            }

            // Chrome rim
            using (var rimPen = new Pen(Color.FromArgb(100, 105, 110), 2))
                g.DrawEllipse(rimPen, cx - radius - 4, cy - radius - 4, (radius + 4) * 2, (radius + 4) * 2);
        }

        private void DrawColoredArc(Graphics g, int cx, int cy, int radius)
        {
            float startAngle = 135f;
            float sweepAngle = 270f;
            float percentage = _maxValue > 0 ? _currentValue / _maxValue : 0;
            float currentSweep = sweepAngle * percentage;

            int arcRadius = radius - 15;
            Rectangle arcRect = new Rectangle(cx - arcRadius, cy - arcRadius, arcRadius * 2, arcRadius * 2);

            // Only draw the colored arc if it has a meaningful sweep angle
            // Very small sweep angles cause GDI+ issues with LineCap.Round on thick pens
            if (currentSweep >= 1.0f)
            {
                // Glow effect
                using (var glowPath = new GraphicsPath())
                {
                    glowPath.AddArc(arcRect, startAngle, currentSweep);
                    for (int i = 5; i > 0; i--)
                    {
                        using (var glowPen = new Pen(Color.FromArgb(25, _gaugeColor), i * 2))
                        {
                            glowPen.StartCap = glowPen.EndCap = LineCap.Round;
                            g.DrawPath(glowPen, glowPath);
                        }
                    }

                    using (var arcPen = new Pen(_gaugeColor, 8))
                    {
                        arcPen.StartCap = arcPen.EndCap = LineCap.Round;
                        g.DrawPath(arcPen, glowPath);
                    }
                }
            }

            // Background arc (draw full arc if currentSweep is too small)
            float bgStart = currentSweep >= 1.0f ? startAngle + currentSweep : startAngle;
            float bgSweep = currentSweep >= 1.0f ? sweepAngle - currentSweep : sweepAngle;

            using (var bgPen = new Pen(Color.FromArgb(50, 50, 50), 8))
            {
                bgPen.StartCap = bgPen.EndCap = LineCap.Round;
                g.DrawArc(bgPen, arcRect, bgStart, bgSweep);
            }
        }

        private void DrawTickMarks(Graphics g, int cx, int cy, int radius)
        {
            float startAngle = 135f;
            float sweepAngle = 270f;

            for (int i = 0; i <= 10; i++)
            {
                float angle = startAngle + (sweepAngle * i / 10);
                double rad = angle * Math.PI / 180.0;

                float innerR = radius - 10;
                float outerR = innerR - (i % 2 == 0 ? 8 : 4);
                float tickWidth = i % 2 == 0 ? 2f : 1f;

                int x1 = cx + (int)(innerR * Math.Cos(rad));
                int y1 = cy + (int)(innerR * Math.Sin(rad));
                int x2 = cx + (int)(outerR * Math.Cos(rad));
                int y2 = cy + (int)(outerR * Math.Sin(rad));

                using (var tickPen = new Pen(Color.FromArgb(150, 150, 150), tickWidth))
                {
                    tickPen.StartCap = tickPen.EndCap = LineCap.Round;
                    g.DrawLine(tickPen, x1, y1, x2, y2);
                }
            }
        }

        private void DrawNeedle(Graphics g, int cx, int cy, int radius)
        {
            float startAngle = 135f;
            float sweepAngle = 270f;
            float percentage = _maxValue > 0 ? _currentValue / _maxValue : 0;
            float needleAngle = startAngle + (sweepAngle * percentage);
            double rad = needleAngle * Math.PI / 180.0;

            float needleLength = radius - 22;
            float baseWidth = 5f;

            PointF tip = new PointF(cx + (float)(needleLength * Math.Cos(rad)), cy + (float)(needleLength * Math.Sin(rad)));

            double perpLeft = rad - Math.PI / 2;
            double perpRight = rad + Math.PI / 2;

            PointF baseL = new PointF(cx + (float)(baseWidth * Math.Cos(perpLeft)), cy + (float)(baseWidth * Math.Sin(perpLeft)));
            PointF baseR = new PointF(cx + (float)(baseWidth * Math.Cos(perpRight)), cy + (float)(baseWidth * Math.Sin(perpRight)));

            // Shadow
            PointF[] shadow = { new PointF(baseL.X + 1, baseL.Y + 1), new PointF(tip.X + 1, tip.Y + 1), new PointF(baseR.X + 1, baseR.Y + 1) };
            using (var shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                g.FillPolygon(shadowBrush, shadow);

            // Needle
            PointF[] needle = { baseL, tip, baseR };
            using (var needleBrush = new LinearGradientBrush(new PointF(cx, cy), tip, _needleColor,
                Color.FromArgb(Math.Min(255, _needleColor.R + 50), Math.Min(255, _needleColor.G + 50), Math.Min(255, _needleColor.B + 50))))
            {
                g.FillPolygon(needleBrush, needle);
            }
        }

        private void DrawCenterHub(Graphics g, int cx, int cy)
        {
            int hubR = 8;

            using (var ringBrush = new LinearGradientBrush(new Point(cx, cy - hubR), new Point(cx, cy + hubR),
                Color.FromArgb(80, 85, 90), Color.FromArgb(45, 50, 55)))
            {
                g.FillEllipse(ringBrush, cx - hubR, cy - hubR, hubR * 2, hubR * 2);
            }

            int innerR = 5;
            using (var innerBrush = new SolidBrush(Color.FromArgb(55, 60, 65)))
                g.FillEllipse(innerBrush, cx - innerR, cy - innerR, innerR * 2, innerR * 2);
        }

        private void DrawSecondaryValue(Graphics g, int cx, int cy, int radius)
        {
            // Draw temperature below the center hub, scaled to gauge size
            int tempY = cy + (int)(radius * 0.35f);

            using (var font = new Font("Consolas", Math.Max(8f, radius * 0.12f), FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Draw glow effect
                using (var glowBrush = new SolidBrush(Color.FromArgb(40, _secondaryColor)))
                {
                    g.DrawString(_secondaryValue, font, glowBrush, cx, tempY, sf);
                }
                
                // Draw text
                using (var textBrush = new SolidBrush(_secondaryColor))
                {
                    g.DrawString(_secondaryValue, font, textBrush, cx, tempY, sf);
                }
            }
        }

        private void DrawDigitalValue(Graphics g, int cx, int cy, int radius)
        {
            int displayY = cy + (int)(radius * 0.65f);
            int displayW = (int)(radius * 0.9f);
            int displayH = (int)(radius * 0.18f);
            int displayX = cx - displayW / 2;

            Rectangle displayRect = new Rectangle(displayX, displayY, displayW, displayH);

            // Background
            using (var bgBrush = new LinearGradientBrush(displayRect, Color.FromArgb(20, 25, 30), Color.FromArgb(30, 35, 40), LinearGradientMode.Vertical))
                g.FillRectangle(bgBrush, displayRect);

            using (var borderPen = new Pen(Color.FromArgb(60, 65, 70), 1))
                g.DrawRectangle(borderPen, displayRect);

            // Value text - scale font with gauge size
            float fontSize = Math.Max(8f, radius * 0.1f);
            using (var font = new Font("Consolas", fontSize, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var textBrush = new SolidBrush(_gaugeColor))
            {
                g.DrawString(_displayValue, font, textBrush, cx, displayY + displayH / 2, sf);
            }
        }

        private void DrawLabel(Graphics g, int cx, int gaugeAreaHeight)
        {
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Draw device name above the label (smaller font, gauge color)
                if (!string.IsNullOrEmpty(_deviceName))
                {
                    int deviceNameY = gaugeAreaHeight + 5;
                    using (var deviceFont = new Font("Segoe UI", 10f, FontStyle.Regular))
                    using (var deviceBrush = new SolidBrush(_gaugeColor))
                    {
                        g.DrawString(_deviceName, deviceFont, deviceBrush, cx, deviceNameY, sf);
                    }
                }

                // Draw main label
                if (!string.IsNullOrEmpty(_label))
                {
                    int labelY = string.IsNullOrEmpty(_deviceName) 
                        ? gaugeAreaHeight + 15 
                        : gaugeAreaHeight + 28;

                    using (var font = new Font("Segoe UI", 18f, FontStyle.Bold))
                    using (var brush = new SolidBrush(_labelColor))
                    {
                        g.DrawString(_label, font, brush, cx, labelY, sf);
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
