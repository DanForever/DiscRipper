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

        public static string InstallDirectory { get; set; } = """C:\Program Files (x86)\MakeMKV""";

        public static string MakeMkvConFilepath { get; set; } = Path.Join(InstallDirectory, "makemkvcon.exe");

        public string StandardOutput => _standardOutput;

        public string StandardError => _standardError;

        #endregion Public properties

        #region Events

        public delegate void OutputHandler(string output);
        public event OutputHandler? StandardOutputRecieved;
        public event OutputHandler? StandardErrorRecieved;

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
            process.OutputDataReceived += OnStandardOutRecieved;
            process.ErrorDataReceived += OnStandardErrorRecieved;

            process.Start();

            // Begin async read
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();
        }

        #endregion Public methods

        #region Event Handlers

        private void OnStandardOutRecieved(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Debug.WriteLine(e.Data);

                string sanitisedOutput = e.Data.Trim();

                _standardOutput += $"{sanitisedOutput}\n";
                StandardOutputRecieved?.Invoke(sanitisedOutput);
            }
        }

        private void OnStandardErrorRecieved(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Debug.WriteLine($"Error: {e.Data}");

                string sanitisedError = e.Data.Trim();

                _standardError += $"\n{sanitisedError}";
                StandardErrorRecieved?.Invoke(sanitisedError);
            }
        }

        #endregion Event Handlers
    }
}
