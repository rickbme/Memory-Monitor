using System;
using System.Drawing;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Partial class containing date/time display functionality
    /// </summary>
    public partial class MiniMonitorForm
    {
        private const float DATE_TIME_FONT_SIZE = 22f;
        
        private void InitializeDateTimeDisplay()
        {
            // Configure time label (top right - 12-hour format)
            lblTime.Font = new Font("Segoe UI", DATE_TIME_FONT_SIZE, FontStyle.Bold);
            lblTime.ForeColor = Color.White;
            lblTime.BackColor = Color.Transparent;
            lblTime.TextAlign = ContentAlignment.MiddleRight;
            lblTime.AutoSize = false;

            // Configure date label (top left - Month Day, Year)
            lblDate.Font = new Font("Segoe UI", DATE_TIME_FONT_SIZE, FontStyle.Bold);
            lblDate.ForeColor = Color.White;
            lblDate.BackColor = Color.Transparent;
            lblDate.TextAlign = ContentAlignment.MiddleLeft;
            lblDate.AutoSize = false;

            // Initial update
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            DateTime now = DateTime.Now;
            
            // 12-hour format without seconds (e.g., "3:45 PM")
            lblTime.Text = now.ToString("h:mm tt");
            
            // Month and Day only (e.g., "January 15")
            lblDate.Text = now.ToString("MMMM d");
        }

        private void LayoutDateTimeLabels()
        {
            int margin = 15;
            int labelWidth = 280;
            int labelHeight = 40;

            // Date label - top left
            lblDate.Location = new Point(margin, margin);
            lblDate.Size = new Size(labelWidth, labelHeight);

            // Time label - top right
            lblTime.Location = new Point(ClientSize.Width - labelWidth - margin, margin);
            lblTime.Size = new Size(labelWidth, labelHeight);

            // Bring to front so they're visible over any background
            lblDate.BringToFront();
            lblTime.BringToFront();
        }
    }
}
