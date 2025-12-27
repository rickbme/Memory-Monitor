using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Compact popup form for selecting a device from available options
    /// </summary>
    public class DeviceSelectionForm : Form
    {
        private readonly IReadOnlyList<DeviceInfo> _devices;
        private readonly string? _currentDeviceId;
        private readonly Panel _deviceListPanel;
        private readonly Label _titleLabel;

        /// <summary>
        /// Gets the selected device ID, or null if cancelled or aggregate selected
        /// </summary>
        public string? SelectedDeviceId { get; private set; }

        /// <summary>
        /// Gets whether a device was selected (vs cancelled)
        /// </summary>
        public bool DeviceSelected { get; private set; }

        public DeviceSelectionForm(IReadOnlyList<DeviceInfo> devices, string? currentDeviceId, string title)
        {
            _devices = devices;
            _currentDeviceId = currentDeviceId;

            // Form settings for popup appearance
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            BackColor = Color.FromArgb(35, 38, 42);
            Padding = new Padding(2);
            
            // Auto-size based on content
            AutoSize = false;
            
            // Calculate size based on device count
            int itemHeight = 40;
            int titleHeight = 35;
            int padding = 10;
            int width = 280;
            int contentHeight = titleHeight + (_devices.Count * itemHeight) + padding * 2;
            Size = new Size(width, Math.Min(contentHeight, 400));

            // Title label
            _titleLabel = new Label
            {
                Text = title,
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = titleHeight,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(45, 48, 52)
            };
            Controls.Add(_titleLabel);

            // Device list panel with scrolling
            _deviceListPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(5)
            };
            Controls.Add(_deviceListPanel);

            // Add device buttons
            CreateDeviceButtons();

            // Handle deactivation (click outside)
            Deactivate += (s, e) => 
            {
                if (!DeviceSelected)
                    Close();
            };

            // Handle escape key
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    Close();
            };

            // Draw border
            Paint += DeviceSelectionForm_Paint;
        }

        private void CreateDeviceButtons()
        {
            int y = 5;
            int buttonHeight = 36;
            int buttonWidth = _deviceListPanel.Width - 25;

            foreach (var device in _devices)
            {
                var button = CreateDeviceButton(device, buttonWidth, buttonHeight);
                button.Location = new Point(5, y);
                _deviceListPanel.Controls.Add(button);
                y += buttonHeight + 4;
            }
        }

        private Button CreateDeviceButton(DeviceInfo device, int width, int height)
        {
            bool isCurrent = device.Id == _currentDeviceId || 
                             (string.IsNullOrEmpty(_currentDeviceId) && device.Type == DeviceType.Aggregate);

            var button = new Button
            {
                Width = width,
                Height = height,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 10, 0),
                Font = new Font("Segoe UI", 9.5f, isCurrent ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = isCurrent ? Color.FromArgb(100, 180, 255) : Color.White,
                BackColor = isCurrent ? Color.FromArgb(50, 55, 65) : Color.FromArgb(40, 43, 47),
                Cursor = Cursors.Hand,
                Tag = device
            };

            // Format button text
            string displayText = device.DisplayName;
            if (!string.IsNullOrEmpty(device.Description))
            {
                displayText = $"{device.DisplayName}\n{device.Description}";
                button.Font = new Font("Segoe UI", 8.5f, isCurrent ? FontStyle.Bold : FontStyle.Regular);
            }

            // Add checkmark for current device
            if (isCurrent)
                displayText = "? " + displayText;

            button.Text = displayText;

            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = isCurrent 
                ? Color.FromArgb(100, 180, 255) 
                : Color.FromArgb(60, 63, 67);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 60, 70);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(65, 70, 80);

            button.Click += (s, e) =>
            {
                SelectedDeviceId = device.Type == DeviceType.Aggregate ? null : device.Id;
                DeviceSelected = true;
                DialogResult = DialogResult.OK;
                Close();
            };

            return button;
        }

        private void DeviceSelectionForm_Paint(object? sender, PaintEventArgs e)
        {
            // Draw border around the form
            using var pen = new Pen(Color.FromArgb(80, 85, 90), 2);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        /// <summary>
        /// Shows the device selection popup centered over the specified control
        /// </summary>
        public static (bool selected, string? deviceId) ShowNear(
            Control anchor, 
            IReadOnlyList<DeviceInfo> devices, 
            string? currentDeviceId, 
            string title)
        {
            if (devices.Count <= 1)
                return (false, currentDeviceId);

            using var form = new DeviceSelectionForm(devices, currentDeviceId, title);

            // Get the parent form to find the correct screen
            Form? parentForm = anchor.FindForm();
            
            // Get the center of the anchor control in screen coordinates
            Point anchorCenter = anchor.PointToScreen(new Point(anchor.Width / 2, anchor.Height / 2));
            
            // Center the popup over the anchor control
            int x = anchorCenter.X - form.Width / 2;
            int y = anchorCenter.Y - form.Height / 2;

            // Get the screen that contains the anchor control
            Screen screen = Screen.FromControl(anchor);
            Rectangle screenBounds = screen.Bounds;

            // Ensure the popup stays within the screen bounds
            // Left edge
            if (x < screenBounds.Left)
                x = screenBounds.Left + 5;
            
            // Right edge
            if (x + form.Width > screenBounds.Right)
                x = screenBounds.Right - form.Width - 5;
            
            // Top edge
            if (y < screenBounds.Top)
                y = screenBounds.Top + 5;
            
            // Bottom edge
            if (y + form.Height > screenBounds.Bottom)
                y = screenBounds.Bottom - form.Height - 5;

            form.Location = new Point(x, y);
            form.ShowDialog(parentForm);

            return (form.DeviceSelected, form.SelectedDeviceId);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Add drop shadow
                var cp = base.CreateParams;
                cp.ClassStyle |= 0x20000; // CS_DROPSHADOW
                return cp;
            }
        }
    }
}
