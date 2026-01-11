using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Memory_Monitor
{
    public class CircularGaugeControl : Control
    {
        private float _currentValue = 0;
        private float _maxValue = 100;
        private Color _gaugeColor = Color.FromArgb(220, 50, 50); // Red needle color
        private Color _backgroundColor = Color.FromArgb(245, 242, 235); // Beige background
        private Color _needleColor = Color.FromArgb(220, 50, 50); // Red needle
        private Color _tickColor = Color.FromArgb(40, 40, 40); // Dark tick marks

        public CircularGaugeControl()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.MinimumSize = new Size(120, 120);
        }

        public Color GaugeColor
        {
            get => _gaugeColor;
            set
            {
                _gaugeColor = value;
                _needleColor = value; // Keep needle same color as gauge
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

        public Color TickColor
        {
            get => _tickColor;
            set
            {
                _tickColor = value;
                Invalidate();
            }
        }

        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = Math.Max(1, value); // Prevent division by zero
                Invalidate();
            }
        }

        public void SetValue(float value, string displayText)
        {
            _currentValue = Math.Max(0, Math.Min(value, _maxValue));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Calculate dimensions
            int size = Math.Min(Width, Height);
            int centerX = Width / 2;
            int centerY = Height / 2;
            int gaugeRadius = (int)(size * 0.42f);
            int rimWidth = (int)(size * 0.04f);

            // Ensure minimum dimensions
            if (gaugeRadius < 20)
                return;

            // Draw outer rim (3D effect with gradient)
            DrawRim(g, centerX, centerY, gaugeRadius, rimWidth);

            // Draw main gauge face
            using (SolidBrush faceBrush = new SolidBrush(_backgroundColor))
            {
                g.FillEllipse(faceBrush, 
                    centerX - gaugeRadius + rimWidth, 
                    centerY - gaugeRadius + rimWidth,
                    (gaugeRadius - rimWidth) * 2, 
                    (gaugeRadius - rimWidth) * 2);
            }

            // Draw tick marks
            DrawTickMarks(g, centerX, centerY, gaugeRadius, rimWidth);

            // Draw tapered needle
            DrawTaperedNeedle(g, centerX, centerY, gaugeRadius, rimWidth);

            // Draw center hub
            DrawCenterHub(g, centerX, centerY, gaugeRadius);
        }

        private void DrawRim(Graphics g, int centerX, int centerY, int radius, int rimWidth)
        {
            // Outer rim with gradient for 3D effect
            using (GraphicsPath path = new GraphicsPath())
            {
                Rectangle outerRect = new Rectangle(
                    centerX - radius,
                    centerY - radius,
                    radius * 2,
                    radius * 2);
                path.AddEllipse(outerRect);

                using (PathGradientBrush gradBrush = new PathGradientBrush(path))
                {
                    gradBrush.CenterColor = Color.FromArgb(90, 95, 100);
                    gradBrush.SurroundColors = new[] { Color.FromArgb(50, 55, 60) };
                    g.FillEllipse(gradBrush, outerRect);
                }
            }

            // Inner rim edge
            using (Pen rimPen = new Pen(Color.FromArgb(30, 35, 40), 2))
            {
                g.DrawEllipse(rimPen,
                    centerX - radius + rimWidth,
                    centerY - radius + rimWidth,
                    (radius - rimWidth) * 2,
                    (radius - rimWidth) * 2);
            }
        }

        private void DrawTickMarks(Graphics g, int centerX, int centerY, int radius, int rimWidth)
        {
            float startAngle = 135f; // Start at bottom left
            float sweepAngle = 270f; // 270 degree sweep
            int majorTicks = 11; // 0, 10, 20, ... 100% of max value
            int minorTicksPerMajor = 5;

            float angleStep = sweepAngle / (majorTicks - 1);
            float minorAngleStep = angleStep / minorTicksPerMajor;

            // Draw all ticks
            for (int i = 0; i < majorTicks; i++)
            {
                float angle = startAngle + (i * angleStep);
                DrawTick(g, centerX, centerY, radius, rimWidth, angle, true);

                // Draw minor ticks between major ticks (except after last major tick)
                if (i < majorTicks - 1)
                {
                    for (int j = 1; j < minorTicksPerMajor; j++)
                    {
                        float minorAngle = angle + (j * minorAngleStep);
                        DrawTick(g, centerX, centerY, radius, rimWidth, minorAngle, false);
                    }
                }
            }
        }

        private void DrawTick(Graphics g, int centerX, int centerY, int radius, int rimWidth, float angle, bool isMajor)
        {
            double angleRad = angle * Math.PI / 180.0;
            
            float tickLength = isMajor ? radius * 0.12f : radius * 0.07f;
            float tickWidth = isMajor ? 2.5f : 1.5f;
            float innerRadius = radius - rimWidth - tickLength;
            float outerRadius = radius - rimWidth - 2;

            int x1 = centerX + (int)(innerRadius * Math.Cos(angleRad));
            int y1 = centerY + (int)(innerRadius * Math.Sin(angleRad));
            int x2 = centerX + (int)(outerRadius * Math.Cos(angleRad));
            int y2 = centerY + (int)(outerRadius * Math.Sin(angleRad));

            using (Pen tickPen = new Pen(_tickColor, tickWidth))
            {
                tickPen.StartCap = LineCap.Round;
                tickPen.EndCap = LineCap.Round;
                g.DrawLine(tickPen, x1, y1, x2, y2);
            }
        }

        private void DrawTaperedNeedle(Graphics g, int centerX, int centerY, int radius, int rimWidth)
        {
            float startAngle = 135f;
            float sweepAngle = 270f;
            float percentage = _maxValue > 0 ? _currentValue / _maxValue : 0;
            float needleAngle = startAngle + (sweepAngle * percentage);
            double angleRad = needleAngle * Math.PI / 180.0;

            float needleLength = radius - rimWidth - radius * 0.18f;
            float needleBaseWidth = radius * 0.06f; // Width at base

            // Calculate needle points
            PointF tipPoint = new PointF(
                centerX + (float)(needleLength * Math.Cos(angleRad)),
                centerY + (float)(needleLength * Math.Sin(angleRad))
            );

            // Calculate base points (perpendicular to needle direction)
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

            // Draw tapered needle as filled polygon
            PointF[] needlePoints = { baseLeft, tipPoint, baseRight };
            
            using (GraphicsPath needlePath = new GraphicsPath())
            {
                needlePath.AddPolygon(needlePoints);
                
                // Fill with gradient for depth
                using (LinearGradientBrush needleBrush = new LinearGradientBrush(
                    baseLeft, tipPoint, 
                    _needleColor, 
                    Color.FromArgb(Math.Min(255, _needleColor.R + 30), 
                                   Math.Min(255, _needleColor.G + 30), 
                                   Math.Min(255, _needleColor.B + 30))))
                {
                    g.FillPath(needleBrush, needlePath);
                }

                // Draw outline for definition
                using (Pen outlinePen = new Pen(Color.FromArgb(150, 0, 0, 0), 1f))
                {
                    g.DrawPath(outlinePen, needlePath);
                }
            }
        }

        private void DrawCenterHub(Graphics g, int centerX, int centerY, int radius)
        {
            int hubRadius = (int)(radius * 0.11f);

            // Draw outer hub ring (darker)
            using (SolidBrush outerBrush = new SolidBrush(Color.FromArgb(60, 65, 70)))
            {
                g.FillEllipse(outerBrush,
                    centerX - hubRadius,
                    centerY - hubRadius,
                    hubRadius * 2,
                    hubRadius * 2);
            }

            // Draw inner hub with gradient
            int innerHubRadius = (int)(hubRadius * 0.75f);
            using (GraphicsPath path = new GraphicsPath())
            {
                Rectangle hubRect = new Rectangle(
                    centerX - innerHubRadius,
                    centerY - innerHubRadius,
                    innerHubRadius * 2,
                    innerHubRadius * 2);
                path.AddEllipse(hubRect);

                using (PathGradientBrush gradBrush = new PathGradientBrush(path))
                {
                    gradBrush.CenterColor = Color.FromArgb(90, 95, 100);
                    gradBrush.SurroundColors = new[] { Color.FromArgb(60, 65, 70) };
                    g.FillEllipse(gradBrush, hubRect);
                }
            }

            // Add highlight for 3D effect
            int highlightRadius = (int)(innerHubRadius * 0.4f);
            using (SolidBrush highlightBrush = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
            {
                g.FillEllipse(highlightBrush,
                    centerX - highlightRadius - innerHubRadius / 3,
                    centerY - highlightRadius - innerHubRadius / 3,
                    highlightRadius * 2,
                    highlightRadius * 2);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            // No fonts to dispose anymore
            base.Dispose(disposing);
        }
    }
}
