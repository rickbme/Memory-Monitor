using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Memory_Monitor.Controls
{
    /// <summary>
    /// Circular needle gauge for horizontal mini monitor displays (1920x480).
    /// Avalonia port of the Windows CompactGaugeControl: same geometry, colors and proportions.
    /// </summary>
    public class GaugeControl : Control
    {
        private const double START_ANGLE = 135;
        private const double SWEEP_ANGLE = 270;

        // GDI+ font sizes are in points; Avalonia uses 96 DPI device-independent pixels
        private const double PT_TO_DIP = 96.0 / 72.0;

        private static readonly FontFamily MonoFont = new("Consolas, DejaVu Sans Mono, Liberation Mono, monospace");
        private static readonly FontFamily UiFont = new("fonts:Inter#Inter");

        private static readonly Color FaceCenterColor = Color.FromRgb(50, 53, 57);
        private static readonly Color FaceEdgeColor = Color.FromRgb(30, 33, 37);
        private static readonly Color RimColor = Color.FromRgb(100, 105, 110);
        private static readonly Color ArcBackgroundColor = Color.FromRgb(50, 50, 50);
        private static readonly Color TickColor = Color.FromRgb(150, 150, 150);

        private float _currentValue = 0;
        private string _displayValue = "0";
        private string _secondaryValue = "";  // For temperature display
        private bool _isHovered = false;

        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<GaugeControl, string>(nameof(Label), "");

        public static readonly StyledProperty<string> DeviceNameProperty =
            AvaloniaProperty.Register<GaugeControl, string>(nameof(DeviceName), "");

        public static readonly StyledProperty<Color> GaugeColorProperty =
            AvaloniaProperty.Register<GaugeControl, Color>(nameof(GaugeColor), Color.FromRgb(58, 150, 221));

        public static readonly StyledProperty<Color> LabelColorProperty =
            AvaloniaProperty.Register<GaugeControl, Color>(nameof(LabelColor), Color.FromRgb(200, 200, 200));

        public static readonly StyledProperty<Color> SecondaryColorProperty =
            AvaloniaProperty.Register<GaugeControl, Color>(nameof(SecondaryColor), Color.FromRgb(255, 100, 100));

        public static readonly StyledProperty<float> MaxValueProperty =
            AvaloniaProperty.Register<GaugeControl, float>(nameof(MaxValue), 100f, coerce: (_, v) => Math.Max(0.001f, v));

        public static readonly StyledProperty<bool> IsSelectableProperty =
            AvaloniaProperty.Register<GaugeControl, bool>(nameof(IsSelectable));

        public static readonly StyledProperty<bool> HasMultipleDevicesProperty =
            AvaloniaProperty.Register<GaugeControl, bool>(nameof(HasMultipleDevices));

        static GaugeControl()
        {
            AffectsRender<GaugeControl>(LabelProperty, DeviceNameProperty, GaugeColorProperty, LabelColorProperty,
                SecondaryColorProperty, MaxValueProperty, IsSelectableProperty, HasMultipleDevicesProperty);
        }

        #region Properties

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the device name displayed above the label (e.g., "RTX 3060", "nvme0n1", "wlan0")
        /// </summary>
        public string DeviceName
        {
            get => GetValue(DeviceNameProperty);
            set => SetValue(DeviceNameProperty, value);
        }

        public Color GaugeColor
        {
            get => GetValue(GaugeColorProperty);
            set => SetValue(GaugeColorProperty, value);
        }

        public Color LabelColor
        {
            get => GetValue(LabelColorProperty);
            set => SetValue(LabelColorProperty, value);
        }

        public Color SecondaryColor
        {
            get => GetValue(SecondaryColorProperty);
            set => SetValue(SecondaryColorProperty, value);
        }

        public float MaxValue
        {
            get => GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public float CurrentValue => _currentValue;

        /// <summary>
        /// Gets or sets whether this gauge supports device selection
        /// </summary>
        public bool IsSelectable
        {
            get => GetValue(IsSelectableProperty);
            set => SetValue(IsSelectableProperty, value);
        }

        /// <summary>
        /// Gets or sets whether multiple devices are available for selection
        /// </summary>
        public bool HasMultipleDevices
        {
            get => GetValue(HasMultipleDevicesProperty);
            set => SetValue(HasMultipleDevicesProperty, value);
        }

        private bool CanSelectDevice => IsSelectable && HasMultipleDevices;

        #endregion

        /// <summary>
        /// Event raised when the gauge is clicked or tapped for device selection
        /// </summary>
        public event EventHandler? DeviceSelectionRequested;

        public void SetValue(float value, string displayText) => SetValue(value, displayText, "");

        /// <summary>
        /// Set value with secondary display (e.g., temperature above usage)
        /// </summary>
        public void SetValue(float value, string displayText, string secondaryText)
        {
            float clampedValue = Math.Max(0, Math.Min(value, MaxValue));

            // Only repaint if values actually changed
            if (Math.Abs(_currentValue - clampedValue) > 0.01f ||
                _displayValue != displayText ||
                _secondaryValue != secondaryText)
            {
                _currentValue = clampedValue;
                _displayValue = displayText;
                _secondaryValue = secondaryText;
                InvalidateVisual();
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsSelectableProperty || change.Property == HasMultipleDevicesProperty)
            {
                Cursor = CanSelectDevice ? new Cursor(StandardCursorType.Hand) : Cursor.Default;
            }
        }

        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            if (CanSelectDevice && !_isHovered)
            {
                _isHovered = true;
                InvalidateVisual();
            }
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            if (_isHovered)
            {
                _isHovered = false;
                InvalidateVisual();
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (CanSelectDevice && e.InitialPressMouseButton == MouseButton.Left)
            {
                DeviceSelectionRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        public override void Render(DrawingContext g)
        {
            double width = Bounds.Width;
            double height = Bounds.Height;

            // Transparent fill so the whole gauge area receives pointer input
            g.FillRectangle(Brushes.Transparent, new Rect(0, 0, width, height));

            // Calculate size to fit gauge, device name, and label
            double labelAreaHeight = string.IsNullOrEmpty(DeviceName) ? 25 : 45;
            double availableHeight = height - labelAreaHeight;
            double size = Math.Min(width, availableHeight);

            var center = new Point(width / 2, availableHeight / 2);
            double gaugeRadius = Math.Floor(size * 0.42);

            if (gaugeRadius < 15) return;

            if (_isHovered && CanSelectDevice)
            {
                DrawHoverHighlight(g, center, gaugeRadius);
            }

            DrawGaugeFace(g, center, gaugeRadius);
            DrawColoredArc(g, center, gaugeRadius);
            DrawTickMarks(g, center, gaugeRadius);
            DrawNeedle(g, center, gaugeRadius);
            DrawCenterHub(g, center);

            if (!string.IsNullOrEmpty(_secondaryValue))
            {
                DrawSecondaryValue(g, center, gaugeRadius);
            }

            DrawDigitalValue(g, center, gaugeRadius);
            DrawLabel(g, center.X, availableHeight);

            if (CanSelectDevice)
            {
                DrawSelectionIndicator(g, center, gaugeRadius);
            }
        }

        private static Point PointOnCircle(Point center, double radius, double angleDegrees)
        {
            double rad = angleDegrees * Math.PI / 180.0;
            return new Point(center.X + radius * Math.Cos(rad), center.Y + radius * Math.Sin(rad));
        }

        private static Color WithAlpha(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

        private static Pen RoundPen(Color color, double thickness) =>
            new(new SolidColorBrush(color), thickness, lineCap: PenLineCap.Round);

        private static StreamGeometry Polygon(params Point[] points)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(points[0], true);
                for (int i = 1; i < points.Length; i++)
                    ctx.LineTo(points[i]);
                ctx.EndFigure(true);
            }
            return geometry;
        }

        private static StreamGeometry Arc(Point center, double radius, double startAngle, double sweepAngle)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(PointOnCircle(center, radius, startAngle), false);
                ctx.ArcTo(PointOnCircle(center, radius, startAngle + sweepAngle), new Size(radius, radius), 0,
                    sweepAngle > 180, SweepDirection.Clockwise);
                ctx.EndFigure(false);
            }
            return geometry;
        }

        private static void DrawCenteredText(DrawingContext g, string text, FontFamily family, double fontSizePt,
            FontWeight weight, Color color, Point center)
        {
            var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(family, FontStyle.Normal, weight), fontSizePt * PT_TO_DIP, new SolidColorBrush(color));
            g.DrawText(formatted, new Point(center.X - formatted.Width / 2, center.Y - formatted.Height / 2));
        }

        private void DrawHoverHighlight(DrawingContext g, Point c, double radius)
        {
            double highlightRadius = radius + 15;
            g.DrawEllipse(new SolidColorBrush(WithAlpha(GaugeColor, 20)), null, c, highlightRadius, highlightRadius);
        }

        private void DrawSelectionIndicator(DrawingContext g, Point c, double radius)
        {
            double indicatorY = c.Y + radius + 5;
            double arrowSize = 6;

            Color indicatorColor = _isHovered
                ? WithAlpha(GaugeColor, 200)
                : Color.FromArgb(100, 150, 150, 150);

            g.DrawGeometry(new SolidColorBrush(indicatorColor), null, Polygon(
                new Point(c.X - arrowSize, indicatorY),
                new Point(c.X + arrowSize, indicatorY),
                new Point(c.X, indicatorY + arrowSize)));
        }

        private static void DrawGaugeFace(DrawingContext g, Point c, double radius)
        {
            var shadowBrush = new RadialGradientBrush
            {
                GradientStops = { new GradientStop(Color.FromArgb(50, 0, 0, 0), 0), new GradientStop(Colors.Transparent, 1) }
            };
            g.DrawEllipse(shadowBrush, null, c, radius + 10, radius + 10);

            var faceBrush = new RadialGradientBrush
            {
                GradientStops = { new GradientStop(FaceCenterColor, 0), new GradientStop(FaceEdgeColor, 1) }
            };
            g.DrawEllipse(faceBrush, null, c, radius, radius);

            g.DrawEllipse(null, new Pen(new SolidColorBrush(RimColor), 2), c, radius + 4, radius + 4);
        }

        private void DrawColoredArc(DrawingContext g, Point c, double radius)
        {
            double percentage = MaxValue > 0 ? _currentValue / MaxValue : 0;
            double currentSweep = SWEEP_ANGLE * percentage;
            double arcRadius = radius - 15;

            if (currentSweep >= 1.0)
            {
                var arc = Arc(c, arcRadius, START_ANGLE, currentSweep);
                for (int i = 5; i > 0; i--)
                {
                    g.DrawGeometry(null, RoundPen(WithAlpha(GaugeColor, 25), i * 2), arc);
                }
                g.DrawGeometry(null, RoundPen(GaugeColor, 8), arc);
            }

            double bgStart = currentSweep >= 1.0 ? START_ANGLE + currentSweep : START_ANGLE;
            double bgSweep = currentSweep >= 1.0 ? SWEEP_ANGLE - currentSweep : SWEEP_ANGLE;

            if (bgSweep > 0.01)
            {
                g.DrawGeometry(null, RoundPen(ArcBackgroundColor, 8), Arc(c, arcRadius, bgStart, bgSweep));
            }
        }

        private static void DrawTickMarks(DrawingContext g, Point c, double radius)
        {
            for (int i = 0; i <= 10; i++)
            {
                double angle = START_ANGLE + (SWEEP_ANGLE * i / 10);

                double innerR = radius - 10;
                double outerR = innerR - (i % 2 == 0 ? 8 : 4);
                double tickWidth = i % 2 == 0 ? 2 : 1;

                g.DrawLine(RoundPen(TickColor, tickWidth), PointOnCircle(c, innerR, angle), PointOnCircle(c, outerR, angle));
            }
        }

        private void DrawNeedle(DrawingContext g, Point c, double radius)
        {
            double percentage = MaxValue > 0 ? _currentValue / MaxValue : 0;
            double needleAngle = START_ANGLE + (SWEEP_ANGLE * percentage);

            double needleLength = radius - 22;
            double baseWidth = 5;

            Point tip = PointOnCircle(c, needleLength, needleAngle);
            Point baseL = PointOnCircle(c, baseWidth, needleAngle - 90);
            Point baseR = PointOnCircle(c, baseWidth, needleAngle + 90);

            var shadowOffset = new Point(1, 1);
            g.DrawGeometry(new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)), null,
                Polygon(baseL + shadowOffset, tip + shadowOffset, baseR + shadowOffset));

            Color needleColor = GaugeColor;
            Color tipColor = Color.FromRgb(
                (byte)Math.Min(255, needleColor.R + 50),
                (byte)Math.Min(255, needleColor.G + 50),
                (byte)Math.Min(255, needleColor.B + 50));

            var needleBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(c, RelativeUnit.Absolute),
                EndPoint = new RelativePoint(tip, RelativeUnit.Absolute),
                GradientStops = { new GradientStop(needleColor, 0), new GradientStop(tipColor, 1) }
            };
            g.DrawGeometry(needleBrush, null, Polygon(baseL, tip, baseR));
        }

        private static void DrawCenterHub(DrawingContext g, Point c)
        {
            double hubR = 8;
            var ringBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                GradientStops = { new GradientStop(Color.FromRgb(80, 85, 90), 0), new GradientStop(Color.FromRgb(45, 50, 55), 1) }
            };
            g.DrawEllipse(ringBrush, null, c, hubR, hubR);

            double innerR = 5;
            g.DrawEllipse(new SolidColorBrush(Color.FromRgb(55, 60, 65)), null, c, innerR, innerR);
        }

        private void DrawSecondaryValue(DrawingContext g, Point c, double radius)
        {
            double tempY = c.Y + Math.Floor(radius * 0.35);
            DrawCenteredText(g, _secondaryValue, MonoFont, Math.Max(8, radius * 0.12), FontWeight.Bold,
                SecondaryColor, new Point(c.X, tempY));
        }

        private void DrawDigitalValue(DrawingContext g, Point c, double radius)
        {
            double displayY = c.Y + Math.Floor(radius * 0.65);
            double displayW = Math.Floor(radius * 0.9);
            double displayH = Math.Floor(radius * 0.18);
            double displayX = c.X - Math.Floor(displayW / 2);

            var displayRect = new Rect(displayX, displayY, displayW, displayH);

            var bgBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                GradientStops = { new GradientStop(Color.FromRgb(20, 25, 30), 0), new GradientStop(Color.FromRgb(30, 35, 40), 1) }
            };
            // Offset the border by half a pixel so the 1px line is crisp
            g.DrawRectangle(bgBrush, new Pen(new SolidColorBrush(Color.FromRgb(60, 65, 70)), 1),
                displayRect.Translate(new Vector(0.5, 0.5)));

            DrawCenteredText(g, _displayValue, MonoFont, Math.Max(8, radius * 0.1), FontWeight.Bold,
                GaugeColor, new Point(c.X, displayY + displayH / 2));
        }

        private void DrawLabel(DrawingContext g, double cx, double gaugeAreaHeight)
        {
            // Draw device name above the label (smaller font, gauge color)
            if (!string.IsNullOrEmpty(DeviceName))
            {
                DrawCenteredText(g, DeviceName, UiFont, 10, FontWeight.Normal, GaugeColor,
                    new Point(cx, gaugeAreaHeight + 5));
            }

            // Draw main label
            if (!string.IsNullOrEmpty(Label))
            {
                double labelY = string.IsNullOrEmpty(DeviceName)
                    ? gaugeAreaHeight + 15
                    : gaugeAreaHeight + 28;

                DrawCenteredText(g, Label, UiFont, 18, FontWeight.Bold, LabelColor, new Point(cx, labelY));
            }
        }
    }
}
