using System;
using System.Diagnostics;
using System.Drawing;

namespace Memory_Monitor
{
    /// <summary>
    /// Partial class containing all metric update methods
    /// </summary>
    public partial class MiniMonitorForm
    {
        // Fields for metric updates are defined here (shared with other partial classes)
        private static readonly float[] SCALE_OPTIONS = { 10f, 25f, 50f, 100f, 250f, 500f, 1000f, 2500f, 5000f, 10000f };

        private float _diskPeakMbps = 0;
        private float _networkPeakMbps = 0;
        private int _lastCpuUsage = 0;
        private int _lastGpuUsage = 0;

        // FPS monitoring
        private int _fpsRefreshCounter = 0;
        private const int FPS_SENSOR_REFRESH_INTERVAL = 10;

        // Tray icon
        private System.Drawing.Icon? _currentTrayIcon;
        private int _lastMemoryPercentage = 0;

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
            SafeUpdate(UpdateRAM);
            SafeUpdate(UpdateCPU);
            SafeUpdate(UpdateGPUUsage);
            SafeUpdate(UpdateGPUMemory);
            SafeUpdate(UpdateFps);
            SafeUpdate(UpdateDisk);
            SafeUpdate(UpdateNetwork);
            UpdateTrayIconText();
        }

        private void SafeUpdate(Action updateAction)
        {
            try
            {
                updateAction();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update error in {updateAction.Method.Name}: {ex.Message}");
            }
        }

        private void UpdateRAM()
        {
            try
            {
                if (_memoryMonitor != null)
                {
                    int pct = _memoryMonitor.Update();
                    ramGauge.SetValue(pct, _memoryMonitor.GetDisplayText());
                    UpdateTrayIcon(pct);
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

                    int temp = _cpuMonitor.UpdateTemperature();
                    if (temp > 0)
                    {
                        cpuGauge.SetValue(usage, $"{_lastCpuUsage}%", $"{temp}°C");
                    }
                    else
                    {
                        cpuGauge.SetValue(usage, $"{_lastCpuUsage}%");
                    }
                }
                else
                {
                    cpuGauge.SetValue(0, "N/A");
                }
            }
            catch { cpuGauge.SetValue(0, "ERR"); }
        }

        private void UpdateGPUUsage()
        {
            try
            {
                if (_gpuMonitor?.IsUsageAvailable == true)
                {
                    float usage = _gpuMonitor.UpdateUsage();
                    _lastGpuUsage = (int)Math.Round(usage);

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

        private void UpdateFps()
        {
            try
            {
                _fpsRefreshCounter++;
                if (_fpsRefreshCounter >= FPS_SENSOR_REFRESH_INTERVAL)
                {
                    _fpsRefreshCounter = 0;
                    _hwInfoReader?.RefreshSensors();
                }

                // Get FPS data from HWiNFO
                bool hasFpsData = _hwInfoReader?.IsFpsAvailable == true;
                int? fps = hasFpsData ? _hwInfoReader?.GetFps() : null;
                
                // Use game activity detector to determine if FPS should be shown
                if (_gameActivityDetector != null)
                {
                    bool shouldShow = _gameActivityDetector.ShouldShowFps(hasFpsData, fps, _lastGpuUsage);
                    
                    if (shouldShow && fps.HasValue && fps.Value > 0)
                    {
                        fpsGauge.SetFps(fps.Value);
                        fpsGauge.Visible = true;
                        return;
                    }
                }

                fpsGauge.Visible = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating FPS: {ex.Message}");
                fpsGauge.Visible = false;
            }
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
    }
}
