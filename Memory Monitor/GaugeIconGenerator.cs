using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Memory_Monitor
{
    /// <summary>
    /// Generates gauge-style icons for system tray use.
    /// Creates dynamic icons that display live percentage values.
    /// </summary>
    public static class GaugeIconGenerator
    {
        // Standard icon sizes
        public const int TrayIconSize = 16;

        // Default colors matching CompactGaugeControl
        private static readonly Color DefaultGaugeColor = Color.FromArgb(58, 150, 221);
        private static readonly Color DefaultBackgroundColor = Color.FromArgb(30, 33, 37);
        private static readonly Color DefaultArcBackgroundColor = Color.FromArgb(60, 60, 60);

        /// <summary>
        /// Creates a dynamic tray icon showing the current percentage value as a gauge arc.
        /// </summary>
        /// <param name="percentage">Value from 0 to 100</param>
        /// <param name="gaugeColor">Color of the filled arc (null for default blue)</param>
        /// <param name="size">Icon size in pixels (default 16 for tray)</param>
        /// <returns>Icon that must be disposed by caller</returns>
        public static Icon CreateDynamicTrayIcon(float percentage, Color? gaugeColor = null, int size = TrayIconSize)
        {
            percentage = Math.Max(0, Math.Min(100, percentage));
            var color = gaugeColor ?? DefaultGaugeColor;

            using var bitmap = new Bitmap(size, size);
            using var g = Graphics.FromImage(bitmap);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            int padding = size >= 32 ? 2 : 1;
            int arcThickness = size >= 32 ? 3 : 2;
            
            var rect = new Rectangle(padding, padding, size - padding * 2, size - padding * 2);

            // Draw dark background circle for better visibility
            using (var bgBrush = new SolidBrush(DefaultBackgroundColor))
            {
                g.FillEllipse(bgBrush, rect);
            }

            // Adjust arc rectangle for pen thickness
            var arcRect = new Rectangle(
                rect.X + arcThickness / 2,
                rect.Y + arcThickness / 2,
                rect.Width - arcThickness,
                rect.Height - arcThickness);

            // Draw background arc (270 degrees, starting from bottom-left)
            const float startAngle = 135f;
            const float sweepAngle = 270f;

            using (var bgPen = new Pen(DefaultArcBackgroundColor, arcThickness))
            {
                bgPen.StartCap = bgPen.EndCap = LineCap.Round;
                g.DrawArc(bgPen, arcRect, startAngle, sweepAngle);
            }

            // Draw filled arc based on percentage
            float filledSweep = sweepAngle * (percentage / 100f);
            if (filledSweep >= 1.0f)
            {
                using var filledPen = new Pen(color, arcThickness);
                filledPen.StartCap = filledPen.EndCap = LineCap.Round;
                g.DrawArc(filledPen, arcRect, startAngle, filledSweep);
            }

            // For larger icons, add center dot
            if (size >= 32)
            {
                int dotSize = size / 8;
                int dotX = size / 2 - dotSize / 2;
                int dotY = size / 2 - dotSize / 2;
                using var dotBrush = new SolidBrush(color);
                g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
            }

            return CreateIconFromBitmap(bitmap);
        }

        /// <summary>
        /// Gets a color based on the percentage value (green -> yellow -> red)
        /// </summary>
        public static Color GetColorForPercentage(float percentage)
        {
            if (percentage < 50)
            {
                // Green to Yellow
                int r = (int)(percentage / 50 * 255);
                return Color.FromArgb(r, 200, 80);
            }
            else if (percentage < 80)
            {
                // Yellow to Orange
                float t = (percentage - 50) / 30;
                int g = (int)(200 - t * 100);
                return Color.FromArgb(255, g, 80);
            }
            else
            {
                // Orange to Red
                float t = (percentage - 80) / 20;
                int g = (int)(100 - t * 100);
                return Color.FromArgb(255, Math.Max(0, g), 80);
            }
        }

        private static Icon CreateIconFromBitmap(Bitmap bitmap)
        {
            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);
            // Create a copy so we can destroy the original handle
            Icon clonedIcon = (Icon)icon.Clone();
            DestroyIcon(hIcon);
            return clonedIcon;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}
