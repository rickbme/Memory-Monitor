using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Memory_Monitor
{
    public partial class Form1 : Form
    {
        private const long BYTES_TO_MB = 1024 * 1024;
        private const long BYTES_TO_GB = 1024 * 1024 * 1024;
        private const int MIN_MEMORY_MB = 400;

        // Auto-scaling thresholds for gauges (in Mbps)
        private static readonly float[] SCALE_OPTIONS = { 10f, 25f, 50f, 100f, 250f, 500f, 1000f, 2500f, 5000f, 10000f };

        private CPUMonitor? _cpuMonitor;
        private GPUMonitor? _gpuMonitor;
        private DiskMonitor? _diskMonitor;
        private NetworkMonitor? _networkMonitor;

        private bool _showDiskMonitor = true; // Changed to true by default
        private bool _showNetworkMonitor = true; // Changed to true by default

        // Track peak values for auto-scaling
        private float _diskPeakMbps = 0;
        private float _networkPeakMbps = 0;

        // Track CPU and GPU values for tray icon
        private int _lastCpuUsage = 0;
        private int _lastGpuUsage = 0;

        // Windows API for memory info
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public Form1()
        {
            InitializeComponent();
            LoadSettings();
            InitializeMonitors();
            InitializeUI();
            InitializeTrayIcon();
            ApplyTheme(); // Apply saved or default theme
        }

        private void InitializeTrayIcon()
        {
            // Use form icon if available, otherwise use system application icon
            if (this.Icon != null)
            {
                notifyIcon.Icon = this.Icon;
            }
            else
            {
                notifyIcon.Icon = SystemIcons.Application;
            }
            
            // Set initial tray icon text
            UpdateTrayIconText();
            
            // Set up form to minimize to tray
            this.Resize += Form1_ResizeMinimizeToTray;
        }

        private void Form1_ResizeMinimizeToTray(object? sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            ShowForm();
        }

        private void ShowToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ShowForm();
        }

        private void ExitToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ShowForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private void UpdateTrayIconText()
        {
            // Format: "CPU: 45 GPU: 32"
            string trayText = $"CPU: {_lastCpuUsage} GPU: {_lastGpuUsage}";
            
            // Tooltip text is limited to 63 characters
            if (trayText.Length > 63)
            {
                trayText = trayText.Substring(0, 63);
            }
            
            notifyIcon.Text = trayText;
        }

        private void LoadSettings()
        {
            try
            {
                // Load saved theme preference
                bool darkMode = Properties.Settings.Default.DarkMode;
                ThemeManager.SetTheme(darkMode ? ThemeManager.Theme.Dark : ThemeManager.Theme.Light);
                darkModeToolStripMenuItem.Checked = darkMode;

                // Load monitor visibility preferences
                _showDiskMonitor = Properties.Settings.Default.ShowDiskMonitor;
                _showNetworkMonitor = Properties.Settings.Default.ShowNetworkMonitor;
                
                // Default to true if never set
                if (!_showDiskMonitor && !_showNetworkMonitor && 
                    Properties.Settings.Default.ShowDiskMonitor == false && 
                    Properties.Settings.Default.ShowNetworkMonitor == false)
                {
                    _showDiskMonitor = true;
                    _showNetworkMonitor = true;
                }
                
                showDiskMonitorToolStripMenuItem.Checked = _showDiskMonitor;
                showNetworkMonitorToolStripMenuItem.Checked = _showNetworkMonitor;
            }
            catch
            {
                // Use defaults if settings can't be loaded
                ThemeManager.SetTheme(ThemeManager.Theme.Light);
                darkModeToolStripMenuItem.Checked = false;
                _showDiskMonitor = true;
                _showNetworkMonitor = true;
                showDiskMonitorToolStripMenuItem.Checked = true;
                showNetworkMonitorToolStripMenuItem.Checked = true;
            }
        }

        private void SaveSettings()
        {
            try
            {
                Properties.Settings.Default.DarkMode = ThemeManager.IsDarkMode();
                Properties.Settings.Default.ShowDiskMonitor = _showDiskMonitor;
                Properties.Settings.Default.ShowNetworkMonitor = _showNetworkMonitor;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        private void InitializeMonitors()
        {
            // Initialize CPU monitor
            _cpuMonitor = new CPUMonitor();

            // Initialize GPU monitor
            _gpuMonitor = new GPUMonitor();

            // Initialize Disk monitor
            _diskMonitor = new DiskMonitor();

            // Initialize Network monitor
            _networkMonitor = new NetworkMonitor();
        }

        private void InitializeUI()
        {
            // Initialize gauges
            ramUsageGauge.MaxValue = 100f;
            cpuLoadGauge.MaxValue = 100f;
            diskUsageGauge.MaxValue = 100f;
            networkUsageGauge.MaxValue = 100f;
            
            // Set GPU VRAM max based on detected GPU
            if (_gpuMonitor != null && _gpuMonitor.IsMemoryAvailable)
            {
                gpuVramGauge.MaxValue = (float)Math.Ceiling(_gpuMonitor.TotalMemoryGB);
            }
            else
            {
                gpuVramGauge.MaxValue = 24f; // Default max
            }

            // Apply visibility settings
            UpdateMonitorVisibility();

            // Start the update timer
            updateTimer.Start();

            // Do an initial update
            UpdateAllMetrics();
        }

        /// <summary>
        /// Determines the appropriate scale for a gauge based on current and peak values
        /// </summary>
        private float GetAutoScale(float currentValue, float peakValue)
        {
            // Use the higher of current or peak for scale selection
            float targetValue = Math.Max(currentValue, peakValue);
            
            // Find the smallest scale that can accommodate the value with some headroom
            // Scale up when value exceeds 95% of current scale
            foreach (float scale in SCALE_OPTIONS)
            {
                if (targetValue <= scale * 0.95f) // Use 95% of scale as threshold
                {
                    return scale;
                }
            }
            
            // If value exceeds all scales, use the largest
            return SCALE_OPTIONS[^1];
        }

        private void UpdateMonitorVisibility()
        {
            // Show/hide gauges and labels based on settings
            diskUsageGauge.Visible = _showDiskMonitor;
            networkUsageGauge.Visible = _showNetworkMonitor;
            
            lblDiskGauge.Visible = _showDiskMonitor;
            lblNetworkGauge.Visible = _showNetworkMonitor;

            // Trigger resize to recalculate layout
            Form1_Resize(this, EventArgs.Empty);
        }

        private void DarkModeToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            // Toggle theme
            ThemeManager.ToggleTheme();
            
            // Update menu item checked state
            darkModeToolStripMenuItem.Checked = ThemeManager.IsDarkMode();
            
            // Save preference
            SaveSettings();
            
            // Apply the new theme
            ApplyTheme();
        }

        private void ShowDiskMonitorToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            _showDiskMonitor = showDiskMonitorToolStripMenuItem.Checked;
            SaveSettings();
            UpdateMonitorVisibility();
        }

        private void ShowNetworkMonitorToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            _showNetworkMonitor = showNetworkMonitorToolStripMenuItem.Checked;
            SaveSettings();
            UpdateMonitorVisibility();
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

            // Apply to gauges
            ApplyGaugeColors(colors);

            // Apply to ListView
            ApplyListViewColors(colors);
        }

        private void ApplyLabelColors((Color FormBackground, Color ControlBackground, Color TextPrimary, 
            Color TextSecondary, Color GraphBackground, Color GraphGrid, Color CPUColor, Color GPUColor, 
            Color DiskColor, Color NetworkColor,
            Color ListViewBackground, Color ListViewText, Color ListViewGrid, Color MenuBackground, 
            Color MenuText) colors)
        {
            // Gauge labels below dials
            lblRamGauge.ForeColor = colors.TextPrimary;
            lblCpuGauge.ForeColor = colors.TextPrimary;
            lblDiskGauge.ForeColor = colors.TextPrimary;
            lblNetworkGauge.ForeColor = colors.TextPrimary;
            lblGpuGauge.ForeColor = colors.TextPrimary;
            lblProcessesTitle.ForeColor = colors.TextPrimary;
        }

        private void ApplyGaugeColors((Color FormBackground, Color ControlBackground, Color TextPrimary, 
            Color TextSecondary, Color GraphBackground, Color GraphGrid, Color CPUColor, Color GPUColor, 
            Color DiskColor, Color NetworkColor,
            Color ListViewBackground, Color ListViewText, Color ListViewGrid, Color MenuBackground, 
            Color MenuText) colors)
        {
            // Apply colors to enhanced gauges
            ramUsageGauge.GaugeColor = Color.FromArgb(58, 150, 221); // Blue
            ramUsageGauge.NeedleColor = Color.FromArgb(58, 150, 221);
            ramUsageGauge.TextColor = colors.TextPrimary;
            ramUsageGauge.ScaleTextColor = Color.FromArgb(180, 180, 180);

            cpuLoadGauge.GaugeColor = Color.FromArgb(220, 50, 50); // Red
            cpuLoadGauge.NeedleColor = Color.FromArgb(220, 50, 50);
            cpuLoadGauge.TextColor = colors.TextPrimary;
            cpuLoadGauge.ScaleTextColor = Color.FromArgb(180, 180, 180);

            diskUsageGauge.GaugeColor = Color.FromArgb(50, 200, 80); // Green
            diskUsageGauge.NeedleColor = Color.FromArgb(50, 200, 80);
            diskUsageGauge.TextColor = colors.TextPrimary;
            diskUsageGauge.ScaleTextColor = Color.FromArgb(180, 180, 180);

            networkUsageGauge.GaugeColor = Color.FromArgb(255, 200, 50); // Yellow/Gold
            networkUsageGauge.NeedleColor = Color.FromArgb(255, 200, 50);
            networkUsageGauge.TextColor = colors.TextPrimary;
            networkUsageGauge.ScaleTextColor = Color.FromArgb(180, 180, 180);

            gpuVramGauge.GaugeColor = Color.FromArgb(160, 90, 240); // Purple
            gpuVramGauge.NeedleColor = Color.FromArgb(160, 90, 240);
            gpuVramGauge.TextColor = colors.TextPrimary;
            gpuVramGauge.ScaleTextColor = Color.FromArgb(180, 180, 180);

            ramUsageGauge.Invalidate();
            cpuLoadGauge.Invalidate();
            diskUsageGauge.Invalidate();
            networkUsageGauge.Invalidate();
            gpuVramGauge.Invalidate();
        }

        private void ApplyListViewColors((Color FormBackground, Color ControlBackground, Color TextPrimary, 
            Color TextSecondary, Color GraphBackground, Color GraphGrid, Color CPUColor, Color GPUColor, 
            Color DiskColor, Color NetworkColor,
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
            
            if (_showDiskMonitor)
                UpdateDiskUsage();
            
            if (_showNetworkMonitor)
                UpdateNetworkUsage();
            
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

                    _lastCpuUsage = cpuPercent;
                    
                    // Update gauge
                    cpuLoadGauge.SetValue(cpuUsage, $"{cpuPercent}");
                }
                else
                {
                    _lastCpuUsage = 0;
                    cpuLoadGauge.SetValue(0, "N/A");
                }
            }
            catch (Exception ex)
            {
                _lastCpuUsage = 0;
                cpuLoadGauge.SetValue(0, "ERR");
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

                    _lastGpuUsage = gpuPercent;
                }
                else
                {
                    _lastGpuUsage = 0;
                }
                
                UpdateTrayIconText();
            }
            catch (Exception ex)
            {
                _lastGpuUsage = 0;
                Debug.WriteLine($"Error in UpdateGPUUsage: {ex.Message}");
            }
        }

        private void UpdateDiskUsage()
        {
            try
            {
                if (_diskMonitor != null && _diskMonitor.IsAvailable)
                {
                    var (readMbps, writeMbps, totalMbps) = _diskMonitor.Update();

                    _diskPeakMbps = Math.Max(_diskPeakMbps, totalMbps);
                    
                    float newScale = GetAutoScale(totalMbps, _diskPeakMbps);
                    diskUsageGauge.MaxValue = newScale;
                    
                    diskUsageGauge.SetValue(totalMbps, $"{totalMbps:F0}");
                }
                else
                {
                    diskUsageGauge.SetValue(0, "N/A");
                }
            }
            catch (Exception ex)
            {
                diskUsageGauge.SetValue(0, "ERR");
                Debug.WriteLine($"Error in UpdateDiskUsage: {ex.Message}");
            }
        }

        private void UpdateNetworkUsage()
        {
            try
            {
                if (_networkMonitor != null && _networkMonitor.IsAvailable)
                {
                    var (uploadMbps, downloadMbps, totalMbps) = _networkMonitor.Update();

                    _networkPeakMbps = Math.Max(_networkPeakMbps, totalMbps);
                    
                    float newScale = GetAutoScale(totalMbps, _networkPeakMbps);
                    networkUsageGauge.MaxValue = newScale;
                    
                    networkUsageGauge.SetValue(totalMbps, $"{totalMbps:F0}");
                }
                else
                {
                    networkUsageGauge.SetValue(0, "N/A");
                }
            }
            catch (Exception ex)
            {
                networkUsageGauge.SetValue(0, "ERR");
                Debug.WriteLine($"Error in UpdateNetworkUsage: {ex.Message}");
            }
        }

        private void UpdateSystemMemory()
        {
            try
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                if (!GlobalMemoryStatusEx(memStatus))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                ulong totalMemory = memStatus.ullTotalPhys;
                ulong availableMemory = memStatus.ullAvailPhys;
                ulong usedMemory = totalMemory - availableMemory;

                double totalGB = totalMemory / (double)BYTES_TO_GB;
                double usedGB = usedMemory / (double)BYTES_TO_GB;

                int percentage = (int)((usedMemory * 100) / totalMemory);
                
                // Update RAM gauge
                ramUsageGauge.SetValue(percentage, $"{usedGB:F1}/{totalGB:F0}");
            }
            catch (Exception ex)
            {
                ramUsageGauge.SetValue(0, "ERR");
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
                    
                    // Update GPU VRAM gauge
                    gpuVramGauge.SetValue((float)usedGB, $"{usedGB:F1}/{totalGB:F0}");
                }
                else
                {
                    gpuVramGauge.SetValue(0, "N/A");
                }
            }
            catch (Exception ex)
            {
                gpuVramGauge.SetValue(0, "ERR");
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
            _diskMonitor?.Dispose();
            _networkMonitor?.Dispose();

            base.OnFormClosing(e);
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            this.SuspendLayout();

            try
            {
                int formWidth = this.ClientSize.Width;
                int formHeight = this.ClientSize.Height;
                
                const int margin = 20;
                const int spacing = 20;
                const int menuHeight = 24;
                const int gaugeSize = 200;
                const int labelHeight = 25;
                const int labelSpacing = 10;
                
                int currentY = menuHeight + margin;

                // Top row: 3 gauges (RAM, CPU, Disk)
                int topRowY = currentY;
                int gauge1X = (formWidth - (gaugeSize * 3 + spacing * 2)) / 2;
                
                // RAM gauge and label
                ramUsageGauge.Location = new Point(gauge1X, topRowY);
                ramUsageGauge.Size = new Size(gaugeSize, gaugeSize);
                lblRamGauge.Location = new Point(gauge1X, topRowY + gaugeSize + labelSpacing);
                lblRamGauge.Size = new Size(gaugeSize, labelHeight);
                
                // CPU gauge and label
                cpuLoadGauge.Location = new Point(gauge1X + gaugeSize + spacing, topRowY);
                cpuLoadGauge.Size = new Size(gaugeSize, gaugeSize);
                lblCpuGauge.Location = new Point(gauge1X + gaugeSize + spacing, topRowY + gaugeSize + labelSpacing);
                lblCpuGauge.Size = new Size(gaugeSize, labelHeight);
                
                // Disk gauge and label
                diskUsageGauge.Location = new Point(gauge1X + (gaugeSize + spacing) * 2, topRowY);
                diskUsageGauge.Size = new Size(gaugeSize, gaugeSize);
                lblDiskGauge.Location = new Point(gauge1X + (gaugeSize + spacing) * 2, topRowY + gaugeSize + labelSpacing);
                lblDiskGauge.Size = new Size(gaugeSize, labelHeight);

                // Bottom row: 2 gauges (Network, GPU VRAM) - centered
                int bottomRowY = topRowY + gaugeSize + labelHeight + labelSpacing + spacing + 20;
                int gauge2X = (formWidth - (gaugeSize * 2 + spacing)) / 2;
                
                // Network gauge and label
                networkUsageGauge.Location = new Point(gauge2X, bottomRowY);
                networkUsageGauge.Size = new Size(gaugeSize, gaugeSize);
                lblNetworkGauge.Location = new Point(gauge2X, bottomRowY + gaugeSize + labelSpacing);
                lblNetworkGauge.Size = new Size(gaugeSize, labelHeight);
                
                // GPU VRAM gauge and label
                gpuVramGauge.Location = new Point(gauge2X + gaugeSize + spacing, bottomRowY);
                gpuVramGauge.Size = new Size(gaugeSize, gaugeSize);
                lblGpuGauge.Location = new Point(gauge2X + gaugeSize + spacing, bottomRowY + gaugeSize + labelSpacing);
                lblGpuGauge.Size = new Size(gaugeSize, labelHeight);

                // Process list below gauges
                currentY = bottomRowY + gaugeSize + labelHeight + labelSpacing + spacing + 20;
                
                lblProcessesTitle.Location = new Point(margin, currentY);
                
                int listViewY = currentY + 30;
                int listViewHeight = formHeight - listViewY - margin;
                int listViewWidth = formWidth - (2 * margin);
                
                listViewProcesses.Location = new Point(margin, listViewY);
                listViewProcesses.Size = new Size(listViewWidth, Math.Max(100, listViewHeight));

                if (listViewProcesses.Columns.Count >= 3)
                {
                    int totalWidth = listViewProcesses.ClientSize.Width - 4;
                    listViewProcesses.Columns[0].Width = (int)(totalWidth * 0.45);
                    listViewProcesses.Columns[1].Width = (int)(totalWidth * 0.30);
                    listViewProcesses.Columns[2].Width = (int)(totalWidth * 0.25);
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
            if (e.Item?.Selected == true)
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

            // Get text to display (with null safety)
            string text = e.SubItem?.Text ?? string.Empty;

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

                TextRenderer.DrawText(e.Graphics, text, 
                    listViewProcesses.Font, textBounds, colors.ListViewText,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
            else
            {
                // Draw text for other columns
                TextFormatFlags flags = e.ColumnIndex == 2 
                    ? TextFormatFlags.Right | TextFormatFlags.VerticalCenter 
                    : TextFormatFlags.Left | TextFormatFlags.VerticalCenter;

                TextRenderer.DrawText(e.Graphics, text, 
                    listViewProcesses.Font, e.Bounds, colors.ListViewText,
                    flags | TextFormatFlags.EndEllipsis);
            }
        }

        #endregion
    }
}
