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
            InitializeMonitors();
            InitializeUI();
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
    }
}
