using System.Diagnostics;
using System.IO;

namespace DiscRipper
{
    internal class MakeMkvRunner
    {
        #region Private properties

        private string _standardOutput = "";
        private string _standardError = "";

        #endregion Private properties

        #region Public properties

        public static string MakeMkvConFilepath => Path.Join(Settings.Default.MakeMkvInstallFolder, "makemkvcon64.exe");

        public static string TitleCountRegex => """TCOUNT:(\d+)""";
        public static string DriveRegex => """
            DRV:(\d+),(\d+),(\d+),(\d+),"([^"]*)","([^"]*)","([^"]*)"
            """;
        public static string MessageRegex => """
            MSG:(\d+),(\d+),(\d+),"([^"]*)"
            """;
        public static string DiscInfoRegex => """
            CINFO:(\d+),(\d+),"([^"]*)"
            """;
        public static string TitleInfoRegex => """
            TINFO:(\d+),(\d+),(\d+),"([^"]*)"
            """;
        public static string TrackInfoRegex => """
            SINFO:(\d+),(\d+),(\d+),(\d+),"([^"]*)"
            """;

        public string StandardOutput => _standardOutput;

        public string StandardError => _standardError;

        #endregion Public properties

        #region Events

        public delegate void OutputHandler(string output);
        public event OutputHandler? StandardOutputReceived;
        public event OutputHandler? StandardErrorReceived;

        #endregion Events

        #region Public methods

        public async Task Drives()
        {
            await Run($"-r --cache=1 info disc:9999");
        }

        public async Task Info(int driveIndex)
        {
            await Run($"-r --cache=1 info disc:{driveIndex}");
        }

        public async Task Mkv(int driveIndex, int titleIndex, string outputDirectory)
        {
            await Run($"""-r --cache=128 mkv disc:{driveIndex} {titleIndex} "{outputDirectory}" """);
        }

        private async Task Run(string arguments)
        {
            if (!File.Exists(MakeMkvConFilepath))
                return;

            ProcessStartInfo psi = new()
            {
                FileName = MakeMkvConFilepath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };

            // Subscribe using a method instead of a lambda
            process.OutputDataReceived += OnStandardOutReceived;
            process.ErrorDataReceived += OnStandardErrorReceived;

            process.Start();

            // Begin async read
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();
        }

        #endregion Public methods

        #region Event Handlers

        private void OnStandardOutReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Debug.WriteLine(e.Data);

                string sanitisedOutput = e.Data.Trim();

                _standardOutput += $"{sanitisedOutput}\n";
                StandardOutputReceived?.Invoke(sanitisedOutput);
            }
        }

        private void OnStandardErrorReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Debug.WriteLine($"Error: {e.Data}");

                string sanitisedError = e.Data.Trim();

                _standardError += $"\n{sanitisedError}";
                StandardErrorReceived?.Invoke(sanitisedError);
            }
        }

        #endregion Event Handlers
    }
}
