using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Enhanced circular gauge control with digital display and scale markings
    /// </summary>
    public class EnhancedGaugeControl : Control
    {
        private float _currentValue = 0;
        private float _maxValue = 100;
        private string _displayValue = "0";
        private string _title = "";
        private string _unit = "%";
        private Color _gaugeColor = Color.FromArgb(58, 150, 221); // Blue
        private Color _backgroundColor = Color.FromArgb(50, 55, 60); // Dark metallic
        private Color _needleColor = Color.FromArgb(255, 80, 80);
        private Color _textColor = Color.White;
        private Color _scaleTextColor = Color.FromArgb(180, 180, 180);

        public EnhancedGaugeControl()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | 
                          ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.MinimumSize = new Size(180, 180);
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                Invalidate();
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                Invalidate();
            }
        }

        public Color GaugeColor
        {
            get => _gaugeColor;
            set
            {
                _gaugeColor = value;
                Invalidate();
            }
        }

        public Color GaugeBackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                Invalidate();
            }
        }

        public Color NeedleColor
        {
            get => _needleColor;
            set
            {
                _needleColor = value;
                Invalidate();
            }
        }

        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                Invalidate();
            }
        }

        public Color ScaleTextColor
        {
            get => _scaleTextColor;
            set
            {
                _scaleTextColor = value;
                Invalidate();
            }
        }

        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = Math.Max(1, value);
                Invalidate();
            }
        }

        public void SetValue(float value, string displayText)
        {
            _currentValue = Math.Max(0, Math.Min(value, _maxValue));
            _displayValue = displayText;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Calculate dimensions - center the gauge in the control
            int size = Math.Min(Width, Height);
            int centerX = Width / 2;
            int centerY = Height / 2;
            int gaugeRadius = (int)(size * 0.42f);

            if (gaugeRadius < 20)
                return;

            // Draw gauge components only (no background panel)
            DrawGaugeBackground(g, centerX, centerY, gaugeRadius);
            DrawColoredArc(g, centerX, centerY, gaugeRadius);
            DrawTickMarks(g, centerX, centerY, gaugeRadius);
            DrawScaleNumbers(g, centerX, centerY, gaugeRadius);
            DrawNeedle(g, centerX, centerY, gaugeRadius);
            DrawCenterHub(g, centerX, centerY, gaugeRadius);
            
            // Draw digital display at bottom of gauge
            DrawDigitalDisplay(g, centerX, centerY, gaugeRadius);
        }

        private void DrawGaugeBackground(Graphics g, int centerX, int centerY, int radius)
        {
            int outerRadius = radius + 15;
            
            // Outer shadow
            using (GraphicsPath shadowPath = new GraphicsPath())
            {
                shadowPath.AddEllipse(centerX - outerRadius, centerY - outerRadius, 
                                     outerRadius * 2, outerRadius * 2);
                using (PathGradientBrush shadowBrush = new PathGradientBrush(shadowPath))
                {
                    shadowBrush.CenterColor = Color.FromArgb(40, 0, 0, 0);
                    shadowBrush.SurroundColors = new[] { Color.Transparent };
                    g.FillPath(shadowBrush, shadowPath);
                }
            }

            // Main gauge face
            using (GraphicsPath facePath = new GraphicsPath())
            {
                Rectangle faceRect = new Rectangle(centerX - radius, centerY - radius, 
                                                   radius * 2, radius * 2);
                facePath.AddEllipse(faceRect);
                
                using (PathGradientBrush faceBrush = new PathGradientBrush(facePath))
                {
                    faceBrush.CenterColor = Color.FromArgb(45, 48, 52);
                    faceBrush.SurroundColors = new[] { Color.FromArgb(30, 33, 37) };
                    g.FillEllipse(faceBrush, faceRect);
                }
            }

            // Chrome rim
            using (Pen rimPen = new Pen(Color.FromArgb(120, 125, 130), 3))
            {
                g.DrawEllipse(rimPen, centerX - radius - 8, centerY - radius - 8, 
                             (radius + 8) * 2, (radius + 8) * 2);
            }
            using (Pen innerRimPen = new Pen(Color.FromArgb(80, 85, 90), 2))
            {
                g.DrawEllipse(innerRimPen, centerX - radius - 5, centerY - radius - 5, 
                             (radius + 5) * 2, (radius + 5) * 2);
            }
        }

        private void DrawColoredArc(Graphics g, int centerX, int centerY, int radius)
        {
            float startAngle = 135f;
            float sweepAngle = 270f;
            float percentage = _maxValue > 0 ? _currentValue / _maxValue : 0;
            float currentSweep = sweepAngle * percentage;

            int arcRadius = radius - 25;
            Rectangle arcRect = new Rectangle(centerX - arcRadius, centerY - arcRadius, 
                                             arcRadius * 2, arcRadius * 2);

            // Draw colored arc with glow effect
            using (GraphicsPath glowPath = new GraphicsPath())
            {
                glowPath.AddArc(arcRect, startAngle, currentSweep);
                
                // Outer glow
                for (int i = 8; i > 0; i--)
                {
                    using (Pen glowPen = new Pen(Color.FromArgb(20, _gaugeColor), i * 2))
                    {
                        glowPen.StartCap = LineCap.Round;
                        glowPen.EndCap = LineCap.Round;
                        g.DrawPath(glowPen, glowPath);
                    }
                }

                // Main colored arc
                using (Pen arcPen = new Pen(_gaugeColor, 12))
                {
                    arcPen.StartCap = LineCap.Round;
                    arcPen.EndCap = LineCap.Round;
                    g.DrawPath(arcPen, glowPath);
                }
            }

            // Draw dim background arc
            using (Pen bgArcPen = new Pen(Color.FromArgb(60, 60, 60), 12))
            {
                bgArcPen.StartCap = LineCap.Round;
                bgArcPen.EndCap = LineCap.Round;
                g.DrawArc(bgArcPen, arcRect, startAngle + currentSweep, sweepAngle - currentSweep);
            }
        }

        private void DrawTickMarks(Graphics g, int centerX, int centerY, int radius)
        {
            float startAngle = 135f;
            float sweepAngle = 270f;
            int majorTicks = 11;
            int minorTicksPerMajor = 5;

            float angleStep = sweepAngle / (majorTicks - 1);
            float minorAngleStep = angleStep / minorTicksPerMajor;

            for (int i = 0; i < majorTicks; i++)
            {
                float angle = startAngle + (i * angleStep);
                DrawTick(g, centerX, centerY, radius, angle, true);

                if (i < majorTicks - 1)
                {
                    for (int j = 1; j < minorTicksPerMajor; j++)
                    {
                        float minorAngle = angle + (j * minorAngleStep);
                        DrawTick(g, centerX, centerY, radius, minorAngle, false);
                    }
                }
            }
        }

        private void DrawTick(Graphics g, int centerX, int centerY, int radius, float angle, bool isMajor)
        {
            double angleRad = angle * Math.PI / 180.0;
            
            float tickLength = isMajor ? 12f : 6f;
            float tickWidth = isMajor ? 2.5f : 1.5f;
            float innerRadius = radius - 20;
            float outerRadius = innerRadius - tickLength;

            int x1 = centerX + (int)(innerRadius * Math.Cos(angleRad));
            int y1 = centerY + (int)(innerRadius * Math.Sin(angleRad));
            int x2 = centerX + (int)(outerRadius * Math.Cos(angleRad));
            int y2 = centerY + (int)(outerRadius * Math.Sin(angleRad));

            using (Pen tickPen = new Pen(Color.FromArgb(180, 180, 180), tickWidth))
            {
                tickPen.StartCap = LineCap.Round;
                tickPen.EndCap = LineCap.Round;
                g.DrawLine(tickPen, x1, y1, x2, y2);
            }
        }

        private void DrawScaleNumbers(Graphics g, int centerX, int centerY, int radius)
        {
            float startAngle = 135f;
            float sweepAngle = 270f;
            int numLabels = 9; // Show fewer labels for clarity

            using (Font labelFont = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                for (int i = 0; i <= numLabels; i++)
                {
                    float angle = startAngle + (sweepAngle * i / numLabels);
                    double angleRad = angle * Math.PI / 180.0;
                    
                    float textRadius = radius - 45;
                    int x = centerX + (int)(textRadius * Math.Cos(angleRad));
                    int y = centerY + (int)(textRadius * Math.Sin(angleRad));

                    float labelValue = (_maxValue * i / numLabels);
                    string label = labelValue >= 1000 ? $"{labelValue / 1000:F0}k" : $"{labelValue:F0}";

                    // Draw text shadow
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                    {
                        g.DrawString(label, labelFont, shadowBrush, x + 1, y + 1, sf);
                    }
                    
                    // Draw text
                    using (SolidBrush textBrush = new SolidBrush(_scaleTextColor))
                    {
                        g.DrawString(label, labelFont, textBrush, x, y, sf);
                    }
                }
            }
        }

        private void DrawNeedle(Graphics g, int centerX, int centerY, int radius)
        {
            float startAngle = 135f;
            float sweepAngle = 270f;
            float percentage = _maxValue > 0 ? _currentValue / _maxValue : 0;
            float needleAngle = startAngle + (sweepAngle * percentage);
            double angleRad = needleAngle * Math.PI / 180.0;

            float needleLength = radius - 35;
            float needleBaseWidth = 8f;

            PointF tipPoint = new PointF(
                centerX + (float)(needleLength * Math.Cos(angleRad)),
                centerY + (float)(needleLength * Math.Sin(angleRad))
            );

            double perpAngleLeft = angleRad - Math.PI / 2;
            double perpAngleRight = angleRad + Math.PI / 2;

            PointF baseLeft = new PointF(
                centerX + (float)(needleBaseWidth * Math.Cos(perpAngleLeft)),
                centerY + (float)(needleBaseWidth * Math.Sin(perpAngleLeft))
            );

            PointF baseRight = new PointF(
                centerX + (float)(needleBaseWidth * Math.Cos(perpAngleRight)),
                centerY + (float)(needleBaseWidth * Math.Sin(perpAngleRight))
            );

            // Draw needle shadow
            PointF[] shadowPoints = {
                new PointF(baseLeft.X + 2, baseLeft.Y + 2),
                new PointF(tipPoint.X + 2, tipPoint.Y + 2),
                new PointF(baseRight.X + 2, baseRight.Y + 2)
            };
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
            {
                g.FillPolygon(shadowBrush, shadowPoints);
            }

            // Draw needle
            PointF[] needlePoints = { baseLeft, tipPoint, baseRight };
            using (LinearGradientBrush needleBrush = new LinearGradientBrush(
                new PointF(centerX, centerY), tipPoint,
                _needleColor,
                Color.FromArgb(Math.Min(255, _needleColor.R + 40),
                              Math.Min(255, _needleColor.G + 40),
                              Math.Min(255, _needleColor.B + 40))))
            {
                g.FillPolygon(needleBrush, needlePoints);
            }

            // Needle outline
            using (Pen outlinePen = new Pen(Color.FromArgb(150, 0, 0, 0), 1.5f))
            {
                g.DrawPolygon(outlinePen, needlePoints);
            }
        }

        private void DrawCenterHub(Graphics g, int centerX, int centerY, int radius)
        {
            int hubRadius = 14;

            // Outer shadow
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
            {
                g.FillEllipse(shadowBrush, centerX - hubRadius - 2, centerY - hubRadius - 2, 
                             (hubRadius + 2) * 2, (hubRadius + 2) * 2);
            }

            // Outer ring
            using (LinearGradientBrush ringBrush = new LinearGradientBrush(
                new Point(centerX, centerY - hubRadius),
                new Point(centerX, centerY + hubRadius),
                Color.FromArgb(90, 95, 100),
                Color.FromArgb(50, 55, 60)))
            {
                g.FillEllipse(ringBrush, centerX - hubRadius, centerY - hubRadius, 
                             hubRadius * 2, hubRadius * 2);
            }

            // Inner hub
            int innerRadius = 9;
            using (GraphicsPath hubPath = new GraphicsPath())
            {
                Rectangle hubRect = new Rectangle(centerX - innerRadius, centerY - innerRadius, 
                                                  innerRadius * 2, innerRadius * 2);
                hubPath.AddEllipse(hubRect);
                
                using (PathGradientBrush hubBrush = new PathGradientBrush(hubPath))
                {
                    hubBrush.CenterColor = Color.FromArgb(70, 75, 80);
                    hubBrush.SurroundColors = new[] { Color.FromArgb(40, 45, 50) };
                    g.FillEllipse(hubBrush, hubRect);
                }
            }

            // Highlight
            int highlightRadius = 4;
            using (SolidBrush highlightBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
            {
                g.FillEllipse(highlightBrush,
                    centerX - highlightRadius - 3,
                    centerY - highlightRadius - 3,
                    highlightRadius * 2,
                    highlightRadius * 2);
            }
        }

        private void DrawDigitalDisplay(Graphics g, int centerX, int centerY, int radius)
        {
            int displayY = centerY + radius - 15; // Position inside the gauge near bottom
            int displayWidth = (int)(radius * 0.8f);
            int displayHeight = 28;
            int displayX = centerX - displayWidth / 2;

            // Draw display background with rounded rectangle
            Rectangle displayRect = new Rectangle(displayX, displayY, displayWidth, displayHeight);
            using (GraphicsPath displayPath = new GraphicsPath())
            {
                int cornerRadius = 4;
                displayPath.AddArc(displayRect.X, displayRect.Y, cornerRadius, cornerRadius, 180, 90);
                displayPath.AddArc(displayRect.Right - cornerRadius, displayRect.Y, cornerRadius, cornerRadius, 270, 90);
                displayPath.AddArc(displayRect.Right - cornerRadius, displayRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
                displayPath.AddArc(displayRect.X, displayRect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                displayPath.CloseFigure();
                
                using (LinearGradientBrush displayBg = new LinearGradientBrush(
                    displayRect,
                    Color.FromArgb(20, 25, 30),
                    Color.FromArgb(30, 35, 40),
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(displayBg, displayPath);
                }
                
                // Draw display border
                using (Pen borderPen = new Pen(Color.FromArgb(80, 85, 90), 2))
                {
                    g.DrawPath(borderPen, displayPath);
                }
            }

            // Draw digital value
            using (Font valueFont = new Font("Consolas", 11f, FontStyle.Bold))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                // Glow effect
                for (int i = 3; i > 0; i--)
                {
                    using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(30, _gaugeColor)))
                    {
                        g.DrawString(_displayValue, valueFont, glowBrush, 
                                    centerX, displayY + displayHeight / 2, sf);
                    }
                }

                // Main text
                using (SolidBrush valueBrush = new SolidBrush(_gaugeColor))
                {
                    g.DrawString(_displayValue, valueFont, valueBrush, 
                                centerX, displayY + displayHeight / 2, sf);
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
