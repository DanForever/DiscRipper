using OGIb = global::ImportBuddy;

namespace DiscRipper.TheDiscDb.Submit;

public class TmdbFetch : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Release?.TMDB))
        {
            throw new ArgumentException("Submission must have TMDB set.");
        }

        if (string.IsNullOrWhiteSpace(context.Release?.MediaType))
        {
            throw new ArgumentException("Submission must have MediaType set.");
        }

        OGIb.RecentItemImportTask recentItemImportTask = new(context.InitializationData.FileSystem, context.InitializationData.WrappedOptions);
        OGIb.TmdbByIdImportTask tmdbTask = new(context.InitializationData.TmdbClient);

        OGIb.IImportTask[] tasks = [recentItemImportTask, tmdbTask];

        foreach (var task in tasks)
        {
            if (task.CanHandle(context.Release.TMDB, context.Release.MediaType))
            {
                var importedItem = await task.GetImportItem(context.Release.TMDB, context.Release.MediaType);
                if (importedItem != null)
                {
                    context.ImportItem = importedItem;
                    break;
                }
            }
        }
    }
}
