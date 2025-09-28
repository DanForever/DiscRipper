namespace DiscRipper.TheDiscDb.ImportBuddy;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OGIB = global::ImportBuddy;

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

public class InitializationData
{
    #region Import buddy options

    readonly OGIB.ImportBuddyOptions _importBuddyOptions = new();
    readonly OptionsWrapper<OGIB.ImportBuddyOptions> _wrappedInputBuddyOptions;

    #endregion // Import buddy options

    #region File system

    Fantastic.TheMovieDb.Caching.FileSystem.FileSystemCacheOptions _filesystemCacheOptions = new();
    Fantastic.FileSystem.IFileSystem _fileSystem = new Fantastic.FileSystem.PhysicalFileSystem();

    readonly string _fileSystemCacheName = "DiscRipper";
    readonly Fantastic.TheMovieDb.Caching.FileSystem.FileSystemCache fileSystemCache;

    #endregion // File system

    #region The movie database

    HttpClient httpClient = new();
    ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.SetMinimumLevel(LogLevel.Debug).AddDebug();
    });

    Fantastic.TheMovieDb.TheMovieDbOptions theMovieDbOptions = new()
    {
        ApiKey = DiscRipper.ImportBuddy.TMDB.ApiKey
    };

    Fantastic.TheMovieDb.TheMovieDbClient tmdbClient;
    IOptionsMonitor<Fantastic.TheMovieDb.TheMovieDbOptions> _theMovieDbOptionsMonitor;

    #endregion The movie database


    public Fantastic.FileSystem.IFileSystem FileSystem => _fileSystem;
    public OptionsWrapper<OGIB.ImportBuddyOptions> WrappedOptions => _wrappedInputBuddyOptions;

    public Fantastic.TheMovieDb.TheMovieDbClient TmdbClient => tmdbClient;

    public InitializationData(string dataRepositoryPath)
    {
        _importBuddyOptions.DataRepositoryPath = dataRepositoryPath;

        _wrappedInputBuddyOptions = new(_importBuddyOptions);

        _filesystemCacheOptions.BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiscRipper", "FilesystemCache");
        fileSystemCache = new(_fileSystemCacheName, _filesystemCacheOptions, FileSystem);
        _theMovieDbOptionsMonitor = new StaticOptionsMonitor<Fantastic.TheMovieDb.TheMovieDbOptions>(theMovieDbOptions);
        tmdbClient = new(httpClient, _theMovieDbOptionsMonitor, loggerFactory, fileSystemCache);


    }
}
