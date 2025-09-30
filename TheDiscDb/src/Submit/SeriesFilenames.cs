namespace DiscRipper.TheDiscDb.Submit;

using global::ImportBuddy;

public class SeriesFilenames : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if(context.ImportItem == null)
        {
            throw new ArgumentException("ImportItem must be set in context before building series filenames.");
        }

        if (string.IsNullOrWhiteSpace(context.Submission.MediaType))
        {
            throw new ArgumentException("MediaType must be set in context.Submission before building series filenames.");
        }

        if(string.IsNullOrWhiteSpace(context.BasePath))
        {
            throw new ArgumentException("BasePath must be set in context before building series filenames.");
        }

        if (context.Submission.MediaType.Equals("Series", StringComparison.CurrentCultureIgnoreCase))
        {
            await GetSeriesFilenamesTask_RunInternal(context.InitializationData.FileSystem, context.InitializationData.TmdbClient, context.ImportItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Series, context.BasePath);
        }
    }

    private static async Task GetSeriesFilenamesTask_RunInternal(Fantastic.FileSystem.IFileSystem fileSystem, Fantastic.TheMovieDb.TheMovieDbClient tmdb, Fantastic.TheMovieDb.Models.Series? series, string basePath)
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
}
