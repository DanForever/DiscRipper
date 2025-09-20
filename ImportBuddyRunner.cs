using System.Diagnostics;
using System.IO;

namespace DiscRipper
{
    class ImportBuddyRunner
    {
        static string ExePath => """C:\Users\theme\Desktop\Import buddy\ImportBuddy.exe""";

        public async Task Run()
        {
            Neon.WinTTY.ConsoleTTY console = new();

            console.Run(ExePath);

            //ProcessStartInfo psi = new()
            //{
            //    FileName = ExePath,
            //    WorkingDirectory = Path.GetDirectoryName(ExePath),
            //    Arguments = "",
            //    RedirectStandardOutput = true,
            //    RedirectStandardError = true,
            //    UseShellExecute = false,
            //    CreateNoWindow = true,
            //};

            //using var process = new Process { StartInfo = psi };

            //// Subscribe using a method instead of a lambda
            //process.OutputDataReceived += OnStandardOutRecieved;
            //process.ErrorDataReceived += OnStandardErrorRecieved;

            //process.Start();

            //// Begin async read
            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();

            //await process.WaitForExitAsync();
        }

        private void OnStandardErrorRecieved(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data);
        }

        private void OnStandardOutRecieved(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data);
        }
    }
}
