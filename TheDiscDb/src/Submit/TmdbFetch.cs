using OGIb = global::ImportBuddy;

namespace DiscRipper.TheDiscDb.Submit;

public class TmdbFetch : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Submission.TMDB))
        {
            throw new ArgumentException("Submission must have TMDB set.");
        }

        if (string.IsNullOrWhiteSpace(context.Submission.MediaType))
        {
            throw new ArgumentException("Submission must have MediaType set.");
        }

        OGIb.RecentItemImportTask recentItemImportTask = new(context.InitializationData.FileSystem, context.InitializationData.WrappedOptions);
        OGIb.TmdbByIdImportTask tmdbTask = new(context.InitializationData.TmdbClient);

        OGIb.IImportTask[] tasks = [recentItemImportTask, tmdbTask];

        foreach (var task in tasks)
        {
            if (task.CanHandle(context.Submission.TMDB, context.Submission.MediaType))
            {
                var importedItem = await task.GetImportItem(context.Submission.TMDB, context.Submission.MediaType);
                if (importedItem != null)
                {
                    context.ImportItem = importedItem;
                    break;
                }
            }
        }
    }
}
