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

        private bool _showDiskMonitor = false;
        private bool _showNetworkMonitor = false;

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
                showDiskMonitorToolStripMenuItem.Checked = _showDiskMonitor;
                showNetworkMonitorToolStripMenuItem.Checked = _showNetworkMonitor;
            }
            catch
            {
                // Use defaults if settings can't be loaded
                ThemeManager.SetTheme(ThemeManager.Theme.Light);
                darkModeToolStripMenuItem.Checked = false;
                _showDiskMonitor = false;
                _showNetworkMonitor = false;
                showDiskMonitorToolStripMenuItem.Checked = false;
                showNetworkMonitorToolStripMenuItem.Checked = false;
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

            // Set initial values for Disk (now in Mbps)
            lblDiskUsageTitle.Text = "Disk Speed";
            lblDiskUsageValue.Text = _diskMonitor?.IsAvailable == true ? "0 Mbps" : "N/A";
            
            // Configure disk gauge
            diskUsageGauge.MaxValue = 100f; // Start with 100 Mbps scale

            // Set initial values for Network (now in Mbps)
            lblNetworkUsageTitle.Text = "Network Speed";
            lblNetworkUsageValue.Text = _networkMonitor?.IsAvailable == true ? "0 Mbps" : "N/A";
            
            // Configure network gauge
            networkUsageGauge.MaxValue = 100f; // Start with 100 Mbps scale

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
            // Show/hide disk monitor controls
            lblDiskUsageTitle.Visible = _showDiskMonitor;
            lblDiskUsageValue.Visible = _showDiskMonitor;
            diskUsageGauge.Visible = _showDiskMonitor;

            // Show/hide network monitor controls
            lblNetworkUsageTitle.Visible = _showNetworkMonitor;
            lblNetworkUsageValue.Visible = _showNetworkMonitor;
            networkUsageGauge.Visible = _showNetworkMonitor;

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

            // Apply to graphs
            ApplyGraphColors(colors);

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
            // Title labels
            lblCPUUsageTitle.ForeColor = colors.TextPrimary;
            lblGPUUsageTitle.ForeColor = colors.TextPrimary;
            lblSystemMemoryTitle.ForeColor = colors.TextPrimary;
            lblGPUMemoryTitle.ForeColor = colors.TextPrimary;
            lblDiskUsageTitle.ForeColor = colors.TextPrimary;
            lblNetworkUsageTitle.ForeColor = colors.TextPrimary;
            lblProcessesTitle.ForeColor = colors.TextPrimary;

            // Value labels (keep CPU/GPU/Disk/Network colors)
            lblCPUUsageValue.ForeColor = colors.CPUColor;
            lblGPUUsageValue.ForeColor = colors.GPUColor;
            lblDiskUsageValue.ForeColor = colors.DiskColor;
            lblNetworkUsageValue.ForeColor = colors.NetworkColor;

            // Secondary labels
            lblSystemMemoryValue.ForeColor = colors.TextSecondary;
            lblSystemMemoryPercent.ForeColor = colors.TextSecondary;
            lblGPUMemoryValue.ForeColor = colors.TextSecondary;
            lblGPUMemoryPercent.ForeColor = colors.TextSecondary;
        }

        private void ApplyGraphColors((Color FormBackground, Color ControlBackground, Color TextPrimary, 
            Color TextSecondary, Color GraphBackground, Color GraphGrid, Color CPUColor, Color GPUColor, 
            Color DiskColor, Color NetworkColor,
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

        private void ApplyGaugeColors((Color FormBackground, Color ControlBackground, Color TextPrimary, 
            Color TextSecondary, Color GraphBackground, Color GraphGrid, Color CPUColor, Color GPUColor, 
            Color DiskColor, Color NetworkColor,
            Color ListViewBackground, Color ListViewText, Color ListViewGrid, Color MenuBackground, 
            Color MenuText) colors)
        {
            // Disk gauge
            diskUsageGauge.GaugeBackgroundColor = colors.GraphBackground;
            diskUsageGauge.GaugeColor = colors.DiskColor;
            diskUsageGauge.NeedleColor = colors.DiskColor;

            // Network gauge
            networkUsageGauge.GaugeBackgroundColor = colors.GraphBackground;
            networkUsageGauge.GaugeColor = colors.NetworkColor;
            networkUsageGauge.NeedleColor = colors.NetworkColor;

            // Force gauges to redraw
            diskUsageGauge.Invalidate();
            networkUsageGauge.Invalidate();
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
                    lblCPUUsageValue.Text = $"{cpuPercent}%";
                    cpuUsageGraph.AddDataPoint(cpuUsage);
                }
                else
                {
                    _lastCpuUsage = 0;
                    lblCPUUsageValue.Text = "N/A";
                    cpuUsageGraph.AddDataPoint(0);
                }
            }
            catch (Exception ex)
            {
                _lastCpuUsage = 0;
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

                    _lastGpuUsage = gpuPercent;
                    lblGPUUsageValue.Text = $"{gpuPercent}%";
                    gpuUsageGraph.AddDataPoint(gpuUsage);
                }
                else
                {
                    _lastGpuUsage = 0;
                    lblGPUUsageValue.Text = "N/A";
                    gpuUsageGraph.AddDataPoint(0);
                }
                
                // Update tray icon text after GPU update (called after CPU)
                UpdateTrayIconText();
            }
            catch (Exception ex)
            {
                _lastGpuUsage = 0;
                lblGPUUsageValue.Text = "Error";
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

                    // Update peak value
                    _diskPeakMbps = Math.Max(_diskPeakMbps, totalMbps);
                    
                    // Auto-scale the gauge
                    float newScale = GetAutoScale(totalMbps, _diskPeakMbps);
                    diskUsageGauge.MaxValue = newScale;

                    // Display current speed with scale
                    lblDiskUsageValue.Text = $"{totalMbps:F1} Mbps (Scale: {newScale:F0})";
                    
                    // Update gauge
                    diskUsageGauge.SetValue(totalMbps, $"{totalMbps:F1}");
                }
                else
                {
                    lblDiskUsageValue.Text = "N/A";
                    diskUsageGauge.SetValue(0, "0");
                }
            }
            catch (Exception ex)
            {
                lblDiskUsageValue.Text = "Error";
                diskUsageGauge.SetValue(0, "0");
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

                    // Update peak value
                    _networkPeakMbps = Math.Max(_networkPeakMbps, totalMbps);
                    
                    // Auto-scale the gauge
                    float newScale = GetAutoScale(totalMbps, _networkPeakMbps);
                    networkUsageGauge.MaxValue = newScale;

                    // Display current speed with scale
                    lblNetworkUsageValue.Text = $"{totalMbps:F1} Mbps (Scale: {newScale:F0})";
                    
                    // Update gauge
                    networkUsageGauge.SetValue(totalMbps, $"{totalMbps:F1}");
                }
                else
                {
                    lblNetworkUsageValue.Text = "N/A";
                    networkUsageGauge.SetValue(0, "0");
                }
            }
            catch (Exception ex)
            {
                lblNetworkUsageValue.Text = "Error";
                networkUsageGauge.SetValue(0, "0");
                Debug.WriteLine($"Error in UpdateNetworkUsage: {ex.Message}");
            }
        }

        private void UpdateSystemMemory()
        {
            try
            {
                // Get memory status using Windows API
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                if (!GlobalMemoryStatusEx(memStatus))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                ulong totalMemory = memStatus.ullTotalPhys;
                ulong availableMemory = memStatus.ullAvailPhys;
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
            _diskMonitor?.Dispose();
            _networkMonitor?.Dispose();

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
                
                // Determine if we show disk/network gauges
                bool showDiskMonitor = _showDiskMonitor;
                bool showNetworkMonitor = _showNetworkMonitor;
                
                // Always use 2 columns for main content
                int columnWidth = (formWidth - (margin * 3)) / 2;
                int col2X = margin + columnWidth + spacing;

                int currentY = menuHeight + margin;

                // Top section - CPU and GPU Usage (first row)
                int usageHeight = 160; // Title + Value + Graph

                // CPU Usage (left)
                lblCPUUsageTitle.Location = new Point(margin, currentY);
                lblCPUUsageValue.Location = new Point(margin, currentY + 25);
                cpuUsageGraph.Location = new Point(margin, currentY + 65);
                cpuUsageGraph.Size = new Size(columnWidth, 60);

                // GPU Usage (right)
                lblGPUUsageTitle.Location = new Point(col2X, currentY);
                lblGPUUsageValue.Location = new Point(col2X, currentY + 25);
                gpuUsageGraph.Location = new Point(col2X, currentY + 65);
                gpuUsageGraph.Size = new Size(columnWidth, 60);

                currentY += usageHeight;

                // Memory section
                int memoryHeight = 160;

                // System Memory (left)
                lblSystemMemoryTitle.Location = new Point(margin, currentY);
                lblSystemMemoryValue.Location = new Point(margin, currentY + 25);
                progressBarSystemMemory.Location = new Point(margin, currentY + 50);
                progressBarSystemMemory.Size = new Size(columnWidth - 60, 20);
                lblSystemMemoryPercent.Location = new Point(margin + columnWidth - 50, currentY + 52);
                systemMemoryGraph.Location = new Point(margin, currentY + 80);
                systemMemoryGraph.Size = new Size(columnWidth, 60);

                // GPU Memory (right)
                lblGPUMemoryTitle.Location = new Point(col2X, currentY);
                lblGPUMemoryValue.Location = new Point(col2X, currentY + 25);
                progressBarGPUMemory.Location = new Point(col2X, currentY + 50);
                progressBarGPUMemory.Size = new Size(columnWidth - 60, 20);
                lblGPUMemoryPercent.Location = new Point(col2X + columnWidth - 50, currentY + 52);
                gpuMemoryGraph.Location = new Point(col2X, currentY + 80);
                gpuMemoryGraph.Size = new Size(columnWidth, 60);

                currentY += memoryHeight;

                // Gauge section - below memory graphs
                if (showDiskMonitor || showNetworkMonitor)
                {
                    int gaugeSize = Math.Min(columnWidth - 40, 160);
                    int gaugeSectionHeight = 0;
                    
                    if (showDiskMonitor)
                    {
                        int gaugeX = margin + (columnWidth - gaugeSize) / 2;
                        
                        lblDiskUsageTitle.Location = new Point(margin, currentY);
                        lblDiskUsageValue.Location = new Point(margin, currentY + 23);
                        diskUsageGauge.Location = new Point(gaugeX, currentY + 45);
                        diskUsageGauge.Size = new Size(gaugeSize, gaugeSize);
                        
                        gaugeSectionHeight = Math.Max(gaugeSectionHeight, 45 + gaugeSize);
                    }

                    if (showNetworkMonitor)
                    {
                        int gaugeX = col2X + (columnWidth - gaugeSize) / 2;
                        
                        lblNetworkUsageTitle.Location = new Point(col2X, currentY);
                        lblNetworkUsageValue.Location = new Point(col2X, currentY + 23);
                        networkUsageGauge.Location = new Point(gaugeX, currentY + 45);
                        networkUsageGauge.Size = new Size(gaugeSize, gaugeSize);
                        
                        gaugeSectionHeight = Math.Max(gaugeSectionHeight, 45 + gaugeSize);
                    }
                    
                    currentY += gaugeSectionHeight + 20; // Add spacing after gauges
                }

                // Process list (uses full width, below gauges)
                lblProcessesTitle.Location = new Point(margin, currentY);
                
                int listViewY = currentY + 30;
                int listViewHeight = formHeight - listViewY - margin;
                int listViewWidth = formWidth - (2 * margin); // Full width
                
                listViewProcesses.Location = new Point(margin, listViewY);
                listViewProcesses.Size = new Size(listViewWidth, Math.Max(100, listViewHeight));

                // Adjust column widths for full-width list
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
