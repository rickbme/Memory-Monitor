using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Memory_Monitor
{
    /// <summary>
    /// Main form optimized for 1920x480 mini monitor display
    /// </summary>
    public partial class MiniMonitorForm : Form
    {
        private const long BYTES_TO_MB = 1024 * 1024;
        private const long BYTES_TO_GB = 1024 * 1024 * 1024;

        private static readonly float[] SCALE_OPTIONS = { 10f, 25f, 50f, 100f, 250f, 500f, 1000f, 2500f, 5000f, 10000f };

        private CPUMonitor? _cpuMonitor;
        private GPUMonitor? _gpuMonitor;
        private DiskMonitor? _diskMonitor;
        private NetworkMonitor? _networkMonitor;

        private float _diskPeakMbps = 0;
        private float _networkPeakMbps = 0;
        private int _lastCpuUsage = 0;
        private int _lastGpuUsage = 0;

        // CPU temperature warning
        private bool _cpuTempWarningShown = false;
        private int _cpuTempCheckCount = 0;
        private const int CPU_TEMP_CHECK_THRESHOLD = 5; // Check for 5 seconds before showing warning

        #region Windows API

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

            public MEMORYSTATUSEX() { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        #endregion

        public MiniMonitorForm()
        {
            InitializeComponent();
            InitializeMonitors();
            InitializeUI();
            InitializeTrayIcon();
            ApplyTheme();
        }

        private void InitializeMonitors()
        {
            _cpuMonitor = new CPUMonitor();
            _gpuMonitor = new GPUMonitor();
            _diskMonitor = new DiskMonitor();
            _networkMonitor = new NetworkMonitor();
        }

        private void InitializeUI()
        {
            // Set gauge max values
            ramGauge.MaxValue = 100f;
            cpuGauge.MaxValue = 100f;
            diskGauge.MaxValue = 100f;
            networkGauge.MaxValue = 100f;

            if (_gpuMonitor != null && _gpuMonitor.IsMemoryAvailable)
                gpuVramGauge.MaxValue = (float)Math.Ceiling(_gpuMonitor.TotalMemoryGB);
            else
                gpuVramGauge.MaxValue = 24f;

            if (_gpuMonitor != null && _gpuMonitor.IsUsageAvailable)
                gpuUsageGauge.MaxValue = 100f;
            else
                gpuUsageGauge.MaxValue = 100f;

            updateTimer.Start();
            UpdateAllMetrics();
        }

        private void InitializeTrayIcon()
        {
            notifyIcon.Icon = this.Icon ?? SystemIcons.Application;
            UpdateTrayIconText();
            this.Resize += (s, e) =>
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    Hide();
                    ShowInTaskbar = false;
                }
            };
        }

        private void UpdateTrayIconText()
        {
            string text = $"CPU: {_lastCpuUsage}% GPU: {_lastGpuUsage}%";
            notifyIcon.Text = text.Length > 63 ? text.Substring(0, 63) : text;
        }

        private void ApplyTheme()
        {
            this.BackColor = Color.FromArgb(25, 28, 32);

            Color labelColor = Color.FromArgb(200, 200, 200);
            Color textColor = Color.White;

            // Apply colors to gauges
            ramGauge.GaugeColor = Color.FromArgb(58, 150, 221);
            ramGauge.LabelColor = labelColor;
            ramGauge.TextColor = textColor;

            cpuGauge.GaugeColor = Color.FromArgb(220, 50, 50);
            cpuGauge.LabelColor = labelColor;
            cpuGauge.TextColor = textColor;

            gpuUsageGauge.GaugeColor = Color.FromArgb(255, 140, 0);
            gpuUsageGauge.LabelColor = labelColor;
            gpuUsageGauge.TextColor = textColor;

            gpuVramGauge.GaugeColor = Color.FromArgb(160, 90, 240);
            gpuVramGauge.LabelColor = labelColor;
            gpuVramGauge.TextColor = textColor;

            diskGauge.GaugeColor = Color.FromArgb(50, 200, 80);
            diskGauge.LabelColor = labelColor;
            diskGauge.TextColor = textColor;

            networkGauge.GaugeColor = Color.FromArgb(255, 200, 50);
            networkGauge.LabelColor = labelColor;
            networkGauge.TextColor = textColor;
        }

        private float GetAutoScale(float currentValue, float peakValue)
        {
            float target = Math.Max(currentValue, peakValue);
            foreach (float scale in SCALE_OPTIONS)
                if (target <= scale * 0.95f) return scale;
            return SCALE_OPTIONS[^1];
        }

        #region Update Methods

        private void UpdateTimer_Tick(object? sender, EventArgs e) => UpdateAllMetrics();

        private void UpdateAllMetrics()
        {
            UpdateRAM();
            UpdateCPU();
            UpdateGPUUsage();
            UpdateGPUMemory();
            UpdateDisk();
            UpdateNetwork();
            UpdateTrayIconText();
        }

        private void UpdateRAM()
        {
            try
            {
                var mem = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(mem))
                {
                    ulong used = mem.ullTotalPhys - mem.ullAvailPhys;
                    double usedGB = used / (double)BYTES_TO_GB;
                    double totalGB = mem.ullTotalPhys / (double)BYTES_TO_GB;
                    int pct = (int)((used * 100) / mem.ullTotalPhys);
                    ramGauge.SetValue(pct, $"{usedGB:F1}/{totalGB:F0}GB");
                }
            }
            catch { ramGauge.SetValue(0, "ERR"); }
        }

        private void UpdateCPU()
        {
            try
            {
                if (_cpuMonitor?.IsAvailable == true)
                {
                    float usage = _cpuMonitor.Update();
                    _lastCpuUsage = (int)Math.Round(usage);
                    
                    // Try to get temperature - only display if we get a valid reading > 0
                    int temp = _cpuMonitor.UpdateTemperature();
                    if (temp > 0)
                    {
                        cpuGauge.SetValue(usage, $"{_lastCpuUsage}%", $"{temp}°C");
                        _cpuTempCheckCount = 0; // Reset counter when temp is available
                    }
                    else
                    {
                        cpuGauge.SetValue(usage, $"{_lastCpuUsage}%");
                        
                        // Check if we should show the warning
                        _cpuTempCheckCount++;
                        if (!_cpuTempWarningShown && _cpuTempCheckCount >= CPU_TEMP_CHECK_THRESHOLD)
                        {
                            ShowCpuTempWarning();
                        }
                    }
                }
                else
                {
                    cpuGauge.SetValue(0, "N/A");
                }
            }
            catch { cpuGauge.SetValue(0, "ERR"); }
        }

        private void ShowCpuTempWarning()
        {
            _cpuTempWarningShown = true;
            
            // Create a semi-transparent overlay panel with the warning message
            Panel warningPanel = new Panel
            {
                BackColor = Color.FromArgb(220, 30, 30, 35),
                Size = new Size(500, 120),
                BorderStyle = BorderStyle.None
            };

            Label titleLabel = new Label
            {
                Text = "? CPU Temperature Not Available",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 200, 50),
                AutoSize = false,
                Size = new Size(480, 25),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label messageLabel = new Label
            {
                Text = "To enable CPU temperature monitoring:\n" +
                       "1. Run HWiNFO (download from hwinfo.com)\n" +
                       "2. Go to Settings ? Enable 'Shared Memory Support'\n" +
                       "3. Keep HWiNFO running in the background",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = false,
                Size = new Size(400, 60),
                Location = new Point(50, 35),
                TextAlign = ContentAlignment.TopLeft
            };

            Button dismissButton = new Button
            {
                Text = "Dismiss",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
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
                this.Controls.Remove(warningPanel);
                warningPanel.Dispose();
            };

            warningPanel.Controls.Add(titleLabel);
            warningPanel.Controls.Add(messageLabel);
            warningPanel.Controls.Add(dismissButton);

            // Center the panel on the form
            warningPanel.Location = new Point(
                (this.ClientSize.Width - warningPanel.Width) / 2,
                (this.ClientSize.Height - warningPanel.Height) / 2
            );

            // Add border effect
            warningPanel.Paint += (s, e) =>
            {
                using (Pen borderPen = new Pen(Color.FromArgb(255, 200, 50), 2))
                {
                    e.Graphics.DrawRectangle(borderPen, 0, 0, warningPanel.Width - 1, warningPanel.Height - 1);
                }
            };

            this.Controls.Add(warningPanel);
            warningPanel.BringToFront();
        }

        private void UpdateGPUUsage()
        {
            try
            {
                if (_gpuMonitor?.IsUsageAvailable == true)
                {
                    float usage = _gpuMonitor.UpdateUsage();
                    _lastGpuUsage = (int)Math.Round(usage);
                    
                    // Also get temperature if available
                    if (_gpuMonitor.IsTemperatureAvailable)
                    {
                        int temp = _gpuMonitor.UpdateTemperature();
                        gpuUsageGauge.SetValue(usage, $"{_lastGpuUsage}%", $"{temp}°C");
                    }
                    else
                    {
                        gpuUsageGauge.SetValue(usage, $"{_lastGpuUsage}%");
                    }
                }
                else
                {
                    gpuUsageGauge.SetValue(0, "N/A");
                }
            }
            catch { gpuUsageGauge.SetValue(0, "ERR"); }
        }

        private void UpdateGPUMemory()
        {
            try
            {
                if (_gpuMonitor?.IsMemoryAvailable == true)
                {
                    _gpuMonitor.UpdateMemory();
                    double usedGB = _gpuMonitor.UsedMemoryGB;
                    double totalGB = _gpuMonitor.TotalMemoryGB;
                    gpuVramGauge.SetValue((float)usedGB, $"{usedGB:F1}/{totalGB:F0}GB");
                }
                else
                {
                    gpuVramGauge.SetValue(0, "N/A");
                }
            }
            catch { gpuVramGauge.SetValue(0, "ERR"); }
        }

        private void UpdateDisk()
        {
            try
            {
                if (_diskMonitor?.IsAvailable == true)
                {
                    var (_, _, totalMbps) = _diskMonitor.Update();
                    _diskPeakMbps = Math.Max(_diskPeakMbps, totalMbps);
                    diskGauge.MaxValue = GetAutoScale(totalMbps, _diskPeakMbps);
                    diskGauge.SetValue(totalMbps, $"{totalMbps:F0}MB/s");
                }
                else
                {
                    diskGauge.SetValue(0, "N/A");
                }
            }
            catch { diskGauge.SetValue(0, "ERR"); }
        }

        private void UpdateNetwork()
        {
            try
            {
                if (_networkMonitor?.IsAvailable == true)
                {
                    var (_, _, totalMbps) = _networkMonitor.Update();
                    _networkPeakMbps = Math.Max(_networkPeakMbps, totalMbps);
                    networkGauge.MaxValue = GetAutoScale(totalMbps, _networkPeakMbps);
                    networkGauge.SetValue(totalMbps, $"{totalMbps:F0}Mbps");
                }
                else
                {
                    networkGauge.SetValue(0, "N/A");
                }
            }
            catch { networkGauge.SetValue(0, "ERR"); }
        }

        #endregion

        #region Event Handlers

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e) => ShowForm();

        private void ShowToolStripMenuItem_Click(object? sender, EventArgs e) => ShowForm();

        private void ExitToolStripMenuItem_Click(object? sender, EventArgs e) => Application.Exit();

        private void ShowForm()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            Activate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer.Stop();
            _cpuMonitor?.Dispose();
            _gpuMonitor?.Dispose();
            _diskMonitor?.Dispose();
            _networkMonitor?.Dispose();
            base.OnFormClosing(e);
        }

        private void MiniMonitorForm_Resize(object? sender, EventArgs e)
        {
            LayoutGauges();
        }

        private void LayoutGauges()
        {
            int formWidth = ClientSize.Width;
            int formHeight = ClientSize.Height;

            // 6 gauges in a row with equal spacing
            int gaugeCount = 6;
            int margin = 20;
            int availableWidth = formWidth - (margin * 2);
            int gaugeWidth = availableWidth / gaugeCount;
            int gaugeHeight = formHeight - 20; // Leave small margin top/bottom

            int startX = margin;
            int startY = 10;

            ramGauge.Location = new Point(startX, startY);
            ramGauge.Size = new Size(gaugeWidth, gaugeHeight);

            cpuGauge.Location = new Point(startX + gaugeWidth, startY);
            cpuGauge.Size = new Size(gaugeWidth, gaugeHeight);

            gpuUsageGauge.Location = new Point(startX + gaugeWidth * 2, startY);
            gpuUsageGauge.Size = new Size(gaugeWidth, gaugeHeight);

            gpuVramGauge.Location = new Point(startX + gaugeWidth * 3, startY);
            gpuVramGauge.Size = new Size(gaugeWidth, gaugeHeight);

            diskGauge.Location = new Point(startX + gaugeWidth * 4, startY);
            diskGauge.Size = new Size(gaugeWidth, gaugeHeight);

            networkGauge.Location = new Point(startX + gaugeWidth * 5, startY);
            networkGauge.Size = new Size(gaugeWidth, gaugeHeight);
        }

        #endregion
    }
}
