using System;
using System.Drawing;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Reusable warning overlay panel for displaying messages with proper resource disposal.
    /// </summary>
    public class WarningOverlay : Panel
    {
        private readonly Font _titleFont;
        private readonly Font _messageFont;
        private readonly Font _buttonFont;
        private bool _disposed = false;

        /// <summary>
        /// Creates a warning overlay with title, message, and dismiss button.
        /// </summary>
        /// <param name="title">Warning title text</param>
        /// <param name="message">Warning message body</param>
        /// <param name="onDismiss">Optional callback when dismissed</param>
        public WarningOverlay(string title, string message, Action? onDismiss = null)
        {
            _titleFont = new Font("Segoe UI", 12f, FontStyle.Bold);
            _messageFont = new Font("Segoe UI", 9f, FontStyle.Regular);
            _buttonFont = new Font("Segoe UI", 9f, FontStyle.Bold);

            BackColor = Color.FromArgb(220, 30, 30, 35);
            Size = new Size(500, 120);
            BorderStyle = BorderStyle.None;

            var titleLabel = new Label
            {
                Text = title,
                Font = _titleFont,
                ForeColor = Color.FromArgb(255, 200, 50),
                AutoSize = false,
                Size = new Size(480, 25),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var messageLabel = new Label
            {
                Text = message,
                Font = _messageFont,
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = false,
                Size = new Size(400, 60),
                Location = new Point(50, 35),
                TextAlign = ContentAlignment.TopLeft
            };

            var dismissButton = new Button
            {
                Text = "Dismiss",
                Font = _buttonFont,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, 65, 70),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 28),
                Location = new Point(210, 85),
                Cursor = Cursors.Hand
            };
            dismissButton.FlatAppearance.BorderColor = Color.FromArgb(100, 105, 110);
            dismissButton.Click += (s, e) =>
            {
                onDismiss?.Invoke();
                Parent?.Controls.Remove(this);
                Dispose();
            };

            Controls.Add(titleLabel);
            Controls.Add(messageLabel);
            Controls.Add(dismissButton);

            Paint += WarningOverlay_Paint;
        }

        private void WarningOverlay_Paint(object? sender, PaintEventArgs e)
        {
            using var borderPen = new Pen(Color.FromArgb(255, 200, 50), 2);
            e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        }

        /// <summary>
        /// Centers the overlay on the specified parent control.
        /// </summary>
        public void CenterOn(Control parent)
        {
            Location = new Point(
                (parent.ClientSize.Width - Width) / 2,
                (parent.ClientSize.Height - Height) / 2
            );
        }

        /// <summary>
        /// Shows the overlay centered on the specified parent control.
        /// </summary>
        public void ShowOn(Control parent)
        {
            CenterOn(parent);
            parent.Controls.Add(this);
            BringToFront();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _titleFont?.Dispose();
                    _messageFont?.Dispose();
                    _buttonFont?.Dispose();
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
