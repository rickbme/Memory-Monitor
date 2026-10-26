using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using static Memory_Monitor.Controls.PanelDrawing;

namespace Memory_Monitor.Controls
{
    /// <summary>
    /// Bar graph panel with a short bar history, large value and secondary info line.
    /// Used for CPU, GPU, VRAM and Drive usage panels.
    /// Avalonia port of the Windows BarGraphPanelControl.
    /// </summary>
    public class BarGraphPanelControl : Control
    {
        private const int MAX_HISTORY = 12;
        private readonly Queue<float> _valueHistory = new(Enumerable.Repeat(0f, MAX_HISTORY));

        private float _currentValue = 0;
        private string _primaryText = "0%";
        private string _secondaryText = "";
        private string _tertiaryText = "";

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<BarGraphPanelControl, string>(nameof(Title), "USAGE");

        public static readonly StyledProperty<Color> AccentColorProperty =
            AvaloniaProperty.Register<BarGraphPanelControl, Color>(nameof(AccentColor), Color.FromRgb(0, 200, 255));

        public static readonly StyledProperty<float> MaxValueProperty =
            AvaloniaProperty.Register<BarGraphPanelControl, float>(nameof(MaxValue), 100f, coerce: (_, v) => Math.Max(1, v));

        static BarGraphPanelControl()
        {
            AffectsRender<BarGraphPanelControl>(TitleProperty, AccentColorProperty, MaxValueProperty);
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

        public float MaxValue
        {
            get => GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public float CurrentValue => _currentValue;

        /// <summary>
        /// Updates the value and adds it to the history for the bar graph
        /// </summary>
        public void SetValue(float value, string primaryText, string secondaryText = "", string tertiaryText = "")
        {
            _currentValue = Math.Max(0, Math.Min(value, MaxValue));
            _primaryText = primaryText;
            _secondaryText = secondaryText;
            _tertiaryText = tertiaryText;

            if (_valueHistory.Count >= MAX_HISTORY)
                _valueHistory.Dequeue();
            _valueHistory.Enqueue(_currentValue);

            InvalidateVisual();
        }

        public override void Render(DrawingContext g)
        {
            Size size = Bounds.Size;
            if (size.Width < 40 || size.Height < 40) return;

            DrawPanel(g, size);
            DrawTitle(g, size, Title);
            DrawBarGraph(g, size);
            DrawPrimaryValue(g, size);
            DrawSecondaryInfo(g, size);
            DrawBorderAccents(g, size, AccentColor);
        }

        private void DrawBarGraph(DrawingContext g, Size size)
        {
            float[] history = _valueHistory.ToArray();
            int barCount = history.Length;
            Color barColor = AccentColor;

            double graphLeft = 15;
            double graphRight = size.Width - 15;
            double graphTop = Math.Floor(size.Height * 0.22);
            double graphBottom = Math.Floor(size.Height * 0.50);
            double graphWidth = graphRight - graphLeft;
            double graphHeight = graphBottom - graphTop;

            double barSpacing = 3;
            double barWidth = Math.Max(8, Math.Floor((graphWidth - (barSpacing * (barCount - 1))) / barCount));

            var glowBrush = new SolidColorBrush(WithAlpha(barColor, 40));
            var gradientBrush = VerticalGradient(barColor,
                Color.FromArgb(180, (byte)(barColor.R / 2), (byte)(barColor.G / 2), (byte)(barColor.B / 2)));
            var solidBrush = new SolidColorBrush(barColor);
            var highlightBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));

            for (int i = 0; i < barCount; i++)
            {
                double percentage = MaxValue > 0 ? history[i] / MaxValue : 0;
                double barHeight = Math.Max(2, Math.Floor(graphHeight * percentage));

                double x = graphLeft + i * (barWidth + barSpacing);
                double y = graphBottom - barHeight;

                // Glow behind bar
                g.FillRectangle(glowBrush, new Rect(x - 2, y - 2, barWidth + 4, barHeight + 4));

                // Bar with gradient (solid when too short for a gradient to show)
                g.FillRectangle(barHeight > 4 ? gradientBrush : solidBrush, new Rect(x, y, barWidth, barHeight));

                // Highlight at top of bar
                if (barHeight > 6)
                {
                    g.FillRectangle(highlightBrush, new Rect(x, y, barWidth, 2));
                }
            }

            // Baseline glow
            g.DrawLine(new Pen(new SolidColorBrush(WithAlpha(AccentColor, 60)), 1),
                new Point(graphLeft, graphBottom + 2.5), new Point(graphRight, graphBottom + 2.5));
        }

        private void DrawPrimaryValue(DrawingContext g, Size size)
        {
            double fontSize = Math.Max(20, size.Height * 0.16);
            double valueY = Math.Floor(size.Height * 0.52);

            // Soft glow offset by one pixel, then the text itself
            g.DrawText(Text(_primaryText, fontSize, FontWeight.Bold, WithAlpha(AccentColor, 50)), new Point(16, valueY));
            g.DrawText(Text(_primaryText, fontSize, FontWeight.Bold, AccentColor), new Point(15, valueY));
        }

        private void DrawSecondaryInfo(DrawingContext g, Size size)
        {
            if (string.IsNullOrEmpty(_secondaryText) && string.IsNullOrEmpty(_tertiaryText))
                return;

            // Smaller font for secondary info to fit longer text like "DISK: 772 GB / 953 GB"
            double fontSize = Math.Max(7, size.Height * 0.042);
            double infoY = Math.Floor(size.Height * 0.84);

            if (!string.IsNullOrEmpty(_secondaryText))
            {
                g.DrawText(Text(_secondaryText, fontSize, FontWeight.Normal, SecondaryTextColor), new Point(15, infoY));
            }

            if (!string.IsNullOrEmpty(_tertiaryText))
            {
                // Tertiary info on the right
                var text = Text(_tertiaryText, fontSize, FontWeight.Normal, SecondaryTextColor);
                g.DrawText(text, new Point(size.Width - text.Width - 15, infoY));
            }
        }
    }
}
