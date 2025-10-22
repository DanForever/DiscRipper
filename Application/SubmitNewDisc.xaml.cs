using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using DiscRipper.Sessions;
using DiscRipper.ViewModel;

using Microsoft.Extensions.Options;

namespace DiscRipper
{
    internal partial class SubmitNewDisc : Window
    {
        public Session Session { get; init; }
        public ViewModel.Submission Submission { get; init; }
        public MakeMkv.Log Log { get; init; }
        public MakeMkv.Drive? Drive { get; init; }

        public static string[] MediaTypes { get; } = ["Movie", "Series"];
        public static string[] DiscFormats { get; } = ["Blu-Ray", "UHD", "DVD"];
        public static IEnumerable<CultureInfo> AllRegions { get; private set; }

        static SubmitNewDisc()
        {
            AllRegions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).OrderBy(c => c.DisplayName);
        }

        public SubmitNewDisc(MakeMkv.Log log, List<Types.Title> titles, MakeMkv.Drive? drive = null)
        {
            InitializeComponent();

            IEnumerable<SubmissionTitle> submissionTitles = titles.Select(t => new SubmissionTitle { Model = t });

            Log = log;
            Drive = drive;

            Session = SessionManager.Instance.Value.CreateSession(titles);
            Session.Log = Log.ExportRawLog();

            Submission = new()
            {
                Model = Session.Submission,
                Titles = submissionTitles,

                // sensible defaults
                Locale = "en-US",
                MediaType = MediaTypes[0],
            };

            switch (Drive?.DiscType)
            {
            case MakeMkv.DiscType.DVD:
                Submission.DiscFormat = "DVD";
                break;

            case MakeMkv.DiscType.BD:
            default:
                Submission.DiscFormat = "Blu-Ray";
                break;
            }

            DataContext = Submission;
        }

        public SubmitNewDisc(Session session, MakeMkv.Log log)
        {
            InitializeComponent();

            Log = log;
            Session = session;

            IEnumerable<SubmissionTitle> submissionTitles = Session.Submission.Titles.Select(t => new SubmissionTitle { Model = t });

            Submission = new()
            {
                Model = Session.Submission,
                Titles = submissionTitles,
            };

            DataContext = Submission;
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            TheDiscDb.Submit.SubmissionContext context = new()
            {
                Submission = Session.Submission,
                InitializationData = new(Settings.Default.RepositoryFolder),
                Log = Log.ExportRawLog(true),
                DriveIndex = Drive?.Index,
                DrivePath = Drive?.DrivePath,
            };

            TheDiscDb.Submit.IStep[] steps =
            [
                new TheDiscDb.Submit.GenerateHashData(),
                new TheDiscDb.Submit.TmdbFetch(),
                new TheDiscDb.Submit.BuildMetadata(),
                new TheDiscDb.Submit.CreateDirectory(),
                new TheDiscDb.Submit.SeriesFilenames(),
                new TheDiscDb.Submit.DownloadPoster(),
                new TheDiscDb.Submit.WriteTmdb(),
                new TheDiscDb.Submit.WriteImdb(),
                new TheDiscDb.Submit.WriteMetadata(),
                new TheDiscDb.Submit.WriteRelease(),
                new TheDiscDb.Submit.SetDiscName(),
                //new TheDiscDb.Submit.WriteDiscMakemkvLog(), // Writing the makemkv log is currently done in AppendHashesToMakemkvLog because the "fantastic" filesystem doesn't actually allow for appending to files
                new TheDiscDb.Submit.WriteDiscSummary(),
                new TheDiscDb.Submit.AppendHashesToMakemkvLog(),
                new TheDiscDb.Submit.WriteDiscJson(),
            ];

            try
            {
                foreach (var step in steps)
                {
                    await step.Run(context);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Debug.WriteLine("Done");

            /*
            Fantastic.TheMovieDb.Caching.FileSystem.FileSystemCacheOptions options = new()
            {
                BaseDirectory = """C:\Users\theme\Desktop\Import buddy\.TheMovieDbCache"""
            };

            HttpClient httpClient = new();
            
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Debug).AddDebug();
            });

            ImportBuddy.ImportBuddyOptions ibOptions = new()
            {
                DataRepositoryPath = """J:\Reference\TheDiscDb\data""",
            };

            Fantastic.FileSystem.IFileSystem fileSystem = FileSystem;
            string name = "DanTestFS";
            Fantastic.TheMovieDb.Caching.FileSystem.FileSystemCache fileSystemCache = new(name, options, fileSystem);

            IOptionsMonitor<Fantastic.TheMovieDb.TheMovieDbOptions> theMovieDbOptionsMonitor =
                new StaticOptionsMonitor<Fantastic.TheMovieDb.TheMovieDbOptions>(theMovieDbOptions);

            Microsoft.Extensions.Options.OptionsWrapper<ImportBuddy.ImportBuddyOptions> wrappedOptions = new(ibOptions);

            Fantastic.TheMovieDb.TheMovieDbClient client = new(httpClient, theMovieDbOptionsMonitor, loggerFactory, fileSystemCache );

            ImportBuddy.RecentItemImportTask recentItemImportTask = new(fileSystem, wrappedOptions);
            ImportBuddy.TmdbByIdImportTask tmdbTask = new(client);

            ImportBuddy.IImportTask[] tasks = { recentItemImportTask, tmdbTask };

            ImportBuddy.ImportItem? importItem = null;
            foreach (var task in tasks)
            {
                if (task.CanHandle(TMDB, MediaType))
                {
                    var importedItem = await task.GetImportItem(TMDB, MediaType);
                    if (importedItem != null)
                    {
                        importItem = importedItem;
                        break;
                    }
                }
            }

            if (importItem == null)
                return;

            int year = importItem.TryGetYear();
            TDDB.ImportModels.MetadataFile? metadata = BuildMetadata(importItem.ImdbTitle, importItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Movie, importItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Series, year, MediaType);

            if (metadata != null && metadata.Title == null)
            {
                //AnsiConsole.WriteLine("Could not determine title for metadata");
                return;
            }

            string folderName = $"{fileSystem.CleanPath(metadata!.Title!)} ({year})";
            string subFolderName = MediaType.ToLower();

            string basePath = fileSystem.Path.Combine(wrappedOptions.Value.DataRepositoryPath!, subFolderName, folderName);

            if (!(await fileSystem.Directory.Exists(basePath)))
            {
                await fileSystem.Directory.CreateDirectory(basePath);
            }

            if (subFolderName == "series")
            {
                await GetSeriesFilenamesTask_RunInternal(fileSystem, client, importItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Series, basePath);
            }

            string? posterUrl = importItem.GetPosterUrl();
            string posterPath = fileSystem.Path.Combine(basePath, "cover.jpg");
            if (!await fileSystem.File.Exists(posterPath) && !string.IsNullOrEmpty(posterUrl))
            {
                //AnsiConsole.MarkupLine("Downloading poster...");
                await httpClient.Download(fileSystem, posterUrl, posterPath);
            }

            string tmdbPath = fileSystem.Path.Combine(basePath, "tmdb.json");
            if (!await fileSystem.File.Exists(tmdbPath) && importItem.GetTmdbItemToSerialize() != null)
            {
                await fileSystem.File.WriteAllText(tmdbPath, JsonSerializer.Serialize(importItem.GetTmdbItemToSerialize(), ImportBuddy.JsonHelper.JsonOptions));
            }

            string imdbPath = fileSystem.Path.Combine(basePath, "imdb.json");
            if (!await fileSystem.File.Exists(imdbPath) && importItem.ImdbTitle != null)
            {
                await fileSystem.File.WriteAllText(imdbPath, JsonSerializer.Serialize(importItem.ImdbTitle, ImportBuddy.JsonHelper.JsonOptions));
            }

            string metadataPath = fileSystem.Path.Combine(basePath, TDDB.ImportModels.MetadataFile.Filename);
            if (!await fileSystem.File.Exists(metadataPath))
            {
                await fileSystem.File.WriteAllText(metadataPath, JsonSerializer.Serialize(metadata, ImportBuddy.JsonHelper.JsonOptions));
            }

            var releaseFolders = await fileSystem.Directory.GetDirectories(basePath);
            bool hasReleases = releaseFolders.Any();
            string releaseFolder = fileSystem.Path.Combine(basePath, ReleaseSlug);

            // Create Release
            if (!await fileSystem.Directory.Exists(releaseFolder))
            {
                await fileSystem.Directory.CreateDirectory(releaseFolder);

                string upc = UPC;
                string asin = ASIN;
                string releaseDateString = PublicationDate.ToString("yyyy-MM-dd");
                string frontCoverUrl = FrontCoverUrl;
                string backCoverUrl = BackCoverUrl;
                string releaseName = EditionName;

                if (!string.IsNullOrEmpty(frontCoverUrl))
                {
                    string frontCoverPath = fileSystem.Path.Combine(releaseFolder, "front.jpg");
                    await httpClient.Download(fileSystem, frontCoverUrl, frontCoverPath);

                    //TODO: Update image url in the release file (and upload to blob storage)?
                }

                if (!string.IsNullOrEmpty(backCoverUrl))
                {
                    string backCoverPath = fileSystem.Path.Combine(releaseFolder, "back.jpg");
                    await httpClient.Download(fileSystem, backCoverUrl, backCoverPath);
                }

                DateTimeOffset releaseDate = default;
                int releaseYear = year;
                if (!string.IsNullOrEmpty(releaseDateString))
                {
                    // Format: October 25, 2022
                    DateTimeOffset.TryParse(releaseDateString, out releaseDate);
                    releaseYear = releaseDate.Year;
                }

                string releaseFile = fileSystem.Path.Combine(releaseFolder, TDDB.ImportModels.ReleaseFile.Filename);
                if (!await fileSystem.Directory.Exists(releaseFile))
                {
                    var release = new TDDB.ImportModels.ReleaseFile
                    {
                        Title = releaseName,
                        SortTitle = $"{releaseYear} {GetSortTitle(releaseName)}",
                        Slug = ReleaseSlug,
                        Upc = upc,
                        Locale = "en-us",
                        Year = releaseYear,
                        RegionCode = "1",
                        Asin = asin,
                        ReleaseDate = releaseDate,
                        DateAdded = DateTimeOffset.UtcNow.Date
                    };

                    string json = JsonSerializer.Serialize(release, ImportBuddy.JsonHelper.JsonOptions);
                    await fileSystem.File.WriteAllText(releaseFile, json);
                }
            }


            var discName = await GetDiscName(releaseFolder);
            string discTitle = DiscTitle;
            string discSlug = DiscSlug;

            string resolution = "1080p";
            switch(DiscFormat)
            {
            case "Blu-Ray":
                resolution = "1080p";
                break;

            case "UHD":
                resolution = "2160p";
                break;

            case "DVD":
                resolution = "720p";
                break;
            }

            string formattedTitle = $"{folderName} [{resolution}].mkv";

            string makeMkvLogPath = fileSystem.Path.Combine(releaseFolder, $"{discName.Name}.txt");
            if (!await fileSystem.File.Exists(makeMkvLogPath))
            {
                var summaryContents = new StringBuilder();
                summaryContents.AppendLine($"Name: {metadata.Title}");
                summaryContents.AppendLine("Type: MainMovie");
                summaryContents.AppendLine($"Year: {metadata.Year}");
                summaryContents.AppendLine($"File name: {formattedTitle}");

                string summaryPath = fileSystem.Path.Combine(releaseFolder, $"{discName.Name}-summary.txt");
                if (!await fileSystem.File.Exists(summaryPath))
                {
                    await fileSystem.File.WriteAllText(summaryPath, summaryContents.ToString());
                }

                string discJsonFilePath = fileSystem.Path.Combine(releaseFolder, $"{discName.Name}.json");
                if (!await fileSystem.File.Exists(discJsonFilePath))
                {
                    var discJsonFile = new TDDB.ImportModels.DiscFile
                    {
                        Index = discName.Index,
                        Slug = discSlug,
                        Name = discTitle,
                        Format = DiscFormat,
                        //ContentHash = data.HashInfo?.Hash
                    };

                    await fileSystem.File.WriteAllText(discJsonFilePath, JsonSerializer.Serialize(discJsonFile, ImportBuddy.JsonHelper.JsonOptions));
                }

               // await this.makeMkv.WriteLogs(data.Drive!.Index, makeMkvLogPath);
               // await DiskContentHash.TryAppendHashInfo(fileSystem, makeMkvLogPath, data.HashInfo, cancellationToken);
            }
            //*/
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Title_Selected(object sender, RoutedEventArgs e)
        {
            var grid = (DataGrid)sender;
            if (grid.SelectedItem != null)
            {
                SelectedTitleDetails.DataContext = grid.SelectedItem;
            }
        }

        private void MediaType_Changed(object sender, SelectionChangedEventArgs e)
        {
            var seasonColumn = TitleDetails.Columns.First(c => c.Header?.ToString() == "Season");
            var episodeColumn = TitleDetails.Columns.First(c => c.Header?.ToString() == "Episode");

            switch (Submission.MediaType)
            {
            case "Series":
                seasonColumn.Visibility = Visibility.Visible;
                episodeColumn.Visibility = Visibility.Visible;
                break;

                case "Movie":
                seasonColumn.Visibility = Visibility.Collapsed;
                episodeColumn.Visibility = Visibility.Collapsed;
                break;
            }
        }
    }
}
