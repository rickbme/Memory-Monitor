using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using static Memory_Monitor.Controls.PanelDrawing;

namespace Memory_Monitor.Controls
{
    /// <summary>
    /// Network panel with horizontal upload and download bars.
    /// Avalonia port of the Windows NetworkBarPanelControl.
    /// </summary>
    public class NetworkBarPanelControl : Control
    {
        private static readonly Color LabelColor = Color.FromRgb(180, 180, 180);
        private static readonly Color TrackColor = Color.FromRgb(40, 50, 60);

        private float _uploadSpeed = 0;
        private float _downloadSpeed = 0;
        private string _uploadText = "0 KB/s";
        private string _downloadText = "0 KB/s";

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<NetworkBarPanelControl, string>(nameof(Title), "NETWORK");

        public static readonly StyledProperty<Color> AccentColorProperty =
            AvaloniaProperty.Register<NetworkBarPanelControl, Color>(nameof(AccentColor), Color.FromRgb(0, 200, 255));

        public static readonly StyledProperty<float> MaxSpeedProperty =
            AvaloniaProperty.Register<NetworkBarPanelControl, float>(nameof(MaxSpeed), 100f, coerce: (_, v) => Math.Max(1, v));

        static NetworkBarPanelControl()
        {
            AffectsRender<NetworkBarPanelControl>(TitleProperty, AccentColorProperty, MaxSpeedProperty);
        }

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Color AccentColor
        {
            get => GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        /// <summary>
        /// Speed that fills a bar completely, in the same unit as the speeds passed to SetSpeeds
        /// </summary>
        public float MaxSpeed
        {
            get => GetValue(MaxSpeedProperty);
            set => SetValue(MaxSpeedProperty, value);
        }

        public void SetSpeeds(float upload, float download, string uploadText, string downloadText)
        {
            _uploadSpeed = Math.Max(0, upload);
            _downloadSpeed = Math.Max(0, download);
            _uploadText = uploadText;
            _downloadText = downloadText;
            InvalidateVisual();
        }

        public override void Render(DrawingContext g)
        {
            Size size = Bounds.Size;
            if (size.Width < 40 || size.Height < 40) return;

            DrawPanel(g, size);
            DrawTitle(g, size, Title);
            DrawSpeedSection(g, size, "UPLOAD", _uploadSpeed, _uploadText, 0.18);
            DrawSpeedSection(g, size, "DOWNLOAD", _downloadSpeed, _downloadText, 0.52);
            DrawBorderAccents(g, size, AccentColor);
        }

        private void DrawSpeedSection(DrawingContext g, Size size, string label, float speed, string speedText, double labelPosition)
        {
            Color color = AccentColor;

            double barLeft = 15;
            double barRight = size.Width - 15;
            double barWidth = barRight - barLeft;
            double barHeight = Math.Max(14, Math.Min(Math.Floor(size.Height * 0.10), 24));

            // Layout: label at top, bar below label, speed value below bar
            double labelY = Math.Floor(size.Height * labelPosition);
            double barY = labelY + Math.Floor(size.Height * 0.10);
            double speedY = barY + barHeight + 4;

            double labelFontSize = Math.Max(7, size.Height * 0.042);
            double speedFontSize = Math.Max(10, size.Height * 0.08);

            g.DrawText(Text(label, labelFontSize, FontWeight.Normal, LabelColor), new Point(barLeft, labelY));

            // Background track
            g.DrawRectangle(new SolidColorBrush(TrackColor), null, new Rect(barLeft, barY, barWidth, barHeight), 4, 4);

            // Filled bar
            double percentage = MaxSpeed > 0 ? speed / MaxSpeed : 0;
            double filledWidth = Math.Max(4, Math.Floor(barWidth * Math.Min(1, percentage)));

            if (percentage > 0.001)
            {
                var fillRect = new Rect(barLeft, barY, filledWidth, barHeight);

                // Glow effect
                g.FillRectangle(new SolidColorBrush(WithAlpha(color, 30)),
                    new Rect(barLeft - 2, barY - 2, filledWidth + 4, barHeight + 4));

                // Filled bar with gradient
                g.DrawRectangle(VerticalGradient(color,
                        Color.FromArgb(200, (byte)(color.R * 2 / 3), (byte)(color.G * 2 / 3), (byte)(color.B * 2 / 3))),
                    null, fillRect, 4, 4);

                // Highlight at top
                if (filledWidth > 6)
                {
                    g.FillRectangle(new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                        new Rect(barLeft + 2, barY + 1, filledWidth - 4, 2));
                }

                DrawBarSegments(g, barLeft, barY, filledWidth, barHeight);
            }

            // Speed value below the bar, aligned right
            var text = Text(speedText, speedFontSize, FontWeight.Bold, color);
            g.DrawText(text, new Point(barRight - text.Width, speedY));
        }

        private static void DrawBarSegments(DrawingContext g, double x, double y, double width, double height)
        {
            // Vertical segment lines for a "tech" look
            double segmentWidth = 8;
            double gap = 2;
            var segmentPen = new Pen(new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)), 1);

            for (double sx = x + segmentWidth; sx < x + width - gap; sx += segmentWidth + gap)
            {
                g.DrawLine(segmentPen, new Point(sx + 0.5, y + 2), new Point(sx + 0.5, y + height - 2));
            }
        }
    }
}
