using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using LibreHardwareMonitor.Hardware;

namespace Memory_Monitor
{
    /// <summary>
    /// Update visitor required by LibreHardwareMonitor to traverse and update hardware
    /// </summary>
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
            {
                subHardware.Accept(this);
            }
        }

        public void VisitSensor(ISensor sensor) { }

        public void VisitParameter(IParameter parameter) { }
    }

    /// <summary>
    /// Wrapper service for LibreHardwareMonitor to provide CPU and other hardware temperatures.
    /// Falls back to HWiNFO shared memory if LibreHardwareMonitor doesn't work.
    /// </summary>
    public class HardwareMonitorService : IDisposable
    {
        private Computer? _computer;
        private UpdateVisitor? _updateVisitor;
        private IHardware? _cpuHardware;
        private IHardware? _motherboardHardware;
        private HWiNFOReader? _hwinfoReader;
        private bool _isInitialized = false;
        private bool _sensorsFound = false;
        private bool _useHwinfoFallback = false;

        // Cached sensor references for quick updates
        private ISensor? _cpuPackageTemp;
        private ISensor? _cpuCoreMaxTemp;
        private ISensor? _motherboardCpuTemp;

        public bool IsAvailable => _isInitialized && _computer != null;
        public bool IsCpuTemperatureAvailable => _sensorsFound || _useHwinfoFallback;

        public HardwareMonitorService()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = false,
                    IsMemoryEnabled = false,
                    IsMotherboardEnabled = true,
                    IsStorageEnabled = false,
                    IsNetworkEnabled = false,
                    IsControllerEnabled = false,
                    IsBatteryEnabled = false,
                    IsPsuEnabled = false
                };

                _computer.Open();
                _updateVisitor = new UpdateVisitor();
                _isInitialized = true;

                Debug.WriteLine("LibreHardwareMonitor Computer opened");

                // Initial update using visitor pattern
                for (int i = 0; i < 3; i++)
                {
                    _computer.Accept(_updateVisitor);
                    System.Threading.Thread.Sleep(100);
                }

                // Find CPU and Motherboard hardware
                foreach (var hardware in _computer.Hardware)
                {
                    Debug.WriteLine($"Found hardware: {hardware.Name}, Type: {hardware.HardwareType}");
                    
                    if (hardware.HardwareType == HardwareType.Cpu)
                    {
                        _cpuHardware = hardware;
                        Debug.WriteLine($"Selected CPU hardware: {hardware.Name}");
                    }
                    else if (hardware.HardwareType == HardwareType.Motherboard)
                    {
                        _motherboardHardware = hardware;
                        Debug.WriteLine($"Selected Motherboard hardware: {hardware.Name}");
                    }
                }

                // Do sensor discovery
                FindCpuTemperatureSensors();
                FindMotherboardCpuTemperature();

                // If no valid sensors found from LibreHardwareMonitor, try HWiNFO fallback
                if (!_sensorsFound)
                {
                    Debug.WriteLine("No valid LibreHardwareMonitor CPU temp sensors, trying HWiNFO fallback...");
                    TryHwinfoFallback();
                }

                Debug.WriteLine($"HardwareMonitorService initialized. LHM Sensors: {_sensorsFound}, HWiNFO Fallback: {_useHwinfoFallback}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize HardwareMonitorService: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                _isInitialized = false;
                
                // Even if LHM fails, try HWiNFO
                TryHwinfoFallback();
            }
        }

        private void TryHwinfoFallback()
        {
            try
            {
                _hwinfoReader = new HWiNFOReader();
                if (_hwinfoReader.IsAvailable)
                {
                    // Test if we can get a temperature
                    var temp = _hwinfoReader.GetCpuTemperature();
                    if (temp.HasValue)
                    {
                        _useHwinfoFallback = true;
                        Debug.WriteLine($"HWiNFO fallback enabled: {_hwinfoReader.CpuTempSensorName} = {temp}°C");
                    }
                    else
                    {
                        Debug.WriteLine("HWiNFO is available but no CPU temperature found");
                        _hwinfoReader.Dispose();
                        _hwinfoReader = null;
                    }
                }
                else
                {
                    Debug.WriteLine("HWiNFO shared memory not available");
                    _hwinfoReader = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HWiNFO fallback failed: {ex.Message}");
                _hwinfoReader?.Dispose();
                _hwinfoReader = null;
            }
        }

        private void FindMotherboardCpuTemperature()
        {
            if (_motherboardHardware == null) return;

            try
            {
                Debug.WriteLine($"Motherboard {_motherboardHardware.Name} has {_motherboardHardware.Sensors.Length} sensors:");

                foreach (var sensor in _motherboardHardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        Debug.WriteLine($"  MB Sensor: {sensor.Name}, Value: {sensor.Value}, HasValue: {sensor.Value.HasValue}");
                        
                        string name = sensor.Name.ToLower();
                        if (name.Contains("cpu") && sensor.Value.HasValue && sensor.Value.Value > 0)
                        {
                            _motherboardCpuTemp = sensor;
                            _sensorsFound = true;
                            Debug.WriteLine($"  -> Selected MB CPU temp sensor: {sensor.Name} = {sensor.Value}°C");
                        }
                    }
                }

                // Check sub-hardware (SuperIO chips)
                Debug.WriteLine($"Motherboard has {_motherboardHardware.SubHardware.Length} sub-hardware devices:");
                foreach (var subHardware in _motherboardHardware.SubHardware)
                {
                    subHardware.Update();
                    Debug.WriteLine($"  MB SubHardware: {subHardware.Name}, Type: {subHardware.HardwareType}, Sensors: {subHardware.Sensors.Length}");

                    foreach (var sensor in subHardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            Debug.WriteLine($"    MB Sub Sensor: {sensor.Name}, Value: {sensor.Value}, HasValue: {sensor.Value.HasValue}");
                            
                            string name = sensor.Name.ToLower();
                            bool hasValidValue = sensor.Value.HasValue && sensor.Value.Value > 0 && sensor.Value.Value < 120;
                            
                            if (hasValidValue && (name.Contains("cpu") || name.Contains("core") || name.Contains("cputin")))
                            {
                                if (_motherboardCpuTemp == null)
                                {
                                    _motherboardCpuTemp = sensor;
                                    _sensorsFound = true;
                                    Debug.WriteLine($"    -> Selected MB SubHW CPU temp: {sensor.Name} = {sensor.Value}°C");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding motherboard CPU temperature: {ex.Message}");
            }
        }

        private void FindCpuTemperatureSensors()
        {
            if (_cpuHardware == null) return;

            try
            {
                Debug.WriteLine($"CPU {_cpuHardware.Name} has {_cpuHardware.Sensors.Length} sensors:");
                
                foreach (var sensor in _cpuHardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        Debug.WriteLine($"  Sensor: {sensor.Name}, Type: {sensor.SensorType}, Value: {sensor.Value}, HasValue: {sensor.Value.HasValue}");
                        
                        string name = sensor.Name.ToLower();
                        bool hasValidValue = sensor.Value.HasValue && sensor.Value.Value > 0 && sensor.Value.Value < 150;
                        
                        if (name.Contains("package"))
                        {
                            _cpuPackageTemp = sensor;
                            if (hasValidValue) _sensorsFound = true;
                            Debug.WriteLine($"  -> Selected as PRIMARY (Package): {sensor.Name}, Valid: {hasValidValue}");
                        }
                        else if (name.Contains("tctl") || name.Contains("tdie") || 
                                 (name.Contains("core") && name.Contains("average")))
                        {
                            if (_cpuPackageTemp == null)
                            {
                                _cpuPackageTemp = sensor;
                                if (hasValidValue) _sensorsFound = true;
                                Debug.WriteLine($"  -> Selected as PRIMARY: {sensor.Name}, Valid: {hasValidValue}");
                            }
                        }
                        else if (name.Contains("max") || name.StartsWith("core #") || name.StartsWith("p-core") || name.StartsWith("e-core"))
                        {
                            if (_cpuCoreMaxTemp == null)
                            {
                                _cpuCoreMaxTemp = sensor;
                                if (hasValidValue) _sensorsFound = true;
                                Debug.WriteLine($"  -> Selected as FALLBACK: {sensor.Name}, Valid: {hasValidValue}");
                            }
                        }
                    }
                }

                Debug.WriteLine($"CPU Sensor search complete. Package: {_cpuPackageTemp?.Name ?? "null"}, CoreMax: {_cpuCoreMaxTemp?.Name ?? "null"}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding CPU temperature sensors: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current CPU temperature in Celsius
        /// </summary>
        public int? GetCpuTemperature()
        {
            // First, try LibreHardwareMonitor sensors
            if (_sensorsFound && _computer != null)
            {
                try
                {
                    _computer.Accept(_updateVisitor);

                    // Try CPU Package sensor
                    if (_cpuPackageTemp != null && _cpuPackageTemp.Value.HasValue && _cpuPackageTemp.Value.Value > 0)
                    {
                        int temp = (int)Math.Round(_cpuPackageTemp.Value.Value);
                        if (temp > 0 && temp < 150) return temp;
                    }

                    // Try CPU Core Max sensor
                    if (_cpuCoreMaxTemp != null && _cpuCoreMaxTemp.Value.HasValue && _cpuCoreMaxTemp.Value.Value > 0)
                    {
                        int temp = (int)Math.Round(_cpuCoreMaxTemp.Value.Value);
                        if (temp > 0 && temp < 150) return temp;
                    }

                    // Try motherboard CPU temperature
                    if (_motherboardCpuTemp != null && _motherboardCpuTemp.Value.HasValue && _motherboardCpuTemp.Value.Value > 0)
                    {
                        int temp = (int)Math.Round(_motherboardCpuTemp.Value.Value);
                        if (temp > 0 && temp < 150) return temp;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading LHM temperature: {ex.Message}");
                }
            }

            // Fallback to HWiNFO
            if (_useHwinfoFallback && _hwinfoReader != null)
            {
                try
                {
                    var temp = _hwinfoReader.GetCpuTemperature();
                    if (temp.HasValue)
                    {
                        return temp.Value;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading HWiNFO temperature: {ex.Message}");
                }
            }

            return null;
        }

        public void Dispose()
        {
            try
            {
                _computer?.Close();
                _computer = null;
                _updateVisitor = null;
                _cpuHardware = null;
                _motherboardHardware = null;
                _cpuPackageTemp = null;
                _cpuCoreMaxTemp = null;
                _motherboardCpuTemp = null;
                
                _hwinfoReader?.Dispose();
                _hwinfoReader = null;
                
                _isInitialized = false;
                _sensorsFound = false;
                _useHwinfoFallback = false;
                
                Debug.WriteLine("HardwareMonitorService disposed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disposing HardwareMonitorService: {ex.Message}");
            }
        }
    }
}
