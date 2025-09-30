using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using DiscRipper.TheDiscDb.Submit;
using DiscRipper.ViewModel;

using ImportBuddy;

using Microsoft.Extensions.Options;

using TDDB = global::TheDiscDb;

namespace DiscRipper
{
    public class StaticOptionsMonitor<T> : IOptionsMonitor<T>
    {
        private readonly T _currentValue;

        public StaticOptionsMonitor(T currentValue)
        {
            _currentValue = currentValue;
        }

        public T CurrentValue => _currentValue;

        public T Get(string? name) => _currentValue;

        public IDisposable OnChange(Action<T, string> listener) => NullDisposable.Instance;

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }

    public class StringVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public string Match { get; set; } = string.Empty;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string str)
            {
                var match = parameter as string ?? Match;
                return str == match ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }

    public static class DataGridColumnHelper
    {
        public static readonly DependencyProperty BindableVisibilityProperty =
            DependencyProperty.RegisterAttached(
                "BindableVisibility",
                typeof(Visibility),
                typeof(DataGridColumnHelper),
                new PropertyMetadata(Visibility.Visible, OnBindableVisibilityChanged));

        public static void SetBindableVisibility(DataGridColumn column, Visibility value) =>
            column.SetValue(BindableVisibilityProperty, value);

        public static Visibility GetBindableVisibility(DataGridColumn column) =>
            (Visibility)column.GetValue(BindableVisibilityProperty);

        private static void OnBindableVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridColumn column)
            {
                column.Visibility = (Visibility)e.NewValue;
            }
        }
    }

    /// <summary>
    /// Interaction logic for SubmitNewDisc.xaml
    /// </summary>
    internal partial class SubmitNewDisc : Window
    {
        private TheDiscDb.Submission _submission;
        public ViewModel.Submission Submission { get; init; }

        public static string[] MediaTypes { get; } = ["Movie", "Series"];
        public static string[] DiscFormats { get; } = ["Blu-Ray", "UHD", "DVD"];
        public static IEnumerable<CultureInfo> AllRegions { get; private set; }

        IEnumerable<string> ExistingReleases { get; set; } = [];

        Fantastic.FileSystem.IFileSystem FileSystem { get; } = new Fantastic.FileSystem.PhysicalFileSystem();

        static SubmitNewDisc()
        {
            AllRegions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).OrderBy(c => c.DisplayName);
        }

        public SubmitNewDisc(IEnumerable<TheDiscDb.Title> titles, MakeMkv.Drive? drive = null)
        {
            InitializeComponent();

            IEnumerable<SubmissionTitle> submissionTitles = titles.Select(t => new SubmissionTitle { Model = t });

            _submission = new() { Titles = titles };
            Submission = new()
            {
                Model = _submission,
                Titles = submissionTitles,

                // sensible defaults
                Locale = "en-US",
                MediaType = MediaTypes[0],
            };

            if(drive != null)
            {
                switch(drive.DiscType)
                {
                case MakeMkv.DiscType.BD:
                    Submission.DiscFormat = "Blu-Ray";
                    break;

                case MakeMkv.DiscType.DVD:
                    Submission.DiscFormat = "DVD";
                    break;
                }
            }

            DataContext = Submission;
            Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            DiscRipper.TheDiscDb.Submit.SubmissionContext context = new()
            {
                Submission = _submission,
                InitializationData = new(Settings.Default.RepositoryFolder)
            };

            IStep[] steps =
            [
                new TheDiscDb.Submit.TmdbFetch(),
                new TheDiscDb.Submit.BuildMetadata(),
                new TheDiscDb.Submit.CreateDirectory(),
                new TheDiscDb.Submit.SeriesFilenames(),
                new TheDiscDb.Submit.DownloadPoster(),
                new TheDiscDb.Submit.WriteTmdb(),
                new TheDiscDb.Submit.WriteImdb(),
                new TheDiscDb.Submit.WriteMetadata(),
            ];

            foreach(var step in steps)
            {
                await step.Run(context);
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

        internal async Task GetSeriesFilenamesTask_RunInternal(Fantastic.FileSystem.IFileSystem fileSystem, Fantastic.TheMovieDb.TheMovieDbClient tmdb, Fantastic.TheMovieDb.Models.Series? series, string basePath)
        {
            if (series == null)
            {
                return;
            }

            string episodeListPath = fileSystem.Path.Combine(basePath, GetSeriesFilenamesTask.EpisodesFilename);
            if (!await fileSystem.File.Exists(episodeListPath))
            {
                using (var writer = await fileSystem.File.CreateText(episodeListPath))
                {
                    List<Fantastic.TheMovieDb.Models.Episode> season0Episodes = new();
                    foreach (var season in series.Seasons)
                    {
                        var fullSeason = await tmdb.GetSeason(series.Id, season.SeasonNumber);

                        if (season.SeasonNumber != 0)
                        {
                            await writer.WriteLineAsync($"------------ Season {season.SeasonNumber:00} -----------");
                            await writer.WriteLineAsync();
                        }

                        foreach (var episode in fullSeason.Episodes)
                        {
                            if (season.SeasonNumber == 0)
                            {
                                season0Episodes.Add(episode);
                                continue;
                            }

                            string fileName = $"{series.Name}.S{season.SeasonNumber:00}.E{episode.EpisodeNumber:00}.{episode.Name}.mkv";
                            fileName = fileSystem.CleanPath(fileName);

                            // TODO: Handle multipart episode naming
                            await writer.WriteLineAsync($"Name: {episode.Name}");
                            await writer.WriteLineAsync("Type: Episode");
                            await writer.WriteLineAsync($"Season: {season.SeasonNumber}");
                            await writer.WriteLineAsync($"Episode: {episode.EpisodeNumber}");
                            await writer.WriteLineAsync($"File name: {fileName}");
                            await writer.WriteLineAsync();
                        }

                        await writer.WriteLineAsync();
                    }

                    // write the season 0 items at the end
                    foreach (var episode in season0Episodes)
                    {
                        string fileName = $"{series.Name}.S00.E{episode.EpisodeNumber:00}.{episode.Name}.mkv";
                        await writer.WriteLineAsync(fileName);
                    }
                }
            }
        }

        internal struct DiscName
        {
            public string Name;
            public int Index;
        }

        private async Task<DiscName> GetDiscName(string path)
        {
            var files = await FileSystem.Directory.GetFiles(path, "*disc*");
            var name = new DiscName
            {
                Name = "disc01",
                Index = 1
            };

            for (int i = 1; i < 100; i++)
            {
                name.Name = string.Format("disc{0:00}", i);
                name.Index = i;

                if (files.Any(f => f.Contains(name.Name)))
                {
                    continue;
                }

                break;
            }

            return name;
        }

        private string CreateSlug(string name, int year)
        {
            if (year != default(int))
            {
                return string.Format("{0}-{1}", TDDB.StringExtensions.Slugify(name), year);
            }

            return TDDB.StringExtensions.Slugify(name);
        }

        private string? GetSortTitle(string? title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return title;
            }

            if (title.StartsWith("the", StringComparison.OrdinalIgnoreCase))
            {
                return title.Substring(4, title.Length - 4).Trim() + ", The";
            }
            else
            {
                return title;
            }
        }

        private TDDB.ImportModels.MetadataFile BuildMetadata(TDDB.Imdb.TitleData? imdbTitle, Fantastic.TheMovieDb.Models.Movie? movie, Fantastic.TheMovieDb.Models.Series? series, int year, string importItemType)
        {
            var metadata = new TDDB.ImportModels.MetadataFile
            {
                Year = year,
                Type = importItemType,
                DateAdded = DateTimeOffset.UtcNow.Date
            };

            if (imdbTitle != null && string.IsNullOrEmpty(imdbTitle.ErrorMessage))
            {
                metadata.Title = imdbTitle.Title;
                metadata.FullTitle = imdbTitle.FullTitle;
                metadata.ExternalIds.Imdb = imdbTitle.Id;
                if (imdbTitle?.Title != null)
                {
                    metadata.Slug = CreateSlug(imdbTitle.Title, year);
                }
            }
            else if (movie != null)
            {
                if (movie.ReleaseDate.HasValue)
                {
                    metadata.ReleaseDate = movie.ReleaseDate.Value;
                }

                metadata.Title = movie.Title;
                metadata.FullTitle = movie.OriginalTitle;

                if (movie?.Title != null)
                {
                    metadata.Slug = CreateSlug(movie.Title, year);
                }

                if (string.IsNullOrEmpty(metadata.ExternalIds.Imdb))
                {
                    metadata.ExternalIds.Imdb = movie!.ImdbId;
                    if (string.IsNullOrEmpty(metadata.ExternalIds.Imdb))
                    {
                        metadata.ExternalIds.Imdb = movie!.ExternalIds?.ImdbId;
                    }
                }
            }
            else if (series != null)
            {
                if (series.FirstAirDate.HasValue)
                {
                    metadata.ReleaseDate = series.FirstAirDate.Value;
                }

                metadata.Title = series.Name;
                metadata.SortTitle = GetSortTitle(series.Name);
                metadata.SortTitle = GetSortTitle(metadata.Title);
                metadata.FullTitle = series.OriginalName;

                if (series?.Name != null)
                {
                    metadata.Slug = this.CreateSlug(series.Name, year);
                }

                if (string.IsNullOrEmpty(metadata.ExternalIds.Imdb))
                {
                    metadata.ExternalIds.Imdb = series!.ExternalIds?.ImdbId;
                }
            }

            if (imdbTitle != null && string.IsNullOrEmpty(imdbTitle.ErrorMessage))
            {
                metadata.Plot = imdbTitle.Plot;
                metadata.Directors = imdbTitle.Directors;
                metadata.Stars = imdbTitle.Stars;
                metadata.Writers = imdbTitle.Writers;
                metadata.Genres = imdbTitle.Genres;
                metadata.Runtime = imdbTitle.RuntimeStr;
                metadata.ContentRating = imdbTitle.ContentRating;
                metadata.Tagline = imdbTitle.Tagline;
                if (metadata.ReleaseDate == default(DateTimeOffset) && !string.IsNullOrEmpty(imdbTitle.ReleaseDate))
                {
                    metadata.ReleaseDate = DateTimeOffset.Parse(imdbTitle.ReleaseDate + "T00:00:00+00:00");
                }

                if (Int32.TryParse(imdbTitle.RuntimeMins, out int minutes))
                {
                    metadata.RuntimeMinutes = minutes;
                }
            }

            if (movie != null)
            {
                metadata.ExternalIds.Tmdb = movie.Id.ToString();

                if (string.IsNullOrEmpty(metadata.Plot))
                {
                    metadata.Plot = movie.Overview;
                }

                if (string.IsNullOrEmpty(metadata.Tagline))
                {
                    metadata.Tagline = movie.Tagline;
                }
            }
            else if (series != null)
            {
                metadata.ExternalIds.Tmdb = series.Id.ToString();

                if (string.IsNullOrEmpty(metadata.Plot))
                {
                    metadata.Plot = series.Overview;
                }
            }

            if (metadata.Title != null && metadata.Title.StartsWith("the", StringComparison.OrdinalIgnoreCase))
            {
                metadata.SortTitle = metadata.Title.Substring(4, metadata.Title.Length - 4).Trim() + ", The";
            }
            else
            {
                metadata.SortTitle = metadata.Title;
            }

            return metadata;
        }
        //*/
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
