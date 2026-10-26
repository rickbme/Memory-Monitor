using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Diagnostics;

namespace Memory_Monitor
{
    public partial class MainWindow : Window
    {
        // Hardware monitors
        private IMonitor? _memoryMonitor;
        private IMonitor? _cpuMonitor;
        private ISelectableMonitor? _gpuMonitor;
        private ISelectableMonitor? _diskMonitor;
        private ISelectableMonitor? _networkMonitor;

        // Update timer
        private DispatcherTimer? _updateTimer;

        // UI Controls
        private TextBlock? _cpuUsageText;
        private TextBlock? _cpuTempText;
        private TextBlock? _memoryUsageText;
        private TextBlock? _gpuUsageText;
        private TextBlock? _gpuMemoryText;
        private TextBlock? _gpuTempText;
        private TextBlock? _diskText;
        private TextBlock? _networkText;

        public MainWindow()
        {
            InitializeComponent();
            InitializeMonitors();
            InitializeUI();
            StartMonitoring();
        }

        private void InitializeComponent()
        {
            Title = "Memory Monitor - Linux Edition";
            Width = 1920;
            Height = 480;
            Background = new SolidColorBrush(Color.Parse("#1E1E1E"));
            
            // Set window to be borderless and always on top
            SystemDecorations = SystemDecorations.BorderOnly;
            CanResize = false;
        }

        private void InitializeMonitors()
        {
            try
            {
                _cpuMonitor = MonitorFactory.CreateCPUMonitor();
                Debug.WriteLine($"CPU Monitor initialized: {_cpuMonitor.IsAvailable}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize CPU monitor: {ex.Message}");
            }

            try
            {
                _memoryMonitor = MonitorFactory.CreateMemoryMonitor();
                Debug.WriteLine($"Memory Monitor initialized: {_memoryMonitor.IsAvailable}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize Memory monitor: {ex.Message}");
            }

            try
            {
                _gpuMonitor = MonitorFactory.CreateGPUMonitor();
                Debug.WriteLine($"GPU Monitor initialized: {_gpuMonitor.IsAvailable}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize GPU monitor: {ex.Message}");
            }

            try
            {
                _diskMonitor = MonitorFactory.CreateDiskMonitor();
                Debug.WriteLine($"Disk Monitor initialized: {_diskMonitor.IsAvailable}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize Disk monitor: {ex.Message}");
            }

            try
            {
                _networkMonitor = MonitorFactory.CreateNetworkMonitor();
                Debug.WriteLine($"Network Monitor initialized: {_networkMonitor.IsAvailable}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize Network monitor: {ex.Message}");
            }
        }

        private void InitializeUI()
        {
            var mainPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Margin = new Thickness(20),
                Spacing = 15
            };

            // Title
            var titleText = new TextBlock
            {
                Text = "System Monitor",
                FontSize = 24,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 20)
            };
            mainPanel.Children.Add(titleText);

            // CPU Section
            var cpuPanel = CreateSection("CPU");
            _cpuUsageText = new TextBlock
            {
                Text = "Usage: ---%",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.LightGray)
            };
            _cpuTempText = new TextBlock
            {
                Text = "Temperature: ---°C",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.LightGray)
            };
            cpuPanel.Children.Add(_cpuUsageText);
            cpuPanel.Children.Add(_cpuTempText);
            mainPanel.Children.Add(cpuPanel);

            // Memory Section
            var memoryPanel = CreateSection("Memory");
            _memoryUsageText = new TextBlock
            {
                Text = "Usage: --- / --- GB (---%)",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.LightGray)
            };
            memoryPanel.Children.Add(_memoryUsageText);
            mainPanel.Children.Add(memoryPanel);

            // GPU Section
            var gpuPanel = CreateSection("GPU");
            _gpuUsageText = new TextBlock
            {
                Text = "Usage: ---%",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.LightGray)
            };
            _gpuMemoryText = new TextBlock
            {
                Text = "Memory: --- / --- GB (---%)",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.LightGray)
            };
            _gpuTempText = new TextBlock
            {
                Text = "Temperature: ---°C",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.LightGray)
            };
            gpuPanel.Children.Add(_gpuUsageText);
            gpuPanel.Children.Add(_gpuMemoryText);
            gpuPanel.Children.Add(_gpuTempText);
            mainPanel.Children.Add(gpuPanel);

            // Disk Section
            var diskPanel = CreateSection("Disk");
            _diskText = new TextBlock
            {
                Text = "Read: --- MB/s | Write: --- MB/s",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.LightGray)
            };
            diskPanel.Children.Add(_diskText);
            mainPanel.Children.Add(diskPanel);

            // Network Section
            var networkPanel = CreateSection("Network");
            _networkText = new TextBlock
            {
                Text = "Download: --- Mbps | Upload: --- Mbps",
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.LightGray)
            };
            networkPanel.Children.Add(_networkText);
            mainPanel.Children.Add(networkPanel);

            Content = new ScrollViewer
            {
                Content = mainPanel,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            };
        }

        private StackPanel CreateSection(string title)
        {
            var panel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Spacing = 5
            };

            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#00D4FF")),
                Margin = new Thickness(0, 0, 0, 5)
            };
            panel.Children.Add(titleBlock);

            return panel;
        }

        private void StartMonitoring()
        {
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateCPU();
            UpdateMemory();
            UpdateGPU();
            UpdateDisk();
            UpdateNetwork();
        }

        private void UpdateCPU()
        {
            if (_cpuMonitor != null && _cpuMonitor.IsAvailable)
            {
                try
                {
#if WINDOWS
                    var cpu = _cpuMonitor as CPUMonitor;
#endif
                    var linuxCpu = _cpuMonitor as Linux.LinuxCPUMonitor;

#if WINDOWS
                    if (cpu != null)
                    {
                        var usage = cpu.Update();
                        if (_cpuUsageText != null)
                            _cpuUsageText.Text = $"Usage: {usage:F1}%";

                        var temp = cpu.UpdateTemperature();
                        if (_cpuTempText != null && temp > 0)
                            _cpuTempText.Text = $"Temperature: {temp}°C";
                    }
                    else
#endif
                    if (linuxCpu != null)
                    {
                        var usage = linuxCpu.Update();
                        if (_cpuUsageText != null)
                            _cpuUsageText.Text = $"Usage: {usage:F1}%";

                        var temp = linuxCpu.UpdateTemperature();
                        if (_cpuTempText != null && temp > 0)
                            _cpuTempText.Text = $"Temperature: {temp}°C";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating CPU: {ex.Message}");
                }
            }
        }

        private void UpdateMemory()
        {
            if (_memoryMonitor != null && _memoryMonitor.IsAvailable)
            {
                try
                {
#if WINDOWS
                    var mem = _memoryMonitor as SystemMemoryMonitor;
#endif
                    var linuxMem = _memoryMonitor as Linux.LinuxMemoryMonitor;

#if WINDOWS
                    if (mem != null)
                    {
                        mem.Update();
                        if (_memoryUsageText != null)
                            _memoryUsageText.Text = $"Usage: {mem.UsedMemoryGB:F2} / {mem.TotalMemoryGB:F2} GB ({mem.UsagePercent}%)";
                    }
                    else
#endif
                    if (linuxMem != null)
                    {
                        linuxMem.Update();
                        if (_memoryUsageText != null)
                            _memoryUsageText.Text = $"Usage: {linuxMem.UsedMemoryGB:F2} / {linuxMem.TotalMemoryGB:F2} GB ({linuxMem.UsagePercent}%)";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating Memory: {ex.Message}");
                }
            }
        }

        private void UpdateGPU()
        {
            if (_gpuMonitor != null && _gpuMonitor.IsAvailable)
            {
                try
                {
#if WINDOWS
                    var gpu = _gpuMonitor as GPUMonitor;
#endif
                    var linuxGpu = _gpuMonitor as Linux.LinuxGPUMonitor;

#if WINDOWS
                    if (gpu != null)
                    {
                        gpu.Update();
                        if (_gpuUsageText != null)
                            _gpuUsageText.Text = $"Usage: {gpu.CurrentUsagePercent:F1}%";
                        if (_gpuMemoryText != null)
                            _gpuMemoryText.Text = $"Memory: {gpu.UsedMemoryGB:F2} / {gpu.TotalMemoryGB:F2} GB ({gpu.MemoryUsagePercent}%)";
                        if (_gpuTempText != null && gpu.CurrentTemperatureCelsius > 0)
                            _gpuTempText.Text = $"Temperature: {gpu.CurrentTemperatureCelsius}°C";
                    }
                    else
#endif
                    if (linuxGpu != null)
                    {
                        linuxGpu.Update();
                        if (_gpuUsageText != null)
                            _gpuUsageText.Text = $"Usage: {linuxGpu.CurrentUsagePercent:F1}%";
                        if (_gpuMemoryText != null)
                            _gpuMemoryText.Text = $"Memory: {linuxGpu.UsedMemoryGB:F2} / {linuxGpu.TotalMemoryGB:F2} GB ({linuxGpu.MemoryUsagePercent}%)";
                        if (_gpuTempText != null && linuxGpu.CurrentTemperatureCelsius > 0)
                            _gpuTempText.Text = $"Temperature: {linuxGpu.CurrentTemperatureCelsius}°C";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating GPU: {ex.Message}");
                }
            }
        }

        private void UpdateDisk()
        {
            if (_diskMonitor != null && _diskMonitor.IsAvailable)
            {
                try
                {
#if WINDOWS
                    var disk = _diskMonitor as DiskMonitor;
#endif
                    var linuxDisk = _diskMonitor as Linux.LinuxDiskMonitor;

#if WINDOWS
                    if (disk != null)
                    {
                        disk.Update();
                        if (_diskText != null)
                            _diskText.Text = $"Read: {disk.CurrentReadMBps:F2} MB/s | Write: {disk.CurrentWriteMBps:F2} MB/s";
                    }
                    else
#endif
                    if (linuxDisk != null)
                    {
                        linuxDisk.Update();
                        if (_diskText != null)
                            _diskText.Text = $"Read: {linuxDisk.CurrentReadMBps:F2} MB/s | Write: {linuxDisk.CurrentWriteMBps:F2} MB/s";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating Disk: {ex.Message}");
                }
            }
        }

        private void UpdateNetwork()
        {
            if (_networkMonitor != null && _networkMonitor.IsAvailable)
            {
                try
                {
#if WINDOWS
                    var network = _networkMonitor as NetworkMonitor;
#endif
                    var linuxNetwork = _networkMonitor as Linux.LinuxNetworkMonitor;

#if WINDOWS
                    if (network != null)
                    {
                        network.Update();
                        if (_networkText != null)
                            _networkText.Text = $"Download: {network.CurrentDownloadMbps:F2} Mbps | Upload: {network.CurrentUploadMbps:F2} Mbps";
                    }
                    else
#endif
                    if (linuxNetwork != null)
                    {
                        linuxNetwork.Update();
                        if (_networkText != null)
                            _networkText.Text = $"Download: {linuxNetwork.CurrentDownloadMbps:F2} Mbps | Upload: {linuxNetwork.CurrentUploadMbps:F2} Mbps";
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating Network: {ex.Message}");
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _updateTimer?.Stop();
            
            (_cpuMonitor as IDisposable)?.Dispose();
            (_memoryMonitor as IDisposable)?.Dispose();
            (_gpuMonitor as IDisposable)?.Dispose();
            (_diskMonitor as IDisposable)?.Dispose();
            (_networkMonitor as IDisposable)?.Dispose();

            base.OnClosed(e);
        }
    }
}
