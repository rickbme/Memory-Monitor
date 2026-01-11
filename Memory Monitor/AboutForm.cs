using System.Drawing.Drawing2D;

namespace Memory_Monitor
{
    /// <summary>
    /// About dialog for DFS - Dad's Fixit Shop with theme support
    /// </summary>
    public class AboutForm : Form
    {
        private Label? titleLabel;
        private Label? subtitleLabel;
        private Label? descLabel;
        private Button? closeButton;

        public AboutForm()
        {
            // Window basics
            this.Text = "About System Monitor";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Gradient background with theme support
            this.Paint += AboutForm_Paint;

            // Title label
            titleLabel = new Label()
            {
                Text = "DFS - Dad's Fixit Shop",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };
            this.Controls.Add(titleLabel);

            // Subtitle
            subtitleLabel = new Label()
            {
                Text = "Electronics • Software • Game Development",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = Color.Transparent
            };
            this.Controls.Add(subtitleLabel);

            // Description
            descLabel = new Label()
            {
                Text = "Making tech accessible, reliable, and fun since 2025.\n\nSystem Monitor v1.0",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            this.Controls.Add(descLabel);

            // Close button
            closeButton = new Button()
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Bottom,
                Height = 40,
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(closeButton);

            this.AcceptButton = closeButton;

            // Apply theme colors
            ApplyTheme();
        }

        private void AboutForm_Paint(object? sender, PaintEventArgs e)
        {
            // Get theme-specific gradient colors
            Color startColor, endColor;

            if (ThemeManager.IsDarkMode())
            {
                // Dark mode: Darker blue gradient
                startColor = Color.FromArgb(20, 60, 120);      // Dark blue
                endColor = Color.FromArgb(30, 90, 150);        // Medium dark blue
            }
            else
            {
                // Light mode: Bright blue gradient
                startColor = Color.FromArgb(0, 120, 215);      // Bright blue
                endColor = Color.FromArgb(0, 200, 255);        // Lighter cyan
            }

            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                startColor,
                endColor,
                45f))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void ApplyTheme()
        {
            bool isDark = ThemeManager.IsDarkMode();

            // Text colors
            Color textColor = isDark ? Color.FromArgb(220, 220, 220) : Color.White;
            Color subtitleColor = isDark ? Color.FromArgb(180, 180, 180) : Color.WhiteSmoke;

            if (titleLabel != null)
                titleLabel.ForeColor = textColor;

            if (subtitleLabel != null)
                subtitleLabel.ForeColor = subtitleColor;

            if (descLabel != null)
                descLabel.ForeColor = textColor;

            // Button styling
            if (closeButton != null)
            {
                if (isDark)
                {
                    // Dark mode: Light button
                    closeButton.BackColor = Color.FromArgb(45, 45, 45);
                    closeButton.ForeColor = Color.FromArgb(220, 220, 220);
                    closeButton.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
                    closeButton.FlatAppearance.BorderSize = 1;
                }
                else
                {
                    // Light mode: White button with blue text
                    closeButton.BackColor = Color.White;
                    closeButton.ForeColor = Color.FromArgb(0, 120, 215);
                    closeButton.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
                    closeButton.FlatAppearance.BorderSize = 1;
                }
            }

            // Force redraw
            this.Invalidate();
        }
    }
}
