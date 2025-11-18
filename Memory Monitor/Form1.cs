using System.Diagnostics;
using System.Windows.Forms;

namespace Memory_Monitor
{
    public partial class Form1 : Form
    {
        private const long BYTES_TO_MB = 1024 * 1024;
        private const long BYTES_TO_GB = 1024 * 1024 * 1024;
        private const int MIN_MEMORY_MB = 400;

        private CPUMonitor? _cpuMonitor;
        private GPUMonitor? _gpuMonitor;

        public Form1()
        {
            InitializeComponent();
            LoadThemePreference();
            InitializeMonitors();
            InitializeUI();
            ApplyTheme(); // Apply saved or default theme
        }

        private void LoadThemePreference()
        {
            try
            {
                // Load saved theme preference
                bool darkMode = Properties.Settings.Default.DarkMode;
                ThemeManager.SetTheme(darkMode ? ThemeManager.Theme.Dark : ThemeManager.Theme.Light);
                darkModeToolStripMenuItem.Checked = darkMode;
            }
            catch
            {
                // Use default light theme if settings can't be loaded
                ThemeManager.SetTheme(ThemeManager.Theme.Light);
                darkModeToolStripMenuItem.Checked = false;
            }
        }

        private void SaveThemePreference()
        {
            try
            {
                Properties.Settings.Default.DarkMode = ThemeManager.IsDarkMode();
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save theme preference: {ex.Message}");
            }
        }

        private void InitializeMonitors()
        {
            // Initialize CPU monitor
            _cpuMonitor = new CPUMonitor();

            // Initialize GPU monitor
            _gpuMonitor = new GPUMonitor();
        }

        private void InitializeUI()
        {
            // Set initial values for system memory
            lblSystemMemoryValue.Text = "0 GB / 0 GB Used";
            lblSystemMemoryPercent.Text = "0%";
            progressBarSystemMemory.Value = 0;
            progressBarSystemMemory.Maximum = 100;

            // Set initial values for CPU usage
            lblCPUUsageValue.Text = _cpuMonitor?.IsAvailable == true ? "0%" : "N/A";

            // Set initial values for GPU
            if (_gpuMonitor != null)
            {
                // Update GPU titles with detected GPU name
                if (_gpuMonitor.IsMemoryAvailable)
                {
                    lblGPUMemoryTitle.Text = _gpuMonitor.GetShortName();
                    lblGPUMemoryValue.Text = "0 GB / 0 GB";
                    lblGPUMemoryPercent.Text = "0%";
                }
                else
                {
                    lblGPUMemoryValue.Text = "Not Available";
                    lblGPUMemoryPercent.Text = "N/A";
                }

                lblGPUUsageValue.Text = _gpuMonitor.IsUsageAvailable ? "0%" : "N/A";
            }
            else
            {
                lblGPUMemoryValue.Text = "Not Available";
                lblGPUMemoryPercent.Text = "N/A";
                lblGPUUsageValue.Text = "N/A";
            }

            progressBarGPUMemory.Value = 0;
            progressBarGPUMemory.Maximum = 100;

            // Start the update timer
            updateTimer.Start();

            // Do an initial update
            UpdateAllMetrics();
        }

        private void DarkModeToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            // Toggle theme
            ThemeManager.ToggleTheme();
            
            // Update menu item checked state
            darkModeToolStripMenuItem.Checked = ThemeManager.IsDarkMode();
            
            // Save preference
            SaveThemePreference();
            
            // Apply the new theme
            ApplyTheme();
        }

        private void AboutToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            using (AboutForm aboutForm = new AboutForm())
            {
                aboutForm.ShowDialog(this);
            }
        }

        private void ApplyTheme()
        {
            var colors = ThemeManager.GetCurrentColors();

            // Apply form background
            this.BackColor = colors.FormBackground;

            // Apply menu colors
            menuStrip.BackColor = colors.MenuBackground;
            menuStrip.ForeColor = colors.MenuText;
            viewToolStripMenuItem.ForeColor = colors.MenuText;
            darkModeToolStripMenuItem.ForeColor = colors.MenuText;

            // Apply to all labels
            ApplyLabelColors(colors);

            // Apply to graphs
            ApplyGraphColors(colors);

            // Apply to ListView
            ApplyListViewColors(colors);
        }

        private void ApplyLabelColors((Color FormBackground, Color ControlBackground, Color TextPrimary, 
            Color TextSecondary, Color GraphBackground, Color GraphGrid, Color CPUColor, Color GPUColor, 
            Color ListViewBackground, Color ListViewText, Color ListViewGrid, Color MenuBackground, 
            Color MenuText) colors)
        {
            // Title labels
            lblCPUUsageTitle.ForeColor = colors.TextPrimary;
            lblGPUUsageTitle.ForeColor = colors.TextPrimary;
            lblSystemMemoryTitle.ForeColor = colors.TextPrimary;
            lblGPUMemoryTitle.ForeColor = colors.TextPrimary;
            lblProcessesTitle.ForeColor = colors.TextPrimary;

            // Value labels (keep CPU/GPU colors)
            lblCPUUsageValue.ForeColor = colors.CPUColor;
            lblGPUUsageValue.ForeColor = colors.GPUColor;

            // Secondary labels
            lblSystemMemoryValue.ForeColor = colors.TextSecondary;
            lblSystemMemoryPercent.ForeColor = colors.TextSecondary;
            lblGPUMemoryValue.ForeColor = colors.TextSecondary;
            lblGPUMemoryPercent.ForeColor = colors.TextSecondary;
        }

        private void ApplyGraphColors((Color FormBackground, Color ControlBackground, Color TextPrimary, 
            Color TextSecondary, Color GraphBackground, Color GraphGrid, Color CPUColor, Color GPUColor, 
            Color ListViewBackground, Color ListViewText, Color ListViewGrid, Color MenuBackground, 
            Color MenuText) colors)
        {
            // CPU graph
            cpuUsageGraph.BackColor = colors.GraphBackground;
            cpuUsageGraph.LineColor = colors.CPUColor;
            
            // GPU graph
            gpuUsageGraph.BackColor = colors.GraphBackground;
            gpuUsageGraph.LineColor = colors.GPUColor;
            
            // System Memory graph
            systemMemoryGraph.BackColor = colors.GraphBackground;
            systemMemoryGraph.LineColor = colors.CPUColor;
            
            // GPU Memory graph
            gpuMemoryGraph.BackColor = colors.GraphBackground;
            gpuMemoryGraph.LineColor = colors.GPUColor;

            // Force graphs to redraw
            cpuUsageGraph.Invalidate();
            gpuUsageGraph.Invalidate();
            systemMemoryGraph.Invalidate();
            gpuMemoryGraph.Invalidate();
        }

        private void ApplyListViewColors((Color FormBackground, Color ControlBackground, Color TextPrimary, 
            Color TextSecondary, Color GraphBackground, Color GraphGrid, Color CPUColor, Color GPUColor, 
            Color ListViewBackground, Color ListViewText, Color ListViewGrid, Color MenuBackground, 
            Color MenuText) colors)
        {
            listViewProcesses.BackColor = colors.ListViewBackground;
            listViewProcesses.ForeColor = colors.ListViewText;
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateAllMetrics();
        }

        private void UpdateAllMetrics()
        {
            UpdateSystemMemory();
            UpdateCPUUsage();
            UpdateGPUUsage();
            UpdateGPUMemory();
            UpdateProcessList();
        }

        private void UpdateCPUUsage()
        {
            try
            {
                if (_cpuMonitor != null && _cpuMonitor.IsAvailable)
                {
                    float cpuUsage = _cpuMonitor.Update();
                    int cpuPercent = (int)Math.Round(cpuUsage);

                    lblCPUUsageValue.Text = $"{cpuPercent}%";
                    cpuUsageGraph.AddDataPoint(cpuUsage);
                }
                else
                {
                    lblCPUUsageValue.Text = "N/A";
                    cpuUsageGraph.AddDataPoint(0);
                }
            }
            catch (Exception ex)
            {
                lblCPUUsageValue.Text = "Error";
                Debug.WriteLine($"Error in UpdateCPUUsage: {ex.Message}");
            }
        }

        private void UpdateGPUUsage()
        {
            try
            {
                if (_gpuMonitor != null && _gpuMonitor.IsUsageAvailable)
                {
                    float gpuUsage = _gpuMonitor.UpdateUsage();
                    int gpuPercent = (int)Math.Round(gpuUsage);

                    lblGPUUsageValue.Text = $"{gpuPercent}%";
                    gpuUsageGraph.AddDataPoint(gpuUsage);
                }
                else
                {
                    lblGPUUsageValue.Text = "N/A";
                    gpuUsageGraph.AddDataPoint(0);
                }
            }
            catch (Exception ex)
            {
                lblGPUUsageValue.Text = "Error";
                Debug.WriteLine($"Error in UpdateGPUUsage: {ex.Message}");
            }
        }

        private void UpdateSystemMemory()
        {
            try
            {
                // Get total physical memory
                var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                ulong totalMemory = computerInfo.TotalPhysicalMemory;
                ulong availableMemory = computerInfo.AvailablePhysicalMemory;
                ulong usedMemory = totalMemory - availableMemory;

                // Convert to GB
                double totalGB = totalMemory / (double)BYTES_TO_GB;
                double usedGB = usedMemory / (double)BYTES_TO_GB;

                // Calculate percentage
                int percentage = (int)((usedMemory * 100) / totalMemory);

                // Update UI
                lblSystemMemoryValue.Text = $"{usedGB:F2} GB / {totalGB:F2} GB Used";
                lblSystemMemoryPercent.Text = $"{percentage}%";
                progressBarSystemMemory.Value = Math.Min(percentage, 100);

                // Update graph
                systemMemoryGraph.AddDataPoint(percentage);
            }
            catch (Exception ex)
            {
                lblSystemMemoryValue.Text = $"Error: {ex.Message}";
                Debug.WriteLine($"Error in UpdateSystemMemory: {ex.Message}");
            }
        }

        private void UpdateGPUMemory()
        {
            try
            {
                if (_gpuMonitor != null && _gpuMonitor.IsMemoryAvailable)
                {
                    _gpuMonitor.UpdateMemory();

                    double totalGB = _gpuMonitor.TotalMemoryGB;
                    double usedGB = _gpuMonitor.UsedMemoryGB;
                    int percentage = _gpuMonitor.MemoryUsagePercent;

                    // Update UI
                    lblGPUMemoryValue.Text = $"{usedGB:F2} GB / {totalGB:F2} GB";
                    lblGPUMemoryPercent.Text = $"{percentage}%";
                    progressBarGPUMemory.Value = percentage;

                    // Update graph
                    gpuMemoryGraph.AddDataPoint(percentage);
                }
                else
                {
                    lblGPUMemoryValue.Text = "Not Available";
                    lblGPUMemoryPercent.Text = "N/A";
                    progressBarGPUMemory.Value = 0;
                    gpuMemoryGraph.AddDataPoint(0);
                }
            }
            catch (Exception ex)
            {
                lblGPUMemoryValue.Text = "Error";
                Debug.WriteLine($"Error in UpdateGPUMemory: {ex.Message}");
            }
        }

        private void UpdateProcessList()
        {
            try
            {
                // Get all processes using more than MIN_MEMORY_MB
                var processes = Process.GetProcesses()
                    .Where(p => {
                        try
                        {
                            return p.WorkingSet64 / BYTES_TO_MB > MIN_MEMORY_MB;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .OrderByDescending(p => p.WorkingSet64)
                    .ToList();

                // Update ListView
                listViewProcesses.BeginUpdate();
                listViewProcesses.Items.Clear();

                foreach (var process in processes)
                {
                    try
                    {
                        long memoryMB = process.WorkingSet64 / BYTES_TO_MB;
                        double memoryGB = process.WorkingSet64 / (double)BYTES_TO_GB;

                        var item = new ListViewItem(process.ProcessName);
                        item.SubItems.Add($"{memoryGB:F2} GB");
                        item.SubItems.Add($"{memoryMB:N0}");

                        listViewProcesses.Items.Add(item);
                    }
                    catch
                    {
                        // Skip processes we can't access
                    }
                }

                listViewProcesses.EndUpdate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating process list: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer.Stop();

            // Dispose monitors
            _cpuMonitor?.Dispose();
            _gpuMonitor?.Dispose();

            base.OnFormClosing(e);
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            // Suspend layout updates during resize
            this.SuspendLayout();

            try
            {
                // Calculate available space
                int formWidth = this.ClientSize.Width;
                int formHeight = this.ClientSize.Height;
                
                const int margin = 20;
                const int spacing = 20;
                const int menuHeight = 24;
                
                // Calculate column widths (50% each with margin)
                int columnWidth = (formWidth - (3 * margin)) / 2;
                int rightColumnX = margin + columnWidth + spacing;

                // Top section - CPU and GPU Usage (first row)
                int topY = menuHeight + margin;
                int usageHeight = 160; // Title + Value + Graph

                // CPU Usage (left)
                lblCPUUsageTitle.Location = new Point(margin, topY);
                lblCPUUsageValue.Location = new Point(margin, topY + 25);
                cpuUsageGraph.Location = new Point(margin, topY + 65);
                cpuUsageGraph.Size = new Size(columnWidth, 60);

                // GPU Usage (right)
                lblGPUUsageTitle.Location = new Point(rightColumnX, topY);
                lblGPUUsageValue.Location = new Point(rightColumnX, topY + 25);
                gpuUsageGraph.Location = new Point(rightColumnX, topY + 65);
                gpuUsageGraph.Size = new Size(columnWidth, 60);

                // Memory section (second row)
                int memoryY = topY + usageHeight;

                // System Memory (left)
                lblSystemMemoryTitle.Location = new Point(margin, memoryY);
                lblSystemMemoryValue.Location = new Point(margin, memoryY + 25);
                progressBarSystemMemory.Location = new Point(margin, memoryY + 45);
                progressBarSystemMemory.Size = new Size(columnWidth - 60, 20);
                lblSystemMemoryPercent.Location = new Point(margin + columnWidth - 50, memoryY + 47);
                systemMemoryGraph.Location = new Point(margin, memoryY + 70);
                systemMemoryGraph.Size = new Size(columnWidth, 60);

                // GPU Memory (right)
                lblGPUMemoryTitle.Location = new Point(rightColumnX, memoryY);
                lblGPUMemoryValue.Location = new Point(rightColumnX, memoryY + 25);
                progressBarGPUMemory.Location = new Point(rightColumnX, memoryY + 45);
                progressBarGPUMemory.Size = new Size(columnWidth - 60, 20);
                lblGPUMemoryPercent.Location = new Point(rightColumnX + columnWidth - 50, memoryY + 47);
                gpuMemoryGraph.Location = new Point(rightColumnX, memoryY + 70);
                gpuMemoryGraph.Size = new Size(columnWidth, 60);

                // Process list (bottom section)
                int processY = memoryY + 150;
                lblProcessesTitle.Location = new Point(margin, processY);
                
                int listViewY = processY + 30;
                int listViewHeight = formHeight - listViewY - margin;
                listViewProcesses.Location = new Point(margin, listViewY);
                listViewProcesses.Size = new Size(formWidth - (2 * margin), Math.Max(100, listViewHeight));

                // Adjust column widths
                if (listViewProcesses.Columns.Count >= 3)
                {
                    int totalWidth = listViewProcesses.ClientSize.Width - 4; // Subtract scrollbar space
                    listViewProcesses.Columns[0].Width = (int)(totalWidth * 0.45); // Process name
                    listViewProcesses.Columns[1].Width = (int)(totalWidth * 0.30); // Memory
                    listViewProcesses.Columns[2].Width = (int)(totalWidth * 0.25); // MB
                }
            }
            finally
            {
                this.ResumeLayout(true);
            }
        }

        #region ListView Custom Drawing

        private void ListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            // Draw column headers with theme colors
            var colors = ThemeManager.GetCurrentColors();
            
            using (SolidBrush backBrush = new SolidBrush(colors.ControlBackground))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            // Draw header text
            TextRenderer.DrawText(e.Graphics, e.Header?.Text ?? "", 
                listViewProcesses.Font, e.Bounds, colors.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // Draw bottom border
            using (Pen borderPen = new Pen(colors.GraphGrid))
            {
                e.Graphics.DrawLine(borderPen, e.Bounds.Left, e.Bounds.Bottom - 1, 
                    e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        private void ListView_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            // Let DrawSubItem handle the rendering
            e.DrawDefault = false;
        }

        private void ListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            var colors = ThemeManager.GetCurrentColors();

            // Alternate row colors for better readability
            Color backColor;
            if (e.ItemIndex % 2 == 0)
            {
                backColor = colors.ListViewBackground;
            }
            else
            {
                // Slightly different shade for alternate rows
                if (ThemeManager.IsDarkMode())
                    backColor = Color.FromArgb(40, 40, 40);
                else
                    backColor = Color.FromArgb(248, 248, 248);
            }

            // Highlight selected item
            if (e.Item.Selected)
            {
                backColor = ThemeManager.IsDarkMode() 
                    ? Color.FromArgb(60, 60, 60) 
                    : Color.FromArgb(220, 230, 240);
            }

            // Fill background
            using (SolidBrush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            // Draw marker for first column (process name)
            if (e.ColumnIndex == 0)
            {
                // Draw small circular marker
                int markerSize = 6;
                int markerX = e.Bounds.Left + 8;
                int markerY = e.Bounds.Top + (e.Bounds.Height - markerSize) / 2;
                
                Rectangle markerRect = new Rectangle(markerX, markerY, markerSize, markerSize);
                
                // Use accent color for marker
                Color markerColor = colors.CPUColor;
                using (SolidBrush markerBrush = new SolidBrush(markerColor))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(markerBrush, markerRect);
                }

                // Draw text with offset for marker
                Rectangle textBounds = new Rectangle(
                    e.Bounds.Left + 20, 
                    e.Bounds.Top, 
                    e.Bounds.Width - 20, 
                    e.Bounds.Height);

                TextRenderer.DrawText(e.Graphics, e.SubItem?.Text ?? "", 
                    listViewProcesses.Font, textBounds, colors.ListViewText,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
            else
            {
                // Draw text for other columns
                TextFormatFlags flags = e.ColumnIndex == 2 
                    ? TextFormatFlags.Right | TextFormatFlags.VerticalCenter 
                    : TextFormatFlags.Left | TextFormatFlags.VerticalCenter;

                TextRenderer.DrawText(e.Graphics, e.SubItem?.Text ?? "", 
                    listViewProcesses.Font, e.Bounds, colors.ListViewText,
                    flags | TextFormatFlags.EndEllipsis);
            }
        }

        #endregion
    }
}
