using DiscRipper.TheDiscDb.ImportBuddy;

using OGIB = global::ImportBuddy;

namespace DiscRipper.TheDiscDb.Submit;

public class TMDB_Fetch
{
    public OGIB.ImportItem? ImportItem { get; private set; } = null;

    public async Task Fetch(Submission submission, InitializationData initData)
    {
        if(string.IsNullOrWhiteSpace(submission.TMDB))
        {
            throw new ArgumentException("Submission must have TMDB set.");
        }

        if (string.IsNullOrWhiteSpace(submission.MediaType))
        {
            throw new ArgumentException("Submission must have MediaType set.");
        }

        OGIB.RecentItemImportTask recentItemImportTask = new(initData.FileSystem, initData.WrappedOptions);
        OGIB.TmdbByIdImportTask tmdbTask = new(initData.TmdbClient);

        OGIB.IImportTask[] tasks = [recentItemImportTask, tmdbTask];

        foreach (var task in tasks)
        {
            if (task.CanHandle(submission.TMDB, submission.MediaType))
            {
                var importedItem = await task.GetImportItem(submission.TMDB, submission.MediaType);
                if (importedItem != null)
                {
                    ImportItem = importedItem;
                    break;
                }
            }
        }
    }

}
