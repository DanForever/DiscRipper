using ImportBuddy;

namespace DiscRipper.TheDiscDb.Submit;

public class DownloadPoster : IStep
{
    public async Task Run(SubmissionContext context)
    {
        if (context.ImportItem == null)
        {
            throw new ArgumentException("ImportItem must be set in context");
        }

        if (string.IsNullOrEmpty(context.BasePath))
        {
            throw new ArgumentException("BasePath must be set in context");
        }

        string? posterUrl = context.ImportItem.GetPosterUrl();
        string posterPath = context.InitializationData.FileSystem.Path.Combine(context.BasePath, "cover.jpg");
        if (!await context.InitializationData.FileSystem.File.Exists(posterPath) && !string.IsNullOrEmpty(posterUrl))
        {
            await context.InitializationData.HttpClient.Download(context.InitializationData.FileSystem, posterUrl, posterPath);
        }
    }
}
