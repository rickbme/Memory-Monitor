using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

namespace Memory_Monitor
{
    /// <summary>
    /// Reads sensor data from HWiNFO's shared memory interface.
    /// Requires HWiNFO to be running with "Shared Memory Support" enabled in settings.
    /// </summary>
    public class HWiNFOReader : IDisposable
    {
        private const string HWINFO_SHARED_MEM_FILE_NAME = "Global\\HWiNFO_SENS_SM2";
        private const string HWINFO_SHARED_MEM_MUTEX_NAME = "Global\\HWiNFO_SM2_MUTEX";

        private MemoryMappedFile? _memoryMappedFile;
        private MemoryMappedViewAccessor? _accessor;
        private bool _isAvailable = false;

        // Cached CPU temperature sensor info
        private uint _cpuTempSensorIndex = uint.MaxValue;
        private uint _cpuTempReadingIndex = uint.MaxValue;
        private string _cpuTempSensorName = "";

        // Cached FPS sensor info
        private uint _fpsSensorIndex = uint.MaxValue;
        private uint _fpsReadingIndex = uint.MaxValue;
        private string _fpsSensorName = "";

        public bool IsAvailable => _isAvailable;
        public string CpuTempSensorName => _cpuTempSensorName;
        public bool IsFpsAvailable => _fpsReadingIndex != uint.MaxValue;
        public string FpsSensorName => _fpsSensorName;

        #region HWiNFO Structures

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct HWiNFO_SHARED_MEM
        {
            public uint Signature;           // "HWiS" = 0x53695748
            public uint Version;             // Version of shared memory
            public uint Revision;            // Revision
            public long PollTime;            // Last poll time
            public uint OffsetOfSensorSection;
            public uint SizeOfSensorElement;
            public uint NumSensorElements;
            public uint OffsetOfReadingSection;
            public uint SizeOfReadingElement;
            public uint NumReadingElements;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct HWiNFO_SENSOR
        {
            public uint SensorId;
            public uint SensorInstance;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string SensorNameOrig;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string SensorNameUser;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct HWiNFO_READING
        {
            public uint ReadingType;         // 0=None, 1=Temp, 2=Voltage, 3=Fan, 4=Current, 5=Power, 6=Clock, 7=Usage, 8=Other
            public uint SensorIndex;
            public uint ReadingId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string LabelOrig;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string LabelUser;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Unit;
            public double Value;
            public double ValueMin;
            public double ValueMax;
            public double ValueAvg;
        }

        private const uint READING_TYPE_TEMP = 1;
        private const uint READING_TYPE_OTHER = 8;

        #endregion

        public HWiNFOReader()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Try to open HWiNFO's shared memory
                _memoryMappedFile = MemoryMappedFile.OpenExisting(HWINFO_SHARED_MEM_FILE_NAME, MemoryMappedFileRights.Read);
                _accessor = _memoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

                // Read header
                var header = ReadHeader();
                if (header.Signature != 0x53695748) // "HWiS"
                {
                    Debug.WriteLine("HWiNFO: Invalid signature in shared memory");
                    Dispose();
                    return;
                }

                Debug.WriteLine($"HWiNFO: Connected to shared memory v{header.Version}.{header.Revision}");
                Debug.WriteLine($"HWiNFO: {header.NumSensorElements} sensors, {header.NumReadingElements} readings");

                _isAvailable = true;

                // Find sensors
                FindSensors(header);
            }
            catch (System.IO.FileNotFoundException)
            {
                Debug.WriteLine("HWiNFO: Shared memory not found. Is HWiNFO running with 'Shared Memory Support' enabled?");
                _isAvailable = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HWiNFO: Failed to initialize - {ex.Message}");
                _isAvailable = false;
            }
        }

        private HWiNFO_SHARED_MEM ReadHeader()
        {
            byte[] buffer = new byte[Marshal.SizeOf<HWiNFO_SHARED_MEM>()];
            _accessor!.ReadArray(0, buffer, 0, buffer.Length);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<HWiNFO_SHARED_MEM>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        private void FindSensors(HWiNFO_SHARED_MEM header)
        {
            if (_accessor == null) return;

            try
            {
                // Read all sensors
                var sensors = new HWiNFO_SENSOR[header.NumSensorElements];
                int sensorSize = (int)header.SizeOfSensorElement;

                for (int i = 0; i < header.NumSensorElements; i++)
                {
                    long offset = header.OffsetOfSensorSection + (i * sensorSize);
                    byte[] buffer = new byte[sensorSize];
                    _accessor.ReadArray(offset, buffer, 0, Math.Min(buffer.Length, Marshal.SizeOf<HWiNFO_SENSOR>()));

                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    try
                    {
                        sensors[i] = Marshal.PtrToStructure<HWiNFO_SENSOR>(handle.AddrOfPinnedObject());
                        
                        string name = sensors[i].SensorNameOrig?.ToLower() ?? "";
                        if (name.Contains("cpu") || name.Contains("core") || name.Contains("processor"))
                        {
                            Debug.WriteLine($"HWiNFO Sensor[{i}]: {sensors[i].SensorNameOrig}");
                        }
                    }
                    finally
                    {
                        handle.Free();
                    }
                }

                // Read all readings to find CPU temperature and FPS
                int readingSize = (int)header.SizeOfReadingElement;
                
                for (int i = 0; i < header.NumReadingElements; i++)
                {
                    long offset = header.OffsetOfReadingSection + (i * readingSize);
                    byte[] buffer = new byte[readingSize];
                    _accessor.ReadArray(offset, buffer, 0, Math.Min(buffer.Length, Marshal.SizeOf<HWiNFO_READING>()));

                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    try
                    {
                        var reading = Marshal.PtrToStructure<HWiNFO_READING>(handle.AddrOfPinnedObject());
                        
                        // Look for temperature readings (CPU)
                        if (reading.ReadingType == READING_TYPE_TEMP)
                        {
                            string label = reading.LabelOrig?.ToLower() ?? "";
                            string sensorName = sensors[reading.SensorIndex].SensorNameOrig?.ToLower() ?? "";

                            bool isCpuSensor = sensorName.Contains("cpu") || sensorName.Contains("core") || sensorName.Contains("processor");
                            bool isCpuTemp = label.Contains("package") || label.Contains("tctl") || label.Contains("tdie") || 
                                            label.Contains("cpu") || (label.Contains("core") && !label.Contains("distance"));

                            if (isCpuSensor && isCpuTemp && reading.Value > 0 && reading.Value < 150)
                            {
                                Debug.WriteLine($"HWiNFO: Found CPU temp - {sensors[reading.SensorIndex].SensorNameOrig} / {reading.LabelOrig} = {reading.Value}°C");
                                
                                if (_cpuTempSensorIndex == uint.MaxValue || label.Contains("package"))
                                {
                                    _cpuTempSensorIndex = reading.SensorIndex;
                                    _cpuTempReadingIndex = (uint)i;
                                    _cpuTempSensorName = $"{sensors[reading.SensorIndex].SensorNameOrig} - {reading.LabelOrig}";
                                    
                                    if (label.Contains("package"))
                                    {
                                        // Package is preferred, but continue to find FPS
                                    }
                                }
                            }
                        }

                        // Look for FPS readings (typically in "Other" category)
                        // FPS sensors come from RTSS, games, or overlay software reporting to HWiNFO
                        string fpsLabel = reading.LabelOrig?.ToLower() ?? "";
                        string fpsUnit = reading.Unit?.ToLower() ?? "";
                        
                        // Check for FPS indicators in label or unit
                        bool isFpsReading = fpsLabel.Contains("fps") || 
                                           fpsLabel.Contains("framerate") || 
                                           fpsLabel.Contains("frame rate") ||
                                           fpsLabel.Contains("frames") ||
                                           fpsUnit.Contains("fps");

                        if (isFpsReading && reading.Value >= 0 && reading.Value < 10000)
                        {
                            Debug.WriteLine($"HWiNFO: Found FPS sensor - {sensors[reading.SensorIndex].SensorNameOrig} / {reading.LabelOrig} = {reading.Value} {reading.Unit}");
                            
                            // Prefer sensors with higher values (more likely to be active)
                            if (_fpsReadingIndex == uint.MaxValue || reading.Value > 0)
                            {
                                _fpsSensorIndex = reading.SensorIndex;
                                _fpsReadingIndex = (uint)i;
                                _fpsSensorName = $"{sensors[reading.SensorIndex].SensorNameOrig} - {reading.LabelOrig}";
                            }
                        }
                    }
                    finally
                    {
                        handle.Free();
                    }
                }

                if (_cpuTempSensorIndex != uint.MaxValue)
                {
                    Debug.WriteLine($"HWiNFO: Selected CPU temp sensor: {_cpuTempSensorName}");
                }
                else
                {
                    Debug.WriteLine("HWiNFO: No CPU temperature sensor found");
                }

                if (_fpsReadingIndex != uint.MaxValue)
                {
                    Debug.WriteLine($"HWiNFO: Selected FPS sensor: {_fpsSensorName}");
                }
                else
                {
                    Debug.WriteLine("HWiNFO: No FPS sensor found (RTSS or game overlay may not be running)");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HWiNFO: Error finding sensors - {ex.Message}");
            }
        }

        /// <summary>
        /// Refresh sensor detection - useful if RTSS/game starts after HWiNFO
        /// </summary>
        public void RefreshSensors()
        {
            if (!_isAvailable || _accessor == null)
                return;

            try
            {
                var header = ReadHeader();
                
                // Only refresh FPS if not already found
                if (_fpsReadingIndex == uint.MaxValue)
                {
                    FindSensors(header);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HWiNFO: Error refreshing sensors - {ex.Message}");
            }
        }

        /// <summary>
        /// Get current CPU temperature from HWiNFO
        /// </summary>
        public int? GetCpuTemperature()
        {
            if (!_isAvailable || _accessor == null || _cpuTempReadingIndex == uint.MaxValue)
                return null;

            try
            {
                var header = ReadHeader();
                int readingSize = (int)header.SizeOfReadingElement;
                long offset = header.OffsetOfReadingSection + (_cpuTempReadingIndex * readingSize);

                byte[] buffer = new byte[readingSize];
                _accessor.ReadArray(offset, buffer, 0, Math.Min(buffer.Length, Marshal.SizeOf<HWiNFO_READING>()));

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    var reading = Marshal.PtrToStructure<HWiNFO_READING>(handle.AddrOfPinnedObject());
                    
                    if (reading.ReadingType == READING_TYPE_TEMP && reading.Value > 0 && reading.Value < 150)
                    {
                        return (int)Math.Round(reading.Value);
                    }
                }
                finally
                {
                    handle.Free();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HWiNFO: Error reading temperature - {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get current FPS from HWiNFO (requires RTSS or game overlay reporting to HWiNFO)
        /// </summary>
        public int? GetFps()
        {
            if (!_isAvailable || _accessor == null || _fpsReadingIndex == uint.MaxValue)
                return null;

            try
            {
                var header = ReadHeader();
                int readingSize = (int)header.SizeOfReadingElement;
                long offset = header.OffsetOfReadingSection + (_fpsReadingIndex * readingSize);

                byte[] buffer = new byte[readingSize];
                _accessor.ReadArray(offset, buffer, 0, Math.Min(buffer.Length, Marshal.SizeOf<HWiNFO_READING>()));

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    var reading = Marshal.PtrToStructure<HWiNFO_READING>(handle.AddrOfPinnedObject());
                    
                    if (reading.Value >= 0 && reading.Value < 10000)
                    {
                        return (int)Math.Round(reading.Value);
                    }
                }
                finally
                {
                    handle.Free();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HWiNFO: Error reading FPS - {ex.Message}");
            }

            return null;
        }

        public void Dispose()
        {
            _accessor?.Dispose();
            _accessor = null;

            _memoryMappedFile?.Dispose();
            _memoryMappedFile = null;

            _isAvailable = false;
        }
    }
}
