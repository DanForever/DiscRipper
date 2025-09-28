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
    OGIB.ImportBuddyOptions ibOptions = new();
    OptionsWrapper<OGIB.ImportBuddyOptions> wrappedOptions;

    Fantastic.TheMovieDb.Caching.FileSystem.FileSystemCacheOptions filesystemCachingOptions = new();
    Fantastic.FileSystem.IFileSystem _fileSystem = new Fantastic.FileSystem.PhysicalFileSystem();

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



    string _fileSystemCacheName = "DiscRipper";
    Fantastic.TheMovieDb.Caching.FileSystem.FileSystemCache fileSystemCache;

    public Fantastic.FileSystem.IFileSystem FileSystem => _fileSystem;
    public OptionsWrapper<OGIB.ImportBuddyOptions> WrappedOptions => wrappedOptions;

    public Fantastic.TheMovieDb.TheMovieDbClient TmdbClient => tmdbClient;

    public InitializationData(string dataRepositoryPath)
    {
        ibOptions.DataRepositoryPath = dataRepositoryPath;

        wrappedOptions = new(ibOptions);

        filesystemCachingOptions.BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiscRipper", "FilesystemCache");
        fileSystemCache = new(_fileSystemCacheName, filesystemCachingOptions, FileSystem);
        _theMovieDbOptionsMonitor = new StaticOptionsMonitor<Fantastic.TheMovieDb.TheMovieDbOptions>(theMovieDbOptions);
        tmdbClient = new(httpClient, _theMovieDbOptionsMonitor, loggerFactory, fileSystemCache);


    }
}
