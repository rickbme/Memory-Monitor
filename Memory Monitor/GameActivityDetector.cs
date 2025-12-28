using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Memory_Monitor
{
    /// <summary>
    /// Detects game activity using multiple signals to determine when FPS display should be shown.
    /// Uses a weighted scoring system combining FPS availability, fullscreen detection,
    /// GPU usage, and known game processes.
    /// </summary>
    public class GameActivityDetector : IDisposable
    {
        #region Win32 API

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_STYLE = -16;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_CAPTION = 0x00C00000;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        /// <summary>
        /// Display mode for FPS overlay
        /// </summary>
        public enum FpsDisplayMode
        {
            AutoDetect,
            AlwaysShow,
            AlwaysHide
        }

        // Configuration
        private const float GPU_USAGE_THRESHOLD = 70f;
        private const int GPU_SUSTAINED_SECONDS = 5;
        private const float FULLSCREEN_SCORE = 0.6f;
        private const float GPU_USAGE_SCORE = 0.3f;
        private const float KNOWN_GAME_SCORE = 0.5f;
        private const float DETECTION_THRESHOLD = 0.5f;

        // State tracking
        private readonly Queue<float> _gpuUsageHistory = new();
        private readonly int _gpuHistorySize;
        private FpsDisplayMode _displayMode = FpsDisplayMode.AutoDetect;
        private bool _lastDetectionResult = false;

        // Known game process names (lowercase, without .exe)
        private static readonly HashSet<string> KnownGameProcesses = new(StringComparer.OrdinalIgnoreCase)
        {
            // Popular games
            "csgo", "cs2", "valorant", "fortnite", "apex_legends",
            "overwatch", "rocketleague", "gtav", "gta5",
            "cyberpunk2077", "eldenring", "baldursgate3",
            "helldivers2", "palworld", "enshrouded",
            "cod", "modernwarfare", "warzone",
            "minecraft", "javaw",
            "destiny2", "diablo", "worldofwarcraft", "wow",
            "leagueoflegends", "league of legends",
            "dota2", "starcraft", "hearthstone",
            "pubg", "rainbowsix", "r6",
            "battlefield", "fifa", "nba2k",
            "witcher3", "rdr2", "hogwartslegacy",
            "deadspace", "residentevil", "devilmaycry",
            "monsterhunter", "darksouls", "sekiro",
            "godofwar", "horizonzerodawn", "spiderman",
            "ffxiv", "finalfantasy", "persona",
            "totalwar", "civilization", "crusaderkings",
            "flightsimulator", "msfs", "xplane",
            "assettocorsa", "iracing", "forza"
        };

        public FpsDisplayMode DisplayMode
        {
            get => _displayMode;
            set
            {
                _displayMode = value;
                Debug.WriteLine($"FPS display mode changed to: {value}");
            }
        }

        public bool IsGameDetected => _lastDetectionResult;

        public GameActivityDetector(int updateIntervalMs = 1000)
        {
            _gpuHistorySize = (GPU_SUSTAINED_SECONDS * 1000) / updateIntervalMs;
        }

        /// <summary>
        /// Determines if FPS should be displayed based on current detection mode and signals.
        /// </summary>
        public bool ShouldShowFps(bool hasFpsData, int? fpsValue, float currentGpuUsage)
        {
            switch (_displayMode)
            {
                case FpsDisplayMode.AlwaysShow:
                    _lastDetectionResult = true;
                    return hasFpsData && fpsValue.HasValue && fpsValue.Value > 0;

                case FpsDisplayMode.AlwaysHide:
                    _lastDetectionResult = false;
                    return false;

                case FpsDisplayMode.AutoDetect:
                default:
                    _lastDetectionResult = DetectGameActivity(hasFpsData, fpsValue, currentGpuUsage);
                    return _lastDetectionResult && hasFpsData && fpsValue.HasValue && fpsValue.Value > 0;
            }
        }

        private bool DetectGameActivity(bool hasFpsData, int? fpsValue, float currentGpuUsage)
        {
            // Signal 1: FPS data is available and valid - strongest signal
            if (hasFpsData && fpsValue.HasValue && fpsValue.Value > 0)
            {
                return true;
            }

            // Calculate weighted score from other signals
            float score = 0f;

            // Signal 2: Fullscreen or borderless fullscreen detection
            if (IsFullscreenAppRunning())
            {
                score += FULLSCREEN_SCORE;
            }

            // Signal 3: Sustained high GPU usage
            UpdateGpuHistory(currentGpuUsage);
            if (HasSustainedGpuUsage())
            {
                score += GPU_USAGE_SCORE;
            }

            // Signal 4: Known game process in foreground
            if (IsForegroundProcessKnownGame())
            {
                score += KNOWN_GAME_SCORE;
            }

            return score >= DETECTION_THRESHOLD;
        }

        private bool IsFullscreenAppRunning()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                    return false;

                if (!GetWindowRect(foregroundWindow, out RECT windowRect))
                    return false;

                var windowBounds = new System.Drawing.Rectangle(
                    windowRect.Left,
                    windowRect.Top,
                    windowRect.Right - windowRect.Left,
                    windowRect.Bottom - windowRect.Top
                );

                var screen = System.Windows.Forms.Screen.FromRectangle(windowBounds);
                var screenBounds = screen.Bounds;

                bool coversScreen = windowRect.Left <= screenBounds.Left &&
                                   windowRect.Top <= screenBounds.Top &&
                                   windowRect.Right >= screenBounds.Right &&
                                   windowRect.Bottom >= screenBounds.Bottom;

                if (!coversScreen)
                    return false;

                int style = GetWindowLong(foregroundWindow, GWL_STYLE);
                bool isBorderless = (style & WS_CAPTION) == 0 || ((uint)style & WS_POPUP) != 0;

                return coversScreen && isBorderless;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateGpuHistory(float currentUsage)
        {
            _gpuUsageHistory.Enqueue(currentUsage);
            while (_gpuUsageHistory.Count > _gpuHistorySize)
            {
                _gpuUsageHistory.Dequeue();
            }
        }

        private bool HasSustainedGpuUsage()
        {
            if (_gpuUsageHistory.Count < _gpuHistorySize)
                return false;

            foreach (float usage in _gpuUsageHistory)
            {
                if (usage < GPU_USAGE_THRESHOLD)
                    return false;
            }

            return true;
        }

        private bool IsForegroundProcessKnownGame()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero)
                    return false;

                GetWindowThreadProcessId(foregroundWindow, out uint processId);
                if (processId == 0)
                    return false;

                using var process = Process.GetProcessById((int)processId);
                string processName = process.ProcessName.ToLowerInvariant();

                if (KnownGameProcesses.Contains(processName))
                    return true;

                foreach (string knownGame in KnownGameProcesses)
                {
                    if (processName.Contains(knownGame))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _gpuUsageHistory.Clear();
        }
    }
}
