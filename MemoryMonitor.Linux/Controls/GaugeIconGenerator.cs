using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Memory_Monitor.Controls
{
    /// <summary>
    /// Generates gauge-style icons for system tray use.
    /// Creates dynamic icons that display live percentage values.
    /// Avalonia port of the Windows GaugeIconGenerator.
    /// </summary>
    public static class GaugeIconGenerator
    {
        // Linux panels show tray icons at about 22-24px; render larger so they scale down cleanly
        public const int TrayIconSize = 32;

        // Default colors matching GaugeControl
        private static readonly Color DefaultGaugeColor = Color.FromRgb(58, 150, 221);
        private static readonly Color DefaultBackgroundColor = Color.FromRgb(30, 33, 37);
        private static readonly Color DefaultArcBackgroundColor = Color.FromRgb(60, 60, 60);

        /// <summary>
        /// Creates a dynamic tray icon showing the current percentage value as a gauge arc.
        /// </summary>
        public static WindowIcon CreateDynamicTrayIcon(float percentage, Color? gaugeColor = null, int size = TrayIconSize)
        {
            percentage = Math.Max(0, Math.Min(100, percentage));
            var color = gaugeColor ?? DefaultGaugeColor;

            return RenderIcon(size, g =>
            {
                double padding = size >= 32 ? 2 : 1;
                double arcThickness = size >= 32 ? 3 : 2;
                var center = new Point(size / 2.0, size / 2.0);

                // Dark background circle for better visibility
                double radius = size / 2.0 - padding;
                g.DrawEllipse(new SolidColorBrush(DefaultBackgroundColor), null, center, radius, radius);

                // Background arc (270 degrees, starting from bottom-left), inset for the pen thickness
                double arcRadius = radius - arcThickness / 2;
                const double startAngle = 135;
                const double sweepAngle = 270;
                g.DrawGeometry(null, RoundPen(DefaultArcBackgroundColor, arcThickness),
                    Arc(center, arcRadius, startAngle, sweepAngle));

                // Filled arc based on percentage
                double filledSweep = sweepAngle * (percentage / 100);
                if (filledSweep >= 1.0)
                {
                    g.DrawGeometry(null, RoundPen(color, arcThickness), Arc(center, arcRadius, startAngle, filledSweep));
                }

                // For larger icons, add center dot
                if (size >= 32)
                {
                    double dotRadius = size / 16.0;
                    g.DrawEllipse(new SolidColorBrush(color), null, center, dotRadius, dotRadius);
                }
            });
        }

        /// <summary>
        /// Creates a dynamic tray icon showing vertical bars (bar graph style).
        /// Used when bar graph display mode is active.
        /// </summary>
        public static WindowIcon CreateBarGraphTrayIcon(float percentage, Color? barColor = null, int size = TrayIconSize)
        {
            percentage = Math.Max(0, Math.Min(100, percentage));
            var color = barColor ?? Color.FromRgb(0, 200, 220); // Cyan for bar graph mode

            return RenderIcon(size, g =>
            {
                // The Windows layout is designed at 16px; scale it to the icon size
                double scale = size / 16.0;
                var rect = new Rect(scale, scale, size - scale * 2, size - scale * 2);

                // Dark background rounded rectangle
                g.DrawRectangle(new SolidColorBrush(DefaultBackgroundColor), null, rect, 2 * scale, 2 * scale);

                // Vertical bars (4 bars representing activity)
                int barCount = 4;
                double margin = 2 * scale;
                double barSpacing = scale;
                double barWidth = (rect.Width - margin * 2 - (barSpacing * (barCount - 1))) / barCount;
                double barMaxHeight = rect.Height - margin * 2;

                double startX = rect.X + margin;
                double bottomY = rect.Bottom - margin;

                float[] barHeights = GetBarHeights(percentage);
                var dimBrush = new SolidColorBrush(DefaultArcBackgroundColor);
                var barBrush = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, bottomY - barMaxHeight, RelativeUnit.Absolute),
                    EndPoint = new RelativePoint(0, bottomY, RelativeUnit.Absolute),
                    GradientStops =
                    {
                        new GradientStop(color, 0),
                        new GradientStop(Color.FromRgb((byte)(color.R / 2), (byte)(color.G / 2), (byte)(color.B / 2)), 1)
                    }
                };

                for (int i = 0; i < barCount; i++)
                {
                    double barX = startX + i * (barWidth + barSpacing);
                    double barHeight = Math.Max(scale, barMaxHeight * barHeights[i]);

                    // Background bar (dim), then the filled bar
                    g.FillRectangle(dimBrush, new Rect(barX, bottomY - barMaxHeight, barWidth, barMaxHeight));
                    g.FillRectangle(barBrush, new Rect(barX, bottomY - barHeight, barWidth, barHeight));
                }
            });
        }

        /// <summary>
        /// Generates varying bar heights based on percentage to create visual interest.
        /// </summary>
        private static float[] GetBarHeights(float percentage)
        {
            float basePct = percentage / 100f;

            // Create a wave-like pattern that still reflects the overall percentage
            return new float[]
            {
                Math.Min(1f, basePct * 0.7f + 0.1f),
                Math.Min(1f, basePct * 1.0f),
                Math.Min(1f, basePct * 0.85f + 0.05f),
                Math.Min(1f, basePct * 0.6f + 0.15f)
            };
        }

        /// <summary>
        /// Gets a color based on the percentage value (green -> yellow -> red)
        /// </summary>
        public static Color GetColorForPercentage(float percentage)
        {
            if (percentage < 50)
            {
                // Green to Yellow
                byte r = (byte)(percentage / 50 * 255);
                return Color.FromRgb(r, 200, 80);
            }
            else if (percentage < 80)
            {
                // Yellow to Orange
                float t = (percentage - 50) / 30;
                byte g = (byte)(200 - t * 100);
                return Color.FromRgb(255, g, 80);
            }
            else
            {
                // Orange to Red
                float t = (percentage - 80) / 20;
                byte g = (byte)Math.Max(0, 100 - t * 100);
                return Color.FromRgb(255, g, 80);
            }
        }

        private static WindowIcon RenderIcon(int size, Action<DrawingContext> draw)
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize(size, size));
            using (var g = bitmap.CreateDrawingContext())
            {
                draw(g);
            }

            using var stream = new MemoryStream();
            bitmap.Save(stream);
            stream.Position = 0;
            return new WindowIcon(stream);
        }

        private static Pen RoundPen(Color color, double thickness) =>
            new(new SolidColorBrush(color), thickness, lineCap: PenLineCap.Round);

        private static StreamGeometry Arc(Point center, double radius, double startAngle, double sweepAngle)
        {
            static Point OnCircle(Point c, double r, double angle)
            {
                double rad = angle * Math.PI / 180.0;
                return new Point(c.X + r * Math.Cos(rad), c.Y + r * Math.Sin(rad));
            }

            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(OnCircle(center, radius, startAngle), false);
                ctx.ArcTo(OnCircle(center, radius, startAngle + sweepAngle), new Size(radius, radius), 0,
                    sweepAngle > 180, SweepDirection.Clockwise);
                ctx.EndFigure(false);
            }
            return geometry;
        }
    }
}
