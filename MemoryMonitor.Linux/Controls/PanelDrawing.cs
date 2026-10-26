using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace Memory_Monitor.Controls
{
    /// <summary>
    /// Drawing helpers shared by the bar graph panels (background, border accents, text)
    /// </summary>
    internal static class PanelDrawing
    {
        // GDI+ font sizes are in points; Avalonia uses 96 DPI device-independent pixels
        public const double PT_TO_DIP = 96.0 / 72.0;

        public static readonly FontFamily UiFont = new("fonts:Inter#Inter");

        public static readonly Color BorderColor = Color.FromRgb(60, 70, 80);
        public static readonly Color TextColor = Colors.White;
        public static readonly Color SecondaryTextColor = Color.FromRgb(180, 180, 180);

        public static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

        public static LinearGradientBrush VerticalGradient(Color top, Color bottom) => new()
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops = { new GradientStop(top, 0), new GradientStop(bottom, 1) }
        };

        public static FormattedText Text(string text, double fontSizePt, FontWeight weight, Color color) =>
            new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(UiFont, FontStyle.Normal, weight), fontSizePt * PT_TO_DIP, new SolidColorBrush(color));

        /// <summary>
        /// Rounded panel with a vertical gradient background and a 2px border
        /// </summary>
        public static void DrawPanel(DrawingContext g, Size size)
        {
            var panelRect = new Rect(2, 2, size.Width - 4, size.Height - 4);
            g.DrawRectangle(VerticalGradient(Color.FromRgb(30, 35, 42), Color.FromRgb(18, 22, 28)),
                new Pen(new SolidColorBrush(BorderColor), 2), panelRect, 8, 8);
        }

        public static void DrawTitle(DrawingContext g, Size size, string title)
        {
            double fontSize = Math.Max(10, size.Height * 0.08);
            g.DrawText(Text(title, fontSize, FontWeight.Bold, TextColor), new Point(15, 10));
        }

        /// <summary>
        /// Accent line along the top and small L-shaped accents in the bottom corners
        /// </summary>
        public static void DrawBorderAccents(DrawingContext g, Size size, Color accentColor)
        {
            double w = size.Width;
            double h = size.Height;

            g.DrawLine(new Pen(new SolidColorBrush(accentColor), 2), new Point(8, 3), new Point(w - 8, 3));

            double cornerSize = 15;
            var cornerPen = new Pen(new SolidColorBrush(WithAlpha(accentColor, 150)), 2);

            // Bottom-left
            g.DrawLine(cornerPen, new Point(3, h - cornerSize - 3), new Point(3, h - 3));
            g.DrawLine(cornerPen, new Point(3, h - 3), new Point(cornerSize + 3, h - 3));

            // Bottom-right
            g.DrawLine(cornerPen, new Point(w - 3, h - cornerSize - 3), new Point(w - 3, h - 3));
            g.DrawLine(cornerPen, new Point(w - 3, h - 3), new Point(w - cornerSize - 3, h - 3));
        }
    }
}
