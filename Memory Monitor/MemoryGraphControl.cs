using System.Drawing.Drawing2D;

namespace Memory_Monitor
{
    public class MemoryGraphControl : Control
    {
        private List<float> _dataPoints;
        private int _maxDataPoints = 60; // 60 seconds of history
        private Color _lineColor = Color.FromArgb(0, 120, 215); // Windows blue
        private Color _gridColor = Color.FromArgb(40, 40, 40);
        private Color _backgroundColor = Color.FromArgb(20, 20, 20);

        public MemoryGraphControl()
        {
            _dataPoints = new List<float>();
            this.DoubleBuffered = true;
            this.BackColor = _backgroundColor;
        }

        public void AddDataPoint(float value)
        {
            // Value should be between 0 and 100 (percentage)
            if (value < 0) value = 0;
            if (value > 100) value = 100;

            _dataPoints.Add(value);

            // Remove old data points if we exceed max
            if (_dataPoints.Count > _maxDataPoints)
            {
                _dataPoints.RemoveAt(0);
            }

            this.Invalidate(); // Trigger repaint
        }

        public void Clear()
        {
            _dataPoints.Clear();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw background
            using (SolidBrush bgBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // Draw grid lines
            DrawGrid(g);

            // Draw the line graph
            if (_dataPoints.Count > 1)
            {
                DrawGraph(g);
            }

            // Draw border
            using (Pen borderPen = new Pen(Color.FromArgb(60, 60, 60), 1))
            {
                g.DrawRectangle(borderPen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        private void DrawGrid(Graphics g)
        {
            using (Pen gridPen = new Pen(_gridColor, 1))
            {
                gridPen.DashStyle = DashStyle.Dot;

                // Horizontal grid lines (every 25%)
                for (int i = 1; i < 4; i++)
                {
                    int y = (int)(this.Height * i / 4.0f);
                    g.DrawLine(gridPen, 0, y, this.Width, y);
                }

                // Vertical grid lines (every 10 seconds)
                int verticalLines = _maxDataPoints / 10;
                for (int i = 1; i < verticalLines; i++)
                {
                    int x = (int)(this.Width * i / (float)verticalLines);
                    g.DrawLine(gridPen, x, 0, x, this.Height);
                }
            }
        }

        private void DrawGraph(Graphics g)
        {
            if (_dataPoints.Count < 2) return;

            List<PointF> points = new List<PointF>();

            // Calculate points
            float xStep = this.Width / (float)(_maxDataPoints - 1);
            int startIndex = Math.Max(0, _dataPoints.Count - _maxDataPoints);

            for (int i = 0; i < _dataPoints.Count; i++)
            {
                float x = i * xStep;
                float y = this.Height - (_dataPoints[i] / 100.0f * this.Height);
                points.Add(new PointF(x, y));
            }

            // Draw filled area under the line
            if (points.Count > 0)
            {
                List<PointF> fillPoints = new List<PointF>();
                fillPoints.Add(new PointF(points[0].X, this.Height));
                fillPoints.AddRange(points);
                fillPoints.Add(new PointF(points[points.Count - 1].X, this.Height));

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddLines(fillPoints.ToArray());
                    
                    using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                        this.ClientRectangle,
                        Color.FromArgb(80, _lineColor),
                        Color.FromArgb(10, _lineColor),
                        LinearGradientMode.Vertical))
                    {
                        g.FillPath(fillBrush, path);
                    }
                }
            }

            // Draw the line
            using (Pen linePen = new Pen(_lineColor, 2))
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    g.DrawLine(linePen, points[i], points[i + 1]);
                }
            }
        }

        public Color LineColor
        {
            get => _lineColor;
            set
            {
                _lineColor = value;
                this.Invalidate();
            }
        }

        public Color GridColor
        {
            get => _gridColor;
            set
            {
                _gridColor = value;
                this.Invalidate();
            }
        }

        public int MaxDataPoints
        {
            get => _maxDataPoints;
            set
            {
                _maxDataPoints = value;
                if (_dataPoints.Count > _maxDataPoints)
                {
                    _dataPoints.RemoveRange(0, _dataPoints.Count - _maxDataPoints);
                }
                this.Invalidate();
            }
        }
    }
}
