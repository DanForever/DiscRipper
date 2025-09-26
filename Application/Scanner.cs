using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DiscRipper
{
    namespace MakeMkv
    {
        enum TitleEntryType
        {
            Duration = 9,
            Filename = 27
        }
    }

    internal class Scanner
    {
        #region Private fields

        private string _makeMkvDir = """C:\Program Files (x86)\MakeMKV""";

        #endregion Private fields

        #region Public properties

        public string MakeMKVDir => _makeMkvDir;

        public string MakeMkvCon => $"""{MakeMKVDir}\makemkvcon.exe""";

        public static string DriveRegex => """
                                    DRV:(\d+),(\d+),999,(\d+),"([^"]*)","([^"]*)","([^"]*)"
                                    """;

        public static string TitleRegex => """
                                    TINFO:(\d+),(9|27),\d+,"([^"]+)"
                                    """;

        public ObservableCollection<MakeMkv.Drive> Drives { get; set; } = [];

        public List<MakeMkv.Title> Titles { get; set; } = [];

        public MakeMkv.Title LongestTitle => GetLongestTitle(Titles);

        #endregion Public properties

        #region Public methods

        public async Task ScanDrives()
        {
            string driveData = await RunCommandAsync(MakeMkvCon, "-r --cache=1 info disc:9999");

            Debug.WriteLine("Drive data:");
            Debug.WriteLine(driveData);

            Drives = AnalyseDriveData(driveData);
        }

        public static ObservableCollection<MakeMkv.Drive> AnalyseDriveData(string data)
        {
            ObservableCollection<MakeMkv.Drive> drives = [];

            MatchCollection matches = Regex.Matches(data, DriveRegex);

            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;

                MakeMkv.DriveStatus driveStatus = (MakeMkv.DriveStatus)int.Parse(match.Groups[2].Value);

                if (driveStatus == MakeMkv.DriveStatus.NotAttached)
                    continue;

                MakeMkv.Drive drive = new()
                {
                    DriveStatus = driveStatus,
                    Index = int.Parse(match.Groups[1].Value),
                    DiscType = (MakeMkv.DiscType)int.Parse(match.Groups[3].Value),
                    DriveName = match.Groups[4].Value,
                    MediaTitle = match.Groups[5].Value,
                    DrivePath = match.Groups[6].Value
                };

                drives.Add(drive);
            }

            return drives;
        }

        public async Task ScanTitles(MakeMkv.Drive drive)
        {
            string titleInfo = await RunCommandAsync(MakeMkvCon, $"-r --cache=1 info disc:{drive.Index}");

            Debug.WriteLine("Title data:");
            Debug.WriteLine(titleInfo);

            Titles = AnalyseTitleData(titleInfo);
        }

        public static List<MakeMkv.Title> AnalyseTitleData(string data)
        {
            List<MakeMkv.Title> titles = [];

            MatchCollection matches = Regex.Matches(data, TitleRegex);

            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;

                int index = int.Parse(match.Groups[1].Value);

                MakeMkv.Title title = GetTitle(index, titles);

                MakeMkv.TitleEntryType entryType = (MakeMkv.TitleEntryType)int.Parse(match.Groups[2].Value);

                switch (entryType)
                {
                case MakeMkv.TitleEntryType.Duration:
                    title.Duration = match.Groups[3].Value;
                    title.DurationInSeconds = ConvertDurationToSeconds(title.Duration);
                    break;

                case MakeMkv.TitleEntryType.Filename:
                    title.Filename = match.Groups[3].Value;
                    break;
                }
            }

            DebugPrintTitles(titles);

            return titles;
        }

        public static MakeMkv.Title GetLongestTitle(List<MakeMkv.Title> titles)
        {
            MakeMkv.Title longestTitle = titles[0];

            for (int i = 1; i < titles.Count; ++i)
            {
                if (longestTitle.DurationInSeconds < titles[i].DurationInSeconds)
                    longestTitle = titles[i];
            }

            return longestTitle;
        }

        #endregion Public methods

        #region Private methods

        private MakeMkv.Title GetTitle(int index)
        {
            
            return GetTitle(index, Titles);
        }

        private static MakeMkv.Title GetTitle(int index, List<MakeMkv.Title> titles)
        {
            MakeMkv.Title title;

            int arrayIndex = titles.FindIndex(t => t.Index == index);

            if (arrayIndex < 0)
            {
                title = new()
                {
                    Index = index
                };

                titles.Add(title);
            }
            else
            {
                title = titles[arrayIndex];
            }

            return title;
        }

        private static void DebugPrintTitles(IEnumerable<MakeMkv.Title> titles)
        {
            Debug.WriteLine($"Found {titles.Count()} titles");

            foreach(var title in titles)
            {
                Debug.WriteLine($"Title: {title.Filename} ({title.Duration})");
            }
        }

        private static async Task<string> RunCommandAsync(string fileName, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new() { StartInfo = psi, EnableRaisingEvents = true })
            {
                process.Start();

                // Read asynchronously
                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();

                await Task.WhenAll(outputTask, errorTask);
                await process.WaitForExitAsync();

                return outputTask.Result;
            }
        }

        private static int ConvertDurationToSeconds(string duration)
        {
            //Match match = Regex.Match(duration, @"(\d+):(\d+):(\d+)");

            //int hours = int.Parse(match.Groups[1].Value);
            //int minutes = int.Parse(match.Groups[2].Value);
            //int seconds = int.Parse(match.Groups[3].Value);

            //TimeSpan timeSpan = new(hours, minutes, seconds);

            //return timeSpan.Seconds;

            return (int)TimeSpan.Parse(duration).TotalSeconds;
        }

        #endregion Private methods
    }
}
