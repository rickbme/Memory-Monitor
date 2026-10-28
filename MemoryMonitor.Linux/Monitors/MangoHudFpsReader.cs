using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Memory_Monitor.Linux
{
    /// <summary>
    /// Reads the current game FPS from MangoHud's CSV logs (the Linux stand-in for HWiNFO/RTSS on Windows).
    /// With logging enabled and log_interval set, MangoHud appends and flushes one row per interval to
    /// "&lt;output_folder&gt;/&lt;game&gt;_&lt;date&gt;_&lt;time&gt;.csv"; the first column of each row is the FPS.
    /// </summary>
    public class MangoHudFpsReader
    {
        // A log that hasn't been written to for this long belongs to a game that has stopped
        private static readonly TimeSpan StaleAfter = TimeSpan.FromSeconds(3);
        private const int TAIL_BYTES = 4096;

        /// <summary>
        /// Folder MangoHud writes its logs to (MangoHud's output_folder setting)
        /// </summary>
        public string LogFolder { get; }

        public MangoHudFpsReader(string logFolder)
        {
            LogFolder = logFolder;
        }

        /// <summary>
        /// Default log folder: inside the Steam snap's home when Steam is a snap (the snap can't write
        /// elsewhere in your home folder), otherwise ~/mangohud-logs
        /// </summary>
        public static string DefaultLogFolder
        {
            get
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string snapSteamHome = Path.Combine(home, "snap", "steam", "common");
                return Directory.Exists(snapSteamHome)
                    ? Path.Combine(snapSteamHome, "mangohud-logs")
                    : Path.Combine(home, "mangohud-logs");
            }
        }

        /// <summary>
        /// Returns the latest FPS from a game that is currently logging, or null if none is
        /// </summary>
        public int? GetFps()
        {
            try
            {
                if (!Directory.Exists(LogFolder))
                    return null;

                // Newest log being written right now (skip the per-session summary files)
                var latest = new DirectoryInfo(LogFolder)
                    .EnumerateFiles("*.csv")
                    .Where(f => !f.Name.EndsWith("_summary.csv", StringComparison.Ordinal))
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .FirstOrDefault();

                if (latest == null || DateTime.UtcNow - latest.LastWriteTimeUtc > StaleAfter)
                    return null;

                return ReadLastFps(latest.FullName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading MangoHud log: {ex.Message}");
                return null;
            }
        }

        // MangoHud names its logs "<program>_<YYYY-MM-DD>_<HH-MM-SS>.csv" (plus "..._summary.csv")
        private static readonly Regex MangoHudLogName = new(@"_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}(_summary)?\.csv$");

        /// <summary>
        /// Deletes MangoHud logs (and their summaries) older than maxAge from the log folder. MangoHud writes a new
        /// file for every game session, so the folder grows by roughly 1 MB per hour of play. Only files named
        /// like MangoHud logs are touched, so other CSV files kept in the folder are left alone.
        /// </summary>
        public void DeleteOldLogs(TimeSpan maxAge)
        {
            try
            {
                if (!Directory.Exists(LogFolder))
                    return;

                foreach (var file in new DirectoryInfo(LogFolder).EnumerateFiles("*.csv"))
                {
                    if (MangoHudLogName.IsMatch(file.Name) && DateTime.UtcNow - file.LastWriteTimeUtc > maxAge)
                    {
                        file.Delete();
                        Debug.WriteLine($"Deleted old MangoHud log: {file.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error cleaning up MangoHud logs: {ex.Message}");
            }
        }

        private static int? ReadLastFps(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            long start = Math.Max(0, stream.Length - TAIL_BYTES);
            stream.Seek(start, SeekOrigin.Begin);

            var buffer = new byte[stream.Length - start];
            int read = stream.Read(buffer, 0, buffer.Length);
            string tail = Encoding.UTF8.GetString(buffer, 0, read);

            // The last line may still be partly written, so only use complete lines
            string[] lines = tail.Split('\n');
            for (int i = lines.Length - 2; i >= 0; i--)
            {
                string firstField = lines[i].Split(',')[0].Trim();
                if (double.TryParse(firstField, NumberStyles.Float, CultureInfo.InvariantCulture, out double fps))
                    return (int)Math.Round(fps);
            }
            return null;
        }
    }
}
