using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Memory_Monitor.Controls
{
    /// <summary>
    /// Compact circular gauge control designed specifically for FPS display.
    /// Shows just the FPS number prominently with a small "FPS" label below.
    /// Avalonia port of the Windows FpsGaugeControl.
    /// </summary>
    public class FpsGaugeControl : Control
    {
        // GDI+ font sizes are in points; Avalonia uses 96 DPI device-independent pixels
        private const double PT_TO_DIP = 96.0 / 72.0;

        private static readonly FontFamily MonoFont = new("Consolas, DejaVu Sans Mono, Liberation Mono, monospace");
        private static readonly FontFamily UiFont = new("fonts:Inter#Inter");
        private static readonly Color TextColor = Colors.White;
        private static readonly Color LabelColor = Color.FromRgb(180, 180, 180);

        private int _fpsValue = 0;

        public int FpsValue => _fpsValue;

        /// <summary>
        /// Sets the FPS value to display
        /// </summary>
        public void SetFps(int fps)
        {
            int newValue = Math.Max(0, fps);
            // Only repaint if value actually changed
            if (_fpsValue != newValue)
            {
                _fpsValue = newValue;
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext g)
        {
            double size = Math.Min(Bounds.Width, Bounds.Height);
            var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
            double gaugeRadius = Math.Floor(size * 0.45);

            if (gaugeRadius < 20) return;

            DrawGaugeFace(g, center, gaugeRadius);
            DrawFpsRing(g, center, gaugeRadius);
            DrawFpsValue(g, center, gaugeRadius);
            DrawFpsLabel(g, center, gaugeRadius);
        }

        private static void DrawGaugeFace(DrawingContext g, Point c, double radius)
        {
            // Shadow
            var shadowBrush = new RadialGradientBrush
            {
                GradientStops = { new GradientStop(Color.FromArgb(40, 0, 0, 0), 0), new GradientStop(Colors.Transparent, 1) }
            };
            g.DrawEllipse(shadowBrush, null, c, radius + 6, radius + 6);

            // Main face
            var faceBrush = new RadialGradientBrush
            {
                GradientStops = { new GradientStop(Color.FromRgb(45, 48, 52), 0), new GradientStop(Color.FromRgb(28, 31, 35), 1) }
            };
            g.DrawEllipse(faceBrush, null, c, radius, radius);

            // Chrome rim
            g.DrawEllipse(null, new Pen(new SolidColorBrush(Color.FromRgb(80, 85, 90)), 2), c, radius + 2, radius + 2);
        }

        private void DrawFpsRing(DrawingContext g, Point c, double radius)
        {
            // Colored ring around the gauge based on FPS quality
            double ringRadius = radius - 8;
            Color ringColor = GetFpsColor();

            // Glow effect
            for (int i = 3; i > 0; i--)
            {
                g.DrawEllipse(null, new Pen(new SolidColorBrush(Color.FromArgb(30, ringColor.R, ringColor.G, ringColor.B)), i * 2),
                    c, ringRadius, ringRadius);
            }

            // Main ring
            g.DrawEllipse(null, new Pen(new SolidColorBrush(ringColor), 4), c, ringRadius, ringRadius);
        }

        private Color GetFpsColor()
        {
            // Color coding based on FPS quality
            if (_fpsValue >= 60)
                return Color.FromRgb(0, 200, 100);  // Green - excellent
            else if (_fpsValue >= 45)
                return Color.FromRgb(150, 200, 50); // Yellow-green - good
            else if (_fpsValue >= 30)
                return Color.FromRgb(255, 180, 0);  // Orange - acceptable
            else
                return Color.FromRgb(255, 80, 80);  // Red - poor
        }

        private void DrawFpsValue(DrawingContext g, Point c, double radius)
        {
            string fpsText = _fpsValue.ToString();

            // Scale font based on radius, reduced for larger numbers to prevent overflow
            double baseFontSize = radius * 0.55;
            if (_fpsValue >= 1000)
                baseFontSize *= 0.50;  // 4+ digits: 50% of base
            else if (_fpsValue >= 100)
                baseFontSize *= 0.70;  // 3 digits: 70% of base

            double fontSize = Math.Max(10, baseFontSize);

            // Offset up slightly to make room for "FPS" label below
            var textCenter = new Point(c.X, c.Y - Math.Floor(radius * 0.1));
            Color glowColor = GetFpsColor();

            DrawCenteredText(g, fpsText, MonoFont, fontSize, Color.FromArgb(60, glowColor.R, glowColor.G, glowColor.B),
                textCenter + new Point(1, 1));
            DrawCenteredText(g, fpsText, MonoFont, fontSize, TextColor, textCenter);
        }

        private static void DrawFpsLabel(DrawingContext g, Point c, double radius)
        {
            double fontSize = Math.Max(8, radius * 0.2);
            DrawCenteredText(g, "FPS", UiFont, fontSize, LabelColor, new Point(c.X, c.Y + Math.Floor(radius * 0.35)));
        }

        private static void DrawCenteredText(DrawingContext g, string text, FontFamily family, double fontSizePt, Color color, Point center)
        {
            var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(family, FontStyle.Normal, FontWeight.Bold), fontSizePt * PT_TO_DIP, new SolidColorBrush(color));
            g.DrawText(formatted, new Point(center.X - formatted.Width / 2, center.Y - formatted.Height / 2));
        }
    }
}
